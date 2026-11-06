using DocumentTools.Core.Operations;

namespace DocumentTools.Tests;

public sealed class DocumentOperationRunnerTests
{
    private static readonly DocumentOperationRequest Request = new()
    {
        InputFiles = ["input.docx"],
        OutputDirectory = "output",
        Options = new Dictionary<string, string>()
    };

    [Fact]
    public async Task SynchronousOperation_RunsAwayFromCallingThread()
    {
        int callingThread = Environment.CurrentManagedThreadId;
        var operation = new RecordingOperation("word.pdf");
        var runner = CreateRunner();

        DocumentOperationResult result = await runner.ExecuteAsync(operation, Request);

        Assert.True(result.Success);
        Assert.NotEqual(callingThread, operation.ExecutingThread);
    }

    [Fact]
    public async Task LocalOfficeOperation_RunsOnDedicatedStaThread()
    {
        var operation = new RecordingOperation("word.pdf");
        var runner = CreateRunner();
        var request = new DocumentOperationRequest
        {
            InputFiles = ["input.docx"],
            OutputDirectory = "output",
            Options = new Dictionary<string, string> { ["useLocalOffice"] = "true" }
        };

        DocumentOperationResult result = await runner.ExecuteAsync(operation, request);

        Assert.True(result.Success);
        Assert.Equal(ApartmentState.STA, operation.ApartmentState);
    }

    [Fact]
    public async Task CancellationDuringSynchronousOperation_ProducesCanceledTask()
    {
        using var cancellation = new CancellationTokenSource();
        using var operation = new BlockingCancelableOperation();
        var runner = CreateRunner();

        Task<DocumentOperationResult> pending = runner.ExecuteAsync(operation, Request, cancellation.Token);
        Assert.True(operation.Started.Wait(TimeSpan.FromSeconds(5)), "操作没有进入后台执行线程。");

        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.True(pending.IsCanceled);
    }

    [Fact]
    public async Task OperationException_IsPreservedForCaller()
    {
        var runner = CreateRunner();
        var operation = new ThrowingOperation();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => runner.ExecuteAsync(operation, Request));

        Assert.Equal("runner-test", error.Message);
    }

    private sealed class RecordingOperation(string featureId) : IDocumentOperation
    {
        public string FeatureId { get; } = featureId;

        public int ExecutingThread { get; private set; }

        public ApartmentState ApartmentState { get; private set; }

        public Task<DocumentOperationResult> ExecuteAsync(
            DocumentOperationRequest request,
            CancellationToken cancellationToken = default)
        {
            ExecutingThread = Environment.CurrentManagedThreadId;
            ApartmentState = Thread.CurrentThread.GetApartmentState();
            return Task.FromResult(DocumentOperationResult.Ok("ok"));
        }
    }

    private sealed class BlockingCancelableOperation : IDocumentOperation, IDisposable
    {
        public string FeatureId => "pdf.merge";

        public ManualResetEventSlim Started { get; } = new(initialState: false);

        public Task<DocumentOperationResult> ExecuteAsync(
            DocumentOperationRequest request,
            CancellationToken cancellationToken = default)
        {
            Started.Set();
            cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(DocumentOperationResult.Fail("等待取消超时"));
        }

        public void Dispose() => Started.Dispose();
    }

    private sealed class ThrowingOperation : IDocumentOperation
    {
        public string FeatureId => "pdf.merge";

        public Task<DocumentOperationResult> ExecuteAsync(
            DocumentOperationRequest request,
            CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("runner-test");
    }

    private static DocumentOperationRunner CreateRunner()
        => new(new BackgroundOperationRunner());
}
