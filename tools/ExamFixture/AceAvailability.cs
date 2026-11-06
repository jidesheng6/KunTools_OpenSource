using DocumentTools.Core.Data;

namespace DocumentTools.ExamFixture;

/// <summary>
/// 探测本机是否具备可用的 ACE/Jet OLE DB 通道。
/// 集成测试用它决定是否跳过：缺 provider 的机器上测试必须"跳过并说明原因"，而不是报红。
/// 探测包含两步，因为夹具生成同时依赖两者：ADOX 建库 + OLE DB 打开库。
/// </summary>
public static class AceAvailability
{
    private static readonly Lazy<(bool Available, string Reason)> LazyProbe = new(Probe, true);

    /// <summary>
    /// 把该环境变量设为 1 可强制模拟"本机没有 ACE"。存在的意义：跳过路径必须可验证，
    /// 否则"缺 provider 也不报红"只是口头承诺（本机装了 ACE，无法靠卸载来试）。
    /// 判定逻辑已下沉到 Core（<see cref="AccessDb.SimulateMissingAceVariable"/> 与
    /// <see cref="AccessDb.IsMissingAceSimulated"/>），这里只做别名，不再维护第二份实现。
    /// </summary>
    public const string SimulateMissingVariable = AccessDb.SimulateMissingAceVariable;

    public static bool Available => LazyProbe.Value.Available;

    public static string Reason => LazyProbe.Value.Reason;

    public static (bool Available, string Reason) Result => LazyProbe.Value;

    private static (bool Available, string Reason) Probe()
    {
        // 用"真的生成一套空夹具"来探测：这一步同时覆盖 ADOX 建库、ACE/Jet 打开、建表与查询，
        // 与集成测试随后要做的事完全一致，避免用某个特殊系统表探出假阴性。
        if (AccessDb.IsMissingAceSimulated())
        {
            return (false,
                "未检测到可用的 Access OLE DB 提供者（模拟：环境变量 " + SimulateMissingVariable + "=1），"
                + "考评 Access 集成测试依赖真实 .mdb，已跳过。");
        }

        string? root = null;
        try
        {
            ExamFixtureSet fixture = ExamFixtureBuilder.Create(
                ExamFixtureBuilder.CreateRootDirectory(),
                new ExamFixtureOptions { SeedSampleRows = false });
            root = fixture.DirectoryPath;
            using (var session = fixture.OpenDatabase())
            {
                session.Scalar("SELECT COUNT(*) FROM Exam_EssInfo");
            }

            fixture.Dispose();
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false,
                "未检测到可用的 Access OLE DB 提供者（Microsoft.ACE.OLEDB.12.0 / 16.0 / Jet 4.0，当前进程 "
                + (Environment.Is64BitProcess ? "64" : "32") + " 位），"
                + "考评 Access 集成测试依赖真实 .mdb，已跳过。根本原因："
                + AceDiagnostics.Chain(ex));
        }
        finally
        {
            if (root is not null)
            {
                try
                {
                    Directory.Delete(root, recursive: true);
                }
                catch (IOException)
                {
                    // 临时探测目录清理失败不影响结论
                }
                catch (UnauthorizedAccessException)
                {
                    // 同上
                }
            }
        }
    }
}
