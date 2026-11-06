namespace DocumentTools.Core.Services.Code;

public interface IBarcodeService
{
    /// <summary>Returns PNG bytes using full render options.</summary>
    byte[] CreatePng(string content, BarcodeRenderOptions? options = null);

    /// <summary>
    /// Convenience overload.
    /// <paramref name="symbology"/>: QR / DataMatrix / Pdf417 / Aztec / Code128 / Code39.
    /// </summary>
    byte[] CreatePng(
        string content,
        string symbology,
        int moduleSize = 4,
        string? logoFile = null);

    /// <summary>Returns PNG bytes for a QR code.</summary>
    byte[] CreateQrCodePng(string content, int moduleSize = 4);

    /// <summary>
    /// Reads every barcode text found in an image file (legacy <c>Util.ReadBarCodeContent</c>).
    /// Returns an empty list when nothing decodes, so the caller owns the wording.
    /// </summary>
    IReadOnlyList<string> ReadTexts(string imageFile);

    /// <summary>Exports an .xlsx template with 编号 / 标题 / 内容 columns.</summary>
    void ExportBatchTemplate(string outputFile, IReadOnlyList<BarcodeBatchRow>? rows = null);

    /// <summary>Reads the first worksheet containing the legacy 编号 / 标题 / 内容 columns.</summary>
    IReadOnlyList<BarcodeBatchRow> ReadBatchTemplate(string templateFile);

    /// <summary>Renders template rows using each row's file name, caption and barcode content.</summary>
    BarcodeBatchResult GenerateBatch(
        IReadOnlyList<BarcodeBatchRow> rows,
        string outputDirectory,
        BarcodeRenderOptions options,
        BarcodeBatchImageFormat format = BarcodeBatchImageFormat.Png);
}
