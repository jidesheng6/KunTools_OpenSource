using System.Text;

namespace DocumentTools.Core.Settings;

/// <summary>
/// 设置存储后端。存在的意义有二：让单测用内存实现（不碰真实磁盘/用户配置），
/// 以及把所有 IO 异常收敛成「失败信息」而不是让调用点炸掉。
/// </summary>
public interface ISettingsStore
{
    /// <summary>存储位置的可读标识（文件路径或内存标识），用于日志与提示。</summary>
    string Location { get; }

    /// <summary>存储是否已存在（不存在时调用方按 legacy/默认值初始化）。</summary>
    bool Exists { get; }

    /// <summary>读取原始内容；失败时返回 false 并给出可直接展示的中文原因，不抛异常。</summary>
    bool TryRead(out string content, out string error);

    /// <summary>整体写入；失败时返回 false 并给出可直接展示的中文原因，不抛异常。</summary>
    bool TryWrite(string content, out string error);
}

/// <summary>
/// 单文件 JSON 存储：写入走「同目录临时文件 → 覆盖重命名」，避免写到一半断电留下半截配置。
/// </summary>
public sealed class JsonFileSettingsStore : ISettingsStore
{
    /// <summary>默认存储位置：%LOCALAPPDATA%\DocumentTools\settings.json。</summary>
    public static string DefaultLocation
        => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DocumentTools",
            "settings.json");

    private const string TempSuffix = ".tmp";

    /// <summary>指向默认位置的存储。</summary>
    public JsonFileSettingsStore()
        : this(DefaultLocation)
    {
    }

    /// <summary>指向指定文件的存储（相对路径按当前工作目录解析）。</summary>
    public JsonFileSettingsStore(string location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            throw new ArgumentException("设置存储路径不能为空。", nameof(location));
        }

        Location = Path.GetFullPath(location);
    }

    public string Location { get; }

    public bool Exists => File.Exists(Location);

    public bool TryRead(out string content, out string error)
    {
        content = string.Empty;
        error = string.Empty;

        try
        {
            if (!File.Exists(Location))
            {
                error = $"配置文件不存在：{Location}";
                return false;
            }

            // File.ReadAllText 自动识别并剥离 BOM；null 字节等损坏内容留给 JSON 解析报告
            content = File.ReadAllText(Location);
            return true;
        }
        catch (Exception ex) when (IsIoFailure(ex))
        {
            error = $"读取配置文件失败（{Location}）：{ex.Message}";
            return false;
        }
    }

    public bool TryWrite(string content, out string error)
    {
        error = string.Empty;
        string? tempPath = null;

        try
        {
            string? directory = Path.GetDirectoryName(Location);
            if (string.IsNullOrEmpty(directory))
            {
                error = $"配置文件路径无效：{Location}";
                return false;
            }

            Directory.CreateDirectory(directory);

            if (File.Exists(Location) && (File.GetAttributes(Location) & FileAttributes.ReadOnly) != 0)
            {
                error = $"配置文件是只读的，无法写入：{Location}（去掉只读属性后重试）";
                return false;
            }

            tempPath = Location + TempSuffix;
            using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(content);
            }

            File.Move(tempPath, Location, overwrite: true);
            tempPath = null;
            return true;
        }
        catch (Exception ex) when (IsIoFailure(ex))
        {
            error = $"写入配置文件失败（{Location}）：{ex.Message}";
            return false;
        }
        finally
        {
            if (tempPath is not null)
            {
                TryDelete(tempPath);
            }
        }
    }

    /// <summary>IO 类失败（权限、只读卷、路径不存在等）；其余异常照旧上抛，避免掩盖代码缺陷。</summary>
    internal static bool IsIoFailure(Exception ex)
        => ex is IOException
            or UnauthorizedAccessException
            or NotSupportedException
            or ArgumentException
            or System.Security.SecurityException;

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex) when (IsIoFailure(ex))
        {
            // 清理临时文件失败不影响写入结论，残留 .tmp 会在下次写入时被覆盖
        }
    }
}
