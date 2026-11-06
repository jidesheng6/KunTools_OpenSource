using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Exam;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// exam.single — generate one Word document per person by matching Shape.AlternativeText
/// (port of Util.SingleDocumentReplacePersonCode).
/// </summary>
public sealed class ExamSingleView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly ISingleDocumentService _single;
    private readonly TextBox _template = ExamViewKit.PathBox("单人表 Word 模板", "单人表 Word 模板");
    private readonly TextBox _imageSource = ExamViewKit.PathBox("人员图片目录 / 单张图片", "人员图片目录 / 单张图片");
    private readonly TextBox _output = ExamViewKit.PathBox("输出目录", "输出目录", folder: true);
    private readonly TextBox _count = new() { PlaceholderText = "空 = 全部" };
    private readonly TextBox _alternatives = new() { Text = "人员码", PlaceholderText = "Alt 文本，用 @ 分隔" };
    private readonly ComboBox _suffixMode = new() { SelectedIndex = 0, MinWidth = 180 };
    private readonly TextBox _suffixWord = new() { Text = SingleDocumentNaming.DefaultSuffixWord, Width = 140 };
    private readonly CheckBox _useBuiltIn = new()
    {
        Content = "使用内置测评人码图（001–999）",
        IsChecked = true,
    };
    private readonly TextBox _startIndex = new()
    {
        Text = PersonCodeLibrary.FirstIndex.ToString(CultureInfo.InvariantCulture),
        Width = 90,
        ToolTip = "内置码图起始序号，001–999；份数留空表示取到 999 为止。",
    };
    private readonly TextBlock _imageSourceHint = ExamViewKit.Hint("正在使用内置码图，图片源已清空；需要外部图片请取消勾选上方「使用内置测评人码图」。");
    private readonly TextBlock _status = ExamViewKit.Status("选择模板、图片源与输出目录后点击「开始生成」。");
    private readonly Button _generate;

    /// <summary>与 <see cref="SingleDocumentNameSuffixMode"/> 一一对应的下拉项（legacy Designer:11499-11535 三档）。</summary>
    private static readonly (SingleDocumentNameSuffixMode Mode, string Label)[] SuffixModes =
    [
        (SingleDocumentNameSuffixMode.NumberWithSuffix, "纯数字+后缀（001测评人）"),
        (SingleDocumentNameSuffixMode.PersonIndex, "第X人（第1人）"),
        (SingleDocumentNameSuffixMode.PageIndex, "第X页（第1页）")
    ];

    public ExamSingleView(FeatureDescriptor descriptor, IServiceProvider services)
        : base(descriptor.Title, "按图片 Alt 文本匹配单人表模板中的占位图形，批量生成 Word 文档。")
    {
        _dialogs = services.GetRequiredService<IDialogService>();
        _single = services.GetRequiredService<ISingleDocumentService>();

        // Field 会把控件设成 Stretch，带固定宽度的文本框会被居中；用 Inline 包一层才是左对齐起点。
        _count.Width = 120;
        foreach ((SingleDocumentNameSuffixMode mode, string label) in SuffixModes)
        {
            _suffixMode.Items.Add(new ComboBoxItem { Content = label, Tag = mode });
        }

        Border card = ExamViewKit.Card(
            "素材",
            ExamViewKit.Field("Word 模板", _template, ExamViewKit.Secondary("选择…", () =>
                ExamViewKit.PickFile(_dialogs, _template, "Word|*.doc;*.docx|所有文件|*.*"))),
            ExamViewKit.Field("图片源", _imageSource, ExamViewKit.Secondary("选择…", PickImageSource)),
            ExamViewKit.Field(
                "内置码图",
                _useBuiltIn,
                ExamViewKit.Inline(_startIndex)),
            _imageSourceHint,
            ExamViewKit.Field("输出目录", _output, ExamViewKit.Secondary("选择…", () =>
                ExamViewKit.PickFolder(_dialogs, _output))),
            ExamViewKit.Field("Alt 文本", _alternatives),
            ExamViewKit.Field("生成份数", ExamViewKit.Inline(_count)),
            ExamViewKit.Field("生成名称后缀", ExamViewKit.Inline(_suffixMode, _suffixWord)),
            ExamViewKit.Hint("勾选「使用内置测评人码图」时不需要图片源，可用序号 001–999；取消勾选后使用外部图片。份数留空表示按图片数量全部生成；替代文字可用 @ 分隔多个占位名。文件名 = 模板名 + 后缀（如「模板（001测评人）.docx」），后缀词只在「纯数字+后缀」模式下参与拼接。"));

        // 后缀词只影响「纯数字+后缀」档，其它档禁用，避免误以为它生效。
        // 注意：SyncSuffixWord 会走到 SyncReady，而 SyncReady 要设置 _generate.IsEnabled，
        // 所以首次调用必须放在 _generate 赋值之后（否则构造期 NRE）。
        _suffixMode.SelectionChanged += (_, _) => SyncSuffixWord();
        _suffixWord.TextChanged += (_, _) => SyncReady();

        _generate = ExamViewKit.Primary("开始生成", Generate);
        var actions = ExamViewKit.ActionBar(_generate);

        var bottom = new StackPanel();
        bottom.Children.Add(_status);
        bottom.Children.Add(actions);

        _template.TextChanged += (_, _) => SyncReady();
        _imageSource.TextChanged += (_, _) => SyncReady();
        _output.TextChanged += (_, _) => SyncReady();
        _alternatives.TextChanged += (_, _) => SyncReady();
        _useBuiltIn.Checked += (_, _) => SyncBuiltIn();
        _useBuiltIn.Unchecked += (_, _) => SyncBuiltIn();
        _startIndex.TextChanged += (_, _) => SyncReady();
        SyncSuffixWord();
        SyncBuiltIn();

        SetBody(BuildBottomActionLayout(ExamViewKit.FormArea(card), null, bottom));
    }

    /// <summary>
    /// 勾选内置码图时不需要图片源（legacy <c>SignDocumentUseEmbedImage</c> 默认勾选，Designer:11616），
    /// 对应地把起始序号放开、图片源清空并提示，避免用户以为两处都要填。
    /// <para>
    /// 这里刻意不用 <c>IsEnabled = false</c>：路径框是只读框，禁用会连「选中复制路径」都做不到（见 <c>Ui\PathInputs.cs</c>）。
    /// </para>
    /// </summary>
    private void SyncBuiltIn()
    {
        bool builtIn = _useBuiltIn.IsChecked == true;
        if (builtIn)
        {
            _imageSource.Text = string.Empty;
        }

        _startIndex.IsEnabled = builtIn;
        _imageSourceHint.Visibility = builtIn ? Visibility.Visible : Visibility.Collapsed;
        SyncReady();
    }

    private bool UseBuiltIn() => _useBuiltIn.IsChecked == true;

    /// <summary>起始序号；留空或非法时回落 001（越界由 Core 抛中文异常说明）。</summary>
    private int StartIndex()
        => int.TryParse(ExamViewKit.Value(_startIndex), out int value) && value > 0
            ? value
            : PersonCodeLibrary.FirstIndex;

    /// <summary>
    /// 未填齐三项输入时禁用主操作，并在状态行说清还缺什么，而不是让用户点了再弹窗。
    /// <para>依赖 <c>_generate</c>：构造期只能在「主按钮已赋值」之后调用，否则 _generate 还是 null（会 NRE）。</para>
    /// </summary>
    private void SyncReady()
    {
        List<string> missing = new();
        if (ExamViewKit.Value(_template).Length == 0) missing.Add("Word 模板");
        if (!UseBuiltIn() && ExamViewKit.Value(_imageSource).Length == 0) missing.Add("图片源");
        if (ExamViewKit.Value(_output).Length == 0) missing.Add("输出目录");
        if (ExamViewKit.Value(_alternatives).Length == 0) missing.Add("Alt 文本");

        _generate.IsEnabled = missing.Count == 0;
        ExamViewKit.ShowStatus(
            _status,
            missing.Count == 0 ? "就绪：点击「开始生成」。" : "还缺：" + string.Join("、", missing) + "。");
    }

    /// <summary>后缀词仅对「纯数字+后缀」档生效，其它档禁用并在状态行说明。</summary>
    private void SyncSuffixWord()
    {
        bool needsWord = SelectedSuffixMode() == SingleDocumentNameSuffixMode.NumberWithSuffix;
        _suffixWord.IsEnabled = needsWord;
        SyncReady();
    }

    private SingleDocumentNameSuffixMode SelectedSuffixMode()
        => _suffixMode.SelectedItem is ComboBoxItem { Tag: SingleDocumentNameSuffixMode mode }
            ? mode
            : SingleDocumentNameSuffixMode.NumberWithSuffix;

    private void PickImageSource()
    {        IReadOnlyList<string> files = _dialogs.OpenFiles(
            "图片|*.jpg;*.jpeg;*.png;*.bmp;*.tif;*.tiff|所有文件|*.*",
            multiSelect: false);
        if (files.Count > 0)
        {
            _imageSource.Text = files[0];
            return;
        }

        ExamViewKit.PickFolder(_dialogs, _imageSource);
    }

    private void Generate()
    {
        string template = ExamViewKit.Value(_template);
        string source = ExamViewKit.Value(_imageSource);
        string output = ExamViewKit.Value(_output);
        bool builtIn = UseBuiltIn();

        if (template.Length == 0 || output.Length == 0 || (!builtIn && source.Length == 0))
        {
            _dialogs.Info(builtIn ? "请填写模板与输出目录。" : "请填写模板、图片源与输出目录。");
            return;
        }

        List<string> alts = SingleDocumentNaming.ParseAltTexts(ExamViewKit.Value(_alternatives)).ToList();
        if (alts.Count == 0)
        {
            _dialogs.Info("请填写至少一个 Alt 匹配文本。");
            return;
        }

        int? max = int.TryParse(ExamViewKit.Value(_count), out int n) && n > 0 ? n : null;

        var request = new SingleDocumentRequest
        {
            TemplatePath = template,
            AltTexts = alts,
            ImageSourcePathOrDirectory = source,
            OutputDirectory = output,
            MaxCount = max,
            NameSuffixMode = SelectedSuffixMode(),
            SuffixWord = ExamViewKit.Value(_suffixWord),
            UseBuiltInPersonCode = builtIn,
            PersonCodeStartIndex = StartIndex()
        };

        try
        {
            IReadOnlyList<string> outputs = _single.GenerateBatch(request);
            if (outputs.Count == 0)
            {
                ExamViewKit.ShowStatus(_status, "未找到可处理的图片。", error: true);
                _dialogs.Info("未找到可处理的图片。");
                return;
            }

            ExamViewKit.ShowStatus(_status, $"已生成 {outputs.Count} 份文档：{output}", StatusTone.Success);
            if (_dialogs.Confirm($"已生成 {outputs.Count} 份。是否打开输出目录？"))
            {
                ExamViewKit.OpenFolder(output);
            }
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }
}
