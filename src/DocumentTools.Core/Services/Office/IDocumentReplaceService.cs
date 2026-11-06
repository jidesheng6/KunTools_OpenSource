namespace DocumentTools.Core.Services.Office;

public sealed class DocumentReplaceOptions
{
    public IReadOnlyList<(string Find, string Replace)> Pairs { get; init; } =
        Array.Empty<(string, string)>();

    /// <summary>When true, match case (legacy NoIgnoreCase checked).</summary>
    public bool MatchCase { get; init; }

    public bool FindWholeWord { get; init; }

    public bool UseRegex { get; init; }

    public bool IgnoreFootnotes { get; init; }

    public bool IgnoreDeleted { get; init; }

    public bool IgnoreFieldCodes { get; init; }

    public bool IgnoreFields { get; init; }

    public bool IgnoreInserted { get; init; }

    public bool IgnoreShapes { get; init; }

    public bool UseRegexSubstitutions { get; init; }

    public bool ExcelMatchEntireCell { get; init; }

    public bool ExcelRemoveRows { get; init; }

    public int ExcelStartRow { get; init; } = -1;

    public int ExcelEndRow { get; init; } = -1;

    /// <summary>
    /// 把替换进去的文字刷成新样式（legacy <c>Util.cs:5373 UseNewStyle</c> + <c>Util.cs:5412-5416 ApplyFont</c>）。
    /// 默认 false = 纯文本替换，沿用被替换处的格式，与 legacy 实跑行为一致。
    /// </summary>
    public bool UseNewStyle { get; init; }

    /// <summary>替换文字的字体名；空 = 不改字体。仅在 <see cref="UseNewStyle"/> 为 true 时生效。</summary>
    public string FontName { get; init; } = string.Empty;

    /// <summary>替换文字的字号（磅）；0 = 不改字号。仅在 <see cref="UseNewStyle"/> 为 true 时生效。</summary>
    public double FontSize { get; init; }

    /// <summary>
    /// 替换文字是否加粗：null = 不改。仅在 <see cref="UseNewStyle"/> 为 true 时生效。
    /// legacy 的 <c>ApplyFont</c> 分支只设字体名与字号，加粗是本实现补的三态项，null 保证默认无副作用。
    /// </summary>
    public bool? Bold { get; init; }
}

public interface IDocumentReplaceService
{
    void ReplaceWord(string sourceFile, string savedFile, DocumentReplaceOptions options);

    void ReplaceExcel(string sourceFile, string savedFile, DocumentReplaceOptions options);
}
