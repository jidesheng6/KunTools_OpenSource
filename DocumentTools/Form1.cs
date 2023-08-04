using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

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
        private List<Bitmap> TempScannedImageBmpList = new List<Bitmap>();
        private IKeyboardEvents GlobalHook = Hook.GlobalEvents();//全局钩子事件

        public bool NameReplaceMode = false;
        public string ShowOpenTargetDialog = "on";//是否启用完成后显示打开转换目录弹窗
        public string AddWaterMarkBackGroudImageFileName = string.Empty;

        string NowVersion = "23.8.5";

        #region 控件大小随窗体大小等比例缩放

        private readonly float x; //定义当前窗体的宽度
        private readonly float y; //定义当前窗体的高度

        private void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ";" + con.Height + ";" + con.Left + ";" + con.Top + ";" + con.Font.Size;
                if (con.Controls.Count > 0) setTag(con);
            }
        }

        private void setControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
                //获取控件的Tag属性值，并分割后存储字符串数组
                if (con.Tag != null)
                {
                    var mytag = con.Tag.ToString().Split(';');
                    //根据窗体缩放的比例确定控件的值
                    con.Width = Convert.ToInt32(Convert.ToSingle(mytag[0]) * newx); //宽度
                    con.Height = Convert.ToInt32(Convert.ToSingle(mytag[1]) * newy); //高度
                    con.Left = Convert.ToInt32(Convert.ToSingle(mytag[2]) * newx); //左边距
                    con.Top = Convert.ToInt32(Convert.ToSingle(mytag[3]) * newy); //顶边距
                    var currentSize = Convert.ToSingle(mytag[4]) * newy; //字体大小                   
                    if (currentSize > 0) con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    con.Focus();
                    if (con.Controls.Count > 0) setControls(newx, newy, con);
                }
        }


        /// <summary>
        /// 重置窗体布局
        /// </summary>
        private void ReWinformLayout()
        {
            var newx = Width / x;
            var newy = Height / y;
            setControls(newx, newy, this);

        }

        #endregion


        private void OutPutListViewIndex()
        {
            foreach (TabPage TempPage in this.MainTabPage.TabPages)
            {
                Debug.WriteLine($"A：\n{TempPage.Text}：{MainTabPage.TabPages.IndexOf(TempPage)}");
                Debug.WriteLine($"B：\n{TempPage.Text}：{MainTabPage.TabPages.IndexOf(TempPage)+1}");
            }
        }

        private void SetInitTabPage()
        {
            MainTabPage.SelectedIndex = int.Parse(ConfigurationManager.AppSettings["StartPageIndex"]);
        }

        private void SetTabCardHeighLight()
        {
            string OpenTabPageHeight = ConfigurationManager.AppSettings["TabPageHeight"];
            this.MainTabPage.DrawMode = OpenTabPageHeight == "on" ? TabDrawMode.OwnerDrawFixed : TabDrawMode.Normal;

        }

        private void SetConfirmDialogShow()
        {
            ShowOpenTargetDialog = ConfigurationManager.AppSettings["ShowOpenTargetDialog"];
        }

        private void SetTabPageVisable()
        {
            List<string> DisablePageIndexList = ConfigurationManager.AppSettings["HidePageIndex"].Split(',').ToList();

            foreach (string DisablePageIndex in DisablePageIndexList)
            {
                if (DisablePageIndex != string.Empty)
                {
                    if (int.TryParse(DisablePageIndex, out int PageIndex))
                    {
                        if (PageIndex >= 0 & PageIndex <= MainTabPage.TabCount-1)
                        {
                            MainTabPage.TabPages[PageIndex].Parent = null;
                        }
                    }
                }
            }
        }

        private void InitConfigSettings()
        {
            //如果配置文件存在就按照配置文件中的参数来设置
            if (File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
            {
                SetInitTabPage();//设置起始页面
                SetTabCardHeighLight();//设置标签卡高亮
                SetConfirmDialogShow();//设置是否显示转换后弹窗
                SetTabPageVisable();//设置标签卡隐藏
            }

        }

        public BodyForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            MainForm = this;

            x = Width;
            y = Height;
            setTag(this);



        }
        private void BodyForm_Load(object sender, EventArgs e)
        {
            this.Text = $"坤_Tools【软件完全离线|更新请看关于软件|版本：{NowVersion}】";
            InitListView();
            InitComboxValue();
            InitMoveItemButtonHandler();
            InitRemoveItemHandler();
            InitToolTipHandler();
            InitConfigSettings();
            OutPutListViewIndex();
            this.AllowDrop = true;
            this.PageExportCheck.Checked = true;
            this.PageExportCheck.Enabled = false;

            this.MergePdfButton.Enabled = false;

            this.PreviewPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            this.ScanPictureSavePathBox.Enabled = false;
            this.DecodeDataMatrixView.ShowLines = true;
            this.DecodeDataMatrixView.ShowRootLines = true;
            this.DecodeDataMatrixView.FullRowSelect = true;
            this.CompressImage_OutPutImageFormat.Enabled = false;
            this.IcoSizeDrop.Enabled = false;
            //ReleaseRegistry.Enabled = false;
            this.Image2PDFCOmpressImage.Enabled = false;

            //Util.PlayMp3WithStream();

            ShortStatus.Text = Util.GetShortStatus();
            ShortStatus.ForeColor = ShortStatus.Text=="已移除"? Color.Red: Color.Green;

            try
            {
                InitTwain();
            }
            catch
            {
                MessageBox.Show("没有在系统中找到TwainDsm数据源管理器，请自行下载TwainDsm.dll文件放在程序目录下调用，否则无法使用twain调试功能", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            this.WaterMarkImageHeight.Validating += ValidTextBoxContent;
            this.WaterMarkImageHeight.Validated += ValidedTextBoxContent;

            this.WaterMarkImageWidth.Validating += ValidTextBoxContent;
            this.WaterMarkImageWidth.Validated += ValidedTextBoxContent;

            this.PdfWaterMarkRotateDegNumber.Validating += ValidTextBoxContent;
            this.PdfWaterMarkRotateDegNumber.Validated += ValidedTextBoxContent;

            this.ImagePdfPageSIze_UP.Validating += ValidTextBoxContent;
            this.ImagePdfPageSIze_UP.Validated += ValidedTextBoxContent;

            this.ImagePdfPageSIze_LEFT.Validating += ValidTextBoxContent;
            this.ImagePdfPageSIze_LEFT.Validated += ValidedTextBoxContent;

            this.ImagePdfPageSIze_BOTTOM.Validating += ValidTextBoxContent;
            this.ImagePdfPageSIze_BOTTOM.Validated += ValidedTextBoxContent;

            this.ImagePdfPageSIze_RIGHT.Validating += ValidTextBoxContent;
            this.ImagePdfPageSIze_RIGHT.Validated += ValidedTextBoxContent;



        }


        //初始化Listview
        private void InitListView()
        {
            // WordFileListView
            this.WordFileListView.View = View.Details;
            this.WordFileListView.FullRowSelect = true;
            this.WordFileListView.AllowDrop = true;
            this.WordFileListView.MultiSelect = false;
            this.WordFileListView.Columns.Add("文件名称", 300);
            this.WordFileListView.Columns.Add("添加时间", 300);
            this.WordFileListView.Columns.Add("转换后文件名称", 300);
            this.WordFileListView.Columns.Add("文件大小", 300);
            this.WordFileListView.Sorting = SortOrder.Ascending;
            this.WordFileListView.Name = "WordFileListView";

            this.WordFileListView.DragEnter += ListViewDragEnter;
            this.WordFileListView.DragDrop += ListviewDragDrop;
            this.WordFileListView.ColumnClick += ListviewSortHandler;

            //PdfFileListView
            this.PdfFileListView.View = View.Details;
            this.PdfFileListView.FullRowSelect = true;
            this.PdfFileListView.MultiSelect = false;
            this.PdfFileListView.AllowDrop = true;
            this.PdfFileListView.Columns.Add("文件名称", 300);
            this.PdfFileListView.Columns.Add("添加时间", 300);
            this.PdfFileListView.Columns.Add("转换后文件名称", 300);
            this.PdfFileListView.Columns.Add("文件大小", 300);
            this.PdfFileListView.Sorting = SortOrder.Ascending;
            this.PdfFileListView.Name = "PdfFileListView";

            this.PdfFileListView.DragEnter += ListViewDragEnter;
            this.PdfFileListView.DragDrop += ListviewDragDrop;
            this.PdfFileListView.ColumnClick += ListviewSortHandler;

            //ExcelFileListView
            this.ExcelFileListView.View = View.Details;
            this.ExcelFileListView.FullRowSelect = true;
            this.ExcelFileListView.AllowDrop = true;
            this.ExcelFileListView.MultiSelect = false;
            this.ExcelFileListView.Columns.Add("文件名称", 300);
            this.ExcelFileListView.Columns.Add("添加时间", 300);
            this.ExcelFileListView.Columns.Add("转换后文件名称", 300);
            this.ExcelFileListView.Columns.Add("文件大小", 300);
            this.ExcelFileListView.Sorting = SortOrder.Ascending;
            this.ExcelFileListView.Name = "ExcelFileListView";

            this.ExcelFileListView.DragEnter += ListViewDragEnter;
            this.ExcelFileListView.DragDrop += ListviewDragDrop;
            this.ExcelFileListView.ColumnClick += ListviewSortHandler;

            //DocumentsListView
            this.DocumentsListView.View = View.Details;
            this.DocumentsListView.FullRowSelect = true;
            this.DocumentsListView.MultiSelect = false;
            this.DocumentsListView.AllowDrop = true;
            this.DocumentsListView.Columns.Add("文件名称", 300);
            this.DocumentsListView.Columns.Add("添加时间", 300);
            this.DocumentsListView.Columns.Add("转换后文件名称", 300);
            this.DocumentsListView.Columns.Add("文件大小", 300);
            this.DocumentsListView.Sorting = SortOrder.Ascending;
            this.DocumentsListView.Name = "DocumentsListView";

            this.DocumentsListView.DragEnter += ListViewDragEnter;
            this.DocumentsListView.DragDrop += ListviewDragDrop;
            this.DocumentsListView.ColumnClick += ListviewSortHandler;

            //PDF2ExcelFileView
            this.PDF2ExcelFileView.View = View.Details;
            this.PDF2ExcelFileView.FullRowSelect = true;
            this.PDF2ExcelFileView.MultiSelect = false;
            this.PDF2ExcelFileView.AllowDrop = true;
            this.PDF2ExcelFileView.Columns.Add("文件名称", 300);
            this.PDF2ExcelFileView.Columns.Add("添加时间", 300);
            this.PDF2ExcelFileView.Columns.Add("转换后文件名称", 300);
            this.PDF2ExcelFileView.Columns.Add("文件大小", 300);
            this.PDF2ExcelFileView.Sorting = SortOrder.Ascending;
            this.PDF2ExcelFileView.Name = "PDF2ExcelFileView";

            this.PDF2ExcelFileView.DragEnter += ListViewDragEnter;
            this.PDF2ExcelFileView.DragDrop += ListviewDragDrop;
            this.PDF2ExcelFileView.ColumnClick += ListviewSortHandler;

            //MergePdfFileView
            this.MergePdfFileView.View = View.Details;
            this.MergePdfFileView.FullRowSelect = true;
            this.MergePdfFileView.MultiSelect = false;
            this.MergePdfFileView.AllowDrop = true;
            this.MergePdfFileView.Columns.Add("文件名称", 300);
            this.MergePdfFileView.Columns.Add("添加时间", 300);
            this.MergePdfFileView.Columns.Add("转换后文件名称", 300);
            this.MergePdfFileView.Columns.Add("文件大小", 300);
            this.MergePdfFileView.Sorting = SortOrder.Ascending;
            this.MergePdfFileView.Name = "MergePdfFileView";

            this.MergePdfFileView.DragEnter += ListViewDragEnter;
            this.MergePdfFileView.DragDrop += ListviewDragDrop;
            this.MergePdfFileView.ColumnClick += ListviewSortHandler;


            //ReplaceWordFileList
            this.ReplaceWordFileList.View = View.Details;
            this.ReplaceWordFileList.FullRowSelect = true;
            this.ReplaceWordFileList.MultiSelect = false;
            this.ReplaceWordFileList.AllowDrop = true;
            this.ReplaceWordFileList.Columns.Add("文件名称", 300);
            this.ReplaceWordFileList.Columns.Add("添加时间", 300);
            this.ReplaceWordFileList.Columns.Add("转换后文件名称", 300);
            this.ReplaceWordFileList.Columns.Add("文件大小", 300);
            this.ReplaceWordFileList.Sorting = SortOrder.Ascending;
            this.ReplaceWordFileList.Name = "ReplaceWordFileList";

            this.ReplaceWordFileList.DragEnter += ListViewDragEnter;
            this.ReplaceWordFileList.DragDrop += ListviewDragDrop;
            this.ReplaceWordFileList.ColumnClick += ListviewSortHandler;

            //ReplaceExcelFileView
            this.ReplaceExcelFileView.View = View.Details;
            this.ReplaceExcelFileView.FullRowSelect = true;
            this.ReplaceExcelFileView.MultiSelect = false;
            this.ReplaceExcelFileView.AllowDrop = true;
            this.ReplaceExcelFileView.Columns.Add("文件名称", 300);
            this.ReplaceExcelFileView.Columns.Add("添加时间", 300);
            this.ReplaceExcelFileView.Columns.Add("转换后文件名称", 300);
            this.ReplaceExcelFileView.Columns.Add("文件大小", 300);
            this.ReplaceExcelFileView.Sorting = SortOrder.Ascending;
            this.ReplaceExcelFileView.Name = "ReplaceExcelFileView";

            this.ReplaceExcelFileView.DragEnter += ListViewDragEnter;
            this.ReplaceExcelFileView.DragDrop += ListviewDragDrop;
            this.ReplaceExcelFileView.ColumnClick += ListviewSortHandler;

            //DecodeMatrixWordListView
            this.DecodeMatrixWordListView.View = View.Details;
            this.DecodeMatrixWordListView.FullRowSelect = true;
            this.DecodeMatrixWordListView.MultiSelect = false;
            this.DecodeMatrixWordListView.AllowDrop = true;
            this.DecodeMatrixWordListView.Columns.Add("文件名称", 300);
            this.DecodeMatrixWordListView.Columns.Add("添加时间", 300);
            this.DecodeMatrixWordListView.Columns.Add("转换后文件名称", 300);
            this.DecodeMatrixWordListView.Columns.Add("文件大小", 300);
            this.DecodeMatrixWordListView.Sorting = SortOrder.Ascending;
            this.DecodeMatrixWordListView.Name = "DecodeMatrixWordListView";

            this.DecodeMatrixWordListView.DragEnter += ListViewDragEnter;
            this.DecodeMatrixWordListView.DragDrop += ListviewDragDrop;
            this.DecodeMatrixWordListView.ColumnClick += ListviewSortHandler;

            //IcoFileView
            this.IcoFileView.View = View.Details;
            this.IcoFileView.FullRowSelect = true;
            this.IcoFileView.MultiSelect = false;
            this.IcoFileView.AllowDrop = true;
            this.IcoFileView.Columns.Add("文件名称", 300);
            this.IcoFileView.Columns.Add("添加时间", 300);
            this.IcoFileView.Columns.Add("转换后文件名称", 300);
            this.IcoFileView.Columns.Add("文件大小", 300);
            this.IcoFileView.Sorting = SortOrder.Ascending;
            this.IcoFileView.Name = "IcoFileView";

            this.IcoFileView.DragEnter += ListViewDragEnter;
            this.IcoFileView.DragDrop += ListviewDragDrop;
            this.IcoFileView.ColumnClick += ListviewSortHandler;


            // MergeWordFileList
            this.MergeWordFileList.View = View.Details;
            this.MergeWordFileList.FullRowSelect = true;
            this.MergeWordFileList.MultiSelect = false;
            this.MergeWordFileList.AllowDrop = true;
            this.MergeWordFileList.Columns.Add("文件名称", 300);
            this.MergeWordFileList.Columns.Add("添加时间", 300);
            this.MergeWordFileList.Columns.Add("转换后文件名称", 300);
            this.MergeWordFileList.Columns.Add("文件大小", 300);
            this.MergeWordFileList.Sorting = SortOrder.Ascending;
            this.MergeWordFileList.Name = "MergeWordFileList";

            this.MergeWordFileList.DragEnter += ListViewDragEnter;
            this.MergeWordFileList.DragDrop += ListviewDragDrop;
            this.MergeWordFileList.ColumnClick += ListviewSortHandler;

            // Word2ExcelFileList
            this.Word2ExcelFileList.View = View.Details;
            this.Word2ExcelFileList.FullRowSelect = true;
            this.Word2ExcelFileList.MultiSelect = false;
            this.Word2ExcelFileList.AllowDrop = true;
            this.Word2ExcelFileList.Columns.Add("文件名称", 300);
            this.Word2ExcelFileList.Columns.Add("添加时间", 300);
            this.Word2ExcelFileList.Columns.Add("转换后文件名称", 300);
            this.Word2ExcelFileList.Columns.Add("文件大小", 300);
            this.Word2ExcelFileList.Sorting = SortOrder.Ascending;
            this.Word2ExcelFileList.Name = "Word2ExcelFileList";

            this.Word2ExcelFileList.DragEnter += ListViewDragEnter;
            this.Word2ExcelFileList.DragDrop += ListviewDragDrop;
            this.Word2ExcelFileList.ColumnClick += ListviewSortHandler;

            // Word2ExcelFileList
            this.Excel2WordFileList.View = View.Details;
            this.Excel2WordFileList.FullRowSelect = true;
            this.Excel2WordFileList.MultiSelect = false;
            this.Excel2WordFileList.AllowDrop = true;
            this.Excel2WordFileList.Columns.Add("文件名称", 300);
            this.Excel2WordFileList.Columns.Add("添加时间", 300);
            this.Excel2WordFileList.Columns.Add("转换后文件名称", 300);
            this.Excel2WordFileList.Columns.Add("文件大小", 300);
            this.Excel2WordFileList.Sorting = SortOrder.Ascending;
            this.Excel2WordFileList.Name = "Excel2WordFileList";

            this.Excel2WordFileList.DragEnter += ListViewDragEnter;
            this.Excel2WordFileList.DragDrop += ListviewDragDrop;
            this.Excel2WordFileList.ColumnClick += ListviewSortHandler;

            // SplitPdfFileList
            this.SplitPdfFileList.View = View.Details;
            this.SplitPdfFileList.FullRowSelect = true;
            this.SplitPdfFileList.MultiSelect = false;
            this.SplitPdfFileList.AllowDrop = true;
            this.SplitPdfFileList.Columns.Add("文件名称", 300);
            this.SplitPdfFileList.Columns.Add("添加时间", 300);
            this.SplitPdfFileList.Columns.Add("转换后文件名称", 300);
            this.SplitPdfFileList.Columns.Add("文件大小", 300);
            this.SplitPdfFileList.Sorting = SortOrder.Ascending;
            this.SplitPdfFileList.Name = "SplitPdfFileList";

            this.SplitPdfFileList.DragEnter += ListViewDragEnter;
            this.SplitPdfFileList.DragDrop += ListviewDragDrop;
            this.SplitPdfFileList.ColumnClick += ListviewSortHandler;

            // PdfWaterMarkFileList
            this.PdfWaterMarkFileList.View = View.Details;
            this.PdfWaterMarkFileList.FullRowSelect = true;
            this.PdfWaterMarkFileList.MultiSelect = false;
            this.PdfWaterMarkFileList.AllowDrop = true;
            this.PdfWaterMarkFileList.Columns.Add("文件名称", 300);
            this.PdfWaterMarkFileList.Columns.Add("添加时间", 300);
            this.PdfWaterMarkFileList.Columns.Add("转换后文件名称", 300);
            this.PdfWaterMarkFileList.Columns.Add("文件大小", 300);
            this.PdfWaterMarkFileList.Sorting = SortOrder.Ascending;
            this.PdfWaterMarkFileList.Name = "PdfWaterMarkFileList";

            this.PdfWaterMarkFileList.DragEnter += ListViewDragEnter;
            this.PdfWaterMarkFileList.DragDrop += ListviewDragDrop;
            this.PdfWaterMarkFileList.ColumnClick += ListviewSortHandler;

            // RemovePdfWaterMarkFileList
            this.RemovePdfWaterMarkFileList.View = View.Details;
            this.RemovePdfWaterMarkFileList.FullRowSelect = true;
            this.RemovePdfWaterMarkFileList.MultiSelect = false;
            this.RemovePdfWaterMarkFileList.AllowDrop = true;
            this.RemovePdfWaterMarkFileList.Columns.Add("文件名称", 300);
            this.RemovePdfWaterMarkFileList.Columns.Add("添加时间", 300);
            this.RemovePdfWaterMarkFileList.Columns.Add("转换后文件名称", 300);
            this.RemovePdfWaterMarkFileList.Columns.Add("文件大小", 300);
            this.RemovePdfWaterMarkFileList.Sorting = SortOrder.Ascending;
            this.RemovePdfWaterMarkFileList.Name = "RemovePdfWaterMarkFileList";

            this.RemovePdfWaterMarkFileList.DragEnter += ListViewDragEnter;
            this.RemovePdfWaterMarkFileList.DragDrop += ListviewDragDrop;
            this.RemovePdfWaterMarkFileList.ColumnClick += ListviewSortHandler;


            // PPT2PdfFileList
            this.PPT2PdfFileList.View = View.Details;
            this.PPT2PdfFileList.FullRowSelect = true;
            this.PPT2PdfFileList.MultiSelect = false;
            this.PPT2PdfFileList.AllowDrop = true;
            this.PPT2PdfFileList.Columns.Add("文件名称", 300);
            this.PPT2PdfFileList.Columns.Add("添加时间", 300);
            this.PPT2PdfFileList.Columns.Add("转换后文件名称", 300);
            this.PPT2PdfFileList.Columns.Add("文件大小", 300);
            this.PPT2PdfFileList.Sorting = SortOrder.Ascending;
            this.PPT2PdfFileList.Name = "PPT2PdfFileList";

            this.PPT2PdfFileList.DragEnter += ListViewDragEnter;
            this.PPT2PdfFileList.DragDrop += ListviewDragDrop;
            this.PPT2PdfFileList.ColumnClick += ListviewSortHandler;

            // PDF2PPTFileList
            this.PDF2PPTFileList.View = View.Details;
            this.PDF2PPTFileList.FullRowSelect = true;
            this.PDF2PPTFileList.MultiSelect = false;
            this.PDF2PPTFileList.AllowDrop = true;
            this.PDF2PPTFileList.Columns.Add("文件名称", 300);
            this.PDF2PPTFileList.Columns.Add("添加时间", 300);
            this.PDF2PPTFileList.Columns.Add("转换后文件名称", 300);
            this.PDF2PPTFileList.Columns.Add("文件大小", 300);
            this.PDF2PPTFileList.Sorting = SortOrder.Ascending;
            this.PDF2PPTFileList.Name = "PDF2PPTFileList";

            this.PDF2PPTFileList.DragEnter += ListViewDragEnter;
            this.PDF2PPTFileList.DragDrop += ListviewDragDrop;
            this.PDF2PPTFileList.ColumnClick += ListviewSortHandler;

            // ConvertPdfComplianceFileList
            this.ConvertPdfComplianceFileList.View = View.Details;
            this.ConvertPdfComplianceFileList.FullRowSelect = true;
            this.ConvertPdfComplianceFileList.MultiSelect = false;
            this.ConvertPdfComplianceFileList.AllowDrop = true;
            this.ConvertPdfComplianceFileList.Columns.Add("文件名称", 300);
            this.ConvertPdfComplianceFileList.Columns.Add("添加时间", 300);
            this.ConvertPdfComplianceFileList.Columns.Add("转换后文件名称", 300);
            this.ConvertPdfComplianceFileList.Columns.Add("文件大小", 300);
            this.ConvertPdfComplianceFileList.Sorting = SortOrder.Ascending;
            this.ConvertPdfComplianceFileList.Name = "ConvertPdfComplianceFileList";

            this.ConvertPdfComplianceFileList.DragEnter += ListViewDragEnter;
            this.ConvertPdfComplianceFileList.DragDrop += ListviewDragDrop;
            this.ConvertPdfComplianceFileList.ColumnClick += ListviewSortHandler;

            // ConvertImage2PDFFileList
            this.ConvertImage2PDFFileList.View = View.Details;
            this.ConvertImage2PDFFileList.FullRowSelect = true;
            this.ConvertImage2PDFFileList.MultiSelect = false;
            this.ConvertImage2PDFFileList.AllowDrop = true;
            this.ConvertImage2PDFFileList.Columns.Add("文件名称", 300);
            this.ConvertImage2PDFFileList.Columns.Add("添加时间", 300);
            this.ConvertImage2PDFFileList.Columns.Add("转换后文件名称", 300);
            this.ConvertImage2PDFFileList.Columns.Add("文件大小", 300);
            this.ConvertImage2PDFFileList.Sorting = SortOrder.Ascending;
            this.ConvertImage2PDFFileList.Name = "ConvertImage2PDFFileList";

            this.ConvertImage2PDFFileList.DragEnter += ListViewDragEnter;
            this.ConvertImage2PDFFileList.DragDrop += ListviewDragDrop;
            this.ConvertImage2PDFFileList.ColumnClick += ListviewSortHandler;

            // CompressPdfFileList
            this.CompressPdfFileList.View = View.Details;
            this.CompressPdfFileList.FullRowSelect = true;
            this.CompressPdfFileList.MultiSelect = false;
            this.CompressPdfFileList.AllowDrop = true;
            this.CompressPdfFileList.Columns.Add("文件名称", 300);
            this.CompressPdfFileList.Columns.Add("添加时间", 300);
            this.CompressPdfFileList.Columns.Add("转换后文件名称", 300);
            this.CompressPdfFileList.Columns.Add("文件大小", 300);
            this.CompressPdfFileList.Sorting = SortOrder.Ascending;
            this.CompressPdfFileList.Name = "CompressPdfFileList";

            this.CompressPdfFileList.DragEnter += ListViewDragEnter;
            this.CompressPdfFileList.DragDrop += ListviewDragDrop;
            this.CompressPdfFileList.ColumnClick += ListviewSortHandler;

            // CompressImageFileList
            this.CompressImageFileList.View = View.Details;
            this.CompressImageFileList.FullRowSelect = true;
            this.CompressImageFileList.MultiSelect = false;
            this.CompressImageFileList.AllowDrop = true;
            this.CompressImageFileList.Columns.Add("文件名称", 300);
            this.CompressImageFileList.Columns.Add("添加时间", 300);
            this.CompressImageFileList.Columns.Add("转换后文件名称", 300);
            this.CompressImageFileList.Columns.Add("文件大小", 300);
            this.CompressImageFileList.Sorting = SortOrder.Ascending;
            this.CompressImageFileList.Name = "CompressImageFileList";

            this.CompressImageFileList.DragEnter += ListViewDragEnter;
            this.CompressImageFileList.DragDrop += ListviewDragDrop;
            this.CompressImageFileList.ColumnClick += ListviewSortHandler;
        }

        //初始化ComBox数据
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


            DataTable PdfWaterMarkHorSettingTable = new DataTable();

            PdfWaterMarkHorSettingTable.Columns.Add("DisplayValue");
            PdfWaterMarkHorSettingTable.Columns.Add("RealValue");
            PdfWaterMarkHorSettingTable.Rows.Add("不对齐", Aspose.Pdf.HorizontalAlignment.None);
            PdfWaterMarkHorSettingTable.Rows.Add("左对齐", Aspose.Pdf.HorizontalAlignment.Left);
            PdfWaterMarkHorSettingTable.Rows.Add("居中对齐", Aspose.Pdf.HorizontalAlignment.Center);
            PdfWaterMarkHorSettingTable.Rows.Add("右对齐", Aspose.Pdf.HorizontalAlignment.Right);
            PdfWaterMarkHorSettingTable.Rows.Add("未知对齐方式1", Aspose.Pdf.HorizontalAlignment.Justify);
            PdfWaterMarkHorSettingTable.Rows.Add("未知对齐方式2", Aspose.Pdf.HorizontalAlignment.FullJustify);

            this.PdfWaterMarkHorSetting.DataSource = PdfWaterMarkHorSettingTable;
            this.PdfWaterMarkHorSetting.DisplayMember = "DisplayValue";
            this.PdfWaterMarkHorSetting.ValueMember = "RealValue";

            this.PdfWaterMarkHorSetting.SelectedIndex = 2;


            DataTable PdfWaterMarkVerSettingTable = new DataTable();
            PdfWaterMarkVerSettingTable.Columns.Add("DisplayValue");
            PdfWaterMarkVerSettingTable.Columns.Add("RealValue");
            PdfWaterMarkVerSettingTable.Rows.Add("不对齐", Aspose.Pdf.VerticalAlignment.None);
            PdfWaterMarkVerSettingTable.Rows.Add("顶部对齐", Aspose.Pdf.VerticalAlignment.Top);
            PdfWaterMarkVerSettingTable.Rows.Add("居中对齐", Aspose.Pdf.VerticalAlignment.Center);
            PdfWaterMarkVerSettingTable.Rows.Add("底部对齐", Aspose.Pdf.VerticalAlignment.Bottom);

            this.PDFWaterMarkVerSetting.DataSource = PdfWaterMarkVerSettingTable;
            this.PDFWaterMarkVerSetting.DisplayMember = "DisplayValue";
            this.PDFWaterMarkVerSetting.ValueMember = "RealValue";

            this.PDFWaterMarkVerSetting.SelectedIndex = 2;

            this.PdfWaterMarkOpcityNumber.DataSource = new float[] { 0.0f, 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f };
            this.PdfWaterMarkOpcityNumber.FormatString = "F1"; //控制文本格式，这里显示小数点后1位
            this.PdfWaterMarkOpcityNumber.SelectedIndex = 5;



            this.PDFComplianceBox.DataSource = Enum.GetNames(typeof(Aspose.Words.Saving.PdfCompliance));


            DataTable ExportNotePageData = new DataTable();
            ExportNotePageData.Columns.Add("DisplayValue");
            ExportNotePageData.Columns.Add("RealValue");
            ExportNotePageData.Rows.Add("不导出", Aspose.Slides.Export.NotesPositions.None);
            ExportNotePageData.Rows.Add("自动调整", Aspose.Slides.Export.NotesPositions.BottomFull);
            ExportNotePageData.Rows.Add("强制单页", Aspose.Slides.Export.NotesPositions.BottomTruncated);

            this.ExportNotePageCombox.DataSource = ExportNotePageData;
            this.ExportNotePageCombox.DisplayMember = "DisplayValue";
            this.ExportNotePageCombox.ValueMember = "RealValue";


            this.ConvertPdfComplianceVersion.DataSource = Enum.GetNames(typeof(Aspose.Pdf.PdfFormat));

            DataTable Image2PDFPageStyle = new DataTable();
            Image2PDFPageStyle.Columns.Add("DisplayValue");
            Image2PDFPageStyle.Columns.Add("RealValue");
            Image2PDFPageStyle.Rows.Add("横排", true);
            Image2PDFPageStyle.Rows.Add("竖排", false);

            this.Image2PDFCombox.DataSource = Image2PDFPageStyle;
            this.Image2PDFCombox.DisplayMember = "DisplayValue";
            this.Image2PDFCombox.ValueMember = "RealValue";

            this.CompressPDF_ImageQuailty.DataSource = Enumerable.Range(0, 101).ToList();

            DataTable CompressImageVersion = new DataTable();
            CompressImageVersion.Columns.Add("DisplayValue");
            CompressImageVersion.Columns.Add("RealValue");
            CompressImageVersion.Rows.Add("标准", Aspose.Pdf.Optimization.ImageCompressionVersion.Standard);
            CompressImageVersion.Rows.Add("快速", Aspose.Pdf.Optimization.ImageCompressionVersion.Fast);
            CompressImageVersion.Rows.Add("混合", Aspose.Pdf.Optimization.ImageCompressionVersion.Mixed);

            this.CompressPDF_ImageCompressVersion.DataSource = CompressImageVersion;
            this.CompressPDF_ImageCompressVersion.DisplayMember = "DisplayValue";
            this.CompressPDF_ImageCompressVersion.ValueMember = "RealValue";

            this.CompressPDF_ImageResoution.DataSource = Enumerable.Range(0, 600).Where(x => x % 50 == 0).ToList();

            this.CompressJpegMode_Combox.DataSource = Enum.GetNames(typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionMode));

            this.CompressJpegColorMode_Combox.DataSource = Enum.GetNames(typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionColorMode));
            this.CompressJpegColorMode_Combox.SelectedIndex = 4;

            this.CompressJpegQuailty_Combox.DataSource = Enumerable.Range(1, 100).ToList();
            this.CompressJpegQuailty_Combox.SelectedIndex = 99;

            this.CompressPngLevel_Combox.DataSource = Enumerable.Range(0, 10).ToList();

            this.CompressPngColorMode_Combox.DataSource = Enum.GetNames(typeof(Aspose.Imaging.FileFormats.Png.PngColorType));
            this.CompressPngColorMode_Combox.SelectedIndex = 1;

            this.CompressBmpMode_Combox.DataSource = Enum.GetNames(typeof(Aspose.Imaging.FileFormats.Bmp.BitmapCompression));


            this.CompressBmpPixeBits_Combox.DataSource = new int[] { 4, 8, 16, 24, 32 };//BMP像素深度范围
            this.CompressBmpPixeBits_Combox.SelectedIndex = 3;

            DataTable CompressImage_OutPutFormat = new DataTable();
            CompressImage_OutPutFormat.Columns.Add("DisplayValue");
            CompressImage_OutPutFormat.Columns.Add("RealValue");
            CompressImage_OutPutFormat.Rows.Add("JPG", ".jpg");
            CompressImage_OutPutFormat.Rows.Add("JPEG", ".jpeg");
            CompressImage_OutPutFormat.Rows.Add("PNG", ".png");
            CompressImage_OutPutFormat.Rows.Add("BMP", ".bmp");

            this.CompressImage_OutPutImageFormat.DataSource = CompressImage_OutPutFormat;
            this.CompressImage_OutPutImageFormat.DisplayMember = "DisplayValue";
            this.CompressImage_OutPutImageFormat.ValueMember = "RealValue";

            DataTable PDF2WORDMOde = new DataTable();
            PDF2WORDMOde.Columns.Add("DisplayValue");
            PDF2WORDMOde.Columns.Add("RealValue");
            PDF2WORDMOde.Rows.Add("文本框模式（文档美观，不易编辑）", Aspose.Pdf.DocSaveOptions.RecognitionMode.Textbox);
            PDF2WORDMOde.Rows.Add("全识别模式（文档不美观，较好编辑", Aspose.Pdf.DocSaveOptions.RecognitionMode.Flow);

            this.PDF2WORD_Mode.DataSource = PDF2WORDMOde;
            this.PDF2WORD_Mode.DisplayMember = "DisplayValue";
            this.PDF2WORD_Mode.ValueMember = "RealValue";


            this.MeregeFilePage.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TwainColorMode.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TwainDpi.DropDownStyle = ComboBoxStyle.DropDownList;
            this.TwainPaperSizeBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.IcoSizeDrop.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PdfWaterMarkHorSetting.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PDFWaterMarkVerSetting.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PdfWaterMarkOpcityNumber.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PDFComplianceBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ExportNotePageCombox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.ConvertPdfComplianceVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Image2PDFCombox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressPDF_ImageCompressVersion.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressPDF_ImageQuailty.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressPDF_ImageResoution.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressJpegMode_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressJpegColorMode_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressJpegQuailty_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressPngLevel_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressPngColorMode_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressBmpMode_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressBmpPixeBits_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.CompressImage_OutPutImageFormat.DropDownStyle = ComboBoxStyle.DropDownList;
            this.PDF2WORD_Mode.DropDownStyle = ComboBoxStyle.DropDownList;



        }

        //注册移动Listview项目的按钮事件
        private void InitMoveItemButtonHandler()
        {
            #region 上下移动项目位置按钮注册点击事件
            WordFileListView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(WordFileListView);
            };
            WordFileListView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(WordFileListView);
            };

            PdfFileListView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(PdfFileListView);
            };
            PdfFileListView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(PdfFileListView);
            };

            ExcelFileListView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(ExcelFileListView);
            };
            ExcelFileListView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(ExcelFileListView);
            };

            PDF2ExcelFileView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(PDF2ExcelFileView);
            };
            PDF2ExcelFileView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(PDF2ExcelFileView);
            };

            PPT2PdfFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(PPT2PdfFileList);
            };
            PPT2PdfFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(PPT2PdfFileList);
            };

            PDF2PPTFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(PDF2PPTFileList);
            };
            PDF2PPTFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(PDF2PPTFileList);
            };


            MergeWordFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(MergeWordFileList);
            };
            MergeWordFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(MergeWordFileList);
            };

            MergePdfFileView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(MergePdfFileView);
            };
            MergePdfFileView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(MergePdfFileView);
            };

            SplitPdfFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(SplitPdfFileList);
            };
            SplitPdfFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(SplitPdfFileList);
            };

            PdfWaterMarkFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(PdfWaterMarkFileList);
            };
            PdfWaterMarkFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(PdfWaterMarkFileList);
            };

            ReplaceWordFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(ReplaceWordFileList);
            };
            ReplaceWordFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(ReplaceWordFileList);
            };

            ReplaceExcelFileView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(ReplaceExcelFileView);
            };
            ReplaceExcelFileView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(ReplaceExcelFileView);
            };

            DecodeMatrixWordListView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(DecodeMatrixWordListView);
            };
            DecodeMatrixWordListView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(DecodeMatrixWordListView);
            };

            IcoFileView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(IcoFileView);
            };
            IcoFileView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(IcoFileView);
            };

            RemovePdfWaterMarkFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(RemovePdfWaterMarkFileList);
            };
            RemovePdfWaterMarkFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(RemovePdfWaterMarkFileList);
            };

            Word2ExcelFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(Word2ExcelFileList);
            };
            Word2ExcelFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(Word2ExcelFileList);
            };

            Excel2WordFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(Excel2WordFileList);
            };
            Excel2WordFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(Excel2WordFileList);
            };

            DocumentsListView_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(DocumentsListView);
            };
            DocumentsListView_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(DocumentsListView);
            };

            ConvertImage2PDFFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(ConvertImage2PDFFileList);
            };
            ConvertImage2PDFFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(ConvertImage2PDFFileList);
            };

            CompressPdfFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(CompressPdfFileList);
            };
            CompressPdfFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(CompressPdfFileList);
            };

            CompressImageFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(CompressImageFileList);
            };
            CompressImageFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(CompressImageFileList);
            };

            ConvertPdfComplianceFileList_MoveUp.Click += (s1, s2) =>
            {
                UI.MoveUpListViewItem(ConvertPdfComplianceFileList);
            };
            ConvertPdfComplianceFileList_MoveDown.Click += (s1, s2) =>
            {
                UI.MoveDownListViewItem(ConvertPdfComplianceFileList);
            };

            #endregion
        }

        //注册删除Listview项目的事件

        private void InitRemoveItemHandler()
        {

            word2pdf_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.WordFileListView);
            };

            pdf2word_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.PdfFileListView);
            };

            excel2pdf_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.ExcelFileListView);
            };

            pdf2excel_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.PdfFileListView);
            };

            ppt2pdf_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.PdfFileListView);
            };

            pdf2ppt_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.PDF2PPTFileList);
            };

            meregeword_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.PdfFileListView);
            };

            meregepdf_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.MergePdfFileView);
            };

            sqplitpdf_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.SplitPdfFileList);
            };

            addpdfwater_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.PdfWaterMarkFileList);
            };

            removepdfwater_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.RemovePdfWaterMarkFileList);
            };

            pdfversion_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.ConvertPdfComplianceFileList);
            };

            word2excel_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.Word2ExcelFileList);
            };

            excel2word_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.Excel2WordFileList);
            };

            document2image_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.DocumentsListView);
            };

            image2pdf_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.ConvertImage2PDFFileList);
            };

            compresspdf_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.CompressPdfFileList);
            };

            compressimage_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.CompressImageFileList);
            };

            wordreplace_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.ReplaceWordFileList);
            };

            excelreplace_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.ReplaceExcelFileView);
            };

            datamatrix_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.DecodeMatrixWordListView);
            };

            image2ico_clear.Click += (s1, s2) => {
                UI.RemoveListViewItem(true, this.IcoFileView);
            };

            word2pdf_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.WordFileListView);
            };

            pdf2word_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.PdfFileListView);
            };

            excel2pdf_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.ExcelFileListView);
            };

            pdf2excel_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.PDF2ExcelFileView);
            };

            ppt2pdf_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.PPT2PdfFileList);
            };

            pdf2ppt_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.PDF2PPTFileList);
            };

            meregeword_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.MergeWordFileList);
            };

            meregepdf_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.MergePdfFileView);
            };

            sqplitpdf_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.SplitPdfFileList);
            };

            addpdfwater_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.PdfWaterMarkFileList);
            };

            removepdfwater_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.RemovePdfWaterMarkFileList);
            };

            pdfversion_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.ConvertPdfComplianceFileList);
            };

            word2excel_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.Word2ExcelFileList);
            };

            excel2word_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.Excel2WordFileList);
            };

            document2image_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.DocumentsListView);
            };

            image2pdf_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.ConvertImage2PDFFileList);
            };

            compresspdf_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.CompressPdfFileList);
            };

            compressimage_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.CompressImageFileList);
            };

            wordreplace_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.ReplaceWordFileList);
            };

            excelreplace_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.ReplaceExcelFileView);
            };

            datamatrix_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.DecodeMatrixWordListView);
            };

            image2ico_remove.Click += (s1, s2) => {
                UI.RemoveListViewItem(false, this.IcoFileView);
            };


        }
        
        //注册TooTip到符号按钮上
        private void InitToolTipHandler()
        {
            MainToolsTip.SetToolTip(this.word2pdf_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.word2pdf_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.WordFileListView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.WordFileListView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.pdf2word_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.pdf2word_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.PdfFileListView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.PdfFileListView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.ExcelFileListView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.ExcelFileListView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.excel2pdf_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.excel2pdf_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.PDF2ExcelFileView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.PDF2ExcelFileView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.pdf2excel_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.pdf2excel_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.PDF2PPTFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.PDF2PPTFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.pdf2ppt_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.pdf2ppt_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.MergePdfFileView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.MergePdfFileView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.meregepdf_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.meregepdf_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.SplitPdfFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.SplitPdfFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.sqplitpdf_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.sqplitpdf_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.PdfWaterMarkFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.PdfWaterMarkFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.addpdfwater_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.addpdfwater_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.RemovePdfWaterMarkFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.RemovePdfWaterMarkFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.removepdfwater_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.removepdfwater_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.ConvertPdfComplianceFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.ConvertPdfComplianceFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.pdfversion_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.pdfversion_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.Word2ExcelFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.Word2ExcelFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.word2excel_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.word2excel_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.Excel2WordFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.Excel2WordFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.excel2word_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.excel2word_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.DocumentsListView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.DocumentsListView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.document2image_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.document2image_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.ConvertImage2PDFFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.ConvertImage2PDFFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.image2pdf_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.image2pdf_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.CompressPdfFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.CompressPdfFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.compresspdf_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.compresspdf_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.CompressImageFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.CompressImageFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.compressimage_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.compressimage_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.ReplaceWordFileList_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.ReplaceWordFileList_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.wordreplace_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.wordreplace_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.ReplaceExcelFileView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.ReplaceExcelFileView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.excelreplace_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.excelreplace_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.DecodeMatrixWordListView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.DecodeMatrixWordListView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.datamatrix_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.datamatrix_remove, "移除选中文件");
            MainToolsTip.SetToolTip(this.IcoFileView_MoveDown, "向下移动选中文件");
            MainToolsTip.SetToolTip(this.IcoFileView_MoveUp, "向上移动选中文件");
            MainToolsTip.SetToolTip(this.image2ico_clear, "清空列表中所有文件");
            MainToolsTip.SetToolTip(this.image2ico_remove, "移除选中文件");


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
                    Debug.WriteLine(ex);
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
            try
            {
                List<string> TempLines = new List<string>();
                TargetTextBox.Lines = Util.AddText2List(SourceTextBox.Text, RemoveNumberCheck.Checked, RemoveSquareCheck.Checked, NameStorkeChcek.Checked,
                                      NameReplaceWord.Checked,NameReplaceWord_Search.Text,NameReplaceWord_Target.Text
                                       );
                //双字对齐功能，放在全部的文本处理完毕以后再进行重新处理，不然只能针对源文本字符长度进行判断，不太合适

                if (this.TargetTextBox.Text != string.Empty)
                {
                    if (DoubleTextAlignCheck.Checked)
                    {
                        string DoubleText;
                        foreach (string TargetTextTemp in this.TargetTextBox.Lines)
                        {
                            DoubleText = TargetTextTemp;
                            if (DoubleText.Length == 2)
                            {
                                DoubleText = DoubleText.Insert(1, "  ");
                            }
                            TempLines.Add(DoubleText);//把不需要对齐的也一起加回来
                        }
                        TargetTextBox.Lines = TempLines.ToArray();
                    }


                    if (NameAutoCopy.Checked)
                    {
                        Clipboard.SetText(TargetTextBox.Text);
                        NameAutoCopyStatusText.Text = "[已复制到剪贴板]";
                        NameAutoCopyStatusText.ForeColor = Color.ForestGreen;
                    }
                    else
                    {
                        NameAutoCopyStatusText.Text = "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                        UI.RemoveListViewItem(false, WordFileListView);
                        break;
                    case "PdfFileListView":
                        UI.RemoveListViewItem(false, PdfFileListView);
                        break;
                    case "ExcelFileListView":
                        UI.RemoveListViewItem(false, ExcelFileListView);
                        break;
                    case "DocumentsListView":
                        UI.RemoveListViewItem(false, DocumentsListView);
                        break;
                    case "PDF2ExcelFileView":
                        UI.RemoveListViewItem(false, PDF2ExcelFileView);
                        break;
                    case "MergePdfFileView":
                        UI.RemoveListViewItem(false, MergePdfFileView);
                        break;
                    case "ReplaceWordFileList":
                        UI.RemoveListViewItem(false, ReplaceWordFileList);
                        break;
                    case "ReplaceExcelFileView":
                        UI.RemoveListViewItem(false, ReplaceExcelFileView);
                        break;
                    case "DecodeMatrixWordListView":
                        UI.RemoveListViewItem(false, DecodeMatrixWordListView);
                        break;
                    case "IcoFileView":
                        UI.RemoveListViewItem(false, IcoFileView);
                        break;
                    case "MergeWordFileList":
                        UI.RemoveListViewItem(false, MergeWordFileList);
                        break;
                    case "Word2ExcelFileList":
                        UI.RemoveListViewItem(false, Word2ExcelFileList);
                        break;
                    case "Excel2WordFileList":
                        UI.RemoveListViewItem(false, Excel2WordFileList);
                        break;
                    case "SplitPdfFileList":
                        UI.RemoveListViewItem(false, SplitPdfFileList);
                        break;
                    case "PdfWaterMarkFileList":
                        UI.RemoveListViewItem(false, PdfWaterMarkFileList);
                        break;
                    case "RemovePdfWaterMarkFileList":
                        UI.RemoveListViewItem(false, RemovePdfWaterMarkFileList);
                        break;
                    case "PPT2PdfFileList":
                        UI.RemoveListViewItem(false, PPT2PdfFileList);
                        break;
                    case "PDF2PPTFileList":
                        UI.RemoveListViewItem(false, PDF2PPTFileList);
                        break;
                    case "ConvertPdfComplianceFileList":
                        UI.RemoveListViewItem(false, ConvertPdfComplianceFileList);
                        break;
                    case "ConvertImage2PDFFileList":
                        UI.RemoveListViewItem(false, ConvertImage2PDFFileList);
                        break;
                    case "CompressPdfFileList":
                        UI.RemoveListViewItem(false, CompressPdfFileList);
                        break;
                    case "CompressImageFileList":
                        UI.RemoveListViewItem(false, CompressImageFileList);
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
                    case "MergeWordFileList":
                        UI.RemoveListViewItem(true, MergeWordFileList);
                        break;
                    case "Word2ExcelFileList":
                        UI.RemoveListViewItem(true, Word2ExcelFileList);
                        break;
                    case "Excel2WordFileList":
                        UI.RemoveListViewItem(true, Excel2WordFileList);
                        break;
                    case "SplitPdfFileList":
                        UI.RemoveListViewItem(true, SplitPdfFileList);
                        break;
                    case "PdfWaterMarkFileList":
                        UI.RemoveListViewItem(true, PdfWaterMarkFileList);
                        break;
                    case "RemovePdfWaterMarkFileList":
                        UI.RemoveListViewItem(true, RemovePdfWaterMarkFileList);
                        break;
                    case "PPT2PdfFileList":
                        UI.RemoveListViewItem(true, PPT2PdfFileList);
                        break;
                    case "PDF2PPTFileList":
                        UI.RemoveListViewItem(true, PDF2PPTFileList);
                        break;
                    case "ConvertPdfComplianceFileList":
                        UI.RemoveListViewItem(true, ConvertPdfComplianceFileList);
                        break;
                    case "ConvertImage2PDFFileList":
                        UI.RemoveListViewItem(true, ConvertImage2PDFFileList);
                        break;
                    case "CompressPdfFileList":
                        UI.RemoveListViewItem(true, CompressPdfFileList);
                        break;
                    case "CompressImageFileList":
                        UI.RemoveListViewItem(true, CompressImageFileList);
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
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.IcoFileView, new string[] { ".jpg", ".png", ".bmp" });
                    break;
                case "MergeWordFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.MergeWordFileList, new string[] { ".doc", ".docx" });
                    break;
                case "Word2ExcelFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.Word2ExcelFileList, new string[] { ".doc", ".docx" });
                    break;
                case "Excel2WordFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.Excel2WordFileList, new string[] { ".xls", ".xlsx" });
                    break;
                case "SplitPdfFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.SplitPdfFileList, new string[] { ".pdf" });
                    break;
                case "PdfWaterMarkFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.PdfWaterMarkFileList, new string[] { ".pdf" });
                    break;
                case "RemovePdfWaterMarkFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.RemovePdfWaterMarkFileList, new string[] { ".pdf" });
                    break;
                case "PPT2PdfFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.PPT2PdfFileList, new string[] { ".ppt", ".pptx" });
                    break;
                case "PDF2PPTFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.PDF2PPTFileList, new string[] { ".pdf" });
                    break;
                case "ConvertPdfComplianceFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.ConvertPdfComplianceFileList, new string[] { ".pdf" });
                    break;
                case "ConvertImage2PDFFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.ConvertImage2PDFFileList, new string[] { ".jpg", ".jpeg", ".png", ".bmp" });
                    break;
                case "CompressPdfFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.CompressPdfFileList, new string[] { ".pdf" });
                    break;
                case "CompressImageFileList":
                    UI.AddFilesToListviewWithNotDialog(DropFiles, this.CompressImageFileList, new string[] { ".jpg", ".jpeg", ".png", ".bmp" });
                    break;
                default: break;
            }
        }

        private void DownloadFrameWorkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net48");
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
            };

            twainSession.TransferReady += (twainSender, twainArgs) =>
            {
                //dont do anything
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
                    if (dataSource.Capabilities.CapSerialNumber.CanGet)
                    {
                        ScanSerialNumber.Text = $"序列号：{dataSource.Capabilities.CapSerialNumber.GetValues().FirstOrDefault()}";
                    }

                    if (this.ExportCaps.Checked)
                    {
                        //输出扫描仪支持的设置参数
                        string DebugName = $"{dataSource.Name}_Support_Capabilities.txt";
                        if (File.Exists(DebugName))
                        {
                            File.Delete(DebugName);
                        }
                        dataSource.Capabilities.CapSupportedCaps.GetValues().ToList().ForEach(x =>
                        {
                            StreamWriter sr = File.AppendText(DebugName);
                            sr.WriteLine(x);
                            sr.Close();
                        }
                        );
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
                //设置本次会话中需要获取的文件数量
                if (TwainScanNumberSet.Text != string.Empty && Int32.TryParse(TwainScanNumberSet.Text, out int TwainScanNumberSetInt))
                {
                    if (dataSource.Capabilities.CapXferCount.CanSet)
                    {
                        if (TwainScanNumberSetInt > 0)
                        {
                            //对于支持双面扫描的扫描仪，先把扫描模式设置为单面扫描
                            if (dataSource.Capabilities.CapDuplex.IsSupported && !(DoubleScanBox.Checked))
                            {
                                dataSource.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                            }
                            dataSource.Capabilities.CapXferCount.SetValue(TwainScanNumberSetInt);
                        }
                        else
                        {
                            dataSource.Capabilities.CapXferCount.SetValue(-1);
                        }
                    }
                }



                if (UseInterfaceUi.Checked)
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

                    this.NowScanNumber.Text = "本次扫描数量：" + TempImageCount.ToString();
                    this.ScannTotalNumber.Text = "扫描总数量：" + TotalTempImageCount.ToString();
                    this.SavedPathFileCount.Text = $"扫描路径内文件数量：{Directory.GetFiles(this.ScanPictureSavePathBox.Text).Length}";
                    sw.Stop();
                    ScanSeconds += sw.Elapsed.TotalSeconds;//一个扫描过程中花费的时间值，值将会在开始扫描按钮中重置为0
                    PictureBoxPreviewImage = new Bitmap(SaveImageName);
                    this.PreviewPictureBox.Image = PictureBoxPreviewImage;
                    TempScannedImageBmpList.Add(PictureBoxPreviewImage);//添加bitmap对象到列表中
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
                    //删除文件之前清除全部的bitmap对象
                    if (TempScannedImageBmpList.Count > 0)
                    {
                        TempScannedImageBmpList.ForEach(ScannBmp =>
                        {
                            ScannBmp.Dispose();
                        });
                        this.PreviewPictureBox.Image.Dispose();
                        this.PreviewPictureBox.Image = null;
                        TempScannedImageBmpList.Clear();
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
            object RegistryString = Util.GetEmbedResourceObj("注册");

            File.WriteAllText("注册_Accusoft控件_右键管理员运行.reg", RegistryString.ToString());

            MessageBox.Show("释放完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void label9_DoubleClick(object sender, EventArgs e)
        {
            if (ReleaseRegistry.Enabled)
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

        private void BodyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void MainTabPage_DoubleClick(object sender, EventArgs e)
        {
            if (this.MainTabPage.Multiline)
            {
                this.Height -= 46;
                this.MainTabPage.Height -= 48;
                this.MainTabPage.Multiline = false;

            }
            else
            {
                this.Height += 46;
                this.MainTabPage.Height += 48;
                this.MainTabPage.Multiline = true;
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.52pojie.cn/thread-1769111-1-1.html");
        }

        private void MergeWordFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.MergeWordFileList, e.Location);
            }
        }

        private void SelectMergeWordFiles_Click(object sender, EventArgs e)
        {
            UI.AddMergeWordFiles();
        }

        private void MergeWordFile_Click(object sender, EventArgs e)
        {
            Task<List<string>> MergeWordFiles = UI.GetListViewFirstItemContent(this.MergeWordFileList);
            if (MergeWordFiles.Result.Count > 0)
            {
                string MergeWordFileName = Util.ShowSaveFileDialog("WORD文档|.docx", "设置文件名称");
                if (MergeWordFileName != string.Empty)
                {
                    Task.Run(() =>
                    {
                        try
                        {
                            string MeregeDirectory = System.IO.Path.GetDirectoryName(MergeWordFileName);
                            this.MeregeWordStatusText.Text = "正在合并中...";
                            Util.MergeWordFile(MergeWordFiles.Result, MergeWordFileName);
                            this.MeregeWordStatusText.Text = "合并完毕，右键名称可移除文件";
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

        private void SelectWord2ExcelFiles_Click(object sender, EventArgs e)
        {
            UI.AddWord2ExcelFiles();
        }

        private void Word2ExcelFileList_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.Word2ExcelFileList, e.Location);
            }
        }

        private void ConvertWord2Excel_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.Word2ExcelFileList, this.ConverWord2ExcelStatusText, ConvertFileType.Word2Excel, ".xlsx");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void SelectExcel2WordFiles_Click(object sender, EventArgs e)
        {
            UI.AddExcel2WordFiles();
        }

        private void Excel2WordFileList_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.Excel2WordFileList, e.Location);
            }
        }

        private void ConvertExcel2Word_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.Excel2WordFileList, this.ConvertExcel2WordStatusText, ConvertFileType.Excel2Word, ".docx");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void SelectSplitPdf_Click(object sender, EventArgs e)
        {
            UI.AddSplitPdfFiles();
        }

        private void SplitPdfFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.SplitPdfFileList, e.Location);
            }
        }

        private void StartSplitPdf_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.SplitPdfFileList, this.SplitPdfStatusText, ConvertFileType.SplitPdf, "");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void SelectWaterMarkPdfFiles_Click(object sender, EventArgs e)
        {
            UI.AddWaterMarkPdfFiles();
        }

        private void PdfWaterMarkFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.PdfWaterMarkFileList, e.Location);
            }
        }

        private void PDFWaterMarkTextFontSetting_Click(object sender, EventArgs e)
        {
            DialogResult FontSelectDialogResult = FontSelectDialog.ShowDialog();

            if (FontSelectDialogResult == DialogResult.OK)
            {
                PrivateFontSetting.FontName = FontSelectDialog.Font.FontFamily.GetName(1033);
                PrivateFontSetting.FontSize = FontSelectDialog.Font.Size;
            }
        }

        private void PdfWaterMarkFontColorSettings_Click(object sender, EventArgs e)
        {
            DialogResult ColorSelectDialogResult = ColorSelectDialog.ShowDialog();

            if (ColorSelectDialogResult == DialogResult.OK)
            {
                PrivateFontSetting.FontColor = ColorSelectDialog.Color;
            }
        }

        private void AddWaterMark2Pdf_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.PdfWaterMarkFileList, this.AddWaterMark2PdfStatusText, ConvertFileType.AddWaterMarkPdf, ".pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });

        }

        private void SelectRemovePdfWaterMarkFiles_Click(object sender, EventArgs e)
        {
            UI.AddRemoveWaterFiles();
        }

        private void RemovePdfWaterMarkFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.RemovePdfWaterMarkFileList, e.Location);
            }
        }

        private void RemovePdfWaterMark_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.RemovePdfWaterMarkFileList, this.RemovePdfWaterMarkStatusText, ConvertFileType.RemovePdfWaterMark, ".pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void BodyForm_Resize(object sender, EventArgs e)
        {
            ReWinformLayout();
        }

        private void AddBackGroudImage_Click(object sender, EventArgs e)
        {
            IEnumerable<string> BackGroundImageFileNames = Util.ShwoSelectDialog(false, false, "图片文件", "*.jpg;*png;*.jpeg;*.bmp");

            if (BackGroundImageFileNames.FirstOrDefault() != default)
            {
                AddWaterMarkBackGroudImageFileName = BackGroundImageFileNames.FirstOrDefault();

                Image TempLoadImage = Image.FromFile(AddWaterMarkBackGroudImageFileName);

                WaterMarkImage.Image = TempLoadImage;

                this.WaterMarkImageHeight.Text = TempLoadImage.Height.ToString();

                this.WaterMarkImageWidth.Text = TempLoadImage.Width.ToString();
            }
        }

        private void ValidTextBoxContent(object sender, CancelEventArgs e)
        {
            TextBox TempTextBox = (TextBox)sender;

            string TempText = TempTextBox.Text;

            if (!IsNumber(TempText))
            {
                e.Cancel = true;
                MainFormError.SetError(TempTextBox, "请输入正确的数字");
            }

        }

        private void ValidedTextBoxContent(object sender, EventArgs e)
        {
            MainFormError.Clear();
        }

        private bool IsNumber(string InputString)
        {

            Regex TempRegex = new Regex("^[0-9]*$");

            bool NumberStatus = TempRegex.IsMatch(InputString);

            return NumberStatus;

        }


        #region 重绘tabpage的标签颜色
        private void MainTabPage_DrawItem(object sender, DrawItemEventArgs e)
        {
            Font PageFont;
            Brush BackBrush, ForeBrush;

            Rectangle TabRec = e.Bounds;

            TabRec = new Rectangle(TabRec.X, TabRec.Y + 4, TabRec.Width, TabRec.Height - 4);


            if (e.Index == this.MainTabPage.SelectedIndex)
            {

                PageFont = new Font(e.Font, FontStyle.Regular);
                BackBrush = new System.Drawing.Drawing2D.LinearGradientBrush(e.Bounds, SystemColors.Control, SystemColors.Control, System.Drawing.Drawing2D.LinearGradientMode.BackwardDiagonal);

                ForeBrush = Brushes.Red;
            }
            else
            {
                PageFont = e.Font;
                BackBrush = new SolidBrush(SystemColors.Control);
                ForeBrush = new SolidBrush(Color.Black);
            }

            string TabName = this.MainTabPage.TabPages[e.Index].Text;

            StringFormat FormatTab = new StringFormat();

            e.Graphics.FillRectangle(BackBrush, e.Bounds);

            e.Graphics.DrawString(TabName, PageFont, ForeBrush, TabRec, FormatTab);
        }
        #endregion

        private async void ChceckVersion_Click(object sender, EventArgs e)
        {
            try
            {
                Uri CheckVersionUrl = new Uri("http://jdsnmsl.tpddns.cn:3009/pwn/Version/raw/branch/main/DocumentTools.txt");

                using (HttpClient client = new HttpClient())
                {
                    string UserAgentName = $"{Application.ProductName}_{RuntimeInformation.ProcessArchitecture}";
                    client.DefaultRequestHeaders.Add("User-Agent", UserAgentName);
                    string RemoteVersion = await client.GetStringAsync(CheckVersionUrl);

                    if (int.Parse(NowVersion.Replace(".", "")) < int.Parse(RemoteVersion.Replace(".", "")))
                    {
                        MessageBox.Show($"当前版本为：{NowVersion}，最新版本为：{RemoteVersion}，建议及时更新", "检查更新", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else if (int.Parse(NowVersion.Replace(".", "")) == int.Parse(RemoteVersion.Replace(".", "")))
                    {
                        MessageBox.Show($"当前版本为最新版本：{NowVersion}", "检查更新", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                    }
                    else
                    {
                        MessageBox.Show($"当前版本和服务器版本不一致，请及时前往更新地址查看最新更新日志", "检查更新", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        private void SelectPPT2PdfFile_Click(object sender, EventArgs e)
        {
            UI.AddPPT2PdfFiles();
        }

        private void ConvertPPT2PDF_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.PPT2PdfFileList, this.ConvertPPT2PDFStatusText, ConvertFileType.PPT2PDF, ".pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void PPT2PdfFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.PPT2PdfFileList, e.Location);
            }
        }

        private void ListviewSortHandler(object sender, ColumnClickEventArgs e)
        {
            ListView TempListview = sender as ListView;

            if (e.Column == 0)
            {
                if (TempListview.Sorting == SortOrder.Descending)
                {
                    TempListview.Sorting = SortOrder.Ascending;
                }
                else if (TempListview.Sorting == SortOrder.Ascending)
                {
                    TempListview.Sorting = SortOrder.Descending;
                }

                TempListview.Sort();
            }


        }

        private void ConvertPDF2PPT_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.PDF2PPTFileList, this.Pdf2PPTStatusText, ConvertFileType.PDF2PPT, ".pptx");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void SelectConvertPDF2PPT_Click(object sender, EventArgs e)
        {
            UI.AddPdf2PPTFiles();
        }

        private void PDF2PPTFileList_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.WordFileListView, e.Location);
            }
        }

        private void ConvertPdfComplianceFileList_MouseClick(object sender, MouseEventArgs e)
        {
            //右键项目才显示菜单
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.ConvertPdfComplianceFileList, e.Location);
            }
        }

        private void SelectConvertComplaincePdf_Click(object sender, EventArgs e)
        {
            UI.AddConvertComplanicePdfFiles();
        }

        private void ConvertPdfCompliance_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.ConvertPdfComplianceFileList, this.ConvertPdfComplianceStatusText, ConvertFileType.ConvertPdfCompliance, ".pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void ConvertImage2PDFFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.ConvertImage2PDFFileList, e.Location);
            }
        }

        private void SelectConvertImage2PDF_Click(object sender, EventArgs e)
        {
            UI.AddConvertImage2PDFFiles();
        }

        private void ConvertImage2PDF_Click(object sender, EventArgs e)
        {
            Task<List<string>> ImageFiles = UI.GetListViewFirstItemContent(this.ConvertImage2PDFFileList);
            if (ImageFiles.Result.Count > 0)
            {
                try
                {
                    if (ExportSignlePDF.Checked)
                    {
                        IEnumerable<string> SavePaths = Util.ShwoSelectDialog(true, true, "", "");
                        Task.Run(() =>
                        {
                            string SavePath = SavePaths.FirstOrDefault();
                            if (SavePath != string.Empty || SavePath != default)
                            {
                                if (SavePath != null)
                                {
                                    int ImageCount = 0;
                                    ImageFiles.Result.ForEach(TempFile =>
                                    {
                                        ImageCount++;
                                        this.COnvertImage2PDFStatusText.Text = $"正在转换第{ImageCount}个文件";
                                        string SavePdfName = $"{Path.Combine(SavePath, Path.GetFileNameWithoutExtension(TempFile) + ".pdf")}";
                                        Util.ConvertImage2PDF(TempFile, SavePdfName, false, ImageFiles.Result, bool.Parse(this.Image2PDFCombox.SelectedValue.ToString()), this.Image2PDFOptiomize.Checked, this.Image2PDFCOmpressImage.Checked, double.Parse(this.ImagePdfPageSIze_UP.Text), double.Parse(this.ImagePdfPageSIze_LEFT.Text), double.Parse(this.ImagePdfPageSIze_BOTTOM.Text), double.Parse(this.ImagePdfPageSIze_RIGHT.Text));

                                        this.COnvertImage2PDFStatusText.Text = $"转换完毕";
                                    });
                                    this.BeginInvoke(new Action(() =>
                                    {
                                        DialogResult OpenResult = MessageBox.Show("合并完毕，是否需要打开合并文件所在目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                                        if (OpenResult == DialogResult.Yes)
                                        {
                                            Process.Start(SavePath);
                                        }
                                    }));
                                }
                            }
                        });
                    }
                    else
                    {
                        string MeregePdfFileName = Util.ShowSaveFileDialog("PDF文档|.pdf", "设置文件名称");
                        Task.Run(() =>
                        {
                            if (MeregePdfFileName != string.Empty)
                            {
                                this.COnvertImage2PDFStatusText.Text = "正在合并中...";
                                Util.ConvertImage2PDF("", MeregePdfFileName, true, ImageFiles.Result, bool.Parse(this.Image2PDFCombox.SelectedValue.ToString()), this.Image2PDFOptiomize.Checked, this.Image2PDFCOmpressImage.Checked, double.Parse(this.ImagePdfPageSIze_UP.Text), double.Parse(this.ImagePdfPageSIze_LEFT.Text), double.Parse(this.ImagePdfPageSIze_BOTTOM.Text), double.Parse(this.ImagePdfPageSIze_RIGHT.Text));
                                this.COnvertImage2PDFStatusText.Text = "合并完毕";
                                this.BeginInvoke(new Action(() =>
                                {
                                    DialogResult OpenResult = MessageBox.Show("合并完毕，是否需要打开合并文件所在目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                                    if (OpenResult == DialogResult.Yes)
                                    {
                                        Process.Start(Path.GetDirectoryName(MeregePdfFileName));
                                    }
                                }));
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CompressPdfFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.CompressPdfFileList, e.Location);
            }
        }

        private void SelectCompressPDFFiles_Click(object sender, EventArgs e)
        {
            UI.AddCompressPDFFiles();
        }

        private void CompressPDF_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.CompressPdfFileList, this.CompressPDFStatusText, ConvertFileType.CompressPDF, ".pdf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void CompressImageFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.ListViewRightMenu.Show(this.CompressImageFileList, e.Location);
            }
        }

        private void SelectCompressImageFile_Click(object sender, EventArgs e)
        {
            UI.AddCompressImageFiles();
        }

        private void CompressImage_Click(object sender, EventArgs e)
        {


            Task.Run(() =>
            {
                try
                {
                    UI.ConvertStart(this.CompressImageFileList, this.CompressImageStatusText, ConvertFileType.CompressImage, this.CompressImage_OutPutImageFormat.SelectedValue.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            });
        }

        private void CompressImage_NotUseSelfExt_CheckedChanged(object sender, EventArgs e)
        {
            if (this.CompressImage_NotUseSelfExt.Checked)
            {
                this.CompressImage_OutPutImageFormat.Enabled = true;
            }
            else
            {
                this.CompressImage_OutPutImageFormat.Enabled = false;
            }
        }

        private void PlayCXKMusic_Click(object sender, EventArgs e)
        {
            Util.PlayMp3WithStream();
        }

        private void NameReplaceRegexMode_Click(object sender, EventArgs e)
        {
            if (NameReplaceMode)
            {
                NameReplaceMode = false;
                NameReplaceRegexMode.Text = "[正则模式：关]";
                NameReplaceRegexMode.ForeColor = Color.Red;
            }
            else
            {
                NameReplaceMode = true;
                NameReplaceRegexMode.Text = "[正则模式：开]";
                NameReplaceRegexMode.ForeColor = Color.ForestGreen;
            }
        }

        #region 锁定解锁键盘操作
        private void LockKeyBoard_Click(object sender, EventArgs e)
        {

            GlobalHook.KeyDown += HookKeyboard_KeyDown;
            GlobalHook.KeyUp   += HookKeyboard_KeyUp;
            KeyBoardLockStatus.Text = $"已锁定";
            KeyBoardLockStatus.ForeColor = Color.Red;
        }

        private void UnlockKeyBoard_Click(object sender, EventArgs e)
        {
            GlobalHook.KeyDown -= HookKeyboard_KeyDown;
            GlobalHook.KeyUp   -= HookKeyboard_KeyUp;

            KeyBoardLockStatus.Text = "已解锁";
            KeyBoardLockStatus.ForeColor = Color.Green;
        }

        private void HookKeyboard_KeyDown(object sender,KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void HookKeyboard_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
        }
        #endregion

        private void RemoveShort_Click(object sender, EventArgs e)
        {
            Util.RemoveShort();
            ShortStatus.Text = "已移除";
            ShortStatus.ForeColor = Color.Red;
            if (AutoRestartExplorer.Checked)
            {
                Process.GetProcesses().ToList().ForEach(p=>
                {
                    if(p.ProcessName=="explorer")
                    {
                        p.Kill();
                    }
                });
                Process.Start("explorer.exe");
            }
        }

        private void RestoreShort_Click(object sender, EventArgs e)
        {
            Util.RestoreShort();
            ShortStatus.Text = "已恢复";
            ShortStatus.ForeColor = Color.Green;
            if (AutoRestartExplorer.Checked)
            {
                Process.GetProcesses().ToList().ForEach(p =>
                {
                    if (p.ProcessName == "explorer")
                    {
                        p.Kill();
                    }
                });
                Process.Start("explorer.exe");
            }
        }
    }
}

internal class PrivateFontSetting
{
    //public fontfam

    public static string FontName { get; set; } = "SimSun";

    public static float FontSize { get; set; } = 12;

    public static Color FontColor { get; set; } = Color.Red;
}
