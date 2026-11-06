namespace DocumentTools.Core.Services.Office;

/// <summary>
/// legacy CompressPDF_CompressImageVersion（Util.cs:1460-1522）的三项，
/// 对应 Aspose.Pdf.Optimization.ImageCompressionVersion；Core 内用自有枚举避免把 Aspose 类型扩散到调用方。
/// </summary>
public enum PdfImageCompressionVersion
{
    Standard = 0,
    Fast = 1,
    Mixed = 2
}

/// <summary>PDF 压缩参数（对齐旧界面开关）。</summary>
public sealed class PdfCompressionOptions
{
    public bool WebOptimize { get; set; }

    public bool CompressImages { get; set; } = true;

    /// <summary>图片质量 1-100。</summary>
    public int ImageQuality { get; set; } = 75;

    /// <summary>图片压缩版本，仅当 <see cref="CompressImages"/> 为 true 时有意义。</summary>
    public PdfImageCompressionVersion ImageVersion { get; set; } = PdfImageCompressionVersion.Standard;

    /// <summary>图片最大 DPI，大于 0 才生效。</summary>
    public int MaxResolutionDpi { get; set; }

    public bool UseFlateEncoding { get; set; }

    public bool RemoveUnusedStreams { get; set; } = true;

    public bool LinkDuplicateStreams { get; set; } = true;

    public bool UnembedFonts { get; set; }

    public bool FlattenFormFields { get; set; }

    public bool RemoveUnusedObjects { get; set; } = true;
}

/// <summary>
/// 一次压缩的结果。低熵 PDF（纯文本、无位图）经 Aspose.Pdf 重写后体积反而会增长：
/// 实测 68,466B 的文本 PDF 连「只 load + save、不做任何优化」都变成 92,356B（134.9%），
/// 与是否开图压缩/资源精简无关，所以服务侧做「不放大」兜底并把事实回传给调用方。
/// </summary>
public sealed class PdfCompressionResult
{
    public string OutputFile { get; init; } = string.Empty;

    /// <summary>原始大小（字节）。</summary>
    public long SourceBytes { get; init; }

    /// <summary>实际写出的大小（字节）；回退时为原始大小。</summary>
    public long OutputBytes { get; init; }

    /// <summary>压缩结果不小于原始大小时回退，原样保留原始内容。</summary>
    public bool KeptOriginal { get; init; }

    /// <summary>输出/原始倍数，原始为 0 时返回 1。</summary>
    public double Ratio => SourceBytes <= 0 ? 1 : (double)OutputBytes / SourceBytes;
}

public interface IPdfCompressionService
{
    PdfCompressionResult Compress(string sourceFile, string outputFile, PdfCompressionOptions? options = null);
}
