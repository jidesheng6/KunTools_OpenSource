using SkiaSharp;

namespace DocumentTools.Core.Services.Image;

/// <summary>
/// 最小可用的单帧 GIF89a 编码器（8bpp 索引色 + LZW）。
/// SkiaSharp 3.x 不含 GIF 编码器（<c>SKEncodedImageFormat.Gif</c> 的 Encode 返回 null），
/// legacy 走的是 Aspose.Imaging 的 <c>GifOptions</c>；这里按 legacy 的「单帧」语义自己写字节，
/// 供 <see cref="ImageConversionService"/> 输出 .gif。
/// </summary>
internal static class GifEncoder
{
    private const int MaxPaletteSize = 256;
    private const int MinCodeSize = 8;      // 8bpp → 最小码长 8，clear = 256，end = 257
    private const int MaxCode = 4096;

    /// <summary>把位图编码成单帧 GIF89a 字节。</summary>
    /// <param name="bitmap">源位图；透明像素按白底合成（GIF 单帧输出不带透明索引）。</param>
    /// <returns>完整 GIF 文件字节；失败抛 <see cref="InvalidOperationException"/>。</returns>
    public static byte[] Encode(SKBitmap bitmap)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        if (bitmap.Width <= 0 || bitmap.Height <= 0)
        {
            throw new InvalidOperationException("编码失败：位图尺寸无效（.gif）。");
        }

        int width = bitmap.Width;
        int height = bitmap.Height;
        int pixelCount = width * height;

        byte[] reds = new byte[pixelCount];
        byte[] greens = new byte[pixelCount];
        byte[] blues = new byte[pixelCount];
        ReadPixels(bitmap, reds, greens, blues);

        (byte[] palette, byte[] indices) = Quantize(reds, greens, blues, pixelCount);

        using var ms = new MemoryStream(pixelCount / 2 + 1024);
        using var bw = new BinaryWriter(ms);

        // ── GIF89a 头 + 逻辑屏幕描述符（全局调色板 256 色）──
        bw.Write("GIF89a"u8);
        bw.Write((ushort)width);
        bw.Write((ushort)height);
        bw.Write((byte)0xF7);   // GCT 存在 + 8 位颜色分辨率 + GCT 大小 2^(7+1) = 256
        bw.Write((byte)0);      // 背景色索引
        bw.Write((byte)0);      // 像素宽高比

        for (int i = 0; i < MaxPaletteSize; i++)
        {
            int p = i * 3;
            bw.Write(i * 3 < palette.Length ? palette[p] : (byte)0);
            bw.Write(i * 3 < palette.Length ? palette[p + 1] : (byte)0);
            bw.Write(i * 3 < palette.Length ? palette[p + 2] : (byte)0);
        }

        // ── 图像描述符（无局部调色板、不交错）──
        bw.Write((byte)0x2C);
        bw.Write((ushort)0);
        bw.Write((ushort)0);
        bw.Write((ushort)width);
        bw.Write((ushort)height);
        bw.Write((byte)0);

        // ── LZW 图像数据 ──
        bw.Write((byte)MinCodeSize);
        byte[] compressed = CompressLzw(indices);
        int offset = 0;
        while (offset < compressed.Length)
        {
            int block = Math.Min(255, compressed.Length - offset);
            bw.Write((byte)block);
            bw.Write(compressed, offset, block);
            offset += block;
        }

        bw.Write((byte)0);      // 块终止符
        bw.Write((byte)0x3B);   // trailer
        bw.Flush();
        return ms.ToArray();
    }

    private static void ReadPixels(SKBitmap bitmap, byte[] reds, byte[] greens, byte[] blues)
    {
        SKBitmap? bgra = bitmap.ColorType == SKColorType.Bgra8888 ? null : bitmap.Copy(SKColorType.Bgra8888);
        SKBitmap source = bgra ?? bitmap;
        try
        {
            using SKPixmap pixmap = source.PeekPixels()
                ?? throw new InvalidOperationException("编码失败：无法读取像素（.gif）。");
            ReadOnlySpan<byte> pixels = pixmap.GetPixelSpan();
            int stride = pixmap.RowBytes;
            int width = source.Width;
            int height = source.Height;

            for (int y = 0; y < height; y++)
            {
                int srcRow = y * stride;
                int dstRow = y * width;
                for (int x = 0; x < width; x++)
                {
                    int src = srcRow + (x * 4);
                    byte a = pixels[src + 3];
                    int dst = dstRow + x;
                    reds[dst] = Composite(pixels[src + 2], a);
                    greens[dst] = Composite(pixels[src + 1], a);
                    blues[dst] = Composite(pixels[src], a);
                }
            }
        }
        finally
        {
            bgra?.Dispose();
        }
    }

    /// <summary>透明像素合成到白底：GIF 单帧不带透明索引，留黑会毁掉图标类素材。</summary>
    private static byte Composite(byte value, byte alpha)
        => alpha == 255 ? value : (byte)(((value * alpha) + (255 * (255 - alpha)) + 127) / 255);

    /// <summary>
    /// 生成调色板与索引：颜色数 ≤ 256 时直接用原色（无损），否则用中位切分（median cut）量化。
    /// </summary>
    private static (byte[] Palette, byte[] Indices) Quantize(byte[] reds, byte[] greens, byte[] blues, int pixelCount)
    {
        var histogram = new Dictionary<int, int>(pixelCount);
        for (int i = 0; i < pixelCount; i++)
        {
            int key = (reds[i] << 16) | (greens[i] << 8) | blues[i];
            histogram[key] = histogram.TryGetValue(key, out int count) ? count + 1 : 1;
        }

        List<int> colors = [.. histogram.Keys];
        byte[] palette;
        Dictionary<int, int>? exactIndex = null;

        if (colors.Count <= MaxPaletteSize)
        {
            palette = new byte[colors.Count * 3];
            exactIndex = new Dictionary<int, int>(colors.Count);
            for (int i = 0; i < colors.Count; i++)
            {
                int key = colors[i];
                palette[i * 3] = (byte)((key >> 16) & 0xFF);
                palette[i * 3 + 1] = (byte)((key >> 8) & 0xFF);
                palette[i * 3 + 2] = (byte)(key & 0xFF);
                exactIndex[key] = i;
            }
        }
        else
        {
            int[][] boxes = MedianCut(colors, histogram);
            palette = new byte[boxes.Length * 3];
            for (int b = 0; b < boxes.Length; b++)
            {
                (byte r, byte g, byte blue) = AverageColor(boxes[b], histogram);
                palette[b * 3] = r;
                palette[b * 3 + 1] = g;
                palette[b * 3 + 2] = blue;
            }
        }

        byte[] indices = new byte[pixelCount];
        int[] cache = new int[32768];
        Array.Fill(cache, -1);

        for (int i = 0; i < pixelCount; i++)
        {
            byte r = reds[i];
            byte g = greens[i];
            byte b = blues[i];

            if (exactIndex is not null)
            {
                int key = (r << 16) | (g << 8) | b;
                indices[i] = (byte)exactIndex[key];
                continue;
            }

            int cacheKey = ((r >> 3) << 10) | ((g >> 3) << 5) | (b >> 3);
            int index = cache[cacheKey];
            if (index < 0)
            {
                index = NearestColor(palette, r, g, b);
                cache[cacheKey] = index;
            }

            indices[i] = (byte)index;
        }

        return (palette, indices);
    }

    /// <summary>中位切分：反复把「颜色数最多的盒子」沿跨度最大的通道对半切，直到 ≤ 256 盒。</summary>
    private static int[][] MedianCut(List<int> colors, Dictionary<int, int> histogram)
    {
        var boxes = new List<List<int>> { colors };

        while (boxes.Count < MaxPaletteSize)
        {
            int target = -1;
            int targetSpan = 0;
            for (int i = 0; i < boxes.Count; i++)
            {
                if (boxes[i].Count < 2)
                {
                    continue;
                }

                int span = MaxChannelSpan(boxes[i]);
                if (span > targetSpan)
                {
                    target = i;
                    targetSpan = span;
                }
            }

            if (target < 0)
            {
                break;
            }

            List<int> box = boxes[target];
            var ordered = box.OrderBy(ColorChannelKey(ChannelOfMaxSpan(box))).ToList();
            int totalWeight = ordered.Sum(c => histogram[c]);
            int half = totalWeight / 2;
            int running = 0;
            int split = 1;
            for (int i = 0; i < ordered.Count - 1; i++)
            {
                running += histogram[ordered[i]];
                if (running >= half)
                {
                    split = i + 1;
                    break;
                }

                split = i + 2;
            }

            split = Math.Clamp(split, 1, ordered.Count - 1);
            boxes[target] = ordered.Take(split).ToList();
            boxes.Add(ordered.Skip(split).ToList());
        }

        return [.. boxes.Select(b => b.ToArray())];
    }

    private static Func<int, int> ColorChannelKey(int channel)
        => channel switch
        {
            0 => c => (c >> 16) & 0xFF,
            1 => c => (c >> 8) & 0xFF,
            _ => c => c & 0xFF
        };

    private static int ChannelOfMaxSpan(List<int> box)
    {
        int minR = 255, maxR = 0, minG = 255, maxG = 0, minB = 255, maxB = 0;
        foreach (int c in box)
        {
            int r = (c >> 16) & 0xFF;
            int g = (c >> 8) & 0xFF;
            int b = c & 0xFF;
            if (r < minR) { minR = r; }
            if (r > maxR) { maxR = r; }
            if (g < minG) { minG = g; }
            if (g > maxG) { maxG = g; }
            if (b < minB) { minB = b; }
            if (b > maxB) { maxB = b; }
        }

        int spanR = maxR - minR;
        int spanG = maxG - minG;
        int spanB = maxB - minB;
        return spanR >= spanG && spanR >= spanB ? 0 : (spanG >= spanB ? 1 : 2);
    }

    private static int MaxChannelSpan(List<int> box)
    {
        int minR = 255, maxR = 0, minG = 255, maxG = 0, minB = 255, maxB = 0;
        foreach (int c in box)
        {
            int r = (c >> 16) & 0xFF;
            int g = (c >> 8) & 0xFF;
            int b = c & 0xFF;
            if (r < minR) { minR = r; }
            if (r > maxR) { maxR = r; }
            if (g < minG) { minG = g; }
            if (g > maxG) { maxG = g; }
            if (b < minB) { minB = b; }
            if (b > maxB) { maxB = b; }
        }

        return Math.Max(maxR - minR, Math.Max(maxG - minG, maxB - minB));
    }

    private static (byte R, byte G, byte B) AverageColor(int[] box, Dictionary<int, int> histogram)
    {
        long r = 0, g = 0, b = 0, weight = 0;
        foreach (int c in box)
        {
            int count = histogram[c];
            r += ((c >> 16) & 0xFF) * count;
            g += ((c >> 8) & 0xFF) * count;
            b += (c & 0xFF) * count;
            weight += count;
        }

        if (weight == 0)
        {
            return (0, 0, 0);
        }

        return ((byte)(r / weight), (byte)(g / weight), (byte)(b / weight));
    }

    private static int NearestColor(byte[] palette, byte r, byte g, byte b)
    {
        int best = 0;
        int bestDistance = int.MaxValue;
        for (int i = 0; i < palette.Length / 3; i++)
        {
            int dr = palette[i * 3] - r;
            int dg = palette[i * 3 + 1] - g;
            int db = palette[i * 3 + 2] - b;
            int distance = (dr * dr) + (dg * dg) + (db * db);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = i;
                if (distance == 0)
                {
                    break;
                }
            }
        }

        return best;
    }

    /// <summary>
    /// GIF 变体 LZW。码长增长点与解码器的表增长对齐：编码器在分配完「第 2^codeSize 个码」时加宽，
    /// 表满 4096 时发 clear 重置（GIF 允许在任意位置重置）。
    /// </summary>
    private static byte[] CompressLzw(byte[] indices)
    {
        int clearCode = 1 << MinCodeSize;
        int endCode = clearCode + 1;
        int codeSize = MinCodeSize + 1;
        int nextCode = endCode + 1;

        var table = new Dictionary<int, int>(MaxCode);
        var output = new List<byte>(indices.Length / 2 + 16);

        int bitBuffer = 0;
        int bitCount = 0;

        void WriteCode(int code, int size)
        {
            bitBuffer |= code << bitCount;
            bitCount += size;
            while (bitCount >= 8)
            {
                output.Add((byte)(bitBuffer & 0xFF));
                bitBuffer >>= 8;
                bitCount -= 8;
            }
        }

        WriteCode(clearCode, codeSize);

        if (indices.Length > 0)
        {
            int prefix = indices[0];
            for (int i = 1; i < indices.Length; i++)
            {
                int k = indices[i];
                int key = (prefix << 8) | k;

                if (table.TryGetValue(key, out int existing))
                {
                    prefix = existing;
                    continue;
                }

                WriteCode(prefix, codeSize);

                if (nextCode < MaxCode)
                {
                    table[key] = nextCode++;
                    // 解码器比编码器晚两格建表（giflib 的 RunningCode - 2 语义），所以码长要在
                    // 「分配完 2^codeSize + 1 号」时才 +1；提前一格会出现 9/10 位错位，Skia 的
                    // GIF 解码器会报 ErrorInInput（已用 1200 像素渐变图实测四种阈值确定）。
                    if (nextCode == (1 << codeSize) + 1 && codeSize < 12)
                    {
                        codeSize++;
                    }
                }
                else
                {
                    WriteCode(clearCode, codeSize);
                    table.Clear();
                    nextCode = endCode + 1;
                    codeSize = MinCodeSize + 1;
                }

                prefix = k;
            }

            WriteCode(prefix, codeSize);
        }

        WriteCode(endCode, codeSize);
        if (bitCount > 0)
        {
            output.Add((byte)(bitBuffer & 0xFF));
        }

        return [.. output];
    }
}
