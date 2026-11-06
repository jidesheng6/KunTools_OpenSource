using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Exam;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// exam.report — replace Item= / Raid= placeholders inside Report_Detail.Content
/// (port of the legacy 报表内容批量替换 path).
/// </summary>
public sealed class ExamReportView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly IExamReportService _reports;
    private readonly TextBox _database = ExamViewKit.PathBox("考评库", "考评 Access 数据库");
    private readonly ListView _list = new();
    private readonly ScrollViewer _listHost = new()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
    };
    private readonly Border _listFrame;
    /// <summary>
    /// 项目筛选：可编辑下拉（legacy <c>ExamReportRowMode_ExamList</c> 默认 DropDown、中键切换 DropDownList）。
    /// 下拉里是报表实际用到的项目名（「所有项目」在首位），也允许手填库里暂时没有的名字。
    /// </summary>
    private readonly ComboBox _itemName = new() { IsEditable = true, MinWidth = 220, Text = ExamReportService.AllItemsName };
    private readonly TextBox _newItemName = new() { PlaceholderText = "新的项目名称" };
    private readonly TextBox _newRaid = new() { PlaceholderText = "新的 Raid 编号" };
    private readonly TextBox _oldRaid = new() { PlaceholderText = "只替换该 Raid（留空=全部）", Width = 240 };
    private readonly TextBox _newDataFormat = new() { PlaceholderText = "新的数据格式（Kind=…）" };
    private readonly System.Windows.Controls.CheckBox _changeItem = new() { Content = "替换项目名称 (Item=…)" };
    private readonly System.Windows.Controls.CheckBox _changeRaid = new() { Content = "替换 Raid 编号 (Raid=…)" };
    private readonly System.Windows.Controls.CheckBox _changeFormat = new() { Content = "替换数据格式 (Kind=…)" };
    private readonly ComboBox _raidOptions = new() { MinWidth = 260 };

    /// <summary>考评表定义里的项目（legacy ExamItemsCombox）；选中后联动加载该项目的选项。</summary>
    private readonly ComboBox _examItems = new() { MinWidth = 220 };

    /// <summary>所选考评表项目的选项（legacy ExamItemsSelectorsCombox，已排除「无效」）。</summary>
    private readonly ComboBox _examSelectors = new() { MinWidth = 220 };
    private readonly TextBlock _gridInfo = ExamViewKit.Hint("选中报表后可读取结构。");
    private readonly TextBlock _status = ExamViewKit.Status("先打开考评库并刷新报表列表。");

    // 批量参数（legacy ChangeExamReportParaContent，Util.cs:864）：数据类型/身份、小数位数、项目排序、复制到剪贴板。
    private readonly ComboBox _bulkMode = new() { MinWidth = 110 };
    private readonly TextBox _bulkLine = new() { Width = 72, Text = "1" };
    private readonly TextBox _bulkStart = new() { Width = 72, Text = "1" };
    private readonly TextBox _bulkEnd = new() { Width = 72, Text = "1" };
    private readonly ComboBox _bulkKind = new() { MinWidth = 170 };
    private readonly ComboBox _bulkDecimals = new() { MinWidth = 130 };
    private readonly TextBox _bulkRaid = new() { Width = 210, PlaceholderText = "身份（列模式用 | 分隔）" };

    /// <summary>
    /// 批量参数的身份下拉（legacy <c>ExamReportParaRaidInfo_Combox</c>，含「合计」「批量设置全部身份」）。
    /// 手打「A|B|C|A，B，C」是不可能记住的口径，下拉只是把同一份 Value 填进文本框。
    /// </summary>
    private readonly ComboBox _bulkRaidPick = new() { MinWidth = 150 };

    // 字体三段（legacy「字体,大小,加粗」，Designer:12933 的默认值 宋体,11,0）。
    // 拆成三个控件而不是一个手填文本框：名字段写错只会在写库后才于报表渲染时暴露。
    private readonly ComboBox _bulkFontName = new() { MinWidth = 160 };
    private readonly TextBox _bulkFontSize = new() { Width = 70, Text = DefaultFontSize };
    private readonly CheckBox _bulkFontBold = new() { Content = "加粗", VerticalAlignment = VerticalAlignment.Center };
    private readonly TextBox _bulkItems = new() { Width = 170, PlaceholderText = "项目名，多个用 | 分隔" };
    private readonly TextBox _bulkSelectors = new() { Width = 170, PlaceholderText = "选项，多个用 | 分隔" };

    /// <summary>
    /// legacy「插入相同项目 / 插入相同选项」（Designer:12776/12787，默认勾选）。
    /// 勾选 = 按单元格键去重（同一格可写多条内容）；不勾 = 键+值双去重（不同身份会塌成一行）。
    /// </summary>
    private readonly System.Windows.Controls.CheckBox _bulkInsertSame = new()
    {
        Content = "插入相同项目 / 选项",
        IsChecked = true,
        VerticalAlignment = VerticalAlignment.Center,
        ToolTip = "勾选：同一单元格可写入多条不同内容（按单元格去重）。取消：内容相同的行不再重复插入（不同身份会合并成一行）。",
        Margin = new Thickness(0, 0, 0, 8)
    };
    private readonly ListView _itemList = new();
    private readonly TextBlock _bulkInfo = ExamViewKit.Hint("先选中报表；「批量设置」只写选定的行/列范围。");

    /// <summary>项目顺序的备份：读库时记一份物理顺序，「重置项目排序」用它还原（legacy TempExamReportItemLists_NoSort）。</summary>
    private IReadOnlyList<string> _storedItems = Array.Empty<string>();

    private readonly Button _apply;

    private const string ReportEmptyHint = "还没有报表：选择考评库后点「刷新报表」。";

    // 默认字体串的唯一来源是 Core（ExamReportService.DefaultFontSpec），这里只拆段，不另抄一份常量。
    private static readonly string[] DefaultFontParts = ExamReportService.DefaultFontSpec.Split(',');

    private static string DefaultFontName => DefaultFontParts[0];

    private static string DefaultFontSize => DefaultFontParts.Length > 1 ? DefaultFontParts[1] : "11";

    public ExamReportView(FeatureDescriptor descriptor, IServiceProvider services)
        : base(descriptor.Title, "批量替换报表正文里的 Item= / Raid= 占位（「所有项目」= 全部项目）。")
    {
        _dialogs = services.GetRequiredService<IDialogService>();
        _reports = services.GetRequiredService<IExamReportService>();
        IAppSettings settings = services.GetRequiredService<IAppSettings>();

        _database.Text = settings.Current.VfsoftExamDataPath;
        BuildColumns();

        _list.SelectionChanged += (_, _) =>
        {
            if (_list.SelectedItem is ExamReportRow row)
            {
                ExamViewKit.ShowStatus(_status, $"已选报表：{row.ReportName}（考评表 {row.ExamName}）。");
                LoadReportContext(row);
                LoadItems(row);
                SyncReady();
            }
            else
            {
                _raidOptions.ItemsSource = null;
                _raidOptions.IsEnabled = false;
                _examItems.ItemsSource = null;
                _examSelectors.ItemsSource = null;
                _gridInfo.Text = "选中报表后可读取结构。";
                SyncReady();
            }
        };

        // 没选报表就没有身份可选：先禁用，避免用户点开一个空下拉。
        _raidOptions.IsEnabled = false;
        _raidOptions.DisplayMemberPath = nameof(RaidChoice.Display);
        _raidOptions.SelectionChanged += (_, _) =>
        {
            // "a|b|合计" 这类批量身份只在列模式下有意义（legacy Util.cs:4403 + BodyForm.cs:7045），
            // 直接回填到「新 Raid」会被 Core 当成字面值，所以只提示不写入。
            if (_raidOptions.SelectedItem is not RaidChoice choice || choice.Value.Length == 0)
            {
                return;
            }

            if (ExamReportService.IsMultiRaidValue(choice.Value))
            {
                // 只提示不清理会误写库：先选普通身份「甲」，再切到批量组合值时输入框里还留着「甲」，
                // 此时直接点「执行替换」会把「甲」当成 NewRaid 写进去。所以清空并取消勾选。
                _newRaid.Text = string.Empty;
                _changeRaid.IsChecked = false;
                ExamViewKit.ShowStatus(
                    _status,
                    "这是「批量设置全部身份」组合值，只在列模式下生效；已清空「新 Raid」并取消勾选，请手工填写其中一个身份。",
                    error: true);
                return;
            }

            _newRaid.Text = choice.Value;
        };

        // 批量参数的身份下拉：Core 的 ListExamRaids 已给出「逐身份 / 合计 / 批量全部」三种 Value，
        // 这里只做回填；拼接口径不在这里判断（IsMultiRaidValue 是 Core 的判据）。
        AutomationProperties.SetName(_bulkRaidPick, "批量参数身份");
        _bulkRaidPick.SelectionChanged += (_, _) =>
        {
            if (_bulkRaidPick.SelectedItem is not ExamRaidOption option)
            {
                return;
            }

            if (option.Value.Length == 0)
            {
                ExamViewKit.ShowStatus(_status, "该考评表没有测评身份，下拉里没有可填的内容。", error: true);
                return;
            }

            _bulkRaid.Text = option.Value;
            if (ExamReportService.IsMultiRaidValue(option.Value))
            {
                ExamViewKit.ShowStatus(
                    _status,
                    "已填入批量身份：只有「列」范围会按 | 拆成多个身份逐行写入；行范围会把整段内容作为一个身份。",
                    StatusTone.Busy);
            }
        };

        // 联动：选考评表项目 → 加载该项目的选项（legacy ExamItemsCombox_SelectedIndexChanged）。
        // 只提示、不写库；失败也不弹框（dev 这里是 MessageBox，迁移统一改状态行，减少打扰）。
        _examItems.SelectionChanged += (_, _) => LoadExamSelectors();

        // 报表项目列表右键菜单（legacy ExamReportItemSortMenu 挂在 ExamReportRowMode_ExamList 上）。
        _itemList.ContextMenu = BuildItemSortMenu();

        Border source = ExamViewKit.Card(
            "数据库",
            ExamViewKit.Field("考评库", _database, ExamViewKit.Secondary("选择…", () =>
            {
                ExamViewKit.PickFile(_dialogs, _database, ExamViewKit.AccessDatabaseFilter);
                RefreshReports();
            })));

        _listHost.Content = _list;
        _listFrame = ExamViewKit.ListFrame(_listHost, 180);
        Border list = ExamViewKit.Card("报表列表", _listFrame);

        // 「读取结构与身份」是这张卡的动作，不再塞进标签列：结构摘要自己占一行，读到了就写在这一行。
        Border structure = ExamViewKit.Card(
            "报表结构",
            _gridInfo,
            ExamViewKit.ActionBar(ExamViewKit.Secondary("读取结构与身份", () =>
            {
                if (_list.SelectedItem is ExamReportRow row)
                {
                    LoadReportContext(row);
                }
                else
                {
                    ExamViewKit.ShowStatus(_status, "请先选中一张报表。", error: true);
                }
            })),
            ExamViewKit.Field("测评身份", _raidOptions),
            ExamViewKit.Field("考评表项目", _examItems),
            ExamViewKit.Field("项目选项", _examSelectors));

        Border replace = ExamViewKit.Card(
            "内容替换",
            ExamViewKit.Hint("按项目名筛选报表正文里的占位；只有勾选的动作会写库，未勾选的字段不参与。「新 Raid」留空表示删除该评测主体；建议执行前关闭报表设计页面。"),
            ExamViewKit.Field("项目筛选", _itemName),
            _changeItem,
            ExamViewKit.Field("新项目名", _newItemName),
            _changeRaid,
            ExamViewKit.Field("原 Raid", _oldRaid),
            ExamViewKit.Field("新 Raid", _newRaid),
            _changeFormat,
            ExamViewKit.Field("新数据格式", _newDataFormat),
            ExamViewKit.ActionBar(ExamViewKit.Secondary("清除勾选", () =>
            {
                _changeItem.IsChecked = false;
                _changeRaid.IsChecked = false;
                _changeFormat.IsChecked = false;
            })));

        // 本页的产出动作是「把替换写进报表正文」，保存/刷新这类维护动作放在 Secondary。
        _apply = ExamViewKit.Primary("执行替换", Save);
        var actions = ExamViewKit.ActionBar(_apply, ExamViewKit.Secondary("刷新报表", RefreshReports));

        _bulkMode.ItemsSource = ExamReportService.ModeOptions;
        _bulkMode.SelectedIndex = 0;
        _bulkKind.ItemsSource = ExamReportService.DataFormatKinds.ToList();
        _bulkKind.SelectedIndex = 0;
        _bulkDecimals.ItemsSource = ExamReportService.NumberFormatOptions.Select(option => option.Label).ToList();
        _bulkDecimals.SelectedIndex = 0;

        _bulkFontName.ItemsSource = InstalledFontNames();
        _bulkFontName.SelectedItem = DefaultFontName;
        if (_bulkFontName.SelectedItem is null && _bulkFontName.Items.Count > 0)
        {
            _bulkFontName.SelectedIndex = 0;
        }

        AutomationProperties.SetName(_bulkFontName, "报表字体");

        Border bulk = ExamViewKit.Card(
            "批量参数",
            ExamViewKit.Hint("按行或列范围逐格写入数据类型、身份与小数位数。"),
            ExamViewKit.Field("范围", ExamViewKit.Inline(_bulkMode, _bulkLine, _bulkStart, _bulkEnd), stretchContent: false),
            ExamViewKit.Field("数据类型", _bulkKind),
            ExamViewKit.Field("小数位数", _bulkDecimals),
            ExamViewKit.Field("身份", ExamViewKit.Inline(_bulkRaidPick, _bulkRaid), stretchContent: false),
            ExamViewKit.Field("字体", ExamViewKit.Inline(_bulkFontName, _bulkFontSize, _bulkFontBold), stretchContent: false),
            ExamViewKit.Field("项目/选项", ExamViewKit.Inline(_bulkItems, _bulkSelectors), stretchContent: false),
            _bulkInsertSame,
            ExamViewKit.ActionBar(
                ExamViewKit.Secondary("批量设置数据类型与身份", ApplyBulk),
                ExamViewKit.Secondary("复制身份列表", CopyRaids)));

        Button sortButton = ExamViewKit.Secondary("报表项目排序", SortItems);
        sortButton.ToolTip = "按自然顺序排列（数字按数值比，项目2 在 项目10 之前）。只影响本页显示与复制出的文本，不写库。";
        Button resetButton = ExamViewKit.Secondary("重置项目排序", ResetItemOrder);
        resetButton.ToolTip = "还原为库里的物理行序。排序不落库：切页或重开考评库后会回到物理行序，不要把它当成持久设置。";

        Border itemCard = ExamViewKit.Card(
            "报表项目",
            _bulkInfo,
            ExamViewKit.ListFrame(_itemList, 120),
            ExamViewKit.ActionBar(
                sortButton,
                resetButton,
                ExamViewKit.Secondary("复制项目列表", CopyItems),
                BuildGetMenuButton()));

        var bottom = new StackPanel();
        bottom.Children.Add(_status);
        bottom.Children.Add(actions);

        SetBody(BuildBottomActionLayout(
            ExamViewKit.FormArea(source, list, structure, replace, bulk, itemCard),
            null,
            bottom));

        _database.TextChanged += (_, _) => SyncReady();
        _changeItem.Checked += (_, _) => SyncReady();
        _changeItem.Unchecked += (_, _) => SyncReady();
        _changeRaid.Checked += (_, _) => SyncReady();
        _changeRaid.Unchecked += (_, _) => SyncReady();
        _changeFormat.Checked += (_, _) => SyncReady();
        _changeFormat.Unchecked += (_, _) => SyncReady();
        _newItemName.TextChanged += (_, _) => SyncReady();
        _newRaid.TextChanged += (_, _) => SyncReady();
        _newDataFormat.TextChanged += (_, _) => SyncReady();
        ExamViewKit.ShowPlaceholder(_listFrame, null, ReportEmptyHint);
        SyncReady();

        if (PathGuard.MissingFileReason(ExamViewKit.Value(_database), "考评库") is null)
        {
            RefreshReports();
        }
    }

    /// <summary>没库、没选报表、没勾动作、缺新值时禁用「执行替换」，并把下一步写在状态行。</summary>
    private void SyncReady()
    {
        if (_apply is null)
        {
            return;
        }

        List<string> missing = new();
        if (PathGuard.MissingFileReason(ExamViewKit.Value(_database), "考评库") is { } databaseProblem)
        {
            missing.Add(databaseProblem);
        }

        if (_list.SelectedItem is not ExamReportRow)
        {
            missing.Add("选中的报表");
        }

        bool changeItem = _changeItem.IsChecked == true;
        bool changeRaid = _changeRaid.IsChecked == true;
        bool changeFormat = _changeFormat.IsChecked == true;
        if (!changeItem && !changeRaid && !changeFormat)
        {
            missing.Add("至少一个替换动作");
        }

        if (changeItem && ExamViewKit.Value(_newItemName).Length == 0)
        {
            missing.Add("新项目名");
        }

        // 「修改为」为空 = 删除该评测主体（legacy label173 注释）；两个都空时 Core 与 dev 一样不产生任何修改。
        // 所以这里不再把「新 Raid 为空」当缺参拦截。
        if (changeFormat && ExamViewKit.Value(_newDataFormat).Length == 0)
        {
            missing.Add("新数据格式");
        }

        _apply.IsEnabled = missing.Count == 0;
        if (missing.Count > 0)
        {
            ExamViewKit.ShowStatus(_status, "还缺：" + string.Join("、", missing) + "。");
        }
    }

    private void BuildColumns()
    {
        var view = new GridView();
        view.Columns.Add(Column("报表 ID", 80, nameof(ExamReportRow.ReportId)));
        view.Columns.Add(Column("报表名", 300, nameof(ExamReportRow.ReportName)));
        view.Columns.Add(Column("考评表", 240, nameof(ExamReportRow.ExamName)));
        _list.View = view;
    }

    private static GridViewColumn Column(string header, double width, string path)
        => new()
        {
            Header = header,
            Width = width,
            DisplayMemberBinding = new Binding(path)
        };

    private void RefreshReports()
    {
        string database = ExamViewKit.Value(_database);
        _list.ItemsSource = null;
        ExamViewKit.ShowPlaceholder(_listFrame, null, ReportEmptyHint);
        if (PathGuard.MissingFileReason(database, "考评库") is { } problem)
        {
            ExamViewKit.ShowStatus(_status, problem + "，请重新选择。", error: true);
            return;
        }

        try
        {
            IReadOnlyList<ExamReportRow> rows = _reports.ListReports(database);
            _list.ItemsSource = rows;
            ExamViewKit.ShowPlaceholder(_listFrame, _listHost, "Report_Ess 中暂无报表。");
            string summary = rows.Count == 0
                ? "Report_Ess 中暂无报表。"
                : $"共 {rows.Count} 张报表，选中一行后执行替换。";
            SyncReady();
            ExamViewKit.ShowStatus(_status, summary);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    /// <summary>读取选中报表的网格尺寸与测评身份（legacy GetExamReportColumnsAndRows / GetExamRaidInfo）。</summary>
    private void LoadReportContext(ExamReportRow row)
    {
        string database = ExamViewKit.Value(_database);
        if (PathGuard.MissingFileReason(database, "考评库") is { } problem)
        {
            _gridInfo.Text = problem;
            return;
        }

        try
        {
            ExamReportGrid grid = _reports.GetGridSize(database, row.ReportId);
            IReadOnlyList<ExamRaidOption> raids = _reports.ListExamRaids(database, row.ExamId);

            _gridInfo.Text = $"列 {grid.Columns} × 行 {grid.Rows}" +
                             $"（列索引 {Describe(grid.ColumnIndexes)}，行索引 {Describe(grid.RowIndexes)}）";

            _raidOptions.ItemsSource = raids.Select(r => new RaidChoice(r.Display, r.Value)).ToList();
            _raidOptions.IsEnabled = raids.Count > 0;

            // 批量参数的身份下拉用 Core 原始项（含「批量设置全部身份」），SelectedValue 即要写库的 Raid 串。
            _bulkRaidPick.ItemsSource = raids;
            _bulkRaidPick.DisplayMemberPath = nameof(ExamRaidOption.Display);
            _bulkRaidPick.SelectedItem = null;

            // 考评表项目（联动源）：与报表正文无关，读的是 Exam_Score 的项目行。
            _examItems.ItemsSource = _reports.ListExamTableItems(database, row.ExamId);
            _examItems.SelectedItem = null;
            _examSelectors.ItemsSource = null;

            if (raids.Count == 0)
            {
                _raidOptions.ItemsSource = new[] { new RaidChoice("（该考评表没有测评身份）", string.Empty) };
            }
        }
        catch (Exception ex)
        {
            _gridInfo.Text = "读取失败：" + ex.Message;
        }
    }

    private static string Describe(IReadOnlyList<int> values)
        => values.Count == 0 ? "无" : string.Join(",", values);

    private void Save()
    {
        string database = ExamViewKit.Value(_database);
        if (PathGuard.MissingFileReason(database, "考评数据库") is { } databaseProblem)
        {
            _dialogs.Info(databaseProblem + "，请先选择。");
            return;
        }

        if (_list.SelectedItem is not ExamReportRow row)
        {
            _dialogs.Info("请先在列表中选择一张报表。");
            return;
        }

        bool changeItem = _changeItem.IsChecked == true;
        bool changeRaid = _changeRaid.IsChecked == true;
        bool changeFormat = _changeFormat.IsChecked == true;
        if (!changeItem && !changeRaid && !changeFormat)
        {
            _dialogs.Info("请至少勾选一项替换动作。");
            return;
        }

        string itemName = _itemName.Text?.Trim() ?? string.Empty;
        if (changeItem && ExamViewKit.Value(_newItemName).Length == 0)
        {
            _dialogs.Info("请填写新的项目名称。");
            return;
        }

        // 「修改为」为空 = 删除该评测主体（legacy label173）；与「原 Raid」都为空时 Core 不产生修改，
        // 这里不再当缺参拦截，让删除语义可用。
        if (changeFormat && ExamViewKit.Value(_newDataFormat).Length == 0)
        {
            _dialogs.Info("请填写新的数据格式。");
            return;
        }

        try
        {
            ReportContentReplaceResult result = _reports.ReplaceContent(database, new ReportContentReplaceRequest
            {
                ReportId = row.ReportId,
                ItemName = itemName.Length == 0 ? ExamReportService.AllItemsName : itemName,
                ChangeItemName = changeItem,
                ChangeRaid = changeRaid,
                ChangeDataFormat = changeFormat,
                NewItemName = ExamViewKit.Value(_newItemName),
                NewRaid = ExamViewKit.Value(_newRaid),
                NewDataFormat = ExamViewKit.Value(_newDataFormat),
                OldRaid = ExamViewKit.Value(_oldRaid)
            });

            ExamViewKit.ShowStatus(_status, $"扫描 {result.ScannedRows} 行，修改 {result.ChangedRows} 行。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>读报表项目清单，同时把库里的物理顺序记成备份（重置排序的来源）。</summary>
    private void LoadItems(ExamReportRow row)
    {
        string database = ExamViewKit.Value(_database);
        try
        {
            ExamReportItemOrder order = _reports.GetReportItemOrder(database, row.ReportId);
            _storedItems = order.StoredOrder;
            ShowItems(order.StoredOrder);
            _bulkInfo.Text = order.StoredOrder.Count == 0
                ? "这张报表在 Report_Detail 里还没有项目行。"
                : $"共 {order.StoredOrder.Count} 个项目；当前顺序是库里的物理顺序（重置排序的备份）。";
        }
        catch (Exception ex)
        {
            _storedItems = Array.Empty<string>();
            ShowItems(_storedItems);
            _bulkInfo.Text = "读取项目失败：" + ex.Message;
        }
    }

    private void ShowItems(IReadOnlyList<string> items)
    {
        _itemList.ItemsSource = items.Count == 0 ? null : items.ToList();

        // 项目筛选下拉与列表同源：选中报表后下拉里就是这张报表真实用到的项目名。
        string current = _itemName.Text?.Trim() ?? string.Empty;
        _itemName.ItemsSource = items.Count == 0 ? null : items.ToList();
        _itemName.Text = current.Length == 0 ? ExamReportService.AllItemsName : current;
    }

    /// <summary>批量设置数据类型与身份（legacy 按钮 ChangeExamReportParaButton_Click，BodyForm.cs:7053）。</summary>
    private void ApplyBulk()
    {
        if (!TryGetDatabase(out string database))
        {
            return;
        }

        if (_list.SelectedItem is not ExamReportRow row)
        {
            _dialogs.Info("请先在列表中选择一张报表。");
            return;
        }

        int line = ParseIndex(_bulkLine);
        int start = ParseIndex(_bulkStart);
        int end = ParseIndex(_bulkEnd);
        if (line <= 0 || start <= 0 || end < start)
        {
            _bulkInfo.Text = "行号/列号与起止序号都要大于 0，且结束不小于起始。";
            return;
        }

        if (!int.TryParse(ExamViewKit.Value(_bulkFontSize), out int fontSize) || fontSize is <= 0 or > 200)
        {
            _bulkInfo.Text = "字号必须是 1–200 之间的数字（例如 11）。";
            ExamViewKit.ShowStatus(_status, "字号无效：请填 1–200 之间的数字。", error: true);
            return;
        }

        var request = new ExamReportParaRequest
        {
            ReportId = row.ReportId,
            RowMode = ExamReportService.IsRowMode(_bulkMode.SelectedIndex),
            Line = line,
            StartIndex = start,
            EndIndex = end,
            DataFormatKind = _bulkKind.SelectedItem as string ?? ExamReportService.DefaultDataFormatKind,
            Raid = ExamViewKit.Value(_bulkRaid),
            NumberFormat = ExamReportService.NumberFormatAt(_bulkDecimals.SelectedIndex),
            FontSpec = BuildFontSpec(),
            ItemNames = ExamViewKit.Value(_bulkItems),
            ItemSelectors = ExamViewKit.Value(_bulkSelectors),
            InsertSameItemSelector = _bulkInsertSame.IsChecked == true
        };

        try
        {
            ExamReportParaResult result = _reports.ApplyDataFormat(database, request);
            string texts = result.DisplayTexts.Count == 0 ? "无" : string.Join("/", result.DisplayTexts);
            _bulkInfo.Text = $"最近一次写入：{result.Cells} 格、格式行 {result.FormatRows}、数据行 {result.DataRows}，小数位显示文本 {texts}。";
            ExamViewKit.ShowStatus(
                _status,
                $"已写入 {result.Cells} 个单元格（格式行 {result.FormatRows}、数据行 {result.DataRows}）。",
                StatusTone.Success);
        }
        catch (Exception ex)
        {
            _bulkInfo.Text = "写入失败：" + ex.Message;
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    /// <summary>「报表项目排序」：自然排序（legacy StrCmpLogicalW），只改显示顺序。</summary>
    private void SortItems()
    {
        if (!TryGetDatabase(out string database) || _list.SelectedItem is not ExamReportRow row)
        {
            return;
        }

        try
        {
            ExamReportItemOrder order = _reports.GetReportItemOrder(database, row.ReportId);
            if (_storedItems.Count == 0)
            {
                _storedItems = order.StoredOrder;
            }

            ShowItems(order.SortedOrder);
            _bulkInfo.Text = $"已按自然顺序排列 {order.SortedOrder.Count} 个项目；原顺序已备份，可「重置项目排序」还原。排序只在本页生效、不写库。";
            ExamViewKit.ShowStatus(_status, $"已排序 {order.SortedOrder.Count} 个项目（只改本页显示，不写库；切页或重开考评库后回到物理行序）。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    /// <summary>「重置项目排序」：用读库时备份的物理顺序还原（legacy TempExamReportItemLists_NoSort）。</summary>
    private void ResetItemOrder()
    {
        if (_storedItems.Count == 0)
        {
            _bulkInfo.Text = "还没有可还原的顺序：先选中报表读取项目。";
            return;
        }

        ShowItems(_storedItems);
        _bulkInfo.Text = $"已还原为库里的物理顺序（{_storedItems.Count} 个项目）；排序本来就不写库，切页或重开考评库后也是这个顺序。";
        ExamViewKit.ShowStatus(_status, "已还原项目顺序。（排序不落库：切页或重开考评库后同样回到物理行序）", StatusTone.Success);
    }

    /// <summary>「复制项目列表」：Core 只给结构化文本，剪贴板写在这里（legacy BodyForm.cs:7923-7943）。</summary>
    private void CopyItems()
    {
        IReadOnlyList<string> items = _itemList.ItemsSource is IEnumerable<string> current
            ? (IReadOnlyList<string>)current.ToList()
            : _storedItems;
        string text = _reports.BuildClipboardText(items);
        if (text.Length == 0)
        {
            ExamViewKit.ShowStatus(_status, "还没有可复制的项目：先选中报表。", error: true);
            return;
        }

        Clipboard.SetText(text);
        ExamViewKit.ShowStatus(_status, $"已复制 {items.Count} 个项目（| 分隔）到剪贴板。", StatusTone.Success);
    }

    /// <summary>已安装字体名（GDI+ 口径，与 legacy 字体对话框给的名字同源）。枚举失败时退回默认字体。</summary>
    private static IReadOnlyList<string> InstalledFontNames()
    {
        try
        {
            using var fonts = new System.Drawing.Text.InstalledFontCollection();
            return fonts.Families
                .Select(family => family.Name)
                .Where(name => name.Length > 0)
                .Distinct(StringComparer.CurrentCulture)
                .OrderBy(name => name, StringComparer.CurrentCulture)
                .ToList();
        }
        catch (Exception)
        {
            // 字体服务异常不该让整页不可用：至少保留默认字体可选。
            return new[] { DefaultFontName };
        }
    }

    /// <summary>拼回 Core 认识的三段串「字体,大小,加粗」（<see cref="ExamReportService.DefaultFontSpec"/>）。</summary>
    private string BuildFontSpec()
    {
        string name = _bulkFontName.SelectedItem as string ?? DefaultFontName;
        string size = ExamViewKit.Value(_bulkFontSize);
        if (size.Length == 0)
        {
            size = DefaultFontSize;
        }

        return $"{name},{size},{(_bulkFontBold.IsChecked == true ? 1 : 0)}";
    }

    /// <summary>选中考评表项目 → 选项联动（legacy ExamItemsCombox_SelectedIndexChanged）。</summary>
    private void LoadExamSelectors()
    {
        _examSelectors.ItemsSource = null;
        if (_examItems.SelectedItem is not string item || item.Length == 0)
        {
            return;
        }

        if (_list.SelectedItem is not ExamReportRow row || !TryGetDatabase(out string database))
        {
            return;
        }

        try
        {
            IReadOnlyList<string> selectors = _reports.ListItemSelectors(database, row.ExamId, item);
            _examSelectors.ItemsSource = selectors;
            if (selectors.Count == 0)
            {
                ExamViewKit.ShowStatus(_status, $"项目「{item}」没有可选选项。");
            }
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, "读取选项失败：" + ex.Message, error: true);
        }
    }

    /// <summary>报表项目列表右键菜单：legacy <c>ExamReportItemSortMenu</c>（排序 / 重置）。</summary>
    private ContextMenu BuildItemSortMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(MenuAction("报表项目排序", SortItems));
        menu.Items.Add(MenuAction("重置项目排序", ResetItemOrder));
        return menu;
    }

    private static MenuItem MenuAction(string header, Action action)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => action();
        return item;
    }

    /// <summary>
    /// 「获取…」：legacy <c>GetAllItemSplit</c> 的下拉（获取全部项目名称（考评表）/ 获取所选项目的选项）。
    /// 报表项目那一项已由「复制项目列表」承担，不重复放。
    /// </summary>
    private Button BuildGetMenuButton()
    {
        var menu = new ContextMenu();
        menu.Items.Add(MenuAction("复制考评表项目名称", () =>
            CopyNames("考评表项目", _examItems.Items.Cast<object>())));
        menu.Items.Add(MenuAction("复制所选项目的选项", () =>
            CopyNames("项目选项", _examSelectors.Items.Cast<object>())));

        Button button = ExamViewKit.Secondary("获取…", () => { });
        button.ToolTip = "从考评表定义里取项目名 / 所选项目的选项，复制到剪贴板（| 分隔）。";
        button.Click += (_, _) =>
        {
            menu.PlacementTarget = button;
            menu.Placement = PlacementMode.Bottom;
            menu.IsOpen = true;
        };
        return button;
    }

    private void CopyNames(string what, IEnumerable<object> items)
    {
        var names = items
            .Select(item => item?.ToString() ?? string.Empty)
            .Where(name => name.Length > 0)
            .ToList();

        if (names.Count == 0)
        {
            ExamViewKit.ShowStatus(_status, $"还没有可复制的{what}：先选中报表并读取结构。", error: true);
            return;
        }

        Clipboard.SetText(string.Join("|", names));
        ExamViewKit.ShowStatus(_status, $"已复制 {names.Count} 个{what}（| 分隔）。", StatusTone.Success);
    }

    /// <summary>「复制身份列表」：legacy <c>ExamRaidsCopy_Click</c>（dev 里因 DataSource 强转失败而空转）。</summary>
    private void CopyRaids()
    {
        if (!TryGetDatabase(out string database))
        {
            ExamViewKit.ShowStatus(_status, _bulkInfo.Text, error: true);
            return;
        }

        if (_list.SelectedItem is not ExamReportRow row)
        {
            ExamViewKit.ShowStatus(_status, "请先选中一个报表，再点「复制身份列表」。", error: true);
            return;
        }

        try
        {
            IReadOnlyList<ExamRaidOption> raids = _reports.ListExamRaids(database, row.ExamId);
            string text = _reports.BuildRaidClipboardText(raids);
            if (text.Length == 0)
            {
                ExamViewKit.ShowStatus(_status, "该考评表没有测评身份，没有可复制的内容。", error: true);
                return;
            }

            Clipboard.SetText(text);
            ExamViewKit.ShowStatus(_status, $"已复制评测主体合计身份：{text}", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, "复制失败：" + ex.Message, error: true);
        }
    }

    private bool TryGetDatabase(out string database)
    {
        database = ExamViewKit.Value(_database);
        if (PathGuard.MissingFileReason(database, "考评数据库") is { } problem)
        {
            _bulkInfo.Text = problem + "。";
            return false;
        }

        return true;
    }

    private static int ParseIndex(TextBox box)
        => int.TryParse(ExamViewKit.Value(box), out int value) ? value : 0;

    private sealed class RaidChoice
    {
        public RaidChoice(string display, string value)
        {
            Display = display;
            Value = value;
        }

        public string Display { get; }

        public string Value { get; }
    }
}
