using System.Globalization;

namespace DocumentTools.QaPerf;

internal sealed class LeakFinding
{
    public string ScenarioId { get; set; } = string.Empty;

    public int Rounds { get; set; }

    public List<double> ManagedMb { get; set; } = new();

    public List<double> WorkingSetMb { get; set; } = new();

    public List<double> RoundPeakMb { get; set; } = new();

    public double ManagedGrowthPct { get; set; }

    public double WorkingSetGrowthPct { get; set; }

    public double ManagedSlopeMbPerRound { get; set; }

    public double ManagedRSquared { get; set; }

    public string Verdict { get; set; } = string.Empty;

    public string Note { get; set; } = string.Empty;
}

/// <summary>
/// Growth sniffing over repeated rounds of the same scenario inside one process. Managed heap is
/// the honest signal (GC.GetTotalMemory(true) after the round); working set is reported as context
/// because the runtime is free to keep freed pages resident.
/// </summary>
internal static class LeakAnalyzer
{
    public const double GrowthThresholdPct = 30d;

    public const double SlopeThresholdMbPerRound = 2d;

    public const double RSquaredThreshold = 0.9d;

    public const int MinimumRounds = 3;

    private const double Mebibyte = 1024d * 1024d;

    public static List<LeakFinding> Analyze(IReadOnlyList<RunMetrics> runs)
    {
        var findings = new List<LeakFinding>();

        foreach (IGrouping<string, RunMetrics> group in runs.GroupBy(run => run.ScenarioId, StringComparer.Ordinal))
        {
            List<RunMetrics> ordered = group.OrderBy(run => run.Round).ToList();
            var finding = new LeakFinding
            {
                ScenarioId = ordered[0].ScenarioId,
                Rounds = ordered.Count
            };

            foreach (RunMetrics run in ordered)
            {
                finding.ManagedMb.Add(Math.Round(run.ManagedAfterCollectBytes / Mebibyte, 2));
                finding.WorkingSetMb.Add(Math.Round(run.EndWorkingSetBytes / Mebibyte, 2));
                finding.RoundPeakMb.Add(Math.Round(run.RoundPeakWorkingSetBytes / Mebibyte, 2));
            }

            finding.ManagedGrowthPct = GrowthPercent(finding.ManagedMb);
            finding.WorkingSetGrowthPct = GrowthPercent(finding.WorkingSetMb);
            (finding.ManagedSlopeMbPerRound, finding.ManagedRSquared) = LinearTrend(finding.ManagedMb);

            if (ordered.Count < MinimumRounds)
            {
                finding.Verdict = "轮次不足";
                finding.Note = string.Format(
                    CultureInfo.InvariantCulture,
                    "仅 {0} 轮；用 --rounds 5 复测才够看趋势",
                    ordered.Count);
            }
            else if (finding.ManagedGrowthPct > GrowthThresholdPct)
            {
                finding.Verdict = "疑似泄漏（增幅超阈值）";
                finding.Note = string.Format(
                    CultureInfo.InvariantCulture,
                    "托管堆 {0:0.00} -> {1:0.00} MB（+{2:0.0}%），单轮增量 {3:0.00} MB，R²={4:0.00}",
                    finding.ManagedMb[0],
                    finding.ManagedMb[^1],
                    finding.ManagedGrowthPct,
                    finding.ManagedSlopeMbPerRound,
                    finding.ManagedRSquared);
            }
            else if (finding.ManagedSlopeMbPerRound > SlopeThresholdMbPerRound && finding.ManagedRSquared > RSquaredThreshold)
            {
                finding.Verdict = "疑似泄漏（线性趋势）";
                finding.Note = string.Format(
                    CultureInfo.InvariantCulture,
                    "托管堆线性上升 {0:0.00} MB/轮，R²={1:0.00}（增幅 {2:0.0}% 未过 30% 但仍单调）",
                    finding.ManagedSlopeMbPerRound,
                    finding.ManagedRSquared,
                    finding.ManagedGrowthPct);
            }
            else
            {
                finding.Verdict = "未见单调上升";
                finding.Note = string.Format(
                    CultureInfo.InvariantCulture,
                    "托管堆 {0:0.00} -> {1:0.00} MB（{2:+0.0;-0.0;0}%），斜率 {3:0.00} MB/轮，R²={4:0.00}",
                    finding.ManagedMb[0],
                    finding.ManagedMb[^1],
                    finding.ManagedGrowthPct,
                    finding.ManagedSlopeMbPerRound,
                    finding.ManagedRSquared);
            }

            findings.Add(finding);
        }

        return findings;
    }

    private static double GrowthPercent(List<double> series)
    {
        if (series.Count < 2 || series[0] <= 0)
        {
            return 0;
        }

        return (series[^1] - series[0]) / series[0] * 100d;
    }

    /// <summary>Least squares slope over x = 1..n plus the coefficient of determination.</summary>
    private static (double Slope, double RSquared) LinearTrend(List<double> series)
    {
        int n = series.Count;
        if (n < 2)
        {
            return (0, 0);
        }

        double sumX = 0;
        double sumY = 0;
        for (int i = 0; i < n; i++)
        {
            sumX += i + 1;
            sumY += series[i];
        }

        double meanX = sumX / n;
        double meanY = sumY / n;
        double covariance = 0;
        double varianceX = 0;
        for (int i = 0; i < n; i++)
        {
            double dx = (i + 1) - meanX;
            covariance += dx * (series[i] - meanY);
            varianceX += dx * dx;
        }

        if (varianceX == 0)
        {
            return (0, 0);
        }

        double slope = covariance / varianceX;
        double intercept = meanY - (slope * meanX);

        double residual = 0;
        double total = 0;
        for (int i = 0; i < n; i++)
        {
            double predicted = intercept + (slope * (i + 1));
            residual += Math.Pow(series[i] - predicted, 2);
            total += Math.Pow(series[i] - meanY, 2);
        }

        double rSquared = total == 0 ? 0 : 1 - (residual / total);
        return (Math.Round(slope, 3), Math.Round(rSquared, 3));
    }
}
