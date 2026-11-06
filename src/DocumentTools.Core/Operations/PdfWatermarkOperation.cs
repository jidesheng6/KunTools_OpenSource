using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>Maps pdf.watermark.add / pdf.watermark.remove.</summary>
public sealed class PdfWatermarkOperation : IDocumentOperation
{
    private readonly IPdfWatermarkService _watermark;

    public PdfWatermarkOperation(string featureId, IPdfWatermarkService watermark)
    {
        FeatureId = featureId;
        _watermark = watermark;
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

        try
        {
            Directory.CreateDirectory(request.OutputDirectory);
            var outputs = new List<string>();
            bool remove = FeatureId == "pdf.watermark.remove";

            PdfWatermarkOptions options = BuildOptions(request);

            foreach (string input in request.InputFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string name = Path.GetFileNameWithoutExtension(input);
                string suffix = remove ? "_无水印.pdf" : "_水印.pdf";
                string output = Path.Combine(request.OutputDirectory, name + suffix);

                if (remove)
                {
                    _watermark.Remove(input, output);
                }
                else
                {
                    _watermark.Apply(input, output, options);
                }

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

    private static PdfWatermarkOptions BuildOptions(DocumentOperationRequest request)
    {
        // UI keys from feature-ux-spec / GenericConvertView; accept legacy aliases too.
        string text = FirstOption(request, "text") ?? string.Empty;
        string image = FirstOption(request, "image", "imageFile") ?? string.Empty;
        bool useFileName = ParseBool(FirstOption(request, "useFileName"));

        double opacity = 0.5;
        if (double.TryParse(FirstOption(request, "opacity"), out double parsedOpacity))
        {
            opacity = parsedOpacity;
        }

        double rotation = 0;
        if (double.TryParse(FirstOption(request, "rotate", "rotation"), out double parsedRotation))
        {
            rotation = parsedRotation;
        }

        string fontName = FirstOption(request, "fontName", "font") ?? PdfWatermarkOptions.DefaultFontName;
        double fontSize = PdfWatermarkOptions.DefaultFontSize;
        if (double.TryParse(FirstOption(request, "fontSize"), out double parsedFontSize) && parsedFontSize > 0)
        {
            fontSize = parsedFontSize;
        }

        bool repeat = ParseBool(FirstOption(request, "repeat", "fullScreen"));

        // legacy 平铺横纵间距是两个独立输入（Util.cs:598-735 的 Dmi_Hor/Dmi_Ver）；
        // 向后兼容：旧键 repeatSpacing/fullScreenSpacing 存在时优先（单值同时给 X/Y），
        // 否则用新键 repeatSpacingX/repeatSpacingY，再否则回退 DefaultRepeatSpacing。
        double spacingX = PdfWatermarkOptions.DefaultRepeatSpacing;
        double spacingY = PdfWatermarkOptions.DefaultRepeatSpacing;
        if (double.TryParse(FirstOption(request, "repeatSpacing", "fullScreenSpacing"), out double legacySpacing)
            && legacySpacing > 0)
        {
            spacingX = legacySpacing;
            spacingY = legacySpacing;
        }
        else
        {
            spacingX = ParsePositive(FirstOption(request, "repeatSpacingX"), spacingX);
            spacingY = ParsePositive(FirstOption(request, "repeatSpacingY"), spacingY);
        }

        return new PdfWatermarkOptions
        {
            Text = text,
            ImageFile = image,
            UseFileNameAsText = useFileName || (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(image)),
            Opacity = opacity,
            RotationDegrees = rotation,
            Horizontal = ParseHorizontal(FirstOption(request, "horizontal")),
            Vertical = ParseVertical(FirstOption(request, "vertical")),
            FontColor = ParseColor(FirstOption(request, "fontColor")),
            FontName = fontName,
            FontSize = fontSize,
            ImageWidth = ParsePositive(FirstOption(request, "imageWidth"), PdfWatermarkOptions.DefaultImageSize),
            ImageHeight = ParsePositive(FirstOption(request, "imageHeight"), PdfWatermarkOptions.DefaultImageSize),
            Repeat = repeat,
            RepeatSpacingX = spacingX,
            RepeatSpacingY = spacingY,
            PlaceBehindContent = ParseBool(FirstOption(request, "underContent", "placeBehind")),
            CustomPosition = ParseBool(FirstOption(request, "customPosition")),
            PositionX = ParseFinite(FirstOption(request, "positionX"), 0),
            PositionY = ParseFinite(FirstOption(request, "positionY"), 0)
        };
    }

    private static double ParsePositive(string? text, double fallback)
        => double.TryParse(text, out double value) && value > 0 && !double.IsNaN(value) && !double.IsInfinity(value)
            ? value
            : fallback;

    private static double ParseFinite(string? text, double fallback)
        => double.TryParse(text, out double value) && !double.IsNaN(value) && !double.IsInfinity(value)
            ? value
            : fallback;

    /// <summary>值来自 Schema 的 WatermarkHorizontals（枚举名）；缺省或非法回退 Center（与 legacy 下拉默认一致）。</summary>
    private static PdfWatermarkHorizontal ParseHorizontal(string? text)
        => Enum.TryParse(text, ignoreCase: true, out PdfWatermarkHorizontal value)
            ? value
            : PdfWatermarkHorizontal.Center;

    /// <summary>值来自 Schema 的 WatermarkVerticals（枚举名）；缺省或非法回退 Center。</summary>
    private static PdfWatermarkVertical ParseVertical(string? text)
        => Enum.TryParse(text, ignoreCase: true, out PdfWatermarkVertical value)
            ? value
            : PdfWatermarkVertical.Center;

    /// <summary>支持 #RRGGBB / RRGGBB / rgb(r,g,b)；缺省或非法回退红色（legacy 默认字体颜色）。</summary>
    private static RgbColor ParseColor(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return RgbColor.Red;
        }

        string value = text.Trim();
        if (value.StartsWith('#') || (value.Length == 6 && value.All(Uri.IsHexDigit)))
        {
            string hex = value.TrimStart('#');
            if (hex.Length == 6 && int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int rgb))
            {
                return new RgbColor((byte)((rgb >> 16) & 0xFF), (byte)((rgb >> 8) & 0xFF), (byte)(rgb & 0xFF));
            }
        }

        return RgbColor.Red;
    }

    private static string? FirstOption(DocumentOperationRequest request, params string[] keys)
    {
        if (request.Options is null)
        {
            return null;
        }

        foreach (string key in keys)
        {
            if (request.Options.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static bool ParseBool(string? text)
        => string.Equals(text, "1", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase);
}
