namespace DocumentTools.Core.Services.Image;

/// <summary>PNG 颜色类型（legacy CompressPngColorMode_Combox 里 SkiaSharp 能落地的子集）。</summary>
public enum ImagePngColorType
{
    Truecolor = 0,
    Grayscale = 1,
    TruecolorWithAlpha = 2
}

/// <summary>
/// 图片重编码参数。键值来自 <c>image.compress</c> 的 Schema：
/// JPEG 质量 / JPEG 颜色模式 / PNG 压缩级别 / PNG 颜色模式。
/// </summary>
public sealed record ImageEncodeOptions
{
    /// <summary>png|jpg|jpeg|bmp|webp（大小写不敏感，可带前导点）。</summary>
    public string Format { get; init; } = "png";

    /// <summary>JPEG 质量 1-100（legacy CompressJpegQuailty_Combox，默认 80）。</summary>
    public int JpegQuality { get; init; } = 80;

    /// <summary>JPEG 灰度输出（legacy JpegCompressionColorMode.Grayscale）。</summary>
    public bool JpegGrayscale { get; init; }

    /// <summary>PNG zlib 级别 0-9（legacy CompressPngLevel_Combox，默认 3）。</summary>
    public int PngLevel { get; init; } = 3;

    /// <summary>PNG 颜色类型（legacy CompressPngColorMode_Combox，默认 Truecolor）。</summary>
    public ImagePngColorType PngColor { get; init; } = ImagePngColorType.Truecolor;

    /// <summary>长边上限（像素），null/≤0 表示不缩放。</summary>
    public int? MaxEdgePx { get; init; }

    /// <summary>BMP 色彩位数：4 / 8 / 16 / 24 / 32（legacy CompressBmpPixeBits_Combox，默认 24）。</summary>
    public int BmpPixelBits { get; init; } = 24;

    /// <summary>
    /// TIFF 条带压缩方式（tag 259）。未压缩 24bpp 太大，服务层默认 <see cref="TiffCompression.Lzw"/>；
    /// legacy 由 Aspose <c>TiffOptions</c> 决定，这里显式暴露以便需要时选 PackBits 或无压缩。
    /// 实测（320×240，None 基准 230,624 字节）：LZW 在彩色渐变/灰色渐变/白页/棋盘二值图上分别省
    /// 15.7% / 59.5% / 99.2% / 95.7%，只有纯噪声会反向增长 36.8%；PackBits 在彩色渐变上为 -0.8%。
    /// </summary>
    public TiffCompression TiffCompression { get; init; } = TiffCompression.Lzw;

    /// <summary>
    /// 沿用源文件后缀名（legacy <c>CompressImage_NotUseSelfExt</c> 未勾选时的行为）：
    /// 为真时输出扩展名取源文件扩展名，忽略 <see cref="Format"/>。
    /// 服务层默认 false（显式指定的 <see cref="Format"/> 必须生效），legacy 的「默认沿用源后缀」
    /// 由 <c>ImageConvertOperation</c> 读 <c>useSourceExtension</c> 键（默认 "true"）决定。
    /// </summary>
    public bool UseSourceExtension { get; init; }

    /// <summary>
    /// 不执行压缩（legacy <c>CompressIamge_NoCompress</c>）：只按命名规则另存一份，字节与源文件完全一致。
    /// </summary>
    public bool NoCompress { get; init; }
}

public interface IImageConversionService
{
    /// <summary>Re-encode / optional resize. <paramref name="format"/> is png|jpg|jpeg|bmp|webp.</summary>
    string Convert(
        string sourceFile,
        string outputDirectory,
        string format,
        int jpegQuality = 75,
        int? maxEdgePx = null);

    /// <summary>Same as above with the细分参数 (JPEG 颜色/质量、PNG 级别/颜色).</summary>
    string Convert(string sourceFile, string outputDirectory, ImageEncodeOptions options);

    /// <summary>WEBP → png/jpg/bmp under output directory (legacy JPEG quality = 100).</summary>
    string WebpToImage(string sourceFile, string outputDirectory, string format = "png");

    /// <summary>Raster → single-size ICO (PNG payload, Vista+ layout).</summary>
    string ToIco(string sourceFile, string outputDirectory, int size = 32);

    /// <summary>Raster → one ICO carrying every requested size (PNG payloads).</summary>
    string ToIco(string sourceFile, string outputDirectory, IReadOnlyList<int> sizes);
}
