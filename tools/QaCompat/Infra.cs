using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace QaCompat;

internal enum CheckStatus
{
    Pass,
    Fail,
    Unverified
}

internal sealed class Finding
{
    public required string Severity { get; init; }

    public required string Title { get; init; }

    public required string Detail { get; init; }

    public required string Location { get; init; }

    public required string CheckId { get; init; }
}

internal sealed class Check
{
    public required string Id { get; init; }

    public required string Group { get; init; }

    public required string Title { get; init; }

    public required string Setup { get; init; }

    public required string Expected { get; init; }

    public string Actual { get; set; } = string.Empty;

    public string Evidence { get; set; } = string.Empty;

    public string? SkipReason { get; set; }

    public CheckStatus Status { get; set; } = CheckStatus.Unverified;

    public List<Finding> Findings { get; } = new();

    public List<string> Notes { get; } = new();
}

/// <summary>检查体主动放弃（场景在本机无法复现）时抛出，结论记为「无法验证」。</summary>
internal sealed class SkipCheckException : Exception
{
    public SkipCheckException(string reason)
        : base(reason)
    {
    }
}

internal sealed class QaContext
{
    public QaContext(string repoRoot, string workRoot, string reportPath)
    {
        RepoRoot = repoRoot;
        WorkRoot = workRoot;
        ReportPath = reportPath;
        Directory.CreateDirectory(workRoot);
    }

    public string RepoRoot { get; }

    public string WorkRoot { get; }

    public string ReportPath { get; }

    public string ExamFixtureDll => Path.Combine(
        RepoRoot, "tools", "ExamFixture", "bin", "Debug", "net8.0-windows", "ExamFixture.dll");

    public List<Check> Checks { get; } = new();

    public List<(string Key, string Value)> Facts { get; } = new();

    public int PassCount => Checks.Count(c => c.Status == CheckStatus.Pass);

    public int FailCount => Checks.Count(c => c.Status == CheckStatus.Fail);

    public int UnverifiedCount => Checks.Count(c => c.Status == CheckStatus.Unverified);

    public IEnumerable<Finding> AllFindings =>
        Checks.SelectMany(c => c.Findings)
            .OrderBy(f => f.Severity switch { "高" => 0, "中" => 1, _ => 2 })
            .ThenBy(f => f.CheckId, StringComparer.Ordinal);

    public string Dir(params string[] parts)
    {
        string path = WorkRoot;
        foreach (string part in parts)
        {
            path = Path.Combine(path, part);
        }

        Directory.CreateDirectory(path);
        return path;
    }

    public void Fact(string key, string value) => Facts.Add((key, value));

    public void Run(
        string id,
        string group,
        string title,
        string setup,
        string expected,
        Action<Check> body)
    {
        var check = new Check
        {
            Id = id,
            Group = group,
            Title = title,
            Setup = setup,
            Expected = expected
        };
        Checks.Add(check);

        try
        {
            body(check);
        }
        catch (SkipCheckException skip)
        {
            check.Status = CheckStatus.Unverified;
            check.SkipReason = skip.Message;
            check.Actual = check.Actual.Length > 0 ? check.Actual : skip.Message;
            check.Evidence = check.Evidence.Length > 0 ? check.Evidence : skip.Message;
        }
        catch (Exception ex)
        {
            // 检查体之外的意外异常本身就是降级缺陷：记录类型 + 完整消息链。
            check.Status = CheckStatus.Fail;
            check.Actual = "执行检查时抛出未预期异常：" + ex.GetType().Name;
            check.Evidence = Fmt.Ex(ex);
            check.Notes.Add("该异常未被任何降级分支捕获，直接冒到调用方。");
        }

        // 检查体既没抛异常也没给判定：视为结构错误，避免「默认 Unverified」掩盖漏判。
        if (check.Status == CheckStatus.Unverified && check.SkipReason is null)
        {
            check.Status = CheckStatus.Fail;
            check.Actual = "检查体结束时没有给出结论（无异常也无判定）。";
        }
    }

    public void Pass(Check check, string actual, string evidence)
    {
        check.Status = CheckStatus.Pass;
        check.Actual = actual;
        check.Evidence = evidence;
    }

    public void Fail(Check check, string actual, string evidence)
    {
        check.Status = CheckStatus.Fail;
        check.Actual = actual;
        check.Evidence = evidence;
    }

    public void Skip(Check check, string actual, string reason)
    {
        check.Status = CheckStatus.Unverified;
        check.SkipReason = reason;
        check.Actual = actual;
        check.Evidence = reason;
    }

    public void AddFinding(Check check, string severity, string title, string detail, string location)
    {
        check.Findings.Add(new Finding
        {
            Severity = severity,
            Title = title,
            Detail = detail,
            Location = location,
            CheckId = check.Id
        });
    }
}

internal static class Fmt
{
    public static string Ex(Exception exception)
    {
        var builder = new StringBuilder();
        Exception? current = exception;
        int depth = 0;
        while (current is not null && depth < 4)
        {
            string prefix = depth == 0 ? string.Empty : new string(' ', depth * 2) + "inner: ";
            builder.Append(prefix)
                .Append(current.GetType().Name)
                .Append(": ")
                .Append(OneLine(current.Message))
                .Append('\n');
            current = current.InnerException;
            depth++;
        }

        return builder.ToString().TrimEnd('\n');
    }

    public static string OneLine(string text)
        => text.Replace("\r", " ").Replace("\n", " ").Trim();

    public static string Clip(string text, int max = 700)
    {
        string oneLine = text.Trim();
        return oneLine.Length <= max ? oneLine : oneLine[..max] + "…";
    }
}

internal sealed record ProcResult(int ExitCode, string StdOut, string StdErr);

internal static class Proc
{
    public static ProcResult Run(
        string fileName,
        IReadOnlyList<string> arguments,
        string workingDirectory,
        IReadOnlyDictionary<string, string>? environment = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        if (environment is not null)
        {
            foreach (KeyValuePair<string, string> pair in environment)
            {
                startInfo.Environment[pair.Key] = pair.Value;
            }
        }

        using Process process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("无法启动进程：" + fileName);

        // 直接读原始字节：子进程重定向时的控制台编码随宿主代码页变化
        // （ExamFixture 在本机输出 GBK，强制按 UTF-8 解码会得到乱码）。
        using var stdout = new MemoryStream();
        using var stderr = new MemoryStream();
        Task copyOut = process.StandardOutput.BaseStream.CopyToAsync(stdout);
        Task copyErr = process.StandardError.BaseStream.CopyToAsync(stderr);

        if (!process.WaitForExit(180_000))
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch
            {
                // 超时后终止失败不影响结论记录。
            }
        }

        Task.WaitAll(copyOut, copyErr);
        int exitCode = process.HasExited ? process.ExitCode : -1;
        return new ProcResult(exitCode, Decode(stdout.ToArray()), Decode(stderr.ToArray()));
    }

    /// <summary>优先按严格 UTF-8 解码，失败则按系统 ANSI 代码页（中文 Windows 为 936）解码。</summary>
    private static string Decode(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return string.Empty;
        }

        try
        {
            string text = new UTF8Encoding(false, true).GetString(bytes);
            return text.TrimStart('\uFEFF');
        }
        catch (DecoderFallbackException)
        {
            return DecodeAnsi(bytes);
        }
    }

    private static string DecodeAnsi(byte[] bytes)
    {
        const uint cpAcp = 0;
        int count = MultiByteToWideChar(cpAcp, 0, bytes, bytes.Length, null, 0);
        if (count <= 0)
        {
            return Encoding.Latin1.GetString(bytes);
        }

        var chars = new char[count];
        count = MultiByteToWideChar(cpAcp, 0, bytes, bytes.Length, chars, count);
        return count > 0 ? new string(chars, 0, count) : Encoding.Latin1.GetString(bytes);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int MultiByteToWideChar(
        uint codePage,
        uint flags,
        byte[] bytes,
        int byteCount,
        [Out] char[]? chars,
        int charCount);
}
