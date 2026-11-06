using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Bootstrap;

/// <summary>Composition root helper (tests / alternate hosts).</summary>
public static class AppHost
{
    public static ServiceProvider CreateServiceProvider()
    {
        var collection = new ServiceCollection();
        collection.AddDocumentToolsApp();
        return collection.BuildServiceProvider();
    }
}
