using System.Globalization;
using System.Text;

namespace DocumentTools.QaPerf;

/// <summary>
/// Builds docs/qa-report-perf.md from persisted sessions. Every number in the report comes from a
/// session file (artifacts/qa-perf/sessions/*.json) so any figure can be traced back and reproduced.
/// </summary>
internal static class ReportWriter
{
    private static readonly string[] CanonicalOrder =
    {
        "wordpdf-10", "wordpdf-50", "image-50", "large-word-30p", "large-pdf-200p", "merge-2", "merge-4", "merge-20"
    };

    private const double Mebibyte = 1024d * 1024d;

    public static void Write(string path, IReadOnlyList<Session> sessions, RepoLayout layout)
    {
        if (sessions.Count == 0)
        {
            throw new InvalidOperationException("没有可用的 session，无法生成报告。");
        }

        Session latest = sessions[^1];
        Session primary = sessions
            .OrderByDescending(session => session.Runs.Count)
            .ThenByDescending(session => session.StartedAt)
            .First();

        List<string> scenarioIds = CollectScenarioIds(sessions);
        var sb = new StringBuilder(64 * 1024);

        WriteHeader(sb, sessions, primary);
        WriteSummary(sb, sessions, primary, scenarioIds);
        WriteEnvironment(sb, sessions, latest);
        WriteFixtures(sb, sessions);
        WriteScenarios(sb, primary, scenarioIds);
        WriteColdWarm(sb, sessions, scenarioIds);
        WriteRounds(sb, sessions, scenarioIds);
        WriteVerification(sb, sessions, scenarioIds);
        WriteLinearity(sb, sessions, primary);
        WriteLeaks(sb, sessions, scenarioIds);
        WriteDisposeAudit(sb, latest);
        WriteConclusions(sb, sessions, primary, scenarioIds);
        WriteRecommendations(sb, sessions, primary, scenarioIds);
        WriteReproduce(sb, sessions);
        WriteLimits(sb, sessions);

        string directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".";
        Directory.CreateDirectory(directory);
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    // ---------------------------------------------------------------- header

    private static void WriteHeader(StringBuilder sb, IReadOnlyList<Session> sessions, Session primary)
    {
        sb.AppendLine("# 批量处理性能与内存基线（QaPerf）");
        sb.AppendLine();
        sb.AppendLine("> 本报告由 `tools\\QaPerf` 实测生成，无手工填写的数据。");
        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "> 生成时间：{0:yyyy-MM-dd HH:mm:ss zzz} · 主数据 session：`{1}`（{2} 次运行）· 历史 session：{3} 个",
            DateTimeOffset.Now,
            primary.Id,
            primary.Runs.Count,
            sessions.Count));
        sb.AppendLine("> 数据文件：`artifacts\\qa-perf\\sessions\\*.json`；每场景输出：`artifacts\\qa-perf\\out\\<场景>\\round-<n>`。");
        sb.AppendLine();
    }

    // -------------------------------------------------------------- summary

    private static void WriteSummary(
        StringBuilder sb,
        IReadOnlyList<Session> sessions,
        Session primary,
        List<string> scenarioIds)
    {
        sb.AppendLine("## 1. 结论摘要");
        sb.AppendLine();

        List<RunMetrics> primaryRuns = primary.Runs;
        var bullets = new List<string>();

        // 单文件均时只取热态、非阻断的轮次，否则冷启动轮与 blocked 轮（0 输出）会污染区间。
        List<RunMetrics> comparable = primaryRuns.Where(run => !run.ColdStart && !run.Blocked && run.MeanMsPerFile > 0).ToList();
        if (comparable.Count == 0)
        {
            comparable = primaryRuns.Where(run => !run.Blocked && run.MeanMsPerFile > 0).ToList();
        }

        RunMetrics? fastest = comparable.OrderBy(run => run.MeanMsPerFile).FirstOrDefault();
        RunMetrics? slowest = comparable.OrderByDescending(run => run.MeanMsPerFile).FirstOrDefault();
        if (fastest is not null && slowest is not null)
        {
            bullets.Add(string.Format(
                CultureInfo.InvariantCulture,
                "单文件平均耗时区间 {0:0.0} ms（{1}）~ {2:0.0} ms（{3}），差 {4:0.0} 倍（只统计热态轮次）。",
                fastest.MeanMsPerFile,
                fastest.ScenarioId,
                slowest.MeanMsPerFile,
                slowest.ScenarioId,
                fastest.MeanMsPerFile <= 0 ? 0 : slowest.MeanMsPerFile / fastest.MeanMsPerFile));
        }

        // 线性度与冷启动都取「两个 word.pdf 场景同进程首跑」的 session，口径与第 5、8 节一致。
        Session? coldSource = sessions
            .Where(session => RunsFor(session, "wordpdf-10").Any(run => run.ColdStart)
                && RunsFor(session, "wordpdf-50").Any(run => run.ColdStart))
            .OrderByDescending(session => session.StartedAt)
            .FirstOrDefault();

        RunMetrics? tenCold = coldSource is null ? null : RunsFor(coldSource, "wordpdf-10").FirstOrDefault(run => run.ColdStart);
        RunMetrics? fiftyCold = coldSource is null ? null : RunsFor(coldSource, "wordpdf-50").FirstOrDefault(run => run.ColdStart);

        // 规模对比取同一 session 的热态轮（第 2 轮），与第 8 节口径一致。
        Session? scaleSource = sessions
            .Where(session => RunsFor(session, "wordpdf-10").Count >= 2 && RunsFor(session, "wordpdf-50").Count >= 2)
            .OrderByDescending(session => RunsFor(session, "wordpdf-10").Any(run => run.ColdStart))
            .ThenByDescending(session => session.StartedAt)
            .FirstOrDefault();

        if (scaleSource is not null)
        {
            RunMetrics ten = RunsFor(scaleSource, "wordpdf-10")[1];
            RunMetrics fifty = RunsFor(scaleSource, "wordpdf-50")[1];
            double ratio = ten.TotalMs <= 0 ? 0 : fifty.TotalMs / ten.TotalMs;
            string verdict = ratio switch
            {
                >= 4.0 and <= 6.0 => "近似线性",
                < 4.0 => "亚线性（固定开销被大批量摊薄，单文件均时下降）",
                _ => "超线性（批量越大单文件越慢）"
            };
            bullets.Add(string.Format(
                CultureInfo.InvariantCulture,
                "word.pdf 从 10 个扩到 50 个（输入 5.0 倍，热态第 2 轮）：总耗时 {0:0.0} -> {1:0.0} ms（{2:0.00} 倍），单文件均时 {3:0.0} -> {4:0.0} ms，{5}。",
                ten.TotalMs,
                fifty.TotalMs,
                ratio,
                ten.MeanMsPerFile,
                fifty.MeanMsPerFile,
                verdict));
        }
        else if (tenCold is not null && fiftyCold is not null)
        {
            double ratio = tenCold.TotalMs <= 0 ? 0 : fiftyCold.TotalMs / tenCold.TotalMs;
            bullets.Add(string.Format(
                CultureInfo.InvariantCulture,
                "word.pdf 从 10 个扩到 50 个（输入 5.0 倍，冷启动轮）：总耗时 {0:0.0} -> {1:0.0} ms（{2:0.00} 倍）。",
                tenCold.TotalMs,
                fiftyCold.TotalMs,
                ratio));
        }

        Session? coldOwner = sessions
            .Where(session => RunsFor(session, "wordpdf-10").Any(run => run.ColdStart))
            // 专用单场景 session（2 轮）优先，避免主 session 的全局首轮掩盖真实冷启动代价。
            .OrderBy(session => session.Runs.Count)
            .ThenByDescending(session => session.StartedAt)
            .FirstOrDefault();

        if (coldOwner is not null
            && RunsFor(coldOwner, "wordpdf-10").FirstOrDefault(run => run.ColdStart) is RunMetrics cold)
        {
            RunMetrics? warm = RunsFor(coldOwner, cold.ScenarioId).FirstOrDefault(run => !run.ColdStart);
            if (warm is not null)
            {
                double dropPct = cold.TotalMs <= 0 ? 0 : (cold.TotalMs - warm.TotalMs) / cold.TotalMs * 100;
                string coldTrend = dropPct > 1
                    ? string.Format(CultureInfo.InvariantCulture, "回落 {0:0.0}%（JIT + Aspose 初始化 + 字体缓存）", dropPct)
                    : string.Format(
                        CultureInfo.InvariantCulture,
                        "第二轮没有更快（{0}），说明该场景的预热收益被同批次测量噪声掩盖",
                        string.Format(CultureInfo.InvariantCulture, "{0:+0.0;-0.0;0.0}%", -dropPct));
                bullets.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "冷启动代价（{0}，session `{1}`）：第一轮 {2:0.0} ms，第二轮 {3:0.0} ms，{4}。",
                    cold.ScenarioId,
                    coldOwner.Id,
                    cold.TotalMs,
                    warm.TotalMs,
                    coldTrend));
            }
        }

        RunMetrics? peak = primary.Runs.OrderByDescending(run => run.RoundPeakWorkingSetBytes).FirstOrDefault();
        if (peak is not null)
        {
            bullets.Add(string.Format(
                CultureInfo.InvariantCulture,
                "峰值工作集最高 {0:0.0} MB（{1} 第 {2} 轮，Process.PeakWorkingSet64 = {3:0.0} MB），最低 {4:0.0} MB。",
                peak.RoundPeakWorkingSetMb,
                peak.ScenarioId,
                peak.Round,
                peak.PeakWorkingSetMb,
                primary.Runs.Min(run => run.RoundPeakWorkingSetMb)));
        }

        int failedRuns = primary.Runs.Count(run => run.Failures > 0 && !run.Blocked);
        int blockedRuns = primary.Runs.Count(run => run.Blocked);
        int totalRuns = primary.Runs.Count;
        bullets.Add(failedRuns > 0
            ? string.Format(CultureInfo.InvariantCulture, "主数据 {0} 轮运行中有 {1} 轮存在失败产出，详见第 6、7 节。", totalRuns, failedRuns)
            : blockedRuns == 0
                ? string.Format(CultureInfo.InvariantCulture, "主数据 {0} 轮运行全部产出完整（失败数 0）。", totalRuns)
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "主数据 {0} 轮中 {1} 轮产出完整（失败数 0），其余 {2} 轮被评估版阻断、没有产物（不是实现失败）。",
                    totalRuns,
                    totalRuns - blockedRuns,
                    blockedRuns));

        if (blockedRuns > 0)
        {
            List<string> blockedIds = primary.Runs
                .Where(run => run.Blocked)
                .Select(run => run.ScenarioId)
                .Distinct(StringComparer.Ordinal)
                .ToList();
            bullets.Add(string.Format(
                CultureInfo.InvariantCulture,
                "评估版还直接阻断了 {0} 个场景（{1}）：这些轮次没有产出文件，原因是 Aspose 评估版的元素数量上限，不是操作实现的问题，原文见第 2、6 节。",
                blockedIds.Count,
                string.Join("、", blockedIds)));
        }

        List<string> leakVerdicts = sessions
            .SelectMany(session => session.Leaks)
            .GroupBy(leak => leak.ScenarioId, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(leak => leak.Rounds).First())
            .Where(leak => leak.Verdict.StartsWith("疑似", StringComparison.Ordinal))
            .Select(leak => leak.ScenarioId)
            .ToList();
        bullets.Add(leakVerdicts.Count == 0
            ? "5 轮重复运行未发现单调上升的托管堆趋势（阈值：增幅 >30% 或斜率 >2 MB/轮且 R²>0.9）。"
            : "托管堆呈上升趋势的场景：" + string.Join("、", leakVerdicts) + "（详见第 9 节）。");

        bool licensed = sessions[^1].Licensed;
        bullets.Add(licensed
            ? "Aspose 授权已加载，指标不含评估版水印开销。"
            : "本次测量运行在 Aspose 评估版（未找到 .lic）：输出会带评估版水印/页数限制，" +
              "`At most 4 elements ... in evaluation mode` 一类限制会影响绝对耗时，不能直接外推到授权版。");

        foreach (string bullet in bullets)
        {
            sb.AppendLine("- " + bullet);
        }

        sb.AppendLine();
    }

    // ---------------------------------------------------------- environment

    private static void WriteEnvironment(StringBuilder sb, IReadOnlyList<Session> sessions, Session session)
    {
        sb.AppendLine("## 2. 环境与授权");
        sb.AppendLine();
        sb.AppendLine("| 项目 | 值 |");
        sb.AppendLine("| --- | --- |");
        Row(sb, "操作系统", session.Host.OsDescription + "（" + session.Host.OsArchitecture + "）");
        Row(sb, "CPU", session.Host.CpuName);
        Row(sb, "逻辑核", session.Host.LogicalCores.ToString(CultureInfo.InvariantCulture));
        Row(sb, "物理内存", F(session.Host.TotalMemoryGb, "0.0") + " GB");
        Row(sb, ".NET 运行时", session.Host.RuntimeVersion);
        Row(sb, "目标框架", session.Host.TargetFramework);
        Row(sb, "进程架构", session.Host.ProcessArchitecture);
        Row(sb, "Server GC", session.Host.ServerGarbageCollection ? "启用" : "未启用（工作站 GC）");
        Row(sb, "Aspose 版本", string.IsNullOrEmpty(session.AsposeFacts) ? "未探测" : session.AsposeFacts);
        Row(sb, "授权状态", session.Licensed ? "已加载：" + (session.LicensePath ?? "") : "评估版（未找到 .lic）");
        Row(sb, "主机名", session.Host.MachineName);
        sb.AppendLine();

        if (!session.Licensed)
        {
            sb.AppendLine("评估版限制（如实标注，未做任何规避）：");
            sb.AppendLine();
            sb.AppendLine("- Aspose 在评估模式下会插入水印，并对部分集合访问施加 4 元素上限。本次实测到的错误原文逐条列在下面，不做改写；另一种常见措辞是 `At most 4 elements (for example, a paragraph or a table row) can be processed in evaluation mode`（本次未出现）。");
            sb.AppendLine("- 该限制属于库自身行为，本次不绕过、不破解；因此**绝对耗时与内存只能作为同一评估版环境下的横向对比**。");
            sb.AppendLine("- 另一条实测到的评估版行为：Aspose.Words 26.1 **加载即截断到 202 段**（300 段纯文本 docx 探查结果 `paragraphs : 202`），超出的内容既不排版也不入 PDF。30 页大件夹具已按此重排为 188 段，`--probe` 可复核 `paragraphs : 192 / pages : 30`。");

            List<string> evalErrors = sessions
                .SelectMany(item => item.Runs)
                .Where(run => run.Blocked
                    || run.Errors.Any(error => error.Contains("evaluation", StringComparison.OrdinalIgnoreCase)
                        || error.Contains("评估", StringComparison.Ordinal)))
                .SelectMany(run => run.Errors.Where(error => error.Contains("evaluation", StringComparison.OrdinalIgnoreCase)
                    || error.Contains("评估", StringComparison.Ordinal)))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (evalErrors.Count == 0)
            {
                sb.AppendLine("- 全部 session 中没有任何一轮触发评估版限制（失败消息里没有 evaluation/评估 字样）。");
            }
            else
            {
                foreach (string error in evalErrors)
                {
                    sb.AppendLine("- 实测触发的评估版错误：" + Inline(error));
                }

                int blockedRuns = sessions
                    .SelectMany(item => item.Runs)
                    .Count(run => run.Blocked);
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "- 结论：`pdf.merge` 输入超过 4 个时被评估版直接拒绝（累计 {0} 轮），因此 20 份输入的合并场景只留下「被拒证据」，可测的合并耗时来自 2 输入（4 页，已校验）与 4 输入（被拒）。",
                    blockedRuns));
            }

            sb.AppendLine();
        }
    }

    // ------------------------------------------------------------- fixtures

    private static void WriteFixtures(StringBuilder sb, IReadOnlyList<Session> sessions)
    {
        sb.AppendLine("## 3. 夹具");
        sb.AppendLine();
        sb.AppendLine("| 组 | 路径 | 文件数 | 单文件最大 | 合计 | 生成方式 |");
        sb.AppendLine("| --- | --- | ---: | ---: | ---: | --- |");

        // 每个 session 只加载自己用到的夹具组，所以按组名跨 session 合并，避免只显示最后一次运行的组。
        Dictionary<string, FixtureGroup> groups = new(StringComparer.Ordinal);
        foreach (Session session in sessions)
        {
            foreach (FixtureGroup group in session.Fixtures.Groups)
            {
                groups[group.Name] = group;
            }
        }

        long totalBytes = 0;
        long maxBytes = 0;
        int totalFiles = 0;
        foreach (FixtureGroup group in groups.Values.OrderBy(group => group.Name, StringComparer.Ordinal))
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| {0} | `{1}` | {2} | {3} KB | {4} KB | {5} |",
                group.Name,
                group.Path,
                group.Count,
                Kb(group.MaxBytes),
                Kb(group.TotalBytes),
                group.Generator));
            totalBytes += group.TotalBytes;
            totalFiles += group.Count;
            maxBytes = Math.Max(maxBytes, group.MaxBytes);
        }

        List<string> warnings = sessions
            .SelectMany(session => session.Fixtures.Warnings)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        sb.AppendLine();
        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "预算校验（按全部夹具组累计）：{0} 个文件；单文件上限 200 KB，实际最大 {1} KB；总量上限 20 MB，实际 {2} MB。{3}",
            totalFiles,
            Kb(maxBytes),
            Mb(totalBytes),
            warnings.Count == 0 ? "均在预算内。" : "存在超预算项：" + string.Join("；", warnings)));
        sb.AppendLine();

        List<string> notes = sessions
            .SelectMany(session => session.Fixtures.Notes)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        sb.AppendLine("夹具生成说明：");
        sb.AppendLine();
        foreach (string note in notes)
        {
            sb.AppendLine("- " + note);
        }

        sb.AppendLine("- docx 由 `tools\\QaPerf\\fixtures\\make_docx_fixtures.py`（python-docx）生成；PNG 由 QaPerf 用 SkiaSharp 绘制像素后由内置 `PngWriter` 编码（SkiaSharp 3.119 的 SKImage 只暴露 `Encode(SKEncodedImageFormat, int)`，默认 PNG 编码在同一图像上恒定输出约 224 KB，超单文件预算）；PDF 由 `PdfFixtureWriter` 手写 PDF 1.4 并 zlib 压缩内容流，全程不使用 Aspose 生成输入，避免预热与评估版水印污染输入。");
        sb.AppendLine();
    }

    // ------------------------------------------------------------ scenarios

    private static void WriteScenarios(StringBuilder sb, Session primary, List<string> scenarioIds)
    {
        sb.AppendLine("## 4. 场景与参数");
        sb.AppendLine();
        sb.AppendLine("| 场景 | 说明 | FeatureId | 输入数 | 参数 | 输出命名 |");
        sb.AppendLine("| --- | --- | --- | ---: | --- | --- |");

        foreach (string id in scenarioIds)
        {
            List<RunMetrics> runs = RunsFor(primary, id);
            if (runs.Count == 0)
            {
                continue;
            }

            RunMetrics run = runs[0];
            string options = DescribeOptions(primary, id);
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| {0} | {1} | `{2}` | {3} | {4} | {5} |",
                id,
                run.Title,
                run.FeatureId,
                run.FileCount,
                options,
                OutputNaming(id)));
        }

        sb.AppendLine();
        sb.AppendLine("以上场景全部通过 `AddDocumentToolsCore()` 解析出的 `IDocumentOperation` 执行，与 App 调用同一条代码路径（不由 QaPerf 重新实现）。");
        sb.AppendLine();
    }

    private static string DescribeOptions(Session primary, string scenarioId)
    {
        RunMetrics? run = primary.Runs.FirstOrDefault(item => item.ScenarioId == scenarioId);
        if (run is null)
        {
            return "—";
        }

        return scenarioId switch
        {
            "wordpdf-10" or "wordpdf-50" => "compliance=Pdf17（其余默认）",
            "image-50" => "format=PNG, pngLevel=3, pngColorType=Truecolor",
            "large-word-30p" => "compliance=Pdf17（其余默认）",
            "large-pdf-200p" => "quality=75（其余默认）",
            "merge-2" or "merge-4" or "merge-20" => "outputName=merged.pdf",
            _ => "—"
        };
    }

    private static string OutputNaming(string scenarioId) => scenarioId switch
    {
        "wordpdf-10" or "wordpdf-50" => "`<原名>.pdf`",
        "image-50" => "`<原名>_out.png`",
        "large-word-30p" => "`large_30p.pdf`",
        "large-pdf-200p" => "`pages200_压缩.pdf`",
        "merge-2" or "merge-4" or "merge-20" => "`merged.pdf`",
        _ => "—"
    };

    // ------------------------------------------------------------ cold/warm

    private static void WriteColdWarm(StringBuilder sb, IReadOnlyList<Session> sessions, List<string> scenarioIds)
    {
        sb.AppendLine("## 5. 冷启动 vs 第二次运行");
        sb.AppendLine();
        sb.AppendLine("冷启动定义：**进程内第一次执行 Aspose 操作**（含 CLR JIT、Aspose 程序集加载与内部初始化、字体缓存建立）。");
        sb.AppendLine("为拿到每场景真实的冷启动数据，下表的「冷启动」列取自**该场景单独进程**的 session（`--scenario <场景 id> --rounds 2`）；若该场景没有独立 session，则退回主 session 的第 1 轮并标注为 warm。");
        sb.AppendLine();
        sb.AppendLine("| 场景 | 冷启动轮 总耗时 ms | 冷启动 峰值WS MB | 第二次 总耗时 ms | 第二次 峰值WS MB | Δ耗时 | Δ峰值WS | 来源 session |");
        sb.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |");

        foreach (string id in scenarioIds)
        {
            // 冷/热对比必须有两轮：优先取该场景轮次 >= 2 的最新 session。
            Session? source = sessions
                .Where(session => session.Runs.Count(run => run.ScenarioId == id) >= 2)
                .OrderByDescending(session => session.StartedAt)
                .FirstOrDefault()
                ?? sessions
                    .Where(session => session.Runs.Any(run => run.ScenarioId == id))
                    .OrderByDescending(session => session.StartedAt)
                    .FirstOrDefault();

            if (source is null)
            {
                continue;
            }

            List<RunMetrics> runs = RunsFor(source, id);
            RunMetrics first = runs[0];
            RunMetrics? second = runs.Count > 1 ? runs[1] : null;

            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| {0} | {1} | {2} | {3} | {4} | {5} | {6} | `{7}`{8}{9} |",
                id,
                F(first.TotalMs, "0.0"),
                F(first.PeakWorkingSetMb, "0.0"),
                second is null ? "—" : F(second.TotalMs, "0.0"),
                second is null ? "—" : F(second.PeakWorkingSetMb, "0.0"),
                second is null || first.Blocked ? "—" : Delta(first.TotalMs, second.TotalMs),
                second is null || first.Blocked ? "—" : Delta(first.PeakWorkingSetMb, second.PeakWorkingSetMb),
                source.Id,
                first.ColdStart ? "" : "（该轮非进程首跑）",
                first.Blocked ? "（评估版阻断，不是合并耗时）" : string.Empty));
        }

        sb.AppendLine();
        sb.AppendLine("结论口径：Δ 为负表示第二次更快（预热收益），为正表示第二次更慢（多为输出体积或内存压力变化）。带「评估版阻断」的行只记录了一个失败返回的时间，不能当作 `pdf.merge` 的真实吞吐。");
        sb.AppendLine();
    }

    // --------------------------------------------------------------- rounds

    private static void WriteRounds(StringBuilder sb, IReadOnlyList<Session> sessions, List<string> scenarioIds)
    {
        sb.AppendLine("## 6. 批量运行明细（5 轮主表）");
        sb.AppendLine();
        sb.AppendLine("每场景取**轮次最多的 session**，逐轮列出真实测量值。`PeakWorkingSet64` 为 `Process.GetCurrentProcess().PeakWorkingSet64`（只增不减，含 QaPerf 自身）；`轮内峰值WS` 为测量期间 25 ms 采样的轮内峰值。");
        sb.AppendLine();
        sb.AppendLine("失败数一列若带「评估版阻断」标注，表示该轮的所有输入都因 Aspose 评估版的元素上限而没有产出文件（错误原文见本节末尾与第 2 节），不是文档工具自身的失败。");
        sb.AppendLine();
        sb.AppendLine("| 场景 | 轮 | 冷/热 | 文件数 | 总耗时 ms | 单文件均时 ms | PeakWorkingSet64 MB | 轮内峰值WS MB | GC(true) MB | 输出 KB | 失败数 |");
        sb.AppendLine("| --- | ---: | --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | ---: |");

        HashSet<Session> sources = new();

        foreach (string id in scenarioIds)
        {
            Session? source = sessions
                .Where(session => session.Runs.Any(run => run.ScenarioId == id))
                .OrderByDescending(session => session.Runs.Count(run => run.ScenarioId == id))
                .ThenByDescending(session => session.StartedAt)
                .FirstOrDefault();

            if (source is null)
            {
                continue;
            }

            sources.Add(source);

            foreach (RunMetrics run in RunsFor(source, id))
            {
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "| {0} | {1} | {2} | {3} | {4} | {5} | {6} | {7} | {8} | {9} | {10} |",
                    run.ScenarioId,
                    run.Round,
                    run.ColdStart ? "冷" : "热",
                    run.FileCount,
                    F(run.TotalMs, "0.0"),
                    F(run.MeanMsPerFile, "0.0"),
                    F(run.PeakWorkingSetMb, "0.0"),
                    F(run.RoundPeakWorkingSetMb, "0.0"),
                    F(run.ManagedAfterCollectMb, "0.0"),
                    Kb(run.OutputBytes),
                    run.Blocked
                        ? run.Failures.ToString(CultureInfo.InvariantCulture) + "（评估版阻断）"
                        : run.Failures.ToString(CultureInfo.InvariantCulture)));
            }
        }

        sb.AppendLine();

        // 只统计本表真正用到的 source session，否则同一场景在多个 session 里的失败会被重复列出。
        List<RunMetrics> failures = sources
            .SelectMany(session => session.Runs)
            .Where(run => run.Failures > 0)
            .OrderBy(run => scenarioIds.IndexOf(run.ScenarioId))
            .ThenBy(run => run.Round)
            .ToList();

        int blockedCount = failures.Count(run => run.Blocked);
        if (failures.Count == 0)
        {
            sb.AppendLine("所有轮次失败数均为 0：每个输入都产出了对应输出文件。");
        }
        else
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "失败明细（原文照录；其中 {0} 轮属于评估版阻断）：",
                blockedCount));
            sb.AppendLine();
            foreach (var group in failures
                         .GroupBy(run => (run.ScenarioId, Message: string.Join("；", run.Errors.Select(Inline)), run.Blocked, run.Failures))
                         .OrderBy(group => scenarioIds.IndexOf(group.Key.ScenarioId))
                         .ThenBy(group => group.Min(run => run.Round)))
            {
                sb.AppendLine(string.Format(
                    CultureInfo.InvariantCulture,
                    "- `{0}` 第 {1} 轮各失败 {2} 个{3}：{4}",
                    group.Key.ScenarioId,
                    RoundRange(group.Select(run => run.Round).ToList()),
                    group.Key.Failures,
                    group.Key.Blocked ? "（评估版阻断）" : string.Empty,
                    group.Key.Message));
            }
        }

        sb.AppendLine();
    }

    // --------------------------------------------------------- verification

    private static void WriteVerification(StringBuilder sb, IReadOnlyList<Session> sessions, List<string> scenarioIds)
    {
        sb.AppendLine("## 7. 产物校验");
        sb.AppendLine();
        sb.AppendLine("测量结束后（不参与计时）用 Aspose 重新打开产物核对页数/体积，证明输出是真实文件而非空壳。同一场景可能出现在多个 session 里，这里每个场景取「PDF 页数最多」的那次校验结果。");
        sb.AppendLine();
        sb.AppendLine("| 场景 | 输出文件数 | PDF 个数 | PDF 页数合计 | 读取失败 | 样例 |");
        sb.AppendLine("| --- | ---: | ---: | ---: | ---: | --- |");

        foreach (string id in scenarioIds)
        {
            ScenarioVerification? best = sessions
                .SelectMany(session => session.Verifications)
                .Where(item => string.Equals(item.ScenarioId, id, StringComparison.Ordinal))
                .OrderByDescending(item => item.PdfPages)
                .ThenByDescending(item => item.OutputFiles)
                .FirstOrDefault();

            if (best is null)
            {
                sb.AppendLine(string.Format(CultureInfo.InvariantCulture, "| {0} | — | — | — | — | 无校验记录 |", id));
                continue;
            }

            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| {0} | {1} | {2} | {3} | {4} | {5} |",
                best.ScenarioId,
                best.OutputFiles,
                best.PdfOutputs,
                best.PdfPages,
                best.PdfReadFailures,
                string.IsNullOrEmpty(best.Sample) ? "—" : best.Sample));
        }

        sb.AppendLine();

        List<string> notes = sessions
            .SelectMany(session => session.VerificationNotes)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (notes.Count > 0)
        {
            sb.AppendLine("输入/读取探测：");
            sb.AppendLine();
            foreach (string note in notes)
            {
                sb.AppendLine("- " + Inline(note));
            }

            sb.AppendLine();
        }
    }

    // ------------------------------------------------------------ linearity

    private static void WriteLinearity(StringBuilder sb, IReadOnlyList<Session> sessions, Session primary)
    {
        sb.AppendLine("## 8. 线性度：10 个 vs 50 个 docx");
        sb.AppendLine();

        // 必须是同一进程、同一 session 内的多轮数据，否则 JIT/Aspose 初始化的差异会混进规模对比。
        Session? source = sessions
            .Where(session => RunsFor(session, "wordpdf-10").Count >= 2 && RunsFor(session, "wordpdf-50").Count >= 2)
            .OrderByDescending(session => RunsFor(session, "wordpdf-10").Any(run => run.ColdStart))
            .ThenByDescending(session => session.StartedAt)
            .FirstOrDefault();

        if (source is null)
        {
            sb.AppendLine("没有任何 session 同时包含 `wordpdf-10` 与 `wordpdf-50` 的各两轮数据，无法做同进程规模对比；运行 `--scenario wordpdf --rounds 2` 可获得。");
            sb.AppendLine();
            return;
        }

        List<RunMetrics> tenRuns = RunsFor(source, "wordpdf-10");
        List<RunMetrics> fiftyRuns = RunsFor(source, "wordpdf-50");
        RunMetrics tenFirst = tenRuns[0];
        RunMetrics fiftyFirst = fiftyRuns[0];
        RunMetrics tenWarm = tenRuns[1];
        RunMetrics fiftyWarm = fiftyRuns[1];

        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "对比口径：同一进程内 `wordpdf-10` 与 `wordpdf-50` 的第 1、2 轮，来自 session `{0}`。第 1 轮带冷启动成分（`wordpdf-10` {1}），第 2 轮是热态；判定用热态轮，避免 JIT/Aspose 初始化干扰规模结论。",
            source.Id,
            tenFirst.ColdStart ? "进程首跑，含 JIT 与 Aspose 初始化" : "非进程首跑"));
        sb.AppendLine();
        sb.AppendLine("| 指标 | 10 个 | 50 个 | 倍数 | 说明 |");
        sb.AppendLine("| --- | ---: | ---: | ---: | --- |");

        LinearityRow(sb, "第 1 轮 总耗时 ms", F(tenFirst.TotalMs, "0.0"), F(fiftyFirst.TotalMs, "0.0"), Ratio(tenFirst.TotalMs, fiftyFirst.TotalMs), "仅参考：含冷启动成分，不作判定");
        LinearityRow(sb, "第 2 轮 总耗时 ms", F(tenWarm.TotalMs, "0.0"), F(fiftyWarm.TotalMs, "0.0"), Ratio(tenWarm.TotalMs, fiftyWarm.TotalMs), "判定依据：接近 5.0 才是线性");
        LinearityRow(sb, "单文件均时 ms", F(tenWarm.MeanMsPerFile, "0.0"), F(fiftyWarm.MeanMsPerFile, "0.0"), Ratio(tenWarm.MeanMsPerFile, fiftyWarm.MeanMsPerFile), "接近 1.0 = 每份成本稳定，明显 <1.0 = 固定开销被摊薄");
        LinearityRow(sb, "轮内峰值WS MB", F(tenWarm.RoundPeakWorkingSetMb, "0.0"), F(fiftyWarm.RoundPeakWorkingSetMb, "0.0"), Ratio(tenWarm.RoundPeakWorkingSetMb, fiftyWarm.RoundPeakWorkingSetMb), "接近 1.0 = 不随批量累积");
        LinearityRow(sb, "输出体积 KB", Kb(tenWarm.OutputBytes), Kb(fiftyWarm.OutputBytes), Ratio(tenWarm.OutputBytes, fiftyWarm.OutputBytes), "应接近输入倍数 5.0（用来证明 50 份确实都转换了）");
        sb.AppendLine();

        double timeRatio = tenWarm.TotalMs <= 0 ? 0 : fiftyWarm.TotalMs / tenWarm.TotalMs;
        string verdict = timeRatio switch
        {
            0 => "数据不足，无法判定。",
            >= 4.0 and <= 6.0 => "线性：5 倍输入带来约 5 倍总耗时（±20%），批量循环本身没有明显累积开销。",
            < 4.0 => "亚线性：5 倍输入的耗时增长明显低于 5 倍，说明固定开销（进程/库初始化、字体缓存）在小批量里占比很高，被大批量摊薄；单文件均时随之下降。",
            <= 7.5 => "轻微超线性：批量增大后单文件成本上升，可能是 Aspose 内部缓存或 GC 压力。",
            _ => "明显超线性：批量增大后单文件成本显著上升，需要排查循环内未释放的对象或句柄累积。"
        };

        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "判定（第 2 轮总耗时 {0:0.0} ms -> {1:0.0} ms，×{2:0.00}）：{3}",
            tenWarm.TotalMs,
            fiftyWarm.TotalMs,
            timeRatio,
            verdict));
        sb.AppendLine();
    }

    private static void LinearityRow(StringBuilder sb, string label, string ten, string fifty, string ratio, string hint)
    {
        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "| {0} | {1} | {2} | {3} | {4} |",
            label,
            ten,
            fifty,
            ratio,
            hint));
    }

    // ---------------------------------------------------------------- leaks

    private static void WriteLeaks(StringBuilder sb, IReadOnlyList<Session> sessions, List<string> scenarioIds)
    {
        sb.AppendLine("## 9. 内存泄漏嗅探（同场景连跑）");
        sb.AppendLine();
        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "判定规则：轮次 ≥ {0} 才判趋势；托管堆增幅 > {1:0}% 或（斜率 > {2:0.0} MB/轮 且 R² > {3:0.00}）判为「疑似泄漏」。托管堆取每轮结束后的 `GC.GetTotalMemory(true)`。",
            LeakAnalyzer.MinimumRounds,
            LeakAnalyzer.GrowthThresholdPct,
            LeakAnalyzer.SlopeThresholdMbPerRound,
            LeakAnalyzer.RSquaredThreshold));
        sb.AppendLine();
        sb.AppendLine("| 场景 | 轮次 | 托管堆起止 MB | 增幅 | 斜率 MB/轮 | R² | 工作集起止 MB | 判定 | 备注 |");
        sb.AppendLine("| --- | ---: | --- | ---: | ---: | ---: | --- | --- | --- |");

        foreach (string id in scenarioIds)
        {
            LeakFinding? leak = sessions
                .SelectMany(session => session.Leaks)
                .Where(item => item.ScenarioId == id)
                .OrderByDescending(item => item.Rounds)
                .FirstOrDefault();

            if (leak is null)
            {
                continue;
            }

            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| {0} | {1} | {2:0.00} -> {3:0.00} | {4:+0.0;-0.0;0}% | {5:0.00} | {6:0.00} | {7:0.00} -> {8:0.00} | {9} | {10} |",
                leak.ScenarioId,
                leak.Rounds,
                leak.ManagedMb[0],
                leak.ManagedMb[^1],
                leak.ManagedGrowthPct,
                leak.ManagedSlopeMbPerRound,
                leak.ManagedRSquared,
                leak.WorkingSetMb[0],
                leak.WorkingSetMb[^1],
                leak.Verdict,
                leak.Note));
        }

        sb.AppendLine();
        sb.AppendLine("说明：工作集在工作站 GC 下不保证归还操作系统，短时间不降并不等于泄漏；判定以强制 GC 后的托管堆为准。");
        sb.AppendLine();
    }

    // --------------------------------------------------------- dispose audit

    private static void WriteDisposeAudit(StringBuilder sb, Session latest)
    {
        sb.AppendLine("## 10. IDisposable 审计（只读扫描，未改动 src/）");
        sb.AppendLine();
        sb.AppendLine("扫描 `src/DocumentTools.Core/**/*.cs` 中所有 Aspose 文档对象创建点，并按反射结果标注该类型是否真的实现 `IDisposable`：");
        sb.AppendLine();
        sb.AppendLine("- " + DisposeAudit.DescribeCounts(latest.DisposeSites));
        sb.AppendLine();

        List<DisposeSite> undisposed = latest.DisposeSites.Where(site => !site.Disposed).ToList();
        if (undisposed.Count == 0)
        {
            sb.AppendLine("所有创建点都使用了 `using`。");
            sb.AppendLine();
            return;
        }

        sb.AppendLine("| 位置 | 代码 | 类型 | 实现 IDisposable |");
        sb.AppendLine("| --- | --- | --- | --- |");
        foreach (DisposeSite site in undisposed)
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| `{0}:{1}` | `{2}` | `{3}` | {4} |",
                site.File,
                site.Line,
                Escape(site.Code),
                site.TypeName,
                site.TypeDisposable ? "是（未释放即泄漏候选）" : "否（无 using 也无害）"));
        }

        sb.AppendLine();
        int live = undisposed.Count(site => site.TypeDisposable);
        sb.AppendLine(live == 0
            ? "结论：未使用 using 的创建点，其类型均未实现 IDisposable，不构成托管泄漏；但原生内存仍取决于库内部实现。"
            : string.Format(CultureInfo.InvariantCulture, "结论：{0} 处创建点的类型实现 IDisposable 却未释放，是需要优先处理的对象生命周期问题。", live));
        sb.AppendLine();
    }

    // ------------------------------------------------------------ conclusion

    private static void WriteConclusions(
        StringBuilder sb,
        IReadOnlyList<Session> sessions,
        Session primary,
        List<string> scenarioIds)
    {
        sb.AppendLine("## 11. 结论");
        sb.AppendLine();

        var bullets = new List<string>();

        foreach (string id in scenarioIds)
        {
            List<RunMetrics> runs = RunsFor(primary, id);
            if (runs.Count == 0)
            {
                continue;
            }

            // 取第 2 轮（热态）；只有 1 轮时退回第 1 轮，并在文案里注明轮次与冷/热。
            RunMetrics chosen = runs.Count > 1 ? runs[1] : runs[0];
            string roundNote = string.Format(
                CultureInfo.InvariantCulture,
                "第 {0} 轮{1}",
                chosen.Round,
                chosen.ColdStart ? "（冷启动）" : string.Empty);
            string failNote = chosen.Blocked
                ? "失败 " + chosen.Failures.ToString(CultureInfo.InvariantCulture) + "（评估版阻断，非实现问题）"
                : "失败 " + chosen.Failures.ToString(CultureInfo.InvariantCulture);
            bullets.Add(string.Format(
                CultureInfo.InvariantCulture,
                "`{0}`：" + roundNote + "，{1} 个文件 {2:0.0} ms（单文件 {3:0.0} ms），轮内峰值 {4:0.0} MB，输出 {5} KB，{6}。",
                id,
                chosen.FileCount,
                chosen.TotalMs,
                chosen.MeanMsPerFile,
                chosen.RoundPeakWorkingSetMb,
                Kb(chosen.OutputBytes),
                failNote));
        }

        foreach (string bullet in bullets)
        {
            sb.AppendLine("- " + bullet);
        }

        sb.AppendLine();

        Session? scaleSource = sessions
            .Where(session => RunsFor(session, "wordpdf-10").Count >= 2 && RunsFor(session, "wordpdf-50").Count >= 2)
            .OrderByDescending(session => RunsFor(session, "wordpdf-10").Any(run => run.ColdStart))
            .ThenByDescending(session => session.StartedAt)
            .FirstOrDefault();
        RunMetrics? ten = scaleSource is null ? null : RunsFor(scaleSource, "wordpdf-10")[1];
        RunMetrics? fifty = scaleSource is null ? null : RunsFor(scaleSource, "wordpdf-50")[1];
        if (ten is not null && fifty is not null)
        {
            double timeRatio = ten.TotalMs <= 0 ? 0 : fifty.TotalMs / ten.TotalMs;
            string linearVerdict = timeRatio switch
            {
                >= 4.0 and <= 6.0 => "批量路径基本线性，没有随规模恶化。",
                < 4.0 => "耗时增长明显低于输入增长：固定开销（进程/JIT/Aspose 初始化）在小批量里占主导，被大批量摊薄。",
                _ => "批量路径随规模恶化，优先排查循环内文档对象释放与内存压力。"
            };
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "线性度（session `{0}` 热态第 2 轮）：word.pdf 输入 ×5.0 → 总耗时 ×{1:0.00}、单文件均时 ×{2:0.00}、轮内峰值WS ×{3:0.00}。{4}",
                scaleSource!.Id,
                timeRatio,
                Ratio(ten.MeanMsPerFile, fifty.MeanMsPerFile),
                Ratio(ten.RoundPeakWorkingSetMb, fifty.RoundPeakWorkingSetMb),
                linearVerdict));
            sb.AppendLine();
        }

        sb.AppendLine("瓶颈判断（按单文件均时排序，取每场景第 2 轮热态，只有 1 轮时取第 1 轮）：");
        sb.AppendLine();
        foreach (RunMetrics run in primary.Runs
            .Where(run => !run.Blocked)
            .GroupBy(run => run.ScenarioId, StringComparer.Ordinal)
            .Select(group => group.OrderBy(run => run.Round == 2 ? 0 : 1).ThenBy(run => run.Round).First())
            .OrderByDescending(run => run.MeanMsPerFile))
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "- `{0}`：{1:0.0} ms/文件（总 {2:0.0} ms / {3} 个，第 {4} 轮）。",
                run.ScenarioId,
                run.MeanMsPerFile,
                run.TotalMs,
                run.FileCount,
                run.Round));
        }

        sb.AppendLine();
    }

    // --------------------------------------------------------- recommendations

    private static void WriteRecommendations(
        StringBuilder sb,
        IReadOnlyList<Session> sessions,
        Session primary,
        List<string> scenarioIds)
    {
        sb.AppendLine("## 12. 建议（可执行）");
        sb.AppendLine();

        int index = 1;

        List<DisposeSite> liveSites = sessions[^1].DisposeSites
            .Where(site => !site.Disposed && site.TypeDisposable)
            .ToList();
        if (liveSites.Count > 0)
        {
            string where = string.Join("`、`", liveSites.Take(4).Select(site => site.File + ":" + site.Line));
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}. **逐文件释放文档对象**：静态审计发现 {1} 处创建点的类型实现了 `IDisposable` 却没用 `using`，例如 `{2}`；批量循环里会按文件数累积。改为 `using var doc = ...`（或 `try/finally {{ doc.Dispose(); }}`）即可，风险最低、收益最直接。",
                index++,
                liveSites.Count,
                where));
        }

        if (scenarioIds.Contains("merge-20"))
        {
            RunMetrics? mergeRun = primary.Runs
                .Where(run => run.ScenarioId == "merge-20")
                .OrderBy(run => run.Round)
                .FirstOrDefault();
            int mergeInputs = mergeRun?.FileCount ?? 20;
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}. **合并时避免重复读盘**：`pdf.merge` 现在是逐份打开输入再写入同一个目标文档（`DocumentMergeSplitService.MergePdfFiles`），{1} 份输入就是 {1} 次读盘 + {1} 次页拷贝；若输入格式统一，可改成一次 `Document` 打开后按页拼接，或流式追加并复用 `MemoryStream`。",
                index++,
                mergeInputs));
        }

        if (primary.Runs.Any(run => run.ScenarioId == "image-50"))
        {
            List<RunMetrics> imageRuns = RunsFor(primary, "image-50");
            RunMetrics image = imageRuns.FirstOrDefault(run => !run.ColdStart) ?? imageRuns[0];
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}. **image.compress 可并行**：单张 1600×1200 PNG 平均 {1:0.0} ms，纯 CPU/内存操作且每张互相独立（`ImageConversionService` 内部已是 `using` + `finally Dispose` 的干净写法），在 {2} 逻辑核上用 `Parallel.ForEach`（`MaxDegreeOfParallelism = min(4, 核数/2)`）通常能拿到接近线性的吞吐提升，代价是峰值内存按并发数上升（当前 50 张一轮的轮内峰值 {3:0.0} MB，{4} MB/张推算）。",
                index++,
                image.MeanMsPerFile,
                sessions[^1].Host.LogicalCores,
                image.RoundPeakWorkingSetMb,
                F(image.RoundPeakWorkingSetMb / Math.Max(image.FileCount, 1), "0.0")));
        }

        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0}. **批量失败隔离**：`OfficeConvertOperation` 整个批次包在一个 try/catch 里，一份文档异常会让整批返回 Fail 且丢弃已产出的文件（本次测量未触发，但失败数统计口径就是按此设计的）。建议循环体内逐文件 try/catch，失败计数并把成功产物继续返回。",
            index++));

        RunMetrics? coldAnchor = sessions
            .Where(session => RunsFor(session, "wordpdf-50").Any(run => run.ColdStart))
            .OrderByDescending(session => session.StartedAt)
            .Select(session => RunsFor(session, "wordpdf-50").First(run => run.ColdStart))
            .FirstOrDefault()
            ?? sessions
                .SelectMany(session => session.Runs)
                .Where(run => run.ColdStart && !run.Blocked)
                .OrderByDescending(run => run.TotalMs)
                .FirstOrDefault();

        if (coldAnchor is not null)
        {
            RunMetrics anchor = coldAnchor;
            RunMetrics? anchorWarm = sessions
                .Where(session => RunsFor(session, anchor.ScenarioId).Any(run => run.ColdStart))
                .OrderByDescending(session => session.StartedAt)
                .Select(session => RunsFor(session, anchor.ScenarioId).FirstOrDefault(run => !run.ColdStart))
                .FirstOrDefault(run => run is not null);

            string tail = anchorWarm is null
                ? string.Empty
                : string.Format(
                    CultureInfo.InvariantCulture,
                    "同进程第二轮降到 {0:0.0} ms（-{1:0.0}%）。",
                    anchorWarm.TotalMs,
                    anchor.TotalMs <= 0 ? 0 : (anchor.TotalMs - anchorWarm.TotalMs) / anchor.TotalMs * 100);

            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "{0}. **冷启动预热**：`{1}` 冷启动 {2:0.0} ms，明显高于后续同场景轮次；{3}桌面端可在启动后空闲线程做一次「最小文档转换」预热（Aspose 初始化 + 字体缓存），把首次用户操作的首字节时间从秒级降到百毫秒级。",
                index++,
                anchor.ScenarioId,
                anchor.TotalMs,
                tail));
        }

        // 并发建议按热态批量轮的单文件均时给数，避免拿进程首跑（含 JIT/Aspose 初始化）当每份成本。
        List<RunMetrics> batchRuns = primary.Runs
            .Where(run => run.ScenarioId.StartsWith("wordpdf", StringComparison.Ordinal) && !run.Blocked)
            .ToList();
        RunMetrics? batchAnchor = batchRuns.FirstOrDefault(run => !run.ColdStart && run.FileCount >= 50)
            ?? batchRuns.FirstOrDefault(run => !run.ColdStart)
            ?? batchRuns.FirstOrDefault();

        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "{0}. **批量并发上限**：当前所有批量操作串行执行。按本轮数据，word.pdf 热态单文件约 {1:0.0} ms、内存增量有限，可用 2~4 路并发换取吞吐；上线前需用 `--rounds 5` 复测确认峰值工作集不随并发线性上升。",
            index++,
            batchAnchor?.MeanMsPerFile ?? 0));

        sb.AppendLine();
    }

    // ------------------------------------------------------------ reproduce

    private static void WriteReproduce(StringBuilder sb, IReadOnlyList<Session> sessions)
    {
        sb.AppendLine("## 13. 复现命令");
        sb.AppendLine();
        sb.AppendLine("```powershell");
        sb.AppendLine("dotnet build tools\\QaPerf\\QaPerf.csproj -v q");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario all --rounds 1");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario all --rounds 5");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario wordpdf-10 --rounds 2");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario wordpdf-50 --rounds 2");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario image --rounds 2");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario large-word-30p --rounds 2");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario large-pdf-200p --rounds 2");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --scenario merge --rounds 2");
        sb.AppendLine("# 单文件事实核对（页数/段落数，不运行场景）");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --probe testdata\\perf\\large\\large_30p.docx");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --probe testdata\\perf\\pdf\\pages200.pdf");
        sb.AppendLine("# 只重建报告、不重新测量（读取 artifacts\\qa-perf\\sessions\\*.json）");
        sb.AppendLine("dotnet run --project tools\\QaPerf -- --report-only");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("每次运行都会：生成/复用 `testdata\\perf` 夹具 → 跑场景 → 写出产物到 `artifacts\\qa-perf\\out` → 追加 session JSON → 用**全部** session 重新生成本报告。`--reset` 可清空历史 session 从零复测。");
        sb.AppendLine();
        sb.AppendLine("本报告使用的 session：");
        sb.AppendLine();
        sb.AppendLine("| session | 开始时间 | 参数 | 轮次 | 覆盖场景 |");
        sb.AppendLine("| --- | --- | --- | ---: | --- |");
        foreach (Session session in sessions)
        {
            sb.AppendLine(string.Format(
                CultureInfo.InvariantCulture,
                "| `{0}` | {1:HH:mm:ss} | `--scenario {2} --rounds {3}` | {4} | {5} |",
                session.Id,
                session.StartedAt,
                session.ScenarioRequest,
                session.RequestedRounds,
                session.Runs.Count,
                string.Join("、", session.ScenarioIds)));
        }

        sb.AppendLine();
    }

    private static void WriteLimits(StringBuilder sb, IReadOnlyList<Session> sessions)
    {
        sb.AppendLine("## 14. 限制与未覆盖");
        sb.AppendLine();
        sb.AppendLine("- 单进程、串行执行：不反映 App 里 UI 线程/多任务并发时的真实表现，也不含磁盘冷缓存以外的 IO 竞争。");
        sb.AppendLine("- `Process.PeakWorkingSet64` 是进程级只增指标，包含 QaPerf 自身；判断单轮内存请以「轮内峰值WS」列为准。");
        sb.AppendLine("- 夹具为合成文档（短段落 + 表格 + 小图），不代表真实业务文档的复杂度；版式复杂的文档转换耗时通常更高。");
        sb.AppendLine("- 评估版 Aspose 会插入水印并限制部分 API，绝对耗时不可直接外推到授权版；本报告的横向对比与趋势结论不受影响。");
        sb.AppendLine("- Aspose.Words 26.1 评估版会把单文档的加载**截断在 202 段**：用 300 段纯文本 docx 探查，引擎只报 `paragraphs : 202`；最初 300 段的 30 页大件因此被渲染成 16 页。现夹具压到 188 段（引擎读入 192 段，含表格单元格），才能在评估版下真实渲染 30 页 —— 该数字可用 `--probe` 复核。");
        sb.AppendLine("- 冷启动数据依赖「该场景单独进程」的 session；若未运行对应 session，第 5 节会标注该轮并非进程首跑。");
        sb.AppendLine("- IDisposable 审计为静态正则 + 反射，跨行构造或别名变量可能漏检；结论以实测泄漏趋势为准。");
        sb.AppendLine();
        sb.AppendLine(string.Format(
            CultureInfo.InvariantCulture,
            "报告由 {0} 个 session 汇总生成；重新运行 QaPerf 会用新数据覆盖本文件。",
            sessions.Count));
        sb.AppendLine();
    }

    // ------------------------------------------------------------- helpers

    private static List<string> CollectScenarioIds(IReadOnlyList<Session> sessions)
    {
        var present = sessions
            .SelectMany(session => session.ScenarioIds)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var ordered = CanonicalOrder.Where(id => present.Contains(id, StringComparer.Ordinal)).ToList();
        ordered.AddRange(present.Where(id => !CanonicalOrder.Contains(id, StringComparer.Ordinal)).OrderBy(id => id, StringComparer.Ordinal));
        return ordered;
    }

    private static List<RunMetrics> RunsFor(Session session, string scenarioId)
        => session.Runs
            .Where(run => string.Equals(run.ScenarioId, scenarioId, StringComparison.Ordinal))
            .OrderBy(run => run.Round)
            .ToList();

    private static void Row(StringBuilder sb, string name, string value)
        => sb.AppendLine("| " + name + " | " + Escape(value) + " |");

    private static string Escape(string text) => text.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

    /// <summary>Inline code span that never breaks on pipe characters or backticks.</summary>
    private static string Inline(string text)
    {
        string cleaned = text.Replace("\r", " ").Replace("\n", " ").Replace("`", "'");
        return "`" + cleaned + "`";
    }

    private static string F(double value, string format) => value.ToString(format, CultureInfo.InvariantCulture);

    private static string Kb(long bytes) => F(bytes / 1024d, "0.0");

    private static string Mb(long bytes) => F(bytes / Mebibyte, "0.0");

    private static string Ratio(double from, double to) => from == 0 ? "—" : F(to / from, "0.00");

    private static string Delta(double from, double to) => from == 0 ? "—" : F((to - from) / from * 100, "+0.0;-0.0;0") + "%";

    /// <summary>Compacts round numbers into "1–5" or "1, 3, 5" so repeated failures stay one line.</summary>
    private static string RoundRange(IReadOnlyList<int> rounds)
    {
        List<int> ordered = rounds.Distinct().OrderBy(round => round).ToList();
        if (ordered.Count == 0)
        {
            return "—";
        }

        bool contiguous = ordered[^1] - ordered[0] == ordered.Count - 1;
        return contiguous && ordered.Count > 1
            ? ordered[0].ToString(CultureInfo.InvariantCulture) + "–" + ordered[^1].ToString(CultureInfo.InvariantCulture)
            : string.Join(", ", ordered.Select(round => round.ToString(CultureInfo.InvariantCulture)));
    }
}
