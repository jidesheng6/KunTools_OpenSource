using DocumentTools.Core.Features;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Ui;

/// <summary>
/// Maps features to WPF-UI Fluent symbols. Prefer these over shipping WinForms PNG resources.
/// </summary>
public static class FeatureIcons
{
    /// <summary>侧边栏条目 / 卡片标题行的图标尺寸（Fluent 正文行 16，卡片 20）。</summary>
    public const double NavIconSize = 16;

    public const double CardIconSize = 20;

    public const double GroupIconSize = 15;

    public static SymbolRegular ForFeature(string featureId)
        => featureId.ToLowerInvariant() switch
        {
            "home" => SymbolRegular.Home24,
            "about" => SymbolRegular.Info24,

            "word.pdf" => SymbolRegular.DocumentPdf24,
            "word.merge" => SymbolRegular.DocumentMultiple24,
            "word.split" => SymbolRegular.DocumentLandscapeSplitHint24,
            "word.excel" => SymbolRegular.Table24,
            "word.image" => SymbolRegular.ImageMultiple24,
            "word.epub" => SymbolRegular.BookOpen24,
            "word.replace" => SymbolRegular.TextEditStyle24,

            "pdf.word" => SymbolRegular.DocumentText24,
            "pdf.excel" => SymbolRegular.TableSimple24,
            "pdf.ppt" => SymbolRegular.SlideText24,
            "pdf.merge" => SymbolRegular.DocumentCopy24,
            "pdf.split" => SymbolRegular.DocumentPageBreak24,
            "pdf.watermark.add" => SymbolRegular.InkingTool24,
            "pdf.watermark.remove" => SymbolRegular.Eraser24,
            "pdf.compliance" => SymbolRegular.DocumentArrowRight24,
            "pdf.compress" => SymbolRegular.Archive24,
            "html.pdf" => SymbolRegular.Globe24,

            "excel.pdf" => SymbolRegular.DocumentPdf24,
            "excel.word" => SymbolRegular.DocumentText24,
            "excel.replace" => SymbolRegular.TextEditStyle24,

            "ppt.pdf" => SymbolRegular.DocumentPdf24,
            "ppt.split" => SymbolRegular.SplitVertical24,

            "image.pdf" => SymbolRegular.DocumentPdf24,
            "image.compress" => SymbolRegular.Archive24,
            "webp.image" => SymbolRegular.Image24,
            "image.ico" => SymbolRegular.Icons24,

            "epub.word" => SymbolRegular.DocumentText24,
            "epub.pdf" => SymbolRegular.DocumentPdf24,

            "code.barcode" => SymbolRegular.QrCode24,
            "code.datamatrix" => SymbolRegular.BarcodeScanner24,
            "idcard.check" => SymbolRegular.ContactCard24,
            "name.vertical" => SymbolRegular.TextDirectionVertical24,
            "twain.debug" => SymbolRegular.ScanCamera24,
            "other.misc" => SymbolRegular.AppsList24,
            "other.extra" => SymbolRegular.Toolbox24,

            "exam.mss3" => SymbolRegular.ClipboardTask24,
            "exam.single" => SymbolRegular.Person24,
            "exam.insert" => SymbolRegular.Form24,
            "exam.report" => SymbolRegular.DocumentData24,
            "exam.vote" => SymbolRegular.ClipboardCheckmark24,
            "exam.package" => SymbolRegular.Box24,
            "exam.circle" => SymbolRegular.CircleEdit24,

            "system.settings" => SymbolRegular.Settings24,
            "system.tools" => SymbolRegular.WindowWrench24,
            "system.network" => SymbolRegular.VirtualNetwork24,

            _ => ForCategory(GuessCategory(featureId))
        };

    public static SymbolRegular ForCategory(FeatureCategory category)
        => category switch
        {
            FeatureCategory.Home => SymbolRegular.Home24,
            FeatureCategory.Word => SymbolRegular.Document24,
            FeatureCategory.Pdf => SymbolRegular.DocumentPdf24,
            FeatureCategory.Excel => SymbolRegular.Table24,
            FeatureCategory.Ppt => SymbolRegular.SlideText24,
            FeatureCategory.Image => SymbolRegular.Image24,
            FeatureCategory.Epub => SymbolRegular.Book24,
            FeatureCategory.Tools => SymbolRegular.Wrench24,
            FeatureCategory.Exam => SymbolRegular.ClipboardTask24,
            FeatureCategory.System => SymbolRegular.Desktop24,
            FeatureCategory.About => SymbolRegular.Info24,
            _ => SymbolRegular.Apps24
        };

    public static SymbolIcon Create(string featureId, double fontSize = 18)
        => new()
        {
            Symbol = ForFeature(featureId),
            FontSize = fontSize,
            Margin = new System.Windows.Thickness(0, 0, 10, 0),
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };

    public static SymbolIcon Create(SymbolRegular symbol, double fontSize = 16, double rightMargin = 8)
        => new()
        {
            Symbol = symbol,
            FontSize = fontSize,
            Margin = new System.Windows.Thickness(0, 0, rightMargin, 0),
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };

    private static FeatureCategory GuessCategory(string featureId)
    {
        if (featureId.StartsWith("word.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.Word;
        }

        if (featureId.StartsWith("pdf.", StringComparison.OrdinalIgnoreCase) ||
            featureId.StartsWith("html.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.Pdf;
        }

        if (featureId.StartsWith("excel.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.Excel;
        }

        if (featureId.StartsWith("ppt.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.Ppt;
        }

        if (featureId.StartsWith("image.", StringComparison.OrdinalIgnoreCase) ||
            featureId.StartsWith("webp.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.Image;
        }

        if (featureId.StartsWith("epub.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.Epub;
        }

        if (featureId.StartsWith("exam.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.Exam;
        }

        if (featureId.StartsWith("system.", StringComparison.OrdinalIgnoreCase))
        {
            return FeatureCategory.System;
        }

        return FeatureCategory.Tools;
    }
}
