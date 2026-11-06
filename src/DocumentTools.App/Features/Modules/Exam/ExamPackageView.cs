using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Exam;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// exam.package — merge / reset an exam data package (Exam_Base.mdb + *.exam + SelfReport).
/// Merge mirrors the legacy 合并数据包 flow: duplicate-unit preprocessing, Exam_People copy,
/// Unit_Ess append by AtOrder, optional *.exam copy.
/// </summary>
public sealed class ExamPackageView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly IExamPackageService _packages;
    private readonly TextBox _source = ExamViewKit.PathBox("待合并数据包目录", "待合并数据包目录", folder: true);
    private readonly TextBox _target = ExamViewKit.PathBox("目标数据包目录", "被合并（目标）数据包目录", folder: true);
    private readonly ListView _units = new();

    /// <summary>「移动到…」的目标序号（1 起；legacy ExamDataPakUnitOrder_QuickFindButton 的输入框）。</summary>
    private readonly TextBox _moveToOrder = new() { Width = 80, PlaceholderText = "序号" };

    /// <summary>「查找」关键字（legacy ExamDataPakUnitOrder_QuickFind）。</summary>
    private readonly TextBox _quickFind = new() { Width = 120, PlaceholderText = "单位名称" };

    /// <summary>上一次「查找」命中的行号：关键字不变时从下一行继续找（循环），关键字变了从头找。</summary>
    private int _quickFindIndex = -1;

    private string _quickFindKeyword = string.Empty;
    private readonly System.Collections.ObjectModel.ObservableCollection<UnitRow> _unitRows = new();
    private readonly ScrollViewer _unitHost = new()
    {
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
    };
    private readonly Border _unitFrame;
    private readonly System.Windows.Controls.CheckBox _copyExam = new() { Content = "同时复制 .exam 文件", IsChecked = true };
    private readonly System.Windows.Controls.CheckBox _dedupe = new() { Content = "合并前清理目标包中的重复单位/人员", IsChecked = true };
    private readonly TextBlock _status = ExamViewKit.Status("选择两个数据包目录后先「检查状态」。");
    private readonly Button _merge;

    private const string UnitEmptyHint = "还没有单位：选择目标数据包目录，然后点「刷新单位」读取顺序。";

    public ExamPackageView(FeatureDescriptor descriptor, IServiceProvider services)
        : base(descriptor.Title, "合并或重置考评数据包（Exam_Base.mdb / *.exam / SelfReport）。")
    {
        _dialogs = services.GetRequiredService<IDialogService>();
        _packages = services.GetRequiredService<IExamPackageService>();
        BuildColumns();
        _units.ItemsSource = _unitRows;
        _unitHost.Content = _units;
        _unitFrame = ExamViewKit.ListFrame(_unitHost, 200);

        Border dirs = ExamViewKit.Card(
            "数据包目录",
            ExamViewKit.Field("待合并（源）", _source, ExamViewKit.Secondary("选择…", () =>
                ExamViewKit.PickFolder(_dialogs, _source))),
            ExamViewKit.Field("被合并（目标）", _target, ExamViewKit.Secondary("选择…", () =>
                ExamViewKit.PickFolder(_dialogs, _target))));

        Border unitCard = ExamViewKit.Card(
            "目标包单位顺序",
            _unitFrame,
            ExamViewKit.Hint("「上移 / 下移 / 移动到指定序号」只调整当前列表；点「保存单位顺序」后才会写入数据包。"),
            ExamViewKit.ActionBar(
                ExamViewKit.Secondary("△ 上移", () => MoveUnit(-1)),
                ExamViewKit.Secondary("▽ 下移", () => MoveUnit(1)),
                // legacy ExamDataPakUnitOrder_QuickFindButton + MoveToOrder（Designer:16194/16211）。
                ExamViewKit.Inline(_quickFind, ExamViewKit.Secondary("查找", QuickFindUnit)),
                ExamViewKit.Inline(_moveToOrder, ExamViewKit.Secondary("移动到…", MoveUnitToOrder)),
                ExamViewKit.Secondary("保存单位顺序", SaveUnitOrder)));

        _merge = ExamViewKit.Primary("合并数据包", Merge);
        var actions = ExamViewKit.ActionBar(
            _merge,
            ExamViewKit.Secondary("重置目标数据包", ResetTarget),
            ExamViewKit.Secondary("检查状态", Inspect),
            ExamViewKit.Secondary("刷新单位", RefreshUnits));

        var options = ExamViewKit.Inline(_copyExam, _dedupe);

        var bottom = new StackPanel();
        bottom.Children.Add(_status);
        bottom.Children.Add(actions);

        SetBody(BuildBottomActionLayout(ExamViewKit.FormArea(dirs, unitCard), options, bottom));

        _source.TextChanged += (_, _) => SyncReady();
        _target.TextChanged += (_, _) => SyncReady();
        ExamViewKit.ShowPlaceholder(_unitFrame, null, UnitEmptyHint);
        SyncReady();

        if (_packages.IsPackageDirectory(ExamViewKit.Value(_target)))
        {
            RefreshUnits();
        }
    }

    /// <summary>目录没落到硬盘上就不能合并，先禁用主操作并把原因写在状态行。</summary>
    private void SyncReady()
    {
        string source = ExamViewKit.Value(_source);
        string target = ExamViewKit.Value(_target);
        List<string> missing = new();
        if (PathGuard.MissingFolderReason(source, "待合并数据包目录") is { } sourceProblem)
        {
            missing.Add(sourceProblem);
        }
        else if (!_packages.IsPackageDirectory(source))
        {
            missing.Add("待合并数据包：" + _packages.Describe(source));
        }

        if (PathGuard.MissingFolderReason(target, "被合并数据包目录") is { } targetProblem)
        {
            missing.Add(targetProblem);
        }
        else if (!_packages.IsPackageDirectory(target))
        {
            missing.Add("被合并数据包：" + _packages.Describe(target));
        }

        bool same = missing.Count == 0 && PathGuard.SamePath(source, target);
        _merge.IsEnabled = missing.Count == 0 && !same;

        if (same)
        {
            ExamViewKit.ShowStatus(_status, "源目录与目标目录相同，请选择不同的数据包。", error: true);
        }
        else if (missing.Count > 0)
        {
            ExamViewKit.ShowStatus(_status, "还缺：" + string.Join("、", missing) + "。");
        }
        else
        {
            ExamViewKit.ShowStatus(_status, "就绪：可先「检查状态」，再「合并数据包」。");
        }
    }

    private void BuildColumns()
    {
        var view = new GridView();
        view.Columns.Add(Column("顺序", 70, nameof(UnitRow.Order)));
        view.Columns.Add(Column("单位", 280, nameof(UnitRow.Unit)));
        view.Columns.Add(Column("编码", 140, nameof(UnitRow.Code)));
        view.Columns.Add(Column("类型", 120, nameof(UnitRow.Kind)));
        _units.View = view;
    }

    private static GridViewColumn Column(string header, double width, string path)
        => new()
        {
            Header = header,
            Width = width,
            DisplayMemberBinding = new Binding(path)
        };

    private void Inspect()
    {
        string source = ExamViewKit.Value(_source);
        string target = ExamViewKit.Value(_target);
        var parts = new List<string>();

        try
        {
            if (source.Length > 0)
            {
                parts.Add("源：" + _packages.Inspect(source).Summary);
            }

            if (target.Length > 0)
            {
                ExamPackageStatus status = _packages.Inspect(target);
                parts.Add("目标：" + status.Summary);
                if (status.IsAlreadyInitialized)
                {
                    parts.Add("目标包已是初始化状态。");
                }
            }

            if (parts.Count == 0)
            {
                _dialogs.Info("请先选择源数据包与目标数据包目录。");
                return;
            }

            ExamViewKit.ShowStatus(_status, string.Join("　|　", parts));
            RefreshUnits();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    private void RefreshUnits()
    {
        string target = ExamViewKit.Value(_target);
        _units.ItemsSource = null;
        ExamViewKit.ShowPlaceholder(_unitFrame, null, UnitEmptyHint);
        if (!_packages.IsPackageDirectory(target))
        {
            ExamViewKit.ShowStatus(_status, "目标数据包：" + _packages.Describe(target) + "。", error: true);
            return;
        }

        try
        {
            List<UnitRow> rows = _packages.ListUnits(target)
                .Select(u => new UnitRow(u.Order, u.Unit, u.Code, u.Kind))
                .ToList();
            _unitRows.Clear();
            foreach (UnitRow row in rows)
            {
                _unitRows.Add(row);
            }

            ExamViewKit.ShowPlaceholder(_unitFrame, _unitHost, "目标包里没有单位记录。");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    private void Merge()
    {
        string source = ExamViewKit.Value(_source);
        string target = ExamViewKit.Value(_target);

        if (source.Length == 0 || target.Length == 0)
        {
            _dialogs.Info("请先选择待合并数据包与被合并数据包目录。");
            return;
        }

        if (PathGuard.SamePath(source, target))
        {
            _dialogs.Info("源目录与目标目录相同，无法合并。");
            return;
        }

        if (!_packages.IsPackageDirectory(source))
        {
            _dialogs.Info("源数据包：" + _packages.Describe(source) + "。");
            return;
        }

        if (!_packages.IsPackageDirectory(target))
        {
            _dialogs.Info("目标数据包：" + _packages.Describe(target) + "。");
            return;
        }

        if (!_dialogs.Confirm("该操作无法自动还原，请确认已备份数据包。是否继续？"))
        {
            return;
        }

        bool duplicates;
        try
        {
            duplicates = _packages.HasDuplicateUnits(source, target);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
            return;
        }

        if (duplicates)
        {
            if (!_dialogs.Confirm(
                    "待合并数据包中的单位在目标数据包中已存在。\n建议先在考评软件中完成合并并删除重复单位。\n是否继续由本工具预处理？"))
            {
                return;
            }

            if (!_dedupe.IsChecked.GetValueOrDefault(true))
            {
                if (!_dialogs.Confirm("未勾选「清理重复单位」，重复单位将被保留两份。是否继续？"))
                {
                    return;
                }
            }
        }

        try
        {
            ExamPackageMergeResult result = _packages.Merge(new ExamPackageMergeRequest
            {
                SourceDirectory = source,
                TargetDirectory = target,
                CopyExamFiles = _copyExam.IsChecked.GetValueOrDefault(true),
                RemoveDuplicateUnitsInTarget = _dedupe.IsChecked.GetValueOrDefault(true)
            });

            ExamViewKit.ShowStatus(_status, "合并完成：" + result.Summary, StatusTone.Success);
            RefreshUnits();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>上移 / 下移一行（只动本页顺序，写库在 <see cref="SaveUnitOrder"/>）。</summary>
    private void MoveUnit(int delta)
    {
        if (_units.SelectedItem is not UnitRow row)
        {
            ExamViewKit.ShowStatus(_status, "先在列表里选中一个单位。", error: true);
            return;
        }

        int index = _unitRows.IndexOf(row);
        int target = index + delta;
        if (index < 0 || target < 0 || target >= _unitRows.Count)
        {
            return;
        }

        _unitRows.Move(index, target);
        Renumber();
        _units.SelectedIndex = target;
    }

    /// <summary>
    /// 「查找」：按单位名称 / 类别循环定位下一行（legacy <c>ExamDataPakUnitOrder_DoQuickFind</c>，
    /// BodyForm.cs:10392-10470）。关键字不变时从上次命中位置之后继续，到头再从头找一圈。
    /// 找不到只写状态行，不弹框（dev 这里是静默 return，保持一致）。
    /// </summary>
    private void QuickFindUnit()
    {
        string keyword = ExamViewKit.Value(_quickFind);
        if (keyword.Length == 0)
        {
            ExamViewKit.ShowStatus(_status, "先填写要查找的单位名称或类别。");
            return;
        }

        if (_unitRows.Count == 0)
        {
            ExamViewKit.ShowStatus(_status, "还没有单位：先选择目标数据包目录并点「刷新单位」。");
            return;
        }

        // 关键字变化 → 从头找（legacy _examDataPakQuickFindLastKeyword 口径）。
        if (!string.Equals(keyword, _quickFindKeyword, StringComparison.OrdinalIgnoreCase))
        {
            _quickFindKeyword = keyword;
            _quickFindIndex = -1;
        }

        int FindFrom(int start)
        {
            for (int i = start; i < _unitRows.Count; i++)
            {
                UnitRow row = _unitRows[i];
                if (row.Unit.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || row.Kind.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                    || row.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        int startIndex = _quickFindIndex + 1;
        if (startIndex >= _unitRows.Count)
        {
            startIndex = 0;
        }

        int target = FindFrom(startIndex);
        if (target < 0 && startIndex > 0)
        {
            target = FindFrom(0);
        }

        if (target < 0)
        {
            _quickFindIndex = -1;
            ExamViewKit.ShowStatus(_status, $"没有找到包含「{keyword}」的单位。");
            return;
        }

        _quickFindIndex = target;
        _units.SelectedIndex = target;
        _units.ScrollIntoView(_unitRows[target]);
        ExamViewKit.ShowStatus(_status, $"已定位到第 {target + 1} 行：{_unitRows[target].Unit}。");
    }

    /// <summary>
    /// 「移动到…」：把选中单位挪到指定序号（1 基，legacy <c>ExamDataPakUnitOrder_MoveToOrder</c>）。
    /// 越界夹到首/尾，与上移/下移一样只改本页顺序。
    /// </summary>
    private void MoveUnitToOrder()
    {
        if (_units.SelectedItem is not UnitRow row)
        {
            ExamViewKit.ShowStatus(_status, "先在列表里选中一个单位。", error: true);
            return;
        }

        if (!int.TryParse(ExamViewKit.Value(_moveToOrder), out int order) || order <= 0)
        {
            ExamViewKit.ShowStatus(_status, "「移动到…」要填 1 起的序号（例如 3 表示排到第 3 位）。", error: true);
            return;
        }

        int index = _unitRows.IndexOf(row);
        int target = Math.Clamp(order - 1, 0, _unitRows.Count - 1);
        if (index < 0 || target == index)
        {
            return;
        }

        _unitRows.Move(index, target);
        Renumber();
        _units.SelectedIndex = target;
        ExamViewKit.ShowStatus(
            _status,
            $"已把「{row.Unit}」移到第 {target + 1} 位（点「保存单位顺序」才写库）。");
    }

    /// <summary>把当前显示顺序写回 Unit_Ess.AtOrder（legacy BodyForm.cs:10526-10657）。</summary>
    private void SaveUnitOrder()
    {
        string target = ExamViewKit.Value(_target);
        if (_unitRows.Count == 0)
        {
            _dialogs.Info("目标包里没有单位记录。");
            return;
        }

        if (!_dialogs.Confirm("按当前列表顺序重写 Unit_Ess.AtOrder？修改后无法复原，请先备份数据包。"))
        {
            return;
        }

        try
        {
            int updated = _packages.ReorderUnits(target, _unitRows.Select(r => r.Code).ToList());
            ExamViewKit.ShowStatus(_status, $"单位顺序已写回 Unit_Ess，更新 {updated} 行。", StatusTone.Success);
            RefreshUnits();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>顺序列跟着列表走，避免界面上出现「1 2 2 4」这种断号。</summary>
    private void Renumber()
    {
        for (int i = 0; i < _unitRows.Count; i++)
        {
            _unitRows[i].Order = i + 1;
        }

        // UnitRow 是可变 POCO，改 Order 不会触发通知；整体重挂一次让 ListView 重新取值。
        List<UnitRow> snapshot = _unitRows.ToList();
        _unitRows.Clear();
        foreach (UnitRow row in snapshot)
        {
            _unitRows.Add(row);
        }
    }

    private void ResetTarget()
    {
        string target = ExamViewKit.Value(_target);
        if (!_packages.IsPackageDirectory(target))
        {
            _dialogs.Info("目标数据包：" + _packages.Describe(target) + "。");
            return;
        }

        if (!_dialogs.Confirm("将清空目标包的 Exam_People / Unit_Ess，删除 *.exam 与 SelfReport 目录，无法撤销。是否继续？"))
        {
            return;
        }

        try
        {
            ExamPackageStatus status = _packages.Inspect(target);
            if (status.IsAlreadyInitialized && !_dialogs.Confirm("目标包已是初始化状态，是否仍要重置？"))
            {
                return;
            }

            int rows = _packages.Reset(target);
            ExamViewKit.ShowStatus(_status, $"已重置目标数据包（删除 {rows} 行人员/单位记录）。", StatusTone.Success);
            RefreshUnits();
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    private sealed class UnitRow
    {
        public UnitRow(int order, string unit, string code, string kind)
        {
            Order = order;
            Unit = unit;
            Code = code;
            Kind = kind;
        }

        public int Order { get; set; }

        public string Unit { get; }

        public string Code { get; }

        public string Kind { get; }
    }
}
