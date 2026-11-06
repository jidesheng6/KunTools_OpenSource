using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.Core.Services.Office;

namespace DocumentTools.App.Ui.Controls;

/// <summary>Wires legacy ListViewRightMenu callbacks onto <see cref="BorderedFileList"/>.</summary>
public static class FileListContextWiring
{
    public static void Attach(
        BorderedFileList list,
        IDialogService dialogs,
        IDocumentInfoService? documentInfo = null,
        IDictionary<string, string>? convertedBySource = null)
    {
        list.ShowInfo = msg => dialogs.Info(msg);
        if (documentInfo is not null)
        {
            list.GetDocumentInfo = documentInfo.GetInfo;
        }

        if (convertedBySource is not null)
        {
            list.ResolveConvertedPath = source =>
                convertedBySource.TryGetValue(source, out string? path) ? path : null;
        }
    }

    /// <summary>Record last batch outputs for “打开转换后文件/目录”.</summary>
    public static void RememberOutputs(
        BorderedFileList list,
        IDictionary<string, string> convertedBySource,
        IReadOnlyList<string> sources,
        IReadOnlyList<string> outputs,
        string outputDirectory)
    {
        convertedBySource.Clear();

        // 转换页现在是逐文件调用：中途失败会让「源[i] ↔ 产物[i]」错位，一个源产出多个文件时也会错位。
        // 按索引配对会把产物配给错误的源（点“打开转换后文件”会打开别人的文件），所以以文件名为锚点配对。
        var unmatched = new List<string>(outputs);
        foreach (string source in sources)
        {
            string stem = Path.GetFileNameWithoutExtension(source);
            string? hit = unmatched.FirstOrDefault(o =>
                    string.Equals(Path.GetFileNameWithoutExtension(o), stem, StringComparison.OrdinalIgnoreCase))
                ?? unmatched.FirstOrDefault(o =>
                    Path.GetFileNameWithoutExtension(o).StartsWith(stem, StringComparison.OrdinalIgnoreCase));

            if (hit is null)
            {
                continue;
            }

            convertedBySource[source] = hit;
            unmatched.Remove(hit);
        }

        // 多对一（如 pdf.merge / image.pdf 合并）只有一个产物：所有源都指向它。
        if (outputs.Count == 1)
        {
            foreach (string source in sources)
            {
                convertedBySource[source] = outputs[0];
            }
        }
        else if (convertedBySource.Count == 0 && sources.Count == outputs.Count)
        {
            // 产物被改成与源无关的固定名时的最后兜底：数量一致才允许按顺序配对。
            for (int i = 0; i < sources.Count; i++)
            {
                convertedBySource[sources[i]] = outputs[i];
            }
        }

        list.LastOutputDirectory = outputDirectory;
        list.ResolveConvertedPath = source =>
            convertedBySource.TryGetValue(source, out string? path) ? path : null;
    }
}
