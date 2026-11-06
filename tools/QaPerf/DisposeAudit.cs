using System.Globalization;
using System.Text.RegularExpressions;

namespace DocumentTools.QaPerf;

internal sealed class DisposeSite
{
    /// <summary>Repository-relative path.</summary>
    public string File { get; set; } = string.Empty;

    public int Line { get; set; }

    public string Code { get; set; } = string.Empty;

    public string TypeName { get; set; } = string.Empty;

    /// <summary>Reflection result: does the created type actually implement IDisposable?</summary>
    public bool TypeDisposable { get; set; }

    /// <summary>Static reading of the statement: wrapped in using (or not).</summary>
    public bool Disposed { get; set; }
}

/// <summary>
/// Static scan of Core for document objects created from Aspose. Read-only: it reports file:line
/// and never edits src/. Whether "not using" is actually a leak depends on whether the type
/// implements IDisposable, so that is answered by reflection instead of assumption.
/// </summary>
internal static class DisposeAudit
{
    private const int MaxCodeLength = 150;

    private static readonly Regex QualifiedNew = new(
        @"new\s+Aspose\.(Pdf|Words|Cells|Slides)\.(Document|Workbook|Presentation)\s*\(",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex UnqualifiedNew = new(
        @"new\s+(Document|Workbook|Presentation)\s*\(",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>Target-typed creation: "Aspose.Pdf.Document doc = new(path)".</summary>
    private static readonly Regex TargetTypedNew = new(
        @"(Aspose\.(?:Pdf|Words|Cells|Slides)\.(?:Document|Workbook|Presentation))\s+[A-Za-z_]\w*\s*=\s*new\s*\(",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex AsposeUsing = new(
        @"^\s*using\s+Aspose\.(Pdf|Words|Cells|Slides)\s*;",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex DisposalKeyword = new(
        @"\busing\s+(?:var\s+|\(|async\s+)?[A-Za-z_]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex NamespaceUsing = new(
        @"^using\s+[\w.]+\s*;$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static List<DisposeSite> Scan(string repoRoot)
    {
        var sites = new List<DisposeSite>();
        string coreRoot = Path.Combine(repoRoot, "src", "DocumentTools.Core");
        if (!Directory.Exists(coreRoot))
        {
            return sites;
        }

        foreach (string path in Directory.EnumerateFiles(coreRoot, "*.cs", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(repoRoot, path).Replace('\\', '/');
            if (relative.Contains("/bin/", StringComparison.Ordinal) || relative.Contains("/obj/", StringComparison.Ordinal))
            {
                continue;
            }

            string[] lines;
            try
            {
                lines = File.ReadAllLines(path);
            }
            catch (IOException)
            {
                continue;
            }

            string? inferredNamespace = InferNamespace(lines);

            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                string? typeName = MatchType(line, inferredNamespace);
                if (typeName is null)
                {
                    continue;
                }

                bool disposed = IsDisposal(lines, index);

                sites.Add(new DisposeSite
                {
                    File = relative,
                    Line = index + 1,
                    Code = Truncate(line.Trim()),
                    TypeName = typeName,
                    TypeDisposable = IsDisposable(typeName),
                    Disposed = disposed
                });
            }
        }

        return sites
            .OrderBy(site => site.File, StringComparer.Ordinal)
            .ThenBy(site => site.Line)
            .ToList();
    }

    public static bool IsDisposable(string typeName)
    {
        try
        {
            return typeName switch
            {
                "Aspose.Pdf.Document" => typeof(IDisposable).IsAssignableFrom(typeof(Aspose.Pdf.Document)),
                "Aspose.Words.Document" => typeof(IDisposable).IsAssignableFrom(typeof(Aspose.Words.Document)),
                "Aspose.Cells.Workbook" => typeof(IDisposable).IsAssignableFrom(typeof(Aspose.Cells.Workbook)),
                "Aspose.Slides.Presentation" => typeof(IDisposable).IsAssignableFrom(typeof(Aspose.Slides.Presentation)),
                _ => false
            };
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>Maps the created type to a fully qualified name when the line is unambiguous.</summary>
    private static string? MatchType(string line, string? inferredNamespace)
    {
        Match targetTyped = TargetTypedNew.Match(line);
        if (targetTyped.Success)
        {
            return targetTyped.Groups[1].Value;
        }

        Match qualified = QualifiedNew.Match(line);
        if (qualified.Success)
        {
            string assembly = qualified.Groups[1].Value;
            string kind = qualified.Groups[2].Value;
            return "Aspose." + assembly + "." + kind;
        }

        Match unqualified = UnqualifiedNew.Match(line);
        if (!unqualified.Success || inferredNamespace is null)
        {
            return null;
        }

        string created = unqualified.Groups[1].Value;
        bool consistent =
            (created == "Document" && (inferredNamespace == "Pdf" || inferredNamespace == "Words")) ||
            (created == "Workbook" && inferredNamespace == "Cells") ||
            (created == "Presentation" && inferredNamespace == "Slides");

        return consistent ? "Aspose." + inferredNamespace + "." + created : null;
    }

    /// <summary>Only one Aspose namespace may be imported, otherwise "Document" is ambiguous.</summary>
    private static string? InferNamespace(string[] lines)
    {
        var namespaces = new HashSet<string>(StringComparer.Ordinal);
        foreach (string line in lines)
        {
            Match match = AsposeUsing.Match(line);
            if (!match.Success)
            {
                continue;
            }

            namespaces.Add(match.Groups[1].Value);
            if (namespaces.Count > 1)
            {
                return null;
            }
        }

        return namespaces.Count == 1 ? namespaces.First() : null;
    }

    /// <summary>
    /// Walks the statement the creation belongs to (a ternary or a wrapped declaration can put
    /// "using var" several lines above), stopping at a statement boundary.
    /// </summary>
    private static bool IsDisposal(string[] lines, int index)
    {
        if (ContainsDisposal(lines[index]))
        {
            return true;
        }

        for (int probe = index - 1, steps = 0; probe >= 0 && steps < 8; probe--, steps++)
        {
            string trimmed = lines[probe].Trim();
            if (trimmed.Length == 0 || trimmed.EndsWith(';') || trimmed.EndsWith('{') || trimmed.EndsWith('}'))
            {
                break;
            }

            if (ContainsDisposal(lines[probe]))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsDisposal(string line)
    {
        string trimmed = StripComment(line).Trim();
        if (trimmed.Length == 0 || NamespaceUsing.IsMatch(trimmed))
        {
            return false;
        }

        return DisposalKeyword.IsMatch(trimmed);
    }

    /// <summary>Drops a trailing line comment so a commented-out "using" cannot mark a site as released.</summary>
    private static string StripComment(string line)
    {
        int comment = line.IndexOf("//", StringComparison.Ordinal);
        return comment < 0 ? line : line[..comment];
    }

    private static string Truncate(string text)
        => text.Length <= MaxCodeLength ? text : text[..MaxCodeLength] + "…";

    public static string DescribeCounts(IReadOnlyList<DisposeSite> sites)
    {
        int undisposed = sites.Count(site => !site.Disposed);
        int undisposedDisposable = sites.Count(site => !site.Disposed && site.TypeDisposable);
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0} 处 Aspose 文档对象创建：{1} 处显式 using，{2} 处未 using；其中 {3} 处的类型确实实现 IDisposable",
            sites.Count,
            sites.Count - undisposed,
            undisposed,
            undisposedDisposable);
    }
}
