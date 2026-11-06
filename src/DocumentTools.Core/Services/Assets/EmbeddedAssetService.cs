using System.Text;

namespace DocumentTools.Core.Services.Assets;

public sealed class EmbeddedAssetService : IEmbeddedAssetService
{
    /// <summary>Logical keys → default file names under embedded/.</summary>
    public static IReadOnlyDictionary<string, string> Catalog { get; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["registry"] = "registry.reg",
            ["kmsaio"] = "kmsaio.cmd",
            ["renamer"] = "ReNamerPro73.exe",
            ["renamer.rules"] = "renamer-rules.rnp",
            ["link.netfx35"] = "link-netfx35.txt",
            ["vote.report.doc"] = "VoteReportDoc.doc"
        };

    private readonly string _root;

    public EmbeddedAssetService()
        : this(Path.Combine(AppContext.BaseDirectory, "embedded"))
    {
    }

    public EmbeddedAssetService(string rootDirectory)
    {
        _root = rootDirectory;
    }

    public IReadOnlyList<string> ListAvailableKeys()
        => Catalog.Keys.Where(Exists).OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();

    public bool Exists(string key)
        => TryResolvePath(key, out _);

    public string? GetText(string key)
    {
        if (!TryResolvePath(key, out string? path) || path is null)
        {
            return null;
        }

        return File.ReadAllText(path, Encoding.UTF8);
    }

    public void ReleaseToFile(string key, string targetPath)
    {
        if (!TryResolvePath(key, out string? source) || source is null)
        {
            throw new FileNotFoundException(
                $"未找到嵌入资源「{key}」。请将文件放到程序目录 embedded/（参见 assets/embedded/README.md）。",
                key);
        }

        string? dir = Path.GetDirectoryName(Path.GetFullPath(targetPath));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.Copy(source, targetPath, overwrite: true);
    }

    private bool TryResolvePath(string key, out string? path)
    {
        path = null;
        if (!Catalog.TryGetValue(key, out string? fileName))
        {
            return false;
        }

        string candidate = Path.Combine(_root, fileName);
        if (File.Exists(candidate))
        {
            path = candidate;
            return true;
        }

        return false;
    }
}
