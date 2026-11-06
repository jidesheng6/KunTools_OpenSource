namespace DocumentTools.Core.Features.Options;

/// <summary>UI-agnostic option field. Views render this; Operations consume Keys.</summary>
public enum OptionFieldKind
{
    Check,
    Text,
    Choice,

    /// <summary>文件路径：视图额外给一个「选择文件…」按钮，Filter 用于对话框。</summary>
    FilePath
}

/// <summary>Choice item: <see cref="Value"/> goes into Options dictionary; <see cref="Label"/> is display-only.</summary>
public sealed record OptionChoice(string Value, string Label);

public sealed record OptionField(
    string Key,
    string Label,
    OptionFieldKind Kind,
    string? DefaultValue = null,
    IReadOnlyList<OptionChoice>? Choices = null,
    double TextWidth = 160,
    string? FileFilter = null);

public sealed record OptionSection(string Title, IReadOnlyList<OptionField> Fields);
