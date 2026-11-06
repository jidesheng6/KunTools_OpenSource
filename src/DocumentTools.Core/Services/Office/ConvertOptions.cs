namespace DocumentTools.Core.Services.Office;

/// <summary>WORD→PDF options aligned with legacy BodyForm / Util.ConvertWord2Pdf.</summary>
public sealed class WordToPdfOptions
{
    public bool EmbedFonts { get; init; }

    public bool UpdateFields { get; init; }

    /// <summary>Aspose.Words.Saving.PdfCompliance name, e.g. Pdf17, PdfA1b.</summary>
    public string Compliance { get; init; } = "Pdf17";

    /// <summary>Aspose.Words.Saving.ColorMode name: Normal or Grayscale.</summary>
    public string ColorMode { get; init; } = "Normal";

    /// <summary>Aspose.Words.Settings.MsWordVersion name.</summary>
    public string MsWordVersion { get; init; } = "Word2016";

    public string? Password { get; init; }

    /// <summary>When true, prefer local Office interop path (legacy UseLocalOffice).</summary>
    public bool UseLocalOffice { get; init; }
}

public sealed class PdfToWordOptions
{
    public bool UseBullet { get; init; }

    public bool UseOcr { get; init; }

    /// <summary>Aspose.Pdf.DocSaveOptions.RecognitionMode name.</summary>
    public string Mode { get; init; } = "Flow";

    public string? Password { get; init; }
}

public sealed class ExcelToPdfOptions
{
    public bool ExportEachSheet { get; init; }

    public bool ForceContentOnOnePage { get; init; }

    public bool HorizontalCenter { get; init; }

    public bool VerticalCenter { get; init; }

    public int PrintScaleRow { get; init; }

    public int PrintScaleCol { get; init; }

    public bool PrintScalePercentEnabled { get; init; }

    public int PrintScalePercent { get; init; } = 100;
}

public sealed class PptToPdfOptions
{
    public bool DrawBlackFrame { get; init; }

    public bool ExportHiddenSlides { get; init; }

    /// <summary>Aspose.Slides.Export.NotesPositions name, e.g. None / BottomFull.</summary>
    public string NotesPosition { get; init; } = "None";

    public string? Password { get; init; }
}

public sealed class PdfToPptOptions
{
    public bool ExtraOcr { get; init; }

    public bool SplitShapes { get; init; }

    public bool RenderAsImages { get; init; }
}

/// <summary>IMAGE→PDF options aligned with legacy Util.ConvertImage2PDF (Util.cs:1805-1899).</summary>
public sealed class ImagePdfOptions
{
    /// <summary>legacy CompressPdfImage：OptimizeResources + Flate 图片压缩（Util.cs:1827-1831）。</summary>
    public bool CompressImages { get; init; } = true;

    /// <summary>
    /// legacy LandStyleIndex：0 = 页面尺寸取图片像素尺寸、1 = 横向、2 = 纵向（Util.cs:1840-1854）。
    /// </summary>
    public int LandStyleIndex { get; init; }

    /// <summary>legacy AutoOptiomizePdf：额外调用 Document.Optimize()（Util.cs:1861-1865）。</summary>
    public bool AutoOptimize { get; init; }

    public double MarginTop { get; init; }

    public double MarginLeft { get; init; }

    public double MarginBottom { get; init; }

    public double MarginRight { get; init; }
}

public sealed class WordToEpubOptions
{
    public string Author { get; init; } = "DocumentTools";

    /// <summary>自定义封面图片路径；为空时用文档第一页渲染成封面（legacy 行为）。</summary>
    public string? CoverImagePath { get; init; }

    /// <summary>无自定义封面时渲染第一页的 DPI（legacy CoverImageDpi，默认 100，服务端 clamp 24-600）。</summary>
    public int CoverDpi { get; init; } = 100;
}
