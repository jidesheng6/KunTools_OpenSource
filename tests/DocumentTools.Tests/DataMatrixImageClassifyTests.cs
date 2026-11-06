using DocumentTools.Core.Services.Code;

namespace DocumentTools.Tests;

/// <summary>
/// 图片目录批量分类（legacy <c>Util.RecognitionPictureDataMatrix</c> 的 DataMatrix 分支）。
/// 解码用替身注入：Aspose.BarCode 评估版会把 <c>T001</c> 读成 <c>T00*Recognized by Aspose...</c>，
/// 真解码结果不能用于断言目录名；真解码路径由最后一条冒烟用例覆盖。
/// </summary>
public sealed class DataMatrixImageClassifyTests
{
    [Fact]
    public void Classify_FilesImagesIntoPageFolders_AndKeepsSourcesByDefault()
    {
        using var workspace = new TempWorkspace();
        workspace.AddImage("a.png");
        workspace.AddImage("b.jpg");
        workspace.AddImage("c.bmp");

        var service = new DataMatrixImageClassifyService(
            new StubPageCodeReader(new() { ["a.png"] = "001", ["b.jpg"] = "001", ["c.bmp"] = null }));

        DataMatrixImageClassifyOutcome outcome = service.Classify(workspace.Request());

        Assert.Equal(3, outcome.TotalImages);
        Assert.Equal(2, outcome.ClassifiedImages);
        Assert.Equal(1, outcome.UnrecognizedImages);
        Assert.Equal(0, outcome.FailedImages);
        Assert.False(outcome.Canceled);

        Assert.True(File.Exists(Path.Combine(workspace.Target, "页码-001", "a.png")));
        Assert.True(File.Exists(Path.Combine(workspace.Target, "页码-001", "b.jpg")));
        Assert.True(File.Exists(
            Path.Combine(workspace.Target, DataMatrixImageClassifyNaming.UnrecognizedFolder, "c.bmp")));

        // 默认「保留源目录内文件」= 复制：源文件一张都不能少。
        Assert.Equal(3, Directory.GetFiles(workspace.Source, "*", SearchOption.AllDirectories).Length);

        var pageFolder = Assert.Single(
            outcome.Folders,
            folder => folder.FolderName == DataMatrixImageClassifyNaming.PageFolderPrefix + "001");
        Assert.Equal(2, pageFolder.ImageCount);
    }

    [Fact]
    public void Classify_MovesFilesWhenKeepSourceFilesIsFalse()
    {
        using var workspace = new TempWorkspace();
        workspace.AddImage("a.png");
        workspace.AddImage("b.png");

        var service = new DataMatrixImageClassifyService(
            new StubPageCodeReader(new() { ["a.png"] = "007", ["b.png"] = "007" }));

        DataMatrixImageClassifyOutcome outcome = service.Classify(
            workspace.Request() with { KeepSourceFiles = false });

        Assert.Equal(2, outcome.ClassifiedImages);
        Assert.Empty(Directory.GetFiles(workspace.Source));
        Assert.True(File.Exists(Path.Combine(workspace.Target, "页码-007", "a.png")));
        Assert.True(File.Exists(Path.Combine(workspace.Target, "页码-007", "b.png")));
    }

    [Fact]
    public void Classify_PreservesRelativeSubdirectoriesUnderTarget()
    {
        using var workspace = new TempWorkspace();
        workspace.AddImage("a.png");
        workspace.AddImage(Path.Combine("考点A", "b.png"));

        var service = new DataMatrixImageClassifyService(
            new StubPageCodeReader(new() { ["a.png"] = "001", ["b.png"] = "001" }));

        DataMatrixImageClassifyOutcome outcome = service.Classify(
            workspace.Request() with { IncludeSubdirectories = true });

        Assert.Equal(2, outcome.ClassifiedImages);

        // legacy 用 Path.Combine(target, "\考点A", …) 拼根路径，会写到盘符根；这里必须落在目标目录下。
        Assert.True(File.Exists(Path.Combine(workspace.Target, "页码-001", "a.png")));
        Assert.True(File.Exists(Path.Combine(workspace.Target, "考点A", "页码-001", "b.png")));
    }

    [Fact]
    public void Classify_IgnoresSubdirectoriesWhenNotRequested()
    {
        using var workspace = new TempWorkspace();
        workspace.AddImage("a.png");
        string nested = workspace.AddImage(Path.Combine("考点A", "b.png"));

        var service = new DataMatrixImageClassifyService(
            new StubPageCodeReader(new() { ["a.png"] = "001", ["b.png"] = "001" }));

        DataMatrixImageClassifyOutcome outcome = service.Classify(workspace.Request());

        Assert.Equal(1, outcome.TotalImages);
        Assert.True(File.Exists(nested));
        Assert.False(Directory.Exists(Path.Combine(workspace.Target, "考点A")));
    }

    [Fact]
    public void Classify_IgnoresUnsupportedExtensions()
    {
        using var workspace = new TempWorkspace();
        workspace.AddImage("a.png");
        workspace.AddImage("note.txt");
        workspace.AddImage("anim.gif");

        var service = new DataMatrixImageClassifyService(
            new StubPageCodeReader(new() { ["a.png"] = "001" }));

        DataMatrixImageClassifyOutcome outcome = service.Classify(workspace.Request());

        Assert.Equal(1, outcome.TotalImages);
        Assert.True(File.Exists(Path.Combine(workspace.Source, "note.txt")));
        Assert.True(File.Exists(Path.Combine(workspace.Source, "anim.gif")));
    }

    [Fact]
    public void Classify_EmptyDirectoryReturnsZeroCounts()
    {
        using var workspace = new TempWorkspace();
        var service = new DataMatrixImageClassifyService(new StubPageCodeReader(new()));

        DataMatrixImageClassifyOutcome outcome = service.Classify(workspace.Request());

        Assert.Equal(0, outcome.TotalImages);
        Assert.Empty(outcome.Folders);
        Assert.True(Directory.Exists(workspace.Target));
    }

    [Fact]
    public void Classify_RecordsPerFileFailuresWithoutAbortingBatch()
    {
        using var workspace = new TempWorkspace();
        workspace.AddImage("a.png");
        workspace.AddImage("broken.png");
        workspace.AddImage("c.png");

        var service = new DataMatrixImageClassifyService(
            new StubPageCodeReader(
                new() { ["a.png"] = "002", ["c.png"] = "002" },
                throwFor: ["broken.png"]));

        DataMatrixImageClassifyOutcome outcome = service.Classify(workspace.Request());

        Assert.Equal(3, outcome.TotalImages);
        Assert.Equal(2, outcome.ClassifiedImages);
        Assert.Equal(1, outcome.FailedImages);
        Assert.Single(outcome.Failures);
        Assert.Contains("broken.png", outcome.Failures[0], StringComparison.Ordinal);
        Assert.True(File.Exists(Path.Combine(workspace.Target, "页码-002", "a.png")));
    }

    [Fact]
    public void Classify_ThrowsActionableMessageWhenSourceIsMissing()
    {
        using var workspace = new TempWorkspace();
        var service = new DataMatrixImageClassifyService(new StubPageCodeReader(new()));

        var request = workspace.Request() with
        {
            SourceDirectory = Path.Combine(workspace.Root, "not-there")
        };

        var error = Assert.Throws<DirectoryNotFoundException>(() => service.Classify(request));
        Assert.Contains("源目录不存在", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AsposePageCodeReader_DecodesGeneratedDataMatrix()
    {
        // 真解码冒烟：评估版截断末字符并加水印，故只断言「读到了 T 开头的码」。
        byte[] png = new BarcodeService().CreatePng(
            "T001",
            new BarcodeRenderOptions { Symbology = "DataMatrix", ModuleSize = 6 });

        string file = Path.Combine(Path.GetTempPath(), "dt-dm-" + Guid.NewGuid().ToString("N") + ".png");
        File.WriteAllBytes(file, png);

        try
        {
            string? code = new AsposeImagePageCodeReader().ReadPageCode(file);

            Assert.NotNull(code);
            Assert.StartsWith("0", code, StringComparison.Ordinal);
        }
        finally
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                // 临时文件残留无害。
            }
        }
    }

    private sealed class StubPageCodeReader : IImagePageCodeReader
    {
        private readonly Dictionary<string, string?> _codes;
        private readonly HashSet<string> _throwFor;

        public StubPageCodeReader(Dictionary<string, string?> codes, params string[] throwFor)
        {
            _codes = codes;
            _throwFor = throwFor.ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public string? ReadPageCode(string imageFile)
        {
            string name = Path.GetFileName(imageFile);
            if (_throwFor.Contains(name))
            {
                throw new InvalidOperationException("解码失败（测试替身）");
            }

            return _codes.TryGetValue(name, out string? code) ? code : null;
        }
    }

    private sealed class TempWorkspace : IDisposable
    {
        public TempWorkspace()
        {
            Directory.CreateDirectory(Source);
        }

        public string Root { get; } = Path.Combine(
            Path.GetTempPath(),
            "dt-classify-" + Guid.NewGuid().ToString("N"));

        public string Source => Path.Combine(Root, "src");

        public string Target => Path.Combine(Root, "out");

        public DataMatrixImageClassifyRequest Request()
            => new() { SourceDirectory = Source, TargetDirectory = Target };

        public string AddImage(string relativePath, string content = "stub-image")
        {
            string path = Path.Combine(Source, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
            return path;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Root, recursive: true);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // 清理失败不影响断言结论。
            }
        }
    }
}
