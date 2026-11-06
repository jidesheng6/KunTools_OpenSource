using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace DocumentTools.Core.Services.Office;

/// <summary>通过 PuppeteerSharp 调用本机 Chromium/Edge 渲染 HTML → PDF。</summary>
public sealed class HtmlToPdfService : IHtmlToPdfService
{
    public async Task ConvertAsync(
        string source,
        string savedFileName,
        HtmlToPdfOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ArgumentException("HTML 源为空。", nameof(source));
        }

        if (string.IsNullOrWhiteSpace(savedFileName))
        {
            throw new ArgumentException("保存路径为空。", nameof(savedFileName));
        }

        if (string.IsNullOrWhiteSpace(options.BrowserExecutablePath)
            || !File.Exists(options.BrowserExecutablePath))
        {
            // 只报路径用户不知道下一步做什么，这里补上「装什么 / 在哪个选项里改」。
            string current = string.IsNullOrWhiteSpace(options.BrowserExecutablePath)
                ? "「浏览器路径」(chromePath) 为空"
                : "当前指向：" + options.BrowserExecutablePath;
            throw new InvalidOperationException(
                "转换 HTML 失败：未找到可用的 Chrome/Edge 可执行文件（"
                + current
                + "）。请安装 Chrome 或 Edge，或在选项里把「浏览器路径」(chromePath) 指向 chrome.exe。");
        }

        string? dir = Path.GetDirectoryName(Path.GetFullPath(savedFileName));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var launch = new LaunchOptions
        {
            Headless = true,
            AcceptInsecureCerts = true,
            ExecutablePath = options.BrowserExecutablePath,
        };

        await using IBrowser browser = await Puppeteer.LaunchAsync(launch).ConfigureAwait(false);
        await using IPage page = await browser.NewPageAsync().ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        if (!options.WaitAllResources)
        {
            page.DefaultTimeout = options.DefaultTimeoutSeconds * 1000;
            page.DefaultNavigationTimeout = options.NavigationTimeoutSeconds * 1000;
        }

        string navigateUrl = ToNavigateUrl(source);
        await page.GoToAsync(navigateUrl).ConfigureAwait(false);

        var pdf = new PdfOptions
        {
            Format = ToPaperFormat(options.PaperSizeIndex),
            DisplayHeaderFooter = false,
            Landscape = options.Landscape,
            PrintBackground = true,
            PreferCSSPageSize = options.PreferCssPageSize,
        };

        if (!options.PreferCssPageSize)
        {
            pdf.MarginOptions = new MarginOptions
            {
                Top = options.MarginTopPx + "px",
                Left = options.MarginLeftPx + "px",
                Bottom = options.MarginBottomPx + "px",
                Right = options.MarginRightPx + "px",
            };
        }

        await page.PdfAsync(savedFileName, pdf).ConfigureAwait(false);
    }

    private static string ToNavigateUrl(string source)
    {
        if (source.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || source.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
        {
            return source;
        }

        return new Uri(Path.GetFullPath(source)).AbsoluteUri;
    }

    private static PaperFormat ToPaperFormat(int index)
        => index switch
        {
            0 => PaperFormat.A0,
            1 => PaperFormat.A1,
            2 => PaperFormat.A2,
            3 => PaperFormat.A3,
            4 => PaperFormat.A4,
            5 => PaperFormat.A5,
            6 => PaperFormat.A6,
            7 => PaperFormat.Ledger,
            8 => PaperFormat.Legal,
            9 => PaperFormat.Letter,
            10 => PaperFormat.Tabloid,
            _ => PaperFormat.A4
        };
}
