using DocumentTools.Core.Data;

namespace DocumentTools.ExamFixture;

/// <summary>夹具生成选项。</summary>
public sealed class ExamFixtureOptions
{
    /// <summary>是否建 Exam_BoolItem。关掉可制造"缺表"场景，用来验证 ExamInsertService 的失败回滚。</summary>
    public bool IncludeExamBoolItem { get; init; } = true;

    /// <summary>
    /// 给 Exam_People / Unit_Ess 追加 COUNTER 主键 AutoId（结构探针列）。
    /// 这两张表在源码与 legacy 里只出现在 SELECT * / UnitCode 的位置，没有可据以断言的既有列名，
    /// 因此默认不建；打开后仅用于验证 ExamPackageService 的 ADOX 自增列并集与"不写自增列"路径。
    /// </summary>
    public bool IncludeAutoIncrementProbeColumn { get; init; }

    /// <summary>是否写入样例行（选票/考评表/报表/单位/人员）。关掉则只建空表，由测试自己灌数据。</summary>
    public bool SeedSampleRows { get; init; } = true;

    /// <summary>是否创建 SelfReport 目录（ExamPackageService.Inspect / Reset 会看它）。</summary>
    public bool CreateSelfReportFolder { get; init; }

    /// <summary>
    /// 合成人员行数。0 表示沿用示例的两行；&gt;0 时按此数量灌 Exam_People（ExamID 恒为 1，
    /// UnitCode 在合成单位里循环取），用于量合并的大数据量行为。
    /// </summary>
    public int SeedPeopleCount { get; init; }

    /// <summary>
    /// 合成单位行数。0 表示沿用示例的三行；&gt;0 时按此数量灌 Unit_Ess（AtOrder 1..N）。
    /// 需与 <see cref="SeedPeopleCount"/> 一起用，否则人员会引用不到单位。
    /// </summary>
    public int SeedUnitCount { get; init; }
}

/// <summary>一套临时夹具（目录 + Exam_Base.mdb）。Dispose 时尽力删除整个临时目录。</summary>
public sealed class ExamFixtureSet : IDisposable
{
    internal ExamFixtureSet(string directoryPath)
    {
        DirectoryPath = directoryPath;
    }

    public string DirectoryPath { get; }

    public string DatabaseFile => Path.Combine(DirectoryPath, ExamFixtureBuilder.BaseDatabaseName);

    public string SelfReportDirectory => Path.Combine(DirectoryPath, ExamFixtureBuilder.SelfReportFolderName);

    /// <summary>打开夹具库（provider 由 AccessDb 自己挑 ACE/Jet）。</summary>
    public AccessDb OpenDatabase() => new(DatabaseFile);

    public void Dispose()
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, recursive: true);
                }

                return;
            }
            catch (IOException)
            {
                // ACE 可能还握着文件句柄，稍等重试
                Thread.Sleep(50);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(50);
            }
        }
    }
}

/// <summary>
/// 考评 Access 夹具生成器。
///
/// 表结构只来自两处真实证据，不臆造列：
/// 1. <c>src\DocumentTools.Core\Services\Exam\*.cs</c> 里实际执行过的 SQL；
/// 2. legacy <c>DocumentTools\BodyForm.cs</c> / <c>Util.cs</c> 对应的读写语句（行号见各 DDL 注释）。
/// 列类型取"能装下这些写入值的最小等价类型"；legacy 只给了重置时的 TEXT 长度，就按那组长度建。
/// </summary>
public static class ExamFixtureBuilder
{
    public const string BaseDatabaseName = "Exam_Base.mdb";

    public const string SelfReportFolderName = "SelfReport";

    /// <summary>建库时按顺序尝试的提供者，与 <see cref="AccessDb"/> 的打开顺序保持一致。</summary>
    public static readonly string[] ProviderCandidates =
    [
        "Microsoft.ACE.OLEDB.12.0",
        "Microsoft.ACE.OLEDB.16.0",
        "Microsoft.Jet.OLEDB.4.0"
    ];

    /// <summary>在 %TEMP% 下分配一个新的夹具目录（不创建）。</summary>
    public static string CreateRootDirectory(string? rootDirectory = null)
    {
        string root = string.IsNullOrWhiteSpace(rootDirectory)
            ? Path.Combine(Path.GetTempPath(), "dtexam-fixture-" + Guid.NewGuid().ToString("n"))
            : rootDirectory!;
        return Path.GetFullPath(root);
    }

    /// <summary>生成一套夹具。目录已存在且非空时会抛异常，避免把测试数据写进别人的目录。</summary>
    public static ExamFixtureSet Create(string? rootDirectory = null, ExamFixtureOptions? options = null)
    {
        options ??= new ExamFixtureOptions();
        string root = CreateRootDirectory(rootDirectory);
        if (Directory.Exists(root) && Directory.EnumerateFileSystemEntries(root).Any())
        {
            throw new InvalidOperationException("夹具目录必须是新建或空目录：" + root);
        }

        Directory.CreateDirectory(root);
        string databaseFile = Path.Combine(root, BaseDatabaseName);
        TryCreateEmptyDatabase(databaseFile);

        using (var session = new AccessDb(databaseFile))
        {
            foreach (string ddl in BuildSchema(options))
            {
                session.Execute(ddl);
            }

            if (options.SeedSampleRows)
            {
                Seed(session, options);
            }
        }

        if (options.CreateSelfReportFolder)
        {
            Directory.CreateDirectory(Path.Combine(root, SelfReportFolderName));
        }

        return new ExamFixtureSet(root);
    }

    /// <summary>
    /// 用 ADOX 新建一个空 .mdb。ACE 的 OLE DB 通道不能靠 Open 创建文件，必须先 Catalog.Create。
    /// 内部走 COM 晚绑定，所以编译期不需要 ADOX 互操作程序集；运行期缺 ADOX 会抛 InvalidOperationException。
    /// </summary>
    public static void TryCreateEmptyDatabase(string databaseFile)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);
        string? directory = Path.GetDirectoryName(Path.GetFullPath(databaseFile));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(databaseFile))
        {
            throw new InvalidOperationException("夹具库已存在，拒绝覆盖：" + databaseFile);
        }

        Type? catalogType = Type.GetTypeFromProgID("ADOX.Catalog");
        if (catalogType is null)
        {
            throw new InvalidOperationException("ADOX.Catalog 未注册，无法创建 Access 数据库。");
        }

        Exception? last = null;
        var failures = new List<string>();
        foreach (string provider in ProviderCandidates)
        {
            // 上一轮 provider 建出库但打开失败时会留下文件，先清掉，否则 ADOX 会报"文件已存在"并掩盖真实原因
            if (File.Exists(databaseFile))
            {
                try
                {
                    File.Delete(databaseFile);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    failures.Add($"清理上一轮残留库失败({provider}): {AceDiagnostics.Chain(ex)}");
                    continue;
                }
            }

            object? instance = null;
            try
            {
                instance = Activator.CreateInstance(catalogType);
                if (instance is null)
                {
                    failures.Add($"ADOX 建库({provider}): Activator.CreateInstance 返回 null");
                    continue;
                }

                dynamic catalog = instance;
                catalog.Create($"Provider={provider};Data Source={databaseFile};");
            }
            catch (Exception ex)
            {
                last = ex;
                failures.Add($"ADOX 建库({provider}): {AceDiagnostics.Chain(ex)}");
                continue;
            }
            finally
            {
                if (instance is not null && System.Runtime.InteropServices.Marshal.IsComObject(instance))
                {
                    System.Runtime.InteropServices.Marshal.FinalReleaseComObject(instance);
                }
            }

            // ADOX 建库后立刻被 ACE 打开偶发"文件被占用"，短暂重试一次更稳。
            for (int attempt = 0; attempt < 5; attempt++)
            {
                if (TryOpenOnce(databaseFile, provider, out Exception? openError))
                {
                    return;
                }

                last = openError;
                if (attempt == 4)
                {
                    failures.Add($"OLE DB 打开({provider}): 已建库但打开失败：{AceDiagnostics.Chain(openError!)}");
                }

                Thread.Sleep(50);
            }
        }

        throw new InvalidOperationException(
            "无法创建 Access 数据库：" + databaseFile
            + "；逐 provider 明细 → " + string.Join(" | ", failures),
            last);
    }

    /// <summary>
    /// 建库后的一次连通性探测：打开连接并读一次表清单。
    /// 注意不能拿 MSysObjects 当探针——ACE 默认不允许读系统表，会报"在 'MSysObjects' 上没有读取数据权限"，
    /// 那是权限错误而非 provider 缺失，用它探测会得出错误的"ACE 不可用"结论。
    /// </summary>
    private static bool TryOpenOnce(string databaseFile, string provider, out Exception? error)
    {
        try
        {
            using var connection = new System.Data.OleDb.OleDbConnection(
                $"Provider={provider};Data Source={databaseFile};Persist Security Info=False;");
            connection.Open();
            connection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables, null);
            error = null;
            return true;
        }
        catch (Exception ex)
        {
            error = ex;
            return false;
        }
    }

    /// <summary>
    /// 建表 DDL。每张表的列都在注释里标了来源，新增列前先回到源码/legacy 找出真实读写语句。
    /// </summary>
    public static IReadOnlyList<string> BuildSchema(ExamFixtureOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var statements = new List<string>
        {
            // Mss3VoteService.cs:47 (SELECT VoteId,VoteName,Threshold,SelName)
            // Mss3VoteService.cs:98 (SELECT COUNT(*) ... WHERE VoteID=?)
            // Mss3VoteService.cs:102 (INSERT 11 列)
            // legacy Util.cs:4534 / 4989 同名列
            "CREATE TABLE Vote_EssInfo (VoteID TEXT(50), VoteName TEXT(100), Threshold TEXT(50), SelName TEXT(50), "
            + "HxrScore LONG, LxrScore LONG, OutsideThreshold LONG, HxrWidth LONG, HxrHeight LONG, "
            + "LxrWidth LONG, LxrHeight LONG)",

            // Mss3VoteService.cs:86/92/117 (VoteID,PageIndex,MatrixCode,PicContent)
            // legacy Util.cs:4985/4986/4987
            // PicContent 收到的是 EncodeGrayBmp 产出的 BMP 字节流（byte[] 参数），对应 OLEOBJECT
            "CREATE TABLE Vote_PageInfo (VoteID TEXT(50), PageIndex LONG, MatrixCode TEXT(100), PicContent OLEOBJECT)",

            // ExamInsertService.cs:71/134/141/144 (ExamId,ExamName,TypeMode)
            // ExamReportService.cs:46 (Exam_EssInfo.ExamID / ExamName)
            "CREATE TABLE Exam_EssInfo (ExamId LONG, ExamName TEXT(100), TypeMode TEXT(50))",

            // ExamInsertService.cs:177/185/218/226 (ExamId,Type,Kind,ItemName,SelName,Score)
            // legacy Util.cs:4193/4197/4240/4243/4248/4251
            "CREATE TABLE Exam_Score (ExamId LONG, Type TEXT(50), Kind TEXT(50), ItemName TEXT(100), "
            + "SelName TEXT(100), Score DOUBLE)",

            // ExamInsertService.cs:167/237/244 (ExamId,AtKind,ItemName,SelName,ItemOrder,AllowMuti)
            // 只有 Sqlite 分支写 AllowMuti，Access 分支不写；列仍存在，故允许空
            "CREATE TABLE Exam_BoolItem (ExamId LONG, AtKind TEXT(50), ItemName TEXT(100), SelName TEXT(100), "
            + "ItemOrder LONG, AllowMuti LONG)",

            // ExamInsertService.cs:159 (ExamId,RulerKind,RulerPara,RulerOrder)
            // legacy Util.cs:3808
            "CREATE TABLE Exam_Ruler (ExamId LONG, RulerKind TEXT(100), RulerPara TEXT(255), RulerOrder LONG)",

            // 以下三张表是 legacy Util.cs:3820-3832 的 ExamId 绑定表清单成员，
            // legacy 对它们只有 "UPDATE {表} SET ExamID=..." 一句（Util.cs:3849/3868/3874/3879），
            // 没有任何真实读写的其它列 —— 与 Exam_People 只建 UnitCode 同理，只建 ExamID。
            "CREATE TABLE Exam_CharItem (ExamID LONG)",
            "CREATE TABLE Exam_PeopleDes (ExamID LONG)",
            "CREATE TABLE Exam_VerifyCode (ExamID LONG)",

            // legacy Util.cs:4380 "SELECT RaidName FROM Exam_RaidInfo WHERE ExamID = ..."
            // legacy Util.cs:3827 同表也在 ExamId 绑定清单里
            "CREATE TABLE Exam_RaidInfo (ExamID LONG, RaidName TEXT(100))",

            // ExamPackageService.cs:84/130/238 (SELECT */COUNT/DELETE)
            // ExamPackageService.cs:706 + legacy BodyForm.cs:11219 (UnitCode)
            // ExamID 来自 legacy Util.cs:3820-3832 的 ExamId 绑定表清单（Exam_People 在内），
            // legacy 对它执行 "UPDATE Exam_People SET ExamID=..."（Util.cs:3849），故该列确实存在。
            "CREATE TABLE Exam_People (ExamID LONG, UnitCode TEXT(50))",

            // ExamPackageService.cs:85/131/169/197/239/346/657/709 (AtOrder,Unit,UnitCode,AtKind)
            // legacy Util.cs:3701 同名列
            "CREATE TABLE Unit_Ess (AtOrder LONG, Unit TEXT(100), UnitCode TEXT(50), AtKind TEXT(50))",

            // ExamReportService.cs:46 (Report_Ess.ReportID / ReportName / ExamID)
            // legacy Util.cs:3666 / 4433 (ColWidthStr,RowHeightStr)
            "CREATE TABLE Report_Ess (ReportID LONG, ReportName TEXT(100), ExamID LONG, "
            + "ColWidthStr TEXT(255), RowHeightStr TEXT(255))",

            // ExamReportService.cs:83/130 (Item,Kind,Content) + ReportID
            // legacy Util.cs:888 (INSERT (ReportID,Kind,Item,Content))
            // Item 长度取 legacy BodyForm.cs:4425 重置值 TEXT(255)
            "CREATE TABLE Report_Detail (ReportID LONG, Kind TEXT(50), Item TEXT(255), Content MEMO)",

            // Mss3VoteService.cs:132 ALTER TABLE Page_SelectorInfo ALTER COLUMN Item Memo
            // 长度取 legacy BodyForm.cs:4431 重置值 TEXT(50)
            "CREATE TABLE Page_SelectorInfo (Item TEXT(50))",

            // Mss3VoteService.cs:132 + legacy BodyForm.cs:4428 TEXT(100)
            "CREATE TABLE Result_DetailInfo (Item TEXT(100))",

            // Mss3VoteService.cs:132 + legacy BodyForm.cs:4425 TEXT(255)
            "CREATE TABLE Result_NumInfo (Item TEXT(255))",

            // Mss3VoteService.cs:132 + legacy BodyForm.cs:4428 TEXT(100)
            "CREATE TABLE TempResult_DetailInfo (Item TEXT(100))"
        };

        if (options.IncludeExamBoolItem is false)
        {
            statements.RemoveAll(sql => sql.Contains("Exam_BoolItem", StringComparison.Ordinal));
        }

        if (options.IncludeAutoIncrementProbeColumn)
        {
            // 结构探针：仅用于验证 ADOX 自增列探测与"不把自增列写回目标库"。
            // 只替换这两张表本身；用 StartsWith 而不是 Contains，否则 "CREATE TABLE Exam_People" 会把
            // 名字更长的 Exam_PeopleDes 一起删掉。
            statements.RemoveAll(sql =>
                sql.StartsWith("CREATE TABLE Exam_People (", StringComparison.Ordinal)
                || sql.StartsWith("CREATE TABLE Unit_Ess (", StringComparison.Ordinal));
            statements.Add("CREATE TABLE Exam_People (AutoId COUNTER PRIMARY KEY, ExamID LONG, UnitCode TEXT(50))");
            statements.Add("CREATE TABLE Unit_Ess (AutoId COUNTER PRIMARY KEY, AtOrder LONG, Unit TEXT(100), "
                           + "UnitCode TEXT(50), AtKind TEXT(50))");
        }

        return statements;
    }

    /// <summary>样例行：只写源码里真实出现过的列，值本身是可辨识的合成数据。</summary>
    private static void Seed(AccessDb session, ExamFixtureOptions options)
    {
        SeedReferenceRows(session);

        if (options.SeedUnitCount > 0 || options.SeedPeopleCount > 0)
        {
            SeedSyntheticPeopleAndUnits(session, options);
            return;
        }

        session.Execute(
            "INSERT INTO Unit_Ess (AtOrder,Unit,UnitCode,AtKind) VALUES (?,?,?,?)",
            ("@p0", 1), ("@p1", "甲单位"), ("@p2", "U01"), ("@p3", "速甲"));
        session.Execute(
            "INSERT INTO Unit_Ess (AtOrder,Unit,UnitCode,AtKind) VALUES (?,?,?,?)",
            ("@p0", 2), ("@p1", "乙单位"), ("@p2", "U02"), ("@p3", "速乙"));
        session.Execute(
            "INSERT INTO Unit_Ess (AtOrder,Unit,UnitCode,AtKind) VALUES (?,?,?,?)",
            ("@p0", 3), ("@p1", "丙单位"), ("@p2", "U03"), ("@p3", "速丙"));

        session.Execute("INSERT INTO Exam_People (ExamID,UnitCode) VALUES (?,?)", ("@p0", 1), ("@p1", "U01"));
        session.Execute("INSERT INTO Exam_People (ExamID,UnitCode) VALUES (?,?)", ("@p0", 1), ("@p1", "U03"));
    }

    /// <summary>
    /// 合成人员/单位：单位按 AtOrder 1..N、人员按 UnitCode 循环引用。
    /// 只用于度量「合并 5 千人 / 200 单位」这类大数据量，不参与任何既有断言的默认夹具。
    /// </summary>
    private static void SeedSyntheticPeopleAndUnits(AccessDb session, ExamFixtureOptions options)
    {
        int unitCount = Math.Max(options.SeedUnitCount, 1);
        var codes = new string[unitCount];
        for (int i = 0; i < unitCount; i++)
        {
            codes[i] = "U" + (i + 1).ToString("D4", System.Globalization.CultureInfo.InvariantCulture);
            session.Execute(
                "INSERT INTO Unit_Ess (AtOrder,Unit,UnitCode,AtKind) VALUES (?,?,?,?)",
                ("@p0", i + 1),
                ("@p1", "合成单位" + (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture)),
                ("@p2", codes[i]),
                ("@p3", "速甲"));
        }

        for (int i = 0; i < options.SeedPeopleCount; i++)
        {
            session.Execute(
                "INSERT INTO Exam_People (ExamID,UnitCode) VALUES (?,?)",
                ("@p0", 1),
                ("@p1", codes[i % unitCount]));
        }
    }

    /// <summary>与人员/单位规模无关的参考数据（选票、考评表、报表、测评身份、ExamId 绑定表）。</summary>
    private static void SeedReferenceRows(AccessDb session)
    {
        session.Execute(
            "INSERT INTO Vote_EssInfo (VoteID,VoteName,Threshold,SelName,HxrScore,LxrScore,OutsideThreshold,"
            + "HxrWidth,HxrHeight,LxrWidth,LxrHeight) VALUES (?,?,?,?,?,?,?,?,?,?,?)",
            ("@p0", "V-001"), ("@p1", "测评票甲"), ("@p2", "45"), ("@p3", "方框"),
            ("@p4", 45), ("@p5", 15), ("@p6", 20), ("@p7", 42), ("@p8", 42), ("@p9", 150), ("@p10", 45));
        session.Execute(
            "INSERT INTO Vote_EssInfo (VoteID,VoteName,Threshold,SelName,HxrScore,LxrScore,OutsideThreshold,"
            + "HxrWidth,HxrHeight,LxrWidth,LxrHeight) VALUES (?,?,?,?,?,?,?,?,?,?,?)",
            ("@p0", "V-002"), ("@p1", "测评票乙"), ("@p2", "30"), ("@p3", "圆框"),
            ("@p4", 45), ("@p5", 15), ("@p6", 20), ("@p7", 40), ("@p8", 40), ("@p9", 140), ("@p10", 45));

        session.Execute(
            "INSERT INTO Exam_EssInfo (ExamId,ExamName,TypeMode) VALUES (?,?,?)",
            ("@p0", 1), ("@p1", "一级考评"), ("@p2", "人员纵排"));
        session.Execute(
            "INSERT INTO Exam_EssInfo (ExamId,ExamName,TypeMode) VALUES (?,?,?)",
            ("@p0", 5), ("@p1", "二级考评"), ("@p2", "人员横排"));

        // 宽高串的分隔符按 legacy Util.cs:4442-4448 的真实 Split(new[] { ",", "，" }) 用半角逗号。
        // 报表A：3 段列宽 / 3 段行高；报表B：2 段列宽 / 1 段行高（用于验证 Access 侧各减 1 的规则）。
        session.Execute(
            "INSERT INTO Report_Ess (ReportID,ReportName,ExamID,ColWidthStr,RowHeightStr) VALUES (?,?,?,?,?)",
            ("@p0", 7), ("@p1", "考评报表A"), ("@p2", 1), ("@p3", "100,120,140"), ("@p4", "25,25,25"));
        session.Execute(
            "INSERT INTO Report_Ess (ReportID,ReportName,ExamID,ColWidthStr,RowHeightStr) VALUES (?,?,?,?,?)",
            ("@p0", 8), ("@p1", "考评报表B"), ("@p2", 5), ("@p3", "80,90"), ("@p4", "28"));

        session.Execute(
            "INSERT INTO Report_Detail (ReportID,Kind,Item,Content) VALUES (?,?,?,?)",
            ("@p0", 7), ("@p1", "GridFormatData"), ("@p2", "甲项"), ("@p3", "Item=甲项&Raid=1"));
        session.Execute(
            "INSERT INTO Report_Detail (ReportID,Kind,Item,Content) VALUES (?,?,?,?)",
            ("@p0", 7), ("@p1", "GridFormatText"), ("@p2", "乙项"), ("@p3", "Item=乙项&Raid=2&Kind=3"));
        session.Execute(
            "INSERT INTO Report_Detail (ReportID,Kind,Item,Content) VALUES (?,?,?,?)",
            ("@p0", 8), ("@p1", "GridFormatData"), ("@p2", "丙项"), ("@p3", "Item=丙项&Raid=1"));

        // 测评身份：legacy Util.cs:4380 按 ExamID 查 RaidName，用于报表的「合计 / 批量设置全部身份」两行。
        session.Execute("INSERT INTO Exam_RaidInfo (ExamID,RaidName) VALUES (?,?)", ("@p0", 1), ("@p1", "领导"));
        session.Execute("INSERT INTO Exam_RaidInfo (ExamID,RaidName) VALUES (?,?)", ("@p0", 1), ("@p1", "同事"));
        session.Execute("INSERT INTO Exam_RaidInfo (ExamID,RaidName) VALUES (?,?)", ("@p0", 5), ("@p1", "上级"));

        // 只带 ExamID 的三张绑定表各给 1、5 两个序号各一行，用来验证改号时 UPDATE 真的落到了每一张表。
        foreach (string table in new[] { "Exam_CharItem", "Exam_PeopleDes", "Exam_VerifyCode" })
        {
            session.Execute($"INSERT INTO {table} (ExamID) VALUES (?)", ("@p0", 1));
            session.Execute($"INSERT INTO {table} (ExamID) VALUES (?)", ("@p0", 5));
        }
    }
}
