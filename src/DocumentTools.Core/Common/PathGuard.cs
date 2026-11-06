namespace DocumentTools.Core.Common;

/// <summary>Validates file paths before domain services touch the disk.</summary>
public static class PathGuard
{
    public static void RequirePath(string? path, string paramName)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("路径不能为空。", paramName);
        }
    }

    public static void RequireExistingFile(string? path, string paramName)
    {
        RequirePath(path, paramName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"找不到文件：{path}", path);
        }
    }

    public static void RequireExistingDirectory(string? path, string paramName)
    {
        RequirePath(path, paramName);

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"找不到目录：{path}");
        }
    }

    /// <summary>
    /// 可选附件（封面图、水印图等）：空或文件不存在时返回 null，由调用方回退默认行为，
    /// 不打断主任务 —— 与 legacy 一致（填错路径不该让整篇转换失败）。
    /// </summary>
    public static string? OptionalExistingFile(string? path)
        => string.IsNullOrWhiteSpace(path) || !File.Exists(path) ? null : path;

    /// <summary>
    /// 必填文件守卫：返回规范化后的绝对路径。空 → <see cref="ArgumentException"/>，
    /// 不存在 → <see cref="FileNotFoundException"/>，消息直接面向用户（含 <paramref name="label"/>）。
    /// 视图/调用方只负责把 <see cref="Exception.Message"/> 显示出来，不再各写一句中文提示。
    /// </summary>
    public static string RequireFile(string? path, string label = "文件")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"{label}未选择。", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"{label}不存在：{path}", path);
        }

        return Path.GetFullPath(path);
    }

    /// <summary>必填目录守卫：语义同 <see cref="RequireFile"/>，判据换成目录存在性。</summary>
    public static string RequireFolder(string? path, string label = "目录")
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException($"{label}未选择。", nameof(path));
        }

        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"{label}不存在：{path}");
        }

        return Path.GetFullPath(path);
    }

    /// <summary>
    /// 必填文件「还差什么」的短语：已就绪返回 null，否则返回可直接拼进状态行「还缺：…」的短语。
    /// 用于按钮使能/状态体检这类**不能抛异常**的场合（与 <see cref="RequireFile"/> 分工：
    /// 体检用本法，动作边界用守卫）。
    /// </summary>
    public static string? MissingFileReason(string? path, string label = "文件")
        => string.IsNullOrWhiteSpace(path) ? $"{label}未选择"
            : !File.Exists(path) ? $"{label}不存在"
            : null;

    /// <summary>目录版的 <see cref="MissingFileReason"/>。</summary>
    public static string? MissingFolderReason(string? path, string label = "目录")
        => string.IsNullOrWhiteSpace(path) ? $"{label}未选择"
            : !Directory.Exists(path) ? $"{label}不存在"
            : null;

    /// <summary>
    /// 两个路径是否指向同一位置：容忍相对路径、尾部斜杠、大小写差异（Windows 口径）。
    /// 比较规则是业务知识（例如「源数据包与目标数据包不能是同一个」），视图不得自己 <c>==</c> 全路径。
    /// </summary>
    public static bool SamePath(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);

        static string Normalize(string path)
        {
            string trimmed = path.Trim();
            string full;
            try
            {
                full = Path.GetFullPath(trimmed);
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return trimmed.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            return full.Length > 1
                ? full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                : full;
        }
    }
}
