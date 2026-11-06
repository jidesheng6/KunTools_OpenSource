using Aspose.Words;
using DocumentTools.Core.Services.Office;
using WordsDocument = Aspose.Words.Document;

namespace DocumentTools.Tests;

/// <summary>
/// 本机 Office 转换（legacy <c>UseLocalOffice</c>，<c>Util.cs:2717-2738</c>）的端到端验证。
/// 默认跳过：这条路径需要本机安装 Word，并且会真拉起 <c>WINWORD.EXE</c>（约 3-6 秒）。
/// 需要验证时设置 <c>DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1</c> 再跑。
/// 归进 <see cref="ConversionLogCollection"/>：与归因日志用例共用进程级环境变量，必须串行。
/// </summary>
[Collection(ConversionLogCollection.Name)]
public sealed class WordLocalOfficeTests : IDisposable
{
    private const string OptInVariable = "DOCUMENTTOOLS_TEST_LOCAL_OFFICE";

    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "dt-localoffice-" + Guid.NewGuid().ToString("N"));

    public WordLocalOfficeTests() => Directory.CreateDirectory(_root);

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
            // Word 可能仍持有临时文件句柄，清理失败不影响断言结果。
        }
    }

    [LocalOfficeFact]
    public void WordToPdf_UseLocalOffice_ProducesPdfMadeByWord()
    {
        string source = Path.Combine(_root, "local-office.docx");
        string produced = Path.Combine(_root, "local-office.pdf");

        WordsDocument document = new();
        new DocumentBuilder(document).Write("LOCAL-OFFICE-MARKER-12345");
        document.Save(source);

        var service = new OfficeConvertService();
        service.WordToPdf(source, produced, new WordToPdfOptions { UseLocalOffice = true });

        Assert.True(File.Exists(produced), "本机 Office 转换应产出 PDF。");
        Assert.True(new FileInfo(produced).Length > 1024, "产出的 PDF 不应是空壳。");

        using var pdf = new Aspose.Pdf.Document(produced);
        Assert.Single(pdf.Pages);

        var absorber = new Aspose.Pdf.Text.TextAbsorber();
        pdf.Pages.Accept(absorber);
        Assert.Contains("LOCAL-OFFICE-MARKER", absorber.Text, StringComparison.Ordinal);

        // Aspose 路径的 Producer 是 "Aspose.PDF for .NET"；走 Word COM 时由 Word 自己写元数据。
        // 这条断言是「真的走了本机 Office」而不是「悄悄回退到 Aspose」的证据。
        string producer = pdf.Info.Producer ?? string.Empty;
        Assert.DoesNotContain("Aspose", producer, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WordToPdf_UseLocalOffice_MissingWord_ReportsActionableMessage()
    {
        // 装了 Word 就走成功分支（由上面的 opt-in 用例覆盖真实路径），这里只锁「没装 Word」的文案。
        if (Type.GetTypeFromProgID("Word.Application") is not null)
        {
            return;
        }

        string source = Path.Combine(_root, "no-word.docx");
        WordsDocument document = new();
        new DocumentBuilder(document).Write("x");
        document.Save(source);

        var service = new OfficeConvertService();
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => service.WordToPdf(source, Path.Combine(_root, "no-word.pdf"), new WordToPdfOptions { UseLocalOffice = true }));

        Assert.Contains("Word", ex.Message, StringComparison.Ordinal);
        Assert.Contains("本机", ex.Message, StringComparison.Ordinal);
    }

    /// <summary>只有显式 opt-in 且本机注册了 Word 时才运行的本机 Office 用例。</summary>
    private sealed class LocalOfficeFactAttribute : FactAttribute
    {
        public LocalOfficeFactAttribute()
        {
            if (!string.Equals(Environment.GetEnvironmentVariable(OptInVariable), "1", StringComparison.Ordinal))
            {
                Skip = $"需要显式开启：设置 {OptInVariable}=1（会拉起本机 WINWORD.EXE）。";
            }
            else if (Type.GetTypeFromProgID("Word.Application") is null)
            {
                Skip = "本机未注册 Word.Application，无法验证本机 Office 转换。";
            }
        }
    }
}
