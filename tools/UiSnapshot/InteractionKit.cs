using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 交互态快照用的最小操作集：按可见文案找按钮、按占位提示找输入框、点一下、对比前后文本。
/// 全部走视觉树 + 路由事件，不需要真窗口、不需要鼠标，也不依赖 Automation（离屏视觉树没有对等控件）。
/// </summary>
internal static class InteractionKit
{
    /// <summary>按视觉树顺序枚举所有可见子元素（深度优先）。</summary>
    internal static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            yield return child;

            foreach (DependencyObject grandChild in Descendants(child))
            {
                yield return grandChild;
            }
        }
    }

    /// <summary>按钮可见文本（Label 里的文字），用于按「识别条码」这类标签定位。</summary>
    internal static string LabelOf(ContentControl control)
    {
        string text = TextOf(control.Content);
        return text.Length > 0 ? text : TextOf(control);
    }

    /// <summary>递归抽取任意内容对象里的文字。</summary>
    internal static string TextOf(object? content)
    {
        switch (content)
        {
            case null:
                return string.Empty;

            case string text:
                return text.Trim();

            case TextBlock block:
                return (block.Text ?? string.Empty).Trim();

            case AccessText access:
                return (access.Text ?? string.Empty).Trim();

            case HeaderedContentControl headered:
                return TextOf(headered.Content);

            case ContentControl contentControl:
                return TextOf(contentControl.Content);

            case Panel panel:
                return string.Join(
                    " ",
                    panel.Children.OfType<object>().Select(TextOf).Where(s => s.Length > 0));

            default:
                return string.Empty;
        }
    }

    /// <summary>按标签文本找第一个按钮；找不到返回 null（由调用方记成 MISS）。</summary>
    internal static Button? FindButton(DependencyObject root, string label)
    {
        foreach (Button button in Descendants(root).OfType<Button>())
        {
            if (LabelOf(button).Contains(label, StringComparison.Ordinal))
            {
                return button;
            }
        }

        return null;
    }

    /// <summary>按占位提示找输入框（Wpf.Ui 的 PlaceholderText 是普通 CLR 属性，用反射按运行时类型取）。</summary>
    internal static TextBox? FindTextBox(DependencyObject root, string placeholder)
    {
        var cache = new Dictionary<Type, PropertyInfo?>();
        foreach (TextBox box in Descendants(root).OfType<TextBox>())
        {
            Type type = box.GetType();
            if (!cache.TryGetValue(type, out PropertyInfo? property))
            {
                property = type.GetProperty("PlaceholderText", BindingFlags.Public | BindingFlags.Instance);
                cache[type] = property;
            }

            if (property?.GetValue(box) is string text && text.Contains(placeholder, StringComparison.Ordinal))
            {
                return box;
            }
        }

        return null;
    }

    /// <summary>触发 Click 路由事件：同步执行处理逻辑，Modal 弹窗会在这里阻塞住整个进程（由父进程超时发现）。</summary>
    internal static void Click(Button button)
        => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

    /// <summary>收集整棵树的可见文本（含输入框内容），用于点击前后做差集。</summary>
    internal static HashSet<string> CollectTexts(DependencyObject root)
    {
        var texts = new HashSet<string>(StringComparer.Ordinal);

        foreach (DependencyObject element in Descendants(root))
        {
            if (element is not UIElement { Visibility: Visibility.Visible })
            {
                continue;
            }

            switch (element)
            {
                case TextBlock block when !string.IsNullOrWhiteSpace(block.Text):
                    texts.Add(block.Text.Trim());
                    break;

                case TextBox box when !string.IsNullOrWhiteSpace(box.Text):
                    texts.Add(box.Text.Trim());
                    break;

                case ContentControl control when control.Content is string content && !string.IsNullOrWhiteSpace(content):
                    texts.Add(content.Trim());
                    break;
            }
        }

        return texts;
    }
}
