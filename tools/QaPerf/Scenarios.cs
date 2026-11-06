using System.Diagnostics;
using System.Globalization;
using DocumentTools.Core.Operations;

namespace DocumentTools.QaPerf;

/// <summary>One measured workload: a feature id plus the concrete input list and options.</summary>
internal sealed class Scenario
{
    public string Id { get; init; } = string.Empty;

    public string Family { get; init; } = string.Empty;

    public string FeatureId { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public IReadOnlyList<string> Inputs { get; init; } = Array.Empty<string>();

    public Dictionary<string, string>? Options { get; init; }

    /// <summary>Human readable options summary for the report.</summary>
    public string OptionsDescription { get; init; } = "默认（不传 Options）";

    public string OutputDescription { get; init; } = string.Empty;

    /// <summary>Number of files the operation should produce; 0 means "one per input" (the batch rule).</summary>
    public int ExpectedOutputs { get; init; }
}

internal static class ScenarioSet
{
    public const string FamilyWordPdf = "wordpdf";
    public const string FamilyImage = "image";
    public const string FamilyLarge = "large";
    public const string FamilyMerge = "merge";

    private static readonly string[] AllFamilies = { FamilyWordPdf, FamilyImage, FamilyLarge, FamilyMerge };

    /// <summary>
    /// Turns a --scenario value into a selection: whole families ("all", "wordpdf", "image", "large",
    /// "merge") or a single scenario id ("wordpdf-50"), so one scenario can be measured cold.
    /// </summary>
    public static IReadOnlyList<string> ResolveSelection(string request)
    {
        if (string.Equals(request, "all", StringComparison.OrdinalIgnoreCase))
        {
            return AllFamilies;
        }

        foreach (string family in AllFamilies)
        {
            if (string.Equals(request, family, StringComparison.OrdinalIgnoreCase))
            {
                return new[] { family };
            }
        }

        if (FamilyOf(request) is not null)
        {
            return new[] { request };
        }

        throw new FormatException(
            $"未知场景 \"{request}\"；可用值：all, {string.Join(", ", AllFamilies)}，" +
            "或单场景 id（wordpdf-10 / wordpdf-50 / image-50 / large-word-30p / large-pdf-200p / merge-2 / merge-4 / merge-20）。");
    }

    /// <summary>Families whose fixtures the selection needs.</summary>
    public static IReadOnlyList<string> FixtureFamilies(IReadOnlyList<string> selection)
        => AllFamilies.Where(family => selection.Contains(family) || selection.Any(id => FamilyOf(id) == family))
            .ToArray();

    private static string? FamilyOf(string scenarioId) => scenarioId switch
    {
        "wordpdf-10" or "wordpdf-50" => FamilyWordPdf,
        "image-50" => FamilyImage,
        "large-word-30p" or "large-pdf-200p" => FamilyLarge,
        "merge-2" or "merge-4" or "merge-20" => FamilyMerge,
        _ => null
    };

    /// <summary>Builds every scenario of the requested selection; fixtures must already exist.</summary>
    public static IReadOnlyList<Scenario> Build(RepoLayout layout, IReadOnlyList<string> selection)
    {
        var scenarios = new List<Scenario>();

        bool Want(string id) => selection.Contains(id) || (FamilyOf(id) is string family && selection.Contains(family));

        if (Want("wordpdf-10") || Want("wordpdf-50"))
        {
            string[] batch = Directory.GetFiles(layout.WordBatchRoot, "batch_*.docx")
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            if (Want("wordpdf-10"))
            {
                scenarios.Add(new Scenario
                {
                    Id = "wordpdf-10",
                    Family = FamilyWordPdf,
                    FeatureId = "word.pdf",
                    Title = "批量 word.pdf：10 个 docx（各 2 页）",
                    Inputs = batch.Take(10).ToArray(),
                    Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["compliance"] = "Pdf17" },
                    OptionsDescription = "compliance=Pdf17（其余走默认：embedFonts=false, colorMode=Normal, msWordVersion=Word2016）",
                    OutputDescription = "每个 docx 输出同名 .pdf"
                });
            }

            if (Want("wordpdf-50"))
            {
                scenarios.Add(new Scenario
                {
                    Id = "wordpdf-50",
                    Family = FamilyWordPdf,
                    FeatureId = "word.pdf",
                    Title = "批量 word.pdf：50 个 docx（各 2 页）",
                    Inputs = batch.Take(50).ToArray(),
                    Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["compliance"] = "Pdf17" },
                    OptionsDescription = "compliance=Pdf17（与 wordpdf-10 完全一致，便于看线性度）",
                    OutputDescription = "每个 docx 输出同名 .pdf"
                });
            }
        }

        if (Want("image-50"))
        {
            string[] images = Directory.GetFiles(layout.ImageRoot, "px_*.png")
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            scenarios.Add(new Scenario
            {
                Id = "image-50",
                Family = FamilyImage,
                FeatureId = "image.compress",
                Title = "批量 image.compress：50 张 1600x1200 PNG",
                Inputs = images,
                Options = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["format"] = "PNG",
                    ["pngLevel"] = "3",
                    ["pngColorType"] = "Truecolor"
                },
                OptionsDescription = "format=PNG, pngLevel=3, pngColorType=Truecolor（与操作默认值一致）",
                OutputDescription = "{原名}_out.png"
            });
        }

        if (Want("large-word-30p"))
        {
            scenarios.Add(new Scenario
            {
                Id = "large-word-30p",
                Family = FamilyLarge,
                FeatureId = "word.pdf",
                Title = "单文件大件 word.pdf：1 个 30 页 docx",
                Inputs = new[] { Path.Combine(layout.LargeRoot, "large_30p.docx") },
                Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["compliance"] = "Pdf17" },
                OptionsDescription = "compliance=Pdf17（与批量场景一致）",
                OutputDescription = "large_30p.pdf"
            });
        }

        if (Want("large-pdf-200p"))
        {
            scenarios.Add(new Scenario
            {
                Id = "large-pdf-200p",
                Family = FamilyLarge,
                FeatureId = "pdf.compress",
                Title = "单文件大件 pdf.compress：1 个 200 页 PDF",
                Inputs = new[] { Path.Combine(layout.PdfRoot, "pages200.pdf") },
                Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["quality"] = "75" },
                OptionsDescription = "quality=75（其余走默认：webOptimize=false, maxDpi 未设置）",
                OutputDescription = "{原名}_压缩.pdf"
            });
        }

        if (Want("merge-2") || Want("merge-4") || Want("merge-20"))
        {
            string[] parts = Directory.GetFiles(Path.Combine(layout.PdfRoot, "merge20"), "part_*.pdf")
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();

            // The evaluation build caps collections at 4 elements, so the 20-input merge cannot run
            // without a license. 2 and 4 inputs give the per-input merge cost inside that cap.
            void AddMerge(string id, int count, string title, string pageExpectation)
            {
                if (!Want(id))
                {
                    return;
                }

                scenarios.Add(new Scenario
                {
                    Id = id,
                    Family = FamilyMerge,
                    FeatureId = "pdf.merge",
                    Title = title,
                    Inputs = parts.Take(count).ToArray(),
                    Options = new Dictionary<string, string>(StringComparer.Ordinal) { ["outputName"] = "merged.pdf" },
                    OptionsDescription = "outputName=merged.pdf（默认有序合并）",
                    OutputDescription = "merged.pdf（期望 " + pageExpectation + "）",
                    ExpectedOutputs = 1
                });
            }

            AddMerge("merge-2", 2, "pdf.merge：2 个 2 页 PDF", "4 页");
            AddMerge("merge-4", 4, "pdf.merge：4 个 2 页 PDF", "8 页");
            AddMerge("merge-20", 20, "pdf.merge：20 个 2 页 PDF 合并成 1 个 40 页 PDF", "40 页");
        }

        return scenarios;
    }
}

/// <summary>Runs one scenario round and records time, memory, output volume and failures.</summary>
internal static class ScenarioRunner
{
    public static RunMetrics Run(IDocumentOperation operation, Scenario scenario, RepoLayout layout, int round, bool coldStart)
    {
        string outputDirectory = layout.RoundDirectory(scenario.Id, round);
        if (Directory.Exists(outputDirectory))
        {
            Directory.Delete(outputDirectory, recursive: true);
        }

        Directory.CreateDirectory(outputDirectory);

        var request = new DocumentOperationRequest
        {
            InputFiles = scenario.Inputs,
            OutputDirectory = outputDirectory,
            Options = scenario.Options
        };

        using Process process = Process.GetCurrentProcess();
        process.Refresh();
        long startWorkingSet = process.WorkingSet64;
        long managedBefore = GC.GetTotalMemory(forceFullCollection: false);

        DocumentOperationResult result;
        double totalMs;
        long roundPeakWorkingSet;
        long roundPeakPrivate;
        int samples;

        using (var sampler = new MemorySampler())
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                result = operation.ExecuteAsync(request, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                result = DocumentOperationResult.Fail(exception.GetType().Name + ": " + exception.Message);
            }

            stopwatch.Stop();
            sampler.Stop();
            totalMs = stopwatch.Elapsed.TotalMilliseconds;
            roundPeakWorkingSet = sampler.PeakWorkingSetBytes;
            roundPeakPrivate = sampler.PeakPrivateBytes;
            samples = sampler.Samples;
        }

        var outputs = new List<string>();
        long outputBytes = 0;
        foreach (string output in result.OutputFiles)
        {
            if (!File.Exists(output))
            {
                continue;
            }

            outputs.Add(output);
            outputBytes += new FileInfo(output).Length;
        }

        // Working set first, GC collection last: a forced collection would shrink the numbers.
        process.Refresh();
        long endWorkingSet = process.WorkingSet64;
        long peakWorkingSet = process.PeakWorkingSet64;
        long managedAfter = GC.GetTotalMemory(forceFullCollection: true);

        // A merge takes N inputs and produces one file, so "inputs minus outputs" would report
        // phantom failures; every scenario states how many files it must produce.
        int expectedOutputs = scenario.ExpectedOutputs > 0 ? scenario.ExpectedOutputs : scenario.Inputs.Count;
        int failures = Math.Max(0, expectedOutputs - outputs.Count);
        var errors = new List<string>();
        if (!result.Success)
        {
            errors.Add(result.Message);
        }

        // The evaluation build refuses some collections outright; that is an environment limit,
        // not a defect of the operation, so it is recorded separately instead of as a failure.
        string? blockedReason = null;
        if (!result.Success && IsEvaluationLimit(result.Message))
        {
            blockedReason = result.Message;
        }

        if (failures > 0)
        {
            errors.Add(string.Format(
                CultureInfo.InvariantCulture,
                "{0} 个输出文件缺失（期望 {1} 个，实际 {2} 个）",
                failures,
                expectedOutputs,
                outputs.Count));
        }

        return new RunMetrics
        {
            ScenarioId = scenario.Id,
            Family = scenario.Family,
            FeatureId = scenario.FeatureId,
            Title = scenario.Title,
            Round = round,
            ColdStart = coldStart,
            FileCount = scenario.Inputs.Count,
            TotalMs = Math.Round(totalMs, 1),
            MeanMsPerFile = scenario.Inputs.Count == 0 ? 0 : Math.Round(totalMs / scenario.Inputs.Count, 1),
            PeakWorkingSetBytes = peakWorkingSet,
            RoundPeakWorkingSetBytes = roundPeakWorkingSet,
            RoundPeakPrivateBytes = roundPeakPrivate,
            StartWorkingSetBytes = startWorkingSet,
            EndWorkingSetBytes = endWorkingSet,
            ManagedBeforeBytes = managedBefore,
            ManagedAfterCollectBytes = managedAfter,
            OutputBytes = outputBytes,
            OutputFiles = outputs,
            Failures = failures,
            Blocked = blockedReason is not null,
            BlockedReason = blockedReason,
            Errors = errors,
            OutputDirectory = outputDirectory,
            SamplerSamples = samples
        };
    }

    private static bool IsEvaluationLimit(string message)
        => message.Contains("evaluation mode", StringComparison.OrdinalIgnoreCase)
            || message.Contains("evaluation only", StringComparison.OrdinalIgnoreCase)
            || message.Contains("evaluation version", StringComparison.OrdinalIgnoreCase)
            // The Core layer translates the same Aspose limit into Chinese before returning it:
            // "当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。"
            || message.Contains("评估模式", StringComparison.Ordinal)
            || message.Contains("集合超过 4 个元素", StringComparison.Ordinal);
}
