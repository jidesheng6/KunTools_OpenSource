namespace DocumentTools.Core.Services.Office;

public enum PdfWatermarkHorizontal
{
    None = 0,
    Left = 1,
    Center = 2,
    Right = 3,
    Justify = 4,
    FullJustify = 5
}

public enum PdfWatermarkVertical
{
    None = 0,
    Top = 1,
    Center = 2,
    Bottom = 3
}

/// <summary>UI-agnostic RGB (avoids System.Drawing in Core).</summary>
public readonly struct RgbColor
{
    public RgbColor(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }

    public byte R { get; }
    public byte G { get; }
    public byte B { get; }

    public static RgbColor Red => new(255, 0, 0);
}

/// <summary>PDF 水印参数。复杂字段可通过 Options 字典映射。</summary>
public sealed class PdfWatermarkOptions
{
    public const string DefaultFontName = "宋体";
    public const double DefaultOpacity = 0.5;
    public const double DefaultFontSize = 12;
    public const double DefaultImageSize = 100;
    public const double DefaultRepeatSpacing = 25;

    public string Text { get; set; } = string.Empty;

    public string ImageFile { get; set; } = string.Empty;

    public double Opacity { get; set; } = DefaultOpacity;

    public double RotationDegrees { get; set; }

    public PdfWatermarkHorizontal Horizontal { get; set; } = PdfWatermarkHorizontal.Center;

    public PdfWatermarkVertical Vertical { get; set; } = PdfWatermarkVertical.Center;

    public RgbColor FontColor { get; set; } = RgbColor.Red;

    public string FontName { get; set; } = DefaultFontName;

    public double FontSize { get; set; } = DefaultFontSize;

    public double ImageWidth { get; set; } = DefaultImageSize;

    public double ImageHeight { get; set; } = DefaultImageSize;

    public bool Repeat { get; set; }

    public double RepeatSpacingX { get; set; } = DefaultRepeatSpacing;

    public double RepeatSpacingY { get; set; } = DefaultRepeatSpacing;

    public bool UseFileNameAsText { get; set; }

    public bool PlaceBehindContent { get; set; }

    public bool CustomPosition { get; set; }

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public bool HasContent =>
        !string.IsNullOrWhiteSpace(Text)
        || !string.IsNullOrWhiteSpace(ImageFile)
        || UseFileNameAsText;

    public bool UsesImage => !string.IsNullOrWhiteSpace(ImageFile);

    public PdfWatermarkOptions Normalize()
        => new()
        {
            Text = Text ?? string.Empty,
            ImageFile = ImageFile ?? string.Empty,
            Opacity = ClampOpacity(Opacity),
            RotationDegrees = NormalizeDegrees(RotationDegrees),
            Horizontal = Horizontal,
            Vertical = Vertical,
            FontColor = FontColor,
            FontName = string.IsNullOrWhiteSpace(FontName) ? DefaultFontName : FontName,
            FontSize = Positive(FontSize, DefaultFontSize),
            ImageWidth = Positive(ImageWidth, DefaultImageSize),
            ImageHeight = Positive(ImageHeight, DefaultImageSize),
            Repeat = Repeat,
            RepeatSpacingX = Positive(RepeatSpacingX, DefaultRepeatSpacing),
            RepeatSpacingY = Positive(RepeatSpacingY, DefaultRepeatSpacing),
            UseFileNameAsText = UseFileNameAsText,
            PlaceBehindContent = PlaceBehindContent,
            CustomPosition = CustomPosition,
            PositionX = FiniteOrZero(PositionX),
            PositionY = FiniteOrZero(PositionY)
        };

    private static double ClampOpacity(double value)
    {
        if (double.IsNaN(value))
        {
            return DefaultOpacity;
        }

        if (value < 0)
        {
            return 0;
        }

        return value > 1 ? 1 : value;
    }

    private static double NormalizeDegrees(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            return 0;
        }

        double folded = value % 360;
        return folded < 0 ? folded + 360 : folded;
    }

    private static double Positive(double value, double fallback)
        => double.IsNaN(value) || double.IsInfinity(value) || value <= 0 ? fallback : value;

    private static double FiniteOrZero(double value)
        => double.IsNaN(value) || double.IsInfinity(value) ? 0 : value;
}

public interface IPdfWatermarkService
{
    void Apply(string sourceFile, string targetFile, PdfWatermarkOptions options);

    void Remove(string sourceFile, string targetFile);
}
