using Aspose.BarCode.BarCodeRecognition;
using SkiaSharp;

namespace DocumentTools.Core.Services.Exam;

/// <summary>自动旋转的四种结果（角度为顺时针）。</summary>
public enum ExamCircleRotation
{
    None = 0,
    Clockwise90 = 90,
    HalfTurn = 180,
    CounterClockwise90 = 270,
}

/// <summary>
/// 自动旋转识别结果。<see cref="CenterX"/>/<see cref="CenterY"/> 为 T 码区域的几何中心（像素，左上为原点）。
/// </summary>
public sealed record ExamCircleOrientation(
    bool FoundTCode,
    ExamCircleRotation Rotation,
    string Message,
    double CenterX,
    double CenterY);

/// <summary>考评表图片方向识别（legacy「自动旋转」，BodyForm.cs:322-400）。</summary>
public interface IExamCircleOrientationService
{
    /// <summary>
    /// 读图里的 DataMatrix 码，取以 <paramref name="prefix"/> 开头的第一条，按码中心所在象限判断该怎么转正：
    /// 右上＝已正确、左上＝顺时针 90、左下＝180、右下＝逆时针 90（与 legacy 四分支一一对应）。
    /// </summary>
    ExamCircleOrientation Detect(string imageFile, string prefix = "T");
}

/// <summary>
/// legacy 用 Aspose.BarCode 的 <c>BarCodeReader(..., DecodeType.DataMatrix)</c> + <c>BarCodeResult.Region.Points</c>
/// 定位 T 码（BodyForm.cs:345-388）。这里保持同一识别器与同一象限判据，只是把结果做成可测试的 DTO。
/// </summary>
public sealed class ExamCircleOrientationService : IExamCircleOrientationService
{
    private const string MissingImageMessage = "图片不存在。";
    private const string DecodeFailureMessage = "无法解码图片。";

    public ExamCircleOrientation Detect(string imageFile, string prefix = "T")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(imageFile);
        if (!File.Exists(imageFile))
        {
            throw new FileNotFoundException(MissingImageMessage, imageFile);
        }

        using SKBitmap? source = SKBitmap.Decode(imageFile) ?? throw new InvalidOperationException(DecodeFailureMessage);
        int width = source.Width;
        int height = source.Height;
        if (width <= 0 || height <= 0)
        {
            throw new InvalidOperationException(DecodeFailureMessage);
        }

        // legacy 先合成到白底再识别（PNG 带透明时识别率会掉，BodyForm.cs:332-339）。
        using MemoryStream stream = FlattenToPng(source);
        using var reader = new BarCodeReader(stream, DecodeType.DataMatrix);
        reader.QualitySettings = QualitySettings.HighPerformance;

        foreach (BarCodeResult result in reader.ReadBarCodes())
        {
            string text = result.CodeText ?? string.Empty;
            if (text.Length == 0 || !text.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            if (!TryGetCenter(result, out double centerX, out double centerY))
            {
                continue;
            }

            bool right = centerX > width / 2.0;
            bool top = centerY < height / 2.0;
            (ExamCircleRotation rotation, string message) = (right, top) switch
            {
                (true, true) => (ExamCircleRotation.None, "当前方向已是正确方向(T码在右上角)。"),
                (false, true) => (ExamCircleRotation.Clockwise90, $"检测到T码在左上角(X:{centerX:F0},Y:{centerY:F0})，已自动顺时针旋转 90°。"),
                (false, false) => (ExamCircleRotation.HalfTurn, $"检测到T码在左下角(X:{centerX:F0},Y:{centerY:F0})，已自动旋转 180°。"),
                _ => (ExamCircleRotation.CounterClockwise90, $"检测到T码在右下角(X:{centerX:F0},Y:{centerY:F0})，已自动逆时针旋转 90°。"),
            };

            return new ExamCircleOrientation(true, rotation, message, centerX, centerY);
        }

        return new ExamCircleOrientation(false, ExamCircleRotation.None, "未找到可识别的 T 码，未做旋转。", 0, 0);
    }

    /// <summary>把识别用的位图铺在白底上再编码成 PNG（透明像素按白色参与识别）。</summary>
    private static MemoryStream FlattenToPng(SKBitmap source)
    {
        var stream = new MemoryStream();
        using (var flattened = new SKBitmap(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Opaque))
        {
            using (var canvas = new SKCanvas(flattened))
            {
                canvas.Clear(SKColors.White);
                canvas.DrawBitmap(source, 0, 0);
            }

            using SKImage image = SKImage.FromBitmap(flattened);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100)
                                ?? throw new InvalidOperationException("无法编码识别用图片。");
            stream.Write(data.ToArray(), 0, (int)data.Size);
        }

        stream.Position = 0;
        return stream;
    }

    /// <summary>取码区域的几何中心；个别识别器版本不回传点位时返回 false，交由调用方跳过该条。</summary>
    private static bool TryGetCenter(BarCodeResult result, out double centerX, out double centerY)
    {
        centerX = 0;
        centerY = 0;
        // 26.x 的 BarCodeResult.Region 是 BarCodeRegionParameters，Points 为 Aspose.Drawing.Point[]。
        Aspose.Drawing.Point[]? points = result.Region?.Points;
        if (points is null || points.Length == 0)
        {
            return false;
        }

        double sumX = 0;
        double sumY = 0;
        foreach (Aspose.Drawing.Point point in points)
        {
            sumX += point.X;
            sumY += point.Y;
        }

        centerX = sumX / points.Length;
        centerY = sumY / points.Length;
        return true;
    }
}
