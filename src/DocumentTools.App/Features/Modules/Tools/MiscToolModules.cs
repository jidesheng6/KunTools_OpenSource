using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Assets;
using DocumentTools.Core.Services.Files;
using DocumentTools.Core.Services.Scan;
using DocumentTools.Core.Services.System;
using DocumentTools.Core.Services.Text;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Tools;

/// <summary>Twain 调试页 / 其他功能（embedded 资源释放）/ 补充功能。</summary>
public static class MiscToolModules
{
    public static IEnumerable<IFeatureModule> CreateAll()
    {
        yield return new TwainModule();
        yield return new OtherMiscModule();
        yield return new ExtraToolsModule();
    }
}

internal sealed class TwainModule : FeatureModuleBase
{
    public TwainModule() : base(BuiltInFeatures.All.First(f => f.Id == "twain.debug")) { }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new TwainDebugView(
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<ITwainScanService>());
}

/// <summary>
/// TWAIN 扫描调试页。设备缺失时给出可执行指引（检查连接、在 Windows「传真和扫描」确认可用、刷新设备），
/// 技术原因降级到卡片内的次级说明，不占用主状态行。
/// </summary>
public sealed class TwainDebugView : FeaturePageBase
{
    private const string NoScannerHint =
        "未检测到扫描仪：请检查设备连接（USB/电源），在 Windows「传真和扫描」中确认扫描仪可用；装好驱动后点「刷新设备」。";

    private readonly IDialogService _dialogs;
    private readonly ITwainScanService _twain;
    private readonly ComboBox _device = new() { MinWidth = 240, IsEditable = false };
    private readonly TextBox _path = ToolViewKit.PathBox("扫描图片保存目录", "扫描图片保存目录", folder: true);
    private readonly ComboBox _dpi = new() { MinWidth = 100, IsEditable = false };
    private readonly CheckBox _duplex = new() { Content = "双面", Margin = new Thickness(0, 0, 16, 0) };
    private readonly CheckBox _showUi = new() { Content = "显示扫描仪界面", Margin = new Thickness(0, 0, 16, 0) };
    private readonly CheckBox _skipBlank = new() { Content = "跳过空白页" };
    private readonly System.Windows.Controls.Image _preview = new() { Stretch = Stretch.Uniform, MaxHeight = 300 };
    private readonly Border _previewFrame;
    private readonly TextBlock _status = ToolViewKit.Status(
        "点「刷新设备」检测扫描仪；在「扫描配置」里选好目录后点「开始扫描」，第一页会显示在右侧预览里。");
    private readonly TextBlock _tech = ToolViewKit.Hint(string.Empty);
    private readonly TextBlock _infoDevice = Info("—");
    private readonly TextBlock _infoSession = Info("不可用");
    private readonly TextBlock _infoDirectory = Info("未选择");
    private readonly TextBlock _infoCount = Info("0 张");
    private readonly TextBlock _infoElapsed = Info("—");
    private readonly Stopwatch _scanWatch = new();
    private readonly Button _scan;

    public TwainDebugView(IDialogService dialogs, ITwainScanService twain)
        : base("Twain 扫描仪调试", "设备能力 / 扫描路径 / 预览（对齐原 TabPage_09）")
    {
        _dialogs = dialogs;
        _twain = twain;

        foreach (var d in new[] { "100", "150", "200", "300", "600" })
        {
            _dpi.Items.Add(d);
        }

        _dpi.SelectedIndex = 3;

        _previewFrame = ToolViewKit.Placeholder("扫描完成后在此显示第一页图像。", 280);

        _scan = ToolViewKit.Primary("开始扫描", StartScan);
        _scan.MinWidth = 120;

        // 读屏软件：下拉框只有「扫描仪 / 分辨率」这种卡片上的可见标签，控件本身没有名称；
        // 预览图是无文字的 Image，数据信息五项都是纯文本。
        AutomationProperties.SetName(_device, "扫描仪设备");
        AutomationProperties.SetName(_dpi, "分辨率 DPI");
        AutomationProperties.SetName(_preview, "扫描预览图");
        AutomationProperties.SetName(_infoDevice, "设备");
        AutomationProperties.SetName(_infoSession, "会话状态");
        AutomationProperties.SetName(_infoDirectory, "保存目录");
        AutomationProperties.SetName(_infoCount, "本次张数");
        AutomationProperties.SetName(_infoElapsed, "耗时");

        Border twainCard = ToolViewKit.Card(
            "TWAIN 设置",
            ToolViewKit.Field("扫描仪", _device, ToolViewKit.Secondary("刷新设备", RefreshDevices)),
            _tech);

        Border configCard = ToolViewKit.Card(
            "扫描配置",
            ToolViewKit.Field("保存目录", _path, ToolViewKit.Secondary("选择…", PickDirectory)),
            ToolViewKit.Field("分辨率 DPI", _dpi),
            ToolViewKit.Row(_duplex, _showUi, _skipBlank));

        Border infoCard = ToolViewKit.Card(
            "数据信息",
            ToolViewKit.Field("设备", _infoDevice, null, 72),
            ToolViewKit.Field("会话状态", _infoSession, null, 72),
            ToolViewKit.Field("保存目录", _infoDirectory, null, 72),
            ToolViewKit.Row(
                ToolViewKit.Field("本次张数", _infoCount, null, 68),
                ToolViewKit.Field("耗时", _infoElapsed, null, 40)));

        Border previewCard = ToolViewKit.Card("扫描图片预览", _previewFrame);
        previewCard.Margin = new Thickness(12, 0, 0, 12);

        var rightColumn = new StackPanel { VerticalAlignment = VerticalAlignment.Top };
        rightColumn.Children.Add(previewCard);
        rightColumn.Children.Add(infoCard);

        var split = new Grid();
        split.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.1, GridUnitType.Star) });
        split.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(rightColumn, 1);

        split.Children.Add(ToolViewKit.FormArea(twainCard, configCard));
        split.Children.Add(rightColumn);

        _device.SelectionChanged += (_, _) => UpdateInfo();

        SetBody(BuildToolLayout(split, _status, ToolViewKit.ActionBar(_scan)));

        // 键盘：Enter = 开始扫描（焦点在下拉框 / 按钮里时不拦）；Esc 只把状态行收回初始提示，不停扫描、不清目录。
        ToolViewKit.AttachPageKeyboard(
            this,
            _scan,
            () => ToolViewKit.ShowStatus(
                _status,
                "点「刷新设备」检测扫描仪；在「扫描配置」里选好目录后点「开始扫描」，第一页会显示在右侧预览里。"));

        RefreshDevices();
    }

    private void PickDirectory()
    {
        ToolViewKit.PickFolder(_dialogs, _path);
        UpdateInfo();
    }

    /// <summary>「数据信息」分区：把当前设备、会话可用性、保存目录与本次扫描量摊开给用户看。</summary>
    private void UpdateInfo()
    {
        string? name = (_device.SelectedItem as ComboBoxItem)?.Content as string;
        bool ready = _twain.IsAvailable && (_device.SelectedItem as ComboBoxItem)?.Tag is string;

        _infoDevice.Text = string.IsNullOrWhiteSpace(name)
            ? "—"
            : name.Replace("（未检测到扫描仪）", "未检测到扫描仪");
        _infoSession.Text = ready ? "已就绪：可以开始扫描" : "不可用：详见「TWAIN 设置」中的技术详情";
        _infoSession.Foreground = ready ? ToolViewKit.SuccessTextBrush : ToolViewKit.ErrorTextBrush;

        string directory = ToolViewKit.Value(_path);
        _infoDirectory.Text = directory.Length == 0 ? "未选择" : directory;
    }

    private void RefreshDevices()
    {
        _device.Items.Clear();

        if (!_twain.IsAvailable)
        {
            SetScannerMissing(_twain.UnavailableReason);
            return;
        }

        var sources = _twain.ListSources();
        if (sources.Count == 0)
        {
            SetScannerMissing("TWAIN 已加载，但系统里没有可用的扫描仪数据源。");
            return;
        }

        foreach (var s in sources)
        {
            _device.Items.Add(new ComboBoxItem { Content = s.Name, Tag = s.Id });
        }

        _device.SelectedIndex = 0;
        _device.IsEnabled = true;
        _scan.IsEnabled = true;
        ToolViewKit.ShowStatus(_tech, string.Empty);
        ToolViewKit.ShowStatus(_status, $"已检测到 {sources.Count} 台扫描仪，选好目录后点「开始扫描」。", StatusTone.Success);
        UpdateInfo();
    }

    private void SetScannerMissing(string technicalReason)
    {
        _device.Items.Add(new ComboBoxItem { Content = "（未检测到扫描仪）", Tag = null });
        _device.SelectedIndex = 0;
        _device.IsEnabled = false;
        _scan.IsEnabled = false;
        // 服务在 Twain 可用时返回空串（不再谎报「未初始化」），这里要保住这行说明不变成空标签。
        ToolViewKit.ShowStatus(
            _tech,
            string.IsNullOrWhiteSpace(technicalReason)
                ? "技术详情：Twain 会话未就绪。"
                : "技术详情：" + technicalReason);
        ToolViewKit.ShowStatus(_status, NoScannerHint, StatusTone.Error);
        UpdateInfo();
    }

    private static TextBlock Info(string text) => new()
    {
        Text = text,
        FontSize = 13,
        TextWrapping = TextWrapping.Wrap,
        Foreground = UiTokens.MutedBrush,
        VerticalAlignment = VerticalAlignment.Center
    };

    private void StartScan()
    {
        if (!_twain.IsAvailable)
        {
            ToolViewKit.ShowStatus(_status, NoScannerHint, StatusTone.Error);
            return;
        }

        string? sourceId = (_device.SelectedItem as ComboBoxItem)?.Tag as string;
        if (string.IsNullOrWhiteSpace(sourceId))
        {
            ToolViewKit.ShowStatus(_status, "请先在「扫描仪」里选择一个设备，再点「开始扫描」。", StatusTone.Error);
            return;
        }

        string directory = ToolViewKit.Value(_path);
        if (directory.Length == 0 || !Directory.Exists(directory))
        {
            ToolViewKit.ShowStatus(_status, "请先点「选择…」指定一个存在的保存目录。", StatusTone.Error);
            return;
        }

        _scan.IsEnabled = false;
        _scanWatch.Restart();
        ToolViewKit.ShowStatus(_status, "正在扫描…（勾选了「显示扫描仪界面」时请在弹窗里完成设置）", StatusTone.Busy);

        try
        {
            var files = _twain.Scan(sourceId, new TwainScanOptions
            {
                OutputDirectory = directory,
                Dpi = int.TryParse(_dpi.SelectedItem?.ToString(), out int d) ? d : 200,
                Duplex = _duplex.IsChecked == true,
                ShowUi = _showUi.IsChecked == true,
                SkipBlankPages = _skipBlank.IsChecked == true
            });

            if (files.Count == 0)
            {
                ToolViewKit.ShowStatus(
                    _status,
                    "扫描结束但没有返回图像：请确认扫描仪就绪、文档已放入进纸器，然后重试。",
                    StatusTone.Error);
                return;
            }

            ShowPreview(files[0]);
            _infoCount.Text = $"{files.Count} 张";
            ToolViewKit.ShowStatus(_status, $"扫描完成：{files.Count} 个文件，已保存到 {directory}。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(
                _status,
                "扫描失败：" + ex.Message + "（请确认扫描仪已连接并在「传真和扫描」中可用）",
                StatusTone.Error);
        }
        finally
        {
            _scanWatch.Stop();
            _infoElapsed.Text = $"{_scanWatch.Elapsed.TotalSeconds:0.0} 秒";
            _scan.IsEnabled = _twain.IsAvailable;
            UpdateInfo();
        }
    }

    private void ShowPreview(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            ToolViewKit.ShowPlaceholder(_previewFrame, null, "扫描完成后在此显示第一页图像。");
            return;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path);
            image.EndInit();
            image.Freeze();
            _preview.Source = image;
            ToolViewKit.ShowPlaceholder(_previewFrame, _preview, string.Empty);
        }
        catch
        {
            // 预览失败不影响扫描结果落盘，状态行已给出文件位置。
            ToolViewKit.ShowPlaceholder(_previewFrame, null, "已在磁盘生成扫描件，但图像预览加载失败。");
        }
    }
}

internal sealed class OtherMiscModule : FeatureModuleBase
{
    public OtherMiscModule() : base(BuiltInFeatures.All.First(f => f.Id == "other.misc")) { }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new OtherMiscView(
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<IEmbeddedAssetService>());
}

/// <summary>释放内嵌工具。每条动作显示对应 embedded/ 资源是否就绪，缺文件时给放置位置。</summary>
public sealed class OtherMiscView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly IEmbeddedAssetService _assets;
    private readonly StackPanel _rows = new();
    private readonly List<(string Key, TextBlock State)> _states = new();
    private readonly TextBlock _status = ToolViewKit.Status(
        "选择一个工具释放到指定位置；缺资源时按下面的说明补齐后点「刷新状态」。");

    public OtherMiscView(IDialogService dialogs, IEmbeddedAssetService assets)
        : base("其他功能", "释放内嵌工具 / 下载运行库（对齐原 TabPage_13；文件来自 embedded/）")
    {
        _dialogs = dialogs;
        _assets = assets;

        AddActionRow("下载 .NET FX 3.5", "打开系统「启用或关闭 Windows 功能」，或跳转官方下载页。", "link.netfx35", OpenNetFx35);
        AddActionRow("注册表脚本", "释放 registry.reg 到指定位置，双击合并即可。", "registry",
            () => Release("registry", "注册表脚本|*.reg", "registry.reg"));
        AddActionRow("KMS 工具", "释放 kmsaio.cmd 批处理到指定位置。", "kmsaio",
            () => Release("kmsaio", "批处理|*.cmd", "kmsaio.cmd"));
        AddActionRow("ReNamer", "释放 ReNamerPro73.exe（附带 rules 时一并释放）。", "renamer", ReleaseRenamer);

        Border tools = ToolViewKit.Card("内嵌工具", _rows);

        Border notes = ToolViewKit.Card(
            "资源说明",
            ToolViewKit.Hint("本页为逐项资源释放，无统一主操作：每一项都只释放自己那个文件，按需点右侧「释放…」即可。"),
            ToolViewKit.Hint("所有文件来自程序目录 embedded/（构建时从 assets/embedded 复制）；缺文件时按上面的资源名补齐。"),
            ToolViewKit.Hint("「图片转 ICO」已拆成独立功能页，从左侧导航打开，不在此页释放。"));

        var actions = ToolViewKit.ActionBar(
            ToolViewKit.Secondary("打开资源目录", OpenAssetDirectory),
            ToolViewKit.Secondary("刷新状态", () =>
            {
                RefreshAssetStates();
                ToolViewKit.ShowStatus(_status, "已刷新资源状态。");
            }));

        SetBody(BuildToolLayout(ToolViewKit.FormArea(tools, notes), _status, actions));

        // 本页刻意没有主操作（每个资源各自释放），所以 Enter 不绑定任何动作；Esc 只收起状态行。
        ToolViewKit.AttachPageKeyboard(
            this,
            null,
            () => ToolViewKit.ShowStatus(_status, "选择一个工具释放到指定位置；缺资源时按下面的说明补齐后点「刷新状态」。"));

        RefreshAssetStates();
    }

    private void AddActionRow(string title, string description, string assetKey, Action click)
    {
        var state = new TextBlock
        {
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };
        _states.Add((assetKey, state));

        Button run = ToolViewKit.Secondary("释放…", click);
        run.MinWidth = 96;

        // 读屏软件：四行都是「释放…」，仅靠文字分不清释放哪个资源；状态文本是纯文本。
        AutomationProperties.SetName(run, $"释放 {title}");
        AutomationProperties.SetHelpText(run, description);
        AutomationProperties.SetName(state, $"{title} 资源状态");

        _rows.Children.Add(ToolViewKit.Field(
            title,
            new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = description, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 2) },
                    state
                }
            },
            run,
            labelWidth: 140));
    }

    private void RefreshAssetStates()
    {
        foreach (var (key, state) in _states)
        {
            bool exists = _assets.Exists(key);
            state.Text = exists
                ? $"embedded/{key} 已就绪"
                : $"未找到 embedded/{key}";
            state.Foreground = exists ? ToolViewKit.SuccessTextBrush : ToolViewKit.ErrorTextBrush;
        }
    }

    private void OpenAssetDirectory()
    {
        string directory = Path.Combine(AppContext.BaseDirectory, "embedded");
        if (!Directory.Exists(directory))
        {
            ToolViewKit.ShowStatus(
                _status,
                $"未找到资源目录：{directory}。请重新构建，或先把 assets/embedded 的内容复制到该目录。",
                StatusTone.Error);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
            ToolViewKit.ShowStatus(_status, "已打开资源目录：" + directory, StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "打开目录失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void OpenNetFx35()
    {
        string url = _assets.GetText("link.netfx35")?.Trim() ?? string.Empty;
        if (url.Length == 0)
        {
            url = "ms-settings:optionalfeatures";
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            ToolViewKit.ShowStatus(_status, "已打开 .NET Framework 3.5 的安装入口。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "打开失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void Release(string key, string filter, string defaultName)
    {
        if (!_assets.Exists(key))
        {
            ToolViewKit.ShowStatus(
                _status,
                $"未找到 embedded/{key}：请把文件放到程序目录 embedded/ 后点「刷新状态」。",
                StatusTone.Error);
            return;
        }

        string? target = _dialogs.SaveFile(filter, defaultName);
        if (string.IsNullOrWhiteSpace(target))
        {
            ToolViewKit.ShowStatus(_status, "已取消释放。");
            return;
        }

        try
        {
            _assets.ReleaseToFile(key, target);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "释放失败：" + ex.Message, StatusTone.Error);
            return;
        }

        ToolViewKit.ShowStatus(_status, "已释放：" + target, StatusTone.Success);
        if (_dialogs.Confirm("已释放。是否打开所在目录？"))
        {
            OpenFolder(Path.GetDirectoryName(target));
        }
    }

    private void ReleaseRenamer()
    {
        if (!_assets.Exists("renamer"))
        {
            ToolViewKit.ShowStatus(
                _status,
                "未找到 embedded/renamer：请把 ReNamerPro73.exe 放到程序目录 embedded/ 后点「刷新状态」。",
                StatusTone.Error);
            return;
        }

        string? target = _dialogs.SaveFile("可执行文件|*.exe", "ReNamerPro73.exe");
        if (string.IsNullOrWhiteSpace(target))
        {
            ToolViewKit.ShowStatus(_status, "已取消释放。");
            return;
        }

        try
        {
            _assets.ReleaseToFile("renamer", target);
            if (_assets.Exists("renamer.rules"))
            {
                string rulesPath = Path.Combine(Path.GetDirectoryName(target) ?? ".", "renamer-rules.rnp");
                _assets.ReleaseToFile("renamer.rules", rulesPath);
            }
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "释放失败：" + ex.Message, StatusTone.Error);
            return;
        }

        ToolViewKit.ShowStatus(_status, "已释放：" + target, StatusTone.Success);
        if (_dialogs.Confirm("已释放。是否立即运行？"))
        {
            RunFile(target);
        }
    }

    private void RunFile(string path)
    {
        try
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "运行失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void OpenFolder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "打开失败：" + ex.Message, StatusTone.Error);
        }
    }
}

internal sealed class ExtraToolsModule : FeatureModuleBase
{
    public ExtraToolsModule() : base(BuiltInFeatures.All.First(f => f.Id == "other.extra")) { }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new ExtraToolsView(
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<IMoneyChineseService>(),
            services.GetRequiredService<IFileCopyService>());
}

/// <summary>金额大写 + 目录监视（对齐 TabPage_36 的图片文件监控主路径）。</summary>
public sealed class ExtraToolsView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly IMoneyChineseService _money;
    // 数字框：窄、右对齐（宽度走 token 70–110），单位写在字段标签里。
    private readonly TextBox _moneyIn = new()
    {
        PlaceholderText = "如 1234.56",
        MinWidth = UiTokens.NumberBoxMinWidth,
        Width = UiTokens.NumberBoxMaxWidth,
        TextAlignment = TextAlignment.Right
    };
    private readonly TextBox _moneyOut = new() { IsReadOnly = true, PlaceholderText = "大写结果", MinWidth = 240 };
    private readonly TextBox _watchPath = ToolViewKit.PathBox("监视目录", "监视目录", folder: true);
    // legacy WatcherTimeDelayOut：等文件写完再列出来（默认 500ms，范围 100–553600）。
    private readonly TextBox _delay = new()
    {
        Text = NewFileWatchFilter.DefaultDelayMilliseconds.ToString(),
        Width = 90,
        TextAlignment = TextAlignment.Right
    };
    private readonly TextBlock _latest = new()
    {
        Text = "未启动监视。",
        Foreground = UiTokens.MutedBrush,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 8, 0, 0)
    };
    private readonly ListBox _events = new() { MinHeight = 140 };
    private readonly Button _startBtn;
    private readonly Button _stopBtn;
    private readonly TextBlock _status = ToolViewKit.Status(
        "输入金额后点「金额转大写」；或选择目录后点「开始监视」，新建 jpg/tif/bmp 时会逐条列出。");
    private FileSystemWatcher? _watcher;

    // 文件副本生成（对齐 legacy TabPage_36 Gen_Copied_*）：视图只收集值，命名规则在 Core FileCopyService。
    private readonly IFileCopyService _fileCopy;
    private readonly TextBox _copySource = ToolViewKit.PathBox("要复制的源文件", "要复制的源文件");
    private readonly TextBox _copyTarget = ToolViewKit.PathBox("副本输出目录", "副本输出目录", folder: true);
    private readonly TextBox _copyCount = ToolViewKit.NumberBox("生成数量", "如 10");
    private readonly TextBox _copySuffix = new() { Width = 120, Text = FileCopyService.DefaultSuffix };
    private readonly TextBox _copyDigits = ToolViewKit.NumberBox("序号位数", "如 1", 80);
    private readonly TextBlock _copyPreview = new()
    {
        Foreground = ToolViewKit.ErrorTextBrush,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 4, 0, 0)
    };

    public ExtraToolsView(IDialogService dialogs, IMoneyChineseService money, IFileCopyService fileCopy)
        : base("补充功能", "金额大写 / 路径监视 / 文件副本生成（jpg/tif/bmp 新建）")
    {
        _dialogs = dialogs;
        _money = money;
        _fileCopy = fileCopy;
        _copyCount.Text = "10";
        _copyDigits.Text = FileCopyService.DefaultPrefixDigits.ToString();

        // 读屏软件：金额、大写结果、监视目录都只有卡片上的可见标签；事件列表与状态行同理。
        AutomationProperties.SetName(_moneyIn, "金额（数字）");
        AutomationProperties.SetHelpText(_moneyIn, "输入金额数字后按回车或点「金额转大写」。");
        AutomationProperties.SetName(_moneyOut, "金额大写结果（只读）");
        AutomationProperties.SetName(_events, "监视事件列表");
        AutomationProperties.SetName(_latest, "监视状态");

        _moneyIn.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter)
            {
                ConvertMoney();
            }
        };

        Border moneyCard = ToolViewKit.Card(
            "金额转大写",
            ToolViewKit.Field("金额", _moneyIn),
            ToolViewKit.Field("大写", _moneyOut),
            ToolViewKit.Hint("支持两位小数；用于票据、合同金额栏。按回车等同点「金额转大写」。"));

        _startBtn = ToolViewKit.Secondary("开始监视", StartWatch);
        _stopBtn = ToolViewKit.Secondary("停止", () => StopWatch("已停止监视。"));
        _stopBtn.IsEnabled = false;

        Border watchCard = ToolViewKit.Card(
            "路径监视（新建 jpg/tif/bmp）",
            ToolViewKit.Field("目录", _watchPath, ToolViewKit.Secondary(
                "选择…",
                () =>
                {
                    string? dir = _dialogs.PickFolder();
                    if (!string.IsNullOrWhiteSpace(dir))
                    {
                        _watchPath.Text = dir;
                    }
                })),
            ToolViewKit.Field("延迟(ms)", _delay),
            ToolViewKit.ActionBar(_startBtn, _stopBtn),
            _latest,
            ToolViewKit.Frame(_events, 140),
            ToolViewKit.Hint("还没有监视事件：点「开始监视」后，新建的 jpg/tif/bmp 会出现在上面的列表里。"),
            ToolViewKit.Hint("只监视所选目录的第一层；忽略 .jpg.err / .jpg.han / .tif.err / .tif.han，最多保留 50 条。新建文件会等「延迟」毫秒再列出（默认 500），避免把还没写完的文件报出来。离开本页会自动停止监视。"));

        // 文件副本生成卡（对齐 legacy TabPage_36 Gen_Copied_*）。命名规则在 Core，预览名由服务计算。
        _copySource.TextChanged += (_, _) => UpdateCopyPreview();
        _copyTarget.TextChanged += (_, _) => UpdateCopyPreview();
        _copySuffix.TextChanged += (_, _) => UpdateCopyPreview();
        _copyDigits.TextChanged += (_, _) => UpdateCopyPreview();
        AutomationProperties.SetName(_copySource, "要复制的源文件");
        AutomationProperties.SetName(_copyTarget, "副本输出目录");
        AutomationProperties.SetName(_copyCount, "生成数量");
        AutomationProperties.SetName(_copySuffix, "副本名称后缀");
        AutomationProperties.SetName(_copyDigits, "副本序号位数");
        AutomationProperties.SetName(_copyPreview, "下一个副本名称预览");

        Border copyCard = ToolViewKit.Card(
            "文件副本生成",
            ToolViewKit.Field("源文件", _copySource,
                ToolViewKit.Secondary("选择…", () => ToolViewKit.PickFile(_dialogs, _copySource, "所有文件|*.*"))),
            ToolViewKit.Field("输出目录", _copyTarget,
                ToolViewKit.Secondary("选择…", () => ToolViewKit.PickFolder(_dialogs, _copyTarget))),
            ToolViewKit.Row(
                ToolViewKit.Field("数量", _copyCount, null, 44, stretchContent: false),
                ToolViewKit.Field("后缀名", _copySuffix, null, 56, stretchContent: false),
                ToolViewKit.Field("序号位数", _copyDigits, null, 72, stretchContent: false)),
            _copyPreview,
            ToolViewKit.Hint("命名：源文件名 - 后缀 (序号).扩展名；序号接着输出目录里已有副本往下排，不覆盖已有文件。"),
            ToolViewKit.ActionBar(ToolViewKit.Secondary("生成副本", GenerateCopies)));

        var moneyPrimary = ToolViewKit.Primary("金额转大写", ConvertMoney);
        var actions = ToolViewKit.ActionBar(
            moneyPrimary,
            ToolViewKit.Secondary("清空", () =>
            {
                _moneyIn.Text = string.Empty;
                _moneyOut.Text = string.Empty;
                ToolViewKit.ShowStatus(_status, "已清空金额输入。");
            }));

        SetBody(BuildToolLayout(ToolViewKit.FormArea(moneyCard, watchCard, copyCard), _status, actions));
        UpdateCopyPreview();

        // 本页主操作是「金额转大写」；监视的启停是卡内次级动作，不占 Enter。
        // Esc 只收回状态行：不停监视、不清金额（停止监视用卡内「停止」）。
        ToolViewKit.AttachPageKeyboard(
            this,
            moneyPrimary,
            () => ToolViewKit.ShowStatus(
                _status,
                "输入金额后点「金额转大写」；或选择目录后点「开始监视」，新建 jpg/tif/bmp 时会逐条列出。"));

        Unloaded += (_, _) => StopWatch(null);
    }

    private void ConvertMoney()
    {
        string result = _money.ToChineseUpper(_moneyIn.Text);
        if (string.IsNullOrEmpty(result))
        {
            _moneyOut.Text = string.Empty;
            ToolViewKit.ShowStatus(_status, "请输入大于 0 的金额数字（可带两位小数）。", StatusTone.Error);
            return;
        }

        _moneyOut.Text = result;
        ToolViewKit.ShowStatus(_status, "已转换：" + result, StatusTone.Success);
    }

    private void GenerateCopies()
    {
        string source = ToolViewKit.Value(_copySource);
        string target = ToolViewKit.Value(_copyTarget);
        if (source.Length == 0 || target.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "请先选择源文件与输出目录，再点「生成副本」。", StatusTone.Error);
            return;
        }

        if (!int.TryParse(ToolViewKit.Value(_copyCount), out int count) || count < 1)
        {
            ToolViewKit.ShowStatus(_status, "生成数量必须是大于 0 的整数。", StatusTone.Error);
            return;
        }

        try
        {
            FileCopyResult result = _fileCopy.Generate(source, target, count, ToolViewKit.Value(_copySuffix), ParseDigits());
            ToolViewKit.ShowStatus(_status, result.Summary, StatusTone.Success);
            UpdateCopyPreview();
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "生成副本失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void UpdateCopyPreview()
    {
        string source = ToolViewKit.Value(_copySource);
        if (source.Length == 0)
        {
            _copyPreview.Text = "选择源文件后，这里显示下一个将要生成的副本名称。";
            _copyPreview.Foreground = UiTokens.MutedBrush;
            return;
        }

        try
        {
            string name = _fileCopy.PreviewName(source, ToolViewKit.Value(_copyTarget), ToolViewKit.Value(_copySuffix), ParseDigits());
            _copyPreview.Text = "下一个副本名称：" + name;
            _copyPreview.Foreground = UiTokens.MutedBrush;
        }
        catch (Exception ex)
        {
            _copyPreview.Text = ex.Message;
            _copyPreview.Foreground = ToolViewKit.ErrorTextBrush;
        }
    }

    private int ParseDigits()
        => int.TryParse(ToolViewKit.Value(_copyDigits), out int digits) ? digits : FileCopyService.DefaultPrefixDigits;

    private void StartWatch()
    {
        string path = ToolViewKit.Value(_watchPath);
        if (path.Length == 0 || !Directory.Exists(path))
        {
            ToolViewKit.ShowStatus(_status, "监视目录无效：请点「选择…」指定一个存在的文件夹。", StatusTone.Error);
            return;
        }

        StopWatch(null);
        _watcher = new FileSystemWatcher(path)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
            IncludeSubdirectories = false,
            EnableRaisingEvents = true
        };
        _watcher.Created += OnCreated;
        _startBtn.IsEnabled = false;
        _stopBtn.IsEnabled = true;
        _latest.Text = "已启动监控，等待新建 jpg/tif/bmp。";
        _events.Items.Clear();

        // 延迟填了非数字时不静默纠正：告诉用户实际生效的是默认值。
        if (!NewFileWatchFilter.TryParseDelay(_delay.Text, out _))
        {
            _delay.Text = NewFileWatchFilter.DefaultDelayMilliseconds.ToString();
            ToolViewKit.ShowStatus(
                _status,
                $"延迟填了非数字，已按默认 {NewFileWatchFilter.DefaultDelayMilliseconds} ms 处理。正在监视：{path}",
                StatusTone.Error);
            return;
        }

        ToolViewKit.ShowStatus(_status, "正在监视：" + path, StatusTone.Success);
    }

    private void StopWatch(string? message)
    {
        if (_watcher is not null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= OnCreated;
            _watcher.Dispose();
            _watcher = null;
        }

        _startBtn.IsEnabled = true;
        _stopBtn.IsEnabled = false;
        if (message is not null)
        {
            _latest.Text = message;
            ToolViewKit.ShowStatus(_status, message);
        }
    }

    private async void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (!NewFileWatchFilter.IsTargetFile(e.Name))
        {
            return;
        }

        // legacy 同样先 Delay(WatcherTimeDelayOut.Value) 再 File.Exists：
        // 正在写入 / 写完即删的文件不该进列表。非法输入在 StartWatch 时已提示过，这里只取实际生效值。
        int delay = NewFileWatchFilter.ParseDelay(_delay.Text);
        await Task.Delay(delay).ConfigureAwait(true);
        if (!File.Exists(e.FullPath))
        {
            return;
        }

        _ = Dispatcher.InvokeAsync(() =>
        {
            string line = $"{DateTime.Now:HH:mm:ss}  {e.Name}";
            _latest.Text = "最新：" + e.Name;
            _events.Items.Insert(0, line);
            while (_events.Items.Count > 50)
            {
                _events.Items.RemoveAt(_events.Items.Count - 1);
            }
        });
    }
}
