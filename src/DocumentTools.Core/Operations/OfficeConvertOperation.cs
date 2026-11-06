using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>Maps a convert feature id to <see cref="IOfficeConvertService"/>.</summary>
public sealed class OfficeConvertOperation : IDocumentOperation
{
    private readonly IOfficeConvertService _convert;

    public OfficeConvertOperation(string featureId, IOfficeConvertService convert)
    {
        FeatureId = featureId;
        _convert = convert;
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

        Directory.CreateDirectory(request.OutputDirectory);
        var outputs = new List<string>();

        try
        {
            // image.pdf: unless onePdfPerImage, merge all selected images into one PDF (Strategy batch).
            if (FeatureId == "image.pdf" && !IsTrue(request.Options, "onePdfPerImage"))
            {
                string merged = Path.Combine(request.OutputDirectory, "images.pdf");
                _convert.ImagesToPdf(request.InputFiles, merged, BuildImagePdfOptions(request.Options));
                return Task.FromResult(
                    DocumentOperationResult.Ok($"已合并 {request.InputFiles.Count} 张图片为 PDF。", [merged]));
            }

            foreach (string input in request.InputFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (FeatureId == "word.image")
                {
                    string extension = GetOption(request, "extension", ".png");
                    int dpi = int.TryParse(GetOption(request, "dpi", "150"), out int parsedDpi)
                        ? parsedDpi
                        : 150;
                    bool gray = IsTrue(request.Options, "gray");
                    bool pageExport = ParseBoolOption(request.Options, defaultValue: true, "pageExport");

                    outputs.AddRange(
                        _convert.DocumentToImages(
                            input,
                            request.OutputDirectory,
                            extension,
                            dpi,
                            gray,
                            pageExport));
                    continue;
                }

                string output = BuildOutputPath(input, request.OutputDirectory);

                // excel.pdf 的 eachSheet 会产生多个 PDF（{name}_{sheet}.pdf），
                // 只登记真实写盘的文件，避免回报不存在的路径。
                if (FeatureId == "excel.pdf")
                {
                    outputs.AddRange(
                        _convert.ExcelToPdfWithOutputs(input, output, BuildExcelToPdfOptions(request.Options)));
                    continue;
                }

                ConvertOne(input, output, request.Options);
                outputs.Add(output);
            }

            return Task.FromResult(
                DocumentOperationResult.Ok($"完成 {outputs.Count} 个文件。", outputs));
        }
        catch (Exception ex)
        {
            return Task.FromResult(DocumentOperationResult.Fail(ex.Message));
        }
    }

    private void ConvertOne(string input, string output, IDictionary<string, string>? options)
    {
        switch (FeatureId)
        {
            case "word.pdf":
                _convert.WordToPdf(input, output, new WordToPdfOptions
                {
                    EmbedFonts = IsTrue(options, "embedFonts"),
                    UpdateFields = IsTrue(options, "updateFields"),
                    UseLocalOffice = IsTrue(options, "useLocalOffice"),
                    Compliance = GetOption(options, "compliance", "Pdf17"),
                    ColorMode = GetOption(options, "colorMode", "Normal"),
                    MsWordVersion = GetOption(options, "msWordVersion", "Word2016"),
                    Password = NullIfEmpty(GetOption(options, "password", string.Empty))
                });
                break;
            case "pdf.word":
                _convert.PdfToWord(input, output, new PdfToWordOptions
                {
                    UseBullet = IsTrue(options, "useBullet"),
                    UseOcr = IsTrue(options, "useOcr"),
                    Mode = GetOption(options, "mode", "Flow"),
                    Password = NullIfEmpty(GetOption(options, "password", string.Empty))
                });
                break;
            case "excel.pdf":
                _convert.ExcelToPdf(input, output, BuildExcelToPdfOptions(options));
                break;
            case "ppt.pdf":
                _convert.PptToPdf(input, output, new PptToPdfOptions
                {
                    DrawBlackFrame = IsTrue(options, "drawBlackLine"),
                    ExportHiddenSlides = IsTrue(options, "hiddenSlides"),
                    // P2-8：exportNotes 由勾选升级为位置 Choice（None/BottomFull/BottomTruncated/RightFull）。
                    NotesPosition = MapNotesPosition(options),
                    Password = NullIfEmpty(GetOption(options, "password", string.Empty))
                });
                break;
            case "image.pdf":
                _convert.ImagesToPdf([input], output, BuildImagePdfOptions(options));
                break;
            case "pdf.excel":
                // UI key: oneSheet; also accept legacy oneSheetPerDocument (default true).
                bool oneSheet = ParseBoolOption(
                    options,
                    defaultValue: true,
                    "oneSheet",
                    "oneSheetPerDocument");
                _convert.PdfToExcel(input, output, oneSheet);
                break;
            case "pdf.ppt":
                _convert.PdfToPpt(input, output, new PdfToPptOptions
                {
                    ExtraOcr = IsTrue(options, "extraOcr"),
                    SplitShapes = IsTrue(options, "splitShape"),
                    RenderAsImages = IsTrue(options, "renderImage")
                });
                break;
            case "word.excel":
                bool singleSheet = !string.Equals(
                    GetOption(options, "exportSingleSheet", "true"),
                    "false",
                    StringComparison.OrdinalIgnoreCase);
                _convert.WordToExcel(input, output, singleSheet);
                break;
            case "excel.word":
                _convert.ExcelToWord(input, output);
                break;
            case "word.epub":
                _convert.WordToEpub(input, output, new WordToEpubOptions
                {
                    Author = GetOption(options, "author", "DocumentTools"),
                    CoverImagePath = NullIfEmpty(GetOption(options, "coverImage", string.Empty)),
                    CoverDpi = ParseInt(GetOption(options, "coverDpi", "100"), 100)
                });
                break;
            case "epub.word":
                _convert.EpubToWord(input, output);
                break;
            case "epub.pdf":
                _convert.EpubToPdf(input, output);
                break;
            case "pdf.compliance":
                // 键缺失时的回退与 dev 下拉第 0 项一致（dev: BodyForm.cs:1223 用 GetNames，默认 PDF_A_1A）。
                string version = GetOption(options, "version", GetOption(options, "format", "PDF_A_1A"));
                _convert.PdfToCompliance(input, output, MapPdfVersion(version));
                break;
            default:
                throw new NotSupportedException($"未支持的转换功能：{FeatureId}");
        }
    }

    private string BuildOutputPath(string input, string outputDir)
    {
        string name = Path.GetFileNameWithoutExtension(input);
        string ext = FeatureId switch
        {
            "pdf.word" or "excel.word" or "epub.word" => ".docx",
            "pdf.excel" or "word.excel" => ".xlsx",
            "pdf.ppt" => ".pptx",
            "word.epub" => ".epub",
            _ => ".pdf"
        };

        return Path.Combine(outputDir, name + ext);
    }

    private static string GetOption(DocumentOperationRequest request, string key, string fallback)
        => GetOption(request.Options, key, fallback);

    private static string GetOption(IDictionary<string, string>? options, string key, string fallback)
    {
        if (options is null)
        {
            return fallback;
        }

        return options.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : fallback;
    }

    private static bool IsTrue(IDictionary<string, string>? options, string key)
        => string.Equals(GetOption(options, key, "false"), "true", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// P2-8 备注导出位置：新键值是 <c>NotesPositions</c> 名称；同时兼容升级前的布尔值
    /// （true 视为 BottomFull，false/空视为 None），避免旧配置静默丢失。
    /// </summary>
    private static string MapNotesPosition(IDictionary<string, string>? options)
    {
        string raw = GetOption(options, "exportNotes", "None").Trim();
        if (string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase))
        {
            return "BottomFull";
        }

        if (string.Equals(raw, "false", StringComparison.OrdinalIgnoreCase))
        {
            return "None";
        }

        return raw switch
        {
            "BottomFull" => "BottomFull",
            "BottomTruncated" => "BottomTruncated",
            "RightFull" => "RightFull",
            _ => "None"
        };
    }

    private static bool ParseBoolOption(
        IDictionary<string, string>? options,
        bool defaultValue,
        params string[] keys)
    {
        if (options is null)
        {
            return defaultValue;
        }

        foreach (string key in keys)
        {
            if (!options.TryGetValue(key, out string? value) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            if (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(value, "false", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "0", StringComparison.OrdinalIgnoreCase)
                || string.Equals(value, "no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return defaultValue;
    }

    private static string? NullIfEmpty(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value;

    /// <summary>
    /// EXCEL→PDF 参数（legacy Util.ConvertExcel2Pdf，Util.cs:1571-1632）；
    /// 键缺省时按 legacy 默认：不分表导出、不强制一页、不额外缩放。
    /// </summary>
    private static ExcelToPdfOptions BuildExcelToPdfOptions(IDictionary<string, string>? options) => new()
    {
        ExportEachSheet = IsTrue(options, "eachSheet"),
        ForceContentOnOnePage = IsTrue(options, "fitOnePage"),
        HorizontalCenter = IsTrue(options, "horPrint"),
        VerticalCenter = IsTrue(options, "verPrint"),
        PrintScaleRow = ParseInt(GetOption(options, "printScaleRow", "0")),
        PrintScaleCol = ParseInt(GetOption(options, "printScaleCol", "0")),
        PrintScalePercentEnabled = IsTrue(options, "printScalePercentEnable"),
        PrintScalePercent = ParseInt(GetOption(options, "printScalePercent", "100"), 100)
    };

    /// <summary>
    /// IMAGE→PDF 参数（legacy Util.ConvertImage2PDF，Util.cs:1805-1899）；
    /// 键缺省时按 legacy 默认：压缩图片开、按图片像素定页、边距 0、不额外 Optimize。
    /// </summary>
    private static ImagePdfOptions BuildImagePdfOptions(IDictionary<string, string>? options) => new()
    {
        CompressImages = ParseBoolOption(options, defaultValue: true, "compress"),
        AutoOptimize = ParseBoolOption(options, defaultValue: false, "autoOptimize"),
        LandStyleIndex = ParseLandStyleOption(GetOption(options, "orientation", "0")),
        MarginTop = ParseDouble(GetOption(options, "marginTop", "0")),
        MarginLeft = ParseDouble(GetOption(options, "marginLeft", "0")),
        MarginBottom = ParseDouble(GetOption(options, "marginBottom", "0")),
        MarginRight = ParseDouble(GetOption(options, "marginRight", "0"))
    };

    /// <summary>legacy LandStyleIndex：0=按图片尺寸、1=横向、2=纵向（Util.cs:1840-1854）。</summary>
    private static int ParseLandStyleOption(string text) => text.Trim().ToLowerInvariant() switch
    {
        "1" or "横向" or "landscape" => 1,
        "2" or "纵向" or "portrait" => 2,
        _ => 0
    };

    private static double ParseDouble(string text, double fallback = 0)
        => double.TryParse(
            text,
            System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture,
            out double value)
            ? value
            : fallback;

    private static int ParseInt(string text, int fallback = 0)
        => int.TryParse(text, out int n) ? n : fallback;

    private static string MapPdfVersion(string version) => version switch
    {
        "1.4" => "v_1_4",
        "1.5" => "v_1_5",
        "1.6" => "v_1_6",
        "1.7" => "v_1_7",
        "PDF/A-1b" or "PDFA1b" => "PDF_A_1B",
        _ => version
    };
}
