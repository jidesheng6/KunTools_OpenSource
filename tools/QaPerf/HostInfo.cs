using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace DocumentTools.QaPerf;

/// <summary>
/// Machine / runtime facts captured once per session so the report can state what the numbers
/// were measured on. Captured before any Aspose type is touched (no warming side effects).
/// </summary>
internal sealed class HostInfo
{
    public string OsDescription { get; set; } = string.Empty;

    public string OsArchitecture { get; set; } = string.Empty;

    public string CpuName { get; set; } = string.Empty;

    public int LogicalCores { get; set; }

    public double TotalMemoryGb { get; set; }

    public string RuntimeVersion { get; set; } = string.Empty;

    public string TargetFramework { get; set; } = string.Empty;

    public string ProcessArchitecture { get; set; } = string.Empty;

    public bool ServerGarbageCollection { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public static HostInfo Capture()
    {
        return new HostInfo
        {
            OsDescription = RuntimeInformation.OSDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            CpuName = ReadCpuName(),
            LogicalCores = Environment.ProcessorCount,
            TotalMemoryGb = Math.Round(ReadTotalMemoryGb(), 1),
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            TargetFramework = AppContext.TargetFrameworkName ?? "unknown",
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ServerGarbageCollection = System.Runtime.GCSettings.IsServerGC,
            MachineName = Environment.MachineName
        };
    }

    public string Describe() =>
        $"host       : {CpuName} / {LogicalCores} logical cores / {TotalMemoryGb.ToString("0.0", CultureInfo.InvariantCulture)} GB RAM";
    public override string ToString() => Describe();

    private static string ReadCpuName()
    {
        try
        {
            object? value = Microsoft.Win32.Registry.GetValue(
                @"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0",
                "ProcessorNameString",
                null);
            if (value is string text && !string.IsNullOrWhiteSpace(text))
            {
                return text.Trim();
            }
        }
        catch
        {
            // Registry read is best-effort; the environment table degrades to PROCESSOR_IDENTIFIER.
        }

        return Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER") ?? "unknown CPU";
    }

    private static double ReadTotalMemoryGb()
    {
        MemoryStatusEx status = new() { dwLength = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        return GlobalMemoryStatusEx(ref status)
            ? status.ullTotalPhys / (1024d * 1024d * 1024d)
            : 0d;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx lpBuffer);
}
