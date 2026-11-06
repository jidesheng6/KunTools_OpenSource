using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>Maps pdf.merge / word.merge to <see cref="IDocumentMergeSplitService"/>.</summary>
public sealed class MergeDocumentOperation : IDocumentOperation
{
    private readonly IDocumentMergeSplitService _merge;

    public MergeDocumentOperation(string featureId, IDocumentMergeSplitService merge)
    {
        FeatureId = featureId;
        _merge = merge;
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
            string? outputFile = GetOption(request, "outputFile");
            string? customName = GetOption(request, "outputName");
            string pdfTarget = outputFile
                ?? Path.Combine(request.OutputDirectory, customName ?? "merged.pdf");
            string wordTarget = outputFile
                ?? Path.Combine(request.OutputDirectory, customName ?? "merged.docx");

            string output = FeatureId switch
            {
                "pdf.merge" => _merge.MergePdfFiles(request.InputFiles.ToList(), pdfTarget),
                "word.merge" => _merge.MergeWordFiles(request.InputFiles.ToList(), wordTarget),
                _ => throw new NotSupportedException($"未支持的合并功能：{FeatureId}")
            };

            return Task.FromResult(DocumentOperationResult.Ok("合并完成。", new[] { output }));
        }
        catch (Exception ex)
        {
            // Aspose 评估模式合并超过 4 个元素时抛的是英文原文，用户看不懂也修不了。
            Exception friendly = OperationErrors.Translate(
                ex,
                FeatureId == "word.merge" ? "合并 Word 文档" : "合并 PDF",
                request.OutputDirectory);
            return Task.FromResult(DocumentOperationResult.Fail(friendly.Message));
        }
    }

    private static string? GetOption(DocumentOperationRequest request, string key)
    {
        if (request.Options is null)
        {
            return null;
        }

        return request.Options.TryGetValue(key, out string? value) && !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : null;
    }
}
