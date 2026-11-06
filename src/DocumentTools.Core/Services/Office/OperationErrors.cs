namespace DocumentTools.Core.Services.Office;

/// <summary>
/// 把底层异常翻译成「做了什么：为什么失败。怎么修」的中文提示。
/// 触发场景见 docs/qa-report-compat.md 与 docs/feature-gap.md §9：
/// Aspose 评估模式、输出目录不可写、浏览器路径缺失这三类都会把英文原文直接甩给用户。
/// 已经带中文的消息（本仓库各 Service 自己抛的、可操作的）原样放行，避免二次包裹。
/// </summary>
public static class OperationErrors
{
    private const string EvaluationHint =
        "当前为 Aspose 评估模式，集合超过 4 个元素时会被限制。"
        + "本构建不随程序分发授权文件，请把文件拆分后再试。";

    /// <summary>Aspose 评估模式的典型文本（PDF 集合上限、Words 水印、Cells 警告表）。</summary>
    private static readonly string[] EvaluationMarkers =
    {
        "evaluation mode",
        "evaluation only",
        "evaluation warning",
        "created with aspose",
        "at most "
    };

    public static bool IsEvaluationLimit(Exception? exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            string message = current.Message ?? string.Empty;

            foreach (string marker in EvaluationMarkers)
            {
                if (!message.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // "At most N elements" 才是评估限制；其它 "at most" 措辞不算。
                if (marker == "at most "
                    && !message.Contains("element", StringComparison.OrdinalIgnoreCase)
                    && !message.Contains("collection", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return true;
            }

            if (current.GetType().Name.Contains("Evaluation", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 把 <paramref name="exception"/> 变成可直接展示的异常；无法识别的英文原文也会补上「怎么办」。
    /// 已经是中文可操作消息的异常原样返回；中文但**没给修复动作**的（如「无法写入输出文件（被占用或权限不足）：a.png」）
    /// 会补上 <paramref name="fixHint"/>，不再二次包裹原因。
    /// </summary>
    public static Exception Translate(
        Exception exception,
        string action,
        string? outputDirectory = null,
        string? fixHint = null)
    {
        ArgumentNullException.ThrowIfNull(exception);

        string verb = string.IsNullOrWhiteSpace(action) ? "操作" : action.Trim();

        // 「合并 PDF失败：」读起来别扭，动词以拉丁字母/数字结尾时补一个空格。
        char last = verb[^1];
        if ((last >= 'A' && last <= 'Z') || (last >= 'a' && last <= 'z') || (last >= '0' && last <= '9'))
        {
            verb += " ";
        }

        if (IsEvaluationLimit(exception))
        {
            return new InvalidOperationException($"{verb}失败：{EvaluationHint}", exception);
        }

        if (ContainsCjk(exception.Message))
        {
            if (string.IsNullOrWhiteSpace(fixHint)
                || exception.Message.Contains("请", StringComparison.Ordinal))
            {
                return exception;
            }

            string cause = exception.Message.Trim().TrimEnd('。', '.');
            return new InvalidOperationException($"{cause}。{fixHint.Trim()}", exception);
        }

        if (FindInner<UnauthorizedAccessException>(exception) is not null)
        {
            return new UnauthorizedAccessException(
                $"{verb}失败：输出目录不可写：{Describe(outputDirectory)}。请换一个目录，或去掉该目录的只读属性。",
                exception);
        }

        if (FindInner<DirectoryNotFoundException>(exception) is not null)
        {
            return new DirectoryNotFoundException(
                $"{verb}失败：输出目录不存在且无法创建：{Describe(outputDirectory)}。请确认该路径可用，或换一个目录。",
                exception);
        }

        if (FindInner<IOException>(exception) is not null)
        {
            return new IOException(
                $"{verb}失败：输出目录无法写入：{Describe(outputDirectory)}。请换一个目录，或关闭占用该文件的程序后重试。",
                exception);
        }

        string reason = exception.Message.Trim().TrimEnd('。', '.');
        return new InvalidOperationException(
            $"{verb}失败：{reason}。若问题持续，请换一个输出目录，或确认源文件未被其它程序占用。",
            exception);
    }

    private static T? FindInner<T>(Exception exception)
        where T : Exception
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is T match)
            {
                return match;
            }
        }

        return null;
    }

    private static string Describe(string? outputDirectory)
        => string.IsNullOrWhiteSpace(outputDirectory) ? "（输出目录未指定）" : outputDirectory;

    private static bool ContainsCjk(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        foreach (char c in text)
        {
            if (c >= 0x4E00 && c <= 0x9FFF)
            {
                return true;
            }
        }

        return false;
    }
}
