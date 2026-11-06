using DocumentTools.Core.Services.Text;

namespace DocumentTools.Tests;

public class MoneyChineseServiceTests
{
    private readonly MoneyChineseService _sut = new();

    [Fact]
    public void Zero_IsLingYuanZheng()
    {
        Assert.Equal("零元整", _sut.ToChineseUpper(0m));
    }

    [Fact]
    public void SimpleAmount_Works()
    {
        string text = _sut.ToChineseUpper(100m);
        Assert.Contains("佰", text);
        Assert.Contains("元", text);
    }

    [Fact]
    public void InvalidText_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, _sut.ToChineseUpper("abc"));
        Assert.Equal(string.Empty, _sut.ToChineseUpper("-1"));
    }
}
