using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// 考评表序号修改 + Exam_EssInfo 物理顺序重建（legacy Util.cs:3817-3915 ChangeTableIndex、
/// :3930-4073 RebuildExamEssInfoPhysicalOrder、界面入口 BodyForm.cs:9827-9902）。
/// </summary>
public sealed class ExamIndexServiceAccessTests
{
    private readonly ExamIndexService _sut = new();

    [AceFact]
    public void ChangeExamIndex_FreeTarget_UpdatesEveryExamIdBoundTable()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamIndexChangeResult result = _sut.ChangeExamIndex(new ExamIndexChangeRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            OriginIndex = 1,
            NewIndex = 9,
            RebuildPhysicalOrder = false
        });

        Assert.True(result.Success, result.Summary);
        Assert.False(result.Swapped);
        Assert.Equal(-1, result.RecommendedExamId);
        Assert.False(result.PhysicalOrderRebuilt);
        // 夹具里 10 张 ExamID 绑定表全都存在，所以一张都不能被跳过。
        Assert.Equal(ExamIndexService.ExamIdBoundTables.Length, result.UpdatedTables.Count);

        using var session = fixture.OpenDatabase();
        Assert.Equal(
            "一级考评",
            Convert.ToString(session.Scalar("SELECT ExamName FROM Exam_EssInfo WHERE ExamId=9")));
        Assert.Equal(0, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_EssInfo WHERE ExamId=1"));
        Assert.Equal(2, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_EssInfo"));
        Assert.Equal(1, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Report_Ess WHERE ExamID=9 AND ReportID=7"));
        Assert.Equal(0, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Report_Ess WHERE ExamID=1"));
        Assert.Equal(2, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_RaidInfo WHERE ExamID=9"));
        Assert.Equal(1, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_CharItem WHERE ExamID=9"));
        Assert.Equal(2, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_People WHERE ExamID=9"));

        foreach (string table in ExamIndexService.ExamIdBoundTables)
        {
            Assert.Equal(0, ExamAccessTestKit.Count(session, $"SELECT COUNT(*) FROM [{table}] WHERE ExamID=1"));
        }
    }

    [AceFact]
    public void ChangeExamIndex_OccupiedTarget_SwapsBothIndexes()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamIndexChangeResult result = _sut.ChangeExamIndex(new ExamIndexChangeRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            OriginIndex = 1,
            NewIndex = 5,
            RebuildPhysicalOrder = false
        });

        Assert.True(result.Success, result.Summary);
        Assert.True(result.Swapped);
        Assert.Equal(6, result.RecommendedExamId);

        using var session = fixture.OpenDatabase();
        Assert.Equal(
            "一级考评",
            Convert.ToString(session.Scalar("SELECT ExamName FROM Exam_EssInfo WHERE ExamId=5")));
        Assert.Equal(
            "二级考评",
            Convert.ToString(session.Scalar("SELECT ExamName FROM Exam_EssInfo WHERE ExamId=1")));
        Assert.Equal(1, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Report_Ess WHERE ReportID=7 AND ExamID=5"));
        Assert.Equal(1, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Report_Ess WHERE ReportID=8 AND ExamID=1"));
        Assert.Equal(2, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_RaidInfo WHERE ExamID=5"));
        Assert.Equal(1, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_RaidInfo WHERE ExamID=1"));
    }

    [AceFact]
    public void ChangeExamIndex_RebuildPhysicalOrder_OrdersRowsByExamId()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        using (var seed = fixture.OpenDatabase())
        {
            seed.Execute(
                "INSERT INTO Exam_EssInfo (ExamId,ExamName,TypeMode) VALUES (?,?,?)",
                ("@p0", 3), ("@p1", "三级考评"), ("@p2", "人员纵排"));
        }

        Assert.Equal(new[] { 1, 5, 3 }, ReadExamIds(fixture));

        ExamIndexChangeResult result = _sut.ChangeExamIndex(new ExamIndexChangeRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            OriginIndex = 1,
            NewIndex = 2
        });

        Assert.True(result.Success, result.Summary);
        Assert.True(result.PhysicalOrderRebuilt, result.Summary);
        Assert.Contains("物理顺序已按 ExamID 升序重建", result.Summary);
        Assert.Equal(new[] { 2, 3, 5 }, ReadExamIds(fixture));
    }

    [AceFact]
    public void RebuildExamEssInfoPhysicalOrder_VerifiesEveryRow()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamPhysicalOrderResult result = _sut.RebuildExamEssInfoPhysicalOrder(fixture.DatabaseFile);

        Assert.True(result.Success, result.Message);
        Assert.Equal(2, result.RowCount);
        Assert.Contains("逐列校验一致", result.Message);
        Assert.Equal(new[] { 1, 5 }, ReadExamIds(fixture));
    }

    [AceFact]
    public void RebuildExamEssInfoPhysicalOrder_MissingDatabase_ReportsFailure()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        string missing = Path.Combine(fixture.DirectoryPath, "not-there.mdb");

        ExamPhysicalOrderResult result = _sut.RebuildExamEssInfoPhysicalOrder(missing);

        Assert.False(result.Success);
        Assert.Contains("数据库文件不存在", result.Message);
    }

    [Fact]
    public void ChangeExamIndex_MissingDatabase_Throws()
    {
        string missing = Path.Combine(Path.GetTempPath(), "dt-index-" + Guid.NewGuid().ToString("n") + ".mdb");

        var ex = Assert.Throws<FileNotFoundException>(() => _sut.ChangeExamIndex(new ExamIndexChangeRequest
        {
            DatabaseFile = missing,
            OriginIndex = 1,
            NewIndex = 2
        }));

        Assert.Contains("数据库文件不存在", ex.Message);
    }

    [Fact]
    public void ChangeExamIndex_NegativeIndex_Throws()
    {
        // 序号校验在打开数据库之前，所以一个空文件就够，不需要 ACE。
        string path = Path.Combine(Path.GetTempPath(), "dt-index-" + Guid.NewGuid().ToString("n") + ".mdb");
        File.WriteAllBytes(path, []);
        try
        {
            var ex = Assert.Throws<ArgumentException>(() => _sut.ChangeExamIndex(new ExamIndexChangeRequest
            {
                DatabaseFile = path,
                OriginIndex = -1,
                NewIndex = 2
            }));

            Assert.Contains("原始序号不能为负数", ex.Message);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>不带 ORDER BY 读 Exam_EssInfo —— 这正是 legacy 填下拉框的读法，用来验证物理顺序。</summary>
    private static int[] ReadExamIds(ExamFixtureSet fixture)
    {
        using var session = fixture.OpenDatabase();
        return ExamAccessTestKit
            .Rows(session, "SELECT ExamId FROM Exam_EssInfo")
            .Select(row => Convert.ToInt32(row["ExamId"]))
            .ToArray();
    }
}
