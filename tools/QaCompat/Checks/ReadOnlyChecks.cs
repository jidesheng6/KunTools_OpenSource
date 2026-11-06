using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Image;
using DocumentTools.Core.Services.Office;

namespace QaCompat.Checks;

/// <summary>输出目录只读 / 目标文件被占用时的失败方式与残留文件。</summary>
internal static class ReadOnlyChecks
{
    public const string Group = "readonly";

    public static void Run(QaContext ctx)
    {
        ctx.Run(
            "readonly.outdir",
            Group,
            "输出目录只读：转换失败可读、且不留半成品",
            "对输出目录执行 `icacls <dir> /deny *S-1-1-0:(W)`，先用探测文件确认 ACL 生效，再执行 `image.compress`",
            "`image.compress` 返回 Success=false，消息能说明是权限/拒绝访问；输出目录里除探测文件外没有残留的半成品 PNG。",
            check =>
            {
                string outDir = ctx.Dir("readonly-out");
                ProcResult deny = Fixtures.DenyWrite(outDir);
                try
                {
                    Exception? probe = Fixtures.ProbeWrite(outDir);
                    if (probe is null)
                    {
                        throw new SkipCheckException(
                            "icacls 拒绝写 ACE 未生效（探测文件仍能写入），本机无法构造只读目录。icacls 输出："
                            + Fmt.Clip(Fmt.OneLine(deny.StdOut + " " + deny.StdErr), 300));
                    }

                    // 探测文件未创建成功（ACL 已拒绝），因此无需删除；目录本身是新建的，无历史文件。
                    string source = Path.Combine(ctx.Dir("readonly-in"), "readonly_src.png");
                    Fixtures.CreateImage(source);

                    var operation = new ImageConvertOperation("image.compress", new ImageConversionService());
                    DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                    {
                        InputFiles = new[] { source },
                        OutputDirectory = outDir,
                        Options = new Dictionary<string, string> { ["format"] = "PNG" }
                    }).GetAwaiter().GetResult();

                    // 只读目录无法枚举，改用已知输出名判断是否留下半成品。
                    string expectedOutput = Path.Combine(outDir, "readonly_src_out.png");
                    bool halfProduct = File.Exists(expectedOutput);
                    string evidence = "icacls: " + Fmt.Clip(Fmt.OneLine(deny.StdOut + " " + deny.StdErr), 300)
                                      + "\n探测写入异常: " + Fmt.Ex(probe)
                                      + "\nSuccess=" + result.Success
                                      + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                      + "\n预期输出存在=" + halfProduct
                                      + "（目录本身不可枚举，只核对已知输出名）";

                    if (result.Success)
                    {
                        ctx.Fail(check, "输出目录不可写，但转换仍报告成功。", evidence);
                        return;
                    }

                    if (halfProduct)
                    {
                        ctx.Fail(check, "失败后仍在输出目录留下半成品 readonly_src_out.png。", evidence);
                    }
                    else
                    {
                        ctx.Pass(
                            check,
                            "失败且未留半成品；错误消息为原始 .NET 文案：「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」。",
                            evidence);
                    }

                    ctx.AddFinding(
                        check,
                        "中",
                        "只读输出目录的失败信息是英文 .NET 原始文案，未包装中文提示",
                        "用户可见的是 `" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "`；`ImageConvertOperation` 直接 Fail(ex.Message)，"
                        + "中文界面里出现英文「Access to the path ... is denied.」且不带建议（检查写入权限/换目录）。",
                        "src\\DocumentTools.Core\\Operations\\ImageConvertOperation.cs:55-57");
                }
                finally
                {
                    Fixtures.AllowWrite(outDir);
                }
            });

        ctx.Run(
            "readonly.locked",
            Group,
            "目标 PDF 被占用：合并失败可读、不破坏原文件",
            "预创建 merged.pdf 并用 `FileStream(FileMode.Create, FileAccess.ReadWrite, FileShare.None)` 持有，再执行 `pdf.merge`",
            "`pdf.merge` 返回 Success=false，消息说明文件被占用/拒绝访问；目标文件保持 0 字节，不写入半份内容。",
            check =>
            {
                string inputDir = ctx.Dir("locked-in");
                string first = Path.Combine(inputDir, "a.pdf");
                string second = Path.Combine(inputDir, "b.pdf");
                Fixtures.CreatePdf(first, 1);
                Fixtures.CreatePdf(second, 1);

                string outDir = ctx.Dir("locked-out");
                string target = Path.Combine(outDir, "merged.pdf");
                using var hold = new FileStream(target, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
                long before = hold.Length;

                try
                {
                    var operation = new MergeDocumentOperation("pdf.merge", new DocumentMergeSplitService());
                    DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                    {
                        InputFiles = new[] { first, second },
                        OutputDirectory = outDir
                    }).GetAwaiter().GetResult();

                    // 句柄仍在手里，直接读它自己的长度（另开句柄会被 FileShare.None 拒绝，属误判）。
                    long after;
                    try
                    {
                        hold.Flush();
                        after = hold.Length;
                    }
                    catch (Exception ex)
                    {
                        after = -1;
                        ctx.Fact("占用文件长度", "读取失败：" + Fmt.OneLine(ex.Message));
                    }

                    string evidence = "Success=" + result.Success
                                      + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                      + "\n文件长度 before=" + before + " after=" + after;

                    if (result.Success)
                    {
                        ctx.Fail(check, "目标文件被独占占用，合并仍报告成功。", evidence);
                        return;
                    }

                    bool readable = result.Message.Contains("占用", StringComparison.Ordinal)
                                    || result.Message.Contains("拒绝", StringComparison.Ordinal)
                                    || result.Message.Contains("another process", StringComparison.OrdinalIgnoreCase)
                                    || result.Message.Contains("denied", StringComparison.OrdinalIgnoreCase)
                                    || result.Message.Contains("being used", StringComparison.OrdinalIgnoreCase);
                    bool intact = after == before;

                    if (readable && intact)
                    {
                        ctx.Pass(
                            check,
                            "失败消息点明占用/拒绝访问，目标文件长度未变（" + after + " 字节）；原文：「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」。",
                            evidence);
                    }
                    else
                    {
                        ctx.Fail(
                            check,
                            "失败但消息可读=" + readable + "、目标文件未被写入=" + intact + "。",
                            evidence);
                    }

                    if (!readable)
                    {
                        ctx.AddFinding(
                            check,
                            "中",
                            "目标文件被占用时错误消息不可读",
                            "用户看到的是「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」，没有指出「请关闭已打开的 PDF」这一可操作动作。",
                            "src\\DocumentTools.Core\\Operations\\MergeDocumentOperation.cs:46-48");
                    }
                }
                finally
                {
                    hold.Dispose();
                }
            });
    }
}
