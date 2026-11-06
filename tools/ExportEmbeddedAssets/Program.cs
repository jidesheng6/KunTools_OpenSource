using System.Text;
using System.Xml.Linq;

// Export selected Resources.resx entries (incl. ResXFileRef) into assets/embedded/
// for WPF EmbeddedAssetService (do not commit proprietary binaries).

string? resxPath = null;
string outDir = Path.GetFullPath("assets/embedded");

for (int i = 0; i < args.Length; i++)
{
    if (args[i] is "--resx" && i + 1 < args.Length)
    {
        resxPath = args[++i];
    }
    else if (args[i] is "--out" && i + 1 < args.Length)
    {
        outDir = Path.GetFullPath(args[++i]);
    }
}

if (string.IsNullOrWhiteSpace(resxPath) || !File.Exists(resxPath))
{
    Console.Error.WriteLine("Usage: ExportEmbeddedAssets --resx <Resources.resx> [--out assets/embedded]");
    return 1;
}

resxPath = Path.GetFullPath(resxPath);
string resxDir = Path.GetDirectoryName(resxPath)!;
Directory.CreateDirectory(outDir);

var map = new (string Key, string[] ResxNames, string FileName)[]
{
    ("registry", new[] { "注册", "registry" }, "registry.reg"),
    ("kmsaio", new[] { "KMSAIO", "kmsaio" }, "kmsaio.cmd"),
    ("renamer", new[] { "ReNamerPro73", "renamer" }, "ReNamerPro73.exe"),
    ("renamer.rules", new[] { "移除副本规则_部分自定义规则", "renamer_rules" }, "renamer-rules.rnp"),
    ("link.netfx35", new[] { "Link_NetPan_Download_NetFx35", "Link_Download_NetFx35_Private", "link_netfx35" }, "link-netfx35.txt")
};

XDocument doc = XDocument.Load(resxPath);
int exported = 0;
foreach (var entry in map)
{
    XElement? data = null;
    foreach (string name in entry.ResxNames)
    {
        data = doc.Root?
            .Elements("data")
            .FirstOrDefault(e =>
                string.Equals((string?)e.Attribute("name"), name, StringComparison.OrdinalIgnoreCase));
        if (data is not null)
        {
            break;
        }
    }

    if (data is null)
    {
        Console.WriteLine($"[skip] {entry.Key}: not found in resx");
        continue;
    }

    string? typeAttr = (string?)data.Attribute("type");
    string? mime = (string?)data.Attribute("mimetype");
    string? value = data.Element("value")?.Value;
    if (string.IsNullOrEmpty(value))
    {
        Console.WriteLine($"[skip] {entry.Key}: empty value");
        continue;
    }

    string target = Path.Combine(outDir, entry.FileName);

    // ResXFileRef: "relative\path;Type[, ...];[encoding]"
    if (!string.IsNullOrEmpty(typeAttr) &&
        typeAttr.Contains("ResXFileRef", StringComparison.OrdinalIgnoreCase))
    {
        string[] parts = value.Split(';');
        string relative = parts[0].Trim();
        string source = Path.GetFullPath(Path.Combine(resxDir, relative));
        if (!File.Exists(source))
        {
            Console.WriteLine($"[skip] {entry.Key}: file missing -> {source}");
            continue;
        }

        bool asText = parts.Length >= 2 &&
                      parts[1].Contains("System.String", StringComparison.OrdinalIgnoreCase);
        if (asText)
        {
            Encoding enc = Encoding.UTF8;
            if (parts.Length >= 3)
            {
                try
                {
                    enc = Encoding.GetEncoding(parts[2].Trim());
                }
                catch
                {
                    // keep UTF-8
                }
            }

            string text = File.ReadAllText(source, enc);
            await File.WriteAllTextAsync(target, text, Encoding.UTF8);
        }
        else
        {
            File.Copy(source, target, overwrite: true);
        }

        Console.WriteLine($"[ok] {entry.Key} <- {source} ({new FileInfo(target).Length} bytes)");
        exported++;
        continue;
    }

    bool looksBinary = !string.IsNullOrEmpty(mime) &&
                       (mime.Contains("octet", StringComparison.OrdinalIgnoreCase)
                        || mime.Contains("binary", StringComparison.OrdinalIgnoreCase)
                        || mime.Contains("byte", StringComparison.OrdinalIgnoreCase));

    if (looksBinary)
    {
        try
        {
            byte[] bytes = Convert.FromBase64String(value.Trim());
            await File.WriteAllBytesAsync(target, bytes);
        }
        catch (FormatException)
        {
            Console.WriteLine($"[skip] {entry.Key}: not valid base64");
            continue;
        }
    }
    else
    {
        await File.WriteAllTextAsync(target, value, Encoding.UTF8);
    }

    Console.WriteLine($"[ok] {entry.Key} -> {target} ({new FileInfo(target).Length} bytes)");
    exported++;
}

Console.WriteLine($"Done. Exported {exported}/{map.Length}. Do NOT commit proprietary/crack binaries.");
return exported > 0 ? 0 : 2;