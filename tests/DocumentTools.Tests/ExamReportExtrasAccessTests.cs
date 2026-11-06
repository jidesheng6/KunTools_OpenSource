using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// 报表页在 Content 替换之外、legacy 还有的三项能力：
/// 网格行列数（Util.cs:4424-4458）、测评身份清单（Util.cs:4372-4416）、数据格式段 Kind 的替换（Util.cs:848）。
/// </summary>
public sealed class ExamReportExtrasAccessTests
{
    private readonly ExamReportService _sut = new();

    [AceFact]
    public void GetGridSize_DropsHiddenLastSegmentLikeLegacyAccess()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamReportGrid grid = _sut.GetGridSize(fixture.DatabaseFile, 7);

        // 夹具 Report_Ess：ColWidthStr="100,120,140"、RowHeightStr="25,25,25" → 3 段各减 1。
        Assert.Equal(2, grid.Columns);
        Assert.Equal(2, grid.Rows);
        Assert.Equal(new[] { 1, 2 }, grid.ColumnIndexes.ToArray());
        Assert.Equal(new[] { 1, 2 }, grid.RowIndexes.ToArray());
    }

    [AceFact]
    public void GetGridSize_SingleSegmentRowHeight_YieldsZeroRows()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamReportGrid grid = _sut.GetGridSize(fixture.DatabaseFile, 8);

        Assert.Equal(1, grid.Columns);
        Assert.Equal(0, grid.Rows);
        Assert.Equal(new[] { 1 }, grid.ColumnIndexes.ToArray());
        Assert.Empty(grid.RowIndexes);
    }

    [AceFact]
    public void GetGridSize_InvalidReportId_Throws()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        var ex = Assert.Throws<ArgumentException>(() => _sut.GetGridSize(fixture.DatabaseFile, 0));

        Assert.Contains("请先选中一个报表", ex.Message);
    }

    [AceFact]
    public void ListExamRaids_AppendsSumAndBatchRows()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        IReadOnlyList<ExamRaidOption> options = _sut.ListExamRaids(fixture.DatabaseFile, 1);

        Assert.Equal(4, options.Count);
        Assert.Equal(new[] { "领导", "同事", "合计", "批量设置全部身份" },
            options.Select(option => option.Display).ToArray());
        // 合计用全角逗号拼接（legacy Util.cs:4393），批量行是 "a|b|合计"。
        Assert.Equal("领导，同事", options[2].Value);
        Assert.Equal("领导|同事|领导，同事", options[3].Value);
        Assert.Equal(
            new[]
            {
                ExamRaidOptionKind.Identity,
                ExamRaidOptionKind.Identity,
                ExamRaidOptionKind.Total,
                ExamRaidOptionKind.BatchAll
            },
            options.Select(option => option.Kind).ToArray());
    }

    [Fact]
    public void BuildRaidClipboardText_JoinsIdentitiesAndTotalWithoutBatchRow()
    {
        var raids = new[]
        {
            new ExamRaidOption { Display = "领导", Value = "领导", Kind = ExamRaidOptionKind.Identity },
            new ExamRaidOption { Display = "同事", Value = "同事", Kind = ExamRaidOptionKind.Identity },
            new ExamRaidOption { Display = "合计", Value = "领导，同事", Kind = ExamRaidOptionKind.Total },
            new ExamRaidOption
            {
                Display = "批量设置全部身份",
                Value = "领导|同事|领导，同事",
                Kind = ExamRaidOptionKind.BatchAll
            }
        };

        // legacy ExamRaidsCopy_Click 的拼法：显示名逐个追加「，」，末尾不留分隔符。
        Assert.Equal("领导，同事，合计", new ExamReportService().BuildRaidClipboardText(raids));
    }

    [Fact]
    public void BuildRaidClipboardText_EmptyList_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, new ExamReportService().BuildRaidClipboardText(Array.Empty<ExamRaidOption>()));
    }

    [AceFact]
    public void ListExamRaids_NoRaids_UsesPlaceholderLabel()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        IReadOnlyList<ExamRaidOption> options = _sut.ListExamRaids(fixture.DatabaseFile, 999);

        Assert.Equal(2, options.Count);
        Assert.Equal("无测评身份", options[0].Display);
        Assert.Equal(string.Empty, options[0].Value);
        Assert.Equal("批量设置全部身份", options[1].Display);
        Assert.Equal("|", options[1].Value);
    }

    [AceFact]
    public void ReplaceContent_OldRaid_RestrictsToThatRaid()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ChangeRaid = true,
            OldRaid = "2",
            NewRaid = "8"
        });

        Assert.Equal(2, result.ScannedRows);
        Assert.Equal(1, result.ChangedRows);
        Assert.Equal("Item=甲项&Raid=1", ContentOf(fixture, 7, "甲项"));
        Assert.Equal("Item=乙项&Raid=8&Kind=3", ContentOf(fixture, 7, "乙项"));
    }

    [AceFact]
    public void ReplaceContent_ItemSelectedWithOnlyRaidChange_FiltersToThatItem()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // 选中「甲项」、只勾 Raid：legacy 的 ChangeType 是过滤条件（Util.cs:842-854），不能改到整张报表。
        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ItemName = "甲项",
            ChangeRaid = true,
            NewRaid = "4"
        });

        Assert.Equal(2, result.ScannedRows);
        Assert.Equal(1, result.ChangedRows);
        Assert.Equal("Item=甲项&Raid=4", ContentOf(fixture, 7, "甲项"));
        Assert.Equal("Item=乙项&Raid=2&Kind=3", ContentOf(fixture, 7, "乙项"));
    }

    [AceFact]
    public void ReplaceContent_ChangeDataFormat_ReplacesKindSegment()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ReportContentReplaceResult result = _sut.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ChangeDataFormat = true,
            NewDataFormat = "7"
        });

        Assert.Equal(2, result.ScannedRows);
        // 甲项本来没有 Kind= 段，正则落空 → 内容不变，也不计入 ChangedRows。
        Assert.Equal(1, result.ChangedRows);
        Assert.Equal("Item=甲项&Raid=1", ContentOf(fixture, 7, "甲项"));
        Assert.Equal("Item=乙项&Raid=2&Kind=7", ContentOf(fixture, 7, "乙项"));
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
