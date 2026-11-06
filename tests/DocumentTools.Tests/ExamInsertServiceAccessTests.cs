using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// ExamInsertService 对真实 .mdb 的写入：默认规则、分值行、选题行，以及失败时的事务回滚。
/// 期望值全部来自 ExamInsertService.cs 的实现常量（ExamInsertRules / AccessRulers），不另立口径。
/// </summary>
public sealed class ExamInsertServiceAccessTests
{
    private readonly ExamInsertService _sut = new();

    [AceFact]
    public void ListExams_ReturnsSeededExamsOrderedByExamId()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        IReadOnlyList<(int Id, string Name, string TypeMode)> exams = _sut.ListExams(fixture.DatabaseFile);

        Assert.Equal(2, exams.Count);
        Assert.Equal((1, "一级考评", "人员纵排"), exams[0]);
        Assert.Equal((5, "二级考评", "人员横排"), exams[1]);
    }

    [AceFact]
    public void Insert_NewExam_WritesFiveDefaultRulersInOrder()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamInsertResult result = _sut.Insert(BuildRequest(fixture.DatabaseFile, "三级考评", ItemWithSelectors()));

        Assert.True(result.Success, result.Summary);
        Assert.True(result.CreatedExam);
        Assert.Equal(6, result.ExamId);
        Assert.Equal(1, result.ItemCount);
        Assert.Equal(3, result.SelectorCount);
        Assert.Equal(2, result.BoolItemCount);

        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> rulers = ExamAccessTestKit.Rows(
            session, "SELECT RulerKind,RulerPara,RulerOrder FROM Exam_Ruler WHERE ExamId=6 ORDER BY RulerOrder");
        Assert.Equal(
            [
                ("单项不填规则", "自动当作弃权", 2),
                ("单项多填规则", "只记录最前被选项后再交给人工", 3),
                ("多项不填规则", "自动当作弃权", 4),
                ("全部无效规则", "无效票False", 5),
                ("全部弃权规则", "无效票False", 6)
            ],
            rulers.Select(row => (
                ExamAccessTestKit.Text(row, "RulerKind"),
                ExamAccessTestKit.Text(row, "RulerPara"),
                Convert.ToInt32(row["RulerOrder"]))));
    }

    [AceFact]
    public void Insert_NewExam_WritesItemAndSelectorScores()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamInsertResult result = _sut.Insert(BuildRequest(fixture.DatabaseFile, "三级考评", ItemWithSelectors()));
        Assert.True(result.Success, result.Summary);

        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> scores = ExamAccessTestKit.Rows(
            session, "SELECT Kind,ItemName,SelName,Score FROM Exam_Score WHERE ExamId=6 ORDER BY SelName");

        // 项目行 1 + 调用方选项 3 + 按 legacy 自动补写的「弃权」行 1（「无效」调用方已显式给出，不重复）＝ 5。
        Assert.Equal(5, scores.Count);
        Assert.Equal(1, result.ImplicitSelectorCount);

        Dictionary<string, object?> itemRow = Assert.Single(scores, row => ExamAccessTestKit.Text(row, "ItemName") == "德"
                                                                          && ExamAccessTestKit.Text(row, "SelName") == "德");
        Assert.Equal(ExamInsertRules.ItemType, ExamAccessTestKit.Text(itemRow, "Kind"));
        Assert.Equal(10d, Convert.ToDouble(itemRow["Score"]));

        Assert.Equal(3d, ScoreOf(scores, "优秀"));
        Assert.Equal(2d, ScoreOf(scores, "合格"));
        Assert.Equal(0d, ScoreOf(scores, "无效"));

        // 弃权行由请求默认比重填充（legacy InsertExam_SelectorAbstain_ScorePer 默认 -1）。
        Assert.Equal(-1d, ScoreOf(scores, ExamInsertRules.AbstainSelectorName));
        Assert.All(
            scores.Where(row => ExamAccessTestKit.Text(row, "SelName") != "德"),
            row => Assert.Equal(ExamInsertRules.SelectorType, ExamAccessTestKit.Text(row, "Kind")));
    }

    [AceFact]
    public void Insert_NewExam_BoolItemSkipsInvalidAndAbstainSelectors()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamInsertResult result = _sut.Insert(BuildRequest(fixture.DatabaseFile, "三级考评", ItemWithSelectors()));
        Assert.True(result.Success, result.Summary);

        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> boolItems = ExamAccessTestKit.Rows(
            session, "SELECT AtKind,ItemName,SelName,ItemOrder FROM Exam_BoolItem WHERE ExamId=6 ORDER BY ItemOrder");
        Assert.Equal(2, boolItems.Count);
        Assert.Equal("优秀", ExamAccessTestKit.Text(boolItems[0], "SelName"));
        Assert.Equal(1, Convert.ToInt32(boolItems[0]["ItemOrder"]));
        Assert.Equal("合格", ExamAccessTestKit.Text(boolItems[1], "SelName"));
        Assert.Equal(2, Convert.ToInt32(boolItems[1]["ItemOrder"]));
        Assert.All(boolItems, row => Assert.Equal(ExamInsertRules.MeasurementKind, ExamAccessTestKit.Text(row, "AtKind")));
        Assert.All(boolItems, row => Assert.Equal("德", ExamAccessTestKit.Text(row, "ItemName")));
        Assert.DoesNotContain(boolItems, row => ExamInsertRules.IsImplicitSelector(ExamAccessTestKit.Text(row, "SelName")));
    }

    [AceFact]
    public void Insert_CalcAvgFalse_AppendsExcludedSelectorRowWithFixedScore()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        var request = new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "不计平均分考评",
            Items =
            [
                new ExamInsertItem
                {
                    ItemName = "能",
                    ItemScore = "5",
                    Selectors =
                    [
                        new ExamInsertSelector { SelectorName = "甲", ScorePer = "3", CalcAvg = false }
                    ]
                }
            ]
        };

        ExamInsertResult result = _sut.Insert(request);

        Assert.True(result.Success, result.Summary);
        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> scores = ExamAccessTestKit.Rows(
            session,
            $"SELECT Kind,SelName,Score FROM Exam_Score WHERE ExamId={result.ExamId} AND SelName='甲' ORDER BY Kind");
        Assert.Equal(2, scores.Count);
        Assert.Equal(ExamInsertRules.SelectorType, ExamAccessTestKit.Text(scores[0], "Kind"));
        Assert.Equal(3d, Convert.ToDouble(scores[0]["Score"]));
        Assert.Equal(ExamInsertRules.SelectorExcludedFromAverageType, ExamAccessTestKit.Text(scores[1], "Kind"));
        Assert.Equal(1d, Convert.ToDouble(scores[1]["Score"]));
    }

    [AceFact]
    public void Insert_ExistingExamName_AppendsItemsWithoutDuplicatingRulers()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        ExamInsertResult first = _sut.Insert(BuildRequest(fixture.DatabaseFile, "追加考评", ItemWithSelectors()));
        Assert.True(first.Success, first.Summary);

        ExamInsertResult second = _sut.Insert(BuildRequest(fixture.DatabaseFile, "追加考评", ItemWithSelectors()));

        Assert.True(second.Success, second.Summary);
        Assert.False(second.CreatedExam);
        Assert.Equal(first.ExamId, second.ExamId);

        using var session = fixture.OpenDatabase();
        Assert.Equal(1, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_EssInfo WHERE ExamName='追加考评'"));
        Assert.Equal(5, ExamAccessTestKit.Count(session, $"SELECT COUNT(*) FROM Exam_Ruler WHERE ExamId={first.ExamId}"));
        // 第二轮 ItemOrder 从上一轮 MAX(ItemOrder) 继续，不会与第一轮撞号。
        Assert.Equal(
            [1, 2, 3, 4],
            ExamAccessTestKit
                .Rows(session, $"SELECT ItemOrder FROM Exam_BoolItem WHERE ExamId={first.ExamId} ORDER BY ItemOrder")
                .Select(row => Convert.ToInt32(row["ItemOrder"])));
    }

    [AceFact]
    public void Insert_MissingBoolItemTable_RollsBackExamAndScores()
    {
        // 夹具刻意不建 Exam_BoolItem：写入途中必然抛异常，用来验证 BeginTransaction 的整段回滚。
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture(
            new ExamFixtureOptions { IncludeExamBoolItem = false });

        ExamInsertResult result = _sut.Insert(BuildRequest(fixture.DatabaseFile, "回滚考评", ItemWithSelectors()));

        Assert.False(result.Success);
        Assert.Contains("已回滚", result.Summary);

        using var session = fixture.OpenDatabase();
        Assert.Equal(0, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_EssInfo WHERE ExamName='回滚考评'"));
        Assert.Equal(0, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_Ruler"));
        Assert.Equal(0, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_Score"));
        Assert.Equal(2, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_EssInfo"));
    }

    [Fact]
    public void Insert_BlankExamName_Fails()
    {
        ExamInsertResult result = _sut.Insert(new ExamInsertRequest { DatabaseFile = "whatever.mdb", ExamName = " " });

        Assert.False(result.Success);
        Assert.Contains("考评表名称为空", result.Summary);
    }

    [Fact]
    public void Insert_MissingDatabaseFile_Fails()
    {
        string missing = Path.Combine(Path.GetTempPath(), "dt-missing-" + Guid.NewGuid().ToString("n") + ".mdb");

        ExamInsertResult result = _sut.Insert(new ExamInsertRequest { DatabaseFile = missing, ExamName = "任意" });

        Assert.False(result.Success);
        Assert.Contains("数据库文件不存在", result.Summary);
    }

    [Fact]
    public void Insert_SqliteExtension_FailsWithGuidance()
    {
        string path = Path.Combine(Path.GetTempPath(), "dt-probe-" + Guid.NewGuid().ToString("n") + ".db");
        File.WriteAllBytes(path, []);
        try
        {
            ExamInsertResult result = _sut.Insert(new ExamInsertRequest { DatabaseFile = path, ExamName = "任意" });

            Assert.False(result.Success);
            Assert.Contains(".mdb/.accdb", result.Summary);
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static ExamInsertRequest BuildRequest(string databaseFile, string examName, ExamInsertItem item)
        => new()
        {
            DatabaseFile = databaseFile,
            ExamName = examName,
            Items = [item]
        };

    private static ExamInsertItem ItemWithSelectors()
        => new()
        {
            ItemName = "德",
            ItemScore = "10",
            Selectors =
            [
                new ExamInsertSelector { SelectorName = "优秀", ScorePer = "3" },
                new ExamInsertSelector { SelectorName = "合格", ScorePer = "2" },
                new ExamInsertSelector { SelectorName = ExamInsertRules.InvalidSelectorName }
            ]
        };

    private static double ScoreOf(IEnumerable<Dictionary<string, object?>> rows, string selectorName)
        => Convert.ToDouble(
            Assert.Single(rows, row => ExamAccessTestKit.Text(row, "SelName") == selectorName)["Score"]);
}
