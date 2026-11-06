using Aspose.BarCode.BarCodeRecognition;
using Aspose.Words;
using Aspose.Words.Saving;
using DocumentTools.Core.Common;
using PdfDocument = Aspose.Pdf.Document;

namespace DocumentTools.Core.Services.Code;

/// <summary>Port of legacy <c>Util.ReadDocumentDataMatrix</c> without WinForms TreeView.</summary>
public sealed class DataMatrixService : IDataMatrixService
{
    public DataMatrixDocumentResult ReadDocument(string documentFile, int dpi = 300)
    {
        PathGuard.RequireExistingFile(documentFile, nameof(documentFile));
        dpi = Math.Clamp(dpi, 72, 600);

        string extension = Path.GetExtension(documentFile);
        var pages = new List<DataMatrixPageResult>();

        if (extension.Equals(".doc", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
        {
            var doc = new Document(documentFile);
            for (int pageIndex = 0; pageIndex < doc.PageCount; pageIndex++)
            {
                using var ms = new MemoryStream();
                var imageOptions = new ImageSaveOptions(SaveFormat.Png)
                {
                    PageSet = new PageSet(pageIndex),
                    Resolution = dpi
                };
                doc.Save(ms, imageOptions);
                ms.Position = 0;
                pages.Add(new DataMatrixPageResult(pageIndex + 1, DecodePage(ms)));
            }
        }
        else if (extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            using var pdf = new PdfDocument(documentFile);
            var resolution = new Aspose.Pdf.Devices.Resolution(dpi);
            var jpeg = new Aspose.Pdf.Devices.JpegDevice(resolution, 100);
            for (int pageIndex = 1; pageIndex <= pdf.Pages.Count; pageIndex++)
            {
                using var ms = new MemoryStream();
                jpeg.Process(pdf.Pages[pageIndex], ms);
                ms.Position = 0;
                pages.Add(new DataMatrixPageResult(pageIndex, DecodePage(ms)));
            }
        }
        else
        {
            throw new NotSupportedException("仅支持 Word（.doc/.docx）或 PDF。");
        }

        return new DataMatrixDocumentResult(documentFile, pages);
    }

    private static List<DataMatrixFinding> DecodePage(Stream imageStream)
    {
        var findings = new List<DataMatrixFinding>();
        using var reader = new BarCodeReader(imageStream, DecodeType.DataMatrix);
        foreach (BarCodeResult result in reader.ReadBarCodes())
        {
            string text = result.CodeText ?? string.Empty;
            if (string.Equals(text, "A01", StringComparison.Ordinal))
            {
                findings.Add(new DataMatrixFinding("表格类型", "测评"));
            }
            else if (string.Equals(text, "B", StringComparison.Ordinal))
            {
                findings.Add(new DataMatrixFinding("表格类型", "选举/推荐"));
            }
            else if (text.StartsWith('T'))
            {
                findings.Add(new DataMatrixFinding("页面编码", text.TrimStart('T')));
            }
            else if (text.Length > 0)
            {
                findings.Add(new DataMatrixFinding("单人码编码", text));
            }
        }

        return findings;
    }
}
