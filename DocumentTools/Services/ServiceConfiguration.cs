using System;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.Services
{
    /// <summary>
    /// 应用服务注册中心，负责配置依赖注入容器。
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// 配置并返回全局服务提供者。
        /// </summary>
        public static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            RegisterServices(services);
            return services.BuildServiceProvider();
        }

        /// <summary>
        /// 注册项目中用到的所有服务。
        /// </summary>
        public static void RegisterServices(IServiceCollection services)
        {
            // 窗体本身
            services.AddSingleton<BodyForm>();

            // 窗体访问器
            services.AddSingleton<IFormAccessor>(sp => new FormAccessor(sp.GetRequiredService<BodyForm>()));

            // 服务层
            services.AddSingleton<IDocumentConversionService, DocumentConversionService>();
            services.AddSingleton<IFileOperationService, FileOperationService>();
            services.AddSingleton<IUIHelperService, UIHelperService>();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IRegistryService, RegistryService>();
            services.AddSingleton<IAudioService, AudioService>();
            services.AddSingleton<IImageProcessingService, ImageProcessingService>();
            services.AddSingleton<IUtilityService, UtilityService>();
            services.AddSingleton<TextProcessingService>();
        }
    }
}
