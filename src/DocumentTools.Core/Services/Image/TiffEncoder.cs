using SkiaSharp;

namespace DocumentTools.Core.Services.Image;

/// <summary>TIFF 压缩方式；数值即 tag 259（Compression）的规范取值。</summary>
public enum TiffCompression
{
    /// <summary>不压缩（tag 259 = 1）。</summary>
    None = 1,

    /// <summary>LZW（tag 259 = 5），TIFF 6.0 的通用压缩，无损。</summary>
    Lzw = 5,

    /// <summary>PackBits（tag 259 = 32773），Macintosh RLE，适合大色块截图。</summary>
    PackBits = 32773
}

/// <summary>
/// 单页 TIFF 编码器（TIFF 6.0，little-endian，24bpp RGB）。
/// SkiaSharp 3.x 不含 TIFF 编码器（<c>SKEncodedImageFormat</c> 无 Tif），legacy 走的是
/// Aspose.Imaging 的 <c>TiffOptions</c>；这里按 legacy 的「单页」语义自己写字节，
/// 供 <see cref="ImageConversionService"/> 输出 .tif/.tiff。
/// </summary>
/// <remarks>
/// 未压缩的 24bpp 12MP 图约 36MB，对文档工具不可接受，因此默认按条带（strip）做 PackBits，
/// 并可用 LZW 或无压缩。条带行数按「目标 64KB 一条」推算后夹在 8–256 行，宽图和方图都不退化。
/// </remarks>
internal static class TiffEncoder
{
    private const int HeaderSize = 8;          // II + 42 + IFD 偏移
    private const int EntryCount = 13;
    private const int EntrySize = 12;
    private const int IfdSize = 2 + (EntryCount * EntrySize) + 4; // 条目数 + 条目 + 下一个 IFD 指针

    /// <summary>每条带的目标字节数：64KB 是 TIFF 实现里常见的默认条带尺寸。</summary>
    private const int TargetStripBytes = 64 * 1024;

    private const int MinRowsPerStrip = 8;
    private const int MaxRowsPerStrip = 256;

    private const int TagImageWidth = 256;
    private const int TagImageLength = 257;
    private const int TagBitsPerSample = 258;
    private const int TagCompression = 259;
    private const int TagPhotometric = 262;
    private const int TagStripOffsets = 273;
    private const int TagSamplesPerPixel = 277;
    private const int TagRowsPerStrip = 278;
    private const int TagStripByteCounts = 279;
    private const int TagXResolution = 282;
    private const int TagYResolution = 283;
    private const int TagPlanarConfig = 284;
    private const int TagResolutionUnit = 296;

    private const int TypeShort = 3;
    private const int TypeLong = 4;
    private const int TypeRational = 5;

    private const int LzwClearCode = 256;
    private const int LzwEoiCode = 257;
    private const int LzwFirstFreeCode = 258;
    private const int LzwMaxCodeWidth = 12;

    /// <summary>把位图编码成单页、24bpp RGB 的 TIFF 字节。</summary>
    /// <param name="bitmap">源位图；透明像素按白底合成（TIFF 6.0 基线无 alpha 通道）。</param>
    /// <param name="compression">条带压缩方式，默认 LZW。</param>
    /// <returns>完整 TIFF 文件字节；失败抛 <see cref="InvalidOperationException"/>。</returns>
    public static byte[] Encode(SKBitmap bitmap, TiffCompression compression = TiffCompression.Lzw)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        if (bitmap.Width <= 0 || bitmap.Height <= 0)
        {
            throw new InvalidOperationException("编码失败：位图尺寸无效（.tif）。");
        }

        int width = bitmap.Width;
        int height = bitmap.Height;
        int rowBytes = width * 3;
        int rowsPerStrip = ResolveRowsPerStrip(rowBytes, height);
        int stripCount = (height + rowsPerStrip - 1) / rowsPerStrip;

        byte[] raw = ReadRgb24(bitmap, width, height);

        // ── 逐条带压缩 ──
        var strips = new byte[stripCount][];
        for (int s = 0; s < stripCount; s++)
        {
            int firstRow = s * rowsPerStrip;
            int rowCount = Math.Min(rowsPerStrip, height - firstRow);
            ReadOnlySpan<byte> slice = raw.AsSpan(firstRow * rowBytes, rowCount * rowBytes);
            strips[s] = compression switch
            {
                TiffCompression.None => slice.ToArray(),
                TiffCompression.Lzw => LzwEncode(slice),
                TiffCompression.PackBits => PackBitsEncode(slice),
                _ => throw new InvalidOperationException($"编码失败：不支持的 TIFF 压缩方式 {compression}（.tif）。")
            };
        }

        // ── 外部数据区布局：位深数组 → 两个 72DPI 有理数 → 条带偏移/长度数组 → 条带数据 ──
        int extraOffset = HeaderSize + IfdSize;
        int bitsPerSampleOffset = extraOffset;                       // 3 × SHORT
        int xResolutionOffset = bitsPerSampleOffset + 6;              // RATIONAL
        int yResolutionOffset = xResolutionOffset + 8;
        int stripOffsetsOffset = yResolutionOffset + 8;
        int stripByteCountsOffset = stripOffsetsOffset + (stripCount > 1 ? stripCount * 4 : 0);
        int dataOffset = stripByteCountsOffset + (stripCount > 1 ? stripCount * 4 : 0);

        int totalSize = dataOffset;
        foreach (byte[] strip in strips)
        {
            totalSize += strip.Length;
        }

        byte[] buffer = new byte[totalSize];

        // ── 文件头：字节序、42 魔数、首个 IFD 偏移 ──
        buffer[0] = (byte)'I';
        buffer[1] = (byte)'I';
        WriteInt16(buffer, 2, 42);
        WriteInt32(buffer, 4, HeaderSize);

        // ── IFD：条目必须按 tag 升序 ──
        int cursor = HeaderSize;
        WriteInt16(buffer, cursor, EntryCount);
        cursor += 2;

        cursor = WriteEntry(buffer, cursor, TagImageWidth, TypeShort, 1, (uint)width);
        cursor = WriteEntry(buffer, cursor, TagImageLength, TypeShort, 1, (uint)height);
        cursor = WriteEntry(buffer, cursor, TagBitsPerSample, TypeShort, 3, (uint)bitsPerSampleOffset);
        cursor = WriteEntry(buffer, cursor, TagCompression, TypeShort, 1, (uint)compression);
        cursor = WriteEntry(buffer, cursor, TagPhotometric, TypeShort, 1, 2);          // 2 = RGB
        cursor = WriteEntry(
            buffer,
            cursor,
            TagStripOffsets,
            TypeLong,
            stripCount,
            stripCount > 1 ? (uint)stripOffsetsOffset : (uint)dataOffset);
        cursor = WriteEntry(buffer, cursor, TagSamplesPerPixel, TypeShort, 1, 3);
        cursor = WriteEntry(buffer, cursor, TagRowsPerStrip, TypeLong, 1, (uint)rowsPerStrip);
        cursor = WriteEntry(
            buffer,
            cursor,
            TagStripByteCounts,
            TypeLong,
            stripCount,
            stripCount > 1 ? (uint)stripByteCountsOffset : (uint)strips[0].Length);
        cursor = WriteEntry(buffer, cursor, TagXResolution, TypeRational, 1, (uint)xResolutionOffset);
        cursor = WriteEntry(buffer, cursor, TagYResolution, TypeRational, 1, (uint)yResolutionOffset);
        cursor = WriteEntry(buffer, cursor, TagPlanarConfig, TypeShort, 1, 1);         // 1 = chunky
        cursor = WriteEntry(buffer, cursor, TagResolutionUnit, TypeShort, 1, 2);       // 2 = 英寸
        WriteInt32(buffer, cursor, 0);                                                 // 没有下一个 IFD

        WriteInt16(buffer, bitsPerSampleOffset, 8);
        WriteInt16(buffer, bitsPerSampleOffset + 2, 8);
        WriteInt16(buffer, bitsPerSampleOffset + 4, 8);
        WriteInt32(buffer, xResolutionOffset, 72);
        WriteInt32(buffer, xResolutionOffset + 4, 1);
        WriteInt32(buffer, yResolutionOffset, 72);
        WriteInt32(buffer, yResolutionOffset + 4, 1);

        // ── 条带偏移/长度数组 + 条带数据 ──
        int offset = dataOffset;
        for (int s = 0; s < stripCount; s++)
        {
            if (stripCount > 1)
            {
                WriteInt32(buffer, stripOffsetsOffset + (s * 4), offset);
                WriteInt32(buffer, stripByteCountsOffset + (s * 4), strips[s].Length);
            }

            strips[s].CopyTo(buffer, offset);
            offset += strips[s].Length;
        }

        return buffer;
    }

    /// <summary>
    /// 由目标条带字节数推算每带行数：宽图少行、窄图多行，并夹在 8–256 行避免极端值。
    /// </summary>
    internal static int ResolveRowsPerStrip(int rowBytes, int height)
    {
        if (rowBytes <= 0)
        {
            return Math.Max(1, height);
        }

        int byTarget = TargetStripBytes / rowBytes;
        int rows = Math.Clamp(byTarget, MinRowsPerStrip, MaxRowsPerStrip);
        return Math.Min(rows, Math.Max(1, height));
    }

    /// <summary>把位图读成自上而下、RGB 三通道、透明合成白底的裸像素。</summary>
    private static byte[] ReadRgb24(SKBitmap bitmap, int width, int height)
    {
        int stride = width * 3;
        byte[] raw = new byte[stride * height];

        SKBitmap? bgra = bitmap.ColorType == SKColorType.Bgra8888 ? null : bitmap.Copy(SKColorType.Bgra8888);
        SKBitmap source = bgra ?? bitmap;
        try
        {
            using SKPixmap pixmap = source.PeekPixels()
                ?? throw new InvalidOperationException("编码失败：无法读取像素（.tif）。");
            ReadOnlySpan<byte> pixels = pixmap.GetPixelSpan();
            int sourceStride = pixmap.RowBytes;

            for (int y = 0; y < height; y++)
            {
                int srcRow = y * sourceStride;
                int dstRow = y * stride;
                for (int x = 0; x < width; x++)
                {
                    int src = srcRow + (x * 4);
                    byte b = pixels[src];
                    byte g = pixels[src + 1];
                    byte r = pixels[src + 2];
                    byte a = pixels[src + 3];

                    int dst = dstRow + (x * 3);
                    raw[dst] = Composite(r, a);
                    raw[dst + 1] = Composite(g, a);
                    raw[dst + 2] = Composite(b, a);
                }
            }
        }
        finally
        {
            bgra?.Dispose();
        }

        return raw;
    }

    /// <summary>PackBits（Macintosh RLE）：重复串 ≥3 用重复包，其余用字面包，单包最多 128 字节。</summary>
    private static byte[] PackBitsEncode(ReadOnlySpan<byte> data)
    {
        var output = new List<byte>(data.Length);
        int count = data.Length;
        int i = 0;

        while (i < count)
        {
            int runLength = 1;
            while (i + runLength < count && runLength < 128 && data[i + runLength] == data[i])
            {
                runLength++;
            }

            if (runLength >= 3)
            {
                output.Add((byte)(257 - runLength));
                output.Add(data[i]);
                i += runLength;
                continue;
            }

            int literalStart = i;
            int literalLength = 0;
            while (i < count && literalLength < 128)
            {
                // 后面已经形成 3 连重复就结束字面包，交给重复包。
                if (runLength < 3 && i + 2 < count && data[i] == data[i + 1] && data[i] == data[i + 2])
                {
                    break;
                }

                i++;
                literalLength++;
            }

            if (literalLength == 0)
            {
                literalLength = 1;
                i++;
            }

            output.Add((byte)(literalLength - 1));
            for (int j = 0; j < literalLength; j++)
            {
                output.Add(data[literalStart + j]);
            }
        }

        return [.. output];
    }

    /// <summary>
    /// TIFF 6.0 LZW：MSB 优先打包、clear 256 / EOI 257、9→12 位，
    /// 并按规范开 EarlyChange（可用码差一格就加宽）。
    /// </summary>
    private static byte[] LzwEncode(ReadOnlySpan<byte> data)
    {
        var writer = new MsbBitWriter();
        writer.Write(LzwClearCode, 9);

        if (data.Length == 0)
        {
            writer.Write(LzwEoiCode, 9);
            return writer.Finish();
        }

        var table = new Dictionary<int, int>(4096);
        int codeWidth = 9;
        int nextCode = LzwFirstFreeCode;
        int current = data[0];

        for (int i = 1; i < data.Length; i++)
        {
            int next = data[i];
            int key = (current << 8) | next;

            if (table.TryGetValue(key, out int existing))
            {
                current = existing;
                continue;
            }

            writer.Write(current, codeWidth);
            table[key] = nextCode++;
            current = next;

            // 加宽时机对齐 libtiff：下一个可用码到达 2^width 时再多给一位。
            // 实测依据：按「2^width - 1 提前加宽」写出的流，libtiff/PIL 报 decoder error -2；
            // 按「2^width」加宽可被 PIL 12.1.1 完整解码且像素逐点一致。
            if (nextCode >= (1 << codeWidth) && codeWidth < LzwMaxCodeWidth)
            {
                codeWidth++;
            }

            if (nextCode >= (1 << LzwMaxCodeWidth) - 2)
            {
                writer.Write(LzwClearCode, codeWidth);
                table.Clear();
                nextCode = LzwFirstFreeCode;
                codeWidth = 9;
            }
        }

        writer.Write(current, codeWidth);
        writer.Write(LzwEoiCode, codeWidth);
        return writer.Finish();
    }

    /// <summary>MSB 优先的定长码位写入器（TIFF LZW 的位序）。</summary>
    private sealed class MsbBitWriter
    {
        private readonly List<byte> _output = [];
        private int _buffer;
        private int _bits;

        public void Write(int code, int width)
        {
            _buffer = (_buffer << width) | (code & ((1 << width) - 1));
            _bits += width;
            while (_bits >= 8)
            {
                _bits -= 8;
                _output.Add((byte)((_buffer >> _bits) & 0xFF));
            }

            _buffer &= (1 << _bits) - 1;
        }

        public byte[] Finish()
        {
            if (_bits > 0)
            {
                _output.Add((byte)((_buffer << (8 - _bits)) & 0xFF));
                _buffer = 0;
                _bits = 0;
            }

            return [.. _output];
        }
    }

    /// <summary>透明像素合成到白底，避免 TIFF 基线没有 alpha 时出现黑边。</summary>
    private static byte Composite(byte value, byte alpha)
        => alpha == 255 ? value : (byte)(((value * alpha) + (255 * (255 - alpha)) + 127) / 255);

    private static int WriteEntry(byte[] buffer, int offset, int tag, int type, int count, uint valueOrOffset)
    {
        WriteInt16(buffer, offset, tag);
        WriteInt16(buffer, offset + 2, type);
        WriteInt32(buffer, offset + 4, count);

        // SHORT 且 count == 1 时数值直接放在 4 字节值域的**前**两个字节（小端，余两字节为 0）。
        if (type == TypeShort && count == 1)
        {
            WriteInt16(buffer, offset + 8, (int)(valueOrOffset & 0xFFFF));
            WriteInt16(buffer, offset + 10, 0);
        }
        else
        {
            WriteInt32(buffer, offset + 8, (int)valueOrOffset);
        }

        return offset + EntrySize;
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
