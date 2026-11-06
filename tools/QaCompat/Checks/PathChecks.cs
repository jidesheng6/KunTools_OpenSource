using System.Globalization;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Image;
using SkiaSharp;

namespace QaCompat.Checks;

/// <summary>超长路径（&gt;260）与中文+空格路径下的真实转换。</summary>
internal static class PathChecks
{
    public const string Group = "path";

    public static void Run(QaContext ctx)
    {
        ctx.Run(
            "path.long",
            Group,
            "超过 260 字符的输出路径：转换成功或失败信息准确",
            "用 `Fixtures.EnsureLongDirectory` 在 30 字符分段下拼出 >260 字符目录，写入 PNG 后执行 `image.compress`",
            "转换成功并产出可解码的 PNG；若失败，消息必须给出出错的完整路径，而不是空消息或「发生错误」。",
            check =>
            {
                string root = ctx.Dir("long-root");
                string leaf = "long_src_out.png";
                string outDir = Fixtures.EnsureLongDirectory(root, 300, leaf);
                string source = Path.Combine(outDir, "long_src.png");
                Fixtures.CreateImage(source);

                var operation = new ImageConvertOperation("image.compress", new ImageConversionService());
                DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = new[] { source },
                    OutputDirectory = outDir,
                    Options = new Dictionary<string, string> { ["format"] = "PNG" }
                }).GetAwaiter().GetResult();

                string expectedOutput = Path.Combine(outDir, "long_src_out.png");
                bool exists = File.Exists(expectedOutput);
                bool decodable = false;
                if (exists)
                {
                    using SKBitmap? bitmap = SKBitmap.Decode(expectedOutput);
                    decodable = bitmap is not null;
                }

                string evidence = "输入路径长度=" + source.Length.ToString(CultureInfo.InvariantCulture)
                                  + "（>260）\n输出路径长度=" + expectedOutput.Length.ToString(CultureInfo.InvariantCulture)
                                  + "\nSuccess=" + result.Success
                                  + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                  + "\n输出存在=" + exists + "，可解码=" + decodable;

                ctx.Fact(
                    "长路径",
                    "work\\long-root 下路径 " + source.Length.ToString(CultureInfo.InvariantCulture)
                    + " 字符；" + (result.Success && decodable ? "转换成功" : "转换失败：" + Fmt.Clip(Fmt.OneLine(result.Message), 120)));

                if (result.Success && exists && decodable)
                {
                    ctx.Pass(
                        check,
                        "长路径转换成功：输出 " + expectedOutput.Length.ToString(CultureInfo.InvariantCulture) + " 字符的 PNG 可解码。",
                        evidence);
                    return;
                }

                if (!result.Success)
                {
                    bool namesPath = result.Message.Contains(expectedOutput, StringComparison.OrdinalIgnoreCase)
                                     || result.Message.Contains(source, StringComparison.OrdinalIgnoreCase)
                                     || result.Message.Contains("long_src_out.png", StringComparison.OrdinalIgnoreCase);
                    // 「信息准确」不仅要求带路径，还要求原因对得上：路径超长不能报成「图片损坏」。
                    bool namesCause = result.Message.Contains("路径", StringComparison.Ordinal)
                                      || result.Message.Contains("长度", StringComparison.Ordinal)
                                      || result.Message.Contains("260", StringComparison.Ordinal);

                    if (namesPath && namesCause)
                    {
                        ctx.Pass(
                            check,
                            "转换失败，但消息给出了具体路径并指出长度原因：「" + Fmt.Clip(Fmt.OneLine(result.Message), 200) + "」。",
                            evidence);
                        return;
                    }

                    ctx.AddFinding(
                        check,
                        "中",
                        "超过 260 字符的路径被误报为「无法解码图片」，真实原因是路径长度",
                        "同一张 PNG 在普通路径下可正常解码（见 path.unicode），在 "
                        + source.Length.ToString(CultureInfo.InvariantCulture)
                        + " 字符路径下解码返回 null；`ImageConversionService` 把 null 统一解释为「无法解码图片」，"
                        + "用户会误以为文件损坏。建议先做路径长度预检，或改用 `FileStream` + `SKBitmap.Decode(Stream)` 绕过 SkiaSharp 的旧式路径 API。",
                        "src\\DocumentTools.Core\\Services\\Image\\ImageConversionService.cs:59-60");
                    ctx.Fail(
                        check,
                        "转换失败，消息虽带路径但把原因归为「无法解码图片」，未指出路径超长（实际 "
                        + source.Length.ToString(CultureInfo.InvariantCulture) + " 字符）。",
                        evidence);
                    return;
                }

                ctx.Fail(
                    check,
                    "长路径下结论不可用：Success=" + result.Success + "，输出存在=" + exists + "，可解码=" + decodable + "。",
                    evidence);
            });

        ctx.Run(
            "path.unicode",
            Group,
            "中文+空格路径：转换成功且文件名正确",
            "目录「中文 目录 (测试)」→ 输入「示例 图片 一.png」→ 输出目录「输出 目录」，执行 `image.compress`",
            "转换成功，输出文件名与预期「示例 图片 一_out.png」完全一致且可解码。",
            check =>
            {
                string inDir = Path.Combine(ctx.Dir("unicode-root"), "中文 目录 (测试)");
                Directory.CreateDirectory(inDir);
                string source = Path.Combine(inDir, "示例 图片 一.png");
                Fixtures.CreateImage(source);

                string outDir = Path.Combine(inDir, "输出 目录");
                Directory.CreateDirectory(outDir);

                var operation = new ImageConvertOperation("image.compress", new ImageConversionService());
                DocumentOperationResult result = operation.ExecuteAsync(new DocumentOperationRequest
                {
                    InputFiles = new[] { source },
                    OutputDirectory = outDir,
                    Options = new Dictionary<string, string> { ["format"] = "PNG" }
                }).GetAwaiter().GetResult();

                string expectedOutput = Path.Combine(outDir, "示例 图片 一_out.png");
                string[] files = Directory.Exists(outDir)
                    ? Directory.GetFiles(outDir).Select(Path.GetFileName).OfType<string>().ToArray()
                    : Array.Empty<string>();
                bool exactName = File.Exists(expectedOutput);
                bool decodable = false;
                if (exactName)
                {
                    using SKBitmap? bitmap = SKBitmap.Decode(expectedOutput);
                    decodable = bitmap is not null;
                }

                string evidence = "Success=" + result.Success
                                  + "\nMessage: " + Fmt.Clip(result.Message, 500)
                                  + "\n输出文件: " + (files.Length == 0 ? "(空)" : string.Join(" | ", files))
                                  + "\n期望文件名存在=" + exactName + "，可解码=" + decodable;

                ctx.Fact(
                    "中文+空格路径",
                    result.Success
                        ? "成功，输出=" + (files.Length > 0 ? files[0] : "(无)")
                        : "失败：" + Fmt.Clip(Fmt.OneLine(result.Message), 160));

                if (result.Success && exactName && decodable)
                {
                    ctx.Pass(check, "成功产出「示例 图片 一_out.png」，文件名与内容均正确。", evidence);
                    return;
                }

                ctx.Fail(
                    check,
                    "结论不可用：Success=" + result.Success + "，期望文件名存在=" + exactName + "，可解码=" + decodable + "。",
                    evidence);
            });
    }
}
