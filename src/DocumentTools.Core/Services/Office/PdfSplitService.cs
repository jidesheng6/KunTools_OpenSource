using Aspose.Pdf;
using Aspose.Pdf.Facades;
using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Office;

/// <summary>PDF 拆分：每块都重新打开源文件，避免页被“搬走”影响后续计划。</summary>
public sealed class PdfSplitService : IPdfSplitService
{
    public IReadOnlyList<string> Split(string sourceFile, string outputDirectory, PdfSplitOptions options)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(outputDirectory, nameof(outputDirectory));
        ArgumentNullException.ThrowIfNull(options);

        Directory.CreateDirectory(outputDirectory);

        string stem = Path.GetFileNameWithoutExtension(sourceFile);
        var produced = new List<string>();

        int totalPages;
        using (var probe = new Document(sourceFile))
        {
            totalPages = probe.Pages.Count;
        }

        if (options.SplitIntoChunks)
        {
            IReadOnlyList<PdfPageRange> chunks = PdfPagePlanner.PlanChunks(totalPages, options.PagesPerFile);
            int index = 0;
            foreach (PdfPageRange chunk in chunks)
            {
                index++;
                string target = Path.Combine(outputDirectory, PdfPagePlanner.BuildChunkFileName(stem, index));
                SaveRange(sourceFile, chunk, target);
                produced.Add(target);
            }
        }

        if (options.ExtractStartPage.HasValue && options.ExtractEndPage.HasValue)
        {
            PdfPageRange range = PdfPagePlanner.NormalizeRange(
                totalPages,
                options.ExtractStartPage.Value,
                options.ExtractEndPage.Value);

            string target = Path.Combine(outputDirectory, PdfPagePlanner.BuildExtractFileName(stem, range));
            SaveRange(sourceFile, range, target);
            produced.Add(target);
        }

        if (options.SplitByBookmark)
        {
            produced.AddRange(SplitByBookmarks(sourceFile, outputDirectory, stem, totalPages));
        }

        if (options.DeletePages)
        {
            if (!options.DeleteStartPage.HasValue || !options.DeleteEndPage.HasValue)
            {
                throw new ArgumentException("删除页需要同时给出起始页与结束页。", nameof(options));
            }

            IReadOnlyList<int> pages = PdfPagePlanner.PlanDeletion(
                totalPages,
                options.DeleteStartPage.Value,
                options.DeleteEndPage.Value);

            string target = Path.Combine(outputDirectory, stem + ".pdf");
            SaveWithoutPages(sourceFile, pages, target);
            produced.Add(target);
        }

        return produced;
    }

    private static void SaveRange(string sourceFile, PdfPageRange range, string target)
    {
        using var source = new Document(sourceFile);
        using var output = new Document();
        for (int page = range.FirstPage; page <= range.LastPage; page++)
        {
            output.Pages.Add(source.Pages[page]);
        }

        output.Save(target, SaveFormat.Pdf);
    }

    private static void SaveWithoutPages(string sourceFile, IReadOnlyList<int> pagesToDelete, string target)
    {
        using var source = new Document(sourceFile);
        source.Pages.Delete(pagesToDelete.ToArray());
        source.Save(target, SaveFormat.Pdf);
    }

    private static List<string> SplitByBookmarks(string sourceFile, string outputDirectory, string stem, int totalPages)
    {
        var produced = new List<string>();

        using var source = new Document(sourceFile);
        using var editor = new PdfBookmarkEditor();
        editor.BindPdf(source);
        Bookmarks bookmarks = editor.ExtractBookmarks();

        if (bookmarks is null || bookmarks.Count == 0)
        {
            return produced;
        }

        int startPage = 1;
        int index = 0;

        foreach (Bookmark bookmark in bookmarks)
        {
            index++;
            string title = (bookmark.Title ?? string.Empty)
                .Replace("\t", string.Empty, StringComparison.Ordinal)
                .Replace("/", string.Empty, StringComparison.Ordinal);

            int endPage = bookmark.PageNumber <= startPage
                ? startPage
                : bookmark.PageNumber - 1;

            PdfPageRange range = PdfPagePlanner.NormalizeRange(totalPages, startPage, endPage);
            string target = Path.Combine(outputDirectory, index + "、" + stem + "_" + title + ".pdf");
            SaveRange(sourceFile, range, target);
            produced.Add(target);

            if (range.LastPage >= totalPages)
            {
                break;
            }

            startPage = range.LastPage + 1;
        }

        return produced;
    }
}
