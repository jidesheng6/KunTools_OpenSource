using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocumentTools.Core.Services.Exam;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// 圆圈修复与 dev 的算法等价性证据（task-40）。
/// dev 侧基准：<c>dev:DocumentTools/ExamCircleFixForm.cs</c> 的
/// <c>btnStartMatch_Click</c>（:625-687，OpenCV <c>MatchTemplate(CCoeffNormed)</c> + <c>MinMaxLoc</c> 贪心 + 邻域置零）
/// 与 <c>btnDoFix_Click</c>（:713-819，同尺寸 <c>CopyTo</c>、异尺寸 <c>Resize(..., Linear)</c>）。
/// 本文件用纯 C# 复刻 dev 的这两个算法（模板/窗口都减去均值，Pearson 相关，值域 -1..1），
/// 再把新版 <see cref="ExamCircleFixService"/> 的结果逐项比对。
/// </summary>
public sealed class ExamCircleParityTests : IDisposable
{
    private readonly string _root;

    public ExamCircleParityTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "dt-circle-parity-" + Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(_root);
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

    [Fact]
    public void Threshold_ComparesTheSamePearsonValueAsDevSlider()
    {
        string image = CreateRingImage("threshold.png", 200, 120, new (int X, int Y)[] { (100, 60) });
        var template = new ExamCircleRect { Left = 88, Top = 48, Width = 24, Height = 24 };
        var service = new ExamCircleFixService();

        var matches = service.Match(image, template, null, 0.8, 100);
        Assert.Single(matches);
        ExamCircleMatch match = matches[0];
        Assert.Equal(88, match.Left);
        Assert.Equal(48, match.Top);

        // 同一位置的参考 Pearson：dev 的 CCoeffNormed 与新版的 (Σtp - n·t̄p̄)/(n·σt·σp) 是同一个量。
        (byte[] gray, int width, int height) = LoadGrayscale(image);
        double[,] surface = CorrelationSurface(gray, width, height, template, new ExamCircleRect { Left = 0, Top = 0, Width = width, Height = height });
        double reference = surface[88, 48];
        Assert.Equal(reference, match.Similarity, 6);

        // 阈值就是这条 Pearson 值：抬到分数之上立刻消失，压到分数之下立刻回来（dev 的 80 → 0.8 同理）。
        Assert.Empty(service.Match(image, template, null, Math.Min(1.0, match.Similarity + 0.02), 100));
        Assert.Single(service.Match(image, template, null, Math.Max(0.01, match.Similarity - 0.02), 100));
    }

    [Fact]
    public void DenseAdjacentCircles_MatchesDevMinMaxLocLoop()
    {
        // 相邻圆圈：间距 14px < 模板宽 24px，恰好落在「邻域抑制」会生效的区间。
        var centers = new (int X, int Y)[] { (40, 40), (54, 40), (68, 40), (82, 40), (96, 40), (150, 80), (164, 80) };
        string image = CreateRingImage("dense.png", 220, 120, centers);
        var template = new ExamCircleRect { Left = 28, Top = 28, Width = 24, Height = 24 };
        var search = new ExamCircleRect { Left = 20, Top = 20, Width = 160, Height = 80 };

        var service = new ExamCircleFixService();
        IReadOnlyList<ExamCircleMatch> actual = service.Match(image, template, search, 0.8, 1000);

        (byte[] gray, int width, int height) = LoadGrayscale(image);
        double[,] surface = CorrelationSurface(gray, width, height, template, search);
        List<(int X, int Y, double Score)> expected = DevMinMaxLocLoop(surface, template, search, 0.8);

        // 顺序不是等价性的一部分（dev 按 MinMaxLoc 的扫描序、新版按相似度降序），这里按位置归一后比集合。
        List<(int X, int Y, double Score)> dev = expected.OrderBy(e => e.Y).ThenBy(e => e.X).ToList();
        List<ExamCircleMatch> ours = actual.OrderBy(a => a.Top).ThenBy(a => a.Left).ToList();

        Assert.True(
            dev.Count == ours.Count,
            $"dev={dev.Count} [{string.Join(" ", dev.Select(e => $"{e.X},{e.Y},{e.Score:F4}"))}] " +
            $"new={ours.Count} [{string.Join(" ", ours.Select(a => $"{a.Left},{a.Top},{a.Similarity:F4}"))}]");

        for (int i = 0; i < dev.Count; i++)
        {
            // dev 用整数窗口置零、新版用中心距抑制，二者边界处允许 1px 差异。
            Assert.InRange(ours[i].Left, dev[i].X - 1, dev[i].X + 1);
            Assert.InRange(ours[i].Top, dev[i].Y - 1, dev[i].Y + 1);
            Assert.Equal(dev[i].Score, ours[i].Similarity, 6);
        }
    }

    [Fact]
    public void Fix_WithoutReplacement_CopiesTemplatePatchExactlyLikeDev()
    {
        var centers = new (int X, int Y)[] { (40, 40), (100, 40), (160, 40) };
        string image = CreateRingImage("fix.png", 220, 100, centers);
        var template = new ExamCircleRect { Left = 28, Top = 28, Width = 24, Height = 24 };
        var service = new ExamCircleFixService();

        IReadOnlyList<ExamCircleMatch> matches = service.Match(image, template, null, 0.8, 100);
        Assert.Equal(3, matches.Count);

        string output = Path.Combine(_root, "fixed.png");
        string produced = service.Fix(new ExamCircleFixRequest
        {
            SourceImage = image,
            TemplateRect = template,
            SimilarityThreshold = 0.8,
            OutputFile = output,
        });
        Assert.True(File.Exists(produced));

        // dev 分支：无替代图 → 用模板区域本像素做 patch，同尺寸 CopyTo。参考实现照抄这一步。
        using SKBitmap source = SKBitmap.Decode(image) ?? throw new InvalidOperationException("源图解码失败。");
        using SKBitmap reference = new(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(reference))
        {
            canvas.DrawBitmap(source, 0, 0);
            using var patch = Crop(source, template);
            foreach (ExamCircleMatch match in matches)
            {
                canvas.DrawBitmap(patch, match.Left, match.Top);
            }
        }

        byte[] referenceBytes = Pixels(reference);
        using SKBitmap actualBitmap = SKBitmap.Decode(produced) ?? throw new InvalidOperationException("产物解码失败。");
        byte[] actualBytes = Pixels(actualBitmap);
        Assert.Equal(referenceBytes.Length, actualBytes.Length);

        int maxDiff = 0;
        for (int i = 0; i < referenceBytes.Length; i++)
        {
            maxDiff = Math.Max(maxDiff, Math.Abs(referenceBytes[i] - actualBytes[i]));
        }

        Assert.InRange(maxDiff, 0, 2);
    }

    [Fact]
    public void Fix_ReplacementImage_ResizesIntoRectLikeDevLinear()
    {
        string image = CreateRingImage("repl-source.png", 200, 80, new (int X, int Y)[] { (40, 40), (140, 40) });
        string replacement = CreateRingImage("repl-patch.png", 36, 36, new (int X, int Y)[] { (18, 18) });
        var template = new ExamCircleRect { Left = 28, Top = 28, Width = 24, Height = 24 };
        var service = new ExamCircleFixService();

        IReadOnlyList<ExamCircleMatch> matches = service.Match(image, template, null, 0.8, 100);
        Assert.Equal(2, matches.Count);

        string output = Path.Combine(_root, "repl-fixed.png");
        service.Fix(new ExamCircleFixRequest
        {
            SourceImage = image,
            TemplateRect = template,
            SimilarityThreshold = 0.8,
            ReplacementImage = replacement,
            OutputFile = output,
        });

        // dev 把替代图 Resize 到目标矩形尺寸（Linear）再写回 —— 尺寸必须恰好等于匹配框。
        using SKBitmap source = SKBitmap.Decode(image) ?? throw new InvalidOperationException("源图解码失败。");
        using SKBitmap patchSource = SKBitmap.Decode(replacement) ?? throw new InvalidOperationException("替代图解码失败。");
        using SKBitmap expected = new(new SKImageInfo(source.Width, source.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(expected))
        {
            canvas.DrawBitmap(source, 0, 0);
            using SKBitmap scaled = patchSource.Resize(
                new SKImageInfo(template.Width, template.Height, SKColorType.Bgra8888, SKAlphaType.Premul),
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None))
                ?? throw new InvalidOperationException("参考缩放失败。");
            foreach (ExamCircleMatch match in matches)
            {
                canvas.DrawBitmap(scaled, match.Left, match.Top);
            }
        }

        using SKBitmap actual = SKBitmap.Decode(output) ?? throw new InvalidOperationException("产物解码失败。");
        byte[] expectedBytes = Pixels(expected);
        byte[] actualBytes = Pixels(actual);
        Assert.Equal(expectedBytes.Length, actualBytes.Length);

        double sum = 0;
        int maxDiff = 0;
        for (int i = 0; i < expectedBytes.Length; i++)
        {
            int diff = Math.Abs(expectedBytes[i] - actualBytes[i]);
            sum += diff;
            maxDiff = Math.Max(maxDiff, diff);
        }

        // 同一插值（Linear）下只允许混合引擎的舍入差；出现大范围偏差就说明缩放策略不同。
        Assert.InRange(maxDiff, 0, 8);
        Assert.InRange(sum / expectedBytes.Length, 0, 1.0);
    }

    private string CreateRingImage(string fileName, int width, int height, (int X, int Y)[] centers)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = false, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };
            foreach ((int x, int y) in centers)
            {
                canvas.DrawCircle(x, y, 8f, paint);
            }
        }

        string path = Path.Combine(_root, fileName);
        using FileStream stream = File.Create(path);
        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
        return path;
    }

    private static SKBitmap Crop(SKBitmap source, ExamCircleRect rect)
    {
        var patch = new SKBitmap(new SKImageInfo(rect.Width, rect.Height, SKColorType.Bgra8888, SKAlphaType.Premul));
        for (int y = 0; y < rect.Height; y++)
        {
            for (int x = 0; x < rect.Width; x++)
            {
                patch.SetPixel(x, y, source.GetPixel(rect.Left + x, rect.Top + y));
            }
        }

        return patch;
    }

    private static byte[] Pixels(SKBitmap bitmap)
    {
        byte[] bytes = new byte[bitmap.RowBytes * bitmap.Height];
        System.Runtime.InteropServices.Marshal.Copy(bitmap.GetPixels(), bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>与 Core 的 BT.601 整数灰度完全一致（ExamCircleFixService.LoadGrayscale :935）。</summary>
    private static (byte[] Gray, int Width, int Height) LoadGrayscale(string path)
    {
        using SKBitmap bitmap = SKBitmap.Decode(path) ?? throw new InvalidOperationException("解码失败。");
        int width = bitmap.Width;
        int height = bitmap.Height;
        var gray = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                SKColor color = bitmap.GetPixel(x, y);
                gray[(y * width) + x] = (byte)(((color.Blue * 299) + (color.Green * 587) + (color.Red * 114)) / 1000);
            }
        }

        return (gray, width, height);
    }

    /// <summary>复刻 dev 的 <c>Cv2.MatchTemplate(..., CCoeffNormed)</c>：每个窗口与模板的 Pearson 相关。</summary>
    private static double[,] CorrelationSurface(byte[] gray, int width, int height, ExamCircleRect template, ExamCircleRect region)
    {
        int tw = template.Width;
        int th = template.Height;
        double[,] surface = new double[region.Width - tw + 1, region.Height - th + 1];
        int count = tw * th;

        double templateSum = 0;
        double templateSquare = 0;
        for (int y = 0; y < th; y++)
        {
            for (int x = 0; x < tw; x++)
            {
                double value = gray[((template.Top + y) * width) + template.Left + x];
                templateSum += value;
                templateSquare += value * value;
            }
        }

        double templateMean = templateSum / count;
        double templateStd = Math.Sqrt(Math.Max(0, (templateSquare / count) - (templateMean * templateMean)));

        for (int ry = 0; ry < surface.GetLength(1); ry++)
        {
            for (int rx = 0; rx < surface.GetLength(0); rx++)
            {
                double sum = 0;
                double square = 0;
                double cross = 0;
                for (int y = 0; y < th; y++)
                {
                    int row = (region.Top + ry + y) * width;
                    for (int x = 0; x < tw; x++)
                    {
                        double image = gray[row + region.Left + rx + x];
                        double tmpl = gray[((template.Top + y) * width) + template.Left + x];
                        sum += image;
                        square += image * image;
                        cross += image * tmpl;
                    }
                }

                double imageMean = sum / count;
                double imageStd = Math.Sqrt(Math.Max(0, (square / count) - (imageMean * imageMean)));
                double denom = count * templateStd * imageStd;
                surface[rx, ry] = denom <= 1e-9 ? 0 : (cross - (count * templateMean * imageMean)) / denom;
            }
        }

        return surface;
    }

    /// <summary>复刻 dev 循环：取全局最大值 → 低于阈值即停 → 命中 → 把 maxLoc 中心、模板宽高窗口置零。</summary>
    private static List<(int X, int Y, double Score)> DevMinMaxLocLoop(double[,] surface, ExamCircleRect template, ExamCircleRect region, double threshold)
    {
        var matches = new List<(int, int, double)>();
        int cols = surface.GetLength(0);
        int rows = surface.GetLength(1);

        while (true)
        {
            double best = double.NegativeInfinity;
            int bestX = -1;
            int bestY = -1;
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (surface[x, y] > best)
                    {
                        best = surface[x, y];
                        bestX = x;
                        bestY = y;
                    }
                }
            }

            if (bestX < 0 || best < threshold)
            {
                break;
            }

            matches.Add((region.Left + bestX, region.Top + bestY, best));

            int x0 = Math.Max(0, bestX - (template.Width / 2));
            int y0 = Math.Max(0, bestY - (template.Height / 2));
            int x1 = Math.Min(cols, bestX + (template.Width / 2));
            int y1 = Math.Min(rows, bestY + (template.Height / 2));
            for (int y = y0; y < y1; y++)
            {
                for (int x = x0; x < x1; x++)
                {
                    surface[x, y] = 0;
                }
            }
        }

        return matches;
    }
}
