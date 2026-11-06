namespace DocumentTools.Core.Features;

/// <summary>
/// Immutable metadata for one user-facing feature.
/// Navigation, search, and operation dispatch all use this record — not Designer control positions.
/// </summary>
public sealed class FeatureDescriptor
{
    public FeatureDescriptor(
        string id,
        string title,
        FeatureCategory category,
        string? description = null,
        params string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("功能 ID 不能为空。", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("功能标题不能为空。", nameof(title));
        }

        Id = id.Trim();
        Title = title.Trim();
        Category = category;
        Description = description?.Trim() ?? string.Empty;
        Keywords = keywords ?? Array.Empty<string>();
    }

    public string Id { get; }

    public string Title { get; }

    public FeatureCategory Category { get; }

    public string Description { get; }

    public IReadOnlyList<string> Keywords { get; }
}
