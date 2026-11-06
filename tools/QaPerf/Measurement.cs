using System.Diagnostics;
using System.Globalization;
using System.Text.Json.Serialization;

namespace DocumentTools.QaPerf;

/// <summary>One scenario execution (a single round). All numbers are measured, never derived.</summary>
internal sealed class RunMetrics
{
    private const double Mebibyte = 1024d * 1024d;

    public string ScenarioId { get; set; } = string.Empty;

    public string Family { get; set; } = string.Empty;

    public string FeatureId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public int Round { get; set; }

    /// <summary>True only for the very first operation execution in this process.</summary>
    public bool ColdStart { get; set; }

    public int FileCount { get; set; }

    public double TotalMs { get; set; }

    /// <summary>Amortised per-file time (total / input count).</summary>
    public double MeanMsPerFile { get; set; }

    /// <summary>Process.GetCurrentProcess().PeakWorkingSet64 right after the round.</summary>
    public long PeakWorkingSetBytes { get; set; }

    /// <summary>Highest working set sampled while the round was running.</summary>
    public long RoundPeakWorkingSetBytes { get; set; }

    public long RoundPeakPrivateBytes { get; set; }

    public long StartWorkingSetBytes { get; set; }

    public long EndWorkingSetBytes { get; set; }

    public long ManagedBeforeBytes { get; set; }

    /// <summary>GC.GetTotalMemory(forceFullCollection: true) taken after the round.</summary>
    public long ManagedAfterCollectBytes { get; set; }

    public long OutputBytes { get; set; }

    public List<string> OutputFiles { get; set; } = new();

    public int Failures { get; set; }

    public List<string> Errors { get; set; } = new();

    public string OutputDirectory { get; set; } = string.Empty;

    public int SamplerSamples { get; set; }

    /// <summary>
    /// True when the operation refused to run because of the Aspose evaluation limit rather than a
    /// defect. Blocked runs are reported, quoted verbatim, and kept out of the exit code.
    /// </summary>
    public bool Blocked { get; set; }

    public string? BlockedReason { get; set; }

    [JsonIgnore]
    public double PeakWorkingSetMb => PeakWorkingSetBytes / Mebibyte;

    [JsonIgnore]
    public double RoundPeakWorkingSetMb => RoundPeakWorkingSetBytes / Mebibyte;

    [JsonIgnore]
    public double ManagedAfterCollectMb => ManagedAfterCollectBytes / Mebibyte;

    [JsonIgnore]
    public double OutputMb => OutputBytes / Mebibyte;

    public static string ConsoleHeader() => string.Format(
        CultureInfo.InvariantCulture,
        "{0,-15} {1,5} {2,-5} {3,6} {4,10} {5,9} {6,11} {7,11} {8,9} {9,8} {10,5}",
        "scenario", "round", "cold", "files", "total_ms", "mean_ms", "peakWS_MB", "roundWS_MB", "gc_MB", "out_MB", "fail");

    public string FormatConsoleRow() => string.Format(
        CultureInfo.InvariantCulture,
        "{0,-15} {1,5} {2,-5} {3,6} {4,10:0.0} {5,9:0.0} {6,11:0.0} {7,11:0.0} {8,9:0.0} {9,8:0.00} {10,5}",
        ScenarioId,
        Round,
        ColdStart ? "cold" : "warm",
        FileCount,
        TotalMs,
        MeanMsPerFile,
        PeakWorkingSetMb,
        RoundPeakWorkingSetMb,
        ManagedAfterCollectMb,
        OutputMb,
        Failures);
}

/// <summary>
/// Background working-set sampler. `Process.PeakWorkingSet64` only ever grows and includes the
/// harness itself, so the per-round peak has to be sampled while the operation runs.
/// </summary>
internal sealed class MemorySampler : IDisposable
{
    private readonly Process _process;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;
    private long _peakWorkingSet;
    private long _peakPrivate;
    private int _samples;
    private int _disposed;

    public MemorySampler(int intervalMs = 25)
    {
        _process = Process.GetCurrentProcess();
        _loop = Task.Run(() => LoopAsync(intervalMs));
    }

    public long PeakWorkingSetBytes => Interlocked.Read(ref _peakWorkingSet);

    public long PeakPrivateBytes => Interlocked.Read(ref _peakPrivate);

    public int Samples => Volatile.Read(ref _samples);

    public void Stop()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _cts.Cancel();
        try
        {
            _loop.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException)
        {
            // Sampler teardown must never fail a measurement.
        }

        _cts.Dispose();
        _process.Dispose();
    }

    public void Dispose() => Stop();

    private async Task LoopAsync(int intervalMs)
    {
        try
        {
            while (!_cts.IsCancellationRequested)
            {
                Sample();
                await Task.Delay(intervalMs, _cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on Stop().
        }
        catch (ObjectDisposedException)
        {
            // Expected when the round finished while a sample was in flight.
        }
        finally
        {
            Sample();
        }
    }

    private void Sample()
    {
        try
        {
            _process.Refresh();
            Max(ref _peakWorkingSet, _process.WorkingSet64);
            Max(ref _peakPrivate, _process.PrivateMemorySize64);
            Interlocked.Increment(ref _samples);
        }
        catch (Exception)
        {
            // A sample is best-effort: the process may be tearing down.
        }
    }

    private static void Max(ref long target, long value)
    {
        long current = Interlocked.Read(ref target);
        while (value > current)
        {
            long previous = Interlocked.CompareExchange(ref target, value, current);
            if (previous == current)
            {
                return;
            }

            current = previous;
        }
    }
}
