using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Code;
using DocumentTools.Core.Services.Image;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// task-13 收口项：P1-7 假参数裁除、P2-1 条码外观、P2-7 image.compress 补充开关、
/// P2-8 备注位置 Choice、P2-9 忽略脚注。
/// </summary>
public class Task13OptionTests
{
    private static Dictionary<string, OptionField> Fields(string featureId)
        => FeatureOptionSchemas.GetSections(featureId)
            .SelectMany(s => s.Fields)
            .ToDictionary(f => f.Key, f => f);

    // P1-7：refreshPageNumber 只在 word 分支真正生效，pdf/ppt 不再暴露假参数。
    [Fact]
    public void RefreshPageNumber_OnlyWordSplitExposesIt()
    {
        Assert.True(Fields("word.split").ContainsKey("refreshPageNumber"));
        Assert.False(Fields("pdf.split").ContainsKey("refreshPageNumber"));
        Assert.False(Fields("ppt.split").ContainsKey("refreshPageNumber"));
    }

    // P2-1：条码外观键。
    [Fact]
    public void Barcode_ExposesSizeAndRotationKeys()
    {
        var fields = Fields("code.barcode");

        OptionField sizeMode = fields["sizeMode"];
        Assert.Equal(OptionFieldKind.Choice, sizeMode.Kind);
        Assert.Equal("auto", sizeMode.DefaultValue);
        Assert.Equal(["auto", "fixed"], sizeMode.Choices!.Select(c => c.Value));

        OptionField rotate = fields["rotate"];
        Assert.Equal(OptionFieldKind.Choice, rotate.Kind);
        Assert.Equal("0", rotate.DefaultValue);
        Assert.Equal(["0", "90", "180", "270"], rotate.Choices!.Select(c => c.Value));

        Assert.Equal(OptionFieldKind.Text, fields["barcodeWidth"].Kind);
        Assert.Equal("0", fields["barcodeWidth"].DefaultValue);
        Assert.Equal(OptionFieldKind.Text, fields["barcodeHeight"].Kind);
        Assert.Equal("0", fields["barcodeHeight"].DefaultValue);
    }

    [Fact]
    public void Barcode_Rotate90_SwapsOutputDimensions()
    {
        var service = new BarcodeService();
        byte[] upright = service.CreatePng("DT-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 2
        });
        byte[] rotated = service.CreatePng("DT-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 2,
            Rotate = 90
        });

        (int uprightWidth, int uprightHeight) = ReadPngSize(upright);
        (int rotatedWidth, int rotatedHeight) = ReadPngSize(rotated);

        Assert.Equal(uprightHeight, rotatedWidth);
        Assert.Equal(uprightWidth, rotatedHeight);
    }

    [Fact]
    public void Barcode_Rotate180_KeepsDimensions()
    {
        var service = new BarcodeService();
        byte[] upright = service.CreatePng("DT-2026", new BarcodeRenderOptions { Symbology = "Code39", ModuleSize = 2 });
        byte[] rotated = service.CreatePng("DT-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 2,
            Rotate = 180
        });

        Assert.Equal(ReadPngSize(upright), ReadPngSize(rotated));
    }

    [Fact]
    public void Barcode_FixedWidth_ProducesRequestedWidth()
    {
        var service = new BarcodeService();
        byte[] png = service.CreatePng("DT-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 2,
            SizeMode = "fixed",
            BarcodeWidth = 300
        });

        (int width, _) = ReadPngSize(png);
        Assert.Equal(300, width);
    }

    [Fact]
    public void Barcode_FixedSize_ProducesRequestedBox()
    {
        var service = new BarcodeService();
        byte[] png = service.CreatePng("DT-2026", new BarcodeRenderOptions
        {
            Symbology = "QR",
            SizeMode = "fixed",
            BarcodeWidth = 240,
            BarcodeHeight = 180
        });

        Assert.Equal((240, 180), ReadPngSize(png));
    }

    // P2-7：image.compress 的补充开关。
    [Fact]
    public void ImageCompress_ExposesSaveSwitchesAndBmpBits()
    {
        var fields = Fields("image.compress");

        Assert.Equal(OptionFieldKind.Check, fields["useSourceExtension"].Kind);
        // 有意偏离 legacy 默认（legacy 未勾 = 沿用源后缀，但那时格式下拉被禁用）：
        // 面板没有跨字段禁用能力，默认不勾才能让「输出格式」如实生效。
        Assert.Equal("false", fields["useSourceExtension"].DefaultValue);

        Assert.Equal(OptionFieldKind.Check, fields["noCompress"].Kind);
        Assert.Equal("false", fields["noCompress"].DefaultValue); // legacy CompressIamge_NoCompress 未勾

        OptionField bits = fields["bmpPixelBits"];
        Assert.Equal(OptionFieldKind.Choice, bits.Kind);
        Assert.Equal("24", bits.DefaultValue); // legacy CompressBmpPixeBits_Combox SelectedIndex=3
        Assert.Equal(["4", "8", "16", "24", "32"], bits.Choices!.Select(c => c.Value));
    }

    [Fact]
    public void ImageCompress_NoCompress_CopiesSourceBytesVerbatim()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 24, 18);
            byte[] original = File.ReadAllBytes(source);
            var service = new ImageConversionService();

            string output = service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "JPG",
                NoCompress = true,
                UseSourceExtension = false
            });

            Assert.Equal(original, File.ReadAllBytes(output));
        });
    }

    [Fact]
    public void ImageCompress_UseSourceExtension_KeepsSourceExtension()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 24, 18);
            var service = new ImageConversionService();

            string kept = service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "JPG",
                UseSourceExtension = true
            });
            Assert.EndsWith("_out.png", kept, StringComparison.Ordinal);
            Assert.Equal("IHDR", System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(kept), 12, 4));

            string converted = service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "JPG",
                UseSourceExtension = false
            });
            Assert.EndsWith("_out.jpg", converted, StringComparison.Ordinal);
        });
    }

    [Theory]
    [InlineData(24, 54, 0)]
    [InlineData(32, 54, 0)]
    [InlineData(16, 54, 0)]
    [InlineData(8, 54 + 1024, 256)]
    [InlineData(4, 54 + 64, 16)]
    public void Bmp_RespectsPixelBits(int bits, int pixelOffset, int paletteCount)
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 12, 8);
            var service = new ImageConversionService();

            string output = service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "BMP",
                UseSourceExtension = false,
                BmpPixelBits = bits
            });

            byte[] bmp = File.ReadAllBytes(output);
            Assert.Equal((byte)'B', bmp[0]);
            Assert.Equal((byte)'M', bmp[1]);
            Assert.Equal(bits, ReadInt16(bmp, 28));
            Assert.Equal(0, ReadInt32(bmp, 30)); // BI_RGB
            Assert.Equal(pixelOffset, ReadInt32(bmp, 10));
            Assert.Equal(paletteCount, ReadInt32(bmp, 46));

            int stride = (12 * bits + 31) / 32 * 4;
            Assert.Equal(pixelOffset + (stride * 8), bmp.Length);
            Assert.Equal(bmp.Length, ReadInt32(bmp, 2));
        });
    }

    // P2-8：备注导出位置升级为 Choice。
    // 只有三档：Aspose.Slides 25.12 的 NotesPositions 枚举就是 None/BottomFull/BottomTruncated，
    // 早先的「右侧占位（RightFull）」档会被 Enum.TryParse 判失败后静默按 None 导出（备注全丢），已收窄。
    [Fact]
    public void PptPdf_ExportNotesIsPositionChoice()
    {
        OptionField notes = Fields("ppt.pdf")["exportNotes"];

        Assert.Equal(OptionFieldKind.Choice, notes.Kind);
        Assert.Equal("None", notes.DefaultValue);
        Assert.Equal(
            ["None", "BottomFull", "BottomTruncated"],
            notes.Choices!.Select(c => c.Value));
        Assert.Equal(["不导出", "页底完整", "页底截断"], notes.Choices!.Select(c => c.Label));
        Assert.DoesNotContain(notes.Choices!, c => c.Value == "RightFull");
    }

    // P2-9：替换页「忽略脚注」——只对 Word 有意义（Aspose.Words FindReplaceOptions.IgnoreFootnotes），
    // Excel 工作簿没有脚注概念，因此不在 excel.replace 上暴露（task-13 就是收假参数）。
    [Fact]
    public void Replace_ExposesIgnoreFootnotesForWordOnly()
    {
        OptionField field = Fields("word.replace")["ignoreFootnotes"];
        Assert.Equal(OptionFieldKind.Check, field.Kind);
        Assert.Equal("false", field.DefaultValue);

        Assert.False(Fields("excel.replace").ContainsKey("ignoreFootnotes"));
    }

    private static (int Width, int Height) ReadPngSize(byte[] png)
    {
        Assert.Equal("IHDR", System.Text.Encoding.ASCII.GetString(png, 12, 4));
        return (ReadInt32BigEndian(png, 16), ReadInt32BigEndian(png, 20));
    }

    private static int ReadInt32BigEndian(byte[] buffer, int offset)
        => (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];

    private static int ReadInt16(byte[] buffer, int offset)
        => buffer[offset] | (buffer[offset + 1] << 8);

    private static int ReadInt32(byte[] buffer, int offset)
        => buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);

    private static void Run(Action<string> body)
    {
        string dir = Directory.CreateTempSubdirectory("documenttools-task13-").FullName;
        try
        {
            body(dir);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    /// <summary>现造一张小 PNG 作为唯一素材（不依赖仓库图片）。</summary>
    private static string CreateSourcePng(string directory, int width, int height)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, new SKColor(
                    (byte)(x * 255 / Math.Max(1, width - 1)),
                    (byte)(y * 255 / Math.Max(1, height - 1)),
                    120,
                    255));
            }
        }

        string path = Path.Combine(directory, "sample.png");
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using (FileStream fs = File.Open(path, FileMode.Create, FileAccess.Write))
        {
            data.SaveTo(fs);
        }

        return path;
    }
}
