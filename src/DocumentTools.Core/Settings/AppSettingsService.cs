using System.Text;

namespace DocumentTools.Core.Settings;

/// <summary>设置读写门面：App 层只依赖这个接口。</summary>
public interface IAppSettings
{
    /// <summary>
    /// 成功保存、重新加载或导入后触发。调用方可按 <see cref="SettingsChangedEventArgs.Fields"/>
    /// 定向刷新界面；失败的持久化不会触发，避免界面表现成已经应用。
    /// </summary>
    event EventHandler<SettingsChangedEventArgs>? Changed;

    /// <summary>当前生效的设置实例（就地修改后调用 <see cref="Save"/> 即持久化）。</summary>
    AppSettings Current { get; }

    /// <summary>最近一次加载时修正/降级了哪些值（空列表表示原样载入）。</summary>
    IReadOnlyList<string> LastLoadWarnings { get; }

    /// <summary>重新从存储读取；坏值修正与失败原因都在返回值里。</summary>
    SettingsLoadResult Reload();

    /// <summary>保存当前设置；权限/只读等失败以结果返回，不抛异常。</summary>
    SettingsSaveResult Save();

    /// <summary>体检当前设置（目录是否存在、路径是否合法），不抛异常。</summary>
    SettingsValidationReport Validate();

    /// <summary>导出为独立 JSON 文件（字段顺序稳定、格式化、中文不转义）。</summary>
    SettingsExportResult ExportTo(string jsonFile);

    /// <summary>从 JSON 文件导入：全部字段校验通过才写入，逐字段返回结果。</summary>
    SettingsImportResult ImportFrom(string jsonFile);

    /// <summary>从 JSON 文本导入（供文件之外的来源复用，语义同 <see cref="ImportFrom"/>）。</summary>
    SettingsImportResult ImportJson(string json);
}

/// <summary>一次已经生效的设置变更；字段名与 settings.json / <c>AppSettingsSchema</c> 一致。</summary>
public sealed class SettingsChangedEventArgs : EventArgs
{
    private readonly HashSet<string> _fields;

    public SettingsChangedEventArgs(IEnumerable<string> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        _fields = new HashSet<string>(fields, StringComparer.OrdinalIgnoreCase);
        Fields = _fields.OrderBy(static field => field, StringComparer.Ordinal).ToArray();
    }

    public IReadOnlyList<string> Fields { get; }

    public bool Contains(string field) => _fields.Contains(field);
}

/// <summary>保存结果。失败时 <see cref="Message"/> 可直接展示给用户。</summary>
public sealed record SettingsSaveResult(bool Succeeded, string Message)
{
    public static SettingsSaveResult Success(string message) => new(true, message);

    public static SettingsSaveResult Failure(string message) => new(false, message);
}

/// <summary>
/// 加载结果。<see cref="Succeeded"/> 为 false 表示没读到有效配置（已回退 legacy/默认值）；
/// 为 true 但 <see cref="Warnings"/> 非空表示读到并修正了坏值。
/// </summary>
public sealed record SettingsLoadResult(bool Succeeded, IReadOnlyList<string> Warnings, string Message)
{
    public bool HasWarnings => Warnings.Count > 0;
}

/// <summary>导出结果。</summary>
public sealed record SettingsExportResult(bool Succeeded, string Message, string FilePath)
{
    public static SettingsExportResult Failure(string message, string filePath = "") => new(false, message, filePath);
}

/// <summary>导入时单字段的处置方式。</summary>
public enum SettingsImportFieldOutcome
{
    /// <summary>校验通过并采用。</summary>
    Applied,

    /// <summary>文件里没有该字段，采用默认值。</summary>
    Defaulted,

    /// <summary>值非法，整个导入被取消。</summary>
    Rejected,

    /// <summary>未知字段，忽略（不算失败）。</summary>
    Ignored
}

/// <summary>导入的单字段结果。</summary>
public sealed record SettingsFieldResult(string Field, SettingsImportFieldOutcome Outcome, string Message)
{
    public bool IsProblem => Outcome == SettingsImportFieldOutcome.Rejected;
}

/// <summary>
/// 导入结果：全有或全无——只要有字段 <see cref="SettingsImportFieldOutcome.Rejected"/>，
/// 当前设置保持不变，<see cref="Fields"/> 会逐项说明原因。
/// </summary>
public sealed record SettingsImportResult(bool Succeeded, IReadOnlyList<SettingsFieldResult> Fields, string Message)
{
    public IReadOnlyList<SettingsFieldResult> RejectedFields
        => Fields.Where(f => f.Outcome == SettingsImportFieldOutcome.Rejected).ToArray();

    public IReadOnlyList<SettingsFieldResult> AppliedFields
        => Fields.Where(f => f.Outcome == SettingsImportFieldOutcome.Applied).ToArray();

    public static SettingsImportResult Failure(string message)
        => new(false, [], message);
}

/// <summary>
/// 设置服务：加载（坏值兜底 + 警告）、保存（失败不抛）、校验、导出/导入。
/// 存储后端通过 <see cref="ISettingsStore"/> 注入，单测可换成内存实现而不碰真实文件。
/// </summary>
public sealed class AppSettingsService : IAppSettings
{
    private readonly ISettingsStore _store;
    private readonly AppSettings? _firstRunSeed;
    private AppSettings _current = new();
    private AppSettings _applied = new();
    private SettingsLoadResult _lastLoad = new(true, [], string.Empty);

    public event EventHandler<SettingsChangedEventArgs>? Changed;

    /// <summary>使用默认存储（%LOCALAPPDATA%\DocumentTools\settings.json），首次运行从 legacy App.config 播种。</summary>
    public AppSettingsService()
        : this(new JsonFileSettingsStore(), firstRunSeed: null)
    {
    }

    /// <param name="store">存储后端。</param>
    /// <param name="firstRunSeed">
    /// 存储不存在/损坏时使用的初始值；null 表示从 legacy App.config 播种（迁移前行为）。
    /// 传入 <c>new AppSettings()</c> 可让加载完全走内置默认值（测试用）。
    /// </param>
    public AppSettingsService(ISettingsStore store, AppSettings? firstRunSeed = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _firstRunSeed = firstRunSeed;
        Reload();
    }

    public AppSettings Current => _current;

    public IReadOnlyList<string> LastLoadWarnings => _lastLoad.Warnings;

    public SettingsLoadResult Reload()
    {
        // Current 可能含有尚未保存的窗口内编辑；Reload 的语义是撤销这些值并回到存储内容，
        // 所以变更通知要与“重载前实际可见值”比较，而不是只与上次持久化快照比较。
        AppSettings previous = Clone(_current);
        var warnings = new List<string>();

        if (_store.TryRead(out string content, out string readError))
        {
            SettingsReadResult read = AppSettingsJson.Read(content);
            if (read.Succeeded)
            {
                _current = read.Settings;
                warnings.AddRange(read.Warnings);
                _lastLoad = warnings.Count == 0
                    ? new SettingsLoadResult(true, warnings, $"已载入设置：{_store.Location}")
                    : new SettingsLoadResult(
                        true,
                        warnings,
                        $"已载入设置并修正 {warnings.Count} 处：{_store.Location}（修正值已生效，保存后写回）");
                PublishChanges(previous);
                return _lastLoad;
            }

            warnings.Add($"{read.Error}；已改用初始值");
        }
        else
        {
            warnings.Add(readError);
        }

        _current = BuildSeed(warnings);
        _lastLoad = new SettingsLoadResult(false, warnings, string.Join("；", warnings));
        PublishChanges(previous);
        return _lastLoad;
    }

    public SettingsSaveResult Save()
    {
        string json = AppSettingsJson.Write(_current);
        if (!_store.TryWrite(json, out string error))
        {
            return SettingsSaveResult.Failure(error);
        }

        PublishChanges(_applied);
        return SettingsSaveResult.Success($"设置已保存：{_store.Location}");
    }

    public SettingsValidationReport Validate() => AppSettingsValidator.Validate(_current);

    public SettingsExportResult ExportTo(string jsonFile)
    {
        if (string.IsNullOrWhiteSpace(jsonFile))
        {
            return SettingsExportResult.Failure("导出路径不能为空。");
        }

        string fullPath = jsonFile;
        try
        {
            fullPath = Path.GetFullPath(jsonFile);
            string? directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 导出文件固定 UTF-8 无 BOM：便于 git diff 与人工编辑
            File.WriteAllText(fullPath, AppSettingsJson.Write(_current), new UTF8Encoding(false));
            return new SettingsExportResult(
                true,
                $"已导出 {AppSettingsSchema.Fields.Count} 项设置：{fullPath}",
                fullPath);
        }
        catch (Exception ex) when (JsonFileSettingsStore.IsIoFailure(ex))
        {
            return SettingsExportResult.Failure($"导出失败（{fullPath}）：{ex.Message}", fullPath);
        }
    }

    public SettingsImportResult ImportFrom(string jsonFile)
    {
        if (string.IsNullOrWhiteSpace(jsonFile))
        {
            return SettingsImportResult.Failure("导入文件路径不能为空。");
        }

        string fullPath;
        string content;
        try
        {
            fullPath = Path.GetFullPath(jsonFile);
            if (!File.Exists(fullPath))
            {
                return SettingsImportResult.Failure($"导入文件不存在：{fullPath}");
            }

            content = File.ReadAllText(fullPath);
        }
        catch (Exception ex) when (JsonFileSettingsStore.IsIoFailure(ex))
        {
            return SettingsImportResult.Failure($"读取导入文件失败（{jsonFile}）：{ex.Message}");
        }

        SettingsImportResult result = ImportJson(content);
        return result with { Message = $"{result.Message}（来源：{fullPath}）" };
    }

    public SettingsImportResult ImportJson(string json)
    {
        if (!AppSettingsJson.TryExtract(
                json,
                out IReadOnlyList<SettingsRawValue> values,
                out IReadOnlyList<string> unknown,
                out string error))
        {
            return SettingsImportResult.Failure($"导入失败：{error}");
        }

        var byName = values.ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);
        var candidate = new AppSettings();
        var fields = new List<SettingsFieldResult>(AppSettingsSchema.Fields.Count + unknown.Count);
        int rejected = 0;

        foreach (SettingField field in AppSettingsSchema.Fields)
        {
            if (!byName.TryGetValue(field.Name, out SettingsRawValue raw))
            {
                fields.Add(new SettingsFieldResult(
                    field.Name,
                    SettingsImportFieldOutcome.Defaulted,
                    $"文件中缺少该字段，使用默认值 {field.Get(candidate)}"));
                continue;
            }

            if (!raw.IsScalar)
            {
                rejected++;
                fields.Add(new SettingsFieldResult(
                    field.Name,
                    SettingsImportFieldOutcome.Rejected,
                    "类型不符：应为字符串/数字/布尔值，实际是对象或数组"));
                continue;
            }

            SettingCoercion coercion = field.Coerce(raw.Text, true);
            if (coercion.Rejected)
            {
                rejected++;
                fields.Add(new SettingsFieldResult(field.Name, SettingsImportFieldOutcome.Rejected, coercion.RejectReason!));
                continue;
            }

            field.Set(candidate, coercion.Text);
            fields.Add(new SettingsFieldResult(
                field.Name,
                SettingsImportFieldOutcome.Applied,
                coercion.Warning ?? $"已应用：{coercion.Text}"));
        }

        foreach (string name in unknown)
        {
            fields.Add(new SettingsFieldResult(
                name,
                SettingsImportFieldOutcome.Ignored,
                "未知字段，已忽略（不影响导入结果）"));
        }

        if (rejected > 0)
        {
            return new SettingsImportResult(
                false,
                fields,
                $"导入已取消：{rejected} 个字段非法，当前设置未改变");
        }

        if (!_store.TryWrite(AppSettingsJson.Write(candidate), out string writeError))
        {
            return new SettingsImportResult(false, fields, $"校验通过，但{writeError}；当前设置未改变");
        }

        AppSettings previous = _applied;
        _current = candidate;
        _lastLoad = new SettingsLoadResult(true, [], $"已导入设置：{_store.Location}");
        PublishChanges(previous);

        int applied = fields.Count(f => f.Outcome == SettingsImportFieldOutcome.Applied);
        return new SettingsImportResult(
            true,
            fields,
            $"已导入 {applied} 项设置（{fields.Count(f => f.Outcome == SettingsImportFieldOutcome.Defaulted)} 项取默认值）：{_store.Location}");
    }

    private AppSettings BuildSeed(List<string> warnings)
    {
        if (_firstRunSeed is not null)
        {
            return Clone(_firstRunSeed);
        }

        AppSettings seeded = AppSettings.FromLegacyConfig(out bool found);
        if (found)
        {
            warnings.Add("已从 legacy App.config 播种初始值");
        }

        return seeded;
    }

    /// <summary>复制一份，避免调用方传入的 seed 实例与 <see cref="Current"/> 共享引用被后续改动污染。</summary>
    private static AppSettings Clone(AppSettings source)
    {
        var clone = new AppSettings();
        foreach (SettingField field in AppSettingsSchema.Fields)
        {
            field.Set(clone, field.Get(source));
        }

        return clone;
    }

    private void PublishChanges(AppSettings previous)
    {
        string[] changed = AppSettingsSchema.Fields
            .Where(field => !string.Equals(field.Get(previous), field.Get(_current), StringComparison.Ordinal))
            .Select(field => field.Name)
            .ToArray();

        // 即使没有订阅者，也要推进快照；下一次 Save 只报告这一轮真正改变的字段。
        _applied = Clone(_current);
        if (changed.Length > 0)
        {
            Changed?.Invoke(this, new SettingsChangedEventArgs(changed));
        }
    }
}
