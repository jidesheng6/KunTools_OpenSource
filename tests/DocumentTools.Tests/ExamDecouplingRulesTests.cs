using DocumentTools.Core.Services.Exam;

namespace DocumentTools.Tests;

/// <summary>
/// 「视图里没有业务知识」的规则单测：每个用例对应一条原先写在
/// <c>src\DocumentTools.App\Features\Modules\Exam\*View.cs</c> 里的业务规则，
/// 现在规则本体在 Core，视图只摆控件。判例表见 <c>docs\decoupling-audit.md</c>。
/// 全部是纯函数，不碰 ACE。
/// </summary>
public sealed class ExamDecouplingRulesTests
{
    // ── ExamInsertView:631 SplitNames ────────────────────────────────────────

    [Theory]
    [InlineData("同意,不同意,弃权", 3)]
    [InlineData("同意，不同意，弃权", 3)]
    [InlineData("同意;不同意;弃权", 3)]
    [InlineData("同意；不同意；弃权", 3)]
    [InlineData("同意|不同意|弃权", 3)]
    [InlineData(" 同意 , , 不同意 ", 2)]
    public void ParseSelectorList_AcceptsAllLegacySeparators(string raw, int expected)
    {
        Assert.Equal(expected, ExamInsertRules.ParseSelectorList(raw).Count);
    }

    [Fact]
    public void ParseSelectorList_TrimsAndDropsEmptyEntries()
    {
        Assert.Equal(
            new[] { "同意", "不同意", "弃权" },
            ExamInsertRules.ParseSelectorList(" 同意 ,，;| 不同意 ； 弃权 "));
    }

    [Fact]
    public void ParseSelectorList_Blank_ReturnsEmpty()
    {
        Assert.Empty(ExamInsertRules.ParseSelectorList(null));
        Assert.Empty(ExamInsertRules.ParseSelectorList("   "));
    }

    // ── ExamInsertView:21 TypeModes（写库字面值）──────────────────────────────

    [Fact]
    public void TypeModes_AreTheLegacyWriteValues_InDropDownOrder()
    {
        Assert.Equal(
            new[] { "人员纵排", "人员纵排2", "人员横排", "单人标识" },
            ExamInsertRules.TypeModes);
        Assert.Equal("人员纵排", ExamInsertRules.PeopleVerticalTypeMode);
        Assert.Equal("人员纵排2", ExamInsertRules.PeopleVertical2TypeMode);
        Assert.Equal("人员横排", ExamInsertRules.PeopleHorizontalTypeMode);
        Assert.Equal("单人标识", ExamInsertRules.SingleMarkTypeMode);
    }

    [Fact]
    public void TypeModeIndex_MapsStoredValueToDropDownIndex()
    {
        Assert.Equal(0, ExamInsertRules.TypeModeIndex("人员纵排"));
        Assert.Equal(2, ExamInsertRules.TypeModeIndex(" 人员横排 "));
        Assert.Equal(-1, ExamInsertRules.TypeModeIndex("未知模式"));
        Assert.Equal(-1, ExamInsertRules.TypeModeIndex(null));
    }

    // ── ExamMss3View:42/46 方框/圆框 与 第N页 ─────────────────────────────────

    [Fact]
    public void Mss3_SquareTypes_ListsTheTwoAcceptedValues()
    {
        Assert.Equal(new[] { "方框", "圆框" }, Mss3VoteService.SquareTypes);
        Assert.Equal("方框", Mss3VoteService.SquareTypeSquare);
        Assert.Equal("圆框", Mss3VoteService.SquareTypeCircle);
    }

    [Fact]
    public void Mss3_PageChoices_AreZeroBasedPageIndexes()
    {
        // Vote_PageInfo.PageIndex 存 0/1，文案「第N页」= N-1。
        Assert.Equal(new[] { (0, "第1页"), (1, "第2页") }, Mss3VoteService.PageChoices);
    }

    // ── ExamReportView:104/171/175/414 身份组合、行/列模式、小数位数、所有项目 ──

    [Fact]
    public void IsMultiRaidValue_DetectsPipeJoinedIdentities()
    {
        Assert.True(ExamReportService.IsMultiRaidValue("甲|乙|合计"));
        Assert.False(ExamReportService.IsMultiRaidValue("甲"));
        Assert.False(ExamReportService.IsMultiRaidValue(""));
        Assert.False(ExamReportService.IsMultiRaidValue(null));
    }

    [Fact]
    public void ModeOptions_RowModeFirst_AndIsRowModeFollowsIt()
    {
        Assert.Equal(new[] { "行模式", "列模式" }, ExamReportService.ModeOptions);
        Assert.True(ExamReportService.IsRowMode(0));
        Assert.False(ExamReportService.IsRowMode(1));
        // SelectedIndex = -1（未选中）按首项「行模式」处理，与原先 `!= 1` 一致。
        Assert.True(ExamReportService.IsRowMode(-1));
    }

    [Fact]
    public void NumberFormatAt_MapsDropDownIndexToEnum_AndClampsOutOfRange()
    {
        Assert.Equal(ExamReportNumberFormat.Integer, ExamReportService.NumberFormatAt(0));
        Assert.Equal(ExamReportNumberFormat.TwoDecimals, ExamReportService.NumberFormatAt(2));
        Assert.Equal(ExamReportNumberFormat.PercentTwoDecimals, ExamReportService.NumberFormatAt(5));
        Assert.Equal(ExamReportNumberFormat.Integer, ExamReportService.NumberFormatAt(-1));
        Assert.Equal(ExamReportNumberFormat.PercentTwoDecimals, ExamReportService.NumberFormatAt(99));
        Assert.Equal(6, ExamReportService.NumberFormatOptions.Count);
    }

    [Fact]
    public void AllItemsName_IsTheSentinelCoreRecognises()
    {
        Assert.Equal("所有项目", ExamReportService.AllItemsName);
    }

    // ── ExamVoteView:151/191-194/164 名单换行、默认列名、默认文件名 ─────────────

    [Fact]
    public void SplitCandidateNames_OnePerLine_DropsBlankLines()
    {
        Assert.Equal(
            new[] { "张三", "李四", "王五" },
            VoteReportRules.SplitCandidateNames("张三\r\n  李四 \n\n王五\r"));
        Assert.Empty(VoteReportRules.SplitCandidateNames(null));
        Assert.Empty(VoteReportRules.SplitCandidateNames("  \r\n "));
    }

    [Fact]
    public void VoteReport_DefaultColumnsAndFileName_MatchTheTemplateHeaders()
    {
        var request = new VoteReportRequest();

        Assert.Equal("姓名", request.NameColumn);
        Assert.Equal("同意", request.AgreeColumn);
        Assert.Equal("不同意", request.UnAgreeColumn);
        Assert.Equal("弃权", request.AbstainColumn);
        Assert.Equal(VoteReportRules.DefaultNameColumn, request.NameColumn);
        Assert.Equal(VoteReportRules.DefaultAgreeColumn, request.AgreeColumn);
        Assert.Equal(VoteReportRules.DefaultUnAgreeColumn, request.UnAgreeColumn);
        Assert.Equal(VoteReportRules.DefaultAbstainColumn, request.AbstainColumn);
        Assert.Equal("选举结果报告单.docx", VoteReportRules.DefaultOutputFileName);
    }

    // ── ExamSingleView:183 Alt 文本 @ 分隔 ────────────────────────────────────

    [Fact]
    public void ParseAltTexts_SplitsOnAtSign_AndTrims()
    {
        Assert.Equal(new[] { "图1", "图2", "图3" }, SingleDocumentNaming.ParseAltTexts("图1@ 图2 @图3"));
        Assert.Equal('@', SingleDocumentNaming.AltTextSeparator);
        Assert.Empty(SingleDocumentNaming.ParseAltTexts(""));
        Assert.Empty(SingleDocumentNaming.ParseAltTexts(null));
    }

    // ── ExamCircleView:647 UniqueSibling + _fixed/_r90/_auto90 命名规则 ────────

    [Fact]
    public void ExamCircleNaming_Suffixes_MatchTheLegacySaveNames()
    {
        Assert.Equal("_fixed", ExamCircleNaming.FixedSuffix);
        Assert.Equal("_r90", ExamCircleNaming.RotationSuffix(90));
        Assert.Equal("_auto180", ExamCircleNaming.AutoRotationSuffix(180));
    }

    [Fact]
    public void ExamCircleNaming_FixedFileName_KeepsStemAndExtension()
    {
        string source = Path.Combine(Path.GetTempPath(), "选票扫描.tif");

        Assert.Equal("选票扫描_fixed.tif", ExamCircleNaming.FixedFileName(source));
    }

    // ── ExamPackageView:87/92/176/212/218/281 数据包布局 ───────────────────────

    [Fact]
    public void PackageDirectory_IsRecognisedByTheBaseDatabase()
    {
        var sut = new ExamPackageService();
        string root = Path.Combine(Path.GetTempPath(), "dt-pkg-layout-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(root);
        try
        {
            Assert.False(sut.IsPackageDirectory(root));
            Assert.Equal("不是考评数据包（缺少 Exam_Base.mdb）", sut.Describe(root));

            // 只判存在性：IsPackageDirectory / Describe 的非数据包分支不打开 ACE。
            File.WriteAllBytes(Path.Combine(root, "Exam_Base.mdb"), []);
            Assert.True(sut.IsPackageDirectory(root));

            Assert.Equal("数据包目录未选择", sut.Describe("  "));
            Assert.StartsWith("数据包目录不存在：", sut.Describe(Path.Combine(root, "nope")), StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
