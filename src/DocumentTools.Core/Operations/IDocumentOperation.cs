namespace DocumentTools.Core.Operations;

/// <summary>
/// Strategy: one operation per feature (or shared convert pipeline keyed by FeatureId).
/// Presenters call this instead of referencing Aspose types.
/// </summary>
public interface IDocumentOperation
{
    string FeatureId { get; }

    Task<DocumentOperationResult> ExecuteAsync(
        DocumentOperationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class DocumentOperationRequest
{
    public required IReadOnlyList<string> InputFiles { get; init; }

    public required string OutputDirectory { get; init; }

    public IDictionary<string, string>? Options { get; init; }
}

public sealed class DocumentOperationResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public IReadOnlyList<string> OutputFiles { get; init; } = Array.Empty<string>();

    public static DocumentOperationResult Ok(string message, IReadOnlyList<string>? outputs = null)
        => new()
        {
            Success = true,
            Message = message,
            OutputFiles = outputs ?? Array.Empty<string>()
        };

    public static DocumentOperationResult Fail(string message)
        => new() { Success = false, Message = message };
}
