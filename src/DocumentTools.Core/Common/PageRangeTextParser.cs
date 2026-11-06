using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Common;

/// <summary>
/// Parses UI page-range text such as <c>1-3,5,8-10</c> into closed 1-based ranges.
/// Pure logic — used by Operations before calling split Services.
/// </summary>
public static class PageRangeTextParser
{
    /// <summary>
    /// Returns one or more ranges. Empty / whitespace → empty list (caller decides fallback).
    /// </summary>
    public static IReadOnlyList<PdfPageRange> Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Array.Empty<PdfPageRange>();
        }

        var ranges = new List<PdfPageRange>();
        foreach (string chunk in text.Split([',', ';', '，', '；'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (chunk.Length == 0)
            {
                continue;
            }

            int sep = chunk.IndexOfAny(['-', '–', '~', '到']);
            if (sep < 0)
            {
                if (!int.TryParse(chunk, out int single) || single < 1)
                {
                    throw new ArgumentException($"无效页码：{chunk}");
                }

                ranges.Add(new PdfPageRange(single, single));
                continue;
            }

            string left = chunk[..sep].Trim();
            string right = chunk[(sep + 1)..].Trim().TrimStart('到').Trim();
            if (!int.TryParse(left, out int first) || !int.TryParse(right, out int last) || first < 1 || last < 1)
            {
                throw new ArgumentException($"无效页码范围：{chunk}");
            }

            if (first > last)
            {
                (first, last) = (last, first);
            }

            ranges.Add(new PdfPageRange(first, last));
        }

        return ranges;
    }

    /// <summary>Legacy-style single start/end from the first parsed segment (or nulls if empty).</summary>
    public static (int? Start, int? End) FirstAsStartEnd(string? text)
    {
        IReadOnlyList<PdfPageRange> ranges = Parse(text);
        if (ranges.Count == 0)
        {
            return (null, null);
        }

        return (ranges[0].FirstPage, ranges[0].LastPage);
    }
}
