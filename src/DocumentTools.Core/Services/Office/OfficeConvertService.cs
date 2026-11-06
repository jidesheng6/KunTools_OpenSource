using System.Diagnostics;
using System.Text;
using Aspose.Cells;
using Aspose.Cells.Rendering;
using Aspose.Pdf;
using Aspose.Slides;
using Aspose.Words;
using Aspose.Words.Saving;
using DocumentTools.Core.Common;
using PdfSaveOptions = Aspose.Words.Saving.PdfSaveOptions;
using WordsImageSaveOptions = Aspose.Words.Saving.ImageSaveOptions;

namespace DocumentTools.Core.Services.Office;

/// <summary>
/// Thin Aspose wrappers: load - configure - save.
/// 只有 <c>word.pdf</c> 的本机 Office 分支会写一行引擎归因日志（<see cref="ConversionLog"/>），
/// 其余转换不碰磁盘日志，仍可在测试与后台线程里安全调用。
/// </summary>
public sealed class OfficeConvertService : IOfficeConvertService
{
    /// <summary>Word 自动化入口的 ProgID；错误消息里也要报出它，方便用户判断到底缺了什么。</summary>
    public const string WordApplicationProgId = "Word.Application";

    /// <summary>置任意非空值即模拟「本机没装 Word」，仅供自动化测试使用。</summary>
    public const string SimulateMissingWordVariable = "DOCUMENTTOOLS_SIMULATE_NO_WORD";

    /// <summary>
    /// 本机 Office 导出 PDF 时的书签模式，对应 Word 的 <c>WdExportCreateBookmarks</c>：
    /// 2 = 按 Word 书签生成（legacy <c>Util.cs:2731</c> 传的就是 2）；
    /// 1 = 按标题样式生成，会让目录结构随样式漂移，与旧版产物不一致，故不采用。
    /// 提成常量是为了让「2」这个值只出现在一处，测试与调用点共用同一定义。
    /// </summary>
    public const int WordBookmarksExportMode = 2;

    private const string ConversionLogCategory = "word.pdf";

    public void WordToPdf(string sourceFile, string savedFile, WordToPdfOptions? options = null)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        options ??= new WordToPdfOptions();

        // legacy UseLocalOffice（BodyForm.cs:7002-7019 / Util.cs:2717-2738）：勾选时改走本机 Word 的
        // ExportAsFixedFormat 出 PDF，并忽略合规/颜色模式/字体嵌入等 Aspose 专属参数（legacy 会把这些控件禁用）。
        if (options.UseLocalOffice)
        {
            WordToPdfViaLocalOffice(sourceFile, savedFile, options.Password);
            return;
        }

        bool hasVersion = Enum.TryParse(
            options.MsWordVersion, ignoreCase: true, out Aspose.Words.Settings.MsWordVersion version);

        // legacy ConvertWord2Pdf（Util.cs:2589-2739）除界面 7 个参数外还固定设置了一批
        // 加载/渲染选项（不新增 UI 键，属服务层对齐），用于保证缺字体与图表场景的排版一致。
        var loadOptions = new Aspose.Words.Loading.LoadOptions
        {
            Password = options.Password ?? string.Empty,
            ConvertMetafilesToPng = false
        };
        // legacy 固定把编辑语言设为简体中文（Util.cs:2601 附近），影响分页与校对语言判定。
        loadOptions.LanguagePreferences.DefaultEditingLanguage = Aspose.Words.Loading.EditingLanguage.ChinesePRC;
        if (hasVersion)
        {
            loadOptions.MswVersion = version;
        }

        Aspose.Words.Document doc = new(sourceFile, loadOptions);
        if (options.UpdateFields)
        {
            doc.UpdateFields();
        }

        doc.RemovePersonalInformation = true;
        doc.CompatibilityOptions.BalanceSingleByteDoubleByteWidth = true;

        if (hasVersion)
        {
            doc.CompatibilityOptions.OptimizeFor(version);
        }

        var save = new PdfSaveOptions
        {
            EmbedFullFonts = options.EmbedFonts,
            AllowEmbeddingPostScriptFonts = true,
            FontEmbeddingMode = Aspose.Words.Saving.PdfFontEmbeddingMode.EmbedAll,
            ImageCompression = Aspose.Words.Saving.PdfImageCompression.Auto,
            PageMode = Aspose.Words.Saving.PdfPageMode.UseOutlines,
            ZoomBehavior = Aspose.Words.Saving.PdfZoomBehavior.None,
            UseHighQualityRendering = true,
            UseAntiAliasing = true,
            PreblendImages = true,
            JpegQuality = 100,
            ExportGeneratorName = false,
            UpdateFields = options.UpdateFields,
            HeaderFooterBookmarksExportMode = Aspose.Words.Saving.HeaderFooterBookmarksExportMode.All
        };

        // OutlineOptions 为只读属性（实例已存在），按其成员逐项赋值以对齐 legacy 的书签大纲设置。
        save.OutlineOptions.DefaultBookmarksOutlineLevel = 0;
        save.OutlineOptions.CreateMissingOutlineLevels = false;
        save.OutlineOptions.ExpandedOutlineLevels = 3;
        save.OutlineOptions.HeadingsOutlineLevels = 9;

        if (Enum.TryParse(options.Compliance, ignoreCase: true, out Aspose.Words.Saving.PdfCompliance compliance))
        {
            save.Compliance = compliance;
        }

        if (Enum.TryParse(options.ColorMode, ignoreCase: true, out Aspose.Words.Saving.ColorMode colorMode))
        {
            save.ColorMode = colorMode;
        }

        doc.Save(savedFile, save);
    }

    /// <summary>
    /// 用本机 Word 的 <c>ExportAsFixedFormat</c> 出 PDF，对齐 legacy <c>Util.cs:2717-2738</c>。
    /// 走晚绑定 COM（ProgID <c>Word.Application</c>），不引用 Office Interop 程序集。
    /// 成功/失败都写一行 <see cref="ConversionLog"/>：同一份 PDF 是 Word 导出的还是 Aspose 导出的，
    /// 事后只能靠这行日志区分（用户报「为什么这份 PDF 没有书签/目录」时要用）。
    /// </summary>
    private static void WordToPdfViaLocalOffice(string sourceFile, string savedFile, string? password)
    {
        Type? wordType = ResolveWordApplicationType();
        if (wordType is null)
        {
            ConversionLog.Write(ConversionLogCategory,
                $"结果=失败 引擎={WordApplicationProgId} 源={sourceFile} 原因=未检测到 Microsoft Word");
            throw new InvalidOperationException(
                "转换 Word 文档失败：未检测到 Microsoft Word。" +
                "请安装 Word，或取消「使用本机 Office 转换」勾选后重试。" +
                $"（已尝试：{WordApplicationProgId}；该 ProgID 在本机注册表查不到。）");
        }

        long startedAt = Stopwatch.GetTimestamp();
        object? application = null;
        object? document = null;
        try
        {
            application = Activator.CreateInstance(wordType);
            dynamic word = application!;
            word.Visible = false;
            word.DisplayAlerts = 0; // wdAlertsNone

            document = word.Documents.Open(
                FileName: sourceFile,
                ConfirmConversions: false,
                ReadOnly: false,
                AddToRecentFiles: false,
                PasswordDocument: password ?? string.Empty,
                Visible: false);

            // wdExportFormatPDF=17、wdExportOptimizeForPrint=0、wdExportCreateWordBookmarks=2
            // （1 是 wdExportCreateHeadingBookmarks；legacy Util.cs:2731 传的是 WordBookmarks=2，
            //  传 1 会让目录按标题样式生成，与旧版产物不一致）。
            dynamic wordDocument = document!;
            wordDocument.ExportAsFixedFormat(
                OutputFileName: savedFile,
                ExportFormat: 17,
                OpenAfterExport: false,
                OptimizeFor: 0,
                Range: 0,
                Item: 0,
                IncludeDocProps: true,
                KeepIRM: true,
                CreateBookmarks: WordBookmarksExportMode,
                DocStructureTags: true,
                BitmapMissingFonts: true,
                UseISO19005_1: false);

            long elapsedMs = (long)Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            ConversionLog.Write(ConversionLogCategory,
                $"结果=成功 引擎={WordApplicationProgId} 源={sourceFile} 输出={savedFile} 耗时={elapsedMs}ms");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // 不只 COMException：ProgID 指向别的自动化服务器、成员缺失等会抛 RuntimeBinderException，
            // 那时「这份 PDF 为什么是 Aspose 出的」必须能在日志里查到，否则归因链断掉。
            ConversionLog.Write(ConversionLogCategory,
                $"结果=失败 引擎={WordApplicationProgId} 源={sourceFile} 异常={ex.GetType().Name} 原因={ex.Message}");

            string hint = ex is global::System.Runtime.InteropServices.COMException
                ? "请确认该文件未被 Word 打开，且文档未加密。"
                : "请确认本机 Word 安装完整（Word.Application 自动化可用），或取消「使用本机 Office 转换」勾选后重试。";

            throw new InvalidOperationException($"本机 Word 转换失败（{ex.Message}）：{hint}", ex);
        }
        finally
        {
            ReleaseComObject(ref document, closeFirst: true);
            ReleaseComObject(ref application, closeFirst: false);
        }
    }

    /// <summary>
    /// 探测本机是否注册了 Word 自动化入口。<c>DOCUMENTTOOLS_SIMULATE_NO_WORD</c> 置任意非空值即
    /// 模拟「本机没装 Word」，让降级文案可以在装了 Word 的机器上也被测试（照 <c>AccessDb</c> 的模拟开关风格）。
    /// </summary>
    private static Type? ResolveWordApplicationType()
        => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(SimulateMissingWordVariable))
            ? Type.GetTypeFromProgID(WordApplicationProgId)
            : null;

    private static void ReleaseComObject(ref object? instance, bool closeFirst)
    {
        if (instance is null)
        {
            return;
        }

        try
        {
            if (closeFirst)
            {
                ((dynamic)instance).Close(0); // wdDoNotSaveChanges
            }
            else
            {
                ((dynamic)instance).Quit(0);
            }
        }
        catch
        {
            // 退出阶段的对象状态不可靠（文档已被用户关闭、Word 已退出等），忽略即可。
        }

        if (global::System.Runtime.InteropServices.Marshal.IsComObject(instance))
        {
            global::System.Runtime.InteropServices.Marshal.ReleaseComObject(instance);
        }

        instance = null;
    }

    public void PdfToWord(string sourceFile, string savedFile, PdfToWordOptions? options = null)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        options ??= new PdfToWordOptions();
        using var pdf = string.IsNullOrWhiteSpace(options.Password)
            ? new Aspose.Pdf.Document(sourceFile)
            : new Aspose.Pdf.Document(sourceFile, options.Password);

        // legacy Util.ConvertPdf2Word 先改写属性并解密再保存（Util.cs:2130-2134），保证产物元数据一致。
        pdf.Info.Author = "DocumentTools";
        pdf.Info.Producer = "DocumentTools";
        pdf.Info.Creator = "DocumentTools";
        pdf.Decrypt();

        var saveOptions = new Aspose.Pdf.DocSaveOptions
        {
            Format = Aspose.Pdf.DocSaveOptions.DocFormat.DocX,
            RecognizeBullets = options.UseBullet,
            ExtractOcrSublayerOnly = options.UseOcr
        };

        if (Enum.TryParse(options.Mode, ignoreCase: true, out Aspose.Pdf.DocSaveOptions.RecognitionMode mode))
        {
            saveOptions.Mode = mode;
        }

        pdf.Save(savedFile, saveOptions);
    }

    public void ExcelToPdf(string sourceFile, string savedFile, ExcelToPdfOptions? options = null)
        => ExcelToPdfWorker(sourceFile, savedFile, options);

    public IReadOnlyList<string> ExcelToPdfWithOutputs(
        string sourceFile,
        string savedFile,
        ExcelToPdfOptions? options = null)
        => ExcelToPdfWorker(sourceFile, savedFile, options);

    /// <summary>
    /// EXCEL→PDF。`eachSheet` 会产出多个文件，故内部统一返回真实产物路径，
    /// 供 <see cref="void"/> 重载与需要回报清单的调用方共用。
    /// </summary>
    private static IReadOnlyList<string> ExcelToPdfWorker(
        string sourceFile,
        string savedFile,
        ExcelToPdfOptions? options)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        options ??= new ExcelToPdfOptions();
        using var workbook = new Workbook(sourceFile);

        // legacy Util.ConvertExcel2Pdf 的 PdfSaveOptions（Util.cs:1586-1591）：
        // eachSheet 对应 OnePagePerSheet；fitOnePage 对应 AllColumnsInOnePagePerSheet；公式恒计算。
        var pdfSaveOptions = new Aspose.Cells.PdfSaveOptions
        {
            OnePagePerSheet = options.ExportEachSheet,
            AllColumnsInOnePagePerSheet = options.ForceContentOnOnePage,
            CalculateFormula = true
        };

        foreach (Worksheet sheet in workbook.Worksheets)
        {
            Aspose.Cells.PageSetup setup = sheet.PageSetup;
            setup.CenterHorizontally = options.HorizontalCenter;
            setup.CenterVertically = options.VerticalCenter;
            if (options.ForceContentOnOnePage)
            {
                setup.FitToPagesWide = 1;
                setup.FitToPagesTall = 1;
            }

            if (options.PrintScalePercentEnabled)
            {
                setup.Zoom = Math.Clamp(options.PrintScalePercent, 10, 400);
            }
            else if (options.PrintScaleRow > 0 || options.PrintScaleCol > 0)
            {
                setup.FitToPagesTall = Math.Max(0, options.PrintScaleRow);
                setup.FitToPagesWide = Math.Max(0, options.PrintScaleCol);
            }
        }

        if (options.ExportEachSheet)
        {
            string dir = Path.GetDirectoryName(savedFile) ?? ".";
            string name = Path.GetFileNameWithoutExtension(savedFile);
            var outputs = new List<string>();
            for (int i = 0; i < workbook.Worksheets.Count; i++)
            {
                // legacy 用「只留第 i 个 sheet 可见」逐表导出（Util.cs:1612-1626），
                // 复制到新 Workbook 会丢 PageSetup（居中、缩放），故此处置可见性切换。
                // 必须两段式：先把目标表显式设为可见，再隐藏其余，
                // 否则第二轮会先把当时唯一可见的表隐藏，触发
                // CellsException「A workbook must contain at least a visible worksheet」。
                workbook.Worksheets[i].IsVisible = true;
                workbook.Worksheets.ActiveSheetIndex = i;
                for (int j = 0; j < workbook.Worksheets.Count; j++)
                {
                    if (j != i)
                    {
                        workbook.Worksheets[j].IsVisible = false;
                    }
                }

                string path = Path.Combine(dir, $"{name}_{workbook.Worksheets[i].Name}.pdf");
                workbook.Save(path, pdfSaveOptions);
                outputs.Add(path);
            }

            return outputs;
        }

        workbook.Save(savedFile, pdfSaveOptions);
        return [savedFile];
    }

    public void PptToPdf(string sourceFile, string savedFile, PptToPdfOptions? options = null)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));
        options ??= new PptToPdfOptions();

        var load = string.IsNullOrWhiteSpace(options.Password)
            ? new Aspose.Slides.LoadOptions()
            : new Aspose.Slides.LoadOptions { Password = options.Password };

        using var presentation = new Presentation(sourceFile, load);
        var pdfOptions = new Aspose.Slides.Export.PdfOptions
        {
            ShowHiddenSlides = options.ExportHiddenSlides,
            DrawSlidesFrame = options.DrawBlackFrame,
            JpegQuality = 100
        };

        if (!string.IsNullOrWhiteSpace(options.Password))
        {
            pdfOptions.Password = options.Password;
        }

        // 备注版式：Aspose.Slides 只认 None/BottomFull/BottomTruncated。解析不了不能静默按 None 导出，
        // 否则用户选了不支持的版式会拿到一份完全没有备注的 PDF 却毫无提示。
        string notesText = (options.NotesPosition ?? string.Empty).Trim();
        bool wantsNotes = notesText.Length > 0
            && !string.Equals(notesText, nameof(Aspose.Slides.Export.NotesPositions.None), StringComparison.OrdinalIgnoreCase);
        if (wantsNotes)
        {
            if (!Enum.TryParse(notesText, ignoreCase: true, out Aspose.Slides.Export.NotesPositions notes)
                || notes == Aspose.Slides.Export.NotesPositions.None)
            {
                throw new InvalidOperationException(
                    $"不支持的备注导出位置：「{notesText}」。Aspose.Slides 只支持 None / BottomFull / BottomTruncated。");
            }

            pdfOptions.SlidesLayoutOptions = new Aspose.Slides.Export.NotesCommentsLayoutingOptions
            {
                NotesPosition = notes
            };
        }

        presentation.Save(savedFile, Aspose.Slides.Export.SaveFormat.Pdf, pdfOptions);
    }

    public void ImagesToPdf(IEnumerable<string> imageFiles, string savedFile)
        => ImagesToPdf(imageFiles, savedFile, new ImagePdfOptions());

    public void ImagesToPdf(IEnumerable<string> imageFiles, string savedFile, ImagePdfOptions? options)
    {
        PathGuard.RequirePath(savedFile, nameof(savedFile));
        options ??= new ImagePdfOptions();

        var files = imageFiles?.Where(File.Exists).ToList()
                    ?? throw new ArgumentNullException(nameof(imageFiles));

        if (files.Count == 0)
        {
            throw new ArgumentException("至少需要一张有效图片。", nameof(imageFiles));
        }

        using var pdf = new Aspose.Pdf.Document();
        foreach (string image in files)
        {
            var page = pdf.Pages.Add();
            var content = new Aspose.Pdf.Image { File = image };

            // legacy LandStyleIndex（Util.cs:1840-1854）：0=页宽高取图片像素、1=横向、2=纵向。
            switch (options.LandStyleIndex)
            {
                case 0:
                    (double pixelWidth, double pixelHeight) = ReadImagePixelSize(image);
                    page.PageInfo.Width = pixelWidth;
                    page.PageInfo.Height = pixelHeight;
                    break;
                case 1:
                    page.PageInfo.IsLandscape = true;
                    break;
                case 2:
                    page.PageInfo.IsLandscape = false;
                    break;
                default:
                    break;
            }

            page.PageInfo.Margin.Top = options.MarginTop;
            page.PageInfo.Margin.Left = options.MarginLeft;
            page.PageInfo.Margin.Bottom = options.MarginBottom;
            page.PageInfo.Margin.Right = options.MarginRight;

            page.Paragraphs.Add(content);
        }

        if (options.AutoOptimize)
        {
            // legacy AutoOptiomizePdf（Util.cs:1861-1865）：Optimize() + OptimizeResources()。
            pdf.Optimize();
        }

        if (options.CompressImages)
        {
            // legacy CompressPdfImage（Util.cs:1819-1831）：OptimizeResources + Flate 图片压缩。
            pdf.OptimizeResources(CreateImageOptimizationOptions());
        }

        pdf.Save(savedFile);
    }

    private static (double Width, double Height) ReadImagePixelSize(string file)
    {
        using global::System.Drawing.Image image = global::System.Drawing.Image.FromFile(file);
        return (image.Width, image.Height);
    }

    private static Aspose.Pdf.Optimization.OptimizationOptions CreateImageOptimizationOptions()
    {
        var optimization = new Aspose.Pdf.Optimization.OptimizationOptions
        {
            RemoveUnusedObjects = true,
            RemoveUnusedStreams = true,
            AllowReusePageContent = true,
            LinkDuplicateStreams = true
        };
        optimization.ImageCompressionOptions.CompressImages = true;
        optimization.ImageCompressionOptions.Encoding = Aspose.Pdf.Optimization.ImageEncoding.Flate;
        return optimization;
    }

    public void PdfToExcel(string sourceFile, string savedFile, bool oneSheetPerDocument = true)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        using var document = new Aspose.Pdf.Document(sourceFile);
        var saveOptions = new ExcelSaveOptions
        {
            Format = ExcelSaveOptions.ExcelFormat.XLSX,
            ExtractOcrSublayerOnly = true,
            MinimizeTheNumberOfWorksheets = oneSheetPerDocument
        };

        document.Save(savedFile, saveOptions);
    }

    public void PdfToPpt(string sourceFile, string savedFile, PdfToPptOptions? options = null)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));
        options ??= new PdfToPptOptions();

        using var document = new Aspose.Pdf.Document(sourceFile);
        var saveOptions = new Aspose.Pdf.PptxSaveOptions
        {
            SlidesAsImages = options.RenderAsImages,
            SeparateImages = options.SplitShapes,
            // legacy ExtraOcrBox「开启OCR子层识别（非扫描件）」对应 UnifiedSaveOptions.ExtractOcrSublayerOnly。
            ExtractOcrSublayerOnly = options.ExtraOcr
        };

        document.Save(savedFile, saveOptions);
    }

    public void WordToExcel(string sourceFile, string savedFile, bool exportSingleSheet = true)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        var document = new Aspose.Words.Document(sourceFile);
        var saveOptions = new XlsxSaveOptions
        {
            SaveFormat = Aspose.Words.SaveFormat.Xlsx,
            CompressionLevel = CompressionLevel.Maximum,
            SectionMode = exportSingleSheet
                ? XlsxSectionMode.SingleWorksheet
                : XlsxSectionMode.MultipleWorksheets
        };

        document.Save(savedFile, saveOptions);
    }

    public void ExcelToWord(string sourceFile, string savedFile)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        using var workbook = new Workbook(sourceFile);
        var saveOptions = new DocxSaveOptions
        {
            AllColumnsInOnePagePerSheet = false,
            CheckWorkbookDefaultFont = true,
            MergeAreas = true,
            ValidateMergedAreas = true,
            GridlineType = GridlineType.Hair,
            TextCrossType = TextCrossType.Default,
            OnePagePerSheet = false
        };

        workbook.Save(savedFile, saveOptions);
    }

    public void WordToEpub(string sourceFile, string savedFile, WordToEpubOptions? options = null)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));
        options ??= new WordToEpubOptions();

        // 封面图片是可选项：路径写错或文件不存在时回退为「首页渲染封面」，不打断整篇转换。
        string? coverImagePath = PathGuard.OptionalExistingFile(options.CoverImagePath);

        var document = new Aspose.Words.Document(sourceFile);
        document.BuiltInDocumentProperties.Title = Path.GetFileNameWithoutExtension(sourceFile);
        document.BuiltInDocumentProperties.Author =
            string.IsNullOrWhiteSpace(options.Author) ? "DocumentTools" : options.Author;
        document.BuiltInDocumentProperties.Company = "DocumentTools";
        document.BuiltInDocumentProperties.Thumbnail =
            BuildEpubCoverBytes(document, coverImagePath, options.CoverDpi);

        var saveOptions = new Aspose.Words.Saving.HtmlSaveOptions
        {
            Encoding = Encoding.UTF8,
            DocumentSplitCriteria = DocumentSplitCriteria.HeadingParagraph,
            ExportDocumentProperties = true,
            ExportGeneratorName = false,
            ExportPageMargins = true,
            ExportPageSetup = true,
            ExportTocPageNumbers = true,
            SaveFormat = Aspose.Words.SaveFormat.Epub
        };

        document.Save(savedFile, saveOptions);
    }

    public void EpubToWord(string sourceFile, string savedFile)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        var saveOptions = new Aspose.Html.Saving.DocSaveOptions
        {
            PageSetup = { AdjustToWidestPage = true }
        };

        Aspose.Html.Converters.Converter.ConvertEPUB(sourceFile, saveOptions, savedFile);
    }

    public void EpubToPdf(string sourceFile, string savedFile)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        var saveOptions = new Aspose.Html.Saving.PdfSaveOptions
        {
            JpegQuality = 100
        };
        saveOptions.PageSetup.AdjustToWidestPage = true;

        Aspose.Html.Converters.Converter.ConvertEPUB(sourceFile, saveOptions, savedFile);
    }

    public IReadOnlyList<string> DocumentToImages(
        string sourceFile,
        string outputDirectory,
        string imageExtension = ".png",
        int dpi = 150,
        bool grayscale = false)
        => DocumentToImages(sourceFile, outputDirectory, imageExtension, dpi, grayscale, pageExport: true);

    public IReadOnlyList<string> DocumentToImages(
        string sourceFile,
        string outputDirectory,
        string imageExtension,
        int dpi,
        bool grayscale,
        bool pageExport)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(outputDirectory, nameof(outputDirectory));
        Directory.CreateDirectory(outputDirectory);

        string extension = NormalizeImageExtension(imageExtension);
        string baseName = Path.GetFileNameWithoutExtension(sourceFile);
        string fileType = Path.GetExtension(sourceFile)?.ToLowerInvariant() ?? string.Empty;
        var outputs = new List<string>();

        if (!pageExport && fileType is ".doc" or ".docx")
        {
            // legacy PageExport=false（Util.cs:2505-2548）只支持 Word，输出「整篇一张长图」。
            string merged = Path.Combine(outputDirectory, baseName + extension);
            MergeWordPagesToSingleImage(sourceFile, merged, extension, dpi);
            outputs.Add(merged);
            return outputs;
        }

        switch (fileType)
        {
            case ".doc":
            case ".docx":
                ExportWordPages(sourceFile, outputDirectory, baseName, extension, dpi, grayscale, outputs);
                break;

            case ".pdf":
                ExportPdfPages(sourceFile, outputDirectory, baseName, extension, dpi, outputs);
                break;

            case ".xls":
            case ".xlsx":
                ExportExcelSheets(sourceFile, outputDirectory, baseName, extension, dpi, outputs);
                break;

            default:
                throw new NotSupportedException($"不支持转图片的文件类型：{fileType}");
        }

        return outputs;
    }

    /// <summary>
    /// legacy PageExport=false 时 Word 合并导出（Util.cs:2505-2548）。
    /// legacy 走 SVG 加 <c>Aspose.Imaging</c> 光栅化；本项目未引用 Aspose.Imaging 且禁止加包，
    /// 故改用等价路径：Word 转 PDF，再逐页 PngDevice 光栅化，最后用 System.Drawing 纵向拼接成一张长图。
    /// </summary>
    private static void MergeWordPagesToSingleImage(
        string sourceFile,
        string targetFile,
        string extension,
        int dpi)
    {
        int resolution = Math.Clamp(dpi, 24, 600);
        string tempDirectory = Path.Combine(
            Path.GetTempPath(),
            "DocumentTools_word2image_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);

        try
        {
            string tempPdf = Path.Combine(tempDirectory, "source.pdf");
            var document = new Aspose.Words.Document(sourceFile);
            document.Save(tempPdf, Aspose.Words.SaveFormat.Pdf);

            var pageImages = new List<string>();
            using (var pdf = new Aspose.Pdf.Document(tempPdf))
            {
                var device = new Aspose.Pdf.Devices.PngDevice(new Aspose.Pdf.Devices.Resolution(resolution));
                for (int pageIndex = 1; pageIndex <= pdf.Pages.Count; pageIndex++)
                {
                    string pageFile = Path.Combine(tempDirectory, $"第{pageIndex}页.png");
                    using Stream stream = File.Create(pageFile);
                    device.Process(pdf.Pages[pageIndex], stream);
                    pageImages.Add(pageFile);
                }
            }

            ComposeVertically(pageImages, targetFile, extension);
        }
        finally
        {
            try
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
            catch (IOException)
            {
                // 临时目录清理失败不影响产物。
            }
        }
    }

    private static void ComposeVertically(IReadOnlyList<string> pageImages, string targetFile, string extension)
    {
        if (pageImages.Count == 0)
        {
            throw new InvalidOperationException("文档没有可导出的页面。");
        }

        var pages = new List<global::System.Drawing.Image>(pageImages.Count);
        try
        {
            foreach (string pageImage in pageImages)
            {
                pages.Add(global::System.Drawing.Image.FromFile(pageImage));
            }

            int width = pages.Max(page => page.Width);
            int height = pages.Sum(page => page.Height);
            using var canvas = new global::System.Drawing.Bitmap(width, height);
            using (global::System.Drawing.Graphics graphics = global::System.Drawing.Graphics.FromImage(canvas))
            {
                graphics.Clear(global::System.Drawing.Color.White);
                int top = 0;
                foreach (global::System.Drawing.Image page in pages)
                {
                    graphics.DrawImage(page, 0, top, page.Width, page.Height);
                    top += page.Height;
                }
            }

            SaveBitmap(canvas, targetFile, extension);
        }
        finally
        {
            foreach (global::System.Drawing.Image page in pages)
            {
                page.Dispose();
            }
        }
    }

    private static void SaveBitmap(global::System.Drawing.Bitmap bitmap, string targetFile, string extension)
    {
        global::System.Drawing.Imaging.ImageFormat format = global::System.Drawing.Imaging.ImageFormat.Png;
        if (extension is ".jpg")
        {
            format = global::System.Drawing.Imaging.ImageFormat.Jpeg;
        }
        else if (extension is ".bmp")
        {
            format = global::System.Drawing.Imaging.ImageFormat.Bmp;
        }

        if (format == global::System.Drawing.Imaging.ImageFormat.Jpeg)
        {
            global::System.Drawing.Imaging.ImageCodecInfo codec = global::System.Drawing.Imaging.ImageCodecInfo
                .GetImageEncoders()
                .First(item => item.FormatID == global::System.Drawing.Imaging.ImageFormat.Jpeg.Guid);
            using var parameters = new global::System.Drawing.Imaging.EncoderParameters(1);
            parameters.Param[0] = new global::System.Drawing.Imaging.EncoderParameter(
                global::System.Drawing.Imaging.Encoder.Quality,
                100L);
            bitmap.Save(targetFile, codec, parameters);
            return;
        }

        bitmap.Save(targetFile, format);
    }

    public void PdfToCompliance(string sourceFile, string savedFile, string pdfFormat = "v_1_7")
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));

        PdfFormat format = ParsePdfFormat(pdfFormat);

        using var document = new Aspose.Pdf.Document(sourceFile);
        using var conversionLog = new MemoryStream();
        document.Convert(conversionLog, format, ConvertErrorAction.Delete);
        document.Save(savedFile);
    }

    /// <summary>
    /// EPUB 封面：<paramref name="coverImagePath"/> 非空（调用方已确认存在）就用图片原字节，
    /// 否则把第一页按 <paramref name="coverDpi"/> 渲染成 JPEG 当封面（对应 legacy ConvertWord2EPUB）。
    /// </summary>
    private static byte[] BuildEpubCoverBytes(
        Aspose.Words.Document document,
        string? coverImagePath,
        int coverDpi)
    {
        if (coverImagePath is not null)
        {
            return File.ReadAllBytes(coverImagePath);
        }

        int resolution = Math.Clamp(coverDpi, 24, 600);
        var imageOptions = new WordsImageSaveOptions(Aspose.Words.SaveFormat.Jpeg)
        {
            PageSet = new PageSet(0),
            Resolution = resolution
        };

        using var stream = new MemoryStream();
        document.Save(stream, imageOptions);
        return stream.ToArray();
    }

    private static void ExportWordPages(
        string sourceFile,
        string outputDirectory,
        string baseName,
        string extension,
        int dpi,
        bool grayscale,
        List<string> outputs)
    {
        var document = new Aspose.Words.Document(sourceFile);
        WordsImageSaveOptions saveOptions = CreateWordImageSaveOptions(extension, dpi, grayscale);

        for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
        {
            string pageFile = Path.Combine(outputDirectory, $"{baseName}_第{pageIndex + 1}页{extension}");
            saveOptions.PageSet = new PageSet(pageIndex);
            document.Save(pageFile, saveOptions);
            outputs.Add(pageFile);
        }
    }

    private static void ExportPdfPages(
        string sourceFile,
        string outputDirectory,
        string baseName,
        string extension,
        int dpi,
        List<string> outputs)
    {
        using var document = new Aspose.Pdf.Document(sourceFile);
        Aspose.Pdf.Devices.ImageDevice device = CreatePdfDevice(extension, dpi);

        for (int pageIndex = 1; pageIndex <= document.Pages.Count; pageIndex++)
        {
            string pageFile = Path.Combine(outputDirectory, $"{baseName}_第{pageIndex}页{extension}");
            device.Process(document.Pages[pageIndex], pageFile);
            outputs.Add(pageFile);
        }
    }

    private static void ExportExcelSheets(
        string sourceFile,
        string outputDirectory,
        string baseName,
        string extension,
        int dpi,
        List<string> outputs)
    {
        using var workbook = new Workbook(sourceFile);
        ImageOrPrintOptions printOptions = CreateExcelPrintOptions(extension, dpi);

        for (int sheetIndex = 0; sheetIndex < workbook.Worksheets.Count; sheetIndex++)
        {
            Worksheet sheet = workbook.Worksheets[sheetIndex];
            if (!sheet.IsVisible)
            {
                continue;
            }

            string sheetFile = Path.Combine(outputDirectory, $"{baseName}_{sheet.Name}{extension}");
            var render = new SheetRender(sheet, printOptions);
            render.ToImage(0, sheetFile);
            outputs.Add(sheetFile);
        }
    }

    private static WordsImageSaveOptions CreateWordImageSaveOptions(string extension, int dpi, bool grayscale = false)
    {
        WordsImageSaveOptions saveOptions = extension switch
        {
            ".jpg" or ".jpeg" => new WordsImageSaveOptions(Aspose.Words.SaveFormat.Jpeg)
            {
                JpegQuality = 100,
                PrettyFormat = true
            },
            ".bmp" => new WordsImageSaveOptions(Aspose.Words.SaveFormat.Bmp),
            _ => new WordsImageSaveOptions(Aspose.Words.SaveFormat.Png)
        };

        saveOptions.UseHighQualityRendering = true;
        saveOptions.Resolution = dpi;
        saveOptions.ColorMode = grayscale ? ColorMode.Grayscale : ColorMode.Normal;
        saveOptions.UseAntiAliasing = true;
        saveOptions.AllowEmbeddingPostScriptFonts = true;
        return saveOptions;
    }

    private static Aspose.Pdf.Devices.ImageDevice CreatePdfDevice(string extension, int dpi)
    {
        var resolution = new Aspose.Pdf.Devices.Resolution(dpi);
        Aspose.Pdf.Devices.ImageDevice device = extension switch
        {
            ".jpg" or ".jpeg" => new Aspose.Pdf.Devices.JpegDevice(resolution, 100),
            ".bmp" => new Aspose.Pdf.Devices.BmpDevice(resolution),
            _ => new Aspose.Pdf.Devices.PngDevice(resolution)
        };

        device.RenderingOptions.DefaultFontName = "SimSun";
        return device;
    }

    private static ImageOrPrintOptions CreateExcelPrintOptions(string extension, int dpi)
    {
        var printOptions = new ImageOrPrintOptions
        {
            CheckWorkbookDefaultFont = true,
            HorizontalResolution = dpi,
            VerticalResolution = dpi,
            DefaultFont = "SimSun"
        };

        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                printOptions.ImageType = Aspose.Cells.Drawing.ImageType.Jpeg;
                printOptions.Quality = 100;
                break;
            case ".bmp":
                printOptions.ImageType = Aspose.Cells.Drawing.ImageType.Bmp;
                break;
            default:
                printOptions.ImageType = Aspose.Cells.Drawing.ImageType.Png;
                break;
        }

        return printOptions;
    }

    private static string NormalizeImageExtension(string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
        {
            return ".png";
        }

        string value = extension.Trim().ToLowerInvariant();
        if (!value.StartsWith('.'))
        {
            value = "." + value;
        }

        return value switch
        {
            ".jpeg" => ".jpg",
            ".jpg" or ".png" or ".bmp" => value,
            _ => ".png"
        };
    }

    private static PdfFormat ParsePdfFormat(string pdfFormat)
    {
        if (string.IsNullOrWhiteSpace(pdfFormat))
        {
            return PdfFormat.v_1_7;
        }

        string key = pdfFormat.Trim();
        if (Enum.TryParse(key, ignoreCase: true, out PdfFormat format))
        {
            return format;
        }

        // Common aliases from the legacy UI combo box.
        return key.ToLowerInvariant() switch
        {
            "1.3" or "pdf13" or "v13" => PdfFormat.v_1_3,
            "1.4" or "pdf14" or "v14" => PdfFormat.v_1_4,
            "1.5" or "pdf15" or "v15" => PdfFormat.v_1_5,
            "1.6" or "pdf16" or "v16" => PdfFormat.v_1_6,
            "1.7" or "pdf17" or "v17" => PdfFormat.v_1_7,
            "pdf/a-1a" or "pdfa1a" => PdfFormat.PDF_A_1A,
            "pdf/a-1b" or "pdfa1b" => PdfFormat.PDF_A_1B,
            "pdf/a-2a" or "pdfa2a" => PdfFormat.PDF_A_2A,
            "pdf/a-2b" or "pdfa2b" => PdfFormat.PDF_A_2B,
            "pdf/a-3a" or "pdfa3a" => PdfFormat.PDF_A_3A,
            "pdf/a-3b" or "pdfa3b" => PdfFormat.PDF_A_3B,
            _ => throw new ArgumentException($"不支持的 PDF 格式：{pdfFormat}", nameof(pdfFormat))
        };
    }
}