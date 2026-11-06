using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Infrastructure.Plugins;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Code;
using DocumentTools.Core.Services.Office;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Tools;

public sealed class DataMatrixFeatureModule : FeatureModuleBase
{
    public DataMatrixFeatureModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "code.datamatrix"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new DataMatrixView(
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<IDataMatrixService>(),
            services.GetRequiredService<IDataMatrixImageClassifyService>(),
            services.GetRequiredService<OcrComponent>(),
            services.GetRequiredService<IDocumentInfoService>(),
            services.GetRequiredService<IAppSettings>());
}

/// <summary>Legacy「特殊二维码校验」: list + decode tree; Core <see cref="IDataMatrixService"/>.</summary>
public sealed class DataMatrixView : FeaturePageBase
{
    private const string EmptyHint = "还没有校验结果：选择 Word/PDF 后点「开始校验」，结果按「文件 → 页 → 分类」展开。";

    /// <summary>选择对话框：允许用户用「所有文件」显式兜底，和 legacy 过滤器一致。</summary>
    private const string PickerFilter = "Word/PDF|*.doc;*.docx;*.pdf|所有文件|*.*";

    /// <summary>拖放过滤：只认 Word/PDF，免得把无关文件一把拖进列表。</summary>
    private const string DropFilter = "Word/PDF|*.doc;*.docx;*.pdf";

    private readonly BorderedFileList _list = new();
    private readonly TreeView _tree = new();
    private readonly Border _treeFrame;
    private readonly TextBlock _status = ToolViewKit.Status(
        "选择 Word/PDF → 点「开始校验」→ 结果写入下方树。右键树可「展开本节点全部内容 / 折叠全部节点 / 打开文件 / 复制节点文本」。");
    private readonly IDataMatrixService _decoder;
    private readonly IDataMatrixImageClassifyService _classifier;
    private readonly OcrComponent _ocr;
    private readonly IAppSettings _settings;
    private readonly IDialogService _dialogs;
    private readonly Button _cancel;
    private CancellationTokenSource? _cts;

    private readonly TextBox _classifySource = ToolViewKit.PathBox(
        "图片源目录",
        "待分类图片所在目录",
        folder: true);

    private readonly TextBox _classifyTarget = ToolViewKit.PathBox(
        "图片分类目标目录",
        "分类结果保存目录",
        folder: true);

    private readonly CheckBox _classifyIncludeSubdirs = new()
    {
        Content = "包含子目录下文件",
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 16, 8)
    };

    private readonly CheckBox _classifyKeepSource = new()
    {
        Content = "保留源目录内文件",
        IsChecked = true,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 16, 8)
    };

    private readonly ComboBox _classifyThreads = new()
    {
        IsEditable = false,
        MinWidth = 90,
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 16, 8)
    };

    private readonly TextBlock _classifyStatus = ToolViewKit.Status(
        "选择源目录与目标目录 → 点「开始分类」：图片按 DataMatrix 页码码归入「页码-XXX」子目录，识别不到的归入「异常-无法识别的图片」。");

    /// <summary>勾选 OCR 的用法说明（组件可用时显示）。</summary>
    private const string OcrUsageHint =
        "OCR 按识别到的文字归档：匹配文字填正则（如 语文），命中图片进「分类目录名」（留空则用匹配文字）；勾「内容标注」会在命中文字上画红框再保存。";

    /// <summary>组件缺失时的弱提示：只用中性标签说明怎么装，不弹窗、不改变其它功能。</summary>
    private const string OcrMissingHint =
        "OCR 是可选功能，当前未安装：需要时把发布产物的 ocr 目录放到程序目录即可启用；不安装不影响下面的 DataMatrix 页码分类。";

    private readonly CheckBox _classifyUseOcr = new()
    {
        Content = "图像内容分类（OCR）",
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 16, 8)
    };

    private readonly TextBox _ocrPattern = new()
    {
        MinWidth = 160,
        Width = 180,
        PlaceholderText = "如 语文"
    };

    private readonly TextBox _ocrFolder = new()
    {
        MinWidth = 120,
        Width = 140,
        PlaceholderText = "默认同匹配文字"
    };

    private readonly CheckBox _ocrMark = new()
    {
        Content = "内容标注（画框）",
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, 16, 8)
    };

    private readonly TextBlock _ocrHint = ToolViewKit.Hint(OcrUsageHint);

    private readonly Button _classifyCancel;
    private CancellationTokenSource? _classifyCts;

    public DataMatrixView(
        IDialogService dialogs,
        IDataMatrixService decoder,
        IDataMatrixImageClassifyService classifier,
        OcrComponent ocr,
        IDocumentInfoService documentInfo,
        IAppSettings settings)
        : base("特殊二维码校验", "从 Word/PDF 逐页渲染并解码 DataMatrix（对齐 Util.ReadDocumentDataMatrix）")
    {
        _decoder = decoder;
        _classifier = classifier;
        _ocr = ocr;
        _settings = settings;
        _dialogs = dialogs;
        FileListContextWiring.Attach(_list, dialogs, documentInfo);
        _list.FileFilter = DropFilter;
        _list.FilesRejected = rejected => ToolViewKit.ShowStatus(
            _status,
            $"已忽略 {rejected} 个不支持的文件。",
            StatusTone.Error);
        _tree.ContextMenu = BuildTreeContextMenu();
        _tree.PreviewMouseRightButtonDown += OnTreePreviewRightButtonDown;

        // 读屏软件：结果树没有可见标题（标题在卡片上，卡片不是控件），必须有可访问名称。
        AutomationProperties.SetName(_tree, "解析结果树");
        AutomationProperties.SetHelpText(_tree, "层级：文件 → 页 → 识别分类；右键可展开、折叠或打开文件。");

        var add = ToolViewKit.Secondary("选择文件", () => _list.AddFiles(dialogs.OpenFiles(PickerFilter)));
        add.MinWidth = 120;

        Button run = ToolViewKit.Primary("开始校验", () => { });
        run.MinWidth = 120;
        run.Click += (_, _) => _ = RunDecodeAsync(run);

        // 解码是逐页渲染，大文件耗时以分钟计：必须能叫停，停下后保留已解析的部分。
        _cancel = ToolViewKit.Secondary("取消", () => _cts?.Cancel());
        _cancel.MinWidth = 96;
        _cancel.Visibility = Visibility.Collapsed;

        // 分类同样是长任务；与校验各自持有 CTS，互不干扰。
        _classifyCancel = ToolViewKit.Secondary("取消分类", () => _classifyCts?.Cancel());
        _classifyCancel.MinWidth = 96;
        _classifyCancel.Visibility = Visibility.Collapsed;

        _list.EmptyHint = "还没有文件：点「选择文件」，或把 Word / PDF 拖到这里。";
        _list.EmptyActionText = "选择文件";
        _list.EmptyAction = () => _list.AddFiles(dialogs.OpenFiles(PickerFilter));

        _treeFrame = ToolViewKit.Placeholder(EmptyHint, 180);
        ToolViewKit.ShowPlaceholder(_treeFrame, null, EmptyHint);

        // 原型 E 的「数据信息」式分区：结果树套在「解析信息」卡里，与列表区视觉分离。
        Border resultCard = ToolViewKit.Card("解析信息", _treeFrame);
        resultCard.Margin = new Thickness(0);

        var split = new Grid();
        split.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        split.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.2, GridUnitType.Star) });
        Grid.SetRow(_list, 0);
        Grid.SetRow(resultCard, 1);
        _list.Margin = new Thickness(0, 0, 0, 8);
        split.Children.Add(_list);
        split.Children.Add(resultCard);

        SetBody(BuildToolLayout(split, BuildClassifyCard(), _status, ToolViewKit.ActionBar(add, run, _cancel)));

        // 键盘：Enter（焦点不在列表/按钮里时）开始校验；Esc 收回状态行，不删列表也不动结果树。
        ToolViewKit.AttachPageKeyboard(
            this,
            run,
            () => ToolViewKit.ShowStatus(
                _status,
                "选择 Word/PDF → 点「开始校验」→ 结果写入下方树。右键树可「展开本节点全部内容 / 折叠全部节点 / 打开文件 / 复制节点文本」。"));
    }

    private ContextMenu BuildTreeContextMenu()
    {
        var menu = new ContextMenu();
        MenuItem expandNode = TreeItem("展开本节点全部内容", () => SetExpanded(GetSelectedItem(), expand: true, recursive: true));
        MenuItem collapseNode = TreeItem("折叠本节点全部内容", () => SetExpanded(GetSelectedItem(), expand: false, recursive: true));
        MenuItem expandAll = TreeItem("展开全部节点", () =>
        {
            foreach (TreeViewItem item in _tree.Items.OfType<TreeViewItem>())
            {
                SetExpanded(item, expand: true, recursive: true);
            }
        });
        MenuItem collapseAll = TreeItem("折叠全部节点", () =>
        {
            foreach (TreeViewItem item in _tree.Items.OfType<TreeViewItem>())
            {
                SetExpanded(item, expand: false, recursive: true);
            }
        });
        MenuItem openFile = TreeItem("打开文件", () =>
        {
            string? path = FindRootFilePath(GetSelectedItem());
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                ToolViewKit.ShowStatus(_status, "请先选中某个文件节点或其子节点，再点「打开文件」。", StatusTone.Error);
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
        });
        MenuItem copyText = TreeItem("复制节点文本", () =>
        {
            var item = GetSelectedItem();
            if (item?.Header is null)
            {
                ToolViewKit.ShowStatus(_status, "请先选中一个节点，再点「复制节点文本」。", StatusTone.Error);
                return;
            }

            Clipboard.SetText(item.Header.ToString() ?? string.Empty);
            ToolViewKit.ShowStatus(_status, "已复制到剪贴板。", StatusTone.Success);
        });

        menu.Items.Add(expandNode);
        menu.Items.Add(collapseNode);
        menu.Items.Add(expandAll);
        menu.Items.Add(collapseAll);
        menu.Items.Add(new Separator());
        menu.Items.Add(openFile);
        menu.Items.Add(copyText);

        // 与列表右菜单一致：打开时按当前选中状态刷新可用性，避免"点了没反应"。
        menu.Opened += (_, _) =>
        {
            TreeViewItem? selected = GetSelectedItem();
            bool hasNode = selected is not null;
            bool hasRoots = _tree.Items.Count > 0;
            string? rootPath = FindRootFilePath(selected);

            expandNode.IsEnabled = hasNode;
            collapseNode.IsEnabled = hasNode;
            expandAll.IsEnabled = hasRoots;
            collapseAll.IsEnabled = hasRoots;
            openFile.IsEnabled = !string.IsNullOrWhiteSpace(rootPath) && File.Exists(rootPath);
            copyText.IsEnabled = hasNode;
        };

        return menu;
    }

    private static System.Windows.Controls.MenuItem TreeItem(string header, Action action)
    {
        var item = new System.Windows.Controls.MenuItem { Header = header };
        item.Click += (_, _) => action();
        return item;
    }

    private TreeViewItem? GetSelectedItem() => _tree.SelectedItem as TreeViewItem;

    /// <summary>右键落在某个节点上时先选中它，菜单项才对应到用户真正点的位置。</summary>
    private void OnTreePreviewRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        for (DependencyObject? node = source; node is not null; node = VisualTreeHelper.GetParent(node))
        {
            if (node is TreeViewItem item)
            {
                item.IsSelected = true;
                item.Focus();
                break;
            }
        }
    }

    private static string? FindRootFilePath(TreeViewItem? item)
    {
        while (item is not null)
        {
            if (item.Parent is TreeView && item.Header is string path && File.Exists(path))
            {
                return path;
            }

            item = ItemsControl.ItemsControlFromItemContainer(item) as TreeViewItem;
        }

        return null;
    }

    private static void SetExpanded(TreeViewItem? item, bool expand, bool recursive)
    {
        if (item is null)
        {
            return;
        }

        item.IsExpanded = expand;
        if (!recursive)
        {
            return;
        }

        foreach (TreeViewItem child in item.Items.OfType<TreeViewItem>())
        {
            SetExpanded(child, expand, recursive: true);
        }
    }

    /// <summary>
    /// 底部「图片目录批量分类」分区：对齐 dev <c>groupBox67</c>（源/目标目录、包含子目录、保留源文件、线程数）。
    /// OCR 两项（图像内容分类 / 内容标注）不在本分区，见 <c>docs/dev-parity-tools.md</c> §2.1。
    /// </summary>
    private FrameworkElement BuildClassifyCard()
    {
        Button pickSource = ToolViewKit.Secondary(
            "选择目录…",
            () => ToolViewKit.PickFolder(_dialogs, _classifySource));
        pickSource.MinWidth = 104;

        Button pickTarget = ToolViewKit.Secondary(
            "选择目录…",
            () => ToolViewKit.PickFolder(_dialogs, _classifyTarget));
        pickTarget.MinWidth = 104;

        // 与 dev 同一口径：自动 + 1 + 8 的倍数（上限 ProcessorCount×2）。
        _classifyThreads.Items.Add("自动");
        foreach (int count in Enumerable.Range(1, Environment.ProcessorCount * 2).Where(x => x == 1 || x % 8 == 0))
        {
            _classifyThreads.Items.Add(count.ToString(CultureInfo.InvariantCulture));
        }

        _classifyThreads.SelectedIndex = 0;
        _classifyThreads.Margin = new Thickness(0);
        AutomationProperties.SetName(_classifyThreads, "分类处理线程数量");

        var threadGroup = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 8)
        };
        threadGroup.Children.Add(new TextBlock
        {
            Text = "线程数量",
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });
        threadGroup.Children.Add(_classifyThreads);

        Button start = ToolViewKit.Secondary("开始分类", () => { });
        start.MinWidth = 104;
        start.Click += (_, _) => _ = RunClassifyAsync(start);

        // OCR 是可选的独立组件：组件缺失时三个控件保持禁用，并把「怎么装」写在下面，而不是弹一次就没了。
        _classifyUseOcr.IsEnabled = _ocr.IsAvailable;
        _classifyUseOcr.ToolTip = _ocr.IsAvailable
            ? "按 OCR 识别到的文字归档；勾选后用「匹配文字」正则判定，命中图片进「分类目录名」目录。"
            : _ocr.UnavailableReason;
        _classifyUseOcr.Checked += (_, _) => SyncOcrInputs();
        _classifyUseOcr.Unchecked += (_, _) => SyncOcrInputs();

        _ocrPattern.Margin = new Thickness(0, 0, 16, 8);
        _ocrFolder.Margin = new Thickness(0, 0, 16, 8);
        _ocrMark.Margin = new Thickness(0, 0, 16, 8);
        _ocrMark.VerticalAlignment = VerticalAlignment.Center;
        AutomationProperties.SetName(_ocrPattern, "OCR 匹配文字");
        AutomationProperties.SetName(_ocrFolder, "OCR 分类目录名");
        SyncOcrInputs();

        var ocrRow = ToolViewKit.Row(
            _classifyUseOcr,
            Labeled("匹配文字（正则）", _ocrPattern),
            Labeled("分类目录名", _ocrFolder),
            _ocrMark);

        var ocrBlock = new StackPanel();
        ocrBlock.Children.Add(ocrRow);
        ocrBlock.Children.Add(_ocrHint);

        // 组件缺失是「可选功能未安装」，不是错误：用中性色标签说明即可，
        // 不弹窗、不阻断 DataMatrix 页码分类（dev 对这类可选能力也是弱提示）。
        _ocrHint.Text = _ocr.IsAvailable ? OcrUsageHint : OcrMissingHint;
        _ocrHint.Foreground = UiTokens.MutedBrush;

        return ToolViewKit.Card(
            "图片目录批量分类",
            ToolViewKit.Field("源目录", _classifySource, pickSource),
            ToolViewKit.Field("目标目录", _classifyTarget, pickTarget),
            ToolViewKit.Row(threadGroup, _classifyIncludeSubdirs, _classifyKeepSource, start, _classifyCancel),
            ocrBlock,
            _classifyStatus);
    }

    /// <summary>OCR 三项只在「勾选 OCR 且组件可用」时可编辑；否则一律禁用，避免半套可点的假参数。</summary>
    private void SyncOcrInputs()
    {
        bool enabled = _ocr.IsAvailable && _classifyUseOcr.IsChecked == true;
        _ocrPattern.IsEnabled = enabled;
        _ocrFolder.IsEnabled = enabled;
        _ocrMark.IsEnabled = enabled;
    }

    private static StackPanel Labeled(string caption, FrameworkElement control)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 8)
        };
        panel.Children.Add(new TextBlock
        {
            Text = caption,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });
        panel.Children.Add(control);
        return panel;
    }

    private async Task RunClassifyAsync(Button start)
    {
        string source = ToolViewKit.Value(_classifySource);
        string target = ToolViewKit.Value(_classifyTarget);

        if (source.Length == 0)
        {
            ToolViewKit.ShowStatus(_classifyStatus, "请先选择「源目录」（待分类图片所在目录）。", StatusTone.Error);
            return;
        }

        if (target.Length == 0)
        {
            ToolViewKit.ShowStatus(_classifyStatus, "请先选择「目标目录」（分类结果保存目录）。", StatusTone.Error);
            return;
        }

        if (!Directory.Exists(source))
        {
            ToolViewKit.ShowStatus(_classifyStatus, $"源目录不存在：{source}。请重新选择。", StatusTone.Error);
            return;
        }

        bool useOcr = _classifyUseOcr.IsChecked == true;
        if (useOcr && (!_ocr.IsAvailable || _ocr.Service is not { } ocrService))
        {
            ToolViewKit.ShowStatus(_classifyStatus, _ocr.UnavailableReason, StatusTone.Error);
            return;
        }

        string ocrPattern = ToolViewKit.Value(_ocrPattern);
        if (useOcr && ocrPattern.Length == 0)
        {
            ToolViewKit.ShowStatus(_classifyStatus, "请先填写 OCR「匹配文字」（正则，例如 语文）。", StatusTone.Error);
            return;
        }

        var request = new DataMatrixImageClassifyRequest
        {
            SourceDirectory = source,
            TargetDirectory = target,
            IncludeSubdirectories = _classifyIncludeSubdirs.IsChecked == true,
            KeepSourceFiles = _classifyKeepSource.IsChecked == true,
            MaxDegreeOfParallelism = SelectedThreadCount()
        };

        var ocrRequest = new OcrImageClassifyRequest
        {
            SourceDirectory = source,
            TargetDirectory = target,
            MatchPattern = ocrPattern,
            FolderName = ToolViewKit.Value(_ocrFolder),
            MarkContent = _ocrMark.IsChecked == true,
            IncludeSubdirectories = request.IncludeSubdirectories,
            KeepSourceFiles = request.KeepSourceFiles,
            MaxDegreeOfParallelism = request.MaxDegreeOfParallelism
        };

        _classifyCts = new CancellationTokenSource();
        CancellationToken token = _classifyCts.Token;
        start.IsEnabled = false;
        _classifyCancel.Visibility = Visibility.Visible;

        try
        {
            if (useOcr)
            {
                var ocrProgress = new Progress<OcrImageClassifyProgress>(item =>
                    ToolViewKit.ShowStatus(
                        _classifyStatus,
                        $"正在 OCR 分类：{item.Processed}/{item.Total}（{item.CurrentFile}）",
                        StatusTone.Busy));

                OcrImageClassifyOutcome ocrOutcome = await Task.Run(
                    () => _ocr.Service!.Classify(ocrRequest, ocrProgress, token),
                    token);
                ReportOcrOutcome(ocrOutcome, target);
            }
            else
            {
                var progress = new Progress<DataMatrixImageClassifyProgress>(item =>
                    ToolViewKit.ShowStatus(
                        _classifyStatus,
                        $"正在分类：{item.Processed}/{item.Total}（{item.CurrentFile}）",
                        StatusTone.Busy));

                DataMatrixImageClassifyOutcome outcome = await Task.Run(
                    () => _classifier.Classify(request, progress, token),
                    token);
                ReportClassifyOutcome(outcome, target);
            }
        }
        catch (OperationCanceledException)
        {
            ToolViewKit.ShowStatus(_classifyStatus, "已取消分类。已产出的文件保留在目标目录。", StatusTone.Neutral);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_classifyStatus, "分类失败：" + ex.Message, StatusTone.Error);
        }
        finally
        {
            start.IsEnabled = true;
            _classifyCancel.Visibility = Visibility.Collapsed;
            _classifyCts?.Dispose();
            _classifyCts = null;
        }
    }

    private void ReportOcrOutcome(OcrImageClassifyOutcome outcome, string target)
    {
        if (outcome.TotalImages == 0)
        {
            ToolViewKit.ShowStatus(
                _classifyStatus,
                "源目录里没有可分类的图片（支持 .jpg/.jpeg/.png/.bmp；勾「包含子目录下文件」可把子目录一并处理）。",
                StatusTone.Error);
            return;
        }

        if (outcome.Canceled)
        {
            ToolViewKit.ShowStatus(
                _classifyStatus,
                $"已取消：处理 {outcome.MatchedImages + outcome.UnmatchedImages + outcome.FailedImages}/{outcome.TotalImages} 张，已产出的文件保留在目标目录。",
                StatusTone.Neutral);
            return;
        }

        string detail = outcome.Folders.Count == 0
            ? string.Empty
            : "；" + string.Join(
                "、",
                outcome.Folders.Take(6).Select(folder => $"{folder.FolderName}({folder.ImageCount} 张)"));

        string summary =
            $"OCR 分类完成：共 {outcome.TotalImages} 张，命中归档 {outcome.MatchedImages} 张，未命中 {outcome.UnmatchedImages} 张";
        if (outcome.FailedImages > 0)
        {
            summary += $"，失败 {outcome.FailedImages} 张（首个：{outcome.Failures[0]}）";
        }

        StatusTone tone = outcome.FailedImages > 0 || outcome.MatchedImages == 0
            ? StatusTone.Error
            : StatusTone.Success;

        ToolViewKit.ShowStatus(_classifyStatus, summary + detail + "。", tone);

        if (outcome.FailedImages > 0 || outcome.MatchedImages == 0)
        {
            return;
        }

        OfferOpenTarget(target);
    }

    private void ReportClassifyOutcome(DataMatrixImageClassifyOutcome outcome, string target)
    {
        if (outcome.TotalImages == 0)
        {
            ToolViewKit.ShowStatus(
                _classifyStatus,
                "源目录里没有可分类的图片（支持 .jpg/.jpeg/.png/.bmp；勾「包含子目录下文件」可把子目录一并处理）。",
                StatusTone.Error);
            return;
        }

        string detail = string.Join(
            "、",
            outcome.Folders.Take(6).Select(folder => $"{folder.FolderName}({folder.ImageCount} 张)"));
        if (outcome.Folders.Count > 6)
        {
            detail += $" 等 {outcome.Folders.Count} 个目录";
        }

        if (outcome.Canceled)
        {
            int handled = outcome.ClassifiedImages + outcome.UnrecognizedImages + outcome.FailedImages;
            ToolViewKit.ShowStatus(
                _classifyStatus,
                $"已取消：处理 {handled}/{outcome.TotalImages} 张，已产出的文件保留在目标目录。{detail}",
                StatusTone.Neutral);
            return;
        }

        string summary = $"分类完成：共 {outcome.TotalImages} 张，按页码归档 {outcome.ClassifiedImages} 张，无法识别 {outcome.UnrecognizedImages} 张";
        if (outcome.FailedImages > 0)
        {
            summary += $"，失败 {outcome.FailedImages} 张（首个：{outcome.Failures[0]}）";
        }

        if (detail.Length > 0)
        {
            summary += "；" + detail;
        }

        // 一张都没归进页码目录 = 这批图没有可用的页码码，按错误语气提示，避免用户以为成功了。
        StatusTone tone = outcome.FailedImages > 0 || outcome.ClassifiedImages == 0
            ? StatusTone.Error
            : StatusTone.Success;

        ToolViewKit.ShowStatus(_classifyStatus, summary + "。", tone);

        if (outcome.FailedImages > 0)
        {
            return;
        }

        OfferOpenTarget(target);
    }

    /// <summary>处理完询问是否打开目标目录（受「系统设置 → 询问打开目录」控制，与转换页同口径）。</summary>
    private void OfferOpenTarget(string target)
    {
        if (!_settings.Current.ShowOpenTargetDialog
            || !_dialogs.Confirm("分类完成，是否需要打开目标目录？", "提示"))
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = target, UseShellExecute = true });
        }
        catch
        {
            // 打不开目录不影响分类结果，忽略。
        }
    }

    private int SelectedThreadCount()
        => _classifyThreads.SelectedIndex <= 0
            ? 0
            : int.TryParse(_classifyThreads.SelectedItem?.ToString(), out int count)
                ? count
                : 0;

    private async Task RunDecodeAsync(Button run)
    {
        _tree.Items.Clear();
        if (_list.Paths.Count == 0)
        {
            ToolViewKit.ShowStatus(_status, "请先点「选择文件」加入要校验的 Word/PDF。", StatusTone.Error);
            return;
        }

        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;
        run.IsEnabled = false;
        _cancel.Visibility = Visibility.Visible;

        int decoded = 0;
        int failed = 0;
        int found = 0;
        int emptyPages = 0;
        bool cancelled = false;
        int index = 0;
        int total = _list.Paths.Count;

        try
        {
            foreach (string path in _list.Paths)
            {
                index++;
                if (token.IsCancellationRequested)
                {
                    cancelled = true;
                    break;
                }

                ToolViewKit.ShowStatus(
                    _status,
                    $"正在解码第 {index} 个 / 共 {total} 个：{Path.GetFileName(path)}（逐页渲染，文件较大时较慢）",
                    StatusTone.Busy);

                var root = new TreeViewItem { Header = path, IsExpanded = true };
                try
                {
                    // 解码是纯 CPU 活：放到线程池，UI 才能重绘、取消按钮才点得动。
                    var result = await Task.Run(() => _decoder.ReadDocument(path), token);
                    foreach (var page in result.Pages)
                    {
                        var pageNode = new TreeViewItem
                        {
                            Header = $"第{page.PageNumber}页",
                            IsExpanded = true,
                            Foreground = Brushes.ForestGreen
                        };
                        if (page.Findings.Count == 0)
                        {
                            emptyPages++;
                            pageNode.Items.Add(new TreeViewItem { Header = "（未识别到 DataMatrix）" });
                        }
                        else
                        {
                            found += page.Findings.Count;
                            foreach (var group in page.Findings.GroupBy(f => f.Category))
                            {
                                var cat = new TreeViewItem { Header = group.Key, IsExpanded = true };
                                foreach (var finding in group)
                                {
                                    cat.Items.Add(new TreeViewItem
                                    {
                                        Header = finding.Value,
                                        Foreground = Brushes.Tomato
                                    });
                                }

                                pageNode.Items.Add(cat);
                            }
                        }

                        root.Items.Add(pageNode);
                    }

                    decoded++;
                    _list.MarkState(path, FileRowState.Success);
                }
                catch (OperationCanceledException)
                {
                    cancelled = true;
                    break;
                }
                catch (Exception ex)
                {
                    failed++;
                    _list.MarkState(path, FileRowState.Failed, ex.Message);
                    root.Items.Add(new TreeViewItem { Header = "错误: " + ex.Message, Foreground = Brushes.Tomato });
                }

                _tree.Items.Add(root);
            }
        }
        finally
        {
            run.IsEnabled = true;
            _cancel.Visibility = Visibility.Collapsed;
            _cts?.Dispose();
            _cts = null;
        }

        if (_tree.Items.Count > 0)
        {
            ToolViewKit.ShowPlaceholder(_treeFrame, _tree, EmptyHint);
        }

        string summary = $"校验完成：{decoded} 个文件识别到 {found} 处 DataMatrix";
        if (emptyPages > 0)
        {
            summary += $"，{emptyPages} 页未识别";
        }

        if (cancelled)
        {
            ToolViewKit.ShowStatus(
                _status,
                $"已取消：已解析 {decoded} 个文件（剩余 {total - decoded - failed} 个未处理），上方树保留已解析结果。",
                StatusTone.Neutral);
            return;
        }

        if (failed > 0)
        {
            ToolViewKit.ShowStatus(_status, summary + $"；{failed} 个文件失败（原因见结果树）。", StatusTone.Error);
            return;
        }

        ToolViewKit.ShowStatus(_status, summary + "。", found > 0 ? StatusTone.Success : StatusTone.Busy);
    }
}
