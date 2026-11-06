using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DocumentTools.App.Bootstrap;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Shell;
using DocumentTools.App.Ui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DocumentTools.QaFunction;

/// <summary>
/// UX 政策的行为断言（task-46）。和 UiSnapshot 的静态视觉树检查互补：这里在**真实 Window**
/// 里布局、真的触发拖放 / 点击 / 执行，证明「只读但能复制」「选择…会写值」「路径失效给中文提示」
/// 「父级行高 ≥ 34」是行为而不是样式声明。
/// </summary>
internal static partial class Program
{
    /// <summary>脚本化对话框：只回答路径，绝不打真实文件对话框（无人值守运行不能弹窗）。</summary>
    private sealed class ScriptedDialogService : IDialogService
    {
        public string? FilePath { get; set; }

        public string? FolderPath { get; set; }

        public IReadOnlyList<string> OpenFiles(string filter, bool multiSelect = true)
            => string.IsNullOrEmpty(FilePath) ? Array.Empty<string>() : new[] { FilePath! };

        public string? SaveFile(string filter, string? defaultName = null) => FilePath;

        public string? PickFolder() => FolderPath;

        public void Info(string message, string title = "提示")
        {
        }

        public void Error(string message, string title = "错误")
        {
        }

        public bool Confirm(string message, string title = "确认") => true;
    }

    /// <summary>路径框政策标记：ToolViewKit.PathBox 会写入这条 HelpText。</summary>
    private const string PathBoxMarker = "只读：点右侧按钮选择";

    private static void RunUiPolicyChecks()
    {
        Console.WriteLine();
        Console.WriteLine("== UX 政策（行为断言）==");

        // WPF 需要 STA，且 Application/Window 必须与控件同一线程：整组在一次 STA 会话里跑完。
        Exception? crash = null;
        var thread = new Thread(() =>
        {
            try
            {
                RunUiPolicyOnStaThread();
            }
            catch (Exception ex)
            {
                crash = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        thread.Join();

        if (crash is not null)
        {
            foreach (string feature in new[] { "ui.policy.pathbox", "ui.policy.navrow", "ui.policy.missing-path" })
            {
                if (!Wanted(feature))
                {
                    continue;
                }

                Results.Add(new CheckResult(feature, "(n/a)", "(n/a)", 0, 0, false,
                    $"{crash.GetType().Name}: {crash.Message}", true, ExtendedGroup));
            }
        }
    }

    private static void RunUiPolicyOnStaThread()
    {
        if (Application.Current is null)
        {
            // 必须用应用自己的 App 实例：App.xaml 里合并了 Wpf.Ui 的 ThemesDictionary/ControlsDictionary，
            // 裸 Application 没有这些资源，FluentWindow 的模板不会实现，控件量出来的 ActualHeight 全是 0。
            var app = new DocumentTools.App.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.InitializeComponent();
        }

        var dialogs = new ScriptedDialogService();
        var collection = new ServiceCollection();
        collection.AddDocumentToolsApp();
        collection.RemoveAll<IDialogService>();
        collection.AddSingleton<IDialogService>(dialogs);
        using ServiceProvider provider = collection.BuildServiceProvider();

        List<IFeatureModule> modules = provider.GetServices<IFeatureModule>().ToList();
        Console.WriteLine($"  ui 视图：{modules.Count} 个 feature module");

        RunExt("ui.policy.pathbox", () => CheckPathBoxPolicy(provider, modules, dialogs));
        RunExt("ui.policy.pathbox.coverage", () => CheckPathBoxCoverage(provider, modules));
        RunExt("ui.policy.navrow", () => CheckNavRowPolicy(provider));
        RunExt("ui.policy.missing-path", () => CheckMissingPathNotice(provider, modules));
    }

    /// <summary>把视图放进真实 Window 里布局一次，拿到 ActualWidth/ActualHeight 与可交互的句柄。</summary>
    private static Window HostInWindow(FrameworkElement view, double width = 1180, double height = 900)
    {
        var window = new Window
        {
            Content = view,
            Width = width,
            Height = height,
            WindowStyle = WindowStyle.None,
            ShowInTaskbar = false,
            ShowActivated = false,
            Left = -6000,
            Top = -6000
        };
        window.Show();
        window.UpdateLayout();
        Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Loaded);
        return window;
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, index);
            if (child is T typed)
            {
                yield return typed;
            }

            foreach (T nested in Descendants<T>(child))
            {
                yield return nested;
            }
        }
    }

    /// <summary>视觉树尚未实现（模板未应用）时的兜底：逻辑树能看到构造函数里挂上的节点。</summary>
    private static IEnumerable<T> LogicalDescendants<T>(DependencyObject root) where T : DependencyObject
    {
        foreach (object? child in LogicalTreeHelper.GetChildren(root))
        {
            if (child is not DependencyObject child2)
            {
                continue;
            }

            if (child2 is T typed)
            {
                yield return typed;
            }

            foreach (T nested in LogicalDescendants<T>(child2))
            {
                yield return nested;
            }
        }
    }

    private static string TextOf(ContentControl control) => control.Content switch
    {
        string text => text,
        TextBlock block => block.Text,
        null => string.Empty,
        _ => control.Content.ToString() ?? string.Empty
    };

    /// <summary>
    /// 构造一次 Drop 事件。<c>DragEventArgs</c> 的可用构造函数在各 .NET 版本上可见性不同，
    /// 所以按反射取第一个五参构造，取不到就返回 null（由调用方记成「拖放未验证」而不是假装过了）。
    /// </summary>
    private static DragEventArgs? CreateDropEvent(IDataObject data, DependencyObject target)
    {
        ConstructorInfo? ctor = typeof(DragEventArgs)
            .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(c => c.GetParameters().Length == 5);
        if (ctor is null)
        {
            return null;
        }

        object? created = ctor.Invoke(new object[]
        {
            data, DragDropKeyStates.None, DragDropEffects.Copy, target, new Point(4, 4)
        });
        if (created is not DragEventArgs args)
        {
            return null;
        }

        args.RoutedEvent = System.Windows.DragDrop.DropEvent;
        return args;
    }

    /// <summary>
    /// 「已纳入只读政策」= <see cref="DocumentTools.App.Ui.PathInputs"/> 的四个可见契约，不看 HelpText。
    /// help text 只有 <c>ToolViewKit.PathBox</c> 这条入口会写；直接调 <c>PathInputs.ForFile</c> 的视图
    /// （如 exam.circle）同样合规，先前按 marker 判定把它们误判成「未迁移」。
    /// </summary>
    private static List<TextBox> PolicyPathBoxes(DependencyObject root)
        => Descendants<TextBox>(root)
            .Where(box => box.IsReadOnly && box.IsTabStop && box.AllowDrop)
            .ToList();

    /// <summary>占位文本：应用里的 TextBox 是 Wpf.Ui.Controls.TextBox（有 PlaceholderText），裸 WPF 类型没有，故反射读。</summary>
    private static string PlaceholderOf(TextBox box)
    {
        object? value = box.GetType().GetProperty("PlaceholderText")?.GetValue(box);
        if (value is string text && text.Length > 0)
        {
            return text;
        }

        return System.Windows.Automation.AutomationProperties.GetName(box) ?? string.Empty;
    }

    /// <summary>占位文本是路径语义的输入框（路径/目录/文件/原图/替换图/浏览器），无论有没有走 PathInputs。</summary>
    private static List<TextBox> PathLikeTextBoxes(DependencyObject root)
    {
        string[] keywords = { "路径", "目录", "文件", "原图", "替换图", "浏览器" };
        return Descendants<TextBox>(root)
            .Where(box => keywords.Any(key => PlaceholderOf(box).Contains(key, StringComparison.Ordinal)))
            .ToList();
    }

    /// <summary>
    /// 会写路径框的次要按钮。除「选择…/浏览…」外还有「打开…」（图片类页面用它选原图），
    /// 早期只认前两个词，把 exam.circle 的「打开…」漏掉，误判成「点了按钮却没写入」。
    /// </summary>
    private static List<Button> PickButtons(DependencyObject root)
        => Descendants<Button>(root)
            .Where(button =>
            {
                string text = TextOf(button);
                return text.Contains("选择", StringComparison.Ordinal)
                       || text.Contains("浏览", StringComparison.Ordinal)
                       || text.Contains("打开", StringComparison.Ordinal);
            })
            .ToList();

    /// <summary>
    /// 路径框覆盖率：占位文本是路径语义、但没走 PathInputs（因而不是只读）的输入框。
    /// 2026-11-06：exam.* 四个视图已由 circle-fix-dev / dev-exam-access 迁到 PathInputs，
    /// 这条从「已派 owner 的 SKIP」收紧成硬断言 —— 再出现普通 TextBox 就是回退，直接 FAIL 并点名视图。
    /// </summary>
    private static ExtOutcome CheckPathBoxCoverage(
        IServiceProvider provider,
        IReadOnlyList<IFeatureModule> modules)
    {
        var uncovered = new List<string>();
        int pathLike = 0;

        foreach (IFeatureModule module in modules)
        {
            string id = module.Descriptor.Id;
            FrameworkElement view;
            try
            {
                view = module.CreateView(provider);
            }
            catch
            {
                continue;
            }

            Window window = HostInWindow(view);
            try
            {
                List<TextBox> all = PathLikeTextBoxes(view);
                List<TextBox> policy = PolicyPathBoxes(view);
                List<TextBox> loose = all.Except(policy).ToList();
                if (all.Count == 0)
                {
                    continue;
                }

                pathLike += all.Count;
                if (loose.Count > 0)
                {
                    uncovered.Add($"{id}({loose.Count}/{all.Count})");
                }
            }
            finally
            {
                window.Close();
            }
        }

        Require(pathLike > 0, "没有找到任何路径语义的输入框（夹具扫描失效？）");
        Require(
            uncovered.Count == 0,
            $"{pathLike} 个路径语义输入框里有 {uncovered.Count} 个视图仍是普通 TextBox（未走 PathInputs，可编辑）："
            + string.Join("、", uncovered)
            + "；要求 = PathInputs 同款（IsReadOnly + IsTabStop + AllowDrop + MinWidth ≥ 320 + 路径失效中文提示）");

        return new ExtOutcome(true, $"（全部 {modules.Count} 个视图）", $"{pathLike} 个路径语义输入框全部纳入只读政策",
            0, $"路径语义输入框 {pathLike} 个，全部走 PathInputs（只读 + 可拖放），未迁移视图 0 个");
    }

    /// <summary>路径框：只读 + 可 Tab 聚焦 + 可拖放 + 最小宽度；拖放与「选择…」都要真的改 Text。</summary>
    private static ExtOutcome CheckPathBoxPolicy(
        IServiceProvider provider,
        IReadOnlyList<IFeatureModule> modules,
        ScriptedDialogService dialogs)
    {
        string probe = Fixture("image", "sample.png");
        RequireFixture(probe);
        dialogs.FilePath = probe;
        dialogs.FolderPath = _testData;

        var problems = new List<string>();
        var notes = new List<string>();
        long bytes = 0;
        int totalBoxes = 0;
        int dropVerified = 0;
        int pickVerified = 0;

        foreach (IFeatureModule module in modules)
        {
            string id = module.Descriptor.Id;
            FrameworkElement view;
            try
            {
                view = module.CreateView(provider);
            }
            catch (Exception ex)
            {
                problems.Add($"{id}: CreateView 抛异常（{ex.GetType().Name}: {ex.Message}）");
                continue;
            }

            Window window = HostInWindow(view);
            try
            {
                List<TextBox> boxes = PolicyPathBoxes(view);
                totalBoxes += boxes.Count;

                foreach (TextBox box in boxes)
                {
                    if (!box.IsReadOnly)
                    {
                        problems.Add($"{id}: 路径框可编辑（IsReadOnly=false）");
                    }

                    if (!box.IsTabStop)
                    {
                        problems.Add($"{id}: 路径框不可 Tab 聚焦（IsTabStop=false）");
                    }

                    if (!box.AllowDrop)
                    {
                        problems.Add($"{id}: 路径框不接受拖放（AllowDrop=false）");
                    }

                    if (box.MinWidth < UiTokens.PathBoxMinWidth)
                    {
                        problems.Add($"{id}: 路径框最小宽度 {box.MinWidth} < {UiTokens.PathBoxMinWidth}");
                    }
                }

                if (boxes.Count > 0)
                {
                    // 拖放：真的构造 FileDrop 数据并触发 Drop，只读框必须能被程序写入。
                    TextBox first = boxes[0];
                    first.Text = string.Empty;
                    var data = new DataObject(DataFormats.FileDrop, new[] { probe });
                    DragEventArgs? drop = CreateDropEvent(data, first);
                    if (drop is null)
                    {
                        problems.Add($"{id}: 无法构造 DragEventArgs（WPF 版本差异），拖放未验证");
                    }
                    else
                    {
                        first.RaiseEvent(drop);
                        if (first.Text == probe)
                        {
                            dropVerified++;
                        }
                        else
                        {
                            problems.Add($"{id}: 拖放后路径框 Text=\"{first.Text}\"（期望 {Rel(probe)}）");
                        }
                    }

                    // 「选择…」：脚本化对话框返回夹具路径，点完必须写进路径框。
                    // 一个页面可能有多个「选择/浏览」按钮，而且先出现的往往是文件列表的「选择文件」——
                    // 它写的是输入列表，不是路径框。所以逐个点、累计「哪个框被写过」，
                    // 最后要求每个路径框都被某个按钮写过（比只点第一个更严，也不会把有序差异误判成缺陷）。
                    List<Button> picks = PickButtons(view);
                    if (picks.Count > 0)
                    {
                        // 先清空：拖放那一步已经把同一个夹具路径写进去了，不清空就分不清是谁写的。
                        foreach (TextBox box in boxes)
                        {
                            box.Text = string.Empty;
                        }

                        var written = new HashSet<TextBox>();
                        var tried = new List<string>();
                        foreach (Button pick in picks)
                        {
                            pick.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, pick));
                            window.UpdateLayout();

                            string label = TextOf(pick);
                            if (!tried.Contains(label, StringComparer.Ordinal))
                            {
                                tried.Add(label);
                            }

                            foreach (TextBox box in boxes.Where(b => !string.IsNullOrEmpty(b.Text)).ToList())
                            {
                                written.Add(box);
                                box.Text = string.Empty;
                            }

                            if (written.Count == boxes.Count)
                            {
                                break;
                            }
                        }

                        if (written.Count == boxes.Count)
                        {
                            pickVerified++;
                        }
                        else
                        {
                            problems.Add(
                                $"{id}: 依次点了「{string.Join("/", tried)}」后仍有 {boxes.Count - written.Count} 个路径框没被写入");
                        }
                    }
                    else
                    {
                        notes.Add($"{id}: 无「选择…」按钮");
                    }
                }
            }
            finally
            {
                window.Close();
            }
        }

        Require(totalBoxes > 0, "一个路径政策框都没找到（PathBox 标记失效？）");
        Require(problems.Count == 0, string.Join("；", problems.Take(6)));

        return new ExtOutcome(true, $"（全部 {modules.Count} 个视图）",
            $"{totalBoxes} 个路径框（只读/可聚焦/可拖放/最小宽 {UiTokens.PathBoxMinWidth}）", bytes,
            $"路径框 {totalBoxes} 个全部 IsReadOnly+IsTabStop+AllowDrop；拖放写入验证 {dropVerified} 个视图；"
            + $"「选择…」写入验证 {pickVerified} 个视图；notes={notes.Count}");
    }

    /// <summary>父级导航行高：真实 MainWindow 里分组头与功能项都要 ≥ UiTokens.RowMinHeight（34）。</summary>
    private static ExtOutcome CheckNavRowPolicy(IServiceProvider provider)
    {
        MainWindow window = provider.GetRequiredService<MainWindow>();
        window.WindowStyle = WindowStyle.None;
        window.ShowInTaskbar = false;
        window.ShowActivated = false;
        window.Left = -6000;
        window.Top = -6000;
        window.Show();
        // 模板与 Loaded 回调在多个 Dispatcher 优先级上排队，逐级泵到空闲再量高度。
        for (int pass = 0; pass < 4; pass++)
        {
            window.UpdateLayout();
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
        }

        try
        {
            var problems = new List<string>();
            List<CollapsibleSection> sections = Descendants<CollapsibleSection>(window).ToList();
            if (sections.Count == 0)
            {
                // 模板尚未实现时视觉树为空，退到逻辑树（BuildMenu 在构造函数里就把分组挂进了 NavHost）。
                sections = LogicalDescendants<CollapsibleSection>(window).ToList();
            }

            int headers = 0;
            foreach (CollapsibleSection section in sections)
            {
                if (section.Children.Count == 0 || section.Children[0] is not FrameworkElement header)
                {
                    continue;
                }

                headers++;
                if (header.ActualHeight < UiTokens.RowMinHeight - 0.5)
                {
                    problems.Add($"分组「{section.Title}」头高 {header.ActualHeight:F1} < {UiTokens.RowMinHeight}");
                }
            }

            List<Button> buttons = Descendants<Button>(window).ToList();
            if (buttons.Count == 0)
            {
                buttons = LogicalDescendants<Button>(window).ToList();
            }

            int navRows = 0;
            foreach (Button button in buttons)
            {
                if (button.Tag is not string tag || !button.IsVisible)
                {
                    continue;
                }

                navRows++;
                if (button.ActualHeight < UiTokens.RowMinHeight - 0.5)
                {
                    problems.Add($"功能项「{tag}」行高 {button.ActualHeight:F1} < {UiTokens.RowMinHeight}");
                }
            }

            Require(headers > 0, "MainWindow 里没有分组头（CollapsibleSection 未创建？）");
            Require(problems.Count == 0, string.Join("；", problems.Take(6)));

            return new ExtOutcome(true, "MainWindow（真实 Window，1180x900 默认尺寸）",
                $"{headers} 个分组头 / {navRows} 个功能项", 0,
                $"分组头 {headers} 个、功能项 {navRows} 个，ActualHeight 均 ≥ {UiTokens.RowMinHeight}；"
                + "（离屏 Border 宿主测不出真实行高，这里用 Show 后的布局值）");
        }
        finally
        {
            window.Close();
        }
    }

    /// <summary>路径失效：把文件删掉再点执行，状态行必须出现「文件已不存在」，而不是抛异常。</summary>
    private static ExtOutcome CheckMissingPathNotice(IServiceProvider provider, IReadOnlyList<IFeatureModule> modules)
    {
        IFeatureModule? barcode = modules.FirstOrDefault(m => m.Descriptor.Id == "code.barcode");
        Require(barcode is not null, "未找到 code.barcode 模块");

        FrameworkElement view = barcode!.CreateView(provider);
        Window window = HostInWindow(view);
        try
        {
            TextBox? path = PolicyPathBoxes(view)
                .FirstOrDefault(box => System.Windows.Automation.AutomationProperties.GetName(box)
                    .Contains("识别", StringComparison.Ordinal));
            Require(path is not null, "条码页没有「识别图片」路径框");

            string missing = Path.Combine(_outRoot, "ui.policy.missing-path", "已被删除-不存在.png");
            Directory.CreateDirectory(Path.GetDirectoryName(missing)!);
            if (File.Exists(missing))
            {
                File.Delete(missing);
            }

            path!.Text = missing;

            Button? run = Descendants<Button>(view)
                .FirstOrDefault(button => TextOf(button).Contains("识别", StringComparison.Ordinal));
            Require(run is not null, "条码页没有「识别」按钮");
            run!.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, run));
            window.UpdateLayout();

            List<string> texts = Descendants<TextBlock>(view).Select(block => block.Text).ToList();
            string? notice = texts.FirstOrDefault(text => text.Contains("文件已不存在", StringComparison.Ordinal));
            Require(notice is not null,
                $"点击执行后状态行未出现「文件已不存在」；现有文案：{string.Join(" | ", texts.Where(t => t.Length > 0).Take(4))}");

            return new ExtOutcome(true, Rel(missing), "(status)", 0,
                $"缺失路径 → 状态行：「{notice}」");
        }
        finally
        {
            window.Close();
        }
    }
}
