using DocumentTools.Core.Services.Exam;

namespace DocumentTools.Tests;

/// <summary>
/// 分值解析的纯函数测试（不需要 ACE，因此不在 <c>[AceFact]</c> 之列）。
/// 这里锁的是「非空且不是数字必须报错」这条：曾经 TryParse 失败直接返回 0，
/// 用户填的「10分」「abc」会静默变成 0 分写进 Exam_Score，错误要等成绩汇总才暴露。
/// </summary>
public sealed class ExamInsertScoreParsingTests
{
    [Theory]
    [InlineData("", 0d)]
    [InlineData("   ", 0d)]
    [InlineData("0", 0d)]
    [InlineData("1", 1d)]
    [InlineData("1.5", 1.5d)]
    [InlineData(" 3 ", 3d)]
    [InlineData("-1", -1d)]
    public void ParseScore_AcceptsNumbersAndBlank(string raw, double expected)
    {
        Assert.Equal(expected, ExamInsertRules.ParseScore(raw));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("10分")]
    [InlineData("1,5")]
    public void ParseScore_RejectsNonNumeric_WithTheOriginalValueInMessage(string raw)
    {
        var error = Assert.Throws<ArgumentException>(() => ExamInsertRules.ParseScore(raw));

        Assert.Contains("分值必须是数字", error.Message, StringComparison.Ordinal);
        Assert.Contains(raw, error.Message, StringComparison.Ordinal);
    }
}
