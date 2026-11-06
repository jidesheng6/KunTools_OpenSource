namespace DocumentTools.Core.Services.Exam;

public sealed class VoteReportRequest
{
    public Stream? Template { get; set; }
    public string OutputFile { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string StartText { get; set; } = string.Empty;
    public string EndText { get; set; } = string.Empty;
    public string GenDateText { get; set; } = string.Empty;
    public string NameColumn { get; set; } = VoteReportRules.DefaultNameColumn;
    public string AgreeColumn { get; set; } = VoteReportRules.DefaultAgreeColumn;
    public string UnAgreeColumn { get; set; } = VoteReportRules.DefaultUnAgreeColumn;
    public string AbstainColumn { get; set; } = VoteReportRules.DefaultAbstainColumn;
    public IReadOnlyList<string> FillNames { get; set; } = Array.Empty<string>();
    public bool KeepNumberIndex { get; set; } = true;
    public bool RandomVoteCounts { get; set; }
    public bool NeedAnotherTable { get; set; }
    public bool KeepFooter { get; set; } = true;
    public double RowHeightCentimeters { get; set; } = 0.8;
    public double FontSize { get; set; } = 12;

    /// <summary>
    /// 表格数据行行高（厘米）。legacy 只给表头行设行高（Util.cs:3315），数据行没设；
    /// 这里单独给数据行一个值，默认与表头一致。
    /// </summary>
    public double DataRowHeightCentimeters { get; set; } = 0.8;

    /// <summary>表格数据行字号（磅）。legacy 的 <c>Font.Size</c> 只落在表头（Util.cs:3322），数据行没设。</summary>
    public double DataRowFontSize { get; set; } = 12;

    /// <summary>
    /// 手工填写的生成日期文本。留空时用当天日期（legacy「今天」按钮写 <c>yyyy年MM月dd日</c>，BodyForm.cs:5784-5791）。
    /// </summary>
    public string GenerationDate { get; set; } = string.Empty;
}

public interface IVoteReportService
{
    /// <summary>Generate election result report DOCX from embedded/template stream.</summary>
    void Create(VoteReportRequest request);

    /// <summary>Open default template from embedded/VoteReportDoc.doc when present.</summary>
    Stream? OpenDefaultTemplate();
}

/// <summary>选举结果报告单的输入格式规则（纯逻辑，便于单测）。</summary>
public static class VoteReportRules
{
    /// <summary>候选人姓名的表头默认值（模板里的列名，legacy BodyForm.cs:5784 起）。</summary>
    public const string DefaultNameColumn = "姓名";

    /// <summary>同意票的表头默认值。</summary>
    public const string DefaultAgreeColumn = "同意";

    /// <summary>不同意票的表头默认值。</summary>
    public const string DefaultUnAgreeColumn = "不同意";

    /// <summary>弃权票的表头默认值。</summary>
    public const string DefaultAbstainColumn = "弃权";

    /// <summary>另存对话框的默认文件名。</summary>
    public const string DefaultOutputFileName = "选举结果报告单.docx";

    /// <summary>候选人名单按行贴入：「每行一个候选人」，空行丢弃。</summary>
    public static IReadOnlyList<string> SplitCandidateNames(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? Array.Empty<string>()
            : text.Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
