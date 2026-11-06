using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Features;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Services.Exam;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// exam.vote — generate the election result report DOCX from the embedded template
/// (port of Util.GenerateVoteReportDocx).
/// </summary>
public sealed class ExamVoteView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly IVoteReportService _vote;
    private readonly TextBox _title = new() { Text = "选举结果报告单" };
    private readonly TextBox _startText = new() { Text = "根据投票情况，结果如下：" };
    private readonly TextBox _endText = new() { Text = "特此报告。" };
    private readonly TextBox _names = new()
    {
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        MinHeight = 140,
        PlaceholderText = "候选人姓名，每行一个"
    };
    // 占位文本只放短词：输入框宽 100–110，带「（默认）」的长占位会被裁掉（UiSnapshot 文本溢出检查）。
    private readonly TextBox _nameColumn = new()
    {
        PlaceholderText = "姓名",
        ToolTip = "表头第一列的列名，留空即用「姓名」。"
    };
    private readonly TextBox _agreeColumn = new()
    {
        PlaceholderText = "同意",
        ToolTip = "同意票的列名，留空即用「同意」。"
    };
    private readonly TextBox _disagreeColumn = new()
    {
        PlaceholderText = "不同意",
        ToolTip = "不同意票的列名，留空即用「不同意」。"
    };
    private readonly TextBox _abstainColumn = new()
    {
        PlaceholderText = "弃权",
        ToolTip = "弃权票的列名，留空即用「弃权」。"
    };
    private readonly System.Windows.Controls.CheckBox _keepIndex = new() { Content = "保留序号列", IsChecked = true };
    private readonly System.Windows.Controls.CheckBox _random = new() { Content = "随机票数（测试）" };
    private readonly System.Windows.Controls.CheckBox _another = new() { Content = "另选人表" };
    private readonly System.Windows.Controls.CheckBox _keepFooter = new() { Content = "保留页脚", IsChecked = true };

    // 排版三项（legacy 表格字体大小 / 表格数据行高 / 生成日期）：默认值取自 Core Schema，避免与业务默认值分叉。
    private readonly TextBox _fontSize = new()
    {
        Width = 70,
        Text = SchemaDefault("voteFontSize", "12"),
        ToolTip = "表格字号（磅），同时作用于表头与数据行。"
    };
    private readonly TextBox _dataRowHeight = new()
    {
        Width = 70,
        Text = SchemaDefault("voteRowHeight", "0.8"),
        ToolTip = "表格数据行高（厘米），留空或非法时用 0.8。"
    };
    private readonly TextBox _genDate = new()
    {
        Width = 150,
        Text = SchemaDefault("voteGenDate", string.Empty),
        PlaceholderText = "留空=当天",
        ToolTip = "手工填写报告单上的生成日期；留空时用当天（yyyy年MM月dd日）。"
    };
    private readonly TextBlock _status = ExamViewKit.Status("填写姓名后生成报告单。");
    private readonly Button _generate;

    public ExamVoteView(FeatureDescriptor descriptor, IServiceProvider services)
        : base(descriptor.Title, "用 embedded/VoteReportDoc.doc 模板生成选举结果报告 DOCX。")
    {
        _dialogs = services.GetRequiredService<IDialogService>();
        _vote = services.GetRequiredService<IVoteReportService>();

        Border text = ExamViewKit.Card(
            "正文",
            ExamViewKit.Field("标题", _title),
            ExamViewKit.Field("开头", _startText),
            ExamViewKit.Field("结尾", _endText));

        Border people = ExamViewKit.Card(
            "候选人",
            ExamViewKit.FieldTop("姓名", _names),
            ExamViewKit.Hint("每行一个姓名，生成的表格按这里的顺序排列。"),
            ExamViewKit.Field(
                "表头列名",
                ExamViewKit.Inline(
                    Sized(_nameColumn, 110),
                    Sized(_agreeColumn, 100),
                    Sized(_disagreeColumn, 100),
                    Sized(_abstainColumn, 100)),
                stretchContent: false),
            ExamViewKit.Hint("四个列名对应表格表头；留空即用默认列名（悬停输入框可看默认值）。"));

        Border layout = ExamViewKit.Card(
            "报告单排版",
            ExamViewKit.Field(
                "字号/行高",
                ExamViewKit.Inline(
                    _fontSize,
                    _dataRowHeight,
                    _genDate,
                    // legacy「填入当前日期」（VoteTableHelpDate_Click，BodyForm.cs:5784-5790）：按 yyyy年MM月dd日 填。
                    ExamViewKit.Secondary("填入当前日期", FillCurrentDate)),
                stretchContent: false),
            ExamViewKit.Hint("字号（磅）与数据行高（厘米）同时作用于表头和数据行；生成日期留空时用当天。"));

        var options = new WrapPanel { Margin = UiTokens.RowGap };
        foreach (System.Windows.Controls.CheckBox box in new[] { _keepIndex, _random, _another, _keepFooter })
        {
            box.Margin = UiTokens.ButtonGap;
            options.Children.Add(box);
        }

        _generate = ExamViewKit.Primary("生成报告单", Generate);
        var actions = ExamViewKit.ActionBar(_generate);

        var bottom = new StackPanel();
        bottom.Children.Add(_status);
        bottom.Children.Add(actions);

        SetBody(BuildBottomActionLayout(ExamViewKit.FormArea(text, people, layout), options, bottom));

        _names.TextChanged += (_, _) => SyncReady();
        SyncReady();
    }

    /// <summary>没有姓名就没有可生成的行，主操作先禁用并在状态行说明原因。</summary>
    private void SyncReady()
    {
        int count = SplitNames().Count;
        _generate.IsEnabled = count > 0;
        ExamViewKit.ShowStatus(
            _status,
            count > 0 ? $"就绪：将为 {count} 位候选人生成报告单。" : "还缺：候选人姓名（每行一个）。");
    }

    private List<string> SplitNames()
        => VoteReportRules.SplitCandidateNames(ExamViewKit.Value(_names)).ToList();

    /// <summary>legacy「填入当前日期」：yyyy年MM月dd日（VoteTableHelpDate_Click，BodyForm.cs:5784-5790）。</summary>
    private void FillCurrentDate()
        => _genDate.Text = DateTime.Now.ToString("yyyy年MM月dd日", CultureInfo.GetCultureInfo("zh-CN"));

    private void Generate()
    {
        List<string> fillNames = SplitNames();
        if (fillNames.Count == 0)
        {
            _dialogs.Info("请至少填写一位候选人姓名。");
            return;
        }

        string? path = _dialogs.SaveFile("Word 文档|*.docx", VoteReportRules.DefaultOutputFileName);
        if (path is null)
        {
            return;
        }

        try
        {
            using Stream? template = _vote.OpenDefaultTemplate();
            if (template is null)
            {
                ExamViewKit.ShowStatus(_status, "未找到模板 embedded/VoteReportDoc.doc。", error: true);
                _dialogs.Error("未找到模板 embedded/VoteReportDoc.doc。请确认 assets/embedded 已复制到输出目录。");
                return;
            }

            double fontSize = ParseDouble(ExamViewKit.Value(_fontSize), 12);
            double dataRowHeight = ParseDouble(ExamViewKit.Value(_dataRowHeight), 0.8);

            _vote.Create(new VoteReportRequest
            {
                Template = template,
                OutputFile = path,
                Title = ExamViewKit.Value(_title),
                StartText = ExamViewKit.Value(_startText),
                EndText = ExamViewKit.Value(_endText),
                GenerationDate = ExamViewKit.Value(_genDate),
                NameColumn = Default(_nameColumn, VoteReportRules.DefaultNameColumn),
                AgreeColumn = Default(_agreeColumn, VoteReportRules.DefaultAgreeColumn),
                UnAgreeColumn = Default(_disagreeColumn, VoteReportRules.DefaultUnAgreeColumn),
                AbstainColumn = Default(_abstainColumn, VoteReportRules.DefaultAbstainColumn),
                FillNames = fillNames,
                KeepNumberIndex = _keepIndex.IsChecked == true,
                RandomVoteCounts = _random.IsChecked == true,
                NeedAnotherTable = _another.IsChecked == true,
                KeepFooter = _keepFooter.IsChecked == true,
                FontSize = fontSize,
                DataRowFontSize = fontSize,
                RowHeightCentimeters = dataRowHeight,
                DataRowHeightCentimeters = dataRowHeight
            });

            ExamViewKit.ShowStatus(_status, $"已生成 {fillNames.Count} 人的报告单：{path}", StatusTone.Success);
            if (_dialogs.Confirm("已生成。是否打开文件？"))
            {
                ExamViewKit.OpenFile(path);
            }
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    private static string Default(TextBox box, string fallback)
    {
        string value = ExamViewKit.Value(box);
        return value.Length == 0 ? fallback : value;
    }

    /// <summary>取 Core Schema 里的默认值，保证 UI 初值与业务默认值同源（FeatureOptionSchemas 的 exam.vote 段）。</summary>
    private static string SchemaDefault(string key, string fallback)
    {
        foreach (OptionSection section in FeatureOptionSchemas.GetSections("exam.vote"))
        {
            foreach (OptionField field in section.Fields)
            {
                if (string.Equals(field.Key, key, StringComparison.Ordinal) &&
                    !string.IsNullOrWhiteSpace(field.DefaultValue))
                {
                    return field.DefaultValue;
                }
            }
        }

        return fallback;
    }

    /// <summary>排版数值：非法或非正数一律退回默认值，避免把 0 号字/0 行高写进 DOCX。</summary>
    private static double ParseDouble(string text, double fallback)
        => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) && value > 0
            ? value
            : fallback;

    private static TextBox Sized(TextBox box, double width)
    {
        box.Width = width;
        return box;
    }
}
