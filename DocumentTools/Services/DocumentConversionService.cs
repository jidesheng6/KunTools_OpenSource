using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools.Services
{
    /// <summary>
    /// 文档转换服务：先将 UI.ConvertStart 的“遍历/分发”逻辑迁移到服务层，逐步解耦 UI.cs。
    /// 说明：当前仍调用 Util.* 作为底层实现（阶段4再拆 Util）。
    /// </summary>
    public class DocumentConversionService : IDocumentConversionService
    {
        private readonly IFormAccessor _formAccessor;

        public DocumentConversionService(IFormAccessor formAccessor)
        {
            _formAccessor = formAccessor;
        }

        public Task ConvertWord2PdfAsync(Word2PdfOptions options)
        {
            // 旧接口占位：后续再统一改造。
            throw new NotImplementedException("Word2Pdf 转换将在后续阶段实现（当前请走 ConvertStartAsync）。");
        }

        /// <summary>
        /// 从 UI.ConvertStart 迁移的通用转换入口。
        /// </summary>
         public Task ConvertStartAsync(
            ListView listViewControl,
            Label statusLabelControl,
            DocumentTools.ConvertFileType convertType,
            string extensionString,
            bool useSourcePath = false,
            Button callButton = null)
        {
            if (listViewControl == null) return Task.CompletedTask;

            return Task.Run(
                () =>
                {
                    if (listViewControl.Items.Count <= 0) return;

                    string tempSavePath = string.Empty;
                    string selectSavePath = "A";

                    if (!useSourcePath)
                    {
                        IEnumerable<string> selected = Util.ShowSelectDialog(false, true, string.Empty, string.Empty);
                        selectSavePath = selected.FirstOrDefault();
                    }

                    if (string.Compare(selectSavePath, default, StringComparison.Ordinal) == 0 ||
                        string.IsNullOrEmpty(selectSavePath))
                    {
                        return;
                    }

                    // 禁用按钮（如果传入）
                    try
                    {
                        if (callButton != null)
                        {
                            callButton.BeginInvoke(new Action(() => callButton.Enabled = false));
                        }
                    }
                    catch { }

                    for (int tempIndex = 0; tempIndex < listViewControl.Items.Count; tempIndex++)
                    {
                        string sourceFile = listViewControl.Items[tempIndex].Text;
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFile);
                        string fileDir = Path.GetDirectoryName(sourceFile);

                        if (useSourcePath)
                        {
                            tempSavePath = fileDir;
                        }
                        else
                        {
                            tempSavePath = selectSavePath;
                        }

                        string convertedFileName = Path.Combine(tempSavePath, fileNameWithoutExtension + extensionString);

                        try
                        {
                            // 状态更新
                            if (statusLabelControl != null)
                            {
                                statusLabelControl.BeginInvoke(new Action(() =>
                                {
                                    statusLabelControl.Text = $"[{tempIndex + 1}/{listViewControl.Items.Count}] {fileNameWithoutExtension}";
                                }));
                            }
                        }
                        catch { }

                        try
                        {
                            DispatchConvert(convertType, sourceFile, convertedFileName, extensionString, tempIndex);
                        }
                        catch
                        {
                            // 保持原行为：吞异常，避免中断批处理（后续可引入日志/错误收集）
                        }
                    }

                    // 结束后恢复按钮
                    try
                    {
                        if (callButton != null)
                        {
                            callButton.BeginInvoke(new Action(() => callButton.Enabled = true));
                        }
                    }
                    catch { }

                    try
                    {
                        if (statusLabelControl != null)
                        {
                            statusLabelControl.BeginInvoke(new Action(() => statusLabelControl.Text = "完成"));
                        }
                    }
                    catch { }
                });
        }

        private void DispatchConvert(DocumentTools.ConvertFileType convertType, string sourceFile, string convertedFileName, string extensionString, int tempIndex)
        {
            // 通过 IFormAccessor 读取参数，完全解耦 UI 层
            // 下一步会把这些参数逐步通过 IFormAccessor 暴露。
            switch (convertType)
            {
                case DocumentTools.ConvertFileType.Word2Pdf:
                    {
                        // 通过 IFormAccessor 读取转换参数，完全解耦 UI 层
                        var pdfComplianceSelectedValue = _formAccessor.GetConvertPdfComplianceSelectedValue();
                        var wordVersionSelectedText = _formAccessor.GetWord2PdfWordVersionSelectedText();
                        var needUpdateFields = _formAccessor.GetWord2PdfNeedUpdateFields();
                        var colorModeSelectedValue = _formAccessor.GetWord2PdfColorModeSelectedValue();
                        var embedFonts = _formAccessor.GetWord2PdfEmbedFonts();
                        var loadPassword = _formAccessor.GetWord2PdfLoadPassword();
                        var useLocalOffice = _formAccessor.GetWord2PdfUseLocalOffice();

                        int selectedPdfCompliance = (int)Enum.Parse(
                            typeof(Aspose.Words.Saving.PdfCompliance),
                            pdfComplianceSelectedValue);

                        Util.ConvertWord2Pdf(
                            sourceFile,
                            convertedFileName,
                            string.Empty, // tempSaveDocxPath - 临时保存路径，空字符串表示不使用
                            (Aspose.Words.Saving.PdfCompliance)selectedPdfCompliance,
                            (Aspose.Words.Settings.MsWordVersion)Enum.Parse(
                                typeof(Aspose.Words.Settings.MsWordVersion),
                                wordVersionSelectedText),
                            needUpdateFields,
                            (Aspose.Words.Saving.ColorMode)Enum.Parse(
                                typeof(Aspose.Words.Saving.ColorMode),
                                colorModeSelectedValue),
                            embedFonts,
                            loadPassword,
                            useLocalOffice);
                        break;
                    }
                case DocumentTools.ConvertFileType.Pdf2Word:
                    {
                        var useBullet = _formAccessor.GetPdf2WordUseBullet();
                        var useSubOcr = _formAccessor.GetPdf2WordUseSubOcr();
                        var modeSelectedValue = _formAccessor.GetPdf2WordRecognitionModeSelectedValue();
                        var loadPassword = _formAccessor.GetPdf2WordLoadPassword();

                        Util.ConvertPdf2Word(
                            sourceFile,
                            convertedFileName,
                            useBullet,
                            useSubOcr,
                            (Aspose.Pdf.DocSaveOptions.RecognitionMode)Enum.Parse(
                                typeof(Aspose.Pdf.DocSaveOptions.RecognitionMode),
                                modeSelectedValue),
                            loadPassword);
                        break;
                    }

                case DocumentTools.ConvertFileType.Excel2Pdf:
                    {
                        Util.ConvertExcel2Pdf(
                            sourceFile,
                            convertedFileName,
                            _formAccessor.GetExcel2PdfExportEachSheet(),
                            _formAccessor.GetExcel2PdfForceContentOnOnePage(),
                            _formAccessor.GetExcel2PdfHorPrint(),
                            _formAccessor.GetExcel2PdfVerPrint(),
                            _formAccessor.GetExcel2PdfPrintScaleRow(),
                            _formAccessor.GetExcel2PdfPrintScaleCol(),
                            _formAccessor.GetExcel2PdfPrintScalePercentEnable(),
                            _formAccessor.GetExcel2PdfPrintScalePercentNumber());
                        break;
                    }

                case DocumentTools.ConvertFileType.Word2Image:
                    {
                        Util.ConvertWord2Image(
                            sourceFile,
                            convertedFileName,
                            extensionString,
                            _formAccessor.GetWord2ImageSetGray(),
                            _formAccessor.GetWord2ImagePageExport(),
                            _formAccessor.GetWord2ImageDpiSelectedValue());
                        break;
                    }

                case DocumentTools.ConvertFileType.Pdf2Excel:
                    {
                        Util.ConvertPdf2Excel(
                            sourceFile,
                            convertedFileName,
                            _formAccessor.GetPdf2ExcelSheetMode());
                        break;
                    }

                case DocumentTools.ConvertFileType.WordReplace:
                    {
                        if (_formAccessor.GetWordReplaceFirstFindText() == string.Empty) break;

                        var tempFindReplaceList = _formAccessor.GetWordReplacePairs()
                            .Select(p => (p.Find, p.Replace))
                            .ToList();

                        List<(string, string)> finalFindReplaceList;

                        if (_formAccessor.GetWordReplaceOneToOneMode())
                        {
                            finalFindReplaceList = new List<(string, string)>();
                            if (tempIndex < tempFindReplaceList.Count)
                            {
                                finalFindReplaceList.Add(tempFindReplaceList[tempIndex]);
                            }
                        }
                        else
                        {
                            finalFindReplaceList = tempFindReplaceList;
                        }

                        Util.ReplaceDocumentContent(
                            DocumentsFileName: sourceFile,
                            SavedDoucmentFileName: convertedFileName,
                            RelaceDocumentType: "WORD",
                            FindReplaceTupleList: finalFindReplaceList,
                            IgnoreFootText: _formAccessor.GetWordReplaceIgnoreFootContent(),
                            NoIgnoreCase: _formAccessor.GetWordReplaceIgnoreCase(),
                            replace_word_fontName: ReplaceWordFontSetting.FontName,
                            replace_word_fontSize: ReplaceWordFontSetting.FontSize,
                            WordReplace_FindWholeWord: _formAccessor.GetWordReplaceMatchWholeWord(),
                            WordReplace_IgnoreDelete: _formAccessor.GetWordReplaceIgnoreDelete(),
                            WordReplace_IgnoreFiledCodes: _formAccessor.GetWordReplaceIgnoreFiledCodes(),
                            WordReplace_IgnoreFileds: _formAccessor.GetWordReplaceIgnoreFileds(),
                            WordReplace_IgnoreInserted: _formAccessor.GetWordReplaceIgnoreInsert(),
                            WordReplace_IgnoreShapes: _formAccessor.GetWordReplaceIgnoreShapes(),
                            UseRegexSub: _formAccessor.GetWordReplaceUseRegexSub());
                        break;
                    }

                default:
                    // 其他转换类型后续逐步补齐
                    break;
            }
        }
    }
}
