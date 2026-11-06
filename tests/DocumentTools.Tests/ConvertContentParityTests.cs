using Aspose.Pdf.Text;
using DocumentTools.Core.Services.Office;
using PdfDocument = Aspose.Pdf.Document;
using SlidesPresentation = Aspose.Slides.Presentation;
using WordsDocument = Aspose.Words.Document;

namespace DocumentTools.Tests;

/// <summary>
/// task-26：转换产物的内容级核对（页数 / 幻灯片数 / 工作表数 / 文本标记 / 纸张几何）。
/// 为什么断言结构量而不是字节或总字数：Aspose 评估版会给产物加水印、部分路径有截断，
/// 字节与字数都会失真，只有「页数、工作表数、幻灯片数、页面几何、关键标记是否存在」
/// 这些结构指标能在评估模式下反映真实逻辑。
/// </summary>
public sealed class ConvertContentParityTests : IDisposable
{
    private const double Tolerance = 2.0;

    // A4=LETTER 等纸张在 PDF 坐标里的理论尺寸（pt）。来源：legacy Util.cs:1724-1738 的索引映射。
    private static readonly (int Index, string Name, double Width, double Height)[] Papers =
    [
        (4, "A4", 595.28, 841.89),
        (9, "Letter", 612.0, 792.0),
        (8, "Legal", 612.0, 1008.0),
        (10, "Tabloid", 792.0, 1224.0),
    ];

    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "dt-parity-" + Guid.NewGuid().ToString("N"));

    private readonly OfficeConvertService _convert = new();

    public ConvertContentParityTests() => Directory.CreateDirectory(_root);

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
            // 残留临时目录无副作用，清理失败不算测试失败。
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private string Dir(string name)
    {
        string dir = Path.Combine(_root, name);
        Directory.CreateDirectory(dir);
        return dir;
    }

    [Fact]
    public void WordToPdf_PreservesPageCountAndFirstPageText()
    {
        string dir = Dir("word-pdf");
        string docx = ConvertTestAssets.CreateWord(dir, "PARITY-WORD-P1", "PARITY-WORD-P2", "PARITY-WORD-P3");
        string pdf = Path.Combine(dir, "out.pdf");

        _convert.WordToPdf(docx, pdf);

        using var document = new PdfDocument(pdf);
        int pages = document.Pages.Count;
        Assert.True(pages == 3, $"期望 3 页（输入含 2 个分页符），实际 {pages} 页");

        var absorber = new TextAbsorber();
        document.Pages.Accept(absorber);
        Assert.True(
            absorber.Text.Contains("PARITY-WORD-P1", StringComparison.Ordinal),
            $"PDF 文本未包含首段标记 PARITY-WORD-P1；实际抽取：{Excerpt(absorber.Text)}");
    }

    [Fact]
    public void PdfToWord_OutputTextContainsPageMarkers()
    {
        string dir = Dir("pdf-word");
        string pdf = ConvertTestAssets.CreatePdf(dir, "PARITY-PDFW-P1", "PARITY-PDFW-P2");
        string docx = Path.Combine(dir, "out.docx");

        _convert.PdfToWord(pdf, docx);

        var document = new WordsDocument(docx);
        string text = document.GetText();
        Assert.True(
            text.Contains("PARITY-PDFW-P1", StringComparison.Ordinal)
            && text.Contains("PARITY-PDFW-P2", StringComparison.Ordinal),
            $"输出 Word 未同时包含两页标记；实际抽取：{Excerpt(text)}");
    }

    [Fact]
    public void PdfToExcel_OutputCellsContainPageMarkers()
    {
        string dir = Dir("pdf-excel");
        string pdf = ConvertTestAssets.CreatePdf(dir, "PARITY-PDFX-P1", "PARITY-PDFX-P2");
        string xlsx = Path.Combine(dir, "out.xlsx");

        _convert.PdfToExcel(pdf, xlsx);

        IReadOnlyList<string> cells = ConvertTestAssets.ReadExcelText(xlsx);
        string joined = string.Join(" | ", cells);
        Assert.True(
            joined.Contains("PARITY-PDFX-P1", StringComparison.Ordinal)
            && joined.Contains("PARITY-PDFX-P2", StringComparison.Ordinal),
            $"输出 Excel 未同时包含两页标记；实际单元格：{Excerpt(joined)}");
    }

    [Fact]
    public void ExcelToPdf_EachSheet_WritesOnePdfPerVisibleSheet()
    {
        string dir = Dir("excel-pdf-split");
        string xlsx = ConvertTestAssets.CreateExcel(dir, ("Alpha", "PARITY-SHEET-ALPHA"), ("Beta", "PARITY-SHEET-BETA"));
        string target = Path.Combine(dir, "out.pdf");

        IReadOnlyList<string> outputs = _convert.ExcelToPdfWithOutputs(
            xlsx,
            target,
            new ExcelToPdfOptions { ExportEachSheet = true });

        IReadOnlyList<string> realSheets = ConvertTestAssets.ReadRealSheetNames(xlsx);
        Assert.True(
            outputs.Count >= realSheets.Count,
            $"eachSheet=true 时输出份数应 >= 工作表数 {realSheets.Count}，实际 {outputs.Count} 份：{string.Join(", ", outputs.Select(Path.GetFileName))}");

        int totalPages = 0;
        var text = new System.Text.StringBuilder();
        foreach (string output in outputs)
        {
            Assert.True(File.Exists(output), $"登记了不存在的产物：{output}");
            using var document = new PdfDocument(output);
            totalPages += document.Pages.Count;
            var absorber = new TextAbsorber();
            document.Pages.Accept(absorber);
            text.AppendLine(absorber.Text);
        }

        Assert.True(
            totalPages >= realSheets.Count,
            $"每个工作表至少 1 页，合计应 >= {realSheets.Count} 页，实际 {totalPages} 页");
        Assert.True(
            text.ToString().Contains("PARITY-SHEET-ALPHA", StringComparison.Ordinal)
            && text.ToString().Contains("PARITY-SHEET-BETA", StringComparison.Ordinal),
            $"分表产物未同时包含两个工作表标记；实际抽取：{Excerpt(text.ToString())}");
    }

    [Fact]
    public void ExcelToPdf_SingleFile_ContainsAllSheetMarkers()
    {
        string dir = Dir("excel-pdf-one");
        string xlsx = ConvertTestAssets.CreateExcel(dir, ("Alpha", "PARITY-ONE-ALPHA"), ("Beta", "PARITY-ONE-BETA"));
        string pdf = Path.Combine(dir, "out.pdf");

        _convert.ExcelToPdf(xlsx, pdf, new ExcelToPdfOptions { ExportEachSheet = false });

        Assert.True(File.Exists(pdf), "eachSheet=false 时应产出单个 PDF");
        using var document = new PdfDocument(pdf);
        Assert.True(document.Pages.Count >= 1, $"至少 1 页，实际 {document.Pages.Count} 页");

        var absorber = new TextAbsorber();
        document.Pages.Accept(absorber);
        string text = absorber.Text;
        Assert.True(
            text.Contains("PARITY-ONE-ALPHA", StringComparison.Ordinal)
            && text.Contains("PARITY-ONE-BETA", StringComparison.Ordinal),
            $"单文件产物未同时包含两个工作表标记；实际抽取：{Excerpt(text)}");
    }

    [Fact]
    public void PptToPdf_PreservesSlideCount()
    {
        string dir = Dir("ppt-pdf");
        string pptx = ConvertTestAssets.CreatePptx(dir, 3);
        string pdf = Path.Combine(dir, "out.pdf");

        _convert.PptToPdf(pptx, pdf, new PptToPdfOptions());

        using var document = new PdfDocument(pdf);
        Assert.True(document.Pages.Count == 3, $"期望 3 页（3 张幻灯片），实际 {document.Pages.Count} 页");
    }

    [Fact]
    public void PdfToPpt_PreservesPageCountAsSlides()
    {
        string dir = Dir("pdf-ppt");
        string pdf = ConvertTestAssets.CreatePdf(dir, "PARITY-PPTP-P1", "PARITY-PPTP-P2", "PARITY-PPTP-P3");
        string pptx = Path.Combine(dir, "out.pptx");

        _convert.PdfToPpt(pdf, pptx, new PdfToPptOptions());

        using var presentation = new SlidesPresentation(pptx);
        Assert.True(
            presentation.Slides.Count == 3,
            $"期望 3 张幻灯片（3 页 PDF），实际 {presentation.Slides.Count} 张");
    }

    [Fact]
    public void WordToExcel_MergesIntoSingleWorksheet()
    {
        string dir = Dir("word-excel");
        string docx = ConvertTestAssets.CreateWord(dir, "PARITY-WORDX-MARK");
        string xlsx = Path.Combine(dir, "out.xlsx");

        _convert.WordToExcel(docx, xlsx, exportSingleSheet: true);

        IReadOnlyList<string> realSheets = ConvertTestAssets.ReadRealSheetNames(xlsx);
        Assert.True(
            realSheets.Count == 1,
            $"exportSingleSheet=true 应合并为 1 个工作表，实际 {realSheets.Count} 个：{string.Join(", ", realSheets)}");

        string joined = string.Join(" | ", ConvertTestAssets.ReadExcelText(xlsx));
        Assert.True(
            joined.Contains("PARITY-WORDX-MARK", StringComparison.Ordinal),
            $"输出单元格未包含源文档标记；实际单元格：{Excerpt(joined)}");
    }

    [Fact]
    public async Task HtmlToPdf_PaperIndexMapsToLegacyPageGeometry()
    {
        string browser = FindBrowser();
        if (browser.Length == 0)
        {
            Assert.Fail("本机没有 Chrome/Edge 可执行文件：HTML→PDF 的纸张几何无法验证（不假装通过）");
        }

        foreach ((int index, string name, double width, double height) in Papers)
        {
            string dir = Dir("html-paper-" + index);
            string html = ConvertTestAssets.CreateHtml(dir, "<h1>PARITY-HTML</h1>");
            string pdf = Path.Combine(dir, "out.pdf");

            var service = new HtmlToPdfService();
            await service.ConvertAsync(html, pdf, new HtmlToPdfOptions
            {
                BrowserExecutablePath = browser,
                PaperSizeIndex = index,
                Landscape = false,
                PreferCssPageSize = false,
                MarginTopPx = "0",
                MarginLeftPx = "0",
                MarginBottomPx = "0",
                MarginRightPx = "0",
            });

            using var document = new PdfDocument(pdf);
            Aspose.Pdf.Page page = document.Pages[1];
            double actualWidth = page.Rect.Width;
            double actualHeight = page.Rect.Height;
            Assert.True(
                Math.Abs(actualWidth - width) <= Tolerance && Math.Abs(actualHeight - height) <= Tolerance,
                $"{name}（索引 {index}）期望 {width}x{height}pt，实际 {actualWidth:F2}x{actualHeight:F2}pt");
        }
    }

    [Fact]
    public async Task HtmlToPdf_LandscapeSwapsPageGeometry()
    {
        string browser = FindBrowser();
        if (browser.Length == 0)
        {
            Assert.Fail("本机没有 Chrome/Edge 可执行文件：HTML→PDF 的纸张几何无法验证（不假装通过）");
        }

        string dir = Dir("html-landscape");
        string html = ConvertTestAssets.CreateHtml(dir, "<h1>PARITY-HTML-LANDSCAPE</h1>");
        string pdf = Path.Combine(dir, "out.pdf");

        var service = new HtmlToPdfService();
        await service.ConvertAsync(html, pdf, new HtmlToPdfOptions
        {
            BrowserExecutablePath = browser,
            PaperSizeIndex = 4,
            Landscape = true,
            PreferCssPageSize = false,
        });

        using var document = new PdfDocument(pdf);
        Aspose.Pdf.Page page = document.Pages[1];
        double width = page.Rect.Width;
        double height = page.Rect.Height;
        Assert.True(
            Math.Abs(width - 841.89) <= Tolerance && Math.Abs(height - 595.28) <= Tolerance,
            $"A4 横向期望 841.89x595.28pt，实际 {width:F2}x{height:F2}pt");
    }

    [Fact]
    public void ImagesToPdf_DefaultOrientationUsesImagePixels()
    {
        string dir = Dir("image-pdf-default");
        string png = ConvertTestAssets.CreatePng(dir, 300, 500);
        string pdf = Path.Combine(dir, "out.pdf");

        _convert.ImagesToPdf([png], pdf, new ImagePdfOptions { LandStyleIndex = 0 });

        using var document = new PdfDocument(pdf);
        Assert.True(document.Pages.Count == 1, $"单图应为 1 页，实际 {document.Pages.Count} 页");
        Aspose.Pdf.Page page = document.Pages[1];
        double width = page.Rect.Width;
        double height = page.Rect.Height;
        Assert.True(
            Math.Abs(width - 300) <= Tolerance && Math.Abs(height - 500) <= Tolerance,
            $"orientation=0 应取图片像素 300x500pt，实际 {width:F2}x{height:F2}pt");
    }

    [Fact]
    public void ImagesToPdf_OrientationSwitchChangesPageGeometry()
    {
        string dir = Dir("image-pdf-orientation");
        string png = ConvertTestAssets.CreatePng(dir, 300, 500);
        string landscapePdf = Path.Combine(dir, "landscape.pdf");
        string portraitPdf = Path.Combine(dir, "portrait.pdf");

        _convert.ImagesToPdf([png], landscapePdf, new ImagePdfOptions { LandStyleIndex = 1 });
        _convert.ImagesToPdf([png], portraitPdf, new ImagePdfOptions { LandStyleIndex = 2 });

        using var landscape = new PdfDocument(landscapePdf);
        double landscapeWidth = landscape.Pages[1].Rect.Width;
        double landscapeHeight = landscape.Pages[1].Rect.Height;
        Assert.True(
            landscapeWidth > landscapeHeight,
            $"orientation=1 应为横向（宽>高），实际 {landscapeWidth:F2}x{landscapeHeight:F2}pt");

        using var portrait = new PdfDocument(portraitPdf);
        double portraitWidth = portrait.Pages[1].Rect.Width;
        double portraitHeight = portrait.Pages[1].Rect.Height;
        Assert.True(
            portraitWidth < portraitHeight,
            $"orientation=2 应为纵向（宽<高），实际 {portraitWidth:F2}x{portraitHeight:F2}pt");
    }

    /// <summary>与 HtmlToPdfOperation 一致：优先 Chrome，其次 Edge；都找不到返回空串。</summary>
    private static string FindBrowser()
    {
        string[] candidates =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft", "Edge", "Application", "msedge.exe"),
        ];

        return candidates.FirstOrDefault(File.Exists) ?? string.Empty;
    }

    private static string Excerpt(string text)
    {
        string flattened = text.Replace("\r", " ").Replace("\n", " ").Trim();
        return flattened.Length <= 180 ? flattened : flattened[..180] + "…";
    }
}
