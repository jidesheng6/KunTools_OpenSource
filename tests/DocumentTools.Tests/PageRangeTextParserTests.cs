using DocumentTools.Core.Common;
using DocumentTools.Core.Services.Office;

namespace DocumentTools.Tests;

public class PageRangeTextParserTests
{
    [Fact]
    public void Parse_SingleAndRanges_Works()
    {
        IReadOnlyList<PdfPageRange> ranges = PageRangeTextParser.Parse("1-3,5,8-10");
        Assert.Equal(3, ranges.Count);
        Assert.Equal(1, ranges[0].FirstPage);
        Assert.Equal(3, ranges[0].LastPage);
        Assert.Equal(5, ranges[1].FirstPage);
        Assert.Equal(5, ranges[1].LastPage);
        Assert.Equal(8, ranges[2].FirstPage);
        Assert.Equal(10, ranges[2].LastPage);
    }

    [Fact]
    public void Parse_ChineseDaoSeparator_Works()
    {
        IReadOnlyList<PdfPageRange> ranges = PageRangeTextParser.Parse("2到4");
        Assert.Single(ranges);
        Assert.Equal(new PdfPageRange(2, 4).ToString(), ranges[0].ToString());
    }

    [Fact]
    public void Parse_Empty_ReturnsEmpty()
    {
        Assert.Empty(PageRangeTextParser.Parse("  "));
        Assert.Empty(PageRangeTextParser.Parse(null));
    }

    [Fact]
    public void Parse_Invalid_Throws()
    {
        Assert.Throws<ArgumentException>(() => PageRangeTextParser.Parse("abc"));
    }
}
