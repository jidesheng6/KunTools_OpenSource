namespace DocumentTools.Core.Services.Code;

/// <summary>
/// 图片分类的目录 / 文件名规则（业务知识，视图不得自行拼接）。
/// 对应 legacy <c>Util.RecognitionPictureDataMatrix</c> 的 DataMatrix 分支。
/// </summary>
public static class DataMatrixImageClassifyNaming
{
    /// <summary>按页码归档的子目录前缀：<c>页码-001</c>（legacy <c>$"页码-{PageNumberText}"</c>）。</summary>
    public const string PageFolderPrefix = "页码-";

    /// <summary>无法识别 DataMatrix 的图片归档目录（legacy 声明了该目录名，但分支被写在循环内从未执行）。</summary>
    public const string UnrecognizedFolder = "异常-无法识别的图片";
}

/// <summary>图片目录批量分类请求（legacy <c>ExamPicture_Class_Start_Click</c> 的参数）。</summary>
public sealed record DataMatrixImageClassifyRequest
{
    /// <summary>待分类图片所在目录。</summary>
    public required string SourceDirectory { get; init; }

    /// <summary>分类产物目录；不存在时自动创建。</summary>
    public required string TargetDirectory { get; init; }

    /// <summary>legacy「包含子目录下文件」：开启时在目标目录下保留源目录的相对层级。</summary>
    public bool IncludeSubdirectories { get; init; }

    /// <summary>legacy「保留源目录内文件」：true = 复制，false = 移动。</summary>
    public bool KeepSourceFiles { get; init; } = true;

    /// <summary>legacy「设置处理线程数量」：0 = 自动（与 legacy 的 <c>-1</c> 同义）。</summary>
    public int MaxDegreeOfParallelism { get; init; }
}

/// <summary>单张图片处理进度。</summary>
public sealed record DataMatrixImageClassifyProgress(int Processed, int Total, string CurrentFile);

/// <summary>批量分类结果。</summary>
public sealed record DataMatrixImageClassifyOutcome(
    int TotalImages,
    int ClassifiedImages,
    int UnrecognizedImages,
    int FailedImages,
    bool Canceled,
    IReadOnlyList<ImageClassifyFolder> Folders,
    IReadOnlyList<string> Failures);

public interface IDataMatrixImageClassifyService
{
    /// <summary>
    /// 逐张解码图片里的 DataMatrix，按 <c>T</c> 开头的页码码归档到 <c>页码-{码}</c> 子目录；
    /// 没有页码码的图片归入 <c>异常-无法识别的图片</c>。
    /// 单张失败不中断整批：失败明细进 <see cref="DataMatrixImageClassifyOutcome.Failures"/>。
    /// </summary>
    DataMatrixImageClassifyOutcome Classify(
        DataMatrixImageClassifyRequest request,
        IProgress<DataMatrixImageClassifyProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 单张图片的页码码读取（业务动作）。抽出接口是为了让归档/搬运规则能被单测覆盖：
/// Aspose.BarCode 评估版会截断末字符并追加水印，真解码结果不可用于断言目录名。
/// </summary>
public interface IImagePageCodeReader
{
    /// <summary>返回第一处 <c>T</c> 开头的 DataMatrix 内容（不含 <c>T</c>）；没有则返回 <c>null</c>。</summary>
    string? ReadPageCode(string imageFile);
}

