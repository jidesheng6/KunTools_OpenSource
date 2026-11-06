using DocumentTools.Core.Common;
using DocumentTools.Core.Services.Office;
using WordsDocument = Aspose.Words.Document;

namespace DocumentTools.Tests;

/// <summary>
/// <c>word.pdf</c> 本机 Office 分支的降级文案与引擎归因日志（<c>ConversionLog</c>）。
/// 缺 Word 的用例用 <c>DOCUMENTTOOLS_SIMULATE_NO_WORD</c> 模拟，因此在装了 Word 的机器上也跑得起来；
/// 真实导出的用例是 opt-in（<c>DOCUMENTTOOLS_TEST_LOCAL_OFFICE=1</c>），会拉起本机 WINWORD.EXE。
/// 归进 <see cref="ConversionLogCollection"/>：它改的是进程级环境变量，必须串行。
/// </summary>
[Collection(ConversionLogCollection.Name)]
public sealed class WordLocalOfficeDiagnosticsTests : IDisposable
{
    private const string OptInVariable = "DOCUMENTTOOLS_TEST_LOCAL_OFFICE";

    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "dt-word-engine-" + Guid.NewGuid().ToString("N"));

    private readonly string _logDirectory;

    private readonly string? _previousLogDirectory =
        Environment.GetEnvironmentVariable(ConversionLog.LogDirectoryVariable);

    private readonly string? _previousSimulate =
        Environment.GetEnvironmentVariable(OfficeConvertService.SimulateMissingWordVariable);

    public WordLocalOfficeDiagnosticsTests()
    {
        Directory.CreateDirectory(_root);
        _logDirectory = Path.Combine(_root, "logs");
        Directory.CreateDirectory(_logDirectory);

        // 日志落到本用例自己的目录，避免和本机真实运行、其它用例互相污染。
        Environment.SetEnvironmentVariable(ConversionLog.LogDirectoryVariable, _logDirectory);
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable(ConversionLog.LogDirectoryVariable, _previousLogDirectory);
        Environment.SetEnvironmentVariable(OfficeConvertService.SimulateMissingWordVariable, _previousSimulate);

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

    [Fact]
    public void WordToPdf_UseLocalOffice_MissingWord_SaysWhatWasTriedAndHowToFix()
    {
        // 装了 Word 也能跑：模拟开关优先于真实 ProgID 探测。
        Environment.SetEnvironmentVariable(OfficeConvertService.SimulateMissingWordVariable, "1");

        string source = Path.Combine(_root, "no-word.docx");
        WordsDocument document = new();
        new Aspose.Words.DocumentBuilder(document).Write("SIMULATED-NO-WORD");
        document.Save(source);

        var service = new OfficeConvertService();
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(
            () => service.WordToPdf(
                source,
                Path.Combine(_root, "no-word.pdf"),
                new WordToPdfOptions { UseLocalOffice = true }));

        Assert.Contains("未检测到 Microsoft Word", ex.Message, StringComparison.Ordinal);
        Assert.Contains("取消「使用本机 Office 转换」勾选后重试", ex.Message, StringComparison.Ordinal);
        Assert.Contains("已尝试：Word.Application", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WordToPdf_UseLocalOffice_MissingWord_WritesFailureAttribution()
    {
        Environment.SetEnvironmentVariable(OfficeConvertService.SimulateMissingWordVariable, "1");

        string source = Path.Combine(_root, "attr.docx");
        WordsDocument document = new();
        new Aspose.Words.DocumentBuilder(document).Write("ATTRIBUTION");
        document.Save(source);

        var service = new OfficeConvertService();
        Assert.Throws<InvalidOperationException>(
            () => service.WordToPdf(
                source,
                Path.Combine(_root, "attr.pdf"),
                new WordToPdfOptions { UseLocalOffice = true }));

        Assert.True(File.Exists(ConversionLog.LogPath), $"应写出日志：{ConversionLog.LogPath}");
        string log = File.ReadAllText(ConversionLog.LogPath);

        Assert.Contains("[word.pdf]", log, StringComparison.Ordinal);
        Assert.Contains("结果=失败", log, StringComparison.Ordinal);
        Assert.Contains("引擎=Word.Application", log, StringComparison.Ordinal);
        Assert.Contains("attr.docx", log, StringComparison.Ordinal);
        Assert.Contains("未检测到 Microsoft Word", log, StringComparison.Ordinal);
    }

    [LocalWordFact]
    public void WordToPdf_UseLocalOffice_RealWord_LogsEngineAndElapsed()
    {
        string source = Path.Combine(_root, "engine.docx");
        WordsDocument document = new();
        new Aspose.Words.DocumentBuilder(document).Write("ENGINE-ATTRIBUTION");
        document.Save(source);

        string produced = Path.Combine(_root, "engine.pdf");
        var service = new OfficeConvertService();
        service.WordToPdf(source, produced, new WordToPdfOptions { UseLocalOffice = true });

        Assert.True(File.Exists(produced), "本机 Office 转换应产出 PDF。");

        string log = File.ReadAllText(ConversionLog.LogPath);
        string line = Assert.Single(log.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));

        Assert.Contains("结果=成功", line, StringComparison.Ordinal);
        Assert.Contains("引擎=Word.Application", line, StringComparison.Ordinal);
        Assert.Contains("engine.docx", line, StringComparison.Ordinal);
        Assert.Contains("engine.pdf", line, StringComparison.Ordinal);
        Assert.Matches(@"耗时=\d+ms", line);
    }

    /// <summary>只有显式 opt-in 且本机注册了 Word 时才运行（会真拉起 WINWORD.EXE）。</summary>
    private sealed class LocalWordFactAttribute : FactAttribute
    {
        public LocalWordFactAttribute()
        {
            if (!string.Equals(Environment.GetEnvironmentVariable(OptInVariable), "1", StringComparison.Ordinal))
            {
                Skip = $"需要显式开启：设置 {OptInVariable}=1（会拉起本机 WINWORD.EXE）。";
            }
            else if (Type.GetTypeFromProgID(OfficeConvertService.WordApplicationProgId) is null)
            {
                Skip = "本机未注册 Word.Application，无法验证本机 Office 转换。";
            }
        }
    }
}
