using DocumentTools.Core.Data;

namespace DocumentTools.Core.Services.Exam;

public sealed class ExamPackageStatus
{
    public bool DatabaseExists { get; set; }
    public int PeopleCount { get; set; }
    public int UnitCount { get; set; }
    public int ExamFileCount { get; set; }
    public bool HasSelfReport { get; set; }

    public bool IsAlreadyInitialized =>
        DatabaseExists && PeopleCount == 0 && UnitCount == 0 && ExamFileCount == 0 && !HasSelfReport;

    public string Summary =>
        !DatabaseExists
            ? "未找到 Exam_Base.mdb"
            : $"人员 {PeopleCount}，单位 {UnitCount}，.exam {ExamFileCount}，SelfReport={(HasSelfReport ? "有" : "无")}";
}

public sealed class ExamPackageMergeRequest
{
    public string SourceDirectory { get; set; } = string.Empty;

    public string TargetDirectory { get; set; } = string.Empty;

    public bool CopyExamFiles { get; set; } = true;

    public bool RemoveDuplicateUnitsInTarget { get; set; } = true;
}

public sealed class ExamPackageMergeResult
{
    public bool HadDuplicateUnits { get; set; }

    public int DuplicateUnitsRemoved { get; set; }

    public int PeopleInserted { get; set; }

    public int UnitsInserted { get; set; }

    public int ExamFilesCopied { get; set; }

    public string Summary { get; set; } = string.Empty;
}

public interface IExamPackageService
{
    ExamPackageStatus Inspect(string packageDirectory);

    /// <summary>
    /// 该目录是否为考评数据包。判据就是「目录下有 <c>Exam_Base.mdb</c>」——
    /// 数据包布局知识（文件名、扁平存放、SelfReport 子目录）只在 Core 出现，
    /// 视图不得再 <c>Path.Combine(dir, "Exam_Base.mdb")</c>。
    /// </summary>
    bool IsPackageDirectory(string directory);

    /// <summary>状态行用的一句话：目录未选择 / 目录不存在 / 不是数据包（缺 Exam_Base.mdb） / 内容摘要。</summary>
    string Describe(string directory);

    int Reset(string packageDirectory);

    IReadOnlyList<(int Order, string Unit, string Code, string Kind)> ListUnits(string packageDirectory);

    /// <summary>源库 Unit_Ess.UnitCode 与目标库是否有交集（大小写不敏感，空值忽略）。</summary>
    bool HasDuplicateUnits(string sourceDirectory, string targetDirectory);

    /// <summary>把一个数据包的人员/单位合并进另一个数据包，可选复制 .exam 与清理目标库重复单位。</summary>
    ExamPackageMergeResult Merge(ExamPackageMergeRequest request);

    /// <summary>
    /// 按给定单位编码顺序重写 Unit_Ess.AtOrder（legacy「数据包单位顺序修改」，
    /// BodyForm.cs:10526-10657：逐行 <c>UPDATE Unit_Ess SET AtOrder=? WHERE UnitCode=?</c>）。
    /// 顺序里第 1 个单位的 AtOrder=1，依次递增；不在顺序里的单位保持原值不动。返回更新的行数。
    /// </summary>
    int ReorderUnits(string packageDirectory, IReadOnlyList<string> orderedUnitCodes);
}

public sealed class ExamPackageService : IExamPackageService
{
    private const string BaseDatabaseName = "Exam_Base.mdb";
    private const string SelfReportFolderName = "SelfReport";

    public ExamPackageStatus Inspect(string packageDirectory)
    {
        var status = new ExamPackageStatus();
        string? root = NormalizeRoot(packageDirectory);
        if (root is null)
        {
            return status;
        }

        string databaseFile = Path.Combine(root, BaseDatabaseName);
        status.DatabaseExists = File.Exists(databaseFile);
        if (status.DatabaseExists)
        {
            try
            {
                using var session = new AccessDb(databaseFile);
                status.PeopleCount = ToCount(session.Scalar("SELECT COUNT(*) FROM Exam_People"));
                status.UnitCount = ToCount(session.Scalar("SELECT COUNT(*) FROM Unit_Ess"));
            }
            catch
            {
                status.PeopleCount = -1;
                status.UnitCount = -1;
            }
        }

        try
        {
            status.ExamFileCount = Directory.Exists(root)
                ? Directory.GetFiles(root, "*.exam", SearchOption.TopDirectoryOnly).Length
                : 0;
        }
        catch
        {
            status.ExamFileCount = 0;
        }

        try
        {
            status.HasSelfReport = Directory.Exists(Path.Combine(root, SelfReportFolderName));
        }
        catch
        {
            status.HasSelfReport = false;
        }

        return status;
    }

    public bool IsPackageDirectory(string directory)
        => NormalizeRoot(directory) is { } root && File.Exists(Path.Combine(root, BaseDatabaseName));

    public string Describe(string directory)
    {
        string? root = NormalizeRoot(directory);
        if (root is null)
        {
            return "数据包目录未选择";
        }

        if (!Directory.Exists(root))
        {
            return "数据包目录不存在：" + root;
        }

        return File.Exists(Path.Combine(root, BaseDatabaseName))
            ? Inspect(root).Summary
            : $"不是考评数据包（缺少 {BaseDatabaseName}）";
    }

    public int Reset(string packageDirectory)
    {
        string root = NormalizeRoot(packageDirectory)
                      ?? throw new ArgumentException("数据包目录为空。");
        string databaseFile = Path.Combine(root, BaseDatabaseName);
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException("找不到 Exam_Base.mdb。", databaseFile);
        }

        int rows;
        using (var session = new AccessDb(databaseFile))
        {
            rows = session.Execute("DELETE FROM Exam_People");
            rows += session.Execute("DELETE FROM Unit_Ess");
        }

        foreach (string file in Directory.GetFiles(root, "*.exam", SearchOption.TopDirectoryOnly))
        {
            try
            {
                File.Delete(file);
            }
            catch
            {
                // ignore locked
            }
        }

        string selfReport = Path.Combine(root, SelfReportFolderName);
        if (Directory.Exists(selfReport))
        {
            try
            {
                Directory.Delete(selfReport, recursive: true);
            }
            catch
            {
                // ignore
            }
        }

        return rows;
    }

    public IReadOnlyList<(int Order, string Unit, string Code, string Kind)> ListUnits(string packageDirectory)
    {
        string root = NormalizeRoot(packageDirectory)
                      ?? throw new ArgumentException("数据包目录为空。");
        string databaseFile = Path.Combine(root, BaseDatabaseName);
        using var session = new AccessDb(databaseFile);
        var table = session.Query(
            "SELECT AtOrder, Unit, UnitCode, AtKind FROM Unit_Ess ORDER BY AtOrder ASC");
        var list = new List<(int, string, string, string)>();
        foreach (global::System.Data.DataRow row in table.Rows)
        {
            list.Add((
                Convert.ToInt32(row["AtOrder"]),
                Convert.ToString(row["Unit"]) ?? string.Empty,
                Convert.ToString(row["UnitCode"]) ?? string.Empty,
                Convert.ToString(row["AtKind"]) ?? string.Empty));
        }

        return list;
    }

    /// <summary>
    /// 按给定顺序重写 Unit_Ess.AtOrder（legacy 数据包单位顺序修改，BodyForm.cs:10526-10657）。
    /// legacy 是逐行 UPDATE、无事务；这里包一个事务，失败时整批回滚，避免序号出现半成品。
    /// </summary>
    public int ReorderUnits(string packageDirectory, IReadOnlyList<string> orderedUnitCodes)
    {
        ArgumentNullException.ThrowIfNull(orderedUnitCodes);

        string root = NormalizeRoot(packageDirectory) ?? throw new ArgumentException("数据包目录为空。");
        string databaseFile = ResolveBaseDatabase(root, string.Empty);

        var codes = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string? code in orderedUnitCodes)
        {
            string trimmed = code?.Trim() ?? string.Empty;
            if (trimmed.Length == 0)
            {
                continue;
            }

            if (!seen.Add(trimmed))
            {
                throw new ArgumentException($"单位编码重复：{trimmed}。", nameof(orderedUnitCodes));
            }

            codes.Add(trimmed);
        }

        if (codes.Count == 0)
        {
            return 0;
        }

        using var session = new AccessDb(databaseFile);
        var existing = new HashSet<string>(
            CollectUnitCodes(session.Query("SELECT UnitCode FROM Unit_Ess")),
            StringComparer.OrdinalIgnoreCase);
        List<string> unknown = codes.Where(code => !existing.Contains(code)).ToList();
        if (unknown.Count > 0)
        {
            throw new ArgumentException($"数据包里没有这些单位编码：{string.Join("、", unknown)}。", nameof(orderedUnitCodes));
        }

        session.BeginTransaction();
        try
        {
            int updated = 0;
            for (int i = 0; i < codes.Count; i++)
            {
                updated += session.Execute(
                    "UPDATE Unit_Ess SET AtOrder=? WHERE UnitCode=?",
                    ("@p0", i + 1),
                    ("@p1", codes[i]));
            }

            session.Commit();
            return updated;
        }
        catch
        {
            session.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 源库与目标库的 Unit_Ess.UnitCode 交集判定（移植手写 EXAMMerge 的 HasDuplicateUnitOrPeople 单位部分）。
    /// </summary>
    public bool HasDuplicateUnits(string sourceDirectory, string targetDirectory)
    {
        string sourceRoot = NormalizeRoot(sourceDirectory) ?? throw new ArgumentException("源数据包目录为空。");
        string targetRoot = NormalizeRoot(targetDirectory) ?? throw new ArgumentException("目标数据包目录为空。");

        string sourceDatabase = ResolveBaseDatabase(sourceRoot, "源");
        string targetDatabase = ResolveBaseDatabase(targetRoot, "目标");

        List<string> sourceCodes;
        using (var source = new AccessDb(sourceDatabase))
        {
            sourceCodes = CollectUnitCodes(source.Query("SELECT UnitCode FROM Unit_Ess"));
        }

        if (sourceCodes.Count == 0)
        {
            return false;
        }

        using var target = new AccessDb(targetDatabase);
        return FindExistingUnitCodes(target, sourceCodes).Count > 0;
    }

    /// <summary>
    /// 数据包合并：可选清理目标库重复单位 → 迁移人员 → 顺延插入单位 → 复制 .exam。
    /// 数据库写操作全部落在目标库同一事务内，异常回滚后重抛；.exam 复制在提交之后进行
    /// （文件系统无事务，放在提交前会让复制失败拖累已成功的写入）。
    /// </summary>
    public ExamPackageMergeResult Merge(ExamPackageMergeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        string sourceRoot = NormalizeRoot(request.SourceDirectory) ?? throw new ArgumentException("源数据包目录为空。");
        string targetRoot = NormalizeRoot(request.TargetDirectory) ?? throw new ArgumentException("目标数据包目录为空。");

        string sourceDatabase = ResolveBaseDatabase(sourceRoot, "源");
        string targetDatabase = ResolveBaseDatabase(targetRoot, "目标");

        if (string.Equals(
                Path.GetFullPath(sourceRoot),
                Path.GetFullPath(targetRoot),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("源数据包目录与目标数据包目录相同，无需合并。");
        }

        // 先把源数据整体读成快照：合并期间不再持有源库连接，也避免与目标库事务交错。
        global::System.Data.DataTable sourcePeople;
        global::System.Data.DataTable sourceUnits;
        List<string> sourceUnitCodes;
        using (var source = new AccessDb(sourceDatabase))
        {
            sourcePeople = source.Query("SELECT * FROM Exam_People");
            sourceUnits = source.Query("SELECT * FROM Unit_Ess ORDER BY AtOrder ASC");
            sourceUnitCodes = CollectUnitCodes(sourceUnits);
        }

        var result = new ExamPackageMergeResult();
        using (var target = new AccessDb(targetDatabase))
        {
            // 结构探测（含 ADOX 自增列）放在事务之前：ADOX 会另开一条连接，
            // 早读早释放，避免它和写事务长期并存。
            Dictionary<string, HashSet<string>> autoIncrementColumns =
                ReadAutoIncrementColumns(target, ["Exam_People", "Unit_Ess"]);
            TableProjection peopleProjection = ReadTableProjection(target, "Exam_People", autoIncrementColumns);
            TableProjection unitProjection = ReadTableProjection(target, "Unit_Ess", autoIncrementColumns);

            target.BeginTransaction();
            try
            {
                List<string> duplicateCodes = FindExistingUnitCodes(target, sourceUnitCodes);
                result.HadDuplicateUnits = duplicateCodes.Count > 0;

                if (result.HadDuplicateUnits && request.RemoveDuplicateUnitsInTarget)
                {
                    result.DuplicateUnitsRemoved = RemoveDuplicateUnits(target, duplicateCodes, peopleProjection);
                }

                result.PeopleInserted = InsertPeople(target, sourcePeople, peopleProjection);
                result.UnitsInserted = InsertUnits(target, sourceUnits, unitProjection);

                target.Commit();
            }
            catch (Exception ex)
            {
                target.Rollback();

                // 底层 ACE 的报错只讲现象（"操作必须使用一个可更新的查询。"），不说是哪一端、有没有回滚、
                // 源数据包动没动。合并是不可逆的高风险动作，这里补成可照做的中文消息，原始异常挂 InnerException。
                throw new InvalidOperationException(
                    $"合并失败：写入目标数据包 {targetRoot} 的 {BaseDatabaseName} 时出错（事务已回滚，"
                    + $"目标库保持合并前状态，源数据包未改动）。底层错误：{ex.GetBaseException().Message}",
                    ex);
            }
        }

        int renamedExamFiles = 0;
        if (request.CopyExamFiles)
        {
            (int copied, int renamed) = CopyExamPackages(sourceRoot, targetRoot);
            result.ExamFilesCopied = copied;
            renamedExamFiles = renamed;
        }

        result.Summary = BuildSummary(result, renamedExamFiles);
        return result;
    }

    private static string? NormalizeRoot(string packageDirectory)
        => string.IsNullOrWhiteSpace(packageDirectory) ? null : packageDirectory.Trim();

    private static int ToCount(object? value)
    {
        if (value is null or DBNull)
        {
            return 0;
        }

        return int.TryParse(Convert.ToString(value), out int count) ? count : 0;
    }

    private static string ResolveBaseDatabase(string root, string label)
    {
        string databaseFile = Path.Combine(root, BaseDatabaseName);
        if (!File.Exists(databaseFile))
        {
            throw new FileNotFoundException($"{label}数据包目录下未找到 {BaseDatabaseName}。", databaseFile);
        }

        return databaseFile;
    }

    /// <summary>取出表中非空 UnitCode（大小写去重，保持出现顺序）。</summary>
    private static List<string> CollectUnitCodes(global::System.Data.DataTable table)
    {
        var codes = new List<string>();
        if (!table.Columns.Contains("UnitCode"))
        {
            return codes;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (global::System.Data.DataRow row in table.Rows)
        {
            string code = Convert.ToString(row["UnitCode"]) ?? string.Empty;
            if (code.Length > 0 && seen.Add(code))
            {
                codes.Add(code);
            }
        }

        return codes;
    }

    /// <summary>返回目标库 Unit_Ess 中真实存在的那些 UnitCode。</summary>
    private static List<string> FindExistingUnitCodes(AccessDb target, IReadOnlyList<string> codes)
    {
        var existing = new List<string>();
        if (codes.Count == 0)
        {
            return existing;
        }

        // OleDb 的 IN 只能用字面量，单引号按旧实现转义成两个单引号。
        string inList = BuildInList(codes);
        global::System.Data.DataTable table = target.Query(
            "SELECT UnitCode FROM Unit_Ess WHERE UnitCode IN (" + inList + ")");

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (global::System.Data.DataRow row in table.Rows)
        {
            string code = Convert.ToString(row["UnitCode"]) ?? string.Empty;
            if (code.Length > 0 && seen.Add(code))
            {
                existing.Add(code);
            }
        }

        return existing;
    }

    private static string BuildInList(IEnumerable<string> values)
        => string.Join(", ", values.Select(value => "'" + value.Replace("'", "''") + "'"));

    /// <summary>目标表结构投影：列名顺序、列名集合、自增列集合。</summary>
    private sealed class TableProjection
    {
        public TableProjection(List<string> columns, HashSet<string> autoIncrementColumns)
        {
            Columns = columns;
            ColumnLookup = new HashSet<string>(columns, StringComparer.OrdinalIgnoreCase);
            AutoIncrementColumns = autoIncrementColumns;
        }

        public List<string> Columns { get; }

        public HashSet<string> ColumnLookup { get; }

        public HashSet<string> AutoIncrementColumns { get; }
    }

    private static TableProjection ReadTableProjection(
        AccessDb target,
        string tableName,
        Dictionary<string, HashSet<string>> autoIncrementColumns)
    {
        // 列名用 SELECT * ... WHERE 1=0 的空结果集探测；DataTable 不携带自增信息。
        global::System.Data.DataTable probe = target.Query($"SELECT * FROM {tableName} WHERE 1=0");
        var columns = new List<string>();
        foreach (global::System.Data.DataColumn column in probe.Columns)
        {
            if (!string.IsNullOrWhiteSpace(column.ColumnName))
            {
                columns.Add(column.ColumnName);
            }
        }

        var autoIncrement = autoIncrementColumns.TryGetValue(tableName, out HashSet<string>? detected)
            ? new HashSet<string>(detected, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return new TableProjection(columns, autoIncrement);
    }

    /// <summary>
    /// 探测目标表的自增列（COUNTER/AutoNumber）。两条通道合并：
    /// 1) OLE DB 标准元数据 <c>GetSchemaTable().IsAutoIncrement</c>——SQL Server 等 provider 有效；
    /// 2) ADOX 目录的 <c>Column.Properties("AutoIncrement")</c>——Jet/ACE 下唯一可靠的来源。
    /// 实测 ACE 12.0 的 IsAutoIncrement 恒为 False，而 Columns 行集的 ISROWID 位在普通 LONG
    /// 主键上也会置位，所以两者都不能单独用来判断，必须把 ADOX 结果并进来。
    /// </summary>
    private static Dictionary<string, HashSet<string>> ReadAutoIncrementColumns(
        AccessDb target,
        IReadOnlyList<string> tableNames)
    {
        var map = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (string tableName in tableNames)
        {
            map[tableName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        foreach (string tableName in tableNames)
        {
            try
            {
                using var command = target.CreateCommand($"SELECT * FROM {tableName} WHERE 1=0");
                using var reader = command.ExecuteReader();
                global::System.Data.DataTable? schema = reader.GetSchemaTable();
                if (schema is null
                    || !schema.Columns.Contains("IsAutoIncrement")
                    || !schema.Columns.Contains("ColumnName"))
                {
                    continue;
                }

                foreach (global::System.Data.DataRow row in schema.Rows)
                {
                    if (row["IsAutoIncrement"] is bool isAutoIncrement && isAutoIncrement)
                    {
                        string name = Convert.ToString(row["ColumnName"]) ?? string.Empty;
                        if (name.Length > 0)
                        {
                            map[tableName].Add(name);
                        }
                    }
                }
            }
            catch
            {
                // 标准通道不可用时不阻断：ADOX 通道仍会尝试，最终 INSERT 失败也会整体回滚。
            }
        }

        string? connectionString = null;
        try
        {
            // 复用 AccessDb 已经选定的 provider 连接串，保证 ADOX 读到的是同一个库。
            using var command = target.CreateCommand(string.Empty);
            connectionString = command.Connection?.ConnectionString;
        }
        catch
        {
            // 取不到连接串时只保留标准通道的结果。
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return map;
        }

        foreach ((string table, string column) in ReadAdoxAutoIncrementColumns(connectionString!, tableNames))
        {
            if (map.TryGetValue(table, out HashSet<string>? set))
            {
                set.Add(column);
            }
        }

        return map;
    }

    /// <summary>通过 ADOX 目录读出各表的自增列；COM 不可用时返回空集合。</summary>
    private static List<(string Table, string Column)> ReadAdoxAutoIncrementColumns(
        string connectionString,
        IReadOnlyList<string> tableNames)
    {
        var found = new List<(string Table, string Column)>();
        Type? catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
        if (catalogType is null)
        {
            return found;
        }

        object? instance = null;
        try
        {
            instance = Activator.CreateInstance(catalogType);
            if (instance is null)
            {
                return found;
            }

            dynamic catalog = instance;
            catalog.ActiveConnection = connectionString;
            dynamic tables = catalog.Tables;

            foreach (string tableName in tableNames)
            {
                object? tableObject = ResolveAdoxTable(tables, tableName);
                if (tableObject is null)
                {
                    continue;
                }

                dynamic table = tableObject;
                try
                {
                    foreach (dynamic column in table.Columns)
                    {
                        string name = Convert.ToString((object)column.Name) ?? string.Empty;
                        if (name.Length > 0 && ReadAdoxAutoIncrementFlag(column))
                        {
                            found.Add((tableName, name));
                        }
                    }
                }
                catch
                {
                    // 单表元数据异常不影响其它表。
                }
            }
        }
        catch
        {
            // ADOX 未注册/位数不匹配/库不可读：退回标准通道结果。
        }
        finally
        {
            if (instance is not null && global::System.Runtime.InteropServices.Marshal.IsComObject(instance))
            {
                global::System.Runtime.InteropServices.Marshal.FinalReleaseComObject(instance);
            }
        }

        return found;
    }

    private static object? ResolveAdoxTable(dynamic tables, string tableName)
    {
        try
        {
            return tables[tableName] as object;
        }
        catch
        {
            // 名字大小写不一致时按名逐一比对（ADOX 的集合索引对大小写并不总是宽容）。
        }

        try
        {
            foreach (dynamic candidate in tables)
            {
                string name = Convert.ToString((object)candidate.Name) ?? string.Empty;
                if (string.Equals(name, tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate as object;
                }
            }
        }
        catch
        {
            // 枚举失败按“无此表”处理。
        }

        return null;
    }

    private static bool ReadAdoxAutoIncrementFlag(dynamic column)
    {
        try
        {
            dynamic properties = column.Properties;
            dynamic property = properties["AutoIncrement"];
            return property.Value is bool flag && flag;
        }
        catch
        {
            // ADOX 对非 ACE 提供者不一定暴露该属性。
            return false;
        }
    }

    private static List<string> SelectInsertColumns(
        global::System.Data.DataTable source,
        TableProjection projection)
    {
        var columns = new List<string>();
        foreach (global::System.Data.DataColumn column in source.Columns)
        {
            string name = column.ColumnName;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            // 只写目标表真实存在的列；目标是自增列时排除——旧实现把源库自增值一并写回，
            // 目标库已有同名主键时会撞键，而且自增值本应由目标库分配。
            if (projection.ColumnLookup.Contains(name) && !projection.AutoIncrementColumns.Contains(name))
            {
                columns.Add(name);
            }
        }

        return columns;
    }

    private static string BuildInsertSql(string tableName, List<string> columns)
    {
        string columnList = string.Join(", ", columns.Select(name => "[" + name + "]"));
        string parameterList = string.Join(", ", columns.Select((name, index) => "@p" + index));
        return $"INSERT INTO {tableName} ({columnList}) VALUES ({parameterList})";
    }

    private static int InsertPeople(
        AccessDb target,
        global::System.Data.DataTable source,
        TableProjection projection)
    {
        List<string> columns = SelectInsertColumns(source, projection);
        if (columns.Count == 0)
        {
            return 0;
        }

        string sql = BuildInsertSql("Exam_People", columns);
        int inserted = 0;
        foreach (global::System.Data.DataRow row in source.Rows)
        {
            var args = new (string Name, object? Value)[columns.Count];
            for (int i = 0; i < columns.Count; i++)
            {
                // DataRow 里缺失/空值为 DBNull，AccessDb 会原样作为参数值传入。
                args[i] = ("@p" + i, row[columns[i]]);
            }

            inserted += target.Execute(sql, args);
        }

        return inserted;
    }

    private static int InsertUnits(
        AccessDb target,
        global::System.Data.DataTable source,
        TableProjection projection)
    {
        // 旧实现语义：取目标库当前 MAX(AtOrder)，源行按 AtOrder 升序逐行 +1 顺延。
        int maxAtOrder = ToCount(target.Scalar("SELECT MAX(AtOrder) FROM Unit_Ess"));
        bool renumberOrder = projection.ColumnLookup.Contains("AtOrder");

        List<string> columns = SelectInsertColumns(source, projection);
        if (columns.Count == 0)
        {
            return 0;
        }

        string sql = BuildInsertSql("Unit_Ess", columns);
        int inserted = 0;
        foreach (global::System.Data.DataRow row in source.Rows)
        {
            if (renumberOrder)
            {
                maxAtOrder++;
            }

            var args = new (string Name, object? Value)[columns.Count];
            for (int i = 0; i < columns.Count; i++)
            {
                object? value = renumberOrder
                                && string.Equals(columns[i], "AtOrder", StringComparison.OrdinalIgnoreCase)
                    ? (object)maxAtOrder
                    : row[columns[i]];
                args[i] = ("@p" + i, value);
            }

            inserted += target.Execute(sql, args);
        }

        return inserted;
    }

    private static int RemoveDuplicateUnits(
        AccessDb target,
        IReadOnlyList<string> duplicateCodes,
        TableProjection peopleProjection)
    {
        if (duplicateCodes.Count == 0)
        {
            return 0;
        }

        string inList = BuildInList(duplicateCodes);

        // 只有 Exam_People 真的带 UnitCode 列时才能级联删人员（旧实现同样先探测再删）。
        if (peopleProjection.ColumnLookup.Contains("UnitCode"))
        {
            target.Execute("DELETE FROM Exam_People WHERE UnitCode IN (" + inList + ")");
        }

        return target.Execute("DELETE FROM Unit_Ess WHERE UnitCode IN (" + inList + ")");
    }

    private static (int Copied, int Renamed) CopyExamPackages(string sourceRoot, string targetRoot)
    {
        if (!Directory.Exists(sourceRoot) || !Directory.Exists(targetRoot))
        {
            return (0, 0);
        }

        int copied = 0;
        int renamed = 0;
        foreach (string file in Directory.GetFiles(sourceRoot, "*.exam", SearchOption.TopDirectoryOnly))
        {
            try
            {
                string fileName = Path.GetFileName(file);
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                string destination = Path.Combine(targetRoot, fileName);
                if (File.Exists(destination))
                {
                    // 旧实现以 overwrite=true 直接覆盖目标同名 .exam；这里改为不覆盖，
                    // 追加 _1/_2 后缀另存，避免冲掉目标包原有试卷。
                    destination = BuildUniqueExamPath(targetRoot, fileName);
                    renamed++;
                }

                File.Copy(file, destination, overwrite: false);
                copied++;
            }
            catch
            {
                // 单个文件失败不影响整体（旧实现同样静默跳过）。
            }
        }

        return (copied, renamed);
    }

    private static string BuildUniqueExamPath(string targetRoot, string fileName)
    {
        string baseName = Path.GetFileNameWithoutExtension(fileName);
        string extension = Path.GetExtension(fileName);
        for (int index = 1; index <= 999; index++)
        {
            string candidate = Path.Combine(targetRoot, baseName + "_" + index + extension);
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new IOException("目标目录中同名 .exam 过多，无法生成新文件名：" + fileName);
    }

    private static string BuildSummary(ExamPackageMergeResult result, int renamedExamFiles)
    {
        string summary = $"人员 +{result.PeopleInserted}，单位 +{result.UnitsInserted}，"
                         + $".exam +{result.ExamFilesCopied}，清理重复单位 {result.DuplicateUnitsRemoved}";

        if (result.HadDuplicateUnits && result.DuplicateUnitsRemoved == 0)
        {
            summary += "（目标库存在重复单位，本次未清理）";
        }

        if (renamedExamFiles > 0)
        {
            summary += $"；重名 .exam 以 _1/_2 后缀另存 {renamedExamFiles} 个";
        }

        return summary;
    }
}
