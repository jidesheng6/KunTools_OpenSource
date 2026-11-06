using System.Globalization;
using SkiaSharp;

namespace DocumentTools.QaPerf;

/// <summary>Deterministic xorshift PRNG: fixture bytes must be identical on every machine.</summary>
internal struct XorShift
{
    private uint _state;

    public XorShift(uint seed) => _state = seed == 0 ? 0x9E3779B9u : seed;

    public uint NextRaw()
    {
        uint x = _state;
        x ^= x << 13;
        x ^= x >> 17;
        x ^= x << 5;
        _state = x;
        return x;
    }

    public int Next(int minInclusive, int maxExclusive)
    {
        if (maxExclusive <= minInclusive)
        {
            return minInclusive;
        }

        uint range = (uint)(maxExclusive - minInclusive);
        return minInclusive + (int)(NextRaw() % range);
    }
}

/// <summary>Fixture generation for the image scenario: real 1600x1200 PNGs built with SkiaSharp.</summary>
internal static class ImageFixtures
{
    public const int Width = 1600;
    public const int Height = 1200;
    public const int Count = 50;
    private const int MaxFixtureBytes = 200 * 1024;

    /// <summary>Progressively coarser noise keeps 1600x1200 PNGs inside the 200 KB budget.</summary>
    private static readonly (int BlockSize, int Speckles)[] SizeLadder =
    {
        (24, 900), (32, 700), (48, 500), (64, 350), (96, 200), (128, 100), (192, 40), (256, 0)
    };

    /// <summary>Creates any missing PNG; existing files are reused so runs stay comparable.</summary>
    public static int Ensure(string directory, ICollection<string> notes)
    {
        Directory.CreateDirectory(directory);
        int created = 0;
        for (int index = 1; index <= Count; index++)
        {
            string path = Path.Combine(directory, string.Format(CultureInfo.InvariantCulture, "px_{0:000}.png", index));
            if (File.Exists(path) && new FileInfo(path).Length > 1024)
            {
                continue;
            }

            WriteOne(path, index);
            created++;
        }

        notes.Add(created == 0
            ? "PNG 夹具已存在，直接复用（保证多轮测量输入一致）"
            : string.Format(CultureInfo.InvariantCulture, "新建 {0} 张 PNG", created));
        return created;
    }

    /// <summary>
    /// Gradient + block noise + speckles, coarsened until the file fits the 200 KB fixture budget.
    /// Same seed always produces the same bytes for a given ladder step.
    /// </summary>
    private static void WriteOne(string path, int seed)
    {
        foreach ((int blockSize, int speckles) in SizeLadder)
        {
            if (Write(path, seed, blockSize, speckles) <= MaxFixtureBytes)
            {
                return;
            }
        }
    }

    private static long Write(string path, int seed, int blockSize, int speckleCount)
    {
        var random = new XorShift((uint)((seed * 2654435761u) + 12345u));
        // SkiaSharp 3.x has no PNG encoder-options overload on SKImage (only
        // Encode(SKEncodedImageFormat, int)), and its default deflate output on this
        // gradient+noise content is ~224 KB, over the fixture budget. Paint with Skia,
        // then write the PNG container ourselves: Sub filter + zlib level 9 keeps a
        // 1600x1200 frame near 50 KB and makes the byte size reproducible.
        var info = new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
        using var bitmap = new SKBitmap(info);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(new SKColor(18, 24, 38));

        using (var gradient = new SKPaint { IsAntialias = false })
        {
            gradient.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(Width, Height),
                new[] { new SKColor(12, 34, 68), new SKColor(226, 176, 96) },
                null,
                SKShaderTileMode.Clamp);
            canvas.DrawRect(SKRect.Create(Width, Height), gradient);
        }

        using (var paint = new SKPaint { IsAntialias = false })
        {
            for (int by = 0; by * blockSize < Height; by++)
            {
                for (int bx = 0; bx * blockSize < Width; bx++)
                {
                    int jitter = random.Next(0, 41) - 20;
                    paint.Color = new SKColor(
                        (byte)Math.Clamp(24 + jitter, 0, 255),
                        (byte)Math.Clamp(24 + jitter, 0, 255),
                        (byte)Math.Clamp(24 + jitter, 0, 255),
                        20);
                    canvas.DrawRect(
                        SKRect.Create(bx * blockSize, by * blockSize, blockSize, blockSize),
                        paint);
                }
            }

            for (int i = 0; i < speckleCount; i++)
            {
                int x = random.Next(0, Width - 3);
                int y = random.Next(0, Height - 3);
                paint.Color = new SKColor(
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256),
                    (byte)random.Next(0, 256),
                    200);
                canvas.DrawRect(SKRect.Create(x, y, 2, 2), paint);
            }
        }

        canvas.Flush();

        PngWriter.Write(path, Width, Height, bitmap.GetPixelSpan());
        return new FileInfo(path).Length;
    }
}
