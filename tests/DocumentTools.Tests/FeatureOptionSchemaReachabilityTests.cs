using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentTools.Core.Features.Options;
using Xunit;

namespace DocumentTools.Tests;

/// <summary>
/// Schema 死键护栏：<see cref="FeatureOptionSchemas"/> 声明的每个键，在 src\ 下必须至少有一处字面量消费点。
/// <para>
/// 起因：docs\qa-doc-consistency.md 的 S1 —— <c>voteGenDate</c> 只在 Schema 声明、全仓无人读取，
/// 界面按 Schema 渲染就会得到一个「填了不生效」的输入框，即"第二类假参数"。
/// </para>
/// <para>
/// 检索必须大小写不敏感：Schema 键是 camelCase（<c>excelRemoveRows</c>），而 DTO 属性与测试里的
/// 请求构造是 PascalCase（<c>ExcelRemoveRows</c>），只搜一种形态会得到假阴性（§1.1）。
/// </para>
/// </summary>
public sealed class FeatureOptionSchemaReachabilityTests
{
    /// <summary>Schema 里的键工厂调用：Text("key", …) / Choice("key", …) / Check("key", …) / FilePath("key", …)。</summary>
    private static readonly Regex KeyRegex = new(@"(?:Text|Choice|Check|FilePath)\(\s*""(?<key>[^""]+)""", RegexOptions.Compiled);

    [Fact]
    public void EveryDeclaredSchemaKey_HasAConsumerOutsideTheSchemaFile()
    {
        string root = FindRepositoryRoot();
        string schemaFile = Path.Combine(root, "src", "DocumentTools.Core", "Features", "Options", "FeatureOptionSchemas.cs");
        Assert.True(File.Exists(schemaFile), "找不到 Schema 文件：" + schemaFile);

        List<string> keys = KeyRegex.Matches(File.ReadAllText(schemaFile))
            .Select(match => match.Groups["key"].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToList();

        // 解析自身失效（例如工厂方法改名）时直接失败，而不是"零个键、零个孤儿"地悄悄通过。
        Assert.True(keys.Count >= 100, $"Schema 键解析疑似失效：只解析到 {keys.Count} 个键。");

        string separator = Path.DirectorySeparatorChar.ToString();
        List<string> sources = Directory
            .EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains(separator + "bin" + separator) &&
                           !path.Contains(separator + "obj" + separator) &&
                           !string.Equals(path, schemaFile, StringComparison.OrdinalIgnoreCase))
            .Select(File.ReadAllText)
            .ToList();

        List<string> orphans = keys.Where(key => !HasConsumerLiteral(sources, key)).ToList();

        Assert.True(
            orphans.Count == 0,
            "以下 Schema 键在 src 下没有任何消费点（声明了却没人读，界面上就是「填了不生效」）：" +
            string.Join("、", orphans) +
            "。修法：让界面或 DTO 真正读取该键，或在 FeatureOptionSchemas 里删掉它。");
    }

    [Fact]
    public void ExamVoteSurfaceKeys_AreTheThreeLayoutInputs()
    {
        List<OptionField> fields = FeatureOptionSchemas.GetSections("exam.vote")
            .SelectMany(section => section.Fields)
            .ToList();

        Assert.Equal(
            new[] { "voteFontSize", "voteRowHeight", "voteGenDate" },
            fields.Select(field => field.Key).ToArray());

        OptionField date = fields.Single(field => field.Key == "voteGenDate");
        Assert.Equal(OptionFieldKind.Text, date.Kind);

        // 标签必须写明"留空"语义：否则用户不知道空值会被当天日期替换（VoteReportService.ResolveGenerationDate）。
        Assert.Contains("留空", date.Label, StringComparison.Ordinal);
    }

    private static bool HasConsumerLiteral(IReadOnlyList<string> sources, string key)
    {
        string needle = "\"" + key + "\"";
        foreach (string source in sources)
        {
            if (source.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "DocumentTools.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("未能从 " + AppContext.BaseDirectory + " 向上找到 DocumentTools.sln。");
    }
}
