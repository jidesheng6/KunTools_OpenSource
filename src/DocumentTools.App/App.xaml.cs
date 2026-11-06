using System.Windows;
using DocumentTools.App.Bootstrap;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App;

public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _services = AppHost.CreateServiceProvider();
            // 容器就绪后把弹窗能力交给崩溃日志，运行期异常就能提示日志路径。
            CrashLog.Notifier = _services.GetService<IDialogService>();
            MainWindow = _services.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }
        catch (Exception ex)
        {
            Infrastructure.Licensing.AsposeBootstrap.Log($"App.OnStartup failed: {ex}");
            // 启动兜底：容器未就绪时只能直接弹窗，不走 IDialogService。
            CrashLog.Report("App.OnStartup", ex, "程序启动失败，已记录诊断日志。", startup: true);
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        CrashLog.Notifier = null;
        _services?.Dispose();
        base.OnExit(e);
    }
}
