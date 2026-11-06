namespace DocumentTools.Core.Services.Office;

/// <summary>HTML 转 PDF 的渲染参数（Chromium / Edge 可执行文件）。</summary>
public sealed class HtmlToPdfOptions
{
    /// <summary>Chrome / Edge 可执行文件路径。</summary>
    public string BrowserExecutablePath { get; set; } = string.Empty;

    /// <summary>0=A0 … 4=A4 … 10=Tabloid，越界按 A4。</summary>
    public int PaperSizeIndex { get; set; } = 4;

    public bool Landscape { get; set; }

    public bool PreferCssPageSize { get; set; }

    public string MarginTopPx { get; set; } = "0";

    public string MarginLeftPx { get; set; } = "0";

    public string MarginBottomPx { get; set; } = "0";

    public string MarginRightPx { get; set; } = "0";

    public int DefaultTimeoutSeconds { get; set; } = 30;

    public int NavigationTimeoutSeconds { get; set; } = 30;

    public bool WaitAllResources { get; set; }
}

public interface IHtmlToPdfService
{
    Task ConvertAsync(
        string source,
        string savedFileName,
        HtmlToPdfOptions options,
        CancellationToken cancellationToken = default);
}
