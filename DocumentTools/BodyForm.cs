using AutoUpdaterDotNET;
using Gma.System.MouseKeyHook;
using Newtonsoft.Json.Linq;
using NTwain;
using NTwain.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace DocumentTools
{
    public partial class BodyForm : Form
    {
        public static BodyForm MainForm;
        private TwainSession twainSession;
        private DataSource dataSource;
        private int ImageIndex;
        private int TempImageCount;
        private int TotalTempImageCount;
        private double ScanSeconds = 0;
        private Stopwatch ScanStopWatch;
        //private Bitmap PictureBoxPreviewImage;
        private readonly List<Bitmap> TempScannedImageBmpList = new List<Bitmap>();
        private readonly IKeyboardEvents GlobalHook = Hook.GlobalEvents();//全局钩子事件
        private readonly ImageList TempImageList = new ImageList() { ImageSize = new Size(1, 25) };

        public FileDropHandler FileDropper; //全局的

        private string ListviewShowGridLines = "on";
        private string ShowTabPageIcon = "on";
        private string ChromeDriverPath = string.Empty;
        private string Vfsoft_ExamDataPath = string.Empty;
        private string Vfsoft_MssDataPath = string.Empty;

        public bool NameReplaceMode;
        public string ShowOpenTargetDialog = "on";//是否启用完成后显示打开转换目录弹窗
        public string AddWaterMarkBackGroundImageFileName = string.Empty;
        public string WidgetAnchorMode = "off";

        public int FormWidth = 906;
        public int FormHeight = 834;

        public JArray City_Json;//城市的json字符串数据

        bool TwainInitState;

        private string ChangeReportItemTempHelpPdfFile = string.Empty;


        private void OutPutListViewIndex()
        {
            foreach (TabPage TempPage in this.MainTabPage.TabPages)
            {
                Debug.WriteLine($"{TempPage.Text}：{MainTabPage.TabPages.IndexOf(TempPage)}");
            }
        }

        private void GetAdministratorBehaviorStatus()
        {
            string BehaviorStatus = Util.GetAdministratorBehavior();
            if (string.Compare(BehaviorStatus, "1", StringComparison.Ordinal) == 0)
            {
                DisableAdminBehavior.Text = "禁用管理员批准模式";
            }
            else if (string.Compare(BehaviorStatus, "0", StringComparison.Ordinal) == 0)
            {
                DisableAdminBehavior.Text = "启用管理员批准模式";
            }
        }

        private void SetInitTabPage()
        {
            if (int.TryParse(ConfigurationManager.AppSettings["StartPageIndex"], out int StartIndex))
            {
                MainTabPage.SelectedIndex = StartIndex;
                menu_start_page_index_combox.SelectedIndex = StartIndex;
            }
        }

        private void SetTabCardHeighLight()
        {
            string OpenTabPageHeight = ConfigurationManager.AppSettings["TabPageHeighLight"];

            SetTabPageShowIco();//先对tabpage进行设置

            if (string.Compare(OpenTabPageHeight, "on", StringComparison.Ordinal) == 0)
            {
                this.MainTabPage.DrawMode = TabDrawMode.OwnerDrawFixed;
                menu_tabpage_heigh_light_combox.SelectedIndex = 0;
            }
            else
            {
                this.MainTabPage.DrawMode = TabDrawMode.Normal;
                menu_tabpage_heigh_light_combox.SelectedIndex = 1;

                if (string.Compare(ShowTabPageIcon, "on", StringComparison.Ordinal) == 0)
                {
                    MainTabPage.ImageList = TabPageIcoList;
                }
            }
        }

        private void SetConfirmDialogShow()
        {
            ShowOpenTargetDialog = ConfigurationManager.AppSettings["ShowOpenTargetDialog"];

            menu_finshined_messbox_combox.SelectedIndex = string.Compare(
                        ShowOpenTargetDialog,
                        "on",
                        StringComparison.Ordinal) ==
                    0
                ? 0
                : 1;
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
                        if (PageIndex >= 0 & PageIndex <= MainTabPage.TabCount - 1)
                        {
                            MainTabPage.TabPages[PageIndex].Parent = null;
                        }
                    }
                }
            }
        }

        private void SetListViewLineHeight()
        {
            string ListViewLineHeight = ConfigurationManager.AppSettings["ListViewLineHeight"];

            if (int.TryParse(ListViewLineHeight, out int LineHeight) && LineHeight > 0 && LineHeight <= 256)
            {
                TempImageList.ImageSize = new Size(1, LineHeight);
                menu_listview_line_heigh_combox.SelectedIndex = LineHeight - 1;
            }
        }

        private void SetListViewShowGridLines()
        {
            ListviewShowGridLines = ConfigurationManager.AppSettings["ListViewShowGridLines"];
            menu_listview_gridline_combox.SelectedIndex = string.Compare(
                        ListviewShowGridLines,
                        "on",
                        StringComparison.Ordinal) ==
                    0
                ? 0
                : 1;
        }

        private void SetTabPageShowIco()
        {
            ShowTabPageIcon = ConfigurationManager.AppSettings["ShowTabPageIco"];
            menu_tabpage_icon_combox.SelectedIndex = string.Compare(ShowTabPageIcon, "on", StringComparison.Ordinal) ==
                    0
                ? 0
                : 1;
        }

        private void SetChromeDriverPath()
        {
            ChromeDriverPath = ConfigurationManager.AppSettings["ChromeDriverPath"];

            menu_chrome_path.Text = ChromeDriverPath;
            html2pdf_textbox_chromepath.Text = ChromeDriverPath;
        }

        private void SetVfsoftDataPath()
        {
            Vfsoft_ExamDataPath = ConfigurationManager.AppSettings["Vfsoft_ExamDataPath"];
            Vfsoft_MssDataPath = ConfigurationManager.AppSettings["Vfsoft_MssDataPath"];

            vfsoft_examdata_path_textbox.Text = Vfsoft_ExamDataPath;
            vfsoft_mssdata_path_textbox.Text = Vfsoft_MssDataPath;
        }

        private void SetFormSize()
        {
            if (int.TryParse(ConfigurationManager.AppSettings["FormWidth"], out int width) &&
                int.TryParse(ConfigurationManager.AppSettings["FormHeight"], out int height))
            {
                FormWidth = width;
                FormHeight = height;
            }

            this.Width = FormWidth;
            this.Height = FormHeight;

            bodyform_width.Text = this.Width.ToString();
            bodyform_height.Text = this.Height.ToString();
        }

        private void SetWidgetAnchorMode()
        {
            WidgetAnchorMode = ConfigurationManager.AppSettings["WidgetAnchorMode"];

            menu_widget_anchor_combox.SelectedIndex = string.Compare(WidgetAnchorMode, "on", StringComparison.Ordinal) ==
                    0
                ? 0
                : 1;
        }

        /// <summary>
        /// 设置控件的Anchor 锚点
        /// </summary>
        private void SetWidgetAnchor()
        {
            MainTabPage.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            votedataview.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            SignDocumentPicSourceGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            SingleDocumentConfigGroupBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            foreach (TabPage TempPage in MainTabPage.TabPages)
            {
                foreach (System.Windows.Forms.Control TempControl in TempPage.Controls)
                {
                    if (TempControl is System.Windows.Forms.GroupBox)
                    {
                        foreach (System.Windows.Forms.Control control in TempControl.Controls)
                        {
                            switch (control.Tag)
                            {
                                case "anchor_full_fill"://全方向跟随
                                    control.Anchor = AnchorStyles.Top |
                                        AnchorStyles.Bottom |
                                        AnchorStyles.Left |
                                        AnchorStyles.Right;
                                    break;
                                case "anchor_left_bottom"://左下跟随
                                    control.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                                    break;
                                case "anchor_left_top"://左上跟随
                                    control.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                                    break;
                                case "anchor_right_bottom"://右下跟随
                                    control.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                                    break;
                                case "anchor_right_top"://右上跟随
                                    control.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                                    break;
                                case "anchor_top"://固定上方
                                    control.Anchor = AnchorStyles.Top;
                                    break;
                                case "anchor_bottom"://固定下方
                                    control.Anchor = AnchorStyles.Bottom;
                                    break;
                                case "anchor_left"://固定左侧
                                    control.Anchor = AnchorStyles.Left;
                                    break;
                                case "anchor_right"://固定右侧
                                    control.Anchor = AnchorStyles.Right;
                                    break;
                                case "anchor_top_bottom"://上下跟随
                                    control.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
                                    break;
                                case "anchor_top_left_right"://上左右跟随
                                    control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                                    break;
                                case "anchor_left_right"://左右跟随
                                    control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                                    break;
                                case "anchor_top_left_bottom"://上左下跟随
                                    control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
                                    break;
                                case "anchor_top_right_bottom"://上右下跟随
                                    control.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                                    break;
                                case "anchor_bottom_left_right"://下左右跟随
                                    control.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    switch (TempControl.Tag)
                    {
                        case "anchor_full_fill"://全方向跟随
                            TempControl.Anchor = AnchorStyles.Top |
                                AnchorStyles.Bottom |
                                AnchorStyles.Left |
                                AnchorStyles.Right;
                            break;
                        case "anchor_left_bottom"://左下跟随
                            TempControl.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
                            break;
                        case "anchor_left_top"://左上跟随
                            TempControl.Anchor = AnchorStyles.Left | AnchorStyles.Top;
                            break;
                        case "anchor_right_bottom"://右下跟随
                            TempControl.Anchor = AnchorStyles.Right | AnchorStyles.Bottom;
                            break;
                        case "anchor_right_top"://右上跟随
                            TempControl.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                            break;
                        case "anchor_top"://固定上方
                            TempControl.Anchor = AnchorStyles.Top;
                            break;
                        case "anchor_bottom"://固定下方
                            TempControl.Anchor = AnchorStyles.Bottom;
                            break;
                        case "anchor_left"://固定左侧
                            TempControl.Anchor = AnchorStyles.Left;
                            break;
                        case "anchor_right"://固定右侧
                            TempControl.Anchor = AnchorStyles.Right;
                            break;
                        case "anchor_top_bottom"://上下跟随
                            TempControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
                            break;
                        case "anchor_top_left_right"://上左右跟随
                            TempControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                            break;
                        case "anchor_left_right"://左右跟随
                            TempControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                            break;
                        case "anchor_top_left_bottom"://上左下跟随
                            TempControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;
                            break;
                        case "anchor_top_right_bottom"://上右下跟随
                            TempControl.Anchor = AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
                            break;
                        case "anchor_bottom_left_right"://下左右跟随
                            TempControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                            break;
                        default:
                            break;
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
                SetConfirmDialogShow();//设置是否显示转换后弹窗
                SetTabPageVisable();//设置标签卡隐藏
                SetListViewLineHeight();//设置文件列表行距
                SetListViewShowGridLines();//设置是否开启网格线
                SetTabCardHeighLight();//设置标签卡高亮
                SetChromeDriverPath();//读取chrome的驱动地址
                SetVfsoftDataPath();//读取vfsoft的数据包路径
                SetFormSize();//设置窗体大小
                SetWidgetAnchorMode();//获取控件锚点配置
            }
        }

        private void InitDataGridView()
        {
            barcodeMutilList.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            barcodeMutilList.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            barcodeMutilList.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            barcodeMutilList.AllowUserToDeleteRows = true;
            barcodeMutilList.Font = new System.Drawing.Font("宋体", 12);
            barcodeMutilList.BackgroundColor = Color.AliceBlue;


            votedataview.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            votedataview.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            votedataview.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            votedataview.AllowUserToDeleteRows = false;
            votedataview.AllowUserToAddRows = false;
            votedataview.AllowUserToOrderColumns = false;
            votedataview.RowHeadersVisible = true;
            votedataview.ReadOnly = true;
            votedataview.Font = new System.Drawing.Font("宋体", 10);
            votedataview.BackgroundColor = Color.AliceBlue;
        }


        public BodyForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            MainForm = this;


            //this.AutoScaleMode = AutoScaleMode.Font;
        }

        private void UpdateCheckEvent(UpdateInfoEventArgs updateArgs)
        {
            if (updateArgs.Error == null && updateArgs.IsUpdateAvailable)
            {
                VersionInfo.VersionUpdateLogUrl = updateArgs.ChangelogURL;
                this.UpdateCheckText.IsLink = true;
                this.UpdateCheckText.Text = $"当前版本为：{VersionInfo.VersionNumber}，最新版本为：{updateArgs.CurrentVersion}，点击更新";
                this.UpdateCheckText.Click -= (s1, s2) =>
                {
                    Process.Start(updateArgs.DownloadURL);
                };
                this.UpdateCheckText.Click += (s1, s2) =>
                {
                    Process.Start(updateArgs.DownloadURL);
                };
            }
        }

        private void BodyForm_Load(object sender, EventArgs e)
        {
            //判断内部版本

            AutoUpdater.InstalledVersion = new Version(VersionInfo.VersionNumber);
            AutoUpdater.Start(VersionInfo.VersionUpdateCheckUrl);
            AutoUpdater.CheckForUpdateEvent += UpdateCheckEvent;


            //初始化
            this.Text = VersionInfo.VersionTitle;
            this.UpdateCheckText.Text = $"当前版本为：{VersionInfo.VersionNumber}，没有检测到新版本";
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoSize = false;
            City_Json = JArray.Parse((string)Util.GetEmbedResourceObj("china_level"));
            InitDataGridView();
            InitComboxValue();
            InitConfigSettings();
            InitListView();
            InitMoveItemButtonHandler();
            InitRemoveItemHandler();
            InitToolTipHandler();
            //InitConfigSettings();
            InitTabPageIcon();
            OutPutListViewIndex();
            if (string.Compare(WidgetAnchorMode, "on", StringComparison.Ordinal) == 0)
            {
                SetWidgetAnchor();//设置控件锚点
            }
            GetAdministratorBehaviorStatus();
            //SetAutoScrollTabPages();
            this.AllowDrop = true;
            this.PageExportCheck.Checked = true;
            this.PageExportCheck.Enabled = true;

            this.PreviewPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            this.ScanPictureSavePathBox.Enabled = false;
            this.DecodeDataMatrixView.ShowLines = true;
            this.DecodeDataMatrixView.ShowRootLines = true;
            this.DecodeDataMatrixView.FullRowSelect = true;
            this.CompressImage_OutPutImageFormat.Enabled = false;
            this.IcoSizeDrop.Enabled = true;
            //ReleaseRegistry.Enabled = false;
            this.Image2PDFCOmpressImage.Enabled = false;

            this.gen_card_id_group.Enabled = false;
            this.gen_card_result_group.Enabled = false;

            //Util.PlayMp3WithStream();

            ShortStatus.Text = Util.GetShortStatus();
            ShortStatus.ForeColor = string.Compare(ShortStatus.Text, "已移除", StringComparison.Ordinal) == 0
                ? Color.Red
                : Color.Green;


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

            this.CoverImageDpi.Validating += ValidTextBoxContent;
            this.CoverImageDpi.Validated += ValidedTextBoxContent;

            this.html2pdf_textbox_margin_top.Validating += ValidTextBoxContent;
            this.html2pdf_textbox_margin_top.Validated += ValidedTextBoxContent;
            this.html2pdf_textbox_margin_left.Validating += ValidTextBoxContent;
            this.html2pdf_textbox_margin_left.Validated += ValidedTextBoxContent;
            this.html2pdf_textbox_margin_bottom.Validating += ValidTextBoxContent;
            this.html2pdf_textbox_margin_bottom.Validated += ValidedTextBoxContent;
            this.html2pdf_textbox_margin_right.Validating += ValidTextBoxContent;
            this.html2pdf_textbox_margin_right.Validated += ValidedTextBoxContent;

            this.voteid.Validating += ValidTextBoxContent;
            this.voteid.Validated += ValidedTextBoxContent;

            this.voteindex.Validating += ValidTextBoxContent;
            this.voteindex.Validated += ValidedTextBoxContent;

            this.barcodeDmiBox.Validating += ValidTextBoxContent;
            this.barcodeDmiBox.Validated += ValidedTextBoxContent;

            this.barcodeImageHeight.Validating += ValidTextBoxContent;
            this.barcodeImageHeight.Validated += ValidedTextBoxContent;

            this.barcodePaddingBox.Validating += ValidTextBoxContent;
            this.barcodePaddingBox.Validated += ValidedTextBoxContent;

            this.barcodeRotateBox.Validating += ValidTextBoxContent;
            this.barcodeRotateBox.Validated += ValidedTextBoxContent;

            this.barcodeTextFontSizeBox.Validating += ValidTextBoxContent;
            this.barcodeTextFontSizeBox.Validated += ValidedTextBoxContent;

            this.barcodeTextContentSpaceCombox.Validating += ValidTextBoxContent;
            this.barcodeTextContentSpaceCombox.Validated += ValidedTextBoxContent;

            this.barcodeCaptionFontSizeBox.Validating += ValidTextBoxContent;
            this.barcodeCaptionFontSizeBox.Validated += ValidedTextBoxContent;


            this.barcodeImageHeight.Validating += ValidTextBoxContent;
            this.barcodeImageHeight.Validated += ValidedTextBoxContent;


            this.barcodeImageWidth.Validating += ValidTextBoxContent;
            this.barcodeImageWidth.Validated += ValidedTextBoxContent;

            this.copied_number_prefix_count.Validating += ValidTextBoxContent;
            this.copied_number_prefix_count.Validated += ValidedTextBoxContent;

            this.SplitPdfRange_Start.Validating += ValidTextBoxContent;
            this.SplitPdfRange_Start.Validated += ValidedTextBoxContent;
            this.SplitPdfRange_End.Validating += ValidTextBoxContent;
            this.SplitPdfRange_End.Validated += ValidedTextBoxContent;

            this.SplitWord_Range_Start.Validating += ValidTextBoxContent;
            this.SplitWord_Range_Start.Validated += ValidedTextBoxContent;
            this.SplitWord_Range_End.Validating += ValidTextBoxContent;
            this.SplitWord_Range_End.Validated += ValidedTextBoxContent;

            this.SplitPPT_RangeExport_Start.Validating += ValidTextBoxContent;
            this.SplitPPT_RangeExport_Start.Validated += ValidedTextBoxContent;
            this.SplitPPT_RangeExport_End.Validating += ValidTextBoxContent;
            this.SplitPPT_RangeExport_End.Validated += ValidedTextBoxContent;


            this.DecodeDataMatrixView.MouseClick += (s1, s2) =>
            {
                if (s2.Button == MouseButtons.Right)
                {
                    if (s1 is System.Windows.Forms.TreeView TempTreeView)
                    {
                        TreeViewContentMenu.Show(TempTreeView, s2.Location);
                    }
                }
            };

            //AutoSize = new AutoAdaptWindowsSize(this);
        }


        //初始化Listview
        private void InitListView()
        {
            //初始化所有listview

            GetFormMembers(typeof(System.Windows.Forms.ListView))
                .ForEach(
                    x =>
                    {
                        System.Windows.Forms.ListView TempListView = x as System.Windows.Forms.ListView;
                        TempListView.View = System.Windows.Forms.View.Details;
                        TempListView.FullRowSelect = true;
                        TempListView.AllowDrop = true;
                        TempListView.ShowItemToolTips = true;
                        TempListView.MultiSelect = false;
                        TempListView.GridLines = string.Compare(ListviewShowGridLines, "on", StringComparison.Ordinal) ==
                            0;
                        TempListView.SmallImageList = TempImageList;
                        TempListView.Name = TempListView.Name.ToString();

                        if (string.Compare(TempListView.Name, "VFSOFT_Report_ListView", StringComparison.Ordinal) == 0)
                        {
                            TempListView.Columns.Add("报表ID", 330);
                            TempListView.Columns.Add("报表名称", 330);
                            TempListView.Columns.Add("所属考评表", 330);
                            TempListView.Columns.Add("考评表ID", 330);
                        }
                        else
                        {
                            TempListView.Columns.Add("文件名称", 300, HorizontalAlignment.Left);
                            TempListView.Columns.Add("添加时间", 300, HorizontalAlignment.Left);
                            TempListView.Columns.Add("转换后文件名称", 300, HorizontalAlignment.Left);
                            TempListView.Columns.Add("转换后文件大小", 300, HorizontalAlignment.Left);

                            TempListView.DragEnter += ListViewDragEnter;
                            TempListView.DragDrop += ListviewDragDrop;


                            TempListView.MouseClick += (MouseSender, MouseEvent) =>
                            {
                                if (MouseEvent.Button == MouseButtons.Right)
                                {
                                    ListViewRightMenu.Show(TempListView, MouseEvent.Location);
                                }
                            };
                        }

                        TempListView.ColumnClick += ListviewSortHandler;
                    });
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

            this.PdfWaterMarkOpcityNumber.DataSource = new float[]
            {
                0.0f,
                0.1f,
                0.2f,
                0.3f,
                0.4f,
                0.5f,
                0.6f,
                0.7f,
                0.8f,
                0.9f,
                1.0f
            };
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
            Image2PDFPageStyle.Rows.Add("自动", false);
            Image2PDFPageStyle.Rows.Add("横排", true);
            Image2PDFPageStyle.Rows.Add("竖排", false);

            this.Image2PDFCombox.DataSource = Image2PDFPageStyle;
            this.Image2PDFCombox.DisplayMember = "DisplayValue";
            this.Image2PDFCombox.ValueMember = "RealValue";

            this.CompressPDF_ImageQuailty.DataSource = Enumerable.Range(0, 101).ToList();
            this.CompressPDF_ImageQuailty.SelectedIndex = 80;

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

            this.CompressPDF_ImageResoution.SelectedIndex = 4;

            this.CompressJpegMode_Combox.DataSource = Enum.GetNames(
                typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionMode));

            this.CompressJpegColorMode_Combox.DataSource = Enum.GetNames(
                typeof(Aspose.Imaging.FileFormats.Jpeg.JpegCompressionColorMode));
            this.CompressJpegColorMode_Combox.SelectedIndex = 4;

            this.CompressJpegQuailty_Combox.DataSource = Enumerable.Range(1, 100).ToList();
            this.CompressJpegQuailty_Combox.SelectedIndex = 79;

            this.CompressPngLevel_Combox.DataSource = Enumerable.Range(0, 10).ToList();
            this.CompressPngLevel_Combox.SelectedIndex = 3;

            this.CompressPngColorMode_Combox.DataSource = Enum.GetNames(
                typeof(Aspose.Imaging.FileFormats.Png.PngColorType));
            this.CompressPngColorMode_Combox.SelectedIndex = 1;

            this.CompressBmpMode_Combox.DataSource = Enum.GetNames(
                typeof(Aspose.Imaging.FileFormats.Bmp.BitmapCompression));


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

            this.Word2PDFOptimizCombox.DataSource = Enum.GetNames(typeof(Aspose.Words.Settings.MsWordVersion));
            this.Word2PDFOptimizCombox.SelectedIndex = 4; //默认为ms2010


            DataTable htm2pdf_page_size_table = new DataTable();
            htm2pdf_page_size_table.Columns.Add("DisplayValue");
            htm2pdf_page_size_table.Columns.Add("RealValue");
            htm2pdf_page_size_table.Rows.Add("A0", PuppeteerSharp.Media.PaperFormat.A0);
            htm2pdf_page_size_table.Rows.Add("A1", PuppeteerSharp.Media.PaperFormat.A1);
            htm2pdf_page_size_table.Rows.Add("A2", PuppeteerSharp.Media.PaperFormat.A2);
            htm2pdf_page_size_table.Rows.Add("A3", PuppeteerSharp.Media.PaperFormat.A3);
            htm2pdf_page_size_table.Rows.Add("A4", PuppeteerSharp.Media.PaperFormat.A4);
            htm2pdf_page_size_table.Rows.Add("A5", PuppeteerSharp.Media.PaperFormat.A5);
            htm2pdf_page_size_table.Rows.Add("A6", PuppeteerSharp.Media.PaperFormat.A6);
            htm2pdf_page_size_table.Rows.Add("Ledger", PuppeteerSharp.Media.PaperFormat.Ledger);
            htm2pdf_page_size_table.Rows.Add("Legal", PuppeteerSharp.Media.PaperFormat.Legal);
            htm2pdf_page_size_table.Rows.Add("Letter", PuppeteerSharp.Media.PaperFormat.Letter);
            htm2pdf_page_size_table.Rows.Add("Tabloid", PuppeteerSharp.Media.PaperFormat.Tabloid);

            this.html2pdf_combox_pagesize.DataSource = htm2pdf_page_size_table;

            this.html2pdf_combox_pagesize.DataSource = htm2pdf_page_size_table;
            this.html2pdf_combox_pagesize.DisplayMember = "DisplayValue";
            this.html2pdf_combox_pagesize.ValueMember = "RealValue";
            this.html2pdf_combox_pagesize.SelectedIndex = 4;

            DataTable html2pdf_pagedirect = new DataTable();
            html2pdf_pagedirect.Columns.Add("DisplayValue");
            html2pdf_pagedirect.Columns.Add("RealValue");
            html2pdf_pagedirect.Rows.Add("竖向", false);
            html2pdf_pagedirect.Rows.Add("横向", true);

            this.html2pdf_combox_pagedirect.DataSource = html2pdf_pagedirect;
            this.html2pdf_combox_pagedirect.DisplayMember = "DisplayValue";
            this.html2pdf_combox_pagedirect.ValueMember = "RealValue";

            foreach (TabPage tempPage in MainTabPage.TabPages)
            {
                this.menu_start_page_index_combox.Items.Add(MainTabPage.TabPages.IndexOf(tempPage));
            }

            List<string> On_Off_Source = new List<string> { "开启", "关闭" };

            ArrayList TempHeightArray = new ArrayList();

            Enumerable.Range(1, 256)
                .ToList()
                .ForEach(
                    x =>
                    {
                        TempHeightArray.Add(x);
                    });


            this.menu_tabpage_heigh_light_combox.Items.AddRange(On_Off_Source.ToArray());
            this.menu_tabpage_icon_combox.Items.AddRange(On_Off_Source.ToArray());
            this.menu_listview_gridline_combox.Items.AddRange(On_Off_Source.ToArray());
            this.menu_finshined_messbox_combox.Items.AddRange(On_Off_Source.ToArray());
            this.menu_listview_line_heigh_combox.Items.AddRange(TempHeightArray.ToArray());
            this.menu_widget_anchor_combox.Items.AddRange(On_Off_Source.ToArray());

            DataTable barcodeType = new DataTable();
            barcodeType.Columns.Add("DisplayValue");
            barcodeType.Columns.Add("RealValue");
            barcodeType.Rows.Add("QR二维码", Aspose.BarCode.Generation.EncodeTypes.QR);
            barcodeType.Rows.Add("DataMatrix二维码", Aspose.BarCode.Generation.EncodeTypes.DataMatrix);
            barcodeType.Rows.Add("PDF417条码", Aspose.BarCode.Generation.EncodeTypes.Pdf417);
            barcodeType.Rows.Add("Atzec二维码", Aspose.BarCode.Generation.EncodeTypes.Aztec);
            barcodeType.Rows.Add("汉信二维码", Aspose.BarCode.Generation.EncodeTypes.HanXin);


            this.barcodeTypeCombox.DataSource = barcodeType;
            this.barcodeTypeCombox.DisplayMember = "DisplayValue";
            this.barcodeTypeCombox.ValueMember = "RealValue";


            DataTable barcodeAutoSizeMode = new DataTable();
            barcodeAutoSizeMode.Columns.Add("DisplayValue");
            barcodeAutoSizeMode.Columns.Add("RealValue");
            barcodeAutoSizeMode.Rows.Add("无", Aspose.BarCode.Generation.AutoSizeMode.None);
            barcodeAutoSizeMode.Rows.Add("自动", Aspose.BarCode.Generation.AutoSizeMode.Nearest);
            barcodeAutoSizeMode.Rows.Add("手动调整", Aspose.BarCode.Generation.AutoSizeMode.Interpolation);

            this.barcodeAutoSizeCombox.DataSource = barcodeAutoSizeMode;
            this.barcodeAutoSizeCombox.DisplayMember = "DisplayValue";
            this.barcodeAutoSizeCombox.ValueMember = "RealValue";
            this.barcodeAutoSizeCombox.SelectedIndex = 1;

            this.barcodeDpiCombox.DataSource = Enumerable.Range(50, 300).Where(x => x % 50 == 0).ToList();
            this.barcodeDpiCombox.SelectedIndex = 2;


            DataTable barcodeTextLocation = new DataTable();
            barcodeTextLocation.Columns.Add("DisplayValue");
            barcodeTextLocation.Columns.Add("RealValue");
            barcodeTextLocation.Rows.Add("不显示", Aspose.BarCode.Generation.CodeLocation.None);
            barcodeTextLocation.Rows.Add("条码上方", Aspose.BarCode.Generation.CodeLocation.Above);
            barcodeTextLocation.Rows.Add("条码下方", Aspose.BarCode.Generation.CodeLocation.Below);

            this.barcodeTextLocationCombox.DataSource = barcodeTextLocation;
            this.barcodeTextLocationCombox.DisplayMember = "DisplayValue";
            this.barcodeTextLocationCombox.ValueMember = "RealValue";

            DataTable barcodeTextAlign = new DataTable();
            barcodeTextAlign.Columns.Add("DisplayValue");
            barcodeTextAlign.Columns.Add("RealValue");
            barcodeTextAlign.Rows.Add("左对齐", Aspose.BarCode.Generation.TextAlignment.Left);
            barcodeTextAlign.Rows.Add("右对齐", Aspose.BarCode.Generation.TextAlignment.Right);
            barcodeTextAlign.Rows.Add("居中对齐", Aspose.BarCode.Generation.TextAlignment.Center);

            this.barcodeTextAlignCombox.DataSource = barcodeTextAlign;
            this.barcodeTextAlignCombox.DisplayMember = "DisplayValue";
            this.barcodeTextAlignCombox.ValueMember = "RealValue";
            this.barcodeTextAlignCombox.SelectedIndex = 2;

            this.barcodeCaptionLocationCombox.DataSource = new string[] { "不显示", "条码上方", "条码下方" };

            this.barcodeScanTypeCombox.DataSource = new string[] { "普通文本", "网址" };

            DataTable webp2image_format = new DataTable();
            webp2image_format.Columns.Add("DisplayValue");
            webp2image_format.Columns.Add("RealValue");
            webp2image_format.Rows.Add("JPG", ".jpg");
            webp2image_format.Rows.Add("JPEG", ".jpeg");
            webp2image_format.Rows.Add("PNG", ".png");
            webp2image_format.Rows.Add("BMP", ".bmp");
            webp2image_format.Rows.Add("GIF", ".gif");


            this.webp2image_combox.DataSource = webp2image_format;
            this.webp2image_combox.DisplayMember = "DisplayValue";
            this.webp2image_combox.ValueMember = "RealValue";


            DataTable VoteSquareType = new DataTable();
            VoteSquareType.Columns.Add("DisplayValue");
            VoteSquareType.Columns.Add("RealValue");
            VoteSquareType.Rows.Add("赞成不赞成(正选)", "赞成（票面直接有）|不赞成（票面直接有）|弃权（规则运算出）");
            VoteSquareType.Rows.Add("赞成不赞成(反选)", "赞成（规则运算出）|不赞成（票面直接有）|弃权（票面直接有）");
            VoteSquareType.Rows.Add("赞成反对(正选)", "赞成（票面直接有）|反对（票面直接有）|弃权（规则运算出）");
            VoteSquareType.Rows.Add("赞成反对(反选)", "赞成（规则运算出）|反对（票面直接有）|弃权（票面直接有）");
            VoteSquareType.Rows.Add("同意反对(正选)", "同意（票面直接有）|反对（票面直接有）|弃权（规则运算出）");
            VoteSquareType.Rows.Add("同意反对(反选)", "同意（规则运算出）|反对（票面直接有）|弃权（票面直接有）");
            VoteSquareType.Rows.Add("同意不赞成(正选)", "同意（票面直接有）|不赞成（票面直接有）|弃权（规则运算出）");
            VoteSquareType.Rows.Add("同意不赞成(反选)", "同意（规则运算出）|不赞成（票面直接有）|弃权（票面直接有）");


            this.votepage_type_combox.DataSource = VoteSquareType;
            this.votepage_type_combox.DisplayMember = "DisplayValue";
            this.votepage_type_combox.ValueMember = "RealValue";

            DataTable VotepageIndexTable = new DataTable();
            VotepageIndexTable.Columns.Add("DisplayValue");
            VotepageIndexTable.Columns.Add("RealValue");
            VotepageIndexTable.Rows.Add("第1页", 0);
            VotepageIndexTable.Rows.Add("第2页", 1);

            this.votepageIndexCombox.DataSource = VoteSquareType;
            this.votepageIndexCombox.DisplayMember = "DisplayValue";
            this.votepageIndexCombox.ValueMember = "RealValue";


            this.votepageIndexCombox.DataSource = VotepageIndexTable;


            DataTable Word2Pdf_ColorMode = new DataTable();
            Word2Pdf_ColorMode.Columns.Add("DisplayValue");
            Word2Pdf_ColorMode.Columns.Add("RealValue");
            Word2Pdf_ColorMode.Rows.Add("彩色", Aspose.Words.Saving.ColorMode.Normal);
            Word2Pdf_ColorMode.Rows.Add("黑白", Aspose.Words.Saving.ColorMode.Grayscale);

            this.word2pdf_colorMode.DataSource = Word2Pdf_ColorMode;
            this.word2pdf_colorMode.DisplayMember = "DisplayValue";
            this.word2pdf_colorMode.ValueMember = "RealValue";

            DataTable NameMarkColor = new DataTable();
            NameMarkColor.Columns.Add("DisplayValue");
            NameMarkColor.Columns.Add("RealValue");
            NameMarkColor.Rows.Add("黄色", System.Drawing.Color.Yellow.Name);
            NameMarkColor.Rows.Add("森林绿", System.Drawing.Color.ForestGreen.Name);
            NameMarkColor.Rows.Add("红色", System.Drawing.Color.Red.Name);
            NameMarkColor.Rows.Add("爱丽丝蓝", System.Drawing.Color.AliceBlue.Name);
            NameMarkColor.Rows.Add("古董白", System.Drawing.Color.AntiqueWhite.Name);
            NameMarkColor.Rows.Add("湖绿色", System.Drawing.Color.Aqua.Name);
            NameMarkColor.Rows.Add("海蓝宝石色", System.Drawing.Color.Aquamarine.Name);
            NameMarkColor.Rows.Add("白杏色", System.Drawing.Color.BlanchedAlmond.Name);

            this.NameMarkColor.DataSource = NameMarkColor;
            this.NameMarkColor.DisplayMember = "DisplayValue";
            this.NameMarkColor.ValueMember = "RealValue";

            DataTable PauseUpdateData = new DataTable();
            PauseUpdateData.Columns.Add("DisplayValue");
            PauseUpdateData.Columns.Add("RealValue");
            PauseUpdateData.Rows.Add("5年", 365 * 5);
            PauseUpdateData.Rows.Add("10年", 365 * 10);
            PauseUpdateData.Rows.Add("15年", 365 * 15);
            PauseUpdateData.Rows.Add("20年", 365 * 20);
            PauseUpdateData.Rows.Add("30年", 365 * 30);
            PauseUpdateData.Rows.Add("40年", 365 * 40);
            PauseUpdateData.Rows.Add("50年", 365 * 50);
            PauseUpdateData.Rows.Add("60年", 365 * 60);
            PauseUpdateData.Rows.Add("70年", 365 * 70);
            PauseUpdateData.Rows.Add("80年", 365 * 80);
            PauseUpdateData.Rows.Add("90年", 365 * 90);
            PauseUpdateData.Rows.Add("100年", 365 * 100);

            this.PauseUpdateYears.DataSource = PauseUpdateData;
            this.PauseUpdateYears.DisplayMember = "DisplayValue";
            this.PauseUpdateYears.ValueMember = "RealValue";


            string[] ExamDataFormatList = new string[] { "该人得票数", "该人得票率", "该人得分", "该人得分率" };

            this.ExamReplaceDataFormat.DataSource = ExamDataFormatList;
            this.ExamReportParaDataFormat_Combox.DataSource = ExamDataFormatList.Clone();


            this.SplitPdfSizeCombox.DataSource = Enum.GetValues(typeof(Aspose.Slides.SlideSizeType));

            DataTable PPTScaleMode = new DataTable();
            PPTScaleMode.Columns.Add("DisplayValue");
            PPTScaleMode.Columns.Add("RealValue");
            PPTScaleMode.Rows.Add("最大化", Aspose.Slides.SlideSizeScaleType.Maximize);
            PPTScaleMode.Rows.Add("合适大小", Aspose.Slides.SlideSizeScaleType.EnsureFit);
            PPTScaleMode.Rows.Add("不缩放", Aspose.Slides.SlideSizeScaleType.DoNotScale);

            this.SplitPdfScaleCombox.DataSource = PPTScaleMode;
            this.SplitPdfScaleCombox.DisplayMember = "DisplayValue";
            this.SplitPdfScaleCombox.ValueMember = "RealValue";

            this.ExamReportParaDataFormatReverse_Combox.DataSource = new string[]
            {
                "不保留",
                "保留1位",
                "保留2位",
                "不保留并加百分号",
                "保留1位并加百分号",
                "保留2位并加百分号"
            };


            //加载城市数据到combox中
            this.city_level_1_combox.Items.Clear();//先清空数据
            if (City_Json != null)
            {
                foreach (JToken City_1_Name in City_Json)
                {
                    city_level_1_combox.Items.Add(City_1_Name["name"]);//添加省级信息
                }
                city_level_1_combox.SelectedIndex = 0;
            }


            //修改下拉样式
            GetFormMembers(typeof(System.Windows.Forms.ComboBox))
                .ForEach(
                    x =>
                    {
                        System.Windows.Forms.ComboBox TempCombox = x as System.Windows.Forms.ComboBox;
                        TempCombox.DropDownStyle = ComboBoxStyle.DropDownList;
                    });

            GetFormMembers(typeof(ToolStripComboBox))
                .ForEach(
                    x =>
                    {
                        ToolStripComboBox TempCombox = x as ToolStripComboBox;
                        TempCombox.DropDownStyle = ComboBoxStyle.DropDownList;
                    });
        }

        /// <summary>
        /// 通过反射获取程序内全部的成员
        /// </summary>
        /// <param name="controlType"></param>
        /// <returns></returns>
        private List<object> GetFormMembers(Type controlType)
        {
            List<object> controlTypes = new List<object>();
            GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .ToList()
                .ForEach(
                    x =>
                    {
                        if (x.FieldType == controlType)
                        {
                            object TempObj = x.GetValue(this);

                            if (TempObj != null)
                            {
                                controlTypes.Add(TempObj);
                            }
                        }
                    });
            return controlTypes;
        }

        //注册移动Listview项目的按钮事件
        private void InitMoveItemButtonHandler()
        {
            #region 上下移动项目位置按钮注册点击事件
            GetFormMembers(typeof(System.Windows.Forms.Button))
                .ForEach(
                    x =>
                    {
                        System.Windows.Forms.Button TempButton = x as System.Windows.Forms.Button;
                        string[] SplitButtonName = TempButton.Name.Split('_');

                        if (SplitButtonName.Length == 2)//只有确实分割了才处理
                        {
                            GetFormMembers(typeof(System.Windows.Forms.ListView))
                                .ForEach(
                                    listview =>
                                    {
                                        System.Windows.Forms.ListView TempListView = listview as System.Windows.Forms.ListView;
                                        if (SplitButtonName[1].Contains("MoveUp") &&
                                            string.Compare(
                                            TempListView.Name,
                                            SplitButtonName[0],
                                            StringComparison.Ordinal) ==
                                            0)
                                        {
                                            TempButton.Click += (s1, s2) =>
                                            {
                                                UI.MoveUpListViewItem(TempListView);
                                            };
                                        }
                                        else if (SplitButtonName[1].Contains("MoveDown") &&
                                            string.Compare(
                                            TempListView.Name,
                                            SplitButtonName[0],
                                            StringComparison.Ordinal) ==
                                            0)
                                        {
                                            TempButton.Click += (s1, s2) =>
                                            {
                                                UI.MoveDownListViewItem(TempListView);
                                            };
                                        }
                                    });
                        }
                    });
            #endregion
        }

        //注册删除Listview项目的事件

        private void InitRemoveItemHandler()
        {
            word2pdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.WordFileListView);
            };

            pdf2word_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.PdfFileListView);
            };

            excel2pdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.ExcelFileListView);
            };

            pdf2excel_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.PDF2ExcelFileView);
            };

            ppt2pdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.PPT2PdfFileList);
            };

            pdf2ppt_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.PDF2PPTFileList);
            };

            meregeword_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.MergeWordFileList);
            };

            meregepdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.MergePdfFileView);
            };

            sqplitpdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.SplitPdfFileList);
            };

            addpdfwater_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.PdfWaterMarkFileList);
            };

            removepdfwater_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.RemovePdfWaterMarkFileList);
            };

            pdfversion_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.ConvertPdfComplianceFileList);
            };

            word2excel_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.Word2ExcelFileList);
            };

            excel2word_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.Excel2WordFileList);
            };

            document2image_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.DocumentsListView);
            };

            image2pdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.ConvertImage2PDFFileList);
            };

            compresspdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.CompressPdfFileList);
            };

            compressimage_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.CompressImageFileList);
            };

            wordreplace_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.ReplaceWordFileList);
            };

            excelreplace_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.ReplaceExcelFileView);
            };

            datamatrix_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.DecodeMatrixWordListView);
            };

            image2ico_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.IcoFileView);
            };

            word2epub_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.Word2EPUBFileList);
            };

            epub2word_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.EPUB2WORDFileList);
            };

            epub2pdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.EPUB2PDFFileList);
            };
            html2pdf_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.HTML2PDFFileList);
            };
            splitword_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.SplitWordFileList);
            };
            webp2image_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.WebpFileList);
            };
            splitppt_clear.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(true, this.SplitPPTFilesList);
            };


            //remove
            word2pdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.WordFileListView);
            };

            pdf2word_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.PdfFileListView);
            };

            excel2pdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.ExcelFileListView);
            };

            pdf2excel_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.PDF2ExcelFileView);
            };

            ppt2pdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.PPT2PdfFileList);
            };

            pdf2ppt_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.PDF2PPTFileList);
            };

            meregeword_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.MergeWordFileList);
            };

            meregepdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.MergePdfFileView);
            };

            sqplitpdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.SplitPdfFileList);
            };

            addpdfwater_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.PdfWaterMarkFileList);
            };

            removepdfwater_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.RemovePdfWaterMarkFileList);
            };

            pdfversion_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.ConvertPdfComplianceFileList);
            };

            word2excel_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.Word2ExcelFileList);
            };

            excel2word_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.Excel2WordFileList);
            };

            document2image_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.DocumentsListView);
            };

            image2pdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.ConvertImage2PDFFileList);
            };

            compresspdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.CompressPdfFileList);
            };

            compressimage_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.CompressImageFileList);
            };

            wordreplace_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.ReplaceWordFileList);
            };

            excelreplace_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.ReplaceExcelFileView);
            };

            datamatrix_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.DecodeMatrixWordListView);
            };

            image2ico_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.IcoFileView);
            };

            word2epub_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.Word2EPUBFileList);
            };

            epub2word_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.EPUB2WORDFileList);
            };
            epub2pdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.EPUB2PDFFileList);
            };

            html2pdf_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.HTML2PDFFileList);
            };
            splitword_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.SplitWordFileList);
            };
            webp2image_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.WebpFileList);
            };
            splitppt_remove.Click += (s1, s2) =>
            {
                UI.RemoveListViewItem(false, this.SplitPPTFilesList);
            };
        }

        //注册TooTip到符号按钮上
        private void InitToolTipHandler()
        {
            GetFormMembers(typeof(System.Windows.Forms.Button))
                .ForEach(
                    member =>
                    {
                        System.Windows.Forms.Button TempButton = member as System.Windows.Forms.Button;

                        if (TempButton.Name.Contains("_clear"))
                        {
                            MainToolsTip.SetToolTip(TempButton, "清空列表中所有文件");
                        }
                        else if (TempButton.Name.Contains("_remove"))
                        {
                            MainToolsTip.SetToolTip(TempButton, "移除选中文件");
                        }
                        else if (TempButton.Name.Contains("MoveDown"))
                        {
                            MainToolsTip.SetToolTip(TempButton, "向下移动选中文件");
                        }
                        else if (TempButton.Name.Contains("MoveUp"))
                        {
                            MainToolsTip.SetToolTip(TempButton, "向上移动选中文件");
                        }
                    });
        }

        //为tabpage添加图标
        private void InitTabPageIcon()
        {
            GetFormMembers(typeof(TabPage))
                .ForEach(
                    member =>
                    {
                        if (member is TabPage TempPage)
                        {
                            string TabPageCaption = TempPage.Text;
                            if (TabPageCaption.StartsWith("WORD转"))
                            {
                                TempPage.ImageIndex = 0;
                            }
                            else if (TabPageCaption.StartsWith("PDF"))
                            {
                                TempPage.ImageIndex = 1;
                            }
                            else if (TabPageCaption.StartsWith("EXCEL转"))
                            {
                                TempPage.ImageIndex = 2;
                            }
                            else if (TabPageCaption.StartsWith("PPT转"))
                            {
                                TempPage.ImageIndex = 3;
                            }
                            else if (TabPageCaption.StartsWith("WORD内"))
                            {
                                TempPage.ImageIndex = 0;
                            }
                            else if (TabPageCaption.StartsWith("EXCEL内"))
                            {
                                TempPage.ImageIndex = 1;
                            }
                            else if (TabPageCaption.StartsWith("EPUB"))
                            {
                                TempPage.ImageIndex = 4;
                            }
                            else if (TabPageCaption.StartsWith("压缩"))
                            {
                                TempPage.ImageIndex = 7;
                            }
                            else if (TabPageCaption.Contains("图片"))
                            {
                                TempPage.ImageIndex = 5;
                            }
                            else if (TabPageCaption.EndsWith("水印"))
                            {
                                TempPage.ImageIndex = 13;
                            }
                            else if (TabPageCaption.EndsWith("WORD"))
                            {
                                TempPage.ImageIndex = 0;
                            }
                            else if (TabPageCaption.EndsWith("PDF"))
                            {
                                TempPage.ImageIndex = 1;
                            }
                            else if (TabPageCaption.StartsWith("特殊"))
                            {
                                TempPage.ImageIndex = 8;
                            }
                            else if (TabPageCaption.StartsWith("Twain"))
                            {
                                TempPage.ImageIndex = 9;
                            }
                            else if (TabPageCaption.StartsWith("其他"))
                            {
                                TempPage.ImageIndex = 10;
                            }
                            else if (TabPageCaption.StartsWith("系统"))
                            {
                                TempPage.ImageIndex = 11;
                            }
                            else if (TabPageCaption.StartsWith("关于"))
                            {
                                TempPage.ImageIndex = 12;
                            }
                            else if (TabPageCaption.StartsWith("姓名"))
                            {
                                TempPage.ImageIndex = 14;
                            }
                            else if (TabPageCaption.StartsWith("MSS"))
                            {
                                TempPage.ImageIndex = 11;
                            }
                            else if (TabPageCaption.StartsWith("简易"))
                            {
                                TempPage.ImageIndex = 15;
                            }
                            else if (TabPageCaption.Contains("身份证"))
                            {
                                TempPage.ImageIndex = 16;
                            }
                            else if (TabPageCaption.Contains("文件副本"))
                            {
                                TempPage.ImageIndex = 17;
                            }
                            else if (TabPageCaption.Contains("网卡设置"))
                            {
                                TempPage.ImageIndex = 18;
                            }
                            else if (TabPageCaption.Contains("选举结果报告单样式生成"))
                            {
                                TempPage.ImageIndex = 19;
                            }
                            else if (TabPageCaption.Contains("单人表"))
                            {
                                TempPage.ImageIndex = 20;
                            }
                            else if (TabPageCaption.Contains("考评报表"))
                            {
                                TempPage.ImageIndex = 21;
                            }
                            else if (TabPageCaption.Contains("拆分PPT"))
                            {
                                TempPage.ImageIndex = 3;
                            }
                        }
                    });
        }


        private void ChooseDocButton_Click(object sender, EventArgs e)
        {
            Task<int> FileCount = UI.AddWordFilesAsync();
            this.StatusText.Text = $"{FileCount.Result}个待转换文档";
        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.WordFileListView,
                            this.StatusText,
                            ConvertFileType.Word2Pdf,
                            ".pdf",
                            word2pdf_save_source.Checked,
                            sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                    Debug.WriteLine(ex);
                                }));
                    }
                });
        }

        private void SelectPdfButton_Click(object sender, EventArgs e) { UI.AddPdfFiles(); }

        private void Convert2DocButton_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.PdfFileListView,
                            this.PdfStatusText,
                            ConvertFileType.Pdf2Word,
                            ".docx",
                            pdf2word_save_source.Checked,
                            sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectExcelFileButton_Click(object sender, EventArgs e) { UI.AddExcelFiles(); }

        private void ConvertExcel2PdfButton_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.ExcelFileListView,
                            this.Excel2PdfStatusText,
                            ConvertFileType.Excel2Pdf,
                            ".pdf",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void ConvertImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void ConvertWord2ImageButton_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.DocumentsListView,
                            this.ConvertWord2ImageStatus,
                            ConvertFileType.Word2Image,
                            this.ConvertImageList.SelectedValue.ToString(),
                            word2image_save_source.Checked,
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectWord2ImageFileButton_Click(object sender, EventArgs e) { UI.AddWord2ImageFiles(); }

        private void DpiItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void ConvertTextButton_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> TempLines = new List<string>();
                TargetTextBox.Lines = Util.AddText2List(
                    SourceTextBox.Text,
                    RemoveNumberCheck.Checked,
                    RemoveSquareCheck.Checked,
                    NameStorkeChcek.Checked,
                    NameReplaceWord.Checked,
                    NameReplaceWord_Search.Text,
                    NameReplaceWord_Target.Text);
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


                    if (NameAutoCopy.Checked && TargetTextBox.Text != string.Empty)
                    {
                        Clipboard.SetText(TargetTextBox.Text.Replace("\n", Environment.NewLine));//替换换行
                        NameAutoCopyStatusText.Text = "[已复制到剪贴板]";
                        NameAutoCopyStatusText.ForeColor = Color.ForestGreen;
                    }
                    else
                    {
                        NameAutoCopyStatusText.Text = string.Empty;
                    }

                    if (MarkDupName.Checked)
                    {
                        Task.Run(
                            () =>
                            {
                                // 清除之前的高亮显示
                                TargetTextBox.SelectAll();
                                TargetTextBox.SelectionBackColor = TargetTextBox.BackColor;

                                // 用于存储重复的文本行索引
                                var duplicateLineIndices = new HashSet<int>();

                                // 遍历富文本框中的文本行
                                for (int i = 0; i < TargetTextBox.Lines.Length; i++)
                                {
                                    string currentLine = TargetTextBox.Lines[i];

                                    // 在当前行之后的行中查找是否存在相同的文本行
                                    for (int j = i + 1; j < TargetTextBox.Lines.Length; j++)
                                    {
                                        if (currentLine.Equals(TargetTextBox.Lines[j]))
                                        {
                                            // 记录重复行的索引
                                            duplicateLineIndices.Add(i);
                                            duplicateLineIndices.Add(j);
                                        }
                                    }
                                }

                                if (duplicateLineIndices.Count > 0)
                                {
                                    // 高亮显示所有重复的文本行
                                    foreach (int index in duplicateLineIndices)
                                    {
                                        TargetTextBox.Select(
                                            TargetTextBox.GetFirstCharIndexFromLine(index),
                                            TargetTextBox.Lines[index].Length);
                                        TargetTextBox.SelectionBackColor = System.Drawing.Color
                                            .FromName(NameMarkColor.SelectedValue.ToString());
                                    }
                                    // 将焦点设置回富文本框，以便用户可以看到高亮显示的文本行
                                    TargetTextBox.Focus();
                                    TargetTextBox.Refresh();
                                }
                            });
                    }

                    if (DoubleTextClear.Checked)
                    {
                        this.TargetTextBox.Lines = this.TargetTextBox.Lines.ToList().Distinct().ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                this.BeginInvoke(
                    new Action(
                        () =>
                        {
                            (sender as System.Windows.Forms.Button).Enabled = true;
                            MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
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
        { TargetTextNumberText.Text = $"处理后姓名有{TargetTextBox.Lines.Length}个"; }

        private void MutilLineMode_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void SelectPDF2ExcelButton_Click(object sender, EventArgs e) { UI.AddPdf2ExcelFiles(); }

        private void ConvertPDF2Excel_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.PDF2ExcelFileView,
                            this.ConvertPDF2ExcelStatusText,
                            ConvertFileType.Pdf2Excel,
                            ".xlsx",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void MeregePdfFileButtonConvert_Click(object sender, EventArgs e)
        {
            Task<List<string>> MeregePdfFiles = UI.GetListViewFirstItemContentAsync(this.MergePdfFileView);
            if (MeregePdfFiles.Result.Count > 0)
            {
                string MeregePdfFileName = Util.ShowSaveFileDialog("PDF文档|.pdf", "设置文件名称");
                if (MeregePdfFileName != string.Empty)
                {
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                (sender as System.Windows.Forms.Button).Enabled = false;
                                string MeregeDirectory = System.IO.Path.GetDirectoryName(MeregePdfFileName);
                                this.MeregePdfFileStatusText.Text = "正在合并中...";
                                Util.MergerPdfFIle(MeregePdfFiles.Result, MeregePdfFileName);
                                this.MeregePdfFileStatusText.Text = "合并完毕，右键名称可移除文件";
                                (sender as System.Windows.Forms.Button).Enabled = true;
                                if (OpenMeregePdfFile.Checked)
                                {
                                    Process.Start(MeregePdfFileName);
                                }
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            DialogResult OpenResult = MessageBox.Show(
                                                "合并完毕，是否需要打开合并文件所在目录？",
                                                "提示",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Asterisk);
                                            if (OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(MeregeDirectory);
                                            }
                                        }));
                            }
                            catch (Exception ex)
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show(
                                                ex.Message,
                                                "错误",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                        }));
                            }
                        });
                }
            }
        }

        private void MergePdfFileSelectButton_Click(object sender, EventArgs e) { UI.AddMergePdfFiles(); }

        /// <summary>
        /// 删除选中项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void 删除本项ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListViewRightMenu.SourceControl.GetType() == typeof(System.Windows.Forms.ListView))
            {
                System.Windows.Forms.ListView TempListView = ListViewRightMenu.SourceControl as System.Windows.Forms.ListView;
                UI.RemoveListViewItem(false, TempListView);
            }
        }

        /// <summary>
        /// 各个listview的清空列表操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void 清空列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListViewRightMenu.SourceControl.GetType() == typeof(System.Windows.Forms.ListView))
            {
                System.Windows.Forms.ListView TempListView = ListViewRightMenu.SourceControl as System.Windows.Forms.ListView;
                UI.RemoveListViewItem(true, TempListView);
            }
        }

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
            System.Windows.Forms.ListView DragedListview = sender as System.Windows.Forms.ListView;
            string[] DropFiles = e.Data.GetData(DataFormats.FileDrop, false) as string[];
            switch (DragedListview.Name)
            {
                case "WordFileListView":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.WordFileListView,
                        new string[] { ".doc", ".docx", ".wps" });
                    break;
                case "PdfFileListView":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.PdfFileListView, new string[] { ".pdf" });
                    break;
                case "ExcelFileListView":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.ExcelFileListView,
                        new string[] { ".xls", ".xlsx", ".et" });
                    break;
                case "DocumentsListView":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.DocumentsListView,
                        new string[] { ".doc", ".docx", ".pdf", ".xls", ".xlsx" });
                    break;
                case "PDF2ExcelFileView":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.PDF2ExcelFileView, new string[] { ".pdf" });
                    break;
                case "MergePdfFileView":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.MergePdfFileView,
                        new string[] { ".pdf", ".jpg", ".png", ".bmp", ".tif" });
                    break;
                case "ReplaceWordFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.ReplaceWordFileList,
                        new string[] { ".doc", ".docx", ".wps" });
                    break;
                case "ReplaceExcelFileView":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.ReplaceExcelFileView,
                        new string[] { ".xls", ".xlsx" });
                    break;
                case "DecodeMatrixWordListView":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.DecodeMatrixWordListView,
                        new string[] { ".doc", ".docx", ".pdf" });
                    break;
                case "IcoFileView":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.IcoFileView,
                        new string[] { ".jpg", ".png", ".bmp" });
                    break;
                case "MergeWordFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.MergeWordFileList,
                        new string[] { ".doc", ".docx", ".wps" });
                    break;
                case "Word2ExcelFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.Word2ExcelFileList,
                        new string[] { ".doc", ".docx", ".wps" });
                    break;
                case "Excel2WordFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.Excel2WordFileList,
                        new string[] { ".xls", ".xlsx" });
                    break;
                case "SplitPdfFileList":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.SplitPdfFileList, new string[] { ".pdf" });
                    break;
                case "PdfWaterMarkFileList":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.PdfWaterMarkFileList, new string[] { ".pdf" });
                    break;
                case "RemovePdfWaterMarkFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.RemovePdfWaterMarkFileList,
                        new string[] { ".pdf" });
                    break;
                case "PPT2PdfFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.PPT2PdfFileList,
                        new string[] { ".ppt", ".pptx" });
                    break;
                case "PDF2PPTFileList":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.PDF2PPTFileList, new string[] { ".pdf" });
                    break;
                case "ConvertPdfComplianceFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.ConvertPdfComplianceFileList,
                        new string[] { ".pdf" });
                    break;
                case "ConvertImage2PDFFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.ConvertImage2PDFFileList,
                        new string[] { ".jpg", ".jpeg", ".png", ".bmp" });
                    break;
                case "CompressPdfFileList":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.CompressPdfFileList, new string[] { ".pdf" });
                    break;
                case "CompressImageFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.CompressImageFileList,
                        new string[] { ".jpg", ".jpeg", ".png", ".bmp" });
                    break;
                case "Word2EPUBFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.Word2EPUBFileList,
                        new string[] { ".doc", ".docx", ".wps" });
                    break;
                case "EPUB2WORDFileList":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.EPUB2WORDFileList, new string[] { ".epub" });
                    break;
                case "EPUB2PDFFileList":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.EPUB2PDFFileList, new string[] { ".epub" });
                    break;
                case "HTML2PDFFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.HTML2PDFFileList,
                        new string[] { ".html", ".htm", ".mhtml", ".mht" });
                    break;
                case "SplitWordFileList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.SplitWordFileList,
                        new string[] { ".doc", ".docx", ".wps" });
                    break;
                case "WebpFileList":
                    UI.AddFilesToListViewWithNotDialog(DropFiles, this.WebpFileList, new string[] { ".webp" });
                    break;
                case "SplitPPTFilesList":
                    UI.AddFilesToListViewWithNotDialog(
                        DropFiles,
                        this.SplitPPTFilesList,
                        new string[] { ".ppt", ".pptx" });
                    break;
                default:
                    break;
            }
            DragedListview.ListViewItemSorter = new ListViewItemComparer_ASC();
        }

        private void DownloadFrameWorkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://dotnet.microsoft.com/zh-cn/download/dotnet-framework/net48"); }

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
            //NTwain.PlatformInfo.Current.PreferNewDSM = false;
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
                GC.Collect();
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
                    if (dataSource.Capabilities.CapPowerSaveTime.IsSupported)
                    {
                        TwainSleepTimeSet.DataSource = dataSource.Capabilities.CapPowerSaveTime.GetValues().ToList();
                        TwainSleepTimeSet.SelectedItem = dataSource.Capabilities.CapPowerSaveTime.GetCurrent();
                        TwainSetDeviceSaveTime.Enabled = true;
                    }
                    else
                    {
                        TwainSleepTimeSet.DataSource = new string[] { "不支持" };
                        TwainSetDeviceSaveTime.Enabled = false;
                    }
                    if (dataSource.Capabilities.ICapXResolution.IsSupported ||
                        dataSource.Capabilities.ICapYResolution.IsSupported)
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
                        ScanSerialNumberTextBox.Text = $"{dataSource.Capabilities.CapSerialNumber.GetValues().FirstOrDefault()}";
                    }
                    else
                    {
                        ScanSerialNumberTextBox.Text = $"无";
                    }
                    if (this.ExportCaps.Checked)
                    {
                        //输出扫描仪支持的设置参数
                        string DebugName = $"{dataSource.Name}_Support_Capabilities.txt";
                        if (File.Exists(DebugName))
                        {
                            File.Delete(DebugName);
                        }
                        dataSource.Capabilities.CapSupportedCaps
                            .GetValues()
                            .ToList()
                            .ForEach(
                                x =>
                                {
                                    StreamWriter sr = File.AppendText(DebugName);
                                    sr.WriteLine(x);
                                    sr.Close();
                                });
                    }
                    dataSource.Close();
                }
            }
        }

        private void SetTwainDevice_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            dataSource ??= twainSession.DefaultSource;

            if (dataSource.Open() == ReturnCode.Success)
            {
                dataSource.Enable(SourceEnableMode.ShowUIOnly, false, IntPtr.Zero);
            }
        }

        private void StartScanButton_Click(object sender, EventArgs e)
        {
            if (this.ScanPictureSavePathBox.Text != string.Empty)
            {
                TempImageCount = 0;
                ScanSeconds = 0;
                dataSource ??= twainSession.DefaultSource;
                ImageIndex = Directory.GetFiles(this.ScanPictureSavePathBox.Text).Length;

                if (dataSource.Open() == ReturnCode.Success)
                {
                    #region 扫描仪参数设置
                    //if (dataSource.Capabilities.ICapPixelType.CanSet)
                    //{
                    //    if (TwainColorMode.Items.Count > 0)
                    //    {
                    //        dataSource.Capabilities.ICapPixelType
                    //            .SetValue((NTwain.Data.PixelType)TwainColorMode.SelectedItem);
                    //    }
                    //}
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
                    //if (dataSource.Capabilities.ICapSupportedSizes.CanSet)
                    //{
                    //    if (TwainPaperSizeBox.Items.Count > 0)
                    //    {
                    //        dataSource.Capabilities.ICapSupportedSizes
                    //            .SetValue((SupportedSize)TwainPaperSizeBox.SelectedItem);
                    //    }
                    //}
                    //设置本次会话中需要获取的文件数量
                    if (TwainScanNumberSet.Text != string.Empty &&
                        Int32.TryParse(TwainScanNumberSet.Text, out int TwainScanNumberSetInt))
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
                        (sender as System.Windows.Forms.Button).Enabled = false;
                        dataSource.Enable(SourceEnableMode.ShowUI, false, IntPtr.Zero);
                        (sender as System.Windows.Forms.Button).Enabled = true;
                        ScanStopWatch = new Stopwatch();
                    }
                    else
                    {
                        if (dataSource.Capabilities.CapUIControllable.IsSupported)
                        {
                            (sender as System.Windows.Forms.Button).Enabled = false;
                            dataSource.Enable(SourceEnableMode.NoUI, false, IntPtr.Zero);
                            (sender as System.Windows.Forms.Button).Enabled = true;
                            ScanStopWatch = new Stopwatch();
                        }
                    }
                    #endregion
                }
            }
        }

        private void Twain_Scanning(object sender, DataTransferredEventArgs e)
        {
            using System.Drawing.Image TempScannedImage = System.Drawing.Image.FromStream(e.GetNativeImageStream());
            if (this.ScanPictureSavePathBox.Text != string.Empty)
            {
                ScanStopWatch.Start();
                ++TempImageCount;//每次扫描的时候进入纸张数量，扫描完毕后需要在开始扫描按钮重置该值为0
                ++TotalTempImageCount;//程序运行期间，扫描纸张的总数量，和上面的不同之处在于，点击开始扫描以后不进行重置值
                string SaveImageName = System.IO.Path
                    .Combine(ScanPictureSavePathBox.Text, $"ScanImage_{++ImageIndex}.jpg");//Imageindex为全局属性，值为目录内的文件数量，防止下次扫描原图片被覆盖
                TempScannedImage.Save(SaveImageName, System.Drawing.Imaging.ImageFormat.Jpeg);
                this.PreviewPictureName.Text = $"图片名称：{System.IO.Path.GetFileName(SaveImageName)}";

                this.NowScanNumber.Text = $"本次扫描数量：{TempImageCount}";
                this.ScannTotalNumber.Text = $"扫描总数量：{TotalTempImageCount}";
                this.SavedPathFileCount.Text = $"扫描路径内文件数量：{Directory.GetFiles(this.ScanPictureSavePathBox.Text).Length}";
                ScanStopWatch.Stop();
                ScanSeconds += ScanStopWatch.Elapsed.TotalSeconds;//一个扫描过程中花费的时间值，值将会在开始扫描按钮中重置为0
                //PictureBoxPreviewImage = new Bitmap(SaveImageName);
                //this.PreviewPictureBox.Image = PictureBoxPreviewImage;
                //TempScannedImageBmpList.Add(PictureBoxPreviewImage);//添加bitmap对象到列表中
            }
        }

        private void SelectScanPath_Click(object sender, EventArgs e)
        {
            IEnumerable<string> IDirectoryString = Util.ShowSelectDialog(false, true, string.Empty, string.Empty);
            string DirectoryString = IDirectoryString.FirstOrDefault();
            if (string.Compare(DirectoryString, default, StringComparison.Ordinal) != 0)
            {
                this.ScanPictureSavePathBox.Text = DirectoryString;
                this.SavedPathFileCount.Text = $"扫描路径内文件数量：{Directory.GetFiles(DirectoryString).Length}";
            }
        }

        private void MemoryTimer_Tick(object sender, EventArgs e)
        {
            var MemoryInfo = Util.GetProcessUsedMemory();

            double MemoryUsedPercent = Math.Round((Util.Byte2GB((long)MemoryInfo.PUM) / MemoryInfo.MTM) * 100.0, 2);

            this.MemoryStatusLabel.Text = $"占用内存：{MemoryInfo.PUM / 1024.0 / 1024.0:f2}MB|可用内存：{MemoryInfo.MFM:f2}GB|总内存：{MemoryInfo.MTM:f2}GB|程序占用比例（{MemoryUsedPercent}%）";

            this.MemoryProgressBar.Maximum = (int)MemoryInfo.MTM;
            this.MemoryProgressBar.Value = (int)(Util.Byte2GB((long)MemoryInfo.PUM));

            this.MemoryProgressBar.ToolTipText = $"程序占用比例：{MemoryUsedPercent}%";
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
                        TempScannedImageBmpList.ForEach(
                            ScannBmp =>
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

        private void SelectReplaceWordFilesButton_Click(object sender, EventArgs e) { UI.AddWordReplaceFiles(); }

        private void ReplaceWordContentButton_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            ReplaceWordFileList,
                            ReplaceWordContentStatusText,
                            ConvertFileType.WordReplace,
                            ".docx",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void TurialRegex_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void OnlineRegextTool_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://c.runoob.com/front-end/854/"); }

        private void SelectRelaceExcelButton_Click(object sender, EventArgs e) { UI.AddExcelReplaceFiles(); }

        private void ReplaceExcelContentButton_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            ReplaceExcelFileView,
                            ReplaceExcelStatusText,
                            ConvertFileType.ExcelReplace,
                            ".xlsx",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void ExcelRegexReadMe_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://c.runoob.com/front-end/854/"); }

        private void TabPage_12_MouseDown(object sender, MouseEventArgs e)
        {
        }

        private void DecodeDataMarix_Click(object sender, EventArgs e)
        {
            int DecodeCount = 0;
            DecodeDataMatrixView.Nodes.Clear();
            Task.Run(
                () =>
                {
                    UI.GetListViewFirstItemContentAsync(this.DecodeMatrixWordListView).Result
                        .ForEach(
                            FileName =>
                            {
                                ++DecodeCount;
                                DecodeDataMatixStatusText.Text = $"正在解析第{DecodeCount}个文件";
                                Util.ReadDocumentDataMatrix(FileName, DecodeDataMatrixView);
                            });
                    DecodeDataMatixStatusText.Text = $"处理完毕";
                });
        }

        private void SelectDataMatrixWord_Click(object sender, EventArgs e) { UI.AddDataMatrixWordFiles(); }

        private void 展开折叠本节点全部ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.TreeNode CurrentSelectNode = this.DecodeDataMatrixView.SelectedNode;
            CurrentSelectNode.ExpandAll();
        }

        private void 展开折叠全部节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (System.Windows.Forms.TreeNode TempNode in DecodeDataMatrixView.Nodes)
            {
                TempNode.ExpandAll();
            }
        }

        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(this.DecodeDataMatrixView.SelectedNode.Text))
                Process.Start(this.DecodeDataMatrixView.SelectedNode.Text);
        }

        private void DownloadNetfx35_Click(object sender, EventArgs e)
        {
            string Netfx35_Private = "http://jdsnmsl.tpddns.cn:3002/directlink/default/Windows%E8%BD%AF%E4%BB%B6/GorunTime%E8%BF%90%E8%A1%8C%E5%BA%93%E5%90%88%E9%9B%86/GoRuntime_DotNetFramework_3.x.zip";
            //string Netfx35_Private = "https://i-920.lanzouc.com:446/11271300149682909bb/2023/11/27/5404769fd0f075f33072184f6765a12b.zip?st=tf2aLH7S1zIferEFhUiCfA&e=1701063569&b=UkddMlACVCRYY1NxBj9UPQMzCAsNSAFoBiEISwVnVyFSRgh6BmcDawIyU3IGOQZwBDoKCVBiACgFLlx_aUSxSOVJw&fi=149682909&pid=117-81-188-214&up=2&mp=0&co=0";
            Util.DownloadFile(Netfx35_Private, "Net35.zip", Downloadfx35Progress, ProcessLabel);
        }

        private void LanzouNetfx_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://wwqb.lanzout.com/i2VL61g371kj"); }

        private void ReleaseRegistry_Click(object sender, EventArgs e)
        {
            string SaveFileName = Util.ShowSaveFileDialog("注册表文件|*.reg");

            if (SaveFileName != string.Empty)
            {
                object RegistryString = Util.GetEmbedResourceObj("注册");

                File.WriteAllText(SaveFileName, RegistryString.ToString(), Encoding.Default);

                MessageBox.Show("释放完成，请用管理员身份运行本文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void Label9_DoubleClick(object sender, EventArgs e)
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

        private void SelectFilesIco_Click(object sender, EventArgs e) { UI.AddIcoConvertFiles(); }

        private void ConvertImage2Ico_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.IcoFileView,
                            this.Image2IcoStatusText,
                            ConvertFileType.Image2Ico,
                            ".ico",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void BodyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ChangeReportItemTempHelpPdfFile != string.Empty)
            {
                File.Delete(ChangeReportItemTempHelpPdfFile);
            }
            Environment.Exit(0);
        }

        private void MainTabPage_DoubleClick(object sender, EventArgs e)
        {
            //if(this.MainTabPage.Multiline)
            //{
            //    this.Height -= 46;
            //    this.MainTabPage.Height -= 48;
            //    this.MainTabPage.Multiline = false;
            //} else
            //{
            //    this.Height += 46;
            //    this.MainTabPage.Height += 48;
            //    this.MainTabPage.Multiline = true;
            //}
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://www.52pojie.cn/thread-1769111-1-1.html"); }


        private void SelectMergeWordFiles_Click(object sender, EventArgs e) { UI.AddMergeWordFiles(); }

        private void MergeWordFile_Click(object sender, EventArgs e)
        {
            Task<List<string>> MergeWordFiles = UI.GetListViewFirstItemContentAsync(this.MergeWordFileList);
            if (MergeWordFiles.Result.Count > 0)
            {
                string MergeWordFileName = Util.ShowSaveFileDialog("WORD文档|.docx", "设置文件名称");
                if (MergeWordFileName != string.Empty)
                {
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                (sender as System.Windows.Forms.Button).Enabled = false;
                                string MeregeDirectory = System.IO.Path.GetDirectoryName(MergeWordFileName);
                                this.MeregeWordStatusText.Text = "正在合并中...";
                                Util.MergeWordFile(MergeWordFiles.Result, MergeWordFileName);
                                this.MeregeWordStatusText.Text = "合并完毕，右键名称可移除文件";
                                (sender as System.Windows.Forms.Button).Enabled = true;
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            DialogResult OpenResult = MessageBox.Show(
                                                "合并完毕，是否需要打开合并文件所在目录？",
                                                "提示",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Asterisk);
                                            if (OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(MeregeDirectory);
                                            }
                                        }));
                            }
                            catch (Exception ex)
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show(
                                                ex.Message,
                                                "错误",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                        }));
                            }
                        });
                }
            }
        }

        private void SelectWord2ExcelFiles_Click(object sender, EventArgs e) { UI.AddWord2ExcelFiles(); }

        private void ConvertWord2Excel_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.Word2ExcelFileList,
                            this.ConverWord2ExcelStatusText,
                            ConvertFileType.Word2Excel,
                            ".xlsx",
                            word2excel_save_source.Checked,
                            sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectExcel2WordFiles_Click(object sender, EventArgs e) { UI.AddExcel2WordFiles(); }


        private void ConvertExcel2Word_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.Excel2WordFileList,
                            this.ConvertExcel2WordStatusText,
                            ConvertFileType.Excel2Word,
                            ".docx",
                            excel2word_save_source.Checked,
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectSplitPdf_Click(object sender, EventArgs e) { UI.AddSplitPdfFiles(); }

        private void StartSplitPdf_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.SplitPdfFileList,
                            this.SplitPdfStatusText,
                            ConvertFileType.SplitPdf,
                            ".pdf",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectWaterMarkPdfFiles_Click(object sender, EventArgs e) { UI.AddWaterMarkPdfFiles(); }


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
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.PdfWaterMarkFileList,
                            this.AddWaterMark2PdfStatusText,
                            ConvertFileType.AddWaterMarkPdf,
                            ".pdf",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectRemovePdfWaterMarkFiles_Click(object sender, EventArgs e) { UI.AddRemoveWaterFiles(); }


        private void RemovePdfWaterMark_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.RemovePdfWaterMarkFileList,
                            this.RemovePdfWaterMarkStatusText,
                            ConvertFileType.RemovePdfWaterMark,
                            ".pdf",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void AddBackGroudImage_Click(object sender, EventArgs e)
        {
            IEnumerable<string> BackGroundImageFileNames = Util.ShowSelectDialog(
                false,
                false,
                "图片文件",
                "*.jpg;*png;*.jpeg;*.bmp");

            if (string.Compare(BackGroundImageFileNames.FirstOrDefault(), default, StringComparison.Ordinal) != 0)
            {
                AddWaterMarkBackGroundImageFileName = BackGroundImageFileNames.FirstOrDefault();

                System.Drawing.Image TempLoadImage = System.Drawing.Image.FromFile(AddWaterMarkBackGroundImageFileName);

                WaterMarkImage.Image = TempLoadImage;

                this.WaterMarkImageHeight.Text = TempLoadImage.Height.ToString();

                this.WaterMarkImageWidth.Text = TempLoadImage.Width.ToString();
            }
        }

        private void ValidTextBoxContent(object sender, CancelEventArgs e)
        {
            System.Windows.Forms.TextBox TempTextBox = (System.Windows.Forms.TextBox)sender;

            string TempText = TempTextBox.Text;

            if (!IsNumber(TempText))
            {
                e.Cancel = true;
                MainFormError.SetError(TempTextBox, "请输入正确的数字");
            }
        }

        private void ValidedTextBoxContent(object sender, EventArgs e) { MainFormError.Clear(); }

        private bool IsNumber(string InputString)
        {
            Regex TempRegex = new Regex("^[0-9]*$");

            bool NumberStatus = TempRegex.IsMatch(InputString);

            return NumberStatus;
        }

        #region 重绘tabpage的标签颜色
        private void MainTabPage_DrawItem(object sender, DrawItemEventArgs e)
        {
            System.Drawing.Font PageFont;
            Brush BackBrush, ForeBrush;

            Rectangle TabRec = e.Bounds;

            TabRec = new Rectangle(TabRec.X, TabRec.Y + 4, TabRec.Width, TabRec.Height - 4);


            if (e.Index == this.MainTabPage.SelectedIndex)
            {
                PageFont = new System.Drawing.Font(e.Font, FontStyle.Regular);
                BackBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    e.Bounds,
                    SystemColors.Control,
                    SystemColors.Control,
                    System.Drawing.Drawing2D.LinearGradientMode.BackwardDiagonal);

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

        private void SelectPPT2PdfFile_Click(object sender, EventArgs e) { UI.AddPPT2PdfFiles(); }

        private void ConvertPPT2PDF_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.PPT2PdfFileList,
                            this.ConvertPPT2PDFStatusText,
                            ConvertFileType.PPT2PDF,
                            ".pdf",
                            ppt2pdf_save_source.Checked,
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void ListviewSortHandler(object sender, ColumnClickEventArgs e)
        {
            bool IsAscing = true;
            //自定义比较器进行排序

            if (e.Column == 0)
            {
                System.Windows.Forms.ListView TempListview = sender as System.Windows.Forms.ListView;

                if(IsAscing)
                {
                    TempListview.ListViewItemSorter = new ListViewItemComparer_DSC();
                    IsAscing = false;
                }
                else
                {
                    TempListview.ListViewItemSorter = new ListViewItemComparer_ASC();
                    IsAscing = true;
                }
            }
        }

        private void ConvertPDF2PPT_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.PDF2PPTFileList,
                            this.Pdf2PPTStatusText,
                            ConvertFileType.PDF2PPT,
                            ".pptx",
                            pdf2ppt_save_source.Checked,
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectConvertPDF2PPT_Click(object sender, EventArgs e) { UI.AddPdf2PPTFiles(); }

        private void SelectConvertComplaincePdf_Click(object sender, EventArgs e) { UI.AddConvertComplicePdfFiles(); }

        private void ConvertPdfCompliance_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.ConvertPdfComplianceFileList,
                            this.ConvertPdfComplianceStatusText,
                            ConvertFileType.ConvertPdfCompliance,
                            ".pdf",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectConvertImage2PDF_Click(object sender, EventArgs e) { UI.AddConvertImage2PDFFiles(); }

        private void ConvertImage2PDF_Click(object sender, EventArgs e)
        {
            Task<List<string>> ImageFiles = UI.GetListViewFirstItemContentAsync(this.ConvertImage2PDFFileList);
            if (ImageFiles.Result.Count > 0)
            {
                if (ExportSignlePDF.Checked)
                {
                    IEnumerable<string> SavePaths = Util.ShowSelectDialog(true, true, string.Empty, string.Empty);
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                string SavePath = SavePaths.FirstOrDefault();
                                if (SavePath != string.Empty ||
                                    string.Compare(SavePath, default, StringComparison.Ordinal) != 0)
                                {
                                    if (SavePath != null)
                                    {
                                        (sender as System.Windows.Forms.Button).Enabled = false;
                                        int ImageCount = 0;
                                        ImageFiles.Result
                                            .ForEach(
                                                TempFile =>
                                                {
                                                    ImageCount++;
                                                    this.COnvertImage2PDFStatusText.Text = $"正在转换第{ImageCount}个文件";
                                                    string SavePdfName = $"{Path.Combine(SavePath, $"{Path.GetFileNameWithoutExtension(TempFile)}.pdf")}";
                                                    Util.ConvertImage2PDF(
                                                        TempFile,
                                                        SavePdfName,
                                                        false,
                                                        ImageFiles.Result,
                                                        this.Image2PDFCombox.SelectedIndex,
                                                        this.Image2PDFOptiomize.Checked,
                                                        this.Image2PDFCOmpressImage.Checked,
                                                        double.Parse(this.ImagePdfPageSIze_UP.Text),
                                                        double.Parse(this.ImagePdfPageSIze_LEFT.Text),
                                                        double.Parse(this.ImagePdfPageSIze_BOTTOM.Text),
                                                        double.Parse(this.ImagePdfPageSIze_RIGHT.Text));

                                                    this.COnvertImage2PDFStatusText.Text = $"转换完毕";
                                                });
                                        this.BeginInvoke(
                                            new Action(
                                                () =>
                                                {
                                                    DialogResult OpenResult = MessageBox.Show(
                                                        "转换完毕，是否需要打开文件所在目录？",
                                                        "提示",
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Asterisk);
                                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                                    if (OpenResult == DialogResult.Yes)
                                                    {
                                                        Process.Start(SavePath);
                                                    }
                                                }));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show(
                                                ex.Message,
                                                "错误",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                        }));
                            }
                        });
                }
                else
                {
                    string MeregePdfFileName = Util.ShowSaveFileDialog("PDF文档|.pdf", "设置文件名称");
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                if (MeregePdfFileName != string.Empty)
                                {
                                    (sender as System.Windows.Forms.Button).Enabled = false;
                                    this.COnvertImage2PDFStatusText.Text = "正在合并中...";
                                    Util.ConvertImage2PDF(
                                        string.Empty,
                                        MeregePdfFileName,
                                        true,
                                        ImageFiles.Result,
                                        this.Image2PDFCombox.SelectedIndex,
                                        this.Image2PDFOptiomize.Checked,
                                        this.Image2PDFCOmpressImage.Checked,
                                        double.Parse(this.ImagePdfPageSIze_UP.Text),
                                        double.Parse(this.ImagePdfPageSIze_LEFT.Text),
                                        double.Parse(this.ImagePdfPageSIze_BOTTOM.Text),
                                        double.Parse(this.ImagePdfPageSIze_RIGHT.Text));
                                    this.COnvertImage2PDFStatusText.Text = "合并完毕";
                                    this.BeginInvoke(
                                        new Action(
                                            () =>
                                            {
                                                (sender as System.Windows.Forms.Button).Enabled = true;
                                                DialogResult OpenResult = MessageBox.Show(
                                                    "合并完毕，是否需要打开合并文件所在目录？",
                                                    "提示",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Asterisk);
                                                if (OpenResult == DialogResult.Yes)
                                                {
                                                    Process.Start(Path.GetDirectoryName(MeregePdfFileName));
                                                }
                                            }));
                                }
                            }
                            catch (Exception ex)
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show(
                                                ex.Message,
                                                "错误",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                        }));
                            }
                        });
                }
            }
        }

        private void SelectCompressPDFFiles_Click(object sender, EventArgs e) { UI.AddCompressPDFFiles(); }

        private void CompressPDF_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.CompressPdfFileList,
                            this.CompressPDFStatusText,
                            ConvertFileType.CompressPDF,
                            ".pdf",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectCompressImageFile_Click(object sender, EventArgs e) { UI.AddCompressImageFiles(); }

        private void CompressImage_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.CompressImageFileList,
                            this.CompressImageStatusText,
                            ConvertFileType.CompressImage,
                            this.CompressImage_OutPutImageFormat.SelectedValue.ToString(),
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
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
            PlayCXKMusic.Enabled = false;
            PlayCXKMusic.Text = "正在唱跳RAP中...";
            Util.PlayMp3WithStream(
                "zdsny",
                status =>
                {
                    PlayCXKMusic.Enabled = true;
                    PlayCXKMusic.Text = "点一下";
                });
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
            GlobalHook.KeyUp += HookKeyboard_KeyUp;
            KeyBoardLockStatus.Text = $"已锁定";
            KeyBoardLockStatus.ForeColor = Color.Red;
            LockKeyBoard.Enabled = false;//锁定按钮不可用
        }

        private void UnlockKeyBoard_Click(object sender, EventArgs e)
        {
            GlobalHook.KeyDown -= HookKeyboard_KeyDown;
            GlobalHook.KeyUp -= HookKeyboard_KeyUp;

            KeyBoardLockStatus.Text = "已解锁";
            KeyBoardLockStatus.ForeColor = Color.Green;
            LockKeyBoard.Enabled = true;
        }

        private void HookKeyboard_KeyDown(object sender, KeyEventArgs e) { e.Handled = true; }

        private void HookKeyboard_KeyUp(object sender, KeyEventArgs e) { e.Handled = true; }
        #endregion

        private void RemoveShort_Click(object sender, EventArgs e)
        {
            try
            {
                if (Util.IsAdministrator())
                {
                    Util.RemoveShort();
                    ShortStatus.Text = "已移除";
                    ShortStatus.ForeColor = Color.Red;
                    if (AutoRestartExplorer.Checked)
                    {
                        Process.GetProcesses()
                            .ToList()
                            .ForEach(
                                p =>
                                {
                                    if (string.Compare(p.ProcessName, "explorer", StringComparison.Ordinal) == 0)
                                    {
                                        p.Kill();
                                    }
                                });
                        Process.Start("explorer.exe");
                    }
                }
                else
                {
                    MessageBox.Show("请使用管理员身份证重新运行软件后执行本操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreShort_Click(object sender, EventArgs e)
        {
            try
            {
                if (Util.IsAdministrator())
                {
                    Util.RestoreShort();
                    ShortStatus.Text = "已恢复";
                    ShortStatus.ForeColor = Color.Green;
                    if (AutoRestartExplorer.Checked)
                    {
                        Process.GetProcesses()
                            .ToList()
                            .ForEach(
                                p =>
                                {
                                    if (string.Compare(p.ProcessName, "explorer", StringComparison.Ordinal) == 0)
                                    {
                                        p.Kill();
                                    }
                                });
                        Process.Start("explorer.exe");
                    }
                }
                else
                {
                    MessageBox.Show("请使用管理员身份证重新运行软件后执行本操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectHtml2PdfFiles_Click(object sender, EventArgs e) { UI.AddWord2EPUBFiles(); }

        private void ConvertHTML2PDF_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.Word2EPUBFileList,
                            this.ConvertWord2EpubStatusText,
                            ConvertFileType.Word2EPUB,
                            ".epub",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectEpub2WordFiles_Click(object sender, EventArgs e) { UI.AddEPUB2WORDFiles(); }

        private void ConvertEpub2Word_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.EPUB2WORDFileList,
                            this.ConvertEpub2WordStatusText,
                            ConvertFileType.EPUB2Word,
                            ".docx",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (NullReferenceException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as System.Windows.Forms.Button).Enabled = true;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as System.Windows.Forms.Button).Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectEPUB2PDFFiles_Click(object sender, EventArgs e) { UI.AddEPUB2PDFFiles(); }

        private void ConvertEPUB2PDF_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.EPUB2PDFFileList,
                            this.ConvertEPUB2PDFStatusText,
                            ConvertFileType.EPUB2Pdf,
                            ".pdf",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (NullReferenceException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as System.Windows.Forms.Button).Enabled = true;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as System.Windows.Forms.Button).Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void 打开源文件目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListViewRightMenu.SourceControl.GetType() == typeof(System.Windows.Forms.ListView))
            {
                System.Windows.Forms.ListView TempListView = ListViewRightMenu.SourceControl as System.Windows.Forms.ListView;

                if (File.Exists(TempListView.FocusedItem.Text))
                {
                    Process.Start(Path.GetDirectoryName(TempListView.FocusedItem.Text));
                }
            }
        }

        private void 打开源文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListViewRightMenu.SourceControl.GetType() == typeof(System.Windows.Forms.ListView))
            {
                System.Windows.Forms.ListView TempListView = ListViewRightMenu.SourceControl as System.Windows.Forms.ListView;

                if (File.Exists(TempListView.FocusedItem.Text))
                {
                    Process.Start(TempListView.FocusedItem.Text);
                }
            }
        }

        private void 打开转换后目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListViewRightMenu.SourceControl.GetType() == typeof(System.Windows.Forms.ListView))
            {
                System.Windows.Forms.ListView TempListView = ListViewRightMenu.SourceControl as System.Windows.Forms.ListView;

                if (TempListView.FocusedItem.SubItems.Count >= 4)
                {
                    if (File.Exists(TempListView.FocusedItem.SubItems[2].Text))
                    {
                        Process.Start(Path.GetDirectoryName(TempListView.FocusedItem.SubItems[2].Text));
                    }
                }
            }
        }

        private void 打开转换后文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListViewRightMenu.SourceControl.GetType() == typeof(System.Windows.Forms.ListView))
            {
                System.Windows.Forms.ListView TempListView = ListViewRightMenu.SourceControl as System.Windows.Forms.ListView;

                if (TempListView.FocusedItem.SubItems.Count >= 4)
                {
                    if (File.Exists(TempListView.FocusedItem.SubItems[2].Text))
                    {
                        Process.Start(TempListView.FocusedItem.SubItems[2].Text);
                    }
                }
            }
        }

        private void 获取PDF文件版本信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ListViewRightMenu.SourceControl.GetType() == typeof(System.Windows.Forms.ListView))
            {
                System.Windows.Forms.ListView TempListView = ListViewRightMenu.SourceControl as System.Windows.Forms.ListView;

                Task.Run(
                    () =>
                    {
                        string FileName = TempListView.FocusedItem.Text;

                        string Information = string.Empty;

                        if (File.Exists(FileName))
                        {
                            Information = Path.GetExtension(FileName) switch
                            {
                                ".pdf" => Util.GetPdfVersion(FileName),
                                ".doc" => Util.GetWordInformation(FileName),
                                ".docx" => Util.GetWordInformation(FileName),
                                ".xls" => Util.GetExcelInformation(FileName),
                                ".xlsx" => Util.GetExcelInformation(FileName),
                                ".ppt" => Util.GetPPtInformation(FileName),
                                ".pptx" => Util.GetPPtInformation(FileName),
                                _ => "暂不支持获取该文件信息",
                            };
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(
                                            Information,
                                            "信息",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Asterisk);
                                    }));
                        }
                        else
                        {
                            MessageBox.Show("所选文件不存在", "信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    });
            }
        }

        private void SelectHtml2PDFFiles_Click_1(object sender, EventArgs e) { UI.AddHtml2PdfFiles(); }

        private void ConvertHtml2Pdf_Click_1(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        if (this.html2pdf_textbox_chromepath.Text != string.Empty)
                        {
                            UI.ConvertStart(
                                this.HTML2PDFFileList,
                                this.ConvertHtml2PdfStatusText,
                                ConvertFileType.Html2PDF,
                                ".pdf",
                                CallButton: sender as System.Windows.Forms.Button);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SetWord2EpubCoverImage_Click(object sender, EventArgs e)
        {
            string SelectImageFileName = Util.ShowSelectDialog(false, false, "图片文件", "*.jpg;*.jpeg;*.png;*.bmp")
                .FirstOrDefault();

            if (string.Compare(SelectImageFileName, default, StringComparison.Ordinal) != 0 &&
                !string.IsNullOrEmpty(SelectImageFileName))
            {
                this.Word2EpubConverImage.Text = SelectImageFileName;
            }
        }

        private void SelectSplitWordFiles_Click(object sender, EventArgs e) { UI.AddSplitWordFiles(); }

        private void StartSplitWord_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.SplitWordFileList,
                            this.StartSplitWordStatusText,
                            ConvertFileType.SplitWord,
                            ".docx",
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(
                "https://commondatastorage.googleapis.com/chromium-browser-snapshots/index.html?prefix=Win_x64/999988/");
        }

        private void Html2pdf_button_set_chrome_path_Click(object sender, EventArgs e)
        {
            IEnumerable<string> chrome_path = Util.ShowSelectDialog(false, false, "chrome浏览器", "*.exe");

            string chrome_path_string = chrome_path.FirstOrDefault();

            if (chrome_path_string != string.Empty &&
                string.Compare(chrome_path_string, default, StringComparison.Ordinal) != 0 &&
                chrome_path_string != null)
            {
                this.html2pdf_textbox_chromepath.Text = chrome_path_string;
            }
        }

        private void Html2pdf_textbox_chromepath_MouseHover(object sender, EventArgs e)
        { MainToolsTip.SetToolTip(this.html2pdf_textbox_chromepath, this.html2pdf_textbox_chromepath.Text); }

        private void SelectMss3_Click(object sender, EventArgs e)
        {
            string Mss3Path = Util.ShowSelectDialog(
                false,
                false,
                "MSS3文件",
                "*.mss3",
                this.vfsoft_mssdata_path_textbox.Text)
                .FirstOrDefault();

            Task.Run(
                new Action(
                    () =>
                    {
                        try
                        {
                            if (Mss3Path != string.Empty &&
                                string.Compare(Mss3Path, default, StringComparison.Ordinal) != 0 &&
                                Mss3Path != null)
                            {
                                this.Mss3Path.Text = Mss3Path;

                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            DataTable TempVoteDataTable = Util.GetMss3VoteInfo(Mss3Path);

                                            if (TempVoteDataTable != null && TempVoteDataTable.Rows.Count > 0)
                                            {
                                                this.votedataview.DataSource = TempVoteDataTable;
                                            }
                                        }));
                            }
                        }
                        catch (Exception ex)
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }));
                        }
                    }));
        }

        private void StartInsert_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        (sender as System.Windows.Forms.Button).Enabled = false;
                        Util.InsertVotePicContent(
                            this.Mss3Path.Text,
                            VotePicture.ImageLocation,
                            this.votestatustext,
                            this.voteid.Text,
                            this.votename.Text,
                            this.voteindex.Text,
                            this.votepageIndexCombox.SelectedValue.ToString(),
                            this.votepage_type_combox.SelectedValue.ToString());
                        (sender as System.Windows.Forms.Button).Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectVotePic_Click(object sender, EventArgs e)
        {
            string VoteImagePath = Util.ShowSelectDialog(false, false, "图片文件", "*.jpg;*.bmp;*.png;*.tif;*.tiff")
                .FirstOrDefault();
            if (VoteImagePath != string.Empty &&
                string.Compare(VoteImagePath, default, StringComparison.Ordinal) != 0 &&
                VoteImagePath != null)
            {
                VotePicture.ImageLocation = VoteImagePath;
            }
        }

        private void WordReplaceFontSeeting_Click(object sender, EventArgs e)
        {
            FontSelectDialog.ShowEffects = false;
            FontSelectDialog.AllowScriptChange = false;
            FontSelectDialog.AllowSimulations = false;
            FontSelectDialog.AllowVerticalFonts = false;
            DialogResult FontSelectDialogResult = FontSelectDialog.ShowDialog();

            if (FontSelectDialogResult == DialogResult.OK)
            {
                ReplaceWordFontSetting.FontName = FontSelectDialog.Font.FontFamily.GetName(1033);
                ReplaceWordFontSetting.FontSize = FontSelectDialog.Font.Size;
            }
        }

        private void wORD转PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("WORD转PDF"); }

        private void wORD转EXCELToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("WORD转EXCEL"); }

        private void wORDToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("WORD转EPUB"); }

        private void wORD内容批量替换ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("WORD内容批量替换"); }

        private void eXCEL转PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("EXCEL转PDF"); }

        private void eXCEL转WORDToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("EXCEL转WORD"); }

        private void eXCEL内容批量替换ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("EXCEL内容批量替换"); }

        private void pPT转PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("PPT转PDF"); }

        private void pDF转EXCELToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("PDF转WORD"); }

        private void pDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("PDF转EXCEL"); }

        private void pDF转PPTToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("PDF转PPT"); }

        private void ePUB转WORDToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("EPUB转WORD"); }

        private void ePUB转PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("EPUB转PDF"); }

        private void hTML转PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("HTML转PDF"); }

        private void 拆分WORDToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("拆分WORD"); }

        private void 拆分PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("拆分PDF"); }

        private void 合并WORDToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("合并WORD"); }

        private void 合并PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("合并PDF"); }

        private void 压缩PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("压缩PDF"); }

        private void 压缩图片ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("压缩图片"); }

        private void wORDEXCELPDF转图片ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("WORD/PDF/EXCEL转图片"); }

        private void 图片转PDFToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("图片转PDF"); }

        private void 姓名横转竖列ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("姓名横转竖列"); }

        private void 特殊二维码校验ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("特殊二维码校验"); }

        private void twain扫描仪调试ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("Twain扫描仪调试"); }

        private void mSS3模板修改ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("MSS3模板修改"); }

        private void 其他功能ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("其他功能"); }

        private void 系统工具ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("系统工具"); }

        private void 关于软件ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("关于软件"); }

        private void 保存设置并重启软件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
            {
                Configuration TempConfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                TempConfig.AppSettings.Settings["StartPageIndex"].Value = menu_start_page_index_combox.Text;
                TempConfig.AppSettings.Settings["TabPageHeighLight"].Value = menu_tabpage_heigh_light_combox.SelectedIndex ==
                        0
                    ? "on"
                    : "off";
                TempConfig.AppSettings.Settings["ShowTabPageIco"].Value = menu_tabpage_icon_combox.SelectedIndex == 0
                    ? "on"
                    : "off";
                TempConfig.AppSettings.Settings["ShowOpenTargetDialog"].Value = menu_finshined_messbox_combox.SelectedIndex ==
                        0
                    ? "on"
                    : "off";
                TempConfig.AppSettings.Settings["ListViewLineHeight"].Value = menu_listview_line_heigh_combox.Text;
                TempConfig.AppSettings.Settings["ListViewShowGridLines"].Value = menu_listview_gridline_combox.SelectedIndex ==
                        0
                    ? "on"
                    : "off";
                TempConfig.AppSettings.Settings["WidgetAnchorMode"].Value = menu_widget_anchor_combox.SelectedIndex == 0
                    ? "on"
                    : "off";
                TempConfig.AppSettings.Settings["ChromeDriverPath"].Value = menu_chrome_path.Text;
                TempConfig.AppSettings.Settings["FormWidth"].Value = bodyform_width.Text;
                TempConfig.AppSettings.Settings["FormHeight"].Value = bodyform_height.Text;
                TempConfig.AppSettings.Settings["Vfsoft_ExamDataPath"].Value = vfsoft_examdata_path_textbox.Text;
                TempConfig.AppSettings.Settings["Vfsoft_MssDataPath"].Value = vfsoft_mssdata_path_textbox.Text;

                TempConfig.Save(ConfigurationSaveMode.Modified);

                DialogResult ExitResult = MessageBox.Show(
                    "配置保存完毕，是否需要重启软件？",
                    "提示",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Asterisk);
                if (ExitResult == DialogResult.OK)
                {
                    string AppExcuteFile = Application.ExecutablePath;
                    Process.Start(AppExcuteFile);

                    Environment.Exit(0);
                }
            }
            else
            {
                string ProgramFileName = Process.GetCurrentProcess().ProcessName;

                string ApplicationConfigFile = $"{ProgramFileName}.exe.config";

                string AppConfigContent = Util.GetEmbedResourceObj("ConfigTemplate").ToString();

                File.WriteAllText(ApplicationConfigFile, AppConfigContent);

                MessageBox.Show("没有找到配置文件,已经初始化配置文件到软件目录", "无法保存配置", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void 查看标签索引对照表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder TempIndexTable = new StringBuilder();
            foreach (TabPage TempPage in this.MainTabPage.TabPages)
            {
                TempIndexTable.AppendLine($"{TempPage.Text}：{MainTabPage.TabPages.IndexOf(TempPage)}");
            }
            MessageBox.Show(TempIndexTable.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        private void UseLibList_Click(object sender, EventArgs e)
        {
            Assembly TempAssmbly = Assembly.GetExecutingAssembly();
            AssemblyName[] LibNames = TempAssmbly.GetReferencedAssemblies();
            StringBuilder LibNameShow = new StringBuilder();

            foreach (AssemblyName LibName in LibNames)
            {
                if (string.Compare(LibName.Name, "Aspose.Hook", StringComparison.Ordinal) != 0)
                {
                    LibNameShow.AppendLine($"· {LibName.Name}\tV{LibName.Version}");
                }
            }
            MessageBox.Show(LibNameShow.ToString(), "使用库列表");
        }

        private void GenerateBarCode_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        Util.PlayMp3WithStream(
                            "amagi",
                            status =>
                            {
                            });
                        if (barcodeTextContentBox.Text != string.Empty)
                        {
                            MemoryStream tempstream = Util.GenerateBarCode(
                                barcodeTypeCombox.SelectedIndex,
                                (Aspose.BarCode.Generation.AutoSizeMode)Enum.Parse(
                                    typeof(Aspose.BarCode.Generation.AutoSizeMode),
                                    barcodeAutoSizeCombox.SelectedValue.ToString()),
                                float.Parse(barcodeDmiBox.Text),
                                float.Parse(barcodeRotateBox.Text),
                                float.Parse(barcodeImageWidth.Text),
                                float.Parse(barcodeImageHeight.Text),
                                float.Parse(barcodeDpiCombox.Text),
                                barcodeTextContentBox.Text,
                                (Aspose.BarCode.Generation.CodeLocation)Enum.Parse(
                                    typeof(Aspose.BarCode.Generation.CodeLocation),
                                    barcodeTextLocationCombox.SelectedValue.ToString()),
                                (Aspose.BarCode.Generation.TextAlignment)Enum.Parse(
                                    typeof(Aspose.BarCode.Generation.TextAlignment),
                                    barcodeTextAlignCombox.SelectedValue.ToString()),
                                float.Parse(barcodeTextContentSpaceCombox.Text),
                                int.Parse(barcodePaddingBox.Text),
                                float.Parse(barcodeTextFontSizeBox.Text),
                                "barcode_preview",
                                string.Empty,
                                barcodeCaptionBox.Text,
                                barcodeCaptionLocationCombox.SelectedIndex,
                                float.Parse(barcodeCaptionFontSizeBox.Text),
                                barcodeScanTypeCombox.SelectedIndex);
                            barcodePreviewBox.Image = new Bitmap(tempstream);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void barcodeTemplateUpload_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        IEnumerable<string> barcodeTemplateName = Util.ShowSelectDialog(
                            false,
                            false,
                            "Excel文件",
                            "*.xls;*.xlsx");
                        string barcodeTemplateFile = barcodeTemplateName.FirstOrDefault();
                        if (!string.IsNullOrEmpty(barcodeTemplateFile))
                        {
                            this.Invoke(
                                (Action)(() =>
                                {
                                    this.barcodeMutilList.DataSource = Util.ReadBarCodeTemplate(barcodeTemplateFile);
                                }));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void barcodeMutilGenerate_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        if (this.barcodeMutilList.DataSource != null && this.barcodeMutilList.Rows.Count > 1)
                        {
                            IEnumerable<string> TempMutilSavePath = Util.ShowSelectDialog(
                                false,
                                true,
                                string.Empty,
                                string.Empty);
                            string MutilSavePath = TempMutilSavePath.FirstOrDefault();

                            if (MutilSavePath != string.Empty &&
                                string.Compare(MutilSavePath, default, StringComparison.Ordinal) != 0)
                            {
                                (sender as System.Windows.Forms.Button).Enabled = false;
                                foreach (DataGridViewRow TempRow in barcodeMutilList.Rows)
                                {
                                    if (!TempRow.IsNewRow)
                                    {
                                        string barcodeParam_FileName = TempRow.Cells[0].Value.ToString();
                                        string barcodeParam_Caption = TempRow.Cells[1].Value.ToString();
                                        string barcodeParam_Content = TempRow.Cells[2].Value.ToString();

                                        string SaveFileName = Path.Combine(
                                            MutilSavePath,
                                            $"{barcodeParam_FileName}.bmp");
                                        Util.GenerateBarCode(
                                            barcodeTypeCombox.SelectedIndex,
                                            (Aspose.BarCode.Generation.AutoSizeMode)Enum.Parse(
                                                typeof(Aspose.BarCode.Generation.AutoSizeMode),
                                                barcodeAutoSizeCombox.SelectedValue.ToString()),
                                            float.Parse(barcodeDmiBox.Text),
                                            float.Parse(barcodeRotateBox.Text),
                                            float.Parse(barcodeImageHeight.Text),
                                            float.Parse(barcodeImageHeight.Text),
                                            float.Parse(barcodeDpiCombox.Text),
                                            barcodeParam_Content,
                                            (Aspose.BarCode.Generation.CodeLocation)Enum.Parse(
                                                typeof(Aspose.BarCode.Generation.CodeLocation),
                                                barcodeTextLocationCombox.SelectedValue.ToString()),
                                            (Aspose.BarCode.Generation.TextAlignment)Enum.Parse(
                                                typeof(Aspose.BarCode.Generation.TextAlignment),
                                                barcodeTextAlignCombox.SelectedValue.ToString()),
                                            float.Parse(barcodeTextContentSpaceCombox.Text),
                                            int.Parse(barcodePaddingBox.Text),
                                            float.Parse(barcodeTextFontSizeBox.Text),
                                            "barcode_mutil_save",
                                            SaveFileName,
                                            barcodeParam_Caption,
                                            barcodeCaptionLocationCombox.SelectedIndex,
                                            float.Parse(barcodeCaptionFontSizeBox.Text),
                                            barcodeScanTypeCombox.SelectedIndex);
                                    }
                                }

                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                            if (MessageBox.Show(
                                                    "生成完毕，是否打开生成目录？",
                                                    "提示",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Asterisk) ==
                                                DialogResult.Yes)
                                            {
                                                Process.Start(MutilSavePath);
                                            }
                                        }));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void ExportBarCodeTempLate_Click(object sender, EventArgs e)
        {
            object BarcodeTemplateUpload = Util.GetEmbedResourceObj("BarCodeTemplate");

            string SaveTemplateFile = Util.ShowSaveFileDialog("Excel文件|*.xlsx");

            if (SaveTemplateFile != string.Empty)
            {
                File.WriteAllBytes(SaveTemplateFile, (byte[])BarcodeTemplateUpload);

                if (File.Exists(SaveTemplateFile))
                {
                    MessageBox.Show("导出完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        private void SaveBarCodePreview_Click(object sender, EventArgs e)
        {
            try
            {
                if (barcodePreviewBox.Image != null)
                {
                    string SaveFileName = Util.ShowSaveFileDialog("PNG图片|*.png");
                    Bitmap TempBitMap = new Bitmap(barcodePreviewBox.Image);

                    if (SaveFileName != string.Empty)
                    {
                        TempBitMap.Save(SaveFileName);

                        if (MessageBox.Show("生成完毕，是否打开生成目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) ==
                            DialogResult.Yes)
                        {
                            Process.Start(Path.GetDirectoryName(SaveFileName));
                        }
                    }
                }
            }
            catch (ExternalException)
            {
                MessageBox.Show("请不要覆盖保存已被加载到软件中的图片文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 简易二维码生成ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("简易二维码生成"); }

        private void barcodeCopyImage_Click(object sender, EventArgs e)
        {
            if (barcodePreviewBox.Image != null)
            {
                Clipboard.SetImage(barcodePreviewBox.Image);
                MessageBox.Show("已复制图像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void city_level_1_combox_TextChanged(object sender, EventArgs e)
        {
            city_level_2_combox.Items.Clear();
            foreach (JToken City_2_Name in City_Json[city_level_1_combox.SelectedIndex]["children"])
            {
                city_level_2_combox.Items.Add(City_2_Name["name"]);//添加对应的二级城市
            }
            city_level_2_combox.SelectedIndex = 0;
        }

        private void city_level_2_combox_TextChanged(object sender, EventArgs e)
        {
            city_level_3_combox.Items.Clear();
            foreach (JToken City_3_Name in City_Json[city_level_1_combox.SelectedIndex]["children"][
                city_level_2_combox.SelectedIndex]["children"])
            {
                city_level_3_combox.Items.Add(City_3_Name["name"]);//添加对应的三级城市
            }
            city_level_3_combox.SelectedIndex = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Task.Run(
                    () =>
                    {
                        //获取对应的行政区域代码
                        JToken Pac_Code = City_Json[city_level_1_combox.SelectedIndex]["children"][
                            city_level_2_combox.SelectedIndex]["children"][city_level_3_combox.SelectedIndex]["code"];
                        List<string> gen_card_ids = new List<string>();
                        List<string> gen_card_names = new List<string>();
                        Random random = new Random();
                        for (int i = 0; i < gen_card_number.Value; i++)
                        {
                            if (gen_card_id_name_check_box.Checked)
                            {
                                string gen_card_name = Util.GenRandomName(random, random);
                                gen_card_names.Add(gen_card_name);
                                gen_card_id_name_text.Text = $"[已勾选，已生成{i + 1}个姓名]";
                            }
                            string gen_card_id = Util.GenChinaCard(
                                Pac_Code.ToString(),
                                Util.GenRandomBirthday(random, ony_gen_18_year.Checked),
                                Util.GenRandomIndex(random));
                            gen_card_ids.Add(gen_card_id);
                            gen_card_num_text.Text = $"正在生成第{i + 1}个号码";
                        }
                        gen_card_id_box.Lines = gen_card_ids.ToArray();
                        gen_card_id_name_box.Lines = gen_card_names.ToArray();
                    });
            }
            catch (Exception ex)
            {
                this.BeginInvoke(
                    new Action(
                        () =>
                        {
                            MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
            }
        }

        private void gen_card_id_box_TextChanged(object sender, EventArgs e)
        { gen_card_num_text.Text = $"共生成{gen_card_id_box.Lines.Length.ToString()}个号码"; }

        private void copy_gen_card_cliboard_Click(object sender, EventArgs e)
        {
            if (gen_card_id_box.Text != string.Empty)
            {
                Clipboard.SetText(gen_card_id_box.Text.Replace("\n", Environment.NewLine));
                MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void 随机身份证号码生成ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("身份证号码校验"); }

        private void ver_card_id_text_box_TextChanged(object sender, EventArgs e)
        {
            var vaildate_tuple = Util.ValidateChinaCard(ver_card_id_text_box.Text.Trim());

            ver_card_id_birthday.Text = vaildate_tuple.Item1;
            ver_card_id_sex.Text = vaildate_tuple.Item2;
            ver_card_id_index.Text = vaildate_tuple.Item3;
            if (vaildate_tuple.Item4)
            {
                ver_card_id_sign_tips.ForeColor = Color.ForestGreen;
                ver_card_id_sign_tips.Text = "正确";
            }
            else
            {
                ver_card_id_sign_tips.ForeColor = Color.Tomato;
                ver_card_id_sign_tips.Text = $"校验码不正确，应为 {vaildate_tuple.Item5}";
            }

            if (string.Compare(ver_card_id_text_box.Text, "sfz", StringComparison.Ordinal) == 0)
            {
                gen_card_id_group.Enabled = true;
                gen_card_result_group.Enabled = true;
                Util.PlayMp3WithStream(
                    "amagi",
                    status =>
                    {
                    });
            }

            if (string.Compare(ver_card_id_birthday.Text, "日期不正确", StringComparison.Ordinal) == 0)
            {
                ver_card_id_birthday.ForeColor = Color.Tomato;
            }
            else
            {
                ver_card_id_birthday.ForeColor = Color.ForestGreen;
            }
        }

        private void gen_card_id_name_copy_Click(object sender, EventArgs e)
        {
            if (gen_card_id_name_box.Text != string.Empty)
            {
                Clipboard.SetText(gen_card_id_name_box.Text.Replace("\n", Environment.NewLine));
                MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void gen_card_id_name_check_box_CheckedChanged(object sender, EventArgs e)
        {
            if (gen_card_id_name_check_box.Checked)
            {
                gen_card_id_name_text.ForeColor = Color.Green;
                gen_card_id_name_text.Text = $"[已勾选]";
            }
            else
            {
                gen_card_id_name_text.ForeColor = Color.Red;
                gen_card_id_name_text.Text = "[未勾选]";
            }
        }

        private void barcodeSelectRegionPicture_Click(object sender, EventArgs e)
        {
            try
            {
                Task.Run(
                    () =>
                    {
                        string BarcodeImageFile = Util.ShowSelectDialog(
                            false,
                            false,
                            "支持的图片",
                            "*.jpg;*.jpeg;*.png;*.bmp")
                            .FirstOrDefault();
                        if (string.Compare(BarcodeImageFile, default, StringComparison.Ordinal) != 0 &&
                            BarcodeImageFile != string.Empty)
                        {
                            barcodeRegionPicture.Image = System.Drawing.Image.FromFile(BarcodeImageFile);
                        }
                    });
            }
            catch (Exception ex)
            {
                this.BeginInvoke(
                    new Action(
                        () =>
                        {
                            MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
            }
        }

        private void barcodeReadStart_Click(object sender, EventArgs e)
        {
            try
            {
                Task.Run(
                    () =>
                    {
                        if (barcodeRegionPicture.Image != null)
                        {
                            (sender as System.Windows.Forms.Button).Enabled = false;
                            barcodeReaderResultbox.Text = Util.ReadBarCodeContent((Bitmap)barcodeRegionPicture.Image);
                            (sender as System.Windows.Forms.Button).Enabled = true;
                        }
                    });
            }
            catch (Exception ex)
            {
                this.BeginInvoke(
                    new Action(
                        () =>
                        {
                            MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            (sender as System.Windows.Forms.Button).Enabled = true;
                        }));
            }
        }

        private void CopyBarcodeReadResult_Click(object sender, EventArgs e)
        {
            if (barcodeReaderResultbox.Text != string.Empty)
            {
                Clipboard.SetText(barcodeReaderResultbox.Text.Replace("\n", Environment.NewLine));
                MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void webpPictureSelect_Click(object sender, EventArgs e) { UI.AddWeBpFiles(); }

        private void webpPictureStartConvert_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        UI.ConvertStart(
                            this.WebpFileList,
                            this.webpconvert_statusText,
                            ConvertFileType.WeBp2Image,
                            this.webp2image_combox.SelectedValue.ToString(),
                            CallButton: sender as System.Windows.Forms.Button);
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                }));
                    }
                });
        }

        private void wEBP图片格式转换ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("WEBP图片格式转换"); }

        private void ExportNameTxt_Click(object sender, EventArgs e)
        {
            if (TargetTextBox.Text != string.Empty)
            {
                string TempExportTxtFile = Util.ShowSaveFileDialog("文本文件|*.txt");

                Task.Run(
                    () =>
                    {
                        try
                        {
                            if (TempExportTxtFile != string.Empty && TempExportTxtFile != null)
                            {
                                (sender as System.Windows.Forms.Button).Enabled = false;
                                File.WriteAllText(TempExportTxtFile, TargetTextBox.Text);
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show("导出完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                        }));
                            }
                        }
                        catch (Exception ex)
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        (sender as System.Windows.Forms.Button).Enabled = true;
                                    }));
                        }
                    });
            }
        }

        private void Gen_Copied_File_Click(object sender, EventArgs e)
        {
            string TempFileName = Util.ShowSelectDialog(false, false, "所有文件", "*.*").FirstOrDefault();

            if (!string.IsNullOrEmpty(TempFileName) &&
                string.Compare(TempFileName, default, StringComparison.Ordinal) != 0)
            {
                Gen_Copied_SourceFileBox.Text = TempFileName;
            }
        }

        private void Gen_Copied_Path_Click(object sender, EventArgs e)
        {
            string TempPath = Util.ShowSelectDialog(false, true, "所有文件", "*.*").FirstOrDefault();

            if (!string.IsNullOrEmpty(TempPath) && string.Compare(TempPath, default, StringComparison.Ordinal) != 0)
            {
                Gen_Copied_TargetPath.Text = TempPath;
            }
        }

        private void CopiedGen_Click(object sender, EventArgs e)
        {
            if (Gen_Copied_SourceFileBox.Text != string.Empty && Gen_Copied_TargetPath.Text != string.Empty)
            {
                Task.Run(
                    () =>
                    {
                        try
                        {
                            (sender as System.Windows.Forms.Button).Enabled = false;
                            Util.GenCopySameFile(
                                Gen_Copied_SourceFileBox.Text,
                                int.Parse(Gen_Copied_Number.Value.ToString()),
                                Gen_Copied_StatusText,
                                Gen_Copied_Namer.Text =
                                    Gen_Copied_Namer.Text == string.Empty ? "副本" : Gen_Copied_Namer.Text,
                                copied_number_prefix_count.Text,
                                Gen_Copied_TargetPath.Text);
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        (sender as System.Windows.Forms.Button).Enabled = true;
                                        MessageBox.Show("生成完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                    }));
                        }
                        catch (Exception ex)
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        (sender as System.Windows.Forms.Button).Enabled = true;
                                    }));
                        }
                    });
            }
        }

        private void Gen_Copied_Namer_TextChanged(object sender, EventArgs e)
        {
            if (Gen_Copied_Namer.Text != string.Empty && Gen_Copied_SourceFileBox.Text != string.Empty)
            {
                string Number = 1.ToString($"D{copied_number_prefix_count.Text}");
                CopiedNamerText.Text = $"最终生成名称为：{Path.GetFileNameWithoutExtension(Gen_Copied_SourceFileBox.Text)}- {Gen_Copied_Namer.Text}（{Number}）{Path.GetExtension(Gen_Copied_SourceFileBox.Text)}";
            }
        }

        private void 文件副本生成ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("文件副本生成"); }

        private void network_info_card_list_SelectedIndexChanged(object sender, EventArgs e) { UpdateNetworkInfo(); }

        private void UpdateNetworkInfo()
        {
            Task.Run(
                () =>
                {
                    var (ipv4_address_source, default_mask_source, default_gateway_source, dns1_address, dns2_address, mac_address, is_dhcp_enabled) = Util.GetCard_Information(
                        network_info_card_list.Text);
                    if (network_info_ipv4 != null &&
                        network_info_mask != null &&
                        network_info_gateway != null &&
                        network_info_dns1 != null &&
                        network_info_dns2 != null &&
                        network_info_mac_address != null)
                    {
                        network_info_ipv4.DataSource = ipv4_address_source.Count == 0
                            ? new List<string> { "未获取到" }
                            : ipv4_address_source;
                        network_info_mask.DataSource = default_mask_source.Count == 0
                            ? new List<string> { "未获取到" }
                            : default_mask_source;
                        network_info_gateway.DataSource = default_gateway_source.Count == 0
                            ? new List<string> { "未获取到" }
                            : default_gateway_source;
                        network_info_dns1.DataSource = new string[]
                        {
                            dns1_address == string.Empty ? "未获取到" : dns1_address
                        };
                        network_info_dns2.DataSource = new string[]
                        {
                            dns2_address == string.Empty ? "未获取到" : dns2_address
                        };
                        network_info_mac_address.DataSource = new string[]
                        {
                            mac_address == string.Empty ? "未获取到" : mac_address
                        };
                    }
                    if (is_dhcp_enabled)
                    {
                        set_interface_mode_auto.Checked = true;
                    }
                    else
                    {
                        set_interface_mode_static.Checked = true;
                    }
                });
        }

        private void UpdateSetNetWorkInfo()
        {
            Task.Run(
                () =>
                {
                    var (ipv4_address_source, default_mask_source, default_gateway_source, dns1_address, dns2_address, _, _) = Util.GetCard_Information(
                        network_info_card_list.Text);

                    set_interface_ipaddress.Text = ipv4_address_source.Count == 0 ? "0.0.0.0" : ipv4_address_source[0];
                    set_interface_mask.Text = default_mask_source.Count == 0 ? "0.0.0.0" : default_mask_source[0];
                    set_interface_gateway.Text = default_gateway_source.Count == 0
                        ? "0.0.0.0"
                        : default_gateway_source[0];
                    set_interface_dns1.Text = dns1_address == string.Empty ? "0.0.0.0" : dns1_address;
                    set_interface_dns2.Text = dns2_address == string.Empty ? "0.0.0.0" : dns2_address;
                });
        }

        private void UpdateNetworkList()
        {
            Task.Run(
                () =>
                {
                    if (network_info_card_list != null && set_interface_name_list != null)
                    {
                        this.network_info_card_list.DataSource = Util.GetNetWorkCardList(
                            network_info_show_normal.Checked);

                        this.set_interface_name_list.DataSource = this.network_info_card_list.DataSource;
                    }
                });
        }

        private void set_interface_start_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        if (Util.IsAdministrator())
                        {
                            (sender as System.Windows.Forms.Button).Enabled = false;
                            Util.SetInterfaceInformation(
                                set_interface_name_list.SelectedItem.ToString(),
                                set_interface_ipaddress.Text,
                                set_interface_mask.Text,
                                set_interface_gateway.Text,
                                set_interface_dns1.Text,
                                set_interface_dns2.Text,
                                set_interface_mode_static.Checked);
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        (sender as System.Windows.Forms.Button).Enabled = true;
                                        MessageBox.Show("修改成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                        UpdateNetworkInfo();//更新基础网络信息
                                        UpdateSetNetWorkInfo();//更新设置部分网络信息
                                    }));
                        }
                        else
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(
                                            "请使用管理员身份重新运行软件修改",
                                            "提示",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Exclamation);
                                    }));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void set_interface_name_list_SelectedIndexChanged(object sender, EventArgs e)
        { UpdateSetNetWorkInfo(); }

        private void set_interface_mode_auto_CheckedChanged(object sender, EventArgs e)
        {
            if (set_interface_mode_auto.Checked)
            {
                set_interface_ipaddress.Enabled = false;
                set_interface_mask.Enabled = false;
                set_interface_gateway.Enabled = false;
                set_interface_dns1.Enabled = false;
                set_interface_dns2.Enabled = false;
            }
            else
            {
                set_interface_ipaddress.Enabled = true;
                set_interface_mask.Enabled = true;
                set_interface_gateway.Enabled = true;
                set_interface_dns1.Enabled = true;
                set_interface_dns2.Enabled = true;
            }
        }

        private void set_interface_clear_Click(object sender, EventArgs e)
        {
            if (set_interface_mode_static.Checked)
            {
                set_interface_ipaddress.Text = "0.0.0.0";
                set_interface_mask.Text = "0.0.0.0";
                set_interface_gateway.Text = "0.0.0.0";
                set_interface_dns1.Text = "0.0.0.0";
                set_interface_dns2.Text = "0.0.0.0";
            }
        }

        private void Enable_NetworkCard_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        Util.SetInterfaceStatus(network_info_card_list.SelectedItem.ToString(), "Enable");
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show("启用成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                }));
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void Disable_NetworkCard_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        Util.SetInterfaceStatus(network_info_card_list.SelectedItem.ToString(), "Disable");
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show("禁用成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                }));
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void network_info_show_normal_CheckedChanged(object sender, EventArgs e) { UpdateNetworkList(); }

        private void update_network_info_Click(object sender, EventArgs e)
        {
            UpdateNetworkInfo();
            UpdateSetNetWorkInfo();
        }

        private void copy_network_info_mac_address_Click(object sender, EventArgs e)
        {
            if (network_info_mac_address.Text != string.Empty &&
                string.Compare(network_info_mac_address.Text, "未获取到", StringComparison.Ordinal) != 0)
            {
                Clipboard.SetText(network_info_mac_address.Text);
                MessageBox.Show("复制成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void network_open_ncap_Click(object sender, EventArgs e) { Process.Start("ncpa.cpl"); }

        private void 网卡设置ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("网卡设置"); }

        private void Gen_Copied_SourceFileBox_TextChanged(object sender, EventArgs e)
        {
            if (Gen_Copied_Namer.Text != string.Empty && Gen_Copied_SourceFileBox.Text != string.Empty)
            {
                string Number = 1.ToString($"D{copied_number_prefix_count.Text}");
                CopiedNamerText.Text = $"最终生成名称为：{Path.GetFileNameWithoutExtension(Gen_Copied_SourceFileBox.Text)}- {Gen_Copied_Namer.Text}（{Number}）{Path.GetExtension(Gen_Copied_SourceFileBox.Text)}";
            }
        }

        private void copy_network_info_ip_address_Click(object sender, EventArgs e)
        {
            if (network_info_ipv4.Text != string.Empty &&
                string.Compare(network_info_ipv4.Text, "未获取到", StringComparison.Ordinal) != 0)
            {
                Clipboard.SetText(network_info_ipv4.Text);
                MessageBox.Show("复制成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void copied_number_prefix_count_TextChanged(object sender, EventArgs e)
        {
            if (Gen_Copied_Namer.Text != string.Empty && Gen_Copied_SourceFileBox.Text != string.Empty)
            {
                string Number = 1.ToString($"D{copied_number_prefix_count.Text}");
                CopiedNamerText.Text = $"最终生成名称为：{Path.GetFileNameWithoutExtension(Gen_Copied_SourceFileBox.Text)}- {Gen_Copied_Namer.Text}（{Number}）{Path.GetExtension(Gen_Copied_SourceFileBox.Text)}";
            }
        }

        private void set_interface_export_Click(object sender, EventArgs e)
        {
            try
            {
                string JSON_IPCONFIG = @$"{{IPADDRESS:'{set_interface_ipaddress.Text}',
IPMASK:'{set_interface_mask.Text}',
IPGATEWAY:'{set_interface_gateway.Text}',
'DNS1':'{set_interface_dns1.Text}',
'DNS2':'{set_interface_dns2.Text}'}}";

                string TempSaveFileName = Util.ShowSaveFileDialog("坤Tools网卡配置(*.KUNNET)|*.KUNNET");

                if (TempSaveFileName != string.Empty)
                {
                    File.WriteAllText(TempSaveFileName, JObject.Parse(JSON_IPCONFIG).ToString());
                    MessageBox.Show("导出完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void set_interface_load_Click(object sender, EventArgs e)
        {
            try
            {
                string TempLoadConfigFileName = Util.ShowSelectDialog(false, false, "坤Tools网卡配置", "*.KUNNET")
                    .FirstOrDefault();

                if (TempLoadConfigFileName != string.Empty &&
                    string.Compare(TempLoadConfigFileName, default, StringComparison.Ordinal) != 0)
                {
                    string JsonContent = File.ReadAllText(TempLoadConfigFileName);

                    if (JsonContent != string.Empty)
                    {
                        JObject TempObj = JObject.Parse(JsonContent);

                        set_interface_ipaddress.Text = TempObj.TryGetValue("IPADDRESS", out JToken ipaddressToken)
                            ? ipaddressToken.ToString()
                            : "0.0.0.0";
                        set_interface_mask.Text = TempObj.TryGetValue("IPMASK", out JToken ipmaskToken)
                            ? ipmaskToken.ToString()
                            : "0.0.0.0";
                        set_interface_gateway.Text = TempObj.TryGetValue("IPGATEWAY", out JToken ipgatewayToken)
                            ? ipgatewayToken.ToString()
                            : "0.0.0.0";
                        set_interface_dns1.Text = TempObj.TryGetValue("DNS1", out JToken dns1Token)
                            ? dns1Token.ToString()
                            : "0.0.0.0";
                        set_interface_dns2.Text = TempObj.TryGetValue("DNS2", out JToken dns2Token)
                            ? dns2Token.ToString()
                            : "0.0.0.0";
                    }
                    else
                    {
                        MessageBox.Show("配置文件内无内容，配置未加载", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(
                "https://microsoftedge.microsoft.com/addons/detail/save-as-mhtml-mime-html/mlicehneaeopekphfhnfcbkdnelhldmo");
        }

        private void money_small_TextChanged(object sender, EventArgs e)
        {
            if (money_small.Text != string.Empty && Decimal.TryParse(money_small.Text, out decimal Money) && Money > 0)
            {
                money_big.Text = Util.ConvertMoney2ChineseYulan(Decimal.Parse(money_small.Text));
            }
            else
            {
                money_big.Text = string.Empty;
            }
        }

        private void copy_money_big_result_Click(object sender, EventArgs e)
        {
            if (money_big.Text != string.Empty)
            {
                Clipboard.SetText(money_big.Text);
                MessageBox.Show("复制成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void DontateMe_Click(object sender, EventArgs e)
        {
            DonateMeFrm DMF = new DonateMeFrm();

            DMF.ShowDialog();
        }

        private void DisableAdminBehavior_Click(object sender, EventArgs e)
        {
            if (Util.IsAdministrator())
            {
                if (DisableAdminBehavior.Text.StartsWith("启用"))
                {
                    Util.SetAdministratorBehavior(true);
                }
                else if (DisableAdminBehavior.Text.StartsWith("禁用"))
                {
                    Util.SetAdministratorBehavior(false);
                }

                MessageBox.Show("修改成功，重启电脑后设置生效", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                GetAdministratorBehaviorStatus();
            }
            else
            {
                MessageBox.Show("请使用管理员身份重新运行软件执行修改", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void 添加PDF水印ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("添加PDF水印"); }

        private void 移除PDF水印ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("移除PDF水印"); }

        private void pDF版本转换ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("PDF版本转换"); }


        private int GetTabPageIndex(string TabPageName)
        {
            int PageIndex = -1;
            foreach (TabPage TempPage in MainTabPage.TabPages)
            {
                if (string.Compare(TempPage.Text, TabPageName, StringComparison.Ordinal) == 0)
                {
                    PageIndex = MainTabPage.TabPages.IndexOf(TempPage);
                }
            }
            return PageIndex;
        }

        private void BodyForm_Resize(object sender, EventArgs e)
        {
            bodyform_width.Text = this.Width.ToString();
            bodyform_height.Text = this.Height.ToString();
        }

        private void MainTabPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MainTabPage.SelectedIndex == GetTabPageIndex("网卡设置"))
            {
                UpdateNetworkList();
            }
            if (MainTabPage.SelectedIndex == GetTabPageIndex("单人表生成"))
            {
                SignDocumentUseOutPersonPathTextBox.Enabled = SignDocumentUseOutPersonPath.Checked;
                SignDocumentUseOutPersonPathButton.Enabled = SignDocumentUseOutPersonPath.Checked;

                SignDocumentOnlyUseOnlyImageTextBox.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
                SignDocumentOnlyUseOnlyImageButton.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
            }
            if (MainTabPage.SelectedIndex == GetTabPageIndex("Twain扫描仪调试"))
            {
                try
                {
                    if (!TwainInitState)
                    {
                        InitTwain();
                        TwainInitState = true;
                    }

                    foreach (Control TwainControl in MainTabPage.TabPages[GetTabPageIndex("Twain扫描仪调试")].Controls)
                    {
                        TwainControl.Enabled = true;
                    }
                }
                catch
                {
                    MessageBox.Show(
                        "没有在系统中找到TwainDsm数据源管理器，请自行下载TwainDsm.dll文件放在程序目录下调用，否则无法使用twain调试功能",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    foreach (Control TwainControl in MainTabPage.TabPages[GetTabPageIndex("Twain扫描仪调试")].Controls)
                    {
                        TwainControl.Enabled = false;
                    }
                }
            }
        }

        private void 初始化配置文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ProgramFileName = Process.GetCurrentProcess().ProcessName;

            string ApplicationConfigFile = $"{ProgramFileName}.exe.config";

            string AppConfigContent = Util.GetEmbedResourceObj("ConfigTemplate").ToString();

            File.WriteAllText(ApplicationConfigFile, AppConfigContent);

            DialogResult ExitResult = MessageBox.Show(
                "已经初始化配置文件，是否需要重启软件？",
                "提示",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Asterisk);
            if (ExitResult == DialogResult.OK)
            {
                string AppExcuteFile = Application.ExecutablePath;
                Process.Start(AppExcuteFile);
                Environment.Exit(0);
            }
        }

        private void VoteTableHelpDate_Click(object sender, EventArgs e)
        {
            DateTime NowDateTime = DateTime.Now;

            string GetNowDateString = NowDateTime.ToString("yyyy年MM月dd日");

            VoteTable_GenDate.Text = GetNowDateString;
        }

        private void VoteTable_Render_Click(object sender, EventArgs e)
        {
            string SaveVoteFileName = Util.ShowSaveFileDialog("WORD文档|*.docx");

            Task.Run(
                () =>
                {
                    try
                    {
                        if (SaveVoteFileName != string.Empty)
                        {
                            (sender as System.Windows.Forms.Button).Enabled = false;
                            using MemoryStream TempDocumentMemoryStream = new MemoryStream(
                                (byte[])Util.GetEmbedResourceObj("VoteReportDoc"));
                            Util.GenerateVoteReportDocx(
                                TempDocumentMemoryStream,
                                VoteTable_Title.Text,
                                VoteTable_Start.Text,
                                VoteTable_Name.Text,
                                VoteTable_Agree.Text,
                                VoteTable_UnAgree.Text,
                                VoteTable_Abdon.Text,
                                VoteTable_End.Text,
                                VoteTable_GenDate.Text,
                                VoteTable_Names.Lines,
                                SaveVoteFileName,
                                VoteTable_Random_PageCounts.Checked,
                                (double)VoteTableHeight.Value,
                                (int)VoteTableFontSize.Value,
                                VoteTable_KeepPageFooter.Checked,
                                VoteTable_NumberIndex_Keep.Checked,
                                VoteTable_AnotherTableShow.Checked);
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        (sender as System.Windows.Forms.Button).Enabled = true;
                                        if (MessageBox.Show(
                                                "已生成选举报告单，是否打开文件？",
                                                "提示",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Asterisk) ==
                                            DialogResult.Yes)
                                        {
                                            Process.Start(SaveVoteFileName);
                                        }
                                    }));
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void 选举报告单样式生成ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("选举结果报告单样式生成"); }

        private void VoteTable_Names_TextChanged(object sender, EventArgs e)
        { VoteTableName_CountText.Text = $"共有{VoteTable_Names.Lines.Count()}个名字"; }

        private void SetPauseTime_Click(object sender, EventArgs e)
        {
            if (Util.IsAdministrator())
            {
                Util.SetPauseUpdateTime(int.Parse(PauseUpdateYears.SelectedValue.ToString()));

                string CheckVersionResult = Util.Check_11_Or_10_Version();
                if (string.Compare(CheckVersionResult, "10", StringComparison.Ordinal) == 0)
                {
                    Process.Start("ms-settings:windowsupdate-options");
                }
                else if (string.Compare(CheckVersionResult, "11", StringComparison.Ordinal) == 0)
                {
                    Process.Start("ms-settings:windowsupdate");
                }
                else
                {
                    MessageBox.Show("只支持windows10/windows11系统", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            else
            {
                MessageBox.Show("请使用管理员身份证重新运行软件后执行本操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void OpenSystemUpdate_Click(object sender, EventArgs e)
        {
            string CheckVersionResult = Util.Check_11_Or_10_Version();
            if (string.Compare(CheckVersionResult, "10", StringComparison.Ordinal) == 0)
            {
                Process.Start("ms-settings:windowsupdate-options");
            }
            else if (string.Compare(CheckVersionResult, "11", StringComparison.Ordinal) == 0)
            {
                Process.Start("ms-settings:windowsupdate");
            }
        }


        private void LoadExamFile_FillReportView()
        {
            Task.Run(
                () =>
                {
                    if (ExamFileText.Text != string.Empty)
                    {
                        try
                        {
                            //获取数据库内的报表和考评表信息
                            DataTable TempTable = Util.GetExamInfo(ExamFileText.Text);

                            if (TempTable != null && TempTable.Rows.Count > 0)
                            {
                                VFSOFT_Report_ListView.Items.Clear();//加载之前先清除数据
                                foreach (DataRow row in TempTable.Rows)
                                {
                                    System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(
                                        row["报表ID"].ToString());
                                    item.SubItems.Add(row["报表名称"].ToString());
                                    item.SubItems.Add(row["所属考评表"].ToString());
                                    item.SubItems.Add(row["考评表ID"].ToString());
                                    VFSOFT_Report_ListView.Items.Add(item);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }));
                        }
                    }
                });
        }


        private string GET_VFSOFT_ID(string IdType)
        {
            string ResultId = string.Empty;
            if (VFSOFT_Report_ListView.Items.Count > 0 && VFSOFT_Report_ListView.SelectedItems.Count > 0)
            {
                if (string.Compare(IdType, "exam", StringComparison.Ordinal) == 0)
                {
                    ResultId = VFSOFT_Report_ListView.SelectedItems[0].SubItems[3].Text;
                }
                else if (string.Compare(IdType, "report", StringComparison.Ordinal) == 0)
                {
                    ResultId = VFSOFT_Report_ListView.SelectedItems[0].Text;
                }
            }

            return ResultId;
        }

        private void SelectExamFile_Click(object sender, EventArgs e)
        {
            string ExamFile = Util.ShowSelectDialog(false, false, "MDB文件", "*.mdb", vfsoft_examdata_path_textbox.Text)
                .FirstOrDefault();

            if (string.Compare(ExamFile, default, StringComparison.Ordinal) != 0 && ExamFile != string.Empty)
            {
                ExamFileText.Text = ExamFile;
                LoadExamFile_FillReportView();
            }
        }

        private async void VFSOFT_Report_ListView_ItemSelectionChangedAsync(
            object sender,
            ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                string selectedExamId = GET_VFSOFT_ID("exam");
                string selectedReportId = GET_VFSOFT_ID("report");

                if (ExamFileText.Text != string.Empty && selectedExamId != string.Empty)
                {
                    List<string> selectedExamItems = await Task.Run(
                        () => Util.GetExamItemsAndExamSelectors(ExamFileText.Text, selectedExamId))
                        .ConfigureAwait(true);

                    DataTable selectedExamRaidInfoTable = await Task.Run(
                        () => Util.GetExamRaidInfo(ExamFileText.Text, selectedExamId))
                        .ConfigureAwait(true);

                    List<string> selectedReportItems = await Task.Run(
                        () => Util.GetReportItems(ExamFileText.Text, selectedReportId))
                        .ConfigureAwait(true);

                    var selectedReportColsAndRows = await Task.Run(
                        () => Util.GetExamReportColsAndRows(ExamFileText.Text, selectedReportId))
                        .ConfigureAwait(true);


                    ExamItemsCombox.DataSource = selectedExamItems;
                    ExamReportRowMode_ExamList.DataSource = new List<string>(selectedExamItems);
                    ExamRaidInfoCombox.DataSource = selectedExamRaidInfoTable;
                    ExamRaidInfoCombox.DisplayMember = "RaidShow";
                    ExamRaidInfoCombox.ValueMember = "RaidValue";
                    ExamReportParaRaidInfo_Combox.DataSource = selectedExamRaidInfoTable.Copy();
                    ExamReportParaRaidInfo_Combox.DisplayMember = "RaidShow";
                    ExamReportParaRaidInfo_Combox.ValueMember = "RaidValue";
                    ExamReportItems.DataSource = selectedReportItems;

                    ExamReportRowMode_Row_Combox.DataSource = new List<int>(selectedReportColsAndRows.report_rows);
                    ExamReportRowMode_Col_Start_Combox.DataSource = new List<int>(selectedReportColsAndRows.report_cols);
                    ExamReportRowMode_Col_End_Combox.DataSource = new List<int>(selectedReportColsAndRows.report_cols);

                    ExamReportColMode_Col_Combox.DataSource = new List<int>(selectedReportColsAndRows.report_cols);
                    ExamReportColMode_Row_Start_Combox.DataSource = new List<int>(selectedReportColsAndRows.report_rows);
                    ExamReportColMode_Row_End_Combox.DataSource = new List<int>(selectedReportColsAndRows.report_rows);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ExamItemsCombox_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            try
            {
                string selectedExamId = GET_VFSOFT_ID("exam");

                if (ExamFileText.Text != string.Empty && selectedExamId != string.Empty)
                {
                    List<string> selectedExamItemsSelectors = await Task.Run(
                        () => Util.GetExamItems_Selectors(ExamFileText.Text, selectedExamId, ExamItemsCombox.Text))
                        .ConfigureAwait(true);
                    ExamItemsSelectorsCombox.DataSource = selectedExamItemsSelectors;

                    if (ExamReportItems.SelectedIndex != 0)
                    {
                        ExamReplaceItemName.Text = ExamItemsCombox.SelectedItem.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshVFSOFTREPORT_Click(object sender, EventArgs e) { LoadExamFile_FillReportView(); }

        private void ExamReportChange_Click(object sender, EventArgs e)
        {
            try
            {
                if (VFSOFT_Report_ListView.SelectedItems.Count > 0 &&
                    (ExamReportChangeItemName.Checked ||
                        ExamReportChangeItemDataFormat.Checked ||
                        ExamReportChangeItemDataRaid.Checked))
                {
                    DialogResult TempConfirmResult = MessageBox.Show(
                        "确定执行修改吗？执行后数据无法恢复",
                        "提示",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);
                    Task.Run(
                        () =>
                        {
                            if (TempConfirmResult == DialogResult.Yes)
                            {
                                (sender as System.Windows.Forms.Button).Enabled = false;
                                RefreshVFSOFTREPORT.Enabled = false;
                                List<string> SqlStrings = new List<string>();

                                string selectedReportId = GET_VFSOFT_ID("report");

                                if (ExamFileText.Text != string.Empty && selectedReportId != string.Empty)
                                {
                                    if (ExamReportItems.SelectedIndex == 0)
                                    {
                                        SqlStrings = Util.ChangeExamReportContent(
                                            ExamFileText.Text,
                                            selectedReportId,
                                            ExamReportItems.SelectedItem.ToString(),
                                            ExamReportChangeItemName.Checked,
                                            ExamReportChangeItemDataFormat.Checked,
                                            ExamReportChangeItemDataRaid.Checked,
                                            ExamReplaceItemName.Text,
                                            ExamReplaceDataFormat.SelectedItem.ToString(),
                                            ExamReplaceRaids.Text);
                                    }
                                    else
                                    {
                                        SqlStrings = Util.ChangeExamReportContent(
                                            ExamFileText.Text,
                                            selectedReportId,
                                            ExamReportItems.SelectedItem.ToString(),
                                            ExamReportChangeItemName.Checked,
                                            ExamReportChangeItemDataFormat.Checked,
                                            ExamReportChangeItemDataRaid.Checked,
                                            ExamReplaceItemName.Text,
                                            ExamReplaceDataFormat.SelectedItem.ToString(),
                                            ExamReplaceRaids.Text,
                                            ExamReportDataFormat.SelectedItem.ToString(),
                                            ExamReportRaids.SelectedItem.ToString()

                );
                                    }

                                    if (ExamReportChangeItemName.Checked ||
                                        ExamReportChangeItemDataFormat.Checked ||
                                        ExamReportChangeItemDataRaid.Checked)
                                    {
                                        foreach (string UpdateString in SqlStrings)
                                        {
                                            Debug.WriteLine(UpdateString);
                                            var (_, Connection) = Util.ExecuteAccessSql(ExamFileText.Text, UpdateString);

                                            Connection.Close();

                                            ExamReportChangeProgress.PerformStep();
                                        }

                                        ExamReportChangeProgress.Value = 0;

                                        this.BeginInvoke(
                                            new Action(
                                                () =>
                                                {
                                                    MessageBox.Show(
                                                        "修改完毕，请前往考评系统核对报表",
                                                        "提示",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk);
                                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                                    RefreshVFSOFTREPORT.Enabled = true;
                                                }));
                                    }
                                }
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                (sender as System.Windows.Forms.Button).Enabled = true;
                RefreshVFSOFTREPORT.Enabled = true;
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SignPersionDocumentChooseFile_Click(object sender, EventArgs e)
        {
            string SignDocumentFileName = Util.ShowSelectDialog(false, false, "WORD文档", "*.doc;*.docx").FirstOrDefault();

            if (string.Compare(SignDocumentFileName, default, StringComparison.Ordinal) != 0 &&
                SignDocumentFileName != string.Empty)
            {
                SignPersionDocument.Text = SignDocumentFileName;
            }
        }

        private void SignDocumentFileSaveButton_Click(object sender, EventArgs e)
        {
            string SignDocumentFileSavePath = Util.ShowSelectDialog(false, true, string.Empty, string.Empty)
                .FirstOrDefault();

            if (string.Compare(SignDocumentFileSavePath, default, StringComparison.Ordinal) != 0 &&
                SignDocumentFileSavePath != string.Empty)
            {
                SignDocumentFileSaveBox.Text = SignDocumentFileSavePath;
            }
        }

        private void SignDocumentReplaceStart_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        if (SignPersionDocument.Text != string.Empty &&
                            SignDocumentFileSaveBox.Text != string.Empty &&
                            SingPersonImageAlt.Text != string.Empty)
                        {
                            (sender as System.Windows.Forms.Button).Enabled = false;
                            SignDocumentProgress.Maximum = (int)SignDocumentNeedCount.Value;
                            int FileIndex = 0;

                            string SourceFileNameNotExtension = Path.GetFileNameWithoutExtension(
                                SignPersionDocument.Text);
                            string NewDocumentFileName;
                            string FileIndexName = string.Empty;
                            string FileEndName = SignDocumentFileEndName.Text;

                            if (SignDocumentUseEmbedImage.Checked)
                            {
                                //使用内置的图片文件，最多只有999张

                                List<Stream> TempPersonStrem = Util.GetPersonCodeZipFileStream();

                                foreach (Stream PersonCode in TempPersonStrem)
                                {
                                    FileIndex++;
                                    if (SignDocumentUseOnlyPageNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}页）";
                                    }
                                    if (SignDocumentUsePersonNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}人）";
                                    }
                                    if (SignDocumentUseOnlyNumber.Checked)
                                    {
                                        FileIndexName = $"（{FileIndex.ToString("D3")}{FileEndName}）";
                                    }
                                    NewDocumentFileName = Path.Combine(
                                        SignDocumentFileSaveBox.Text,
                                        $"{SourceFileNameNotExtension}{FileIndexName}.docx");
                                    Util.SingleDocumentReplacePersonCode(
                                        SignPersionDocument.Text,
                                        SingPersonImageAlt.Text.Split('@'),
                                        PersonCode,
                                        NewDocumentFileName);
                                    PersonCode.Close();
                                    SignDocumentProgress.PerformStep();
                                    if (FileIndex >= SignDocumentNeedCount.Value)
                                    {
                                        break;
                                    }
                                }
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            SignDocumentProgress.Value = 0;

                                            DialogResult OpenResult = MessageBox.Show(
                                                "生成完毕，是否需要打开文件所在目录？",
                                                "提示",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Asterisk);
                                            if (OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(SignDocumentFileSaveBox.Text);
                                            }
                                        }));
                            }

                            if (SignDocumentUseOutPersonPath.Checked &&
                                SignDocumentUseOutPersonPathTextBox.Text != string.Empty)
                            {
                                //使用外部的图片文件
                                string[] PersonFiles = Directory.GetFiles(SignDocumentUseOutPersonPathTextBox.Text, "*");

                                foreach (string PersonFile in PersonFiles)
                                {
                                    if (PersonFile.Contains(".jpeg") ||
                                        PersonFile.Contains(".jpg") ||
                                        PersonFile.Contains(".png") ||
                                        PersonFile.Contains(".bmp"))
                                    {
                                        FileIndex++;
                                        if (SignDocumentUseOnlyPageNumber.Checked)
                                        {
                                            FileIndexName = $"（第{FileIndex}页）";
                                        }
                                        if (SignDocumentUsePersonNumber.Checked)
                                        {
                                            FileIndexName = $"（第{FileIndex}人）";
                                        }
                                        if (SignDocumentUseOnlyNumber.Checked)
                                        {
                                            FileIndexName = $"（{FileIndex.ToString("D3")}{FileEndName}）";
                                        }
                                        NewDocumentFileName = Path.Combine(
                                            SignDocumentFileSaveBox.Text,
                                            $"{SourceFileNameNotExtension}{FileIndexName}.docx");
                                        Util.SingleDocumentReplacePersonCode(
                                            SignPersionDocument.Text,
                                            SingPersonImageAlt.Text.Split('@'),
                                            PersonFile,
                                            NewDocumentFileName);
                                        SignDocumentProgress.PerformStep();
                                        if (FileIndex >= SignDocumentNeedCount.Value)
                                        {
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            SignDocumentProgress.Value = 0;

                                            DialogResult OpenResult = MessageBox.Show(
                                                "生成完毕，是否需要打开文件所在目录？",
                                                "提示",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Asterisk);
                                            if (OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(SignDocumentFileSaveBox.Text);
                                            }
                                        }));
                            }

                            if (SignDocumentOnlyUseOnlyImage.Checked &&
                                SignDocumentOnlyUseOnlyImageTextBox.Text != string.Empty)
                            {
                                //只使用一张图片
                                for (int TempIndex = 0; TempIndex < SignDocumentNeedCount.Value; TempIndex++)
                                {
                                    if (SignDocumentUseOnlyPageNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}页）";
                                    }
                                    if (SignDocumentUsePersonNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}人）";
                                    }
                                    if (SignDocumentUseOnlyNumber.Checked)
                                    {
                                        FileIndexName = $"（{FileIndex.ToString("D3")}{FileEndName}）";
                                    }
                                    NewDocumentFileName = Path.Combine(
                                        SignDocumentFileSaveBox.Text,
                                        $"{SourceFileNameNotExtension}{FileIndexName}.docx");
                                    Util.SingleDocumentReplacePersonCode(
                                        SignPersionDocument.Text,
                                        SingPersonImageAlt.Text.Split('@'),
                                        SignDocumentOnlyUseOnlyImageTextBox.Text,
                                        NewDocumentFileName);
                                    SignDocumentProgress.PerformStep();
                                }
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            SignDocumentProgress.Value = 0;

                                            DialogResult OpenResult = MessageBox.Show(
                                                "生成完毕，是否需要打开文件所在目录？",
                                                "提示",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Asterisk);
                                            if (OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(SignDocumentFileSaveBox.Text);
                                            }
                                        }));
                            }
                 (sender as System.Windows.Forms.Button).Enabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    (sender as System.Windows.Forms.Button).Enabled = true;
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void SignDocumentUseOutPersonPathButton_Click(object sender, EventArgs e)
        {
            string PerSonPath = Util.ShowSelectDialog(false, true, string.Empty, string.Empty).FirstOrDefault();

            if (string.Compare(PerSonPath, default, StringComparison.Ordinal) != 0 && PerSonPath != string.Empty)
            {
                SignDocumentUseOutPersonPathTextBox.Text = PerSonPath;
            }
        }

        private void SignDocumentUseOutPersonPath_CheckedChanged(object sender, EventArgs e)
        {
            SignDocumentUseOutPersonPathTextBox.Enabled = SignDocumentUseOutPersonPath.Checked;
            SignDocumentUseOutPersonPathButton.Enabled = SignDocumentUseOutPersonPath.Checked;
        }

        private void SignDocumentOnlyUseOnlyImageButton_Click(object sender, EventArgs e)
        {
            string PerSonImage = Util.ShowSelectDialog(false, false, "图片文件", "*.jpg;*.jpeg;*.bmp;*.png")
                .FirstOrDefault();

            if (string.Compare(PerSonImage, default, StringComparison.Ordinal) != 0 && PerSonImage != string.Empty)
            {
                SignDocumentOnlyUseOnlyImageTextBox.Text = PerSonImage;
            }
        }

        private void SignDocumentOnlyUseOnlyImage_CheckedChanged(object sender, EventArgs e)
        {
            SignDocumentOnlyUseOnlyImageTextBox.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
            SignDocumentOnlyUseOnlyImageButton.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
        }

        private void TwainScanNumberSet_TextChanged(object sender, EventArgs e)
        {
            //当设置扫描份数的时候，禁止选择跳过空白页和双面扫描
            if (TwainScanNumberSet.Text != string.Empty)
            {
                DoubleScanBox.Checked = false;
                SkipBlankPage.Checked = false;
            }
            DoubleScanBox.Enabled = (TwainScanNumberSet.Text == string.Empty);
            SkipBlankPage.Enabled = (TwainScanNumberSet.Text == string.Empty);
        }

        private async void ExamReportItems_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            try
            {
                ExamReportDataFormat.Enabled = ExamReportItems.SelectedIndex != 0;
                ExamReportRaids.Enabled = ExamReportItems.SelectedIndex != 0;
                ExamReplaceItemName.Enabled = ExamReportItems.SelectedIndex != 0;
                ExamReportChangeItemName.Enabled = ExamReportItems.SelectedIndex != 0;


                if (ExamReportItems.SelectedIndex == 0)
                {
                    ExamReplaceItemName.Text = string.Empty;
                    ExamReportChangeItemName.Checked = false;
                }
                else
                {
                    if (ExamReportItems.SelectedIndex <= ExamItemsCombox.Items.Count)
                    {
                        ExamItemsCombox.SelectedIndex = ExamReportItems.SelectedIndex - 1;
                    }
                }

                string selectedReportId = GET_VFSOFT_ID("report");
                if (ExamFileText.Text != string.Empty && selectedReportId != string.Empty)
                {
                    List<string> selectedReportKind = await Task.Run(
                        () => Util.GetReportItemInfoByExamName(
                            ExamFileText.Text,
                            selectedReportId,
                            ExamReportItems.SelectedItem.ToString(),
                            "Kind"))
                        .ConfigureAwait(true);
                    List<string> selectedReportRaid = await Task.Run(
                        () => Util.GetReportItemInfoByExamName(
                            ExamFileText.Text,
                            selectedReportId,
                            ExamReportItems.SelectedItem.ToString(),
                            "Raid"))
                        .ConfigureAwait(true);


                    ExamReportDataFormat.DataSource = selectedReportKind;
                    ExamReportRaids.DataSource = selectedReportRaid;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExamReportRaids_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ExamReplaceRaids.Text = ExamReportRaids.SelectedItem.ToString(); 
        }

        private void BodyForm_SizeChanged(object sender, EventArgs e)
        {
            //if (AutoSize != null) // 一定加这个判断，电脑缩放布局不是100%的时候，会报错
            //{
            //    AutoSize.FormSizeChanged();
            //}
        }

        private void 单人表生成ToolStripMenuItem_Click(object sender, EventArgs e)
        { MainTabPage.SelectedIndex = GetTabPageIndex("单人表生成"); }

        private void 考评报表修改ToolStripMenuItem_Click(object sender, EventArgs e)
        { MainTabPage.SelectedIndex = GetTabPageIndex("考评报表修改"); }

        private void ExamRaidsCopy_Click(object sender, EventArgs e)
        {
            List<string> RaidsList = ExamRaidInfoCombox.DataSource as List<string>;

            if (RaidsList != null && RaidsList.Count > 0)
            {
                StringBuilder TempRaidString = new StringBuilder();

                foreach (string Raid in RaidsList)
                {
                    TempRaidString.Append(Raid);
                    TempRaidString.Append("，");
                }
                TempRaidString.Remove(TempRaidString.Length - 1, 1);

                Clipboard.SetText(TempRaidString.ToString());

                MessageBox.Show("已复制评测主体合计身份", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ChooseExamFileButton_Click(object sender, EventArgs e)
        {
            string UnitExamFileName = Util.ShowSelectDialog(false, false, "exam文件", ".exam").FirstOrDefault();

            if (string.Compare(UnitExamFileName, default, StringComparison.Ordinal) != 0 &&
                UnitExamFileName != string.Empty)
            {
                ChooseExamFileTextBox.Text = UnitExamFileName;
                LoadUnitCompNameAsync();
            }
        }

        private async void LoadUnitCompNameAsync()
        {
            try
            {
                if (ChooseExamFileTextBox.Text != string.Empty)
                {
                    List<string> UnitComputerNames = await Task.Run(
                        () => Util.GetUnitExamComputerName(ChooseExamFileTextBox.Text))
                        .ConfigureAwait(true);

                    ExamComputerNames.DataSource = UnitComputerNames;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetRandomComputerNameButton_Click(object sender, EventArgs e)
        { ExamComputerChangeName.Text = Util.GetRandomMatchName((int)GetRandomComputerNameRandLength.Value); }

        private void ExamComputerNameChange_Click(object sender, EventArgs e)
        {
            if (ChooseExamFileTextBox.Text != string.Empty &&
                ExamComputerChangeName.Text != string.Empty &&
                ExamComputerNames.SelectedItem != null)
            {
                DialogResult TempConfirmResult = MessageBox.Show(
                    "确定执行修改吗？执行后数据无法恢复",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);
                if (TempConfirmResult == DialogResult.Yes)
                {
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                (sender as System.Windows.Forms.Button).Enabled = false;

                                string UpdateString = Util.ChangeUnitExamComputerName(
                                    ExamComputerNames.SelectedItem.ToString(),
                                    ExamComputerChangeName.Text);

                                var (_, Connection) = Util.ExecuteAccessSql(ChooseExamFileTextBox.Text, UpdateString);

                                Connection.Close();


                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                            MessageBox.Show("修改完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                            LoadUnitCompNameAsync();
                                        }));
                            }
                            catch (Exception ex)
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            (sender as System.Windows.Forms.Button).Enabled = true;
                                            MessageBox.Show(
                                                ex.Message,
                                                "错误",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                        }));
                            }
                        });
                }
            }
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://www.123pan.com/s/JXY5Vv-HMDwd.html"); }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://www.123pan.com/s/JXY5Vv-HMDwd.html"); }

        private void ExamItemsCombox_MouseHover(object sender, EventArgs e)
        {
            if (ExamItemsCombox.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamItemsCombox, $"[共有{ExamItemsCombox.Items.Count}个项目]");
            }
        }

        private void ExamItemsSelectorsCombox_MouseHover(object sender, EventArgs e)
        {
            if (ExamItemsSelectorsCombox.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamItemsSelectorsCombox, $"[共有{ExamItemsSelectorsCombox.Items.Count}个选项]");
            }
        }

        private void ExamRaidInfoCombox_MouseHover(object sender, EventArgs e)
        {
            if (ExamRaidInfoCombox.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamRaidInfoCombox, $"[共有{ExamRaidInfoCombox.Items.Count}个主体]");
            }
        }

        private void ExamReportItems_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportItems.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamReportItems, $"[共有{ExamReportItems.Items.Count - 1}个项目]");
            }
        }

        private void ExamRaidInfoCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.Compare(
                    ExamRaidInfoCombox.SelectedValue.ToString(),
                    "System.Data.DataRowView",
                    StringComparison.Ordinal) !=
                0)
            {
                ExamReplaceRaids.Text = ExamRaidInfoCombox.SelectedValue.ToString();
            }
        }

        private void GetNumberIndexList_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    List<string> TempIndexLists = new List<string>();

                    for (int NumberIndex = (int)NumberIndexStart.Value; NumberIndex <= (int)NumberIndexEnd.Value; NumberIndex++)
                    {
                        TempIndexLists.Add(NumberIndex.ToString());
                    }

                    NumberIndexResultBox.Lines = TempIndexLists.ToArray();
                });
        }

        private void CopyNumberIndexResult_Click(object sender, EventArgs e)
        {
            if (NumberIndexResultBox.Text != string.Empty)
            {
                Clipboard.SetText(NumberIndexResultBox.Text.Replace("\n", Environment.NewLine));

                MessageBox.Show("复制成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void SplitPPT_SelectFiles_Click(object sender, EventArgs e) { UI.AddSplitPPTFiles(); }

        private void SplitPPT_Button_Click(object sender, EventArgs e)
        {
        }

        private void 拆分PPTToolStripMenuItem_Click(object sender, EventArgs e)
        { MainTabPage.SelectedIndex = GetTabPageIndex("拆分PPT"); }

        private void ReleaseKMSAIO_Click(object sender, EventArgs e)
        {
            string SaveFileName = Util.ShowSaveFileDialog("CMD文件|*.cmd");

            if (SaveFileName != string.Empty)
            {
                string KMSAIOCMD = Util.GetEmbedResourceObj("KMSAIO") as string;
                File.WriteAllText(SaveFileName, KMSAIOCMD, Encoding.Default);
                MessageBox.Show("释放完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ReleaseRenamerApp_Click(object sender, EventArgs e)
        {
            string SaveFileName = Util.ShowSaveFileDialog("EXE文件|*.exe");

            if (SaveFileName != string.Empty)
            {
                byte[] RenamerProApplicationBytes = Util.GetEmbedResourceObj("ReNamerPro73") as byte[];
                File.WriteAllBytes(SaveFileName, RenamerProApplicationBytes);
                MessageBox.Show("释放完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ReleaseRenamerReplaceConfig_Click(object sender, EventArgs e)
        {
            string SaveFileName = Util.ShowSaveFileDialog("RNP文件|*.rnp");

            if (SaveFileName != string.Empty)
            {
                byte[] RenamerProRnpBytes = Util.GetEmbedResourceObj("移除副本规则_部分自定义规则") as byte[];
                File.WriteAllBytes(SaveFileName, RenamerProRnpBytes);
                MessageBox.Show("释放完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void 折叠本节点全部内容ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.TreeNode CurrentSelectNode = this.DecodeDataMatrixView.SelectedNode;
            CurrentSelectNode.Collapse();
        }

        private void 折叠全部节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (System.Windows.Forms.TreeNode TempNode in DecodeDataMatrixView.Nodes)
            {
                TempNode.Collapse();
            }
        }

        private void TwainSetDeviceSaveTime_Click(object sender, EventArgs e)
        {
            dataSource ??= twainSession.DefaultSource;

            if (dataSource.Open() == ReturnCode.Success)
            {
                if (dataSource.Capabilities.CapPowerSaveTime.CanSet)
                {
                    dataSource.Capabilities.CapPowerSaveTime.SetValue((int)TwainSleepTimeSet.SelectedItem);
                }
            }
        }

        private void CheckVersionButton_Click(object sender, EventArgs e)
        {
            AutoUpdater.Start(VersionInfo.VersionUpdateCheckUrl);
            AutoUpdater.CheckForUpdateEvent -= AutoUpdater_CheckVersion_ClickEvent;
            AutoUpdater.CheckForUpdateEvent += AutoUpdater_CheckVersion_ClickEvent;
        }

        private void AutoUpdater_CheckVersion_ClickEvent(UpdateInfoEventArgs s1)
        {
            if (s1.Error == null && s1.IsUpdateAvailable)
            {
                VersionInfo.VersionUpdateLogUrl = s1.ChangelogURL;
                DialogResult UpdateResult = MessageBox.Show(
                    $"当前版本为：{s1.InstalledVersion}，最新版本为：{s1.CurrentVersion}，是否需要打开更新页面进行更新？",
                    "提示",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Asterisk);

                if (UpdateResult == DialogResult.OK)
                {
                    Process.Start(s1.DownloadURL);
                }
            }
            else
            {
                MessageBox.Show(
                    $"当前版本为{s1.InstalledVersion}，没有检测到新版本",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void VersionUpdateLog_Click(object sender, EventArgs e)
        {
            if (VersionInfo.VersionUpdateLogUrl != string.Empty)
            {
                Process.Start(VersionInfo.VersionUpdateLogUrl);
            }
        }

        private void UseLocalOffice_CheckedChanged(object sender, EventArgs e)
        {
            if (Util.IsAdministrator())
            {
                this.PDFComplianceBox.Enabled = !UseLocalOffice.Checked;
                this.Word2PDFOptimizCombox.Enabled = !UseLocalOffice.Checked;
                this.word2pdf_colorMode.Enabled = !UseLocalOffice.Checked;
                this.EmbedFonts.Enabled = !UseLocalOffice.Checked;
                this.word2pdf_need_update_fields.Enabled = !UseLocalOffice.Checked;
                UseLocalOffice.Enabled = true;
            }
            else
            {
                if (UseLocalOffice.Checked)
                {
                    MessageBox.Show("本功能需要管理员身份运行程序，请使用管理员身份运行后重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                UseLocalOffice.Checked = false;
                UseLocalOffice.Enabled = false;
            }
        }

        private void ExamReportRowMode_ExamList_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportRowMode_ExamList.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(ExamReportRowMode_ExamList, ExamReportRowMode_ExamList.SelectedItem.ToString());
            }
        }

        private void ExamReportRowMode_ExamSelectorBox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportRowMode_ExamSelectorBox.Text != string.Empty)
            {
                MainToolsTip.SetToolTip(ExamReportRowMode_ExamSelectorBox, ExamReportRowMode_ExamSelectorBox.Text);
            }
        }

        private void ExamReportParaRaidInfo_Combox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportParaRaidInfo_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportParaRaidInfo_Combox,
                    ExamReportParaRaidInfo_Combox.SelectedValue.ToString());
            }
        }

        private void ChangeExamReportParaButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (VFSOFT_Report_ListView.SelectedItems.Count > 0)
                {
                    DialogResult TempConfirmResult = MessageBox.Show(
                        "确定执行修改吗？执行后数据无法恢复",
                        "提示",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);
                    Task.Run(
                        () =>
                        {
                            if (TempConfirmResult == DialogResult.Yes)
                            {
                                (sender as System.Windows.Forms.Button).Enabled = false;

                                string ExamReportId = GET_VFSOFT_ID("report");

                                if (ExamFileText.Text != string.Empty && ExamReportId != string.Empty)
                                {
                                    Util.ChangeExamReportParaContent(
                                        ExamFileText.Text,
                                        ExamReportId,
                                        ExamReportPara_RowMode.Checked,
                                        ExamReportParaDataFormat_Combox.SelectedItem.ToString(),
                                        ExamReportParaRaidInfo_Combox.SelectedValue.ToString(),
                                        ExamReportParaDataFormatReverse_Combox.SelectedIndex.ToString(),
                                        ExamReportParaDataFormatFont_Box.Text,
                                        ExamReportRowMode_Row_Combox.SelectedItem.ToString(),
                                        ExamReportRowMode_Col_Start_Combox.SelectedItem.ToString(),
                                        ExamReportRowMode_Col_End_Combox.SelectedItem.ToString(),
                                        ExamReportRowMode_ExamList.SelectedItem.ToString(),
                                        ExamReportRowMode_ExamSelectorBox.Text,
                                        ExamReportRowMode_ExamSelectorInsertSame.Checked,
                                        ExamReportColMode_Col_Combox.SelectedItem.ToString(),
                                        ExamReportColMode_Row_Start_Combox.SelectedItem.ToString(),
                                        ExamReportColMode_Row_End_Combox.SelectedItem.ToString(),
                                        ExamReportColMode_ExamBox.Text,
                                        ExamReportColMode_ExamSelectorBox.Text,
                                        ExamReportRowMode_ExamItemInsertSame.Checked,
                                        ExamReportParaChangeProcessBar);

                                    this.BeginInvoke(
                                        new Action(
                                            () =>
                                            {
                                                MessageBox.Show(
                                                    "修改完毕，请前往考评系统核对报表",
                                                    "提示",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Asterisk);
                                                (sender as System.Windows.Forms.Button).Enabled = true;
                                            }));
                                }
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                (sender as System.Windows.Forms.Button).Enabled = true;
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetAllItemSplit_Click(object sender, EventArgs e)
        {
            if (ExamReportRowMode_ExamList.Items.Count > 0)
            {
                string ItemSplitsString = string.Join(
                    "|",
                    ExamReportRowMode_ExamList.Items.Cast<object>().Select(item => item.ToString()).ToList());
                if (ItemSplitsString != string.Empty)
                {
                    Clipboard.SetText(ItemSplitsString);

                    MessageBox.Show("已复制所有项目，请自行粘贴", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        private void ExamReportColMode_ExamBox_MouseHover(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ExamReportColMode_ExamBox.Text))
            {
                MainToolsTip.SetToolTip(ExamReportColMode_ExamBox, ExamReportColMode_ExamBox.Text);
            }
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChangeReportItemTempHelpPdfFile) || !File.Exists(ChangeReportItemTempHelpPdfFile))
            {
                ChangeReportItemTempHelpPdfFile = Path.GetTempFileName().Replace(".tmp", ".pdf");
                byte[] resourceBytes = (byte[])Util.GetEmbedResourceObj("ChangeReporItem");
                using (MemoryStream TempChangeReportItemHelpStream = new MemoryStream(resourceBytes))
                using (FileStream TempPdfFileStream = File.Create(ChangeReportItemTempHelpPdfFile))
                {
                    TempChangeReportItemHelpStream.CopyTo(TempPdfFileStream);
                    TempPdfFileStream.Flush(); // 确保文件写入完成
                }
            }
            Process.Start(ChangeReportItemTempHelpPdfFile);
        }

        private void ExamReportRowMode_Row_Combox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportRowMode_Row_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportRowMode_Row_Combox,
                    ExamReportRowMode_Row_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportRowMode_Col_Start_Combox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportRowMode_Col_Start_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportRowMode_Col_Start_Combox,
                    ExamReportRowMode_Col_Start_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportRowMode_Col_End_Combox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportRowMode_Col_End_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportRowMode_Col_End_Combox,
                    ExamReportRowMode_Col_End_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportColMode_Col_Combox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportColMode_Col_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportColMode_Col_Combox,
                    ExamReportColMode_Col_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportColMode_Row_Start_Combox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportColMode_Row_Start_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportColMode_Row_Start_Combox,
                    ExamReportColMode_Row_Start_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportColMode_Row_End_Combox_MouseHover(object sender, EventArgs e)
        {
            if (ExamReportColMode_Row_End_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportColMode_Row_End_Combox,
                    ExamReportColMode_Row_End_Combox.SelectedItem.ToString());
            }
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start("https://www.123pan.com/s/JXY5Vv-dLrwd.html"); }

        private void ManW_Click(object sender, EventArgs e)
        {
            ManW.Enabled = false;
            ManW.Text = "正在复活中...";
            Util.PlayMp3WithStream(
                "Man",
                status =>
                {
                    ManW.Enabled = true;
                    ManW.Text = "点击复活牢大";
                });
        }

        private void city_level_1_combox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}