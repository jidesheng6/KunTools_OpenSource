using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Settings;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Shell;

public partial class MainWindow : FluentWindow
{
    private readonly IFeatureCatalog _catalog;
    private readonly IFeatureViewFactory _views;
    private readonly INavigationService _navigation;
    private readonly IAppSettings _settings;
    private readonly IDialogService _dialogs;
    private readonly IEnumerable<IFeatureModule> _modules;
    private readonly Dictionary<string, FrameworkElement> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Button> _navButtons = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>用户手动折叠的分类状态；搜索时不覆盖它。</summary>
    private readonly Dictionary<FeatureCategory, bool> _groupExpanded = new();
    private string? _currentId;

    public MainWindow(
        IFeatureCatalog catalog,
        IFeatureViewFactory views,
        INavigationService navigation,
        IAppSettings settings,
        IDialogService dialogs,
        IEnumerable<IFeatureModule> modules)
    {
        _catalog = catalog;
        _views = views;
        _navigation = navigation;
        _settings = settings;
        _dialogs = dialogs;
        _modules = modules;

        InitializeComponent();

        Title = $"{AppInfo.DisplayName} {AppInfo.Version}";
        TitleBar.Title = $"{AppInfo.DisplayName}  {AppInfo.Version}";
        NavVersion.Text = $"{AppInfo.DisplayName} {AppInfo.Version}";

        BuildMenu(string.Empty);
        _navigation.FeatureRequested += ActivateFeature;
        _settings.Changed += OnSettingsChanged;
        Closed += (_, _) => _settings.Changed -= OnSettingsChanged;
        PreviewKeyDown += OnPreviewKeyDown;
        Loaded += (_, _) =>
        {
            if (_currentId is null)
            {
                StartResolution start = NavigationFallback.Resolve(
                    _settings.Current.StartFeatureId,
                    _modules.Select(m => m.Descriptor.Id),
                    _settings.Current.GetHiddenFeatureIds());

                ActivateFeature(start.FeatureId);

                if (start.Notice is not null)
                {
                    NavStatus.Text = start.Notice;
                    NavStatus.Visibility = Visibility.Visible;
                }
            }
        };
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.K && Keyboard.Modifiers == ModifierKeys.Control)
        {
            SearchBox.Focus();
            SearchBox.SelectAll();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && SearchBox.IsKeyboardFocusWithin)
        {
            if (SearchBox.Text.Length > 0)
            {
                // TextChanged 会自动重建列表，这里不再重复调用。
                SearchBox.Clear();
            }
            else
            {
                BuildMenu(string.Empty);
            }

            e.Handled = true;
        }
    }

    private void SearchBox_OnTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        => BuildMenu(SearchBox.Text);

    /// <summary>搜索框里按回车直接打开第一条匹配结果。</summary>
    private void SearchBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || _navButtons.Count == 0)
        {
            return;
        }

        ActivateFeature(_navButtons.Keys.First());
        e.Handled = true;
    }

    private void BuildMenu(string? query)
    {
        NavHost.Children.Clear();
        _navButtons.Clear();

        var moduleIds = new HashSet<string>(
            _modules.Select(m => m.Descriptor.Id),
            StringComparer.OrdinalIgnoreCase);

        var missing = _catalog.All.Where(f => !moduleIds.Contains(f.Id)).Select(f => f.Id).ToList();
        if (missing.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine("Unregistered features: " + string.Join(", ", missing));
        }

        bool searching = !string.IsNullOrWhiteSpace(query);
        IEnumerable<FeatureDescriptor> features = searching
            ? _catalog.Search(query!)
            : _catalog.All;

        var hidden = new HashSet<string>(_settings.Current.GetHiddenFeatureIds(), StringComparer.OrdinalIgnoreCase);
        var groups = features
            .Where(f => moduleIds.Contains(f.Id) && !hidden.Contains(f.Id))
            .GroupBy(f => f.Category)
            .OrderBy(g => g.Key)
            .ToList();

        NavEmptyHint.Text = searching
            ? $"没有匹配「{query!.Trim()}」的功能，试试更短的关键词。"
            : "功能清单尚未注册：请检查功能模块是否已加载。";
        NavEmptyHint.Visibility = groups.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        foreach (var group in groups)
        {
            if (group.Count() == 1)
            {
                // 只有一个入口的分类（首页、关于）不再套折叠壳，避免「首页/首页」这种重复层级
                NavHost.Children.Add(CreateNavButton(group.First()));
                continue;
            }

            // 搜索时强制展开命中分组；否则恢复用户手动折叠状态（默认展开）。
            bool expanded = searching || !_groupExpanded.TryGetValue(group.Key, out bool saved) || saved;
            if (!searching)
            {
                // 只在非搜索时记录折叠状态，搜索结束后能回到用户原来的分组状态
                _groupExpanded[group.Key] = expanded;
            }

            FeatureCategory category = group.Key;
            var section = new CollapsibleSection
            {
                Title = CategoryLabels.For(category),
                Glyph = FeatureIcons.ForCategory(category),
                Trailing = group.Count().ToString(),
                IsExpanded = expanded,
                Tag = category,
                // 右侧留出滚动条覆盖的宽度，否则分组计数会被压在滚动条下面
                Margin = new Thickness(0, 0, 8, 2)
            };
            section.Body.Margin = new Thickness(UiTokens.SpaceXs, 2, 0, 4);
            foreach (var feature in group)
            {
                section.Body.Children.Add(CreateNavButton(feature));
            }

            section.ExpandedChanged += value =>
            {
                if (!searching)
                {
                    _groupExpanded[category] = value;
                }
            };

            NavHost.Children.Add(section);
        }

        // 列表重建后重新套用高亮
        if (_currentId is not null && _settings.Current.HighlightActiveNav)
        {
            HighlightNav(_currentId);
        }
    }

    /// <summary>
    /// 设置刷新边界：导航设置立即重建壳；首页是纯目录页，可安全丢弃并按新隐藏项重建；
    /// 文件列表样式直接更新缓存页里的列表控件，保留用户已经加入的文件和页面输入。
    /// 其它业务页缓存不失效。
    /// </summary>
    private void OnSettingsChanged(object? sender, SettingsChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(new Action(() => OnSettingsChanged(sender, e)));
            return;
        }

        bool navigationChanged = e.Contains("HideFeatureIds") || e.Contains("HighlightActiveNav");
        if (e.Contains("HideFeatureIds"))
        {
            RefreshHomeCache();
        }

        if (navigationChanged)
        {
            BuildMenu(SearchBox.Text);
            ApplyNavigationHighlight();
        }

        if (e.Contains("ListViewShowGridLines") || e.Contains("ListViewLineHeight"))
        {
            foreach (FrameworkElement view in _cache.Values)
            {
                foreach (BorderedFileList list in LogicalDescendants<BorderedFileList>(view))
                {
                    list.ShowGridLines = _settings.Current.ListViewShowGridLines;
                    list.RowHeight = _settings.Current.ListViewLineHeight;
                }
            }
        }
    }

    private void RefreshHomeCache()
    {
        _cache.Remove("home");
        if (!string.Equals(_currentId, "home", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        FrameworkElement? refreshed = GetOrCreate("home");
        if (refreshed is not null)
        {
            ContentHost.Content = refreshed;
        }
    }

    private void ApplyNavigationHighlight()
    {
        if (_settings.Current.HighlightActiveNav && _currentId is not null)
        {
            HighlightNav(_currentId);
            return;
        }

        foreach (Button button in _navButtons.Values)
        {
            button.Appearance = ControlAppearance.Transparent;
        }
    }

    private static IEnumerable<T> LogicalDescendants<T>(DependencyObject root) where T : DependencyObject
    {
        foreach (object child in LogicalTreeHelper.GetChildren(root))
        {
            if (child is not DependencyObject dependencyObject)
            {
                continue;
            }

            if (dependencyObject is T match)
            {
                yield return match;
            }

            foreach (T descendant in LogicalDescendants<T>(dependencyObject))
            {
                yield return descendant;
            }
        }
    }

    private Button CreateNavButton(FeatureDescriptor feature)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };
        content.Children.Add(FeatureIcons.Create(feature.Id, FeatureIcons.NavIconSize));
        content.Children.Add(new TextBlock
        {
            Text = feature.Title,
            FontSize = UiTokens.BodyFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        var button = new Button
        {
            Content = content,
            Appearance = ControlAppearance.Transparent,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            VerticalContentAlignment = VerticalAlignment.Center,
            CornerRadius = UiTokens.ListCornerRadius,
            Margin = new Thickness(0, 0, 0, 2),
            // 与分组头同一行高：父级不矮于子级，点按区域才稳定（见 UiTokens.RowMinHeight）。
            MinHeight = UiTokens.RowMinHeight,
            Padding = UiTokens.NavRowPadding,
            Tag = feature.Id,
            ToolTip = string.IsNullOrWhiteSpace(feature.Description) ? feature.Title : feature.Description
        };
        string id = feature.Id;
        // 图标 + 文本组合的内容，读屏拿到的是空名称，这里补上可读名称与稳定的自动化 Id
        AutomationProperties.SetName(button, feature.Title);
        AutomationProperties.SetAutomationId(button, feature.Id);
        button.Click += (_, _) => ActivateFeature(id);
        _navButtons[feature.Id] = button;
        return button;
    }

    public void ActivateFeature(string featureId)
    {
        // 用户开始操作后，启动提示不再有意义
        NavStatus.Text = string.Empty;
        NavStatus.Visibility = Visibility.Collapsed;

        FrameworkElement? view = GetOrCreate(featureId);
        if (view is null)
        {
            _dialogs.Info($"当前版本没有「{featureId}」这个功能入口。", "提示");
            return;
        }

        var descriptor = _catalog.All.FirstOrDefault(f =>
            string.Equals(f.Id, featureId, StringComparison.OrdinalIgnoreCase));
        UpdatePageChrome(descriptor);
        ContentHost.Content = view;
        _currentId = featureId;

        // 激活的功能必须可见：展开它所在的分类
        if (descriptor is not null)
        {
            _groupExpanded[descriptor.Category] = true;
            foreach (CollapsibleSection section in NavHost.Children.OfType<CollapsibleSection>())
            {
                if (section.Tag is FeatureCategory cat && cat == descriptor.Category)
                {
                    section.IsExpanded = true;
                }
            }
        }

        if (_settings.Current.HighlightActiveNav)
        {
            HighlightNav(featureId);
        }
    }

    /// <summary>页头面包屑只标分类，避免和页面自身标题重复占两行大标题。</summary>
    private void UpdatePageChrome(FeatureDescriptor? descriptor)
    {
        if (descriptor is null)
        {
            PageCrumb.Text = "功能";
            PageGlyph.Symbol = SymbolRegular.Apps24;
            return;
        }

        PageCrumb.Text = CategoryLabels.For(descriptor.Category);
        PageGlyph.Symbol = FeatureIcons.ForCategory(descriptor.Category);
    }

    private void HighlightNav(string featureId)
    {
        foreach (var kv in _navButtons)
        {
            kv.Value.Appearance = string.Equals(kv.Key, featureId, StringComparison.OrdinalIgnoreCase)
                ? ControlAppearance.Secondary
                : ControlAppearance.Transparent;
        }

        if (_navButtons.TryGetValue(featureId, out var active))
        {
            // 等布局完成再滚动，保证折叠分组展开后能定位到当前项
            active.Dispatcher.BeginInvoke(new Action(active.BringIntoView), DispatcherPriority.Loaded);
        }
    }

    private FrameworkElement? GetOrCreate(string featureId)
    {
        if (_cache.TryGetValue(featureId, out var cached))
        {
            return cached;
        }

        try
        {
            var created = _views.Create(featureId);
            if (created is not null)
            {
                _cache[featureId] = created;
            }

            return created;
        }
        catch (Exception ex)
        {
            _dialogs.Error($"打开功能失败：{featureId}\n\n{ex.Message}");
            return null;
        }
    }
}
