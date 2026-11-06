namespace DocumentTools.Core.Settings;

/// <summary>单个设置项的体检结论。</summary>
public enum SettingsValueState
{
    /// <summary>值合法且可直接使用（目录/文件型设置意味着目标存在）。</summary>
    Ok,

    /// <summary>值为空：不算错误，走默认行为。</summary>
    NotSet,

    /// <summary>值合法但目标暂不存在（例如 U 盘未插、软件尚未安装）。</summary>
    Missing,

    /// <summary>值本身非法（非法字符、非绝对路径、类型不符、目录与文件混淆）。</summary>
    Invalid
}

/// <summary>一个设置项的体检结果：机器可读的 <see cref="State"/> + 给人看的 <see cref="Text"/>。</summary>
public sealed record SettingsFieldStatus(string Field, string Value, SettingsValueState State, string Text)
{
    public bool IsOk => State == SettingsValueState.Ok;
}

/// <summary>整份设置的体检报告。不抛异常，任何异常情况都折算成具体字段的坏状态。</summary>
public sealed class SettingsValidationReport
{
    internal SettingsValidationReport(IReadOnlyList<SettingsFieldStatus> fields)
    {
        Fields = fields;
        Problems = fields.Where(f => !f.IsOk).ToArray();

        int ok = fields.Count(f => f.State == SettingsValueState.Ok);
        int notSet = fields.Count(f => f.State == SettingsValueState.NotSet);
        int missing = fields.Count(f => f.State == SettingsValueState.Missing);
        int invalid = fields.Count(f => f.State == SettingsValueState.Invalid);
        Summary = $"{fields.Count} 项设置：{ok} 项正常，{notSet} 项未设置，{missing} 项路径不可用，{invalid} 项非法";
    }

    public IReadOnlyList<SettingsFieldStatus> Fields { get; }

    /// <summary>非 Ok 的项（未设置 / 不存在 / 非法），用于界面逐行提示。</summary>
    public IReadOnlyList<SettingsFieldStatus> Problems { get; }

    /// <summary>全部字段都 Ok（未设置的字段也算需要关注）。</summary>
    public bool IsOk => Problems.Count == 0;

    public string Summary { get; }
}

/// <summary>
/// 设置校验：目录型设置判断「存在且是目录」，文件型设置判断「存在且是文件」。
/// 全部返回状态对象，不抛异常——设置页刷新时不该因为一个坏路径崩掉。
/// </summary>
public static class AppSettingsValidator
{
    /// <summary>指向目录的设置项名（与 <see cref="AppSettings"/> 属性同名，便于界面直接对照）。</summary>
    public static readonly IReadOnlyList<string> DirectoryFields = ["VfsoftExamDataPath", "VfsoftMssDataPath"];

    /// <summary>指向文件的设置项名。</summary>
    public static readonly IReadOnlyList<string> FileFields = ["ChromeDriverPath"];

    /// <summary>体检整份设置（字段顺序与导出 JSON 一致）。</summary>
    public static SettingsValidationReport Validate(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var statuses = new List<SettingsFieldStatus>(AppSettingsSchema.Fields.Count);
        foreach (SettingField field in AppSettingsSchema.Fields)
        {
            statuses.Add(Describe(field, field.Get(settings) ?? string.Empty));
        }

        return new SettingsValidationReport(statuses);
    }

    /// <summary>目录型设置：存在且是目录才算 Ok。</summary>
    public static SettingsFieldStatus CheckDirectory(string fieldName, string? path)
    {
        string value = (path ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return new SettingsFieldStatus(fieldName, value, SettingsValueState.NotSet, "未设置");
        }

        if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return new SettingsFieldStatus(fieldName, value, SettingsValueState.Invalid, InvalidChars(fieldName));
        }

        if (!Path.IsPathFullyQualified(value))
        {
            return new SettingsFieldStatus(
                fieldName,
                value,
                SettingsValueState.Invalid,
                $"{fieldName} 不是绝对路径：「{SettingsText.Short(value)}」");
        }

        if (Directory.Exists(value))
        {
            return new SettingsFieldStatus(fieldName, value, SettingsValueState.Ok, "目录已就绪");
        }

        return File.Exists(value)
            ? new SettingsFieldStatus(fieldName, value, SettingsValueState.Invalid, $"{fieldName} 指向的是文件，不是目录")
            : new SettingsFieldStatus(fieldName, value, SettingsValueState.Missing, $"目录不存在：{SettingsText.Short(value)}");
    }

    /// <summary>文件型设置：存在且是文件才算 Ok。</summary>
    public static SettingsFieldStatus CheckFile(string fieldName, string? path)
    {
        string value = (path ?? string.Empty).Trim();
        if (value.Length == 0)
        {
            return new SettingsFieldStatus(fieldName, value, SettingsValueState.NotSet, "未设置");
        }

        if (value.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        {
            return new SettingsFieldStatus(fieldName, value, SettingsValueState.Invalid, InvalidChars(fieldName));
        }

        if (!Path.IsPathFullyQualified(value))
        {
            return new SettingsFieldStatus(
                fieldName,
                value,
                SettingsValueState.Invalid,
                $"{fieldName} 不是绝对路径：「{SettingsText.Short(value)}」");
        }

        if (File.Exists(value))
        {
            return new SettingsFieldStatus(fieldName, value, SettingsValueState.Ok, "文件存在");
        }

        return Directory.Exists(value)
            ? new SettingsFieldStatus(fieldName, value, SettingsValueState.Invalid, $"{fieldName} 指向的是目录，不是文件")
            : new SettingsFieldStatus(fieldName, value, SettingsValueState.Missing, $"文件不存在：{SettingsText.Short(value)}");
    }

    /// <summary>目录型设置的快捷判断：非空、存在且确实是目录。</summary>
    public static bool IsDirectoryReady(string? path)
        => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);

    private static SettingsFieldStatus Describe(SettingField field, string value)
    {
        if (DirectoryFields.Contains(field.Name, StringComparer.Ordinal))
        {
            return CheckDirectory(field.Name, value);
        }

        if (FileFields.Contains(field.Name, StringComparer.Ordinal))
        {
            return CheckFile(field.Name, value);
        }

        if (value.Length == 0)
        {
            string hint = field.Name switch
            {
                "StartFeatureId" => "未设置，启动时进入默认首页",
                "HideFeatureIds" => "未隐藏任何功能",
                _ => "未设置"
            };
            return new SettingsFieldStatus(field.Name, value, SettingsValueState.NotSet, hint);
        }

        string text = field.Kind switch
        {
            SettingValueKind.Boolean => value == SettingsText.Bool(true) ? "已启用" : "已关闭",
            SettingValueKind.Integer => $"值为 {value}",
            _ => "已设置"
        };

        return new SettingsFieldStatus(field.Name, value, SettingsValueState.Ok, text);
    }

    private static string InvalidChars(string fieldName)
        => $"{fieldName} 含非法路径字符（\" < > | 或控制字符），请重新选择";
}
