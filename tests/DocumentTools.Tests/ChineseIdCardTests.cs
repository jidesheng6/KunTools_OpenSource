using DocumentTools.Core.Services.Text;

namespace DocumentTools.Tests;

public class ChineseIdCardTests
{
    [Theory]
    [InlineData("11010519491231002X", true)]
    [InlineData("110105194912310021", false)]
    [InlineData("", false)]
    [InlineData("123", false)]
    public void IsValid_Works(string id, bool expected)
    {
        // 11010519491231002X is a well-known checksum example used in many docs.
        Assert.Equal(expected, ChineseIdCard.IsValid(id));
    }
}
