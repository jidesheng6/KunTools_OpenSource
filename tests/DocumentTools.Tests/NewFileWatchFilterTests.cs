using DocumentTools.Core.Services.System;

namespace DocumentTools.Tests;

/// <summary>
/// 「路径监视」的过滤与延迟规则，对应 legacy TabPage_36 groupBox69
/// （<c>DocumentTools/BodyForm.cs:7725-7791</c>、<c>BodyForm.Designer.cs:14111-14137</c>）。
/// </summary>
public sealed class NewFileWatchFilterTests
{
    [Theory]
    [InlineData("a.jpg")]
    [InlineData("a.JPG")]
    [InlineData("扫描件.tif")]
    [InlineData("扫描件.TIFF")]
    [InlineData("b.bmp")]
    public void IsTargetFile_AcceptsLegacyTargets(string name)
    {
        Assert.True(NewFileWatchFilter.IsTargetFile(name));
    }

    [Theory]
    [InlineData("a.jpg.err")]
    [InlineData("a.jpg.han")]
    [InlineData("a.tif.err")]
    [InlineData("a.TIF.HAN")]
    [InlineData("a.png")]
    [InlineData("a.txt")]
    [InlineData("a")]
    [InlineData("")]
    [InlineData(null)]
    public void IsTargetFile_RejectsEngineArtifactsAndOtherExtensions(string? name)
    {
        Assert.False(NewFileWatchFilter.IsTargetFile(name));
    }

    [Fact]
    public void IgnoredArtifactsAreAlsoOutsideTargets()
    {
        // legacy 的忽略分支因为 GetExtension(".jpg.err") 只返回 ".err" 而永远不命中；
        // 行为等价的前提是这些产物本来就不在目标扩展名里。
        foreach (string ignored in NewFileWatchFilter.IgnoredExtensions)
        {
            string name = "x" + ignored;
            Assert.False(NewFileWatchFilter.IsTargetFile(name));
        }
    }

    [Theory]
    [InlineData(null, 500)]
    [InlineData(100, 100)]
    [InlineData(500, 500)]
    [InlineData(0, 100)]
    [InlineData(-5, 100)]
    [InlineData(9_999_999, NewFileWatchFilter.MaxDelayMilliseconds)]
    public void NormalizeDelay_ClampsToLegacyNumericRange(int? input, int expected)
    {
        Assert.Equal(expected, NewFileWatchFilter.NormalizeDelay(input));
    }

    [Theory]
    [InlineData("500", 500)]
    [InlineData(" 1000 ", 1000)]
    [InlineData("", 500)]
    [InlineData("abc", 500)]
    [InlineData("50", 100)]
    public void ParseDelay_FallsBackToDefault(string? text, int expected)
    {
        Assert.Equal(expected, NewFileWatchFilter.ParseDelay(text));
    }

    // 静默纠正会让用户以为设置生效：空白 = 用默认（不算错误），非数字 = 必须能被调用方察觉到。
    [Theory]
    [InlineData("500", true, 500)]
    [InlineData(" 1000 ", true, 1000)]
    [InlineData("50", true, 100)]
    [InlineData("", true, 500)]
    [InlineData("   ", true, 500)]
    [InlineData(null, true, 500)]
    [InlineData("abc", false, 500)]
    [InlineData("5abc", false, 500)]
    [InlineData("1.5", false, 500)]
    public void TryParseDelay_ReportsNonNumericInsteadOfSilentlyDefaulting(string? text, bool expectedOk, int expectedDelay)
    {
        bool ok = NewFileWatchFilter.TryParseDelay(text, out int delay);

        Assert.Equal(expectedOk, ok);
        Assert.Equal(expectedDelay, delay);
    }
}
