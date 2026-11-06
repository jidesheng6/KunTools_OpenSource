namespace DocumentTools.Core.Services.Office;

/// <summary>Word 拆分参数（每 N 页 / 页码区间）。</summary>
public sealed class WordSplitOptions
{
    /// <summary>true = 按 PagesPerFile 每 N 页切一份；false = 只导出一段页码。</summary>
    public bool SplitEveryPages { get; set; } = true;

    public int PagesPerFile { get; set; } = 1;

    public string? RangeStart { get; set; }

    public string? RangeEnd { get; set; }

    /// <summary>每份文档页码是否从 1 开始。</summary>
    public bool ResetPageNumber { get; set; }
}

/// <summary>
/// PPT 拆分参数。幻灯片尺寸/缩放以枚举名字符串传入（服务侧解析），避免把 Aspose 枚举泄漏到 UI 与 Schema。
/// </summary>
public sealed class SlideSplitOptions
{
    /// <summary>
    /// true = 按 <see cref="SlidesPerFile"/> 切块（1 = 每页）；
    /// false = 只导出 <see cref="RangeStart"/>–<see cref="RangeEnd"/>。
    /// </summary>
    public bool SplitEverySlide { get; set; } = true;

    /// <summary>每份幻灯片数，仅当 <see cref="SplitEverySlide"/> 为 true 时生效。</summary>
    public int SlidesPerFile { get; set; } = 1;

    public string? RangeStart { get; set; }

    public string? RangeEnd { get; set; }

    /// <summary>
    /// legacy SplitPPT 的 SplitPdfSizeCombox：Aspose.Slides.SlideSizeType 枚举名（OnScreen/A4Paper…）。
    /// 空值或无法解析时回退 OnScreen，保持与拆分前的历史行为一致。
    /// </summary>
    public string? SlideSizeName { get; set; }

    /// <summary>
    /// legacy SplitPPT 的 SplitPdfScaleCombox：Aspose.Slides.SlideSizeScaleType 枚举名
    /// （Maximize/EnsureFit/DoNotScale）。空值或无法解析时回退 Maximize。
    /// </summary>
    public string? SlideScaleName { get; set; }
}

public interface IDocumentMergeSplitService
{
    /// <summary>按顺序拼接多个 PDF。非 .pdf 输入会被拒绝。</summary>
    string MergePdfFiles(IList<string> pdfFiles, string outputFile);

    /// <summary>按顺序拼接多个 Word（保留各自格式）。</summary>
    string MergeWordFiles(IList<string> wordFiles, string outputFile);

    /// <summary>拆分 Word，返回生成的文件列表（按页序）。</summary>
    IReadOnlyList<string> SplitWord(string sourceFile, string outputDirectory, WordSplitOptions options);

    /// <summary>拆分 PPT，返回生成的文件列表（按页序）。</summary>
    IReadOnlyList<string> SplitPowerPoint(string sourceFile, string outputDirectory, SlideSplitOptions options);
}
