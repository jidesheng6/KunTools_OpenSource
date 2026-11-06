using System.Globalization;

namespace DocumentTools.QaPerf;

internal sealed class ScenarioVerification
{
    public string ScenarioId { get; set; } = string.Empty;

    public int OutputFiles { get; set; }

    public int PdfOutputs { get; set; }

    public long PdfPages { get; set; }

    public int PdfReadFailures { get; set; }

    public long OutputBytes { get; set; }

    public string Sample { get; set; } = string.Empty;
}

/// <summary>
/// Post-measurement checks: open the produced files (and the big inputs) with Aspose and count
/// pages. This runs after every round on purpose, so the library warm-up it causes cannot
/// contaminate the timings.
/// </summary>
internal static class Verification
{
    public static (List<ScenarioVerification> Items, List<string> Notes) Run(
        IReadOnlyList<RunMetrics> runs,
        IReadOnlyList<Scenario> scenarios)
    {
        var items = new List<ScenarioVerification>();
        var notes = new List<string>();

        foreach (IGrouping<string, RunMetrics> group in runs.GroupBy(run => run.ScenarioId, StringComparer.Ordinal))
        {
            RunMetrics last = group.OrderBy(run => run.Round).Last();
            var item = new ScenarioVerification
            {
                ScenarioId = last.ScenarioId,
                OutputFiles = last.OutputFiles.Count,
                OutputBytes = last.OutputBytes
            };

            foreach (string output in last.OutputFiles)
            {
                if (!output.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                item.PdfOutputs++;
                int? pages = TryCountPdfPages(output, out string? error);
                if (pages is null)
                {
                    item.PdfReadFailures++;
                    notes.Add(string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}: 读取 {1} 失败 - {2}",
                        last.ScenarioId,
                        Path.GetFileName(output),
                        error));
                    continue;
                }

                item.PdfPages += pages.Value;
                if (item.Sample.Length == 0)
                {
                    item.Sample = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} = {1} 页",
                        Path.GetFileName(output),
                        pages.Value);
                }
            }

            items.Add(item);
        }

        foreach (Scenario scenario in scenarios)
        {
            if (scenario.Inputs.Count != 1)
            {
                continue;
            }

            string input = scenario.Inputs[0];
            if (input.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                notes.Add(ProbeDocx(input, scenario.Id));
            }
            else if (input.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                int? pages = TryCountPdfPages(input, out string? error);
                notes.Add(pages is null
                    ? string.Format(CultureInfo.InvariantCulture, "{0}: 输入 {1} 读取失败 - {2}", scenario.Id, Path.GetFileName(input), error)
                    : string.Format(CultureInfo.InvariantCulture, "{0}: 输入 {1} 由 Aspose.Pdf 打开成功，共 {2} 页", scenario.Id, Path.GetFileName(input), pages.Value));
            }
        }

        return (items, notes);
    }

    private static string ProbeDocx(string path, string scenarioId)
    {
        try
        {
            // Aspose.Words.Document does not implement IDisposable (checked by the C# compiler, CS1674),
            // so there is nothing to release here; Aspose.Pdf.Document does and is disposed elsewhere.
            var document = new Aspose.Words.Document(path);
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}: 输入 {1} 由 Aspose.Words 打开成功，PageCount = {2}",
                scenarioId,
                Path.GetFileName(path),
                document.PageCount);
        }
        catch (Exception exception)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}: 输入 {1} 读取失败 - {2}",
                scenarioId,
                Path.GetFileName(path),
                exception.Message);
        }
    }

    internal static int? TryCountPdfPages(string path, out string? error)
    {
        try
        {
            using var document = new Aspose.Pdf.Document(path);
            error = null;
            return document.Pages.Count;
        }
        catch (Exception exception)
        {
            error = exception.GetType().Name + ": " + exception.Message;
            return null;
        }
    }
}

/// <summary>Aspose assembly versions actually loaded by this build (evidence for the report).</summary>
internal static class AsposeFacts
{
    public static string Describe()
    {
        var parts = new List<string>();
        Add<Aspose.Words.Document>("Words", parts);
        Add<Aspose.Pdf.Document>("Pdf", parts);
        Add<Aspose.Cells.Workbook>("Cells", parts);
        Add<Aspose.Slides.Presentation>("Slides", parts);
        return string.Join(" / ", parts);

        static void Add<T>(string label, List<string> target)
        {
            try
            {
                target.Add(label + " " + typeof(T).Assembly.GetName().Version);
            }
            catch (Exception)
            {
                target.Add(label + " (unavailable)");
            }
        }
    }
}
