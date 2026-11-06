using System.Globalization;
using SkiaSharp;

namespace QaCompat;

/// <summary>夹具与文件系统辅助：不依赖 Aspose 生成 PDF（评估模式会改页数/加水印，污染基准）。</summary>
internal static class Fixtures
{
    /// <summary>生成指定页数的 PDF（SkiaSharp 直接写 PDF，页数精确）。</summary>
    public static void CreatePdf(string filePath, int pageCount)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        using SKDocument document = SKDocument.CreatePdf(stream)
            ?? throw new InvalidOperationException("SKDocument.CreatePdf 返回 null。");

        for (int page = 1; page <= pageCount; page++)
        {
            SKCanvas canvas = document.BeginPage(595f, 842f);
            canvas.Clear(SKColors.White);
            using (var block = new SKPaint
                   {
                       Color = new SKColor((byte)(30 + page * 35 % 200), 90, 160),
                       Style = SKPaintStyle.Fill,
                       IsAntialias = true
                   })
            {
                canvas.DrawRect(new SKRect(60, 80, 535, 320), block);
            }

            using (var font = new SKFont(SKTypeface.Default, 32f))
            using (var ink = new SKPaint { Color = SKColors.Black, IsAntialias = true })
            {
                canvas.DrawText(
                    "QA page " + page.ToString(CultureInfo.InvariantCulture) + " / " + pageCount.ToString(CultureInfo.InvariantCulture),
                    80f,
                    420f,
                    SKTextAlign.Left,
                    font,
                    ink);
            }

            document.EndPage();
        }

        document.Close();
    }

    /// <summary>生成一张 PNG（默认 320×240）。</summary>
    public static void CreateImage(string filePath, SKColor? background = null)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var bitmap = new SKBitmap(320, 240);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(background ?? new SKColor(60, 120, 200));
            using var dot = new SKPaint { Color = SKColors.White, IsAntialias = true };
            canvas.DrawCircle(160, 120, 70, dot);
            using var bar = new SKPaint { Color = new SKColor(20, 30, 40), IsAntialias = false };
            canvas.DrawRect(new SKRect(0, 200, 320, 240), bar);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 90);
        using var stream = File.Create(filePath);
        data.SaveTo(stream);
    }

    public static int CountPdfPages(string filePath)
    {
        using var document = new Aspose.Pdf.Document(filePath);
        return document.Pages.Count;
    }

    public static string PdfText(string filePath)
    {
        using var document = new Aspose.Pdf.Document(filePath);
        var absorber = new Aspose.Pdf.Text.TextAbsorber();
        document.Pages.Accept(absorber);
        return absorber.Text ?? string.Empty;
    }

    /// <summary>逐级拼出超过 <paramref name="minimumLength"/> 的目录（Windows 长路径场景）。</summary>
    public static string EnsureLongDirectory(string rootDirectory, int minimumLength, string leafFileName)
    {
        string current = rootDirectory;
        int index = 0;
        while (current.Length + 1 + leafFileName.Length < minimumLength)
        {
            string segment = "seg" + index.ToString("D2", CultureInfo.InvariantCulture) + "_" + new string('x', 34);
            current = Path.Combine(current, segment);
            index++;
        }

        Directory.CreateDirectory(current);
        return current;
    }

    /// <summary>用 icacls 对 Everyone 加拒绝写 ACE；返回 icacls 的原始输出。</summary>
    public static ProcResult DenyWrite(string directory)
        => Proc.Run(
            "icacls",
            new[] { directory, "/deny", "*S-1-1-0:(W)" },
            Path.GetDirectoryName(directory) ?? directory);

    public static ProcResult AllowWrite(string directory)
        => Proc.Run(
            "icacls",
            new[] { directory, "/remove:d", "*S-1-1-0" },
            Path.GetDirectoryName(directory) ?? directory);

    /// <summary>探针写入：验证 ACL 是否真的生效（返回 null 表示被拒绝，即生效）。</summary>
    public static Exception? ProbeWrite(string directory)
    {
        try
        {
            File.WriteAllText(Path.Combine(directory, "qa-write-probe.txt"), "probe");
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
