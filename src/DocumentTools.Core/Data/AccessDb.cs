using System.Data;
using System.Data.OleDb;

namespace DocumentTools.Core.Data;

/// <summary>ACE/Jet Access 简易会话（对齐旧 AccessSession 的打开/关闭边界）。</summary>
public sealed class AccessDb : IDisposable
{
    /// <summary>
    /// 仅用于自动化测试：把该环境变量设为 1 或 true，本类型打开任何库都会按"本机缺少 ACE"失败。
    /// 存在的意义是让"缺 provider 时不报红、只跳过"这条路径能在装了 ACE 的机器上被回归，
    /// 否则该分支永远没人执行过。
    /// </summary>
    public const string SimulateMissingAceVariable = "DOCUMENTTOOLS_SIMULATE_NO_ACE";

    private readonly OleDbConnection _connection;
    private OleDbTransaction? _transaction;
    private bool _disposed;

    public AccessDb(string databaseFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("找不到数据库文件：" + databaseFile, databaseFile);
        }

        _connection = Open(databaseFile);
    }

    public void BeginTransaction()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("事务已经开启");
        }

        _transaction = _connection.BeginTransaction();
    }

    public void Commit()
    {
        if (_transaction == null)
        {
            return;
        }

        _transaction.Commit();
        _transaction.Dispose();
        _transaction = null;
    }

    public void Rollback()
    {
        if (_transaction == null)
        {
            return;
        }

        try
        {
            _transaction.Rollback();
        }
        catch
        {
            // ignore
        }

        _transaction.Dispose();
        _transaction = null;
    }

    public int Execute(string sql, params (string Name, object? Value)[] args)
    {
        using OleDbCommand cmd = CreateCommand(sql, args);
        return cmd.ExecuteNonQuery();
    }

    public object? Scalar(string sql, params (string Name, object? Value)[] args)
    {
        using OleDbCommand cmd = CreateCommand(sql, args);
        object? value = cmd.ExecuteScalar();
        return value is DBNull ? null : value;
    }

    public DataTable Query(string sql, params (string Name, object? Value)[] args)
    {
        using OleDbCommand cmd = CreateCommand(sql, args);
        using var adapter = new OleDbDataAdapter(cmd);
        var table = new DataTable();
        adapter.Fill(table);
        return table;
    }

    /// <summary>
    /// 读用户表清单（跳过 MSys* 系统表）。考评导入/改号这类"按表批量执行"的场景需要先知道库里到底有哪几张表：
    /// 老版本数据包可能缺表，硬写表名会让整条流程炸在一个与用户目标无关的表上。
    /// </summary>
    public IReadOnlyList<string> ListTables()
    {
        var names = new List<string>();
        DataTable? schema = _connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
        if (schema is null)
        {
            return names;
        }

        foreach (DataRow row in schema.Rows)
        {
            string? name = row["TABLE_NAME"] is DBNull ? null : Convert.ToString(row["TABLE_NAME"]);
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            // GetOleDbSchemaTable 同时会返回视图/系统表，只认普通表。
            string? tableType = row.Table.Columns.Contains("TABLE_TYPE") && row["TABLE_TYPE"] is not DBNull
                ? Convert.ToString(row["TABLE_TYPE"])
                : null;
            if (!string.Equals(tableType, "TABLE", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (name.StartsWith("MSys", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            names.Add(name);
        }

        return names;
    }

    public OleDbCommand CreateCommand(string sql, params (string Name, object? Value)[] args)
    {
        var cmd = _connection.CreateCommand();
        cmd.Transaction = _transaction;
        cmd.CommandText = sql;
        if (args != null)
        {
            foreach ((string name, object? value) in args)
            {
                cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
            }
        }

        return cmd;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            Rollback();
        }
        catch
        {
            // ignore
        }

        _connection.Dispose();
    }

    /// <summary>是否处于"模拟缺少 ACE"的自动化测试环境（见 <see cref="SimulateMissingAceVariable"/>）。</summary>
    public static bool IsMissingAceSimulated()
    {
        string? value = Environment.GetEnvironmentVariable(SimulateMissingAceVariable);
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        value = value.Trim();
        return value == "1" || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>缺少 ACE 时的统一提示（真实缺 provider 与模拟缺 provider 共用同一句，避免两套文案）。</summary>
    public static string MissingAceMessage()
    {
        return "无法打开 Access 数据库。未检测到可用的 Microsoft Access Database Engine (ACE) OLE DB 提供者"
               + "（当前进程 " + (Environment.Is64BitProcess ? "64" : "32") + " 位：64 位进程需要 64 位 ACE，"
               + "32 位进程需要 32 位 ACE）。请安装 Microsoft Access Database Engine。";
    }

    private static OleDbConnection Open(string databaseFile)
    {
        // 模拟分支放在最前：装了 ACE 的机器也要能跑到"缺 provider"的调用方处理逻辑。
        if (IsMissingAceSimulated())
        {
            throw new InvalidOperationException(MissingAceMessage());
        }

        var errors = new List<Exception>();
        foreach (string provider in new[]
                 {
                     "Microsoft.ACE.OLEDB.12.0",
                     "Microsoft.ACE.OLEDB.16.0",
                     "Microsoft.Jet.OLEDB.4.0"
                 })
        {
            try
            {
                var connection = new OleDbConnection(
                    $"Provider={provider};Data Source={databaseFile};Persist Security Info=False;");
                connection.Open();
                return connection;
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }

        throw BuildOpenFailure(databaseFile, errors);
    }

    private static InvalidOperationException BuildOpenFailure(string databaseFile, List<Exception> errors)
    {
        // 保留最后一层原始异常（OleDb 的 provider 错误链最深一层最接近真因）。
        Exception? inner = errors.Count > 0 ? errors[^1] : null;
        if (errors.Count == 0)
        {
            return new InvalidOperationException(MissingAceMessage());
        }

        List<Exception> innermost = errors.Select(Innermost).ToList();

        // 只有"每个 provider 都报未注册"才是真的没装 ACE；否则说明 provider 在，问题出在文件上。
        // 注意排除"未注册"噪音后再归因：同一份坏文件可能让 provider 各自报不同的错，
        // 其中 Jet 只会说"未注册"，若照搬最后一条就会把损坏文件误报成缺 ACE（QA 实测过）。
        List<Exception> informative = innermost.Where(e => !IsNotRegistered(e)).ToList();
        if (informative.Count == 0)
        {
            return new InvalidOperationException(MissingAceMessage(), inner);
        }

        Exception? damaged = informative.FirstOrDefault(IsFileProblem) ?? innermost.FirstOrDefault(IsFileProblem);
        if (damaged is not null)
        {
            return new InvalidOperationException(
                "数据库文件可能损坏、被占用或不是 Access 2003/2007+ 格式：" + Path.GetFileName(databaseFile)
                + "（底层错误：" + FirstLine(damaged.Message) + "）",
                informative[^1]);
        }

        return new InvalidOperationException(
            "无法打开数据库 " + databaseFile + "：" + FirstLine(informative[^1].Message),
            informative[^1]);
    }

    private static bool IsNotRegistered(Exception ex)
    {
        string message = ex.Message;
        return message.Contains("provider is not registered", StringComparison.OrdinalIgnoreCase)
               || message.Contains("未在本地计算机上注册", StringComparison.Ordinal);
    }

    private static Exception Innermost(Exception ex)
    {
        Exception current = ex;
        while (current.InnerException is not null)
        {
            current = current.InnerException;
        }

        return current;
    }

    /// <summary>
    /// 判断底层错误是否指向"文件本身有问题"（损坏 / 不是 Access 格式 / 被占用 / 密码错 / 路径失效）。
    /// 这里刻意不按 provider 名分流：同一份损坏文件在不同 provider 下会报不同错误，
    /// 例如 ACE 报"不可识别的数据库格式"而 Jet 只报"未注册"。
    /// </summary>
    private static bool IsFileProblem(Exception ex)
    {
        string message = ex.Message;
        foreach (string token in new[]
                 {
                     "not a valid password",
                     "unrecognized database format",
                     "not a valid file name",
                     "could not find file",
                     "cannot open",
                     "is not a valid path",
                     "不是有效的密码",
                     "不可识别的数据库格式",
                     "无法识别的数据库格式",
                     "无法识别的文件格式",
                     "找不到文件",
                     "文件名无效"
                 })
        {
            if (message.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string FirstLine(string message)
    {
        int index = message.IndexOfAny(['\r', '\n']);
        return index < 0 ? message : message[..index];
    }
}
