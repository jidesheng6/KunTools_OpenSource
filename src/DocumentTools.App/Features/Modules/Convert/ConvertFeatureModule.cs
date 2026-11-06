using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.App.Ui.Options;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Office;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Features.Modules.Convert;

/// <summary>Feature Module: one instance per convert FeatureId (Factory registration).</summary>
public sealed class ConvertFeatureModule : FeatureModuleBase
{
    public ConvertFeatureModule(FeatureDescriptor descriptor)
        : base(descriptor)
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new GenericConvertView(
            Descriptor,
            services.GetRequiredService<IEnumerable<IDocumentOperation>>(),
            services.GetRequiredService<IDocumentOperationRunner>(),
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<IAppSettings>(),
            services.GetRequiredService<IDocumentInfoService>());

    public static IEnumerable<ConvertFeatureModule> CreateAll()
        => BuiltInFeatures.ConvertFeatures.Select(d => new ConvertFeatureModule(d));
}

/// <summary>
/// Presentation only: list + schema-driven options + ConvertStart flow.
/// Business option keys come from <see cref="FeatureOptionSchemas"/> (Core);
/// execution goes through <see cref="IDocumentOperation"/> (Strategy).
/// </summary>
public sealed class GenericConvertView : FeaturePageBase
{
    private readonly FeatureDescriptor _feature;
    private readonly IDocumentOperation? _operation;
    private readonly IDocumentOperationRunner _operationRunner;
    private readonly IDialogService _dialogs;
    private readonly IAppSettings _settings;
    private readonly BorderedFileList _fileList;
    private readonly Dictionary<string, string> _convertedBySource = new(StringComparer.OrdinalIgnoreCase);
    private readonly StackPanel _optionsHost = new();
    private readonly OptionPanelBuilder _options;
    private readonly TextBlock _status = new()
    {
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(12, 0, 12, 0),
        TextWrapping = TextWrapping.Wrap
    };
    private Button? _runButton;
    private Button? _cancelButton;
    private Button? _copyFailuresButton;
    private CancellationTokenSource? _cts;
    private bool _running;

    public GenericConvertView(
        FeatureDescriptor feature,
        IEnumerable<IDocumentOperation> operations,
        IDocumentOperationRunner operationRunner,
        IDialogService dialogs,
        IAppSettings settings,
        IDocumentInfoService documentInfo)
        : base(feature.Title, feature.Description)
    {
        _feature = feature;
        _operationRunner = operationRunner;
        _dialogs = dialogs;
        _settings = settings;
        _options = new OptionPanelBuilder(dialogs);
        _operation = operations.FirstOrDefault(o =>
            string.Equals(o.FeatureId, feature.Id, StringComparison.OrdinalIgnoreCase));

        _fileList = new BorderedFileList
        {
            ShowGridLines = settings.Current.ListViewShowGridLines,
            RowHeight = settings.Current.ListViewLineHeight,
            MinHeight = 180
        };
        FileListContextWiring.Attach(_fileList, dialogs, documentInfo, _convertedBySource);

        // 拖放与「选择文件」共用同一份过滤串；空串表示该功能不限后缀（放行）。
        _fileList.FileFilter = FeatureOptionSchemas.GetFileFilter(_feature.Id);
        _fileList.FilesRejected = rejected => ToolViewKit.ShowStatus(
            _status,
            $"已忽略 {rejected} 个不支持的文件。",
            StatusTone.Error);

        BuildBody();
        _status.Text = "先添加文件并设置参数，再点「开始处理」。也可把文件拖进列表。";

        // 空态给「下一步做什么」+ 一个可点按钮；按钮只是把「选择文件」搬到手边，仍是次级样式（主操作只有一个）。
        _fileList.EmptyHint = "还没有文件：点下面的「选择文件」，或把文件 / 文件夹拖到这里。";
        _fileList.EmptyActionText = "选择文件";
        _fileList.EmptyAction = ChooseFiles;

        // 键盘语义：Ctrl+O 选择文件、Ctrl+Enter 开始处理、Delete 移除选中行（仅当焦点在列表里）。
        PreviewKeyDown += OnPagePreviewKeyDown;
        ToolViewKit.AttachPageKeyboard(this, _runButton);
    }

    private void ChooseFiles()
        => _fileList.AddFiles(_dialogs.OpenFiles(FeatureOptionSchemas.GetFileFilter(_feature.Id)));

    private void OnPagePreviewKeyDown(object sender, KeyEventArgs e)
    {
        // 壳层已占用 Ctrl+K（搜索）与 Esc（收提示）。这里只用 Ctrl+O / Ctrl+Enter / Delete，
        // 并且焦点落在编辑类控件里时一律让位——不然用户在输入框里按 Ctrl+O 会莫名弹选择对话框。
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
        {
            if (!IsEditingFocus())
            {
                ChooseFiles();
                e.Handled = true;
            }

            return;
        }

        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.Enter)
        {
            if (!_running && !IsEditingFocus())
            {
                _ = RunAsync();
                e.Handled = true;
            }

            return;
        }

        // Delete 只在文件列表有焦点时删行：否则在输入框里按 Delete 会被误当成「移除文件」。
        if (e.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None &&
            _fileList.IsKeyboardFocusWithin && !IsEditingFocus())
        {
            _fileList.RemoveSelected();
            e.Handled = true;
        }
    }

    /// <summary>焦点是否在会吃掉按键的编辑类控件里（输入框 / 可编辑下拉 / 密码框）。</summary>
    private static bool IsEditingFocus()
        => Keyboard.FocusedElement is System.Windows.Controls.Primitives.TextBoxBase
            or System.Windows.Controls.PasswordBox
            or ComboBox;

    private void BuildBody()
    {
        var side = new StackPanel
        {
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        side.Children.Add(SideBtn(SymbolRegular.ArrowUp24, "上移", () => _fileList.MoveSelected(-1)));
        side.Children.Add(SideBtn(SymbolRegular.ArrowDown24, "下移", () => _fileList.MoveSelected(1)));
        side.Children.Add(SideBtn(SymbolRegular.Subtract24, "移除选中", () => _fileList.RemoveSelected()));
        side.Children.Add(SideBtn(SymbolRegular.Dismiss24, "清空列表", () => _fileList.ClearFiles()));

        // 列表与卡片必须同一条右边缘：侧栏（上移/下移/移除/清空）是列表外的固定列，
        // 如果卡片铺满整行，右边缘会比列表多出侧栏宽度，看起来「卡片戳出列表」。
        // 用 Grid 把卡片放进列表所在列（第 0 列），视觉对齐由结构保证，不靠拍像素。
        var listRow = new Grid();
        listRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        listRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        listRow.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        listRow.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(_fileList, 0);
        Grid.SetColumn(_fileList, 0);
        Grid.SetRow(side, 0);
        Grid.SetColumn(side, 1);
        listRow.Children.Add(_fileList);
        listRow.Children.Add(side);

        Grid.SetRow(_optionsHost, 1);
        Grid.SetColumn(_optionsHost, 0);
        listRow.Children.Add(_optionsHost);

        // Schema from Core; chromePath default from settings (runtime preference, not business rule).
        var overrides = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(_settings.Current.ChromeDriverPath))
        {
            overrides["chromePath"] = _settings.Current.ChromeDriverPath;
        }

        _options.BuildInto(_optionsHost, FeatureOptionSchemas.GetSections(_feature.Id), overrides);

        // 格式选择的实测口径（A4 300dpi 扫描页：TIF/LZW 仅比未压缩小 0.7%–1.5%，而彩色照片 TIF 反而涨到 102.6%）：
        // 这行只提示选择方向，不展开讲压缩原理。
        if (string.Equals(_feature.Id, "image.compress", StringComparison.OrdinalIgnoreCase))
        {
            _optionsHost.Children.Add(ToolViewKit.Hint("TIF 适合扫描件/二值文档；彩色照片请选 JPEG。"));
        }

        var actionBar = new DockPanel { Margin = new Thickness(0, 8, 0, 0), LastChildFill = true };
        var choose = MakeButton("选择文件", ControlAppearance.Primary, ChooseFiles, SymbolRegular.FolderOpen24);
        _runButton = MakeButton("开始处理", ControlAppearance.Success, () => _ = RunAsync(),
            SymbolRegular.Play24);

        // 长耗时操作必须能叫停：处理中才出现，取消后状态行说明「已处理几个、还剩几个」。
        _cancelButton = MakeButton("取消", ControlAppearance.Secondary, () => _cts?.Cancel(),
            SymbolRegular.Dismiss24);
        _cancelButton.Visibility = Visibility.Collapsed;
        _cancelButton.MinWidth = 96;

        // 失败清单可复制，省得用户从状态行里手抄前三条。
        _copyFailuresButton = MakeButton("复制失败清单", ControlAppearance.Secondary, CopyFailures,
            SymbolRegular.Copy24);
        _copyFailuresButton.Visibility = Visibility.Collapsed;
        _copyFailuresButton.MinWidth = 140;

        var rightGroup = new StackPanel { Orientation = Orientation.Horizontal };
        rightGroup.Children.Add(_copyFailuresButton);
        rightGroup.Children.Add(_cancelButton);
        rightGroup.Children.Add(_runButton);

        DockPanel.SetDock(choose, Dock.Left);
        DockPanel.SetDock(rightGroup, Dock.Right);
        actionBar.Children.Add(choose);
        actionBar.Children.Add(rightGroup);
        actionBar.Children.Add(_status);

        SetBody(BuildBottomActionLayout(listRow, null, actionBar));
    }

    private async Task RunAsync()
    {
        if (_running)
        {
            return;
        }

        if (_operation is null)
        {
            ToolViewKit.ShowStatus(_status, $"该功能没有注册处理流程：{_feature.Id}", StatusTone.Error);
            return;
        }

        var files = _fileList.Paths;
        if (files.Count == 0)
        {
            // 契约：列表为空只提示，不弹目录选择。
            ToolViewKit.ShowStatus(_status, "请先点「选择文件」把要处理的文件加进列表。", StatusTone.Error);
            return;
        }

        _running = true;
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;

        if (_runButton is not null)
        {
            _runButton.IsEnabled = false;
        }

        if (_cancelButton is not null)
        {
            _cancelButton.Visibility = Visibility.Visible;
        }

        if (_copyFailuresButton is not null)
        {
            _copyFailuresButton.Visibility = Visibility.Collapsed;
        }

        // 每轮开始清掉上一轮的行颜色与失败原因，避免「上一轮的红行」被当成本轮结果。
        _fileList.ResetStates();

        try
        {
            if (_feature.Id is "pdf.merge" or "word.merge")
            {
                await RunMergeAsync(files, token);
                return;
            }

            bool saveSource = _options.GetBool("saveSource");

            // image.pdf 在未勾选「每张图一个 PDF」时是批处理（合并成 1 个 PDF），必须整体调用。
            // 产物只有一个文件，无法「各回各家」，所以这种情形仍需一个统一目录。
            if (_feature.Id == "image.pdf" && !_options.GetBool("onePdfPerImage"))
            {
                string? batchOutput = saveSource ? ResolveSharedDirectory(files) : PickOutputFolder();
                if (string.IsNullOrWhiteSpace(batchOutput))
                {
                    ShowCancelled();
                    return;
                }

                await RunBatchAsync(files, batchOutput, token);
                return;
            }

            if (saveSource)
            {
                // 逐文件调用 → 每个文件都能写回自己的目录（规范 §3.2），不需要统一目录，也不弹框。
                await RunPerFileAsync(files, outputDirectory: null, writeToSource: true, token);
                return;
            }

            string? output = PickOutputFolder();
            if (string.IsNullOrWhiteSpace(output))
            {
                // 契约：取消目录选择静默返回，只留状态行。
                ShowCancelled();
                return;
            }

            await RunPerFileAsync(files, output, writeToSource: false, token);
        }
        finally
        {
            _running = false;
            _cts?.Dispose();
            _cts = null;

            if (_runButton is not null)
            {
                _runButton.IsEnabled = true;
            }

            if (_cancelButton is not null)
            {
                _cancelButton.Visibility = Visibility.Collapsed;
            }

            if (_copyFailuresButton is not null)
            {
                _copyFailuresButton.Visibility =
                    _fileList.FailedItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    /// <summary>把失败行的「文件 → 路径 → 原因」整段复制到剪贴板。</summary>
    private void CopyFailures()
    {
        IReadOnlyList<FileListItem> failed = _fileList.FailedItems;
        if (failed.Count == 0)
        {
            ToolViewKit.ShowStatus(_status, "本轮没有失败的文件。");
            return;
        }

        string text = string.Join(
            Environment.NewLine,
            failed.Select(i => $"{i.Name}\t{i.FullPath}\t{i.Reason ?? "失败原因未记录"}"));
        try
        {
            Clipboard.SetText(text);
            ToolViewKit.ShowStatus(_status, $"已复制 {failed.Count} 条失败记录（文件名、路径、原因）。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "复制失败：剪贴板正被其他程序占用，请稍后重试。", StatusTone.Error);
            _copyFailuresButton!.ToolTip = ex.Message;
        }
    }

    private string? PickOutputFolder()
    {
        string? folder = _dialogs.PickFolder();
        return string.IsNullOrWhiteSpace(folder) ? null : folder;
    }

    private void ShowCancelled()
        => ToolViewKit.ShowStatus(_status, "已取消，没有处理任何文件。");

    /// <summary>
    /// 合并类功能（<c>image.pdf</c>）的产物只有一个文件，必须落在同一个目录：
    /// 输入同目录时直接用该目录，跨目录才让用户选（无法「各回各家」）。
    /// </summary>
    private string? ResolveSharedDirectory(IReadOnlyList<string> files)
    {
        var directories = DistinctDirectories(files);
        if (directories.Count == 1)
        {
            return directories[0];
        }

        ToolViewKit.ShowStatus(
            _status,
            $"合并结果只能保存到一个目录，但所选文件来自 {directories.Count} 个不同目录；请选择输出目录。",
            StatusTone.Busy);
        return PickOutputFolder();
    }

    private static List<string> DistinctDirectories(IReadOnlyList<string> files)
        => files
            .Select(f => Path.GetDirectoryName(f))
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>逐文件执行：给「第 N 个 / 共 M 个」进度、行级成功/失败着色，并汇总成功/失败个数。</summary>
    /// <param name="outputDirectory">统一输出目录；<paramref name="writeToSource"/> 为真时不用。</param>
    /// <param name="writeToSource">勾选「保存到源目录」时为真：每个文件写回它自己的目录。</param>
    /// <param name="token">取消令牌：点「取消」后在两个文件之间停下，已在处理的那个文件跑完。</param>
    private async Task RunPerFileAsync(
        IReadOnlyList<string> files,
        string? outputDirectory,
        bool writeToSource,
        CancellationToken token)
    {
        int total = files.Count;
        int succeeded = 0;
        int failed = 0;
        bool cancelled = false;
        var outputs = new List<string>();
        var reasons = new List<string>();
        IDictionary<string, string> options = _options.Collect();

        int distinctDirectories = writeToSource ? DistinctDirectories(files).Count : 0;

        for (int i = 0; i < total; i++)
        {
            if (token.IsCancellationRequested)
            {
                cancelled = true;
                break;
            }

            string file = files[i];
            string fileName = Path.GetFileName(file);

            string? fileDirectory = writeToSource ? Path.GetDirectoryName(file) : null;
            if (writeToSource && string.IsNullOrWhiteSpace(fileDirectory))
            {
                failed++;
                reasons.Add($"{fileName}（无法确定该文件所在目录）");
                _fileList.MarkState(file, FileRowState.Failed, "无法确定该文件所在目录");
                continue;
            }

            _fileList.MarkState(file, FileRowState.Running);
            ToolViewKit.ShowStatus(
                _status,
                writeToSource
                    ? $"正在处理第 {i + 1} 个文件 / 共 {total} 个：{fileName}（保存到本文件所在目录：{fileDirectory}）"
                    : $"正在处理第 {i + 1} 个文件 / 共 {total} 个：{fileName}",
                StatusTone.Busy);
            DocumentOperationResult result;
            try
            {
                result = await _operationRunner.ExecuteAsync(
                    _operation!,
                    new DocumentOperationRequest
                    {
                        InputFiles = [file],
                        OutputDirectory = writeToSource ? fileDirectory! : outputDirectory!,
                        Options = options
                    },
                    token);
            }
            catch (OperationCanceledException)
            {
                cancelled = true;
                _fileList.MarkState(file, FileRowState.None);
                break;
            }

            if (result.Success)
            {
                succeeded++;
                outputs.AddRange(result.OutputFiles);
                _fileList.MarkState(file, FileRowState.Success);
            }
            else
            {
                failed++;
                reasons.Add($"{fileName}（{result.Message}）");
                _fileList.MarkState(file, FileRowState.Failed, result.Message);
            }
        }

        if (cancelled)
        {
            int remaining = total - succeeded - failed;
            ToolViewKit.ShowStatus(
                _status,
                $"已取消：成功 {succeeded} 个，失败 {failed} 个，剩余 {remaining} 个未处理。",
                StatusTone.Neutral);
        }

        string writtenBack = writeToSource && distinctDirectories > 1 ? "，已分别写回各自源目录" : string.Empty;

        string message = cancelled
            ? $"已取消：成功 {succeeded} 个{writtenBack}，失败 {failed} 个，剩余 {total - succeeded - failed} 个未处理。"
            : failed == 0
                ? $"处理完毕，成功 {succeeded} 个文件{writtenBack}。"
                : $"处理完毕，成功 {succeeded} 个{writtenBack}，失败 {failed} 个。{string.Join("；", reasons.Take(3))}"
                  + (reasons.Count > 3 ? $"（另有 {reasons.Count - 3} 个失败未列出）" : string.Empty);

        // 跨目录写回时没有「唯一的输出目录」可打开，因此不给打开目录的提示。
        bool singleOutputDirectory = !writeToSource || distinctDirectories <= 1;
        string remembered = writeToSource
            ? DistinctDirectories(files).FirstOrDefault() ?? string.Empty
            : outputDirectory!;

        Finish(
            new DocumentOperationResult
            {
                Success = succeeded > 0,
                Message = message,
                OutputFiles = outputs
            },
            remembered,
            failed,
            offerOpenDirectory: singleOutputDirectory && !cancelled);
    }

    /// <summary>批处理（image.pdf 合并）：一次调用，进度只报总数。</summary>
    private async Task RunBatchAsync(IReadOnlyList<string> files, string output, CancellationToken token)
    {
        foreach (string file in files)
        {
            _fileList.MarkState(file, FileRowState.Running);
        }

        ToolViewKit.ShowStatus(_status, $"正在合并 {files.Count} 张图片为 1 个 PDF…", StatusTone.Busy);
        DocumentOperationResult result;
        try
        {
            result = await _operationRunner.ExecuteAsync(
                _operation!,
                new DocumentOperationRequest
                {
                    InputFiles = files.ToList(),
                    OutputDirectory = output,
                    Options = _options.Collect()
                },
                token);
        }
        catch (OperationCanceledException)
        {
            foreach (string file in files)
            {
                _fileList.MarkState(file, FileRowState.None);
            }

            ToolViewKit.ShowStatus(_status, "已取消：合并没有完成，输出目录里可能留有不完整的文件。", StatusTone.Error);
            return;
        }

        foreach (string file in files)
        {
            _fileList.MarkState(
                file,
                result.Success ? FileRowState.Success : FileRowState.Failed,
                result.Success ? null : result.Message);
        }

        Finish(result, output);
    }

    private async Task RunMergeAsync(IReadOnlyList<string> files, CancellationToken token)
    {
        string filter = _feature.Id == "pdf.merge" ? "PDF 文档|*.pdf" : "WORD 文档|*.docx";
        string defaultName = _feature.Id == "pdf.merge" ? "合并.pdf" : "合并.docx";
        string? output = _dialogs.SaveFile(filter, defaultName);
        if (output is null)
        {
            ToolViewKit.ShowStatus(_status, "已取消，没有处理任何文件。");
            return;
        }

        string? dir = Path.GetDirectoryName(output);
        if (string.IsNullOrWhiteSpace(dir))
        {
            ToolViewKit.ShowStatus(_status, $"无法确定输出目录，请换一个保存位置：{output}", StatusTone.Error);
            return;
        }

        var mergeOptions = _options.Collect();
        mergeOptions["outputFile"] = output;
        ToolViewKit.ShowStatus(_status, $"正在合并 {files.Count} 个文件…", StatusTone.Busy);
        DocumentOperationResult mergeResult;
        try
        {
            mergeResult = await _operationRunner.ExecuteAsync(
                _operation!,
                new DocumentOperationRequest
                {
                    InputFiles = files.ToList(),
                    OutputDirectory = dir,
                    Options = mergeOptions
                },
                token);
        }
        catch (OperationCanceledException)
        {
            ToolViewKit.ShowStatus(_status, "已取消：合并没有完成。", StatusTone.Neutral);
            return;
        }

        Finish(mergeResult, dir);
    }

    /// <summary>落状态行、记住输出、按需打开产物/输出目录。</summary>
    /// <param name="outputDir">用于记忆「上次输出目录」；跨目录写回时是其中一个源目录。</param>
    /// <param name="failedCount">大于 0 表示有失败，状态行转为错误色。</param>
    /// <param name="offerOpenDirectory">跨目录写回时没有唯一输出目录，此时跳过「是否打开目录」询问。</param>
    private void Finish(
        DocumentOperationResult result,
        string outputDir,
        int failedCount = 0,
        bool offerOpenDirectory = true)
    {
        if (result.Success)
        {
            FileListContextWiring.RememberOutputs(
                _fileList,
                _convertedBySource,
                _fileList.Paths,
                result.OutputFiles,
                outputDir);

            ToolViewKit.ShowStatus(
                _status,
                result.Message,
                failedCount > 0 ? StatusTone.Error : StatusTone.Success);
            if (_options.GetBool("openPdfOptions") && result.OutputFiles is { Count: > 0 })
            {
                foreach (string pdf in result.OutputFiles.Where(f =>
                             f.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                             // 「这个产物还在不在」只该有一个判定：PathGuard（见 docs/decoupling-audit.md 判例 5）。
                             // 自己写 File.Exists 会让「空路径算不算就绪」这类口径在下一次复制时走偏。
                             && PathGuard.MissingFileReason(f, "输出文件") is null))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo { FileName = pdf, UseShellExecute = true });
                    }
                    catch
                    {
                        // ignore open failures
                    }
                }
            }

            if (offerOpenDirectory &&
                _settings.Current.ShowOpenTargetDialog &&
                _dialogs.Confirm("处理完毕，是否需要打开转换后目录？", "提示"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = outputDir,
                    UseShellExecute = true
                });
            }
        }
        else
        {
            ToolViewKit.ShowStatus(_status, result.Message, StatusTone.Error);
        }
    }

    private static Button MakeButton(
        string text,
        ControlAppearance appearance,
        Action click,
        SymbolRegular? icon = null)
    {
        object content = text;
        if (icon is { } symbol)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(FeatureIcons.Create(symbol, fontSize: 16, rightMargin: 8));
            row.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center
            });
            content = row;
        }

        var btn = new Button
        {
            Content = content,
            Appearance = appearance,
            Padding = new Thickness(18, 8, 18, 8),
            MinWidth = 120,
            MinHeight = 36
        };
        btn.Click += (_, _) => click();
        return btn;
    }

    private static Button SideBtn(SymbolRegular symbol, string toolTip, Action click)
    {
        var btn = new Button
        {
            Content = FeatureIcons.Create(symbol, fontSize: 16, rightMargin: 0),
            Appearance = ControlAppearance.Secondary,
            Width = 36,
            Height = 40,
            Margin = new Thickness(0, 0, 0, 6),
            Padding = new Thickness(0),
            ToolTip = toolTip
        };
        btn.Click += (_, _) => click();
        return btn;
    }
}
