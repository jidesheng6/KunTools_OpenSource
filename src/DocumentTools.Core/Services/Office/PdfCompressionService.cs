using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Optimization;
using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Office;

/// <summary>PDF 压缩：从 sunnyui PdfConversionService.Compress 抽出。</summary>
public sealed class PdfCompressionService : IPdfCompressionService
{
    public PdfCompressionResult Compress(string sourceFile, string outputFile, PdfCompressionOptions? options = null)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(outputFile, nameof(outputFile));
        options ??= new PdfCompressionOptions();

        string? dir = Path.GetDirectoryName(Path.GetFullPath(outputFile));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        long sourceBytes = new FileInfo(sourceFile).Length;

        // 先写临时文件：Aspose.Pdf 重写 PDF（重排对象流/xref、重做字体子集）对低熵文件是净增，
        // 直接把产物交出去等于让用户拿到更大的文件，所以比完字节再决定要不要回退。
        string tempFile = Path.Combine(
            string.IsNullOrEmpty(dir) ? Directory.GetCurrentDirectory() : dir,
            $"{Path.GetFileNameWithoutExtension(outputFile)}.compressing-{Guid.NewGuid():N}.pdf");

        long producedBytes;
        try
        {
            using (var document = new Document(sourceFile))
            {
                if (options.WebOptimize)
                {
                    document.Optimize();
                }

                var optimization = new OptimizationOptions();

                if (options.CompressImages)
                {
                    optimization.ImageCompressionOptions.CompressImages = true;
                    optimization.ImageCompressionOptions.ImageQuality = Math.Clamp(options.ImageQuality, 1, 100);
                    optimization.ImageCompressionOptions.Version = ToAsposeVersion(options.ImageVersion);
                }

                if (options.MaxResolutionDpi > 0)
                {
                    optimization.ImageCompressionOptions.ResizeImages = true;
                    optimization.ImageCompressionOptions.MaxResolution = options.MaxResolutionDpi;
                }

                if (options.UseFlateEncoding)
                {
                    optimization.ImageCompressionOptions.Encoding = ImageEncoding.Flate;
                }

                if (options.FlattenFormFields)
                {
                    foreach (Page page in document.Pages)
                    {
                        foreach (Annotation annotation in page.Annotations)
                        {
                            annotation.Flatten();
                        }
                    }
                }

                optimization.RemoveUnusedStreams = options.RemoveUnusedStreams;
                optimization.LinkDuplicateStreams = options.LinkDuplicateStreams;
                optimization.RemoveUnusedObjects = options.RemoveUnusedObjects;
                optimization.UnembedFonts = options.UnembedFonts;

                document.OptimizeResources(optimization);
                document.Save(tempFile);
            }

            producedBytes = new FileInfo(tempFile).Length;

            if (producedBytes >= sourceBytes)
            {
                File.Copy(sourceFile, outputFile, overwrite: true);
                return new PdfCompressionResult
                {
                    OutputFile = outputFile,
                    SourceBytes = sourceBytes,
                    OutputBytes = sourceBytes,
                    KeptOriginal = true
                };
            }

            File.Move(tempFile, outputFile, overwrite: true);
            return new PdfCompressionResult
            {
                OutputFile = outputFile,
                SourceBytes = sourceBytes,
                OutputBytes = producedBytes,
                KeptOriginal = false
            };
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch (IOException)
                {
                    // 临时文件清不掉不影响结果，交给系统清理临时目录。
                }
            }
        }
    }

    private static ImageCompressionVersion ToAsposeVersion(PdfImageCompressionVersion version)
        => version switch
        {
            PdfImageCompressionVersion.Fast => ImageCompressionVersion.Fast,
            PdfImageCompressionVersion.Mixed => ImageCompressionVersion.Mixed,
            _ => ImageCompressionVersion.Standard
        };
}
