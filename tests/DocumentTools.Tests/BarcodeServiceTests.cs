using DocumentTools.Core.Services.Code;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// Covers the barcode reader added for P1-3 (legacy <c>Util.ReadBarCodeContent</c>).
/// Generation and recognition both run through Aspose.BarCode, so the round-trip proves the
/// wiring even on an unlicensed build (which stamps a watermark but still decodes our own code).
/// </summary>
public sealed class BarcodeServiceTests
{
    private readonly BarcodeService _service = new();

    [Fact]
    public void ReadTexts_RoundTripsCode39Content()
    {
        // Code39 is the one symbology the Aspose evaluation reader decodes verbatim,
        // so an exact round-trip assertion stays valid with or without a license.
        const string content = "DOC-2026";
        byte[] png = _service.CreatePng(content, new BarcodeRenderOptions { Symbology = "Code39", ModuleSize = 4 });
        string file = WriteTempImage(png);

        try
        {
            Assert.Equal([content], _service.ReadTexts(file));
        }
        finally
        {
            Delete(file);
        }
    }

    [Fact]
    public void ReadTexts_DecodesGeneratedQrCode()
    {
        // The evaluation reader appends its own watermark sentence to QR results, so assert the
        // payload prefix instead of equality; a licensed build returns the payload unchanged.
        const string content = "DocumentTools-2026";
        string file = WriteTempImage(_service.CreateQrCodePng(content, moduleSize: 8));

        try
        {
            IReadOnlyList<string> texts = _service.ReadTexts(file);

            Assert.NotEmpty(texts);
            Assert.Contains(texts, t => t.StartsWith("DocumentTools", StringComparison.Ordinal));
        }
        finally
        {
            Delete(file);
        }
    }

    [Fact]
    public void ReadTexts_ReturnsEmptyListForBlankImage()
    {
        string file = WriteTempImage(CreateBlankPng(240, 160));

        try
        {
            Assert.Empty(_service.ReadTexts(file));
        }
        finally
        {
            Delete(file);
        }
    }

    [Fact]
    public void ReadTexts_ThrowsWhenFileMissing()
    {
        string file = Path.Combine(Path.GetTempPath(), "dt-missing-" + Guid.NewGuid().ToString("N") + ".png");

        Assert.Throws<FileNotFoundException>(() => _service.ReadTexts(file));
    }

    [Fact]
    public void ReadTexts_ThrowsOnBlankPath()
    {
        Assert.Throws<ArgumentException>(() => _service.ReadTexts("   "));
    }

    private static byte[] CreateBlankPng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }

    private static string WriteTempImage(byte[] bytes)
    {
        string file = Path.Combine(Path.GetTempPath(), "dt-barcode-" + Guid.NewGuid().ToString("N") + ".png");
        File.WriteAllBytes(file, bytes);
        return file;
    }

    private static void Delete(string file)
    {
        try
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
        catch (IOException)
        {
            // A leftover temp file is harmless; never fail the assertion because cleanup failed.
        }
    }
}
