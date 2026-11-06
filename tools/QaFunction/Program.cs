using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DocumentTools.Core.DependencyInjection;
using DocumentTools.Core.Licensing;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Code;
using DocumentTools.Core.Services.Exam;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.QaFunction;

/// <summary>Raised by a check's verification delegate; turned into a FAIL row.</summary>
internal sealed class CheckException : Exception
{
    public CheckException(string message) : base(message)
    {
    }
}

internal sealed record CheckResult(
    string Feature,
    string Input,
    string Outputs,
    long Bytes,
    double ElapsedMs,
    bool Pass,
    string Detail,
    bool Required,
    string Group = "",
    bool Skipped = false);

/// <summary>
/// End-to-end function harness: resolves the real Core operations through the real DI
/// container, feeds committed fixtures from <c>testdata/</c>, and verifies every produced
/// file structurally (signature, OOXML entries, page objects) instead of trusting the
/// operation's own success flag.
/// </summary>
internal static partial class Program
{
    private static string _repoRoot = string.Empty;
    private static string _testData = string.Empty;
    private static string _outRoot = string.Empty;
    private static HashSet<string>? _filter;
    private static bool _runBonus;
    private static bool _childNoAce;

    private static readonly List<CheckResult> Results = new();
    private static ServiceProvider? _provider;
    private static Dictionary<string, IDocumentOperation> _operations = new(StringComparer.Ordinal);

    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            _repoRoot = ResolveRepoRoot();
            _testData = Path.Combine(_repoRoot, "testdata");
            _outRoot = Path.Combine(_repoRoot, "artifacts", "qa-function");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("无法定位仓库根目录：" + ex.Message);
            return 1;
        }

        _filter = ParseFilter(args);
        _runBonus = args.Contains("--bonus", StringComparer.OrdinalIgnoreCase);

        // 无 ACE 通道的降级路径必须在独立进程里验证：AceAvailability 是 Lazy 静态，同进程内无法伪造两次不同环境。
        _childNoAce = args.Contains("--child-no-ace", StringComparer.OrdinalIgnoreCase);
        if (_childNoAce)
        {
            _filter = new HashSet<string>(new[] { "exam.*" }, StringComparer.OrdinalIgnoreCase);
        }

        if (args.Contains("--hook-probe", StringComparer.OrdinalIgnoreCase))
        {
            return RunHookProbe();
        }

        if (args.Contains("--font-probe", StringComparer.OrdinalIgnoreCase))
        {
            return RunFontProbe();
        }

        Console.WriteLine("== 环境与 Aspose 授权探测 ==");
        PrintEnvironmentProbe();
        Console.WriteLine();

        BuildContainer();
        Console.WriteLine($"  operations(registered): {_operations.Count}");
        Console.WriteLine();

        Directory.CreateDirectory(_outRoot);
        if (!_childNoAce)
        {
            // 子进程跑在父进程已经写好的 artifacts 目录里，绝不能清空它。
            foreach (string existing in Directory.GetDirectories(_outRoot))
            {
                Directory.Delete(existing, true);
            }
        }

        await RunRequiredChecksAsync();
        await RunExtendedChecksAsync();
        if (_runBonus)
        {
            await RunBonusChecksAsync();
        }

        return PrintSummaryAndWriteArtifacts();
    }

    // ---------------------------------------------------------------- bootstrap

    private static string ResolveRepoRoot()
    {
        DirectoryInfo? current = new(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "DocumentTools.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("从 " + AppContext.BaseDirectory + " 向上未找到 DocumentTools.sln");
    }

    private static HashSet<string>? ParseFilter(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], "--filter", StringComparison.OrdinalIgnoreCase) || i + 1 >= args.Length)
            {
                continue;
            }

            return new HashSet<string>(
                args[i + 1].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                StringComparer.OrdinalIgnoreCase);
        }

        return null;
    }

    private static void BuildContainer()
    {
        var services = new ServiceCollection();
        services.AddDocumentToolsCore();
        _provider = services.BuildServiceProvider();

        foreach (IDocumentOperation operation in _provider.GetServices<IDocumentOperation>())
        {
            _operations[operation.FeatureId] = operation;
        }
    }

    private static void PrintEnvironmentProbe()
    {
        string hookPath = Path.Combine(_repoRoot, "libs", "Aspose.Hook", "Aspose.Hook.dll");
        string licenseDir = Path.Combine(_repoRoot, "assets", "licenses");
        string[] licenseFiles = Directory.Exists(licenseDir)
            ? Directory.GetFiles(licenseDir, "*.lic", SearchOption.AllDirectories)
            : Array.Empty<string>();
        string? envLicense = Environment.GetEnvironmentVariable("ASPOSE_LICENSE_PATH");

        Console.WriteLine($"  repoRoot            : {_repoRoot}");
        Console.WriteLine($"  testdata            : {_testData}");
        Console.WriteLine($"  hookDll             : {Rel(hookPath)} exists={File.Exists(hookPath)}"
                          + (File.Exists(hookPath) ? $" size={FileProbe.Size(hookPath)}" : string.Empty));
        Console.WriteLine($"  licenseFiles(.lic)  : {licenseFiles.Length} in {Rel(licenseDir)}");
        Console.WriteLine($"  ASPOSE_LICENSE_PATH : {(string.IsNullOrWhiteSpace(envLicense) ? "(null)" : envLicense)}");

        AsposeLicenseStatus status = AsposeLicenseLoader.TryLoad();
        Console.WriteLine($"  AsposeLicenseLoader : IsLicensed={status.IsLicensed} path={status.LicensePath ?? "(none)"}");

        Console.WriteLine($"  Aspose.Words        : {typeof(Aspose.Words.Document).Assembly.GetName().Version}");
        Console.WriteLine($"  Aspose.Pdf          : {typeof(Aspose.Pdf.Document).Assembly.GetName().Version}");
        Console.WriteLine($"  Aspose.Cells        : {typeof(Aspose.Cells.Workbook).Assembly.GetName().Version}");
        Console.WriteLine($"  Aspose.Slides       : {typeof(Aspose.Slides.Presentation).Assembly.GetName().Version}");
        Console.WriteLine($"  Aspose.BarCode      : {typeof(Aspose.BarCode.Generation.BarcodeGenerator).Assembly.GetName().Version}");
        Console.WriteLine($"  Aspose.Html         : {typeof(Aspose.Html.Converters.Converter).Assembly.GetName().Version}");
    }

    /// <summary>
    /// Optional isolated probe: load libs/Aspose.Hook via reflection and start it *before*
    /// any Aspose type is touched. Kept in a separate process on purpose — a mismatched hook
    /// can crash the CLR, and that must not take the functional run down with it.
    /// </summary>
    private static int RunHookProbe()
    {
        string hookPath = Path.Combine(_repoRoot, "libs", "Aspose.Hook", "Aspose.Hook.dll");
        Console.WriteLine("== Aspose.Hook 独立探针 ==");
        Console.WriteLine($"  hookDll : {Rel(hookPath)} exists={File.Exists(hookPath)}");
        if (!File.Exists(hookPath))
        {
            Console.WriteLine("  结论    : 未提供 Hook DLL，跳过");
            return 0;
        }

        try
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFrom(hookPath);
            Console.WriteLine($"  loaded  : {assembly.FullName}");
            Type? manager = assembly.GetType("Aspose.Hook.HookManager");
            Console.WriteLine($"  HookManager : {(manager is null ? "missing" : "found")}");
            if (manager is null)
            {
                return 0;
            }

            foreach (string methodName in new[] { "ShowHookDetails", "StartHook" })
            {
                System.Reflection.MethodInfo? method = manager.GetMethod(
                    methodName,
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method is null)
                {
                    Console.WriteLine($"  {methodName} : missing");
                    continue;
                }

                object?[] callArgs = method.GetParameters().Length == 1 ? new object?[] { true } : Array.Empty<object?>();
                object? returned = method.Invoke(null, callArgs);
                Console.WriteLine($"  {methodName} : invoked, returned={returned ?? "(null)"}");
            }

            AsposeLicenseStatus status = AsposeLicenseLoader.TryLoad();
            Console.WriteLine($"  license after hook : IsLicensed={status.IsLicensed} path={status.LicensePath ?? "(none)"}");
            Console.WriteLine("  结论    : Hook 已加载并启动（是否真正生效需看转换产物的评估水印）");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  FAILED : {ex.GetType().Name}: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Proves whether the watermark default font name ("宋体") is resolvable by
    /// Aspose.Pdf.FontRepository, and which spellings are. Isolates the font naming
    /// problem from "font missing on this machine".
    /// </summary>
    private static int RunFontProbe()
    {
        Console.WriteLine("== Aspose.Pdf 字体解析探针 ==");
        Console.WriteLine($"  FontRepository.Sources : {Aspose.Pdf.Text.FontRepository.Sources.Count}");
        string[] candidates =
        [
            "宋体", "SimSun", "黑体", "SimHei", "微软雅黑", "Microsoft YaHei",
            "Arial", "Times New Roman", "Courier New"
        ];

        foreach (string name in candidates)
        {
            try
            {
                Aspose.Pdf.Text.Font font = Aspose.Pdf.Text.FontRepository.FindFont(name);
                Console.WriteLine($"  {name,-16} : OK  -> FontName={font.FontName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  {name,-16} : FAIL -> {ex.Message}");
            }
        }

        return 0;
    }

    /// <summary>
    /// Runs a feature once into a diagnostic folder without touching the pass/fail matrix.
    /// Used to separate "the code path is broken" from "this particular option set is broken".
    /// </summary>
    private static (bool ok, string message, string[] files) TryRun(
        string featureId,
        string tag,
        IReadOnlyList<string> inputs,
        IDictionary<string, string>? options)
    {
        if (!_operations.TryGetValue(featureId, out IDocumentOperation? operation))
        {
            return (false, $"未注册 {featureId}", []);
        }

        string outDir = Path.Combine(_outRoot, "_diag", tag);
        Directory.CreateDirectory(outDir);
        try
        {
            var request = new DocumentOperationRequest
            {
                InputFiles = inputs,
                OutputDirectory = outDir,
                Options = options
            };

            DocumentOperationResult result = operation.ExecuteAsync(request).GetAwaiter().GetResult();
            string[] files = Directory.GetFiles(outDir)
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToArray();
            bool ok = result.Success && files.Length > 0;
            return (ok, result.Message, files);
        }
        catch (Exception ex)
        {
            return (false, $"{ex.GetType().Name}: {ex.Message}", []);
        }
    }

    /// <summary>
    /// Diagnostic-only rerun of the watermark feature with an explicit, Aspose-resolvable font
    /// name. Never part of the pass/fail matrix; it separates "feature broken" from
    /// "default font name unresolvable".
    /// </summary>
    private static bool ProbeWatermarkWithFont(string pdf, string fontName)
    {
        (bool ok, string message, string[] files) = TryRun(
            "pdf.watermark.add",
            "pdf.watermark.add.font-probe",
            [pdf],
            new Dictionary<string, string>
            {
                ["text"] = "QA 水印",
                ["opacity"] = "0.35",
                ["rotate"] = "45",
                ["repeat"] = "true",
                ["fontName"] = fontName
            });

        Console.WriteLine($"    ↳ 诊断：fontName={fontName} success={ok} message={message} 产出={files.Length}");
        return ok;
    }

    /// <summary>
    /// Diagnostic-only sweep over the excel.pdf option combinations, because "eachSheet" is the
    /// only documented switch that changes the produced file set.
    /// </summary>
    private static void ProbeExcelPdf(string xlsx)
    {
        (string Label, Dictionary<string, string> Options)[] cases =
        [
            ("单文件（eachSheet 缺省）", new Dictionary<string, string>()),
            ("eachSheet=true", new Dictionary<string, string> { ["eachSheet"] = "true" }),
            ("eachSheet=true+fitOnePage", new Dictionary<string, string>
            {
                ["eachSheet"] = "true",
                ["fitOnePage"] = "true"
            })
        ];

        foreach ((string label, Dictionary<string, string> options) in cases)
        {
            (bool ok, string message, string[] files) = TryRun("excel.pdf", "excel.pdf." + label, [xlsx], options);
            string produced = files.Length == 0 ? "(none)" : string.Join("; ", files.Select(Rel));
            Console.WriteLine($"    ↳ 诊断：excel.pdf {label} success={ok} message={message} 产出={produced}");
        }
    }

    // ---------------------------------------------------------------- check plumbing

    private static async Task OpAsync(
        string featureId,
        IReadOnlyList<string> inputs,
        IDictionary<string, string>? options,
        Func<DocumentOperationResult, string> verify,
        bool required = true,
        string group = "")
    {
        string outDir = Path.Combine(_outRoot, featureId);
        Directory.CreateDirectory(outDir);

        if (!_operations.TryGetValue(featureId, out IDocumentOperation? operation))
        {
            Results.Add(new CheckResult(featureId, Join(inputs), "(none)", 0, 0, false,
                "DI 容器未注册该 FeatureId", required, group));
            return;
        }

        var request = new DocumentOperationRequest
        {
            InputFiles = inputs,
            OutputDirectory = outDir,
            Options = options
        };

        Stopwatch watch = Stopwatch.StartNew();
        DocumentOperationResult result;
        try
        {
            result = await operation.ExecuteAsync(request);
        }
        catch (Exception ex)
        {
            watch.Stop();
            Results.Add(new CheckResult(featureId, Join(inputs), "(none)", 0, watch.Elapsed.TotalMilliseconds,
                false, $"操作抛出 {ex.GetType().Name}: {ex.Message}", required, group));
            return;
        }

        watch.Stop();
        long bytes = result.OutputFiles.Where(File.Exists).Sum(FileProbe.Size);
        string outputs = result.OutputFiles.Count == 0 ? "(none)" : Join(result.OutputFiles);

        string detail;
        bool pass;
        try
        {
            detail = verify(result);
            pass = result.Success;
            if (!pass)
            {
                detail = "Success=false：" + result.Message + " | " + detail;
            }
        }
        catch (CheckException ex)
        {
            pass = false;
            detail = ex.Message + " (opMessage=" + result.Message + ")";
        }

        Results.Add(new CheckResult(featureId, Join(inputs), outputs, bytes,
            watch.Elapsed.TotalMilliseconds, pass, detail, required, group));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new CheckException(message);
        }
    }

    private static string Output(DocumentOperationResult result, string extension)
    {
        string? path = result.OutputFiles.FirstOrDefault(
            p => p.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        Require(path is not null, $"未产出 {extension} 文件（共 {result.OutputFiles.Count} 个输出）");
        Require(File.Exists(path!), $"输出路径不存在：{path}");
        Require(FileProbe.Size(path!) > 0, $"输出为空文件：{path}");
        return path!;
    }

    private static string Fixture(params string[] relative)
        => Path.Combine(_testData, Path.Combine(relative));

    private static void RequireFixture(string path)
        => Require(File.Exists(path), "夹具缺失：" + Rel(path));

    private static string Rel(string path)
        => Path.GetRelativePath(_repoRoot, path).Replace('\\', '/');

    private static string Join(IEnumerable<string> paths)
        => string.Join("; ", paths.Select(Rel));

    private static string Mb(long bytes)
        => bytes < 1024 ? $"{bytes} B" : bytes < 1024 * 1024 ? $"{bytes / 1024.0:F1} KB" : $"{bytes / (1024.0 * 1024):F2} MB";

    // ---------------------------------------------------------------- required checks

    private static async Task RunRequiredChecksAsync()
    {
        Console.WriteLine("== 必测项（16）==");

        string docx = Fixture("word", "sample.docx");
        string docx2 = Fixture("word", "sample2.docx");
        string xlsx = Fixture("excel", "sample.xlsx");
        string pptx = Fixture("ppt", "sample.pptx");
        string pdf = Fixture("pdf", "sample.pdf");
        string png = Fixture("image", "sample.png");
        string jpg = Fixture("image", "sample.jpg");
        string html = Fixture("html", "sample.html");

        foreach (string path in new[] { docx, docx2, xlsx, pptx, pdf, png, jpg, html })
        {
            RequireFixture(path);
        }

        if (Wanted("word.pdf"))
        {
            await OpAsync("word.pdf", new[] { docx }, null, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages == 3, $"页数 {pages} != 源文档 3 页");
                string text = PdfTextProbe.Extract(output);
                Require(text.Contains("DocumentTools", StringComparison.Ordinal),
                    "PDF 文本层未提取到源文档内容（可能只渲染了空白页）");
                bool watermark = text.Contains("Evaluation", StringComparison.OrdinalIgnoreCase);
                return $"pdf pages={pages} textKept=True evalWatermark={watermark}";
            });
        }

        if (Wanted("pdf.word"))
        {
            await OpAsync("pdf.word", new[] { pdf }, null, result =>
            {
                string output = Output(result, ".docx");
                Require(FileProbe.IsOfficePackage(output, "word/document.xml"),
                    "不是合法 DOCX（缺 word/document.xml）");
                string xml = FileProbe.ReadZipText(output, "word/document.xml");
                Require(xml.Contains("<w:p", StringComparison.Ordinal), "文档正文为空");
                bool textKept = xml.Contains("DocumentTools", StringComparison.Ordinal);
                return $"docx entry=ok paragraphs={CountRegex(xml, "<w:p[ >/]")} textKept={textKept}";
            });
        }

        if (Wanted("excel.pdf"))
        {
            await OpAsync("excel.pdf", new[] { xlsx }, new Dictionary<string, string>
            {
                ["eachSheet"] = "true",
                ["fitOnePage"] = "true"
            }, result =>
            {
                // eachSheet=true makes the service write one PDF per worksheet, so cross-check the
                // reported output list against what actually landed on disk.
                List<string> missing = result.OutputFiles.Where(p => !File.Exists(p)).ToList();
                string[] actual = Directory
                    .GetFiles(Path.Combine(_outRoot, "excel.pdf"), "*.pdf")
                    .OrderBy(p => p, StringComparer.Ordinal)
                    .ToArray();
                Require(missing.Count == 0,
                    "操作返回的输出路径不存在：" + string.Join("; ", missing.Select(Rel))
                    + "；实际产出：" + string.Join("; ", actual.Select(Rel)));
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages >= 1, $"页数 {pages} < 1");
                return $"pdf pages={pages} (workbook sheets=2)";
            });

            ProbeExcelPdf(xlsx);
        }

        if (Wanted("ppt.pdf"))
        {
            await OpAsync("ppt.pdf", new[] { pptx }, null, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages == 2, $"页数 {pages} != 幻灯片 2 页");
                return $"pdf pages={pages}";
            });
        }

        if (Wanted("image.pdf"))
        {
            await OpAsync("image.pdf", new[] { png, jpg }, null, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages == 2, $"页数 {pages} != 输入图片 2 张");
                return $"pdf pages={pages} inputs=2";
            });
        }

        if (Wanted("pdf.excel"))
        {
            await OpAsync("pdf.excel", new[] { pdf }, null, result =>
            {
                string output = Output(result, ".xlsx");
                Require(FileProbe.IsOfficePackage(output, "xl/workbook.xml"),
                    "不是合法 XLSX（缺 xl/workbook.xml）");
                int sheets = CountZipEntries(output, "xl/worksheets/sheet");
                Require(sheets >= 1, "工作簿没有任何工作表");
                return $"xlsx entry=ok worksheets={sheets}";
            });
        }

        if (Wanted("word.image"))
        {
            await OpAsync("word.image", new[] { docx }, new Dictionary<string, string>
            {
                ["extension"] = ".png",
                ["dpi"] = "150"
            }, result =>
            {
                List<string> images = result.OutputFiles
                    .Where(p => p.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                Require(images.Count >= 1, "未产出任何 PNG");
                Require(images.Count == 3, $"页图 {images.Count} != 源文档 3 页");
                foreach (string image in images)
                {
                    Require(FileProbe.IsPng(image), "输出缺少 PNG 签名：" + Rel(image));
                    (int w, int h) = FileProbe.PngSize(image);
                    Require(w > 0 && h > 0, "PNG 尺寸解析失败：" + Rel(image));
                }

                (int firstWidth, int firstHeight) = FileProbe.PngSize(images[0]);
                return $"png count={images.Count} first={firstWidth}x{firstHeight}";
            });
        }

        if (Wanted("pdf.compress"))
        {
            await OpAsync("pdf.compress", new[] { pdf }, new Dictionary<string, string>
            {
                ["quality"] = "中",
                ["maxDpi"] = "150"
            }, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages == 3, $"页数 {pages} != 源 3 页");
                long sourceBytes = FileProbe.Size(pdf);
                long outputBytes = FileProbe.Size(output);
                return $"pages={pages} {Mb(sourceBytes)} -> {Mb(outputBytes)} ({outputBytes * 100.0 / sourceBytes:F1}%)";
            });
        }

        if (Wanted("pdf.watermark.add"))
        {
            // Deliberately no fontName: the UI pre-fills the schema default ("宋体"), so this is the
            // out-of-the-box path. A localized font name that Aspose.Pdf cannot resolve makes the
            // whole feature fail here - that failure is the finding, not a harness artifact.
            await OpAsync("pdf.watermark.add", new[] { pdf }, new Dictionary<string, string>
            {
                ["text"] = "QA 水印",
                ["opacity"] = "0.35",
                ["rotate"] = "45",
                ["repeat"] = "true"
            }, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages == 3, $"页数 {pages} != 源 3 页");
                long sourceBytes = FileProbe.Size(pdf);
                long outputBytes = FileProbe.Size(output);
                Require(outputBytes != sourceBytes, "输出字节数与源完全一致，水印未写入");
                string text = PdfTextProbe.Extract(output);
                bool textFound = text.Contains("QA", StringComparison.Ordinal);
                return $"pages={pages} {Mb(sourceBytes)} -> {Mb(outputBytes)} watermarkTextFound={textFound}";
            });

            // Diagnostic companion: same feature, but with a font name Aspose.Pdf can resolve.
            // Not part of the pass/fail matrix - it only shows whether the pipeline itself works.
            bool resolvableFont = ProbeWatermarkWithFont(pdf, "SimSun");
            Console.WriteLine($"    ↳ 诊断：fontName=SimSun 时 {(resolvableFont ? "成功" : "仍然失败")}");
        }

        if (Wanted("pdf.split"))
        {
            await OpAsync("pdf.split", new[] { pdf }, new Dictionary<string, string>
            {
                ["splitMode"] = "每页拆分"
            }, result =>
            {
                List<string> parts = result.OutputFiles
                    .Where(p => p.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                Require(parts.Count == 3, $"拆分文件数 {parts.Count} != 3");
                foreach (string part in parts)
                {
                    Require(FileProbe.IsPdf(part), "分片缺少 PDF 签名：" + Rel(part));
                    int pages = FileProbe.PdfPageCount(part);
                    Require(pages == 1, $"分片页数 {pages} != 1：" + Path.GetFileName(part));
                }

                return $"parts={parts.Count} each pages=1";
            });
        }

        if (Wanted("word.merge"))
        {
            await OpAsync("word.merge", new[] { docx, docx2 }, null, result =>
            {
                string output = Output(result, ".docx");
                Require(FileProbe.IsOfficePackage(output, "word/document.xml"),
                    "不是合法 DOCX（缺 word/document.xml）");
                string xml = FileProbe.ReadZipText(output, "word/document.xml");
                Require(xml.Contains("第三页内容", StringComparison.Ordinal),
                    "合并结果缺少第一个输入的内容");
                Require(xml.Contains("合并附录", StringComparison.Ordinal),
                    "合并结果缺少第二个输入的内容");
                return "docx contains text from both inputs";
            });
        }

        if (Wanted("word.replace"))
        {
            await OpAsync("word.replace", new[] { docx }, new Dictionary<string, string>
            {
                ["pairCount"] = "1",
                ["pair0.find"] = "{{NAME}}",
                ["pair0.replace"] = "QA_REPLACED"
            }, result =>
            {
                string output = Output(result, ".docx");
                Require(FileProbe.IsOfficePackage(output, "word/document.xml"),
                    "不是合法 DOCX（缺 word/document.xml）");
                string xml = FileProbe.ReadZipText(output, "word/document.xml");
                Require(!xml.Contains("{{NAME}}", StringComparison.Ordinal), "占位符 {{NAME}} 仍存在");
                Require(xml.Contains("QA_REPLACED", StringComparison.Ordinal), "替换文本未写入");
                bool otherKept = xml.Contains("{{DATE}}", StringComparison.Ordinal);
                return $"replaced=ok untouchedPlaceholder_kept={otherKept}";
            });
        }

        if (Wanted("html.pdf"))
        {
            await OpAsync("html.pdf", new[] { html }, new Dictionary<string, string>
            {
                ["waitResources"] = "true",
                ["paper"] = "A4",
                ["timeout"] = "45",
                ["navTimeout"] = "45"
            }, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages >= 1, $"页数 {pages} < 1");
                Require(FileProbe.Size(output) > 2048, "PDF 过小，可能只渲染了空白页");
                string text = PdfTextProbe.Extract(output);
                return $"pdf pages={pages} extractedChars={text.Length}";
            });
        }

        if (Wanted("image.compress"))
        {
            await OpAsync("image.compress", new[] { png }, new Dictionary<string, string>
            {
                ["format"] = "PNG",
                ["quality"] = "75"
            }, result =>
            {
                string output = Output(result, ".png");
                Require(FileProbe.IsPng(output), "输出缺少 PNG 签名");
                (int w, int h) = FileProbe.PngSize(output);
                Require(w > 0 && h > 0, "PNG 尺寸解析失败");
                long sourceBytes = FileProbe.Size(png);
                long outputBytes = FileProbe.Size(output);
                return $"png {w}x{h} {Mb(sourceBytes)} -> {Mb(outputBytes)} ({outputBytes * 100.0 / sourceBytes:F1}%)";
            });
        }

        if (Wanted("code.barcode"))
        {
            CheckBarcode();
        }

        if (Wanted("exam.circle"))
        {
            CheckExamCircle();
        }
    }

    private static void CheckBarcode()
    {
        const string feature = "code.barcode";
        string outDir = Path.Combine(_outRoot, feature);
        Directory.CreateDirectory(outDir);
        Stopwatch watch = Stopwatch.StartNew();
        try
        {
            IBarcodeService barcode = _provider!.GetRequiredService<IBarcodeService>();
            byte[] qr = barcode.CreatePng("https://example.com/qa", new BarcodeRenderOptions
            {
                Symbology = "QR",
                ModuleSize = 6,
                Caption = "QA",
                CaptionPosition = "below"
            });
            byte[] code128 = barcode.CreatePng("QA-1234567890", "Code128", 3);

            string qrPath = Path.Combine(outDir, "barcode_qr.png");
            string codePath = Path.Combine(outDir, "barcode_code128.png");
            File.WriteAllBytes(qrPath, qr);
            File.WriteAllBytes(codePath, code128);
            watch.Stop();

            Require(FileProbe.IsPng(qrPath), "二维码缺少 PNG 签名");
            Require(FileProbe.IsPng(codePath), "Code128 缺少 PNG 签名");
            (int qw, int qh) = FileProbe.PngSize(qrPath);
            (int cw, int ch) = FileProbe.PngSize(codePath);
            Require(qw > 60 && qh > 60, $"二维码尺寸异常：{qw}x{qh}");
            Require(cw > 40 && ch > 20, $"条码尺寸异常：{cw}x{ch}");

            long bytes = FileProbe.Size(qrPath) + FileProbe.Size(codePath);
            Results.Add(new CheckResult(feature,
                "(内存生成：QR + Code128)",
                Rel(qrPath) + "; " + Rel(codePath),
                bytes, watch.Elapsed.TotalMilliseconds, true,
                $"qr={qw}x{qh} {qr.Length}B, code128={cw}x{ch} {code128.Length}B", true));
        }
        catch (CheckException ex)
        {
            watch.Stop();
            Results.Add(new CheckResult(feature, "(内存生成：QR + Code128)", "(none)", 0,
                watch.Elapsed.TotalMilliseconds, false, ex.Message, true));
        }
        catch (Exception ex)
        {
            watch.Stop();
            Results.Add(new CheckResult(feature, "(内存生成：QR + Code128)", "(none)", 0,
                watch.Elapsed.TotalMilliseconds, false, $"{ex.GetType().Name}: {ex.Message}", true));
        }
    }

    private static void CheckExamCircle()
    {
        const string feature = "exam.circle";
        string outDir = Path.Combine(_outRoot, feature);
        Directory.CreateDirectory(outDir);
        Stopwatch watch = Stopwatch.StartNew();

        string image = Fixture("exam", "circle_sheet.png");
        string specPath = Fixture("exam", "circle_sheet.json");
        try
        {
            Require(File.Exists(image), "夹具缺失：" + Rel(image));
            Require(File.Exists(specPath), "夹具缺失：" + Rel(specPath));

            using JsonDocument spec = JsonDocument.Parse(File.ReadAllText(specPath, Encoding.UTF8));
            JsonElement template = spec.RootElement.GetProperty("template");
            var rect = new ExamCircleRect
            {
                Left = template.GetProperty("left").GetInt32(),
                Top = template.GetProperty("top").GetInt32(),
                Width = template.GetProperty("width").GetInt32(),
                Height = template.GetProperty("height").GetInt32()
            };
            int expected = spec.RootElement.GetProperty("expectedMatches").GetInt32();

            IExamCircleFixService service = _provider!.GetRequiredService<IExamCircleFixService>();

            (int width, int height) = service.GetImageSize(image);
            Require(width > 0 && height > 0, $"图片尺寸读取失败：{width}x{height}");

            IReadOnlyList<ExamCircleMatch> matches = service.Match(image, rect, null, 0.8, 100);
            Require(matches.Count >= expected - 3,
                $"匹配数 {matches.Count} 低于期望 {expected}（容差 3）");
            double minSimilarity = matches.Min(m => m.Similarity);
            Require(minSimilarity >= 0.8, $"存在相似度 {minSimilarity:F3} < 0.8 的匹配");

            string outputFile = Path.Combine(outDir, "circle_fixed.png");
            string fixedPath = service.Fix(new ExamCircleFixRequest
            {
                SourceImage = image,
                TemplateRect = rect,
                SimilarityThreshold = 0.8,
                MaxMatches = 100,
                OutputFile = outputFile
            });

            watch.Stop();
            Require(File.Exists(fixedPath), "修复输出不存在：" + fixedPath);
            Require(FileProbe.IsPng(fixedPath), "修复输出缺少 PNG 签名");
            (int fw, int fh) = FileProbe.PngSize(fixedPath);
            Require(fw == width && fh == height, $"修复后尺寸变化：{fw}x{fh} != {width}x{height}");

            Results.Add(new CheckResult(feature,
                Rel(image) + $" (template {rect.Width}x{rect.Height})",
                Rel(fixedPath),
                FileProbe.Size(fixedPath), watch.Elapsed.TotalMilliseconds, true,
                $"image={width}x{height} matches={matches.Count}/{expected} minSim={minSimilarity:F3}", true));
        }
        catch (CheckException ex)
        {
            watch.Stop();
            Results.Add(new CheckResult(feature, Rel(image), "(none)", 0,
                watch.Elapsed.TotalMilliseconds, false, ex.Message, true));
        }
        catch (Exception ex)
        {
            watch.Stop();
            Results.Add(new CheckResult(feature, Rel(image), "(none)", 0,
                watch.Elapsed.TotalMilliseconds, false, $"{ex.GetType().Name}: {ex.Message}", true));
        }
    }

    // ---------------------------------------------------------------- bonus checks

    private static async Task RunBonusChecksAsync()
    {
        Console.WriteLine();
        Console.WriteLine("== 追加观察项（非 16 项清单，仅记录）==");

        string docx = Fixture("word", "sample.docx");
        string pdf = Fixture("pdf", "sample.pdf");
        string epub = Fixture("epub", "sample.epub");

        await OpAsync("word.epub", new[] { docx }, null, result =>
        {
            string output = Output(result, ".epub");
            Require(FileProbe.IsZip(output), "EPUB 不是 ZIP 容器");
            Require(FileProbe.ZipHasEntry(output, "mimetype"), "EPUB 缺 mimetype 条目");
            return "epub container=ok";
        }, required: false);

        // Evaluation mode caps a document at 4 elements, so merging the 3-page sample twice
        // cannot be exercised; 2+2 keeps the merge logic verifiable inside that cap.
        string mergeA = Fixture("pdf", "merge_a.pdf");
        string mergeB = Fixture("pdf", "merge_b.pdf");
        if (File.Exists(mergeA) && File.Exists(mergeB))
        {
            await OpAsync("pdf.merge", new[] { mergeA, mergeB }, new Dictionary<string, string>
            {
                ["outputName"] = "merged.pdf"
            }, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                int pages = FileProbe.PdfPageCount(output);
                Require(pages == 4, $"合并页数 {pages} != 2+2");
                string text = PdfTextProbe.Extract(output, 4);
                string firstPage = PdfTextProbe.Extract(output, 1);
                bool firstHalf = text.Contains("MERGE-A", StringComparison.Ordinal);
                bool secondHalf = text.Contains("MERGE-B", StringComparison.Ordinal);
                Require(firstHalf && secondHalf,
                    $"合并结果缺少任一来源的内容（提取 {text.Length} 字符）");
                Require(firstPage.Contains("MERGE-A", StringComparison.Ordinal),
                    "第 1 页不是第一个输入的内容，合并顺序不对");
                return $"pdf pages={pages} (2+2) bothSourcesKept=True order=AB";
            }, required: false);

            (bool overLimitOk, string overLimitMessage, _) = TryRun(
                "pdf.merge",
                "pdf.merge.over-4-elements",
                [pdf, pdf],
                new Dictionary<string, string> { ["outputName"] = "merged6.pdf" });
            Console.WriteLine(
                $"    ↳ 诊断：3+3 页合并（评估限制演示）success={overLimitOk} message={overLimitMessage}");
        }

        if (File.Exists(epub))
        {
            await OpAsync("epub.word", new[] { epub }, null, result =>
            {
                string output = Output(result, ".docx");
                Require(FileProbe.IsOfficePackage(output, "word/document.xml"), "不是合法 DOCX");
                return "docx entry=ok";
            }, required: false);

            await OpAsync("epub.pdf", new[] { epub }, null, result =>
            {
                string output = Output(result, ".pdf");
                Require(FileProbe.IsPdf(output), "输出缺少 %PDF- 签名");
                return $"pdf pages={FileProbe.PdfPageCount(output)}";
            }, required: false);
        }
    }

    // ---------------------------------------------------------------- reporting

    private static bool Wanted(string featureId)
        => _filter is null || _filter.Contains(featureId) || _filter.Any(f => f.EndsWith('*') && featureId.StartsWith(f[..^1], StringComparison.Ordinal));

    private static int CountOf(string text, string token)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(token, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += token.Length;
        }

        return count;
    }

    private static int CountRegex(string text, string pattern)
        => System.Text.RegularExpressions.Regex.Matches(text, pattern).Count;

    private static int CountZipEntries(string path, string prefix)
    {
        if (!FileProbe.IsZip(path))
        {
            return 0;
        }

        using System.IO.Compression.ZipArchive archive = System.IO.Compression.ZipFile.OpenRead(path);
        return archive.Entries.Count(e => e.FullName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                                          && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
    }

    private static int PrintSummaryAndWriteArtifacts()
    {
        Console.WriteLine();
        Console.WriteLine("== 逐项结果 ==");

        List<CheckResult> required = Results.Where(r => r.Group != ExtendedGroup && r.Required).ToList();
        List<CheckResult> extended = Results.Where(r => r.Group == ExtendedGroup).ToList();

        foreach (CheckResult row in required)
        {
            Console.WriteLine(
                $"[{Status(row)}] {row.Feature} | in={row.Input} | out={row.Outputs} | "
                + $"{Mb(row.Bytes)} | {row.ElapsedMs:F0} ms | {row.Detail}");
        }

        if (extended.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("== 扩展项（第二轮：姓名/考评/工具/系统）==");
            foreach (CheckResult row in extended)
            {
                Console.WriteLine(
                    $"[{Status(row)}] {row.Feature} | in={row.Input} | out={row.Outputs} | "
                    + $"{Mb(row.Bytes)} | {row.ElapsedMs:F0} ms | {row.Detail}");
            }
        }

        List<CheckResult> bonus = Results.Where(r => r.Group != ExtendedGroup && !r.Required).ToList();
        if (bonus.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("== 追加观察项 ==");
            foreach (CheckResult row in bonus)
            {
                Console.WriteLine(
                    $"[{Status(row)}] {row.Feature} | in={row.Input} | out={row.Outputs} | "
                    + $"{Mb(row.Bytes)} | {row.ElapsedMs:F0} ms | {row.Detail}");
            }
        }

        // SKIP 行既不算 PASS 也不算 FAIL：用于「库能力/Schema 承诺不支持、已上报缺陷」这类
        // 明确无法验证的项，避免把未验证的东西算成通过，也避免它污染退出码。
        int requiredFail = required.Count(r => !r.Pass && !r.Skipped);
        int extendedFail = extended.Count(r => !r.Pass && !r.Skipped);
        int bonusFail = bonus.Count(r => !r.Pass && !r.Skipped);
        int requiredSkip = required.Count(r => r.Skipped);
        int extendedSkip = extended.Count(r => r.Skipped);
        int bonusSkip = bonus.Count(r => r.Skipped);

        Console.WriteLine();
        Console.WriteLine("== 汇总 ==");
        Console.WriteLine($"  必测 {required.Count} 项：PASS {required.Count - requiredFail - requiredSkip} / FAIL {requiredFail}"
                          + (requiredSkip > 0 ? $" / SKIP {requiredSkip}" : string.Empty));
        if (extended.Count > 0)
        {
            Console.WriteLine($"  扩展 {extended.Count} 项：PASS {extended.Count - extendedFail - extendedSkip} / FAIL {extendedFail}"
                              + (extendedSkip > 0 ? $" / SKIP {extendedSkip}" : string.Empty));
        }

        if (bonus.Count > 0)
        {
            Console.WriteLine($"  追加 {bonus.Count} 项：PASS {bonus.Count - bonusFail - bonusSkip} / FAIL {bonusFail}"
                              + (bonusSkip > 0 ? $" / SKIP {bonusSkip}" : string.Empty));
        }

        Console.WriteLine($"  退出码 = 失败数 = {requiredFail + extendedFail + bonusFail}");

        WriteArtifacts(requiredFail, extendedFail, bonusFail);
        return requiredFail + extendedFail + bonusFail;
    }

    private static string Status(CheckResult row) => row.Skipped ? "SKIP" : row.Pass ? "PASS" : "FAIL";

    private static void WriteArtifacts(int requiredFail, int extendedFail, int bonusFail)
    {
        Directory.CreateDirectory(_outRoot);

        var payload = new
        {
            generatedAt = DateTimeOffset.Now.ToString("o"),
            repoRoot = _repoRoot,
            requiredFail,
            extendedFail,
            bonusFail,
            rows = Results.Select(r => new
            {
                r.Feature,
                r.Required,
                group = GroupLabel(r),
                status = Status(r),
                r.Input,
                r.Outputs,
                bytes = r.Bytes,
                elapsedMs = Math.Round(r.ElapsedMs, 1),
                r.Detail
            })
        };

        string jsonPath = Path.Combine(_outRoot, "result.json");
        File.WriteAllText(jsonPath,
            JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }),
            new UTF8Encoding(false));

        var markdown = new StringBuilder();
        markdown.AppendLine("| 功能 | 类型 | 结果 | 输入 | 输出 | 字节 | 耗时 ms | 详情 |");
        markdown.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
        foreach (CheckResult row in Results)
        {
            markdown.AppendLine(
                $"| {row.Feature} | {GroupLabel(row)} | {Status(row)} | {row.Input} | "
                + $"{row.Outputs} | {row.Bytes} | {row.ElapsedMs:F0} | {row.Detail} |");
        }

        string mdPath = Path.Combine(_outRoot, "result.md");
        File.WriteAllText(mdPath, markdown.ToString(), new UTF8Encoding(false));

        Console.WriteLine($"  明细文件：{Rel(jsonPath)} , {Rel(mdPath)}");
    }

    /// <summary>Rows of the second round (name/eval/tool/system) are required too, just reported apart.</summary>
    private const string ExtendedGroup = "ext";

    private static string GroupLabel(CheckResult row)
        => row.Group == ExtendedGroup ? "扩展" : row.Required ? "必测" : "追加";

}
