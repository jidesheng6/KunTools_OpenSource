using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// ExamPackageService 对真实 .mdb 数据包的操作：Inspect/Reset/ListUnits/HasDuplicateUnits/Merge。
/// 夹具目录即数据包根目录，库固定名为 Exam_Base.mdb（该常量在服务里是 private，这里按文件名写死）。
/// </summary>
public sealed class ExamPackageServiceAccessTests
{
    private readonly ExamPackageService _sut = new();

    [AceFact]
    public void Inspect_ReportsDatabaseCountsExamFilesAndSelfReport()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture(
            new ExamFixtureOptions { CreateSelfReportFolder = true });
        File.WriteAllBytes(Path.Combine(fixture.DirectoryPath, "卷一.exam"), [1, 2, 3]);
        File.WriteAllBytes(Path.Combine(fixture.DirectoryPath, "卷二.exam"), [4]);

        ExamPackageStatus status = _sut.Inspect(fixture.DirectoryPath);

        Assert.True(status.DatabaseExists);
        Assert.Equal(2, status.PeopleCount);
        Assert.Equal(3, status.UnitCount);
        Assert.Equal(2, status.ExamFileCount);
        Assert.True(status.HasSelfReport);
        Assert.False(status.IsAlreadyInitialized);
        Assert.Contains("人员 2，单位 3", status.Summary);
    }

    [AceFact]
    public void Inspect_EmptyPackage_ReportsAlreadyInitialized()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture(new ExamFixtureOptions { SeedSampleRows = false });

        ExamPackageStatus status = _sut.Inspect(fixture.DirectoryPath);

        Assert.True(status.DatabaseExists);
        Assert.Equal(0, status.PeopleCount);
        Assert.Equal(0, status.UnitCount);
        Assert.Equal(0, status.ExamFileCount);
        Assert.False(status.HasSelfReport);
        Assert.True(status.IsAlreadyInitialized);
    }

    [Fact]
    public void Inspect_DirectoryWithoutDatabase_ReportsMissing()
    {
        string root = Path.Combine(Path.GetTempPath(), "dt-inspect-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        try
        {
            ExamPackageStatus status = _sut.Inspect(root);

            Assert.False(status.DatabaseExists);
            Assert.False(status.IsAlreadyInitialized);
            Assert.Equal("未找到 Exam_Base.mdb", status.Summary);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [AceFact]
    public void Reset_ClearsRowsExamFilesAndSelfReport()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture(
            new ExamFixtureOptions { CreateSelfReportFolder = true });
        File.WriteAllBytes(Path.Combine(fixture.DirectoryPath, "卷一.exam"), [1]);

        int removed = _sut.Reset(fixture.DirectoryPath);

        Assert.Equal(5, removed);
        ExamPackageStatus status = _sut.Inspect(fixture.DirectoryPath);
        Assert.Equal(0, status.PeopleCount);
        Assert.Equal(0, status.UnitCount);
        Assert.Equal(0, status.ExamFileCount);
        Assert.False(status.HasSelfReport);
        Assert.True(status.IsAlreadyInitialized);
    }

    [AceFact]
    public void ListUnits_ReturnsUnitsOrderedByAtOrder()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        IReadOnlyList<(int Order, string Unit, string Code, string Kind)> units = _sut.ListUnits(fixture.DirectoryPath);

        Assert.Equal(
            [(1, "甲单位", "U01", "速甲"), (2, "乙单位", "U02", "速乙"), (3, "丙单位", "U03", "速丙")],
            units);
    }

    [AceFact]
    public void HasDuplicateUnits_DetectsSharedUnitCode()
    {
        using ExamFixtureSet source = ExamAccessTestKit.NewFixture();
        using ExamFixtureSet target = ExamAccessTestKit.NewFixture();

        Assert.True(_sut.HasDuplicateUnits(source.DirectoryPath, target.DirectoryPath));
    }

    [AceFact]
    public void HasDuplicateUnits_ReturnsFalseWhenCodesAreDisjoint()
    {
        using ExamFixtureSet source = ExamAccessTestKit.NewFixture();
        using ExamFixtureSet target = ExamAccessTestKit.NewFixture(new ExamFixtureOptions { SeedSampleRows = false });
        InsertUnit(target, 1, "丁单位", "U99", "速丁");

        Assert.False(_sut.HasDuplicateUnits(source.DirectoryPath, target.DirectoryPath));
    }

    [AceFact]
    public void HasDuplicateUnits_IgnoresCaseInUnitCode()
    {
        using ExamFixtureSet source = ExamAccessTestKit.NewFixture();
        using ExamFixtureSet target = ExamAccessTestKit.NewFixture(new ExamFixtureOptions { SeedSampleRows = false });
        InsertUnit(target, 1, "甲单位", "u01", "速甲");

        Assert.True(_sut.HasDuplicateUnits(source.DirectoryPath, target.DirectoryPath));
    }

    [AceFact]
    public void Merge_IntoEmptyTarget_InsertsAllPeopleAndUnits()
    {
        using ExamFixtureSet source = ExamAccessTestKit.NewFixture();
        using ExamFixtureSet target = ExamAccessTestKit.NewFixture(new ExamFixtureOptions { SeedSampleRows = false });

        ExamPackageMergeResult result = _sut.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = source.DirectoryPath,
            TargetDirectory = target.DirectoryPath,
            CopyExamFiles = false
        });

        Assert.False(result.HadDuplicateUnits);
        Assert.Equal(0, result.DuplicateUnitsRemoved);
        Assert.Equal(2, result.PeopleInserted);
        Assert.Equal(3, result.UnitsInserted);
        Assert.Equal([(1, "U01"), (2, "U02"), (3, "U03")], UnitOrders(target));
        Assert.Equal(["U01", "U03"], PeopleCodes(target));
        Assert.Contains("人员 +2", result.Summary);
    }

    [AceFact]
    public void Merge_DuplicateUnitsInTarget_RemovedBeforeAppending()
    {
        using ExamFixtureSet source = ExamAccessTestKit.NewFixture();
        using ExamFixtureSet target = ExamAccessTestKit.NewFixture(new ExamFixtureOptions { SeedSampleRows = false });
        InsertUnit(target, 1, "旧乙单位", "U02", "速旧");
        InsertPerson(target, "U02");

        ExamPackageMergeResult result = _sut.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = source.DirectoryPath,
            TargetDirectory = target.DirectoryPath,
            CopyExamFiles = false
        });

        Assert.True(result.HadDuplicateUnits);
        Assert.Equal(1, result.DuplicateUnitsRemoved);
        Assert.Equal(2, result.PeopleInserted);
        Assert.Equal(3, result.UnitsInserted);
        // 目标库原有的 U02 已被清掉，源包的三个单位按 MAX(AtOrder)+1 重新编号。
        Assert.Equal([(1, "U01"), (2, "U02"), (3, "U03")], UnitOrders(target));
        using var session = target.OpenDatabase();
        Assert.Equal("乙单位", ExamAccessTestKit.Text(
            ExamAccessTestKit.Rows(session, "SELECT Unit FROM Unit_Ess WHERE UnitCode='U02'")[0], "Unit"));
        Assert.Equal(["U01", "U03"], PeopleCodes(target));
    }

    [AceFact]
    public void Merge_TargetAutoIncrementColumn_IsNotCopiedFromSource()
    {
        // 目标库 Exam_People 的 AutoId 是 COUNTER 主键（结构探针列）。源包同样带 AutoId=1,2，
        // 若把源的自增值原样写回就会撞主键；Merge 必须靠 ADOX/GetSchemaTable 判定并跳过该列。
        using ExamFixtureSet source = ExamAccessTestKit.NewFixture(
            new ExamFixtureOptions { IncludeAutoIncrementProbeColumn = true });
        using ExamFixtureSet target = ExamAccessTestKit.NewFixture(
            new ExamFixtureOptions { IncludeAutoIncrementProbeColumn = true, SeedSampleRows = false });
        InsertPerson(target, "X1");
        InsertPerson(target, "X2");

        ExamPackageMergeResult result = _sut.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = source.DirectoryPath,
            TargetDirectory = target.DirectoryPath,
            CopyExamFiles = false
        });

        Assert.Equal(2, result.PeopleInserted);
        using var session = target.OpenDatabase();
        List<int> autoIds = ExamAccessTestKit
            .Rows(session, "SELECT AutoId FROM Exam_People ORDER BY AutoId")
            .Select(row => Convert.ToInt32(row["AutoId"]))
            .ToList();
        Assert.Equal([1, 2, 3, 4], autoIds);
    }

    [AceFact]
    public void Merge_ExamFileWithSameName_IsCopiedAsRenamedCopy()
    {
        using ExamFixtureSet source = ExamAccessTestKit.NewFixture();
        using ExamFixtureSet target = ExamAccessTestKit.NewFixture();
        File.WriteAllBytes(Path.Combine(source.DirectoryPath, "卷一.exam"), [7, 7]);
        File.WriteAllBytes(Path.Combine(target.DirectoryPath, "卷一.exam"), [9]);

        ExamPackageMergeResult result = _sut.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = source.DirectoryPath,
            TargetDirectory = target.DirectoryPath
        });

        Assert.Equal(1, result.ExamFilesCopied);
        Assert.Equal([9], File.ReadAllBytes(Path.Combine(target.DirectoryPath, "卷一.exam")));
        Assert.Equal([7, 7], File.ReadAllBytes(Path.Combine(target.DirectoryPath, "卷一_1.exam")));
        Assert.Contains("_1/_2 后缀", result.Summary);
    }

    private static void InsertUnit(ExamFixtureSet fixture, int order, string unit, string code, string kind)
    {
        using var session = fixture.OpenDatabase();
        session.Execute(
            "INSERT INTO Unit_Ess (AtOrder,Unit,UnitCode,AtKind) VALUES (?,?,?,?)",
            ("@p0", order), ("@p1", unit), ("@p2", code), ("@p3", kind));
    }

    private static void InsertPerson(ExamFixtureSet fixture, string unitCode)
    {
        using var session = fixture.OpenDatabase();
        session.Execute("INSERT INTO Exam_People (UnitCode) VALUES (?)", ("@p0", unitCode));
    }

    private static List<(int Order, string Code)> UnitOrders(ExamFixtureSet fixture)
    {
        using var session = fixture.OpenDatabase();
        return ExamAccessTestKit
            .Rows(session, "SELECT AtOrder,UnitCode FROM Unit_Ess ORDER BY AtOrder ASC")
            .Select(row => (Convert.ToInt32(row["AtOrder"]), ExamAccessTestKit.Text(row, "UnitCode")))
            .ToList();
    }

    private static List<string> PeopleCodes(ExamFixtureSet fixture)
    {
        using var session = fixture.OpenDatabase();
        return ExamAccessTestKit
            .Rows(session, "SELECT UnitCode FROM Exam_People ORDER BY UnitCode ASC")
            .Select(row => ExamAccessTestKit.Text(row, "UnitCode"))
            .ToList();
    }
}
