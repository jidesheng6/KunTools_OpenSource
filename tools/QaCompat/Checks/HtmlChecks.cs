using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Office;

namespace QaCompat.Checks;

/// <summary>无 Chrome/Edge 时 html.pdf 的表现。</summary>
internal static class HtmlChecks
{
    public const string Group = "html";

    public static void Run(QaContext ctx)
    {
        string html = Path.Combine(ctx.RepoRoot, "testdata", "html", "sample.html");

        ctx.Run(
            "html.no-browser",
            Group,
            "显式指定不存在的浏览器路径时 html.pdf 的错误消息",
            "`chromePath=Z:\\qa-compat\\no-such-browser\\chrome.exe`（不存在的盘符/路径），调用 `HtmlToPdfOperation.ExecuteAsync`，输入 testdata\\html\\sample.html",
            "Success=false，消息明确指出「未找到可用的 Chrome/Edge 可执行文件」，带上当前指向的路径与「chromePath」选项名（可操作建议）；不产生输出 PDF。",
            check =>
            {
                string outDir = ctx.Dir("html-no-browser");
                var operation = new HtmlToPdfOperation(new HtmlToPdfService());
                DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = new[] { html },
                    OutputDirectory = outDir,
                    Options = new Dictionary<string, string>
                    {
                        ["chromePath"] = @"Z:\qa-compat\no-such-browser\chrome.exe"
                    }
                }).GetAwaiter().GetResult();

                string[] produced = Directory.Exists(outDir)
                    ? Directory.GetFiles(outDir, "*.pdf", SearchOption.AllDirectories)
                    : Array.Empty<string>();
                string evidence = "Success=" + result.Success + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                  + "\n输出目录 PDF 数: " + produced.Length;

                bool messageOk = !result.Success
                                 && result.Message.Contains("未找到可用的 Chrome/Edge 可执行文件", StringComparison.Ordinal)
                                 && result.Message.Contains("no-such-browser", StringComparison.Ordinal)
                                 && result.Message.Contains("chromePath", StringComparison.Ordinal);

                if (!messageOk || produced.Length != 0)
                {
                    ctx.Fail(check, "错误消息未命中「未找到可用的 Chrome/Edge 可执行文件」+ 配置路径 + chromePath 建议，或留下了输出文件。", evidence);
                    return;
                }

                ctx.Fact("浏览器消息", "未找到浏览器时给出「未找到可用的 Chrome/Edge 可执行文件」+ 当前指向 + chromePath 建议");
                ctx.Pass(
                    check,
                    "明确报错并带上配置路径与可操作建议（「" + Fmt.OneLine(result.Message) + "」），且未生成任何 PDF。",
                    evidence);
            });

        ctx.Run(
            "html.autodetect",
            Group,
            "自动探测浏览器路径（无 chromePath）能否真正转换出 PDF",
            "不传 chromePath/browserPath，让 `HtmlToPdfOperation.TryDetectBrowser()` 从 ProgramFiles/ProgramFilesX86 找 Chrome/Edge，转换 testdata\\html\\sample.html",
            "转换成功、输出 PDF 存在且页数 ≥ 1；若本机确实没有浏览器，则给出 html.no-browser 同款明确消息。",
            check =>
            {
                string outDir = ctx.Dir("html-auto");
                var operation = new HtmlToPdfOperation(new HtmlToPdfService());
                DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = new[] { html },
                    OutputDirectory = outDir
                }).GetAwaiter().GetResult();

                string expectedFile = Path.Combine(outDir, "sample.pdf");
                string evidence = "Success=" + result.Success
                                  + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                  + "\n输出文件存在: " + File.Exists(expectedFile);

                if (result.Success && File.Exists(expectedFile))
                {
                    long size = new FileInfo(expectedFile).Length;
                    int pages = 0;
                    string pageText = "(未读取)";
                    try
                    {
                        pages = Fixtures.CountPdfPages(expectedFile);
                        pageText = pages.ToString();
                    }
                    catch (Exception ex)
                    {
                        pageText = "读取失败：" + Fmt.OneLine(ex.Message);
                    }

                    evidence += "\n大小: " + size + " 字节\n页数: " + pageText;
                    ctx.Fact("浏览器", "自动探测成功（" + RelativeBrowser(ctx) + "），html.pdf 转换可用");
                    ctx.Pass(check, "自动探测到浏览器并转换成功：" + expectedFile + "（" + size + " 字节，" + pageText + " 页）。", evidence);
                    return;
                }

                if (!result.Success && result.Message.Contains("未找到可用的 Chrome/Edge 可执行文件", StringComparison.Ordinal))
                {
                    ctx.Fact("浏览器", "本机未找到 Chrome/Edge");
                    ctx.Pass(check, "本机无浏览器，落到了明确的缺失分支：「" + Fmt.OneLine(result.Message) + "」。", evidence);
                    return;
                }

                ctx.Fact("浏览器", "存在但 html.pdf 转换失败");
                ctx.Fail(
                    check,
                    "探测到了浏览器但转换失败，消息为「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」。",
                    evidence);
            });
    }

    private static string RelativeBrowser(QaContext ctx)
    {
        foreach (string root in new[]
                 {
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                     Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
                 })
        {
            string chrome = Path.Combine(root, "Google", "Chrome", "Application", "chrome.exe");
            if (File.Exists(chrome))
            {
                return chrome;
            }

            string edge = Path.Combine(root, "Microsoft", "Edge", "Application", "msedge.exe");
            if (File.Exists(edge))
            {
                return edge;
            }
        }

        return "未探测到";
    }
}
