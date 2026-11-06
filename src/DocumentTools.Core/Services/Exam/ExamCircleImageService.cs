using SkiaSharp;

namespace DocumentTools.Core.Services.Exam;

public interface IExamCircleImageService
{
    /// <summary>Rotate image by degrees (90/180/270) and overwrite or save to output.</summary>
    string Rotate(string imagePath, int degreesClockwise, string? outputPath = null);
}

public sealed class ExamCircleImageService : IExamCircleImageService
{
    public string Rotate(string imagePath, int degreesClockwise, string? outputPath = null)
    {
        if (!File.Exists(imagePath))
        {
            throw new FileNotFoundException("图片不存在。", imagePath);
        }

        degreesClockwise = ((degreesClockwise % 360) + 360) % 360;
        if (degreesClockwise is not (0 or 90 or 180 or 270))
        {
            throw new ArgumentException("仅支持 0/90/180/270 度旋转。");
        }

        using SKBitmap? src = SKBitmap.Decode(imagePath)
                             ?? throw new InvalidOperationException("无法解码图片。");
        SKBitmap rotated = degreesClockwise switch
        {
            0 => src.Copy(),
            90 => Rotate90(src),
            180 => Rotate180(src),
            _ => Rotate270(src)
        };

        string target = string.IsNullOrWhiteSpace(outputPath) ? imagePath : outputPath!;
        string? dir = Path.GetDirectoryName(Path.GetFullPath(target));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using (rotated)
        {
            WriteEncoded(rotated, target);
        }

        return target;
    }

    private static void WriteEncoded(SKBitmap bitmap, string target)
    {
        // SkiaSharp 3.x 的托管构建没有 BMP 编码器：Encode(SKEncodedImageFormat.Bmp) 返回 null，
        // 旧写法 data.SaveTo(fs) 会直接 NullReferenceException（旋转 .bmp 素材必崩）。
        if (GuessFormat(target) == SKEncodedImageFormat.Bmp)
        {
            File.WriteAllBytes(target, BmpEncoder.EncodeBgra32(bitmap));
            return;
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData? data = image.Encode(GuessFormat(target), 92);
        if (data is null)
        {
            throw new InvalidOperationException(
                "当前环境不支持把图片编码为 " + Path.GetExtension(target) + "，请改为 .png 或 .jpg。");
        }

        using var fs = File.Create(target);
        data.SaveTo(fs);
    }

    private static SKEncodedImageFormat GuessFormat(string path)
        => Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".png" => SKEncodedImageFormat.Png,
            ".bmp" => SKEncodedImageFormat.Bmp,
            ".webp" => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Png
        };

    private static SKBitmap Rotate90(SKBitmap src)
    {
        var dst = new SKBitmap(src.Height, src.Width);
        using var canvas = new SKCanvas(dst);
        canvas.Translate(dst.Width, 0);
        canvas.RotateDegrees(90);
        canvas.DrawBitmap(src, 0, 0);
        return dst;
    }

    private static SKBitmap Rotate180(SKBitmap src)
    {
        var dst = new SKBitmap(src.Width, src.Height);
        using var canvas = new SKCanvas(dst);
        canvas.Translate(dst.Width, dst.Height);
        canvas.RotateDegrees(180);
        canvas.DrawBitmap(src, 0, 0);
        return dst;
    }

    private static SKBitmap Rotate270(SKBitmap src)
    {
        var dst = new SKBitmap(src.Height, src.Width);
        using var canvas = new SKCanvas(dst);
        canvas.Translate(0, dst.Height);
        canvas.RotateDegrees(270);
        canvas.DrawBitmap(src, 0, 0);
        return dst;
    }
}
