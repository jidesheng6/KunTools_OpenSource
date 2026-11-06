using System.IO.Compression;
using System.Text.RegularExpressions;
using Aspose.Words;
using DocumentTools.Core.Services.Exam;

namespace DocumentTools.Tests;

/// <summary>
/// exam.vote 报告单排版（legacy <c>Util.GenerateVoteReportDocx</c>，Util.cs:3252）。
/// 断言落在**产物 DOCX 的 XML** 上：字号是 <c>w:sz</c>（半磅）、行高是 <c>w:trHeight</c>（twips）、
/// 生成日期是正文文本，这样才能证明写进文档的是新参数而不是只改了请求对象。。
/// <para>
/// 踩过的坑（别重犯）：Aspose 把行高写成 <c>&lt;w:trHeight w:hRule="exact" w:val="682" /&gt;</c>，
/// **属性顺序不固定**（<c>hRule</c> 排在 <c>val</c> 之前）。正则写死 <c>w:trHeight w:val=</c> 会一条都匹配不到，
/// 失败现象只是「集合为空」，很难看出是属性顺序问题；必须用 <c>w:trHeight[^&gt;]*w:val=</c> 这类容忍顺序的写法。
/// </para>
/// </summary>
public sealed class VoteReportServiceTests
{
    private const double PointsPerCentimeter = 28.4;
    private const double TwipsPerPoint = 20;

    private static readonly Regex FontSizeRegex = new("w:sz w:val=\"(?<v>\\d+)\"", RegexOptions.Compiled);
    // Aspose 把行高写成 <w:trHeight w:hRule="exact" w:val="682" />，属性顺序不固定，只认 w:val。
    private static readonly Regex RowHeightRegex = new("w:trHeight[^>]*w:val=\"(?<v>\\d+)\"", RegexOptions.Compiled);

    private readonly VoteReportService _sut = new();

    [Fact]
    public void Create_WritesTableFontSizeAndRowHeightsIntoTheDocument()
    {
        string output = NewOutputPath();
        try
        {
            using MemoryStream template = CreateTemplate();
            _sut.Create(new VoteReportRequest
            {
                Template = template,
                OutputFile = output,
                Title = "选举结果报告单",
                StartText = "根据投票情况，结果如下：",
                EndText = "特此报告。",
                GenerationDate = "2024年5月6日",
                FillNames = ["张三", "李四"],
                FontSize = 14,
                RowHeightCentimeters = 1.2,
                DataRowFontSize = 9,
                DataRowHeightCentimeters = 0.5
            });

            string xml = ReadDocumentXml(output);

            List<int> sizes = FontSizeRegex.Matches(xml)
                .Select(match => int.Parse(match.Groups["v"].Value, System.Globalization.CultureInfo.InvariantCulture))
                .ToList();
            Assert.Contains(28, sizes);   // 表头 14 磅
            Assert.Contains(18, sizes);   // 数据行 9 磅

            List<double> heights = RowHeightRegex.Matches(xml)
                .Select(match => double.Parse(match.Groups["v"].Value, System.Globalization.CultureInfo.InvariantCulture))
                .ToList();
            Assert.Contains(heights, value => Math.Abs(value - ToTwips(1.2)) <= 12);   // 表头行高
            Assert.Contains(heights, value => Math.Abs(value - ToTwips(0.5)) <= 12);   // 数据行行高
        }
        finally
        {
            File.Delete(output);
        }
    }

    [Fact]
    public void Create_ManualGenerationDate_ReplacesDatePlaceholder()
    {
        string output = NewOutputPath();
        try
        {
            using MemoryStream template = CreateTemplate();
            _sut.Create(new VoteReportRequest
            {
                Template = template,
                OutputFile = output,
                Title = "选举结果报告单",
                GenerationDate = "2023年11月2日",
                FillNames = ["张三"]
            });

            string xml = ReadDocumentXml(output);
            Assert.Contains("2023年11月2日", xml, StringComparison.Ordinal);
            Assert.DoesNotContain("%Report_Date%", xml, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(output);
        }
    }

    [Fact]
    public void Create_BlankGenerationDate_FallsBackToToday()
    {
        string output = NewOutputPath();
        string today = DateTime.Now.ToString("yyyy年MM月dd日", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            using MemoryStream template = CreateTemplate();
            _sut.Create(new VoteReportRequest
            {
                Template = template,
                OutputFile = output,
                Title = "选举结果报告单",
                GenerationDate = "   ",
                FillNames = ["张三"]
            });

            Assert.Contains(today, ReadDocumentXml(output), StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(output);
        }
    }

    private static double ToTwips(double centimeters) => centimeters * PointsPerCentimeter * TwipsPerPoint;

    private static string NewOutputPath()
        => Path.Combine(Path.GetTempPath(), "dt-vote-" + Guid.NewGuid().ToString("N") + ".docx");

    /// <summary>造一份带占位符的模板：Create 会 MoveToParagraph(4)，所以要留够段落。</summary>
    private static MemoryStream CreateTemplate()
    {
        var document = new Document();
        var builder = new DocumentBuilder(document);
        builder.Writeln("%Report_Title%");
        builder.Writeln("%Report_Start%");
        builder.Writeln("%Report_End%");
        builder.Writeln("%Report_Date%");
        builder.Writeln("占位段落一");
        builder.Writeln("占位段落二");

        var stream = new MemoryStream();
        document.Save(stream, SaveFormat.Docx);
        stream.Position = 0;
        return stream;
    }

    private static string ReadDocumentXml(string docxPath)
    {
        using ZipArchive archive = ZipFile.OpenRead(docxPath);
        ZipArchiveEntry entry = archive.GetEntry("word/document.xml")
            ?? throw new InvalidOperationException("DOCX 里没有 word/document.xml");
        using Stream stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
