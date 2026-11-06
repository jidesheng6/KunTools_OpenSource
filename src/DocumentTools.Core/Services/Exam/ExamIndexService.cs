using System.Data;
using System.Globalization;
using DocumentTools.Core.Data;

namespace DocumentTools.Core.Services.Exam;

/// <summary>改号请求：把考评表序号 <see cref="OriginIndex"/> 改成 <see cref="NewIndex"/>。</summary>
public sealed class ExamIndexChangeRequest
{
    public string DatabaseFile { get; set; } = string.Empty;

    /// <summary>原始考评表序号（legacy 的 OrginTableIndex）。</summary>
    public int OriginIndex { get; set; }

    /// <summary>目标考评表序号。</summary>
    public int NewIndex { get; set; }

    /// <summary>
    /// 改号后是否按 ExamID 升序重写 Exam_EssInfo 的物理记录顺序。
    /// legacy Util.cs:3896-3901 默认就做：UPDATE 只改字段值不改行位置，
    /// 而老版考评软件用不带 ORDER BY 的 "Select * from Exam_EssInfo" 填下拉框。
    /// </summary>
    public bool RebuildPhysicalOrder { get; set; } = true;
}

/// <summary>改号结果。</summary>
public sealed class ExamIndexChangeResult
{
    public bool Success { get; set; }

    /// <summary>目标序号已被占用时为 true：legacy 走"三步换位"把两个序号互换（Util.cs:3863-3887）。</summary>
    public bool Swapped { get; set; }

    /// <summary>legacy 返回的推荐序号（换位时 = 原 MAX(ExamID)+1，未换位时 = -1）。</summary>
    public int RecommendedExamId { get; set; } = -1;

    public bool PhysicalOrderRebuilt { get; set; }

    /// <summary>实际被 UPDATE 的表（库里缺的表会被跳过）。</summary>
    public IReadOnlyList<string> UpdatedTables { get; set; } = Array.Empty<string>();

    public string Summary { get; set; } = string.Empty;
}

/// <summary>Exam_EssInfo 物理顺序重建结果。</summary>
public sealed class ExamPhysicalOrderResult
{
    public bool Success { get; set; }

    public int RowCount { get; set; }

    public string Message { get; set; } = string.Empty;
}

public interface IExamIndexService
{
    ExamIndexChangeResult ChangeExamIndex(ExamIndexChangeRequest request);

    ExamPhysicalOrderResult RebuildExamEssInfoPhysicalOrder(string databaseFile);
}

/// <summary>
/// 考评表序号修改 + Exam_EssInfo 物理顺序重建。
///
/// legacy 出处：
/// - <c>DocumentTools\Util.cs:3817-3915</c> ChangeTableIndex（改号 + 换位 + 触发重建）；
/// - <c>DocumentTools\Util.cs:3930-4073</c> RebuildExamEssInfoPhysicalOrder（DELETE + INSERT 重写并逐列校验）；
/// - 界面入口 <c>DocumentTools\BodyForm.cs:9827-9902</c> ChangeExamTableNo_Start_Click（"修改考评表序号"）。
/// </summary>
public sealed class ExamIndexService : IExamIndexService
{
    /// <summary>考评表主表。</summary>
    public const string ExamEssInfoTable = "Exam_EssInfo";

    /// <summary>
    /// 按 ExamID 关联的表（顺序照抄 legacy Util.cs:3820-3832）。
    /// 改号必须把每一张表里的旧序号一起改掉，否则主表换了号、明细还挂在老号上。
    /// </summary>
    public static readonly string[] ExamIdBoundTables =
    [
        "Exam_BoolItem",
        "Exam_CharItem",
        "Exam_EssInfo",
        "Exam_People",
        "Exam_PeopleDes",
        "Exam_RaidInfo",
        "Exam_Ruler",
        "Exam_Score",
        "Exam_VerifyCode",
        "Report_Ess"
    ];

    public ExamIndexChangeResult ChangeExamIndex(ExamIndexChangeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.DatabaseFile) || !File.Exists(request.DatabaseFile))
        {
            throw new FileNotFoundException("数据库文件不存在：" + request.DatabaseFile, request.DatabaseFile);
        }

        if (request.OriginIndex < 0)
        {
            throw new ArgumentException("原始序号不能为负数。", nameof(request));
        }

        if (request.NewIndex < 0)
        {
            throw new ArgumentException("新序号不能为负数。", nameof(request));
        }

        IReadOnlyList<string> updatedTables;
        var skipped = new List<string>();
        int recommendedExamId;
        bool swapped;

        using (var session = new AccessDb(request.DatabaseFile))
        {
            var existing = new HashSet<string>(session.ListTables(), StringComparer.OrdinalIgnoreCase);
            if (!existing.Contains(ExamEssInfoTable))
            {
                throw new InvalidOperationException("数据库缺少 Exam_EssInfo 表，无法修改考评序号。");
            }

            // 只改真正带 ExamID 列的表：老库/夹具里个别绑定表可能没有该列，
            // 一条 UPDATE 撞上不存在的列会把整次改号连带回滚，所以先按实际结构筛一遍。
            var targets = new List<string>();
            foreach (string table in ExamIdBoundTables.Where(existing.Contains))
            {
                if (HasColumn(session, table, "ExamID"))
                {
                    targets.Add(table);
                }
                else
                {
                    skipped.Add(table);
                }
            }

            session.BeginTransaction();
            try
            {
                object? occupiedValue = session.Scalar(
                    "SELECT COUNT(*) FROM Exam_EssInfo WHERE ExamID = ?",
                    ("@p0", request.NewIndex));
                long occupied = occupiedValue is null
                    ? 0
                    : Convert.ToInt64(occupiedValue, CultureInfo.InvariantCulture);

                if (occupied == 0)
                {
                    foreach (string table in targets)
                    {
                        session.Execute(
                            $"UPDATE [{table}] SET ExamID = ? WHERE ExamID = ?",
                            ("@p0", request.NewIndex),
                            ("@p1", request.OriginIndex));
                    }

                    swapped = false;
                    recommendedExamId = -1;
                }
                else
                {
                    // 目标序号已被占用：target → 临时号，origin → target，临时号 → origin，两步之后两个序号互换。
                    long maxExamId = Convert.ToInt64(
                        session.Scalar("SELECT MAX(ExamID) FROM Exam_EssInfo") ?? 0,
                        CultureInfo.InvariantCulture);
                    int tempExamId = checked((int)maxExamId + 1);

                    foreach (string table in targets)
                    {
                        session.Execute(
                            $"UPDATE [{table}] SET ExamID = ? WHERE ExamID = ?",
                            ("@p0", tempExamId),
                            ("@p1", request.NewIndex));
                        session.Execute(
                            $"UPDATE [{table}] SET ExamID = ? WHERE ExamID = ?",
                            ("@p0", request.NewIndex),
                            ("@p1", request.OriginIndex));
                        session.Execute(
                            $"UPDATE [{table}] SET ExamID = ? WHERE ExamID = ?",
                            ("@p0", request.OriginIndex),
                            ("@p1", tempExamId));
                    }

                    swapped = true;
                    recommendedExamId = tempExamId;
                }

                session.Commit();
                updatedTables = targets;
            }
            catch (Exception ex)
            {
                session.Rollback();
                return new ExamIndexChangeResult
                {
                    Success = false,
                    Summary = "修改失败：" + ex.Message + "（已回滚）"
                };
            }
        }

        var result = new ExamIndexChangeResult
        {
            Success = true,
            Swapped = swapped,
            RecommendedExamId = recommendedExamId,
            UpdatedTables = updatedTables
        };

        string changeSummary = swapped
            ? $"序号 {request.OriginIndex} 与已存在的序号 {request.NewIndex} 已互换（临时序号 {recommendedExamId}）"
            : $"序号 {request.OriginIndex} 已改为 {request.NewIndex}";
        result.Summary = changeSummary + $"；涉及 {updatedTables.Count} 张表";
        if (skipped.Count > 0)
        {
            result.Summary += $"；跳过缺少 ExamID 列的 {skipped.Count} 张表（{string.Join(",", skipped)}）";
        }

        if (request.RebuildPhysicalOrder)
        {
            ExamPhysicalOrderResult order = RebuildExamEssInfoPhysicalOrder(request.DatabaseFile);
            result.PhysicalOrderRebuilt = order.Success;
            result.Summary += "；" + order.Message;
        }

        return result;
    }

    /// <summary>
    /// 按 ExamID 升序重建 Exam_EssInfo 的物理记录顺序（legacy Util.cs:3930-4073）。
    /// DELETE 与 INSERT 在同一事务内，校验不过就整表回滚。
    /// </summary>
    public ExamPhysicalOrderResult RebuildExamEssInfoPhysicalOrder(string databaseFile)
    {
        if (string.IsNullOrWhiteSpace(databaseFile) || !File.Exists(databaseFile))
        {
            return new ExamPhysicalOrderResult { Success = false, Message = "数据库文件不存在：" + databaseFile };
        }

        try
        {
            using var session = new AccessDb(databaseFile);
            DataTable snapshot = session.Query("SELECT * FROM Exam_EssInfo");
            if (snapshot.Rows.Count <= 1)
            {
                return new ExamPhysicalOrderResult
                {
                    Success = true,
                    RowCount = snapshot.Rows.Count,
                    Message = "<Exam_EssInfo>记录数不超过 1 条，无需重建"
                };
            }

            string? examIdColumn = FindColumn(snapshot, "ExamID");
            if (examIdColumn is null)
            {
                return new ExamPhysicalOrderResult
                {
                    Success = false,
                    Message = "Exam_EssInfo 缺少 ExamID 列，无法重建物理顺序"
                };
            }

            List<string> columns = snapshot.Columns
                .Cast<DataColumn>()
                .Select(column => column.ColumnName)
                .ToList();

            List<DataRow> orderedRows = snapshot.Rows
                .Cast<DataRow>()
                .OrderBy(row => Convert.ToInt32(row[examIdColumn], CultureInfo.InvariantCulture))
                .ToList();

            List<string> expectedSignatures = orderedRows
                .Select(row => BuildRowSignature(row, columns))
                .ToList();

            string columnList = string.Join(",", columns.Select(name => "[" + name + "]"));
            string parameterList = string.Join(",", columns.Select(_ => "?"));
            string insertSql = $"INSERT INTO Exam_EssInfo ({columnList}) VALUES ({parameterList})";

            session.BeginTransaction();
            try
            {
                session.Execute("DELETE FROM Exam_EssInfo");

                foreach (DataRow row in orderedRows)
                {
                    var args = new (string Name, object? Value)[columns.Count];
                    for (int index = 0; index < columns.Count; index++)
                    {
                        object? value = row[columns[index]];
                        args[index] = ("@p" + index, value is DBNull ? null : value);
                    }

                    session.Execute(insertSql, args);
                }

                DataTable verify = session.Query("SELECT * FROM Exam_EssInfo");
                List<string> actualSignatures = verify.Rows
                    .Cast<DataRow>()
                    .Select(row => BuildRowSignature(row, columns))
                    .ToList();

                if (actualSignatures.Count != expectedSignatures.Count)
                {
                    session.Rollback();
                    return new ExamPhysicalOrderResult
                    {
                        Success = false,
                        RowCount = actualSignatures.Count,
                        Message = $"重建后记录数不一致（应为 {expectedSignatures.Count} 条，实际 {actualSignatures.Count} 条），已整表回滚"
                    };
                }

                for (int index = 0; index < expectedSignatures.Count; index++)
                {
                    if (string.CompareOrdinal(expectedSignatures[index], actualSignatures[index]) != 0)
                    {
                        session.Rollback();
                        return new ExamPhysicalOrderResult
                        {
                            Success = false,
                            RowCount = actualSignatures.Count,
                            Message = $"重建后第 {index + 1} 条记录与原始内容不一致，已整表回滚"
                        };
                    }
                }

                List<int> actualIds = verify.Rows
                    .Cast<DataRow>()
                    .Select(row => Convert.ToInt32(row[examIdColumn], CultureInfo.InvariantCulture))
                    .ToList();
                if (!actualIds.SequenceEqual(actualIds.OrderBy(id => id)))
                {
                    session.Rollback();
                    return new ExamPhysicalOrderResult
                    {
                        Success = false,
                        RowCount = actualSignatures.Count,
                        Message = "重建后物理顺序仍不是 ExamID 升序（"
                                  + string.Join(",", actualIds) + "），已整表回滚"
                    };
                }

                session.Commit();
                return new ExamPhysicalOrderResult
                {
                    Success = true,
                    RowCount = actualSignatures.Count,
                    Message = $"<Exam_EssInfo>物理顺序已按 ExamID 升序重建，{actualSignatures.Count} 条记录逐列校验一致（"
                              + string.Join(",", actualIds) + "）"
                };
            }
            catch
            {
                session.Rollback();
                throw;
            }
        }
        catch (Exception ex)
        {
            return new ExamPhysicalOrderResult
            {
                Success = false,
                Message = "重建异常：" + ex.Message + "（已整表回滚）"
            };
        }
    }

    /// <summary>表里是否真的有这一列（用 WHERE 1=0 取空结果集的列集合，不依赖 ADOX）。</summary>
    private static bool HasColumn(AccessDb session, string table, string columnName)
    {
        DataTable probe = session.Query($"SELECT * FROM [{table}] WHERE 1=0");
        return FindColumn(probe, columnName) is not null;
    }

    /// <summary>DataTable 列名不区分大小写，这里返回表里真实存在的那个名字。</summary>
    private static string? FindColumn(DataTable table, string columnName)
    {
        foreach (DataColumn column in table.Columns)
        {
            if (string.Equals(column.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
            {
                return column.ColumnName;
            }
        }

        return null;
    }

    /// <summary>把一行拼成可比较的字符串（legacy BuildOrderRebuildRowSignature，Util.cs:4103-4115）。</summary>
    private static string BuildRowSignature(DataRow row, List<string> columnNames)
    {
        return string.Join(
            "\u0001",
            columnNames.Select(name =>
            {
                object value = row[name];
                return value is DBNull ? "<null>" : Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            }));
    }
}
