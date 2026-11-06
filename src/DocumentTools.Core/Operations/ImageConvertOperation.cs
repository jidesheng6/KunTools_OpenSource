using DocumentTools.Core.Services.Image;
using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>Maps image.compress / webp.image / image.ico.</summary>
public sealed class ImageConvertOperation : IDocumentOperation
{
    /// <summary>legacy IcoSizeTable 的全部尺寸，multiSize 时一次性写进同一个 .ico。</summary>
    private static readonly int[] MultiIcoSizes = { 16, 32, 64, 128, 256 };

    private readonly IImageConversionService _images;

    public ImageConvertOperation(string featureId, IImageConversionService images)
    {
        FeatureId = featureId;
        _images = images;
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
            string format = GetOption(request, "format", "PNG");
            ImageEncodeOptions encodeOptions = BuildEncodeOptions(request, format);
            bool multiSize = ParseBool(GetOption(request, "multiSize", "false"));

            foreach (string input in request.InputFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string path = FeatureId switch
                {
                    "webp.image" => _images.WebpToImage(input, request.OutputDirectory, format),
                    "image.ico" => multiSize
                        ? _images.ToIco(input, request.OutputDirectory, MultiIcoSizes)
                        : _images.ToIco(input, request.OutputDirectory, ParseIcoSize(format)),
                    _ => _images.Convert(input, request.OutputDirectory, encodeOptions)
                };
                outputs.Add(path);
            }

            return Task.FromResult(
                DocumentOperationResult.Ok($"完成 {outputs.Count} 个文件。", outputs));
        }
        catch (Exception ex)
        {
            // 只读/被占用/是文件而不是目录的输出目录，.NET 原文是 "Access to the path ... is denied."，换成可操作提示。
            // 服务层已有的中文原因（如「无法写入输出文件（被占用或权限不足）：a.png」）保留，只补「怎么办」。
            Exception friendly = OperationErrors.Translate(
                ex,
                ActionLabel(),
                request.OutputDirectory,
                $"请换一个输出目录，或去掉目标文件的只读属性、关闭占用它的程序后重试（当前输出目录：{request.OutputDirectory}）。");
            return Task.FromResult(DocumentOperationResult.Fail(friendly.Message));
        }
    }

    private string ActionLabel()
    {
        return FeatureId switch
        {
            "webp.image" => "WebP 转图片",
            "image.ico" => "生成 ICO",
            _ => "转换图片"
        };
    }

    private static ImageEncodeOptions BuildEncodeOptions(DocumentOperationRequest request, string format)
    {
        return new ImageEncodeOptions
        {
            Format = format,
            JpegQuality = ParseQuality(GetOption(request, "quality", "80")),
            JpegGrayscale = GetOption(request, "jpegColorMode", "Rgb")
                .Equals("Grayscale", StringComparison.OrdinalIgnoreCase),
            PngLevel = ParseInt(GetOption(request, "pngLevel", "3"), 3, 0, 9),
            PngColor = ParsePngColor(GetOption(request, "pngColorType", "Truecolor")),
            BmpPixelBits = ParseInt(GetOption(request, "bmpPixelBits", "24"), 24, 4, 32),
            // legacy CompressImage_NotUseSelfExt 未勾选 = 沿用源后缀，但那时格式下拉是禁用的；
            // 本面板无跨字段禁用能力，故默认 false 让「输出格式」生效，勾选即恢复 legacy 行为。
            UseSourceExtension = ParseBool(GetOption(request, "useSourceExtension", "false")),
            NoCompress = ParseBool(GetOption(request, "noCompress", "false"))
        };
    }

    private static ImagePngColorType ParsePngColor(string text)
    {
        return text.Trim() switch
        {
            "Grayscale" => ImagePngColorType.Grayscale,
            "TruecolorWithAlpha" => ImagePngColorType.TruecolorWithAlpha,
            _ => ImagePngColorType.Truecolor
        };
    }

    private static string GetOption(DocumentOperationRequest request, string key, string fallback)
    {
        if (request.Options is not null
            && request.Options.TryGetValue(key, out string? value)
            && !string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return fallback;
    }

    private static bool ParseBool(string text)
    {
        return text.Trim().ToLowerInvariant() is "true" or "1" or "yes" or "是";
    }

    private static int ParseInt(string text, int fallback, int min, int max)
    {
        return int.TryParse(text, out int n) ? Math.Clamp(n, min, max) : fallback;
    }

    private static int ParseQuality(string text)
    {
        if (int.TryParse(text, out int n))
        {
            return Math.Clamp(n, 10, 100);
        }

        return text.Trim() switch
        {
            "低" => 40,
            "中" => 75,
            "高" => 92,
            _ => 80
        };
    }

    private static int ParseIcoSize(string formatLabel)
    {
        // Labels like "ICO 32"
        foreach (string part in formatLabel.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (int.TryParse(part, out int size))
            {
                return size;
            }
        }

        return 32;
    }
}
