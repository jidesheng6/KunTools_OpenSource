using DocumentTools.Core.Data;
using DocumentTools.Core.Services.Exam;
using DocumentTools.ExamFixture;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// Mss3VoteService 对真实 .mdb 的读写。
/// 夹具表结构来自 Mss3VoteService.cs 与 legacy 的真实语句（见 tools\ExamFixture\ExamFixtureBuilder.cs 注释）。
/// </summary>
public sealed class Mss3VoteServiceAccessTests
{
    private readonly Mss3VoteService _sut = new();

    [AceFact]
    public void ListVotes_ReadsVoteEssInfoColumns()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        IReadOnlyList<Mss3VoteRow> votes = _sut.ListVotes(fixture.DatabaseFile);

        Assert.Equal(2, votes.Count);
        Mss3VoteRow first = Assert.Single(votes, row => row.VoteId == "V-001");
        Assert.Equal("测评票甲", first.VoteName);
        Assert.Equal("45", first.Threshold);
        Assert.Equal("方框", first.SelName);
        Mss3VoteRow second = Assert.Single(votes, row => row.VoteId == "V-002");
        Assert.Equal("测评票乙", second.VoteName);
        Assert.Equal("圆框", second.SelName);
    }

    [AceFact]
    public void InsertVotePage_NewVote_CreatesEssInfoRowAndPage()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        string image = ExamAccessTestKit.CreateImage(Path.Combine(fixture.DirectoryPath, "ballot.png"), 40, 20);

        _sut.InsertVotePage(fixture.DatabaseFile, image, "V-100", "新增票", "MC-1", 1, "圆框");

        using var session = fixture.OpenDatabase();
        List<Dictionary<string, object?>> ess = ExamAccessTestKit.Rows(
            session, "SELECT VoteID,VoteName,Threshold,HxrScore,LxrScore,OutsideThreshold,SelName FROM Vote_EssInfo WHERE VoteID='V-100'");
        Dictionary<string, object?> row = Assert.Single(ess);
        Assert.Equal("新增票", ExamAccessTestKit.Text(row, "VoteName"));
        Assert.Equal(45, Convert.ToInt32(row["Threshold"]));
        Assert.Equal(15, Convert.ToInt32(row["HxrScore"]));
        Assert.Equal(20, Convert.ToInt32(row["LxrScore"]));
        Assert.Equal(1, Convert.ToInt32(row["OutsideThreshold"]));
        Assert.Equal("圆框", ExamAccessTestKit.Text(row, "SelName"));

        List<Dictionary<string, object?>> pages = ExamAccessTestKit.Rows(
            session, "SELECT VoteID,PageIndex,MatrixCode,PicContent FROM Vote_PageInfo WHERE VoteID='V-100'");
        Dictionary<string, object?> page = Assert.Single(pages);
        Assert.Equal(1, Convert.ToInt32(page["PageIndex"]));
        Assert.Equal("MC-1", ExamAccessTestKit.Text(page, "MatrixCode"));

        // PicContent 存的是 EncodeGrayBmp 产出的 BMP 字节流，OLEOBJECT 列必须原样存住二进制。
        var picture = Assert.IsType<byte[]>(page["PicContent"]);
        Assert.True(picture.Length > 100, "选票图片字节数为 " + picture.Length);
        Assert.Equal((byte)'B', picture[0]);
        Assert.Equal((byte)'M', picture[1]);
        // 字节流必须是能被解码器读回的 BMP，且尺寸与原图一致（不能是空壳魔数）。
        using SKBitmap decoded = SKBitmap.Decode(picture)!;
        Assert.Equal(40, decoded.Width);
        Assert.Equal(20, decoded.Height);
    }

    [AceFact]
    public void InsertVotePage_ExistingVote_ReusesEssInfoAndKeepsVoteName()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        string image = ExamAccessTestKit.CreateImage(Path.Combine(fixture.DirectoryPath, "ballot.png"), 24, 24);

        _sut.InsertVotePage(fixture.DatabaseFile, image, "V-001", "不该出现的名字", "MC-2", 7, "方框");

        using var session = fixture.OpenDatabase();
        Assert.Equal(2, ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Vote_EssInfo"));
        Assert.Equal(
            "测评票甲",
            Convert.ToString(session.Scalar("SELECT VoteName FROM Vote_EssInfo WHERE VoteID='V-001'")));
        Assert.Equal(
            1,
            ExamAccessTestKit.Count(
                session, "SELECT COUNT(*) FROM Vote_PageInfo WHERE VoteID='V-001' AND PageIndex=7"));
    }

    [AceFact]
    public void InsertVotePage_SameVoteAndPageIndex_ReplacesInsteadOfDuplicating()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();
        string image = ExamAccessTestKit.CreateImage(Path.Combine(fixture.DirectoryPath, "ballot.png"), 32, 16);

        _sut.InsertVotePage(fixture.DatabaseFile, image, "V-200", "重复页", "MC-old", 3, "方框");
        _sut.InsertVotePage(fixture.DatabaseFile, image, "V-200", "重复页", "MC-new", 3, "方框");

        using var session = fixture.OpenDatabase();
        Assert.Equal(
            1,
            ExamAccessTestKit.Count(session, "SELECT COUNT(*) FROM Vote_PageInfo WHERE VoteID='V-200' AND PageIndex=3"));
        Assert.Equal(
            "MC-new",
            Convert.ToString(
                session.Scalar("SELECT MatrixCode FROM Vote_PageInfo WHERE VoteID='V-200' AND PageIndex=3")));
    }

    [AceFact]
    public void InsertVotePage_MissingImageFile_ThrowsFileNotFound()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        var ex = Assert.Throws<FileNotFoundException>(() => _sut.InsertVotePage(
            fixture.DatabaseFile,
            Path.Combine(fixture.DirectoryPath, "no-such.png"),
            "V-300",
            "缺图",
            "MC",
            1,
            "方框"));

        Assert.Contains("选票图片不存在", ex.Message);
    }

    [AceFact]
    public void SetItemColumnsToMemo_AltersAllFiveItemTablesAndAcceptsLongText()
    {
        using ExamFixtureSet fixture = ExamAccessTestKit.NewFixture();

        int executed = _sut.SetItemColumnsToMemo(fixture.DatabaseFile);

        Assert.Equal(ItemTables.Length, executed);

        // 断言"真的变成 Memo"的唯一可靠方式是写超长值再读回：TEXT(255) 会在写入阶段就失败或截断。
        string longText = new string('测', 400);
        using var session = fixture.OpenDatabase();
        foreach (string table in ItemTables)
        {
            session.Execute($"DELETE FROM {table}");
            session.Execute($"INSERT INTO {table} (Item) VALUES (?)", ("@p0", longText));
            string stored = Convert.ToString(session.Scalar($"SELECT Item FROM {table}")) ?? string.Empty;
            Assert.Equal(longText.Length, stored.Length);
        }
    }

    /// <summary>Mss3VoteService.ItemTableNames 是 private，这里按源码逐个写死，与实现保持一一对应。</summary>
    private static readonly string[] ItemTables =
    [
        "Page_SelectorInfo",
        "Report_Detail",
        "Result_DetailInfo",
        "Result_NumInfo",
        "TempResult_DetailInfo"
    ];
}
