using System.IO;
using System.Text;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 把逐页结果落成两份机器可读的 CSV（UTF-8 带 BOM，Excel 可直接打开），
/// 报告 `docs/qa-report-ui.md` 由它们派生，避免手抄数字。
/// </summary>
internal static class QaReportWriter
{
    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    /// <summary>
    /// 单页/子集运行（--only、--pages、--interact）不许覆盖权威 CSV：
    /// 报告里的「47 行」必须只能由全量运行改写，否则取证数字会被别人的一次单页调试悄悄换掉。
    /// </summary>
    private static string FileName(string name, bool partial) => partial ? "partial-" + name : name;

    internal static void WriteIndex(string outputDirectory, IReadOnlyList<PageSnapshot> pages, bool partial = false)
    {
        var builder = new StringBuilder();
        builder.AppendLine("序号,分类,功能ID,标题,截图,字节,缩放,元素数,文本块,按钮,Tab可达,首个输入控件,High,Medium,Low,致命错误");

        foreach (PageSnapshot page in pages)
        {
            builder.Append(page.Index).Append(',');
            builder.Append(Field(page.Feature.Category.ToString())).Append(',');
            builder.Append(Field(page.Feature.Id)).Append(',');
            builder.Append(Field(page.Feature.Title)).Append(',');
            builder.Append(Field(page.ScreenshotFile)).Append(',');
            builder.Append(page.PngBytes).Append(',');
            builder.Append(page.PngScale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(',');
            builder.Append(page.ElementCount).Append(',');
            builder.Append(page.TextBlockCount).Append(',');
            builder.Append(page.ButtonCount).Append(',');
            builder.Append(page.TabStopCount).Append(',');
            builder.Append(Field(page.FirstInput)).Append(',');
            builder.Append(page.CountOf(Severity.High)).Append(',');
            builder.Append(page.CountOf(Severity.Medium)).Append(',');
            builder.Append(page.CountOf(Severity.Low)).Append(',');
            builder.AppendLine(Field(page.FatalError));
        }

        File.WriteAllText(Path.Combine(outputDirectory, FileName("_index.csv", partial)), builder.ToString(), Utf8WithBom);
    }

    internal static void WriteIssues(string outputDirectory, IReadOnlyList<PageSnapshot> pages, bool partial = false)
    {
        var builder = new StringBuilder();
        builder.AppendLine("序号,分类,功能ID,严重度,检查项,控件,说明");

        foreach (PageSnapshot page in pages)
        {
            foreach (AuditIssue issue in page.Issues.OrderBy(i => i.Severity))
            {
                builder.Append(page.Index).Append(',');
                builder.Append(Field(page.Feature.Category.ToString())).Append(',');
                builder.Append(Field(page.Feature.Id)).Append(',');
                builder.Append(Field(issue.Severity.ToString())).Append(',');
                builder.Append(Field(issue.Kind)).Append(',');
                builder.Append(Field(issue.Element)).Append(',');
                builder.AppendLine(Field(issue.Detail));
            }

            if (page.FatalError is not null)
            {
                builder.Append(page.Index).Append(',');
                builder.Append(Field(page.Feature.Category.ToString())).Append(',');
                builder.Append(Field(page.Feature.Id)).Append(',');
                builder.Append(Field(Severity.High.ToString())).Append(',');
                builder.Append(Field("页面不可用")).Append(',');
                builder.Append(Field("-")).Append(',');
                builder.AppendLine(Field(page.FatalError));
            }
        }

        File.WriteAllText(Path.Combine(outputDirectory, FileName("_issues.csv", partial)), builder.ToString(), Utf8WithBom);
    }

    /// <summary>高 DPI 快照：同一页在 120 / 144 DPI 下的栅格结果与检查计数。</summary>
    internal static void WriteHidpi(string outputDirectory, IReadOnlyList<PageSnapshot> pages)
    {
        var builder = new StringBuilder();
        builder.AppendLine("功能ID,DPI,像素尺寸,截图,字节,缩放,High,Medium,Low,致命错误");

        foreach (PageSnapshot page in pages)
        {
            int pixelWidth = (int)System.Math.Round(SnapshotRunner.PageWidth * page.Dpi / 96.0);
            int pixelHeight = (int)System.Math.Round(SnapshotRunner.PageHeight * page.Dpi / 96.0);

            builder.Append(Field(page.Feature.Id)).Append(',');
            builder.Append(page.Dpi).Append(',');
            builder.Append(Field(pixelWidth + "x" + pixelHeight)).Append(',');
            builder.Append(Field(page.ScreenshotFile)).Append(',');
            builder.Append(page.PngBytes).Append(',');
            builder.Append(page.PngScale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)).Append(',');
            builder.Append(page.CountOf(Severity.High)).Append(',');
            builder.Append(page.CountOf(Severity.Medium)).Append(',');
            builder.Append(page.CountOf(Severity.Low)).Append(',');
            builder.AppendLine(Field(page.FatalError));
        }

        File.WriteAllText(Path.Combine(outputDirectory, "_hidpi.csv"), builder.ToString(), Utf8WithBom);
    }

    /// <summary>高 DPI 的问题明细：按 DPI 单列，便于回答"某个 DPI 下多出哪几条"。</summary>
    internal static void WriteHidpiIssues(string outputDirectory, IReadOnlyList<PageSnapshot> pages)
    {
        var builder = new StringBuilder();
        builder.AppendLine("功能ID,DPI,严重度,检查项,控件,说明,截图");

        foreach (PageSnapshot page in pages)
        {
            foreach (AuditIssue issue in page.Issues)
            {
                builder.Append(Field(page.Feature.Id)).Append(',');
                builder.Append(page.Dpi).Append(',');
                builder.Append(Field(issue.Severity.ToString())).Append(',');
                builder.Append(Field(issue.Kind)).Append(',');
                builder.Append(Field(issue.Element)).Append(',');
                builder.Append(Field(issue.Detail)).Append(',');
                builder.AppendLine(Field(page.ScreenshotFile));
            }
        }

        File.WriteAllText(Path.Combine(outputDirectory, "_hidpi-issues.csv"), builder.ToString(), Utf8WithBom);
    }

    /// <summary>交互态结果：每个页面的点击结果、新增状态行与截图。</summary>
    internal static void WriteInteract(string outputDirectory, IReadOnlyList<InteractResult> results, bool partial = false)
    {
        var builder = new StringBuilder();
        builder.AppendLine("功能ID,结果,退出码,超时,状态行数,截图,状态行文本,痕迹");

        foreach (InteractResult result in results)
        {
            builder.Append(Field(result.FeatureId)).Append(',');
            builder.Append(Field(result.Outcome)).Append(',');
            builder.Append(result.ExitCode).Append(',');
            builder.Append(Field(result.TimedOut ? "是" : "否")).Append(',');
            builder.Append(result.StatusLines).Append(',');
            builder.Append(Field(result.ScreenshotFile)).Append(',');
            builder.Append(Field(string.Join(" | ", result.Trace.Where(t => !t.Contains("click ", StringComparison.Ordinal) && !t.Contains("set ", StringComparison.Ordinal))))).Append(',');
            builder.AppendLine(Field(string.Join(" | ", result.Trace.Take(12))));
        }

        File.WriteAllText(Path.Combine(outputDirectory, "_interact.csv"), builder.ToString(), Utf8WithBom);
    }

    private static string Field(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
