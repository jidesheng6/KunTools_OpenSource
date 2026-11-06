using DocumentTools.Core.Services.Office;

namespace DocumentTools.Core.Operations;

/// <summary>Maps word.replace / excel.replace.</summary>
public sealed class ReplaceDocumentOperation : IDocumentOperation
{
    private readonly IDocumentReplaceService _replace;

    public ReplaceDocumentOperation(string featureId, IDocumentReplaceService replace)
    {
        FeatureId = featureId;
        _replace = replace;
    }

    public string FeatureId { get; }

    public Task<DocumentOperationResult> ExecuteAsync(
        DocumentOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.InputFiles.Count == 0)
        {
            return Task.FromResult(DocumentOperationResult.Fail("请先选择输入文件。"));
        }

        var pairs = ParsePairs(request);
        if (pairs.Count == 0)
        {
            return Task.FromResult(DocumentOperationResult.Fail("请至少填写一组查找/替换内容。"));
        }

        var options = new DocumentReplaceOptions
        {
            Pairs = pairs,
            MatchCase = IsTrue(request, "matchCase"),
            FindWholeWord = IsTrue(request, "wholeWord"),
            UseRegex = IsTrue(request, "regex"),
            IgnoreFootnotes = IsTrue(request, "ignoreFootnotes"),
            IgnoreDeleted = IsTrue(request, "ignoreDeleted"),
            IgnoreFieldCodes = IsTrue(request, "ignoreFieldCodes"),
            IgnoreFields = IsTrue(request, "ignoreFields"),
            IgnoreInserted = IsTrue(request, "ignoreInserted"),
            IgnoreShapes = IsTrue(request, "ignoreShapes"),
            // legacy Util.cs:5461 只认 UseRegexSub；这里不能把「使用正则」当成「使用 $1 捕获替换」的开关，
            // 否则用户勾「使用正则」就再也关不掉捕获替换（Finding：task-25 审计）。
            UseRegexSubstitutions = IsTrue(request, "regexSub"),
            ExcelMatchEntireCell = IsTrue(request, "wholeWord"),
            ExcelRemoveRows = IsTrue(request, "excelRemoveRows"),
            ExcelStartRow = ParseInt(request, "excelStartRow", -1),
            ExcelEndRow = ParseInt(request, "excelEndRow", -1),
            // legacy 的「更改替换的字体和大小」（Util.cs:5373 UseNewStyle，UI 勾选框见 BodyForm.Designer.cs:7732）。
            UseNewStyle = IsTrue(request, "replaceUseNewStyle"),
            FontName = GetOption(request, "replaceFontName")?.Trim() ?? string.Empty,
            FontSize = ParseDouble(request, "replaceFontSize", 0),
            Bold = ParseTriState(request, "replaceBold")
        };

        try
        {
            Directory.CreateDirectory(request.OutputDirectory);
            var outputs = new List<string>();
            bool excel = FeatureId == "excel.replace";

            foreach (string input in request.InputFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string ext = excel ? ".xlsx" : ".docx";
                string output = Path.Combine(
                    request.OutputDirectory,
                    Path.GetFileNameWithoutExtension(input) + "_替换" + ext);

                if (excel)
                {
                    _replace.ReplaceExcel(input, output, options);
                }
                else
                {
                    _replace.ReplaceWord(input, output, options);
                }

                outputs.Add(output);
            }

            return Task.FromResult(
                DocumentOperationResult.Ok($"替换完成 {outputs.Count} 个文件。", outputs));
        }
        catch (Exception ex)
        {
            return Task.FromResult(DocumentOperationResult.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Pairs encoded as options["pairCount"]=N and pair0.find / pair0.replace …
    /// or a single JSON-like "pairs" = "a=>b||c=>d".
    /// </summary>
    private static List<(string Find, string Replace)> ParsePairs(DocumentOperationRequest request)
    {
        var list = new List<(string, string)>();
        if (request.Options is null)
        {
            return list;
        }

        if (request.Options.TryGetValue("pairCount", out string? countText)
            && int.TryParse(countText, out int count))
        {
            for (int i = 0; i < count; i++)
            {
                request.Options.TryGetValue($"pair{i}.find", out string? find);
                request.Options.TryGetValue($"pair{i}.replace", out string? replace);
                if (!string.IsNullOrEmpty(find))
                {
                    list.Add((find, replace ?? string.Empty));
                }
            }

            return list;
        }

        if (request.Options.TryGetValue("pairs", out string? packed) && !string.IsNullOrWhiteSpace(packed))
        {
            foreach (string chunk in packed.Split("||", StringSplitOptions.RemoveEmptyEntries))
            {
                int sep = chunk.IndexOf("=>", StringComparison.Ordinal);
                if (sep <= 0)
                {
                    continue;
                }

                list.Add((chunk[..sep], chunk[(sep + 2)..]));
            }
        }

        return list;
    }

    private static bool IsTrue(DocumentOperationRequest request, string key)
    {
        if (request.Options is null || !request.Options.TryGetValue(key, out string? value))
        {
            return false;
        }

        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
               || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase);
    }

    private static int ParseInt(DocumentOperationRequest request, string key, int fallback)
    {
        if (request.Options is null || !request.Options.TryGetValue(key, out string? value))
        {
            return fallback;
        }

        return int.TryParse(value, out int n) ? n : fallback;
    }

    private static double ParseDouble(DocumentOperationRequest request, string key, double fallback)
    {
        if (request.Options is null || !request.Options.TryGetValue(key, out string? value))
        {
            return fallback;
        }

        // 中文界面里用户可能填「18 磅」或「18pt」，只取前缀数字，取不到就回退。
        string trimmed = (value ?? string.Empty).Trim();
        int end = 0;
        while (end < trimmed.Length && (char.IsDigit(trimmed[end]) || trimmed[end] == '.'))
        {
            end++;
        }

        return end > 0 && double.TryParse(trimmed[..end], out double d) ? d : fallback;
    }

    /// <summary>三态：keep(默认) = 不改，bold/normal 显式设定；无法识别的值按「不改」。</summary>
    private static bool? ParseTriState(DocumentOperationRequest request, string key)
    {
        string? value = GetOption(request, key);
        return value?.Trim().ToLowerInvariant() switch
        {
            "bold" or "true" or "加粗" => true,
            "normal" or "false" or "不加粗" => false,
            _ => null
        };
    }

    private static string? GetOption(DocumentOperationRequest request, string key)
        => request.Options is not null && request.Options.TryGetValue(key, out string? value) ? value : null;
}
