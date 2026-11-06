namespace DocumentTools.Core.Services.Assets;

/// <summary>
/// Releases/opens payload files that used to live in WinForms Resources.resx.
/// Files are loaded from <c>embedded/</c> next to the executable (copied from assets/embedded).
/// </summary>
public interface IEmbeddedAssetService
{
    IReadOnlyList<string> ListAvailableKeys();

    bool Exists(string key);

    /// <summary>Copy asset bytes/text to <paramref name="targetPath"/>.</summary>
    void ReleaseToFile(string key, string targetPath);

    /// <summary>Open a known download URL resource (text file containing one URL).</summary>
    string? GetText(string key);
}
