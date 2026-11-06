using DocumentTools.Core.Services.Code;
using DocumentTools.Core.Services.Exam;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// 「自动旋转」的象限判据（legacy BodyForm.cs:322-400）：
/// T 码在右上＝无需旋转；左上＝顺时针 90；左下＝180；右下＝逆时针 90。
/// 用例是真条码：先用 <see cref="IBarcodeService"/> 生成 DataMatrix，再贴到目标象限。
/// </summary>
public sealed class ExamCircleOrientationTests : IDisposable
{
    private const int CanvasSize = 400;

    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-orient-" + Guid.NewGuid().ToString("n"));
    private readonly IBarcodeService _barcodes = new BarcodeService();

    public ExamCircleOrientationTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void Detect_CodeInTopRight_ReportsAlreadyCorrect()
    {
        string path = ComposeCode(horizontal: 1, vertical: -1);

        ExamCircleOrientation result = new ExamCircleOrientationService().Detect(path);

        Assert.True(result.FoundTCode);
        Assert.Equal(ExamCircleRotation.None, result.Rotation);
        Assert.Contains("已是正确方向", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_CodeInTopLeft_RequestsClockwise90()
    {
        string path = ComposeCode(horizontal: -1, vertical: -1);

        ExamCircleOrientation result = new ExamCircleOrientationService().Detect(path);

        Assert.True(result.FoundTCode);
        Assert.Equal(ExamCircleRotation.Clockwise90, result.Rotation);
        Assert.Contains("左上角", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_CodeInBottomLeft_RequestsHalfTurn()
    {
        string path = ComposeCode(horizontal: -1, vertical: 1);

        ExamCircleOrientation result = new ExamCircleOrientationService().Detect(path);

        Assert.True(result.FoundTCode);
        Assert.Equal(ExamCircleRotation.HalfTurn, result.Rotation);
        Assert.Contains("左下角", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_CodeInBottomRight_RequestsCounterClockwise90()
    {
        string path = ComposeCode(horizontal: 1, vertical: 1);

        ExamCircleOrientation result = new ExamCircleOrientationService().Detect(path);

        Assert.True(result.FoundTCode);
        Assert.Equal(ExamCircleRotation.CounterClockwise90, result.Rotation);
        Assert.Contains("右下角", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_AfterApplyingSuggestedRotation_BecomesCorrect()
    {
        // 端到端闭环：左上角 → 建议顺时针 90 → 转完再识别应为「已是正确方向」。
        string path = ComposeCode(horizontal: -1, vertical: -1);
        var service = new ExamCircleOrientationService();
        ExamCircleOrientation before = service.Detect(path);
        Assert.Equal(ExamCircleRotation.Clockwise90, before.Rotation);

        string rotated = new ExamCircleImageService().Rotate(
            path,
            (int)before.Rotation,
            Path.Combine(_root, "rotated.png"));

        ExamCircleOrientation after = service.Detect(rotated);
        Assert.True(after.FoundTCode);
        Assert.Equal(ExamCircleRotation.None, after.Rotation);
    }

    [Fact]
    public void Detect_ImageWithoutCode_ReportsNotFoundAndNoRotation()
    {
        string path = Path.Combine(_root, "blank.png");
        using (var bitmap = new SKBitmap(CanvasSize, CanvasSize))
        {
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(SKColors.White);
            }

            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes(path, data.ToArray());
        }

        ExamCircleOrientation result = new ExamCircleOrientationService().Detect(path);

        Assert.False(result.FoundTCode);
        Assert.Equal(ExamCircleRotation.None, result.Rotation);
        Assert.Contains("未找到", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Detect_MissingFile_ThrowsFileNotFound()
    {
        string missing = Path.Combine(_root, "nope.png");

        Assert.Throws<FileNotFoundException>(() => new ExamCircleOrientationService().Detect(missing));
    }

    /// <summary>
    /// 生成一张 <see cref="CanvasSize"/>×<see cref="CanvasSize"/> 的白底图，把「T001」DataMatrix 贴到指定象限。
    /// <paramref name="horizontal"/>：-1 左 / 1 右；<paramref name="vertical"/>：-1 上 / 1 下。
    /// </summary>
    private string ComposeCode(int horizontal, int vertical)
    {
        byte[] codePng = _barcodes.CreatePng("T001", "DataMatrix", moduleSize: 6);
        using SKBitmap code = SKBitmap.Decode(codePng) ?? throw new InvalidOperationException("DataMatrix 生成失败");
        string path = Path.Combine(_root, $"code-{horizontal}-{vertical}.png");

        using var canvasBitmap = new SKBitmap(CanvasSize, CanvasSize);
        using (var canvas = new SKCanvas(canvasBitmap))
        {
            canvas.Clear(SKColors.White);
            float margin = 30f;
            float x = horizontal < 0 ? margin : CanvasSize - code.Width - margin;
            float y = vertical < 0 ? margin : CanvasSize - code.Height - margin;
            canvas.DrawBitmap(code, x, y);
        }

        using SKImage image = SKImage.FromBitmap(canvasBitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
        return path;
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
