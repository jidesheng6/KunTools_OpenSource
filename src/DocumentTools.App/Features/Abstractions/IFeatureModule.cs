using System.Windows;
using DocumentTools.Core.Features;

namespace DocumentTools.App.Features.Abstractions;

/// <summary>
/// Plugin-style feature: self-describes metadata and creates its WPF view.
/// Shell only enumerates modules — never hard-codes page types.
/// </summary>
public interface IFeatureModule
{
    FeatureDescriptor Descriptor { get; }

    FrameworkElement CreateView(IServiceProvider services);
}

public abstract class FeatureModuleBase : IFeatureModule
{
    protected FeatureModuleBase(FeatureDescriptor descriptor)
    {
        Descriptor = descriptor;
    }

    public FeatureDescriptor Descriptor { get; }

    public abstract FrameworkElement CreateView(IServiceProvider services);
}
