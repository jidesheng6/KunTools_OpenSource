using DocumentTools.Core.Services.Image;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// 图片管线报错口径：路径过长 / 读不了 / 不是图片，必须分得开。
/// QA 报告（docs/qa-report-compat.md）里「路径过长被误报成图片损坏」的回归用例。
/// </summary>
public class ImageConversionDiagnosticsTests
{
    private static readonly ImageConversionService Service = new();

    [Fact]
    public void Convert_WithOverlongPath_ReportsPathTooLong_NotCorruptImage()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-diagnostics-").FullName;
        try
        {
            // 7 段 40 字符 + 文件名，整体超过 259（Windows MAX_PATH）。
            string deep = Path.Combine(
                directory,
                string.Join(Path.DirectorySeparatorChar, Enumerable.Repeat(new string('a', 40), 7)),
                "too-long.png");
            Assert.True(deep.Length > 259, $"构造的路径不够长：{deep.Length}");

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => Service.Convert(deep, directory, new ImageEncodeOptions { Format = "png" }));

            Assert.Contains("路径过长", ex.Message, StringComparison.Ordinal);
            Assert.Contains($"{deep.Length} 字符", ex.Message, StringComparison.Ordinal);
            Assert.Contains("D:\\temp\\", ex.Message, StringComparison.Ordinal);
            // 关键：不能把它说成解码问题，否则用户会去换图片。
            Assert.DoesNotContain("无法解码", ex.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("损坏", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ToIco_WithOverlongPath_ReportsPathTooLong()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-diagnostics-").FullName;
        try
        {
            string deep = Path.Combine(
                directory,
                string.Join(Path.DirectorySeparatorChar, Enumerable.Repeat(new string('b', 40), 7)),
                "too-long.ico.png");
            Assert.True(deep.Length > 259, $"构造的路径不够长：{deep.Length}");

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => Service.ToIco(deep, directory, 32));

            Assert.Contains("路径过长", ex.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("无法解码", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Convert_WithGarbageBytes_ReportsCorruptImage()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-diagnostics-").FullName;
        try
        {
            string broken = Path.Combine(directory, "broken.png");
            File.WriteAllBytes(broken, new byte[64]);   // 64 个 0，任何解码器都不认

            InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
                () => Service.Convert(broken, directory, new ImageEncodeOptions { Format = "png" }));

            Assert.Contains("损坏", ex.Message, StringComparison.Ordinal);
            Assert.Contains("broken.png", ex.Message, StringComparison.Ordinal);
            Assert.DoesNotContain("路径过长", ex.Message, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Convert_WithNormalImage_StillSucceeds()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-diagnostics-").FullName;
        try
        {
            string source = Path.Combine(directory, "normal.png");
            using (var bitmap = new SKBitmap(new SKImageInfo(24, 12, SKColorType.Rgba8888, SKAlphaType.Premul)))
            {
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(new SKColor(12, 34, 56));
                }

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                File.WriteAllBytes(source, data.ToArray());
            }

            string output = Service.Convert(source, directory, new ImageEncodeOptions { Format = "jpg" });

            Assert.EndsWith("_out.jpg", output, StringComparison.Ordinal);
            Assert.True(File.Exists(output));
            Assert.True(new FileInfo(output).Length > 0);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
