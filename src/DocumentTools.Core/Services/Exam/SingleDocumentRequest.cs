using System.Globalization;

namespace DocumentTools.Core.Services.Exam;

/// <summary>
/// 单人表批量生成的「生成名称后缀模式」。
/// 对应 legacy 单选组 <c>SignDocumentUseOnlyNumber</c> / <c>SignDocumentUseOnlyPageNumber</c> /
/// <c>SignDocumentUsePersonNumber</c>（BodyForm.Designer.cs:11499/11507/11521/11535）。
/// </summary>
public enum SingleDocumentNameSuffixMode
{
    /// <summary>纯数字 + 后缀词：<c>XXX（001测评人）.docx</c>（legacy 默认档）。</summary>
    NumberWithSuffix,

    /// <summary>第 X 页：<c>XXX（第1页）.docx</c>。</summary>
    PageIndex,

    /// <summary>第 X 人：<c>XXX（第1人）.docx</c>。</summary>
    PersonIndex
}

/// <summary>
/// 单人表批量生成请求。文件名规则完全照 legacy
/// <c>BodyForm.cs:6286-6311</c>：模板文件名去扩展名 + 后缀 + <c>.docx</c>。
/// </summary>
public sealed class SingleDocumentRequest
{
    public string TemplatePath { get; set; } = string.Empty;

    /// <summary>图片形状的 AlternativeText（legacy 用 <c>@</c> 分隔多个占位名）。</summary>
    public List<string> AltTexts { get; set; } = new();

    /// <summary>单张图片文件，或图片目录（legacy 只认 jpeg/jpg/png/bmp）。</summary>
    public string ImageSourcePathOrDirectory { get; set; } = string.Empty;

    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>null 表示按图片数量全部生成（legacy 用 NumericUpDown 份数，Minimum=2）。</summary>
    public int? MaxCount { get; set; }

    public SingleDocumentNameSuffixMode NameSuffixMode { get; set; } = SingleDocumentNameSuffixMode.NumberWithSuffix;

    /// <summary>「生成名称后缀」文本（legacy <c>SignDocumentFileEndName</c>，默认「测评人」），仅 001 档参与拼接。</summary>
    public string SuffixWord { get; set; } = SingleDocumentNaming.DefaultSuffixWord;

    /// <summary>
    /// 使用内置测评人码图（legacy <c>SignDocumentUseEmbedImage</c>，Designer:11616 默认勾选）。
    /// 勾选后 <see cref="ImageSourcePathOrDirectory"/> 不再要求填写：图源来自
    /// <see cref="PersonCodeLibrary"/> 的 001–999 号内置码图。
    /// </summary>
    public bool UseBuiltInPersonCode { get; set; }

    /// <summary>内置码图起始序号（1–999），默认从 001 开始。</summary>
    public int PersonCodeStartIndex { get; set; } = PersonCodeLibrary.FirstIndex;
}

/// <summary>
/// 单人表文件名拼装（纯逻辑，便于无 ACE / 无 Word 环境下单测）。
/// </summary>
public static class SingleDocumentNaming
{
    public const string DefaultSuffixWord = "测评人";

    /// <summary>输出扩展名固定 .docx（legacy :6311 硬编码）。</summary>
    public const string OutputExtension = ".docx";

    /// <summary>Alt 文本分隔符：legacy 用 <c>@</c> 分隔多个图片占位名（BodyForm.cs:6242 附近的口径）。</summary>
    public const char AltTextSeparator = '@';

    /// <summary>把「占位名1@占位名2」拆成占位名清单（空项与首尾空白丢弃）。</summary>
    public static IReadOnlyList<string> ParseAltTexts(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? Array.Empty<string>()
            : text.Split(
                [AltTextSeparator],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>
    /// 生成后缀段（含全角括号）。<paramref name="index"/> 为 1 基文件序号：
    /// legacy 的 <c>FileIndex</c> 从 0 起、每份产出先自增再拼名（BodyForm.cs:6296/6307/6311）。
    /// </summary>
    public static string BuildSuffix(SingleDocumentNameSuffixMode mode, int index, string? suffixWord)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index, "文件序号不能为负数。");
        }

        return mode switch
        {
            SingleDocumentNameSuffixMode.PageIndex => $"（第{index.ToString(CultureInfo.InvariantCulture)}页）",
            SingleDocumentNameSuffixMode.PersonIndex => $"（第{index.ToString(CultureInfo.InvariantCulture)}人）",
            _ => $"（{index.ToString("D3", CultureInfo.InvariantCulture)}{(suffixWord ?? string.Empty).Trim()}）"
        };
    }

    /// <summary>由模板路径与后缀拼出输出文件名（不含目录）。</summary>
    public static string BuildFileName(string templatePath, string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templatePath);
        string stem = Path.GetFileNameWithoutExtension(templatePath);
        return stem + suffix + OutputExtension;
    }
}
