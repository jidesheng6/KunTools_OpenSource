using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Shell;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.System;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Features.Modules.SystemTools;

public static class SystemFeatureModules
{
    public static IEnumerable<IFeatureModule> CreateAll()
    {
        yield return new SettingsModule();
        yield return new SystemToolsModule();
        yield return new NetworkModule();
    }
}

internal sealed class SettingsModule : FeatureModuleBase
{
    public SettingsModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "system.settings"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new SettingsView(
            services.GetRequiredService<IAppSettings>(),
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<IFeatureCatalog>());
}

/// <summary>
/// Legacy App.config preferences. Keeps business settings except FormWidth/FormHeight/WidgetAnchorMode.
/// Layout: sections first, save actions at bottom (docs/feature-ux-spec.md).
/// </summary>
public sealed class SettingsView : FeaturePageBase
{
    private readonly IAppSettings _settings;
    private readonly IDialogService _dialogs;
    private readonly IFeatureCatalog _catalog;
    private readonly TextBlock _feedback = Caption(string.Empty);
    private readonly CheckBox _showOpenTarget = new() { Content = "转换完成后询问是否打开目标目录" };
    private readonly CheckBox _gridLines = new() { Content = "文件列表显示网格线" };
    private readonly CheckBox _highlight = new() { Content = "导航高亮当前功能" };
    // 数字参数：窄、右对齐，单位写在下方说明里（18–64 行），不跟路径框抢宽度
    private readonly TextBox _lineHeight = new()
    {
        MinWidth = UiTokens.NumberBoxMinWidth,
        MaxWidth = UiTokens.NumberBoxMaxWidth,
        TextAlignment = TextAlignment.Right,
        PlaceholderText = "如 24"
    };

    // 路径类：只读可复制，值由「浏览…」或拖放写入（见 PathInputs）
    private readonly TextBox _chrome = PathInputs.ForFile(@"如 C:\Program Files\Chrome\chrome.exe");
    private readonly TextBox _examPath = PathInputs.ForFolder(@"如 D:\考评数据");
    private readonly TextBox _mssPath = PathInputs.ForFolder(@"如 D:\考评数据\mss");

    // 隐藏功能：勾选列表替代裸 FeatureId 输入框；配置里的未知 Id 标红提示、保存时原样保留
    private readonly TextBox _featureSearch = new() { PlaceholderText = "搜索功能名称或标识" };
    private readonly StackPanel _featureRows = new();
    private readonly TextBlock _featureEmptyHint = Caption("没有匹配的功能，换个关键词试试。");
    private readonly TextBlock _unknownHint = Caption(string.Empty);
    private readonly Button _removeUnknown = new() { Visibility = Visibility.Collapsed };
    private readonly HashSet<string> _hidden = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _unknownHidden = new();

    // 开始页：下拉选择，值取 FeatureId
    private readonly ComboBox _startCombo = new()
    {
        MinWidth = 260,
        HorizontalAlignment = HorizontalAlignment.Left
    };

    // 校验问题与动作失败各占一行：两者可能同时成立，合并成一句会丢信息
    private readonly TextBlock _problems = Caption(string.Empty);
    private readonly TextBlock _actionError = Caption(string.Empty);
    // 启动修正提示：可折叠的一行说明 + 逐条明细（不用 Expander，它在 StackPanel 里会塌成 0 高）
    private readonly Border _warningHost = new();
    private readonly CollapsibleSection _warnings = new()
    {
        Title = "启动提示",
        IsExpanded = false,
        QuietHeader = true,
        Visibility = Visibility.Collapsed,
        Margin = new Thickness(0)
    };

    public SettingsView(IAppSettings settings, IDialogService dialogs, IFeatureCatalog catalog)
        : base("系统设置", "管理启动页面、功能显示、列表外观以及常用数据路径。")
    {
        _settings = settings;
        _dialogs = dialogs;
        _catalog = catalog;
        LoadFromSettings();

        _problems.Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs);
        _problems.TextWrapping = TextWrapping.Wrap;
        _problems.SetResourceReference(TextBlock.ForegroundProperty, "SystemFillColorCriticalBrush");
        _problems.Visibility = Visibility.Collapsed;

        _actionError.Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs);
        _actionError.TextWrapping = TextWrapping.Wrap;
        _actionError.SetResourceReference(TextBlock.ForegroundProperty, "SystemFillColorCriticalBrush");
        _actionError.Visibility = Visibility.Collapsed;

        var paths = new StackPanel();
        paths.Children.Add(Field("Chrome 路径", _chrome, "ChromeDriverPath", PickFile(_chrome, "可执行文件|*.exe|所有文件|*.*")));
        paths.Children.Add(Field("考评数据目录", _examPath, "Vfsoft_ExamDataPath", PickFolder(_examPath)));
        paths.Children.Add(Field("Mss 数据目录", _mssPath, "Vfsoft_MssDataPath", PickFolder(_mssPath)));

        var sections = new StackPanel
        {
            MaxWidth = UiTokens.ContentMaxWidth,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        sections.Children.Add(WarningHost());
        sections.Children.Add(Card(
            "交互",
            CheckRow(_showOpenTarget, "ShowOpenTargetDialog"),
            CheckRow(_gridLines, "ListViewShowGridLines"),
            CheckRow(_highlight, "TabPageHeighLight"),
            Field("列表行高", _lineHeight, "ListViewLineHeight · 单位：行，取值 18–64")));
        sections.Children.Add(Card(
            "启动与隐藏",
            Field("启动功能", _startCombo, "原 StartPageIndex · 启动时自动打开的页面，下次启动生效"),
            HiddenFeatureField()));
        sections.Children.Add(Card("路径", paths));
        sections.Children.Add(new TextBlock
        {
            Text = $"配置文件：{AppSettings.StorePath}",
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0)
        });

        var bar = new WrapPanel { Margin = new Thickness(0, UiTokens.SpaceSm, 0, 0) };
        bar.Children.Add(Btn("保存设置", ControlAppearance.Primary, Save));
        bar.Children.Add(Btn("重新加载", ControlAppearance.Secondary, () =>
        {
            SettingsLoadResult load = _settings.Reload();
            LoadFromSettings();
            RefreshLoadWarnings();
            RefreshValidation();
            _feedback.Text = load.Message;
        }));
        bar.Children.Add(Btn("导出设置…", ControlAppearance.Secondary, ExportSettings));
        bar.Children.Add(Btn("导入设置…", ControlAppearance.Secondary, ImportSettings));
        bar.Children.Add(Btn("打开配置文件", ControlAppearance.Secondary, OpenStoreFile));

        var bottom = new StackPanel
        {
            Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0),
            MaxWidth = UiTokens.ContentMaxWidth,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        bottom.Children.Add(_problems);
        bottom.Children.Add(_actionError);
        bottom.Children.Add(_feedback);
        bottom.Children.Add(bar);

        var fill = new ScrollViewer
        {
            Content = sections,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        Panel layout = BuildBottomActionLayout(fill, optionsBottom: null, actionBar: bottom);

        // 布局内部已按「内容 → 操作条」的逻辑顺序添加（Grid 而非 DockPanel），
        // 所以这里不需要再手工设 TabIndex —— 容器上设 TabIndex 也不会传给里面的按钮。

        SetBody(layout);

        RefreshLoadWarnings();
        RefreshValidation();
    }

    /// <summary>启动修正提示：LastLoadWarnings 非空时才出现，默认收起，展开逐条。</summary>
    private UIElement WarningHost()
    {
        _warnings.Body.Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0);

        _warningHost.Child = _warnings;
        _warningHost.Background = UiTokens.SubtleBrush;
        _warningHost.BorderBrush = UiTokens.DividerBrush;
        _warningHost.BorderThickness = new Thickness(1);
        _warningHost.CornerRadius = UiTokens.ListCornerRadius;
        _warningHost.Padding = new Thickness(UiTokens.SpaceMd, UiTokens.SpaceSm, UiTokens.SpaceMd, UiTokens.SpaceSm);
        _warningHost.Margin = new Thickness(0, 0, 0, UiTokens.SpaceMd);
        _warningHost.Visibility = Visibility.Collapsed;
        return _warningHost;
    }

    private void RefreshLoadWarnings()
    {
        IReadOnlyList<string> warnings = _settings.LastLoadWarnings;
        _warnings.Body.Children.Clear();

        if (warnings.Count == 0)
        {
            _warningHost.Visibility = Visibility.Collapsed;
            return;
        }

        foreach (string warning in warnings)
        {
            TextBlock line = Caption("· " + warning);
            line.Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs);
            _warnings.Body.Children.Add(line);
        }

        _warnings.Title = $"启动时修正了 {warnings.Count} 项异常设置";
        _warnings.Trailing = "点此展开";
        _warnings.Visibility = Visibility.Visible;
        _warningHost.Visibility = Visibility.Visible;
    }

    /// <summary>校验提示：只列「非法」与「指向不存在」，未设置属于正常，不吵人。</summary>
    private void RefreshValidation()
    {
        SettingsValidationReport report = _settings.Validate();
        var problems = new List<string>();
        bool hasInvalid = false;

        foreach (SettingsFieldStatus field in report.Problems)
        {
            if (field.State is SettingsValueState.Invalid or SettingsValueState.Missing)
            {
                hasInvalid |= field.State == SettingsValueState.Invalid;
                problems.Add($"{FriendlyField(field.Field)}：{Reason(field)}");
            }
        }

        if (problems.Count == 0)
        {
            ClearProblems();
            return;
        }

        ShowProblems((hasInvalid ? "有设置项不合法：" : "有设置项目前不可用：") + string.Join("；", problems));
    }

    private static string FriendlyField(string field) => field switch
    {
        "VfsoftExamDataPath" => "考评数据目录",
        "VfsoftMssDataPath" => "Mss 数据目录",
        "ChromeDriverPath" => "Chrome 路径",
        _ => field
    };

    /// <summary>字段说明自带字段名，这里去掉前缀，避免「考评数据目录：VfsoftExamDataPath …」重复。</summary>
    private static string Reason(SettingsFieldStatus field)
    {
        string text = field.Text;
        if (text.StartsWith(field.Field, StringComparison.Ordinal))
        {
            text = text[field.Field.Length..].TrimStart('：', ':', ' ');
        }

        return text.Length == 0 ? "需要确认" : text;
    }

    private void ShowProblems(string text)
    {
        _problems.Text = text;
        _problems.Visibility = Visibility.Visible;
    }

    private void ClearProblems()
    {
        _problems.Text = string.Empty;
        _problems.Visibility = Visibility.Collapsed;
    }

    /// <summary>保存/导出/导入这类动作的失败单独一行，不覆盖校验提示。</summary>
    private void ShowActionError(string text)
    {
        _actionError.Text = text;
        _actionError.Visibility = Visibility.Visible;
    }

    private void ClearActionError()
    {
        _actionError.Text = string.Empty;
        _actionError.Visibility = Visibility.Collapsed;
    }

    private void ExportSettings()
    {
        string? target = _dialogs.SaveFile("JSON|*.json", "DocumentTools-settings.json");
        if (string.IsNullOrWhiteSpace(target))
        {
            return;
        }

        SettingsExportResult result = _settings.ExportTo(target);
        if (result.Succeeded)
        {
            ClearActionError();
            _feedback.Text = result.Message;
            return;
        }

        ShowActionError("导出失败：" + result.Message);
        _feedback.Text = "设置没有导出，请换一个可写入的位置再试。";
    }

    private void ImportSettings()
    {
        IReadOnlyList<string> files = _dialogs.OpenFiles("JSON|*.json", multiSelect: false);
        if (files.Count == 0)
        {
            return;
        }

        SettingsImportResult result = _settings.ImportFrom(files[0]);
        if (!result.Succeeded)
        {
            IReadOnlyList<SettingsFieldResult> rejected = result.RejectedFields;
            string detail = rejected.Count == 0
                ? string.Empty
                : " 被拒绝的字段：" + string.Join(
                    "；",
                    rejected.Select(f => $"{FriendlyField(f.Field)}（{f.Message}）"));

            ShowActionError(result.Message + detail);
            _feedback.Text = "当前设置保持不变，界面上的改动尚未保存。";
            return;
        }

        int applied = result.AppliedFields.Count;
        int defaulted = result.Fields.Count(f => f.Outcome == SettingsImportFieldOutcome.Defaulted);
        int ignored = result.Fields.Count(f => f.Outcome == SettingsImportFieldOutcome.Ignored);

        ClearActionError();
        LoadFromSettings();
        RefreshLoadWarnings();
        RefreshValidation();
        _feedback.Text = $"导入完成：应用 {applied} 项，默认化 {defaulted} 项，忽略 {ignored} 项。{result.Message}";
    }

    private void LoadFromSettings()
    {
        var s = _settings.Current;
        _showOpenTarget.IsChecked = s.ShowOpenTargetDialog;
        _gridLines.IsChecked = s.ListViewShowGridLines;
        _highlight.IsChecked = s.HighlightActiveNav;
        _lineHeight.Text = s.ListViewLineHeight.ToString();
        _chrome.Text = s.ChromeDriverPath;
        _examPath.Text = s.VfsoftExamDataPath;
        _mssPath.Text = s.VfsoftMssDataPath;
        LoadHiddenFeatures(s);
        LoadStartFeature(s);
    }

    /// <summary>隐藏功能：列表来自功能目录，未知 Id 单独记录以便标红保留。</summary>
    private UIElement HiddenFeatureField()
    {
        _featureSearch.TextChanged += (_, _) => RebuildFeatureRows();
        _featureSearch.Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs);

        var list = new Border
        {
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = UiTokens.ListCornerRadius,
            Background = UiTokens.SurfaceBrush,
            Padding = new Thickness(UiTokens.SpaceSm, UiTokens.SpaceXs, UiTokens.SpaceSm, UiTokens.SpaceXs),
            Child = new ScrollViewer
            {
                Content = _featureRows,
                MaxHeight = 200,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            }
        };

        _featureEmptyHint.Visibility = Visibility.Collapsed;
        _featureEmptyHint.Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0);

        _unknownHint.Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0);
        _unknownHint.SetResourceReference(TextBlock.ForegroundProperty, "SystemFillColorCriticalBrush");
        _removeUnknown.Content = "移除这些未知 Id";
        _removeUnknown.Appearance = ControlAppearance.Secondary;
        _removeUnknown.Padding = UiTokens.ButtonPadding;
        _removeUnknown.CornerRadius = UiTokens.ListCornerRadius;
        _removeUnknown.Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0);
        _removeUnknown.HorizontalAlignment = HorizontalAlignment.Left;
        _removeUnknown.Click += (_, _) =>
        {
            _unknownHidden.Clear();
            UpdateUnknownHint();
        };

        TextBlock note = Caption("勾选即隐藏，取消勾选即恢复；隐藏的功能不出现在侧边栏与首页。");
        note.Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0);

        var stack = new StackPanel();
        stack.Children.Add(_featureSearch);
        stack.Children.Add(list);
        stack.Children.Add(_featureEmptyHint);
        stack.Children.Add(_unknownHint);
        stack.Children.Add(_removeUnknown);
        stack.Children.Add(note);

        return Field("隐藏功能", stack, "原 HidePageIndex");
    }

    private void LoadHiddenFeatures(AppSettings s)
    {
        _hidden.Clear();
        _unknownHidden.Clear();

        var known = new HashSet<string>(
            _catalog.All.Select(f => f.Id),
            StringComparer.OrdinalIgnoreCase);

        foreach (string id in s.GetHiddenFeatureIds())
        {
            if (known.Contains(id))
            {
                _hidden.Add(id);
            }
            else
            {
                _unknownHidden.Add(id);
            }
        }

        RebuildFeatureRows();
    }

    private void LoadStartFeature(AppSettings s)
    {
        _startCombo.Items.Clear();
        foreach (FeatureDescriptor feature in SortedFeatures())
        {
            _startCombo.Items.Add(new ComboBoxItem
            {
                Content = FeatureLabel(feature),
                Tag = feature.Id
            });
        }

        string wanted = string.IsNullOrWhiteSpace(s.StartFeatureId)
            ? NavigationFallback.DefaultFeatureId
            : s.StartFeatureId.Trim();

        // 配置里的旧值可能已不在目录中：保留为可选项，避免保存时被静默改写
        if (_catalog.All.All(f => !string.Equals(f.Id, wanted, StringComparison.OrdinalIgnoreCase)))
        {
            _startCombo.Items.Add(new ComboBoxItem
            {
                Content = wanted + " · 库中不存在",
                Tag = wanted
            });
        }

        foreach (object item in _startCombo.Items)
        {
            if (item is ComboBoxItem { Tag: string id } && string.Equals(id, wanted, StringComparison.OrdinalIgnoreCase))
            {
                _startCombo.SelectedItem = item;
                return;
            }
        }

        _startCombo.SelectedIndex = 0;
    }

    private IEnumerable<FeatureDescriptor> SortedFeatures()
        => _catalog.All
            .OrderBy(f => f.Category)
            .ThenBy(f => f.Title, StringComparer.CurrentCulture);

    private static string FeatureLabel(FeatureDescriptor feature)
        => $"{feature.Title} · {feature.Id} · {CategoryLabels.For(feature.Category)}";

    private void RebuildFeatureRows()
    {
        _featureRows.Children.Clear();
        string query = _featureSearch.Text?.Trim() ?? string.Empty;

        var matches = new List<FeatureDescriptor>();
        foreach (FeatureDescriptor feature in SortedFeatures())
        {
            if (query.Length == 0 || FeatureLabel(feature).Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(feature);
            }
        }

        foreach (FeatureDescriptor feature in matches)
        {
            _featureRows.Children.Add(FeatureCheckRow(feature));
        }

        _featureEmptyHint.Visibility = matches.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        UpdateUnknownHint();
    }

    private CheckBox FeatureCheckRow(FeatureDescriptor feature)
    {
        var box = new CheckBox
        {
            Content = new TextBlock
            {
                Text = FeatureLabel(feature),
                FontSize = UiTokens.BodyFontSize,
                TextTrimming = TextTrimming.CharacterEllipsis
            },
            IsChecked = _hidden.Contains(feature.Id),
            Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs),
            ToolTip = string.IsNullOrWhiteSpace(feature.Description) ? feature.Title : feature.Description
        };

        string id = feature.Id;
        box.Checked += (_, _) => _hidden.Add(id);
        box.Unchecked += (_, _) => _hidden.Remove(id);
        return box;
    }

    private void UpdateUnknownHint()
    {
        bool hasUnknown = _unknownHidden.Count > 0;
        _unknownHint.Text = hasUnknown
            ? "当前版本中找不到这些功能标识：" + string.Join("、", _unknownHidden) + "。保存时会原样保留。"
            : string.Empty;
        _unknownHint.Visibility = hasUnknown ? Visibility.Visible : Visibility.Collapsed;
        _removeUnknown.Content = $"移除这些未知 Id（{_unknownHidden.Count}）";
        _removeUnknown.Visibility = hasUnknown ? Visibility.Visible : Visibility.Collapsed;
    }

    private List<string> HiddenIdsForSave()
    {
        // 已知功能按目录顺序写回，未知 Id 追加在末尾，保持配置字符串稳定可 diff
        var ids = new List<string>();
        foreach (FeatureDescriptor feature in _catalog.All)
        {
            if (_hidden.Contains(feature.Id))
            {
                ids.Add(feature.Id);
            }
        }

        ids.AddRange(_unknownHidden);
        return ids;
    }

    private void Save()
    {
        var s = _settings.Current;
        s.ShowOpenTargetDialog = _showOpenTarget.IsChecked == true;
        s.ListViewShowGridLines = _gridLines.IsChecked == true;
        s.HighlightActiveNav = _highlight.IsChecked == true;
        if (int.TryParse(_lineHeight.Text, out int h))
        {
            s.ListViewLineHeight = Math.Clamp(h, 18, 64);
        }

        s.ChromeDriverPath = _chrome.Text?.Trim() ?? string.Empty;
        s.VfsoftExamDataPath = _examPath.Text?.Trim() ?? string.Empty;
        s.VfsoftMssDataPath = _mssPath.Text?.Trim() ?? string.Empty;
        s.HideFeatureIds = string.Join(",", HiddenIdsForSave());

        // 路径框是只读的，值只会被「浏览…」或拖放替换；但文件/目录可能在选中之后被外部删掉或改名。
        // 这种时候要当场说清是哪条路径失效，而不是把死路径写进配置、等用户在功能页撞底层异常。
        if (PathInputs.IsMissingFile(s.ChromeDriverPath))
        {
            ShowActionError(PathInputs.MissingFileNotice(s.ChromeDriverPath));
            _feedback.Text = "设置没有写入配置文件，请重新选择这条路径后再保存。";
            return;
        }

        if (PathInputs.IsMissingFolder(s.VfsoftExamDataPath))
        {
            ShowActionError(PathInputs.MissingFolderNotice(s.VfsoftExamDataPath));
            _feedback.Text = "设置没有写入配置文件，请重新选择这条路径后再保存。";
            return;
        }

        if (PathInputs.IsMissingFolder(s.VfsoftMssDataPath))
        {
            ShowActionError(PathInputs.MissingFolderNotice(s.VfsoftMssDataPath));
            _feedback.Text = "设置没有写入配置文件，请重新选择这条路径后再保存。";
            return;
        }

        string? selected = (_startCombo.SelectedItem as ComboBoxItem)?.Tag as string;
        s.StartFeatureId = string.IsNullOrWhiteSpace(selected) ? NavigationFallback.DefaultFeatureId : selected.Trim();

        // 先体检再写盘：问题行在保存前后都反映当前编辑值
        RefreshValidation();
        SettingsSaveResult saved = _settings.Save();
        _lineHeight.Text = s.ListViewLineHeight.ToString();

        if (!saved.Succeeded)
        {
            ShowActionError($"保存失败：{saved.Message}");
            _feedback.Text = "设置没有写入配置文件，界面上的改动只在本窗口内有效。";
            return;
        }

        ClearActionError();

        var notes = new List<string>();
        if (_unknownHidden.Count > 0)
        {
            notes.Add($"{_unknownHidden.Count} 个未知功能标识已原样保留");
        }

        if (_hidden.Contains(s.StartFeatureId))
        {
            notes.Add($"开始页「{s.StartFeatureId}」同时被隐藏，启动时会回到首页");
        }

        _feedback.Text = notes.Count == 0
            ? $"已保存并应用：隐藏 {_hidden.Count} 项，开始页 {s.StartFeatureId}。导航与已打开页面的列表样式已同步更新。"
            : "已保存，但要注意：" + string.Join("；", notes) + "。";
    }

    private void OpenStoreFile()
    {
        try
        {
            string path = AppSettings.StorePath;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            if (!File.Exists(path))
            {
                _settings.Save();
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _dialogs.Error($"打不开配置文件：{ex.Message}");
        }
    }

    private Action? PickFile(TextBox box, string filter)
        => () =>
        {
            var files = _dialogs.OpenFiles(filter, multiSelect: false);
            if (files.Count > 0)
            {
                box.Text = files[0];
            }
        };

    private Action? PickFolder(TextBox box)
        => () =>
        {
            var folder = _dialogs.PickFolder();
            if (!string.IsNullOrWhiteSpace(folder))
            {
                box.Text = folder;
            }
        };

    /// <summary>一行设置：左标签，右侧控件占满，关键项名以弱化小字跟在控件下方。</summary>
    private static Grid Field(string label, UIElement control, string? note = null, Action? browse = null)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, UiTokens.SpaceSm) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(UiTokens.FieldLabelWidth) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var caption = new TextBlock
        {
            Text = label,
            FontSize = UiTokens.BodyFontSize,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetColumn(caption, 0);
        grid.Children.Add(caption);

        var stack = new StackPanel();
        stack.Children.Add(control);
        if (!string.IsNullOrWhiteSpace(note))
        {
            TextBlock noteBlock = Caption(note!);
            noteBlock.Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0);
            stack.Children.Add(noteBlock);
        }

        Grid.SetColumn(stack, 1);
        grid.Children.Add(stack);

        if (browse is not null)
        {
            var button = Btn("浏览…", ControlAppearance.Secondary, browse);
            button.Margin = new Thickness(UiTokens.SpaceSm, 0, 0, 0);
            button.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(button, 2);
            grid.Children.Add(button);
        }

        return grid;
    }

    private static Grid CheckRow(CheckBox box, string keyName)
    {
        box.Margin = new Thickness(0, 0, 0, 6);

        var grid = new Grid { Margin = new Thickness(0, 0, 0, 2) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Grid.SetColumn(box, 0);
        grid.Children.Add(box);

        TextBlock key = Caption(keyName);
        key.Margin = new Thickness(UiTokens.SpaceSm, 0, 0, 0);
        key.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(key, 1);
        grid.Children.Add(key);

        return grid;
    }

    private static TextBlock Caption(string text)
        => new()
        {
            Text = text,
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap
        };

    private static Border Card(string title, params UIElement[] children)
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

    private static Button Btn(string text, ControlAppearance appearance, Action click)
    {
        var btn = new Button
        {
            Content = text,
            Appearance = appearance,
            Margin = UiTokens.ButtonGap,
            Padding = UiTokens.ButtonPadding,
            CornerRadius = UiTokens.ListCornerRadius
        };
        btn.Click += (_, _) => click();
        return btn;
    }
}

internal sealed class SystemToolsModule : FeatureModuleBase
{
    public SystemToolsModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "system.tools"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new SystemToolsView(
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<IWindowsSystemToolsService>(),
            services.GetRequiredService<IKeyboardLockService>());
}

internal sealed class NetworkModule : FeatureModuleBase
{
    public NetworkModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "system.network"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new NetworkView(
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<INetworkAdapterService>());
}

/// <summary>Legacy system tools — OS tweaks only, no file list.</summary>
public sealed class SystemToolsView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly IWindowsSystemToolsService _sys;
    private readonly IKeyboardLockService _keyboard;
    private readonly TextBlock _lockStatus = new()
    {
        Text = "键盘未锁定。",
        FontSize = UiTokens.CaptionFontSize,
        Foreground = UiTokens.MutedBrush,
        Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0)
    };

    public SystemToolsView(
        IDialogService dialogs,
        IWindowsSystemToolsService sys,
        IKeyboardLockService keyboard)
        : base("系统工具", "系统配置、快捷方式与键盘分组操作；写系统项时以管理员身份运行更稳妥。")
    {
        _dialogs = dialogs;
        _sys = sys;
        _keyboard = keyboard;

        var pauseDays = new ComboBox { MinWidth = 140, IsEditable = false };
        // Legacy PauseUpdateYears RealValue = days (Util.SetPauseUpdateTime).
        var choices = new (string Label, int Days)[]
        {
            ("5年", 365 * 5),
            ("10年", 365 * 10),
            ("15年", 365 * 15),
            ("20年", 365 * 20),
            ("30年", 365 * 30),
            ("50年", 365 * 50),
            ("80年", 365 * 80),
            ("100年", 365 * 100)
        };
        foreach (var c in choices)
        {
            pauseDays.Items.Add(new ComboBoxItem { Content = c.Label, Tag = c.Days });
        }

        pauseDays.SelectedIndex = 0;

        var updateCard = Card(
            "Windows 更新",
            () =>
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal };
                row.Children.Add(new TextBlock
                {
                    Text = "最长暂停",
                    FontSize = UiTokens.BodyFontSize,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, UiTokens.SpaceSm, 0)
                });
                row.Children.Add(pauseDays);
                row.Children.Add(Action("设置暂停时间", ControlAppearance.Primary, () =>
                {
                    int days = pauseDays.SelectedItem is ComboBoxItem { Tag: int d } ? d : 365 * 5;
                    RunAdmin(() =>
                    {
                        _sys.SetPauseUpdateDays(days);
                        _dialogs.Info($"已设置 FlightSettingsMaxPauseDays = {days}。请到 Windows 更新中手动暂停。");
                    });
                }, marginLeft: 12));
                row.Children.Add(Action("打开更新设置", ControlAppearance.Secondary, () =>
                {
                    try
                    {
                        _sys.OpenWindowsUpdateSettings();
                    }
                    catch (Exception ex)
                    {
                        _dialogs.Error(ex.Message);
                    }
                }, marginLeft: 8));
                return row;
            },
            Caption("改完天数后，还需要到 Windows 更新里手动点一次暂停。"),
            adminHint());

        var shortcutCard = Card(
            "快捷方式小箭头",
            () =>
            {
                var panel = new WrapPanel();
                panel.Children.Add(Action("隐藏箭头", ControlAppearance.Secondary, () =>
                    RunAdmin(() =>
                    {
                        _sys.HideShortcutArrow();
                        _dialogs.Info("已隐藏快捷方式箭头（可能需注销/重启资源管理器后生效）。");
                    })));
                panel.Children.Add(Action("恢复箭头", ControlAppearance.Secondary, () =>
                    RunAdmin(() =>
                    {
                        _sys.RestoreShortcutArrow();
                        _dialogs.Info("已恢复快捷方式箭头。");
                    })));
                return panel;
            },
            Caption("隐藏后需要注销或重启资源管理器才会生效。"));

        var keyboardCard = Card(
            "键盘工具",
            () =>
            {
                var panel = new WrapPanel();
                panel.Children.Add(Action("关闭粘滞键", ControlAppearance.Caution, () =>
                {
                    try
                    {
                        _sys.DisableStickyKeys();
                        _dialogs.Info("已关闭粘滞键。");
                    }
                    catch (Exception ex)
                    {
                        _dialogs.Error(ex.Message);
                    }
                }));
                panel.Children.Add(Action("键盘辅助设置", ControlAppearance.Secondary, () =>
                {
                    try
                    {
                        _sys.OpenKeyboardEaseOfAccess();
                    }
                    catch (Exception ex)
                    {
                        _dialogs.Error(ex.Message);
                    }
                }));
                // legacy TabPage_26 groupBox26：锁定后吞掉全部按键，只能用鼠标点「解锁键盘」恢复。
                panel.Children.Add(Action("锁定键盘", ControlAppearance.Caution, LockKeyboard));
                panel.Children.Add(Action("解锁键盘", ControlAppearance.Secondary, UnlockKeyboard));
                return panel;
            },
            _lockStatus,
            Caption("锁定时会吞掉所有按键（Ctrl+Alt+Del 除外，系统不允许拦截），只能用鼠标点「解锁键盘」恢复；"
                + "离开本页或退出程序会自动解锁。"),
            Caption("「键盘辅助设置」会打开系统的辅助功能键盘页，便于逐项核对。"));

        var layout = new StackPanel
        {
            MaxWidth = UiTokens.ContentMaxWidth,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        layout.Children.Add(updateCard);
        layout.Children.Add(shortcutCard);
        layout.Children.Add(keyboardCard);

        SetBody(new ScrollViewer
        {
            Content = layout,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        });

        // 离开本页必须解锁：否则用户切走页面后键盘仍被吞，且没有任何可见的解锁入口。
        Unloaded += (_, _) => _keyboard.Unlock();
    }

    private TextBlock adminHint()
    {
        TextBlock hint = Caption(_sys.IsAdministrator()
            ? "当前以管理员身份运行。"
            : "部分操作需要管理员身份重新启动本程序。");
        hint.Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0);
        return hint;
    }

    private void LockKeyboard()
    {
        try
        {
            _keyboard.Lock();
            _lockStatus.Text = "键盘已锁定：按键已被拦截，请用鼠标点「解锁键盘」。";
            _lockStatus.Foreground = Brushes.Firebrick;
        }
        catch (Exception ex)
        {
            _dialogs.Error(ex.Message);
        }
    }

    private void UnlockKeyboard()
    {
        try
        {
            _keyboard.Unlock();
            _lockStatus.Text = "键盘未锁定。";
            _lockStatus.Foreground = UiTokens.MutedBrush;
        }
        catch (Exception ex)
        {
            _dialogs.Error(ex.Message);
        }
    }

    private void RunAdmin(Action action)
    {
        try
        {
            action();
        }
        catch (UnauthorizedAccessException ex)
        {
            _dialogs.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _dialogs.Error(ex.Message);
        }
    }

    private static TextBlock Caption(string text)
        => new()
        {
            Text = text,
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap
        };

    private static Button Action(
        string text,
        ControlAppearance appearance,
        Action click,
        double marginLeft = 0)
    {
        var btn = new Button
        {
            Content = text,
            Appearance = appearance,
            Margin = new Thickness(marginLeft, 0, UiTokens.SpaceSm, 0),
            Padding = UiTokens.ButtonPadding,
            CornerRadius = UiTokens.ListCornerRadius,
            MinWidth = 120
        };
        btn.Click += (_, _) => click();
        return btn;
    }

    private static Border Card(string title, Func<UIElement> body, params UIElement[] footnotes)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = UiTokens.SectionTitleFontSize,
            FontWeight = FontWeights.SemiBold,
            Margin = UiTokens.SectionTitleMargin
        });
        stack.Children.Add(body());

        foreach (UIElement note in footnotes)
        {
            if (note is TextBlock textBlock)
            {
                textBlock.Margin = new Thickness(0, UiTokens.SpaceSm, 0, 0);
            }

            stack.Children.Add(note);
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
}

/// <summary>Legacy network settings — NIC list fills; IP editor + actions at bottom.</summary>
public sealed class NetworkView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly INetworkAdapterService _network;
    private readonly TextBlock _feedback = Caption(string.Empty);
    private readonly TextBlock _emptyHint = Caption("没有检测到可用的网络适配器：请确认本机有网卡，或以管理员身份运行后再刷新。");
    private readonly System.Windows.Controls.ListView _list;

    private readonly TextBox _ip = new() { PlaceholderText = "IP", Width = 140 };
    private readonly TextBox _mask = new() { PlaceholderText = "子网掩码", Width = 140, Margin = new Thickness(8, 0, 0, 0) };
    private readonly TextBox _gw = new() { PlaceholderText = "网关", Width = 140, Margin = new Thickness(8, 0, 0, 0) };
    private readonly TextBox _dns1 = new() { PlaceholderText = "DNS1", Width = 140, Margin = new Thickness(0, 8, 0, 0) };
    private readonly TextBox _dns2 = new() { PlaceholderText = "DNS2", Width = 140, Margin = new Thickness(8, 8, 0, 0) };

    public NetworkView(IDialogService dialogs, INetworkAdapterService network)
        : base("网卡设置", "列表在上；启用 / 禁用 / 静态 IP / DHCP 走 WMI，失败时回退 netsh。需要管理员权限。")
    {
        _dialogs = dialogs;
        _network = network;
        _list = BuildList();
        Refresh();
        _list.SelectionChanged += (_, _) => FillFromSelection();

        _emptyHint.VerticalAlignment = VerticalAlignment.Center;
        _emptyHint.HorizontalAlignment = HorizontalAlignment.Center;
        _emptyHint.TextAlignment = TextAlignment.Center;
        _emptyHint.MaxWidth = 420;
        _emptyHint.Visibility = Visibility.Collapsed;

        var listFrame = new Border
        {
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = UiTokens.ListCornerRadius,
            Child = new Grid { Children = { _list, _emptyHint } },
            MinHeight = 220
        };

        var ipRow = new StackPanel { Orientation = Orientation.Horizontal };
        ipRow.Children.Add(_ip);
        ipRow.Children.Add(_mask);
        ipRow.Children.Add(_gw);
        var dnsRow = new StackPanel { Orientation = Orientation.Horizontal };
        dnsRow.Children.Add(_dns1);
        dnsRow.Children.Add(_dns2);

        var editor = new Border
        {
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = UiTokens.CardCornerRadius,
            Padding = UiTokens.CardPadding,
            Margin = new Thickness(0, UiTokens.SpaceSm, 0, 0),
            Background = UiTokens.SurfaceBrush,
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = "IP 设置",
                        FontSize = UiTokens.SectionTitleFontSize,
                        FontWeight = FontWeights.SemiBold,
                        Margin = UiTokens.SectionTitleMargin
                    },
                    ipRow,
                    dnsRow,
                    CaptionFor("选中一张网卡会自动带出当前配置，改完点「应用静态 IP」。")
                }
            }
        };

        var bar = new WrapPanel { Margin = new Thickness(0, UiTokens.SpaceSm, 0, 0) };
        bar.Children.Add(Btn("刷新", ControlAppearance.Secondary, Refresh));
        bar.Children.Add(Btn("启用", ControlAppearance.Secondary, () => RunSelected(a =>
        {
            _network.SetEnabled(a.Description, enabled: true);
            Refresh();
            _feedback.Text = $"已请求启用「{a.Name}」。";
        })));
        bar.Children.Add(Btn("禁用", ControlAppearance.Caution, () => RunSelected(a =>
        {
            if (!_dialogs.Confirm($"确定禁用「{a.Name}」？"))
            {
                return;
            }

            _network.SetEnabled(a.Description, enabled: false);
            Refresh();
            _feedback.Text = $"已请求禁用「{a.Name}」。";
        })));
        bar.Children.Add(Btn("应用静态 IP", ControlAppearance.Primary, () => RunSelected(a =>
        {
            _network.ApplyStatic(a.Description, _ip.Text ?? "", _mask.Text ?? "", _gw.Text ?? "", _dns1.Text, _dns2.Text);
            Refresh();
            _feedback.Text = $"已为「{a.Name}」应用静态 IP。";
        })));
        bar.Children.Add(Btn("DHCP", ControlAppearance.Secondary, () => RunSelected(a =>
        {
            _network.ApplyDhcp(a.Description);
            Refresh();
            _feedback.Text = $"「{a.Name}」已切换为 DHCP。";
        })));
        bar.Children.Add(Btn("导出配置…", ControlAppearance.Secondary, ExportConfig));
        bar.Children.Add(Btn("加载配置…", ControlAppearance.Secondary, LoadConfig));
        bar.Children.Add(Btn("复制 MAC", ControlAppearance.Secondary, CopyMac));
        bar.Children.Add(Btn("复制 IPv4", ControlAppearance.Secondary, CopyIpv4));
        bar.Children.Add(Btn("打开网络连接", ControlAppearance.Secondary, OpenNcpa));

        var bottom = new StackPanel { Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0) };
        bottom.Children.Add(_feedback);
        bottom.Children.Add(bar);

        SetBody(BuildBottomActionLayout(listFrame, editor, bottom));
    }

    private System.Windows.Controls.ListView BuildList()
    {
        var list = new System.Windows.Controls.ListView
        {
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            View = new System.Windows.Controls.GridView
            {
                Columns =
                {
                    new System.Windows.Controls.GridViewColumn { Header = "名称", CellTemplate = TextCell(nameof(NetworkAdapterInfo.Name)) },
                    new System.Windows.Controls.GridViewColumn { Header = "状态", CellTemplate = TextCell(nameof(NetworkAdapterInfo.Status)) },
                    new System.Windows.Controls.GridViewColumn { Header = "IPv4", CellTemplate = TextCell(nameof(NetworkAdapterInfo.Ipv4)) },
                    new System.Windows.Controls.GridViewColumn { Header = "DHCP", CellTemplate = BoolCell(nameof(NetworkAdapterInfo.DhcpEnabled)) },
                    new System.Windows.Controls.GridViewColumn { Header = "描述", CellTemplate = TextCell(nameof(NetworkAdapterInfo.Description)) }
                }
            }
        };
        list.SizeChanged += (_, _) => LayoutColumns(list);
        return list;
    }

    /// <summary>
    /// 列宽按列表可用宽度分配：名称/状态/IPv4 取比例但各有下限，DHCP 固定窄列，
    /// 描述吃掉剩余宽度且不低于 260。GridViewColumn 没有 MinWidth，所以由代码跟随
    /// SizeChanged 计算——固定像素列会硬裁长网卡名，纯 Auto 列又永远不触发省略号。
    /// </summary>
    private static void LayoutColumns(System.Windows.Controls.ListView list)
    {
        if (list.View is not System.Windows.Controls.GridView grid || grid.Columns.Count < 5)
        {
            return;
        }

        double width = list.ActualWidth - SystemParameters.VerticalScrollBarWidth - 4;
        if (width <= 0)
        {
            return;
        }

        const double dhcpWidth = 70;
        double name = Math.Max(140, width * 0.26);
        double status = Math.Max(80, width * 0.10);
        double ipv4 = Math.Max(110, width * 0.15);
        double description = Math.Max(260, width - name - status - ipv4 - dhcpWidth);

        grid.Columns[0].Width = name;
        grid.Columns[1].Width = status;
        grid.Columns[2].Width = ipv4;
        grid.Columns[3].Width = dhcpWidth;
        grid.Columns[4].Width = description;
    }

    /// <summary>文本列模板：省略号 + ToolTip 显示完整值，列宽由 <see cref="LayoutColumns"/> 分配。</summary>
    private static DataTemplate TextCell(string path)
    {
        var text = new FrameworkElementFactory(typeof(TextBlock));
        text.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding(path));
        text.SetBinding(FrameworkElement.ToolTipProperty, new System.Windows.Data.Binding(path));
        text.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
        text.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        text.SetValue(TextBlock.MarginProperty, new Thickness(UiTokens.SpaceXs, 0, UiTokens.SpaceXs, 0));
        return new DataTemplate(typeof(NetworkAdapterInfo)) { VisualTree = text };
    }

    /// <summary>DHCP 列显示「是/否」，不再直接吐 True/False。</summary>
    private static DataTemplate BoolCell(string path)
    {
        var text = new FrameworkElementFactory(typeof(TextBlock));
        text.SetValue(TextBlock.TextProperty, "否");
        text.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        text.SetValue(TextBlock.MarginProperty, new Thickness(UiTokens.SpaceXs, 0, UiTokens.SpaceXs, 0));

        var style = new Style(typeof(TextBlock));
        var trigger = new DataTrigger
        {
            Binding = new System.Windows.Data.Binding(path),
            Value = true
        };
        trigger.Setters.Add(new Setter(TextBlock.TextProperty, "是"));
        style.Triggers.Add(trigger);
        text.SetValue(FrameworkElement.StyleProperty, style);

        return new DataTemplate(typeof(NetworkAdapterInfo)) { VisualTree = text };
    }

    private void FillFromSelection()
    {
        if (_list.SelectedItem is not NetworkAdapterInfo a)
        {
            return;
        }

        _ip.Text = a.Ipv4 ?? string.Empty;
        _mask.Text = a.SubnetMask ?? string.Empty;
        _gw.Text = a.Gateway ?? string.Empty;
        _dns1.Text = a.Dns1 ?? string.Empty;
        _dns2.Text = a.Dns2 ?? string.Empty;
    }

    private void RunSelected(Action<NetworkAdapterInfo> action)
    {
        if (_list.SelectedItem is not NetworkAdapterInfo adapter)
        {
            _dialogs.Info("请先选中一张网卡。");
            return;
        }

        try
        {
            action(adapter);
        }
        catch (Exception ex)
        {
            _dialogs.Error(ex.Message);
        }
    }

    private void Refresh()
    {
        IReadOnlyList<NetworkAdapterInfo> adapters = _network.ListAdapters();
        _list.ItemsSource = adapters;
        _emptyHint.Visibility = adapters.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        _feedback.Text = adapters.Count == 0
            ? "列表为空，点「刷新」重试。"
            : $"共 {adapters.Count} 张网卡，选中一行后可在下方修改。";
    }

    private void ExportConfig()
    {
        string? path = _dialogs.SaveFile(NetworkConfigFile.Filter, "网卡配置" + NetworkConfigFile.Extension);
        if (string.IsNullOrWhiteSpace(path))
        {
            _feedback.Text = "已取消导出。";
            return;
        }

        try
        {
            NetworkConfigFile.Save(path, new NetworkConfig(
                _ip.Text ?? string.Empty,
                _mask.Text ?? string.Empty,
                _gw.Text ?? string.Empty,
                _dns1.Text ?? string.Empty,
                _dns2.Text ?? string.Empty));
            _feedback.Text = "已导出网卡配置：" + path;
        }
        catch (Exception ex)
        {
            _dialogs.Error("导出失败：" + ex.Message);
        }
    }

    private void LoadConfig()
    {
        IReadOnlyList<string> files = _dialogs.OpenFiles(NetworkConfigFile.Filter, multiSelect: false);
        if (files.Count == 0)
        {
            _feedback.Text = "已取消加载。";
            return;
        }

        try
        {
            NetworkConfig cfg = NetworkConfigFile.Load(files[0]);
            _ip.Text = cfg.Ip;
            _mask.Text = cfg.Mask;
            _gw.Text = cfg.Gateway;
            _dns1.Text = cfg.Dns1;
            _dns2.Text = cfg.Dns2;
            _feedback.Text = "已加载网卡配置，确认后点「应用静态 IP」生效。";
        }
        catch (Exception ex)
        {
            _dialogs.Error("加载失败：" + ex.Message);
        }
    }

    private void CopyMac()
    {
        if (_list.SelectedItem is not NetworkAdapterInfo a)
        {
            _dialogs.Info("请先选中一张网卡。");
            return;
        }

        if (string.IsNullOrWhiteSpace(a.Mac))
        {
            _feedback.Text = "选中网卡没有可用的 MAC 地址。";
            return;
        }

        try
        {
            Clipboard.SetText(a.Mac);
            _feedback.Text = "已复制 MAC：" + a.Mac;
        }
        catch (Exception ex)
        {
            _dialogs.Error("复制失败：" + ex.Message);
        }
    }

    private void CopyIpv4()
    {
        string ip = _ip.Text?.Trim() ?? string.Empty;
        if (ip.Length == 0 || ip == "0.0.0.0")
        {
            _feedback.Text = "当前没有可复制的 IPv4 地址。";
            return;
        }

        try
        {
            Clipboard.SetText(ip);
            _feedback.Text = "已复制 IPv4：" + ip;
        }
        catch (Exception ex)
        {
            _dialogs.Error("复制失败：" + ex.Message);
        }
    }

    private void OpenNcpa()
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = "ncpa.cpl", UseShellExecute = true });
            _feedback.Text = "已打开网络连接。";
        }
        catch (Exception ex)
        {
            _dialogs.Error("打开失败：" + ex.Message);
        }
    }

    private static TextBlock Caption(string text)
        => new()
        {
            Text = text,
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap
        };

    private static TextBlock CaptionFor(string text)
    {
        TextBlock block = Caption(text);
        block.Margin = new Thickness(0, UiTokens.SpaceSm, 0, 0);
        return block;
    }

    private static Button Btn(string text, ControlAppearance appearance, Action click)
    {
        var btn = new Button
        {
            Content = text,
            Appearance = appearance,
            Margin = UiTokens.ButtonGap,
            Padding = UiTokens.ButtonPadding,
            CornerRadius = UiTokens.ListCornerRadius
        };
        btn.Click += (_, _) => click();
        return btn;
    }
}
