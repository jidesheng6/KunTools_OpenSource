using DocumentTools.Core.Services.Exam;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// ExamCircleImageService.Rotate 的几何与落盘行为。只依赖 SkiaSharp，不需要 ACE，所以在任何机器上都真跑。
/// 造图刻意用"左半黑右半白"的对称性明确素材：90/180/270 度后的黑白分布能唯一确定旋转方向。
/// </summary>
public sealed class ExamCircleImageServiceTests : IDisposable
{
    private readonly ExamCircleImageService _sut = new();
    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-rotate-" + Guid.NewGuid().ToString("n"));

    public ExamCircleImageServiceTests()
    {
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
            // 临时目录清理失败不影响断言结果
        }
        catch (UnauthorizedAccessException)
        {
            // 同上
        }
    }

    [Fact]
    public void Rotate_90_ExchangesWidthAndHeight()
    {
        string source = MakeSplitImage("src-90.png", 60, 40);

        string output = _sut.Rotate(source, 90, Path.Combine(_root, "out-90.png"));

        using SKBitmap rotated = SKBitmap.Decode(output)!;
        Assert.Equal(40, rotated.Width);
        Assert.Equal(60, rotated.Height);
    }

    [Fact]
    public void Rotate_90_MovesLeftHalfToTop()
    {
        string source = MakeSplitImage("src-dir.png", 60, 40);

        string output = _sut.Rotate(source, 90, Path.Combine(_root, "out-dir.png"));

        using SKBitmap rotated = SKBitmap.Decode(output)!;
        // 顺时针 90 度：原来的左半（黑）应当落到上半。
        Assert.True(IsDark(rotated.GetPixel(20, 10)), "旋转 90 度后上半应为黑");
        Assert.False(IsDark(rotated.GetPixel(20, 50)), "旋转 90 度后下半应为白");
    }

    [Fact]
    public void Rotate_Minus90_Equals270()
    {
        string source = MakeSplitImage("src-neg.png", 60, 40);
        string clockwise270 = _sut.Rotate(source, 270, Path.Combine(_root, "out-270.png"));

        string counterClockwise = _sut.Rotate(source, -90, Path.Combine(_root, "out-neg90.png"));

        using SKBitmap expected = SKBitmap.Decode(clockwise270)!;
        using SKBitmap actual = SKBitmap.Decode(counterClockwise)!;
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.GetPixel(20, 10), actual.GetPixel(20, 10));
        Assert.Equal(expected.GetPixel(20, 50), actual.GetPixel(20, 50));
        // 逆时针 90 度 = 顺时针 270 度：原左半（黑）落到下半。
        Assert.False(IsDark(actual.GetPixel(20, 10)), "顺时针 270 度后上半应为白");
        Assert.True(IsDark(actual.GetPixel(20, 50)), "顺时针 270 度后下半应为黑");
    }

    [Fact]
    public void Rotate_180_KeepsSizeAndMirrorsHorizontally()
    {
        string source = MakeSplitImage("src-180.png", 60, 40);

        string output = _sut.Rotate(source, 180, Path.Combine(_root, "out-180.png"));

        using SKBitmap rotated = SKBitmap.Decode(output)!;
        Assert.Equal(60, rotated.Width);
        Assert.Equal(40, rotated.Height);
        // 旋转 180 度后，原左半（黑）跑到右半。
        Assert.False(IsDark(rotated.GetPixel(15, 20)), "旋转 180 度后左半应为白");
        Assert.True(IsDark(rotated.GetPixel(45, 20)), "旋转 180 度后右半应为黑");
    }

    [Fact]
    public void Rotate_WithoutOutputPath_OverwritesSourceFile()
    {
        string source = MakeSplitImage("src-inplace.png", 60, 40);

        string output = _sut.Rotate(source, 90);

        Assert.Equal(source, output);
        using SKBitmap rotated = SKBitmap.Decode(source)!;
        Assert.Equal(40, rotated.Width);
        Assert.Equal(60, rotated.Height);
    }

    [Fact]
    public void Rotate_BmpOutput_WritesBitmapSignature()
    {
        string source = MakeSplitImage("src-bmp.png", 30, 20);

        string output = _sut.Rotate(source, 90, Path.Combine(_root, "nested", "out-90.bmp"));

        Assert.True(File.Exists(output));
        byte[] head = File.ReadAllBytes(output).Take(2).ToArray();
        Assert.Equal([(byte)'B', (byte)'M'], head);
        // 不只是魔数对：整份 BMP 必须能被解码器读回，且尺寸按 90 度交换。
        using SKBitmap decoded = SKBitmap.Decode(output)!;
        Assert.Equal(20, decoded.Width);
        Assert.Equal(30, decoded.Height);
    }

    [Fact]
    public void Rotate_UnsupportedDegrees_Throws()
    {
        string source = MakeSplitImage("src-bad.png", 10, 10);

        var ex = Assert.Throws<ArgumentException>(() => _sut.Rotate(source, 45));

        Assert.Contains("仅支持 0/90/180/270 度旋转", ex.Message);
    }

    [Fact]
    public void Rotate_MissingFile_ThrowsFileNotFound()
    {
        string missing = Path.Combine(_root, "nope.png");

        var ex = Assert.Throws<FileNotFoundException>(() => _sut.Rotate(missing, 90));

        Assert.Contains("图片不存在", ex.Message);
    }

    [Fact]
    public void Rotate_UndecodableFile_ThrowsInvalidOperation()
    {
        string broken = Path.Combine(_root, "broken.png");
        File.WriteAllBytes(broken, [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07]);

        var ex = Assert.Throws<InvalidOperationException>(() => _sut.Rotate(broken, 90));

        Assert.Contains("无法解码图片", ex.Message);
    }

    /// <summary>左半黑、右半白的 PNG，用于判定旋转方向。</summary>
    private string MakeSplitImage(string fileName, int width, int height)
    {
        string path = Path.Combine(_root, fileName);
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = false };
            canvas.DrawRect(SKRect.Create(0, 0, width / 2f, height), paint);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
        return path;
    }

    private static bool IsDark(SKColor color) => color.Red < 64 && color.Green < 64 && color.Blue < 64;
}
