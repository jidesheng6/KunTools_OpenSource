using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Operations;

namespace DocumentTools.QaFunction;

/// <summary>
/// dev 对照组（task-46 / task-38 验收口径）：同一输入、与 <c>dev</c> 分支（WinForms +
/// <c>DocumentTools/Services/</c>）的已知行为比对。凡是新版与 dev 已知行为不一致的，
/// 用 <see cref="DeferToTask38"/> 标 Skipped 并把证据写进 detail —— 「差异待 task-38 收口」，
/// 绝不当 PASS 记（未验证的绿灯比红灯更危险）。
/// </summary>
internal static partial class Program
{
    /// <summary>dev 与新实现行为不一致：不是回归、也不能算通过，交给 task-38 决定改哪一边。</summary>
    private sealed class ParityDeferredException : Exception
    {
        public ParityDeferredException(string message, string label) : base(message)
        {
            Label = label;
        }

        /// <summary>Skipped 行的生效前缀；默认按 dev 对照口径，派了 owner 的另标明细。</summary>
        public string Label { get; }
    }

    private static void DeferToTask38(string reason) => throw new ParityDeferredException(reason, "差异待 task-38 收口");

    /// <summary>差异已派给具体 owner、等对方改完再翻 PASS：Skipped 行要能看出该找谁要结果。</summary>
    private static void DeferToOwner(string owners, string reason)
        => throw new ParityDeferredException(reason, "已派 owner（" + owners + "），改完再翻 PASS");

    private static DocumentOperationResult RunOp(
        string featureId,
        IReadOnlyList<string> inputs,
        Dictionary<string, string> options,
        string directory)
    {
        if (!_operations.TryGetValue(featureId, out IDocumentOperation? operation))
        {
            throw new CheckException("未注册操作：" + featureId);
        }

        Directory.CreateDirectory(directory);
        return operation.ExecuteAsync(
                new DocumentOperationRequest
                {
                    InputFiles = inputs,
                    OutputDirectory = directory,
                    Options = options
                },
                CancellationToken.None)
            .GetAwaiter()
            .GetResult();
    }

    private static void RunDevParityChecks()
    {
        Console.WriteLine();
        Console.WriteLine("== dev 对照（task-38 目标行为）==");

        RunExt("dev-parity.word.pdf", CheckParityWordPdf);
        RunExt("dev-parity.excel.pdf", CheckParityExcelPdf);
        RunExt("dev-parity.html.pdf.paper", CheckParityHtmlPaper);
        RunExt("dev-parity.pdf.compliance", CheckParityPdfCompliance);
        RunExt("dev-parity.webp.image.formats", CheckParityWebpFormats);
        RunExt("dev-parity.webp.image.outputs", CheckParityWebpOutputs);
        RunExt("dev-parity.pdf.compress", CheckParityPdfCompress);
    }

    /// <summary>
    /// dev: 本机 Word 分支用 <c>wdExportCreateWordBookmarks=2</c>（每处 Word 书签一条大纲）；
    /// Aspose 分支（legacy Util.cs）没设 CreateBookmarks、且 <c>DefaultBookmarksOutlineLevel=0</c>，
    /// 所以同一份无标题样式的文档在 Aspose 下本来就没有大纲 —— 两条路径都要分别记录。
    /// </summary>
    private static ExtOutcome CheckParityWordPdf()
    {
        string source = Fixture("word", "six_pages.docx");
        RequireFixture(source);
        string root = ExtDir("dev-parity.word.pdf");
        int mode = DocumentTools.Core.Services.Office.OfficeConvertService.WordBookmarksExportMode;
        Require(mode == 2, $"书签导出模式应为 dev 的 wdExportCreateWordBookmarks=2，实际常量={mode}");

        // 路径 1：Aspose（默认）——dev 的 Aspose 分支同样没有 CreateBookmarks，期望 6 页、无大纲。
        DocumentOperationResult aspose = RunOp("word.pdf", new[] { source }, new() { ["embedFonts"] = "true" }, Path.Combine(root, "aspose"));
        Require(aspose.Success, "Aspose 分支：Success=false：" + aspose.Message);
        string asposePdf = Output(aspose, ".pdf");
        int asposePages = FileProbe.PdfPageCount(asposePdf);
        int asposeBookmarks = PdfTextProbe.BookmarkCount(asposePdf);
        Require(asposePages == 6, $"dev 期望 6 页，Aspose 实际 {asposePages} 页（{Rel(asposePdf)}）");

        // 路径 2：本机 Word——这才是 wdExportCreateWordBookmarks 的语义来源。
        DocumentOperationResult word = RunOp("word.pdf", new[] { source }, new() { ["useLocalOffice"] = "true" }, Path.Combine(root, "localword"));
        if (!word.Success)
        {
            DeferToTask38($"本机 Word 分支未能出稿（{word.Message}），无法核对 wdExportCreateWordBookmarks 的书签数");
        }

        string wordPdf = Output(word, ".pdf");
        int wordPages = FileProbe.PdfPageCount(wordPdf);
        int wordBookmarks = PdfTextProbe.BookmarkCount(wordPdf);
        Require(wordPages == 6, $"dev 期望 6 页，本机 Word 实际 {wordPages} 页（{Rel(wordPdf)}）");
        if (wordBookmarks != 2)
        {
            DeferToTask38($"夹具含 2 处 Word 书签（PARITY_2 / PARITY_5），本机 Word 期望大纲 2 条，实际 {wordBookmarks} 条");
        }

        return new ExtOutcome(true, Rel(source), Rel(asposePdf) + "; " + Rel(wordPdf),
            FileProbe.Size(asposePdf) + FileProbe.Size(wordPdf),
            $"本机 Word：pages={wordPages}、bookmarks={wordBookmarks}（每处 Word 书签 1 条，非按标题）; "
            + $"Aspose：pages={asposePages}、bookmarks={asposeBookmarks}（dev 的 Aspose 分支同样未设 CreateBookmarks）; 常量={mode}");
    }

    /// <summary>dev: 「每表一份 PDF」复选框 ⇒ <c>eachSheet=true</c> 每张工作表一份。</summary>
    private static ExtOutcome CheckParityExcelPdf()
    {
        string source = Fixture("excel", "three_sheets.xlsx");
        RequireFixture(source);
        string root = ExtDir("dev-parity.excel.pdf");

        DocumentOperationResult each = RunOp("excel.pdf", new[] { source }, new() { ["eachSheet"] = "true" }, Path.Combine(root, "each"));
        Require(each.Success, "eachSheet=true：Success=false：" + each.Message);
        List<string> split = each.OutputFiles.Where(File.Exists).ToList();
        int splitPages = split.Sum(FileProbe.PdfPageCount);
        Require(split.Count == 3, $"dev 期望 3 张表各一份 PDF，实际 {split.Count} 份（{Join(split)}）");
        Require(splitPages == 3, $"dev 期望 3 份合计 3 页（每表 1 页），实际合计 {splitPages} 页");

        DocumentOperationResult single = RunOp("excel.pdf", new[] { source }, new() { ["eachSheet"] = "false" }, Path.Combine(root, "single"));
        Require(single.Success, "eachSheet=false：Success=false：" + single.Message);
        string merged = Output(single, ".pdf");
        int mergedPages = FileProbe.PdfPageCount(merged);
        Require(mergedPages >= 3, $"dev 期望单文件包含 3 张表（≥3 页），实际 {mergedPages} 页");

        return new ExtOutcome(true, Rel(source), Join(split) + " + " + Rel(merged),
            split.Sum(FileProbe.Size) + FileProbe.Size(merged),
            $"eachSheet=true→{split.Count} 份/{splitPages} 页; eachSheet=false→1 份/{mergedPages} 页");
    }

    /// <summary>dev: Html2PDF 纸张下拉默认 A4（SelectedIndex=4），可选 Letter / Legal。</summary>
    private static ExtOutcome CheckParityHtmlPaper()
    {
        string source = Fixture("html", "sample.html");
        RequireFixture(source);
        string root = ExtDir("dev-parity.html.pdf.paper");

        (string Paper, double Width, double Height)[] expected =
        {
            ("A4", 595.92, 841.92),
            ("Letter", 612, 792),
            ("Legal", 612, 1008)
        };

        var parts = new List<string>();
        var failures = new List<string>();
        long bytes = 0;
        foreach ((string paper, double width, double height) in expected)
        {
            DocumentOperationResult result = RunOp(
                "html.pdf",
                new[] { source },
                new() { ["paper"] = paper, ["waitResources"] = "true", ["timeout"] = "45", ["navTimeout"] = "45" },
                Path.Combine(root, paper.ToLowerInvariant()));
            if (!result.Success)
            {
                failures.Add($"{paper}: Success=false（{result.Message}）");
                continue;
            }

            string pdf = Output(result, ".pdf");
            (double actualWidth, double actualHeight) = PdfTextProbe.FirstPageRect(pdf);
            bytes += FileProbe.Size(pdf);
            parts.Add($"{paper}={actualWidth:F2}x{actualHeight:F2}");
            if (Math.Abs(actualWidth - width) > 1 || Math.Abs(actualHeight - height) > 1)
            {
                failures.Add($"dev 期望 {paper} = {width}x{height} pt，实际 {actualWidth:F2}x{actualHeight:F2}（{Rel(pdf)}）");
            }
        }

        Require(failures.Count == 0, string.Join("；", failures));
        return new ExtOutcome(true, Rel(source), string.Join("; ", parts), bytes,
            $"Page.Rect（pt）：{string.Join("; ", parts)}；dev 出厂默认 A4（SelectedIndex=4）");
    }

    /// <summary>dev: 版本下拉直接给 Aspose <c>PdfFormat</c> 枚举名（如 <c>v_1_7</c>），输出头版本须与所选一致。</summary>
    private static ExtOutcome CheckParityPdfCompliance()
    {
        string source = Fixture("pdf", "sample.pdf");
        RequireFixture(source);
        string root = ExtDir("dev-parity.pdf.compliance");

        (string Version, string Expected)[] cases = { ("v_1_4", "1.4"), ("v_1_7", "1.7") };
        var parts = new List<string>();
        long bytes = 0;
        foreach ((string version, string expected) in cases)
        {
            DocumentOperationResult result = RunOp(
                "pdf.compliance",
                new[] { source },
                new() { ["version"] = version },
                Path.Combine(root, version.Replace('.', '_')));
            Require(result.Success, $"version={version}：Success=false：{result.Message}");
            string pdf = Output(result, ".pdf");
            string actual = PdfTextProbe.Version(pdf);
            bytes += FileProbe.Size(pdf);
            parts.Add($"{version}→{actual}");
            Require(actual == expected,
                $"dev 期望 version={version} 输出 PDF 头版本 {expected}，实际 {actual}（{Rel(pdf)}）");
        }

        return new ExtOutcome(true, Rel(source), string.Join("; ", parts), bytes,
            $"PDF 头版本：{string.Join("; ", parts)}（dev 传 Aspose 枚举名，如 v_1_7）");
    }

    /// <summary>
    /// dev 的 <c>webp2image_combox</c>（<c>BodyForm.cs:1423-1432</c>）是五档、按 JPG/JPEG/PNG/BMP/GIF 同序、
    /// 默认第 0 行 JPG。task-38 把两项补成五档后，这条差异消失，改成对 Schema 实值的硬断言。
    /// </summary>
    private static ExtOutcome CheckParityWebpFormats()
    {
        string[] expected = ["JPG", "JPEG", "PNG", "BMP", "GIF"];
        IReadOnlyList<OptionSection> sections = FeatureOptionSchemas.GetSections("webp.image");
        OptionField? field = sections
            .SelectMany(section => section.Fields)
            .FirstOrDefault(item => string.Equals(item.Key, "format", StringComparison.Ordinal));
        Require(field is not null, "webp.image 的 Schema 里没有 format 字段");
        Require(field!.Choices is { Count: > 0 }, "webp.image 的 format 没有选项列表");

        string[] values = field.Choices!.Select(choice => choice.Value).ToArray();
        Require(
            values.SequenceEqual(expected, StringComparer.Ordinal),
            $"dev 期望 format 档位按序为 [{string.Join(", ", expected)}]，实际 [{string.Join(", ", values)}]");
        Require(
            string.Equals(field.DefaultValue, "JPG", StringComparison.Ordinal),
            $"dev 期望默认档位 JPG（BodyForm.cs:1423 第 0 行），实际「{field.DefaultValue}」");

        return new ExtOutcome(true, "webp.image Schema", "format 五档 + 默认值",
            0, $"format = [{string.Join(", ", values)}]，默认 {field.DefaultValue}（与 dev webp2image_combox 同序同项）");
    }

    /// <summary>
    /// 产物层：五档各跑一次，只看「后缀 + 魔数 + 尺寸」。
    /// dev 的 <c>ConvertWeBp2Image</c> 把 <c>JpegOptions.Quality</c> 固定 100、UI 也没有质量档，
    /// 所以这里**不**断言 quality 键（断言它等于把新参数写成 dev 契约）。
    /// </summary>
    private static ExtOutcome CheckParityWebpOutputs()
    {
        string source = Fixture("image", "sample.webp");
        RequireFixture(source);
        string root = ExtDir("dev-parity.webp.image.outputs");
        (int SourceWidth, int SourceHeight) = FileProbe.PngSize(Fixture("image", "sample.png"));

        var failures = new List<string>();
        var parts = new List<string>();
        long bytes = 0;

        foreach ((string format, string extension, byte[] magic, string magicLabel) in new[]
        {
            ("JPG", ".jpg", new byte[] { 0xFF, 0xD8, 0xFF }, "FF D8 FF"),
            ("JPEG", ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF }, "FF D8 FF"),
            ("PNG", ".png", new byte[] { 0x89, 0x50, 0x4E, 0x47 }, "89 50 4E 47"),
            ("BMP", ".bmp", new byte[] { 0x42, 0x4D }, "42 4D"),
            ("GIF", ".gif", new byte[] { 0x47, 0x49, 0x46, 0x38 }, "47 49 46 38")
        })
        {
            DocumentOperationResult result = RunOp(
                "webp.image",
                new[] { source },
                new() { ["format"] = format },
                Path.Combine(root, format.ToLowerInvariant()));
            if (!result.Success)
            {
                failures.Add($"{format}: Success=false（{result.Message}）");
                continue;
            }

            string output = result.OutputFiles.FirstOrDefault(File.Exists) ?? string.Empty;
            if (output.Length == 0)
            {
                failures.Add($"{format}: 未产出文件");
                continue;
            }

            long size = FileProbe.Size(output);
            bytes += size;
            if (!output.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                failures.Add($"{format}: 期望 {extension}，实际 {Path.GetExtension(output)}（{Rel(output)}）");
                continue;
            }

            byte[] head = FileProbe.Head(output, 8);
            if (head.Length < magic.Length || !head.Take(magic.Length).SequenceEqual(magic))
            {
                failures.Add(
                    $"{format}: 期望魔数 {magicLabel}，实际 {ToHex(head)}（{Rel(output)}，{size} B）");
                continue;
            }

            (int width, int height) = extension switch
            {
                ".png" => FileProbe.PngSize(output),
                ".jpg" or ".jpeg" => FileProbe.JpegSize(output),
                _ => (0, 0)
            };
            if (width > 0 && (width != SourceWidth || height != SourceHeight))
            {
                failures.Add($"{format}: 尺寸 {width}x{height} 与源 {SourceWidth}x{SourceHeight} 不一致");
                continue;
            }

            parts.Add($"{format}→{extension} {size} B");
        }

        Require(failures.Count == 0, string.Join("；", failures));

        return new ExtOutcome(true, Rel(source), string.Join("; ", parts), bytes,
            $"{string.Join("; ", parts)}；后缀/魔数/JPG·PNG 尺寸均与 dev 一致（源 {SourceWidth}x{SourceHeight}）；dev 无 quality 档，故不断言");
    }

    /// <summary>dev: CompressPDF；新版对无可压缩空间的 PDF 回退原始内容并给出 <c>KeptOriginal</c> 说明。</summary>
    private static ExtOutcome CheckParityPdfCompress()
    {
        string text = Fixture("pdf", "sample.pdf");
        RequireFixture(text);
        string root = ExtDir("dev-parity.pdf.compress");

        // 图片密集样本：用真实链路 image.pdf 把两张图做成 PDF，而不是拿文字 PDF 冒充。
        DocumentOperationResult images = RunOp(
            "image.pdf",
            new[] { Fixture("image", "sample.png"), Fixture("image", "sample.jpg") },
            new() { ["onePdfPerImage"] = "false" },
            Path.Combine(root, "input"));
        Require(images.Success, "生成图片样本失败：" + images.Message);
        string dense = Output(images, ".pdf");

        var parts = new List<string>();
        long bytes = 0;
        foreach ((string label, string input) in new[] { ("文字", text), ("图片密集", dense) })
        {
            DocumentOperationResult result = RunOp(
                "pdf.compress",
                new[] { input },
                new() { ["quality"] = "中", ["maxDpi"] = "150" },
                Path.Combine(root, label == "文字" ? "text" : "dense"));
            Require(result.Success, $"{label}：Success=false：{result.Message}");
            string output = Output(result, ".pdf");
            long before = FileProbe.Size(input);
            long after = FileProbe.Size(output);
            bool keptOriginal = result.Message.Contains("已保留原始内容", StringComparison.Ordinal);
            bytes += after;

            if (keptOriginal)
            {
                // 回退标志必须与字节一致：说「保留了原始内容」就不能同时把文件改小。
                Require(after == before,
                    $"{label}：报告「已保留原始内容」但体积从 {before} B 变为 {after} B（{Rel(output)}）");
            }

            parts.Add($"{label}: {before}→{after} B（{(after * 100.0 / before):F1}%）KeptOriginal={keptOriginal}");
        }

        return new ExtOutcome(true, Rel(text) + "; " + Rel(dense), string.Join("; ", parts), bytes,
            string.Join("; ", parts) + "（dev 体积基准：扫描件/照片类样本本机暂无 dev 实测，仅记录回退标志与体积比）");
    }
}
