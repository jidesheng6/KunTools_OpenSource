using DocumentTools.Core.Services.Scan;

namespace DocumentTools.Tests;

public sealed class TwainCapabilityApplicatorTests
{
    [Fact]
    public void Apply_RequestedCapabilities_AreEnabled()
    {
        var target = new FakeTarget(canSetDuplex: true, canSetBlankPageDiscard: true);

        TwainCapabilityApplicator.Apply(target, new TwainScanOptions
        {
            Duplex = true,
            SkipBlankPages = true
        });

        Assert.True(target.Duplex);
        Assert.True(target.BlankPageDiscard);
    }

    [Fact]
    public void Apply_DisabledCapabilities_AreExplicitlyReset()
    {
        var target = new FakeTarget(canSetDuplex: true, canSetBlankPageDiscard: true)
        {
            Duplex = true,
            BlankPageDiscard = true
        };

        TwainCapabilityApplicator.Apply(target, new TwainScanOptions
        {
            Duplex = false,
            SkipBlankPages = false
        });

        Assert.False(target.Duplex);
        Assert.False(target.BlankPageDiscard);
        Assert.Equal(1, target.DuplexWrites);
        Assert.Equal(1, target.BlankPageWrites);
    }

    [Fact]
    public void Apply_UnsupportedDuplex_ExplainsHowToContinue()
    {
        var target = new FakeTarget(canSetDuplex: false, canSetBlankPageDiscard: true);

        NotSupportedException error = Assert.Throws<NotSupportedException>(() =>
            TwainCapabilityApplicator.Apply(target, new TwainScanOptions { Duplex = true }));

        Assert.Contains("不支持双面扫描", error.Message);
        Assert.Contains("取消勾选", error.Message);
        Assert.Equal(0, target.DuplexWrites);
    }

    [Fact]
    public void Apply_UnsupportedBlankPageDiscard_ExplainsHowToContinue()
    {
        var target = new FakeTarget(canSetDuplex: true, canSetBlankPageDiscard: false);

        NotSupportedException error = Assert.Throws<NotSupportedException>(() =>
            TwainCapabilityApplicator.Apply(target, new TwainScanOptions { SkipBlankPages = true }));

        Assert.Contains("不支持自动丢弃空白页", error.Message);
        Assert.Contains("取消勾选", error.Message);
        Assert.Equal(0, target.BlankPageWrites);
    }

    [Fact]
    public void Apply_UnsupportedButNotRequested_DoesNotFail()
    {
        var target = new FakeTarget(canSetDuplex: false, canSetBlankPageDiscard: false);

        TwainCapabilityApplicator.Apply(target, new TwainScanOptions());

        Assert.Equal(0, target.DuplexWrites);
        Assert.Equal(0, target.BlankPageWrites);
    }

    private sealed class FakeTarget(bool canSetDuplex, bool canSetBlankPageDiscard) : ITwainCapabilityTarget
    {
        public bool CanSetDuplex { get; } = canSetDuplex;

        public bool CanSetBlankPageDiscard { get; } = canSetBlankPageDiscard;

        public bool Duplex { get; set; }

        public bool BlankPageDiscard { get; set; }

        public int DuplexWrites { get; private set; }

        public int BlankPageWrites { get; private set; }

        public void SetDuplex(bool enabled)
        {
            DuplexWrites++;
            Duplex = enabled;
        }

        public void SetBlankPageDiscard(bool enabled)
        {
            BlankPageWrites++;
            BlankPageDiscard = enabled;
        }
    }
}
