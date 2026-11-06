namespace DocumentTools.Core.Services.Exam;

public interface ISingleDocumentService
{
    /// <summary>
    /// Replace image shapes whose AlternativeText matches <paramref name="altTexts"/>,
    /// then save as new Word document (Util.SingleDocumentReplacePersonCode).
    /// </summary>
    void ReplacePersonImages(
        string templatePath,
        IReadOnlyList<string> altTexts,
        string personImagePath,
        string outputPath);

    /// <summary>
    /// Batch: for each image in <paramref name="imageSourcePathOrDirectory"/> (file or folder),
    /// produce one document named after the image stem.
    /// </summary>
    IReadOnlyList<string> GenerateBatch(
        string templatePath,
        IReadOnlyList<string> altTexts,
        string imageSourcePathOrDirectory,
        string outputDirectory,
        int? maxCount = null);

    /// <summary>
    /// Batch with legacy naming: output name = template stem + suffix + .docx,
    /// suffix driven by <see cref="SingleDocumentRequest.NameSuffixMode"/>
    /// (legacy BodyForm.cs:6286-6311).
    /// </summary>
    IReadOnlyList<string> GenerateBatch(SingleDocumentRequest request);
}
