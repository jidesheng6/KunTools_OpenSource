using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Text;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Tools;

public sealed class IdCardFeatureModule : FeatureModuleBase
{
    public IdCardFeatureModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "idcard.check"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new IdCardView(services.GetRequiredService<IRandomIdCardService>());
}

/// <summary>Matches legacy Tab「身份证号码校验」: validate box + result rows (no file picker).</summary>
public sealed class IdCardView : FeaturePageBase
{
    private readonly TextBox _input = new() { PlaceholderText = "输入 18 位身份证号", FontSize = 16 };
    private readonly TextBlock _birthday = Result("—");
    private readonly TextBlock _sex = Result("—");
    private readonly TextBlock _index = Result("—");
    private readonly TextBlock _checksum = Result("—");
    private readonly TextBlock _status = ToolViewKit.Status("粘贴或输入 18 位身份证号码后即时校验；也可点「校验」重新判定。");

    // 随机身份证号码生成（对齐 legacy TabPage_34 的 gen_card_* 一组）。
    private readonly IRandomIdCardService _random;
    private readonly ComboBox _province = new() { MinWidth = 140, IsEditable = false, DisplayMemberPath = nameof(DivisionNode.Name) };
    private readonly ComboBox _city = new() { MinWidth = 140, IsEditable = false, DisplayMemberPath = nameof(DivisionNode.Name) };
    private readonly ComboBox _district = new() { MinWidth = 160, IsEditable = false, DisplayMemberPath = nameof(DivisionNode.Name) };
    private readonly TextBox _genCount = ToolViewKit.NumberBox("生成数量", "如 10", 90);
    private readonly CheckBox _genAdult = new() { Content = "仅年满 18 岁", Margin = new Thickness(12, 0, 12, 0) };
    private readonly CheckBox _genNames = new() { Content = "同时生成姓名" };
    private readonly TextBox _genOutput = new()
    {
        IsReadOnly = true,
        AcceptsReturn = true,
        TextWrapping = TextWrapping.NoWrap,
        MinHeight = 120,
        MaxHeight = 200,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
        FontFamily = new FontFamily("Consolas")
    };
    private readonly TextBlock _genStatus = ToolViewKit.Status(
        "选择省 / 市 / 区县后点「生成」；勾选「同时生成姓名」则号码与姓名成对输出。");

    public IdCardView(IRandomIdCardService random)
        : base("身份证号码校验", "输入号码即时校验 / 按行政区随机生成合法号码（对齐原 ver_card_id_*、gen_card_* 控件）")
    {
        _random = random;
        _genCount.Text = "10";
        _input.TextChanged += (_, _) => ValidateNow();

        // 读屏软件：输入框靠占位提示识别，输入后提示消失；结果四项是纯文本，也要有名称。
        AutomationProperties.SetName(_input, "身份证号码（18 位）");
        AutomationProperties.SetName(_birthday, "出生日期");
        AutomationProperties.SetName(_sex, "性别");
        AutomationProperties.SetName(_index, "顺序码");
        AutomationProperties.SetName(_checksum, "校验码");

        Border inputCard = ToolViewKit.Card(
            "身份证号码",
            _input,
            ToolViewKit.Hint("支持直接粘贴；校验码、出生日期与顺序码会同时给出判定。"));

        Border resultCard = ToolViewKit.Card(
            "校验结果",
            ToolViewKit.Field("出生日期", _birthday),
            ToolViewKit.Field("性别", _sex),
            ToolViewKit.Field("顺序码", _index),
            ToolViewKit.Field("校验码", _checksum));

        AutomationProperties.SetName(_province, "省");
        AutomationProperties.SetName(_city, "市");
        AutomationProperties.SetName(_district, "区县");
        AutomationProperties.SetName(_genCount, "生成数量");
        AutomationProperties.SetName(_genOutput, "生成的身份证号码");
        InitialiseGenerators();

        Border genCard = ToolViewKit.Card(
            "随机身份证号码生成",
            ToolViewKit.Row(
                ToolViewKit.Field("省", _province, null, 40, stretchContent: false),
                ToolViewKit.Field("市", _city, null, 40, stretchContent: false),
                ToolViewKit.Field("区县", _district, null, 52, stretchContent: false)),
            ToolViewKit.Row(
                ToolViewKit.Field("数量", _genCount, null, 44, stretchContent: false),
                _genAdult,
                _genNames),
            ToolViewKit.ActionBar(
                ToolViewKit.Secondary("生成", Generate),
                ToolViewKit.Secondary("复制生成结果", CopyGenerated)),
            _genStatus,
            ToolViewKit.Frame(_genOutput, 140),
            ToolViewKit.Hint("号码 = 区县编码 + 随机生日 + 随机顺序码 + 校验位，全部为合法号，仅供测试数据使用。"));

        var validate = ToolViewKit.Primary("校验", ValidateNow);
        var actions = ToolViewKit.ActionBar(
            validate,
            ToolViewKit.Secondary("复制结果", CopyResult),
            ToolViewKit.Secondary("清空", () => _input.Text = string.Empty));

        SetBody(BuildToolLayout(ToolViewKit.FormArea(inputCard, resultCard, genCard), _status, actions));

        // 键盘：焦点不在输入框时 Enter = 校验；Esc 收回状态行（不清空已输入的号码）。
        ToolViewKit.AttachPageKeyboard(
            this,
            validate,
            () => ToolViewKit.ShowStatus(_status, "粘贴或输入 18 位身份证号码后即时校验；也可点「校验」重新判定。"));
    }

    private void InitialiseGenerators()
    {
        _province.ItemsSource = _random.Provinces;
        _province.SelectionChanged += (_, _) =>
        {
            _city.ItemsSource = _province.SelectedItem is DivisionNode p ? _random.Cities(p.Code) : null;
            _city.SelectedIndex = _city.Items.Count > 0 ? 0 : -1;
        };
        _city.SelectionChanged += (_, _) =>
        {
            _district.ItemsSource = _province.SelectedItem is DivisionNode p && _city.SelectedItem is DivisionNode c
                ? _random.Districts(p.Code, c.Code)
                : null;
            _district.SelectedIndex = _district.Items.Count > 0 ? 0 : -1;
        };

        if (_province.Items.Count > 0)
        {
            _province.SelectedIndex = 0;
        }
    }

    private void Generate()
    {
        if (_district.SelectedItem is not DivisionNode district)
        {
            ToolViewKit.ShowStatus(_genStatus, "请先选择省 / 市 / 区县，再点「生成」。", StatusTone.Error);
            return;
        }

        if (!int.TryParse(ToolViewKit.Value(_genCount), out int count) || count < 1)
        {
            ToolViewKit.ShowStatus(_genStatus, "生成数量必须是大于 0 的整数。", StatusTone.Error);
            return;
        }

        try
        {
            var batch = _random.Generate(district.Code, count, _genAdult.IsChecked == true, _genNames.IsChecked == true);
            var sb = new StringBuilder();
            foreach (GeneratedIdCard item in batch)
            {
                sb.AppendLine(item.Name is null ? item.Id : $"{item.Id}  {item.Name}");
            }

            _genOutput.Text = sb.ToString().TrimEnd();
            ToolViewKit.ShowStatus(_genStatus, $"已生成 {batch.Count} 条（{district.Name}）。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_genStatus, "生成失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void CopyGenerated()
    {
        string text = _genOutput.Text?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            ToolViewKit.ShowStatus(_genStatus, "还没有生成结果：请先点「生成」。", StatusTone.Error);
            return;
        }

        Clipboard.SetText(text);
        ToolViewKit.ShowStatus(_genStatus, "已复制生成结果到剪贴板。", StatusTone.Success);
    }

    private void ValidateNow()
    {
        string text = _input.Text?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            ResetResults();
            ToolViewKit.ShowStatus(_status, "粘贴或输入 18 位身份证号码后即时校验；也可点「校验」重新判定。");
            return;
        }

        var result = ChineseIdCard.Validate(text);
        _birthday.Text = result.Birthday;
        _birthday.Foreground = result.Birthday == "日期不正确" ? Brushes.Tomato : Brushes.ForestGreen;
        _sex.Text = result.Sex;
        _sex.Foreground = Brushes.ForestGreen;
        _index.Text = result.Sequence;
        _index.Foreground = Brushes.ForestGreen;

        // 合法号码也显示校验码字符；不合法时同样给出「应为」的那个字符，方便直接改。
        _checksum.Text = string.IsNullOrEmpty(result.ExpectedChecksum)
            ? "—"
            : $"{result.ExpectedChecksum}（末位应为 {result.ExpectedChecksum}）";
        _checksum.Foreground = string.IsNullOrEmpty(result.ExpectedChecksum)
            ? UiTokens.MutedBrush
            : result.IsChecksumValid ? Brushes.ForestGreen : Brushes.Tomato;

        if (result.IsChecksumValid)
        {
            ToolViewKit.ShowStatus(_status, "校验通过：编号格式与校验码均正确。", StatusTone.Success);
            return;
        }

        ToolViewKit.ShowStatus(
            _status,
            string.IsNullOrEmpty(result.ExpectedChecksum)
                ? "号码格式不正确：请确认是 18 位数字（末位可为 X）。"
                : $"校验码不正确：末位应为 {result.ExpectedChecksum}。",
            StatusTone.Error);
    }

    private void CopyResult()
    {
        string text = _input.Text?.Trim() ?? string.Empty;
        if (text.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "还没有可复制的结果：请先输入身份证号。", StatusTone.Error);
            return;
        }

        Clipboard.SetText(
            $"{text}\n生日：{_birthday.Text}  性别：{_sex.Text}  顺序码：{_index.Text}  校验码：{_checksum.Text}");
        ToolViewKit.ShowStatus(_status, "已复制校验结果到剪贴板。", StatusTone.Success);
    }

    private void ResetResults()
    {
        foreach (TextBlock block in new[] { _birthday, _sex, _index, _checksum })
        {
            block.Text = "—";
            block.Foreground = UiTokens.MutedBrush;
        }
    }

    private static TextBlock Result(string text)
        => new()
        {
            Text = text,
            FontSize = 15,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center
        };
}
