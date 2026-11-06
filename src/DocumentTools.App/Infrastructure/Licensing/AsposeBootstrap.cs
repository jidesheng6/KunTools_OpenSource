using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using DocumentTools.Core.Licensing;

namespace DocumentTools.App.Infrastructure.Licensing;

/// <summary>
/// Must run before any Aspose API / UI that touches Aspose types.
/// Order: console+log → EnableLog → ShowHookDetails → StartHook → dump status → optional .lic
/// </summary>
public static class AsposeBootstrap
{
    private static readonly object Sync = new();
    private static string? _logPath;

    public static string LogPath => _logPath ?? Path.Combine(AppContext.BaseDirectory, "logs", "aspose-hook.log");

    public static AsposeLicenseStatus Start(bool showHookDetails = true)
    {
        lock (Sync)
        {
            EnsureLogSink();
            TryEnableHookLog();

            Log("==== AsposeBootstrap.Start ====");
            Log($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
            Log($"BaseDirectory: {AppContext.BaseDirectory}");
            Log($"Hook assembly: {typeof(Aspose.Hook.HookManager).Assembly.Location}");
            Log($"Hook version: {typeof(Aspose.Hook.HookManager).Assembly.GetName().Version}");

            // Preload product assemblies so StartHook can see/bind them.
            PreloadAsposeAssemblies();
            LogLoadedAsposeAssemblies("before-StartHook");

            Aspose.Hook.HookManager.ShowHookDetails(showHookDetails);
            Aspose.Hook.HookManager.StartHook();

            DumpHookStatus("after-StartHook");
            LogLoadedAsposeAssemblies("after-StartHook");

            var status = AsposeLicenseLoader.TryLoad();
            Log($"License: IsLicensed={status.IsLicensed}, Path={status.LicensePath ?? "(evaluation)"}");
            Log("==== AsposeBootstrap.Start done ====");
            return status;
        }
    }

    /// <summary>Call after first Aspose use to see whether hooks still attached.</summary>
    public static void DumpHookStatus(string phase)
    {
        try
        {
            Log($"---- Hook status ({phase}) ----");
            Type manager = typeof(Aspose.Hook.HookManager);

            var started = manager.GetField("mHookStarted", BindingFlags.NonPublic | BindingFlags.Static);
            Log($"mHookStarted = {started?.GetValue(null)}");

            var statusField = manager.GetField("mHookStatus", BindingFlags.NonPublic | BindingFlags.Static);
            if (statusField?.GetValue(null) is System.Collections.IEnumerable statusList)
            {
                int i = 0;
                foreach (object? item in statusList)
                {
                    Log($"  mHookStatus[{i++}] = {DescribeHookItem(item)}");
                }

                if (i == 0)
                {
                    Log("  mHookStatus: (empty)");
                }
            }

            var licensed = manager.GetField("mAssembliesLicenseSetted", BindingFlags.NonPublic | BindingFlags.Static);
            if (licensed?.GetValue(null) is System.Collections.IEnumerable licList)
            {
                int i = 0;
                foreach (object? item in licList)
                {
                    Log($"  mAssembliesLicenseSetted[{i++}] = {item}");
                }
            }

            Type? hookMgrType = manager.Assembly.GetType("Crane.MethodHook.MethodHookManager");
            object? instance = hookMgrType?.GetField("Instance", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null)
                               ?? hookMgrType?.GetProperty("Instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
            var listProp = hookMgrType?.GetProperty("MethodHookList");
            if (listProp?.GetValue(instance) is System.Collections.IEnumerable hooks)
            {
                int i = 0;
                foreach (object? hook in hooks)
                {
                    Log($"  MethodHook[{i++}] = {DescribeHook(hook)}");
                }

                if (i == 0)
                {
                    Log("  MethodHookList: (empty) — Hook may not have attached (version mismatch?)");
                }
            }
        }
        catch (Exception ex)
        {
            Log($"DumpHookStatus failed: {ex}");
        }
    }

    private static string DescribeHookItem(object? item)
    {
        if (item is null)
        {
            return "null";
        }

        Type t = item.GetType();
        object? method = t.GetProperty("Method")?.GetValue(item);
        object? enabled = t.GetProperty("Enabled")?.GetValue(item);
        object? hook = t.GetProperty("Hook")?.GetValue(item);
        return $"Method={method}, Enabled={enabled}, Hook={DescribeHook(hook)}";
    }

    private static string DescribeHook(object? hook)
    {
        if (hook is null)
        {
            return "null";
        }

        Type t = hook.GetType();
        object? enabled = t.GetProperty("IsEnabled")?.GetValue(hook);
        object? source = t.GetProperty("SourceMethod")?.GetValue(hook);
        object? target = t.GetProperty("TargetMethod")?.GetValue(hook);
        return $"Enabled={enabled}, Source={source}, Target={target}";
    }

    private static void PreloadAsposeAssemblies()
    {
        try
        {
            // Touch types so the assemblies are loaded before hooking.
            _ = typeof(Aspose.Words.Document).Assembly.FullName;
            _ = typeof(Aspose.Pdf.Document).Assembly.FullName;
            _ = typeof(Aspose.Cells.Workbook).Assembly.FullName;
            _ = typeof(Aspose.Slides.Presentation).Assembly.FullName;
            _ = typeof(Aspose.BarCode.BarCodeRecognition.BarCodeReader).Assembly.FullName;
            Log("PreloadAsposeAssemblies OK");
        }
        catch (Exception ex)
        {
            Log($"PreloadAsposeAssemblies failed: {ex.Message}");
        }
    }

    private static void LogLoadedAsposeAssemblies(string phase)
    {
        Log($"---- Loaded Aspose assemblies ({phase}) ----");
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies()
                     .Where(a => a.GetName().Name?.StartsWith("Aspose.", StringComparison.OrdinalIgnoreCase) == true)
                     .OrderBy(a => a.GetName().Name))
        {
            var name = asm.GetName();
            Log($"  {name.Name} {name.Version} @ {asm.Location}");
        }
    }

    private static void TryEnableHookLog()
    {
        try
        {
            Type? utils = typeof(Aspose.Hook.HookManager).Assembly.GetType("Crane.MethodHook.Utils");
            utils?.GetMethod("EnableLog", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            Log("Crane.MethodHook.Utils.EnableLog() OK");
        }
        catch (Exception ex)
        {
            Log($"EnableLog failed: {ex.Message}");
        }
    }

    private static void EnsureLogSink()
    {
        string dir = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(dir);
        _logPath = Path.Combine(dir, "aspose-hook.log");

        try
        {
            AllocConsole();
            Console.Title = "DocumentTools · Aspose.Hook log";
            Console.OutputEncoding = Encoding.UTF8;
        }
        catch
        {
            // already has console or alloc failed — file log still works
        }

        Log($"Log file: {_logPath}");
    }

    public static void Log(string message)
    {
        string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        try
        {
            Console.WriteLine(line);
        }
        catch
        {
            // ignore
        }

        try
        {
            File.AppendAllText(LogPath, line + Environment.NewLine, Encoding.UTF8);
        }
        catch
        {
            // ignore
        }
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AllocConsole();
}
