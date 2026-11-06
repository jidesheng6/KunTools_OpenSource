using System.Text;

namespace DocumentTools.Core.Common;

/// <summary>
/// Core 侧最薄的运行日志写入器：每次调用追加一行到 <c>logs\conversion.log</c>。
/// App 层已有 <c>AsposeBootstrap</c> / <c>CrashLog</c> 各自的日志，但 Core 不能引用 App
/// （分层约束），这里自带实现且不引任何新包。
/// 用途是「引擎归因」：同一份 PDF 由本机 Word 导出还是 Aspose 导出，事后只能靠这行日志区分。
/// 写日志永远不能影响转换结果，所以所有 IO 异常一律吞掉。
/// </summary>
public static class ConversionLog
{
    /// <summary>覆盖日志目录（自动化测试用）；不设时落在程序目录的 <c>logs</c>。</summary>
    public const string LogDirectoryVariable = "DOCUMENTTOOLS_LOG_DIR";

    private static readonly object Gate = new();

    public static string LogDirectory
    {
        get
        {
            string? overridden = Environment.GetEnvironmentVariable(LogDirectoryVariable);
            return string.IsNullOrWhiteSpace(overridden)
                ? Path.Combine(AppContext.BaseDirectory, "logs")
                : overridden;
        }
    }

    public static string LogPath => Path.Combine(LogDirectory, "conversion.log");

    /// <summary>按 <c>时间 [类别] 内容</c> 追加一行；类别用功能标识（如 <c>word.pdf</c>）。</summary>
    public static void Write(string category, string message)
    {
        // yyyy-MM-dd HH:mm:ss 里没有受区域设置影响的格式符，直接用内插字符串即可。
        string line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{category}] {message}{Environment.NewLine}";

        try
        {
            lock (Gate)
            {
                Directory.CreateDirectory(LogDirectory);
                File.AppendAllText(LogPath, line, Encoding.UTF8);
            }
        }
        catch (IOException)
        {
            // 日志写不进去不影响转换结果。
        }
        catch (UnauthorizedAccessException)
        {
            // 同上：目录不可写时静默放弃。
        }
    }
}
