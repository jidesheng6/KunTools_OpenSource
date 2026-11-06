namespace DocumentTools.Core.Services.Office;

/// <summary>Office / PDF / EPUB format conversion without UI concerns.</summary>
public interface IOfficeConvertService
{
    void WordToPdf(string sourceFile, string savedFile, WordToPdfOptions? options = null);

    void PdfToWord(string sourceFile, string savedFile, PdfToWordOptions? options = null);

    void ExcelToPdf(string sourceFile, string savedFile, ExcelToPdfOptions? options = null);

    /// <summary>
    /// EXCEL→PDF 并返回真实产物路径（`eachSheet` 时一个输入会产生多个 PDF）。
    /// 默认实现只回报单个目标文件，便于既有实现/测试保持编译；
    /// 真实实现按实际写盘结果返回，供调用方避免登记不存在的文件。
    /// </summary>
    IReadOnlyList<string> ExcelToPdfWithOutputs(
        string sourceFile,
        string savedFile,
        ExcelToPdfOptions? options = null)
    {
        ExcelToPdf(sourceFile, savedFile, options);
        return [savedFile];
    }

    void PptToPdf(string sourceFile, string savedFile, PptToPdfOptions? options = null);

    void ImagesToPdf(IEnumerable<string> imageFiles, string savedFile);

    /// <summary>
    /// IMAGE→PDF with legacy Util.ConvertImage2PDF switches (Util.cs:1805-1899).
    /// Declared as a default interface member so existing implementers/tests keep compiling.
    /// </summary>
    void ImagesToPdf(IEnumerable<string> imageFiles, string savedFile, ImagePdfOptions? options)
        => ImagesToPdf(imageFiles, savedFile);

    void PdfToExcel(string sourceFile, string savedFile, bool oneSheetPerDocument = true);

    void PdfToPpt(string sourceFile, string savedFile, PdfToPptOptions? options = null);

    void WordToExcel(string sourceFile, string savedFile, bool exportSingleSheet = true);

    void ExcelToWord(string sourceFile, string savedFile);

    void WordToEpub(string sourceFile, string savedFile, WordToEpubOptions? options = null);

    void EpubToWord(string sourceFile, string savedFile);

    void EpubToPdf(string sourceFile, string savedFile);

    /// <summary>
    /// Export Word / PDF / Excel pages (or sheets) to image files under <paramref name="outputDirectory"/>.
    /// Returns the written image paths.
    /// </summary>
    IReadOnlyList<string> DocumentToImages(
        string sourceFile,
        string outputDirectory,
        string imageExtension = ".png",
        int dpi = 150,
        bool grayscale = false);

    /// <summary>
    /// Same as the 5-argument overload plus legacy PageExport (Util.cs:2445-2549):
    /// <c>false</c> merges the whole Word document into a single tall image.
    /// Declared as a default interface member so existing implementers/tests keep compiling.
    /// </summary>
    IReadOnlyList<string> DocumentToImages(
        string sourceFile,
        string outputDirectory,
        string imageExtension,
        int dpi,
        bool grayscale,
        bool pageExport)
        => DocumentToImages(sourceFile, outputDirectory, imageExtension, dpi, grayscale);

    /// <summary>
    /// Convert PDF to a target standard / compliance version.
    /// <paramref name="pdfFormat"/> accepts Aspose.Pdf.PdfFormat names such as <c>v_1_7</c>, <c>PDF_A_1A</c>.
    /// </summary>
    void PdfToCompliance(string sourceFile, string savedFile, string pdfFormat = "v_1_7");
}
