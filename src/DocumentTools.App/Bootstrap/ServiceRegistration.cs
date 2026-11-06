using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Features.Catalog;
using DocumentTools.App.Features.Modules.About;
using DocumentTools.App.Features.Modules.Convert;
using DocumentTools.App.Features.Modules.Exam;
using DocumentTools.App.Features.Modules.Home;
using DocumentTools.App.Features.Modules.SystemTools;
using DocumentTools.App.Features.Modules.Tools;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Infrastructure.Plugins;
using DocumentTools.App.Shell;
using DocumentTools.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Bootstrap;

public static class ServiceRegistration
{
    public static IServiceCollection AddDocumentToolsApp(this IServiceCollection services)
    {
        services.AddDocumentToolsCore();

        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IFeatureViewFactory, FeatureViewFactory>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<MainWindow>();

        // OCR 是可选组件：目录不存在时这里拿到的是「缺失 + 中文原因」，页面据此禁用 OCR 选项。
        services.AddSingleton(OcrPluginLoader.Load(AppContext.BaseDirectory));

        services.AddFeatureModule<HomeFeatureModule>();
        services.AddFeatureModule<AboutFeatureModule>();
        services.AddFeatureModule<IdCardFeatureModule>();
        services.AddFeatureModule<NameVerticalFeatureModule>();
        services.AddFeatureModule<BarcodeFeatureModule>();

        foreach (var convert in ConvertFeatureModule.CreateAll())
        {
            services.AddSingleton<IFeatureModule>(convert);
        }

        foreach (var tool in SimpleToolModules.CreateAll())
        {
            services.AddSingleton(tool);
        }

        foreach (var exam in ExamFeatureModules.CreateAll())
        {
            services.AddSingleton(exam);
        }

        foreach (var sys in SystemFeatureModules.CreateAll())
        {
            services.AddSingleton(sys);
        }

        return services;
    }

    public static IServiceCollection AddFeatureModule<TModule>(this IServiceCollection services)
        where TModule : class, IFeatureModule
    {
        services.AddSingleton<IFeatureModule, TModule>();
        return services;
    }
}
