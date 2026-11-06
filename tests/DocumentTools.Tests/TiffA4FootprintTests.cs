using System.Diagnostics;
using System.Globalization;
using DocumentTools.Core.Services.Image;
using SkiaSharp;
using Xunit.Abstractions;

namespace DocumentTools.Tests;

/// <summary>
/// A4 300dpi（2480×3508 = 8.7MP）真实扫描页的体积与耗时验收。
/// 320×240 的矩阵只能证明编码器正确，说明不了「扫描件到底省多少」，这里按三种典型内容量。
/// 阈值来自 task-31 的验收线：表格线 LZW &lt; 未压缩的 15%，纯白页 &lt; 1%，彩色照片只记录不设线。
/// </summary>
public class TiffA4FootprintTests
{
    private const int Width = 2480;
    private const int Height = 3508;

    private readonly ITestOutputHelper _output;

    public TiffA4FootprintTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void A4_300dpi_ReportsFootprint_AndMeetsThresholds()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-a4-").FullName;
        try
        {
            _output.WriteLine("A4 300dpi（2480×3508，8.7MP）· 未压缩 24bpp RGB 理论值 "
                + (Width * 3L * Height / 1024d / 1024d).ToString("F2", CultureInfo.InvariantCulture) + " MB");
            _output.WriteLine("内容 | None | PackBits | LZW | PB 占比 | LZW 占比 | LZW 耗时");

            (string Name, SKBitmap Bitmap, double MaxLzwRatio)[] pages =
            [
                ("纯白页（空白扫描）", WhitePage(), 0.01),
                ("表格线 + 文字块", TablePage(), 0.15),
                ("彩色噪声渐变", PhotoPage(), double.NaN)   // NaN = 只记录，不设阈值
            ];

            foreach ((string name, SKBitmap bitmap, double maxLzwRatio) in pages)
            {
                using (bitmap)
                {
                    Measure(directory, name, bitmap, maxLzwRatio);
                }
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>12MP 场景（4000×3000，常见手机照片）别慢到用户以为卡死。</summary>
    [Fact]
    public void TwelveMegapixel_PhotoPage_EncodesWithinBudget()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-12mp-").FullName;
        try
        {
            _output.WriteLine("12MP（4000×3000）彩色照片式内容 · LZW");
            using SKBitmap bitmap = PhotoPage(4000, 3000);
            Measure(directory, "12MP 彩色噪声渐变", bitmap, double.NaN);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private void Measure(string directory, string name, SKBitmap bitmap, double maxLzwRatio)
    {
        string source = Path.Combine(directory, "page.png");
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        using (var stream = File.Open(source, FileMode.Create, FileAccess.Write))
        {
            data.SaveTo(stream);
        }

        var sizes = new Dictionary<TiffCompression, long>();
        var elapsed = new Dictionary<TiffCompression, double>();
        foreach (TiffCompression compression in new[]
                 {
                     TiffCompression.None, TiffCompression.PackBits, TiffCompression.Lzw
                 })
        {
            string outputDirectory = Path.Combine(directory, compression.ToString());
            Directory.CreateDirectory(outputDirectory);

            var watch = Stopwatch.StartNew();
            string output = new ImageConversionService().Convert(source, outputDirectory, new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = compression
            });
            watch.Stop();

            byte[] tiff = File.ReadAllBytes(output);
            Assert.Equal(0x49, tiff[0]);
            Assert.Equal(0x49, tiff[1]);
            Assert.Equal(42, tiff[2] | (tiff[3] << 8));

            sizes[compression] = tiff.LongLength;
            elapsed[compression] = watch.Elapsed.TotalMilliseconds;
        }

        double Ratio(TiffCompression compression)
            => sizes[compression] / (double)sizes[TiffCompression.None];

        _output.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0} | {1:F2} MB | {2:F2} MB | {3:F2} MB | {4:P1} | {5:P1} | {6:F0} ms",
            name,
            sizes[TiffCompression.None] / 1024d / 1024d,
            sizes[TiffCompression.PackBits] / 1024d / 1024d,
            sizes[TiffCompression.Lzw] / 1024d / 1024d,
            Ratio(TiffCompression.PackBits),
            Ratio(TiffCompression.Lzw),
            elapsed[TiffCompression.Lzw]));

        if (!double.IsNaN(maxLzwRatio))
        {
            Assert.True(
                Ratio(TiffCompression.Lzw) < maxLzwRatio,
                $"{name}：LZW 体积占未压缩的 {Ratio(TiffCompression.Lzw):P1}，超过验收线 {maxLzwRatio:P0}");
        }

        // 大图必须分条：2480 像素宽 → 7440 字节/行，64KB 目标 ≈ 8 行/条。
        Assert.True(elapsed[TiffCompression.Lzw] < 30000, $"{name}：LZW 编码 {elapsed[TiffCompression.Lzw]:F0} ms 过慢");
    }

    private static SKBitmap NewPage() =>
        new(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul));

    /// <summary>空白扫描：整页白。</summary>
    private static SKBitmap WhitePage()
    {
        SKBitmap bitmap = NewPage();
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);
        return bitmap;
    }

    /// <summary>表格线 + 文字块：横竖细线加字形块，接近考评表扫描件的熵。</summary>
    private static SKBitmap TablePage()
    {
        SKBitmap bitmap = NewPage();
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        using var stroke = new SKPaint { Color = new SKColor(55, 55, 55), StrokeWidth = 2, IsAntialias = false };
        using var fill = new SKPaint { Color = new SKColor(35, 35, 35), IsAntialias = false };

        const int top = 150;
        const int rowStep = 70;
        const int rows = 44;
        for (int i = 0; i <= rows; i++)
        {
            int y = top + (i * rowStep);
            canvas.DrawLine(160, y, Width - 160, y, stroke);
        }

        for (int c = 0; c <= 7; c++)
        {
            int x = 160 + (c * (Width - 320) / 7);
            canvas.DrawLine(x, top, x, top + (rows * rowStep), stroke);
        }

        // 每个单元格里一排「字形块」：笔画宽 2px、字距 14px，模拟 300dpi 扫描下的文字熵。
        for (int row = 0; row < rows; row++)
        {
            int y = top + (row * rowStep) + 20;
            for (int col = 0; col < 7; col++)
            {
                int x = 175 + (col * (Width - 320) / 7);
                for (int ch = 0; ch < 9; ch++)
                {
                    for (int line = 0; line < 3; line++)
                    {
                        canvas.DrawRect(x + (ch * 14), y + (line * 14), 10, 8, fill);
                    }
                }
            }
        }

        return bitmap;
    }

    /// <summary>彩色照片式内容：平滑渐变叠小幅噪声，用来如实展示 TIF 在照片上的体积。</summary>
    private static SKBitmap PhotoPage(int width = Width, int height = Height)
    {
        var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Opaque));
        using SKPixmap pixmap = bitmap.PeekPixels();
        Span<byte> span = pixmap.GetPixelSpan();
        var random = new Random(20260101);

        for (int y = 0; y < height; y++)
        {
            int row = y * pixmap.RowBytes;
            for (int x = 0; x < width; x++)
            {
                int at = row + (x * 4);
                span[at] = (byte)((((x + y) * 255) / (width + height) + random.Next(10)) & 0xFF);  // B
                span[at + 1] = (byte)(((y * 255) / height + random.Next(10)) & 0xFF);              // G
                span[at + 2] = (byte)(((x * 255) / width + random.Next(10)) & 0xFF);               // R
                span[at + 3] = 255;
            }
        }

        return bitmap;
    }
}
