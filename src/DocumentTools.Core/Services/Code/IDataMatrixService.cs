namespace DocumentTools.Core.Services.Code;

/// <summary>One decoded DataMatrix payload classified like legacy Util.ReadDocumentDataMatrix.</summary>
public sealed record DataMatrixFinding(string Category, string Value);

/// <summary>Findings for a single rendered page.</summary>
public sealed record DataMatrixPageResult(int PageNumber, IReadOnlyList<DataMatrixFinding> Findings);

/// <summary>Document-level decode result for TreeView binding.</summary>
public sealed record DataMatrixDocumentResult(
    string FilePath,
    IReadOnlyList<DataMatrixPageResult> Pages);

public interface IDataMatrixService
{
    /// <summary>
    /// Renders each page of Word/PDF and decodes DataMatrix codes
    /// (port of <c>Util.ReadDocumentDataMatrix</c>).
    /// </summary>
    DataMatrixDocumentResult ReadDocument(string documentFile, int dpi = 300);
}
