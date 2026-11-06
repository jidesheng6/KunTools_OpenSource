using System.Windows;
using System.Windows.Controls;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Exam;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// exam.mss3 — write ballot page images into the MSS3 ballot database
/// (port of Util.InsertVotePicContent + the Vote_EssInfo listing).
/// </summary>
public sealed class ExamMss3View : FeaturePageBase
{
    private const string ImageFilter = "图片|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|所有文件|*.*";

    private readonly IDialogService _dialogs;
    private readonly IMss3VoteService _votes;
    private readonly TextBox _database = ExamViewKit.PathBox("MSS3 库", "MSS3 / MDB 数据库路径");
    private readonly TextBox _image = ExamViewKit.PathBox("选票图片", "选票图片路径");
    private readonly ComboBox _voteList = new() { MinWidth = 420 };
    private readonly TextBox _voteId = new() { PlaceholderText = "VoteId", Width = 160 };
    private readonly TextBox _voteName = new() { PlaceholderText = "VoteName", Width = 260 };
    private readonly TextBox _matrix = new() { PlaceholderText = "矩阵码（可空）" };
    private readonly ComboBox _pageIndex = new() { MinWidth = 100 };
    private readonly ComboBox _squareType = new();
    private readonly TextBlock _status = ExamViewKit.Status("先选择 MSS3 数据库与选票图片。");
    private readonly Button _insert;

    public ExamMss3View(FeatureDescriptor descriptor, IServiceProvider services)
        : base(descriptor.Title, "把选票图片写入 MSS3 数据库的 Vote_PageInfo（对齐原「MSS3 模板修改」）。")
    {
        _dialogs = services.GetRequiredService<IDialogService>();
        _votes = services.GetRequiredService<IMss3VoteService>();
        IAppSettings settings = services.GetRequiredService<IAppSettings>();

        _database.Text = settings.Current.VfsoftMssDataPath;
        _squareType.ItemsSource = Mss3VoteService.SquareTypes;
        _squareType.SelectedIndex = 0;
        // 「第N页」文案由 Core 提供，Vote_PageInfo.PageIndex 存的是 0 基值（legacy BodyForm.cs:1452-1463）。
        _pageIndex.ItemsSource = Mss3VoteService.PageChoices
            .Select(choice => new PageChoice(choice.Index, choice.Label))
            .ToList();
        _pageIndex.DisplayMemberPath = nameof(PageChoice.Display);
        _pageIndex.SelectedIndex = 0;
        _voteList.DisplayMemberPath = nameof(VoteChoice.Display);
        _voteList.SelectionChanged += (_, _) =>
        {
            if (_voteList.SelectedItem is VoteChoice choice)
            {
                _voteId.Text = choice.Row.VoteId;
                _voteName.Text = choice.Row.VoteName;
                ExamViewKit.ShowStatus(
                    _status,
                    $"已载入选票：{choice.Row.VoteId} / {choice.Row.VoteName}（选项列 {choice.Row.SelName}）。");
            }
        };

        var pageRow = ExamViewKit.Field(
            "选票页面 / 选区框",
            ExamViewKit.Inline(Sized(_pageIndex, 110), Spacer(16), Sized(_squareType, 120)),
            stretchContent: false);

        Border source = ExamViewKit.Card(
            "数据源",
            ExamViewKit.Field("MSS3 库", _database, ExamViewKit.Secondary("选择…", () =>
            {
                // legacy「MSS3文件」对话框的过滤器是 *.mss3（选票模板不是考评库）；
                // 选错文件时 Core 会给「缺少表 Vote_EssInfo，请选择 .mss3 文件」的中文提示。
                ExamViewKit.PickFile(_dialogs, _database, ExamViewKit.Mss3Filter);
                RefreshVotes();
            })),
            ExamViewKit.Field("选票图片", _image, ExamViewKit.Secondary("选择…", () =>
                ExamViewKit.PickFile(_dialogs, _image, ImageFilter))));

        Border target = ExamViewKit.Card(
            "目标选票",
            ExamViewKit.Field("已有选票", _voteList, ExamViewKit.Secondary("刷新", RefreshVotes)),
            ExamViewKit.Field("选票 ID / 名称", ExamViewKit.Inline(Sized(_voteId, 160), Spacer(8), _voteName), stretchContent: false),
            ExamViewKit.Field("矩阵码", _matrix),
            pageRow);

        Border maintain = ExamViewKit.Card(
            "表结构维护",
            ExamViewKit.Hint("把 5 张表的 Item 列改为备注类型（长文本不再截断），或恢复为默认字段长度。"),
            ExamViewKit.ActionBar(
                ExamViewKit.Secondary("Item 列转备注", ConvertItemColumns),
                ExamViewKit.Secondary("恢复 Item 列长度", ResetItemColumnLengths)));

        _insert = ExamViewKit.Primary("插入选票模板", InsertBallotPage);
        var actions = ExamViewKit.ActionBar(_insert, ExamViewKit.Secondary("刷新选票列表", RefreshVotes));

        var bottom = new StackPanel();
        bottom.Children.Add(_status);
        bottom.Children.Add(actions);

        SetBody(BuildBottomActionLayout(ExamViewKit.FormArea(source, target, maintain), null, bottom));

        _database.TextChanged += (_, _) => SyncReady();
        _image.TextChanged += (_, _) => SyncReady();
        _voteId.TextChanged += (_, _) => SyncReady();
        _pageIndex.SelectionChanged += (_, _) => SyncReady();
        SyncReady();

        if (PathGuard.MissingFileReason(ExamViewKit.Value(_database), "MSS3 数据库") is null)
        {
            RefreshVotes();
        }
    }

    /// <summary>缺库/缺图/缺 VoteId 时禁用「插入选票模板」，把缺口写在状态行，避免点了才报错。</summary>
    private void SyncReady()
    {
        string database = ExamViewKit.Value(_database);
        string image = ExamViewKit.Value(_image);
        List<string> missing = new();
        if (PathGuard.MissingFileReason(database, "MSS3 数据库") is { } databaseProblem)
        {
            missing.Add(databaseProblem);
        }

        if (PathGuard.MissingFileReason(image, "选票图片") is { } imageProblem)
        {
            missing.Add(imageProblem);
        }

        if (ExamViewKit.Value(_voteId).Length == 0)
        {
            missing.Add("VoteId（可从上方选票列表带入）");
        }

        bool pageOk = _pageIndex.SelectedItem is PageChoice;
        if (!pageOk)
        {
            missing.Add("选票页面");
        }

        _insert.IsEnabled = missing.Count == 0;
        ExamViewKit.ShowStatus(
            _status,
            missing.Count == 0 ? "就绪：点击「插入选票模板」写入 Vote_PageInfo。" : "还缺：" + string.Join("、", missing) + "。");
    }

    private void RefreshVotes()
    {
        string database = ExamViewKit.Value(_database);
        _voteList.ItemsSource = null;
        if (PathGuard.MissingFileReason(database, "MSS3 数据库") is { } problem)
        {
            ExamViewKit.ShowStatus(_status, problem + "，请重新选择。", error: true);
            return;
        }

        try
        {
            IReadOnlyList<Mss3VoteRow> rows = _votes.ListVotes(database);
            _voteList.ItemsSource = rows.Select(r => new VoteChoice(r)).ToList();
            _voteList.SelectedIndex = rows.Count > 0 ? 0 : -1;
            ExamViewKit.ShowStatus(_status, rows.Count == 0
                ? "Vote_EssInfo 中没有选票，可直接填写 VoteId 插入。"
                : $"共 {rows.Count} 张选票。");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    private void InsertBallotPage()
    {
        string database = ExamViewKit.Value(_database);
        string image = ExamViewKit.Value(_image);
        string voteId = ExamViewKit.Value(_voteId);
        string voteName = ExamViewKit.Value(_voteName);

        if (PathGuard.MissingFileReason(database, "MSS3 数据库") is { } databaseProblem)
        {
            _dialogs.Info(databaseProblem + "，请先选择。");
            return;
        }

        if (PathGuard.MissingFileReason(image, "选票图片") is { } imageProblem)
        {
            _dialogs.Info(imageProblem + "，请先选择。");
            return;
        }

        if (voteId.Length == 0)
        {
            _dialogs.Info("请填写 VoteId（可从已有选票中选择）。");
            return;
        }

        if (_pageIndex.SelectedItem is not PageChoice page)
        {
            _dialogs.Info("请选择选票页面（第1页 / 第2页）。");
            return;
        }

        string squareType = _squareType.SelectedItem as string ?? Mss3VoteService.SquareTypeSquare;

        try
        {
            _votes.InsertVotePage(database, image, voteId, voteName.Length == 0 ? voteId : voteName,
                ExamViewKit.Value(_matrix), page.Index, squareType);
            ExamViewKit.ShowStatus(_status, $"已插入：{voteId} {page.Display}（PageIndex={page.Index}，{squareType}）。", StatusTone.Success);
            if (_dialogs.Confirm("插入完成，是否打开数据库所在目录？"))
            {
                ExamViewKit.RevealFile(database);
            }
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    private void ConvertItemColumns()
    {
        string database = ExamViewKit.Value(_database);
        if (PathGuard.MissingFileReason(database, "MSS3 数据库") is { } problem)
        {
            _dialogs.Info(problem + "，请先选择。");
            return;
        }

        if (!_dialogs.Confirm("将把 5 张表的 Item 列改为备注类型，无法自动还原，是否继续？"))
        {
            return;
        }

        try
        {
            int affected = _votes.SetItemColumnsToMemo(database);
            ExamViewKit.ShowStatus(_status, $"Item 列已改为备注类型，执行语句 {affected} 条。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>legacy「恢复项目列字段结构长度」（BodyForm.cs:4417-4432）：把 Item 列还原成默认长度。</summary>
    private void ResetItemColumnLengths()
    {
        string database = ExamViewKit.Value(_database);
        if (PathGuard.MissingFileReason(database, "MSS3 数据库") is { } problem)
        {
            _dialogs.Info(problem + "，请先选择。");
            return;
        }

        if (!_dialogs.Confirm("将把 5 张表的 Item 列恢复为默认长度（Report_Detail/Result_NumInfo 255，Result_DetailInfo/TempResult_DetailInfo 100，其余 50），超长内容会被截断，是否继续？"))
        {
            return;
        }

        try
        {
            int affected = _votes.ResetItemColumnLengths(database);
            ExamViewKit.ShowStatus(_status, $"Item 列长度已恢复，执行语句 {affected} 条。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    private static TextBox Sized(TextBox box, double width)
    {
        box.Width = width;
        return box;
    }

    private static ComboBox Sized(ComboBox box, double width)
    {
        box.Width = width;
        return box;
    }

    private static FrameworkElement Spacer(double width) => new Border { Width = width };

    /// <summary>「第N页」→ Vote_PageInfo.PageIndex（0 基，legacy BodyForm.cs:1452-1463 只有 0/1 两项）。</summary>
    private sealed record PageChoice(int Index, string Display);

    private sealed class VoteChoice
    {
        public VoteChoice(Mss3VoteRow row) => Row = row;

        public Mss3VoteRow Row { get; }

        public string Display => string.IsNullOrWhiteSpace(Row.VoteName)
            ? Row.VoteId
            : $"{Row.VoteId} — {Row.VoteName}（{Row.SelName}）";
    }
}
