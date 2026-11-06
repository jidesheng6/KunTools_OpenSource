using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
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
/// exam.insert — insert exam items / selectors into Exam_EssInfo + Exam_Score
/// (port of the legacy 「插入考评信息」 tab, including Exam_Ruler defaults).
/// </summary>
public sealed class ExamInsertView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly IExamInsertService _insert;
    private readonly IExamIndexService _index;
    private readonly TextBox _database = ExamViewKit.PathBox("考评库", "考评 Access 数据库");
    private readonly ComboBox _examList = new() { MinWidth = 340 };
    private readonly TextBox _examName = new() { PlaceholderText = "考评表名称（新名字会新建考评表）" };
    private readonly ComboBox _typeMode = new() { MinWidth = 140 };
    private readonly TextBox _bulkItemTemplate = new() { Text = "第X项", Width = 150 };
    private readonly TextBox _bulkItemCount = new() { Text = "5", Width = 70 };
    private readonly TextBox _bulkItemScore = new() { Text = "100", Width = 70 };
    private readonly CheckBox _bulkItemChinese = new()
    {
        Content = "中文序号（一、二、三…）",
        IsChecked = true,
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly ComboBox _preset = new() { MinWidth = 230 };
    private readonly TextBox _presetSuffix = new() { Width = 120, PlaceholderText = "后缀（可选）" };
    private readonly TextBox _selectorScorePer = new() { Text = "0", Width = 70 };
    private readonly CheckBox _selectorCalcAvg = new()
    {
        Content = "计入得票率",
        IsChecked = true,
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly TextBox _indexFrom = new() { Width = 70, PlaceholderText = "原序号" };
    private readonly TextBox _indexTo = new() { Width = 70, PlaceholderText = "新序号" };

    // 三种得分比重（legacy InsertExam_ExamScore / InsertExam_SelectorAbstain_ScorePer /
    // InsertExam_SelectorInvalid_ScorePer，BodyForm.Designer.cs:12357/12391/12558）。
    // 默认值也照 legacy：得分 0、弃权 -1（计入得票率）、无效 -1（不计入）。
    private readonly TextBox _itemScoreWeight = new() { Text = ExamInsertRules.DefaultItemScoreWeight, Width = 70 };
    private readonly TextBox _abstainScoreWeight = new() { Text = ExamInsertRules.DefaultAbstainScoreWeight, Width = 70 };
    private readonly TextBox _invalidScoreWeight = new() { Text = ExamInsertRules.DefaultInvalidScoreWeight, Width = 70 };
    private readonly CheckBox _abstainCalcAvg = new()
    {
        Content = "计入得票率",
        IsChecked = true,
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly CheckBox _invalidCalcAvg = new()
    {
        Content = "计入得票率",
        IsChecked = false,
        VerticalAlignment = VerticalAlignment.Center
    };
    private readonly DataGrid _items = new()
    {
        AutoGenerateColumns = false,
        CanUserAddRows = false,
        CanUserDeleteRows = false,
        HeadersVisibility = DataGridHeadersVisibility.Column,
        RowHeight = 30,
        MinHeight = 240
    };
    private readonly ObservableCollection<InsertItemRow> _rows = new();
    private readonly TextBlock _status = ExamViewKit.Status("填写项目与分值后点击「插入信息」。");
    private readonly Button _insertButton;

    public ExamInsertView(FeatureDescriptor descriptor, IServiceProvider services)
        : base(descriptor.Title, "把项目 / 选项写入 Exam_EssInfo、Exam_Score 与 Exam_BoolItem（对齐原「插入考评信息」）。")
    {
        _dialogs = services.GetRequiredService<IDialogService>();
        _insert = services.GetRequiredService<IExamInsertService>();
        _index = services.GetRequiredService<IExamIndexService>();
        IAppSettings settings = services.GetRequiredService<IAppSettings>();

        _database.Text = settings.Current.VfsoftExamDataPath;
        _typeMode.ItemsSource = ExamInsertRules.TypeModes;
        _typeMode.SelectedIndex = 0;
        _preset.ItemsSource = ExamBatchInsertTemplates.CustomSelectorPresets
            .Select((names, i) => new PresetChoice(i + 1, string.Join("/", names)))
            .ToList();
        _preset.DisplayMemberPath = nameof(PresetChoice.Display);
        _preset.SelectedIndex = 5; // 「优秀/合格/基本合格/不合格」是 legacy 最常用的一组。
        _examList.SelectionChanged += (_, _) =>
        {
            if (_examList.SelectedItem is ExamChoice choice)
            {
                _examName.Text = choice.Name;
                int index = ExamInsertRules.TypeModeIndex(choice.TypeMode);
                if (index >= 0)
                {
                    _typeMode.SelectedIndex = index;
                }
            }
        };

        BuildColumns();
        _items.ItemsSource = _rows;
        _rows.Add(InsertItemRow.CreateSample());

        Border source = ExamViewKit.Card(
            "数据源",
            ExamViewKit.Field("考评库", _database, ExamViewKit.Secondary("选择…", () =>
            {
                ExamViewKit.PickFile(_dialogs, _database, ExamViewKit.AccessDatabaseFilter);
                RefreshExams();
            })));

        Border exam = ExamViewKit.Card(
            "考评表",
            ExamViewKit.Field("已有考评表", _examList, ExamViewKit.Secondary("刷新", RefreshExams)),
            ExamViewKit.Field("名称", _examName),
            ExamViewKit.Field("排版类型", _typeMode));

        // 三种得分比重：legacy 的「得分比重 / 弃权得分比重 / 无效得分比重」三个 NumericUpDown
        // （BodyForm.Designer.cs:12457/12482/12491）。它们只影响 Exam_Score 的 Score 列：
        // 项目行取「得分」，每个项目自动追加的「无效」「弃权」行取各自比重。
        Border weights = ExamViewKit.Card(
            "得分比重",
            ExamViewKit.Field("得分", _itemScoreWeight),
            ExamViewKit.Field("弃权得分", ExamViewKit.Inline(_abstainScoreWeight, _abstainCalcAvg), stretchContent: false),
            ExamViewKit.Field("无效得分", ExamViewKit.Inline(_invalidScoreWeight, _invalidCalcAvg), stretchContent: false),
            ExamViewKit.Hint("得分是项目分值的默认值（项目行自己填了分值则以它为准）；弃权 / 无效会为每个项目各写一行 Exam_Score，默认 -1，弃权计入得票率、无效不计入。"));

        Border items = ExamViewKit.Card(
            "项目与选项",
            ExamViewKit.Hint("选项用逗号分隔；「无效 / 弃权」为系统隐式选项，无需填写。勾选「计入得票率」按选项统计平均分。"),
            ExamViewKit.ListFrame(_items, 240),
            ExamViewKit.ActionBar(
                ExamViewKit.Secondary("添加项目", () => _rows.Add(new InsertItemRow())),
                ExamViewKit.Secondary("删除选中", RemoveSelected),
                // legacy InsetExam_BacthMenu 的「移除全部项目」（Designer:17921）。
                ExamViewKit.Secondary("清空项目", ClearItems)));

        // 批量模板：legacy 的「批量添加项目 / 批量添加选项」在界面层展开成 N 个控件，
        // 这里是同一件事的纯逻辑版本（ExamBatchInsertTemplates），展开后仍走同一条插入管线。
        // 卡片放在项目表之后：这两张卡产出的是表里的行，紧贴表下方才能立刻看到结果。
        Border bulkItem = ExamViewKit.Card(
            "批量项目",
            ExamViewKit.Field(
                "名称模板",
                ExamViewKit.Inline(_bulkItemTemplate, _bulkItemCount, _bulkItemScore, _bulkItemChinese),
                ExamViewKit.Secondary("展开为项目", ExpandItemRows),
                stretchContent: false),
            ExamViewKit.Hint("模板里的 X 会替换成序号（如「第X项」→ 第1项、第2项…）；整批共用同一个分值。"));

        Border bulkSelector = ExamViewKit.Card(
            "批量选项",
            ExamViewKit.Field(
                "内置名称",
                ExamViewKit.Inline(_preset, _presetSuffix, _selectorScorePer, _selectorCalcAvg),
                ExamViewKit.Secondary("套用到选中项目", ApplySelectorPreset),
                stretchContent: false),
            ExamViewKit.ActionBar(
                ExamViewKit.Secondary("从剪贴板导入选项", ImportSelectorsFromClipboard)));

        Border renumber = ExamViewKit.Card(
            "重排表号",
            ExamViewKit.Field(
                "序号",
                ExamViewKit.Inline(_indexFrom, new TextBlock
                {
                    Text = "→",
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(6, 0, 6, 0)
                }, _indexTo),
                ExamViewKit.ActionBar(
                    ExamViewKit.Secondary("重排表号", ChangeExamIndex),
                    ExamViewKit.Secondary("重建物理顺序", RebuildPhysicalOrder)),
                stretchContent: false),
            ExamViewKit.Hint(
                $"改号会同步改写 {ExamIndexService.ExamIdBoundTables.Length} 张按 ExamID 关联的表；" +
                "目标序号已占用时两张表互换序号（无法撤销）。" +
                "「重建物理顺序」把 Exam_EssInfo 按序号重写一遍，用于修复序号错乱的旧库。"));

        _insertButton = ExamViewKit.Primary("插入信息", DoInsert);
        var actions = ExamViewKit.ActionBar(
            _insertButton,
            ExamViewKit.Secondary("重置为示例", () =>
            {
                _rows.Clear();
                _rows.Add(InsertItemRow.CreateSample());
            }));

        var bottom = new StackPanel();
        bottom.Children.Add(_status);
        bottom.Children.Add(actions);

        SetBody(BuildBottomActionLayout(
            ExamViewKit.FormArea(source, exam, weights, items, bulkItem, bulkSelector, renumber),
            null,
            bottom));

        _database.TextChanged += (_, _) => SyncReady();
        _examName.TextChanged += (_, _) => SyncReady();
        _rows.CollectionChanged += (_, _) => SyncReady();
        _items.CellEditEnding += (_, _) => Dispatcher.BeginInvoke(new Action(SyncReady));
        SyncReady();

        if (PathGuard.MissingFileReason(ExamViewKit.Value(_database), "考评库") is null)
        {
            RefreshExams();
        }
    }

    /// <summary>库/表名/至少一行带名称的项目齐了才允许插入，缺口写进状态行而不是点了再弹窗。</summary>
    private void SyncReady()
    {
        if (_insertButton is null)
        {
            return;
        }

        List<string> missing = new();
        if (PathGuard.MissingFileReason(ExamViewKit.Value(_database), "考评库") is { } databaseProblem)
        {
            missing.Add(databaseProblem);
        }

        if (ExamViewKit.Value(_examName).Length == 0)
        {
            missing.Add("考评表名称");
        }

        int named = _rows.Count(r => !string.IsNullOrWhiteSpace(r.ItemName));
        if (named == 0)
        {
            missing.Add("至少一行项目名称");
        }

        _insertButton.IsEnabled = missing.Count == 0;
        if (missing.Count > 0)
        {
            ExamViewKit.ShowStatus(_status, "还缺：" + string.Join("、", missing) + "。");
        }
        else
        {
            ExamViewKit.ShowStatus(_status, $"就绪：{named} 个项目待写入，点击「插入信息」。");
        }
    }

    /// <summary>给单元格挂 ToolTip：DataGrid 列本身没有 ToolTip，只能落到生成的 TextBlock 上。</summary>
    private static Style ToolTipStyle(string text)
    {
        var style = new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, text));
        return style;
    }

    private void BuildColumns()
    {
        _items.Columns.Add(new DataGridTextColumn
        {
            Header = "项目名称",
            Width = new DataGridLength(1, DataGridLengthUnitType.Star),
            Binding = new Binding(nameof(InsertItemRow.ItemName))
        });
        _items.Columns.Add(new DataGridTextColumn
        {
            Header = "分值",
            Width = new DataGridLength(90),
            Binding = new Binding(nameof(InsertItemRow.ItemScore)),
            // 与 Core 的 XML 注释同一句：空 = 沿用「得分比重」，显式 0 才是 0 分。
            // ElementStyle 才作用于单元格里的 TextBlock；CellStyle 的 TargetType 是 DataGridCell，套上去会类型不匹配。
            ElementStyle = ToolTipStyle("空 = 沿用上方「得分比重」；填 0 表示该项目得分为 0。")
        });
        // 选项分值必须落在表格里才能随行落库：Core 会把它写进 Exam_Score.Score
        // （ExamInsertService.cs 的 InsertSelectorScore），藏在面板里就会丢。
        _items.Columns.Add(new DataGridTextColumn
        {
            Header = "选项分值",
            Width = new DataGridLength(90),
            Binding = new Binding(nameof(InsertItemRow.SelectorScorePer))
        });
        _items.Columns.Add(new DataGridTextColumn
        {
            Header = "选项（逗号分隔）",
            Width = new DataGridLength(2, DataGridLengthUnitType.Star),
            Binding = new Binding(nameof(InsertItemRow.SelectorText))
        });
        _items.Columns.Add(new DataGridCheckBoxColumn
        {
            Header = "计入得票率",
            Width = new DataGridLength(110),
            Binding = new Binding(nameof(InsertItemRow.CalcAverage))
        });
    }

    private void RefreshExams()
    {
        string database = ExamViewKit.Value(_database);
        _examList.ItemsSource = null;
        if (PathGuard.MissingFileReason(database, "考评库") is { } problem)
        {
            ExamViewKit.ShowStatus(_status, problem + "，请重新选择。", error: true);
            return;
        }

        try
        {
            var rows = _insert.ListExams(database);
            _examList.DisplayMemberPath = nameof(ExamChoice.Display);
            _examList.ItemsSource = rows.Select(r => new ExamChoice(r.Id, r.Name, r.TypeMode)).ToList();
            ExamViewKit.ShowStatus(_status, rows.Count == 0
                ? "该库还没有考评表，填写名称后插入会新建。"
                : $"共 {rows.Count} 张考评表。");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    private void RemoveSelected()
    {
        if (_items.SelectedItem is InsertItemRow row)
        {
            _rows.Remove(row);
        }
    }

    /// <summary>「清除项目」：清空表格行（legacy「移除全部项目」，Designer:17921）。</summary>
    private void ClearItems()
    {
        if (_rows.Count == 0)
        {
            return;
        }

        _rows.Clear();
        ExamViewKit.ShowStatus(_status, "已清空项目表（库里的数据不会被删除，只有点「插入信息」才会写库）。", StatusTone.Neutral);
    }

    /// <summary>按名称模板展开多条项目并追加到表格（legacy「批量添加项目」）。</summary>
    private void ExpandItemRows()
    {
        string template = ExamViewKit.Value(_bulkItemTemplate);
        string score = ExamViewKit.Value(_bulkItemScore) is { Length: > 0 } s ? s : "0";

        if (!int.TryParse(ExamViewKit.Value(_bulkItemCount), out int count) || count <= 0)
        {
            ExamViewKit.ShowStatus(_status, "批量数量要填大于 0 的整数。", error: true);
            return;
        }

        try
        {
            IReadOnlyList<ExamInsertItem> expanded = ExamBatchInsertTemplates.ExpandItems(
                template,
                count,
                score,
                _bulkItemChinese.IsChecked == true);

            // 沿用表格最后一行的选项/计入得票率，避免新增行丢掉用户已配好的选项。
            InsertItemRow? last = _rows.Count > 0 ? _rows[_rows.Count - 1] : null;
            foreach (ExamInsertItem item in expanded)
            {
                _rows.Add(new InsertItemRow
                {
                    ItemName = item.ItemName,
                    ItemScore = item.ItemScore,
                    SelectorText = last?.SelectorText ?? "同意,不同意,弃权",
                    SelectorScorePer = last?.SelectorScorePer ?? "0",
                    CalcAverage = last?.CalcAverage ?? true
                });
            }

            ExamViewKit.ShowStatus(_status, $"已按模板展开 {expanded.Count} 个项目，追加到表格末尾。");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    /// <summary>把内置的 11 组选项名套用到当前选中项目。</summary>
    private void ApplySelectorPreset()
    {
        if (_items.SelectedItem is not InsertItemRow row)
        {
            ExamViewKit.ShowStatus(_status, "请先在表格里选中一行项目，再套用选项。", error: true);
            return;
        }

        if (_preset.SelectedItem is not PresetChoice choice)
        {
            ExamViewKit.ShowStatus(_status, "请选择一组内置选项名。", error: true);
            return;
        }

        try
        {
            IReadOnlyList<ExamInsertSelector> selectors = ExamBatchInsertTemplates.FromPreset(
                choice.Number,
                ExamViewKit.Value(_selectorScorePer) is { Length: > 0 } per ? per : "0",
                _selectorCalcAvg.IsChecked == true,
                ExamViewKit.Value(_presetSuffix));

            row.SelectorText = string.Join(",", selectors.Select(x => x.SelectorName));
            row.SelectorScorePer = ExamViewKit.Value(_selectorScorePer) is { Length: > 0 } per0 ? per0 : "0";
            row.CalcAverage = _selectorCalcAvg.IsChecked == true;
            _items.Items.Refresh();
            ExamViewKit.ShowStatus(
                _status,
                $"已把「{choice.Display}」套用到项目「{row.ItemName}」，共 {selectors.Count} 个选项。");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    /// <summary>从剪贴板一行一个选项名导入（legacy「批量自定义选项名」的粘贴路径）。</summary>
    private void ImportSelectorsFromClipboard()
    {
        if (_items.SelectedItem is not InsertItemRow row)
        {
            ExamViewKit.ShowStatus(_status, "请先在表格里选中一行项目，再导入选项名。", error: true);
            return;
        }

        string text;
        try
        {
            text = Clipboard.GetText();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, "读取剪贴板失败：" + ex.Message, error: true);
            return;
        }

        IReadOnlyList<string> names = ExamBatchInsertTemplates.ParseClipboardNames(text);
        if (names.Count == 0)
        {
            ExamViewKit.ShowStatus(_status, "剪贴板里没有可用的选项名（一行一个）。", error: true);
            return;
        }

        IReadOnlyList<ExamInsertSelector> selectors = ExamBatchInsertTemplates.FromNames(
            names,
            ExamViewKit.Value(_selectorScorePer) is { Length: > 0 } per ? per : "0",
            _selectorCalcAvg.IsChecked == true,
            ExamViewKit.Value(_presetSuffix));

        row.SelectorText = string.Join(",", selectors.Select(x => x.SelectorName));
        row.SelectorScorePer = ExamViewKit.Value(_selectorScorePer) is { Length: > 0 } per1 ? per1 : "0";
        row.CalcAverage = _selectorCalcAvg.IsChecked == true;
        _items.Items.Refresh();
        ExamViewKit.ShowStatus(_status, $"已从剪贴板导入 {selectors.Count} 个选项到项目「{row.ItemName}」。");
    }

    /// <summary>重排表号（legacy「修改考评表序号」，目标序号被占用时两张表互换）。</summary>
    private void ChangeExamIndex()
    {
        string database = ExamViewKit.Value(_database);
        if (PathGuard.MissingFileReason(database, "考评数据库") is { } problem)
        {
            ExamViewKit.ShowStatus(_status, problem + "，请先选择。", error: true);
            return;
        }

        if (!int.TryParse(ExamViewKit.Value(_indexFrom), out int from) ||
            !int.TryParse(ExamViewKit.Value(_indexTo), out int to))
        {
            ExamViewKit.ShowStatus(_status, "原序号与新序号都要填整数。", error: true);
            return;
        }

        if (!_dialogs.Confirm(
                $"把第 {from} 张考评表改为第 {to} 张？\n会按 ExamID 同步改写 {ExamIndexService.ExamIdBoundTables.Length} 张关联表（库里缺的表自动跳过），且无法撤销。",
                "重排表号"))
        {
            return;
        }

        try
        {
            ExamIndexChangeResult result = _index.ChangeExamIndex(new ExamIndexChangeRequest
            {
                DatabaseFile = database,
                OriginIndex = from,
                NewIndex = to
            });

            ExamViewKit.ShowStatus(_status, result.Summary, error: !result.Success);
            if (!result.Success)
            {
                _dialogs.Error(result.Summary);
            }

            RefreshExams();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>重建 Exam_EssInfo 的物理顺序（旧库序号错乱时的修复手段）。</summary>
    private void RebuildPhysicalOrder()
    {
        string database = ExamViewKit.Value(_database);
        if (PathGuard.MissingFileReason(database, "考评数据库") is { } problem)
        {
            ExamViewKit.ShowStatus(_status, problem + "，请先选择。", error: true);
            return;
        }

        if (!_dialogs.Confirm("按序号重写 Exam_EssInfo 的物理顺序？改写过程在事务内，校验不一致会整表回滚。", "重建物理顺序"))
        {
            return;
        }

        try
        {
            ExamPhysicalOrderResult result = _index.RebuildExamEssInfoPhysicalOrder(database);
            ExamViewKit.ShowStatus(_status, result.Message, error: !result.Success);
            if (!result.Success)
            {
                _dialogs.Error(result.Message);
            }

            RefreshExams();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>比重输入框留空时回落到 legacy 默认值（得分 0、弃权/无效 -1）。</summary>
    private static string WeightOr(TextBox box, string fallback)
    {
        string value = ExamViewKit.Value(box);
        return value.Length == 0 ? fallback : value;
    }

    private void DoInsert()
    {
        string database = ExamViewKit.Value(_database);
        string examName = ExamViewKit.Value(_examName);

        if (PathGuard.MissingFileReason(database, "考评数据库") is { } problem)
        {
            ExamViewKit.ShowStatus(_status, problem + "，请先选择。", error: true);
            return;
        }

        if (examName.Length == 0)
        {
            _dialogs.Info("请填写考评表名称。");
            return;
        }

        // Commit the in-flight cell edit, otherwise the last typed row would be skipped.
        _items.CommitEdit(DataGridEditingUnit.Cell, true);
        _items.CommitEdit(DataGridEditingUnit.Row, true);

        List<ExamInsertItem> items = new();
        foreach (InsertItemRow row in _rows)
        {
            if (string.IsNullOrWhiteSpace(row.ItemName))
            {
                continue;
            }

            string selectorScore = string.IsNullOrWhiteSpace(row.SelectorScorePer) ? "0" : row.SelectorScorePer.Trim();
            items.Add(new ExamInsertItem
            {
                ItemName = row.ItemName.Trim(),
                // 空串要原样交给 Core：它表示「沿用上方得分比重」，在这里补 0 就把该语义吃掉了。
                ItemScore = row.ItemScore?.Trim() ?? string.Empty,
                Selectors = ExamInsertRules.ParseSelectorList(row.SelectorText)
                    .Select(name => new ExamInsertSelector
                    {
                        SelectorName = name,
                        ScorePer = selectorScore,
                        CalcAvg = row.CalcAverage
                    })
                    .ToList()
            });
        }

        if (items.Count == 0)
        {
            _dialogs.Info("请至少填写一个项目名称。");
            return;
        }

        try
        {
            ExamInsertResult result = _insert.Insert(new ExamInsertRequest
            {
                DatabaseFile = database,
                ExamName = examName,
                TypeMode = _typeMode.SelectedItem as string ?? ExamInsertRules.PeopleVerticalTypeMode,
                Items = items,
                ItemScoreWeight = WeightOr(_itemScoreWeight, ExamInsertRules.DefaultItemScoreWeight),
                AbstainScoreWeight = WeightOr(_abstainScoreWeight, ExamInsertRules.DefaultAbstainScoreWeight),
                AbstainCalcAvg = _abstainCalcAvg.IsChecked == true,
                InvalidScoreWeight = WeightOr(_invalidScoreWeight, ExamInsertRules.DefaultInvalidScoreWeight),
                InvalidCalcAvg = _invalidCalcAvg.IsChecked == true
            });

            if (!result.Success)
            {
                ExamViewKit.ShowStatus(_status, result.Summary, error: true);
                _dialogs.Error(result.Summary);
                return;
            }

            ExamViewKit.ShowStatus(
                _status,
                $"完成：考评表 {result.ExamId}（{(result.CreatedExam ? "新建" : "复用")}），" +
                $"项目 {result.ItemCount}，选项 {result.SelectorCount}，布尔项 {result.BoolItemCount}" +
                $"{(result.ImplicitSelectorCount == 0 ? string.Empty : $"，内置无效/弃权 {result.ImplicitSelectorCount} 行")}。",
                StatusTone.Success);
            RefreshExams();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    internal sealed class InsertItemRow
    {
        public string ItemName { get; set; } = string.Empty;

        public string ItemScore { get; set; } = "0";

        public string SelectorText { get; set; } = "同意,不同意,弃权";

        /// <summary>每个选项的分值，落库到 Exam_Score.Score。</summary>
        public string SelectorScorePer { get; set; } = "0";

        public bool CalcAverage { get; set; } = true;

        public static InsertItemRow CreateSample() => new()
        {
            ItemName = "德能勤绩",
            ItemScore = "100",
            SelectorText = "优秀,良好,合格,不合格",
            SelectorScorePer = "0",
            CalcAverage = true
        };
    }

    private sealed class PresetChoice
    {
        public PresetChoice(int number, string names)
        {
            Number = number;
            Names = names;
        }

        public int Number { get; }

        public string Names { get; }

        public string Display => $"{Number}. {Names}";
    }

    private sealed class ExamChoice
    {
        public ExamChoice(int id, string name, string typeMode)
        {
            Id = id;
            Name = name;
            TypeMode = typeMode;
        }

        public int Id { get; }

        public string Name { get; }

        public string TypeMode { get; }

        public string Display => $"{Id} — {Name}（{TypeMode}）";
    }
}
