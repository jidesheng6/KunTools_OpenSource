namespace DocumentTools.Core.Features;

/// <summary>Canonical feature list (ids stay stable across UI frameworks).</summary>
public static class BuiltInFeatures
{
    public static IReadOnlyList<FeatureDescriptor> All { get; } =
    [
        // Shell
        new("home", "首页", FeatureCategory.Home, "功能入口与快速导航", "home", "sy"),
        new("about", "关于软件", FeatureCategory.About, "版本与许可信息", "about", "gy"),

        // Word
        new("word.pdf", "WORD 转 PDF", FeatureCategory.Word, "将 Word 文档批量转为 PDF", "word", "pdf", "w2p"),
        new("word.merge", "合并 WORD", FeatureCategory.Word, "按顺序合并多个 Word", "word", "merge", "hb", "hbword"),
        new("word.split", "拆分 WORD", FeatureCategory.Word, "按页数拆分 Word", "word", "split", "cf", "cfword"),
        new("word.excel", "WORD 转 EXCEL", FeatureCategory.Word, "将 Word 转为 Excel", "word", "excel", "w2e", "zdexcel"),
        new("word.image", "WORD/PDF/EXCEL 转图片", FeatureCategory.Word, "将文档按页导出为图片", "word", "image", "img", "w2i", "zdtp"),
        new("word.epub", "WORD 转 EPUB", FeatureCategory.Word, "将 Word 转为 EPUB", "word", "epub", "zepub"),
        new("word.replace", "WORD 内容批量替换", FeatureCategory.Word, "批量替换 Word 文档中的文本", "word", "replace", "th", "plth"),

        // Pdf
        new("pdf.word", "PDF 转 WORD", FeatureCategory.Pdf, "将 PDF 转为 Word", "pdf", "word", "p2w"),
        new("pdf.excel", "PDF 转 EXCEL", FeatureCategory.Pdf, "将 PDF 转为 Excel", "pdf", "excel", "p2e", "zdexcel"),
        new("pdf.ppt", "PDF 转 PPT", FeatureCategory.Pdf, "将 PDF 转为 PPT", "pdf", "ppt", "zdppt"),
        new("pdf.merge", "合并 PDF", FeatureCategory.Pdf, "按顺序合并多个 PDF", "pdf", "merge", "hb", "hbpdf"),
        new("pdf.split", "拆分 PDF", FeatureCategory.Pdf, "按页数拆分 PDF", "pdf", "split", "cf", "cfpdf"),
        new("pdf.watermark.add", "添加 PDF 水印", FeatureCategory.Pdf, "为 PDF 添加文本或图片水印", "pdf", "watermark", "sy", "tjswy"),
        new("pdf.watermark.remove", "移除 PDF 水印", FeatureCategory.Pdf, "移除 PDF 中的水印与背景层", "pdf", "watermark", "remove", "ycswy"),
        new("pdf.compliance", "PDF 版本转换", FeatureCategory.Pdf, "转换 PDF 标准 / 合规版本", "pdf", "version", "compliance", "bbzh"),
        new("pdf.compress", "压缩 PDF", FeatureCategory.Pdf, "优化并压缩 PDF", "pdf", "compress", "ys", "yspdf"),
        new("html.pdf", "HTML 转 PDF", FeatureCategory.Pdf, "通过浏览器引擎将 HTML 渲染为 PDF", "html", "pdf", "web", "zdpdf"),

        // Excel
        new("excel.pdf", "EXCEL 转 PDF", FeatureCategory.Excel, "将 Excel 转为 PDF", "excel", "pdf", "e2p"),
        new("excel.word", "EXCEL 转 WORD", FeatureCategory.Excel, "将 Excel 转为 Word", "excel", "word", "e2w", "zdword"),
        new("excel.replace", "EXCEL 内容批量替换", FeatureCategory.Excel, "批量替换 Excel 中的文本", "excel", "replace", "th", "plth"),

        // Ppt
        new("ppt.pdf", "PPT 转 PDF", FeatureCategory.Ppt, "将 PPT 转为 PDF", "ppt", "pdf"),
        new("ppt.split", "拆分 PPT", FeatureCategory.Ppt, "按幻灯片拆分 PPT", "ppt", "split", "cf", "cfppt"),

        // Image
        new("image.pdf", "图片转 PDF", FeatureCategory.Image, "将图片合并为 PDF", "image", "pdf", "img"),
        new("image.compress", "压缩图片", FeatureCategory.Image, "压缩图片并可选格式转换", "image", "compress", "img", "ystp"),
        new("webp.image", "WEBP 转图片", FeatureCategory.Image, "将 WEBP 转换为常见图片格式", "webp", "image", "zdtp"),
        new("image.ico", "图片转 ICO", FeatureCategory.Image, "将图片转换为 ICO 图标", "image", "ico", "zico"),

        // Epub
        new("epub.word", "EPUB 转 WORD", FeatureCategory.Epub, "将 EPUB 转为 Word", "epub", "word", "zdword"),
        new("epub.pdf", "EPUB 转 PDF", FeatureCategory.Epub, "将 EPUB 转为 PDF", "epub", "pdf", "zdpdf"),

        // Tools
        new("code.barcode", "简易二维码", FeatureCategory.Tools, "生成条码 / 二维码", "barcode", "qr"),
        new("code.datamatrix", "特殊二维码校验", FeatureCategory.Tools, "校验特殊二维码 / DataMatrix", "barcode", "datamatrix", "qrcheck"),
        new("idcard.check", "身份证校验", FeatureCategory.Tools, "校验中国大陆居民身份证号", "idcard", "sfz"),
        new("name.vertical", "姓名横转竖", FeatureCategory.Tools, "姓名列表竖排处理", "name", "vertical"),
        new("twain.debug", "Twain 扫描仪调试", FeatureCategory.Tools, "调试 Twain 扫描仪设备", "twain", "scan"),
        new("other.misc", "其他功能", FeatureCategory.Tools, "杂项工具入口（原「其他功能」页）", "other", "misc", "extras"),
        new("other.extra", "补充功能", FeatureCategory.Tools, "补充工具入口", "other", "extra", "buchong"),

        // Exam
        new("exam.mss3", "MSS3 模板修改", FeatureCategory.Exam, "修改 MSS3 考评模板", "exam", "mss3"),
        new("exam.single", "单人表生成", FeatureCategory.Exam, "生成单人考评表", "exam", "single"),
        new("exam.insert", "考评表信息插入", FeatureCategory.Exam, "向考评表插入信息", "exam", "insert"),
        new("exam.report", "考评报表修改", FeatureCategory.Exam, "修改考评报表", "exam", "report"),
        new("exam.vote", "选举结果报告单样式生成", FeatureCategory.Exam, "生成选举结果报告单样式", "exam", "vote"),
        new("exam.package", "考评系统数据包管理", FeatureCategory.Exam, "管理考评系统数据包", "exam", "package"),
        new("exam.circle", "考评圆圈修复工具", FeatureCategory.Exam, "修复考评表圆圈标记", "exam", "circle"),

        // System
        new("system.settings", "系统设置", FeatureCategory.System, "转换弹窗、列表、路径等首选项（不含锚点/窗体大小）", "system", "settings", "config"),
        new("system.tools", "系统工具", FeatureCategory.System, "系统维护与诊断工具", "system", "tools"),
        new("system.network", "网卡设置", FeatureCategory.System, "查看与配置网卡信息", "system", "network", "nic"),
    ];

    /// <summary>Batch file features that share GenericConvertView / IDocumentOperation.</summary>
    public static IReadOnlyList<FeatureDescriptor> ConvertFeatures { get; } =
        All.Where(f => f.Id is
                "word.pdf" or "pdf.word" or "excel.pdf" or "ppt.pdf" or "image.pdf"
                or "pdf.excel" or "pdf.ppt" or "word.excel" or "excel.word"
                or "word.epub" or "epub.word" or "epub.pdf"
                or "word.image" or "pdf.compliance"
                or "pdf.merge" or "word.merge"
                or "pdf.split" or "word.split" or "ppt.split"
                or "pdf.watermark.add" or "pdf.watermark.remove"
                or "pdf.compress" or "html.pdf"
                or "image.compress" or "webp.image" or "image.ico")
            .ToList();
}
