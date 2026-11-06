using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>Maps html.pdf.</summary>
public sealed class HtmlToPdfOperation : IDocumentOperation
{
    private readonly IHtmlToPdfService _html;

    public HtmlToPdfOperation(IHtmlToPdfService html)
    {
        FeatureId = "html.pdf";
        _html = html;
    }

    public string FeatureId { get; }

    public async Task<DocumentOperationResult> ExecuteAsync(
        DocumentOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.InputFiles.Count == 0)
        {
            return DocumentOperationResult.Fail("请先选择 HTML 文件或填入 URL。");
        }

        string? browser = GetOption(request, "chromePath")
                          ?? GetOption(request, "browserPath");

        if (string.IsNullOrWhiteSpace(browser))
        {
            browser = TryDetectBrowser();
        }

        int paperIndex = MapPaper(GetOption(request, "paper") ?? GetOption(request, "paperSize"));
        var options = new HtmlToPdfOptions
        {
            BrowserExecutablePath = browser ?? string.Empty,
            PaperSizeIndex = paperIndex,
            Landscape = ParseBool(GetOption(request, "landscape")),
            PreferCssPageSize = ParseBool(GetOption(request, "useCss"))
                               || ParseBool(GetOption(request, "preferCss")),
            WaitAllResources = ParseBool(GetOption(request, "waitResources")),
            // 缺省值与 dev 面板一致：四边距 50px、不用内部 CSS、不等待全部资源。
            // 超时保持 30s（dev 是 3s/5s，慢页面直接失败，属 dev 的可靠性缺陷，不复刻）。
            MarginTopPx = GetOption(request, "marginTop") ?? GetOption(request, "marginTopPx") ?? "50",
            MarginLeftPx = GetOption(request, "marginLeft") ?? GetOption(request, "marginLeftPx") ?? "50",
            MarginBottomPx = GetOption(request, "marginBottom") ?? GetOption(request, "marginBottomPx") ?? "50",
            MarginRightPx = GetOption(request, "marginRight") ?? GetOption(request, "marginRightPx") ?? "50",
            DefaultTimeoutSeconds = ParseInt(GetOption(request, "timeout"), 30),
            NavigationTimeoutSeconds = ParseInt(GetOption(request, "navTimeout"), 30)
        };

        try
        {
            Directory.CreateDirectory(request.OutputDirectory);
            var outputs = new List<string>();

            foreach (string input in request.InputFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string name = Path.GetFileNameWithoutExtension(input);
                if (string.IsNullOrWhiteSpace(name)
                    || input.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    name = "page_" + outputs.Count;
                }

                string output = Path.Combine(request.OutputDirectory, name + ".pdf");
                await _html.ConvertAsync(input, output, options, cancellationToken).ConfigureAwait(false);
                outputs.Add(output);
            }

            return DocumentOperationResult.Ok($"完成 {outputs.Count} 个文件。", outputs);
        }
        catch (Exception ex)
        {
            return DocumentOperationResult.Fail(ex.Message);
        }
    }

    private static string? TryDetectBrowser()
    {
        string[] candidates =
        [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Google", "Chrome", "Application", "chrome.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "Microsoft", "Edge", "Application", "msedge.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Microsoft", "Edge", "Application", "msedge.exe"),
        ];

        return candidates.FirstOrDefault(File.Exists);
    }

    private static string? GetOption(DocumentOperationRequest request, string key)
    {
        if (request.Options is null)
        {
            return null;
        }

        return request.Options.TryGetValue(key, out string? value) ? value : null;
    }

    private static bool ParseBool(string? text)
        => string.Equals(text, "1", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase)
           || string.Equals(text, "yes", StringComparison.OrdinalIgnoreCase);

    private static int ParseInt(string? text, int fallback)
        => int.TryParse(text, out int n) ? n : fallback;

    /// <summary>
    /// 纸张 → HtmlToPdfOptions 索引（legacy `Util.cs:1724-1738` 的 0..10 顺序：
    /// A0/A1/A2/A3/A4/A5/A6/Ledger/Legal/Letter/Tabloid），Schema 的 Value 即数字串。
    /// </summary>
    private static int MapPaper(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return 4;
        }

        if (int.TryParse(label, out int index))
        {
            return index is >= 0 and <= 10 ? index : 4;
        }

        return label.Trim().ToUpperInvariant() switch
        {
            "A0" => 0,
            "A1" => 1,
            "A2" => 2,
            "A3" => 3,
            "A4" => 4,
            "A5" => 5,
            "A6" => 6,
            "LEDGER" => 7,
            "LEGAL" => 8,
            "LETTER" => 9,
            "TABLOID" => 10,
            _ => 4
        };
    }
}
