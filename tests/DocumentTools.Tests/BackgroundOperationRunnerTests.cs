using DocumentTools.Core.Operations;

namespace DocumentTools.Tests;

public sealed class BackgroundOperationRunnerTests
{
    [Fact]
    public async Task SynchronousWork_RunsAwayFromCallingThread()
    {
        int callingThread = Environment.CurrentManagedThreadId;
        int workerThread = 0;
        var runner = new BackgroundOperationRunner();

        int result = await runner.RunAsync(token =>
        {
            workerThread = Environment.CurrentManagedThreadId;
            return 42;
        });

        Assert.Equal(42, result);
        Assert.NotEqual(callingThread, workerThread);
    }

    [Fact]
    public async Task CooperativeCancellation_ProducesCanceledTask()
    {
        using var started = new ManualResetEventSlim(false);
        using var cancellation = new CancellationTokenSource();
        var runner = new BackgroundOperationRunner();

        Task<int> pending = runner.RunAsync(token =>
        {
            started.Set();
            token.WaitHandle.WaitOne(TimeSpan.FromSeconds(10));
            token.ThrowIfCancellationRequested();
            return 1;
        }, cancellation.Token);

        Assert.True(started.Wait(TimeSpan.FromSeconds(5)), "后台委托没有开始执行。");
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending);
        Assert.True(pending.IsCanceled);
    }

    [Fact]
    public async Task Exception_IsPreservedForCaller()
    {
        var runner = new BackgroundOperationRunner();

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(
            () => runner.RunAsync<int>(
                (Func<CancellationToken, int>)(_ => throw new InvalidOperationException("background-test"))));

        Assert.Equal("background-test", error.Message);
    }

    [Fact]
    public async Task TypedProgress_IsForwardedToOperation()
    {
        var values = new List<int>();
        var progress = new InlineProgress<int>(values.Add);
        var runner = new BackgroundOperationRunner();

        string result = await runner.RunAsync<string, int>(
            (token, reporter) =>
            {
                reporter?.Report(25);
                reporter?.Report(100);
                return "done";
            },
            progress);

        Assert.Equal("done", result);
        Assert.Equal([25, 100], values);
    }

    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
