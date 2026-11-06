using System.Runtime.Versioning;

namespace DocumentTools.Core.Services.System;

/// <summary>
/// 状态机 + 钩子生命周期。真正的吞键逻辑在 <see cref="Win32KeyboardHook"/>，
/// 这里只保证「装一次、拆一次」，避免 legacy 里重复挂事件导致多次卸载不全的问题。
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class KeyboardLockService : IKeyboardLockService
{
    private readonly Func<IKeyboardHook> _hookFactory;
    private IKeyboardHook? _hook;

    public KeyboardLockService()
        : this(static () => new Win32KeyboardHook())
    {
    }

    /// <summary>测试用：注入假钩子，避免在测试进程里真的吞掉按键。</summary>
    public KeyboardLockService(Func<IKeyboardHook> hookFactory)
    {
        _hookFactory = hookFactory ?? throw new ArgumentNullException(nameof(hookFactory));
    }

    public bool IsLocked => _hook is not null;

    public void Lock()
    {
        if (_hook is not null)
        {
            return;
        }

        IKeyboardHook hook = _hookFactory();
        try
        {
            hook.Install();
        }
        catch
        {
            hook.Dispose();
            throw;
        }

        _hook = hook;
    }

    public void Unlock()
    {
        IKeyboardHook? hook = _hook;
        if (hook is null)
        {
            return;
        }

        _hook = null;
        try
        {
            hook.Remove();
        }
        finally
        {
            hook.Dispose();
        }
    }

    public void Dispose() => Unlock();
}
