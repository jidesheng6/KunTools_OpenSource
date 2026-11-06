using DocumentTools.Core.Common;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Office;

namespace DocumentTools.Tests;

/// <summary>
/// 转换选项连通性：UI key → OfficeConvertOperation → *Options DTO → 服务入口。
/// 用记录型假服务替代真实 Aspose 转换（本机无许可证/夹具），只验证参数确实被消费。
/// </summary>
public class ConvertOptionWiringTests
{
    [Fact]
    public async Task PdfToPpt_WiresAllThreeLegacyFlags()
    {
        string dir = Directory.CreateTempSubdirectory("documenttools-wiring-").FullName;
        try
        {
            var recorder = new RecordingConvertService();
            DocumentOperationResult result = await RunAsync("pdf.ppt", recorder, dir, new()
            {
                ["extraOcr"] = "true",
                ["splitShape"] = "true",
                ["renderImage"] = "false"
            });

            Assert.True(result.Success, result.Message);
            var options = Assert.IsType<PdfToPptOptions>(recorder.LastOptions);
            Assert.True(options.ExtraOcr);
            Assert.True(options.SplitShapes);
            Assert.False(options.RenderAsImages);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task WordToEpub_WiresCoverImageAuthorAndDpi()
    {
        string dir = Directory.CreateTempSubdirectory("documenttools-wiring-").FullName;
        try
        {
            string cover = Path.Combine(dir, "cover.png");
            var recorder = new RecordingConvertService();
            DocumentOperationResult result = await RunAsync("word.epub", recorder, dir, new()
            {
                ["author"] = "李雷",
                ["coverImage"] = cover,
                ["coverDpi"] = "150"
            });

            Assert.True(result.Success, result.Message);
            var options = Assert.IsType<WordToEpubOptions>(recorder.LastOptions);
            Assert.Equal("李雷", options.Author);
            Assert.Equal(cover, options.CoverImagePath);
            Assert.Equal(150, options.CoverDpi);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public async Task WordToEpub_WithoutCoverImage_KeepsPathNullAndDpiDefault()
    {
        string dir = Directory.CreateTempSubdirectory("documenttools-wiring-").FullName;
        try
        {
            var recorder = new RecordingConvertService();
            DocumentOperationResult result = await RunAsync("word.epub", recorder, dir, new());

            Assert.True(result.Success, result.Message);
            var options = Assert.IsType<WordToEpubOptions>(recorder.LastOptions);
            Assert.Null(options.CoverImagePath);
            Assert.Equal(100, options.CoverDpi);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void WordEpubSchema_ExposesCoverImageAndDpiDefault()
    {
        var fields = FeatureOptionSchemas.GetSections("word.epub")
            .SelectMany(s => s.Fields)
            .ToList();

        OptionField cover = Assert.Single(fields, f => f.Key == "coverImage");
        Assert.Equal(OptionFieldKind.FilePath, cover.Kind);
        Assert.False(string.IsNullOrWhiteSpace(cover.FileFilter));
        OptionField dpi = Assert.Single(fields, f => f.Key == "coverDpi");
        Assert.Equal("100", dpi.DefaultValue);
    }

    [Fact]
    public void CoverImageResolver_FallsBackWhenMissingAndKeepsExistingFile()
    {
        // 封面是可选项：空路径/文件不存在 → null（回退首页渲染），存在 → 原样返回。
        string dir = Directory.CreateTempSubdirectory("documenttools-cover-").FullName;
        try
        {
            string cover = Path.Combine(dir, "cover.png");
            File.WriteAllBytes(cover, [0x89, 0x50]);

            Assert.Null(PathGuard.OptionalExistingFile(null));
            Assert.Null(PathGuard.OptionalExistingFile("  "));
            Assert.Null(PathGuard.OptionalExistingFile(Path.Combine(dir, "missing.png")));
            Assert.Equal(cover, PathGuard.OptionalExistingFile(cover));
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static Task<DocumentOperationResult> RunAsync(
        string featureId,
        IOfficeConvertService service,
        string outputDirectory,
        Dictionary<string, string> options)
        => new OfficeConvertOperation(featureId, service).ExecuteAsync(new DocumentOperationRequest
        {
            InputFiles = [Path.Combine(outputDirectory, "input.docx")],
            OutputDirectory = outputDirectory,
            Options = options
        });

    private sealed class RecordingConvertService : IOfficeConvertService
    {
        public object? LastOptions { get; private set; }

        public void PdfToPpt(string sourceFile, string savedFile, PdfToPptOptions? options = null)
            => LastOptions = options ?? new PdfToPptOptions();

        public void WordToEpub(string sourceFile, string savedFile, WordToEpubOptions? options = null)
            => LastOptions = options ?? new WordToEpubOptions();

        public void WordToPdf(string sourceFile, string savedFile, WordToPdfOptions? options = null)
            => throw new NotSupportedException();

        public void PdfToWord(string sourceFile, string savedFile, PdfToWordOptions? options = null)
            => throw new NotSupportedException();

        public void ExcelToPdf(string sourceFile, string savedFile, ExcelToPdfOptions? options = null)
            => throw new NotSupportedException();

        public void PptToPdf(string sourceFile, string savedFile, PptToPdfOptions? options = null)
            => throw new NotSupportedException();

        public void ImagesToPdf(IEnumerable<string> imageFiles, string savedFile)
            => throw new NotSupportedException();

        public void PdfToExcel(string sourceFile, string savedFile, bool oneSheetPerDocument = true)
            => throw new NotSupportedException();

        public void WordToExcel(string sourceFile, string savedFile, bool exportSingleSheet = true)
            => throw new NotSupportedException();

        public void ExcelToWord(string sourceFile, string savedFile)
            => throw new NotSupportedException();

        public void EpubToWord(string sourceFile, string savedFile)
            => throw new NotSupportedException();

        public void EpubToPdf(string sourceFile, string savedFile)
            => throw new NotSupportedException();

        public IReadOnlyList<string> DocumentToImages(
            string sourceFile,
            string outputDirectory,
            string imageExtension = ".png",
            int dpi = 150,
            bool grayscale = false)
            => throw new NotSupportedException();

        public void PdfToCompliance(string sourceFile, string savedFile, string pdfFormat = "v_1_7")
            => throw new NotSupportedException();
    }
}
