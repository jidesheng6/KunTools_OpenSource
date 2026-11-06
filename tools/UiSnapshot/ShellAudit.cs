using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentTools.App.Shell;
using Microsoft.Extensions.DependencyInjection;
// 用基类 System.Windows.Controls.Button：真实导航项是 Wpf.Ui.Controls.Button（子类），
// 自检里的旧布局是原生 Button——同一套规则要覆盖两者。
using Button = System.Windows.Controls.Button;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 壳层检查：页面快照只量「页面」，量不到「壳」。侧边栏分组一旦折叠之后内容高度变 0，
/// 页面照样全绿，但导航实际上按不动——所以这里单独对真实 MainWindow 断言。
///
/// 规则（对每个导航分组）：
/// ① 收起 / 展开各做一次真实布局，展开后的「可见按钮数」不得少于收起时；
/// ② 展开后组内每个导航按钮必须 IsVisible 且 ActualHeight ≥ <see cref="MinNavButtonHeight"/>。
///
/// 同一份规则也用于旧实现（System.Windows.Controls.Expander）的自检，见 <see cref="AuditSelfTest"/>：
/// 对一个已知有缺陷的布局必须判红，否则这条检查等于空转。
/// </summary>
internal static class ShellAudit
{
    /// <summary>
    /// 导航项最小可点击高度：分组头在 task-42 里做到 ≥34，导航项按同一尺度对齐，
    /// 低于这个值在触屏/高 DPI 下按不准（旧值 20 只能证明「有点击区域」）。
    /// </summary>
    internal const double MinNavButtonHeight = 34;

    private const double ShellWidth = 1440;
    private const double ShellHeight = 900;

    /// <summary>一个导航分组的可测外观：读/改折叠态 + 取组内按钮。</summary>
    internal sealed record NavGroup(
        string Label,
        Action<bool> SetExpanded,
        Func<IReadOnlyList<Button>> Buttons);

    internal sealed record Result(
        int Groups,
        int Buttons,
        double MinButtonHeight,
        int CollapsedVisible,
        int ExpandedVisible,
        IReadOnlyList<string> Problems);

    /// <summary>对真实 MainWindow 跑壳检查；返回问题条数（0 = 通过）。</summary>
    internal static int Run(ServiceProvider provider)
    {
        MainWindow window;
        try
        {
            window = provider.GetRequiredService<MainWindow>();
        }
        catch (Exception ex)
        {
            Console.WriteLine("SHELL|error|创建 MainWindow 失败：" + ex.Message);
            return 1;
        }

        window.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));

        if (window.Content is not UIElement chrome)
        {
            Console.WriteLine("SHELL|error|MainWindow.Content 为空");
            return 1;
        }

        // 离屏渲染需要把内容树从 Window 上摘下来，否则 Measure 会被窗口本身的尺寸/DPI 影响。
        window.Content = null;

        var host = new Border { Background = Brushes.White, Child = chrome };

        void Relayout()
        {
            host.Measure(new Size(ShellWidth, ShellHeight));
            host.Arrange(new Rect(0, 0, ShellWidth, ShellHeight));
            host.UpdateLayout();
        }

        Relayout();

        StackPanel? navHost = FindByName<StackPanel>(host, "NavHost");
        if (navHost is null)
        {
            Console.WriteLine("SHELL|error|找不到导航容器 NavHost");
            return 1;
        }

        var groups = new List<NavGroup>();
        foreach (UIElement child in navHost.Children)
        {
            if (child is CollapsibleSection section)
            {
                CollapsibleSection captured = section;
                groups.Add(new NavGroup(
                    captured.Title,
                    value => captured.IsExpanded = value,
                    () => ButtonsIn(captured)));
            }
        }

        int singleButtons = navHost.Children
            .OfType<Button>()
            .Count();

        Result result = Inspect(groups, Relayout);

        Console.WriteLine(
            "SHELL|groups=" + result.Groups.ToString()
            + "|singleButtons=" + singleButtons.ToString()
            + "|buttons=" + result.Buttons.ToString()
            + "|collapsedVisible=" + result.CollapsedVisible.ToString()
            + "|expandedVisible=" + result.ExpandedVisible.ToString()
            + "|minButtonH=" + result.MinButtonHeight.ToString("F1")
            + "|problems=" + result.Problems.Count.ToString());

        foreach (string problem in result.Problems)
        {
            Console.WriteLine("SHELL|FAIL|" + problem);
        }

        return result.Problems.Count;
    }

    /// <summary>规则本体：与具体控件无关，旧 Expander 布局也能套进来做自检。</summary>
    internal static Result Inspect(IReadOnlyList<NavGroup> groups, Action relayout)
    {
        var problems = new List<string>();
        int buttons = 0;
        int collapsedVisible = 0;
        int expandedVisible = 0;
        double minHeight = double.MaxValue;

        foreach (NavGroup group in groups)
        {
            group.SetExpanded(false);
            relayout();
            int before = CountVisible(group.Buttons());
            collapsedVisible += before;

            group.SetExpanded(true);
            relayout();
            int after = CountVisible(group.Buttons());
            expandedVisible += after;

            IReadOnlyList<Button> members = group.Buttons();
            buttons += members.Count;

            if (after < before)
            {
                problems.Add(group.Label + "：展开后可见按钮 " + after.ToString() + " 个，少于收起时的 " + before.ToString() + " 个");
            }

            foreach (Button button in members)
            {
                if (!IsEffectivelyVisible(button) || button.ActualHeight < MinNavButtonHeight)
                {
                    problems.Add(
                        group.Label + "：导航按钮「" + Label(button) + "」不可见或高度 "
                        + button.ActualHeight.ToString("F1") + " < " + MinNavButtonHeight.ToString("F1"));
                }
                else
                {
                    minHeight = Math.Min(minHeight, button.ActualHeight);
                }
            }
        }

        if (groups.Count == 0)
        {
            problems.Add("导航区没有任何分组");
        }

        if (minHeight == double.MaxValue)
        {
            minHeight = 0;
        }

        return new Result(groups.Count, buttons, minHeight, collapsedVisible, expandedVisible, problems);
    }

    private static int CountVisible(IReadOnlyList<Button> buttons)
        => buttons.Count(button => IsEffectivelyVisible(button) && button.ActualHeight >= 1);

    /// <summary>
    /// 有效可见性：离屏检查时元素没有挂到 Window 上，<see cref="UIElement.IsVisible"/> 恒为 false，
    /// 所以沿祖先链看 Visibility 更可靠（也正好能识别 Expander 收起后模板里的 Collapsed 内容）。
    /// </summary>
    private static bool IsEffectivelyVisible(DependencyObject element)
    {
        DependencyObject? node = element;
        while (node is not null)
        {
            if (node is UIElement ui && ui.Visibility != Visibility.Visible)
            {
                return false;
            }

            node = node is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(node)
                : LogicalTreeHelper.GetParent(node);
        }

        return true;
    }

    /// <summary>按钮的可读名称：优先自动化名称，其次内容里的文本。</summary>
    private static string Label(Button button)
    {
        string name = System.Windows.Automation.AutomationProperties.GetName(button);
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return TextOf(button.Content) ?? button.Name;
    }

    private static string? TextOf(object? content)
    {
        switch (content)
        {
            case null:
                return null;
            case string text:
                return text;
            case TextBlock block:
                return block.Text;
            case ContentControl control:
                return TextOf(control.Content);
            case Panel panel:
                return string.Join(" ", panel.Children.OfType<UIElement>().Select(child => child switch
                {
                    TextBlock block => block.Text,
                    ContentControl control => TextOf(control.Content),
                    Panel nested => TextOf(nested),
                    _ => null
                }).Where(part => !string.IsNullOrWhiteSpace(part)));
            default:
                return null;
        }
    }

    /// <summary>按逻辑树收集按钮：折叠态下视觉树里没有子元素，逻辑树才是稳定的。</summary>
    private static IReadOnlyList<Button> ButtonsIn(DependencyObject root)
    {
        var found = new List<Button>();
        Collect(root);
        return found;

        void Collect(DependencyObject node)
        {
            foreach (object child in LogicalTreeHelper.GetChildren(node))
            {
                if (child is not DependencyObject element)
                {
                    continue;
                }

                if (element is Button button)
                {
                    found.Add(button);
                }

                Collect(element);
            }
        }
    }

    private static T? FindByName<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        if (root is T match && string.Equals(match.Name, name, StringComparison.Ordinal))
        {
            return match;
        }

        foreach (object child in LogicalTreeHelper.GetChildren(root))
        {
            if (child is not DependencyObject element)
            {
                continue;
            }

            T? found = FindByName<T>(element, name);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
