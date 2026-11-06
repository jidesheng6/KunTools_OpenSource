using System.Diagnostics;
using System.Text;
using DocumentTools.Core.Services.Office;
using Xunit;

namespace DocumentTools.Tests;

/// <summary>
/// task-29 的两条回归护栏：
/// 1) pdf.compress 不能把文件压大——底层（Aspose.Pdf 重写对象流）对低熵文本 PDF 必然膨胀 134%，
///    所以服务改成「产物不小于原始就回退原始内容」，这里钉住这个约定。
/// 2) PDF 平铺水印从「每格 new WatermarkArtifact」改成「复用同一个 TextStamp」后提速 ~30 倍，
///    这里钉住两件事：几何与旧实现逐像素一致、去水印仍然删得干净。
/// </summary>
public sealed class PdfCompressWatermarkTests : IDisposable
{
    private readonly string _root;

    public PdfCompressWatermarkTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "dt-compress-wm-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
            // 临时目录清不掉不影响断言结果。
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public void Compress_LowEntropyTextPdf_NeverGrows()
    {
        string source = Path.Combine(_root, "text.pdf");
        WriteRawPdf(source, pageCount: 1);
        long sourceBytes = new FileInfo(source).Length;

        string output = Path.Combine(_root, "text_out.pdf");
        var service = new PdfCompressionService();
        PdfCompressionResult result = service.Compress(source, output, new PdfCompressionOptions
        {
            ImageQuality = 75,
            MaxResolutionDpi = 150
        });

        Assert.Equal(sourceBytes, result.SourceBytes);
        Assert.True(
            result.OutputBytes <= result.SourceBytes,
            $"压缩后不能比原始大：原始 {result.SourceBytes}B，产物 {result.OutputBytes}B");
        Assert.Equal(result.OutputBytes, new FileInfo(output).Length);

        // 底层对这类文件只会变大，所以这里应当是「回退原始内容」而不是硬写出一个更大的文件。
        Assert.True(result.KeptOriginal, $"低熵文本 PDF 应当走回退路径，实际 KeptOriginal={result.KeptOriginal}");
        Assert.Equal(File.ReadAllBytes(source), File.ReadAllBytes(output));
    }

    [Fact]
    public void Compress_ImageHeavyPdf_ShrinksAndReportsBytes()
    {
        string source = Path.Combine(_root, "noise.pdf");
        WriteNoiseImagePdf(source);
        long sourceBytes = new FileInfo(source).Length;

        string output = Path.Combine(_root, "noise_out.pdf");
        var service = new PdfCompressionService();
        PdfCompressionResult result = service.Compress(source, output, new PdfCompressionOptions
        {
            ImageQuality = 60,
            MaxResolutionDpi = 100
        });

        Assert.True(
            result.OutputBytes < result.SourceBytes,
            $"图片密集的 PDF 应当真的变小：原始 {result.SourceBytes}B，产物 {result.OutputBytes}B");
        Assert.False(result.KeptOriginal, "真压缩成功时不应回退原始内容");
        Assert.True(result.Ratio < 1.0, $"Ratio 应在 0 与 1 之间，实际 {result.Ratio:F3}");
        Assert.Equal(result.OutputBytes, new FileInfo(output).Length);
    }

    [Fact]
    public void Watermark_Repeated_KeepsArtifactGeometryAndRemovesCleanly()
    {
        string source = Path.Combine(_root, "blank.pdf");
        WriteRawPdf(source, pageCount: 1);

        // 间距取 100pt（每页 (5+1)×(8+1)=54 个 artifact）：既覆盖平铺几何，又不至于让单测跑成性能测试。
        var options = new PdfWatermarkOptions
        {
            Text = "QA",
            Repeat = true,
            RepeatSpacingX = 100,
            RepeatSpacingY = 100,
            RotationDegrees = 45,
            Opacity = 0.5,
            FontName = "宋体"
        };

        string watermarked = Path.Combine(_root, "wm.pdf");
        new PdfWatermarkService().Apply(source, watermarked, options);
        int artifacts = CountArtifacts(watermarked);
        Assert.True(artifacts > 0, "平铺水印应当产生 artifact");

        // 旧实现（每格一个 WatermarkArtifact + Position）作为几何基准。
        string legacy = Path.Combine(_root, "wm_legacy.pdf");
        WriteLegacyWatermark(source, legacy, spacing: 100);
        Assert.Equal(CountArtifacts(legacy), artifacts);

        double diff = PageDiff(legacy, watermarked);
        Assert.True(diff <= 0.5, $"平铺水印与旧实现的渲染差异应 ≤0.5%，实际 {diff:F3}%");

        string removed = Path.Combine(_root, "wm_removed.pdf");
        new PdfWatermarkService().Remove(watermarked, removed);
        Assert.Equal(0, CountArtifacts(removed));

        // 去水印后应当回到原图（PDF 重写本身会带来约 0.2% 的渲染抖动，故留 1% 余量）。
        double residual = PageDiff(source, removed);
        Assert.True(residual <= 1.0, $"去水印后应与原始页面基本一致，实际差异 {residual:F3}%");
    }

    [Fact]
    public void Watermark_Tiled_IsDramaticallyFasterThanPerCellArtifacts()
    {
        string source = Path.Combine(_root, "bench.pdf");
        WriteRawPdf(source, pageCount: 1);

        var options = new PdfWatermarkOptions
        {
            Text = "QA",
            Repeat = true,
            RepeatSpacingX = 25,
            RepeatSpacingY = 25,
            RotationDegrees = 45,
            Opacity = 0.5,
            FontName = "宋体"
        };

        var watch = Stopwatch.StartNew();
        new PdfWatermarkService().Apply(source, Path.Combine(_root, "bench_new.pdf"), options);
        watch.Stop();
        long tiledMs = watch.ElapsedMilliseconds;

        watch.Restart();
        WriteLegacyWatermark(source, Path.Combine(_root, "bench_legacy.pdf"), spacing: 25);
        watch.Stop();
        long legacyMs = watch.ElapsedMilliseconds;

        // 单页 816 格：新实现约 0.2-0.5s，旧实现 8s 上下；这里只钉「明显更快」，不钉具体秒数以免抖动。
        Assert.True(
            tiledMs * 4 < legacyMs,
            $"复用 stamp 的平铺应当快一个数量级：新实现 {tiledMs}ms，旧实现 {legacyMs}ms");
    }

    private static int CountArtifacts(string path)
    {
        using var document = new Aspose.Pdf.Document(path);
        int total = 0;
        foreach (Aspose.Pdf.Page page in document.Pages)
        {
            total += page.Artifacts.Count;
        }

        return total;
    }

    private static double PageDiff(string left, string right)
    {
        using var a = Render(left);
        using var b = Render(right);
        int different = 0;
        for (int y = 0; y < a.Height; y++)
        {
            for (int x = 0; x < a.Width; x++)
            {
                global::System.Drawing.Color ca = a.GetPixel(x, y);
                global::System.Drawing.Color cb = b.GetPixel(x, y);
                int delta = Math.Abs(ca.R - cb.R) + Math.Abs(ca.G - cb.G) + Math.Abs(ca.B - cb.B);
                if (delta > 24)
                {
                    different++;
                }
            }
        }

        return different * 100.0 / (a.Width * a.Height);
    }

    private static global::System.Drawing.Bitmap Render(string path)
    {
        using var document = new Aspose.Pdf.Document(path);
        using var stream = new MemoryStream();
        var device = new Aspose.Pdf.Devices.PngDevice(new Aspose.Pdf.Devices.Resolution(72));
        device.Process(document.Pages[1], stream);
        stream.Position = 0;
        return new global::System.Drawing.Bitmap(stream);
    }

    private static void WriteLegacyWatermark(string source, string output, int spacing)
    {
        var font = Aspose.Pdf.Text.FontRepository.FindFont("SimSun");
        var state = new Aspose.Pdf.Text.TextState
        {
            Font = font,
            FontSize = 12,
            ForegroundColor = Aspose.Pdf.Color.Red
        };

        using var document = new Aspose.Pdf.Document(source);
        foreach (Aspose.Pdf.Page page in document.Pages)
        {
            int rows = (int)(page.Rect.Height / spacing);
            int columns = (int)(page.Rect.Width / spacing);
            for (int row = 0; row <= rows; row++)
            {
                for (int column = 0; column <= columns; column++)
                {
                    var artifact = new Aspose.Pdf.WatermarkArtifact
                    {
                        ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Center,
                        ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.Center,
                        Rotation = 45f,
                        Opacity = 0.5f
                    };
                    artifact.SetTextAndState("QA", state);
                    artifact.Position = new Aspose.Pdf.Point(column * spacing, row * spacing);
                    page.Artifacts.Add(artifact);
                }
            }
        }

        document.Save(output);
    }

    private static void WriteNoiseImagePdf(string path)
    {
        var random = new Random(7);
        string jpeg = Path.ChangeExtension(path, ".jpg");
        using (var bitmap = new global::System.Drawing.Bitmap(600, 800))
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    bitmap.SetPixel(x, y, global::System.Drawing.Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                }
            }

            bitmap.Save(jpeg, global::System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        using var document = new Aspose.Pdf.Document();
        var page = document.Pages.Add();
        page.AddImage(jpeg, new Aspose.Pdf.Rectangle(0, 0, 595, 842));
        document.Save(path);
    }

    /// <summary>
    /// 手写最小 PDF：评估模式下不能用 Aspose 造大集合，也避免为「低熵」特性引入依赖。
    /// </summary>
    private static void WriteRawPdf(string path, int pageCount)
    {
        var kids = new StringBuilder();
        for (int i = 0; i < pageCount; i++)
        {
            kids.Append(3 + (i * 2)).Append(" 0 R ");
        }

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [ {kids.ToString().Trim()} ] /Count {pageCount} >>"
        };

        for (int i = 0; i < pageCount; i++)
        {
            objects.Add(
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595.28 841.89]"
                + " /Resources << >>"
                + $" /Contents {4 + (i * 2)} 0 R >>");
            const string content = "q Q";
            objects.Add($"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        }

        var document = new StringBuilder();
        document.Append("%PDF-1.4\n");
        var offsets = new List<int>();
        for (int i = 0; i < objects.Count; i++)
        {
            offsets.Add(document.Length);
            document.Append(i + 1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n");
        }

        int xrefOffset = document.Length;
        document.Append("xref\n0 ").Append(objects.Count + 1).Append('\n');
        document.Append("0000000000 65535 f \n");
        foreach (int offset in offsets)
        {
            document.Append(offset.ToString("D10")).Append(" 00000 n \n");
        }

        document.Append("trailer\n<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\n");
        document.Append("startxref\n").Append(xrefOffset).Append('\n');
        document.Append("%%EOF\n");

        File.WriteAllText(path, document.ToString(), Encoding.ASCII);
    }
}
