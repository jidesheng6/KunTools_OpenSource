using DocumentTools.Core.Features.Options;

namespace DocumentTools.Tests;

/// <summary>
/// 补齐的选项键契约（legacy 控件名 → Schema 键）。默认值一律对齐 legacy 控件初始选中项。
/// </summary>
public class FeatureOptionSchemaAddedKeysTests
{
    private static Dictionary<string, OptionField> Fields(string featureId)
        => FeatureOptionSchemas.GetSections(featureId)
            .SelectMany(s => s.Fields)
            .ToDictionary(f => f.Key, f => f);

    [Fact]
    public void PptSplit_ExposesSlideSizeAndScale()
    {
        var fields = Fields("ppt.split");

        Assert.Equal(OptionFieldKind.Choice, fields["slideSize"].Kind);
        Assert.Equal("OnScreen", fields["slideSize"].DefaultValue);
        Assert.Equal(17, fields["slideSize"].Choices!.Count);

        Assert.Equal(OptionFieldKind.Choice, fields["slideScale"].Kind);
        Assert.Equal("Maximize", fields["slideScale"].DefaultValue);
        Assert.Equal("EnsureFit", fields["slideScale"].Choices![1].Value);
        Assert.Equal("DoNotScale", fields["slideScale"].Choices![2].Value); // legacy 显示顺序：最大化/合适大小/不缩放
    }

    [Fact]
    public void PdfSplit_ExposesDeleteRangeAsText()
    {
        var fields = Fields("pdf.split");

        Assert.Equal(OptionFieldKind.Text, fields["deleteStart"].Kind);
        Assert.Equal(OptionFieldKind.Text, fields["deleteEnd"].Kind);
        Assert.Equal(string.Empty, fields["deleteStart"].DefaultValue); // 留空 = 不删除
    }

    [Fact]
    public void PdfCompress_ExposesImageVersionAndResourceFlags()
    {
        var fields = Fields("pdf.compress");

        // legacy CompressPDF_ImageQuailty 是整数语义（Util.cs:1465），与 image.compress 统一为 Text。
        Assert.Equal(OptionFieldKind.Text, fields["quality"].Kind);
        Assert.Equal("80", fields["quality"].DefaultValue);

        Assert.Equal(OptionFieldKind.Choice, fields["imageVersion"].Kind);
        Assert.Equal("Standard", fields["imageVersion"].DefaultValue);
        Assert.Equal(new[] { "Standard", "Fast", "Mixed" },
            fields["imageVersion"].Choices!.Select(c => c.Value).ToArray());

        foreach (string key in new[] { "removeUnusedStreams", "linkDuplicateStreams", "removeUnusedObjects" })
        {
            Assert.Equal(OptionFieldKind.Check, fields[key].Kind);
            Assert.Equal("true", fields[key].DefaultValue); // 保持迁移前压缩强度（Lead 决策）
        }
    }

    [Fact]
    public void PdfWatermark_ExposesAlignmentImageSizeAndCustomPosition()
    {
        var fields = Fields("pdf.watermark.add");

        Assert.Equal("0.5", fields["opacity"].DefaultValue); // legacy 不透明度 SelectedIndex=5
        Assert.Equal(OptionFieldKind.Text, fields["fontColor"].Kind);
        Assert.Equal("#FF0000", fields["fontColor"].DefaultValue);

        Assert.Equal(OptionFieldKind.Choice, fields["horizontal"].Kind);
        Assert.Equal("Center", fields["horizontal"].DefaultValue);
        Assert.Equal("FullJustify", fields["horizontal"].Choices!.Last().Value);
        Assert.Equal(OptionFieldKind.Choice, fields["vertical"].Kind);
        Assert.Equal("Center", fields["vertical"].DefaultValue);
        Assert.Equal(4, fields["vertical"].Choices!.Count);

        Assert.Equal("100", fields["imageWidth"].DefaultValue);
        Assert.Equal("100", fields["imageHeight"].DefaultValue);
        Assert.Equal("false", fields["customPosition"].DefaultValue);
        Assert.Equal("0", fields["positionX"].DefaultValue);
        Assert.Equal("0", fields["positionY"].DefaultValue);
        Assert.Equal("25", fields["repeatSpacingX"].DefaultValue);
        Assert.Equal("25", fields["repeatSpacingY"].DefaultValue);
    }

    [Fact]
    public void HtmlPdf_ExposesElevenPapersLandscapeAndNavigationTimeout()
    {
        var fields = Fields("html.pdf");

        OptionField paper = fields["paper"];
        Assert.Equal(OptionFieldKind.Choice, paper.Kind);
        Assert.Equal(11, paper.Choices!.Count);
        Assert.Equal("A4", paper.Choices![4].Label);
        Assert.Equal("4", paper.DefaultValue);
        Assert.Equal("0", paper.Choices![0].Value);       // 数字串，HtmlToPdfOperation 走 int.TryParse
        Assert.Equal("Tabloid", paper.Choices![10].Label);

        Assert.Equal(OptionFieldKind.Check, fields["landscape"].Kind);
        Assert.Equal("false", fields["landscape"].DefaultValue);
        Assert.Equal(OptionFieldKind.Text, fields["navTimeout"].Kind);
        Assert.Equal("30", fields["navTimeout"].DefaultValue);
    }

    [Fact]
    public void ImagePdf_ExposesAutoOptimizeOrientationAndMargins()
    {
        var fields = Fields("image.pdf");

        Assert.Equal(OptionFieldKind.Check, fields["autoOptimize"].Kind);
        Assert.Equal("false", fields["autoOptimize"].DefaultValue);
        Assert.Equal(OptionFieldKind.Choice, fields["orientation"].Kind);
        Assert.Equal("0", fields["orientation"].DefaultValue);
        Assert.Equal(3, fields["orientation"].Choices!.Count);

        foreach (string key in new[] { "marginTop", "marginLeft", "marginBottom", "marginRight" })
        {
            Assert.Equal(OptionFieldKind.Text, fields[key].Kind);
            Assert.Equal("0", fields[key].DefaultValue);
        }
    }

    [Fact]
    public void ImageCompress_ExposesJpegAndPngSections()
    {
        var fields = Fields("image.compress");

        Assert.Equal(OptionFieldKind.Text, fields["quality"].Kind);
        Assert.Equal("80", fields["quality"].DefaultValue); // legacy CompressJpegQuailty_Combox
        Assert.Equal(OptionFieldKind.Choice, fields["jpegColorMode"].Kind);
        Assert.Equal("Rgb", fields["jpegColorMode"].DefaultValue);

        Assert.Equal(OptionFieldKind.Choice, fields["pngLevel"].Kind);
        Assert.Equal("3", fields["pngLevel"].DefaultValue);
        Assert.Equal(10, fields["pngLevel"].Choices!.Count);
        Assert.Equal(OptionFieldKind.Choice, fields["pngColorType"].Kind);
        Assert.Equal("Truecolor", fields["pngColorType"].DefaultValue);

        Assert.Contains(fields["format"].Choices!, c => c.Value == "JPEG");
        Assert.Contains(fields["format"].Choices!, c => c.Value == "BMP");
    }

    [Fact]
    public void ImageIco_Exposes256AndMultiSize()
    {
        var fields = Fields("image.ico");

        Assert.Contains(fields["format"].Choices!, c => c.Value == "ICO 256");
        Assert.Equal(OptionFieldKind.Check, fields["multiSize"].Kind);
        Assert.Equal("false", fields["multiSize"].DefaultValue);
    }
}
