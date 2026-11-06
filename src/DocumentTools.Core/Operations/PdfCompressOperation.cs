using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>Maps pdf.compress.</summary>
public sealed class PdfCompressOperation : IDocumentOperation
{
    private readonly IPdfCompressionService _compress;

    public PdfCompressOperation(IPdfCompressionService compress)
    {
        FeatureId = "pdf.compress";
        _compress = compress;
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
            var keptOriginal = new List<string>();
            var options = new PdfCompressionOptions
            {
                WebOptimize = ParseBool(GetOption(request, "webOptimize")),
                CompressImages = !ParseBool(GetOption(request, "skipImages")),
                ImageQuality = ParseQuality(GetOption(request, "quality") ?? GetOption(request, "imageQuality"), 75),
                ImageVersion = ParseImageVersion(GetOption(request, "imageVersion")),
                MaxResolutionDpi = ParseInt(GetOption(request, "maxDpi"), 0),
                // legacy 这三项在旧界面是独立可关的开关（Util.cs:1460-1522）；老配置没有这些键时默认仍为 true。
                RemoveUnusedStreams = ParseBoolDefaultTrue(GetOption(request, "removeUnusedStreams")),
                LinkDuplicateStreams = ParseBoolDefaultTrue(GetOption(request, "linkDuplicateStreams")),
                RemoveUnusedObjects = ParseBoolDefaultTrue(GetOption(request, "removeUnusedObjects")),
                UnembedFonts = ParseBool(GetOption(request, "unembedFonts")),
                FlattenFormFields = ParseBool(GetOption(request, "flattenForms")),
                UseFlateEncoding = ParseBool(GetOption(request, "flate"))
            };

            foreach (string input in request.InputFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string output = Path.Combine(
                    request.OutputDirectory,
                    Path.GetFileNameWithoutExtension(input) + "_压缩.pdf");
                PdfCompressionResult result = _compress.Compress(input, output, options);
                outputs.Add(output);
                if (result.KeptOriginal)
                {
                    keptOriginal.Add(Path.GetFileName(output));
                }
            }

            // 低熵 PDF 经 Aspose.Pdf 重写只会变大（连纯 load+save 都 134.9%），服务侧已回退原始内容；
            // 这里必须把事实说出来，否则用户看到「压缩完成」却发现体积一模一样会以为没生效。
            string message = $"压缩完成 {outputs.Count} 个文件。";
            if (keptOriginal.Count > 0)
            {
                message += $"其中 {keptOriginal.Count} 个文件无可压缩空间，已保留原始内容（{string.Join("、", keptOriginal)}）。";
            }

            return Task.FromResult(DocumentOperationResult.Ok(message, outputs));
        }
        catch (Exception ex)
        {
            return Task.FromResult(DocumentOperationResult.Fail(ex.Message));
        }
    }

    private static string? GetOption(DocumentOperationRequest request, string key)
    {
        if (request.Options is null)
        {
            return null;
        }

        return request.Options.TryGetValue(key, out string? value) ? value : null;
    }

    private static bool ParseBool(string? text)
        => string.Equals(text, "1", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase);

    /// <summary>键缺省时保持 true（老配置行为不回退）；只有显式填 0/false/no 才关闭。</summary>
    private static bool ParseBoolDefaultTrue(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        return !(string.Equals(text, "0", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(text, "false", StringComparison.OrdinalIgnoreCase)
                 || string.Equals(text, "no", StringComparison.OrdinalIgnoreCase));
    }

    private static PdfImageCompressionVersion ParseImageVersion(string? text)
        => Enum.TryParse(text, ignoreCase: true, out PdfImageCompressionVersion version)
            ? version
            : PdfImageCompressionVersion.Standard;

    private static int ParseInt(string? text, int fallback)
        => int.TryParse(text, out int n) ? n : fallback;

    /// <summary>
    /// `quality` 现为 1-100 文本（原「低/中/高」Choice 已改）；为兼容旧配置仍先认低/中/高，
    /// 再按整数解析并夹到 1..100，非法值回退 75。
    /// </summary>
    private static int ParseQuality(string? text, int fallback)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return fallback;
        }

        switch (text.Trim())
        {
            case "低" or "low":
                return 40;
            case "中" or "medium" or "med":
                return 75;
            case "高" or "high":
                return 92;
        }

        return int.TryParse(text, out int value) ? Math.Clamp(value, 1, 100) : fallback;
    }
}
