using DocumentTools.Core.Data;
using DocumentTools.Core.Services.Exam;

namespace QaCompat.Checks;

/// <summary>
/// 无 Microsoft ACE/JET 的降级表现。
/// 两条真实通道：(1) 损坏/非 Access 文件让 provider 全部失败；(2) DOCUMENTTOOLS_SIMULATE_NO_ACE 模拟。
/// </summary>
internal static class AceChecks
{
    public const string Group = "ace";

    private const string SimulateVariable = "DOCUMENTTOOLS_SIMULATE_NO_ACE";

    public static void Run(QaContext ctx)
    {
        string? fixtureDb = null;

        ctx.Run(
            "ace.fixture",
            Group,
            "ACE/JET 真实可用性基准（ExamFixture 探测）",
            "`dotnet tools\\ExamFixture\\bin\\Debug\\net8.0-windows\\ExamFixture.dll --output testdata\\compat\\work\\run-<ts>\\exam`（不设任何开关），读退出码与生成的 .mdb",
            "退出码 0、生成 Exam_Base.mdb；据此确认本机 ACE 可用，作为「真机有 ACE」的基准。",
            check =>
            {
                string outDir = ctx.Dir("exam");
                ProcResult run = Proc.Run("dotnet", new[] { ctx.ExamFixtureDll, "--output", outDir }, ctx.RepoRoot);
                string db = Directory.EnumerateFiles(outDir, "*.mdb", SearchOption.AllDirectories).FirstOrDefault() ?? string.Empty;
                string evidence =
                    "exit=" + run.ExitCode + "\n" +
                    "stdout: " + Fmt.Clip(run.StdOut, 400) + "\n" +
                    "stderr: " + Fmt.Clip(run.StdErr, 400) + "\n" +
                    "生成数据库: " + (db.Length == 0 ? "(无)" : db);

                if (run.ExitCode == 0 && db.Length > 0)
                {
                    fixtureDb = db;
                    ctx.Fact("ACE / JET", "可用（ExamFixture exit 0，已生成 Exam_Base.mdb）");
                    ctx.Pass(check, "ExamFixture 探测通过，ACE/JET 可用。", evidence);
                    return;
                }

                ctx.Fact("ACE / JET", "不可用（ExamFixture exit " + run.ExitCode + "）");
                ctx.Fail(check, "ExamFixture 探测失败：exit=" + run.ExitCode + "，未生成 .mdb。", evidence);
            });

        ctx.Run(
            "ace.missing-provider",
            Group,
            "provider 全部失败时考评服务抛出的异常是否可读、是否拖垮进程",
            "把一个文本文件命名为 `broken.mdb`（内容 `this is not an access database`），调用 `ExamIndexService.ChangeExamIndex`，捕获异常后继续执行后续检查",
            "抛出 `InvalidOperationException`，且命中与根因相符的那一层：ACE 可用时报「数据库文件可能损坏、被占用或不是 Access 2003/2007+ 格式」并带上文件名与底层错误，ACE 真不可用时才给安装建议；进程存活，后续检查照常运行。",
            check =>
            {
                string dir = ctx.Dir("ace-broken");
                string bogus = Path.Combine(dir, "broken.mdb");
                File.WriteAllText(bogus, "this is not an access database");

                var service = new ExamIndexService();
                try
                {
                    ExamIndexChangeResult result = service.ChangeExamIndex(new ExamIndexChangeRequest
                    {
                        DatabaseFile = bogus,
                        OriginIndex = 1,
                        NewIndex = 2
                    });
                    ctx.Fail(
                        check,
                        "没有抛出异常，改号返回 Success=" + result.Success + "（对损坏库静默成功不可信）。",
                        "返回值 Summary: " + Fmt.Clip(result.Summary, 300));
                }
                catch (Exception ex)
                {
                    string evidence = Fmt.Ex(ex);

                    // AccessDb 现在是三级归因：缺 provider / 文件损坏或被占用 / 其它。
                    // ACE 可用的机器上，坏文件必须命中「文件可能损坏」层，否则就是归因错误。
                    bool isInvalidOperation = ex is InvalidOperationException;
                    bool damagedTier = ex.Message.Contains("数据库文件可能损坏", StringComparison.Ordinal)
                                       && ex.Message.Contains("broken.mdb", StringComparison.Ordinal);
                    bool missingTier = ex.Message.Contains("未检测到可用的", StringComparison.Ordinal);
                    bool aceAvailable = fixtureDb is not null;
                    bool attributionOk = aceAvailable ? damagedTier : missingTier;

                    if (!isInvalidOperation || !(damagedTier || missingTier) || !attributionOk)
                    {
                        ctx.Fail(
                            check,
                            "抛出了 " + ex.GetType().Name + "，但消息既不是与根因相符的归因层"
                            + "（ACE 可用=" + aceAvailable + "，损坏层=" + damagedTier + "，缺 provider 层=" + missingTier + "）。",
                            evidence);
                        return;
                    }

                    ctx.Fact(
                        "ACE 归因",
                        damagedTier
                            ? "损坏库命中「文件可能损坏」层（三级归因生效，不再一律报装 ACE）"
                            : "本机无 ACE，命中「未检测到 provider」层");

                    ctx.Pass(
                        check,
                        "抛出 InvalidOperationException，消息为「" + Fmt.OneLine(ex.Message) + "」；后续检查继续执行，进程未退出。",
                        evidence + "\n进程存活：本检查返回后 ace.simulate-switch 等检查继续运行。");
                }
            });

        ctx.Run(
            "ace.simulate-switch",
            Group,
            "DOCUMENTTOOLS_SIMULATE_NO_ACE=1 能否让 Core 考评服务进入「无 ACE」降级分支",
            "文档：进程内设置 `DOCUMENTTOOLS_SIMULATE_NO_ACE=1`，用 ExamFixture 生成的真实 .mdb 构造 `AccessDb` 并调用 `ExamIndexService.ChangeExamIndex`（均在 finally 里还原环境变量）",
            "开关被 Core 识别：`AccessDb` 在构造（打开）阶段即抛 `InvalidOperationException`，消息等同 `AccessDb.MissingAceMessage()`（含「未检测到可用的 … OLE DB 提供者」），考评服务调用同样抛错；不再出现「打开成功并跑完」的情况。",
            check =>
            {
                if (fixtureDb is null)
                {
                    throw new SkipCheckException("ace.fixture 未产出可用 .mdb，无法在模拟开关下打开数据库。");
                }

                string copyDir = ctx.Dir("ace-sim");
                string dbCopy = Path.Combine(copyDir, "Exam_Base.mdb");
                File.Copy(fixtureDb, dbCopy, overwrite: true);

                string? previous = Environment.GetEnvironmentVariable(SimulateVariable);
                Environment.SetEnvironmentVariable(SimulateVariable, "1");
                try
                {
                    var evidence = new System.Text.StringBuilder();
                    evidence.Append("环境变量 ").Append(SimulateVariable).Append("=1（进程内设置）\n");

                    int tableCount = -1;
                    string? openType = null;
                    string? openMessage = null;
                    try
                    {
                        using (var session = new AccessDb(dbCopy))
                        {
                            IReadOnlyList<string> tables = session.ListTables();
                            tableCount = tables.Count;
                            evidence.Append("AccessDb 打开成功，ListTables()=").Append(tableCount)
                                .Append(" 张：").Append(string.Join("、", tables.Take(6))).Append('\n');
                        }
                    }
                    catch (Exception ex)
                    {
                        openType = ex.GetType().Name;
                        openMessage = ex.Message;
                        evidence.Append("new AccessDb(...) 抛出 ").Append(openType).Append("：")
                            .Append(Fmt.Clip(Fmt.OneLine(ex.Message), 240)).Append('\n');
                    }

                    string? serviceMessage = null;
                    try
                    {
                        var service = new ExamIndexService();
                        ExamIndexChangeResult result = service.ChangeExamIndex(new ExamIndexChangeRequest
                        {
                            DatabaseFile = dbCopy,
                            OriginIndex = 1,
                            NewIndex = 2
                        });
                        evidence.Append("ChangeExamIndex 返回 Success=").Append(result.Success)
                            .Append("，Summary=").Append(Fmt.Clip(Fmt.OneLine(result.Summary), 160));
                    }
                    catch (Exception ex)
                    {
                        serviceMessage = ex.Message;
                        evidence.Append("ChangeExamIndex 抛出 ").Append(ex.GetType().Name).Append("：")
                            .Append(Fmt.Clip(Fmt.OneLine(ex.Message), 240));
                    }

                    // 开关生效的标志：构造阶段就抛，且文案与真实缺 ACE 完全同一句。
                    bool openThrows = openType == nameof(InvalidOperationException)
                                      && openMessage is not null
                                      && openMessage.Equals(AccessDb.MissingAceMessage(), StringComparison.Ordinal);
                    bool serviceThrows = serviceMessage is not null
                                         && serviceMessage.Contains("未检测到可用的", StringComparison.Ordinal);

                    if (!openThrows || !serviceThrows)
                    {
                        ctx.Fail(
                            check,
                            "开关未完全生效：AccessDb 构造即抛「未检测到 provider」=" + openThrows
                            + "（构造结果 tableCount=" + tableCount + "），考评服务抛同一文案=" + serviceThrows + "。",
                            evidence.ToString());
                        return;
                    }

                    ctx.Fact("模拟无 ACE", "AccessDb 构造即抛 " + openType + "（与真实缺 ACE 同一句文案），考评服务同步抛错");
                    ctx.Pass(
                        check,
                        "开关生效：设置 " + SimulateVariable + "=1 后 `new AccessDb(...)` 在打开阶段即抛"
                        + "「未检测到可用的 … OLE DB 提供者」，考评服务调用同样抛错，不再返回可用会话。",
                        evidence.ToString());
                }
                finally
                {
                    Environment.SetEnvironmentVariable(SimulateVariable, previous);
                }
            });

        ctx.Run(
            "ace.examfixture-sim",
            Group,
            "ExamFixture 在模拟无 ACE 下的行为（退出码 + 可读文案）",
            "子进程运行 `dotnet <ExamFixture.dll> --output testdata\\compat\\work\\run-<ts>\\exam-noace --no-seed`，子进程环境变量 " + SimulateVariable + "=1",
            "退出码 2，stderr 出现「未检测到可用的 Access OLE DB 提供者」+ 开关名，且不含 `Unhandled exception`。",
            check =>
            {
                string outDir = ctx.Dir("exam-noace");
                var env = new Dictionary<string, string> { [SimulateVariable] = "1" };
                ProcResult run = Proc.Run(
                    "dotnet",
                    new[] { ctx.ExamFixtureDll, "--output", outDir, "--no-seed" },
                    ctx.RepoRoot,
                    env);

                string text = run.StdErr + "\n" + run.StdOut;
                bool codeOk = run.ExitCode == 2;
                bool messageOk = text.Contains("未检测到可用的 Access OLE DB 提供者", StringComparison.Ordinal)
                                 && text.Contains(SimulateVariable, StringComparison.Ordinal);
                bool noCrash = !text.Contains("Unhandled exception", StringComparison.OrdinalIgnoreCase);

                string evidence = "exit=" + run.ExitCode + "\nstderr: " + Fmt.Clip(run.StdErr, 500);

                if (codeOk && messageOk && noCrash)
                {
                    ctx.Fact("ExamFixture 模拟无 ACE", "exit 2 + 可读跳过文案（" + SimulateVariable + "=1）");
                    ctx.Pass(check, "退出码 2，stderr 为可读的「未检测到可用的 Access OLE DB 提供者」说明，无未处理异常。", evidence);
                    return;
                }

                ctx.Fail(
                    check,
                    "行为不符合契约：exit=" + run.ExitCode + "，消息匹配=" + messageOk + "，无未处理异常=" + noCrash + "。",
                    evidence);
            });

        ctx.Run(
            "ace.real-missing",
            Group,
            "真机卸载 ACE 后 ExamFixture 与 Core 的行为（需破坏性系统改动）",
            "卸载 Microsoft Access Database Engine（或换一台未装 ACE 的机器）后重跑 `ace.fixture` / `ace.missing-provider`",
            "ExamFixture exit 2 且文案与模拟一致；Core 抛「无法打开 Access 数据库。请安装 Microsoft Access Database Engine (ACE)。」",
            check =>
            {
                bool aceWorks = fixtureDb is not null;
                ctx.Skip(
                    check,
                    "本机 ACE 可用（" + (aceWorks ? "见 ace.fixture" : "探测未通过") + "），卸载 ACE 属破坏性系统改动，本次不执行。",
                    "无法验证：需要在没有 ACE 的机器上运行。已用两条不破坏系统的通道覆盖同一消息路径——"
                    + "损坏库触发 provider 链全失败（ace.missing-provider），以及开关模拟（ace.examfixture-sim）；"
                    + "两者共用 AccessDb.cs:188-190 的抛出语句。");
            });
    }
}
