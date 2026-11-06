using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using DocumentTools.Core.Data;

namespace DocumentTools.Core.Services.Exam;

public sealed class ExamReportRow
{
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public int ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
}

public sealed class ReportContentReplaceRequest
{
    public int ReportId { get; set; }
    public string ItemName { get; set; } = "所有项目";
    public bool ChangeItemName { get; set; }
    public bool ChangeRaid { get; set; }
    public string NewItemName { get; set; } = string.Empty;
    public string NewRaid { get; set; } = string.Empty;

    /// <summary>
    /// 是否替换 Content 里的数据格式段 <c>Kind=</c>（legacy ChangeExamReportContent 的 ChangeItemDataFormat，Util.cs:848）。
    /// 注意与 Report_Detail 表自己的 Kind 列（GridFormatData 等）无关，改的是 Content 字符串内部那一段。
    /// </summary>
    public bool ChangeDataFormat { get; set; }

    /// <summary>新的数据格式值，写进 <c>Kind=</c>。</summary>
    public string NewDataFormat { get; set; } = string.Empty;

    /// <summary>
    /// 只替换 <c>Raid=</c> 等于此值的段；留空表示替换所有 Raid 段。
    /// legacy 侧是按 "Replace(Content,'Raid={旧值}','Raid={新值}')" 定点替换的（Util.cs:854），所以旧值得能指定。
    /// </summary>
    public string OldRaid { get; set; } = string.Empty;
}

public sealed class ReportContentReplaceResult
{
    public int ScannedRows { get; set; }
    public int ChangedRows { get; set; }
}

/// <summary>报表网格尺寸（legacy GetExamReportColumnsAndRows，Util.cs:4424-4458）。</summary>
public sealed class ExamReportGrid
{
    public int ReportId { get; set; }

    public int Columns { get; set; }

    public int Rows { get; set; }

    /// <summary>legacy 返回的就是 1 基的序号列表，供报表设计器按列号/行号取值。</summary>
    public IReadOnlyList<int> ColumnIndexes { get; set; } = Array.Empty<int>();

    public IReadOnlyList<int> RowIndexes { get; set; } = Array.Empty<int>();
}

/// <summary>报表「身份」下拉项（legacy GetExamRaidInfo 的两列 RaidShow/RaidValue，Util.cs:4372-4416）。</summary>
/// <summary>身份下拉项的类别（legacy <c>Util.GetExamRaidInfo</c> 追加的两行）。</summary>
public enum ExamRaidOptionKind
{
    /// <summary>单个测评身份。</summary>
    Identity,

    /// <summary>「合计」行：Value 是全部身份用「，」连接的串。</summary>
    Total,

    /// <summary>「批量设置全部身份」行：Value 是 <c>A|B|C|A，B，C</c>，只给批量参数用。</summary>
    BatchAll
}

public sealed class ExamRaidOption
{
    public string Display { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public ExamRaidOptionKind Kind { get; set; }
}

/// <summary>
/// 单元格小数位数（legacy 下拉 <c>ExamReportParaDataFormatReverse_Combox</c> 的 SelectedIndex 0..5）。
/// 映射见 Util.cs:914-942：显示文本与写库的 <c>ReserveDec</c> 一一对应。
/// </summary>
public enum ExamReportNumberFormat
{
    /// <summary>999（整数）。</summary>
    Integer = 0,

    /// <summary>999.9。</summary>
    OneDecimal = 1,

    /// <summary>999.99。</summary>
    TwoDecimals = 2,

    /// <summary>999%（整数百分比）。</summary>
    PercentInteger = 3,

    /// <summary>999.9%。</summary>
    PercentOneDecimal = 4,

    /// <summary>999.99%。</summary>
    PercentTwoDecimals = 5
}

/// <summary>
/// 批量设置数据类型与身份（legacy <c>Util.ChangeExamReportParaContent</c>，Util.cs:864）。
/// 行模式：固定 <see cref="Line"/> 行、遍历 <see cref="StartIndex"/>..<see cref="EndIndex"/> 列；
/// 列模式：固定 <see cref="Line"/> 列、遍历 <see cref="StartIndex"/>..<see cref="EndIndex"/> 行。
/// </summary>
public sealed class ExamReportParaRequest
{
    public int ReportId { get; set; }

    /// <summary>true=行模式（legacy <c>ExamReportPara_RowMode.Checked</c>）；false=列模式。</summary>
    public bool RowMode { get; set; } = true;

    /// <summary>行模式的起始列 / 列模式的起始行（legacy RowMode_Col_Start / ColMode_Row_Start）。</summary>
    public int StartIndex { get; set; } = 1;

    /// <summary>行模式的结束列 / 列模式的结束行（legacy RowMode_Col_End / ColMode_Row_End）。</summary>
    public int EndIndex { get; set; } = 1;

    /// <summary>行模式的行号（RowMode_Row）/ 列模式的列号（ColMode_Col）。</summary>
    public int Line { get; set; } = 1;

    /// <summary>数据类型，取值同 legacy 下拉（见 <see cref="ExamReportService.DataFormatKinds"/>）。</summary>
    public string DataFormatKind { get; set; } = ExamReportService.DefaultDataFormatKind;

    /// <summary>身份；列模式用 <c>|</c> 分隔多个身份（legacy DataFormatRaid.Split('|')，Util.cs:1153）。</summary>
    public string Raid { get; set; } = string.Empty;

    public ExamReportNumberFormat NumberFormat { get; set; } = ExamReportNumberFormat.Integer;

    /// <summary>字体串，三段式 <c>字体,字号,样式</c>，默认 <c>宋体,11,0</c>（Util.cs:909-912、:1046-1057）。</summary>
    public string FontSpec { get; set; } = ExamReportService.DefaultFontSpec;

    /// <summary>项目名，多个用 <c>|</c> 分隔。</summary>
    public string ItemNames { get; set; } = string.Empty;

    /// <summary>选项，多个用 <c>|</c> 分隔；与项目按序号配对成 <c>A#X，B#Y</c>（Util.cs:997-1012）。</summary>
    public string ItemSelectors { get; set; } = string.Empty;

    /// <summary>
    /// legacy 的「插入相同项目/选项」开关：false 时同一内容不会写到第二个单元格（Util.cs:1099-1116）。
    /// 注：本项目按「单元格 × 身份」逐行写入，开关只用于跨单元格抑制重复内容。
    /// </summary>
    public bool InsertSameItemSelector { get; set; } = true;

    /// <summary>
    /// 写库前先删掉该单元格的旧行，保证重复执行不会叠加。
    /// legacy 只在 sqlite 分支做这事（Util.cs:959-961），Access 分支会累积；这里默认开启。
    /// </summary>
    public bool ReplaceExistingRows { get; set; } = true;
}

/// <summary>批量设置数据类型与身份的执行结果。</summary>
public sealed class ExamReportParaResult
{
    /// <summary>处理过的单元格数。</summary>
    public int Cells { get; set; }

    /// <summary>写入的格式行数（每单元格 4 行：Text/Font/TextAlign/Border）。</summary>
    public int FormatRows { get; set; }

    /// <summary>写入的 <c>GridFormatData</c> 行数。</summary>
    public int DataRows { get; set; }

    /// <summary>实际写进 <c>GridFormatText</c> 的显示文本（去重），供 UI 回显与测试断言。</summary>
    public IReadOnlyList<string> DisplayTexts { get; set; } = Array.Empty<string>();
}

/// <summary>报表项目顺序：<see cref="StoredOrder"/> 是库里的物理顺序（重置排序的备份来源）。</summary>
public sealed class ExamReportItemOrder
{
    public int ReportId { get; set; }

    public IReadOnlyList<string> StoredOrder { get; set; } = Array.Empty<string>();

    public IReadOnlyList<string> SortedOrder { get; set; } = Array.Empty<string>();
}

public interface IExamReportService
{
    IReadOnlyList<ExamReportRow> ListReports(string databaseFile);

    ReportContentReplaceResult ReplaceContent(string databaseFile, ReportContentReplaceRequest request);

    ExamReportGrid GetGridSize(string databaseFile, int reportId);

    IReadOnlyList<ExamRaidOption> ListExamRaids(string databaseFile, int examId);

    /// <summary>批量设置数据类型与身份（legacy ChangeExamReportParaContent）。</summary>
    ExamReportParaResult ApplyDataFormat(string databaseFile, ExamReportParaRequest request);

    /// <summary>报表项目清单，按库里的物理顺序返回（legacy Util.GetReportItems，Util.cs:4823-4848）。</summary>
    IReadOnlyList<string> ListReportItems(string databaseFile, int reportId);

    /// <summary>
    /// 考评表自身的项目（legacy <c>Util.GetExamItemsAndExamSelectors</c>，Util.cs:4333-4366：
    /// <c>SELECT ItemName FROM Exam_Score WHERE ExamID=? AND Kind='项目'</c>）。
    /// 与 <see cref="ListReportItems"/> 的区别：这里读的是「考评表定义」，上面读的是「报表正文里用到的占位」。
    /// </summary>
    IReadOnlyList<string> ListExamTableItems(string databaseFile, int examId);

    /// <summary>
    /// 某项目的选项（legacy <c>Util.GetExamItems_Selectors</c>，Util.cs:4299-4330）：
    /// <c>Exam_Score</c> 里 <c>Kind='选项'</c> 的行，排除系统隐藏的「无效」。
    /// </summary>
    IReadOnlyList<string> ListItemSelectors(string databaseFile, int examId, string itemName);

    /// <summary>报表项目顺序：库里物理顺序（备份）+ 自然排序结果（legacy 报表项目排序），供「重置排序」还原。</summary>
    ExamReportItemOrder GetReportItemOrder(string databaseFile, int reportId);

    /// <summary>把项目清单拼成 legacy 的剪贴板文本（<c>|</c> 分隔，BodyForm.cs:7933/7955）。</summary>
    string BuildClipboardText(IReadOnlyList<string> items);

    /// <summary>
    /// 「复制评测主体合计身份」的文本（legacy <c>ExamRaidsCopy_Click</c>）：
    /// 逐个身份显示名 + 「合计」，用「，」连接；「批量设置全部身份」不参与（它只给批量参数用）。
    /// </summary>
    string BuildRaidClipboardText(IReadOnlyList<ExamRaidOption> raids);
}

public sealed class ExamReportService : IExamReportService
{
    /// <summary>Content 内 <c>Item=</c> 段（legacy VF_Report.ExamItemFormat_Regex）。</summary>
    private static readonly Regex ItemRegex = new(@"Item=([^&]*)", RegexOptions.Compiled);

    /// <summary>Content 内 <c>Raid=</c> 段（legacy VF_Report.RaidFormat_Regex）。</summary>
    private static readonly Regex RaidRegex = new(@"Raid=([^&]*)", RegexOptions.Compiled);

    /// <summary>Content 内数据格式段 <c>Kind=</c>（legacy VF_Report.KindFormat_Regex）。</summary>
    private static readonly Regex KindRegex = new(@"Kind=([^&]*)", RegexOptions.Compiled);

    /// <summary>全部项目（不按项目过滤）。</summary>
    public const string AllItemsName = "所有项目";

    /// <summary>报表宽高串的分隔符：legacy 同时认半角与全角逗号（Util.cs:4442-4448）。</summary>
    private static readonly char[] WidthHeightSeparators = [',', '，'];

    /// <summary>Access 侧宽高串要多减 1 段的原因（legacy 注释「老版的系统把最后一列隐藏了」）。</summary>
    public const int AccessHiddenColumnCount = 1;

    /// <summary>默认字体串（legacy Util.cs:909-912 的兜底值）。</summary>
    public const string DefaultFontSpec = "宋体,11,0";

    /// <summary>默认数据类型（legacy 下拉首项，BodyForm.cs:1513）。</summary>
    public const string DefaultDataFormatKind = "该人得票数";

    /// <summary>数据格式对齐值：4 = 居中（legacy Util.cs:1061-1072）。</summary>
    public const string DefaultTextAlign = "4";

    /// <summary>legacy 数据格式下拉的全部取值（BodyForm.cs:1503-1517）。</summary>
    public static readonly IReadOnlyList<string> DataFormatKinds =
    [
        "该人得票数",
        "该人得票率",
        "该人得分",
        "该人得分率",
        "该人得票数|该人得票率"
    ];

    /// <summary>
    /// 小数位数下拉：文案 + 枚举，顺序即 legacy <c>ExamReportParaDataFormatReverse_Combox</c> 的
    /// SelectedIndex 0..5（Util.cs:914-942）。文案属格式知识，视图只负责摆出来。
    /// </summary>
    public static readonly IReadOnlyList<(string Label, ExamReportNumberFormat Format)> NumberFormatOptions =
    [
        ("0 位（999）", ExamReportNumberFormat.Integer),
        ("1 位（999.9）", ExamReportNumberFormat.OneDecimal),
        ("2 位（999.99）", ExamReportNumberFormat.TwoDecimals),
        ("整数百分比（999%）", ExamReportNumberFormat.PercentInteger),
        ("1 位百分比（999.9%）", ExamReportNumberFormat.PercentOneDecimal),
        ("2 位百分比（999.99%）", ExamReportNumberFormat.PercentTwoDecimals)
    ];

    /// <summary>下拉序号 → 小数位数枚举；越界（未选中时 SelectedIndex = -1）夹到两端，不抛异常。</summary>
    public static ExamReportNumberFormat NumberFormatAt(int optionIndex)
        => NumberFormatOptions[Math.Clamp(optionIndex, 0, NumberFormatOptions.Count - 1)].Format;

    /// <summary>
    /// 批量设置的下拉：按行铺或按列铺（legacy 按钮的 bool RowMode，BodyForm.cs:7053）。
    /// 「行模式」是第 1 项，文案与选项顺序都属业务契约，视图不得自写字符串。
    /// </summary>
    public static readonly IReadOnlyList<string> ModeOptions = ["行模式", "列模式"];

    /// <summary>下拉序号 → 是否行模式（未选中 SelectedIndex = -1 时按首项处理）。</summary>
    public static bool IsRowMode(int optionIndex)
        => optionIndex != ModeOptions.Count - 1;

    /// <summary>
    /// 身份值是否为「多身份组合」：列模式用 <c>|</c> 拼多个身份（legacy <c>DataFormatRaid.Split('|')</c>，Util.cs:1153）。
    /// 分隔符属 Core 的格式契约，视图不得自己 <c>Contains('|')</c>。
    /// </summary>
    public static bool IsMultiRaidValue(string? raid)
        => !string.IsNullOrEmpty(raid) && raid.Contains('|', StringComparison.Ordinal);

    public IReadOnlyList<ExamReportRow> ListReports(string databaseFile)
    {
        using var session = new AccessDb(databaseFile);
        DataTable table = session.Query(
            "SELECT Report_Ess.ReportID, Report_Ess.ReportName, Exam_EssInfo.ExamID, Exam_EssInfo.ExamName " +
            "FROM Report_Ess INNER JOIN Exam_EssInfo ON Report_Ess.ExamID = Exam_EssInfo.ExamID");
        var list = new List<ExamReportRow>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(new ExamReportRow
            {
                ReportId = Convert.ToInt32(row["ReportID"]),
                ReportName = Convert.ToString(row["ReportName"]) ?? string.Empty,
                ExamId = Convert.ToInt32(row["ExamID"]),
                ExamName = Convert.ToString(row["ExamName"]) ?? string.Empty
            });
        }

        return list;
    }

    public ReportContentReplaceResult ReplaceContent(string databaseFile, ReportContentReplaceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ReportId <= 0)
        {
            throw new ArgumentException("请先选中一个报表。");
        }

        if (!request.ChangeItemName && !request.ChangeRaid && !request.ChangeDataFormat)
        {
            throw new ArgumentException("请勾选要修改的内容。");
        }

        var result = new ReportContentReplaceResult();
        using var session = new AccessDb(databaseFile);
        session.BeginTransaction();
        try
        {
            var rows = new List<(string Item, string Kind, string Content)>();
            using (var cmd = session.CreateCommand(
                       "SELECT Item, Kind, Content FROM Report_Detail WHERE ReportID = ?",
                       ("@p0", request.ReportId)))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    rows.Add((
                        reader.IsDBNull(0) ? string.Empty : Convert.ToString(reader.GetValue(0)) ?? string.Empty,
                        reader.IsDBNull(1) ? string.Empty : Convert.ToString(reader.GetValue(1)) ?? string.Empty,
                        reader.IsDBNull(2) ? string.Empty : Convert.ToString(reader.GetValue(2)) ?? string.Empty));
                }
            }

            result.ScannedRows = rows.Count;
            foreach (var row in rows)
            {
                // 项目过滤与"改哪几段"是两件事：legacy 的 ChangeType 选中某个项目后，改 Kind / Raid 也只作用于该项目
                // （Util.cs:842-854 三条 Replace 都以该项目为条件），所以这里不能拿 ChangeItemName 当过滤开关。
                if (!string.Equals(request.ItemName, AllItemsName, StringComparison.Ordinal) &&
                    row.Content.IndexOf("Item=" + request.ItemName, StringComparison.Ordinal) < 0)
                {
                    continue;
                }

                string updated = row.Content;
                if (request.ChangeItemName && !string.IsNullOrWhiteSpace(request.NewItemName))
                {
                    updated = ItemRegex.Replace(updated, "Item=" + request.NewItemName);
                }

                if (request.ChangeRaid)
                {
                    string oldRaid = request.OldRaid ?? string.Empty;

                    // legacy「修改为」为空 = 删除该评测主体（BodyForm label173 的注释）：
                    // Replace(Content,'Raid={旧}','Raid=') 把值清空但保留段结构。
                    // 旧值也为空时 Replace('Raid=','Raid=') 无变化（dev 实跑就是没有任何行被改）。
                    if (request.NewRaid.Length == 0)
                    {
                        if (oldRaid.Length > 0 && updated.Contains("Raid=" + oldRaid, StringComparison.Ordinal))
                        {
                            updated = updated.Replace("Raid=" + oldRaid, "Raid=", StringComparison.Ordinal);
                        }
                    }
                    else if (updated.Contains("Raid=", StringComparison.Ordinal))
                    {
                        updated = RaidRegex.Replace(
                            updated,
                            match => oldRaid.Length == 0
                                     || string.Equals(match.Groups[1].Value, oldRaid, StringComparison.Ordinal)
                                ? "Raid=" + request.NewRaid
                                : match.Value);
                    }
                    else if (oldRaid.Length == 0)
                    {
                        // legacy 侧没有 Raid 段时 Replace 落空；这里保留原来的补齐行为（NewRaid 追加在末尾）。
                        updated += (updated.EndsWith('&') || updated.Length == 0 ? "" : "&") + "Raid=" + request.NewRaid;
                    }
                }

                if (request.ChangeDataFormat && !string.IsNullOrWhiteSpace(request.NewDataFormat))
                {
                    updated = KindRegex.Replace(updated, "Kind=" + request.NewDataFormat);
                }

                if (string.Equals(updated, row.Content, StringComparison.Ordinal))
                {
                    continue;
                }

                session.Execute(
                    "UPDATE Report_Detail SET Content = ? WHERE ReportID = ? AND Item = ? AND Kind = ?",
                    ("@p0", updated),
                    ("@p1", request.ReportId),
                    ("@p2", row.Item),
                    ("@p3", row.Kind));
                result.ChangedRows++;
            }

            session.Commit();
            return result;
        }
        catch
        {
            session.Rollback();
            throw;
        }
    }

    /// <summary>
    /// 报表行列数（legacy GetExamReportColumnsAndRows，Util.cs:4424-4458）。
    /// Access 侧宽高串的段数都要减 1：老版考评系统把最后一列/最后一行隐藏了。
    /// </summary>
    public ExamReportGrid GetGridSize(string databaseFile, int reportId)
    {
        if (reportId <= 0)
        {
            throw new ArgumentException("请先选中一个报表。");
        }

        using var session = new AccessDb(databaseFile);
        object? colWidths = session.Scalar(
            "SELECT ColWidthStr FROM Report_Ess WHERE ReportID = ?",
            ("@p0", reportId));
        object? rowHeights = session.Scalar(
            "SELECT RowHeightStr FROM Report_Ess WHERE ReportID = ?",
            ("@p0", reportId));

        int columns = Math.Max(0, CountSegments(colWidths) - AccessHiddenColumnCount);
        int rows = Math.Max(0, CountSegments(rowHeights) - AccessHiddenColumnCount);

        return new ExamReportGrid
        {
            ReportId = reportId,
            Columns = columns,
            Rows = rows,
            ColumnIndexes = Enumerable.Range(1, columns).ToList(),
            RowIndexes = Enumerable.Range(1, rows).ToList()
        };
    }

    /// <summary>
    /// 考评表的测评身份清单 + 「合计」「批量设置全部身份」两项（legacy GetExamRaidInfo，Util.cs:4372-4416）。
    /// 合计值用全角逗号拼接 —— 老版报表把多身份当成一个字符串塞进 Raid 段，逗号是它自己的约定（Util.cs:4393）。
    /// </summary>
    public IReadOnlyList<ExamRaidOption> ListExamRaids(string databaseFile, int examId)
    {
        using var session = new AccessDb(databaseFile);
        DataTable table = session.Query(
            "SELECT RaidName FROM Exam_RaidInfo WHERE ExamID = ?",
            ("@p0", examId));

        var names = new List<string>();
        foreach (DataRow row in table.Rows)
        {
            string name = row["RaidName"] is DBNull ? string.Empty : Convert.ToString(row["RaidName"]) ?? string.Empty;
            if (name.Length > 0)
            {
                names.Add(name);
            }
        }

        var options = names
            .Select(name => new ExamRaidOption
            {
                Display = name,
                Value = name,
                Kind = ExamRaidOptionKind.Identity
            })
            .ToList();

        string joined = string.Join("，", names);
        options.Add(new ExamRaidOption
        {
            Display = names.Count == 0 ? "无测评身份" : "合计",
            Value = joined,
            Kind = ExamRaidOptionKind.Total
        });
        options.Add(new ExamRaidOption
        {
            Display = "批量设置全部身份",
            Value = string.Join("|", names) + "|" + joined,
            Kind = ExamRaidOptionKind.BatchAll
        });

        return options;
    }

    /// <summary>
    /// 批量设置数据类型与身份（legacy ChangeExamReportParaContent，Util.cs:864-1269）。
    /// 每个单元格写 4 行格式（GridFormatText/GridFormatFont/GridFormatTextAlign/GridFormatBorder）
    /// 加每个身份一行 GridFormatData —— Content 拼装口径见 Util.cs:1016-1020。
    /// </summary>
    public ExamReportParaResult ApplyDataFormat(string databaseFile, ExamReportParaRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.ReportId <= 0)
        {
            throw new ArgumentException("请先选中一个报表。");
        }

        if (request.Line <= 0)
        {
            throw new ArgumentException("行号或列号必须大于 0。");
        }

        if (request.StartIndex <= 0 || request.EndIndex < request.StartIndex)
        {
            throw new ArgumentException("行/列范围不合法：起始序号必须大于 0，且终止序号不小于起始序号。");
        }

        string font = NormalizeFontSpec(request.FontSpec);
        (string display, int reserveDec) = MapNumberFormat(request.NumberFormat);
        (string kind2, string itemSegment, string selectorSegment) = ResolveItemParts(request);
        List<ExamCellTarget> cells = BuildCellTargets(request);

        var result = new ExamReportParaResult();
        var displayTexts = new List<string>();
        var seenContents = new HashSet<string>(StringComparer.Ordinal);

        using var session = new AccessDb(databaseFile);
        session.BeginTransaction();
        try
        {
            foreach (ExamCellTarget cell in cells)
            {
                if (request.ReplaceExistingRows)
                {
                    session.Execute(
                        "DELETE FROM Report_Detail WHERE ReportID = ? AND Item = ?",
                        ("@p0", request.ReportId),
                        ("@p1", cell.Item));
                }

                InsertDetail(session, request.ReportId, "GridFormatText", cell.Item, display);
                InsertDetail(session, request.ReportId, "GridFormatFont", cell.Item, font);
                InsertDetail(session, request.ReportId, "GridFormatTextAlign", cell.Item, DefaultTextAlign);
                InsertDetail(session, request.ReportId, "GridFormatBorder", cell.Item, string.Empty);
                result.FormatRows += 4;

                if (!displayTexts.Contains(display, StringComparer.Ordinal))
                {
                    displayTexts.Add(display);
                }

                var writtenForCell = new HashSet<string>(StringComparer.Ordinal);
                foreach (string raid in cell.Raids)
                {
                    string content = BuildCellContent(
                        request.DataFormatKind,
                        kind2,
                        raid,
                        reserveDec,
                        itemSegment,
                        selectorSegment);

                    if (!writtenForCell.Add(content))
                    {
                        continue;
                    }

                    if (!request.InsertSameItemSelector && !seenContents.Add(content))
                    {
                        continue;
                    }

                    seenContents.Add(content);
                    InsertDetail(session, request.ReportId, "GridFormatData", cell.Item, content);
                    result.DataRows++;
                }

                result.Cells++;
            }

            session.Commit();
        }
        catch
        {
            session.Rollback();
            throw;
        }

        result.DisplayTexts = displayTexts;
        return result;
    }

    /// <summary>
    /// 报表项目清单（legacy Util.GetReportItems，Util.cs:4823-4848）。
    /// 只看带格式的 <c>GridFormatData</c> 行，从 <c>Content</c> 的 <c>Item=</c> 段取项目名——
    /// <c>Item</c> 列存的是单元格坐标（<c>1|1</c>），不是项目名；查询口径照抄 <c>VF_Report.QueryContent</c>。
    /// 结果按自然顺序排列，并在首位插入「所有项目」（legacy 同一处 <c>Insert(0, "所有项目")</c>）。
    /// </summary>
    public IReadOnlyList<string> ListReportItems(string databaseFile, int reportId)
    {
        if (reportId <= 0)
        {
            throw new ArgumentException("请先选中一个报表。");
        }

        using var session = new AccessDb(databaseFile);
        DataTable table = session.Query(
            "SELECT Content FROM Report_Detail WHERE Kind = 'GridFormatData' AND Content LIKE 'Bool:%vftab%' AND ReportID = ?",
            ("@p0", reportId));

        var items = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (DataRow row in table.Rows)
        {
            string content = row["Content"] is DBNull ? string.Empty : Convert.ToString(row["Content"]) ?? string.Empty;
            Match match = ItemRegex.Match(content);
            if (!match.Success)
            {
                continue;
            }

            string item = match.Groups[1].Value.Trim();
            if (item.Length > 0 && seen.Add(item))
            {
                items.Add(item);
            }
        }

        return WithAllItemsFirst(items);
    }

    /// <summary>自然排序后把「所有项目」放到首位（legacy 在排序之后 <c>Insert(0, …)</c>）。</summary>
    private static IReadOnlyList<string> WithAllItemsFirst(IEnumerable<string> items)
    {
        var ordered = items.ToList();
        ordered.Sort(ExplorerStringComparer.Instance);
        ordered.Insert(0, AllItemsName);
        return ordered;
    }

    /// <summary>考评表定义里的项目（legacy Util.GetExamItemsAndExamSelectors）。</summary>
    public IReadOnlyList<string> ListExamTableItems(string databaseFile, int examId)
    {
        using var session = new AccessDb(databaseFile);
        DataTable table = session.Query(
            "SELECT ItemName FROM Exam_Score WHERE ExamID = ? AND Kind = '项目'",
            ("@p0", examId));

        var items = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (DataRow row in table.Rows)
        {
            string item = row["ItemName"] is DBNull ? string.Empty : Convert.ToString(row["ItemName"]) ?? string.Empty;
            item = item.Trim();
            if (item.Length > 0 && seen.Add(item))
            {
                items.Add(item);
            }
        }

        return items;
    }

    /// <summary>某项目的选项（legacy Util.GetExamItems_Selectors，排除「无效」）。</summary>
    public IReadOnlyList<string> ListItemSelectors(string databaseFile, int examId, string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return Array.Empty<string>();
        }

        using var session = new AccessDb(databaseFile);
        DataTable table = session.Query(
            "SELECT SelName FROM Exam_Score WHERE ExamID = ? AND ItemName = ? AND Kind = '选项'",
            ("@p0", examId),
            ("@p1", itemName.Trim()));

        var selectors = new List<string>();
        foreach (DataRow row in table.Rows)
        {
            string name = row["SelName"] is DBNull ? string.Empty : Convert.ToString(row["SelName"]) ?? string.Empty;
            name = name.Trim();

            // legacy 只跳过「无效」：它是系统隐式行，不算用户可选的选项。
            if (name.Length > 0 && !string.Equals(name, "无效", StringComparison.Ordinal))
            {
                selectors.Add(name);
            }
        }

        return selectors;
    }

    /// <summary>
    /// 报表项目顺序（legacy「报表项目排序」/「重置项目排序」，BodyForm.cs:9949-9964）。
    /// legacy 的「原顺序备份」= 首次读取时的清单（GetReportItems 已自然排序、已在首位放「所有项目」）；
    /// 排序按钮再排一次、重置按钮还原备份。这里把两者都算好交回 App，重置不必再读库。
    /// </summary>
    public ExamReportItemOrder GetReportItemOrder(string databaseFile, int reportId)
    {
        IReadOnlyList<string> stored = ListReportItems(databaseFile, reportId);

        // 排序时不能把「所有项目」当普通名字排进去：legacy 是排完再 Insert(0)。
        var sorted = stored
            .Where(item => !string.Equals(item, AllItemsName, StringComparison.Ordinal))
            .ToList();
        sorted.Sort(ExplorerStringComparer.Instance);

        return new ExamReportItemOrder
        {
            ReportId = reportId,
            StoredOrder = stored,
            SortedOrder = WithAllItemsFirst(sorted)
        };
    }

    /// <summary>剪贴板文本：项目之间用 <c>|</c> 连接（legacy BodyForm.cs:7933）。</summary>
    public string BuildClipboardText(IReadOnlyList<string> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        return string.Join("|", items);
    }

    /// <summary>
    /// 身份剪贴板文本：显示名用「，」连接（legacy <c>ExamRaidsCopy_Click</c> 的拼法）。
    /// 空列表返回空串，由调用方给「该考评表没有测评身份」的提示。
    /// </summary>
    public string BuildRaidClipboardText(IReadOnlyList<ExamRaidOption> raids)
    {
        ArgumentNullException.ThrowIfNull(raids);
        return string.Join(
            "，",
            raids.Where(raid => raid.Kind != ExamRaidOptionKind.BatchAll)
                .Select(raid => raid.Display)
                .Where(display => display.Length > 0));
    }

    private static void InsertDetail(AccessDb session, int reportId, string kind, string item, string content)
    {
        session.Execute(
            "INSERT INTO Report_Detail (ReportID, Kind, Item, Content) VALUES (?, ?, ?, ?)",
            ("@p0", reportId),
            ("@p1", kind),
            ("@p2", item),
            ("@p3", content));
    }

    /// <summary>Content 拼装（legacy Util.cs:1016-1020）：<c>&amp;vftab;</c> 是它自己的段分隔符。</summary>
    private static string BuildCellContent(
        string dataFormatKind,
        string kind2,
        string raid,
        int reserveDec,
        string itemSegment,
        string selectorSegment)
    {
        var builder = new StringBuilder();
        builder.Append("Bool:Kind=").Append(dataFormatKind).Append("&vftab;");
        builder.Append("Kind2=").Append(kind2).Append("&vftab;");
        builder.Append("Raid=").Append(raid).Append("&vftab;");
        builder.Append("ReserveDec=").Append(reserveDec).Append("&vftab;");
        builder.Append(itemSegment);
        if (selectorSegment.Length > 0)
        {
            builder.Append("&vftab;").Append(selectorSegment);
        }

        return builder.ToString();
    }

    /// <summary>
    /// 项目/选项段：单项写 <c>Item={名}</c> + <c>Selector={选项}</c>；
    /// 多类型（<c>该人得票数|该人得票率</c>）写 <c>GridItem=A#X，B#Y</c>（全角逗号，Util.cs:997-1012）。
    /// </summary>
    private static (string Kind2, string ItemSegment, string SelectorSegment) ResolveItemParts(ExamReportParaRequest request)
    {
        string[] items = SplitPipeValues(request.ItemNames);
        string[] selectors = SplitPipeValues(request.ItemSelectors);
        bool multiKind = request.DataFormatKind.Contains('|', StringComparison.Ordinal);

        if (multiKind || items.Length > 1)
        {
            var pairs = new List<string>();
            int count = Math.Max(items.Length, selectors.Length);
            for (int i = 0; i < count; i++)
            {
                string item = i < items.Length ? items[i] : string.Empty;
                string selector = i < selectors.Length ? selectors[i] : string.Empty;
                pairs.Add(selector.Length == 0 ? item : item + "#" + selector);
            }

            return ("多项合计", "GridItem=" + string.Join("，", pairs), string.Empty);
        }

        return (
            "单个项目",
            "Item=" + (items.Length == 1 ? items[0] : string.Empty),
            "Selector=" + (selectors.Length == 1 ? selectors[0] : string.Empty));
    }

    private static string[] SplitPipeValues(string? text)
        => string.IsNullOrWhiteSpace(text)
            ? Array.Empty<string>()
            : text.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>单元格目标：Item 键 <c>{行}|{列}</c>（Util.cs:953/1034/1149）。</summary>
    private static List<ExamCellTarget> BuildCellTargets(ExamReportParaRequest request)
    {
        var cells = new List<ExamCellTarget>();
        if (request.RowMode)
        {
            for (int column = request.StartIndex; column <= request.EndIndex; column++)
            {
                cells.Add(new ExamCellTarget(request.Line + "|" + column, [request.Raid]));
            }

            return cells;
        }

        string[] raids = SplitPipeValues(request.Raid);
        if (raids.Length == 0)
        {
            raids = [request.Raid];
        }

        for (int row = request.StartIndex; row <= request.EndIndex; row++)
        {
            cells.Add(new ExamCellTarget(row + "|" + request.Line, raids));
        }

        return cells;
    }

    /// <summary>小数位数 →（显示文本, ReserveDec）（legacy Util.cs:914-942）。</summary>
    private static (string Display, int ReserveDec) MapNumberFormat(ExamReportNumberFormat format)
        => format switch
        {
            ExamReportNumberFormat.Integer => ("999", 0),
            ExamReportNumberFormat.OneDecimal => ("999.9", 1),
            ExamReportNumberFormat.TwoDecimals => ("999.99", 2),
            ExamReportNumberFormat.PercentInteger => ("999%", 3),
            ExamReportNumberFormat.PercentOneDecimal => ("999.9%", 4),
            ExamReportNumberFormat.PercentTwoDecimals => ("999.99%", 5),
            _ => ("999", 0)
        };

    /// <summary>字体串兜底：空或不是三段就退回 <c>宋体,11,0</c>（Util.cs:909-912）。</summary>
    private static string NormalizeFontSpec(string? fontSpec)
    {
        if (string.IsNullOrWhiteSpace(fontSpec))
        {
            return DefaultFontSpec;
        }

        return fontSpec.Split(',').Length == 3 ? fontSpec : DefaultFontSpec;
    }

    /// <summary>单元格目标（Item 键 + 该单元格要写的身份列表）。</summary>
    private readonly struct ExamCellTarget(string item, string[] raids)
    {
        public string Item { get; } = item;

        public string[] Raids { get; } = raids;
    }

    /// <summary>
    /// 自然排序（legacy <c>FileNameComparerInstance</c> → <c>ShareImportWindowsApi.StrCmpLogicalW</c>，
    /// FileNameComapreInstance.cs:11 / ShareImportWindowsApi.cs:16）。
    /// </summary>
    private sealed class ExplorerStringComparer : IComparer<string>
    {
        public static readonly ExplorerStringComparer Instance = new();

        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int StrCmpLogicalW(string x, string y);

        public int Compare(string? x, string? y)
        {
            if (x is null)
            {
                return y is null ? 0 : -1;
            }

            if (y is null)
            {
                return 1;
            }

            try
            {
                return StrCmpLogicalW(x, y);
            }
            catch (DllNotFoundException)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
            catch (EntryPointNotFoundException)
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }
        }
    }

    private static int CountSegments(object? widthsOrHeights)
    {
        string text = widthsOrHeights is null || widthsOrHeights is DBNull
            ? string.Empty
            : Convert.ToString(widthsOrHeights) ?? string.Empty;
        return text.Length == 0 ? 0 : text.Split(WidthHeightSeparators, StringSplitOptions.None).Length;
    }
}
