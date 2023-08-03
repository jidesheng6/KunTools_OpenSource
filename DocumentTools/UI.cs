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
    internal enum ConvertFileType
    {
        Word2Pdf,
        Pdf2Word,
        Excel2Pdf,
        Word2Image,
        Pdf2Excel,
        WordReplace,
        ExcelReplace,
        Image2Ico,
        Word2Excel,
        Excel2Word,
        SplitPdf,
        AddWaterMarkPdf,
        RemovePdfWaterMark,
        PPT2PDF,
        PDF2PPT,
        ConvertPdfCompliance,
        CompressPDF,
        CompressImage,
    }

    internal class UI
    {


        /// <summary>
        /// 添加文件列表到Listview中，并返回项目数量
        /// </summary>
        /// <param name="DisplayFilterString"></param>
        /// <param name="FilterArgs"></param>
        /// <param name="ListViewControl"></param>
        /// <returns>文件数量</returns>
        public static Task<int> AddFilesToListView(string DisplayFilterString, string FilterArgs, ListView ListViewControl)
        {
            return Task.Run(() =>
            {
                IEnumerable<string> TempDocuemntFileNames = Util.ShwoSelectDialog(true, false, DisplayFilterString, FilterArgs);
                ListViewControl.BeginUpdate();
                foreach (string TempDocumentFileName in TempDocuemntFileNames)
                {
                    ListViewItem TempDocuemntFileNamesListView = new ListViewItem
                    {
                        Text = TempDocumentFileName
                    };
                    TempDocuemntFileNamesListView.SubItems.Add(Util.GetNowDateTime());
                    ListViewControl.Items.Add(TempDocuemntFileNamesListView);
                    ListViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                }
                ListViewControl.EndUpdate();
                return ListViewControl.Items.Count;
            });

        }
        /// <summary>
        /// 添加文件到listview中，主要是配合drag事件使用
        /// </summary>
        /// <param name="FileList"></param>
        /// <param name="ListviewControl"></param>
        /// <param name="FileFilters"></param>
        public static void AddFilesToListviewWithNotDialog(string[] FileList, ListView ListviewControl, string[] FileFilters)
        {
            Task.Run(() =>
            {
                if (FileList.Length > 0)
                {
                    ListviewControl.BeginUpdate();
                    foreach (string File in FileList)
                    {
                        foreach (string FileFilter in FileFilters)
                        {
                            if (Path.GetExtension(File) == FileFilter)
                            {
                                ListViewItem FileListListView = new ListViewItem
                                {
                                    Text = File
                                };
                                FileListListView.SubItems.Add(Util.GetNowDateTime());
                                ListviewControl.Items.Add(FileListListView);
                                ListviewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                            }
                        }
                    }
                    ListviewControl.EndUpdate();
                }
            });

        }

        /// <summary>
        /// 获取Listview控件中最后一列的文本项目内容
        /// </summary>
        /// <param name="ListViewControl"></param>
        /// <returns></returns>
        public static Task<List<string>> GetListViewLastItemContent(ListView ListViewControl)
        {
            return Task.Run(() =>
            {
                List<string> FileNames = new List<string>();
                try
                {

                    if (ListViewControl.Items.Count > 0)
                    {
                        for (int Index = 0; Index < ListViewControl.Items.Count; Index++)
                        {
                            FileNames.Add(ListViewControl.Items[Index].SubItems[2].Text);
                        }
                    }

                }
                catch
                {

                }
                return FileNames;
            });
        }

        /// <summary>
        /// 获取list view控件第一列的文本项目内容
        /// </summary>
        /// <param name="ListViewControl"></param>
        /// <returns></returns>
        public static Task<List<string>> GetListViewFirstItemContent(ListView ListViewControl)
        {
            return Task.Run(() =>
            {
                List<string> FileNames = new List<string>();
                try
                {

                    if (ListViewControl.Items.Count > 0)
                    {
                        for (int Index = 0; Index < ListViewControl.Items.Count; Index++)
                        {
                            FileNames.Add(ListViewControl.Items[Index].Text);
                        }
                    }

                }
                catch
                {

                }
                return FileNames;
            });


        }
        /// <summary>
        /// 移除listvie中的单个项目或者全部项目
        /// </summary>
        /// <param name="ClearAllItems"></param>
        /// <param name="ListViewControl"></param>
        /// <param name="ItemsIndex"></param>
        public static void RemoveListViewItem(bool ClearAllItems, ListView ListViewControl, int ItemsIndex = 0)
        {
            if (ClearAllItems)
            {
                ListViewControl.Items.Clear();
            }
            else
            {
                ListViewControl.Items.RemoveAt(ItemsIndex);
            }
        }

        public static Task<int> AddWordFiles()
        {
            return AddFilesToListView("WORD文档", "*.doc;*.docx", BodyForm.MainForm.WordFileListView);
        }

        public static void AddPdfFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.PdfFileListView);
        }

        public static void AddExcelFiles()
        {
            AddFilesToListView("EXCEL文档", "*.xls;*.xlsx", BodyForm.MainForm.ExcelFileListView);
        }

        public static void AddWord2ImageFiles()
        {
            AddFilesToListView("支持的文档", "*.docx;*.doc;*.xls;*.xlsx;*.pdf", BodyForm.MainForm.DocumentsListView);
        }

        public static void AddMeregePdfFiles()
        {
            AddFilesToListView("支持的文件", "*.pdf;*.jpg;*.png;*.bmp;*.tif", BodyForm.MainForm.MergePdfFileView);
        }

        public static void AddPdf2ExcelFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.PDF2ExcelFileView);
        }

        public static void AddWordRelaceFiles()
        {
            AddFilesToListView("WORD文档", "*.docx;*.doc", BodyForm.MainForm.ReplaceWordFileList);
        }

        public static void AddExcelRelaceFiles()
        {
            AddFilesToListView("EXCEL文档", "*.xls;*.xlsx", BodyForm.MainForm.ReplaceExcelFileView);
        }

        public static void AddDataMattixWordFiles()
        {
            AddFilesToListView("WORD文档", "*.doc;*.docx", BodyForm.MainForm.DecodeMatrixWordListView);
        }

        public static void AddIcoConvertFiles()
        {
            AddFilesToListView("图片文件", "*.jpg;*.png;*.bmp", BodyForm.MainForm.IcoFileView);
        }

        public static void AddMergeWordFiles()
        {
            AddFilesToListView("WORD文档", "*.doc;*.docx", BodyForm.MainForm.MergeWordFileList);
        }

        public static void AddWord2ExcelFiles()
        {
            AddFilesToListView("WORD文档", "*.doc;*.docx", BodyForm.MainForm.Word2ExcelFileList);
        }

        public static void AddExcel2WordFiles()
        {
            AddFilesToListView("EXCEL文档", "*.xls;*.xlsx", BodyForm.MainForm.Excel2WordFileList);
        }

        public static void AddSplitPdfFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.SplitPdfFileList);
        }

        public static void AddWaterMarkPdfFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.PdfWaterMarkFileList);
        }

        public static void AddRemoveWaterFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.RemovePdfWaterMarkFileList);
        }

        public static void AddPPT2PdfFiles()
        {
            AddFilesToListView("演示文稿", "*.ppt;*.pptx", BodyForm.MainForm.PPT2PdfFileList);
        }

        public static void AddPdf2PPTFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.PDF2PPTFileList);
        }

        public static void AddConvertComplanicePdfFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.ConvertPdfComplianceFileList);
        }

        public static void AddConvertImage2PDFFiles()
        {
            AddFilesToListView("图片文件", "*.jpg;*.jpeg;*.png;*.bmp", BodyForm.MainForm.ConvertImage2PDFFileList);
        }

        public static void AddCompressPDFFiles()
        {
            AddFilesToListView("PDF文档", "*.pdf", BodyForm.MainForm.CompressPdfFileList);
        }

        public static void AddCompressImageFiles()
        {
            AddFilesToListView("PDF文档", "*.jpg;*.jpeg;*.png;*.bmp", BodyForm.MainForm.CompressImageFileList);
        }

        /// <summary>
        /// 转换文件通用方法
        /// </summary>
        /// <param name="ListViewControl">转换文件所在的listview控件名称</param>
        /// <param name="StatusLabelControl">用于表示进度状态的标签控件</param>
        /// <param name="ConverType">转换的目标类型</param>
        /// <param name="ExtensionString">保存文件的后缀名称</param>
        public static void ConvertStart(ListView ListViewControl, Label StatusLabelControl, ConvertFileType ConverType, string ExtensionString)
        {
            if (ListViewControl.Items.Count > 0)
            {
                string TempSaveDocxPathString = string.Empty;
                IEnumerable<string> ISelectSavePathstring = Util.ShwoSelectDialog(false, true, "", "");
                string SelectSavePathstring = ISelectSavePathstring.FirstOrDefault();

                if (SelectSavePathstring != default)
                {
                    for (int TempIndex = 0; TempIndex < ListViewControl.Items.Count; TempIndex++)
                    {
                        string ListViewFirstColumnText = ListViewControl.Items[TempIndex].Text;
                        string FileNameWithOutExtension = Path.GetFileNameWithoutExtension(ListViewFirstColumnText);//不带后缀的文件名
                        string ConvertedFileName = Path.Combine(SelectSavePathstring, FileNameWithOutExtension + ExtensionString);
                        ListViewControl.Items[TempIndex].SubItems.Add(ConvertedFileName);
                        ListViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        StatusLabelControl.Text = $"正在处理第{TempIndex + 1}个文件";

                        //根据不同类型调用不同的函数
                        switch (ConverType)
                        {
                            //word转pdf
                            case ConvertFileType.Word2Pdf:
                                TempSaveDocxPathString = Path.Combine(SelectSavePathstring, "TempDocx");
                                Directory.CreateDirectory(TempSaveDocxPathString);

                                object SelectedPdfCompliance = Enum.Parse(typeof(Aspose.Words.Saving.PdfCompliance), BodyForm.MainForm.PDFComplianceBox.SelectedItem.ToString());

                                Util.ConvertWord2Pdf(ListViewFirstColumnText, ConvertedFileName, TempSaveDocxPathString, (Aspose.Words.Saving.PdfCompliance)SelectedPdfCompliance);
                                break;
                            //pdf转word
                            case ConvertFileType.Pdf2Word:
                                Util.ConvertPdf2Word(ListViewFirstColumnText, ConvertedFileName, BodyForm.MainForm.PDF2WORD_UseBullet.Checked, BodyForm.MainForm.PDF2WORD_UseSUbOcr.Checked, (Aspose.Pdf.DocSaveOptions.RecognitionMode)Enum.Parse(typeof(Aspose.Pdf.DocSaveOptions.RecognitionMode), BodyForm.MainForm.PDF2WORD_Mode.SelectedValue.ToString()));
                                break;
                            //excel转pdf
                            case ConvertFileType.Excel2Pdf:
                                Util.ConvertExcel2Pdf(ListViewFirstColumnText, ConvertedFileName, BodyForm.MainForm.ExportEachSheet.Checked);
                                break;
                            //word转图片
                            case ConvertFileType.Word2Image:
                                Util.ConvertWord2Image(ListViewFirstColumnText, ConvertedFileName, ExtensionString, BodyForm.MainForm.SetGrayCheckBox.Checked, BodyForm.MainForm.PageExportCheck.Checked, Convert.ToInt32(BodyForm.MainForm.DpiItemList.SelectedValue));
                                break;
                            //pdf转excel
                            case ConvertFileType.Pdf2Excel:
                                Util.ConvertPdf2Excel(ListViewFirstColumnText, ConvertedFileName, BodyForm.MainForm.ExcelSheelMode.Checked);
                                break;
                            //批量替换word内容
                            case ConvertFileType.WordReplace:
                                if (BodyForm.MainForm.WordReplaceFindTextBox.Text != string.Empty)
                                {
                                    Util.RelaceDocumentContent(ListViewFirstColumnText, ConvertedFileName, "WORD", BodyForm.MainForm.WordReplaceFindTextBox.Text, BodyForm.MainForm.WordReplaceTextBox.Text, BodyForm.MainForm.IgnoreFootContent.Checked, BodyForm.MainForm.IgnoreCase.Checked);
                                }
                                break;
                            //批量替换Excel内容
                            case ConvertFileType.ExcelReplace:
                                Util.RelaceDocumentContent(ListViewFirstColumnText, ConvertedFileName, "EXCEL", BodyForm.MainForm.ExcelFindTextBox.Text, BodyForm.MainForm.ExcelReplaceBox.Text, false, BodyForm.MainForm.ExcelNoIgnoreCase.Checked, BodyForm.MainForm.RemoveRows.Checked, Convert.ToInt32(BodyForm.MainForm.ExcelStartRow.Text), Convert.ToInt32(BodyForm.MainForm.ExcelEndRow.Text), BodyForm.MainForm.UseRegex.Checked);
                                break;
                            // 批量图片转为ICO
                            case ConvertFileType.Image2Ico:
                                Util.ConvertImage2Ico(ListViewFirstColumnText, ConvertedFileName, Convert.ToInt32(BodyForm.MainForm.IcoSizeDrop.SelectedValue));
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
                                Util.SplitPdf(ListViewFirstColumnText, SelectSavePathstring);
                                break;
                            // pdf添加水印
                            case ConvertFileType.AddWaterMarkPdf:
                                Util.AddWaterMark2Pdf(ListViewFirstColumnText, ConvertedFileName, BodyForm.MainForm.PdfWaterMarkText.Text, float.Parse(BodyForm.MainForm.PdfWaterMarkOpcityNumber.Text.ToString()), BodyForm.MainForm.PdfWaterMarkRotateDegNumber.Text, BodyForm.MainForm.PdfWaterMarkHorSetting.SelectedIndex, BodyForm.MainForm.PDFWaterMarkVerSetting.SelectedIndex, PrivateFontSetting.FontColor, PrivateFontSetting.FontName, PrivateFontSetting.FontSize, BodyForm.MainForm.AddWaterMarkBackGroudImageFileName, BodyForm.MainForm.WaterMarkImageHeight.Text, BodyForm.MainForm.WaterMarkImageWidth.Text);
                                break;
                            // pdf移除水印
                            case ConvertFileType.RemovePdfWaterMark:
                                Util.RemovePdfWaterMark(ListViewFirstColumnText, ConvertedFileName);
                                break;
                            // ppt转pdf
                            case ConvertFileType.PPT2PDF:
                                object SelectCommentPostion = Enum.Parse(typeof(Aspose.Slides.Export.NotesPositions), BodyForm.MainForm.ExportNotePageCombox.SelectedValue.ToString());
                                Util.ConvertPPT2PDF(ListViewFirstColumnText, ConvertedFileName, BodyForm.MainForm.DrawBlackLine.Checked, BodyForm.MainForm.ConvertPPT2PDFPassword.Text.Trim(), BodyForm.MainForm.ExportHiddenPage.Checked, (Aspose.Slides.Export.NotesPositions)SelectCommentPostion);
                                break;
                            // pdf转ppt
                            case ConvertFileType.PDF2PPT:
                                Util.ConvertPDF2PPT(ListViewFirstColumnText, ConvertedFileName, BodyForm.MainForm.ExtraOcrBox.Checked, BodyForm.MainForm.SplitShapeBox.Checked, BodyForm.MainForm.RenderImageBox.Checked);
                                break;
                            // pdf版本转换
                            case ConvertFileType.ConvertPdfCompliance:
                                object SelectPdfCompliance = Enum.Parse(typeof(Aspose.Pdf.PdfFormat), BodyForm.MainForm.ConvertPdfComplianceVersion.SelectedItem.ToString());
                                Util.ConvertPdfCompliance(ListViewFirstColumnText, ConvertedFileName, (Aspose.Pdf.PdfFormat)SelectPdfCompliance);
                                break;
                            // 压缩PDF
                            case ConvertFileType.CompressPDF:
                                Util.CompressPDF(ListViewFirstColumnText, ConvertedFileName,
                                                 BodyForm.MainForm.CompressPDF_WEB.Checked, BodyForm.MainForm.CompressPDF_CompressImage.Checked,
                                                 int.Parse(BodyForm.MainForm.CompressPDF_ImageQuailty.SelectedItem.ToString()),
                                                 int.Parse(BodyForm.MainForm.CompressPDF_ImageResoution.SelectedItem.ToString()),
                                                 (Aspose.Pdf.Optimization.ImageCompressionVersion)Enum.Parse(typeof(Aspose.Pdf.Optimization.ImageCompressionVersion), BodyForm.MainForm.CompressPDF_ImageCompressVersion.SelectedValue.ToString()),
                                                 BodyForm.MainForm.CompressPDF_CompressImageFlate.Checked, BodyForm.MainForm.CompressPDF_RemoveUnuseStream.Checked,
                                                 BodyForm.MainForm.CompressPDF_LinkSameStream.Checked, BodyForm.MainForm.CompressPDF_CancelEmbedFont.Checked,
                                                 BodyForm.MainForm.CompressPDF_RemoveForms.Checked, BodyForm.MainForm.CompressPDF_RemoveUnuseObject.Checked
                                                 );
                                break;
                            // 压缩图片
                            case ConvertFileType.CompressImage:
                                bool UseSelfExt = BodyForm.MainForm.CompressImage_NotUseSelfExt.Checked;//是否不使用源文件后缀名
                                string SavedFileName = string.Empty;
                                if (UseSelfExt)
                                {
                                    SavedFileName = ConvertedFileName;
                                }
                                else
                                {
                                    string ImageFileExtenString = Path.GetExtension(ListViewFirstColumnText);

                                    SavedFileName = Path.Combine(SelectSavePathstring, FileNameWithOutExtension + ImageFileExtenString);
                                }

                                Util.CompressImage(ListViewFirstColumnText, SavedFileName,
                                                   (Aspose.Imaging.FileFormats.Jpeg.JpegCompressionMode)Enum.Parse(typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionMode), BodyForm.MainForm.CompressJpegMode_Combox.SelectedItem.ToString()),
                                                   (Aspose.Imaging.FileFormats.Jpeg.JpegCompressionColorMode)Enum.Parse(typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionColorMode), BodyForm.MainForm.CompressJpegColorMode_Combox.SelectedItem.ToString()),
                                                   int.Parse(BodyForm.MainForm.CompressJpegQuailty_Combox.SelectedItem.ToString()), int.Parse(BodyForm.MainForm.CompressPngLevel_Combox.SelectedItem.ToString()),
                                                   (Aspose.Imaging.FileFormats.Png.PngColorType)Enum.Parse(typeof(Aspose.Imaging.FileFormats.Png.PngColorType), BodyForm.MainForm.CompressPngColorMode_Combox.SelectedItem.ToString()),
                                                   (Aspose.Imaging.FileFormats.Bmp.BitmapCompression)Enum.Parse(typeof(Aspose.Imaging.FileFormats.Bmp.BitmapCompression), BodyForm.MainForm.CompressBmpMode_Combox.SelectedItem.ToString()),
                                                   int.Parse(BodyForm.MainForm.CompressBmpPixeBits_Combox.SelectedItem.ToString())
                                                   );
                                break;
                            default: break;
                        }
                        FileInfo ConverdFileInfo = new FileInfo(ConvertedFileName);
                        //获取转换后的文件大小
                        ListViewControl.Items[TempIndex].SubItems.Add(Util.GetFileSizeDisplayString(ConverdFileInfo.Length));

                        ListViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);

                        StatusLabelControl.Text = "处理完毕";

                    }

                    //处理完毕后需要做的事情
                    switch (ConverType)
                    {
                        case ConvertFileType.Word2Pdf:
                            if (BodyForm.MainForm.OpenPdfOptions.Checked)
                            {
                                Task<List<string>> TempPdfFileNames = GetListViewLastItemContent(BodyForm.MainForm.WordFileListView);
                                TempPdfFileNames.Result.ForEach((pdfFileName) =>
                                {
                                    Process.Start(pdfFileName);
                                });
                            }

                            if (BodyForm.MainForm.RemoveTempDocx.Checked)
                            {
                                if (TempSaveDocxPathString != string.Empty)
                                {
                                    Directory.Delete(TempSaveDocxPathString, true);
                                }
                            }
                            break;
                        default: break;
                    }


                    BodyForm.MainForm.BeginInvoke(new Action(() =>
                    {
                        if (BodyForm.MainForm.ShowOpenTargetDialog == "on")
                        {
                            DialogResult MessageBoxResult = MessageBox.Show("处理完毕，是否需要打开转换后目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (MessageBoxResult == DialogResult.Yes)
                            {
                                Process.Start(SelectSavePathstring);
                            }
                        }
                    }));
                }
            }
        }

        /// <summary>
        /// 将Listview中的项目向上移动（文本）
        /// </summary>
        /// <param name="ListViewControl"></param>
        public static void MoveUpListViewItem(ListView ListViewControl)
        {
            //列表中有项目才响应
            if (ListViewControl.Items.Count > 0)
            {
                //获取当前选中项目的索引
                int NowSelectIndex = ListViewControl.SelectedIndices[0];

                if (NowSelectIndex > 0)
                {
                    //项目索引不在第一个时才处理

                    ListViewItem NowSelectItem = ListViewControl.Items[NowSelectIndex]; //当前选中的项目
                    ListViewItem AboveSelectItem = ListViewControl.Items[NowSelectIndex - 1];//当前选中项目的上一个项目

                    ListViewControl.BeginUpdate();

                    string TempText = AboveSelectItem.Text;

                    AboveSelectItem.Text = NowSelectItem.Text;//移动后，上方的文本变为原来选中的文本

                    NowSelectItem.Text = TempText;// 移动后，原来位置，对于移动后位置来说，就是下方的文本变为之前上方的文本

                    ListViewControl.Items[NowSelectIndex - 1].Selected = true;// 选中移动后的文本

                    ListViewControl.EndUpdate();

                }

                ListViewControl.Focus();//选中控件，获取焦点
            }
        }

        /// <summary>
        /// 将Listview中的项目向下移动（文本）
        /// </summary>
        /// <param name="ListViewControl"></param>
        public static void MoveDownListViewItem(ListView ListViewControl)
        {
            //列表中有项目才响应
            if (ListViewControl.Items.Count > 0)
            {
                //获取当前选中项目的索引
                int NowSelectIndex = ListViewControl.SelectedIndices[0];

                if (NowSelectIndex < ListViewControl.Items.Count - 1)
                {
                    //项目索引不在第一个时才处理

                    ListViewItem NowSelectItem = ListViewControl.Items[NowSelectIndex]; //当前选中的项目
                    ListViewItem BottomSelectItem = ListViewControl.Items[NowSelectIndex + 1];//当前选中项目的下一个项目

                    ListViewControl.BeginUpdate();

                    string TempText = BottomSelectItem.Text;

                    BottomSelectItem.Text = NowSelectItem.Text;

                    NowSelectItem.Text = TempText;

                    ListViewControl.Items[NowSelectIndex + 1].Selected = true;// 选中移动后的文本

                    ListViewControl.EndUpdate();

                }

                ListViewControl.Focus();//选中控件，获取焦点
            }
        }


        public static Task<List<string>> GetPdfFileNames()
        {
            return GetListViewLastItemContent(BodyForm.MainForm.PdfFileListView);
        }


    }
}
