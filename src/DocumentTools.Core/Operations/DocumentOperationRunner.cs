namespace DocumentTools.Core.Operations;

/// <summary>
/// Executes document operations away from the UI thread.
/// </summary>
/// <remarks>
/// Most operations are synchronous wrappers around Aspose or image APIs even though their
/// contract returns <see cref="Task{TResult}"/>. Local Microsoft Word automation is the exception:
/// COM requires an STA thread, so that path uses a dedicated STA worker instead of the thread pool.
/// Cancellation is cooperative; the runner never aborts a worker thread while it may be writing a file.
/// </remarks>
public interface IDocumentOperationRunner
{
    Task<DocumentOperationResult> ExecuteAsync(
        IDocumentOperation operation,
        DocumentOperationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class DocumentOperationRunner : IDocumentOperationRunner
{
    private readonly IBackgroundOperationRunner _background;

    public DocumentOperationRunner(IBackgroundOperationRunner background)
        => _background = background;

    public Task<DocumentOperationResult> ExecuteAsync(
        IDocumentOperation operation,
        DocumentOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(request);

        BackgroundExecutionMode mode = RequiresSta(operation, request)
            ? BackgroundExecutionMode.StaThread
            : BackgroundExecutionMode.ThreadPool;

        return _background.RunAsync(
            token => operation.ExecuteAsync(request, token),
            cancellationToken,
            mode);
    }

    private static bool RequiresSta(IDocumentOperation operation, DocumentOperationRequest request)
        => string.Equals(operation.FeatureId, "word.pdf", StringComparison.OrdinalIgnoreCase)
           && IsTrue(request.Options, "useLocalOffice");

    private static bool IsTrue(IDictionary<string, string>? options, string key)
        => options is not null
           && options.TryGetValue(key, out string? value)
           && (string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase));
}
