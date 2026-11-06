using System.Text;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Image;
using DocumentTools.Core.Services.Office;
using Xunit;

namespace DocumentTools.Tests;

/// <summary>
/// 用户可见错误消息规范化（docs/qa-report-compat.md 的 4 条）。
/// 断言只钉关键中文片段，不钉整串——文案可以润色，但「为什么失败 + 怎么修」必须在。
/// </summary>
public sealed class OperationErrorMessageTests : IDisposable
{
    private readonly string _root;

    public OperationErrorMessageTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "dt-errmsg-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
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
            // 临时目录清不掉不影响断言结果。
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public async Task Merge_BeyondEvaluationLimit_ExplainsLimitAndFix()
    {
        // 3 个 2 页 PDF 合并 = 6 页，评估模式下 Pages 集合超过 4 个元素必然触发限制。
        var inputs = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            string path = Path.Combine(_root, $"merge{i}.pdf");
            WriteRawPdf(path, pageCount: 2);
            inputs.Add(path);
        }

        var operation = new MergeDocumentOperation("pdf.merge", new DocumentMergeSplitService());
        DocumentOperationResult result = await operation.ExecuteAsync(new DocumentOperationRequest
        {
            InputFiles = inputs,
            OutputDirectory = Path.Combine(_root, "merged")
        });

        // 授权脚手架已移除：提示不再指向 assets\licenses，改断言「拆分后再试」这个可执行动作。
        AssertTranslatedOrSucceeded(result, "评估模式", "拆分");
    }

    [Fact]
    public async Task Split_BeyondEvaluationLimit_ExplainsLimitAndFix()
    {
        string source = Path.Combine(_root, "six-pages.pdf");
        WriteRawPdf(source, pageCount: 6);

        var operation = new SplitDocumentOperation(
            "pdf.split",
            new DocumentMergeSplitService(),
            new PdfSplitService());
        DocumentOperationResult result = await operation.ExecuteAsync(new DocumentOperationRequest
        {
            InputFiles = new[] { source },
            OutputDirectory = Path.Combine(_root, "split"),
            Options = new Dictionary<string, string> { ["splitMode"] = "每页拆分" }
        });

        AssertTranslatedOrSucceeded(result, "评估模式", "拆分");
    }

    [Fact]
    public async Task ImageConvert_OutputDirectoryIsAFile_ReturnsActionableChineseMessage()
    {
        string image = Path.Combine(_root, "src.png");
        using (var bitmap = new global::System.Drawing.Bitmap(40, 30))
        {
            bitmap.Save(image, global::System.Drawing.Imaging.ImageFormat.Png);
        }

        // 把「输出目录」指向一个已存在的文件：Directory.CreateDirectory 必抛英文 IOException。
        string blocker = Path.Combine(_root, "blocker.txt");
        File.WriteAllText(blocker, "not a directory");

        var operation = new ImageConvertOperation("image.compress", new ImageConversionService());
        DocumentOperationResult result = await operation.ExecuteAsync(new DocumentOperationRequest
        {
            InputFiles = new[] { image },
            OutputDirectory = blocker
        });

        Assert.False(result.Success, "输出目录指向文件时不应该成功");
        Assert.Contains("输出目录", result.Message);
        Assert.Contains("请换一个目录", result.Message);
        Assert.DoesNotContain("Access to the path", result.Message);
        Assert.DoesNotContain("System.IO", result.Message);
    }

    [Fact]
    public async Task ImageConvert_ReadOnlyOutputFile_ExplainsHowToFix()
    {
        // 真实触发 UnauthorizedAccessException：目录可写，但目标文件是只读的
        // （Windows 上给「目录」设 ReadOnly 属性并不会阻止写入——实测确认过，所以只能锁文件）。
        string image = Path.Combine(_root, "ro.png");
        using (var bitmap = new global::System.Drawing.Bitmap(40, 30))
        {
            bitmap.Save(image, global::System.Drawing.Imaging.ImageFormat.Png);
        }

        string outputDirectory = Path.Combine(_root, "readonly-target");
        Directory.CreateDirectory(outputDirectory);
        string victim = Path.Combine(outputDirectory, "ro_out.png");
        File.WriteAllText(victim, "placeholder");

        try
        {
            File.SetAttributes(victim, FileAttributes.ReadOnly);

            var operation = new ImageConvertOperation("image.compress", new ImageConversionService());
            DocumentOperationResult result = await operation.ExecuteAsync(new DocumentOperationRequest
            {
                InputFiles = new[] { image },
                OutputDirectory = outputDirectory
            });

            Assert.False(result.Success, "覆盖只读文件必须失败");
            Assert.Contains("输出文件", result.Message);
            Assert.Contains("请换一个", result.Message);
            Assert.Contains("只读属性", result.Message);
            Assert.Contains(outputDirectory, result.Message);
            Assert.DoesNotContain("Access to the path", result.Message);
        }
        finally
        {
            File.SetAttributes(victim, FileAttributes.Normal);
        }
    }

    [Fact]
    public void Translate_UnauthorizedAccess_ExplainsReadOnlyFixAndKeepsInnerException()
    {
        var original = new UnauthorizedAccessException(@"Access to the path 'D:\out\a.png' is denied.");

        Exception friendly = OperationErrors.Translate(original, "转换图片", @"D:\out");

        Assert.Contains("输出目录不可写", friendly.Message);
        Assert.Contains(@"D:\out", friendly.Message);
        Assert.Contains("只读属性", friendly.Message);
        Assert.Same(original, friendly.InnerException);
    }

    [Fact]
    public void Translate_ChineseMessageFromService_IsNotWrappedTwice()
    {
        // 各 Service 自己抛的中文可操作消息必须原样透出，不能被「XX 失败：」再包一层。
        const string original = "合并 PDF 只接受 .pdf 文件，以下输入不会被合并：a.txt";
        var exception = new InvalidOperationException(original);

        Exception friendly = OperationErrors.Translate(exception, "合并 PDF", @"D:\out");

        Assert.Same(exception, friendly);
        Assert.Equal(original, friendly.Message);
    }

    [Fact]
    public void Translate_EvaluationLimitMessage_IsRecognized()
    {
        var evaluation = new InvalidOperationException(
            "At most 4 elements (for any collection) can be viewed in evaluation mode.");

        Assert.True(OperationErrors.IsEvaluationLimit(evaluation));

        Exception friendly = OperationErrors.Translate(evaluation, "拆分文档", @"D:\out");
        Assert.Contains("评估模式", friendly.Message);
        Assert.Contains("拆分", friendly.Message);
        Assert.DoesNotContain("evaluation mode", friendly.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HtmlToPdf_MissingBrowser_SaysWhatToInstallAndWhichOptionKey()
    {
        string html = Path.Combine(_root, "page.html");
        File.WriteAllText(html, "<html><body><p>hello</p></body></html>", Encoding.UTF8);

        var service = new HtmlToPdfService();
        var options = new HtmlToPdfOptions
        {
            BrowserExecutablePath = Path.Combine(_root, "chrome-not-here.exe")
        };

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ConvertAsync(html, Path.Combine(_root, "out.pdf"), options));

        Assert.Contains("Chrome", ex.Message);
        Assert.Contains("chromePath", ex.Message);
        Assert.Contains("请安装", ex.Message);
    }

    private static void AssertTranslatedOrSucceeded(
        DocumentOperationResult result,
        string requiredFragment,
        string secondFragment)
    {
        if (result.Success)
        {
            // 装了正式授权时评估限制不存在，此时只要求产出真实文件。
            Assert.NotEmpty(result.OutputFiles);
            Assert.True(File.Exists(result.OutputFiles[0]), "成功时输出文件必须存在：" + result.OutputFiles[0]);
            return;
        }

        Assert.Contains(requiredFragment, result.Message);
        Assert.Contains(secondFragment, result.Message);
        Assert.DoesNotContain("evaluation mode", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("At most", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 手写最小 PDF（评估模式不允许用 Aspose 造 &gt;4 页的集合，所以只能用原始字节拼）。
    /// </summary>
    private static void WriteRawPdf(string path, int pageCount)
    {
        var kids = new StringBuilder();
        for (int i = 0; i < pageCount; i++)
        {
            kids.Append(3 + (i * 2)).Append(" 0 R ");
        }

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [ {kids.ToString().Trim()} ] /Count {pageCount} >>"
        };

        for (int i = 0; i < pageCount; i++)
        {
            objects.Add(
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595.28 841.89]"
                + " /Resources << /Font << /F1 << /Type /Font /Subtype /Type1 /BaseFont /Helvetica >> >> >>"
                + $" /Contents {4 + (i * 2)} 0 R >>");
            string content = $"BT /F1 24 Tf 72 700 Td (PAGE {i + 1}) Tj ET";
            objects.Add($"<< /Length {content.Length} >>\nstream\n{content}\nendstream");
        }

        var document = new StringBuilder();
        document.Append("%PDF-1.4\n");
        var offsets = new List<int>();
        for (int i = 0; i < objects.Count; i++)
        {
            offsets.Add(document.Length);
            document.Append(i + 1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n");
        }

        int xrefOffset = document.Length;
        document.Append("xref\n0 ").Append(objects.Count + 1).Append('\n');
        document.Append("0000000000 65535 f \n");
        foreach (int offset in offsets)
        {
            document.Append(offset.ToString("D10")).Append(" 00000 n \n");
        }

        document.Append("trailer\n<< /Size ").Append(objects.Count + 1).Append(" /Root 1 0 R >>\n");
        document.Append("startxref\n").Append(xrefOffset).Append('\n');
        document.Append("%%EOF\n");

        File.WriteAllText(path, document.ToString(), Encoding.ASCII);
    }
}
