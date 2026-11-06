using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Principal;
using Microsoft.Win32;

namespace DocumentTools.Core.Services.System;

/// <summary>Port of selected Util registry helpers used by TabPage_26.</summary>
[SupportedOSPlatform("windows")]
public sealed class WindowsSystemToolsService : IWindowsSystemToolsService
{
    private const uint SpiGetStickyKeys = 0x003A;
    private const uint SpiSetStickyKeys = 0x003B;
    private const uint SpifUpdateIniFile = 0x01;
    private const uint SpifSendChange = 0x02;
    private const uint SkfStickyKeysOn = 0x00000001;

    /// <summary>legacy Util.GetAdministratorBehavior 读的同一路径。</summary>
    private const string UacPolicyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";

    public bool IsAdministrator()
    {
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public void SetPauseUpdateDays(int days)
    {
        RequireAdmin();
        days = Math.Clamp(days, 1, 365 * 100);
        using RegistryKey key = OpenOrCreate(
            RegistryHive.LocalMachine,
            @"SOFTWARE\Microsoft\WindowsUpdate\UX\Settings");
        key.SetValue("FlightSettingsMaxPauseDays", days, RegistryValueKind.DWord);
    }

    public void OpenWindowsUpdateSettings()
    {
        string uri = IsWindows11()
            ? "ms-settings:windowsupdate"
            : "ms-settings:windowsupdate-options";
        Process.Start(new ProcessStartInfo { FileName = uri, UseShellExecute = true });
    }

    public void HideShortcutArrow()
    {
        RequireAdmin();
        string systemRoot = Environment.GetEnvironmentVariable("systemroot") ?? @"C:\Windows";
        using RegistryKey key = OpenOrCreate(
            RegistryHive.LocalMachine,
            @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons");
        key.SetValue("29", $"{systemRoot}\\system32\\imageres.dll,197", RegistryValueKind.String);
        NotifyIconCache();
    }

    public void RestoreShortcutArrow()
    {
        RequireAdmin();
        using RegistryKey? key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
            .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", writable: true)
            ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", writable: true);
        key?.DeleteValue("29", throwOnMissingValue: false);
        NotifyIconCache();
    }

    public void DisableStickyKeys()
    {
        var keys = new StickyKeys { CbSize = (uint)Marshal.SizeOf<StickyKeys>() };
        if (!SystemParametersInfo(SpiGetStickyKeys, keys.CbSize, ref keys, 0))
        {
            throw new InvalidOperationException("无法读取粘滞键状态。");
        }

        keys.DwFlags &= ~SkfStickyKeysOn;
        if (!SystemParametersInfo(SpiSetStickyKeys, keys.CbSize, ref keys, SpifUpdateIniFile | SpifSendChange))
        {
            throw new InvalidOperationException("无法关闭粘滞键。");
        }
    }

    public void OpenKeyboardEaseOfAccess()
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "ms-settings:easeofaccess-keyboard",
            UseShellExecute = true
        });
    }

    public bool IsUacEnabled()
    {
        using RegistryKey? key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
            .OpenSubKey(UacPolicyPath)
            ?? RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)
                .OpenSubKey(UacPolicyPath);

        // 读不到（权限/策略未配置）按 Windows 默认值 1 处理，与「启用管理员批准模式」一致。
        object? raw = key?.GetValue("EnableLUA");
        return raw switch
        {
            int value => value != 0,
            long value => value != 0,
            string text => !int.TryParse(text, out int parsed) || parsed != 0,
            _ => true
        };
    }

    public void SetUacEnabled(bool enabled)
    {
        RequireAdmin();
        using RegistryKey key = OpenOrCreate(RegistryHive.LocalMachine, UacPolicyPath);
        key.SetValue("EnableLUA", enabled ? 1 : 0, RegistryValueKind.DWord);
    }

    private void RequireAdmin()
    {
        if (!IsAdministrator())
        {
            throw new UnauthorizedAccessException("请使用管理员身份重新运行软件后再执行本操作。");
        }
    }

    private static RegistryKey OpenOrCreate(RegistryHive hive, string path)
    {
        using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64);
        RegistryKey? key = baseKey.CreateSubKey(path, writable: true)
                          ?? RegistryKey.OpenBaseKey(hive, RegistryView.Registry32).CreateSubKey(path, writable: true);
        return key ?? throw new InvalidOperationException("无法打开注册表：" + path);
    }

    private static bool IsWindows11()
    {
        // Build 22000+ ≈ Windows 11
        return Environment.OSVersion.Version.Build >= 22000;
    }

    private static void NotifyIconCache()
    {
        // Best-effort: restart explorer is too aggressive; legacy called SetIconCacheAttr.
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "ie4uinit.exe",
                Arguments = "-show",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }
        catch
        {
            // optional
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref StickyKeys pvParam, uint fWinIni);

    [StructLayout(LayoutKind.Sequential)]
    private struct StickyKeys
    {
        public uint CbSize;
        public uint DwFlags;
    }
}
