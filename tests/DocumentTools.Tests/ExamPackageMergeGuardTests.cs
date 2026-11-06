using DocumentTools.Core.Data;
using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// exam.package 合并的两条守门断言：
/// 1. 目标库不可写时（这里用只读属性模拟"被占用/被别的会话锁住"），合并必须抛「能照着做」的
///    中文消息（指出是哪一端、已回滚、源未改动），且源库一行都不许动；
/// 2. 5 千人 / 200 单位这种规模下合并仍然完整落库（并把实测耗时打出来，供性能回归对照）。
///
/// 为什么不用 FileStream(FileShare.None)：ACE 一旦在本进程里打开过 .mdb，句柄会留在 OLE DB
/// 会话池里直到进程退出，独占打开 20 次 × 100ms 重试依然报 "being used by another process"，
/// 因此进程内造不出"独占"；只读属性走的是同一条写入失败 + 回滚路径，且可复现。
///
/// 规模断言选 5000/200 而不是更小，是因为工具侧 MergeBench 就是按这个口径量的
/// （5000 人 / 200 单位实测约 1.9 秒、进程峰值约 85 MB），测试与量测用同一组数字才能互相印证。
/// </summary>
public sealed class ExamPackageMergeGuardTests : IDisposable
{
    private const int ScalePeople = 5000;

    private const int ScaleUnits = 200;

    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-merge-guard-" + Guid.NewGuid().ToString("n"));

    public ExamPackageMergeGuardTests()
    {
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                if (Directory.Exists(_root))
                {
                    Directory.Delete(_root, recursive: true);
                }

                return;
            }
            catch (IOException)
            {
                Thread.Sleep(50);
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(50);
            }
        }
    }

    [AceFact]
    public void Merge_TargetLockedExclusively_ThrowsActionableChineseMessageAndLeavesSourceAlone()
    {
        using ExamFixtureSet source = ExamFixtureBuilder.Create(
            Path.Combine(_root, "source"),
            new ExamFixtureOptions { SeedPeopleCount = 40, SeedUnitCount = 4 });
        using ExamFixtureSet target = ExamFixtureBuilder.Create(Path.Combine(_root, "target"));

        int sourcePeopleBefore = Count(source, "Exam_People");
        int sourceUnitsBefore = Count(source, "Unit_Ess");
        int targetPeopleBefore = Count(target, "Exam_People");

        var service = new ExamPackageService();
        var request = new ExamPackageMergeRequest
        {
            SourceDirectory = source.DirectoryPath,
            TargetDirectory = target.DirectoryPath,
            CopyExamFiles = false
        };

        Exception? failure = null;
        // 目标库只读 = 另一种"被占用"：ACE 能打开（读），但事务里的 INSERT 会失败，
        // 因此这条走的是"合并中途失败 → 回滚"路径，比"打开就失败"更贴近真实占用场景。
        File.SetAttributes(target.DatabaseFile, FileAttributes.ReadOnly);
        try
        {
            try
            {
                service.Merge(request);
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        }
        finally
        {
            File.SetAttributes(target.DatabaseFile, FileAttributes.Normal);
        }

        Assert.NotNull(failure);
        string message = AllMessages(failure!);
        Assert.Contains("目标数据包", message, StringComparison.Ordinal);
        Assert.Contains("已回滚", message, StringComparison.Ordinal);
        Assert.Contains("源数据包未改动", message, StringComparison.Ordinal);
        Assert.True(IsChinese(message), "消息应为中文可操作提示，实际：" + message);
        // 原始 ACE 报错要留在消息里（不吞证据），否则排查时看不到真实原因。
        Assert.Contains("操作必须使用一个可更新的查询", message, StringComparison.Ordinal);

        // 源库一行都不许动；目标库也不该留下半成品（合并事务未开始就失败了）。
        Assert.Equal(sourcePeopleBefore, Count(source, "Exam_People"));
        Assert.Equal(sourceUnitsBefore, Count(source, "Unit_Ess"));
        Assert.Equal(targetPeopleBefore, Count(target, "Exam_People"));

        // 放开占用后同一请求必须能成功——证明失败原因是占用，不是别的东西坏了。
        ExamPackageMergeResult retried = service.Merge(request);
        Assert.Equal(sourcePeopleBefore, retried.PeopleInserted);
    }

    [AceFact]
    public void Merge_FiveThousandPeopleAndTwoHundredUnits_InsertsEveryRowWithFlattenedOrder()
    {
        using ExamFixtureSet source = ExamFixtureBuilder.Create(
            Path.Combine(_root, "bulk-source"),
            new ExamFixtureOptions { SeedPeopleCount = ScalePeople, SeedUnitCount = ScaleUnits });
        using ExamFixtureSet target = ExamFixtureBuilder.Create(
            Path.Combine(_root, "bulk-target"),
            new ExamFixtureOptions { SeedSampleRows = false });

        var service = new ExamPackageService();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        ExamPackageMergeResult result = service.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = source.DirectoryPath,
            TargetDirectory = target.DirectoryPath,
            CopyExamFiles = false
        });
        watch.Stop();

        Assert.Equal(ScalePeople, result.PeopleInserted);
        Assert.Equal(ScaleUnits, result.UnitsInserted);
        Assert.Equal(ScalePeople, Count(target, "Exam_People"));
        Assert.Equal(ScaleUnits, Count(target, "Unit_Ess"));

        using (AccessDb session = target.OpenDatabase())
        {
            Assert.Equal(ScaleUnits, Convert.ToInt32(session.Scalar("SELECT MAX(AtOrder) FROM Unit_Ess")));
        }

        // 守门阈值与工具侧 MergeBench 的判定口径一致：60 秒。
        Assert.True(
            watch.Elapsed < TimeSpan.FromSeconds(60),
            $"合并 {ScalePeople} 人 / {ScaleUnits} 单位耗时 {watch.Elapsed.TotalSeconds:F2} 秒，已超过 60 秒阈值");
    }

    private static int Count(ExamFixtureSet fixture, string table)
    {
        using AccessDb session = fixture.OpenDatabase();
        return Convert.ToInt32(session.Scalar($"SELECT COUNT(*) FROM {table}"));
    }

    private static string AllMessages(Exception exception)
    {
        var text = new System.Text.StringBuilder();
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            text.Append(current.Message).Append(' ');
        }

        return text.ToString();
    }

    private static bool IsChinese(string text)
        => text.Any(ch => ch >= '\u4e00' && ch <= '\u9fff');
}
