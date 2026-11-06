using DocumentTools.Core.Services.System;

namespace DocumentTools.Tests;

/// <summary>
/// 网卡静态 IP 参数校验。legacy 用 Win32 SysIPAddress32 控件拦住非法输入
/// （<c>DocumentTools/IPAddressInputTextBox.cs</c>），新版是普通 TextBox，所以校验必须落在服务层。
/// </summary>
public sealed class NetworkAdapterValidationTests
{
    [Theory]
    [InlineData("192.168.1.10", 0xC0A8010Au)]
    [InlineData("0.0.0.0", 0u)]
    [InlineData("255.255.255.255", 0xFFFFFFFFu)]
    [InlineData(" 10.1.2.3 ", 0x0A010203u)]
    public void TryParseIPv4_AcceptsFourOctets(string text, uint expected)
    {
        Assert.True(NetworkAddressValidator.TryParseIPv4(text, out uint value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("192.168.1")]
    [InlineData("192.168.1.1.1")]
    [InlineData("256.1.1.1")]
    [InlineData("192.168.1.-1")]
    [InlineData("192.168.1.a")]
    [InlineData("192.168.1.")]
    [InlineData(".192.168.1.1")]
    [InlineData("192.168.01.1")]
    [InlineData("192.168.1.1234")]
    public void TryParseIPv4_RejectsMalformed(string text)
    {
        Assert.False(NetworkAddressValidator.TryParseIPv4(text, out _));
    }

    [Theory]
    [InlineData("255.255.255.0", 24)]
    [InlineData("255.255.252.0", 22)]
    [InlineData("255.0.0.0", 8)]
    [InlineData("255.255.255.255", 32)]
    [InlineData("0.0.0.0", 0)]
    public void Mask_AcceptsContiguous(string mask, int expectedPrefix)
    {
        Assert.True(NetworkAddressValidator.TryParseIPv4(mask, out uint value));
        Assert.True(NetworkAddressValidator.IsContiguousMask(value));
        Assert.Equal(expectedPrefix, NetworkAddressValidator.MaskPrefixLength(value));
    }

    [Theory]
    [InlineData("255.0.255.0")]
    [InlineData("255.255.254.1")]
    [InlineData("0.255.255.255")]
    public void Mask_RejectsNonContiguous(string mask)
    {
        Assert.True(NetworkAddressValidator.TryParseIPv4(mask, out uint value));
        Assert.False(NetworkAddressValidator.IsContiguousMask(value));
        Assert.Equal(-1, NetworkAddressValidator.MaskPrefixLength(value));
    }

    [Fact]
    public void ValidateStatic_AcceptsTypicalLanConfiguration()
    {
        NetworkAddressValidator.ValidateStaticArguments("192.168.1.10", "255.255.255.0", "192.168.1.1", "223.5.5.5", "8.8.8.8");
    }

    [Fact]
    public void ValidateStatic_AllowsEmptyGatewayAndDns()
    {
        NetworkAddressValidator.ValidateStaticArguments("10.0.0.5", "255.255.0.0", null, null, "  ");
    }

    [Fact]
    public void ValidateStatic_RejectsEmptyIpOrMask()
    {
        ArgumentException ip = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments(" ", "255.255.255.0", null, null, null));
        Assert.Contains("不能为空", ip.Message, StringComparison.Ordinal);

        ArgumentException mask = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments("192.168.1.10", "", null, null, null));
        Assert.Contains("不能为空", mask.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateStatic_RejectsBadIpWithActionableMessage()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments("999.1.1.1", "255.255.255.0", null, null, null));
        Assert.Contains("IP 地址格式不正确", ex.Message, StringComparison.Ordinal);
        Assert.Contains("999.1.1.1", ex.Message, StringComparison.Ordinal);
        Assert.Contains("192.168.1.10", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateStatic_RejectsNonContiguousMask()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments("192.168.1.10", "255.0.255.0", null, null, null));
        Assert.Contains("子网掩码格式不正确", ex.Message, StringComparison.Ordinal);
        Assert.Contains("255.255.255.0", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateStatic_RejectsZeroMask()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments("192.168.1.10", "0.0.0.0", null, null, null));
        Assert.Contains("子网掩码不能是 0.0.0.0", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ValidateStatic_RejectsBadGatewayAndDns()
    {
        ArgumentException gateway = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments("192.168.1.10", "255.255.255.0", "192.168.1", null, null));
        Assert.Contains("网关格式不正确", gateway.Message, StringComparison.Ordinal);

        ArgumentException dns = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments("192.168.1.10", "255.255.255.0", null, "223.5.5", null));
        Assert.Contains("DNS 格式不正确", dns.Message, StringComparison.Ordinal);

        ArgumentException dns2 = Assert.Throws<ArgumentException>(
            () => NetworkAddressValidator.ValidateStaticArguments("192.168.1.10", "255.255.255.0", null, null, "8.8.8.8.8"));
        Assert.Contains("DNS 格式不正确", dns2.Message, StringComparison.Ordinal);
    }
}
