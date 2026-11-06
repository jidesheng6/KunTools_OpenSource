namespace DocumentTools.Core.Services.Code;

/// <summary>一个归档子目录的落盘张数（含「异常-无法识别的图片」这类兜底目录）。</summary>
public sealed record ImageClassifyFolder(string FolderName, int ImageCount);

/// <summary>
/// 图片目录分类的共用文件规则（DataMatrix 页码分类与 OCR 文字分类共用）：
/// 后缀白名单、子目录展开、目标路径拼接、复制/移动投递。
/// 视图不得自行拼接这些路径——目录结构口径属于 Core。
/// </summary>
public static class ImageClassifyFiles
{
    /// <summary>legacy 只认这四个后缀；比较忽略大小写（legacy 用 <c>==</c>，<c>.JPG</c> 会被漏掉）。</summary>
    public static readonly string[] SupportedExtensions = [".jpg", ".jpeg", ".png", ".bmp"];

    /// <summary>按后缀白名单枚举图片，按文件名排序（并行分类时结果稳定可复现）。</summary>
    public static IReadOnlyList<string> EnumerateImages(string sourceRoot, bool includeSubdirectories)
        => Directory
            .EnumerateFiles(
                sourceRoot,
                "*",
                includeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(file => SupportedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToList();

    /// <summary>
    /// 目标目录：开启「包含子目录」时把源目录的相对层级保留在目标目录下。
    /// legacy 用 <c>Path.Combine(target, "\子目录", …)</c> 拼根路径会写到盘符根，这里改用相对路径。
    /// </summary>
    public static string ResolveDestinationDirectory(
        string sourceRoot,
        string targetRoot,
        string file,
        string folderName,
        bool includeSubdirectories)
    {
        if (!includeSubdirectories)
        {
            return Path.Combine(targetRoot, folderName);
        }

        string? fileDirectory = Path.GetDirectoryName(file);
        string relative = fileDirectory is null ? "." : Path.GetRelativePath(sourceRoot, fileDirectory);
        return relative is "." or ""
            ? Path.Combine(targetRoot, folderName)
            : Path.Combine(targetRoot, relative, folderName);
    }

    /// <summary>复制或移动（<paramref name="keepSourceFiles"/> = legacy「保留源目录内文件」）。</summary>
    public static void Deliver(string sourceFile, string destination, bool keepSourceFiles)
    {
        if (keepSourceFiles)
        {
            File.Copy(sourceFile, destination, overwrite: true);
        }
        else
        {
            File.Move(sourceFile, destination, overwrite: true);
        }
    }

    /// <summary>目录显示名取「相对目标根」的一段，嵌套子目录也能区分开。</summary>
    public static string DescribeFolder(string targetRoot, string destinationDirectory)
        => Path.GetRelativePath(targetRoot, destinationDirectory);
}
