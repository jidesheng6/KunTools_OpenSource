namespace DocumentTools.Core.Services.Office;

/// <summary>一页到一页的闭区间（页码从 1 开始）。</summary>
public readonly struct PdfPageRange
{
    public PdfPageRange(int firstPage, int lastPage)
    {
        FirstPage = firstPage;
        LastPage = lastPage;
    }

    public int FirstPage { get; }

    public int LastPage { get; }

    public int Count => LastPage - FirstPage + 1;

    public override string ToString()
        => FirstPage == LastPage ? FirstPage.ToString() : FirstPage + "-" + LastPage;
}

/// <summary>PDF 拆分参数。</summary>
public sealed class PdfSplitOptions
{
    /// <summary>逐页（或每 N 页）拆分成多个文件。</summary>
    public bool SplitIntoChunks { get; set; } = true;

    /// <summary>每个文件包含的页数，必须大于 0。</summary>
    public int PagesPerFile { get; set; } = 1;

    public int? ExtractStartPage { get; set; }

    public int? ExtractEndPage { get; set; }

    public bool SplitByBookmark { get; set; }

    public bool DeletePages { get; set; }

    public int? DeleteStartPage { get; set; }

    public int? DeleteEndPage { get; set; }
}

/// <summary>拆分计划：纯算术，便于单测。</summary>
public static class PdfPagePlanner
{
    public static IReadOnlyList<PdfPageRange> PlanChunks(int totalPages, int pagesPerFile)
    {
        if (totalPages < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalPages), "总页数不能为负。");
        }

        if (pagesPerFile <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(pagesPerFile), "每份页数必须大于 0。");
        }

        var chunks = new List<PdfPageRange>();
        for (int first = 1; first <= totalPages; first += pagesPerFile)
        {
            int last = Math.Min(first + pagesPerFile - 1, totalPages);
            chunks.Add(new PdfPageRange(first, last));
        }

        return chunks;
    }

    public static PdfPageRange NormalizeRange(int totalPages, int firstPage, int lastPage)
    {
        if (totalPages <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalPages), "文档没有页面。");
        }

        if (firstPage < 1 || lastPage < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(firstPage), "页码从 1 开始。");
        }

        if (firstPage > lastPage)
        {
            throw new ArgumentOutOfRangeException(nameof(firstPage), "起始页不能大于结束页。");
        }

        return new PdfPageRange(Math.Min(firstPage, totalPages), Math.Min(lastPage, totalPages));
    }

    public static IReadOnlyList<int> PlanDeletion(int totalPages, int firstPage, int lastPage)
    {
        PdfPageRange range = NormalizeRange(totalPages, firstPage, lastPage);
        var pages = new List<int>(range.Count);
        for (int page = range.FirstPage; page <= range.LastPage; page++)
        {
            pages.Add(page);
        }

        return pages;
    }

    public static string BuildChunkFileName(string sourceFileStem, int chunkIndex)
    {
        if (string.IsNullOrWhiteSpace(sourceFileStem))
        {
            throw new ArgumentNullException(nameof(sourceFileStem));
        }

        if (chunkIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkIndex), "块序号从 1 开始。");
        }

        return sourceFileStem + "_第" + chunkIndex + "页.pdf";
    }

    public static string BuildExtractFileName(string sourceFileStem, PdfPageRange range)
    {
        if (string.IsNullOrWhiteSpace(sourceFileStem))
        {
            throw new ArgumentNullException(nameof(sourceFileStem));
        }

        return sourceFileStem + "_第" + range.FirstPage + "到" + range.LastPage + "页.pdf";
    }
}

public interface IPdfSplitService
{
    IReadOnlyList<string> Split(string sourceFile, string outputDirectory, PdfSplitOptions options);
}
