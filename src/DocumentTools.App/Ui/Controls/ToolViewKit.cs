using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;

namespace DocumentTools.App.Ui.Controls;

/// <summary>反馈语气：空态 / 进行中 / 成功 / 失败。状态行与按钮之外的反馈都用它统一着色。</summary>
public enum StatusTone
{
    Neutral,
    Busy,
    Success,
    Error
}

/// <summary>
/// 工具页 / 替换页 / 考评页共用的构件：卡片、字段行、状态行、主次按钮、操作条、内容区、预览框。
/// 这里是构件的**唯一实现**；Exam 目录的 ExamViewKit 只是转发的薄适配层（保留考评页的 96px 标签列与 bool 错误位）。
/// 布局口径见 docs/feature-ux-spec.md：内容在上填满，状态与操作固定在底部。
/// </summary>
/// <remarks>
/// 实例所有权约定：本类每个工厂方法**每次调用都返回新实例**，不缓存、不复用控件；唯一共享的是
/// <see cref="PlaceholderBrush"/> 这个画刷（Brush 可被多个元素同时引用，安全）。
/// 反过来，调用方传进来的 <see cref="UIElement"/> 会被挂到新建的容器上，同一个元素**只能传一次**：
/// 一个控件一旦有了逻辑父级，再被 add 到第二个容器就会抛「指定的元素已经是另一个元素的逻辑子元素」。
/// 这条在单页上看不出来，`FeatureSmoke` 在同一进程里连建 47 页时才会炸，所以重建容器时要么每次 `new`，
/// 要么挂之前先把它从旧父级摘下来。
/// </remarks>
public static class ToolViewKit
{
    private static readonly Brush PlaceholderBrush = new SolidColorBrush(Color.FromRgb(0xF7, 0xF7, 0xF7));

    /// <summary>带标题的分区卡片（等价原 GroupBox）。每次调用返回新 Border；传入的子元素必须是新控件或已从旧父级摘下的控件。</summary>
    public static Border Card(string title, params UIElement[] children)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = UiTokens.SectionTitleFontSize,
            FontWeight = FontWeights.SemiBold,
            Margin = UiTokens.SectionTitleMargin
        });

        foreach (UIElement child in children)
        {
            stack.Children.Add(child);
        }

        return new Border
        {
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = UiTokens.CardCornerRadius,
            Padding = UiTokens.CardPadding,
            Margin = UiTokens.CardMargin,
            Background = UiTokens.SurfaceBrush,
            Child = stack
        };
    }

    /// <summary>
    /// 标签 + 控件 + 可选尾部按钮（如「选择…」/「刷新」）。
    /// <para>
    /// <paramref name="stretchContent"/> 为 true 时控件吃满中间列；但**自己声明了固定 <c>Width</c> 的控件不拉伸**——
    /// 固定宽 + Stretch 在 WPF 里表现为「在宽格子里居中」，看着像没对齐。这类控件改为左对齐。
    /// 需要整组靠左的调用方可传 <c>stretchContent: false</c>；无论哪种，尾部按钮始终贴右。
    /// </para>
    /// <para>每次调用返回新的 <see cref="Grid"/>；但 <paramref name="control"/> / <paramref name="trailing"/>
    /// 会被挂进这个 Grid，同一控件实例只能传一次。</para>
    /// </summary>
    public static Grid Field(
        string label,
        UIElement control,
        UIElement? trailing = null,
        double labelWidth = 88,
        bool stretchContent = true)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, 8) };

        // Auto + MinWidth：固定宽度会把「被合并（目标）」这类长标签削边，Auto 单独用又会让各行标签列不齐。
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var caption = new TextBlock
        {
            Text = label,
            MinWidth = labelWidth,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(caption, 0);
        grid.Children.Add(caption);

        if (control is FrameworkElement element)
        {
            bool fixedWidth = !double.IsNaN(element.Width);
            element.HorizontalAlignment =
                stretchContent && !fixedWidth ? HorizontalAlignment.Stretch : HorizontalAlignment.Left;
            element.VerticalAlignment = VerticalAlignment.Center;
            if (trailing is not null)
            {
                element.Margin = new Thickness(0, 0, 8, 0);
            }
        }

        Grid.SetColumn(control, 1);
        grid.Children.Add(control);

        if (trailing is not null)
        {
            if (trailing is FrameworkElement tail)
            {
                tail.HorizontalAlignment = HorizontalAlignment.Right;
            }

            Grid.SetColumn(trailing, 2);
            grid.Children.Add(trailing);
        }

        return grid;
    }

    /// <summary>横向一行的控件组（勾选项等）。每次调用返回新 WrapPanel；子元素不能复用。</summary>
    public static WrapPanel Row(params UIElement[] items)
    {
        var panel = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        foreach (UIElement item in items)
        {
            panel.Children.Add(item);
        }

        return panel;
    }

    /// <summary>状态行：默认中性色，配合 <see cref="ShowStatus"/> 切换语气。</summary>
    public static TextBlock Status(string text)
    {
        var block = new TextBlock
        {
            Text = text,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        };

        // 读屏软件：这是"状态提示"区域，文字变化要主动播报（播报强度由 ShowStatus 按语气调整）。
        AutomationProperties.SetName(block, "状态提示");
        AutomationProperties.SetLiveSetting(block, AutomationLiveSetting.Polite);
        return block;
    }

    /// <summary>
    /// 改写状态行文字与语气。文案模板：主行写「做了什么：为什么失败。怎么修。」；异常原文（含英文/技术细节）
    /// 放进相关控件的 <c>ToolTip</c>，并在下次成功或换输入时清空，避免读到过期详情。
    /// </summary>
    public static void ShowStatus(TextBlock target, string text, StatusTone tone = StatusTone.Neutral)
    {
        // 失败/进行中要打断朗读（Assertive），成功与中性排队播报（Polite），避免一句话被吞掉。
        AutomationProperties.SetLiveSetting(
            target,
            tone is StatusTone.Error or StatusTone.Busy ? AutomationLiveSetting.Assertive : AutomationLiveSetting.Polite);

        target.Text = text;
        target.Foreground = ToneBrush(tone);
    }

    /// <summary>
    /// 语气对应的文字色。这三支颜色都是深色档：状态文字压在白/浅灰底上必须过正文对比度
    /// 4.5:1（Tomato #FF6347 只有 2.82:1、ForestGreen 4.21:1，都不达标），
    /// 所以这里取同色系的深色（红 #C62828 / 绿 #1B5E20 / 蓝 #1565C0），色相不变、可读性达标。
    /// </summary>
    public static Brush ToneBrush(StatusTone tone) => tone switch
    {
        StatusTone.Error => ErrorTextBrush,
        StatusTone.Success => SuccessTextBrush,
        StatusTone.Busy => BusyTextBrush,
        _ => UiTokens.MutedBrush
    };

    /// <summary>失败色（4.5:1 达标，勿改回 Brushes.Tomato）。</summary>
    public static Brush ErrorTextBrush { get; } = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));

    /// <summary>成功色（4.5:1 达标，勿改回 Brushes.ForestGreen）。</summary>
    public static Brush SuccessTextBrush { get; } = new SolidColorBrush(Color.FromRgb(0x1B, 0x5E, 0x20));

    /// <summary>进行中色（4.5:1 达标，勿改回 Brushes.SteelBlue）。</summary>
    public static Brush BusyTextBrush { get; } = new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0));

    /// <summary>次级说明文字（资源位置、规则说明等），不使用弹窗。</summary>
    public static TextBlock Hint(string text)
        => new()
        {
            Text = text,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 6, 0, 0)
        };

    /// <summary>主操作按钮；每次调用返回新 Button（按钮不共享，同一个按钮不能挂到两个容器里）。</summary>
    public static Button Primary(string text, Action onClick)
        => MakeButton(text, onClick, Wpf.Ui.Controls.ControlAppearance.Primary);

    /// <summary>次级操作按钮；每次调用返回新 Button。</summary>
    public static Button Secondary(string text, Action onClick)
        => MakeButton(text, onClick, Wpf.Ui.Controls.ControlAppearance.Secondary);

    /// <summary>底部操作条；主操作放第一个，其余一律次级。每次调用返回新 WrapPanel。</summary>
    public static WrapPanel ActionBar(params UIElement[] elements)
    {
        var bar = new WrapPanel();
        foreach (UIElement element in elements)
        {
            bar.Children.Add(element);
        }

        return bar;
    }

    /// <summary>内容区：可纵向滚动，不出现横向滚动。</summary>
    public static ScrollViewer FormArea(params UIElement[] blocks)
    {
        var stack = new StackPanel();
        foreach (UIElement block in blocks)
        {
            stack.Children.Add(block);
        }

        var scroll = new ScrollViewer
        {
            Content = stack,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Padding = new Thickness(0, 0, 4, 0),

            // 容器本身不进 Tab 序列：Tab 的第一个落点应该是页面里的第一个输入控件，
            // 而不是"先停在滚动区域上再按一次"。滚轮/拖动滚动仍然可用。
            IsTabStop = false
        };

        KeyboardNavigation.SetTabNavigation(scroll, KeyboardNavigationMode.Continue);
        return scroll;
    }

    /// <summary>
    /// 页面级键盘语义：Enter = 触发本页唯一主操作，Esc = 收起当前提示（绝不退出页面、不改数据）。
    /// 焦点在输入框 / 按钮 / 复选 / 下拉 / 列表项里时一律不拦——那些控件自己的 Enter/Esc 语义优先。
    /// </summary>
    public static void AttachPageKeyboard(UIElement page, Button? primary = null, Action? onEscape = null)
    {
        page.PreviewKeyDown += (_, e) =>
        {
            if (Keyboard.Modifiers != ModifierKeys.None)
            {
                return;
            }

            if (e.Key == Key.Escape)
            {
                if (onEscape is not null)
                {
                    onEscape();
                    e.Handled = true;
                }

                return;
            }

            if (e.Key != Key.Enter || primary is null || !primary.IsEnabled)
            {
                return;
            }

            if (Keyboard.FocusedElement is TextBoxBase or ButtonBase or ComboBox or TreeViewItem or ListViewItem or MenuItem)
            {
                return;
            }

            primary.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            e.Handled = true;
        };
    }

    /// <summary>需要边框的内容容器（列表 / 结果树 / 预览）。每次调用返回新 Border；<paramref name="child"/> 不能复用。</summary>
    public static Border Frame(UIElement child, double minHeight = 160)
        => new()
        {
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = UiTokens.ListCornerRadius,
            Background = UiTokens.SurfaceBrush,
            Child = child,
            MinHeight = minHeight
        };

    /// <summary>空态框：无内容时给一句可执行的下一步提示。每次调用返回新 Border（提示文字也是新建的 TextBlock）。</summary>
    public static Border Placeholder(string text, double minHeight = 160)
    {
        Border frame = Frame(PlaceholderLabel(text), minHeight);
        frame.Background = PlaceholderBrush;
        AutomationProperties.SetName(frame, "空： " + text);
        return frame;
    }

    /// <summary>
    /// 在「空态提示」与「真实内容」之间切换同一个框。
    /// 换回空态时旧 Child 会被替换掉，所以同一个 <paramref name="content"/> 实例再来一次没有问题；
    /// 但把同一个实例同时交给两个框（或再交给 <see cref="Card"/>）仍会撞逻辑父级冲突。
    /// </summary>
    public static void ShowPlaceholder(Border frame, UIElement? content, string emptyText)
    {
        frame.Child = content ?? PlaceholderLabel(emptyText);
        frame.Background = content is null ? PlaceholderBrush : UiTokens.SurfaceBrush;

        // 空 → 有内容时把可访问名称让给真实内容，避免读屏还念着空态提示。
        if (content is null)
        {
            AutomationProperties.SetName(frame, "空： " + emptyText);
        }
        else
        {
            AutomationProperties.SetName(frame, string.Empty);
        }
    }

    /// <summary>
    /// 路径类输入框：文件 / 文件夹 / 图片路径一律只读——值只能由「选择…」按钮或拖放写入，
    /// 手输的路径有一半是拼错的，报错又晚又难懂。
    /// <para>
    /// 政策实现在 <see cref="PathInputs"/>（只读 + 可聚焦 + 可拖放 + <see cref="UiTokens.PathBoxMinWidth"/> 最小宽）；
    /// 这里只补读屏名称、悬停补全长路径（框内宽度有限）与空值提示，避免同一政策在工具页里再抄一份。
    /// 用 <c>IsReadOnly</c> 而不是 <c>IsEnabled = false</c>：禁用后无法聚焦、无法选中复制。
    /// </para>
    /// </summary>
    public static TextBox PathBox(string automationName, string placeholder, bool folder = false)
    {
        TextBox box = folder ? PathInputs.ForFolder(placeholder) : PathInputs.ForFile(placeholder);

        AutomationProperties.SetName(box, automationName);
        AutomationProperties.SetHelpText(box, "只读：点右侧按钮选择，或把文件 / 文件夹拖进来。");

        string emptyHint = folder
            ? "点右侧按钮选择目录，或把文件夹拖进来。"
            : "点右侧按钮选择文件，或把文件拖进来。";
        box.ToolTip = emptyHint;
        box.TextChanged += (_, _) =>
        {
            string path = box.Text?.Trim() ?? string.Empty;
            box.ToolTip = path.Length == 0
                ? emptyHint
                // 失效文案与 PathInputs 同一句式，避免两套「文件找不到了」的说法。
                : (folder ? PathInputs.IsMissingFolder(path) : PathInputs.IsMissingFile(path))
                    ? (folder ? PathInputs.MissingFolderNotice(path) : PathInputs.MissingFileNotice(path))
                    : path;
        };

        return box;
    }

    /// <summary>数字类输入框：窄而右对齐，单位写在标签或尾部提示里（如「宽度 px」）。</summary>
    public static TextBox NumberBox(string automationName, string placeholder, double width = 90)
    {
        var box = new TextBox
        {
            Width = Math.Clamp(width, UiTokens.NumberBoxMinWidth, UiTokens.NumberBoxMaxWidth),
            TextAlignment = TextAlignment.Right,
            PlaceholderText = placeholder
        };

        AutomationProperties.SetName(box, automationName);
        return box;
    }

    public static string Value(TextBox box) => box.Text?.Trim() ?? string.Empty;

    public static void PickFile(IDialogService dialogs, TextBox target, string filter)
    {
        IReadOnlyList<string> files = dialogs.OpenFiles(filter, multiSelect: false);
        if (files.Count > 0)
        {
            target.Text = files[0];
        }
    }

    public static void PickFolder(IDialogService dialogs, TextBox target)
    {
        string? folder = dialogs.PickFolder();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            target.Text = folder;
        }
    }

    private static TextBlock PlaceholderLabel(string text)
        => new()
        {
            Text = text,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(16)
        };

    private static Button MakeButton(string text, Action onClick, Wpf.Ui.Controls.ControlAppearance appearance)
    {
        var button = new Button
        {
            Content = text,
            Appearance = appearance,
            Padding = UiTokens.ButtonPadding,
            Margin = UiTokens.ButtonGap
        };
        button.Click += (_, _) => onClick();
        return button;
    }
}
