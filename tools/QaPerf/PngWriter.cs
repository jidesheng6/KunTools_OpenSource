using System.Buffers.Binary;
using System.IO.Compression;

namespace DocumentTools.QaPerf;

/// <summary>
/// Minimal PNG encoder (8-bit RGBA, Sub filter, zlib level 9).
/// SkiaSharp 3.x only exposes SKImage.Encode(SKEncodedImageFormat, int) and its default
/// deflate output on gradient+noise content lands around 224 KB, above the 200 KB fixture
/// budget; writing the container here keeps the size small and byte-for-byte reproducible.
/// </summary>
internal static class PngWriter
{
    private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    private static readonly uint[] CrcTable = BuildCrcTable();

    /// <summary>Writes <paramref name="rgba"/> (row-major, 4 bytes per pixel) as an 8-bit RGBA PNG.</summary>
    public static void Write(string path, int width, int height, ReadOnlySpan<byte> rgba)
    {
        int stride = width * 4;
        if (rgba.Length < stride * height)
        {
            throw new ArgumentException("Pixel buffer is smaller than width*height*4.", nameof(rgba));
        }

        using FileStream stream = File.Create(path);
        stream.Write(Signature);

        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt32BigEndian(header[..4], width);
        BinaryPrimitives.WriteInt32BigEndian(header.Slice(4, 4), height);
        header[8] = 8;  // bit depth
        header[9] = 6;  // colour type: truecolour with alpha
        header[10] = 0; // deflate
        header[11] = 0; // adaptive filtering
        header[12] = 0; // no interlace
        WriteChunk(stream, "IHDR", header);

        WriteChunk(stream, "IDAT", Compress(ApplySubFilter(rgba, width, height, stride)));
        WriteChunk(stream, "IEND", ReadOnlySpan<byte>.Empty);
    }

    /// <summary>Filters every scanline with the PNG "Sub" predictor, which collapses gradients to near-zero deltas.</summary>
    private static byte[] ApplySubFilter(ReadOnlySpan<byte> rgba, int width, int height, int stride)
    {
        byte[] filtered = new byte[(stride + 1) * height];
        for (int y = 0; y < height; y++)
        {
            int source = y * stride;
            int target = y * (stride + 1);
            filtered[target] = 1; // filter type 1 = Sub
            for (int x = 0; x < stride; x++)
            {
                byte left = x >= 4 ? rgba[source + x - 4] : (byte)0;
                filtered[target + 1 + x] = (byte)(rgba[source + x] - left);
            }
        }

        return filtered;
    }

    private static byte[] Compress(byte[] raw)
    {
        using var buffer = new MemoryStream(raw.Length / 8 + 64);
        using (var zlib = new ZLibStream(buffer, CompressionLevel.SmallestSize, leaveOpen: true))
        {
            zlib.Write(raw);
        }

        return buffer.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, ReadOnlySpan<byte> data)
    {
        Span<byte> length = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
        stream.Write(length);

        Span<byte> name = stackalloc byte[4];
        for (int i = 0; i < 4; i++)
        {
            name[i] = (byte)type[i];
        }

        stream.Write(name);
        stream.Write(data);

        // CRC covers the chunk type and its data, and must be finalised once over both.
        uint crc = UpdateCrc(0xFFFFFFFFu, name);
        crc = UpdateCrc(crc, data) ^ 0xFFFFFFFFu;
        Span<byte> crcBytes = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBytes, crc);
        stream.Write(crcBytes);
    }

    private static uint UpdateCrc(uint seed, ReadOnlySpan<byte> data)
    {
        uint crc = seed;
        foreach (byte value in data)
        {
            crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
        }

        return crc;
    }

    private static uint[] BuildCrcTable()
    {
        uint[] table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
