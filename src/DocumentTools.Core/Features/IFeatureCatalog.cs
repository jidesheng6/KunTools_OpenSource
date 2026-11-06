namespace DocumentTools.Core.Features;

/// <summary>Read-only registry of all discoverable features.</summary>
public interface IFeatureCatalog
{
    IReadOnlyList<FeatureDescriptor> All { get; }

    FeatureDescriptor? Find(string id);

    IReadOnlyList<FeatureDescriptor> InCategory(FeatureCategory category);

    IReadOnlyList<FeatureDescriptor> Search(string? query);
}
