using System.Text.RegularExpressions;
using NPinyin;

namespace DocumentTools.Core.Services.Text;

/// <summary>
/// legacy <c>GetChinsesHanPinYin_TypeBox</c> 的 6 个模式（<c>BodyForm.cs:1555-1563</c>），
/// 索引 0..5 与 legacy 下拉顺序一一对应。
/// </summary>
public enum NamePinyinMode
{
    /// <summary>大写拼音首字母（legacy 索引 0 <c>Pinyin.GetInitials().ToUpper()</c>）。</summary>
    InitialsUpper = 0,

    /// <summary>小写拼音首字母（legacy 索引 1 <c>Pinyin.GetInitials().ToLower()</c>）。</summary>
    InitialsLower = 1,

    /// <summary>大写拼音（带空格，legacy 索引 2）。</summary>
    PinyinUpperSpaced = 2,

    /// <summary>大写拼音（不带空格，legacy 索引 3）。</summary>
    PinyinUpperCompact = 3,

    /// <summary>小写拼音（带空格，legacy 索引 4）。</summary>
    PinyinLowerSpaced = 4,

    /// <summary>小写拼音（不带空格，legacy 索引 5）。</summary>
    PinyinLowerCompact = 5
}

/// <summary>
/// 姓名列表清洗/转换开关，语义逐项对齐 legacy 姓名页控件：
/// RemoveNumber=<c>RemoveNumberCheck</c>、RemoveSquare=<c>RemoveSquareCheck</c>、
/// DoubleAlign=<c>DoubleTextAlignCheck</c>、ReplaceEnabled=<c>NameReplaceWord</c>、
/// ReplaceSearch/ReplaceTarget=<c>NameReplaceWord_Search/Target</c>、ReplaceRegex=<c>NameReplaceRegexMode</c>、
/// PinyinEnabled=<c>GetChinsesHanPinYin</c>、PinyinMode=<c>GetChinsesHanPinYin_TypeBox</c>、
/// Distinct=<c>DoubleTextClear</c>。
/// </summary>
public sealed record NameVerticalOptions
{
    /// <summary>文本去重（legacy <c>DoubleTextClear</c>，在清洗/对齐/拼音之后执行）。</summary>
    public bool Distinct { get; init; }

    /// <summary>移除数字（legacy 删除 <c>[0-9]</c>）。</summary>
    public bool RemoveNumber { get; init; }

    /// <summary>移除括号（legacy 删除 <c>(..)</c> / <c>（..）</c> / 空括号）。</summary>
    public bool RemoveSquare { get; init; }

    /// <summary>双字对齐：两字姓名在第 1 个字符后插入两个空格（legacy 默认勾选）。</summary>
    public bool DoubleAlign { get; init; } = true;

    /// <summary>启用「替换文字」。</summary>
    public bool ReplaceEnabled { get; init; }

    /// <summary>替换查找串（字面或正则，取决于 <see cref="ReplaceRegex"/>）。</summary>
    public string ReplaceSearch { get; init; } = string.Empty;

    /// <summary>替换目标串。</summary>
    public string ReplaceTarget { get; init; } = string.Empty;

    /// <summary>替换使用正则（legacy <c>NameReplaceMode</c>，默认关闭）。</summary>
    public bool ReplaceRegex { get; init; }

    /// <summary>汉字转拼音（legacy <c>GetChinsesHanPinYin</c>，默认不勾）。</summary>
    public bool PinyinEnabled { get; init; }

    /// <summary>拼音模式，仅在 <see cref="PinyinEnabled"/> 为真时生效。</summary>
    public NamePinyinMode PinyinMode { get; init; } = NamePinyinMode.InitialsUpper;
}

/// <summary>处理结果：每行一个姓名（已清洗/对齐/转拼音），并给出重复行号供 UI 高亮。</summary>
public sealed record NameVerticalResult(IReadOnlyList<string> Lines, IReadOnlyList<int> DuplicateLineIndices)
{
    /// <summary>空结果。</summary>
    public static NameVerticalResult Empty { get; } = new(Array.Empty<string>(), Array.Empty<int>());

    /// <summary>处理后姓名个数（legacy「处理后姓名有{N}个」）。</summary>
    public int Count => Lines.Count;

    /// <summary>逐字符竖排：每个字占一行，姓名之间不插空行。</summary>
    public string ToVerticalText()
        => string.Join(
            Environment.NewLine,
            Lines.Select(line => string.Join(Environment.NewLine, line.ToCharArray())));
}

/// <summary>Turns horizontal name lists into vertical column text.</summary>
public interface INameVerticalService
{
    /// <summary>兼容旧调用方的便捷重载：按换行/逗号/分号/制表符/空格切分后逐字竖排。</summary>
    string ToVertical(string? source, bool distinct = true);

    /// <summary>
    /// legacy <c>Util.AddText2List</c> + <c>ConvertTextButton_Click</c> 全量管线：
    /// 表格(<c>\t</c>)/换行两种切分 → 去空白(表格分支额外去 <c>○□£</c>) → 去数字 → 去括号 →
    /// 替换（字面/正则）→ 双字对齐 → 拼音 → 去重。
    /// </summary>
    NameVerticalResult Process(string? source, NameVerticalOptions options);
}

/// <summary>legacy 姓名竖排文本管线的 Core 实现。</summary>
public sealed class NameVerticalService : INameVerticalService
{
    /// <summary>表格分支逐段清洗：空白 + legacy <c>AllSpaceChar</c> 里的 ○ □ £。</summary>
    private static readonly string[] TableFilters = ["\\s", "○", "□", "£"];

    /// <summary>换行分支逐段清洗：仅空白（legacy 该分支不删 ○ □ £）。</summary>
    private static readonly string[] LineFilters = ["\\s"];

    private const string DigitsPattern = "[0-9]";

    /// <summary>legacy 括号正则：半角/全角成对括号与空括号。</summary>
    private const string SquarePattern = "\\(.+?\\)|（.+?）|\\(\\)|（）";

    private static readonly Regex WhitespaceRun = new("\\s+", RegexOptions.Compiled);

    private static readonly Regex AnySeparator = new("[,，;；\t ]", RegexOptions.Compiled);

    public string ToVertical(string? source, bool distinct = true)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return string.Empty;
        }

        // 沿用迁移后的便捷语义：逗号/分号/制表符/空格也算分隔符（legacy 只认换行与表格）。
        string normalized = AnySeparator.Replace(source.Replace("\r\n", "\n").Replace('\r', '\n'), "\n");
        NameVerticalResult result = Process(
            normalized,
            new NameVerticalOptions { Distinct = distinct, DoubleAlign = false });
        return result.ToVerticalText();
    }

    public NameVerticalResult Process(string? source, NameVerticalOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrEmpty(source))
        {
            return NameVerticalResult.Empty;
        }

        List<string> names = SplitNames(source, options);

        if (options.DoubleAlign)
        {
            names = names.ConvertAll(AlignDouble);
        }

        if (options.PinyinEnabled)
        {
            names = names.ConvertAll(name => ToPinyin(name, options.PinyinMode));
        }

        // legacy 的 DoubleTextClear 是处理链最后一环（BodyForm.cs:2358-2361）。
        if (options.Distinct)
        {
            names = [.. names.Distinct(StringComparer.Ordinal)];
        }

        return new NameVerticalResult(names, FindDuplicateLines(names));
    }

    private static List<string> SplitNames(string source, NameVerticalOptions options)
    {
        bool fromTable = source.Contains('\t');

        // 行尾统一成 \n，再按 \n 切分。
        string normalized = source.Replace("\r\n", "\n").Replace('\r', '\n');

        // 表格分支：\t 也是单元格分隔，一律当行分隔处理。
        // legacy（Util.cs:399-402）是「先删全部 \n，再把 \t 换成 \n」，那会把上一行末格
        // 与下一行首格并成一个名字（"李四"+"王五"→"李四王五"）。legacy 此处会并名，
        // 属于缺陷，本实现有意修正为保留行边界，使「从 Word 表格复制整列」得到每格一名。
        if (fromTable)
        {
            normalized = normalized.Replace('\t', '\n');
        }

        string[] filters = fromTable ? TableFilters : LineFilters;
        var names = new List<string>();

        foreach (string segment in normalized.Split('\n'))
        {
            string text = RemovePatterns(filters, segment);

            if (options.RemoveNumber)
            {
                text = RemovePatterns([DigitsPattern], text);
            }

            if (options.RemoveSquare)
            {
                text = RemovePatterns([SquarePattern], text);
            }

            if (options.ReplaceEnabled && !string.IsNullOrEmpty(options.ReplaceSearch))
            {
                text = options.ReplaceRegex
                    ? Regex.Replace(text, options.ReplaceSearch, options.ReplaceTarget, RegexOptions.Multiline)
                    : text.Replace(options.ReplaceSearch, options.ReplaceTarget);
            }

            if (text.Length > 0)
            {
                names.Add(text);
            }
        }

        return names;
    }

    /// <summary>legacy <c>RemoveSpeciCharacter</c>：逐个正则模式清空匹配内容。</summary>
    private static string RemovePatterns(string[] patterns, string text)
    {
        foreach (string pattern in patterns)
        {
            if (text.Length == 0)
            {
                return text;
            }

            if (Regex.IsMatch(text, pattern, RegexOptions.Multiline))
            {
                text = Regex.Replace(text, pattern, string.Empty, RegexOptions.Multiline);
            }
        }

        return text;
    }

    /// <summary>legacy 双字对齐：两字姓名在索引 1 处插入两个空格。</summary>
    private static string AlignDouble(string name)
        => name.Length == 2 ? name.Insert(1, "  ") : name;

    private static string ToPinyin(string name, NamePinyinMode mode)
        => mode switch
        {
            NamePinyinMode.InitialsUpper => Pinyin.GetInitials(name).ToUpper(),
            NamePinyinMode.InitialsLower => Pinyin.GetInitials(name).ToLower(),
            NamePinyinMode.PinyinUpperSpaced => Pinyin.GetPinyin(name).ToUpper(),
            NamePinyinMode.PinyinUpperCompact => WhitespaceRun.Replace(Pinyin.GetPinyin(name).ToUpper(), string.Empty),
            NamePinyinMode.PinyinLowerSpaced => Pinyin.GetPinyin(name).ToLower(),
            NamePinyinMode.PinyinLowerCompact => WhitespaceRun.Replace(Pinyin.GetPinyin(name).ToLower(), string.Empty),
            _ => name
        };

    /// <summary>legacy MarkDupName：返回所有参与重复的行号（至少出现两次的行）。</summary>
    private static List<int> FindDuplicateLines(IReadOnlyList<string> lines)
    {
        var indices = new SortedSet<int>();

        for (int i = 0; i < lines.Count; i++)
        {
            for (int j = i + 1; j < lines.Count; j++)
            {
                if (string.Equals(lines[i], lines[j], StringComparison.Ordinal))
                {
                    indices.Add(i);
                    indices.Add(j);
                }
            }
        }

        return [.. indices];
    }
}
