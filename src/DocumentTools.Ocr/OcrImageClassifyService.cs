using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using DocumentTools.Core.Common;
using DocumentTools.Core.Services.Code;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;

namespace DocumentTools.Ocr;

/// <summary>
/// legacy <c>Util.RecognitionPictureDataMatrix</c> 的 OCR 分支（<c>Util.cs:6216-6280</c>）：
/// 逐张识别图片文字，命中正则的图片按「分类目录名」归档；可选把命中区域画框后再保存。
/// <para>并发口径：Paddle 引擎不是线程安全的，dev 也是在 <c>lock (engine)</c> 内逐张识别——这里直接串行，避免锁竞争。</para>
/// </summary>
public sealed class OcrImageClassifyService : IOcrImageClassifyService
{
    private const string NativeLibrary = "paddle_inference_c.dll";

    private static readonly string ModuleDirectory =
        Path.GetDirectoryName(typeof(OcrImageClassifyService).Assembly.Location) ?? AppContext.BaseDirectory;

    public bool IsAvailable => File.Exists(Path.Combine(ModuleDirectory, NativeLibrary));

    public string UnavailableReason => IsAvailable
        ? string.Empty
        : $"OCR 推理库缺失：{Path.Combine(ModuleDirectory, NativeLibrary)}（请重新解压 ocr 组件目录）";

    public OcrImageClassifyOutcome Classify(
        OcrImageClassifyRequest request,
        IProgress<OcrImageClassifyProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string sourceRoot = PathGuard.RequireFolder(request.SourceDirectory, "源目录");
        if (string.IsNullOrWhiteSpace(request.TargetDirectory))
        {
            throw new ArgumentException("目标目录未选择。", nameof(request));
        }

        string folderName = ResolveFolderName(request);
        Regex pattern = CompilePattern(request.MatchPattern);

        string targetRoot = Path.GetFullPath(request.TargetDirectory);
        Directory.CreateDirectory(targetRoot);

        List<string> images = ImageClassifyFiles.EnumerateImages(sourceRoot, request.IncludeSubdirectories).ToList();

        int total = images.Count;
        int processed = 0;
        int matched = 0;
        int unmatched = 0;
        int failed = 0;
        bool canceled = false;

        var folderCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var failures = new List<string>();

        // 引擎只建一次：模型加载约 1–2 秒，逐张重建会慢到不可用。
        using PaddleOcrAll engine = new(LocalFullModels.ChineseV3, PaddleDevice.Mkldnn())
        {
            AllowRotateDetection = true,
            Enable180Classification = true
        };

        foreach (string file in images)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                canceled = true;
                break;
            }

            try
            {
                using Mat image = Cv2.ImRead(file, ImreadModes.Color);
                if (image.Empty())
                {
                    throw new InvalidOperationException("无法读取图片（文件损坏或不是受支持的图片格式）。");
                }

                PaddleOcrResult result = engine.Run(image);
                var hits = result.Regions
                    .Where(region => !string.IsNullOrEmpty(region.Text) && pattern.IsMatch(region.Text))
                    .ToList();

                if (hits.Count == 0)
                {
                    unmatched++;
                }
                else
                {
                    string destinationDirectory = ImageClassifyFiles.ResolveDestinationDirectory(
                        sourceRoot,
                        targetRoot,
                        file,
                        folderName,
                        request.IncludeSubdirectories);
                    Directory.CreateDirectory(destinationDirectory);

                    string destination = Path.Combine(destinationDirectory, Path.GetFileName(file));

                    if (request.MarkContent)
                    {
                        // legacy：同一个 Mat 上逐命中画框，最后写一次；再按「保留源文件」决定是否删除源图。
                        foreach (PaddleOcrResultRegion hit in hits)
                        {
                            Cv2.Rectangle(image, hit.Rect.BoundingRect(), Scalar.OrangeRed, thickness: 2);
                        }

                        Cv2.ImWrite(destination, image);
                        if (!request.KeepSourceFiles && !PathGuard.SamePath(file, destination))
                        {
                            File.Delete(file);
                        }
                    }
                    else if (!PathGuard.SamePath(file, destination))
                    {
                        ImageClassifyFiles.Deliver(file, destination, request.KeepSourceFiles);
                    }

                    matched++;
                    string relativeFolder = ImageClassifyFiles.DescribeFolder(targetRoot, destinationDirectory);
                    folderCounts.AddOrUpdate(relativeFolder, 1, (_, count) => count + 1);
                }
            }
            catch (OperationCanceledException)
            {
                canceled = true;
                break;
            }
            catch (Exception ex)
            {
                failed++;
                failures.Add($"{Path.GetFileName(file)}：{ex.Message}");
            }

            processed++;
            progress?.Report(new OcrImageClassifyProgress(processed, total, Path.GetFileName(file)));
        }

        IReadOnlyList<ImageClassifyFolder> folders = folderCounts
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new ImageClassifyFolder(pair.Key, pair.Value))
            .ToList();

        return new OcrImageClassifyOutcome(
            total,
            matched,
            unmatched,
            failed,
            canceled,
            folders,
            failures.OrderBy(text => text, StringComparer.OrdinalIgnoreCase).ToList());
    }

    /// <summary>
    /// 目录名：优先用显式的「分类目录名」，否则沿用 legacy 的「把匹配串当目录名」。
    /// 正则里常见 <c>\d+</c> 这类字符不能做目录名，所以这里提前拦截并给可执行的提示。
    /// </summary>
    private static string ResolveFolderName(OcrImageClassifyRequest request)
    {
        string name = string.IsNullOrWhiteSpace(request.FolderName)
            ? request.MatchPattern.Trim()
            : request.FolderName.Trim();

        if (name.Length == 0)
        {
            throw new ArgumentException("分类目录名不能为空。", nameof(request));
        }

        char[] invalid = Path.GetInvalidFileNameChars();
        int bad = name.IndexOfAny(invalid);
        if (bad >= 0)
        {
            throw new ArgumentException(
                $"分类目录名「{name}」含有不能用于文件夹的字符「{name[bad]}」。请把「分类目录名」填成纯文字（如 语文），匹配文字可以继续用正则。",
                nameof(request));
        }

        return name;
    }

    private static Regex CompilePattern(string? pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("请先填写 OCR 匹配文字（正则，例如 语文）。", nameof(pattern));
        }

        try
        {
            // 只匹配文本片段，不需要跨行语义；超时防止病态正则把界面卡死。
            return new Regex(pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"OCR 匹配文字不是合法的正则：{ex.Message}", nameof(pattern), ex);
        }
    }

    /// <summary>给宿主写日志用（不参与业务）。</summary>
    public static string DescribeEnvironment()
        => string.Create(
            CultureInfo.InvariantCulture,
            $"OCR 组件目录={ModuleDirectory}；native={NativeLibrary}（{IsFilePresent(NativeLibrary)}）");

    private static string IsFilePresent(string fileName)
        => File.Exists(Path.Combine(ModuleDirectory, fileName)) ? "存在" : "缺失";
}
