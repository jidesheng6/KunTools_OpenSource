using System.Windows;
using System.Windows.Controls;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Features.Options;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Ui.Options;

/// <summary>
/// Renders Core <see cref="OptionSection"/> into WPF controls.
/// Knows nothing about feature business rules — only field kinds.
/// </summary>
public sealed class OptionPanelBuilder
{
    private readonly Dictionary<string, FrameworkElement> _controls = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDialogService? _dialogs;

    /// <param name="dialogs">
    /// 提供后 <see cref="OptionFieldKind.FilePath"/> 字段会多一个「浏览…」按钮；
    /// 不提供则退化为纯文本框（Schema 与业务语义不变）。
    /// </param>
    public OptionPanelBuilder(IDialogService? dialogs = null) => _dialogs = dialogs;

    public IReadOnlyDictionary<string, FrameworkElement> Controls => _controls;

    public void BuildInto(Panel host, IReadOnlyList<OptionSection> sections, IReadOnlyDictionary<string, string>? overrides = null)
    {
        host.Children.Clear();
        _controls.Clear();

        foreach (OptionSection section in sections)
        {
            var wrap = new WrapPanel();
            foreach (OptionField field in section.Fields)
            {
                string initial = ResolveInitial(field, overrides);
                switch (field.Kind)
                {
                    case OptionFieldKind.Check:
                        AddCheck(wrap, field.Key, field.Label, IsTrue(initial));
                        break;
                    case OptionFieldKind.Choice:
                        AddChoice(wrap, field.Key, field.Label, field.Choices ?? Array.Empty<OptionChoice>(), initial);
                        break;
                    case OptionFieldKind.FilePath:
                        AddFile(wrap, field, initial);
                        break;
                    default:
                        AddText(wrap, field.Key, field.Label, initial, field.TextWidth);
                        break;
                }
            }

            host.Children.Add(FeaturePageBase.SectionCard(section.Title, wrap));
        }
    }

    public Dictionary<string, string> Collect()
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, control) in _controls)
        {
            switch (control)
            {
                case CheckBox check:
                    options[key] = check.IsChecked == true ? "true" : "false";
                    break;
                case ComboBox combo:
                    options[key] = combo.SelectedValue?.ToString()
                                   ?? (combo.SelectedItem as ComboBoxItem)?.Tag?.ToString()
                                   ?? combo.SelectedItem?.ToString()
                                   ?? string.Empty;
                    break;
                case TextBox text:
                    options[key] = text.Text?.Trim() ?? string.Empty;
                    break;
            }
        }

        return options;
    }

    public bool GetBool(string key)
        => _controls.TryGetValue(key, out FrameworkElement? c) && c is CheckBox { IsChecked: true };

    private static string ResolveInitial(OptionField field, IReadOnlyDictionary<string, string>? overrides)
    {
        if (overrides is not null && overrides.TryGetValue(field.Key, out string? over) && over is not null)
        {
            return over;
        }

        return field.DefaultValue ?? string.Empty;
    }

    private static bool IsTrue(string? value)
        => string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);

    private void AddCheck(Panel parent, string key, string label, bool value)
    {
        var box = new CheckBox
        {
            Content = label,
            IsChecked = value,
            Margin = new Thickness(0, 4, 16, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        _controls[key] = box;
        parent.Children.Add(box);
    }

    private void AddChoice(
        Panel parent,
        string key,
        string label,
        IReadOnlyList<OptionChoice> choices,
        string selectedValue)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 4, 16, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        row.Children.Add(new TextBlock
        {
            Text = label,
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center
        });

        var combo = new ComboBox
        {
            MinWidth = 120,
            IsEditable = false,
            DisplayMemberPath = nameof(OptionChoice.Label),
            SelectedValuePath = nameof(OptionChoice.Value)
        };
        foreach (OptionChoice choice in choices)
        {
            combo.Items.Add(choice);
        }

        int index = 0;
        for (int i = 0; i < choices.Count; i++)
        {
            if (string.Equals(choices[i].Value, selectedValue, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }
        }

        if (choices.Count > 0)
        {
            combo.SelectedIndex = Math.Clamp(index, 0, choices.Count - 1);
        }

        _controls[key] = combo;
        row.Children.Add(combo);
        parent.Children.Add(row);
    }

    private void AddFile(Panel parent, OptionField field, string initial)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 4, 16, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        row.Children.Add(new TextBlock
        {
            Text = field.Label,
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center
        });

        // 路径一律只读（政策见 PathInputs / ToolViewKit.PathBox）：值只能由「浏览…」或拖放写入。
        // 手打路径的失败方式太多——斜杠方向、盘符大小写、空格没转义——而这些值会原样交给 Aspose
        // 或进程启动参数，写错一半只会得到底层英文异常，用户也看不出错在哪一段。
        // 占位文本沿用字段标签（如「水印图片路径」/「Chrome路径」），读屏与 QA 扫描都靠它判路径语义。
        TextBox box = ToolViewKit.PathBox(field.Label, field.Label, folder: false);
        // 水平 StackPanel 里 Stretch 不生效，必须给显式宽度；MinWidth=320 由 PathBox 保证，取两者较大值。
        box.Width = Math.Max(field.TextWidth, UiTokens.PathBoxMinWidth);
        box.Text = initial;
        _controls[field.Key] = box;
        row.Children.Add(box);

        var browse = new Button
        {
            Content = "浏览…",
            Margin = new Thickness(4, 0, 0, 0),
            Padding = new Thickness(10, 4, 10, 4),
            ToolTip = field.FileFilter ?? "所有文件|*.*",
            IsEnabled = _dialogs is not null
        };
        browse.Click += (_, _) =>
        {
            string? picked = _dialogs?.OpenFiles(field.FileFilter ?? "所有文件|*.*", multiSelect: false)
                .FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(picked))
            {
                box.Text = picked;
            }
        };
        row.Children.Add(browse);

        if (_dialogs is null)
        {
            // 没有对话框时按钮是灰的，只读框就只剩拖放一条路，得把这条唯一路径说清楚
            box.ToolTip = "当前环境没有选择对话框：把文件直接拖到这个框里。";
        }

        parent.Children.Add(row);
    }

    /// <summary>低于这个宽度的数值输入框连「1000」都显示不全，Schema 里的 60 是给表格用的。</summary>
    private const double MinTextWidth = 70;

    private void AddText(Panel parent, string key, string label, string initial, double width)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 4, 16, 4),
            VerticalAlignment = VerticalAlignment.Center
        };
        row.Children.Add(new TextBlock
        {
            Text = label,
            Margin = new Thickness(0, 0, 6, 0),
            VerticalAlignment = VerticalAlignment.Center
        });
        var box = new TextBox
        {
            Text = initial,
            // Schema 里 width 只描述「它需要多宽」，最小可用宽度由面板兜底：
            // 60px 的框连 4 位数都露不全，选项面板不负责窄表格那种密度。
            Width = Math.Max(width, MinTextWidth),
            TextAlignment = TextAlignment.Right
        };
        _controls[key] = box;
        row.Children.Add(box);
        parent.Children.Add(row);
    }
}
