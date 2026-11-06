using System.Data.OleDb;
using System.Text;

namespace DocumentTools.ExamFixture;

/// <summary>
/// ACE 通道诊断：把"建库（ADOX）"与"打开（OLE DB）"两步分开报告，
/// 因为两者可能在不同位宽/不同注册状态下成功或失败。仅用于人工排障与测试跳过的原因说明。
/// </summary>
public static class AceDiagnostics
{
    public static string Describe()
    {
        var text = new StringBuilder();
        text.AppendLine("进程位宽: " + (Environment.Is64BitProcess ? "64 位" : "32 位"));

        Type? catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
        text.AppendLine("ADOX.Catalog: " + (catalogType is null ? "未注册" : "已注册"));

        string root = Path.Combine(Path.GetTempPath(), "dtexam-diag-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        try
        {
            string databaseFile = Path.Combine(root, ExamFixtureBuilder.BaseDatabaseName);
            try
            {
                ExamFixtureBuilder.TryCreateEmptyDatabase(databaseFile);
                text.AppendLine("夹具建库: 成功（" + new FileInfo(databaseFile).Length + " 字节）");
            }
            catch (Exception ex)
            {
                text.AppendLine("夹具建库: 失败 → " + Chain(ex));
                return text.ToString();
            }

            foreach (string provider in ExamFixtureBuilder.ProviderCandidates)
            {
                text.AppendLine($"OLE DB {provider}: {TryOpenOnce(databaseFile, provider)}");
            }
        }
        finally
        {
            try
            {
                Directory.Delete(root, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // 诊断目录清理失败不影响结论
            }
        }

        return text.ToString();
    }

    /// <summary>只测"打开已有库"，不做任何建库动作，用于把 ADOX 问题与 OLE DB 问题分开。</summary>
    public static string VerifyExisting(string databaseFile)
    {
        if (!File.Exists(databaseFile))
        {
            return "数据库不存在：" + databaseFile;
        }

        var text = new StringBuilder();
        text.AppendLine("文件: " + databaseFile + "（" + new FileInfo(databaseFile).Length + " 字节）");
        text.AppendLine("进程位宽: " + (Environment.Is64BitProcess ? "64 位" : "32 位"));
        foreach (string provider in ExamFixtureBuilder.ProviderCandidates)
        {
            text.AppendLine($"OLE DB {provider}: {TryOpenOnce(databaseFile, provider)}");
        }

        return text.ToString();
    }

    private static string TryOpenOnce(string databaseFile, string provider)
    {
        try
        {
            using var connection =
                new OleDbConnection($"Provider={provider};Data Source={databaseFile};Persist Security Info=False;");
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM MSysObjects WHERE 1=0";
            command.ExecuteScalar();
            return "成功";
        }
        catch (Exception ex)
        {
            return "失败 → " + Chain(ex);
        }
    }

    /// <summary>把异常链（含 COM HRESULT）串成一行，避免只看到最外层"没有注册类"。</summary>
    public static string Chain(Exception ex)
    {
        var text = new StringBuilder();
        for (Exception? current = ex; current is not null; current = current.InnerException)
        {
            if (text.Length > 0)
            {
                text.Append(" <- ");
            }

            text.Append(current.GetType().Name).Append(": ").Append(current.Message);
            if (current is System.Runtime.InteropServices.COMException com)
            {
                text.Append(" [HRESULT 0x").Append(com.HResult.ToString("X8")).Append(']');
            }
        }

        return text.ToString();
    }
}
