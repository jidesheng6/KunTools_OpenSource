using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DocumentTools.Services
{
    public interface IFormAccessor
    {
        // ListView Accessors
        ListView GetWordFileListView();
        ListView GetPdfFileListView();
        ListView GetExcelFileListView();
        ListView GetCompressImageFileList();
        ListView GetCompressPdfFileList();
        ListView GetConvertPdfComplianceFileList();
        ListView GetConvertImage2PDFFileList();
        ListView GetDecodeMatrixWordListView();
        ListView GetHtml2PdfFileList();
        ListView GetIcoFileView();
        ListView GetMergePdfFileView();
        ListView GetMergeWordFileList();
        ListView GetPdf2ExcelFileView();
        ListView GetPdf2PptFileList();
        ListView GetPpt2PdfFileList();
        ListView GetRemovePdfWaterMarkFileList();
        ListView GetSplitPdfFileList();
        ListView GetSplitWordFileList();
        ListView GetPdfWaterMarkFileList();
        ListView GetWebpFileList();
        ListView GetWord2EpubFileList();
        ListView GetWord2ExcelFileList();
        ListView GetDocumentsListView();
        ListView GetReplaceWordFileList();
        ListView GetSplitPptFilesList();
        ListView GetEpub2PdfFileList();
        ListView GetEpub2WordFileList();
        ListView GetExcel2WordFileList();
        ListView GetReplaceExcelFileView();

        // Conversion Parameter Accessors
        string GetConvertPdfComplianceSelectedValue();
        string GetWord2PdfWordVersionSelectedText();
        bool GetWord2PdfNeedUpdateFields();
        string GetWord2PdfColorModeSelectedValue();
        bool GetWord2PdfEmbedFonts();
        string GetWord2PdfLoadPassword();
        bool GetWord2PdfUseLocalOffice();
        bool GetPdf2WordUseBullet();
        bool GetPdf2WordUseSubOcr();
        string GetPdf2WordRecognitionModeSelectedValue();
        string GetPdf2WordLoadPassword();
        bool GetExcel2PdfExportEachSheet();
        bool GetExcel2PdfForceContentOnOnePage();
        bool GetExcel2PdfHorPrint();
        bool GetExcel2PdfVerPrint();
        int GetExcel2PdfPrintScaleRow();
        int GetExcel2PdfPrintScaleCol();
        bool GetExcel2PdfPrintScalePercentEnable();
        int GetExcel2PdfPrintScalePercentNumber();
        bool GetWord2ImageSetGray();
        bool GetWord2ImagePageExport();
        int GetWord2ImageDpiSelectedValue();
        bool GetPdf2ExcelSheetMode();
        string GetWordReplaceFirstFindText();
        IList<(string Find, string Replace)> GetWordReplacePairs();
        bool GetWordReplaceOneToOneMode();
        bool GetWordReplaceIgnoreFootContent();
        bool GetWordReplaceIgnoreCase();
        bool GetWordReplaceMatchWholeWord();
        bool GetWordReplaceIgnoreDelete();
        bool GetWordReplaceIgnoreFiledCodes();
        bool GetWordReplaceIgnoreFileds();
        bool GetWordReplaceIgnoreInsert();
        bool GetWordReplaceIgnoreShapes();
        bool GetWordReplaceUseRegexSub();

        // Additional UI Accessors for Util.cs
        bool GetNameReplaceMode();
        DataGridView GetVotedataview();
        SaveFileDialog GetSaveFileDialog();
        Label GetProcessPage_P2W();

        // UI Setters
        void SetStatusLabelText(string text);
        void SetButtonEnabled(Button button, bool enabled);
        void SetProcessPage_P2WText(string text);
        void SetVotedataviewDataSource(object dataSource);
        void SetVotedataviewFirstDisplayedScrollingRowIndex(int index);

        // Form Invoke Support
        void BeginInvoke(Action action);

        T FindControl<T>(string name) where T : Control;
    }
}
