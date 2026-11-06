namespace DocumentTools.Core.Services.Code;

/// <summary>OCR 图片分类请求（legacy <c>RecognitionPictureDataMatrix</c> 的 OCR 分支）。</summary>
public sealed record OcrImageClassifyRequest
{
    /// <summary>待分类图片所在目录。</summary>
    public required string SourceDirectory { get; init; }

    /// <summary>分类产物目录；不存在时自动创建。</summary>
    public required string TargetDirectory { get; init; }

    /// <summary>识别文字要匹配的正则（legacy <c>ExamPictureOcrMatchString</c>）。</summary>
    public required string MatchPattern { get; init; }

    /// <summary>归档子目录名；空则用 <see cref="MatchPattern"/>（legacy 直接拿匹配串当目录名）。</summary>
    public string? FolderName { get; init; }

    /// <summary>legacy「内容标注（OCR）」：把命中的文字区域用红框画在图上再保存。</summary>
    public bool MarkContent { get; init; }

    /// <summary>legacy「包含子目录下文件」。</summary>
    public bool IncludeSubdirectories { get; init; }

    /// <summary>legacy「保留源目录内文件」：true = 复制，false = 移动。</summary>
    public bool KeepSourceFiles { get; init; } = true;

    /// <summary>0 = 自动。</summary>
    public int MaxDegreeOfParallelism { get; init; }
}

/// <summary>OCR 分类的进度。</summary>
public sealed record OcrImageClassifyProgress(int Processed, int Total, string CurrentFile);

/// <summary>OCR 分类结果（matched = 命中并归档，unmatched = 未命中留在原处）。</summary>
public sealed record OcrImageClassifyOutcome(
    int TotalImages,
    int MatchedImages,
    int UnmatchedImages,
    int FailedImages,
    bool Canceled,
    IReadOnlyList<ImageClassifyFolder> Folders,
    IReadOnlyList<string> Failures);

/// <summary>
/// OCR 图片分类（可选组件）。实现由 <c>ocr\DocumentTools.Ocr.dll</c> 在运行时提供；
/// 组件缺失时宿主拿不到实现，页面把 OCR 选项禁用并给出安装提示。
/// </summary>
public interface IOcrImageClassifyService
{
    bool IsAvailable { get; }

    /// <summary>不可用原因（中文，可直接展示）；可用时为空串。</summary>
    string UnavailableReason { get; }

    OcrImageClassifyOutcome Classify(
        OcrImageClassifyRequest request,
        IProgress<OcrImageClassifyProgress>? progress = null,
        CancellationToken cancellationToken = default);
}
