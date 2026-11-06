using DocumentTools.Core.Services.System;

namespace DocumentTools.Tests;

/// <summary>
/// 键盘锁定状态机。真实钩子会吞掉整个测试进程的按键，所以只验证「装一次、拆一次」的契约，
/// 钩子本身用假实现（<see cref="IKeyboardHook"/> 就是为这个留的缝）。
/// </summary>
public sealed class KeyboardLockServiceTests
{
    private sealed class FakeHook : IKeyboardHook
    {
        private readonly bool _failInstall;

        public FakeHook(bool failInstall)
        {
            _failInstall = failInstall;
        }

        public int InstallCount { get; private set; }

        public int RemoveCount { get; private set; }

        public int DisposeCount { get; private set; }

        public void Install()
        {
            InstallCount++;
            if (_failInstall)
            {
                throw new InvalidOperationException("模拟钩子安装失败");
            }
        }

        public void Remove() => RemoveCount++;

        public void Dispose() => DisposeCount++;
    }

    [Fact]
    public void Lock_InstallsHookOnce_AndReportsLocked()
    {
        var hook = new FakeHook(failInstall: false);
        using var service = new KeyboardLockService(() => hook);

        Assert.False(service.IsLocked);
        service.Lock();

        Assert.True(service.IsLocked);
        Assert.Equal(1, hook.InstallCount);
        Assert.Equal(0, hook.RemoveCount);
    }

    [Fact]
    public void Lock_IsIdempotent()
    {
        var hook = new FakeHook(failInstall: false);
        using var service = new KeyboardLockService(() => hook);

        service.Lock();
        service.Lock();
        service.Lock();

        Assert.Equal(1, hook.InstallCount);
        Assert.True(service.IsLocked);
    }

    [Fact]
    public void Unlock_RemovesAndDisposes_AndAllowsRelock()
    {
        var hook = new FakeHook(failInstall: false);
        var second = new FakeHook(failInstall: false);
        var queue = new Queue<FakeHook>([hook, second]);
        using var service = new KeyboardLockService(() => queue.Dequeue());

        service.Lock();
        service.Unlock();

        Assert.False(service.IsLocked);
        Assert.Equal(1, hook.RemoveCount);
        Assert.Equal(1, hook.DisposeCount);

        service.Lock();
        Assert.True(service.IsLocked);
        Assert.Equal(1, second.InstallCount);
    }

    [Fact]
    public void Unlock_WithoutLock_IsNoOp()
    {
        using var service = new KeyboardLockService(() => new FakeHook(failInstall: false));

        service.Unlock();
        service.Unlock();

        Assert.False(service.IsLocked);
    }

    [Fact]
    public void Dispose_Unlocks()
    {
        var hook = new FakeHook(failInstall: false);
        var service = new KeyboardLockService(() => hook);

        service.Lock();
        service.Dispose();

        Assert.False(service.IsLocked);
        Assert.Equal(1, hook.RemoveCount);
        Assert.Equal(1, hook.DisposeCount);
    }

    [Fact]
    public void Lock_WhenHookFails_StaysUnlocked_AndSurfacesError()
    {
        var hook = new FakeHook(failInstall: true);
        using var service = new KeyboardLockService(() => hook);

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() => service.Lock());

        Assert.Contains("模拟钩子安装失败", ex.Message, StringComparison.Ordinal);
        Assert.False(service.IsLocked);
        Assert.Equal(1, hook.DisposeCount);
    }
}
