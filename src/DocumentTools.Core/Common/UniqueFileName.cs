using System.Globalization;

namespace DocumentTools.Core.Common;

/// <summary>
/// 同目录避撞文件名规则：<c>考评表_fixed.png</c> → <c>考评表_fixed_1.png</c> → <c>考评表_fixed_2.png</c>。
/// 命名规则属文件业务知识（legacy 形体修正/旋转都按「原名+后缀」另存），视图只该把结果交给保存对话框；
/// 判重按文件名不区分大小写（Windows 语义，也避免同目录出现 <c>a_fixed.png</c> 与 <c>A_FIXED.PNG</c>）。
/// </summary>
public static class UniqueFileName
{
    /// <summary>
    /// 在 <paramref name="path"/> 所在目录里生成「原名 + <paramref name="suffix"/>」的可用文件名（含目录的完整路径）。
    /// 扩展名保留不变；同名已存在时依次尝试 <c>_1</c>、<c>_2</c> …
    /// </summary>
    public static string For(string path, string suffix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(suffix);

        string full = Path.GetFullPath(path);
        string directory = Path.GetDirectoryName(full)
                           ?? throw new ArgumentException("路径里没有目录部分。", nameof(path));
        string stem = Path.GetFileNameWithoutExtension(full);
        string extension = Path.GetExtension(full);
        string baseName = stem + suffix;

        HashSet<string> taken = ExistingNames(directory);
        string candidate = baseName + extension;
        for (int index = 1; taken.Contains(candidate); index++)
        {
            candidate = string.Concat(
                baseName, "_", index.ToString(CultureInfo.InvariantCulture), extension);
        }

        return Path.Combine(directory, candidate);
    }

    private static HashSet<string> ExistingNames(string directory)
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(directory))
        {
            return names;
        }

        foreach (string file in Directory.EnumerateFiles(directory))
        {
            names.Add(Path.GetFileName(file));
        }

        return names;
    }
}
