using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// exam.insert 的三种得分比重（得分 / 弃权 / 无效）落库验收。
/// legacy 依据：BodyForm.Designer.cs:12357（弃权比重默认 -1）、:12391（无效比重默认 -1）、
/// :12558（得分比重默认 0）、:12226（弃权默认计入得票率）、:12336（无效默认不计入）；
/// BodyForm.cs:9254-9268 为每个项目追加「无效」「弃权」两条隐式选项；
/// Util.cs:4208-4211 隐式选项不占 ItemOrder、:4261-4265 不写 Exam_BoolItem、:4240 写 Exam_Score。
/// </summary>
public sealed class ExamInsertScoreWeightAccessTests
{
    private readonly ExamInsertService _sut = new();

    [AceFact]
    public void Insert_DefaultWeights_WriteLegacyValuesIntoExamScore()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // 不传任何一种比重，走 legacy 默认：得分 0、弃权 -1、无效 -1。
        ExamInsertResult result = _sut.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "默认比重考评",
            Items =
            [
                new ExamInsertItem
                {
                    ItemName = "德",
                    ItemScore = "10",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = "优秀", ScorePer = "3" }
                    ]
                }
            ]
        });

        Assert.True(result.Success, result.Summary);
        Assert.Equal(2, result.ImplicitSelectorCount);

        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> scores = ExamAccessTestKit.Rows(
            session,
            $"SELECT Kind,SelName,Score FROM Exam_Score WHERE ExamId={result.ExamId} ORDER BY SelName");

        // 项目行 1 + 选项行 1 + 弃权行 1 + 无效 2 行（默认不计入得票率 → 分值行 + 「选项不计入得票率」行）。
        Assert.Equal(5, scores.Count);
        Assert.Equal(10d, ScoreOf(scores, "德"));
        Assert.Equal(3d, ScoreOf(scores, "优秀"));

        // 弃权默认计入得票率 → 单行，分值取默认 -1。
        Assert.Equal(-1d, ScoreOf(scores, ExamInsertRules.AbstainSelectorName));

        // 无效默认不计入得票率 → 两行：分值 -1 与固定 1 的「不计入」行。
        List<Dictionary<string, object?>> invalid = scores
            .Where(row => ExamAccessTestKit.Text(row, "SelName") == ExamInsertRules.InvalidSelectorName)
            .ToList();
        Assert.Equal(2, invalid.Count);
        Assert.Equal(
            -1d,
            Convert.ToDouble(invalid.Single(row => ExamAccessTestKit.Text(row, "Kind") == ExamInsertRules.SelectorType)["Score"]));
        Assert.Equal(
            1d,
            Convert.ToDouble(
                invalid.Single(
                    row => ExamAccessTestKit.Text(row, "Kind") == ExamInsertRules.SelectorExcludedFromAverageType)["Score"]));

        // 除项目行与「不计入得票率」补行外，其余都应是普通选项行。
        Assert.All(
            scores.Where(row => ExamAccessTestKit.Text(row, "SelName") != "德"
                                && ExamAccessTestKit.Text(row, "Kind") != ExamInsertRules.SelectorExcludedFromAverageType),
            row => Assert.Equal(ExamInsertRules.SelectorType, ExamAccessTestKit.Text(row, "Kind")));
    }

    [AceFact]
    public void Insert_CustomWeights_AreWrittenIntoImplicitRows()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamInsertResult result = _sut.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "自定义比重考评",
            ItemScoreWeight = "7.5",
            AbstainScoreWeight = "4.5",
            AbstainCalcAvg = true,
            InvalidScoreWeight = "2",
            InvalidCalcAvg = false,
            Items =
            [
                // ItemScore 留空 → 项目行用请求级「得分」比重。
                new ExamInsertItem
                {
                    ItemName = "能",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = "甲", ScorePer = "6" }
                    ]
                }
            ]
        });

        Assert.True(result.Success, result.Summary);

        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> scores = ExamAccessTestKit.Rows(
            session,
            $"SELECT Kind,SelName,Score FROM Exam_Score WHERE ExamId={result.ExamId} ORDER BY SelName,Kind");

        Assert.Equal(7.5d, ScoreOf(scores, "能"));
        Assert.Equal(4.5d, ScoreOf(scores, ExamInsertRules.AbstainSelectorName));

        // 无效 CalcAvg=false → 原分值行 + 「选项不计入得票率」行（固定 1）。
        List<Dictionary<string, object?>> invalid = scores
            .Where(row => ExamAccessTestKit.Text(row, "SelName") == ExamInsertRules.InvalidSelectorName)
            .ToList();
        Assert.Equal(2, invalid.Count);
        Assert.Equal(
            2d,
            Convert.ToDouble(invalid.Single(row => ExamAccessTestKit.Text(row, "Kind") == ExamInsertRules.SelectorType)["Score"]));
        Assert.Equal(
            1d,
            Convert.ToDouble(
                invalid.Single(
                    row => ExamAccessTestKit.Text(row, "Kind") == ExamInsertRules.SelectorExcludedFromAverageType)["Score"]));
    }

    [AceFact]
    public void Insert_ExplicitImplicitSelector_IsNotDuplicated()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamInsertResult result = _sut.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "显式无效考评",
            Items =
            [
                new ExamInsertItem
                {
                    ItemName = "德",
                    ItemScore = "10",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = ExamInsertRules.InvalidSelectorName, ScorePer = "0" }
                    ]
                }
            ]
        });

        Assert.True(result.Success, result.Summary);
        Assert.Equal(1, result.ImplicitSelectorCount);

        using var session = fixture.OpenDatabase();
        Assert.Equal(
            1,
            ExamAccessTestKit.Count(
                session,
                $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId={result.ExamId} " +
                $"AND SelName='{ExamInsertRules.InvalidSelectorName}'"));
        Assert.Equal(
            1,
            ExamAccessTestKit.Count(
                session,
                $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId={result.ExamId} " +
                $"AND SelName='{ExamInsertRules.AbstainSelectorName}'"));
    }

    [AceFact]
    public void Insert_ImplicitRows_DoNotTouchBoolItemOrItemOrder()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamInsertResult result = _sut.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "隐式行不占序考评",
            Items =
            [
                new ExamInsertItem
                {
                    ItemName = "德",
                    ItemScore = "10",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = "优秀", ScorePer = "3" },
                        new ExamInsertSelector { SelectorName = "合格", ScorePer = "2" }
                    ]
                }
            ]
        });

        Assert.True(result.Success, result.Summary);
        Assert.Equal(2, result.BoolItemCount);
        Assert.Equal(2, result.ImplicitSelectorCount);

        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> boolItems = ExamAccessTestKit.Rows(
            session,
            $"SELECT SelName,ItemOrder FROM Exam_BoolItem WHERE ExamId={result.ExamId} ORDER BY ItemOrder");
        Assert.Equal([1, 2], boolItems.Select(row => Convert.ToInt32(row["ItemOrder"])));
        Assert.DoesNotContain(
            boolItems,
            row => ExamInsertRules.IsImplicitSelector(ExamAccessTestKit.Text(row, "SelName")));
    }

    [AceFact]
    public void Insert_MissingBoolItemTable_RollbackAlsoDropsImplicitRows()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture(
            new ExamFixtureOptions { IncludeExamBoolItem = false });

        ExamInsertResult result = _sut.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "回滚比重考评",
            Items =
            [
                new ExamInsertItem
                {
                    ItemName = "德",
                    ItemScore = "10",
                    Selectors = [new ExamInsertSelector { SelectorName = "优秀", ScorePer = "3" }]
                }
            ]
        });

        Assert.False(result.Success);
        Assert.Contains("已回滚", result.Summary);

        using var session = fixture.OpenDatabase();
        Assert.Equal(0, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_Score"));
        Assert.Equal(0, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_EssInfo WHERE ExamName='回滚比重考评'"));
    }

    private static double ScoreOf(IReadOnlyList<Dictionary<string, object?>> rows, string selName)
        => Convert.ToDouble(rows.Single(row => ExamAccessTestKit.Text(row, "SelName") == selName)["Score"]);
}
