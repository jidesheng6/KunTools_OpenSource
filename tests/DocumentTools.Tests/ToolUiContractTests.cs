using Aspose.Cells;
using Aspose.Words;
using DocumentTools.Core.Services.Code;
using DocumentTools.Core.Services.Office;
using DocumentTools.Core.Services.Text;
using SkiaSharp;
using CellsWorkbook = Aspose.Cells.Workbook;
using WordsDocument = Aspose.Words.Document;

namespace DocumentTools.Tests;

/// <summary>
/// Pins the Core-side contract behind three Tools pages (task-25):
/// <c>name.vertical</c> option semantics, <c>code.barcode</c> appearance keys that must reach the
/// produced pixels, and <c>word.replace</c> / <c>excel.replace</c> option keys that must reach
/// Aspose. Each test names the UI key it protects so an App-side regression shows up here first.
/// </summary>
public sealed class ToolUiContractTests : IDisposable
{
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "dt-toolui-" + Guid.NewGuid().ToString("N"));

    public ToolUiContractTests() => Directory.CreateDirectory(_root);

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
            // A leftover temp directory is harmless; never fail on cleanup.
        }
        catch (UnauthorizedAccessException)
        {
            // Same as above.
        }
    }

    // ---------- name.vertical: doubleAlign / distinct / markDuplicate ----------

    [Fact]
    public void NameVertical_DoubleAlign_InsertsExactlyTwoHalfWidthSpaces()
    {
        var service = new NameVerticalService();

        NameVerticalResult result = service.Process(
            "\u5f20\u4e09\n\u6b27\u9633\u4fee",
            new NameVerticalOptions { DoubleAlign = true });

        Assert.Equal(["\u5f20  \u4e09", "\u6b27\u9633\u4fee"], result.Lines);
        Assert.Equal(4, result.Lines[0].Length);
        Assert.Equal(2, result.Lines[0].Count(c => c == ' '));
    }

    [Fact]
    public void NameVertical_DistinctTrue_SuppressesDuplicateIndicesThatMarkDuplicateNeeds()
    {
        var service = new NameVerticalService();

        // UI 的「文本去重」（distinct）在去重之后才算重复行，而「高亮重名」（markDuplicate）
        // 读的正是这份行号列表：两键同时打开时列表必然为空。这条测试把这个相互作用固定下来，
        // 免得以后有人把 distinct 默认值或执行顺序改掉却没人发现。
        NameVerticalResult withDistinct = service.Process(
            "\u5f20\u4e09\n\u674e\u56db\n\u5f20\u4e09",
            new NameVerticalOptions { Distinct = true, DoubleAlign = false });
        NameVerticalResult withoutDistinct = service.Process(
            "\u5f20\u4e09\n\u674e\u56db\n\u5f20\u4e09",
            new NameVerticalOptions { Distinct = false, DoubleAlign = false });

        Assert.Equal(2, withDistinct.Count);
        Assert.Empty(withDistinct.DuplicateLineIndices);

        Assert.Equal(3, withoutDistinct.Count);
        Assert.Equal([0, 2], withoutDistinct.DuplicateLineIndices);
    }

    [Fact]
    public void NameVertical_RemoveSquareAndReplace_AreBothConsumed()
    {
        var service = new NameVerticalService();

        NameVerticalResult result = service.Process(
            "\u5f20\u4e09\uff08\u7ec4\u957f\uff09\n\u674e\u56db-ab",
            new NameVerticalOptions
            {
                DoubleAlign = false,
                RemoveSquare = true,
                ReplaceEnabled = true,
                ReplaceSearch = "-ab",
                ReplaceTarget = "-\u7532"
            });

        // 过滤链顺序是「去空白 → 去数字 → 去括号 → 文本替换」：括号里的内容先被 RemoveSquare 摘掉，
        // 替换只作用于剩下的文本，所以两键都必须独立生效。
        Assert.Equal(["\u5f20\u4e09", "\u674e\u56db-\u7532"], result.Lines);
    }

    // ---------- code.barcode: sizeMode / width / height / rotate / caption / logo / dpi ----------

    [Fact]
    public void Barcode_FixedSize_UsesRequestedWidthAndAutoHeight()
    {
        var service = new BarcodeService();

        byte[] png = service.CreatePng(
            "DOC-2026",
            new BarcodeRenderOptions
            {
                Symbology = "Code39",
                ModuleSize = 4,
                SizeMode = "fixed",
                BarcodeWidth = 300,
                BarcodeHeight = 0
            });

        using SKBitmap bitmap = SKBitmap.Decode(png);
        Assert.Equal(300, bitmap.Width);
        Assert.True(bitmap.Height > 0);
    }

    [Fact]
    public void Barcode_Rotate90_SwapsProducedDimensions()
    {
        var service = new BarcodeService();

        using SKBitmap upright = SKBitmap.Decode(service.CreatePng("DOC-2026", SizedOptions(rotate: 0)));
        using SKBitmap rotated = SKBitmap.Decode(service.CreatePng("DOC-2026", SizedOptions(rotate: 90)));

        Assert.Equal(300, upright.Width);
        Assert.Equal(120, upright.Height);
        Assert.Equal(upright.Height, rotated.Width);
        Assert.Equal(upright.Width, rotated.Height);
    }

    [Fact]
    public void Barcode_CaptionAndLogo_ReachTheRenderedPixels()
    {
        var service = new BarcodeService();
        var plain = new BarcodeRenderOptions { Symbology = "Code39", ModuleSize = 4 };

        using SKBitmap withoutCaption = SKBitmap.Decode(service.CreatePng("DOC-2026", plain));
        using SKBitmap withCaption = SKBitmap.Decode(service.CreatePng("DOC-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 4,
            Caption = "ASSET-NO",
            CaptionPosition = "above",
            CaptionFontSize = 18
        }));

        // 标题会占掉一行高度：宽度不变、高度变大，说明 captionPosition/captionFontSize 真的进了渲染。
        Assert.Equal(withoutCaption.Width, withCaption.Width);
        Assert.True(
            withCaption.Height > withoutCaption.Height,
            $"标题应增加高度：无标题 {withoutCaption.Height}，有标题 {withCaption.Height}");

        string logo = Path.Combine(_root, "logo.png");
        File.WriteAllBytes(logo, CreateSolidPng(24, 24, SKColors.Red));

        byte[] withLogo = service.CreatePng("DOC-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 4,
            LogoFile = logo
        });
        byte[] withoutLogo = service.CreatePng("DOC-2026", plain);

        // LOGO 走 SkiaSharp 叠加：内容变了、尺寸不变，证明 LogoFile 不是死参数。
        Assert.NotEqual(Convert.ToHexString(withoutLogo), Convert.ToHexString(withLogo));
        using SKBitmap logoBitmap = SKBitmap.Decode(withLogo);
        Assert.Equal(withoutCaption.Width, logoBitmap.Width);
    }

    [Fact]
    public void Barcode_ResolutionDpi_ScalesRenderedPixelSize()
    {
        var service = new BarcodeService();

        using SKBitmap at96 = SKBitmap.Decode(service.CreatePng("DOC-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 4,
            ResolutionDpi = 96
        }));
        using SKBitmap at300 = SKBitmap.Decode(service.CreatePng("DOC-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 4,
            ResolutionDpi = 300
        }));

        // 分辨率决定物理尺寸换算成的像素数：300dpi 出图必须明显更宽，否则 dpi 是死参数。
        Assert.True(
            at300.Width > at96.Width,
            $"300dpi 应更宽：96dpi={at96.Width}，300dpi={at300.Width}");
    }

    [Fact]
    public void Barcode_CaptionFontSize_ChangesRenderedCaptionHeight()
    {
        var service = new BarcodeService();

        using SKBitmap small = SKBitmap.Decode(service.CreatePng("DOC-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 4,
            Caption = "ASSET-NO",
            CaptionPosition = "below",
            CaptionFontSize = 8
        }));
        using SKBitmap large = SKBitmap.Decode(service.CreatePng("DOC-2026", new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 4,
            Caption = "ASSET-NO",
            CaptionPosition = "below",
            CaptionFontSize = 28
        }));

        // 字号直接决定标题块占用的高度，因此「标题字号」不是死参数。
        Assert.True(
            large.Height > small.Height,
            $"28pt 标题应比 8pt 更高：8pt={small.Height}，28pt={large.Height}");
    }

    // ---------- word.replace: ignoreFootnotes / regexSub ----------

    [Fact]
    public void WordReplace_IgnoreFootnotes_LeavesFootnoteTextUntouched()
    {
        string input = Path.Combine(_root, "source.docx");
        string kept = Path.Combine(_root, "kept.docx");
        string replaced = Path.Combine(_root, "replaced.docx");

        WordsDocument source = CreateDocumentWithFootnote("BODY-A", "NOTE-B");
        source.Save(input);

        var service = new DocumentReplaceService();
        service.ReplaceWord(input, kept, new DocumentReplaceOptions
        {
            Pairs = [("B", "C")],
            IgnoreFootnotes = true
        });
        service.ReplaceWord(input, replaced, new DocumentReplaceOptions
        {
            Pairs = [("B", "C")],
            IgnoreFootnotes = false
        });

        string keptFootnotes = FootnoteText(kept);
        string replacedFootnotes = FootnoteText(replaced);

        Assert.Contains("NOTE-B", keptFootnotes, StringComparison.Ordinal);
        Assert.DoesNotContain("NOTE-C", keptFootnotes, StringComparison.Ordinal);

        // 关掉「忽略脚注」后同一处替换必须落进脚注，否则这个键就是死参数。
        Assert.Contains("NOTE-C", replacedFootnotes, StringComparison.Ordinal);
    }

    [Fact]
    public void WordReplace_RegexSubstitution_ExpandsCaptureGroupsOnlyWithTheFlag()
    {
        string input = Path.Combine(_root, "regex-source.docx");
        string sub = Path.Combine(_root, "regex-sub.docx");
        string literal = Path.Combine(_root, "regex-literal.docx");

        WordsDocument source = new();
        new DocumentBuilder(source).Write("NO-A-001-Z");
        source.Save(input);

        var service = new DocumentReplaceService();

        service.ReplaceWord(input, sub, new DocumentReplaceOptions
        {
            Pairs = [("A-([0-9]+)", "B-$1")],
            UseRegex = true,
            UseRegexSubstitutions = true
        });

        // UseSubstitutions 关掉时 $1 是字面量：这证明 regexSub 不是死参数。
        // ReplaceDocumentOperation 已改为只认 regexSub（legacy Util.cs:5461 同构），
        // 操作层回归见 WordReplaceOperation_RegexFlagAlone_KeepsDollarLiteral。
        service.ReplaceWord(input, literal, new DocumentReplaceOptions
        {
            Pairs = [("A-([0-9]+)", "B-$1")],
            UseRegex = true,
            UseRegexSubstitutions = false
        });

        Assert.Contains("NO-B-001-Z", BodyText(sub), StringComparison.Ordinal);
        Assert.Contains("NO-B-$1-Z", BodyText(literal), StringComparison.Ordinal);
    }

    [Fact]
    public async Task WordReplaceOperation_RegexFlagAlone_KeepsDollarLiteral()
    {
        // 回归 task-25 审计发现的接线缺陷：ReplaceDocumentOperation 曾把 regex 也映射成
        // UseRegexSubstitutions，于是勾「使用正则」就无法单独关闭捕获替换（legacy Util.cs:5461 只认 regexSub）。
        string input = Path.Combine(_root, "regex-wiring.docx");
        WordsDocument source = new();
        new DocumentBuilder(source).Write("NO-A-001-Z");
        source.Save(input);

        var operation = new DocumentTools.Core.Operations.ReplaceDocumentOperation(
            "word.replace",
            new DocumentReplaceService());

        DocumentTools.Core.Operations.DocumentOperationResult result = await operation.ExecuteAsync(
            new DocumentTools.Core.Operations.DocumentOperationRequest
            {
                InputFiles = [input],
                OutputDirectory = _root,
                Options = new Dictionary<string, string>
                {
                    ["pairs"] = "A-([0-9]+)=>B-$1",
                    ["regex"] = "true",
                    ["regexSub"] = "false"
                }
            });

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.OutputFiles);
        string produced = Assert.Single(result.OutputFiles!);
        Assert.Contains("NO-B-$1-Z", BodyText(produced), StringComparison.Ordinal);
    }

    [Fact]
    public void WordReplace_MatchCaseAndWholeWord_AreHonouredIndependently()
    {
        string input = Path.Combine(_root, "case-whole.docx");
        WordsDocument source = new();
        new DocumentBuilder(source).Write("ABC abc cat catalog");
        source.Save(input);

        var service = new DocumentReplaceService();

        // matchCase：只有大小写一致的 "abc" 应该被替换，"ABC" 必须原样保留。
        string caseOut = Path.Combine(_root, "case-out.docx");
        service.ReplaceWord(input, caseOut, new DocumentReplaceOptions
        {
            Pairs = [("abc", "MATCH")],
            MatchCase = true
        });
        string caseText = BodyText(caseOut);
        Assert.Contains("ABC MATCH cat catalog", caseText, StringComparison.Ordinal);

        // wholeWord：独立的 "cat" 被替换，而 "catalog" 里的 cat 不能动。
        string wholeOut = Path.Combine(_root, "whole-out.docx");
        service.ReplaceWord(input, wholeOut, new DocumentReplaceOptions
        {
            Pairs = [("cat", "DOG")],
            MatchCase = true,
            FindWholeWord = true
        });
        string wholeText = BodyText(wholeOut);
        Assert.Contains("DOG catalog", wholeText, StringComparison.Ordinal);
        Assert.DoesNotContain("DOGalog", wholeText, StringComparison.Ordinal);
    }

    // ---------- excel.replace: wholeWord / matchCase / row range ----------

    [Fact]
    public void ExcelReplace_WholeWord_RequiresTheWholeCellToMatch()
    {
        string input = Path.Combine(_root, "whole.xlsx");
        string output = Path.Combine(_root, "whole-out.xlsx");

        using (var workbook = new CellsWorkbook())
        {
            Cells cells = workbook.Worksheets[0].Cells;
            cells[0, 0].PutValue("abc");
            cells[1, 0].PutValue("xabcx");
            workbook.Save(input, Aspose.Cells.SaveFormat.Xlsx);
        }

        var service = new DocumentReplaceService();
        service.ReplaceExcel(input, output, new DocumentReplaceOptions
        {
            Pairs = [("abc", "X")],
            FindWholeWord = true,
            ExcelMatchEntireCell = true
        });

        using var result = new CellsWorkbook(output);
        Cells resultCells = result.Worksheets[0].Cells;

        Assert.Equal("X", resultCells[0, 0].Value?.ToString());
        Assert.Equal("xabcx", resultCells[1, 0].Value?.ToString());
    }

    [Fact]
    public void ExcelReplace_MatchCase_LeavesOtherCasingAlone()
    {
        string input = Path.Combine(_root, "case.xlsx");
        string output = Path.Combine(_root, "case-out.xlsx");

        using (var workbook = new CellsWorkbook())
        {
            Cells cells = workbook.Worksheets[0].Cells;
            cells[0, 0].PutValue("ABC");
            cells[1, 0].PutValue("abc");
            workbook.Save(input, Aspose.Cells.SaveFormat.Xlsx);
        }

        var service = new DocumentReplaceService();
        service.ReplaceExcel(input, output, new DocumentReplaceOptions
        {
            Pairs = [("abc", "X")],
            MatchCase = true
        });

        using var result = new CellsWorkbook(output);
        Cells resultCells = result.Worksheets[0].Cells;

        Assert.Equal("ABC", resultCells[0, 0].Value?.ToString());
        Assert.Equal("X", resultCells[1, 0].Value?.ToString());
    }

    [Fact]
    public void ExcelReplace_RemoveRows_DeletesExactlyTheRequestedRange()
    {
        string input = Path.Combine(_root, "rows.xlsx");
        string output = Path.Combine(_root, "rows-out.xlsx");

        CreateRowNumberedWorkbook(input, rows: 8);

        var service = new DocumentReplaceService();
        service.ReplaceExcel(input, output, new DocumentReplaceOptions
        {
            Pairs = [("NO-SUCH-VALUE", "x")],
            ExcelRemoveRows = true,
            ExcelStartRow = 3,
            ExcelEndRow = 7
        });

        using var workbook = new CellsWorkbook(output);
        Cells cells = workbook.Worksheets[0].Cells;

        // 修好后语义是「删第 3~7 行共 5 行」，剩 R1、R2、R8 三行（索引 0~2）。
        // 旧实现把结束行当 Aspose DeleteRows 的行数传下去，实际删 6 行只剩两行——本断言即那次缺陷的回归锁。
        Assert.Equal(2, cells.MaxRow);
        Assert.Equal("R1", cells[0, 0].Value?.ToString());
        Assert.Equal("R2", cells[1, 0].Value?.ToString());
        Assert.Equal("R8", cells[2, 0].Value?.ToString());
    }

    [Fact]
    public void ExcelReplace_ClearRange_KeepsRowsAndBlanksCells()
    {
        string input = Path.Combine(_root, "clear.xlsx");
        string output = Path.Combine(_root, "clear-out.xlsx");

        CreateRowNumberedWorkbook(input, rows: 6);

        var service = new DocumentReplaceService();
        service.ReplaceExcel(input, output, new DocumentReplaceOptions
        {
            Pairs = [("NO-SUCH-VALUE", "x")],
            ExcelRemoveRows = false,
            ExcelStartRow = 2,
            ExcelEndRow = 4
        });

        using var workbook = new CellsWorkbook(output);
        Cells cells = workbook.Worksheets[0].Cells;

        // ClearRange(startRow, startColumn, endRow, endColumn) 的第二、第四个参数是结束行列，
        // 所以「清空」分支的区间是对的：行数不变，第 2~4 行（索引 1~3）的单元格被清空。
        Assert.Equal(5, cells.MaxRow);
        Assert.Equal("R1", cells[0, 0].Value?.ToString());
        Assert.Null(cells[1, 0].Value);
        Assert.Null(cells[2, 0].Value);
        Assert.Null(cells[3, 0].Value);
        Assert.Equal("R5", cells[4, 0].Value?.ToString());
    }

    [Fact]
    public void ExcelReplace_ReversedRowRange_ThrowsActionableMessage()
    {
        string input = Path.Combine(_root, "reverse.xlsx");
        string output = Path.Combine(_root, "reverse-out.xlsx");

        CreateRowNumberedWorkbook(input, rows: 8);

        var service = new DocumentReplaceService();
        var error = Assert.Throws<ArgumentException>(() => service.ReplaceExcel(input, output, new DocumentReplaceOptions
        {
            Pairs = [("NO-SUCH-VALUE", "x")],
            ExcelRemoveRows = true,
            ExcelStartRow = 7,
            ExcelEndRow = 3
        }));

        // 旧实现把 totalRows 算成负数后 continue，什么都没删却照常保存，用户以为删掉了。
        Assert.Contains("结束行不能小于起始行", error.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(output));
    }

    [Fact]
    public void ExcelReplace_EndRowBeyondLastRow_DeletesOnlyExistingRows()
    {
        string input = Path.Combine(_root, "overflow.xlsx");
        string output = Path.Combine(_root, "overflow-out.xlsx");

        CreateRowNumberedWorkbook(input, rows: 8);

        var service = new DocumentReplaceService();
        service.ReplaceExcel(input, output, new DocumentReplaceOptions
        {
            Pairs = [("NO-SUCH-VALUE", "x")],
            ExcelRemoveRows = true,
            ExcelStartRow = 6,
            ExcelEndRow = 50
        });

        using var workbook = new CellsWorkbook(output);
        Cells cells = workbook.Worksheets[0].Cells;

        // 结束行越界时夹到该表最大行（否则 Aspose 抛英文 CellsException）：只剩 R1..R5。
        Assert.Equal(4, cells.MaxRow);
        Assert.Equal("R5", cells[4, 0].Value?.ToString());
    }

    private static BarcodeRenderOptions SizedOptions(int rotate) => new()
    {
        Symbology = "Code39",
        ModuleSize = 4,
        SizeMode = "fixed",
        BarcodeWidth = 300,
        BarcodeHeight = 120,
        Rotate = rotate
    };

    private static WordsDocument CreateDocumentWithFootnote(string body, string footnote)
    {
        WordsDocument doc = new();
        var builder = new DocumentBuilder(doc);
        builder.Write(body);
        builder.InsertFootnote(Aspose.Words.Notes.FootnoteType.Footnote, footnote);
        return doc;
    }

    private static string BodyText(string file)
        => new WordsDocument(file).GetText();

    private static string FootnoteText(string file)
    {
        WordsDocument doc = new(file);
        return string.Join(
            "|",
            doc.GetChildNodes(NodeType.Footnote, isDeep: true)
                .Cast<Node>()
                .Select(note => note.GetText()));
    }

    private static void CreateRowNumberedWorkbook(string file, int rows)
    {
        using var workbook = new CellsWorkbook();
        Cells cells = workbook.Worksheets[0].Cells;

        for (int row = 0; row < rows; row++)
        {
            cells[row, 0].PutValue("R" + (row + 1));
        }

        workbook.Save(file, Aspose.Cells.SaveFormat.Xlsx);
    }

    private static byte[] CreateSolidPng(int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(color);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
