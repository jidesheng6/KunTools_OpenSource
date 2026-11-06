using System.Globalization;

namespace DocumentTools.Core.Settings;

internal enum SettingValueKind
{
    Text,
    Boolean,
    Integer
}

/// <summary>
/// 单字段修正结果。<see cref="RejectReason"/> 非空表示该值不可接受：
/// 读配置时回退默认值并记警告，导入时该字段判失败（不会静默改值）。
/// </summary>
internal sealed record SettingCoercion(string Text, string? Warning = null, string? RejectReason = null)
{
    public bool Rejected => RejectReason is not null;

    public static SettingCoercion Fixed(string text, string warning) => new(text, warning);

    public static SettingCoercion Reject(string reason) => new(string.Empty, null, reason);
}

/// <summary>一个设置项：JSON 名、类型、读写访问器、以及值修正规则（唯一真相）。</summary>
internal sealed record SettingField(
    string Name,
    SettingValueKind Kind,
    Func<AppSettings, string> Get,
    Action<AppSettings, string> Set,
    Func<string, bool, SettingCoercion> Coerce);

/// <summary>
/// 设置字段表。<see cref="Fields"/> 的顺序即导出 JSON 的字段顺序（稳定，便于 diff/评审）。
/// 新增设置项只改这一张表：JSON 读写、导入导出、未知字段识别都从它派生。
/// </summary>
internal static class AppSettingsSchema
{
    /// <summary>用于取默认值的空白实例（默认值只在 <see cref="AppSettings"/> 的初始化器里定义一次）。</summary>
    internal static AppSettings Defaults { get; } = new();

    internal static IReadOnlyList<SettingField> Fields { get; } =
    [
        new(
            "SettingsVersion",
            SettingValueKind.Integer,
            s => s.SettingsVersion.ToString(CultureInfo.InvariantCulture),
            (s, v) => s.SettingsVersion = SettingsText.ParseInt(v, AppSettings.CurrentVersion),
            AppSettingsCoercers.Version),

        new(
            "StartFeatureId",
            SettingValueKind.Text,
            s => s.StartFeatureId,
            (s, v) => s.StartFeatureId = v,
            AppSettingsCoercers.StartFeatureId),

        new(
            "ShowOpenTargetDialog",
            SettingValueKind.Boolean,
            s => SettingsText.Bool(s.ShowOpenTargetDialog),
            (s, v) => s.ShowOpenTargetDialog = SettingsText.ParseBool(v, true),
            AppSettingsCoercers.Boolean("ShowOpenTargetDialog")),

        new(
            "ListViewShowGridLines",
            SettingValueKind.Boolean,
            s => SettingsText.Bool(s.ListViewShowGridLines),
            (s, v) => s.ListViewShowGridLines = SettingsText.ParseBool(v, true),
            AppSettingsCoercers.Boolean("ListViewShowGridLines")),

        new(
            "ListViewLineHeight",
            SettingValueKind.Integer,
            s => s.ListViewLineHeight.ToString(CultureInfo.InvariantCulture),
            (s, v) => s.ListViewLineHeight = SettingsText.ParseInt(v, AppSettings.DefaultLineHeight),
            AppSettingsCoercers.LineHeight),

        new(
            "HighlightActiveNav",
            SettingValueKind.Boolean,
            s => SettingsText.Bool(s.HighlightActiveNav),
            (s, v) => s.HighlightActiveNav = SettingsText.ParseBool(v, true),
            AppSettingsCoercers.Boolean("HighlightActiveNav")),

        new(
            "ChromeDriverPath",
            SettingValueKind.Text,
            s => s.ChromeDriverPath,
            (s, v) => s.ChromeDriverPath = v,
            AppSettingsCoercers.PathValue("ChromeDriverPath", allowDirectory: false)),

        new(
            "VfsoftExamDataPath",
            SettingValueKind.Text,
            s => s.VfsoftExamDataPath,
            (s, v) => s.VfsoftExamDataPath = v,
            AppSettingsCoercers.PathValue("VfsoftExamDataPath", allowDirectory: true)),

        new(
            "VfsoftMssDataPath",
            SettingValueKind.Text,
            s => s.VfsoftMssDataPath,
            (s, v) => s.VfsoftMssDataPath = v,
            AppSettingsCoercers.PathValue("VfsoftMssDataPath", allowDirectory: true)),

        new(
            "HideFeatureIds",
            SettingValueKind.Text,
            s => s.HideFeatureIds,
            (s, v) => s.HideFeatureIds = v,
            AppSettingsCoercers.FeatureIdList)
    ];

    /// <summary>按 JSON 名查找字段（大小写不敏感：手工编辑写成 camelCase 也能读）。</summary>
    internal static SettingField? Find(string name)
        => Fields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
}

/// <summary>各字段的值修正规则。全部返回 <see cref="SettingCoercion"/>，不抛异常。</summary>
internal static class AppSettingsCoercers
{
    /// <summary>路径里永远不可能合法的字符（不含盘符冒号与 UNC 前缀里的 ?）。</summary>
    private static readonly char[] PathTraps = ['"', '<', '>', '|', '\0', '\n', '\r', '\t'];

    public static SettingCoercion Version(string raw, bool strict)
    {
        string text = raw.Trim();
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            return SettingCoercion.Reject($"「{SettingsText.Short(text)}」不是整数");
        }

        int clamped = Math.Clamp(value, 1, AppSettings.CurrentVersion);
        return clamped == value
            ? new SettingCoercion(text)
            : SettingCoercion.Fixed(
                clamped.ToString(CultureInfo.InvariantCulture),
                $"SettingsVersion={value} 超出 1–{AppSettings.CurrentVersion}，已按 {clamped} 处理（未知字段已忽略）");
    }

    public static SettingCoercion StartFeatureId(string raw, bool strict)
    {
        string text = raw.Trim();
        if (text.Length == 0)
        {
            return SettingCoercion.Fixed(
                AppSettings.DefaultStartFeatureId,
                $"StartFeatureId 为空，已用 {AppSettings.DefaultStartFeatureId}");
        }

        return string.Equals(text, raw, StringComparison.Ordinal)
            ? new SettingCoercion(text)
            : SettingCoercion.Fixed(text, $"StartFeatureId 已去掉首尾空白：「{SettingsText.Short(raw)}」→「{text}」");
    }

    /// <summary>布尔值：JSON 原生 true/false，同时接受 legacy App.config 的 on/off 与 1/0/y​es/no。</summary>
    public static Func<string, bool, SettingCoercion> Boolean(string fieldName)
        => (raw, strict) =>
        {
            string text = raw.Trim();
            if (IsTrue(text))
            {
                return new SettingCoercion(SettingsText.Bool(true));
            }

            if (IsFalse(text))
            {
                return new SettingCoercion(SettingsText.Bool(false));
            }

            return SettingCoercion.Reject($"{fieldName} 的「{SettingsText.Short(text)}」不是布尔值（可用 true/false 或 on/off）");
        };

    public static SettingCoercion LineHeight(string raw, bool strict)
    {
        string text = raw.Trim();
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
        {
            return SettingCoercion.Reject($"ListViewLineHeight 的「{SettingsText.Short(text)}」不是整数");
        }

        int clamped = Math.Clamp(value, AppSettings.MinLineHeight, AppSettings.MaxLineHeight);
        return clamped == value
            ? new SettingCoercion(text)
            : SettingCoercion.Fixed(
                clamped.ToString(CultureInfo.InvariantCulture),
                $"ListViewLineHeight={value} 超出 {AppSettings.MinLineHeight}–{AppSettings.MaxLineHeight}，已按 {clamped} 处理");
    }

    /// <summary>
    /// 路径类设置。读配置（strict=false）时只清空「永远不可能合法」的值，其余（相对路径、盘符未挂载）
    /// 原样保留——因为读盘时清空等于用户下一次保存就把配置吃掉；导入（strict=true）时逐条判非法。
    /// </summary>
    public static Func<string, bool, SettingCoercion> PathValue(string fieldName, bool allowDirectory)
        => (raw, strict) =>
        {
            string text = raw.Trim();
            if (text.Length == 0)
            {
                return new SettingCoercion(string.Empty);
            }

            if (HasTrapChar(text))
            {
                string reason = $"{fieldName} 的「{SettingsText.Short(text)}」含非法字符（\"<>| 等）";
                return strict
                    ? SettingCoercion.Reject(reason)
                    : SettingCoercion.Fixed(string.Empty, reason + "，已清空");
            }

            if (!SettingsText.TryGetFullPath(text, out _))
            {
                string reason = $"{fieldName} 的「{SettingsText.Short(text)}」无法解析为有效路径";
                return strict
                    ? SettingCoercion.Reject(reason)
                    : SettingCoercion.Fixed(string.Empty, reason + "，已清空");
            }

            if (!Path.IsPathFullyQualified(text))
            {
                string reason = $"{fieldName} 的「{SettingsText.Short(text)}」不是绝对路径，依赖当前工作目录";
                return strict
                    ? SettingCoercion.Reject(reason + "，请改用绝对路径")
                    : SettingCoercion.Fixed(text, reason + "，已保留原值但可能不可用");
            }

            if (!allowDirectory && text.EndsWith(Path.DirectorySeparatorChar))
            {
                return strict
                    ? SettingCoercion.Reject($"{fieldName} 需要文件路径，但「{SettingsText.Short(text)}」是目录")
                    : SettingCoercion.Fixed(text, $"{fieldName} 需要文件路径，但配置值是目录，已保留原值");
            }

            return new SettingCoercion(text);
        };

    /// <summary>隐藏功能 Id 列表：分隔符统一为逗号，去空白、去重（大小写不敏感）。</summary>
    public static SettingCoercion FeatureIdList(string raw, bool strict)
    {
        string[] parts = raw.Split(
            [',', ';', ' ', '\t', '\r', '\n'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var kept = new List<string>(parts.Length);
        foreach (string part in parts)
        {
            if (seen.Add(part))
            {
                kept.Add(part);
            }
        }

        string joined = string.Join(",", kept);
        return string.Equals(joined, raw, StringComparison.Ordinal)
            ? new SettingCoercion(joined)
            : SettingCoercion.Fixed(joined, $"HideFeatureIds 已规范化（去空白/去重）：「{SettingsText.Short(raw)}」→「{joined}」");
    }

    private static bool HasTrapChar(string value)
    {
        // 长路径前缀 \\?\ 里的 ? 是合法语法，先剥掉再查
        string probe = value.StartsWith(@"\\?\", StringComparison.Ordinal) ? value[4..] : value;
        return probe.IndexOfAny(PathTraps) >= 0;
    }

    private static bool IsTrue(string text)
        => text.Equals("true", StringComparison.OrdinalIgnoreCase)
           || text.Equals("on", StringComparison.OrdinalIgnoreCase)
           || text.Equals("1", StringComparison.Ordinal)
           || text.Equals("yes", StringComparison.OrdinalIgnoreCase);

    private static bool IsFalse(string text)
        => text.Equals("false", StringComparison.OrdinalIgnoreCase)
           || text.Equals("off", StringComparison.OrdinalIgnoreCase)
           || text.Equals("0", StringComparison.Ordinal)
           || text.Equals("no", StringComparison.OrdinalIgnoreCase);
}

/// <summary>文本/路径/数值的小工具，集中放在这里避免各字段重复 try/catch。</summary>
internal static class SettingsText
{
    public static string Bool(bool value) => value ? "true" : "false";

    public static bool ParseBool(string text, bool fallback)
        => bool.TryParse(text, out bool parsed) ? parsed : fallback;

    public static int ParseInt(string text, int fallback)
        => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed) ? parsed : fallback;

    public static bool TryGetFullPath(string text, out string fullPath)
    {
        try
        {
            fullPath = Path.GetFullPath(text);
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException or IOException)
        {
            fullPath = string.Empty;
            return false;
        }
    }

    /// <summary>提示语里的值截断，避免把整段配置粘进警告。</summary>
    public static string Short(string text, int max = 40)
        => text.Length <= max ? text : text[..max] + "…";
}
