namespace DocumentTools.App.Shell;

/// <summary>
/// 启动落点解析：配置里的 StartFeatureId 可能指向已被移除的功能，或指向一个已被隐藏的功能。
/// 抽成纯函数，让 <see cref="MainWindow"/> 与自检脚本共用同一套判定，避免两处各写一份。
/// </summary>
public static class NavigationFallback
{
    /// <summary>兜底落点：首页永远存在，即使它自己被隐藏（隐藏只影响导航列表）。</summary>
    public const string DefaultFeatureId = "home";

    public static StartResolution Resolve(
        string? configured,
        IEnumerable<string> availableIds,
        IEnumerable<string>? hiddenIds = null)
    {
        var available = new HashSet<string>(availableIds, StringComparer.OrdinalIgnoreCase);
        var hidden = new HashSet<string>(hiddenIds ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        string wanted = configured?.Trim() ?? string.Empty;

        if (wanted.Length == 0 || string.Equals(wanted, DefaultFeatureId, StringComparison.OrdinalIgnoreCase))
        {
            return new StartResolution(
                DefaultFeatureId,
                hidden.Contains(DefaultFeatureId) ? "首页已被隐藏，导航里不再显示它；启动仍停在首页。" : null);
        }

        if (!available.Contains(wanted))
        {
            return new StartResolution(DefaultFeatureId, $"启动功能「{wanted}」没有注册，已回到首页。");
        }

        if (hidden.Contains(wanted))
        {
            return new StartResolution(DefaultFeatureId, $"启动功能「{wanted}」已被隐藏，已回到首页。");
        }

        return new StartResolution(wanted, null);
    }
}

/// <summary>落点功能 + 需要向用户说明的原因（null 表示按配置原样打开）。</summary>
public readonly record struct StartResolution(string FeatureId, string? Notice);
