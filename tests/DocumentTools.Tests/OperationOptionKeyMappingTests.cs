using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Image;
using DocumentTools.Core.Services.Office;
using CellsWorkbook = Aspose.Cells.Workbook;

namespace DocumentTools.Tests;

/// <summary>
/// 选项键 → 强类型 Options 属性 → 值 的映射锁（S3）。
/// <para>
/// <c>BuildOptions</c> 全是字符串字面量，键名写错或读到相邻功能的同名键时，
/// <c>FeatureOptionSchemaReachabilityTests</c> 只证明「Schema 声明了且有人引用」，
/// 证不出「这个功能读的是自己的键」。这里用记录型假服务截获每次调用收到的 Options，
/// 断言三元组；不断言「不抛异常」（那种断言恒绿）。
/// </para>
/// <para>三类重点：同名不同义（quality）、同词根带前缀（pdf./ppt./word.）、本轮新接的键。</para>
/// </summary>
public sealed class OperationOptionKeyMappingTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("documenttools-keymap-").FullName;

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
            // 真实转换用例可能仍持有临时文件句柄，清理失败不影响断言。
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    // ───────────────────────── word.pdf ─────────────────────────

    [Fact]
    public async Task WordToPdf_SevenKeys_MapToOwnProperties()
    {
        var recorder = new RecordingConvertService();
        var keys = Keys(
            ("embedFonts", "true"),
            ("updateFields", "true"),
            ("useLocalOffice", "true"),
            ("compliance", "PdfA1b"),
            ("colorMode", "Grayscale"),
            ("msWordVersion", "Word2010"),
            ("password", "s3cret"));

        DocumentOperationResult result = await ConvertAsync("word.pdf", recorder, keys);

        Assert.True(result.Success, result.Message);
        WordToPdfOptions options = Assert.IsType<WordToPdfOptions>(recorder.LastWordToPdf);
        Same("embedFonts", true, options.EmbedFonts);
        Same("updateFields", true, options.UpdateFields);
        Same("useLocalOffice", true, options.UseLocalOffice);
        Same("compliance", "PdfA1b", options.Compliance);
        Same("colorMode", "Grayscale", options.ColorMode);
        Same("msWordVersion", "Word2010", options.MsWordVersion);
        Same("password", "s3cret", options.Password);
    }

    [Fact]
    public async Task WordToPdf_Defaults_MatchLegacyAndBlankPasswordIsNull()
    {
        var recorder = new RecordingConvertService();
        DocumentOperationResult result = await ConvertAsync("word.pdf", recorder, Keys(("password", "   ")));

        Assert.True(result.Success, result.Message);
        WordToPdfOptions options = Assert.IsType<WordToPdfOptions>(recorder.LastWordToPdf);
        Same("compliance(缺省)", "Pdf17", options.Compliance);
        Same("colorMode(缺省)", "Normal", options.ColorMode);
        Same("msWordVersion(缺省)", "Word2016", options.MsWordVersion);
        Same("useLocalOffice(缺省)", false, options.UseLocalOffice);
        Same("password(空白)", null, options.Password);
    }

    // ───────────────────────── pdf.word ─────────────────────────

    [Fact]
    public async Task PdfToWord_FourKeys_MapToOwnProperties()
    {
        var recorder = new RecordingConvertService();
        var keys = Keys(
            ("useBullet", "true"),
            ("useOcr", "true"),
            ("mode", "Textbox"),
            ("password", "pw"));

        DocumentOperationResult result = await ConvertAsync("pdf.word", recorder, keys);

        Assert.True(result.Success, result.Message);
        PdfToWordOptions options = Assert.IsType<PdfToWordOptions>(recorder.LastPdfToWord);
        Same("useBullet", true, options.UseBullet);
        Same("useOcr", true, options.UseOcr);
        Same("mode", "Textbox", options.Mode);
        Same("password", "pw", options.Password);
    }

    // ───────────────────────── excel.pdf ─────────────────────────

    [Fact]
    public async Task ExcelToPdf_EightKeys_MapToOwnProperties()
    {
        var recorder = new RecordingConvertService();
        var keys = Keys(
            ("eachSheet", "true"),
            ("fitOnePage", "true"),
            ("horPrint", "true"),
            ("verPrint", "true"),
            ("printScaleRow", "2"),
            ("printScaleCol", "3"),
            ("printScalePercentEnable", "true"),
            ("printScalePercent", "85"));

        DocumentOperationResult result = await ConvertAsync("excel.pdf", recorder, keys);

        Assert.True(result.Success, result.Message);
        ExcelToPdfOptions options = Assert.IsType<ExcelToPdfOptions>(recorder.LastExcelToPdf);
        Same("eachSheet", true, options.ExportEachSheet);
        Same("fitOnePage", true, options.ForceContentOnOnePage);
        Same("horPrint", true, options.HorizontalCenter);
        Same("verPrint", true, options.VerticalCenter);
        Same("printScaleRow", 2, options.PrintScaleRow);
        Same("printScaleCol", 3, options.PrintScaleCol);
        Same("printScalePercentEnable", true, options.PrintScalePercentEnabled);
        Same("printScalePercent", 85, options.PrintScalePercent);
    }

    // ───────────────────────── ppt.pdf（前缀同类：word./ppt./pdf. 各读各的） ─────────────────────────

    [Fact]
    public async Task PptToPdf_OwnKeysOnly_DoNotLeakToWordOrExcel()
    {
        var recorder = new RecordingConvertService();
        var keys = Keys(
            ("drawBlackLine", "true"),
            ("hiddenSlides", "true"),
            ("password", "pw"),
            // 同名前缀对照：word.pdf 的 updateFields / excel.pdf 的 eachSheet 不该出现在 ppt.pdf 的 DTO 里。
            ("updateFields", "true"),
            ("eachSheet", "true"));

        DocumentOperationResult result = await ConvertAsync("ppt.pdf", recorder, keys);

        Assert.True(result.Success, result.Message);
        PptToPdfOptions options = Assert.IsType<PptToPdfOptions>(recorder.LastPptToPdf);
        Same("ppt.pdf/drawBlackLine", true, options.DrawBlackFrame);
        Same("ppt.pdf/hiddenSlides", true, options.ExportHiddenSlides);
        Same("ppt.pdf/password", "pw", options.Password);
        Same("ppt.pdf/exportNotes(缺省)", "None", options.NotesPosition);
        // 录制器只收到一次调用，且不是 word.pdf / excel.pdf 的 DTO。
        Same("调用次数", 1, recorder.Calls);
        Same("word.pdf 未被调用", null, recorder.LastWordToPdf);
        Same("excel.pdf 未被调用", null, recorder.LastExcelToPdf);
    }

    [Fact]
    public async Task PptToPdf_ExportNotes_AcceptsPositionNamesAndLegacyBooleans()
    {
        var byName = new RecordingConvertService();
        await ConvertAsync("ppt.pdf", byName, Keys(("exportNotes", "BottomTruncated")));
        Same("exportNotes=BottomTruncated", "BottomTruncated",
            Assert.IsType<PptToPdfOptions>(byName.LastPptToPdf).NotesPosition);

        // 旧配置存的是 CheckBox 的 true/false：true 等价于「页底完整」。
        var byLegacyTrue = new RecordingConvertService();
        await ConvertAsync("ppt.pdf", byLegacyTrue, Keys(("exportNotes", "true")));
        Same("exportNotes=true → BottomFull", "BottomFull",
            Assert.IsType<PptToPdfOptions>(byLegacyTrue.LastPptToPdf).NotesPosition);

        var byLegacyFalse = new RecordingConvertService();
        await ConvertAsync("ppt.pdf", byLegacyFalse, Keys(("exportNotes", "false")));
        Same("exportNotes=false → None", "None",
            Assert.IsType<PptToPdfOptions>(byLegacyFalse.LastPptToPdf).NotesPosition);
    }

    // ───────────────────────── image.pdf ─────────────────────────

    [Fact]
    public async Task ImagePdf_SevenKeys_MapToImagePdfOptions()
    {
        var recorder = new RecordingConvertService();
        var keys = Keys(
            ("compress", "false"),
            ("autoOptimize", "true"),
            ("orientation", "横向"),
            ("marginTop", "1.5"),
            ("marginLeft", "2.5"),
            ("marginBottom", "3.5"),
            ("marginRight", "4.5"));

        DocumentOperationResult result = await ConvertAsync("image.pdf", recorder, keys);

        Assert.True(result.Success, result.Message);
        ImagePdfOptions options = Assert.IsType<ImagePdfOptions>(recorder.ImagePdf);
        Same("compress", false, options.CompressImages);
        Same("autoOptimize", true, options.AutoOptimize);
        Same("orientation=横向", 1, options.LandStyleIndex);
        Same("marginTop", 1.5, options.MarginTop);
        Same("marginLeft", 2.5, options.MarginLeft);
        Same("marginBottom", 3.5, options.MarginBottom);
        Same("marginRight", 4.5, options.MarginRight);
    }

    [Fact]
    public async Task ImagePdf_DefaultsAndOrientationAliases()
    {
        var recorder = new RecordingConvertService();
        await ConvertAsync("image.pdf", recorder, Keys());
        ImagePdfOptions options = Assert.IsType<ImagePdfOptions>(recorder.ImagePdf);
        Same("compress(缺省)", true, options.CompressImages);
        Same("autoOptimize(缺省)", false, options.AutoOptimize);
        Same("orientation(缺省)", 0, options.LandStyleIndex);

        var portrait = new RecordingConvertService();
        await ConvertAsync("image.pdf", portrait, Keys(("orientation", "纵向")));
        Same("orientation=纵向", 2, Assert.IsType<ImagePdfOptions>(portrait.ImagePdf).LandStyleIndex);

        var numeric = new RecordingConvertService();
        await ConvertAsync("image.pdf", numeric, Keys(("orientation", "1")));
        Same("orientation=1", 1, Assert.IsType<ImagePdfOptions>(numeric.ImagePdf).LandStyleIndex);
    }

    // ───────────────────────── pdf.excel ─────────────────────────

    [Fact]
    public async Task PdfToExcel_OneSheetKeys_IncludeLegacyAlias()
    {
        var byDefault = new RecordingConvertService();
        await ConvertAsync("pdf.excel", byDefault, Keys());
        Same("oneSheet(缺省)", true, byDefault.PdfToExcelOneSheet);

        var byNewKey = new RecordingConvertService();
        await ConvertAsync("pdf.excel", byNewKey, Keys(("oneSheet", "false")));
        Same("oneSheet=false", false, byNewKey.PdfToExcelOneSheet);

        // 老配置的键名仍要生效（Schema 早期写的是 oneSheetPerDocument）。
        var byLegacyKey = new RecordingConvertService();
        await ConvertAsync("pdf.excel", byLegacyKey, Keys(("oneSheetPerDocument", "false")));
        Same("oneSheetPerDocument=false", false, byLegacyKey.PdfToExcelOneSheet);
    }

    // ───────────────────────── pdf.ppt ─────────────────────────

    [Fact]
    public async Task PdfToPpt_ThreeKeys_MapToOwnProperties()
    {
        var recorder = new RecordingConvertService();
        var keys = Keys(("extraOcr", "true"), ("splitShape", "true"), ("renderImage", "true"));

        DocumentOperationResult result = await ConvertAsync("pdf.ppt", recorder, keys);

        Assert.True(result.Success, result.Message);
        PdfToPptOptions options = Assert.IsType<PdfToPptOptions>(recorder.LastPdfToPpt);
        Same("extraOcr", true, options.ExtraOcr);
        Same("splitShape", true, options.SplitShapes);
        Same("renderImage", true, options.RenderAsImages);
    }

    // ───────────────────────── word.excel ─────────────────────────

    [Fact]
    public async Task WordToExcel_ExportSingleSheet_OnlyLiteralFalseFlips()
    {
        var byDefault = new RecordingConvertService();
        await ConvertAsync("word.excel", byDefault, Keys());
        Same("exportSingleSheet(缺省)", true, byDefault.WordToExcelExportSingleSheet);

        var explicitFalse = new RecordingConvertService();
        await ConvertAsync("word.excel", explicitFalse, Keys(("exportSingleSheet", "false")));
        Same("exportSingleSheet=false", false, explicitFalse.WordToExcelExportSingleSheet);

        var upperFalse = new RecordingConvertService();
        await ConvertAsync("word.excel", upperFalse, Keys(("exportSingleSheet", "FALSE")));
        Same("exportSingleSheet=FALSE", false, upperFalse.WordToExcelExportSingleSheet);

        // legacy 只认字面 false：0/no 保持「单表」。
        var byZero = new RecordingConvertService();
        await ConvertAsync("word.excel", byZero, Keys(("exportSingleSheet", "0")));
        Same("exportSingleSheet=0", true, byZero.WordToExcelExportSingleSheet);
    }

    // ───────────────────────── word.image ─────────────────────────

    [Fact]
    public async Task WordImage_FourKeys_MapToDocumentToImagesArguments()
    {
        var recorder = new RecordingConvertService();
        var keys = Keys(
            ("extension", ".jpg"),
            ("dpi", "300"),
            ("gray", "true"),
            ("pageExport", "false"));

        DocumentOperationResult result = await ConvertAsync("word.image", recorder, keys);

        Assert.True(result.Success, result.Message);
        (string Extension, int Dpi, bool Gray, bool PageExport) call =
            recorder.WordImage ?? throw new Xunit.Sdk.XunitException("word.image 未调用 DocumentToImages。");
        Same("extension", ".jpg", call.Extension);
        Same("dpi", 300, call.Dpi);
        Same("gray", true, call.Gray);
        Same("pageExport", false, call.PageExport);
    }

    [Fact]
    public async Task WordImage_PageExportDefaultsToTrue_AndDpiInvalidFallsBack()
    {
        var recorder = new RecordingConvertService();
        await ConvertAsync("word.image", recorder, Keys(("dpi", "不是数字")));
        (string Extension, int Dpi, bool Gray, bool PageExport) call = recorder.WordImage!.Value;
        Same("pageExport(缺省)", true, call.PageExport);
        Same("dpi(非法回退)", 150, call.Dpi);
        Same("extension(缺省)", ".png", call.Extension);
        Same("gray(缺省)", false, call.Gray);
    }

    // ───────────────────────── word.epub ─────────────────────────

    [Fact]
    public async Task WordToEpub_ThreeKeys_MapToOwnProperties()
    {
        string cover = Path.Combine(_root, "cover.png");
        File.WriteAllBytes(cover, [0x89, 0x50, 0x4E, 0x47]);

        var recorder = new RecordingConvertService();
        var keys = Keys(("author", "韩梅梅"), ("coverImage", cover), ("coverDpi", "240"));

        DocumentOperationResult result = await ConvertAsync("word.epub", recorder, keys);

        Assert.True(result.Success, result.Message);
        WordToEpubOptions options = Assert.IsType<WordToEpubOptions>(recorder.LastWordToEpub);
        Same("author", "韩梅梅", options.Author);
        Same("coverImage", cover, options.CoverImagePath);
        Same("coverDpi", 240, options.CoverDpi);
    }

    [Fact]
    public async Task WordToEpub_BlankCoverImage_IsNullAndDefaultsHold()
    {
        var recorder = new RecordingConvertService();
        await ConvertAsync("word.epub", recorder, Keys(("coverImage", "  "), ("author", "")));
        WordToEpubOptions options = Assert.IsType<WordToEpubOptions>(recorder.LastWordToEpub);
        Same("coverImage(空白)", null, options.CoverImagePath);
        Same("author(空白回退)", "DocumentTools", options.Author);
        Same("coverDpi(缺省)", 100, options.CoverDpi);
    }

    // ───────────────────────── pdf.compliance ─────────────────────────

    [Fact]
    public async Task PdfCompliance_VersionWinsOverFormat_AndMapsLegacyNames()
    {
        var byVersion = new RecordingConvertService();
        await ConvertAsync("pdf.compliance", byVersion, Keys(("version", "1.4"), ("format", "1.7")));
        Same("version=1.4", "v_1_4", byVersion.ComplianceFormat);

        var byFormat = new RecordingConvertService();
        await ConvertAsync("pdf.compliance", byFormat, Keys(("format", "PDF/A-1b")));
        Same("format=PDF/A-1b", "PDF_A_1B", byFormat.ComplianceFormat);

        var byDefault = new RecordingConvertService();
        await ConvertAsync("pdf.compliance", byDefault, Keys());
        // dev 下拉铺 GetNames(PdfFormat) 27 项、默认第 0 项（BodyForm.cs:1223）。
        Same("缺省", "PDF_A_1A", byDefault.ComplianceFormat);
    }

    // ───────────────────────── 无参数功能：必须仍调用对的服务方法 ─────────────────────────

    [Fact]
    public async Task NoOptionFeatures_StillCallTheirOwnServiceMethod()
    {
        foreach ((string featureId, string expectedMethod) in new[]
                 {
                     ("excel.word", "ExcelToWord"),
                     ("epub.word", "EpubToWord"),
                     ("epub.pdf", "EpubToPdf")
                 })
        {
            var recorder = new RecordingConvertService();
            DocumentOperationResult result = await ConvertAsync(featureId, recorder, Keys());
            Assert.True(result.Success, $"{featureId}: {result.Message}");
            Same(featureId, expectedMethod, recorder.LastMethod);
        }
    }

    // ───────────────────────── 拆分类新键（ppt.split / pdf.split / word.split） ─────────────────────────

    [Fact]
    public async Task PptSplit_SlideSizeAndScale_ReachSlideSplitOptions()
    {
        var split = new RecordingSplitService();
        var pdfSplit = new RecordingPdfSplitService();
        var request = Request("ppt.split", Keys(("slideSize", "A4Paper"), ("slideScale", "EnsureFit")));

        await new SplitDocumentOperation("ppt.split", split, pdfSplit).ExecuteAsync(request);

        SlideSplitOptions options = Assert.IsType<SlideSplitOptions>(split.Slides);
        Same("slideSize", "A4Paper", options.SlideSizeName);
        Same("slideScale", "EnsureFit", options.SlideScaleName);
    }

    [Fact]
    public async Task PdfSplit_DeleteKeys_AreIndependentFromRangeText()
    {
        var split = new RecordingSplitService();
        var pdfSplit = new RecordingPdfSplitService();
        var request = Request("pdf.split", Keys(("deleteStart", "2"), ("deleteEnd", "4")));

        await new SplitDocumentOperation("pdf.split", split, pdfSplit).ExecuteAsync(request);

        PdfSplitOptions options = Assert.Single(pdfSplit.Calls);
        Same("deleteStart/deleteEnd→DeletePages", true, options.DeletePages);
        Same("DeleteStartPage", 2, options.DeleteStartPage);
        Same("DeleteEndPage", 4, options.DeleteEndPage);
        Same("未填 range 时不得走拆分", false, options.SplitIntoChunks);
    }

    [Fact]
    public async Task PdfSplit_LegacyDeleteFlag_FallsBackToRangeKey()
    {
        var split = new RecordingSplitService();
        var pdfSplit = new RecordingPdfSplitService();
        var request = Request("pdf.split", Keys(("deletePages", "true"), ("range", "3-5")));

        await new SplitDocumentOperation("pdf.split", split, pdfSplit).ExecuteAsync(request);

        PdfSplitOptions options = Assert.Single(pdfSplit.Calls);
        Same("deletePages+range→DeleteStartPage", 3, options.DeleteStartPage);
        Same("deletePages+range→DeleteEndPage", 5, options.DeleteEndPage);
    }

    [Fact]
    public async Task WordSplit_Keys_ReachWordSplitOptions()
    {
        var split = new RecordingSplitService();
        var pdfSplit = new RecordingPdfSplitService();
        var request = Request("word.split", Keys(
            ("splitMode", "按页数"),
            ("pagesPerFile", "2"),
            ("refreshPageNumber", "true")));

        await new SplitDocumentOperation("word.split", split, pdfSplit).ExecuteAsync(request);

        WordSplitOptions options = Assert.IsType<WordSplitOptions>(split.Words);
        Same("splitMode=按页数→SplitEveryPages", true, options.SplitEveryPages);
        Same("pagesPerFile", 2, options.PagesPerFile);
        Same("refreshPageNumber", true, options.ResetPageNumber);
    }

    [Fact]
    public async Task WordSplit_EveryPageMode_ForcesOnePagePerFile()
    {
        // 「每页拆分」的语义就是一页一份：此时 pagesPerFile 被忽略（强制 1），
        // 锁住这点，避免有人把 words 的页数框当成每页模式的生效项。
        var split = new RecordingSplitService();
        var pdfSplit = new RecordingPdfSplitService();
        await new SplitDocumentOperation("word.split", split, pdfSplit).ExecuteAsync(
            Request("word.split", Keys(("splitMode", "每页拆分"), ("pagesPerFile", "3"))));

        WordSplitOptions options = Assert.IsType<WordSplitOptions>(split.Words);
        Same("每页拆分→PagesPerFile 强制 1", 1, options.PagesPerFile);
    }

    // ───────────────────────── word.replace / excel.replace ─────────────────────────

    [Fact]
    public async Task Replace_RegexFlagAlone_DoesNotEnableCaptureSubstitution()
    {
        var onlyRegex = new RecordingReplaceService();
        await ReplaceAsync("word.replace", onlyRegex, Keys(("regex", "true"), ("pairs", "a=>b")));
        DocumentReplaceOptions regexOptions = Assert.IsType<DocumentReplaceOptions>(onlyRegex.LastOptions);
        Same("regex", true, regexOptions.UseRegex);
        Same("regexSub(未勾)", false, regexOptions.UseRegexSubstitutions);

        var withSub = new RecordingReplaceService();
        await ReplaceAsync("word.replace", withSub, Keys(("regexSub", "true"), ("pairs", "a=>$1")));
        Same("regexSub", true, Assert.IsType<DocumentReplaceOptions>(withSub.LastOptions).UseRegexSubstitutions);
    }

    [Fact]
    public async Task Replace_StyleKeys_MapToReplaceOptions()
    {
        var recorder = new RecordingReplaceService();
        await ReplaceAsync("word.replace", recorder, Keys(
            ("pairs", "a=>b"),
            ("replaceUseNewStyle", "true"),
            ("replaceFontName", "  KaiTi  "),
            ("replaceFontSize", "14.5"),
            ("replaceBold", "加粗")));

        DocumentReplaceOptions options = Assert.IsType<DocumentReplaceOptions>(recorder.LastOptions);
        Same("replaceUseNewStyle", true, options.UseNewStyle);
        Same("replaceFontName(Trim)", "KaiTi", options.FontName);
        Same("replaceFontSize", 14.5, options.FontSize);
        Same("replaceBold=加粗", true, options.Bold);
    }

    [Fact]
    public async Task Replace_PdfOnlyKeys_DoNotLeakIntoWordReplace()
    {
        var recorder = new RecordingReplaceService();
        await ReplaceAsync("word.replace", recorder, Keys(
            ("pairs", "a=>b"),
            ("excelRemoveRows", "true"),
            ("excelStartRow", "2"),
            ("excelEndRow", "4")));

        DocumentReplaceOptions options = Assert.IsType<DocumentReplaceOptions>(recorder.LastOptions);
        // word.replace 也带这两列是因为 DTO 共用；关键是不能把 excel 的键当成别的语义。
        Same("excelRemoveRows(透传)", true, options.ExcelRemoveRows);
        Same("excelStartRow", 2, options.ExcelStartRow);
        Same("excelEndRow", 4, options.ExcelEndRow);
    }

    [Fact]
    public async Task ExcelReplace_EndRowBeyondUsedRange_ClampsInsteadOfThrowing()
    {
        string source = Path.Combine(_root, "rows.xlsx");
        using (var workbook = new CellsWorkbook())
        {
            var cells = workbook.Worksheets[0].Cells;
            for (int row = 0; row < 5; row++)
            {
                cells[row, 0].PutValue($"R{row + 1}");
            }

            workbook.Save(source, Aspose.Cells.SaveFormat.Xlsx);
        }

        string saved = Path.Combine(_root, "rows_out.xlsx");
        var service = new DocumentReplaceService();
        service.ReplaceExcel(source, saved, new DocumentReplaceOptions
        {
            Pairs = [("R1", "R1")],
            ExcelRemoveRows = true,
            ExcelStartRow = 2,
            ExcelEndRow = 99 // 远超实际末行：应夹到末行，而不是把 Aspose 的英文 CellsException 抛给用户。
        });

        using var result = new CellsWorkbook(saved);
        var after = result.Worksheets[0].Cells;
        Same("A1 保留", "R1", after[0, 0].StringValue);
        Same("末行夹到实际末行", 0, after.MaxRow);
    }

    // ───────────────────────── pdf.compress（同名不同义：quality） ─────────────────────────

    [Fact]
    public async Task PdfCompress_Keys_MapToCompressionOptionsWithTrueDefaults()
    {
        var recorder = new RecordingCompressionService();
        await CompressAsync(recorder, Keys(
            ("webOptimize", "true"),
            ("quality", "高"),
            ("imageVersion", "Fast"),
            ("maxDpi", "120"),
            ("flate", "true"),
            ("unembedFonts", "true"),
            ("flattenForms", "true")));

        PdfCompressionOptions options = Assert.IsType<PdfCompressionOptions>(recorder.LastOptions);
        Same("webOptimize", true, options.WebOptimize);
        Same("quality=高", 92, options.ImageQuality);
        Same("imageVersion=Fast", PdfImageCompressionVersion.Fast, options.ImageVersion);
        Same("maxDpi", 120, options.MaxResolutionDpi);
        Same("flate", true, options.UseFlateEncoding);
        Same("unembedFonts", true, options.UnembedFonts);
        Same("flattenForms", true, options.FlattenFormFields);
    }

    [Fact]
    public async Task PdfCompress_ResourceSwitches_StayTrueWhenKeyMissing()
    {
        var byDefault = new RecordingCompressionService();
        await CompressAsync(byDefault, Keys());
        PdfCompressionOptions defaults = Assert.IsType<PdfCompressionOptions>(byDefault.LastOptions);
        Same("removeUnusedStreams(缺省)", true, defaults.RemoveUnusedStreams);
        Same("linkDuplicateStreams(缺省)", true, defaults.LinkDuplicateStreams);
        Same("removeUnusedObjects(缺省)", true, defaults.RemoveUnusedObjects);
        Same("skipImages(缺省)", true, defaults.CompressImages);

        var allOff = new RecordingCompressionService();
        await CompressAsync(allOff, Keys(
            ("removeUnusedStreams", "false"),
            ("linkDuplicateStreams", "false"),
            ("removeUnusedObjects", "false"),
            ("skipImages", "true")));
        PdfCompressionOptions off = Assert.IsType<PdfCompressionOptions>(allOff.LastOptions);
        Same("removeUnusedStreams=false", false, off.RemoveUnusedStreams);
        Same("removeUnusedObjects=false", false, off.RemoveUnusedObjects);
        Same("skipImages=true→CompressImages", false, off.CompressImages);
    }

    [Fact]
    public async Task Quality_MeansImageQualityForPdf_ButJpegQualityForImages()
    {
        // 同名不同义：Schema 里两个字面量都叫 quality，落到不同 DTO、不同属性，连合法区间都不同。
        var pdfLowTier = new RecordingCompressionService();
        await CompressAsync(pdfLowTier, Keys(("quality", "低")));
        Same("pdf.compress/quality=低→ImageQuality", 40,
            Assert.IsType<PdfCompressionOptions>(pdfLowTier.LastOptions).ImageQuality);

        // pdf.compress 认 legacy 的英文标签，且夹到 1..100。
        var pdfEnglish = new RecordingCompressionService();
        await CompressAsync(pdfEnglish, Keys(("quality", "low")));
        Same("pdf.compress/quality=low→ImageQuality", 40,
            Assert.IsType<PdfCompressionOptions>(pdfEnglish.LastOptions).ImageQuality);

        var pdfBelowFloor = new RecordingCompressionService();
        await CompressAsync(pdfBelowFloor, Keys(("quality", "5")));
        Same("pdf.compress/quality=5→夹到 1", 5,
            Assert.IsType<PdfCompressionOptions>(pdfBelowFloor.LastOptions).ImageQuality);

        var imageLowTier = await RunImageCompressAsync(Keys(("quality", "低")));
        Same("image.compress/quality=低→JpegQuality", 40, imageLowTier.JpegQuality);

        // 两处差异：image.compress 的下限是 10（pdf 是 1），且不认英文标签（回退默认 80）。
        var imageBelowFloor = await RunImageCompressAsync(Keys(("quality", "5")));
        Same("image.compress/quality=5→夹到 10", 10, imageBelowFloor.JpegQuality);

        var imageEnglish = await RunImageCompressAsync(Keys(("quality", "low")));
        Same("image.compress/quality=low→回退 80", 80, imageEnglish.JpegQuality);

        var imageNumeric = await RunImageCompressAsync(Keys(("quality", "64")));
        Same("image.compress/quality=64", 64, imageNumeric.JpegQuality);
    }

    private async Task<ImageEncodeOptions> RunImageCompressAsync(Dictionary<string, string> options)
    {
        var recorder = new RecordingImageService();
        DocumentOperationResult result = await new ImageConvertOperation("image.compress", recorder)
            .ExecuteAsync(new DocumentOperationRequest
            {
                InputFiles = [Path.Combine(_root, "input.png")],
                OutputDirectory = _root,
                Options = options
            });

        Assert.True(result.Success, result.Message);
        return Assert.IsType<ImageEncodeOptions>(recorder.LastOptions);
    }

    // ───────────────────────── word.pdf 本机 Office 的书签导出（Lead 修的那处） ─────────────────────────

    [Fact]
    public void WordBookmarksExportMode_IsWordBookmarksNotHeadings()
    {
        // 调用点用它传 ExportAsFixedFormat(CreateBookmarks:)；2 = 按 Word 书签，1 = 按标题样式。
        // legacy Util.cs:2731 传的是 2，改回 1 会让目录结构随样式漂移，这里把值钉死。
        Same("WordBookmarksExportMode", 2, OfficeConvertService.WordBookmarksExportMode);
    }

    [LocalWordFact]
    public void WordToPdf_LocalOffice_ExportsOneBookmarkPerWordBookmark()
    {
        string docx = Path.Combine(_root, "bookmarks.docx");
        var document = new Aspose.Words.Document();
        var builder = new Aspose.Words.DocumentBuilder(document);
        builder.Writeln("封面");
        builder.StartBookmark("BM1");
        builder.Writeln("第一章正文");
        builder.EndBookmark("BM1");
        builder.StartBookmark("BM2");
        builder.Writeln("第二章正文");
        builder.EndBookmark("BM2");
        document.Save(docx);

        string pdfPath = Path.Combine(_root, "bookmarks.pdf");
        new OfficeConvertService().WordToPdf(docx, pdfPath, new WordToPdfOptions { UseLocalOffice = true });

        using var pdf = new Aspose.Pdf.Document(pdfPath);
        int bookmarks = pdf.Outlines.Count;
        Assert.True(
            bookmarks == 2,
            $"本机 Word 应按 2 个 Word 书签生成 PDF 书签（CreateBookmarks=2），实际 {bookmarks} 个。");
    }

    /// <summary>本机 Office 用例的开关：显式 opt-in 且本机注册了 Word.Application 才跑。</summary>
    private sealed class LocalWordFactAttribute : FactAttribute
    {
        public LocalWordFactAttribute()
        {
            bool optedIn = string.Equals(
                Environment.GetEnvironmentVariable("DOCUMENTTOOLS_TEST_LOCAL_OFFICE"),
                "1",
                StringComparison.Ordinal);

            if (!optedIn)
            {
                Skip = "需要显式开启：设置 DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1（会拉起本机 WINWORD.EXE）。";
            }
            else if (Type.GetTypeFromProgID(OfficeConvertService.WordApplicationProgId) is null)
            {
                Skip = "本机未注册 Word.Application，无法验证本机 Office 转换。";
            }
        }
    }

    // ───────────────────────── html.pdf（另一个 Operation，同一个词根陷阱） ─────────────────────────

    [Fact]
    public async Task HtmlToPdf_Keys_MapToHtmlToPdfOptions()
    {
        string html = Path.Combine(_root, "input.html");
        File.WriteAllText(html, "<html><body>hi</body></html>");

        var recorder = new RecordingHtmlToPdfService();
        DocumentOperationResult result = await new HtmlToPdfOperation(recorder).ExecuteAsync(
            new DocumentOperationRequest
            {
                InputFiles = [html],
                OutputDirectory = _root,
                Options = Keys(
                    ("chromePath", @"C:\Chrome\chrome.exe"),
                    ("paper", "10"),
                    ("landscape", "true"),
                    ("useCss", "true"),
                    ("waitResources", "true"),
                    ("marginTop", "12"),
                    ("marginLeft", "13"),
                    ("marginBottom", "14"),
                    ("marginRight", "15"),
                    ("timeout", "45"),
                    ("navTimeout", "60"))
            });

        Assert.True(result.Success, result.Message);
        HtmlToPdfOptions options = recorder.LastOptions
            ?? throw new Xunit.Sdk.XunitException("html.pdf 没有调用 ConvertAsync。");

        Same("chromePath", @"C:\Chrome\chrome.exe", options.BrowserExecutablePath);
        Same("paper=10（Tabloid）", 10, options.PaperSizeIndex);
        Same("landscape", true, options.Landscape);
        Same("useCss", true, options.PreferCssPageSize);
        Same("waitResources", true, options.WaitAllResources);
        Same("marginTop", "12", options.MarginTopPx);
        Same("marginLeft", "13", options.MarginLeftPx);
        Same("marginBottom", "14", options.MarginBottomPx);
        Same("marginRight", "15", options.MarginRightPx);
        Same("timeout", 45, options.DefaultTimeoutSeconds);
        Same("navTimeout", 60, options.NavigationTimeoutSeconds);
    }

    // ───────────────────────── dev 对齐回归（dev 下拉给的是枚举名/扩展名） ─────────────────────────

    [Theory]
    [InlineData("PDF_A_1A", "PDF_A_1A")]
    [InlineData("PDF_A_4F", "PDF_A_4F")]
    [InlineData("v_1_3", "v_1_3")]
    [InlineData("PDF_X_1A_2001", "PDF_X_1A_2001")]
    [InlineData("ZUGFeRD", "ZUGFeRD")]
    [InlineData("PDF/A-1b", "PDF_A_1B")]
    public async Task PdfCompliance_AcceptsEveryDevVersionName(string version, string expected)
    {
        // dev 的下拉是 Enum.GetNames(typeof(Aspose.Pdf.PdfFormat))（BodyForm.cs:1223），
        // 所以 27 个枚举名都会原样传下来；新版 UI 只列 5 项，但 operation 必须全部认。
        var recorder = new RecordingConvertService();
        DocumentOperationResult result = await ConvertAsync("pdf.compliance", recorder, Keys(("version", version)));
        Assert.True(result.Success, result.Message);
        Same($"version={version}", expected, recorder.ComplianceFormat);
    }

    [Fact]
    public async Task WebpImage_PassesFormatLabelThrough()
    {
        // dev: webp2image_combox 的 DisplayValue/RealValue = JPG/.jpg、JPEG/.jpeg、PNG/.png…（BodyForm.cs:1423-1432）
        var recorder = new RecordingImageService();
        DocumentOperationResult result = await new ImageConvertOperation("webp.image", recorder).ExecuteAsync(
            new DocumentOperationRequest
            {
                InputFiles = [Path.Combine(_root, "input.webp")],
                OutputDirectory = _root,
                Options = Keys(("format", "JPEG"))
            });

        Assert.True(result.Success, result.Message);
        Same("format=JPEG", "JPEG", recorder.LastWebpFormat);
    }

    // ───────────────────────── 录制的辅助方法 ─────────────────────────

    private static Dictionary<string, string> Keys(params (string Key, string Value)[] pairs)
        => pairs.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

    private static void Same(string key, object? expected, object? actual)
        => Assert.True(
            object.Equals(expected, actual),
            $"键 {key}：期望 {expected ?? "<null>"}，实际 {actual ?? "<null>"}");

    private DocumentOperationRequest Request(string featureId, Dictionary<string, string> options)
        => new()
        {
            InputFiles = [Path.Combine(_root, "input.docx")],
            OutputDirectory = _root,
            Options = options
        };

    private Task<DocumentOperationResult> ConvertAsync(
        string featureId,
        IOfficeConvertService service,
        Dictionary<string, string> options)
        => new OfficeConvertOperation(featureId, service).ExecuteAsync(Request(featureId, options));

    private Task<DocumentOperationResult> ReplaceAsync(
        string featureId,
        IDocumentReplaceService service,
        Dictionary<string, string> options)
        => new ReplaceDocumentOperation(featureId, service).ExecuteAsync(Request(featureId, options));

    private Task<DocumentOperationResult> CompressAsync(
        IPdfCompressionService service,
        Dictionary<string, string> options)
        => new PdfCompressOperation(service).ExecuteAsync(new DocumentOperationRequest
        {
            InputFiles = [Path.Combine(_root, "input.pdf")],
            OutputDirectory = _root,
            Options = options
        });

    private sealed class RecordingConvertService : IOfficeConvertService
    {
        public int Calls { get; private set; }

        public string? LastMethod { get; private set; }

        public WordToPdfOptions? LastWordToPdf { get; private set; }

        public PdfToWordOptions? LastPdfToWord { get; private set; }

        public ExcelToPdfOptions? LastExcelToPdf { get; private set; }

        public PptToPdfOptions? LastPptToPdf { get; private set; }

        public PdfToPptOptions? LastPdfToPpt { get; private set; }

        public WordToEpubOptions? LastWordToEpub { get; private set; }

        public ImagePdfOptions? ImagePdf { get; private set; }

        public bool? PdfToExcelOneSheet { get; private set; }

        public bool? WordToExcelExportSingleSheet { get; private set; }

        public string? ComplianceFormat { get; private set; }

        public (string Extension, int Dpi, bool Gray, bool PageExport)? WordImage { get; private set; }

        public void WordToPdf(string sourceFile, string savedFile, WordToPdfOptions? options = null)
            => Record("WordToPdf", () => LastWordToPdf = options ?? new WordToPdfOptions());

        public void PdfToWord(string sourceFile, string savedFile, PdfToWordOptions? options = null)
            => Record("PdfToWord", () => LastPdfToWord = options ?? new PdfToWordOptions());

        public void ExcelToPdf(string sourceFile, string savedFile, ExcelToPdfOptions? options = null)
            => Record("ExcelToPdf", () => LastExcelToPdf = options ?? new ExcelToPdfOptions());

        public IReadOnlyList<string> ExcelToPdfWithOutputs(
            string sourceFile,
            string savedFile,
            ExcelToPdfOptions? options = null)
        {
            Record("ExcelToPdfWithOutputs", () => LastExcelToPdf = options ?? new ExcelToPdfOptions());
            return [savedFile];
        }

        public void PptToPdf(string sourceFile, string savedFile, PptToPdfOptions? options = null)
            => Record("PptToPdf", () => LastPptToPdf = options ?? new PptToPdfOptions());

        public void ImagesToPdf(IEnumerable<string> imageFiles, string savedFile)
            => Record("ImagesToPdf(2 参重载被调用：ImagePdfOptions 会丢失)", () => { });

        public void ImagesToPdf(IEnumerable<string> imageFiles, string savedFile, ImagePdfOptions? options)
            => Record("ImagesToPdf", () => ImagePdf = options ?? new ImagePdfOptions());

        public void PdfToExcel(string sourceFile, string savedFile, bool oneSheetPerDocument = true)
            => Record("PdfToExcel", () => PdfToExcelOneSheet = oneSheetPerDocument);

        public void PdfToPpt(string sourceFile, string savedFile, PdfToPptOptions? options = null)
            => Record("PdfToPpt", () => LastPdfToPpt = options ?? new PdfToPptOptions());

        public void WordToExcel(string sourceFile, string savedFile, bool exportSingleSheet = true)
            => Record("WordToExcel", () => WordToExcelExportSingleSheet = exportSingleSheet);

        public void ExcelToWord(string sourceFile, string savedFile)
            => Record("ExcelToWord", () => { });

        public void WordToEpub(string sourceFile, string savedFile, WordToEpubOptions? options = null)
            => Record("WordToEpub", () => LastWordToEpub = options ?? new WordToEpubOptions());

        public void EpubToWord(string sourceFile, string savedFile)
            => Record("EpubToWord", () => { });

        public void EpubToPdf(string sourceFile, string savedFile)
            => Record("EpubToPdf", () => { });

        public IReadOnlyList<string> DocumentToImages(
            string sourceFile,
            string outputDirectory,
            string imageExtension = ".png",
            int dpi = 150,
            bool grayscale = false)
            => throw new NotSupportedException("word.image 必须走 6 参重载，否则 pageExport 会丢失。");

        public IReadOnlyList<string> DocumentToImages(
            string sourceFile,
            string outputDirectory,
            string imageExtension,
            int dpi,
            bool grayscale,
            bool pageExport)
        {
            Record("DocumentToImages", () => WordImage = (imageExtension, dpi, grayscale, pageExport));
            return [Path.Combine(outputDirectory, "page-1" + imageExtension)];
        }

        public void PdfToCompliance(string sourceFile, string savedFile, string pdfFormat = "v_1_7")
            => Record("PdfToCompliance", () => ComplianceFormat = pdfFormat);

        private void Record(string method, Action assign)
        {
            Calls++;
            LastMethod = method;
            assign();
        }
    }

    private sealed class RecordingSplitService : IDocumentMergeSplitService
    {
        public WordSplitOptions? Words { get; private set; }

        public SlideSplitOptions? Slides { get; private set; }

        public string MergePdfFiles(IList<string> pdfFiles, string outputFile) => outputFile;

        public string MergeWordFiles(IList<string> wordFiles, string outputFile) => outputFile;

        public IReadOnlyList<string> SplitWord(string sourceFile, string outputDirectory, WordSplitOptions options)
        {
            Words = options;
            return ["word_part.docx"];
        }

        public IReadOnlyList<string> SplitPowerPoint(
            string sourceFile,
            string outputDirectory,
            SlideSplitOptions options)
        {
            Slides = options;
            return ["slide_part.pptx"];
        }
    }

    private sealed class RecordingPdfSplitService : IPdfSplitService
    {
        public List<PdfSplitOptions> Calls { get; } = [];

        public IReadOnlyList<string> Split(
            string sourceFile,
            string outputDirectory,
            PdfSplitOptions options)
        {
            Calls.Add(options);
            return ["pdf_part.pdf"];
        }
    }

    private sealed class RecordingReplaceService : IDocumentReplaceService
    {
        public DocumentReplaceOptions? LastOptions { get; private set; }

        public void ReplaceWord(string sourceFile, string savedFile, DocumentReplaceOptions options)
            => LastOptions = options;

        public void ReplaceExcel(string sourceFile, string savedFile, DocumentReplaceOptions options)
            => LastOptions = options;
    }

    private sealed class RecordingCompressionService : IPdfCompressionService
    {
        public PdfCompressionOptions? LastOptions { get; private set; }

        public PdfCompressionResult Compress(
            string sourceFile,
            string outputFile,
            PdfCompressionOptions? options = null)
        {
            LastOptions = options ?? new PdfCompressionOptions();
            return new PdfCompressionResult { OutputFile = outputFile };
        }
    }

    private sealed class RecordingImageService : IImageConversionService
    {
        public ImageEncodeOptions? LastOptions { get; private set; }

        public string Convert(
            string sourceFile,
            string outputDirectory,
            string format,
            int jpegQuality = 75,
            int? maxEdgePx = null)
            => throw new NotSupportedException("image.compress 必须走 ImageEncodeOptions 重载。");

        public string Convert(string sourceFile, string outputDirectory, ImageEncodeOptions options)
        {
            LastOptions = options;
            return Path.Combine(outputDirectory, "out.png");
        }

        public string WebpToImage(string sourceFile, string outputDirectory, string format = "png")
        {
            LastWebpFormat = format;
            return Path.Combine(outputDirectory, "out." + format.ToLowerInvariant());
        }

        public string? LastWebpFormat { get; private set; }

        public string ToIco(string sourceFile, string outputDirectory, int size = 32)
            => throw new NotSupportedException();

        public string ToIco(string sourceFile, string outputDirectory, IReadOnlyList<int> sizes)
            => throw new NotSupportedException();
    }

    private sealed class RecordingHtmlToPdfService : IHtmlToPdfService
    {
        public HtmlToPdfOptions? LastOptions { get; private set; }

        public Task ConvertAsync(
            string source,
            string savedFileName,
            HtmlToPdfOptions options,
            CancellationToken cancellationToken = default)
        {
            LastOptions = options;
            return Task.CompletedTask;
        }
    }
}
