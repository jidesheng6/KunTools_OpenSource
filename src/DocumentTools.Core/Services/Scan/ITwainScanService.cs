namespace DocumentTools.Core.Services.Scan;

public sealed record TwainSourceInfo(string Id, string Name, string? ProductFamily = null);

public sealed class TwainScanOptions
{
    public string OutputDirectory { get; set; } = string.Empty;
    public int Dpi { get; set; } = 200;
    public bool Duplex { get; set; }
    public bool ShowUi { get; set; }
    public bool SkipBlankPages { get; set; }
}

public interface ITwainScanService
{
    bool IsAvailable { get; }

    string UnavailableReason { get; }

    IReadOnlyList<TwainSourceInfo> ListSources();

    /// <summary>
    /// 用 NTwain 打开指定数据源并扫描，返回落盘的图片路径。
    /// 没有扫描仪 / 没有 DSM 时抛 <see cref="NotSupportedException"/>，消息取 <see cref="UnavailableReason"/>（给用户可执行的处理建议）；
    /// 有数据源但一张都没扫到时返回空集合。
    /// </summary>
    IReadOnlyList<string> Scan(string sourceId, TwainScanOptions options);
}
