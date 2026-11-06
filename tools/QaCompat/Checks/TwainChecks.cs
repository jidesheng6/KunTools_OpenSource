using DocumentTools.Core.Services.Scan;

namespace QaCompat.Checks;

/// <summary>无扫描仪 / 无 TWAIN DSM 时扫描服务的降级表现。</summary>
internal static class TwainChecks
{
    public const string Group = "twain";

    public static void Run(QaContext ctx)
    {
        using var scanner = new TwainScanService();
        bool available = scanner.IsAvailable;
        string reason = scanner.UnavailableReason;

        ctx.Fact(
            "TWAIN",
            available
                ? "Session 打开成功（DSM 可用；无实机扫描仪不参与本次判定）"
                : "不可用：" + Fmt.Clip(Fmt.OneLine(reason), 200));

        ctx.Run(
            "twain.unavailable",
            Group,
            "无扫描仪/无 DSM 时 Scan 抛 NotSupportedException，消息含可操作建议",
            "`TwainScanService.IsAvailable` 为 false（本机未装 TWAINDSM.dll 或扫描仪驱动）时调用 `Scan(\"NO_SUCH_SCANNER\", options)`",
            "抛 `NotSupportedException`，消息等于 `UnavailableReason`，且包含「请安装扫描仪驱动 / TWAIN Data Source Manager」这类可执行建议。",
            check =>
            {
                string scanDir = ctx.Dir("scan");
                if (available)
                {
                    throw new SkipCheckException(
                        "本机 TwainSession 打开成功（IsAvailable=true），NotSupportedException 分支不可达；见 twain.no-source。");
                }

                try
                {
                    scanner.Scan("NO_SUCH_SCANNER", new TwainScanOptions { OutputDirectory = scanDir });
                    ctx.Fail(check, "IsAvailable=false 但 Scan 没有抛 NotSupportedException。", "返回正常，未抛异常。");
                }
                catch (NotSupportedException ex)
                {
                    string evidence = Fmt.Ex(ex) + "\nUnavailableReason: " + Fmt.Clip(reason, 400);
                    bool sameMessage = string.Equals(Fmt.OneLine(ex.Message), Fmt.OneLine(reason), StringComparison.Ordinal);
                    bool actionable = ex.Message.Contains("请安装", StringComparison.Ordinal)
                                      || ex.Message.Contains("TWAINDSM", StringComparison.Ordinal)
                                      || ex.Message.Contains("驱动", StringComparison.Ordinal);

                    if (sameMessage && actionable)
                    {
                        ctx.Pass(
                            check,
                            "抛出 NotSupportedException，消息与 UnavailableReason 一致，含可操作建议：「" + Fmt.Clip(Fmt.OneLine(ex.Message), 200) + "」。",
                            evidence);
                        return;
                    }

                    ctx.Fail(
                        check,
                        "异常类型正确，但消息与 UnavailableReason 一致=" + sameMessage + "、含可操作建议=" + actionable + "。",
                        evidence);
                }
            });

        ctx.Run(
            "twain.no-source",
            Group,
            "有 DSM 但没有匹配数据源时的消息",
            "`Scan(\"NO_SUCH_SCANNER\", options)`（Session 已打开、数据源名不存在）",
            "抛 `InvalidOperationException`，消息含「未找到扫描仪：NO_SUCH_SCANNER」。",
            check =>
            {
                string scanDir = ctx.Dir("scan-no-source");
                if (!available)
                {
                    throw new SkipCheckException("本机 IsAvailable=false（无 DSM），该分支不可达；见 twain.unavailable。");
                }

                try
                {
                    scanner.Scan("NO_SUCH_SCANNER", new TwainScanOptions { OutputDirectory = scanDir });
                    ctx.Fail(check, "扫描仪名不存在却未抛异常。", "Scan 正常返回。");
                }
                catch (InvalidOperationException ex)
                {
                    string evidence = Fmt.Ex(ex);
                    bool messageOk = ex.Message.Contains("未找到扫描仪", StringComparison.Ordinal)
                                     && ex.Message.Contains("NO_SUCH_SCANNER", StringComparison.Ordinal);
                    if (messageOk)
                    {
                        ctx.Pass(check, "消息明确列出缺失的数据源名：「" + Fmt.OneLine(ex.Message) + "」。", evidence);
                        return;
                    }

                    ctx.Fail(check, "抛出了 InvalidOperationException，但消息未点明扫描仪名。", evidence);
                }
            });
    }
}
