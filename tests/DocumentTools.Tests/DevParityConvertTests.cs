using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Services.Image;
using SkiaSharp;
using Xunit;

namespace DocumentTools.Tests;

/// <summary>
/// dev 对齐回归（task-38）：把「新版界面/默认值与 dev 面板逐项一致」这件事锁住。
/// 证据行号见 docs/dev-parity-convert.md（dev 侧文件与工作树 DocumentTools/ 逐字节相同）。
/// </summary>
public sealed class DevParityConvertTests : IDisposable
{
    private readonly string _dir;

    public DevParityConvertTests()
    {
        _dir = Directory.CreateTempSubdirectory("documenttools-devparity-").FullName;
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_dir, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public void PdfCompliance_OffersEveryDevVersionInEnumOrder()
    {
        // dev: BodyForm.cs:1223 `Enum.GetNames(typeof(Aspose.Pdf.PdfFormat))` —— 27 项，默认第 0 项。
        OptionField version = Field("pdf.compliance", "version");

        // 逐项比字符串序列，不用 Count==27：`Enum.GetNames` 的顺序只是「今天的枚举声明顺序」，
        // Aspose 升级可能插值，那时 dev 面板会跟着变、我们的 Schema 不会——这条断言就是提醒改键表的人。
        string[] expected =
        [
            "PDF_A_1A", "PDF_A_1B", "PDF_A_2A", "PDF_A_3A", "PDF_A_2B", "PDF_A_2U", "PDF_A_3B", "PDF_A_3U",
            "v_1_0", "v_1_1", "v_1_2", "v_1_3", "v_1_4", "v_1_5", "v_1_6", "v_1_7",
            "v_2_0", "PDF_UA_1", "PDF_X_1A_2001", "PDF_X_1A", "PDF_X_3", "ZUGFeRD",
            "PDF_A_4", "PDF_A_4E", "PDF_A_4F", "PDF_X_4", "PDF_E_1"
        ];
        Assert.Equal(expected, version.Choices!.Select(c => c.Value).ToArray());
        Assert.Equal("PDF_A_1A", version.DefaultValue);

        // 每一项都必须是 Aspose.Pdf.PdfFormat 的真实枚举名（否则运行期会抛「不支持的 PDF 格式」）。
        foreach (OptionChoice choice in version.Choices!)
        {
            Assert.True(
                Enum.TryParse<Aspose.Pdf.PdfFormat>(choice.Value, ignoreCase: true, out _),
                $"下拉项「{choice.Value}」不是 Aspose.Pdf.PdfFormat 的枚举名。");
        }
    }

    [Fact]
    public void WebpImage_OffersDevFiveFormatsInPanelOrder()
    {
        // dev: BodyForm.cs:1423-1427 JPG/.jpg、JPEG/.jpeg、PNG/.png、BMP/.bmp、GIF/.gif，默认第 0 行 JPG。
        OptionField format = Field("webp.image", "format");

        Assert.Equal(["JPG", "JPEG", "PNG", "BMP", "GIF"], format.Choices!.Select(c => c.Value).ToArray());
        Assert.Equal("JPG", format.DefaultValue);
    }

    [Fact]
    public void HtmlPdf_DefaultsMatchDevPanel()
    {
        // dev: BodyForm.Designer.cs:6597/6615/6624 四边距 "50"；:6697/6727 两个勾选框未勾。
        Assert.Equal("50", Field("html.pdf", "marginTop").DefaultValue);
        Assert.Equal("50", Field("html.pdf", "marginBottom").DefaultValue);
        Assert.Equal("50", Field("html.pdf", "marginLeft").DefaultValue);
        Assert.Equal("50", Field("html.pdf", "marginRight").DefaultValue);

        Assert.Equal("false", Field("html.pdf", "useCss").DefaultValue);
        Assert.Equal("false", Field("html.pdf", "waitResources").DefaultValue);

        // dev: BodyForm.cs:1325 SelectedIndex = 4 → A4；方向默认「竖向」。
        Assert.Equal("4", Field("html.pdf", "paper").DefaultValue);
        Assert.Equal("false", Field("html.pdf", "landscape").DefaultValue);
    }

    [Fact]
    public void WebpToImage_KeepsDevDistinctJpegExtension()
    {
        // dev 的格式表把 JPG(.jpg) 与 JPEG(.jpeg) 列成两行，编码参数相同、后缀不同（BodyForm.cs:1423-1424）。
        string webp = CreateWebp(Path.Combine(_dir, "src.webp"));
        var service = new ImageConversionService();

        Assert.EndsWith("_out.jpg", service.WebpToImage(webp, _dir, "JPG"), StringComparison.Ordinal);
        Assert.EndsWith("_out.jpeg", service.WebpToImage(webp, _dir, "JPEG"), StringComparison.Ordinal);
        Assert.True(new FileInfo(Path.Combine(_dir, "src_out.jpeg")).Length > 0);
    }

    // ───────────────────────── 辅助 ─────────────────────────

    private static OptionField Field(string featureId, string key) => FeatureOptionSchemas
        .GetSections(featureId)
        .SelectMany(s => s.Fields)
        .Single(f => f.Key == key);

    private static string CreateWebp(string path)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(16, 12, SKColorType.Rgba8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(new SKColor(0, 128, 255));
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Webp, 100);
        using FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write);
        data.SaveTo(stream);
        return path;
    }
}
