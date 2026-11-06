using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace DocumentTools.Core.Settings;

/// <summary>从 JSON 里取出的一个字段原文。<see cref="IsScalar"/> 为 false 表示是对象/数组（类型不符）。</summary>
internal readonly record struct SettingsRawValue(string Name, string Text, bool IsScalar);

/// <summary>读取结果：设置本体 + 修正记录 + 失败原因（失败时 <see cref="Error"/> 非空）。</summary>
internal sealed record SettingsReadResult(AppSettings Settings, IReadOnlyList<string> Warnings, string? Error)
{
    public bool Succeeded => Error is null;
}

/// <summary>
/// 设置 JSON 读写。字段顺序由 <see cref="AppSettingsSchema.Fields"/> 决定（稳定，便于 diff），
/// 读取时按字段名大小写不敏感匹配，未知字段忽略并记警告，缺失字段回退默认值。
/// </summary>
internal static class AppSettingsJson
{
    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        AllowTrailingCommas = true,
        CommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>序列化为格式化 JSON（字段顺序稳定，中文路径不转义，末尾带换行）。</summary>
    internal static string Write(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(
                   stream,
                   new JsonWriterOptions
                   {
                       Indented = true,
                       Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                   }))
        {
            writer.WriteStartObject();
            foreach (SettingField field in AppSettingsSchema.Fields)
            {
                string text = field.Get(settings) ?? string.Empty;
                switch (field.Kind)
                {
                    case SettingValueKind.Boolean:
                        writer.WriteBoolean(field.Name, SettingsText.ParseBool(text, fallback: false));
                        break;
                    case SettingValueKind.Integer:
                        writer.WriteNumber(field.Name, SettingsText.ParseInt(text, fallback: 0));
                        break;
                    default:
                        writer.WriteString(field.Name, text);
                        break;
                }
            }

            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray()) + Environment.NewLine;
    }

    /// <summary>读取 JSON（宽松模式：坏值修正为默认值并记警告，绝不抛异常）。</summary>
    internal static SettingsReadResult Read(string json)
    {
        var settings = new AppSettings();
        var warnings = new List<string>();

        if (!TryExtract(json, out IReadOnlyList<SettingsRawValue> values, out IReadOnlyList<string> unknown, out string error))
        {
            return new SettingsReadResult(settings, warnings, error);
        }

        if (unknown.Count > 0)
        {
            warnings.Add($"忽略未知字段：{string.Join(", ", unknown)}");
        }

        var byName = values.ToDictionary(v => v.Name, StringComparer.OrdinalIgnoreCase);
        foreach (SettingField field in AppSettingsSchema.Fields)
        {
            if (!byName.TryGetValue(field.Name, out SettingsRawValue match))
            {
                continue; // 缺字段：保持构造函数的默认值，不写警告（新增字段的旧文件属于正常情况）
            }

            if (!match.IsScalar)
            {
                RestoreDefault(field, settings);
                warnings.Add($"{field.Name} 的值不是标量（对象/数组），已用默认值 {field.Get(settings)}");
                continue;
            }

            SettingCoercion coercion = field.Coerce(match.Text, false);
            if (coercion.Rejected)
            {
                RestoreDefault(field, settings);
                warnings.Add($"{field.Name}：{coercion.RejectReason}，已用默认值 {field.Get(settings)}");
                continue;
            }

            field.Set(settings, coercion.Text);
            if (coercion.Warning is not null)
            {
                warnings.Add(coercion.Warning);
            }
        }

        return new SettingsReadResult(settings, warnings, null);
    }

    /// <summary>
    /// 把 JSON 对象拆成「字段名 → 原文」。字段按 schema 顺序返回，同名重复键以最后一个为准；
    /// 结构错误（非对象根、语法错误）返回 false 并给出原因。
    /// </summary>
    internal static bool TryExtract(
        string json,
        out IReadOnlyList<SettingsRawValue> values,
        out IReadOnlyList<string> unknown,
        out string error)
    {
        values = [];
        unknown = [];
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = "JSON 内容为空";
            return false;
        }

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json, DocumentOptions);
        }
        catch (JsonException ex)
        {
            error = $"JSON 语法错误：{ex.Message}";
            return false;
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                error = $"JSON 根节点必须是对象，实际是 {Describe(document.RootElement.ValueKind)}";
                return false;
            }

            var present = new Dictionary<string, SettingsRawValue>(StringComparer.OrdinalIgnoreCase);
            var unknownNames = new List<string>();
            foreach (JsonProperty property in document.RootElement.EnumerateObject())
            {
                if (AppSettingsSchema.Find(property.Name) is null)
                {
                    if (!unknownNames.Contains(property.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        unknownNames.Add(property.Name);
                    }

                    continue;
                }

                bool scalar = TryReadText(property.Value, out string text);
                present[property.Name] = new SettingsRawValue(property.Name, text, scalar);
            }

            var ordered = new List<SettingsRawValue>(present.Count);
            foreach (SettingField field in AppSettingsSchema.Fields)
            {
                if (present.TryGetValue(field.Name, out SettingsRawValue value))
                {
                    ordered.Add(value);
                }
            }

            values = ordered;
            unknown = unknownNames;
            return true;
        }
    }

    private static bool TryReadText(JsonElement element, out string text)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                text = element.GetString() ?? string.Empty;
                return true;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                text = element.GetRawText();
                return true;
            case JsonValueKind.Null:
                text = string.Empty;
                return true;
            default:
                text = element.GetRawText();
                return false;
        }
    }

    private static void RestoreDefault(SettingField field, AppSettings target)
        => field.Set(target, field.Get(AppSettingsSchema.Defaults));

    private static string Describe(JsonValueKind kind) => kind switch
    {
        JsonValueKind.Array => "数组",
        JsonValueKind.Number => "数字",
        JsonValueKind.String => "字符串",
        JsonValueKind.True or JsonValueKind.False => "布尔值",
        JsonValueKind.Null => "null",
        JsonValueKind.Undefined => "未定义",
        _ => "对象"
    };
}
