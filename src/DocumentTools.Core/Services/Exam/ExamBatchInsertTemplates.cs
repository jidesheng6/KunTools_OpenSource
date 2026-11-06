using System.Globalization;

namespace DocumentTools.Core.Services.Exam;

/// <summary>
/// 考评录入页的「批量」能力：把一份带占位符的名称模板展开成多条项目 / 选项。
/// <para>
/// legacy 的批量完全发生在界面层（BodyForm.cs:8895-8982 批量项目、:9609-9721 批量选项、
/// :9501-9607 按名称批量选项），落库时仍是一条条 INSERT，所以这里只补「模板 → 行集合」这一段纯逻辑，
/// 真正的写库仍由 <see cref="IExamInsertService"/> 负责。
/// </para>
/// </summary>
public static class ExamBatchInsertTemplates
{
    /// <summary>名称模板里的序号占位符；legacy 用正则 <c>X</c> 判断模板是否可用。</summary>
    public const string Placeholder = "X";

    /// <summary>中文序号只在 0..999 有定义（legacy Util.ConvertNumberToChinese 的单位表到「百」为止）。</summary>
    public const int MaxChineseNumber = 999;

    /// <summary>剪贴板里一行一个选项名，legacy 同时按制表符/回车/换行切分（BodyForm.cs:9532-9538）。</summary>
    private static readonly char[] ClipboardSeparators = ['\t', '\r', '\n'];

    /// <summary>
    /// legacy 的 11 组内置选项名（InsertExamBacthCustomSelectors.cs，菜单见 BodyForm.cs:10139-10201）。
    /// 序号即 legacy 字段名 <c>CustomSelectors_1</c> … <c>CustomSelectors_11</c> 的后缀。
    /// </summary>
    public static IReadOnlyList<IReadOnlyList<string>> CustomSelectorPresets { get; } =
    [
        ["好", "较好", "一般", "差"],
        ["好", "较好", "一般", "较差"],
        ["优秀", "良好", "一般", "差"],
        ["优秀", "良好", "一般", "较差"],
        ["优秀", "称职", "基本称职", "不称职"],
        ["优秀", "合格", "基本合格", "不合格"],
        ["认同", "基本认同", "不认同", "不了解"],
        ["好", "一般", "不好", "不了解"],
        ["满意", "基本满意", "较满意", "不满意"],
        ["A", "B", "C", "D"],
        ["a", "b", "c", "d"]
    ];

    /// <summary>
    /// 按模板展开多条项目（legacy InsertExam_BacthAddItem_Click）。
    /// 与 legacy 一致：<paramref name="nameTemplate"/> 必须含 <see cref="Placeholder"/>，所有项目共用同一个分值。
    /// </summary>
    public static IReadOnlyList<ExamInsertItem> ExpandItems(
        string nameTemplate,
        int count,
        string score = "0",
        bool useChineseNumbering = true)
    {
        ValidateTemplate(nameTemplate, "项目名称模板必须包含占位符 X，例如「第X项」。");
        if (count <= 0)
        {
            throw new ArgumentException("批量项目数量必须大于 0。", nameof(count));
        }

        var items = new List<ExamInsertItem>(count);
        for (int index = 1; index <= count; index++)
        {
            items.Add(new ExamInsertItem
            {
                ItemName = ExpandName(nameTemplate, index, useChineseNumbering),
                // legacy 侧 InsertExam_ExamScore.Value = InsertExam_BacthScoreSet.Value，即整批同一个分值。
                ItemScore = score,
                Type = ExamInsertRules.ItemType
            });
        }

        return items;
    }

    /// <summary>
    /// 按模板展开多个选项（legacy InsertExam_BatchSelector_Click）。legacy 侧默认勾选「计入平均」。
    /// </summary>
    public static IReadOnlyList<ExamInsertSelector> ExpandSelectors(
        string nameTemplate,
        int count,
        string scorePer = "0",
        bool calcAvg = true,
        bool useChineseNumbering = false)
    {
        ValidateTemplate(nameTemplate, "选项名称模板必须包含占位符 X，例如「选项X」。");
        if (count <= 0)
        {
            throw new ArgumentException("批量选项数量必须大于 0。", nameof(count));
        }

        var names = new List<string>(count);
        for (int index = 1; index <= count; index++)
        {
            names.Add(ExpandName(nameTemplate, index, useChineseNumbering));
        }

        return FromNames(names, scorePer, calcAvg);
    }

    /// <summary>用 11 组内置选项名之一展开（<paramref name="presetNumber"/> 为 1..11）。</summary>
    public static IReadOnlyList<ExamInsertSelector> FromPreset(
        int presetNumber,
        string scorePer = "0",
        bool calcAvg = true,
        string suffix = "")
    {
        if (presetNumber < 1 || presetNumber > CustomSelectorPresets.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(presetNumber),
                presetNumber,
                $"内置选项名只有 1..{CustomSelectorPresets.Count} 组。");
        }

        return FromNames(CustomSelectorPresets[presetNumber - 1], scorePer, calcAvg, suffix);
    }

    /// <summary>
    /// 用给定名称列表展开（legacy InsertExam_BatchCustomSelectorsWithName，BodyForm.cs:9501-9607）。
    /// <paramref name="suffix"/> 非空时追加到每个名称之后。
    /// </summary>
    public static IReadOnlyList<ExamInsertSelector> FromNames(
        IEnumerable<string> names,
        string scorePer = "0",
        bool calcAvg = true,
        string suffix = "")
    {
        ArgumentNullException.ThrowIfNull(names);

        var selectors = new List<ExamInsertSelector>();
        foreach (string name in names)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            selectors.Add(new ExamInsertSelector
            {
                SelectorName = string.IsNullOrEmpty(suffix) ? name : name + suffix,
                Type = ExamInsertRules.SelectorType,
                ScorePer = scorePer,
                CalcAvg = calcAvg
            });
        }

        if (selectors.Count == 0)
        {
            throw new ArgumentException("选项名称不能全部为空。", nameof(names));
        }

        return selectors;
    }

    /// <summary>
    /// 解析剪贴板文本为选项名（legacy 用 <c>Split(new[]{'\t','\r','\n'}, RemoveEmptyEntries)</c> 再滤空白）。
    /// </summary>
    public static IReadOnlyList<string> ParseClipboardNames(string? clipboardText)
    {
        if (string.IsNullOrWhiteSpace(clipboardText))
        {
            return Array.Empty<string>();
        }

        return clipboardText
            .Split(ClipboardSeparators, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0)
            .ToList();
    }

    /// <summary>剪贴板 → 选项（名称后追加 <paramref name="suffix"/>）。</summary>
    public static IReadOnlyList<ExamInsertSelector> FromClipboard(
        string? clipboardText,
        string scorePer = "0",
        bool calcAvg = true,
        string suffix = "")
    {
        IReadOnlyList<string> names = ParseClipboardNames(clipboardText);
        if (names.Count == 0)
        {
            throw new ArgumentException("剪贴板里没有可用的选项名称。", nameof(clipboardText));
        }

        return FromNames(names, scorePer, calcAvg, suffix);
    }

    /// <summary>
    /// 数值 → 中文序号，对齐 legacy Util.ConvertNumberToChinese（DocumentTools\Util.cs:1900-1938）：
    /// 「一十」读作「十」、11 读作「十一」、110 读作「一百一十」。
    /// legacy 的单位表只到「百」，1000 会指数组越界；这里对越界值退回阿拉伯数字，避免批量生成时抛异常。
    /// </summary>
    public static string ConvertNumberToChinese(int value)
    {
        if (value < 0 || value > MaxChineseNumber)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        if (value == 0)
        {
            return "零";
        }

        char[] digits = "零一二三四五六七八九".ToCharArray();
        string[] units = ["", "十", "百"];
        string result = string.Empty;
        int remaining = value;
        int position = 0;

        while (remaining > 0)
        {
            int digit = remaining % 10;
            if (digit == 0)
            {
                if (result.Length > 0 && !result.StartsWith('零'))
                {
                    result = "零" + result;
                }
            }
            else
            {
                result = digits[digit] + units[position] + result;
            }

            remaining /= 10;
            position++;
        }

        result = result.TrimEnd('零');
        return result.StartsWith("一十", StringComparison.Ordinal) ? result[1..] : result;
    }

    private static string ExpandName(string nameTemplate, int index, bool useChineseNumbering)
    {
        string number = useChineseNumbering
            ? ConvertNumberToChinese(index)
            : index.ToString(CultureInfo.InvariantCulture);
        return nameTemplate.Replace(Placeholder, number, StringComparison.Ordinal);
    }

    private static void ValidateTemplate(string nameTemplate, string message)
    {
        if (string.IsNullOrWhiteSpace(nameTemplate))
        {
            throw new ArgumentException(message, nameof(nameTemplate));
        }

        if (!nameTemplate.Contains(Placeholder, StringComparison.Ordinal))
        {
            throw new ArgumentException(message, nameof(nameTemplate));
        }
    }
}
