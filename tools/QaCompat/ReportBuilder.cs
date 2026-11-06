using System.Text;

namespace QaCompat;

/// <summary>把本次运行的检查结果渲染成 docs\qa-report-compat.md（报告内容与实际运行结果同源）。</summary>
internal static class ReportBuilder
{
    public static string Build(QaContext ctx, IReadOnlyList<(string Key, string Value)> environment)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# 环境缺失降级行为验证报告（QaCompat）");
        sb.AppendLine();
        sb.AppendLine("> 本文件由 `tools\\QaCompat` 在每次运行时自动生成，内容与本次真实执行结果一致。");
        sb.AppendLine();

        sb.AppendLine("## 环境");
        sb.AppendLine();
        sb.AppendLine("| 项 | 值 |");
        sb.AppendLine("| --- | --- |");
        foreach ((string key, string value) in environment)
        {
            sb.AppendLine("| " + Cell(key) + " | " + Cell(value) + " |");
        }

        foreach ((string key, string value) in ctx.Facts)
        {
            sb.AppendLine("| " + Cell(key) + " | " + Cell(value) + " |");
        }

        sb.AppendLine();
        sb.AppendLine("## 矩阵表");
        sb.AppendLine();
        sb.AppendLine("| 检查项 | 组 | 结论 | 实际行为（摘要） |");
        sb.AppendLine("| --- | --- | --- | --- |");
        foreach (Check check in ctx.Checks)
        {
            string summary;
            if (check.Status == CheckStatus.Unverified && check.SkipReason is not null)
            {
                const string prefix = "无法验证：";
                string reason = check.SkipReason.StartsWith(prefix, StringComparison.Ordinal)
                    ? check.SkipReason[prefix.Length..]
                    : check.SkipReason;
                summary = prefix + reason;
            }
            else
            {
                summary = check.Actual;
            }

            sb.AppendLine("| `" + check.Id + "` | " + Cell(check.Group) + " | " + StatusText(check.Status) + " | " + Cell(Fmt.Clip(Fmt.OneLine(summary), 120)) + " |");
        }

        sb.AppendLine();
        sb.AppendLine("统计：PASS " + ctx.PassCount + " / FAIL " + ctx.FailCount + " / 无法验证 " + ctx.UnverifiedCount + "。");
        sb.AppendLine();

        sb.AppendLine("## 每条详情");
        sb.AppendLine();
        foreach (Check check in ctx.Checks)
        {
            sb.AppendLine("### `" + check.Id + "` — " + check.Title);
            sb.AppendLine();
            sb.AppendLine("- **设定方式**：" + check.Setup);
            sb.AppendLine("- **期望行为**：" + check.Expected);
            sb.AppendLine("- **实际行为**：" + (check.Actual.Length == 0 ? "(无)" : check.Actual));
            sb.AppendLine("- **结论**：" + StatusText(check.Status));
            if (check.SkipReason is not null)
            {
                sb.AppendLine("- **无法验证原因**：" + check.SkipReason);
            }

            sb.AppendLine("- **证据**：");
            sb.AppendLine();
            sb.AppendLine("```text");
            foreach (string line in SplitLines(check.Evidence.Length == 0 ? "(无)" : check.Evidence))
            {
                sb.AppendLine(line);
            }

            sb.AppendLine("```");
            if (check.Findings.Count > 0)
            {
                sb.AppendLine("- **关联发现**：" + string.Join("、", check.Findings.Select(f => "[" + f.Severity + "] " + f.Title)));
            }

            if (check.Notes.Count > 0)
            {
                sb.AppendLine("- **备注**：" + string.Join("；", check.Notes));
            }

            sb.AppendLine();
        }

        sb.AppendLine("## 发现的问题（按严重度）");
        sb.AppendLine();
        List<Finding> findings = ctx.AllFindings.ToList();
        if (findings.Count == 0)
        {
            sb.AppendLine("本次运行未发现需要记录的问题。");
            sb.AppendLine();
        }
        else
        {
            for (int i = 0; i < findings.Count; i++)
            {
                Finding finding = findings[i];
                sb.AppendLine((i + 1).ToString() + ". **[" + finding.Severity + "] " + finding.Title + "**");
                sb.AppendLine("   - 详情：" + finding.Detail);
                sb.AppendLine("   - 位置：`" + finding.Location + "`");
                sb.AppendLine("   - 来源检查：`" + finding.CheckId + "`");
                sb.AppendLine();
            }
        }

        sb.AppendLine("## 建议");
        sb.AppendLine();
        foreach (string suggestion in Suggestions(ctx, findings))
        {
            sb.AppendLine("- " + suggestion);
        }

        sb.AppendLine();
        sb.AppendLine("## 复现命令");
        sb.AppendLine();
        sb.AppendLine("```powershell");
        sb.AppendLine("# 在仓库根目录执行（工作目录 D:\\sharp_project\\source\\repos\\DocumentTools）");
        sb.AppendLine("dotnet build tools\\QaCompat\\QaCompat.csproj -v q");
        sb.AppendLine("dotnet run --project tools\\QaCompat -- --all");
        sb.AppendLine("");
        sb.AppendLine("# 只跑某个分组（ace,html,twain,license,readonly,path）");
        sb.AppendLine("dotnet run --project tools\\QaCompat -- --only ace,license");
        sb.AppendLine("");
        sb.AppendLine("# 指定工作目录 / 报告输出位置");
        sb.AppendLine("dotnet run --project tools\\QaCompat -- --all --work testdata\\compat\\work\\manual --report docs\\qa-report-compat.md");
        sb.AppendLine("```");
        sb.AppendLine();
        sb.AppendLine("说明：退出码 1 表示存在 FAIL；「无法验证」不计入失败。本次工作目录见上方「环境」表。");
        return sb.ToString();
    }

    private static IEnumerable<string> Suggestions(QaContext ctx, List<Finding> findings)
    {
        var items = new List<string>();
        bool HasGroup(string group) => ctx.Checks.Any(c => c.Group == group);
        bool HasSeverity(string group, string severity) => findings.Any(f => f.Severity == severity && ctx.Checks.Any(c => c.Id == f.CheckId && c.Group == group));

        items.Add("把 `ImageConvertOperation` / `MergeDocumentOperation` / `SplitDocumentOperation` 的 catch 分支从 `Fail(ex.Message)` 改为「中文场景提示 + 原始消息」两层结构，避免只读目录、文件占用这类系统错误把英文 .NET 文案直接暴露给用户。");

        if (HasGroup("ace"))
        {
            items.Add("`AccessDb.Open()` 目前把所有 provider 失败统一归因为「请安装 ACE」。建议在消息里附上实际尝试过的 provider 列表与首个真实错误（`InnerException` 摘要），并区分「未安装」与「文件损坏/格式不匹配」。");
            items.Add("如果希望 `DOCUMENTTOOLS_SIMULATE_NO_ACE=1` 能覆盖 Core 的考评服务，需要让 Core 的 Access 探测读同一开关（当前仅 `tools\\ExamFixture\\AceAvailability.cs` 读取），否则该开关只能测 ExamFixture 自身。");
        }

        if (HasGroup("html"))
        {
            items.Add("`HtmlToPdfService.ConvertAsync` 的「没有找到浏览器可执行文件」消息应补上安装建议与可配置项（如「请安装 Chrome/Edge，或在参数中指定 chromePath」），当前只有路径占位。");
        }

        if (HasGroup("twain"))
        {
            items.Add("Twain 不可用文案已包含驱动/DSM 建议，可再补一句「无扫描仪时可直接用图片导入」，减少用户误判为程序故障。");
        }

        if (HasGroup("license"))
        {
            items.Add("合并/拆分入口应在执行前检查 `AsposeLicenseLoader.TryLoad().IsLicensed`：评估模式下要么拒绝大文档并提示「未授权，最多 4 元素」，要么在结果消息里明确标注输出受限/带水印，避免静默截断或缺页。");
            items.Add("把评估模式状态暴露到 UI（标题栏/关于页），并在 `DocumentOperationResult.Message` 中统一附加「评估模式」标记，便于用户判断产物是否可信。");
        }

        if (HasGroup("readonly"))
        {
            items.Add("只读目录与文件占用属于高频现场问题：建议在 Operation 层预检输出目录可写性、目标文件是否被独占打开，并在失败时清理 `.tmp` 类中间文件，保证不留半成品。");
        }

        if (HasGroup("path"))
        {
            items.Add("长路径依赖系统 `LongPathsEnabled`。建议在启动时检测该注册表项，未开启时提示「启用 Win32 长路径或缩短输出目录」，而不是等到转换阶段才失败。");
        }

        if (HasSeverity("license", "高"))
        {
            items.Add("优先级最高的修复：评估模式下的合并/拆分必须「失败得明确」或「输出得完整」，当前存在静默缺页/水印风险，属于数据可信度问题。");
        }

        return items;
    }

    private static IEnumerable<string> SplitLines(string text)
        => text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

    private static string StatusText(CheckStatus status) => status switch
    {
        CheckStatus.Pass => "PASS",
        CheckStatus.Fail => "FAIL",
        _ => "无法验证"
    };

    private static string Cell(string text)
        => Fmt.OneLine(text).Replace("|", "\\|");
}
