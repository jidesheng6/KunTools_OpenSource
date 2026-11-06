using DocumentTools.Core.Common;

namespace DocumentTools.Tests;

/// <summary>
/// 视图解耦带出来的两个路径工具：
/// <list type="bullet">
/// <item><see cref="PathGuard"/>：把「文件/目录是否存在 + 面向用户的中文提示」收进 Core，
/// 视图只负责把消息显示到状态行（原先 ExamInsert/Mss3/Report 三个视图各自 <c>File.Exists</c> + 各写一句话）。</item>
/// <item><see cref="UniqueFileName"/>：另存避撞命名（原先写在 ExamCircleView.UniqueSibling 里）。</item>
/// </list>
/// 都是纯函数/纯文件系统语义，不碰 ACE，所以不在 <c>[AceFact]</c> 之列。
/// </summary>
public sealed class PathGuardTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-guard-" + Guid.NewGuid().ToString("n"));

    public PathGuardTests()
    {
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
            // 临时目录清理失败不影响断言
        }
    }

    [Fact]
    public void MissingFileReason_Blank_ReportsNotChosen()
    {
        Assert.Equal("考评库未选择", PathGuard.MissingFileReason(null, "考评库"));
        Assert.Equal("考评库未选择", PathGuard.MissingFileReason("   ", "考评库"));
    }

    [Fact]
    public void MissingFileReason_Missing_ReportsNotExist_AndReadyReturnsNull()
    {
        string path = Path.Combine(_root, "nope.mdb");

        Assert.Equal("考评库不存在", PathGuard.MissingFileReason(path, "考评库"));

        File.WriteAllBytes(path, []);
        Assert.Null(PathGuard.MissingFileReason(path, "考评库"));
    }

    [Fact]
    public void MissingFolderReason_DistinguishesBlankAndMissing()
    {
        Assert.Equal("待合并数据包目录未选择", PathGuard.MissingFolderReason(null, "待合并数据包目录"));
        Assert.Equal("待合并数据包目录不存在", PathGuard.MissingFolderReason(Path.Combine(_root, "nope"), "待合并数据包目录"));
        Assert.Null(PathGuard.MissingFolderReason(_root, "待合并数据包目录"));
    }

    [Fact]
    public void RequireFile_Blank_ThrowsArgumentExceptionNamingTheLabel()
    {
        var error = Assert.Throws<ArgumentException>(() => PathGuard.RequireFile("", "考评库"));

        Assert.Contains("考评库未选择", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RequireFile_Missing_ThrowsFileNotFound_WithLabelAndPath()
    {
        string path = Path.Combine(_root, "nope.mdb");

        var error = Assert.Throws<FileNotFoundException>(() => PathGuard.RequireFile(path, "考评库"));

        Assert.Contains("考评库不存在", error.Message, StringComparison.Ordinal);
        Assert.Contains(path, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void RequireFile_Existing_ReturnsFullPath()
    {
        string path = Path.Combine(_root, "存在.mdb");
        File.WriteAllBytes(path, []);

        Assert.Equal(Path.GetFullPath(path), PathGuard.RequireFile(path, "考评库"));
    }

    [Fact]
    public void RequireFolder_Missing_ThrowsDirectoryNotFound()
    {
        var error = Assert.Throws<DirectoryNotFoundException>(
            () => PathGuard.RequireFolder(Path.Combine(_root, "nope"), "数据包目录"));

        Assert.Contains("数据包目录不存在", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SamePath_ToleratesCaseTrailingSeparatorAndDots()
    {
        string directory = Path.Combine(_root, "Data");

        Assert.True(PathGuard.SamePath(directory, directory.ToUpperInvariant()));
        Assert.True(PathGuard.SamePath(directory + Path.DirectorySeparatorChar, directory));
        Assert.True(PathGuard.SamePath(Path.Combine(_root, ".", "Data"), directory));
        Assert.False(PathGuard.SamePath(directory, Path.Combine(_root, "Data2")));
        Assert.False(PathGuard.SamePath(null, directory));
        Assert.False(PathGuard.SamePath(" ", directory));
    }

    [Fact]
    public void UniqueFileName_NoCollision_AppendsSuffixAndKeepsExtension()
    {
        string source = Path.Combine(_root, "考评表.png");
        File.WriteAllBytes(source, []);

        Assert.Equal(
            Path.Combine(_root, "考评表_fixed.png"),
            UniqueFileName.For(source, "_fixed"));
    }

    [Fact]
    public void UniqueFileName_Collision_IncrementsFromOne()
    {
        string source = Path.Combine(_root, "考评表.png");
        File.WriteAllBytes(source, []);
        File.WriteAllBytes(Path.Combine(_root, "考评表_fixed.png"), []);

        Assert.Equal(
            Path.Combine(_root, "考评表_fixed_1.png"),
            UniqueFileName.For(source, "_fixed"));

        File.WriteAllBytes(Path.Combine(_root, "考评表_fixed_1.png"), []);

        Assert.Equal(
            Path.Combine(_root, "考评表_fixed_2.png"),
            UniqueFileName.For(source, "_fixed"));
    }

    [Fact]
    public void UniqueFileName_CollisionIsCaseInsensitive()
    {
        string source = Path.Combine(_root, "Sheet.png");
        File.WriteAllBytes(source, []);
        // 同目录已有 SHEET_FIXED.PNG：Windows 上这就是同一个名字，必须判定为冲突。
        File.WriteAllBytes(Path.Combine(_root, "SHEET_FIXED.PNG"), []);

        Assert.Equal(
            Path.Combine(_root, "Sheet_fixed_1.png"),
            UniqueFileName.For(source, "_fixed"));
    }

    [Fact]
    public void UniqueFileName_KeepsMultiPartExtensionAndRotationSuffixShape()
    {
        string source = Path.Combine(_root, "选票.jpg");
        File.WriteAllBytes(source, []);

        Assert.Equal(
            Path.Combine(_root, "选票_r90.jpg"),
            UniqueFileName.For(source, "_r90"));
    }
}
