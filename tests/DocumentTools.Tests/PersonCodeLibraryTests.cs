using System.IO.Compression;
using Aspose.Words;
using Aspose.Words.Drawing;
using DocumentTools.Core.Services.Exam;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// 内置测评人码图（legacy <c>Util.GetPersonCodeZipFileStream</c> / <c>PersonCode.zip</c>）的契约测试。
/// 资源通过 csproj 的 Content/Link 落到 <c>AppContext.BaseDirectory\assets\embedded\PersonCode.zip</c>，
/// 与 App 发布路径一致，故这里用默认解析而不另造夹具。
/// </summary>
public sealed class PersonCodeLibraryTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-personcode-" + Guid.NewGuid().ToString("n"));

    public PersonCodeLibraryTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void Archive_ContainsOneThousandEntries_And999Images()
    {
        Assert.True(PersonCodeLibrary.IsAvailable(), PersonCodeLibrary.ResolveArchivePath());

        // legacy 包 = 1 个目录项 PersonCode/ + 999 张 bmp。
        Assert.Equal(1000, PersonCodeLibrary.CountEntries());
        Assert.Equal(999, PersonCodeLibrary.CountImages());
        Assert.Equal(1, PersonCodeLibrary.ListIndexes().First());
        Assert.Equal(999, PersonCodeLibrary.ListIndexes().Last());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(999)]
    public void ReadImage_FirstAndLast_DecodeAsBitmap(int index)
    {
        byte[] bytes = PersonCodeLibrary.ReadImage(index);

        Assert.True(bytes.Length > 1000, $"条目 {index} 只有 {bytes.Length} 字节");
        using SKBitmap bitmap = SKBitmap.Decode(bytes);
        Assert.NotNull(bitmap);
        Assert.True(bitmap.Width > 0 && bitmap.Height > 0);
    }

    [Fact]
    public void ReadImage_OutOfRange_ThrowsChineseRangeMessage()
    {
        ArgumentOutOfRangeException low = Assert.Throws<ArgumentOutOfRangeException>(() => PersonCodeLibrary.ReadImage(0));
        ArgumentOutOfRangeException high = Assert.Throws<ArgumentOutOfRangeException>(() => PersonCodeLibrary.ReadImage(1000));

        Assert.Contains("内置测评人码图只有 001–999", low.Message, StringComparison.Ordinal);
        Assert.Contains("内置测评人码图只有 001–999", high.Message, StringComparison.Ordinal);
        Assert.Contains("收到 1000", high.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ReadImage_ToleratesPrefixAndCaseDifferences()
    {
        // 条目名只靠 3 位序号识别：前缀、扩展名大小写都不影响取图。
        string archive = CreateArchive(("OTHER/001测评人.BMP", 0x10), ("PersonCode/002测评人.bmp", 0x20));

        Assert.Equal(new byte[] { 0x10 }, PersonCodeLibrary.ReadImage(1, archive));
        Assert.Equal(new byte[] { 0x20 }, PersonCodeLibrary.ReadImage(2, archive));
        Assert.Equal(2, PersonCodeLibrary.CountImages(archive));
    }

    [Fact]
    public void ReadImage_MissingArchive_ReportsPathAndPublishHint()
    {
        string missing = Path.Combine(_root, "nope.zip");

        FileNotFoundException ex = Assert.Throws<FileNotFoundException>(() => PersonCodeLibrary.ReadImage(1, missing));
        Assert.Contains("内置测评人码图包", ex.Message, StringComparison.Ordinal);
        Assert.Contains("PersonCode.zip", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ResolveArchivePath_FallsBackToRepoTreeWhenOutputMissesTheCopy()
    {
        // 复现「测试工程输出目录没拿到 zip」：临时根下只有源码树布局 <root>\DocumentTools\Resources\PersonCode.zip。
        string root = Path.Combine(_root, "bin", "Debug", "net8.0-windows");
        Directory.CreateDirectory(root);
        string resources = Path.Combine(_root, "DocumentTools", "Resources");
        Directory.CreateDirectory(resources);
        string expected = Path.Combine(resources, "PersonCode.zip");
        File.Copy(CreateArchive(("PersonCode/001测评人.bmp", 0x30)), expected);

        string resolved = PersonCodeLibrary.ResolveArchivePath(root);

        Assert.Equal(expected, resolved, ignoreCase: true);
        Assert.True(PersonCodeLibrary.IsAvailable(resolved));
        Assert.Equal(1, PersonCodeLibrary.CountImages(resolved));
    }

    [Fact]
    public void GenerateBatch_BuiltInCodes_KeepNamingAndCountRules()
    {
        string template = CreateTemplateWithPersonCodeShape();
        string externalDirectory = CreateExternalImages(3);

        var builtIn = new SingleDocumentRequest
        {
            TemplatePath = template,
            AltTexts = ["人员码"],
            OutputDirectory = Path.Combine(_root, "builtin"),
            MaxCount = 3,
            NameSuffixMode = SingleDocumentNameSuffixMode.NumberWithSuffix,
            SuffixWord = "测评人",
            UseBuiltInPersonCode = true,
            PersonCodeStartIndex = 7,
        };
        var fromCodes = new SingleDocumentService().GenerateBatch(builtIn);

        var manual = new SingleDocumentRequest
        {
            TemplatePath = template,
            AltTexts = ["人员码"],
            ImageSourcePathOrDirectory = externalDirectory,
            OutputDirectory = Path.Combine(_root, "manual"),
            MaxCount = 3,
            NameSuffixMode = SingleDocumentNameSuffixMode.NumberWithSuffix,
            SuffixWord = "测评人",
            UseBuiltInPersonCode = false,
        };
        var fromImage = new SingleDocumentService().GenerateBatch(manual);

        // 命名规则由后缀模式决定，与图片来源无关：两批文件名逐字相同、份数相同。
        Assert.Equal(3, fromCodes.Count);
        Assert.Equal(fromImage.Select(Path.GetFileName), fromCodes.Select(Path.GetFileName));
        Assert.Equal(
            ["模板（001测评人）.docx", "模板（002测评人）.docx", "模板（003测评人）.docx"],
            fromCodes.Select(Path.GetFileName));
        Assert.All(fromCodes, path => Assert.True(File.Exists(path), path));
    }

    [Fact]
    public void GenerateBatch_BuiltInStartIndexBeyondRange_ThrowsChineseMessage()
    {
        var request = new SingleDocumentRequest
        {
            TemplatePath = CreateTemplateWithPersonCodeShape(),
            AltTexts = ["人员码"],
            OutputDirectory = Path.Combine(_root, "over"),
            MaxCount = 2,
            UseBuiltInPersonCode = true,
            PersonCodeStartIndex = 1000,
        };

        ArgumentOutOfRangeException ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new SingleDocumentService().GenerateBatch(request));
        Assert.Contains("内置测评人码图只有 001–999", ex.Message, StringComparison.Ordinal);
    }

    /// <summary>造一个把 <paramref name="entries"/> 塞进 zip 的最小包，用于验证「前缀/大小写容忍」。</summary>
    private string CreateArchive(params (string Name, byte Value)[] entries)
    {
        string path = Path.Combine(_root, "custom-" + Guid.NewGuid().ToString("n") + ".zip");
        using FileStream stream = File.Create(path);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        foreach ((string name, byte value) in entries)
        {
            ZipArchiveEntry entry = archive.CreateEntry(name);
            using Stream entryStream = entry.Open();
            entryStream.WriteByte(value);
        }

        return path;
    }

    private string CreateTemplateWithPersonCodeShape()
    {
        string path = Path.Combine(_root, "模板.docx");
        var document = new Document();
        DocumentBuilder builder = new(document);
        builder.Writeln("姓名：____________");

        using var bitmap = new SKBitmap(10, 10);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.White);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 90);
        Shape shape = builder.InsertImage(encoded.ToArray());
        shape.AlternativeText = "人员码";

        document.Save(path);
        return path;
    }

    /// <summary>造一个含 <paramref name="count"/> 张 PNG 的外部素材目录（legacy「使用外部单人码目录」分支）。</summary>
    private string CreateExternalImages(int count)
    {
        string directory = Path.Combine(_root, "external");
        Directory.CreateDirectory(directory);
        for (int i = 1; i <= count; i++)
        {
            File.WriteAllBytes(Path.Combine(directory, $"code{i:D3}.png"), CreateSolidPng(8, 8));
        }

        return directory;
    }

    private static byte[] CreateSolidPng(int width, int height)    {
        using var bitmap = new SKBitmap(width, height);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.Black);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
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
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
