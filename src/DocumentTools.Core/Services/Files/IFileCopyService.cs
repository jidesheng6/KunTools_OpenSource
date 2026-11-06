using System.IO;

namespace DocumentTools.Core.Services.Files;

/// <summary>一次「文件副本生成」的结果。</summary>
public sealed record FileCopyResult(
    int Created,
    int FirstIndex,
    int LastIndex,
    IReadOnlyList<string> Paths)
{
    public string Summary => Created == 0
        ? "没有生成任何副本。"
        : Created == 1
            ? $"已生成 1 个副本：{Path.GetFileName(Paths[0])}"
            : $"已生成 {Created} 个副本（序号 {FirstIndex}–{LastIndex}）。";
}

public interface IFileCopyService
{
    /// <summary>单个副本文件名：{源名} - {后缀} ({序号}){扩展名}。序号按 <paramref name="prefixDigits"/> 零填充。</summary>
    string BuildCopyName(string sourceFile, string namerSuffix, int index, int prefixDigits);

    /// <summary>界面预览：在 <paramref name="targetDirectory"/> 现有副本之后，下一个可用的副本名（序号连续）。</summary>
    string PreviewName(string sourceFile, string targetDirectory, string namerSuffix, int prefixDigits);

    /// <summary>把 <paramref name="sourceFile"/> 复制 <paramref name="count"/> 份到目标目录，序号接着目录里已有的副本往下排。</summary>
    FileCopyResult Generate(string sourceFile, string targetDirectory, int count, string namerSuffix, int prefixDigits);
}
