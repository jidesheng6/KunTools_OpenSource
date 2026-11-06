using System.Drawing;
using DrawingImageFormat = System.Drawing.Imaging.ImageFormat;
using System.Drawing.Imaging;
using Aspose.Cells;
using Aspose.Pdf.Text;
using Aspose.Slides;
using CellsWorkbook = Aspose.Cells.Workbook;
using PdfDocument = Aspose.Pdf.Document;
using SlidesPresentation = Aspose.Slides.Presentation;
using WordsDocument = Aspose.Words.Document;

namespace DocumentTools.Tests;

/// <summary>
/// 内容级核对用的夹具生成器（task-26）。
/// 全部在临时目录现造，不依赖 testdata/；文本标记统一用 ASCII，
/// 避免评估版字体替换影响文本抽取结果。
/// </summary>
internal static class ConvertTestAssets
{
    /// <summary>生成 N 页 Word（页间插分页符），每页写入一个 pageMarker。</summary>
    public static string CreateWord(string directory, params string[] pageMarkers)
    {
        string path = Path.Combine(directory, "input.docx");
        var document = new WordsDocument();
        var builder = new Aspose.Words.DocumentBuilder(document);
        for (int i = 0; i < pageMarkers.Length; i++)
        {
            if (i > 0)
            {
                builder.InsertBreak(Aspose.Words.BreakType.PageBreak);
            }

            builder.Writeln(pageMarkers[i]);
        }

        document.Save(path, Aspose.Words.SaveFormat.Docx);
        return path;
    }

    /// <summary>生成 N 页 PDF，每页写入一个 pageMarker。</summary>
    public static string CreatePdf(string directory, params string[] pageMarkers)
    {
        string path = Path.Combine(directory, "input.pdf");
        using var document = new PdfDocument();
        foreach (string marker in pageMarkers)
        {
            var page = document.Pages.Add();
            page.Paragraphs.Add(new TextFragment(marker));
        }

        document.Save(path);
        return path;
    }

    /// <summary>生成多工作表 Excel，每个工作表 A1 写入一条 sheetMarker。</summary>
    public static string CreateExcel(string directory, params (string Name, string Marker)[] sheets)
    {
        string path = Path.Combine(directory, "input.xlsx");
        using var workbook = new CellsWorkbook();
        for (int i = 0; i < sheets.Length; i++)
        {
            Worksheet worksheet = i == 0 ? workbook.Worksheets[0] : workbook.Worksheets[workbook.Worksheets.Add()];
            worksheet.Name = sheets[i].Name;
            worksheet.Cells["A1"].PutValue(sheets[i].Marker);
        }

        workbook.Save(path, SaveFormat.Xlsx);
        return path;
    }

    /// <summary>生成 N 张幻灯片的 PPTX，每张一个文本框。</summary>
    public static string CreatePptx(string directory, int slideCount)
    {
        string path = Path.Combine(directory, "input.pptx");
        using var presentation = new SlidesPresentation();
        for (int i = 0; i < slideCount; i++)
        {
            ISlide slide = i == 0
                ? presentation.Slides[0]
                : presentation.Slides.AddEmptySlide(presentation.LayoutSlides[0]);

            IAutoShape box = slide.Shapes.AddAutoShape(ShapeType.Rectangle, 50, 50, 400, 120);
            box.TextFrame.Text = $"SLIDE-MARK-{i + 1}";
        }

        presentation.Save(path, Aspose.Slides.Export.SaveFormat.Pptx);
        return path;
    }

    /// <summary>生成一份最小 HTML（本地文件，Chrome 以 file:// 打开）。</summary>
    public static string CreateHtml(string directory, string body)
    {
        string path = Path.Combine(directory, "input.html");
        File.WriteAllText(
            path,
            $"<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>parity</title></head><body>{body}</body></html>");
        return path;
    }

    /// <summary>生成指定像素尺寸的 PNG。</summary>
    public static string CreatePng(string directory, int width, int height, string fileName = "input.png")
    {
        string path = Path.Combine(directory, fileName);
        using var bitmap = new Bitmap(width, height);
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            graphics.Clear(Color.LightSteelBlue);
            graphics.DrawString("IMG", SystemFonts.DefaultFont, Brushes.Black, 4, 4);
        }

        bitmap.Save(path, DrawingImageFormat.Png);
        return path;
    }

    /// <summary>抽取 PDF 全文（各页拼接），用于断言关键标记是否真的进了产物。</summary>
    public static string ReadPdfText(string pdfPath)
    {
        using var document = new PdfDocument(pdfPath);
        var absorber = new TextAbsorber();
        document.Pages.Accept(absorber);
        return absorber.Text;
    }

    /// <summary>抽取 Word 全文。</summary>
    public static string ReadWordText(string docxPath)
    {
        var document = new WordsDocument(docxPath);
        return document.GetText();
    }

    /// <summary>抽取 Excel 所有工作表的单元格文本，逐行 “工作表名: 值”。</summary>
    public static IReadOnlyList<string> ReadExcelText(string xlsxPath)
    {
        using var workbook = new CellsWorkbook(xlsxPath);
        var lines = new List<string>();
        foreach (Worksheet worksheet in workbook.Worksheets)
        {
            foreach (Aspose.Cells.Cell cell in worksheet.Cells)
            {
                string value = cell.StringValue;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    lines.Add($"{worksheet.Name}: {value}");
                }
            }
        }

        return lines;
    }

    /// <summary>
    /// Aspose.Cells 评估版会往工作簿里插一张 “Evaluation Warning” 工作表，属评估产物，
    /// 断言“工作表数”时必须先排除它，否则测的是 Aspose 而不是我们的逻辑。
    /// </summary>
    public static bool IsEvaluationSheet(string sheetName)
        => sheetName.Contains("Evaluation", StringComparison.OrdinalIgnoreCase)
           || sheetName.Contains("评估", StringComparison.Ordinal);

    /// <summary>返回除评估产物以外的工作表名。</summary>
    public static IReadOnlyList<string> ReadRealSheetNames(string xlsxPath)
    {
        using var workbook = new CellsWorkbook(xlsxPath);
        return workbook.Worksheets.Cast<Worksheet>()
            .Select(sheet => sheet.Name)
            .Where(name => !IsEvaluationSheet(name))
            .ToList();
    }
}
