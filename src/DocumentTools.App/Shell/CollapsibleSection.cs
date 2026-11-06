using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DocumentTools.App.Ui;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Shell;

/// <summary>
/// 可折叠分组。WPF 自带的 Expander 在 StackPanel / ScrollViewer 里按无限高测量，
/// 模板里占剩余空间的那一行会塌成 0 高——展开后子项看不见。导航与设置页共用这个实现：
/// 头部是一个透明按钮，本体直接放进同一个 StackPanel，折叠只切 Visibility，行为可预测。
/// </summary>
/// <remarks>
/// 行高与缩进是显式给的，不靠内容撑：分组头 <see cref="UiTokens.RowMinHeight"/> 与功能项同高，
/// 图标左沿与子项图标对齐、标题左沿与子项文字对齐（都从 <see cref="UiTokens.NavRowPadding"/> 推出来），
/// 折叠箭头固定在右端 16px。父级比子级矮会让"点得到"这件事变得靠运气，所以这里不给自动高度留余地。
/// </remarks>
public sealed class CollapsibleSection : StackPanel
{
    private readonly Button _header;
    private readonly DockPanel _row = new() { LastChildFill = true };
    private readonly SymbolIcon _chevron;
    private readonly TextBlock _title;
    private readonly TextBlock _trailing;
    private readonly StackPanel _body = new();
    private UIElement? _glyph;
    private SymbolRegular? _glyphSymbol;
    private bool _expanded = true;

    public CollapsibleSection()
    {
        // 箭头放在右端（Windows 导航控件的习惯），这样左边的图标/文字能与子项严格对齐。
        _chevron = new SymbolIcon(SymbolRegular.ChevronDown24)
        {
            FontSize = 16,
            Width = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(UiTokens.SpaceXs, 0, 0, 0)
        };

        _title = new TextBlock
        {
            FontSize = UiTokens.SectionTitleFontSize,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        _trailing = new TextBlock
        {
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, UiTokens.SpaceXs, 0),
            Visibility = Visibility.Collapsed
        };

        DockPanel.SetDock(_chevron, Dock.Right);
        _row.Children.Add(_chevron);
        DockPanel.SetDock(_trailing, Dock.Right);
        _row.Children.Add(_trailing);
        _row.Children.Add(_title);

        _header = new Button
        {
            Content = _row,
            Appearance = ControlAppearance.Transparent,
            Cursor = Cursors.Hand,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            VerticalContentAlignment = VerticalAlignment.Center,
            CornerRadius = UiTokens.ListCornerRadius,
            // 显式行高与内边距：父级节点不许比子项矮，也不许靠内容撑出高度。
            MinHeight = UiTokens.RowMinHeight,
            Padding = UiTokens.GroupHeaderPadding,
            Margin = new Thickness(0, 0, 0, 2)
        };
        _header.Click += (_, _) => IsExpanded = !IsExpanded;
        // 悬停用浅一档的 Tertiary（#4D），按下用更深一档的 Secondary（#80）——
        // 顺序反了会出现「按下比悬停更淡」的反直觉反馈，实测过才知道这两个键的 alpha 是 0x4D / 0x80。
        _header.MouseEnter += (_, _) => SetHeaderFill("ControlFillColorTertiaryBrush");
        _header.MouseLeave += (_, _) => ClearHeaderFill();
        _header.PreviewMouseLeftButtonDown += (_, _) => SetHeaderFill("ControlFillColorSecondaryBrush");
        _header.PreviewMouseLeftButtonUp += (_, _) => SetHeaderFill("ControlFillColorTertiaryBrush");

        _title.Text = "分组";
        AutomationProperties.SetName(_header, "分组");
        AutomationProperties.SetHelpText(_header, "当前已展开，按回车收起");

        Children.Add(_header);
        Children.Add(_body);
    }

    /// <summary>分类图标（可选；设置页的启动提示条只显示文字）。</summary>
    public SymbolRegular? Glyph
    {
        get => _glyphSymbol;
        set
        {
            if (_glyph is not null)
            {
                _row.Children.Remove(_glyph);
                _glyph = null;
            }

            _glyphSymbol = value;
            if (value is null)
            {
                return;
            }

            // 图标左沿与子项图标同起点（左内边距 10），右侧留 12 让标题落在子项文字的左沿。
            UIElement icon = FeatureIcons.Create(value.Value, FeatureIcons.GroupIconSize, UiTokens.SpaceMd);
            if (icon is FrameworkElement element)
            {
                element.VerticalAlignment = VerticalAlignment.Center;
            }

            _row.Children.Insert(0, icon);
            _glyph = icon;
        }
    }

    public string Title
    {
        get => _title.Text;
        set
        {
            _title.Text = value;
            // 头部内容不是纯文本，读屏/自动化需要一个稳定的名称；空标题退化成「分组」
            AutomationProperties.SetName(_header, string.IsNullOrWhiteSpace(value) ? "分组" : value);
        }
    }

    /// <summary>右侧计数/补充文字，空串时自动隐藏。</summary>
    public string Trailing
    {
        get => _trailing.Text;
        set
        {
            _trailing.Text = value;
            _trailing.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>头部用弱化样式（设置页的提示条用这种，别让提示抢主操作的视线）。</summary>
    public bool QuietHeader
    {
        get => _title.FontWeight != FontWeights.SemiBold;
        set
        {
            _title.FontWeight = value ? FontWeights.Normal : FontWeights.SemiBold;
            _title.Foreground = value ? UiTokens.MutedBrush : null;
        }
    }

    public StackPanel Body => _body;

    public bool IsExpanded
    {
        get => _expanded;
        set
        {
            if (_expanded == value)
            {
                return;
            }

            _expanded = value;
            _body.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            _chevron.Symbol = value ? SymbolRegular.ChevronDown24 : SymbolRegular.ChevronRight24;
            AutomationProperties.SetHelpText(
                _header,
                value ? "当前已展开，按回车收起" : "当前已收起，按回车展开");
            ExpandedChanged?.Invoke(value);
        }
    }

    /// <summary>展开/收起时触发；代码里设置 <see cref="IsExpanded"/> 也会触发。</summary>
    public event Action<bool>? ExpandedChanged;

    // hover/pressed 反馈：WPF-UI 的透明按钮主题里本来有状态画刷，但分组头是自带宿主的合成控件，
    // 这里显式补一层，保证任何主题下「能点」是看得见的。取不到资源就保持透明，不猜颜色。
    // 不额外判 IsMouseOver：这些事件由鼠标本身触发，再叠一道只读属性判断只会让离屏自检
    // （合成 MouseEnter）永远测不到画刷，等于把反馈做成测不了的东西。
    private void SetHeaderFill(string resourceKey)
    {
        if (TryFindResource(resourceKey) is Brush brush)
        {
            _header.Background = brush;
        }
    }

    private void ClearHeaderFill() => _header.ClearValue(Control.BackgroundProperty);
}
