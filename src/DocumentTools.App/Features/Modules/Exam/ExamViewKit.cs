using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui.Controls;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// 考评页构件入口：**薄适配层**，实现全在 <see cref="ToolViewKit"/>（构件单源，见 docs/architecture.md）。
/// 只保留考评页既有的调用签名：标签列默认 96px、ShowStatus 用 bool 表示错误，
/// 这样 8 个考评视图不需要改任何一行。
/// </summary>
internal static class ExamViewKit
{
    private const double DefaultLabelWidth = 96;

    /// <summary>
    /// 考评库/选票库的打开对话框过滤器。扩展名清单是「哪些文件算 Access 库」的口径，
    /// 4 个视图原先各自写了一份，改一处就会漏；统一放在构件层（纯 UI 关注点，不进 Core）。
    /// </summary>
    public const string AccessDatabaseFilter = "Access 数据库|*.mdb;*.accdb|所有文件|*.*";

    /// <summary>MSS3 选票模板（legacy「MSS3文件」对话框只认 <c>*.mss3</c>，不是 .mdb）。</summary>
    public const string Mss3Filter = "MSS3 选票模板|*.mss3|所有文件|*.*";

    public static Border Card(string title, params UIElement[] children)
        => ToolViewKit.Card(title, children);

    /// <summary>标签 + 拉伸控件 + 可选尾部按钮（如「选择…」）。</summary>
    public static Grid Field(
        string label,
        UIElement control,
        UIElement? trailing = null,
        double labelWidth = DefaultLabelWidth,
        bool stretchContent = true)
        => ToolViewKit.Field(label, control, trailing, labelWidth, stretchContent);

    /// <summary>
    /// 多行输入框专用：标签贴顶而不是垂直居中。ToolViewKit.Field 的标签居中对齐，
    /// 配 140px 高的文本框时「姓名」会飘在框的中线上（见 exam.vote 快照）。
    /// </summary>
    public static Grid FieldTop(string label, UIElement control, double labelWidth = DefaultLabelWidth)
    {
        var grid = new Grid { Margin = DocumentTools.App.Ui.UiTokens.RowGap };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var caption = new TextBlock
        {
            Text = label,
            MinWidth = labelWidth,
            Foreground = DocumentTools.App.Ui.UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Top,
            // 让标签首行与文本框首行文本基线对齐（WPF-UI TextBox 上边距约 4px）。
            Margin = new Thickness(0, DocumentTools.App.Ui.UiTokens.SpaceXs, DocumentTools.App.Ui.UiTokens.SpaceSm, 0)
        };
        grid.Children.Add(caption);
        Grid.SetColumn(control, 1);
        grid.Children.Add(control);
        return grid;
    }

    public static FrameworkElement Inline(params UIElement[] items)
        => ToolViewKit.Row(items);

    public static Border ListFrame(UIElement child, double minHeight = 200)
        => ToolViewKit.Frame(child, minHeight);

    /// <summary>空态框：没有内容时给一句可执行的下一步提示（未选库 / 列表为空）。</summary>
    public static Border Placeholder(string text, double minHeight = 160)
        => ToolViewKit.Placeholder(text, minHeight);

    /// <summary>在「空态提示」与「真实内容」之间切换同一个框。</summary>
    public static void ShowPlaceholder(Border frame, UIElement? content, string emptyText)
        => ToolViewKit.ShowPlaceholder(frame, content, emptyText);

    /// <summary>次级说明文字（规则说明、后果提醒），放在卡片内而不是弹窗。</summary>
    public static TextBlock Hint(string text) => ToolViewKit.Hint(text);

    public static TextBlock Status(string text)
        => ToolViewKit.Status(text);

    public static void ShowStatus(TextBlock target, string text, bool error = false)
        => ToolViewKit.ShowStatus(target, text, error ? StatusTone.Error : StatusTone.Neutral);

    /// <summary>按语气着色：成功用绿色、进行中用蓝色，避免“完成”和“失败”同一个灰色。</summary>
    public static void ShowStatus(TextBlock target, string text, StatusTone tone)
        => ToolViewKit.ShowStatus(target, text, tone);

    public static Button Primary(string text, Action onClick)
        => ToolViewKit.Primary(text, onClick);

    public static Button Secondary(string text, Action onClick)
        => ToolViewKit.Secondary(text, onClick);

    public static WrapPanel ActionBar(params UIElement[] elements)
        => ToolViewKit.ActionBar(elements);

    public static string Value(TextBox box) => ToolViewKit.Value(box);

    public static void PickFile(IDialogService dialogs, TextBox target, string filter)
        => ToolViewKit.PickFile(dialogs, target, filter);

    public static void PickFolder(IDialogService dialogs, TextBox target)
        => ToolViewKit.PickFolder(dialogs, target);

    /// <summary>
    /// 路径框：只读 + 可聚焦 + 可拖放 + 最小宽 320（政策实现见 <c>Ui\PathInputs.cs</c>，验收见
    /// <c>tools\QaFunction</c> 的 <c>ui.policy.pathbox</c>）。考评页 6 个视图都用这一处，
    /// 免得有人回头再 <c>new TextBox { PlaceholderText = "…路径" }</c> 把可编辑框写回去。
    /// </summary>
    public static TextBox PathBox(string automationName, string placeholder, bool folder = false)
        => ToolViewKit.PathBox(automationName, placeholder, folder);

    /// <summary>Vertically scrollable stack of cards — the standard content area for form pages.</summary>
    public static ScrollViewer FormArea(params UIElement[] blocks)
        => ToolViewKit.FormArea(blocks);

    /// <summary>
    /// 在资源管理器里打开目录。纯 UI 导航（没有任何业务语义），
    /// 放到这里是因为考评页有 3 个视图要「打开结果目录 / 数据库所在目录」，
    /// 各处复制一份 Process.Start 只会让壳层细节散开。
    /// </summary>
    public static void OpenFolder(string? folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    /// <summary>打开文件所在目录（例如「插入完成，是否打开数据库所在目录？」）。</summary>
    public static void RevealFile(string? path)
        => OpenFolder(Path.GetDirectoryName(path));

    /// <summary>用系统默认程序打开一个已生成的文件（壳层动作，不涉及任何业务判断）。</summary>
    public static void OpenFile(string? file)
    {
        if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = file,
            UseShellExecute = true
        });
    }
}
