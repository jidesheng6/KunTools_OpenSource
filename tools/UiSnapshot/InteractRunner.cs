using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.Core.Features;

namespace DocumentTools.UiSnapshot;

/// <summary>一次交互态快照的结果（父进程视角）。</summary>
internal sealed class InteractResult
{
    public InteractResult(string featureId)
    {
        FeatureId = featureId;
    }

    public string FeatureId { get; }

    /// <summary>ok / hang / fatal / no-button / no-status / unknown-feature / crashed。</summary>
    public string Outcome { get; set; } = "unknown";

    public int ExitCode { get; set; }

    /// <summary>true = 子进程超时被杀，通常意味着点击弹出了模态对话框。</summary>
    public bool TimedOut { get; set; }

    public string ScreenshotFile { get; set; } = string.Empty;

    public int StatusLines { get; set; }

    public List<string> Trace { get; } = new();

    public string Output { get; set; } = string.Empty;
}

/// <summary>
/// 交互态快照。每个页面在**独立子进程**里点击：一旦按钮弹出模态框，被卡住的是子进程，
/// 父进程按超时判定为 hang 并记为缺陷证据，不会连累其余页面和主快照。
/// </summary>
internal static class InteractRunner
{
    private const int ChildTimeoutMilliseconds = 25_000;

    /// <summary>单步看门狗：任一步骤超过这么久没有走完，就判 step-timeout 并自杀，绝不让子进程挂死到锁 dll。</summary>
    private const int StepWatchdogMilliseconds = 10_000;

    /// <summary>子进程入口：点一次按钮 → 截图 → 写 sidecar 明细 → 返回退出码。</summary>
    internal static int RunChild(
        string featureId,
        string outputDirectory,
        IFeatureViewFactory factory,
        IFeatureCatalog catalog,
        QaDialogService dialogs)
    {
        List<FeatureDescriptor> all = catalog.All.ToList();
        int index = all.FindIndex(f => string.Equals(f.Id, featureId, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
        {
            Console.WriteLine("INTERACT|" + featureId + "|unknown-feature|status=0|issues=0|-");
            return 2;
        }

        FeatureDescriptor feature = all[index];
        var trace = new List<string>();

        // 看门狗与交互脚本同生命周期：脚本一旦卡死，这里直接落盘并退出，避免留下锁 bin 的僵尸进程。
        using var watchdog = new System.Threading.Timer(
            _ =>
            {
                try
                {
                    File.AppendAllText(
                        Path.Combine(outputDirectory, featureId + "@interact.txt"),
                        "outcome: step-timeout" + Environment.NewLine
                        + "step: " + (trace.Count > 0 ? trace[^1] : "-") + Environment.NewLine,
                        new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                }
                catch (IOException)
                {
                    // 落盘失败也要退出，父进程按超时处理。
                }

                Environment.Exit(7);
            },
            null,
            StepWatchdogMilliseconds,
            System.Threading.Timeout.Infinite);

        PageSnapshot page = SnapshotRunner.Capture(
            feature,
            index + 1,
            factory,
            outputDirectory,
            featureId + "@interact.png",
            96,
            view =>
            {
                try
                {
                    InteractionScripts.Run(featureId, view, trace, dialogs);
                }
                catch (Exception ex)
                {
                    // 脚本自身出错时仍保留已执行步骤，避免 trace 全空、结论无从追溯。
                    trace.Add("EXCEPTION " + ex.GetType().Name + ": " + ex.Message);
                }
            });

        watchdog.Dispose();

        int statusLines = trace.Count(line => line.StartsWith("status + ", StringComparison.Ordinal));
        bool missedControl = trace.Any(line => line.StartsWith("MISS ", StringComparison.Ordinal));
        bool foundHint = trace.Any(line => line.StartsWith("present ", StringComparison.Ordinal));
        bool scriptError = trace.Any(line => line.StartsWith("EXCEPTION ", StringComparison.Ordinal));

        int exitCode;
        string outcome;
        if (page.FatalError is not null)
        {
            exitCode = 3;
            outcome = "fatal";
        }
        else if (scriptError)
        {
            exitCode = 6;
            outcome = "exception";
        }
        else if (missedControl)
        {
            exitCode = 4;
            outcome = "no-button";
        }
        else if (statusLines == 0)
        {
            exitCode = 5;
            outcome = "no-status";
        }
        else
        {
            exitCode = 0;
            outcome = "ok";
        }

        WriteSidecar(outputDirectory, featureId, page, trace, outcome);

        Console.WriteLine(
            "INTERACT|" + featureId + "|" + outcome
            + "|status=" + statusLines
            + "|hint=" + (foundHint ? "yes" : "no")
            + "|issues=" + page.Issues.Count
            + "|" + Path.GetFileName(page.ScreenshotPath));
        return exitCode;
    }

    /// <summary>父进程：逐个页面起子进程点击，超时即判 hang。</summary>
    internal static IReadOnlyList<InteractResult> RunAll(string outputDirectory)
    {
        var results = new List<InteractResult>();
        string assemblyPath = typeof(InteractRunner).Assembly.Location;
        string host = Environment.ProcessPath ?? "dotnet";
        // UseAppHost=false 时进程是 dotnet.exe，要先传 dll 路径；万一将来换成 apphost，则直接传参数。
        bool isAppHost = Path.GetFileNameWithoutExtension(host)
            .Equals("DocumentTools.UiSnapshot", StringComparison.OrdinalIgnoreCase);
        Console.WriteLine("CHILD|host=" + host + "|apphost=" + (isAppHost ? "yes" : "no"));

        foreach (string featureId in InteractionScripts.PageIds)
        {
            var result = new InteractResult(featureId);
            string sidecarPath = Path.Combine(outputDirectory, featureId + "@interact.txt");
            if (File.Exists(sidecarPath))
            {
                File.Delete(sidecarPath);
            }

            var startInfo = new ProcessStartInfo(host)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = outputDirectory
            };

            if (!isAppHost)
            {
                startInfo.ArgumentList.Add(assemblyPath);
            }

            startInfo.ArgumentList.Add("--interact");
            startInfo.ArgumentList.Add(featureId);
            startInfo.ArgumentList.Add("--out");
            startInfo.ArgumentList.Add(outputDirectory);
            startInfo.ArgumentList.Add("--no-suite");

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            Task<string> stdout = process.StandardOutput.ReadToEndAsync();
            Task<string> stderr = process.StandardError.ReadToEndAsync();

            if (!process.WaitForExit(ChildTimeoutMilliseconds))
            {
                result.TimedOut = true;
                result.Outcome = "hang";
                TryKill(process);
            }
            else
            {
                result.ExitCode = process.ExitCode;
                result.Outcome = OutcomeOf(result.ExitCode);
            }

            result.Output = Combine(stdout, stderr);
            ReadSidecar(sidecarPath, result);
            results.Add(result);

            Console.WriteLine(
                "INTERACT|" + result.FeatureId + "|" + result.Outcome
                + "|status=" + result.StatusLines
                + "|exit=" + result.ExitCode
                + "|png=" + (result.ScreenshotFile.Length > 0 ? result.ScreenshotFile : "-"));
        }

        // 只跑单个页面时（--interact X）不许覆盖权威 _interact.csv。
        QaReportWriter.WriteInteract(outputDirectory, results, partial: results.Count < InteractionScripts.PageIds.Length);
        return results;
    }

    private static string OutcomeOf(int exitCode) => exitCode switch
    {
        0 => "ok",
        2 => "unknown-feature",
        3 => "fatal",
        4 => "no-button",
        5 => "no-status",
        6 => "exception",
        7 => "step-timeout",
        _ => "crashed"
    };

    private static void TryKill(Process process)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                if (process.HasExited)
                {
                    return;
                }

                process.Kill(entireProcessTree: true);
                if (process.WaitForExit(5000))
                {
                    return;
                }
            }
            catch (InvalidOperationException)
            {
                // 已经自己退出，忽略。
                return;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // 权限/句柄问题：下一轮再试。
            }
        }
    }

    private static string Combine(Task<string> stdout, Task<string> stderr)
    {
        string output = string.Empty;
        string error = string.Empty;

        try
        {
            output = stdout.Wait(3000) ? stdout.Result : string.Empty;
            error = stderr.Wait(3000) ? stderr.Result : string.Empty;
        }
        catch (AggregateException)
        {
            return output + error;
        }

        return (output + error).Trim();
    }

    private static void WriteSidecar(
        string outputDirectory,
        string featureId,
        PageSnapshot page,
        IReadOnlyList<string> trace,
        string outcome)
    {
        var builder = new StringBuilder();
        builder.AppendLine("feature: " + featureId);
        builder.AppendLine("outcome: " + outcome);
        builder.AppendLine("screenshot: " + Path.GetFileName(page.ScreenshotPath));
        builder.AppendLine("png bytes: " + page.PngBytes);
        builder.AppendLine("fatal: " + (page.FatalError ?? "no"));

        foreach (AuditIssue issue in page.Issues.OrderBy(i => i.Severity))
        {
            builder.AppendLine("issue: " + issue.Severity + " | " + issue.Kind + " | " + issue.Element + " | " + issue.Detail);
        }

        builder.AppendLine("trace:");
        foreach (string line in trace)
        {
            builder.AppendLine("  " + line);
        }

        File.WriteAllText(
            Path.Combine(outputDirectory, featureId + "@interact.txt"),
            builder.ToString(),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private static void ReadSidecar(string path, InteractResult result)
    {
        if (!File.Exists(path))
        {
            return;
        }

        foreach (string line in File.ReadAllLines(path))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("screenshot: ", StringComparison.Ordinal))
            {
                result.ScreenshotFile = trimmed["screenshot: ".Length..];
            }
            else if (trimmed.StartsWith("status + ", StringComparison.Ordinal))
            {
                result.StatusLines++;
                result.Trace.Add(trimmed["status + ".Length..]);
            }
            else if (trimmed.StartsWith("MISS ", StringComparison.Ordinal)
                || trimmed.StartsWith("click ", StringComparison.Ordinal)
                || trimmed.StartsWith("set ", StringComparison.Ordinal)
                || trimmed.StartsWith("present ", StringComparison.Ordinal)
                || trimmed.StartsWith("nostatus ", StringComparison.Ordinal))
            {
                result.Trace.Add(trimmed);
            }
        }
    }
}
