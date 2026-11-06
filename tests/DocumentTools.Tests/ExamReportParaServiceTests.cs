using DocumentTools.Core.Data;
using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;

namespace DocumentTools.Tests;

/// <summary>
/// exam.report 批量参数（legacy <c>Util.ChangeExamReportParaContent</c>，Util.cs:864）在真实 .mdb 上的落库证据。
/// 每个用例都断言库里被改了哪个 Item / Kind / Content，而不是只看返回对象。
/// </summary>
public sealed class ExamReportParaServiceTests
{
    private readonly ExamReportService _sut = new();

    [AceFact]
    public void ApplyDataFormat_RowMode_WritesFourFormatRowsAndOneDataRowPerCell()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamReportParaResult result = _sut.ApplyDataFormat(fixture.DatabaseFile, new ExamReportParaRequest
        {
            ReportId = 7,
            RowMode = true,
            Line = 2,
            StartIndex = 1,
            EndIndex = 2,
            DataFormatKind = "该人得分",
            Raid = "甲",
            NumberFormat = ExamReportNumberFormat.TwoDecimals,
            ItemNames = "笔试",
            ItemSelectors = "%"
        });

        Assert.Equal(2, result.Cells);
        Assert.Equal(8, result.FormatRows);
        Assert.Equal(2, result.DataRows);
        Assert.Contains("999.99", result.DisplayTexts);

        using AccessDb session = fixture.OpenDatabase();

        // GridFormatText：小数位数的显示文本（legacy Util.cs:1032-1042）。
        List<Dictionary<string, object?>> textRows = ExamAccessTestKit.Rows(
            session,
            "SELECT Item, Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatText' AND Item LIKE '2|%'");
        Assert.Equal(2, textRows.Count);
        Assert.All(textRows, row => Assert.Equal("999.99", ExamAccessTestKit.Text(row, "Content")));
        Assert.Contains(textRows, row => ExamAccessTestKit.Text(row, "Item") == "2|1");
        Assert.Contains(textRows, row => ExamAccessTestKit.Text(row, "Item") == "2|2");

        // GridFormatFont / GridFormatTextAlign / GridFormatBorder：每格各一行。
        List<Dictionary<string, object?>> fontRows = ExamAccessTestKit.Rows(
            session,
            "SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatFont' AND Item LIKE '2|%'");
        Assert.Equal(2, fontRows.Count);
        Assert.All(fontRows, row => Assert.Equal("宋体,11,0", ExamAccessTestKit.Text(row, "Content")));

        List<Dictionary<string, object?>> alignRows = ExamAccessTestKit.Rows(
            session,
            "SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatTextAlign' AND Item LIKE '2|%'");
        Assert.Equal(2, alignRows.Count);
        Assert.All(alignRows, row => Assert.Equal("4", ExamAccessTestKit.Text(row, "Content")));

        Assert.Equal(
            2,
            ExamAccessTestKit.Count(
                session,
                "SELECT COUNT(*) FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatBorder' AND Item LIKE '2|%'"));

        // GridFormatData：Content 六段（legacy Util.cs:1016-1020）。
        Dictionary<string, object?> dataRow = Assert.Single(ExamAccessTestKit.Rows(
            session,
            "SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatData' AND Item = '2|1'"));
        string content = ExamAccessTestKit.Text(dataRow, "Content");
        Assert.Contains("Bool:Kind=该人得分", content, StringComparison.Ordinal);
        Assert.Contains("Kind2=单个项目", content, StringComparison.Ordinal);
        Assert.Contains("Raid=甲", content, StringComparison.Ordinal);
        Assert.Contains("ReserveDec=2", content, StringComparison.Ordinal);
        Assert.Contains("Item=笔试", content, StringComparison.Ordinal);
        Assert.Contains("Selector=%", content, StringComparison.Ordinal);
    }

    [AceFact]
    public void ApplyDataFormat_EveryNumberFormat_WritesItsDisplayTextAndReserveDec()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        (ExamReportNumberFormat Format, string Display, int ReserveDec)[] cases =
        [
            (ExamReportNumberFormat.Integer, "999", 0),
            (ExamReportNumberFormat.OneDecimal, "999.9", 1),
            (ExamReportNumberFormat.TwoDecimals, "999.99", 2),
            (ExamReportNumberFormat.PercentInteger, "999%", 3),
            (ExamReportNumberFormat.PercentOneDecimal, "999.9%", 4),
            (ExamReportNumberFormat.PercentTwoDecimals, "999.99%", 5)
        ];

        for (int i = 0; i < cases.Length; i++)
        {
            (ExamReportNumberFormat format, string display, int reserveDec) = cases[i];
            int line = i + 11;
            _sut.ApplyDataFormat(fixture.DatabaseFile, new ExamReportParaRequest
            {
                ReportId = 7,
                RowMode = true,
                Line = line,
                StartIndex = 1,
                EndIndex = 1,
                DataFormatKind = "该人得票率",
                Raid = "甲",
                NumberFormat = format,
                ItemNames = "项目" + line,
                ItemSelectors = "同意"
            });

            using AccessDb session = fixture.OpenDatabase();
            string item = line + "|1";

            Dictionary<string, object?> textRow = Assert.Single(ExamAccessTestKit.Rows(
                session,
                $"SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatText' AND Item = '{item}'"));
            Assert.Equal(display, ExamAccessTestKit.Text(textRow, "Content"));

            Dictionary<string, object?> dataRow = Assert.Single(ExamAccessTestKit.Rows(
                session,
                $"SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatData' AND Item = '{item}'"));
            Assert.Contains("ReserveDec=" + reserveDec, ExamAccessTestKit.Text(dataRow, "Content"), StringComparison.Ordinal);
        }
    }

    [AceFact]
    public void ApplyDataFormat_ColumnMode_WritesOneDataRowPerRaid()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamReportParaResult result = _sut.ApplyDataFormat(fixture.DatabaseFile, new ExamReportParaRequest
        {
            ReportId = 7,
            RowMode = false,
            Line = 3,
            StartIndex = 4,
            EndIndex = 4,
            DataFormatKind = "该人得票数",
            Raid = "甲|乙",
            ItemNames = "面试",
            ItemSelectors = "同意"
        });

        Assert.Equal(1, result.Cells);
        Assert.Equal(2, result.DataRows);

        using AccessDb session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> rows = ExamAccessTestKit.Rows(
            session,
            "SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatData' AND Item = '4|3'");

        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, row => ExamAccessTestKit.Text(row, "Content").Contains("Raid=甲", StringComparison.Ordinal));
        Assert.Contains(rows, row => ExamAccessTestKit.Text(row, "Content").Contains("Raid=乙", StringComparison.Ordinal));
    }

    [AceFact]
    public void ApplyDataFormat_BatchAllRaidValue_WritesEachIdentityPlusTotalRow()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        // 下拉「批量设置全部身份」的 Value 是 "甲|乙|丙|甲，乙，丙"（legacy Util.cs:4403）：
        // 前 N 段是逐个身份，末段是「合计」用的连接串。列模式必须写成 N+1 行，
        // 否则报表渲染按 Raid= 找身份时会找不到「合计」。
        ExamReportParaResult result = _sut.ApplyDataFormat(fixture.DatabaseFile, new ExamReportParaRequest
        {
            ReportId = 7,
            RowMode = false,
            Line = 3,
            StartIndex = 5,
            EndIndex = 5,
            DataFormatKind = "该人得票数",
            Raid = "甲|乙|甲，乙",
            ItemNames = "面试",
            ItemSelectors = "同意"
        });

        Assert.Equal(1, result.Cells);
        Assert.Equal(3, result.DataRows);

        using AccessDb session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> rows = ExamAccessTestKit.Rows(
            session,
            "SELECT Content FROM Report_Detail WHERE ReportID = 7 AND Kind = 'GridFormatData' AND Item = '5|3'");

        Assert.Equal(3, rows.Count);
        Assert.Contains(rows, row => ExamAccessTestKit.Text(row, "Content").Contains("Raid=甲&", StringComparison.Ordinal));
        Assert.Contains(rows, row => ExamAccessTestKit.Text(row, "Content").Contains("Raid=乙&", StringComparison.Ordinal));
        Assert.Contains(rows, row => ExamAccessTestKit.Text(row, "Content").Contains("Raid=甲，乙&", StringComparison.Ordinal));
    }

    [AceFact]
    public void ApplyDataFormat_RunTwice_DoesNotAccumulateRows()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ExamReportParaRequest request = new()
        {
            ReportId = 7,
            RowMode = true,
            Line = 5,
            StartIndex = 1,
            EndIndex = 1,
            DataFormatKind = "该人得票数",
            Raid = "甲",
            ItemNames = "笔试",
            ItemSelectors = "同意"
        };

        _sut.ApplyDataFormat(fixture.DatabaseFile, request);
        _sut.ApplyDataFormat(fixture.DatabaseFile, request);

        using AccessDb session = fixture.OpenDatabase();
        Assert.Equal(
            5,
            ExamAccessTestKit.Count(
                session,
                "SELECT COUNT(*) FROM Report_Detail WHERE ReportID = 7 AND Item = '5|1'"));
    }

    [AceFact]
    public void ApplyDataFormat_InvalidRange_ThrowsChineseArgumentException()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        ArgumentException error = Assert.Throws<ArgumentException>(() => _sut.ApplyDataFormat(
            fixture.DatabaseFile,
            new ExamReportParaRequest { ReportId = 7, StartIndex = 3, EndIndex = 1 }));

        Assert.Contains("范围不合法", error.Message, StringComparison.Ordinal);
    }

    [AceFact]
    public void ListExamTableItems_ReturnsProjectRowsOnly_ForThatExam()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        using (AccessDb session = fixture.OpenDatabase())
        {
            // 项目行（Kind=项目）+ 选项行（Kind=选项）；另一张考评表的项目不能串进来。
            foreach (var (examId, kind, item, sel) in new (int, string, string, string)[]
            {
                (1, "项目", "总体评价", string.Empty),
                (1, "项目", "履职情况", string.Empty),
                (1, "选项", "总体评价", "优秀"),
                (2, "项目", "别的表项目", string.Empty)
            })
            {
                session.Execute(
                    "INSERT INTO Exam_Score (ExamId, Kind, ItemName, SelName, Score) VALUES (?, ?, ?, ?, ?)",
                    ("@p0", examId),
                    ("@p1", kind),
                    ("@p2", item),
                    ("@p3", sel),
                    ("@p4", 0d));
            }
        }

        Assert.Equal(["总体评价", "履职情况"], _sut.ListExamTableItems(fixture.DatabaseFile, 1));
    }

    [AceFact]
    public void ListItemSelectors_ExcludesSystemInvalidRow()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        using (AccessDb session = fixture.OpenDatabase())
        {
            foreach (string selector in new[] { "优秀", "合格", "无效", "弃权" })
            {
                session.Execute(
                    "INSERT INTO Exam_Score (ExamId, Kind, ItemName, SelName, Score) VALUES (?, ?, ?, ?, ?)",
                    ("@p0", 1),
                    ("@p1", "选项"),
                    ("@p2", "总体评价"),
                    ("@p3", selector),
                    ("@p4", 0d));
            }
        }

        // legacy Util.GetExamItems_Selectors 只跳过「无效」；「弃权」仍是可选项。
        Assert.Equal(["优秀", "合格", "弃权"], _sut.ListItemSelectors(fixture.DatabaseFile, 1, "总体评价"));
        Assert.Empty(_sut.ListItemSelectors(fixture.DatabaseFile, 1, "没有这个项目"));
    }

    [AceFact]
    public void GetReportItemOrder_SortsNaturallyAndKeepsAllItemsFirst()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        using (AccessDb session = fixture.OpenDatabase())
        {
            // 真实库形态（Exam_Base.mdb 实测）：Kind=GridFormatData，Content 以 Bool: 开头、用 &vftab; 分段，
            // Item= 在最后一段；Item 列是单元格坐标，不是项目名。
            foreach (string item in new[] { "项目10", "项目9", "项目1" })
            {
                session.Execute(
                    "INSERT INTO Report_Detail (ReportID, Kind, Item, Content) VALUES (?, ?, ?, ?)",
                    ("@p0", 99),
                    ("@p1", "GridFormatData"),
                    ("@p2", "1|1"),
                    ("@p3", $"Bool:Kind=该人得票数&vftab;Raid=甲&vftab;Item={item}&vftab;Selector=优秀"));
            }
        }

        ExamReportItemOrder order = _sut.GetReportItemOrder(fixture.DatabaseFile, 99);

        Assert.Equal(["所有项目", "项目1", "项目9", "项目10"], order.StoredOrder);
        Assert.Equal(["所有项目", "项目1", "项目9", "项目10"], order.SortedOrder);
    }

    [AceFact]
    public void ListReportItems_ReadsItemSegmentFromRealShapedContent()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        using (AccessDb session = fixture.OpenDatabase())
        {
            // 项目10 出现两次（不同选项）应去重；空 Item 段跳过；GridFormatText 行不参与（legacy 只查 GridFormatData）。
            foreach (string content in new[]
            {
                "Bool:Kind=该人得票数&vftab;Raid=甲&vftab;Item=项目10&vftab;Selector=优秀",
                "Bool:Kind=该人得票数&vftab;Raid=甲&vftab;Item=项目10&vftab;Selector=合格",
                "Bool:Kind=该人得票数&vftab;Raid=甲&vftab;Item=&vftab;Selector=弃权",
                "Bool:Kind=该人得票数&vftab;Raid=甲&vftab;Item=项目2&vftab;Selector=优秀"
            })
            {
                session.Execute(
                    "INSERT INTO Report_Detail (ReportID, Kind, Item, Content) VALUES (?, ?, ?, ?)",
                    ("@p0", 98),
                    ("@p1", "GridFormatData"),
                    ("@p2", "1|1"),
                    ("@p3", content));
            }

            session.Execute(
                "INSERT INTO Report_Detail (ReportID, Kind, Item, Content) VALUES (?, ?, ?, ?)",
                ("@p0", 98),
                ("@p1", "GridFormatText"),
                ("@p2", "2|2"),
                ("@p3", "Item=不该出现&vftab;Raid=甲"));
        }

        // 「所有项目」恒在首位（legacy 是在自然排序之后 Insert(0)）；项目2 < 项目10 走数值比较。
        Assert.Equal(["所有项目", "项目2", "项目10"], _sut.ListReportItems(fixture.DatabaseFile, 98));
    }

    [Fact]
    public void BuildClipboardText_JoinsItemsWithPipe()
        => Assert.Equal("甲|乙|丙", _sut.BuildClipboardText(["甲", "乙", "丙"]));
}
