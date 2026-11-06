using System.Windows;
using DocumentTools.App.Features.Abstractions;

namespace DocumentTools.App.Features.Catalog;

public sealed class FeatureViewFactory : IFeatureViewFactory
{
    private readonly IReadOnlyDictionary<string, IFeatureModule> _modules;
    private readonly IServiceProvider _services;

    public FeatureViewFactory(IEnumerable<IFeatureModule> modules, IServiceProvider services)
    {
        _services = services;
        _modules = modules.ToDictionary(
            m => m.Descriptor.Id,
            m => m,
            StringComparer.OrdinalIgnoreCase);
    }

    public FrameworkElement? Create(string featureId)
    {
        if (!_modules.TryGetValue(featureId, out var module))
        {
            return null;
        }

        return module.CreateView(_services);
    }
}
