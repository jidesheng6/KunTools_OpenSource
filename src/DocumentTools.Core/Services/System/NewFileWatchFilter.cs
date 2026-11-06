namespace DocumentTools.Core.Services.System;

/// <summary>
/// 「路径监视」的过滤与延迟规则，对应 legacy TabPage_36 的 groupBox69：
/// <c>DocumentTools/BodyForm.cs:7725-7791</c>（扩展名过滤 + <c>WatcherTimeDelayOut</c> 延迟 + <c>File.Exists</c> 二次确认）。
/// legacy 的忽略列表用 <c>Path.GetExtension</c> 判断 ".jpg.err"，永远匹配不上（只返回 ".err"），
/// 这里按「整名以忽略后缀结尾」实现，可观察行为与 legacy 一致（这些文件本来也不在目标扩展名里），但意图是真的。
/// </summary>
public static class NewFileWatchFilter
{
    /// <summary>legacy WatcherTimeDelayOut.Value 默认值（Designer :14124-14128）。</summary>
    public const int DefaultDelayMilliseconds = 500;

    /// <summary>legacy WatcherTimeDelayOut.Minimum（Designer :14116-14120）。</summary>
    public const int MinDelayMilliseconds = 100;

    /// <summary>legacy WatcherTimeDelayOut.Maximum（Designer :14111-14115）。</summary>
    public const int MaxDelayMilliseconds = 6_553_600;

    public static readonly IReadOnlyList<string> TargetExtensions = [".jpg", ".tif", ".tiff", ".bmp"];

    public static readonly IReadOnlyList<string> IgnoredExtensions =
        [".jpg.err", ".jpg.han", ".tif.err", ".tif.han"];

    /// <summary>新建文件是否应该列进列表：先排除引擎产物（.jpg.err 等），再看扩展名。</summary>
    public static bool IsTargetFile(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        string name = fileName;
        foreach (string ignored in IgnoredExtensions)
        {
            if (name.EndsWith(ignored, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        string ext = Path.GetExtension(name);
        foreach (string target in TargetExtensions)
        {
            if (ext.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>把用户填的延迟夹到 legacy 控件的范围内；空/非法值取默认 500ms。</summary>
    public static int NormalizeDelay(int? milliseconds)
        => Math.Clamp(milliseconds ?? DefaultDelayMilliseconds, MinDelayMilliseconds, MaxDelayMilliseconds);

    /// <summary>解析文本框里的延迟，非法返回默认值。需要区分「用户填了非数字」时改用 <see cref="TryParseDelay"/>。</summary>
    public static int ParseDelay(string? text) => TryParseDelay(text, out int delay) ? delay : DefaultDelayMilliseconds;

    /// <summary>
    /// 解析文本框里的延迟。空白视为「用默认值」（返回 true，<paramref name="delay"/> = 默认值）；
    /// 填了但不是整数时返回 false，<paramref name="delay"/> 仍给出默认值——调用方据此在状态行提示用户，
    /// 不能静默纠正，否则用户会以为生效的是自己填的数字。
    /// </summary>
    public static bool TryParseDelay(string? text, out int delay)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            delay = DefaultDelayMilliseconds;
            return true;
        }

        if (int.TryParse(text.Trim(), out int value))
        {
            delay = NormalizeDelay(value);
            return true;
        }

        delay = DefaultDelayMilliseconds;
        return false;
    }
}
