using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using QaCompat.Checks;

namespace QaCompat;

internal static class Program
{
    private static readonly string[] GroupOrder = { "ace", "html", "twain", "license", "readonly", "path" };

    private static int Main(string[] args)
    {
        try
        {
            return Run(args);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[QaCompat] 未处理异常：" + Fmt.Ex(ex));
            return 3;
        }
    }

    private static int Run(string[] args)
    {
        string? repoRoot = FindRepoRoot();
        if (repoRoot is null)
        {
            Console.Error.WriteLine("[QaCompat] 找不到仓库根（自当前目录向上未发现 src\\DocumentTools.Core\\DocumentTools.Core.csproj）。");
            return 2;
        }

        string reportPath = Path.Combine(repoRoot, "docs", "qa-report-compat.md");
        string workRoot = Path.Combine(
            repoRoot,
            "testdata",
            "compat",
            "work",
            "run-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture));
        var selected = new List<string>();
        bool all = false;

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            switch (arg)
            {
                case "--all":
                    all = true;
                    break;
                case "--only":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("[QaCompat] --only 需要分组列表，例如 --only ace,html");
                        return 2;
                    }

                    selected.AddRange(args[++i]
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                    break;
                case "--report":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("[QaCompat] --report 需要文件路径");
                        return 2;
                    }

                    reportPath = Path.GetFullPath(args[++i], repoRoot);
                    break;
                case "--work":
                    if (i + 1 >= args.Length)
                    {
                        Console.Error.WriteLine("[QaCompat] --work 需要目录路径");
                        return 2;
                    }

                    workRoot = Path.GetFullPath(args[++i], repoRoot);
                    break;
                case "--help":
                case "-h":
                    PrintUsage();
                    return 0;
                default:
                    Console.Error.WriteLine("[QaCompat] 未知参数：" + arg);
                    PrintUsage();
                    return 2;
            }
        }

        List<string> groups = all || selected.Count == 0
            ? GroupOrder.ToList()
            : GroupOrder.Where(g => selected.Contains(g, StringComparer.OrdinalIgnoreCase)).ToList();

        if (groups.Count == 0)
        {
            Console.Error.WriteLine("[QaCompat] 未选择任何有效分组。有效值：" + string.Join(",", GroupOrder));
            return 2;
        }

        if (!all && selected.Count > 0)
        {
            string[] unknown = selected
                .Where(g => !GroupOrder.Contains(g, StringComparer.OrdinalIgnoreCase))
                .ToArray();
            if (unknown.Length > 0)
            {
                Console.Error.WriteLine("[QaCompat] 未知分组：" + string.Join(",", unknown) + "；有效值：" + string.Join(",", GroupOrder));
                return 2;
            }
        }

        var ctx = new QaContext(repoRoot, workRoot, reportPath);
        Console.WriteLine("[QaCompat] 仓库: " + repoRoot);
        Console.WriteLine("[QaCompat] 工作目录: " + workRoot);
        Console.WriteLine("[QaCompat] 分组: " + string.Join(",", groups));

        if (groups.Contains("ace")) AceChecks.Run(ctx);
        if (groups.Contains("html")) HtmlChecks.Run(ctx);
        if (groups.Contains("twain")) TwainChecks.Run(ctx);
        if (groups.Contains("license")) LicenseChecks.Run(ctx);
        if (groups.Contains("readonly")) ReadOnlyChecks.Run(ctx);
        if (groups.Contains("path")) PathChecks.Run(ctx);

        List<(string Key, string Value)> environment = Environment_Collect(repoRoot, workRoot, reportPath);
        string markdown = ReportBuilder.Build(ctx, environment);
        Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? repoRoot);
        File.WriteAllText(reportPath, markdown, new UTF8Encoding(false));

        Console.WriteLine();
        foreach (Check check in ctx.Checks)
        {
            Console.WriteLine("  " + check.Id.PadRight(24) + StatusText(check.Status) + "  " + Fmt.Clip(Fmt.OneLine(check.Actual), 110));
        }

        Console.WriteLine();
        Console.WriteLine("[QaCompat] PASS " + ctx.PassCount + " / FAIL " + ctx.FailCount + " / 无法验证 " + ctx.UnverifiedCount);
        foreach (Finding finding in ctx.AllFindings)
        {
            Console.WriteLine("[QaCompat] [" + finding.Severity + "] " + finding.Title + "  (" + finding.Location + ")");
        }

        Console.WriteLine("[QaCompat] 报告: " + reportPath);
        return ctx.FailCount > 0 ? 1 : 0;
    }

    private static List<(string Key, string Value)> Environment_Collect(string repoRoot, string workRoot, string reportPath)
    {
        var items = new List<(string Key, string Value)>
        {
            ("运行时间", DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss zzz", CultureInfo.InvariantCulture)),
            ("主机", Environment.MachineName),
            ("操作系统", RuntimeInformation.OSDescription),
            ("进程/系统架构", RuntimeInformation.ProcessArchitecture + " / " + RuntimeInformation.OSArchitecture),
            ("运行时", RuntimeInformation.FrameworkDescription + "（Environment.Version=" + Environment.Version + "）"),
            ("仓库根", repoRoot),
            ("Git 分支", GitBranch(repoRoot)),
            ("工作目录", workRoot),
            ("报告路径", reportPath),
            ("长路径（LongPathsEnabled）", LongPathsEnabled()),
            ("ExamFixture 程序集", ctx_examFixture(repoRoot)),
            ("浏览器候选", BrowserCandidates())
        };

        return items;
    }

    private static string ctx_examFixture(string repoRoot)
    {
        string dll = Path.Combine(repoRoot, "tools", "ExamFixture", "bin", "Debug", "net8.0-windows", "ExamFixture.dll");
        return File.Exists(dll) ? dll : "(未找到，需先构建 tools\\ExamFixture)";
    }

    private static string GitBranch(string repoRoot)
    {
        try
        {
            ProcResult result = Proc.Run("git", new[] { "rev-parse", "--abbrev-ref", "HEAD" }, repoRoot);
            string branch = result.StdOut.Trim();
            return result.ExitCode == 0 && branch.Length > 0 ? branch : "(未取到，" + Fmt.Clip(Fmt.OneLine(result.StdErr), 120) + ")";
        }
        catch (Exception ex)
        {
            return "(未取到：" + Fmt.OneLine(ex.Message) + ")";
        }
    }

    private static string LongPathsEnabled()
    {
        try
        {
            object? value = Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\FileSystem",
                "LongPathsEnabled",
                null);
            return value switch
            {
                null => "未设置（默认关闭）",
                1 => "1（已启用）",
                _ => value.ToString() ?? "(未知)"
            };
        }
        catch (Exception ex)
        {
            return "(读取失败：" + Fmt.OneLine(ex.Message) + ")";
        }
    }

    private static string BrowserCandidates()
    {
        string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        string[] candidates =
        {
            Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(programFiles, "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(programFilesX86, "Microsoft", "Edge", "Application", "msedge.exe")
        };
        string[] found = candidates.Where(File.Exists).ToArray();
        return found.Length == 0 ? "(四个候选均不存在)" : string.Join(" | ", found);
    }

    private static string? FindRepoRoot()
    {
        var starts = new List<string> { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (string start in starts)
        {
            var dir = new DirectoryInfo(start);
            for (int depth = 0; dir is not null && depth < 10; depth++, dir = dir.Parent)
            {
                if (File.Exists(Path.Combine(dir.FullName, "src", "DocumentTools.Core", "DocumentTools.Core.csproj")))
                {
                    return dir.FullName;
                }
            }
        }

        return null;
    }

    private static string StatusText(CheckStatus status) => status switch
    {
        CheckStatus.Pass => "PASS",
        CheckStatus.Fail => "FAIL",
        _ => "无法验证"
    };

    private static void PrintUsage()
    {
        Console.WriteLine("用法：dotnet run --project tools\\QaCompat -- [--all | --only ace,html,twain,license,readonly,path] [--work <dir>] [--report <file>]");
        Console.WriteLine("  --all       运行全部分组（默认）");
        Console.WriteLine("  --only      只运行指定分组，逗号分隔");
        Console.WriteLine("  --work      工作目录（默认 testdata\\compat\\work\\run-<时间戳>）");
        Console.WriteLine("  --report    报告输出路径（默认 docs\\qa-report-compat.md）");
        Console.WriteLine("退出码：0 全部 PASS/无法验证；1 存在 FAIL；2 参数错误；3 未处理异常。");
    }
}
