using Microsoft.Extensions.DependencyInjection;
using System;

namespace DocumentTools.Services
{
    /// <summary>
    /// 服务定位器：提供便捷的服务访问方法（用于向后兼容场景）
    /// 注意：优先使用依赖注入，此定位器仅用于无法注入的场景（如静态方法）
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;

        /// <summary>
        /// 初始化服务定位器
        /// </summary>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public static T GetService<T>() where T : class
        {
            return _serviceProvider?.GetService<T>();
        }

        /// <summary>
        /// 获取必需的服务实例（如果不存在则抛出异常）
        /// </summary>
        public static T GetRequiredService<T>() where T : class
        {
            if (_serviceProvider == null)
                throw new InvalidOperationException("ServiceProvider 未初始化。");
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 获取服务实例（非泛型版本）
        /// </summary>
        public static object GetService(Type serviceType)
        {
            return _serviceProvider?.GetService(serviceType);
        }
    }
}
