using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// 考评录入的「批量」模板展开（legacy BodyForm.cs:8895-8982 批量项目、:9609-9721 批量选项、
/// :9501-9607 按名称批量选项、Util.cs:1900-1938 中文序号）。
/// 纯逻辑用例不依赖 ACE；最后一个用例验证展开结果能被真实写库。
/// </summary>
public sealed class ExamBatchInsertTemplatesTests
{
    private readonly ExamInsertService _sut = new();

    [Theory]
    [InlineData(0, "零")]
    [InlineData(1, "一")]
    [InlineData(10, "十")]
    [InlineData(11, "十一")]
    [InlineData(20, "二十")]
    [InlineData(100, "一百")]
    [InlineData(110, "一百一十")]
    [InlineData(999, "九百九十九")]
    public void ConvertNumberToChinese_MatchesLegacyReading(int value, string expected)
    {
        Assert.Equal(expected, ExamBatchInsertTemplates.ConvertNumberToChinese(value));
    }

    [Fact]
    public void ConvertNumberToChinese_BeyondLegacyRange_FallsBackToArabicDigits()
    {
        // legacy 的单位表只到「百」，1000 会数组越界；这里退回阿拉伯数字而不是让批量生成崩掉。
        Assert.Equal("1000", ExamBatchInsertTemplates.ConvertNumberToChinese(1000));
        Assert.Equal("-1", ExamBatchInsertTemplates.ConvertNumberToChinese(-1));
    }

    [Fact]
    public void ExpandItems_ReplacesPlaceholderWithChineseNumbersAndSharesScore()
    {
        IReadOnlyList<ExamInsertItem> items =
            ExamBatchInsertTemplates.ExpandItems("第X项", 3, "7.5");

        Assert.Equal(3, items.Count);
        Assert.Equal(
            new[] { "第一项", "第二项", "第三项" },
            items.Select(item => item.ItemName).ToArray());
        Assert.All(items, item => Assert.Equal("7.5", item.ItemScore));
        Assert.All(items, item => Assert.Equal(ExamInsertRules.ItemType, item.Type));
        Assert.All(items, item => Assert.Empty(item.Selectors));
    }

    [Fact]
    public void ExpandItems_ArabicNumbering_WhenChineseSwitchedOff()
    {
        IReadOnlyList<ExamInsertItem> items =
            ExamBatchInsertTemplates.ExpandItems("项目X", 2, useChineseNumbering: false);

        Assert.Equal(
            new[] { "项目1", "项目2" },
            items.Select(item => item.ItemName).ToArray());
    }

    [Fact]
    public void ExpandItems_RejectsTemplateWithoutPlaceholder()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ExamBatchInsertTemplates.ExpandItems("没有占位符", 2));

        Assert.Contains("必须包含占位符 X", ex.Message);
    }

    [Fact]
    public void ExpandItems_RejectsNonPositiveCount()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => ExamBatchInsertTemplates.ExpandItems("第X项", 0));

        Assert.Contains("批量项目数量必须大于 0", ex.Message);
    }

    [Fact]
    public void ExpandSelectors_UsesSharedScoreAndCalcAvgFlag()
    {
        IReadOnlyList<ExamInsertSelector> selectors =
            ExamBatchInsertTemplates.ExpandSelectors("选项X", 2, "30", calcAvg: false);

        Assert.Equal(
            new[] { "选项1", "选项2" },
            selectors.Select(selector => selector.SelectorName).ToArray());
        Assert.All(selectors, selector => Assert.Equal("30", selector.ScorePer));
        Assert.All(selectors, selector => Assert.False(selector.CalcAvg));
        Assert.All(selectors, selector => Assert.Equal(ExamInsertRules.SelectorType, selector.Type));
    }

    [Fact]
    public void FromPreset_ReturnsLegacyBuiltInNames()
    {
        IReadOnlyList<ExamInsertSelector> selectors = ExamBatchInsertTemplates.FromPreset(6);

        Assert.Equal(
            new[] { "优秀", "合格", "基本合格", "不合格" },
            selectors.Select(selector => selector.SelectorName).ToArray());
        Assert.All(selectors, selector => Assert.True(selector.CalcAvg));
    }

    [Fact]
    public void FromPreset_AllElevenGroupsHaveFourNames()
    {
        Assert.Equal(11, ExamBatchInsertTemplates.CustomSelectorPresets.Count);
        Assert.All(ExamBatchInsertTemplates.CustomSelectorPresets, preset => Assert.Equal(4, preset.Count));

        for (int preset = 1; preset <= 11; preset++)
        {
            Assert.Equal(4, ExamBatchInsertTemplates.FromPreset(preset).Count);
        }
    }

    [Fact]
    public void FromPreset_OutOfRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ExamBatchInsertTemplates.FromPreset(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExamBatchInsertTemplates.FromPreset(12));
    }

    [Fact]
    public void FromNames_AppendsSuffixToEveryName()
    {
        IReadOnlyList<ExamInsertSelector> selectors =
            ExamBatchInsertTemplates.FromNames(new[] { "甲", "乙" }, "1", suffix: "项");

        Assert.Equal(
            new[] { "甲项", "乙项" },
            selectors.Select(selector => selector.SelectorName).ToArray());
    }

    [Fact]
    public void FromNames_RejectsAllBlankNames()
    {
        Assert.Throws<ArgumentException>(() => ExamBatchInsertTemplates.FromNames(new[] { " ", "" }));
    }

    [Fact]
    public void ParseClipboardNames_SplitsOnTabCrLfLikeLegacy()
    {
        IReadOnlyList<string> names = ExamBatchInsertTemplates.ParseClipboardNames("好\t较好\r\n一般\n\n 差 \n");

        Assert.Equal(new[] { "好", "较好", "一般", "差" }, names.ToArray());
    }

    [Fact]
    public void ParseClipboardNames_EmptyText_ReturnsEmpty()
    {
        Assert.Empty(ExamBatchInsertTemplates.ParseClipboardNames(null));
        Assert.Empty(ExamBatchInsertTemplates.ParseClipboardNames("  \r\n "));
    }

    [Fact]
    public void FromClipboard_EmptyClipboard_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => ExamBatchInsertTemplates.FromClipboard("  "));

        Assert.Contains("剪贴板里没有可用的选项名称", ex.Message);
    }

    [AceFact]
    public void ExpandedTemplates_WriteThroughInsertService()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        List<ExamInsertItem> items = ExamBatchInsertTemplates.ExpandItems("评价X", 2, "9").ToList();
        items[0].Selectors.AddRange(ExamBatchInsertTemplates.FromPreset(6, "30"));
        items[1].Selectors.AddRange(ExamBatchInsertTemplates.FromClipboard("甲\t乙", "10"));

        ExamInsertResult result = _sut.Insert(new ExamInsertRequest
        {
            DatabaseFile = fixture.DatabaseFile,
            ExamName = "批量考评",
            Items = items
        });

        Assert.True(result.Success, result.Summary);
        Assert.True(result.CreatedExam);
        Assert.Equal(6, result.ExamId);

        using var session = fixture.OpenDatabase();
        // 新考评表：五条默认规则 + 2 个项目行 + 6 个选项行 + 每项隐式「弃权 1 + 无效 2」6 行。
        Assert.Equal(5, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_Ruler WHERE ExamId=6"));
        Assert.Equal(14, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Exam_Score WHERE ExamId=6"));

        var itemRows = ExamAccessTestKit.Rows(
            session,
            "SELECT ItemName FROM Exam_Score WHERE ExamId=6 AND Kind='项目'");
        Assert.Equal(
            new[] { "评价一", "评价二" },
            itemRows.Select(row => ExamAccessTestKit.Text(row, "ItemName")).ToArray());
    }
}
