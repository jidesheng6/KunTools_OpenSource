using System.Globalization;

namespace DocumentTools.Core.Services.Exam;

public enum ExamStoreDialect
{
    Access2003,
    Sqlite
}

public static class ExamInsertRules
{
    public const string InvalidSelectorName = "无效";
    public const string AbstainSelectorName = "弃权";
    public const string ItemType = "项目";
    public const string SelectorType = "选项";
    public const string SelectorExcludedFromAverageType = "选项不计入得票率";
    public const string MeasurementKind = "测评";
    public const string ExcludedSelectorFixedScore = "1";

    /// <summary>得分比重默认值（legacy Designer:12558 无显式 Value → 0）。</summary>
    public const string DefaultItemScoreWeight = "0";

    /// <summary>弃权得分比重默认值（legacy Designer:12357 的 decimal 值 → -1）。</summary>
    public const string DefaultAbstainScoreWeight = "-1";

    /// <summary>无效得分比重默认值（legacy Designer:12391 的 decimal 值 → -1）。</summary>
    public const string DefaultInvalidScoreWeight = "-1";

    /// <summary>考评表布局类型（写库值 <c>Exam_EssInfo.TypeMode</c>，legacy 插入考评页的单选组）。</summary>
    public const string PeopleVerticalTypeMode = "人员纵排";

    public const string PeopleVertical2TypeMode = "人员纵排2";

    public const string PeopleHorizontalTypeMode = "人员横排";

    public const string SingleMarkTypeMode = "单人标识";

    /// <summary>布局类型下拉的全部取值，顺序即 legacy 单选组顺序。</summary>
    public static readonly IReadOnlyList<string> TypeModes =
    [
        PeopleVerticalTypeMode,
        PeopleVertical2TypeMode,
        PeopleHorizontalTypeMode,
        SingleMarkTypeMode
    ];

    /// <summary>选项串分隔符：半角/全角逗号、分号、竖线都算（legacy 多选拼接串里这几种都出现过）。
    /// 属输入格式业务知识，视图不得自写 <c>Split</c>。
    /// </summary>
    private static readonly char[] SelectorListSeparators = [',', '，', ';', '；', '|'];

    /// <summary>库里的 <c>TypeMode</c> 值对应下拉第几项；未知值返回 -1（下拉保持原选择）。</summary>
    public static int TypeModeIndex(string? typeMode)
    {
        string? trimmed = typeMode?.Trim();
        for (int index = 0; index < TypeModes.Count; index++)
        {
            if (string.Equals(TypeModes[index], trimmed, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>把「同意,不同意,弃权」这类选项串拆成选项名清单（空项与首尾空白丢弃）。</summary>
    public static IReadOnlyList<string> ParseSelectorList(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? Array.Empty<string>()
            : text.Split(SelectorListSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public static bool IsImplicitSelector(string selectorName)
        => string.Equals(selectorName, InvalidSelectorName, StringComparison.Ordinal)
           || string.Equals(selectorName, AbstainSelectorName, StringComparison.Ordinal);

    /// <summary>
    /// 把「项目分值 / 选项分值 / 得分比重」这类字符串解析成数值。
    /// 空串按 0 处理（沿用默认比重），**非空且不是数字时必须抛错**——静默变 0 会让用户填的分值无声消失，
    /// 报表里怎么算都对不上，而错误要等到成绩汇总时才被发现。
    /// </summary>
    public static double ParseScore(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return 0d;
        }

        if (double.TryParse(raw.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        throw new ArgumentException(
            $"分值必须是数字，收到「{raw.Trim()}」。请填 0、1 或 1.5 这样的数字。", nameof(raw));
    }
}

public sealed class ExamInsertSelector
{
    public string SelectorName { get; set; } = string.Empty;
    public string Type { get; set; } = ExamInsertRules.SelectorType;
    public string ScorePer { get; set; } = "0";
    public bool CalcAvg { get; set; } = true;
}

public sealed class ExamInsertItem
{
    public string ItemName { get; set; } = string.Empty;
    public string Type { get; set; } = ExamInsertRules.ItemType;

    /// <summary>
    /// 项目分值。空串＝沿用 <see cref="ExamInsertRequest.ItemScoreWeight"/>；显式 <c>"0"</c> 表示该项目得分为 0。
    /// </summary>
    public string ItemScore { get; set; } = string.Empty;
    public List<ExamInsertSelector> Selectors { get; set; } = new();
}

public sealed class ExamInsertRequest
{
    public string DatabaseFile { get; set; } = string.Empty;
    public string ExamName { get; set; } = string.Empty;
    public string TypeMode { get; set; } = ExamInsertRules.PeopleVerticalTypeMode;
    public List<ExamInsertItem> Items { get; set; } = new();

    /// <summary>
    /// 得分比重（legacy <c>InsertExam_ExamScore</c>，默认 0）：项目未单独填分值时用它。
    /// 落库位置：Exam_Score 的项目行（Kind=项目）。
    /// </summary>
    public string ItemScoreWeight { get; set; } = ExamInsertRules.DefaultItemScoreWeight;

    /// <summary>
    /// 弃权得分比重（legacy <c>InsertExam_SelectorAbstain_ScorePer</c>，默认 -1）。
    /// 落库位置：Exam_Score 里每个项目追加的 SelName='弃权' 行。
    /// </summary>
    public string AbstainScoreWeight { get; set; } = ExamInsertRules.DefaultAbstainScoreWeight;

    /// <summary>弃权是否计入得票率（legacy <c>InsertExam_SelectorAbstain_CalcAvg</c>，默认 true）。</summary>
    public bool AbstainCalcAvg { get; set; } = true;

    /// <summary>
    /// 无效得分比重（legacy <c>InsertExam_SelectorInvalid_ScorePer</c>，默认 -1）。
    /// 落库位置：Exam_Score 里每个项目追加的 SelName='无效' 行。
    /// </summary>
    public string InvalidScoreWeight { get; set; } = ExamInsertRules.DefaultInvalidScoreWeight;

    /// <summary>无效是否计入得票率（legacy <c>InsertExam_SelectorInvalid_CacAvg</c>，默认不勾选）。</summary>
    public bool InvalidCalcAvg { get; set; }
}

public sealed class ExamInsertResult
{
    public bool Success { get; set; }
    public int ExamId { get; set; }
    public bool CreatedExam { get; set; }
    public int ItemCount { get; set; }
    public int SelectorCount { get; set; }
    public int BoolItemCount { get; set; }

    /// <summary>按 legacy 自动追加的「无效」「弃权」行数（不计入 <see cref="SelectorCount"/>）。</summary>
    public int ImplicitSelectorCount { get; set; }
    public string Summary { get; set; } = string.Empty;

    public static ExamInsertResult Failed(string summary) => new() { Success = false, Summary = summary };
}
