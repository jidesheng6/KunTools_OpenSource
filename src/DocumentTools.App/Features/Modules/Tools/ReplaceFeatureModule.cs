using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui.Controls;
using DocumentTools.App.Ui.Options;
using DocumentTools.Core.Features;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Office;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Tools;

public sealed class ReplaceFeatureModule : FeatureModuleBase
{
    public ReplaceFeatureModule(FeatureDescriptor descriptor)
        : base(descriptor)
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new ReplacePairsView(
            Descriptor,
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<IEnumerable<IDocumentOperation>>(),
            services.GetRequiredService<IAppSettings>(),
            services.GetRequiredService<IDocumentInfoService>());

    public static IEnumerable<IFeatureModule> CreateAll()
    {
        yield return new ReplaceFeatureModule(BuiltInFeatures.All.First(f => f.Id == "word.replace"));
        yield return new ReplaceFeatureModule(BuiltInFeatures.All.First(f => f.Id == "excel.replace"));
    }
}

/// <summary>
/// Presentation: dynamic find/replace pairs (UI-only) + schema-driven match options from Core.
/// Execution: <see cref="IDocumentOperation"/> Strategy.
/// Layout (spec archetype D): file list fills the top, find/replace + options scroll above the action bar.
/// </summary>
public sealed class ReplacePairsView : FeaturePageBase
{
    private const double OptionsMaxHeight = 300;

    private readonly FeatureDescriptor _feature;
    private readonly IDialogService _dialogs;
    private readonly IDocumentOperation? _operation;
    private readonly IAppSettings _settings;
    private readonly BorderedFileList _list = new();
    private readonly Dictionary<string, string> _convertedBySource = new(StringComparer.OrdinalIgnoreCase);
    private readonly StackPanel _findHost = new();
    private readonly StackPanel _replaceHost = new();
    private readonly List<PairRow> _pairs = new();
    private readonly StackPanel _optionsHost = new();
    private readonly OptionPanelBuilder _options;
    private readonly TextBlock _status = ToolViewKit.Status("先填写查找/替换对并选择文件，再点「开始处理」。");

    public ReplacePairsView(
        FeatureDescriptor feature,
        IDialogService dialogs,
        IEnumerable<IDocumentOperation> operations,
        IAppSettings settings,
        IDocumentInfoService documentInfo)
        : base(feature.Title, "添加要查找和替换的内容，可按需要设置大小写、全字匹配等选项。")
    {
        _feature = feature;
        _dialogs = dialogs;
        _settings = settings;
        _options = new OptionPanelBuilder(dialogs);
        _operation = operations.FirstOrDefault(o =>
            string.Equals(o.FeatureId, feature.Id, StringComparison.OrdinalIgnoreCase));
        _list.ShowGridLines = settings.Current.ListViewShowGridLines;
        _list.RowHeight = settings.Current.ListViewLineHeight;
        FileListContextWiring.Attach(_list, dialogs, documentInfo, _convertedBySource);

        AddPair();
        _options.BuildInto(_optionsHost, FeatureOptionSchemas.GetSections(feature.Id));

        // 拖放与「选择文件」共用同一份过滤串；空串表示该功能不限后缀（放行）。
        _list.FileFilter = FeatureOptionSchemas.GetFileFilter(feature.Id);
        _list.FilesRejected = rejected => ToolViewKit.ShowStatus(
            _status,
            $"已忽略 {rejected} 个不支持的文件。",
            StatusTone.Error);

        // 空态：列表 0 项时整页没有引导语，用户只看到一片空白（与原「转换」页同一套文案口径）。
        string documentKind = feature.Id.StartsWith("excel", StringComparison.OrdinalIgnoreCase) ? "Excel" : "Word";
        _list.EmptyHint = $"还没有文件：点下面的「选择文件」，或把 {documentKind} 文档拖到这里。";
        _list.EmptyActionText = "选择文件";
        _list.EmptyAction = () => _list.AddFiles(dialogs.OpenFiles(FeatureOptionSchemas.GetFileFilter(feature.Id)));

        var pairBar = ToolViewKit.ActionBar(ToolViewKit.Secondary("添加查找项", AddPair));

        // 原型 D：查找 / 替换是两个分区（对齐 legacy WordReplaceFindPanel / WordReplaceReplacePanel）。
        Border findCard = ToolViewKit.Card(
            "查找内容",
            ToolViewKit.Hint("第 N 行与「替换内容」的第 N 行配对；「查找」留空的行不写入。"),
            pairBar,
            _findHost);
        Border replaceCard = ToolViewKit.Card("替换内容", _replaceHost);

        var optionArea = new ScrollViewer
        {
            Content = new StackPanel { Children = { findCard, replaceCard, _optionsHost } },
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            MaxHeight = OptionsMaxHeight,
            Margin = new Thickness(0, 4, 0, 0),

            // 容器不进 Tab 序列：Tab 应先落到「查找」输入框，而不是滚动区本身。
            IsTabStop = false
        };

        var addFiles = ToolViewKit.Secondary("选择文件", () =>
            _list.AddFiles(dialogs.OpenFiles(FeatureOptionSchemas.GetFileFilter(feature.Id))));
        addFiles.MinWidth = 120;

        Button run = ToolViewKit.Primary("开始处理", () => { });
        run.MinWidth = 120;
        run.Click += (_, _) => _ = RunAsync(run);

        SetBody(BuildToolLayout(_list, optionArea, _status, ToolViewKit.ActionBar(addFiles, run)));

        // 键盘：焦点不在列表/输入框里时 Enter 开始处理；Esc 收回状态行（不动已填的替换对）。
        ToolViewKit.AttachPageKeyboard(
            this,
            run,
            () => ToolViewKit.ShowStatus(_status, "先填写查找/替换对并选择文件，再点「开始处理」。"));
    }

    private async Task RunAsync(Button run)
    {
        if (_operation is null)
        {
            ToolViewKit.ShowStatus(_status, $"该功能没有注册处理流程：{_feature.Id}", StatusTone.Error);
            return;
        }

        if (_list.Paths.Count == 0)
        {
            ToolViewKit.ShowStatus(_status, "请先点「选择文件」加入要处理的文档。", StatusTone.Error);
            return;
        }

        var options = _options.Collect();
        AppendPairs(options);
        if (!options.ContainsKey("pairCount") || options["pairCount"] == "0")
        {
            ToolViewKit.ShowStatus(_status, "请至少填写一组「查找内容」，空行不会写入。", StatusTone.Error);
            return;
        }

        string? dir = _dialogs.PickFolder();
        if (string.IsNullOrWhiteSpace(dir))
        {
            ToolViewKit.ShowStatus(_status, "已取消，没有处理任何文件。");
            return;
        }

        run.IsEnabled = false;
        ToolViewKit.ShowStatus(_status, $"正在处理 {_list.Paths.Count} 个文件…", StatusTone.Busy);
        try
        {
            var result = await _operation.ExecuteAsync(new DocumentOperationRequest
            {
                InputFiles = _list.Paths.ToList(),
                OutputDirectory = dir,
                Options = options
            });

            if (result.Success)
            {
                FileListContextWiring.RememberOutputs(
                    _list,
                    _convertedBySource,
                    _list.Paths,
                    result.OutputFiles,
                    dir);
                ToolViewKit.ShowStatus(_status, result.Message, StatusTone.Success);

                if (_settings.Current.ShowOpenTargetDialog &&
                    _dialogs.Confirm("处理完毕，是否需要打开转换后目录？", "提示"))
                {
                    Process.Start(new ProcessStartInfo { FileName = dir, UseShellExecute = true });
                }
            }
            else
            {
                ToolViewKit.ShowStatus(_status, result.Message, StatusTone.Error);
            }
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "处理失败：" + ex.Message, StatusTone.Error);
        }
        finally
        {
            run.IsEnabled = true;
        }
    }

    private void AppendPairs(Dictionary<string, string> options)
    {
        int index = 0;
        foreach (PairRow pair in _pairs)
        {
            string find = pair.Find.Text?.Trim() ?? string.Empty;
            if (find.Length == 0)
            {
                continue;
            }

            options[$"pair{index}.find"] = find;
            options[$"pair{index}.replace"] = pair.Replace.Text?.Trim() ?? string.Empty;
            index++;
        }

        options["pairCount"] = index.ToString();
    }

    private void AddPair()
    {
        var find = new TextBox { PlaceholderText = "要查找的文字" };
        var replace = new TextBox { PlaceholderText = "替换为（留空表示删除）" };

        Button remove = ToolViewKit.Secondary("移除", () => { });
        remove.Padding = new Thickness(10, 4, 10, 4);
        remove.Margin = new Thickness(8, 0, 0, 0);
        remove.MinWidth = 0;

        // 读屏软件：多组「移除」按钮念起来完全一样，要用名称说清"移除哪一组"；
        // 「查找/替换」框靠占位提示区分，输入后提示消失，也给名称。
        AutomationProperties.SetName(remove, $"移除第 {_pairs.Count + 1} 组查找替换");
        AutomationProperties.SetName(find, $"第 {_pairs.Count + 1} 组：查找内容");
        AutomationProperties.SetName(replace, $"第 {_pairs.Count + 1} 组：替换内容");

        var findRow = new DockPanel { Height = PairRowHeight, LastChildFill = true };
        DockPanel.SetDock(remove, Dock.Right);
        findRow.Children.Add(remove);
        findRow.Children.Add(find);

        var replaceRow = new StackPanel { Height = PairRowHeight };
        replaceRow.Children.Add(replace);

        var pair = new PairRow(findRow, replaceRow, find, replace);
        remove.Click += (_, _) => RemovePair(pair);

        _pairs.Add(pair);
        _findHost.Children.Add(findRow);
        _replaceHost.Children.Add(replaceRow);
    }

    private void RemovePair(PairRow pair)
    {
        if (_pairs.Count <= 1)
        {
            ToolViewKit.ShowStatus(_status, "至少保留一组查找/替换对；要停用请把「查找」留空。", StatusTone.Error);
            return;
        }

        _pairs.Remove(pair);
        _findHost.Children.Remove(pair.FindRow);
        _replaceHost.Children.Remove(pair.ReplaceRow);
    }

    /// <summary>一张「查找」行和它对应的「替换」行必须等高，才能看出谁和谁配对。</summary>
    private const double PairRowHeight = 36;

    private sealed class PairRow(DockPanel findRow, StackPanel replaceRow, TextBox find, TextBox replace)
    {
        public DockPanel FindRow { get; } = findRow;

        public StackPanel ReplaceRow { get; } = replaceRow;

        public TextBox Find { get; } = find;

        public TextBox Replace { get; } = replace;
    }
}
