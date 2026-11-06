using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace DocumentTools.QaPerf;

/// <summary>
/// Writes plain PDF 1.4 fixtures by hand instead of through Aspose: generating inputs with Aspose
/// would warm the very library whose cold-start cost the harness is measuring, and would stamp
/// evaluation watermarks into the inputs.
/// </summary>
internal static class PdfFixtureWriter
{
    public const int LargePageCount = 200;
    public const int MergePartCount = 20;
    public const int MergePartPages = 2;

    private const int PageObjectBase = 5; // 1 = catalog, 2 = page tree, 3/4 = fonts

    public static string EnsureLarge(string pdfRoot, ICollection<string> notes)
    {
        Directory.CreateDirectory(pdfRoot);
        string path = Path.Combine(pdfRoot, "pages200.pdf");
        if (!File.Exists(path) || new FileInfo(path).Length < 4096)
        {
            Write(path, LargePageCount, seed: 17);
            notes.Add("新建 200 页 PDF");
        }
        else
        {
            notes.Add("200 页 PDF 已存在，匹配当前页数则复用");
            if (CountPages(path) is int pages && pages != LargePageCount)
            {
                Write(path, LargePageCount, seed: 17);
                notes.Add(string.Format(CultureInfo.InvariantCulture, "200 页 PDF 页数不符（{0}），已重建", pages));
            }
        }

        return path;
    }

    public static string EnsureMergeParts(string pdfRoot, ICollection<string> notes)
    {
        string directory = Path.Combine(pdfRoot, "merge20");
        Directory.CreateDirectory(directory);
        int created = 0;
        for (int index = 1; index <= MergePartCount; index++)
        {
            string path = Path.Combine(directory, string.Format(CultureInfo.InvariantCulture, "part_{0:00}.pdf", index));
            if (File.Exists(path) && new FileInfo(path).Length > 256)
            {
                continue;
            }

            Write(path, MergePartPages, seed: 100 + index);
            created++;
        }

        notes.Add(created == 0
            ? "20 个 2 页 PDF 已存在，直接复用"
            : string.Format(CultureInfo.InvariantCulture, "新建 {0} 个 2 页 PDF", created));
        return directory;
    }

    /// <summary>Cheap page count straight from the file (no Aspose involvement).</summary>
    public static int? CountPages(string path)
    {
        try
        {
            string head = Encoding.Latin1.GetString(File.ReadAllBytes(path));
            int marker = head.IndexOf("/Type /Pages /Count ", StringComparison.Ordinal);
            if (marker < 0)
            {
                return null;
            }

            int start = marker + "/Type /Pages /Count ".Length;
            int end = start;
            while (end < head.Length && char.IsDigit(head[end]))
            {
                end++;
            }

            return int.TryParse(head.AsSpan(start, end - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out int pages)
                ? pages
                : null;
        }
        catch (IOException)
        {
            return null;
        }
    }

    public static void Write(string path, int pageCount, int seed)
    {
        if (pageCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageCount), "PDF 至少需要 1 页。");
        }

        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        var random = new XorShift((uint)(seed * 7919) + 104729u);
        int objectCount = 4 + (pageCount * 2);
        long[] offsets = new long[objectCount + 1];

        using var stream = new MemoryStream(64 * 1024);

        void WriteText(string text)
        {
            byte[] bytes = Encoding.Latin1.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        void BeginObject(int number)
        {
            offsets[number] = stream.Position;
            WriteText(number.ToString(CultureInfo.InvariantCulture) + " 0 obj\n");
        }

        // Header plus a binary comment so the file is not mistreated as pure ASCII.
        WriteText("%PDF-1.4\n%\u00e2\u00e3\u00cf\u00d3\n");

        BeginObject(1);
        WriteText("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        BeginObject(2);
        var kids = new StringBuilder();
        for (int i = 0; i < pageCount; i++)
        {
            if (i > 0)
            {
                kids.Append(' ');
            }

            kids.Append((PageObjectBase + (2 * i)).ToString(CultureInfo.InvariantCulture)).Append(" 0 R");
        }

        WriteText(string.Format(
            CultureInfo.InvariantCulture,
            "<< /Type /Pages /Count {0} /Kids [{1}] >>\nendobj\n",
            pageCount,
            kids));
        kids.Clear();

        BeginObject(3);
        WriteText("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>\nendobj\n");

        BeginObject(4);
        WriteText("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>\nendobj\n");

        for (int i = 0; i < pageCount; i++)
        {
            int pageObject = PageObjectBase + (2 * i);
            int contentObject = pageObject + 1;
            byte[] content = Deflate(BuildContent(i + 1, pageCount, ref random));

            BeginObject(pageObject);
            WriteText(string.Format(
                CultureInfo.InvariantCulture,
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {0} 0 R >>\nendobj\n",
                contentObject));

            BeginObject(contentObject);
            WriteText(string.Format(
                CultureInfo.InvariantCulture,
                "<< /Length {0} /Filter /FlateDecode >>\nstream\n",
                content.Length));
            stream.Write(content, 0, content.Length);
            WriteText("\nendstream\nendobj\n");
        }

        long xrefOffset = stream.Position;
        WriteText("xref\n0 " + (objectCount + 1).ToString(CultureInfo.InvariantCulture) + "\n");
        WriteText("0000000000 65535 f \n");
        for (int number = 1; number <= objectCount; number++)
        {
            WriteText(offsets[number].ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
        }

        WriteText(string.Format(
            CultureInfo.InvariantCulture,
            "trailer\n<< /Size {0} /Root 1 0 R >>\nstartxref\n{1}\n%%EOF\n",
            objectCount + 1,
            xrefOffset));

        File.WriteAllBytes(fullPath, stream.ToArray());
    }

    /// <summary>zlib-wrapped deflate, what /FlateDecode expects; keeps the fixture inside 200 KB.</summary>
    private static byte[] Deflate(byte[] raw)
    {
        using var output = new MemoryStream(raw.Length);
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            zlib.Write(raw, 0, raw.Length);
        }

        return output.ToArray();
    }

    /// <summary>Page content: a few shaded bars plus text lines, ASCII only (WinAnsi safe).</summary>
    private static byte[] BuildContent(int pageNumber, int pageCount, ref XorShift random)
    {
        var builder = new StringBuilder(1024);
        builder.Append("0.11 0.18 0.31 rg 40 794 515 20 re f\n");
        builder.Append("0.85 0.72 0.45 rg 40 794 120 6 re f\n");

        for (int bar = 0; bar < 4; bar++)
        {
            int y = 660 + (bar * 26);
            int width = random.Next(60, 460);
            double shade = random.Next(20, 80) / 100d;
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "{0:0.00} {0:0.00} {0:0.00} rg 40 {1} {2} 8 re f\n",
                shade,
                y,
                width);
        }

        builder.AppendFormat(
            CultureInfo.InvariantCulture,
            "BT /F2 13 Tf 40 762 Td (QA perf fixture - page {0} of {1}) Tj ET\n",
            pageNumber,
            pageCount);
        builder.Append("BT /F1 9 Tf 40 736 Td 12 TL\n");

        int lines = 8 + random.Next(0, 5);
        for (int line = 0; line < lines; line++)
        {
            builder.AppendFormat(
                CultureInfo.InvariantCulture,
                "(line {0}: batch conversion fixture, latin text only, seed {1}) Tj T*\n",
                line + 1,
                random.Next(1000, 9999));
        }

        builder.Append("ET\n");
        return Encoding.Latin1.GetBytes(builder.ToString());
    }
}
