using SkiaSharp;

namespace DocumentTools.Core.Services.Exam;

/// <summary>
/// SkiaSharp 3.x 的托管构建没有 BMP 编码器（<c>SKImage.Encode(SKEncodedImageFormat.Bmp, ...)</c> 返回 null，
/// 直接 <c>data.ToArray()</c> 就是 NullReferenceException），但考评链路两端都必须是真 BMP：
/// 选票页 PicContent 要落 8bpp 灰度 BMP（对齐 legacy <c>Util.ConvertTo256Color</c> + <c>ImageFormat.Bmp</c>），
/// 图片旋转遇到 .bmp 目标也要写合法 BMP。这里手写 BITMAPFILEHEADER/BITMAPINFOHEADER，不依赖任何编码器。
/// </summary>
internal static class BmpEncoder
{
    private const int FileHeaderSize = 14;
    private const int InfoHeaderSize = 40;
    private const int PaletteEntrySize = 4;

    /// <summary>8bpp 索引位图 + 256 级灰阶调色板（与 legacy 256 色灰度完全一致）。</summary>
    public static byte[] EncodeGray8(SKBitmap gray)
    {
        int width = gray.Width;
        int height = gray.Height;
        int stride = (width + 3) / 4 * 4;
        byte[] pixels = gray.Bytes;
        int sourceStride = gray.RowBytes;
        int paletteSize = 256 * PaletteEntrySize;
        int pixelOffset = FileHeaderSize + InfoHeaderSize + paletteSize;

        var buffer = new byte[pixelOffset + stride * height];
        WriteFileHeader(buffer, pixelOffset);
        WriteInfoHeader(buffer, width, height, bitCount: 8, imageSize: stride * height, paletteSize);
        for (int index = 0; index < 256; index++)
        {
            int at = FileHeaderSize + InfoHeaderSize + index * PaletteEntrySize;
            buffer[at] = (byte)index;
            buffer[at + 1] = (byte)index;
            buffer[at + 2] = (byte)index;
            buffer[at + 3] = 0;
        }

        // BMP 像素行自下而上存放。
        for (int y = 0; y < height; y++)
        {
            int sourceRow = y * sourceStride;
            int targetRow = pixelOffset + (height - 1 - y) * stride;
            Array.Copy(pixels, sourceRow, buffer, targetRow, width);
        }

        return buffer;
    }

    /// <summary>32bpp BI_RGB 位图（第 4 字节按规范留 0），用于彩色图片另存为 .bmp。</summary>
    public static byte[] EncodeBgra32(SKBitmap source)
    {
        SKBitmap bitmap = source;
        if (source.ColorType != SKColorType.Bgra8888)
        {
            bitmap = source.Copy(SKColorType.Bgra8888);
        }

        try
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int stride = width * 4;
            byte[] pixels = bitmap.Bytes;
            int sourceStride = bitmap.RowBytes;
            int pixelOffset = FileHeaderSize + InfoHeaderSize;

            var buffer = new byte[pixelOffset + stride * height];
            WriteFileHeader(buffer, pixelOffset);
            WriteInfoHeader(buffer, width, height, bitCount: 32, imageSize: stride * height, paletteSize: 0);
            for (int y = 0; y < height; y++)
            {
                int sourceRow = y * sourceStride;
                int targetRow = pixelOffset + (height - 1 - y) * stride;
                for (int x = 0; x < width; x++)
                {
                    int to = targetRow + x * 4;
                    int from = sourceRow + x * 4;
                    buffer[to] = pixels[from];
                    buffer[to + 1] = pixels[from + 1];
                    buffer[to + 2] = pixels[from + 2];
                    buffer[to + 3] = 0;
                }
            }

            return buffer;
        }
        finally
        {
            if (!ReferenceEquals(bitmap, source))
            {
                bitmap.Dispose();
            }
        }
    }

    private static void WriteFileHeader(byte[] buffer, int pixelOffset)
    {
        buffer[0] = (byte)'B';
        buffer[1] = (byte)'M';
        WriteInt32(buffer, 2, buffer.Length);
        WriteInt32(buffer, 6, 0);
        WriteInt32(buffer, 10, pixelOffset);
    }

    private static void WriteInfoHeader(
        byte[] buffer,
        int width,
        int height,
        int bitCount,
        int imageSize,
        int paletteSize)
    {
        const int at = FileHeaderSize;
        WriteInt32(buffer, at, InfoHeaderSize);
        WriteInt32(buffer, at + 4, width);
        WriteInt32(buffer, at + 8, height);
        WriteInt16(buffer, at + 12, 1);
        WriteInt16(buffer, at + 14, bitCount);
        WriteInt32(buffer, at + 16, 0);
        WriteInt32(buffer, at + 20, imageSize);
        WriteInt32(buffer, at + 24, 2835);
        WriteInt32(buffer, at + 28, 2835);
        WriteInt32(buffer, at + 32, paletteSize / PaletteEntrySize);
        WriteInt32(buffer, at + 36, paletteSize / PaletteEntrySize);
    }

    private static void WriteInt32(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
        buffer[offset + 2] = (byte)(value >> 16);
        buffer[offset + 3] = (byte)(value >> 24);
    }

    private static void WriteInt16(byte[] buffer, int offset, int value)
    {
        buffer[offset] = (byte)value;
        buffer[offset + 1] = (byte)(value >> 8);
    }
}
