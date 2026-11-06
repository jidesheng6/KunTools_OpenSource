using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using DocumentTools.App.Infrastructure.Dialogs;

namespace DocumentTools.App.Shell;

/// <summary>
/// 崩溃日志的唯一写入口。每条未捕获异常写一个 <c>logs\crash-&lt;yyyyMMdd-HHmmss&gt;.log</c>，
/// 内容含异常全串、程序集版本、.NET / OS 信息与进程号，便于按时间戳定位。
/// <para>
/// <see cref="Notifier"/> 由 <c>App.OnStartup</c> 在容器就绪后注入；容器未就绪（启动早期失败）
/// 时退化为直接弹窗，保证任何阶段出错都能把日志路径交到用户手里。
/// </para>
/// </summary>
public static class CrashLog
{
    /// <summary>日志目录：输出目录下的 <c>logs</c>，与 <c>AsposeBootstrap.LogPath</c> 同级。</summary>
    public static string DirectoryPath => Path.Combine(AppContext.BaseDirectory, "logs");

    /// <summary>启动期失败的历史日志路径，保持与旧版本一致，不因新增 crash-*.log 而搬家。</summary>
    public static string StartupFile => Path.Combine(DirectoryPath, "startup-crash.log");

    /// <summary>容器就绪后由 <c>App.OnStartup</c> 注入；为空时用 MessageBox 兜底。</summary>
    public static IDialogService? Notifier { get; set; }

    /// <summary>
    /// 写一条 crash 日志并通知用户，返回日志完整路径（写盘失败时返回空串）。
    /// </summary>
    /// <param name="phase">出错阶段，如 <c>DispatcherUnhandledException</c>。</param>
    /// <param name="ex">异常对象，可为空（只记录阶段）。</param>
    /// <param name="headline">给用户看的一句话，日志路径会自动附在后面。</param>
    /// <param name="startup">启动期失败时额外追加 <c>startup-crash.log</c>，保持旧路径可查。</param>
    public static string Report(string phase, Exception? ex, string headline, bool startup = false)
    {
        string path = Write(phase, ex, appendStartup: startup);
        Notify(headline, path);
        return path;
    }

    /// <summary>只写日志、不打扰用户（后台任务的未观察异常用）。</summary>
    public static string ReportQuiet(string phase, Exception? ex)
    {
        string path = Write(phase, ex);
        NotifyOnce(headline: "后台任务出现未处理的错误，已记录诊断日志。", path);
        return path;
    }

    /// <summary>把异常写成一个独立日志文件，返回路径。</summary>
    public static string Write(string phase, Exception? ex, bool appendStartup = false)
    {
        string text = Describe(phase, ex);
        string path = string.Empty;
        try
        {
            System.IO.Directory.CreateDirectory(DirectoryPath);
            path = BuildPath();
            File.WriteAllText(path, text, Encoding.UTF8);
            if (appendStartup)
            {
                File.AppendAllText(StartupFile, text, Encoding.UTF8);
            }

            Console.Error.WriteLine(text);
        }
        catch
        {
            // 日志写不出来时不能让异常处理本身再抛。
            path = string.Empty;
        }

        return path;
    }

    /// <summary>提示用户：容器就绪走 <see cref="IDialogService"/>，否则直接弹窗。</summary>
    public static void Notify(string headline, string path)
    {
        string message = string.IsNullOrEmpty(path)
            ? headline + "\n\n诊断日志写入失败，请检查程序目录是否可写。"
            : headline + "\n\n诊断日志：" + path;

        try
        {
            if (Notifier is { } notifier)
            {
                notifier.Error(message, "出错了");
                return;
            }

            // 早于容器的兜底：启动失败时没有 IDialogService 可用，只能直接弹窗。
            // 这是全库唯一允许的 MessageBox，G8 静态扫描命中 1 条属有意为之。
            MessageBox.Show(message, "出错了", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch
        {
            Console.Error.WriteLine(message);
        }
    }

    private static bool _notifiedOnce;

    /// <summary>同一次会话只打扰一次，避免后台任务刷屏。</summary>
    private static void NotifyOnce(string headline, string path)
    {
        if (_notifiedOnce)
        {
            return;
        }

        _notifiedOnce = true;
        Notify(headline, path);
    }

    private static string BuildPath()
    {
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string path = Path.Combine(DirectoryPath, $"crash-{stamp}.log");
        for (int i = 2; File.Exists(path); i++)
        {
            path = Path.Combine(DirectoryPath, $"crash-{stamp}-{i}.log");
        }

        return path;
    }

    private static string Describe(string phase, Exception? ex)
    {
        Assembly assembly = typeof(CrashLog).Assembly;
        var sb = new StringBuilder();
        sb.AppendLine($"==== {DateTime.Now:yyyy-MM-dd HH:mm:ss} [{phase}] ====");
        sb.AppendLine($"程序集: {assembly.GetName().Name} {assembly.GetName().Version}");
        sb.AppendLine($"运行时: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($"系统: {RuntimeInformation.OSDescription} ({RuntimeInformation.ProcessArchitecture})");
        sb.AppendLine($"进程: {Environment.ProcessId} · 目录: {AppContext.BaseDirectory}");
        sb.AppendLine();
        sb.AppendLine(ex?.ToString() ?? "(无异常对象)");
        sb.AppendLine();
        return sb.ToString();
    }
}
