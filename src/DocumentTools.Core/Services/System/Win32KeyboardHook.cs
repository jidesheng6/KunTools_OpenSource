using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DocumentTools.Core.Services.System;

/// <summary>
/// WH_KEYBOARD_LL 低层键盘钩子：钩子回调一律返回 1（= 已处理），把所有按键吞掉。
/// 与 legacy 的 MouseKeyHook 行为一致：Ctrl+Alt+Del 属于 SAS，钩不住；
/// 进程退出时系统会自动撤销钩子，所以即使异常退出也不会把用户永久锁在键盘外。
/// </summary>
[SupportedOSPlatform("windows")]
internal sealed class Win32KeyboardHook : IKeyboardHook
{
    private const int WhKeyboardLl = 13;

    // 委托必须存成字段保活，否则会被 GC 回收，回调地址失效直接崩溃。
    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hookId = IntPtr.Zero;

    public Win32KeyboardHook()
    {
        _proc = HookCallback;
    }

    public void Install()
    {
        if (_hookId != IntPtr.Zero)
        {
            return;
        }

        _hookId = SetWindowsHookEx(WhKeyboardLl, _proc, GetModuleHandle(null), 0);
        if (_hookId == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                "无法锁定键盘：安装键盘钩子失败（" + Marshal.GetLastWin32Error() + "）。"
                + "可能是安全软件拦截了全局钩子，请换一台机器或先用「键盘辅助设置」处理。");
        }
    }

    public void Remove()
    {
        if (_hookId == IntPtr.Zero)
        {
            return;
        }

        IntPtr id = _hookId;
        _hookId = IntPtr.Zero;
        UnhookWindowsHookEx(id);
    }

    public void Dispose() => Remove();

    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            // 吞掉全部按键：让用户只能通过鼠标点「解锁键盘」恢复。
            return 1;
        }

        return CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);
}
