using Aspose.Words;
using Aspose.Words.Tables;
using System.Globalization;
using Color = System.Drawing.Color;

namespace DocumentTools.Core.Services.Exam;

/// <summary>Port of Util.GenerateVoteReportDocx / DocumentTools.Core.Office.VoteReportDocxService.</summary>
public sealed class VoteReportService : IVoteReportService
{
    private const double PointsPerCentimeter = 28.4;

    public Stream? OpenDefaultTemplate()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "embedded", "VoteReportDoc.doc");
        return File.Exists(path) ? File.OpenRead(path) : null;
    }

    public void Create(VoteReportRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Template is null)
        {
            throw new ArgumentException("模板流不能为空。请将 VoteReportDoc.doc 放到程序目录 embedded/。");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputFile);

        var document = new Document(request.Template);
        var builder = new DocumentBuilder(document);

        var headerItems = new List<string>
        {
            "序号",
            request.NameColumn,
            request.AgreeColumn,
            request.UnAgreeColumn,
            request.AbstainColumn
        };
        var anotherHeaderItems = new List<string> { "序号", "另选人姓名", request.AgreeColumn };

        document.Range.Replace("%Report_Title%", request.Title);
        document.Range.Replace("%Report_Start%", request.StartText);
        document.Range.Replace("%Report_End%", request.EndText);
        document.Range.Replace("%Report_Date%", ResolveGenerationDate(request));

        double dataRowHeight = request.DataRowHeightCentimeters * PointsPerCentimeter;
        double dataRowFontSize = request.DataRowFontSize;

        builder.MoveToParagraph(4, 0);
        Table table = builder.StartTable();

        if (!request.KeepNumberIndex)
        {
            headerItems.RemoveAt(0);
            anotherHeaderItems.RemoveAt(0);
        }

        foreach (string header in headerItems)
        {
            builder.InsertCell();
            table.RelativeHorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center;
            table.TextWrapping = TextWrapping.None;
            table.AllowAutoFit = false;
            builder.RowFormat.Height = request.RowHeightCentimeters * PointsPerCentimeter;
            builder.RowFormat.HeightRule = HeightRule.Exactly;
            builder.RowFormat.HeadingFormat = true;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
            builder.CellFormat.Shading.BackgroundPatternColor = Color.LightYellow;
            builder.CellFormat.WrapText = false;
            builder.Font.Size = request.FontSize;
            builder.Font.Name = "SimSun";
            builder.Write(header);
        }

        builder.EndRow();
        var random = new Random();
        int index = 1;
        foreach (string fillName in request.FillNames)
        {
            if (string.IsNullOrWhiteSpace(fillName))
            {
                continue;
            }

            builder.InsertCell();
            builder.RowFormat.HeadingFormat = false;
            builder.RowFormat.Height = dataRowHeight;
            builder.RowFormat.HeightRule = HeightRule.Exactly;
            builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
            builder.Font.Size = dataRowFontSize;
            builder.Font.Name = "SimSun";
            builder.Write(request.KeepNumberIndex ? index.ToString() : fillName);

            for (int column = 1; column < headerItems.Count; column++)
            {
                builder.InsertCell();
                builder.RowFormat.Height = dataRowHeight;
                builder.RowFormat.HeightRule = HeightRule.Exactly;
                builder.Font.Size = dataRowFontSize;
                if (column == 1 && request.KeepNumberIndex)
                {
                    builder.Write(fillName);
                }
                else if (request.RandomVoteCounts)
                {
                    builder.Write(random.Next(index, Math.Max(index + 1, index * 10)).ToString());
                }
                else
                {
                    builder.Write("0");
                }
            }

            builder.EndRow();
            index++;
        }

        builder.EndTable();

        if (request.NeedAnotherTable)
        {
            builder.InsertParagraph();
            builder.ParagraphFormat.FirstLineIndent = 15;
            builder.ParagraphFormat.SpaceBeforeAuto = true;
            builder.ParagraphFormat.SpaceAfter = 6;
            builder.Font.Size = 16;
            builder.Font.Name = "FangSong_GB2312";
            builder.Write("另选人得票情况如下：");

            Table anotherTable = builder.StartTable();
            foreach (string header in anotherHeaderItems)
            {
                builder.InsertCell();
                anotherTable.RelativeHorizontalAlignment = Aspose.Words.Drawing.HorizontalAlignment.Center;
                anotherTable.TextWrapping = TextWrapping.None;
                anotherTable.AllowAutoFit = false;
                builder.ParagraphFormat.SpaceAfter = 0;
                builder.RowFormat.Height = request.RowHeightCentimeters * PointsPerCentimeter;
                builder.RowFormat.HeightRule = HeightRule.Exactly;
                builder.RowFormat.HeadingFormat = true;
                builder.ParagraphFormat.FirstLineIndent = 0;
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                builder.CellFormat.VerticalAlignment = CellVerticalAlignment.Center;
                builder.CellFormat.Shading.BackgroundPatternColor = Color.LightYellow;
                builder.CellFormat.WrapText = false;
                builder.Font.Size = request.FontSize;
                builder.Font.Name = "SimSun";
                builder.Write(header);
            }

            builder.EndRow();
            for (int row = 0; row < 3; row++)
            {
                builder.InsertCell();
                builder.RowFormat.HeadingFormat = false;
                builder.RowFormat.Height = dataRowHeight;
                builder.RowFormat.HeightRule = HeightRule.Exactly;
                builder.CellFormat.Shading.BackgroundPatternColor = Color.White;
                builder.Font.Size = dataRowFontSize;
                builder.Font.Name = "SimSun";
                builder.Write(request.KeepNumberIndex ? (row + 1).ToString() : string.Empty);
                for (int column = 1; column < anotherHeaderItems.Count; column++)
                {
                    builder.InsertCell();
                    builder.RowFormat.Height = dataRowHeight;
                    builder.RowFormat.HeightRule = HeightRule.Exactly;
                    builder.Font.Size = dataRowFontSize;
                }

                builder.EndRow();
            }

            builder.EndTable();
        }

        if (!request.KeepFooter)
        {
            foreach (Section section in document.Sections)
            {
                section.HeadersFooters[HeaderFooterType.FooterEven]?.Remove();
                section.HeadersFooters[HeaderFooterType.FooterPrimary]?.Remove();
                section.HeadersFooters[HeaderFooterType.FooterFirst]?.Remove();
            }
        }

        string? dir = Path.GetDirectoryName(Path.GetFullPath(request.OutputFile));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        document.Save(request.OutputFile, SaveFormat.Docx);
    }

    /// <summary>
    /// 生成日期：优先手工填写的 <see cref="VoteReportRequest.GenerationDate"/>，其次旧字段 <c>GenDateText</c>，
    /// 都为空时用当天（legacy「今天」按钮口径 <c>yyyy年MM月dd日</c>，BodyForm.cs:5784-5791）。
    /// </summary>
    private static string ResolveGenerationDate(VoteReportRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.GenerationDate))
        {
            return request.GenerationDate.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.GenDateText))
        {
            return request.GenDateText.Trim();
        }

        return DateTime.Now.ToString("yyyy年MM月dd日", CultureInfo.InvariantCulture);
    }
}
