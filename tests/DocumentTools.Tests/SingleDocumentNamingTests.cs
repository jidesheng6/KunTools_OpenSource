using Aspose.Words;
using Aspose.Words.Drawing;
using DocumentTools.Core.Services.Exam;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// exam.single「生成名称后缀模式」验收。
/// legacy 依据：BodyForm.cs:6286-6311（FileIndex 先自增再拼名、全角括号、输出固定 .docx、
/// 模板文件名去扩展名 + 后缀）、Designer:11499/11507/11521/11535 三档单选、
/// Designer:11546/11557「生成名称后缀」文本框（默认「测评人」）。
/// </summary>
public sealed class SingleDocumentNamingTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-single-" + Guid.NewGuid().ToString("n"));
    private readonly SingleDocumentService _sut = new();

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

    [Fact]
    public void BuildSuffix_NumberMode_UsesThreeDigitIndexPlusSuffixWord()
    {
        // legacy：$"（{FileIndex:D3}{FileEndName}）"，如「（001测评人）」。
        Assert.Equal(
            "（001测评人）",
            SingleDocumentNaming.BuildSuffix(SingleDocumentNameSuffixMode.NumberWithSuffix, 1, "测评人"));
        Assert.Equal(
            "（012测评人）",
            SingleDocumentNaming.BuildSuffix(SingleDocumentNameSuffixMode.NumberWithSuffix, 12, "测评人"));
        Assert.Equal(
            "（003组长）",
            SingleDocumentNaming.BuildSuffix(SingleDocumentNameSuffixMode.NumberWithSuffix, 3, " 组长 "));
    }

    [Fact]
    public void BuildSuffix_PersonAndPageModes_MatchLegacyTextBoxes()
    {
        Assert.Equal(
            "（第1人）",
            SingleDocumentNaming.BuildSuffix(SingleDocumentNameSuffixMode.PersonIndex, 1, "测评人"));
        Assert.Equal(
            "（第3页）",
            SingleDocumentNaming.BuildSuffix(SingleDocumentNameSuffixMode.PageIndex, 3, "测评人"));
    }

    [Fact]
    public void BuildSuffix_NegativeIndex_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => SingleDocumentNaming.BuildSuffix(SingleDocumentNameSuffixMode.PersonIndex, -1, "测评人"));
    }

    [Fact]
    public void BuildFileName_UsesTemplateStemAndDocxExtension()
    {
        Assert.Equal(
            "单人表模板（001测评人）.docx",
            SingleDocumentNaming.BuildFileName(Path.Combine("C:", "素材", "单人表模板.docx"), "（001测评人）"));
    }

    [Fact]
    public void FourModeCombinations_ProduceDistinctFileNames()
    {
        // 三档 radio + 可编辑后缀词（第四种组合）必须互不撞名。
        (SingleDocumentNameSuffixMode Mode, string Word)[] combos =
        [
            (SingleDocumentNameSuffixMode.NumberWithSuffix, "测评人"),
            (SingleDocumentNameSuffixMode.NumberWithSuffix, "选举人"),
            (SingleDocumentNameSuffixMode.PersonIndex, "测评人"),
            (SingleDocumentNameSuffixMode.PageIndex, "测评人")
        ];

        string template = Path.Combine("C:", "素材", "单人表模板.docx");
        string[] names = combos
            .Select(c => SingleDocumentNaming.BuildFileName(template, SingleDocumentNaming.BuildSuffix(c.Mode, 3, c.Word)))
            .ToArray();

        Assert.Equal(4, names.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("单人表模板（003测评人）.docx", names);
        Assert.Contains("单人表模板（003选举人）.docx", names);
        Assert.Contains("单人表模板（第3人）.docx", names);
        Assert.Contains("单人表模板（第3页）.docx", names);
    }

    [Fact]
    public void GenerateBatch_NamingModes_WriteDistinctFilesPerImage()
    {
        string template = CreateTemplateWithPersonCodeShapes(out int shapeCount);
        Assert.Equal(1, shapeCount);

        string source = Path.Combine(_root, "images");
        Directory.CreateDirectory(source);
        CreateSolidPng(Path.Combine(source, "a.png"));
        CreateSolidPng(Path.Combine(source, "b.png"));

        (SingleDocumentNameSuffixMode Mode, string Word)[] combos =
        [
            (SingleDocumentNameSuffixMode.NumberWithSuffix, "测评人"),
            (SingleDocumentNameSuffixMode.NumberWithSuffix, "选举人"),
            (SingleDocumentNameSuffixMode.PersonIndex, "测评人"),
            (SingleDocumentNameSuffixMode.PageIndex, "测评人")
        ];

        var seen = new List<string>();
        foreach ((SingleDocumentNameSuffixMode mode, string word) in combos)
        {
            string outputDir = Path.Combine(_root, "out-" + seen.Count);
            IReadOnlyList<string> outputs = _sut.GenerateBatch(new SingleDocumentRequest
            {
                TemplatePath = template,
                AltTexts = ["人员码"],
                ImageSourcePathOrDirectory = source,
                OutputDirectory = outputDir,
                NameSuffixMode = mode,
                SuffixWord = word
            });

            Assert.Equal(2, outputs.Count);
            Assert.All(outputs, file => Assert.True(File.Exists(file), file));
            seen.AddRange(outputs.Select(Path.GetFileName)!);
        }

        // 同一份输入、四种模式 → 8 个文件名互不相同。
        Assert.Equal(8, seen.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("模板（001测评人）.docx", seen);
        Assert.Contains("模板（002测评人）.docx", seen);
        Assert.Contains("模板（001选举人）.docx", seen);
        Assert.Contains("模板（第1人）.docx", seen);
        Assert.Contains("模板（第2人）.docx", seen);
        Assert.Contains("模板（第1页）.docx", seen);
    }

    private string CreateTemplateWithPersonCodeShapes(out int shapeCount)
    {
        Directory.CreateDirectory(_root);
        string image = Path.Combine(_root, "placeholder.png");
        CreateSolidPng(image);

        string template = Path.Combine(_root, "模板.docx");
        // Aspose.Words.Document 不实现 IDisposable，不用 using。
        var document = new Document();
        var builder = new DocumentBuilder(document);
        builder.Writeln("单人表");
        Shape shape = builder.InsertImage(image, 40, 40);
        shape.AlternativeText = "人员码";
        document.Save(template);

        var reloaded = new Document(template);
        shapeCount = reloaded.GetChildNodes(NodeType.Shape, true)
            .OfType<Shape>()
            .Count(s => s.AlternativeText == "人员码");
        return template;
    }

    private static void CreateSolidPng(string filePath)
    {
        using var bitmap = new SKBitmap(24, 24);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = false };
            canvas.DrawRect(SKRect.Create(4, 4, 16, 16), paint);
        }

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using FileStream stream = File.Create(filePath);
        data.SaveTo(stream);
    }
}
