using DocumentTools.Core.Data;

namespace DocumentTools.Tests;

/// <summary>
/// AccessDb 打开失败时的错误归因。QA 实测发现旧实现把所有打不开的情况都写成
/// "请安装 ACE"，连文件损坏 / 被占用 / 不是 Access 格式也套用同一句，用户按提示装了 ACE 也没用。
/// 这里固定分级后的契约：缺 provider、文件问题、其它，三类消息互不混用。
/// </summary>
public sealed class AccessDbFailureMessageTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "dt-access-fail-" + Guid.NewGuid().ToString("n"));

    public AccessDbFailureMessageTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void MissingFile_StillReportsFileNotFound()
    {
        string missing = Path.Combine(_root, "not-there.mdb");

        FileNotFoundException error = Assert.Throws<FileNotFoundException>(() => new AccessDb(missing));

        Assert.Contains(missing, error.Message, StringComparison.Ordinal);
    }

    /// <summary>
    /// 坏文件（64 字节垃圾）必须落到"文件可能损坏"分支，并且点名文件，
    /// 而不是把用户引去安装 ACE。该分支要求本机真的有 provider，否则无从区分。
    /// </summary>
    [AceFact]
    public void CorruptFile_ReportsFileProblemInsteadOfMissingAce()
    {
        string corrupt = Path.Combine(_root, "corrupt.mdb");
        File.WriteAllBytes(corrupt, new byte[64]);

        var error = Assert.Throws<InvalidOperationException>(() => new AccessDb(corrupt));

        Assert.Contains("corrupt.mdb", error.Message, StringComparison.Ordinal);
        Assert.True(
            error.Message.Contains("损坏", StringComparison.Ordinal)
            || error.Message.Contains("无法打开", StringComparison.Ordinal),
            "坏文件的提示应指向文件本身，实际：" + error.Message);
        Assert.NotNull(error.InnerException);
        // 保留的原始异常必须是"有信息量"的那条（ACE 的格式错误），不能是 Jet 的"未注册"噪音。
        Assert.DoesNotContain(
            "provider is not registered",
            error.InnerException?.Message ?? string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 模拟缺少 ACE：环境变量一开，打开任何存在的文件都必须报"需要 Access Database Engine"。
    /// 这条路径以前只有 tools 侧认得，装好 ACE 的机器上 Core 永远不会走到，等于没有回归。
    /// </summary>
    [Fact]
    public void SimulatedMissingAce_ReportsEngineHintOnAnyExistingFile()
    {
        string anyFile = Path.Combine(_root, "any.mdb");
        File.WriteAllBytes(anyFile, new byte[8]);

        string? previous = Environment.GetEnvironmentVariable(AccessDb.SimulateMissingAceVariable);
        try
        {
            Environment.SetEnvironmentVariable(AccessDb.SimulateMissingAceVariable, "1");

            var error = Assert.Throws<InvalidOperationException>(() => new AccessDb(anyFile));

            Assert.Contains("Microsoft Access Database Engine", error.Message, StringComparison.Ordinal);
        }
        finally
        {
            // 必须还原：这个变量是进程级的，泄漏会让并发的其它 Access 集成测试集体失败。
            Environment.SetEnvironmentVariable(AccessDb.SimulateMissingAceVariable, previous);
        }
    }

    [Fact]
    public void SimulatedMissingAceFlag_ReadsOneAndTrueOnly()
    {
        string? previous = Environment.GetEnvironmentVariable(AccessDb.SimulateMissingAceVariable);
        try
        {
            Environment.SetEnvironmentVariable(AccessDb.SimulateMissingAceVariable, "0");
            Assert.False(AccessDb.IsMissingAceSimulated());

            Environment.SetEnvironmentVariable(AccessDb.SimulateMissingAceVariable, "true");
            Assert.True(AccessDb.IsMissingAceSimulated());

            Environment.SetEnvironmentVariable(AccessDb.SimulateMissingAceVariable, " 1 ");
            Assert.True(AccessDb.IsMissingAceSimulated());

            Environment.SetEnvironmentVariable(AccessDb.SimulateMissingAceVariable, null);
            Assert.False(AccessDb.IsMissingAceSimulated());
        }
        finally
        {
            Environment.SetEnvironmentVariable(AccessDb.SimulateMissingAceVariable, previous);
        }
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
            // 临时目录清理失败不影响结论
        }
        catch (UnauthorizedAccessException)
        {
            // 同上
        }
    }
}
