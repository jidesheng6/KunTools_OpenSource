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
        Image2Ico
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
                    ListViewItem TempDocuemntFileNamesListView = new ListViewItem();
                    TempDocuemntFileNamesListView.Text = TempDocumentFileName;
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
                                ListViewItem FileListListView = new ListViewItem();
                                FileListListView.Text = File;
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
                        string FileNameWithOutExtension = Path.GetFileNameWithoutExtension(ListViewFirstColumnText);
                        string ConvertedFileName = Path.Combine(SelectSavePathstring, FileNameWithOutExtension + ExtensionString);
                        ListViewControl.Items[TempIndex].SubItems.Add(ConvertedFileName);
                        ListViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                        StatusLabelControl.Text = $"正在处理{TempIndex + 1}个文件";

                        //根据不同类型调用不同的函数
                        switch (ConverType)
                        {
                            //word转pdf
                            case ConvertFileType.Word2Pdf:
                                TempSaveDocxPathString = Path.Combine(SelectSavePathstring, "TempDocx");
                                Directory.CreateDirectory(TempSaveDocxPathString);
                                Util.ConvertWord2Pdf(ListViewFirstColumnText, ConvertedFileName, TempSaveDocxPathString);
                                break;
                            //pdf转word
                            case ConvertFileType.Pdf2Word:
                                Util.ConvertPdf2Word(ListViewFirstColumnText, ConvertedFileName);
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
                            default: break;
                        }
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
                        DialogResult MessageBoxResult = MessageBox.Show("处理完毕，是否需要打开转换后目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (MessageBoxResult == DialogResult.Yes)
                        {
                            Process.Start(SelectSavePathstring);
                        }
                    }));
                }
            }
        }


        public static Task<List<string>> GetPdfFileNames()
        {
            return GetListViewLastItemContent(BodyForm.MainForm.PdfFileListView);
        }


    }
}
