using System.Globalization;
using System.IO;
using System.Text;

namespace DocumentTools.Core.Services.Files;

/// <summary>
/// 批量生成同名文件副本（对齐 legacy <c>Util.GenCopySameFile</c>）。
/// 命名规则集中在这里：<c>{源名} - {后缀} ({序号}){扩展名}</c>，序号接着目标目录里已有副本的最大值往下排。
/// 视图只收集「源文件 / 目标目录 / 数量 / 后缀 / 序号位宽」，不复制命名或扫描逻辑。
/// </summary>
public sealed class FileCopyService : IFileCopyService
{
    public const string DefaultSuffix = "副本";
    public const int DefaultPrefixDigits = 1;
    public const int MaxPrefixDigits = 10;

    public string BuildCopyName(string sourceFile, string namerSuffix, int index, int prefixDigits)
    {
        string stem = Path.GetFileNameWithoutExtension(sourceFile);
        string ext = Path.GetExtension(sourceFile);
        string suffix = NormalizeSuffix(namerSuffix);
        int digits = NormalizeDigits(prefixDigits);
        string number = digits <= 0 ? index.ToString(CultureInfo.InvariantCulture)
                                    : index.ToString("D" + digits.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
        return $"{stem} - {suffix} ({number}){ext}";
    }

    public string PreviewName(string sourceFile, string targetDirectory, string namerSuffix, int prefixDigits)
    {
        int next = FindNextIndex(sourceFile, targetDirectory, namerSuffix);
        return BuildCopyName(sourceFile, namerSuffix, next, prefixDigits);
    }

    public FileCopyResult Generate(string sourceFile, string targetDirectory, int count, string namerSuffix, int prefixDigits)
    {
        if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
        {
            throw new FileNotFoundException("要复制的源文件不存在，请重新选择。", sourceFile);
        }

        if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
        {
            throw new DirectoryNotFoundException($"目标目录不存在：{targetDirectory}。请选择存在的文件夹。");
        }

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "生成数量至少为 1。");
        }

        int next = FindNextIndex(sourceFile, targetDirectory, namerSuffix);
        int firstIndex = next;
        var paths = new List<string>(count);

        for (int i = 0; i < count; i++)
        {
            string name = BuildCopyName(sourceFile, namerSuffix, next, prefixDigits);
            string target = Path.Combine(targetDirectory, name);

            // 极端并发/同名占位：跳过已存在的目标，顺延到下一个可用序号，避免静默覆盖用户文件。
            while (File.Exists(target))
            {
                next++;
                name = BuildCopyName(sourceFile, namerSuffix, next, prefixDigits);
                target = Path.Combine(targetDirectory, name);
            }

            File.Copy(sourceFile, target);
            paths.Add(target);
            next++;
        }

        return new FileCopyResult(paths.Count, firstIndex, next - 1, paths);
    }

    /// <summary>扫描目标目录里形如 <c>{源名} - {后缀} (n){扩展名}</c> 的已有副本，返回下一个可用序号。</summary>
    private static int FindNextIndex(string sourceFile, string targetDirectory, string namerSuffix)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
        {
            return 1;
        }

        string stem = Path.GetFileNameWithoutExtension(sourceFile);
        string ext = Path.GetExtension(sourceFile);
        string suffix = NormalizeSuffix(namerSuffix);
        string prefix = $"{stem} - {suffix} (";

        int max = 0;
        foreach (string file in Directory.EnumerateFiles(targetDirectory))
        {
            string name = Path.GetFileName(file);
            if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!name.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            int closeParen = name.LastIndexOf(')');
            if (closeParen <= prefix.Length)
            {
                continue;
            }

            string inner = name[prefix.Length..closeParen];
            if (int.TryParse(inner, NumberStyles.None, CultureInfo.InvariantCulture, out int number) && number > max)
            {
                max = number;
            }
        }

        return max + 1;
    }

    private static string NormalizeSuffix(string? namerSuffix)
        => string.IsNullOrWhiteSpace(namerSuffix) ? DefaultSuffix : namerSuffix.Trim();

    private static int NormalizeDigits(int prefixDigits)
        => Math.Clamp(prefixDigits, 0, MaxPrefixDigits);
}
