using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace DocumentTools.QaFunction;

/// <summary>
/// Format sniffing / structural assertions. Everything here inspects the produced
/// bytes directly (signature, OOXML zip entries, PNG IHDR, PDF page objects) so a
/// "success" never relies on the service's own claim.
/// </summary>
internal static class FileProbe
{
    private static readonly Encoding Latin1 = Encoding.Latin1;

    public static long Size(string path)
        => File.Exists(path) ? new FileInfo(path).Length : -1;

    public static bool HasSignature(string path, params byte[] signature)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        using FileStream stream = File.OpenRead(path);
        byte[] head = new byte[signature.Length];
        int read = stream.Read(head, 0, head.Length);
        if (read != head.Length)
        {
            return false;
        }

        return head.AsSpan().SequenceEqual(signature);
    }

    public static bool IsPdf(string path) => HasSignature(path, (byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-');

    public static bool IsPng(string path) => HasSignature(path, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A);

    public static bool IsJpeg(string path) => HasSignature(path, 0xFF, 0xD8, 0xFF);

    public static bool IsZip(string path) => HasSignature(path, 0x50, 0x4B, 0x03, 0x04);

    /// <summary>First <paramref name="count"/> bytes of a file, for magic-number reporting.</summary>
    public static byte[] Head(string path, int count)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] buffer = new byte[count];
        int read = stream.Read(buffer, 0, count);
        return read == count ? buffer : buffer[..read];
    }

    /// <summary>PNG width/height straight from the IHDR chunk (bytes 16..23, big endian).</summary>
    public static (int Width, int Height) PngSize(string path)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] header = new byte[24];
        if (stream.Read(header, 0, header.Length) != header.Length)
        {
            return (0, 0);
        }

        return PngSize(header);
    }

    /// <summary>Same parse, for an image already read out of an OOXML media part.</summary>
    public static (int Width, int Height) PngSize(byte[] bytes)
    {
        if (bytes.Length < 24)
        {
            return (0, 0);
        }

        int width = (bytes[16] << 24) | (bytes[17] << 16) | (bytes[18] << 8) | bytes[19];
        int height = (bytes[20] << 24) | (bytes[21] << 16) | (bytes[22] << 8) | bytes[23];
        return (width, height);
    }

    /// <summary>JPEG width/height from the first SOF0/SOF1/SOF2 marker.</summary>
    public static (int Width, int Height) JpegSize(string path)
    {
        using FileStream stream = File.OpenRead(path);
        byte[] buffer = new byte[4];
        stream.Read(buffer, 0, 2);
        while (stream.Position < stream.Length)
        {
            if (stream.Read(buffer, 0, 2) != 2)
            {
                break;
            }

            if (buffer[0] != 0xFF)
            {
                continue;
            }

            byte marker = buffer[1];
            if (marker is 0xD8 or 0xD9 or 0x01 || (marker >= 0xD0 && marker <= 0xD7))
            {
                continue;
            }

            if (stream.Read(buffer, 0, 2) != 2)
            {
                break;
            }

            int length = (buffer[0] << 8) | buffer[1];
            bool isStartOfFrame = marker >= 0xC0 && marker <= 0xCF
                                  && marker != 0xC4 && marker != 0xC8 && marker != 0xCC;
            if (!isStartOfFrame)
            {
                stream.Seek(length - 2, SeekOrigin.Current);
                continue;
            }

            byte[] frame = new byte[5];
            if (stream.Read(frame, 0, frame.Length) != frame.Length)
            {
                break;
            }

            return ((frame[3] << 8) | frame[4], (frame[1] << 8) | frame[2]);
        }

        return (0, 0);
    }

    /// <summary>
    /// Counts page objects (<c>/Type /Page</c> excluding <c>/Pages</c>). Falls back to the
    /// largest <c>/Count</c> in the page tree when the dictionary is compressed.
    /// </summary>
    public static int PdfPageCount(string path)
    {
        if (!IsPdf(path))
        {
            return -1;
        }

        string content = Latin1.GetString(File.ReadAllBytes(path));
        int count = Regex.Matches(content, @"/Type\s*/Page(?!s)").Count;
        if (count > 0)
        {
            return count;
        }

        int best = 0;
        foreach (Match match in Regex.Matches(content, @"/Count\s+(\d+)"))
        {
            if (int.TryParse(match.Groups[1].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value > best)
            {
                best = value;
            }
        }

        return best;
    }

    public static bool ZipHasEntry(string path, string entryName)
    {
        if (!IsZip(path))
        {
            return false;
        }

        using ZipArchive archive = ZipFile.OpenRead(path);
        return archive.GetEntry(entryName) is not null;
    }

    public static string ReadZipText(string path, string entryName)
    {
        if (!IsZip(path))
        {
            return string.Empty;
        }

        using ZipArchive archive = ZipFile.OpenRead(path);
        ZipArchiveEntry? entry = archive.GetEntry(entryName);
        if (entry is null)
        {
            return string.Empty;
        }

        using Stream stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// True when the OOXML package is structurally what the extension claims
    /// (docx → word/document.xml, xlsx → xl/workbook.xml, pptx → ppt/presentation.xml).
    /// </summary>
    public static bool IsOfficePackage(string path, string expectedEntry) => ZipHasEntry(path, expectedEntry);

    /// <summary>Heuristic: Aspose evaluation builds embed an "Evaluation" watermark/notice.</summary>
    public static bool ContainsEvaluationNotice(string path)
    {
        if (!File.Exists(path))
        {
            return false;
        }

        string content = Latin1.GetString(File.ReadAllBytes(path));
        return content.Contains("Evaluation Only", StringComparison.OrdinalIgnoreCase)
               || content.Contains("Created with Aspose", StringComparison.OrdinalIgnoreCase)
               || content.Contains("evaluation version", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// <c>biBitCount</c> straight out of the BMP header (BITMAPINFOHEADER offset 28, little-endian).
    /// Hand-parsed on purpose: the assertion must not depend on the same encoder stack that produced it.
    /// </summary>
    public static int BmpPixelBits(string path)
    {
        if (!HasSignature(path, (byte)'B', (byte)'M'))
        {
            return -1;
        }

        byte[] header = Head(path, 30);
        return header.Length < 30 ? -1 : header[28] | (header[29] << 8);
    }
}

/// <summary>
/// Independent PDF text extraction via Aspose.Pdf (the same third-party parser the product
/// uses, but a separate code path from the service under test). Used to prove a produced PDF
/// carries real text and to expose the evaluation watermark.
/// </summary>
internal static class PdfTextProbe
{
    public static string Extract(string path, int maxPages = 3)
    {
        if (!FileProbe.IsPdf(path))
        {
            return string.Empty;
        }

        try
        {
            using var document = new Aspose.Pdf.Document(path);
            var absorber = new Aspose.Pdf.Text.TextAbsorber();
            int limit = Math.Min(maxPages, document.Pages.Count);
            for (int page = 1; page <= limit; page++)
            {
                document.Pages[page].Accept(absorber);
            }

            return absorber.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            return "<extract failed: " + ex.GetType().Name + ": " + ex.Message + ">";
        }
    }

    public static bool ContainsEvaluation(string path)
        => Extract(path, 5).Contains("Evaluation", StringComparison.OrdinalIgnoreCase);

    /// <summary>第一页的 MediaBox 尺寸（pt）。用于证明 exportNotes=RightFull 真的加宽了页面。</summary>
    public static (double Width, double Height) FirstPageSize(string path)
    {
        if (!FileProbe.IsPdf(path))
        {
            return (0, 0);
        }

        try
        {
            using var document = new Aspose.Pdf.Document(path);
            if (document.Pages.Count == 0)
            {
                return (0, 0);
            }

            Aspose.Pdf.Page page = document.Pages[1];
            Aspose.Pdf.Rectangle box = page.MediaBox ?? page.Rect;
            return (box.Width, box.Height);
        }
        catch
        {
            return (0, 0);
        }
    }

    /// <summary>第一页 <c>Page.Rect</c> 尺寸（pt）——纸张尺寸断言必须用它，不用 PageInfo。</summary>
    public static (double Width, double Height) FirstPageRect(string path)
    {
        if (!FileProbe.IsPdf(path))
        {
            return (0, 0);
        }

        try
        {
            using var document = new Aspose.Pdf.Document(path);
            if (document.Pages.Count == 0)
            {
                return (0, 0);
            }

            Aspose.Pdf.Rectangle rect = document.Pages[1].Rect;
            return (rect.Width, rect.Height);
        }
        catch
        {
            return (0, 0);
        }
    }

    /// <summary>PDF 头版本号（如 "1.7"），空表示读不到。</summary>
    public static string Version(string path)
    {
        if (!FileProbe.IsPdf(path))
        {
            return string.Empty;
        }

        try
        {
            using var document = new Aspose.Pdf.Document(path);
            return document.Version;
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>顶层大纲（书签）条数；dev 用 wdExportCreateWordBookmarks，每处 Word 书签一条。</summary>
    public static int BookmarkCount(string path)
    {
        if (!FileProbe.IsPdf(path))
        {
            return -1;
        }

        try
        {
            using var document = new Aspose.Pdf.Document(path);
            return document.Outlines.Count;
        }
        catch
        {
            return -1;
        }
    }
}

