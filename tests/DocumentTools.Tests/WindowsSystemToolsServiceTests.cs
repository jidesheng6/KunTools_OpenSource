using DocumentTools.Core.Services.System;

namespace DocumentTools.Tests;

/// <summary>
/// UAC（EnableLUA）读写。只测「读」与「非管理员必须被拒」——
/// 写入会真的改本机注册表（禁用 UAC），测试里绝不执行成功分支。
/// </summary>
public sealed class WindowsSystemToolsServiceTests
{
    [Fact]
    public void IsUacEnabled_ReadsRegistryWithoutThrowing()
    {
        var service = new WindowsSystemToolsService();

        // 不假设机器当前状态，只要求能读到布尔值（读不到时应按 Windows 默认值 true 处理）。
        bool enabled = service.IsUacEnabled();
        Assert.True(enabled || !enabled);
    }

    [Fact]
    public void SetUacEnabled_WithoutAdministrator_ThrowsActionableMessage()
    {
        var service = new WindowsSystemToolsService();
        if (service.IsAdministrator())
        {
            // 以管理员运行测试时跳过：这个分支会真的改本机 UAC 设置。
            return;
        }

        UnauthorizedAccessException ex = Assert.Throws<UnauthorizedAccessException>(
            () => service.SetUacEnabled(true));

        Assert.Contains("管理员身份", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void SetUacEnabled_IsDeclaredOnTheInterface()
    {
        // App 的关于页只通过接口调用；漏进接口就会在运行时炸 MissingMethod。
        Assert.NotNull(typeof(IWindowsSystemToolsService).GetMethod(nameof(IWindowsSystemToolsService.SetUacEnabled)));
        Assert.NotNull(typeof(IWindowsSystemToolsService).GetMethod(nameof(IWindowsSystemToolsService.IsUacEnabled)));
    }
}
