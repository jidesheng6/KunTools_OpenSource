using System.Globalization;
using System.Xml.Linq;

namespace DocumentTools.Core.Settings;

/// <summary>
/// 运行期设置模型（迁移自 legacy App.config；不含 FormWidth/FormHeight 与 WidgetAnchorMode）。
/// 本类型只描述数据与默认值：JSON 读写见 <see cref="AppSettingsJson"/>，状态校验见
/// <see cref="AppSettingsValidator"/>，加载/保存/导入导出见 <see cref="AppSettingsService"/>。
/// </summary>
public sealed class AppSettings
{
    /// <summary>当前配置文件格式版本。<see cref="SettingsVersion"/> 高于此值时按已知字段尽力读取。</summary>
    public const int CurrentVersion = 1;

    public const string DefaultStartFeatureId = "home";
    public const int DefaultLineHeight = 25;
    public const int MinLineHeight = 18;
    public const int MaxLineHeight = 64;

    /// <summary>配置文件格式版本；导入时若高于 <see cref="CurrentVersion"/> 收敛到当前版本并给出提示。</summary>
    public int SettingsVersion { get; set; } = CurrentVersion;

    public string StartFeatureId { get; set; } = DefaultStartFeatureId;
    public bool ShowOpenTargetDialog { get; set; } = true;
    public bool ListViewShowGridLines { get; set; } = true;
    public int ListViewLineHeight { get; set; } = DefaultLineHeight;
    public bool HighlightActiveNav { get; set; } = true;
    public string ChromeDriverPath { get; set; } = string.Empty;
    public string VfsoftExamDataPath { get; set; } = string.Empty;
    public string VfsoftMssDataPath { get; set; } = string.Empty;
    public string HideFeatureIds { get; set; } = string.Empty;

    public IReadOnlyList<string> GetHiddenFeatureIds()
        => HideFeatureIds
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    /// <summary>默认配置文件位置（%LOCALAPPDATA%\DocumentTools\settings.json）。</summary>
    public static string StorePath => JsonFileSettingsStore.DefaultLocation;

    /// <summary>按默认存储读取设置；坏值已在读取阶段修正，修正明细由 <see cref="AppSettingsService.LastLoadWarnings"/> 提供。</summary>
    public static AppSettings Load() => new AppSettingsService().Current;

    /// <summary>以默认存储保存当前设置；写入失败以结果返回，不抛异常。</summary>
    public SettingsSaveResult Save()
    {
        string json = AppSettingsJson.Write(this);
        var store = new JsonFileSettingsStore();
        return store.TryWrite(json, out string error)
            ? SettingsSaveResult.Success($"设置已保存：{store.Location}")
            : SettingsSaveResult.Failure(error);
    }

    /// <summary>从 legacy App.config 播种初始值（找不到配置文件时返回内置默认值）。</summary>
    public static AppSettings FromLegacyConfig() => FromLegacyConfig(out _);

    /// <summary>从 legacy App.config 播种初始值，并报告是否真的读到了配置文件。</summary>
    public static AppSettings FromLegacyConfig(out bool found)
    {
        var settings = new AppSettings();
        string[] candidates =
        [
            Path.Combine(AppContext.BaseDirectory, "DocumentTools.dll.config"),
            Path.Combine(AppContext.BaseDirectory, "App.config"),
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "DocumentTools", "App.config"))
        ];

        string? path = candidates.FirstOrDefault(File.Exists);
        found = path is not null;
        if (path is null)
        {
            return settings;
        }

        try
        {
            var doc = XDocument.Load(path);
            string? Get(string key)
                => doc.Descendants("add")
                    .FirstOrDefault(e => string.Equals((string?)e.Attribute("key"), key, StringComparison.OrdinalIgnoreCase))
                    ?.Attribute("value")
                    ?.Value;

            settings.ShowOpenTargetDialog = IsOn(Get("ShowOpenTargetDialog"), true);
            settings.ListViewShowGridLines = IsOn(Get("ListViewShowGridLines"), true);
            settings.HighlightActiveNav = IsOn(Get("TabPageHeighLight"), false);
            if (int.TryParse(Get("ListViewLineHeight"), NumberStyles.Integer, CultureInfo.InvariantCulture, out int height)
                && height >= MinLineHeight
                && height <= MaxLineHeight)
            {
                settings.ListViewLineHeight = height;
            }

            // 键名与 legacy App.config 保持一致，不改名
            settings.ChromeDriverPath = Get("ChromeDriverPath") ?? string.Empty;
            settings.VfsoftExamDataPath = Get("Vfsoft_ExamDataPath") ?? string.Empty;
            settings.VfsoftMssDataPath = Get("Vfsoft_MssDataPath") ?? string.Empty;
            settings.HideFeatureIds = Get("HidePageIndex") ?? string.Empty;
            // StartPageIndex was tab index; map 0 → home
            settings.StartFeatureId = DefaultStartFeatureId;
        }
        catch
        {
            // 配置损坏时保留内置默认值：启动不能因为一份坏 XML 失败
        }

        return settings;
    }

    private static bool IsOn(string? value, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value.Equals("on", StringComparison.OrdinalIgnoreCase)
               || value.Equals("true", StringComparison.OrdinalIgnoreCase)
               || value.Equals("1", StringComparison.OrdinalIgnoreCase);
    }
}
