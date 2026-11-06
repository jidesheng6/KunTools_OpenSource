using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.App.Ui.Options;
using DocumentTools.Core.Features;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Services.Text;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Tools;

public sealed class NameVerticalFeatureModule : FeatureModuleBase
{
    public NameVerticalFeatureModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "name.vertical"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new NameVerticalView(
            services.GetRequiredService<INameVerticalService>(),
            services.GetRequiredService<IDialogService>(),
            Descriptor.Id);
}

/// <summary>
/// 对齐 legacy Tab「姓名横转竖列」（<c>BodyForm.ConvertTextButton_Click</c>）：源/结果双文本框 + 参数区，无文件列表。
/// 布局沿用工具页骨架：内容在上，处理选项与状态/操作在底部。
/// 处理参数一律来自 <see cref="FeatureOptionSchemas"/>，视图只做键到 <see cref="NameVerticalOptions"/> 的映射。
/// </summary>
public sealed class NameVerticalView : FeaturePageBase
{
    /// <summary>重名行最多列出多少条，避免一次粘几千个姓名时把结果区挤掉。</summary>
    private const int MaxDuplicateRows = 50;

    /// <summary>逗号/分号/空白视作姓名分隔符；含 <c>\t</c> 的表格粘贴交给 Core 按单元格切分。</summary>
    private static readonly Regex Separators = new("[,，;；\\s]+", RegexOptions.Compiled);

    private readonly INameVerticalService _service;
    private readonly IDialogService _dialogs;
    private readonly OptionPanelBuilder _options;
    private readonly StackPanel _optionsHost = new();

    private readonly TextBox _source = new()
    {
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        PlaceholderText = "每行一个姓名，也可用逗号分隔",
        MinHeight = 180,
        MaxHeight = 300
    };

    private readonly TextBox _target = new()
    {
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        IsReadOnly = true,
        PlaceholderText = "竖排结果：每行一个字，可直接粘到 Word 的竖排表格里",
        MinHeight = 180,
        MaxHeight = 300
    };

    /// <summary>页面级行为（不属于处理参数，因此不进 Schema）：转换成功后顺手复制。</summary>
    private readonly CheckBox _autoCopy = new()
    {
        Content = "转换成功后复制到剪贴板",
        Margin = new Thickness(0, 0, 16, 0),
        VerticalAlignment = VerticalAlignment.Center
    };

    private readonly StackPanel _duplicateRows = new();
    private readonly ScrollViewer _duplicateScroll = new()
    {
        MaxHeight = 120,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
    };

    private readonly Border _duplicateCard;
    private readonly TextBlock _status = ToolViewKit.Status("粘贴姓名后点「转换」；结果每行一个字，可直接粘到 Word 的竖排表里。");

    public NameVerticalView(INameVerticalService service, IDialogService dialogs, string featureId)
        : base("姓名横转竖", "粘贴姓名列表后转换；处理选项与原「姓名横转竖列」页逐项对应")
    {
        _service = service;
        _dialogs = dialogs;

        // 读屏软件：源/结果两个多行框靠占位提示区分，输入后提示就没了，必须给名称与用途。
        AutomationProperties.SetName(_source, "源文本：姓名列表");
        AutomationProperties.SetHelpText(_source, "每行一个姓名，也可以用逗号分隔；支持从 Word 表格整列粘贴。");
        AutomationProperties.SetName(_target, "竖排结果（只读）");

        _options = new OptionPanelBuilder(dialogs);
        _options.BuildInto(_optionsHost, FeatureOptionSchemas.GetSections(featureId));

        _source.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                ConvertNames();
                e.Handled = true;
            }
        };

        Border sourceCard = ToolViewKit.Card("源文本", _source);
        sourceCard.Margin = new Thickness(0, 0, 6, 0);

        _duplicateScroll.Content = _duplicateRows;
        _duplicateCard = ToolViewKit.Card(
            "重名检查",
            ToolViewKit.Hint("列出重复出现的姓名（同一姓名出现两次以上），只在勾选「高亮重名」时显示。"),
            ToolViewKit.Hint("注意：「文本去重」开启时重复姓名会被直接去掉，这里通常为空；要看重名请先取消「文本去重」。"),
            _duplicateScroll);
        _duplicateCard.Visibility = Visibility.Collapsed;
        AutomationProperties.SetName(_duplicateCard, "重名检查结果");

        var resultColumn = new Grid();
        resultColumn.RowDefinitions.Add(new RowDefinition());
        resultColumn.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Border resultCard = ToolViewKit.Card("竖排结果", _target);
        resultCard.Margin = new Thickness(6, 0, 0, 0);
        Grid.SetRow(resultCard, 0);
        resultColumn.Children.Add(resultCard);

        Grid.SetRow(_duplicateCard, 1);
        resultColumn.Children.Add(_duplicateCard);

        // 两列等宽但各自不低于路径/长文本可读宽度：窗口变窄时先挤滚动条，不把源框压成一条缝。
        var split = new Grid();
        split.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = UiTokens.PathBoxMinWidth });
        split.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = UiTokens.PathBoxMinWidth });
        split.Children.Add(sourceCard);
        Grid.SetColumn(resultColumn, 1);
        split.Children.Add(resultColumn);

        var optionsStack = new StackPanel();
        optionsStack.Children.Add(_optionsHost);
        optionsStack.Children.Add(ToolViewKit.Row(_autoCopy));

        var optionsArea = new ScrollViewer
        {
            Content = optionsStack,
            MaxHeight = 210,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,

            // 选项区只是容器：Tab 应直接落到里面的勾选框，不必先在滚动区停一次。
            IsTabStop = false
        };

        var convert = ToolViewKit.Primary("转换", ConvertNames);
        var actions = ToolViewKit.ActionBar(
            convert,
            ToolViewKit.Secondary("复制结果", CopyResult),
            ToolViewKit.Secondary("导出 TXT", ExportTxt),
            ToolViewKit.Secondary("清空", ClearAll));

        SetBody(BuildToolLayout(ToolViewKit.FormArea(split), optionsArea, _status, actions));

        // 键盘：Ctrl+Enter 在源文本框里已可转换；页面级 Enter（焦点不在输入框时）同样转换。
        // Esc 收起状态行与重名卡，不动源文本与结果。
        ToolViewKit.AttachPageKeyboard(this, convert, () =>
        {
            ShowDuplicates(null, markDuplicate: false);
            ToolViewKit.ShowStatus(_status, "粘贴姓名后点「转换」；结果每行一个字，可直接粘到 Word 的竖排表里。");
        });
    }

    private void ConvertNames()
    {
        string text = _source.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            ShowDuplicates(null, markDuplicate: false);
            ToolViewKit.ShowStatus(_status, "请先粘贴姓名（每行一个，也可用逗号/空格分隔），再点「转换」。", StatusTone.Error);
            return;
        }

        NameVerticalResult result = _service.Process(NormalizeSeparators(text), BuildOptions());
        _target.Text = result.ToVerticalText();

        if (result.Count == 0)
        {
            ShowDuplicates(null, markDuplicate: false);
            ToolViewKit.ShowStatus(
                _status,
                "转换结果为空：请检查「移除数字 / 移除括号」是否把姓名都清掉了。",
                StatusTone.Error);
            return;
        }

        bool markDuplicate = _options.GetBool("markDuplicate");
        ShowDuplicates(result, markDuplicate);

        var message = new StringBuilder();
        message.Append("已生成 ")
            .Append(CountLines(result.ToVerticalText()))
            .Append(" 行竖排文本（")
            .Append(result.Count)
            .Append(" 个姓名）");

        if (markDuplicate && result.DuplicateLineIndices.Count > 0)
        {
            message.Append("，其中 ").Append(result.DuplicateLineIndices.Count).Append(" 行重名（见「重名检查」）");
        }

        message.Append('。');

        if (_autoCopy.IsChecked == true)
        {
            Clipboard.SetText(_target.Text);
            message.Append("已复制到剪贴板。");
        }

        ToolViewKit.ShowStatus(_status, message.ToString(), StatusTone.Success);
    }

    private NameVerticalOptions BuildOptions()
    {
        IReadOnlyDictionary<string, string> values = _options.Collect();
        values.TryGetValue("pinyinMode", out string? pinyinMode);

        return new NameVerticalOptions
        {
            Distinct = _options.GetBool("distinct"),
            RemoveNumber = _options.GetBool("removeNumber"),
            RemoveSquare = _options.GetBool("removeSquare"),
            DoubleAlign = _options.GetBool("doubleAlign"),
            ReplaceEnabled = _options.GetBool("replaceEnabled"),
            ReplaceSearch = Raw(values, "replaceSearch"),
            ReplaceTarget = Raw(values, "replaceTarget"),
            ReplaceRegex = _options.GetBool("replaceRegex"),
            PinyinEnabled = _options.GetBool("pinyinEnabled"),
            PinyinMode = ParsePinyinMode(pinyinMode)
        };
    }

    private static string Raw(IReadOnlyDictionary<string, string> values, string key)
        => values.TryGetValue(key, out string? value) ? value ?? string.Empty : string.Empty;

    /// <summary>下拉值来自 Schema 的 <c>OptionChoice.Value</c>（与枚举同名），此处只做解析，不抄标签。</summary>
    private static NamePinyinMode ParsePinyinMode(string? value)
        => Enum.TryParse(value, ignoreCase: true, out NamePinyinMode mode) ? mode : NamePinyinMode.InitialsUpper;

    /// <summary>
    /// 含制表符（Word 表格粘贴）时原样交给 Core，由它按单元格切分并额外清掉 ○ □ £；
    /// 其余输入沿用旧版页面承诺：逗号、分号、空格也算姓名分隔符。
    /// </summary>
    private static string NormalizeSeparators(string text)
        => text.Contains('\t') ? text : Separators.Replace(text, Environment.NewLine).Trim();

    private static int CountLines(string text)
        => text.Replace("\r", string.Empty).Split('\n').Length;

    private void ShowDuplicates(NameVerticalResult? result, bool markDuplicate)
    {
        _duplicateRows.Children.Clear();

        if (!markDuplicate || result is null)
        {
            _duplicateCard.Visibility = Visibility.Collapsed;
            return;
        }

        if (result.DuplicateLineIndices.Count == 0)
        {
            _duplicateRows.Children.Add(ToolViewKit.Hint("未发现重名。"));
            _duplicateCard.Visibility = Visibility.Visible;
            return;
        }

        int shown = 0;
        foreach (int index in result.DuplicateLineIndices)
        {
            if (shown >= MaxDuplicateRows)
            {
                _duplicateRows.Children.Add(ToolViewKit.Hint($"另有 {result.DuplicateLineIndices.Count - shown} 行重名未列出。"));
                break;
            }

            _duplicateRows.Children.Add(DuplicateRow(index, result.Lines[index]));
            shown++;
        }

        _duplicateCard.Visibility = Visibility.Visible;
    }

    /// <summary>重名行：行号 + 姓名 + 「重名」徽标（列表行上的视觉标记，不是日志）。</summary>
    private static UIElement DuplicateRow(int index, string name)
    {
        var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(64) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var line = new TextBlock
        {
            Text = $"第 {index + 1} 行",
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(line, 0);
        row.Children.Add(line);

        // 这里不做 Trim：双字对齐插入的是两个半角空格，去掉就看不出对齐了。
        var text = new TextBlock
        {
            Text = name,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        Grid.SetColumn(text, 1);
        row.Children.Add(text);

        var badge = new Border
        {
            Background = UiTokens.SubtleBrush,
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 1, 6, 1),
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = "重名",
                FontSize = UiTokens.CaptionFontSize,
                Foreground = Brushes.Tomato
            }
        };
        Grid.SetColumn(badge, 2);
        row.Children.Add(badge);

        return row;
    }

    private void CopyResult()
    {
        string result = _target.Text ?? string.Empty;
        if (result.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "还没有可复制的结果：请先点「转换」。", StatusTone.Error);
            return;
        }

        Clipboard.SetText(result);
        ToolViewKit.ShowStatus(_status, $"已复制 {CountLines(result)} 行到剪贴板。", StatusTone.Success);
    }

    /// <summary>对齐 legacy <c>ExportNameTxt_Click</c>：逐行写出、UTF-8（带 BOM），空结果不弹保存框。</summary>
    private void ExportTxt()
    {
        string text = _target.Text ?? string.Empty;
        if (text.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "还没有可导出的结果：请先点「转换」。", StatusTone.Error);
            return;
        }

        string? path = _dialogs.SaveFile("文本文件|*.txt", "姓名纵排.txt");
        if (string.IsNullOrWhiteSpace(path))
        {
            ToolViewKit.ShowStatus(_status, "已取消导出，结果仍保留在右侧。");
            return;
        }

        try
        {
            string normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", Environment.NewLine);
            File.WriteAllText(path, normalized + Environment.NewLine, Encoding.UTF8);
            ToolViewKit.ShowStatus(_status, $"已导出到 {path}", StatusTone.Success);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            ToolViewKit.ShowStatus(_status, $"导出失败：{ex.Message}", StatusTone.Error);
        }
    }

    private void ClearAll()
    {
        _source.Text = string.Empty;
        _target.Text = string.Empty;
        ShowDuplicates(null, markDuplicate: false);
        ToolViewKit.ShowStatus(_status, "已清空，可粘贴新的姓名列表。");
    }
}
