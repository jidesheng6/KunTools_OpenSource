using Aspose.Words;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Office;
using WordsDocument = Aspose.Words.Document;
using WordsRun = Aspose.Words.Run;

namespace DocumentTools.Tests;

/// <summary>
/// word.replace 的「更改替换的字体和大小」（legacy <c>Util.cs:5373 UseNewStyle</c> + <c>Util.cs:5412-5416 ApplyFont</c>）。
/// 断言的是产物里 run 的真实字体名/字号/加粗，而不是「参数被传进去了」。
/// </summary>
public class WordReplaceStyleTests
{
    private const string SourceFont = "Arial";
    private const double SourceSize = 10;
    private const string TargetFont = "SimSun";
    private const double TargetSize = 18;

    [Fact]
    public void UseNewStyle_AppliesFontNameSizeAndBold()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-replace-style-").FullName;
        try
        {
            string source = CreateSource(directory, "旧");
            string output = Path.Combine(directory, "out.docx");

            new DocumentReplaceService().ReplaceWord(source, output, new DocumentReplaceOptions
            {
                Pairs = [("旧", "新")],
                UseNewStyle = true,
                FontName = TargetFont,
                FontSize = TargetSize,
                Bold = true
            });

            WordsRun run = FindRun(output, "新");
            Assert.Equal(TargetFont, run.Font.Name);
            Assert.Equal(TargetSize, run.Font.Size);
            Assert.True(run.Font.Bold);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void UseNewStyleFalse_KeepsTheFormatOfTheReplacedText()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-replace-style-").FullName;
        try
        {
            string source = CreateSource(directory, "旧");
            string output = Path.Combine(directory, "out.docx");

            new DocumentReplaceService().ReplaceWord(source, output, new DocumentReplaceOptions
            {
                Pairs = [("旧", "新")],
                UseNewStyle = false,
                // 开关关着时这些值必须被忽略——这正是 legacy 的默认语义。
                FontName = TargetFont,
                FontSize = TargetSize,
                Bold = true
            });

            WordsRun run = FindRun(output, "新");
            Assert.Equal(SourceFont, run.Font.Name);
            Assert.Equal(SourceSize, run.Font.Size);
            Assert.False(run.Font.Bold);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void BoldKeep_LeavesTheOriginalWeightAlone()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-replace-style-").FullName;
        try
        {
            string source = CreateSource(directory, "旧");
            string output = Path.Combine(directory, "out.docx");

            new DocumentReplaceService().ReplaceWord(source, output, new DocumentReplaceOptions
            {
                Pairs = [("旧", "新")],
                UseNewStyle = true,
                FontName = TargetFont,
                FontSize = TargetSize,
                Bold = null
            });

            WordsRun run = FindRun(output, "新");
            Assert.Equal(TargetFont, run.Font.Name);
            Assert.Equal(TargetSize, run.Font.Size);
            Assert.False(run.Font.Bold);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void EmptyFontAndZeroSize_DoNotTouchFormatting()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-replace-style-").FullName;
        try
        {
            string source = CreateSource(directory, "旧");
            string output = Path.Combine(directory, "out.docx");

            new DocumentReplaceService().ReplaceWord(source, output, new DocumentReplaceOptions
            {
                Pairs = [("旧", "新")],
                UseNewStyle = true,
                FontName = "   ",
                FontSize = 0,
                Bold = null
            });

            WordsRun run = FindRun(output, "新");
            Assert.Equal(SourceFont, run.Font.Name);
            Assert.Equal(SourceSize, run.Font.Size);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Operation_MapsTheNewStyleKeysIntoOptions()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-replace-style-").FullName;
        try
        {
            var recorder = new RecordingReplaceService();
            DocumentOperationResult result = await RunOperationAsync("word.replace", recorder, directory, new()
            {
                ["pairCount"] = "1",
                ["pair0.find"] = "旧",
                ["pair0.replace"] = "新",
                ["replaceUseNewStyle"] = "true",
                ["replaceFontName"] = " SimSun ",
                ["replaceFontSize"] = "18磅",
                ["replaceBold"] = "bold"
            });

            Assert.True(result.Success, result.Message);
            DocumentReplaceOptions options = Assert.IsType<DocumentReplaceOptions>(recorder.LastOptions);
            Assert.True(options.UseNewStyle);
            Assert.Equal(TargetFont, options.FontName);      // 前后空格被去掉
            Assert.Equal(TargetSize, options.FontSize);      // 「18磅」取前缀数字
            Assert.True(options.Bold);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Operation_BoldNormalMapsToFalse()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-replace-style-").FullName;
        try
        {
            var recorder = new RecordingReplaceService();
            DocumentOperationResult result = await RunOperationAsync("word.replace", recorder, directory, new()
            {
                ["pairCount"] = "1",
                ["pair0.find"] = "旧",
                ["pair0.replace"] = "新",
                ["replaceUseNewStyle"] = "true",
                ["replaceBold"] = "normal"
            });

            Assert.True(result.Success, result.Message);
            DocumentReplaceOptions options = Assert.IsType<DocumentReplaceOptions>(recorder.LastOptions);
            Assert.False(options.Bold);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Operation_DefaultsToPlainTextReplace()
    {
        string directory = Directory.CreateTempSubdirectory("documenttools-replace-style-").FullName;
        try
        {
            var recorder = new RecordingReplaceService();
            DocumentOperationResult result = await RunOperationAsync("word.replace", recorder, directory, new()
            {
                ["pairCount"] = "1",
                ["pair0.find"] = "旧",
                ["pair0.replace"] = "新"
            });

            Assert.True(result.Success, result.Message);
            DocumentReplaceOptions options = Assert.IsType<DocumentReplaceOptions>(recorder.LastOptions);
            Assert.False(options.UseNewStyle);
            Assert.Equal(string.Empty, options.FontName);
            Assert.Equal(0d, options.FontSize);
            Assert.Null(options.Bold);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void Schema_ExposesTheReplaceStyleKeys()
    {
        OptionSection section = Assert.Single(
            FeatureOptionSchemas.GetSections("word.replace"), s => s.Title == "替换后字体样式");

        OptionField useNewStyle = Assert.Single(section.Fields, f => f.Key == "replaceUseNewStyle");
        Assert.Equal(OptionFieldKind.Check, useNewStyle.Kind);
        Assert.Equal("false", useNewStyle.DefaultValue);

        OptionField fontName = Assert.Single(section.Fields, f => f.Key == "replaceFontName");
        Assert.Equal(OptionFieldKind.Text, fontName.Kind);
        Assert.Equal("SimSun", fontName.DefaultValue);

        OptionField fontSize = Assert.Single(section.Fields, f => f.Key == "replaceFontSize");
        Assert.Equal(OptionFieldKind.Text, fontSize.Kind);
        Assert.Equal("12", fontSize.DefaultValue);

        OptionField bold = Assert.Single(section.Fields, f => f.Key == "replaceBold");
        Assert.Equal(OptionFieldKind.Choice, bold.Kind);
        Assert.Equal("keep", bold.DefaultValue);
        Assert.Equal(["keep", "bold", "normal"], bold.Choices!.Select(c => c.Value));
        Assert.Equal(["不改", "加粗", "不加粗"], bold.Choices!.Select(c => c.Label));
    }

    private static string CreateSource(string directory, string text, bool bold = false)
    {
        string path = Path.Combine(directory, "source.docx");
        var document = new WordsDocument();
        var builder = new Aspose.Words.DocumentBuilder(document);
        builder.Font.Name = SourceFont;
        builder.Font.Size = SourceSize;
        builder.Font.Bold = bold;
        builder.Write(text);
        document.Save(path, Aspose.Words.SaveFormat.Docx);
        return path;
    }

    private static WordsRun FindRun(string documentPath, string text)
    {
        var document = new WordsDocument(documentPath);
        foreach (Aspose.Words.Node node in document.GetChildNodes(Aspose.Words.NodeType.Run, true))
        {
            if (node is WordsRun run && run.Text.Contains(text, StringComparison.Ordinal))
            {
                return run;
            }
        }

        throw new InvalidOperationException($"产物里找不到包含「{text}」的 run。");
    }

    private static Task<DocumentOperationResult> RunOperationAsync(
        string featureId,
        IDocumentReplaceService service,
        string outputDirectory,
        Dictionary<string, string> options)
    {
        string input = Path.Combine(outputDirectory, "input.docx");
        File.WriteAllBytes(input, []);
        return new ReplaceDocumentOperation(featureId, service).ExecuteAsync(new DocumentOperationRequest
        {
            InputFiles = [input],
            OutputDirectory = outputDirectory,
            Options = options
        });
    }

    private sealed class RecordingReplaceService : IDocumentReplaceService
    {
        public object? LastOptions { get; private set; }

        public void ReplaceWord(string sourceFile, string savedFile, DocumentReplaceOptions options)
            => LastOptions = options;

        public void ReplaceExcel(string sourceFile, string savedFile, DocumentReplaceOptions options)
            => throw new NotSupportedException();
    }
}
