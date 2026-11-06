using DocumentTools.Core.Services.Exam;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// 圆圈修复：模板匹配（NCC + NMS）与替换写回。用 SkiaSharp 现场造图，结果完全确定，
/// 不依赖外部素材，也不依赖 ACE / Aspose。
/// </summary>
public sealed class ExamCircleFixServiceTests : IDisposable
{
    private const int PatternSize = 24;

    private readonly ExamCircleFixService _sut = new();
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "dt-circlefix-" + Guid.NewGuid().ToString("n"));

    public ExamCircleFixServiceTests() => Directory.CreateDirectory(_dir);

    public void Dispose()
    {
        try
        {
            Directory.Delete(_dir, recursive: true);
        }
        catch (IOException)
        {
            // 临时目录清理失败不影响断言结果
        }
    }

    [Fact]
    public void Match_FindsEveryIdenticalPattern()
    {
        (int X, int Y)[] spots = [(30, 30), (150, 60), (220, 140)];
        string imagePath = CreatePage("page.png", 300, 200, spots);

        var matches = _sut.Match(
            imagePath,
            new ExamCircleRect { Left = spots[0].X, Top = spots[0].Y, Width = PatternSize, Height = PatternSize },
            similarityThreshold: 0.9,
            maxMatches: 50);

        Assert.Equal(spots.Length, matches.Count);
        Assert.All(matches, m => Assert.InRange(m.Similarity, 0.999, 1.0));
        Assert.True(matches[0].Similarity >= matches[^1].Similarity, "匹配结果应按相似度降序");
        foreach ((int x, int y) in spots)
        {
            Assert.Contains(matches, m => Math.Abs(m.Left - x) <= 1 && Math.Abs(m.Top - y) <= 1);
        }
    }

    [Fact]
    public void Match_SearchRectLimitsResult()
    {
        (int X, int Y)[] spots = [(30, 30), (150, 60), (220, 140)];
        string imagePath = CreatePage("page.png", 300, 200, spots);

        var matches = _sut.Match(
            imagePath,
            new ExamCircleRect { Left = spots[1].X, Top = spots[1].Y, Width = PatternSize, Height = PatternSize },
            searchRect: new ExamCircleRect { Left = 140, Top = 50, Width = 40, Height = 40 },
            similarityThreshold: 0.9,
            maxMatches: 50);

        Assert.Single(matches);
        Assert.InRange(matches[0].Left, spots[1].X - 1, spots[1].X + 1);
    }

    [Fact]
    public void Match_MaxMatchesTruncates()
    {
        string imagePath = CreatePage("page.png", 300, 200, [(30, 30), (150, 60), (220, 140)]);

        var matches = _sut.Match(
            imagePath,
            new ExamCircleRect { Left = 30, Top = 30, Width = PatternSize, Height = PatternSize },
            similarityThreshold: 0.9,
            maxMatches: 2);

        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void Match_FlatTemplateReturnsEmpty()
    {
        string imagePath = CreatePage("page.png", 120, 120, [(40, 40)]);

        var matches = _sut.Match(
            imagePath,
            new ExamCircleRect { Left = 10, Top = 10, Width = 16, Height = 16 },
            similarityThreshold: 0.5,
            maxMatches: 10);

        Assert.Empty(matches);
    }

    [Fact]
    public void Match_MissingFileThrows()
    {
        Assert.Throws<FileNotFoundException>(() =>
            _sut.Match(
                Path.Combine(_dir, "nope.png"),
                new ExamCircleRect { Left = 0, Top = 0, Width = 8, Height = 8 }));
    }

    [Fact]
    public void Match_OutOfBoundsTemplateThrows()
    {
        string imagePath = CreatePage("page.png", 100, 100, [(30, 30)]);

        Assert.Throws<ArgumentException>(() =>
            _sut.Match(
                imagePath,
                new ExamCircleRect { Left = 95, Top = 95, Width = 40, Height = 40 }));
    }

    [Fact]
    public void Fix_WritesFixedCopyAndKeepsSource()
    {
        string imagePath = CreatePage("page.png", 200, 160, [(60, 50)]);
        byte[] before = File.ReadAllBytes(imagePath);

        string output = _sut.Fix(new ExamCircleFixRequest
        {
            SourceImage = imagePath,
            TemplateRect = new ExamCircleRect { Left = 60, Top = 50, Width = PatternSize, Height = PatternSize }
        });

        Assert.True(File.Exists(output));
        Assert.Equal("page_fixed.png", Path.GetFileName(output));
        Assert.Equal(before, File.ReadAllBytes(imagePath));
        Assert.Equal((200, 160), _sut.GetImageSize(output));
    }

    [Fact]
    public void Fix_UsesManualMatchesAndReplacementImage()
    {
        string imagePath = CreatePage("page.png", 200, 160, [(60, 50)]);
        string replacement = Path.Combine(_dir, "mark.png");
        using (var bmp = new SKBitmap(8, 8))
        {
            using var canvas = new SKCanvas(bmp);
            canvas.Clear(new SKColor(0, 200, 0));
            using var image = SKImage.FromBitmap(bmp);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using FileStream fs = File.Create(replacement);
            data.SaveTo(fs);
        }

        string output = _sut.Fix(new ExamCircleFixRequest
        {
            SourceImage = imagePath,
            TemplateRect = new ExamCircleRect { Left = 60, Top = 50, Width = PatternSize, Height = PatternSize },
            ReplacementImage = replacement,
            ManualMatches =
            [
                new ExamCircleMatch { Left = 120, Top = 100, Width = PatternSize, Height = PatternSize, Similarity = 1 }
            ]
        });

        using SKBitmap result = SKBitmap.Decode(output);
        SKColor painted = result.GetPixel(120 + (PatternSize / 2), 100 + (PatternSize / 2));
        Assert.True(painted.Green > 150 && painted.Red < 80, $"替换区域应为绿色，实际 {painted}");
    }

    [Fact]
    public void Fix_RejectsSamePath()
    {
        string imagePath = CreatePage("page.png", 200, 160, [(60, 50)]);

        Assert.Throws<ArgumentException>(() => _sut.Fix(new ExamCircleFixRequest
        {
            SourceImage = imagePath,
            TemplateRect = new ExamCircleRect { Left = 60, Top = 50, Width = PatternSize, Height = PatternSize },
            OutputFile = imagePath
        }));
    }

    [Fact]
    public void GetImageSize_ReadsPixelSize()
    {
        string imagePath = CreatePage("page.png", 137, 89, [(20, 20)]);

        (int width, int height) = _sut.GetImageSize(imagePath);

        Assert.Equal(137, width);
        Assert.Equal(89, height);
    }

    /// <summary>白底页面 + 若干带白色缺口的方块图案（模板有明确纹理，方差足够）。</summary>
    private string CreatePage(string fileName, int width, int height, (int X, int Y)[] spots)
    {
        string path = Path.Combine(_dir, fileName);
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            foreach ((int x, int y) in spots)
            {
                DrawPattern(canvas, x, y);
            }
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using FileStream stream = File.Create(path);
        data.SaveTo(stream);
        return path;
    }

    private static void DrawPattern(SKCanvas canvas, int left, int top)
    {
        using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = false };
        canvas.DrawRect(SKRect.Create(left, top, PatternSize, PatternSize), paint);
        paint.Color = SKColors.White;
        canvas.DrawRect(SKRect.Create(left + 4, top + 10, PatternSize - 8, 4), paint);
        paint.Color = new SKColor(120, 120, 120);
        canvas.DrawRect(SKRect.Create(left + 2, top + 2, 5, 5), paint);
    }
}
