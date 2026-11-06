using DocumentTools.Core.Licensing;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Office;

namespace QaCompat.Checks;

/// <summary>没有 .lic 许可证时 Aspose 的评估模式行为（重点：5 页以上合并/拆分是否静默截断）。</summary>
internal static class LicenseChecks
{
    public const string Group = "license";

    private const string EvaluationMarker = "Evaluation Only";

    public static void Run(QaContext ctx)
    {
        string licensesDir = Path.Combine(ctx.RepoRoot, "assets", "licenses");

        ctx.Run(
            "license.status",
            Group,
            "当前 Aspose 许可证状态（决定后两项的期望）",
            "调用 `AsposeLicenseLoader.TryLoad()`；同时枚举 assets\\licenses\\*.lic 与 ASPOSE_LICENSE_PATH",
            "仓库未附 .lic 且未设环境变量时返回 Evaluation（IsLicensed=false, LicensePath=null）。",
            check =>
            {
                AsposeLicenseStatus status = AsposeLicenseLoader.TryLoad();
                int licCount = Directory.Exists(licensesDir)
                    ? Directory.EnumerateFiles(licensesDir, "*.lic").Count()
                    : 0;
                string envPath = Environment.GetEnvironmentVariable("ASPOSE_LICENSE_PATH") ?? string.Empty;

                string evidence =
                    "IsLicensed=" + status.IsLicensed + "\n" +
                    "LicensePath=" + (status.LicensePath ?? "(none)") + "\n" +
                    "assets\\licenses\\*.lic 数量=" + licCount + "\n" +
                    "ASPOSE_LICENSE_PATH=" + (envPath.Length == 0 ? "(未设置)" : envPath);

                ctx.Fact(
                    "Aspose 许可证",
                    status.IsLicensed
                        ? "已加载：" + status.LicensePath
                        : "评估模式（无 .lic；assets\\licenses 下 " + licCount + " 个 .lic）");

                if (!status.IsLicensed && licCount == 0 && envPath.Trim().Length == 0)
                {
                    ctx.Pass(check, "确认处于评估模式：未找到 .lic，ASPOSE_LICENSE_PATH 未设置。", evidence);
                    return;
                }

                ctx.Fail(
                    check,
                    "许可证状态与预期不符：IsLicensed=" + status.IsLicensed + "，lic 文件=" + licCount + "，环境变量=" + (envPath.Length == 0 ? "(空)" : "已设置") + "。",
                    evidence);
            });

        ctx.Run(
            "license.merge",
            Group,
            "评估模式下合并 6 个单页 PDF：明确异常 / 完整输出 / 静默截断",
            "SkiaSharp 生成 6 个 1 页 PDF（fixture-1..6.pdf），用 `MergeDocumentOperation(\"pdf.merge\")` 合并成 merged.pdf，再用 Aspose.Pdf 读回页数与文本",
            "要么合并出 6 页且无评估水印，要么抛出可读异常（消息点明评估/许可限制）。不允许「Success 但页数变少」或「成功但输出带水印」。",
            check =>
            {
                string inputDir = ctx.Dir("license-merge-in");
                var inputs = new List<string>();
                for (int i = 1; i <= 6; i++)
                {
                    string file = Path.Combine(inputDir, "fixture-" + i + ".pdf");
                    Fixtures.CreatePdf(file, 1);
                    inputs.Add(file);
                }

                string outDir = ctx.Dir("license-merge-out");
                var operation = new MergeDocumentOperation("pdf.merge", new DocumentMergeSplitService());
                DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = inputs,
                    OutputDirectory = outDir
                }).GetAwaiter().GetResult();

                string target = Path.Combine(outDir, "merged.pdf");
                string evidence = "Success=" + result.Success
                                  + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                  + "\nmerged.pdf 存在: " + File.Exists(target);

                if (!result.Success)
                {
                    bool readable = result.Message.Trim().Length > 0
                                    && (result.Message.Contains("Evaluation", StringComparison.OrdinalIgnoreCase)
                                        || result.Message.Contains("评估", StringComparison.Ordinal)
                                        || result.Message.Contains("许可", StringComparison.Ordinal)
                                        || result.Message.Contains("license", StringComparison.OrdinalIgnoreCase)
                                        || result.Message.Contains("4", StringComparison.Ordinal));
                    ctx.Fact("6 页合并（评估模式）", "抛异常：" + Fmt.Clip(Fmt.OneLine(result.Message), 160));
                    if (readable)
                    {
                        ctx.Pass(check, "抛出可读异常：「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」。", evidence);
                        ctx.AddFinding(
                            check,
                            "中",
                            "评估模式下的合并失败信息是 Aspose 英文原文，且没有「如何解除限制」的指引",
                            "用户看到「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」：虽然是明确异常（没有静默截断），"
                            + "但既未翻译，也未提示加载 .lic / 授权入口，中文界面里难以自救。",
                            "src\\DocumentTools.Core\\Operations\\MergeDocumentOperation.cs:46-48");
                        return;
                    }

                    ctx.Fail(check, "虽然失败，但消息没有指出评估/许可限制，用户无法判断原因。", evidence);
                    return;
                }

                int pages;
                try
                {
                    pages = File.Exists(target) ? Fixtures.CountPdfPages(target) : 0;
                }
                catch (Exception ex)
                {
                    pages = -1;
                    evidence += "\n读回页数失败：" + Fmt.Ex(ex);
                }

                string text = string.Empty;
                bool watermarked = false;
                try
                {
                    text = File.Exists(target) ? Fixtures.PdfText(target) : string.Empty;
                    watermarked = text.Contains(EvaluationMarker, StringComparison.OrdinalIgnoreCase);
                }
                catch (Exception ex)
                {
                    evidence += "\n读取文本失败：" + Fmt.Ex(ex);
                }

                evidence += "\n输出页数=" + pages + "（期望 6）\n含评估水印=" + watermarked
                            + "\n文本片段: " + Fmt.Clip(Fmt.OneLine(text), 300);
                ctx.Fact(
                    "6 页合并（评估模式）",
                    "Success=true，输出 " + pages + " 页，水印=" + (watermarked ? "有" : "无"));

                if (pages == 6 && !watermarked)
                {
                    ctx.Pass(check, "合并结果 6 页且无评估水印，输出完整。", evidence);
                    return;
                }

                ctx.Fail(
                    check,
                    "Success=true 但输出不合格：页数 " + pages + "/6，评估水印=" + (watermarked ? "有" : "无") + "。",
                    evidence);

                if (pages != 6)
                {
                    ctx.AddFinding(
                        check,
                        "高",
                        "评估模式下 6 个单页 PDF 合并后页数不符，且没有任何提示",
                        "MergePdfFiles 逐页 Pages.Add 后直接 Save，全程不检查许可证与页数；本次 Success=true，"
                        + "输出 " + pages + " 页（输入 6 页）。用户会拿到缺页的合并件而不知道被评估模式截断。",
                        "src\\DocumentTools.Core\\Services\\Office\\DocumentMergeSplitService.cs:28-40");
                }

                if (watermarked)
                {
                    ctx.AddFinding(
                        check,
                        "高",
                        "评估模式给输出 PDF 打上 Aspose 水印，调用方未告警",
                        "输出文本中检测到「" + EvaluationMarker + "」水印；合并/拆分在评估模式下会产出带水印的正式文件，"
                        + "而 `MergeDocumentOperation` 仍返回 Ok(\"合并完成。\")。",
                        "src\\DocumentTools.Core\\Operations\\MergeDocumentOperation.cs:44");
                }
            });

        ctx.Run(
            "license.split",
            Group,
            "评估模式下把 5 页 PDF 逐页拆分：明确异常 / 完整输出 / 静默截断",
            "SkiaSharp 生成 5 页 source.pdf，用 `SplitDocumentOperation(\"pdf.split\")` + splitMode=「每页拆分」拆到输出目录，统计文件数与每份页数",
            "产出 5 个单页 PDF 且无水印，或抛出可读异常。不允许「成功但文件数/页数缺失」。",
            check =>
            {
                string inputDir = ctx.Dir("license-split-in");
                string source = Path.Combine(inputDir, "source.pdf");
                Fixtures.CreatePdf(source, 5);

                string outDir = ctx.Dir("license-split-out");
                var operation = new SplitDocumentOperation(
                    "pdf.split",
                    new DocumentMergeSplitService(),
                    new PdfSplitService());
                DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = new[] { source },
                    OutputDirectory = outDir,
                    Options = new Dictionary<string, string>
                    {
                        ["splitMode"] = "每页拆分",
                        ["pagesPerFile"] = "1"
                    }
                }).GetAwaiter().GetResult();

                string[] files = Directory.Exists(outDir)
                    ? Directory.GetFiles(outDir, "*.pdf", SearchOption.AllDirectories)
                    : Array.Empty<string>();
                string evidence = "Success=" + result.Success
                                  + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                  + "\n输出文件数=" + files.Length + "（期望 5）";

                if (!result.Success)
                {
                    bool readable = result.Message.Trim().Length > 0
                                    && (result.Message.Contains("Evaluation", StringComparison.OrdinalIgnoreCase)
                                        || result.Message.Contains("评估", StringComparison.Ordinal)
                                        || result.Message.Contains("许可", StringComparison.Ordinal)
                                        || result.Message.Contains("license", StringComparison.OrdinalIgnoreCase)
                                        || result.Message.Contains("4", StringComparison.Ordinal));
                    ctx.Fact("5 页拆分（评估模式）", "抛异常：" + Fmt.Clip(Fmt.OneLine(result.Message), 160));
                    if (readable)
                    {
                        ctx.Pass(check, "抛出可读异常：「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」。", evidence);
                        ctx.AddFinding(
                            check,
                            "低",
                            "拆分失败同样直出 Aspose 英文文案",
                            "与 license.merge 同源：`SplitDocumentOperation` 也是 `Fail(ex.Message)`，评估模式下用户只会看到英文限制说明。",
                            "src\\DocumentTools.Core\\Operations\\SplitDocumentOperation.cs:50-52");
                        return;
                    }

                    ctx.Fail(check, "虽然失败，但消息没有指出评估/许可限制。", evidence);
                    return;
                }

                var pageCounts = new List<string>();
                bool watermarked = false;
                foreach (string file in files)
                {
                    try
                    {
                        pageCounts.Add(Path.GetFileName(file) + "=" + Fixtures.CountPdfPages(file));
                        if (!watermarked && Fixtures.PdfText(file).Contains(EvaluationMarker, StringComparison.OrdinalIgnoreCase))
                        {
                            watermarked = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        pageCounts.Add(Path.GetFileName(file) + "=读取失败(" + Fmt.OneLine(ex.Message) + ")");
                    }
                }

                evidence += "\n每份页数: " + string.Join("; ", pageCounts)
                            + "\n含评估水印=" + watermarked;
                ctx.Fact("5 页拆分（评估模式）", "Success=true，输出 " + files.Length + " 个文件，水印=" + (watermarked ? "有" : "无"));

                if (files.Length == 5 && !watermarked)
                {
                    ctx.Pass(check, "拆出 5 个文件、每份 1 页、无评估水印。", evidence);
                    return;
                }

                ctx.Fail(
                    check,
                    "Success=true 但输出不合格：文件数 " + files.Length + "/5，评估水印=" + (watermarked ? "有" : "无") + "。",
                    evidence);

                if (files.Length != 5)
                {
                    ctx.AddFinding(
                        check,
                        "高",
                        "评估模式下 5 页拆分产出数量不符，且没有任何提示",
                        "PdfSplitService 每块都重新打开源文件并 SaveRange，不检查许可证；本次 Success=true，"
                        + "产出 " + files.Length + " 个文件（期望 5）。缺的那几页被静默丢弃。",
                        "src\\DocumentTools.Core\\Services\\Office\\PdfSplitService.cs:27-38, 77-87");
                }

                if (watermarked)
                {
                    ctx.AddFinding(
                        check,
                        "中",
                        "拆出的 PDF 带 Aspose 评估水印，调用方未告警",
                        "拆分产物文本中检测到「" + EvaluationMarker + "」；`SplitDocumentOperation` 仍返回 Ok(\"拆分完成…\")。",
                        "src\\DocumentTools.Core\\Operations\\SplitDocumentOperation.cs:47-48");
                }
            });
    }
}
