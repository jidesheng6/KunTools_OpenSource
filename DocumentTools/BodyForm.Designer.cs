using System.Windows.Forms;
namespace DocumentTools
{
    partial class BodyForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BodyForm));
            this.MainTabPage = new System.Windows.Forms.TabControl();
            this.TabPage_01 = new System.Windows.Forms.TabPage();
            this.UseLocalOffice = new System.Windows.Forms.CheckBox();
            this.WORD2PDF_LoadPassword = new System.Windows.Forms.TextBox();
            this.label147 = new System.Windows.Forms.Label();
            this.EmbedFonts = new System.Windows.Forms.CheckBox();
            this.word2pdf_colorMode = new System.Windows.Forms.ComboBox();
            this.label125 = new System.Windows.Forms.Label();
            this.word2pdf_need_update_fields = new System.Windows.Forms.CheckBox();
            this.word2pdf_save_source = new System.Windows.Forms.CheckBox();
            this.Word2PDFOptimizCombox = new System.Windows.Forms.ComboBox();
            this.label62 = new System.Windows.Forms.Label();
            this.word2pdf_clear = new System.Windows.Forms.Button();
            this.word2pdf_remove = new System.Windows.Forms.Button();
            this.WordFileListView_MoveDown = new System.Windows.Forms.Button();
            this.WordFileListView_MoveUp = new System.Windows.Forms.Button();
            this.label34 = new System.Windows.Forms.Label();
            this.PDFComplianceBox = new System.Windows.Forms.ComboBox();
            this.RemoveTempDocx = new System.Windows.Forms.CheckBox();
            this.OpenPdfOptions = new System.Windows.Forms.CheckBox();
            this.StatusText = new System.Windows.Forms.Label();
            this.ConvertButton = new System.Windows.Forms.Button();
            this.ChooseDocButton = new System.Windows.Forms.Button();
            this.WordFileListView = new System.Windows.Forms.ListView();
            this.TabPage_02 = new System.Windows.Forms.TabPage();
            this.PDF2WORD_LoadPassword = new System.Windows.Forms.TextBox();
            this.label148 = new System.Windows.Forms.Label();
            this.pdf2word_save_source = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.PDF2WORD_Mode = new System.Windows.Forms.ComboBox();
            this.pdf2word_clear = new System.Windows.Forms.Button();
            this.pdf2word_remove = new System.Windows.Forms.Button();
            this.label55 = new System.Windows.Forms.Label();
            this.PDF2WORD_UseSUbOcr = new System.Windows.Forms.CheckBox();
            this.PDF2WORD_UseBullet = new System.Windows.Forms.CheckBox();
            this.PdfFileListView_MoveDown = new System.Windows.Forms.Button();
            this.PdfFileListView_MoveUp = new System.Windows.Forms.Button();
            this.ProcessPage_P2W = new System.Windows.Forms.Label();
            this.PdfStatusText = new System.Windows.Forms.Label();
            this.Convert2DocButton = new System.Windows.Forms.Button();
            this.SelectPdfButton = new System.Windows.Forms.Button();
            this.PdfFileListView = new System.Windows.Forms.ListView();
            this.TabPage_03 = new System.Windows.Forms.TabPage();
            this.ExcelFileListView_MoveDown = new System.Windows.Forms.Button();
            this.ExcelFileListView_MoveUp = new System.Windows.Forms.Button();
            this.excel2pdf_clear = new System.Windows.Forms.Button();
            this.excel2pdf_remove = new System.Windows.Forms.Button();
            this.ExportEachSheet = new System.Windows.Forms.CheckBox();
            this.Excel2PdfStatusText = new System.Windows.Forms.Label();
            this.ConvertExcel2PdfButton = new System.Windows.Forms.Button();
            this.SelectExcelFileButton = new System.Windows.Forms.Button();
            this.ExcelFileListView = new System.Windows.Forms.ListView();
            this.TabPage_07 = new System.Windows.Forms.TabPage();
            this.PDF2ExcelFileView_MoveDown = new System.Windows.Forms.Button();
            this.PDF2ExcelFileView_MoveUp = new System.Windows.Forms.Button();
            this.pdf2excel_clear = new System.Windows.Forms.Button();
            this.pdf2excel_remove = new System.Windows.Forms.Button();
            this.ExcelSheelMode = new System.Windows.Forms.CheckBox();
            this.ConvertPDF2ExcelStatusText = new System.Windows.Forms.Label();
            this.ConvertPDF2Excel = new System.Windows.Forms.Button();
            this.SelectPDF2ExcelButton = new System.Windows.Forms.Button();
            this.PDF2ExcelFileView = new System.Windows.Forms.ListView();
            this.TabPage_20 = new System.Windows.Forms.TabPage();
            this.ppt2pdf_save_source = new System.Windows.Forms.CheckBox();
            this.PPT2PdfFileList_MoveDown = new System.Windows.Forms.Button();
            this.PPT2PdfFileList_MoveUp = new System.Windows.Forms.Button();
            this.ppt2pdf_clear = new System.Windows.Forms.Button();
            this.ppt2pdf_remove = new System.Windows.Forms.Button();
            this.label36 = new System.Windows.Forms.Label();
            this.ConvertPPT2PDFPassword = new System.Windows.Forms.TextBox();
            this.label35 = new System.Windows.Forms.Label();
            this.ExportNotePageCombox = new System.Windows.Forms.ComboBox();
            this.DrawBlackLine = new System.Windows.Forms.CheckBox();
            this.ExportHiddenPage = new System.Windows.Forms.CheckBox();
            this.ConvertPPT2PDFStatusText = new System.Windows.Forms.Label();
            this.ConvertPPT2PDF = new System.Windows.Forms.Button();
            this.SelectPPT2PdfFile = new System.Windows.Forms.Button();
            this.PPT2PdfFileList = new System.Windows.Forms.ListView();
            this.TabPage_21 = new System.Windows.Forms.TabPage();
            this.pdf2ppt_save_source = new System.Windows.Forms.CheckBox();
            this.PDF2PPTFileList_MoveDown = new System.Windows.Forms.Button();
            this.PDF2PPTFileList_MoveUp = new System.Windows.Forms.Button();
            this.pdf2ppt_clear = new System.Windows.Forms.Button();
            this.pdf2ppt_remove = new System.Windows.Forms.Button();
            this.RenderImageBox = new System.Windows.Forms.CheckBox();
            this.SplitShapeBox = new System.Windows.Forms.CheckBox();
            this.ExtraOcrBox = new System.Windows.Forms.CheckBox();
            this.Pdf2PPTStatusText = new System.Windows.Forms.Label();
            this.ConvertPDF2PPT = new System.Windows.Forms.Button();
            this.SelectConvertPDF2PPT = new System.Windows.Forms.Button();
            this.PDF2PPTFileList = new System.Windows.Forms.ListView();
            this.TabPage_14 = new System.Windows.Forms.TabPage();
            this.MergeWordFileList_MoveDown = new System.Windows.Forms.Button();
            this.MergeWordFileList_MoveUp = new System.Windows.Forms.Button();
            this.meregeword_clear = new System.Windows.Forms.Button();
            this.meregeword_remove = new System.Windows.Forms.Button();
            this.MeregeWordStatusText = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.MergeWordFile = new System.Windows.Forms.Button();
            this.SelectMergeWordFiles = new System.Windows.Forms.Button();
            this.MergeWordFileList = new System.Windows.Forms.ListView();
            this.TabPage_31 = new System.Windows.Forms.TabPage();
            this.SplitWordReUpdatePageNumber = new System.Windows.Forms.CheckBox();
            this.groupBox48 = new System.Windows.Forms.GroupBox();
            this.label145 = new System.Windows.Forms.Label();
            this.label146 = new System.Windows.Forms.Label();
            this.SplitWord_PageCount = new System.Windows.Forms.NumericUpDown();
            this.SplitWord_Single = new System.Windows.Forms.RadioButton();
            this.label135 = new System.Windows.Forms.Label();
            this.SplitWord_Range_End = new System.Windows.Forms.TextBox();
            this.label136 = new System.Windows.Forms.Label();
            this.SplitWord_Range_Start = new System.Windows.Forms.TextBox();
            this.label137 = new System.Windows.Forms.Label();
            this.SplitWord_Range = new System.Windows.Forms.RadioButton();
            this.SplitWordFileList_MoveDown = new System.Windows.Forms.Button();
            this.SplitWordFileList_MoveUp = new System.Windows.Forms.Button();
            this.splitword_clear = new System.Windows.Forms.Button();
            this.splitword_remove = new System.Windows.Forms.Button();
            this.StartSplitWordStatusText = new System.Windows.Forms.Label();
            this.StartSplitWord = new System.Windows.Forms.Button();
            this.SelectSplitWordFiles = new System.Windows.Forms.Button();
            this.SplitWordFileList = new System.Windows.Forms.ListView();
            this.TabPage_06 = new System.Windows.Forms.TabPage();
            this.OpenMeregePdfFile = new System.Windows.Forms.CheckBox();
            this.MeregePdfFileStatusText = new System.Windows.Forms.Label();
            this.MeregePdfFileButtonConvert = new System.Windows.Forms.Button();
            this.MergePdfFileSelectButton = new System.Windows.Forms.Button();
            this.MergePdfFileView_MoveDown = new System.Windows.Forms.Button();
            this.MergePdfFileView_MoveUp = new System.Windows.Forms.Button();
            this.meregepdf_clear = new System.Windows.Forms.Button();
            this.meregepdf_remove = new System.Windows.Forms.Button();
            this.MergePdfFileView = new System.Windows.Forms.ListView();
            this.TabPage_17 = new System.Windows.Forms.TabPage();
            this.groupBox47 = new System.Windows.Forms.GroupBox();
            this.SplitPdf_ByBookMark = new System.Windows.Forms.RadioButton();
            this.label144 = new System.Windows.Forms.Label();
            this.label143 = new System.Windows.Forms.Label();
            this.SplitPdf_PerPageCount = new System.Windows.Forms.NumericUpDown();
            this.SplitPdf_Single = new System.Windows.Forms.RadioButton();
            this.label134 = new System.Windows.Forms.Label();
            this.SplitPdfRange_End = new System.Windows.Forms.TextBox();
            this.label133 = new System.Windows.Forms.Label();
            this.SplitPdfRange_Start = new System.Windows.Forms.TextBox();
            this.label132 = new System.Windows.Forms.Label();
            this.SplitPdf_Range = new System.Windows.Forms.RadioButton();
            this.SplitPdfFileList_MoveDown = new System.Windows.Forms.Button();
            this.SplitPdfFileList_MoveUp = new System.Windows.Forms.Button();
            this.sqplitpdf_clear = new System.Windows.Forms.Button();
            this.sqplitpdf_remove = new System.Windows.Forms.Button();
            this.SplitPdfStatusText = new System.Windows.Forms.Label();
            this.StartSplitPdf = new System.Windows.Forms.Button();
            this.SelectSplitPdf = new System.Windows.Forms.Button();
            this.SplitPdfFileList = new System.Windows.Forms.ListView();
            this.TabPage_41 = new System.Windows.Forms.TabPage();
            this.groupBox62 = new System.Windows.Forms.GroupBox();
            this.label178 = new System.Windows.Forms.Label();
            this.label177 = new System.Windows.Forms.Label();
            this.SplitPdfScaleCombox = new System.Windows.Forms.ComboBox();
            this.SplitPdfSizeCombox = new System.Windows.Forms.ComboBox();
            this.groupBox61 = new System.Windows.Forms.GroupBox();
            this.SplitPPT_SignleExport = new System.Windows.Forms.RadioButton();
            this.label179 = new System.Windows.Forms.Label();
            this.SplitPPT_RangeExport_End = new System.Windows.Forms.TextBox();
            this.label180 = new System.Windows.Forms.Label();
            this.SplitPPT_RangeExport_Start = new System.Windows.Forms.TextBox();
            this.label181 = new System.Windows.Forms.Label();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.SplitPPTFilesList_MoveDown = new System.Windows.Forms.Button();
            this.SplitPPTFilesList_MoveUp = new System.Windows.Forms.Button();
            this.splitppt_clear = new System.Windows.Forms.Button();
            this.splitppt_remove = new System.Windows.Forms.Button();
            this.SplitPPTStatusText = new System.Windows.Forms.Label();
            this.SplitPPT_Button = new System.Windows.Forms.Button();
            this.SplitPPT_SelectFiles = new System.Windows.Forms.Button();
            this.SplitPPTFilesList = new System.Windows.Forms.ListView();
            this.TabPage_18 = new System.Windows.Forms.TabPage();
            this.PdfWaterMarkFileList_MoveDown = new System.Windows.Forms.Button();
            this.PdfWaterMarkFileList_MoveUp = new System.Windows.Forms.Button();
            this.addpdfwater_clear = new System.Windows.Forms.Button();
            this.addpdfwater_remove = new System.Windows.Forms.Button();
            this.label31 = new System.Windows.Forms.Label();
            this.AddWaterMark2PdfStatusText = new System.Windows.Forms.Label();
            this.AddWaterMark2Pdf = new System.Windows.Forms.Button();
            this.SelectWaterMarkPdfFiles = new System.Windows.Forms.Button();
            this.groupBox18 = new System.Windows.Forms.GroupBox();
            this.label33 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.WaterMarkImageWidth = new System.Windows.Forms.TextBox();
            this.AddBackGroudImage = new System.Windows.Forms.Button();
            this.WaterMarkImageHeight = new System.Windows.Forms.TextBox();
            this.WaterMarkImage = new System.Windows.Forms.PictureBox();
            this.PdfWaterMarkOpcityNumber = new System.Windows.Forms.ComboBox();
            this.label30 = new System.Windows.Forms.Label();
            this.PdfWaterMarkRotateDegNumber = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.PDFWaterMarkVerSetting = new System.Windows.Forms.ComboBox();
            this.PdfWaterMarkHorSetting = new System.Windows.Forms.ComboBox();
            this.label28 = new System.Windows.Forms.Label();
            this.shui = new System.Windows.Forms.Label();
            this.PDFWaterMarkTextFontSetting = new System.Windows.Forms.Button();
            this.PdfWaterMarkFontColorSettings = new System.Windows.Forms.Button();
            this.label27 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.PdfWaterMarkText = new System.Windows.Forms.TextBox();
            this.PdfWaterMarkFileList = new System.Windows.Forms.ListView();
            this.TabPage_19 = new System.Windows.Forms.TabPage();
            this.RemovePdfWaterMarkFileList_MoveDown = new System.Windows.Forms.Button();
            this.RemovePdfWaterMarkFileList_MoveUp = new System.Windows.Forms.Button();
            this.removepdfwater_clear = new System.Windows.Forms.Button();
            this.removepdfwater_remove = new System.Windows.Forms.Button();
            this.RemovePdfWaterMarkStatusText = new System.Windows.Forms.Label();
            this.RemovePdfWaterMark = new System.Windows.Forms.Button();
            this.SelectRemovePdfWaterMarkFiles = new System.Windows.Forms.Button();
            this.RemovePdfWaterMarkFileList = new System.Windows.Forms.ListView();
            this.TabPage_22 = new System.Windows.Forms.TabPage();
            this.ConvertPdfComplianceFileList_MoveDown = new System.Windows.Forms.Button();
            this.ConvertPdfComplianceFileList_MoveUp = new System.Windows.Forms.Button();
            this.pdfversion_clear = new System.Windows.Forms.Button();
            this.pdfversion_remove = new System.Windows.Forms.Button();
            this.ConvertPdfComplianceVersion = new System.Windows.Forms.ComboBox();
            this.label39 = new System.Windows.Forms.Label();
            this.ConvertPdfComplianceStatusText = new System.Windows.Forms.Label();
            this.ConvertPdfCompliance = new System.Windows.Forms.Button();
            this.SelectConvertComplaincePdf = new System.Windows.Forms.Button();
            this.ConvertPdfComplianceFileList = new System.Windows.Forms.ListView();
            this.TabPage_15 = new System.Windows.Forms.TabPage();
            this.word2excel_save_source = new System.Windows.Forms.CheckBox();
            this.Word2ExcelFileList_MoveDown = new System.Windows.Forms.Button();
            this.Word2ExcelFileList_MoveUp = new System.Windows.Forms.Button();
            this.word2excel_clear = new System.Windows.Forms.Button();
            this.word2excel_remove = new System.Windows.Forms.Button();
            this.ConverWord2ExcelStatusText = new System.Windows.Forms.Label();
            this.ConvertWord2Excel = new System.Windows.Forms.Button();
            this.SelectWord2ExcelFiles = new System.Windows.Forms.Button();
            this.Word2ExcelFileList = new System.Windows.Forms.ListView();
            this.TabPage_16 = new System.Windows.Forms.TabPage();
            this.excel2word_save_source = new System.Windows.Forms.CheckBox();
            this.Excel2WordFileList_MoveDown = new System.Windows.Forms.Button();
            this.Excel2WordFileList_MoveUp = new System.Windows.Forms.Button();
            this.excel2word_clear = new System.Windows.Forms.Button();
            this.excel2word_remove = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.ConvertExcel2WordStatusText = new System.Windows.Forms.Label();
            this.ConvertExcel2Word = new System.Windows.Forms.Button();
            this.SelectExcel2WordFiles = new System.Windows.Forms.Button();
            this.Excel2WordFileList = new System.Windows.Forms.ListView();
            this.TabPage_04 = new System.Windows.Forms.TabPage();
            this.word2image_save_source = new System.Windows.Forms.CheckBox();
            this.DocumentsListView_MoveDown = new System.Windows.Forms.Button();
            this.DocumentsListView_MoveUp = new System.Windows.Forms.Button();
            this.document2image_clear = new System.Windows.Forms.Button();
            this.document2image_remove = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.PageExportCheck = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.DpiItemList = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SelectWord2ImageFileButton = new System.Windows.Forms.Button();
            this.ConvertWord2ImageStatus = new System.Windows.Forms.Label();
            this.SetGrayCheckBox = new System.Windows.Forms.CheckBox();
            this.ConvertWord2ImageButton = new System.Windows.Forms.Button();
            this.ConvertImageList = new System.Windows.Forms.ComboBox();
            this.DocumentsListView = new System.Windows.Forms.ListView();
            this.TabPage_27 = new System.Windows.Forms.TabPage();
            this.label65 = new System.Windows.Forms.Label();
            this.CoverImageDpi = new System.Windows.Forms.TextBox();
            this.label64 = new System.Windows.Forms.Label();
            this.SetWord2EpubCoverImage = new System.Windows.Forms.Button();
            this.Word2EpubConverImage = new System.Windows.Forms.TextBox();
            this.label63 = new System.Windows.Forms.Label();
            this.Word2EpubAuthorName = new System.Windows.Forms.TextBox();
            this.label61 = new System.Windows.Forms.Label();
            this.ConvertWord2EpubStatusText = new System.Windows.Forms.Label();
            this.word2epub_clear = new System.Windows.Forms.Button();
            this.word2epub_remove = new System.Windows.Forms.Button();
            this.Word2EPUBFileList_MoveDown = new System.Windows.Forms.Button();
            this.Word2EPUBFileList_MoveUp = new System.Windows.Forms.Button();
            this.ConvertWord2Epub = new System.Windows.Forms.Button();
            this.SelectWord2EpubFiles = new System.Windows.Forms.Button();
            this.Word2EPUBFileList = new System.Windows.Forms.ListView();
            this.TabPage_28 = new System.Windows.Forms.TabPage();
            this.label60 = new System.Windows.Forms.Label();
            this.EPUB2WORDFileList_MoveDown = new System.Windows.Forms.Button();
            this.EPUB2WORDFileList_MoveUp = new System.Windows.Forms.Button();
            this.epub2word_clear = new System.Windows.Forms.Button();
            this.epub2word_remove = new System.Windows.Forms.Button();
            this.ConvertEpub2WordStatusText = new System.Windows.Forms.Label();
            this.ConvertEpub2Word = new System.Windows.Forms.Button();
            this.SelectEpub2WordFiles = new System.Windows.Forms.Button();
            this.EPUB2WORDFileList = new System.Windows.Forms.ListView();
            this.TabPage_29 = new System.Windows.Forms.TabPage();
            this.label59 = new System.Windows.Forms.Label();
            this.EPUB2PDFFileList_MoveDown = new System.Windows.Forms.Button();
            this.EPUB2PDFFileList_MoveUp = new System.Windows.Forms.Button();
            this.epub2pdf_clear = new System.Windows.Forms.Button();
            this.epub2pdf_remove = new System.Windows.Forms.Button();
            this.ConvertEPUB2PDFStatusText = new System.Windows.Forms.Label();
            this.ConvertEPUB2PDF = new System.Windows.Forms.Button();
            this.SelectEPUB2PDFFiles = new System.Windows.Forms.Button();
            this.EPUB2PDFFileList = new System.Windows.Forms.ListView();
            this.TabPage_23 = new System.Windows.Forms.TabPage();
            this.ConvertImage2PDFFileList_MoveDown = new System.Windows.Forms.Button();
            this.ConvertImage2PDFFileList_MoveUp = new System.Windows.Forms.Button();
            this.image2pdf_clear = new System.Windows.Forms.Button();
            this.image2pdf_remove = new System.Windows.Forms.Button();
            this.COnvertImage2PDFStatusText = new System.Windows.Forms.Label();
            this.groupBox19 = new System.Windows.Forms.GroupBox();
            this.Image2PDFCOmpressImage = new System.Windows.Forms.CheckBox();
            this.Image2PDFOptiomize = new System.Windows.Forms.CheckBox();
            this.groupBox20 = new System.Windows.Forms.GroupBox();
            this.ImagePdfPageSIze_RIGHT = new System.Windows.Forms.TextBox();
            this.ImagePdfPageSIze_BOTTOM = new System.Windows.Forms.TextBox();
            this.ImagePdfPageSIze_LEFT = new System.Windows.Forms.TextBox();
            this.ImagePdfPageSIze_UP = new System.Windows.Forms.TextBox();
            this.label43 = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.label41 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.Image2PDFCombox = new System.Windows.Forms.ComboBox();
            this.label38 = new System.Windows.Forms.Label();
            this.ExportSignlePDF = new System.Windows.Forms.CheckBox();
            this.ConvertImage2PDF = new System.Windows.Forms.Button();
            this.SelectConvertImage2PDF = new System.Windows.Forms.Button();
            this.ConvertImage2PDFFileList = new System.Windows.Forms.ListView();
            this.TabPage_30 = new System.Windows.Forms.TabPage();
            this.linkLabel4 = new System.Windows.Forms.LinkLabel();
            this.label74 = new System.Windows.Forms.Label();
            this.groupBox31 = new System.Windows.Forms.GroupBox();
            this.label129 = new System.Windows.Forms.Label();
            this.label128 = new System.Windows.Forms.Label();
            this.html2pdf_wait_timeout = new System.Windows.Forms.NumericUpDown();
            this.html2pdf_timeout = new System.Windows.Forms.NumericUpDown();
            this.label127 = new System.Windows.Forms.Label();
            this.html2pdf_button_set_chrome_path = new System.Windows.Forms.Button();
            this.label126 = new System.Windows.Forms.Label();
            this.linkLabel3 = new System.Windows.Forms.LinkLabel();
            this.html2pdf_textbox_chromepath = new System.Windows.Forms.TextBox();
            this.label73 = new System.Windows.Forms.Label();
            this.groupBox30 = new System.Windows.Forms.GroupBox();
            this.html2pdf_textbox_margin_bottom = new System.Windows.Forms.TextBox();
            this.html2pdf_textbox_margin_top = new System.Windows.Forms.TextBox();
            this.html2pdf_textbox_margin_right = new System.Windows.Forms.TextBox();
            this.html2pdf_textbox_margin_left = new System.Windows.Forms.TextBox();
            this.label72 = new System.Windows.Forms.Label();
            this.label71 = new System.Windows.Forms.Label();
            this.label70 = new System.Windows.Forms.Label();
            this.label69 = new System.Windows.Forms.Label();
            this.groupBox29 = new System.Windows.Forms.GroupBox();
            this.html2pdf_wait_all_resource = new System.Windows.Forms.CheckBox();
            this.html2pdf_combox_pagedirect = new System.Windows.Forms.ComboBox();
            this.html2pdf_combox_pagesize = new System.Windows.Forms.ComboBox();
            this.html2pdf_checkbox_usecss = new System.Windows.Forms.CheckBox();
            this.label68 = new System.Windows.Forms.Label();
            this.label67 = new System.Windows.Forms.Label();
            this.label66 = new System.Windows.Forms.Label();
            this.HTML2PDFFileList_MoveDown = new System.Windows.Forms.Button();
            this.HTML2PDFFileList_MoveUp = new System.Windows.Forms.Button();
            this.html2pdf_clear = new System.Windows.Forms.Button();
            this.html2pdf_remove = new System.Windows.Forms.Button();
            this.ConvertHtml2PdfStatusText = new System.Windows.Forms.Label();
            this.ConvertHtml2Pdf = new System.Windows.Forms.Button();
            this.SelectHtml2PDFFiles = new System.Windows.Forms.Button();
            this.HTML2PDFFileList = new System.Windows.Forms.ListView();
            this.TabPage_24 = new System.Windows.Forms.TabPage();
            this.CompressPdfFileList_MoveDown = new System.Windows.Forms.Button();
            this.CompressPdfFileList_MoveUp = new System.Windows.Forms.Button();
            this.compresspdf_clear = new System.Windows.Forms.Button();
            this.compresspdf_remove = new System.Windows.Forms.Button();
            this.CompressPDFStatusText = new System.Windows.Forms.Label();
            this.groupBox21 = new System.Windows.Forms.GroupBox();
            this.CompressPDF_CompressImageFlate = new System.Windows.Forms.CheckBox();
            this.CompressPDF_RemoveForms = new System.Windows.Forms.CheckBox();
            this.CompressPDF_CancelEmbedFont = new System.Windows.Forms.CheckBox();
            this.CompressPDF_LinkSameStream = new System.Windows.Forms.CheckBox();
            this.CompressPDF_RemoveUnuseStream = new System.Windows.Forms.CheckBox();
            this.CompressPDF_RemoveUnuseObject = new System.Windows.Forms.CheckBox();
            this.CompressPDF_ImageCompressVersion = new System.Windows.Forms.ComboBox();
            this.label46 = new System.Windows.Forms.Label();
            this.CompressPDF_ImageResoution = new System.Windows.Forms.ComboBox();
            this.label45 = new System.Windows.Forms.Label();
            this.CompressPDF_ImageQuailty = new System.Windows.Forms.ComboBox();
            this.label44 = new System.Windows.Forms.Label();
            this.CompressPDF_CompressImage = new System.Windows.Forms.CheckBox();
            this.CompressPDF_WEB = new System.Windows.Forms.CheckBox();
            this.CompressPDF = new System.Windows.Forms.Button();
            this.SelectCompressPDFFiles = new System.Windows.Forms.Button();
            this.CompressPdfFileList = new System.Windows.Forms.ListView();
            this.TabPage_25 = new System.Windows.Forms.TabPage();
            this.CompressImageFileList_MoveDown = new System.Windows.Forms.Button();
            this.CompressImageFileList_MoveUp = new System.Windows.Forms.Button();
            this.compressimage_clear = new System.Windows.Forms.Button();
            this.compressimage_remove = new System.Windows.Forms.Button();
            this.CompressImageStatusText = new System.Windows.Forms.Label();
            this.groupBox25 = new System.Windows.Forms.GroupBox();
            this.CompressImage_NotUseSelfExt = new System.Windows.Forms.CheckBox();
            this.CompressImage_OutPutImageFormat = new System.Windows.Forms.ComboBox();
            this.label54 = new System.Windows.Forms.Label();
            this.groupBox24 = new System.Windows.Forms.GroupBox();
            this.CompressBmpPixeBits_Combox = new System.Windows.Forms.ComboBox();
            this.CompressBmpMode_Combox = new System.Windows.Forms.ComboBox();
            this.label50 = new System.Windows.Forms.Label();
            this.label53 = new System.Windows.Forms.Label();
            this.groupBox23 = new System.Windows.Forms.GroupBox();
            this.CompressPngColorMode_Combox = new System.Windows.Forms.ComboBox();
            this.CompressPngLevel_Combox = new System.Windows.Forms.ComboBox();
            this.label51 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.groupBox22 = new System.Windows.Forms.GroupBox();
            this.CompressJpegQuailty_Combox = new System.Windows.Forms.ComboBox();
            this.label49 = new System.Windows.Forms.Label();
            this.CompressJpegColorMode_Combox = new System.Windows.Forms.ComboBox();
            this.CompressJpegMode_Combox = new System.Windows.Forms.ComboBox();
            this.label48 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.CompressImage = new System.Windows.Forms.Button();
            this.SelectCompressImageFile = new System.Windows.Forms.Button();
            this.CompressImageFileList = new System.Windows.Forms.ListView();
            this.TabPage_10 = new System.Windows.Forms.TabPage();
            this.UseNewStyle = new System.Windows.Forms.CheckBox();
            this.WordReplaceFontSeeting = new System.Windows.Forms.Button();
            this.ReplaceWordFileList_MoveDown = new System.Windows.Forms.Button();
            this.ReplaceWordFileList_MoveUp = new System.Windows.Forms.Button();
            this.wordreplace_clear = new System.Windows.Forms.Button();
            this.wordreplace_remove = new System.Windows.Forms.Button();
            this.TurialRegex = new System.Windows.Forms.LinkLabel();
            this.OnlineRegextTool = new System.Windows.Forms.LinkLabel();
            this.groupBox10 = new System.Windows.Forms.GroupBox();
            this.WordReplaceTextBox = new System.Windows.Forms.TextBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.WordReplaceFindTextBox = new System.Windows.Forms.TextBox();
            this.IgnoreFootContent = new System.Windows.Forms.CheckBox();
            this.IgnoreCase = new System.Windows.Forms.CheckBox();
            this.ReplaceWordContentStatusText = new System.Windows.Forms.Label();
            this.ReplaceWordContentButton = new System.Windows.Forms.Button();
            this.SelectReplaceWordFilesButton = new System.Windows.Forms.Button();
            this.ReplaceWordFileList = new System.Windows.Forms.ListView();
            this.TabPage_11 = new System.Windows.Forms.TabPage();
            this.ReplaceExcelFileView_MoveDown = new System.Windows.Forms.Button();
            this.ReplaceExcelFileView_MoveUp = new System.Windows.Forms.Button();
            this.excelreplace_clear = new System.Windows.Forms.Button();
            this.excelreplace_remove = new System.Windows.Forms.Button();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.ExcelRegexReadMe = new System.Windows.Forms.LinkLabel();
            this.UseRegex = new System.Windows.Forms.CheckBox();
            this.ExcelNoIgnoreCase = new System.Windows.Forms.CheckBox();
            this.groupBox13 = new System.Windows.Forms.GroupBox();
            this.ExcelStartRow = new System.Windows.Forms.TextBox();
            this.ExcelEndRow = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.RemoveRows = new System.Windows.Forms.CheckBox();
            this.groupBox12 = new System.Windows.Forms.GroupBox();
            this.ExcelReplaceBox = new System.Windows.Forms.TextBox();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.ExcelFindTextBox = new System.Windows.Forms.TextBox();
            this.ReplaceExcelStatusText = new System.Windows.Forms.Label();
            this.ReplaceExcelContentButton = new System.Windows.Forms.Button();
            this.SelectRelaceExcelButton = new System.Windows.Forms.Button();
            this.ReplaceExcelFileView = new System.Windows.Forms.ListView();
            this.TabPage_12 = new System.Windows.Forms.TabPage();
            this.DecodeMatrixWordListView_MoveDown = new System.Windows.Forms.Button();
            this.DecodeMatrixWordListView_MoveUp = new System.Windows.Forms.Button();
            this.datamatrix_clear = new System.Windows.Forms.Button();
            this.datamatrix_remove = new System.Windows.Forms.Button();
            this.DecodeDataMatixStatusText = new System.Windows.Forms.Label();
            this.DecodeDataMarix = new System.Windows.Forms.Button();
            this.SelectDataMatrixWord = new System.Windows.Forms.Button();
            this.groupBox14 = new System.Windows.Forms.GroupBox();
            this.DecodeDataMatrixView = new System.Windows.Forms.TreeView();
            this.DecodeMatrixWordListView = new System.Windows.Forms.ListView();
            this.TabPage_33 = new System.Windows.Forms.TabPage();
            this.barcodeReadStart = new System.Windows.Forms.Button();
            this.CopyBarcodeReadResult = new System.Windows.Forms.Button();
            this.barcodeReaderResultbox = new System.Windows.Forms.RichTextBox();
            this.barcodeSelectRegionPicture = new System.Windows.Forms.Button();
            this.barcodeRegionPicture = new System.Windows.Forms.PictureBox();
            this.groupBox36 = new System.Windows.Forms.GroupBox();
            this.barcodeCopyImage = new System.Windows.Forms.Button();
            this.GenerateBarCode = new System.Windows.Forms.Button();
            this.SaveBarCodePreview = new System.Windows.Forms.Button();
            this.barcodePreviewBox = new System.Windows.Forms.PictureBox();
            this.groupBox38 = new System.Windows.Forms.GroupBox();
            this.barcodeCaptionLocationCombox = new System.Windows.Forms.ComboBox();
            this.label93 = new System.Windows.Forms.Label();
            this.barcodeCaptionFontSizeBox = new System.Windows.Forms.TextBox();
            this.label92 = new System.Windows.Forms.Label();
            this.barcodeCaptionBox = new System.Windows.Forms.TextBox();
            this.label90 = new System.Windows.Forms.Label();
            this.groupBox37 = new System.Windows.Forms.GroupBox();
            this.ExportBarCodeTempLate = new System.Windows.Forms.Button();
            this.barcodeMutilGenerate = new System.Windows.Forms.Button();
            this.barcodeTemplateUpload = new System.Windows.Forms.Button();
            this.barcodeMutilList = new System.Windows.Forms.DataGridView();
            this.groupBox35 = new System.Windows.Forms.GroupBox();
            this.barcodeImageWidth = new System.Windows.Forms.TextBox();
            this.barcodePaddingBox = new System.Windows.Forms.TextBox();
            this.label89 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.barcodeTypeCombox = new System.Windows.Forms.ComboBox();
            this.label80 = new System.Windows.Forms.Label();
            this.barcodeDmiBox = new System.Windows.Forms.TextBox();
            this.label81 = new System.Windows.Forms.Label();
            this.barcodeRotateBox = new System.Windows.Forms.TextBox();
            this.barcodeDpiCombox = new System.Windows.Forms.ComboBox();
            this.label82 = new System.Windows.Forms.Label();
            this.label84 = new System.Windows.Forms.Label();
            this.label83 = new System.Windows.Forms.Label();
            this.barcodeAutoSizeCombox = new System.Windows.Forms.ComboBox();
            this.label79 = new System.Windows.Forms.Label();
            this.barcodeImageHeight = new System.Windows.Forms.TextBox();
            this.groupBox34 = new System.Windows.Forms.GroupBox();
            this.barcodeScanTypeCombox = new System.Windows.Forms.ComboBox();
            this.label94 = new System.Windows.Forms.Label();
            this.barcodeTextFontSizeBox = new System.Windows.Forms.TextBox();
            this.label91 = new System.Windows.Forms.Label();
            this.barcodeTextContentBox = new System.Windows.Forms.TextBox();
            this.label88 = new System.Windows.Forms.Label();
            this.barcodeTextContentSpaceCombox = new System.Windows.Forms.TextBox();
            this.label87 = new System.Windows.Forms.Label();
            this.barcodeTextAlignCombox = new System.Windows.Forms.ComboBox();
            this.label86 = new System.Windows.Forms.Label();
            this.barcodeTextLocationCombox = new System.Windows.Forms.ComboBox();
            this.label85 = new System.Windows.Forms.Label();
            this.TabPage_34 = new System.Windows.Forms.TabPage();
            this.groupBox41 = new System.Windows.Forms.GroupBox();
            this.groupBox42 = new System.Windows.Forms.GroupBox();
            this.ver_card_id_index = new System.Windows.Forms.Label();
            this.label103 = new System.Windows.Forms.Label();
            this.ver_card_id_sign_tips = new System.Windows.Forms.Label();
            this.ver_card_id_sex = new System.Windows.Forms.Label();
            this.ver_card_id_birthday = new System.Windows.Forms.Label();
            this.label102 = new System.Windows.Forms.Label();
            this.label101 = new System.Windows.Forms.Label();
            this.label100 = new System.Windows.Forms.Label();
            this.ver_card_id_text_box = new System.Windows.Forms.TextBox();
            this.label99 = new System.Windows.Forms.Label();
            this.gen_card_result_group = new System.Windows.Forms.GroupBox();
            this.gen_card_id_name_copy = new System.Windows.Forms.Button();
            this.copy_gen_card_cliboard = new System.Windows.Forms.Button();
            this.gen_card_num_text = new System.Windows.Forms.Label();
            this.gen_card_id_box = new System.Windows.Forms.RichTextBox();
            this.gen_card_id_name_box = new System.Windows.Forms.RichTextBox();
            this.gen_card_id_group = new System.Windows.Forms.GroupBox();
            this.gen_card_id_name_text = new System.Windows.Forms.Label();
            this.label104 = new System.Windows.Forms.Label();
            this.gen_card_id_name_check_box = new System.Windows.Forms.CheckBox();
            this.ony_gen_18_year = new System.Windows.Forms.CheckBox();
            this.gen_card_number = new System.Windows.Forms.NumericUpDown();
            this.label97 = new System.Windows.Forms.Label();
            this.gen_card_button = new System.Windows.Forms.Button();
            this.label96 = new System.Windows.Forms.Label();
            this.city_level_1_combox = new System.Windows.Forms.ComboBox();
            this.city_level_3_combox = new System.Windows.Forms.ComboBox();
            this.city_level_2_combox = new System.Windows.Forms.ComboBox();
            this.TabPage_05 = new System.Windows.Forms.TabPage();
            this.DoubleTextClear = new System.Windows.Forms.CheckBox();
            this.NameMarkColor = new System.Windows.Forms.ComboBox();
            this.MarkDupName = new System.Windows.Forms.CheckBox();
            this.ExportNameTxt = new System.Windows.Forms.Button();
            this.label57 = new System.Windows.Forms.Label();
            this.NameReplaceRegexMode = new System.Windows.Forms.Label();
            this.NameAutoCopyStatusText = new System.Windows.Forms.Label();
            this.NameAutoCopy = new System.Windows.Forms.CheckBox();
            this.NameReplaceWord = new System.Windows.Forms.CheckBox();
            this.label56 = new System.Windows.Forms.Label();
            this.NameReplaceWord_Target = new System.Windows.Forms.TextBox();
            this.NameReplaceWord_Search = new System.Windows.Forms.TextBox();
            this.NameStorkeChcek = new System.Windows.Forms.CheckBox();
            this.MutilLineMode = new System.Windows.Forms.CheckBox();
            this.DoubleTextAlignCheck = new System.Windows.Forms.CheckBox();
            this.RemoveNumberCheck = new System.Windows.Forms.CheckBox();
            this.RemoveSquareCheck = new System.Windows.Forms.CheckBox();
            this.TargetTextNumberText = new System.Windows.Forms.Label();
            this.ConvertTextButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.TargetTextBox = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SourceTextBox = new System.Windows.Forms.RichTextBox();
            this.TabPage_35 = new System.Windows.Forms.TabPage();
            this.groupBox39 = new System.Windows.Forms.GroupBox();
            this.webp2image_combox = new System.Windows.Forms.ComboBox();
            this.label98 = new System.Windows.Forms.Label();
            this.WebpFileList_MoveDown = new System.Windows.Forms.Button();
            this.WebpFileList_MoveUp = new System.Windows.Forms.Button();
            this.webp2image_clear = new System.Windows.Forms.Button();
            this.webp2image_remove = new System.Windows.Forms.Button();
            this.webpconvert_statusText = new System.Windows.Forms.Label();
            this.webpPictureStartConvert = new System.Windows.Forms.Button();
            this.webpPictureSelect = new System.Windows.Forms.Button();
            this.WebpFileList = new System.Windows.Forms.ListView();
            this.TabPage_09 = new System.Windows.Forms.TabPage();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.ResetTotalScanNumber = new System.Windows.Forms.Button();
            this.RemoveAllFiles = new System.Windows.Forms.Button();
            this.SavedPathFileCount = new System.Windows.Forms.Label();
            this.ScanSpendTime = new System.Windows.Forms.Label();
            this.NowScanNumber = new System.Windows.Forms.Label();
            this.ScannTotalNumber = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.PreviewPictureName = new System.Windows.Forms.Label();
            this.PreviewPictureBox = new System.Windows.Forms.PictureBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.ScanSerialNumberTextBox = new System.Windows.Forms.TextBox();
            this.TwainSetDeviceSaveTime = new System.Windows.Forms.Button();
            this.ExportCaps = new System.Windows.Forms.CheckBox();
            this.TwainScanNumberSet = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.UseInterfaceUi = new System.Windows.Forms.CheckBox();
            this.TwainPaperSizeBox = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.SkipBlankPage = new System.Windows.Forms.CheckBox();
            this.ScanSerialNumber = new System.Windows.Forms.Label();
            this.DoubleScanBox = new System.Windows.Forms.CheckBox();
            this.TwainDpi = new System.Windows.Forms.ComboBox();
            this.label16 = new System.Windows.Forms.Label();
            this.TwainSleepTimeSet = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.SelectScanPath = new System.Windows.Forms.Button();
            this.StartScanButton = new System.Windows.Forms.Button();
            this.ScanPictureSavePathBox = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.SetTwainDevice = new System.Windows.Forms.LinkLabel();
            this.SelectTwainDevice = new System.Windows.Forms.LinkLabel();
            this.TabPage_13 = new System.Windows.Forms.TabPage();
            this.groupBox64 = new System.Windows.Forms.GroupBox();
            this.ReleaseRenamerReplaceConfig = new System.Windows.Forms.Button();
            this.ReleaseRenamerApp = new System.Windows.Forms.Button();
            this.groupBox63 = new System.Windows.Forms.GroupBox();
            this.ReleaseKMSAIO = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.groupBox17 = new System.Windows.Forms.GroupBox();
            this.image2ico_remove = new System.Windows.Forms.Button();
            this.IcoFileView_MoveDown = new System.Windows.Forms.Button();
            this.IcoFileView_MoveUp = new System.Windows.Forms.Button();
            this.image2ico_clear = new System.Windows.Forms.Button();
            this.IcoSizeDrop = new System.Windows.Forms.ComboBox();
            this.label22 = new System.Windows.Forms.Label();
            this.Image2IcoStatusText = new System.Windows.Forms.Label();
            this.ConvertImage2Ico = new System.Windows.Forms.Button();
            this.SelectFilesIco = new System.Windows.Forms.Button();
            this.IcoFileView = new System.Windows.Forms.ListView();
            this.groupBox16 = new System.Windows.Forms.GroupBox();
            this.ReleaseRegistry = new System.Windows.Forms.Button();
            this.groupBox15 = new System.Windows.Forms.GroupBox();
            this.LanzouNetfx = new System.Windows.Forms.LinkLabel();
            this.ProcessLabel = new System.Windows.Forms.Label();
            this.Downloadfx35Progress = new System.Windows.Forms.ProgressBar();
            this.DownloadNetfx35 = new System.Windows.Forms.Button();
            this.TabPage_26 = new System.Windows.Forms.TabPage();
            this.blockgroup = new System.Windows.Forms.GroupBox();
            this.OpenSystemUpdate = new System.Windows.Forms.Button();
            this.SetPauseTime = new System.Windows.Forms.Button();
            this.PauseUpdateYears = new System.Windows.Forms.ComboBox();
            this.label153 = new System.Windows.Forms.Label();
            this.groupBox27 = new System.Windows.Forms.GroupBox();
            this.AutoRestartExplorer = new System.Windows.Forms.CheckBox();
            this.ShortStatus = new System.Windows.Forms.Label();
            this.RestoreShort = new System.Windows.Forms.Button();
            this.RemoveShort = new System.Windows.Forms.Button();
            this.groupBox26 = new System.Windows.Forms.GroupBox();
            this.KeyBoardLockStatus = new System.Windows.Forms.Label();
            this.UnlockKeyBoard = new System.Windows.Forms.Button();
            this.LockKeyBoard = new System.Windows.Forms.Button();
            this.TabPage_32 = new System.Windows.Forms.TabPage();
            this.groupBox32 = new System.Windows.Forms.GroupBox();
            this.groupBox40 = new System.Windows.Forms.GroupBox();
            this.votedataview = new System.Windows.Forms.DataGridView();
            this.linkLabel6 = new System.Windows.Forms.LinkLabel();
            this.groupBox33 = new System.Windows.Forms.GroupBox();
            this.votepageIndexCombox = new System.Windows.Forms.ComboBox();
            this.votepage_type_combox = new System.Windows.Forms.ComboBox();
            this.label105 = new System.Windows.Forms.Label();
            this.label95 = new System.Windows.Forms.Label();
            this.voteindex = new System.Windows.Forms.TextBox();
            this.label78 = new System.Windows.Forms.Label();
            this.votename = new System.Windows.Forms.TextBox();
            this.label77 = new System.Windows.Forms.Label();
            this.label76 = new System.Windows.Forms.Label();
            this.voteid = new System.Windows.Forms.TextBox();
            this.votestatustext = new System.Windows.Forms.Label();
            this.StartInsert = new System.Windows.Forms.Button();
            this.SelectVotePic = new System.Windows.Forms.Button();
            this.VotePicture = new System.Windows.Forms.PictureBox();
            this.SelectMss3 = new System.Windows.Forms.Button();
            this.Mss3Path = new System.Windows.Forms.TextBox();
            this.label75 = new System.Windows.Forms.Label();
            this.TabPage_39 = new System.Windows.Forms.TabPage();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.label167 = new System.Windows.Forms.Label();
            this.label166 = new System.Windows.Forms.Label();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.SingleDocumentConfigGroupBox = new System.Windows.Forms.GroupBox();
            this.SignDocumentProgress = new System.Windows.Forms.ProgressBar();
            this.groupBox55 = new System.Windows.Forms.GroupBox();
            this.SignDocumentUseOnlyNumber = new System.Windows.Forms.RadioButton();
            this.SignDocumentUsePersonNumber = new System.Windows.Forms.RadioButton();
            this.SignDocumentUseOnlyPageNumber = new System.Windows.Forms.RadioButton();
            this.SignDocumentFileEndName = new System.Windows.Forms.TextBox();
            this.label162 = new System.Windows.Forms.Label();
            this.label160 = new System.Windows.Forms.Label();
            this.SignDocumentNeedCount = new System.Windows.Forms.NumericUpDown();
            this.SignDocumentPicSourceGroupBox = new System.Windows.Forms.GroupBox();
            this.SignDocumentUseEmbedImage = new System.Windows.Forms.RadioButton();
            this.SignDocumentUseOutPersonPath = new System.Windows.Forms.RadioButton();
            this.SignDocumentUseOutPersonPathTextBox = new System.Windows.Forms.TextBox();
            this.SignDocumentUseOutPersonPathButton = new System.Windows.Forms.Button();
            this.label164 = new System.Windows.Forms.Label();
            this.SignDocumentOnlyUseOnlyImage = new System.Windows.Forms.RadioButton();
            this.SignDocumentOnlyUseOnlyImageTextBox = new System.Windows.Forms.TextBox();
            this.SignDocumentOnlyUseOnlyImageButton = new System.Windows.Forms.Button();
            this.SignDocumentReplaceStart = new System.Windows.Forms.Button();
            this.groupBox58 = new System.Windows.Forms.GroupBox();
            this.label161 = new System.Windows.Forms.Label();
            this.SignDocumentFileSaveBox = new System.Windows.Forms.TextBox();
            this.SignDocumentFileSaveButton = new System.Windows.Forms.Button();
            this.groupBox54 = new System.Windows.Forms.GroupBox();
            this.label165 = new System.Windows.Forms.Label();
            this.SingPersonImageAlt = new System.Windows.Forms.TextBox();
            this.label159 = new System.Windows.Forms.Label();
            this.SignPersionDocumentChooseFile = new System.Windows.Forms.Button();
            this.SignPersionDocument = new System.Windows.Forms.TextBox();
            this.label152 = new System.Windows.Forms.Label();
            this.TabPage_40 = new System.Windows.Forms.TabPage();
            this.linkLabel5 = new System.Windows.Forms.LinkLabel();
            this.label173 = new System.Windows.Forms.Label();
            this.groupBox59 = new System.Windows.Forms.GroupBox();
            this.ExamReportChangeItemDataFormat = new System.Windows.Forms.RadioButton();
            this.ExamReportChangeItemName = new System.Windows.Forms.RadioButton();
            this.ExamReportChangeItemDataRaid = new System.Windows.Forms.RadioButton();
            this.ExamReportChangeProgress = new System.Windows.Forms.ProgressBar();
            this.ExamReplaceRaids = new System.Windows.Forms.TextBox();
            this.ExamReplaceDataFormat = new System.Windows.Forms.ComboBox();
            this.ExamReplaceItemName = new System.Windows.Forms.TextBox();
            this.label172 = new System.Windows.Forms.Label();
            this.label171 = new System.Windows.Forms.Label();
            this.label170 = new System.Windows.Forms.Label();
            this.ExamReportRaids = new System.Windows.Forms.ComboBox();
            this.label169 = new System.Windows.Forms.Label();
            this.label168 = new System.Windows.Forms.Label();
            this.ExamReportItems = new System.Windows.Forms.ComboBox();
            this.label158 = new System.Windows.Forms.Label();
            this.ExamReportDataFormat = new System.Windows.Forms.ComboBox();
            this.ExamReportChange = new System.Windows.Forms.Button();
            this.groupBox56 = new System.Windows.Forms.GroupBox();
            this.ExamRaidsCopy = new System.Windows.Forms.Button();
            this.ExamRaidInfoCombox = new System.Windows.Forms.ComboBox();
            this.label157 = new System.Windows.Forms.Label();
            this.label156 = new System.Windows.Forms.Label();
            this.label155 = new System.Windows.Forms.Label();
            this.ExamItemsCombox = new System.Windows.Forms.ComboBox();
            this.ExamItemsSelectorsCombox = new System.Windows.Forms.ComboBox();
            this.SelectExamFile = new System.Windows.Forms.Button();
            this.ExamFileText = new System.Windows.Forms.TextBox();
            this.label154 = new System.Windows.Forms.Label();
            this.RefreshVFSOFTREPORT = new System.Windows.Forms.Button();
            this.groupBox57 = new System.Windows.Forms.GroupBox();
            this.VFSOFT_Report_ListView = new System.Windows.Forms.ListView();
            this.TabPage_38 = new System.Windows.Forms.TabPage();
            this.groupBox53 = new System.Windows.Forms.GroupBox();
            this.VoteTable_AnotherTableShow = new System.Windows.Forms.CheckBox();
            this.VoteTable_NumberIndex_Keep = new System.Windows.Forms.CheckBox();
            this.VoteTable_KeepPageFooter = new System.Windows.Forms.CheckBox();
            this.VoteTableFontSize = new System.Windows.Forms.NumericUpDown();
            this.label151 = new System.Windows.Forms.Label();
            this.label150 = new System.Windows.Forms.Label();
            this.VoteTableHeight = new System.Windows.Forms.NumericUpDown();
            this.label149 = new System.Windows.Forms.Label();
            this.VoteTable_Random_PageCounts = new System.Windows.Forms.CheckBox();
            this.label142 = new System.Windows.Forms.Label();
            this.VoteTable_Render = new System.Windows.Forms.Button();
            this.报告单生成日期 = new System.Windows.Forms.GroupBox();
            this.VoteTableHelpDate = new System.Windows.Forms.Button();
            this.VoteTable_GenDate = new System.Windows.Forms.TextBox();
            this.报告单结束语 = new System.Windows.Forms.GroupBox();
            this.VoteTable_End = new System.Windows.Forms.TextBox();
            this.groupBox52 = new System.Windows.Forms.GroupBox();
            this.VoteTable_Start = new System.Windows.Forms.TextBox();
            this.groupBox51 = new System.Windows.Forms.GroupBox();
            this.VoteTable_Title = new System.Windows.Forms.TextBox();
            this.groupBox50 = new System.Windows.Forms.GroupBox();
            this.VoteTableName_CountText = new System.Windows.Forms.Label();
            this.VoteTable_Names = new System.Windows.Forms.RichTextBox();
            this.groupBox49 = new System.Windows.Forms.GroupBox();
            this.VoteTable_Abdon = new System.Windows.Forms.TextBox();
            this.label141 = new System.Windows.Forms.Label();
            this.VoteTable_UnAgree = new System.Windows.Forms.TextBox();
            this.label140 = new System.Windows.Forms.Label();
            this.VoteTable_Agree = new System.Windows.Forms.TextBox();
            this.label139 = new System.Windows.Forms.Label();
            this.VoteTable_Name = new System.Windows.Forms.TextBox();
            this.label138 = new System.Windows.Forms.Label();
            this.TabPage_36 = new System.Windows.Forms.TabPage();
            this.groupBox60 = new System.Windows.Forms.GroupBox();
            this.label163 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.GetNumberIndexList = new System.Windows.Forms.Button();
            this.NumberIndexEnd = new System.Windows.Forms.NumericUpDown();
            this.NumberIndexStart = new System.Windows.Forms.NumericUpDown();
            this.CopyNumberIndexResult = new System.Windows.Forms.Button();
            this.NumberIndexResultBox = new System.Windows.Forms.RichTextBox();
            this.groupBox28 = new System.Windows.Forms.GroupBox();
            this.label175 = new System.Windows.Forms.Label();
            this.GetRandomComputerNameRandLength = new System.Windows.Forms.NumericUpDown();
            this.GetRandomComputerNameButton = new System.Windows.Forms.Button();
            this.ExamComputerNameChange = new System.Windows.Forms.Button();
            this.ExamComputerChangeName = new System.Windows.Forms.TextBox();
            this.label174 = new System.Windows.Forms.Label();
            this.ExamComputerNames = new System.Windows.Forms.ComboBox();
            this.label58 = new System.Windows.Forms.Label();
            this.ChooseExamFileButton = new System.Windows.Forms.Button();
            this.ChooseExamFileTextBox = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox46 = new System.Windows.Forms.GroupBox();
            this.money_big = new System.Windows.Forms.TextBox();
            this.money_small = new System.Windows.Forms.TextBox();
            this.label131 = new System.Windows.Forms.Label();
            this.label130 = new System.Windows.Forms.Label();
            this.copy_money_big_result = new System.Windows.Forms.Button();
            this.groupBox43 = new System.Windows.Forms.GroupBox();
            this.copied_number_prefix_count = new System.Windows.Forms.TextBox();
            this.label124 = new System.Windows.Forms.Label();
            this.CopiedNamerText = new System.Windows.Forms.Label();
            this.Gen_Copied_Namer = new System.Windows.Forms.TextBox();
            this.label109 = new System.Windows.Forms.Label();
            this.Gen_Copied_StatusText = new System.Windows.Forms.Label();
            this.Gen_Copied_Number = new System.Windows.Forms.NumericUpDown();
            this.CopiedGen = new System.Windows.Forms.Button();
            this.label108 = new System.Windows.Forms.Label();
            this.Gen_Copied_Path = new System.Windows.Forms.Button();
            this.Gen_Copied_File = new System.Windows.Forms.Button();
            this.Gen_Copied_TargetPath = new System.Windows.Forms.TextBox();
            this.Gen_Copied_SourceFileBox = new System.Windows.Forms.TextBox();
            this.label107 = new System.Windows.Forms.Label();
            this.label106 = new System.Windows.Forms.Label();
            this.TabPage_37 = new System.Windows.Forms.TabPage();
            this.label123 = new System.Windows.Forms.Label();
            this.groupBox45 = new System.Windows.Forms.GroupBox();
            this.set_interface_load = new System.Windows.Forms.Button();
            this.set_interface_export = new System.Windows.Forms.Button();
            this.set_interface_reset = new System.Windows.Forms.Button();
            this.set_interface_start = new System.Windows.Forms.Button();
            this.set_interface_dns2 = new DocumentTools.IPAddressInputTextBox();
            this.set_interface_dns1 = new DocumentTools.IPAddressInputTextBox();
            this.set_interface_gateway = new DocumentTools.IPAddressInputTextBox();
            this.set_interface_mask = new DocumentTools.IPAddressInputTextBox();
            this.set_interface_ipaddress = new DocumentTools.IPAddressInputTextBox();
            this.set_interface_mode_static = new System.Windows.Forms.RadioButton();
            this.set_interface_mode_auto = new System.Windows.Forms.RadioButton();
            this.label121 = new System.Windows.Forms.Label();
            this.label120 = new System.Windows.Forms.Label();
            this.label119 = new System.Windows.Forms.Label();
            this.label118 = new System.Windows.Forms.Label();
            this.label117 = new System.Windows.Forms.Label();
            this.set_interface_name_list = new System.Windows.Forms.ComboBox();
            this.label116 = new System.Windows.Forms.Label();
            this.groupBox44 = new System.Windows.Forms.GroupBox();
            this.copy_network_info_ip_address = new System.Windows.Forms.Button();
            this.copy_network_info_mac_address = new System.Windows.Forms.Button();
            this.network_info_mac_address = new System.Windows.Forms.ComboBox();
            this.label122 = new System.Windows.Forms.Label();
            this.network_open_ncap = new System.Windows.Forms.Button();
            this.update_network_info = new System.Windows.Forms.Button();
            this.network_info_show_normal = new System.Windows.Forms.CheckBox();
            this.Disable_NetworkCard = new System.Windows.Forms.Button();
            this.Enable_NetworkCard = new System.Windows.Forms.Button();
            this.network_info_dns2 = new System.Windows.Forms.ComboBox();
            this.network_info_dns1 = new System.Windows.Forms.ComboBox();
            this.network_info_gateway = new System.Windows.Forms.ComboBox();
            this.network_info_mask = new System.Windows.Forms.ComboBox();
            this.network_info_ipv4 = new System.Windows.Forms.ComboBox();
            this.network_info_card_list = new System.Windows.Forms.ComboBox();
            this.label115 = new System.Windows.Forms.Label();
            this.label114 = new System.Windows.Forms.Label();
            this.label113 = new System.Windows.Forms.Label();
            this.label112 = new System.Windows.Forms.Label();
            this.label111 = new System.Windows.Forms.Label();
            this.label110 = new System.Windows.Forms.Label();
            this.TabPage_08 = new System.Windows.Forms.TabPage();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox9 = new System.Windows.Forms.PictureBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.PlayCXKMusic = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label13 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.DisableAdminBehavior = new System.Windows.Forms.Button();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.DontateMe = new System.Windows.Forms.Button();
            this.UseLibList = new System.Windows.Forms.Button();
            this.DownloadFrameWorkLabel = new System.Windows.Forms.LinkLabel();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.ListViewRightMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.列表操作ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.删除本项ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.清空列表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.文件操作ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开源文件目录ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开源文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开转换后目录ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开转换后文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.获取PDF文件版本信息ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MemoryTimer = new System.Windows.Forms.Timer(this.components);
            this.TreeViewContentMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.展开折叠本节点全部ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.折叠本节点全部内容ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.展开折叠全部节点ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.折叠全部节点ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ColorSelectDialog = new System.Windows.Forms.ColorDialog();
            this.FontSelectDialog = new System.Windows.Forms.FontDialog();
            this.MainFormError = new System.Windows.Forms.ErrorProvider(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.liblist = new System.Windows.Forms.ToolStripMenuItem();
            this.wORD功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wORD转PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wORD转EXCELToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wORDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wORD内容批量替换ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eXCEL功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eXCEL转PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eXCEL转WORDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eXCEL内容批量替换ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pPT功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pPT转PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pDF功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pDF转EXCELToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pDF转PPTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.添加PDF水印ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.移除PDF水印ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pDF版本转换ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ePUB功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ePUB转WORDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ePUB转PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hTML功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hTML转PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.拆分合并功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.拆分WORDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.拆分PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.合并WORDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.合并PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.拆分PPTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.压缩功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.压缩PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.压缩图片ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.图片功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wORDEXCELPDF转图片ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.图片转PDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wEBP图片格式转换ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sYX功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.姓名横转竖列ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.特殊二维码校验ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.twain扫描仪调试ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mSS3模板修改ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.简易二维码生成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.随机身份证号码生成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.文件副本生成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.网卡设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.选举报告单样式生成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.单人表生成ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.考评报表修改ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.其他功能ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.系统工具ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.关于软件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.软件设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_start_page_index = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_start_page_index_combox = new System.Windows.Forms.ToolStripComboBox();
            this.menu_tabpage_heighlight = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_tabpage_heigh_light_combox = new System.Windows.Forms.ToolStripComboBox();
            this.menu_tabpage_icon = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_tabpage_icon_combox = new System.Windows.Forms.ToolStripComboBox();
            this.menu_confirm_dialog = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_finshined_messbox_combox = new System.Windows.Forms.ToolStripComboBox();
            this.menu_listview_height = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_listview_line_heigh_combox = new System.Windows.Forms.ToolStripComboBox();
            this.menu_listview_gridline = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_listview_gridline_combox = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_chrome_path = new System.Windows.Forms.ToolStripTextBox();
            this.vFSOFT数据包路径配置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.考评数据包路径ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vfsoft_examdata_path_textbox = new System.Windows.Forms.ToolStripTextBox();
            this.选举数据包路径ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.vfsoft_mssdata_path_textbox = new System.Windows.Forms.ToolStripTextBox();
            this.界面大小设置ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.设置界面宽度ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bodyform_width = new System.Windows.Forms.ToolStripTextBox();
            this.设置界面高度ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bodyform_height = new System.Windows.Forms.ToolStripTextBox();
            this.控件锚点模式ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menu_widget_anchor_combox = new System.Windows.Forms.ToolStripComboBox();
            this.查看标签索引对照表ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.初始化配置文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.保存设置并重启软件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.程序更新ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CheckVersionButton = new System.Windows.Forms.ToolStripMenuItem();
            this.VersionUpdateLog = new System.Windows.Forms.ToolStripMenuItem();
            this.MainToolsTip = new System.Windows.Forms.ToolTip(this.components);
            this.TabPageIcoList = new System.Windows.Forms.ImageList(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.MemoryStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.MemoryProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.UpdateCheckText = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainTabPage.SuspendLayout();
            this.TabPage_01.SuspendLayout();
            this.TabPage_02.SuspendLayout();
            this.TabPage_03.SuspendLayout();
            this.TabPage_07.SuspendLayout();
            this.TabPage_20.SuspendLayout();
            this.TabPage_21.SuspendLayout();
            this.TabPage_14.SuspendLayout();
            this.TabPage_31.SuspendLayout();
            this.groupBox48.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitWord_PageCount)).BeginInit();
            this.TabPage_06.SuspendLayout();
            this.TabPage_17.SuspendLayout();
            this.groupBox47.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitPdf_PerPageCount)).BeginInit();
            this.TabPage_41.SuspendLayout();
            this.groupBox62.SuspendLayout();
            this.groupBox61.SuspendLayout();
            this.TabPage_18.SuspendLayout();
            this.groupBox18.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WaterMarkImage)).BeginInit();
            this.TabPage_19.SuspendLayout();
            this.TabPage_22.SuspendLayout();
            this.TabPage_15.SuspendLayout();
            this.TabPage_16.SuspendLayout();
            this.TabPage_04.SuspendLayout();
            this.TabPage_27.SuspendLayout();
            this.TabPage_28.SuspendLayout();
            this.TabPage_29.SuspendLayout();
            this.TabPage_23.SuspendLayout();
            this.groupBox19.SuspendLayout();
            this.groupBox20.SuspendLayout();
            this.TabPage_30.SuspendLayout();
            this.groupBox31.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.html2pdf_wait_timeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.html2pdf_timeout)).BeginInit();
            this.groupBox30.SuspendLayout();
            this.groupBox29.SuspendLayout();
            this.TabPage_24.SuspendLayout();
            this.groupBox21.SuspendLayout();
            this.TabPage_25.SuspendLayout();
            this.groupBox25.SuspendLayout();
            this.groupBox24.SuspendLayout();
            this.groupBox23.SuspendLayout();
            this.groupBox22.SuspendLayout();
            this.TabPage_10.SuspendLayout();
            this.groupBox10.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.TabPage_11.SuspendLayout();
            this.groupBox13.SuspendLayout();
            this.groupBox12.SuspendLayout();
            this.groupBox11.SuspendLayout();
            this.TabPage_12.SuspendLayout();
            this.groupBox14.SuspendLayout();
            this.TabPage_33.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barcodeRegionPicture)).BeginInit();
            this.groupBox36.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barcodePreviewBox)).BeginInit();
            this.groupBox38.SuspendLayout();
            this.groupBox37.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.barcodeMutilList)).BeginInit();
            this.groupBox35.SuspendLayout();
            this.groupBox34.SuspendLayout();
            this.TabPage_34.SuspendLayout();
            this.groupBox41.SuspendLayout();
            this.groupBox42.SuspendLayout();
            this.gen_card_result_group.SuspendLayout();
            this.gen_card_id_group.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gen_card_number)).BeginInit();
            this.TabPage_05.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.TabPage_35.SuspendLayout();
            this.groupBox39.SuspendLayout();
            this.TabPage_09.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).BeginInit();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.TabPage_13.SuspendLayout();
            this.groupBox64.SuspendLayout();
            this.groupBox63.SuspendLayout();
            this.groupBox17.SuspendLayout();
            this.groupBox16.SuspendLayout();
            this.groupBox15.SuspendLayout();
            this.TabPage_26.SuspendLayout();
            this.blockgroup.SuspendLayout();
            this.groupBox27.SuspendLayout();
            this.groupBox26.SuspendLayout();
            this.TabPage_32.SuspendLayout();
            this.groupBox32.SuspendLayout();
            this.groupBox40.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.votedataview)).BeginInit();
            this.groupBox33.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VotePicture)).BeginInit();
            this.TabPage_39.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            this.SingleDocumentConfigGroupBox.SuspendLayout();
            this.groupBox55.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SignDocumentNeedCount)).BeginInit();
            this.SignDocumentPicSourceGroupBox.SuspendLayout();
            this.groupBox58.SuspendLayout();
            this.groupBox54.SuspendLayout();
            this.TabPage_40.SuspendLayout();
            this.groupBox59.SuspendLayout();
            this.groupBox56.SuspendLayout();
            this.groupBox57.SuspendLayout();
            this.TabPage_38.SuspendLayout();
            this.groupBox53.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VoteTableFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.VoteTableHeight)).BeginInit();
            this.报告单生成日期.SuspendLayout();
            this.报告单结束语.SuspendLayout();
            this.groupBox52.SuspendLayout();
            this.groupBox51.SuspendLayout();
            this.groupBox50.SuspendLayout();
            this.groupBox49.SuspendLayout();
            this.TabPage_36.SuspendLayout();
            this.groupBox60.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberIndexEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumberIndexStart)).BeginInit();
            this.groupBox28.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GetRandomComputerNameRandLength)).BeginInit();
            this.groupBox46.SuspendLayout();
            this.groupBox43.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Gen_Copied_Number)).BeginInit();
            this.TabPage_37.SuspendLayout();
            this.groupBox45.SuspendLayout();
            this.groupBox44.SuspendLayout();
            this.TabPage_08.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            this.ListViewRightMenu.SuspendLayout();
            this.TreeViewContentMenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainFormError)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabPage
            // 
            this.MainTabPage.Controls.Add(this.TabPage_01);
            this.MainTabPage.Controls.Add(this.TabPage_02);
            this.MainTabPage.Controls.Add(this.TabPage_03);
            this.MainTabPage.Controls.Add(this.TabPage_07);
            this.MainTabPage.Controls.Add(this.TabPage_20);
            this.MainTabPage.Controls.Add(this.TabPage_21);
            this.MainTabPage.Controls.Add(this.TabPage_14);
            this.MainTabPage.Controls.Add(this.TabPage_31);
            this.MainTabPage.Controls.Add(this.TabPage_06);
            this.MainTabPage.Controls.Add(this.TabPage_17);
            this.MainTabPage.Controls.Add(this.TabPage_41);
            this.MainTabPage.Controls.Add(this.TabPage_18);
            this.MainTabPage.Controls.Add(this.TabPage_19);
            this.MainTabPage.Controls.Add(this.TabPage_22);
            this.MainTabPage.Controls.Add(this.TabPage_15);
            this.MainTabPage.Controls.Add(this.TabPage_16);
            this.MainTabPage.Controls.Add(this.TabPage_04);
            this.MainTabPage.Controls.Add(this.TabPage_27);
            this.MainTabPage.Controls.Add(this.TabPage_28);
            this.MainTabPage.Controls.Add(this.TabPage_29);
            this.MainTabPage.Controls.Add(this.TabPage_23);
            this.MainTabPage.Controls.Add(this.TabPage_30);
            this.MainTabPage.Controls.Add(this.TabPage_24);
            this.MainTabPage.Controls.Add(this.TabPage_25);
            this.MainTabPage.Controls.Add(this.TabPage_10);
            this.MainTabPage.Controls.Add(this.TabPage_11);
            this.MainTabPage.Controls.Add(this.TabPage_12);
            this.MainTabPage.Controls.Add(this.TabPage_33);
            this.MainTabPage.Controls.Add(this.TabPage_34);
            this.MainTabPage.Controls.Add(this.TabPage_05);
            this.MainTabPage.Controls.Add(this.TabPage_35);
            this.MainTabPage.Controls.Add(this.TabPage_09);
            this.MainTabPage.Controls.Add(this.TabPage_13);
            this.MainTabPage.Controls.Add(this.TabPage_26);
            this.MainTabPage.Controls.Add(this.TabPage_32);
            this.MainTabPage.Controls.Add(this.TabPage_39);
            this.MainTabPage.Controls.Add(this.TabPage_40);
            this.MainTabPage.Controls.Add(this.TabPage_38);
            this.MainTabPage.Controls.Add(this.TabPage_36);
            this.MainTabPage.Controls.Add(this.TabPage_37);
            this.MainTabPage.Controls.Add(this.TabPage_08);
            this.MainTabPage.Location = new System.Drawing.Point(0, 26);
            this.MainTabPage.Margin = new System.Windows.Forms.Padding(1);
            this.MainTabPage.Multiline = true;
            this.MainTabPage.Name = "MainTabPage";
            this.MainTabPage.SelectedIndex = 0;
            this.MainTabPage.Size = new System.Drawing.Size(890, 739);
            this.MainTabPage.SizeMode = System.Windows.Forms.TabSizeMode.FillToRight;
            this.MainTabPage.TabIndex = 0;
            this.MainTabPage.Tag = "anchor_full_fill";
            this.MainTabPage.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.MainTabPage_DrawItem);
            this.MainTabPage.SelectedIndexChanged += new System.EventHandler(this.MainTabPage_SelectedIndexChanged);
            this.MainTabPage.DoubleClick += new System.EventHandler(this.MainTabPage_DoubleClick);
            // 
            // TabPage_01
            // 
            this.TabPage_01.Controls.Add(this.UseLocalOffice);
            this.TabPage_01.Controls.Add(this.WORD2PDF_LoadPassword);
            this.TabPage_01.Controls.Add(this.label147);
            this.TabPage_01.Controls.Add(this.EmbedFonts);
            this.TabPage_01.Controls.Add(this.word2pdf_colorMode);
            this.TabPage_01.Controls.Add(this.label125);
            this.TabPage_01.Controls.Add(this.word2pdf_need_update_fields);
            this.TabPage_01.Controls.Add(this.word2pdf_save_source);
            this.TabPage_01.Controls.Add(this.Word2PDFOptimizCombox);
            this.TabPage_01.Controls.Add(this.label62);
            this.TabPage_01.Controls.Add(this.word2pdf_clear);
            this.TabPage_01.Controls.Add(this.word2pdf_remove);
            this.TabPage_01.Controls.Add(this.WordFileListView_MoveDown);
            this.TabPage_01.Controls.Add(this.WordFileListView_MoveUp);
            this.TabPage_01.Controls.Add(this.label34);
            this.TabPage_01.Controls.Add(this.PDFComplianceBox);
            this.TabPage_01.Controls.Add(this.RemoveTempDocx);
            this.TabPage_01.Controls.Add(this.OpenPdfOptions);
            this.TabPage_01.Controls.Add(this.StatusText);
            this.TabPage_01.Controls.Add(this.ConvertButton);
            this.TabPage_01.Controls.Add(this.ChooseDocButton);
            this.TabPage_01.Controls.Add(this.WordFileListView);
            this.TabPage_01.Location = new System.Drawing.Point(4, 76);
            this.TabPage_01.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_01.Name = "TabPage_01";
            this.TabPage_01.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_01.Size = new System.Drawing.Size(882, 659);
            this.TabPage_01.TabIndex = 0;
            this.TabPage_01.Text = "WORD转PDF";
            this.TabPage_01.UseVisualStyleBackColor = true;
            // 
            // UseLocalOffice
            // 
            this.UseLocalOffice.AutoSize = true;
            this.UseLocalOffice.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.UseLocalOffice.Location = new System.Drawing.Point(255, 466);
            this.UseLocalOffice.Margin = new System.Windows.Forms.Padding(1);
            this.UseLocalOffice.Name = "UseLocalOffice";
            this.UseLocalOffice.Size = new System.Drawing.Size(132, 16);
            this.UseLocalOffice.TabIndex = 22;
            this.UseLocalOffice.Tag = "anchor_left_bottom";
            this.UseLocalOffice.Text = "使用本机Office转换";
            this.MainToolsTip.SetToolTip(this.UseLocalOffice, resources.GetString("UseLocalOffice.ToolTip"));
            this.UseLocalOffice.UseVisualStyleBackColor = true;
            this.UseLocalOffice.CheckedChanged += new System.EventHandler(this.UseLocalOffice_CheckedChanged);
            // 
            // WORD2PDF_LoadPassword
            // 
            this.WORD2PDF_LoadPassword.Location = new System.Drawing.Point(489, 461);
            this.WORD2PDF_LoadPassword.Margin = new System.Windows.Forms.Padding(1);
            this.WORD2PDF_LoadPassword.Name = "WORD2PDF_LoadPassword";
            this.WORD2PDF_LoadPassword.Size = new System.Drawing.Size(117, 21);
            this.WORD2PDF_LoadPassword.TabIndex = 21;
            this.WORD2PDF_LoadPassword.Tag = "anchor_right_bottom";
            // 
            // label147
            // 
            this.label147.AutoSize = true;
            this.label147.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label147.Location = new System.Drawing.Point(432, 465);
            this.label147.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label147.Name = "label147";
            this.label147.Size = new System.Drawing.Size(53, 12);
            this.label147.TabIndex = 20;
            this.label147.Tag = "anchor_right_bottom";
            this.label147.Text = "WORD密码";
            // 
            // EmbedFonts
            // 
            this.EmbedFonts.AutoSize = true;
            this.EmbedFonts.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.EmbedFonts.Location = new System.Drawing.Point(4, 465);
            this.EmbedFonts.Margin = new System.Windows.Forms.Padding(1);
            this.EmbedFonts.Name = "EmbedFonts";
            this.EmbedFonts.Size = new System.Drawing.Size(72, 16);
            this.EmbedFonts.TabIndex = 19;
            this.EmbedFonts.Tag = "anchor_left_bottom";
            this.EmbedFonts.Text = "嵌入字体";
            this.EmbedFonts.UseVisualStyleBackColor = true;
            // 
            // word2pdf_colorMode
            // 
            this.word2pdf_colorMode.FormattingEnabled = true;
            this.word2pdf_colorMode.Location = new System.Drawing.Point(763, 461);
            this.word2pdf_colorMode.Margin = new System.Windows.Forms.Padding(1);
            this.word2pdf_colorMode.Name = "word2pdf_colorMode";
            this.word2pdf_colorMode.Size = new System.Drawing.Size(83, 20);
            this.word2pdf_colorMode.TabIndex = 18;
            this.word2pdf_colorMode.Tag = "anchor_right_bottom";
            // 
            // label125
            // 
            this.label125.AutoSize = true;
            this.label125.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label125.Location = new System.Drawing.Point(687, 465);
            this.label125.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label125.Name = "label125";
            this.label125.Size = new System.Drawing.Size(71, 12);
            this.label125.TabIndex = 17;
            this.label125.Tag = "anchor_right_bottom";
            this.label125.Text = "PDF颜色模式";
            // 
            // word2pdf_need_update_fields
            // 
            this.word2pdf_need_update_fields.AutoSize = true;
            this.word2pdf_need_update_fields.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2pdf_need_update_fields.Location = new System.Drawing.Point(103, 465);
            this.word2pdf_need_update_fields.Margin = new System.Windows.Forms.Padding(1);
            this.word2pdf_need_update_fields.Name = "word2pdf_need_update_fields";
            this.word2pdf_need_update_fields.Size = new System.Drawing.Size(84, 16);
            this.word2pdf_need_update_fields.TabIndex = 16;
            this.word2pdf_need_update_fields.Tag = "anchor_left_bottom";
            this.word2pdf_need_update_fields.Text = "自动更新域";
            this.word2pdf_need_update_fields.UseVisualStyleBackColor = true;
            // 
            // word2pdf_save_source
            // 
            this.word2pdf_save_source.AutoSize = true;
            this.word2pdf_save_source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2pdf_save_source.Location = new System.Drawing.Point(4, 436);
            this.word2pdf_save_source.Margin = new System.Windows.Forms.Padding(1);
            this.word2pdf_save_source.Name = "word2pdf_save_source";
            this.word2pdf_save_source.Size = new System.Drawing.Size(96, 16);
            this.word2pdf_save_source.TabIndex = 15;
            this.word2pdf_save_source.Tag = "anchor_left_bottom";
            this.word2pdf_save_source.Text = "保存到源目录";
            this.word2pdf_save_source.UseVisualStyleBackColor = true;
            // 
            // Word2PDFOptimizCombox
            // 
            this.Word2PDFOptimizCombox.FormattingEnabled = true;
            this.Word2PDFOptimizCombox.Location = new System.Drawing.Point(763, 433);
            this.Word2PDFOptimizCombox.Margin = new System.Windows.Forms.Padding(1);
            this.Word2PDFOptimizCombox.Name = "Word2PDFOptimizCombox";
            this.Word2PDFOptimizCombox.Size = new System.Drawing.Size(83, 20);
            this.Word2PDFOptimizCombox.TabIndex = 14;
            this.Word2PDFOptimizCombox.Tag = "anchor_right_bottom";
            // 
            // label62
            // 
            this.label62.AutoSize = true;
            this.label62.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label62.Location = new System.Drawing.Point(681, 436);
            this.label62.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label62.Name = "label62";
            this.label62.Size = new System.Drawing.Size(77, 12);
            this.label62.TabIndex = 13;
            this.label62.Tag = "anchor_right_bottom";
            this.label62.Text = "设置兼容版本";
            // 
            // word2pdf_clear
            // 
            this.word2pdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.word2pdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.word2pdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2pdf_clear.Location = new System.Drawing.Point(847, 225);
            this.word2pdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.word2pdf_clear.Name = "word2pdf_clear";
            this.word2pdf_clear.Size = new System.Drawing.Size(23, 57);
            this.word2pdf_clear.TabIndex = 12;
            this.word2pdf_clear.Tag = "anchor_right";
            this.word2pdf_clear.UseVisualStyleBackColor = true;
            // 
            // word2pdf_remove
            // 
            this.word2pdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.word2pdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.word2pdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2pdf_remove.Location = new System.Drawing.Point(847, 145);
            this.word2pdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.word2pdf_remove.Name = "word2pdf_remove";
            this.word2pdf_remove.Size = new System.Drawing.Size(23, 57);
            this.word2pdf_remove.TabIndex = 11;
            this.word2pdf_remove.Tag = "anchor_right";
            this.word2pdf_remove.UseVisualStyleBackColor = true;
            // 
            // WordFileListView_MoveDown
            // 
            this.WordFileListView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.WordFileListView_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.WordFileListView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.WordFileListView_MoveDown.Name = "WordFileListView_MoveDown";
            this.WordFileListView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.WordFileListView_MoveDown.TabIndex = 10;
            this.WordFileListView_MoveDown.Tag = "anchor_right";
            this.WordFileListView_MoveDown.Text = "▽";
            this.WordFileListView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // WordFileListView_MoveUp
            // 
            this.WordFileListView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.WordFileListView_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.WordFileListView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.WordFileListView_MoveUp.Name = "WordFileListView_MoveUp";
            this.WordFileListView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.WordFileListView_MoveUp.TabIndex = 9;
            this.WordFileListView_MoveUp.Tag = "anchor_right";
            this.WordFileListView_MoveUp.Text = "△";
            this.WordFileListView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label34.Location = new System.Drawing.Point(439, 436);
            this.label34.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(47, 12);
            this.label34.TabIndex = 8;
            this.label34.Tag = "anchor_right_bottom";
            this.label34.Text = "PDF标准";
            // 
            // PDFComplianceBox
            // 
            this.PDFComplianceBox.FormattingEnabled = true;
            this.PDFComplianceBox.Location = new System.Drawing.Point(489, 433);
            this.PDFComplianceBox.Margin = new System.Windows.Forms.Padding(1);
            this.PDFComplianceBox.Name = "PDFComplianceBox";
            this.PDFComplianceBox.Size = new System.Drawing.Size(117, 20);
            this.PDFComplianceBox.TabIndex = 7;
            this.PDFComplianceBox.Tag = "anchor_right_bottom";
            // 
            // RemoveTempDocx
            // 
            this.RemoveTempDocx.AutoSize = true;
            this.RemoveTempDocx.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemoveTempDocx.Location = new System.Drawing.Point(255, 436);
            this.RemoveTempDocx.Margin = new System.Windows.Forms.Padding(1);
            this.RemoveTempDocx.Name = "RemoveTempDocx";
            this.RemoveTempDocx.Size = new System.Drawing.Size(180, 16);
            this.RemoveTempDocx.TabIndex = 6;
            this.RemoveTempDocx.Tag = "anchor_left_bottom";
            this.RemoveTempDocx.Text = "处理完毕后移除TempDocx目录";
            this.RemoveTempDocx.UseVisualStyleBackColor = true;
            // 
            // OpenPdfOptions
            // 
            this.OpenPdfOptions.AutoSize = true;
            this.OpenPdfOptions.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.OpenPdfOptions.Location = new System.Drawing.Point(103, 436);
            this.OpenPdfOptions.Margin = new System.Windows.Forms.Padding(1);
            this.OpenPdfOptions.Name = "OpenPdfOptions";
            this.OpenPdfOptions.Size = new System.Drawing.Size(150, 16);
            this.OpenPdfOptions.TabIndex = 5;
            this.OpenPdfOptions.Tag = "anchor_left_bottom";
            this.OpenPdfOptions.Text = "处理完毕后打开PDF文件";
            this.OpenPdfOptions.UseVisualStyleBackColor = true;
            // 
            // StatusText
            // 
            this.StatusText.AutoSize = true;
            this.StatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.StatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StatusText.Location = new System.Drawing.Point(156, 575);
            this.StatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(95, 12);
            this.StatusText.TabIndex = 4;
            this.StatusText.Tag = "anchor_left_bottom";
            this.StatusText.Text = "正在处理X个文件";
            // 
            // ConvertButton
            // 
            this.ConvertButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertButton.Location = new System.Drawing.Point(697, 561);
            this.ConvertButton.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertButton.Name = "ConvertButton";
            this.ConvertButton.Size = new System.Drawing.Size(147, 37);
            this.ConvertButton.TabIndex = 2;
            this.ConvertButton.Tag = "anchor_right_bottom";
            this.ConvertButton.Text = "开始处理";
            this.ConvertButton.UseVisualStyleBackColor = true;
            this.ConvertButton.Click += new System.EventHandler(this.ConvertButton_Click);
            // 
            // ChooseDocButton
            // 
            this.ChooseDocButton.AutoSize = true;
            this.ChooseDocButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ChooseDocButton.Location = new System.Drawing.Point(5, 561);
            this.ChooseDocButton.Margin = new System.Windows.Forms.Padding(1);
            this.ChooseDocButton.Name = "ChooseDocButton";
            this.ChooseDocButton.Size = new System.Drawing.Size(147, 37);
            this.ChooseDocButton.TabIndex = 1;
            this.ChooseDocButton.Tag = "anchor_left_bottom";
            this.ChooseDocButton.Text = "选择文件";
            this.ChooseDocButton.UseVisualStyleBackColor = true;
            this.ChooseDocButton.Click += new System.EventHandler(this.ChooseDocButton_Click);
            // 
            // WordFileListView
            // 
            this.WordFileListView.HideSelection = false;
            this.WordFileListView.Location = new System.Drawing.Point(4, 4);
            this.WordFileListView.Margin = new System.Windows.Forms.Padding(1);
            this.WordFileListView.Name = "WordFileListView";
            this.WordFileListView.Size = new System.Drawing.Size(841, 420);
            this.WordFileListView.TabIndex = 0;
            this.WordFileListView.Tag = "anchor_full_fill";
            this.WordFileListView.UseCompatibleStateImageBehavior = false;
            this.WordFileListView.View = System.Windows.Forms.View.List;
            // 
            // TabPage_02
            // 
            this.TabPage_02.Controls.Add(this.PDF2WORD_LoadPassword);
            this.TabPage_02.Controls.Add(this.label148);
            this.TabPage_02.Controls.Add(this.pdf2word_save_source);
            this.TabPage_02.Controls.Add(this.label1);
            this.TabPage_02.Controls.Add(this.PDF2WORD_Mode);
            this.TabPage_02.Controls.Add(this.pdf2word_clear);
            this.TabPage_02.Controls.Add(this.pdf2word_remove);
            this.TabPage_02.Controls.Add(this.label55);
            this.TabPage_02.Controls.Add(this.PDF2WORD_UseSUbOcr);
            this.TabPage_02.Controls.Add(this.PDF2WORD_UseBullet);
            this.TabPage_02.Controls.Add(this.PdfFileListView_MoveDown);
            this.TabPage_02.Controls.Add(this.PdfFileListView_MoveUp);
            this.TabPage_02.Controls.Add(this.ProcessPage_P2W);
            this.TabPage_02.Controls.Add(this.PdfStatusText);
            this.TabPage_02.Controls.Add(this.Convert2DocButton);
            this.TabPage_02.Controls.Add(this.SelectPdfButton);
            this.TabPage_02.Controls.Add(this.PdfFileListView);
            this.TabPage_02.Location = new System.Drawing.Point(4, 76);
            this.TabPage_02.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_02.Name = "TabPage_02";
            this.TabPage_02.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_02.Size = new System.Drawing.Size(882, 659);
            this.TabPage_02.TabIndex = 1;
            this.TabPage_02.Text = "PDF转WORD";
            this.TabPage_02.UseVisualStyleBackColor = true;
            // 
            // PDF2WORD_LoadPassword
            // 
            this.PDF2WORD_LoadPassword.Location = new System.Drawing.Point(727, 431);
            this.PDF2WORD_LoadPassword.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2WORD_LoadPassword.Name = "PDF2WORD_LoadPassword";
            this.PDF2WORD_LoadPassword.Size = new System.Drawing.Size(119, 21);
            this.PDF2WORD_LoadPassword.TabIndex = 18;
            this.PDF2WORD_LoadPassword.Tag = "anchor_right_bottom";
            // 
            // label148
            // 
            this.label148.AutoSize = true;
            this.label148.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label148.Location = new System.Drawing.Point(675, 435);
            this.label148.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label148.Name = "label148";
            this.label148.Size = new System.Drawing.Size(47, 12);
            this.label148.TabIndex = 17;
            this.label148.Tag = "anchor_right_bottom";
            this.label148.Text = "PDF密码";
            // 
            // pdf2word_save_source
            // 
            this.pdf2word_save_source.AutoSize = true;
            this.pdf2word_save_source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2word_save_source.Location = new System.Drawing.Point(4, 433);
            this.pdf2word_save_source.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2word_save_source.Name = "pdf2word_save_source";
            this.pdf2word_save_source.Size = new System.Drawing.Size(96, 16);
            this.pdf2word_save_source.TabIndex = 16;
            this.pdf2word_save_source.Tag = "anchor_left_bottom";
            this.pdf2word_save_source.Text = "保存到源目录";
            this.pdf2word_save_source.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Tomato;
            this.label1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label1.Location = new System.Drawing.Point(4, 471);
            this.label1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 12);
            this.label1.TabIndex = 12;
            this.label1.Tag = "anchor_left_bottom";
            this.label1.Text = "注：不支持扫描件PDF";
            // 
            // PDF2WORD_Mode
            // 
            this.PDF2WORD_Mode.FormattingEnabled = true;
            this.PDF2WORD_Mode.Location = new System.Drawing.Point(379, 432);
            this.PDF2WORD_Mode.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2WORD_Mode.Name = "PDF2WORD_Mode";
            this.PDF2WORD_Mode.Size = new System.Drawing.Size(293, 20);
            this.PDF2WORD_Mode.TabIndex = 11;
            this.PDF2WORD_Mode.Tag = "anchor_right_bottom";
            // 
            // pdf2word_clear
            // 
            this.pdf2word_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.pdf2word_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdf2word_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2word_clear.Location = new System.Drawing.Point(847, 225);
            this.pdf2word_clear.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2word_clear.Name = "pdf2word_clear";
            this.pdf2word_clear.Size = new System.Drawing.Size(23, 57);
            this.pdf2word_clear.TabIndex = 12;
            this.pdf2word_clear.Tag = "anchor_right";
            this.pdf2word_clear.UseVisualStyleBackColor = true;
            // 
            // pdf2word_remove
            // 
            this.pdf2word_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.pdf2word_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdf2word_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2word_remove.Location = new System.Drawing.Point(847, 145);
            this.pdf2word_remove.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2word_remove.Name = "pdf2word_remove";
            this.pdf2word_remove.Size = new System.Drawing.Size(23, 57);
            this.pdf2word_remove.TabIndex = 12;
            this.pdf2word_remove.Tag = "anchor_right";
            this.pdf2word_remove.UseVisualStyleBackColor = true;
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label55.Location = new System.Drawing.Point(321, 435);
            this.label55.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(53, 12);
            this.label55.TabIndex = 10;
            this.label55.Tag = "anchor_right_bottom";
            this.label55.Text = "输出模式";
            // 
            // PDF2WORD_UseSUbOcr
            // 
            this.PDF2WORD_UseSUbOcr.AutoSize = true;
            this.PDF2WORD_UseSUbOcr.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PDF2WORD_UseSUbOcr.Location = new System.Drawing.Point(104, 433);
            this.PDF2WORD_UseSUbOcr.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2WORD_UseSUbOcr.Name = "PDF2WORD_UseSUbOcr";
            this.PDF2WORD_UseSUbOcr.Size = new System.Drawing.Size(114, 16);
            this.PDF2WORD_UseSUbOcr.TabIndex = 9;
            this.PDF2WORD_UseSUbOcr.Tag = "anchor_left_bottom";
            this.PDF2WORD_UseSUbOcr.Text = "开启OCR子层识别";
            this.PDF2WORD_UseSUbOcr.UseVisualStyleBackColor = true;
            // 
            // PDF2WORD_UseBullet
            // 
            this.PDF2WORD_UseBullet.AutoSize = true;
            this.PDF2WORD_UseBullet.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PDF2WORD_UseBullet.Location = new System.Drawing.Point(223, 433);
            this.PDF2WORD_UseBullet.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2WORD_UseBullet.Name = "PDF2WORD_UseBullet";
            this.PDF2WORD_UseBullet.Size = new System.Drawing.Size(96, 16);
            this.PDF2WORD_UseBullet.TabIndex = 8;
            this.PDF2WORD_UseBullet.Tag = "anchor_left_bottom";
            this.PDF2WORD_UseBullet.Text = "开启子弹识别";
            this.PDF2WORD_UseBullet.UseVisualStyleBackColor = true;
            // 
            // PdfFileListView_MoveDown
            // 
            this.PdfFileListView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PdfFileListView_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.PdfFileListView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.PdfFileListView_MoveDown.Name = "PdfFileListView_MoveDown";
            this.PdfFileListView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.PdfFileListView_MoveDown.TabIndex = 7;
            this.PdfFileListView_MoveDown.Tag = "anchor_right";
            this.PdfFileListView_MoveDown.Text = "▽";
            this.PdfFileListView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // PdfFileListView_MoveUp
            // 
            this.PdfFileListView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PdfFileListView_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.PdfFileListView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.PdfFileListView_MoveUp.Name = "PdfFileListView_MoveUp";
            this.PdfFileListView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.PdfFileListView_MoveUp.TabIndex = 6;
            this.PdfFileListView_MoveUp.Tag = "anchor_right";
            this.PdfFileListView_MoveUp.Text = "△";
            this.PdfFileListView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // ProcessPage_P2W
            // 
            this.ProcessPage_P2W.AutoSize = true;
            this.ProcessPage_P2W.ForeColor = System.Drawing.Color.ForestGreen;
            this.ProcessPage_P2W.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ProcessPage_P2W.Location = new System.Drawing.Point(377, 575);
            this.ProcessPage_P2W.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ProcessPage_P2W.Name = "ProcessPage_P2W";
            this.ProcessPage_P2W.Size = new System.Drawing.Size(59, 12);
            this.ProcessPage_P2W.TabIndex = 5;
            this.ProcessPage_P2W.Tag = "anchor_left_bottom";
            this.ProcessPage_P2W.Text = "已保存X页";
            // 
            // PdfStatusText
            // 
            this.PdfStatusText.AutoSize = true;
            this.PdfStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.PdfStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PdfStatusText.Location = new System.Drawing.Point(156, 575);
            this.PdfStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.PdfStatusText.Name = "PdfStatusText";
            this.PdfStatusText.Size = new System.Drawing.Size(95, 12);
            this.PdfStatusText.TabIndex = 4;
            this.PdfStatusText.Tag = "anchor_left_bottom";
            this.PdfStatusText.Text = "正在处理X个文件";
            // 
            // Convert2DocButton
            // 
            this.Convert2DocButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Convert2DocButton.Location = new System.Drawing.Point(697, 561);
            this.Convert2DocButton.Margin = new System.Windows.Forms.Padding(1);
            this.Convert2DocButton.Name = "Convert2DocButton";
            this.Convert2DocButton.Size = new System.Drawing.Size(147, 37);
            this.Convert2DocButton.TabIndex = 2;
            this.Convert2DocButton.Tag = "anchor_right_bottom";
            this.Convert2DocButton.Text = "开始处理";
            this.Convert2DocButton.UseVisualStyleBackColor = true;
            this.Convert2DocButton.Click += new System.EventHandler(this.Convert2DocButton_Click);
            // 
            // SelectPdfButton
            // 
            this.SelectPdfButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectPdfButton.Location = new System.Drawing.Point(5, 561);
            this.SelectPdfButton.Margin = new System.Windows.Forms.Padding(1);
            this.SelectPdfButton.Name = "SelectPdfButton";
            this.SelectPdfButton.Size = new System.Drawing.Size(147, 37);
            this.SelectPdfButton.TabIndex = 1;
            this.SelectPdfButton.Tag = "anchor_left_bottom";
            this.SelectPdfButton.Text = "选择文件";
            this.SelectPdfButton.UseVisualStyleBackColor = true;
            this.SelectPdfButton.Click += new System.EventHandler(this.SelectPdfButton_Click);
            // 
            // PdfFileListView
            // 
            this.PdfFileListView.HideSelection = false;
            this.PdfFileListView.Location = new System.Drawing.Point(4, 4);
            this.PdfFileListView.Margin = new System.Windows.Forms.Padding(1);
            this.PdfFileListView.Name = "PdfFileListView";
            this.PdfFileListView.Size = new System.Drawing.Size(841, 420);
            this.PdfFileListView.TabIndex = 0;
            this.PdfFileListView.Tag = "anchor_full_fill";
            this.PdfFileListView.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_03
            // 
            this.TabPage_03.Controls.Add(this.ExcelFileListView_MoveDown);
            this.TabPage_03.Controls.Add(this.ExcelFileListView_MoveUp);
            this.TabPage_03.Controls.Add(this.excel2pdf_clear);
            this.TabPage_03.Controls.Add(this.excel2pdf_remove);
            this.TabPage_03.Controls.Add(this.ExportEachSheet);
            this.TabPage_03.Controls.Add(this.Excel2PdfStatusText);
            this.TabPage_03.Controls.Add(this.ConvertExcel2PdfButton);
            this.TabPage_03.Controls.Add(this.SelectExcelFileButton);
            this.TabPage_03.Controls.Add(this.ExcelFileListView);
            this.TabPage_03.Location = new System.Drawing.Point(4, 76);
            this.TabPage_03.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_03.Name = "TabPage_03";
            this.TabPage_03.Size = new System.Drawing.Size(882, 659);
            this.TabPage_03.TabIndex = 2;
            this.TabPage_03.Text = "EXCEL转PDF";
            this.TabPage_03.UseVisualStyleBackColor = true;
            // 
            // ExcelFileListView_MoveDown
            // 
            this.ExcelFileListView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExcelFileListView_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.ExcelFileListView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelFileListView_MoveDown.Name = "ExcelFileListView_MoveDown";
            this.ExcelFileListView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.ExcelFileListView_MoveDown.TabIndex = 7;
            this.ExcelFileListView_MoveDown.Tag = "anchor_right";
            this.ExcelFileListView_MoveDown.Text = "▽";
            this.ExcelFileListView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // ExcelFileListView_MoveUp
            // 
            this.ExcelFileListView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExcelFileListView_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.ExcelFileListView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelFileListView_MoveUp.Name = "ExcelFileListView_MoveUp";
            this.ExcelFileListView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.ExcelFileListView_MoveUp.TabIndex = 6;
            this.ExcelFileListView_MoveUp.Tag = "anchor_right";
            this.ExcelFileListView_MoveUp.Text = "△";
            this.ExcelFileListView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // excel2pdf_clear
            // 
            this.excel2pdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.excel2pdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.excel2pdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.excel2pdf_clear.Location = new System.Drawing.Point(847, 225);
            this.excel2pdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.excel2pdf_clear.Name = "excel2pdf_clear";
            this.excel2pdf_clear.Size = new System.Drawing.Size(23, 57);
            this.excel2pdf_clear.TabIndex = 12;
            this.excel2pdf_clear.Tag = "anchor_right";
            this.excel2pdf_clear.UseVisualStyleBackColor = true;
            // 
            // excel2pdf_remove
            // 
            this.excel2pdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.excel2pdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.excel2pdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.excel2pdf_remove.Location = new System.Drawing.Point(847, 145);
            this.excel2pdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.excel2pdf_remove.Name = "excel2pdf_remove";
            this.excel2pdf_remove.Size = new System.Drawing.Size(23, 57);
            this.excel2pdf_remove.TabIndex = 11;
            this.excel2pdf_remove.Tag = "anchor_right";
            this.excel2pdf_remove.UseVisualStyleBackColor = true;
            // 
            // ExportEachSheet
            // 
            this.ExportEachSheet.AutoSize = true;
            this.ExportEachSheet.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExportEachSheet.Location = new System.Drawing.Point(4, 431);
            this.ExportEachSheet.Margin = new System.Windows.Forms.Padding(1);
            this.ExportEachSheet.Name = "ExportEachSheet";
            this.ExportEachSheet.Size = new System.Drawing.Size(192, 16);
            this.ExportEachSheet.TabIndex = 5;
            this.ExportEachSheet.Tag = "anchor_left_bottom";
            this.ExportEachSheet.Text = "导出每个Sheet到单独的PDF文件";
            this.ExportEachSheet.UseVisualStyleBackColor = true;
            // 
            // Excel2PdfStatusText
            // 
            this.Excel2PdfStatusText.AutoSize = true;
            this.Excel2PdfStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.Excel2PdfStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Excel2PdfStatusText.Location = new System.Drawing.Point(156, 575);
            this.Excel2PdfStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Excel2PdfStatusText.Name = "Excel2PdfStatusText";
            this.Excel2PdfStatusText.Size = new System.Drawing.Size(95, 12);
            this.Excel2PdfStatusText.TabIndex = 4;
            this.Excel2PdfStatusText.Tag = "anchor_left_bottom";
            this.Excel2PdfStatusText.Text = "正在处理X个文件";
            // 
            // ConvertExcel2PdfButton
            // 
            this.ConvertExcel2PdfButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertExcel2PdfButton.Location = new System.Drawing.Point(697, 561);
            this.ConvertExcel2PdfButton.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertExcel2PdfButton.Name = "ConvertExcel2PdfButton";
            this.ConvertExcel2PdfButton.Size = new System.Drawing.Size(147, 37);
            this.ConvertExcel2PdfButton.TabIndex = 3;
            this.ConvertExcel2PdfButton.Tag = "anchor_right_bottom";
            this.ConvertExcel2PdfButton.Text = "开始处理";
            this.ConvertExcel2PdfButton.UseVisualStyleBackColor = true;
            this.ConvertExcel2PdfButton.Click += new System.EventHandler(this.ConvertExcel2PdfButton_Click);
            // 
            // SelectExcelFileButton
            // 
            this.SelectExcelFileButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectExcelFileButton.Location = new System.Drawing.Point(5, 561);
            this.SelectExcelFileButton.Margin = new System.Windows.Forms.Padding(1);
            this.SelectExcelFileButton.Name = "SelectExcelFileButton";
            this.SelectExcelFileButton.Size = new System.Drawing.Size(147, 37);
            this.SelectExcelFileButton.TabIndex = 2;
            this.SelectExcelFileButton.Tag = "anchor_left_bottom";
            this.SelectExcelFileButton.Text = "选择文件";
            this.SelectExcelFileButton.UseVisualStyleBackColor = true;
            this.SelectExcelFileButton.Click += new System.EventHandler(this.SelectExcelFileButton_Click);
            // 
            // ExcelFileListView
            // 
            this.ExcelFileListView.HideSelection = false;
            this.ExcelFileListView.Location = new System.Drawing.Point(4, 4);
            this.ExcelFileListView.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelFileListView.Name = "ExcelFileListView";
            this.ExcelFileListView.Size = new System.Drawing.Size(841, 420);
            this.ExcelFileListView.TabIndex = 1;
            this.ExcelFileListView.Tag = "anchor_full_fill";
            this.ExcelFileListView.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_07
            // 
            this.TabPage_07.Controls.Add(this.PDF2ExcelFileView_MoveDown);
            this.TabPage_07.Controls.Add(this.PDF2ExcelFileView_MoveUp);
            this.TabPage_07.Controls.Add(this.pdf2excel_clear);
            this.TabPage_07.Controls.Add(this.pdf2excel_remove);
            this.TabPage_07.Controls.Add(this.ExcelSheelMode);
            this.TabPage_07.Controls.Add(this.ConvertPDF2ExcelStatusText);
            this.TabPage_07.Controls.Add(this.ConvertPDF2Excel);
            this.TabPage_07.Controls.Add(this.SelectPDF2ExcelButton);
            this.TabPage_07.Controls.Add(this.PDF2ExcelFileView);
            this.TabPage_07.Location = new System.Drawing.Point(4, 76);
            this.TabPage_07.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_07.Name = "TabPage_07";
            this.TabPage_07.Size = new System.Drawing.Size(882, 659);
            this.TabPage_07.TabIndex = 6;
            this.TabPage_07.Text = "PDF转EXCEL";
            this.TabPage_07.UseVisualStyleBackColor = true;
            // 
            // PDF2ExcelFileView_MoveDown
            // 
            this.PDF2ExcelFileView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PDF2ExcelFileView_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.PDF2ExcelFileView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2ExcelFileView_MoveDown.Name = "PDF2ExcelFileView_MoveDown";
            this.PDF2ExcelFileView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.PDF2ExcelFileView_MoveDown.TabIndex = 6;
            this.PDF2ExcelFileView_MoveDown.Tag = "anchor_right";
            this.PDF2ExcelFileView_MoveDown.Text = "▽";
            this.PDF2ExcelFileView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // PDF2ExcelFileView_MoveUp
            // 
            this.PDF2ExcelFileView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PDF2ExcelFileView_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.PDF2ExcelFileView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2ExcelFileView_MoveUp.Name = "PDF2ExcelFileView_MoveUp";
            this.PDF2ExcelFileView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.PDF2ExcelFileView_MoveUp.TabIndex = 5;
            this.PDF2ExcelFileView_MoveUp.Tag = "anchor_right";
            this.PDF2ExcelFileView_MoveUp.Text = "△";
            this.PDF2ExcelFileView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // pdf2excel_clear
            // 
            this.pdf2excel_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.pdf2excel_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdf2excel_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2excel_clear.Location = new System.Drawing.Point(847, 225);
            this.pdf2excel_clear.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2excel_clear.Name = "pdf2excel_clear";
            this.pdf2excel_clear.Size = new System.Drawing.Size(23, 57);
            this.pdf2excel_clear.TabIndex = 12;
            this.pdf2excel_clear.Tag = "anchor_right";
            this.pdf2excel_clear.UseVisualStyleBackColor = true;
            // 
            // pdf2excel_remove
            // 
            this.pdf2excel_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.pdf2excel_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdf2excel_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2excel_remove.Location = new System.Drawing.Point(847, 145);
            this.pdf2excel_remove.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2excel_remove.Name = "pdf2excel_remove";
            this.pdf2excel_remove.Size = new System.Drawing.Size(23, 57);
            this.pdf2excel_remove.TabIndex = 11;
            this.pdf2excel_remove.Tag = "anchor_right";
            this.pdf2excel_remove.UseVisualStyleBackColor = true;
            // 
            // ExcelSheelMode
            // 
            this.ExcelSheelMode.AutoSize = true;
            this.ExcelSheelMode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExcelSheelMode.Location = new System.Drawing.Point(4, 431);
            this.ExcelSheelMode.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelSheelMode.Name = "ExcelSheelMode";
            this.ExcelSheelMode.Size = new System.Drawing.Size(174, 16);
            this.ExcelSheelMode.TabIndex = 4;
            this.ExcelSheelMode.Tag = "anchor_left_bottom";
            this.ExcelSheelMode.Text = "所有页面合并在一个Sheet上";
            this.ExcelSheelMode.UseVisualStyleBackColor = true;
            // 
            // ConvertPDF2ExcelStatusText
            // 
            this.ConvertPDF2ExcelStatusText.AutoSize = true;
            this.ConvertPDF2ExcelStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertPDF2ExcelStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPDF2ExcelStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertPDF2ExcelStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertPDF2ExcelStatusText.Name = "ConvertPDF2ExcelStatusText";
            this.ConvertPDF2ExcelStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertPDF2ExcelStatusText.TabIndex = 3;
            this.ConvertPDF2ExcelStatusText.Tag = "anchor_left_bottom";
            this.ConvertPDF2ExcelStatusText.Text = "正在处理X个文件";
            // 
            // ConvertPDF2Excel
            // 
            this.ConvertPDF2Excel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPDF2Excel.Location = new System.Drawing.Point(697, 561);
            this.ConvertPDF2Excel.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPDF2Excel.Name = "ConvertPDF2Excel";
            this.ConvertPDF2Excel.Size = new System.Drawing.Size(147, 37);
            this.ConvertPDF2Excel.TabIndex = 2;
            this.ConvertPDF2Excel.Tag = "anchor_right_bottom";
            this.ConvertPDF2Excel.Text = "开始处理";
            this.ConvertPDF2Excel.UseVisualStyleBackColor = true;
            this.ConvertPDF2Excel.Click += new System.EventHandler(this.ConvertPDF2Excel_Click);
            // 
            // SelectPDF2ExcelButton
            // 
            this.SelectPDF2ExcelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectPDF2ExcelButton.Location = new System.Drawing.Point(5, 561);
            this.SelectPDF2ExcelButton.Margin = new System.Windows.Forms.Padding(1);
            this.SelectPDF2ExcelButton.Name = "SelectPDF2ExcelButton";
            this.SelectPDF2ExcelButton.Size = new System.Drawing.Size(147, 37);
            this.SelectPDF2ExcelButton.TabIndex = 1;
            this.SelectPDF2ExcelButton.Tag = "anchor_left_bottom";
            this.SelectPDF2ExcelButton.Text = "选择文件";
            this.SelectPDF2ExcelButton.UseVisualStyleBackColor = true;
            this.SelectPDF2ExcelButton.Click += new System.EventHandler(this.SelectPDF2ExcelButton_Click);
            // 
            // PDF2ExcelFileView
            // 
            this.PDF2ExcelFileView.HideSelection = false;
            this.PDF2ExcelFileView.Location = new System.Drawing.Point(4, 4);
            this.PDF2ExcelFileView.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2ExcelFileView.Name = "PDF2ExcelFileView";
            this.PDF2ExcelFileView.Size = new System.Drawing.Size(841, 420);
            this.PDF2ExcelFileView.TabIndex = 0;
            this.PDF2ExcelFileView.Tag = "anchor_full_fill";
            this.PDF2ExcelFileView.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_20
            // 
            this.TabPage_20.Controls.Add(this.ppt2pdf_save_source);
            this.TabPage_20.Controls.Add(this.PPT2PdfFileList_MoveDown);
            this.TabPage_20.Controls.Add(this.PPT2PdfFileList_MoveUp);
            this.TabPage_20.Controls.Add(this.ppt2pdf_clear);
            this.TabPage_20.Controls.Add(this.ppt2pdf_remove);
            this.TabPage_20.Controls.Add(this.label36);
            this.TabPage_20.Controls.Add(this.ConvertPPT2PDFPassword);
            this.TabPage_20.Controls.Add(this.label35);
            this.TabPage_20.Controls.Add(this.ExportNotePageCombox);
            this.TabPage_20.Controls.Add(this.DrawBlackLine);
            this.TabPage_20.Controls.Add(this.ExportHiddenPage);
            this.TabPage_20.Controls.Add(this.ConvertPPT2PDFStatusText);
            this.TabPage_20.Controls.Add(this.ConvertPPT2PDF);
            this.TabPage_20.Controls.Add(this.SelectPPT2PdfFile);
            this.TabPage_20.Controls.Add(this.PPT2PdfFileList);
            this.TabPage_20.Location = new System.Drawing.Point(4, 76);
            this.TabPage_20.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_20.Name = "TabPage_20";
            this.TabPage_20.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_20.Size = new System.Drawing.Size(882, 659);
            this.TabPage_20.TabIndex = 19;
            this.TabPage_20.Tag = "anchor_left_bottom";
            this.TabPage_20.Text = "PPT转PDF";
            this.TabPage_20.UseVisualStyleBackColor = true;
            // 
            // ppt2pdf_save_source
            // 
            this.ppt2pdf_save_source.AutoSize = true;
            this.ppt2pdf_save_source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ppt2pdf_save_source.Location = new System.Drawing.Point(4, 457);
            this.ppt2pdf_save_source.Margin = new System.Windows.Forms.Padding(1);
            this.ppt2pdf_save_source.Name = "ppt2pdf_save_source";
            this.ppt2pdf_save_source.Size = new System.Drawing.Size(96, 16);
            this.ppt2pdf_save_source.TabIndex = 17;
            this.ppt2pdf_save_source.Tag = "anchor_left_bottom";
            this.ppt2pdf_save_source.Text = "保存到源目录";
            this.ppt2pdf_save_source.UseVisualStyleBackColor = true;
            // 
            // PPT2PdfFileList_MoveDown
            // 
            this.PPT2PdfFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PPT2PdfFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.PPT2PdfFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.PPT2PdfFileList_MoveDown.Name = "PPT2PdfFileList_MoveDown";
            this.PPT2PdfFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.PPT2PdfFileList_MoveDown.TabIndex = 12;
            this.PPT2PdfFileList_MoveDown.Tag = "anchor_right";
            this.PPT2PdfFileList_MoveDown.Text = "▽";
            this.PPT2PdfFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // PPT2PdfFileList_MoveUp
            // 
            this.PPT2PdfFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PPT2PdfFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.PPT2PdfFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.PPT2PdfFileList_MoveUp.Name = "PPT2PdfFileList_MoveUp";
            this.PPT2PdfFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.PPT2PdfFileList_MoveUp.TabIndex = 11;
            this.PPT2PdfFileList_MoveUp.Tag = "anchor_right";
            this.PPT2PdfFileList_MoveUp.Text = "△";
            this.PPT2PdfFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // ppt2pdf_clear
            // 
            this.ppt2pdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.ppt2pdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ppt2pdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ppt2pdf_clear.Location = new System.Drawing.Point(847, 225);
            this.ppt2pdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.ppt2pdf_clear.Name = "ppt2pdf_clear";
            this.ppt2pdf_clear.Size = new System.Drawing.Size(23, 57);
            this.ppt2pdf_clear.TabIndex = 12;
            this.ppt2pdf_clear.Tag = "anchor_right";
            this.ppt2pdf_clear.UseVisualStyleBackColor = true;
            // 
            // ppt2pdf_remove
            // 
            this.ppt2pdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.ppt2pdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ppt2pdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ppt2pdf_remove.Location = new System.Drawing.Point(847, 145);
            this.ppt2pdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.ppt2pdf_remove.Name = "ppt2pdf_remove";
            this.ppt2pdf_remove.Size = new System.Drawing.Size(23, 57);
            this.ppt2pdf_remove.TabIndex = 11;
            this.ppt2pdf_remove.Tag = "anchor_right";
            this.ppt2pdf_remove.UseVisualStyleBackColor = true;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label36.Location = new System.Drawing.Point(371, 431);
            this.label36.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(53, 12);
            this.label36.TabIndex = 9;
            this.label36.Tag = "anchor_left_bottom";
            this.label36.Text = "设置密码";
            // 
            // ConvertPPT2PDFPassword
            // 
            this.ConvertPPT2PDFPassword.Location = new System.Drawing.Point(428, 427);
            this.ConvertPPT2PDFPassword.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPPT2PDFPassword.Name = "ConvertPPT2PDFPassword";
            this.ConvertPPT2PDFPassword.Size = new System.Drawing.Size(99, 21);
            this.ConvertPPT2PDFPassword.TabIndex = 8;
            this.ConvertPPT2PDFPassword.Tag = "anchor_left_bottom";
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label35.Location = new System.Drawing.Point(191, 431);
            this.label35.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(89, 12);
            this.label35.TabIndex = 7;
            this.label35.Tag = "anchor_left_bottom";
            this.label35.Text = "备注页导出方式";
            // 
            // ExportNotePageCombox
            // 
            this.ExportNotePageCombox.FormattingEnabled = true;
            this.ExportNotePageCombox.Location = new System.Drawing.Point(284, 427);
            this.ExportNotePageCombox.Margin = new System.Windows.Forms.Padding(1);
            this.ExportNotePageCombox.Name = "ExportNotePageCombox";
            this.ExportNotePageCombox.Size = new System.Drawing.Size(83, 20);
            this.ExportNotePageCombox.TabIndex = 6;
            this.ExportNotePageCombox.Tag = "anchor_left_bottom";
            // 
            // DrawBlackLine
            // 
            this.DrawBlackLine.AutoSize = true;
            this.DrawBlackLine.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DrawBlackLine.Location = new System.Drawing.Point(116, 431);
            this.DrawBlackLine.Margin = new System.Windows.Forms.Padding(1);
            this.DrawBlackLine.Name = "DrawBlackLine";
            this.DrawBlackLine.Size = new System.Drawing.Size(72, 16);
            this.DrawBlackLine.TabIndex = 5;
            this.DrawBlackLine.Tag = "anchor_left_bottom";
            this.DrawBlackLine.Text = "绘制黑框";
            this.DrawBlackLine.UseVisualStyleBackColor = true;
            // 
            // ExportHiddenPage
            // 
            this.ExportHiddenPage.AutoSize = true;
            this.ExportHiddenPage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExportHiddenPage.Location = new System.Drawing.Point(4, 431);
            this.ExportHiddenPage.Margin = new System.Windows.Forms.Padding(1);
            this.ExportHiddenPage.Name = "ExportHiddenPage";
            this.ExportHiddenPage.Size = new System.Drawing.Size(108, 16);
            this.ExportHiddenPage.TabIndex = 4;
            this.ExportHiddenPage.Tag = "anchor_left_bottom";
            this.ExportHiddenPage.Text = "隐藏页面也导出";
            this.ExportHiddenPage.UseVisualStyleBackColor = true;
            // 
            // ConvertPPT2PDFStatusText
            // 
            this.ConvertPPT2PDFStatusText.AutoSize = true;
            this.ConvertPPT2PDFStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertPPT2PDFStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPPT2PDFStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertPPT2PDFStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertPPT2PDFStatusText.Name = "ConvertPPT2PDFStatusText";
            this.ConvertPPT2PDFStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertPPT2PDFStatusText.TabIndex = 3;
            this.ConvertPPT2PDFStatusText.Tag = "anchor_left_bottom";
            this.ConvertPPT2PDFStatusText.Text = "正在处理X个文件";
            // 
            // ConvertPPT2PDF
            // 
            this.ConvertPPT2PDF.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPPT2PDF.Location = new System.Drawing.Point(697, 561);
            this.ConvertPPT2PDF.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPPT2PDF.Name = "ConvertPPT2PDF";
            this.ConvertPPT2PDF.Size = new System.Drawing.Size(147, 37);
            this.ConvertPPT2PDF.TabIndex = 2;
            this.ConvertPPT2PDF.Tag = "anchor_right_bottom";
            this.ConvertPPT2PDF.Text = "开始处理";
            this.ConvertPPT2PDF.UseVisualStyleBackColor = true;
            this.ConvertPPT2PDF.Click += new System.EventHandler(this.ConvertPPT2PDF_Click);
            // 
            // SelectPPT2PdfFile
            // 
            this.SelectPPT2PdfFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectPPT2PdfFile.Location = new System.Drawing.Point(5, 561);
            this.SelectPPT2PdfFile.Margin = new System.Windows.Forms.Padding(1);
            this.SelectPPT2PdfFile.Name = "SelectPPT2PdfFile";
            this.SelectPPT2PdfFile.Size = new System.Drawing.Size(147, 37);
            this.SelectPPT2PdfFile.TabIndex = 1;
            this.SelectPPT2PdfFile.Tag = "anchor_left_bottom";
            this.SelectPPT2PdfFile.Text = "选择文件";
            this.SelectPPT2PdfFile.UseVisualStyleBackColor = true;
            this.SelectPPT2PdfFile.Click += new System.EventHandler(this.SelectPPT2PdfFile_Click);
            // 
            // PPT2PdfFileList
            // 
            this.PPT2PdfFileList.HideSelection = false;
            this.PPT2PdfFileList.Location = new System.Drawing.Point(4, 4);
            this.PPT2PdfFileList.Margin = new System.Windows.Forms.Padding(1);
            this.PPT2PdfFileList.Name = "PPT2PdfFileList";
            this.PPT2PdfFileList.Size = new System.Drawing.Size(841, 420);
            this.PPT2PdfFileList.TabIndex = 0;
            this.PPT2PdfFileList.Tag = "anchor_full_fill";
            this.PPT2PdfFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_21
            // 
            this.TabPage_21.Controls.Add(this.pdf2ppt_save_source);
            this.TabPage_21.Controls.Add(this.PDF2PPTFileList_MoveDown);
            this.TabPage_21.Controls.Add(this.PDF2PPTFileList_MoveUp);
            this.TabPage_21.Controls.Add(this.pdf2ppt_clear);
            this.TabPage_21.Controls.Add(this.pdf2ppt_remove);
            this.TabPage_21.Controls.Add(this.RenderImageBox);
            this.TabPage_21.Controls.Add(this.SplitShapeBox);
            this.TabPage_21.Controls.Add(this.ExtraOcrBox);
            this.TabPage_21.Controls.Add(this.Pdf2PPTStatusText);
            this.TabPage_21.Controls.Add(this.ConvertPDF2PPT);
            this.TabPage_21.Controls.Add(this.SelectConvertPDF2PPT);
            this.TabPage_21.Controls.Add(this.PDF2PPTFileList);
            this.TabPage_21.Location = new System.Drawing.Point(4, 76);
            this.TabPage_21.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_21.Name = "TabPage_21";
            this.TabPage_21.Size = new System.Drawing.Size(882, 659);
            this.TabPage_21.TabIndex = 20;
            this.TabPage_21.Text = "PDF转PPT";
            this.TabPage_21.UseVisualStyleBackColor = true;
            // 
            // pdf2ppt_save_source
            // 
            this.pdf2ppt_save_source.AutoSize = true;
            this.pdf2ppt_save_source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2ppt_save_source.Location = new System.Drawing.Point(4, 457);
            this.pdf2ppt_save_source.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2ppt_save_source.Name = "pdf2ppt_save_source";
            this.pdf2ppt_save_source.Size = new System.Drawing.Size(96, 16);
            this.pdf2ppt_save_source.TabIndex = 17;
            this.pdf2ppt_save_source.Tag = "anchor_left_bottom";
            this.pdf2ppt_save_source.Text = "保存到源目录";
            this.pdf2ppt_save_source.UseVisualStyleBackColor = true;
            // 
            // PDF2PPTFileList_MoveDown
            // 
            this.PDF2PPTFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PDF2PPTFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.PDF2PPTFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2PPTFileList_MoveDown.Name = "PDF2PPTFileList_MoveDown";
            this.PDF2PPTFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.PDF2PPTFileList_MoveDown.TabIndex = 8;
            this.PDF2PPTFileList_MoveDown.Tag = "anchor_right";
            this.PDF2PPTFileList_MoveDown.Text = "▽";
            this.PDF2PPTFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // PDF2PPTFileList_MoveUp
            // 
            this.PDF2PPTFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PDF2PPTFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.PDF2PPTFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2PPTFileList_MoveUp.Name = "PDF2PPTFileList_MoveUp";
            this.PDF2PPTFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.PDF2PPTFileList_MoveUp.TabIndex = 7;
            this.PDF2PPTFileList_MoveUp.Tag = "anchor_right";
            this.PDF2PPTFileList_MoveUp.Text = "△";
            this.PDF2PPTFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // pdf2ppt_clear
            // 
            this.pdf2ppt_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.pdf2ppt_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdf2ppt_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2ppt_clear.Location = new System.Drawing.Point(847, 225);
            this.pdf2ppt_clear.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2ppt_clear.Name = "pdf2ppt_clear";
            this.pdf2ppt_clear.Size = new System.Drawing.Size(23, 57);
            this.pdf2ppt_clear.TabIndex = 12;
            this.pdf2ppt_clear.Tag = "anchor_right";
            this.pdf2ppt_clear.UseVisualStyleBackColor = true;
            // 
            // pdf2ppt_remove
            // 
            this.pdf2ppt_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.pdf2ppt_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdf2ppt_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdf2ppt_remove.Location = new System.Drawing.Point(847, 145);
            this.pdf2ppt_remove.Margin = new System.Windows.Forms.Padding(1);
            this.pdf2ppt_remove.Name = "pdf2ppt_remove";
            this.pdf2ppt_remove.Size = new System.Drawing.Size(23, 57);
            this.pdf2ppt_remove.TabIndex = 11;
            this.pdf2ppt_remove.Tag = "anchor_right";
            this.pdf2ppt_remove.UseVisualStyleBackColor = true;
            // 
            // RenderImageBox
            // 
            this.RenderImageBox.AutoSize = true;
            this.RenderImageBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RenderImageBox.Location = new System.Drawing.Point(305, 431);
            this.RenderImageBox.Margin = new System.Windows.Forms.Padding(1);
            this.RenderImageBox.Name = "RenderImageBox";
            this.RenderImageBox.Size = new System.Drawing.Size(132, 16);
            this.RenderImageBox.TabIndex = 6;
            this.RenderImageBox.Tag = "anchor_left_bottom";
            this.RenderImageBox.Text = "所有页面识别为图片";
            this.RenderImageBox.UseVisualStyleBackColor = true;
            // 
            // SplitShapeBox
            // 
            this.SplitShapeBox.AutoSize = true;
            this.SplitShapeBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitShapeBox.Location = new System.Drawing.Point(193, 431);
            this.SplitShapeBox.Margin = new System.Windows.Forms.Padding(1);
            this.SplitShapeBox.Name = "SplitShapeBox";
            this.SplitShapeBox.Size = new System.Drawing.Size(108, 16);
            this.SplitShapeBox.TabIndex = 5;
            this.SplitShapeBox.Tag = "anchor_left_bottom";
            this.SplitShapeBox.Text = "分离图像与图形";
            this.SplitShapeBox.UseVisualStyleBackColor = true;
            // 
            // ExtraOcrBox
            // 
            this.ExtraOcrBox.AutoSize = true;
            this.ExtraOcrBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExtraOcrBox.Location = new System.Drawing.Point(4, 431);
            this.ExtraOcrBox.Margin = new System.Windows.Forms.Padding(1);
            this.ExtraOcrBox.Name = "ExtraOcrBox";
            this.ExtraOcrBox.Size = new System.Drawing.Size(186, 16);
            this.ExtraOcrBox.TabIndex = 4;
            this.ExtraOcrBox.Tag = "anchor_left_bottom";
            this.ExtraOcrBox.Text = "开启OCR子层识别（非扫描件）";
            this.ExtraOcrBox.UseVisualStyleBackColor = true;
            // 
            // Pdf2PPTStatusText
            // 
            this.Pdf2PPTStatusText.AutoSize = true;
            this.Pdf2PPTStatusText.BackColor = System.Drawing.Color.Transparent;
            this.Pdf2PPTStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.Pdf2PPTStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Pdf2PPTStatusText.Location = new System.Drawing.Point(156, 575);
            this.Pdf2PPTStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Pdf2PPTStatusText.Name = "Pdf2PPTStatusText";
            this.Pdf2PPTStatusText.Size = new System.Drawing.Size(95, 12);
            this.Pdf2PPTStatusText.TabIndex = 3;
            this.Pdf2PPTStatusText.Tag = "anchor_left_bottom";
            this.Pdf2PPTStatusText.Text = "正在处理X个文件";
            // 
            // ConvertPDF2PPT
            // 
            this.ConvertPDF2PPT.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPDF2PPT.Location = new System.Drawing.Point(697, 561);
            this.ConvertPDF2PPT.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPDF2PPT.Name = "ConvertPDF2PPT";
            this.ConvertPDF2PPT.Size = new System.Drawing.Size(147, 37);
            this.ConvertPDF2PPT.TabIndex = 2;
            this.ConvertPDF2PPT.Tag = "anchor_right_bottom";
            this.ConvertPDF2PPT.Text = "开始处理";
            this.ConvertPDF2PPT.UseVisualStyleBackColor = true;
            this.ConvertPDF2PPT.Click += new System.EventHandler(this.ConvertPDF2PPT_Click);
            // 
            // SelectConvertPDF2PPT
            // 
            this.SelectConvertPDF2PPT.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectConvertPDF2PPT.Location = new System.Drawing.Point(5, 561);
            this.SelectConvertPDF2PPT.Margin = new System.Windows.Forms.Padding(1);
            this.SelectConvertPDF2PPT.Name = "SelectConvertPDF2PPT";
            this.SelectConvertPDF2PPT.Size = new System.Drawing.Size(147, 37);
            this.SelectConvertPDF2PPT.TabIndex = 1;
            this.SelectConvertPDF2PPT.Tag = "anchor_left_bottom";
            this.SelectConvertPDF2PPT.Text = "选择文件";
            this.SelectConvertPDF2PPT.UseVisualStyleBackColor = true;
            this.SelectConvertPDF2PPT.Click += new System.EventHandler(this.SelectConvertPDF2PPT_Click);
            // 
            // PDF2PPTFileList
            // 
            this.PDF2PPTFileList.HideSelection = false;
            this.PDF2PPTFileList.Location = new System.Drawing.Point(4, 4);
            this.PDF2PPTFileList.Margin = new System.Windows.Forms.Padding(1);
            this.PDF2PPTFileList.Name = "PDF2PPTFileList";
            this.PDF2PPTFileList.Size = new System.Drawing.Size(841, 420);
            this.PDF2PPTFileList.TabIndex = 0;
            this.PDF2PPTFileList.Tag = "anchor_full_fill";
            this.PDF2PPTFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_14
            // 
            this.TabPage_14.Controls.Add(this.MergeWordFileList_MoveDown);
            this.TabPage_14.Controls.Add(this.MergeWordFileList_MoveUp);
            this.TabPage_14.Controls.Add(this.meregeword_clear);
            this.TabPage_14.Controls.Add(this.meregeword_remove);
            this.TabPage_14.Controls.Add(this.MeregeWordStatusText);
            this.TabPage_14.Controls.Add(this.label23);
            this.TabPage_14.Controls.Add(this.MergeWordFile);
            this.TabPage_14.Controls.Add(this.SelectMergeWordFiles);
            this.TabPage_14.Controls.Add(this.MergeWordFileList);
            this.TabPage_14.Location = new System.Drawing.Point(4, 76);
            this.TabPage_14.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_14.Name = "TabPage_14";
            this.TabPage_14.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_14.Size = new System.Drawing.Size(882, 659);
            this.TabPage_14.TabIndex = 13;
            this.TabPage_14.Text = "合并WORD";
            this.TabPage_14.UseVisualStyleBackColor = true;
            // 
            // MergeWordFileList_MoveDown
            // 
            this.MergeWordFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MergeWordFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.MergeWordFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.MergeWordFileList_MoveDown.Name = "MergeWordFileList_MoveDown";
            this.MergeWordFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.MergeWordFileList_MoveDown.TabIndex = 6;
            this.MergeWordFileList_MoveDown.Tag = "anchor_right";
            this.MergeWordFileList_MoveDown.Text = "▽";
            this.MergeWordFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // MergeWordFileList_MoveUp
            // 
            this.MergeWordFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MergeWordFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.MergeWordFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.MergeWordFileList_MoveUp.Name = "MergeWordFileList_MoveUp";
            this.MergeWordFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.MergeWordFileList_MoveUp.TabIndex = 5;
            this.MergeWordFileList_MoveUp.Tag = "anchor_right";
            this.MergeWordFileList_MoveUp.Text = "△";
            this.MergeWordFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // meregeword_clear
            // 
            this.meregeword_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.meregeword_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.meregeword_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.meregeword_clear.Location = new System.Drawing.Point(847, 225);
            this.meregeword_clear.Margin = new System.Windows.Forms.Padding(1);
            this.meregeword_clear.Name = "meregeword_clear";
            this.meregeword_clear.Size = new System.Drawing.Size(23, 57);
            this.meregeword_clear.TabIndex = 12;
            this.meregeword_clear.Tag = "anchor_right";
            this.meregeword_clear.UseVisualStyleBackColor = true;
            // 
            // meregeword_remove
            // 
            this.meregeword_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.meregeword_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.meregeword_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.meregeword_remove.Location = new System.Drawing.Point(847, 145);
            this.meregeword_remove.Margin = new System.Windows.Forms.Padding(1);
            this.meregeword_remove.Name = "meregeword_remove";
            this.meregeword_remove.Size = new System.Drawing.Size(23, 57);
            this.meregeword_remove.TabIndex = 11;
            this.meregeword_remove.Tag = "anchor_right";
            this.meregeword_remove.UseVisualStyleBackColor = true;
            // 
            // MeregeWordStatusText
            // 
            this.MeregeWordStatusText.AutoSize = true;
            this.MeregeWordStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.MeregeWordStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MeregeWordStatusText.Location = new System.Drawing.Point(156, 575);
            this.MeregeWordStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.MeregeWordStatusText.Name = "MeregeWordStatusText";
            this.MeregeWordStatusText.Size = new System.Drawing.Size(95, 12);
            this.MeregeWordStatusText.TabIndex = 4;
            this.MeregeWordStatusText.Tag = "anchor_left_bottom";
            this.MeregeWordStatusText.Text = "正在处理X个文件";
            // 
            // label23
            // 
            this.label23.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label23.AutoSize = true;
            this.label23.ForeColor = System.Drawing.Color.Tomato;
            this.label23.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label23.Location = new System.Drawing.Point(4, 812);
            this.label23.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(437, 12);
            this.label23.TabIndex = 3;
            this.label23.Text = "提示：合并WORD不能保证格式完全一致，合并完毕后请自行检查文档是否需要调整";
            // 
            // MergeWordFile
            // 
            this.MergeWordFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MergeWordFile.Location = new System.Drawing.Point(697, 561);
            this.MergeWordFile.Margin = new System.Windows.Forms.Padding(1);
            this.MergeWordFile.Name = "MergeWordFile";
            this.MergeWordFile.Size = new System.Drawing.Size(147, 37);
            this.MergeWordFile.TabIndex = 2;
            this.MergeWordFile.Tag = "anchor_right_bottom";
            this.MergeWordFile.Text = "开始处理";
            this.MergeWordFile.UseVisualStyleBackColor = true;
            this.MergeWordFile.Click += new System.EventHandler(this.MergeWordFile_Click);
            // 
            // SelectMergeWordFiles
            // 
            this.SelectMergeWordFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectMergeWordFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectMergeWordFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectMergeWordFiles.Name = "SelectMergeWordFiles";
            this.SelectMergeWordFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectMergeWordFiles.TabIndex = 1;
            this.SelectMergeWordFiles.Tag = "anchor_left_bottom";
            this.SelectMergeWordFiles.Text = "选择文件";
            this.SelectMergeWordFiles.UseVisualStyleBackColor = true;
            this.SelectMergeWordFiles.Click += new System.EventHandler(this.SelectMergeWordFiles_Click);
            // 
            // MergeWordFileList
            // 
            this.MergeWordFileList.HideSelection = false;
            this.MergeWordFileList.Location = new System.Drawing.Point(4, 4);
            this.MergeWordFileList.Margin = new System.Windows.Forms.Padding(1);
            this.MergeWordFileList.Name = "MergeWordFileList";
            this.MergeWordFileList.Size = new System.Drawing.Size(841, 420);
            this.MergeWordFileList.TabIndex = 0;
            this.MergeWordFileList.Tag = "anchor_full_fill";
            this.MergeWordFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_31
            // 
            this.TabPage_31.Controls.Add(this.SplitWordReUpdatePageNumber);
            this.TabPage_31.Controls.Add(this.groupBox48);
            this.TabPage_31.Controls.Add(this.SplitWordFileList_MoveDown);
            this.TabPage_31.Controls.Add(this.SplitWordFileList_MoveUp);
            this.TabPage_31.Controls.Add(this.splitword_clear);
            this.TabPage_31.Controls.Add(this.splitword_remove);
            this.TabPage_31.Controls.Add(this.StartSplitWordStatusText);
            this.TabPage_31.Controls.Add(this.StartSplitWord);
            this.TabPage_31.Controls.Add(this.SelectSplitWordFiles);
            this.TabPage_31.Controls.Add(this.SplitWordFileList);
            this.TabPage_31.Location = new System.Drawing.Point(4, 76);
            this.TabPage_31.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_31.Name = "TabPage_31";
            this.TabPage_31.Size = new System.Drawing.Size(882, 659);
            this.TabPage_31.TabIndex = 30;
            this.TabPage_31.Text = "拆分WORD";
            this.TabPage_31.UseVisualStyleBackColor = true;
            // 
            // SplitWordReUpdatePageNumber
            // 
            this.SplitWordReUpdatePageNumber.AutoSize = true;
            this.SplitWordReUpdatePageNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitWordReUpdatePageNumber.Location = new System.Drawing.Point(4, 508);
            this.SplitWordReUpdatePageNumber.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWordReUpdatePageNumber.Name = "SplitWordReUpdatePageNumber";
            this.SplitWordReUpdatePageNumber.Size = new System.Drawing.Size(120, 16);
            this.SplitWordReUpdatePageNumber.TabIndex = 22;
            this.SplitWordReUpdatePageNumber.Tag = "anchor_left_bottom";
            this.SplitWordReUpdatePageNumber.Text = "重新更新页码信息";
            this.SplitWordReUpdatePageNumber.UseVisualStyleBackColor = true;
            // 
            // groupBox48
            // 
            this.groupBox48.Controls.Add(this.label145);
            this.groupBox48.Controls.Add(this.label146);
            this.groupBox48.Controls.Add(this.SplitWord_PageCount);
            this.groupBox48.Controls.Add(this.SplitWord_Single);
            this.groupBox48.Controls.Add(this.label135);
            this.groupBox48.Controls.Add(this.SplitWord_Range_End);
            this.groupBox48.Controls.Add(this.label136);
            this.groupBox48.Controls.Add(this.SplitWord_Range_Start);
            this.groupBox48.Controls.Add(this.label137);
            this.groupBox48.Controls.Add(this.SplitWord_Range);
            this.groupBox48.Location = new System.Drawing.Point(4, 427);
            this.groupBox48.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox48.Name = "groupBox48";
            this.groupBox48.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox48.Size = new System.Drawing.Size(353, 77);
            this.groupBox48.TabIndex = 21;
            this.groupBox48.TabStop = false;
            this.groupBox48.Tag = "anchor_left_bottom";
            this.groupBox48.Text = "拆分设置";
            // 
            // label145
            // 
            this.label145.AutoSize = true;
            this.label145.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label145.Location = new System.Drawing.Point(231, 48);
            this.label145.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label145.Name = "label145";
            this.label145.Size = new System.Drawing.Size(101, 12);
            this.label145.TabIndex = 12;
            this.label145.Text = "页拆分为一个文件";
            // 
            // label146
            // 
            this.label146.AutoSize = true;
            this.label146.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label146.Location = new System.Drawing.Point(127, 48);
            this.label146.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label146.Name = "label146";
            this.label146.Size = new System.Drawing.Size(17, 12);
            this.label146.TabIndex = 11;
            this.label146.Text = "每";
            // 
            // SplitWord_PageCount
            // 
            this.SplitWord_PageCount.Location = new System.Drawing.Point(147, 45);
            this.SplitWord_PageCount.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWord_PageCount.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.SplitWord_PageCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.SplitWord_PageCount.Name = "SplitWord_PageCount";
            this.SplitWord_PageCount.Size = new System.Drawing.Size(80, 21);
            this.SplitWord_PageCount.TabIndex = 10;
            this.SplitWord_PageCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // SplitWord_Single
            // 
            this.SplitWord_Single.AutoSize = true;
            this.SplitWord_Single.Checked = true;
            this.SplitWord_Single.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitWord_Single.Location = new System.Drawing.Point(4, 48);
            this.SplitWord_Single.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWord_Single.Name = "SplitWord_Single";
            this.SplitWord_Single.Size = new System.Drawing.Size(71, 16);
            this.SplitWord_Single.TabIndex = 6;
            this.SplitWord_Single.TabStop = true;
            this.SplitWord_Single.Text = "逐页拆分";
            this.SplitWord_Single.UseVisualStyleBackColor = true;
            // 
            // label135
            // 
            this.label135.AutoSize = true;
            this.label135.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label135.Location = new System.Drawing.Point(231, 17);
            this.label135.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label135.Name = "label135";
            this.label135.Size = new System.Drawing.Size(17, 12);
            this.label135.TabIndex = 5;
            this.label135.Text = "页";
            // 
            // SplitWord_Range_End
            // 
            this.SplitWord_Range_End.Location = new System.Drawing.Point(200, 15);
            this.SplitWord_Range_End.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWord_Range_End.Name = "SplitWord_Range_End";
            this.SplitWord_Range_End.Size = new System.Drawing.Size(28, 21);
            this.SplitWord_Range_End.TabIndex = 4;
            this.SplitWord_Range_End.Text = "2";
            // 
            // label136
            // 
            this.label136.AutoSize = true;
            this.label136.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label136.Location = new System.Drawing.Point(177, 17);
            this.label136.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label136.Name = "label136";
            this.label136.Size = new System.Drawing.Size(17, 12);
            this.label136.TabIndex = 3;
            this.label136.Text = "到";
            // 
            // SplitWord_Range_Start
            // 
            this.SplitWord_Range_Start.Location = new System.Drawing.Point(147, 15);
            this.SplitWord_Range_Start.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWord_Range_Start.Name = "SplitWord_Range_Start";
            this.SplitWord_Range_Start.Size = new System.Drawing.Size(28, 21);
            this.SplitWord_Range_Start.TabIndex = 2;
            this.SplitWord_Range_Start.Text = "1";
            // 
            // label137
            // 
            this.label137.AutoSize = true;
            this.label137.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label137.Location = new System.Drawing.Point(127, 19);
            this.label137.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label137.Name = "label137";
            this.label137.Size = new System.Drawing.Size(17, 12);
            this.label137.TabIndex = 1;
            this.label137.Text = "从";
            // 
            // SplitWord_Range
            // 
            this.SplitWord_Range.AutoSize = true;
            this.SplitWord_Range.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitWord_Range.Location = new System.Drawing.Point(4, 17);
            this.SplitWord_Range.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWord_Range.Name = "SplitWord_Range";
            this.SplitWord_Range.Size = new System.Drawing.Size(119, 16);
            this.SplitWord_Range.TabIndex = 0;
            this.SplitWord_Range.Text = "按照页面范围拆分";
            this.SplitWord_Range.UseVisualStyleBackColor = true;
            // 
            // SplitWordFileList_MoveDown
            // 
            this.SplitWordFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitWordFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.SplitWordFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWordFileList_MoveDown.Name = "SplitWordFileList_MoveDown";
            this.SplitWordFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.SplitWordFileList_MoveDown.TabIndex = 18;
            this.SplitWordFileList_MoveDown.Tag = "anchor_right";
            this.SplitWordFileList_MoveDown.Text = "▽";
            this.SplitWordFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // SplitWordFileList_MoveUp
            // 
            this.SplitWordFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitWordFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.SplitWordFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWordFileList_MoveUp.Name = "SplitWordFileList_MoveUp";
            this.SplitWordFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.SplitWordFileList_MoveUp.TabIndex = 17;
            this.SplitWordFileList_MoveUp.Tag = "anchor_right";
            this.SplitWordFileList_MoveUp.Text = "△";
            this.SplitWordFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // splitword_clear
            // 
            this.splitword_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.splitword_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.splitword_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.splitword_clear.Location = new System.Drawing.Point(847, 225);
            this.splitword_clear.Margin = new System.Windows.Forms.Padding(1);
            this.splitword_clear.Name = "splitword_clear";
            this.splitword_clear.Size = new System.Drawing.Size(23, 57);
            this.splitword_clear.TabIndex = 20;
            this.splitword_clear.Tag = "anchor_right";
            this.splitword_clear.UseVisualStyleBackColor = true;
            // 
            // splitword_remove
            // 
            this.splitword_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.splitword_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.splitword_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.splitword_remove.Location = new System.Drawing.Point(847, 145);
            this.splitword_remove.Margin = new System.Windows.Forms.Padding(1);
            this.splitword_remove.Name = "splitword_remove";
            this.splitword_remove.Size = new System.Drawing.Size(23, 57);
            this.splitword_remove.TabIndex = 19;
            this.splitword_remove.Tag = "anchor_right";
            this.splitword_remove.UseVisualStyleBackColor = true;
            // 
            // StartSplitWordStatusText
            // 
            this.StartSplitWordStatusText.AutoSize = true;
            this.StartSplitWordStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.StartSplitWordStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StartSplitWordStatusText.Location = new System.Drawing.Point(156, 575);
            this.StartSplitWordStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.StartSplitWordStatusText.Name = "StartSplitWordStatusText";
            this.StartSplitWordStatusText.Size = new System.Drawing.Size(95, 12);
            this.StartSplitWordStatusText.TabIndex = 16;
            this.StartSplitWordStatusText.Tag = "anchor_left_bottom";
            this.StartSplitWordStatusText.Text = "正在处理X个文件";
            // 
            // StartSplitWord
            // 
            this.StartSplitWord.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StartSplitWord.Location = new System.Drawing.Point(697, 561);
            this.StartSplitWord.Margin = new System.Windows.Forms.Padding(1);
            this.StartSplitWord.Name = "StartSplitWord";
            this.StartSplitWord.Size = new System.Drawing.Size(147, 37);
            this.StartSplitWord.TabIndex = 15;
            this.StartSplitWord.Tag = "anchor_right_bottom";
            this.StartSplitWord.Text = "开始处理";
            this.StartSplitWord.UseVisualStyleBackColor = true;
            this.StartSplitWord.Click += new System.EventHandler(this.StartSplitWord_Click);
            // 
            // SelectSplitWordFiles
            // 
            this.SelectSplitWordFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectSplitWordFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectSplitWordFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectSplitWordFiles.Name = "SelectSplitWordFiles";
            this.SelectSplitWordFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectSplitWordFiles.TabIndex = 14;
            this.SelectSplitWordFiles.Tag = "anchor_left_bottom";
            this.SelectSplitWordFiles.Text = "选择文件";
            this.SelectSplitWordFiles.UseVisualStyleBackColor = true;
            this.SelectSplitWordFiles.Click += new System.EventHandler(this.SelectSplitWordFiles_Click);
            // 
            // SplitWordFileList
            // 
            this.SplitWordFileList.HideSelection = false;
            this.SplitWordFileList.Location = new System.Drawing.Point(4, 4);
            this.SplitWordFileList.Margin = new System.Windows.Forms.Padding(1);
            this.SplitWordFileList.Name = "SplitWordFileList";
            this.SplitWordFileList.Size = new System.Drawing.Size(841, 420);
            this.SplitWordFileList.TabIndex = 13;
            this.SplitWordFileList.Tag = "anchor_full_fill";
            this.SplitWordFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_06
            // 
            this.TabPage_06.Controls.Add(this.OpenMeregePdfFile);
            this.TabPage_06.Controls.Add(this.MeregePdfFileStatusText);
            this.TabPage_06.Controls.Add(this.MeregePdfFileButtonConvert);
            this.TabPage_06.Controls.Add(this.MergePdfFileSelectButton);
            this.TabPage_06.Controls.Add(this.MergePdfFileView_MoveDown);
            this.TabPage_06.Controls.Add(this.MergePdfFileView_MoveUp);
            this.TabPage_06.Controls.Add(this.meregepdf_clear);
            this.TabPage_06.Controls.Add(this.meregepdf_remove);
            this.TabPage_06.Controls.Add(this.MergePdfFileView);
            this.TabPage_06.Location = new System.Drawing.Point(4, 76);
            this.TabPage_06.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_06.Name = "TabPage_06";
            this.TabPage_06.Size = new System.Drawing.Size(882, 659);
            this.TabPage_06.TabIndex = 5;
            this.TabPage_06.Text = "合并PDF";
            this.TabPage_06.UseVisualStyleBackColor = true;
            // 
            // OpenMeregePdfFile
            // 
            this.OpenMeregePdfFile.AutoSize = true;
            this.OpenMeregePdfFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.OpenMeregePdfFile.Location = new System.Drawing.Point(4, 431);
            this.OpenMeregePdfFile.Margin = new System.Windows.Forms.Padding(1);
            this.OpenMeregePdfFile.Name = "OpenMeregePdfFile";
            this.OpenMeregePdfFile.Size = new System.Drawing.Size(168, 16);
            this.OpenMeregePdfFile.TabIndex = 4;
            this.OpenMeregePdfFile.Tag = "anchor_left_bottom";
            this.OpenMeregePdfFile.Text = "处理完毕后打开合并的文件";
            this.OpenMeregePdfFile.UseVisualStyleBackColor = true;
            // 
            // MeregePdfFileStatusText
            // 
            this.MeregePdfFileStatusText.AutoSize = true;
            this.MeregePdfFileStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.MeregePdfFileStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MeregePdfFileStatusText.Location = new System.Drawing.Point(156, 575);
            this.MeregePdfFileStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.MeregePdfFileStatusText.Name = "MeregePdfFileStatusText";
            this.MeregePdfFileStatusText.Size = new System.Drawing.Size(95, 12);
            this.MeregePdfFileStatusText.TabIndex = 3;
            this.MeregePdfFileStatusText.Tag = "anchor_left_bottom";
            this.MeregePdfFileStatusText.Text = "正在处理X个文件";
            // 
            // MeregePdfFileButtonConvert
            // 
            this.MeregePdfFileButtonConvert.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MeregePdfFileButtonConvert.Location = new System.Drawing.Point(697, 561);
            this.MeregePdfFileButtonConvert.Margin = new System.Windows.Forms.Padding(1);
            this.MeregePdfFileButtonConvert.Name = "MeregePdfFileButtonConvert";
            this.MeregePdfFileButtonConvert.Size = new System.Drawing.Size(147, 37);
            this.MeregePdfFileButtonConvert.TabIndex = 2;
            this.MeregePdfFileButtonConvert.Tag = "anchor_right_bottom";
            this.MeregePdfFileButtonConvert.Text = "开始处理";
            this.MeregePdfFileButtonConvert.UseVisualStyleBackColor = true;
            this.MeregePdfFileButtonConvert.Click += new System.EventHandler(this.MeregePdfFileButtonConvert_Click);
            // 
            // MergePdfFileSelectButton
            // 
            this.MergePdfFileSelectButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MergePdfFileSelectButton.Location = new System.Drawing.Point(5, 561);
            this.MergePdfFileSelectButton.Margin = new System.Windows.Forms.Padding(1);
            this.MergePdfFileSelectButton.Name = "MergePdfFileSelectButton";
            this.MergePdfFileSelectButton.Size = new System.Drawing.Size(147, 37);
            this.MergePdfFileSelectButton.TabIndex = 1;
            this.MergePdfFileSelectButton.Tag = "anchor_left_bottom";
            this.MergePdfFileSelectButton.Text = "选择文件";
            this.MergePdfFileSelectButton.UseVisualStyleBackColor = true;
            this.MergePdfFileSelectButton.Click += new System.EventHandler(this.MergePdfFileSelectButton_Click);
            // 
            // MergePdfFileView_MoveDown
            // 
            this.MergePdfFileView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MergePdfFileView_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.MergePdfFileView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.MergePdfFileView_MoveDown.Name = "MergePdfFileView_MoveDown";
            this.MergePdfFileView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.MergePdfFileView_MoveDown.TabIndex = 9;
            this.MergePdfFileView_MoveDown.Tag = "anchor_right";
            this.MergePdfFileView_MoveDown.Text = "▽";
            this.MergePdfFileView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // MergePdfFileView_MoveUp
            // 
            this.MergePdfFileView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MergePdfFileView_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.MergePdfFileView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.MergePdfFileView_MoveUp.Name = "MergePdfFileView_MoveUp";
            this.MergePdfFileView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.MergePdfFileView_MoveUp.TabIndex = 8;
            this.MergePdfFileView_MoveUp.Tag = "anchor_right";
            this.MergePdfFileView_MoveUp.Text = "△";
            this.MergePdfFileView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // meregepdf_clear
            // 
            this.meregepdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.meregepdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.meregepdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.meregepdf_clear.Location = new System.Drawing.Point(847, 225);
            this.meregepdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.meregepdf_clear.Name = "meregepdf_clear";
            this.meregepdf_clear.Size = new System.Drawing.Size(23, 57);
            this.meregepdf_clear.TabIndex = 12;
            this.meregepdf_clear.Tag = "anchor_right";
            this.meregepdf_clear.UseVisualStyleBackColor = true;
            // 
            // meregepdf_remove
            // 
            this.meregepdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.meregepdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.meregepdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.meregepdf_remove.Location = new System.Drawing.Point(847, 145);
            this.meregepdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.meregepdf_remove.Name = "meregepdf_remove";
            this.meregepdf_remove.Size = new System.Drawing.Size(23, 57);
            this.meregepdf_remove.TabIndex = 11;
            this.meregepdf_remove.Tag = "anchor_right";
            this.meregepdf_remove.UseVisualStyleBackColor = true;
            // 
            // MergePdfFileView
            // 
            this.MergePdfFileView.HideSelection = false;
            this.MergePdfFileView.Location = new System.Drawing.Point(4, 4);
            this.MergePdfFileView.Margin = new System.Windows.Forms.Padding(1);
            this.MergePdfFileView.Name = "MergePdfFileView";
            this.MergePdfFileView.Size = new System.Drawing.Size(841, 420);
            this.MergePdfFileView.TabIndex = 0;
            this.MergePdfFileView.Tag = "anchor_full_fill";
            this.MergePdfFileView.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_17
            // 
            this.TabPage_17.Controls.Add(this.groupBox47);
            this.TabPage_17.Controls.Add(this.SplitPdfFileList_MoveDown);
            this.TabPage_17.Controls.Add(this.SplitPdfFileList_MoveUp);
            this.TabPage_17.Controls.Add(this.sqplitpdf_clear);
            this.TabPage_17.Controls.Add(this.sqplitpdf_remove);
            this.TabPage_17.Controls.Add(this.SplitPdfStatusText);
            this.TabPage_17.Controls.Add(this.StartSplitPdf);
            this.TabPage_17.Controls.Add(this.SelectSplitPdf);
            this.TabPage_17.Controls.Add(this.SplitPdfFileList);
            this.TabPage_17.Location = new System.Drawing.Point(4, 76);
            this.TabPage_17.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_17.Name = "TabPage_17";
            this.TabPage_17.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_17.Size = new System.Drawing.Size(882, 659);
            this.TabPage_17.TabIndex = 16;
            this.TabPage_17.Text = "拆分PDF";
            this.TabPage_17.UseVisualStyleBackColor = true;
            // 
            // groupBox47
            // 
            this.groupBox47.Controls.Add(this.SplitPdf_ByBookMark);
            this.groupBox47.Controls.Add(this.label144);
            this.groupBox47.Controls.Add(this.label143);
            this.groupBox47.Controls.Add(this.SplitPdf_PerPageCount);
            this.groupBox47.Controls.Add(this.SplitPdf_Single);
            this.groupBox47.Controls.Add(this.label134);
            this.groupBox47.Controls.Add(this.SplitPdfRange_End);
            this.groupBox47.Controls.Add(this.label133);
            this.groupBox47.Controls.Add(this.SplitPdfRange_Start);
            this.groupBox47.Controls.Add(this.label132);
            this.groupBox47.Controls.Add(this.SplitPdf_Range);
            this.groupBox47.Location = new System.Drawing.Point(4, 427);
            this.groupBox47.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox47.Name = "groupBox47";
            this.groupBox47.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox47.Size = new System.Drawing.Size(360, 99);
            this.groupBox47.TabIndex = 13;
            this.groupBox47.TabStop = false;
            this.groupBox47.Tag = "anchor_left_bottom";
            this.groupBox47.Text = "拆分设置";
            // 
            // SplitPdf_ByBookMark
            // 
            this.SplitPdf_ByBookMark.AutoSize = true;
            this.SplitPdf_ByBookMark.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPdf_ByBookMark.Location = new System.Drawing.Point(4, 75);
            this.SplitPdf_ByBookMark.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdf_ByBookMark.Name = "SplitPdf_ByBookMark";
            this.SplitPdf_ByBookMark.Size = new System.Drawing.Size(119, 16);
            this.SplitPdf_ByBookMark.TabIndex = 10;
            this.SplitPdf_ByBookMark.Text = "按照书签名称拆分";
            this.SplitPdf_ByBookMark.UseVisualStyleBackColor = true;
            // 
            // label144
            // 
            this.label144.AutoSize = true;
            this.label144.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label144.Location = new System.Drawing.Point(231, 47);
            this.label144.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label144.Name = "label144";
            this.label144.Size = new System.Drawing.Size(101, 12);
            this.label144.TabIndex = 9;
            this.label144.Text = "页拆分为一个文件";
            // 
            // label143
            // 
            this.label143.AutoSize = true;
            this.label143.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label143.Location = new System.Drawing.Point(127, 47);
            this.label143.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label143.Name = "label143";
            this.label143.Size = new System.Drawing.Size(17, 12);
            this.label143.TabIndex = 8;
            this.label143.Text = "每";
            // 
            // SplitPdf_PerPageCount
            // 
            this.SplitPdf_PerPageCount.Location = new System.Drawing.Point(147, 44);
            this.SplitPdf_PerPageCount.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdf_PerPageCount.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.SplitPdf_PerPageCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.SplitPdf_PerPageCount.Name = "SplitPdf_PerPageCount";
            this.SplitPdf_PerPageCount.Size = new System.Drawing.Size(80, 21);
            this.SplitPdf_PerPageCount.TabIndex = 7;
            this.SplitPdf_PerPageCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // SplitPdf_Single
            // 
            this.SplitPdf_Single.AutoSize = true;
            this.SplitPdf_Single.Checked = true;
            this.SplitPdf_Single.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPdf_Single.Location = new System.Drawing.Point(4, 48);
            this.SplitPdf_Single.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdf_Single.Name = "SplitPdf_Single";
            this.SplitPdf_Single.Size = new System.Drawing.Size(71, 16);
            this.SplitPdf_Single.TabIndex = 6;
            this.SplitPdf_Single.TabStop = true;
            this.SplitPdf_Single.Text = "逐页拆分";
            this.SplitPdf_Single.UseVisualStyleBackColor = true;
            // 
            // label134
            // 
            this.label134.AutoSize = true;
            this.label134.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label134.Location = new System.Drawing.Point(231, 17);
            this.label134.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label134.Name = "label134";
            this.label134.Size = new System.Drawing.Size(17, 12);
            this.label134.TabIndex = 5;
            this.label134.Text = "页";
            // 
            // SplitPdfRange_End
            // 
            this.SplitPdfRange_End.Location = new System.Drawing.Point(200, 15);
            this.SplitPdfRange_End.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdfRange_End.Name = "SplitPdfRange_End";
            this.SplitPdfRange_End.Size = new System.Drawing.Size(28, 21);
            this.SplitPdfRange_End.TabIndex = 4;
            this.SplitPdfRange_End.Text = "2";
            // 
            // label133
            // 
            this.label133.AutoSize = true;
            this.label133.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label133.Location = new System.Drawing.Point(177, 17);
            this.label133.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label133.Name = "label133";
            this.label133.Size = new System.Drawing.Size(17, 12);
            this.label133.TabIndex = 3;
            this.label133.Text = "到";
            // 
            // SplitPdfRange_Start
            // 
            this.SplitPdfRange_Start.Location = new System.Drawing.Point(147, 15);
            this.SplitPdfRange_Start.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdfRange_Start.Name = "SplitPdfRange_Start";
            this.SplitPdfRange_Start.Size = new System.Drawing.Size(28, 21);
            this.SplitPdfRange_Start.TabIndex = 2;
            this.SplitPdfRange_Start.Text = "1";
            // 
            // label132
            // 
            this.label132.AutoSize = true;
            this.label132.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label132.Location = new System.Drawing.Point(127, 19);
            this.label132.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label132.Name = "label132";
            this.label132.Size = new System.Drawing.Size(17, 12);
            this.label132.TabIndex = 1;
            this.label132.Text = "从";
            // 
            // SplitPdf_Range
            // 
            this.SplitPdf_Range.AutoSize = true;
            this.SplitPdf_Range.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPdf_Range.Location = new System.Drawing.Point(4, 17);
            this.SplitPdf_Range.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdf_Range.Name = "SplitPdf_Range";
            this.SplitPdf_Range.Size = new System.Drawing.Size(119, 16);
            this.SplitPdf_Range.TabIndex = 0;
            this.SplitPdf_Range.Text = "按照页面范围拆分";
            this.SplitPdf_Range.UseVisualStyleBackColor = true;
            // 
            // SplitPdfFileList_MoveDown
            // 
            this.SplitPdfFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPdfFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.SplitPdfFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdfFileList_MoveDown.Name = "SplitPdfFileList_MoveDown";
            this.SplitPdfFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.SplitPdfFileList_MoveDown.TabIndex = 5;
            this.SplitPdfFileList_MoveDown.Tag = "anchor_right";
            this.SplitPdfFileList_MoveDown.Text = "▽";
            this.SplitPdfFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // SplitPdfFileList_MoveUp
            // 
            this.SplitPdfFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPdfFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.SplitPdfFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdfFileList_MoveUp.Name = "SplitPdfFileList_MoveUp";
            this.SplitPdfFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.SplitPdfFileList_MoveUp.TabIndex = 4;
            this.SplitPdfFileList_MoveUp.Tag = "anchor_right";
            this.SplitPdfFileList_MoveUp.Text = "△";
            this.SplitPdfFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // sqplitpdf_clear
            // 
            this.sqplitpdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.sqplitpdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.sqplitpdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.sqplitpdf_clear.Location = new System.Drawing.Point(847, 225);
            this.sqplitpdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.sqplitpdf_clear.Name = "sqplitpdf_clear";
            this.sqplitpdf_clear.Size = new System.Drawing.Size(23, 57);
            this.sqplitpdf_clear.TabIndex = 12;
            this.sqplitpdf_clear.Tag = "anchor_right";
            this.sqplitpdf_clear.UseVisualStyleBackColor = true;
            // 
            // sqplitpdf_remove
            // 
            this.sqplitpdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.sqplitpdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.sqplitpdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.sqplitpdf_remove.Location = new System.Drawing.Point(847, 145);
            this.sqplitpdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.sqplitpdf_remove.Name = "sqplitpdf_remove";
            this.sqplitpdf_remove.Size = new System.Drawing.Size(23, 57);
            this.sqplitpdf_remove.TabIndex = 11;
            this.sqplitpdf_remove.Tag = "anchor_right";
            this.sqplitpdf_remove.UseVisualStyleBackColor = true;
            // 
            // SplitPdfStatusText
            // 
            this.SplitPdfStatusText.AutoSize = true;
            this.SplitPdfStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.SplitPdfStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPdfStatusText.Location = new System.Drawing.Point(156, 575);
            this.SplitPdfStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SplitPdfStatusText.Name = "SplitPdfStatusText";
            this.SplitPdfStatusText.Size = new System.Drawing.Size(95, 12);
            this.SplitPdfStatusText.TabIndex = 3;
            this.SplitPdfStatusText.Tag = "anchor_left_bottom";
            this.SplitPdfStatusText.Text = "正在处理X个文件";
            // 
            // StartSplitPdf
            // 
            this.StartSplitPdf.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StartSplitPdf.Location = new System.Drawing.Point(697, 561);
            this.StartSplitPdf.Margin = new System.Windows.Forms.Padding(1);
            this.StartSplitPdf.Name = "StartSplitPdf";
            this.StartSplitPdf.Size = new System.Drawing.Size(147, 37);
            this.StartSplitPdf.TabIndex = 2;
            this.StartSplitPdf.Tag = "anchor_right_bottom";
            this.StartSplitPdf.Text = "开始处理";
            this.StartSplitPdf.UseVisualStyleBackColor = true;
            this.StartSplitPdf.Click += new System.EventHandler(this.StartSplitPdf_Click);
            // 
            // SelectSplitPdf
            // 
            this.SelectSplitPdf.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectSplitPdf.Location = new System.Drawing.Point(5, 561);
            this.SelectSplitPdf.Margin = new System.Windows.Forms.Padding(1);
            this.SelectSplitPdf.Name = "SelectSplitPdf";
            this.SelectSplitPdf.Size = new System.Drawing.Size(147, 37);
            this.SelectSplitPdf.TabIndex = 1;
            this.SelectSplitPdf.Tag = "anchor_left_bottom";
            this.SelectSplitPdf.Text = "选择文件";
            this.SelectSplitPdf.UseVisualStyleBackColor = true;
            this.SelectSplitPdf.Click += new System.EventHandler(this.SelectSplitPdf_Click);
            // 
            // SplitPdfFileList
            // 
            this.SplitPdfFileList.HideSelection = false;
            this.SplitPdfFileList.Location = new System.Drawing.Point(4, 4);
            this.SplitPdfFileList.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdfFileList.Name = "SplitPdfFileList";
            this.SplitPdfFileList.Size = new System.Drawing.Size(841, 420);
            this.SplitPdfFileList.TabIndex = 0;
            this.SplitPdfFileList.Tag = "anchor_full_fill";
            this.SplitPdfFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_41
            // 
            this.TabPage_41.Controls.Add(this.groupBox62);
            this.TabPage_41.Controls.Add(this.groupBox61);
            this.TabPage_41.Controls.Add(this.SplitPPTFilesList_MoveDown);
            this.TabPage_41.Controls.Add(this.SplitPPTFilesList_MoveUp);
            this.TabPage_41.Controls.Add(this.splitppt_clear);
            this.TabPage_41.Controls.Add(this.splitppt_remove);
            this.TabPage_41.Controls.Add(this.SplitPPTStatusText);
            this.TabPage_41.Controls.Add(this.SplitPPT_Button);
            this.TabPage_41.Controls.Add(this.SplitPPT_SelectFiles);
            this.TabPage_41.Controls.Add(this.SplitPPTFilesList);
            this.TabPage_41.Location = new System.Drawing.Point(4, 76);
            this.TabPage_41.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_41.Name = "TabPage_41";
            this.TabPage_41.Size = new System.Drawing.Size(882, 659);
            this.TabPage_41.TabIndex = 31;
            this.TabPage_41.Text = "拆分PPT";
            this.TabPage_41.UseVisualStyleBackColor = true;
            // 
            // groupBox62
            // 
            this.groupBox62.Controls.Add(this.label178);
            this.groupBox62.Controls.Add(this.label177);
            this.groupBox62.Controls.Add(this.SplitPdfScaleCombox);
            this.groupBox62.Controls.Add(this.SplitPdfSizeCombox);
            this.groupBox62.Location = new System.Drawing.Point(265, 427);
            this.groupBox62.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox62.Name = "groupBox62";
            this.groupBox62.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox62.Size = new System.Drawing.Size(201, 77);
            this.groupBox62.TabIndex = 33;
            this.groupBox62.TabStop = false;
            this.groupBox62.Tag = "anchor_left_bottom";
            this.groupBox62.Text = "页面设置";
            // 
            // label178
            // 
            this.label178.AutoSize = true;
            this.label178.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label178.Location = new System.Drawing.Point(4, 51);
            this.label178.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label178.Name = "label178";
            this.label178.Size = new System.Drawing.Size(53, 12);
            this.label178.TabIndex = 3;
            this.label178.Text = "缩放模式";
            // 
            // label177
            // 
            this.label177.AutoSize = true;
            this.label177.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label177.Location = new System.Drawing.Point(4, 23);
            this.label177.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label177.Name = "label177";
            this.label177.Size = new System.Drawing.Size(53, 12);
            this.label177.TabIndex = 2;
            this.label177.Text = "尺寸大小";
            // 
            // SplitPdfScaleCombox
            // 
            this.SplitPdfScaleCombox.FormattingEnabled = true;
            this.SplitPdfScaleCombox.Location = new System.Drawing.Point(61, 47);
            this.SplitPdfScaleCombox.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdfScaleCombox.Name = "SplitPdfScaleCombox";
            this.SplitPdfScaleCombox.Size = new System.Drawing.Size(123, 20);
            this.SplitPdfScaleCombox.TabIndex = 1;
            // 
            // SplitPdfSizeCombox
            // 
            this.SplitPdfSizeCombox.FormattingEnabled = true;
            this.SplitPdfSizeCombox.Location = new System.Drawing.Point(61, 19);
            this.SplitPdfSizeCombox.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPdfSizeCombox.Name = "SplitPdfSizeCombox";
            this.SplitPdfSizeCombox.Size = new System.Drawing.Size(123, 20);
            this.SplitPdfSizeCombox.TabIndex = 0;
            // 
            // groupBox61
            // 
            this.groupBox61.Controls.Add(this.SplitPPT_SignleExport);
            this.groupBox61.Controls.Add(this.label179);
            this.groupBox61.Controls.Add(this.SplitPPT_RangeExport_End);
            this.groupBox61.Controls.Add(this.label180);
            this.groupBox61.Controls.Add(this.SplitPPT_RangeExport_Start);
            this.groupBox61.Controls.Add(this.label181);
            this.groupBox61.Controls.Add(this.radioButton2);
            this.groupBox61.Location = new System.Drawing.Point(4, 427);
            this.groupBox61.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox61.Name = "groupBox61";
            this.groupBox61.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox61.Size = new System.Drawing.Size(257, 77);
            this.groupBox61.TabIndex = 31;
            this.groupBox61.TabStop = false;
            this.groupBox61.Tag = "anchor_left_bottom";
            this.groupBox61.Text = "拆分设置";
            // 
            // SplitPPT_SignleExport
            // 
            this.SplitPPT_SignleExport.AutoSize = true;
            this.SplitPPT_SignleExport.Checked = true;
            this.SplitPPT_SignleExport.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPPT_SignleExport.Location = new System.Drawing.Point(4, 48);
            this.SplitPPT_SignleExport.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPT_SignleExport.Name = "SplitPPT_SignleExport";
            this.SplitPPT_SignleExport.Size = new System.Drawing.Size(71, 16);
            this.SplitPPT_SignleExport.TabIndex = 6;
            this.SplitPPT_SignleExport.TabStop = true;
            this.SplitPPT_SignleExport.Text = "逐页拆分";
            this.SplitPPT_SignleExport.UseVisualStyleBackColor = true;
            // 
            // label179
            // 
            this.label179.AutoSize = true;
            this.label179.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label179.Location = new System.Drawing.Point(231, 17);
            this.label179.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label179.Name = "label179";
            this.label179.Size = new System.Drawing.Size(17, 12);
            this.label179.TabIndex = 5;
            this.label179.Text = "页";
            // 
            // SplitPPT_RangeExport_End
            // 
            this.SplitPPT_RangeExport_End.Location = new System.Drawing.Point(200, 15);
            this.SplitPPT_RangeExport_End.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPT_RangeExport_End.Name = "SplitPPT_RangeExport_End";
            this.SplitPPT_RangeExport_End.Size = new System.Drawing.Size(28, 21);
            this.SplitPPT_RangeExport_End.TabIndex = 4;
            this.SplitPPT_RangeExport_End.Text = "2";
            // 
            // label180
            // 
            this.label180.AutoSize = true;
            this.label180.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label180.Location = new System.Drawing.Point(177, 17);
            this.label180.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label180.Name = "label180";
            this.label180.Size = new System.Drawing.Size(17, 12);
            this.label180.TabIndex = 3;
            this.label180.Text = "到";
            // 
            // SplitPPT_RangeExport_Start
            // 
            this.SplitPPT_RangeExport_Start.Location = new System.Drawing.Point(147, 15);
            this.SplitPPT_RangeExport_Start.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPT_RangeExport_Start.Name = "SplitPPT_RangeExport_Start";
            this.SplitPPT_RangeExport_Start.Size = new System.Drawing.Size(28, 21);
            this.SplitPPT_RangeExport_Start.TabIndex = 2;
            this.SplitPPT_RangeExport_Start.Text = "1";
            // 
            // label181
            // 
            this.label181.AutoSize = true;
            this.label181.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label181.Location = new System.Drawing.Point(127, 19);
            this.label181.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label181.Name = "label181";
            this.label181.Size = new System.Drawing.Size(17, 12);
            this.label181.TabIndex = 1;
            this.label181.Text = "从";
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.radioButton2.Location = new System.Drawing.Point(4, 17);
            this.radioButton2.Margin = new System.Windows.Forms.Padding(1);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(119, 16);
            this.radioButton2.TabIndex = 0;
            this.radioButton2.Text = "按照页面范围拆分";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // SplitPPTFilesList_MoveDown
            // 
            this.SplitPPTFilesList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPPTFilesList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.SplitPPTFilesList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPTFilesList_MoveDown.Name = "SplitPPTFilesList_MoveDown";
            this.SplitPPTFilesList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.SplitPPTFilesList_MoveDown.TabIndex = 28;
            this.SplitPPTFilesList_MoveDown.Tag = "anchor_right";
            this.SplitPPTFilesList_MoveDown.Text = "▽";
            this.SplitPPTFilesList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // SplitPPTFilesList_MoveUp
            // 
            this.SplitPPTFilesList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPPTFilesList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.SplitPPTFilesList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPTFilesList_MoveUp.Name = "SplitPPTFilesList_MoveUp";
            this.SplitPPTFilesList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.SplitPPTFilesList_MoveUp.TabIndex = 27;
            this.SplitPPTFilesList_MoveUp.Tag = "anchor_right";
            this.SplitPPTFilesList_MoveUp.Text = "△";
            this.SplitPPTFilesList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // splitppt_clear
            // 
            this.splitppt_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.splitppt_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.splitppt_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.splitppt_clear.Location = new System.Drawing.Point(847, 225);
            this.splitppt_clear.Margin = new System.Windows.Forms.Padding(1);
            this.splitppt_clear.Name = "splitppt_clear";
            this.splitppt_clear.Size = new System.Drawing.Size(23, 57);
            this.splitppt_clear.TabIndex = 30;
            this.splitppt_clear.Tag = "anchor_right";
            this.splitppt_clear.UseVisualStyleBackColor = true;
            // 
            // splitppt_remove
            // 
            this.splitppt_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.splitppt_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.splitppt_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.splitppt_remove.Location = new System.Drawing.Point(847, 145);
            this.splitppt_remove.Margin = new System.Windows.Forms.Padding(1);
            this.splitppt_remove.Name = "splitppt_remove";
            this.splitppt_remove.Size = new System.Drawing.Size(23, 57);
            this.splitppt_remove.TabIndex = 29;
            this.splitppt_remove.Tag = "anchor_right";
            this.splitppt_remove.UseVisualStyleBackColor = true;
            // 
            // SplitPPTStatusText
            // 
            this.SplitPPTStatusText.AutoSize = true;
            this.SplitPPTStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.SplitPPTStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPPTStatusText.Location = new System.Drawing.Point(156, 575);
            this.SplitPPTStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SplitPPTStatusText.Name = "SplitPPTStatusText";
            this.SplitPPTStatusText.Size = new System.Drawing.Size(95, 12);
            this.SplitPPTStatusText.TabIndex = 26;
            this.SplitPPTStatusText.Tag = "anchor_left_bottom";
            this.SplitPPTStatusText.Text = "正在处理X个文件";
            // 
            // SplitPPT_Button
            // 
            this.SplitPPT_Button.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPPT_Button.Location = new System.Drawing.Point(697, 561);
            this.SplitPPT_Button.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPT_Button.Name = "SplitPPT_Button";
            this.SplitPPT_Button.Size = new System.Drawing.Size(147, 37);
            this.SplitPPT_Button.TabIndex = 25;
            this.SplitPPT_Button.Tag = "anchor_right_bottom";
            this.SplitPPT_Button.Text = "开始处理";
            this.SplitPPT_Button.UseVisualStyleBackColor = true;
            this.SplitPPT_Button.Click += new System.EventHandler(this.SplitPPT_Button_Click);
            // 
            // SplitPPT_SelectFiles
            // 
            this.SplitPPT_SelectFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SplitPPT_SelectFiles.Location = new System.Drawing.Point(5, 561);
            this.SplitPPT_SelectFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPT_SelectFiles.Name = "SplitPPT_SelectFiles";
            this.SplitPPT_SelectFiles.Size = new System.Drawing.Size(147, 37);
            this.SplitPPT_SelectFiles.TabIndex = 24;
            this.SplitPPT_SelectFiles.Tag = "anchor_left_bottom";
            this.SplitPPT_SelectFiles.Text = "选择文件";
            this.SplitPPT_SelectFiles.UseVisualStyleBackColor = true;
            this.SplitPPT_SelectFiles.Click += new System.EventHandler(this.SplitPPT_SelectFiles_Click);
            // 
            // SplitPPTFilesList
            // 
            this.SplitPPTFilesList.HideSelection = false;
            this.SplitPPTFilesList.Location = new System.Drawing.Point(4, 4);
            this.SplitPPTFilesList.Margin = new System.Windows.Forms.Padding(1);
            this.SplitPPTFilesList.Name = "SplitPPTFilesList";
            this.SplitPPTFilesList.Size = new System.Drawing.Size(841, 420);
            this.SplitPPTFilesList.TabIndex = 23;
            this.SplitPPTFilesList.Tag = "anchor_full_fill";
            this.SplitPPTFilesList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_18
            // 
            this.TabPage_18.Controls.Add(this.PdfWaterMarkFileList_MoveDown);
            this.TabPage_18.Controls.Add(this.PdfWaterMarkFileList_MoveUp);
            this.TabPage_18.Controls.Add(this.addpdfwater_clear);
            this.TabPage_18.Controls.Add(this.addpdfwater_remove);
            this.TabPage_18.Controls.Add(this.label31);
            this.TabPage_18.Controls.Add(this.AddWaterMark2PdfStatusText);
            this.TabPage_18.Controls.Add(this.AddWaterMark2Pdf);
            this.TabPage_18.Controls.Add(this.SelectWaterMarkPdfFiles);
            this.TabPage_18.Controls.Add(this.groupBox18);
            this.TabPage_18.Controls.Add(this.PdfWaterMarkFileList);
            this.TabPage_18.Location = new System.Drawing.Point(4, 76);
            this.TabPage_18.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_18.Name = "TabPage_18";
            this.TabPage_18.Size = new System.Drawing.Size(882, 659);
            this.TabPage_18.TabIndex = 17;
            this.TabPage_18.Text = "添加PDF水印";
            this.TabPage_18.UseVisualStyleBackColor = true;
            // 
            // PdfWaterMarkFileList_MoveDown
            // 
            this.PdfWaterMarkFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PdfWaterMarkFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.PdfWaterMarkFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkFileList_MoveDown.Name = "PdfWaterMarkFileList_MoveDown";
            this.PdfWaterMarkFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.PdfWaterMarkFileList_MoveDown.TabIndex = 17;
            this.PdfWaterMarkFileList_MoveDown.Tag = "anchor_right";
            this.PdfWaterMarkFileList_MoveDown.Text = "▽";
            this.PdfWaterMarkFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // PdfWaterMarkFileList_MoveUp
            // 
            this.PdfWaterMarkFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PdfWaterMarkFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.PdfWaterMarkFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkFileList_MoveUp.Name = "PdfWaterMarkFileList_MoveUp";
            this.PdfWaterMarkFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.PdfWaterMarkFileList_MoveUp.TabIndex = 16;
            this.PdfWaterMarkFileList_MoveUp.Tag = "anchor_right";
            this.PdfWaterMarkFileList_MoveUp.Text = "△";
            this.PdfWaterMarkFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // addpdfwater_clear
            // 
            this.addpdfwater_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.addpdfwater_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.addpdfwater_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.addpdfwater_clear.Location = new System.Drawing.Point(847, 225);
            this.addpdfwater_clear.Margin = new System.Windows.Forms.Padding(1);
            this.addpdfwater_clear.Name = "addpdfwater_clear";
            this.addpdfwater_clear.Size = new System.Drawing.Size(23, 57);
            this.addpdfwater_clear.TabIndex = 12;
            this.addpdfwater_clear.Tag = "anchor_right";
            this.addpdfwater_clear.UseVisualStyleBackColor = true;
            // 
            // addpdfwater_remove
            // 
            this.addpdfwater_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.addpdfwater_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.addpdfwater_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.addpdfwater_remove.Location = new System.Drawing.Point(847, 145);
            this.addpdfwater_remove.Margin = new System.Windows.Forms.Padding(1);
            this.addpdfwater_remove.Name = "addpdfwater_remove";
            this.addpdfwater_remove.Size = new System.Drawing.Size(23, 57);
            this.addpdfwater_remove.TabIndex = 11;
            this.addpdfwater_remove.Tag = "anchor_right";
            this.addpdfwater_remove.UseVisualStyleBackColor = true;
            // 
            // label31
            // 
            this.label31.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label31.AutoSize = true;
            this.label31.ForeColor = System.Drawing.Color.Tomato;
            this.label31.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label31.Location = new System.Drawing.Point(273, 883);
            this.label31.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(161, 36);
            this.label31.TabIndex = 15;
            this.label31.Text = "提示：如果不设置字体和颜色\r\n\r\n默认为宋体、12号、红色";
            // 
            // AddWaterMark2PdfStatusText
            // 
            this.AddWaterMark2PdfStatusText.AutoSize = true;
            this.AddWaterMark2PdfStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.AddWaterMark2PdfStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.AddWaterMark2PdfStatusText.Location = new System.Drawing.Point(156, 575);
            this.AddWaterMark2PdfStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.AddWaterMark2PdfStatusText.Name = "AddWaterMark2PdfStatusText";
            this.AddWaterMark2PdfStatusText.Size = new System.Drawing.Size(95, 12);
            this.AddWaterMark2PdfStatusText.TabIndex = 4;
            this.AddWaterMark2PdfStatusText.Tag = "anchor_left_bottom";
            this.AddWaterMark2PdfStatusText.Text = "正在处理X个文件";
            // 
            // AddWaterMark2Pdf
            // 
            this.AddWaterMark2Pdf.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.AddWaterMark2Pdf.Location = new System.Drawing.Point(697, 561);
            this.AddWaterMark2Pdf.Margin = new System.Windows.Forms.Padding(1);
            this.AddWaterMark2Pdf.Name = "AddWaterMark2Pdf";
            this.AddWaterMark2Pdf.Size = new System.Drawing.Size(147, 37);
            this.AddWaterMark2Pdf.TabIndex = 3;
            this.AddWaterMark2Pdf.Tag = "anchor_right_bottom";
            this.AddWaterMark2Pdf.Text = "开始处理";
            this.AddWaterMark2Pdf.UseVisualStyleBackColor = true;
            this.AddWaterMark2Pdf.Click += new System.EventHandler(this.AddWaterMark2Pdf_Click);
            // 
            // SelectWaterMarkPdfFiles
            // 
            this.SelectWaterMarkPdfFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectWaterMarkPdfFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectWaterMarkPdfFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectWaterMarkPdfFiles.Name = "SelectWaterMarkPdfFiles";
            this.SelectWaterMarkPdfFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectWaterMarkPdfFiles.TabIndex = 2;
            this.SelectWaterMarkPdfFiles.Tag = "anchor_left_bottom";
            this.SelectWaterMarkPdfFiles.Text = "选择文件";
            this.SelectWaterMarkPdfFiles.UseVisualStyleBackColor = true;
            this.SelectWaterMarkPdfFiles.Click += new System.EventHandler(this.SelectWaterMarkPdfFiles_Click);
            // 
            // groupBox18
            // 
            this.groupBox18.Controls.Add(this.label33);
            this.groupBox18.Controls.Add(this.label32);
            this.groupBox18.Controls.Add(this.WaterMarkImageWidth);
            this.groupBox18.Controls.Add(this.AddBackGroudImage);
            this.groupBox18.Controls.Add(this.WaterMarkImageHeight);
            this.groupBox18.Controls.Add(this.WaterMarkImage);
            this.groupBox18.Controls.Add(this.PdfWaterMarkOpcityNumber);
            this.groupBox18.Controls.Add(this.label30);
            this.groupBox18.Controls.Add(this.PdfWaterMarkRotateDegNumber);
            this.groupBox18.Controls.Add(this.label29);
            this.groupBox18.Controls.Add(this.PDFWaterMarkVerSetting);
            this.groupBox18.Controls.Add(this.PdfWaterMarkHorSetting);
            this.groupBox18.Controls.Add(this.label28);
            this.groupBox18.Controls.Add(this.shui);
            this.groupBox18.Controls.Add(this.PDFWaterMarkTextFontSetting);
            this.groupBox18.Controls.Add(this.PdfWaterMarkFontColorSettings);
            this.groupBox18.Controls.Add(this.label27);
            this.groupBox18.Controls.Add(this.label26);
            this.groupBox18.Controls.Add(this.label25);
            this.groupBox18.Controls.Add(this.PdfWaterMarkText);
            this.groupBox18.Location = new System.Drawing.Point(4, 427);
            this.groupBox18.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox18.Name = "groupBox18";
            this.groupBox18.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox18.Size = new System.Drawing.Size(839, 105);
            this.groupBox18.TabIndex = 1;
            this.groupBox18.TabStop = false;
            this.groupBox18.Tag = "anchor_left_bottom";
            this.groupBox18.Text = "水印设置";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label33.Location = new System.Drawing.Point(493, 87);
            this.label33.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(53, 12);
            this.label33.TabIndex = 20;
            this.label33.Text = "图片宽度";
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label32.Location = new System.Drawing.Point(493, 57);
            this.label32.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(53, 12);
            this.label32.TabIndex = 19;
            this.label32.Text = "图片高度";
            // 
            // WaterMarkImageWidth
            // 
            this.WaterMarkImageWidth.Location = new System.Drawing.Point(551, 83);
            this.WaterMarkImageWidth.Margin = new System.Windows.Forms.Padding(1);
            this.WaterMarkImageWidth.Name = "WaterMarkImageWidth";
            this.WaterMarkImageWidth.Size = new System.Drawing.Size(83, 21);
            this.WaterMarkImageWidth.TabIndex = 18;
            // 
            // AddBackGroudImage
            // 
            this.AddBackGroudImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.AddBackGroudImage.Location = new System.Drawing.Point(641, 80);
            this.AddBackGroudImage.Margin = new System.Windows.Forms.Padding(1);
            this.AddBackGroudImage.Name = "AddBackGroudImage";
            this.AddBackGroudImage.Size = new System.Drawing.Size(136, 23);
            this.AddBackGroudImage.TabIndex = 15;
            this.AddBackGroudImage.Text = "选择图片水印...";
            this.AddBackGroudImage.UseVisualStyleBackColor = true;
            this.AddBackGroudImage.Click += new System.EventHandler(this.AddBackGroudImage_Click);
            // 
            // WaterMarkImageHeight
            // 
            this.WaterMarkImageHeight.Location = new System.Drawing.Point(551, 53);
            this.WaterMarkImageHeight.Margin = new System.Windows.Forms.Padding(1);
            this.WaterMarkImageHeight.Name = "WaterMarkImageHeight";
            this.WaterMarkImageHeight.Size = new System.Drawing.Size(83, 21);
            this.WaterMarkImageHeight.TabIndex = 17;
            // 
            // WaterMarkImage
            // 
            this.WaterMarkImage.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.WaterMarkImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.WaterMarkImage.Location = new System.Drawing.Point(641, 23);
            this.WaterMarkImage.Margin = new System.Windows.Forms.Padding(1);
            this.WaterMarkImage.Name = "WaterMarkImage";
            this.WaterMarkImage.Size = new System.Drawing.Size(136, 57);
            this.WaterMarkImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.WaterMarkImage.TabIndex = 16;
            this.WaterMarkImage.TabStop = false;
            // 
            // PdfWaterMarkOpcityNumber
            // 
            this.PdfWaterMarkOpcityNumber.FormattingEnabled = true;
            this.PdfWaterMarkOpcityNumber.Location = new System.Drawing.Point(551, 23);
            this.PdfWaterMarkOpcityNumber.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkOpcityNumber.Name = "PdfWaterMarkOpcityNumber";
            this.PdfWaterMarkOpcityNumber.Size = new System.Drawing.Size(83, 20);
            this.PdfWaterMarkOpcityNumber.TabIndex = 14;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label30.Location = new System.Drawing.Point(493, 27);
            this.label30.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(53, 12);
            this.label30.TabIndex = 13;
            this.label30.Text = "不透明度";
            // 
            // PdfWaterMarkRotateDegNumber
            // 
            this.PdfWaterMarkRotateDegNumber.Location = new System.Drawing.Point(327, 76);
            this.PdfWaterMarkRotateDegNumber.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkRotateDegNumber.Name = "PdfWaterMarkRotateDegNumber";
            this.PdfWaterMarkRotateDegNumber.Size = new System.Drawing.Size(83, 21);
            this.PdfWaterMarkRotateDegNumber.TabIndex = 12;
            this.PdfWaterMarkRotateDegNumber.Text = "45";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label29.Location = new System.Drawing.Point(245, 83);
            this.label29.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(77, 12);
            this.label29.TabIndex = 11;
            this.label29.Text = "水印旋转角度";
            // 
            // PDFWaterMarkVerSetting
            // 
            this.PDFWaterMarkVerSetting.FormattingEnabled = true;
            this.PDFWaterMarkVerSetting.Location = new System.Drawing.Point(327, 51);
            this.PDFWaterMarkVerSetting.Margin = new System.Windows.Forms.Padding(1);
            this.PDFWaterMarkVerSetting.Name = "PDFWaterMarkVerSetting";
            this.PDFWaterMarkVerSetting.Size = new System.Drawing.Size(83, 20);
            this.PDFWaterMarkVerSetting.TabIndex = 10;
            // 
            // PdfWaterMarkHorSetting
            // 
            this.PdfWaterMarkHorSetting.FormattingEnabled = true;
            this.PdfWaterMarkHorSetting.Location = new System.Drawing.Point(327, 24);
            this.PdfWaterMarkHorSetting.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkHorSetting.Name = "PdfWaterMarkHorSetting";
            this.PdfWaterMarkHorSetting.Size = new System.Drawing.Size(83, 20);
            this.PdfWaterMarkHorSetting.TabIndex = 9;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label28.Location = new System.Drawing.Point(243, 55);
            this.label28.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(77, 12);
            this.label28.TabIndex = 8;
            this.label28.Text = "垂直对齐方式";
            // 
            // shui
            // 
            this.shui.AutoSize = true;
            this.shui.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.shui.Location = new System.Drawing.Point(245, 27);
            this.shui.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.shui.Name = "shui";
            this.shui.Size = new System.Drawing.Size(77, 12);
            this.shui.TabIndex = 7;
            this.shui.Text = "水平对齐方式";
            // 
            // PDFWaterMarkTextFontSetting
            // 
            this.PDFWaterMarkTextFontSetting.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PDFWaterMarkTextFontSetting.Location = new System.Drawing.Point(61, 53);
            this.PDFWaterMarkTextFontSetting.Margin = new System.Windows.Forms.Padding(1);
            this.PDFWaterMarkTextFontSetting.Name = "PDFWaterMarkTextFontSetting";
            this.PDFWaterMarkTextFontSetting.Size = new System.Drawing.Size(112, 17);
            this.PDFWaterMarkTextFontSetting.TabIndex = 6;
            this.PDFWaterMarkTextFontSetting.Text = "设置...";
            this.PDFWaterMarkTextFontSetting.UseVisualStyleBackColor = true;
            this.PDFWaterMarkTextFontSetting.Click += new System.EventHandler(this.PDFWaterMarkTextFontSetting_Click);
            // 
            // PdfWaterMarkFontColorSettings
            // 
            this.PdfWaterMarkFontColorSettings.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PdfWaterMarkFontColorSettings.Location = new System.Drawing.Point(63, 79);
            this.PdfWaterMarkFontColorSettings.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkFontColorSettings.Name = "PdfWaterMarkFontColorSettings";
            this.PdfWaterMarkFontColorSettings.Size = new System.Drawing.Size(111, 17);
            this.PdfWaterMarkFontColorSettings.TabIndex = 5;
            this.PdfWaterMarkFontColorSettings.Text = "设置...";
            this.PdfWaterMarkFontColorSettings.UseVisualStyleBackColor = true;
            this.PdfWaterMarkFontColorSettings.Click += new System.EventHandler(this.PdfWaterMarkFontColorSettings_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label27.Location = new System.Drawing.Point(4, 83);
            this.label27.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(53, 12);
            this.label27.TabIndex = 4;
            this.label27.Text = "设置颜色";
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label26.Location = new System.Drawing.Point(4, 59);
            this.label26.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(53, 12);
            this.label26.TabIndex = 2;
            this.label26.Text = "设置字体";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label25.Location = new System.Drawing.Point(4, 29);
            this.label25.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(53, 12);
            this.label25.TabIndex = 1;
            this.label25.Text = "水印内容";
            // 
            // PdfWaterMarkText
            // 
            this.PdfWaterMarkText.Location = new System.Drawing.Point(61, 27);
            this.PdfWaterMarkText.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkText.Name = "PdfWaterMarkText";
            this.PdfWaterMarkText.Size = new System.Drawing.Size(113, 21);
            this.PdfWaterMarkText.TabIndex = 0;
            // 
            // PdfWaterMarkFileList
            // 
            this.PdfWaterMarkFileList.HideSelection = false;
            this.PdfWaterMarkFileList.Location = new System.Drawing.Point(4, 4);
            this.PdfWaterMarkFileList.Margin = new System.Windows.Forms.Padding(1);
            this.PdfWaterMarkFileList.Name = "PdfWaterMarkFileList";
            this.PdfWaterMarkFileList.Size = new System.Drawing.Size(841, 420);
            this.PdfWaterMarkFileList.TabIndex = 0;
            this.PdfWaterMarkFileList.Tag = "anchor_full_fill";
            this.PdfWaterMarkFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_19
            // 
            this.TabPage_19.Controls.Add(this.RemovePdfWaterMarkFileList_MoveDown);
            this.TabPage_19.Controls.Add(this.RemovePdfWaterMarkFileList_MoveUp);
            this.TabPage_19.Controls.Add(this.removepdfwater_clear);
            this.TabPage_19.Controls.Add(this.removepdfwater_remove);
            this.TabPage_19.Controls.Add(this.RemovePdfWaterMarkStatusText);
            this.TabPage_19.Controls.Add(this.RemovePdfWaterMark);
            this.TabPage_19.Controls.Add(this.SelectRemovePdfWaterMarkFiles);
            this.TabPage_19.Controls.Add(this.RemovePdfWaterMarkFileList);
            this.TabPage_19.Location = new System.Drawing.Point(4, 76);
            this.TabPage_19.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_19.Name = "TabPage_19";
            this.TabPage_19.Size = new System.Drawing.Size(882, 659);
            this.TabPage_19.TabIndex = 18;
            this.TabPage_19.Text = "移除PDF水印";
            this.TabPage_19.UseVisualStyleBackColor = true;
            // 
            // RemovePdfWaterMarkFileList_MoveDown
            // 
            this.RemovePdfWaterMarkFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemovePdfWaterMarkFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.RemovePdfWaterMarkFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.RemovePdfWaterMarkFileList_MoveDown.Name = "RemovePdfWaterMarkFileList_MoveDown";
            this.RemovePdfWaterMarkFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.RemovePdfWaterMarkFileList_MoveDown.TabIndex = 5;
            this.RemovePdfWaterMarkFileList_MoveDown.Tag = "anchor_right";
            this.RemovePdfWaterMarkFileList_MoveDown.Text = "▽";
            this.RemovePdfWaterMarkFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // RemovePdfWaterMarkFileList_MoveUp
            // 
            this.RemovePdfWaterMarkFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemovePdfWaterMarkFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.RemovePdfWaterMarkFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.RemovePdfWaterMarkFileList_MoveUp.Name = "RemovePdfWaterMarkFileList_MoveUp";
            this.RemovePdfWaterMarkFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.RemovePdfWaterMarkFileList_MoveUp.TabIndex = 4;
            this.RemovePdfWaterMarkFileList_MoveUp.Tag = "anchor_right";
            this.RemovePdfWaterMarkFileList_MoveUp.Text = "△";
            this.RemovePdfWaterMarkFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // removepdfwater_clear
            // 
            this.removepdfwater_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.removepdfwater_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.removepdfwater_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.removepdfwater_clear.Location = new System.Drawing.Point(847, 225);
            this.removepdfwater_clear.Margin = new System.Windows.Forms.Padding(1);
            this.removepdfwater_clear.Name = "removepdfwater_clear";
            this.removepdfwater_clear.Size = new System.Drawing.Size(23, 57);
            this.removepdfwater_clear.TabIndex = 12;
            this.removepdfwater_clear.Tag = "anchor_right";
            this.removepdfwater_clear.UseVisualStyleBackColor = true;
            // 
            // removepdfwater_remove
            // 
            this.removepdfwater_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.removepdfwater_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.removepdfwater_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.removepdfwater_remove.Location = new System.Drawing.Point(847, 145);
            this.removepdfwater_remove.Margin = new System.Windows.Forms.Padding(1);
            this.removepdfwater_remove.Name = "removepdfwater_remove";
            this.removepdfwater_remove.Size = new System.Drawing.Size(23, 57);
            this.removepdfwater_remove.TabIndex = 11;
            this.removepdfwater_remove.Tag = "anchor_right";
            this.removepdfwater_remove.UseVisualStyleBackColor = true;
            // 
            // RemovePdfWaterMarkStatusText
            // 
            this.RemovePdfWaterMarkStatusText.AutoSize = true;
            this.RemovePdfWaterMarkStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.RemovePdfWaterMarkStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemovePdfWaterMarkStatusText.Location = new System.Drawing.Point(156, 575);
            this.RemovePdfWaterMarkStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.RemovePdfWaterMarkStatusText.Name = "RemovePdfWaterMarkStatusText";
            this.RemovePdfWaterMarkStatusText.Size = new System.Drawing.Size(95, 12);
            this.RemovePdfWaterMarkStatusText.TabIndex = 3;
            this.RemovePdfWaterMarkStatusText.Tag = "anchor_left_bottom";
            this.RemovePdfWaterMarkStatusText.Text = "正在处理X个文件";
            // 
            // RemovePdfWaterMark
            // 
            this.RemovePdfWaterMark.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemovePdfWaterMark.Location = new System.Drawing.Point(697, 561);
            this.RemovePdfWaterMark.Margin = new System.Windows.Forms.Padding(1);
            this.RemovePdfWaterMark.Name = "RemovePdfWaterMark";
            this.RemovePdfWaterMark.Size = new System.Drawing.Size(147, 37);
            this.RemovePdfWaterMark.TabIndex = 2;
            this.RemovePdfWaterMark.Tag = "anchor_right_bottom";
            this.RemovePdfWaterMark.Text = "开始处理";
            this.RemovePdfWaterMark.UseVisualStyleBackColor = true;
            this.RemovePdfWaterMark.Click += new System.EventHandler(this.RemovePdfWaterMark_Click);
            // 
            // SelectRemovePdfWaterMarkFiles
            // 
            this.SelectRemovePdfWaterMarkFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectRemovePdfWaterMarkFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectRemovePdfWaterMarkFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectRemovePdfWaterMarkFiles.Name = "SelectRemovePdfWaterMarkFiles";
            this.SelectRemovePdfWaterMarkFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectRemovePdfWaterMarkFiles.TabIndex = 1;
            this.SelectRemovePdfWaterMarkFiles.Tag = "anchor_left_bottom";
            this.SelectRemovePdfWaterMarkFiles.Text = "选择文件";
            this.SelectRemovePdfWaterMarkFiles.UseVisualStyleBackColor = true;
            this.SelectRemovePdfWaterMarkFiles.Click += new System.EventHandler(this.SelectRemovePdfWaterMarkFiles_Click);
            // 
            // RemovePdfWaterMarkFileList
            // 
            this.RemovePdfWaterMarkFileList.HideSelection = false;
            this.RemovePdfWaterMarkFileList.Location = new System.Drawing.Point(4, 4);
            this.RemovePdfWaterMarkFileList.Margin = new System.Windows.Forms.Padding(1);
            this.RemovePdfWaterMarkFileList.Name = "RemovePdfWaterMarkFileList";
            this.RemovePdfWaterMarkFileList.Size = new System.Drawing.Size(841, 420);
            this.RemovePdfWaterMarkFileList.TabIndex = 0;
            this.RemovePdfWaterMarkFileList.Tag = "anchor_full_fill";
            this.RemovePdfWaterMarkFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_22
            // 
            this.TabPage_22.Controls.Add(this.ConvertPdfComplianceFileList_MoveDown);
            this.TabPage_22.Controls.Add(this.ConvertPdfComplianceFileList_MoveUp);
            this.TabPage_22.Controls.Add(this.pdfversion_clear);
            this.TabPage_22.Controls.Add(this.pdfversion_remove);
            this.TabPage_22.Controls.Add(this.ConvertPdfComplianceVersion);
            this.TabPage_22.Controls.Add(this.label39);
            this.TabPage_22.Controls.Add(this.ConvertPdfComplianceStatusText);
            this.TabPage_22.Controls.Add(this.ConvertPdfCompliance);
            this.TabPage_22.Controls.Add(this.SelectConvertComplaincePdf);
            this.TabPage_22.Controls.Add(this.ConvertPdfComplianceFileList);
            this.TabPage_22.Location = new System.Drawing.Point(4, 76);
            this.TabPage_22.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_22.Name = "TabPage_22";
            this.TabPage_22.Size = new System.Drawing.Size(882, 659);
            this.TabPage_22.TabIndex = 21;
            this.TabPage_22.Text = "PDF版本转换";
            this.TabPage_22.UseVisualStyleBackColor = true;
            // 
            // ConvertPdfComplianceFileList_MoveDown
            // 
            this.ConvertPdfComplianceFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPdfComplianceFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.ConvertPdfComplianceFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPdfComplianceFileList_MoveDown.Name = "ConvertPdfComplianceFileList_MoveDown";
            this.ConvertPdfComplianceFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.ConvertPdfComplianceFileList_MoveDown.TabIndex = 7;
            this.ConvertPdfComplianceFileList_MoveDown.Tag = "anchor_right";
            this.ConvertPdfComplianceFileList_MoveDown.Text = "▽";
            this.ConvertPdfComplianceFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // ConvertPdfComplianceFileList_MoveUp
            // 
            this.ConvertPdfComplianceFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPdfComplianceFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.ConvertPdfComplianceFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPdfComplianceFileList_MoveUp.Name = "ConvertPdfComplianceFileList_MoveUp";
            this.ConvertPdfComplianceFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.ConvertPdfComplianceFileList_MoveUp.TabIndex = 6;
            this.ConvertPdfComplianceFileList_MoveUp.Tag = "anchor_right";
            this.ConvertPdfComplianceFileList_MoveUp.Text = "△";
            this.ConvertPdfComplianceFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // pdfversion_clear
            // 
            this.pdfversion_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.pdfversion_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdfversion_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdfversion_clear.Location = new System.Drawing.Point(847, 225);
            this.pdfversion_clear.Margin = new System.Windows.Forms.Padding(1);
            this.pdfversion_clear.Name = "pdfversion_clear";
            this.pdfversion_clear.Size = new System.Drawing.Size(23, 57);
            this.pdfversion_clear.TabIndex = 12;
            this.pdfversion_clear.Tag = "anchor_right";
            this.pdfversion_clear.UseVisualStyleBackColor = true;
            // 
            // pdfversion_remove
            // 
            this.pdfversion_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.pdfversion_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pdfversion_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pdfversion_remove.Location = new System.Drawing.Point(847, 145);
            this.pdfversion_remove.Margin = new System.Windows.Forms.Padding(1);
            this.pdfversion_remove.Name = "pdfversion_remove";
            this.pdfversion_remove.Size = new System.Drawing.Size(23, 57);
            this.pdfversion_remove.TabIndex = 11;
            this.pdfversion_remove.Tag = "anchor_right";
            this.pdfversion_remove.UseVisualStyleBackColor = true;
            // 
            // ConvertPdfComplianceVersion
            // 
            this.ConvertPdfComplianceVersion.FormattingEnabled = true;
            this.ConvertPdfComplianceVersion.Location = new System.Drawing.Point(61, 427);
            this.ConvertPdfComplianceVersion.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPdfComplianceVersion.Name = "ConvertPdfComplianceVersion";
            this.ConvertPdfComplianceVersion.Size = new System.Drawing.Size(147, 20);
            this.ConvertPdfComplianceVersion.TabIndex = 5;
            this.ConvertPdfComplianceVersion.Tag = "anchor_left_bottom";
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label39.Location = new System.Drawing.Point(4, 431);
            this.label39.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(53, 12);
            this.label39.TabIndex = 4;
            this.label39.Tag = "anchor_left_bottom";
            this.label39.Text = "转换版本";
            // 
            // ConvertPdfComplianceStatusText
            // 
            this.ConvertPdfComplianceStatusText.AutoSize = true;
            this.ConvertPdfComplianceStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertPdfComplianceStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPdfComplianceStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertPdfComplianceStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertPdfComplianceStatusText.Name = "ConvertPdfComplianceStatusText";
            this.ConvertPdfComplianceStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertPdfComplianceStatusText.TabIndex = 3;
            this.ConvertPdfComplianceStatusText.Tag = "anchor_left_bottom";
            this.ConvertPdfComplianceStatusText.Text = "正在处理X个文件";
            // 
            // ConvertPdfCompliance
            // 
            this.ConvertPdfCompliance.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertPdfCompliance.Location = new System.Drawing.Point(697, 561);
            this.ConvertPdfCompliance.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPdfCompliance.Name = "ConvertPdfCompliance";
            this.ConvertPdfCompliance.Size = new System.Drawing.Size(147, 37);
            this.ConvertPdfCompliance.TabIndex = 2;
            this.ConvertPdfCompliance.Tag = "anchor_right_bottom";
            this.ConvertPdfCompliance.Text = "开始处理";
            this.ConvertPdfCompliance.UseVisualStyleBackColor = true;
            this.ConvertPdfCompliance.Click += new System.EventHandler(this.ConvertPdfCompliance_Click);
            // 
            // SelectConvertComplaincePdf
            // 
            this.SelectConvertComplaincePdf.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectConvertComplaincePdf.Location = new System.Drawing.Point(5, 561);
            this.SelectConvertComplaincePdf.Margin = new System.Windows.Forms.Padding(1);
            this.SelectConvertComplaincePdf.Name = "SelectConvertComplaincePdf";
            this.SelectConvertComplaincePdf.Size = new System.Drawing.Size(147, 37);
            this.SelectConvertComplaincePdf.TabIndex = 1;
            this.SelectConvertComplaincePdf.Tag = "anchor_left_bottom";
            this.SelectConvertComplaincePdf.Text = "选择文件";
            this.SelectConvertComplaincePdf.UseVisualStyleBackColor = true;
            this.SelectConvertComplaincePdf.Click += new System.EventHandler(this.SelectConvertComplaincePdf_Click);
            // 
            // ConvertPdfComplianceFileList
            // 
            this.ConvertPdfComplianceFileList.HideSelection = false;
            this.ConvertPdfComplianceFileList.Location = new System.Drawing.Point(4, 4);
            this.ConvertPdfComplianceFileList.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertPdfComplianceFileList.Name = "ConvertPdfComplianceFileList";
            this.ConvertPdfComplianceFileList.Size = new System.Drawing.Size(841, 420);
            this.ConvertPdfComplianceFileList.TabIndex = 0;
            this.ConvertPdfComplianceFileList.Tag = "anchor_full_fill";
            this.ConvertPdfComplianceFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_15
            // 
            this.TabPage_15.Controls.Add(this.word2excel_save_source);
            this.TabPage_15.Controls.Add(this.Word2ExcelFileList_MoveDown);
            this.TabPage_15.Controls.Add(this.Word2ExcelFileList_MoveUp);
            this.TabPage_15.Controls.Add(this.word2excel_clear);
            this.TabPage_15.Controls.Add(this.word2excel_remove);
            this.TabPage_15.Controls.Add(this.ConverWord2ExcelStatusText);
            this.TabPage_15.Controls.Add(this.ConvertWord2Excel);
            this.TabPage_15.Controls.Add(this.SelectWord2ExcelFiles);
            this.TabPage_15.Controls.Add(this.Word2ExcelFileList);
            this.TabPage_15.Location = new System.Drawing.Point(4, 76);
            this.TabPage_15.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_15.Name = "TabPage_15";
            this.TabPage_15.Size = new System.Drawing.Size(882, 659);
            this.TabPage_15.TabIndex = 14;
            this.TabPage_15.Text = "WORD转EXCEL";
            this.TabPage_15.UseVisualStyleBackColor = true;
            // 
            // word2excel_save_source
            // 
            this.word2excel_save_source.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.word2excel_save_source.AutoSize = true;
            this.word2excel_save_source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2excel_save_source.Location = new System.Drawing.Point(5, 776);
            this.word2excel_save_source.Margin = new System.Windows.Forms.Padding(1);
            this.word2excel_save_source.Name = "word2excel_save_source";
            this.word2excel_save_source.Size = new System.Drawing.Size(96, 16);
            this.word2excel_save_source.TabIndex = 17;
            this.word2excel_save_source.Text = "保存到源目录";
            this.word2excel_save_source.UseVisualStyleBackColor = true;
            // 
            // Word2ExcelFileList_MoveDown
            // 
            this.Word2ExcelFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Word2ExcelFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.Word2ExcelFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.Word2ExcelFileList_MoveDown.Name = "Word2ExcelFileList_MoveDown";
            this.Word2ExcelFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.Word2ExcelFileList_MoveDown.TabIndex = 5;
            this.Word2ExcelFileList_MoveDown.Tag = "anchor_right";
            this.Word2ExcelFileList_MoveDown.Text = "▽";
            this.Word2ExcelFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // Word2ExcelFileList_MoveUp
            // 
            this.Word2ExcelFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Word2ExcelFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.Word2ExcelFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.Word2ExcelFileList_MoveUp.Name = "Word2ExcelFileList_MoveUp";
            this.Word2ExcelFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.Word2ExcelFileList_MoveUp.TabIndex = 4;
            this.Word2ExcelFileList_MoveUp.Tag = "anchor_right";
            this.Word2ExcelFileList_MoveUp.Text = "△";
            this.Word2ExcelFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // word2excel_clear
            // 
            this.word2excel_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.word2excel_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.word2excel_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2excel_clear.Location = new System.Drawing.Point(847, 225);
            this.word2excel_clear.Margin = new System.Windows.Forms.Padding(1);
            this.word2excel_clear.Name = "word2excel_clear";
            this.word2excel_clear.Size = new System.Drawing.Size(23, 57);
            this.word2excel_clear.TabIndex = 12;
            this.word2excel_clear.Tag = "anchor_right";
            this.word2excel_clear.UseVisualStyleBackColor = true;
            // 
            // word2excel_remove
            // 
            this.word2excel_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.word2excel_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.word2excel_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2excel_remove.Location = new System.Drawing.Point(847, 145);
            this.word2excel_remove.Margin = new System.Windows.Forms.Padding(1);
            this.word2excel_remove.Name = "word2excel_remove";
            this.word2excel_remove.Size = new System.Drawing.Size(23, 57);
            this.word2excel_remove.TabIndex = 11;
            this.word2excel_remove.Tag = "anchor_right";
            this.word2excel_remove.UseVisualStyleBackColor = true;
            // 
            // ConverWord2ExcelStatusText
            // 
            this.ConverWord2ExcelStatusText.AutoSize = true;
            this.ConverWord2ExcelStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConverWord2ExcelStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConverWord2ExcelStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConverWord2ExcelStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConverWord2ExcelStatusText.Name = "ConverWord2ExcelStatusText";
            this.ConverWord2ExcelStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConverWord2ExcelStatusText.TabIndex = 3;
            this.ConverWord2ExcelStatusText.Tag = "anchor_left_bottom";
            this.ConverWord2ExcelStatusText.Text = "正在处理X个文件";
            // 
            // ConvertWord2Excel
            // 
            this.ConvertWord2Excel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertWord2Excel.Location = new System.Drawing.Point(697, 561);
            this.ConvertWord2Excel.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertWord2Excel.Name = "ConvertWord2Excel";
            this.ConvertWord2Excel.Size = new System.Drawing.Size(147, 37);
            this.ConvertWord2Excel.TabIndex = 2;
            this.ConvertWord2Excel.Tag = "anchor_right_bottom";
            this.ConvertWord2Excel.Text = "开始处理";
            this.ConvertWord2Excel.UseVisualStyleBackColor = true;
            this.ConvertWord2Excel.Click += new System.EventHandler(this.ConvertWord2Excel_Click);
            // 
            // SelectWord2ExcelFiles
            // 
            this.SelectWord2ExcelFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectWord2ExcelFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectWord2ExcelFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectWord2ExcelFiles.Name = "SelectWord2ExcelFiles";
            this.SelectWord2ExcelFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectWord2ExcelFiles.TabIndex = 1;
            this.SelectWord2ExcelFiles.Tag = "anchor_left_bottom";
            this.SelectWord2ExcelFiles.Text = "选择文件";
            this.SelectWord2ExcelFiles.UseVisualStyleBackColor = true;
            this.SelectWord2ExcelFiles.Click += new System.EventHandler(this.SelectWord2ExcelFiles_Click);
            // 
            // Word2ExcelFileList
            // 
            this.Word2ExcelFileList.HideSelection = false;
            this.Word2ExcelFileList.Location = new System.Drawing.Point(4, 4);
            this.Word2ExcelFileList.Margin = new System.Windows.Forms.Padding(1);
            this.Word2ExcelFileList.Name = "Word2ExcelFileList";
            this.Word2ExcelFileList.Size = new System.Drawing.Size(841, 420);
            this.Word2ExcelFileList.TabIndex = 0;
            this.Word2ExcelFileList.Tag = "anchor_full_fill";
            this.Word2ExcelFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_16
            // 
            this.TabPage_16.Controls.Add(this.excel2word_save_source);
            this.TabPage_16.Controls.Add(this.Excel2WordFileList_MoveDown);
            this.TabPage_16.Controls.Add(this.Excel2WordFileList_MoveUp);
            this.TabPage_16.Controls.Add(this.excel2word_clear);
            this.TabPage_16.Controls.Add(this.excel2word_remove);
            this.TabPage_16.Controls.Add(this.label24);
            this.TabPage_16.Controls.Add(this.ConvertExcel2WordStatusText);
            this.TabPage_16.Controls.Add(this.ConvertExcel2Word);
            this.TabPage_16.Controls.Add(this.SelectExcel2WordFiles);
            this.TabPage_16.Controls.Add(this.Excel2WordFileList);
            this.TabPage_16.Location = new System.Drawing.Point(4, 76);
            this.TabPage_16.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_16.Name = "TabPage_16";
            this.TabPage_16.Size = new System.Drawing.Size(882, 659);
            this.TabPage_16.TabIndex = 15;
            this.TabPage_16.Text = "EXCEL转WORD";
            this.TabPage_16.UseVisualStyleBackColor = true;
            // 
            // excel2word_save_source
            // 
            this.excel2word_save_source.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.excel2word_save_source.AutoSize = true;
            this.excel2word_save_source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.excel2word_save_source.Location = new System.Drawing.Point(7, 736);
            this.excel2word_save_source.Margin = new System.Windows.Forms.Padding(1);
            this.excel2word_save_source.Name = "excel2word_save_source";
            this.excel2word_save_source.Size = new System.Drawing.Size(96, 16);
            this.excel2word_save_source.TabIndex = 17;
            this.excel2word_save_source.Text = "保存到源目录";
            this.excel2word_save_source.UseVisualStyleBackColor = true;
            // 
            // Excel2WordFileList_MoveDown
            // 
            this.Excel2WordFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Excel2WordFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.Excel2WordFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.Excel2WordFileList_MoveDown.Name = "Excel2WordFileList_MoveDown";
            this.Excel2WordFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.Excel2WordFileList_MoveDown.TabIndex = 6;
            this.Excel2WordFileList_MoveDown.Tag = "anchor_right";
            this.Excel2WordFileList_MoveDown.Text = "▽";
            this.Excel2WordFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // Excel2WordFileList_MoveUp
            // 
            this.Excel2WordFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Excel2WordFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.Excel2WordFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.Excel2WordFileList_MoveUp.Name = "Excel2WordFileList_MoveUp";
            this.Excel2WordFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.Excel2WordFileList_MoveUp.TabIndex = 5;
            this.Excel2WordFileList_MoveUp.Tag = "anchor_right";
            this.Excel2WordFileList_MoveUp.Text = "△";
            this.Excel2WordFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // excel2word_clear
            // 
            this.excel2word_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.excel2word_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.excel2word_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.excel2word_clear.Location = new System.Drawing.Point(847, 225);
            this.excel2word_clear.Margin = new System.Windows.Forms.Padding(1);
            this.excel2word_clear.Name = "excel2word_clear";
            this.excel2word_clear.Size = new System.Drawing.Size(23, 57);
            this.excel2word_clear.TabIndex = 12;
            this.excel2word_clear.Tag = "anchor_right";
            this.excel2word_clear.UseVisualStyleBackColor = true;
            // 
            // excel2word_remove
            // 
            this.excel2word_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.excel2word_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.excel2word_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.excel2word_remove.Location = new System.Drawing.Point(847, 145);
            this.excel2word_remove.Margin = new System.Windows.Forms.Padding(1);
            this.excel2word_remove.Name = "excel2word_remove";
            this.excel2word_remove.Size = new System.Drawing.Size(23, 57);
            this.excel2word_remove.TabIndex = 11;
            this.excel2word_remove.Tag = "anchor_right";
            this.excel2word_remove.UseVisualStyleBackColor = true;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.ForeColor = System.Drawing.Color.Tomato;
            this.label24.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label24.Location = new System.Drawing.Point(4, 431);
            this.label24.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(509, 12);
            this.label24.TabIndex = 4;
            this.label24.Tag = "anchor_left_bottom";
            this.label24.Text = "提示：Excel转Word默认设置为所有sheet全部在一个文档中且格式不能保证一致，需要自行调整";
            // 
            // ConvertExcel2WordStatusText
            // 
            this.ConvertExcel2WordStatusText.AutoSize = true;
            this.ConvertExcel2WordStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertExcel2WordStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertExcel2WordStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertExcel2WordStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertExcel2WordStatusText.Name = "ConvertExcel2WordStatusText";
            this.ConvertExcel2WordStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertExcel2WordStatusText.TabIndex = 3;
            this.ConvertExcel2WordStatusText.Tag = "anchor_left_bottom";
            this.ConvertExcel2WordStatusText.Text = "正在处理X个文件";
            // 
            // ConvertExcel2Word
            // 
            this.ConvertExcel2Word.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertExcel2Word.Location = new System.Drawing.Point(697, 561);
            this.ConvertExcel2Word.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertExcel2Word.Name = "ConvertExcel2Word";
            this.ConvertExcel2Word.Size = new System.Drawing.Size(147, 37);
            this.ConvertExcel2Word.TabIndex = 2;
            this.ConvertExcel2Word.Tag = "anchor_right_bottom";
            this.ConvertExcel2Word.Text = "开始处理";
            this.ConvertExcel2Word.UseVisualStyleBackColor = true;
            this.ConvertExcel2Word.Click += new System.EventHandler(this.ConvertExcel2Word_Click);
            // 
            // SelectExcel2WordFiles
            // 
            this.SelectExcel2WordFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectExcel2WordFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectExcel2WordFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectExcel2WordFiles.Name = "SelectExcel2WordFiles";
            this.SelectExcel2WordFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectExcel2WordFiles.TabIndex = 1;
            this.SelectExcel2WordFiles.Tag = "anchor_left_bottom";
            this.SelectExcel2WordFiles.Text = "选择文件";
            this.SelectExcel2WordFiles.UseVisualStyleBackColor = true;
            this.SelectExcel2WordFiles.Click += new System.EventHandler(this.SelectExcel2WordFiles_Click);
            // 
            // Excel2WordFileList
            // 
            this.Excel2WordFileList.HideSelection = false;
            this.Excel2WordFileList.Location = new System.Drawing.Point(4, 4);
            this.Excel2WordFileList.Margin = new System.Windows.Forms.Padding(1);
            this.Excel2WordFileList.Name = "Excel2WordFileList";
            this.Excel2WordFileList.Size = new System.Drawing.Size(841, 420);
            this.Excel2WordFileList.TabIndex = 0;
            this.Excel2WordFileList.Tag = "anchor_full_fill";
            this.Excel2WordFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_04
            // 
            this.TabPage_04.Controls.Add(this.word2image_save_source);
            this.TabPage_04.Controls.Add(this.DocumentsListView_MoveDown);
            this.TabPage_04.Controls.Add(this.DocumentsListView_MoveUp);
            this.TabPage_04.Controls.Add(this.document2image_clear);
            this.TabPage_04.Controls.Add(this.document2image_remove);
            this.TabPage_04.Controls.Add(this.label4);
            this.TabPage_04.Controls.Add(this.PageExportCheck);
            this.TabPage_04.Controls.Add(this.label3);
            this.TabPage_04.Controls.Add(this.DpiItemList);
            this.TabPage_04.Controls.Add(this.label2);
            this.TabPage_04.Controls.Add(this.SelectWord2ImageFileButton);
            this.TabPage_04.Controls.Add(this.ConvertWord2ImageStatus);
            this.TabPage_04.Controls.Add(this.SetGrayCheckBox);
            this.TabPage_04.Controls.Add(this.ConvertWord2ImageButton);
            this.TabPage_04.Controls.Add(this.ConvertImageList);
            this.TabPage_04.Controls.Add(this.DocumentsListView);
            this.TabPage_04.Location = new System.Drawing.Point(4, 76);
            this.TabPage_04.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_04.Name = "TabPage_04";
            this.TabPage_04.Size = new System.Drawing.Size(882, 659);
            this.TabPage_04.TabIndex = 3;
            this.TabPage_04.Text = "WORD/PDF/EXCEL转图片";
            this.TabPage_04.UseVisualStyleBackColor = true;
            // 
            // word2image_save_source
            // 
            this.word2image_save_source.AutoSize = true;
            this.word2image_save_source.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2image_save_source.Location = new System.Drawing.Point(4, 457);
            this.word2image_save_source.Margin = new System.Windows.Forms.Padding(1);
            this.word2image_save_source.Name = "word2image_save_source";
            this.word2image_save_source.Size = new System.Drawing.Size(96, 16);
            this.word2image_save_source.TabIndex = 17;
            this.word2image_save_source.Tag = "anchor_left_bottom";
            this.word2image_save_source.Text = "保存到源目录";
            this.word2image_save_source.UseVisualStyleBackColor = true;
            // 
            // DocumentsListView_MoveDown
            // 
            this.DocumentsListView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DocumentsListView_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.DocumentsListView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.DocumentsListView_MoveDown.Name = "DocumentsListView_MoveDown";
            this.DocumentsListView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.DocumentsListView_MoveDown.TabIndex = 12;
            this.DocumentsListView_MoveDown.Tag = "anchor_right";
            this.DocumentsListView_MoveDown.Text = "▽";
            this.DocumentsListView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // DocumentsListView_MoveUp
            // 
            this.DocumentsListView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DocumentsListView_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.DocumentsListView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.DocumentsListView_MoveUp.Name = "DocumentsListView_MoveUp";
            this.DocumentsListView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.DocumentsListView_MoveUp.TabIndex = 11;
            this.DocumentsListView_MoveUp.Tag = "anchor_right";
            this.DocumentsListView_MoveUp.Text = "△";
            this.DocumentsListView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // document2image_clear
            // 
            this.document2image_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.document2image_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.document2image_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.document2image_clear.Location = new System.Drawing.Point(847, 225);
            this.document2image_clear.Margin = new System.Windows.Forms.Padding(1);
            this.document2image_clear.Name = "document2image_clear";
            this.document2image_clear.Size = new System.Drawing.Size(23, 57);
            this.document2image_clear.TabIndex = 12;
            this.document2image_clear.Tag = "anchor_right";
            this.document2image_clear.UseVisualStyleBackColor = true;
            // 
            // document2image_remove
            // 
            this.document2image_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.document2image_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.document2image_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.document2image_remove.Location = new System.Drawing.Point(847, 145);
            this.document2image_remove.Margin = new System.Windows.Forms.Padding(1);
            this.document2image_remove.Name = "document2image_remove";
            this.document2image_remove.Size = new System.Drawing.Size(23, 57);
            this.document2image_remove.TabIndex = 11;
            this.document2image_remove.Tag = "anchor_right";
            this.document2image_remove.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Tomato;
            this.label4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label4.Location = new System.Drawing.Point(3, 481);
            this.label4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(425, 12);
            this.label4.TabIndex = 10;
            this.label4.Tag = "anchor_left_bottom";
            this.label4.Text = "提示：8位BMP转换只支持word文档，一般情况下，此选项用于选票模板位置比对";
            // 
            // PageExportCheck
            // 
            this.PageExportCheck.AutoSize = true;
            this.PageExportCheck.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PageExportCheck.Location = new System.Drawing.Point(116, 431);
            this.PageExportCheck.Margin = new System.Windows.Forms.Padding(1);
            this.PageExportCheck.Name = "PageExportCheck";
            this.PageExportCheck.Size = new System.Drawing.Size(72, 16);
            this.PageExportCheck.TabIndex = 9;
            this.PageExportCheck.Tag = "anchor_left_bottom";
            this.PageExportCheck.Text = "逐页导出";
            this.PageExportCheck.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label3.Location = new System.Drawing.Point(191, 431);
            this.label3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 8;
            this.label3.Tag = "anchor_left_bottom";
            this.label3.Text = "图像输出分辨率";
            // 
            // DpiItemList
            // 
            this.DpiItemList.FormattingEnabled = true;
            this.DpiItemList.Location = new System.Drawing.Point(284, 427);
            this.DpiItemList.Margin = new System.Windows.Forms.Padding(1);
            this.DpiItemList.Name = "DpiItemList";
            this.DpiItemList.Size = new System.Drawing.Size(83, 20);
            this.DpiItemList.TabIndex = 7;
            this.DpiItemList.Tag = "anchor_left_bottom";
            this.DpiItemList.SelectedIndexChanged += new System.EventHandler(this.DpiItemList_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label2.Location = new System.Drawing.Point(376, 431);
            this.label2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 6;
            this.label2.Tag = "anchor_left_bottom";
            this.label2.Text = "转换格式";
            // 
            // SelectWord2ImageFileButton
            // 
            this.SelectWord2ImageFileButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectWord2ImageFileButton.Location = new System.Drawing.Point(5, 561);
            this.SelectWord2ImageFileButton.Margin = new System.Windows.Forms.Padding(1);
            this.SelectWord2ImageFileButton.Name = "SelectWord2ImageFileButton";
            this.SelectWord2ImageFileButton.Size = new System.Drawing.Size(147, 37);
            this.SelectWord2ImageFileButton.TabIndex = 5;
            this.SelectWord2ImageFileButton.Tag = "anchor_left_bottom";
            this.SelectWord2ImageFileButton.Text = "选择文件";
            this.SelectWord2ImageFileButton.UseVisualStyleBackColor = true;
            this.SelectWord2ImageFileButton.Click += new System.EventHandler(this.SelectWord2ImageFileButton_Click);
            // 
            // ConvertWord2ImageStatus
            // 
            this.ConvertWord2ImageStatus.AutoSize = true;
            this.ConvertWord2ImageStatus.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertWord2ImageStatus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertWord2ImageStatus.Location = new System.Drawing.Point(156, 575);
            this.ConvertWord2ImageStatus.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertWord2ImageStatus.Name = "ConvertWord2ImageStatus";
            this.ConvertWord2ImageStatus.Size = new System.Drawing.Size(95, 12);
            this.ConvertWord2ImageStatus.TabIndex = 4;
            this.ConvertWord2ImageStatus.Tag = "anchor_left_bottom";
            this.ConvertWord2ImageStatus.Text = "正在处理X个文件";
            // 
            // SetGrayCheckBox
            // 
            this.SetGrayCheckBox.AutoSize = true;
            this.SetGrayCheckBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SetGrayCheckBox.Location = new System.Drawing.Point(4, 431);
            this.SetGrayCheckBox.Margin = new System.Windows.Forms.Padding(1);
            this.SetGrayCheckBox.Name = "SetGrayCheckBox";
            this.SetGrayCheckBox.Size = new System.Drawing.Size(96, 16);
            this.SetGrayCheckBox.TabIndex = 3;
            this.SetGrayCheckBox.Tag = "anchor_left_bottom";
            this.SetGrayCheckBox.Text = "转换为8位BMP";
            this.SetGrayCheckBox.UseVisualStyleBackColor = true;
            // 
            // ConvertWord2ImageButton
            // 
            this.ConvertWord2ImageButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertWord2ImageButton.Location = new System.Drawing.Point(697, 561);
            this.ConvertWord2ImageButton.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertWord2ImageButton.Name = "ConvertWord2ImageButton";
            this.ConvertWord2ImageButton.Size = new System.Drawing.Size(147, 37);
            this.ConvertWord2ImageButton.TabIndex = 2;
            this.ConvertWord2ImageButton.Tag = "anchor_right_bottom";
            this.ConvertWord2ImageButton.Text = "开始处理";
            this.ConvertWord2ImageButton.UseVisualStyleBackColor = true;
            this.ConvertWord2ImageButton.Click += new System.EventHandler(this.ConvertWord2ImageButton_Click);
            // 
            // ConvertImageList
            // 
            this.ConvertImageList.FormattingEnabled = true;
            this.ConvertImageList.Location = new System.Drawing.Point(433, 427);
            this.ConvertImageList.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertImageList.Name = "ConvertImageList";
            this.ConvertImageList.Size = new System.Drawing.Size(83, 20);
            this.ConvertImageList.TabIndex = 1;
            this.ConvertImageList.Tag = "anchor_left_bottom";
            this.ConvertImageList.SelectedIndexChanged += new System.EventHandler(this.ConvertImageList_SelectedIndexChanged);
            // 
            // DocumentsListView
            // 
            this.DocumentsListView.HideSelection = false;
            this.DocumentsListView.Location = new System.Drawing.Point(4, 4);
            this.DocumentsListView.Margin = new System.Windows.Forms.Padding(1);
            this.DocumentsListView.Name = "DocumentsListView";
            this.DocumentsListView.Size = new System.Drawing.Size(841, 420);
            this.DocumentsListView.TabIndex = 0;
            this.DocumentsListView.Tag = "anchor_full_fill";
            this.DocumentsListView.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_27
            // 
            this.TabPage_27.Controls.Add(this.label65);
            this.TabPage_27.Controls.Add(this.CoverImageDpi);
            this.TabPage_27.Controls.Add(this.label64);
            this.TabPage_27.Controls.Add(this.SetWord2EpubCoverImage);
            this.TabPage_27.Controls.Add(this.Word2EpubConverImage);
            this.TabPage_27.Controls.Add(this.label63);
            this.TabPage_27.Controls.Add(this.Word2EpubAuthorName);
            this.TabPage_27.Controls.Add(this.label61);
            this.TabPage_27.Controls.Add(this.ConvertWord2EpubStatusText);
            this.TabPage_27.Controls.Add(this.word2epub_clear);
            this.TabPage_27.Controls.Add(this.word2epub_remove);
            this.TabPage_27.Controls.Add(this.Word2EPUBFileList_MoveDown);
            this.TabPage_27.Controls.Add(this.Word2EPUBFileList_MoveUp);
            this.TabPage_27.Controls.Add(this.ConvertWord2Epub);
            this.TabPage_27.Controls.Add(this.SelectWord2EpubFiles);
            this.TabPage_27.Controls.Add(this.Word2EPUBFileList);
            this.TabPage_27.Location = new System.Drawing.Point(4, 76);
            this.TabPage_27.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_27.Name = "TabPage_27";
            this.TabPage_27.Size = new System.Drawing.Size(882, 659);
            this.TabPage_27.TabIndex = 26;
            this.TabPage_27.Text = "WORD转EPUB";
            this.TabPage_27.UseVisualStyleBackColor = true;
            // 
            // label65
            // 
            this.label65.AutoSize = true;
            this.label65.ForeColor = System.Drawing.Color.Red;
            this.label65.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label65.Location = new System.Drawing.Point(4, 457);
            this.label65.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label65.Name = "label65";
            this.label65.Size = new System.Drawing.Size(239, 36);
            this.label65.TabIndex = 24;
            this.label65.Tag = "anchor_left_bottom";
            this.label65.Text = "1、不选择封面图片，将会用第一页作为封面\r\n\r\n2、分辨率不填默认为100";
            // 
            // CoverImageDpi
            // 
            this.CoverImageDpi.Location = new System.Drawing.Point(613, 427);
            this.CoverImageDpi.Margin = new System.Windows.Forms.Padding(1);
            this.CoverImageDpi.Name = "CoverImageDpi";
            this.CoverImageDpi.Size = new System.Drawing.Size(101, 21);
            this.CoverImageDpi.TabIndex = 23;
            this.CoverImageDpi.Tag = "anchor_left_bottom";
            // 
            // label64
            // 
            this.label64.AutoSize = true;
            this.label64.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label64.Location = new System.Drawing.Point(569, 431);
            this.label64.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label64.Name = "label64";
            this.label64.Size = new System.Drawing.Size(41, 12);
            this.label64.TabIndex = 22;
            this.label64.Tag = "anchor_left_bottom";
            this.label64.Text = "分辨率";
            // 
            // SetWord2EpubCoverImage
            // 
            this.SetWord2EpubCoverImage.AutoSize = true;
            this.SetWord2EpubCoverImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SetWord2EpubCoverImage.Location = new System.Drawing.Point(727, 427);
            this.SetWord2EpubCoverImage.Margin = new System.Windows.Forms.Padding(1);
            this.SetWord2EpubCoverImage.Name = "SetWord2EpubCoverImage";
            this.SetWord2EpubCoverImage.Size = new System.Drawing.Size(117, 28);
            this.SetWord2EpubCoverImage.TabIndex = 21;
            this.SetWord2EpubCoverImage.Tag = "anchor_left_bottom";
            this.SetWord2EpubCoverImage.Text = "选择图片...";
            this.SetWord2EpubCoverImage.UseVisualStyleBackColor = true;
            this.SetWord2EpubCoverImage.Click += new System.EventHandler(this.SetWord2EpubCoverImage_Click);
            // 
            // Word2EpubConverImage
            // 
            this.Word2EpubConverImage.Enabled = false;
            this.Word2EpubConverImage.Location = new System.Drawing.Point(349, 427);
            this.Word2EpubConverImage.Margin = new System.Windows.Forms.Padding(1);
            this.Word2EpubConverImage.Name = "Word2EpubConverImage";
            this.Word2EpubConverImage.Size = new System.Drawing.Size(205, 21);
            this.Word2EpubConverImage.TabIndex = 20;
            this.Word2EpubConverImage.Tag = "anchor_left_bottom";
            // 
            // label63
            // 
            this.label63.AutoSize = true;
            this.label63.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label63.Location = new System.Drawing.Point(268, 431);
            this.label63.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label63.Name = "label63";
            this.label63.Size = new System.Drawing.Size(77, 12);
            this.label63.TabIndex = 19;
            this.label63.Tag = "anchor_left_bottom";
            this.label63.Text = "封面图片路径";
            // 
            // Word2EpubAuthorName
            // 
            this.Word2EpubAuthorName.Location = new System.Drawing.Point(85, 427);
            this.Word2EpubAuthorName.Margin = new System.Windows.Forms.Padding(1);
            this.Word2EpubAuthorName.MaxLength = 50;
            this.Word2EpubAuthorName.Name = "Word2EpubAuthorName";
            this.Word2EpubAuthorName.Size = new System.Drawing.Size(167, 21);
            this.Word2EpubAuthorName.TabIndex = 18;
            this.Word2EpubAuthorName.Tag = "anchor_left_bottom";
            // 
            // label61
            // 
            this.label61.AutoSize = true;
            this.label61.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label61.Location = new System.Drawing.Point(4, 431);
            this.label61.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label61.Name = "label61";
            this.label61.Size = new System.Drawing.Size(77, 12);
            this.label61.TabIndex = 17;
            this.label61.Tag = "anchor_left_bottom";
            this.label61.Text = "设置作者名称";
            // 
            // ConvertWord2EpubStatusText
            // 
            this.ConvertWord2EpubStatusText.AutoSize = true;
            this.ConvertWord2EpubStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertWord2EpubStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertWord2EpubStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertWord2EpubStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertWord2EpubStatusText.Name = "ConvertWord2EpubStatusText";
            this.ConvertWord2EpubStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertWord2EpubStatusText.TabIndex = 16;
            this.ConvertWord2EpubStatusText.Tag = "anchor_left_bottom";
            this.ConvertWord2EpubStatusText.Text = "正在处理X个文件";
            // 
            // word2epub_clear
            // 
            this.word2epub_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.word2epub_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.word2epub_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2epub_clear.Location = new System.Drawing.Point(847, 225);
            this.word2epub_clear.Margin = new System.Windows.Forms.Padding(1);
            this.word2epub_clear.Name = "word2epub_clear";
            this.word2epub_clear.Size = new System.Drawing.Size(23, 57);
            this.word2epub_clear.TabIndex = 15;
            this.word2epub_clear.Tag = "anchor_right";
            this.word2epub_clear.UseVisualStyleBackColor = true;
            // 
            // word2epub_remove
            // 
            this.word2epub_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.word2epub_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.word2epub_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.word2epub_remove.Location = new System.Drawing.Point(847, 145);
            this.word2epub_remove.Margin = new System.Windows.Forms.Padding(1);
            this.word2epub_remove.Name = "word2epub_remove";
            this.word2epub_remove.Size = new System.Drawing.Size(23, 57);
            this.word2epub_remove.TabIndex = 14;
            this.word2epub_remove.Tag = "anchor_right";
            this.word2epub_remove.UseVisualStyleBackColor = true;
            // 
            // Word2EPUBFileList_MoveDown
            // 
            this.Word2EPUBFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Word2EPUBFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.Word2EPUBFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.Word2EPUBFileList_MoveDown.Name = "Word2EPUBFileList_MoveDown";
            this.Word2EPUBFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.Word2EPUBFileList_MoveDown.TabIndex = 13;
            this.Word2EPUBFileList_MoveDown.Tag = "anchor_right";
            this.Word2EPUBFileList_MoveDown.Text = "▽";
            this.Word2EPUBFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // Word2EPUBFileList_MoveUp
            // 
            this.Word2EPUBFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Word2EPUBFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.Word2EPUBFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.Word2EPUBFileList_MoveUp.Name = "Word2EPUBFileList_MoveUp";
            this.Word2EPUBFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.Word2EPUBFileList_MoveUp.TabIndex = 5;
            this.Word2EPUBFileList_MoveUp.Tag = "anchor_right";
            this.Word2EPUBFileList_MoveUp.Text = "△";
            this.Word2EPUBFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // ConvertWord2Epub
            // 
            this.ConvertWord2Epub.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertWord2Epub.Location = new System.Drawing.Point(697, 561);
            this.ConvertWord2Epub.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertWord2Epub.Name = "ConvertWord2Epub";
            this.ConvertWord2Epub.Size = new System.Drawing.Size(147, 37);
            this.ConvertWord2Epub.TabIndex = 2;
            this.ConvertWord2Epub.Tag = "anchor_right_bottom";
            this.ConvertWord2Epub.Text = "开始处理";
            this.ConvertWord2Epub.UseVisualStyleBackColor = true;
            this.ConvertWord2Epub.Click += new System.EventHandler(this.ConvertHTML2PDF_Click);
            // 
            // SelectWord2EpubFiles
            // 
            this.SelectWord2EpubFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectWord2EpubFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectWord2EpubFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectWord2EpubFiles.Name = "SelectWord2EpubFiles";
            this.SelectWord2EpubFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectWord2EpubFiles.TabIndex = 1;
            this.SelectWord2EpubFiles.Tag = "anchor_left_bottom";
            this.SelectWord2EpubFiles.Text = "选择文件";
            this.SelectWord2EpubFiles.UseVisualStyleBackColor = true;
            this.SelectWord2EpubFiles.Click += new System.EventHandler(this.SelectHtml2PdfFiles_Click);
            // 
            // Word2EPUBFileList
            // 
            this.Word2EPUBFileList.HideSelection = false;
            this.Word2EPUBFileList.Location = new System.Drawing.Point(4, 4);
            this.Word2EPUBFileList.Margin = new System.Windows.Forms.Padding(1);
            this.Word2EPUBFileList.Name = "Word2EPUBFileList";
            this.Word2EPUBFileList.Size = new System.Drawing.Size(841, 420);
            this.Word2EPUBFileList.TabIndex = 0;
            this.Word2EPUBFileList.Tag = "anchor_full_fill";
            this.Word2EPUBFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_28
            // 
            this.TabPage_28.Controls.Add(this.label60);
            this.TabPage_28.Controls.Add(this.EPUB2WORDFileList_MoveDown);
            this.TabPage_28.Controls.Add(this.EPUB2WORDFileList_MoveUp);
            this.TabPage_28.Controls.Add(this.epub2word_clear);
            this.TabPage_28.Controls.Add(this.epub2word_remove);
            this.TabPage_28.Controls.Add(this.ConvertEpub2WordStatusText);
            this.TabPage_28.Controls.Add(this.ConvertEpub2Word);
            this.TabPage_28.Controls.Add(this.SelectEpub2WordFiles);
            this.TabPage_28.Controls.Add(this.EPUB2WORDFileList);
            this.TabPage_28.Location = new System.Drawing.Point(4, 76);
            this.TabPage_28.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_28.Name = "TabPage_28";
            this.TabPage_28.Size = new System.Drawing.Size(882, 659);
            this.TabPage_28.TabIndex = 27;
            this.TabPage_28.Text = "EPUB转WORD";
            this.TabPage_28.UseVisualStyleBackColor = true;
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.ForeColor = System.Drawing.Color.Tomato;
            this.label60.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label60.Location = new System.Drawing.Point(4, 431);
            this.label60.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(329, 12);
            this.label60.TabIndex = 30;
            this.label60.Tag = "anchor_left_bottom";
            this.label60.Text = "提示：EPUB转WORD需确保EPUB格式正确，功能不稳定，请慎用";
            // 
            // EPUB2WORDFileList_MoveDown
            // 
            this.EPUB2WORDFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.EPUB2WORDFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.EPUB2WORDFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.EPUB2WORDFileList_MoveDown.Name = "EPUB2WORDFileList_MoveDown";
            this.EPUB2WORDFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.EPUB2WORDFileList_MoveDown.TabIndex = 18;
            this.EPUB2WORDFileList_MoveDown.Tag = "anchor_right";
            this.EPUB2WORDFileList_MoveDown.Text = "▽";
            this.EPUB2WORDFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // EPUB2WORDFileList_MoveUp
            // 
            this.EPUB2WORDFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.EPUB2WORDFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.EPUB2WORDFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.EPUB2WORDFileList_MoveUp.Name = "EPUB2WORDFileList_MoveUp";
            this.EPUB2WORDFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.EPUB2WORDFileList_MoveUp.TabIndex = 17;
            this.EPUB2WORDFileList_MoveUp.Tag = "anchor_right";
            this.EPUB2WORDFileList_MoveUp.Text = "△";
            this.EPUB2WORDFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // epub2word_clear
            // 
            this.epub2word_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.epub2word_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.epub2word_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.epub2word_clear.Location = new System.Drawing.Point(847, 225);
            this.epub2word_clear.Margin = new System.Windows.Forms.Padding(1);
            this.epub2word_clear.Name = "epub2word_clear";
            this.epub2word_clear.Size = new System.Drawing.Size(23, 57);
            this.epub2word_clear.TabIndex = 20;
            this.epub2word_clear.Tag = "anchor_right";
            this.epub2word_clear.UseVisualStyleBackColor = true;
            // 
            // epub2word_remove
            // 
            this.epub2word_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.epub2word_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.epub2word_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.epub2word_remove.Location = new System.Drawing.Point(847, 145);
            this.epub2word_remove.Margin = new System.Windows.Forms.Padding(1);
            this.epub2word_remove.Name = "epub2word_remove";
            this.epub2word_remove.Size = new System.Drawing.Size(23, 57);
            this.epub2word_remove.TabIndex = 19;
            this.epub2word_remove.Tag = "anchor_right";
            this.epub2word_remove.UseVisualStyleBackColor = true;
            // 
            // ConvertEpub2WordStatusText
            // 
            this.ConvertEpub2WordStatusText.AutoSize = true;
            this.ConvertEpub2WordStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertEpub2WordStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertEpub2WordStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertEpub2WordStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertEpub2WordStatusText.Name = "ConvertEpub2WordStatusText";
            this.ConvertEpub2WordStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertEpub2WordStatusText.TabIndex = 16;
            this.ConvertEpub2WordStatusText.Tag = "anchor_left_bottom";
            this.ConvertEpub2WordStatusText.Text = "正在处理X个文件";
            // 
            // ConvertEpub2Word
            // 
            this.ConvertEpub2Word.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertEpub2Word.Location = new System.Drawing.Point(697, 561);
            this.ConvertEpub2Word.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertEpub2Word.Name = "ConvertEpub2Word";
            this.ConvertEpub2Word.Size = new System.Drawing.Size(147, 37);
            this.ConvertEpub2Word.TabIndex = 15;
            this.ConvertEpub2Word.Tag = "anchor_right_bottom";
            this.ConvertEpub2Word.Text = "开始处理";
            this.ConvertEpub2Word.UseVisualStyleBackColor = true;
            this.ConvertEpub2Word.Click += new System.EventHandler(this.ConvertEpub2Word_Click);
            // 
            // SelectEpub2WordFiles
            // 
            this.SelectEpub2WordFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectEpub2WordFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectEpub2WordFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectEpub2WordFiles.Name = "SelectEpub2WordFiles";
            this.SelectEpub2WordFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectEpub2WordFiles.TabIndex = 14;
            this.SelectEpub2WordFiles.Tag = "anchor_left_bottom";
            this.SelectEpub2WordFiles.Text = "选择文件";
            this.SelectEpub2WordFiles.UseVisualStyleBackColor = true;
            this.SelectEpub2WordFiles.Click += new System.EventHandler(this.SelectEpub2WordFiles_Click);
            // 
            // EPUB2WORDFileList
            // 
            this.EPUB2WORDFileList.HideSelection = false;
            this.EPUB2WORDFileList.Location = new System.Drawing.Point(4, 4);
            this.EPUB2WORDFileList.Margin = new System.Windows.Forms.Padding(1);
            this.EPUB2WORDFileList.Name = "EPUB2WORDFileList";
            this.EPUB2WORDFileList.Size = new System.Drawing.Size(841, 420);
            this.EPUB2WORDFileList.TabIndex = 13;
            this.EPUB2WORDFileList.Tag = "anchor_full_fill";
            this.EPUB2WORDFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_29
            // 
            this.TabPage_29.Controls.Add(this.label59);
            this.TabPage_29.Controls.Add(this.EPUB2PDFFileList_MoveDown);
            this.TabPage_29.Controls.Add(this.EPUB2PDFFileList_MoveUp);
            this.TabPage_29.Controls.Add(this.epub2pdf_clear);
            this.TabPage_29.Controls.Add(this.epub2pdf_remove);
            this.TabPage_29.Controls.Add(this.ConvertEPUB2PDFStatusText);
            this.TabPage_29.Controls.Add(this.ConvertEPUB2PDF);
            this.TabPage_29.Controls.Add(this.SelectEPUB2PDFFiles);
            this.TabPage_29.Controls.Add(this.EPUB2PDFFileList);
            this.TabPage_29.Location = new System.Drawing.Point(4, 76);
            this.TabPage_29.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_29.Name = "TabPage_29";
            this.TabPage_29.Size = new System.Drawing.Size(882, 659);
            this.TabPage_29.TabIndex = 28;
            this.TabPage_29.Text = "EPUB转PDF";
            this.TabPage_29.UseVisualStyleBackColor = true;
            // 
            // label59
            // 
            this.label59.AutoSize = true;
            this.label59.ForeColor = System.Drawing.Color.Tomato;
            this.label59.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label59.Location = new System.Drawing.Point(4, 431);
            this.label59.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label59.Name = "label59";
            this.label59.Size = new System.Drawing.Size(323, 12);
            this.label59.TabIndex = 29;
            this.label59.Tag = "anchor_left_bottom";
            this.label59.Text = "提示：EPUB转PDF需确保EPUB格式正确，功能不稳定，请慎用";
            // 
            // EPUB2PDFFileList_MoveDown
            // 
            this.EPUB2PDFFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.EPUB2PDFFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.EPUB2PDFFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.EPUB2PDFFileList_MoveDown.Name = "EPUB2PDFFileList_MoveDown";
            this.EPUB2PDFFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.EPUB2PDFFileList_MoveDown.TabIndex = 26;
            this.EPUB2PDFFileList_MoveDown.Tag = "anchor_right";
            this.EPUB2PDFFileList_MoveDown.Text = "▽";
            this.EPUB2PDFFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // EPUB2PDFFileList_MoveUp
            // 
            this.EPUB2PDFFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.EPUB2PDFFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.EPUB2PDFFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.EPUB2PDFFileList_MoveUp.Name = "EPUB2PDFFileList_MoveUp";
            this.EPUB2PDFFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.EPUB2PDFFileList_MoveUp.TabIndex = 25;
            this.EPUB2PDFFileList_MoveUp.Tag = "anchor_right";
            this.EPUB2PDFFileList_MoveUp.Text = "△";
            this.EPUB2PDFFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // epub2pdf_clear
            // 
            this.epub2pdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.epub2pdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.epub2pdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.epub2pdf_clear.Location = new System.Drawing.Point(847, 225);
            this.epub2pdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.epub2pdf_clear.Name = "epub2pdf_clear";
            this.epub2pdf_clear.Size = new System.Drawing.Size(23, 57);
            this.epub2pdf_clear.TabIndex = 28;
            this.epub2pdf_clear.Tag = "anchor_right";
            this.epub2pdf_clear.UseVisualStyleBackColor = true;
            // 
            // epub2pdf_remove
            // 
            this.epub2pdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.epub2pdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.epub2pdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.epub2pdf_remove.Location = new System.Drawing.Point(847, 145);
            this.epub2pdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.epub2pdf_remove.Name = "epub2pdf_remove";
            this.epub2pdf_remove.Size = new System.Drawing.Size(23, 57);
            this.epub2pdf_remove.TabIndex = 27;
            this.epub2pdf_remove.Tag = "anchor_right";
            this.epub2pdf_remove.UseVisualStyleBackColor = true;
            // 
            // ConvertEPUB2PDFStatusText
            // 
            this.ConvertEPUB2PDFStatusText.AutoSize = true;
            this.ConvertEPUB2PDFStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertEPUB2PDFStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertEPUB2PDFStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertEPUB2PDFStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertEPUB2PDFStatusText.Name = "ConvertEPUB2PDFStatusText";
            this.ConvertEPUB2PDFStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertEPUB2PDFStatusText.TabIndex = 24;
            this.ConvertEPUB2PDFStatusText.Tag = "anchor_left_bottom";
            this.ConvertEPUB2PDFStatusText.Text = "正在处理X个文件";
            // 
            // ConvertEPUB2PDF
            // 
            this.ConvertEPUB2PDF.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertEPUB2PDF.Location = new System.Drawing.Point(697, 561);
            this.ConvertEPUB2PDF.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertEPUB2PDF.Name = "ConvertEPUB2PDF";
            this.ConvertEPUB2PDF.Size = new System.Drawing.Size(147, 37);
            this.ConvertEPUB2PDF.TabIndex = 23;
            this.ConvertEPUB2PDF.Tag = "anchor_right_bottom";
            this.ConvertEPUB2PDF.Text = "开始处理";
            this.ConvertEPUB2PDF.UseVisualStyleBackColor = true;
            this.ConvertEPUB2PDF.Click += new System.EventHandler(this.ConvertEPUB2PDF_Click);
            // 
            // SelectEPUB2PDFFiles
            // 
            this.SelectEPUB2PDFFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectEPUB2PDFFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectEPUB2PDFFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectEPUB2PDFFiles.Name = "SelectEPUB2PDFFiles";
            this.SelectEPUB2PDFFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectEPUB2PDFFiles.TabIndex = 22;
            this.SelectEPUB2PDFFiles.Tag = "anchor_left_bottom";
            this.SelectEPUB2PDFFiles.Text = "选择文件";
            this.SelectEPUB2PDFFiles.UseVisualStyleBackColor = true;
            this.SelectEPUB2PDFFiles.Click += new System.EventHandler(this.SelectEPUB2PDFFiles_Click);
            // 
            // EPUB2PDFFileList
            // 
            this.EPUB2PDFFileList.HideSelection = false;
            this.EPUB2PDFFileList.Location = new System.Drawing.Point(4, 4);
            this.EPUB2PDFFileList.Margin = new System.Windows.Forms.Padding(1);
            this.EPUB2PDFFileList.Name = "EPUB2PDFFileList";
            this.EPUB2PDFFileList.Size = new System.Drawing.Size(841, 420);
            this.EPUB2PDFFileList.TabIndex = 21;
            this.EPUB2PDFFileList.Tag = "anchor_full_fill";
            this.EPUB2PDFFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_23
            // 
            this.TabPage_23.Controls.Add(this.ConvertImage2PDFFileList_MoveDown);
            this.TabPage_23.Controls.Add(this.ConvertImage2PDFFileList_MoveUp);
            this.TabPage_23.Controls.Add(this.image2pdf_clear);
            this.TabPage_23.Controls.Add(this.image2pdf_remove);
            this.TabPage_23.Controls.Add(this.COnvertImage2PDFStatusText);
            this.TabPage_23.Controls.Add(this.groupBox19);
            this.TabPage_23.Controls.Add(this.ConvertImage2PDF);
            this.TabPage_23.Controls.Add(this.SelectConvertImage2PDF);
            this.TabPage_23.Controls.Add(this.ConvertImage2PDFFileList);
            this.TabPage_23.Location = new System.Drawing.Point(4, 76);
            this.TabPage_23.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_23.Name = "TabPage_23";
            this.TabPage_23.Size = new System.Drawing.Size(882, 659);
            this.TabPage_23.TabIndex = 22;
            this.TabPage_23.Text = "图片转PDF";
            this.TabPage_23.UseVisualStyleBackColor = true;
            // 
            // ConvertImage2PDFFileList_MoveDown
            // 
            this.ConvertImage2PDFFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertImage2PDFFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.ConvertImage2PDFFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertImage2PDFFileList_MoveDown.Name = "ConvertImage2PDFFileList_MoveDown";
            this.ConvertImage2PDFFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.ConvertImage2PDFFileList_MoveDown.TabIndex = 6;
            this.ConvertImage2PDFFileList_MoveDown.Tag = "anchor_right";
            this.ConvertImage2PDFFileList_MoveDown.Text = "▽";
            this.ConvertImage2PDFFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // ConvertImage2PDFFileList_MoveUp
            // 
            this.ConvertImage2PDFFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertImage2PDFFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.ConvertImage2PDFFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertImage2PDFFileList_MoveUp.Name = "ConvertImage2PDFFileList_MoveUp";
            this.ConvertImage2PDFFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.ConvertImage2PDFFileList_MoveUp.TabIndex = 5;
            this.ConvertImage2PDFFileList_MoveUp.Tag = "anchor_right";
            this.ConvertImage2PDFFileList_MoveUp.Text = "△";
            this.ConvertImage2PDFFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // image2pdf_clear
            // 
            this.image2pdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.image2pdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.image2pdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.image2pdf_clear.Location = new System.Drawing.Point(847, 225);
            this.image2pdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.image2pdf_clear.Name = "image2pdf_clear";
            this.image2pdf_clear.Size = new System.Drawing.Size(23, 57);
            this.image2pdf_clear.TabIndex = 12;
            this.image2pdf_clear.Tag = "anchor_right";
            this.image2pdf_clear.UseVisualStyleBackColor = true;
            // 
            // image2pdf_remove
            // 
            this.image2pdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.image2pdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.image2pdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.image2pdf_remove.Location = new System.Drawing.Point(847, 145);
            this.image2pdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.image2pdf_remove.Name = "image2pdf_remove";
            this.image2pdf_remove.Size = new System.Drawing.Size(23, 57);
            this.image2pdf_remove.TabIndex = 11;
            this.image2pdf_remove.Tag = "anchor_right";
            this.image2pdf_remove.UseVisualStyleBackColor = true;
            // 
            // COnvertImage2PDFStatusText
            // 
            this.COnvertImage2PDFStatusText.AutoSize = true;
            this.COnvertImage2PDFStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.COnvertImage2PDFStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.COnvertImage2PDFStatusText.Location = new System.Drawing.Point(156, 575);
            this.COnvertImage2PDFStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.COnvertImage2PDFStatusText.Name = "COnvertImage2PDFStatusText";
            this.COnvertImage2PDFStatusText.Size = new System.Drawing.Size(95, 12);
            this.COnvertImage2PDFStatusText.TabIndex = 4;
            this.COnvertImage2PDFStatusText.Tag = "anchor_left_bottom";
            this.COnvertImage2PDFStatusText.Text = "正在处理X个文件";
            // 
            // groupBox19
            // 
            this.groupBox19.Controls.Add(this.Image2PDFCOmpressImage);
            this.groupBox19.Controls.Add(this.Image2PDFOptiomize);
            this.groupBox19.Controls.Add(this.groupBox20);
            this.groupBox19.Controls.Add(this.Image2PDFCombox);
            this.groupBox19.Controls.Add(this.label38);
            this.groupBox19.Controls.Add(this.ExportSignlePDF);
            this.groupBox19.Location = new System.Drawing.Point(4, 431);
            this.groupBox19.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox19.Name = "groupBox19";
            this.groupBox19.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox19.Size = new System.Drawing.Size(836, 95);
            this.groupBox19.TabIndex = 3;
            this.groupBox19.TabStop = false;
            this.groupBox19.Tag = "anchor_left_bottom";
            this.groupBox19.Text = "参数设置";
            // 
            // Image2PDFCOmpressImage
            // 
            this.Image2PDFCOmpressImage.AutoSize = true;
            this.Image2PDFCOmpressImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Image2PDFCOmpressImage.Location = new System.Drawing.Point(123, 21);
            this.Image2PDFCOmpressImage.Margin = new System.Windows.Forms.Padding(1);
            this.Image2PDFCOmpressImage.Name = "Image2PDFCOmpressImage";
            this.Image2PDFCOmpressImage.Size = new System.Drawing.Size(72, 16);
            this.Image2PDFCOmpressImage.TabIndex = 5;
            this.Image2PDFCOmpressImage.Text = "压缩图片";
            this.Image2PDFCOmpressImage.UseVisualStyleBackColor = true;
            // 
            // Image2PDFOptiomize
            // 
            this.Image2PDFOptiomize.AutoSize = true;
            this.Image2PDFOptiomize.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Image2PDFOptiomize.Location = new System.Drawing.Point(7, 57);
            this.Image2PDFOptiomize.Margin = new System.Windows.Forms.Padding(1);
            this.Image2PDFOptiomize.Name = "Image2PDFOptiomize";
            this.Image2PDFOptiomize.Size = new System.Drawing.Size(276, 16);
            this.Image2PDFOptiomize.TabIndex = 4;
            this.Image2PDFOptiomize.Text = "自动优化文档（压缩图片、优化页面流等操作）";
            this.Image2PDFOptiomize.UseVisualStyleBackColor = true;
            // 
            // groupBox20
            // 
            this.groupBox20.Controls.Add(this.ImagePdfPageSIze_RIGHT);
            this.groupBox20.Controls.Add(this.ImagePdfPageSIze_BOTTOM);
            this.groupBox20.Controls.Add(this.ImagePdfPageSIze_LEFT);
            this.groupBox20.Controls.Add(this.ImagePdfPageSIze_UP);
            this.groupBox20.Controls.Add(this.label43);
            this.groupBox20.Controls.Add(this.label42);
            this.groupBox20.Controls.Add(this.label41);
            this.groupBox20.Controls.Add(this.label40);
            this.groupBox20.Location = new System.Drawing.Point(599, 9);
            this.groupBox20.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox20.Name = "groupBox20";
            this.groupBox20.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox20.Size = new System.Drawing.Size(227, 81);
            this.groupBox20.TabIndex = 3;
            this.groupBox20.TabStop = false;
            this.groupBox20.Text = "页面边距设置";
            // 
            // ImagePdfPageSIze_RIGHT
            // 
            this.ImagePdfPageSIze_RIGHT.Location = new System.Drawing.Point(143, 45);
            this.ImagePdfPageSIze_RIGHT.Margin = new System.Windows.Forms.Padding(1);
            this.ImagePdfPageSIze_RIGHT.Name = "ImagePdfPageSIze_RIGHT";
            this.ImagePdfPageSIze_RIGHT.Size = new System.Drawing.Size(68, 21);
            this.ImagePdfPageSIze_RIGHT.TabIndex = 7;
            this.ImagePdfPageSIze_RIGHT.Text = "0";
            // 
            // ImagePdfPageSIze_BOTTOM
            // 
            this.ImagePdfPageSIze_BOTTOM.Location = new System.Drawing.Point(33, 45);
            this.ImagePdfPageSIze_BOTTOM.Margin = new System.Windows.Forms.Padding(1);
            this.ImagePdfPageSIze_BOTTOM.Name = "ImagePdfPageSIze_BOTTOM";
            this.ImagePdfPageSIze_BOTTOM.Size = new System.Drawing.Size(68, 21);
            this.ImagePdfPageSIze_BOTTOM.TabIndex = 6;
            this.ImagePdfPageSIze_BOTTOM.Text = "0";
            // 
            // ImagePdfPageSIze_LEFT
            // 
            this.ImagePdfPageSIze_LEFT.Location = new System.Drawing.Point(143, 17);
            this.ImagePdfPageSIze_LEFT.Margin = new System.Windows.Forms.Padding(1);
            this.ImagePdfPageSIze_LEFT.Name = "ImagePdfPageSIze_LEFT";
            this.ImagePdfPageSIze_LEFT.Size = new System.Drawing.Size(68, 21);
            this.ImagePdfPageSIze_LEFT.TabIndex = 5;
            this.ImagePdfPageSIze_LEFT.Text = "0";
            // 
            // ImagePdfPageSIze_UP
            // 
            this.ImagePdfPageSIze_UP.Location = new System.Drawing.Point(33, 17);
            this.ImagePdfPageSIze_UP.Margin = new System.Windows.Forms.Padding(1);
            this.ImagePdfPageSIze_UP.Name = "ImagePdfPageSIze_UP";
            this.ImagePdfPageSIze_UP.Size = new System.Drawing.Size(68, 21);
            this.ImagePdfPageSIze_UP.TabIndex = 4;
            this.ImagePdfPageSIze_UP.Text = "0";
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label43.Location = new System.Drawing.Point(121, 47);
            this.label43.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(17, 12);
            this.label43.TabIndex = 3;
            this.label43.Text = "右";
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label42.Location = new System.Drawing.Point(121, 19);
            this.label42.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(17, 12);
            this.label42.TabIndex = 2;
            this.label42.Text = "左";
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label41.Location = new System.Drawing.Point(13, 47);
            this.label41.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(17, 12);
            this.label41.TabIndex = 1;
            this.label41.Text = "下";
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label40.Location = new System.Drawing.Point(13, 19);
            this.label40.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(17, 12);
            this.label40.TabIndex = 0;
            this.label40.Text = "上";
            // 
            // Image2PDFCombox
            // 
            this.Image2PDFCombox.FormattingEnabled = true;
            this.Image2PDFCombox.Location = new System.Drawing.Point(255, 17);
            this.Image2PDFCombox.Margin = new System.Windows.Forms.Padding(1);
            this.Image2PDFCombox.Name = "Image2PDFCombox";
            this.Image2PDFCombox.Size = new System.Drawing.Size(83, 20);
            this.Image2PDFCombox.TabIndex = 2;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label38.Location = new System.Drawing.Point(199, 21);
            this.label38.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(53, 12);
            this.label38.TabIndex = 1;
            this.label38.Text = "页面布局";
            // 
            // ExportSignlePDF
            // 
            this.ExportSignlePDF.AutoSize = true;
            this.ExportSignlePDF.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExportSignlePDF.Location = new System.Drawing.Point(7, 21);
            this.ExportSignlePDF.Margin = new System.Windows.Forms.Padding(1);
            this.ExportSignlePDF.Name = "ExportSignlePDF";
            this.ExportSignlePDF.Size = new System.Drawing.Size(114, 16);
            this.ExportSignlePDF.TabIndex = 0;
            this.ExportSignlePDF.Text = "导出为单独的PDF";
            this.ExportSignlePDF.UseVisualStyleBackColor = true;
            // 
            // ConvertImage2PDF
            // 
            this.ConvertImage2PDF.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertImage2PDF.Location = new System.Drawing.Point(697, 561);
            this.ConvertImage2PDF.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertImage2PDF.Name = "ConvertImage2PDF";
            this.ConvertImage2PDF.Size = new System.Drawing.Size(147, 37);
            this.ConvertImage2PDF.TabIndex = 2;
            this.ConvertImage2PDF.Tag = "anchor_right_bottom";
            this.ConvertImage2PDF.Text = "开始处理";
            this.ConvertImage2PDF.UseVisualStyleBackColor = true;
            this.ConvertImage2PDF.Click += new System.EventHandler(this.ConvertImage2PDF_Click);
            // 
            // SelectConvertImage2PDF
            // 
            this.SelectConvertImage2PDF.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectConvertImage2PDF.Location = new System.Drawing.Point(5, 561);
            this.SelectConvertImage2PDF.Margin = new System.Windows.Forms.Padding(1);
            this.SelectConvertImage2PDF.Name = "SelectConvertImage2PDF";
            this.SelectConvertImage2PDF.Size = new System.Drawing.Size(147, 37);
            this.SelectConvertImage2PDF.TabIndex = 1;
            this.SelectConvertImage2PDF.Tag = "anchor_left_bottom";
            this.SelectConvertImage2PDF.Text = "选择文件";
            this.SelectConvertImage2PDF.UseVisualStyleBackColor = true;
            this.SelectConvertImage2PDF.Click += new System.EventHandler(this.SelectConvertImage2PDF_Click);
            // 
            // ConvertImage2PDFFileList
            // 
            this.ConvertImage2PDFFileList.HideSelection = false;
            this.ConvertImage2PDFFileList.Location = new System.Drawing.Point(4, 4);
            this.ConvertImage2PDFFileList.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertImage2PDFFileList.Name = "ConvertImage2PDFFileList";
            this.ConvertImage2PDFFileList.Size = new System.Drawing.Size(841, 420);
            this.ConvertImage2PDFFileList.TabIndex = 0;
            this.ConvertImage2PDFFileList.Tag = "anchor_full_fill";
            this.ConvertImage2PDFFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_30
            // 
            this.TabPage_30.Controls.Add(this.linkLabel4);
            this.TabPage_30.Controls.Add(this.label74);
            this.TabPage_30.Controls.Add(this.groupBox31);
            this.TabPage_30.Controls.Add(this.groupBox30);
            this.TabPage_30.Controls.Add(this.groupBox29);
            this.TabPage_30.Controls.Add(this.label66);
            this.TabPage_30.Controls.Add(this.HTML2PDFFileList_MoveDown);
            this.TabPage_30.Controls.Add(this.HTML2PDFFileList_MoveUp);
            this.TabPage_30.Controls.Add(this.html2pdf_clear);
            this.TabPage_30.Controls.Add(this.html2pdf_remove);
            this.TabPage_30.Controls.Add(this.ConvertHtml2PdfStatusText);
            this.TabPage_30.Controls.Add(this.ConvertHtml2Pdf);
            this.TabPage_30.Controls.Add(this.SelectHtml2PDFFiles);
            this.TabPage_30.Controls.Add(this.HTML2PDFFileList);
            this.TabPage_30.Location = new System.Drawing.Point(4, 76);
            this.TabPage_30.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_30.Name = "TabPage_30";
            this.TabPage_30.Size = new System.Drawing.Size(882, 659);
            this.TabPage_30.TabIndex = 29;
            this.TabPage_30.Text = "HTML转PDF";
            this.TabPage_30.UseVisualStyleBackColor = true;
            // 
            // linkLabel4
            // 
            this.linkLabel4.AutoSize = true;
            this.linkLabel4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabel4.Location = new System.Drawing.Point(501, 376);
            this.linkLabel4.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.linkLabel4.Name = "linkLabel4";
            this.linkLabel4.Size = new System.Drawing.Size(119, 12);
            this.linkLabel4.TabIndex = 29;
            this.linkLabel4.TabStop = true;
            this.linkLabel4.Tag = "anchor_left_bottom";
            this.linkLabel4.Text = "点击前往下载SAM扩展";
            this.linkLabel4.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel4_LinkClicked);
            // 
            // label74
            // 
            this.label74.AutoSize = true;
            this.label74.ForeColor = System.Drawing.Color.DarkGreen;
            this.label74.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label74.Location = new System.Drawing.Point(5, 376);
            this.label74.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label74.Name = "label74";
            this.label74.Size = new System.Drawing.Size(491, 12);
            this.label74.TabIndex = 24;
            this.label74.Tag = "anchor_left_bottom";
            this.label74.Text = "提示：推荐使用SAM扩展保存HTML页面，避免部分网站的防盗链设置从而无法保存图片等内容";
            // 
            // groupBox31
            // 
            this.groupBox31.Controls.Add(this.label129);
            this.groupBox31.Controls.Add(this.label128);
            this.groupBox31.Controls.Add(this.html2pdf_wait_timeout);
            this.groupBox31.Controls.Add(this.html2pdf_timeout);
            this.groupBox31.Controls.Add(this.label127);
            this.groupBox31.Controls.Add(this.html2pdf_button_set_chrome_path);
            this.groupBox31.Controls.Add(this.label126);
            this.groupBox31.Controls.Add(this.linkLabel3);
            this.groupBox31.Controls.Add(this.html2pdf_textbox_chromepath);
            this.groupBox31.Controls.Add(this.label73);
            this.groupBox31.Location = new System.Drawing.Point(9, 393);
            this.groupBox31.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox31.Name = "groupBox31";
            this.groupBox31.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox31.Size = new System.Drawing.Size(369, 127);
            this.groupBox31.TabIndex = 23;
            this.groupBox31.TabStop = false;
            this.groupBox31.Tag = "anchor_left_bottom";
            this.groupBox31.Text = "设置chromium路径";
            // 
            // label129
            // 
            this.label129.AutoSize = true;
            this.label129.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label129.Location = new System.Drawing.Point(209, 103);
            this.label129.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label129.Name = "label129";
            this.label129.Size = new System.Drawing.Size(17, 12);
            this.label129.TabIndex = 28;
            this.label129.Text = "秒";
            // 
            // label128
            // 
            this.label128.AutoSize = true;
            this.label128.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label128.Location = new System.Drawing.Point(209, 77);
            this.label128.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label128.Name = "label128";
            this.label128.Size = new System.Drawing.Size(17, 12);
            this.label128.TabIndex = 27;
            this.label128.Text = "秒";
            // 
            // html2pdf_wait_timeout
            // 
            this.html2pdf_wait_timeout.Location = new System.Drawing.Point(89, 100);
            this.html2pdf_wait_timeout.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_wait_timeout.Maximum = new decimal(new int[] {
            276447231,
            23283,
            0,
            0});
            this.html2pdf_wait_timeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.html2pdf_wait_timeout.Name = "html2pdf_wait_timeout";
            this.html2pdf_wait_timeout.Size = new System.Drawing.Size(116, 21);
            this.html2pdf_wait_timeout.TabIndex = 26;
            this.html2pdf_wait_timeout.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // html2pdf_timeout
            // 
            this.html2pdf_timeout.Location = new System.Drawing.Point(89, 73);
            this.html2pdf_timeout.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_timeout.Maximum = new decimal(new int[] {
            276447231,
            23283,
            0,
            0});
            this.html2pdf_timeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.html2pdf_timeout.Name = "html2pdf_timeout";
            this.html2pdf_timeout.Size = new System.Drawing.Size(116, 21);
            this.html2pdf_timeout.TabIndex = 25;
            this.html2pdf_timeout.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label127
            // 
            this.label127.AutoSize = true;
            this.label127.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label127.Location = new System.Drawing.Point(8, 103);
            this.label127.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label127.Name = "label127";
            this.label127.Size = new System.Drawing.Size(77, 12);
            this.label127.TabIndex = 6;
            this.label127.Text = "加载时间设置";
            // 
            // html2pdf_button_set_chrome_path
            // 
            this.html2pdf_button_set_chrome_path.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.html2pdf_button_set_chrome_path.Location = new System.Drawing.Point(211, 47);
            this.html2pdf_button_set_chrome_path.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_button_set_chrome_path.Name = "html2pdf_button_set_chrome_path";
            this.html2pdf_button_set_chrome_path.Size = new System.Drawing.Size(119, 21);
            this.html2pdf_button_set_chrome_path.TabIndex = 24;
            this.html2pdf_button_set_chrome_path.Text = "设置路径...";
            this.html2pdf_button_set_chrome_path.UseVisualStyleBackColor = true;
            this.html2pdf_button_set_chrome_path.Click += new System.EventHandler(this.Html2pdf_button_set_chrome_path_Click);
            // 
            // label126
            // 
            this.label126.AutoSize = true;
            this.label126.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label126.Location = new System.Drawing.Point(8, 77);
            this.label126.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label126.Name = "label126";
            this.label126.Size = new System.Drawing.Size(77, 12);
            this.label126.TabIndex = 5;
            this.label126.Text = "超时时间设置";
            // 
            // linkLabel3
            // 
            this.linkLabel3.AutoSize = true;
            this.linkLabel3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabel3.Location = new System.Drawing.Point(8, 47);
            this.linkLabel3.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.linkLabel3.Name = "linkLabel3";
            this.linkLabel3.Size = new System.Drawing.Size(125, 12);
            this.linkLabel3.TabIndex = 2;
            this.linkLabel3.TabStop = true;
            this.linkLabel3.Text = "点击前往下载chromium";
            this.linkLabel3.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel3_LinkClicked);
            // 
            // html2pdf_textbox_chromepath
            // 
            this.html2pdf_textbox_chromepath.Location = new System.Drawing.Point(39, 19);
            this.html2pdf_textbox_chromepath.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_textbox_chromepath.Name = "html2pdf_textbox_chromepath";
            this.html2pdf_textbox_chromepath.ReadOnly = true;
            this.html2pdf_textbox_chromepath.Size = new System.Drawing.Size(293, 21);
            this.html2pdf_textbox_chromepath.TabIndex = 1;
            this.html2pdf_textbox_chromepath.MouseHover += new System.EventHandler(this.Html2pdf_textbox_chromepath_MouseHover);
            // 
            // label73
            // 
            this.label73.AutoSize = true;
            this.label73.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label73.Location = new System.Drawing.Point(5, 23);
            this.label73.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label73.Name = "label73";
            this.label73.Size = new System.Drawing.Size(29, 12);
            this.label73.TabIndex = 0;
            this.label73.Text = "路径";
            // 
            // groupBox30
            // 
            this.groupBox30.Controls.Add(this.html2pdf_textbox_margin_bottom);
            this.groupBox30.Controls.Add(this.html2pdf_textbox_margin_top);
            this.groupBox30.Controls.Add(this.html2pdf_textbox_margin_right);
            this.groupBox30.Controls.Add(this.html2pdf_textbox_margin_left);
            this.groupBox30.Controls.Add(this.label72);
            this.groupBox30.Controls.Add(this.label71);
            this.groupBox30.Controls.Add(this.label70);
            this.groupBox30.Controls.Add(this.label69);
            this.groupBox30.Location = new System.Drawing.Point(617, 393);
            this.groupBox30.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox30.Name = "groupBox30";
            this.groupBox30.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox30.Size = new System.Drawing.Size(231, 127);
            this.groupBox30.TabIndex = 5;
            this.groupBox30.TabStop = false;
            this.groupBox30.Tag = "anchor_left_bottom";
            this.groupBox30.Text = "页面边距设置";
            // 
            // html2pdf_textbox_margin_bottom
            // 
            this.html2pdf_textbox_margin_bottom.Location = new System.Drawing.Point(57, 89);
            this.html2pdf_textbox_margin_bottom.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_textbox_margin_bottom.Name = "html2pdf_textbox_margin_bottom";
            this.html2pdf_textbox_margin_bottom.Size = new System.Drawing.Size(45, 21);
            this.html2pdf_textbox_margin_bottom.TabIndex = 22;
            this.html2pdf_textbox_margin_bottom.Text = "50";
            // 
            // html2pdf_textbox_margin_top
            // 
            this.html2pdf_textbox_margin_top.Location = new System.Drawing.Point(57, 25);
            this.html2pdf_textbox_margin_top.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_textbox_margin_top.Name = "html2pdf_textbox_margin_top";
            this.html2pdf_textbox_margin_top.Size = new System.Drawing.Size(45, 21);
            this.html2pdf_textbox_margin_top.TabIndex = 21;
            this.html2pdf_textbox_margin_top.Text = "50";
            // 
            // html2pdf_textbox_margin_right
            // 
            this.html2pdf_textbox_margin_right.Location = new System.Drawing.Point(173, 89);
            this.html2pdf_textbox_margin_right.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_textbox_margin_right.Name = "html2pdf_textbox_margin_right";
            this.html2pdf_textbox_margin_right.Size = new System.Drawing.Size(45, 21);
            this.html2pdf_textbox_margin_right.TabIndex = 20;
            this.html2pdf_textbox_margin_right.Text = "50";
            // 
            // html2pdf_textbox_margin_left
            // 
            this.html2pdf_textbox_margin_left.Location = new System.Drawing.Point(173, 25);
            this.html2pdf_textbox_margin_left.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_textbox_margin_left.Name = "html2pdf_textbox_margin_left";
            this.html2pdf_textbox_margin_left.Size = new System.Drawing.Size(45, 21);
            this.html2pdf_textbox_margin_left.TabIndex = 19;
            this.html2pdf_textbox_margin_left.Text = "50";
            // 
            // label72
            // 
            this.label72.AutoSize = true;
            this.label72.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label72.Location = new System.Drawing.Point(128, 95);
            this.label72.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label72.Name = "label72";
            this.label72.Size = new System.Drawing.Size(41, 12);
            this.label72.TabIndex = 16;
            this.label72.Text = "右边距";
            // 
            // label71
            // 
            this.label71.AutoSize = true;
            this.label71.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label71.Location = new System.Drawing.Point(128, 29);
            this.label71.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label71.Name = "label71";
            this.label71.Size = new System.Drawing.Size(41, 12);
            this.label71.TabIndex = 15;
            this.label71.Text = "左边距";
            // 
            // label70
            // 
            this.label70.AutoSize = true;
            this.label70.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label70.Location = new System.Drawing.Point(13, 95);
            this.label70.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label70.Name = "label70";
            this.label70.Size = new System.Drawing.Size(41, 12);
            this.label70.TabIndex = 14;
            this.label70.Text = "下边距";
            // 
            // label69
            // 
            this.label69.AutoSize = true;
            this.label69.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label69.Location = new System.Drawing.Point(13, 29);
            this.label69.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label69.Name = "label69";
            this.label69.Size = new System.Drawing.Size(41, 12);
            this.label69.TabIndex = 13;
            this.label69.Text = "上边距";
            // 
            // groupBox29
            // 
            this.groupBox29.Controls.Add(this.html2pdf_wait_all_resource);
            this.groupBox29.Controls.Add(this.html2pdf_combox_pagedirect);
            this.groupBox29.Controls.Add(this.html2pdf_combox_pagesize);
            this.groupBox29.Controls.Add(this.html2pdf_checkbox_usecss);
            this.groupBox29.Controls.Add(this.label68);
            this.groupBox29.Controls.Add(this.label67);
            this.groupBox29.Location = new System.Drawing.Point(381, 393);
            this.groupBox29.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox29.Name = "groupBox29";
            this.groupBox29.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox29.Size = new System.Drawing.Size(232, 127);
            this.groupBox29.TabIndex = 22;
            this.groupBox29.TabStop = false;
            this.groupBox29.Tag = "anchor_left_bottom";
            this.groupBox29.Text = "生成PDF参数设置";
            // 
            // html2pdf_wait_all_resource
            // 
            this.html2pdf_wait_all_resource.AutoSize = true;
            this.html2pdf_wait_all_resource.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.html2pdf_wait_all_resource.Location = new System.Drawing.Point(4, 101);
            this.html2pdf_wait_all_resource.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_wait_all_resource.Name = "html2pdf_wait_all_resource";
            this.html2pdf_wait_all_resource.Size = new System.Drawing.Size(168, 16);
            this.html2pdf_wait_all_resource.TabIndex = 5;
            this.html2pdf_wait_all_resource.Text = "等待所有页面元素加载完成";
            this.html2pdf_wait_all_resource.UseVisualStyleBackColor = true;
            // 
            // html2pdf_combox_pagedirect
            // 
            this.html2pdf_combox_pagedirect.FormattingEnabled = true;
            this.html2pdf_combox_pagedirect.Location = new System.Drawing.Point(69, 49);
            this.html2pdf_combox_pagedirect.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_combox_pagedirect.Name = "html2pdf_combox_pagedirect";
            this.html2pdf_combox_pagedirect.Size = new System.Drawing.Size(117, 20);
            this.html2pdf_combox_pagedirect.TabIndex = 4;
            // 
            // html2pdf_combox_pagesize
            // 
            this.html2pdf_combox_pagesize.FormattingEnabled = true;
            this.html2pdf_combox_pagesize.Location = new System.Drawing.Point(69, 23);
            this.html2pdf_combox_pagesize.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_combox_pagesize.Name = "html2pdf_combox_pagesize";
            this.html2pdf_combox_pagesize.Size = new System.Drawing.Size(117, 20);
            this.html2pdf_combox_pagesize.TabIndex = 3;
            // 
            // html2pdf_checkbox_usecss
            // 
            this.html2pdf_checkbox_usecss.AutoSize = true;
            this.html2pdf_checkbox_usecss.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.html2pdf_checkbox_usecss.Location = new System.Drawing.Point(4, 73);
            this.html2pdf_checkbox_usecss.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_checkbox_usecss.Name = "html2pdf_checkbox_usecss";
            this.html2pdf_checkbox_usecss.Size = new System.Drawing.Size(162, 16);
            this.html2pdf_checkbox_usecss.TabIndex = 2;
            this.html2pdf_checkbox_usecss.Text = "使用内部CSS样式设置页面";
            this.html2pdf_checkbox_usecss.UseVisualStyleBackColor = true;
            // 
            // label68
            // 
            this.label68.AutoSize = true;
            this.label68.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label68.Location = new System.Drawing.Point(11, 52);
            this.label68.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label68.Name = "label68";
            this.label68.Size = new System.Drawing.Size(53, 12);
            this.label68.TabIndex = 1;
            this.label68.Text = "页面方向";
            // 
            // label67
            // 
            this.label67.AutoSize = true;
            this.label67.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label67.Location = new System.Drawing.Point(11, 25);
            this.label67.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label67.Name = "label67";
            this.label67.Size = new System.Drawing.Size(53, 12);
            this.label67.TabIndex = 0;
            this.label67.Text = "页面大小";
            // 
            // label66
            // 
            this.label66.AutoSize = true;
            this.label66.ForeColor = System.Drawing.Color.Red;
            this.label66.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label66.Location = new System.Drawing.Point(5, 352);
            this.label66.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label66.Name = "label66";
            this.label66.Size = new System.Drawing.Size(623, 12);
            this.label66.TabIndex = 21;
            this.label66.Tag = "anchor_left_bottom";
            this.label66.Text = "提示：本功能使用浏览器渲染方式进行转换，生成完毕后，需等待PDF生成，需要一定时间；且右键菜单部分功能失效";
            // 
            // HTML2PDFFileList_MoveDown
            // 
            this.HTML2PDFFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.HTML2PDFFileList_MoveDown.Location = new System.Drawing.Point(847, 261);
            this.HTML2PDFFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.HTML2PDFFileList_MoveDown.Name = "HTML2PDFFileList_MoveDown";
            this.HTML2PDFFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.HTML2PDFFileList_MoveDown.TabIndex = 18;
            this.HTML2PDFFileList_MoveDown.Tag = "anchor_right";
            this.HTML2PDFFileList_MoveDown.Text = "▽";
            this.HTML2PDFFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // HTML2PDFFileList_MoveUp
            // 
            this.HTML2PDFFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.HTML2PDFFileList_MoveUp.Location = new System.Drawing.Point(847, 19);
            this.HTML2PDFFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.HTML2PDFFileList_MoveUp.Name = "HTML2PDFFileList_MoveUp";
            this.HTML2PDFFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.HTML2PDFFileList_MoveUp.TabIndex = 17;
            this.HTML2PDFFileList_MoveUp.Tag = "anchor_right";
            this.HTML2PDFFileList_MoveUp.Text = "△";
            this.HTML2PDFFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // html2pdf_clear
            // 
            this.html2pdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.html2pdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.html2pdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.html2pdf_clear.Location = new System.Drawing.Point(847, 180);
            this.html2pdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_clear.Name = "html2pdf_clear";
            this.html2pdf_clear.Size = new System.Drawing.Size(23, 57);
            this.html2pdf_clear.TabIndex = 20;
            this.html2pdf_clear.Tag = "anchor_right";
            this.html2pdf_clear.UseVisualStyleBackColor = true;
            // 
            // html2pdf_remove
            // 
            this.html2pdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.html2pdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.html2pdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.html2pdf_remove.Location = new System.Drawing.Point(847, 99);
            this.html2pdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.html2pdf_remove.Name = "html2pdf_remove";
            this.html2pdf_remove.Size = new System.Drawing.Size(23, 57);
            this.html2pdf_remove.TabIndex = 19;
            this.html2pdf_remove.Tag = "anchor_right";
            this.html2pdf_remove.UseVisualStyleBackColor = true;
            // 
            // ConvertHtml2PdfStatusText
            // 
            this.ConvertHtml2PdfStatusText.AutoSize = true;
            this.ConvertHtml2PdfStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ConvertHtml2PdfStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertHtml2PdfStatusText.Location = new System.Drawing.Point(156, 575);
            this.ConvertHtml2PdfStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ConvertHtml2PdfStatusText.Name = "ConvertHtml2PdfStatusText";
            this.ConvertHtml2PdfStatusText.Size = new System.Drawing.Size(95, 12);
            this.ConvertHtml2PdfStatusText.TabIndex = 16;
            this.ConvertHtml2PdfStatusText.Tag = "anchor_left_bottom";
            this.ConvertHtml2PdfStatusText.Text = "正在处理X个文件";
            // 
            // ConvertHtml2Pdf
            // 
            this.ConvertHtml2Pdf.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertHtml2Pdf.Location = new System.Drawing.Point(697, 561);
            this.ConvertHtml2Pdf.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertHtml2Pdf.Name = "ConvertHtml2Pdf";
            this.ConvertHtml2Pdf.Size = new System.Drawing.Size(147, 37);
            this.ConvertHtml2Pdf.TabIndex = 15;
            this.ConvertHtml2Pdf.Tag = "anchor_right_bottom";
            this.ConvertHtml2Pdf.Text = "开始处理";
            this.ConvertHtml2Pdf.UseVisualStyleBackColor = true;
            this.ConvertHtml2Pdf.Click += new System.EventHandler(this.ConvertHtml2Pdf_Click_1);
            // 
            // SelectHtml2PDFFiles
            // 
            this.SelectHtml2PDFFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectHtml2PDFFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectHtml2PDFFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectHtml2PDFFiles.Name = "SelectHtml2PDFFiles";
            this.SelectHtml2PDFFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectHtml2PDFFiles.TabIndex = 14;
            this.SelectHtml2PDFFiles.Tag = "anchor_left_bottom";
            this.SelectHtml2PDFFiles.Text = "选择文件";
            this.SelectHtml2PDFFiles.UseVisualStyleBackColor = true;
            this.SelectHtml2PDFFiles.Click += new System.EventHandler(this.SelectHtml2PDFFiles_Click_1);
            // 
            // HTML2PDFFileList
            // 
            this.HTML2PDFFileList.HideSelection = false;
            this.HTML2PDFFileList.Location = new System.Drawing.Point(4, 4);
            this.HTML2PDFFileList.Margin = new System.Windows.Forms.Padding(1);
            this.HTML2PDFFileList.Name = "HTML2PDFFileList";
            this.HTML2PDFFileList.Size = new System.Drawing.Size(841, 335);
            this.HTML2PDFFileList.TabIndex = 13;
            this.HTML2PDFFileList.Tag = "anchor_full_fill";
            this.HTML2PDFFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_24
            // 
            this.TabPage_24.Controls.Add(this.CompressPdfFileList_MoveDown);
            this.TabPage_24.Controls.Add(this.CompressPdfFileList_MoveUp);
            this.TabPage_24.Controls.Add(this.compresspdf_clear);
            this.TabPage_24.Controls.Add(this.compresspdf_remove);
            this.TabPage_24.Controls.Add(this.CompressPDFStatusText);
            this.TabPage_24.Controls.Add(this.groupBox21);
            this.TabPage_24.Controls.Add(this.CompressPDF);
            this.TabPage_24.Controls.Add(this.SelectCompressPDFFiles);
            this.TabPage_24.Controls.Add(this.CompressPdfFileList);
            this.TabPage_24.Location = new System.Drawing.Point(4, 76);
            this.TabPage_24.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_24.Name = "TabPage_24";
            this.TabPage_24.Size = new System.Drawing.Size(882, 659);
            this.TabPage_24.TabIndex = 23;
            this.TabPage_24.Text = "压缩PDF";
            this.TabPage_24.UseVisualStyleBackColor = true;
            // 
            // CompressPdfFileList_MoveDown
            // 
            this.CompressPdfFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPdfFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.CompressPdfFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPdfFileList_MoveDown.Name = "CompressPdfFileList_MoveDown";
            this.CompressPdfFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.CompressPdfFileList_MoveDown.TabIndex = 6;
            this.CompressPdfFileList_MoveDown.Tag = "anchor_right";
            this.CompressPdfFileList_MoveDown.Text = "▽";
            this.CompressPdfFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // CompressPdfFileList_MoveUp
            // 
            this.CompressPdfFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPdfFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.CompressPdfFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPdfFileList_MoveUp.Name = "CompressPdfFileList_MoveUp";
            this.CompressPdfFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.CompressPdfFileList_MoveUp.TabIndex = 5;
            this.CompressPdfFileList_MoveUp.Tag = "anchor_right";
            this.CompressPdfFileList_MoveUp.Text = "△";
            this.CompressPdfFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // compresspdf_clear
            // 
            this.compresspdf_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.compresspdf_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.compresspdf_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.compresspdf_clear.Location = new System.Drawing.Point(847, 225);
            this.compresspdf_clear.Margin = new System.Windows.Forms.Padding(1);
            this.compresspdf_clear.Name = "compresspdf_clear";
            this.compresspdf_clear.Size = new System.Drawing.Size(23, 57);
            this.compresspdf_clear.TabIndex = 12;
            this.compresspdf_clear.Tag = "anchor_right";
            this.compresspdf_clear.UseVisualStyleBackColor = true;
            // 
            // compresspdf_remove
            // 
            this.compresspdf_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.compresspdf_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.compresspdf_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.compresspdf_remove.Location = new System.Drawing.Point(847, 145);
            this.compresspdf_remove.Margin = new System.Windows.Forms.Padding(1);
            this.compresspdf_remove.Name = "compresspdf_remove";
            this.compresspdf_remove.Size = new System.Drawing.Size(23, 57);
            this.compresspdf_remove.TabIndex = 11;
            this.compresspdf_remove.Tag = "anchor_right";
            this.compresspdf_remove.UseVisualStyleBackColor = true;
            // 
            // CompressPDFStatusText
            // 
            this.CompressPDFStatusText.AutoSize = true;
            this.CompressPDFStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.CompressPDFStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDFStatusText.Location = new System.Drawing.Point(156, 575);
            this.CompressPDFStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.CompressPDFStatusText.Name = "CompressPDFStatusText";
            this.CompressPDFStatusText.Size = new System.Drawing.Size(95, 12);
            this.CompressPDFStatusText.TabIndex = 4;
            this.CompressPDFStatusText.Tag = "anchor_left_bottom";
            this.CompressPDFStatusText.Text = "正在处理X个文件";
            // 
            // groupBox21
            // 
            this.groupBox21.Controls.Add(this.CompressPDF_CompressImageFlate);
            this.groupBox21.Controls.Add(this.CompressPDF_RemoveForms);
            this.groupBox21.Controls.Add(this.CompressPDF_CancelEmbedFont);
            this.groupBox21.Controls.Add(this.CompressPDF_LinkSameStream);
            this.groupBox21.Controls.Add(this.CompressPDF_RemoveUnuseStream);
            this.groupBox21.Controls.Add(this.CompressPDF_RemoveUnuseObject);
            this.groupBox21.Controls.Add(this.CompressPDF_ImageCompressVersion);
            this.groupBox21.Controls.Add(this.label46);
            this.groupBox21.Controls.Add(this.CompressPDF_ImageResoution);
            this.groupBox21.Controls.Add(this.label45);
            this.groupBox21.Controls.Add(this.CompressPDF_ImageQuailty);
            this.groupBox21.Controls.Add(this.label44);
            this.groupBox21.Controls.Add(this.CompressPDF_CompressImage);
            this.groupBox21.Controls.Add(this.CompressPDF_WEB);
            this.groupBox21.Location = new System.Drawing.Point(4, 431);
            this.groupBox21.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox21.Name = "groupBox21";
            this.groupBox21.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox21.Size = new System.Drawing.Size(839, 79);
            this.groupBox21.TabIndex = 3;
            this.groupBox21.TabStop = false;
            this.groupBox21.Tag = "anchor_left_bottom";
            this.groupBox21.Text = "参数设置";
            // 
            // CompressPDF_CompressImageFlate
            // 
            this.CompressPDF_CompressImageFlate.AutoSize = true;
            this.CompressPDF_CompressImageFlate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_CompressImageFlate.Location = new System.Drawing.Point(491, 15);
            this.CompressPDF_CompressImageFlate.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_CompressImageFlate.Name = "CompressPDF_CompressImageFlate";
            this.CompressPDF_CompressImageFlate.Size = new System.Drawing.Size(126, 16);
            this.CompressPDF_CompressImageFlate.TabIndex = 15;
            this.CompressPDF_CompressImageFlate.Text = "使用Flate压缩图像";
            this.CompressPDF_CompressImageFlate.UseVisualStyleBackColor = true;
            // 
            // CompressPDF_RemoveForms
            // 
            this.CompressPDF_RemoveForms.AutoSize = true;
            this.CompressPDF_RemoveForms.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_RemoveForms.Location = new System.Drawing.Point(379, 52);
            this.CompressPDF_RemoveForms.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_RemoveForms.Name = "CompressPDF_RemoveForms";
            this.CompressPDF_RemoveForms.Size = new System.Drawing.Size(84, 16);
            this.CompressPDF_RemoveForms.TabIndex = 14;
            this.CompressPDF_RemoveForms.Text = "删除表单域";
            this.CompressPDF_RemoveForms.UseVisualStyleBackColor = true;
            // 
            // CompressPDF_CancelEmbedFont
            // 
            this.CompressPDF_CancelEmbedFont.AutoSize = true;
            this.CompressPDF_CancelEmbedFont.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_CancelEmbedFont.Location = new System.Drawing.Point(257, 52);
            this.CompressPDF_CancelEmbedFont.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_CancelEmbedFont.Name = "CompressPDF_CancelEmbedFont";
            this.CompressPDF_CancelEmbedFont.Size = new System.Drawing.Size(96, 16);
            this.CompressPDF_CancelEmbedFont.TabIndex = 13;
            this.CompressPDF_CancelEmbedFont.Text = "取消嵌入字体";
            this.CompressPDF_CancelEmbedFont.UseVisualStyleBackColor = true;
            // 
            // CompressPDF_LinkSameStream
            // 
            this.CompressPDF_LinkSameStream.AutoSize = true;
            this.CompressPDF_LinkSameStream.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_LinkSameStream.Location = new System.Drawing.Point(149, 52);
            this.CompressPDF_LinkSameStream.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_LinkSameStream.Name = "CompressPDF_LinkSameStream";
            this.CompressPDF_LinkSameStream.Size = new System.Drawing.Size(84, 16);
            this.CompressPDF_LinkSameStream.TabIndex = 12;
            this.CompressPDF_LinkSameStream.Text = "链接重复流";
            this.CompressPDF_LinkSameStream.UseVisualStyleBackColor = true;
            // 
            // CompressPDF_RemoveUnuseStream
            // 
            this.CompressPDF_RemoveUnuseStream.AutoSize = true;
            this.CompressPDF_RemoveUnuseStream.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_RemoveUnuseStream.Location = new System.Drawing.Point(4, 52);
            this.CompressPDF_RemoveUnuseStream.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_RemoveUnuseStream.Name = "CompressPDF_RemoveUnuseStream";
            this.CompressPDF_RemoveUnuseStream.Size = new System.Drawing.Size(132, 16);
            this.CompressPDF_RemoveUnuseStream.TabIndex = 11;
            this.CompressPDF_RemoveUnuseStream.Text = "移除未使用的文件流";
            this.CompressPDF_RemoveUnuseStream.UseVisualStyleBackColor = true;
            // 
            // CompressPDF_RemoveUnuseObject
            // 
            this.CompressPDF_RemoveUnuseObject.AutoSize = true;
            this.CompressPDF_RemoveUnuseObject.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_RemoveUnuseObject.Location = new System.Drawing.Point(491, 52);
            this.CompressPDF_RemoveUnuseObject.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_RemoveUnuseObject.Name = "CompressPDF_RemoveUnuseObject";
            this.CompressPDF_RemoveUnuseObject.Size = new System.Drawing.Size(108, 16);
            this.CompressPDF_RemoveUnuseObject.TabIndex = 10;
            this.CompressPDF_RemoveUnuseObject.Text = "移除未使用对象";
            this.CompressPDF_RemoveUnuseObject.UseVisualStyleBackColor = true;
            // 
            // CompressPDF_ImageCompressVersion
            // 
            this.CompressPDF_ImageCompressVersion.FormattingEnabled = true;
            this.CompressPDF_ImageCompressVersion.Location = new System.Drawing.Point(433, 15);
            this.CompressPDF_ImageCompressVersion.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_ImageCompressVersion.Name = "CompressPDF_ImageCompressVersion";
            this.CompressPDF_ImageCompressVersion.Size = new System.Drawing.Size(51, 20);
            this.CompressPDF_ImageCompressVersion.TabIndex = 9;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label46.Location = new System.Drawing.Point(377, 17);
            this.label46.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(53, 12);
            this.label46.TabIndex = 8;
            this.label46.Text = "压缩算法";
            // 
            // CompressPDF_ImageResoution
            // 
            this.CompressPDF_ImageResoution.FormattingEnabled = true;
            this.CompressPDF_ImageResoution.Location = new System.Drawing.Point(323, 15);
            this.CompressPDF_ImageResoution.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_ImageResoution.Name = "CompressPDF_ImageResoution";
            this.CompressPDF_ImageResoution.Size = new System.Drawing.Size(51, 20);
            this.CompressPDF_ImageResoution.TabIndex = 7;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label45.Location = new System.Drawing.Point(255, 17);
            this.label45.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(65, 12);
            this.label45.TabIndex = 6;
            this.label45.Text = "最大分辨率";
            // 
            // CompressPDF_ImageQuailty
            // 
            this.CompressPDF_ImageQuailty.FormattingEnabled = true;
            this.CompressPDF_ImageQuailty.Location = new System.Drawing.Point(205, 15);
            this.CompressPDF_ImageQuailty.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_ImageQuailty.Name = "CompressPDF_ImageQuailty";
            this.CompressPDF_ImageQuailty.Size = new System.Drawing.Size(47, 20);
            this.CompressPDF_ImageQuailty.TabIndex = 5;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label44.Location = new System.Drawing.Point(147, 19);
            this.label44.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(53, 12);
            this.label44.TabIndex = 4;
            this.label44.Text = "图像质量";
            // 
            // CompressPDF_CompressImage
            // 
            this.CompressPDF_CompressImage.AutoSize = true;
            this.CompressPDF_CompressImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_CompressImage.Location = new System.Drawing.Point(73, 17);
            this.CompressPDF_CompressImage.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_CompressImage.Name = "CompressPDF_CompressImage";
            this.CompressPDF_CompressImage.Size = new System.Drawing.Size(72, 16);
            this.CompressPDF_CompressImage.TabIndex = 1;
            this.CompressPDF_CompressImage.Text = "压缩图像";
            this.CompressPDF_CompressImage.UseVisualStyleBackColor = true;
            // 
            // CompressPDF_WEB
            // 
            this.CompressPDF_WEB.AutoSize = true;
            this.CompressPDF_WEB.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF_WEB.Location = new System.Drawing.Point(4, 17);
            this.CompressPDF_WEB.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF_WEB.Name = "CompressPDF_WEB";
            this.CompressPDF_WEB.Size = new System.Drawing.Size(66, 16);
            this.CompressPDF_WEB.TabIndex = 0;
            this.CompressPDF_WEB.Text = "WEB优化";
            this.CompressPDF_WEB.UseVisualStyleBackColor = true;
            // 
            // CompressPDF
            // 
            this.CompressPDF.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressPDF.Location = new System.Drawing.Point(697, 561);
            this.CompressPDF.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPDF.Name = "CompressPDF";
            this.CompressPDF.Size = new System.Drawing.Size(147, 37);
            this.CompressPDF.TabIndex = 2;
            this.CompressPDF.Tag = "anchor_right_bottom";
            this.CompressPDF.Text = "开始处理";
            this.CompressPDF.UseVisualStyleBackColor = true;
            this.CompressPDF.Click += new System.EventHandler(this.CompressPDF_Click);
            // 
            // SelectCompressPDFFiles
            // 
            this.SelectCompressPDFFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectCompressPDFFiles.Location = new System.Drawing.Point(5, 561);
            this.SelectCompressPDFFiles.Margin = new System.Windows.Forms.Padding(1);
            this.SelectCompressPDFFiles.Name = "SelectCompressPDFFiles";
            this.SelectCompressPDFFiles.Size = new System.Drawing.Size(147, 37);
            this.SelectCompressPDFFiles.TabIndex = 1;
            this.SelectCompressPDFFiles.Tag = "anchor_left_bottom";
            this.SelectCompressPDFFiles.Text = "选择文件";
            this.SelectCompressPDFFiles.UseVisualStyleBackColor = true;
            this.SelectCompressPDFFiles.Click += new System.EventHandler(this.SelectCompressPDFFiles_Click);
            // 
            // CompressPdfFileList
            // 
            this.CompressPdfFileList.HideSelection = false;
            this.CompressPdfFileList.Location = new System.Drawing.Point(7, 1);
            this.CompressPdfFileList.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPdfFileList.Name = "CompressPdfFileList";
            this.CompressPdfFileList.Size = new System.Drawing.Size(841, 420);
            this.CompressPdfFileList.TabIndex = 0;
            this.CompressPdfFileList.Tag = "anchor_full_fill";
            this.CompressPdfFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_25
            // 
            this.TabPage_25.Controls.Add(this.CompressImageFileList_MoveDown);
            this.TabPage_25.Controls.Add(this.CompressImageFileList_MoveUp);
            this.TabPage_25.Controls.Add(this.compressimage_clear);
            this.TabPage_25.Controls.Add(this.compressimage_remove);
            this.TabPage_25.Controls.Add(this.CompressImageStatusText);
            this.TabPage_25.Controls.Add(this.groupBox25);
            this.TabPage_25.Controls.Add(this.groupBox24);
            this.TabPage_25.Controls.Add(this.groupBox23);
            this.TabPage_25.Controls.Add(this.groupBox22);
            this.TabPage_25.Controls.Add(this.CompressImage);
            this.TabPage_25.Controls.Add(this.SelectCompressImageFile);
            this.TabPage_25.Controls.Add(this.CompressImageFileList);
            this.TabPage_25.Location = new System.Drawing.Point(4, 76);
            this.TabPage_25.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_25.Name = "TabPage_25";
            this.TabPage_25.Size = new System.Drawing.Size(882, 659);
            this.TabPage_25.TabIndex = 24;
            this.TabPage_25.Text = "压缩图片";
            this.TabPage_25.UseVisualStyleBackColor = true;
            // 
            // CompressImageFileList_MoveDown
            // 
            this.CompressImageFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressImageFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.CompressImageFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.CompressImageFileList_MoveDown.Name = "CompressImageFileList_MoveDown";
            this.CompressImageFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.CompressImageFileList_MoveDown.TabIndex = 11;
            this.CompressImageFileList_MoveDown.Tag = "anchor_right";
            this.CompressImageFileList_MoveDown.Text = "▽";
            this.CompressImageFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // CompressImageFileList_MoveUp
            // 
            this.CompressImageFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressImageFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.CompressImageFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.CompressImageFileList_MoveUp.Name = "CompressImageFileList_MoveUp";
            this.CompressImageFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.CompressImageFileList_MoveUp.TabIndex = 10;
            this.CompressImageFileList_MoveUp.Tag = "anchor_right";
            this.CompressImageFileList_MoveUp.Text = "△";
            this.CompressImageFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // compressimage_clear
            // 
            this.compressimage_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.compressimage_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.compressimage_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.compressimage_clear.Location = new System.Drawing.Point(847, 225);
            this.compressimage_clear.Margin = new System.Windows.Forms.Padding(1);
            this.compressimage_clear.Name = "compressimage_clear";
            this.compressimage_clear.Size = new System.Drawing.Size(23, 57);
            this.compressimage_clear.TabIndex = 12;
            this.compressimage_clear.Tag = "anchor_right";
            this.compressimage_clear.UseVisualStyleBackColor = true;
            // 
            // compressimage_remove
            // 
            this.compressimage_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.compressimage_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.compressimage_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.compressimage_remove.Location = new System.Drawing.Point(847, 145);
            this.compressimage_remove.Margin = new System.Windows.Forms.Padding(1);
            this.compressimage_remove.Name = "compressimage_remove";
            this.compressimage_remove.Size = new System.Drawing.Size(23, 57);
            this.compressimage_remove.TabIndex = 11;
            this.compressimage_remove.Tag = "anchor_right";
            this.compressimage_remove.UseVisualStyleBackColor = true;
            // 
            // CompressImageStatusText
            // 
            this.CompressImageStatusText.AutoSize = true;
            this.CompressImageStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.CompressImageStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressImageStatusText.Location = new System.Drawing.Point(156, 575);
            this.CompressImageStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.CompressImageStatusText.Name = "CompressImageStatusText";
            this.CompressImageStatusText.Size = new System.Drawing.Size(95, 12);
            this.CompressImageStatusText.TabIndex = 9;
            this.CompressImageStatusText.Tag = "anchor_left_bottom";
            this.CompressImageStatusText.Text = "正在处理X个文件";
            // 
            // groupBox25
            // 
            this.groupBox25.Controls.Add(this.CompressImage_NotUseSelfExt);
            this.groupBox25.Controls.Add(this.CompressImage_OutPutImageFormat);
            this.groupBox25.Controls.Add(this.label54);
            this.groupBox25.Location = new System.Drawing.Point(697, 431);
            this.groupBox25.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox25.Name = "groupBox25";
            this.groupBox25.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox25.Size = new System.Drawing.Size(147, 99);
            this.groupBox25.TabIndex = 8;
            this.groupBox25.TabStop = false;
            this.groupBox25.Tag = "anchor_left_bottom";
            this.groupBox25.Text = "保存参数设置";
            // 
            // CompressImage_NotUseSelfExt
            // 
            this.CompressImage_NotUseSelfExt.AutoSize = true;
            this.CompressImage_NotUseSelfExt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressImage_NotUseSelfExt.Location = new System.Drawing.Point(7, 29);
            this.CompressImage_NotUseSelfExt.Margin = new System.Windows.Forms.Padding(1);
            this.CompressImage_NotUseSelfExt.Name = "CompressImage_NotUseSelfExt";
            this.CompressImage_NotUseSelfExt.Size = new System.Drawing.Size(132, 16);
            this.CompressImage_NotUseSelfExt.TabIndex = 4;
            this.CompressImage_NotUseSelfExt.Text = "不使用源后缀名保存";
            this.CompressImage_NotUseSelfExt.UseVisualStyleBackColor = true;
            this.CompressImage_NotUseSelfExt.CheckedChanged += new System.EventHandler(this.CompressImage_NotUseSelfExt_CheckedChanged);
            // 
            // CompressImage_OutPutImageFormat
            // 
            this.CompressImage_OutPutImageFormat.FormattingEnabled = true;
            this.CompressImage_OutPutImageFormat.Location = new System.Drawing.Point(63, 69);
            this.CompressImage_OutPutImageFormat.Margin = new System.Windows.Forms.Padding(1);
            this.CompressImage_OutPutImageFormat.Name = "CompressImage_OutPutImageFormat";
            this.CompressImage_OutPutImageFormat.Size = new System.Drawing.Size(83, 20);
            this.CompressImage_OutPutImageFormat.TabIndex = 3;
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label54.Location = new System.Drawing.Point(5, 72);
            this.label54.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(53, 12);
            this.label54.TabIndex = 1;
            this.label54.Text = "保存格式";
            // 
            // groupBox24
            // 
            this.groupBox24.Controls.Add(this.CompressBmpPixeBits_Combox);
            this.groupBox24.Controls.Add(this.CompressBmpMode_Combox);
            this.groupBox24.Controls.Add(this.label50);
            this.groupBox24.Controls.Add(this.label53);
            this.groupBox24.Location = new System.Drawing.Point(455, 431);
            this.groupBox24.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox24.Name = "groupBox24";
            this.groupBox24.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox24.Size = new System.Drawing.Size(147, 99);
            this.groupBox24.TabIndex = 7;
            this.groupBox24.TabStop = false;
            this.groupBox24.Tag = "anchor_left_bottom";
            this.groupBox24.Text = "BMP参数设置";
            // 
            // CompressBmpPixeBits_Combox
            // 
            this.CompressBmpPixeBits_Combox.FormattingEnabled = true;
            this.CompressBmpPixeBits_Combox.Location = new System.Drawing.Point(63, 69);
            this.CompressBmpPixeBits_Combox.Margin = new System.Windows.Forms.Padding(1);
            this.CompressBmpPixeBits_Combox.Name = "CompressBmpPixeBits_Combox";
            this.CompressBmpPixeBits_Combox.Size = new System.Drawing.Size(83, 20);
            this.CompressBmpPixeBits_Combox.TabIndex = 3;
            // 
            // CompressBmpMode_Combox
            // 
            this.CompressBmpMode_Combox.FormattingEnabled = true;
            this.CompressBmpMode_Combox.Location = new System.Drawing.Point(63, 19);
            this.CompressBmpMode_Combox.Margin = new System.Windows.Forms.Padding(1);
            this.CompressBmpMode_Combox.Name = "CompressBmpMode_Combox";
            this.CompressBmpMode_Combox.Size = new System.Drawing.Size(83, 20);
            this.CompressBmpMode_Combox.TabIndex = 2;
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label50.Location = new System.Drawing.Point(5, 72);
            this.label50.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(53, 12);
            this.label50.TabIndex = 1;
            this.label50.Text = "色彩位数";
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label53.Location = new System.Drawing.Point(5, 21);
            this.label53.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(53, 12);
            this.label53.TabIndex = 0;
            this.label53.Text = "压缩模式";
            // 
            // groupBox23
            // 
            this.groupBox23.Controls.Add(this.CompressPngColorMode_Combox);
            this.groupBox23.Controls.Add(this.CompressPngLevel_Combox);
            this.groupBox23.Controls.Add(this.label51);
            this.groupBox23.Controls.Add(this.label52);
            this.groupBox23.Location = new System.Drawing.Point(220, 431);
            this.groupBox23.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox23.Name = "groupBox23";
            this.groupBox23.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox23.Size = new System.Drawing.Size(147, 99);
            this.groupBox23.TabIndex = 6;
            this.groupBox23.TabStop = false;
            this.groupBox23.Tag = "anchor_left_bottom";
            this.groupBox23.Text = "PNG参数设置";
            // 
            // CompressPngColorMode_Combox
            // 
            this.CompressPngColorMode_Combox.FormattingEnabled = true;
            this.CompressPngColorMode_Combox.Location = new System.Drawing.Point(63, 69);
            this.CompressPngColorMode_Combox.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPngColorMode_Combox.Name = "CompressPngColorMode_Combox";
            this.CompressPngColorMode_Combox.Size = new System.Drawing.Size(83, 20);
            this.CompressPngColorMode_Combox.TabIndex = 3;
            // 
            // CompressPngLevel_Combox
            // 
            this.CompressPngLevel_Combox.FormattingEnabled = true;
            this.CompressPngLevel_Combox.Location = new System.Drawing.Point(63, 19);
            this.CompressPngLevel_Combox.Margin = new System.Windows.Forms.Padding(1);
            this.CompressPngLevel_Combox.Name = "CompressPngLevel_Combox";
            this.CompressPngLevel_Combox.Size = new System.Drawing.Size(83, 20);
            this.CompressPngLevel_Combox.TabIndex = 2;
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label51.Location = new System.Drawing.Point(5, 72);
            this.label51.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(53, 12);
            this.label51.TabIndex = 1;
            this.label51.Text = "颜色模式";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label52.Location = new System.Drawing.Point(5, 21);
            this.label52.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(53, 12);
            this.label52.TabIndex = 0;
            this.label52.Text = "压缩级别";
            // 
            // groupBox22
            // 
            this.groupBox22.Controls.Add(this.CompressJpegQuailty_Combox);
            this.groupBox22.Controls.Add(this.label49);
            this.groupBox22.Controls.Add(this.CompressJpegColorMode_Combox);
            this.groupBox22.Controls.Add(this.CompressJpegMode_Combox);
            this.groupBox22.Controls.Add(this.label48);
            this.groupBox22.Controls.Add(this.label47);
            this.groupBox22.Location = new System.Drawing.Point(4, 431);
            this.groupBox22.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox22.Name = "groupBox22";
            this.groupBox22.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox22.Size = new System.Drawing.Size(147, 99);
            this.groupBox22.TabIndex = 3;
            this.groupBox22.TabStop = false;
            this.groupBox22.Tag = "anchor_left_bottom";
            this.groupBox22.Text = "JPG/JPEG参数设置";
            // 
            // CompressJpegQuailty_Combox
            // 
            this.CompressJpegQuailty_Combox.FormattingEnabled = true;
            this.CompressJpegQuailty_Combox.Location = new System.Drawing.Point(63, 71);
            this.CompressJpegQuailty_Combox.Margin = new System.Windows.Forms.Padding(1);
            this.CompressJpegQuailty_Combox.Name = "CompressJpegQuailty_Combox";
            this.CompressJpegQuailty_Combox.Size = new System.Drawing.Size(83, 20);
            this.CompressJpegQuailty_Combox.TabIndex = 5;
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label49.Location = new System.Drawing.Point(5, 73);
            this.label49.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(53, 12);
            this.label49.TabIndex = 4;
            this.label49.Text = "图像质量";
            // 
            // CompressJpegColorMode_Combox
            // 
            this.CompressJpegColorMode_Combox.FormattingEnabled = true;
            this.CompressJpegColorMode_Combox.Location = new System.Drawing.Point(63, 43);
            this.CompressJpegColorMode_Combox.Margin = new System.Windows.Forms.Padding(1);
            this.CompressJpegColorMode_Combox.Name = "CompressJpegColorMode_Combox";
            this.CompressJpegColorMode_Combox.Size = new System.Drawing.Size(83, 20);
            this.CompressJpegColorMode_Combox.TabIndex = 3;
            // 
            // CompressJpegMode_Combox
            // 
            this.CompressJpegMode_Combox.FormattingEnabled = true;
            this.CompressJpegMode_Combox.Location = new System.Drawing.Point(63, 19);
            this.CompressJpegMode_Combox.Margin = new System.Windows.Forms.Padding(1);
            this.CompressJpegMode_Combox.Name = "CompressJpegMode_Combox";
            this.CompressJpegMode_Combox.Size = new System.Drawing.Size(83, 20);
            this.CompressJpegMode_Combox.TabIndex = 2;
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label48.Location = new System.Drawing.Point(5, 47);
            this.label48.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(53, 12);
            this.label48.TabIndex = 1;
            this.label48.Text = "颜色模式";
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label47.Location = new System.Drawing.Point(5, 21);
            this.label47.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(53, 12);
            this.label47.TabIndex = 0;
            this.label47.Text = "压缩模式";
            // 
            // CompressImage
            // 
            this.CompressImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CompressImage.Location = new System.Drawing.Point(697, 561);
            this.CompressImage.Margin = new System.Windows.Forms.Padding(1);
            this.CompressImage.Name = "CompressImage";
            this.CompressImage.Size = new System.Drawing.Size(147, 37);
            this.CompressImage.TabIndex = 2;
            this.CompressImage.Tag = "anchor_right_bottom";
            this.CompressImage.Text = "开始处理";
            this.CompressImage.UseVisualStyleBackColor = true;
            this.CompressImage.Click += new System.EventHandler(this.CompressImage_Click);
            // 
            // SelectCompressImageFile
            // 
            this.SelectCompressImageFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectCompressImageFile.Location = new System.Drawing.Point(5, 561);
            this.SelectCompressImageFile.Margin = new System.Windows.Forms.Padding(1);
            this.SelectCompressImageFile.Name = "SelectCompressImageFile";
            this.SelectCompressImageFile.Size = new System.Drawing.Size(147, 37);
            this.SelectCompressImageFile.TabIndex = 1;
            this.SelectCompressImageFile.Tag = "anchor_left_bottom";
            this.SelectCompressImageFile.Text = "选择文件";
            this.SelectCompressImageFile.UseVisualStyleBackColor = true;
            this.SelectCompressImageFile.Click += new System.EventHandler(this.SelectCompressImageFile_Click);
            // 
            // CompressImageFileList
            // 
            this.CompressImageFileList.HideSelection = false;
            this.CompressImageFileList.Location = new System.Drawing.Point(4, 4);
            this.CompressImageFileList.Margin = new System.Windows.Forms.Padding(1);
            this.CompressImageFileList.Name = "CompressImageFileList";
            this.CompressImageFileList.Size = new System.Drawing.Size(841, 420);
            this.CompressImageFileList.TabIndex = 0;
            this.CompressImageFileList.Tag = "anchor_full_fill";
            this.CompressImageFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_10
            // 
            this.TabPage_10.Controls.Add(this.UseNewStyle);
            this.TabPage_10.Controls.Add(this.WordReplaceFontSeeting);
            this.TabPage_10.Controls.Add(this.ReplaceWordFileList_MoveDown);
            this.TabPage_10.Controls.Add(this.ReplaceWordFileList_MoveUp);
            this.TabPage_10.Controls.Add(this.wordreplace_clear);
            this.TabPage_10.Controls.Add(this.wordreplace_remove);
            this.TabPage_10.Controls.Add(this.TurialRegex);
            this.TabPage_10.Controls.Add(this.OnlineRegextTool);
            this.TabPage_10.Controls.Add(this.groupBox10);
            this.TabPage_10.Controls.Add(this.groupBox9);
            this.TabPage_10.Controls.Add(this.IgnoreFootContent);
            this.TabPage_10.Controls.Add(this.IgnoreCase);
            this.TabPage_10.Controls.Add(this.ReplaceWordContentStatusText);
            this.TabPage_10.Controls.Add(this.ReplaceWordContentButton);
            this.TabPage_10.Controls.Add(this.SelectReplaceWordFilesButton);
            this.TabPage_10.Controls.Add(this.ReplaceWordFileList);
            this.TabPage_10.Location = new System.Drawing.Point(4, 76);
            this.TabPage_10.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_10.Name = "TabPage_10";
            this.TabPage_10.Size = new System.Drawing.Size(882, 659);
            this.TabPage_10.TabIndex = 9;
            this.TabPage_10.Text = "WORD内容批量替换";
            this.TabPage_10.UseVisualStyleBackColor = true;
            // 
            // UseNewStyle
            // 
            this.UseNewStyle.AutoSize = true;
            this.UseNewStyle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.UseNewStyle.Location = new System.Drawing.Point(301, 457);
            this.UseNewStyle.Margin = new System.Windows.Forms.Padding(1);
            this.UseNewStyle.Name = "UseNewStyle";
            this.UseNewStyle.Size = new System.Drawing.Size(144, 16);
            this.UseNewStyle.TabIndex = 14;
            this.UseNewStyle.Tag = "anchor_left_bottom";
            this.UseNewStyle.Text = "更改替换的字体和大小";
            this.UseNewStyle.UseVisualStyleBackColor = true;
            // 
            // WordReplaceFontSeeting
            // 
            this.WordReplaceFontSeeting.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.WordReplaceFontSeeting.Location = new System.Drawing.Point(448, 455);
            this.WordReplaceFontSeeting.Margin = new System.Windows.Forms.Padding(1);
            this.WordReplaceFontSeeting.Name = "WordReplaceFontSeeting";
            this.WordReplaceFontSeeting.Size = new System.Drawing.Size(141, 21);
            this.WordReplaceFontSeeting.TabIndex = 13;
            this.WordReplaceFontSeeting.Tag = "anchor_left_bottom";
            this.WordReplaceFontSeeting.Text = "设置字体和大小...";
            this.WordReplaceFontSeeting.UseVisualStyleBackColor = true;
            this.WordReplaceFontSeeting.Click += new System.EventHandler(this.WordReplaceFontSeeting_Click);
            // 
            // ReplaceWordFileList_MoveDown
            // 
            this.ReplaceWordFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceWordFileList_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.ReplaceWordFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceWordFileList_MoveDown.Name = "ReplaceWordFileList_MoveDown";
            this.ReplaceWordFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.ReplaceWordFileList_MoveDown.TabIndex = 12;
            this.ReplaceWordFileList_MoveDown.Tag = "anchor_right";
            this.ReplaceWordFileList_MoveDown.Text = "▽";
            this.ReplaceWordFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // ReplaceWordFileList_MoveUp
            // 
            this.ReplaceWordFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceWordFileList_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.ReplaceWordFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceWordFileList_MoveUp.Name = "ReplaceWordFileList_MoveUp";
            this.ReplaceWordFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.ReplaceWordFileList_MoveUp.TabIndex = 11;
            this.ReplaceWordFileList_MoveUp.Tag = "anchor_right";
            this.ReplaceWordFileList_MoveUp.Text = "△";
            this.ReplaceWordFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // wordreplace_clear
            // 
            this.wordreplace_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.wordreplace_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.wordreplace_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.wordreplace_clear.Location = new System.Drawing.Point(847, 225);
            this.wordreplace_clear.Margin = new System.Windows.Forms.Padding(1);
            this.wordreplace_clear.Name = "wordreplace_clear";
            this.wordreplace_clear.Size = new System.Drawing.Size(23, 57);
            this.wordreplace_clear.TabIndex = 12;
            this.wordreplace_clear.Tag = "anchor_right";
            this.wordreplace_clear.UseVisualStyleBackColor = true;
            // 
            // wordreplace_remove
            // 
            this.wordreplace_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.wordreplace_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.wordreplace_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.wordreplace_remove.Location = new System.Drawing.Point(847, 145);
            this.wordreplace_remove.Margin = new System.Windows.Forms.Padding(1);
            this.wordreplace_remove.Name = "wordreplace_remove";
            this.wordreplace_remove.Size = new System.Drawing.Size(23, 57);
            this.wordreplace_remove.TabIndex = 11;
            this.wordreplace_remove.Tag = "anchor_right";
            this.wordreplace_remove.UseVisualStyleBackColor = true;
            // 
            // TurialRegex
            // 
            this.TurialRegex.AutoSize = true;
            this.TurialRegex.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.TurialRegex.Location = new System.Drawing.Point(753, 431);
            this.TurialRegex.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.TurialRegex.Name = "TurialRegex";
            this.TurialRegex.Size = new System.Drawing.Size(89, 12);
            this.TurialRegex.TabIndex = 10;
            this.TurialRegex.TabStop = true;
            this.TurialRegex.Tag = "anchor_right_bottom";
            this.TurialRegex.Text = "正则表达式教程";
            this.TurialRegex.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.TurialRegex_LinkClicked);
            // 
            // OnlineRegextTool
            // 
            this.OnlineRegextTool.AutoSize = true;
            this.OnlineRegextTool.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.OnlineRegextTool.Location = new System.Drawing.Point(673, 431);
            this.OnlineRegextTool.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.OnlineRegextTool.Name = "OnlineRegextTool";
            this.OnlineRegextTool.Size = new System.Drawing.Size(77, 12);
            this.OnlineRegextTool.TabIndex = 9;
            this.OnlineRegextTool.TabStop = true;
            this.OnlineRegextTool.Tag = "anchor_right_bottom";
            this.OnlineRegextTool.Text = "在线正则工具";
            this.OnlineRegextTool.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnlineRegextTool_LinkClicked);
            // 
            // groupBox10
            // 
            this.groupBox10.Controls.Add(this.WordReplaceTextBox);
            this.groupBox10.Location = new System.Drawing.Point(148, 431);
            this.groupBox10.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox10.Name = "groupBox10";
            this.groupBox10.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox10.Size = new System.Drawing.Size(141, 47);
            this.groupBox10.TabIndex = 8;
            this.groupBox10.TabStop = false;
            this.groupBox10.Tag = "anchor_left_bottom";
            this.groupBox10.Text = "替换内容";
            // 
            // WordReplaceTextBox
            // 
            this.WordReplaceTextBox.Location = new System.Drawing.Point(4, 20);
            this.WordReplaceTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.WordReplaceTextBox.Name = "WordReplaceTextBox";
            this.WordReplaceTextBox.Size = new System.Drawing.Size(133, 21);
            this.WordReplaceTextBox.TabIndex = 0;
            // 
            // groupBox9
            // 
            this.groupBox9.Controls.Add(this.WordReplaceFindTextBox);
            this.groupBox9.Location = new System.Drawing.Point(4, 431);
            this.groupBox9.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox9.Size = new System.Drawing.Size(141, 47);
            this.groupBox9.TabIndex = 7;
            this.groupBox9.TabStop = false;
            this.groupBox9.Tag = "anchor_left_bottom";
            this.groupBox9.Text = "查找内容";
            // 
            // WordReplaceFindTextBox
            // 
            this.WordReplaceFindTextBox.Location = new System.Drawing.Point(4, 20);
            this.WordReplaceFindTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.WordReplaceFindTextBox.Name = "WordReplaceFindTextBox";
            this.WordReplaceFindTextBox.Size = new System.Drawing.Size(133, 21);
            this.WordReplaceFindTextBox.TabIndex = 0;
            // 
            // IgnoreFootContent
            // 
            this.IgnoreFootContent.AutoSize = true;
            this.IgnoreFootContent.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.IgnoreFootContent.Location = new System.Drawing.Point(301, 431);
            this.IgnoreFootContent.Margin = new System.Windows.Forms.Padding(1);
            this.IgnoreFootContent.Name = "IgnoreFootContent";
            this.IgnoreFootContent.Size = new System.Drawing.Size(96, 16);
            this.IgnoreFootContent.TabIndex = 6;
            this.IgnoreFootContent.Tag = "anchor_left_bottom";
            this.IgnoreFootContent.Text = "忽略脚注文本";
            this.IgnoreFootContent.UseVisualStyleBackColor = true;
            // 
            // IgnoreCase
            // 
            this.IgnoreCase.AutoSize = true;
            this.IgnoreCase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.IgnoreCase.Location = new System.Drawing.Point(399, 431);
            this.IgnoreCase.Margin = new System.Windows.Forms.Padding(1);
            this.IgnoreCase.Name = "IgnoreCase";
            this.IgnoreCase.Size = new System.Drawing.Size(84, 16);
            this.IgnoreCase.TabIndex = 5;
            this.IgnoreCase.Tag = "anchor_left_bottom";
            this.IgnoreCase.Text = "区分大小写";
            this.IgnoreCase.UseVisualStyleBackColor = true;
            // 
            // ReplaceWordContentStatusText
            // 
            this.ReplaceWordContentStatusText.AutoSize = true;
            this.ReplaceWordContentStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ReplaceWordContentStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceWordContentStatusText.Location = new System.Drawing.Point(156, 575);
            this.ReplaceWordContentStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ReplaceWordContentStatusText.Name = "ReplaceWordContentStatusText";
            this.ReplaceWordContentStatusText.Size = new System.Drawing.Size(95, 12);
            this.ReplaceWordContentStatusText.TabIndex = 3;
            this.ReplaceWordContentStatusText.Tag = "anchor_left_bottom";
            this.ReplaceWordContentStatusText.Text = "正在处理X个文件";
            // 
            // ReplaceWordContentButton
            // 
            this.ReplaceWordContentButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceWordContentButton.Location = new System.Drawing.Point(697, 561);
            this.ReplaceWordContentButton.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceWordContentButton.Name = "ReplaceWordContentButton";
            this.ReplaceWordContentButton.Size = new System.Drawing.Size(147, 37);
            this.ReplaceWordContentButton.TabIndex = 2;
            this.ReplaceWordContentButton.Tag = "anchor_right_bottom";
            this.ReplaceWordContentButton.Text = "开始处理";
            this.ReplaceWordContentButton.UseVisualStyleBackColor = true;
            this.ReplaceWordContentButton.Click += new System.EventHandler(this.ReplaceWordContentButton_Click);
            // 
            // SelectReplaceWordFilesButton
            // 
            this.SelectReplaceWordFilesButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectReplaceWordFilesButton.Location = new System.Drawing.Point(5, 561);
            this.SelectReplaceWordFilesButton.Margin = new System.Windows.Forms.Padding(1);
            this.SelectReplaceWordFilesButton.Name = "SelectReplaceWordFilesButton";
            this.SelectReplaceWordFilesButton.Size = new System.Drawing.Size(147, 37);
            this.SelectReplaceWordFilesButton.TabIndex = 1;
            this.SelectReplaceWordFilesButton.Tag = "anchor_left_bottom";
            this.SelectReplaceWordFilesButton.Text = "选择文件";
            this.SelectReplaceWordFilesButton.UseVisualStyleBackColor = true;
            this.SelectReplaceWordFilesButton.Click += new System.EventHandler(this.SelectReplaceWordFilesButton_Click);
            // 
            // ReplaceWordFileList
            // 
            this.ReplaceWordFileList.HideSelection = false;
            this.ReplaceWordFileList.Location = new System.Drawing.Point(4, 4);
            this.ReplaceWordFileList.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceWordFileList.Name = "ReplaceWordFileList";
            this.ReplaceWordFileList.Size = new System.Drawing.Size(841, 420);
            this.ReplaceWordFileList.TabIndex = 0;
            this.ReplaceWordFileList.Tag = "anchor_full_fill";
            this.ReplaceWordFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_11
            // 
            this.TabPage_11.BackColor = System.Drawing.Color.Transparent;
            this.TabPage_11.Controls.Add(this.ReplaceExcelFileView_MoveDown);
            this.TabPage_11.Controls.Add(this.ReplaceExcelFileView_MoveUp);
            this.TabPage_11.Controls.Add(this.excelreplace_clear);
            this.TabPage_11.Controls.Add(this.excelreplace_remove);
            this.TabPage_11.Controls.Add(this.linkLabel1);
            this.TabPage_11.Controls.Add(this.ExcelRegexReadMe);
            this.TabPage_11.Controls.Add(this.UseRegex);
            this.TabPage_11.Controls.Add(this.ExcelNoIgnoreCase);
            this.TabPage_11.Controls.Add(this.groupBox13);
            this.TabPage_11.Controls.Add(this.groupBox12);
            this.TabPage_11.Controls.Add(this.groupBox11);
            this.TabPage_11.Controls.Add(this.ReplaceExcelStatusText);
            this.TabPage_11.Controls.Add(this.ReplaceExcelContentButton);
            this.TabPage_11.Controls.Add(this.SelectRelaceExcelButton);
            this.TabPage_11.Controls.Add(this.ReplaceExcelFileView);
            this.TabPage_11.Location = new System.Drawing.Point(4, 76);
            this.TabPage_11.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_11.Name = "TabPage_11";
            this.TabPage_11.Size = new System.Drawing.Size(882, 659);
            this.TabPage_11.TabIndex = 10;
            this.TabPage_11.Text = "EXCEL内容批量替换";
            this.TabPage_11.UseVisualStyleBackColor = true;
            // 
            // ReplaceExcelFileView_MoveDown
            // 
            this.ReplaceExcelFileView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceExcelFileView_MoveDown.Location = new System.Drawing.Point(847, 307);
            this.ReplaceExcelFileView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceExcelFileView_MoveDown.Name = "ReplaceExcelFileView_MoveDown";
            this.ReplaceExcelFileView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.ReplaceExcelFileView_MoveDown.TabIndex = 11;
            this.ReplaceExcelFileView_MoveDown.Tag = "anchor_right";
            this.ReplaceExcelFileView_MoveDown.Text = "▽";
            this.ReplaceExcelFileView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // ReplaceExcelFileView_MoveUp
            // 
            this.ReplaceExcelFileView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceExcelFileView_MoveUp.Location = new System.Drawing.Point(847, 65);
            this.ReplaceExcelFileView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceExcelFileView_MoveUp.Name = "ReplaceExcelFileView_MoveUp";
            this.ReplaceExcelFileView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.ReplaceExcelFileView_MoveUp.TabIndex = 10;
            this.ReplaceExcelFileView_MoveUp.Tag = "anchor_right";
            this.ReplaceExcelFileView_MoveUp.Text = "△";
            this.ReplaceExcelFileView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // excelreplace_clear
            // 
            this.excelreplace_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.excelreplace_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.excelreplace_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.excelreplace_clear.Location = new System.Drawing.Point(847, 225);
            this.excelreplace_clear.Margin = new System.Windows.Forms.Padding(1);
            this.excelreplace_clear.Name = "excelreplace_clear";
            this.excelreplace_clear.Size = new System.Drawing.Size(23, 57);
            this.excelreplace_clear.TabIndex = 12;
            this.excelreplace_clear.Tag = "anchor_right";
            this.excelreplace_clear.UseVisualStyleBackColor = true;
            // 
            // excelreplace_remove
            // 
            this.excelreplace_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.excelreplace_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.excelreplace_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.excelreplace_remove.Location = new System.Drawing.Point(847, 145);
            this.excelreplace_remove.Margin = new System.Windows.Forms.Padding(1);
            this.excelreplace_remove.Name = "excelreplace_remove";
            this.excelreplace_remove.Size = new System.Drawing.Size(23, 57);
            this.excelreplace_remove.TabIndex = 11;
            this.excelreplace_remove.Tag = "anchor_right";
            this.excelreplace_remove.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabel1.Location = new System.Drawing.Point(764, 431);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(77, 12);
            this.linkLabel1.TabIndex = 9;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Tag = "anchor_right_bottom";
            this.linkLabel1.Text = "在线正则工具";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel1_LinkClicked);
            // 
            // ExcelRegexReadMe
            // 
            this.ExcelRegexReadMe.AutoSize = true;
            this.ExcelRegexReadMe.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExcelRegexReadMe.Location = new System.Drawing.Point(671, 431);
            this.ExcelRegexReadMe.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ExcelRegexReadMe.Name = "ExcelRegexReadMe";
            this.ExcelRegexReadMe.Size = new System.Drawing.Size(89, 12);
            this.ExcelRegexReadMe.TabIndex = 8;
            this.ExcelRegexReadMe.TabStop = true;
            this.ExcelRegexReadMe.Tag = "anchor_right_bottom";
            this.ExcelRegexReadMe.Text = "正则表达式教程";
            this.ExcelRegexReadMe.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ExcelRegexReadMe_LinkClicked);
            // 
            // UseRegex
            // 
            this.UseRegex.AutoSize = true;
            this.UseRegex.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.UseRegex.Location = new System.Drawing.Point(279, 457);
            this.UseRegex.Margin = new System.Windows.Forms.Padding(1);
            this.UseRegex.Name = "UseRegex";
            this.UseRegex.Size = new System.Drawing.Size(108, 16);
            this.UseRegex.TabIndex = 7;
            this.UseRegex.Tag = "anchor_left_bottom";
            this.UseRegex.Text = "使用正则表达式";
            this.UseRegex.UseVisualStyleBackColor = true;
            // 
            // ExcelNoIgnoreCase
            // 
            this.ExcelNoIgnoreCase.AutoSize = true;
            this.ExcelNoIgnoreCase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExcelNoIgnoreCase.Location = new System.Drawing.Point(279, 431);
            this.ExcelNoIgnoreCase.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelNoIgnoreCase.Name = "ExcelNoIgnoreCase";
            this.ExcelNoIgnoreCase.Size = new System.Drawing.Size(84, 16);
            this.ExcelNoIgnoreCase.TabIndex = 6;
            this.ExcelNoIgnoreCase.Tag = "anchor_left_bottom";
            this.ExcelNoIgnoreCase.Text = "区分大小写";
            this.ExcelNoIgnoreCase.UseVisualStyleBackColor = true;
            // 
            // groupBox13
            // 
            this.groupBox13.Controls.Add(this.ExcelStartRow);
            this.groupBox13.Controls.Add(this.ExcelEndRow);
            this.groupBox13.Controls.Add(this.label19);
            this.groupBox13.Controls.Add(this.label18);
            this.groupBox13.Controls.Add(this.RemoveRows);
            this.groupBox13.Location = new System.Drawing.Point(389, 431);
            this.groupBox13.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox13.Name = "groupBox13";
            this.groupBox13.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox13.Size = new System.Drawing.Size(235, 55);
            this.groupBox13.TabIndex = 6;
            this.groupBox13.TabStop = false;
            this.groupBox13.Tag = "anchor_left_bottom";
            this.groupBox13.Text = "清空行数据";
            // 
            // ExcelStartRow
            // 
            this.ExcelStartRow.Location = new System.Drawing.Point(101, 21);
            this.ExcelStartRow.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelStartRow.Name = "ExcelStartRow";
            this.ExcelStartRow.Size = new System.Drawing.Size(55, 21);
            this.ExcelStartRow.TabIndex = 5;
            this.ExcelStartRow.Text = "-1";
            // 
            // ExcelEndRow
            // 
            this.ExcelEndRow.Location = new System.Drawing.Point(177, 23);
            this.ExcelEndRow.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelEndRow.Name = "ExcelEndRow";
            this.ExcelEndRow.Size = new System.Drawing.Size(55, 21);
            this.ExcelEndRow.TabIndex = 4;
            this.ExcelEndRow.Text = "-1";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label19.Location = new System.Drawing.Point(156, 25);
            this.label19.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(17, 12);
            this.label19.TabIndex = 3;
            this.label19.Text = "到";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label18.Location = new System.Drawing.Point(79, 25);
            this.label18.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(17, 12);
            this.label18.TabIndex = 1;
            this.label18.Text = "从";
            // 
            // RemoveRows
            // 
            this.RemoveRows.AutoSize = true;
            this.RemoveRows.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemoveRows.Location = new System.Drawing.Point(4, 25);
            this.RemoveRows.Margin = new System.Windows.Forms.Padding(1);
            this.RemoveRows.Name = "RemoveRows";
            this.RemoveRows.Size = new System.Drawing.Size(60, 16);
            this.RemoveRows.TabIndex = 0;
            this.RemoveRows.Text = "删除行";
            this.RemoveRows.UseVisualStyleBackColor = true;
            // 
            // groupBox12
            // 
            this.groupBox12.Controls.Add(this.ExcelReplaceBox);
            this.groupBox12.Location = new System.Drawing.Point(141, 431);
            this.groupBox12.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox12.Name = "groupBox12";
            this.groupBox12.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox12.Size = new System.Drawing.Size(133, 55);
            this.groupBox12.TabIndex = 5;
            this.groupBox12.TabStop = false;
            this.groupBox12.Tag = "anchor_left_bottom";
            this.groupBox12.Text = "替换内容";
            // 
            // ExcelReplaceBox
            // 
            this.ExcelReplaceBox.Location = new System.Drawing.Point(4, 24);
            this.ExcelReplaceBox.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelReplaceBox.Name = "ExcelReplaceBox";
            this.ExcelReplaceBox.Size = new System.Drawing.Size(127, 21);
            this.ExcelReplaceBox.TabIndex = 0;
            // 
            // groupBox11
            // 
            this.groupBox11.Controls.Add(this.ExcelFindTextBox);
            this.groupBox11.Location = new System.Drawing.Point(4, 431);
            this.groupBox11.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox11.Size = new System.Drawing.Size(133, 55);
            this.groupBox11.TabIndex = 4;
            this.groupBox11.TabStop = false;
            this.groupBox11.Tag = "anchor_left_bottom";
            this.groupBox11.Text = "查找内容";
            // 
            // ExcelFindTextBox
            // 
            this.ExcelFindTextBox.Location = new System.Drawing.Point(4, 24);
            this.ExcelFindTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.ExcelFindTextBox.Name = "ExcelFindTextBox";
            this.ExcelFindTextBox.Size = new System.Drawing.Size(127, 21);
            this.ExcelFindTextBox.TabIndex = 0;
            // 
            // ReplaceExcelStatusText
            // 
            this.ReplaceExcelStatusText.AutoSize = true;
            this.ReplaceExcelStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.ReplaceExcelStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceExcelStatusText.Location = new System.Drawing.Point(156, 575);
            this.ReplaceExcelStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ReplaceExcelStatusText.Name = "ReplaceExcelStatusText";
            this.ReplaceExcelStatusText.Size = new System.Drawing.Size(95, 12);
            this.ReplaceExcelStatusText.TabIndex = 3;
            this.ReplaceExcelStatusText.Tag = "anchor_left_bottom";
            this.ReplaceExcelStatusText.Text = "正在处理X个文件";
            // 
            // ReplaceExcelContentButton
            // 
            this.ReplaceExcelContentButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReplaceExcelContentButton.Location = new System.Drawing.Point(697, 561);
            this.ReplaceExcelContentButton.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceExcelContentButton.Name = "ReplaceExcelContentButton";
            this.ReplaceExcelContentButton.Size = new System.Drawing.Size(147, 37);
            this.ReplaceExcelContentButton.TabIndex = 2;
            this.ReplaceExcelContentButton.Tag = "anchor_right_bottom";
            this.ReplaceExcelContentButton.Text = "开始处理";
            this.ReplaceExcelContentButton.UseVisualStyleBackColor = true;
            this.ReplaceExcelContentButton.Click += new System.EventHandler(this.ReplaceExcelContentButton_Click);
            // 
            // SelectRelaceExcelButton
            // 
            this.SelectRelaceExcelButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectRelaceExcelButton.Location = new System.Drawing.Point(5, 561);
            this.SelectRelaceExcelButton.Margin = new System.Windows.Forms.Padding(1);
            this.SelectRelaceExcelButton.Name = "SelectRelaceExcelButton";
            this.SelectRelaceExcelButton.Size = new System.Drawing.Size(147, 37);
            this.SelectRelaceExcelButton.TabIndex = 1;
            this.SelectRelaceExcelButton.Tag = "anchor_left_bottom";
            this.SelectRelaceExcelButton.Text = "选择文件";
            this.SelectRelaceExcelButton.UseVisualStyleBackColor = true;
            this.SelectRelaceExcelButton.Click += new System.EventHandler(this.SelectRelaceExcelButton_Click);
            // 
            // ReplaceExcelFileView
            // 
            this.ReplaceExcelFileView.HideSelection = false;
            this.ReplaceExcelFileView.Location = new System.Drawing.Point(4, 4);
            this.ReplaceExcelFileView.Margin = new System.Windows.Forms.Padding(1);
            this.ReplaceExcelFileView.Name = "ReplaceExcelFileView";
            this.ReplaceExcelFileView.Size = new System.Drawing.Size(841, 420);
            this.ReplaceExcelFileView.TabIndex = 0;
            this.ReplaceExcelFileView.Tag = "anchor_full_fill";
            this.ReplaceExcelFileView.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_12
            // 
            this.TabPage_12.Controls.Add(this.DecodeMatrixWordListView_MoveDown);
            this.TabPage_12.Controls.Add(this.DecodeMatrixWordListView_MoveUp);
            this.TabPage_12.Controls.Add(this.datamatrix_clear);
            this.TabPage_12.Controls.Add(this.datamatrix_remove);
            this.TabPage_12.Controls.Add(this.DecodeDataMatixStatusText);
            this.TabPage_12.Controls.Add(this.DecodeDataMarix);
            this.TabPage_12.Controls.Add(this.SelectDataMatrixWord);
            this.TabPage_12.Controls.Add(this.groupBox14);
            this.TabPage_12.Controls.Add(this.DecodeMatrixWordListView);
            this.TabPage_12.Location = new System.Drawing.Point(4, 76);
            this.TabPage_12.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_12.Name = "TabPage_12";
            this.TabPage_12.Size = new System.Drawing.Size(882, 659);
            this.TabPage_12.TabIndex = 11;
            this.TabPage_12.Text = "特殊二维码校验";
            this.TabPage_12.UseVisualStyleBackColor = true;
            this.TabPage_12.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TabPage_12_MouseDown);
            // 
            // DecodeMatrixWordListView_MoveDown
            // 
            this.DecodeMatrixWordListView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DecodeMatrixWordListView_MoveDown.Location = new System.Drawing.Point(859, 180);
            this.DecodeMatrixWordListView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.DecodeMatrixWordListView_MoveDown.Name = "DecodeMatrixWordListView_MoveDown";
            this.DecodeMatrixWordListView_MoveDown.Size = new System.Drawing.Size(21, 28);
            this.DecodeMatrixWordListView_MoveDown.TabIndex = 6;
            this.DecodeMatrixWordListView_MoveDown.Tag = "anchor_right_top";
            this.DecodeMatrixWordListView_MoveDown.Text = "▽";
            this.DecodeMatrixWordListView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // DecodeMatrixWordListView_MoveUp
            // 
            this.DecodeMatrixWordListView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DecodeMatrixWordListView_MoveUp.Location = new System.Drawing.Point(859, 48);
            this.DecodeMatrixWordListView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.DecodeMatrixWordListView_MoveUp.Name = "DecodeMatrixWordListView_MoveUp";
            this.DecodeMatrixWordListView_MoveUp.Size = new System.Drawing.Size(21, 28);
            this.DecodeMatrixWordListView_MoveUp.TabIndex = 5;
            this.DecodeMatrixWordListView_MoveUp.Tag = "anchor_right_top";
            this.DecodeMatrixWordListView_MoveUp.Text = "△";
            this.DecodeMatrixWordListView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // datamatrix_clear
            // 
            this.datamatrix_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.datamatrix_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.datamatrix_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.datamatrix_clear.Location = new System.Drawing.Point(859, 136);
            this.datamatrix_clear.Margin = new System.Windows.Forms.Padding(1);
            this.datamatrix_clear.Name = "datamatrix_clear";
            this.datamatrix_clear.Size = new System.Drawing.Size(21, 28);
            this.datamatrix_clear.TabIndex = 12;
            this.datamatrix_clear.Tag = "anchor_right_top";
            this.datamatrix_clear.UseVisualStyleBackColor = true;
            // 
            // datamatrix_remove
            // 
            this.datamatrix_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.datamatrix_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.datamatrix_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.datamatrix_remove.Location = new System.Drawing.Point(859, 92);
            this.datamatrix_remove.Margin = new System.Windows.Forms.Padding(1);
            this.datamatrix_remove.Name = "datamatrix_remove";
            this.datamatrix_remove.Size = new System.Drawing.Size(21, 28);
            this.datamatrix_remove.TabIndex = 11;
            this.datamatrix_remove.Tag = "anchor_right_top";
            this.datamatrix_remove.UseVisualStyleBackColor = true;
            // 
            // DecodeDataMatixStatusText
            // 
            this.DecodeDataMatixStatusText.AutoSize = true;
            this.DecodeDataMatixStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.DecodeDataMatixStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DecodeDataMatixStatusText.Location = new System.Drawing.Point(156, 575);
            this.DecodeDataMatixStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.DecodeDataMatixStatusText.Name = "DecodeDataMatixStatusText";
            this.DecodeDataMatixStatusText.Size = new System.Drawing.Size(65, 12);
            this.DecodeDataMatixStatusText.TabIndex = 4;
            this.DecodeDataMatixStatusText.Tag = "anchor_left_bottom";
            this.DecodeDataMatixStatusText.Text = "等待处理中";
            // 
            // DecodeDataMarix
            // 
            this.DecodeDataMarix.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DecodeDataMarix.Location = new System.Drawing.Point(697, 561);
            this.DecodeDataMarix.Margin = new System.Windows.Forms.Padding(1);
            this.DecodeDataMarix.Name = "DecodeDataMarix";
            this.DecodeDataMarix.Size = new System.Drawing.Size(147, 37);
            this.DecodeDataMarix.TabIndex = 3;
            this.DecodeDataMarix.Tag = "anchor_right_bottom";
            this.DecodeDataMarix.Text = "开始处理";
            this.DecodeDataMarix.UseVisualStyleBackColor = true;
            this.DecodeDataMarix.Click += new System.EventHandler(this.DecodeDataMarix_Click);
            // 
            // SelectDataMatrixWord
            // 
            this.SelectDataMatrixWord.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectDataMatrixWord.Location = new System.Drawing.Point(5, 561);
            this.SelectDataMatrixWord.Margin = new System.Windows.Forms.Padding(1);
            this.SelectDataMatrixWord.Name = "SelectDataMatrixWord";
            this.SelectDataMatrixWord.Size = new System.Drawing.Size(147, 37);
            this.SelectDataMatrixWord.TabIndex = 2;
            this.SelectDataMatrixWord.Tag = "anchor_left_bottom";
            this.SelectDataMatrixWord.Text = "选择文件";
            this.SelectDataMatrixWord.UseVisualStyleBackColor = true;
            this.SelectDataMatrixWord.Click += new System.EventHandler(this.SelectDataMatrixWord_Click);
            // 
            // groupBox14
            // 
            this.groupBox14.Controls.Add(this.DecodeDataMatrixView);
            this.groupBox14.Location = new System.Drawing.Point(7, 223);
            this.groupBox14.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox14.Name = "groupBox14";
            this.groupBox14.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox14.Size = new System.Drawing.Size(848, 289);
            this.groupBox14.TabIndex = 1;
            this.groupBox14.TabStop = false;
            this.groupBox14.Tag = "anchor_full_fill";
            this.groupBox14.Text = "解析信息";
            // 
            // DecodeDataMatrixView
            // 
            this.DecodeDataMatrixView.Location = new System.Drawing.Point(4, 17);
            this.DecodeDataMatrixView.Margin = new System.Windows.Forms.Padding(1);
            this.DecodeDataMatrixView.Name = "DecodeDataMatrixView";
            this.DecodeDataMatrixView.Size = new System.Drawing.Size(843, 268);
            this.DecodeDataMatrixView.TabIndex = 0;
            this.DecodeDataMatrixView.Tag = "anchor_full_fill";
            // 
            // DecodeMatrixWordListView
            // 
            this.DecodeMatrixWordListView.HideSelection = false;
            this.DecodeMatrixWordListView.Location = new System.Drawing.Point(7, 4);
            this.DecodeMatrixWordListView.Margin = new System.Windows.Forms.Padding(1);
            this.DecodeMatrixWordListView.Name = "DecodeMatrixWordListView";
            this.DecodeMatrixWordListView.Size = new System.Drawing.Size(851, 217);
            this.DecodeMatrixWordListView.TabIndex = 0;
            this.DecodeMatrixWordListView.Tag = "anchor_top_left_right";
            this.DecodeMatrixWordListView.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_33
            // 
            this.TabPage_33.Controls.Add(this.barcodeReadStart);
            this.TabPage_33.Controls.Add(this.CopyBarcodeReadResult);
            this.TabPage_33.Controls.Add(this.barcodeReaderResultbox);
            this.TabPage_33.Controls.Add(this.barcodeSelectRegionPicture);
            this.TabPage_33.Controls.Add(this.barcodeRegionPicture);
            this.TabPage_33.Controls.Add(this.groupBox36);
            this.TabPage_33.Controls.Add(this.groupBox38);
            this.TabPage_33.Controls.Add(this.groupBox37);
            this.TabPage_33.Controls.Add(this.groupBox35);
            this.TabPage_33.Controls.Add(this.groupBox34);
            this.TabPage_33.Location = new System.Drawing.Point(4, 76);
            this.TabPage_33.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_33.Name = "TabPage_33";
            this.TabPage_33.Size = new System.Drawing.Size(882, 659);
            this.TabPage_33.TabIndex = 32;
            this.TabPage_33.Text = "简易二维码生成";
            this.TabPage_33.UseVisualStyleBackColor = true;
            // 
            // barcodeReadStart
            // 
            this.barcodeReadStart.Font = new System.Drawing.Font("宋体", 7.5F);
            this.barcodeReadStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.barcodeReadStart.Location = new System.Drawing.Point(307, 343);
            this.barcodeReadStart.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeReadStart.Name = "barcodeReadStart";
            this.barcodeReadStart.Size = new System.Drawing.Size(83, 27);
            this.barcodeReadStart.TabIndex = 30;
            this.barcodeReadStart.Text = "识别条码";
            this.barcodeReadStart.UseVisualStyleBackColor = true;
            this.barcodeReadStart.Click += new System.EventHandler(this.barcodeReadStart_Click);
            // 
            // CopyBarcodeReadResult
            // 
            this.CopyBarcodeReadResult.Font = new System.Drawing.Font("宋体", 7.5F);
            this.CopyBarcodeReadResult.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CopyBarcodeReadResult.Location = new System.Drawing.Point(791, 343);
            this.CopyBarcodeReadResult.Margin = new System.Windows.Forms.Padding(1);
            this.CopyBarcodeReadResult.Name = "CopyBarcodeReadResult";
            this.CopyBarcodeReadResult.Size = new System.Drawing.Size(83, 27);
            this.CopyBarcodeReadResult.TabIndex = 29;
            this.CopyBarcodeReadResult.Tag = "anchor_right_top";
            this.CopyBarcodeReadResult.Text = "复制识别内容";
            this.CopyBarcodeReadResult.UseVisualStyleBackColor = true;
            this.CopyBarcodeReadResult.Click += new System.EventHandler(this.CopyBarcodeReadResult_Click);
            // 
            // barcodeReaderResultbox
            // 
            this.barcodeReaderResultbox.DetectUrls = false;
            this.barcodeReaderResultbox.Location = new System.Drawing.Point(307, 169);
            this.barcodeReaderResultbox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeReaderResultbox.Name = "barcodeReaderResultbox";
            this.barcodeReaderResultbox.Size = new System.Drawing.Size(568, 171);
            this.barcodeReaderResultbox.TabIndex = 28;
            this.barcodeReaderResultbox.Tag = "anchor_top_left_right";
            this.barcodeReaderResultbox.Text = "";
            // 
            // barcodeSelectRegionPicture
            // 
            this.barcodeSelectRegionPicture.Font = new System.Drawing.Font("宋体", 7.5F);
            this.barcodeSelectRegionPicture.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.barcodeSelectRegionPicture.Location = new System.Drawing.Point(201, 349);
            this.barcodeSelectRegionPicture.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeSelectRegionPicture.Name = "barcodeSelectRegionPicture";
            this.barcodeSelectRegionPicture.Size = new System.Drawing.Size(101, 19);
            this.barcodeSelectRegionPicture.TabIndex = 27;
            this.barcodeSelectRegionPicture.Tag = "anchor_left_top";
            this.barcodeSelectRegionPicture.Text = "选择条码图片...";
            this.barcodeSelectRegionPicture.UseVisualStyleBackColor = true;
            this.barcodeSelectRegionPicture.Click += new System.EventHandler(this.barcodeSelectRegionPicture_Click);
            // 
            // barcodeRegionPicture
            // 
            this.barcodeRegionPicture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.barcodeRegionPicture.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.barcodeRegionPicture.Location = new System.Drawing.Point(201, 169);
            this.barcodeRegionPicture.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeRegionPicture.Name = "barcodeRegionPicture";
            this.barcodeRegionPicture.Size = new System.Drawing.Size(103, 177);
            this.barcodeRegionPicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.barcodeRegionPicture.TabIndex = 26;
            this.barcodeRegionPicture.TabStop = false;
            this.barcodeRegionPicture.Tag = "anchor_left_top";
            // 
            // groupBox36
            // 
            this.groupBox36.Controls.Add(this.barcodeCopyImage);
            this.groupBox36.Controls.Add(this.GenerateBarCode);
            this.groupBox36.Controls.Add(this.SaveBarCodePreview);
            this.groupBox36.Controls.Add(this.barcodePreviewBox);
            this.groupBox36.Location = new System.Drawing.Point(665, 3);
            this.groupBox36.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox36.Name = "groupBox36";
            this.groupBox36.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox36.Size = new System.Drawing.Size(225, 164);
            this.groupBox36.TabIndex = 2;
            this.groupBox36.TabStop = false;
            this.groupBox36.Tag = "anchor_right_top";
            this.groupBox36.Text = "条码预览";
            // 
            // barcodeCopyImage
            // 
            this.barcodeCopyImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.barcodeCopyImage.Location = new System.Drawing.Point(77, 125);
            this.barcodeCopyImage.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeCopyImage.Name = "barcodeCopyImage";
            this.barcodeCopyImage.Size = new System.Drawing.Size(60, 29);
            this.barcodeCopyImage.TabIndex = 3;
            this.barcodeCopyImage.Text = "复制";
            this.barcodeCopyImage.UseVisualStyleBackColor = true;
            this.barcodeCopyImage.Click += new System.EventHandler(this.barcodeCopyImage_Click);
            // 
            // GenerateBarCode
            // 
            this.GenerateBarCode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GenerateBarCode.Location = new System.Drawing.Point(4, 127);
            this.GenerateBarCode.Margin = new System.Windows.Forms.Padding(1);
            this.GenerateBarCode.Name = "GenerateBarCode";
            this.GenerateBarCode.Size = new System.Drawing.Size(59, 29);
            this.GenerateBarCode.TabIndex = 2;
            this.GenerateBarCode.Text = "生成";
            this.GenerateBarCode.UseVisualStyleBackColor = true;
            this.GenerateBarCode.Click += new System.EventHandler(this.GenerateBarCode_Click);
            // 
            // SaveBarCodePreview
            // 
            this.SaveBarCodePreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SaveBarCodePreview.Location = new System.Drawing.Point(147, 125);
            this.SaveBarCodePreview.Margin = new System.Windows.Forms.Padding(1);
            this.SaveBarCodePreview.Name = "SaveBarCodePreview";
            this.SaveBarCodePreview.Size = new System.Drawing.Size(61, 29);
            this.SaveBarCodePreview.TabIndex = 1;
            this.SaveBarCodePreview.Text = "保存";
            this.SaveBarCodePreview.UseVisualStyleBackColor = true;
            this.SaveBarCodePreview.Click += new System.EventHandler(this.SaveBarCodePreview_Click);
            // 
            // barcodePreviewBox
            // 
            this.barcodePreviewBox.Image = global::DocumentTools.Properties.Resources._1351;
            this.barcodePreviewBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.barcodePreviewBox.Location = new System.Drawing.Point(4, 15);
            this.barcodePreviewBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodePreviewBox.Name = "barcodePreviewBox";
            this.barcodePreviewBox.Size = new System.Drawing.Size(204, 107);
            this.barcodePreviewBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.barcodePreviewBox.TabIndex = 0;
            this.barcodePreviewBox.TabStop = false;
            // 
            // groupBox38
            // 
            this.groupBox38.Controls.Add(this.barcodeCaptionLocationCombox);
            this.groupBox38.Controls.Add(this.label93);
            this.groupBox38.Controls.Add(this.barcodeCaptionFontSizeBox);
            this.groupBox38.Controls.Add(this.label92);
            this.groupBox38.Controls.Add(this.barcodeCaptionBox);
            this.groupBox38.Controls.Add(this.label90);
            this.groupBox38.Location = new System.Drawing.Point(489, 3);
            this.groupBox38.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox38.Name = "groupBox38";
            this.groupBox38.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox38.Size = new System.Drawing.Size(169, 164);
            this.groupBox38.TabIndex = 25;
            this.groupBox38.TabStop = false;
            this.groupBox38.Tag = "anchor_right_top";
            this.groupBox38.Text = "条码标题设置";
            // 
            // barcodeCaptionLocationCombox
            // 
            this.barcodeCaptionLocationCombox.FormattingEnabled = true;
            this.barcodeCaptionLocationCombox.Location = new System.Drawing.Point(61, 61);
            this.barcodeCaptionLocationCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeCaptionLocationCombox.Name = "barcodeCaptionLocationCombox";
            this.barcodeCaptionLocationCombox.Size = new System.Drawing.Size(105, 20);
            this.barcodeCaptionLocationCombox.TabIndex = 25;
            // 
            // label93
            // 
            this.label93.AutoSize = true;
            this.label93.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label93.Location = new System.Drawing.Point(4, 64);
            this.label93.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label93.Name = "label93";
            this.label93.Size = new System.Drawing.Size(53, 12);
            this.label93.TabIndex = 25;
            this.label93.Text = "显示位置";
            // 
            // barcodeCaptionFontSizeBox
            // 
            this.barcodeCaptionFontSizeBox.Location = new System.Drawing.Point(61, 111);
            this.barcodeCaptionFontSizeBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeCaptionFontSizeBox.Name = "barcodeCaptionFontSizeBox";
            this.barcodeCaptionFontSizeBox.Size = new System.Drawing.Size(105, 21);
            this.barcodeCaptionFontSizeBox.TabIndex = 25;
            this.barcodeCaptionFontSizeBox.Text = "10";
            // 
            // label92
            // 
            this.label92.AutoSize = true;
            this.label92.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label92.Location = new System.Drawing.Point(4, 115);
            this.label92.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label92.Name = "label92";
            this.label92.Size = new System.Drawing.Size(53, 12);
            this.label92.TabIndex = 25;
            this.label92.Text = "字体大小";
            // 
            // barcodeCaptionBox
            // 
            this.barcodeCaptionBox.AcceptsReturn = true;
            this.barcodeCaptionBox.Location = new System.Drawing.Point(61, 17);
            this.barcodeCaptionBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeCaptionBox.Multiline = true;
            this.barcodeCaptionBox.Name = "barcodeCaptionBox";
            this.barcodeCaptionBox.Size = new System.Drawing.Size(105, 20);
            this.barcodeCaptionBox.TabIndex = 25;
            // 
            // label90
            // 
            this.label90.AutoSize = true;
            this.label90.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label90.Location = new System.Drawing.Point(4, 23);
            this.label90.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(53, 12);
            this.label90.TabIndex = 25;
            this.label90.Text = "标题内容";
            // 
            // groupBox37
            // 
            this.groupBox37.Controls.Add(this.ExportBarCodeTempLate);
            this.groupBox37.Controls.Add(this.barcodeMutilGenerate);
            this.groupBox37.Controls.Add(this.barcodeTemplateUpload);
            this.groupBox37.Controls.Add(this.barcodeMutilList);
            this.groupBox37.Location = new System.Drawing.Point(1, 373);
            this.groupBox37.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox37.Name = "groupBox37";
            this.groupBox37.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox37.Size = new System.Drawing.Size(880, 245);
            this.groupBox37.TabIndex = 3;
            this.groupBox37.TabStop = false;
            this.groupBox37.Tag = "anchor_full_fill";
            this.groupBox37.Text = "批量生成条码";
            // 
            // ExportBarCodeTempLate
            // 
            this.ExportBarCodeTempLate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExportBarCodeTempLate.Location = new System.Drawing.Point(375, 193);
            this.ExportBarCodeTempLate.Margin = new System.Windows.Forms.Padding(1);
            this.ExportBarCodeTempLate.Name = "ExportBarCodeTempLate";
            this.ExportBarCodeTempLate.Size = new System.Drawing.Size(105, 41);
            this.ExportBarCodeTempLate.TabIndex = 3;
            this.ExportBarCodeTempLate.Tag = "anchor_bottom";
            this.ExportBarCodeTempLate.Text = "导出模板";
            this.ExportBarCodeTempLate.UseVisualStyleBackColor = true;
            this.ExportBarCodeTempLate.Click += new System.EventHandler(this.ExportBarCodeTempLate_Click);
            // 
            // barcodeMutilGenerate
            // 
            this.barcodeMutilGenerate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.barcodeMutilGenerate.Location = new System.Drawing.Point(767, 193);
            this.barcodeMutilGenerate.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeMutilGenerate.Name = "barcodeMutilGenerate";
            this.barcodeMutilGenerate.Size = new System.Drawing.Size(105, 41);
            this.barcodeMutilGenerate.TabIndex = 2;
            this.barcodeMutilGenerate.Tag = "anchor_right_bottom";
            this.barcodeMutilGenerate.Text = "开始生成";
            this.barcodeMutilGenerate.UseVisualStyleBackColor = true;
            this.barcodeMutilGenerate.Click += new System.EventHandler(this.barcodeMutilGenerate_Click);
            // 
            // barcodeTemplateUpload
            // 
            this.barcodeTemplateUpload.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.barcodeTemplateUpload.Location = new System.Drawing.Point(7, 193);
            this.barcodeTemplateUpload.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeTemplateUpload.Name = "barcodeTemplateUpload";
            this.barcodeTemplateUpload.Size = new System.Drawing.Size(105, 41);
            this.barcodeTemplateUpload.TabIndex = 1;
            this.barcodeTemplateUpload.Tag = "anchor_left_bottom";
            this.barcodeTemplateUpload.Text = "选择模板...";
            this.barcodeTemplateUpload.UseVisualStyleBackColor = true;
            this.barcodeTemplateUpload.Click += new System.EventHandler(this.barcodeTemplateUpload_Click);
            // 
            // barcodeMutilList
            // 
            this.barcodeMutilList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.barcodeMutilList.Location = new System.Drawing.Point(7, 17);
            this.barcodeMutilList.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeMutilList.Name = "barcodeMutilList";
            this.barcodeMutilList.RowHeadersWidth = 62;
            this.barcodeMutilList.RowTemplate.Height = 30;
            this.barcodeMutilList.Size = new System.Drawing.Size(865, 157);
            this.barcodeMutilList.TabIndex = 0;
            this.barcodeMutilList.Tag = "anchor_full_fill";
            // 
            // groupBox35
            // 
            this.groupBox35.Controls.Add(this.barcodeImageWidth);
            this.groupBox35.Controls.Add(this.barcodePaddingBox);
            this.groupBox35.Controls.Add(this.label89);
            this.groupBox35.Controls.Add(this.label12);
            this.groupBox35.Controls.Add(this.barcodeTypeCombox);
            this.groupBox35.Controls.Add(this.label80);
            this.groupBox35.Controls.Add(this.barcodeDmiBox);
            this.groupBox35.Controls.Add(this.label81);
            this.groupBox35.Controls.Add(this.barcodeRotateBox);
            this.groupBox35.Controls.Add(this.barcodeDpiCombox);
            this.groupBox35.Controls.Add(this.label82);
            this.groupBox35.Controls.Add(this.label84);
            this.groupBox35.Controls.Add(this.label83);
            this.groupBox35.Controls.Add(this.barcodeAutoSizeCombox);
            this.groupBox35.Controls.Add(this.label79);
            this.groupBox35.Controls.Add(this.barcodeImageHeight);
            this.groupBox35.Location = new System.Drawing.Point(1, 3);
            this.groupBox35.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox35.Name = "groupBox35";
            this.groupBox35.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox35.Size = new System.Drawing.Size(196, 365);
            this.groupBox35.TabIndex = 1;
            this.groupBox35.TabStop = false;
            this.groupBox35.Tag = "anchor_left_top";
            this.groupBox35.Text = "条码外观设置";
            // 
            // barcodeImageWidth
            // 
            this.barcodeImageWidth.Location = new System.Drawing.Point(73, 117);
            this.barcodeImageWidth.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeImageWidth.Name = "barcodeImageWidth";
            this.barcodeImageWidth.Size = new System.Drawing.Size(113, 21);
            this.barcodeImageWidth.TabIndex = 24;
            this.barcodeImageWidth.Text = "100";
            // 
            // barcodePaddingBox
            // 
            this.barcodePaddingBox.Location = new System.Drawing.Point(73, 199);
            this.barcodePaddingBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodePaddingBox.Name = "barcodePaddingBox";
            this.barcodePaddingBox.Size = new System.Drawing.Size(113, 21);
            this.barcodePaddingBox.TabIndex = 23;
            this.barcodePaddingBox.Text = "5";
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label89.Location = new System.Drawing.Point(4, 205);
            this.label89.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(53, 12);
            this.label89.TabIndex = 15;
            this.label89.Text = "内部间距";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label12.Location = new System.Drawing.Point(4, 23);
            this.label12.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(53, 12);
            this.label12.TabIndex = 0;
            this.label12.Text = "条码类型";
            // 
            // barcodeTypeCombox
            // 
            this.barcodeTypeCombox.FormattingEnabled = true;
            this.barcodeTypeCombox.Location = new System.Drawing.Point(73, 17);
            this.barcodeTypeCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeTypeCombox.Name = "barcodeTypeCombox";
            this.barcodeTypeCombox.Size = new System.Drawing.Size(113, 20);
            this.barcodeTypeCombox.TabIndex = 1;
            // 
            // label80
            // 
            this.label80.AutoSize = true;
            this.label80.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label80.Location = new System.Drawing.Point(3, 73);
            this.label80.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label80.Name = "label80";
            this.label80.Size = new System.Drawing.Size(53, 12);
            this.label80.TabIndex = 4;
            this.label80.Text = "条码密度";
            // 
            // barcodeDmiBox
            // 
            this.barcodeDmiBox.Location = new System.Drawing.Point(73, 67);
            this.barcodeDmiBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeDmiBox.Name = "barcodeDmiBox";
            this.barcodeDmiBox.Size = new System.Drawing.Size(113, 21);
            this.barcodeDmiBox.TabIndex = 5;
            this.barcodeDmiBox.Text = "6";
            // 
            // label81
            // 
            this.label81.AutoSize = true;
            this.label81.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label81.Location = new System.Drawing.Point(4, 97);
            this.label81.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label81.Name = "label81";
            this.label81.Size = new System.Drawing.Size(53, 12);
            this.label81.TabIndex = 6;
            this.label81.Text = "旋转角度";
            // 
            // barcodeRotateBox
            // 
            this.barcodeRotateBox.Location = new System.Drawing.Point(73, 91);
            this.barcodeRotateBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeRotateBox.Name = "barcodeRotateBox";
            this.barcodeRotateBox.Size = new System.Drawing.Size(113, 21);
            this.barcodeRotateBox.TabIndex = 7;
            this.barcodeRotateBox.Text = "0";
            // 
            // barcodeDpiCombox
            // 
            this.barcodeDpiCombox.FormattingEnabled = true;
            this.barcodeDpiCombox.Location = new System.Drawing.Point(73, 175);
            this.barcodeDpiCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeDpiCombox.Name = "barcodeDpiCombox";
            this.barcodeDpiCombox.Size = new System.Drawing.Size(113, 20);
            this.barcodeDpiCombox.TabIndex = 14;
            // 
            // label82
            // 
            this.label82.AutoSize = true;
            this.label82.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label82.Location = new System.Drawing.Point(4, 124);
            this.label82.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label82.Name = "label82";
            this.label82.Size = new System.Drawing.Size(53, 12);
            this.label82.TabIndex = 8;
            this.label82.Text = "条码宽度";
            // 
            // label84
            // 
            this.label84.AutoSize = true;
            this.label84.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label84.Location = new System.Drawing.Point(4, 176);
            this.label84.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label84.Name = "label84";
            this.label84.Size = new System.Drawing.Size(65, 12);
            this.label84.TabIndex = 13;
            this.label84.Text = "条码分辨率";
            // 
            // label83
            // 
            this.label83.AutoSize = true;
            this.label83.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label83.Location = new System.Drawing.Point(3, 151);
            this.label83.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label83.Name = "label83";
            this.label83.Size = new System.Drawing.Size(53, 12);
            this.label83.TabIndex = 10;
            this.label83.Text = "条码高度";
            // 
            // barcodeAutoSizeCombox
            // 
            this.barcodeAutoSizeCombox.FormattingEnabled = true;
            this.barcodeAutoSizeCombox.Location = new System.Drawing.Point(73, 41);
            this.barcodeAutoSizeCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeAutoSizeCombox.Name = "barcodeAutoSizeCombox";
            this.barcodeAutoSizeCombox.Size = new System.Drawing.Size(113, 20);
            this.barcodeAutoSizeCombox.TabIndex = 3;
            // 
            // label79
            // 
            this.label79.AutoSize = true;
            this.label79.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label79.Location = new System.Drawing.Point(4, 47);
            this.label79.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label79.Name = "label79";
            this.label79.Size = new System.Drawing.Size(53, 12);
            this.label79.TabIndex = 2;
            this.label79.Text = "尺寸模式";
            // 
            // barcodeImageHeight
            // 
            this.barcodeImageHeight.Location = new System.Drawing.Point(73, 145);
            this.barcodeImageHeight.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeImageHeight.Name = "barcodeImageHeight";
            this.barcodeImageHeight.Size = new System.Drawing.Size(113, 21);
            this.barcodeImageHeight.TabIndex = 11;
            this.barcodeImageHeight.Text = "100";
            // 
            // groupBox34
            // 
            this.groupBox34.Controls.Add(this.barcodeScanTypeCombox);
            this.groupBox34.Controls.Add(this.label94);
            this.groupBox34.Controls.Add(this.barcodeTextFontSizeBox);
            this.groupBox34.Controls.Add(this.label91);
            this.groupBox34.Controls.Add(this.barcodeTextContentBox);
            this.groupBox34.Controls.Add(this.label88);
            this.groupBox34.Controls.Add(this.barcodeTextContentSpaceCombox);
            this.groupBox34.Controls.Add(this.label87);
            this.groupBox34.Controls.Add(this.barcodeTextAlignCombox);
            this.groupBox34.Controls.Add(this.label86);
            this.groupBox34.Controls.Add(this.barcodeTextLocationCombox);
            this.groupBox34.Controls.Add(this.label85);
            this.groupBox34.Location = new System.Drawing.Point(201, 3);
            this.groupBox34.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox34.Name = "groupBox34";
            this.groupBox34.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox34.Size = new System.Drawing.Size(283, 164);
            this.groupBox34.TabIndex = 0;
            this.groupBox34.TabStop = false;
            this.groupBox34.Tag = "anchor_left_top";
            this.groupBox34.Text = "条码内容设置";
            // 
            // barcodeScanTypeCombox
            // 
            this.barcodeScanTypeCombox.FormattingEnabled = true;
            this.barcodeScanTypeCombox.Location = new System.Drawing.Point(67, 39);
            this.barcodeScanTypeCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeScanTypeCombox.Name = "barcodeScanTypeCombox";
            this.barcodeScanTypeCombox.Size = new System.Drawing.Size(213, 20);
            this.barcodeScanTypeCombox.TabIndex = 26;
            // 
            // label94
            // 
            this.label94.AutoSize = true;
            this.label94.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label94.Location = new System.Drawing.Point(9, 41);
            this.label94.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label94.Name = "label94";
            this.label94.Size = new System.Drawing.Size(53, 12);
            this.label94.TabIndex = 25;
            this.label94.Text = "生成类别";
            // 
            // barcodeTextFontSizeBox
            // 
            this.barcodeTextFontSizeBox.Location = new System.Drawing.Point(67, 139);
            this.barcodeTextFontSizeBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeTextFontSizeBox.Name = "barcodeTextFontSizeBox";
            this.barcodeTextFontSizeBox.Size = new System.Drawing.Size(213, 21);
            this.barcodeTextFontSizeBox.TabIndex = 24;
            this.barcodeTextFontSizeBox.Text = "10";
            // 
            // label91
            // 
            this.label91.AutoSize = true;
            this.label91.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label91.Location = new System.Drawing.Point(9, 143);
            this.label91.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(53, 12);
            this.label91.TabIndex = 23;
            this.label91.Text = "字体大小";
            // 
            // barcodeTextContentBox
            // 
            this.barcodeTextContentBox.AcceptsReturn = true;
            this.barcodeTextContentBox.Location = new System.Drawing.Point(67, 15);
            this.barcodeTextContentBox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeTextContentBox.Multiline = true;
            this.barcodeTextContentBox.Name = "barcodeTextContentBox";
            this.barcodeTextContentBox.Size = new System.Drawing.Size(213, 20);
            this.barcodeTextContentBox.TabIndex = 22;
            this.barcodeTextContentBox.Text = "你干嘛哎呦";
            // 
            // label88
            // 
            this.label88.AutoSize = true;
            this.label88.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label88.Location = new System.Drawing.Point(9, 20);
            this.label88.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(53, 12);
            this.label88.TabIndex = 21;
            this.label88.Text = "文本内容";
            // 
            // barcodeTextContentSpaceCombox
            // 
            this.barcodeTextContentSpaceCombox.Location = new System.Drawing.Point(67, 113);
            this.barcodeTextContentSpaceCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeTextContentSpaceCombox.Name = "barcodeTextContentSpaceCombox";
            this.barcodeTextContentSpaceCombox.Size = new System.Drawing.Size(213, 21);
            this.barcodeTextContentSpaceCombox.TabIndex = 20;
            this.barcodeTextContentSpaceCombox.Text = "5";
            // 
            // label87
            // 
            this.label87.AutoSize = true;
            this.label87.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label87.Location = new System.Drawing.Point(9, 117);
            this.label87.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label87.Name = "label87";
            this.label87.Size = new System.Drawing.Size(53, 12);
            this.label87.TabIndex = 19;
            this.label87.Text = "内容间距";
            // 
            // barcodeTextAlignCombox
            // 
            this.barcodeTextAlignCombox.FormattingEnabled = true;
            this.barcodeTextAlignCombox.Location = new System.Drawing.Point(67, 89);
            this.barcodeTextAlignCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeTextAlignCombox.Name = "barcodeTextAlignCombox";
            this.barcodeTextAlignCombox.Size = new System.Drawing.Size(213, 20);
            this.barcodeTextAlignCombox.TabIndex = 18;
            // 
            // label86
            // 
            this.label86.AutoSize = true;
            this.label86.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label86.Location = new System.Drawing.Point(9, 93);
            this.label86.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label86.Name = "label86";
            this.label86.Size = new System.Drawing.Size(53, 12);
            this.label86.TabIndex = 17;
            this.label86.Text = "对齐方式";
            // 
            // barcodeTextLocationCombox
            // 
            this.barcodeTextLocationCombox.FormattingEnabled = true;
            this.barcodeTextLocationCombox.Location = new System.Drawing.Point(67, 63);
            this.barcodeTextLocationCombox.Margin = new System.Windows.Forms.Padding(1);
            this.barcodeTextLocationCombox.Name = "barcodeTextLocationCombox";
            this.barcodeTextLocationCombox.Size = new System.Drawing.Size(213, 20);
            this.barcodeTextLocationCombox.TabIndex = 16;
            // 
            // label85
            // 
            this.label85.AutoSize = true;
            this.label85.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label85.Location = new System.Drawing.Point(9, 67);
            this.label85.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label85.Name = "label85";
            this.label85.Size = new System.Drawing.Size(53, 12);
            this.label85.TabIndex = 15;
            this.label85.Text = "显示位置";
            // 
            // TabPage_34
            // 
            this.TabPage_34.Controls.Add(this.groupBox41);
            this.TabPage_34.Controls.Add(this.gen_card_result_group);
            this.TabPage_34.Controls.Add(this.gen_card_id_group);
            this.TabPage_34.Location = new System.Drawing.Point(4, 76);
            this.TabPage_34.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_34.Name = "TabPage_34";
            this.TabPage_34.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_34.Size = new System.Drawing.Size(882, 659);
            this.TabPage_34.TabIndex = 33;
            this.TabPage_34.Text = "身份证号码校验";
            this.TabPage_34.UseVisualStyleBackColor = true;
            // 
            // groupBox41
            // 
            this.groupBox41.Controls.Add(this.groupBox42);
            this.groupBox41.Controls.Add(this.ver_card_id_text_box);
            this.groupBox41.Controls.Add(this.label99);
            this.groupBox41.Location = new System.Drawing.Point(319, 119);
            this.groupBox41.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox41.Name = "groupBox41";
            this.groupBox41.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox41.Size = new System.Drawing.Size(564, 499);
            this.groupBox41.TabIndex = 3;
            this.groupBox41.TabStop = false;
            this.groupBox41.Tag = "anchor_full_fill";
            this.groupBox41.Text = "身份证号码校验";
            // 
            // groupBox42
            // 
            this.groupBox42.Controls.Add(this.ver_card_id_index);
            this.groupBox42.Controls.Add(this.label103);
            this.groupBox42.Controls.Add(this.ver_card_id_sign_tips);
            this.groupBox42.Controls.Add(this.ver_card_id_sex);
            this.groupBox42.Controls.Add(this.ver_card_id_birthday);
            this.groupBox42.Controls.Add(this.label102);
            this.groupBox42.Controls.Add(this.label101);
            this.groupBox42.Controls.Add(this.label100);
            this.groupBox42.Location = new System.Drawing.Point(7, 57);
            this.groupBox42.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox42.Name = "groupBox42";
            this.groupBox42.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox42.Size = new System.Drawing.Size(553, 429);
            this.groupBox42.TabIndex = 2;
            this.groupBox42.TabStop = false;
            this.groupBox42.Tag = "anchor_full_fill";
            this.groupBox42.Text = "校验结果";
            // 
            // ver_card_id_index
            // 
            this.ver_card_id_index.AutoSize = true;
            this.ver_card_id_index.Font = new System.Drawing.Font("宋体", 12F);
            this.ver_card_id_index.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ver_card_id_index.Location = new System.Drawing.Point(79, 177);
            this.ver_card_id_index.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ver_card_id_index.Name = "ver_card_id_index";
            this.ver_card_id_index.Size = new System.Drawing.Size(39, 16);
            this.ver_card_id_index.TabIndex = 7;
            this.ver_card_id_index.Tag = "";
            this.ver_card_id_index.Text = "暂无";
            // 
            // label103
            // 
            this.label103.AutoSize = true;
            this.label103.Font = new System.Drawing.Font("宋体", 12F);
            this.label103.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label103.Location = new System.Drawing.Point(4, 177);
            this.label103.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label103.Name = "label103";
            this.label103.Size = new System.Drawing.Size(71, 16);
            this.label103.TabIndex = 6;
            this.label103.Tag = "";
            this.label103.Text = "顺序编码";
            // 
            // ver_card_id_sign_tips
            // 
            this.ver_card_id_sign_tips.AutoSize = true;
            this.ver_card_id_sign_tips.Font = new System.Drawing.Font("宋体", 12F);
            this.ver_card_id_sign_tips.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ver_card_id_sign_tips.Location = new System.Drawing.Point(79, 255);
            this.ver_card_id_sign_tips.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ver_card_id_sign_tips.Name = "ver_card_id_sign_tips";
            this.ver_card_id_sign_tips.Size = new System.Drawing.Size(39, 16);
            this.ver_card_id_sign_tips.TabIndex = 5;
            this.ver_card_id_sign_tips.Tag = "";
            this.ver_card_id_sign_tips.Text = "暂无";
            // 
            // ver_card_id_sex
            // 
            this.ver_card_id_sex.AutoSize = true;
            this.ver_card_id_sex.Font = new System.Drawing.Font("宋体", 12F);
            this.ver_card_id_sex.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ver_card_id_sex.Location = new System.Drawing.Point(55, 107);
            this.ver_card_id_sex.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ver_card_id_sex.Name = "ver_card_id_sex";
            this.ver_card_id_sex.Size = new System.Drawing.Size(39, 16);
            this.ver_card_id_sex.TabIndex = 4;
            this.ver_card_id_sex.Tag = "";
            this.ver_card_id_sex.Text = "暂无";
            // 
            // ver_card_id_birthday
            // 
            this.ver_card_id_birthday.AutoSize = true;
            this.ver_card_id_birthday.Font = new System.Drawing.Font("宋体", 12F);
            this.ver_card_id_birthday.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ver_card_id_birthday.Location = new System.Drawing.Point(55, 41);
            this.ver_card_id_birthday.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ver_card_id_birthday.Name = "ver_card_id_birthday";
            this.ver_card_id_birthday.Size = new System.Drawing.Size(39, 16);
            this.ver_card_id_birthday.TabIndex = 3;
            this.ver_card_id_birthday.Tag = "";
            this.ver_card_id_birthday.Text = "暂无";
            // 
            // label102
            // 
            this.label102.AutoSize = true;
            this.label102.Font = new System.Drawing.Font("宋体", 12F);
            this.label102.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label102.Location = new System.Drawing.Point(4, 255);
            this.label102.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label102.Name = "label102";
            this.label102.Size = new System.Drawing.Size(71, 16);
            this.label102.TabIndex = 2;
            this.label102.Tag = "";
            this.label102.Text = "校验结果";
            // 
            // label101
            // 
            this.label101.AutoSize = true;
            this.label101.Font = new System.Drawing.Font("宋体", 12F);
            this.label101.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label101.Location = new System.Drawing.Point(4, 107);
            this.label101.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label101.Name = "label101";
            this.label101.Size = new System.Drawing.Size(39, 16);
            this.label101.TabIndex = 1;
            this.label101.Tag = "";
            this.label101.Text = "性别";
            // 
            // label100
            // 
            this.label100.AutoSize = true;
            this.label100.Font = new System.Drawing.Font("宋体", 12F);
            this.label100.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label100.Location = new System.Drawing.Point(4, 41);
            this.label100.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label100.Name = "label100";
            this.label100.Size = new System.Drawing.Size(39, 16);
            this.label100.TabIndex = 0;
            this.label100.Tag = "";
            this.label100.Text = "生日";
            // 
            // ver_card_id_text_box
            // 
            this.ver_card_id_text_box.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ver_card_id_text_box.Font = new System.Drawing.Font("宋体", 10.5F);
            this.ver_card_id_text_box.Location = new System.Drawing.Point(81, 23);
            this.ver_card_id_text_box.Margin = new System.Windows.Forms.Padding(1);
            this.ver_card_id_text_box.MaxLength = 18;
            this.ver_card_id_text_box.Name = "ver_card_id_text_box";
            this.ver_card_id_text_box.Size = new System.Drawing.Size(480, 23);
            this.ver_card_id_text_box.TabIndex = 1;
            this.ver_card_id_text_box.Tag = "anchor_top_left_right";
            this.MainToolsTip.SetToolTip(this.ver_card_id_text_box, "输入sfz解锁身份证生成功能");
            this.ver_card_id_text_box.TextChanged += new System.EventHandler(this.ver_card_id_text_box_TextChanged);
            // 
            // label99
            // 
            this.label99.AutoSize = true;
            this.label99.Font = new System.Drawing.Font("宋体", 9F);
            this.label99.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label99.Location = new System.Drawing.Point(4, 27);
            this.label99.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label99.Name = "label99";
            this.label99.Size = new System.Drawing.Size(65, 12);
            this.label99.TabIndex = 0;
            this.label99.Tag = "anchor_left_top";
            this.label99.Text = "身份证号码";
            // 
            // gen_card_result_group
            // 
            this.gen_card_result_group.Controls.Add(this.gen_card_id_name_copy);
            this.gen_card_result_group.Controls.Add(this.copy_gen_card_cliboard);
            this.gen_card_result_group.Controls.Add(this.gen_card_num_text);
            this.gen_card_result_group.Controls.Add(this.gen_card_id_box);
            this.gen_card_result_group.Controls.Add(this.gen_card_id_name_box);
            this.gen_card_result_group.Location = new System.Drawing.Point(1, 119);
            this.gen_card_result_group.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_result_group.Name = "gen_card_result_group";
            this.gen_card_result_group.Padding = new System.Windows.Forms.Padding(1);
            this.gen_card_result_group.Size = new System.Drawing.Size(312, 499);
            this.gen_card_result_group.TabIndex = 4;
            this.gen_card_result_group.TabStop = false;
            this.gen_card_result_group.Tag = "anchor_top_left_bottom";
            this.gen_card_result_group.Text = "生成结果";
            // 
            // gen_card_id_name_copy
            // 
            this.gen_card_id_name_copy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gen_card_id_name_copy.Location = new System.Drawing.Point(243, 455);
            this.gen_card_id_name_copy.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_id_name_copy.Name = "gen_card_id_name_copy";
            this.gen_card_id_name_copy.Size = new System.Drawing.Size(61, 27);
            this.gen_card_id_name_copy.TabIndex = 4;
            this.gen_card_id_name_copy.Tag = "anchor_right_bottom";
            this.gen_card_id_name_copy.Text = "复制姓名";
            this.gen_card_id_name_copy.UseVisualStyleBackColor = true;
            this.gen_card_id_name_copy.Click += new System.EventHandler(this.gen_card_id_name_copy_Click);
            // 
            // copy_gen_card_cliboard
            // 
            this.copy_gen_card_cliboard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.copy_gen_card_cliboard.Location = new System.Drawing.Point(165, 455);
            this.copy_gen_card_cliboard.Margin = new System.Windows.Forms.Padding(1);
            this.copy_gen_card_cliboard.Name = "copy_gen_card_cliboard";
            this.copy_gen_card_cliboard.Size = new System.Drawing.Size(61, 27);
            this.copy_gen_card_cliboard.TabIndex = 2;
            this.copy_gen_card_cliboard.Tag = "anchor_right_bottom";
            this.copy_gen_card_cliboard.Text = "复制号码";
            this.copy_gen_card_cliboard.UseVisualStyleBackColor = true;
            this.copy_gen_card_cliboard.Click += new System.EventHandler(this.copy_gen_card_cliboard_Click);
            // 
            // gen_card_num_text
            // 
            this.gen_card_num_text.AutoSize = true;
            this.gen_card_num_text.Font = new System.Drawing.Font("宋体", 10.5F);
            this.gen_card_num_text.ForeColor = System.Drawing.Color.ForestGreen;
            this.gen_card_num_text.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gen_card_num_text.Location = new System.Drawing.Point(4, 459);
            this.gen_card_num_text.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.gen_card_num_text.Name = "gen_card_num_text";
            this.gen_card_num_text.Size = new System.Drawing.Size(98, 14);
            this.gen_card_num_text.TabIndex = 1;
            this.gen_card_num_text.Tag = "anchor_left_bottom";
            this.gen_card_num_text.Text = "共生成0个号码";
            // 
            // gen_card_id_box
            // 
            this.gen_card_id_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gen_card_id_box.Location = new System.Drawing.Point(7, 19);
            this.gen_card_id_box.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_id_box.Name = "gen_card_id_box";
            this.gen_card_id_box.Size = new System.Drawing.Size(141, 432);
            this.gen_card_id_box.TabIndex = 0;
            this.gen_card_id_box.Tag = "anchor_top_bottom";
            this.gen_card_id_box.Text = "";
            this.gen_card_id_box.TextChanged += new System.EventHandler(this.gen_card_id_box_TextChanged);
            // 
            // gen_card_id_name_box
            // 
            this.gen_card_id_name_box.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gen_card_id_name_box.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gen_card_id_name_box.Location = new System.Drawing.Point(165, 17);
            this.gen_card_id_name_box.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_id_name_box.Name = "gen_card_id_name_box";
            this.gen_card_id_name_box.Size = new System.Drawing.Size(141, 433);
            this.gen_card_id_name_box.TabIndex = 3;
            this.gen_card_id_name_box.Tag = "anchor_top_bottom";
            this.gen_card_id_name_box.Text = "";
            // 
            // gen_card_id_group
            // 
            this.gen_card_id_group.Controls.Add(this.gen_card_id_name_text);
            this.gen_card_id_group.Controls.Add(this.label104);
            this.gen_card_id_group.Controls.Add(this.gen_card_id_name_check_box);
            this.gen_card_id_group.Controls.Add(this.ony_gen_18_year);
            this.gen_card_id_group.Controls.Add(this.gen_card_number);
            this.gen_card_id_group.Controls.Add(this.label97);
            this.gen_card_id_group.Controls.Add(this.gen_card_button);
            this.gen_card_id_group.Controls.Add(this.label96);
            this.gen_card_id_group.Controls.Add(this.city_level_1_combox);
            this.gen_card_id_group.Controls.Add(this.city_level_3_combox);
            this.gen_card_id_group.Controls.Add(this.city_level_2_combox);
            this.gen_card_id_group.Location = new System.Drawing.Point(4, 4);
            this.gen_card_id_group.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_id_group.Name = "gen_card_id_group";
            this.gen_card_id_group.Padding = new System.Windows.Forms.Padding(1);
            this.gen_card_id_group.Size = new System.Drawing.Size(879, 111);
            this.gen_card_id_group.TabIndex = 3;
            this.gen_card_id_group.TabStop = false;
            this.gen_card_id_group.Tag = "anchor_top_left_right";
            this.gen_card_id_group.Text = "基础信息设置";
            // 
            // gen_card_id_name_text
            // 
            this.gen_card_id_name_text.AutoSize = true;
            this.gen_card_id_name_text.Font = new System.Drawing.Font("宋体", 7.5F);
            this.gen_card_id_name_text.ForeColor = System.Drawing.Color.Tomato;
            this.gen_card_id_name_text.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gen_card_id_name_text.Location = new System.Drawing.Point(413, 59);
            this.gen_card_id_name_text.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.gen_card_id_name_text.Name = "gen_card_id_name_text";
            this.gen_card_id_name_text.Size = new System.Drawing.Size(45, 10);
            this.gen_card_id_name_text.TabIndex = 11;
            this.gen_card_id_name_text.Text = "[未勾选]";
            // 
            // label104
            // 
            this.label104.AutoSize = true;
            this.label104.Font = new System.Drawing.Font("宋体", 7.5F);
            this.label104.ForeColor = System.Drawing.Color.Tomato;
            this.label104.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label104.Location = new System.Drawing.Point(11, 85);
            this.label104.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label104.Name = "label104";
            this.label104.Size = new System.Drawing.Size(360, 10);
            this.label104.TabIndex = 10;
            this.label104.Text = "随机姓名由字库随机抽取组合，二字/三字姓名随机，数量较多时需等待一段时间";
            // 
            // gen_card_id_name_check_box
            // 
            this.gen_card_id_name_check_box.AutoSize = true;
            this.gen_card_id_name_check_box.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gen_card_id_name_check_box.Location = new System.Drawing.Point(295, 57);
            this.gen_card_id_name_check_box.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_id_name_check_box.Name = "gen_card_id_name_check_box";
            this.gen_card_id_name_check_box.Size = new System.Drawing.Size(120, 16);
            this.gen_card_id_name_check_box.TabIndex = 9;
            this.gen_card_id_name_check_box.Text = "是否生成随机姓名\r\n";
            this.gen_card_id_name_check_box.UseVisualStyleBackColor = true;
            this.gen_card_id_name_check_box.CheckedChanged += new System.EventHandler(this.gen_card_id_name_check_box_CheckedChanged);
            // 
            // ony_gen_18_year
            // 
            this.ony_gen_18_year.AutoSize = true;
            this.ony_gen_18_year.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ony_gen_18_year.Location = new System.Drawing.Point(149, 57);
            this.ony_gen_18_year.Margin = new System.Windows.Forms.Padding(1);
            this.ony_gen_18_year.Name = "ony_gen_18_year";
            this.ony_gen_18_year.Size = new System.Drawing.Size(144, 16);
            this.ony_gen_18_year.TabIndex = 7;
            this.ony_gen_18_year.Text = "只生成18周岁以上号码";
            this.ony_gen_18_year.UseVisualStyleBackColor = true;
            // 
            // gen_card_number
            // 
            this.gen_card_number.Location = new System.Drawing.Point(65, 55);
            this.gen_card_number.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_number.Maximum = new decimal(new int[] {
            900000,
            0,
            0,
            0});
            this.gen_card_number.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.gen_card_number.Name = "gen_card_number";
            this.gen_card_number.Size = new System.Drawing.Size(80, 21);
            this.gen_card_number.TabIndex = 6;
            this.gen_card_number.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // label97
            // 
            this.label97.AutoSize = true;
            this.label97.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label97.Location = new System.Drawing.Point(8, 59);
            this.label97.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label97.Name = "label97";
            this.label97.Size = new System.Drawing.Size(53, 12);
            this.label97.TabIndex = 5;
            this.label97.Text = "生成数量";
            // 
            // gen_card_button
            // 
            this.gen_card_button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gen_card_button.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.gen_card_button.Location = new System.Drawing.Point(781, 68);
            this.gen_card_button.Margin = new System.Windows.Forms.Padding(1);
            this.gen_card_button.Name = "gen_card_button";
            this.gen_card_button.Size = new System.Drawing.Size(89, 39);
            this.gen_card_button.TabIndex = 4;
            this.gen_card_button.Tag = "anchor_right_top";
            this.gen_card_button.Text = "开始生成";
            this.gen_card_button.UseVisualStyleBackColor = true;
            this.gen_card_button.Click += new System.EventHandler(this.button1_Click);
            // 
            // label96
            // 
            this.label96.AutoSize = true;
            this.label96.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label96.Location = new System.Drawing.Point(5, 29);
            this.label96.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label96.Name = "label96";
            this.label96.Size = new System.Drawing.Size(53, 12);
            this.label96.TabIndex = 3;
            this.label96.Text = "选择地区";
            // 
            // city_level_1_combox
            // 
            this.city_level_1_combox.FormattingEnabled = true;
            this.city_level_1_combox.Location = new System.Drawing.Point(63, 27);
            this.city_level_1_combox.Margin = new System.Windows.Forms.Padding(1);
            this.city_level_1_combox.Name = "city_level_1_combox";
            this.city_level_1_combox.Size = new System.Drawing.Size(189, 20);
            this.city_level_1_combox.TabIndex = 0;
            this.city_level_1_combox.TextChanged += new System.EventHandler(this.city_level_1_combox_TextChanged);
            // 
            // city_level_3_combox
            // 
            this.city_level_3_combox.FormattingEnabled = true;
            this.city_level_3_combox.Location = new System.Drawing.Point(447, 27);
            this.city_level_3_combox.Margin = new System.Windows.Forms.Padding(1);
            this.city_level_3_combox.Name = "city_level_3_combox";
            this.city_level_3_combox.Size = new System.Drawing.Size(427, 20);
            this.city_level_3_combox.TabIndex = 2;
            this.city_level_3_combox.Tag = "anchor_left_right";
            // 
            // city_level_2_combox
            // 
            this.city_level_2_combox.FormattingEnabled = true;
            this.city_level_2_combox.Location = new System.Drawing.Point(255, 27);
            this.city_level_2_combox.Margin = new System.Windows.Forms.Padding(1);
            this.city_level_2_combox.Name = "city_level_2_combox";
            this.city_level_2_combox.Size = new System.Drawing.Size(189, 20);
            this.city_level_2_combox.TabIndex = 1;
            this.city_level_2_combox.TextChanged += new System.EventHandler(this.city_level_2_combox_TextChanged);
            // 
            // TabPage_05
            // 
            this.TabPage_05.Controls.Add(this.DoubleTextClear);
            this.TabPage_05.Controls.Add(this.NameMarkColor);
            this.TabPage_05.Controls.Add(this.MarkDupName);
            this.TabPage_05.Controls.Add(this.ExportNameTxt);
            this.TabPage_05.Controls.Add(this.label57);
            this.TabPage_05.Controls.Add(this.NameReplaceRegexMode);
            this.TabPage_05.Controls.Add(this.NameAutoCopyStatusText);
            this.TabPage_05.Controls.Add(this.NameAutoCopy);
            this.TabPage_05.Controls.Add(this.NameReplaceWord);
            this.TabPage_05.Controls.Add(this.label56);
            this.TabPage_05.Controls.Add(this.NameReplaceWord_Target);
            this.TabPage_05.Controls.Add(this.NameReplaceWord_Search);
            this.TabPage_05.Controls.Add(this.NameStorkeChcek);
            this.TabPage_05.Controls.Add(this.MutilLineMode);
            this.TabPage_05.Controls.Add(this.DoubleTextAlignCheck);
            this.TabPage_05.Controls.Add(this.RemoveNumberCheck);
            this.TabPage_05.Controls.Add(this.RemoveSquareCheck);
            this.TabPage_05.Controls.Add(this.TargetTextNumberText);
            this.TabPage_05.Controls.Add(this.ConvertTextButton);
            this.TabPage_05.Controls.Add(this.groupBox2);
            this.TabPage_05.Controls.Add(this.groupBox1);
            this.TabPage_05.Location = new System.Drawing.Point(4, 76);
            this.TabPage_05.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_05.Name = "TabPage_05";
            this.TabPage_05.Size = new System.Drawing.Size(882, 659);
            this.TabPage_05.TabIndex = 4;
            this.TabPage_05.Text = "姓名横转竖列";
            this.TabPage_05.UseVisualStyleBackColor = true;
            // 
            // DoubleTextClear
            // 
            this.DoubleTextClear.AutoSize = true;
            this.DoubleTextClear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DoubleTextClear.Location = new System.Drawing.Point(397, 382);
            this.DoubleTextClear.Margin = new System.Windows.Forms.Padding(1);
            this.DoubleTextClear.Name = "DoubleTextClear";
            this.DoubleTextClear.Size = new System.Drawing.Size(72, 16);
            this.DoubleTextClear.TabIndex = 22;
            this.DoubleTextClear.Tag = "anchor_bottom";
            this.DoubleTextClear.Text = "文本去重";
            this.DoubleTextClear.UseVisualStyleBackColor = true;
            // 
            // NameMarkColor
            // 
            this.NameMarkColor.FormattingEnabled = true;
            this.NameMarkColor.Location = new System.Drawing.Point(470, 341);
            this.NameMarkColor.Margin = new System.Windows.Forms.Padding(1);
            this.NameMarkColor.Name = "NameMarkColor";
            this.NameMarkColor.Size = new System.Drawing.Size(88, 20);
            this.NameMarkColor.TabIndex = 21;
            this.NameMarkColor.Tag = "anchor_bottom";
            // 
            // MarkDupName
            // 
            this.MarkDupName.AutoSize = true;
            this.MarkDupName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MarkDupName.Location = new System.Drawing.Point(397, 345);
            this.MarkDupName.Margin = new System.Windows.Forms.Padding(1);
            this.MarkDupName.Name = "MarkDupName";
            this.MarkDupName.Size = new System.Drawing.Size(72, 16);
            this.MarkDupName.TabIndex = 20;
            this.MarkDupName.Tag = "anchor_bottom";
            this.MarkDupName.Text = "高亮重名";
            this.MarkDupName.UseVisualStyleBackColor = true;
            // 
            // ExportNameTxt
            // 
            this.ExportNameTxt.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExportNameTxt.Location = new System.Drawing.Point(685, 563);
            this.ExportNameTxt.Margin = new System.Windows.Forms.Padding(1);
            this.ExportNameTxt.Name = "ExportNameTxt";
            this.ExportNameTxt.Size = new System.Drawing.Size(183, 31);
            this.ExportNameTxt.TabIndex = 19;
            this.ExportNameTxt.Tag = "anchor_right_bottom";
            this.ExportNameTxt.Text = "导出内容到本地";
            this.ExportNameTxt.UseVisualStyleBackColor = true;
            this.ExportNameTxt.Click += new System.EventHandler(this.ExportNameTxt_Click);
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.ForeColor = System.Drawing.Color.Red;
            this.label57.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label57.Location = new System.Drawing.Point(287, 17);
            this.label57.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(241, 12);
            this.label57.TabIndex = 18;
            this.label57.Tag = "anchor_top";
            this.label57.Text = "本功能会自动删除文字内的特殊符号、⚪、▢";
            // 
            // NameReplaceRegexMode
            // 
            this.NameReplaceRegexMode.AutoSize = true;
            this.NameReplaceRegexMode.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NameReplaceRegexMode.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Underline);
            this.NameReplaceRegexMode.ForeColor = System.Drawing.Color.Red;
            this.NameReplaceRegexMode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NameReplaceRegexMode.Location = new System.Drawing.Point(492, 316);
            this.NameReplaceRegexMode.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.NameReplaceRegexMode.Name = "NameReplaceRegexMode";
            this.NameReplaceRegexMode.Size = new System.Drawing.Size(89, 12);
            this.NameReplaceRegexMode.TabIndex = 17;
            this.NameReplaceRegexMode.Tag = "anchor_bottom";
            this.NameReplaceRegexMode.Text = "[正则模式：关]";
            this.NameReplaceRegexMode.Click += new System.EventHandler(this.NameReplaceRegexMode_Click);
            // 
            // NameAutoCopyStatusText
            // 
            this.NameAutoCopyStatusText.AutoSize = true;
            this.NameAutoCopyStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NameAutoCopyStatusText.Location = new System.Drawing.Point(467, 365);
            this.NameAutoCopyStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.NameAutoCopyStatusText.Name = "NameAutoCopyStatusText";
            this.NameAutoCopyStatusText.Size = new System.Drawing.Size(0, 12);
            this.NameAutoCopyStatusText.TabIndex = 16;
            this.NameAutoCopyStatusText.Tag = "anchor_bottom";
            // 
            // NameAutoCopy
            // 
            this.NameAutoCopy.AutoSize = true;
            this.NameAutoCopy.Checked = true;
            this.NameAutoCopy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.NameAutoCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NameAutoCopy.Location = new System.Drawing.Point(397, 363);
            this.NameAutoCopy.Margin = new System.Windows.Forms.Padding(1);
            this.NameAutoCopy.Name = "NameAutoCopy";
            this.NameAutoCopy.Size = new System.Drawing.Size(72, 16);
            this.NameAutoCopy.TabIndex = 15;
            this.NameAutoCopy.Tag = "anchor_bottom";
            this.NameAutoCopy.Text = "自动复制";
            this.NameAutoCopy.UseVisualStyleBackColor = true;
            // 
            // NameReplaceWord
            // 
            this.NameReplaceWord.AutoSize = true;
            this.NameReplaceWord.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NameReplaceWord.Location = new System.Drawing.Point(324, 316);
            this.NameReplaceWord.Margin = new System.Windows.Forms.Padding(1);
            this.NameReplaceWord.Name = "NameReplaceWord";
            this.NameReplaceWord.Size = new System.Drawing.Size(72, 16);
            this.NameReplaceWord.TabIndex = 13;
            this.NameReplaceWord.Tag = "anchor_bottom";
            this.NameReplaceWord.Text = "替换文字";
            this.NameReplaceWord.UseVisualStyleBackColor = true;
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label56.Location = new System.Drawing.Point(432, 316);
            this.label56.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(17, 12);
            this.label56.TabIndex = 12;
            this.label56.Tag = "anchor_bottom";
            this.label56.Text = "为";
            // 
            // NameReplaceWord_Target
            // 
            this.NameReplaceWord_Target.AcceptsReturn = true;
            this.NameReplaceWord_Target.Location = new System.Drawing.Point(452, 312);
            this.NameReplaceWord_Target.Margin = new System.Windows.Forms.Padding(1);
            this.NameReplaceWord_Target.Multiline = true;
            this.NameReplaceWord_Target.Name = "NameReplaceWord_Target";
            this.NameReplaceWord_Target.Size = new System.Drawing.Size(35, 20);
            this.NameReplaceWord_Target.TabIndex = 11;
            this.NameReplaceWord_Target.Tag = "anchor_bottom";
            // 
            // NameReplaceWord_Search
            // 
            this.NameReplaceWord_Search.AcceptsReturn = true;
            this.NameReplaceWord_Search.Location = new System.Drawing.Point(397, 312);
            this.NameReplaceWord_Search.Margin = new System.Windows.Forms.Padding(1);
            this.NameReplaceWord_Search.Multiline = true;
            this.NameReplaceWord_Search.Name = "NameReplaceWord_Search";
            this.NameReplaceWord_Search.Size = new System.Drawing.Size(31, 20);
            this.NameReplaceWord_Search.TabIndex = 10;
            this.NameReplaceWord_Search.Tag = "anchor_bottom";
            // 
            // NameStorkeChcek
            // 
            this.NameStorkeChcek.AutoSize = true;
            this.NameStorkeChcek.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NameStorkeChcek.Location = new System.Drawing.Point(397, 400);
            this.NameStorkeChcek.Margin = new System.Windows.Forms.Padding(1);
            this.NameStorkeChcek.Name = "NameStorkeChcek";
            this.NameStorkeChcek.Size = new System.Drawing.Size(72, 16);
            this.NameStorkeChcek.TabIndex = 8;
            this.NameStorkeChcek.Tag = "anchor_bottom";
            this.NameStorkeChcek.Text = "姓氏排序";
            this.NameStorkeChcek.UseVisualStyleBackColor = true;
            // 
            // MutilLineMode
            // 
            this.MutilLineMode.AutoSize = true;
            this.MutilLineMode.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.MutilLineMode.Location = new System.Drawing.Point(397, 418);
            this.MutilLineMode.Margin = new System.Windows.Forms.Padding(1);
            this.MutilLineMode.Name = "MutilLineMode";
            this.MutilLineMode.Size = new System.Drawing.Size(72, 16);
            this.MutilLineMode.TabIndex = 7;
            this.MutilLineMode.Tag = "anchor_bottom";
            this.MutilLineMode.Text = "多行模式";
            this.MutilLineMode.UseVisualStyleBackColor = true;
            this.MutilLineMode.CheckedChanged += new System.EventHandler(this.MutilLineMode_CheckedChanged);
            // 
            // DoubleTextAlignCheck
            // 
            this.DoubleTextAlignCheck.AutoSize = true;
            this.DoubleTextAlignCheck.Checked = true;
            this.DoubleTextAlignCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.DoubleTextAlignCheck.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DoubleTextAlignCheck.Location = new System.Drawing.Point(397, 436);
            this.DoubleTextAlignCheck.Margin = new System.Windows.Forms.Padding(1);
            this.DoubleTextAlignCheck.Name = "DoubleTextAlignCheck";
            this.DoubleTextAlignCheck.Size = new System.Drawing.Size(72, 16);
            this.DoubleTextAlignCheck.TabIndex = 6;
            this.DoubleTextAlignCheck.Tag = "anchor_bottom";
            this.DoubleTextAlignCheck.Text = "双字对齐";
            this.DoubleTextAlignCheck.UseVisualStyleBackColor = true;
            // 
            // RemoveNumberCheck
            // 
            this.RemoveNumberCheck.AutoSize = true;
            this.RemoveNumberCheck.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemoveNumberCheck.Location = new System.Drawing.Point(397, 454);
            this.RemoveNumberCheck.Margin = new System.Windows.Forms.Padding(1);
            this.RemoveNumberCheck.Name = "RemoveNumberCheck";
            this.RemoveNumberCheck.Size = new System.Drawing.Size(72, 16);
            this.RemoveNumberCheck.TabIndex = 5;
            this.RemoveNumberCheck.Tag = "anchor_bottom";
            this.RemoveNumberCheck.Text = "移除数字";
            this.RemoveNumberCheck.UseVisualStyleBackColor = true;
            // 
            // RemoveSquareCheck
            // 
            this.RemoveSquareCheck.AutoSize = true;
            this.RemoveSquareCheck.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemoveSquareCheck.Location = new System.Drawing.Point(397, 472);
            this.RemoveSquareCheck.Margin = new System.Windows.Forms.Padding(1);
            this.RemoveSquareCheck.Name = "RemoveSquareCheck";
            this.RemoveSquareCheck.Size = new System.Drawing.Size(72, 16);
            this.RemoveSquareCheck.TabIndex = 4;
            this.RemoveSquareCheck.Tag = "anchor_bottom";
            this.RemoveSquareCheck.Text = "移除括号";
            this.RemoveSquareCheck.UseVisualStyleBackColor = true;
            // 
            // TargetTextNumberText
            // 
            this.TargetTextNumberText.AutoSize = true;
            this.TargetTextNumberText.ForeColor = System.Drawing.Color.ForestGreen;
            this.TargetTextNumberText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.TargetTextNumberText.Location = new System.Drawing.Point(395, 494);
            this.TargetTextNumberText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.TargetTextNumberText.Name = "TargetTextNumberText";
            this.TargetTextNumberText.Size = new System.Drawing.Size(95, 12);
            this.TargetTextNumberText.TabIndex = 3;
            this.TargetTextNumberText.Tag = "anchor_bottom";
            this.TargetTextNumberText.Text = "处理后姓名有0个";
            // 
            // ConvertTextButton
            // 
            this.ConvertTextButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertTextButton.Location = new System.Drawing.Point(379, 519);
            this.ConvertTextButton.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertTextButton.Name = "ConvertTextButton";
            this.ConvertTextButton.Size = new System.Drawing.Size(127, 29);
            this.ConvertTextButton.TabIndex = 2;
            this.ConvertTextButton.Tag = "anchor_bottom";
            this.ConvertTextButton.Text = "开始处理";
            this.ConvertTextButton.UseVisualStyleBackColor = true;
            this.ConvertTextButton.Click += new System.EventHandler(this.ConvertTextButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TargetTextBox);
            this.groupBox2.Location = new System.Drawing.Point(681, 1);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox2.Size = new System.Drawing.Size(187, 551);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Tag = "anchor_top_right_bottom";
            this.groupBox2.Text = "处理后姓名";
            // 
            // TargetTextBox
            // 
            this.TargetTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TargetTextBox.Location = new System.Drawing.Point(4, 15);
            this.TargetTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.TargetTextBox.Name = "TargetTextBox";
            this.TargetTextBox.Size = new System.Drawing.Size(181, 535);
            this.TargetTextBox.TabIndex = 3;
            this.TargetTextBox.Tag = "anchor_top_right_bottom";
            this.TargetTextBox.Text = "";
            this.TargetTextBox.TextChanged += new System.EventHandler(this.TargetTextBox_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.SourceTextBox);
            this.groupBox1.Location = new System.Drawing.Point(3, 1);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox1.Size = new System.Drawing.Size(187, 551);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Tag = "anchor_top_left_bottom";
            this.groupBox1.Text = "处理前姓名";
            // 
            // SourceTextBox
            // 
            this.SourceTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SourceTextBox.Location = new System.Drawing.Point(3, 16);
            this.SourceTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.SourceTextBox.Name = "SourceTextBox";
            this.SourceTextBox.Size = new System.Drawing.Size(181, 531);
            this.SourceTextBox.TabIndex = 2;
            this.SourceTextBox.Tag = "anchor_top_left_bottom";
            this.SourceTextBox.Text = "";
            this.SourceTextBox.TextChanged += new System.EventHandler(this.SourceTextBox_TextChanged);
            // 
            // TabPage_35
            // 
            this.TabPage_35.Controls.Add(this.groupBox39);
            this.TabPage_35.Controls.Add(this.WebpFileList_MoveDown);
            this.TabPage_35.Controls.Add(this.WebpFileList_MoveUp);
            this.TabPage_35.Controls.Add(this.webp2image_clear);
            this.TabPage_35.Controls.Add(this.webp2image_remove);
            this.TabPage_35.Controls.Add(this.webpconvert_statusText);
            this.TabPage_35.Controls.Add(this.webpPictureStartConvert);
            this.TabPage_35.Controls.Add(this.webpPictureSelect);
            this.TabPage_35.Controls.Add(this.WebpFileList);
            this.TabPage_35.Location = new System.Drawing.Point(4, 76);
            this.TabPage_35.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_35.Name = "TabPage_35";
            this.TabPage_35.Size = new System.Drawing.Size(882, 659);
            this.TabPage_35.TabIndex = 34;
            this.TabPage_35.Text = "WEBP图片格式转换";
            this.TabPage_35.UseVisualStyleBackColor = true;
            // 
            // groupBox39
            // 
            this.groupBox39.Controls.Add(this.webp2image_combox);
            this.groupBox39.Controls.Add(this.label98);
            this.groupBox39.Location = new System.Drawing.Point(4, 431);
            this.groupBox39.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox39.Name = "groupBox39";
            this.groupBox39.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox39.Size = new System.Drawing.Size(147, 63);
            this.groupBox39.TabIndex = 21;
            this.groupBox39.TabStop = false;
            this.groupBox39.Tag = "anchor_left_bottom";
            this.groupBox39.Text = "格式设置";
            // 
            // webp2image_combox
            // 
            this.webp2image_combox.FormattingEnabled = true;
            this.webp2image_combox.Location = new System.Drawing.Point(73, 25);
            this.webp2image_combox.Margin = new System.Windows.Forms.Padding(1);
            this.webp2image_combox.Name = "webp2image_combox";
            this.webp2image_combox.Size = new System.Drawing.Size(65, 20);
            this.webp2image_combox.TabIndex = 1;
            // 
            // label98
            // 
            this.label98.AutoSize = true;
            this.label98.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label98.Location = new System.Drawing.Point(15, 29);
            this.label98.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label98.Name = "label98";
            this.label98.Size = new System.Drawing.Size(53, 12);
            this.label98.TabIndex = 0;
            this.label98.Text = "目标格式";
            // 
            // WebpFileList_MoveDown
            // 
            this.WebpFileList_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.WebpFileList_MoveDown.Location = new System.Drawing.Point(847, 317);
            this.WebpFileList_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.WebpFileList_MoveDown.Name = "WebpFileList_MoveDown";
            this.WebpFileList_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.WebpFileList_MoveDown.TabIndex = 18;
            this.WebpFileList_MoveDown.Tag = "anchor_right_bottom";
            this.WebpFileList_MoveDown.Text = "▽";
            this.WebpFileList_MoveDown.UseVisualStyleBackColor = true;
            // 
            // WebpFileList_MoveUp
            // 
            this.WebpFileList_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.WebpFileList_MoveUp.Location = new System.Drawing.Point(847, 107);
            this.WebpFileList_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.WebpFileList_MoveUp.Name = "WebpFileList_MoveUp";
            this.WebpFileList_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.WebpFileList_MoveUp.TabIndex = 17;
            this.WebpFileList_MoveUp.Tag = "anchor_right_bottom";
            this.WebpFileList_MoveUp.Text = "△";
            this.WebpFileList_MoveUp.UseVisualStyleBackColor = true;
            // 
            // webp2image_clear
            // 
            this.webp2image_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.webp2image_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.webp2image_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.webp2image_clear.Location = new System.Drawing.Point(847, 247);
            this.webp2image_clear.Margin = new System.Windows.Forms.Padding(1);
            this.webp2image_clear.Name = "webp2image_clear";
            this.webp2image_clear.Size = new System.Drawing.Size(23, 57);
            this.webp2image_clear.TabIndex = 20;
            this.webp2image_clear.Tag = "anchor_right_bottom";
            this.webp2image_clear.UseVisualStyleBackColor = true;
            // 
            // webp2image_remove
            // 
            this.webp2image_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.webp2image_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.webp2image_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.webp2image_remove.Location = new System.Drawing.Point(847, 177);
            this.webp2image_remove.Margin = new System.Windows.Forms.Padding(1);
            this.webp2image_remove.Name = "webp2image_remove";
            this.webp2image_remove.Size = new System.Drawing.Size(23, 57);
            this.webp2image_remove.TabIndex = 19;
            this.webp2image_remove.Tag = "anchor_right_bottom";
            this.webp2image_remove.UseVisualStyleBackColor = true;
            // 
            // webpconvert_statusText
            // 
            this.webpconvert_statusText.AutoSize = true;
            this.webpconvert_statusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.webpconvert_statusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.webpconvert_statusText.Location = new System.Drawing.Point(156, 575);
            this.webpconvert_statusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.webpconvert_statusText.Name = "webpconvert_statusText";
            this.webpconvert_statusText.Size = new System.Drawing.Size(95, 12);
            this.webpconvert_statusText.TabIndex = 16;
            this.webpconvert_statusText.Tag = "anchor_left_bottom";
            this.webpconvert_statusText.Text = "正在处理X个文件";
            // 
            // webpPictureStartConvert
            // 
            this.webpPictureStartConvert.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.webpPictureStartConvert.Location = new System.Drawing.Point(717, 551);
            this.webpPictureStartConvert.Margin = new System.Windows.Forms.Padding(1);
            this.webpPictureStartConvert.Name = "webpPictureStartConvert";
            this.webpPictureStartConvert.Size = new System.Drawing.Size(147, 37);
            this.webpPictureStartConvert.TabIndex = 15;
            this.webpPictureStartConvert.Tag = "anchor_right_bottom";
            this.webpPictureStartConvert.Text = "开始处理";
            this.webpPictureStartConvert.UseVisualStyleBackColor = true;
            this.webpPictureStartConvert.Click += new System.EventHandler(this.webpPictureStartConvert_Click);
            // 
            // webpPictureSelect
            // 
            this.webpPictureSelect.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.webpPictureSelect.Location = new System.Drawing.Point(5, 561);
            this.webpPictureSelect.Margin = new System.Windows.Forms.Padding(1);
            this.webpPictureSelect.Name = "webpPictureSelect";
            this.webpPictureSelect.Size = new System.Drawing.Size(147, 37);
            this.webpPictureSelect.TabIndex = 14;
            this.webpPictureSelect.Tag = "anchor_left_bottom";
            this.webpPictureSelect.Text = "选择文件";
            this.webpPictureSelect.UseVisualStyleBackColor = true;
            this.webpPictureSelect.Click += new System.EventHandler(this.webpPictureSelect_Click);
            // 
            // WebpFileList
            // 
            this.WebpFileList.HideSelection = false;
            this.WebpFileList.Location = new System.Drawing.Point(4, 4);
            this.WebpFileList.Margin = new System.Windows.Forms.Padding(1);
            this.WebpFileList.Name = "WebpFileList";
            this.WebpFileList.Size = new System.Drawing.Size(841, 420);
            this.WebpFileList.TabIndex = 13;
            this.WebpFileList.Tag = "anchor_full_fill";
            this.WebpFileList.UseCompatibleStateImageBehavior = false;
            // 
            // TabPage_09
            // 
            this.TabPage_09.Controls.Add(this.groupBox8);
            this.TabPage_09.Controls.Add(this.groupBox7);
            this.TabPage_09.Controls.Add(this.groupBox6);
            this.TabPage_09.Controls.Add(this.groupBox5);
            this.TabPage_09.Location = new System.Drawing.Point(4, 76);
            this.TabPage_09.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_09.Name = "TabPage_09";
            this.TabPage_09.Size = new System.Drawing.Size(882, 659);
            this.TabPage_09.TabIndex = 8;
            this.TabPage_09.Text = "Twain扫描仪调试";
            this.TabPage_09.UseVisualStyleBackColor = true;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.ResetTotalScanNumber);
            this.groupBox8.Controls.Add(this.RemoveAllFiles);
            this.groupBox8.Controls.Add(this.SavedPathFileCount);
            this.groupBox8.Controls.Add(this.ScanSpendTime);
            this.groupBox8.Controls.Add(this.NowScanNumber);
            this.groupBox8.Controls.Add(this.ScannTotalNumber);
            this.groupBox8.Location = new System.Drawing.Point(185, 145);
            this.groupBox8.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox8.Size = new System.Drawing.Size(691, 443);
            this.groupBox8.TabIndex = 4;
            this.groupBox8.TabStop = false;
            this.groupBox8.Tag = "anchor_full_fill";
            this.groupBox8.Text = "数据信息";
            // 
            // ResetTotalScanNumber
            // 
            this.ResetTotalScanNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetTotalScanNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ResetTotalScanNumber.Location = new System.Drawing.Point(553, 411);
            this.ResetTotalScanNumber.Margin = new System.Windows.Forms.Padding(1);
            this.ResetTotalScanNumber.Name = "ResetTotalScanNumber";
            this.ResetTotalScanNumber.Size = new System.Drawing.Size(133, 27);
            this.ResetTotalScanNumber.TabIndex = 5;
            this.ResetTotalScanNumber.Tag = "anchor_right_bottom";
            this.ResetTotalScanNumber.Text = "重置扫描总数量";
            this.ResetTotalScanNumber.UseVisualStyleBackColor = true;
            this.ResetTotalScanNumber.Click += new System.EventHandler(this.ResetTotalScanNumber_Click);
            // 
            // RemoveAllFiles
            // 
            this.RemoveAllFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.RemoveAllFiles.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemoveAllFiles.Location = new System.Drawing.Point(4, 412);
            this.RemoveAllFiles.Margin = new System.Windows.Forms.Padding(1);
            this.RemoveAllFiles.Name = "RemoveAllFiles";
            this.RemoveAllFiles.Size = new System.Drawing.Size(123, 27);
            this.RemoveAllFiles.TabIndex = 4;
            this.RemoveAllFiles.Text = "清空路径全部文件";
            this.RemoveAllFiles.UseVisualStyleBackColor = true;
            this.RemoveAllFiles.Click += new System.EventHandler(this.RemoveAllFiles_Click);
            // 
            // SavedPathFileCount
            // 
            this.SavedPathFileCount.AutoSize = true;
            this.SavedPathFileCount.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SavedPathFileCount.Location = new System.Drawing.Point(4, 117);
            this.SavedPathFileCount.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SavedPathFileCount.Name = "SavedPathFileCount";
            this.SavedPathFileCount.Size = new System.Drawing.Size(125, 12);
            this.SavedPathFileCount.TabIndex = 3;
            this.SavedPathFileCount.Text = "扫描路径内文件数量：";
            // 
            // ScanSpendTime
            // 
            this.ScanSpendTime.AutoSize = true;
            this.ScanSpendTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ScanSpendTime.Location = new System.Drawing.Point(4, 87);
            this.ScanSpendTime.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ScanSpendTime.Name = "ScanSpendTime";
            this.ScanSpendTime.Size = new System.Drawing.Size(89, 12);
            this.ScanSpendTime.TabIndex = 2;
            this.ScanSpendTime.Text = "本次扫描时间：";
            // 
            // NowScanNumber
            // 
            this.NowScanNumber.AutoSize = true;
            this.NowScanNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.NowScanNumber.Location = new System.Drawing.Point(4, 57);
            this.NowScanNumber.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.NowScanNumber.Name = "NowScanNumber";
            this.NowScanNumber.Size = new System.Drawing.Size(65, 12);
            this.NowScanNumber.TabIndex = 1;
            this.NowScanNumber.Text = "本次数量：";
            // 
            // ScannTotalNumber
            // 
            this.ScannTotalNumber.AutoSize = true;
            this.ScannTotalNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ScannTotalNumber.Location = new System.Drawing.Point(4, 28);
            this.ScannTotalNumber.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ScannTotalNumber.Name = "ScannTotalNumber";
            this.ScannTotalNumber.Size = new System.Drawing.Size(77, 12);
            this.ScannTotalNumber.TabIndex = 0;
            this.ScannTotalNumber.Text = "扫描总数量：";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.PreviewPictureName);
            this.groupBox7.Controls.Add(this.PreviewPictureBox);
            this.groupBox7.Enabled = false;
            this.groupBox7.Location = new System.Drawing.Point(5, 145);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox7.Size = new System.Drawing.Size(176, 443);
            this.groupBox7.TabIndex = 3;
            this.groupBox7.TabStop = false;
            this.groupBox7.Tag = "anchor_top_left_bottom";
            this.groupBox7.Text = "扫描图片预览";
            // 
            // PreviewPictureName
            // 
            this.PreviewPictureName.AutoSize = true;
            this.PreviewPictureName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PreviewPictureName.Location = new System.Drawing.Point(4, 21);
            this.PreviewPictureName.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.PreviewPictureName.Name = "PreviewPictureName";
            this.PreviewPictureName.Size = new System.Drawing.Size(83, 12);
            this.PreviewPictureName.TabIndex = 3;
            this.PreviewPictureName.Text = "等待扫描中...";
            // 
            // PreviewPictureBox
            // 
            this.PreviewPictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewPictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.PreviewPictureBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PreviewPictureBox.Location = new System.Drawing.Point(4, 39);
            this.PreviewPictureBox.Margin = new System.Windows.Forms.Padding(1);
            this.PreviewPictureBox.Name = "PreviewPictureBox";
            this.PreviewPictureBox.Size = new System.Drawing.Size(169, 399);
            this.PreviewPictureBox.TabIndex = 2;
            this.PreviewPictureBox.TabStop = false;
            this.PreviewPictureBox.Tag = "anchor_full_fill";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.ScanSerialNumberTextBox);
            this.groupBox6.Controls.Add(this.TwainSetDeviceSaveTime);
            this.groupBox6.Controls.Add(this.ExportCaps);
            this.groupBox6.Controls.Add(this.TwainScanNumberSet);
            this.groupBox6.Controls.Add(this.label21);
            this.groupBox6.Controls.Add(this.UseInterfaceUi);
            this.groupBox6.Controls.Add(this.TwainPaperSizeBox);
            this.groupBox6.Controls.Add(this.label17);
            this.groupBox6.Controls.Add(this.SkipBlankPage);
            this.groupBox6.Controls.Add(this.ScanSerialNumber);
            this.groupBox6.Controls.Add(this.DoubleScanBox);
            this.groupBox6.Controls.Add(this.TwainDpi);
            this.groupBox6.Controls.Add(this.label16);
            this.groupBox6.Controls.Add(this.TwainSleepTimeSet);
            this.groupBox6.Controls.Add(this.label15);
            this.groupBox6.Controls.Add(this.SelectScanPath);
            this.groupBox6.Controls.Add(this.StartScanButton);
            this.groupBox6.Controls.Add(this.ScanPictureSavePathBox);
            this.groupBox6.Controls.Add(this.label14);
            this.groupBox6.Location = new System.Drawing.Point(83, 1);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox6.Size = new System.Drawing.Size(797, 131);
            this.groupBox6.TabIndex = 1;
            this.groupBox6.TabStop = false;
            this.groupBox6.Tag = "anchor_top_left_right";
            this.groupBox6.Text = "扫描配置";
            // 
            // ScanSerialNumberTextBox
            // 
            this.ScanSerialNumberTextBox.Location = new System.Drawing.Point(52, 97);
            this.ScanSerialNumberTextBox.Margin = new System.Windows.Forms.Padding(2);
            this.ScanSerialNumberTextBox.Name = "ScanSerialNumberTextBox";
            this.ScanSerialNumberTextBox.ReadOnly = true;
            this.ScanSerialNumberTextBox.Size = new System.Drawing.Size(224, 21);
            this.ScanSerialNumberTextBox.TabIndex = 17;
            // 
            // TwainSetDeviceSaveTime
            // 
            this.TwainSetDeviceSaveTime.Enabled = false;
            this.TwainSetDeviceSaveTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.TwainSetDeviceSaveTime.Location = new System.Drawing.Point(175, 45);
            this.TwainSetDeviceSaveTime.Margin = new System.Windows.Forms.Padding(1);
            this.TwainSetDeviceSaveTime.Name = "TwainSetDeviceSaveTime";
            this.TwainSetDeviceSaveTime.Size = new System.Drawing.Size(100, 21);
            this.TwainSetDeviceSaveTime.TabIndex = 16;
            this.TwainSetDeviceSaveTime.Tag = "";
            this.TwainSetDeviceSaveTime.Text = "设置休眠时间";
            this.TwainSetDeviceSaveTime.UseVisualStyleBackColor = true;
            this.TwainSetDeviceSaveTime.Click += new System.EventHandler(this.TwainSetDeviceSaveTime_Click);
            // 
            // ExportCaps
            // 
            this.ExportCaps.AutoSize = true;
            this.ExportCaps.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExportCaps.Location = new System.Drawing.Point(677, 99);
            this.ExportCaps.Margin = new System.Windows.Forms.Padding(1);
            this.ExportCaps.Name = "ExportCaps";
            this.ExportCaps.Size = new System.Drawing.Size(78, 16);
            this.ExportCaps.TabIndex = 15;
            this.ExportCaps.Tag = "anchor_right_top";
            this.ExportCaps.Text = "导出Cap项";
            this.MainToolsTip.SetToolTip(this.ExportCaps, "默认导出到程序所在目录，如果没有导出，请勾选后重新选择一次扫描仪即可");
            this.ExportCaps.UseVisualStyleBackColor = true;
            // 
            // TwainScanNumberSet
            // 
            this.TwainScanNumberSet.Location = new System.Drawing.Point(569, 97);
            this.TwainScanNumberSet.Margin = new System.Windows.Forms.Padding(1);
            this.TwainScanNumberSet.Name = "TwainScanNumberSet";
            this.TwainScanNumberSet.Size = new System.Drawing.Size(105, 21);
            this.TwainScanNumberSet.TabIndex = 14;
            this.TwainScanNumberSet.Tag = "anchor_right_top";
            this.TwainScanNumberSet.TextChanged += new System.EventHandler(this.TwainScanNumberSet_TextChanged);
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label21.Location = new System.Drawing.Point(509, 101);
            this.label21.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(53, 12);
            this.label21.TabIndex = 0;
            this.label21.Tag = "anchor_right_top";
            this.label21.Text = "扫描张数";
            // 
            // UseInterfaceUi
            // 
            this.UseInterfaceUi.AutoSize = true;
            this.UseInterfaceUi.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.UseInterfaceUi.Location = new System.Drawing.Point(677, 75);
            this.UseInterfaceUi.Margin = new System.Windows.Forms.Padding(1);
            this.UseInterfaceUi.Name = "UseInterfaceUi";
            this.UseInterfaceUi.Size = new System.Drawing.Size(84, 16);
            this.UseInterfaceUi.TabIndex = 13;
            this.UseInterfaceUi.Tag = "anchor_right_top";
            this.UseInterfaceUi.Text = "调用UI扫描";
            this.UseInterfaceUi.UseVisualStyleBackColor = true;
            // 
            // TwainPaperSizeBox
            // 
            this.TwainPaperSizeBox.Enabled = false;
            this.TwainPaperSizeBox.FormattingEnabled = true;
            this.TwainPaperSizeBox.Location = new System.Drawing.Point(569, 76);
            this.TwainPaperSizeBox.Margin = new System.Windows.Forms.Padding(1);
            this.TwainPaperSizeBox.Name = "TwainPaperSizeBox";
            this.TwainPaperSizeBox.Size = new System.Drawing.Size(105, 20);
            this.TwainPaperSizeBox.TabIndex = 12;
            this.TwainPaperSizeBox.Tag = "anchor_right_top";
            this.MainToolsTip.SetToolTip(this.TwainPaperSizeBox, "程序不会应用此处纸张尺寸设置");
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Enabled = false;
            this.label17.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label17.Location = new System.Drawing.Point(509, 79);
            this.label17.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(53, 12);
            this.label17.TabIndex = 11;
            this.label17.Tag = "anchor_right_top";
            this.label17.Text = "纸张尺寸";
            // 
            // SkipBlankPage
            // 
            this.SkipBlankPage.AutoSize = true;
            this.SkipBlankPage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SkipBlankPage.Location = new System.Drawing.Point(89, 76);
            this.SkipBlankPage.Margin = new System.Windows.Forms.Padding(1);
            this.SkipBlankPage.Name = "SkipBlankPage";
            this.SkipBlankPage.Size = new System.Drawing.Size(84, 16);
            this.SkipBlankPage.TabIndex = 10;
            this.SkipBlankPage.Text = "跳过空白页";
            this.SkipBlankPage.UseVisualStyleBackColor = true;
            // 
            // ScanSerialNumber
            // 
            this.ScanSerialNumber.AutoSize = true;
            this.ScanSerialNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ScanSerialNumber.Location = new System.Drawing.Point(5, 101);
            this.ScanSerialNumber.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ScanSerialNumber.Name = "ScanSerialNumber";
            this.ScanSerialNumber.Size = new System.Drawing.Size(41, 12);
            this.ScanSerialNumber.TabIndex = 8;
            this.ScanSerialNumber.Text = "序列号";
            // 
            // DoubleScanBox
            // 
            this.DoubleScanBox.AutoSize = true;
            this.DoubleScanBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DoubleScanBox.Location = new System.Drawing.Point(7, 77);
            this.DoubleScanBox.Margin = new System.Windows.Forms.Padding(1);
            this.DoubleScanBox.Name = "DoubleScanBox";
            this.DoubleScanBox.Size = new System.Drawing.Size(72, 16);
            this.DoubleScanBox.TabIndex = 9;
            this.DoubleScanBox.Text = "双面扫描";
            this.DoubleScanBox.UseVisualStyleBackColor = true;
            // 
            // TwainDpi
            // 
            this.TwainDpi.FormattingEnabled = true;
            this.TwainDpi.Location = new System.Drawing.Point(569, 49);
            this.TwainDpi.Margin = new System.Windows.Forms.Padding(1);
            this.TwainDpi.Name = "TwainDpi";
            this.TwainDpi.Size = new System.Drawing.Size(105, 20);
            this.TwainDpi.TabIndex = 7;
            this.TwainDpi.Tag = "anchor_right_top";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label16.Location = new System.Drawing.Point(521, 52);
            this.label16.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(41, 12);
            this.label16.TabIndex = 6;
            this.label16.Tag = "anchor_right_top";
            this.label16.Text = "分辨率";
            // 
            // TwainSleepTimeSet
            // 
            this.TwainSleepTimeSet.FormattingEnabled = true;
            this.TwainSleepTimeSet.Location = new System.Drawing.Point(61, 47);
            this.TwainSleepTimeSet.Margin = new System.Windows.Forms.Padding(1);
            this.TwainSleepTimeSet.Name = "TwainSleepTimeSet";
            this.TwainSleepTimeSet.Size = new System.Drawing.Size(112, 20);
            this.TwainSleepTimeSet.TabIndex = 5;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label15.Location = new System.Drawing.Point(4, 49);
            this.label15.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(53, 12);
            this.label15.TabIndex = 4;
            this.label15.Text = "休眠时间";
            // 
            // SelectScanPath
            // 
            this.SelectScanPath.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectScanPath.Location = new System.Drawing.Point(636, 17);
            this.SelectScanPath.Margin = new System.Windows.Forms.Padding(1);
            this.SelectScanPath.Name = "SelectScanPath";
            this.SelectScanPath.Size = new System.Drawing.Size(81, 22);
            this.SelectScanPath.TabIndex = 3;
            this.SelectScanPath.Tag = "anchor_right_top";
            this.SelectScanPath.Text = "设置路径";
            this.SelectScanPath.UseVisualStyleBackColor = true;
            this.SelectScanPath.Click += new System.EventHandler(this.SelectScanPath_Click);
            // 
            // StartScanButton
            // 
            this.StartScanButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StartScanButton.Location = new System.Drawing.Point(677, 47);
            this.StartScanButton.Margin = new System.Windows.Forms.Padding(1);
            this.StartScanButton.Name = "StartScanButton";
            this.StartScanButton.Size = new System.Drawing.Size(79, 21);
            this.StartScanButton.TabIndex = 2;
            this.StartScanButton.Tag = "anchor_right_top";
            this.StartScanButton.Text = "开始扫描";
            this.StartScanButton.UseVisualStyleBackColor = true;
            this.StartScanButton.Click += new System.EventHandler(this.StartScanButton_Click);
            // 
            // ScanPictureSavePathBox
            // 
            this.ScanPictureSavePathBox.Location = new System.Drawing.Point(87, 19);
            this.ScanPictureSavePathBox.Margin = new System.Windows.Forms.Padding(1);
            this.ScanPictureSavePathBox.Name = "ScanPictureSavePathBox";
            this.ScanPictureSavePathBox.Size = new System.Drawing.Size(547, 21);
            this.ScanPictureSavePathBox.TabIndex = 1;
            this.ScanPictureSavePathBox.Tag = "anchor_top_left_right";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label14.Location = new System.Drawing.Point(4, 23);
            this.label14.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(77, 12);
            this.label14.TabIndex = 0;
            this.label14.Text = "扫描图片路径";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.SetTwainDevice);
            this.groupBox5.Controls.Add(this.SelectTwainDevice);
            this.groupBox5.Location = new System.Drawing.Point(1, 1);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox5.Size = new System.Drawing.Size(77, 131);
            this.groupBox5.TabIndex = 0;
            this.groupBox5.TabStop = false;
            this.groupBox5.Tag = "anchor_left_top";
            this.groupBox5.Text = "TWAIN设置";
            // 
            // SetTwainDevice
            // 
            this.SetTwainDevice.AutoSize = true;
            this.SetTwainDevice.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SetTwainDevice.Location = new System.Drawing.Point(5, 87);
            this.SetTwainDevice.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SetTwainDevice.Name = "SetTwainDevice";
            this.SetTwainDevice.Size = new System.Drawing.Size(65, 12);
            this.SetTwainDevice.TabIndex = 1;
            this.SetTwainDevice.TabStop = true;
            this.SetTwainDevice.Text = "扫描仪设置";
            this.SetTwainDevice.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SetTwainDevice_LinkClicked);
            // 
            // SelectTwainDevice
            // 
            this.SelectTwainDevice.AutoSize = true;
            this.SelectTwainDevice.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectTwainDevice.Location = new System.Drawing.Point(4, 28);
            this.SelectTwainDevice.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.SelectTwainDevice.Name = "SelectTwainDevice";
            this.SelectTwainDevice.Size = new System.Drawing.Size(65, 12);
            this.SelectTwainDevice.TabIndex = 0;
            this.SelectTwainDevice.TabStop = true;
            this.SelectTwainDevice.Text = "选择扫描仪";
            this.SelectTwainDevice.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.SelectTwainDevice_LinkClicked);
            // 
            // TabPage_13
            // 
            this.TabPage_13.Controls.Add(this.groupBox64);
            this.TabPage_13.Controls.Add(this.groupBox63);
            this.TabPage_13.Controls.Add(this.label20);
            this.TabPage_13.Controls.Add(this.groupBox17);
            this.TabPage_13.Controls.Add(this.groupBox16);
            this.TabPage_13.Controls.Add(this.groupBox15);
            this.TabPage_13.Location = new System.Drawing.Point(4, 76);
            this.TabPage_13.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_13.Name = "TabPage_13";
            this.TabPage_13.Size = new System.Drawing.Size(882, 659);
            this.TabPage_13.TabIndex = 12;
            this.TabPage_13.Text = "其他功能";
            this.TabPage_13.UseVisualStyleBackColor = true;
            // 
            // groupBox64
            // 
            this.groupBox64.Controls.Add(this.ReleaseRenamerReplaceConfig);
            this.groupBox64.Controls.Add(this.ReleaseRenamerApp);
            this.groupBox64.Location = new System.Drawing.Point(595, 1);
            this.groupBox64.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox64.Name = "groupBox64";
            this.groupBox64.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox64.Size = new System.Drawing.Size(200, 77);
            this.groupBox64.TabIndex = 0;
            this.groupBox64.TabStop = false;
            this.groupBox64.Tag = "anchor_left_top";
            this.groupBox64.Text = "释放RENAMER本体和规则";
            // 
            // ReleaseRenamerReplaceConfig
            // 
            this.ReleaseRenamerReplaceConfig.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReleaseRenamerReplaceConfig.Location = new System.Drawing.Point(111, 25);
            this.ReleaseRenamerReplaceConfig.Margin = new System.Windows.Forms.Padding(1);
            this.ReleaseRenamerReplaceConfig.Name = "ReleaseRenamerReplaceConfig";
            this.ReleaseRenamerReplaceConfig.Size = new System.Drawing.Size(87, 27);
            this.ReleaseRenamerReplaceConfig.TabIndex = 0;
            this.ReleaseRenamerReplaceConfig.Text = "释放规则";
            this.ReleaseRenamerReplaceConfig.UseVisualStyleBackColor = true;
            this.ReleaseRenamerReplaceConfig.Click += new System.EventHandler(this.ReleaseRenamerReplaceConfig_Click);
            // 
            // ReleaseRenamerApp
            // 
            this.ReleaseRenamerApp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReleaseRenamerApp.Location = new System.Drawing.Point(4, 25);
            this.ReleaseRenamerApp.Margin = new System.Windows.Forms.Padding(1);
            this.ReleaseRenamerApp.Name = "ReleaseRenamerApp";
            this.ReleaseRenamerApp.Size = new System.Drawing.Size(87, 27);
            this.ReleaseRenamerApp.TabIndex = 1;
            this.ReleaseRenamerApp.Text = "释放程序";
            this.ReleaseRenamerApp.UseVisualStyleBackColor = true;
            this.ReleaseRenamerApp.Click += new System.EventHandler(this.ReleaseRenamerApp_Click);
            // 
            // groupBox63
            // 
            this.groupBox63.Controls.Add(this.ReleaseKMSAIO);
            this.groupBox63.Location = new System.Drawing.Point(475, 1);
            this.groupBox63.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox63.Name = "groupBox63";
            this.groupBox63.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox63.Size = new System.Drawing.Size(115, 77);
            this.groupBox63.TabIndex = 1;
            this.groupBox63.TabStop = false;
            this.groupBox63.Tag = "anchor_left_top";
            this.groupBox63.Text = "释放KMS激活脚本";
            // 
            // ReleaseKMSAIO
            // 
            this.ReleaseKMSAIO.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReleaseKMSAIO.Location = new System.Drawing.Point(11, 25);
            this.ReleaseKMSAIO.Margin = new System.Windows.Forms.Padding(1);
            this.ReleaseKMSAIO.Name = "ReleaseKMSAIO";
            this.ReleaseKMSAIO.Size = new System.Drawing.Size(87, 27);
            this.ReleaseKMSAIO.TabIndex = 0;
            this.ReleaseKMSAIO.Text = "点击释放";
            this.ReleaseKMSAIO.UseVisualStyleBackColor = true;
            this.ReleaseKMSAIO.Click += new System.EventHandler(this.ReleaseKMSAIO_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.ForeColor = System.Drawing.Color.Tomato;
            this.label20.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label20.Location = new System.Drawing.Point(7, 597);
            this.label20.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(293, 12);
            this.label20.TabIndex = 2;
            this.label20.Tag = "anchor_left_bottom";
            this.label20.Text = "提示：所有释放和下载文件都会存放在软件所在目录下";
            // 
            // groupBox17
            // 
            this.groupBox17.Controls.Add(this.image2ico_remove);
            this.groupBox17.Controls.Add(this.IcoFileView_MoveDown);
            this.groupBox17.Controls.Add(this.IcoFileView_MoveUp);
            this.groupBox17.Controls.Add(this.image2ico_clear);
            this.groupBox17.Controls.Add(this.IcoSizeDrop);
            this.groupBox17.Controls.Add(this.label22);
            this.groupBox17.Controls.Add(this.Image2IcoStatusText);
            this.groupBox17.Controls.Add(this.ConvertImage2Ico);
            this.groupBox17.Controls.Add(this.SelectFilesIco);
            this.groupBox17.Controls.Add(this.IcoFileView);
            this.groupBox17.Location = new System.Drawing.Point(7, 84);
            this.groupBox17.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox17.Name = "groupBox17";
            this.groupBox17.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox17.Size = new System.Drawing.Size(871, 487);
            this.groupBox17.TabIndex = 3;
            this.groupBox17.TabStop = false;
            this.groupBox17.Tag = "anchor_full_fill";
            this.groupBox17.Text = "ICO图标转换";
            // 
            // image2ico_remove
            // 
            this.image2ico_remove.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.image2ico_remove.BackgroundImage = global::DocumentTools.Properties.Resources.Remove;
            this.image2ico_remove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.image2ico_remove.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.image2ico_remove.Location = new System.Drawing.Point(841, 147);
            this.image2ico_remove.Margin = new System.Windows.Forms.Padding(1);
            this.image2ico_remove.Name = "image2ico_remove";
            this.image2ico_remove.Size = new System.Drawing.Size(23, 57);
            this.image2ico_remove.TabIndex = 13;
            this.image2ico_remove.Tag = "anchor_right";
            // 
            // IcoFileView_MoveDown
            // 
            this.IcoFileView_MoveDown.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.IcoFileView_MoveDown.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.IcoFileView_MoveDown.Location = new System.Drawing.Point(841, 312);
            this.IcoFileView_MoveDown.Margin = new System.Windows.Forms.Padding(1);
            this.IcoFileView_MoveDown.Name = "IcoFileView_MoveDown";
            this.IcoFileView_MoveDown.Size = new System.Drawing.Size(23, 57);
            this.IcoFileView_MoveDown.TabIndex = 7;
            this.IcoFileView_MoveDown.Tag = "anchor_right";
            this.IcoFileView_MoveDown.Text = "▽";
            this.IcoFileView_MoveDown.UseVisualStyleBackColor = true;
            // 
            // IcoFileView_MoveUp
            // 
            this.IcoFileView_MoveUp.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.IcoFileView_MoveUp.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.IcoFileView_MoveUp.Location = new System.Drawing.Point(841, 72);
            this.IcoFileView_MoveUp.Margin = new System.Windows.Forms.Padding(1);
            this.IcoFileView_MoveUp.Name = "IcoFileView_MoveUp";
            this.IcoFileView_MoveUp.Size = new System.Drawing.Size(23, 57);
            this.IcoFileView_MoveUp.TabIndex = 6;
            this.IcoFileView_MoveUp.Tag = "anchor_right";
            this.IcoFileView_MoveUp.Text = "△";
            this.IcoFileView_MoveUp.UseVisualStyleBackColor = true;
            // 
            // image2ico_clear
            // 
            this.image2ico_clear.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.image2ico_clear.BackgroundImage = global::DocumentTools.Properties.Resources.Clear;
            this.image2ico_clear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.image2ico_clear.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.image2ico_clear.Location = new System.Drawing.Point(841, 228);
            this.image2ico_clear.Margin = new System.Windows.Forms.Padding(1);
            this.image2ico_clear.Name = "image2ico_clear";
            this.image2ico_clear.Size = new System.Drawing.Size(23, 57);
            this.image2ico_clear.TabIndex = 12;
            this.image2ico_clear.Tag = "anchor_right";
            this.image2ico_clear.UseVisualStyleBackColor = true;
            // 
            // IcoSizeDrop
            // 
            this.IcoSizeDrop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.IcoSizeDrop.FormattingEnabled = true;
            this.IcoSizeDrop.Location = new System.Drawing.Point(439, 429);
            this.IcoSizeDrop.Margin = new System.Windows.Forms.Padding(1);
            this.IcoSizeDrop.Name = "IcoSizeDrop";
            this.IcoSizeDrop.Size = new System.Drawing.Size(92, 20);
            this.IcoSizeDrop.TabIndex = 5;
            this.IcoSizeDrop.Tag = "anchor_left_bottom";
            // 
            // label22
            // 
            this.label22.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label22.AutoSize = true;
            this.label22.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label22.Location = new System.Drawing.Point(340, 431);
            this.label22.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(95, 12);
            this.label22.TabIndex = 4;
            this.label22.Tag = "anchor_left_bottom";
            this.label22.Text = "设置ICO图标大小";
            // 
            // Image2IcoStatusText
            // 
            this.Image2IcoStatusText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Image2IcoStatusText.AutoSize = true;
            this.Image2IcoStatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.Image2IcoStatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Image2IcoStatusText.Location = new System.Drawing.Point(199, 455);
            this.Image2IcoStatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Image2IcoStatusText.Name = "Image2IcoStatusText";
            this.Image2IcoStatusText.Size = new System.Drawing.Size(83, 12);
            this.Image2IcoStatusText.TabIndex = 3;
            this.Image2IcoStatusText.Tag = "anchor_left_bottom";
            this.Image2IcoStatusText.Text = "等待处理中...";
            // 
            // ConvertImage2Ico
            // 
            this.ConvertImage2Ico.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ConvertImage2Ico.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ConvertImage2Ico.Location = new System.Drawing.Point(775, 447);
            this.ConvertImage2Ico.Margin = new System.Windows.Forms.Padding(1);
            this.ConvertImage2Ico.Name = "ConvertImage2Ico";
            this.ConvertImage2Ico.Size = new System.Drawing.Size(91, 29);
            this.ConvertImage2Ico.TabIndex = 2;
            this.ConvertImage2Ico.Tag = "anchor_right_bottom";
            this.ConvertImage2Ico.Text = "开始处理";
            this.ConvertImage2Ico.UseVisualStyleBackColor = true;
            this.ConvertImage2Ico.Click += new System.EventHandler(this.ConvertImage2Ico_Click);
            // 
            // SelectFilesIco
            // 
            this.SelectFilesIco.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelectFilesIco.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectFilesIco.Location = new System.Drawing.Point(1, 447);
            this.SelectFilesIco.Margin = new System.Windows.Forms.Padding(1);
            this.SelectFilesIco.Name = "SelectFilesIco";
            this.SelectFilesIco.Size = new System.Drawing.Size(91, 29);
            this.SelectFilesIco.TabIndex = 1;
            this.SelectFilesIco.Tag = "anchor_left_bottom";
            this.SelectFilesIco.Text = "选择文件";
            this.SelectFilesIco.UseVisualStyleBackColor = true;
            this.SelectFilesIco.Click += new System.EventHandler(this.SelectFilesIco_Click);
            // 
            // IcoFileView
            // 
            this.IcoFileView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.IcoFileView.HideSelection = false;
            this.IcoFileView.Location = new System.Drawing.Point(4, 17);
            this.IcoFileView.Margin = new System.Windows.Forms.Padding(1);
            this.IcoFileView.Name = "IcoFileView";
            this.IcoFileView.Size = new System.Drawing.Size(837, 408);
            this.IcoFileView.TabIndex = 0;
            this.IcoFileView.Tag = "anchor_full_fill";
            this.IcoFileView.UseCompatibleStateImageBehavior = false;
            // 
            // groupBox16
            // 
            this.groupBox16.Controls.Add(this.ReleaseRegistry);
            this.groupBox16.Location = new System.Drawing.Point(372, 1);
            this.groupBox16.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox16.Name = "groupBox16";
            this.groupBox16.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox16.Size = new System.Drawing.Size(99, 77);
            this.groupBox16.TabIndex = 1;
            this.groupBox16.TabStop = false;
            this.groupBox16.Tag = "anchor_left_top";
            this.groupBox16.Text = "释放注册文件";
            // 
            // ReleaseRegistry
            // 
            this.ReleaseRegistry.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ReleaseRegistry.Location = new System.Drawing.Point(4, 25);
            this.ReleaseRegistry.Margin = new System.Windows.Forms.Padding(1);
            this.ReleaseRegistry.Name = "ReleaseRegistry";
            this.ReleaseRegistry.Size = new System.Drawing.Size(87, 27);
            this.ReleaseRegistry.TabIndex = 0;
            this.ReleaseRegistry.Text = "点击释放";
            this.ReleaseRegistry.UseVisualStyleBackColor = true;
            this.ReleaseRegistry.Click += new System.EventHandler(this.ReleaseRegistry_Click);
            // 
            // groupBox15
            // 
            this.groupBox15.Controls.Add(this.LanzouNetfx);
            this.groupBox15.Controls.Add(this.ProcessLabel);
            this.groupBox15.Controls.Add(this.Downloadfx35Progress);
            this.groupBox15.Controls.Add(this.DownloadNetfx35);
            this.groupBox15.Location = new System.Drawing.Point(3, 1);
            this.groupBox15.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox15.Name = "groupBox15";
            this.groupBox15.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox15.Size = new System.Drawing.Size(364, 77);
            this.groupBox15.TabIndex = 0;
            this.groupBox15.TabStop = false;
            this.groupBox15.Tag = "anchor_left_top";
            this.groupBox15.Text = "下载.Net3.5离线包";
            // 
            // LanzouNetfx
            // 
            this.LanzouNetfx.AutoSize = true;
            this.LanzouNetfx.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.LanzouNetfx.Location = new System.Drawing.Point(251, 53);
            this.LanzouNetfx.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.LanzouNetfx.Name = "LanzouNetfx";
            this.LanzouNetfx.Size = new System.Drawing.Size(101, 12);
            this.LanzouNetfx.TabIndex = 3;
            this.LanzouNetfx.TabStop = true;
            this.LanzouNetfx.Text = "前往蓝奏网盘下载";
            this.LanzouNetfx.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LanzouNetfx_LinkClicked);
            // 
            // ProcessLabel
            // 
            this.ProcessLabel.AutoSize = true;
            this.ProcessLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ProcessLabel.Location = new System.Drawing.Point(157, 25);
            this.ProcessLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ProcessLabel.Name = "ProcessLabel";
            this.ProcessLabel.Size = new System.Drawing.Size(77, 12);
            this.ProcessLabel.TabIndex = 2;
            this.ProcessLabel.Text = "下载进度：0%";
            // 
            // Downloadfx35Progress
            // 
            this.Downloadfx35Progress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Downloadfx35Progress.Location = new System.Drawing.Point(4, 47);
            this.Downloadfx35Progress.Margin = new System.Windows.Forms.Padding(1);
            this.Downloadfx35Progress.Name = "Downloadfx35Progress";
            this.Downloadfx35Progress.Size = new System.Drawing.Size(237, 25);
            this.Downloadfx35Progress.TabIndex = 1;
            // 
            // DownloadNetfx35
            // 
            this.DownloadNetfx35.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DownloadNetfx35.Location = new System.Drawing.Point(4, 17);
            this.DownloadNetfx35.Margin = new System.Windows.Forms.Padding(1);
            this.DownloadNetfx35.Name = "DownloadNetfx35";
            this.DownloadNetfx35.Size = new System.Drawing.Size(149, 25);
            this.DownloadNetfx35.TabIndex = 0;
            this.DownloadNetfx35.Text = "开始下载（从私有网盘）";
            this.DownloadNetfx35.UseVisualStyleBackColor = true;
            this.DownloadNetfx35.Click += new System.EventHandler(this.DownloadNetfx35_Click);
            // 
            // TabPage_26
            // 
            this.TabPage_26.Controls.Add(this.blockgroup);
            this.TabPage_26.Controls.Add(this.groupBox27);
            this.TabPage_26.Controls.Add(this.groupBox26);
            this.TabPage_26.Location = new System.Drawing.Point(4, 76);
            this.TabPage_26.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_26.Name = "TabPage_26";
            this.TabPage_26.Size = new System.Drawing.Size(882, 659);
            this.TabPage_26.TabIndex = 25;
            this.TabPage_26.Text = "系统工具";
            this.TabPage_26.UseVisualStyleBackColor = true;
            // 
            // blockgroup
            // 
            this.blockgroup.Controls.Add(this.OpenSystemUpdate);
            this.blockgroup.Controls.Add(this.SetPauseTime);
            this.blockgroup.Controls.Add(this.PauseUpdateYears);
            this.blockgroup.Controls.Add(this.label153);
            this.blockgroup.Font = new System.Drawing.Font("宋体", 9F);
            this.blockgroup.Location = new System.Drawing.Point(587, 1);
            this.blockgroup.Margin = new System.Windows.Forms.Padding(1);
            this.blockgroup.Name = "blockgroup";
            this.blockgroup.Padding = new System.Windows.Forms.Padding(1);
            this.blockgroup.Size = new System.Drawing.Size(281, 137);
            this.blockgroup.TabIndex = 5;
            this.blockgroup.TabStop = false;
            this.blockgroup.Text = "暂停更新（暂时支持Win10和Win11）";
            // 
            // OpenSystemUpdate
            // 
            this.OpenSystemUpdate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.OpenSystemUpdate.Location = new System.Drawing.Point(7, 99);
            this.OpenSystemUpdate.Margin = new System.Windows.Forms.Padding(1);
            this.OpenSystemUpdate.Name = "OpenSystemUpdate";
            this.OpenSystemUpdate.Size = new System.Drawing.Size(97, 29);
            this.OpenSystemUpdate.TabIndex = 3;
            this.OpenSystemUpdate.Text = "打开系统设置";
            this.OpenSystemUpdate.UseVisualStyleBackColor = true;
            this.OpenSystemUpdate.Click += new System.EventHandler(this.OpenSystemUpdate_Click);
            // 
            // SetPauseTime
            // 
            this.SetPauseTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SetPauseTime.Location = new System.Drawing.Point(173, 97);
            this.SetPauseTime.Margin = new System.Windows.Forms.Padding(1);
            this.SetPauseTime.Name = "SetPauseTime";
            this.SetPauseTime.Size = new System.Drawing.Size(97, 29);
            this.SetPauseTime.TabIndex = 2;
            this.SetPauseTime.Text = "设置暂停时间";
            this.SetPauseTime.UseVisualStyleBackColor = true;
            this.SetPauseTime.Click += new System.EventHandler(this.SetPauseTime_Click);
            // 
            // PauseUpdateYears
            // 
            this.PauseUpdateYears.FormattingEnabled = true;
            this.PauseUpdateYears.Location = new System.Drawing.Point(129, 37);
            this.PauseUpdateYears.Margin = new System.Windows.Forms.Padding(1);
            this.PauseUpdateYears.Name = "PauseUpdateYears";
            this.PauseUpdateYears.Size = new System.Drawing.Size(143, 20);
            this.PauseUpdateYears.TabIndex = 1;
            // 
            // label153
            // 
            this.label153.AutoSize = true;
            this.label153.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label153.Location = new System.Drawing.Point(4, 40);
            this.label153.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label153.Name = "label153";
            this.label153.Size = new System.Drawing.Size(101, 12);
            this.label153.TabIndex = 0;
            this.label153.Text = "设置最大暂停时间";
            // 
            // groupBox27
            // 
            this.groupBox27.Controls.Add(this.AutoRestartExplorer);
            this.groupBox27.Controls.Add(this.ShortStatus);
            this.groupBox27.Controls.Add(this.RestoreShort);
            this.groupBox27.Controls.Add(this.RemoveShort);
            this.groupBox27.Font = new System.Drawing.Font("宋体", 9F);
            this.groupBox27.Location = new System.Drawing.Point(291, 1);
            this.groupBox27.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox27.Name = "groupBox27";
            this.groupBox27.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox27.Size = new System.Drawing.Size(292, 137);
            this.groupBox27.TabIndex = 3;
            this.groupBox27.TabStop = false;
            this.groupBox27.Text = "快捷方式小箭头";
            // 
            // AutoRestartExplorer
            // 
            this.AutoRestartExplorer.AutoSize = true;
            this.AutoRestartExplorer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.AutoRestartExplorer.Location = new System.Drawing.Point(157, 105);
            this.AutoRestartExplorer.Margin = new System.Windows.Forms.Padding(1);
            this.AutoRestartExplorer.Name = "AutoRestartExplorer";
            this.AutoRestartExplorer.Size = new System.Drawing.Size(132, 16);
            this.AutoRestartExplorer.TabIndex = 3;
            this.AutoRestartExplorer.Text = "自动重启资源管理器";
            this.AutoRestartExplorer.UseVisualStyleBackColor = true;
            // 
            // ShortStatus
            // 
            this.ShortStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.ShortStatus.AutoSize = true;
            this.ShortStatus.Font = new System.Drawing.Font("宋体", 16F);
            this.ShortStatus.ForeColor = System.Drawing.Color.Red;
            this.ShortStatus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ShortStatus.Location = new System.Drawing.Point(21, 99);
            this.ShortStatus.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.ShortStatus.Name = "ShortStatus";
            this.ShortStatus.Size = new System.Drawing.Size(54, 22);
            this.ShortStatus.TabIndex = 2;
            this.ShortStatus.Text = "快捷";
            // 
            // RestoreShort
            // 
            this.RestoreShort.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.RestoreShort.Font = new System.Drawing.Font("宋体", 9F);
            this.RestoreShort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RestoreShort.Location = new System.Drawing.Point(163, 39);
            this.RestoreShort.Margin = new System.Windows.Forms.Padding(1);
            this.RestoreShort.Name = "RestoreShort";
            this.RestoreShort.Size = new System.Drawing.Size(103, 41);
            this.RestoreShort.TabIndex = 1;
            this.RestoreShort.Text = "恢复箭头";
            this.RestoreShort.UseVisualStyleBackColor = true;
            this.RestoreShort.Click += new System.EventHandler(this.RestoreShort_Click);
            // 
            // RemoveShort
            // 
            this.RemoveShort.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.RemoveShort.Font = new System.Drawing.Font("宋体", 9F);
            this.RemoveShort.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RemoveShort.Location = new System.Drawing.Point(9, 39);
            this.RemoveShort.Margin = new System.Windows.Forms.Padding(1);
            this.RemoveShort.Name = "RemoveShort";
            this.RemoveShort.Size = new System.Drawing.Size(103, 41);
            this.RemoveShort.TabIndex = 0;
            this.RemoveShort.Text = "移除箭头";
            this.RemoveShort.UseVisualStyleBackColor = true;
            this.RemoveShort.Click += new System.EventHandler(this.RemoveShort_Click);
            // 
            // groupBox26
            // 
            this.groupBox26.Controls.Add(this.KeyBoardLockStatus);
            this.groupBox26.Controls.Add(this.UnlockKeyBoard);
            this.groupBox26.Controls.Add(this.LockKeyBoard);
            this.groupBox26.Font = new System.Drawing.Font("宋体", 9F);
            this.groupBox26.Location = new System.Drawing.Point(5, 1);
            this.groupBox26.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox26.Name = "groupBox26";
            this.groupBox26.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox26.Size = new System.Drawing.Size(281, 137);
            this.groupBox26.TabIndex = 0;
            this.groupBox26.TabStop = false;
            this.groupBox26.Text = "键盘工具";
            // 
            // KeyBoardLockStatus
            // 
            this.KeyBoardLockStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.KeyBoardLockStatus.AutoSize = true;
            this.KeyBoardLockStatus.Font = new System.Drawing.Font("宋体", 16F);
            this.KeyBoardLockStatus.ForeColor = System.Drawing.Color.Green;
            this.KeyBoardLockStatus.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.KeyBoardLockStatus.Location = new System.Drawing.Point(99, 97);
            this.KeyBoardLockStatus.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.KeyBoardLockStatus.Name = "KeyBoardLockStatus";
            this.KeyBoardLockStatus.Size = new System.Drawing.Size(76, 22);
            this.KeyBoardLockStatus.TabIndex = 2;
            this.KeyBoardLockStatus.Text = "已解锁";
            // 
            // UnlockKeyBoard
            // 
            this.UnlockKeyBoard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.UnlockKeyBoard.Font = new System.Drawing.Font("宋体", 9F);
            this.UnlockKeyBoard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.UnlockKeyBoard.Location = new System.Drawing.Point(159, 39);
            this.UnlockKeyBoard.Margin = new System.Windows.Forms.Padding(1);
            this.UnlockKeyBoard.Name = "UnlockKeyBoard";
            this.UnlockKeyBoard.Size = new System.Drawing.Size(103, 41);
            this.UnlockKeyBoard.TabIndex = 1;
            this.UnlockKeyBoard.Text = "解锁键盘";
            this.UnlockKeyBoard.UseVisualStyleBackColor = true;
            this.UnlockKeyBoard.Click += new System.EventHandler(this.UnlockKeyBoard_Click);
            // 
            // LockKeyBoard
            // 
            this.LockKeyBoard.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.LockKeyBoard.Font = new System.Drawing.Font("宋体", 9F);
            this.LockKeyBoard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.LockKeyBoard.Location = new System.Drawing.Point(3, 39);
            this.LockKeyBoard.Margin = new System.Windows.Forms.Padding(1);
            this.LockKeyBoard.Name = "LockKeyBoard";
            this.LockKeyBoard.Size = new System.Drawing.Size(103, 41);
            this.LockKeyBoard.TabIndex = 0;
            this.LockKeyBoard.Text = "锁定键盘";
            this.LockKeyBoard.UseVisualStyleBackColor = true;
            this.LockKeyBoard.Click += new System.EventHandler(this.LockKeyBoard_Click);
            // 
            // TabPage_32
            // 
            this.TabPage_32.Controls.Add(this.groupBox32);
            this.TabPage_32.Location = new System.Drawing.Point(4, 76);
            this.TabPage_32.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_32.Name = "TabPage_32";
            this.TabPage_32.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_32.Size = new System.Drawing.Size(882, 659);
            this.TabPage_32.TabIndex = 31;
            this.TabPage_32.Text = "MSS3模板修改";
            this.TabPage_32.UseVisualStyleBackColor = true;
            // 
            // groupBox32
            // 
            this.groupBox32.Controls.Add(this.groupBox40);
            this.groupBox32.Controls.Add(this.linkLabel6);
            this.groupBox32.Controls.Add(this.groupBox33);
            this.groupBox32.Controls.Add(this.votestatustext);
            this.groupBox32.Controls.Add(this.StartInsert);
            this.groupBox32.Controls.Add(this.SelectVotePic);
            this.groupBox32.Controls.Add(this.VotePicture);
            this.groupBox32.Controls.Add(this.SelectMss3);
            this.groupBox32.Controls.Add(this.Mss3Path);
            this.groupBox32.Controls.Add(this.label75);
            this.groupBox32.Location = new System.Drawing.Point(5, 4);
            this.groupBox32.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox32.Name = "groupBox32";
            this.groupBox32.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox32.Size = new System.Drawing.Size(879, 648);
            this.groupBox32.TabIndex = 0;
            this.groupBox32.TabStop = false;
            this.groupBox32.Tag = "anchor_full_fill";
            this.groupBox32.Text = "系统配置";
            // 
            // groupBox40
            // 
            this.groupBox40.Controls.Add(this.votedataview);
            this.groupBox40.Location = new System.Drawing.Point(193, 59);
            this.groupBox40.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox40.Name = "groupBox40";
            this.groupBox40.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox40.Size = new System.Drawing.Size(671, 325);
            this.groupBox40.TabIndex = 10;
            this.groupBox40.TabStop = false;
            this.groupBox40.Tag = "anchor_full_fill";
            this.groupBox40.Text = "选票模板信息概览";
            // 
            // votedataview
            // 
            this.votedataview.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.votedataview.Location = new System.Drawing.Point(4, 17);
            this.votedataview.Margin = new System.Windows.Forms.Padding(1);
            this.votedataview.Name = "votedataview";
            this.votedataview.RowHeadersWidth = 62;
            this.votedataview.RowTemplate.Height = 30;
            this.votedataview.Size = new System.Drawing.Size(663, 297);
            this.votedataview.TabIndex = 0;
            this.votedataview.Tag = "";
            // 
            // linkLabel6
            // 
            this.linkLabel6.AutoSize = true;
            this.linkLabel6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabel6.Location = new System.Drawing.Point(197, 596);
            this.linkLabel6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.linkLabel6.Name = "linkLabel6";
            this.linkLabel6.Size = new System.Drawing.Size(281, 12);
            this.linkLabel6.TabIndex = 17;
            this.linkLabel6.TabStop = true;
            this.linkLabel6.Tag = "anchor_left_bottom";
            this.linkLabel6.Text = "功能需要AccessDataBaseEngine组件，点击前往下载";
            this.linkLabel6.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel6_LinkClicked);
            // 
            // groupBox33
            // 
            this.groupBox33.Controls.Add(this.votepageIndexCombox);
            this.groupBox33.Controls.Add(this.votepage_type_combox);
            this.groupBox33.Controls.Add(this.label105);
            this.groupBox33.Controls.Add(this.label95);
            this.groupBox33.Controls.Add(this.voteindex);
            this.groupBox33.Controls.Add(this.label78);
            this.groupBox33.Controls.Add(this.votename);
            this.groupBox33.Controls.Add(this.label77);
            this.groupBox33.Controls.Add(this.label76);
            this.groupBox33.Controls.Add(this.voteid);
            this.groupBox33.Location = new System.Drawing.Point(7, 59);
            this.groupBox33.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox33.Name = "groupBox33";
            this.groupBox33.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox33.Size = new System.Drawing.Size(184, 167);
            this.groupBox33.TabIndex = 9;
            this.groupBox33.TabStop = false;
            this.groupBox33.Text = "参数设置";
            // 
            // votepageIndexCombox
            // 
            this.votepageIndexCombox.FormattingEnabled = true;
            this.votepageIndexCombox.Location = new System.Drawing.Point(61, 83);
            this.votepageIndexCombox.Margin = new System.Windows.Forms.Padding(1);
            this.votepageIndexCombox.Name = "votepageIndexCombox";
            this.votepageIndexCombox.Size = new System.Drawing.Size(120, 20);
            this.votepageIndexCombox.TabIndex = 11;
            // 
            // votepage_type_combox
            // 
            this.votepage_type_combox.FormattingEnabled = true;
            this.votepage_type_combox.Location = new System.Drawing.Point(61, 140);
            this.votepage_type_combox.Margin = new System.Windows.Forms.Padding(1);
            this.votepage_type_combox.Name = "votepage_type_combox";
            this.votepage_type_combox.Size = new System.Drawing.Size(120, 20);
            this.votepage_type_combox.TabIndex = 16;
            // 
            // label105
            // 
            this.label105.AutoSize = true;
            this.label105.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label105.Location = new System.Drawing.Point(4, 143);
            this.label105.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label105.Name = "label105";
            this.label105.Size = new System.Drawing.Size(53, 12);
            this.label105.TabIndex = 15;
            this.label105.Text = "选框类型";
            // 
            // label95
            // 
            this.label95.AutoSize = true;
            this.label95.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label95.Location = new System.Drawing.Point(4, 85);
            this.label95.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label95.Name = "label95";
            this.label95.Size = new System.Drawing.Size(53, 12);
            this.label95.TabIndex = 13;
            this.label95.Text = "选票页面";
            // 
            // voteindex
            // 
            this.voteindex.Location = new System.Drawing.Point(61, 111);
            this.voteindex.Margin = new System.Windows.Forms.Padding(1);
            this.voteindex.MaxLength = 2;
            this.voteindex.Name = "voteindex";
            this.voteindex.Size = new System.Drawing.Size(120, 21);
            this.voteindex.TabIndex = 12;
            this.voteindex.Text = "1";
            // 
            // label78
            // 
            this.label78.AutoSize = true;
            this.label78.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label78.Location = new System.Drawing.Point(4, 115);
            this.label78.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label78.Name = "label78";
            this.label78.Size = new System.Drawing.Size(53, 12);
            this.label78.TabIndex = 11;
            this.label78.Text = "选票编码";
            // 
            // votename
            // 
            this.votename.Location = new System.Drawing.Point(61, 49);
            this.votename.Margin = new System.Windows.Forms.Padding(1);
            this.votename.MaxLength = 50;
            this.votename.Name = "votename";
            this.votename.Size = new System.Drawing.Size(120, 21);
            this.votename.TabIndex = 10;
            // 
            // label77
            // 
            this.label77.AutoSize = true;
            this.label77.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label77.Location = new System.Drawing.Point(4, 55);
            this.label77.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label77.Name = "label77";
            this.label77.Size = new System.Drawing.Size(53, 12);
            this.label77.TabIndex = 9;
            this.label77.Text = "选票名称";
            // 
            // label76
            // 
            this.label76.AutoSize = true;
            this.label76.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label76.Location = new System.Drawing.Point(4, 24);
            this.label76.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label76.Name = "label76";
            this.label76.Size = new System.Drawing.Size(53, 12);
            this.label76.TabIndex = 7;
            this.label76.Text = "选票序号";
            // 
            // voteid
            // 
            this.voteid.Location = new System.Drawing.Point(61, 19);
            this.voteid.Margin = new System.Windows.Forms.Padding(1);
            this.voteid.MaxLength = 2;
            this.voteid.Name = "voteid";
            this.voteid.Size = new System.Drawing.Size(120, 21);
            this.voteid.TabIndex = 8;
            this.voteid.Text = "1";
            // 
            // votestatustext
            // 
            this.votestatustext.AutoSize = true;
            this.votestatustext.Font = new System.Drawing.Font("宋体", 10.5F);
            this.votestatustext.ForeColor = System.Drawing.Color.Red;
            this.votestatustext.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.votestatustext.Location = new System.Drawing.Point(196, 389);
            this.votestatustext.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.votestatustext.Name = "votestatustext";
            this.votestatustext.Size = new System.Drawing.Size(637, 182);
            this.votestatustext.TabIndex = 6;
            this.votestatustext.Tag = "anchor_left_bottom";
            this.votestatustext.Text = resources.GetString("votestatustext.Text");
            // 
            // StartInsert
            // 
            this.StartInsert.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.StartInsert.Location = new System.Drawing.Point(716, 583);
            this.StartInsert.Margin = new System.Windows.Forms.Padding(1);
            this.StartInsert.Name = "StartInsert";
            this.StartInsert.Size = new System.Drawing.Size(149, 39);
            this.StartInsert.TabIndex = 5;
            this.StartInsert.Tag = "anchor_right_bottom";
            this.StartInsert.Text = "开始执行";
            this.StartInsert.UseVisualStyleBackColor = true;
            this.StartInsert.Click += new System.EventHandler(this.StartInsert_Click);
            // 
            // SelectVotePic
            // 
            this.SelectVotePic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectVotePic.Location = new System.Drawing.Point(4, 583);
            this.SelectVotePic.Margin = new System.Windows.Forms.Padding(1);
            this.SelectVotePic.Name = "SelectVotePic";
            this.SelectVotePic.Size = new System.Drawing.Size(149, 39);
            this.SelectVotePic.TabIndex = 4;
            this.SelectVotePic.Tag = "anchor_left_bottom";
            this.SelectVotePic.Text = "选择模板图片...";
            this.SelectVotePic.UseVisualStyleBackColor = true;
            this.SelectVotePic.Click += new System.EventHandler(this.SelectVotePic_Click);
            // 
            // VotePicture
            // 
            this.VotePicture.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.VotePicture.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VotePicture.Location = new System.Drawing.Point(7, 231);
            this.VotePicture.Margin = new System.Windows.Forms.Padding(1);
            this.VotePicture.Name = "VotePicture";
            this.VotePicture.Size = new System.Drawing.Size(185, 345);
            this.VotePicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.VotePicture.TabIndex = 3;
            this.VotePicture.TabStop = false;
            this.VotePicture.Tag = "anchor_top_left_bottom";
            // 
            // SelectMss3
            // 
            this.SelectMss3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectMss3.Location = new System.Drawing.Point(701, 25);
            this.SelectMss3.Margin = new System.Windows.Forms.Padding(1);
            this.SelectMss3.Name = "SelectMss3";
            this.SelectMss3.Size = new System.Drawing.Size(152, 22);
            this.SelectMss3.TabIndex = 2;
            this.SelectMss3.Tag = "anchor_right_top";
            this.SelectMss3.Text = "加载MSS3数据库文件...";
            this.SelectMss3.UseVisualStyleBackColor = true;
            this.SelectMss3.Click += new System.EventHandler(this.SelectMss3_Click);
            // 
            // Mss3Path
            // 
            this.Mss3Path.Location = new System.Drawing.Point(37, 25);
            this.Mss3Path.Margin = new System.Windows.Forms.Padding(1);
            this.Mss3Path.Name = "Mss3Path";
            this.Mss3Path.ReadOnly = true;
            this.Mss3Path.Size = new System.Drawing.Size(661, 21);
            this.Mss3Path.TabIndex = 1;
            this.Mss3Path.Tag = "anchor_top_left_right";
            // 
            // label75
            // 
            this.label75.AutoSize = true;
            this.label75.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label75.Location = new System.Drawing.Point(4, 28);
            this.label75.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label75.Name = "label75";
            this.label75.Size = new System.Drawing.Size(29, 12);
            this.label75.TabIndex = 0;
            this.label75.Text = "路径";
            // 
            // TabPage_39
            // 
            this.TabPage_39.Controls.Add(this.pictureBox8);
            this.TabPage_39.Controls.Add(this.label167);
            this.TabPage_39.Controls.Add(this.label166);
            this.TabPage_39.Controls.Add(this.pictureBox7);
            this.TabPage_39.Controls.Add(this.SingleDocumentConfigGroupBox);
            this.TabPage_39.Controls.Add(this.groupBox54);
            this.TabPage_39.Location = new System.Drawing.Point(4, 76);
            this.TabPage_39.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_39.Name = "TabPage_39";
            this.TabPage_39.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_39.Size = new System.Drawing.Size(882, 659);
            this.TabPage_39.TabIndex = 38;
            this.TabPage_39.Text = "单人表生成";
            this.TabPage_39.UseVisualStyleBackColor = true;
            // 
            // pictureBox8
            // 
            this.pictureBox8.Image = global::DocumentTools.Properties.Resources.QQ截图20240115183157;
            this.pictureBox8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox8.Location = new System.Drawing.Point(635, 315);
            this.pictureBox8.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(245, 237);
            this.pictureBox8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox8.TabIndex = 5;
            this.pictureBox8.TabStop = false;
            this.pictureBox8.Tag = "anchor_right";
            // 
            // label167
            // 
            this.label167.AutoSize = true;
            this.label167.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label167.ForeColor = System.Drawing.Color.DarkMagenta;
            this.label167.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label167.Location = new System.Drawing.Point(635, 291);
            this.label167.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label167.Name = "label167";
            this.label167.Size = new System.Drawing.Size(245, 14);
            this.label167.TabIndex = 4;
            this.label167.Tag = "anchor_right";
            this.label167.Text = "[WORD2013之前版本设置可选文字图例]";
            // 
            // label166
            // 
            this.label166.AutoSize = true;
            this.label166.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label166.ForeColor = System.Drawing.Color.Tomato;
            this.label166.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label166.Location = new System.Drawing.Point(635, 15);
            this.label166.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label166.Name = "label166";
            this.label166.Size = new System.Drawing.Size(245, 14);
            this.label166.TabIndex = 3;
            this.label166.Tag = "anchor_right_top";
            this.label166.Text = "[WORD2013之后版本设置可选文字图例]";
            // 
            // pictureBox7
            // 
            this.pictureBox7.Image = global::DocumentTools.Properties.Resources.A;
            this.pictureBox7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox7.Location = new System.Drawing.Point(635, 37);
            this.pictureBox7.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(245, 235);
            this.pictureBox7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox7.TabIndex = 2;
            this.pictureBox7.TabStop = false;
            this.pictureBox7.Tag = "anchor_right_top";
            // 
            // SingleDocumentConfigGroupBox
            // 
            this.SingleDocumentConfigGroupBox.Controls.Add(this.SignDocumentProgress);
            this.SingleDocumentConfigGroupBox.Controls.Add(this.groupBox55);
            this.SingleDocumentConfigGroupBox.Controls.Add(this.SignDocumentReplaceStart);
            this.SingleDocumentConfigGroupBox.Controls.Add(this.groupBox58);
            this.SingleDocumentConfigGroupBox.Location = new System.Drawing.Point(4, 101);
            this.SingleDocumentConfigGroupBox.Margin = new System.Windows.Forms.Padding(1);
            this.SingleDocumentConfigGroupBox.Name = "SingleDocumentConfigGroupBox";
            this.SingleDocumentConfigGroupBox.Padding = new System.Windows.Forms.Padding(1);
            this.SingleDocumentConfigGroupBox.Size = new System.Drawing.Size(627, 451);
            this.SingleDocumentConfigGroupBox.TabIndex = 1;
            this.SingleDocumentConfigGroupBox.TabStop = false;
            this.SingleDocumentConfigGroupBox.Tag = "anchor_full_fill";
            this.SingleDocumentConfigGroupBox.Text = "单人表生成参数配置";
            // 
            // SignDocumentProgress
            // 
            this.SignDocumentProgress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentProgress.Location = new System.Drawing.Point(9, 407);
            this.SignDocumentProgress.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentProgress.Name = "SignDocumentProgress";
            this.SignDocumentProgress.Size = new System.Drawing.Size(500, 29);
            this.SignDocumentProgress.TabIndex = 10;
            this.SignDocumentProgress.Tag = "anchor_bottom_left_right";
            // 
            // groupBox55
            // 
            this.groupBox55.Controls.Add(this.SignDocumentUseOnlyNumber);
            this.groupBox55.Controls.Add(this.SignDocumentUsePersonNumber);
            this.groupBox55.Controls.Add(this.SignDocumentUseOnlyPageNumber);
            this.groupBox55.Controls.Add(this.SignDocumentFileEndName);
            this.groupBox55.Controls.Add(this.label162);
            this.groupBox55.Controls.Add(this.label160);
            this.groupBox55.Controls.Add(this.SignDocumentNeedCount);
            this.groupBox55.Controls.Add(this.SignDocumentPicSourceGroupBox);
            this.groupBox55.Location = new System.Drawing.Point(5, 31);
            this.groupBox55.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox55.Name = "groupBox55";
            this.groupBox55.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox55.Size = new System.Drawing.Size(617, 217);
            this.groupBox55.TabIndex = 8;
            this.groupBox55.TabStop = false;
            this.groupBox55.Tag = "anchor_top_left_right";
            this.groupBox55.Text = "参数配置";
            // 
            // SignDocumentUseOnlyNumber
            // 
            this.SignDocumentUseOnlyNumber.AutoSize = true;
            this.SignDocumentUseOnlyNumber.Checked = true;
            this.SignDocumentUseOnlyNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentUseOnlyNumber.Location = new System.Drawing.Point(307, 21);
            this.SignDocumentUseOnlyNumber.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentUseOnlyNumber.Name = "SignDocumentUseOnlyNumber";
            this.SignDocumentUseOnlyNumber.Size = new System.Drawing.Size(89, 16);
            this.SignDocumentUseOnlyNumber.TabIndex = 20;
            this.SignDocumentUseOnlyNumber.TabStop = true;
            this.SignDocumentUseOnlyNumber.Text = "纯数字+后缀";
            this.MainToolsTip.SetToolTip(this.SignDocumentUseOnlyNumber, "注：XXX（001测评人）.docx");
            this.SignDocumentUseOnlyNumber.UseVisualStyleBackColor = true;
            // 
            // SignDocumentUsePersonNumber
            // 
            this.SignDocumentUsePersonNumber.AutoSize = true;
            this.SignDocumentUsePersonNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentUsePersonNumber.Location = new System.Drawing.Point(509, 21);
            this.SignDocumentUsePersonNumber.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentUsePersonNumber.Name = "SignDocumentUsePersonNumber";
            this.SignDocumentUsePersonNumber.Size = new System.Drawing.Size(101, 16);
            this.SignDocumentUsePersonNumber.TabIndex = 19;
            this.SignDocumentUsePersonNumber.TabStop = true;
            this.SignDocumentUsePersonNumber.Text = "第X人作为后缀";
            this.MainToolsTip.SetToolTip(this.SignDocumentUsePersonNumber, "注：XXX（第1人）.docx");
            this.SignDocumentUsePersonNumber.UseVisualStyleBackColor = true;
            // 
            // SignDocumentUseOnlyPageNumber
            // 
            this.SignDocumentUseOnlyPageNumber.AutoSize = true;
            this.SignDocumentUseOnlyPageNumber.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentUseOnlyPageNumber.Location = new System.Drawing.Point(400, 21);
            this.SignDocumentUseOnlyPageNumber.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentUseOnlyPageNumber.Name = "SignDocumentUseOnlyPageNumber";
            this.SignDocumentUseOnlyPageNumber.Size = new System.Drawing.Size(101, 16);
            this.SignDocumentUseOnlyPageNumber.TabIndex = 18;
            this.SignDocumentUseOnlyPageNumber.TabStop = true;
            this.SignDocumentUseOnlyPageNumber.Text = "第X页作为后缀";
            this.MainToolsTip.SetToolTip(this.SignDocumentUseOnlyPageNumber, "注：XXX（第1页）.docx");
            this.SignDocumentUseOnlyPageNumber.UseVisualStyleBackColor = true;
            // 
            // SignDocumentFileEndName
            // 
            this.SignDocumentFileEndName.Location = new System.Drawing.Point(217, 19);
            this.SignDocumentFileEndName.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentFileEndName.Name = "SignDocumentFileEndName";
            this.SignDocumentFileEndName.Size = new System.Drawing.Size(87, 21);
            this.SignDocumentFileEndName.TabIndex = 5;
            this.SignDocumentFileEndName.Text = "测评人";
            // 
            // label162
            // 
            this.label162.AutoSize = true;
            this.label162.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label162.Location = new System.Drawing.Point(137, 23);
            this.label162.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label162.Name = "label162";
            this.label162.Size = new System.Drawing.Size(77, 12);
            this.label162.TabIndex = 13;
            this.label162.Text = "生成名称后缀";
            // 
            // label160
            // 
            this.label160.AutoSize = true;
            this.label160.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label160.Location = new System.Drawing.Point(4, 23);
            this.label160.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label160.Name = "label160";
            this.label160.Size = new System.Drawing.Size(53, 12);
            this.label160.TabIndex = 0;
            this.label160.Text = "生成份数";
            // 
            // SignDocumentNeedCount
            // 
            this.SignDocumentNeedCount.Location = new System.Drawing.Point(61, 20);
            this.SignDocumentNeedCount.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentNeedCount.Maximum = new decimal(new int[] {
            999,
            0,
            0,
            0});
            this.SignDocumentNeedCount.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.SignDocumentNeedCount.Name = "SignDocumentNeedCount";
            this.SignDocumentNeedCount.Size = new System.Drawing.Size(71, 21);
            this.SignDocumentNeedCount.TabIndex = 7;
            this.SignDocumentNeedCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // SignDocumentPicSourceGroupBox
            // 
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.SignDocumentUseEmbedImage);
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.SignDocumentUseOutPersonPath);
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.SignDocumentUseOutPersonPathTextBox);
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.SignDocumentUseOutPersonPathButton);
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.label164);
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.SignDocumentOnlyUseOnlyImage);
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.SignDocumentOnlyUseOnlyImageTextBox);
            this.SignDocumentPicSourceGroupBox.Controls.Add(this.SignDocumentOnlyUseOnlyImageButton);
            this.SignDocumentPicSourceGroupBox.Location = new System.Drawing.Point(7, 45);
            this.SignDocumentPicSourceGroupBox.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentPicSourceGroupBox.Name = "SignDocumentPicSourceGroupBox";
            this.SignDocumentPicSourceGroupBox.Padding = new System.Windows.Forms.Padding(1);
            this.SignDocumentPicSourceGroupBox.Size = new System.Drawing.Size(608, 168);
            this.SignDocumentPicSourceGroupBox.TabIndex = 21;
            this.SignDocumentPicSourceGroupBox.TabStop = false;
            this.SignDocumentPicSourceGroupBox.Tag = "";
            this.SignDocumentPicSourceGroupBox.Text = "设置图片来源";
            // 
            // SignDocumentUseEmbedImage
            // 
            this.SignDocumentUseEmbedImage.AutoSize = true;
            this.SignDocumentUseEmbedImage.Checked = true;
            this.SignDocumentUseEmbedImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentUseEmbedImage.Location = new System.Drawing.Point(4, 27);
            this.SignDocumentUseEmbedImage.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentUseEmbedImage.Name = "SignDocumentUseEmbedImage";
            this.SignDocumentUseEmbedImage.Size = new System.Drawing.Size(131, 16);
            this.SignDocumentUseEmbedImage.TabIndex = 8;
            this.SignDocumentUseEmbedImage.TabStop = true;
            this.SignDocumentUseEmbedImage.Text = "使用内置单人码图片";
            this.SignDocumentUseEmbedImage.UseVisualStyleBackColor = true;
            // 
            // SignDocumentUseOutPersonPath
            // 
            this.SignDocumentUseOutPersonPath.AutoSize = true;
            this.SignDocumentUseOutPersonPath.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentUseOutPersonPath.Location = new System.Drawing.Point(4, 64);
            this.SignDocumentUseOutPersonPath.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentUseOutPersonPath.Name = "SignDocumentUseOutPersonPath";
            this.SignDocumentUseOutPersonPath.Size = new System.Drawing.Size(131, 16);
            this.SignDocumentUseOutPersonPath.TabIndex = 9;
            this.SignDocumentUseOutPersonPath.Text = "使用外部单人码目录";
            this.SignDocumentUseOutPersonPath.UseVisualStyleBackColor = true;
            this.SignDocumentUseOutPersonPath.CheckedChanged += new System.EventHandler(this.SignDocumentUseOutPersonPath_CheckedChanged);
            // 
            // SignDocumentUseOutPersonPathTextBox
            // 
            this.SignDocumentUseOutPersonPathTextBox.Location = new System.Drawing.Point(137, 63);
            this.SignDocumentUseOutPersonPathTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentUseOutPersonPathTextBox.Name = "SignDocumentUseOutPersonPathTextBox";
            this.SignDocumentUseOutPersonPathTextBox.ReadOnly = true;
            this.SignDocumentUseOutPersonPathTextBox.Size = new System.Drawing.Size(303, 21);
            this.SignDocumentUseOutPersonPathTextBox.TabIndex = 9;
            this.SignDocumentUseOutPersonPathTextBox.Tag = "anchor_top_left_right";
            // 
            // SignDocumentUseOutPersonPathButton
            // 
            this.SignDocumentUseOutPersonPathButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentUseOutPersonPathButton.Location = new System.Drawing.Point(443, 63);
            this.SignDocumentUseOutPersonPathButton.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentUseOutPersonPathButton.Name = "SignDocumentUseOutPersonPathButton";
            this.SignDocumentUseOutPersonPathButton.Size = new System.Drawing.Size(141, 22);
            this.SignDocumentUseOutPersonPathButton.TabIndex = 9;
            this.SignDocumentUseOutPersonPathButton.Tag = "anchor_right_top";
            this.SignDocumentUseOutPersonPathButton.Text = "选择目录...";
            this.SignDocumentUseOutPersonPathButton.UseVisualStyleBackColor = true;
            this.SignDocumentUseOutPersonPathButton.Click += new System.EventHandler(this.SignDocumentUseOutPersonPathButton_Click);
            // 
            // label164
            // 
            this.label164.AutoSize = true;
            this.label164.ForeColor = System.Drawing.Color.Crimson;
            this.label164.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label164.Location = new System.Drawing.Point(137, 28);
            this.label164.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label164.Name = "label164";
            this.label164.Size = new System.Drawing.Size(203, 12);
            this.label164.TabIndex = 15;
            this.label164.Text = "[程序内置001-999测评人二维码图片]";
            // 
            // SignDocumentOnlyUseOnlyImage
            // 
            this.SignDocumentOnlyUseOnlyImage.AutoSize = true;
            this.SignDocumentOnlyUseOnlyImage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentOnlyUseOnlyImage.Location = new System.Drawing.Point(4, 103);
            this.SignDocumentOnlyUseOnlyImage.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentOnlyUseOnlyImage.Name = "SignDocumentOnlyUseOnlyImage";
            this.SignDocumentOnlyUseOnlyImage.Size = new System.Drawing.Size(131, 16);
            this.SignDocumentOnlyUseOnlyImage.TabIndex = 10;
            this.SignDocumentOnlyUseOnlyImage.Text = "仅使用单张图片文件";
            this.SignDocumentOnlyUseOnlyImage.UseVisualStyleBackColor = true;
            this.SignDocumentOnlyUseOnlyImage.CheckedChanged += new System.EventHandler(this.SignDocumentOnlyUseOnlyImage_CheckedChanged);
            // 
            // SignDocumentOnlyUseOnlyImageTextBox
            // 
            this.SignDocumentOnlyUseOnlyImageTextBox.Location = new System.Drawing.Point(137, 99);
            this.SignDocumentOnlyUseOnlyImageTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentOnlyUseOnlyImageTextBox.Name = "SignDocumentOnlyUseOnlyImageTextBox";
            this.SignDocumentOnlyUseOnlyImageTextBox.ReadOnly = true;
            this.SignDocumentOnlyUseOnlyImageTextBox.Size = new System.Drawing.Size(303, 21);
            this.SignDocumentOnlyUseOnlyImageTextBox.TabIndex = 11;
            this.SignDocumentOnlyUseOnlyImageTextBox.Tag = "anchor_top_left_right";
            // 
            // SignDocumentOnlyUseOnlyImageButton
            // 
            this.SignDocumentOnlyUseOnlyImageButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentOnlyUseOnlyImageButton.Location = new System.Drawing.Point(443, 99);
            this.SignDocumentOnlyUseOnlyImageButton.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentOnlyUseOnlyImageButton.Name = "SignDocumentOnlyUseOnlyImageButton";
            this.SignDocumentOnlyUseOnlyImageButton.Size = new System.Drawing.Size(141, 22);
            this.SignDocumentOnlyUseOnlyImageButton.TabIndex = 12;
            this.SignDocumentOnlyUseOnlyImageButton.Tag = "anchor_right_top";
            this.SignDocumentOnlyUseOnlyImageButton.Text = "选择文件...";
            this.SignDocumentOnlyUseOnlyImageButton.UseVisualStyleBackColor = true;
            this.SignDocumentOnlyUseOnlyImageButton.Click += new System.EventHandler(this.SignDocumentOnlyUseOnlyImageButton_Click);
            // 
            // SignDocumentReplaceStart
            // 
            this.SignDocumentReplaceStart.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentReplaceStart.Location = new System.Drawing.Point(513, 407);
            this.SignDocumentReplaceStart.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentReplaceStart.Name = "SignDocumentReplaceStart";
            this.SignDocumentReplaceStart.Size = new System.Drawing.Size(108, 27);
            this.SignDocumentReplaceStart.TabIndex = 6;
            this.SignDocumentReplaceStart.Tag = "anchor_right_bottom";
            this.SignDocumentReplaceStart.Text = "开始生成";
            this.SignDocumentReplaceStart.UseVisualStyleBackColor = true;
            this.SignDocumentReplaceStart.Click += new System.EventHandler(this.SignDocumentReplaceStart_Click);
            // 
            // groupBox58
            // 
            this.groupBox58.Controls.Add(this.label161);
            this.groupBox58.Controls.Add(this.SignDocumentFileSaveBox);
            this.groupBox58.Controls.Add(this.SignDocumentFileSaveButton);
            this.groupBox58.Location = new System.Drawing.Point(5, 253);
            this.groupBox58.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox58.Name = "groupBox58";
            this.groupBox58.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox58.Size = new System.Drawing.Size(617, 77);
            this.groupBox58.TabIndex = 9;
            this.groupBox58.TabStop = false;
            this.groupBox58.Tag = "";
            this.groupBox58.Text = "单人表文档保存目录配置";
            // 
            // label161
            // 
            this.label161.AutoSize = true;
            this.label161.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label161.Location = new System.Drawing.Point(19, 35);
            this.label161.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label161.Name = "label161";
            this.label161.Size = new System.Drawing.Size(113, 12);
            this.label161.TabIndex = 2;
            this.label161.Text = "单人表生成保存目录";
            // 
            // SignDocumentFileSaveBox
            // 
            this.SignDocumentFileSaveBox.Location = new System.Drawing.Point(137, 31);
            this.SignDocumentFileSaveBox.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentFileSaveBox.Name = "SignDocumentFileSaveBox";
            this.SignDocumentFileSaveBox.ReadOnly = true;
            this.SignDocumentFileSaveBox.Size = new System.Drawing.Size(315, 21);
            this.SignDocumentFileSaveBox.TabIndex = 3;
            this.SignDocumentFileSaveBox.Tag = "anchor_top_left_right";
            // 
            // SignDocumentFileSaveButton
            // 
            this.SignDocumentFileSaveButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignDocumentFileSaveButton.Location = new System.Drawing.Point(455, 31);
            this.SignDocumentFileSaveButton.Margin = new System.Windows.Forms.Padding(1);
            this.SignDocumentFileSaveButton.Name = "SignDocumentFileSaveButton";
            this.SignDocumentFileSaveButton.Size = new System.Drawing.Size(157, 22);
            this.SignDocumentFileSaveButton.TabIndex = 5;
            this.SignDocumentFileSaveButton.Tag = "anchor_right_top";
            this.SignDocumentFileSaveButton.Text = "选择目录...";
            this.SignDocumentFileSaveButton.UseVisualStyleBackColor = true;
            this.SignDocumentFileSaveButton.Click += new System.EventHandler(this.SignDocumentFileSaveButton_Click);
            // 
            // groupBox54
            // 
            this.groupBox54.Controls.Add(this.label165);
            this.groupBox54.Controls.Add(this.SingPersonImageAlt);
            this.groupBox54.Controls.Add(this.label159);
            this.groupBox54.Controls.Add(this.SignPersionDocumentChooseFile);
            this.groupBox54.Controls.Add(this.SignPersionDocument);
            this.groupBox54.Controls.Add(this.label152);
            this.groupBox54.Location = new System.Drawing.Point(4, 4);
            this.groupBox54.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox54.Name = "groupBox54";
            this.groupBox54.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox54.Size = new System.Drawing.Size(627, 93);
            this.groupBox54.TabIndex = 0;
            this.groupBox54.TabStop = false;
            this.groupBox54.Tag = "anchor_top_left_right";
            this.groupBox54.Text = "单人表模板信息";
            // 
            // label165
            // 
            this.label165.AutoSize = true;
            this.label165.ForeColor = System.Drawing.Color.Crimson;
            this.label165.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label165.Location = new System.Drawing.Point(461, 65);
            this.label165.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label165.Name = "label165";
            this.label165.Size = new System.Drawing.Size(143, 12);
            this.label165.TabIndex = 16;
            this.label165.Tag = "anchor_right_top";
            this.label165.Text = "[支持多个文字，用@分隔]";
            // 
            // SingPersonImageAlt
            // 
            this.SingPersonImageAlt.Location = new System.Drawing.Point(61, 63);
            this.SingPersonImageAlt.Margin = new System.Windows.Forms.Padding(1);
            this.SingPersonImageAlt.Name = "SingPersonImageAlt";
            this.SingPersonImageAlt.Size = new System.Drawing.Size(397, 21);
            this.SingPersonImageAlt.TabIndex = 4;
            this.SingPersonImageAlt.Tag = "anchor_top_left_right";
            this.MainToolsTip.SetToolTip(this.SingPersonImageAlt, "请参考右侧图例设置要替换的图片可选文字信息");
            // 
            // label159
            // 
            this.label159.AutoSize = true;
            this.label159.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label159.Location = new System.Drawing.Point(4, 67);
            this.label159.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label159.Name = "label159";
            this.label159.Size = new System.Drawing.Size(53, 12);
            this.label159.TabIndex = 3;
            this.label159.Text = "可选文字";
            // 
            // SignPersionDocumentChooseFile
            // 
            this.SignPersionDocumentChooseFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SignPersionDocumentChooseFile.Location = new System.Drawing.Point(463, 25);
            this.SignPersionDocumentChooseFile.Margin = new System.Windows.Forms.Padding(1);
            this.SignPersionDocumentChooseFile.Name = "SignPersionDocumentChooseFile";
            this.SignPersionDocumentChooseFile.Size = new System.Drawing.Size(159, 22);
            this.SignPersionDocumentChooseFile.TabIndex = 2;
            this.SignPersionDocumentChooseFile.Tag = "anchor_right_top";
            this.SignPersionDocumentChooseFile.Text = "选择文件...";
            this.SignPersionDocumentChooseFile.UseVisualStyleBackColor = true;
            this.SignPersionDocumentChooseFile.Click += new System.EventHandler(this.SignPersionDocumentChooseFile_Click);
            // 
            // SignPersionDocument
            // 
            this.SignPersionDocument.Location = new System.Drawing.Point(61, 27);
            this.SignPersionDocument.Margin = new System.Windows.Forms.Padding(1);
            this.SignPersionDocument.Name = "SignPersionDocument";
            this.SignPersionDocument.ReadOnly = true;
            this.SignPersionDocument.Size = new System.Drawing.Size(397, 21);
            this.SignPersionDocument.TabIndex = 1;
            this.SignPersionDocument.Tag = "anchor_top_left_right";
            // 
            // label152
            // 
            this.label152.AutoSize = true;
            this.label152.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label152.Location = new System.Drawing.Point(4, 31);
            this.label152.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label152.Name = "label152";
            this.label152.Size = new System.Drawing.Size(53, 12);
            this.label152.TabIndex = 0;
            this.label152.Text = "模板文件";
            // 
            // TabPage_40
            // 
            this.TabPage_40.Controls.Add(this.linkLabel5);
            this.TabPage_40.Controls.Add(this.label173);
            this.TabPage_40.Controls.Add(this.groupBox59);
            this.TabPage_40.Controls.Add(this.ExamReportChange);
            this.TabPage_40.Controls.Add(this.groupBox56);
            this.TabPage_40.Controls.Add(this.SelectExamFile);
            this.TabPage_40.Controls.Add(this.ExamFileText);
            this.TabPage_40.Controls.Add(this.label154);
            this.TabPage_40.Controls.Add(this.RefreshVFSOFTREPORT);
            this.TabPage_40.Controls.Add(this.groupBox57);
            this.TabPage_40.Location = new System.Drawing.Point(4, 76);
            this.TabPage_40.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_40.Name = "TabPage_40";
            this.TabPage_40.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_40.Size = new System.Drawing.Size(882, 659);
            this.TabPage_40.TabIndex = 39;
            this.TabPage_40.Text = "考评报表修改";
            this.TabPage_40.UseVisualStyleBackColor = true;
            // 
            // linkLabel5
            // 
            this.linkLabel5.AutoSize = true;
            this.linkLabel5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabel5.Location = new System.Drawing.Point(15, 489);
            this.linkLabel5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.linkLabel5.Name = "linkLabel5";
            this.linkLabel5.Size = new System.Drawing.Size(281, 12);
            this.linkLabel5.TabIndex = 16;
            this.linkLabel5.TabStop = true;
            this.linkLabel5.Tag = "anchor_left_bottom";
            this.linkLabel5.Text = "功能需要AccessDataBaseEngine组件，点击前往下载";
            this.linkLabel5.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel5_LinkClicked);
            // 
            // label173
            // 
            this.label173.AutoSize = true;
            this.label173.ForeColor = System.Drawing.Color.Crimson;
            this.label173.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label173.Location = new System.Drawing.Point(333, 483);
            this.label173.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label173.Name = "label173";
            this.label173.Size = new System.Drawing.Size(521, 12);
            this.label173.TabIndex = 15;
            this.label173.Tag = "anchor_right_bottom";
            this.label173.Text = "注：评测主体修改内容为空则表示删除该报表内的评测主体；使用本功能时建议关闭报表设计页面";
            // 
            // groupBox59
            // 
            this.groupBox59.Controls.Add(this.ExamReportChangeItemDataFormat);
            this.groupBox59.Controls.Add(this.ExamReportChangeItemName);
            this.groupBox59.Controls.Add(this.ExamReportChangeItemDataRaid);
            this.groupBox59.Controls.Add(this.ExamReportChangeProgress);
            this.groupBox59.Controls.Add(this.ExamReplaceRaids);
            this.groupBox59.Controls.Add(this.ExamReplaceDataFormat);
            this.groupBox59.Controls.Add(this.ExamReplaceItemName);
            this.groupBox59.Controls.Add(this.label172);
            this.groupBox59.Controls.Add(this.label171);
            this.groupBox59.Controls.Add(this.label170);
            this.groupBox59.Controls.Add(this.ExamReportRaids);
            this.groupBox59.Controls.Add(this.label169);
            this.groupBox59.Controls.Add(this.label168);
            this.groupBox59.Controls.Add(this.ExamReportItems);
            this.groupBox59.Controls.Add(this.label158);
            this.groupBox59.Controls.Add(this.ExamReportDataFormat);
            this.groupBox59.Location = new System.Drawing.Point(331, 327);
            this.groupBox59.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox59.Name = "groupBox59";
            this.groupBox59.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox59.Size = new System.Drawing.Size(540, 155);
            this.groupBox59.TabIndex = 14;
            this.groupBox59.TabStop = false;
            this.groupBox59.Tag = "anchor_right_bottom";
            this.groupBox59.Text = "修改报表参数";
            // 
            // ExamReportChangeItemDataFormat
            // 
            this.ExamReportChangeItemDataFormat.AutoSize = true;
            this.ExamReportChangeItemDataFormat.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExamReportChangeItemDataFormat.Location = new System.Drawing.Point(487, 53);
            this.ExamReportChangeItemDataFormat.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportChangeItemDataFormat.Name = "ExamReportChangeItemDataFormat";
            this.ExamReportChangeItemDataFormat.Size = new System.Drawing.Size(47, 16);
            this.ExamReportChangeItemDataFormat.TabIndex = 29;
            this.ExamReportChangeItemDataFormat.TabStop = true;
            this.ExamReportChangeItemDataFormat.Text = "修改";
            this.ExamReportChangeItemDataFormat.UseVisualStyleBackColor = true;
            // 
            // ExamReportChangeItemName
            // 
            this.ExamReportChangeItemName.AutoSize = true;
            this.ExamReportChangeItemName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExamReportChangeItemName.Location = new System.Drawing.Point(487, 27);
            this.ExamReportChangeItemName.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportChangeItemName.Name = "ExamReportChangeItemName";
            this.ExamReportChangeItemName.Size = new System.Drawing.Size(47, 16);
            this.ExamReportChangeItemName.TabIndex = 27;
            this.ExamReportChangeItemName.TabStop = true;
            this.ExamReportChangeItemName.Text = "修改";
            this.MainToolsTip.SetToolTip(this.ExamReportChangeItemName, "注：\r\n1、只建议修改项目名称不同，选项相同的报表\r\n2、如果出现带#符号的项目，表示表格内有多项合计的数据内容，如果需要修改，参考原格式样式进行修改\r\n3、如果" +
        "下拉项目出现空白项，可能是有单元格没合并的原因，可忽略");
            this.ExamReportChangeItemName.UseVisualStyleBackColor = true;
            // 
            // ExamReportChangeItemDataRaid
            // 
            this.ExamReportChangeItemDataRaid.AutoSize = true;
            this.ExamReportChangeItemDataRaid.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExamReportChangeItemDataRaid.Location = new System.Drawing.Point(487, 79);
            this.ExamReportChangeItemDataRaid.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportChangeItemDataRaid.Name = "ExamReportChangeItemDataRaid";
            this.ExamReportChangeItemDataRaid.Size = new System.Drawing.Size(47, 16);
            this.ExamReportChangeItemDataRaid.TabIndex = 28;
            this.ExamReportChangeItemDataRaid.TabStop = true;
            this.ExamReportChangeItemDataRaid.Text = "修改";
            this.ExamReportChangeItemDataRaid.UseVisualStyleBackColor = true;
            // 
            // ExamReportChangeProgress
            // 
            this.ExamReportChangeProgress.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExamReportChangeProgress.Location = new System.Drawing.Point(5, 125);
            this.ExamReportChangeProgress.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportChangeProgress.Name = "ExamReportChangeProgress";
            this.ExamReportChangeProgress.Size = new System.Drawing.Size(529, 25);
            this.ExamReportChangeProgress.TabIndex = 15;
            // 
            // ExamReplaceRaids
            // 
            this.ExamReplaceRaids.Location = new System.Drawing.Point(273, 75);
            this.ExamReplaceRaids.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReplaceRaids.Name = "ExamReplaceRaids";
            this.ExamReplaceRaids.Size = new System.Drawing.Size(211, 21);
            this.ExamReplaceRaids.TabIndex = 23;
            // 
            // ExamReplaceDataFormat
            // 
            this.ExamReplaceDataFormat.FormattingEnabled = true;
            this.ExamReplaceDataFormat.Location = new System.Drawing.Point(273, 51);
            this.ExamReplaceDataFormat.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReplaceDataFormat.Name = "ExamReplaceDataFormat";
            this.ExamReplaceDataFormat.Size = new System.Drawing.Size(211, 20);
            this.ExamReplaceDataFormat.TabIndex = 22;
            // 
            // ExamReplaceItemName
            // 
            this.ExamReplaceItemName.Location = new System.Drawing.Point(273, 25);
            this.ExamReplaceItemName.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReplaceItemName.Name = "ExamReplaceItemName";
            this.ExamReplaceItemName.Size = new System.Drawing.Size(211, 21);
            this.ExamReplaceItemName.TabIndex = 21;
            // 
            // label172
            // 
            this.label172.AutoSize = true;
            this.label172.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label172.Location = new System.Drawing.Point(229, 81);
            this.label172.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label172.Name = "label172";
            this.label172.Size = new System.Drawing.Size(41, 12);
            this.label172.TabIndex = 20;
            this.label172.Text = "修改为";
            // 
            // label171
            // 
            this.label171.AutoSize = true;
            this.label171.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label171.Location = new System.Drawing.Point(229, 55);
            this.label171.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label171.Name = "label171";
            this.label171.Size = new System.Drawing.Size(41, 12);
            this.label171.TabIndex = 19;
            this.label171.Text = "修改为";
            // 
            // label170
            // 
            this.label170.AutoSize = true;
            this.label170.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label170.Location = new System.Drawing.Point(229, 31);
            this.label170.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label170.Name = "label170";
            this.label170.Size = new System.Drawing.Size(41, 12);
            this.label170.TabIndex = 18;
            this.label170.Text = "修改为";
            // 
            // ExamReportRaids
            // 
            this.ExamReportRaids.FormattingEnabled = true;
            this.ExamReportRaids.Location = new System.Drawing.Point(72, 79);
            this.ExamReportRaids.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportRaids.Name = "ExamReportRaids";
            this.ExamReportRaids.Size = new System.Drawing.Size(155, 20);
            this.ExamReportRaids.TabIndex = 17;
            this.ExamReportRaids.SelectedIndexChanged += new System.EventHandler(this.ExamReportRaids_SelectedIndexChanged);
            // 
            // label169
            // 
            this.label169.AutoSize = true;
            this.label169.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label169.Location = new System.Drawing.Point(15, 83);
            this.label169.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label169.Name = "label169";
            this.label169.Size = new System.Drawing.Size(53, 12);
            this.label169.TabIndex = 16;
            this.label169.Text = "评测主体";
            // 
            // label168
            // 
            this.label168.AutoSize = true;
            this.label168.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label168.Location = new System.Drawing.Point(15, 31);
            this.label168.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label168.Name = "label168";
            this.label168.Size = new System.Drawing.Size(53, 12);
            this.label168.TabIndex = 14;
            this.label168.Text = "选择项目";
            // 
            // ExamReportItems
            // 
            this.ExamReportItems.FormattingEnabled = true;
            this.ExamReportItems.Location = new System.Drawing.Point(72, 27);
            this.ExamReportItems.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportItems.Name = "ExamReportItems";
            this.ExamReportItems.Size = new System.Drawing.Size(155, 20);
            this.ExamReportItems.TabIndex = 15;
            this.ExamReportItems.SelectedIndexChanged += new System.EventHandler(this.ExamReportItems_SelectedIndexChanged);
            this.ExamReportItems.MouseHover += new System.EventHandler(this.ExamReportItems_MouseHover);
            // 
            // label158
            // 
            this.label158.AutoSize = true;
            this.label158.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label158.Location = new System.Drawing.Point(15, 57);
            this.label158.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label158.Name = "label158";
            this.label158.Size = new System.Drawing.Size(53, 12);
            this.label158.TabIndex = 12;
            this.label158.Text = "数据类型";
            // 
            // ExamReportDataFormat
            // 
            this.ExamReportDataFormat.FormattingEnabled = true;
            this.ExamReportDataFormat.Location = new System.Drawing.Point(72, 53);
            this.ExamReportDataFormat.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportDataFormat.Name = "ExamReportDataFormat";
            this.ExamReportDataFormat.Size = new System.Drawing.Size(155, 20);
            this.ExamReportDataFormat.TabIndex = 13;
            // 
            // ExamReportChange
            // 
            this.ExamReportChange.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExamReportChange.Location = new System.Drawing.Point(740, 539);
            this.ExamReportChange.Margin = new System.Windows.Forms.Padding(1);
            this.ExamReportChange.Name = "ExamReportChange";
            this.ExamReportChange.Size = new System.Drawing.Size(129, 41);
            this.ExamReportChange.TabIndex = 9;
            this.ExamReportChange.Tag = "anchor_right_bottom";
            this.ExamReportChange.Text = "执行修改";
            this.ExamReportChange.UseVisualStyleBackColor = true;
            this.ExamReportChange.Click += new System.EventHandler(this.ExamReportChange_Click);
            // 
            // groupBox56
            // 
            this.groupBox56.Controls.Add(this.ExamRaidsCopy);
            this.groupBox56.Controls.Add(this.ExamRaidInfoCombox);
            this.groupBox56.Controls.Add(this.label157);
            this.groupBox56.Controls.Add(this.label156);
            this.groupBox56.Controls.Add(this.label155);
            this.groupBox56.Controls.Add(this.ExamItemsCombox);
            this.groupBox56.Controls.Add(this.ExamItemsSelectorsCombox);
            this.groupBox56.Location = new System.Drawing.Point(16, 327);
            this.groupBox56.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox56.Name = "groupBox56";
            this.groupBox56.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox56.Size = new System.Drawing.Size(311, 155);
            this.groupBox56.TabIndex = 7;
            this.groupBox56.TabStop = false;
            this.groupBox56.Tag = "anchor_left_bottom";
            this.groupBox56.Text = "所选考评表信息";
            // 
            // ExamRaidsCopy
            // 
            this.ExamRaidsCopy.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExamRaidsCopy.Location = new System.Drawing.Point(73, 129);
            this.ExamRaidsCopy.Margin = new System.Windows.Forms.Padding(1);
            this.ExamRaidsCopy.Name = "ExamRaidsCopy";
            this.ExamRaidsCopy.Size = new System.Drawing.Size(233, 22);
            this.ExamRaidsCopy.TabIndex = 12;
            this.ExamRaidsCopy.Text = "复制评测主体合计身份";
            this.MainToolsTip.SetToolTip(this.ExamRaidsCopy, "提示：\r\n报表的合计身份在数据库用是用逗号拼接的\r\n\r\n以ABC为例，合计身份就是A，B，C");
            this.ExamRaidsCopy.UseVisualStyleBackColor = true;
            this.ExamRaidsCopy.Click += new System.EventHandler(this.ExamRaidsCopy_Click);
            // 
            // ExamRaidInfoCombox
            // 
            this.ExamRaidInfoCombox.FormattingEnabled = true;
            this.ExamRaidInfoCombox.Location = new System.Drawing.Point(73, 105);
            this.ExamRaidInfoCombox.Margin = new System.Windows.Forms.Padding(1);
            this.ExamRaidInfoCombox.Name = "ExamRaidInfoCombox";
            this.ExamRaidInfoCombox.Size = new System.Drawing.Size(235, 20);
            this.ExamRaidInfoCombox.TabIndex = 11;
            this.ExamRaidInfoCombox.SelectedIndexChanged += new System.EventHandler(this.ExamRaidInfoCombox_SelectedIndexChanged);
            this.ExamRaidInfoCombox.MouseHover += new System.EventHandler(this.ExamRaidInfoCombox_MouseHover);
            // 
            // label157
            // 
            this.label157.AutoSize = true;
            this.label157.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label157.Location = new System.Drawing.Point(17, 108);
            this.label157.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label157.Name = "label157";
            this.label157.Size = new System.Drawing.Size(53, 12);
            this.label157.TabIndex = 10;
            this.label157.Text = "评测主体";
            // 
            // label156
            // 
            this.label156.AutoSize = true;
            this.label156.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label156.Location = new System.Drawing.Point(17, 71);
            this.label156.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label156.Name = "label156";
            this.label156.Size = new System.Drawing.Size(53, 12);
            this.label156.TabIndex = 9;
            this.label156.Text = "项目选项";
            // 
            // label155
            // 
            this.label155.AutoSize = true;
            this.label155.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label155.Location = new System.Drawing.Point(17, 36);
            this.label155.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label155.Name = "label155";
            this.label155.Size = new System.Drawing.Size(53, 12);
            this.label155.TabIndex = 8;
            this.label155.Text = "项目名称";
            // 
            // ExamItemsCombox
            // 
            this.ExamItemsCombox.FormattingEnabled = true;
            this.ExamItemsCombox.Location = new System.Drawing.Point(73, 33);
            this.ExamItemsCombox.Margin = new System.Windows.Forms.Padding(1);
            this.ExamItemsCombox.Name = "ExamItemsCombox";
            this.ExamItemsCombox.Size = new System.Drawing.Size(235, 20);
            this.ExamItemsCombox.TabIndex = 5;
            this.ExamItemsCombox.SelectedIndexChanged += new System.EventHandler(this.ExamItemsCombox_SelectedIndexChanged);
            this.ExamItemsCombox.MouseHover += new System.EventHandler(this.ExamItemsCombox_MouseHover);
            // 
            // ExamItemsSelectorsCombox
            // 
            this.ExamItemsSelectorsCombox.FormattingEnabled = true;
            this.ExamItemsSelectorsCombox.Location = new System.Drawing.Point(73, 68);
            this.ExamItemsSelectorsCombox.Margin = new System.Windows.Forms.Padding(1);
            this.ExamItemsSelectorsCombox.Name = "ExamItemsSelectorsCombox";
            this.ExamItemsSelectorsCombox.Size = new System.Drawing.Size(235, 20);
            this.ExamItemsSelectorsCombox.TabIndex = 6;
            this.ExamItemsSelectorsCombox.MouseHover += new System.EventHandler(this.ExamItemsSelectorsCombox_MouseHover);
            // 
            // SelectExamFile
            // 
            this.SelectExamFile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.SelectExamFile.Location = new System.Drawing.Point(771, 17);
            this.SelectExamFile.Margin = new System.Windows.Forms.Padding(1);
            this.SelectExamFile.Name = "SelectExamFile";
            this.SelectExamFile.Size = new System.Drawing.Size(100, 22);
            this.SelectExamFile.TabIndex = 4;
            this.SelectExamFile.Tag = "anchor_right_top";
            this.SelectExamFile.Text = "选择文件...";
            this.SelectExamFile.UseVisualStyleBackColor = true;
            this.SelectExamFile.Click += new System.EventHandler(this.SelectExamFile_Click);
            // 
            // ExamFileText
            // 
            this.ExamFileText.Location = new System.Drawing.Point(115, 17);
            this.ExamFileText.Margin = new System.Windows.Forms.Padding(1);
            this.ExamFileText.Name = "ExamFileText";
            this.ExamFileText.ReadOnly = true;
            this.ExamFileText.Size = new System.Drawing.Size(653, 21);
            this.ExamFileText.TabIndex = 3;
            this.ExamFileText.Tag = "anchor_top_left_right";
            // 
            // label154
            // 
            this.label154.AutoSize = true;
            this.label154.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label154.Location = new System.Drawing.Point(23, 20);
            this.label154.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label154.Name = "label154";
            this.label154.Size = new System.Drawing.Size(89, 12);
            this.label154.TabIndex = 2;
            this.label154.Text = "数据包文件路径";
            // 
            // RefreshVFSOFTREPORT
            // 
            this.RefreshVFSOFTREPORT.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.RefreshVFSOFTREPORT.Location = new System.Drawing.Point(16, 539);
            this.RefreshVFSOFTREPORT.Margin = new System.Windows.Forms.Padding(1);
            this.RefreshVFSOFTREPORT.Name = "RefreshVFSOFTREPORT";
            this.RefreshVFSOFTREPORT.Size = new System.Drawing.Size(129, 41);
            this.RefreshVFSOFTREPORT.TabIndex = 1;
            this.RefreshVFSOFTREPORT.Tag = "anchor_left_bottom";
            this.RefreshVFSOFTREPORT.Text = "刷新数据";
            this.RefreshVFSOFTREPORT.UseVisualStyleBackColor = true;
            this.RefreshVFSOFTREPORT.Click += new System.EventHandler(this.RefreshVFSOFTREPORT_Click);
            // 
            // groupBox57
            // 
            this.groupBox57.Controls.Add(this.VFSOFT_Report_ListView);
            this.groupBox57.Location = new System.Drawing.Point(16, 43);
            this.groupBox57.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox57.Name = "groupBox57";
            this.groupBox57.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox57.Size = new System.Drawing.Size(863, 276);
            this.groupBox57.TabIndex = 8;
            this.groupBox57.TabStop = false;
            this.groupBox57.Tag = "anchor_full_fill";
            this.groupBox57.Text = "报表基础信息展示";
            // 
            // VFSOFT_Report_ListView
            // 
            this.VFSOFT_Report_ListView.HideSelection = false;
            this.VFSOFT_Report_ListView.Location = new System.Drawing.Point(8, 17);
            this.VFSOFT_Report_ListView.Margin = new System.Windows.Forms.Padding(1);
            this.VFSOFT_Report_ListView.Name = "VFSOFT_Report_ListView";
            this.VFSOFT_Report_ListView.Size = new System.Drawing.Size(848, 251);
            this.VFSOFT_Report_ListView.TabIndex = 0;
            this.VFSOFT_Report_ListView.Tag = "anchor_full_fill";
            this.VFSOFT_Report_ListView.UseCompatibleStateImageBehavior = false;
            this.VFSOFT_Report_ListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.VFSOFT_Report_ListView_ItemSelectionChanged);
            // 
            // TabPage_38
            // 
            this.TabPage_38.Controls.Add(this.groupBox53);
            this.TabPage_38.Controls.Add(this.label142);
            this.TabPage_38.Controls.Add(this.VoteTable_Render);
            this.TabPage_38.Controls.Add(this.报告单生成日期);
            this.TabPage_38.Controls.Add(this.报告单结束语);
            this.TabPage_38.Controls.Add(this.groupBox52);
            this.TabPage_38.Controls.Add(this.groupBox51);
            this.TabPage_38.Controls.Add(this.groupBox50);
            this.TabPage_38.Controls.Add(this.groupBox49);
            this.TabPage_38.Location = new System.Drawing.Point(4, 76);
            this.TabPage_38.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_38.Name = "TabPage_38";
            this.TabPage_38.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_38.Size = new System.Drawing.Size(882, 659);
            this.TabPage_38.TabIndex = 37;
            this.TabPage_38.Text = "选举结果报告单样式生成";
            this.TabPage_38.UseVisualStyleBackColor = true;
            // 
            // groupBox53
            // 
            this.groupBox53.Controls.Add(this.VoteTable_AnotherTableShow);
            this.groupBox53.Controls.Add(this.VoteTable_NumberIndex_Keep);
            this.groupBox53.Controls.Add(this.VoteTable_KeepPageFooter);
            this.groupBox53.Controls.Add(this.VoteTableFontSize);
            this.groupBox53.Controls.Add(this.label151);
            this.groupBox53.Controls.Add(this.label150);
            this.groupBox53.Controls.Add(this.VoteTableHeight);
            this.groupBox53.Controls.Add(this.label149);
            this.groupBox53.Controls.Add(this.VoteTable_Random_PageCounts);
            this.groupBox53.Location = new System.Drawing.Point(655, 268);
            this.groupBox53.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox53.Name = "groupBox53";
            this.groupBox53.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox53.Size = new System.Drawing.Size(220, 167);
            this.groupBox53.TabIndex = 9;
            this.groupBox53.TabStop = false;
            this.groupBox53.Tag = "anchor_right_top";
            this.groupBox53.Text = "其他参数设置";
            // 
            // VoteTable_AnotherTableShow
            // 
            this.VoteTable_AnotherTableShow.AutoSize = true;
            this.VoteTable_AnotherTableShow.Location = new System.Drawing.Point(107, 87);
            this.VoteTable_AnotherTableShow.Margin = new System.Windows.Forms.Padding(2);
            this.VoteTable_AnotherTableShow.Name = "VoteTable_AnotherTableShow";
            this.VoteTable_AnotherTableShow.Size = new System.Drawing.Size(108, 16);
            this.VoteTable_AnotherTableShow.TabIndex = 0;
            this.VoteTable_AnotherTableShow.Text = "插入另选人表格";
            this.MainToolsTip.SetToolTip(this.VoteTable_AnotherTableShow, "不支持字段自定义，可生成后自行调整");
            this.VoteTable_AnotherTableShow.UseVisualStyleBackColor = true;
            // 
            // VoteTable_NumberIndex_Keep
            // 
            this.VoteTable_NumberIndex_Keep.AutoSize = true;
            this.VoteTable_NumberIndex_Keep.Checked = true;
            this.VoteTable_NumberIndex_Keep.CheckState = System.Windows.Forms.CheckState.Checked;
            this.VoteTable_NumberIndex_Keep.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VoteTable_NumberIndex_Keep.Location = new System.Drawing.Point(9, 113);
            this.VoteTable_NumberIndex_Keep.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_NumberIndex_Keep.Name = "VoteTable_NumberIndex_Keep";
            this.VoteTable_NumberIndex_Keep.Size = new System.Drawing.Size(84, 16);
            this.VoteTable_NumberIndex_Keep.TabIndex = 14;
            this.VoteTable_NumberIndex_Keep.Text = "保留序号列";
            this.MainToolsTip.SetToolTip(this.VoteTable_NumberIndex_Keep, "同时影响候选人表格和另选人表格");
            this.VoteTable_NumberIndex_Keep.UseVisualStyleBackColor = true;
            // 
            // VoteTable_KeepPageFooter
            // 
            this.VoteTable_KeepPageFooter.AutoSize = true;
            this.VoteTable_KeepPageFooter.Checked = true;
            this.VoteTable_KeepPageFooter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.VoteTable_KeepPageFooter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VoteTable_KeepPageFooter.Location = new System.Drawing.Point(9, 88);
            this.VoteTable_KeepPageFooter.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_KeepPageFooter.Name = "VoteTable_KeepPageFooter";
            this.VoteTable_KeepPageFooter.Size = new System.Drawing.Size(96, 16);
            this.VoteTable_KeepPageFooter.TabIndex = 13;
            this.VoteTable_KeepPageFooter.Text = "保留底部页码";
            this.VoteTable_KeepPageFooter.UseVisualStyleBackColor = true;
            // 
            // VoteTableFontSize
            // 
            this.VoteTableFontSize.Location = new System.Drawing.Point(89, 55);
            this.VoteTableFontSize.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTableFontSize.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.VoteTableFontSize.Minimum = new decimal(new int[] {
            12,
            0,
            0,
            0});
            this.VoteTableFontSize.Name = "VoteTableFontSize";
            this.VoteTableFontSize.Size = new System.Drawing.Size(51, 21);
            this.VoteTableFontSize.TabIndex = 12;
            this.VoteTableFontSize.Value = new decimal(new int[] {
            14,
            0,
            0,
            0});
            // 
            // label151
            // 
            this.label151.AutoSize = true;
            this.label151.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label151.Location = new System.Drawing.Point(8, 57);
            this.label151.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label151.Name = "label151";
            this.label151.Size = new System.Drawing.Size(77, 12);
            this.label151.TabIndex = 11;
            this.label151.Text = "表格字体大小";
            // 
            // label150
            // 
            this.label150.AutoSize = true;
            this.label150.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label150.Location = new System.Drawing.Point(144, 25);
            this.label150.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label150.Name = "label150";
            this.label150.Size = new System.Drawing.Size(29, 12);
            this.label150.TabIndex = 10;
            this.label150.Text = "厘米";
            // 
            // VoteTableHeight
            // 
            this.VoteTableHeight.DecimalPlaces = 2;
            this.VoteTableHeight.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.VoteTableHeight.Location = new System.Drawing.Point(89, 23);
            this.VoteTableHeight.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTableHeight.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.VoteTableHeight.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.VoteTableHeight.Name = "VoteTableHeight";
            this.VoteTableHeight.Size = new System.Drawing.Size(51, 21);
            this.VoteTableHeight.TabIndex = 9;
            this.VoteTableHeight.Value = new decimal(new int[] {
            120,
            0,
            0,
            131072});
            // 
            // label149
            // 
            this.label149.AutoSize = true;
            this.label149.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label149.Location = new System.Drawing.Point(8, 25);
            this.label149.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label149.Name = "label149";
            this.label149.Size = new System.Drawing.Size(77, 12);
            this.label149.TabIndex = 8;
            this.label149.Text = "表格数据行高";
            // 
            // VoteTable_Random_PageCounts
            // 
            this.VoteTable_Random_PageCounts.AutoSize = true;
            this.VoteTable_Random_PageCounts.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VoteTable_Random_PageCounts.Location = new System.Drawing.Point(9, 139);
            this.VoteTable_Random_PageCounts.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Random_PageCounts.Name = "VoteTable_Random_PageCounts";
            this.VoteTable_Random_PageCounts.Size = new System.Drawing.Size(96, 16);
            this.VoteTable_Random_PageCounts.TabIndex = 8;
            this.VoteTable_Random_PageCounts.Text = "插入随机票数";
            this.VoteTable_Random_PageCounts.UseVisualStyleBackColor = true;
            // 
            // label142
            // 
            this.label142.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label142.ForeColor = System.Drawing.Color.Tomato;
            this.label142.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label142.Location = new System.Drawing.Point(9, 439);
            this.label142.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label142.Name = "label142";
            this.label142.Size = new System.Drawing.Size(521, 124);
            this.label142.TabIndex = 7;
            this.label142.Tag = "anchor_left_bottom";
            this.label142.Text = "\r\n1、不支持多类别报告单生成\r\n\r\n2、字体使用仿宋_GB2312和宋体，确保系统装有两种字体\r\n\r\n3、本功能仅用于快速生成报告单样式，不作他用\r\n\r\n4、空" +
    "白名字不会输出到报告单中";
            // 
            // VoteTable_Render
            // 
            this.VoteTable_Render.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VoteTable_Render.Location = new System.Drawing.Point(655, 439);
            this.VoteTable_Render.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Render.Name = "VoteTable_Render";
            this.VoteTable_Render.Size = new System.Drawing.Size(220, 33);
            this.VoteTable_Render.TabIndex = 0;
            this.VoteTable_Render.Tag = "anchor_right_top";
            this.VoteTable_Render.Text = "生成选举结果报告单";
            this.VoteTable_Render.UseVisualStyleBackColor = true;
            this.VoteTable_Render.Click += new System.EventHandler(this.VoteTable_Render_Click);
            // 
            // 报告单生成日期
            // 
            this.报告单生成日期.Controls.Add(this.VoteTableHelpDate);
            this.报告单生成日期.Controls.Add(this.VoteTable_GenDate);
            this.报告单生成日期.Location = new System.Drawing.Point(655, 153);
            this.报告单生成日期.Margin = new System.Windows.Forms.Padding(1);
            this.报告单生成日期.Name = "报告单生成日期";
            this.报告单生成日期.Padding = new System.Windows.Forms.Padding(1);
            this.报告单生成日期.Size = new System.Drawing.Size(220, 111);
            this.报告单生成日期.TabIndex = 6;
            this.报告单生成日期.TabStop = false;
            this.报告单生成日期.Tag = "anchor_right_top";
            this.报告单生成日期.Text = "报告单生成日期";
            // 
            // VoteTableHelpDate
            // 
            this.VoteTableHelpDate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VoteTableHelpDate.Location = new System.Drawing.Point(81, 73);
            this.VoteTableHelpDate.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTableHelpDate.Name = "VoteTableHelpDate";
            this.VoteTableHelpDate.Size = new System.Drawing.Size(89, 25);
            this.VoteTableHelpDate.TabIndex = 2;
            this.VoteTableHelpDate.Text = "填入当前日期";
            this.VoteTableHelpDate.UseVisualStyleBackColor = true;
            this.VoteTableHelpDate.Click += new System.EventHandler(this.VoteTableHelpDate_Click);
            // 
            // VoteTable_GenDate
            // 
            this.VoteTable_GenDate.AcceptsReturn = true;
            this.VoteTable_GenDate.AcceptsTab = true;
            this.VoteTable_GenDate.Location = new System.Drawing.Point(23, 25);
            this.VoteTable_GenDate.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_GenDate.Multiline = true;
            this.VoteTable_GenDate.Name = "VoteTable_GenDate";
            this.VoteTable_GenDate.Size = new System.Drawing.Size(195, 37);
            this.VoteTable_GenDate.TabIndex = 2;
            this.VoteTable_GenDate.Text = "XXXX年XX月XX日";
            // 
            // 报告单结束语
            // 
            this.报告单结束语.Controls.Add(this.VoteTable_End);
            this.报告单结束语.Location = new System.Drawing.Point(431, 5);
            this.报告单结束语.Margin = new System.Windows.Forms.Padding(1);
            this.报告单结束语.Name = "报告单结束语";
            this.报告单结束语.Padding = new System.Windows.Forms.Padding(1);
            this.报告单结束语.Size = new System.Drawing.Size(445, 145);
            this.报告单结束语.TabIndex = 5;
            this.报告单结束语.TabStop = false;
            this.报告单结束语.Tag = "anchor_top_left_right";
            this.报告单结束语.Text = "报告单结尾";
            // 
            // VoteTable_End
            // 
            this.VoteTable_End.AcceptsReturn = true;
            this.VoteTable_End.AcceptsTab = true;
            this.VoteTable_End.Location = new System.Drawing.Point(23, 23);
            this.VoteTable_End.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_End.Multiline = true;
            this.VoteTable_End.Name = "VoteTable_End";
            this.VoteTable_End.Size = new System.Drawing.Size(419, 111);
            this.VoteTable_End.TabIndex = 1;
            this.VoteTable_End.Tag = "anchor_top_left_right";
            this.VoteTable_End.Text = "\r\n总监票人：                    总计票人：";
            // 
            // groupBox52
            // 
            this.groupBox52.Controls.Add(this.VoteTable_Start);
            this.groupBox52.Location = new System.Drawing.Point(205, 153);
            this.groupBox52.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox52.Name = "groupBox52";
            this.groupBox52.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox52.Size = new System.Drawing.Size(447, 281);
            this.groupBox52.TabIndex = 4;
            this.groupBox52.TabStop = false;
            this.groupBox52.Tag = "anchor_full_fill";
            this.groupBox52.Text = "报告单正文";
            // 
            // VoteTable_Start
            // 
            this.VoteTable_Start.AcceptsReturn = true;
            this.VoteTable_Start.AcceptsTab = true;
            this.VoteTable_Start.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.VoteTable_Start.Location = new System.Drawing.Point(9, 17);
            this.VoteTable_Start.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Start.Multiline = true;
            this.VoteTable_Start.Name = "VoteTable_Start";
            this.VoteTable_Start.Size = new System.Drawing.Size(435, 225);
            this.VoteTable_Start.TabIndex = 1;
            this.VoteTable_Start.Tag = "anchor_full_fill";
            this.VoteTable_Start.Text = "\t本次大会实到XX人，共发出选票XX张，收回选票XX张，其中有效票XX张，无效票XX张。\r\n\t计票结果如下（按姓氏笔画为序）：";
            // 
            // groupBox51
            // 
            this.groupBox51.Controls.Add(this.VoteTable_Title);
            this.groupBox51.Location = new System.Drawing.Point(205, 4);
            this.groupBox51.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox51.Name = "groupBox51";
            this.groupBox51.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox51.Size = new System.Drawing.Size(221, 145);
            this.groupBox51.TabIndex = 3;
            this.groupBox51.TabStop = false;
            this.groupBox51.Tag = "anchor_left_top";
            this.groupBox51.Text = "报告单标题";
            // 
            // VoteTable_Title
            // 
            this.VoteTable_Title.AcceptsReturn = true;
            this.VoteTable_Title.AcceptsTab = true;
            this.VoteTable_Title.Location = new System.Drawing.Point(9, 23);
            this.VoteTable_Title.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Title.Multiline = true;
            this.VoteTable_Title.Name = "VoteTable_Title";
            this.VoteTable_Title.Size = new System.Drawing.Size(203, 112);
            this.VoteTable_Title.TabIndex = 0;
            this.VoteTable_Title.Text = "XX市XX次XX大会\r\nXX选举计票结果报告单";
            // 
            // groupBox50
            // 
            this.groupBox50.Controls.Add(this.VoteTableName_CountText);
            this.groupBox50.Controls.Add(this.VoteTable_Names);
            this.groupBox50.Location = new System.Drawing.Point(5, 153);
            this.groupBox50.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox50.Name = "groupBox50";
            this.groupBox50.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox50.Size = new System.Drawing.Size(195, 281);
            this.groupBox50.TabIndex = 2;
            this.groupBox50.TabStop = false;
            this.groupBox50.Tag = "anchor_top_left_bottom";
            this.groupBox50.Text = "报告单填充姓名信息";
            // 
            // VoteTableName_CountText
            // 
            this.VoteTableName_CountText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.VoteTableName_CountText.AutoSize = true;
            this.VoteTableName_CountText.Font = new System.Drawing.Font("宋体", 10.5F);
            this.VoteTableName_CountText.ForeColor = System.Drawing.Color.ForestGreen;
            this.VoteTableName_CountText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.VoteTableName_CountText.Location = new System.Drawing.Point(57, 251);
            this.VoteTableName_CountText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.VoteTableName_CountText.Name = "VoteTableName_CountText";
            this.VoteTableName_CountText.Size = new System.Drawing.Size(84, 14);
            this.VoteTableName_CountText.TabIndex = 1;
            this.VoteTableName_CountText.Tag = "anchor_bottom";
            this.VoteTableName_CountText.Text = "共有5个名字";
            // 
            // VoteTable_Names
            // 
            this.VoteTable_Names.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.VoteTable_Names.Location = new System.Drawing.Point(7, 17);
            this.VoteTable_Names.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Names.Name = "VoteTable_Names";
            this.VoteTable_Names.Size = new System.Drawing.Size(187, 220);
            this.VoteTable_Names.TabIndex = 0;
            this.VoteTable_Names.Tag = "anchor_top_left_bottom";
            this.VoteTable_Names.Text = "姓名1\n姓名2\n姓名3\n姓名4\n姓名5";
            this.VoteTable_Names.TextChanged += new System.EventHandler(this.VoteTable_Names_TextChanged);
            // 
            // groupBox49
            // 
            this.groupBox49.Controls.Add(this.VoteTable_Abdon);
            this.groupBox49.Controls.Add(this.label141);
            this.groupBox49.Controls.Add(this.VoteTable_UnAgree);
            this.groupBox49.Controls.Add(this.label140);
            this.groupBox49.Controls.Add(this.VoteTable_Agree);
            this.groupBox49.Controls.Add(this.label139);
            this.groupBox49.Controls.Add(this.VoteTable_Name);
            this.groupBox49.Controls.Add(this.label138);
            this.groupBox49.Location = new System.Drawing.Point(5, 5);
            this.groupBox49.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox49.Name = "groupBox49";
            this.groupBox49.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox49.Size = new System.Drawing.Size(195, 145);
            this.groupBox49.TabIndex = 1;
            this.groupBox49.TabStop = false;
            this.groupBox49.Tag = "anchor_left_top";
            this.groupBox49.Text = "报告单表头信息";
            // 
            // VoteTable_Abdon
            // 
            this.VoteTable_Abdon.Location = new System.Drawing.Point(49, 112);
            this.VoteTable_Abdon.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Abdon.Name = "VoteTable_Abdon";
            this.VoteTable_Abdon.Size = new System.Drawing.Size(143, 21);
            this.VoteTable_Abdon.TabIndex = 7;
            this.VoteTable_Abdon.Text = "弃权票";
            // 
            // label141
            // 
            this.label141.AutoSize = true;
            this.label141.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label141.Location = new System.Drawing.Point(4, 116);
            this.label141.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label141.Name = "label141";
            this.label141.Size = new System.Drawing.Size(41, 12);
            this.label141.TabIndex = 6;
            this.label141.Text = "弃权列";
            // 
            // VoteTable_UnAgree
            // 
            this.VoteTable_UnAgree.Location = new System.Drawing.Point(49, 81);
            this.VoteTable_UnAgree.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_UnAgree.Name = "VoteTable_UnAgree";
            this.VoteTable_UnAgree.Size = new System.Drawing.Size(143, 21);
            this.VoteTable_UnAgree.TabIndex = 5;
            this.VoteTable_UnAgree.Text = "不赞成票";
            // 
            // label140
            // 
            this.label140.AutoSize = true;
            this.label140.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label140.Location = new System.Drawing.Point(4, 85);
            this.label140.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label140.Name = "label140";
            this.label140.Size = new System.Drawing.Size(41, 12);
            this.label140.TabIndex = 4;
            this.label140.Text = "反对列";
            // 
            // VoteTable_Agree
            // 
            this.VoteTable_Agree.Location = new System.Drawing.Point(49, 51);
            this.VoteTable_Agree.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Agree.Name = "VoteTable_Agree";
            this.VoteTable_Agree.Size = new System.Drawing.Size(143, 21);
            this.VoteTable_Agree.TabIndex = 3;
            this.VoteTable_Agree.Text = "赞成票";
            // 
            // label139
            // 
            this.label139.AutoSize = true;
            this.label139.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label139.Location = new System.Drawing.Point(4, 55);
            this.label139.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label139.Name = "label139";
            this.label139.Size = new System.Drawing.Size(41, 12);
            this.label139.TabIndex = 2;
            this.label139.Text = "赞成列";
            // 
            // VoteTable_Name
            // 
            this.VoteTable_Name.Location = new System.Drawing.Point(49, 20);
            this.VoteTable_Name.Margin = new System.Windows.Forms.Padding(1);
            this.VoteTable_Name.Name = "VoteTable_Name";
            this.VoteTable_Name.Size = new System.Drawing.Size(143, 21);
            this.VoteTable_Name.TabIndex = 1;
            this.VoteTable_Name.Text = "姓名";
            // 
            // label138
            // 
            this.label138.AutoSize = true;
            this.label138.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label138.Location = new System.Drawing.Point(4, 24);
            this.label138.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label138.Name = "label138";
            this.label138.Size = new System.Drawing.Size(41, 12);
            this.label138.TabIndex = 0;
            this.label138.Text = "姓名列";
            // 
            // TabPage_36
            // 
            this.TabPage_36.Controls.Add(this.groupBox60);
            this.TabPage_36.Controls.Add(this.groupBox28);
            this.TabPage_36.Controls.Add(this.groupBox46);
            this.TabPage_36.Controls.Add(this.groupBox43);
            this.TabPage_36.Location = new System.Drawing.Point(4, 76);
            this.TabPage_36.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_36.Name = "TabPage_36";
            this.TabPage_36.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_36.Size = new System.Drawing.Size(882, 659);
            this.TabPage_36.TabIndex = 35;
            this.TabPage_36.Text = "文件副本生成";
            this.TabPage_36.UseVisualStyleBackColor = true;
            // 
            // groupBox60
            // 
            this.groupBox60.Controls.Add(this.label163);
            this.groupBox60.Controls.Add(this.label37);
            this.groupBox60.Controls.Add(this.GetNumberIndexList);
            this.groupBox60.Controls.Add(this.NumberIndexEnd);
            this.groupBox60.Controls.Add(this.NumberIndexStart);
            this.groupBox60.Controls.Add(this.CopyNumberIndexResult);
            this.groupBox60.Controls.Add(this.NumberIndexResultBox);
            this.groupBox60.Location = new System.Drawing.Point(391, 204);
            this.groupBox60.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox60.Name = "groupBox60";
            this.groupBox60.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox60.Size = new System.Drawing.Size(299, 165);
            this.groupBox60.TabIndex = 0;
            this.groupBox60.TabStop = false;
            this.groupBox60.Text = "序号生成";
            // 
            // label163
            // 
            this.label163.AutoSize = true;
            this.label163.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label163.Location = new System.Drawing.Point(19, 71);
            this.label163.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label163.Name = "label163";
            this.label163.Size = new System.Drawing.Size(53, 12);
            this.label163.TabIndex = 26;
            this.label163.Text = "结束序号";
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label37.Location = new System.Drawing.Point(19, 35);
            this.label37.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(53, 12);
            this.label37.TabIndex = 25;
            this.label37.Text = "起始序号";
            // 
            // GetNumberIndexList
            // 
            this.GetNumberIndexList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GetNumberIndexList.Location = new System.Drawing.Point(21, 135);
            this.GetNumberIndexList.Margin = new System.Windows.Forms.Padding(1);
            this.GetNumberIndexList.Name = "GetNumberIndexList";
            this.GetNumberIndexList.Size = new System.Drawing.Size(80, 24);
            this.GetNumberIndexList.TabIndex = 0;
            this.GetNumberIndexList.Text = "开始生成";
            this.GetNumberIndexList.UseVisualStyleBackColor = true;
            this.GetNumberIndexList.Click += new System.EventHandler(this.GetNumberIndexList_Click);
            // 
            // NumberIndexEnd
            // 
            this.NumberIndexEnd.Location = new System.Drawing.Point(76, 67);
            this.NumberIndexEnd.Margin = new System.Windows.Forms.Padding(1);
            this.NumberIndexEnd.Maximum = new decimal(new int[] {
            999999999,
            0,
            0,
            0});
            this.NumberIndexEnd.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.NumberIndexEnd.Name = "NumberIndexEnd";
            this.NumberIndexEnd.Size = new System.Drawing.Size(80, 21);
            this.NumberIndexEnd.TabIndex = 1;
            this.NumberIndexEnd.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // NumberIndexStart
            // 
            this.NumberIndexStart.Location = new System.Drawing.Point(76, 32);
            this.NumberIndexStart.Margin = new System.Windows.Forms.Padding(1);
            this.NumberIndexStart.Maximum = new decimal(new int[] {
            99999999,
            0,
            0,
            0});
            this.NumberIndexStart.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumberIndexStart.Name = "NumberIndexStart";
            this.NumberIndexStart.Size = new System.Drawing.Size(80, 21);
            this.NumberIndexStart.TabIndex = 2;
            this.NumberIndexStart.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // CopyNumberIndexResult
            // 
            this.CopyNumberIndexResult.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CopyNumberIndexResult.Location = new System.Drawing.Point(204, 135);
            this.CopyNumberIndexResult.Margin = new System.Windows.Forms.Padding(1);
            this.CopyNumberIndexResult.Name = "CopyNumberIndexResult";
            this.CopyNumberIndexResult.Size = new System.Drawing.Size(77, 24);
            this.CopyNumberIndexResult.TabIndex = 23;
            this.CopyNumberIndexResult.Text = "复制结果";
            this.CopyNumberIndexResult.UseVisualStyleBackColor = true;
            this.CopyNumberIndexResult.Click += new System.EventHandler(this.CopyNumberIndexResult_Click);
            // 
            // NumberIndexResultBox
            // 
            this.NumberIndexResultBox.Location = new System.Drawing.Point(204, 9);
            this.NumberIndexResultBox.Margin = new System.Windows.Forms.Padding(1);
            this.NumberIndexResultBox.Name = "NumberIndexResultBox";
            this.NumberIndexResultBox.Size = new System.Drawing.Size(77, 104);
            this.NumberIndexResultBox.TabIndex = 24;
            this.NumberIndexResultBox.Text = "";
            // 
            // groupBox28
            // 
            this.groupBox28.Controls.Add(this.label175);
            this.groupBox28.Controls.Add(this.GetRandomComputerNameRandLength);
            this.groupBox28.Controls.Add(this.GetRandomComputerNameButton);
            this.groupBox28.Controls.Add(this.ExamComputerNameChange);
            this.groupBox28.Controls.Add(this.ExamComputerChangeName);
            this.groupBox28.Controls.Add(this.label174);
            this.groupBox28.Controls.Add(this.ExamComputerNames);
            this.groupBox28.Controls.Add(this.label58);
            this.groupBox28.Controls.Add(this.ChooseExamFileButton);
            this.groupBox28.Controls.Add(this.ChooseExamFileTextBox);
            this.groupBox28.Controls.Add(this.label5);
            this.groupBox28.Location = new System.Drawing.Point(7, 384);
            this.groupBox28.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox28.Name = "groupBox28";
            this.groupBox28.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox28.Size = new System.Drawing.Size(871, 159);
            this.groupBox28.TabIndex = 3;
            this.groupBox28.TabStop = false;
            this.groupBox28.Tag = "anchor_top_left_right";
            this.groupBox28.Text = "考评系统数据包计算机名称修改";
            // 
            // label175
            // 
            this.label175.AutoSize = true;
            this.label175.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label175.Location = new System.Drawing.Point(451, 105);
            this.label175.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label175.Name = "label175";
            this.label175.Size = new System.Drawing.Size(77, 12);
            this.label175.TabIndex = 21;
            this.label175.Text = "随机字符长度";
            // 
            // GetRandomComputerNameRandLength
            // 
            this.GetRandomComputerNameRandLength.Location = new System.Drawing.Point(533, 101);
            this.GetRandomComputerNameRandLength.Margin = new System.Windows.Forms.Padding(1);
            this.GetRandomComputerNameRandLength.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.GetRandomComputerNameRandLength.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.GetRandomComputerNameRandLength.Name = "GetRandomComputerNameRandLength";
            this.GetRandomComputerNameRandLength.Size = new System.Drawing.Size(80, 21);
            this.GetRandomComputerNameRandLength.TabIndex = 20;
            this.GetRandomComputerNameRandLength.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // GetRandomComputerNameButton
            // 
            this.GetRandomComputerNameButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.GetRandomComputerNameButton.Location = new System.Drawing.Point(617, 97);
            this.GetRandomComputerNameButton.Margin = new System.Windows.Forms.Padding(1);
            this.GetRandomComputerNameButton.Name = "GetRandomComputerNameButton";
            this.GetRandomComputerNameButton.Size = new System.Drawing.Size(147, 23);
            this.GetRandomComputerNameButton.TabIndex = 19;
            this.GetRandomComputerNameButton.Text = "生成随机名称";
            this.GetRandomComputerNameButton.UseVisualStyleBackColor = true;
            this.GetRandomComputerNameButton.Click += new System.EventHandler(this.GetRandomComputerNameButton_Click);
            // 
            // ExamComputerNameChange
            // 
            this.ExamComputerNameChange.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ExamComputerNameChange.Location = new System.Drawing.Point(769, 73);
            this.ExamComputerNameChange.Margin = new System.Windows.Forms.Padding(1);
            this.ExamComputerNameChange.Name = "ExamComputerNameChange";
            this.ExamComputerNameChange.Size = new System.Drawing.Size(97, 22);
            this.ExamComputerNameChange.TabIndex = 18;
            this.ExamComputerNameChange.Tag = "anchor_right_top";
            this.ExamComputerNameChange.Text = "执行修改";
            this.MainToolsTip.SetToolTip(this.ExamComputerNameChange, resources.GetString("ExamComputerNameChange.ToolTip"));
            this.ExamComputerNameChange.UseVisualStyleBackColor = true;
            this.ExamComputerNameChange.Click += new System.EventHandler(this.ExamComputerNameChange_Click);
            // 
            // ExamComputerChangeName
            // 
            this.ExamComputerChangeName.Location = new System.Drawing.Point(453, 73);
            this.ExamComputerChangeName.Margin = new System.Windows.Forms.Padding(1);
            this.ExamComputerChangeName.MaxLength = 2;
            this.ExamComputerChangeName.Name = "ExamComputerChangeName";
            this.ExamComputerChangeName.Size = new System.Drawing.Size(312, 21);
            this.ExamComputerChangeName.TabIndex = 14;
            this.ExamComputerChangeName.Tag = "anchor_top_left_right";
            // 
            // label174
            // 
            this.label174.AutoSize = true;
            this.label174.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label174.Location = new System.Drawing.Point(408, 79);
            this.label174.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label174.Name = "label174";
            this.label174.Size = new System.Drawing.Size(41, 12);
            this.label174.TabIndex = 17;
            this.label174.Text = "修改为";
            // 
            // ExamComputerNames
            // 
            this.ExamComputerNames.FormattingEnabled = true;
            this.ExamComputerNames.Location = new System.Drawing.Point(104, 73);
            this.ExamComputerNames.Margin = new System.Windows.Forms.Padding(1);
            this.ExamComputerNames.Name = "ExamComputerNames";
            this.ExamComputerNames.Size = new System.Drawing.Size(301, 20);
            this.ExamComputerNames.TabIndex = 16;
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label58.Location = new System.Drawing.Point(11, 77);
            this.label58.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(89, 12);
            this.label58.TabIndex = 15;
            this.label58.Text = "计算机名称列表";
            // 
            // ChooseExamFileButton
            // 
            this.ChooseExamFileButton.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.ChooseExamFileButton.Location = new System.Drawing.Point(769, 25);
            this.ChooseExamFileButton.Margin = new System.Windows.Forms.Padding(1);
            this.ChooseExamFileButton.Name = "ChooseExamFileButton";
            this.ChooseExamFileButton.Size = new System.Drawing.Size(97, 22);
            this.ChooseExamFileButton.TabIndex = 14;
            this.ChooseExamFileButton.Tag = "anchor_right_top";
            this.ChooseExamFileButton.Text = "选择文件...";
            this.ChooseExamFileButton.UseVisualStyleBackColor = true;
            this.ChooseExamFileButton.Click += new System.EventHandler(this.ChooseExamFileButton_Click);
            // 
            // ChooseExamFileTextBox
            // 
            this.ChooseExamFileTextBox.Location = new System.Drawing.Point(104, 25);
            this.ChooseExamFileTextBox.Margin = new System.Windows.Forms.Padding(1);
            this.ChooseExamFileTextBox.MaxLength = 2;
            this.ChooseExamFileTextBox.Name = "ChooseExamFileTextBox";
            this.ChooseExamFileTextBox.ReadOnly = true;
            this.ChooseExamFileTextBox.Size = new System.Drawing.Size(661, 21);
            this.ChooseExamFileTextBox.TabIndex = 14;
            this.ChooseExamFileTextBox.Tag = "anchor_top_left_right";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label5.Location = new System.Drawing.Point(11, 28);
            this.label5.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "数据包文件路径";
            // 
            // groupBox46
            // 
            this.groupBox46.Controls.Add(this.money_big);
            this.groupBox46.Controls.Add(this.money_small);
            this.groupBox46.Controls.Add(this.label131);
            this.groupBox46.Controls.Add(this.label130);
            this.groupBox46.Controls.Add(this.copy_money_big_result);
            this.groupBox46.Location = new System.Drawing.Point(4, 204);
            this.groupBox46.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox46.Name = "groupBox46";
            this.groupBox46.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox46.Size = new System.Drawing.Size(383, 165);
            this.groupBox46.TabIndex = 2;
            this.groupBox46.TabStop = false;
            this.groupBox46.Text = "金额大写转换";
            // 
            // money_big
            // 
            this.money_big.Font = new System.Drawing.Font("宋体", 9F);
            this.money_big.Location = new System.Drawing.Point(63, 103);
            this.money_big.Margin = new System.Windows.Forms.Padding(1);
            this.money_big.Multiline = true;
            this.money_big.Name = "money_big";
            this.money_big.Size = new System.Drawing.Size(315, 19);
            this.money_big.TabIndex = 17;
            // 
            // money_small
            // 
            this.money_small.Font = new System.Drawing.Font("宋体", 9F);
            this.money_small.Location = new System.Drawing.Point(63, 41);
            this.money_small.Margin = new System.Windows.Forms.Padding(1);
            this.money_small.MaxLength = 13;
            this.money_small.Multiline = true;
            this.money_small.Name = "money_small";
            this.money_small.Size = new System.Drawing.Size(315, 19);
            this.money_small.TabIndex = 16;
            this.money_small.TextChanged += new System.EventHandler(this.money_small_TextChanged);
            // 
            // label131
            // 
            this.label131.AutoSize = true;
            this.label131.Font = new System.Drawing.Font("宋体", 9F);
            this.label131.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label131.Location = new System.Drawing.Point(7, 107);
            this.label131.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label131.Name = "label131";
            this.label131.Size = new System.Drawing.Size(53, 12);
            this.label131.TabIndex = 15;
            this.label131.Text = "大写数字";
            // 
            // label130
            // 
            this.label130.AutoSize = true;
            this.label130.Font = new System.Drawing.Font("宋体", 9F);
            this.label130.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label130.Location = new System.Drawing.Point(7, 45);
            this.label130.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label130.Name = "label130";
            this.label130.Size = new System.Drawing.Size(53, 12);
            this.label130.TabIndex = 14;
            this.label130.Text = "小写数字";
            // 
            // copy_money_big_result
            // 
            this.copy_money_big_result.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.copy_money_big_result.Location = new System.Drawing.Point(273, 135);
            this.copy_money_big_result.Margin = new System.Windows.Forms.Padding(1);
            this.copy_money_big_result.Name = "copy_money_big_result";
            this.copy_money_big_result.Size = new System.Drawing.Size(104, 25);
            this.copy_money_big_result.TabIndex = 0;
            this.copy_money_big_result.Text = "复制结果";
            this.copy_money_big_result.UseVisualStyleBackColor = true;
            this.copy_money_big_result.Click += new System.EventHandler(this.copy_money_big_result_Click);
            // 
            // groupBox43
            // 
            this.groupBox43.Controls.Add(this.copied_number_prefix_count);
            this.groupBox43.Controls.Add(this.label124);
            this.groupBox43.Controls.Add(this.CopiedNamerText);
            this.groupBox43.Controls.Add(this.Gen_Copied_Namer);
            this.groupBox43.Controls.Add(this.label109);
            this.groupBox43.Controls.Add(this.Gen_Copied_StatusText);
            this.groupBox43.Controls.Add(this.Gen_Copied_Number);
            this.groupBox43.Controls.Add(this.CopiedGen);
            this.groupBox43.Controls.Add(this.label108);
            this.groupBox43.Controls.Add(this.Gen_Copied_Path);
            this.groupBox43.Controls.Add(this.Gen_Copied_File);
            this.groupBox43.Controls.Add(this.Gen_Copied_TargetPath);
            this.groupBox43.Controls.Add(this.Gen_Copied_SourceFileBox);
            this.groupBox43.Controls.Add(this.label107);
            this.groupBox43.Controls.Add(this.label106);
            this.groupBox43.Location = new System.Drawing.Point(7, 4);
            this.groupBox43.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox43.Name = "groupBox43";
            this.groupBox43.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox43.Size = new System.Drawing.Size(872, 188);
            this.groupBox43.TabIndex = 1;
            this.groupBox43.TabStop = false;
            this.groupBox43.Tag = "anchor_top_left_right";
            this.groupBox43.Text = "基础信息设置";
            // 
            // copied_number_prefix_count
            // 
            this.copied_number_prefix_count.Location = new System.Drawing.Point(92, 131);
            this.copied_number_prefix_count.Margin = new System.Windows.Forms.Padding(1);
            this.copied_number_prefix_count.MaxLength = 2;
            this.copied_number_prefix_count.Name = "copied_number_prefix_count";
            this.copied_number_prefix_count.Size = new System.Drawing.Size(83, 21);
            this.copied_number_prefix_count.TabIndex = 13;
            this.copied_number_prefix_count.Text = "1";
            this.copied_number_prefix_count.TextChanged += new System.EventHandler(this.copied_number_prefix_count_TextChanged);
            // 
            // label124
            // 
            this.label124.AutoSize = true;
            this.label124.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label124.Location = new System.Drawing.Point(11, 133);
            this.label124.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label124.Name = "label124";
            this.label124.Size = new System.Drawing.Size(53, 12);
            this.label124.TabIndex = 12;
            this.label124.Text = "序号位数";
            // 
            // CopiedNamerText
            // 
            this.CopiedNamerText.AutoSize = true;
            this.CopiedNamerText.ForeColor = System.Drawing.Color.Tomato;
            this.CopiedNamerText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CopiedNamerText.Location = new System.Drawing.Point(177, 107);
            this.CopiedNamerText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.CopiedNamerText.Name = "CopiedNamerText";
            this.CopiedNamerText.Size = new System.Drawing.Size(11, 12);
            this.CopiedNamerText.TabIndex = 11;
            this.CopiedNamerText.Text = "-";
            // 
            // Gen_Copied_Namer
            // 
            this.Gen_Copied_Namer.Location = new System.Drawing.Point(92, 103);
            this.Gen_Copied_Namer.Margin = new System.Windows.Forms.Padding(1);
            this.Gen_Copied_Namer.MaxLength = 1000;
            this.Gen_Copied_Namer.Name = "Gen_Copied_Namer";
            this.Gen_Copied_Namer.Size = new System.Drawing.Size(83, 21);
            this.Gen_Copied_Namer.TabIndex = 10;
            this.Gen_Copied_Namer.Text = "副本";
            this.Gen_Copied_Namer.TextChanged += new System.EventHandler(this.Gen_Copied_Namer_TextChanged);
            // 
            // label109
            // 
            this.label109.AutoSize = true;
            this.label109.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label109.Location = new System.Drawing.Point(11, 107);
            this.label109.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label109.Name = "label109";
            this.label109.Size = new System.Drawing.Size(77, 12);
            this.label109.TabIndex = 9;
            this.label109.Text = "副本字段设置";
            // 
            // Gen_Copied_StatusText
            // 
            this.Gen_Copied_StatusText.AutoSize = true;
            this.Gen_Copied_StatusText.ForeColor = System.Drawing.Color.ForestGreen;
            this.Gen_Copied_StatusText.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Gen_Copied_StatusText.Location = new System.Drawing.Point(243, 163);
            this.Gen_Copied_StatusText.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.Gen_Copied_StatusText.Name = "Gen_Copied_StatusText";
            this.Gen_Copied_StatusText.Size = new System.Drawing.Size(59, 12);
            this.Gen_Copied_StatusText.TabIndex = 8;
            this.Gen_Copied_StatusText.Text = "等待中...";
            // 
            // Gen_Copied_Number
            // 
            this.Gen_Copied_Number.Location = new System.Drawing.Point(93, 157);
            this.Gen_Copied_Number.Margin = new System.Windows.Forms.Padding(1);
            this.Gen_Copied_Number.Maximum = new decimal(new int[] {
            900000000,
            0,
            0,
            0});
            this.Gen_Copied_Number.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.Gen_Copied_Number.Name = "Gen_Copied_Number";
            this.Gen_Copied_Number.Size = new System.Drawing.Size(80, 21);
            this.Gen_Copied_Number.TabIndex = 7;
            this.Gen_Copied_Number.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // CopiedGen
            // 
            this.CopiedGen.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.CopiedGen.Location = new System.Drawing.Point(725, 147);
            this.CopiedGen.Margin = new System.Windows.Forms.Padding(1);
            this.CopiedGen.Name = "CopiedGen";
            this.CopiedGen.Size = new System.Drawing.Size(133, 29);
            this.CopiedGen.TabIndex = 0;
            this.CopiedGen.Tag = "anchor_right_bottom";
            this.CopiedGen.Text = "开始执行";
            this.CopiedGen.UseVisualStyleBackColor = true;
            this.CopiedGen.Click += new System.EventHandler(this.CopiedGen_Click);
            // 
            // label108
            // 
            this.label108.AutoSize = true;
            this.label108.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label108.Location = new System.Drawing.Point(11, 160);
            this.label108.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label108.Name = "label108";
            this.label108.Size = new System.Drawing.Size(77, 12);
            this.label108.TabIndex = 6;
            this.label108.Text = "生成副本数量";
            // 
            // Gen_Copied_Path
            // 
            this.Gen_Copied_Path.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Gen_Copied_Path.Location = new System.Drawing.Point(725, 68);
            this.Gen_Copied_Path.Margin = new System.Windows.Forms.Padding(1);
            this.Gen_Copied_Path.Name = "Gen_Copied_Path";
            this.Gen_Copied_Path.Size = new System.Drawing.Size(133, 22);
            this.Gen_Copied_Path.TabIndex = 5;
            this.Gen_Copied_Path.Tag = "anchor_right_top";
            this.Gen_Copied_Path.Text = "选择目录...";
            this.Gen_Copied_Path.UseVisualStyleBackColor = true;
            this.Gen_Copied_Path.Click += new System.EventHandler(this.Gen_Copied_Path_Click);
            // 
            // Gen_Copied_File
            // 
            this.Gen_Copied_File.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Gen_Copied_File.Location = new System.Drawing.Point(725, 27);
            this.Gen_Copied_File.Margin = new System.Windows.Forms.Padding(1);
            this.Gen_Copied_File.Name = "Gen_Copied_File";
            this.Gen_Copied_File.Size = new System.Drawing.Size(133, 22);
            this.Gen_Copied_File.TabIndex = 4;
            this.Gen_Copied_File.Tag = "anchor_right_top";
            this.Gen_Copied_File.Text = "选择文件...";
            this.Gen_Copied_File.UseVisualStyleBackColor = true;
            this.Gen_Copied_File.Click += new System.EventHandler(this.Gen_Copied_File_Click);
            // 
            // Gen_Copied_TargetPath
            // 
            this.Gen_Copied_TargetPath.Location = new System.Drawing.Point(92, 67);
            this.Gen_Copied_TargetPath.Margin = new System.Windows.Forms.Padding(1);
            this.Gen_Copied_TargetPath.Name = "Gen_Copied_TargetPath";
            this.Gen_Copied_TargetPath.ReadOnly = true;
            this.Gen_Copied_TargetPath.Size = new System.Drawing.Size(631, 21);
            this.Gen_Copied_TargetPath.TabIndex = 3;
            this.Gen_Copied_TargetPath.Tag = "anchor_top_left_right";
            // 
            // Gen_Copied_SourceFileBox
            // 
            this.Gen_Copied_SourceFileBox.Location = new System.Drawing.Point(92, 27);
            this.Gen_Copied_SourceFileBox.Margin = new System.Windows.Forms.Padding(1);
            this.Gen_Copied_SourceFileBox.Name = "Gen_Copied_SourceFileBox";
            this.Gen_Copied_SourceFileBox.ReadOnly = true;
            this.Gen_Copied_SourceFileBox.Size = new System.Drawing.Size(631, 21);
            this.Gen_Copied_SourceFileBox.TabIndex = 2;
            this.Gen_Copied_SourceFileBox.Tag = "anchor_top_left_right";
            this.Gen_Copied_SourceFileBox.TextChanged += new System.EventHandler(this.Gen_Copied_SourceFileBox_TextChanged);
            // 
            // label107
            // 
            this.label107.AutoSize = true;
            this.label107.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label107.Location = new System.Drawing.Point(11, 71);
            this.label107.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label107.Name = "label107";
            this.label107.Size = new System.Drawing.Size(77, 12);
            this.label107.TabIndex = 1;
            this.label107.Text = "文件输出目录";
            // 
            // label106
            // 
            this.label106.AutoSize = true;
            this.label106.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label106.Location = new System.Drawing.Point(11, 31);
            this.label106.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label106.Name = "label106";
            this.label106.Size = new System.Drawing.Size(77, 12);
            this.label106.TabIndex = 0;
            this.label106.Text = "生成副本文件";
            // 
            // TabPage_37
            // 
            this.TabPage_37.Controls.Add(this.label123);
            this.TabPage_37.Controls.Add(this.groupBox45);
            this.TabPage_37.Controls.Add(this.groupBox44);
            this.TabPage_37.Location = new System.Drawing.Point(4, 76);
            this.TabPage_37.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_37.Name = "TabPage_37";
            this.TabPage_37.Padding = new System.Windows.Forms.Padding(1);
            this.TabPage_37.Size = new System.Drawing.Size(882, 659);
            this.TabPage_37.TabIndex = 36;
            this.TabPage_37.Text = "网卡设置";
            this.TabPage_37.UseVisualStyleBackColor = true;
            // 
            // label123
            // 
            this.label123.AutoSize = true;
            this.label123.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label123.ForeColor = System.Drawing.Color.Tomato;
            this.label123.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label123.Location = new System.Drawing.Point(11, 491);
            this.label123.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label123.Name = "label123";
            this.label123.Size = new System.Drawing.Size(798, 112);
            this.label123.TabIndex = 2;
            this.label123.Tag = "anchor_left_bottom";
            this.label123.Text = resources.GetString("label123.Text");
            // 
            // groupBox45
            // 
            this.groupBox45.Controls.Add(this.set_interface_load);
            this.groupBox45.Controls.Add(this.set_interface_export);
            this.groupBox45.Controls.Add(this.set_interface_reset);
            this.groupBox45.Controls.Add(this.set_interface_start);
            this.groupBox45.Controls.Add(this.set_interface_dns2);
            this.groupBox45.Controls.Add(this.set_interface_dns1);
            this.groupBox45.Controls.Add(this.set_interface_gateway);
            this.groupBox45.Controls.Add(this.set_interface_mask);
            this.groupBox45.Controls.Add(this.set_interface_ipaddress);
            this.groupBox45.Controls.Add(this.set_interface_mode_static);
            this.groupBox45.Controls.Add(this.set_interface_mode_auto);
            this.groupBox45.Controls.Add(this.label121);
            this.groupBox45.Controls.Add(this.label120);
            this.groupBox45.Controls.Add(this.label119);
            this.groupBox45.Controls.Add(this.label118);
            this.groupBox45.Controls.Add(this.label117);
            this.groupBox45.Controls.Add(this.set_interface_name_list);
            this.groupBox45.Controls.Add(this.label116);
            this.groupBox45.Location = new System.Drawing.Point(533, 4);
            this.groupBox45.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox45.Name = "groupBox45";
            this.groupBox45.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox45.Size = new System.Drawing.Size(329, 311);
            this.groupBox45.TabIndex = 1;
            this.groupBox45.TabStop = false;
            this.groupBox45.Tag = "anchor_right_top";
            this.groupBox45.Text = "IP地址修改";
            // 
            // set_interface_load
            // 
            this.set_interface_load.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.set_interface_load.Location = new System.Drawing.Point(171, 265);
            this.set_interface_load.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_load.Name = "set_interface_load";
            this.set_interface_load.Size = new System.Drawing.Size(85, 35);
            this.set_interface_load.TabIndex = 27;
            this.set_interface_load.Text = "加载外部配置";
            this.set_interface_load.UseVisualStyleBackColor = true;
            this.set_interface_load.Click += new System.EventHandler(this.set_interface_load_Click);
            // 
            // set_interface_export
            // 
            this.set_interface_export.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.set_interface_export.Location = new System.Drawing.Point(67, 265);
            this.set_interface_export.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_export.Name = "set_interface_export";
            this.set_interface_export.Size = new System.Drawing.Size(93, 35);
            this.set_interface_export.TabIndex = 26;
            this.set_interface_export.Text = "导出当前配置";
            this.set_interface_export.UseVisualStyleBackColor = true;
            this.set_interface_export.Click += new System.EventHandler(this.set_interface_export_Click);
            // 
            // set_interface_reset
            // 
            this.set_interface_reset.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.set_interface_reset.Location = new System.Drawing.Point(7, 265);
            this.set_interface_reset.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_reset.Name = "set_interface_reset";
            this.set_interface_reset.Size = new System.Drawing.Size(51, 35);
            this.set_interface_reset.TabIndex = 25;
            this.set_interface_reset.Text = "重置";
            this.set_interface_reset.UseVisualStyleBackColor = true;
            this.set_interface_reset.Click += new System.EventHandler(this.set_interface_clear_Click);
            // 
            // set_interface_start
            // 
            this.set_interface_start.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.set_interface_start.Location = new System.Drawing.Point(265, 265);
            this.set_interface_start.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_start.Name = "set_interface_start";
            this.set_interface_start.Size = new System.Drawing.Size(53, 35);
            this.set_interface_start.TabIndex = 24;
            this.set_interface_start.Text = "修改";
            this.set_interface_start.UseVisualStyleBackColor = true;
            this.set_interface_start.Click += new System.EventHandler(this.set_interface_start_Click);
            // 
            // set_interface_dns2
            // 
            this.set_interface_dns2.Location = new System.Drawing.Point(91, 228);
            this.set_interface_dns2.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_dns2.MaxLength = 12;
            this.set_interface_dns2.Name = "set_interface_dns2";
            this.set_interface_dns2.Size = new System.Drawing.Size(229, 21);
            this.set_interface_dns2.TabIndex = 4;
            this.set_interface_dns2.Text = "0.0.0.0";
            this.set_interface_dns2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.set_interface_dns2.WordWrap = false;
            // 
            // set_interface_dns1
            // 
            this.set_interface_dns1.Location = new System.Drawing.Point(91, 197);
            this.set_interface_dns1.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_dns1.MaxLength = 12;
            this.set_interface_dns1.Name = "set_interface_dns1";
            this.set_interface_dns1.Size = new System.Drawing.Size(229, 21);
            this.set_interface_dns1.TabIndex = 22;
            this.set_interface_dns1.Text = "0.0.0.0";
            this.set_interface_dns1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.set_interface_dns1.WordWrap = false;
            // 
            // set_interface_gateway
            // 
            this.set_interface_gateway.Location = new System.Drawing.Point(61, 160);
            this.set_interface_gateway.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_gateway.MaxLength = 12;
            this.set_interface_gateway.Name = "set_interface_gateway";
            this.set_interface_gateway.Size = new System.Drawing.Size(259, 21);
            this.set_interface_gateway.TabIndex = 21;
            this.set_interface_gateway.Text = "0.0.0.0";
            this.set_interface_gateway.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.set_interface_gateway.WordWrap = false;
            // 
            // set_interface_mask
            // 
            this.set_interface_mask.Location = new System.Drawing.Point(61, 129);
            this.set_interface_mask.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_mask.MaxLength = 12;
            this.set_interface_mask.Name = "set_interface_mask";
            this.set_interface_mask.Size = new System.Drawing.Size(259, 21);
            this.set_interface_mask.TabIndex = 20;
            this.set_interface_mask.Text = "0.0.0.0";
            this.set_interface_mask.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.set_interface_mask.WordWrap = false;
            // 
            // set_interface_ipaddress
            // 
            this.set_interface_ipaddress.Location = new System.Drawing.Point(61, 91);
            this.set_interface_ipaddress.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_ipaddress.MaxLength = 12;
            this.set_interface_ipaddress.Name = "set_interface_ipaddress";
            this.set_interface_ipaddress.Size = new System.Drawing.Size(259, 21);
            this.set_interface_ipaddress.TabIndex = 19;
            this.set_interface_ipaddress.Text = "0.0.0.0";
            this.set_interface_ipaddress.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.set_interface_ipaddress.WordWrap = false;
            // 
            // set_interface_mode_static
            // 
            this.set_interface_mode_static.AutoSize = true;
            this.set_interface_mode_static.Checked = true;
            this.set_interface_mode_static.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.set_interface_mode_static.Location = new System.Drawing.Point(249, 57);
            this.set_interface_mode_static.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_mode_static.Name = "set_interface_mode_static";
            this.set_interface_mode_static.Size = new System.Drawing.Size(71, 16);
            this.set_interface_mode_static.TabIndex = 18;
            this.set_interface_mode_static.TabStop = true;
            this.set_interface_mode_static.Text = "静态地址";
            this.set_interface_mode_static.UseVisualStyleBackColor = true;
            // 
            // set_interface_mode_auto
            // 
            this.set_interface_mode_auto.AutoSize = true;
            this.set_interface_mode_auto.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.set_interface_mode_auto.Location = new System.Drawing.Point(7, 57);
            this.set_interface_mode_auto.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_mode_auto.Name = "set_interface_mode_auto";
            this.set_interface_mode_auto.Size = new System.Drawing.Size(71, 16);
            this.set_interface_mode_auto.TabIndex = 17;
            this.set_interface_mode_auto.Text = "自动获取";
            this.set_interface_mode_auto.UseVisualStyleBackColor = true;
            this.set_interface_mode_auto.CheckedChanged += new System.EventHandler(this.set_interface_mode_auto_CheckedChanged);
            // 
            // label121
            // 
            this.label121.AutoSize = true;
            this.label121.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label121.Location = new System.Drawing.Point(4, 231);
            this.label121.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label121.Name = "label121";
            this.label121.Size = new System.Drawing.Size(83, 12);
            this.label121.TabIndex = 12;
            this.label121.Text = "备选DNS服务器";
            // 
            // label120
            // 
            this.label120.AutoSize = true;
            this.label120.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label120.Location = new System.Drawing.Point(4, 200);
            this.label120.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label120.Name = "label120";
            this.label120.Size = new System.Drawing.Size(83, 12);
            this.label120.TabIndex = 16;
            this.label120.Text = "首选DNS服务器";
            // 
            // label119
            // 
            this.label119.AutoSize = true;
            this.label119.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label119.Location = new System.Drawing.Point(4, 163);
            this.label119.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label119.Name = "label119";
            this.label119.Size = new System.Drawing.Size(53, 12);
            this.label119.TabIndex = 15;
            this.label119.Text = "默认网关";
            // 
            // label118
            // 
            this.label118.AutoSize = true;
            this.label118.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label118.Location = new System.Drawing.Point(4, 132);
            this.label118.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label118.Name = "label118";
            this.label118.Size = new System.Drawing.Size(53, 12);
            this.label118.TabIndex = 14;
            this.label118.Text = "子网掩码";
            // 
            // label117
            // 
            this.label117.AutoSize = true;
            this.label117.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label117.Location = new System.Drawing.Point(4, 95);
            this.label117.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label117.Name = "label117";
            this.label117.Size = new System.Drawing.Size(53, 12);
            this.label117.TabIndex = 13;
            this.label117.Text = "IPV4地址";
            // 
            // set_interface_name_list
            // 
            this.set_interface_name_list.FormattingEnabled = true;
            this.set_interface_name_list.Location = new System.Drawing.Point(61, 24);
            this.set_interface_name_list.Margin = new System.Windows.Forms.Padding(1);
            this.set_interface_name_list.Name = "set_interface_name_list";
            this.set_interface_name_list.Size = new System.Drawing.Size(259, 20);
            this.set_interface_name_list.TabIndex = 12;
            this.set_interface_name_list.SelectedIndexChanged += new System.EventHandler(this.set_interface_name_list_SelectedIndexChanged);
            // 
            // label116
            // 
            this.label116.AutoSize = true;
            this.label116.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label116.Location = new System.Drawing.Point(4, 27);
            this.label116.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label116.Name = "label116";
            this.label116.Size = new System.Drawing.Size(53, 12);
            this.label116.TabIndex = 12;
            this.label116.Text = "选择网卡";
            // 
            // groupBox44
            // 
            this.groupBox44.Controls.Add(this.copy_network_info_ip_address);
            this.groupBox44.Controls.Add(this.copy_network_info_mac_address);
            this.groupBox44.Controls.Add(this.network_info_mac_address);
            this.groupBox44.Controls.Add(this.label122);
            this.groupBox44.Controls.Add(this.network_open_ncap);
            this.groupBox44.Controls.Add(this.update_network_info);
            this.groupBox44.Controls.Add(this.network_info_show_normal);
            this.groupBox44.Controls.Add(this.Disable_NetworkCard);
            this.groupBox44.Controls.Add(this.Enable_NetworkCard);
            this.groupBox44.Controls.Add(this.network_info_dns2);
            this.groupBox44.Controls.Add(this.network_info_dns1);
            this.groupBox44.Controls.Add(this.network_info_gateway);
            this.groupBox44.Controls.Add(this.network_info_mask);
            this.groupBox44.Controls.Add(this.network_info_ipv4);
            this.groupBox44.Controls.Add(this.network_info_card_list);
            this.groupBox44.Controls.Add(this.label115);
            this.groupBox44.Controls.Add(this.label114);
            this.groupBox44.Controls.Add(this.label113);
            this.groupBox44.Controls.Add(this.label112);
            this.groupBox44.Controls.Add(this.label111);
            this.groupBox44.Controls.Add(this.label110);
            this.groupBox44.Location = new System.Drawing.Point(7, 4);
            this.groupBox44.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox44.Name = "groupBox44";
            this.groupBox44.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox44.Size = new System.Drawing.Size(333, 311);
            this.groupBox44.TabIndex = 0;
            this.groupBox44.TabStop = false;
            this.groupBox44.Text = "网卡信息概览";
            // 
            // copy_network_info_ip_address
            // 
            this.copy_network_info_ip_address.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.copy_network_info_ip_address.Location = new System.Drawing.Point(269, 56);
            this.copy_network_info_ip_address.Margin = new System.Windows.Forms.Padding(1);
            this.copy_network_info_ip_address.Name = "copy_network_info_ip_address";
            this.copy_network_info_ip_address.Size = new System.Drawing.Size(60, 22);
            this.copy_network_info_ip_address.TabIndex = 20;
            this.copy_network_info_ip_address.Text = "复制";
            this.copy_network_info_ip_address.UseVisualStyleBackColor = true;
            this.copy_network_info_ip_address.Click += new System.EventHandler(this.copy_network_info_ip_address_Click);
            // 
            // copy_network_info_mac_address
            // 
            this.copy_network_info_mac_address.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.copy_network_info_mac_address.Location = new System.Drawing.Point(269, 209);
            this.copy_network_info_mac_address.Margin = new System.Windows.Forms.Padding(1);
            this.copy_network_info_mac_address.Name = "copy_network_info_mac_address";
            this.copy_network_info_mac_address.Size = new System.Drawing.Size(60, 22);
            this.copy_network_info_mac_address.TabIndex = 19;
            this.copy_network_info_mac_address.Text = "复制";
            this.copy_network_info_mac_address.UseVisualStyleBackColor = true;
            this.copy_network_info_mac_address.Click += new System.EventHandler(this.copy_network_info_mac_address_Click);
            // 
            // network_info_mac_address
            // 
            this.network_info_mac_address.FormattingEnabled = true;
            this.network_info_mac_address.Location = new System.Drawing.Point(91, 209);
            this.network_info_mac_address.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_mac_address.Name = "network_info_mac_address";
            this.network_info_mac_address.Size = new System.Drawing.Size(175, 20);
            this.network_info_mac_address.TabIndex = 18;
            // 
            // label122
            // 
            this.label122.AutoSize = true;
            this.label122.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label122.Location = new System.Drawing.Point(4, 211);
            this.label122.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label122.Name = "label122";
            this.label122.Size = new System.Drawing.Size(77, 12);
            this.label122.TabIndex = 17;
            this.label122.Text = "网卡物理地址";
            // 
            // network_open_ncap
            // 
            this.network_open_ncap.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.network_open_ncap.Location = new System.Drawing.Point(197, 267);
            this.network_open_ncap.Margin = new System.Windows.Forms.Padding(1);
            this.network_open_ncap.Name = "network_open_ncap";
            this.network_open_ncap.Size = new System.Drawing.Size(132, 33);
            this.network_open_ncap.TabIndex = 16;
            this.network_open_ncap.Text = "打开系统网卡设置";
            this.network_open_ncap.UseVisualStyleBackColor = true;
            this.network_open_ncap.Click += new System.EventHandler(this.network_open_ncap_Click);
            // 
            // update_network_info
            // 
            this.update_network_info.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.update_network_info.Location = new System.Drawing.Point(97, 267);
            this.update_network_info.Margin = new System.Windows.Forms.Padding(1);
            this.update_network_info.Name = "update_network_info";
            this.update_network_info.Size = new System.Drawing.Size(95, 33);
            this.update_network_info.TabIndex = 15;
            this.update_network_info.Text = "刷新网卡信息";
            this.update_network_info.UseVisualStyleBackColor = true;
            this.update_network_info.Click += new System.EventHandler(this.update_network_info_Click);
            // 
            // network_info_show_normal
            // 
            this.network_info_show_normal.AutoSize = true;
            this.network_info_show_normal.Checked = true;
            this.network_info_show_normal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.network_info_show_normal.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.network_info_show_normal.Location = new System.Drawing.Point(7, 237);
            this.network_info_show_normal.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_show_normal.Name = "network_info_show_normal";
            this.network_info_show_normal.Size = new System.Drawing.Size(276, 16);
            this.network_info_show_normal.TabIndex = 14;
            this.network_info_show_normal.Text = "只列出状态正常的网卡（已启用且连接的网卡）";
            this.network_info_show_normal.UseVisualStyleBackColor = true;
            this.network_info_show_normal.CheckedChanged += new System.EventHandler(this.network_info_show_normal_CheckedChanged);
            // 
            // Disable_NetworkCard
            // 
            this.Disable_NetworkCard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Disable_NetworkCard.Location = new System.Drawing.Point(53, 267);
            this.Disable_NetworkCard.Margin = new System.Windows.Forms.Padding(1);
            this.Disable_NetworkCard.Name = "Disable_NetworkCard";
            this.Disable_NetworkCard.Size = new System.Drawing.Size(41, 33);
            this.Disable_NetworkCard.TabIndex = 13;
            this.Disable_NetworkCard.Text = "禁用网卡";
            this.Disable_NetworkCard.UseVisualStyleBackColor = true;
            this.Disable_NetworkCard.Click += new System.EventHandler(this.Disable_NetworkCard_Click);
            // 
            // Enable_NetworkCard
            // 
            this.Enable_NetworkCard.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.Enable_NetworkCard.Location = new System.Drawing.Point(7, 267);
            this.Enable_NetworkCard.Margin = new System.Windows.Forms.Padding(1);
            this.Enable_NetworkCard.Name = "Enable_NetworkCard";
            this.Enable_NetworkCard.Size = new System.Drawing.Size(41, 33);
            this.Enable_NetworkCard.TabIndex = 12;
            this.Enable_NetworkCard.Text = "启用网卡";
            this.Enable_NetworkCard.UseVisualStyleBackColor = true;
            this.Enable_NetworkCard.Click += new System.EventHandler(this.Enable_NetworkCard_Click);
            // 
            // network_info_dns2
            // 
            this.network_info_dns2.FormattingEnabled = true;
            this.network_info_dns2.Location = new System.Drawing.Point(91, 181);
            this.network_info_dns2.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_dns2.Name = "network_info_dns2";
            this.network_info_dns2.Size = new System.Drawing.Size(239, 20);
            this.network_info_dns2.TabIndex = 11;
            // 
            // network_info_dns1
            // 
            this.network_info_dns1.FormattingEnabled = true;
            this.network_info_dns1.Location = new System.Drawing.Point(91, 151);
            this.network_info_dns1.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_dns1.Name = "network_info_dns1";
            this.network_info_dns1.Size = new System.Drawing.Size(239, 20);
            this.network_info_dns1.TabIndex = 10;
            // 
            // network_info_gateway
            // 
            this.network_info_gateway.FormattingEnabled = true;
            this.network_info_gateway.Location = new System.Drawing.Point(61, 119);
            this.network_info_gateway.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_gateway.Name = "network_info_gateway";
            this.network_info_gateway.Size = new System.Drawing.Size(269, 20);
            this.network_info_gateway.TabIndex = 9;
            // 
            // network_info_mask
            // 
            this.network_info_mask.FormattingEnabled = true;
            this.network_info_mask.Location = new System.Drawing.Point(61, 87);
            this.network_info_mask.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_mask.Name = "network_info_mask";
            this.network_info_mask.Size = new System.Drawing.Size(269, 20);
            this.network_info_mask.TabIndex = 8;
            // 
            // network_info_ipv4
            // 
            this.network_info_ipv4.FormattingEnabled = true;
            this.network_info_ipv4.Location = new System.Drawing.Point(61, 56);
            this.network_info_ipv4.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_ipv4.Name = "network_info_ipv4";
            this.network_info_ipv4.Size = new System.Drawing.Size(205, 20);
            this.network_info_ipv4.TabIndex = 7;
            // 
            // network_info_card_list
            // 
            this.network_info_card_list.FormattingEnabled = true;
            this.network_info_card_list.Location = new System.Drawing.Point(61, 25);
            this.network_info_card_list.Margin = new System.Windows.Forms.Padding(1);
            this.network_info_card_list.Name = "network_info_card_list";
            this.network_info_card_list.Size = new System.Drawing.Size(269, 20);
            this.network_info_card_list.TabIndex = 6;
            this.network_info_card_list.SelectedIndexChanged += new System.EventHandler(this.network_info_card_list_SelectedIndexChanged);
            // 
            // label115
            // 
            this.label115.AutoSize = true;
            this.label115.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label115.Location = new System.Drawing.Point(4, 185);
            this.label115.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label115.Name = "label115";
            this.label115.Size = new System.Drawing.Size(83, 12);
            this.label115.TabIndex = 5;
            this.label115.Text = "备选DNS服务器";
            // 
            // label114
            // 
            this.label114.AutoSize = true;
            this.label114.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label114.Location = new System.Drawing.Point(4, 153);
            this.label114.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label114.Name = "label114";
            this.label114.Size = new System.Drawing.Size(83, 12);
            this.label114.TabIndex = 4;
            this.label114.Text = "首选DNS服务器";
            // 
            // label113
            // 
            this.label113.AutoSize = true;
            this.label113.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label113.Location = new System.Drawing.Point(4, 121);
            this.label113.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label113.Name = "label113";
            this.label113.Size = new System.Drawing.Size(53, 12);
            this.label113.TabIndex = 3;
            this.label113.Text = "默认网关";
            // 
            // label112
            // 
            this.label112.AutoSize = true;
            this.label112.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label112.Location = new System.Drawing.Point(4, 91);
            this.label112.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label112.Name = "label112";
            this.label112.Size = new System.Drawing.Size(53, 12);
            this.label112.TabIndex = 2;
            this.label112.Text = "子网掩码";
            // 
            // label111
            // 
            this.label111.AutoSize = true;
            this.label111.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label111.Location = new System.Drawing.Point(4, 59);
            this.label111.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label111.Name = "label111";
            this.label111.Size = new System.Drawing.Size(53, 12);
            this.label111.TabIndex = 1;
            this.label111.Text = "IPV4地址";
            // 
            // label110
            // 
            this.label110.AutoSize = true;
            this.label110.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label110.Location = new System.Drawing.Point(4, 28);
            this.label110.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label110.Name = "label110";
            this.label110.Size = new System.Drawing.Size(53, 12);
            this.label110.TabIndex = 0;
            this.label110.Text = "选择网卡";
            // 
            // TabPage_08
            // 
            this.TabPage_08.Controls.Add(this.pictureBox4);
            this.TabPage_08.Controls.Add(this.pictureBox3);
            this.TabPage_08.Controls.Add(this.pictureBox2);
            this.TabPage_08.Controls.Add(this.pictureBox9);
            this.TabPage_08.Controls.Add(this.groupBox4);
            this.TabPage_08.Controls.Add(this.groupBox3);
            this.TabPage_08.Location = new System.Drawing.Point(4, 76);
            this.TabPage_08.Margin = new System.Windows.Forms.Padding(1);
            this.TabPage_08.Name = "TabPage_08";
            this.TabPage_08.Size = new System.Drawing.Size(882, 659);
            this.TabPage_08.TabIndex = 7;
            this.TabPage_08.Text = "关于软件";
            this.TabPage_08.UseVisualStyleBackColor = true;
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::DocumentTools.Properties.Resources._6f258af2c19c9c3cdb97517573506c11ce48ff98;
            this.pictureBox4.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox4.Location = new System.Drawing.Point(5, 408);
            this.pictureBox4.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(221, 209);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 10;
            this.pictureBox4.TabStop = false;
            this.pictureBox4.Tag = "anchor_top_left_bottom";
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::DocumentTools.Properties.Resources._6f258af2c19c9c3cdb97517573506c11ce48ff98;
            this.pictureBox3.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox3.Location = new System.Drawing.Point(231, 408);
            this.pictureBox3.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(221, 209);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 9;
            this.pictureBox3.TabStop = false;
            this.pictureBox3.Tag = "anchor_top_left_bottom";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::DocumentTools.Properties.Resources._6f258af2c19c9c3cdb97517573506c11ce48ff98;
            this.pictureBox2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox2.Location = new System.Drawing.Point(456, 408);
            this.pictureBox2.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(221, 209);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Tag = "anchor_top_left_bottom";
            // 
            // pictureBox9
            // 
            this.pictureBox9.Image = global::DocumentTools.Properties.Resources._6f258af2c19c9c3cdb97517573506c11ce48ff98;
            this.pictureBox9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox9.Location = new System.Drawing.Point(681, 408);
            this.pictureBox9.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox9.Name = "pictureBox9";
            this.pictureBox9.Size = new System.Drawing.Size(197, 209);
            this.pictureBox9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox9.TabIndex = 7;
            this.pictureBox9.TabStop = false;
            this.pictureBox9.Tag = "anchor_full_fill";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.pictureBox6);
            this.groupBox4.Controls.Add(this.PlayCXKMusic);
            this.groupBox4.Controls.Add(this.pictureBox1);
            this.groupBox4.Controls.Add(this.linkLabel2);
            this.groupBox4.Controls.Add(this.label13);
            this.groupBox4.Controls.Add(this.label11);
            this.groupBox4.Controls.Add(this.label10);
            this.groupBox4.Controls.Add(this.label9);
            this.groupBox4.Location = new System.Drawing.Point(3, 127);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox4.Size = new System.Drawing.Size(875, 277);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Tag = "anchor_top_left_right";
            this.groupBox4.Text = "软件说明";
            // 
            // pictureBox6
            // 
            this.pictureBox6.Image = global::DocumentTools.Properties.Resources._78383f3dgy1hkcbcvjkxgj20rz0rztc2;
            this.pictureBox6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox6.Location = new System.Drawing.Point(467, 87);
            this.pictureBox6.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(136, 179);
            this.pictureBox6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox6.TabIndex = 0;
            this.pictureBox6.TabStop = false;
            this.pictureBox6.Tag = "anchor_right_top";
            // 
            // PlayCXKMusic
            // 
            this.PlayCXKMusic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.PlayCXKMusic.Location = new System.Drawing.Point(607, 45);
            this.PlayCXKMusic.Margin = new System.Windows.Forms.Padding(1);
            this.PlayCXKMusic.Name = "PlayCXKMusic";
            this.PlayCXKMusic.Size = new System.Drawing.Size(261, 37);
            this.PlayCXKMusic.TabIndex = 8;
            this.PlayCXKMusic.Tag = "anchor_right_top";
            this.PlayCXKMusic.Text = "点一下";
            this.PlayCXKMusic.UseVisualStyleBackColor = true;
            this.PlayCXKMusic.Click += new System.EventHandler(this.PlayCXKMusic_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox1.Location = new System.Drawing.Point(607, 87);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(261, 179);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Tag = "anchor_right_top";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.linkLabel2.Location = new System.Drawing.Point(177, 133);
            this.linkLabel2.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(185, 12);
            this.linkLabel2.TabIndex = 5;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "点击跳转更新地址[吾爱破解原帖]";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel2_LinkClicked);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label13.Location = new System.Drawing.Point(0, 57);
            this.label13.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(131, 12);
            this.label13.TabIndex = 4;
            this.label13.Text = "· 完全离线，无需联网";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label11.Location = new System.Drawing.Point(0, 133);
            this.label11.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(167, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "· 完全免费，但禁止售卖盈利";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label10.Location = new System.Drawing.Point(0, 93);
            this.label10.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(167, 12);
            this.label10.TabIndex = 1;
            this.label10.Text = "· 不需要本机安装Office软件";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.ForeColor = System.Drawing.Color.Tomato;
            this.label9.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label9.Location = new System.Drawing.Point(0, 23);
            this.label9.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(497, 12);
            this.label9.TabIndex = 0;
            this.label9.Text = "· 支持的操作系统：Windows 7 、Windows10、Windows11或更高（附带.NetFrameWork 4.8）";
            this.label9.DoubleClick += new System.EventHandler(this.Label9_DoubleClick);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.DisableAdminBehavior);
            this.groupBox3.Controls.Add(this.pictureBox5);
            this.groupBox3.Controls.Add(this.DontateMe);
            this.groupBox3.Controls.Add(this.UseLibList);
            this.groupBox3.Controls.Add(this.DownloadFrameWorkLabel);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Location = new System.Drawing.Point(3, 1);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(1);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(1);
            this.groupBox3.Size = new System.Drawing.Size(879, 121);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Tag = "anchor_top_left_right";
            this.groupBox3.Text = "开发信息";
            // 
            // DisableAdminBehavior
            // 
            this.DisableAdminBehavior.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DisableAdminBehavior.Location = new System.Drawing.Point(711, 45);
            this.DisableAdminBehavior.Margin = new System.Windows.Forms.Padding(1);
            this.DisableAdminBehavior.Name = "DisableAdminBehavior";
            this.DisableAdminBehavior.Size = new System.Drawing.Size(165, 35);
            this.DisableAdminBehavior.TabIndex = 11;
            this.DisableAdminBehavior.Tag = "anchor_right_top";
            this.DisableAdminBehavior.Text = "禁用管理员批准模式";
            this.MainToolsTip.SetToolTip(this.DisableAdminBehavior, "本功能详细解释可查看原帖介绍");
            this.DisableAdminBehavior.UseVisualStyleBackColor = true;
            this.DisableAdminBehavior.Click += new System.EventHandler(this.DisableAdminBehavior_Click);
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = global::DocumentTools.Properties.Resources._78383f3dgy1hkcbcyxmgcj20f60f6goy;
            this.pictureBox5.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.pictureBox5.Location = new System.Drawing.Point(591, 8);
            this.pictureBox5.Margin = new System.Windows.Forms.Padding(1);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(113, 108);
            this.pictureBox5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox5.TabIndex = 1;
            this.pictureBox5.TabStop = false;
            this.pictureBox5.Tag = "anchor_right_top";
            // 
            // DontateMe
            // 
            this.DontateMe.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DontateMe.Location = new System.Drawing.Point(711, 81);
            this.DontateMe.Margin = new System.Windows.Forms.Padding(1);
            this.DontateMe.Name = "DontateMe";
            this.DontateMe.Size = new System.Drawing.Size(165, 35);
            this.DontateMe.TabIndex = 10;
            this.DontateMe.Tag = "anchor_right_top";
            this.DontateMe.Text = "点我给作者捐助";
            this.DontateMe.UseVisualStyleBackColor = true;
            this.DontateMe.Click += new System.EventHandler(this.DontateMe_Click);
            // 
            // UseLibList
            // 
            this.UseLibList.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.UseLibList.Location = new System.Drawing.Point(711, 9);
            this.UseLibList.Margin = new System.Windows.Forms.Padding(1);
            this.UseLibList.Name = "UseLibList";
            this.UseLibList.Size = new System.Drawing.Size(165, 35);
            this.UseLibList.TabIndex = 9;
            this.UseLibList.Tag = "anchor_right_top";
            this.UseLibList.Text = "点击查看引用库列表";
            this.UseLibList.UseVisualStyleBackColor = true;
            this.UseLibList.Click += new System.EventHandler(this.UseLibList_Click);
            // 
            // DownloadFrameWorkLabel
            // 
            this.DownloadFrameWorkLabel.AutoSize = true;
            this.DownloadFrameWorkLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.DownloadFrameWorkLabel.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.DownloadFrameWorkLabel.Location = new System.Drawing.Point(175, 81);
            this.DownloadFrameWorkLabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.DownloadFrameWorkLabel.Name = "DownloadFrameWorkLabel";
            this.DownloadFrameWorkLabel.Size = new System.Drawing.Size(305, 12);
            this.DownloadFrameWorkLabel.TabIndex = 3;
            this.DownloadFrameWorkLabel.TabStop = true;
            this.DownloadFrameWorkLabel.Text = "点击下载.NetFrameWork 4.8 框架（建议Win7用户下载）";
            this.MainToolsTip.SetToolTip(this.DownloadFrameWorkLabel, "Windows7用户可能安装不成功，请查看原帖安装方法");
            this.DownloadFrameWorkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.DownloadFrameWorkLabel_LinkClicked);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label8.Location = new System.Drawing.Point(4, 81);
            this.label8.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(167, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "开发框架：.NetFrameWork 4.8";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label7.Location = new System.Drawing.Point(4, 52);
            this.label7.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(179, 12);
            this.label7.TabIndex = 1;
            this.label7.Text = "开发工具：Visual Studio社区版";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.label6.Location = new System.Drawing.Point(4, 23);
            this.label6.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 0;
            this.label6.Text = "开发语言：C#";
            // 
            // ListViewRightMenu
            // 
            this.ListViewRightMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.ListViewRightMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.列表操作ToolStripMenuItem,
            this.文件操作ToolStripMenuItem,
            this.获取PDF文件版本信息ToolStripMenuItem});
            this.ListViewRightMenu.Name = "contextMenuStrip1";
            this.ListViewRightMenu.Size = new System.Drawing.Size(157, 94);
            // 
            // 列表操作ToolStripMenuItem
            // 
            this.列表操作ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.删除本项ToolStripMenuItem,
            this.清空列表ToolStripMenuItem});
            this.列表操作ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("列表操作ToolStripMenuItem.Image")));
            this.列表操作ToolStripMenuItem.Name = "列表操作ToolStripMenuItem";
            this.列表操作ToolStripMenuItem.Size = new System.Drawing.Size(156, 30);
            this.列表操作ToolStripMenuItem.Text = "列表操作";
            // 
            // 删除本项ToolStripMenuItem
            // 
            this.删除本项ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.Remove;
            this.删除本项ToolStripMenuItem.Name = "删除本项ToolStripMenuItem";
            this.删除本项ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.删除本项ToolStripMenuItem.Text = "删除本项";
            this.删除本项ToolStripMenuItem.Click += new System.EventHandler(this.删除本项ToolStripMenuItem_Click);
            // 
            // 清空列表ToolStripMenuItem
            // 
            this.清空列表ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.Clear;
            this.清空列表ToolStripMenuItem.Name = "清空列表ToolStripMenuItem";
            this.清空列表ToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.清空列表ToolStripMenuItem.Text = "清空列表";
            this.清空列表ToolStripMenuItem.Click += new System.EventHandler(this.清空列表ToolStripMenuItem_Click);
            // 
            // 文件操作ToolStripMenuItem
            // 
            this.文件操作ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开源文件目录ToolStripMenuItem,
            this.打开源文件ToolStripMenuItem,
            this.打开转换后目录ToolStripMenuItem,
            this.打开转换后文件ToolStripMenuItem});
            this.文件操作ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("文件操作ToolStripMenuItem.Image")));
            this.文件操作ToolStripMenuItem.Name = "文件操作ToolStripMenuItem";
            this.文件操作ToolStripMenuItem.Size = new System.Drawing.Size(156, 30);
            this.文件操作ToolStripMenuItem.Text = "文件操作";
            // 
            // 打开源文件目录ToolStripMenuItem
            // 
            this.打开源文件目录ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("打开源文件目录ToolStripMenuItem.Image")));
            this.打开源文件目录ToolStripMenuItem.Name = "打开源文件目录ToolStripMenuItem";
            this.打开源文件目录ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.打开源文件目录ToolStripMenuItem.Text = "打开源文件目录";
            this.打开源文件目录ToolStripMenuItem.Click += new System.EventHandler(this.打开源文件目录ToolStripMenuItem_Click);
            // 
            // 打开源文件ToolStripMenuItem
            // 
            this.打开源文件ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("打开源文件ToolStripMenuItem.Image")));
            this.打开源文件ToolStripMenuItem.Name = "打开源文件ToolStripMenuItem";
            this.打开源文件ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.打开源文件ToolStripMenuItem.Text = "打开源文件";
            this.打开源文件ToolStripMenuItem.Click += new System.EventHandler(this.打开源文件ToolStripMenuItem_Click);
            // 
            // 打开转换后目录ToolStripMenuItem
            // 
            this.打开转换后目录ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("打开转换后目录ToolStripMenuItem.Image")));
            this.打开转换后目录ToolStripMenuItem.Name = "打开转换后目录ToolStripMenuItem";
            this.打开转换后目录ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.打开转换后目录ToolStripMenuItem.Text = "打开转换后目录";
            this.打开转换后目录ToolStripMenuItem.Click += new System.EventHandler(this.打开转换后目录ToolStripMenuItem_Click);
            // 
            // 打开转换后文件ToolStripMenuItem
            // 
            this.打开转换后文件ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("打开转换后文件ToolStripMenuItem.Image")));
            this.打开转换后文件ToolStripMenuItem.Name = "打开转换后文件ToolStripMenuItem";
            this.打开转换后文件ToolStripMenuItem.Size = new System.Drawing.Size(160, 22);
            this.打开转换后文件ToolStripMenuItem.Text = "打开转换后文件";
            this.打开转换后文件ToolStripMenuItem.Click += new System.EventHandler(this.打开转换后文件ToolStripMenuItem_Click);
            // 
            // 获取PDF文件版本信息ToolStripMenuItem
            // 
            this.获取PDF文件版本信息ToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("获取PDF文件版本信息ToolStripMenuItem.Image")));
            this.获取PDF文件版本信息ToolStripMenuItem.Name = "获取PDF文件版本信息ToolStripMenuItem";
            this.获取PDF文件版本信息ToolStripMenuItem.Size = new System.Drawing.Size(156, 30);
            this.获取PDF文件版本信息ToolStripMenuItem.Text = "获取文件信息";
            this.获取PDF文件版本信息ToolStripMenuItem.Click += new System.EventHandler(this.获取PDF文件版本信息ToolStripMenuItem_Click);
            // 
            // MemoryTimer
            // 
            this.MemoryTimer.Enabled = true;
            this.MemoryTimer.Interval = 1000;
            this.MemoryTimer.Tick += new System.EventHandler(this.MemoryTimer_Tick);
            // 
            // TreeViewContentMenu
            // 
            this.TreeViewContentMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.TreeViewContentMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.展开折叠本节点全部ToolStripMenuItem,
            this.折叠本节点全部内容ToolStripMenuItem,
            this.展开折叠全部节点ToolStripMenuItem,
            this.折叠全部节点ToolStripMenuItem,
            this.打开文件ToolStripMenuItem});
            this.TreeViewContentMenu.Name = "TreeViewContentMenu";
            this.TreeViewContentMenu.Size = new System.Drawing.Size(185, 114);
            // 
            // 展开折叠本节点全部ToolStripMenuItem
            // 
            this.展开折叠本节点全部ToolStripMenuItem.Name = "展开折叠本节点全部ToolStripMenuItem";
            this.展开折叠本节点全部ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.展开折叠本节点全部ToolStripMenuItem.Text = "展开本节点全部内容";
            this.展开折叠本节点全部ToolStripMenuItem.Click += new System.EventHandler(this.展开折叠本节点全部ToolStripMenuItem_Click);
            // 
            // 折叠本节点全部内容ToolStripMenuItem
            // 
            this.折叠本节点全部内容ToolStripMenuItem.Name = "折叠本节点全部内容ToolStripMenuItem";
            this.折叠本节点全部内容ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.折叠本节点全部内容ToolStripMenuItem.Text = "折叠本节点全部内容";
            this.折叠本节点全部内容ToolStripMenuItem.Click += new System.EventHandler(this.折叠本节点全部内容ToolStripMenuItem_Click);
            // 
            // 展开折叠全部节点ToolStripMenuItem
            // 
            this.展开折叠全部节点ToolStripMenuItem.Name = "展开折叠全部节点ToolStripMenuItem";
            this.展开折叠全部节点ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.展开折叠全部节点ToolStripMenuItem.Text = "展开全部节点";
            this.展开折叠全部节点ToolStripMenuItem.Click += new System.EventHandler(this.展开折叠全部节点ToolStripMenuItem_Click);
            // 
            // 折叠全部节点ToolStripMenuItem
            // 
            this.折叠全部节点ToolStripMenuItem.Name = "折叠全部节点ToolStripMenuItem";
            this.折叠全部节点ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.折叠全部节点ToolStripMenuItem.Text = "折叠全部节点";
            this.折叠全部节点ToolStripMenuItem.Click += new System.EventHandler(this.折叠全部节点ToolStripMenuItem_Click);
            // 
            // 打开文件ToolStripMenuItem
            // 
            this.打开文件ToolStripMenuItem.Name = "打开文件ToolStripMenuItem";
            this.打开文件ToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.打开文件ToolStripMenuItem.Text = "打开文件";
            this.打开文件ToolStripMenuItem.Click += new System.EventHandler(this.打开文件ToolStripMenuItem_Click);
            // 
            // MainFormError
            // 
            this.MainFormError.ContainerControl = this;
            // 
            // menuStrip1
            // 
            this.menuStrip1.AutoSize = false;
            this.menuStrip1.BackColor = System.Drawing.Color.Moccasin;
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.liblist,
            this.软件设置ToolStripMenuItem,
            this.程序更新ToolStripMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.menuStrip1.Size = new System.Drawing.Size(890, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // liblist
            // 
            this.liblist.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wORD功能ToolStripMenuItem,
            this.eXCEL功能ToolStripMenuItem,
            this.pPT功能ToolStripMenuItem,
            this.pDF功能ToolStripMenuItem,
            this.ePUB功能ToolStripMenuItem,
            this.hTML功能ToolStripMenuItem,
            this.拆分合并功能ToolStripMenuItem,
            this.压缩功能ToolStripMenuItem,
            this.图片功能ToolStripMenuItem,
            this.sYX功能ToolStripMenuItem,
            this.其他功能ToolStripMenuItem,
            this.系统工具ToolStripMenuItem,
            this.关于软件ToolStripMenuItem});
            this.liblist.Image = global::DocumentTools.Properties.Resources.功能;
            this.liblist.Name = "liblist";
            this.liblist.Size = new System.Drawing.Size(92, 24);
            this.liblist.Text = "功能分类";
            // 
            // wORD功能ToolStripMenuItem
            // 
            this.wORD功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wORD转PDFToolStripMenuItem,
            this.wORD转EXCELToolStripMenuItem,
            this.wORDToolStripMenuItem,
            this.wORD内容批量替换ToolStripMenuItem});
            this.wORD功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.word;
            this.wORD功能ToolStripMenuItem.Name = "wORD功能ToolStripMenuItem";
            this.wORD功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.wORD功能ToolStripMenuItem.Text = "WORD功能";
            // 
            // wORD转PDFToolStripMenuItem
            // 
            this.wORD转PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.wORD转PDFToolStripMenuItem.Name = "wORD转PDFToolStripMenuItem";
            this.wORD转PDFToolStripMenuItem.Size = new System.Drawing.Size(195, 30);
            this.wORD转PDFToolStripMenuItem.Text = "WORD转PDF";
            this.wORD转PDFToolStripMenuItem.Click += new System.EventHandler(this.wORD转PDFToolStripMenuItem_Click);
            // 
            // wORD转EXCELToolStripMenuItem
            // 
            this.wORD转EXCELToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.excel;
            this.wORD转EXCELToolStripMenuItem.Name = "wORD转EXCELToolStripMenuItem";
            this.wORD转EXCELToolStripMenuItem.Size = new System.Drawing.Size(195, 30);
            this.wORD转EXCELToolStripMenuItem.Text = "WORD转EXCEL";
            this.wORD转EXCELToolStripMenuItem.Click += new System.EventHandler(this.wORD转EXCELToolStripMenuItem_Click);
            // 
            // wORDToolStripMenuItem
            // 
            this.wORDToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.Epub_Local_default;
            this.wORDToolStripMenuItem.Name = "wORDToolStripMenuItem";
            this.wORDToolStripMenuItem.Size = new System.Drawing.Size(195, 30);
            this.wORDToolStripMenuItem.Text = "WORD转EPUB";
            this.wORDToolStripMenuItem.Click += new System.EventHandler(this.wORDToolStripMenuItem_Click);
            // 
            // wORD内容批量替换ToolStripMenuItem
            // 
            this.wORD内容批量替换ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.查找替换_replace_线;
            this.wORD内容批量替换ToolStripMenuItem.Name = "wORD内容批量替换ToolStripMenuItem";
            this.wORD内容批量替换ToolStripMenuItem.Size = new System.Drawing.Size(195, 30);
            this.wORD内容批量替换ToolStripMenuItem.Text = "WORD内容批量替换";
            this.wORD内容批量替换ToolStripMenuItem.Click += new System.EventHandler(this.wORD内容批量替换ToolStripMenuItem_Click);
            // 
            // eXCEL功能ToolStripMenuItem
            // 
            this.eXCEL功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eXCEL转PDFToolStripMenuItem,
            this.eXCEL转WORDToolStripMenuItem,
            this.eXCEL内容批量替换ToolStripMenuItem});
            this.eXCEL功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.excel;
            this.eXCEL功能ToolStripMenuItem.Name = "eXCEL功能ToolStripMenuItem";
            this.eXCEL功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.eXCEL功能ToolStripMenuItem.Text = "EXCEL功能";
            // 
            // eXCEL转PDFToolStripMenuItem
            // 
            this.eXCEL转PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.eXCEL转PDFToolStripMenuItem.Name = "eXCEL转PDFToolStripMenuItem";
            this.eXCEL转PDFToolStripMenuItem.Size = new System.Drawing.Size(192, 30);
            this.eXCEL转PDFToolStripMenuItem.Text = "EXCEL转PDF";
            this.eXCEL转PDFToolStripMenuItem.Click += new System.EventHandler(this.eXCEL转PDFToolStripMenuItem_Click);
            // 
            // eXCEL转WORDToolStripMenuItem
            // 
            this.eXCEL转WORDToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.word;
            this.eXCEL转WORDToolStripMenuItem.Name = "eXCEL转WORDToolStripMenuItem";
            this.eXCEL转WORDToolStripMenuItem.Size = new System.Drawing.Size(192, 30);
            this.eXCEL转WORDToolStripMenuItem.Text = "EXCEL转WORD";
            this.eXCEL转WORDToolStripMenuItem.Click += new System.EventHandler(this.eXCEL转WORDToolStripMenuItem_Click);
            // 
            // eXCEL内容批量替换ToolStripMenuItem
            // 
            this.eXCEL内容批量替换ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.查找替换_replace_线;
            this.eXCEL内容批量替换ToolStripMenuItem.Name = "eXCEL内容批量替换ToolStripMenuItem";
            this.eXCEL内容批量替换ToolStripMenuItem.Size = new System.Drawing.Size(192, 30);
            this.eXCEL内容批量替换ToolStripMenuItem.Text = "EXCEL内容批量替换";
            this.eXCEL内容批量替换ToolStripMenuItem.Click += new System.EventHandler(this.eXCEL内容批量替换ToolStripMenuItem_Click);
            // 
            // pPT功能ToolStripMenuItem
            // 
            this.pPT功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pPT转PDFToolStripMenuItem});
            this.pPT功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.ppt;
            this.pPT功能ToolStripMenuItem.Name = "pPT功能ToolStripMenuItem";
            this.pPT功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.pPT功能ToolStripMenuItem.Text = "PPT功能";
            // 
            // pPT转PDFToolStripMenuItem
            // 
            this.pPT转PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.pPT转PDFToolStripMenuItem.Name = "pPT转PDFToolStripMenuItem";
            this.pPT转PDFToolStripMenuItem.Size = new System.Drawing.Size(139, 30);
            this.pPT转PDFToolStripMenuItem.Text = "PPT转PDF";
            this.pPT转PDFToolStripMenuItem.Click += new System.EventHandler(this.pPT转PDFToolStripMenuItem_Click);
            // 
            // pDF功能ToolStripMenuItem
            // 
            this.pDF功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pDF转EXCELToolStripMenuItem,
            this.pDFToolStripMenuItem,
            this.pDF转PPTToolStripMenuItem,
            this.添加PDF水印ToolStripMenuItem,
            this.移除PDF水印ToolStripMenuItem,
            this.pDF版本转换ToolStripMenuItem});
            this.pDF功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.pDF功能ToolStripMenuItem.Name = "pDF功能ToolStripMenuItem";
            this.pDF功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.pDF功能ToolStripMenuItem.Text = "PDF功能";
            // 
            // pDF转EXCELToolStripMenuItem
            // 
            this.pDF转EXCELToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.word;
            this.pDF转EXCELToolStripMenuItem.Name = "pDF转EXCELToolStripMenuItem";
            this.pDF转EXCELToolStripMenuItem.Size = new System.Drawing.Size(157, 30);
            this.pDF转EXCELToolStripMenuItem.Text = "PDF转WORD";
            this.pDF转EXCELToolStripMenuItem.Click += new System.EventHandler(this.pDF转EXCELToolStripMenuItem_Click);
            // 
            // pDFToolStripMenuItem
            // 
            this.pDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.excel;
            this.pDFToolStripMenuItem.Name = "pDFToolStripMenuItem";
            this.pDFToolStripMenuItem.Size = new System.Drawing.Size(157, 30);
            this.pDFToolStripMenuItem.Text = "PDF转EXCEL";
            this.pDFToolStripMenuItem.Click += new System.EventHandler(this.pDFToolStripMenuItem_Click);
            // 
            // pDF转PPTToolStripMenuItem
            // 
            this.pDF转PPTToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.ppt;
            this.pDF转PPTToolStripMenuItem.Name = "pDF转PPTToolStripMenuItem";
            this.pDF转PPTToolStripMenuItem.Size = new System.Drawing.Size(157, 30);
            this.pDF转PPTToolStripMenuItem.Text = "PDF转PPT";
            this.pDF转PPTToolStripMenuItem.Click += new System.EventHandler(this.pDF转PPTToolStripMenuItem_Click);
            // 
            // 添加PDF水印ToolStripMenuItem
            // 
            this.添加PDF水印ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.水印;
            this.添加PDF水印ToolStripMenuItem.Name = "添加PDF水印ToolStripMenuItem";
            this.添加PDF水印ToolStripMenuItem.Size = new System.Drawing.Size(157, 30);
            this.添加PDF水印ToolStripMenuItem.Text = "添加PDF水印";
            this.添加PDF水印ToolStripMenuItem.Click += new System.EventHandler(this.添加PDF水印ToolStripMenuItem_Click);
            // 
            // 移除PDF水印ToolStripMenuItem
            // 
            this.移除PDF水印ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.去水印;
            this.移除PDF水印ToolStripMenuItem.Name = "移除PDF水印ToolStripMenuItem";
            this.移除PDF水印ToolStripMenuItem.Size = new System.Drawing.Size(157, 30);
            this.移除PDF水印ToolStripMenuItem.Text = "移除PDF水印";
            this.移除PDF水印ToolStripMenuItem.Click += new System.EventHandler(this.移除PDF水印ToolStripMenuItem_Click);
            // 
            // pDF版本转换ToolStripMenuItem
            // 
            this.pDF版本转换ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.pDF版本转换ToolStripMenuItem.Name = "pDF版本转换ToolStripMenuItem";
            this.pDF版本转换ToolStripMenuItem.Size = new System.Drawing.Size(157, 30);
            this.pDF版本转换ToolStripMenuItem.Text = "PDF版本转换";
            this.pDF版本转换ToolStripMenuItem.Click += new System.EventHandler(this.pDF版本转换ToolStripMenuItem_Click);
            // 
            // ePUB功能ToolStripMenuItem
            // 
            this.ePUB功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ePUB转WORDToolStripMenuItem,
            this.ePUB转PDFToolStripMenuItem});
            this.ePUB功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.Epub_Local_default;
            this.ePUB功能ToolStripMenuItem.Name = "ePUB功能ToolStripMenuItem";
            this.ePUB功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.ePUB功能ToolStripMenuItem.Text = "EPUB功能";
            // 
            // ePUB转WORDToolStripMenuItem
            // 
            this.ePUB转WORDToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.word;
            this.ePUB转WORDToolStripMenuItem.Name = "ePUB转WORDToolStripMenuItem";
            this.ePUB转WORDToolStripMenuItem.Size = new System.Drawing.Size(166, 30);
            this.ePUB转WORDToolStripMenuItem.Text = "EPUB转WORD";
            this.ePUB转WORDToolStripMenuItem.Click += new System.EventHandler(this.ePUB转WORDToolStripMenuItem_Click);
            // 
            // ePUB转PDFToolStripMenuItem
            // 
            this.ePUB转PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.ePUB转PDFToolStripMenuItem.Name = "ePUB转PDFToolStripMenuItem";
            this.ePUB转PDFToolStripMenuItem.Size = new System.Drawing.Size(166, 30);
            this.ePUB转PDFToolStripMenuItem.Text = "EPUB转PDF";
            this.ePUB转PDFToolStripMenuItem.Click += new System.EventHandler(this.ePUB转PDFToolStripMenuItem_Click);
            // 
            // hTML功能ToolStripMenuItem
            // 
            this.hTML功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hTML转PDFToolStripMenuItem});
            this.hTML功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.html;
            this.hTML功能ToolStripMenuItem.Name = "hTML功能ToolStripMenuItem";
            this.hTML功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.hTML功能ToolStripMenuItem.Text = "HTML功能";
            // 
            // hTML转PDFToolStripMenuItem
            // 
            this.hTML转PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.hTML转PDFToolStripMenuItem.Name = "hTML转PDFToolStripMenuItem";
            this.hTML转PDFToolStripMenuItem.Size = new System.Drawing.Size(152, 30);
            this.hTML转PDFToolStripMenuItem.Text = "HTML转PDF";
            this.hTML转PDFToolStripMenuItem.Click += new System.EventHandler(this.hTML转PDFToolStripMenuItem_Click);
            // 
            // 拆分合并功能ToolStripMenuItem
            // 
            this.拆分合并功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.拆分WORDToolStripMenuItem,
            this.拆分PDFToolStripMenuItem,
            this.合并WORDToolStripMenuItem,
            this.合并PDFToolStripMenuItem,
            this.拆分PPTToolStripMenuItem});
            this.拆分合并功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.拆分;
            this.拆分合并功能ToolStripMenuItem.Name = "拆分合并功能ToolStripMenuItem";
            this.拆分合并功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.拆分合并功能ToolStripMenuItem.Text = "拆分/合并功能";
            // 
            // 拆分WORDToolStripMenuItem
            // 
            this.拆分WORDToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.word;
            this.拆分WORDToolStripMenuItem.Name = "拆分WORDToolStripMenuItem";
            this.拆分WORDToolStripMenuItem.Size = new System.Drawing.Size(147, 30);
            this.拆分WORDToolStripMenuItem.Text = "拆分WORD";
            this.拆分WORDToolStripMenuItem.Click += new System.EventHandler(this.拆分WORDToolStripMenuItem_Click);
            // 
            // 拆分PDFToolStripMenuItem
            // 
            this.拆分PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.拆分PDFToolStripMenuItem.Name = "拆分PDFToolStripMenuItem";
            this.拆分PDFToolStripMenuItem.Size = new System.Drawing.Size(147, 30);
            this.拆分PDFToolStripMenuItem.Text = "拆分PDF";
            this.拆分PDFToolStripMenuItem.Click += new System.EventHandler(this.拆分PDFToolStripMenuItem_Click);
            // 
            // 合并WORDToolStripMenuItem
            // 
            this.合并WORDToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.word;
            this.合并WORDToolStripMenuItem.Name = "合并WORDToolStripMenuItem";
            this.合并WORDToolStripMenuItem.Size = new System.Drawing.Size(147, 30);
            this.合并WORDToolStripMenuItem.Text = "合并WORD";
            this.合并WORDToolStripMenuItem.Click += new System.EventHandler(this.合并WORDToolStripMenuItem_Click);
            // 
            // 合并PDFToolStripMenuItem
            // 
            this.合并PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.合并PDFToolStripMenuItem.Name = "合并PDFToolStripMenuItem";
            this.合并PDFToolStripMenuItem.Size = new System.Drawing.Size(147, 30);
            this.合并PDFToolStripMenuItem.Text = "合并PDF";
            this.合并PDFToolStripMenuItem.Click += new System.EventHandler(this.合并PDFToolStripMenuItem_Click);
            // 
            // 拆分PPTToolStripMenuItem
            // 
            this.拆分PPTToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.ppt;
            this.拆分PPTToolStripMenuItem.Name = "拆分PPTToolStripMenuItem";
            this.拆分PPTToolStripMenuItem.Size = new System.Drawing.Size(147, 30);
            this.拆分PPTToolStripMenuItem.Text = "拆分PPT";
            this.拆分PPTToolStripMenuItem.Click += new System.EventHandler(this.拆分PPTToolStripMenuItem_Click);
            // 
            // 压缩功能ToolStripMenuItem
            // 
            this.压缩功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.压缩PDFToolStripMenuItem,
            this.压缩图片ToolStripMenuItem});
            this.压缩功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.压缩;
            this.压缩功能ToolStripMenuItem.Name = "压缩功能ToolStripMenuItem";
            this.压缩功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.压缩功能ToolStripMenuItem.Text = "压缩功能";
            // 
            // 压缩PDFToolStripMenuItem
            // 
            this.压缩PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.压缩PDFToolStripMenuItem.Name = "压缩PDFToolStripMenuItem";
            this.压缩PDFToolStripMenuItem.Size = new System.Drawing.Size(132, 30);
            this.压缩PDFToolStripMenuItem.Text = "压缩PDF";
            this.压缩PDFToolStripMenuItem.Click += new System.EventHandler(this.压缩PDFToolStripMenuItem_Click);
            // 
            // 压缩图片ToolStripMenuItem
            // 
            this.压缩图片ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.图片;
            this.压缩图片ToolStripMenuItem.Name = "压缩图片ToolStripMenuItem";
            this.压缩图片ToolStripMenuItem.Size = new System.Drawing.Size(132, 30);
            this.压缩图片ToolStripMenuItem.Text = "压缩图片";
            this.压缩图片ToolStripMenuItem.Click += new System.EventHandler(this.压缩图片ToolStripMenuItem_Click);
            // 
            // 图片功能ToolStripMenuItem
            // 
            this.图片功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.wORDEXCELPDF转图片ToolStripMenuItem,
            this.图片转PDFToolStripMenuItem,
            this.wEBP图片格式转换ToolStripMenuItem});
            this.图片功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.图片;
            this.图片功能ToolStripMenuItem.Name = "图片功能ToolStripMenuItem";
            this.图片功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.图片功能ToolStripMenuItem.Text = "图片功能";
            // 
            // wORDEXCELPDF转图片ToolStripMenuItem
            // 
            this.wORDEXCELPDF转图片ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.图片;
            this.wORDEXCELPDF转图片ToolStripMenuItem.Name = "wORDEXCELPDF转图片ToolStripMenuItem";
            this.wORDEXCELPDF转图片ToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.wORDEXCELPDF转图片ToolStripMenuItem.Text = "WORD/PDF/EXCEL转图片";
            this.wORDEXCELPDF转图片ToolStripMenuItem.Click += new System.EventHandler(this.wORDEXCELPDF转图片ToolStripMenuItem_Click);
            // 
            // 图片转PDFToolStripMenuItem
            // 
            this.图片转PDFToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.pdf;
            this.图片转PDFToolStripMenuItem.Name = "图片转PDFToolStripMenuItem";
            this.图片转PDFToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.图片转PDFToolStripMenuItem.Text = "图片转PDF";
            this.图片转PDFToolStripMenuItem.Click += new System.EventHandler(this.图片转PDFToolStripMenuItem_Click);
            // 
            // wEBP图片格式转换ToolStripMenuItem
            // 
            this.wEBP图片格式转换ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.图片;
            this.wEBP图片格式转换ToolStripMenuItem.Name = "wEBP图片格式转换ToolStripMenuItem";
            this.wEBP图片格式转换ToolStripMenuItem.Size = new System.Drawing.Size(227, 30);
            this.wEBP图片格式转换ToolStripMenuItem.Text = "WEBP图片格式转换";
            this.wEBP图片格式转换ToolStripMenuItem.Click += new System.EventHandler(this.wEBP图片格式转换ToolStripMenuItem_Click);
            // 
            // sYX功能ToolStripMenuItem
            // 
            this.sYX功能ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.姓名横转竖列ToolStripMenuItem,
            this.特殊二维码校验ToolStripMenuItem,
            this.twain扫描仪调试ToolStripMenuItem,
            this.mSS3模板修改ToolStripMenuItem,
            this.简易二维码生成ToolStripMenuItem,
            this.随机身份证号码生成ToolStripMenuItem,
            this.文件副本生成ToolStripMenuItem,
            this.网卡设置ToolStripMenuItem,
            this.选举报告单样式生成ToolStripMenuItem,
            this.单人表生成ToolStripMenuItem,
            this.考评报表修改ToolStripMenuItem});
            this.sYX功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.云;
            this.sYX功能ToolStripMenuItem.Name = "sYX功能ToolStripMenuItem";
            this.sYX功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.sYX功能ToolStripMenuItem.Text = "特殊功能";
            // 
            // 姓名横转竖列ToolStripMenuItem
            // 
            this.姓名横转竖列ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.姓名;
            this.姓名横转竖列ToolStripMenuItem.Name = "姓名横转竖列ToolStripMenuItem";
            this.姓名横转竖列ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.姓名横转竖列ToolStripMenuItem.Text = "姓名横转竖列";
            this.姓名横转竖列ToolStripMenuItem.Click += new System.EventHandler(this.姓名横转竖列ToolStripMenuItem_Click);
            // 
            // 特殊二维码校验ToolStripMenuItem
            // 
            this.特殊二维码校验ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.二维码;
            this.特殊二维码校验ToolStripMenuItem.Name = "特殊二维码校验ToolStripMenuItem";
            this.特殊二维码校验ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.特殊二维码校验ToolStripMenuItem.Text = "特殊二维码校验";
            this.特殊二维码校验ToolStripMenuItem.Click += new System.EventHandler(this.特殊二维码校验ToolStripMenuItem_Click);
            // 
            // twain扫描仪调试ToolStripMenuItem
            // 
            this.twain扫描仪调试ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.扫描仪;
            this.twain扫描仪调试ToolStripMenuItem.Name = "twain扫描仪调试ToolStripMenuItem";
            this.twain扫描仪调试ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.twain扫描仪调试ToolStripMenuItem.Text = "Twain扫描仪调试";
            this.twain扫描仪调试ToolStripMenuItem.Click += new System.EventHandler(this.twain扫描仪调试ToolStripMenuItem_Click);
            // 
            // mSS3模板修改ToolStripMenuItem
            // 
            this.mSS3模板修改ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.access;
            this.mSS3模板修改ToolStripMenuItem.Name = "mSS3模板修改ToolStripMenuItem";
            this.mSS3模板修改ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.mSS3模板修改ToolStripMenuItem.Text = "MSS3模板修改";
            this.mSS3模板修改ToolStripMenuItem.Click += new System.EventHandler(this.mSS3模板修改ToolStripMenuItem_Click);
            // 
            // 简易二维码生成ToolStripMenuItem
            // 
            this.简易二维码生成ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.二维码__1_;
            this.简易二维码生成ToolStripMenuItem.Name = "简易二维码生成ToolStripMenuItem";
            this.简易二维码生成ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.简易二维码生成ToolStripMenuItem.Text = "简易二维码生成";
            this.简易二维码生成ToolStripMenuItem.Click += new System.EventHandler(this.简易二维码生成ToolStripMenuItem_Click);
            // 
            // 随机身份证号码生成ToolStripMenuItem
            // 
            this.随机身份证号码生成ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.身份证;
            this.随机身份证号码生成ToolStripMenuItem.Name = "随机身份证号码生成ToolStripMenuItem";
            this.随机身份证号码生成ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.随机身份证号码生成ToolStripMenuItem.Text = "身份证号码校验";
            this.随机身份证号码生成ToolStripMenuItem.Click += new System.EventHandler(this.随机身份证号码生成ToolStripMenuItem_Click);
            // 
            // 文件副本生成ToolStripMenuItem
            // 
            this.文件副本生成ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.文件;
            this.文件副本生成ToolStripMenuItem.Name = "文件副本生成ToolStripMenuItem";
            this.文件副本生成ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.文件副本生成ToolStripMenuItem.Text = "文件副本生成";
            this.文件副本生成ToolStripMenuItem.Click += new System.EventHandler(this.文件副本生成ToolStripMenuItem_Click);
            // 
            // 网卡设置ToolStripMenuItem
            // 
            this.网卡设置ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.接口数据;
            this.网卡设置ToolStripMenuItem.Name = "网卡设置ToolStripMenuItem";
            this.网卡设置ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.网卡设置ToolStripMenuItem.Text = "网卡设置";
            this.网卡设置ToolStripMenuItem.Click += new System.EventHandler(this.网卡设置ToolStripMenuItem_Click);
            // 
            // 选举报告单样式生成ToolStripMenuItem
            // 
            this.选举报告单样式生成ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.报告单;
            this.选举报告单样式生成ToolStripMenuItem.Name = "选举报告单样式生成ToolStripMenuItem";
            this.选举报告单样式生成ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.选举报告单样式生成ToolStripMenuItem.Text = "选举结果报告单样式生成";
            this.选举报告单样式生成ToolStripMenuItem.Click += new System.EventHandler(this.选举报告单样式生成ToolStripMenuItem_Click);
            // 
            // 单人表生成ToolStripMenuItem
            // 
            this.单人表生成ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.word;
            this.单人表生成ToolStripMenuItem.Name = "单人表生成ToolStripMenuItem";
            this.单人表生成ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.单人表生成ToolStripMenuItem.Text = "单人表生成";
            this.单人表生成ToolStripMenuItem.Click += new System.EventHandler(this.单人表生成ToolStripMenuItem_Click);
            // 
            // 考评报表修改ToolStripMenuItem
            // 
            this.考评报表修改ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.文件;
            this.考评报表修改ToolStripMenuItem.Name = "考评报表修改ToolStripMenuItem";
            this.考评报表修改ToolStripMenuItem.Size = new System.Drawing.Size(216, 30);
            this.考评报表修改ToolStripMenuItem.Text = "考评报表修改";
            this.考评报表修改ToolStripMenuItem.Click += new System.EventHandler(this.考评报表修改ToolStripMenuItem_Click);
            // 
            // 其他功能ToolStripMenuItem
            // 
            this.其他功能ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.其他单据;
            this.其他功能ToolStripMenuItem.Name = "其他功能ToolStripMenuItem";
            this.其他功能ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.其他功能ToolStripMenuItem.Text = "其他功能";
            this.其他功能ToolStripMenuItem.Click += new System.EventHandler(this.其他功能ToolStripMenuItem_Click);
            // 
            // 系统工具ToolStripMenuItem
            // 
            this.系统工具ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.系统;
            this.系统工具ToolStripMenuItem.Name = "系统工具ToolStripMenuItem";
            this.系统工具ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.系统工具ToolStripMenuItem.Text = "系统工具";
            this.系统工具ToolStripMenuItem.Click += new System.EventHandler(this.系统工具ToolStripMenuItem_Click);
            // 
            // 关于软件ToolStripMenuItem
            // 
            this.关于软件ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.关于;
            this.关于软件ToolStripMenuItem.Name = "关于软件ToolStripMenuItem";
            this.关于软件ToolStripMenuItem.Size = new System.Drawing.Size(161, 30);
            this.关于软件ToolStripMenuItem.Text = "关于软件";
            this.关于软件ToolStripMenuItem.Click += new System.EventHandler(this.关于软件ToolStripMenuItem_Click);
            // 
            // 软件设置ToolStripMenuItem
            // 
            this.软件设置ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_start_page_index,
            this.menu_tabpage_heighlight,
            this.menu_tabpage_icon,
            this.menu_confirm_dialog,
            this.menu_listview_height,
            this.menu_listview_gridline,
            this.toolStripMenuItem1,
            this.vFSOFT数据包路径配置ToolStripMenuItem,
            this.界面大小设置ToolStripMenuItem,
            this.控件锚点模式ToolStripMenuItem,
            this.查看标签索引对照表ToolStripMenuItem,
            this.初始化配置文件ToolStripMenuItem,
            this.保存设置并重启软件ToolStripMenuItem});
            this.软件设置ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.系统;
            this.软件设置ToolStripMenuItem.Name = "软件设置ToolStripMenuItem";
            this.软件设置ToolStripMenuItem.Size = new System.Drawing.Size(92, 24);
            this.软件设置ToolStripMenuItem.Text = "软件设置";
            // 
            // menu_start_page_index
            // 
            this.menu_start_page_index.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_start_page_index_combox});
            this.menu_start_page_index.Image = global::DocumentTools.Properties.Resources.初始化;
            this.menu_start_page_index.Name = "menu_start_page_index";
            this.menu_start_page_index.Size = new System.Drawing.Size(212, 30);
            this.menu_start_page_index.Text = "初始页面索引设置";
            // 
            // menu_start_page_index_combox
            // 
            this.menu_start_page_index_combox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.menu_start_page_index_combox.Name = "menu_start_page_index_combox";
            this.menu_start_page_index_combox.Size = new System.Drawing.Size(121, 25);
            // 
            // menu_tabpage_heighlight
            // 
            this.menu_tabpage_heighlight.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_tabpage_heigh_light_combox});
            this.menu_tabpage_heighlight.Image = global::DocumentTools.Properties.Resources.红旗_高亮;
            this.menu_tabpage_heighlight.Name = "menu_tabpage_heighlight";
            this.menu_tabpage_heighlight.Size = new System.Drawing.Size(212, 30);
            this.menu_tabpage_heighlight.Text = "高亮标签卡";
            // 
            // menu_tabpage_heigh_light_combox
            // 
            this.menu_tabpage_heigh_light_combox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.menu_tabpage_heigh_light_combox.Name = "menu_tabpage_heigh_light_combox";
            this.menu_tabpage_heigh_light_combox.Size = new System.Drawing.Size(121, 25);
            // 
            // menu_tabpage_icon
            // 
            this.menu_tabpage_icon.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_tabpage_icon_combox});
            this.menu_tabpage_icon.Image = global::DocumentTools.Properties.Resources.标签;
            this.menu_tabpage_icon.Name = "menu_tabpage_icon";
            this.menu_tabpage_icon.Size = new System.Drawing.Size(212, 30);
            this.menu_tabpage_icon.Text = "标签卡图标";
            // 
            // menu_tabpage_icon_combox
            // 
            this.menu_tabpage_icon_combox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.menu_tabpage_icon_combox.Name = "menu_tabpage_icon_combox";
            this.menu_tabpage_icon_combox.Size = new System.Drawing.Size(121, 25);
            // 
            // menu_confirm_dialog
            // 
            this.menu_confirm_dialog.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_finshined_messbox_combox});
            this.menu_confirm_dialog.Image = global::DocumentTools.Properties.Resources.弹窗消息;
            this.menu_confirm_dialog.Name = "menu_confirm_dialog";
            this.menu_confirm_dialog.Size = new System.Drawing.Size(212, 30);
            this.menu_confirm_dialog.Text = "处理完毕弹窗";
            // 
            // menu_finshined_messbox_combox
            // 
            this.menu_finshined_messbox_combox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.menu_finshined_messbox_combox.Name = "menu_finshined_messbox_combox";
            this.menu_finshined_messbox_combox.Size = new System.Drawing.Size(121, 25);
            // 
            // menu_listview_height
            // 
            this.menu_listview_height.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_listview_line_heigh_combox});
            this.menu_listview_height.Image = global::DocumentTools.Properties.Resources.字行距;
            this.menu_listview_height.Name = "menu_listview_height";
            this.menu_listview_height.Size = new System.Drawing.Size(212, 30);
            this.menu_listview_height.Text = "文件列表行距";
            // 
            // menu_listview_line_heigh_combox
            // 
            this.menu_listview_line_heigh_combox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.menu_listview_line_heigh_combox.Name = "menu_listview_line_heigh_combox";
            this.menu_listview_line_heigh_combox.Size = new System.Drawing.Size(121, 25);
            // 
            // menu_listview_gridline
            // 
            this.menu_listview_gridline.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_listview_gridline_combox});
            this.menu_listview_gridline.Image = global::DocumentTools.Properties.Resources.icon_网格;
            this.menu_listview_gridline.Name = "menu_listview_gridline";
            this.menu_listview_gridline.Size = new System.Drawing.Size(212, 30);
            this.menu_listview_gridline.Text = "文件列表网格线";
            // 
            // menu_listview_gridline_combox
            // 
            this.menu_listview_gridline_combox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.menu_listview_gridline_combox.Name = "menu_listview_gridline_combox";
            this.menu_listview_gridline_combox.Size = new System.Drawing.Size(121, 25);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_chrome_path});
            this.toolStripMenuItem1.Image = global::DocumentTools.Properties.Resources.谷歌浏览器;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(212, 30);
            this.toolStripMenuItem1.Text = "Chromium路径配置";
            // 
            // menu_chrome_path
            // 
            this.menu_chrome_path.AutoSize = false;
            this.menu_chrome_path.AutoToolTip = true;
            this.menu_chrome_path.Name = "menu_chrome_path";
            this.menu_chrome_path.Size = new System.Drawing.Size(430, 30);
            // 
            // vFSOFT数据包路径配置ToolStripMenuItem
            // 
            this.vFSOFT数据包路径配置ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.考评数据包路径ToolStripMenuItem,
            this.选举数据包路径ToolStripMenuItem});
            this.vFSOFT数据包路径配置ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.云;
            this.vFSOFT数据包路径配置ToolStripMenuItem.Name = "vFSOFT数据包路径配置ToolStripMenuItem";
            this.vFSOFT数据包路径配置ToolStripMenuItem.Size = new System.Drawing.Size(212, 30);
            this.vFSOFT数据包路径配置ToolStripMenuItem.Text = "VFSOFT数据包路径配置";
            // 
            // 考评数据包路径ToolStripMenuItem
            // 
            this.考评数据包路径ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.vfsoft_examdata_path_textbox});
            this.考评数据包路径ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.access;
            this.考评数据包路径ToolStripMenuItem.Name = "考评数据包路径ToolStripMenuItem";
            this.考评数据包路径ToolStripMenuItem.Size = new System.Drawing.Size(168, 30);
            this.考评数据包路径ToolStripMenuItem.Text = "考评数据包路径";
            // 
            // vfsoft_examdata_path_textbox
            // 
            this.vfsoft_examdata_path_textbox.AutoToolTip = true;
            this.vfsoft_examdata_path_textbox.Name = "vfsoft_examdata_path_textbox";
            this.vfsoft_examdata_path_textbox.Size = new System.Drawing.Size(430, 23);
            // 
            // 选举数据包路径ToolStripMenuItem
            // 
            this.选举数据包路径ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.vfsoft_mssdata_path_textbox});
            this.选举数据包路径ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.压缩;
            this.选举数据包路径ToolStripMenuItem.Name = "选举数据包路径ToolStripMenuItem";
            this.选举数据包路径ToolStripMenuItem.Size = new System.Drawing.Size(168, 30);
            this.选举数据包路径ToolStripMenuItem.Text = "选举数据包路径";
            // 
            // vfsoft_mssdata_path_textbox
            // 
            this.vfsoft_mssdata_path_textbox.AutoToolTip = true;
            this.vfsoft_mssdata_path_textbox.Name = "vfsoft_mssdata_path_textbox";
            this.vfsoft_mssdata_path_textbox.Size = new System.Drawing.Size(430, 23);
            // 
            // 界面大小设置ToolStripMenuItem
            // 
            this.界面大小设置ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.设置界面宽度ToolStripMenuItem,
            this.设置界面高度ToolStripMenuItem});
            this.界面大小设置ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.ＧＵＩ界面设计;
            this.界面大小设置ToolStripMenuItem.Name = "界面大小设置ToolStripMenuItem";
            this.界面大小设置ToolStripMenuItem.Size = new System.Drawing.Size(212, 30);
            this.界面大小设置ToolStripMenuItem.Text = "界面大小设置";
            // 
            // 设置界面宽度ToolStripMenuItem
            // 
            this.设置界面宽度ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bodyform_width});
            this.设置界面宽度ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.宽度;
            this.设置界面宽度ToolStripMenuItem.Name = "设置界面宽度ToolStripMenuItem";
            this.设置界面宽度ToolStripMenuItem.Size = new System.Drawing.Size(156, 30);
            this.设置界面宽度ToolStripMenuItem.Text = "设置界面宽度";
            // 
            // bodyform_width
            // 
            this.bodyform_width.MaxLength = 500;
            this.bodyform_width.Name = "bodyform_width";
            this.bodyform_width.Size = new System.Drawing.Size(100, 23);
            // 
            // 设置界面高度ToolStripMenuItem
            // 
            this.设置界面高度ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bodyform_height});
            this.设置界面高度ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.高度;
            this.设置界面高度ToolStripMenuItem.Name = "设置界面高度ToolStripMenuItem";
            this.设置界面高度ToolStripMenuItem.Size = new System.Drawing.Size(156, 30);
            this.设置界面高度ToolStripMenuItem.Text = "设置界面高度";
            // 
            // bodyform_height
            // 
            this.bodyform_height.MaxLength = 500;
            this.bodyform_height.Name = "bodyform_height";
            this.bodyform_height.Size = new System.Drawing.Size(100, 23);
            // 
            // 控件锚点模式ToolStripMenuItem
            // 
            this.控件锚点模式ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_widget_anchor_combox});
            this.控件锚点模式ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.布局;
            this.控件锚点模式ToolStripMenuItem.Name = "控件锚点模式ToolStripMenuItem";
            this.控件锚点模式ToolStripMenuItem.Size = new System.Drawing.Size(212, 30);
            this.控件锚点模式ToolStripMenuItem.Text = "控件锚点配置";
            this.控件锚点模式ToolStripMenuItem.ToolTipText = resources.GetString("控件锚点模式ToolStripMenuItem.ToolTipText");
            // 
            // menu_widget_anchor_combox
            // 
            this.menu_widget_anchor_combox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.menu_widget_anchor_combox.Name = "menu_widget_anchor_combox";
            this.menu_widget_anchor_combox.Size = new System.Drawing.Size(121, 25);
            // 
            // 查看标签索引对照表ToolStripMenuItem
            // 
            this.查看标签索引对照表ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.查看;
            this.查看标签索引对照表ToolStripMenuItem.Name = "查看标签索引对照表ToolStripMenuItem";
            this.查看标签索引对照表ToolStripMenuItem.Size = new System.Drawing.Size(212, 30);
            this.查看标签索引对照表ToolStripMenuItem.Text = "查看标签索引对照表";
            this.查看标签索引对照表ToolStripMenuItem.Click += new System.EventHandler(this.查看标签索引对照表ToolStripMenuItem_Click);
            // 
            // 初始化配置文件ToolStripMenuItem
            // 
            this.初始化配置文件ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.系统初始化;
            this.初始化配置文件ToolStripMenuItem.Name = "初始化配置文件ToolStripMenuItem";
            this.初始化配置文件ToolStripMenuItem.Size = new System.Drawing.Size(212, 30);
            this.初始化配置文件ToolStripMenuItem.Text = "初始化配置文件";
            this.初始化配置文件ToolStripMenuItem.Click += new System.EventHandler(this.初始化配置文件ToolStripMenuItem_Click);
            // 
            // 保存设置并重启软件ToolStripMenuItem
            // 
            this.保存设置并重启软件ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.保存;
            this.保存设置并重启软件ToolStripMenuItem.Name = "保存设置并重启软件ToolStripMenuItem";
            this.保存设置并重启软件ToolStripMenuItem.Size = new System.Drawing.Size(212, 30);
            this.保存设置并重启软件ToolStripMenuItem.Text = "保存设置并重启软件";
            this.保存设置并重启软件ToolStripMenuItem.Click += new System.EventHandler(this.保存设置并重启软件ToolStripMenuItem_Click);
            // 
            // 程序更新ToolStripMenuItem
            // 
            this.程序更新ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.CheckVersionButton,
            this.VersionUpdateLog});
            this.程序更新ToolStripMenuItem.Image = global::DocumentTools.Properties.Resources.版本更新__1_;
            this.程序更新ToolStripMenuItem.Name = "程序更新ToolStripMenuItem";
            this.程序更新ToolStripMenuItem.Size = new System.Drawing.Size(92, 24);
            this.程序更新ToolStripMenuItem.Text = "版本更新";
            // 
            // CheckVersionButton
            // 
            this.CheckVersionButton.Image = global::DocumentTools.Properties.Resources.更新;
            this.CheckVersionButton.Name = "CheckVersionButton";
            this.CheckVersionButton.Size = new System.Drawing.Size(156, 30);
            this.CheckVersionButton.Text = "检查更新";
            this.CheckVersionButton.Click += new System.EventHandler(this.CheckVersionButton_Click);
            // 
            // VersionUpdateLog
            // 
            this.VersionUpdateLog.Image = global::DocumentTools.Properties.Resources.更新日志;
            this.VersionUpdateLog.Name = "VersionUpdateLog";
            this.VersionUpdateLog.Size = new System.Drawing.Size(156, 30);
            this.VersionUpdateLog.Text = "查看更新日志";
            this.VersionUpdateLog.Click += new System.EventHandler(this.VersionUpdateLog_Click);
            // 
            // MainToolsTip
            // 
            this.MainToolsTip.AutoPopDelay = 5000;
            this.MainToolsTip.InitialDelay = 100;
            this.MainToolsTip.ReshowDelay = 100;
            // 
            // TabPageIcoList
            // 
            this.TabPageIcoList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TabPageIcoList.ImageStream")));
            this.TabPageIcoList.TransparentColor = System.Drawing.Color.Transparent;
            this.TabPageIcoList.Images.SetKeyName(0, "word.png");
            this.TabPageIcoList.Images.SetKeyName(1, "pdf.png");
            this.TabPageIcoList.Images.SetKeyName(2, "Excel.png");
            this.TabPageIcoList.Images.SetKeyName(3, "ppt.png");
            this.TabPageIcoList.Images.SetKeyName(4, "epub.png");
            this.TabPageIcoList.Images.SetKeyName(5, "图片.png");
            this.TabPageIcoList.Images.SetKeyName(6, "microsoftoffice.png");
            this.TabPageIcoList.Images.SetKeyName(7, "压缩.png");
            this.TabPageIcoList.Images.SetKeyName(8, "data-matrix-scan.png");
            this.TabPageIcoList.Images.SetKeyName(9, "扫描仪驱动.png");
            this.TabPageIcoList.Images.SetKeyName(10, "其他.png");
            this.TabPageIcoList.Images.SetKeyName(11, "系统.png");
            this.TabPageIcoList.Images.SetKeyName(12, "关于.png");
            this.TabPageIcoList.Images.SetKeyName(13, "水印.png");
            this.TabPageIcoList.Images.SetKeyName(14, "姓名.png");
            this.TabPageIcoList.Images.SetKeyName(15, "二维码 (1).png");
            this.TabPageIcoList.Images.SetKeyName(16, "身份证.png");
            this.TabPageIcoList.Images.SetKeyName(17, "文件.png");
            this.TabPageIcoList.Images.SetKeyName(18, "接口数据.png");
            this.TabPageIcoList.Images.SetKeyName(19, "报告单.png");
            this.TabPageIcoList.Images.SetKeyName(20, "文档.png");
            this.TabPageIcoList.Images.SetKeyName(21, "组 12064.png");
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MemoryStatusLabel,
            this.MemoryProgressBar,
            this.UpdateCheckText});
            this.statusStrip1.Location = new System.Drawing.Point(0, 765);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 9, 0);
            this.statusStrip1.ShowItemToolTips = true;
            this.statusStrip1.Size = new System.Drawing.Size(890, 30);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Tag = "anchor_bottom";
            this.statusStrip1.Text = "statusStrip1";
            // 
            // MemoryStatusLabel
            // 
            this.MemoryStatusLabel.Image = global::DocumentTools.Properties.Resources.内存;
            this.MemoryStatusLabel.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.MemoryStatusLabel.Name = "MemoryStatusLabel";
            this.MemoryStatusLabel.Size = new System.Drawing.Size(80, 24);
            this.MemoryStatusLabel.Text = "内存信息";
            // 
            // MemoryProgressBar
            // 
            this.MemoryProgressBar.Margin = new System.Windows.Forms.Padding(2, 4, 2, 3);
            this.MemoryProgressBar.Name = "MemoryProgressBar";
            this.MemoryProgressBar.Size = new System.Drawing.Size(70, 23);
            this.MemoryProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // UpdateCheckText
            // 
            this.UpdateCheckText.Name = "UpdateCheckText";
            this.UpdateCheckText.Size = new System.Drawing.Size(56, 25);
            this.UpdateCheckText.Text = "程序版本";
            // 
            // BodyForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(890, 795);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.MainTabPage);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("宋体", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "BodyForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "文档工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BodyForm_FormClosing);
            this.Load += new System.EventHandler(this.BodyForm_Load);
            this.SizeChanged += new System.EventHandler(this.BodyForm_SizeChanged);
            this.Resize += new System.EventHandler(this.BodyForm_Resize);
            this.MainTabPage.ResumeLayout(false);
            this.TabPage_01.ResumeLayout(false);
            this.TabPage_01.PerformLayout();
            this.TabPage_02.ResumeLayout(false);
            this.TabPage_02.PerformLayout();
            this.TabPage_03.ResumeLayout(false);
            this.TabPage_03.PerformLayout();
            this.TabPage_07.ResumeLayout(false);
            this.TabPage_07.PerformLayout();
            this.TabPage_20.ResumeLayout(false);
            this.TabPage_20.PerformLayout();
            this.TabPage_21.ResumeLayout(false);
            this.TabPage_21.PerformLayout();
            this.TabPage_14.ResumeLayout(false);
            this.TabPage_14.PerformLayout();
            this.TabPage_31.ResumeLayout(false);
            this.TabPage_31.PerformLayout();
            this.groupBox48.ResumeLayout(false);
            this.groupBox48.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitWord_PageCount)).EndInit();
            this.TabPage_06.ResumeLayout(false);
            this.TabPage_06.PerformLayout();
            this.TabPage_17.ResumeLayout(false);
            this.TabPage_17.PerformLayout();
            this.groupBox47.ResumeLayout(false);
            this.groupBox47.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SplitPdf_PerPageCount)).EndInit();
            this.TabPage_41.ResumeLayout(false);
            this.TabPage_41.PerformLayout();
            this.groupBox62.ResumeLayout(false);
            this.groupBox62.PerformLayout();
            this.groupBox61.ResumeLayout(false);
            this.groupBox61.PerformLayout();
            this.TabPage_18.ResumeLayout(false);
            this.TabPage_18.PerformLayout();
            this.groupBox18.ResumeLayout(false);
            this.groupBox18.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WaterMarkImage)).EndInit();
            this.TabPage_19.ResumeLayout(false);
            this.TabPage_19.PerformLayout();
            this.TabPage_22.ResumeLayout(false);
            this.TabPage_22.PerformLayout();
            this.TabPage_15.ResumeLayout(false);
            this.TabPage_15.PerformLayout();
            this.TabPage_16.ResumeLayout(false);
            this.TabPage_16.PerformLayout();
            this.TabPage_04.ResumeLayout(false);
            this.TabPage_04.PerformLayout();
            this.TabPage_27.ResumeLayout(false);
            this.TabPage_27.PerformLayout();
            this.TabPage_28.ResumeLayout(false);
            this.TabPage_28.PerformLayout();
            this.TabPage_29.ResumeLayout(false);
            this.TabPage_29.PerformLayout();
            this.TabPage_23.ResumeLayout(false);
            this.TabPage_23.PerformLayout();
            this.groupBox19.ResumeLayout(false);
            this.groupBox19.PerformLayout();
            this.groupBox20.ResumeLayout(false);
            this.groupBox20.PerformLayout();
            this.TabPage_30.ResumeLayout(false);
            this.TabPage_30.PerformLayout();
            this.groupBox31.ResumeLayout(false);
            this.groupBox31.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.html2pdf_wait_timeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.html2pdf_timeout)).EndInit();
            this.groupBox30.ResumeLayout(false);
            this.groupBox30.PerformLayout();
            this.groupBox29.ResumeLayout(false);
            this.groupBox29.PerformLayout();
            this.TabPage_24.ResumeLayout(false);
            this.TabPage_24.PerformLayout();
            this.groupBox21.ResumeLayout(false);
            this.groupBox21.PerformLayout();
            this.TabPage_25.ResumeLayout(false);
            this.TabPage_25.PerformLayout();
            this.groupBox25.ResumeLayout(false);
            this.groupBox25.PerformLayout();
            this.groupBox24.ResumeLayout(false);
            this.groupBox24.PerformLayout();
            this.groupBox23.ResumeLayout(false);
            this.groupBox23.PerformLayout();
            this.groupBox22.ResumeLayout(false);
            this.groupBox22.PerformLayout();
            this.TabPage_10.ResumeLayout(false);
            this.TabPage_10.PerformLayout();
            this.groupBox10.ResumeLayout(false);
            this.groupBox10.PerformLayout();
            this.groupBox9.ResumeLayout(false);
            this.groupBox9.PerformLayout();
            this.TabPage_11.ResumeLayout(false);
            this.TabPage_11.PerformLayout();
            this.groupBox13.ResumeLayout(false);
            this.groupBox13.PerformLayout();
            this.groupBox12.ResumeLayout(false);
            this.groupBox12.PerformLayout();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.TabPage_12.ResumeLayout(false);
            this.TabPage_12.PerformLayout();
            this.groupBox14.ResumeLayout(false);
            this.TabPage_33.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barcodeRegionPicture)).EndInit();
            this.groupBox36.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barcodePreviewBox)).EndInit();
            this.groupBox38.ResumeLayout(false);
            this.groupBox38.PerformLayout();
            this.groupBox37.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.barcodeMutilList)).EndInit();
            this.groupBox35.ResumeLayout(false);
            this.groupBox35.PerformLayout();
            this.groupBox34.ResumeLayout(false);
            this.groupBox34.PerformLayout();
            this.TabPage_34.ResumeLayout(false);
            this.groupBox41.ResumeLayout(false);
            this.groupBox41.PerformLayout();
            this.groupBox42.ResumeLayout(false);
            this.groupBox42.PerformLayout();
            this.gen_card_result_group.ResumeLayout(false);
            this.gen_card_result_group.PerformLayout();
            this.gen_card_id_group.ResumeLayout(false);
            this.gen_card_id_group.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gen_card_number)).EndInit();
            this.TabPage_05.ResumeLayout(false);
            this.TabPage_05.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.TabPage_35.ResumeLayout(false);
            this.TabPage_35.PerformLayout();
            this.groupBox39.ResumeLayout(false);
            this.groupBox39.PerformLayout();
            this.TabPage_09.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPictureBox)).EndInit();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.TabPage_13.ResumeLayout(false);
            this.TabPage_13.PerformLayout();
            this.groupBox64.ResumeLayout(false);
            this.groupBox63.ResumeLayout(false);
            this.groupBox17.ResumeLayout(false);
            this.groupBox17.PerformLayout();
            this.groupBox16.ResumeLayout(false);
            this.groupBox15.ResumeLayout(false);
            this.groupBox15.PerformLayout();
            this.TabPage_26.ResumeLayout(false);
            this.blockgroup.ResumeLayout(false);
            this.blockgroup.PerformLayout();
            this.groupBox27.ResumeLayout(false);
            this.groupBox27.PerformLayout();
            this.groupBox26.ResumeLayout(false);
            this.groupBox26.PerformLayout();
            this.TabPage_32.ResumeLayout(false);
            this.groupBox32.ResumeLayout(false);
            this.groupBox32.PerformLayout();
            this.groupBox40.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.votedataview)).EndInit();
            this.groupBox33.ResumeLayout(false);
            this.groupBox33.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VotePicture)).EndInit();
            this.TabPage_39.ResumeLayout(false);
            this.TabPage_39.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            this.SingleDocumentConfigGroupBox.ResumeLayout(false);
            this.groupBox55.ResumeLayout(false);
            this.groupBox55.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SignDocumentNeedCount)).EndInit();
            this.SignDocumentPicSourceGroupBox.ResumeLayout(false);
            this.SignDocumentPicSourceGroupBox.PerformLayout();
            this.groupBox58.ResumeLayout(false);
            this.groupBox58.PerformLayout();
            this.groupBox54.ResumeLayout(false);
            this.groupBox54.PerformLayout();
            this.TabPage_40.ResumeLayout(false);
            this.TabPage_40.PerformLayout();
            this.groupBox59.ResumeLayout(false);
            this.groupBox59.PerformLayout();
            this.groupBox56.ResumeLayout(false);
            this.groupBox56.PerformLayout();
            this.groupBox57.ResumeLayout(false);
            this.TabPage_38.ResumeLayout(false);
            this.groupBox53.ResumeLayout(false);
            this.groupBox53.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VoteTableFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.VoteTableHeight)).EndInit();
            this.报告单生成日期.ResumeLayout(false);
            this.报告单生成日期.PerformLayout();
            this.报告单结束语.ResumeLayout(false);
            this.报告单结束语.PerformLayout();
            this.groupBox52.ResumeLayout(false);
            this.groupBox52.PerformLayout();
            this.groupBox51.ResumeLayout(false);
            this.groupBox51.PerformLayout();
            this.groupBox50.ResumeLayout(false);
            this.groupBox50.PerformLayout();
            this.groupBox49.ResumeLayout(false);
            this.groupBox49.PerformLayout();
            this.TabPage_36.ResumeLayout(false);
            this.groupBox60.ResumeLayout(false);
            this.groupBox60.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumberIndexEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumberIndexStart)).EndInit();
            this.groupBox28.ResumeLayout(false);
            this.groupBox28.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GetRandomComputerNameRandLength)).EndInit();
            this.groupBox46.ResumeLayout(false);
            this.groupBox46.PerformLayout();
            this.groupBox43.ResumeLayout(false);
            this.groupBox43.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Gen_Copied_Number)).EndInit();
            this.TabPage_37.ResumeLayout(false);
            this.TabPage_37.PerformLayout();
            this.groupBox45.ResumeLayout(false);
            this.groupBox45.PerformLayout();
            this.groupBox44.ResumeLayout(false);
            this.groupBox44.PerformLayout();
            this.TabPage_08.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            this.ListViewRightMenu.ResumeLayout(false);
            this.TreeViewContentMenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainFormError)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl MainTabPage;
        private System.Windows.Forms.TabPage TabPage_01;
        private System.Windows.Forms.TabPage TabPage_02;
        private System.Windows.Forms.Button ChooseDocButton;
        internal System.Windows.Forms.ListView WordFileListView;
        private System.Windows.Forms.Button ConvertButton;
        private System.Windows.Forms.Label StatusText;
        private System.Windows.Forms.Button SelectPdfButton;
        private System.Windows.Forms.Button Convert2DocButton;
        internal System.Windows.Forms.ListView PdfFileListView;
        private System.Windows.Forms.Label PdfStatusText;
        private System.Windows.Forms.TabPage TabPage_03;
        internal System.Windows.Forms.ListView ExcelFileListView;
        private System.Windows.Forms.Button ConvertExcel2PdfButton;
        private System.Windows.Forms.Button SelectExcelFileButton;
        internal System.Windows.Forms.SaveFileDialog SaveFileDialog;
        internal System.Windows.Forms.CheckBox OpenPdfOptions;
        internal System.Windows.Forms.CheckBox RemoveTempDocx;
        private System.Windows.Forms.Label Excel2PdfStatusText;
        internal System.Windows.Forms.CheckBox ExportEachSheet;
        private System.Windows.Forms.TabPage TabPage_04;
        private System.Windows.Forms.ComboBox ConvertImageList;
        private System.Windows.Forms.Button ConvertWord2ImageButton;
        internal System.Windows.Forms.ListView DocumentsListView;
        internal System.Windows.Forms.CheckBox SetGrayCheckBox;
        private System.Windows.Forms.Label ConvertWord2ImageStatus;
        private System.Windows.Forms.Button SelectWord2ImageFileButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        internal System.Windows.Forms.CheckBox PageExportCheck;
        internal System.Windows.Forms.ComboBox DpiItemList;
        private System.Windows.Forms.TabPage TabPage_05;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RichTextBox TargetTextBox;
        private System.Windows.Forms.RichTextBox SourceTextBox;
        private System.Windows.Forms.Button ConvertTextButton;
        private System.Windows.Forms.Label TargetTextNumberText;
        private System.Windows.Forms.CheckBox RemoveSquareCheck;
        private System.Windows.Forms.CheckBox RemoveNumberCheck;
        private System.Windows.Forms.CheckBox DoubleTextAlignCheck;
        private System.Windows.Forms.CheckBox MutilLineMode;
        private System.Windows.Forms.CheckBox NameStorkeChcek;
        private System.Windows.Forms.TabPage TabPage_07;
        private System.Windows.Forms.Label ConvertPDF2ExcelStatusText;
        private System.Windows.Forms.Button ConvertPDF2Excel;
        private System.Windows.Forms.Button SelectPDF2ExcelButton;
        private System.Windows.Forms.TabPage TabPage_06;
        internal System.Windows.Forms.ListView PDF2ExcelFileView;
        internal System.Windows.Forms.CheckBox ExcelSheelMode;
        private System.Windows.Forms.Button MeregePdfFileButtonConvert;
        private System.Windows.Forms.Button MergePdfFileSelectButton;
        private System.Windows.Forms.Label MeregePdfFileStatusText;
        internal System.Windows.Forms.ListView MergePdfFileView;
        private System.Windows.Forms.CheckBox OpenMeregePdfFile;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ContextMenuStrip ListViewRightMenu;
        private System.Windows.Forms.ToolStripMenuItem 列表操作ToolStripMenuItem;
        private System.Windows.Forms.TabPage TabPage_08;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.LinkLabel DownloadFrameWorkLabel;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TabPage TabPage_09;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.LinkLabel SelectTwainDevice;
        private System.Windows.Forms.LinkLabel SetTwainDevice;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox ScanPictureSavePathBox;
        private System.Windows.Forms.Button StartScanButton;
        private System.Windows.Forms.Button SelectScanPath;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.PictureBox PreviewPictureBox;
        private System.Windows.Forms.Label PreviewPictureName;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label ScannTotalNumber;
        private System.Windows.Forms.Label NowScanNumber;
        private System.Windows.Forms.Timer MemoryTimer;
        private System.Windows.Forms.Label ScanSpendTime;
        private System.Windows.Forms.Label SavedPathFileCount;
        private System.Windows.Forms.Button RemoveAllFiles;
        private System.Windows.Forms.Button ResetTotalScanNumber;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.ComboBox TwainSleepTimeSet;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox TwainDpi;
        private System.Windows.Forms.Label ScanSerialNumber;
        private System.Windows.Forms.CheckBox DoubleScanBox;
        private System.Windows.Forms.TabPage TabPage_10;
        private System.Windows.Forms.TabPage TabPage_11;
        private System.Windows.Forms.Button SelectReplaceWordFilesButton;
        private System.Windows.Forms.Button ReplaceWordContentButton;
        private System.Windows.Forms.Label ReplaceWordContentStatusText;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.GroupBox groupBox10;
        internal System.Windows.Forms.ListView ReplaceWordFileList;
        internal System.Windows.Forms.TextBox WordReplaceFindTextBox;
        internal System.Windows.Forms.TextBox WordReplaceTextBox;
        internal System.Windows.Forms.CheckBox IgnoreFootContent;
        internal System.Windows.Forms.CheckBox IgnoreCase;
        private System.Windows.Forms.LinkLabel OnlineRegextTool;
        private System.Windows.Forms.LinkLabel TurialRegex;
        private System.Windows.Forms.Button SelectRelaceExcelButton;
        private System.Windows.Forms.Button ReplaceExcelContentButton;
        private System.Windows.Forms.Label ReplaceExcelStatusText;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.GroupBox groupBox12;
        private System.Windows.Forms.GroupBox groupBox13;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        internal System.Windows.Forms.ListView ReplaceExcelFileView;
        internal System.Windows.Forms.CheckBox ExcelNoIgnoreCase;
        internal System.Windows.Forms.TextBox ExcelFindTextBox;
        internal System.Windows.Forms.TextBox ExcelReplaceBox;
        internal System.Windows.Forms.CheckBox RemoveRows;
        internal System.Windows.Forms.TextBox ExcelStartRow;
        internal System.Windows.Forms.TextBox ExcelEndRow;
        internal System.Windows.Forms.CheckBox UseRegex;
        internal System.Windows.Forms.Label ProcessPage_P2W;
        private System.Windows.Forms.LinkLabel ExcelRegexReadMe;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TabPage TabPage_12;
        private System.Windows.Forms.GroupBox groupBox14;
        private System.Windows.Forms.CheckBox SkipBlankPage;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox TwainPaperSizeBox;
        private System.Windows.Forms.Button SelectDataMatrixWord;
        private System.Windows.Forms.Button DecodeDataMarix;
        private System.Windows.Forms.Label DecodeDataMatixStatusText;
        internal System.Windows.Forms.ListView DecodeMatrixWordListView;
        private System.Windows.Forms.ContextMenuStrip TreeViewContentMenu;
        private System.Windows.Forms.ToolStripMenuItem 展开折叠本节点全部ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 展开折叠全部节点ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开文件ToolStripMenuItem;
        private System.Windows.Forms.TreeView DecodeDataMatrixView;
        private System.Windows.Forms.TabPage TabPage_13;
        private System.Windows.Forms.GroupBox groupBox15;
        private System.Windows.Forms.Button DownloadNetfx35;
        private System.Windows.Forms.ProgressBar Downloadfx35Progress;
        private System.Windows.Forms.Label ProcessLabel;
        private System.Windows.Forms.LinkLabel LanzouNetfx;
        private System.Windows.Forms.GroupBox groupBox16;
        private System.Windows.Forms.Button ReleaseRegistry;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.CheckBox UseInterfaceUi;
        private System.Windows.Forms.GroupBox groupBox17;
        private System.Windows.Forms.Label Image2IcoStatusText;
        private System.Windows.Forms.Button ConvertImage2Ico;
        private System.Windows.Forms.Button SelectFilesIco;
        private System.Windows.Forms.Label label22;
        internal System.Windows.Forms.ListView IcoFileView;
        internal System.Windows.Forms.ComboBox IcoSizeDrop;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.TextBox TwainScanNumberSet;
        private System.Windows.Forms.CheckBox ExportCaps;
        private System.Windows.Forms.LinkLabel linkLabel2;
        private System.Windows.Forms.TabPage TabPage_14;
        private System.Windows.Forms.Button MergeWordFile;
        private System.Windows.Forms.Button SelectMergeWordFiles;
        internal System.Windows.Forms.ListView MergeWordFileList;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label MeregeWordStatusText;
        private System.Windows.Forms.TabPage TabPage_15;
        private System.Windows.Forms.TabPage TabPage_16;
        internal System.Windows.Forms.ListView Word2ExcelFileList;
        private System.Windows.Forms.Button SelectWord2ExcelFiles;
        private System.Windows.Forms.Button ConvertWord2Excel;
        private System.Windows.Forms.Label ConverWord2ExcelStatusText;
        private System.Windows.Forms.Button SelectExcel2WordFiles;
        private System.Windows.Forms.Button ConvertExcel2Word;
        private System.Windows.Forms.Label ConvertExcel2WordStatusText;
        internal System.Windows.Forms.ListView Excel2WordFileList;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.TabPage TabPage_17;
        private System.Windows.Forms.Button SelectSplitPdf;
        private System.Windows.Forms.Button StartSplitPdf;
        private System.Windows.Forms.Label SplitPdfStatusText;
        internal System.Windows.Forms.ListView SplitPdfFileList;
        private System.Windows.Forms.TabPage TabPage_18;
        private System.Windows.Forms.GroupBox groupBox18;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Button PdfWaterMarkFontColorSettings;
        private System.Windows.Forms.ColorDialog ColorSelectDialog;
        private System.Windows.Forms.Button PDFWaterMarkTextFontSetting;
        private System.Windows.Forms.FontDialog FontSelectDialog;
        private System.Windows.Forms.Label shui;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label31;
        internal System.Windows.Forms.ListView PdfWaterMarkFileList;
        private System.Windows.Forms.Button SelectWaterMarkPdfFiles;
        private System.Windows.Forms.Button AddWaterMark2Pdf;
        private System.Windows.Forms.Label AddWaterMark2PdfStatusText;
        internal System.Windows.Forms.TextBox PdfWaterMarkText;
        internal System.Windows.Forms.ComboBox PdfWaterMarkOpcityNumber;
        internal System.Windows.Forms.TextBox PdfWaterMarkRotateDegNumber;
        internal System.Windows.Forms.ComboBox PdfWaterMarkHorSetting;
        internal System.Windows.Forms.ComboBox PDFWaterMarkVerSetting;
        private System.Windows.Forms.TabPage TabPage_19;
        internal System.Windows.Forms.ListView RemovePdfWaterMarkFileList;
        private System.Windows.Forms.Button SelectRemovePdfWaterMarkFiles;
        private System.Windows.Forms.Button RemovePdfWaterMark;
        private System.Windows.Forms.Label RemovePdfWaterMarkStatusText;
        private System.Windows.Forms.Button AddBackGroudImage;
        private System.Windows.Forms.PictureBox WaterMarkImage;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.ErrorProvider MainFormError;
        internal System.Windows.Forms.TextBox WaterMarkImageWidth;
        internal System.Windows.Forms.TextBox WaterMarkImageHeight;
        private System.Windows.Forms.Label label34;
        internal System.Windows.Forms.ComboBox PDFComplianceBox;
        private System.Windows.Forms.TabPage TabPage_20;
        private System.Windows.Forms.TabPage TabPage_21;
        private System.Windows.Forms.Button ConvertPPT2PDF;
        private System.Windows.Forms.Button SelectPPT2PdfFile;
        internal System.Windows.Forms.ListView PPT2PdfFileList;
        private System.Windows.Forms.Label ConvertPPT2PDFStatusText;
        private System.Windows.Forms.Label label35;
        internal System.Windows.Forms.ComboBox ExportNotePageCombox;
        internal System.Windows.Forms.CheckBox DrawBlackLine;
        internal System.Windows.Forms.CheckBox ExportHiddenPage;
        private System.Windows.Forms.Label label36;
        internal System.Windows.Forms.TextBox ConvertPPT2PDFPassword;
        private System.Windows.Forms.Button SelectConvertPDF2PPT;
        private System.Windows.Forms.Button ConvertPDF2PPT;
        private System.Windows.Forms.Label Pdf2PPTStatusText;
        internal System.Windows.Forms.ListView PDF2PPTFileList;
        internal System.Windows.Forms.CheckBox SplitShapeBox;
        internal System.Windows.Forms.CheckBox ExtraOcrBox;
        internal System.Windows.Forms.CheckBox RenderImageBox;
        private System.Windows.Forms.TabPage TabPage_22;
        internal System.Windows.Forms.ListView ConvertPdfComplianceFileList;
        private System.Windows.Forms.Button SelectConvertComplaincePdf;
        private System.Windows.Forms.Button ConvertPdfCompliance;
        private System.Windows.Forms.Label ConvertPdfComplianceStatusText;
        private System.Windows.Forms.Label label39;
        internal System.Windows.Forms.ComboBox ConvertPdfComplianceVersion;
        private System.Windows.Forms.TabPage TabPage_23;
        private System.Windows.Forms.Button SelectConvertImage2PDF;
        private System.Windows.Forms.Button ConvertImage2PDF;
        internal System.Windows.Forms.ListView ConvertImage2PDFFileList;
        private System.Windows.Forms.GroupBox groupBox19;
        private System.Windows.Forms.CheckBox ExportSignlePDF;
        private System.Windows.Forms.Label COnvertImage2PDFStatusText;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.ComboBox Image2PDFCombox;
        private System.Windows.Forms.GroupBox groupBox20;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.TextBox ImagePdfPageSIze_RIGHT;
        private System.Windows.Forms.TextBox ImagePdfPageSIze_BOTTOM;
        private System.Windows.Forms.TextBox ImagePdfPageSIze_LEFT;
        private System.Windows.Forms.TextBox ImagePdfPageSIze_UP;
        private System.Windows.Forms.CheckBox Image2PDFOptiomize;
        private System.Windows.Forms.CheckBox Image2PDFCOmpressImage;
        private System.Windows.Forms.TabPage TabPage_24;
        internal System.Windows.Forms.ListView CompressPdfFileList;
        private System.Windows.Forms.Button SelectCompressPDFFiles;
        private System.Windows.Forms.Button CompressPDF;
        private System.Windows.Forms.GroupBox groupBox21;
        internal System.Windows.Forms.CheckBox CompressPDF_WEB;
        private System.Windows.Forms.Label label45;
        internal System.Windows.Forms.ComboBox CompressPDF_ImageQuailty;
        private System.Windows.Forms.Label label44;
        internal System.Windows.Forms.CheckBox CompressPDF_CompressImage;
        internal System.Windows.Forms.CheckBox CompressPDF_RemoveUnuseObject;
        internal System.Windows.Forms.ComboBox CompressPDF_ImageCompressVersion;
        private System.Windows.Forms.Label label46;
        internal System.Windows.Forms.ComboBox CompressPDF_ImageResoution;
        internal System.Windows.Forms.CheckBox CompressPDF_RemoveUnuseStream;
        internal System.Windows.Forms.CheckBox CompressPDF_LinkSameStream;
        internal System.Windows.Forms.CheckBox CompressPDF_CancelEmbedFont;
        internal System.Windows.Forms.CheckBox CompressPDF_CompressImageFlate;
        internal System.Windows.Forms.CheckBox CompressPDF_RemoveForms;
        private System.Windows.Forms.Label CompressPDFStatusText;
        private System.Windows.Forms.TabPage TabPage_25;
        internal System.Windows.Forms.ListView CompressImageFileList;
        private System.Windows.Forms.Button SelectCompressImageFile;
        private System.Windows.Forms.Button CompressImage;
        private System.Windows.Forms.GroupBox groupBox22;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.GroupBox groupBox23;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.Label label52;
        private System.Windows.Forms.GroupBox groupBox24;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.GroupBox groupBox25;
        private System.Windows.Forms.Label label54;
        internal System.Windows.Forms.ComboBox CompressJpegQuailty_Combox;
        internal System.Windows.Forms.ComboBox CompressJpegColorMode_Combox;
        internal System.Windows.Forms.ComboBox CompressJpegMode_Combox;
        internal System.Windows.Forms.ComboBox CompressPngLevel_Combox;
        internal System.Windows.Forms.ComboBox CompressBmpMode_Combox;
        internal System.Windows.Forms.ComboBox CompressBmpPixeBits_Combox;
        internal System.Windows.Forms.CheckBox CompressImage_NotUseSelfExt;
        internal System.Windows.Forms.ComboBox CompressImage_OutPutImageFormat;
        internal System.Windows.Forms.ComboBox CompressPngColorMode_Combox;
        private System.Windows.Forms.Label CompressImageStatusText;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button PlayCXKMusic;
        private System.Windows.Forms.Button WordFileListView_MoveDown;
        private System.Windows.Forms.Button WordFileListView_MoveUp;
        private System.Windows.Forms.Button PdfFileListView_MoveUp;
        private System.Windows.Forms.Button PdfFileListView_MoveDown;
        private System.Windows.Forms.Button ExcelFileListView_MoveUp;
        private System.Windows.Forms.Button ExcelFileListView_MoveDown;
        private System.Windows.Forms.Button PDF2ExcelFileView_MoveUp;
        private System.Windows.Forms.Button PDF2ExcelFileView_MoveDown;
        private System.Windows.Forms.Button PPT2PdfFileList_MoveUp;
        private System.Windows.Forms.Button PPT2PdfFileList_MoveDown;
        private System.Windows.Forms.Button PDF2PPTFileList_MoveUp;
        private System.Windows.Forms.Button PDF2PPTFileList_MoveDown;
        private System.Windows.Forms.Button MergeWordFileList_MoveUp;
        private System.Windows.Forms.Button MergeWordFileList_MoveDown;
        private System.Windows.Forms.Button MergePdfFileView_MoveUp;
        private System.Windows.Forms.Button MergePdfFileView_MoveDown;
        private System.Windows.Forms.Button SplitPdfFileList_MoveUp;
        private System.Windows.Forms.Button SplitPdfFileList_MoveDown;
        private System.Windows.Forms.Button PdfWaterMarkFileList_MoveUp;
        private System.Windows.Forms.Button PdfWaterMarkFileList_MoveDown;
        private System.Windows.Forms.Button ReplaceWordFileList_MoveUp;
        private System.Windows.Forms.Button ReplaceWordFileList_MoveDown;
        private System.Windows.Forms.Button ReplaceExcelFileView_MoveUp;
        private System.Windows.Forms.Button ReplaceExcelFileView_MoveDown;
        private System.Windows.Forms.Button DecodeMatrixWordListView_MoveUp;
        private System.Windows.Forms.Button DecodeMatrixWordListView_MoveDown;
        private System.Windows.Forms.Button IcoFileView_MoveUp;
        private System.Windows.Forms.Button IcoFileView_MoveDown;
        private System.Windows.Forms.Button RemovePdfWaterMarkFileList_MoveUp;
        private System.Windows.Forms.Button RemovePdfWaterMarkFileList_MoveDown;
        private System.Windows.Forms.Button ConvertPdfComplianceFileList_MoveUp;
        private System.Windows.Forms.Button ConvertPdfComplianceFileList_MoveDown;
        private System.Windows.Forms.Button Word2ExcelFileList_MoveUp;
        private System.Windows.Forms.Button Word2ExcelFileList_MoveDown;
        private System.Windows.Forms.Button Excel2WordFileList_MoveUp;
        private System.Windows.Forms.Button Excel2WordFileList_MoveDown;
        private System.Windows.Forms.Button DocumentsListView_MoveUp;
        private System.Windows.Forms.Button DocumentsListView_MoveDown;
        private System.Windows.Forms.Button ConvertImage2PDFFileList_MoveUp;
        private System.Windows.Forms.Button ConvertImage2PDFFileList_MoveDown;
        private System.Windows.Forms.Button CompressPdfFileList_MoveUp;
        private System.Windows.Forms.Button CompressPdfFileList_MoveDown;
        private System.Windows.Forms.Button CompressImageFileList_MoveUp;
        private System.Windows.Forms.Button CompressImageFileList_MoveDown;
        private System.Windows.Forms.TextBox NameReplaceWord_Search;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.TextBox NameReplaceWord_Target;
        private System.Windows.Forms.CheckBox NameReplaceWord;
        private System.Windows.Forms.CheckBox NameAutoCopy;
        private System.Windows.Forms.Label NameAutoCopyStatusText;
        private System.Windows.Forms.Label NameReplaceRegexMode;
        private System.Windows.Forms.Label label55;
        internal System.Windows.Forms.CheckBox PDF2WORD_UseSUbOcr;
        internal System.Windows.Forms.CheckBox PDF2WORD_UseBullet;
        internal System.Windows.Forms.ComboBox PDF2WORD_Mode;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label57;
        private System.Windows.Forms.Button word2pdf_clear;
        private System.Windows.Forms.Button word2pdf_remove;
        private System.Windows.Forms.Button pdf2word_clear;
        private System.Windows.Forms.Button pdf2word_remove;
        private System.Windows.Forms.Button excel2pdf_clear;
        private System.Windows.Forms.Button excel2pdf_remove;
        private System.Windows.Forms.Button pdf2excel_clear;
        private System.Windows.Forms.Button pdf2excel_remove;
        private System.Windows.Forms.Button ppt2pdf_clear;
        private System.Windows.Forms.Button ppt2pdf_remove;
        private System.Windows.Forms.Button pdf2ppt_clear;
        private System.Windows.Forms.Button pdf2ppt_remove;
        private System.Windows.Forms.Button meregeword_clear;
        private System.Windows.Forms.Button meregeword_remove;
        private System.Windows.Forms.Button meregepdf_clear;
        private System.Windows.Forms.Button meregepdf_remove;
        private System.Windows.Forms.Button sqplitpdf_clear;
        private System.Windows.Forms.Button sqplitpdf_remove;
        private System.Windows.Forms.Button addpdfwater_clear;
        private System.Windows.Forms.Button addpdfwater_remove;
        private System.Windows.Forms.Button removepdfwater_clear;
        private System.Windows.Forms.Button removepdfwater_remove;
        private System.Windows.Forms.Button pdfversion_clear;
        private System.Windows.Forms.Button pdfversion_remove;
        private System.Windows.Forms.Button word2excel_clear;
        private System.Windows.Forms.Button word2excel_remove;
        private System.Windows.Forms.Button excel2word_clear;
        private System.Windows.Forms.Button excel2word_remove;
        private System.Windows.Forms.Button document2image_clear;
        private System.Windows.Forms.Button document2image_remove;
        private System.Windows.Forms.Button image2pdf_clear;
        private System.Windows.Forms.Button image2pdf_remove;
        private System.Windows.Forms.Button compresspdf_clear;
        private System.Windows.Forms.Button compresspdf_remove;
        private System.Windows.Forms.Button compressimage_clear;
        private System.Windows.Forms.Button compressimage_remove;
        private System.Windows.Forms.Button wordreplace_clear;
        private System.Windows.Forms.Button wordreplace_remove;
        private System.Windows.Forms.Button excelreplace_clear;
        private System.Windows.Forms.Button excelreplace_remove;
        private System.Windows.Forms.Button datamatrix_clear;
        private System.Windows.Forms.Button datamatrix_remove;
        private System.Windows.Forms.Button image2ico_clear;
        private System.Windows.Forms.Button image2ico_remove;
        private System.Windows.Forms.ToolTip MainToolsTip;
        private System.Windows.Forms.TabPage TabPage_26;
        private System.Windows.Forms.GroupBox groupBox26;
        private System.Windows.Forms.Button UnlockKeyBoard;
        private System.Windows.Forms.Button LockKeyBoard;
        private System.Windows.Forms.Label KeyBoardLockStatus;
        private System.Windows.Forms.GroupBox groupBox27;
        private System.Windows.Forms.Label ShortStatus;
        private System.Windows.Forms.Button RestoreShort;
        private System.Windows.Forms.Button RemoveShort;
        private System.Windows.Forms.CheckBox AutoRestartExplorer;
        private System.Windows.Forms.TabPage TabPage_27;
        private System.Windows.Forms.Button SelectWord2EpubFiles;
        private System.Windows.Forms.Button ConvertWord2Epub;
        private System.Windows.Forms.Button word2epub_clear;
        private System.Windows.Forms.Button word2epub_remove;
        private System.Windows.Forms.Button Word2EPUBFileList_MoveDown;
        private System.Windows.Forms.Button Word2EPUBFileList_MoveUp;
        internal System.Windows.Forms.ListView Word2EPUBFileList;
        private System.Windows.Forms.Label ConvertWord2EpubStatusText;
        private System.Windows.Forms.TabPage TabPage_28;
        private System.Windows.Forms.Button EPUB2WORDFileList_MoveDown;
        private System.Windows.Forms.Button EPUB2WORDFileList_MoveUp;
        private System.Windows.Forms.Button epub2word_clear;
        private System.Windows.Forms.Button epub2word_remove;
        private System.Windows.Forms.Label ConvertEpub2WordStatusText;
        private System.Windows.Forms.Button ConvertEpub2Word;
        private System.Windows.Forms.Button SelectEpub2WordFiles;
        internal System.Windows.Forms.ListView EPUB2WORDFileList;
        private System.Windows.Forms.TabPage TabPage_29;
        private System.Windows.Forms.Button EPUB2PDFFileList_MoveDown;
        private System.Windows.Forms.Button EPUB2PDFFileList_MoveUp;
        private System.Windows.Forms.Button epub2pdf_clear;
        private System.Windows.Forms.Button epub2pdf_remove;
        private System.Windows.Forms.Label ConvertEPUB2PDFStatusText;
        private System.Windows.Forms.Button ConvertEPUB2PDF;
        private System.Windows.Forms.Button SelectEPUB2PDFFiles;
        internal System.Windows.Forms.ListView EPUB2PDFFileList;
        private System.Windows.Forms.Label label59;
        private System.Windows.Forms.Label label60;
        private System.Windows.Forms.ToolStripMenuItem 文件操作ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开源文件目录ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开源文件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开转换后目录ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 打开转换后文件ToolStripMenuItem;
        private System.Windows.Forms.Label label61;
        internal System.Windows.Forms.TextBox Word2EpubAuthorName;
        private System.Windows.Forms.ToolStripMenuItem 清空列表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 删除本项ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 获取PDF文件版本信息ToolStripMenuItem;
        private System.Windows.Forms.ImageList TabPageIcoList;
        private System.Windows.Forms.Label label62;
        internal System.Windows.Forms.ComboBox Word2PDFOptimizCombox;
        private System.Windows.Forms.TabPage TabPage_30;
        private System.Windows.Forms.Button HTML2PDFFileList_MoveDown;
        private System.Windows.Forms.Button HTML2PDFFileList_MoveUp;
        private System.Windows.Forms.Button html2pdf_clear;
        private System.Windows.Forms.Button html2pdf_remove;
        private System.Windows.Forms.Label ConvertHtml2PdfStatusText;
        private System.Windows.Forms.Button ConvertHtml2Pdf;
        private System.Windows.Forms.Button SelectHtml2PDFFiles;
        internal System.Windows.Forms.ListView HTML2PDFFileList;
        private System.Windows.Forms.Label label63;
        private System.Windows.Forms.Button SetWord2EpubCoverImage;
        internal System.Windows.Forms.TextBox Word2EpubConverImage;
        private System.Windows.Forms.Label label64;
        internal System.Windows.Forms.TextBox CoverImageDpi;
        private System.Windows.Forms.Label label65;
        private System.Windows.Forms.TabPage TabPage_31;
        private System.Windows.Forms.Button SplitWordFileList_MoveDown;
        private System.Windows.Forms.Button SplitWordFileList_MoveUp;
        private System.Windows.Forms.Button splitword_clear;
        private System.Windows.Forms.Button splitword_remove;
        private System.Windows.Forms.Label StartSplitWordStatusText;
        private System.Windows.Forms.Button StartSplitWord;
        private System.Windows.Forms.Button SelectSplitWordFiles;
        internal System.Windows.Forms.ListView SplitWordFileList;
        private System.Windows.Forms.Label label66;
        private System.Windows.Forms.GroupBox groupBox29;
        private System.Windows.Forms.Label label67;
        private System.Windows.Forms.Label label68;
        private System.Windows.Forms.GroupBox groupBox30;
        private System.Windows.Forms.Label label72;
        private System.Windows.Forms.Label label71;
        private System.Windows.Forms.Label label70;
        private System.Windows.Forms.Label label69;
        private System.Windows.Forms.GroupBox groupBox31;
        private System.Windows.Forms.Label label73;
        private System.Windows.Forms.Button html2pdf_button_set_chrome_path;
        private System.Windows.Forms.LinkLabel linkLabel3;
        internal System.Windows.Forms.ComboBox html2pdf_combox_pagesize;
        internal System.Windows.Forms.ComboBox html2pdf_combox_pagedirect;
        internal System.Windows.Forms.CheckBox html2pdf_checkbox_usecss;
        internal System.Windows.Forms.TextBox html2pdf_textbox_margin_top;
        internal System.Windows.Forms.TextBox html2pdf_textbox_margin_bottom;
        internal System.Windows.Forms.TextBox html2pdf_textbox_margin_left;
        internal System.Windows.Forms.TextBox html2pdf_textbox_margin_right;
        internal System.Windows.Forms.TextBox html2pdf_textbox_chromepath;
        private System.Windows.Forms.Label label74;
        private System.Windows.Forms.TabPage TabPage_32;
        private System.Windows.Forms.GroupBox groupBox32;
        private System.Windows.Forms.Label label75;
        private System.Windows.Forms.TextBox Mss3Path;
        private System.Windows.Forms.Button SelectMss3;
        private System.Windows.Forms.Button SelectVotePic;
        private System.Windows.Forms.PictureBox VotePicture;
        private System.Windows.Forms.Button StartInsert;
        private System.Windows.Forms.Label votestatustext;
        private System.Windows.Forms.TextBox voteid;
        private System.Windows.Forms.Label label76;
        private System.Windows.Forms.GroupBox groupBox33;
        private System.Windows.Forms.TextBox votename;
        private System.Windows.Forms.Label label77;
        private System.Windows.Forms.TextBox voteindex;
        private System.Windows.Forms.Label label78;
        private System.Windows.Forms.Button WordReplaceFontSeeting;
        internal System.Windows.Forms.CheckBox UseNewStyle;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem liblist;
        private System.Windows.Forms.ToolStripMenuItem wORD功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eXCEL功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pDF功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pPT功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ePUB功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hTML功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 压缩功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wORD转PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wORD转EXCELToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wORDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wORD内容批量替换ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eXCEL转PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eXCEL转WORDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem eXCEL内容批量替换ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 图片功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pDF转EXCELToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pPT转PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pDF转PPTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ePUB转WORDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ePUB转PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem hTML转PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 压缩PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 压缩图片ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 拆分合并功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 拆分WORDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 拆分PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 合并WORDToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 合并PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wORDEXCELPDF转图片ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 图片转PDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sYX功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 姓名横转竖列ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 特殊二维码校验ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem twain扫描仪调试ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mSS3模板修改ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 其他功能ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 系统工具ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 关于软件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 软件设置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menu_start_page_index;
        private System.Windows.Forms.ToolStripMenuItem menu_tabpage_heighlight;
        private System.Windows.Forms.ToolStripMenuItem menu_tabpage_icon;
        private System.Windows.Forms.ToolStripMenuItem menu_confirm_dialog;
        private System.Windows.Forms.ToolStripMenuItem menu_listview_height;
        private System.Windows.Forms.ToolStripMenuItem menu_listview_gridline;
        private System.Windows.Forms.ToolStripComboBox menu_start_page_index_combox;
        private System.Windows.Forms.ToolStripComboBox menu_tabpage_heigh_light_combox;
        private System.Windows.Forms.ToolStripComboBox menu_tabpage_icon_combox;
        private System.Windows.Forms.ToolStripComboBox menu_finshined_messbox_combox;
        private System.Windows.Forms.ToolStripComboBox menu_listview_line_heigh_combox;
        private System.Windows.Forms.ToolStripComboBox menu_listview_gridline_combox;
        private System.Windows.Forms.ToolStripMenuItem 保存设置并重启软件ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 查看标签索引对照表ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripTextBox menu_chrome_path;
        private System.Windows.Forms.Button UseLibList;
        private System.Windows.Forms.TabPage TabPage_33;
        private System.Windows.Forms.GroupBox groupBox34;
        internal System.Windows.Forms.ComboBox barcodeTypeCombox;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label79;
        internal System.Windows.Forms.ComboBox barcodeAutoSizeCombox;
        private System.Windows.Forms.Label label80;
        internal System.Windows.Forms.TextBox barcodeDmiBox;
        private System.Windows.Forms.Label label81;
        internal System.Windows.Forms.TextBox barcodeRotateBox;
        private System.Windows.Forms.Label label82;
        internal System.Windows.Forms.TextBox barcodeImageHeight;
        private System.Windows.Forms.Label label83;
        private System.Windows.Forms.Label label84;
        internal System.Windows.Forms.ComboBox barcodeDpiCombox;
        internal System.Windows.Forms.ComboBox barcodeTextLocationCombox;
        private System.Windows.Forms.Label label85;
        internal System.Windows.Forms.ComboBox barcodeTextAlignCombox;
        private System.Windows.Forms.Label label86;
        private System.Windows.Forms.Label label87;
        internal System.Windows.Forms.TextBox barcodeTextContentSpaceCombox;
        private System.Windows.Forms.GroupBox groupBox35;
        private System.Windows.Forms.Label label88;
        internal System.Windows.Forms.TextBox barcodeTextContentBox;
        private System.Windows.Forms.GroupBox groupBox36;
        private System.Windows.Forms.Button GenerateBarCode;
        private System.Windows.Forms.Button SaveBarCodePreview;
        internal System.Windows.Forms.PictureBox barcodePreviewBox;
        private System.Windows.Forms.Label label89;
        internal System.Windows.Forms.TextBox barcodePaddingBox;
        private System.Windows.Forms.Label label91;
        internal System.Windows.Forms.TextBox barcodeTextFontSizeBox;
        private System.Windows.Forms.GroupBox groupBox37;
        private System.Windows.Forms.Button barcodeTemplateUpload;
        private System.Windows.Forms.Button barcodeMutilGenerate;
        internal System.Windows.Forms.DataGridView barcodeMutilList;
        private System.Windows.Forms.Button ExportBarCodeTempLate;
        private System.Windows.Forms.ToolStripMenuItem 简易二维码生成ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox38;
        internal System.Windows.Forms.TextBox barcodeCaptionBox;
        private System.Windows.Forms.Label label90;
        internal System.Windows.Forms.TextBox barcodeCaptionFontSizeBox;
        private System.Windows.Forms.Label label92;
        internal System.Windows.Forms.ComboBox barcodeCaptionLocationCombox;
        private System.Windows.Forms.Label label93;
        private System.Windows.Forms.Label label94;
        internal System.Windows.Forms.ComboBox barcodeScanTypeCombox;
        internal System.Windows.Forms.TextBox barcodeImageWidth;
        private System.Windows.Forms.Button barcodeCopyImage;
        internal System.Windows.Forms.CheckBox word2pdf_save_source;
        internal System.Windows.Forms.CheckBox pdf2word_save_source;
        internal System.Windows.Forms.CheckBox ppt2pdf_save_source;
        internal System.Windows.Forms.CheckBox pdf2ppt_save_source;
        internal System.Windows.Forms.CheckBox word2excel_save_source;
        internal System.Windows.Forms.CheckBox excel2word_save_source;
        internal System.Windows.Forms.CheckBox word2image_save_source;
        private System.Windows.Forms.TabPage TabPage_34;
        private System.Windows.Forms.ComboBox city_level_1_combox;
        private System.Windows.Forms.ComboBox city_level_2_combox;
        private System.Windows.Forms.ComboBox city_level_3_combox;
        private System.Windows.Forms.GroupBox gen_card_id_group;
        private System.Windows.Forms.Label label96;
        private System.Windows.Forms.Button gen_card_button;
        private System.Windows.Forms.GroupBox gen_card_result_group;
        private System.Windows.Forms.Label label97;
        private System.Windows.Forms.NumericUpDown gen_card_number;
        private System.Windows.Forms.CheckBox ony_gen_18_year;
        private System.Windows.Forms.RichTextBox gen_card_id_box;
        private System.Windows.Forms.Label gen_card_num_text;
        private System.Windows.Forms.Button copy_gen_card_cliboard;
        private System.Windows.Forms.ToolStripMenuItem 随机身份证号码生成ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox41;
        private System.Windows.Forms.TextBox ver_card_id_text_box;
        private System.Windows.Forms.Label label99;
        private System.Windows.Forms.GroupBox groupBox42;
        private System.Windows.Forms.Label label100;
        private System.Windows.Forms.Label label101;
        private System.Windows.Forms.Label label102;
        private System.Windows.Forms.Label ver_card_id_sign_tips;
        private System.Windows.Forms.Label ver_card_id_sex;
        private System.Windows.Forms.Label ver_card_id_birthday;
        private System.Windows.Forms.Label ver_card_id_index;
        private System.Windows.Forms.Label label103;
        private System.Windows.Forms.Button gen_card_id_name_copy;
        private System.Windows.Forms.RichTextBox gen_card_id_name_box;
        private System.Windows.Forms.CheckBox gen_card_id_name_check_box;
        private System.Windows.Forms.Label label104;
        private System.Windows.Forms.Label gen_card_id_name_text;
        private System.Windows.Forms.PictureBox barcodeRegionPicture;
        private System.Windows.Forms.Button barcodeSelectRegionPicture;
        private System.Windows.Forms.RichTextBox barcodeReaderResultbox;
        private System.Windows.Forms.Button CopyBarcodeReadResult;
        private System.Windows.Forms.Button barcodeReadStart;
        private System.Windows.Forms.TabPage TabPage_35;
        private System.Windows.Forms.Button WebpFileList_MoveDown;
        private System.Windows.Forms.Button WebpFileList_MoveUp;
        private System.Windows.Forms.Button webp2image_clear;
        private System.Windows.Forms.Button webp2image_remove;
        private System.Windows.Forms.Label webpconvert_statusText;
        private System.Windows.Forms.Button webpPictureStartConvert;
        private System.Windows.Forms.Button webpPictureSelect;
        internal System.Windows.Forms.ListView WebpFileList;
        private System.Windows.Forms.GroupBox groupBox39;
        private System.Windows.Forms.Label label98;
        private System.Windows.Forms.ComboBox webp2image_combox;
        private System.Windows.Forms.ToolStripMenuItem wEBP图片格式转换ToolStripMenuItem;
        private System.Windows.Forms.Label label95;
        private System.Windows.Forms.GroupBox groupBox40;
        private System.Windows.Forms.Button ExportNameTxt;
        private System.Windows.Forms.ComboBox votepage_type_combox;
        private System.Windows.Forms.Label label105;
        private System.Windows.Forms.ComboBox votepageIndexCombox;
        internal System.Windows.Forms.DataGridView votedataview;
        internal System.Windows.Forms.CheckBox word2pdf_need_update_fields;
        private System.Windows.Forms.TabPage TabPage_36;
        private System.Windows.Forms.Button CopiedGen;
        private System.Windows.Forms.GroupBox groupBox43;
        private System.Windows.Forms.Label label106;
        private System.Windows.Forms.Label label107;
        private System.Windows.Forms.TextBox Gen_Copied_TargetPath;
        private System.Windows.Forms.TextBox Gen_Copied_SourceFileBox;
        private System.Windows.Forms.Button Gen_Copied_Path;
        private System.Windows.Forms.Button Gen_Copied_File;
        private System.Windows.Forms.Label label108;
        private System.Windows.Forms.NumericUpDown Gen_Copied_Number;
        private System.Windows.Forms.Label Gen_Copied_StatusText;
        private System.Windows.Forms.Label label109;
        private System.Windows.Forms.TextBox Gen_Copied_Namer;
        private System.Windows.Forms.Label CopiedNamerText;
        private System.Windows.Forms.ToolStripMenuItem 文件副本生成ToolStripMenuItem;
        private System.Windows.Forms.TabPage TabPage_37;
        private System.Windows.Forms.GroupBox groupBox44;
        private System.Windows.Forms.Label label110;
        private System.Windows.Forms.Label label111;
        private System.Windows.Forms.Label label112;
        private System.Windows.Forms.Label label113;
        private System.Windows.Forms.Label label115;
        private System.Windows.Forms.Label label114;
        private System.Windows.Forms.ComboBox network_info_dns2;
        private System.Windows.Forms.ComboBox network_info_dns1;
        private System.Windows.Forms.ComboBox network_info_gateway;
        private System.Windows.Forms.ComboBox network_info_mask;
        private System.Windows.Forms.ComboBox network_info_ipv4;
        private System.Windows.Forms.ComboBox network_info_card_list;
        private System.Windows.Forms.GroupBox groupBox45;
        private System.Windows.Forms.ComboBox set_interface_name_list;
        private System.Windows.Forms.Label label116;
        private System.Windows.Forms.Label label121;
        private System.Windows.Forms.Label label120;
        private System.Windows.Forms.Label label119;
        private System.Windows.Forms.Label label118;
        private System.Windows.Forms.Label label117;
        private System.Windows.Forms.RadioButton set_interface_mode_static;
        private System.Windows.Forms.RadioButton set_interface_mode_auto;
        private IPAddressInputTextBox set_interface_dns1;
        private IPAddressInputTextBox set_interface_gateway;
        private IPAddressInputTextBox set_interface_mask;
        private IPAddressInputTextBox set_interface_ipaddress;
        private IPAddressInputTextBox set_interface_dns2;
        private System.Windows.Forms.Button set_interface_start;
        private System.Windows.Forms.Button set_interface_reset;
        private System.Windows.Forms.Button Disable_NetworkCard;
        private System.Windows.Forms.Button Enable_NetworkCard;
        private System.Windows.Forms.CheckBox network_info_show_normal;
        private System.Windows.Forms.Button update_network_info;
        private System.Windows.Forms.Button network_open_ncap;
        private System.Windows.Forms.ComboBox network_info_mac_address;
        private System.Windows.Forms.Label label122;
        private System.Windows.Forms.Button copy_network_info_mac_address;
        private System.Windows.Forms.Label label123;
        private System.Windows.Forms.ToolStripMenuItem 网卡设置ToolStripMenuItem;
        private System.Windows.Forms.Button copy_network_info_ip_address;
        private System.Windows.Forms.Label label124;
        private System.Windows.Forms.TextBox copied_number_prefix_count;
        private System.Windows.Forms.Button set_interface_export;
        private System.Windows.Forms.Button set_interface_load;
        private System.Windows.Forms.Label label125;
        internal System.Windows.Forms.ComboBox word2pdf_colorMode;
        private System.Windows.Forms.Label label126;
        private System.Windows.Forms.Label label127;
        private System.Windows.Forms.Label label128;
        private System.Windows.Forms.Label label129;
        internal System.Windows.Forms.NumericUpDown html2pdf_timeout;
        internal System.Windows.Forms.NumericUpDown html2pdf_wait_timeout;
        internal System.Windows.Forms.CheckBox html2pdf_wait_all_resource;
        private System.Windows.Forms.LinkLabel linkLabel4;
        private System.Windows.Forms.GroupBox groupBox46;
        private System.Windows.Forms.Button copy_money_big_result;
        private System.Windows.Forms.Label label130;
        private System.Windows.Forms.Label label131;
        private System.Windows.Forms.TextBox money_small;
        private System.Windows.Forms.TextBox money_big;
        private System.Windows.Forms.Button DontateMe;
        private System.Windows.Forms.Button DisableAdminBehavior;
        private System.Windows.Forms.ToolStripMenuItem 移除PDF水印ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 添加PDF水印ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pDF版本转换ToolStripMenuItem;
        internal System.Windows.Forms.CheckBox EmbedFonts;
        private System.Windows.Forms.ToolStripMenuItem 界面大小设置ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 设置界面宽度ToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox bodyform_width;
        private System.Windows.Forms.ToolStripMenuItem 设置界面高度ToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox bodyform_height;
        private System.Windows.Forms.GroupBox groupBox47;
        private System.Windows.Forms.Label label132;
        private System.Windows.Forms.Label label133;
        private System.Windows.Forms.Label label134;
        internal System.Windows.Forms.TextBox SplitPdfRange_Start;
        internal System.Windows.Forms.TextBox SplitPdfRange_End;
        internal System.Windows.Forms.RadioButton SplitPdf_Single;
        internal System.Windows.Forms.RadioButton SplitPdf_Range;
        private System.Windows.Forms.CheckBox MarkDupName;
        private System.Windows.Forms.ComboBox NameMarkColor;
        private System.Windows.Forms.ToolStripMenuItem 初始化配置文件ToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox48;
        internal System.Windows.Forms.RadioButton SplitWord_Single;
        private System.Windows.Forms.Label label135;
        internal System.Windows.Forms.TextBox SplitWord_Range_End;
        private System.Windows.Forms.Label label136;
        internal System.Windows.Forms.TextBox SplitWord_Range_Start;
        private System.Windows.Forms.Label label137;
        internal System.Windows.Forms.RadioButton SplitWord_Range;
        private System.Windows.Forms.TabPage TabPage_38;
        private System.Windows.Forms.Button VoteTable_Render;
        private System.Windows.Forms.GroupBox groupBox49;
        private System.Windows.Forms.TextBox VoteTable_Name;
        private System.Windows.Forms.Label label138;
        private System.Windows.Forms.TextBox VoteTable_UnAgree;
        private System.Windows.Forms.Label label140;
        private System.Windows.Forms.TextBox VoteTable_Agree;
        private System.Windows.Forms.Label label139;
        private System.Windows.Forms.TextBox VoteTable_Abdon;
        private System.Windows.Forms.Label label141;
        private System.Windows.Forms.GroupBox groupBox50;
        private System.Windows.Forms.RichTextBox VoteTable_Names;
        private System.Windows.Forms.GroupBox groupBox51;
        private System.Windows.Forms.TextBox VoteTable_Title;
        private System.Windows.Forms.GroupBox groupBox52;
        private System.Windows.Forms.TextBox VoteTable_Start;
        private System.Windows.Forms.GroupBox 报告单结束语;
        private System.Windows.Forms.TextBox VoteTable_End;
        private System.Windows.Forms.GroupBox 报告单生成日期;
        private System.Windows.Forms.TextBox VoteTable_GenDate;
        private System.Windows.Forms.Button VoteTableHelpDate;
        private System.Windows.Forms.Label label142;
        private System.Windows.Forms.ToolStripMenuItem 选举报告单样式生成ToolStripMenuItem;
        private System.Windows.Forms.CheckBox VoteTable_Random_PageCounts;
        private System.Windows.Forms.GroupBox groupBox53;
        private System.Windows.Forms.Label label143;
        private System.Windows.Forms.Label label144;
        internal System.Windows.Forms.NumericUpDown SplitPdf_PerPageCount;
        private System.Windows.Forms.Label label145;
        private System.Windows.Forms.Label label146;
        internal System.Windows.Forms.NumericUpDown SplitWord_PageCount;
        private System.Windows.Forms.Label VoteTableName_CountText;
        private System.Windows.Forms.Label label147;
        internal System.Windows.Forms.TextBox WORD2PDF_LoadPassword;
        private System.Windows.Forms.Label label148;
        internal System.Windows.Forms.TextBox PDF2WORD_LoadPassword;
        private System.Windows.Forms.Label label149;
        private System.Windows.Forms.NumericUpDown VoteTableHeight;
        private System.Windows.Forms.Label label150;
        private System.Windows.Forms.Label label151;
        private System.Windows.Forms.NumericUpDown VoteTableFontSize;
        private System.Windows.Forms.CheckBox VoteTable_KeepPageFooter;
        private System.Windows.Forms.CheckBox VoteTable_NumberIndex_Keep;
        internal System.Windows.Forms.RadioButton SplitPdf_ByBookMark;
        private System.Windows.Forms.TabPage TabPage_39;
        private System.Windows.Forms.GroupBox groupBox54;
        private System.Windows.Forms.Button SignPersionDocumentChooseFile;
        private System.Windows.Forms.TextBox SignPersionDocument;
        private System.Windows.Forms.Label label152;
        private System.Windows.Forms.GroupBox blockgroup;
        private System.Windows.Forms.Label label153;
        private System.Windows.Forms.ComboBox PauseUpdateYears;
        private System.Windows.Forms.Button SetPauseTime;
        private System.Windows.Forms.Button OpenSystemUpdate;
        private System.Windows.Forms.TabPage TabPage_40;
        internal System.Windows.Forms.ListView VFSOFT_Report_ListView;
        private System.Windows.Forms.Button RefreshVFSOFTREPORT;
        private System.Windows.Forms.Label label154;
        private System.Windows.Forms.Button SelectExamFile;
        private System.Windows.Forms.TextBox ExamFileText;
        private System.Windows.Forms.ComboBox ExamItemsCombox;
        private System.Windows.Forms.ComboBox ExamItemsSelectorsCombox;
        private System.Windows.Forms.GroupBox groupBox56;
        private System.Windows.Forms.Label label156;
        private System.Windows.Forms.Label label155;
        private System.Windows.Forms.GroupBox groupBox57;
        private System.Windows.Forms.Label label157;
        private System.Windows.Forms.ComboBox ExamRaidInfoCombox;
        private System.Windows.Forms.Label label158;
        private System.Windows.Forms.ComboBox ExamReportDataFormat;
        private System.Windows.Forms.Button ExamReportChange;
        private System.Windows.Forms.TextBox SingPersonImageAlt;
        private System.Windows.Forms.Label label159;
        private System.Windows.Forms.GroupBox SingleDocumentConfigGroupBox;
        private System.Windows.Forms.Label label160;
        private System.Windows.Forms.TextBox SignDocumentFileSaveBox;
        private System.Windows.Forms.Label label161;
        private System.Windows.Forms.Button SignDocumentFileSaveButton;
        private System.Windows.Forms.Button SignDocumentReplaceStart;
        private System.Windows.Forms.NumericUpDown SignDocumentNeedCount;
        private System.Windows.Forms.GroupBox groupBox55;
        private System.Windows.Forms.RadioButton SignDocumentUseEmbedImage;
        private System.Windows.Forms.RadioButton SignDocumentUseOutPersonPath;
        private System.Windows.Forms.Button SignDocumentUseOutPersonPathButton;
        private System.Windows.Forms.TextBox SignDocumentUseOutPersonPathTextBox;
        private System.Windows.Forms.RadioButton SignDocumentOnlyUseOnlyImage;
        private System.Windows.Forms.Button SignDocumentOnlyUseOnlyImageButton;
        private System.Windows.Forms.TextBox SignDocumentOnlyUseOnlyImageTextBox;
        private System.Windows.Forms.GroupBox groupBox58;
        private System.Windows.Forms.ProgressBar SignDocumentProgress;
        private System.Windows.Forms.Label label162;
        private System.Windows.Forms.TextBox SignDocumentFileEndName;
        private System.Windows.Forms.Label label164;
        private System.Windows.Forms.Label label165;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.Label label166;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.Label label167;
        private System.Windows.Forms.GroupBox groupBox59;
        private System.Windows.Forms.Label label168;
        private System.Windows.Forms.ComboBox ExamReportItems;
        private System.Windows.Forms.Label label169;
        private System.Windows.Forms.ComboBox ExamReportRaids;
        private System.Windows.Forms.Label label172;
        private System.Windows.Forms.Label label171;
        private System.Windows.Forms.Label label170;
        private System.Windows.Forms.ComboBox ExamReplaceDataFormat;
        private System.Windows.Forms.TextBox ExamReplaceItemName;
        private System.Windows.Forms.TextBox ExamReplaceRaids;
        private System.Windows.Forms.ProgressBar ExamReportChangeProgress;
        private System.Windows.Forms.Label label173;
        private ToolStripMenuItem 单人表生成ToolStripMenuItem;
        private ToolStripMenuItem 考评报表修改ToolStripMenuItem;
        private Button ExamRaidsCopy;
        private GroupBox groupBox28;
        private Label label5;
        private Button ChooseExamFileButton;
        private TextBox ChooseExamFileTextBox;
        private Label label58;
        private ComboBox ExamComputerNames;
        private Button ExamComputerNameChange;
        private TextBox ExamComputerChangeName;
        private Label label174;
        private Button GetRandomComputerNameButton;
        private Label label175;
        private NumericUpDown GetRandomComputerNameRandLength;
        internal CheckBox SplitWordReUpdatePageNumber;
        private RadioButton ExamReportChangeItemDataFormat;
        private RadioButton ExamReportChangeItemName;
        private RadioButton ExamReportChangeItemDataRaid;
        private LinkLabel linkLabel5;
        private LinkLabel linkLabel6;
        private PictureBox pictureBox4;
        private PictureBox pictureBox3;
        private PictureBox pictureBox2;
        private PictureBox pictureBox9;
        private GroupBox groupBox60;
        private RichTextBox NumberIndexResultBox;
        private Button CopyNumberIndexResult;
        private NumericUpDown NumberIndexEnd;
        private NumericUpDown NumberIndexStart;
        private Button GetNumberIndexList;
        private TabPage TabPage_41;
        private ToolStripMenuItem 拆分PPTToolStripMenuItem;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
        private GroupBox groupBox63;
        private Button ReleaseKMSAIO;
        private GroupBox groupBox64;
        private Button ReleaseRenamerReplaceConfig;
        private Button ReleaseRenamerApp;
        private GroupBox groupBox61;
        internal RadioButton SplitPPT_SignleExport;
        private Label label179;
        internal TextBox SplitPPT_RangeExport_End;
        private Label label180;
        internal TextBox SplitPPT_RangeExport_Start;
        private Label label181;
        internal RadioButton radioButton2;
        private Button SplitPPTFilesList_MoveDown;
        private Button SplitPPTFilesList_MoveUp;
        private Button splitppt_clear;
        private Button splitppt_remove;
        private Label SplitPPTStatusText;
        private Button SplitPPT_Button;
        private Button SplitPPT_SelectFiles;
        internal ListView SplitPPTFilesList;
        private GroupBox groupBox62;
        internal ComboBox SplitPdfSizeCombox;
        internal ComboBox SplitPdfScaleCombox;
        private Label label177;
        private Label label178;
        private RadioButton SignDocumentUseOnlyNumber;
        private RadioButton SignDocumentUsePersonNumber;
        private RadioButton SignDocumentUseOnlyPageNumber;
        private Label label163;
        private Label label37;
        private GroupBox SignDocumentPicSourceGroupBox;
        private ToolStripMenuItem 控件锚点模式ToolStripMenuItem;
        private ToolStripComboBox menu_widget_anchor_combox;
        private CheckBox VoteTable_AnotherTableShow;
        private ToolStripMenuItem 折叠本节点全部内容ToolStripMenuItem;
        private ToolStripMenuItem 折叠全部节点ToolStripMenuItem;
        private ToolStripMenuItem vFSOFT数据包路径配置ToolStripMenuItem;
        private ToolStripMenuItem 考评数据包路径ToolStripMenuItem;
        private ToolStripTextBox vfsoft_examdata_path_textbox;
        private ToolStripMenuItem 选举数据包路径ToolStripMenuItem;
        private ToolStripTextBox vfsoft_mssdata_path_textbox;
        private Button TwainSetDeviceSaveTime;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel MemoryStatusLabel;
        private ToolStripProgressBar MemoryProgressBar;
        private TextBox ScanSerialNumberTextBox;
        private ToolStripStatusLabel UpdateCheckText;
        private ToolStripMenuItem 程序更新ToolStripMenuItem;
        private ToolStripMenuItem CheckVersionButton;
        private ToolStripMenuItem VersionUpdateLog;
        internal CheckBox UseLocalOffice;
        private CheckBox DoubleTextClear;
    }
}

