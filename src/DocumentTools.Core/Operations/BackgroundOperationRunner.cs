namespace DocumentTools.Core.Operations;

/// <summary>Threading policy for blocking background work.</summary>
public enum BackgroundExecutionMode
{
    ThreadPool,
    StaThread
}

/// <summary>
/// Runs blocking application work away from the caller thread without depending on a UI framework.
/// </summary>
/// <remarks>
/// Cancellation is cooperative: the token is passed to the delegate and worker threads are never
/// aborted. The generic progress overload accepts each feature's existing progress DTO, avoiding a
/// dependency on one global progress shape.
/// </remarks>
public interface IBackgroundOperationRunner
{
    Task<TResult> RunAsync<TResult>(
        Func<CancellationToken, TResult> operation,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool);

    Task<TResult> RunAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool);

    Task<TResult> RunAsync<TResult, TProgress>(
        Func<CancellationToken, IProgress<TProgress>?, TResult> operation,
        IProgress<TProgress>? progress = null,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool);

    Task<TResult> RunAsync<TResult, TProgress>(
        Func<CancellationToken, IProgress<TProgress>?, Task<TResult>> operation,
        IProgress<TProgress>? progress = null,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool);
}

public sealed class BackgroundOperationRunner : IBackgroundOperationRunner
{
    public Task<TResult> RunAsync<TResult>(
        Func<CancellationToken, TResult> operation,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return RunAsync(
            token => Task.FromResult(operation(token)),
            cancellationToken,
            executionMode);
    }

    public Task<TResult> RunAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromCanceled<TResult>(cancellationToken);
        }

        return executionMode switch
        {
            BackgroundExecutionMode.ThreadPool => Task.Run(
                () => operation(cancellationToken),
                CancellationToken.None),
            BackgroundExecutionMode.StaThread => RunOnStaThreadAsync(operation, cancellationToken),
            _ => Task.FromException<TResult>(
                new ArgumentOutOfRangeException(nameof(executionMode), executionMode, "未知的后台执行模式。"))
        };
    }

    public Task<TResult> RunAsync<TResult, TProgress>(
        Func<CancellationToken, IProgress<TProgress>?, TResult> operation,
        IProgress<TProgress>? progress = null,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return RunAsync(
            token => Task.FromResult(operation(token, progress)),
            cancellationToken,
            executionMode);
    }

    public Task<TResult> RunAsync<TResult, TProgress>(
        Func<CancellationToken, IProgress<TProgress>?, Task<TResult>> operation,
        IProgress<TProgress>? progress = null,
        CancellationToken cancellationToken = default,
        BackgroundExecutionMode executionMode = BackgroundExecutionMode.ThreadPool)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return RunAsync(
            token => operation(token, progress),
            cancellationToken,
            executionMode);
    }

    private static Task<TResult> RunOnStaThreadAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<TResult>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var worker = new Thread(() =>
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                TResult result = operation(cancellationToken).GetAwaiter().GetResult();
                completion.TrySetResult(result);
            }
            catch (OperationCanceledException ex)
            {
                CancellationToken token = ex.CancellationToken.CanBeCanceled
                    ? ex.CancellationToken
                    : cancellationToken;
                completion.TrySetCanceled(token);
            }
            catch (Exception ex)
            {
                completion.TrySetException(ex);
            }
        })
        {
            IsBackground = true,
            Name = "DocumentTools.Background.STA"
        };

        worker.SetApartmentState(ApartmentState.STA);
        worker.Start();
        return completion.Task;
    }
}
