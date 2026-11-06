using Aspose.Words;
using Aspose.Words.Drawing;
using DocumentTools.Core.Data;
using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// task-40 补迁能力的行为测试（真实 .mdb）：
/// ①「恢复项目列字段结构长度」（dev BodyForm.cs:4417-4435）；
/// ②「数据包单位顺序修改」（dev BodyForm.cs:10526-10657）；
/// ③「仅使用单张图片文件」重复 N 份（dev BodyForm.cs:6409-6436）。
/// </summary>
public sealed class ExamDevParityAccessTests
{
    /// <summary>legacy 5 张表的 Item 列默认长度：Report_Detail/Result_NumInfo 255，Result_DetailInfo/TempResult_DetailInfo 100，其余 50。</summary>
    private static readonly (string Table, int Length)[] ItemColumnLengths =
    [
        ("Page_SelectorInfo", 50),
        ("Report_Detail", 255),
        ("Result_DetailInfo", 100),
        ("Result_NumInfo", 255),
        ("TempResult_DetailInfo", 100),
    ];

    [AceFact]
    public void ResetItemColumnLengths_RevertsEveryItemColumnToLegacyLength()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        var votes = new Mss3VoteService();

        // 先转成 Memo：此时 400 字能写进标题最短的 Page_SelectorInfo。
        Assert.Equal(5, votes.SetItemColumnsToMemo(fixture.DatabaseFile));
        AssertAccepts(fixture, "Page_SelectorInfo", 400);

        // 再恢复长度：同一张表只接受 50 字，400 字必须被数据库拒绝。
        Assert.Equal(5, votes.ResetItemColumnLengths(fixture.DatabaseFile));
        AssertRejects(fixture, "Page_SelectorInfo", 400);
        AssertAccepts(fixture, "Page_SelectorInfo", 50);

        // Report_Detail 是 255，边界两侧各验一次。
        AssertAccepts(fixture, "Report_Detail", 255);
        AssertRejects(fixture, "Report_Detail", 256);
    }

    [AceFact]
    public void ResetItemColumnLengths_ReturnsCountOfExecutedStatements()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // 夹具 5 张表都在，逐条 ALTER 都应成功；重复调用保持幂等。
        Assert.Equal(5, new Mss3VoteService().ResetItemColumnLengths(fixture.DatabaseFile));
        Assert.Equal(5, new Mss3VoteService().ResetItemColumnLengths(fixture.DatabaseFile));
    }

    [Fact]
    public void ResetItemColumnLengths_MissingDatabase_ThrowsFileNotFound()
    {
        string missing = Path.Combine(Path.GetTempPath(), "dt-nope-" + Guid.NewGuid().ToString("n") + ".mdb");

        Assert.Throws<FileNotFoundException>(() => new Mss3VoteService().ResetItemColumnLengths(missing));
    }

    [AceFact]
    public void ReorderUnits_WritesAtOrderFollowingGivenSequence()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        var packages = new ExamPackageService();
        Assert.Equal(["U01", "U02", "U03"], packages.ListUnits(fixture.DirectoryPath).Select(u => u.Code));

        int updated = packages.ReorderUnits(fixture.DirectoryPath, ["U03", "U01", "U02"]);

        Assert.Equal(3, updated);
        Assert.Equal(
            [("U03", 1), ("U01", 2), ("U02", 3)],
            packages.ListUnits(fixture.DirectoryPath).Select(u => (u.Code, u.Order)));
    }

    [AceFact]
    public void ReorderUnits_AllowsPartialList_AndLeavesOtherRowsUntouched()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        var packages = new ExamPackageService();

        // 只给两个编码：仅这两行被重编号，其余行保持原 AtOrder（legacy 也是逐行 UPDATE）。
        Assert.Equal(2, packages.ReorderUnits(fixture.DirectoryPath, ["U02", "U01"]));

        var units = packages.ListUnits(fixture.DirectoryPath);
        Assert.Equal(("U02", 1), (units[0].Code, units[0].Order));
        Assert.Equal(("U01", 2), (units[1].Code, units[1].Order));
        Assert.Equal("U03", units[2].Code);
    }

    [AceFact]
    public void ReorderUnits_UnknownCode_ThrowsAndChangesNothing()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        var packages = new ExamPackageService();

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => packages.ReorderUnits(fixture.DirectoryPath, ["U01", "U99"]));
        Assert.Contains("U99", ex.Message, StringComparison.Ordinal);
        Assert.Equal(["U01", "U02", "U03"], packages.ListUnits(fixture.DirectoryPath).Select(u => u.Code));
    }

    [AceFact]
    public void ReorderUnits_DuplicateCode_Throws()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => new ExamPackageService().ReorderUnits(fixture.DirectoryPath, ["U01", "U01"]));
        Assert.Contains("U01", ex.Message, StringComparison.Ordinal);
    }

    [AceFact]
    public void ReorderUnits_EmptySequence_ReturnsZero()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        Assert.Equal(0, new ExamPackageService().ReorderUnits(fixture.DirectoryPath, ["  ", string.Empty]));
    }

    [Fact]
    public void ReorderUnits_MissingDatabase_ThrowsFileNotFound()
    {
        string root = Path.Combine(Path.GetTempPath(), "dt-nopkg-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        try
        {
            Assert.Throws<FileNotFoundException>(() => new ExamPackageService().ReorderUnits(root, ["U01"]));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GenerateBatch_SingleExternalImage_RepeatsForEveryCopy()
    {
        // dev「仅使用单张图片文件」（BodyForm.cs:6409-6436）：一张图配 N 份，文件名仍按序号递增。
        string root = CreateTempDirectory();
        try
        {
            string template = CreateTemplate(root);
            string image = Path.Combine(root, "单人码.png");
            File.WriteAllBytes(image, CreateSolidPng(8, 8));

            var request = new SingleDocumentRequest
            {
                TemplatePath = template,
                AltTexts = ["人员码"],
                ImageSourcePathOrDirectory = image,
                OutputDirectory = Path.Combine(root, "out"),
                MaxCount = 3,
                NameSuffixMode = SingleDocumentNameSuffixMode.NumberWithSuffix,
                SuffixWord = "测评人",
            };

            var files = new SingleDocumentService().GenerateBatch(request);

            Assert.Equal(3, files.Count);
            Assert.Equal(
                ["模板（001测评人）.docx", "模板（002测评人）.docx", "模板（003测评人）.docx"],
                files.Select(Path.GetFileName));
            Assert.All(files, path => Assert.True(File.Exists(path), path));
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void GenerateBatch_SingleExternalImageWithoutCount_ProducesOneFile()
    {
        string root = CreateTempDirectory();
        try
        {
            string image = Path.Combine(root, "单人码.png");
            File.WriteAllBytes(image, CreateSolidPng(8, 8));

            var request = new SingleDocumentRequest
            {
                TemplatePath = CreateTemplate(root),
                AltTexts = ["人员码"],
                ImageSourcePathOrDirectory = image,
                OutputDirectory = Path.Combine(root, "out"),
                NameSuffixMode = SingleDocumentNameSuffixMode.PersonIndex,
            };

            var files = new SingleDocumentService().GenerateBatch(request);

            Assert.Single(files);
            Assert.Equal("模板（第1人）.docx", Path.GetFileName(files[0]));
        }
        finally
        {
            TryDelete(root);
        }
    }

    /// <summary>往指定表写一条指定长度的 Item，期望数据库接受。</summary>
    private static void AssertAccepts(ExamFixtureSet fixture, string table, int length)
    {
        using var session = fixture.OpenDatabase();
        session.Execute($"DELETE FROM {table}");
        session.Execute($"INSERT INTO {table} (Item) VALUES (?)", ("@item", new string('字', length)));
        string stored = ExamAccessTestKit.Text(ExamAccessTestKit.Rows(session, $"SELECT Item FROM {table}")[0], "Item");
        Assert.Equal(length, stored.Length);
        session.Execute($"DELETE FROM {table}");
    }

    /// <summary>往指定表写超长 Item，期望被数据库拒绝（证明列宽真的回退了）。</summary>
    private static void AssertRejects(ExamFixtureSet fixture, string table, int length)
    {
        using var session = fixture.OpenDatabase();
        session.Execute($"DELETE FROM {table}");
        Assert.ThrowsAny<Exception>(() =>
            session.Execute($"INSERT INTO {table} (Item) VALUES (?)", ("@item", new string('字', length))));
        session.Execute($"DELETE FROM {table}");
    }

    private static string CreateTempDirectory()
    {
        string root = Path.Combine(Path.GetTempPath(), "dt-parity-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static string CreateTemplate(string root)
    {
        string path = Path.Combine(root, "模板.docx");
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

    private static byte[] CreateSolidPng(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using (SKCanvas canvas = new(bitmap))
        {
            canvas.Clear(SKColors.Black);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 90);
        return data.ToArray();
    }

    private static void TryDelete(string root)
    {
        try
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
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
