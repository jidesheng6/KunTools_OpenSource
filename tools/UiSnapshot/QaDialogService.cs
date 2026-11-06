using System.Collections.Generic;
using System.IO;
using DocumentTools.App.Infrastructure.Dialogs;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 交互态快照专用的假弹窗服务：所有文件对话框直接返回固定测试路径，Info/Error/Confirm 只记录不阻塞。
/// 真实现走 MessageBox/OpenFileDialog（模态），在离屏 harness 里会把 Dispatcher 泵永久卡住，
/// 于是既拍不到截图，也会留下锁住 bin 的僵尸进程。
/// </summary>
internal sealed class QaDialogService : IDialogService
{
    /// <summary>每个子进程一个独立 tag，避免两个页面共用同一路径互相占用（会读到"文件被另一个进程使用"）。</summary>
    private readonly string _tag;

    /// <summary>测试用固定路径根目录：放在 %TEMP% 下，跑完由本工具自己清理，不污染仓库与盘根。</summary>
    internal static readonly string Root =
        Path.Combine(Path.GetTempPath(), "dtsnap-dialogs");

    internal QaDialogService(string tag)
    {
        _tag = tag;
        Directory.CreateDirectory(Root);

        // 清掉上一轮同名产物：残留文件被 Office 等外部进程打开时会锁住路径，
        // 让下一轮"生成失败：文件被另一个进程使用"，把工具缺陷伪装成业务缺陷。
        foreach (string stale in Directory.EnumerateFiles(Root, tag + "-*"))
        {
            try
            {
                System.IO.File.Delete(stale);
            }
            catch (IOException)
            {
                // 删不掉就让业务走它自己的错误分支，至少 trace 里留证。
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    /// <summary>对话框返回的文件路径（父目录存在，文件本身通常不存在，让业务走"文件不存在"分支）。</summary>
    internal string File => Path.Combine(Root, _tag + "-file.dat");

    /// <summary>对话框返回的文件夹路径。</summary>
    internal string Folder => Path.Combine(Root, _tag + "-folder");

    /// <summary>按调用顺序记录的弹窗调用，用于在 sidecar 里断言"点了按钮但没有弹模态框"。</summary>
    internal List<string> Calls { get; } = new();

    public IReadOnlyList<string> OpenFiles(string filter, bool multiSelect = true)
    {
        Calls.Add("OpenFiles(" + filter + ")");
        return [File];
    }

    public string? SaveFile(string filter, string? defaultName = null)
    {
        Calls.Add("SaveFile(" + filter + "," + (defaultName ?? "-") + ")");
        return Path.Combine(Root, _tag + "-" + (defaultName ?? "out.dat"));
    }

    public string? PickFolder()
    {
        Calls.Add("PickFolder()");
        Directory.CreateDirectory(Folder);
        return Folder;
    }

    public void Info(string message, string title = "提示") => Calls.Add("Info(" + title + "): " + message);

    public void Error(string message, string title = "错误") => Calls.Add("Error(" + title + "): " + message);

    public bool Confirm(string message, string title = "确认")
    {
        Calls.Add("Confirm(" + title + "): " + message);

        // 一律答"否"：真实流程里 Confirm 常是"已生成，是否打开文件？"这类分支，
        // 答"是"会让业务真的去 Shell 打开 Word/PDF，测试工具不该产生这种副作用。
        return false;
    }
}
