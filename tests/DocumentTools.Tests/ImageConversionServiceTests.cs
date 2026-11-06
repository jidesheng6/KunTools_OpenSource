using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Image;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// 图片管线细分参数（image.compress 的 JPG/PNG 分区）与 ICO 多尺寸容器。
/// 素材用 SkiaSharp 现场生成，不依赖仓库内图片；ICO 直接断言字节结构。
/// </summary>
public class ImageConversionServiceTests
{
    private static readonly ImageConversionService Service = new();

    [Theory]
    [InlineData(ImagePngColorType.Grayscale, 0)]
    [InlineData(ImagePngColorType.Truecolor, 2)]
    [InlineData(ImagePngColorType.TruecolorWithAlpha, 6)]
    public void Convert_Png_WritesRequestedIhdrColorType(ImagePngColorType color, int expectedColorType)
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 24, 16);
            string output = Service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "png",
                PngColor = color
            });

            byte[] png = File.ReadAllBytes(output);
            Assert.Equal("IHDR", System.Text.Encoding.ASCII.GetString(png, 12, 4));
            Assert.Equal(expectedColorType, png[25]);
            Assert.Equal(24, ReadInt32BigEndian(png, 16));
            Assert.Equal(16, ReadInt32BigEndian(png, 20));
        });
    }

    [Fact]
    public void Convert_Png_HonoursZLibLevel()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 64, 64);
            long LevelSize(int level)
            {
                string output = Service.Convert(source, dir, new ImageEncodeOptions
                {
                    Format = "png",
                    PngColor = ImagePngColorType.TruecolorWithAlpha,
                    PngLevel = level
                });
                return new FileInfo(output).Length;
            }

            long stored = LevelSize(0);
            long best = LevelSize(9);
            Assert.True(best < stored, $"level 9 ({best}) 应小于 level 0 ({stored})");
        });
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 3)]
    public void Convert_Jpeg_WritesGrayscaleOrColorComponents(bool grayscale, int expectedComponents)
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 32, 24);
            string output = Service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "JPG",
                JpegQuality = 90,
                JpegGrayscale = grayscale
            });

            Assert.EndsWith("_out.jpg", output, StringComparison.Ordinal);
            byte[] jpeg = File.ReadAllBytes(output);
            Assert.Equal(0xFF, jpeg[0]);
            Assert.Equal(0xD8, jpeg[1]);
            Assert.Equal(expectedComponents, ReadJpegSofComponents(jpeg));
        });
    }

    [Fact]
    public void Convert_Jpeg_QualityChangesSize()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 96, 96);
            long Low(int quality) => new FileInfo(Service.Convert(source, dir, new ImageEncodeOptions
            {
                Format = "jpg",
                JpegQuality = quality
            })).Length;

            Assert.True(Low(20) < Low(95));
        });
    }

    [Fact]
    public void Convert_Bmp_Writes24BitUncompressedHeader()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 20, 10);
            string output = Service.Convert(source, dir, new ImageEncodeOptions { Format = "BMP" });

            byte[] bmp = File.ReadAllBytes(output);
            Assert.Equal((byte)'B', bmp[0]);
            Assert.Equal((byte)'M', bmp[1]);
            Assert.Equal(bmp.Length, ReadInt32(bmp, 2));
            Assert.Equal(54, ReadInt32(bmp, 10));
            Assert.Equal(20, ReadInt32(bmp, 18));
            Assert.Equal(10, ReadInt32(bmp, 22));
            Assert.Equal(24, ReadInt16(bmp, 28));
            Assert.Equal(0, ReadInt32(bmp, 30)); // BI_RGB
            int stride = (20 * 3 + 3) / 4 * 4;
            Assert.Equal(54 + stride * 10, bmp.Length);
        });
    }

    [Fact]
    public void ToIco_MultiSize_WritesOneEntryPerSize()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 40, 40);
            string output = Service.ToIco(source, dir, new[] { 16, 32, 64, 128, 256 });

            Assert.EndsWith("_multi.ico", output, StringComparison.Ordinal);
            byte[] ico = File.ReadAllBytes(output);
            Assert.Equal(0, ReadInt16(ico, 0));  // reserved
            Assert.Equal(1, ReadInt16(ico, 2));  // type = icon
            Assert.Equal(5, ReadInt16(ico, 4));  // count

            int[] expectedSizes = { 16, 32, 64, 128, 256 };
            int expectedOffset = 6 + 16 * expectedSizes.Length;
            for (int i = 0; i < expectedSizes.Length; i++)
            {
                int size = expectedSizes[i];
                int entry = 6 + 16 * i;
                Assert.Equal(size >= 256 ? 0 : size, ico[entry]);      // width
                Assert.Equal(size >= 256 ? 0 : size, ico[entry + 1]);  // height
                Assert.Equal(1, ReadInt16(ico, entry + 4));            // planes
                Assert.Equal(32, ReadInt16(ico, entry + 6));           // bit count
                int length = ReadInt32(ico, entry + 8);
                int offset = ReadInt32(ico, entry + 12);
                Assert.Equal(expectedOffset, offset);
                Assert.Equal(0x89, ico[offset]);
                Assert.Equal((byte)'P', ico[offset + 1]);
                Assert.Equal((byte)'N', ico[offset + 2]);
                Assert.Equal((byte)'G', ico[offset + 3]);
                Assert.Equal(size, ReadInt32BigEndian(ico, offset + 16));       // PNG IHDR width
                Assert.Equal(size, ReadInt32BigEndian(ico, offset + 20));       // PNG IHDR height
                expectedOffset += length;
            }

            Assert.Equal(ico.Length, expectedOffset);
        });
    }

    [Fact]
    public void ToIco_SingleSize_KeepsLegacyContainer()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 40, 40);
            string output = Service.ToIco(source, dir, 64);

            Assert.EndsWith("_64.ico", output, StringComparison.Ordinal);
            byte[] ico = File.ReadAllBytes(output);
            Assert.Equal(1, ReadInt16(ico, 4));
            Assert.Equal(64, ico[6]);
            Assert.Equal(64, ico[7]);
            Assert.Equal(22, ReadInt32(ico, 18));
        });
    }

    [Fact]
    public void ImageConvertOperation_IcoMultiSize_UsesMultiSizeContainer()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 40, 40);
            DocumentOperationResult result = new ImageConvertOperation("image.ico", Service)
                .ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = [source],
                    OutputDirectory = dir,
                    Options = new Dictionary<string, string>
                    {
                        ["format"] = "ICO 32",
                        ["multiSize"] = "true"
                    }
                }).GetAwaiter().GetResult();

            Assert.True(result.Success, result.Message);
            string output = Assert.Single(result.OutputFiles);
            Assert.EndsWith("_multi.ico", output, StringComparison.Ordinal);
            Assert.Equal(5, ReadInt16(File.ReadAllBytes(output), 4));
        });
    }

    [Fact]
    public void ImageConvertOperation_Compress_WiresJpegAndPngOptions()
    {
        Run(dir =>
        {
            string source = CreateSourcePng(dir, 32, 32);
            var operation = new ImageConvertOperation("image.compress", Service);

            DocumentOperationResult jpegResult = operation.ExecuteAsync(new DocumentOperationRequest
            {
                InputFiles = [source],
                OutputDirectory = dir,
                Options = new Dictionary<string, string>
                {
                    // 本用例只验证格式/质量接线；useSourceExtension 勾选时会沿用 .png 后缀（legacy 语义）。
                    ["useSourceExtension"] = "false",
                    ["format"] = "JPEG",
                    ["quality"] = "95",
                    ["jpegColorMode"] = "Grayscale"
                }
            }).GetAwaiter().GetResult();
            Assert.True(jpegResult.Success, jpegResult.Message);
            Assert.Equal(1, ReadJpegSofComponents(File.ReadAllBytes(Assert.Single(jpegResult.OutputFiles))));

            DocumentOperationResult pngResult = operation.ExecuteAsync(new DocumentOperationRequest
            {
                InputFiles = [source],
                OutputDirectory = dir,
                Options = new Dictionary<string, string>
                {
                    ["useSourceExtension"] = "false",
                    ["format"] = "PNG",
                    ["pngLevel"] = "9",
                    ["pngColorType"] = "Grayscale"
                }
            }).GetAwaiter().GetResult();
            Assert.True(pngResult.Success, pngResult.Message);
            Assert.Equal(0, File.ReadAllBytes(Assert.Single(pngResult.OutputFiles))[25]);
        });
    }

    private static void Run(Action<string> body)
    {
        string dir = Directory.CreateTempSubdirectory("documenttools-image-").FullName;
        try
        {
            body(dir);
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    /// <summary>现造一张带透明渐变的小 PNG 作为唯一素材。</summary>
    private static string CreateSourcePng(string directory, int width, int height)
    {
        using var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                byte alpha = (byte)((x + y) % 2 == 0 ? 255 : 128);
                bitmap.SetPixel(x, y, new SKColor((byte)(x * 255 / Math.Max(1, width - 1)),
                    (byte)(y * 255 / Math.Max(1, height - 1)), 200, alpha));
            }
        }

        string path = Path.Combine(directory, "sample.png");
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using (var fs = File.Open(path, FileMode.Create, FileAccess.Write))
        {
            data.SaveTo(fs);
        }

        return path;
    }

    private static int ReadJpegSofComponents(byte[] jpeg)
    {
        for (int i = 0; i < jpeg.Length - 9; i++)
        {
            if (jpeg[i] == 0xFF && jpeg[i + 1] == 0xC0)
            {
                return jpeg[i + 9];
            }
        }

        throw new InvalidOperationException("未找到 JPEG SOF0 段。");
    }

    private static int ReadInt16(byte[] buffer, int offset)
        => buffer[offset] | (buffer[offset + 1] << 8);

    private static int ReadInt32(byte[] buffer, int offset)
        => buffer[offset] | (buffer[offset + 1] << 8) | (buffer[offset + 2] << 16) | (buffer[offset + 3] << 24);

    /// <summary>PNG 的多字节字段是大端。</summary>
    private static int ReadInt32BigEndian(byte[] buffer, int offset)
        => (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
}
