using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// ExamReportService 对真实 .mdb 的读写：Report_Ess⋈Exam_EssInfo 列表与 Report_Detail.Content 的串联替换。
/// </summary>
public sealed class ExamReportServiceAccessTests
{
    private readonly ExamReportService _sut = new();

    [AceFact]
    public void ListReports_JoinsExamEssInfo()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        IReadOnlyList<ExamReportRow> reports = _sut.ListReports(fixture.DatabaseFile);

        Assert.Equal(2, reports.Count);
        ExamReportRow first = Assert.Single(reports, row => row.ReportId == 7);
        Assert.Equal("考评报表A", first.ReportName);
        Assert.Equal(1, first.ExamId);
        Assert.Equal("一级考评", first.ExamName);
        ExamReportRow second = Assert.Single(reports, row => row.ReportId == 8);
        Assert.Equal("二级考评", second.ExamName);
    }

    [AceFact]
    public void ReplaceContent_AllItems_ReplacesEveryRaid()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ChangeRaid = true,
            NewRaid = "9"
        });

        Assert.Equal(2, result.ScannedRows);
        Assert.Equal(2, result.ChangedRows);
        Assert.Equal("Item=甲项&Raid=9", ContentOf(fixture, 7, "甲项"));
        Assert.Equal("Item=乙项&Raid=9&Kind=3", ContentOf(fixture, 7, "乙项"));
        // 报表 8 不在本次范围内，必须原样保留。
        Assert.Equal("Item=丙项&Raid=1", ContentOf(fixture, 8, "丙项"));
    }

    [AceFact]
    public void ReplaceContent_SingleItem_ChangesOnlyThatItemName()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ItemName = "甲项",
            ChangeItemName = true,
            NewItemName = "丙项"
        });

        Assert.Equal(2, result.ScannedRows);
        Assert.Equal(1, result.ChangedRows);
        Assert.Equal("Item=丙项&Raid=1", ContentOf(fixture, 7, "甲项"));
        Assert.Equal("Item=乙项&Raid=2&Kind=3", ContentOf(fixture, 7, "乙项"));
    }

    [AceFact]
    public void ReplaceContent_EmptyNewRaid_DeletesThatRaidButKeepsSegment()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // legacy「评测主体修改内容为空则表示删除该报表内的评测主体」（BodyForm label173）：
        // 实现是 Replace(Content,'Raid={旧}','Raid=')，段保留、值清空。
        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ChangeRaid = true,
            OldRaid = "1",
            NewRaid = string.Empty
        });

        Assert.Equal(2, result.ScannedRows);
        Assert.Equal(1, result.ChangedRows);
        Assert.Equal("Item=甲项&Raid=", ContentOf(fixture, 7, "甲项"));
        Assert.Equal("Item=乙项&Raid=2&Kind=3", ContentOf(fixture, 7, "乙项"));
    }

    [AceFact]
    public void ReplaceContent_EmptyNewAndOldRaid_ChangesNothing()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // 两个都空时 legacy 的 Replace('Raid=','Raid=') 不产生变化：不报错、不改任何行。
        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ChangeRaid = true,
            OldRaid = string.Empty,
            NewRaid = string.Empty
        });

        Assert.Equal(2, result.ScannedRows);
        Assert.Equal(0, result.ChangedRows);
        Assert.Equal("Item=甲项&Raid=1", ContentOf(fixture, 7, "甲项"));
    }

    [AceFact]
    public void ReplaceContent_ContentWithoutRaid_AppendsRaid()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        using (var session = fixture.OpenDatabase())
        {
            session.Execute(
                "INSERT INTO Report_Detail (ReportID,Kind,Item,Content) VALUES (?,?,?,?)",
                ("@p0", 7), ("@p1", "GridFormatData"), ("@p2", "丁项"), ("@p3", "Item=丁项&Kind=2"));
        }

        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ItemName = "丁项",
            ChangeRaid = true,
            NewRaid = "9"
        });

        // 选中「丁项」后只勾 Raid：legacy 的三条 Replace 都以选中项目为条件（Util.cs:842-854），
        // 所以只有丁项这一行被处理，其余行即使被扫描也不改。
        Assert.Equal(3, result.ScannedRows);
        Assert.Equal(1, result.ChangedRows);
        Assert.Equal("Item=丁项&Kind=2&Raid=9", ContentOf(fixture, 7, "丁项"));
        Assert.Equal("Item=甲项&Raid=1", ContentOf(fixture, 7, "甲项"));
    }

    [AceFact]
    public void ReplaceContent_InvalidReportId_Throws()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        var ex = Assert.Throws<ArgumentException>(() => _sut.ReplaceContent(
            fixture.DatabaseFile,
            new ReportContentReplaceRequest { ReportId = 0, ChangeRaid = true, NewRaid = "9" }));

        Assert.Contains("请先选中一个报表", ex.Message);
    }

    [AceFact]
    public void ReplaceContent_NoChangeFlag_Throws()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        var ex = Assert.Throws<ArgumentException>(() => _sut.ReplaceContent(
            fixture.DatabaseFile,
            new ReportContentReplaceRequest { ReportId = 7 }));

        Assert.Contains("请勾选要修改的内容", ex.Message);
    }

    private static string ContentOf(ExamFixtureSet fixture, int reportId, string item)
    {
        using var session = fixture.OpenDatabase();
        return Convert.ToString(
                   session.Scalar(
                       "SELECT Content FROM Report_Detail WHERE ReportID=? AND Item=?",
                       ("@p0", reportId),
                       ("@p1", item)))
               ?? string.Empty;
    }
}
