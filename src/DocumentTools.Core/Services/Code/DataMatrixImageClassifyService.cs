using System.Collections.Concurrent;
using Aspose.BarCode.BarCodeRecognition;
using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Code;

/// <summary>
/// Port of legacy <c>Util.RecognitionPictureDataMatrix</c>（DataMatrix 分支，不含 OCR）。
/// 与 legacy 的三处有意差异见 <c>docs/feature-gap.md</c>「图片目录批量分类」一行。
/// </summary>
public sealed class DataMatrixImageClassifyService : IDataMatrixImageClassifyService
{
    /// <summary>线程数上限：防止界面传入异常值时把机器打满。</summary>
    private const int MaxParallelism = 64;

    private readonly IImagePageCodeReader _pageCodes;

    public DataMatrixImageClassifyService(IImagePageCodeReader pageCodes)
        => _pageCodes = pageCodes ?? throw new ArgumentNullException(nameof(pageCodes));

    public DataMatrixImageClassifyOutcome Classify(
        DataMatrixImageClassifyRequest request,
        IProgress<DataMatrixImageClassifyProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        string sourceRoot = PathGuard.RequireFolder(request.SourceDirectory, "源目录");
        if (string.IsNullOrWhiteSpace(request.TargetDirectory))
        {
            throw new ArgumentException("目标目录未选择。", nameof(request));
        }

        string targetRoot = Path.GetFullPath(request.TargetDirectory);
        Directory.CreateDirectory(targetRoot);

        List<string> images = ImageClassifyFiles.EnumerateImages(sourceRoot, request.IncludeSubdirectories).ToList();

        int total = images.Count;
        int processed = 0;
        int classified = 0;
        int unrecognized = 0;
        int failed = 0;
        bool canceled = false;

        var folderCounts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var failures = new ConcurrentBag<string>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = request.MaxDegreeOfParallelism <= 0
                ? -1
                : Math.Clamp(request.MaxDegreeOfParallelism, 1, MaxParallelism),
            CancellationToken = cancellationToken
        };

        try
        {
            Parallel.ForEach(
                images,
                parallelOptions,
                file =>
                {
                    try
                    {
                        string? pageCode = _pageCodes.ReadPageCode(file);
                        bool recognized = pageCode is not null;
                        string folderName = recognized
                            ? DataMatrixImageClassifyNaming.PageFolderPrefix + pageCode
                            : DataMatrixImageClassifyNaming.UnrecognizedFolder;

                        if (recognized)
                        {
                            Interlocked.Increment(ref classified);
                        }
                        else
                        {
                            Interlocked.Increment(ref unrecognized);
                        }

                        string destinationDirectory = ImageClassifyFiles.ResolveDestinationDirectory(
                            sourceRoot,
                            targetRoot,
                            file,
                            folderName,
                            request.IncludeSubdirectories);

                        Directory.CreateDirectory(destinationDirectory);
                        string destination = Path.Combine(destinationDirectory, Path.GetFileName(file));

                        if (!PathGuard.SamePath(file, destination))
                        {
                            ImageClassifyFiles.Deliver(file, destination, request.KeepSourceFiles);
                        }

                        // 目录显示名取「相对目标根」的一段，嵌套子目录也能区分开。
                        string relativeFolder = ImageClassifyFiles.DescribeFolder(targetRoot, destinationDirectory);
                        folderCounts.AddOrUpdate(relativeFolder, 1, (_, count) => count + 1);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        Interlocked.Increment(ref failed);
                        failures.Add($"{Path.GetFileName(file)}：{ex.Message}");
                    }

                    int done = Interlocked.Increment(ref processed);
                    progress?.Report(new DataMatrixImageClassifyProgress(done, total, Path.GetFileName(file)));
                });
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        IReadOnlyList<ImageClassifyFolder> folders = folderCounts
            .OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase)
            .Select(pair => new ImageClassifyFolder(pair.Key, pair.Value))
            .ToList();

        return new DataMatrixImageClassifyOutcome(
            total,
            classified,
            unrecognized,
            failed,
            canceled,
            folders,
            failures.OrderBy(text => text, StringComparer.OrdinalIgnoreCase).ToList());
    }
}
