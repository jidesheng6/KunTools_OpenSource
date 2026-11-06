namespace DocumentTools.Core.Features;

/// <summary>Built-in feature registry. UI modules may expose a subset via their own modules.</summary>
public sealed class FeatureCatalog : IFeatureCatalog
{
    private readonly IReadOnlyList<FeatureDescriptor> _all;

    public FeatureCatalog(IEnumerable<FeatureDescriptor>? extra = null)
    {
        var list = new List<FeatureDescriptor>(BuiltInFeatures.All);
        if (extra is not null)
        {
            list.AddRange(extra);
        }

        _all = list
            .GroupBy(f => f.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();
    }

    public IReadOnlyList<FeatureDescriptor> All => _all;

    public FeatureDescriptor? Find(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return _all.FirstOrDefault(f =>
            string.Equals(f.Id, id, StringComparison.OrdinalIgnoreCase));
    }

    public IReadOnlyList<FeatureDescriptor> InCategory(FeatureCategory category)
        => _all.Where(f => f.Category == category).ToList();

    public IReadOnlyList<FeatureDescriptor> Search(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return _all;
        }

        string q = query.Trim();
        return _all.Where(f =>
                f.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                || f.Id.Contains(q, StringComparison.OrdinalIgnoreCase)
                || f.Keywords.Any(k => k.Contains(q, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}
