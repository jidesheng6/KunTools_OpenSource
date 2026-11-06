using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DocumentTools.App.Ui;

/// <summary>
/// 路径类输入框的统一政策：只读、可聚焦、可复制，值只由「选择…」按钮或拖放写入。
/// </summary>
/// <remarks>
/// 不要求用户手打绝对路径：Windows 路径里的斜杠方向、盘符、空格转义都是错误来源，
/// 而这些框的值最终会原样交给 Aspose / 进程启动参数，写错一半只会得到底层异常。
/// 也别用 <c>IsEnabled = false</c>——那样连选中复制都做不到，看起来像控件坏了；
/// 只读 + 可聚焦既禁止误编辑，又保留「我能把这段路径复制出去」的能力。
/// </remarks>
public static class PathInputs
{
    /// <summary>文件路径框（只读、可聚焦、可拖放）。</summary>
    public static TextBox ForFile(string placeholder)
    {
        TextBox box = Configure(new TextBox { PlaceholderText = placeholder });
        box.ToolTip = "只读：点右侧「浏览…」选择文件，或把文件直接拖到这里。";
        return box;
    }

    /// <summary>目录路径框（只读、可聚焦、可拖放）。</summary>
    public static TextBox ForFolder(string placeholder)
    {
        TextBox box = Configure(new TextBox { PlaceholderText = placeholder });
        box.ToolTip = "只读：点右侧「浏览…」选择目录，或把目录直接拖到这里。";
        return box;
    }

    /// <summary>路径失效时给用户看的话：说清是哪一条、下一步怎么做。</summary>
    public static string MissingFileNotice(string path) => $"文件已不存在：{path}。请重新选择。";

    /// <summary>目录失效时的同一句式（用户不用区分「文件」还是「目录」的技术差异）。</summary>
    public static string MissingFolderNotice(string path) => $"目录已不存在：{path}。请重新选择。";

    /// <summary>文件路径是否已经指不到东西（空值不算失效，空值交给必填校验）。</summary>
    public static bool IsMissingFile(string? path)
        => !string.IsNullOrWhiteSpace(path) && !File.Exists(path.Trim());

    /// <summary>目录路径是否已经指不到东西（空值不算失效）。</summary>
    public static bool IsMissingFolder(string? path)
        => !string.IsNullOrWhiteSpace(path) && !Directory.Exists(path.Trim());

    private static TextBox Configure(TextBox box)
    {
        box.IsReadOnly = true;
        // 只读仍然要能 Tab 进、能 Ctrl+A / Ctrl+C，否则用户没法核对路径
        box.IsTabStop = true;
        box.MinWidth = UiTokens.PathBoxMinWidth;
        box.HorizontalAlignment = HorizontalAlignment.Stretch;
        box.Cursor = Cursors.IBeam;
        box.TextWrapping = TextWrapping.NoWrap;
        box.AllowDrop = true;
        box.Drop += OnDrop;
        box.DragOver += OnDragOver;
        return box;
    }

    private static void OnDragOver(object sender, DragEventArgs e)
    {
        bool acceptable = e.Data.GetDataPresent(DataFormats.FileDrop);
        e.Effects = acceptable ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private static void OnDrop(object sender, DragEventArgs e)
    {
        if (sender is not TextBox box || !e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] items || items.Length == 0)
        {
            return;
        }

        // 拖多个也只取第一个：这些字段都是单选语义，悄悄拼成一串只会让后面的校验更难看懂
        box.Text = items[0];
        e.Handled = true;
    }
}
