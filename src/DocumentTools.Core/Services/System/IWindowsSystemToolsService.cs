namespace DocumentTools.Core.Services.System;

public interface IWindowsSystemToolsService
{
    bool IsAdministrator();

    /// <summary>Sets FlightSettingsMaxPauseDays (legacy Util.SetPauseUpdateTime). Requires admin.</summary>
    void SetPauseUpdateDays(int days);

    void OpenWindowsUpdateSettings();

    /// <summary>Hide shortcut overlay arrow (Shell Icons 29). Requires admin.</summary>
    void HideShortcutArrow();

    /// <summary>Restore default shortcut arrow. Requires admin.</summary>
    void RestoreShortcutArrow();

    /// <summary>Disable StickyKeys via SystemParametersInfo (SKF_STICKYKEYSON off).</summary>
    void DisableStickyKeys();

    /// <summary>Open Ease of Access keyboard settings (filter keys etc.).</summary>
    void OpenKeyboardEaseOfAccess();

    /// <summary>
    /// HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System 的 EnableLUA 是否为 1。
    /// 对应 legacy Util.GetAdministratorBehavior（<c>DocumentTools/Util.cs:3528-3537</c>）。
    /// </summary>
    bool IsUacEnabled();

    /// <summary>
    /// 写入 EnableLUA（需管理员）。对应 legacy Util.SetAdministratorBehavior
    /// （<c>DocumentTools/Util.cs:5477-5497</c>）：改完要重启电脑才生效。
    /// </summary>
    void SetUacEnabled(bool enabled);
}
