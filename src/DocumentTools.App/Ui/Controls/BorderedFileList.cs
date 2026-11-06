using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DocumentTools.App.Ui.Controls;

/// <summary>
/// WinForms ListView-like file list with legacy <c>ListViewRightMenu</c> parity.
/// </summary>
public sealed class BorderedFileList : Border
{
    private readonly System.Windows.Controls.ListView _listView = new();
    private readonly ObservableCollection<FileListItem> _items = new();
    private readonly Grid _host = new();
    private readonly Border _emptyOverlay = new();
    private readonly TextBlock _emptyHintBlock = new()
    {
        Foreground = UiTokens.MutedBrush,
        TextWrapping = TextWrapping.Wrap,
        TextAlignment = TextAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center
    };

    private string? _emptyHint;
    private string? _emptyActionText;
    private Action? _emptyAction;
    private bool _showGridLines = true;
    private int _rowHeight = 25;

    // 菜单项提成字段，OnOpened 时按当前状态刷新可用性（旧版 WinForms 是"点了没反应"，这里要能看出来）。
    private readonly MenuItem _listOpsMenu = new() { Header = "列表操作" };
    private readonly MenuItem _fileOpsMenu = new() { Header = "文件操作" };
    private readonly MenuItem _deleteItem = new() { Header = "删除本项" };
    private readonly MenuItem _clearItem = new() { Header = "清空列表" };
    private readonly MenuItem _openSourceDirItem = new() { Header = "打开源文件目录" };
    private readonly MenuItem _openSourceItem = new() { Header = "打开源文件" };
    private readonly MenuItem _openOutputDirItem = new() { Header = "打开转换后目录" };
    private readonly MenuItem _openOutputItem = new() { Header = "打开转换后文件" };
    private readonly MenuItem _docInfoItem = new() { Header = "获取文件信息" };

    public BorderedFileList()
    {
        BorderBrush = UiTokens.BorderBrush;
        BorderThickness = new Thickness(1);
        Background = Brushes.White;
        SnapsToDevicePixels = true;
        UseLayoutRounding = true;

        _listView.ItemsSource = _items;
        _listView.BorderThickness = new Thickness(0);
        _listView.SelectionMode = SelectionMode.Extended;
        _listView.View = new System.Windows.Controls.GridView
        {
            AllowsColumnReorder = false,
            Columns =
            {
                new System.Windows.Controls.GridViewColumn { Header = "序号", Width = 56, DisplayMemberBinding = new System.Windows.Data.Binding(nameof(FileListItem.Index)) },
                new System.Windows.Controls.GridViewColumn { Header = "文件名", Width = 280, DisplayMemberBinding = new System.Windows.Data.Binding(nameof(FileListItem.Name)) },
                new System.Windows.Controls.GridViewColumn { Header = "完整路径", Width = 520, DisplayMemberBinding = new System.Windows.Data.Binding(nameof(FileListItem.FullPath)) }
            }
        };

        _listView.ContextMenu = BuildContextMenu();
        _listView.PreviewMouseRightButtonDown += OnPreviewRightButtonDown;
        _listView.ToolTip = "右键：删除本项、清空列表、打开源文件/目录、打开转换后文件/目录、获取文件信息；也可以把文件拖进来。";

        // 读屏软件：列表本身没有可见标题，必须有可访问名称，否则只会念"列表 0 项"。
        AutomationProperties.SetName(_listView, "文件列表");
        AutomationProperties.SetHelpText(_listView, "可拖入文件；右键打开列表操作与文件操作菜单。");
        AllowDrop = true;
        _listView.AllowDrop = true;
        _listView.Drop += OnDrop;
        _listView.DragOver += (_, e) =>
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        };

        Child = _host;

        // 空态放在列表之前：它视觉上盖在列表上（靠 ZIndex），但视觉树里排在列表前面，
        // 这样 Tab 从页面顶部往下走时不会「先跳到底部按钮、再跳回顶部的空态按钮」。
        _host.Children.Add(_emptyOverlay);
        _host.Children.Add(_listView);
        Panel.SetZIndex(_emptyOverlay, 1);
        BuildEmptyOverlay();
        _items.CollectionChanged += (_, _) => UpdateEmptyOverlay();

        _listView.MouseDoubleClick += OnListMouseDoubleClick;
        ApplyListChrome();
    }

    /// <summary>
    /// 双击一行时的动作（默认：打开源文件；文件不在了就打开它所在的目录）。
    /// 右键菜单里有同等入口（「打开源文件」/「打开源文件目录」），两条路都可发现。
    /// </summary>
    public Action<string>? ItemActivated { get; set; }

    /// <summary>列表为空时盖在列表上的提示；null 表示不显示空态（老行为）。</summary>
    public string? EmptyHint
    {
        get => _emptyHint;
        set
        {
            _emptyHint = value;
            BuildEmptyOverlay();
        }
    }

    /// <summary>空态里那个可直接点的按钮文字（如「选择文件」）。</summary>
    public string? EmptyActionText
    {
        get => _emptyActionText;
        set
        {
            _emptyActionText = value;
            BuildEmptyOverlay();
        }
    }

    /// <summary>空态按钮的动作。</summary>
    public Action? EmptyAction
    {
        get => _emptyAction;
        set
        {
            _emptyAction = value;
            BuildEmptyOverlay();
        }
    }

    /// <summary>Last conversion output directory (for “打开转换后目录”).</summary>
    public string? LastOutputDirectory { get; set; }

    /// <summary>Optional: map a source file to its converted output path.</summary>
    public Func<string, string?>? ResolveConvertedPath { get; set; }

    /// <summary>Optional: show document metadata (PDF/Word/…). Injected by host page.</summary>
    public Func<string, string>? GetDocumentInfo { get; set; }

    /// <summary>Optional dialog for info messages when no IDialogService is available.</summary>
    public Action<string>? ShowInfo { get; set; }

    /// <summary>
    /// 「选择文件」用的同一份过滤串（如 <c>Word 文档|*.doc;*.docx|所有文件|*.*</c>）。
    /// 设置后拖放也按它过滤，拖不进列表接受不了的文件；
    /// <c>null</c> 或空白串表示该功能不限后缀 —— <b>一律放行</b>，绝不整批拒收（旧行为不变）。
    /// </summary>
    public string? FileFilter { get; set; }

    /// <summary>拖放被过滤掉的文件数量；宿主页据此在状态行提示（不弹窗）。</summary>
    public Action<int>? FilesRejected { get; set; }

    public bool ShowGridLines
    {
        get => _showGridLines;
        set
        {
            _showGridLines = value;
            ApplyListChrome();
        }
    }

    public int RowHeight
    {
        get => _rowHeight;
        set
        {
            _rowHeight = Math.Clamp(value, 18, 64);
            ApplyListChrome();
        }
    }

    public IReadOnlyList<string> Paths => _items.Select(i => i.FullPath).ToList();

    /// <summary>处理失败的条目（含原因），供「复制失败清单」使用。</summary>
    public IReadOnlyList<FileListItem> FailedItems =>
        _items.Where(i => i.State == FileRowState.Failed).ToList();

    /// <summary>
    /// 给某一行打处理状态：成功绿、失败红并把原因挂到行 <c>ToolTip</c>、处理中蓝。
    /// 行颜色与原因都落在 <see cref="FileListItem"/> 上，由 <see cref="ApplyListChrome"/> 的 DataTrigger 渲染。
    /// </summary>
    public void MarkState(string path, FileRowState state, string? reason = null)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (string.Equals(_items[i].FullPath, path, StringComparison.OrdinalIgnoreCase))
            {
                _items[i] = _items[i] with { State = state, Reason = reason };
                return;
            }
        }
    }

    /// <summary>批量重置行状态（每次开始处理前调用，避免上一轮的颜色残留）。</summary>
    public void ResetStates()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].State != FileRowState.None || _items[i].Reason is not null)
            {
                _items[i] = _items[i] with { State = FileRowState.None, Reason = null };
            }
        }
    }

    public FileListItem? SelectedItem => _listView.SelectedItem as FileListItem;

    public void SetFiles(IEnumerable<string> files)
    {
        _items.Clear();
        int i = 1;
        foreach (string path in files)
        {
            _items.Add(new FileListItem(i++, System.IO.Path.GetFileName(path), path));
        }
    }

    public void ClearFiles() => _items.Clear();

    public void AddFiles(IEnumerable<string> files)
    {
        var existing = new HashSet<string>(_items.Select(i => i.FullPath), StringComparer.OrdinalIgnoreCase);
        foreach (string path in files)
        {
            if (existing.Add(path))
            {
                _items.Add(new FileListItem(_items.Count + 1, System.IO.Path.GetFileName(path), path));
            }
        }

        Renumber();
    }

    public void RemoveSelected()
    {
        var selected = _listView.SelectedItems.Cast<FileListItem>().ToList();
        foreach (var item in selected)
        {
            _items.Remove(item);
        }

        Renumber();
    }

    public void MoveSelected(int delta)
    {
        if (_listView.SelectedItem is not FileListItem item)
        {
            return;
        }

        int index = _items.IndexOf(item);
        int target = index + delta;
        if (index < 0 || target < 0 || target >= _items.Count)
        {
            return;
        }

        _items.Move(index, target);
        Renumber();
        _listView.SelectedItem = _items[target];
    }

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();

        _listOpsMenu.Items.Clear();
        _listOpsMenu.Items.Add(_deleteItem);
        _listOpsMenu.Items.Add(_clearItem);
        menu.Items.Add(_listOpsMenu);

        _fileOpsMenu.Items.Clear();
        _fileOpsMenu.Items.Add(_openSourceDirItem);
        _fileOpsMenu.Items.Add(_openSourceItem);
        _fileOpsMenu.Items.Add(_openOutputDirItem);
        _fileOpsMenu.Items.Add(_openOutputItem);
        menu.Items.Add(_fileOpsMenu);

        menu.Items.Add(_docInfoItem);

        _deleteItem.Click += (_, _) => RemoveSelected();
        _clearItem.Click += (_, _) => ClearFiles();
        _openSourceDirItem.Click += (_, _) => OpenPath(GetSelectedSourceDir(), isDir: true);
        _openSourceItem.Click += (_, _) => OpenPath(SelectedItem?.FullPath, isDir: false);
        _openOutputDirItem.Click += (_, _) => OpenPath(LastOutputDirectory, isDir: true);
        _openOutputItem.Click += (_, _) => OpenPath(ResolveOutputPath(), isDir: false);
        _docInfoItem.Click += (_, _) => ShowDocumentInfo();

        menu.Opened += (_, _) => SyncMenuState();
        return menu;
    }

    /// <summary>当前选中项对应的转换后文件（未转换时为 null）。</summary>
    private string? ResolveOutputPath()
    {
        string? source = SelectedItem?.FullPath;
        return source is null ? null : ResolveConvertedPath?.Invoke(source);
    }

    private void ShowDocumentInfo()
    {
        string? path = SelectedItem?.FullPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            ShowInfo?.Invoke("请先选中一个存在的文件。");
            return;
        }

        if (GetDocumentInfo is null)
        {
            ShowInfo?.Invoke("当前页未接入文件信息查询。");
            return;
        }

        try
        {
            ShowInfo?.Invoke(GetDocumentInfo(path));
        }
        catch (Exception ex)
        {
            ShowInfo?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// 菜单每次打开都按当前状态刷新可用性：列表为空 / 没选中 / 还没转换过 时置灰，
    /// 而不是让人点一下毫无反应。分组项在子项全灰时一并置灰。
    /// </summary>
    private void SyncMenuState()
    {
        FileListItem? selected = SelectedItem;
        string? sourcePath = selected?.FullPath;
        string? sourceDir = sourcePath is null ? null : System.IO.Path.GetDirectoryName(sourcePath);
        string? outputPath = ResolveOutputPath();

        _deleteItem.IsEnabled = _listView.SelectedItems.Count > 0;
        _clearItem.IsEnabled = _items.Count > 0;
        _openSourceDirItem.IsEnabled = !string.IsNullOrWhiteSpace(sourceDir) && Directory.Exists(sourceDir);
        _openSourceItem.IsEnabled = !string.IsNullOrWhiteSpace(sourcePath) && File.Exists(sourcePath);
        _openOutputDirItem.IsEnabled = !string.IsNullOrWhiteSpace(LastOutputDirectory) && Directory.Exists(LastOutputDirectory);
        _openOutputItem.IsEnabled = !string.IsNullOrWhiteSpace(outputPath) && File.Exists(outputPath);
        _docInfoItem.IsEnabled = !string.IsNullOrWhiteSpace(sourcePath) && File.Exists(sourcePath);

        _listOpsMenu.IsEnabled = _deleteItem.IsEnabled || _clearItem.IsEnabled;
        _fileOpsMenu.IsEnabled = _openSourceDirItem.IsEnabled
                                 || _openSourceItem.IsEnabled
                                 || _openOutputDirItem.IsEnabled
                                 || _openOutputItem.IsEnabled;
    }

    /// <summary>右键落在某一行时先选中该行，与 WinForms 的 FocusedItem 语义一致。</summary>
    private void OnPreviewRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        for (DependencyObject? node = source; node is not null; node = VisualTreeHelper.GetParent(node))
        {
            if (node is ListViewItem row)
            {
                if (!row.IsSelected)
                {
                    _listView.SelectedItem = row;
                }

                break;
            }
        }
    }

    private string? GetSelectedSourceDir()
    {
        string? path = SelectedItem?.FullPath;
        return string.IsNullOrWhiteSpace(path) ? null : System.IO.Path.GetDirectoryName(path);
    }

    private void OpenPath(string? path, bool isDir)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            ShowInfo?.Invoke(isDir ? "没有可打开的目录。" : "没有可打开的文件。");
            return;
        }

        if (isDir)
        {
            if (!Directory.Exists(path))
            {
                ShowInfo?.Invoke("目录不存在：" + path);
                return;
            }
        }
        else if (!File.Exists(path))
        {
            ShowInfo?.Invoke("文件不存在：" + path);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            ShowInfo?.Invoke(ex.Message);
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
        {
            var accepted = new List<string>();
            int rejected = 0;
            foreach (string path in ExpandDropped(files))
            {
                if (MatchesFileFilter(path, FileFilter))
                {
                    accepted.Add(path);
                }
                else
                {
                    rejected++;
                }
            }

            AddFiles(accepted);

            if (rejected > 0)
            {
                FilesRejected?.Invoke(rejected);
            }
        }
    }

    /// <summary>
    /// 按 WinForms 风格过滤串（<c>描述|*.doc;*.docx|描述|*.*</c>）判断单个文件是否放行。
    /// 过滤串为空时全部放行，保证未接过滤的页行为不变。
    /// </summary>
    public static bool MatchesFileFilter(string path, string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return true;
        }

        string extension = System.IO.Path.GetExtension(path);
        string[] parts = filter.Split('|');

        // 描述在偶数位，模式在奇数位。
        for (int i = 1; i < parts.Length; i += 2)
        {
            foreach (string pattern in parts[i].Split(
                         ';',
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (pattern is "*.*" or "*")
                {
                    return true;
                }

                if (pattern.StartsWith("*.", StringComparison.Ordinal) &&
                    string.Equals(pattern[1..], extension, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static IEnumerable<string> ExpandDropped(IEnumerable<string> paths)
    {
        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                yield return path;
            }
            else if (Directory.Exists(path))
            {
                foreach (string file in Directory.EnumerateFiles(path))
                {
                    yield return file;
                }
            }
        }
    }

    /// <summary>
    /// 空态：一句「下一步做什么」+ 一个可直接点的按钮。列表为空时盖在列表上（不占额外高度，
    /// 列表本身仍可接收拖放），有文件后自动隐藏。
    /// </summary>
    private void BuildEmptyOverlay()
    {
        var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

        // 空态会被反复重建（同一实例重复设置 EmptyHint，或进程内第二个页面构造时复用同一控件状态）。
        // 复用字段前先断开逻辑父级，否则再次 Add 会抛「指定的元素已经是另一个元素的逻辑子元素」。
        if (LogicalTreeHelper.GetParent(_emptyHintBlock) is Panel previousParent)
        {
            previousParent.Children.Remove(_emptyHintBlock);
        }

        _emptyHintBlock.Text = _emptyHint ?? string.Empty;
        stack.Children.Add(_emptyHintBlock);

        if (!string.IsNullOrWhiteSpace(_emptyActionText) && _emptyAction is not null)
        {
            Button action = ToolViewKit.Secondary(_emptyActionText, _emptyAction);
            action.HorizontalAlignment = HorizontalAlignment.Center;
            action.Margin = new Thickness(0, 12, 0, 0);

            // 这个按钮是底部主操作的同名快捷方式，故意不进 Tab 序列：否则键盘用户会先到底部、
            // 又被拉回列表上方的副本，Tab 顺序出现"倒退"。读屏仍会念空态提示文字。
            action.IsTabStop = false;
            AutomationProperties.SetHelpText(action, "与页面底部的同名按钮相同；键盘请用底部的那个。");
            stack.Children.Add(action);
        }

        _emptyOverlay.Background = Brushes.White;
        _emptyOverlay.Child = stack;
        AutomationProperties.SetName(_emptyOverlay, _emptyHint is null ? string.Empty : "空： " + _emptyHint);

        UpdateEmptyOverlay();
    }

    private void UpdateEmptyOverlay()
    {
        bool show = _items.Count == 0 && !string.IsNullOrWhiteSpace(_emptyHint);
        _emptyOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

        // 空态盖住列表时把可访问名称让给空态提示，避免读屏念「列表 0 项」却听不到该做什么。
        AutomationProperties.SetName(_listView, show ? string.Empty : "文件列表");
    }

    private void OnListMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is not DependencyObject source)
        {
            return;
        }

        for (DependencyObject? node = source; node is not null; node = VisualTreeHelper.GetParent(node))
        {
            if (node is ListViewItem row && row.DataContext is FileListItem item)
            {
                e.Handled = true;
                if (ItemActivated is not null)
                {
                    ItemActivated(item.FullPath);
                }
                else if (File.Exists(item.FullPath))
                {
                    OpenPath(item.FullPath, isDir: false);
                }
                else
                {
                    OpenPath(System.IO.Path.GetDirectoryName(item.FullPath), isDir: true);
                }

                return;
            }
        }
    }

    private void ApplyListChrome()
    {
        var style = new Style(typeof(ListViewItem));
        style.Setters.Add(new Setter(Control.HeightProperty, (double)_rowHeight + 8));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(4, 2, 4, 2)));
        if (_showGridLines)
        {
            style.Setters.Add(new Setter(Control.BorderBrushProperty, UiTokens.BorderBrush));
            style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0, 0, 0, 1)));
        }

        // 处理结果直接落在行上：成功绿、失败红、处理中蓝。失败原因进行 ToolTip，鼠标悬停即可看。
        style.Triggers.Add(RowTrigger(FileRowState.Failed, Brushes.Tomato));
        style.Triggers.Add(RowTrigger(FileRowState.Success, Brushes.ForestGreen));
        style.Triggers.Add(RowTrigger(FileRowState.Running, Brushes.SteelBlue));
        style.Triggers.Add(RowTrigger(FileRowState.Pending, Brushes.SteelBlue));

        _listView.ItemContainerStyle = style;
    }

    private static DataTrigger RowTrigger(FileRowState state, Brush brush)
    {
        var trigger = new DataTrigger
        {
            Binding = new System.Windows.Data.Binding(nameof(FileListItem.State)),
            Value = state
        };

        trigger.Setters.Add(new Setter(Control.ForegroundProperty, brush));
        trigger.Setters.Add(new Setter(
            Control.ToolTipProperty,
            new System.Windows.Data.Binding(nameof(FileListItem.Reason))));
        return trigger;
    }

    private void Renumber()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            _items[i] = _items[i] with { Index = i + 1 };
        }
    }
}

/// <summary>行级处理状态：列表据此着色，失败原因进行的 ToolTip。</summary>
public enum FileRowState
{
    None,
    Pending,
    Running,
    Success,
    Failed
}

public sealed record FileListItem(int Index, string Name, string FullPath)
{
    /// <summary>本轮处理的可见状态（默认无）。</summary>
    public FileRowState State { get; init; } = FileRowState.None;

    /// <summary>失败原因（成功或未处理时为 null）。</summary>
    public string? Reason { get; init; }
}
