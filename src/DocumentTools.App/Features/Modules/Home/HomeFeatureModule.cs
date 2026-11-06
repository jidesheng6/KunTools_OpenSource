using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Shell;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Features.Modules.Home;

public sealed class HomeFeatureModule : FeatureModuleBase
{
    public HomeFeatureModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "home"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new HomeView(
            services.GetRequiredService<IFeatureCatalog>(),
            services.GetRequiredService<INavigationService>(),
            services.GetServices<IFeatureModule>(),
            services.GetRequiredService<IAppSettings>());
}

/// <summary>
/// 首页 = 功能目录。每个分类一段（标题 + 计数 + 卡片），卡片本身就是主操作入口。
/// </summary>
public sealed class HomeView : FeaturePageBase
{
    private const double CardWidth = 216;
    private const double CardHeight = 88;

    public HomeView(
        IFeatureCatalog catalog,
        INavigationService navigation,
        IEnumerable<IFeatureModule> modules,
        IAppSettings settings)
        : base(AppInfo.DisplayName, "从左侧导航进入功能，或直接点下面的入口卡片。")
    {
        var implemented = new HashSet<string>(
            modules.Select(m => m.Descriptor.Id),
            StringComparer.OrdinalIgnoreCase);

        var hidden = new HashSet<string>(settings.Current.GetHiddenFeatureIds(), StringComparer.OrdinalIgnoreCase);
        var available = catalog.All
            .Where(f => implemented.Contains(f.Id) && !hidden.Contains(f.Id) && f.Id != "home")
            .GroupBy(f => f.Category)
            .OrderBy(g => g.Key)
            .ToList();

        var sections = new StackPanel
        {
            MaxWidth = UiTokens.ContentMaxWidth,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        bool first = true;
        foreach (var group in available)
        {
            sections.Children.Add(BuildSectionHeader(group.Key, group.Count(), first));
            first = false;

            var cards = new WrapPanel { Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs) };
            foreach (FeatureDescriptor feature in group)
            {
                cards.Children.Add(BuildCard(feature, navigation));
            }

            sections.Children.Add(cards);
        }

        if (available.Count == 0)
        {
            sections.Children.Add(new TextBlock
            {
                Text = "还没有可以进入的功能：功能模块未注册，或已在「系统设置」里被隐藏。",
                FontSize = UiTokens.BodyFontSize,
                Foreground = UiTokens.MutedBrush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, UiTokens.SpaceSm, 0, 0)
            });
        }

        SetBody(new ScrollViewer
        {
            Content = sections,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        });
    }

    private static UIElement BuildSectionHeader(FeatureCategory category, int count, bool first)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, first ? 0 : UiTokens.SpaceMd, 0, UiTokens.SpaceSm)
        };

        row.Children.Add(FeatureIcons.Create(FeatureIcons.ForCategory(category), FeatureIcons.GroupIconSize));
        row.Children.Add(new TextBlock
        {
            Text = CategoryLabels.For(category),
            FontSize = UiTokens.SectionTitleFontSize,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        });
        row.Children.Add(new TextBlock
        {
            Text = $"{count} 项",
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(UiTokens.SpaceSm, 1, 0, 0)
        });

        return row;
    }

    private static UIElement BuildCard(FeatureDescriptor feature, INavigationService navigation)
    {
        string description = string.IsNullOrWhiteSpace(feature.Description)
            ? CategoryLabels.For(feature.Category)
            : feature.Description;

        var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
        titleRow.Children.Add(FeatureIcons.Create(feature.Id, FeatureIcons.CardIconSize));
        titleRow.Children.Add(new TextBlock
        {
            Text = feature.Title,
            FontSize = UiTokens.BodyFontSize,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        var content = new StackPanel();
        content.Children.Add(titleRow);
        content.Children.Add(new TextBlock
        {
            Text = description,
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            Margin = new Thickness(0, 5, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        var card = new Button
        {
            Content = content,
            Appearance = ControlAppearance.Secondary,
            Width = CardWidth,
            Height = CardHeight,
            Padding = new Thickness(12, 10, 12, 10),
            Margin = new Thickness(0, 0, UiTokens.SpaceMd, UiTokens.SpaceMd),
            CornerRadius = UiTokens.CardCornerRadius,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            ToolTip = description
        };
        card.Click += (_, _) => navigation.Navigate(feature.Id);
        return card;
    }
}
