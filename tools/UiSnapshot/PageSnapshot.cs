using System.Globalization;
using System.IO;
using System.Text;
using DocumentTools.Core.Features;

namespace DocumentTools.UiSnapshot;

/// <summary>问题严重度：High = 用户直接可见的功能/可读性缺陷；Medium = 交互或可读性明显下降；Low = 建议优化。</summary>
internal enum Severity
{
    High = 0,
    Medium = 1,
    Low = 2
}

/// <summary>一条机器可判定的检查结论（来自视觉树遍历，不含人工目视）。</summary>
internal sealed class AuditIssue
{
    public AuditIssue(Severity severity, string kind, string element, string detail)
    {
        Severity = severity;
        Kind = kind;
        Element = element;
        Detail = detail;
    }

    public Severity Severity { get; }

    /// <summary>检查项名称，例如「超出页面视口」「空文本按钮」。</summary>
    public string Kind { get; }

    /// <summary>命中的控件描述（类型 + x:Name + 文本片段）。</summary>
    public string Element { get; }

    public string Detail { get; }
}

/// <summary>单页快照结果：截图文件信息 + 检查计数 + 问题列表。</summary>
internal sealed class PageSnapshot
{
    private const int MaxIssuesPerKind = 8;

    private readonly Dictionary<string, int> _countByKind = new(StringComparer.Ordinal);

    public PageSnapshot(int index, FeatureDescriptor feature, string screenshotPath)
    {
        Index = index;
        Feature = feature;
        ScreenshotPath = screenshotPath;
    }

    public int Index { get; }

    public FeatureDescriptor Feature { get; }

    public string ScreenshotPath { get; }

    /// <summary>相对仓库根的截图路径，用于报告索引列。</summary>
    public string ScreenshotFile { get; set; } = string.Empty;

    public long PngBytes { get; set; }

    public double PngScale { get; set; } = 1.0;

    /// <summary>渲染 DPI。WPF 布局以 DIP 为单位，DPI 只改变栅格化分辨率，不改布局尺寸。</summary>
    public int Dpi { get; set; } = 96;

    /// <summary>创建/布局/渲染阶段的致命失败；非空表示这一页没有可用截图。</summary>
    public string? FatalError { get; set; }

    public List<AuditIssue> Issues { get; } = new();

    public int ElementCount { get; set; }

    public int TextBlockCount { get; set; }

    public int ButtonCount { get; set; }

    public int TabStopCount { get; set; }

    public string? FirstInput { get; set; }

    public int CountOf(Severity severity) => Issues.Count(i => i.Severity == severity);

    /// <summary>同类问题只保留前若干条，其余折叠成一条汇总项，避免单页淹没报告。</summary>
    public void AddIssue(AuditIssue issue)
    {
        _countByKind.TryGetValue(issue.Kind, out int seen);
        _countByKind[issue.Kind] = seen + 1;

        if (seen < MaxIssuesPerKind)
        {
            Issues.Add(issue);
            return;
        }

        if (seen == MaxIssuesPerKind)
        {
            Issues.Add(new AuditIssue(Severity.Low, issue.Kind, "（已折叠）", "同类问题超过 " + MaxIssuesPerKind + " 条，后续不再逐条列出"));
        }
    }

    /// <summary>控制台行只输出 ASCII，中文标题与详情走 CSV / 报告，避免 Windows 控制台代码页把中文写成乱码。</summary>
    public string ToConsoleLine()
    {
        var builder = new StringBuilder();
        builder.Append("PAGE|");
        builder.Append(Index.ToString("000", CultureInfo.InvariantCulture)).Append('|');
        builder.Append(Feature.Category).Append('|');
        builder.Append(Feature.Id).Append('|');
        builder.Append(FatalError is null ? "ok" : "fatal").Append('|');
        builder.Append(Path.GetFileName(ScreenshotPath)).Append('|');
        builder.Append((PngBytes / 1024).ToString("0", CultureInfo.InvariantCulture)).Append("KB|");
        builder.Append(PngScale.ToString("0.00", CultureInfo.InvariantCulture)).Append('|');
        builder.Append("el=").Append(ElementCount.ToString(CultureInfo.InvariantCulture)).Append('|');
        builder.Append("btn=").Append(ButtonCount.ToString(CultureInfo.InvariantCulture)).Append('|');
        builder.Append("tab=").Append(TabStopCount.ToString(CultureInfo.InvariantCulture)).Append('|');
        builder.Append("input=").Append(FirstInput is null ? "no" : "yes").Append('|');
        builder.Append('H').Append(CountOf(Severity.High).ToString(CultureInfo.InvariantCulture));
        builder.Append("/M").Append(CountOf(Severity.Medium).ToString(CultureInfo.InvariantCulture));
        builder.Append("/L").Append(CountOf(Severity.Low).ToString(CultureInfo.InvariantCulture));
        return builder.ToString();
    }
}
