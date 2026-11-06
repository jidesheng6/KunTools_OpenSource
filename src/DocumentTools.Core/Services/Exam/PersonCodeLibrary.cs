using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace DocumentTools.Core.Services.Exam;

/// <summary>
/// 内置测评人码图包（legacy <c>PersonCode.zip</c>：1 个目录项 + <c>PersonCode/001测评人.bmp</c> … <c>999测评人.bmp</c>）。
/// <para>
/// legacy 用 <c>Util.GetPersonCodeZipFileStream()</c>（<c>Util.cs:4615-4647</c>）把整包解压成 999 个流同时驻留内存；
/// 这里改为按需读单个条目：只缓存「序号 → 条目名」的元数据（压缩包中央目录，不涉及像素），
/// 谁要哪一张就只解压哪一张，避免一个 2.3 MB 的包在批处理时被放大成 999 份位图。
/// </para>
/// </summary>
public static class PersonCodeLibrary
{
    /// <summary>legacy Designer:11675 的提示语「[程序内置001-999测评人二维码图片]」即此范围。</summary>
    public const int FirstIndex = 1;

    /// <summary>legacy 内置图片最多 999 张（<c>BodyForm.cs:6291</c> 注释「最多只有999张」）。</summary>
    public const int LastIndex = 999;

    /// <summary>相对 <see cref="AppContext.BaseDirectory"/> 的发布路径（构建期由 csproj 的 Content/Link 落位）。</summary>
    public const string RelativeArchivePath = "assets/embedded/PersonCode.zip";

    /// <summary>条目名形如 <c>PersonCode/001测评人.bmp</c>；只靠 3 位序号识别，因此前缀、分隔符与扩展名大小写都可容忍。</summary>
    private static readonly Regex EntryNamePattern = new(
        @"(?:^|[\\/])(?<index>\d{3})[^\\/]*\.(?:bmp|png|jpe?g|gif|tif{1,2})$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly object CacheLock = new();

    /// <summary>压缩包完整路径 → (序号 → 条目名)。只缓存名字，不缓存像素。</summary>
    private static readonly Dictionary<string, IReadOnlyDictionary<int, string>> NameCache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 默认压缩包路径。<paramref name="baseDirectory"/> 为空时从 <see cref="AppContext.BaseDirectory"/> 起按候选位置查找，
    /// 命中第一个存在的文件；都不存在时返回发布路径（让错误信息指向约定位置）。
    /// </summary>
    /// <remarks>
    /// 候选顺序覆盖三种真实布局：App 发布的 <c>assets\embedded\</c>、App 的 <c>None Include="..\..\assets\embedded\**"</c>
    /// 落位 <c>embedded\</c>，以及从仓库内直接跑（tests/tools 的 bin 目录往上是源码树的 <c>DocumentTools\Resources\</c>）。
    /// 这样测试工程不会因为输出目录少了拷贝就把功能判成不可用。
    /// </remarks>
    public static string ResolveArchivePath(string? baseDirectory = null)
    {
        string root = string.IsNullOrWhiteSpace(baseDirectory) ? AppContext.BaseDirectory : baseDirectory;
        List<string> candidates = CandidatePaths(root);
        foreach (string candidate in candidates)
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return candidates[0];
    }

    private static List<string> CandidatePaths(string root)
    {
        var candidates = new List<string>();
        void Add(string path)
        {
            string full = Path.GetFullPath(path);
            if (!candidates.Contains(full, StringComparer.OrdinalIgnoreCase))
            {
                candidates.Add(full);
            }
        }

        Add(Path.Combine(root, RelativeArchivePath.Replace('/', Path.DirectorySeparatorChar)));
        Add(Path.Combine(root, "embedded", "PersonCode.zip"));
        Add(Path.Combine(root, "PersonCode.zip"));

        // 从输出目录往上找源码树：<repo>\DocumentTools\Resources\PersonCode.zip 与 <...>\Resources\PersonCode.zip。
        DirectoryInfo? current = new(Path.GetFullPath(root));
        for (int depth = 0; depth < 6 && current is not null; depth++, current = current.Parent)
        {
            Add(Path.Combine(current.FullName, "DocumentTools", "Resources", "PersonCode.zip"));
            Add(Path.Combine(current.FullName, "Resources", "PersonCode.zip"));
        }

        return candidates;
    }

    /// <summary>压缩包里是否有可用的码图（文件存在即可，不校验内容）。</summary>
    public static bool IsAvailable(string? archivePath = null)
        => File.Exists(string.IsNullOrWhiteSpace(archivePath) ? ResolveArchivePath() : archivePath);

    /// <summary>压缩包条目总数（含目录项）——legacy 包是 1000 条。</summary>
    public static int CountEntries(string? archivePath = null)
    {
        using ZipArchive archive = OpenArchive(archivePath);
        return archive.Entries.Count;
    }

    /// <summary>可用的码图张数（不含目录项），正常为 999。</summary>
    public static int CountImages(string? archivePath = null) => ReadIndex(archivePath).Count;

    /// <summary>可用序号（升序）。</summary>
    public static IReadOnlyList<int> ListIndexes(string? archivePath = null)
        => ReadIndex(archivePath).Keys.OrderBy(i => i).ToList();

    /// <summary>
    /// 读取指定序号的码图字节。序号必须是 001–999；越界抛中文异常，缺条目也抛中文异常（不静默返回空）。
    /// </summary>
    public static byte[] ReadImage(int index, string? archivePath = null)
    {
        EnsureInRange(index);
        IReadOnlyDictionary<int, string> map = ReadIndex(archivePath);
        if (!map.TryGetValue(index, out string? entryName))
        {
            string path = string.IsNullOrWhiteSpace(archivePath) ? ResolveArchivePath() : archivePath;
            throw new InvalidOperationException(
                $"内置测评人码图包里没有 {Format(index)} 号条目：{path}");
        }

        using ZipArchive archive = OpenArchive(archivePath);
        ZipArchiveEntry? entry = archive.GetEntry(entryName);
        if (entry is null)
        {
            throw new InvalidOperationException($"内置测评人码图包读取失败：找不到条目 {entryName}。");
        }

        using Stream source = entry.Open();
        using var buffer = new MemoryStream(checked((int)entry.Length));
        source.CopyTo(buffer);
        return buffer.ToArray();
    }

    /// <summary>序号越界时抛 <see cref="ArgumentOutOfRangeException"/>（文案与 legacy 提示语一致）。</summary>
    public static void EnsureInRange(int index)
    {
        if (index is < FirstIndex or > LastIndex)
        {
            throw new ArgumentOutOfRangeException(
                nameof(index), index, $"内置测评人码图只有 001–999，收到 {index.ToString(CultureInfo.InvariantCulture)}");
        }
    }

    /// <summary>序号格式化：3 位补零（001、012、999）。</summary>
    public static string Format(int index) => index.ToString("D3", CultureInfo.InvariantCulture);

    private static ZipArchive OpenArchive(string? archivePath)
    {
        string path = string.IsNullOrWhiteSpace(archivePath) ? ResolveArchivePath() : archivePath;
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"找不到内置测评人码图包：{path}。请确认 assets\\embedded\\PersonCode.zip 随程序一起发布。", path);
        }

        return ZipFile.OpenRead(path);
    }

    private static IReadOnlyDictionary<int, string> ReadIndex(string? archivePath)
    {
        string path = Path.GetFullPath(
            string.IsNullOrWhiteSpace(archivePath) ? ResolveArchivePath() : archivePath);

        lock (CacheLock)
        {
            if (NameCache.TryGetValue(path, out IReadOnlyDictionary<int, string>? cached))
            {
                return cached;
            }
        }

        using ZipArchive archive = OpenArchive(path);
        var map = new Dictionary<int, string>();
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            // 目录项（名字以 / 结尾、Length=0）不参与。
            Match match = EntryNamePattern.Match(entry.FullName);
            if (!match.Success)
            {
                continue;
            }

            if (!int.TryParse(match.Groups["index"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int index))
            {
                continue;
            }

            map[index] = entry.FullName;
        }

        lock (CacheLock)
        {
            NameCache[path] = map;
        }

        return map;
    }
}
