namespace DocumentTools.Core.Services.Code;

/// <summary>One row in the legacy Excel barcode batch template.</summary>
public sealed record BarcodeBatchRow(string FileName, string Caption, string Content);

public enum BarcodeBatchImageFormat
{
    Png,
    Bmp
}

public sealed record BarcodeBatchResult(IReadOnlyList<string> OutputFiles)
{
    public int Count => OutputFiles.Count;
}
