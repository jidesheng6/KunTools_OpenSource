namespace DocumentTools.QaPerf;

/// <summary>Repository-relative folders used by the harness (fixtures in, artifacts out).</summary>
internal sealed class RepoLayout
{
    private RepoLayout(string repoRoot, string outRoot)
    {
        RepoRoot = repoRoot;
        OutRoot = outRoot;
    }

    public string RepoRoot { get; }

    /// <summary>Where scenario outputs, per-run sessions and verifications are written.</summary>
    public string OutRoot { get; }

    public string PerfRoot => Path.Combine(RepoRoot, "testdata", "perf");

    public string WordBatchRoot => Path.Combine(PerfRoot, "word", "batch");

    public string LargeRoot => Path.Combine(PerfRoot, "large");

    public string ImageRoot => Path.Combine(PerfRoot, "image");

    public string PdfRoot => Path.Combine(PerfRoot, "pdf");

    public string SessionsRoot => Path.Combine(OutRoot, "sessions");

    public string ScenarioDirectory(string scenarioId) => Path.Combine(OutRoot, "out", scenarioId);

    public string RoundDirectory(string scenarioId, int round)
        => Path.Combine(ScenarioDirectory(scenarioId), "round-" + round.ToString(System.Globalization.CultureInfo.InvariantCulture));

    public static RepoLayout Create(string? outDirectory)
    {
        string repoRoot = FindRepoRoot();
        string outRoot = string.IsNullOrWhiteSpace(outDirectory)
            ? Path.Combine(repoRoot, "artifacts", "qa-perf")
            : Path.GetFullPath(outDirectory, Environment.CurrentDirectory);
        return new RepoLayout(repoRoot, outRoot);
    }

    /// <summary>Walks up from the binary and the current directory looking for the solution file.</summary>
    public static string FindRepoRoot()
    {
        foreach (string start in new[] { AppContext.BaseDirectory, Environment.CurrentDirectory })
        {
            DirectoryInfo? directory = new(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "DocumentTools.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException(
            "未找到 DocumentTools.sln；请在仓库内运行 QaPerf，或用 --out 明确指定输出目录。");
    }

    public void EnsureOutRoot()
    {
        Directory.CreateDirectory(OutRoot);
        Directory.CreateDirectory(SessionsRoot);
    }
}
