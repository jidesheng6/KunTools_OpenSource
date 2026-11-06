using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Services.Text;

namespace DocumentTools.Tests;

/// <summary>
/// legacy 姓名页管线（Util.cs:384-505 AddText2List + BodyForm.cs:2225-2361 ConvertTextButton_Click）的逐项对齐。
/// </summary>
public class NameVerticalServiceTests
{
    private static readonly string Nl = Environment.NewLine;

    private readonly NameVerticalService _service = new();

    /// <summary>只清洗、不对齐：DoubleAlign 默认 true（legacy 勾选），逐项断言时先关掉。</summary>
    private static NameVerticalOptions Raw(bool distinct = false)
        => new() { DoubleAlign = false, Distinct = distinct };

    [Fact]
    public void TableBranch_SplitsOnTab_AndDropsNewlines()
    {
        // 含 \t 时按单元格切分，每个单元格一名；行边界保留（\r\n 不参与合并）。
        NameVerticalResult result = _service.Process("张三\t李四\n王五\t赵六", Raw());

        Assert.Equal(new[] { "张三", "李四", "王五", "赵六" }, result.Lines);
    }

    [Fact]
    public void TableBranch_RealWordCopy_OneCellPerLine()
    {
        // 从 Word 表格复制整列：单元格之间只有 \r\n，不含 \t → 走换行分支，逐行独立。
        NameVerticalResult result = _service.Process("张三\r\n李四\r\n王五", Raw());

        Assert.Equal(new[] { "张三", "李四", "王五" }, result.Lines);
    }

    [Fact]
    public void TableBranch_WordCopyWithTabs_KeepsEveryCell()
    {
        // 从 Word 表格复制二维区域：单元格 \t、行尾 \r\n → 每格一名，不跨行合并。
        NameVerticalResult result = _service.Process("张三\t组长\r\n李四\t组员", Raw());

        Assert.Equal(new[] { "张三", "组长", "李四", "组员" }, result.Lines);
    }

    [Fact]
    public void TableBranch_StripsCircleSquareAndPound()
    {
        NameVerticalResult result = _service.Process("张三○\t李四□\t王五£", Raw());

        Assert.Equal(new[] { "张三", "李四", "王五" }, result.Lines);
    }

    [Fact]
    public void LineBranch_KeepsCircleSquareAndPound()
    {
        // 换行分支 legacy 只删 \s，不删 ○□£。
        NameVerticalResult result = _service.Process("张三○\n李四", Raw());

        Assert.Equal(new[] { "张三○", "李四" }, result.Lines);
    }

    [Fact]
    public void RemoveNumber_DropsAsciiDigits()
    {
        NameVerticalResult result = _service.Process(
            "1张三\n李四2\n王五",
            new NameVerticalOptions { DoubleAlign = false, RemoveNumber = true });

        Assert.Equal(new[] { "张三", "李四", "王五" }, result.Lines);
    }

    [Fact]
    public void RemoveSquare_DropsHalfWidthFullWidthAndEmptyParens()
    {
        NameVerticalResult result = _service.Process(
            "张三(组长)\n李四（副组长）\n王五()\n赵六（）",
            new NameVerticalOptions { DoubleAlign = false, RemoveSquare = true });

        Assert.Equal(new[] { "张三", "李四", "王五", "赵六" }, result.Lines);
    }

    [Fact]
    public void Replace_LiteralMode_DoesNotTreatSearchAsRegex()
    {
        NameVerticalResult result = _service.Process(
            "张三\n张.三",
            new NameVerticalOptions
            {
                DoubleAlign = false,
                ReplaceEnabled = true,
                ReplaceSearch = "张.",
                ReplaceTarget = "王五"
            });

        Assert.Equal(new[] { "张三", "王五三" }, result.Lines);
    }

    [Fact]
    public void Replace_RegexMode_TreatsSearchAsRegex()
    {
        NameVerticalResult result = _service.Process(
            "张三\n张.三",
            new NameVerticalOptions
            {
                DoubleAlign = false,
                ReplaceEnabled = true,
                ReplaceSearch = "张.",
                ReplaceTarget = "王五",
                ReplaceRegex = true
            });

        Assert.Equal(new[] { "王五", "王五三" }, result.Lines);    }

    [Fact]
    public void Replace_IgnoredWhenSearchEmpty()
    {
        NameVerticalResult result = _service.Process(
            "张三",
            new NameVerticalOptions { DoubleAlign = false, ReplaceEnabled = true, ReplaceTarget = "李四" });

        Assert.Equal(new[] { "张三" }, result.Lines);
    }

    [Fact]
    public void DoubleAlign_InsertsTwoSpacesInTwoCharNamesOnly()
    {
        NameVerticalResult result = _service.Process(
            "张三\n王小明\n李",
            new NameVerticalOptions { DoubleAlign = true });

        Assert.Equal(new[] { "张  三", "王小明", "李" }, result.Lines);
        Assert.Equal(2, result.Lines[0].Count(c => c == ' '));
    }

    [Theory]
    [InlineData(NamePinyinMode.InitialsUpper, "ZS")]
    [InlineData(NamePinyinMode.InitialsLower, "zs")]
    [InlineData(NamePinyinMode.PinyinUpperSpaced, "ZHANG SAN")]
    [InlineData(NamePinyinMode.PinyinUpperCompact, "ZHANGSAN")]
    [InlineData(NamePinyinMode.PinyinLowerSpaced, "zhang san")]
    [InlineData(NamePinyinMode.PinyinLowerCompact, "zhangsan")]
    public void PinyinModes_MatchLegacyIndexOrder(NamePinyinMode mode, string expected)
    {
        NameVerticalResult result = _service.Process(
            "张三",
            new NameVerticalOptions { PinyinEnabled = true, PinyinMode = mode, DoubleAlign = false });

        Assert.Equal(new[] { expected }, result.Lines);
    }

    [Theory]
    [InlineData(NamePinyinMode.InitialsUpper, "WXM")]
    [InlineData(NamePinyinMode.PinyinUpperCompact, "WANGXIAOMING")]
    [InlineData(NamePinyinMode.PinyinLowerSpaced, "wang xiao ming")]
    public void PinyinModes_ThreeCharName(NamePinyinMode mode, string expected)
    {
        NameVerticalResult result = _service.Process(
            "王小明",
            new NameVerticalOptions { PinyinEnabled = true, PinyinMode = mode, DoubleAlign = false });

        Assert.Equal(new[] { expected }, result.Lines);
    }

    [Fact]
    public void Pinyin_RunsAfterDoubleAlign_LikeLegacy()
    {
        // legacy 先双字对齐再转拼音 → 两字姓名中间的两个空格保留在首字母里。
        NameVerticalResult result = _service.Process(
            "张三",
            new NameVerticalOptions { PinyinEnabled = true, PinyinMode = NamePinyinMode.InitialsUpper });

        Assert.Equal(new[] { "Z  S" }, result.Lines);
    }

    [Fact]
    public void Distinct_RemovesRepeatedLines_AndRunsLast()
    {
        NameVerticalResult withDistinct = _service.Process(
            "张三\n张三\n李四",
            new NameVerticalOptions { DoubleAlign = false, Distinct = true });

        Assert.Equal(new[] { "张三", "李四" }, withDistinct.Lines);
        Assert.Empty(withDistinct.DuplicateLineIndices);

        NameVerticalResult withoutDistinct = _service.Process(
            "张三\n张三\n李四",
            new NameVerticalOptions { DoubleAlign = false, Distinct = false });

        Assert.Equal(new[] { "张三", "张三", "李四" }, withoutDistinct.Lines);
    }

    [Fact]
    public void DuplicateLineIndices_LikeLegacyMarkDupName()
    {
        NameVerticalResult result = _service.Process(
            "张三\n李四\n张三\n王五\n李四",
            new NameVerticalOptions { DoubleAlign = false, Distinct = false });

        Assert.Equal(new[] { 0, 1, 2, 4 }, result.DuplicateLineIndices);
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public void Process_DropsEmptySegments()
    {
        NameVerticalResult result = _service.Process("张三\n\n   \n李四", Raw());

        Assert.Equal(new[] { "张三", "李四" }, result.Lines);
    }

    [Fact]
    public void Process_ReturnsEmptyForBlankSource()
    {
        Assert.Empty(_service.Process(null, Raw()).Lines);
        Assert.Empty(_service.Process(string.Empty, Raw()).Lines);
    }

    [Fact]
    public void ToVerticalText_StacksCharsWithoutBlankLineBetweenNames()
    {
        var result = new NameVerticalResult(["张三", "李四"], []);

        Assert.Equal($"张{Nl}三{Nl}李{Nl}四", result.ToVerticalText());
    }

    [Fact]
    public void ToVertical_KeepsLegacyFreeConvenienceContract()
    {
        // 旧重载仍按 换行/逗号/分号/制表符/空格 切分并去重。
        Assert.Equal($"张{Nl}三{Nl}李{Nl}四", _service.ToVertical("张三 李四", distinct: true));
        Assert.Equal($"张{Nl}三{Nl}张{Nl}三", _service.ToVertical("张三,张三", distinct: false));
        Assert.Equal(string.Empty, _service.ToVertical("   "));
    }

    [Fact]
    public void Schema_ExposesLegacyNameVerticalKeys()
    {
        var fields = FeatureOptionSchemas.GetSections("name.vertical")
            .SelectMany(section => section.Fields)
            .ToDictionary(field => field.Key);

        Assert.Equal(OptionFieldKind.Check, fields["removeNumber"].Kind);
        Assert.Equal(OptionFieldKind.Check, fields["removeSquare"].Kind);
        Assert.Equal(OptionFieldKind.Check, fields["distinct"].Kind);
        Assert.Equal(OptionFieldKind.Check, fields["markDuplicate"].Kind);
        Assert.Equal("true", fields["doubleAlign"].DefaultValue);
        Assert.Equal(OptionFieldKind.Check, fields["replaceEnabled"].Kind);
        Assert.Equal(OptionFieldKind.Text, fields["replaceSearch"].Kind);
        Assert.Equal(OptionFieldKind.Text, fields["replaceTarget"].Kind);
        Assert.Equal(OptionFieldKind.Check, fields["replaceRegex"].Kind);
        Assert.Equal(OptionFieldKind.Check, fields["pinyinEnabled"].Kind);

        OptionField pinyin = fields["pinyinMode"];
        Assert.Equal(OptionFieldKind.Choice, pinyin.Kind);
        Assert.Equal(
            [
                "InitialsUpper",
                "InitialsLower",
                "PinyinUpperSpaced",
                "PinyinUpperCompact",
                "PinyinLowerSpaced",
                "PinyinLowerCompact"
            ],
            pinyin.Choices!.Select(choice => choice.Value));
        Assert.Equal("InitialsUpper", pinyin.DefaultValue);
        Assert.Equal("大写拼音首字母", pinyin.Choices![0].Label);
        Assert.Equal("小写拼音（不带空格）", pinyin.Choices[5].Label);
    }
}
