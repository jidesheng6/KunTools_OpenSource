using System.Globalization;
using System.IO;
using System.Windows;
using DocumentTools.App.Bootstrap;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.Core.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// UI 快照与客观检查：真 DI 容器 + 真视图工厂逐页创建页面，1200×800 渲染为 PNG，
/// 同时用视觉树遍历做机器可判定的检查（超出视口/被裁切、空文本按钮与标签、键盘可达性）。
/// 一次默认运行依次产出：47 页基准快照、8 页 × 120/144 DPI 快照、3 页交互态快照。
/// 产物落 docs\qa-screens\，明细见 _index.csv / _hidpi.csv / _interact.csv。
/// </summary>
internal static class Program
{
    /// <summary>高 DPI 验证的固定页集合（转换 / 图片 / 条码 / 考评 / 首页 / 系统）。</summary>
    private static readonly string[] HidpiPageIds =
    [
        "word.pdf",
        "image.compress",
        "code.barcode",
        "exam.circle",
        "exam.package",
        "system.network",
        "home",
        "about"
    ];

    private static readonly int[] HidpiDpis = [120, 144];

    [STAThread]
    private static int Main(string[] args)
    {
        string repositoryRoot = FindRepositoryRoot();
        string outputDirectory = Path.Combine(repositoryRoot, "docs", "qa-screens");
        string? pagesFilter = null;
        string? interactFeature = null;
        int dpi = 96;
        bool selfTest = false;
        bool runSuite = true;
        bool runInteract = true;
        bool dumpLayout = false;
        string? windowProbe = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--out" when i + 1 < args.Length:
                    outputDirectory = Path.GetFullPath(args[++i]);
                    break;
                case "--only" when i + 1 < args.Length:
                    pagesFilter = args[++i];
                    break;
                case "--pages" when i + 1 < args.Length:
                    pagesFilter = args[++i];
                    break;
                case "--dpi" when i + 1 < args.Length:
                    dpi = ParseDpi(args[++i]);
                    break;
                case "--interact" when i + 1 < args.Length:
                    interactFeature = args[++i];
                    break;
                case "--self-test":
                    selfTest = true;
                    break;
                case "--no-suite":
                    runSuite = false;
                    break;
                case "--no-interact":
                    runInteract = false;
                    break;
                case "--dump-layout":
                    dumpLayout = true;
                    break;
                case "--window-probe" when i + 1 < args.Length:
                    windowProbe = args[++i];
                    break;
                default:
                    // `dotnet run --project ... -v q --nologo` 会把 dotnet 自己的开关透传给程序，忽略即可。
                    break;
            }
        }

        Directory.CreateDirectory(outputDirectory);

        DocumentTools.App.App app = new();
        app.InitializeComponent();

        if (selfTest)
        {
            return AuditSelfTest.Run();
        }

        // 整套运行独占输出目录：团队里其他人同时跑会互相覆写 CSV/PNG，
        // 让"回归结论"变成两份产物混在一起（本仓库真实发生过）。
        // 子进程（--interact）不参与，因为它本来就写父进程的目录。
        FileStream? outputLock = interactFeature is not null
            ? null
            : TryAcquireOutputLock(outputDirectory);
        if (interactFeature is null && outputLock is null)
        {
            Console.WriteLine("BUSY|另一个 UiSnapshot 正在写 " + outputDirectory + "，本轮退出");
            return 8;
        }

        try
        {
            return Run(repositoryRoot, outputDirectory, pagesFilter, interactFeature, dpi, runSuite, runInteract, dumpLayout, windowProbe);
        }
        finally
        {
            outputLock?.Dispose();
        }
    }

    /// <summary>在输出目录上抢独占句柄；抢不到说明别的实例正在写同一批产物。</summary>
    private static FileStream? TryAcquireOutputLock(string outputDirectory)
    {
        try
        {
            return new FileStream(
                Path.Combine(outputDirectory, ".running.lock"),
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
        }
        catch (IOException)
        {
            return null;
        }
    }

    private static int Run(
        string repositoryRoot,
        string outputDirectory,
        string? pagesFilter,
        string? interactFeature,
        int dpi,
        bool runSuite,
        bool runInteract,
        bool dumpLayout,
        string? windowProbe)
    {
        using ServiceProvider provider = BuildServiceProvider(out QaDialogService dialogs, interactFeature ?? "suite");
        var catalog = provider.GetRequiredService<IFeatureCatalog>();
        var factory = provider.GetRequiredService<IFeatureViewFactory>();

        if (interactFeature is not null)
        {
            return InteractRunner.RunChild(interactFeature, outputDirectory, factory, catalog, dialogs);
        }

        // 诊断模式：不落盘快照，只按页打印底部区域的真实矩形（交互态前后各一次），
        // 用于回答"某个控件到底被挤到哪去了"这类像素级争议。
        if (dumpLayout)
        {
            return DumpLayout(catalog, factory, pagesFilter, dialogs);
        }

        // 实验：把同一页面放进真实 Window 后再读主按钮的画笔，
        // 用来区分"样式真的失效"和"离屏渲染没了主题上下文"。
        if (windowProbe is not null)
        {
            return WindowProbe(catalog, factory, windowProbe);
        }

        List<PageSnapshot> pages = CapturePages(
            repositoryRoot,
            catalog,
            factory,
            outputDirectory,
            pagesFilter,
            dpi);

        // 子集运行（--only/--pages）只写 partial-*.csv，权威 CSV 仅由全量运行改写。
        QaReportWriter.WriteIndex(outputDirectory, pages, partial: pagesFilter is not null);
        QaReportWriter.WriteIssues(outputDirectory, pages, partial: pagesFilter is not null);

        int fatal = pages.Count(p => p.FatalError is not null);
        int high = pages.Sum(p => p.CountOf(Severity.High));
        int medium = pages.Sum(p => p.CountOf(Severity.Medium));
        int low = pages.Sum(p => p.CountOf(Severity.Low));
        long oversized = pages.Count(p => p.PngBytes > 300 * 1024);

        Console.WriteLine(
            "SUMMARY pages=" + pages.Count.ToString(CultureInfo.InvariantCulture)
            + " rendered=" + pages.Count(p => p.PngBytes > 0).ToString(CultureInfo.InvariantCulture)
            + " fatal=" + fatal.ToString(CultureInfo.InvariantCulture)
            + " high=" + high.ToString(CultureInfo.InvariantCulture)
            + " medium=" + medium.ToString(CultureInfo.InvariantCulture)
            + " low=" + low.ToString(CultureInfo.InvariantCulture)
            + " oversized=" + oversized.ToString(CultureInfo.InvariantCulture));
        Console.WriteLine("OUT " + outputDirectory);

        int problems = fatal;

        // 壳层检查：页面全绿也说明不了导航可用。失败计入退出码，明细在 SHELL|FAIL 行。
        problems += ShellAudit.Run(provider);

        if (runSuite)
        {
            problems += RunHidpiSuite(repositoryRoot, catalog, factory, outputDirectory);

            if (runInteract)
            {
                IReadOnlyList<InteractResult> results = InteractRunner.RunAll(outputDirectory);
                int hangs = results.Count(r => r.Outcome == "hang");
                int ok = results.Count(r => r.Outcome == "ok");
                Console.WriteLine(
                    "SUITE|hidpi=" + (HidpiPageIds.Length * HidpiDpis.Length).ToString(CultureInfo.InvariantCulture)
                    + "|interact=" + results.Count.ToString(CultureInfo.InvariantCulture)
                    + "|interactOk=" + ok.ToString(CultureInfo.InvariantCulture)
                    + "|interactHang=" + hangs.ToString(CultureInfo.InvariantCulture));
            }
        }

        return problems == 0 ? 0 : 1;
    }

    private static List<PageSnapshot> CapturePages(
        string repositoryRoot,
        IFeatureCatalog catalog,
        IFeatureViewFactory factory,
        string outputDirectory,
        string? pagesFilter,
        int dpi)
    {
        var pages = new List<PageSnapshot>();
        HashSet<string>? allowed = SplitIds(pagesFilter);
        int index = 0;

        foreach (FeatureDescriptor feature in catalog.All)
        {
            index++;
            if (allowed is not null && !allowed.Contains(feature.Id))
            {
                continue;
            }

            PageSnapshot snapshot = SnapshotRunner.Capture(
                feature,
                index,
                factory,
                outputDirectory,
                FileNameFor(feature.Id, dpi),
                dpi);

            snapshot.ScreenshotFile = RelativeTo(repositoryRoot, snapshot.ScreenshotPath);
            pages.Add(snapshot);
            Console.WriteLine(snapshot.ToConsoleLine());
            Console.WriteLine("UXBOX|" + feature.Id + "|" + VisualAudit.PathBoxStats);
        }

        return pages;
    }

    /// <summary>8 个关键页 × 2 个高 DPI 档，落 docs\qa-screens\hidpi\，用于发现高 DPI 下的栅格/裁切问题。</summary>
    private static int RunHidpiSuite(
        string repositoryRoot,
        IFeatureCatalog catalog,
        IFeatureViewFactory factory,
        string outputDirectory)
    {
        string hidpiDirectory = Path.Combine(outputDirectory, "hidpi");
        Directory.CreateDirectory(hidpiDirectory);

        List<FeatureDescriptor> all = catalog.All.ToList();
        var pages = new List<PageSnapshot>();

        foreach (int dpi in HidpiDpis)
        {
            foreach (string featureId in HidpiPageIds)
            {
                int index = all.FindIndex(f => string.Equals(f.Id, featureId, StringComparison.OrdinalIgnoreCase));
                if (index < 0)
                {
                    Console.WriteLine("HIDPI|" + dpi + "|" + featureId + "|missing");
                    continue;
                }

                PageSnapshot snapshot = SnapshotRunner.Capture(
                    all[index],
                    index + 1,
                    factory,
                    hidpiDirectory,
                    FileNameFor(featureId, dpi),
                    dpi);

                snapshot.ScreenshotFile = RelativeTo(repositoryRoot, snapshot.ScreenshotPath);
                pages.Add(snapshot);

                Console.WriteLine(
                    "HIDPI|" + dpi.ToString(CultureInfo.InvariantCulture)
                    + "|" + featureId
                    + "|" + (snapshot.FatalError is null ? "ok" : "fatal")
                    + "|" + (snapshot.PngBytes / 1024).ToString(CultureInfo.InvariantCulture) + "KB"
                    + "|H" + snapshot.CountOf(Severity.High).ToString(CultureInfo.InvariantCulture)
                    + "/M" + snapshot.CountOf(Severity.Medium).ToString(CultureInfo.InvariantCulture)
                    + "/L" + snapshot.CountOf(Severity.Low).ToString(CultureInfo.InvariantCulture));
            }
        }

        QaReportWriter.WriteHidpi(outputDirectory, pages);
        QaReportWriter.WriteHidpiIssues(outputDirectory, pages);
        return pages.Count(p => p.FatalError is not null) + pages.Sum(p => p.CountOf(Severity.High));
    }

    private static string FileNameFor(string featureId, int dpi)
        => dpi == 96 ? featureId + ".png" : featureId + "@" + dpi.ToString(CultureInfo.InvariantCulture) + ".png";

    /// <summary>打印底部区域（bottom &gt; 560）每个可见控件的真实矩形，交互态页面在脚本前后各打印一次。</summary>
    private static int DumpLayout(IFeatureCatalog catalog, IFeatureViewFactory factory, string? pagesFilter, QaDialogService dialogs)
    {
        HashSet<string>? allowed = SplitIds(pagesFilter);
        string scratch = Path.Combine(Path.GetTempPath(), "dtsnap-dump");
        Directory.CreateDirectory(scratch);

        Console.WriteLine(
            "APP|merged=" + Application.Current.Resources.MergedDictionaries.Count.ToString(CultureInfo.InvariantCulture)
            + "|accent=" + DescribeResource("AccentFillColorDefaultBrush")
            + "|controlFill=" + DescribeResource("ControlFillColorDefaultBrush")
            + "|buttonBg=" + DescribeResource("ButtonBackground"));

        foreach (FeatureDescriptor feature in catalog.All)
        {
            if (allowed is not null && !allowed.Contains(feature.Id))
            {
                continue;
            }

            bool scripted = InteractionScripts.PageIds.Contains(feature.Id, StringComparer.OrdinalIgnoreCase);
            bool[] passes = scripted ? [false, true] : [false];

            foreach (bool runScript in passes)
            {
                var trace = new List<string>();
                Action<FrameworkElement>? script = runScript
                    ? view => InteractionScripts.Run(feature.Id, view, trace, dialogs)
                    : null;
                SnapshotRunner.Capture(
                    feature,
                    0,
                    factory,
                    scratch,
                    feature.Id + (runScript ? "@script.png" : "@base.png"),
                    96,
                    script,
                    view => PrintBottomLayout(feature.Id, runScript, view));
            }
        }

        return 0;
    }

    private static void PrintBottomLayout(string featureId, bool scripted, FrameworkElement view)
    {
        Console.WriteLine("DUMP|" + featureId + "|script=" + (scripted ? "yes" : "no"));

        foreach (FrameworkElement element in InteractionKit.Descendants(view).OfType<FrameworkElement>())
        {
            if (element.Visibility != Visibility.Visible || element.ActualWidth < 1 || element.ActualHeight < 1)
            {
                continue;
            }

            Rect bounds;
            try
            {
                bounds = element.TransformToAncestor(view).TransformBounds(
                    new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            }
            catch (Exception)
            {
                continue;
            }

            if (bounds.Bottom <= 560)
            {
                continue;
            }

            Console.WriteLine(
                "  " + element.GetType().Name
                + "#" + (string.IsNullOrEmpty(element.Name) ? "-" : element.Name)
                + "「" + Trim(InteractionKit.TextOf(element), 16) + "」"
                + " x=" + bounds.Left.ToString("F1", CultureInfo.InvariantCulture)
                + " y=" + bounds.Top.ToString("F1", CultureInfo.InvariantCulture)
                + " w=" + bounds.Width.ToString("F1", CultureInfo.InvariantCulture)
                + " h=" + bounds.Height.ToString("F1", CultureInfo.InvariantCulture)
                + " bottom=" + bounds.Bottom.ToString("F1", CultureInfo.InvariantCulture)
                + DescribePaint(element));
        }
    }

    private static string Trim(string text, int max)
    {
        string single = text.Replace('\r', ' ').Replace('\n', ' ');
        return single.Length <= max ? single : single[..max] + "…";
    }

    /// <summary>打印按钮/文本的画笔与可见性，用于判断"布局在但没画出来"这类问题。</summary>
    private static string DescribePaint(FrameworkElement element)
    {
        if (element is not System.Windows.Controls.Control control)
        {
            return string.Empty;
        }

        string appearance = string.Empty;
        System.Reflection.PropertyInfo? appearanceProperty = control.GetType().GetProperty("Appearance");
        if (appearanceProperty?.GetValue(control) is { } value)
        {
            appearance = " appearance=" + value;
        }

        return appearance
            + " bg=" + Describe(control.Background)
            + " fg=" + Describe(control.Foreground)
            + " opacity=" + control.Opacity.ToString("F2", CultureInfo.InvariantCulture)
            + " enabled=" + (control.IsEnabled ? "yes" : "no");
    }

    private static string Describe(System.Windows.Media.Brush? brush) => brush switch
    {
        null => "null",
        System.Windows.Media.SolidColorBrush solid => solid.Color.ToString(),
        _ => brush.GetType().Name
    };

    /// <summary>
    /// 与 <see cref="AppHost.CreateServiceProvider"/> 一致的容器，但把 <c>IDialogService</c>
    /// 换成不阻塞的假实现：否则点到「开始匹配」「打开…」这类按钮会弹模态框，把整套 harness 卡死。
    /// </summary>
    private static ServiceProvider BuildServiceProvider(out QaDialogService dialogs, string tag)
    {
        var services = new ServiceCollection();
        services.AddDocumentToolsApp();

        var fake = new QaDialogService(tag);
        dialogs = fake;

        services.RemoveAll<IDialogService>();
        services.AddSingleton<IDialogService>(fake);

        return services.BuildServiceProvider();
    }

    private static int WindowProbe(IFeatureCatalog catalog, IFeatureViewFactory factory, string featureId)
    {
        FeatureDescriptor? feature = catalog.All.FirstOrDefault(
            f => string.Equals(f.Id, featureId, StringComparison.OrdinalIgnoreCase));
        if (feature is null)
        {
            Console.WriteLine("PROBE|" + featureId + "|unknown");
            return 2;
        }

        FrameworkElement? view = factory.Create(feature.Id);
        if (view is null)
        {
            Console.WriteLine("PROBE|" + featureId + "|no-view");
            return 3;
        }

        var host = new System.Windows.Controls.Border
        {
            Background = System.Windows.Media.Brushes.White,
            Width = SnapshotRunner.PageWidth,
            Height = SnapshotRunner.PageHeight,
            Child = view
        };

        var window = new Window
        {
            Content = host,
            Width = SnapshotRunner.PageWidth + 16,
            Height = SnapshotRunner.PageHeight + 40,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            Left = -4000,
            Top = -4000
        };

        window.Show();
        SnapshotRunner.PumpDispatcher(400);
        host.UpdateLayout();

        Console.WriteLine("PROBE|" + featureId + "|window-shown");
        PrintPrimaryButtons(view);
        window.Close();
        return 0;
    }

    private static void PrintPrimaryButtons(FrameworkElement view)
    {
        foreach (FrameworkElement element in InteractionKit.Descendants(view).OfType<FrameworkElement>())
        {
            System.Reflection.PropertyInfo? appearance = element.GetType().GetProperty("Appearance");
            if (appearance is null || appearance.GetValue(element)?.ToString() != "Primary")
            {
                continue;
            }

            Console.WriteLine("  PRIMARY「" + Trim(InteractionKit.TextOf(element), 12) + "」" + DescribePaint(element));
        }
    }

    private static string DescribeResource(string key)
    {
        object? value = Application.Current.TryFindResource(key);
        return value switch
        {
            null => "MISSING",
            System.Windows.Media.SolidColorBrush solid => solid.Color.ToString(),
            _ => value.GetType().Name
        };
    }

    private static HashSet<string>? SplitIds(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return null;
        }

        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string part in filter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            ids.Add(part);
        }

        return ids.Count == 0 ? null : ids;
    }

    private static int ParseDpi(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) && parsed >= 96
            ? parsed
            : 96;

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DocumentTools.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("未能定位仓库根目录（找不到 DocumentTools.sln）");
    }

    private static string RelativeTo(string root, string path)
    {
        string full = Path.GetFullPath(path);
        string prefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return full[prefix.Length..].Replace('\\', '/');
        }

        return full.Replace('\\', '/');
    }
}
