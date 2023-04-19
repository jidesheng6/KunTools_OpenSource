using Aspose.Pdf.Drawing;
using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace DocumentTools
{

    public partial class BodyForm : Form
    {
        public static BodyForm MainForm;
        private ListView TempListview;
        private TwainSession twainSession;
        private DataSource dataSource;
        private int ImageIndex = 0;
        private int TempImageCount = 0;
        private int TotalTempImageCount = 0;
        private double ScanSeconds = 0;
        private Bitmap PictureBoxPreviewImage;
        private bool TwainStopScan = false;


        public BodyForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            MainForm = this;
        }
        private void BodyForm_Load(object sender, EventArgs e)
        {
            InitListView();
            InitComboxValue();
            InitTwain();
            this.AllowDrop = true;
            this.PageExportCheck.Checked = true;
            this.PageExportCheck.Enabled = false;

            this.MergePdfButton.Enabled = false;

            this.PreviewPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            this.ScanPictureSavePathBox.Enabled = false;
            this.DecodeDataMatrixView.ShowLines = true;
            this.DecodeDataMatrixView.ShowRootLines = true;
            this.DecodeDataMatrixView.FullRowSelect = true;
            ReleaseRegistry.Enabled = false;



            //Util.PlayMp3WithStream();

           

        }


        private void InitListView()
        {
            // WordFileListView
            this.WordFileListView.View = View.Details;
            this.WordFileListView.FullRowSelect = true;
            this.WordFileListView.AllowDrop = true;
            this.WordFileListView.Columns.Add("文件名称", 300);
            this.WordFileListView.Columns.Add("添加时间", 300);
            this.WordFileListView.Columns.Add("转换后文件名称", 300);
            this.WordFileListView.Name = "WordFileListView";

            this.WordFileListView.DragEnter += ListViewDragEnter;
            this.WordFileListView.DragDrop += ListviewDragDrop;

            //PdfFileListView
            this.PdfFileListView.View = View.Details;
            this.PdfFileListView.FullRowSelect = true;
            this.PdfFileListView.AllowDrop = true;
            this.PdfFileListView.Columns.Add("文件名称", 300);
            this.PdfFileListView.Columns.Add("添加时间", 300);
            this.PdfFileListView.Columns.Add("转换后文件名称", 300);
            this.PdfFileListView.Name = "PdfFileListView";

            this.PdfFileListView.DragEnter += ListViewDragEnter;
            this.PdfFileListView.DragDrop += ListviewDragDrop;

            //ExcelFileListView
            this.ExcelFileListView.View = View.Details;
            this.ExcelFileListView.FullRowSelect = true;
            this.ExcelFileListView.AllowDrop = true;
            this.ExcelFileListView.Columns.Add("文件名称", 300);
            this.ExcelFileListView.Columns.Add("添加时间", 300);
            this.ExcelFileListView.Columns.Add("转换后文件名称", 300);
            this.ExcelFileListView.Name = "ExcelFileListView";

            this.ExcelFileListView.DragEnter += ListViewDragEnter;
            this.ExcelFileListView.DragDrop += ListviewDragDrop;

            //DocumentsListView
            this.DocumentsListView.View = View.Details;
            this.DocumentsListView.FullRowSelect = true;
            this.DocumentsListView.AllowDrop = true;
            this.DocumentsListView.Columns.Add("文件名称", 300);
            this.DocumentsListView.Columns.Add("添加时间", 300);
            this.DocumentsListView.Columns.Add("转换后文件名称", 300);
            this.DocumentsListView.Name = "DocumentsListView";

            this.DocumentsListView.DragEnter += ListViewDragEnter;
            this.DocumentsListView.DragDrop += ListviewDragDrop;

            //PDF2ExcelFileView
            this.PDF2ExcelFileView.View = View.Details;
            this.PDF2ExcelFileView.FullRowSelect = true;
            this.PDF2ExcelFileView.AllowDrop = true;
            this.PDF2ExcelFileView.Columns.Add("文件名称", 300);
            this.PDF2ExcelFileView.Columns.Add("添加时间", 300);
            this.PDF2ExcelFileView.Columns.Add("转换后文件名称", 300);
            this.PDF2ExcelFileView.Name = "PDF2ExcelFileView";

            this.PDF2ExcelFileView.DragEnter += ListViewDragEnter;
            this.PDF2ExcelFileView.DragDrop += ListviewDragDrop;

            //MergePdfFileView
            this.MergePdfFileView.View = View.Details;
            this.MergePdfFileView.FullRowSelect = true;
            this.MergePdfFileView.AllowDrop = true;
            this.MergePdfFileView.Columns.Add("文件名称", 300);
            this.MergePdfFileView.Columns.Add("添加时间", 300);
            this.MergePdfFileView.Columns.Add("转换后文件名称", 300);
            this.MergePdfFileView.Name = "MergePdfFileView";

            this.MergePdfFileView.DragEnter += ListViewDragEnter;
            this.MergePdfFileView.DragDrop += ListviewDragDrop;


            //ReplaceWordFileList
            this.ReplaceWordFileList.View = View.Details;
            this.ReplaceWordFileList.FullRowSelect = true;
            this.ReplaceWordFileList.AllowDrop = true;
            this.ReplaceWordFileList.Columns.Add("文件名称", 300);
            this.ReplaceWordFileList.Columns.Add("添加时间", 300);
            this.ReplaceWordFileList.Columns.Add("转换后文件名称", 300);
            this.ReplaceWordFileList.Name = "ReplaceWordFileList";

            this.ReplaceWordFileList.DragEnter += ListViewDragEnter;
            this.ReplaceWordFileList.DragDrop += ListviewDragDrop;

            //ReplaceExcelFileView
            this.ReplaceExcelFileView.View = View.Details;
            this.ReplaceExcelFileView.FullRowSelect = true;
            this.ReplaceExcelFileView.AllowDrop = true;
            this.ReplaceExcelFileView.Columns.Add("文件名称", 300);
            this.ReplaceExcelFileView.Columns.Add("添加时间", 300);
            this.ReplaceExcelFileView.Columns.Add("转换后文件名称", 300);
            this.ReplaceExcelFileView.Name = "ReplaceExcelFileView";

            this.ReplaceExcelFileView.DragEnter += ListViewDragEnter;
            this.ReplaceExcelFileView.DragDrop += ListviewDragDrop;

            //DecodeMatrixWordListView
            this.DecodeMatrixWordListView.View = View.Details;
            this.DecodeMatrixWordListView.FullRowSelect = true;
            this.DecodeMatrixWordListView.AllowDrop = true;
            this.DecodeMatrixWordListView.Columns.Add("文件名称", 300);
            this.DecodeMatrixWordListView.Columns.Add("添加时间", 300);
            this.DecodeMatrixWordListView.Columns.Add("转换后文件名称", 300);
            this.DecodeMatrixWordListView.Name = "DecodeMatrixWordListView";

            this.DecodeMatrixWordListView.DragEnter += ListViewDragEnter;
            this.DecodeMatrixWordListView.DragDrop += ListviewDragDrop;

            //IcoFileView
            this.IcoFileView.View = View.Details;
            this.IcoFileView.FullRowSelect = true;
            this.IcoFileView.AllowDrop = true;
            this.IcoFileView.Columns.Add("文件名称", 300);
            this.IcoFileView.Columns.Add("添加时间", 300);
            this.IcoFileView.Columns.Add("转换后文件名称", 300);
            this.IcoFileView.Name = "IcoFileView";

            this.IcoFileView.DragEnter += ListViewDragEnter;
            this.IcoFileView.DragDrop += ListviewDragDrop;
        }

        private void InitComboxValue()
        {
            DataTable ImageConverTypeData = new DataTable();
            ImageConverTypeData.Columns.Add("ImageTypeDisplayName");
            ImageConverTypeData.Columns.Add("ImageTypeValueName");

            ImageConverTypeData.Rows.Add("JPG", ".jpg");
            ImageConverTypeData.Rows.Add("PNG", ".png");
            ImageConverTypeData.Rows.Add("BMP", ".bmp");
            ImageConverTypeData.Rows.Add("TIFF", ".tif");

            this.ConvertImageList.DataSource = ImageConverTypeData;
            this.ConvertImageList.DisplayMember = "ImageTypeDisplayName";
            this.ConvertImageList.ValueMember = "ImageTypeValueName";

            this.ConvertImageList.DropDownStyle = ComboBoxStyle.DropDownList;


            DataTable DpiStorgeTable = new DataTable();
            DpiStorgeTable.Columns.Add("DpiDisplayName");
            DpiStorgeTable.Columns.Add("DpiValue");

            DpiStorgeTable.Rows.Add("150DPI", 150);
            DpiStorgeTable.Rows.Add("200DPI", 200);
            DpiStorgeTable.Rows.Add("250DPI", 250);
            DpiStorgeTable.Rows.Add("300DPI", 300);
            DpiStorgeTable.Rows.Add("350DPI", 350);
            DpiStorgeTable.Rows.Add("400DPI", 400);
            DpiStorgeTable.Rows.Add("450DPI", 450);
            DpiStorgeTable.Rows.Add("500DPI", 500);
            DpiStorgeTable.Rows.Add("550DPI", 550);
            DpiStorgeTable.Rows.Add("600DPI", 600);

            this.DpiItemList.DataSource = DpiStorgeTable;
            this.DpiItemList.DisplayMember = "DpiDisplayName";
            this.DpiItemList.ValueMember = "DpiValue";

            this.DpiItemList.DropDownStyle = ComboBoxStyle.DropDownList;


            DataTable PdfFormatPage = new DataTable();
            PdfFormatPage.Columns.Add("PdfFormatDisplayValue");
            PdfFormatPage.Columns.Add("PdfFormatValue");
            PdfFormatPage.Rows.Add("A4横排", "A4_Hen");
            PdfFormatPage.Rows.Add("A4竖排", "A4_Shu");
            PdfFormatPage.Rows.Add("A3横排", "A3_Hen");
            PdfFormatPage.Rows.Add("A3竖排", "A3_Shu");

            this.MeregeFilePage.DataSource = PdfFormatPage;
            this.MeregeFilePage.DisplayMember = "PdfFormatDisplayValue";
            this.MeregeFilePage.ValueMember = "PdfFormatValue";

            DataTable IcoSizeTable = new DataTable();
            IcoSizeTable.Columns.Add("IcoSizeDisplay");
            IcoSizeTable.Columns.Add("IcoSizeValue");
            IcoSizeTable.Rows.Add("16X16", 16);
            IcoSizeTable.Rows.Add("32X32", 32);
            IcoSizeTable.Rows.Add("64X64", 64);
            IcoSizeTable.Rows.Add("128X128", 128);
            IcoSizeTable.Rows.Add("256X256", 256);

            this.IcoSizeDrop.DataSource = IcoSizeTable;
            this.IcoSizeDrop.DisplayMember = "IcoSizeDisplay";
            this.IcoSizeDrop.ValueMember = "IcoSizeValue";



            this.MeregeFilePage.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TwainColorMode.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TwainDpi.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TwainPaperSizeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.IcoSizeDrop.DropDownStyle = ComboBoxStyle.DropDownList;



        }

        private void ChooseDocButton_Click(object sender, EventArgs e)
        {
            Task<int> FileCount = UI.AddWordFiles();
            this.StatusText.Text = $"{FileCount.Result}个待转换文档";
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.WordFileListView, this.StatusText, ConvertFileType.Word2Pdf, ".pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });


        }

        private void SelectPdfButton_Click(object sender, EventArgs e)
        {
            UI.AddPdfFiles();
        }

        private void Convert2DocButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.PdfFileListView, this.PdfStatusText, ConvertFileType.Pdf2Word, ".docx");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });


        }

        private void SelectExcelFileButton_Click(object sender, EventArgs e)
        {
            UI.AddExcelFiles();
        }

        private void MergePdfButton_Click(object sender, EventArgs e)
        {

            Task<List<string>> WaitMergePdfFileNames = UI.GetListViewLastItemContent(this.WordFileListView);
            if (WaitMergePdfFileNames.Result.Count > 0)
            {
                string SaveMergePdfFileName = Util.ShowSaveFileDialog("PDF文件|*.pdf", "设置要保存的PDF文件名称");
                if (SaveMergePdfFileName != string.Empty)
                {
                    Util.MergrePdfFIle(WaitMergePdfFileNames.Result, SaveMergePdfFileName, PictureMergeMode.Checked, MeregeFilePage.SelectedValue.ToString());
                    MessageBox.Show("合并完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        private void ConvertExcel2PdfButton_Click(object sender, EventArgs e)
        {

            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.ExcelFileListView, this.Excel2PdfStatusText, ConvertFileType.Excel2Pdf, ".pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void ConvertImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void ConvertWord2ImageButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.DocumentsListView, this.ConvertWord2ImageStatus, ConvertFileType.Word2Image, this.ConvertImageList.SelectedValue.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });

        }

        private void SelectWord2ImageFileButton_Click(object sender, EventArgs e)
        {
            UI.AddWord2ImageFiles();
        }

        private void DpiItemList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ConvertTextButton_Click(object sender, EventArgs e)
        {
            List<string> TempLines = new List<string>();
            TargetTextBox.Lines = Util.AddText2List(SourceTextBox.Text, DoubleTextAlignCheck.Checked, RemoveNumberCheck.Checked, RemoveSquareCheck.Checked, NameStorkeChcek.Checked);
            //双字对齐功能，放在全部的文本处理完毕以后再进行重新处理，不然只能针对源文本字符长度进行判断，不太合适
            if(DoubleTextAlignCheck.Checked && this.TargetTextBox.Text !=string.Empty)
            {
               string DoubleText = string.Empty;
               foreach(string TargetTextTemp in this.TargetTextBox.Lines)
                {
                    DoubleText = TargetTextTemp;
                   if (DoubleText.Length==2)
                    {
                        DoubleText = DoubleText.Insert(1,"  ");
                    }
                   TempLines.Add(DoubleText);//把不需要对齐的也一起加回来
                }
                TargetTextBox.Lines = TempLines.ToArray();
            }



        }

        private void SourceTextBox_TextChanged(object sender, EventArgs e)
        {
            //如果启用多行模式，则每次添加文本后，都在末尾添加\t控制字符
            if (MutilLineMode.Checked)
            {
                SourceTextBox.AppendText("\t");
            }
        }

        private void TargetTextBox_TextChanged(object sender, EventArgs e)
        {
            TargetTextNumberText.Text = $"处理后姓名有{TargetTextBox.Lines.Length}个";

        }

        private void MutilLineMode_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void SelectPDF2ExcelButton_Click(object sender, EventArgs e)
        {
            UI.AddPdf2ExcelFiles();
        }

        private void ConvertPDF2Excel_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.PDF2ExcelFileView, this.ConvertPDF2ExcelStatusText, ConvertFileType.Pdf2Excel, ".xlsx");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });

        }

        private void MeregePdfFileButtonConvert_Click(object sender, EventArgs e)
        {
            Task<List<string>> MeregePdfFiles = UI.GetListViewFirstItemContent(this.MergePdfFileView);
            if (MeregePdfFiles.Result.Count > 0)
            {
                string MeregePdfFileName = Util.ShowSaveFileDialog("PDF文档|.pdf", "设置文件名称");
                if (MeregePdfFileName != string.Empty)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            string MeregeDirectory = System.IO.Path.GetDirectoryName(MeregePdfFileName);
                            this.MeregePdfFileStatusText.Text = "正在合并中...";
                            Util.MergrePdfFIle(MeregePdfFiles.Result, MeregePdfFileName, PictureMergeMode.Checked, MeregeFilePage.SelectedValue.ToString());
                            this.MeregePdfFileStatusText.Text = "合并完毕，右键名称可移除文件";
                            if (OpenMeregePdfFile.Checked)
                            {
                                Process.Start(MeregePdfFileName);
                            }
                            this.BeginInvoke(new Action(() =>
                            {
                                DialogResult OpenResult = MessageBox.Show("合并完毕，是否需要打开合并文件所在目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                                if (OpenResult == DialogResult.Yes)
                                {
                                    Process.Start(MeregeDirectory);
                                }
                            }));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        }

                    });


                }
            }
        }

        private void MergePdfFileSelectButton_Click(object sender, EventArgs e)
        {
            UI.AddMeregePdfFiles();
        }

        private void WordFileListView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.WordFileListView, e.Location);
            }
        }

        private void 删除本项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TempListview != null)
            {
                switch (TempListview.Name)
                {
                    case "WordFileListView":
                        UI.RemoveListViewItem(false, WordFileListView, WordFileListView.FocusedItem.Index);
                        break;
                    case "PdfFileListView":
                        UI.RemoveListViewItem(false, PdfFileListView, PdfFileListView.FocusedItem.Index);
                        break;
                    case "ExcelFileListView":
                        UI.RemoveListViewItem(false, ExcelFileListView, ExcelFileListView.FocusedItem.Index);
                        break;
                    case "DocumentsListView":
                        UI.RemoveListViewItem(false, DocumentsListView, DocumentsListView.FocusedItem.Index);
                        break;
                    case "PDF2ExcelFileView":
                        UI.RemoveListViewItem(false, PDF2ExcelFileView, PDF2ExcelFileView.FocusedItem.Index);
                        break;
                    case "MergePdfFileView":
                        UI.RemoveListViewItem(false, MergePdfFileView, MergePdfFileView.FocusedItem.Index);
                        break;
                    case "ReplaceWordFileList":
                        UI.RemoveListViewItem(false, ReplaceWordFileList, ReplaceWordFileList.FocusedItem.Index);
                        break;
                    case "ReplaceExcelFileView":
                        UI.RemoveListViewItem(false, ReplaceExcelFileView, ReplaceExcelFileView.FocusedItem.Index);
                        break;
                    case "DecodeMatrixWordListView":
                        UI.RemoveListViewItem(false, DecodeMatrixWordListView, DecodeMatrixWordListView.FocusedItem.Index);
                        break;
                    case "IcoFileView":
                        UI.RemoveListViewItem(false, IcoFileView, IcoFileView.FocusedItem.Index);
                        break;
                    default: break;
                }
            }
        }

        /// <summary>
        /// 右键菜单被打开的时候处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewRightMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //右键菜单被打开的时候，获取当前所在控件，作为listview给后续判断使用
            TempListview = ListViewRightMenu.SourceControl as ListView;
        }

        /// <summary>
        /// 各个listview的清空列表操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void 清空列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TempListview != null)
            {
                switch (TempListview.Name)
                {
                    case "WordFileListView":
                        UI.RemoveListViewItem(true, WordFileListView);
                        break;
                    case "PdfFileListView":
                        UI.RemoveListViewItem(true, PdfFileListView);
                        break;
                    case "ExcelFileListView":
                        UI.RemoveListViewItem(true, ExcelFileListView);
                        break;
                    case "DocumentsListView":
                        UI.RemoveListViewItem(true, DocumentsListView);
                        break;
                    case "PDF2ExcelFileView":
                        UI.RemoveListViewItem(true, PDF2ExcelFileView);
                        break;
                    case "MergePdfFileView":
                        UI.RemoveListViewItem(true, MergePdfFileView);
                        break;
                    case "ReplaceWordFileList":
                        UI.RemoveListViewItem(true, ReplaceWordFileList);
                        break;
                    case "ReplaceExcelFileView":
                        UI.RemoveListViewItem(true, ReplaceExcelFileView);
                        break;
                    case "DecodeMatrixWordListView":
                        UI.RemoveListViewItem(true, DecodeMatrixWordListView);
                        break;
                    case "IcoFileView":
                        UI.RemoveListViewItem(true, IcoFileView);
                        break;
                    default: break;
                }
            }

        }

        #region 各个控件触发鼠标单击事件处理函数
        private void PdfFileListView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.PdfFileListView, e.Location);
            }
        }

        private void ExcelFileListView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.ExcelFileListView, e.Location);
            }
        }

        private void PDF2ExcelFileView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.PDF2ExcelFileView, e.Location);
            }
        }

        private void MergePdfFileView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.MergePdfFileView, e.Location);
            }
        }

        private void DocumentsListView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.DocumentsListView, e.Location);
            }
        }

        private void ReplaceWordFileList_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.ReplaceWordFileList, e.Location);
            }
        }

        private void ReplaceExcelFileView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.ReplaceExcelFileView, e.Location);
            }
        }

        #endregion

        /// <summary>
        /// listview控件被拖入文件时的反馈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewDragEnter(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        /// <summary>
        /// 各个listview拖放文件进入时处理函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListviewDragDrop(object sender, DragEventArgs e)
        {
            ListView DragedListview = sender as ListView;
            string[] DropFiles = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            switch (DragedListview.Name)
            {
                case "WordFileListView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.WordFileListView, new string[] { ".doc", ".docx" });
                    break;
                case "PdfFileListView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.PdfFileListView, new string[] { ".pdf" });
                    break;
                case "ExcelFileListView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.ExcelFileListView, new string[] { ".xls", ".xlsx" });
                    break;
                case "DocumentsListView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.DocumentsListView, new string[] { ".doc", ".docx", ".pdf", ".xls", ".xlsx" });
                    break;
                case "PDF2ExcelFileView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.PDF2ExcelFileView, new string[] { ".pdf" });
                    break;
                case "MergePdfFileView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.MergePdfFileView, new string[] { ".pdf", ".jpg", ".png", ".bmp", ".tif" });
                    break;
                case "ReplaceWordFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.ReplaceWordFileList, new string[] { ".doc", ".docx" });
                    break;
                case "ReplaceExcelFileView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.ReplaceExcelFileView, new string[] { ".xls", ".xlsx" });
                    break;
                case "DecodeMatrixWordListView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.DecodeMatrixWordListView, new string[] { ".doc", ".docx" });
                    break;
                case "IcoFileView":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.IcoFileView, new string[] { ".jpg", ".png",".bmp" });
                    break;
                default: break;
            }
        }

        private void DownloadFrameWorkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net472");
        }

        #region Twain设备的初始化和扫描事件订阅过程代码
        private void InitTwain()
        {
            /**
             * 初始化Twain设备
             * Twain设备的调用比较特殊，一般分为下面三步：
             * 初始化Session、对Session进行Open、订阅SourceDisabled事件，在datasource使用完毕后进行Close操作
             * 这三步严格执行，不会触发任何异常，Session一定要保持开启
             * datasource可进行选择，或者使用默认DSM最后一次选中的作为datasource
             * datasource要Open后再进行Enable操作，也就是扫描或者调出扫描仪自身驱动的操作
             * datasource使用完毕后，订阅了SourceDisabled事件，就会自动进行Close
             * datasource不进行Close，在其他地方调用datasource会出现内存块错误，如果session被提前关闭，datasource将变为null
             * **/
            NTwain.PlatformInfo.Current.PreferNewDSM = false;
            TWIdentity tWIdentity = TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetExecutingAssembly());
            twainSession = new TwainSession(tWIdentity);
            twainSession.Open();
            twainSession.DataTransferred += Twain_Scanning;
            twainSession.SourceDisabled += (twainSender, twainArgs) =>
            {
                Debug.WriteLine("SourceDisabled Event");
                if (ScanSeconds > 0)
                {
                    this.ScanSpendTime.Text = $"本次扫描时间：{ScanSeconds}秒";
                }
                dataSource.Close();
                TwainStopScan = false;
            };

            twainSession.TransferReady += (twainSender, twainArgs) =>
            {
                Debug.WriteLine($"TransferReady Event：Stop_Scan_State={TwainStopScan}");
                twainArgs.CancelAll = TwainStopScan;
            };
        }

        private void SelectTwainDevice_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            dataSource = twainSession.ShowSourceSelector();
            if (dataSource != null)
            {
                if (dataSource.Open() == ReturnCode.Success)
                {
                    if (dataSource.Capabilities.ICapPixelType.IsSupported)
                    {
                        TwainColorMode.DataSource = dataSource.Capabilities.ICapPixelType.GetValues().ToList();
                        TwainColorMode.SelectedItem = dataSource.Capabilities.ICapPixelType.GetCurrent();

                    }
                    if (dataSource.Capabilities.ICapXResolution.IsSupported || dataSource.Capabilities.ICapYResolution.IsSupported)
                    {
                        TwainDpi.DataSource = dataSource.Capabilities.ICapXResolution.GetValues().ToList();
                        TwainDpi.SelectedItem = dataSource.Capabilities.ICapXResolution.GetCurrent();
                    }
                    if (dataSource.Capabilities.ICapSupportedSizes.IsSupported)
                    {
                        TwainPaperSizeBox.DataSource = dataSource.Capabilities.ICapSupportedSizes.GetValues().ToList();
                        TwainPaperSizeBox.SelectedItem = dataSource.Capabilities.ICapSupportedSizes.GetCurrent();

                    }
                    //输出扫描仪支持的设置参数
                    string DebugName = $"{dataSource.Name}_Support_Capabilities.txt";
                    if (File.Exists(DebugName))
                    {
                        File.Delete(DebugName);
                    }
                    dataSource.Capabilities.CapSupportedCaps.GetValues().ToList().ForEach(x => {
                        StreamWriter sr = File.AppendText(DebugName);
                        sr.WriteLine(x);
                        sr.Close();
                        }
                    );


                    if (dataSource.Capabilities.CapSerialNumber.CanGet)
                    {
                        ScanSerialNumber.Text = $"扫描仪序列号：{dataSource.Capabilities.CapSerialNumber.GetValues().FirstOrDefault()}";
                    }
                    dataSource.Close();
                }

            }
        }

        private void SetTwainDevice_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dataSource is null)
            {
                dataSource = twainSession.DefaultSource;
            }

            if (dataSource.Open() == ReturnCode.Success)
            {
                dataSource.Enable(SourceEnableMode.ShowUIOnly, false, this.Handle);
            }
        }

        private void StartScanButton_Click(object sender, EventArgs e)
        {
            TempImageCount = 0;
            ScanSeconds = 0;
            if (dataSource is null)
            {

                dataSource = twainSession.DefaultSource;
            }




            if (dataSource.Open() == ReturnCode.Success)
            {

                #region 扫描仪参数设置

                if (dataSource.Capabilities.ICapPixelType.CanSet)
                {
                    if (TwainColorMode.Items.Count > 0)
                    {
                        dataSource.Capabilities.ICapPixelType.SetValue((NTwain.Data.PixelType)TwainColorMode.SelectedItem);
                    }
                }
                if (dataSource.Capabilities.ICapXResolution.CanSet && dataSource.Capabilities.ICapYResolution.CanSet)
                {
                    if (TwainDpi.Items.Count > 0)
                    {
                        dataSource.Capabilities.ICapXResolution.SetValue((TWFix32)TwainDpi.SelectedItem);
                        dataSource.Capabilities.ICapYResolution.SetValue((TWFix32)TwainDpi.SelectedItem);
                    }
                }
                if (DoubleScanBox.Checked && dataSource.Capabilities.CapDuplex.IsSupported)
                {
                    dataSource.Capabilities.CapDuplexEnabled.SetValue(BoolType.True);
                }
                if (SkipBlankPage.Checked && dataSource.Capabilities.ICapAutoDiscardBlankPages.IsSupported)
                {
                    dataSource.Capabilities.ICapAutoDiscardBlankPages.SetValue(BlankPage.Auto);
                }
                if (dataSource.Capabilities.ICapSupportedSizes.CanSet)
                {
                    if (TwainPaperSizeBox.Items.Count > 0)
                    {
                        dataSource.Capabilities.ICapSupportedSizes.SetValue((SupportedSize)TwainPaperSizeBox.SelectedItem);
                    }
                }
                if(UseInterfaceUi.Checked)
                {
                    dataSource.Enable(SourceEnableMode.ShowUI, false, this.Handle);
                }
                else
                {
                    if (dataSource.Capabilities.CapUIControllable.IsSupported)
                    {
                        dataSource.Enable(SourceEnableMode.NoUI, false, this.Handle);
                    }
                }


                #endregion


            }


            if (this.ScanPictureSavePathBox.Text != string.Empty)
            {
                ImageIndex = Directory.GetFiles(this.ScanPictureSavePathBox.Text).Length;
            }

        }
        private void Twain_Scanning(object sender, DataTransferredEventArgs e)
        {

            using (Image TempScannedImage = Image.FromStream(e.GetNativeImageStream()))
            {
                if (this.ScanPictureSavePathBox.Text != string.Empty)
                {
                    Stopwatch sw = new Stopwatch();//监视器开始
                    sw.Start();
                    ++TempImageCount;//每次扫描的时候进入纸张数量，扫描完毕后需要在开始扫描按钮重置该值为0
                    ++TotalTempImageCount;//程序运行期间，扫描纸张的总数量，和上面的不同之处在于，点击开始扫描以后不进行重置值
                    string SaveImageName = System.IO.Path.Combine(ScanPictureSavePathBox.Text, $"ScanImage_{++ImageIndex}.jpg");//Imageindex为全局属性，值为目录内的文件数量，防止下次扫描原图片被覆盖
                    TempScannedImage.Save(SaveImageName, System.Drawing.Imaging.ImageFormat.Jpeg);
                    this.PreviewPictureName.Text = "图片名称：" + System.IO.Path.GetFileName(SaveImageName);

                    PictureBoxPreviewImage = new Bitmap(SaveImageName);
                    this.PreviewPictureBox.Image = PictureBoxPreviewImage;

                    this.NowScanNumber.Text = "本次扫描数量：" + TempImageCount.ToString();
                    this.ScannTotalNumber.Text = "扫描总数量：" + TotalTempImageCount.ToString();
                    this.SavedPathFileCount.Text = $"扫描路径内文件数量：{Directory.GetFiles(this.ScanPictureSavePathBox.Text).Length}";
                    sw.Stop();
                    ScanSeconds += sw.Elapsed.TotalSeconds;//一个扫描过程中花费的时间值，值将会在开始扫描按钮中重置为0
                    PictureBoxPreviewImage.Dispose();

                    if(TwainScanNumberSet.Text!=string.Empty && Int32.TryParse(TwainScanNumberSet.Text,out int TwainScanNumberSetInt))
                    {
                        if(TwainScanNumberSetInt>0 && TwainScanNumberSetInt== TempImageCount)
                        {
                            TwainStopScan = true;
                        }
                    }

                }
            }

        }

        private void SelectScanPath_Click(object sender, EventArgs e)
        {
            IEnumerable<string> IDirectoryString = Util.ShwoSelectDialog(false, true, "", "");
            string DirectoryString = IDirectoryString.FirstOrDefault();
            if (DirectoryString != default)
            {
                this.ScanPictureSavePathBox.Text = DirectoryString;
                this.SavedPathFileCount.Text = $"扫描路径内文件数量：{Directory.GetFiles(DirectoryString).Length}";
            }
        }

        private void MemoryTimer_Tick(object sender, EventArgs e)
        {
            this.MemoryStatusLabel.Text = Util.GetProcessUsedMemory();
        }

        private void RemoveAllFiles_Click(object sender, EventArgs e)
        {
            if (ScanPictureSavePathBox.Text != string.Empty)
            {
                string[] Files = Directory.GetFiles(ScanPictureSavePathBox.Text);
                if (Files.Length > 0)
                {
                    if (PictureBoxPreviewImage != null)
                    {
                        PictureBoxPreviewImage = null;
                        this.PreviewPictureBox.Image = null;
                    }
                    foreach (string file in Files)
                    {
                        File.Delete(file);
                        this.SavedPathFileCount.Text = $"扫描路径内文件数量：{Directory.GetFiles(this.ScanPictureSavePathBox.Text).Length}";
                    }
                }
            }
        }

        private void ResetTotalScanNumber_Click(object sender, EventArgs e)
        {
            this.TotalTempImageCount = 0;
            this.ScannTotalNumber.Text = "扫描总数量：0";
        }

        #endregion

        private void SelectReplaceWordFilesButton_Click(object sender, EventArgs e)
        {
            UI.AddWordRelaceFiles();
        }

        private void ReplaceWordContentButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(ReplaceWordFileList, ReplaceWordContentStatusText, ConvertFileType.WordReplace, ".docx");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });

        }

        private void TurialRegex_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://deerchao.cn/tutorials/regex/regex-1.htm");
        }

        private void OnlineRegextTool_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://c.runoob.com/front-end/854/");
        }

        private void SelectRelaceExcelButton_Click(object sender, EventArgs e)
        {
            UI.AddExcelRelaceFiles();
        }

        private void ReplaceExcelContentButton_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(ReplaceExcelFileView, ReplaceExcelStatusText, ConvertFileType.ExcelReplace, ".xlsx");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void ExcelRegexReadMe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://deerchao.cn/tutorials/regex/regex-1.htm");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://c.runoob.com/front-end/854/");
        }

        private void TabPage_12_MouseDown(object sender, MouseEventArgs e)
        {

        }

        private void DecodeDataMarix_Click(object sender, EventArgs e)
        {
            int DecodeCount = 0;
            DecodeDataMatrixView.Nodes.Clear();
            Task.Run(() =>
            {
                UI.GetListViewFirstItemContent(this.DecodeMatrixWordListView).Result.ForEach(FileName =>
                {
                    ++DecodeCount;
                    DecodeDataMatixStatusText.Text = $"正在解析第{DecodeCount}个文件";
                    Util.ReadDocumentDataMatrix(FileName, DecodeDataMatrixView);
                });
                DecodeDataMatixStatusText.Text = $"处理完毕";
            });


        }

        private void SelectDataMatrixWord_Click(object sender, EventArgs e)
        {
            UI.AddDataMattixWordFiles();
        }

        private void 展开折叠本节点全部ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode CurrentSelectNode = this.DecodeDataMatrixView.SelectedNode;
            if (CurrentSelectNode.IsExpanded)
            {
                CurrentSelectNode.Collapse();
            }
            else
            {
                CurrentSelectNode.ExpandAll();
            }
        }

        private void DecodeDataMatrixView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.TreeViewContentMenu.Show(this.DecodeDataMatrixView, e.Location);
            }
        }

        private void 展开折叠全部节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (TreeNode TempNode in DecodeDataMatrixView.Nodes)
            {
                if (TempNode.IsExpanded)
                {
                    TempNode.Collapse();
                }
                else
                {
                    TempNode.ExpandAll();
                }
            }
        }

        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (File.Exists(this.DecodeDataMatrixView.SelectedNode.Text)) Process.Start(this.DecodeDataMatrixView.SelectedNode.Text);

        }

        private void DecodeMatrixWordListView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.DecodeMatrixWordListView, e.Location);
            }
        }

        private void DownloadNetfx35_Click(object sender, EventArgs e)
        {
            string Netfx35_Private = "http://jdsnmsl.tpddns.cn:3002/directlink/default/Windows%E8%BD%AF%E4%BB%B6/GorunTime%E8%BF%90%E8%A1%8C%E5%BA%93%E5%90%88%E9%9B%86/GoRuntime_DotNetFramework_3.x.zip";
            Util.DwonloadFile(Netfx35_Private, "Net35.zip", Downloadfx35Progress, ProcessLabel);

        }

        private void LanzouNetfx_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://wwqb.lanzout.com/iVac90srv67a");
        }

        private void ReleaseRegistry_Click(object sender, EventArgs e)
        {
           object RegistryString =  Util.GetEmbedResourceObj("注册");

            File.WriteAllText("注册_Accusoft控件_右键管理员运行.reg", RegistryString.ToString());

            MessageBox.Show("释放完成","提示",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
        }

        private void label9_DoubleClick(object sender, EventArgs e)
        {
            if(ReleaseRegistry.Enabled)
            {
                ReleaseRegistry.Enabled = false;
            }
            else
            {
                ReleaseRegistry.Enabled = true;
            }

        }

        private void SelectFilesIco_Click(object sender, EventArgs e)
        {
            UI.AddIcoConvertFiles();
        }

        private void ConvertImage2Ico_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.IcoFileView, this.Image2IcoStatusText, ConvertFileType.Image2Ico, ".ico");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void IcoFileView_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.IcoFileView, e.Location);
            }
        }
    }

}
