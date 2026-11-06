namespace DocumentTools.Core.Services.System;

/// <summary>
/// 「锁定/解锁键盘」。对应 legacy <c>DocumentTools/BodyForm.cs:3933-3956</c>
/// （MouseKeyHook 的全局 KeyDown/KeyUp 里 <c>e.Handled = true</c> 吞掉全部按键）。
/// </summary>
public interface IKeyboardLockService : IDisposable
{
    bool IsLocked { get; }

    /// <summary>安装全局键盘钩子并吞掉所有按键；重复调用无副作用。</summary>
    void Lock();

    /// <summary>卸载钩子；未锁定时无副作用。</summary>
    void Unlock();
}

/// <summary>键盘钩子的可替换实现，便于在没有真实全局钩子的环境里测试状态机。</summary>
public interface IKeyboardHook : IDisposable
{
    void Install();

    void Remove();
}
