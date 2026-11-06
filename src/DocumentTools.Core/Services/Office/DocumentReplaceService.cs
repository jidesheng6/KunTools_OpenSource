using System.Text.RegularExpressions;
using Aspose.Cells;
using Aspose.Words;
using Aspose.Words.Replacing;
using DocumentTools.Core.Common;
using OoxmlSaveOptions = Aspose.Words.Saving.OoxmlSaveOptions;

namespace DocumentTools.Core.Services.Office;

/// <summary>Port of legacy <c>Util.ReplaceDocumentContent</c> without UI.</summary>
public sealed class DocumentReplaceService : IDocumentReplaceService
{
    public void ReplaceWord(string sourceFile, string savedFile, DocumentReplaceOptions options)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));
        ArgumentNullException.ThrowIfNull(options);

        if (options.Pairs.Count == 0)
        {
            throw new ArgumentException("至少需要一组查找/替换。", nameof(options));
        }

        var load = new Aspose.Words.Loading.LoadOptions
        {
            ConvertMetafilesToPng = true,
            MswVersion = Aspose.Words.Settings.MsWordVersion.Word2010
        };

        var doc = new Document(sourceFile, load);
        var find = new FindReplaceOptions
        {
            IgnoreFootnotes = options.IgnoreFootnotes,
            MatchCase = options.MatchCase,
            FindWholeWordsOnly = options.FindWholeWord,
            IgnoreDeleted = options.IgnoreDeleted,
            IgnoreFieldCodes = options.IgnoreFieldCodes,
            IgnoreFields = options.IgnoreFields,
            IgnoreInserted = options.IgnoreInserted,
            IgnoreShapes = options.IgnoreShapes,
            UseSubstitutions = options.UseRegexSubstitutions
        };

        // legacy Util.cs:5412-5416 —— 只有勾了 UseNewStyle 才把替换进来的文字刷成新样式；
        // 逐项判空，让「只改字号不改字体」这种组合也能表达（legacy 用固定默认值 SimSun/12 兜底）。
        if (options.UseNewStyle)
        {
            if (!string.IsNullOrWhiteSpace(options.FontName))
            {
                find.ApplyFont.Name = options.FontName.Trim();
            }

            if (options.FontSize > 0)
            {
                find.ApplyFont.Size = options.FontSize;
            }

            if (options.Bold is bool bold)
            {
                find.ApplyFont.Bold = bold;
            }
        }

        foreach ((string findText, string replaceText) in options.Pairs)
        {
            if (string.IsNullOrEmpty(findText))
            {
                continue;
            }

            if (options.UseRegex)
            {
                var regex = new Regex(findText, RegexOptions.Multiline);
                doc.Range.Replace(regex, replaceText ?? string.Empty, find);
            }
            else
            {
                doc.Range.Replace(findText, replaceText ?? string.Empty, find);
            }
        }

        doc.Save(savedFile, new OoxmlSaveOptions { SaveFormat = Aspose.Words.SaveFormat.Docx });
    }

    public void ReplaceExcel(string sourceFile, string savedFile, DocumentReplaceOptions options)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(savedFile, nameof(savedFile));
        ArgumentNullException.ThrowIfNull(options);

        if (options.Pairs.Count == 0)
        {
            throw new ArgumentException("至少需要一组查找/替换。", nameof(options));
        }

        using var workbook = new Workbook(sourceFile);
        var replaceOptions = new ReplaceOptions
        {
            MatchEntireCellContents = options.ExcelMatchEntireCell || options.FindWholeWord,
            CaseSensitive = options.MatchCase,
            RegexKey = options.UseRegex
        };

        foreach ((string findText, string replaceText) in options.Pairs)
        {
            if (string.IsNullOrEmpty(findText))
            {
                continue;
            }

            workbook.Replace(findText, replaceText ?? string.Empty, replaceOptions);
        }

        if (options.ExcelStartRow > -1 && options.ExcelEndRow > -1)
        {
            // 倒序输入必须报错：旧实现把 totalRows 算成 0 以下就 continue，结果「什么都没删」却照常保存，
            // 用户以为删掉了（legacy 会把这个值直接传给 Aspose 抛错，属于同类缺陷的另一种形态）。
            if (options.ExcelEndRow < options.ExcelStartRow)
            {
                throw new ArgumentException(
                    $"结束行不能小于起始行（起始行 {options.ExcelStartRow}，结束行 {options.ExcelEndRow}）。",
                    nameof(options));
            }

            foreach (Worksheet sheet in workbook.Worksheets)
            {
                Cells cells = sheet.Cells;
                int firstRow = options.ExcelStartRow - 1;
                int lastExistingRow = cells.MaxRow;

                // 起始行已经超出该表最大行：这张表没有落在区间内的行，跳过（不能算「空操作成功」）。
                if (lastExistingRow < firstRow)
                {
                    continue;
                }

                // 结束行超出最大行时夹到最大行：否则 Aspose 会抛英文 CellsException（"row index should be ..."）。
                int endRow = Math.Min(options.ExcelEndRow - 1, lastExistingRow);
                int totalRows = endRow - firstRow + 1;
                if (totalRows <= 0)
                {
                    continue;
                }

                if (options.ExcelRemoveRows)
                {
                    // Aspose.Cells 的 DeleteRows 第二参是「要删除的行数」而不是「结束行下标」：
                    // 原写法把结束行当行数传，start=3/end=7 会删掉 6 行只剩 2 行（legacy Util.cs:5452 同源缺陷）。
                    cells.DeleteRows(firstRow, totalRows);
                }
                else
                {
                    // 原写法把末列硬编码成 255，超过 256 列的宽表清不到；改用实际末列。
                    int lastColumn = Math.Max(cells.MaxColumn, 0);
                    cells.ClearRange(firstRow, 0, endRow, lastColumn);
                }
            }
        }

        workbook.CalculateFormula();
        workbook.Save(savedFile, Aspose.Cells.SaveFormat.Xlsx);
    }
}
