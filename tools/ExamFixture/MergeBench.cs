using System.Diagnostics;
using DocumentTools.Core.Data;
using DocumentTools.Core.Services.Exam;

namespace DocumentTools.ExamFixture;

/// <summary>
/// 数据包合并的量测入口：造一份「N 人 / M 单位」的源夹具 + 一份空目标夹具，
/// 跑一次 <see cref="ExamPackageService.Merge"/> 并打印耗时与内存。
///
/// 只用 ExamFixtureBuilder 造库（不手写 SQL），保证量测的库结构与集成测试里的完全一致。
/// 内存用两个口径同时给出：
/// 1. 进程 PeakWorkingSet64（含 ACE 非托管页）——判断"会不会把机器吃满"看这个；
/// 2. GC.GetTotalMemory(true) 前后差（托管堆）——判断算法本身有没有把整表留在内存里。
/// </summary>
internal static class MergeBench
{
    internal static int Run(int sourcePeople, int sourceUnits, int targetPeople, int repeat)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sourcePeople);
        ArgumentOutOfRangeException.ThrowIfNegative(sourceUnits);

        if (repeat < 1)
        {
            repeat = 1;
        }

        Console.WriteLine($"[MergeBench] 源夹具：{sourcePeople} 人 / {sourceUnits} 单位；目标首行数：{targetPeople}；重复 {repeat} 次");

        using ExamFixtureSet source = ExamFixtureBuilder.Create(
            ExamFixtureBuilder.CreateRootDirectory(),
            new ExamFixtureOptions
            {
                SeedPeopleCount = sourcePeople,
                SeedUnitCount = sourceUnits
            });

        Console.WriteLine("[MergeBench] 源库已建：" + source.DatabaseFile);

        using ExamFixtureSet target = ExamFixtureBuilder.Create(
            ExamFixtureBuilder.CreateRootDirectory(),
            new ExamFixtureOptions
            {
                SeedSampleRows = targetPeople > 0,
                SeedPeopleCount = targetPeople,
                SeedUnitCount = targetPeople > 0 ? Math.Max(1, targetPeople / 25) : 0
            });

        Console.WriteLine("[MergeBench] 目标库已建：" + target.DatabaseFile);

        var service = new ExamPackageService();
        var request = new ExamPackageMergeRequest
        {
            SourceDirectory = source.DirectoryPath,
            TargetDirectory = target.DirectoryPath,
            CopyExamFiles = false
        };

        // 预热：首次调用要付 JIT、provider 初始化、ADOX 探测的固定成本；
        // 与"随输入规模增长"的那部分分开看，否则 5000 人那次会把固定成本算进去。
        long baselineManaged = GC.GetTotalMemory(forceFullCollection: true);
        long baselineWorkingSet = Process.GetCurrentProcess().WorkingSet64;

        double warmupSeconds = 0;
        if (repeat > 1)
        {
            var warmup = Stopwatch.StartNew();
            service.Merge(request);
            warmup.Stop();
            warmupSeconds = warmup.Elapsed.TotalSeconds;
            Console.WriteLine($"[MergeBench] 预热一次（不计入统计）：{warmupSeconds:F2} 秒");
        }

        double slowestSeconds = 0;
        double totalSeconds = 0;
        ExamPackageMergeResult? last = null;
        for (int round = 1; round <= repeat; round++)
        {
            var watch = Stopwatch.StartNew();
            last = service.Merge(request);
            watch.Stop();

            double seconds = watch.Elapsed.TotalSeconds;
            totalSeconds += seconds;
            slowestSeconds = Math.Max(slowestSeconds, seconds);
            Console.WriteLine(
                $"[MergeBench] 第 {round} 轮：{seconds:F2} 秒，插入人员 {last.PeopleInserted}，插入单位 {last.UnitsInserted}，"
                + $"重复单位 {last.DuplicateUnitsRemoved}");
        }

        // 合并结束后把托管堆压一遍再读：这一轮的总量测。
        long afterManaged = GC.GetTotalMemory(forceFullCollection: true);
        Process process = Process.GetCurrentProcess();
        process.Refresh();
        long peakWorkingSet = process.PeakWorkingSet64;

        using (var session = target.OpenDatabase())
        {
            object? people = session.Scalar("SELECT COUNT(*) FROM Exam_People");
            object? units = session.Scalar("SELECT COUNT(*) FROM Unit_Ess");
            object? maxOrder = session.Scalar("SELECT MAX(AtOrder) FROM Unit_Ess");
            Console.WriteLine($"[MergeBench] 目标库现状：Exam_People {people} 行，Unit_Ess {units} 行，MAX(AtOrder)={maxOrder}");
        }

        Console.WriteLine($"[MergeBench] 最慢一轮耗时：{slowestSeconds:F2} 秒；合计 {totalSeconds:F2} 秒（{repeat} 轮，含预热 {warmupSeconds:F2} 秒）");
        Console.WriteLine($"[MergeBench] 托管堆：合并前 {Mb(baselineManaged)} MB → 合并后 {Mb(afterManaged)} MB，净增 {Mb(afterManaged - baselineManaged)} MB");
        Console.WriteLine($"[MergeBench] 进程工作集：合并前 {Mb(baselineWorkingSet)} MB，峰值 {Mb(peakWorkingSet)} MB");
        Console.WriteLine($"[MergeBench] 判定口径：耗时阈值 60 秒、峰值阈值 500 MB（峰值口径=进程 PeakWorkingSet64）");
        Console.WriteLine(
            slowestSeconds > 60 || peakWorkingSet > 500L * 1024 * 1024
                ? "[MergeBench] 结论：超过阈值，需要写进 docs\\feature-gap.md §9。"
                : "[MergeBench] 结论：未超阈值。");

        if (last is null)
        {
            Console.Error.WriteLine("[MergeBench] 没有任何一轮完成。");
            return 1;
        }

        return last.PeopleInserted == sourcePeople ? 0 : 1;
    }

    private static string Mb(long bytes) => (bytes / 1024.0 / 1024.0).ToString("F1");
}
