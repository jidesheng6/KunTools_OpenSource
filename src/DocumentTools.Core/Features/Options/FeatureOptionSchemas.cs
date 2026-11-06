namespace DocumentTools.Core.Features.Options;

/// <summary>
/// Declarative option contracts for convert-list features.
/// Lives in Core so business keys are not invented inside WPF views.
/// </summary>
public static class FeatureOptionSchemas
{
    private static readonly OptionChoice[] PdfCompliance =
    [
        new("Pdf15", "Pdf15"),
        new("Pdf16", "Pdf16"),
        new("Pdf17", "Pdf17"),
        new("PdfA1a", "PdfA1a"),
        new("PdfA1b", "PdfA1b")
    ];

    private static readonly OptionChoice[] ColorModes =
    [
        new("Normal", "彩色/自动"),
        new("Grayscale", "灰度")
    ];

    private static readonly OptionChoice[] MsWordVersions =
    [
        new("Word2010", "Word2010"),
        new("Word2013", "Word2013"),
        new("Word2016", "Word2016"),
        new("Word2019", "Word2019")
    ];

    private static readonly OptionChoice[] SplitModes =
    [
        new("每页拆分", "每页拆分"),
        new("按页数", "按页数"),
        new("按页码范围", "按页码范围")
    ];

    /// <summary>条码输出尺寸模式：auto=跟随内容，fixed=按固定宽高。</summary>
    private static readonly OptionChoice[] BarcodeSizeModes =
    [
        new("auto", "自动"),
        new("fixed", "固定尺寸")
    ];

    /// <summary>条码旋转角度（度）。</summary>
    private static readonly OptionChoice[] BarcodeRotations =
    [
        new("0", "0°"),
        new("90", "90°"),
        new("180", "180°"),
        new("270", "270°")
    ];

    /// <summary>
    /// legacy PPT 备注导出位置（Aspose.Slides <c>NotesPositions</c>）。
    /// 不含 <c>RightFull</c>：Aspose.Slides 25.12 的枚举只有 None/BottomFull/BottomTruncated，
    /// 早先的「右侧占位」档会让导出静默退回 None（备注全丢），已按真实能力收窄。
    /// </summary>
    private static readonly OptionChoice[] PptNotePositions =
    [
        new("None", "不导出"),
        new("BottomFull", "页底完整"),
        new("BottomTruncated", "页底截断")
    ];

    /// <summary>legacy SplitPdfSizeCombox 直接绑定 Aspose.Slides.SlideSizeType 枚举（显示枚举名）。</summary>
    private static readonly OptionChoice[] SlideSizes =
    [
        new("OnScreen", "OnScreen"),
        new("LetterPaper", "LetterPaper"),
        new("A4Paper", "A4Paper"),
        new("Slide35mm", "Slide35mm"),
        new("Overhead", "Overhead"),
        new("Banner", "Banner"),
        new("Custom", "Custom"),
        new("Ledger", "Ledger"),
        new("A3Paper", "A3Paper"),
        new("B4IsoPaper", "B4IsoPaper"),
        new("B5IsoPaper", "B5IsoPaper"),
        new("B4JisPaper", "B4JisPaper"),
        new("B5JisPaper", "B5JisPaper"),
        new("HagakiCard", "HagakiCard"),
        new("OnScreen16x9", "OnScreen16x9"),
        new("OnScreen16x10", "OnScreen16x10"),
        new("Widescreen", "Widescreen")
    ];

    /// <summary>legacy SplitPdfScaleCombox：最大化 / 合适大小 / 不缩放。</summary>
    private static readonly OptionChoice[] SlideScaleModes =
    [
        new("Maximize", "最大化"),
        new("EnsureFit", "合适大小"),
        new("DoNotScale", "不缩放")
    ];

    /// <summary>legacy CompressPDF_ImageCompressVersion：标准 / 快速 / 混合。</summary>
    private static readonly OptionChoice[] ImageCompressionVersions =
    [
        new("Standard", "标准"),
        new("Fast", "快速"),
        new("Mixed", "混合")
    ];

    /// <summary>legacy PdfWaterMarkHorSetting 的 6 项（值 = Aspose.Pdf.HorizontalAlignment）。</summary>
    private static readonly OptionChoice[] WatermarkHorizontals =
    [
        new("None", "不对齐"),
        new("Left", "左对齐"),
        new("Center", "居中对齐"),
        new("Right", "右对齐"),
        new("Justify", "两端对齐(Justify)"),
        new("FullJustify", "分散对齐(FullJustify)")
    ];

    /// <summary>legacy PDFWaterMarkVerSetting 的 4 项（值 = Aspose.Pdf.VerticalAlignment）。</summary>
    private static readonly OptionChoice[] WatermarkVerticals =
    [
        new("None", "不对齐"),
        new("Top", "顶部对齐"),
        new("Center", "居中对齐"),
        new("Bottom", "底部对齐")
    ];

    /// <summary>legacy PdfPaperSizeIndex：0=A0 … 4=A4 … 10=Tabloid（值保留为索引串）。</summary>
    private static readonly OptionChoice[] HtmlPapers =
    [
        new("0", "A0"),
        new("1", "A1"),
        new("2", "A2"),
        new("3", "A3"),
        new("4", "A4"),
        new("5", "A5"),
        new("6", "A6"),
        new("7", "Ledger"),
        new("8", "Legal"),
        new("9", "Letter"),
        new("10", "Tabloid")
    ];

    /// <summary>legacy Image2PDFCombox 的 SelectedIndex：0=自动（按图片尺寸）、1=横排、2=竖排。</summary>
    private static readonly OptionChoice[] ImagePdfOrientations =
    [
        new("0", "自动（按图片尺寸）"),
        new("1", "横排"),
        new("2", "竖排")
    ];

    /// <summary>legacy CompressJpegColorMode_Combox 中 SkiaSharp 能落地的两项（Rgb 为默认）。</summary>
    private static readonly OptionChoice[] JpegColorModes =
    [
        new("Rgb", "Rgb"),
        new("Grayscale", "Grayscale")
    ];

    /// <summary>legacy CompressPngColorMode_Combox 中 SkiaSharp 能落地的项（Truecolor 为默认）。</summary>
    private static readonly OptionChoice[] PngColorTypes =
    [
        new("Truecolor", "Truecolor"),
        new("Grayscale", "Grayscale"),
        new("TruecolorWithAlpha", "TruecolorWithAlpha")
    ];

    /// <summary>legacy CompressPngLevel_Combox：0-9，默认 3。</summary>
    private static readonly OptionChoice[] PngLevels =
    [
        new("0", "0"),
        new("1", "1"),
        new("2", "2"),
        new("3", "3"),
        new("4", "4"),
        new("5", "5"),
        new("6", "6"),
        new("7", "7"),
        new("8", "8"),
        new("9", "9")
    ];

    /// <summary>legacy CompressBmpPixeBits_Combox：4/8/16/24/32，默认 24（SelectedIndex=3）。</summary>
    private static readonly OptionChoice[] BmpPixelBits =
    [
        new("4", "4 位"),
        new("8", "8 位"),
        new("16", "16 位"),
        new("24", "24 位"),
        new("32", "32 位")
    ];

    /// <summary>legacy GetChinsesHanPinYin_TypeBox 6 模式（BodyForm.cs:1555-1563），索引 0..5 一一对应。</summary>
    private static readonly OptionChoice[] PinyinModes =
    [
        new("InitialsUpper", "大写拼音首字母"),
        new("InitialsLower", "小写拼音首字母"),
        new("PinyinUpperSpaced", "大写拼音（带空格）"),
        new("PinyinUpperCompact", "大写拼音（不带空格）"),
        new("PinyinLowerSpaced", "小写拼音（带空格）"),
        new("PinyinLowerCompact", "小写拼音（不带空格）")
    ];

    /// <summary>替换文字加粗的三态：keep = 不动（legacy ApplyFont 未设 Bold，保持其行为）。</summary>
    private static readonly OptionChoice[] ReplaceBoldModes =
    [
        new("keep", "不改"),
        new("bold", "加粗"),
        new("normal", "不加粗")
    ];

    public static IReadOnlyList<OptionSection> GetSections(string featureId)
        => featureId switch
        {
            "word.pdf" =>
            [
                Section("输出与后续",
                    Check("saveSource", "保存到源目录"),
                    Check("openPdfOptions", "处理完毕后打开PDF文件")),
                Section("转换参数",
                    Check("embedFonts", "嵌入字体"),
                    Check("updateFields", "自动更新域"),
                    Check("useLocalOffice", "使用本机Office转换"),
                    Choice("compliance", "PDF标准", PdfCompliance, 2),
                    Choice("colorMode", "PDF颜色模式", ColorModes, 0),
                    Choice("msWordVersion", "设置兼容版本", MsWordVersions, 2),
                    Text("password", "WORD密码", width: 120))
            ],

            "pdf.word" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("转换参数",
                    Check("useOcr", "启用OCR子层"),
                    Check("useBullet", "识别项目符号"),
                    Choice("mode", "输出模式",
                    [
                        new("Flow", "Flow"),
                        new("Textbox", "Textbox"),
                        new("EnhancedFlow", "EnhancedFlow")
                    ], 0),
                    Text("password", "PDF密码", width: 120))
            ],

            "excel.pdf" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("转换参数",
                    Check("eachSheet", "每个工作表单独导出PDF"),
                    Check("fitOnePage", "强制内容适应一页"),
                    Check("horPrint", "横向打印居中"),
                    Check("verPrint", "纵向打印居中"),
                    Text("printScaleRow", "行缩放", "0", 60),
                    Text("printScaleCol", "列缩放", "0", 60),
                    Check("printScalePercentEnable", "启用百分比缩放"),
                    Text("printScalePercent", "缩放%", "100", 60))
            ],

            "pdf.excel" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("转换参数", Check("oneSheet", "全部页面合并到一个工作表", true))
            ],

            "ppt.pdf" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("转换参数",
                    Choice("exportNotes", "备注导出位置", PptNotePositions, 0),
                    Check("hiddenSlides", "导出隐藏幻灯片"),
                    Check("drawBlackLine", "绘制黑框"),
                    Text("password", "PPT密码", width: 120))
            ],

            "pdf.ppt" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("转换参数",
                    Check("renderImage", "按图片渲染"),
                    Check("splitShape", "拆分形状"),
                    Check("extraOcr", "额外OCR"))
            ],

            "word.excel" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("转换参数", Check("exportSingleSheet", "多页合并到一个工作表", true))
            ],

            "word.image" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("图像输出",
                    Choice("extension", "图片格式",
                    [
                        new(".png", ".png"),
                        new(".jpg", ".jpg"),
                        new(".bmp", ".bmp"),
                        new(".tif", ".tif")
                    ], 0),
                    Choice("dpi", "DPI",
                    [
                        new("96", "96"),
                        new("150", "150"),
                        new("200", "200"),
                        new("300", "300")
                    ], 1),
                    Check("gray", "灰度"),
                    Check("pageExport", "按页导出", true))
            ],

            "word.epub" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("EPUB 参数",
                    Text("author", "作者", width: 160),
                    FilePath("coverImage", "封面图片路径", ImageFilter, 220),
                    Text("coverDpi", "封面DPI", "100", 80))
            ],

            // refreshPageNumber 只有 word 分支在 SplitDocumentOperation 里真正生效（P1-7），
            // pdf/ppt 侧不再暴露这个假参数。
            "pdf.split" => SplitSections(
                includeBookmark: true,
                includeDeletePages: true,
                includeRefreshPageNumber: false),
            "word.split" => SplitSections(includeBookmark: false),
            "ppt.split" => SplitSections(
                includeBookmark: false,
                includeSlideSize: true,
                includeRefreshPageNumber: false),

            "pdf.watermark.add" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("水印内容",
                    Text("text", "水印文字", width: 180),
                    FilePath("image", "水印图片路径", ImageFilter, 220),
                    Text("opacity", "不透明度0-1", "0.5", 80),
                    Text("rotate", "旋转角度", "45", 60),
                    Text("fontName", "字体", "宋体", 80),
                    Text("fontSize", "字号", "36", 60),
                    Text("fontColor", "字体颜色(#RRGGBB)", "#FF0000", 100)),
                Section("对齐与位置",
                    Choice("horizontal", "水平对齐", WatermarkHorizontals, 2),
                    Choice("vertical", "垂直对齐", WatermarkVerticals, 2),
                    Text("imageWidth", "图片宽度", "100", 60),
                    Text("imageHeight", "图片高度", "100", 60),
                    Check("customPosition", "自定义位置"),
                    Text("positionX", "位置X", "0", 60),
                    Text("positionY", "位置Y", "0", 60)),
                Section("平铺设置",
                    Check("fullScreen", "平铺全屏"),
                    Text("repeatSpacingX", "水平间距", "25", 60),
                    Text("repeatSpacingY", "垂直间距", "25", 60),
                    Check("underContent", "水印在内容下层"),
                    Check("useFileName", "使用文件名作水印"))
            ],

            "pdf.compliance" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                // dev 用 Enum.GetNames(typeof(Aspose.Pdf.PdfFormat)) 铺满 27 项、默认第 0 项 PDF_A_1A
                // （BodyForm.cs:1223，顺序即枚举声明顺序）；每项都以枚举名直传，由 OfficeConvertService.ParsePdfFormat 解析。
                Section("版本转换",
                    Choice("version", "目标版本",
                    [
                        new("PDF_A_1A", "PDF_A_1A"),
                        new("PDF_A_1B", "PDF_A_1B"),
                        new("PDF_A_2A", "PDF_A_2A"),
                        new("PDF_A_3A", "PDF_A_3A"),
                        new("PDF_A_2B", "PDF_A_2B"),
                        new("PDF_A_2U", "PDF_A_2U"),
                        new("PDF_A_3B", "PDF_A_3B"),
                        new("PDF_A_3U", "PDF_A_3U"),
                        new("v_1_0", "v_1_0"),
                        new("v_1_1", "v_1_1"),
                        new("v_1_2", "v_1_2"),
                        new("v_1_3", "v_1_3"),
                        new("v_1_4", "v_1_4"),
                        new("v_1_5", "v_1_5"),
                        new("v_1_6", "v_1_6"),
                        new("v_1_7", "v_1_7"),
                        new("v_2_0", "v_2_0"),
                        new("PDF_UA_1", "PDF_UA_1"),
                        new("PDF_X_1A_2001", "PDF_X_1A_2001"),
                        new("PDF_X_1A", "PDF_X_1A"),
                        new("PDF_X_3", "PDF_X_3"),
                        new("ZUGFeRD", "ZUGFeRD"),
                        new("PDF_A_4", "PDF_A_4"),
                        new("PDF_A_4E", "PDF_A_4E"),
                        new("PDF_A_4F", "PDF_A_4F"),
                        new("PDF_X_4", "PDF_X_4"),
                        new("PDF_E_1", "PDF_E_1")
                    ], 0))
            ],

            "pdf.compress" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("参数设置",
                    Text("quality", "图片质量(1-100)", "80", 70),
                    Choice("imageVersion", "图片压缩版本", ImageCompressionVersions, 0),
                    Text("maxDpi", "最大DPI", "150", 80),
                    Check("webOptimize", "Web优化", true),
                    Check("unembedFonts", "取消嵌入字体"),
                    Check("flattenForms", "压平表单域"),
                    Check("flate", "图片 Flate 编码"),
                    Check("skipImages", "跳过图片压缩")),
                // 与 legacy 未勾默认不同：保持迁移前 DocTools 的实际压缩强度（见 docs/feature-gap.md）。
                Section("资源精简",
                    Check("removeUnusedStreams", "移除未使用的文件流", true),
                    Check("linkDuplicateStreams", "链接重复流", true),
                    Check("removeUnusedObjects", "移除未使用对象", true))
            ],

            "code.barcode" =>
            [
                Section("条码外观设置",
                    Choice("symbology", "类型",
                    [
                        new("QR", "QR"),
                        new("DataMatrix", "DataMatrix"),
                        new("Pdf417", "Pdf417"),
                        new("Aztec", "Aztec"),
                        new("Code128", "Code128"),
                        new("Code39", "Code39")
                    ], 0),
                    Choice("moduleSize", "模块尺寸",
                    [
                        new("2", "2"),
                        new("3", "3"),
                        new("4", "4"),
                        new("6", "6"),
                        new("8", "8")
                    ], 2),
                    Choice("dpi", "分辨率DPI",
                    [
                        new("96", "96"),
                        new("150", "150"),
                        new("200", "200"),
                        new("300", "300")
                    ], 0)),
                Section("尺寸与旋转",
                    Choice("sizeMode", "输出尺寸", BarcodeSizeModes, 0),
                    Text("barcodeWidth", "宽度px(0=自动)", "0", 90),
                    Text("barcodeHeight", "高度px(0=自动)", "0", 90),
                    Choice("rotate", "旋转角度", BarcodeRotations, 0)),
                Section("标题设置",
                    Text("caption", "条码标题", width: 200),
                    Choice("captionPosition", "标题位置",
                    [
                        new("none", "隐藏"),
                        new("above", "上方"),
                        new("below", "下方")
                    ], 0),
                    Text("captionFontSize", "标题字号", "12", 60))
            ],

            "html.pdf" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("设置 chromium 路径",
                    FilePath("chromePath", "Chrome路径", "可执行文件|*.exe|所有文件|*.*", 280)),
                Section("页面边距设置",
                    Text("marginTop", "上边距px", "50", 60),
                    Text("marginBottom", "下边距px", "50", 60),
                    Text("marginLeft", "左边距px", "50", 60),
                    Text("marginRight", "右边距px", "50", 60)),
                Section("生成PDF参数设置",
                    Choice("paper", "纸张", HtmlPapers, 4),
                    Check("landscape", "横向"),
                    // dev 的“使用内部CSS样式设置页面”“等待所有页面元素加载完成”两个勾选框默认未勾
                    // （BodyForm.Designer.cs:6697/6727 无 Checked=true），这里跟随 dev。
                    Check("useCss", "使用CSS"),
                    Check("waitResources", "等待全部资源"),
                    Text("timeout", "超时秒", "30", 60),
                    Text("navTimeout", "导航超时秒", "30", 60))
            ],

            "image.pdf" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("参数设置",
                    Check("onePdfPerImage", "每张图单独PDF"),
                    Check("compress", "压缩图片", true),
                    Check("autoOptimize", "自动优化文档"),
                    Choice("orientation", "页面方向", ImagePdfOrientations, 0)),
                Section("页边距",
                    Text("marginTop", "上边距", "0", 60),
                    Text("marginLeft", "左边距", "0", 60),
                    Text("marginBottom", "下边距", "0", 60),
                    Text("marginRight", "右边距", "0", 60))
            ],

            "image.compress" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("保存参数设置",
                    Choice("format", "输出格式",
                    [
                        new("PNG", "PNG"),
                        new("JPG", "JPG"),
                        new("JPEG", "JPEG"),
                        new("BMP", "BMP"),
                        // legacy CompressImage_OutPutImageFormat 有 TIF 一项（Util.cs 里走 TiffOptions）；
                        // GIF 是 legacy 缺、本实现补齐的项（Util.cs 的 webp 分支有 GifOptions 可作参照）。
                        new("TIF", "TIF"),
                        new("TIFF", "TIFF"),
                        new("GIF", "GIF")
                    ], 0),
                    // legacy 默认沿用源后缀（CompressImage_NotUseSelfExt 未勾），但那时格式下拉是被禁用的；
                    // 本面板没有跨字段禁用能力，默认 false 让「输出格式」如实生效，勾选即恢复 legacy 行为。
                    Check("useSourceExtension", "沿用源文件后缀名"),
                    Check("noCompress", "不执行压缩操作")),
                Section("JPG/JPEG参数设置",
                    Text("quality", "图像质量(1-100)", "80", 70),
                    Choice("jpegColorMode", "颜色模式", JpegColorModes, 0)),
                Section("PNG参数设置",
                    Choice("pngLevel", "压缩级别", PngLevels, 3),
                    Choice("pngColorType", "颜色模式", PngColorTypes, 0)),
                Section("BMP参数设置",
                    Choice("bmpPixelBits", "色彩位数", BmpPixelBits, 3))
            ],

            "webp.image" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("WEBP 转换",
                    Choice("format", "输出格式",
                    [
                        // 与 dev 的 webp2image_combox 同序同项（BodyForm.cs:1423-1432，默认第 0 行 JPG）：
                        // JPG→.jpg、JPEG→.jpeg、PNG/BMP/GIF 各自后缀；GIF 由自写 GIF89a 编码器产出。
                        new("JPG", "JPG"),
                        new("JPEG", "JPEG"),
                        new("PNG", "PNG"),
                        new("BMP", "BMP"),
                        new("GIF", "GIF")
                    ], 0))
            ],

            "image.ico" =>
            [
                Section("输出", Check("saveSource", "保存到源目录")),
                Section("ICO图标转换",
                    Choice("format", "ICO 尺寸",
                    [
                        new("ICO 16", "ICO 16"),
                        new("ICO 32", "ICO 32"),
                        new("ICO 64", "ICO 64"),
                        new("ICO 128", "ICO 128"),
                        new("ICO 256", "ICO 256")
                    ], 1),
                    Check("multiSize", "同时输出多尺寸(16/32/64/128/256)"))
            ],

            "name.vertical" =>
            [
                Section("文本清洗",
                    Check("removeNumber", "移除数字"),
                    Check("removeSquare", "移除括号"),
                    Check("distinct", "文本去重", true)),
                Section("文字处理",
                    Check("doubleAlign", "双字对齐", true),
                    Check("replaceEnabled", "替换文字"),
                    Text("replaceSearch", "查找", width: 90),
                    Text("replaceTarget", "替换为", width: 90),
                    Check("replaceRegex", "正则模式")),
                Section("拼音与重名",
                    Check("pinyinEnabled", "汉字转拼音模式"),
                    Choice("pinyinMode", "拼音模式", PinyinModes, 0),
                    Check("markDuplicate", "高亮重名"))
            ],

            "pdf.merge" or "word.merge" => [],

            "pdf.watermark.remove" or "excel.word" or "epub.word" or "epub.pdf" =>
            [
                Section("输出", Check("saveSource", "保存到源目录"))
            ],

            "word.replace" =>
            [
                Section("匹配选项",
                    Check("regex", "使用正则"),
                    Check("matchCase", "区分大小写"),
                    Check("wholeWord", "全字匹配"),
                    Check("ignoreFootnotes", "忽略脚注")),
                Section("WORD 替换选项",
                    Check("ignoreShapes", "忽略文本中的形状"),
                    Check("ignoreFields", "忽略域"),
                    Check("ignoreFieldCodes", "忽略域代码"),
                    Check("ignoreDeleted", "忽略删除修订"),
                    Check("ignoreInserted", "忽略插入修订"),
                    Check("regexSub", "使用文本捕获替换")),
                // legacy 勾选框文案「更改替换的字体和大小」（BodyForm.Designer.cs:7732），
                // 默认值取 legacy 形参默认 SimSun / 12（Util.cs:5371-5372）。
                Section("替换后字体样式",
                    Check("replaceUseNewStyle", "更改替换的字体和大小"),
                    Text("replaceFontName", "字体名(空=不改)", "SimSun", 120),
                    Text("replaceFontSize", "字号(0=不改)", "12", 70),
                    Choice("replaceBold", "加粗", ReplaceBoldModes, 0))
            ],

            "excel.replace" =>
            [
                Section("匹配选项",
                    Check("regex", "使用正则"),
                    Check("matchCase", "区分大小写"),
                    Check("wholeWord", "全字匹配")),
                Section("EXCEL 清空行数据",
                    Text("excelStartRow", "清空起始行", width: 100),
                    Text("excelEndRow", "清空结束行", width: 100),
                    Check("excelRemoveRows", "删除行（否则清空）"))
            ],

            // 选举结果报告单（legacy Exam.VoteReportDocxService）：字号/行高/生成日期三项排版参数，
            // 默认值取 legacy 控件（BodyForm.Designer.cs:13657 字号 14→本页沿用 12、:13702 行高 1.20cm→0.8 与旧 Core 默认一致）。
            "exam.vote" =>
            [
                Section("报告单排版",
                    Text("voteFontSize", "表格字体大小", "12", 70),
                    Text("voteRowHeight", "表格数据行高(cm)", "0.8", 70),
                    Text("voteGenDate", "生成日期(留空=当天)", width: 150))
            ],

            _ =>
            [
                Section("输出", Check("saveSource", "保存到源目录"))
            ]
        };

    /// <summary>OpenFileDialog filter for convert features (business concern, not WPF).</summary>
    public static string GetFileFilter(string featureId)
        => featureId switch
        {
            "word.pdf" or "word.merge" or "word.split" or "word.excel" or "word.epub" or "word.image"
                => "Word 文档|*.doc;*.docx|所有文件|*.*",
            "pdf.word" or "pdf.excel" or "pdf.ppt" or "pdf.merge" or "pdf.split"
                or "pdf.watermark.add" or "pdf.watermark.remove" or "pdf.compress" or "pdf.compliance"
                => "PDF 文件|*.pdf|所有文件|*.*",
            "excel.pdf" or "excel.word" => "Excel 文档|*.xls;*.xlsx|所有文件|*.*",
            "ppt.pdf" or "ppt.split" => "PPT 文档|*.ppt;*.pptx|所有文件|*.*",
            "image.pdf" => "图片|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|所有文件|*.*",
            "html.pdf" => "HTML|*.html;*.htm|所有文件|*.*",
            "epub.word" or "epub.pdf" => "EPUB|*.epub|所有文件|*.*",
            "image.compress" => "图片|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.gif|所有文件|*.*",
            "webp.image" => "WEBP|*.webp|所有文件|*.*",
            "image.ico" => "图片|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*",
            "word.replace" => "Word 文档|*.doc;*.docx|所有文件|*.*",
            "excel.replace" => "Excel 文档|*.xls;*.xlsx|所有文件|*.*",
            _ => "所有文件|*.*"
        };

    private static OptionSection[] SplitSections(
        bool includeBookmark,
        bool includeDeletePages = false,
        bool includeSlideSize = false,
        bool includeRefreshPageNumber = true)
    {
        var fields = new List<OptionField>
        {
            Check("saveSource", "保存到源目录"),
        };
        var splitFields = new List<OptionField>
        {
            Choice("splitMode", "拆分方式", SplitModes, 1),
            Text("pagesPerFile", "每份页数", "1", 60),
            Text("range", "页码范围(如1-3,5)", width: 140)
        };

        if (includeRefreshPageNumber)
        {
            splitFields.Add(Check("refreshPageNumber", "拆分后更新页码"));
        }
        if (includeBookmark)
        {
            splitFields.Add(Check("splitByBookmark", "按书签拆分"));
        }

        if (includeDeletePages)
        {
            // legacy SplitPdf(DeletePdfPages_Start/End)：只填一个端点即不删除，语义与操作层一致。
            splitFields.Add(Text("deleteStart", "删除起始页", width: 70));
            splitFields.Add(Text("deleteEnd", "删除结束页", width: 70));
        }

        var sections = new List<OptionSection>
        {
            Section("输出", fields.ToArray()),
            Section("拆分设置", splitFields.ToArray())
        };

        if (includeSlideSize)
        {
            sections.Add(
                Section(
                    "幻灯片尺寸设置",
                    Choice("slideSize", "尺寸", SlideSizes, 0),
                    Choice("slideScale", "缩放方式", SlideScaleModes, 0)));
        }

        return sections.ToArray();
    }

    private static OptionSection Section(string title, params OptionField[] fields)
        => new(title, fields);

    private static OptionField Check(string key, string label, bool defaultOn = false)
        => new(key, label, OptionFieldKind.Check, defaultOn ? "true" : "false");

    private static OptionField Text(string key, string label, string? initial = null, double width = 160)
        => new(key, label, OptionFieldKind.Text, initial ?? string.Empty, TextWidth: width);

    /// <summary>图片文件路径字段（视图会带「浏览…」按钮，Filter 见 <see cref="ImageFilter"/>）。</summary>
    private static OptionField FilePath(string key, string label, string filter, double width = 220)
        => new(key, label, OptionFieldKind.FilePath, string.Empty, TextWidth: width, FileFilter: filter);

    /// <summary>图片类可选附件的通用对话框过滤串。</summary>
    public const string ImageFilter = "图片|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.gif|所有文件|*.*";

    private static OptionField Choice(string key, string label, IReadOnlyList<OptionChoice> choices, int selectedIndex)
    {
        int index = Math.Clamp(selectedIndex, 0, Math.Max(0, choices.Count - 1));
        string def = choices.Count == 0 ? string.Empty : choices[index].Value;
        return new OptionField(key, label, OptionFieldKind.Choice, def, choices);
    }
}
