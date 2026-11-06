using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// exam.insert / exam.report 的 UI↔Core 契约（task-19 审计）。
/// 每条用例钉住「App 面板上的输入到底有没有落到库里」，变红即说明 UI 侧又出现了
/// 「面板能点但后端不读」的假参数，或 Core 的语义被改坏。
/// App 侧接线位置（只读引用，便于对照）：
/// ExamInsertView.cs:41 分值框、:318/:364 组选项、:491 ScorePer 硬编码、:492 CalcAvg；
/// ExamReportView.cs:70 身份下拉回填 NewRaid、:196 用 row.ExamId 取身份。
/// </summary>
public sealed class ExamUiContractTests
{
    private readonly ExamInsertService _insert = new();

    [AceFact]
    public void SelectorScorePer_IsWrittenIntoExamScore_SoHardcodedZeroLosesPanelValue()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // 三个选项刻意用三种来源：面板填 7.5、面板留 0、压根没填。
        // Core 逐个 ParseScore 后写进 Exam_Score.Score（ExamInsertService.cs:193/228），
        // 所以 UI 在 ExamInsertView.cs:491 恒传 "0" 就等于把面板分值整列丢掉。
        ExamInsertResult result = _insert.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "面板分值考评",
            Items =
            [
                new ExamInsertItem
                {
                    ItemName = "德",
                    ItemScore = "10",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = "优秀", ScorePer = "7.5" },
                        new ExamInsertSelector { SelectorName = "合格", ScorePer = "0" },
                        new ExamInsertSelector { SelectorName = "基本合格", ScorePer = "" }
                    ]
                }
            ]
        });

        Assert.True(result.Success, result.Summary);

        using var session = fixture.OpenDatabase();
        // 只取面板上的三个选项；core 按 legacy 追加的「无效/弃权」行另有用例覆盖。
        List<Dictionary<string, object?>> scores = ExamAccessTestKit.Rows(
            session,
            $"SELECT SelName, Kind, Score FROM Exam_Score WHERE ExamId={result.ExamId} AND Kind='{ExamInsertRules.SelectorType}' " +
            "AND SelName IN ('优秀','合格','基本合格')");

        Assert.Equal(3, scores.Count);
        Assert.Equal(7.5d, ScoreFor(scores, "优秀"));
        Assert.Equal(0d, ScoreFor(scores, "合格"));
        Assert.Equal(0d, ScoreFor(scores, "基本合格"));
    }

    [AceFact]
    public void CalcAvgFlag_OnlyAffectsExamScore_AndLeavesBoolItemRowsUntouched()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // 同一张考评表里放两个项目，唯一差别是选项的 CalcAvg：
        // 计入得票率（true）只写一行分值；不计入（false）在 legacy 里是「先写原分值行，再补一行
        // 选项不计入得票率=1」（Util.cs:4240/4243/4267-4279），Exam_BoolItem 两条路径完全一样（Util.cs:4225）。
        ExamInsertResult result = _insert.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "得票率考评",
            Items =
            [
                new ExamInsertItem
                {
                    ItemName = "德",
                    ItemScore = "10",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = "优秀", ScorePer = "3", CalcAvg = true },
                        new ExamInsertSelector { SelectorName = "合格", ScorePer = "2", CalcAvg = true }
                    ]
                },
                new ExamInsertItem
                {
                    ItemName = "能",
                    ItemScore = "10",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = "优秀", ScorePer = "3", CalcAvg = false },
                        new ExamInsertSelector { SelectorName = "合格", ScorePer = "2", CalcAvg = false }
                    ]
                }
            ]
        });

        Assert.True(result.Success, result.Summary);

        using var session = fixture.OpenDatabase();

        List<Dictionary<string, object?>> boolItems = ExamAccessTestKit.Rows(
            session,
            $"SELECT ItemName, SelName, ItemOrder FROM Exam_BoolItem WHERE ExamId={result.ExamId} ORDER BY ItemOrder");
        Assert.Equal(4, boolItems.Count);
        Assert.Equal(
            [("德", "优秀"), ("德", "合格"), ("能", "优秀"), ("能", "合格")],
            boolItems.Select(row => (ExamAccessTestKit.Text(row, "ItemName"), ExamAccessTestKit.Text(row, "SelName"))).ToArray());

        // 计入得票率的项目：项目行 1 + 选项行 2 + 隐式「弃权」1 +「无效」2（无效默认不计入）＝ 6。
        Assert.Equal(
            6,
            ExamAccessTestKit.Count(
                session, $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId={result.ExamId} AND ItemName='德'"));

        // 不计入得票率的项目：项目行 1 + 选项行 2 + 补写的「不计入得票率」行 2 + 隐式 3 ＝ 8。
        Assert.Equal(
            8,
            ExamAccessTestKit.Count(
                session, $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId={result.ExamId} AND ItemName='能'"));

        List<Dictionary<string, object?>> excluded = ExamAccessTestKit.Rows(
            session,
            $"SELECT SelName, Score FROM Exam_Score WHERE ExamId={result.ExamId} AND ItemName='能' " +
            $"AND Kind='{ExamInsertRules.SelectorExcludedFromAverageType}' ORDER BY SelName");
        // 优秀/合格各一行，外加隐式「无效」那一行（无效默认不计入得票率）。
        Assert.Equal(3, excluded.Count);
        Assert.All(excluded, row => Assert.Equal(1d, Convert.ToDouble(row["Score"])));

        // 原分值行没有被「不计入」那一行取代，两行并存（legacy 也是两行）。
        Assert.Equal(
            3d,
            ScoreFor(
                ExamAccessTestKit.Rows(
                    session,
                    $"SELECT SelName, Kind, Score FROM Exam_Score WHERE ExamId={result.ExamId} AND ItemName='能' " +
                    $"AND Kind='{ExamInsertRules.SelectorType}'"),
                "优秀"));
    }

    [Fact]
    public void BatchItemChineseNumbering_CoversLegacyOneThroughTwelve()
    {
        // ExamInsertView.cs:272-276 在「中文序号」勾选时走这条路径（useChineseNumbering=true）。
        IReadOnlyList<ExamInsertItem> items =
            ExamBatchInsertTemplates.ExpandItems("第X项", 12, "5", useChineseNumbering: true);

        Assert.Equal(
            [
                "第一项", "第二项", "第三项", "第四项", "第五项", "第六项",
                "第七项", "第八项", "第九项", "第十项", "第十一项", "第十二项"
            ],
            items.Select(item => item.ItemName).ToArray());
        Assert.All(items, item => Assert.Equal("5", item.ItemScore));
    }

    [AceFact]
    public void ChangeExamIndex_OccupiedTarget_SummaryReportsSwapInsteadOfFailure()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        var service = new ExamIndexService();

        // 目标序号 5 已被「二级考评」占用：legacy 走三步换位（Util.cs:3863-3891）并把 MAX+1 作为推荐序号返回。
        ExamIndexChangeResult result = service.ChangeExamIndex(new ExamIndexChangeRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            OriginIndex = 1,
            NewIndex = 5,
            RebuildPhysicalOrder = false
        });

        Assert.True(result.Success, result.Summary);
        Assert.True(result.Swapped);
        Assert.Equal(6, result.RecommendedExamId);

        // UI 只把 Summary 显示给用户（ExamInsertView.cs:406），所以这里必须说清「已互换」，
        // 不能出现「修改失败」这种误导（legacy 的失败文案是死分支，见 BodyForm.cs:9879-9887）。
        Assert.Contains("已互换", result.Summary);
        Assert.Contains("临时序号 6", result.Summary);
        Assert.DoesNotContain("修改失败", result.Summary);
        Assert.Contains("Exam_EssInfo", result.UpdatedTables);

        // 换位而不是覆盖：两张表各自换了序号，谁也不丢。
        using var session = fixture.OpenDatabase();
        Assert.Equal(
            2, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_EssInfo"));
        Assert.Equal(
            "一级考评",
            Convert.ToString(ExamAccessTestKit.Rows(session, "SELECT ExamName FROM Exam_EssInfo WHERE ExamId=5")[0]["ExamName"]));
        Assert.Equal(
            "二级考评",
            Convert.ToString(ExamAccessTestKit.Rows(session, "SELECT ExamName FROM Exam_EssInfo WHERE ExamId=1")[0]["ExamName"]));
    }

    [AceFact]
    public void ReportRaidOption_FromListReportsExamId_IsWritableAsNewRaid()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        var service = new ExamReportService();

        // UI 的链路：选中报表 → row.ExamId → ListExamRaids(row.ExamId) → 选中身份 → NewRaid（ExamReportView.cs:196/70）。
        ExamReportRow report = Assert.Single(service.ListReports(fixture.DatabaseFile), row => row.ReportId == 7);

        // ExamId 必须来自 Exam_EssInfo.ExamID（ExamReportService.cs:104/113），不是 ReportID。
        Assert.Equal(1, report.ExamId);

        IReadOnlyList<ExamRaidOption> raids = service.ListExamRaids(fixture.DatabaseFile, report.ExamId);
        ExamRaidOption leader = Assert.Single(raids, option => option.Display == "领导");
        Assert.Equal("领导", leader.Value);

        ReportContentReplaceResult replaced = service.ReplaceContent(fixture.DatabaseFile, new ReportContentReplaceRequest
        {
            ReportId = report.ReportId,
            ItemName = "甲项",
            ChangeRaid = true,
            NewRaid = leader.Value
        });

        Assert.Equal(1, replaced.ChangedRows);

        using var session = fixture.OpenDatabase();
        Assert.Equal(
            "Item=甲项&Raid=领导",
            Convert.ToString(
                ExamAccessTestKit.Rows(
                    session,
                    "SELECT Content FROM Report_Detail WHERE ReportID=7 AND Item='甲项'")[0]["Content"]));
    }

    private static double ScoreFor(List<Dictionary<string, object?>> rows, string selectorName)
        => Convert.ToDouble(Assert.Single(rows, row => ExamAccessTestKit.Text(row, "SelName") == selectorName)["Score"]);
}
