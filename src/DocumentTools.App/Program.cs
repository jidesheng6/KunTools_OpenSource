using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using DocumentTools.App.Infrastructure.Licensing;
using DocumentTools.App.Shell;

namespace DocumentTools.App;

/// <summary>Entry point — Aspose.Hook MUST run before any WPF / Aspose type use.</summary>
internal static class Program
{
    /// <summary>
    /// 单实例互斥体。注意：它只在 <see cref="Main"/> 里创建，
    /// 测试与快照工具（tools\FeatureSmoke、tools\UiSnapshot、QaFunction）加载 App 程序集但不走 Main，
    /// 因此不会被这里拦下。
    /// </summary>
    private const string SingleInstanceMutex = @"Local\DocumentTools.SingleInstance";

    private const int SwRestore = 9;

    [STAThread]
    private static void Main()
    {
        using var mutex = new Mutex(initiallyOwned: true, SingleInstanceMutex, out bool isFirstInstance);
        if (!isFirstInstance)
        {
            ActivateExistingInstance();
            return;
        }

        InstallGlobalExceptionHandlers();

        try
        {
            // 1) Hook + console/file logs first
            AsposeBootstrap.Start(showHookDetails: true);

            // 2) WPF host (DPI via app.manifest PerMonitorV2 + WPF-UI FluentWindow)
            var app = new App();
            app.DispatcherUnhandledException += (_, e) =>
            {
                e.Handled = true;
                // 界面线程异常：容器多半已就绪，优先走 IDialogService 告知日志路径。
                CrashLog.Report(
                    "DispatcherUnhandledException",
                    e.Exception,
                    "界面操作出现未处理的错误，已记录诊断日志。操作可能没有生效，建议再试一次或重启程序。");
            };

            app.InitializeComponent();
            app.Run();
        }
        catch (Exception ex)
        {
            // 启动兜底：容器未就绪时只能直接弹窗，不走 IDialogService。
            CrashLog.Report("Main", ex, "程序启动失败，已记录诊断日志。", startup: true);
        }
    }

    /// <summary>
    /// 运行期兜底：进程级未捕获异常与后台任务未观察异常。
    /// 两者都不依赖 DI，直接写 crash 日志后按是否可就绪决定提示方式。
    /// </summary>
    private static void InstallGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            // 进程即将终止：只来得及写日志 + 尽可能提示路径。
            CrashLog.Report(
                "AppDomain.UnhandledException",
                e.ExceptionObject as Exception,
                "程序遇到无法恢复的错误，即将退出。请把诊断日志路径记下来，重启后再试。");
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            e.SetObserved();
            // 后台任务的异常不打断当前操作：写日志并在本次会话里最多提示一次。
            CrashLog.ReportQuiet("TaskScheduler.UnobservedTaskException", e.Exception);
        };
    }

    /// <summary>第二个实例：还原并前置已有窗口，然后静默退出（返回码 0）。</summary>
    private static void ActivateExistingInstance()
    {
        string selfName = Process.GetCurrentProcess().ProcessName;
        string? selfModule = TryGetModulePath();

        // 已有实例可能还在启动中，短暂重试等它把主窗口建出来。
        for (int attempt = 0; attempt < 40; attempt++)
        {
            foreach (Process candidate in Process.GetProcesses())
            {
                using (candidate)
                {
                    if (candidate.Id == Environment.ProcessId
                        || candidate.MainWindowHandle == IntPtr.Zero
                        || !IsSameApplication(candidate, selfName, selfModule))
                    {
                        continue;
                    }

                    ShowWindow(candidate.MainWindowHandle, SwRestore);
                    SetForegroundWindow(candidate.MainWindowHandle);
                    return;
                }
            }

            Thread.Sleep(150);
        }
    }

    /// <summary>
    /// 判断进程是不是同一个程序。`dotnet DocumentTools.dll` 启动时进程名同样是 dotnet，
    /// 所以再比对一次主模块路径，避免误把别的 dotnet 宿主窗口提到前面。
    /// </summary>
    private static bool IsSameApplication(Process candidate, string selfName, string? selfModule)
    {
        try
        {
            if (!string.Equals(candidate.ProcessName, selfName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (selfModule is null)
            {
                return true;
            }

            string? module = TryGetModulePath(candidate);
            return module is null || string.Equals(module, selfModule, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static string? TryGetModulePath(Process? process = null)
    {
        try
        {
            return process is null
                ? Environment.ProcessPath
                : process.MainModule?.FileName;
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
}
