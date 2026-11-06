using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Office;

/// <summary>
/// 文档合并与拆分。行为对齐 sunnyui DocumentMergeSplitService：
/// 合并 PDF 显式拒绝非 .pdf；合并 Word 去掉空白首节。
/// </summary>
public sealed class DocumentMergeSplitService : IDocumentMergeSplitService
{
    public string MergePdfFiles(IList<string> pdfFiles, string outputFile)
    {
        RequireList(pdfFiles, nameof(pdfFiles));
        PathGuard.RequirePath(outputFile, nameof(outputFile));

        var offenders = pdfFiles
            .Where(f => !string.Equals(Path.GetExtension(f), ".pdf", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .ToList();

        if (offenders.Count > 0)
        {
            throw new ArgumentException(
                "合并 PDF 只接受 .pdf 文件，以下输入不会被合并：" + string.Join("、", offenders),
                nameof(pdfFiles));
        }

        // Aspose.Pdf.Document 实现 IDisposable（Aspose.Words.Document 没有，别按同样写法加 using 去编不过）。
        // 放 using 里是为了在 Save 之后立刻释放，而不是等终结器；merged 最后一次使用就是下面这行 Save。
        using var merged = new Aspose.Pdf.Document();
        foreach (string file in pdfFiles)
        {
            using var part = new Aspose.Pdf.Document(file);
            foreach (Aspose.Pdf.Page page in part.Pages)
            {
                merged.Pages.Add(page);
            }
        }

        EnsureParentDirectory(outputFile);
        merged.Save(outputFile);
        return outputFile;
    }

    public string MergeWordFiles(IList<string> wordFiles, string outputFile)
    {
        RequireList(wordFiles, nameof(wordFiles));
        PathGuard.RequirePath(outputFile, nameof(outputFile));

        var saveOptions = new Aspose.Words.Saving.OoxmlSaveOptions
        {
            Compliance = Aspose.Words.Saving.OoxmlCompliance.Iso29500_2008_Transitional,
            SaveFormat = Aspose.Words.SaveFormat.Docx,
            KeepLegacyControlChars = true,
        };

        var merged = new Aspose.Words.Document();
        foreach (string file in wordFiles)
        {
            var part = new Aspose.Words.Document(file);
            merged.AppendDocument(part, Aspose.Words.ImportFormatMode.KeepSourceFormatting);
        }

        merged.FirstSection.Remove();
        EnsureParentDirectory(outputFile);
        merged.Save(outputFile, saveOptions);
        return outputFile;
    }

    public IReadOnlyList<string> SplitWord(string sourceFile, string outputDirectory, WordSplitOptions options)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(outputDirectory, nameof(outputDirectory));
        options ??= new WordSplitOptions();
        Directory.CreateDirectory(outputDirectory);

        var created = new List<string>();
        string baseName = Path.GetFileNameWithoutExtension(sourceFile);
        var saveOptions = new Aspose.Words.Saving.OoxmlSaveOptions
        {
            Compliance = Aspose.Words.Saving.OoxmlCompliance.Iso29500_2008_Transitional,
        };

        var document = new Aspose.Words.Document(sourceFile);
        if (options.SplitEveryPages)
        {
            int pagesPerFile = Math.Max(1, options.PagesPerFile);
            int totalPages = document.PageCount;
            int fileIndex = 0;

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex += pagesPerFile)
            {
                fileIndex++;
                int pageExtra = Math.Min(pagesPerFile, totalPages - pageIndex);
                if (pageExtra <= 0)
                {
                    break;
                }

                string saved = Path.Combine(outputDirectory, baseName + "_第" + fileIndex + "页.docx");
                SaveExtract(document, pageIndex, pageExtra, saved, saveOptions, options.ResetPageNumber);
                created.Add(saved);
            }
        }
        else
        {
            if (!int.TryParse(options.RangeStart, out int rangeStart)
                || !int.TryParse(options.RangeEnd, out int rangeEnd)
                || rangeStart < 1
                || rangeEnd < rangeStart)
            {
                throw new ArgumentException("页码范围必须是有效数字（起始≤结束，从 1 起）。", nameof(options));
            }

            int pageCount = rangeEnd - rangeStart + 1;
            string saved = Path.Combine(
                outputDirectory,
                baseName + "_第" + options.RangeStart + "到" + options.RangeEnd + "页.docx");

            SaveExtract(document, rangeStart - 1, pageCount, saved, saveOptions, options.ResetPageNumber);
            created.Add(saved);
        }

        return created;
    }

    public IReadOnlyList<string> SplitPowerPoint(string sourceFile, string outputDirectory, SlideSplitOptions options)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(outputDirectory, nameof(outputDirectory));
        options ??= new SlideSplitOptions();
        Directory.CreateDirectory(outputDirectory);

        var created = new List<string>();
        string baseName = Path.GetFileNameWithoutExtension(sourceFile);

        using var source = new Aspose.Slides.Presentation(sourceFile);
        int slideCount = source.Slides.Count;

        // legacy SplitPPT 每份产物都调用 SlideSize.SetSize(SplitSizeType, SplitScaleMode)（Util.cs:5825-5870）。
        Aspose.Slides.SlideSizeType slideSize = ParseSlideSize(options.SlideSizeName);
        Aspose.Slides.SlideSizeScaleType slideScale = ParseSlideScale(options.SlideScaleName);

        if (options.SplitEverySlide)
        {
            int perFile = Math.Max(1, options.SlidesPerFile);
            int fileIndex = 0;
            for (int slideIndex = 0; slideIndex < slideCount; slideIndex += perFile)
            {
                fileIndex++;
                int take = Math.Min(perFile, slideCount - slideIndex);
                string saved = Path.Combine(outputDirectory, baseName + "_第" + fileIndex + "份.pptx");
                using var chunk = new Aspose.Slides.Presentation();
                chunk.SlideSize.SetSize(slideSize, slideScale);
                chunk.Slides.RemoveAt(0);
                for (int i = 0; i < take; i++)
                {
                    chunk.Slides.AddClone(source.Slides[slideIndex + i]);
                }

                chunk.Save(saved, Aspose.Slides.Export.SaveFormat.Pptx);
                created.Add(saved);
            }

            return created;
        }

        if (!int.TryParse(options.RangeStart, out int start)
            || !int.TryParse(options.RangeEnd, out int end))
        {
            throw new ArgumentException("页码范围必须是数字。", nameof(options));
        }

        string rangeFile = Path.Combine(
            outputDirectory,
            baseName + "_第" + options.RangeStart + "到" + options.RangeEnd + "页.pptx");

        using (var rangeOutput = new Aspose.Slides.Presentation())
        {
            rangeOutput.SlideSize.SetSize(slideSize, slideScale);
            rangeOutput.Slides.RemoveAt(0);

            int exportEnd = Math.Min(end, slideCount);
            for (int slideIndex = start - 1; slideIndex < exportEnd; slideIndex++)
            {
                rangeOutput.Slides.AddClone(source.Slides[slideIndex]);
            }

            rangeOutput.Save(rangeFile, Aspose.Slides.Export.SaveFormat.Pptx);
        }

        created.Add(rangeFile);
        return created;
    }

    private static void SaveExtract(
        Aspose.Words.Document document,
        int pageIndex,
        int pageCount,
        string savedFile,
        Aspose.Words.Saving.OoxmlSaveOptions saveOptions,
        bool resetPageNumber)
    {
        Aspose.Words.Document extracted = document.ExtractPages(pageIndex, pageCount);
        extracted.CompatibilityOptions.OptimizeFor(Aspose.Words.Settings.MsWordVersion.Word2010);
        extracted.CompatibilityOptions.BalanceSingleByteDoubleByteWidth = true;

        if (resetPageNumber)
        {
            extracted.FirstSection.PageSetup.PageStartingNumber = 1;
        }

        extracted.UpdatePageLayout();
        extracted.Save(savedFile, saveOptions);
    }

    private static void RequireList(IList<string> files, string name)
    {
        if (files is null || files.Count == 0)
        {
            throw new ArgumentException("文件列表不能为空。", name);
        }
    }

    /// <summary>解析幻灯片尺寸名；缺省或非法值回退 OnScreen（拆分历史默认值）。</summary>
    private static Aspose.Slides.SlideSizeType ParseSlideSize(string? name)
        => Enum.TryParse(name, ignoreCase: true, out Aspose.Slides.SlideSizeType value)
            ? value
            : Aspose.Slides.SlideSizeType.OnScreen;

    /// <summary>解析幻灯片缩放名；缺省或非法值回退 Maximize（拆分历史默认值）。</summary>
    private static Aspose.Slides.SlideSizeScaleType ParseSlideScale(string? name)
        => Enum.TryParse(name, ignoreCase: true, out Aspose.Slides.SlideSizeScaleType value)
            ? value
            : Aspose.Slides.SlideSizeScaleType.Maximize;

    private static void EnsureParentDirectory(string filePath)
    {
        string? dir = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
