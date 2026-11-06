using System.Globalization;
using System.Text;
using DocumentTools.Core.DependencyInjection;
using DocumentTools.Core.Licensing;
using DocumentTools.Core.Operations;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.QaPerf;

/// <summary>
/// Batch conversion / memory baseline for DocumentTools.Core. Resolves the real operations from the
/// production DI graph and measures them; nothing here re-implements a conversion.
/// </summary>
internal static class Program
{
    private static readonly string[] CanonicalOrder =
    {
        "wordpdf-10", "wordpdf-50", "image-50", "large-word-30p", "large-pdf-200p", "merge-2", "merge-4", "merge-20"
    };

    private static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        CliOptions cli;
        try
        {
            cli = CliOptions.Parse(args);
        }
        catch (FormatException exception)
        {
            Console.Error.WriteLine("bad arguments: " + exception.Message);
            Console.Error.WriteLine(CliOptions.Usage);
            return 2;
        }

        if (cli.ShowHelp)
        {
            Console.WriteLine(CliOptions.Usage);
            return 0;
        }

        if (!string.IsNullOrWhiteSpace(cli.ProbePath))
        {
            return Probe(cli.ProbePath);
        }

        RepoLayout layout;
        IReadOnlyList<string> selection;
        try
        {
            layout = RepoLayout.Create(cli.OutDirectory);
            selection = ScenarioSet.ResolveSelection(cli.Scenario);
        }
        catch (Exception exception) when (exception is DirectoryNotFoundException or FormatException)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }

        layout.EnsureOutRoot();
        if (cli.ResetSessions)
        {
            SessionStore.Reset(layout);
            Console.WriteLine("sessions   : cleared (--reset)");
        }

        if (cli.ReportOnly)
        {
            IReadOnlyList<Session> recorded = SessionStore.LoadAll(layout);
            if (recorded.Count == 0)
            {
                Console.Error.WriteLine("report-only: no recorded session under " + layout.SessionsRoot);
                return 2;
            }

            string target = cli.ReportPath is null
                ? Path.Combine(layout.RepoRoot, "docs", "qa-report-perf.md")
                : Path.GetFullPath(cli.ReportPath, Environment.CurrentDirectory);
            ReportWriter.Write(target, recorded, layout);
            Console.WriteLine("sessions   : " + recorded.Count + " replayed (no measurement)");
            Console.WriteLine("report     : " + target);
            return 0;
        }

        HostInfo host = HostInfo.Capture();
        AsposeLicenseStatus license = AsposeLicenseLoader.TryLoad();

        Console.WriteLine("== QaPerf ==");
        Console.WriteLine("time       : " + DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture));
        Console.WriteLine("repo       : " + layout.RepoRoot);
        Console.WriteLine("artifacts  : " + layout.OutRoot);
        Console.WriteLine("scenario   : " + cli.Scenario + " (rounds=" + cli.Rounds.ToString(CultureInfo.InvariantCulture) + ")");
        Console.WriteLine("license    : " + (license.IsLicensed ? "licensed: " + license.LicensePath : "evaluation (no .lic found)"));
        Console.WriteLine(host.Describe());

        var session = new Session
        {
            StartedAt = DateTimeOffset.Now,
            CommandLine = Environment.CommandLine,
            ScenarioRequest = cli.Scenario,
            RequestedRounds = cli.Rounds,
            Host = host,
            Licensed = license.IsLicensed,
            LicensePath = license.LicensePath
        };

        IReadOnlyList<Scenario> scenarios;
        try
        {
            session.Fixtures = FixtureSetup.Ensure(layout, ScenarioSet.FixtureFamilies(selection));
            scenarios = ScenarioSet.Build(layout, selection);
        }
        catch (FixtureMissingException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 2;
        }

        foreach (string note in session.Fixtures.Notes)
        {
            Console.WriteLine("fixture    : " + note);
        }

        foreach (string warning in session.Fixtures.Warnings)
        {
            Console.WriteLine("WARN       : " + warning);
        }

        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "fixtures   : {0} groups, {1} files, {2:0.0} MB total, max file {3:0.0} KB",
            session.Fixtures.Groups.Count,
            session.Fixtures.Groups.Sum(group => group.Count),
            session.Fixtures.TotalBytes / (1024d * 1024d),
            session.Fixtures.MaxFileBytes / 1024d));
        Console.WriteLine();

        using ServiceProvider provider = new ServiceCollection().AddDocumentToolsCore().BuildServiceProvider();
        Dictionary<string, IDocumentOperation> operations = provider
            .GetServices<IDocumentOperation>()
            .ToDictionary(operation => operation.FeatureId, StringComparer.Ordinal);

        Console.WriteLine("operations : " + string.Join(", ", operations.Keys.OrderBy(key => key, StringComparer.Ordinal)));
        Console.WriteLine();
        Console.WriteLine(RunMetrics.ConsoleHeader());

        bool asposeWarm = false;
        foreach (Scenario scenario in scenarios)
        {
            if (!operations.TryGetValue(scenario.FeatureId, out IDocumentOperation? operation))
            {
                Console.Error.WriteLine("no IDocumentOperation registered for feature '" + scenario.FeatureId + "'");
                return 2;
            }

            for (int round = 1; round <= cli.Rounds; round++)
            {
                RunMetrics run = ScenarioRunner.Run(operation, scenario, layout, round, coldStart: round == 1 && !asposeWarm);
                asposeWarm = true;
                session.Runs.Add(run);
                Console.WriteLine(run.FormatConsoleRow());
                foreach (string error in run.Errors)
                {
                    Console.WriteLine("           ! " + error);
                }
            }
        }

        Console.WriteLine();

        (session.Verifications, session.VerificationNotes) = Verification.Run(session.Runs, scenarios);
        session.Leaks = LeakAnalyzer.Analyze(session.Runs);
        session.DisposeSites = DisposeAudit.Scan(layout.RepoRoot);
        session.AsposeFacts = AsposeFacts.Describe();

        SessionStore.Save(layout, session);
        IReadOnlyList<Session> sessions = SessionStore.LoadAll(layout);

        string reportPath = cli.ReportPath is null
            ? Path.Combine(layout.RepoRoot, "docs", "qa-report-perf.md")
            : Path.GetFullPath(cli.ReportPath, Environment.CurrentDirectory);

        try
        {
            ReportWriter.Write(reportPath, sessions, layout);
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException)
        {
            Console.Error.WriteLine("report failed: " + exception.Message);
            return 2;
        }

        PrintSummary(session, sessions, reportPath);
        return session.Runs.Count(run => run.Failures > 0 && !run.Blocked) == 0 ? 0 : 1;
    }

    /// <summary>
    /// Prints layout facts for one fixture file. The page counts quoted in the report must be
    /// reproducible from a single command, without replaying a whole scenario.
    /// </summary>
    private static int Probe(string path)
    {
        string full = Path.GetFullPath(path, Environment.CurrentDirectory);
        if (!File.Exists(full))
        {
            Console.Error.WriteLine("probe: file not found: " + full);
            return 2;
        }

        FileInfo info = new(full);
        Console.WriteLine("probe      : " + full);
        Console.WriteLine("bytes      : " + info.Length.ToString(CultureInfo.InvariantCulture));

        string extension = info.Extension.ToLowerInvariant();
        if (extension == ".docx")
        {
            // Aspose.Words.Document does not implement IDisposable (CS1674), so there is nothing to release.
            var document = new Aspose.Words.Document(full);
            int pageBreaks = document.GetText().Count(character => character is '\f');
            Console.WriteLine("engine     : Aspose.Words " + typeof(Aspose.Words.Document).Assembly.GetName().Version);
            Console.WriteLine("sections   : " + document.Sections.Count.ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("paragraphs : " + document.GetChildNodes(Aspose.Words.NodeType.Paragraph, true).Count.ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("pagebreaks : " + pageBreaks.ToString(CultureInfo.InvariantCulture) + " (explicit \\f breaks seen by the engine)");
            Console.WriteLine("pages      : " + document.PageCount.ToString(CultureInfo.InvariantCulture));
            return 0;
        }

        if (extension == ".pdf")
        {
            using var document = new Aspose.Pdf.Document(full);
            Console.WriteLine("engine     : Aspose.Pdf " + typeof(Aspose.Pdf.Document).Assembly.GetName().Version);
            Console.WriteLine("pages      : " + document.Pages.Count.ToString(CultureInfo.InvariantCulture));
            return 0;
        }

        Console.Error.WriteLine("probe: unsupported extension '" + extension + "' (expected .docx or .pdf)");
        return 2;
    }

    private static void PrintSummary(Session session, IReadOnlyList<Session> sessions, string reportPath)
    {
        long pdfPages = session.Verifications.Sum(item => item.PdfPages);
        int pdfFailures = session.Verifications.Sum(item => item.PdfReadFailures);
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            "verify     : {0} output pdf(s), {1} page(s), {2} read failure(s)",
            session.Verifications.Sum(item => item.PdfOutputs),
            pdfPages,
            pdfFailures));

        foreach (LeakFinding leak in session.Leaks.OrderBy(item => Index(item.ScenarioId)).ThenBy(item => item.ScenarioId, StringComparer.Ordinal))
        {
            Console.WriteLine("leak       : " + leak.ScenarioId.PadRight(15) + leak.Verdict);
        }

        Console.WriteLine("audit      : " + DisposeAudit.DescribeCounts(session.DisposeSites));
        Console.WriteLine("sessions   : " + sessions.Count + " recorded (" + session.FileName + " is this run)");
        Console.WriteLine("report     : " + reportPath);

        foreach (RunMetrics blocked in session.Runs.Where(run => run.Blocked).OrderBy(run => Index(run.ScenarioId)))
        {
            Console.WriteLine("blocked    : " + blocked.ScenarioId.PadRight(15) + blocked.BlockedReason);
        }

        int failed = session.Runs.Count(run => run.Failures > 0 && !run.Blocked);
        Console.WriteLine(failed == 0
            ? "result     : ok - every scenario produced all outputs (evaluation-blocked runs are listed above)"
            : "result     : " + failed.ToString(CultureInfo.InvariantCulture) + " run(s) with failed outputs (see report)");
        Console.WriteLine("exit       : 0 = all outputs produced, 1 = a scenario failed, 2 = bad arguments or fixtures");
    }

    private static int Index(string scenarioId)
    {
        int index = Array.IndexOf(CanonicalOrder, scenarioId);
        return index < 0 ? CanonicalOrder.Length : index;
    }
}
