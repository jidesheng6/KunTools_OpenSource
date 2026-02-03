using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace DocumentTools.Services
{
    /// <summary>
    /// IFormAccessor 的默认实现：封装对 BodyForm 控件/状态的访问。
    /// </summary>
    public class FormAccessor : IFormAccessor
    {
        private readonly BodyForm _form;

        public FormAccessor(BodyForm form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
        }

        public T FindControl<T>(string name) where T : Control
        {
            try
            {
                return _form.Controls.Find(name, true).FirstOrDefault() as T;
            }
            catch
            {
                return null;
            }
        }

        // ListView Accessors...
        public ListView GetWordFileListView() => FindControl<ListView>("WordFileListView");
        public ListView GetPdfFileListView() => FindControl<ListView>("PdfFileListView");
        public ListView GetExcelFileListView() => FindControl<ListView>("ExcelFileListView");
        public ListView GetCompressImageFileList() => FindControl<ListView>("CompressImageFileList");
        public ListView GetCompressPdfFileList() => FindControl<ListView>("CompressPdfFileList");
        public ListView GetConvertPdfComplianceFileList() => FindControl<ListView>("ConvertPdfComplianceFileList");
        public ListView GetConvertImage2PDFFileList() => FindControl<ListView>("ConvertImage2PDFFileList");
        public ListView GetDecodeMatrixWordListView() => FindControl<ListView>("DecodeMatrixWordListView");
        public ListView GetHtml2PdfFileList() => FindControl<ListView>("HTML2PDFFileList");
        public ListView GetIcoFileView() => FindControl<ListView>("IcoFileView");
        public ListView GetMergePdfFileView() => FindControl<ListView>("MergePdfFileView");
        public ListView GetMergeWordFileList() => FindControl<ListView>("MergeWordFileList");
        public ListView GetPdf2ExcelFileView() => FindControl<ListView>("PDF2ExcelFileView");
        public ListView GetPdf2PptFileList() => FindControl<ListView>("PDF2PPTFileList");
        public ListView GetPpt2PdfFileList() => FindControl<ListView>("PPT2PdfFileList");
        public ListView GetRemovePdfWaterMarkFileList() => FindControl<ListView>("RemovePdfWaterMarkFileList");
        public ListView GetSplitPdfFileList() => FindControl<ListView>("SplitPdfFileList");
        public ListView GetSplitWordFileList() => FindControl<ListView>("SplitWordFileList");
        public ListView GetPdfWaterMarkFileList() => FindControl<ListView>("PdfWaterMarkFileList");
        public ListView GetWebpFileList() => FindControl<ListView>("WebpFileList");
        public ListView GetWord2EpubFileList() => FindControl<ListView>("Word2EPUBFileList");
        public ListView GetWord2ExcelFileList() => FindControl<ListView>("Word2ExcelFileList");
        public ListView GetDocumentsListView() => FindControl<ListView>("DocumentsListView");
        public ListView GetReplaceWordFileList() => FindControl<ListView>("ReplaceWordFileList");
        public ListView GetSplitPptFilesList() => FindControl<ListView>("SplitPPTFilesList");
        public ListView GetEpub2PdfFileList() => FindControl<ListView>("EPUB2PDFFileList");
        public ListView GetEpub2WordFileList() => FindControl<ListView>("EPUB2WORDFileList");
        public ListView GetExcel2WordFileList() => FindControl<ListView>("Excel2WordFileList");
        public ListView GetReplaceExcelFileView() => FindControl<ListView>("ReplaceExcelFileView");

        // === Word2Pdf Parameters ===
        public string GetConvertPdfComplianceSelectedValue() => FindControl<ComboBox>("ConvertPdfComplianceVersion")?.SelectedValue?.ToString() ?? string.Empty;
        public string GetWord2PdfWordVersionSelectedText() => FindControl<ComboBox>("Word2PDFOptimizCombox")?.SelectedItem?.ToString() ?? string.Empty;
        public bool GetWord2PdfNeedUpdateFields() => FindControl<CheckBox>("word2pdf_need_update_fields")?.Checked ?? false;
        public string GetWord2PdfColorModeSelectedValue() => FindControl<ComboBox>("word2pdf_colorMode")?.SelectedValue?.ToString() ?? string.Empty;
        public bool GetWord2PdfEmbedFonts() => FindControl<CheckBox>("EmbedFonts")?.Checked ?? false;
        public string GetWord2PdfLoadPassword() => FindControl<TextBox>("WORD2PDF_LoadPassword")?.Text ?? string.Empty;
        public bool GetWord2PdfUseLocalOffice() => FindControl<CheckBox>("UseLocalOffice")?.Checked ?? false;

        // === Pdf2Word Parameters ===
        public bool GetPdf2WordUseBullet() => FindControl<CheckBox>("PDF2WORD_UseBullet")?.Checked ?? false;
        public bool GetPdf2WordUseSubOcr() => FindControl<CheckBox>("PDF2WORD_UseSUbOcr")?.Checked ?? false;
        public string GetPdf2WordRecognitionModeSelectedValue() => FindControl<ComboBox>("PDF2WORD_Mode")?.SelectedValue?.ToString() ?? string.Empty;
        public string GetPdf2WordLoadPassword() => FindControl<TextBox>("PDF2WORD_LoadPassword")?.Text ?? string.Empty;

        // === Excel2Pdf Parameters ===
        public bool GetExcel2PdfExportEachSheet() => FindControl<CheckBox>("ExportEachSheet")?.Checked ?? false;
        public bool GetExcel2PdfForceContentOnOnePage() => FindControl<CheckBox>("Excel2Pdf_ForceContentOnOnePage")?.Checked ?? false;
        public bool GetExcel2PdfHorPrint() => FindControl<CheckBox>("Excel2Pdf_HorPrint")?.Checked ?? false;
        public bool GetExcel2PdfVerPrint() => FindControl<CheckBox>("Excel2Pdf_VerPrint")?.Checked ?? false;
        public int GetExcel2PdfPrintScaleRow() => int.TryParse(FindControl<TextBox>("Excel2Pdf_PrintScale_Row")?.Text, out int v) ? v : 0;
        public int GetExcel2PdfPrintScaleCol() => int.TryParse(FindControl<TextBox>("Excel2Pdf_PrintScale_Col")?.Text, out int v) ? v : 0;
        public bool GetExcel2PdfPrintScalePercentEnable() => FindControl<CheckBox>("Excel2Pdf_PrintScalePercent_Enable")?.Checked ?? false;
        public int GetExcel2PdfPrintScalePercentNumber() => (int)(FindControl<NumericUpDown>("Excel2Pdf_PrintScalePercent_Number")?.Value ?? 0);

        // === Word2Image Parameters ===
        public bool GetWord2ImageSetGray() => FindControl<CheckBox>("SetGrayCheckBox")?.Checked ?? false;
        public bool GetWord2ImagePageExport() => FindControl<CheckBox>("PageExportCheck")?.Checked ?? false;
        public int GetWord2ImageDpiSelectedValue() => int.TryParse(FindControl<ComboBox>("DpiItemList")?.SelectedValue?.ToString(), out int v) ? v : 0;

        // === Pdf2Excel Parameters ===
        public bool GetPdf2ExcelSheetMode() => FindControl<CheckBox>("ExcelSheelMode")?.Checked ?? false;

        // === WordReplace Parameters ===
        public string GetWordReplaceFirstFindText() => FindControl<TextBox>("WordReplaceFindTextBox")?.Text ?? string.Empty;

        public IList<(string Find, string Replace)> GetWordReplacePairs()
        {
            var pairs = new List<(string, string)>();
            var findPanel = FindControl<Panel>("WordReplaceFindPanel");
            var replacePanel = FindControl<Panel>("WordReplaceReplacePanel");

            if (findPanel == null || replacePanel == null) return pairs;

            var findTextBoxes = findPanel.Controls.OfType<TextBox>().Where(x => !string.IsNullOrEmpty(x.Text)).ToList();
            var replaceTextBoxes = replacePanel.Controls.OfType<TextBox>().ToList();

            for (int i = 0; i < Math.Min(findTextBoxes.Count, replaceTextBoxes.Count); i++)
            {
                pairs.Add((findTextBoxes[i].Text, replaceTextBoxes[i].Text));
            }
            return pairs;
        }

        public bool GetWordReplaceOneToOneMode() => FindControl<CheckBox>("WordReplace_OneToOneMode")?.Checked ?? false;
        public bool GetWordReplaceIgnoreFootContent() => FindControl<CheckBox>("IgnoreFootContent")?.Checked ?? false;
        public bool GetWordReplaceIgnoreCase() => FindControl<CheckBox>("IgnoreCase")?.Checked ?? false;
        public bool GetWordReplaceMatchWholeWord() => FindControl<CheckBox>("WordReplace_MatchWholeWord")?.Checked ?? false;
        public bool GetWordReplaceIgnoreDelete() => FindControl<CheckBox>("WordReplace_IgnoreDelete")?.Checked ?? false;
        public bool GetWordReplaceIgnoreFiledCodes() => FindControl<CheckBox>("WordReplace_IgnoreFiledCodes")?.Checked ?? false;
        public bool GetWordReplaceIgnoreFileds() => FindControl<CheckBox>("WordReplace_IgnoreFileds")?.Checked ?? false;
        public bool GetWordReplaceIgnoreInsert() => FindControl<CheckBox>("WordReplace_IgnoreInsert")?.Checked ?? false;
        public bool GetWordReplaceIgnoreShapes() => FindControl<CheckBox>("WordReplace_IgnoreShapes")?.Checked ?? false;
        public bool GetWordReplaceUseRegexSub() => FindControl<CheckBox>("WordReplace_UseRegextSub")?.Checked ?? false;

        // Obsolete placeholders
        public string GetPDFComplianceValue() => string.Empty;
        public string GetWord2PDFOptimizValue() => string.Empty;

        // Additional UI Accessors for Util.cs
        public bool GetNameReplaceMode()
        {
            // 尝试通过FindControl查找，如果找不到则使用反射访问字段
            var checkbox = FindControl<CheckBox>("NameReplaceMode");
            if (checkbox != null) return checkbox.Checked;

            // 使用反射访问私有字段
            var field = _form.GetType().GetField("NameReplaceMode", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null && field.GetValue(_form) is CheckBox cb) return cb.Checked;

            return false;
        }

        public DataGridView GetVotedataview()
        {
            var dgv = FindControl<DataGridView>("votedataview");
            if (dgv != null) return dgv;

            var field = _form.GetType().GetField("votedataview", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(_form) as DataGridView;

            return null;
        }

        public SaveFileDialog GetSaveFileDialog()
        {
            var field = _form.GetType().GetField("SaveFileDialog", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(_form) as SaveFileDialog;

            return null;
        }

        public Label GetProcessPage_P2W()
        {
            var label = FindControl<Label>("ProcessPage_P2W");
            if (label != null) return label;

            var field = _form.GetType().GetField("ProcessPage_P2W", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(_form) as Label;

            return null;
        }

        // UI Setters
        public void SetStatusLabelText(string text) { /* TODO */ }
        public void SetButtonEnabled(Button button, bool enabled)
        {
            if (button == null) return;
            if (button.InvokeRequired) button.Invoke(new MethodInvoker(() => button.Enabled = enabled));
            else button.Enabled = enabled;
        }

        public void SetProcessPage_P2WText(string text)
        {
            var label = GetProcessPage_P2W();
            if (label == null) return;
            if (label.InvokeRequired) label.Invoke(new MethodInvoker(() => label.Text = text));
            else label.Text = text;
        }

        public void SetVotedataviewDataSource(object dataSource)
        {
            var dgv = GetVotedataview();
            if (dgv == null) return;
            if (dgv.InvokeRequired) dgv.Invoke(new MethodInvoker(() => dgv.DataSource = dataSource));
            else dgv.DataSource = dataSource;
        }

        public void SetVotedataviewFirstDisplayedScrollingRowIndex(int index)
        {
            var dgv = GetVotedataview();
            if (dgv == null) return;
            if (dgv.InvokeRequired) dgv.Invoke(new MethodInvoker(() => dgv.FirstDisplayedScrollingRowIndex = index));
            else dgv.FirstDisplayedScrollingRowIndex = index;
        }

        public void BeginInvoke(Action action)
        {
            if (_form == null || action == null) return;
            if (_form.InvokeRequired) _form.BeginInvoke(action);
            else action();
        }
    }
}
