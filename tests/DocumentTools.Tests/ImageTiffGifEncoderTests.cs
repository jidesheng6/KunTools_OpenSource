using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Image;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// TIF/GIF 输出编码器。<see cref="SKEncodedImageFormat"/> 里没有 Tif，Gif 的 Encode 返回 null，
/// 所以 TIF/GIF 由 Core 自写字节（同 BMP 的做法）。
/// SkiaSharp 的 TIFF 解码器在 3.119 版本不存在（<c>SKCodec.Create</c> 返回 null），
/// 因此 TIFF 用例改为「按 TIFF 6.0 规范反向解析自己写出的字节」，GIF 用例仍用 SkiaSharp 解码回读。
/// </summary>
public class ImageTiffGifEncoderTests
{
    private static readonly ImageConversionService Service = new();

    [Fact]
    public void Tiff_WritesLittleEndianHeaderAndBaselineIfd()
    {
        Run(dir =>
        {
            string source = CreateSolidPng(dir, 24, 16, new SKColor(255, 0, 0));
            string output = Service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = TiffCompression.None
            });

            Assert.EndsWith("_out.tif", output, StringComparison.Ordinal);
            byte[] tiff = File.ReadAllBytes(output);

            Assert.Equal(0x49, tiff[0]);
            Assert.Equal(0x49, tiff[1]);
            Assert.Equal(42, ReadInt16(tiff, 2));

            int ifd = ReadInt32(tiff, 4);
            Assert.Equal(8, ifd);

            Dictionary<int, (int Type, int Count, int Value)> entries = ReadEntries(tiff, ifd);
            Assert.Equal(13, entries.Count);

            Assert.Equal(3, entries[256].Type);
            Assert.Equal(1, entries[256].Count);
            Assert.Equal(24, entries[256].Value);
            Assert.Equal(16, entries[257].Value);
            Assert.Equal(3, entries[258].Type);      // BitsPerSample：3 个 SHORT 放在外部数组
            Assert.Equal(3, entries[258].Count);
            Assert.Equal(1, entries[259].Value);     // Compression = 未压缩
            Assert.Equal(2, entries[262].Value);     // Photometric = RGB
            Assert.Equal(3, entries[277].Value);     // SamplesPerPixel
            Assert.Equal(1, entries[284].Value);     // PlanarConfig = chunky
            Assert.Equal(24 * 3 * 16, entries[279].Value); // StripByteCounts

            int bits = entries[258].Value;
            Assert.Equal(8, ReadInt16(tiff, bits));
            Assert.Equal(8, ReadInt16(tiff, bits + 2));
            Assert.Equal(8, ReadInt16(tiff, bits + 4));

            // 条目按 tag 升序，且下一个 IFD 指针为 0（单页）。
            int tags = ifd + 2;
            int previous = 0;
            for (int i = 0; i < 13; i++)
            {
                int tag = ReadInt16(tiff, tags + (i * 12));
                Assert.True(tag > previous, $"IFD 条目必须按 tag 升序：{previous} -> {tag}");
                previous = tag;
            }

            Assert.Equal(0, ReadInt32(tiff, tags + (13 * 12)));
        });
    }

    [Fact]
    public void Tiff_PixelsReadBackAsOpaqueAndTransparentHandledLikeLegacy()
    {
        Run(dir =>
        {
            // 左上半蓝、左下半全透明、右半 50% 绿：TIFF 基线无 alpha，按白底合成。
            using var bitmap = new SKBitmap(new SKImageInfo(20, 20, SKColorType.Rgba8888, SKAlphaType.Unpremul));
            for (int y = 0; y < 20; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    SKColor color = x < 10
                        ? (y < 10 ? new SKColor(0, 0, 255, 255) : new SKColor(0, 0, 255, 0))
                        : new SKColor(0, 255, 0, 128);
                    bitmap.SetPixel(x, y, color);
                }
            }

            string source = SavePng(dir, bitmap, "alpha.png");
            string output = Service.Convert(source, dir, new ImageEncodeOptions { Format = "tiff" });
            (int width, int height, byte[] rgb) = ReadTiffPixels(File.ReadAllBytes(output));

            Assert.Equal(20, width);
            Assert.Equal(20, height);
            Assert.Equal((255, 255, 255), Pixel(rgb, width, 2, 17));    // 全透明 → 白
            Assert.Equal((0, 0, 255), Pixel(rgb, width, 2, 2));         // 不透明 → 原色
            (int r, int g, int b) = Pixel(rgb, width, 17, 2);           // 50% 绿 → 与白底混合
            Assert.InRange(r, 120, 135);
            Assert.InRange(g, 185, 200);
            Assert.InRange(b, 120, 135);
        });
    }

    [Fact]
    public void Gif_Writes89aHeaderAndSkiaDecodesItBack()
    {
        Run(dir =>
        {
            string source = CreateSolidPng(dir, 24, 16, new SKColor(255, 0, 0));
            string output = Service.Convert(source, dir, new ImageEncodeOptions { Format = "gif" });

            Assert.EndsWith("_out.gif", output, StringComparison.Ordinal);
            byte[] gif = File.ReadAllBytes(output);

            Assert.Equal("GIF89a", System.Text.Encoding.ASCII.GetString(gif, 0, 6));
            Assert.Equal(24, ReadInt16(gif, 6));
            Assert.Equal(16, ReadInt16(gif, 8));
            Assert.Equal(0xF7, gif[10]);   // 全局调色板存在 + 256 项

            using SKData data = SKData.CreateCopy(gif);
            using SKCodec? codec = SKCodec.Create(data);
            Assert.NotNull(codec);
            using var decoded = new SKBitmap(codec!.Info);
            Assert.Equal(SKCodecResult.Success, codec.GetPixels(decoded.Info, decoded.GetPixels()));
            Assert.Equal(24, decoded.Width);
            Assert.Equal(16, decoded.Height);
            Assert.Equal(new SKColor(255, 0, 0), decoded.GetPixel(5, 5));
        });
    }

    [Fact]
    public void Gif_QuantizesMoreThan256Colours_AndStillDecodes()
    {
        Run(dir =>
        {
            using var bitmap = new SKBitmap(new SKImageInfo(40, 30, SKColorType.Rgba8888, SKAlphaType.Premul));
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 40; x++)
                {
                    bitmap.SetPixel(x, y, new SKColor((byte)(x * 6), (byte)(y * 8), (byte)((x * y) % 256)));
                }
            }

            string source = SavePng(dir, bitmap, "grad.png");
            string output = Service.Convert(source, dir, new ImageEncodeOptions { Format = "gif" });

            // 1200 像素远超 256 色，必须走中位切分 + 码长增长的 LZW 路径，且解码器要认账。
            using SKData data = SKData.CreateCopy(File.ReadAllBytes(output));
            using SKCodec? codec = SKCodec.Create(data);
            Assert.NotNull(codec);
            using var decoded = new SKBitmap(codec!.Info);
            Assert.Equal(SKCodecResult.Success, codec.GetPixels(decoded.Info, decoded.GetPixels()));
            Assert.Equal(40, decoded.Width);
            Assert.Equal(30, decoded.Height);

            // 量化后左上角仍是深色（中位切分的调色板取盒内均值，不保证逐像素相等）。
            SKColor topLeft = decoded.GetPixel(0, 0);
            Assert.InRange(topLeft.Red, 0, 40);
            Assert.InRange(topLeft.Green, 0, 40);
            Assert.InRange(topLeft.Blue, 0, 40);
        });
    }

    [Fact]
    public void Convert_TiffAlias_KeepsTifExtension()
    {
        Run(dir =>
        {
            string source = CreateSolidPng(dir, 12, 8, new SKColor(10, 20, 30));
            string output = Service.Convert(source, dir, new ImageEncodeOptions { Format = "TIFF" });

            Assert.EndsWith("_out.tif", output, StringComparison.Ordinal);
            Assert.Equal(0x49, File.ReadAllBytes(output)[0]);
        });
    }

    [Fact]
    public void ImageConvertOperation_TifFormat_DoesNotSilentlyFallBackToPng()
    {
        Run(dir =>
        {
            string source = CreateSolidPng(dir, 12, 8, new SKColor(10, 20, 30));

            DocumentOperationResult result = new ImageConvertOperation("image.compress", Service)
                .ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = [source],
                    OutputDirectory = dir,
                    Options = new Dictionary<string, string>
                    {
                        ["useSourceExtension"] = "false",
                        ["format"] = "TIF"
                    }
                }).GetAwaiter().GetResult();

            Assert.True(result.Success, result.Message);
            string output = Assert.Single(result.OutputFiles);
            Assert.EndsWith("_out.tif", output, StringComparison.Ordinal);

            byte[] bytes = File.ReadAllBytes(output);
            Assert.Equal(new byte[] { 0x49, 0x49, 0x2A, 0x00 }, bytes[..4]);
            Assert.NotEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, bytes[..4]);
        });
    }

    [Fact]
    public void ImageConvertOperation_GifFormat_DoesNotSilentlyFallBackToPng()
    {
        Run(dir =>
        {
            string source = CreateSolidPng(dir, 12, 8, new SKColor(10, 20, 30));

            DocumentOperationResult result = new ImageConvertOperation("image.compress", Service)
                .ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = [source],
                    OutputDirectory = dir,
                    Options = new Dictionary<string, string>
                    {
                        ["useSourceExtension"] = "false",
                        ["format"] = "GIF"
                    }
                }).GetAwaiter().GetResult();

            Assert.True(result.Success, result.Message);
            string output = Assert.Single(result.OutputFiles);
            Assert.EndsWith("_out.gif", output, StringComparison.Ordinal);
            Assert.Equal("GIF89a", System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(output), 0, 6));
        });
    }

    [Fact]
    public void Schema_FormatChoiceOffersTifAndGif()
    {
        OptionField format = FeatureOptionSchemas.GetSections("image.compress")
            .SelectMany(s => s.Fields)
            .Single(f => f.Key == "format");

        string[] values = [.. format.Choices!.Select(c => c.Value)];
        Assert.Contains("TIF", values);
        Assert.Contains("TIFF", values);
        Assert.Contains("GIF", values);
        Assert.Equal("PNG", format.DefaultValue);

        // 输入过滤器也要放行 GIF，否则「GIF 可以转出去」但「GIF 转不进来」。
        Assert.Contains("*.gif", FeatureOptionSchemas.GetFileFilter("image.compress"), StringComparison.Ordinal);
    }

    [Fact]
    public void WebpToImage_GifOutput_UsesGifEncoder()
    {
        Run(dir =>
        {
            using var bitmap = new SKBitmap(new SKImageInfo(16, 12, SKColorType.Rgba8888, SKAlphaType.Premul));
            using (var canvas = new SKCanvas(bitmap))
            {
                canvas.Clear(new SKColor(0, 128, 255));
            }

            string webp = Path.Combine(dir, "src.webp");
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Webp, 100))
            using (var fs = File.Open(webp, FileMode.Create, FileAccess.Write))
            {
                data.SaveTo(fs);
            }

            string output = Service.WebpToImage(webp, dir, "gif");
            Assert.EndsWith("_out.gif", output, StringComparison.Ordinal);

            byte[] bytes = File.ReadAllBytes(output);
            Assert.Equal("GIF89a", System.Text.Encoding.ASCII.GetString(bytes, 0, 6));

            using SKData encoded = SKData.CreateCopy(bytes);
            using SKCodec? codec = SKCodec.Create(encoded);
            Assert.NotNull(codec);
            using var decoded = new SKBitmap(codec!.Info);
            Assert.Equal(SKCodecResult.Success, codec.GetPixels(decoded.Info, decoded.GetPixels()));
            Assert.Equal(new SKColor(0, 128, 255), decoded.GetPixel(4, 4));
        });
    }

    [Fact]
    public void Schema_WebpImage_OffersGifOutput()
    {
        OptionField format = FeatureOptionSchemas.GetSections("webp.image")
            .SelectMany(s => s.Fields)
            .Single(f => f.Key == "format");

        Assert.Contains("GIF", format.Choices!.Select(c => c.Value));
    }

    [Fact]
    public void Tiff_DefaultCompression_IsLzw()
    {
        Run(dir =>
        {
            string source = CreateBandedPng(dir, 160, 120);
            string output = Service.Convert(source, SubDirectory(dir, "out"), new ImageEncodeOptions { Format = "tif" });
            byte[] tiff = File.ReadAllBytes(output);

            Dictionary<int, (int Type, int Count, int Value)> entries = ReadEntries(tiff, ReadInt32(tiff, 4));
            Assert.Equal(5, entries[259].Value);   // LZW
        });
    }

    [Fact]
    public void Tiff_PackBits_ShrinksBandedImageByAtLeast30Percent_AndKeepsPixels()
    {
        Run(dir =>
        {
            string source = CreateBandedPng(dir, 200, 200);

            byte[] none = File.ReadAllBytes(Service.Convert(source, SubDirectory(dir, "none"), new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = TiffCompression.None
            }));
            byte[] packed = File.ReadAllBytes(Service.Convert(source, SubDirectory(dir, "packbits"), new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = TiffCompression.PackBits
            }));

            Assert.True(packed.Length * 10 < none.Length * 7, $"PackBits 未省下 30%：{none.Length} -> {packed.Length}");
            Assert.Equal(ReadTiffPixels(none).Rgb, ReadTiffPixels(packed).Rgb);
        });
    }

    [Fact]
    public void Tiff_Lzw_ShrinksBandedImageByAtLeastHalf_AndKeepsPixels()
    {
        Run(dir =>
        {
            string source = CreateBandedPng(dir, 200, 200);

            byte[] none = File.ReadAllBytes(Service.Convert(source, SubDirectory(dir, "none"), new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = TiffCompression.None
            }));
            byte[] lzw = File.ReadAllBytes(Service.Convert(source, SubDirectory(dir, "lzw"), new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = TiffCompression.Lzw
            }));

            Assert.True(lzw.Length * 2 < none.Length, $"LZW 未省下一半：{none.Length} -> {lzw.Length}");
            Assert.Equal(ReadTiffPixels(none).Rgb, ReadTiffPixels(lzw).Rgb);
        });
    }

    [Fact]
    public void Tiff_SplitsStripsBySixtyFourKilobyteTarget()
    {
        Run(dir =>
        {
            string source = CreateBandedPng(dir, 400, 600);
            byte[] tiff = File.ReadAllBytes(Service.Convert(source, SubDirectory(dir, "out"), new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = TiffCompression.None
            }));

            Dictionary<int, (int Type, int Count, int Value)> entries = ReadEntries(tiff, ReadInt32(tiff, 4));
            int width = entries[256].Value;
            int height = entries[257].Value;
            int rowBytes = width * 3;
            int rowsPerStrip = entries[278].Value;

            // 65536 / 1200 = 54 行/条，夹在 [8, 256] 之间。
            Assert.Equal(65536 / rowBytes, rowsPerStrip);
            Assert.Equal(4, entries[273].Type);              // StripOffsets 为 LONG 数组
            Assert.Equal(4, entries[279].Type);              // StripByteCounts 为 LONG 数组

            int[] offsets = ReadLongArray(tiff, entries[273]);
            int[] counts = ReadLongArray(tiff, entries[279]);
            int expectedStrips = (height + rowsPerStrip - 1) / rowsPerStrip;
            Assert.True(expectedStrips > 1, "400x600 应该切成多条带");
            Assert.Equal(expectedStrips, offsets.Length);
            Assert.Equal(expectedStrips, counts.Length);

            for (int i = 0; i < offsets.Length; i++)
            {
                int rows = Math.Min(rowsPerStrip, height - (i * rowsPerStrip));
                Assert.Equal(rows * rowBytes, counts[i]);
                Assert.True(offsets[i] + counts[i] <= tiff.Length, $"第 {i} 条带越界");
            }

            (int decodedWidth, int decodedHeight, byte[] rgb) = ReadTiffPixels(tiff);
            Assert.Equal((width, height), (decodedWidth, decodedHeight));
            Assert.Equal(width * 3 * height, rgb.Length);
        });
    }

    [Fact]
    public void Tiff_TinyImage_IsSingleStrip()
    {
        Run(dir =>
        {
            string source = CreateSolidPng(dir, 4, 4, new SKColor(10, 20, 30));
            byte[] tiff = File.ReadAllBytes(Service.Convert(source, SubDirectory(dir, "out"), new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = TiffCompression.None
            }));

            Dictionary<int, (int Type, int Count, int Value)> entries = ReadEntries(tiff, ReadInt32(tiff, 4));
            Assert.Equal(1, entries[279].Count);              // 单条带：数值内联，不建外部数组
            Assert.Equal(4 * 3 * 4, entries[279].Value);
            Assert.Equal(4, entries[278].Value);              // RowsPerStrip 被夹到图像高度
        });
    }

    [Theory]
    [InlineData(TiffCompression.None)]
    [InlineData(TiffCompression.PackBits)]
    [InlineData(TiffCompression.Lzw)]
    public void Tiff_AllCompressions_RoundTripToSamePixels(TiffCompression compression)
    {
        Run(dir =>
        {
            string source = CreateBandedPng(dir, 61, 37);
            byte[] tiff = File.ReadAllBytes(Service.Convert(source, SubDirectory(dir, "out"), new ImageEncodeOptions
            {
                Format = "tif",
                TiffCompression = compression
            }));

            (int width, int height, byte[] rgb) = ReadTiffPixels(tiff);
            Assert.Equal((61, 37), (width, height));

            for (int y = 0; y < height; y++)
            {
                byte value = BandValue(y);
                for (int x = 0; x < width; x++)
                {
                    Assert.Equal((value, value, value), Pixel(rgb, width, x, y));
                }
            }
        });
    }

    /// <summary>每 8 行一个灰度台阶：24bpp 交错后仍有长串重复字节，便于对比三种压缩。</summary>
    private static byte BandValue(int y) => (byte)((y / 8 * 3) % 256);

    private static string CreateBandedPng(string directory, int width, int height)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        for (int y = 0; y < height; y++)
        {
            byte value = BandValue(y);
            for (int x = 0; x < width; x++)
            {
                bitmap.SetPixel(x, y, new SKColor(value, value, value, 255));
            }
        }

        return SavePng(directory, bitmap, $"banded_{width}x{height}.png");
    }

    private static string SubDirectory(string root, string name)
    {
        string path = Path.Combine(root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static void Run(Action<string> body)
    {
        string dir = Directory.CreateTempSubdirectory("documenttools-tifgif-").FullName;
        try
        {
            body(dir);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    private static string CreateSolidPng(string directory, int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(color);
        }

        return SavePng(directory, bitmap, "solid.png");
    }

    private static string SavePng(string directory, SKBitmap bitmap, string name)
    {
        string path = Path.Combine(directory, name);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using (var fs = File.Open(path, FileMode.Create, FileAccess.Write))
        {
            data.SaveTo(fs);
        }

        return path;
    }

    /// <summary>按 TIFF 6.0 反向解析 IFD：tag → (类型, 数量, 值或偏移)。</summary>
    private static Dictionary<int, (int Type, int Count, int Value)> ReadEntries(byte[] tiff, int ifd)
    {
        int count = ReadInt16(tiff, ifd);
        var entries = new Dictionary<int, (int, int, int)>(count);
        for (int i = 0; i < count; i++)
        {
            int at = ifd + 2 + (i * 12);
            int tag = ReadInt16(tiff, at);
            int type = ReadInt16(tiff, at + 2);
            int valueCount = ReadInt32(tiff, at + 4);
            int value = type == 3 && valueCount == 1 ? ReadInt16(tiff, at + 8) : ReadInt32(tiff, at + 8);
            entries[tag] = (type, valueCount, value);
        }

        return entries;
    }

    /// <summary>
    /// 按 TIFF 6.0 反向解析并解压像素（None / PackBits / LZW 都支持，多条带按 StripOffsets 拼接）。
    /// SkiaSharp 3.119 没有 TIFF 解码器，所以这里自己解，作为产物级断言。
    /// </summary>
    private static (int Width, int Height, byte[] Rgb) ReadTiffPixels(byte[] tiff)
    {
        Assert.Equal(0x49, tiff[0]);
        Assert.Equal(0x49, tiff[1]);
        Assert.Equal(42, ReadInt16(tiff, 2));

        Dictionary<int, (int Type, int Count, int Value)> entries = ReadEntries(tiff, ReadInt32(tiff, 4));
        int width = entries[256].Value;
        int height = entries[257].Value;
        Assert.Equal(3, entries[277].Value);   // SamplesPerPixel

        int compression = entries[259].Value;
        int[] offsets = ReadLongArray(tiff, entries[273]);
        int[] byteCounts = ReadLongArray(tiff, entries[279]);
        Assert.Equal(offsets.Length, byteCounts.Length);

        byte[] rgb = new byte[width * 3 * height];
        int written = 0;
        for (int s = 0; s < offsets.Length; s++)
        {
            Assert.True(offsets[s] + byteCounts[s] <= tiff.Length, "StripOffsets 超出文件长度");
            byte[] strip = tiff[offsets[s]..(offsets[s] + byteCounts[s])];
            byte[] raw = compression switch
            {
                1 => strip,
                5 => LzwDecode(strip),
                32773 => PackBitsDecode(strip),
                _ => throw new InvalidOperationException($"测试解码器不支持压缩方式 {compression}")
            };

            raw.CopyTo(rgb, written);
            written += raw.Length;
        }

        Assert.Equal(rgb.Length, written);
        return (width, height, rgb);
    }

    /// <summary>读 tag 的 LONG 数组（count == 1 时数值内联在值域里）。</summary>
    private static int[] ReadLongArray(byte[] tiff, (int Type, int Count, int Value) entry)
    {
        if (entry.Count == 1)
        {
            return [entry.Value];
        }

        var values = new int[entry.Count];
        for (int i = 0; i < entry.Count; i++)
        {
            values[i] = ReadInt32(tiff, entry.Value + (i * 4));
        }

        return values;
    }

    /// <summary>PackBits 解压（TIFF 6.0 / Macintosh RLE）。</summary>
    private static byte[] PackBitsDecode(byte[] data)
    {
        var output = new List<byte>(data.Length * 2);
        int i = 0;
        while (i < data.Length)
        {
            sbyte header = (sbyte)data[i++];
            if (header >= 0)
            {
                for (int k = 0; k <= header; k++)
                {
                    output.Add(data[i++]);
                }
            }
            else if (header != -128)
            {
                byte value = data[i++];
                for (int k = 0; k < 1 - header; k++)
                {
                    output.Add(value);
                }
            }
        }

        return [.. output];
    }

    /// <summary>TIFF LZW 解压：MSB 优先、clear 256 / EOI 257、9→12 位，与编码器同一加宽时机。</summary>
    private static byte[] LzwDecode(byte[] data)
    {
        var output = new List<byte>(data.Length * 2);
        var table = new List<byte[]>(4096);
        ResetTable(table);

        int bitPosition = 0;
        int codeWidth = 9;
        byte[]? previous = null;

        int ReadCode()
        {
            int code = 0;
            for (int b = 0; b < codeWidth; b++)
            {
                int byteIndex = bitPosition >> 3;
                int bitIndex = 7 - (bitPosition & 7);
                if (byteIndex < data.Length && ((data[byteIndex] >> bitIndex) & 1) != 0)
                {
                    code |= 1 << (codeWidth - 1 - b);
                }

                bitPosition++;
            }

            return code;
        }

        while (bitPosition + codeWidth <= data.Length * 8)
        {
            int code = ReadCode();
            if (code == 256)
            {
                ResetTable(table);
                codeWidth = 9;
                previous = null;
                continue;
            }

            if (code == 257)
            {
                break;
            }

            byte[] entry;
            if (code < table.Count)
            {
                entry = table[code];
            }
            else if (code == table.Count && previous is not null)
            {
                entry = [.. previous, previous[0]];
            }
            else
            {
                throw new InvalidOperationException($"LZW 流损坏：码 {code} 不在表内（表大小 {table.Count}）");
            }

            output.AddRange(entry);
            if (previous is not null && table.Count < 4096)
            {
                table.Add([.. previous, entry[0]]);
                if (table.Count + 1 >= (1 << codeWidth) && codeWidth < 12)
                {
                    codeWidth++;
                }
            }

            previous = entry;
        }

        return [.. output];
    }

    private static void ResetTable(List<byte[]> table)
    {
        table.Clear();
        for (int i = 0; i < 256; i++)
        {
            table.Add([(byte)i]);
        }

        table.Add([]);   // 256 = clear（占位）
        table.Add([]);   // 257 = EOI（占位）
    }

    private static (int R, int G, int B) Pixel(byte[] rgb, int width, int x, int y)
    {
        int at = ((y * width) + x) * 3;
        return (rgb[at], rgb[at + 1], rgb[at + 2]);
    }

    private static int ReadInt16(byte[] buffer, int offset)
        => buffer[offset] | (buffer[offset + 1] << 8);

    private static int ReadInt32(byte[] buffer, int offset)
        => buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);
}
