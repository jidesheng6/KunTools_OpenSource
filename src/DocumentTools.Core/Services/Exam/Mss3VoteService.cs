using System.Data;
using System.Data.OleDb;
using DocumentTools.Core.Data;
using SkiaSharp;

namespace DocumentTools.Core.Services.Exam;

public sealed class Mss3VoteRow
{
    public string VoteId { get; set; } = string.Empty;
    public string VoteName { get; set; } = string.Empty;
    public string Threshold { get; set; } = string.Empty;
    public string SelName { get; set; } = string.Empty;
}

public interface IMss3VoteService
{
    IReadOnlyList<Mss3VoteRow> ListVotes(string databaseFile);

    /// <summary>Insert grayscale ballot page image into Vote_PageInfo (Util.InsertVotePicContent).</summary>
    void InsertVotePage(
        string databaseFile,
        string imageFile,
        string voteId,
        string voteName,
        string matrixCode,
        int pageIndex,
        string pageSquareType);

    int SetItemColumnsToMemo(string databaseFile);

    /// <summary>
    /// 恢复 Item 列的默认长度（legacy「恢复项目列字段结构长度」，BodyForm.cs:4417-4432）：
    /// Report_Detail / Result_NumInfo → TEXT(255)，Result_DetailInfo / TempResult_DetailInfo → TEXT(100)，
    /// 其余（Page_SelectorInfo）→ TEXT(50)。表不存在时跳过，返回成功执行的语句数。
    /// </summary>
    int ResetItemColumnLengths(string databaseFile);
}

public sealed class Mss3VoteService : IMss3VoteService
{
    /// <summary>选票方框类型（写库值 <c>Vote_EssInfo.SelName</c>，legacy「方框/圆框」单选组）。</summary>
    public const string SquareTypeSquare = "方框";

    public const string SquareTypeCircle = "圆框";

    /// <summary>方框类型下拉的全部取值。</summary>
    public static readonly IReadOnlyList<string> SquareTypes = [SquareTypeSquare, SquareTypeCircle];

    /// <summary>
    /// 选票页下拉：<c>Vote_PageInfo.PageIndex</c> 从 0 起，legacy 只提供「第1页/第2页」两页。
    /// 序号与显示文案的对应是库语义，视图不该自己写 0/1。
    /// </summary>
    public static readonly IReadOnlyList<(int Index, string Label)> PageChoices = [(0, "第1页"), (1, "第2页")];

    private static readonly string[] ItemTableNames =
    [
        "Page_SelectorInfo",
        "Report_Detail",
        "Result_DetailInfo",
        "Result_NumInfo",
        "TempResult_DetailInfo"
    ];

    /// <summary>ACE/Jet 的「找不到输入表」两类提示（中/英）——只有这类才归因成「选错文件」。</summary>
    private static bool IsNotVoteTemplate(OleDbException ex)
        => ex.Message.Contains("Vote_EssInfo", StringComparison.OrdinalIgnoreCase)
            || ex.Message.Contains("找不到输入表", StringComparison.Ordinal)
            || ex.Message.Contains("could not be found", StringComparison.OrdinalIgnoreCase);

    public IReadOnlyList<Mss3VoteRow> ListVotes(string databaseFile)
    {
        using var session = new AccessDb(databaseFile);
        DataTable table;
        try
        {
            table = session.Query(
                "SELECT VoteId, VoteName, Threshold, SelName FROM Vote_EssInfo");
        }
        catch (OleDbException ex) when (IsNotVoteTemplate(ex))
        {
            // 用户最容易选错成 Exam_Base.mdb：ACE 只会抛英文「找不到输入表或查询 Vote_EssInfo」，
            // 这里换成能照做的中文提示（选 .mss3 文件），原始异常留在 InnerException。
            throw new InvalidOperationException(
                $"这个文件不是 MSS3 选票模板（缺少表 Vote_EssInfo）：{Path.GetFileName(databaseFile)}。请选择 .mss3 文件。",
                ex);
        }

        var list = new List<Mss3VoteRow>();
        foreach (DataRow row in table.Rows)
        {
            list.Add(new Mss3VoteRow
            {
                VoteId = Convert.ToString(row["VoteId"]) ?? string.Empty,
                VoteName = Convert.ToString(row["VoteName"]) ?? string.Empty,
                Threshold = Convert.ToString(row["Threshold"]) ?? string.Empty,
                SelName = Convert.ToString(row["SelName"]) ?? string.Empty
            });
        }

        return list;
    }

    public void InsertVotePage(
        string databaseFile,
        string imageFile,
        string voteId,
        string voteName,
        string matrixCode,
        int pageIndex,
        string pageSquareType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(imageFile);
        ArgumentException.ThrowIfNullOrWhiteSpace(voteId);
        ArgumentException.ThrowIfNullOrWhiteSpace(voteName);
        if (!File.Exists(imageFile))
        {
            throw new FileNotFoundException("选票图片不存在。", imageFile);
        }

        byte[] pictureBytes = EncodeGrayBmp(imageFile);
        using var session = new AccessDb(databaseFile);

        int pageCount = Convert.ToInt32(
            session.Scalar(
                "SELECT COUNT(*) FROM Vote_PageInfo WHERE VoteID=? AND PageIndex=?",
                ("@p0", voteId),
                ("@p1", pageIndex)) ?? 0);
        if (pageCount > 0)
        {
            session.Execute(
                "DELETE FROM Vote_PageInfo WHERE VoteID=? AND PageIndex=?",
                ("@p0", voteId),
                ("@p1", pageIndex));
        }

        int essCount = Convert.ToInt32(
            session.Scalar("SELECT COUNT(*) FROM Vote_EssInfo WHERE VoteID=?", ("@p0", voteId)) ?? 0);
        if (essCount == 0)
        {
            session.Execute(
                "INSERT INTO Vote_EssInfo (VoteID,VoteName,Threshold,HxrScore,LxrScore,OutsideThreshold,SelName,HxrWidth,HxrHeight,LxrWidth,LxrHeight) VALUES (?,?,?,?,?,?,?,?,?,?,?)",
                ("@p0", voteId),
                ("@p1", voteName),
                ("@p2", 45),
                ("@p3", 15),
                ("@p4", 20),
                ("@p5", 1),
                ("@p6", string.IsNullOrWhiteSpace(pageSquareType) ? SquareTypeSquare : pageSquareType),
                ("@p7", 42),
                ("@p8", 42),
                ("@p9", 150),
                ("@p10", 45));
        }

        session.Execute(
            "INSERT INTO Vote_PageInfo (VoteID,PageIndex,MatrixCode,PicContent) VALUES (?,?,?,?)",
            ("@p0", voteId),
            ("@p1", pageIndex),
            ("@p2", matrixCode ?? string.Empty),
            ("@p3", pictureBytes));
    }

    public int SetItemColumnsToMemo(string databaseFile)
    {
        int executed = 0;
        using var session = new AccessDb(databaseFile);
        foreach (string table in ItemTableNames)
        {
            try
            {
                session.Execute($"ALTER TABLE {table} ALTER COLUMN Item Memo");
                executed++;
            }
            catch
            {
                // table may not exist in all MSS3 variants
            }
        }

        return executed;
    }

    public int ResetItemColumnLengths(string databaseFile)
    {
        int executed = 0;
        using var session = new AccessDb(databaseFile);
        foreach (string table in ItemTableNames)
        {
            string length = table switch
            {
                "Report_Detail" or "Result_NumInfo" => "255",
                "Result_DetailInfo" or "TempResult_DetailInfo" => "100",
                _ => "50"
            };

            try
            {
                session.Execute($"ALTER TABLE {table} ALTER COLUMN Item TEXT({length})");
                executed++;
            }
            catch
            {
                // table may not exist in all MSS3 variants
            }
        }

        return executed;
    }

    private static byte[] EncodeGrayBmp(string imageFile)
    {
        using SKBitmap? src = SKBitmap.Decode(imageFile)
                             ?? throw new InvalidOperationException("无法解码选票图片：" + imageFile);
        using var gray = new SKBitmap(src.Width, src.Height, SKColorType.Gray8, SKAlphaType.Opaque);
        using (var canvas = new SKCanvas(gray))
        {
            using var paint = new SKPaint { ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            {
                0.2126f, 0.7152f, 0.0722f, 0, 0,
                0.2126f, 0.7152f, 0.0722f, 0, 0,
                0.2126f, 0.7152f, 0.0722f, 0, 0,
                0, 0, 0, 1, 0
            }) };
            canvas.DrawBitmap(src, 0, 0, paint);
        }

        // SkiaSharp 没有 BMP 编码器（Encode 返回 null），BMP 由 BmpEncoder 手写，
        // 布局与 legacy 的 8bpp 灰度索引位图一致。
        return BmpEncoder.EncodeGray8(gray);
    }
}
