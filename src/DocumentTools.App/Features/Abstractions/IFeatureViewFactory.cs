using System.Windows;

namespace DocumentTools.App.Features.Abstractions;

public interface IFeatureViewFactory
{
    FrameworkElement? Create(string featureId);
}
