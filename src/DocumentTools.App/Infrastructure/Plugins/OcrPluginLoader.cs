using System.Reflection;
using System.Runtime.Loader;
using DocumentTools.Core.Services.Code;

namespace DocumentTools.App.Infrastructure.Plugins;

/// <summary>
/// OCR 组件的运行期状态：要么拿到实现，要么拿到一句可直接展示的中文原因。
/// 页面据此禁用 OCR 选项，而不是给一个「勾了没反应」的假控件。
/// </summary>
public sealed class OcrComponent
{
    private OcrComponent(IOcrImageClassifyService? service, string unavailableReason)
    {
        Service = service;
        UnavailableReason = unavailableReason;
    }

    public IOcrImageClassifyService? Service { get; }

    public string UnavailableReason { get; }

    public bool IsAvailable => Service is not null && Service.IsAvailable;

    public static OcrComponent Missing(string reason) => new(null, reason);

    public static OcrComponent Loaded(IOcrImageClassifyService service)
        => new(service, service.IsAvailable ? string.Empty : service.UnavailableReason);
}

/// <summary>
/// 从 exe 同级 <c>ocr\DocumentTools.Ocr.dll</c> 加载可选 OCR 组件（见 docs\packaging.md）。
/// <para>
/// 三个关键点：① <c>DocumentTools.Core</c> 必须复用宿主已加载的那一份，否则接口类型会变成两个不同身份；
/// ② 组件依赖（Paddle / OpenCV 托管 dll）按发布目录的 deps.json 解析；
/// ③ 原生 dll（paddle_inference_c / OpenCvSharpExtern）不在宿主探测路径里，加载前把 ocr 目录加进 PATH。
/// </para>
/// </summary>
public static class OcrPluginLoader
{
    public const string FolderName = "ocr";
    public const string AssemblyFileName = "DocumentTools.Ocr.dll";

    public const string InstallHint =
        "OCR 组件未安装：把发布产物的 ocr 目录放到程序目录（与 DocumentTools.exe 同级）即可启用。";

    public static OcrComponent Load(string baseDirectory)
    {
        string pluginDirectory = Path.Combine(baseDirectory, FolderName);
        string assemblyPath = Path.Combine(pluginDirectory, AssemblyFileName);
        if (!File.Exists(assemblyPath))
        {
            return OcrComponent.Missing(InstallHint);
        }

        try
        {
            AddNativeSearchPath(pluginDirectory);

            var context = new OcrLoadContext(assemblyPath);
            Assembly assembly = context.LoadFromAssemblyPath(assemblyPath);

            Type? implementation = assembly
                .GetTypes()
                .FirstOrDefault(type =>
                    !type.IsAbstract &&
                    typeof(IOcrImageClassifyService).IsAssignableFrom(type));

            if (implementation is null)
            {
                return OcrComponent.Missing(
                    $"OCR 组件里没有找到 {nameof(IOcrImageClassifyService)} 的实现：{assemblyPath}");
            }

            if (Activator.CreateInstance(implementation) is not IOcrImageClassifyService service)
            {
                return OcrComponent.Missing($"OCR 组件无法实例化：{implementation.FullName}");
            }

            return OcrComponent.Loaded(service);
        }
        catch (Exception ex)
        {
            return OcrComponent.Missing("OCR 组件加载失败：" + ex.Message);
        }
    }

    /// <summary>Windows 的 LoadLibrary 兜底会查 PATH；不加这一段，Paddle 的 P/Invoke 找不到 ocr 目录里的 dll。</summary>
    private static void AddNativeSearchPath(string directory)
    {
        const string name = "PATH";
        string current = Environment.GetEnvironmentVariable(name) ?? string.Empty;
        bool present = current
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Any(entry => string.Equals(
                entry.TrimEnd('\\'),
                directory.TrimEnd('\\'),
                StringComparison.OrdinalIgnoreCase));

        if (!present)
        {
            Environment.SetEnvironmentVariable(name, directory + ";" + current);
        }
    }

    private sealed class OcrLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public OcrLoadContext(string mainAssemblyPath)
            : base("DocumentTools.Ocr")
            => _resolver = new AssemblyDependencyResolver(mainAssemblyPath);

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            string? name = assemblyName.Name;
            if (name is null)
            {
                return null;
            }

            // 宿主已加载的契约与框架程序集：交回 Default 上下文，保证类型身份一致。
            if (name is "DocumentTools.Core" or "System" or "netstandard"
                || name.StartsWith("System.", StringComparison.Ordinal)
                || name.StartsWith("Microsoft.", StringComparison.Ordinal))
            {
                return null;
            }

            string? path = _resolver.ResolveAssemblyToPath(assemblyName);
            return path is null ? null : LoadFromAssemblyPath(path);
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
        }
    }
}
