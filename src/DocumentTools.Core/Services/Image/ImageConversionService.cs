using DocumentTools.Core.Common;
using SkiaSharp;

namespace DocumentTools.Core.Services.Image;

public sealed class ImageConversionService : IImageConversionService
{
    /// <summary>多尺寸 ICO 的固定尺寸集合（legacy IcoSizeTable 的全部取值）。</summary>
    public static readonly IReadOnlyList<int> MultiSizes = new[] { 16, 32, 64, 128, 256 };

    /// <summary>Windows 传统 MAX_PATH（含结尾 nul 是 260），超过它的路径 SkiaSharp 的原生解码会失败。</summary>
    internal const int MaxPathLength = 259;

    /// <summary>
    /// 路径问题在解码之前就要单独报错：否则「路径过长」会被解码失败掩盖成「图片损坏」，
    /// 用户会去换图片而不是改路径。
    /// </summary>
    private static void GuardSourcePath(string sourceFile)
    {
        if (string.IsNullOrWhiteSpace(sourceFile))
        {
            return;   // 空路径交给 PathGuard.RequireExistingFile 报「路径不能为空：…」
        }

        if (sourceFile.Length > MaxPathLength)
        {
            throw new InvalidOperationException(
                $"路径过长（{sourceFile.Length} 字符，超过 Windows 260 字符限制）：{sourceFile}。"
                + "请把文件移到更短的目录（例如 D:\\temp\\）后重试。");
        }

        if (sourceFile.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            throw new InvalidOperationException($"路径包含非法字符：{sourceFile}");
        }
    }

    /// <summary>
    /// 统一解码入口，把三类原因分开报：路径不合法 / 读不了（占用、权限）/ 不是图片或已损坏。
    /// </summary>
    private static SKBitmap DecodeImage(string sourceFile)
    {
        GuardSourcePath(sourceFile);

        SKBitmap? bitmap;
        try
        {
            bitmap = SKBitmap.Decode(sourceFile);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"无法读取文件（被占用或权限不足）：{Path.GetFileName(sourceFile)}", ex);
        }

        return bitmap
            ?? throw new InvalidOperationException(
                $"文件不是受支持的图片格式或已损坏：{Path.GetFileName(sourceFile)}");
    }

    /// <summary>写出结果文件；占用/权限问题同样要报清原因，而不是让原始 IOException 冒到界面上。</summary>
    private static void WriteOutput(string path, byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(path, bytes);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"无法写入输出文件（被占用或权限不足）：{Path.GetFileName(path)}", ex);
        }
    }

    public string Convert(
        string sourceFile,
        string outputDirectory,
        string format,
        int jpegQuality = 75,
        int? maxEdgePx = null)
    {
        // 旧签名保持原行为：未显式给细化参数时保留透明通道，质量按入参。
        return Convert(
            sourceFile,
            outputDirectory,
            new ImageEncodeOptions
            {
                Format = format,
                JpegQuality = jpegQuality,
                PngColor = ImagePngColorType.TruecolorWithAlpha,
                MaxEdgePx = maxEdgePx,
                // 旧签名总是显式指定目标格式，不沿用源后缀。
                UseSourceExtension = false
            });
    }

    public string Convert(string sourceFile, string outputDirectory, ImageEncodeOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        GuardSourcePath(sourceFile);
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        Directory.CreateDirectory(outputDirectory);

        // legacy：勾了「不使用源后缀名保存」才用选定格式，否则沿用源后缀（默认沿用）。
        string ext = options.UseSourceExtension
            ? Path.GetExtension(sourceFile).ToLowerInvariant()
            : NormalizeFormat(options.Format);
        if (string.IsNullOrEmpty(ext))
        {
            ext = NormalizeFormat(options.Format);
        }

        string output = Path.Combine(
            outputDirectory,
            Path.GetFileNameWithoutExtension(sourceFile) + "_out" + ext);

        // legacy「不执行压缩操作」= 只改名不重编码，字节与源文件一致。
        if (options.NoCompress)
        {
            File.Copy(sourceFile, output, overwrite: true);
            return output;
        }

        using var bitmap = DecodeImage(sourceFile);

        SKBitmap? scaled = null;
        try
        {
            SKBitmap working = bitmap;
            if (options.MaxEdgePx is > 0 && Math.Max(bitmap.Width, bitmap.Height) > options.MaxEdgePx.Value)
            {
                working = scaled = ResizeToMaxEdge(bitmap, options.MaxEdgePx.Value);
            }

            WriteOutput(output, Encode(working, ext, options));
            return output;
        }
        finally
        {
            scaled?.Dispose();
        }
    }

    public string WebpToImage(string sourceFile, string outputDirectory, string format = "png")
    {
        // legacy Util.ConvertWeBp2Image 的 JpegOptions.Quality 固定 100。
        return Convert(
            sourceFile,
            outputDirectory,
            new ImageEncodeOptions
            {
                Format = format,
                JpegQuality = 100,
                PngColor = ImagePngColorType.TruecolorWithAlpha,
                // 目标格式由「输出格式」决定，不能沿用源的 .webp 后缀。
                UseSourceExtension = false
            });
    }

    public string ToIco(string sourceFile, string outputDirectory, int size = 32)
    {
        size = Math.Clamp(size, 16, 256);
        return ToIco(sourceFile, outputDirectory, new[] { size }, $"_{size}.ico");
    }

    public string ToIco(string sourceFile, string outputDirectory, IReadOnlyList<int> sizes)
    {
        ArgumentNullException.ThrowIfNull(sizes);
        return ToIco(sourceFile, outputDirectory, sizes, "_multi.ico");
    }

    private string ToIco(
        string sourceFile,
        string outputDirectory,
        IReadOnlyList<int> sizes,
        string fileNameSuffix)
    {
        GuardSourcePath(sourceFile);
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        if (sizes.Count == 0)
        {
            throw new ArgumentException("尺寸集合不能为空。", nameof(sizes));
        }

        Directory.CreateDirectory(outputDirectory);

        int[] normalized = sizes
            .Select(s => Math.Clamp(s, 16, 256))
            .Distinct()
            .OrderBy(s => s)
            .ToArray();

        using var bitmap = DecodeImage(sourceFile);

        var payloads = new List<byte[]>(normalized.Length);
        foreach (int size in normalized)
        {
            using var square = bitmap.Resize(new SKImageInfo(size, size), SKSamplingOptions.Default)
                ?? throw new InvalidOperationException($"缩放失败（{bitmap.Width}x{bitmap.Height} → {size}x{size}）：{Path.GetFileName(sourceFile)}");
            payloads.Add(EncodePng(square, new ImageEncodeOptions
            {
                PngColor = ImagePngColorType.TruecolorWithAlpha,
                PngLevel = 9
            }));
        }

        string output = Path.Combine(
            outputDirectory,
            Path.GetFileNameWithoutExtension(sourceFile) + fileNameSuffix);

        WriteOutput(output, BuildPngIco(payloads, normalized));
        return output;
    }

    private static byte[] Encode(SKBitmap bitmap, string ext, ImageEncodeOptions options)
    {
        return ext switch
        {
            ".jpg" or ".jpeg" => EncodeJpeg(bitmap, options),
            ".bmp" => EncodeBmp(bitmap, options),
            ".webp" => EncodeWebp(bitmap, options),
            ".tif" or ".tiff" => EncodeTiff(bitmap, options),
            ".gif" => EncodeGif(bitmap),
            _ => EncodePng(bitmap, options)
        };
    }

    // SkiaSharp 3.x 没有 TIFF/GIF 编码器（SKEncodedImageFormat 无 Tif，Gif 的 Encode 返回 null），
    // 沿用 BMP 的做法自写字节；legacy 由 Aspose.Imaging 的 TiffOptions / GifOptions 输出。
    private static byte[] EncodeTiff(SKBitmap bitmap, ImageEncodeOptions options)
        => TiffEncoder.Encode(bitmap, options.TiffCompression);

    private static byte[] EncodeGif(SKBitmap bitmap) => GifEncoder.Encode(bitmap);

    private static byte[] EncodeJpeg(SKBitmap bitmap, ImageEncodeOptions options)
    {
        int quality = Math.Clamp(options.JpegQuality <= 0 ? 80 : options.JpegQuality, 1, 100);

        SKBitmap? gray = null;
        try
        {
            SKBitmap source = bitmap;
            if (options.JpegGrayscale)
            {
                gray = bitmap.Copy(SKColorType.Gray8);
                if (gray is not null)
                {
                    source = gray;
                }
            }

            using SKPixmap pixmap = source.PeekPixels()
                ?? throw new InvalidOperationException("无法读取像素，编码失败：.jpg");
            using SKData data = pixmap.Encode(new SKJpegEncoderOptions(quality))
                ?? throw new InvalidOperationException("编码失败：.jpg");
            return data.ToArray();
        }
        finally
        {
            gray?.Dispose();
        }
    }

    private static byte[] EncodeWebp(SKBitmap bitmap, ImageEncodeOptions options)
    {
        float quality = Math.Clamp(options.JpegQuality <= 0 ? 80 : options.JpegQuality, 1, 100);

        using SKPixmap pixmap = bitmap.PeekPixels()
            ?? throw new InvalidOperationException("无法读取像素，编码失败：.webp");
        using SKData data = pixmap.Encode(
            new SKWebpEncoderOptions(SKWebpEncoderCompression.Lossy, quality))
            ?? throw new InvalidOperationException("编码失败：.webp");
        return data.ToArray();
    }

    private static byte[] EncodePng(SKBitmap bitmap, ImageEncodeOptions options)
    {
        SKColorType target = options.PngColor switch
        {
            ImagePngColorType.Grayscale => SKColorType.Gray8,
            ImagePngColorType.Truecolor => SKColorType.Rgb888x,
            _ => SKColorType.Rgba8888
        };

        SKBitmap? converted = bitmap.ColorType == target ? null : bitmap.Copy(target);
        SKBitmap source = converted ?? bitmap;
        try
        {
            using SKPixmap pixmap = source.PeekPixels()
                ?? throw new InvalidOperationException("无法读取像素，编码失败：.png");
            int level = Math.Clamp(options.PngLevel, 0, 9);
            var pngOptions = new SKPngEncoderOptions(SKPngEncoderFilterFlags.AllFilters, level);
            using SKData data = pixmap.Encode(pngOptions)
                ?? throw new InvalidOperationException("编码失败：.png");
            return data.ToArray();
        }
        finally
        {
            converted?.Dispose();
        }
    }

    /// <summary>
    /// SkiaSharp 3.119 不能编码 BMP（<c>Encode(Bmp)</c> 返回 null），这里手写未压缩 BMP。
    /// 支持 legacy <c>CompressBmpPixeBits_Combox</c> 的 4/8/16/24/32 位：
    /// 4 位用 16 色固定调色板、8 位用 3-3-2 立方调色板（按最近色索引），16 位为 RGB555，
    /// 24/32 位为 BI_RGB 直存（32 位多一个保留字节）。调色板量化是最近色匹配，不抖动。
    /// </summary>
    private static byte[] EncodeBmp(SKBitmap bitmap, ImageEncodeOptions options)
    {
        int bits = NormalizeBmpBits(options.BmpPixelBits);
        SKBitmap? bgra = bitmap.ColorType == SKColorType.Bgra8888
            ? null
            : bitmap.Copy(SKColorType.Bgra8888);
        SKBitmap source = bgra ?? bitmap;
        try
        {
            int width = source.Width;
            int height = source.Height;
            int paletteCount = bits switch { 4 => 16, 8 => 256, _ => 0 };
            int paletteSize = paletteCount * 4;
            int stride = (width * bits + 31) / 32 * 4;
            int imageSize = stride * height;
            int headerSize = 14 + 40;
            int pixelOffset = headerSize + paletteSize;
            (byte R, byte G, byte B)[]? palette = paletteCount > 0 ? BuildPalette(paletteCount) : null;

            using SKPixmap pixmap = source.PeekPixels()
                ?? throw new InvalidOperationException("无法读取像素，编码失败：.bmp");
            ReadOnlySpan<byte> pixels = pixmap.GetPixelSpan();
            int sourceStride = pixmap.RowBytes;

            byte[] buffer = new byte[pixelOffset + imageSize];
            buffer[0] = (byte)'B';
            buffer[1] = (byte)'M';
            WriteInt32(buffer, 2, buffer.Length);
            WriteInt32(buffer, 10, pixelOffset);
            WriteInt32(buffer, 14, 40);
            WriteInt32(buffer, 18, width);
            WriteInt32(buffer, 22, height);
            WriteInt16(buffer, 26, 1);
            WriteInt16(buffer, 28, bits);
            WriteInt32(buffer, 30, 0); // BI_RGB
            WriteInt32(buffer, 34, imageSize);
            WriteInt32(buffer, 38, 2835); // 72 DPI
            WriteInt32(buffer, 42, 2835);
            WriteInt32(buffer, 46, paletteCount);
            WriteInt32(buffer, 50, 0);

            if (palette is not null)
            {
                for (int i = 0; i < paletteCount; i++)
                {
                    int dst = headerSize + i * 4;
                    buffer[dst] = palette[i].B;
                    buffer[dst + 1] = palette[i].G;
                    buffer[dst + 2] = palette[i].R;
                }
            }

            for (int y = 0; y < height; y++)
            {
                int srcRow = (height - 1 - y) * sourceStride;
                int dstRow = pixelOffset + y * stride;
                for (int x = 0; x < width; x++)
                {
                    int src = srcRow + x * 4;
                    byte b = pixels[src];
                    byte g = pixels[src + 1];
                    byte r = pixels[src + 2];

                    switch (bits)
                    {
                        case 32:
                            int dst32 = dstRow + x * 4;
                            buffer[dst32] = b;
                            buffer[dst32 + 1] = g;
                            buffer[dst32 + 2] = r;
                            break;

                        case 24:
                            int dst24 = dstRow + x * 3;
                            buffer[dst24] = b;
                            buffer[dst24 + 1] = g;
                            buffer[dst24 + 2] = r;
                            break;

                        case 16:
                            ushort rgb555 = (ushort)(((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3));
                            int dst16 = dstRow + x * 2;
                            buffer[dst16] = (byte)(rgb555 & 0xFF);
                            buffer[dst16 + 1] = (byte)(rgb555 >> 8);
                            break;

                        case 8:
                        case 4:
                            int index = NearestIndex(palette!, r, g, b);
                            if (bits == 8)
                            {
                                buffer[dstRow + x] = (byte)index;
                            }
                            else
                            {
                                int dst4 = dstRow + (x / 2);
                                buffer[dst4] = x % 2 == 0
                                    ? (byte)((buffer[dst4] & 0x0F) | (index << 4))
                                    : (byte)((buffer[dst4] & 0xF0) | index);
                            }

                            break;
                    }
                }
            }

            return buffer;
        }
        finally
        {
            bgra?.Dispose();
        }
    }

    private static int NormalizeBmpBits(int bits)
        => bits switch
        {
            4 => 4,
            8 => 8,
            16 => 16,
            32 => 32,
            _ => 24
        };

    /// <summary>4 位调色板用 EGA 16 色；8 位用 3-3-2 立方（R3 G3 B2）。</summary>
    private static (byte R, byte G, byte B)[] BuildPalette(int count)
    {
        var colors = new (byte R, byte G, byte B)[count];
        if (count == 16)
        {
            (byte, byte, byte)[] ega =
            [
                (0, 0, 0), (128, 0, 0), (0, 128, 0), (128, 128, 0),
                (0, 0, 128), (128, 0, 128), (0, 128, 128), (192, 192, 192),
                (128, 128, 128), (255, 0, 0), (0, 255, 0), (255, 255, 0),
                (0, 0, 255), (255, 0, 255), (0, 255, 255), (255, 255, 255)
            ];
            Array.Copy(ega, colors, 16);
            return colors;
        }

        for (int index = 0; index < count; index++)
        {
            int r = (index >> 5) & 0x07;
            int g = (index >> 2) & 0x07;
            int b = index & 0x03;
            colors[index] = ((byte)(r * 255 / 7), (byte)(g * 255 / 7), (byte)(b * 255 / 3));
        }

        return colors;
    }

    /// <summary>最近色匹配（欧氏距离平方）。</summary>
    private static int NearestIndex((byte R, byte G, byte B)[] palette, byte r, byte g, byte b)
    {
        int best = 0;
        int bestDistance = int.MaxValue;
        for (int i = 0; i < palette.Length; i++)
        {
            int dr = r - palette[i].R;
            int dg = g - palette[i].G;
            int db = b - palette[i].B;
            int distance = (dr * dr) + (dg * dg) + (db * db);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
            }
        }

        return best;
    }

    private static SKBitmap ResizeToMaxEdge(SKBitmap bitmap, int maxEdgePx)
    {
        int w = bitmap.Width;
        int h = bitmap.Height;
        int edge = Math.Max(w, h);
        double scale = (double)maxEdgePx / edge;
        int nw = Math.Max(1, (int)Math.Round(w * scale));
        int nh = Math.Max(1, (int)Math.Round(h * scale));
        return bitmap.Resize(new SKImageInfo(nw, nh), SKSamplingOptions.Default)
            ?? throw new InvalidOperationException($"缩放失败（{w}x{h} → {nw}x{nh}，最长边限制 {maxEdgePx}px）。");
    }

    private static string NormalizeFormat(string format)
    {
        string f = (format ?? "png").Trim().TrimStart('.').ToLowerInvariant();
        return f switch
        {
            "jpg" => ".jpg",
            // dev 的 webp 输出格式表把 JPG(.jpg) 与 JPEG(.jpeg) 列成两行（BodyForm.cs:1423-1424），
            // 两者编码参数相同（JpegQuality=100），只有后缀不同。
            "jpeg" => ".jpeg",
            "bmp" => ".bmp",
            "webp" => ".webp",
            "tif" or "tiff" => ".tif",
            "gif" => ".gif",
            "ico" => ".png",
            _ => ".png"
        };
    }

    /// <summary>ICO container holding one PNG payload per size (Vista+ layout).</summary>
    private static byte[] BuildPngIco(IReadOnlyList<byte[]> payloads, IReadOnlyList<int> sizes)
    {
        int count = payloads.Count;
        int headerSize = 6 + 16 * count;

        using var ms = new MemoryStream(headerSize + payloads.Sum(p => p.Length));
        using var bw = new BinaryWriter(ms);
        bw.Write((ushort)0); // reserved
        bw.Write((ushort)1); // type = icon
        bw.Write((ushort)count);

        int offset = headerSize;
        for (int i = 0; i < count; i++)
        {
            int size = sizes[i];
            bw.Write((byte)(size >= 256 ? 0 : size)); // width (0 means 256)
            bw.Write((byte)(size >= 256 ? 0 : size)); // height
            bw.Write((byte)0);  // color count
            bw.Write((byte)0);  // reserved
            bw.Write((ushort)1);  // planes
            bw.Write((ushort)32); // bit count
            bw.Write(payloads[i].Length);
            bw.Write(offset);
            offset += payloads[i].Length;
        }

        foreach (byte[] payload in payloads)
        {
            bw.Write(payload);
        }

        bw.Flush();
        return ms.ToArray();
    }

    private static void WriteInt16(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)(value & 0xFF);
        buffer[offset + 1] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 3] = (byte)((value >> 24) & 0xFF);
    }
}
