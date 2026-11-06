using DocumentTools.ExamFixture;

namespace DocumentTools.ExamFixture;

/// <summary>
/// 考评 Access 夹具生成器 CLI。
/// 用法：ExamFixture [--output &lt;目录&gt;] [--no-seed] [--no-boolitem] [--probe-autoincrement] [--selfreport]
/// 退出码：0 生成成功；2 本机缺少 ACE/Jet（不算部署故障）；1 其它失败。
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        string? output = null;
        bool seed = true;
        bool includeBoolItem = true;
        bool probeAutoIncrement = false;
        bool selfReport = false;
        bool benchMerge = false;
        int people = 0;
        int units = 0;
        int targetPeople = 0;
        int repeat = 1;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--output" or "-o":
                    if (i + 1 >= args.Length)
                    {
                        return Usage("--output 缺少目录参数");
                    }

                    output = args[++i];
                    break;
                case "--people" or "-p":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out people))
                    {
                        return Usage("--people 需要一个整数");
                    }

                    break;
                case "--units" or "-u":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out units))
                    {
                        return Usage("--units 需要一个整数");
                    }

                    break;
                case "--target-people":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out targetPeople))
                    {
                        return Usage("--target-people 需要一个整数");
                    }

                    break;
                case "--repeat":
                    if (i + 1 >= args.Length || !int.TryParse(args[++i], out repeat))
                    {
                        return Usage("--repeat 需要一个整数");
                    }

                    break;
                case "--bench-merge":
                    benchMerge = true;
                    break;
                case "--no-seed":
                    seed = false;
                    break;
                case "--no-boolitem":
                    includeBoolItem = false;
                    break;
                case "--probe-autoincrement":
                    probeAutoIncrement = true;
                    break;
                case "--selfreport":
                    selfReport = true;
                    break;
                case "--diagnose":
                    Console.WriteLine(AceDiagnostics.Describe());
                    return 0;
                case "--verify":
                    if (i + 1 >= args.Length)
                    {
                        return Usage("--verify 缺少数据库路径");
                    }

                    Console.WriteLine(AceDiagnostics.VerifyExisting(args[++i]));
                    return 0;
                case "--help" or "-h" or "/?":
                    PrintUsage();
                    return 0;
                default:
                    return Usage("未知参数：" + args[i]);
            }
        }

        if (!AceAvailability.Available)
        {
            Console.Error.WriteLine("[ExamFixture] " + AceAvailability.Reason);
            return 2;
        }

        var options = new ExamFixtureOptions
        {
            IncludeExamBoolItem = includeBoolItem,
            IncludeAutoIncrementProbeColumn = probeAutoIncrement,
            SeedSampleRows = seed,
            CreateSelfReportFolder = selfReport,
            SeedPeopleCount = people,
            SeedUnitCount = units
        };

        try
        {
            if (benchMerge)
            {
                return MergeBench.Run(people, units, targetPeople, repeat);
            }

            using ExamFixtureSet fixture = ExamFixtureBuilder.Create(output, options);
            Console.WriteLine("[ExamFixture] 夹具目录: " + fixture.DirectoryPath);
            Console.WriteLine("[ExamFixture] 数据库:   " + fixture.DatabaseFile);
            using (var session = fixture.OpenDatabase())
            {
                foreach ((string table, string count) in Describe(session))
                {
                    Console.WriteLine($"[ExamFixture]   {table,-22} 行数 {count}");
                }
            }

            Console.WriteLine("[ExamFixture] 生成完成。重复执行请先删除该目录（或换一个 --output）。");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[ExamFixture] 生成失败：" + ex.GetBaseException().Message);
            return 1;
        }
    }

    private static IEnumerable<(string Table, string Count)> Describe(DocumentTools.Core.Data.AccessDb session)
    {
        string[] tables =
        [
            "Vote_EssInfo", "Vote_PageInfo", "Exam_EssInfo", "Exam_Score", "Exam_BoolItem", "Exam_Ruler",
            "Exam_People", "Exam_PeopleDes", "Exam_CharItem", "Exam_VerifyCode", "Exam_RaidInfo",
            "Unit_Ess", "Report_Ess", "Report_Detail", "Page_SelectorInfo",
            "Result_DetailInfo", "Result_NumInfo", "TempResult_DetailInfo"
        ];

        foreach (string table in tables)
        {
            string count;
            try
            {
                count = Convert.ToString(session.Scalar($"SELECT COUNT(*) FROM {table}")) ?? "?";
            }
            catch (Exception ex)
            {
                count = "缺失(" + ex.GetBaseException().Message + ")";
            }

            yield return (table, count);
        }
    }

    private static int Usage(string message)
    {
        Console.Error.WriteLine("[ExamFixture] " + message);
        PrintUsage();
        return 1;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("用法: ExamFixture [--output <目录>] [--no-seed] [--no-boolitem] [--probe-autoincrement] [--selfreport]");
        Console.WriteLine("                ExamFixture --bench-merge --people <N> --units <M> [--target-people <K>] [--repeat <R>]");
        Console.WriteLine("  --output/-o <目录>     指定夹具目录（默认 %TEMP%\\dtexam-fixture-<guid>，必须新建或空目录）");
        Console.WriteLine("  --people/-p <N>        合成 N 行 Exam_People（配合 --units）");
        Console.WriteLine("  --units/-u <M>         合成 M 行 Unit_Ess");
        Console.WriteLine("  --no-seed              只建空表，不写样例行");
        Console.WriteLine("  --no-boolitem          不建 Exam_BoolItem（用于验证插入失败回滚）");
        Console.WriteLine("  --probe-autoincrement  给 Exam_People/Unit_Ess 加 COUNTER 探针列（验证 ADOX 自增列路径）");
        Console.WriteLine("  --selfreport           额外创建 SelfReport 目录");
        Console.WriteLine("  --bench-merge          造源/目标夹具并量 Merge 的耗时与峰值内存（见 MergeBench）");
        Console.WriteLine("  --target-people <K>    量测时目标库预置 K 行人员（默认 0）");
        Console.WriteLine("  --repeat <R>           量测重复次数，第 1 次作预热（默认 1）");
    }
}
