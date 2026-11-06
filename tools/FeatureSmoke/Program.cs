using System.Windows;
using DocumentTools.App.Bootstrap;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.Core.Features;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.FeatureSmoke;

/// <summary>
/// 冒烟验证：真实 DI 容器 + 真实视图工厂，逐个创建功能页并强制走一遍
/// Measure/Arrange（会触发 WPF-UI 模板与资源查找），用于发现「页面点开就炸」的问题。
/// 不做业务执行，不弹窗，结果只写控制台。
/// </summary>
internal static class Program
{
    [STAThread]
    private static int Main()
    {
        var createFailures = new List<string>();
        var renderFailures = new List<string>();
        int created = 0;
        int rendered = 0;

        DocumentTools.App.App app = new();
        app.InitializeComponent();

        using ServiceProvider provider = AppHost.CreateServiceProvider();

        var catalog = provider.GetRequiredService<IFeatureCatalog>();
        var factory = provider.GetRequiredService<IFeatureViewFactory>();

        foreach (FeatureDescriptor feature in catalog.All)
        {
            FrameworkElement? view;
            try
            {
                view = factory.Create(feature.Id);
            }
            catch (Exception ex)
            {
                var frames = (ex.StackTrace ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).Take(4);
                createFailures.Add($"{feature.Id} [{feature.Title}] {ex.GetType().Name}: {ex.Message} @@ {string.Join(" | ", frames.Select(f => f.Trim()))}");
                continue;
            }

            if (view is null)
            {
                createFailures.Add($"{feature.Id} [{feature.Title}] 未注册视图模块");
                continue;
            }

            created++;
            try
            {
                view.Measure(new Size(1200, 800));
                view.Arrange(new Rect(0, 0, 1200, 800));
                view.UpdateLayout();
                rendered++;
            }
            catch (Exception ex)
            {
                renderFailures.Add($"{feature.Id} [{feature.Title}] {ex.GetType().Name}: {ex.Message}");
            }
        }

        // 主窗口本身也建一次：导航壳、图标、分组展开都在这里
        try
        {
            _ = provider.GetRequiredService<DocumentTools.App.Shell.MainWindow>();
        }
        catch (Exception ex)
        {
            createFailures.Add($"MainWindow {ex.GetType().Name}: {ex.Message}");
        }

        // 启动落点判定：配置指向已隐藏/已移除的功能时必须回到首页，不能停在空白页
        var startFailures = new List<string>();
        List<string> allIds = catalog.All.Select(f => f.Id).ToList();
        foreach (string id in allIds)
        {
            DocumentTools.App.Shell.StartResolution visible =
                DocumentTools.App.Shell.NavigationFallback.Resolve(id, allIds);
            if (!string.Equals(visible.FeatureId, id, StringComparison.OrdinalIgnoreCase) || visible.Notice is not null)
            {
                startFailures.Add($"可见功能 {id} 未按配置打开（{visible.FeatureId}）：{visible.Notice}");
            }

            DocumentTools.App.Shell.StartResolution hidden =
                DocumentTools.App.Shell.NavigationFallback.Resolve(id, allIds, new[] { id });
            if (!string.Equals(hidden.FeatureId, DocumentTools.App.Shell.NavigationFallback.DefaultFeatureId, StringComparison.OrdinalIgnoreCase)
                || hidden.Notice is null)
            {
                startFailures.Add($"隐藏功能 {id} 未回退首页（{hidden.FeatureId}）：{hidden.Notice}");
            }
        }

        DocumentTools.App.Shell.StartResolution unknown =
            DocumentTools.App.Shell.NavigationFallback.Resolve("no-such-feature", allIds);
        if (!string.Equals(unknown.FeatureId, DocumentTools.App.Shell.NavigationFallback.DefaultFeatureId, StringComparison.OrdinalIgnoreCase)
            || unknown.Notice is null)
        {
            startFailures.Add($"未注册的启动功能未回退首页（{unknown.FeatureId}）");
        }

        Console.WriteLine($"功能总数 {catalog.All.Count}，创建成功 {created}，布局通过 {rendered}");
        Console.WriteLine($"启动落点检查 {allIds.Count * 2 + 1} 项，失败 {startFailures.Count}");
        Report("创建失败", createFailures);
        Report("布局失败", renderFailures);
        Report("启动落点失败", startFailures);

        return createFailures.Count == 0 && renderFailures.Count == 0 && startFailures.Count == 0 ? 0 : 1;
    }

    private static void Report(string title, List<string> items)
    {
        if (items.Count == 0)
        {
            return;
        }

        Console.WriteLine($"--- {title} ({items.Count}) ---");
        foreach (string item in items)
        {
            Console.WriteLine("  " + item);
        }
    }
}
