using DocumentTools.Core.Services.Office;
using PdfDocument = Aspose.Pdf.Document;

namespace DocumentTools.Tests;

/// <summary>
/// 合并路径的对象生命周期锁（task：`DocumentMergeSplitService.MergePdfFiles` 的 `merged` 改为 using 之后）。
/// 为什么锁这三件事而不是内存：合并出的 `Aspose.Pdf.Document` 从外部不可见，无法断言它「已释放」；
/// 实测（40 轮同进程合并）改前改后托管内存都稳定在 +2.9MB、峰值工作集 84MB / 83MB，
/// 即这条改动没有可测的内存收益，写内存断言只会得到一条改前改后都绿、无法区分回归的假锁。
/// 真正会因这次改动坏掉的是**释放时机**：一旦 `using` 包住的不是「Save 之后」，
/// 产物就会是空文件或者抛 ObjectDisposedException —— 下面两条用例正好卡住这一点。
/// </summary>
public sealed class MergePdfDisposalTests : IDisposable
{
    private readonly string _root;

    public MergePdfDisposalTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "dt-merge-dispose-" + Guid.NewGuid().ToString("N"));
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
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    [Fact]
    public void MergePdfFiles_KeepsEveryPageAfterInputsAreDisposed()
    {
        // CreatePdf 固定写 input.pdf，所以两份输入各放一个子目录。
        string firstDir = Path.Combine(_root, "a");
        string secondDir = Path.Combine(_root, "b");
        Directory.CreateDirectory(firstDir);
        Directory.CreateDirectory(secondDir);

        string first = ConvertTestAssets.CreatePdf(firstDir, "PART-A-PAGE-1", "PART-A-PAGE-2");
        string second = ConvertTestAssets.CreatePdf(secondDir, "PART-B-PAGE-1", "PART-B-PAGE-2");
        string mergedPath = Path.Combine(_root, "merged.pdf");

        var service = new DocumentMergeSplitService();
        string returned = service.MergePdfFiles([first, second], mergedPath);

        Assert.Equal(mergedPath, returned);
        Assert.True(File.Exists(mergedPath), "合并结果文件不存在");
        Assert.True(new FileInfo(mergedPath).Length > 0, "合并结果文件是空的（Save 时对象可能已被释放）");

        // 两个输入 Document 在方法内部各自 using 释放；页对象若还挂在已释放的输入文档上，这里会塌。
        using var merged = new PdfDocument(mergedPath);
        Assert.Equal(4, merged.Pages.Count);

        string text = ConvertTestAssets.ReadPdfText(mergedPath);
        foreach (string marker in new[] { "PART-A-PAGE-1", "PART-A-PAGE-2", "PART-B-PAGE-1", "PART-B-PAGE-2" })
        {
            Assert.True(text.Contains(marker, StringComparison.Ordinal), $"合并结果缺少标记 {marker}，实际文本：{text}");
        }
    }

    [Fact]
    public void MergePdfFiles_RepeatedCalls_KeepProducingCompleteOutput()
    {
        string firstDir = Path.Combine(_root, "a");
        string secondDir = Path.Combine(_root, "b");
        Directory.CreateDirectory(firstDir);
        Directory.CreateDirectory(secondDir);

        string first = ConvertTestAssets.CreatePdf(firstDir, "ROUND-A");
        string second = ConvertTestAssets.CreatePdf(secondDir, "ROUND-B");

        var service = new DocumentMergeSplitService();

        // 每轮都新建一个 merged Document：改前靠终结器回收，改后由 using 立即释放。
        // 20 轮足以让「每轮少释放一个文档」在文件句柄或原生内存上现形；这里只断言功能仍完整，
        // 内存数字见 docs\feature-gap.md 的 pdf.merge 行。
        for (int round = 1; round <= 20; round++)
        {
            string output = Path.Combine(_root, $"merged_{round}.pdf");
            service.MergePdfFiles([first, second], output);

            Assert.True(File.Exists(output), $"第 {round} 轮没有产出文件");
            Assert.True(new FileInfo(output).Length > 0, $"第 {round} 轮产出是空文件");
        }

        using var last = new PdfDocument(Path.Combine(_root, "merged_20.pdf"));
        Assert.Equal(2, last.Pages.Count);
    }
}
