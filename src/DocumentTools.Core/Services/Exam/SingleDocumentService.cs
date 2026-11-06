using Aspose.Words;
using Aspose.Words.Drawing;
using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Exam;

/// <summary>Port of Util.SingleDocumentReplacePersonCode.</summary>
public sealed class SingleDocumentService : ISingleDocumentService
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff", ".gif"];

    public void ReplacePersonImages(
        string templatePath,
        IReadOnlyList<string> altTexts,
        string personImagePath,
        string outputPath)
    {
        PathGuard.RequireExistingFile(templatePath, nameof(templatePath));
        PathGuard.RequireExistingFile(personImagePath, nameof(personImagePath));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        ReplacePersonImagesCore(templatePath, altTexts, image => image.SetImage(personImagePath), outputPath);
    }

    /// <summary>
    /// 共用替换逻辑：<paramref name="applyImage"/> 决定这一份文档用哪张图（外部文件路径或内置码图字节）。
    /// 匹配规则严格照 legacy <c>Util.cs:5672-5740</c>：只认 <c>shape.IsImage</c> 且 Alt 非空且精确命中。
    /// </summary>
    private static void ReplacePersonImagesCore(
        string templatePath,
        IReadOnlyList<string> altTexts,
        Action<ImageData> applyImage,
        string outputPath)
    {
        PathGuard.RequireExistingFile(templatePath, nameof(templatePath));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        if (altTexts.Count == 0)
        {
            throw new ArgumentException("至少需要一个图片替代文字（Alt）匹配项。");
        }

        var document = new Document(templatePath);
        var targets = new HashSet<string>(altTexts.Where(t => !string.IsNullOrWhiteSpace(t)), StringComparer.Ordinal);

        foreach (Shape shape in document.GetChildNodes(NodeType.Shape, true).OfType<Shape>())
        {
            if (!shape.IsImage || string.IsNullOrEmpty(shape.AlternativeText))
            {
                continue;
            }

            if (targets.Contains(shape.AlternativeText))
            {
                applyImage(shape.ImageData);
            }
        }

        string? dir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        document.Save(outputPath);
    }

    public IReadOnlyList<string> GenerateBatch(
        string templatePath,
        IReadOnlyList<string> altTexts,
        string imageSourcePathOrDirectory,
        string outputDirectory,
        int? maxCount = null)
    {
        PathGuard.RequireExistingFile(templatePath, nameof(templatePath));
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        Directory.CreateDirectory(outputDirectory);

        List<string> images = CollectImages(imageSourcePathOrDirectory);
        if (maxCount is > 0)
        {
            images = images.Take(maxCount.Value).ToList();
        }

        var outputs = new List<string>();
        foreach (string image in images)
        {
            string name = Path.GetFileNameWithoutExtension(image);
            string output = Path.Combine(outputDirectory, name + Path.GetExtension(templatePath));
            if (string.IsNullOrEmpty(Path.GetExtension(output)))
            {
                output += ".docx";
            }

            ReplacePersonImages(templatePath, altTexts, image, output);
            outputs.Add(output);
        }

        return outputs;
    }

    public IReadOnlyList<string> GenerateBatch(SingleDocumentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        PathGuard.RequireExistingFile(request.TemplatePath, nameof(request.TemplatePath));
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OutputDirectory);
        Directory.CreateDirectory(request.OutputDirectory);

        List<BatchImage> images = request.UseBuiltInPersonCode
            ? CollectBuiltInImages(request)
            : CollectExternalImages(request);

        var outputs = new List<string>();
        for (int i = 0; i < images.Count; i++)
        {
            // legacy 的 FileIndex 从 0 起、拼名前自增 → 文件名里的序号是 1 基。
            string suffix = SingleDocumentNaming.BuildSuffix(request.NameSuffixMode, i + 1, request.SuffixWord);
            string fileName = SingleDocumentNaming.BuildFileName(request.TemplatePath, suffix);
            string output = Path.Combine(request.OutputDirectory, fileName);

            BatchImage image = images[i];
            if (image.PersonCodeIndex is int index)
            {
                byte[] bytes = PersonCodeLibrary.ReadImage(index);
                ReplacePersonImagesCore(
                    request.TemplatePath,
                    request.AltTexts,
                    data =>
                    {
                        using var stream = new MemoryStream(bytes, writable: false);
                        data.SetImage(stream);
                    },
                    output);
            }
            else
            {
                ReplacePersonImages(request.TemplatePath, request.AltTexts, image.FilePath!, output);
            }

            outputs.Add(output);
        }

        return outputs;
    }

    /// <summary>
    /// 内置码图清单：从 <see cref="SingleDocumentRequest.PersonCodeStartIndex"/> 起取，
    /// 上限既受 999 收尾限制，也受 <see cref="SingleDocumentRequest.MaxCount"/> 限制。
    /// </summary>
    private static List<BatchImage> CollectBuiltInImages(SingleDocumentRequest request)
    {
        int start = request.PersonCodeStartIndex;
        PersonCodeLibrary.EnsureInRange(start);

        int available = PersonCodeLibrary.LastIndex - start + 1;
        int count = request.MaxCount is > 0 ? Math.Min(request.MaxCount.Value, available) : available;

        var images = new List<BatchImage>(count);
        for (int i = 0; i < count; i++)
        {
            images.Add(new BatchImage(null, start + i));
        }

        return images;
    }

    /// <summary>
    /// 外部素材清单。单张文件时按份数重复（legacy「仅使用单张图片文件」分支，
    /// BodyForm.cs:6409-6436：同一张图重复生成 N 份）；份数留空按 1 份。
    /// 目录时一个文件一份，份数只做上限截断。
    /// </summary>
    private static List<BatchImage> CollectExternalImages(SingleDocumentRequest request)
    {
        List<string> paths = CollectImages(request.ImageSourcePathOrDirectory);
        if (paths.Count == 1)
        {
            int copies = request.MaxCount is > 0 ? request.MaxCount.Value : 1;
            return Enumerable.Repeat(new BatchImage(paths[0], null), copies).ToList();
        }

        if (request.MaxCount is int limit && limit > 0 && limit < paths.Count)
        {
            paths = paths.Take(limit).ToList();
        }

        return paths.Select(path => new BatchImage(path, null)).ToList();
    }

    private static List<string> CollectImages(string imageSourcePathOrDirectory)
    {
        if (File.Exists(imageSourcePathOrDirectory))
        {
            return [imageSourcePathOrDirectory];
        }

        if (Directory.Exists(imageSourcePathOrDirectory))
        {
            return Directory.EnumerateFiles(imageSourcePathOrDirectory)
                .Where(f => ImageExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        throw new DirectoryNotFoundException("图片源不存在：" + imageSourcePathOrDirectory);
    }

    /// <summary>一份产物用哪张图：外部素材给 <see cref="FilePath"/>，内置码图给 <see cref="PersonCodeIndex"/>。</summary>
    private readonly record struct BatchImage(string? FilePath, int? PersonCodeIndex);
}
