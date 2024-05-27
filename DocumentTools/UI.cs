using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
//using System.Windows.Controls;
using System.Windows.Forms;

namespace DocumentTools
{
    internal class UI
    {
        public static void AddCompressImageFiles()
        { AddFilesToListViewAsync("PDF文档", "*.jpg;*.jpeg;*.png;*.bmp;*.svg", BodyForm.MainForm.CompressImageFileList); }

        public static void AddCompressPDFFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.CompressPdfFileList); }

        public static void AddConvertComplicePdfFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.ConvertPdfComplianceFileList); }

        public static void AddConvertImage2PDFFiles()
        { AddFilesToListViewAsync("图片文件", "*.jpg;*.jpeg;*.png;*.bmp", BodyForm.MainForm.ConvertImage2PDFFileList); }

        public static void AddDataMatrixWordFiles()
        { AddFilesToListViewAsync("WORD文档|PDF文档", "*.doc;*.docx;*.pdf", BodyForm.MainForm.DecodeMatrixWordListView); }

        public static void AddEPUB2PDFFiles()
        { AddFilesToListViewAsync("EPUB文件", "*.epub", BodyForm.MainForm.EPUB2PDFFileList); }

        public static void AddEPUB2WORDFiles()
        { AddFilesToListViewAsync("EPUB文件", "*.epub", BodyForm.MainForm.EPUB2WORDFileList); }

        public static void AddExcel2WordFiles()
        { AddFilesToListViewAsync("EXCEL文档", "*.xls;*.xlsx", BodyForm.MainForm.Excel2WordFileList); }

        public static void AddExcelFiles()
        { AddFilesToListViewAsync("EXCEL文档", "*.xls;*.xlsx;*.et", BodyForm.MainForm.ExcelFileListView); }

        public static void AddExcelReplaceFiles()
        { AddFilesToListViewAsync("EXCEL文档", "*.xls;*.xlsx", BodyForm.MainForm.ReplaceExcelFileView); }

        /// <summary>
        /// 添加文件列表到Listview中，并返回项目数量
        /// </summary>
        /// <param name="DisplayFilterString"></param>
        /// <param name="FilterArgs"></param>
        /// <param name="ListViewControl"></param>
        /// <returns>文件数量</returns>
        public static Task<int> AddFilesToListViewAsync(
            string DisplayFilterString,
            string FilterArgs,
            ListView ListViewControl)
        {
            return Task.Run(
                () =>
                {
                    IEnumerable<string> TempDocuemntFileNames = Util.ShowSelectDialog(
                        true,
                        false,
                        DisplayFilterString,
                        FilterArgs);
                    //TempDocuemntFileNames = TempDocuemntFileNames.OrderBy(x => x, new FileNameComparerInstance());
                    List<ListViewItem> itemsToAdd = new List<ListViewItem>(); // 保存待添加的 ListViewItems
                    try
                    {
                        foreach(string TempDocumentFileName in TempDocuemntFileNames)
                        {
                            ListViewItem TempDocuemntFileNamesListView = new ListViewItem
                            {
                                Text = TempDocumentFileName
                            };
                            TempDocuemntFileNamesListView.SubItems.Add(Util.GetNowDateTime());
                            itemsToAdd.Add(TempDocuemntFileNamesListView); // 将待添加的项保存起来
                        }

                        // 在 Task.Run() 内部执行 ListView 的更新
                        ListViewControl.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    if(itemsToAdd.Count > 0)
                                    {
                                        ListViewControl.BeginUpdate();
                                        ListViewControl.Items.AddRange(itemsToAdd.ToArray());
                                        // 仅调用一次AutoResizeColumns以提高效率
                                        ListViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                        ListViewControl.EndUpdate();
                                        //ListViewControl.ListViewItemSorter = new ListViewItemComparer_ASC();
                                    }
                                }));
                    } catch(Exception)
                    {
                    }
                    return itemsToAdd.Count;
                });
        }

        /// <summary>
        /// 添加文件到listview中，主要是配合drag事件使用
        /// </summary>
        /// <param name="FileList"></param>
        /// <param name="ListviewControl"></param>
        /// <param name="FileFilters"></param>
        public static void AddFilesToListViewWithNotDialog(
            string[] FileList,
            ListView ListviewControl,
            string[] FileFilters)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        if(FileList.Length > 0)
                        {
                            FileList = FileList.OrderBy(x => x, new FileNameComparerInstance()).ToArray();

                            List<ListViewItem> itemsToAdd = new List<ListViewItem>(); // 保存待添加的 ListViewItems

                            foreach(string file in FileList)
                            {
                                FileInfo TempFileInfo = new FileInfo(file);
                                DirectoryInfo TempDirInfo = new DirectoryInfo(file);
                                if(Directory.Exists(file) &&
                                    (TempDirInfo.Attributes & FileAttributes.ReparsePoint) !=
                                    FileAttributes.ReparsePoint)
                                {
                                    //通过递归实现获取一层目录文件
                                    string[] PathFiles = Directory.GetFiles(file);
                                    AddFilesToListViewWithNotDialog(PathFiles, ListviewControl, FileFilters);

                                    //通过递归实现获取子目录文件
                                    string[] SubPathFiles = Directory.GetDirectories(file);
                                    AddFilesToListViewWithNotDialog(SubPathFiles, ListviewControl, FileFilters);
                                } else if(File.Exists(file) &&
                                    (TempFileInfo.Attributes & FileAttributes.ReparsePoint) !=
                                    FileAttributes.ReparsePoint)
                                {
                                    foreach(string FileFilter in FileFilters)
                                    {
                                        if(string.Compare(Path.GetExtension(file), FileFilter, StringComparison.Ordinal) ==
                                            0)
                                        {
                                            ListViewItem FileListListView = new ListViewItem { Text = file };
                                            FileListListView.SubItems.Add(Util.GetNowDateTime());
                                            itemsToAdd.Add(FileListListView); // 将待添加的项保存起来
                                        }
                                    }
                                }
                            }

                            // 在 Task.Run() 内部使用 BeginInvoke() 执行 ListView 的更新
                            ListviewControl.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        if(itemsToAdd.Count > 0)
                                        {
                                            ListviewControl.BeginUpdate();
                                            ListviewControl.Items.AddRange(itemsToAdd.ToArray());
                                            // 仅调用一次AutoResizeColumns以提高效率
                                            ListviewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                            ListviewControl.EndUpdate();
                                            //ListviewControl.ListViewItemSorter = new ListViewItemComparer_ASC();
                                        }
                                    }));
                        }
                    } catch(Exception)
                    {
                    }
                });
        }

        public static void AddHtml2PdfFiles()
        { AddFilesToListViewAsync("HTML文档", "*.html;*.htm;*.mhtml;*.mht", BodyForm.MainForm.HTML2PDFFileList); }

        public static void AddIcoConvertFiles()
        { AddFilesToListViewAsync("图片文件", "*.jpg;*.png;*.bmp", BodyForm.MainForm.IcoFileView); }

        public static void AddMergePdfFiles()
        { AddFilesToListViewAsync("支持的文件", "*.pdf;*.jpg;*.png;*.bmp;*.tif", BodyForm.MainForm.MergePdfFileView); }

        public static void AddMergeWordFiles()
        { AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", BodyForm.MainForm.MergeWordFileList); }

        public static void AddPdf2ExcelFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.PDF2ExcelFileView); }

        public static void AddPdf2PPTFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.PDF2PPTFileList); }

        public static void AddPdfFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.PdfFileListView); }

        public static void AddPPT2PdfFiles()
        { AddFilesToListViewAsync("演示文稿", "*.ppt;*.pptx", BodyForm.MainForm.PPT2PdfFileList); }

        public static void AddRemoveWaterFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.RemovePdfWaterMarkFileList); }

        public static void AddSplitPdfFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.SplitPdfFileList); }

        public static void AddSplitWordFiles()
        { AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", BodyForm.MainForm.SplitWordFileList); }

        public static void AddWaterMarkPdfFiles()
        { AddFilesToListViewAsync("PDF文档", "*.pdf", BodyForm.MainForm.PdfWaterMarkFileList); }
        public static void AddWeBpFiles()
        { AddFilesToListViewAsync("WEBP图片", "*.webp", BodyForm.MainForm.WebpFileList); }

        public static void AddWord2EPUBFiles()
        { AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", BodyForm.MainForm.Word2EPUBFileList); }

        public static void AddWord2ExcelFiles()
        { AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", BodyForm.MainForm.Word2ExcelFileList); }

        public static void AddWord2ImageFiles()
        { AddFilesToListViewAsync("支持的文档", "*.docx;*.doc;*.xls;*.xlsx;*.pdf", BodyForm.MainForm.DocumentsListView); }

        public static Task<int> AddWordFilesAsync()
        { return AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", BodyForm.MainForm.WordFileListView); }

        public static void AddWordReplaceFiles()
        { AddFilesToListViewAsync("WORD文档", "*.docx;*.doc;*.wps", BodyForm.MainForm.ReplaceWordFileList); }

        public static void AddSplitPPTFiles()
        { AddFilesToListViewAsync("演示文稿", "*.ppt;*.pptx", BodyForm.MainForm.SplitPPTFilesList); }


        /// <summary>
        /// 转换文件通用方法
        /// </summary>
        /// <param name="ListViewControl">转换文件所在的listview控件名称</param>
        /// <param name="StatusLabelControl">用于表示进度状态的标签控件</param>
        /// <param name="ConverType">转换的目标类型</param>
        /// <param name="ExtensionString">保存文件的后缀名称</param>
        public static void ConvertStart(
            ListView ListViewControl,
            Label StatusLabelControl,
            ConvertFileType ConverType,
            string ExtensionString,
            bool UseSourcePath = false,
            Button CallButton = null)
        {
            if(ListViewControl.Items.Count > 0)
            {
                string TempSaveDocxPathString = string.Empty;
                string SelectSavePathstring = "A";
                if(!UseSourcePath)
                {
                    IEnumerable<string> ISelectSavePathstring = Util.ShowSelectDialog(
                        false,
                        true,
                        string.Empty,
                        string.Empty);//调用文件选择窗口，获取文件夹名称
                    SelectSavePathstring = ISelectSavePathstring.FirstOrDefault();
                }

                if(string.Compare(SelectSavePathstring, default, StringComparison.Ordinal) != 0 &&
                    SelectSavePathstring != string.Empty &&
                    SelectSavePathstring != null)
                {
                    CallButton.Enabled = false;
                    for(int TempIndex = 0; TempIndex < ListViewControl.Items.Count; TempIndex++)
                    {
                        string ListViewFirstColumnText = ListViewControl.Items[TempIndex].Text;
                        string FileNameWithOutExtension = Path.GetFileNameWithoutExtension(ListViewFirstColumnText);//不带后缀的文件名
                        string FileNameSourcePath = Path.GetDirectoryName(ListViewFirstColumnText);//所选文件列表中文件的源目录
                        if(UseSourcePath)
                        {
                            SelectSavePathstring = FileNameSourcePath;
                        }
                        string ConvertedFileName = Path.Combine(
                            SelectSavePathstring,
                            $"{FileNameWithOutExtension}{ExtensionString}");
                        StatusLabelControl.Text = $"正在处理第{TempIndex + 1}个文件";

                        //根据不同类型调用不同的函数
                        switch(ConverType)
                        {
                            //word转pdf
                            case ConvertFileType.Word2Pdf:
                                TempSaveDocxPathString = Path.Combine(SelectSavePathstring, "TempDocx");
                                Directory.CreateDirectory(TempSaveDocxPathString);

                                object SelectedPdfCompliance = Enum.Parse(
                                    typeof(Aspose.Words.Saving.PdfCompliance),
                                    BodyForm.MainForm.PDFComplianceBox.SelectedItem.ToString());

                                Util.ConvertWord2Pdf(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    TempSaveDocxPathString,
                                    (Aspose.Words.Saving.PdfCompliance)SelectedPdfCompliance,
                                    (Aspose.Words.Settings.MsWordVersion)Enum.Parse(
                                        typeof(Aspose.Words.Settings.MsWordVersion),
                                        BodyForm.MainForm.Word2PDFOptimizCombox.SelectedItem.ToString()),
                                    BodyForm.MainForm.word2pdf_need_update_fields.Checked,
                                    (Aspose.Words.Saving.ColorMode)Enum.Parse(
                                        typeof(Aspose.Words.Saving.ColorMode),
                                        BodyForm.MainForm.word2pdf_colorMode.SelectedValue.ToString()),
                                    BodyForm.MainForm.EmbedFonts.Checked,
                                    BodyForm.MainForm.WORD2PDF_LoadPassword.Text,
                                    BodyForm.MainForm.UseLocalOffice.Checked);
                                break;
                            //pdf转word
                            case ConvertFileType.Pdf2Word:
                                Util.ConvertPdf2Word(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.PDF2WORD_UseBullet.Checked,
                                    BodyForm.MainForm.PDF2WORD_UseSUbOcr.Checked,
                                    (Aspose.Pdf.DocSaveOptions.RecognitionMode)Enum.Parse(
                                        typeof(Aspose.Pdf.DocSaveOptions.RecognitionMode),
                                        BodyForm.MainForm.PDF2WORD_Mode.SelectedValue.ToString()),
                                    BodyForm.MainForm.PDF2WORD_LoadPassword.Text);
                                break;
                            //excel转pdf
                            case ConvertFileType.Excel2Pdf:
                                Util.ConvertExcel2Pdf(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.ExportEachSheet.Checked,
                                    BodyForm.MainForm.Excel2Pdf_ForceContentOnOnePage.Checked,
                                    BodyForm.MainForm.Excel2Pdf_HorPrint.Checked,
                                    BodyForm.MainForm.Excel2Pdf_VerPrint.Checked,
                                    int.Parse(string.IsNullOrEmpty(BodyForm.MainForm.Excel2Pdf_PrintScale_Row.Text) ? "0": BodyForm.MainForm.Excel2Pdf_PrintScale_Row.Text),
                                    int.Parse(string.IsNullOrEmpty(BodyForm.MainForm.Excel2Pdf_PrintScale_Col.Text) ? "0": BodyForm.MainForm.Excel2Pdf_PrintScale_Col.Text),
                                    BodyForm.MainForm.Excel2Pdf_PrintScalePercent_Enable.Checked,
                                    int.Parse(BodyForm.MainForm.Excel2Pdf_PrintScalePercent_Number.Value.ToString())
                                    );
                                break;
                            //word转图片
                            case ConvertFileType.Word2Image:
                                Util.ConvertWord2Image(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    ExtensionString,
                                    BodyForm.MainForm.SetGrayCheckBox.Checked,
                                    BodyForm.MainForm.PageExportCheck.Checked,
                                    Convert.ToInt32(BodyForm.MainForm.DpiItemList.SelectedValue));
                                break;
                            //pdf转excel
                            case ConvertFileType.Pdf2Excel:
                                Util.ConvertPdf2Excel(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.ExcelSheelMode.Checked);
                                break;
                            //批量替换word内容
                            case ConvertFileType.WordReplace:
                                if(BodyForm.MainForm.WordReplaceFindTextBox.Text != string.Empty)
                                {
                                    List<(string, string)> TempFindReplaceList = new List<(string, string)>();

                                    List<TextBox> FindTextBoxes = BodyForm.MainForm.WordReplaceFindPanel.Controls
                                        .OfType<TextBox>()
                                        .Where(x=>!string.IsNullOrEmpty(x.Text))
                                        .ToList();
                                    
                                    List<TextBox> ReplaceTextBoxes = BodyForm.MainForm. WordReplaceReplacePanel.Controls
                                        .OfType<TextBox>()
                                        .Where (x=>!string.IsNullOrEmpty(x.Text))
                                        .ToList() ;

                                    for(int i=0;i<Math.Min(FindTextBoxes.Count,ReplaceTextBoxes.Count);i++)
                                    {
                                        TextBox TempFindTextBox = FindTextBoxes[i];
                                        TextBox TempReplaceTextBox = ReplaceTextBoxes[i];

                                        TempFindReplaceList.Add((TempFindTextBox.Text, TempReplaceTextBox.Text));
                                    }

                                    Util.ReplaceDocumentContent(
                                       DocumentsFileName:ListViewFirstColumnText,
                                       SavedDoucmentFileName:ConvertedFileName,
                                       RelaceDocumentType:"WORD",
                                       FindReplaceTupleList:TempFindReplaceList,
                                       IgnoreFootText:BodyForm.MainForm.IgnoreFootContent.Checked,
                                       NoIgnoreCase:BodyForm.MainForm.IgnoreCase.Checked,
                                       replace_word_fontName:ReplaceWordFontSetting.FontName,
                                       replace_word_fontSize:ReplaceWordFontSetting.FontSize,
                                       WordReplace_FindWholeWord:BodyForm.MainForm.WordReplace_MatchWholeWord.Checked,
                                       WordReplace_IgnoreDelete:BodyForm.MainForm.WordReplace_IgnoreDelete.Checked,
                                       WordReplace_IgnoreFiledCodes:BodyForm.MainForm.WordReplace_IgnoreFiledCodes.Checked,
                                       WordReplace_IgnoreFileds:BodyForm.MainForm.WordReplace_IgnoreFileds.Checked,
                                       WordReplace_IgnoreInserted:BodyForm.MainForm.WordReplace_IgnoreInsert.Checked,
                                       WordReplace_IgnoreShapes:BodyForm.MainForm.WordReplace_IgnoreShapes.Checked,
                                       UseRegexSub:BodyForm.MainForm.WordReplace_UseRegextSub.Checked);
                                }
                                break;
                            //批量替换Excel内容
                            case ConvertFileType.ExcelReplace:

                                List<(string, string)> TempFindReplaceList_Excel = new List<(string, string)>();

                                List<TextBox> FindTextBoxes_Excel = BodyForm.MainForm.ExcelFindReplace_FindPanel.Controls
                                    .OfType<TextBox>()
                                    .Where(x => !string.IsNullOrEmpty(x.Text))
                                    .ToList();

                                List<TextBox> ReplaceTextBoxes_Excel = BodyForm.MainForm.ExcelFindReplace_ReplacePanel.Controls
                                    .OfType<TextBox>()
                                    .Where(x => !string.IsNullOrEmpty(x.Text))
                                    .ToList();

                                for (int i = 0; i < Math.Min(FindTextBoxes_Excel.Count, ReplaceTextBoxes_Excel.Count); i++)
                                {
                                    TextBox TempFindTextBox = FindTextBoxes_Excel[i];
                                    TextBox TempReplaceTextBox = ReplaceTextBoxes_Excel[i];

                                    TempFindReplaceList_Excel.Add((TempFindTextBox.Text, TempReplaceTextBox.Text));
                                }

                                Util.ReplaceDocumentContent(
                                    DocumentsFileName:ListViewFirstColumnText,
                                    SavedDoucmentFileName:ConvertedFileName,
                                    RelaceDocumentType:"EXCEL",
                                    FindReplaceTupleList: TempFindReplaceList_Excel,
                                    IgnoreFootText:false,
                                    NoIgnoreCase:BodyForm.MainForm.ExcelNoIgnoreCase.Checked,
                                    RemoveRowMode:BodyForm.MainForm.RemoveRows.Checked,
                                    ExcelStartRow:Convert.ToInt32(BodyForm.MainForm.ExcelStartRow.Text),
                                    ExcelEndRow:Convert.ToInt32(BodyForm.MainForm.ExcelEndRow.Text),
                                    UseRegex:BodyForm.MainForm.UseRegex.Checked,
                                    ExcelReplace_MatchWholeWord:BodyForm.MainForm.ExcelReplace_MatchWholeWord.Checked);
                                break;
                            // 批量图片转为ICO
                            case ConvertFileType.Image2Ico:
                                Util.ConvertImage2Ico(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    Convert.ToInt32(BodyForm.MainForm.IcoSizeDrop.SelectedValue));
                                break;

                            // Word批量转换为Excel
                            case ConvertFileType.Word2Excel:
                                Util.ConvertWord2Excel(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            // Excel批量转换为Word
                            case ConvertFileType.Excel2Word:
                                Util.ConvertExcel2Word(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            // pdf拆分
                            case ConvertFileType.SplitPdf:
                                Util.SplitPdf(
                                    ListViewFirstColumnText,
                                    SelectSavePathstring,
                                    BodyForm.MainForm.SplitPdf_Range.Checked,
                                    BodyForm.MainForm.SplitPdfRange_Start.Text.Trim(),
                                    BodyForm.MainForm.SplitPdfRange_End.Text.Trim(),
                                    int.Parse(BodyForm.MainForm.SplitPdf_PerPageCount.Value.ToString()),
                                    BodyForm.MainForm.SplitPdf_ByBookMark.Checked,
                                    BodyForm.MainForm.SplitPdf_Single.Checked);
                                break;
                            // pdf添加水印
                            case ConvertFileType.AddWaterMarkPdf:
                                Util.AddWaterMark2Pdf(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.PdfWaterMarkText.Text,
                                    float.Parse(BodyForm.MainForm.PdfWaterMarkOpcityNumber.Text.ToString()),
                                    BodyForm.MainForm.PdfWaterMarkRotateDegNumber.Text,
                                    BodyForm.MainForm.PdfWaterMarkHorSetting.SelectedIndex,
                                    BodyForm.MainForm.PDFWaterMarkVerSetting.SelectedIndex,
                                    PrivateFontSetting.FontColor,
                                    PrivateFontSetting.FontName,
                                    PrivateFontSetting.FontSize,
                                    BodyForm.MainForm.AddWaterMarkBackGroundImageFileName,
                                    BodyForm.MainForm.WaterMarkImageHeight.Text,
                                    BodyForm.MainForm.WaterMarkImageWidth.Text);
                                break;
                            // pdf移除水印
                            case ConvertFileType.RemovePdfWaterMark:
                                Util.RemovePdfWaterMark(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            // ppt转pdf
                            case ConvertFileType.PPT2PDF:
                                object SelectCommentPostion = Enum.Parse(
                                    typeof(Aspose.Slides.Export.NotesPositions),
                                    BodyForm.MainForm.ExportNotePageCombox.SelectedValue.ToString());
                                Util.ConvertPPT2PDF(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.DrawBlackLine.Checked,
                                    BodyForm.MainForm.ConvertPPT2PDFPassword.Text.Trim(),
                                    BodyForm.MainForm.ExportHiddenPage.Checked,
                                    (Aspose.Slides.Export.NotesPositions)SelectCommentPostion);
                                break;
                            // pdf转ppt
                            case ConvertFileType.PDF2PPT:
                                Util.ConvertPDF2PPT(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.ExtraOcrBox.Checked,
                                    BodyForm.MainForm.SplitShapeBox.Checked,
                                    BodyForm.MainForm.RenderImageBox.Checked);
                                break;
                            // pdf版本转换
                            case ConvertFileType.ConvertPdfCompliance:
                                object SelectPdfCompliance = Enum.Parse(
                                    typeof(Aspose.Pdf.PdfFormat),
                                    BodyForm.MainForm.ConvertPdfComplianceVersion.SelectedItem.ToString());
                                Util.ConvertPdfCompliance(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    (Aspose.Pdf.PdfFormat)SelectPdfCompliance);
                                break;
                            // 压缩PDF
                            case ConvertFileType.CompressPDF:
                                Util.CompressPDF(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.CompressPDF_WEB.Checked,
                                    BodyForm.MainForm.CompressPDF_CompressImage.Checked,
                                    int.Parse(BodyForm.MainForm.CompressPDF_ImageQuailty.SelectedItem.ToString()),
                                    int.Parse(BodyForm.MainForm.CompressPDF_ImageResoution.SelectedItem.ToString()),
                                    (Aspose.Pdf.Optimization.ImageCompressionVersion)Enum.Parse(
                                        typeof(Aspose.Pdf.Optimization.ImageCompressionVersion),
                                        BodyForm.MainForm.CompressPDF_ImageCompressVersion.SelectedValue.ToString()),
                                    BodyForm.MainForm.CompressPDF_CompressImageFlate.Checked,
                                    BodyForm.MainForm.CompressPDF_RemoveUnuseStream.Checked,
                                    BodyForm.MainForm.CompressPDF_LinkSameStream.Checked,
                                    BodyForm.MainForm.CompressPDF_CancelEmbedFont.Checked,
                                    BodyForm.MainForm.CompressPDF_RemoveForms.Checked,
                                    BodyForm.MainForm.CompressPDF_RemoveUnuseObject.Checked);
                                break;
                            // 压缩图片
                            case ConvertFileType.CompressImage:
                                bool UseSelfExt = BodyForm.MainForm.CompressImage_NotUseSelfExt.Checked;//是否不使用源文件后缀名
                                string SavedFileName = string.Empty;
                                if(UseSelfExt)
                                {
                                    SavedFileName = ConvertedFileName;
                                } else
                                {
                                    string ImageFileExtenString = Path.GetExtension(ListViewFirstColumnText);

                                    SavedFileName = Path.Combine(
                                        SelectSavePathstring,
                                        $"{FileNameWithOutExtension}{ImageFileExtenString}");
                                }

                                Util.CompressImage(
                                    ListViewFirstColumnText,
                                    SavedFileName,
                                    (Aspose.Imaging.FileFormats.Jpeg.JpegCompressionMode)Enum.Parse(
                                        typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionMode),
                                        BodyForm.MainForm.CompressJpegMode_Combox.SelectedItem.ToString()),
                                    (Aspose.Imaging.FileFormats.Jpeg.JpegCompressionColorMode)Enum.Parse(
                                        typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionColorMode),
                                        BodyForm.MainForm.CompressJpegColorMode_Combox.SelectedItem.ToString()),
                                    int.Parse(BodyForm.MainForm.CompressJpegQuailty_Combox.SelectedItem.ToString()),
                                    int.Parse(BodyForm.MainForm.CompressPngLevel_Combox.SelectedItem.ToString()),
                                    (Aspose.Imaging.FileFormats.Png.PngColorType)Enum.Parse(
                                        typeof(Aspose.Imaging.FileFormats.Png.PngColorType),
                                        BodyForm.MainForm.CompressPngColorMode_Combox.SelectedItem.ToString()),
                                    (Aspose.Imaging.FileFormats.Bmp.BitmapCompression)Enum.Parse(
                                        typeof(Aspose.Imaging.FileFormats.Bmp.BitmapCompression),
                                        BodyForm.MainForm.CompressBmpMode_Combox.SelectedItem.ToString()),
                                    int.Parse(BodyForm.MainForm.CompressBmpPixeBits_Combox.SelectedItem.ToString()),
                                    BodyForm.MainForm.CompressIamge_NoCompress.Checked);
                                break;
                            case ConvertFileType.Html2PDF:
                                Util.ConvertHtml2PDFAsync(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.html2pdf_textbox_chromepath.Text,
                                    BodyForm.MainForm.html2pdf_combox_pagesize.SelectedIndex,
                                    bool.Parse(BodyForm.MainForm.html2pdf_combox_pagedirect.SelectedValue.ToString()),
                                    BodyForm.MainForm.html2pdf_checkbox_usecss.Checked,
                                    BodyForm.MainForm.html2pdf_textbox_margin_top.Text,
                                    BodyForm.MainForm.html2pdf_textbox_margin_left.Text,
                                    BodyForm.MainForm.html2pdf_textbox_margin_bottom.Text,
                                    BodyForm.MainForm.html2pdf_textbox_margin_right.Text,
                                    BodyForm.MainForm.html2pdf_timeout.Value,
                                    BodyForm.MainForm.html2pdf_wait_timeout.Value,
                                    BodyForm.MainForm.html2pdf_wait_all_resource.Checked);
                                //Util.ConvertHtml2Pdf(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            case ConvertFileType.Word2EPUB:
                                int TempImageDpiValue = 100;
                                if(BodyForm.MainForm.CoverImageDpi.Text != string.Empty)
                                {
                                    TempImageDpiValue = int.Parse(BodyForm.MainForm.CoverImageDpi.Text);
                                }
                                Util.ConvertWord2EPUB(
                                    ListViewFirstColumnText,
                                    ConvertedFileName,
                                    BodyForm.MainForm.Word2EpubAuthorName.Text,
                                    BodyForm.MainForm.Word2EpubConverImage.Text,
                                    TempImageDpiValue);
                                break;
                            case ConvertFileType.EPUB2Word:
                                Util.ConvertEPUB2Word(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            case ConvertFileType.EPUB2Pdf:
                                Util.ConvertEPUB2Pdf(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            case ConvertFileType.SplitWord:
                                Util.SplitWord(
                                    ListViewFirstColumnText,
                                    SelectSavePathstring,
                                    BodyForm.MainForm.SplitWord_Single.Checked,
                                    BodyForm.MainForm.SplitWord_Range_Start.Text.Trim(),
                                    BodyForm.MainForm.SplitWord_Range_End.Text.Trim(),
                                    int.Parse(BodyForm.MainForm.SplitWord_PageCount.Value.ToString()),
                                    BodyForm.MainForm.SplitWordReUpdatePageNumber.Checked);
                                break;
                            case ConvertFileType.WeBp2Image:
                                Util.ConvertWeBp2Image(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            case ConvertFileType.SplitPPT:
                                Util.SplitPPT(
                                    ListViewFirstColumnText,
                                    SelectSavePathstring,
                                    BodyForm.MainForm.SplitPPT_SignleExport.Checked,
                                    BodyForm.MainForm.SplitPPT_RangeExport_Start.Text,
                                    BodyForm.MainForm.SplitPPT_RangeExport_End.Text,
                                    (Aspose.Slides.SlideSizeType)Enum.Parse(
                                        typeof(Aspose.Slides.SlideSizeType),
                                        BodyForm.MainForm.SplitPdfSizeCombox.SelectedItem.ToString()),
                                    (Aspose.Slides.SlideSizeScaleType)Enum.Parse(
                                        typeof(Aspose.Slides.SlideSizeScaleType),
                                        BodyForm.MainForm.SplitPdfScaleCombox.SelectedValue.ToString()));
                                break;
                            default:
                                break;
                        }
                        ListViewControl.Items[TempIndex].SubItems.Add(ConvertedFileName);
                        ListViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                        if(File.Exists(ConvertedFileName))
                        {
                            FileInfo ConverdFileInfo = new FileInfo(ConvertedFileName);

                            //获取转换后的文件大小
                            ListViewControl.Items[TempIndex].SubItems
                                .Add(Util.GetFileSizeDisplayString(ConverdFileInfo.Length));
                            ListViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        }

                        ListViewControl.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    ListViewControl.Items[TempIndex - 1].Selected = true;
                                    ListViewControl.EnsureVisible(TempIndex - 1);
                                    ListViewControl.Focus();
                                }));
                    }
                    StatusLabelControl.Text = "处理完毕";
                    CallButton.Enabled = true;

                    //处理完毕后需要做的事情
                    switch(ConverType)
                    {
                        case ConvertFileType.Word2Pdf:
                            if(BodyForm.MainForm.OpenPdfOptions.Checked)
                            {
                                Task<List<string>> TempPdfFileNames = GetListViewLastItemContentAsync(
                                    BodyForm.MainForm.WordFileListView);
                                TempPdfFileNames.Result
                                    .ForEach(
                                        (pdfFileName) =>
                                        {
                                            Process.Start(pdfFileName);
                                        });
                            }

                            if(BodyForm.MainForm.RemoveTempDocx.Checked)
                            {
                                if(TempSaveDocxPathString != string.Empty)
                                {
                                    Directory.Delete(TempSaveDocxPathString, true);
                                }
                            }
                            break;
                        default:
                            break;
                    }


                    BodyForm.MainForm
                        .BeginInvoke(
                            new Action(
                                () =>
                                {
                                    if(string.Compare(
                                            BodyForm.MainForm.ShowOpenTargetDialog,
                                            "on",
                                            StringComparison.Ordinal) ==
                                        0)
                                    {
                                        DialogResult MessageBoxResult = MessageBox.Show(
                                            "处理完毕，是否需要打开转换后目录？",
                                            "提示",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question);
                                        if(MessageBoxResult == DialogResult.Yes)
                                        {
                                            Process.Start(SelectSavePathstring);
                                        }
                                    }
                                }));
                }
            }
        }

        /// <summary>
        /// 获取list view控件第一列的文本项目内容
        /// </summary>
        /// <param name="ListViewControl"></param>
        /// <returns></returns>
        public static Task<List<string>> GetListViewFirstItemContentAsync(ListView ListViewControl)
        {
            return Task.Run(
                () =>
                {
                    List<string> FileNames = new List<string>();
                    try
                    {
                        if(ListViewControl.Items.Count > 0)
                        {
                            for(int Index = 0; Index < ListViewControl.Items.Count; Index++)
                            {
                                FileNames.Add(ListViewControl.Items[Index].Text);
                            }
                        }
                    } catch
                    {
                    }
                    return FileNames;
                });
        }

        /// <summary>
        /// 获取Listview控件中最后一列的文本项目内容
        /// </summary>
        /// <param name="ListViewControl"></param>
        /// <returns></returns>
        public static Task<List<string>> GetListViewLastItemContentAsync(ListView ListViewControl)
        {
            return Task.Run(
                () =>
                {
                    List<string> FileNames = new List<string>();
                    try
                    {
                        if(ListViewControl.Items.Count > 0)
                        {
                            for(int Index = 0; Index < ListViewControl.Items.Count; Index++)
                            {
                                FileNames.Add(ListViewControl.Items[Index].SubItems[2].Text);
                            }
                        }
                    } catch
                    {
                    }
                    return FileNames;
                });
        }


        public static Task<List<string>> GetPdfFileNamesAsync()
        { return GetListViewLastItemContentAsync(BodyForm.MainForm.PdfFileListView); }

        /// <summary>
        /// 将Listview中的项目向下移动（文本）
        /// </summary>
        /// <param name="ListViewControl"></param>
        public static void MoveDownListViewItem(ListView ListViewControl)
        {
            //列表中有项目才响应
            if(ListViewControl.Items.Count > 0)
            {
                //获取当前选中项目的索引
                int NowSelectIndex = ListViewControl.SelectedIndices[0];

                if(NowSelectIndex < ListViewControl.Items.Count - 1)
                {
                    //项目索引不在第一个时才处理

                    ListViewItem NowSelectItem = ListViewControl.Items[NowSelectIndex]; //当前选中的项目
                    ListViewItem BottomSelectItem = ListViewControl.Items[NowSelectIndex + 1];//当前选中项目的下一个项目

                    ListViewControl.BeginUpdate();

                    (NowSelectItem.Text, BottomSelectItem.Text) = (BottomSelectItem.Text, NowSelectItem.Text);

                    ListViewControl.Items[NowSelectIndex + 1].Selected = true;// 选中移动后的文本

                    ListViewControl.EnsureVisible(NowSelectIndex + 1); //listview跟随当前选中项移动位置

                    ListViewControl.EndUpdate();
                }

                ListViewControl.Focus();//选中控件，获取焦点
            }
        }

        /// <summary>
        /// 将Listview中的项目向上移动（文本）
        /// </summary>
        /// <param name="ListViewControl"></param>
        public static void MoveUpListViewItem(ListView ListViewControl)
        {
            //列表中有项目才响应
            if(ListViewControl.Items.Count > 0)
            {
                //获取当前选中项目的索引
                int NowSelectIndex = ListViewControl.SelectedIndices[0];

                if(NowSelectIndex > 0)
                {
                    //项目索引不在第一个时才处理

                    ListViewItem NowSelectItem = ListViewControl.Items[NowSelectIndex]; //当前选中的项目
                    ListViewItem AboveSelectItem = ListViewControl.Items[NowSelectIndex - 1];//当前选中项目的上一个项目

                    ListViewControl.BeginUpdate();

                    (NowSelectItem.Text, AboveSelectItem.Text) = (AboveSelectItem.Text, NowSelectItem.Text);
                    ListViewControl.Items[NowSelectIndex - 1].Selected = true;// 选中移动后的文本

                    ListViewControl.EnsureVisible(NowSelectIndex - 1);//listview跟随当前选中项移动位置

                    ListViewControl.EndUpdate();
                }

                ListViewControl.Focus();//选中控件，获取焦点
            }
        }

        /// <summary>
        /// 移除listvie中的单个项目或者全部项目
        /// </summary>
        /// <param name="ClearAllItems"></param>
        /// <param name="ListViewControl"></param>
        public static void RemoveListViewItem(bool ClearAllItems, ListView ListViewControl)
        {
            if(ListViewControl.Items.Count != 0)
            {
                if(ClearAllItems)
                {
                    ListViewControl.Items.Clear();
                } else
                {
                    if(ListViewControl.FocusedItem != null && ListViewControl.FocusedItem.Index >= 0)
                    {
                        //删除列表一个项目,先移除，移动到没有项目时，focuseditem会变为null，这时候就不处理了
                        //同时移除后会自动选中临近的item，所以用focuseditem就可以了
                        ListViewControl.Items.RemoveAt(ListViewControl.FocusedItem.Index);
                        if(ListViewControl.FocusedItem != null)
                        {
                            ListViewControl.Items[ListViewControl.FocusedItem.Index].Selected = true;
                        }
                    }
                }
                ListViewControl.Focus();
            }
        }
    }
}
