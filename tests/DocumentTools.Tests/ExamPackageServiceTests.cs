using DocumentTools.Core.Services.Exam;

namespace DocumentTools.Tests;

/// <summary>
/// 数据包合并的前置校验：这些分支在打开 Access 库之前就会返回，
/// 因此在没有 ACE 的机器上也能稳定验证，不需要外部数据库夹具。
/// </summary>
public sealed class ExamPackageServiceTests : IDisposable
{
    private readonly ExamPackageService _sut = new();
    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-pkg-" + Guid.NewGuid().ToString("n"));

    public ExamPackageServiceTests()
    {
        Directory.CreateDirectory(Path.Combine(_root, "src"));
        Directory.CreateDirectory(Path.Combine(_root, "dst"));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
            // 临时目录清理失败不影响断言结果
        }
    }

    [Fact]
    public void Merge_BlankSourceThrows()
    {
        Assert.Throws<ArgumentException>(() => _sut.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = " ",
            TargetDirectory = Path.Combine(_root, "dst")
        }));
    }

    [Fact]
    public void Merge_MissingBaseDatabaseThrows()
    {
        File.WriteAllBytes(Path.Combine(_root, "dst", "Exam_Base.mdb"), []);

        var ex = Assert.Throws<FileNotFoundException>(() => _sut.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = Path.Combine(_root, "src"),
            TargetDirectory = Path.Combine(_root, "dst")
        }));

        Assert.Contains("Exam_Base.mdb", ex.Message);
    }

    [Fact]
    public void Merge_SameDirectoryThrows()
    {
        File.WriteAllBytes(Path.Combine(_root, "dst", "Exam_Base.mdb"), []);

        Assert.Throws<ArgumentException>(() => _sut.Merge(new ExamPackageMergeRequest
        {
            SourceDirectory = Path.Combine(_root, "dst"),
            TargetDirectory = Path.Combine(_root, "dst")
        }));
    }

    [Fact]
    public void Inspect_MissingDatabaseReportsNotInitialized()
    {
        ExamPackageStatus status = _sut.Inspect(Path.Combine(_root, "src"));

        Assert.False(status.DatabaseExists);
        Assert.Contains("Exam_Base.mdb", status.Summary);
    }
}
