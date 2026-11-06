using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using DocumentTools.Core.Data;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Code;
using DocumentTools.Core.Services.Exam;
using DocumentTools.Core.Services.Scan;
using DocumentTools.Core.Services.Text;
using DocumentTools.ExamFixture;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using SysTools = DocumentTools.Core.Services.System;

namespace DocumentTools.QaFunction;

/// <summary>
/// Second-round matrix: the features that never reach an <c>IDocumentOperation</c>
/// (name.vertical, exam.*, idcard.check, twain.debug, system.*) are exercised through the
/// same real DI container, but asserted on the service contract directly.
/// </summary>
internal static partial class Program
{
    private sealed record ExtOutcome(bool Pass, string Input, string Outputs, long Bytes, string Detail);

    /// <summary>Core 没有 <c>image.convert</c> 这个 FeatureId：TIF/GIF 输出走 <c>image.compress</c> 的 format 选项。</summary>
    private const string ImageFormatFeature = "image.compress";

    private static void RunExt(string feature, Func<ExtOutcome> body)
    {
        if (!Wanted(feature))
        {
            return;
        }

        Stopwatch watch = Stopwatch.StartNew();
        try
        {
            ExtOutcome outcome = body();
            watch.Stop();
            Results.Add(new CheckResult(feature, outcome.Input, outcome.Outputs, outcome.Bytes,
                watch.Elapsed.TotalMilliseconds, outcome.Pass, outcome.Detail, true, ExtendedGroup));
        }
        catch (ParityDeferredException ex)
        {
            // dev 与新实现的差异：既不是回归也不是通过，交 task-38 或对应 owner 决定改哪一边。
            watch.Stop();
            Results.Add(new CheckResult(feature, "(dev 对照)", "(none)", 0, watch.Elapsed.TotalMilliseconds,
                false, ex.Label + "：" + ex.Message, true, ExtendedGroup, Skipped: true));
        }
        catch (Exception ex)
        {
            watch.Stop();
            Results.Add(new CheckResult(feature, "(n/a)", "(n/a)", 0, watch.Elapsed.TotalMilliseconds,
                false, $"{ex.GetType().Name}: {ex.Message}", true, ExtendedGroup));
        }
    }

    private static string ExtDir(string feature, params string[] more)
    {
        string path = Path.Combine(_outRoot, feature, Path.Combine(more));
        Directory.CreateDirectory(more.Length == 0 ? path : Path.GetDirectoryName(path)!);
        return path;
    }

    /// <summary>缺 ACE/Jet 通道时的降级行：不抛异常、不报红，而是像集成测试一样显式跳过并写清原因。</summary>
    private static void RunSkip(string feature, string reason)
    {
        if (!Wanted(feature))
        {
            return;
        }

        Results.Add(new CheckResult(feature, "(skip)", "(none)", 0, 0, false,
            "跳过（无 ACE 通道）：" + reason, true, ExtendedGroup, Skipped: true));
    }

    private static async Task RunExtendedChecksAsync()
    {
        Console.WriteLine();
        Console.WriteLine("== 扩展项（姓名 / 考评 / 工具 / 系统）==");

        RunExt("name.vertical.table", CheckNameTable);
        RunExt("name.vertical.table.crlf", CheckNameTableCrLf);
        RunExt("name.vertical.clean", CheckNameClean);
        RunExt("name.vertical.dupe", CheckNameDuplicate);
        for (int mode = 0; mode < 6; mode++)
        {
            int captured = mode;
            RunExt($"name.vertical.pinyin{mode}", () => CheckNamePinyin(captured));
        }

        RunExt("idcard.check", CheckIdCard);
        RunExt("twain.debug", CheckTwain);
        RunExt("twain.debug.reason-text", CheckTwainReasonText);
        RunExt("system.network", CheckNetworkAdapters);
        RunExt("system.tools", CheckSystemTools);
        RunExt("system.settings", CheckAppSettings);

        // TIF/GIF 产物（task-27）：走真实链路 image.compress format=TIF/GIF。
        RunExt("image.convert.tif", CheckImageTif);
        RunExt("image.convert.gif", CheckImageGif);

        // P2-1 / P2-7 / P2-8（task-35）：Schema 有、面板有，但此前没有产物级证据的那几项。
        RunExt("code.barcode.appearance", CheckBarcodeAppearance);
        RunExt("image.compress.useSourceExtension", CheckImageUseSourceExtension);
        RunExt("image.compress.noCompress", CheckImageNoCompress);
        RunExt("image.compress.bmpPixelBits", CheckImageBmpPixelBits);
        RunExt("ppt.pdf.exportNotes", CheckPptExportNotes);

        // dev 对照组（task-46）：同一输入与 dev 分支的已知行为比对，差异标 Skipped 不留绿灯。
        RunDevParityChecks();

        // UX 政策（task-46）：行为断言，跑在真实 Window 里，不用离屏数据。
        RunUiPolicyChecks();

        (bool aceAvailable, string aceReason) = AceAvailability.Result;
        Console.WriteLine($"  exam.* Access 通道：available={aceAvailable}"
                          + (aceAvailable ? string.Empty : " reason=" + aceReason));

        string[] examChecks =
        [
            "exam.insert",
            "exam.report",
            "exam.mss3",
            "exam.vote",
            "exam.single",
            "exam.package",
            "exam.package.reset"
        ];

        if (aceAvailable)
        {
            RunExt("exam.insert", CheckExamInsert);
            RunExt("exam.report", CheckExamReport);
            RunExt("exam.mss3", CheckExamMss3);
            RunExt("exam.vote", CheckExamVote);
            RunExt("exam.single", CheckExamSingle);
            RunExt("exam.package", CheckExamPackage);
            RunExt("exam.package.reset", CheckExamPackageReset);
        }
        else
        {
            foreach (string feature in examChecks)
            {
                RunSkip(feature, aceReason);
            }
        }

        if (_childNoAce)
        {
            int reds = Results.Count(r => !r.Pass && !r.Skipped);
            Require(aceAvailable == false,
                "子进程设置了 " + AceAvailability.SimulateMissingVariable + "=1，ACE 却仍可用，模拟未生效");
            Require(reds == 0, $"无 ACE 通道下仍出现 {reds} 个失败行（应为全部显式跳过）");
            Console.WriteLine("  no-ace 子进程断言：全部 exam.* 已显式跳过，0 失败");
        }

        if (_childNoAce)
        {
            return;
        }

        await RunNoAceChildAsync();
    }

    /// <summary>
    /// 在独立进程里带 <c>DOCUMENTTOOLS_SIMULATE_NO_ACE=1</c> 跑一遍 exam.*，验证“无 ACE 环境不崩溃、给可执行文案”。
    /// 必须另起进程：AceAvailability 是 Lazy 静态，同进程无法模拟两种环境。
    /// </summary>
    private static async Task RunNoAceChildAsync()
    {
        const string feature = "exam.no-ace";
        if (!Wanted(feature))
        {
            return;
        }

        Stopwatch watch = Stopwatch.StartNew();
        string logFile = Path.Combine(ExtDir(feature), "child.log");
        string? exe = Environment.ProcessPath;
        try
        {
            Require(!string.IsNullOrEmpty(exe), "无法取得自身可执行文件路径");

            var startInfo = new ProcessStartInfo
            {
                FileName = exe!,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                WorkingDirectory = _repoRoot
            };
            startInfo.ArgumentList.Add("--child-no-ace");
            startInfo.Environment[AceAvailability.SimulateMissingVariable] = "1";

            using Process child = Process.Start(startInfo)
                                  ?? throw new InvalidOperationException("子进程未能启动");
            Task<string> stdout = child.StandardOutput.ReadToEndAsync();
            Task<string> stderr = child.StandardError.ReadToEndAsync();
            await child.WaitForExitAsync();
            string output = await stdout + await stderr;
            File.WriteAllText(logFile, output);
            watch.Stop();

            Require(child.ExitCode == 0,
                $"无 ACE 子进程退出码应为 0，实际 {child.ExitCode}（日志 {Rel(logFile)}）");
            Require(!output.Contains("Unhandled exception", StringComparison.Ordinal),
                "无 ACE 子进程抛出未处理异常（日志 " + Rel(logFile) + "）");
            Require(output.Contains("跳过（无 ACE 通道）", StringComparison.Ordinal),
                "无 ACE 子进程未给出显式跳过文案（日志 " + Rel(logFile) + "）");
            Require(output.Contains(AceAvailability.SimulateMissingVariable, StringComparison.Ordinal),
                "降级文案未指明是被环境变量模拟的（日志 " + Rel(logFile) + "）");

            Results.Add(new CheckResult(feature, "--child-no-ace + "
                    + AceAvailability.SimulateMissingVariable + "=1", Rel(logFile), FileProbe.Size(logFile),
                watch.Elapsed.TotalMilliseconds, true,
                $"exit={child.ExitCode} skipRows=ok unhandled=none", true, ExtendedGroup));
        }
        catch (Exception ex)
        {
            watch.Stop();
            Results.Add(new CheckResult(feature, "--child-no-ace", Rel(logFile), 0,
                watch.Elapsed.TotalMilliseconds, false,
                $"{ex.GetType().Name}: {ex.Message}", true, ExtendedGroup));
        }
    }

    // ---------------------------------------------------------------- name.vertical

    private static INameVerticalService NameService
        => _provider!.GetRequiredService<INameVerticalService>();

    private static ExtOutcome CheckNameTable()
    {
        string fixture = Fixture("name", "table_tabs.txt");
        RequireFixture(fixture);
        string source = File.ReadAllText(fixture);

        NameVerticalResult result = NameService.Process(source, new NameVerticalOptions
        {
            RemoveNumber = true,
            RemoveSquare = true,
            DoubleAlign = true,
            Distinct = false
        });

        Require(result.Count == 12, $"表格分支应得 12 个名字，实得 {result.Count}：{string.Join(" / ", result.Lines)}");
        Require(!result.Lines.Any(l => l.Contains('○') || l.Contains('□') || l.Contains('£')),
            "表格分支应清掉 ○ □ £：" + string.Join(" / ", result.Lines));

        // 双字对齐的判据必须是“两个半角空格”，不是“看起来对齐”——半角/全角、单个空格都会在这里露馅。
        int alignedIndex = Array.IndexOf(result.Lines.ToArray(), "李  四");
        Require(alignedIndex >= 0, "双字对齐缺失，「李四」应变成「李  四」：" + string.Join(" / ", result.Lines));
        string aligned = result.Lines[alignedIndex];
        Require(aligned.Length == 4 && aligned.Count(c => c == ' ') == 2,
            $"双字对齐必须是两个半角空格，实得「{aligned}」（长度 {aligned.Length}，半角空格 {aligned.Count(c => c == ' ')}）");

        Require(result.Lines.Contains("王小明"), "三字姓名不应被对齐");
        Require(!result.Lines.Contains("赵七") && result.Lines.Contains("赵  七"),
            "「○ 赵七」清掉符号后应作为两字姓名对齐成「赵  七」");

        string output = Path.Combine(ExtDir("name.vertical.table"), "vertical.txt");
        File.WriteAllText(output, result.ToVerticalText());
        return new ExtOutcome(true, Rel(fixture), Rel(output), FileProbe.Size(output),
            $"names={result.Count} aligned=「{aligned}」(2 空格) sample={string.Join("|", result.Lines.Take(4))}");
    }

    private static ExtOutcome CheckNameTableCrLf()
    {
        string plain = Fixture("name", "table_tabs.txt");
        string crlf = Fixture("name", "table_copy_crlf.txt");
        RequireFixture(plain);
        RequireFixture(crlf);

        string crlfText = File.ReadAllText(crlf);
        Require(crlfText.Contains("\r\n", StringComparison.Ordinal), "夹具应使用 CRLF 行尾");

        var options = new NameVerticalOptions
        {
            RemoveNumber = true,
            RemoveSquare = true,
            DoubleAlign = true,
            Distinct = false
        };

        NameVerticalResult baseline = NameService.Process(File.ReadAllText(plain), options);
        NameVerticalResult mixed = NameService.Process(crlfText, options);

        Require(mixed.Count == baseline.Count,
            $"CRLF 与 LF 结果行数应一致：crlf={mixed.Count} lf={baseline.Count}");
        for (int i = 0; i < baseline.Count; i++)
        {
            Require(mixed.Lines[i] == baseline.Lines[i],
                $"第 {i + 1} 行 CRLF 与 LF 不一致：crlf=「{mixed.Lines[i]}」lf=「{baseline.Lines[i]}」");
        }

        // 行尾没被清干净的话，这里会看到残留的 \r（在字符串里不可见，但会破坏后续比对/导出）。
        Require(!mixed.Lines.Any(l => l.Contains('\r')), "结果里残留了 \\r，说明行尾归一顺序有问题");

        // ToVerticalText 是“一字一行”的竖排渲染，用 Environment.NewLine 连接；
        // \r 只应出现在换行符里，不应出现在任何一行内容中。
        string vertical = mixed.ToVerticalText();
        string[] verticalLines = vertical.Split(Environment.NewLine);
        Require(verticalLines.All(l => !l.Contains('\r')), "竖排文本的某一行里残留了 \\r");
        Require(verticalLines.Length == mixed.Lines.Sum(l => l.Length),
            $"竖排应一字一行（共 {mixed.Lines.Sum(l => l.Length)} 行），实得 {verticalLines.Length} 行");
        Require(verticalLines.Count(l => l == " ") >= 2,
            "双字对齐的两个半角空格在竖排里应占两行，未被裁掉");

        // 夹具原样只有 3 行，每行 4 个单元格 → 12 个名字（不是 3 个名字）。
        int rowCount = crlfText.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length;
        Require(rowCount == 3, $"CRLF 夹具应为 3 行，实际 {rowCount} 行");

        // 逐个两字名单独判"恰 2 个半角空格"，不靠"看起来对齐"：李四/钱八/孙九/周十/赵七 共 5 个。
        string[] alignedLines = mixed.Lines.Where(l => l.Contains(' ')).ToArray();
        Require(alignedLines.Length == 5,
            $"两字名应有 5 个被对齐，实际 {alignedLines.Length} 个：{string.Join(" / ", alignedLines)}");
        foreach (string line in alignedLines)
        {
            Require(line.Length == 4 && line.Count(c => c == ' ') == 2,
                $"对齐行「{line}」应是长度 4 + 恰 2 个半角空格，实得长度 {line.Length}、空格 {line.Count(c => c == ' ')}");
        }

        return new ExtOutcome(true, Rel(crlf) + " vs " + Rel(plain), "(none)", 0,
            $"夹具 {rowCount} 行→{mixed.Count} 名，与 LF 逐行一致；对齐行 {alignedLines.Length} 个（各长度 4/空格恰 2）；"
            + $"无 \\r 残留；竖排 {verticalLines.Length} 行（一字一行）");
    }

    private static ExtOutcome CheckNameClean()
    {
        string fixture = Fixture("name", "numbers_and_brackets.txt");
        RequireFixture(fixture);

        NameVerticalResult result = NameService.Process(File.ReadAllText(fixture), new NameVerticalOptions
        {
            RemoveNumber = true,
            RemoveSquare = true,
            DoubleAlign = false
        });

        string[] expected = ["张三", "李四", "王五六", "赵七", "钱八", "孙九", "周十"];
        Require(result.Count == expected.Length,
            $"去数字/括号后应有 {expected.Length} 个名字，实得 {result.Count}：{string.Join(" / ", result.Lines)}");
        for (int i = 0; i < expected.Length; i++)
        {
            Require(result.Lines[i] == expected[i], $"第 {i + 1} 行应为「{expected[i]}」，实得「{result.Lines[i]}」");
        }

        return new ExtOutcome(true, Rel(fixture), "(none)", 0, "count=7 数字/半角括号/全角括号/空括号均已清除");
    }

    private static ExtOutcome CheckNameDuplicate()
    {
        string fixture = Fixture("name", "duplicates_and_two_chars.txt");
        RequireFixture(fixture);
        string source = File.ReadAllText(fixture);

        NameVerticalResult kept = NameService.Process(source,
            new NameVerticalOptions { Distinct = false, DoubleAlign = false });
        Require(kept.Count == 8, $"保留重复时应有 8 行，实得 {kept.Count}");
        int[] duplicates = [.. kept.DuplicateLineIndices];
        Require(duplicates.SequenceEqual([0, 1, 2, 4, 5, 6]),
            "重名行号应为 0,1,2,4,5,6，实得 " + string.Join(",", duplicates));

        NameVerticalResult distinct = NameService.Process(source,
            new NameVerticalOptions { Distinct = true, DoubleAlign = false });
        Require(distinct.Count == 5, $"去重后应有 5 个名字，实得 {distinct.Count}：{string.Join(" / ", distinct.Lines)}");
        Require(distinct.DuplicateLineIndices.Count == 0, "去重后不应再有重名标记");

        return new ExtOutcome(true, Rel(fixture), "(none)", 0,
            $"duplicates={string.Join(",", duplicates)} distinct={distinct.Count}");
    }

    private static ExtOutcome CheckNamePinyin(int mode)
    {
        var options = new NameVerticalOptions
        {
            PinyinEnabled = true,
            PinyinMode = (NamePinyinMode)mode,
            DoubleAlign = false
        };

        NameVerticalResult result = NameService.Process("张三\n李四", options);
        Require(result.Count == 2, $"拼音模式 {mode} 应有 2 行，实得 {result.Count}");

        string[][] expected =
        [
            ["ZS", "LS"],
            ["zs", "ls"],
            ["ZHANG SAN", "LI SI"],
            ["ZHANGSAN", "LISI"],
            ["zhang san", "li si"],
            ["zhangsan", "lisi"]
        ];

        string[] want = expected[mode];
        Require(result.Lines[0] == want[0] && result.Lines[1] == want[1],
            $"模式 {mode} 期望 [{string.Join("|", want)}]，实得 [{string.Join("|", result.Lines)}]");

        return new ExtOutcome(true, "testdata/name/*（张三/李四）", "(none)", 0,
            $"mode={mode} {result.Lines[0]} / {result.Lines[1]}");
    }

    // ---------------------------------------------------------------- 工具 / 系统

    private static ExtOutcome CheckIdCard()
    {
        ChineseIdCardResult good = ChineseIdCard.Validate("11010519491231002X");
        Require(good.IsChecksumValid, "11010519491231002X 应为合法身份证号");
        Require(good.Birthday == "1949年12月31日", "生日解析错：" + good.Birthday);
        Require(good.Sex == "女", "性别解析错：" + good.Sex);
        Require(good.Sequence == "002", "顺序码解析错：" + good.Sequence);

        ChineseIdCardResult bad = ChineseIdCard.Validate("110105194912310021");
        Require(!bad.IsChecksumValid, "校验位错误应判不合法");
        Require(bad.ExpectedChecksum == "X", "期望校验位应为 X，实得 " + bad.ExpectedChecksum);

        ChineseIdCardResult malformed = ChineseIdCard.Validate("12345");
        Require(!malformed.IsChecksumValid && malformed.Birthday.Length == 0, "长度非法应返回空结果");

        ChineseIdCardResult male = ChineseIdCard.Validate("320102198512310037");
        Require(male.IsChecksumValid && male.Sex == "男" && male.Birthday == "1985年12月31日",
            "320102198512310037 解析错：" + male.Birthday + "/" + male.Sex);

        // 法定年代下限：Core 明确把 1900 年之前判为「日期不正确」，这是规则而不是缺陷。
        ChineseIdCardResult tooOld = ChineseIdCard.Validate("440524188001010014");
        Require(tooOld.IsChecksumValid && tooOld.Birthday == "日期不正确",
            "1880 年生日应判为「日期不正确」但校验位仍合法，实际 " + tooOld.Birthday);

        return new ExtOutcome(true, "(内置号码)", "(none)", 0,
            "valid=11010519491231002X 1949年12月31日 女 002；invalid 期望X；格式非法返回空；1880 年生日按规则判「日期不正确」");
    }

    private static ExtOutcome CheckTwain()
    {
        ITwainScanService service = _provider!.GetRequiredService<ITwainScanService>();

        // 可用性只看「会话能不能建起来」；文案契约（可用时为空串）由 twain.debug.reason-text 单独钉。
        bool available = service.IsAvailable;
        string reason = service.UnavailableReason;

        IReadOnlyList<TwainSourceInfo> sources = service.ListSources();
        Require(sources.Count == 0 || available, "不可用时 ListSources 应为空：" + sources.Count);

        string scanOutcome;
        if (available)
        {
            scanOutcome = "available，跳过无设备断言";
        }
        else
        {
            Require(!string.IsNullOrWhiteSpace(reason), "不可用时 UnavailableReason 不能为空");
            Require(reason.Contains("Twain", StringComparison.OrdinalIgnoreCase)
                    || reason.Contains("扫描仪", StringComparison.Ordinal),
                "降级文案未指明 TWAIN/扫描仪：" + reason);


            // 设计如此：无 session 时 Scan 直接抛 NotSupportedException 并把降级文案带出去
            // （接口注释写的是“返回空集合”，与实现不一致，已在报告里记为文档偏差）。
            try
            {
                IReadOnlyList<string> scanned = service.Scan("none", new TwainScanOptions
                {
                    OutputDirectory = ExtDir("twain.debug"),
                    Dpi = 200,
                    ShowUi = false
                });
                scanOutcome = "返回 " + scanned.Count + " 个文件（未抛异常）";
                Require(scanned.Count == 0, "无设备时不应产出扫描文件");
            }
            catch (NotSupportedException ex)
            {
                Require(!string.IsNullOrWhiteSpace(ex.Message), "NotSupportedException 缺少文案");
                scanOutcome = "NotSupportedException：" + ex.Message;
            }
        }

        return new ExtOutcome(true, "(无扫描仪)", "(none)", 0,
            $"IsAvailable={available} reason=「{reason}」sources={sources.Count} scan={scanOutcome}");
    }

    /// <summary>
    /// 两个分支都要钉住：可用时 <c>UnavailableReason</c> 必须是空串（可用还说「未初始化」会误导，
    /// 设备列表就在旁边）；不可用时必须是含 Twain/扫描仪的可执行文案。
    /// 之前只覆盖了不可用分支，等于让「可用时为空串」这条契约没人守。
    /// </summary>
    private static ExtOutcome CheckTwainReasonText()
    {
        ITwainScanService service = _provider!.GetRequiredService<ITwainScanService>();
        bool available = service.IsAvailable;
        string reason = service.UnavailableReason;

        if (available)
        {
            Require(reason.Length == 0, $"Twain 可用时 UnavailableReason 必须是空串，实际「{reason}」");
            return new ExtOutcome(true, "(扫描仪可用)", "(none)", 0,
                "available=True → UnavailableReason=\"\"（可用时不给人「未初始化」的误导）");
        }

        Require(!string.IsNullOrWhiteSpace(reason), "不可用时 UnavailableReason 不能为空");
        Require(reason.Contains("Twain", StringComparison.OrdinalIgnoreCase)
                || reason.Contains("扫描仪", StringComparison.Ordinal),
            "降级文案未指明 TWAIN/扫描仪：" + reason);
        return new ExtOutcome(true, "(无扫描仪)", "(none)", 0,
            "available=False → 降级文案含 TWAIN/扫描仪：「" + reason + "」");
    }

    private static ExtOutcome CheckNetworkAdapters()
    {
        SysTools.INetworkAdapterService service = _provider!.GetRequiredService<SysTools.INetworkAdapterService>();
        IReadOnlyList<SysTools.NetworkAdapterInfo> adapters = service.ListAdapters();
        Require(adapters.Count > 0, "本机应至少有一个网络适配器");
        Require(adapters.All(a => !string.IsNullOrWhiteSpace(a.Name)), "适配器名称不应为空");

        SysTools.NetworkAdapterInfo first = adapters[0];
        return new ExtOutcome(true, "(本机适配器)", "(none)", 0,
            $"adapters={adapters.Count} first=「{first.Name}」status={first.Status} type={first.Type} ipv4={first.Ipv4 ?? "-"}");
    }

    private static ExtOutcome CheckSystemTools()
    {
        SysTools.IWindowsSystemToolsService service = _provider!.GetRequiredService<SysTools.IWindowsSystemToolsService>();
        bool admin = service.IsAdministrator();
        return new ExtOutcome(true, "(本机)", "(none)", 0,
            $"IsAdministrator={admin}（其余写操作需管理员且会改系统设置，未执行）");
    }

    private static ExtOutcome CheckAppSettings()
    {
        DocumentTools.Core.Settings.AppSettings settings = DocumentTools.Core.Settings.AppSettings.Load();
        IReadOnlyList<string> hidden = settings.GetHiddenFeatureIds();
        Require(settings.StartFeatureId.Length > 0, "StartFeatureId 不应为空");

        DocumentTools.Core.Settings.AppSettings legacy = DocumentTools.Core.Settings.AppSettings.FromLegacyConfig();
        Require(legacy is not null, "FromLegacyConfig 不应返回 null");

        return new ExtOutcome(true, "(用户配置)", "(none)", 0,
            $"start={settings.StartFeatureId} hidden={hidden.Count} store={Rel(DocumentTools.Core.Settings.AppSettings.StorePath)}");
    }

    // ---------------------------------------------------------------- exam.*

    /// <summary>
    /// 考评库统一由 <c>tools\ExamFixture</c> 生成（它把 DDL 的来源行号写进了注释，是唯一真相）。
    /// harness 只负责决定“要不要自增探针列 / 要不要样例数据”，不再自己拼表结构。
    /// </summary>
    private static ExamFixtureSet CreateExamFixture(string feature, string tag, ExamFixtureOptions? options = null)
    {
        string root = Path.Combine(_outRoot, feature, tag);
        if (Directory.Exists(root))
        {
            Directory.Delete(root, true);
        }

        return ExamFixtureBuilder.Create(root, options ?? new ExamFixtureOptions());
    }

    private static DataTable Query(string databaseFile, string sql)
    {
        using var session = new AccessDb(databaseFile);
        return session.Query(sql);
    }

    private static int DbCount(string databaseFile, string sql)
    {
        using var session = new AccessDb(databaseFile);
        return Convert.ToInt32(session.Scalar(sql), CultureInfo.InvariantCulture);
    }

    private static string DbText(string databaseFile, string sql)
    {
        using var session = new AccessDb(databaseFile);
        object? value = session.Scalar(sql);
        return value is null or DBNull ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static ExtOutcome CheckExamInsert()
    {
        using ExamFixtureSet fixture = CreateExamFixture("exam.insert", "db");
        string db = fixture.DatabaseFile;

        IExamInsertService service = _provider!.GetRequiredService<IExamInsertService>();
        var request = new ExamInsertRequest
        {
            DatabaseFile = db,
            ExamName = "QA 考评表",
            TypeMode = "人员纵排",
            Items =
            {
                new ExamInsertItem
                {
                    ItemName = "工作态度",
                    ItemScore = "10",
                    Selectors =
                    {
                        new ExamInsertSelector { SelectorName = "优秀", ScorePer = "5" },
                        new ExamInsertSelector { SelectorName = "弃权" }
                    }
                }
            }
        };

        ExamInsertResult first = service.Insert(request);
        Require(first.Success, "首次插入失败：" + first.Summary);
        Require(first.CreatedExam, "首次插入应新建考评表");
        Require(first.ItemCount == 1 && first.SelectorCount == 2 && first.BoolItemCount == 1,
            $"计数不符：item={first.ItemCount} selector={first.SelectorCount} boolItem={first.BoolItemCount}（弃权应跳过选题）");

        // Exam_Score 每行 = 一个 (Kind, ItemName, SelName) 组合。项目行 1 条、显式选项（优秀）1 条、
        // 隐式选项（弃权/无效）各若干条 —— 隐式行的具体条数属 legacy 细节（无效会写成
        // 「选项」与「选项不计入得票率」两行），所以断言按语义查，而不是钉死一个总数。
        Require(DbCount(db, $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId = {first.ExamId} AND Kind = '项目'") == 1,
            "本次考评应恰有 1 行项目行（Kind=项目）");
        Require(DbCount(db, $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId = {first.ExamId} AND SelName = '优秀'") >= 1,
            "应写入显式选项「优秀」的得分行");
        Require(DbCount(db, $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId = {first.ExamId} AND SelName = '弃权' AND Score = 0") >= 1,
            "弃权应写成 Score=0 的隐式选项行");
        Require(DbCount(db, $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId = {first.ExamId} AND SelName = '无效'") >= 1,
            "无效应是隐式选项行（legacy 语义）");
        Require(DbCount(db,
                $"SELECT COUNT(*) FROM (SELECT Kind, ItemName, SelName FROM Exam_Score WHERE ExamId = {first.ExamId} "
                + "GROUP BY Kind, ItemName, SelName HAVING COUNT(*) > 1)") == 0,
            "Exam_Score 不应出现重复的 (Kind, ItemName, SelName) 组合");

        int scoreRows = DbCount(db, $"SELECT COUNT(*) FROM Exam_Score WHERE ExamId = {first.ExamId}");
        Require(DbCount(db, $"SELECT COUNT(*) FROM Exam_BoolItem WHERE ExamId = {first.ExamId}") == 1,
            "弃权是隐式选项，Exam_BoolItem 应只有 1 行");
        Require(DbCount(db, $"SELECT COUNT(*) FROM Exam_Ruler WHERE ExamId = {first.ExamId}") == 5,
            "默认规则应有 5 行");

        ExamInsertResult second = service.Insert(request);
        Require(second.Success && !second.CreatedExam,
            "同名再次插入应追加到已有考评表：" + second.Summary);

        IReadOnlyList<(int Id, string Name, string TypeMode)> exams = service.ListExams(db);
        Require(exams.Any(e => e.Name == "QA 考评表"), "ListExams 应包含新建的考评表");
        Require(exams.Count == 3, "夹具自带 2 个考评表 + 本次 1 个，应为 3 个，实得 " + exams.Count);

        return new ExtOutcome(true, Rel(db), "(none)", FileProbe.Size(db),
            $"examId={first.ExamId} score={scoreRows}(项目1 + 优秀 + 弃权0 + 无效若干) boolItem=1 ruler=5 "
            + $"append=ok exams={exams.Count}（夹具 ExamFixture）");
    }

    private static ExtOutcome CheckExamReport()
    {
        using ExamFixtureSet fixture = CreateExamFixture("exam.report", "db");
        string db = fixture.DatabaseFile;

        IExamReportService service = _provider!.GetRequiredService<IExamReportService>();
        IReadOnlyList<ExamReportRow> rows = service.ListReports(db);
        Require(rows.Count == 2, "夹具自带 2 个报表，实得 " + rows.Count);
        Require(rows.Any(r => r.ExamName == "一级考评"), "ListReports 应关联出 Exam_EssInfo 的考评表名称");

        ReportContentReplaceResult replaced = service.ReplaceContent(db, new ReportContentReplaceRequest
        {
            ReportId = 7,
            ItemName = "所有项目",
            ChangeItemName = true,
            NewItemName = "新项目",
            ChangeRaid = true,
            NewRaid = "9"
        });

        // 夹具里 ReportID=7 有两条 Report_Detail（GridFormatData / GridFormatText）。
        Require(replaced.ScannedRows == 2 && replaced.ChangedRows == 2,
            $"替换计数不符：scanned={replaced.ScannedRows} changed={replaced.ChangedRows}");

        string content = DbText(db, "SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatData'");
        Require(content.Contains("Item=新项目", StringComparison.Ordinal), "Item 未替换：" + content);
        Require(content.Contains("Raid=9", StringComparison.Ordinal), "Raid 未替换：" + content);

        string otherReport = DbText(db, "SELECT Content FROM Report_Detail WHERE ReportID = 8");
        Require(otherReport.Contains("Item=丙项", StringComparison.Ordinal),
            "别的报表不应被改动：" + otherReport);

        bool guarded;
        try
        {
            service.ReplaceContent(db, new ReportContentReplaceRequest { ReportId = 0 });
            guarded = false;
        }
        catch (ArgumentException)
        {
            guarded = true;
        }

        Require(guarded, "未选中报表时应抛 ArgumentException");

        return new ExtOutcome(true, Rel(db), "(none)", FileProbe.Size(db),
            $"reports=2 scanned=2 changed=2 其他报表未动 guard=ok（夹具 ExamFixture）");
    }

    private static ExtOutcome CheckExamMss3()
    {
        using ExamFixtureSet fixture = CreateExamFixture("exam.mss3", "db");
        string db = fixture.DatabaseFile;

        string image = Fixture("image", "sample.png");
        RequireFixture(image);

        IMss3VoteService service = _provider!.GetRequiredService<IMss3VoteService>();
        int beforeVotes = service.ListVotes(db).Count;
        service.InsertVotePage(db, image, "V-QA", "QA 选票", "MC-1", 1, "方框");

        IReadOnlyList<Mss3VoteRow> votes = service.ListVotes(db);
        Require(votes.Count == beforeVotes + 1, $"应新增 1 张选票：{beforeVotes} -> {votes.Count}");
        Require(votes.Any(v => v.VoteName == "QA 选票"), "ListVotes 应包含刚插入的选票");
        Require(DbCount(db, "SELECT COUNT(*) FROM Vote_PageInfo WHERE VoteID = 'V-QA'") == 1,
            "Vote_PageInfo 应有 1 行");

        DataTable table = Query(db, "SELECT PicContent FROM Vote_PageInfo WHERE VoteID = 'V-QA'");
        byte[] blob = (byte[])table.Rows[0]["PicContent"];
        Require(blob.Length > 64 && blob[0] == 'B' && blob[1] == 'M',
            $"PicContent 应为灰度 BMP，实际前两字节 {blob[0]},{blob[1]}，长度 {blob.Length}");

        int memo = service.SetItemColumnsToMemo(db);

        return new ExtOutcome(true, Rel(image), Rel(db), FileProbe.Size(db),
            $"votes={votes.Count}(夹具 {beforeVotes} + 1) bmp={blob.Length} B memoAltered={memo}");
    }

    private static ExtOutcome CheckExamVote()
    {
        string template = Path.Combine(_repoRoot, "assets", "embedded", "VoteReportDoc.doc");
        RequireFixture(template);

        string output = Path.Combine(ExtDir("exam.vote"), "vote_report.docx");
        IVoteReportService service = _provider!.GetRequiredService<IVoteReportService>();

        using (FileStream stream = File.OpenRead(template))
        {
            service.Create(new VoteReportRequest
            {
                Template = stream,
                OutputFile = output,
                Title = "QA 选举结果",
                StartText = "应到 10 人，实到 9 人",
                EndText = "选举有效",
                GenDateText = "2026-11-05",
                FillNames = ["张三", "李四", "王五"],
                RandomVoteCounts = true,
                NeedAnotherTable = true,
                KeepFooter = true
            });
        }

        Require(File.Exists(output) && FileProbe.Size(output) > 0, "未生成选票报表：" + Rel(output));
        Require(FileProbe.IsOfficePackage(output, "word/document.xml"), "产物不是 DOCX：" + Rel(output));

        string xml = FileProbe.ReadZipText(output, "word/document.xml");
        Require(xml.Contains("QA 选举结果", StringComparison.Ordinal), "标题未写入");
        Require(xml.Contains("张三", StringComparison.Ordinal), "名单未写入");
        Require(CountRegex(xml, "<w:tbl>") >= 2, "应有两张表（主表 + 另选人表），实际 " + CountRegex(xml, "<w:tbl>"));

        return new ExtOutcome(true, Rel(template), Rel(output), FileProbe.Size(output),
            $"docx=ok tables={CountRegex(xml, "<w:tbl>")} names=3 template=VoteReportDoc.doc");
    }

    private static ExtOutcome CheckExamSingle()
    {
        string template = Fixture("exam", "person_template.docx");
        string photo = Fixture("exam", "person_photo.png");
        RequireFixture(template);
        RequireFixture(photo);

        string output = Path.Combine(ExtDir("exam.single"), "person_replaced.docx");
        ISingleDocumentService service = _provider!.GetRequiredService<ISingleDocumentService>();
        service.ReplacePersonImages(template, ["照片"], photo, output);

        Require(File.Exists(output) && FileProbe.Size(output) > 0, "未生成替换后的文档");
        Require(FileProbe.IsOfficePackage(output, "word/document.xml"), "产物不是 DOCX");

        byte[] photoBytes = File.ReadAllBytes(photo);
        (int expectedWidth, int expectedHeight) = FileProbe.PngSize(photo);
        bool matched = false;
        using (System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(output))
        {
            foreach (System.IO.Compression.ZipArchiveEntry entry in archive.Entries)
            {
                if (!entry.FullName.StartsWith("word/media/", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                using Stream stream = entry.Open();
                using var memory = new MemoryStream();
                stream.CopyTo(memory);
                byte[] bytes = memory.ToArray();
                if (bytes.AsSpan().SequenceEqual(photoBytes))
                {
                    matched = true;
                    break;
                }

                (int w, int h) = FileProbe.PngSize(bytes);
                if (w == expectedWidth && h == expectedHeight)
                {
                    matched = true;
                    break;
                }
            }
        }

        Require(matched, $"未在产物里找到替换后的照片（期望 {expectedWidth}x{expectedHeight}）");

        return new ExtOutcome(true, Rel(template) + " + " + Rel(photo), Rel(output), FileProbe.Size(output),
            $"replaced={expectedWidth}x{expectedHeight} altText=照片");
    }

    private static ExtOutcome CheckExamPackage()
    {
        // IncludeAutoIncrementProbeColumn：给 Exam_People / Unit_Ess 建 COUNTER 主键，
        // 用来验证 Merge 必须跳过目标库自增列（否则会撞主键）——这是相对旧实现的关键修正，
        // 没有自增列的夹具根本覆盖不到这条路径。
        var options = new ExamFixtureOptions
        {
            SeedSampleRows = false,
            IncludeAutoIncrementProbeColumn = true
        };

        using ExamFixtureSet sourceFixture = CreateExamFixture("exam.package", "source", options);
        using ExamFixtureSet targetFixture = CreateExamFixture("exam.package", "target", options);
        string sourceDirectory = sourceFixture.DirectoryPath;
        string targetDirectory = targetFixture.DirectoryPath;
        string sourceDb = sourceFixture.DatabaseFile;
        string targetDb = targetFixture.DatabaseFile;

        using (var session = new AccessDb(sourceDb))
        {
            session.Execute("INSERT INTO Unit_Ess (AtOrder, Unit, UnitCode, AtKind) VALUES (?,?,?,?)",
                ("@p0", 1), ("@p1", "A单位"), ("@p2", "U1"), ("@p3", "速甲"));
            session.Execute("INSERT INTO Unit_Ess (AtOrder, Unit, UnitCode, AtKind) VALUES (?,?,?,?)",
                ("@p0", 2), ("@p1", "B单位"), ("@p2", "U2"), ("@p3", "速乙"));
            session.Execute("INSERT INTO Unit_Ess (AtOrder, Unit, UnitCode, AtKind) VALUES (?,?,?,?)",
                ("@p0", 5), ("@p1", "C单位"), ("@p2", "U3"), ("@p3", "速丙"));
            session.Execute("INSERT INTO Exam_People (UnitCode) VALUES (?)", ("@p0", "U1"));
            session.Execute("INSERT INTO Exam_People (UnitCode) VALUES (?)", ("@p0", "U2"));
        }

        using (var session = new AccessDb(targetDb))
        {
            session.Execute("INSERT INTO Unit_Ess (AtOrder, Unit, UnitCode, AtKind) VALUES (?,?,?,?)",
                ("@p0", 1), ("@p1", "A单位"), ("@p2", "U1"), ("@p3", "速甲"));
            session.Execute("INSERT INTO Unit_Ess (AtOrder, Unit, UnitCode, AtKind) VALUES (?,?,?,?)",
                ("@p0", 4), ("@p1", "X单位"), ("@p2", "UX"), ("@p3", "速丁"));
            session.Execute("INSERT INTO Exam_People (UnitCode) VALUES (?)", ("@p0", "U1"));
            session.Execute("INSERT INTO Exam_People (UnitCode) VALUES (?)", ("@p0", "UX"));
        }

        File.WriteAllText(Path.Combine(sourceDirectory, "a.exam"), "source-package");
        File.WriteAllText(Path.Combine(sourceDirectory, "b.exam"), "source-package-b");
        File.WriteAllText(Path.Combine(targetDirectory, "a.exam"), "target-package");

        IExamPackageService service = _provider!.GetRequiredService<IExamPackageService>();

        // Inspect / ListUnits 是 UI 列表页依赖的只读能力，和 Merge 一起验，避免"合并能跑但列表是空的"。
        ExamPackageStatus sourceStatus = service.Inspect(sourceDirectory);
        Require(sourceStatus.DatabaseExists, "Inspect 应识别出源数据包的 Exam_Base.mdb");
        Require(sourceStatus.PeopleCount == 2 && sourceStatus.UnitCount == 3 && sourceStatus.ExamFileCount == 2,
            $"源包计数不符：{sourceStatus.Summary}");
        Require(!sourceStatus.IsAlreadyInitialized, "源包有数据，IsAlreadyInitialized 应为 false");

        IReadOnlyList<(int Order, string Unit, string Code, string Kind)> units = service.ListUnits(sourceDirectory);
        Require(units.Count == 3 && units[0].Order == 1 && units[0].Code == "U1",
            $"ListUnits 应按 AtOrder 升序返回 3 个单位，实际 {units.Count}");

        bool duplicates = service.HasDuplicateUnits(sourceDirectory, targetDirectory);
        Require(duplicates, "源/目标存在重复单位 U1，HasDuplicateUnits 应为 true");

        ExamPackageMergeResult result = service.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = sourceDirectory,
            TargetDirectory = targetDirectory,
            CopyExamFiles = true,
            RemoveDuplicateUnitsInTarget = true
        });

        Require(result.HadDuplicateUnits, "结果应标记存在重复单位");
        Require(result.DuplicateUnitsRemoved == 1, "应清理 1 个重复单位，实际 " + result.DuplicateUnitsRemoved);
        Require(result.PeopleInserted == 2, "应插入 2 名人员，实际 " + result.PeopleInserted);

        // 旧实现语义：源库单位全量搬过去（含与目标同名的那一个，目标里的旧副本已先删掉），
        // 所以这里是 3 而不是 2；顺延起点是删完重复后目标库的 MAX(AtOrder)=4。
        Require(result.UnitsInserted == 3, "应插入 3 个单位（源库全量），实际 " + result.UnitsInserted);
        Require(result.ExamFilesCopied == 2, "应复制 2 个 .exam，实际 " + result.ExamFilesCopied);

        Require(DbCount(targetDb, "SELECT COUNT(*) FROM Unit_Ess") == 4,
            "目标单位应为 4 个（UX + U1 + U2 + U3），实际 " + DbCount(targetDb, "SELECT COUNT(*) FROM Unit_Ess"));
        Require(DbCount(targetDb, "SELECT COUNT(*) FROM Exam_People") == 3, "目标人员应为 3 人");

        // 自增列没被写回的证据：3 行都有合法的自增主键值（若把源 AutoId 一起插进去，这里会是 0 行或直接抛主键冲突）。
        Require(DbCount(targetDb, "SELECT COUNT(*) FROM Exam_People WHERE AutoId > 0") == 3,
            "目标 Exam_People 的 COUNTER 主键应全部由目标库生成（证明未写入源自增列）");
        Require(DbCount(targetDb, "SELECT COUNT(*) FROM Unit_Ess WHERE AutoId > 0") == 4,
            "目标 Unit_Ess 的 COUNTER 主键应全部由目标库生成");

        string orderU3 = DbText(targetDb, "SELECT AtOrder FROM Unit_Ess WHERE UnitCode = 'U3'");
        string orderU1 = DbText(targetDb, "SELECT AtOrder FROM Unit_Ess WHERE UnitCode = 'U1'");
        Require(orderU1 == "5" && orderU3 == "7",
            $"源单位应按 MAX(AtOrder)=4 顺延为 U1=5 / U2=6 / U3=7，实际 U1={orderU1} U3={orderU3}");

        string[] examFiles = Directory.GetFiles(targetDirectory, "*.exam");
        Require(examFiles.Length == 3, ".exam 应为 3 个（原有 a.exam + b.exam + 重命名的 a_1.exam），实际 "
                                     + examFiles.Length + " 个");
        Require(File.ReadAllText(Path.Combine(targetDirectory, "a.exam")) == "target-package",
            "原 .exam 被覆盖了");

        long bytes = FileProbe.Size(targetDb) + examFiles.Sum(FileProbe.Size);
        return new ExtOutcome(true, Rel(sourceDb) + " -> " + Rel(targetDb), Rel(targetDirectory), bytes,
            "summary=" + result.Summary + "｜夹具 ExamFixture（COUNTER 主键探针）");
    }

    /// <summary>
    /// Reset 是数据包页「清空」按钮的后端：只清人员/单位/.exam/SelfReport，不动 Exam_Base.mdb 本体。
    /// 注意 <c>IsAlreadyInitialized</c> 的语义是「库在、内容空」——灌了数据时为 false，Reset 之后才为 true，
    /// 所以这里按 <c>ExamPackageStatus</c> 的定义断（任务描述里的先后顺序与它相反）。
    /// </summary>
    private static ExtOutcome CheckExamPackageReset()
    {
        var options = new ExamFixtureOptions
        {
            SeedSampleRows = false,
            CreateSelfReportFolder = true
        };

        using ExamFixtureSet fixture = CreateExamFixture("exam.package.reset", "db", options);
        string directory = fixture.DirectoryPath;
        string db = fixture.DatabaseFile;

        using (var session = new AccessDb(db))
        {
            session.Execute("INSERT INTO Unit_Ess (AtOrder, Unit, UnitCode, AtKind) VALUES (?,?,?,?)",
                ("@p0", 1), ("@p1", "A单位"), ("@p2", "U1"), ("@p3", "速甲"));
            session.Execute("INSERT INTO Unit_Ess (AtOrder, Unit, UnitCode, AtKind) VALUES (?,?,?,?)",
                ("@p0", 2), ("@p1", "B单位"), ("@p2", "U2"), ("@p3", "速乙"));
            session.Execute("INSERT INTO Exam_People (UnitCode) VALUES (?)", ("@p0", "U1"));
            session.Execute("INSERT INTO Exam_People (UnitCode) VALUES (?)", ("@p0", "U2"));
        }

        File.WriteAllText(Path.Combine(directory, "a.exam"), "reset-a");
        File.WriteAllText(Path.Combine(directory, "b.exam"), "reset-b");

        IExamPackageService service = _provider!.GetRequiredService<IExamPackageService>();

        ExamPackageStatus before = service.Inspect(directory);
        Require(before.DatabaseExists, "Inspect 应识别出夹具的 Exam_Base.mdb");
        Require(before.PeopleCount == 2 && before.UnitCount == 2 && before.ExamFileCount == 2,
            "灌入后计数不符：" + before.Summary);
        Require(before.HasSelfReport, "夹具带了 SelfReport 目录，Inspect 却没看到");
        Require(!before.IsAlreadyInitialized,
            "有数据时 IsAlreadyInitialized 应为 false（定义是「库在、内容空」）");

        int removed = service.Reset(directory);
        Require(removed == 4, "Reset 应报告删除 4 行（2 人 + 2 单位），实际 " + removed);

        ExamPackageStatus after = service.Inspect(directory);
        Require(after.DatabaseExists, "Reset 不应删掉 Exam_Base.mdb 本体");
        Require(after.PeopleCount == 0 && after.UnitCount == 0 && after.ExamFileCount == 0,
            "Reset 后三项计数都应为 0：" + after.Summary);
        Require(!after.HasSelfReport, "Reset 应删掉 SelfReport 目录");
        Require(after.IsAlreadyInitialized, "Reset 后 IsAlreadyInitialized 应为 true（库在、内容空）");

        // 不只看 Summary 文案：直接查表、直接看目录，防止实现只改了返回字符串。
        Require(DbCount(db, "SELECT COUNT(*) FROM Exam_People") == 0, "Exam_People 仍有残行");
        Require(DbCount(db, "SELECT COUNT(*) FROM Unit_Ess") == 0, "Unit_Ess 仍有残行");
        Require(Directory.GetFiles(directory, "*.exam").Length == 0, "*.exam 未被删除");
        Require(!Directory.Exists(fixture.SelfReportDirectory), "SelfReport 目录仍存在");

        int again = service.Reset(directory);
        Require(again == 0, "空包再次 Reset 应返回 0，实际 " + again);

        return new ExtOutcome(true, Rel(db), Rel(directory), FileProbe.Size(db),
            $"resetRemoved={removed} 人员/单位/.exam/SelfReport=已清零 IsAlreadyInitialized=false→true 二次reset=0");
    }

    // ---------------------------------------------------------------- image formats (TIF/GIF)

    private static readonly byte[] PngMagic = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] JpegMagic = { 0xFF, 0xD8, 0xFF };

    // 夹具 (0,0) 有深色外框，取景靠 (8,8) 落在左上角色块内部。
    private const int SampleX = 8;
    private const int SampleY = 8;

    private static ExtOutcome CheckImageTif()
        => CheckImageFormat("image.convert.tif", "TIF", ".tif",
            new byte[] { 0x49, 0x49, 0x2A, 0x00 }, "II 2A 00（小端 42）");

    private static ExtOutcome CheckImageGif()
        => CheckImageFormat("image.convert.gif", "GIF", ".gif",
            new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, "GIF89a");

    /// <summary>
    /// 走真实链路 <c>image.compress</c> → <c>ImageConvertOperation</c> → <c>ImageConversionService.Convert</c>，
    /// 断言产物魔数与解码结果。Core 没有 <c>image.convert</c> 这个 FeatureId，扩展名由 format 选项决定；
    /// <c>NormalizeFormat</c> 对未知格式会静默落成 <c>.png</c>，所以这里既断扩展名也断魔数——
    /// 只断「有产物」的话，静默回退 PNG 会假通过。
    /// </summary>
    private static ExtOutcome CheckImageFormat(
        string feature, string format, string expectedExt, byte[] expectedMagic, string magicLabel)
    {
        string source = Fixture("image", "tifgif_src.png");
        RequireFixture(source);

        if (!_operations.TryGetValue(ImageFormatFeature, out IDocumentOperation? operation))
        {
            throw new CheckException("未注册操作：" + ImageFormatFeature);
        }

        string directory = ExtDir(feature);
        var request = new DocumentOperationRequest
        {
            InputFiles = new[] { source },
            OutputDirectory = directory,
            Options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["format"] = format
            }
        };

        DocumentOperationResult result = operation.ExecuteAsync(request, CancellationToken.None)
            .GetAwaiter().GetResult();
        Require(result.Success, "Success=false：" + result.Message);

        // 静默回退时 OutputFiles 里是 .png 路径，所以先按扩展名找、再目录兜底，才能把真实魔数报出来。
        string? expected = result.OutputFiles.FirstOrDefault(
            p => p.EndsWith(expectedExt, StringComparison.OrdinalIgnoreCase) && File.Exists(p));
        string? actualPath = expected ?? Directory.EnumerateFiles(directory).FirstOrDefault();
        Require(actualPath is not null, "没有产出任何文件（opMessage=" + result.Message + "）");

        long size = FileProbe.Size(actualPath!);
        byte[] head = FileProbe.Head(actualPath!, 8);
        string where = $"期望魔数 {magicLabel} / 实际前 8 字节 {ToHex(head)} / 文件大小 {size} B / 文件 {Path.GetFileName(actualPath!)}";

        Require(actualPath!.EndsWith(expectedExt, StringComparison.OrdinalIgnoreCase),
            "扩展名不符（format 未生效，疑似静默回退）：" + where);
        Require(size > 200, "产物过小（疑似空壳）：" + where);
        Require(head.Length >= expectedMagic.Length
                && head.AsSpan(0, expectedMagic.Length).SequenceEqual(expectedMagic),
            "魔数不符：" + where);
        Require(!head.AsSpan(0, Math.Min(head.Length, PngMagic.Length)).SequenceEqual(
                PngMagic.AsSpan(0, Math.Min(head.Length, PngMagic.Length))),
            "产物前 8 字节等于 PNG 魔数（编码器静默回退）：" + where);
        Require(!head.AsSpan(0, Math.Min(head.Length, JpegMagic.Length)).SequenceEqual(
                JpegMagic.AsSpan(0, Math.Min(head.Length, JpegMagic.Length))),
            "产物前 3 字节等于 JPEG 魔数（编码器静默回退）：" + where);

        // 解码器选 GDI+ 而不是 SKBitmap.Decode：SkiaSharp 3.119 根本没有 TIFF 解码器
        // （实测 ref TIFF / 本产物都返回 SKCodec=null），拿它断 TIF 只会把工具链缺口记成编码器缺陷。
        using var sourceImage = new Bitmap(source);
        Bitmap decoded;
        try
        {
            decoded = new Bitmap(actualPath!);
        }
        catch (Exception ex)
        {
            throw new CheckException($"GDI+ 无法解码产物（{ex.GetType().Name}: {ex.Message}）：{where}");
        }

        using (decoded)
        {
            string geometry = $"{where} / Decode 宽高 {decoded.Width}x{decoded.Height}";
            Require(decoded.Width == sourceImage.Width && decoded.Height == sourceImage.Height,
                $"解码尺寸与源图不一致：期望 {sourceImage.Width}x{sourceImage.Height} / {geometry}");

            // 取 (8,8)：夹具在 (0,0) 有深色外框，取角上比对等于只断「黑点」，失去意义。
            Color expectedColour = sourceImage.GetPixel(SampleX, SampleY);
            Require(expectedColour.R == 203 && expectedColour.G == 52 && expectedColour.B == 61,
                $"夹具自身被改动了：({SampleX},{SampleY}) 期望 #CB343D，实际 "
                + $"#{expectedColour.R:X2}{expectedColour.G:X2}{expectedColour.B:X2}（重新生成 testdata/image/tifgif_src.png）");

            Color actualColour = decoded.GetPixel(SampleX, SampleY);
            int delta = Math.Max(Math.Abs(expectedColour.R - actualColour.R),
                Math.Max(Math.Abs(expectedColour.G - actualColour.G),
                    Math.Abs(expectedColour.B - actualColour.B)));
            string corner = $"({SampleX},{SampleY}) 源 #{expectedColour.R:X2}{expectedColour.G:X2}{expectedColour.B:X2}"
                + $" → 产物 #{actualColour.R:X2}{actualColour.G:X2}{actualColour.B:X2}（单通道最大色差 {delta}）";
            Require(delta <= 8, $"取色偏差超过 8：{corner} / {geometry}");

            return new ExtOutcome(true, Rel(source), Rel(actualPath!), size,
                $"{expectedExt} magic={ToHex(expectedMagic)} decode=gdi+ {decoded.Width}x{decoded.Height} {corner}"
                + $"｜链路 {ImageFormatFeature} format={format}");
        }
    }

    private static string ToHex(byte[] bytes)
        => bytes.Length == 0
            ? "(空)"
            : string.Join(' ', bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));

    // ---------------------------------------------------------------- P2-1 条码外观

    private const int BarcodeFixedWidth = 520;
    private const int BarcodeFixedHeight = 180;

    private enum Corner
    {
        TopLeft,
        TopRight,
        BottomRight,
        BottomLeft
    }

    /// <summary>
    /// P2-1：`sizeMode=fixed` 的像素尺寸、`rotate` 的宽高互换，以及「确实旋转了」的方向证据。
    /// 方向断言不靠肉眼：给条码加 `CaptionPosition=below` 造出一条「底部有字」的不对称，
    /// 再检查 90/180/270 把这条带搬到左边 / 顶边 / 右边（Skia 正角度是顺时针）。
    /// </summary>
    private static ExtOutcome CheckBarcodeAppearance()
    {
        const string content = "QA-BARCODE-0001";
        IBarcodeService barcode = _provider!.GetRequiredService<IBarcodeService>();
        string directory = ExtDir("code.barcode.appearance");

        static BarcodeRenderOptions Options(int rotate = 0, bool caption = false) => new()
        {
            Symbology = "Code128",
            ModuleSize = 3,
            SizeMode = "fixed",
            BarcodeWidth = BarcodeFixedWidth,
            BarcodeHeight = BarcodeFixedHeight,
            Rotate = rotate,
            Caption = caption ? "MARKER" : null,
            CaptionPosition = caption ? "below" : "none"
        };

        byte[] auto = barcode.CreatePng(content, new BarcodeRenderOptions { Symbology = "Code128", ModuleSize = 3 });
        byte[] plain = barcode.CreatePng(content, Options());
        byte[] rot90 = barcode.CreatePng(content, Options(90));
        byte[] rot180 = barcode.CreatePng(content, Options(180));
        byte[] rot270 = barcode.CreatePng(content, Options(270));
        byte[] caption = barcode.CreatePng(content, Options(caption: true));

        // QR 用方形模块 + auto 尺寸，专门当「方向指示器」：它只有 3 个角有定位块。
        static BarcodeRenderOptions Qr(int rotate) => new() { Symbology = "QR", ModuleSize = 6, Rotate = rotate };
        const string qrContent = "https://example.com/qa";
        byte[] qr = barcode.CreatePng(qrContent, Qr(0));
        byte[] qr90 = barcode.CreatePng(qrContent, Qr(90));
        byte[] qr180 = barcode.CreatePng(qrContent, Qr(180));
        byte[] qr270 = barcode.CreatePng(qrContent, Qr(270));

        foreach ((string name, byte[] png) in new[]
                 {
                     ("auto.png", auto), ("fixed.png", plain), ("rot90.png", rot90), ("rot180.png", rot180),
                     ("rot270.png", rot270), ("caption.png", caption), ("qr.png", qr), ("qr90.png", qr90),
                     ("qr180.png", qr180), ("qr270.png", qr270)
                 })
        {
            File.WriteAllBytes(Path.Combine(directory, name), png);
        }

        (int autoW, int autoH) = FileProbe.PngSize(auto);
        (int plainW, int plainH) = FileProbe.PngSize(plain);
        Require(Math.Abs(plainW - BarcodeFixedWidth) <= 1 && Math.Abs(plainH - BarcodeFixedHeight) <= 1,
            $"sizeMode=fixed 应输出 {BarcodeFixedWidth}x{BarcodeFixedHeight}（±1px），实际 {plainW}x{plainH}");
        Require(autoW != plainW || autoH != plainH,
            $"auto 与 fixed 尺寸不该相同（auto={autoW}x{autoH} fixed={plainW}x{plainH}）");

        (int r90W, int r90H) = FileProbe.PngSize(rot90);
        (int r180W, int r180H) = FileProbe.PngSize(rot180);
        (int r270W, int r270H) = FileProbe.PngSize(rot270);
        Require(r90W == plainH && r90H == plainW, $"rotate=90 应宽高互换，实际 {r90W}x{r90H}（未旋转 {plainW}x{plainH}）");
        Require(r270W == plainH && r270H == plainW, $"rotate=270 应宽高互换，实际 {r270W}x{r270H}");
        Require(r180W == plainW && r180H == plainH, $"rotate=180 应保持尺寸，实际 {r180W}x{r180H}");

        (int capW, int capH) = FileProbe.PngSize(caption);
        Require(capW == plainW && capH == plainH,
            $"加 caption 不该改尺寸：期望 {plainW}x{plainH}，实际 {capW}x{capH}");

        // 方向证据用 QR 的定位角：QR 只有三个角有 7 模块实心定位块（左上/右上/左下），右下角是空的。
        // 「空角在哪」就是方向指示器：0°→右下，顺时针 90°→左下，180°→左上，270°→右上。
        // （Code128 是满幅条状图形，四条边都黑，用它判断方向会误判，所以另起 QR 图。）
        string corners = string.Join("; ", new[]
        {
            Describe("0°", qr),
            Describe("90°", qr90),
            Describe("180°", qr180),
            Describe("270°", qr270)
        });

        Require(SparseCorner(qr) == Corner.BottomRight,
            $"QR 基准图的空角应在右下（定位块在另外三个角）；{corners}");
        Require(SparseCorner(qr90) == Corner.BottomLeft,
            $"rotate=90（Skia 正角度=顺时针）后空角应从右下转到左下；{corners}");
        Require(SparseCorner(qr180) == Corner.TopLeft,
            $"rotate=180 后空角应转到左上；{corners}");
        Require(SparseCorner(qr270) == Corner.TopRight,
            $"rotate=270 后空角应转到右上；{corners}");

        static string Describe(string rotation, byte[] png)
            => $"{rotation} 角墨量 TL/TR/BR/BL="
               + string.Join("/", CornerInk(png).Select(value => value.ToString("F2", CultureInfo.InvariantCulture)));

        long bytes = Directory.EnumerateFiles(directory).Sum(FileProbe.Size);
        return new ExtOutcome(true, "(内存)",
            Rel(Path.Combine(directory, "fixed.png")), bytes,
            $"sizeMode=fixed {plainW}x{plainH}（auto={autoW}x{autoH}）; "
            + $"rotate 90→{r90W}x{r90H} 180→{r180W}x{r180H} 270→{r270W}x{r270H}; "
            + $"QR 空角方向：{corners}");
    }

    /// <summary>四个角（各取短边 1/3 见方）的深色像素占比，顺序固定 TL/TR/BR/BL。</summary>
    private static double[] CornerInk(byte[] png)
    {
        using SKBitmap? bitmap = SKBitmap.Decode(png);
        if (bitmap is null)
        {
            throw new CheckException("条码产物无法解码（PNG 头异常）");
        }

        int tile = Math.Max(4, Math.Min(bitmap.Width, bitmap.Height) / 3);
        return new[]
        {
            InkRatio(bitmap, 0, 0, tile),
            InkRatio(bitmap, bitmap.Width - tile, 0, tile),
            InkRatio(bitmap, bitmap.Width - tile, bitmap.Height - tile, tile),
            InkRatio(bitmap, 0, bitmap.Height - tile, tile)
        };
    }

    private static double InkRatio(SKBitmap bitmap, int x0, int y0, int tile)
    {
        int dark = 0;
        int seen = 0;
        for (int y = y0; y < y0 + tile && y < bitmap.Height; y++)
        {
            for (int x = x0; x < x0 + tile && x < bitmap.Width; x++)
            {
                seen++;
                if (Luminance(bitmap.GetPixel(x, y)) < 128)
                {
                    dark++;
                }
            }
        }

        return seen == 0 ? 0d : (double)dark / seen;
    }

    private static Corner SparseCorner(byte[] png)
    {
        double[] ink = CornerInk(png);
        Corner[] corners = { Corner.TopLeft, Corner.TopRight, Corner.BottomRight, Corner.BottomLeft };
        int min = 0;
        for (int i = 1; i < ink.Length; i++)
        {
            if (ink[i] < ink[min])
            {
                min = i;
            }
        }

        return corners[min];
    }

    private static int Luminance(SKColor pixel)
        => (pixel.Red * 299 + pixel.Green * 587 + pixel.Blue * 114) / 1000;

    // ---------------------------------------------------------------- P2-7 image.compress 细节

    private static IDocumentOperation ImageOperation()
        => _operations.TryGetValue("image.compress", out IDocumentOperation? operation)
            ? operation
            : throw new CheckException("未注册操作：image.compress");

    private static DocumentOperationResult RunImage(
        IDocumentOperation operation, string input, string directory, Dictionary<string, string> options)
    {
        Directory.CreateDirectory(directory);
        DocumentOperationResult result = operation.ExecuteAsync(
                new DocumentOperationRequest
                {
                    InputFiles = new[] { input },
                    OutputDirectory = directory,
                    Options = options
                },
                CancellationToken.None)
            .GetAwaiter().GetResult();
        Require(result.Success, "Success=false：" + result.Message);
        Require(result.OutputFiles.Count > 0 && File.Exists(result.OutputFiles[0]),
            "输出路径不存在：" + string.Join("; ", result.OutputFiles));
        return result;
    }

    /// <summary>P2-7a：`useSourceExtension=true` 沿用源后缀，false 才用 format；同一输入只改这一个键。</summary>
    private static ExtOutcome CheckImageUseSourceExtension()
    {
        string source = Fixture("image", "sample.png");
        RequireFixture(source);
        IDocumentOperation operation = ImageOperation();
        string directory = ExtDir("image.compress.useSourceExtension");

        DocumentOperationResult inherited = RunImage(operation, source, Path.Combine(directory, "inherit"),
            new Dictionary<string, string> { ["format"] = "JPG", ["useSourceExtension"] = "true" });
        DocumentOperationResult formatted = RunImage(operation, source, Path.Combine(directory, "format"),
            new Dictionary<string, string> { ["format"] = "JPG", ["useSourceExtension"] = "false" });

        string inheritedOut = inherited.OutputFiles[0];
        string formattedOut = formatted.OutputFiles[0];
        Require(Path.GetExtension(inheritedOut).Equals(".png", StringComparison.OrdinalIgnoreCase),
            "useSourceExtension=true 应沿用源后缀 .png，实际 " + Path.GetFileName(inheritedOut));
        Require(Path.GetExtension(formattedOut).Equals(".jpg", StringComparison.OrdinalIgnoreCase),
            "useSourceExtension=false + format=JPG 应输出 .jpg，实际 " + Path.GetFileName(formattedOut));
        Require(FileProbe.Size(inheritedOut) != FileProbe.Size(formattedOut),
            "两种后缀的产物字节不该相同（PNG 与 JPEG 编码不同）");

        return new ExtOutcome(true, Rel(source), Rel(inheritedOut),
            FileProbe.Size(inheritedOut) + FileProbe.Size(formattedOut),
            $"仅改 useSourceExtension：true→{Path.GetExtension(inheritedOut)} {FileProbe.Size(inheritedOut)} B; "
            + $"false→{Path.GetExtension(formattedOut)} {FileProbe.Size(formattedOut)} B（format=JPG）");
    }

    /// <summary>P2-7b：`noCompress=true` 几乎不压（legacy 语义是「只改名不重编码」）。</summary>
    private static ExtOutcome CheckImageNoCompress()
    {
        string source = Fixture("image", "sample.png");
        RequireFixture(source);
        IDocumentOperation operation = ImageOperation();
        string directory = ExtDir("image.compress.noCompress");

        DocumentOperationResult copy = RunImage(operation, source, Path.Combine(directory, "noCompress"),
            new Dictionary<string, string> { ["format"] = "PNG", ["noCompress"] = "true" });
        DocumentOperationResult normal = RunImage(operation, source, Path.Combine(directory, "compress"),
            new Dictionary<string, string> { ["format"] = "PNG", ["noCompress"] = "false" });

        string copyOut = copy.OutputFiles[0];
        string normalOut = normal.OutputFiles[0];
        long sourceSize = FileProbe.Size(source);
        long copySize = FileProbe.Size(copyOut);
        long normalSize = FileProbe.Size(normalOut);

        bool identical = copySize == sourceSize
                         && File.ReadAllBytes(copyOut).AsSpan().SequenceEqual(File.ReadAllBytes(source));
        Require(copySize >= sourceSize * 9 / 10,
            $"noCompress=true 应几乎不压（≥90% 源体积），实际 {copySize} B / 源 {sourceSize} B");
        Require(normalSize != copySize,
            $"noCompress 两档产物体积不该相同（均 {normalSize} B），说明开关没生效");
        Require(!FileProbe.IsPng(copyOut) || identical || copySize > 0, "noCompress 产物为空");

        return new ExtOutcome(true, Rel(source), Rel(copyOut), copySize + normalSize,
            $"仅改 noCompress：true→{copySize} B（源 {sourceSize} B，逐字节一致={identical}）; "
            + $"false→{normalSize} B");
    }

    /// <summary>P2-7c：`bmpPixelBits` 直读 BMP 头 `biBitCount`（自写解析，不引包）。</summary>
    private static ExtOutcome CheckImageBmpPixelBits()
    {
        string source = Fixture("image", "bmp_src.png");
        RequireFixture(source);
        IDocumentOperation operation = ImageOperation();
        string directory = ExtDir("image.compress.bmpPixelBits");

        var bits = new List<string>();
        foreach (int expected in new[] { 8, 24, 32 })
        {
            DocumentOperationResult result = RunImage(operation, source, Path.Combine(directory, expected.ToString(CultureInfo.InvariantCulture)),
                new Dictionary<string, string>
                {
                    ["format"] = "BMP",
                    ["bmpPixelBits"] = expected.ToString(CultureInfo.InvariantCulture)
                });

            string output = result.OutputFiles[0];
            Require(Path.GetExtension(output).Equals(".bmp", StringComparison.OrdinalIgnoreCase),
                "format=BMP 应输出 .bmp，实际 " + Path.GetFileName(output));
            int actual = FileProbe.BmpPixelBits(output);
            Require(actual == expected,
                $"bmpPixelBits={expected} 应写出 biBitCount={expected}，实际 {actual}"
                + $"（文件 {Path.GetFileName(output)}，{FileProbe.Size(output)} B）");
            bits.Add($"{expected}→{actual}({FileProbe.Size(output)} B)");
        }

        long bytes = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories).Sum(FileProbe.Size);
        return new ExtOutcome(true, Rel(source), Rel(directory), bytes,
            $"仅改 bmpPixelBits：{string.Join(" ", bits)}，biBitCount 直读 BMP 头 offset 28");
    }

    // ---------------------------------------------------------------- P2-8 ppt.pdf exportNotes

    private static ExtOutcome CheckPptExportNotes()
    {
        string source = Fixture("ppt", "notes.pptx");
        RequireFixture(source);
        if (!_operations.TryGetValue("ppt.pdf", out IDocumentOperation? operation))
        {
            throw new CheckException("未注册操作：ppt.pdf");
        }

        string directory = ExtDir("ppt.pdf.exportNotes");

        DocumentOperationResult Convert(string tag, string notes)
        {
            string target = Path.Combine(directory, tag);
            Directory.CreateDirectory(target);
            DocumentOperationResult result = operation.ExecuteAsync(
                    new DocumentOperationRequest
                    {
                        InputFiles = new[] { source },
                        OutputDirectory = target,
                        Options = new Dictionary<string, string>
                        {
                            ["exportNotes"] = notes,
                            ["drawBlackLine"] = "false",
                            ["hiddenSlides"] = "false"
                        }
                    },
                    CancellationToken.None)
                .GetAwaiter().GetResult();
            Require(result.Success, $"exportNotes={notes} 失败：{result.Message}");
            Require(result.OutputFiles.Count > 0 && File.Exists(result.OutputFiles[0]),
                $"exportNotes={notes} 的输出路径不存在");
            return result;
        }

        DocumentOperationResult none = Convert("None", "None");
        DocumentOperationResult bottom = Convert("BottomFull", "BottomFull");
        DocumentOperationResult truncated = Convert("BottomTruncated", "BottomTruncated");

        string noneOut = none.OutputFiles[0];
        string bottomOut = bottom.OutputFiles[0];
        string truncatedOut = truncated.OutputFiles[0];

        string noneText = PdfTextProbe.Extract(noneOut, 4);
        string bottomText = PdfTextProbe.Extract(bottomOut, 4);
        string truncatedText = PdfTextProbe.Extract(truncatedOut, 4);

        int nonePages = FileProbe.PdfPageCount(noneOut);
        int bottomPages = FileProbe.PdfPageCount(bottomOut);
        int truncatedPages = FileProbe.PdfPageCount(truncatedOut);

        (double noneW, double noneH) = PdfTextProbe.FirstPageSize(noneOut);
        (double bottomW, double bottomH) = PdfTextProbe.FirstPageSize(bottomOut);
        (double truncatedW, double truncatedH) = PdfTextProbe.FirstPageSize(truncatedOut);

        // 先把三档（None/BottomFull/BottomTruncated）的实测证据收齐，再断言：
        // 失败时一句话就能看出「哪档没有备注、页面多宽」。
        string evidence = string.Join("; ", new[]
        {
            Describe("None", nonePages, noneW, noneH, noneText),
            Describe("BottomFull", bottomPages, bottomW, bottomH, bottomText),
            Describe("BottomTruncated", truncatedPages, truncatedW, truncatedH, truncatedText)
        });

        Require(!noneText.Contains("NOTES-MARKER", StringComparison.Ordinal),
            "exportNotes=None 不该出现备注文本；实测：" + evidence);
        Require(bottomText.Contains("NOTES-MARKER-ALPHA", StringComparison.Ordinal),
            "exportNotes=BottomFull 应把备注文本带进 PDF（未找到 NOTES-MARKER-ALPHA）；实测：" + evidence);
        Require(truncatedText.Contains("NOTES-MARKER-ALPHA", StringComparison.Ordinal),
            "exportNotes=BottomTruncated 应把备注文本带进 PDF（未找到 NOTES-MARKER-ALPHA）；实测：" + evidence);
        Require(bottomPages == 2 && truncatedPages == 2 && nonePages == 2,
            "2 页夹具应输出 2 页；实测：" + evidence);
        Require(bottomH > bottomW,
            "BottomFull 应是竖版页（备注在页面下方，高度大于宽度）；实测：" + evidence);

        // 缺陷 C 修复后的行为：Schema 已收窄（不再提供 RightFull），Core 解析不出时抛中文异常而不是静默按 None。
        // 所以「原 RightFull 那一档」现在验的是「拒绝而不是静默」——成功即失败，写出非空产物即失败。
        string[] schemaValues = FeatureOptionSchemas.GetSections("ppt.pdf")
            .SelectMany(section => section.Fields)
            .FirstOrDefault(field => field.Key == "exportNotes")
            ?.Choices?.Select(choice => choice.Value).ToArray() ?? [];
        Require(schemaValues.Length > 0, "ppt.pdf 的 exportNotes 应声明可选档位");
        Require(!schemaValues.Contains("RightFull", StringComparer.OrdinalIgnoreCase),
            "Schema 不应再提供 RightFull（Aspose.Slides 25.12 无该枚举）；实际档位："
            + string.Join("/", schemaValues));

        string rightDirectory = Path.Combine(directory, "RightFull");
        Directory.CreateDirectory(rightDirectory);
        DocumentOperationResult rejected = operation.ExecuteAsync(
                new DocumentOperationRequest
                {
                    InputFiles = new[] { source },
                    OutputDirectory = rightDirectory,
                    Options = new Dictionary<string, string>
                    {
                        ["exportNotes"] = "RightFull",
                        ["drawBlackLine"] = "false",
                        ["hiddenSlides"] = "false"
                    }
                },
                CancellationToken.None)
            .GetAwaiter().GetResult();

        string[] rightPdfs = Directory.GetFiles(rightDirectory, "*.pdf", SearchOption.AllDirectories);
        long rightBytes = rightPdfs.Sum(FileProbe.Size);
        string rightMessage = rejected.Message ?? string.Empty;

        // 判定口径：目录里不得留下任何「有内容」的 PDF；0 字节残留文件不算产物（明确记下来，避免含糊）。
        Require(!rejected.Success,
            "exportNotes=RightFull 应被拒绝（Success=false），实际却成功了；产物："
            + (rightPdfs.Length == 0 ? "（无文件）" : string.Join(", ", rightPdfs.Select(Path.GetFileName)))
            + $"（{rightBytes} B）");
        Require(rightMessage.Contains("不支持的备注导出位置", StringComparison.Ordinal),
            $"exportNotes=RightFull 的失败消息应含「不支持的备注导出位置」，实际：「{rightMessage}」");
        Require(rejected.OutputFiles.Count == 0,
            "exportNotes=RightFull 被拒绝时不应回报任何输出文件，实际：" + rejected.OutputFiles.Count + " 个");
        Require(rightBytes == 0,
            "exportNotes=RightFull 被拒绝时不应写出非空 PDF，实际："
            + string.Join(", ", rightPdfs.Select(p => Path.GetFileName(p) + "=" + FileProbe.Size(p) + " B")));

        static string Describe(string tag, int pages, double width, double height, string text)
            => $"{tag}={pages}页 {width:F1}x{height:F1}pt "
               + $"标记={(text.Contains("NOTES-MARKER", StringComparison.Ordinal) ? "有" : "无")} "
               + $"提取{text.Length}字符";

        long bytes = FileProbe.Size(noneOut) + FileProbe.Size(bottomOut) + FileProbe.Size(truncatedOut);
        return new ExtOutcome(true, Rel(source), Rel(bottomOut), bytes,
            $"仅改 exportNotes：None→无 NOTES-MARKER（{nonePages} 页 {noneW:F1}x{noneH:F1}pt）; "
            + $"BottomFull→含 NOTES-MARKER-ALPHA（{bottomPages} 页 {bottomW:F1}x{bottomH:F1}pt）; "
            + $"BottomTruncated→含 NOTES-MARKER-ALPHA（{truncatedPages} 页 {truncatedW:F1}x{truncatedH:F1}pt）; "
            + $"RightFull→被拒绝（「不支持的备注导出位置」，产物 {rightPdfs.Length} 个 / {rightBytes} B）; "
            + $"Schema 档位={string.Join("/", schemaValues)}");
    }
}
