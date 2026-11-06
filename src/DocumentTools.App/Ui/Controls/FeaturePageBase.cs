using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DocumentTools.App.Ui.Controls;

/// <summary>
/// Shared title + description chrome. Body layout follows docs/feature-ux-spec.md:
/// list/content fills the center; action/option bars dock to the bottom.
/// </summary>
public class FeaturePageBase : UserControl
{
    protected readonly DockPanel Root = new();
    protected readonly StackPanel Header = new() { Margin = new Thickness(0, 0, 0, 8) };
    protected readonly System.Windows.Controls.TextBlock TitleBlock = new();
    protected readonly System.Windows.Controls.TextBlock DescriptionBlock = new();

    protected FeaturePageBase(string title, string? description = null)
    {
        TitleBlock.Text = title;
        TitleBlock.FontSize = UiTokens.TitleFontSize;
        TitleBlock.FontWeight = FontWeights.SemiBold;

        DescriptionBlock.Text = string.IsNullOrWhiteSpace(description) ? "选择文件后执行。" : description;
        DescriptionBlock.FontSize = UiTokens.SubtitleFontSize;
        DescriptionBlock.Foreground = UiTokens.MutedBrush;
        DescriptionBlock.Margin = new Thickness(0, 4, 0, 0);
        DescriptionBlock.TextWrapping = TextWrapping.Wrap;

        Header.Children.Add(TitleBlock);
        Header.Children.Add(DescriptionBlock);
        DockPanel.SetDock(Header, Dock.Top);
        Root.Children.Add(Header);
        Content = Root;
        Background = Brushes.White;
        Padding = UiTokens.PagePadding;
    }

    protected void SetBody(UIElement body)
    {
        while (Root.Children.Count > 1)
        {
            Root.Children.RemoveAt(1);
        }

        Root.Children.Add(body);
    }

    /// <summary>
    /// Convert-list archetype: fill = list (with optional side rail), bottom = options then action bar.
    /// </summary>
    protected static Panel BuildBottomActionLayout(
        UIElement fillContent,
        UIElement? optionsBottom,
        UIElement actionBar)
    {
        // 用 Grid 而不是 DockPanel：DockPanel 的 LastChildFill 逼着内容最后加，逻辑子元素顺序就变成
        // 「操作条 → 内容」，而 WPF 的 Tab 顺序默认按逻辑顺序走，键盘焦点会先落到底部再跳回顶部。
        // Grid 允许按视觉要求「内容 → 选项 → 操作条」的顺序添加，布局结果相同、Tab 顺序天然正确。
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        layout.Children.Add(fillContent);
        Grid.SetRow(fillContent, 0);

        int row = 0;

        if (optionsBottom is not null)
        {
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            row++;
            layout.Children.Add(optionsBottom);
            Grid.SetRow(optionsBottom, row);
        }

        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        row++;
        layout.Children.Add(actionBar);
        Grid.SetRow(actionBar, row);
        return layout;
    }

    /// <summary>
    /// Tool-page archetype: content fills the center, status line and action bar dock bottom.
    /// </summary>
    protected static Panel BuildToolLayout(UIElement content, UIElement statusLine, UIElement actionBar)
        => BuildToolLayout(content, null, statusLine, actionBar);

    /// <summary>
    /// Tool-page archetype with an optional bottom options band above the status/action rows.
    /// </summary>
    protected static Panel BuildToolLayout(
        UIElement content,
        UIElement? optionsBottom,
        UIElement statusLine,
        UIElement actionBar)
    {
        var bottom = new StackPanel { Margin = new Thickness(0, 4, 0, 0) };
        bottom.Children.Add(statusLine);
        bottom.Children.Add(actionBar);
        return BuildBottomActionLayout(content, optionsBottom, bottom);
    }

    public static Border SectionCard(string title, UIElement body, Thickness? margin = null)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = UiTokens.SectionTitleFontSize,
            FontWeight = FontWeights.SemiBold,
            Margin = UiTokens.SectionTitleMargin
        });
        stack.Children.Add(body);

        return new Border
        {
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = UiTokens.CardCornerRadius,
            Padding = UiTokens.CardPadding,
            Margin = margin ?? UiTokens.CardMargin,
            Background = UiTokens.SurfaceBrush,
            Child = stack
        };
    }
}
