using DocumentTools.Core.Common;
using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>
/// Strategy for pdf.split / word.split / ppt.split.
/// Maps Options → PdfSplitOptions / WordSplitOptions / SlideSplitOptions → Services.
/// </summary>
public sealed class SplitDocumentOperation : IDocumentOperation
{
    private readonly IDocumentMergeSplitService _mergeSplit;
    private readonly IPdfSplitService _pdfSplit;

    public SplitDocumentOperation(
        string featureId,
        IDocumentMergeSplitService mergeSplit,
        IPdfSplitService pdfSplit)
    {
        FeatureId = featureId;
        _mergeSplit = mergeSplit;
        _pdfSplit = pdfSplit;
    }

    public string FeatureId { get; }

    public Task<DocumentOperationResult> ExecuteAsync(
        DocumentOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.InputFiles.Count == 0)
        {
            return Task.FromResult(DocumentOperationResult.Fail("请先选择输入文件。"));
        }

        try
        {
            Directory.CreateDirectory(request.OutputDirectory);
            var outputs = new List<string>();

            foreach (string input in request.InputFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                outputs.AddRange(SplitOne(input, request));
            }

            return Task.FromResult(
                DocumentOperationResult.Ok($"拆分完成，生成 {outputs.Count} 个文件。", outputs));
        }
        catch (Exception ex)
        {
            // 拆分失败时 Aspose 抛的是英文原文（评估模式上限、加密、格式不支持等）。
            Exception friendly = OperationErrors.Translate(ex, "拆分文档", request.OutputDirectory);
            return Task.FromResult(DocumentOperationResult.Fail(friendly.Message));
        }
    }

    private IReadOnlyList<string> SplitOne(string input, DocumentOperationRequest request)
    {
        string? splitMode = GetOption(request, "splitMode");
        int pagesPerFile = ParsePositiveInt(GetOption(request, "pagesPerFile"), 1);
        string? rangeText = GetOption(request, "range");
        bool resetPageNumber = ParseBool(GetOption(request, "refreshPageNumber"))
                               || ParseBool(GetOption(request, "resetPageNumber"));
        bool byBookmark = ParseBool(GetOption(request, "splitByBookmark"));
        bool deletePages = ParseBool(GetOption(request, "deletePages"));
        // legacy SplitPdf 的删除页是独立控件（DeletePdfPages_Start/End，Util.cs:5736-5823），
        // 与「拆分范围」无关；老配置只填 range 时仍按 range 解析，保持兼容。
        string? deleteStartText = GetOption(request, "deleteStart");
        string? deleteEndText = GetOption(request, "deleteEnd");

        bool rangeMode = string.Equals(splitMode, "按页码范围", StringComparison.Ordinal);
        bool everyPage = string.Equals(splitMode, "每页拆分", StringComparison.Ordinal);
        bool byCount = string.Equals(splitMode, "按页数", StringComparison.Ordinal)
                       || (!rangeMode && !everyPage && !byBookmark && !deletePages);

        if (FeatureId == "pdf.split")
        {
            return SplitPdf(input, request.OutputDirectory, everyPage, byCount, rangeMode, rangeText,
                pagesPerFile, byBookmark, deletePages, deleteStartText, deleteEndText);
        }

        if (FeatureId == "word.split")
        {
            return SplitWord(input, request.OutputDirectory, everyPage, byCount, rangeMode, rangeText,
                pagesPerFile, resetPageNumber);
        }

        if (FeatureId == "ppt.split")
        {
            return SplitPpt(
                input,
                request.OutputDirectory,
                everyPage,
                byCount,
                rangeMode,
                rangeText,
                pagesPerFile,
                GetOption(request, "slideSize"),
                GetOption(request, "slideScale"));
        }

        throw new NotSupportedException($"未支持的拆分功能：{FeatureId}");
    }

    private IReadOnlyList<string> SplitPdf(
        string input,
        string outputDirectory,
        bool everyPage,
        bool byCount,
        bool rangeMode,
        string? rangeText,
        int pagesPerFile,
        bool byBookmark,
        bool deletePages,
        string? deleteStartText,
        string? deleteEndText)
    {
        var options = new PdfSplitOptions
        {
            SplitIntoChunks = false,
            PagesPerFile = 1,
            SplitByBookmark = byBookmark
        };

        // legacy 的删除页是「CheckBox + 起止页」两组控件（Util.cs:5736-5823）；
        // 本项目未保存该 CheckBox，故约定：deleteStart 与 deleteEnd 都填即删除，
        // 老配置只给 deletePages + range 时仍按原逻辑回退解析 range。
        int? deleteStart = ParsePositiveIntOrNull(deleteStartText);
        int? deleteEnd = ParsePositiveIntOrNull(deleteEndText);
        bool deleteByKeys = deleteStart is not null && deleteEnd is not null;

        if (deletePages || deleteByKeys)
        {
            if (deleteStart is null || deleteEnd is null)
            {
                var (rangeStart, rangeEnd) = PageRangeTextParser.FirstAsStartEnd(rangeText);
                deleteStart ??= rangeStart;
                deleteEnd ??= rangeEnd;
            }

            if (deleteStart is null || deleteEnd is null)
            {
                throw new ArgumentException("删除页需要填写删除起始页与结束页（如 2 和 4）。");
            }

            options.DeletePages = true;
            options.DeleteStartPage = deleteStart;
            options.DeleteEndPage = deleteEnd;
        }
        else if (rangeMode)
        {
            IReadOnlyList<PdfPageRange> ranges = PageRangeTextParser.Parse(rangeText);
            if (ranges.Count == 0)
            {
                throw new ArgumentException("按页码范围拆分时请填写范围（如 1-3,5）。");
            }

            var produced = new List<string>();
            foreach (PdfPageRange range in ranges)
            {
                produced.AddRange(_pdfSplit.Split(input, outputDirectory, new PdfSplitOptions
                {
                    SplitIntoChunks = false,
                    ExtractStartPage = range.FirstPage,
                    ExtractEndPage = range.LastPage
                }));
            }

            if (byBookmark)
            {
                produced.AddRange(_pdfSplit.Split(input, outputDirectory, new PdfSplitOptions
                {
                    SplitIntoChunks = false,
                    SplitByBookmark = true
                }));
            }

            return produced;
        }
        else if (everyPage || byCount)
        {
            options.SplitIntoChunks = true;
            options.PagesPerFile = everyPage ? 1 : Math.Max(1, pagesPerFile);
        }
        else if (byBookmark)
        {
            options.SplitByBookmark = true;
        }
        else
        {
            // Default: every N pages (legacy SplitSignle with SplitPageCount).
            options.SplitIntoChunks = true;
            options.PagesPerFile = Math.Max(1, pagesPerFile);
        }

        return _pdfSplit.Split(input, outputDirectory, options);
    }

    private IReadOnlyList<string> SplitWord(
        string input,
        string outputDirectory,
        bool everyPage,
        bool byCount,
        bool rangeMode,
        string? rangeText,
        int pagesPerFile,
        bool resetPageNumber)
    {
        if (rangeMode)
        {
            IReadOnlyList<PdfPageRange> ranges = PageRangeTextParser.Parse(rangeText);
            if (ranges.Count == 0)
            {
                throw new ArgumentException("按页码范围拆分时请填写范围（如 1-3）。");
            }

            var produced = new List<string>();
            foreach (PdfPageRange range in ranges)
            {
                produced.AddRange(_mergeSplit.SplitWord(input, outputDirectory, new WordSplitOptions
                {
                    SplitEveryPages = false,
                    RangeStart = range.FirstPage.ToString(),
                    RangeEnd = range.LastPage.ToString(),
                    ResetPageNumber = resetPageNumber
                }));
            }

            return produced;
        }

        return _mergeSplit.SplitWord(input, outputDirectory, new WordSplitOptions
        {
            SplitEveryPages = true,
            PagesPerFile = everyPage ? 1 : Math.Max(1, pagesPerFile),
            ResetPageNumber = resetPageNumber
        });
    }

    private IReadOnlyList<string> SplitPpt(
        string input,
        string outputDirectory,
        bool everyPage,
        bool byCount,
        bool rangeMode,
        string? rangeText,
        int pagesPerFile,
        string? slideSize,
        string? slideScale)
    {
        if (rangeMode)
        {
            IReadOnlyList<PdfPageRange> ranges = PageRangeTextParser.Parse(rangeText);
            if (ranges.Count == 0)
            {
                throw new ArgumentException("按页码范围拆分时请填写范围（如 1-3）。");
            }

            var produced = new List<string>();
            foreach (PdfPageRange range in ranges)
            {
                produced.AddRange(_mergeSplit.SplitPowerPoint(input, outputDirectory, new SlideSplitOptions
                {
                    SplitEverySlide = false,
                    RangeStart = range.FirstPage.ToString(),
                    RangeEnd = range.LastPage.ToString(),
                    SlideSizeName = slideSize,
                    SlideScaleName = slideScale
                }));
            }

            return produced;
        }

        return _mergeSplit.SplitPowerPoint(input, outputDirectory, new SlideSplitOptions
        {
            SplitEverySlide = true,
            SlidesPerFile = everyPage ? 1 : Math.Max(1, pagesPerFile),
            SlideSizeName = slideSize,
            SlideScaleName = slideScale
        });
    }

    private static string? GetOption(DocumentOperationRequest request, string key)
    {
        if (request.Options is null)
        {
            return null;
        }

        return request.Options.TryGetValue(key, out string? value) ? value : null;
    }

    private static int ParsePositiveInt(string? text, int fallback)
        => int.TryParse(text, out int n) && n > 0 ? n : fallback;

    private static int? ParsePositiveIntOrNull(string? text)
        => int.TryParse(text, out int n) && n > 0 ? n : null;

    private static bool ParseBool(string? text)
        => string.Equals(text, "1", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase);
}
