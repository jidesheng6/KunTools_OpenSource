using System.Data;
using System.Globalization;
using DocumentTools.Core.Data;

namespace DocumentTools.Core.Services.Exam;

public interface IExamInsertService
{
    ExamInsertResult Insert(ExamInsertRequest request);

    IReadOnlyList<(int Id, string Name, string TypeMode)> ListExams(string databaseFile);
}

/// <summary>Port of DocumentTools.Core.Exam.ExamInsertService / Util insert exam paths.</summary>
public sealed class ExamInsertService : IExamInsertService
{
    private static readonly (string Kind, string Para)[] AccessRulers =
    [
        ("单项不填规则", "自动当作弃权"),
        ("单项多填规则", "只记录最前被选项后再交给人工"),
        ("多项不填规则", "自动当作弃权"),
        ("全部无效规则", "无效票False"),
        ("全部弃权规则", "无效票False")
    ];

    public ExamInsertResult Insert(ExamInsertRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.ExamName))
        {
            return ExamInsertResult.Failed("考评表名称为空");
        }

        if (string.IsNullOrWhiteSpace(request.DatabaseFile) || !File.Exists(request.DatabaseFile))
        {
            return ExamInsertResult.Failed("数据库文件不存在");
        }

        string ext = Path.GetExtension(request.DatabaseFile);
        if (ext.Equals(".db", StringComparison.OrdinalIgnoreCase))
        {
            return ExamInsertResult.Failed("当前版本完整支持 .mdb/.accdb（Access）；.db 请暂用旧版或后续 SQLite 通道。");
        }

        try
        {
            using var session = new AccessDb(request.DatabaseFile);
            session.BeginTransaction();
            try
            {
                ExamInsertResult result = WriteAll(session, request, ExamStoreDialect.Access2003);
                session.Commit();
                return result;
            }
            catch (Exception ex)
            {
                session.Rollback();
                return ExamInsertResult.Failed("插入失败：" + ex.Message + "（已回滚）");
            }
        }
        catch (Exception ex)
        {
            return ExamInsertResult.Failed("插入失败：" + ex.Message);
        }
    }

    public IReadOnlyList<(int Id, string Name, string TypeMode)> ListExams(string databaseFile)
    {
        using var session = new AccessDb(databaseFile);
        DataTable table = session.Query(
            "SELECT ExamId, ExamName, TypeMode FROM Exam_EssInfo ORDER BY ExamId");
        var list = new List<(int, string, string)>();
        foreach (DataRow row in table.Rows)
        {
            list.Add((
                Convert.ToInt32(row["ExamId"], CultureInfo.InvariantCulture),
                Convert.ToString(row["ExamName"]) ?? string.Empty,
                Convert.ToString(row["TypeMode"]) ?? string.Empty));
        }

        return list;
    }

    private ExamInsertResult WriteAll(AccessDb session, ExamInsertRequest request, ExamStoreDialect dialect)
    {
        int examId = ResolveExamId(session, request.ExamName, request.TypeMode, out bool created);
        int itemOrder = GetMaxItemOrder(session, examId);
        int itemCount = 0, selectorCount = 0, boolItemCount = 0, implicitCount = 0;

        foreach (ExamInsertItem item in request.Items)
        {
            if (string.IsNullOrWhiteSpace(item.ItemName))
            {
                continue;
            }

            InsertItemScore(session, examId, item, request.ItemScoreWeight, dialect);
            itemCount++;

            foreach (ExamInsertSelector selector in item.Selectors)
            {
                if (string.IsNullOrWhiteSpace(selector.SelectorName))
                {
                    continue;
                }

                selectorCount++;
                if (!ExamInsertRules.IsImplicitSelector(selector.SelectorName))
                {
                    itemOrder++;
                    InsertBoolItem(session, examId, item.ItemName, selector.SelectorName, itemOrder, dialect);
                    boolItemCount++;
                }

                InsertSelectorScore(session, examId, item.ItemName, selector, dialect);
            }

            implicitCount += InsertImplicitSelectors(session, examId, item, request, dialect);
        }

        string implicitNote = implicitCount == 0 ? string.Empty : $"、内置无效/弃权 {implicitCount}";
        return new ExamInsertResult
        {
            Success = true,
            ExamId = examId,
            CreatedExam = created,
            ItemCount = itemCount,
            SelectorCount = selectorCount,
            BoolItemCount = boolItemCount,
            ImplicitSelectorCount = implicitCount,
            Summary =
                $"考评表「{request.ExamName}」序号 {examId}{(created ? "已新建" : "已存在，追加")}：项目 {itemCount}、选项 {selectorCount}、选题 {boolItemCount}{implicitNote}"
        };
    }

    /// <summary>
    /// 按 legacy <c>BodyForm.cs:9254-9268</c> 的写法，为每个项目补写「无效」「弃权」两条隐式选项：
    /// 它们只落 Exam_Score（Kind=选项，Score=对应得分比重），不进 Exam_BoolItem、也不占用 ItemOrder。
    /// 调用方已显式提供同名选项时不重复追加。
    /// </summary>
    private static int InsertImplicitSelectors(
        AccessDb session, int examId, ExamInsertItem item, ExamInsertRequest request, ExamStoreDialect dialect)
    {
        (string Name, string Weight, bool CalcAvg)[] implicitSelectors =
        [
            (ExamInsertRules.InvalidSelectorName, request.InvalidScoreWeight, request.InvalidCalcAvg),
            (ExamInsertRules.AbstainSelectorName, request.AbstainScoreWeight, request.AbstainCalcAvg)
        ];

        int written = 0;
        foreach ((string name, string weight, bool calcAvg) in implicitSelectors)
        {
            bool declared = item.Selectors.Exists(selector =>
                string.Equals(selector.SelectorName?.Trim(), name, StringComparison.Ordinal));
            if (declared)
            {
                continue;
            }

            InsertSelectorScore(
                session,
                examId,
                item.ItemName,
                new ExamInsertSelector { SelectorName = name, ScorePer = weight, CalcAvg = calcAvg },
                dialect);
            written++;
        }

        return written;
    }

    private static int ResolveExamId(AccessDb session, string examName, string typeMode, out bool created)
    {
        object? existing = session.Scalar("SELECT ExamID FROM Exam_EssInfo WHERE ExamName = ?", ("@p0", examName));
        if (existing != null)
        {
            created = false;
            return Convert.ToInt32(existing, CultureInfo.InvariantCulture);
        }

        object? maxId = session.Scalar("SELECT MAX(ExamID) FROM Exam_EssInfo");
        int newId = maxId == null ? 1 : Convert.ToInt32(maxId, CultureInfo.InvariantCulture) + 1;
        session.Execute(
            "INSERT INTO Exam_EssInfo (ExamId,ExamName,TypeMode) VALUES (?,?,?)",
            ("@p0", newId),
            ("@p1", examName),
            ("@p2", typeMode ?? string.Empty));
        InsertDefaultRulers(session, newId);
        created = true;
        return newId;
    }

    private static void InsertDefaultRulers(AccessDb session, int examId)
    {
        int rulerOrder = 2;
        foreach ((string kind, string para) in AccessRulers)
        {
            session.Execute(
                "INSERT INTO Exam_Ruler (ExamId,RulerKind,RulerPara,RulerOrder) VALUES (?,?,?,?)",
                ("@p0", examId), ("@p1", kind), ("@p2", para), ("@p3", rulerOrder));
            rulerOrder++;
        }
    }

    private static int GetMaxItemOrder(AccessDb session, int examId)
    {
        object? value = session.Scalar("SELECT MAX(ItemOrder) FROM Exam_BoolItem WHERE ExamID = ?", ("@p0", examId));
        return value == null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static void InsertItemScore(
        AccessDb session, int examId, ExamInsertItem item, string defaultWeight, ExamStoreDialect dialect)
    {
        object score = ParseScore(string.IsNullOrWhiteSpace(item.ItemScore) ? defaultWeight : item.ItemScore);
        if (dialect == ExamStoreDialect.Sqlite)
        {
            session.Execute(
                "INSERT INTO Exam_Score (ExamId,Type,Kind,ItemName,SelName,Score) VALUES (?,?,?,?,?,?)",
                ("@p0", examId), ("@p1", item.Type ?? ExamInsertRules.ItemType),
                ("@p2", ExamInsertRules.MeasurementKind), ("@p3", item.ItemName),
                ("@p4", string.Empty), ("@p5", score));
            return;
        }

        session.Execute(
            "INSERT INTO Exam_Score (ExamId,Kind,ItemName,SelName,Score) VALUES (?,?,?,?,?)",
            ("@p0", examId), ("@p1", item.Type ?? ExamInsertRules.ItemType),
            ("@p2", item.ItemName), ("@p3", item.ItemName), ("@p4", score));
    }

    private static void InsertSelectorScore(
        AccessDb session, int examId, string itemName, ExamInsertSelector selector, ExamStoreDialect dialect)
    {
        object scorePer = ParseScore(selector.ScorePer);
        InsertSelectorScoreRow(session, examId, itemName, selector, scorePer, dialect);
        if (!selector.CalcAvg)
        {
            InsertSelectorScoreRow(
                session,
                examId,
                itemName,
                new ExamInsertSelector
                {
                    SelectorName = selector.SelectorName,
                    Type = ExamInsertRules.SelectorExcludedFromAverageType,
                    ScorePer = ExamInsertRules.ExcludedSelectorFixedScore
                },
                ParseScore(ExamInsertRules.ExcludedSelectorFixedScore),
                dialect);
        }
    }

    private static void InsertSelectorScoreRow(
        AccessDb session, int examId, string itemName, ExamInsertSelector selector, object score, ExamStoreDialect dialect)
    {
        if (dialect == ExamStoreDialect.Sqlite)
        {
            session.Execute(
                "INSERT INTO Exam_Score (ExamId,Type,Kind,ItemName,SelName,Score) VALUES (?,?,?,?,?,?)",
                ("@p0", examId), ("@p1", selector.Type ?? ExamInsertRules.SelectorType),
                ("@p2", ExamInsertRules.MeasurementKind), ("@p3", itemName),
                ("@p4", selector.SelectorName), ("@p5", score));
            return;
        }

        session.Execute(
            "INSERT INTO Exam_Score (ExamId,Kind,ItemName,SelName,Score) VALUES (?,?,?,?,?)",
            ("@p0", examId), ("@p1", selector.Type ?? ExamInsertRules.SelectorType),
            ("@p2", itemName), ("@p3", selector.SelectorName), ("@p4", score));
    }

    private static void InsertBoolItem(
        AccessDb session, int examId, string itemName, string selectorName, int itemOrder, ExamStoreDialect dialect)
    {
        if (dialect == ExamStoreDialect.Sqlite)
        {
            session.Execute(
                "INSERT INTO Exam_BoolItem (ExamId,AtKind,ItemName,SelName,ItemOrder,AllowMuti) VALUES (?,?,?,?,?,0)",
                ("@p0", examId), ("@p1", ExamInsertRules.MeasurementKind),
                ("@p2", itemName), ("@p3", selectorName), ("@p4", itemOrder));
            return;
        }

        session.Execute(
            "INSERT INTO Exam_BoolItem (ExamId,AtKind,ItemName,SelName,ItemOrder) VALUES (?,?,?,?,?)",
            ("@p0", examId), ("@p1", ExamInsertRules.MeasurementKind),
            ("@p2", itemName), ("@p3", selectorName), ("@p4", itemOrder));
    }

    // 解析逻辑放在 ExamInsertRules（可单测的纯函数），见 ExamInsertModels.cs。
    private static object ParseScore(string raw) => ExamInsertRules.ParseScore(raw);
}
