using AutoUpdaterDotNET;
using Gma.System.MouseKeyHook;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json.Linq;
using NPinyin;
using NTwain;
using NTwain.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace DocumentTools
{
    public partial class BodyForm : Form
    {
        public static BodyForm MainForm;
        private FileSystemWatcher TempSystemFileWatcher;
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

        private List<string> TempExamReportItemLists = new List<string>();
        private List<string> TempExamReportItemLists_NoSort = new List<string>();

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

        //AutoCompleteStringCollection MenuStringSource = new AutoCompleteStringCollection();//坤Tools所有菜单的名称，用来给自动建议作为输入源做的

        private List<string> MenuStringOrign = new List<string>();
        private List<string> MenuStringNew = new List<string>();

        private List<string> InsertExam_BatchSelectorCustomLists = new List<string>();

        private int TabMenPagePreIndex = -1;
        private int TabMenPageCurrentIndex = -1;

        private bool IsClickEmojiClick = true;

        private bool RuntimeControl_SendMessage_Status = false;

        private (int PdfWaterCustomPos_TopLeft_X, int PdfWaterCustomPos_TopLeft_Y,
                 int PdfWaterCustomPos_TopCenter_X, int PdfWaterCustomPos_TopCenter_Y,
                 int PdfWaterCustomPos_TopRight_X, int PdfWaterCustomPos_TopRight_Y,
                 int PdfWaterCustomPos_CenterLeft_X, int PdfWaterCustomPos_CenterLeft_Y,
                 int PdfWaterCustomPos_CenterMiddle_X, int PdfWaterCustomPos_CenterMiddle_Y,
                 int PdfWaterCustomPos_CenterRight_X, int PdfWaterCustomPos_CenterRight_Y,
                 int PdfWaterCustomPos_BottomLeft_X, int PdfWaterCustomPos_BottomLeft_Y,
                 int PdfWaterCustomPos_BottomCenter_X, int PdfWaterCustomPos_BottomCenter_Y,
                 int PdfWaterCustomPos_BottomRight_X, int PdfWaterCustomPos_BottomRight_Y) PdfWaterMarkPreCustomPostionValue;

        public int FormWidth = 906;
        public int FormHeight = 872;

        public JArray City_Json;//城市的json字符串数据

        bool TwainInitState;

        bool ExamReportMiddleClickStatus = false;

        private string ChangeReportItemTempHelpPdfFile = string.Empty;
        private string ChangeExamComputerNameHelpPdfFile = string.Empty;
        private string VoteMssTifHelpPdfFile = string.Empty;

        [Obsolete]
        private void SetDpiScalePercentScale()
        {
            float TempDpiX, TempDpiY;

            Graphics TempGraph = this.CreateGraphics();

            TempDpiX = TempGraph.DpiX;

            TempDpiY = TempGraph.DpiY;

            int TempScalePercent = (TempDpiX == 96) ? 100 : (TempDpiX == 120) ? 125 : 150;

            this.Width = this.Width * TempScalePercent / 100;
            this.Height = this.Height * TempScalePercent / 100;
        }

        private void OutPutListViewIndex()
        {
            foreach(TabPage TempPage in this.MainTabPage.TabPages)
            {
                Debug.WriteLine($"{TempPage.Text}：{MainTabPage.TabPages.IndexOf(TempPage)}");
            }
        }

        private void InitMenuNameAutoCompleteSource()
        {
            foreach(TabPage TempPage in this.MainTabPage.TabPages)
            {
                //MenuStringSource.Add(TempPage.Text);
                MenuStringOrign.Add(TempPage.Text);
            }
            this.MenuSearchTextBox.Items.AddRange(MenuStringOrign.ToArray());
        }

        private void GetAdministratorBehaviorStatus()
        {
            string BehaviorStatus = Util.GetAdministratorBehavior();
            if(string.Compare(BehaviorStatus, "1", StringComparison.Ordinal) == 0)
            {
                DisableAdminBehavior.Text = "禁用管理员批准模式";
            } else if(string.Compare(BehaviorStatus, "0", StringComparison.Ordinal) == 0)
            {
                DisableAdminBehavior.Text = "启用管理员批准模式";
            }
        }

        private void SetInitTabPage()
        {
            if(int.TryParse(ConfigurationManager.AppSettings["StartPageIndex"], out int StartIndex))
            {
                MainTabPage.SelectedIndex = StartIndex;
                menu_start_page_index_combox.SelectedIndex = StartIndex;
            }
        }

        private void SetTabCardHeighLight()
        {
            string OpenTabPageHeight = ConfigurationManager.AppSettings["TabPageHeighLight"];

            SetTabPageShowIco();//先对tabpage进行设置

            if(string.Compare(OpenTabPageHeight, "on", StringComparison.Ordinal) == 0)
            {
                this.MainTabPage.DrawMode = TabDrawMode.OwnerDrawFixed;
                menu_tabpage_heigh_light_combox.SelectedIndex = 0;
            } else
            {
                this.MainTabPage.DrawMode = TabDrawMode.Normal;
                menu_tabpage_heigh_light_combox.SelectedIndex = 1;

                if(string.Compare(ShowTabPageIcon, "on", StringComparison.Ordinal) == 0)
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

            foreach(string DisablePageIndex in DisablePageIndexList)
            {
                if(DisablePageIndex != string.Empty)
                {
                    if(int.TryParse(DisablePageIndex, out int PageIndex))
                    {
                        if(PageIndex >= 0 & PageIndex <= MainTabPage.TabCount - 1)
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

            if(int.TryParse(ListViewLineHeight, out int LineHeight) && LineHeight > 0 && LineHeight <= 256)
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
            if(int.TryParse(ConfigurationManager.AppSettings["FormWidth"], out int width) &&
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

            foreach(TabPage TempPage in MainTabPage.TabPages)
            {
                foreach(Control TempControl in TempPage.Controls)
                {
                    if(TempControl is GroupBox)
                    {
                        foreach(Control control in TempControl.Controls)
                        {
                            switch(control.Tag)
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
                    switch(TempControl.Tag)
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
            if(File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
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
            votedataview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            votedataview.MultiSelect = false;

            InsertExamTableView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            InsertExamTableView.MultiSelect = false;
            InsertExamTableView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            InsertExamTableView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            InsertExamTableView.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            InsertExamTableView.AllowUserToDeleteRows = false;
            InsertExamTableView.AllowUserToAddRows = false;
            InsertExamTableView.AllowUserToOrderColumns = false;
            InsertExamTableView.RowHeadersVisible = true;
            InsertExamTableView.ReadOnly = true;
            InsertExamTableView.Font = new System.Drawing.Font("宋体", 9);
            InsertExamTableView.BackgroundColor = Color.AliceBlue;

            ExamDataPakOrder_UnitListView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            ExamDataPakOrder_UnitListView.MultiSelect = false;
            ExamDataPakOrder_UnitListView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ExamDataPakOrder_UnitListView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ExamDataPakOrder_UnitListView.RowsDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ExamDataPakOrder_UnitListView.AllowUserToDeleteRows = false;
            ExamDataPakOrder_UnitListView.AllowUserToAddRows = false;
            ExamDataPakOrder_UnitListView.AllowUserToOrderColumns = false;
            ExamDataPakOrder_UnitListView.RowHeadersVisible = false;
            ExamDataPakOrder_UnitListView.ReadOnly = true;
            ExamDataPakOrder_UnitListView.Font = new System.Drawing.Font("宋体", 12);
            ExamDataPakOrder_UnitListView.BackgroundColor = Color.AliceBlue;
        }


        public BodyForm()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            MainForm = this;

            money_small.Enabled = !IsClickEmojiClick;
            money_big.Enabled = !IsClickEmojiClick;
            //this.AutoScaleMode = AutoScaleMode.Font;
        }

        private void UpdateCheckEvent(UpdateInfoEventArgs updateArgs)
        {
            if(updateArgs.Error == null && updateArgs.IsUpdateAvailable)
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
            //判断程序是否可以打开
            Task.Run(async () => await GetApplicationStartLimit());

            //判断内部版本
            //Util.paint_1();
            AutoUpdater.InstalledVersion = new Version(VersionInfo.VersionNumber);
            AutoUpdater.Start(VersionInfo.VersionUpdateCheckUrl);
            AutoUpdater.CheckForUpdateEvent += UpdateCheckEvent;


            //初始化
            this.Text = VersionInfo.VersionTitle;
            this.UpdateCheckText.Text = $"当前版本为：{VersionInfo.VersionNumber}，没有检测到新版本";
            this.AutoSize = false;
            this.MainNotify.Text = VersionInfo.VersionTitle;
            City_Json = JArray.Parse((string)Util.GetEmbedResourceObj("china_level"));
            InitDataGridView();
            InitComboxValue();
            InitConfigSettings();
            //SetDpiScalePercentScale();
            InitListView();
            InitMoveItemButtonHandler();
            InitRemoveItemHandler();
            InitToolTipHandler();
            //InitConfigSettings();
            InitTabPageIcon();
            OutPutListViewIndex();

            //设置功能搜索直达的自动完成数据源
            InitMenuNameAutoCompleteSource();
            //this.MenuSearchTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            //this.MenuSearchTextBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
            //this.MenuSearchTextBox.AutoCompleteCustomSource = MenuStringSource;
            this.MenuSearchTextBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            this.MenuSearchTextBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            //this.MenuSearchTextBox.MaxDropDownItems = MainTabPage.TabPages.Count;
            //this.MenuSearchTextBox.AutoCompleteCustomSource = MenuStringSource;
            if(string.Compare(WidgetAnchorMode, "on", StringComparison.Ordinal) == 0)
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

            this.Excel2Pdf_PrintScale_Row.Validating += ValidTextBoxContent;
            this.Excel2Pdf_PrintScale_Row.Validated += ValidedTextBoxContent;
            this.Excel2Pdf_PrintScale_Col.Validating += ValidTextBoxContent;
            this.Excel2Pdf_PrintScale_Col.Validated += ValidedTextBoxContent;

            this.DecodeDataMatrixView.MouseClick += (s1, s2) =>
            {
                if(s2.Button == MouseButtons.Right)
                {
                    if(s1 is TreeView TempTreeView)
                    {
                        TreeViewContentMenu.Show(TempTreeView, s2.Location);
                    }
                }
            };

            //从仓库获取按钮状态，如果网络失败或者其他问题，默认隐藏按钮
            Task.Run(async () => await SetLimitControl());
            //AutoSize = new AutoAdaptWindowsSize(this);

            DynamicAddControlParams.TempBaseInsertItemLocationY = InsertExam_ExamName.Location.Y;

            DynamicAddControlParams.TempBaseInsertItemSelectorLocationY = InsertExam_ExamSelector.Location.Y;
        }


        //初始化Listview
        private void InitListView()
        {
            //初始化所有listview

            GetFormMembers(typeof(ListView))
                .ForEach(
                    x =>
                    {
                        ListView TempListView = x as ListView;
                        TempListView.View = View.Details;
                        TempListView.FullRowSelect = true;
                        TempListView.AllowDrop = true;
                        TempListView.ShowItemToolTips = true;
                        TempListView.MultiSelect = false;
                        TempListView.GridLines = string.Compare(ListviewShowGridLines, "on", StringComparison.Ordinal) ==
                            0;
                        TempListView.SmallImageList = TempImageList;
                        TempListView.Name = TempListView.Name.ToString();

                        if(string.Compare(TempListView.Name, "VFSOFT_Report_ListView", StringComparison.Ordinal) == 0)
                        {
                            TempListView.Columns.Add("报表ID", 330);
                            TempListView.Columns.Add("报表名称", 330);
                            TempListView.Columns.Add("所属考评表", 330);
                            TempListView.Columns.Add("考评表ID", 330);
                        } else
                        {
                            TempListView.Columns.Add("文件名称", 300, HorizontalAlignment.Left);
                            TempListView.Columns.Add("添加时间", 300, HorizontalAlignment.Left);
                            TempListView.Columns.Add("转换后文件名称", 300, HorizontalAlignment.Left);
                            TempListView.Columns.Add("转换后文件大小", 300, HorizontalAlignment.Left);

                            TempListView.DragEnter += ListViewDragEnter;
                            TempListView.DragDrop += ListviewDragDrop;
                            //TempListView.DragOver += ListViewDragOver;
                            //TempListView.ItemDrag += ListviewItemDrag;


                            TempListView.MouseClick += (MouseSender, MouseEvent) =>
                            {
                                if(MouseEvent.Button == MouseButtons.Right)
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
            CompressImage_OutPutFormat.Rows.Add("TIF", ".tif");

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

            foreach(TabPage tempPage in MainTabPage.TabPages)
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


            List<string> ExamDataFormatList = new List<string> { "该人得票数", "该人得票率", "该人得分", "该人得分率", "该人得票数|该人得票率" };

            this.ExamReplaceDataFormat.DataSource = ExamDataFormatList.Take(ExamDataFormatList.Count - 1).ToList();
            
            this.ExamReportParaDataFormat_Combox.DataSource = new List<string> (ExamDataFormatList);


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

            this.Exam_Picture_ThreadNumbers.Items.Add("自动");
            this.Exam_Picture_ThreadNumbers.Items
                .AddRange(
                    Enumerable.Range(1, Environment.ProcessorCount * 2)
                        .Where(x => x % 8 == 0 || x == 1)
                        .Select(x => (object)x)
                        .ToArray());
            this.Exam_Picture_ThreadNumbers.SelectedIndex = 0;

            this.MenuSearchMode.Items.AddRange(new string[] { "精确匹配模式", "模糊匹配模式" });
            this.MenuSearchMode.SelectedIndex = 0;

            this.GetChinsesHanPinYin_TypeBox.DataSource = new string[]
            {
                "大写拼音首字母",
                "小写拼音首字母",
                "大写拼音（带空格）",
                "大写拼音（不带空格）",
                "小写拼音（带空格）",
                "小写拼音（不带空格）",
            };

            InserExamBaseInfo_FlowClassCombox.DataSource = new string[] { "人员纵排", "人员纵排2", "人员横排", "单人标识" };


            //加载城市数据到combox中
            this.city_level_1_combox.Items.Clear();//先清空数据
            if(City_Json != null)
            {
                foreach(JToken City_1_Name in City_Json)
                {
                    city_level_1_combox.Items.Add(City_1_Name["name"]);//添加省级信息
                }
                city_level_1_combox.SelectedIndex = 0;
            }


            //修改下拉样式
            GetFormMembers(typeof(ComboBox))
                .ForEach(
                    x =>
                    {
                        ComboBox TempCombox = x as ComboBox;
                        TempCombox.DropDownStyle = ComboBoxStyle.DropDownList;
                    });

            GetFormMembers(typeof(ToolStripComboBox))
                .ForEach(
                    x =>
                    {
                        ToolStripComboBox TempCombox = x as ToolStripComboBox;
                        if(TempCombox.Name != "MenuSearchTextBox")
                        {
                            TempCombox.DropDownStyle = ComboBoxStyle.DropDownList;
                        }
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
                        if(x.FieldType == controlType)
                        {
                            object TempObj = x.GetValue(this);

                            if(TempObj != null)
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
            GetFormMembers(typeof(Button))
                .ForEach(
                    x =>
                    {
                        Button TempButton = x as Button;
                        string[] SplitButtonName = TempButton.Name.Split('_');

                        if(SplitButtonName.Length == 2)//只有确实分割了才处理
                        {
                            GetFormMembers(typeof(ListView))
                                .ForEach(
                                    listview =>
                                    {
                                        ListView TempListView = listview as ListView;
                                        if(SplitButtonName[1].Contains("MoveUp") &&
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
                                        } else if(SplitButtonName[1].Contains("MoveDown") &&
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
            GetFormMembers(typeof(Button))
                .ForEach(
                    member =>
                    {
                        Button TempButton = member as Button;

                        if(TempButton.Name.Contains("_clear"))
                        {
                            MainToolsTip.SetToolTip(TempButton, "清空列表中所有文件");
                        } else if(TempButton.Name.Contains("_remove"))
                        {
                            MainToolsTip.SetToolTip(TempButton, "移除选中文件");
                        } else if(TempButton.Name.Contains("MoveDown"))
                        {
                            MainToolsTip.SetToolTip(TempButton, "向下移动选中文件");
                        } else if(TempButton.Name.Contains("MoveUp"))
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
                        if(member is TabPage TempPage)
                        {
                            string TabPageCaption = TempPage.Text;
                            if(TabPageCaption.StartsWith("WORD转"))
                            {
                                TempPage.ImageIndex = 0;
                            } else if(TabPageCaption.StartsWith("PDF"))
                            {
                                TempPage.ImageIndex = 1;
                            } else if(TabPageCaption.StartsWith("EXCEL转"))
                            {
                                TempPage.ImageIndex = 2;
                            } else if(TabPageCaption.StartsWith("PPT转"))
                            {
                                TempPage.ImageIndex = 3;
                            } else if(TabPageCaption.StartsWith("WORD内"))
                            {
                                TempPage.ImageIndex = 0;
                            } else if(TabPageCaption.StartsWith("EXCEL内"))
                            {
                                TempPage.ImageIndex = 1;
                            } else if(TabPageCaption.StartsWith("EPUB"))
                            {
                                TempPage.ImageIndex = 4;
                            } else if(TabPageCaption.StartsWith("压缩"))
                            {
                                TempPage.ImageIndex = 7;
                            } else if(TabPageCaption.Contains("图片"))
                            {
                                TempPage.ImageIndex = 5;
                            } else if(TabPageCaption.EndsWith("水印"))
                            {
                                TempPage.ImageIndex = 13;
                            } else if(TabPageCaption.EndsWith("WORD"))
                            {
                                TempPage.ImageIndex = 0;
                            } else if(TabPageCaption.EndsWith("PDF"))
                            {
                                TempPage.ImageIndex = 1;
                            } else if(TabPageCaption.StartsWith("特殊"))
                            {
                                TempPage.ImageIndex = 8;
                            } else if(TabPageCaption.StartsWith("Twain"))
                            {
                                TempPage.ImageIndex = 9;
                            } else if(TabPageCaption.StartsWith("其他"))
                            {
                                TempPage.ImageIndex = 10;
                            } else if(TabPageCaption.StartsWith("系统"))
                            {
                                TempPage.ImageIndex = 11;
                            } else if(TabPageCaption.StartsWith("关于"))
                            {
                                TempPage.ImageIndex = 12;
                            } else if(TabPageCaption.StartsWith("姓名"))
                            {
                                TempPage.ImageIndex = 14;
                            } else if(TabPageCaption.StartsWith("MSS"))
                            {
                                TempPage.ImageIndex = 11;
                            } else if(TabPageCaption.StartsWith("简易"))
                            {
                                TempPage.ImageIndex = 15;
                            } else if(TabPageCaption.Contains("身份证"))
                            {
                                TempPage.ImageIndex = 16;
                            } else if(TabPageCaption.Contains("补充"))
                            {
                                TempPage.ImageIndex = 17;
                            } else if(TabPageCaption.Contains("网卡设置"))
                            {
                                TempPage.ImageIndex = 18;
                            } else if(TabPageCaption.Contains("选举结果报告单样式生成"))
                            {
                                TempPage.ImageIndex = 19;
                            } else if(TabPageCaption.Contains("单人表"))
                            {
                                TempPage.ImageIndex = 20;
                            } else if(TabPageCaption.Contains("考评报表"))
                            {
                                TempPage.ImageIndex = 21;
                            } else if(TabPageCaption.Contains("拆分PPT"))
                            {
                                TempPage.ImageIndex = 3;
                            } else if(TabPageCaption.Contains("考评表信息插入"))
                            {
                                TempPage.ImageIndex = 22;
                            }
                            else if (TabPageCaption.Contains("考评系统数据包"))
                            {
                                TempPage.ImageIndex = 22;
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
                            sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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

                if(this.TargetTextBox.Text != string.Empty)
                {
                    if(DoubleTextAlignCheck.Checked)
                    {
                        string DoubleText;
                        foreach(string TargetTextTemp in this.TargetTextBox.Lines)
                        {
                            DoubleText = TargetTextTemp;
                            if(DoubleText.Length == 2)
                            {
                                DoubleText = DoubleText.Insert(1, "  ");
                            }
                            TempLines.Add(DoubleText);//把不需要对齐的也一起加回来
                        }
                        TargetTextBox.Lines = TempLines.ToArray();
                    }

                    if(GetChinsesHanPinYin.Checked)
                    {
                        List<string> PinYingLists = new List<string>();

                        switch(this.GetChinsesHanPinYin_TypeBox.SelectedIndex)
                        {
                            case 0://首字母大写
                                PinYingLists = this.TargetTextBox.Lines
                                    .Select(line => Pinyin.GetInitials(line).ToUpper())
                                    .ToList();
                                break;
                            case 1://首字母小写
                                PinYingLists = this.TargetTextBox.Lines
                                    .Select(line => Pinyin.GetInitials(line).ToLower())
                                    .ToList();
                                break;
                            case 2://拼音大写（空格）
                                PinYingLists = this.TargetTextBox.Lines
                                    .Select(line => Pinyin.GetPinyin(line).ToUpper())
                                    .ToList();
                                break;
                            case 3://拼音大写（无空格）
                                PinYingLists = this.TargetTextBox.Lines
                                    .Select(
                                        line => Regex.Replace(Pinyin.GetPinyin(line).ToUpper(), @"\s+", string.Empty))
                                    .ToList();
                                break;
                            case 4://拼音小写（空格）
                                PinYingLists = this.TargetTextBox.Lines
                                    .Select(line => Pinyin.GetPinyin(line).ToLower())
                                    .ToList();
                                break;
                            case 5://拼音小写（无空格）
                                PinYingLists = this.TargetTextBox.Lines
                                    .Select(
                                        line => Regex.Replace(Pinyin.GetPinyin(line).ToLower(), @"\s+", string.Empty))
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                        this.TargetTextBox.Lines = PinYingLists.ToArray();
                    }

                    if(NameAutoCopy.Checked && TargetTextBox.Text != string.Empty)
                    {
                        Clipboard.SetText(TargetTextBox.Text.Replace("\n", Environment.NewLine));//替换换行
                        NameAutoCopyStatusText.Text = "[已复制到剪贴板]";
                        NameAutoCopyStatusText.ForeColor = Color.ForestGreen;
                    } else
                    {
                        NameAutoCopyStatusText.Text = string.Empty;
                    }

                    if(MarkDupName.Checked)
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
                                for(int i = 0; i < TargetTextBox.Lines.Length; i++)
                                {
                                    string currentLine = TargetTextBox.Lines[i];

                                    // 在当前行之后的行中查找是否存在相同的文本行
                                    for(int j = i + 1; j < TargetTextBox.Lines.Length; j++)
                                    {
                                        if(currentLine.Equals(TargetTextBox.Lines[j]))
                                        {
                                            // 记录重复行的索引
                                            duplicateLineIndices.Add(i);
                                            duplicateLineIndices.Add(j);
                                        }
                                    }
                                }

                                if(duplicateLineIndices.Count > 0)
                                {
                                    // 高亮显示所有重复的文本行
                                    foreach(int index in duplicateLineIndices)
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

                    if(DoubleTextClear.Checked)
                    {
                        this.TargetTextBox.Lines = this.TargetTextBox.Lines.ToList().Distinct().ToArray();
                    }
                }
            } catch(Exception ex)
            {
                this.BeginInvoke(
                    new Action(
                        () =>
                        {
                            (sender as Button).Enabled = true;
                            MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
            }
        }

        private void SourceTextBox_TextChanged(object sender, EventArgs e)
        {
            //如果启用多行模式，则每次添加文本后，都在末尾添加\t控制字符
            if(MutilLineMode.Checked)
            {
                SourceTextBox.AppendText("\t");
            }
        }

        private void UpdateNameIndex()
        {
            Point pt = new Point(0, 0);
            int firstIndex = TargetTextBox.GetCharIndexFromPosition(pt);
            int firstLine = TargetTextBox.GetLineFromCharIndex(firstIndex);

            pt.Y += TargetTextBox.Height;
            int lastIndex = TargetTextBox.GetCharIndexFromPosition(pt);
            int lastLine = TargetTextBox.GetLineFromCharIndex(lastIndex);

            TargetBoxIndex_number.Text = string.Empty;
            for(int i = firstLine; i < lastLine + 1; i++)
            {
                TargetBoxIndex_number.Text += (i + 1) + "\r\n";
            }
        }

        private void TargetTextBox_TextChanged(object sender, EventArgs e)
        {
            TargetTextNumberText.Text = $"处理后姓名有{TargetTextBox.Lines.Length}个";
            UpdateNameIndex();
        }

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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void MeregePdfFileButtonConvert_Click(object sender, EventArgs e)
        {
            Task<List<string>> MeregePdfFiles = UI.GetListViewFirstItemContentAsync(this.MergePdfFileView);
            if(MeregePdfFiles.Result.Count > 0)
            {
                string MeregePdfFileName = Util.ShowSaveFileDialog("PDF文档|.pdf", "设置文件名称");
                if(MeregePdfFileName != string.Empty)
                {
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                (sender as Button).Enabled = false;
                                string MeregeDirectory = System.IO.Path.GetDirectoryName(MeregePdfFileName);
                                this.MeregePdfFileStatusText.Text = "正在合并中...";
                                Util.MergerPdfFIle(MeregePdfFiles.Result, MeregePdfFileName);
                                this.MeregePdfFileStatusText.Text = "合并完毕，右键名称可移除文件";
                                (sender as Button).Enabled = true;
                                if(OpenMeregePdfFile.Checked)
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
                                            if(OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(MeregeDirectory);
                                            }
                                        }));
                            } catch(Exception ex)
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
                                            (sender as Button).Enabled = true;
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
            if(ListViewRightMenu.SourceControl.GetType() == typeof(ListView))
            {
                ListView TempListView = ListViewRightMenu.SourceControl as ListView;
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
            if(ListViewRightMenu.SourceControl.GetType() == typeof(ListView))
            {
                ListView TempListView = ListViewRightMenu.SourceControl as ListView;
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
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            } else if(e.Data.GetDataPresent(typeof(System.Windows.Forms.ListViewItem)))
            {
                e.Effect = DragDropEffects.Move;
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

            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] DropFiles = e.Data.GetData(DataFormats.FileDrop, false) as string[];
                switch(DragedListview.Name)
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
                        UI.AddFilesToListViewWithNotDialog(
                            DropFiles,
                            this.PdfWaterMarkFileList,
                            new string[] { ".pdf" });
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
                            new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".svg" });
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
            } else if(e.Data.GetDataPresent(typeof(System.Windows.Forms.ListViewItem)))
            {
                // 获取目标位置
                Point cp = DragedListview.PointToClient(new Point(e.X, e.Y));
                ListViewItem targetItem = DragedListview.GetItemAt(cp.X, cp.Y);
                int targetIndex = targetItem?.Index ?? DragedListview.Items.Count - 1;

                // 获取拖动的项目
                ListViewItem[] draggedItems = (ListViewItem[])e.Data.GetData("ListViewItemArray");

                // 处理项目交换逻辑
                if(draggedItems != null)
                {
                    // 将拖动的项目从原位置移除
                    foreach(ListViewItem item in draggedItems)
                    {
                        DragedListview.Items.Remove(item);
                    }

                    // 在目标位置插入项目
                    for(int i = 0; i < draggedItems.Length; i++)
                    {
                        DragedListview.Items.Insert(targetIndex + i, draggedItems[i]);
                    }
                }
            }
        }

        [Obsolete]
        private void ListviewItemDrag(object sender, ItemDragEventArgs e)
        {
            ListView TempListView = sender as ListView;
            //TempListView.DoDragDrop(e.Item, DragDropEffects.Move);

            // 开始拖动操作
            ListViewItem[] selectedItems = TempListView.SelectedItems.Cast<ListViewItem>().ToArray();
            TempListView.DoDragDrop(new DataObject("ListViewItemArray", selectedItems), DragDropEffects.Move);
        }

        private void ListViewDragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
            //ListView TempListView = sender as ListView;
            //Point TempPoint = new Point(e.X, e.Y);
            //Point TempListViewPoint = TempListView.PointToClient(TempPoint);

            //ListViewItem TempItem = TempListView.GetItemAt(TempListViewPoint.X, TempListViewPoint.Y);

            //if(TempItem != null)
            //{
            //    TempItem.Selected = true;
            //}
        }

        private void DownloadFrameWorkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_Download_Net48").ToString()); }

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
                if(ScanSeconds > 0)
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
            if(dataSource != null)
            {
                if(dataSource.Open() == ReturnCode.Success)
                {
                    if(dataSource.Capabilities.CapPowerSaveTime.IsSupported)
                    {
                        TwainSleepTimeSet.DataSource = dataSource.Capabilities.CapPowerSaveTime.GetValues().ToList();
                        TwainSleepTimeSet.SelectedItem = dataSource.Capabilities.CapPowerSaveTime.GetCurrent();
                        TwainSetDeviceSaveTime.Enabled = true;
                    } else
                    {
                        TwainSleepTimeSet.DataSource = new string[] { "不支持" };
                        TwainSetDeviceSaveTime.Enabled = false;
                    }
                    if(dataSource.Capabilities.ICapXResolution.IsSupported ||
                        dataSource.Capabilities.ICapYResolution.IsSupported)
                    {
                        TwainDpi.DataSource = dataSource.Capabilities.ICapXResolution.GetValues().ToList();
                        TwainDpi.SelectedItem = dataSource.Capabilities.ICapXResolution.GetCurrent();
                    }
                    if(dataSource.Capabilities.ICapSupportedSizes.IsSupported)
                    {
                        TwainPaperSizeBox.DataSource = dataSource.Capabilities.ICapSupportedSizes.GetValues().ToList();
                        TwainPaperSizeBox.SelectedItem = dataSource.Capabilities.ICapSupportedSizes.GetCurrent();
                    }
                    if(dataSource.Capabilities.CapSerialNumber.CanGet)
                    {
                        ScanSerialNumberTextBox.Text = $"{dataSource.Capabilities.CapSerialNumber.GetValues().FirstOrDefault()}";
                    } else
                    {
                        ScanSerialNumberTextBox.Text = $"无";
                    }
                    if(this.ExportCaps.Checked)
                    {
                        //输出扫描仪支持的设置参数
                        string DebugName = $"{dataSource.Name}_Support_Capabilities.txt";
                        if(File.Exists(DebugName))
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

            if(dataSource?.Open() == ReturnCode.Success)
            {
                dataSource?.Enable(SourceEnableMode.ShowUIOnly, false, IntPtr.Zero);
            }
        }

        private void StartScanButton_Click(object sender, EventArgs e)
        {
            if(this.ScanPictureSavePathBox.Text != string.Empty)
            {
                TempImageCount = 0;
                ScanSeconds = 0;
                dataSource ??= twainSession.DefaultSource;
                ImageIndex = Directory.GetFiles(this.ScanPictureSavePathBox.Text).Length;

                if(dataSource.Open() == ReturnCode.Success)
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
                    if(dataSource.Capabilities.ICapXResolution.CanSet && dataSource.Capabilities.ICapYResolution.CanSet)
                    {
                        if(TwainDpi.Items.Count > 0)
                        {
                            dataSource.Capabilities.ICapXResolution.SetValue((TWFix32)TwainDpi.SelectedItem);
                            dataSource.Capabilities.ICapYResolution.SetValue((TWFix32)TwainDpi.SelectedItem);
                        }
                    }
                    if(DoubleScanBox.Checked && dataSource.Capabilities.CapDuplex.IsSupported)
                    {
                        dataSource.Capabilities.CapDuplexEnabled.SetValue(BoolType.True);
                    }
                    if(SkipBlankPage.Checked && dataSource.Capabilities.ICapAutoDiscardBlankPages.IsSupported)
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
                    if(TwainScanNumberSet.Text != string.Empty &&
                        Int32.TryParse(TwainScanNumberSet.Text, out int TwainScanNumberSetInt))
                    {
                        if(dataSource.Capabilities.CapXferCount.CanSet)
                        {
                            if(TwainScanNumberSetInt > 0)
                            {
                                //对于支持双面扫描的扫描仪，先把扫描模式设置为单面扫描
                                if(dataSource.Capabilities.CapDuplex.IsSupported && !(DoubleScanBox.Checked))
                                {
                                    dataSource.Capabilities.CapDuplexEnabled.SetValue(BoolType.False);
                                }
                                dataSource.Capabilities.CapXferCount.SetValue(TwainScanNumberSetInt);
                            } else
                            {
                                dataSource.Capabilities.CapXferCount.SetValue(-1);
                            }
                        }
                    }
                    if(UseInterfaceUi.Checked)
                    {
                        (sender as Button).Enabled = false;
                        dataSource.Enable(SourceEnableMode.ShowUI, false, IntPtr.Zero);
                        (sender as Button).Enabled = true;
                        ScanStopWatch = new Stopwatch();
                    } else
                    {
                        if(dataSource.Capabilities.CapUIControllable.IsSupported)
                        {
                            (sender as Button).Enabled = false;
                            dataSource.Enable(SourceEnableMode.NoUI, false, IntPtr.Zero);
                            (sender as Button).Enabled = true;
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
            if(this.ScanPictureSavePathBox.Text != string.Empty)
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
            if(string.Compare(DirectoryString, default, StringComparison.Ordinal) != 0)
            {
                this.ScanPictureSavePathBox.Text = DirectoryString;
                this.SavedPathFileCount.Text = $"扫描路径内文件数量：{Directory.GetFiles(DirectoryString).Length}";
            }
        }

        private async void MemoryTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var MemoryInfo = Util.GetProcessUsedMemory();

                double MemoryUsedPercent = Math.Round((Util.Byte2GB((long)MemoryInfo.PUM) / MemoryInfo.MTM) * 100.0, 2);

                this.Invoke(
                    new Action(
                        () =>
                        {
                            this.MemoryStatusLabel.Text = $"占用内存：{MemoryInfo.PUM / 1024.0 / 1024.0:f2}MB|可用内存：{MemoryInfo.MFM:f2}GB|总内存：{MemoryInfo.MTM:f2}GB|程序占用比例（{MemoryUsedPercent}%）";

                            this.MemoryProgressBar.Maximum = (int)MemoryInfo.MTM;
                            this.MemoryProgressBar.Value = (int)(Util.Byte2GB((long)MemoryInfo.PUM));

                            this.MemoryProgressBar.ToolTipText = $"程序占用比例：{MemoryUsedPercent}%";
                        }));

                await GetOnlineApplicationCMD();
            } catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void RemoveAllFiles_Click(object sender, EventArgs e)
        {
            if(ScanPictureSavePathBox.Text != string.Empty)
            {
                string[] Files = Directory.GetFiles(ScanPictureSavePathBox.Text);
                if(Files.Length > 0)
                {
                    //删除文件之前清除全部的bitmap对象
                    if(TempScannedImageBmpList.Count > 0)
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
                    foreach(string file in Files)
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
                            CallButton: sender as Button,
                            UseSourcePath: WordTextReplace_SaveSourceBox.Checked);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
            TreeNode CurrentSelectNode = this.DecodeDataMatrixView.SelectedNode;
            CurrentSelectNode.ExpandAll();
        }

        private void 展开折叠全部节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(TreeNode TempNode in DecodeDataMatrixView.Nodes)
            {
                TempNode.ExpandAll();
            }
        }

        private void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(File.Exists(this.DecodeDataMatrixView.SelectedNode.Text))
                Process.Start(this.DecodeDataMatrixView.SelectedNode.Text);
        }

        private void DownloadNetfx35_Click(object sender, EventArgs e)
        {
            string Netfx35_Private = "http://jdsnmsl.tpddns.cn:3002/directlink/default/Windows%E8%BD%AF%E4%BB%B6/GorunTime%E8%BF%90%E8%A1%8C%E5%BA%93%E5%90%88%E9%9B%86/GoRuntime_DotNetFramework_3.x.zip";
            //string Netfx35_Private = "https://i-920.lanzouc.com:446/11271300149682909bb/2023/11/27/5404769fd0f075f33072184f6765a12b.zip?st=tf2aLH7S1zIferEFhUiCfA&e=1701063569&b=UkddMlACVCRYY1NxBj9UPQMzCAsNSAFoBiEISwVnVyFSRgh6BmcDawIyU3IGOQZwBDoKCVBiACgFLlx_aUSxSOVJw&fi=149682909&pid=117-81-188-214&up=2&mp=0&co=0";
            Util.DownloadFile(Netfx35_Private, "Net35.zip", Downloadfx35Progress, ProcessLabel);
        }

        private void LanzouNetfx_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_NetFx35").ToString()); }

        private void ReleaseRegistry_Click(object sender, EventArgs e)
        {
            string SaveFileName = Util.ShowSaveFileDialog("注册表文件|*.reg");

            if(SaveFileName != string.Empty)
            {
                object RegistryString = Util.GetEmbedResourceObj("注册");

                File.WriteAllText(SaveFileName, RegistryString.ToString(), Encoding.Default);

                MessageBox.Show("释放完成，请用管理员身份运行本文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void Label9_DoubleClick(object sender, EventArgs e)
        {
            if(ReleaseRegistry.Enabled)
            {
                ReleaseRegistry.Enabled = false;
            } else
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void BodyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(ChangeReportItemTempHelpPdfFile != string.Empty)
            {
                File.Delete(ChangeReportItemTempHelpPdfFile);
            }

            if(ChangeExamComputerNameHelpPdfFile != string.Empty)
            {
                File.Delete(ChangeExamComputerNameHelpPdfFile);
            }

            if(VoteMssTifHelpPdfFile != string.Empty)
            {
                File.Delete(VoteMssTifHelpPdfFile);
            }
            MainNotify.Visible = false;
            MainNotify.Icon = null;
            MainNotify.Dispose();

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
        { Process.Start(Util.GetEmbedResourceObj("Link_52Pojie").ToString()); }


        private void SelectMergeWordFiles_Click(object sender, EventArgs e) { UI.AddMergeWordFiles(); }

        private void MergeWordFile_Click(object sender, EventArgs e)
        {
            Task<List<string>> MergeWordFiles = UI.GetListViewFirstItemContentAsync(this.MergeWordFileList);
            if(MergeWordFiles.Result.Count > 0)
            {
                string MergeWordFileName = Util.ShowSaveFileDialog("WORD文档|.docx", "设置文件名称");
                if(MergeWordFileName != string.Empty)
                {
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                (sender as Button).Enabled = false;
                                string MeregeDirectory = System.IO.Path.GetDirectoryName(MergeWordFileName);
                                this.MeregeWordStatusText.Text = "正在合并中...";
                                Util.MergeWordFile(MergeWordFiles.Result, MergeWordFileName);
                                this.MeregeWordStatusText.Text = "合并完毕，右键名称可移除文件";
                                (sender as Button).Enabled = true;
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            DialogResult OpenResult = MessageBox.Show(
                                                "合并完毕，是否需要打开合并文件所在目录？",
                                                "提示",
                                                MessageBoxButtons.YesNo,
                                                MessageBoxIcon.Asterisk);
                                            if(OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(MeregeDirectory);
                                            }
                                        }));
                            } catch(Exception ex)
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
                                            (sender as Button).Enabled = true;
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
                            sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectWaterMarkPdfFiles_Click(object sender, EventArgs e) { UI.AddWaterMarkPdfFiles(); }


        private void PDFWaterMarkTextFontSetting_Click(object sender, EventArgs e)
        {
            DialogResult FontSelectDialogResult = FontSelectDialog.ShowDialog();

            if(FontSelectDialogResult == DialogResult.OK)
            {
                PrivateFontSetting.FontName = FontSelectDialog.Font.FontFamily.GetName(1033);
                PrivateFontSetting.FontSize = FontSelectDialog.Font.Size;
            }
        }

        private void PdfWaterMarkFontColorSettings_Click(object sender, EventArgs e)
        {
            DialogResult ColorSelectDialogResult = ColorSelectDialog.ShowDialog();

            if(ColorSelectDialogResult == DialogResult.OK)
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
                        if((!string.IsNullOrEmpty(PdfWaterMarkText.Text)||AddPdfWaterMark_UseFileNameAsMark.Checked) ||
                            !string.IsNullOrEmpty(AddWaterMarkBackGroundImageFileName))
                        {
                            UI.ConvertStart(
                                this.PdfWaterMarkFileList,
                                this.AddWaterMark2PdfStatusText,
                                ConvertFileType.AddWaterMarkPdf,
                                ".pdf",
                                CallButton: sender as Button);
                        }
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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

            if(string.Compare(BackGroundImageFileNames.FirstOrDefault(), default, StringComparison.Ordinal) != 0)
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
            TextBox TempTextBox = (TextBox)sender;

            string TempText = TempTextBox.Text;

            if(!IsNumber(TempText))
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


            if(e.Index == this.MainTabPage.SelectedIndex)
            {
                PageFont = new System.Drawing.Font(e.Font, FontStyle.Regular);
                BackBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    e.Bounds,
                    SystemColors.Control,
                    SystemColors.Control,
                    System.Drawing.Drawing2D.LinearGradientMode.BackwardDiagonal);

                ForeBrush = Brushes.Red;
            } else
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void ListviewSortHandler(object sender, ColumnClickEventArgs e)
        {
            bool IsAscing = true;
            //自定义比较器进行排序

            if(e.Column == 0)
            {
                ListView TempListview = sender as ListView;

                if(IsAscing)
                {
                    TempListview.ListViewItemSorter = new ListViewItemComparer_DSC();
                    IsAscing = false;
                } else
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectConvertImage2PDF_Click(object sender, EventArgs e) { UI.AddConvertImage2PDFFiles(); }

        private void ConvertImage2PDF_Click(object sender, EventArgs e)
        {
            Task<List<string>> ImageFiles = UI.GetListViewFirstItemContentAsync(this.ConvertImage2PDFFileList);
            if(ImageFiles.Result.Count > 0)
            {
                if(ExportSignlePDF.Checked)
                {
                    IEnumerable<string> SavePaths = Util.ShowSelectDialog(true, true, string.Empty, string.Empty);
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                string SavePath = SavePaths.FirstOrDefault();
                                if(SavePath != string.Empty ||
                                    string.Compare(SavePath, default, StringComparison.Ordinal) != 0)
                                {
                                    if(SavePath != null)
                                    {
                                        (sender as Button).Enabled = false;
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
                                                    (sender as Button).Enabled = true;
                                                    if(OpenResult == DialogResult.Yes)
                                                    {
                                                        Process.Start(SavePath);
                                                    }
                                                }));
                                    }
                                }
                            } catch(Exception ex)
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
                                            (sender as Button).Enabled = true;
                                        }));
                            }
                        });
                } else
                {
                    string MeregePdfFileName = Util.ShowSaveFileDialog("PDF文档|.pdf", "设置文件名称");
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                if(MeregePdfFileName != string.Empty)
                                {
                                    (sender as Button).Enabled = false;
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
                                                (sender as Button).Enabled = true;
                                                DialogResult OpenResult = MessageBox.Show(
                                                    "合并完毕，是否需要打开合并文件所在目录？",
                                                    "提示",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Asterisk);
                                                if(OpenResult == DialogResult.Yes)
                                                {
                                                    Process.Start(Path.GetDirectoryName(MeregePdfFileName));
                                                }
                                            }));
                                }
                            } catch(Exception ex)
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
                                            (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void CompressImage_NotUseSelfExt_CheckedChanged(object sender, EventArgs e)
        {
            this.CompressImage_OutPutImageFormat.Enabled = this.CompressImage_NotUseSelfExt.Checked ||
                this.CompressImage_NotUseSelfExt.Checked;
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
            if(NameReplaceMode)
            {
                NameReplaceMode = false;
                NameReplaceRegexMode.Text = "[正则模式：关]";
                NameReplaceRegexMode.ForeColor = Color.Red;
            } else
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
                if(Util.IsAdministrator())
                {
                    Util.RemoveShort();
                    ShortStatus.Text = "已移除";
                    ShortStatus.ForeColor = Color.Red;
                    if(AutoRestartExplorer.Checked)
                    {
                        Process.GetProcesses()
                            .ToList()
                            .ForEach(
                                p =>
                                {
                                    if(string.Compare(p.ProcessName, "explorer", StringComparison.Ordinal) == 0)
                                    {
                                        p.Kill();
                                    }
                                });
                        Process.Start("explorer.exe");
                    }
                } else
                {
                    MessageBox.Show("请使用管理员身份证重新运行软件后执行本操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreShort_Click(object sender, EventArgs e)
        {
            try
            {
                if(Util.IsAdministrator())
                {
                    Util.RestoreShort();
                    ShortStatus.Text = "已恢复";
                    ShortStatus.ForeColor = Color.Green;
                    if(AutoRestartExplorer.Checked)
                    {
                        Process.GetProcesses()
                            .ToList()
                            .ForEach(
                                p =>
                                {
                                    if(string.Compare(p.ProcessName, "explorer", StringComparison.Ordinal) == 0)
                                    {
                                        p.Kill();
                                    }
                                });
                        Process.Start("explorer.exe");
                    }
                } else
                {
                    MessageBox.Show("请使用管理员身份证重新运行软件后执行本操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            } catch(Exception ex)
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(NullReferenceException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as Button).Enabled = true;
                    } catch(IndexOutOfRangeException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as Button).Enabled = true;
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
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
                            CallButton: sender as Button);
                    } catch(NullReferenceException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as Button).Enabled = true;
                    } catch(IndexOutOfRangeException)
                    {
                        MessageBox.Show($"EPUB格式不正确，请移除错误文件后重试", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        (sender as Button).Enabled = true;
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void 打开源文件目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ListViewRightMenu.SourceControl.GetType() == typeof(ListView))
            {
                ListView TempListView = ListViewRightMenu.SourceControl as ListView;

                if(File.Exists(TempListView.FocusedItem.Text))
                {
                    Process.Start(Path.GetDirectoryName(TempListView.FocusedItem.Text));
                }
            }
        }

        private void 打开源文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ListViewRightMenu.SourceControl.GetType() == typeof(ListView))
            {
                ListView TempListView = ListViewRightMenu.SourceControl as ListView;

                if(File.Exists(TempListView.FocusedItem.Text))
                {
                    Process.Start(TempListView.FocusedItem.Text);
                }
            }
        }

        private void 打开转换后目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ListViewRightMenu.SourceControl.GetType() == typeof(ListView))
            {
                ListView TempListView = ListViewRightMenu.SourceControl as ListView;

                if(TempListView.FocusedItem.SubItems.Count >= 4)
                {
                    if(File.Exists(TempListView.FocusedItem.SubItems[2].Text))
                    {
                        Process.Start(Path.GetDirectoryName(TempListView.FocusedItem.SubItems[2].Text));
                    }
                }
            }
        }

        private void 打开转换后文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ListViewRightMenu.SourceControl.GetType() == typeof(ListView))
            {
                ListView TempListView = ListViewRightMenu.SourceControl as ListView;

                if(TempListView.FocusedItem.SubItems.Count >= 4)
                {
                    if(File.Exists(TempListView.FocusedItem.SubItems[2].Text))
                    {
                        Process.Start(TempListView.FocusedItem.SubItems[2].Text);
                    }
                }
            }
        }

        private void 获取PDF文件版本信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(ListViewRightMenu.SourceControl.GetType() == typeof(ListView))
            {
                ListView TempListView = ListViewRightMenu.SourceControl as ListView;

                Task.Run(
                    () =>
                    {
                        string FileName = TempListView.FocusedItem.Text;

                        string Information = string.Empty;

                        if(File.Exists(FileName))
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
                        } else
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
                        if(this.html2pdf_textbox_chromepath.Text != string.Empty)
                        {
                            UI.ConvertStart(
                                this.HTML2PDFFileList,
                                this.ConvertHtml2PdfStatusText,
                                ConvertFileType.Html2PDF,
                                ".pdf",
                                CallButton: sender as Button);
                        }
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SetWord2EpubCoverImage_Click(object sender, EventArgs e)
        {
            string SelectImageFileName = Util.ShowSelectDialog(false, false, "图片文件", "*.jpg;*.jpeg;*.png;*.bmp")
                .FirstOrDefault();

            if(string.Compare(SelectImageFileName, default, StringComparison.Ordinal) != 0 &&
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_Download_ChromeDriver").ToString()); }

        private void Html2pdf_button_set_chrome_path_Click(object sender, EventArgs e)
        {
            IEnumerable<string> chrome_path = Util.ShowSelectDialog(false, false, "chrome浏览器", "*.exe");

            string chrome_path_string = chrome_path.FirstOrDefault();

            if(chrome_path_string != string.Empty &&
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
                        if(Mss3Path != string.Empty &&
                            string.Compare(Mss3Path, default, StringComparison.Ordinal) != 0 &&
                            Mss3Path != null)
                        {
                            this.Mss3Path.Text = Mss3Path;
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        try
                                        {
                                            DataTable TempVoteDataTable = Util.GetMss3VoteInfo(Mss3Path);

                                            if(TempVoteDataTable != null && TempVoteDataTable.Rows.Count > 0)
                                            {
                                                this.votedataview.DataSource = TempVoteDataTable;
                                            }
                                        } catch(Exception ex)
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
                                                    }));
                                        }
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
                        (sender as Button).Enabled = false;

                        if(Mss3Path.Text != string.Empty)
                        {
                            int ReturnStatusCode = Util.InsertVotePicContent(
                                this.Mss3Path.Text,
                                VotePicture.ImageLocation,
                                this.votestatustext,
                                this.voteid.Text,
                                this.votename.Text,
                                this.voteindex.Text,
                                this.votepageIndexCombox.SelectedValue.ToString(),
                                this.votepage_type_combox.SelectedValue.ToString());
                            List<string> TempChangeTableNames = new List<string>()
                            {
                                "Page_SelectorInfo",
                                "Report_Detail",
                                "Result_DetailInfo",
                                "Result_NumInfo",
                                "TempResult_DetailInfo"
                            };

                            if(ChangeMssItemColumn.Checked)
                            {
                                foreach(string TableName in TempChangeTableNames)
                                {
                                    string TempExcuteSql = $"ALTER TABLE {TableName} ALTER COLUMN Item Memo";

                                    Util.ExecuteAccessSql(this.Mss3Path.Text, TempExcuteSql,new  AccessDatabaseFactory());
                                }
                                if(ReturnStatusCode != 0)
                                {
                                    this.BeginInvoke(
                                        new Action(
                                            () =>
                                            {
                                                MessageBox.Show(
                                                    "已将全部项目列字段长度设置为长文本类型",
                                                    "提示",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Asterisk);
                                                (sender as Button).Enabled = true;
                                            }));
                                }
                            } else if(ResetMssItemColumn.Checked)
                            {
                                foreach(string TableName in TempChangeTableNames)
                                {
                                    string TempExcuteSql = string.Empty;

                                    if(TableName == "Report_Detail" || TableName == "Result_NumInfo")
                                    {
                                        TempExcuteSql = $"ALTER TABLE {TableName} ALTER COLUMN Item TEXT(255)";
                                    } else if(TableName == "Result_DetailInfo" || TableName == "TempResult_DetailInfo")
                                    {
                                        TempExcuteSql = $"ALTER TABLE {TableName} ALTER COLUMN Item TEXT(100)";
                                    } else
                                    {
                                        TempExcuteSql = $"ALTER TABLE {TableName} ALTER COLUMN Item TEXT(50)";
                                    }

                                    Util.ExecuteAccessSql(this.Mss3Path.Text, TempExcuteSql,new AccessDatabaseFactory());
                                }
                                if(ReturnStatusCode != 0)
                                {
                                    this.BeginInvoke(
                                        new Action(
                                            () =>
                                            {
                                                MessageBox.Show(
                                                    "已将全部项目列字段长度重置为默认值",
                                                    "提示",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Asterisk);
                                                (sender as Button).Enabled = true;
                                            }));
                                }
                            }
                        }
                        (sender as Button).Enabled = true;
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void SelectVotePic_Click(object sender, EventArgs e)
        {
            string VoteImagePath = Util.ShowSelectDialog(false, false, "图片文件", "*.jpg;*.bmp;*.png;*.tif;*.tiff")
                .FirstOrDefault();
            if(VoteImagePath != string.Empty &&
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

            if(FontSelectDialogResult == DialogResult.OK)
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
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("压缩图片（格式转换）"); }

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
            if(File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
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
                if(ExitResult == DialogResult.OK)
                {
                    string AppExcuteFile = Application.ExecutablePath;
                    Process.Start(AppExcuteFile);

                    Environment.Exit(0);
                }
            } else
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
            foreach(TabPage TempPage in this.MainTabPage.TabPages)
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

            foreach(AssemblyName LibName in LibNames)
            {
                if(string.Compare(LibName.Name, "Aspose.Hook", StringComparison.Ordinal) != 0)
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
                        if(barcodeTextContentBox.Text != string.Empty)
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
                                barcodeScanTypeCombox.SelectedIndex,
                                barcodeLogoImage: BarCodLogoImageBox.Image);
                            barcodePreviewBox.Image = new Bitmap(tempstream);
                        }
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    if(ex.Message.Contains("enough space"))
                                    {
                                        MessageBox.Show(
                                            "没有足够空间存放文字，请将尺寸模式设置为 无 后重试",
                                            "错误",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                                    } else
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
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
                        if(!string.IsNullOrEmpty(barcodeTemplateFile))
                        {
                            this.Invoke(
                                (Action)(() =>
                                {
                                    this.barcodeMutilList.DataSource = Util.ReadBarCodeTemplate(barcodeTemplateFile);
                                }));
                        }
                    } catch(Exception ex)
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
                        if(this.barcodeMutilList.DataSource != null && this.barcodeMutilList.Rows.Count > 1)
                        {
                            IEnumerable<string> TempMutilSavePath = Util.ShowSelectDialog(
                                false,
                                true,
                                string.Empty,
                                string.Empty);
                            string MutilSavePath = TempMutilSavePath.FirstOrDefault();

                            if(MutilSavePath != string.Empty &&
                                string.Compare(MutilSavePath, default, StringComparison.Ordinal) != 0)
                            {
                                (sender as Button).Enabled = false;
                                foreach(DataGridViewRow TempRow in barcodeMutilList.Rows)
                                {
                                    if(!TempRow.IsNewRow)
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
                                            barcodeScanTypeCombox.SelectedIndex,
                                            barcodeLogoImage: BarCodLogoImageBox.Image);
                                    }
                                }

                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            (sender as Button).Enabled = true;
                                            if(MessageBox.Show(
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
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void ExportBarCodeTempLate_Click(object sender, EventArgs e)
        {
            object BarcodeTemplateUpload = Util.GetEmbedResourceObj("BarCodeTemplate");

            string SaveTemplateFile = Util.ShowSaveFileDialog("Excel文件|*.xlsx");

            if(SaveTemplateFile != string.Empty)
            {
                File.WriteAllBytes(SaveTemplateFile, (byte[])BarcodeTemplateUpload);

                if(File.Exists(SaveTemplateFile))
                {
                    MessageBox.Show("导出完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
        }

        private void SaveBarCodePreview_Click(object sender, EventArgs e)
        {
            try
            {
                if(barcodePreviewBox.Image != null)
                {
                    string SaveFileName = Util.ShowSaveFileDialog("PNG图片|*.png");
                    Bitmap TempBitMap = new Bitmap(barcodePreviewBox.Image);

                    if(SaveFileName != string.Empty)
                    {
                        TempBitMap.Save(SaveFileName);

                        if(MessageBox.Show("生成完毕，是否打开生成目录？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) ==
                            DialogResult.Yes)
                        {
                            Process.Start(Path.GetDirectoryName(SaveFileName));
                        }
                    }
                }
            } catch(ExternalException)
            {
                MessageBox.Show("请不要覆盖保存已被加载到软件中的图片文件", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 简易二维码生成ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("简易二维码生成"); }

        private void barcodeCopyImage_Click(object sender, EventArgs e)
        {
            if(barcodePreviewBox.Image != null)
            {
                Clipboard.SetImage(barcodePreviewBox.Image);
                MessageBox.Show("已复制图像", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void city_level_1_combox_TextChanged(object sender, EventArgs e)
        {
            city_level_2_combox.Items.Clear();
            foreach(JToken City_2_Name in City_Json[city_level_1_combox.SelectedIndex]["children"])
            {
                city_level_2_combox.Items.Add(City_2_Name["name"]);//添加对应的二级城市
            }
            city_level_2_combox.SelectedIndex = 0;
        }

        private void city_level_2_combox_TextChanged(object sender, EventArgs e)
        {
            city_level_3_combox.Items.Clear();
            foreach(JToken City_3_Name in City_Json[city_level_1_combox.SelectedIndex]["children"][
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
                        gen_card_id_box.Clear();
                        gen_card_id_name_box.Clear();
                        Random random = new Random();
                        for(int i = 0; i < gen_card_number.Value; i++)
                        {
                            if(gen_card_id_name_check_box.Checked)
                            {
                                string gen_card_name = Util.GenRandomName(random, random);
                                gen_card_id_name_box.AppendText(gen_card_name + Environment.NewLine);
                                gen_card_id_name_box.ScrollToCaret();
                                gen_card_id_name_text.Text = $"[已勾选，已生成{i + 1}个姓名]";
                            }
                            string gen_card_id = Util.GenChinaCard(
                                Pac_Code.ToString(),
                                Util.GenRandomBirthday(random, ony_gen_18_year.Checked),
                                Util.GenRandomIndex(random));
                            gen_card_id_box.AppendText(gen_card_id + Environment.NewLine);
                            gen_card_id_box.ScrollToCaret();
                            gen_card_num_text.Text = $"正在生成第{i + 1}个号码";
                        }
                        gen_card_id_box.Lines = gen_card_id_box.Lines
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .ToArray();//去除空行重新赋值
                        gen_card_id_name_box.Lines = gen_card_id_name_box.Lines
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .ToArray();
                    });
            } catch(Exception ex)
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
            if(gen_card_id_box.Text != string.Empty)
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
            if(vaildate_tuple.Item4)
            {
                ver_card_id_sign_tips.ForeColor = Color.ForestGreen;
                ver_card_id_sign_tips.Text = "正确";
            } else
            {
                ver_card_id_sign_tips.ForeColor = Color.Tomato;
                ver_card_id_sign_tips.Text = $"校验码不正确，应为 {vaildate_tuple.Item5}";
            }

            if(string.Compare(ver_card_id_text_box.Text, "sfz", StringComparison.Ordinal) == 0)
            {
                gen_card_id_group.Enabled = true;
                gen_card_result_group.Enabled = true;
                Util.PlayMp3WithStream(
                    "amagi",
                    status =>
                    {
                    });
            }

            if(string.Compare(ver_card_id_birthday.Text, "日期不正确", StringComparison.Ordinal) == 0)
            {
                ver_card_id_birthday.ForeColor = Color.Tomato;
            } else
            {
                ver_card_id_birthday.ForeColor = Color.ForestGreen;
            }
        }

        private void gen_card_id_name_copy_Click(object sender, EventArgs e)
        {
            if(gen_card_id_name_box.Text != string.Empty)
            {
                Clipboard.SetText(gen_card_id_name_box.Text.Replace("\n", Environment.NewLine));
                MessageBox.Show("已复制到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void gen_card_id_name_check_box_CheckedChanged(object sender, EventArgs e)
        {
            if(gen_card_id_name_check_box.Checked)
            {
                gen_card_id_name_text.ForeColor = Color.Green;
                gen_card_id_name_text.Text = $"[已勾选]";
            } else
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
                        if(string.Compare(BarcodeImageFile, default, StringComparison.Ordinal) != 0 &&
                            BarcodeImageFile != string.Empty)
                        {
                            barcodeRegionPicture.Image = System.Drawing.Image.FromFile(BarcodeImageFile);
                        }
                    });
            } catch(Exception ex)
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
                        if(barcodeRegionPicture.Image != null)
                        {
                            (sender as Button).Enabled = false;
                            barcodeReaderResultbox.Text = Util.ReadBarCodeContent((Bitmap)barcodeRegionPicture.Image);
                            (sender as Button).Enabled = true;
                        }
                    });
            } catch(Exception ex)
            {
                this.BeginInvoke(
                    new Action(
                        () =>
                        {
                            MessageBox.Show(ex.ToString(), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            (sender as Button).Enabled = true;
                        }));
            }
        }

        private void CopyBarcodeReadResult_Click(object sender, EventArgs e)
        {
            if(barcodeReaderResultbox.Text != string.Empty)
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
                            CallButton: sender as Button);
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    (sender as Button).Enabled = true;
                                }));
                    }
                });
        }

        private void wEBP图片格式转换ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("WEBP图片格式转换"); }

        private void ExportNameTxt_Click(object sender, EventArgs e)
        {
            if(TargetTextBox.Text != string.Empty)
            {
                string TempExportTxtFile = Util.ShowSaveFileDialog("文本文件|*.txt");

                Task.Run(
                    () =>
                    {
                        try
                        {
                            if(TempExportTxtFile != string.Empty && TempExportTxtFile != null)
                            {
                                (sender as Button).Enabled = false;
                                File.WriteAllLines(TempExportTxtFile, TargetTextBox.Lines, Encoding.UTF8);
                                //File.WriteAllText(TempExportTxtFile, TargetTextBox.Text);
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show("导出完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                            (sender as Button).Enabled = true;
                                        }));
                            }
                        } catch(Exception ex)
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        (sender as Button).Enabled = true;
                                    }));
                        }
                    });
            }
        }

        private void Gen_Copied_File_Click(object sender, EventArgs e)
        {
            string TempFileName = Util.ShowSelectDialog(false, false, "所有文件", "*.*").FirstOrDefault();

            if(!string.IsNullOrEmpty(TempFileName) &&
                string.Compare(TempFileName, default, StringComparison.Ordinal) != 0)
            {
                Gen_Copied_SourceFileBox.Text = TempFileName;
            }
        }

        private void Gen_Copied_Path_Click(object sender, EventArgs e)
        {
            string TempPath = Util.ShowSelectDialog(false, true, "所有文件", "*.*").FirstOrDefault();

            if(!string.IsNullOrEmpty(TempPath) && string.Compare(TempPath, default, StringComparison.Ordinal) != 0)
            {
                Gen_Copied_TargetPath.Text = TempPath;
            }
        }

        private void CopiedGen_Click(object sender, EventArgs e)
        {
            if(Gen_Copied_SourceFileBox.Text != string.Empty && Gen_Copied_TargetPath.Text != string.Empty)
            {
                Task.Run(
                    () =>
                    {
                        try
                        {
                            (sender as Button).Enabled = false;
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
                                        (sender as Button).Enabled = true;
                                        MessageBox.Show("生成完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                    }));
                        } catch(Exception ex)
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        (sender as Button).Enabled = true;
                                    }));
                        }
                    });
            }
        }

        private void Gen_Copied_Namer_TextChanged(object sender, EventArgs e)
        {
            if(Gen_Copied_Namer.Text != string.Empty && Gen_Copied_SourceFileBox.Text != string.Empty)
            {
                string Number = 1.ToString($"D{copied_number_prefix_count.Text}");
                CopiedNamerText.Text = $"最终生成名称为：{Path.GetFileNameWithoutExtension(Gen_Copied_SourceFileBox.Text)}- {Gen_Copied_Namer.Text}（{Number}）{Path.GetExtension(Gen_Copied_SourceFileBox.Text)}";
            }
        }

        private void 文件副本生成ToolStripMenuItem_Click(object sender, EventArgs e)
        { this.MainTabPage.SelectedIndex = GetTabPageIndex("补充功能"); }

        private void network_info_card_list_SelectedIndexChanged(object sender, EventArgs e) { UpdateNetworkInfo(); }

        private void UpdateNetworkInfo()
        {
            Task.Run(
                () =>
                {
                    var (ipv4_address_source, default_mask_source, default_gateway_source, dns1_address, dns2_address, mac_address, is_dhcp_enabled) = Util.GetCard_Information(
                        network_info_card_list.Text);
                    if(network_info_ipv4 != null &&
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
                    if(is_dhcp_enabled)
                    {
                        set_interface_mode_auto.Checked = true;
                    } else
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
                    if(network_info_card_list != null && set_interface_name_list != null)
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
                        if(Util.IsAdministrator())
                        {
                            (sender as Button).Enabled = false;
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
                                        (sender as Button).Enabled = true;
                                        MessageBox.Show("修改成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                        UpdateNetworkInfo();//更新基础网络信息
                                        UpdateSetNetWorkInfo();//更新设置部分网络信息
                                    }));
                        } else
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
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    (sender as Button).Enabled = true;
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void set_interface_name_list_SelectedIndexChanged(object sender, EventArgs e)
        { UpdateSetNetWorkInfo(); }

        private void set_interface_mode_auto_CheckedChanged(object sender, EventArgs e)
        {
            if(set_interface_mode_auto.Checked)
            {
                set_interface_ipaddress.Enabled = false;
                set_interface_mask.Enabled = false;
                set_interface_gateway.Enabled = false;
                set_interface_dns1.Enabled = false;
                set_interface_dns2.Enabled = false;
            } else
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
            if(set_interface_mode_static.Checked)
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
                        if(Util.IsAdministrator())
                        {
                            Util.SetInterfaceStatus(network_info_card_list.SelectedItem.ToString(), "Enable");
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show("启用成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                    }));
                        } else
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(
                                            "禁用失败，请使用管理员身份执行",
                                            "提示",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);
                                    }));
                        }
                    } catch(Exception ex)
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
                        if(Util.IsAdministrator())
                        {
                            Util.SetInterfaceStatus(network_info_card_list.SelectedItem.ToString(), "Disable");
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show("禁用成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                                    }));
                        } else
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(
                                            "禁用失败，请使用管理员身份执行",
                                            "提示",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning);
                                    }));
                        }
                    } catch(Exception ex)
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
            if(network_info_mac_address.Text != string.Empty &&
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
            if(Gen_Copied_Namer.Text != string.Empty && Gen_Copied_SourceFileBox.Text != string.Empty)
            {
                string Number = 1.ToString($"D{copied_number_prefix_count.Text}");
                CopiedNamerText.Text = $"最终生成名称为：{Path.GetFileNameWithoutExtension(Gen_Copied_SourceFileBox.Text)}- {Gen_Copied_Namer.Text}（{Number}）{Path.GetExtension(Gen_Copied_SourceFileBox.Text)}";
            }
        }

        private void copy_network_info_ip_address_Click(object sender, EventArgs e)
        {
            if(network_info_ipv4.Text != string.Empty &&
                string.Compare(network_info_ipv4.Text, "未获取到", StringComparison.Ordinal) != 0)
            {
                Clipboard.SetText(network_info_ipv4.Text);
                MessageBox.Show("复制成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void copied_number_prefix_count_TextChanged(object sender, EventArgs e)
        {
            if(Gen_Copied_Namer.Text != string.Empty && Gen_Copied_SourceFileBox.Text != string.Empty)
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

                if(TempSaveFileName != string.Empty)
                {
                    File.WriteAllText(TempSaveFileName, JObject.Parse(JSON_IPCONFIG).ToString());
                    MessageBox.Show("导出完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            } catch(Exception ex)
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

                if(TempLoadConfigFileName != string.Empty &&
                    string.Compare(TempLoadConfigFileName, default, StringComparison.Ordinal) != 0)
                {
                    string JsonContent = File.ReadAllText(TempLoadConfigFileName);

                    if(JsonContent != string.Empty)
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
                    } else
                    {
                        MessageBox.Show("配置文件内无内容，配置未加载", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("LInk_Download_SaveHtmlExtension").ToString()); }

        private void money_small_TextChanged(object sender, EventArgs e)
        {
            if(money_small.Text != string.Empty && Decimal.TryParse(money_small.Text, out decimal Money) && Money > 0)
            {
                money_big.Text = Util.ConvertMoney2ChineseYulan(Decimal.Parse(money_small.Text));
            } else
            {
                money_big.Text = string.Empty;
            }
        }

        private void copy_money_big_result_Click(object sender, EventArgs e)
        {
            if(money_big.Text != string.Empty)
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
            if(Util.IsAdministrator())
            {
                if(DisableAdminBehavior.Text.StartsWith("启用"))
                {
                    Util.SetAdministratorBehavior(true);
                } else if(DisableAdminBehavior.Text.StartsWith("禁用"))
                {
                    Util.SetAdministratorBehavior(false);
                }

                MessageBox.Show("修改成功，重启电脑后设置生效", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                GetAdministratorBehaviorStatus();
            } else
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
            foreach(TabPage TempPage in MainTabPage.TabPages)
            {
                if(string.Compare(TempPage.Text, TabPageName, StringComparison.Ordinal) == 0)
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
            if(MainTabPage.SelectedIndex == GetTabPageIndex("网卡设置"))
            {
                UpdateNetworkList();
            }
            if(MainTabPage.SelectedIndex == GetTabPageIndex("单人表生成"))
            {
                SignDocumentUseOutPersonPathTextBox.Enabled = SignDocumentUseOutPersonPath.Checked;
                SignDocumentUseOutPersonPathButton.Enabled = SignDocumentUseOutPersonPath.Checked;

                SignDocumentOnlyUseOnlyImageTextBox.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
                SignDocumentOnlyUseOnlyImageButton.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
            }
            if(MainTabPage.SelectedIndex == GetTabPageIndex("Twain扫描仪调试"))
            {
                try
                {
                    if(!TwainInitState)
                    {
                        InitTwain();
                        TwainInitState = true;
                    }

                    foreach(Control TwainControl in MainTabPage.TabPages[GetTabPageIndex("Twain扫描仪调试")].Controls)
                    {
                        TwainControl.Enabled = true;
                    }
                } catch
                {
                    MessageBox.Show(
                        "没有在系统中找到TwainDsm数据源管理器，请自行下载TwainDsm.dll文件放在程序目录下调用，否则无法使用twain调试功能",
                        "提示",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    foreach(Control TwainControl in MainTabPage.TabPages[GetTabPageIndex("Twain扫描仪调试")].Controls)
                    {
                        TwainControl.Enabled = false;
                    }
                }
            }

            if(MainTabPage.SelectedIndex == GetTabPageIndex("补充功能"))
            {
                ExamPicture_UseOcr.Enabled = Environment.Is64BitProcess;
            }

            if(TabMenPageCurrentIndex != -1)
            {
                TabMenPagePreIndex = TabMenPageCurrentIndex;
            }
            TabMenPageCurrentIndex = this.MainTabPage.SelectedIndex;
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
            if(ExitResult == DialogResult.OK)
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
                        if(SaveVoteFileName != string.Empty)
                        {
                            (sender as Button).Enabled = false;
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
                                        (sender as Button).Enabled = true;
                                        if(MessageBox.Show(
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
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    (sender as Button).Enabled = true;
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
            if(Util.IsAdministrator())
            {
                Util.SetPauseUpdateTime(int.Parse(PauseUpdateYears.SelectedValue.ToString()));

                string CheckVersionResult = Util.Check_11_Or_10_Version();
                if(string.Compare(CheckVersionResult, "10", StringComparison.Ordinal) == 0)
                {
                    Process.Start("ms-settings:windowsupdate-options");
                } else if(string.Compare(CheckVersionResult, "11", StringComparison.Ordinal) == 0)
                {
                    Process.Start("ms-settings:windowsupdate");
                } else
                {
                    MessageBox.Show("只支持windows10/windows11系统", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            } else
            {
                MessageBox.Show("请使用管理员身份证重新运行软件后执行本操作", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void OpenSystemUpdate_Click(object sender, EventArgs e)
        {
            string CheckVersionResult = Util.Check_11_Or_10_Version();
            if(string.Compare(CheckVersionResult, "10", StringComparison.Ordinal) == 0)
            {
                Process.Start("ms-settings:windowsupdate-options");
            } else if(string.Compare(CheckVersionResult, "11", StringComparison.Ordinal) == 0)
            {
                Process.Start("ms-settings:windowsupdate");
            }
        }


        private void LoadExamFile_FillReportView()
        {
            Task.Run(
                () =>
                {
                    if(ExamFileText.Text != string.Empty)
                    {
                        string ExamFileExtension = Path.GetExtension(ExamFileText.Text);
                        try
                        {
                            DataTable TempTable = new DataTable();
                            //获取数据库内的报表和考评表信息
                            if (ExamFileExtension==".mdb")
                            {
                                TempTable = Util.GetExamInfo(ExamFileText.Text,new AccessDatabaseFactory());
                            }
                            else if(ExamFileExtension==".db")
                            {
                                TempTable = Util.GetExamInfo(ExamFileText.Text, new SqliteDatabaseFactory());
                            }

                            if (TempTable != null && TempTable.Rows.Count > 0)
                            {
                                VFSOFT_Report_ListView.Items.Clear();//加载之前先清除数据
                                foreach(DataRow row in TempTable.Rows)
                                {
                                    ListViewItem item = new ListViewItem(row["报表ID"].ToString());
                                    item.SubItems.Add(row["报表名称"].ToString());
                                    item.SubItems.Add(row["所属考评表"].ToString());
                                    item.SubItems.Add(row["考评表ID"].ToString());
                                    VFSOFT_Report_ListView.Items.Add(item);
                                }
                            }
                        } catch(Exception ex)
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
            if(VFSOFT_Report_ListView.Items.Count > 0 && VFSOFT_Report_ListView.SelectedItems.Count > 0)
            {
                if(string.Compare(IdType, "exam", StringComparison.Ordinal) == 0)
                {
                    ResultId = VFSOFT_Report_ListView.SelectedItems[0].SubItems[3].Text;
                } else if(string.Compare(IdType, "report", StringComparison.Ordinal) == 0)
                {
                    ResultId = VFSOFT_Report_ListView.SelectedItems[0].Text;
                }
            }

            return ResultId;
        }

        private void SelectExamFile_Click(object sender, EventArgs e)
        {
            string ExamFile = Util.ShowSelectDialog(false, false, "数据库文件", "*.mdb;*.db", vfsoft_examdata_path_textbox.Text)
                .FirstOrDefault();

            if(string.Compare(ExamFile, default, StringComparison.Ordinal) != 0 && ExamFile != string.Empty)
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

                if(ExamFileText.Text != string.Empty && selectedExamId != string.Empty)
                {
                    string ExamFileExtension = Path.GetExtension(ExamFileText.Text);
                    
                    List<string> selectedExamItems = await Task.Run(
                        () =>
                        {
                            List<string> TempLists = new List<string>();

                            if(ExamFileExtension==".mdb")
                            {
                                TempLists = Util.GetExamItemsAndExamSelectors(ExamFileText.Text, selectedExamId,new AccessDatabaseFactory());
                                
                            }
                            else if(ExamFileExtension==".db")
                            {
                                TempLists = Util.GetExamItemsAndExamSelectors(ExamFileText.Text, selectedExamId, new SqliteDatabaseFactory());
                            }

                            return TempLists;
                        })
                        .ConfigureAwait(true);

                    DataTable selectedExamRaidInfoTable = await Task.Run(
                        () =>
                        {
                            DataTable TempDataTable = new DataTable();

                            if (ExamFileExtension == ".mdb")
                            {
                                TempDataTable = Util.GetExamRaidInfo(ExamFileText.Text, selectedExamId,new AccessDatabaseFactory());

                            }
                            else if (ExamFileExtension == ".db")
                            {
                                TempDataTable = Util.GetExamRaidInfo(ExamFileText.Text, selectedExamId, new SqliteDatabaseFactory());

                            }

                            return TempDataTable;
                        })
                        .ConfigureAwait(true);

                    List<string> selectedReportItems = await Task.Run(
                        () =>
                        {
                            List<string> TempLists = new List<string>();

                            if (ExamFileExtension == ".mdb")
                            {
                                TempLists = Util.GetReportItems(ExamFileText.Text, selectedReportId, new AccessDatabaseFactory());
                            }
                            else if (ExamFileExtension == ".db")
                            {
                                TempLists = Util.GetReportItems(ExamFileText.Text, selectedReportId, new SqliteDatabaseFactory());
                            }

                            return TempLists;
                        })
                        .ConfigureAwait(true);

                    var selectedReportColsAndRows = await Task.Run(
                        () =>
                        {
                            (List<int> report_cols, List<int> report_rows) TempTuple = (null,null);

                            if(ExamFileExtension==".mdb")
                            {
                                TempTuple = Util.GetExamReportColumnsAndRows(ExamFileText.Text, selectedReportId,new AccessDatabaseFactory());
                                
                            }
                            else if(ExamFileExtension==".db")
                            {
                                TempTuple = Util.GetExamReportColumnsAndRows(ExamFileText.Text, selectedReportId, new SqliteDatabaseFactory());
                            }
                            return TempTuple;
                        })
                        .ConfigureAwait(true);

                    List<string> SortedselectedExamItems = new List<string>(selectedExamItems);

                    SortedselectedExamItems.Sort(new FileNameComparerInstance());

                    ExamItemsCombox.DataSource = SortedselectedExamItems;
                    TempExamReportItemLists = selectedExamItems;
                    TempExamReportItemLists_NoSort = new List<string>(selectedExamItems);
                    ExamReportRowMode_ExamList.DataSource = new List<string>(selectedExamItems);
                    ExamRaidInfoCombox.DataSource = selectedExamRaidInfoTable.AsEnumerable().Take(selectedExamRaidInfoTable.Rows.Count - 1).CopyToDataTable();
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
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ExamItemsCombox_SelectedIndexChangedAsync(object sender, EventArgs e)
        {
            try
            {
                string selectedExamId = GET_VFSOFT_ID("exam");

                if(ExamFileText.Text != string.Empty && selectedExamId != string.Empty)
                {
                    string ExamFileExtension = Path.GetExtension(ExamFileText.Text);
                    List<string> selectedExamItemsSelectors = await Task.Run(
                        () =>
                        {
                            List<string> TempLists = new List<string>();

                            if(ExamFileExtension==".mdb")
                            {
                                TempLists = Util.GetExamItems_Selectors(ExamFileText.Text, selectedExamId, ExamItemsCombox.Text,new AccessDatabaseFactory());
                            }
                            else if(ExamFileExtension==".db")
                            {
                                TempLists = Util.GetExamItems_Selectors(ExamFileText.Text, selectedExamId, ExamItemsCombox.Text, new SqliteDatabaseFactory());
                            }
                            return TempLists;
                        }
                        )
                        .ConfigureAwait(true);
                    
                    ExamItemsSelectorsCombox.DataSource = selectedExamItemsSelectors;

                    if(ExamReportItems.SelectedIndex != 0)
                    {
                        ExamReplaceItemName.Text = ExamItemsCombox.SelectedItem.ToString();
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshVFSOFTREPORT_Click(object sender, EventArgs e) { LoadExamFile_FillReportView(); }

        private void ExamReportChange_Click(object sender, EventArgs e)
        {
            if(VFSOFT_Report_ListView.SelectedItems.Count > 0 &&
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
                        try
                        {
                            if(TempConfirmResult == DialogResult.Yes)
                            {
                                (sender as Button).Enabled = false;
                                RefreshVFSOFTREPORT.Enabled = false;
                                List<string> SqlStrings = new List<string>();

                                string selectedReportId = GET_VFSOFT_ID("report");

                                if(ExamFileText.Text != string.Empty && selectedReportId != string.Empty)
                                {
                                    string ExamFileExtension = Path.GetExtension(ExamFileText.Text);

                                    if(ExamReportItems.SelectedIndex == 0)
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
                                    } else
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
                                            ExamReportRaids.SelectedItem.ToString());
                                    }

                                    if(ExamReportChangeItemName.Checked ||
                                        ExamReportChangeItemDataFormat.Checked ||
                                        ExamReportChangeItemDataRaid.Checked)
                                    {
                                        foreach(string UpdateString in SqlStrings)
                                        {
                                            Debug.WriteLine(UpdateString);

                                            (int TempRows, DbConnection TempConnection) TempBaseEnty = (0,null);

                                            if(ExamFileExtension==".mdb")
                                            {
                                                TempBaseEnty = Util.ExecuteAccessSql(ExamFileText.Text, UpdateString,new AccessDatabaseFactory());
                                            }
                                            else if(ExamFileExtension==".db")
                                            {
                                                TempBaseEnty = Util.ExecuteAccessSql(ExamFileText.Text, UpdateString, new SqliteDatabaseFactory());
                                            }

                                            TempBaseEnty.TempConnection.Close();

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
                                                    (sender as Button).Enabled = true;
                                                    RefreshVFSOFTREPORT.Enabled = true;
                                                }));
                                    }
                                }
                            }
                        } catch(Exception ex)
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        (sender as Button).Enabled = true;
                                        RefreshVFSOFTREPORT.Enabled = true;
                                    }));
                        }
                    });
            }
        }

        private void SignPersionDocumentChooseFile_Click(object sender, EventArgs e)
        {
            string SignDocumentFileName = Util.ShowSelectDialog(false, false, "WORD文档", "*.doc;*.docx").FirstOrDefault();

            if(string.Compare(SignDocumentFileName, default, StringComparison.Ordinal) != 0 &&
                SignDocumentFileName != string.Empty)
            {
                SignPersionDocument.Text = SignDocumentFileName;
            }
        }

        private void SignDocumentFileSaveButton_Click(object sender, EventArgs e)
        {
            string SignDocumentFileSavePath = Util.ShowSelectDialog(false, true, string.Empty, string.Empty)
                .FirstOrDefault();

            if(string.Compare(SignDocumentFileSavePath, default, StringComparison.Ordinal) != 0 &&
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
                        if(SignPersionDocument.Text != string.Empty &&
                            SignDocumentFileSaveBox.Text != string.Empty &&
                            SingPersonImageAlt.Text != string.Empty)
                        {
                            (sender as Button).Enabled = false;
                            SignDocumentProgress.Maximum = (int)SignDocumentNeedCount.Value;
                            int FileIndex = 0;

                            string SourceFileNameNotExtension = Path.GetFileNameWithoutExtension(
                                SignPersionDocument.Text);
                            string NewDocumentFileName;
                            string FileIndexName = string.Empty;
                            string FileEndName = SignDocumentFileEndName.Text;

                            if(SignDocumentUseEmbedImage.Checked)
                            {
                                //使用内置的图片文件，最多只有999张
                                List<Stream> TempPersonStrem = Util.GetPersonCodeZipFileStream();

                                foreach(Stream PersonCode in TempPersonStrem)
                                {
                                    FileIndex++;
                                    if(SignDocumentUseOnlyPageNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}页）";
                                    }
                                    if(SignDocumentUsePersonNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}人）";
                                    }
                                    if(SignDocumentUseOnlyNumber.Checked)
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
                                    if(FileIndex >= SignDocumentNeedCount.Value)
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
                                            if(OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(SignDocumentFileSaveBox.Text);
                                            }
                                        }));
                            }

                            if(SignDocumentUseOutPersonPath.Checked &&
                                SignDocumentUseOutPersonPathTextBox.Text != string.Empty)
                            {
                                //使用外部的图片文件
                                List<string> PersonFiles = Directory.GetFiles(
                                    SignDocumentUseOutPersonPathTextBox.Text,
                                    "*")
                                    .ToList();
                                //使用自然排序
                                PersonFiles.Sort(new FileNameComparerInstance());

                                foreach(string PersonFile in PersonFiles)
                                {
                                    if(PersonFile.Contains(".jpeg") ||
                                        PersonFile.Contains(".jpg") ||
                                        PersonFile.Contains(".png") ||
                                        PersonFile.Contains(".bmp"))
                                    {
                                        FileIndex++;
                                        if(SignDocumentUseOnlyPageNumber.Checked)
                                        {
                                            FileIndexName = $"（第{FileIndex}页）";
                                        }
                                        if(SignDocumentUsePersonNumber.Checked)
                                        {
                                            FileIndexName = $"（第{FileIndex}人）";
                                        }
                                        if(SignDocumentUseOnlyNumber.Checked)
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
                                        if(FileIndex >= SignDocumentNeedCount.Value)
                                        {
                                            break;
                                        }
                                    } else
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
                                            if(OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(SignDocumentFileSaveBox.Text);
                                            }
                                        }));
                            }

                            if(SignDocumentOnlyUseOnlyImage.Checked &&
                                SignDocumentOnlyUseOnlyImageTextBox.Text != string.Empty)
                            {
                                //只使用一张图片
                                for(int TempIndex = 0; TempIndex < SignDocumentNeedCount.Value; TempIndex++)
                                {
                                    if(SignDocumentUseOnlyPageNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}页）";
                                    }
                                    if(SignDocumentUsePersonNumber.Checked)
                                    {
                                        FileIndexName = $"（第{FileIndex}人）";
                                    }
                                    if(SignDocumentUseOnlyNumber.Checked)
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
                                            if(OpenResult == DialogResult.Yes)
                                            {
                                                Process.Start(SignDocumentFileSaveBox.Text);
                                            }
                                        }));
                            }
                 (sender as Button).Enabled = true;
                        }
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    (sender as Button).Enabled = true;
                                    MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void SignDocumentUseOutPersonPathButton_Click(object sender, EventArgs e)
        {
            string PerSonPath = Util.ShowSelectDialog(false, true, string.Empty, string.Empty).FirstOrDefault();

            if(string.Compare(PerSonPath, default, StringComparison.Ordinal) != 0 && PerSonPath != string.Empty)
            {
                SignDocumentUseOutPersonPathTextBox.Text = PerSonPath;
            }
        }

        private void SignDocumentUseOutPersonPath_CheckedChanged(object sender, EventArgs e)
        {
            SignDocumentUseOutPersonPathTextBox.Enabled = SignDocumentUseOutPersonPath.Checked;
            SignDocumentUseOutPersonPathButton.Enabled = SignDocumentUseOutPersonPath.Checked;
            SignDocumentNeedCount.Maximum = 9999;
        }

        private void SignDocumentOnlyUseOnlyImageButton_Click(object sender, EventArgs e)
        {
            string PerSonImage = Util.ShowSelectDialog(false, false, "图片文件", "*.jpg;*.jpeg;*.bmp;*.png")
                .FirstOrDefault();

            if(string.Compare(PerSonImage, default, StringComparison.Ordinal) != 0 && PerSonImage != string.Empty)
            {
                SignDocumentOnlyUseOnlyImageTextBox.Text = PerSonImage;
            }
        }

        private void SignDocumentOnlyUseOnlyImage_CheckedChanged(object sender, EventArgs e)
        {
            SignDocumentOnlyUseOnlyImageTextBox.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
            SignDocumentOnlyUseOnlyImageButton.Enabled = SignDocumentOnlyUseOnlyImage.Checked;
            SignDocumentNeedCount.Maximum = 9999;
        }

        private void TwainScanNumberSet_TextChanged(object sender, EventArgs e)
        {
            //当设置扫描份数的时候，禁止选择跳过空白页和双面扫描
            if(TwainScanNumberSet.Text != string.Empty)
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


                if(ExamReportItems.SelectedIndex == 0)
                {
                    ExamReplaceItemName.Text = string.Empty;
                    ExamReportChangeItemName.Checked = false;
                } else
                {
                    if(ExamReportItems.SelectedIndex <= ExamItemsCombox.Items.Count)
                    {
                        ExamItemsCombox.SelectedIndex = ExamReportItems.SelectedIndex - 1;
                    }
                }

                string selectedReportId = GET_VFSOFT_ID("report");
                if(ExamFileText.Text != string.Empty && selectedReportId != string.Empty)
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
            } catch(Exception ex)
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

            if(RaidsList != null && RaidsList.Count > 0)
            {
                StringBuilder TempRaidString = new StringBuilder();

                foreach(string Raid in RaidsList)
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

            if(string.Compare(UnitExamFileName, default, StringComparison.Ordinal) != 0 &&
                UnitExamFileName != string.Empty)
            {
                ChooseExamFileTextBox.Text = UnitExamFileName;
                LoadUnitCompNameAndPicPathAsync();
            }
        }

        private async void LoadUnitCompNameAndPicPathAsync()
        {
            try
            {
                if(ChooseExamFileTextBox.Text != string.Empty)
                {
                    string ExamFileExtension = Path.GetExtension(ChooseExamFileTextBox.Text);
                    List<string> UnitComputerNames = await Task.Run(
                        () => 
                        {
                            List<string> TempLists = new List<string>();

                            if(ExamFileExtension==".mdb" || ExamFileExtension==".exam")
                            {
                                TempLists = Util.GetUnitExamComputerName(ChooseExamFileTextBox.Text,new AccessDatabaseFactory());
                            }
                            else if(ExamFileExtension==".db")
                            {
                                TempLists = Util.GetUnitExamComputerName(ChooseExamFileTextBox.Text, new SqliteDatabaseFactory());
                            }

                            return TempLists;

                        })
                        .ConfigureAwait(true);

                    ExamComputerNames.DataSource = UnitComputerNames;

                    List<string> UnitPicPaths = await Task.Run(
                        () => 
                        {
                            List<string> TempLists = new List<string>();

                            if (ExamFileExtension == ".mdb" || ExamFileExtension == ".exam")
                            {
                                TempLists = Util.GetUnitExamPicPath(ChooseExamFileTextBox.Text, new AccessDatabaseFactory());
                            }
                            else if (ExamFileExtension == ".db")
                            {
                                TempLists = Util.GetUnitExamPicPath(ChooseExamFileTextBox.Text, new SqliteDatabaseFactory());
                            }

                            return TempLists;
                        })
                        .ConfigureAwait(true);

                    ExamPicPathCombox.DataSource = UnitPicPaths;
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetRandomComputerNameButton_Click(object sender, EventArgs e)
        { ExamComputerChangeName.Text = Util.GetRandomMatchName((int)GetRandomComputerNameRandLength.Value); }

        private void ExamComputerNameChange_Click(object sender, EventArgs e)
        {
            if(ChooseExamFileTextBox.Text != string.Empty &&
                ExamComputerChangeName.Text != string.Empty &&
                ExamComputerNames.SelectedItem != null)
            {
                DialogResult TempConfirmResult = MessageBox.Show(
                    "确定执行修改吗？执行后数据无法恢复",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);
                if(TempConfirmResult == DialogResult.Yes)
                {
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                (sender as Button).Enabled = false;
                                string ExamFileExtension = Path.GetExtension(ChooseExamFileTextBox.Text);

                                if (ChangeExamCompNameSelect.Checked)
                                {
                                    string UpdateString = Util.ChangeUnitExamComputerName(
                                        ExamComputerNames.SelectedItem.ToString(),
                                        ExamComputerChangeName.Text);

                                    (int TempRow, DbConnection TempConnection) TempBaseEnty = (0,null);

                                    if(ExamFileExtension==".exam")
                                    {
                                        TempBaseEnty = Util.ExecuteAccessSql(
                                        ChooseExamFileTextBox.Text,
                                        UpdateString,
                                        new AccessDatabaseFactory());
                                    }
                                    else if(ExamFileExtension==".db")
                                    {
                                        TempBaseEnty = Util.ExecuteAccessSql(
                                        ChooseExamFileTextBox.Text,
                                        UpdateString,
                                        new SqliteDatabaseFactory());
                                    }

                                    TempBaseEnty.TempConnection.Close();

                                    this.BeginInvoke(
                                        new Action(
                                            () =>
                                            {
                                                (sender as Button).Enabled = true;
                                                MessageBox.Show(
                                                    "修改完毕",
                                                    "提示",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Asterisk);
                                            }));
                                } else if(ChangeExamPicPathSelect.Checked)
                                {
                                    if(Directory.Exists(ExamTargetPicPathTextBox.Text))
                                    {
                                        string TempModifyPicPath = ExamTargetPicPathTextBox.Text;
                                        if (!TempModifyPicPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                                        {
                                            TempModifyPicPath += Path.DirectorySeparatorChar.ToString();
                                        }
                                        string UpdateString = Util.ChangeUnitExamPicPath(
                                            ExamPicPathCombox.SelectedItem.ToString(),
                                            TempModifyPicPath);

                                        (int TempRow, DbConnection TempConnection) TempBaseEnty = (0, null);

                                        if (ExamFileExtension == ".exam")
                                        {
                                            TempBaseEnty = Util.ExecuteAccessSql(
                                            ChooseExamFileTextBox.Text,
                                            UpdateString,
                                            new AccessDatabaseFactory());
                                        }
                                        else if (ExamFileExtension == ".db")
                                        {
                                            TempBaseEnty = Util.ExecuteAccessSql(
                                            ChooseExamFileTextBox.Text,
                                            UpdateString,
                                            new SqliteDatabaseFactory());
                                        }

                                        TempBaseEnty.TempConnection.Close();

                                        this.BeginInvoke(
                                            new Action(
                                                () =>
                                                {
                                                    (sender as Button).Enabled = true;
                                                    MessageBox.Show(
                                                        "修改完毕",
                                                        "提示",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk);
                                                }));
                                    } else
                                    {
                                        this.BeginInvoke(
                                            new Action(
                                                () =>
                                                {
                                                    (sender as Button).Enabled = true;
                                                    MessageBox.Show(
                                                        "路径不存在，请重新设置路径！",
                                                        "提示",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Error);
                                                }));
                                    }
                                }
                                LoadUnitCompNameAndPicPathAsync();
                            } catch(Exception ex)
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            (sender as Button).Enabled = true;
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
        { Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_AccessEngine").ToString()); }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_AccessEngine").ToString()); }

        private void ExamItemsCombox_MouseHover(object sender, EventArgs e)
        {
            if(ExamItemsCombox.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamItemsCombox, $"[共有{ExamItemsCombox.Items.Count}个项目]");
            }
        }

        private void ExamItemsSelectorsCombox_MouseHover(object sender, EventArgs e)
        {
            if(ExamItemsSelectorsCombox.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamItemsSelectorsCombox, $"[共有{ExamItemsSelectorsCombox.Items.Count}个选项]");
            }
        }

        private void ExamRaidInfoCombox_MouseHover(object sender, EventArgs e)
        {
            if(ExamRaidInfoCombox.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamRaidInfoCombox, $"[共有{ExamRaidInfoCombox.Items.Count - 1}个主体]");
            }
        }

        private void ExamReportItems_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportItems.Items.Count > 0)
            {
                MainToolsTip.SetToolTip(ExamReportItems, $"[共有{ExamReportItems.Items.Count - 1}个项目]");
            }
        }

        private void ExamRaidInfoCombox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(string.Compare(
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

                    for(int NumberIndex = (int)NumberIndexStart.Value; NumberIndex <= (int)NumberIndexEnd.Value; NumberIndex++)
                    {
                        string NumberIndexString = NumberIndex.ToString();

                        NumberIndexString = $"{Number_Prefix.Text}{NumberIndexString}{Number_Suffix.Text}";

                        TempIndexLists.Add(NumberIndexString);
                    }

                    NumberIndexResultBox.Lines = TempIndexLists.ToArray();
                });
        }

        private void CopyNumberIndexResult_Click(object sender, EventArgs e)
        {
            if(NumberIndexResultBox.Text != string.Empty)
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

            if(SaveFileName != string.Empty)
            {
                string KMSAIOCMD = Util.GetEmbedResourceObj("KMSAIO") as string;
                File.WriteAllText(SaveFileName, KMSAIOCMD, Encoding.Default);
                MessageBox.Show("释放完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ReleaseRenamerApp_Click(object sender, EventArgs e)
        {
            string SaveFileName = Util.ShowSaveFileDialog("EXE文件|*.exe");

            if(SaveFileName != string.Empty)
            {
                byte[] RenamerProApplicationBytes = Util.GetEmbedResourceObj("ReNamerPro73") as byte[];
                File.WriteAllBytes(SaveFileName, RenamerProApplicationBytes);
                MessageBox.Show("释放完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void ReleaseRenamerReplaceConfig_Click(object sender, EventArgs e)
        {
            string SaveFileName = Util.ShowSaveFileDialog("RNP文件|*.rnp");

            if(SaveFileName != string.Empty)
            {
                byte[] RenamerProRnpBytes = Util.GetEmbedResourceObj("移除副本规则_部分自定义规则") as byte[];
                File.WriteAllBytes(SaveFileName, RenamerProRnpBytes);
                MessageBox.Show("释放完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void 折叠本节点全部内容ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode CurrentSelectNode = this.DecodeDataMatrixView.SelectedNode;
            CurrentSelectNode.Collapse();
        }

        private void 折叠全部节点ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach(TreeNode TempNode in DecodeDataMatrixView.Nodes)
            {
                TempNode.Collapse();
            }
        }

        private void TwainSetDeviceSaveTime_Click(object sender, EventArgs e)
        {
            dataSource ??= twainSession.DefaultSource;

            if(dataSource.Open() == ReturnCode.Success)
            {
                if(dataSource.Capabilities.CapPowerSaveTime.CanSet)
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
            if(s1.Error == null && s1.IsUpdateAvailable)
            {
                VersionInfo.VersionUpdateLogUrl = s1.ChangelogURL;
                DialogResult UpdateResult = MessageBox.Show(
                    $"当前版本为：{s1.InstalledVersion}，最新版本为：{s1.CurrentVersion}，是否需要打开更新页面进行更新？",
                    "提示",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Asterisk);

                if(UpdateResult == DialogResult.OK)
                {
                    Process.Start(s1.DownloadURL);
                }
            } else
            {
                MessageBox.Show(
                    $"当前版本为{VersionInfo.VersionNumber}，没有检测到新版本",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Asterisk);
            }
        }

        private void VersionUpdateLog_Click(object sender, EventArgs e)
        {
            if(VersionInfo.VersionUpdateLogUrl != string.Empty)
            {
                Process.Start(VersionInfo.VersionUpdateLogUrl);
            }
        }

        private void UseLocalOffice_CheckedChanged(object sender, EventArgs e)
        {
            if(Util.IsAdministrator())
            {
                this.PDFComplianceBox.Enabled = !UseLocalOffice.Checked;
                this.Word2PDFOptimizCombox.Enabled = !UseLocalOffice.Checked;
                this.word2pdf_colorMode.Enabled = !UseLocalOffice.Checked;
                this.EmbedFonts.Enabled = !UseLocalOffice.Checked;
                this.word2pdf_need_update_fields.Enabled = !UseLocalOffice.Checked;
                UseLocalOffice.Enabled = true;
            } else
            {
                if(UseLocalOffice.Checked)
                {
                    MessageBox.Show("本功能需要管理员身份运行程序，请使用管理员身份运行后重试", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                UseLocalOffice.Checked = false;
                UseLocalOffice.Enabled = false;
            }
        }

        private void ExamReportRowMode_ExamList_MouseHover(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(ExamReportRowMode_ExamList.Text))
            {
                MainToolsTip.SetToolTip(ExamReportRowMode_ExamList, ExamReportRowMode_ExamList.Text);
            }
        }

        private void ExamReportRowMode_ExamSelectorBox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportRowMode_ExamSelectorBox.Text != string.Empty)
            {
                MainToolsTip.SetToolTip(ExamReportRowMode_ExamSelectorBox, ExamReportRowMode_ExamSelectorBox.Text);
            }
        }

        private void ExamReportParaRaidInfo_Combox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportParaRaidInfo_Combox.SelectedItem != null)
            {
                string TempTip = ExamReportParaRaidInfo_Combox.SelectedValue.ToString();

                if(ExamReportParaRaidInfo_Combox.SelectedIndex == ExamReportParaRaidInfo_Combox.Items.Count - 1)
                {
                    TempTip += "（仅列模式可用，表示对列排的多填表人身份批量设置）";
                }
                MainToolsTip.SetToolTip(ExamReportParaRaidInfo_Combox, TempTip);
            }
        }

        private void ChangeExamReportParaButton_Click(object sender, EventArgs e)
        {
            try
            {
                if(VFSOFT_Report_ListView.SelectedItems.Count > 0)
                {
                    DialogResult TempConfirmResult = MessageBox.Show(
                        "确定执行修改吗？执行后数据无法恢复",
                        "提示",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                if(TempConfirmResult == DialogResult.Yes)
                                {
                                    (sender as Button).Enabled = false;

                                    string ExamReportId = GET_VFSOFT_ID("report");

                                    if(ExamFileText.Text != string.Empty && ExamReportId != string.Empty)
                                    {
                                        string ExamFileExtension = Path.GetExtension(ExamFileText.Text);

                                        if (ExamFileExtension == ".mdb")
                                        {
                                            Util.ChangeExamReportParaContent(
                                            ExamFileText.Text,
                                            ExamReportId,
                                            ExamReportPara_RowMode.Checked,
                                            ExamReportParaDataFormat_Combox.SelectedItem.ToString(),
                                            ExamReportParaRaidInfo_Combox.SelectedValue.ToString(),
                                            ExamReportParaDataFormatReverse_Combox.SelectedIndex.ToString(),
                                            ExamReportParaDataFormatFont_Box.Text,
                                            ExamReportRowMode_Row_Combox.Text,
                                            ExamReportRowMode_Col_Start_Combox.Text,
                                            ExamReportRowMode_Col_End_Combox.Text,
                                            ExamReportRowMode_ExamList.Text,
                                            ExamReportRowMode_ExamSelectorBox.Text,
                                            ExamReportRowMode_ExamSelectorInsertSame.Checked,
                                            ExamReportColMode_Col_Combox.Text,
                                            ExamReportColMode_Row_Start_Combox.Text,
                                            ExamReportColMode_Row_End_Combox.Text,
                                            ExamReportColMode_ExamBox.Text,
                                            ExamReportColMode_ExamSelectorBox.Text,
                                            ExamReportRowMode_ExamItemInsertSame.Checked,
                                            ExamReportParaChangeProcessBar,
                                            new AccessDatabaseFactory());
                                        }
                                        else if(ExamFileExtension==".db")
                                        {
                                            Util.ChangeExamReportParaContent(
                                            ExamFileText.Text,
                                            ExamReportId,
                                            ExamReportPara_RowMode.Checked,
                                            ExamReportParaDataFormat_Combox.SelectedItem.ToString(),
                                            ExamReportParaRaidInfo_Combox.SelectedValue.ToString(),
                                            ExamReportParaDataFormatReverse_Combox.SelectedIndex.ToString(),
                                            ExamReportParaDataFormatFont_Box.Text,
                                            ExamReportRowMode_Row_Combox.Text,
                                            ExamReportRowMode_Col_Start_Combox.Text,
                                            ExamReportRowMode_Col_End_Combox.Text,
                                            ExamReportRowMode_ExamList.Text,
                                            ExamReportRowMode_ExamSelectorBox.Text,
                                            ExamReportRowMode_ExamSelectorInsertSame.Checked,
                                            ExamReportColMode_Col_Combox.Text,
                                            ExamReportColMode_Row_Start_Combox.Text,
                                            ExamReportColMode_Row_End_Combox.Text,
                                            ExamReportColMode_ExamBox.Text,
                                            ExamReportColMode_ExamSelectorBox.Text,
                                            ExamReportRowMode_ExamItemInsertSame.Checked,
                                            ExamReportParaChangeProcessBar,
                                            new SqliteDatabaseFactory());
                                        }

                                        

                                        this.BeginInvoke(
                                            new Action(
                                                () =>
                                                {
                                                    MessageBox.Show(
                                                        "修改完毕，请前往考评系统核对报表参数",
                                                        "提示",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk);
                                                    (sender as Button).Enabled = true;
                                                }));
                                    }
                                }
                            } catch(Exception ex)
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
                                            (sender as Button).Enabled = true;
                                        }));
                            }
                        });
                }
            } catch(Exception ex)
            {
                this.BeginInvoke(
                    new Action(
                        () =>
                        {
                            MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            (sender as Button).Enabled = true;
                        }));
            }
        }


        private void ExamReportColMode_ExamBox_MouseHover(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(ExamReportColMode_ExamBox.Text))
            {
                MainToolsTip.SetToolTip(ExamReportColMode_ExamBox, ExamReportColMode_ExamBox.Text);
            }
        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(string.IsNullOrEmpty(ChangeReportItemTempHelpPdfFile) || !File.Exists(ChangeReportItemTempHelpPdfFile))
            {
                ChangeReportItemTempHelpPdfFile = Path.GetTempFileName().Replace(".tmp", ".pdf");
                byte[] resourceBytes = (byte[])Util.GetEmbedResourceObj("ChangeReporItem");
                using(MemoryStream TempChangeReportItemHelpStream = new MemoryStream(resourceBytes))
                    using(FileStream TempPdfFileStream = File.Create(ChangeReportItemTempHelpPdfFile))
                    {
                        TempChangeReportItemHelpStream.CopyTo(TempPdfFileStream);
                        TempPdfFileStream.Flush(); // 确保文件写入完成
                    }
            }
            Process.Start(ChangeReportItemTempHelpPdfFile);
        }

        private void ExamReportRowMode_Row_Combox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportRowMode_Row_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportRowMode_Row_Combox,
                    ExamReportRowMode_Row_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportRowMode_Col_Start_Combox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportRowMode_Col_Start_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportRowMode_Col_Start_Combox,
                    ExamReportRowMode_Col_Start_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportRowMode_Col_End_Combox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportRowMode_Col_End_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportRowMode_Col_End_Combox,
                    ExamReportRowMode_Col_End_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportColMode_Col_Combox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportColMode_Col_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportColMode_Col_Combox,
                    ExamReportColMode_Col_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportColMode_Row_Start_Combox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportColMode_Row_Start_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportColMode_Row_Start_Combox,
                    ExamReportColMode_Row_Start_Combox.SelectedItem.ToString());
            }
        }

        private void ExamReportColMode_Row_End_Combox_MouseHover(object sender, EventArgs e)
        {
            if(ExamReportColMode_Row_End_Combox.SelectedItem != null)
            {
                MainToolsTip.SetToolTip(
                    ExamReportColMode_Row_End_Combox,
                    ExamReportColMode_Row_End_Combox.SelectedItem.ToString());
            }
        }

        private void linkLabel8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_WPS").ToString()); }

        private void ManW_Click(object sender, EventArgs e)
        {
            ManW.Enabled = false;
            ManW.Text = "正在复活中...";
            LocalXMlLicName.Text = Util.GetEmbedResourceObj("LocalXMLFileName").ToString();
            LocalXMlLicName.Visible = false;
            Util.PlayMp3WithStream(
                "Man",
                status =>
                {
                    ManW.Enabled = true;
                    ManW.Text = "点击复活牢大";
                    LocalXMlLicName.Enabled = true;
                    LocalXMlLicName.Visible = true;
                });
        }

        private void 查看公告信息ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PublicNoticeDialog publicNoticeDialog = new PublicNoticeDialog();

            publicNoticeDialog.ShowDialog();
        }

        private void ExamPicture_Source_Select_Click(object sender, EventArgs e)
        {
            string ExamPictureSourcePath = Util.ShowSelectDialog(false, true, string.Empty, string.Empty)
                .FirstOrDefault();

            if(!string.IsNullOrEmpty(ExamPictureSourcePath) && ExamPictureSourcePath != default)
            {
                ExamPicture_Source.Text = ExamPictureSourcePath;
            }
        }

        private void ExamPicture_Target_Select_Click(object sender, EventArgs e)
        {
            string ExamPictureTargetPath = Util.ShowSelectDialog(false, true, string.Empty, string.Empty)
                .FirstOrDefault();

            if(!string.IsNullOrEmpty(ExamPictureTargetPath) && ExamPictureTargetPath != default)
            {
                ExamPicture_Target.Text = ExamPictureTargetPath;
            }
        }

        private void ExamPicture_Class_Start_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    try
                    {
                        if(ExamPicture_Source.Text != string.Empty && ExamPicture_Target.Text != string.Empty)
                        {
                            (sender as Button).Enabled = false;
                            Exam_Picture_Class_StatusText.Text = "正在处理中...";
                            Util.RecognitionPictureDataMatrix(
                                ExamPicture_Source.Text,
                                Exam_Picture_Source_IncludeSubDir.Checked,
                                ExamPicture_Target.Text,
                                Exam_Picture_Class_ProgressBar,
                                Exam_Picture_Class_StatusText,
                                Exam_Picture_ThreadNumbers.SelectedItem.ToString(),
                                Exam_Picture_ThreadNumbers.SelectedIndex,
                                ExamPicture_UseOcr.Checked,
                                ExamPictureOcrMatchString.Text,
                                Exam_Picture_OcrMark.Checked);
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        (sender as Button).Enabled = true;

                                        Exam_Picture_Class_ProgressBar.Value = 0;
                                        Exam_Picture_Class_StatusText.Text = "处理完毕";
                                        DialogResult MessageBoxResult = MessageBox.Show(
                                            "处理完毕，是否需要打开目标目录？",
                                            "提示",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Question);
                                        if(MessageBoxResult == DialogResult.Yes)
                                        {
                                            Process.Start(ExamPicture_Target.Text);
                                        }
                                    }));
                        }
                    } catch(Exception ex)
                    {
                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    (sender as Button).Enabled = true;
                                    Exam_Picture_Class_ProgressBar.Value = 0;
                                    Exam_Picture_Class_StatusText.Text = "等待处理中...";
                                    MessageBox.Show(
                                        $"请检查算法包是否导入，报错信息：{ex.Message}",
                                        "错误",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                                }));
                    }
                });
        }

        private void linkLabel9_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_ScannerDriver").ToString()); }

        /// <summary>
        /// 动态添加控件到另一个控件上去
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="CntainerControl">容器控件</param>
        /// <param name="ControlName">被添加的控件名称</param>
        /// <param name="ControlLocation">被添加的控件位置</param>
        /// <param name="ControlSize">被添加的控件大小</param>
        private T DynamicControlAdd<T>(
            Control CntainerControl,
            string ControlName,
            Point ControlLocation,
            Size ControlSize)
            where T : Control, new()
        {
            T AddedControl = new T();
            AddedControl.Location = ControlLocation;
            AddedControl.Size = ControlSize;
            AddedControl.Name = ControlName;

            CntainerControl.Controls.Add(AddedControl);

            return AddedControl;
        }

        private void WordReplaceAddFind_Click(object sender, EventArgs e)
        {
            DynamicAddControlParams.WORDFINDREPLACE_FIND_NAME_INDEX++;
            //DynamicAddControlParams.WORDFINDREPLACE_FINDREPLACE_LOCATION_Y += 25;


            int TempBaseControlY = WordReplaceFindTextBox.Location.Y + 20;

            int TempLocationY = TempBaseControlY +
                (DynamicAddControlParams.WordFindReplace_FindControl_Lists.Count * 25);

            string TempControlName_Find = $"{DynamicAddControlParams.WORDFINDREPLACE_FIND_NAME_PREFIX}{DynamicAddControlParams.WORDFINDREPLACE_FIND_NAME_INDEX}";
            string TempControlName_Replace = $"{DynamicAddControlParams.WORDFINDREPLACE_REPLACE_NAME_PREFIX}{DynamicAddControlParams.WORDFINDREPLACE_FIND_NAME_INDEX}";

            Point TempPoint_FindArea = new Point(
                WordReplaceFindTextBox.Location.X,
                WordReplaceFindTextBox.Location.Y + TempLocationY);
            Point TempPoint_ReplaceArea = new Point(
                WordReplaceTextBox.Location.X,
                WordReplaceTextBox.Location.Y + TempLocationY);

            TextBox TempFindTextBox = DynamicControlAdd<TextBox>(
                WordReplaceFindPanel,
                TempControlName_Find,
                TempPoint_FindArea,
                WordReplaceFindTextBox.Size);
            TextBox TempReplaceTextBox = DynamicControlAdd<TextBox>(
                WordReplaceReplacePanel,
                TempControlName_Replace,
                TempPoint_ReplaceArea,
                WordReplaceTextBox.Size);

            DynamicAddControlParams.WordFindReplace_FindControl_Lists.Add(TempFindTextBox);
            DynamicAddControlParams.WordFindReplace_ReplaceControl_Lists.Add(TempReplaceTextBox);
        }

        private void WordReplaceRemoveFind_Click(object sender, EventArgs e)
        {
            if(DynamicAddControlParams.WordFindReplace_FindControl_Lists.Count > 0)
            {
                Control LastControl = DynamicAddControlParams.WordFindReplace_FindControl_Lists.Last();
                WordReplaceFindPanel.Controls.Remove(LastControl);
                DynamicAddControlParams.WordFindReplace_FindControl_Lists.Remove(LastControl);
            }

            if(DynamicAddControlParams.WordFindReplace_ReplaceControl_Lists.Count > 0)
            {
                Control LastControl = DynamicAddControlParams.WordFindReplace_ReplaceControl_Lists.Last();
                WordReplaceReplacePanel.Controls.Remove(LastControl);
                DynamicAddControlParams.WordFindReplace_ReplaceControl_Lists.Remove(LastControl);
            }
        }

        private void ExcelReplaceFindAdd_Click(object sender, EventArgs e)
        {
            DynamicAddControlParams.EXCELFINDREPLACE_NAME_INDEX++;
            //DynamicAddControlParams.EXCELFINDREPLACE_LOCATION_Y += 25;

            int TempBaseControlY = ExcelFindTextBox.Location.Y + 20;

            int TempLocationY = TempBaseControlY +
                (DynamicAddControlParams.ExcelFindReplace_FindControl_Lists.Count * 25);

            string TempControlName_Find = $"{DynamicAddControlParams.EXCELFINDREPLACE_FIND_NAME_PREFIX}{DynamicAddControlParams.EXCELFINDREPLACE_NAME_INDEX}";
            string TempControlName_Replace = $"{DynamicAddControlParams.EXCELFINDREPLACE_REPLACE_NAME_PREFIX}{DynamicAddControlParams.EXCELFINDREPLACE_NAME_INDEX}";

            Point TempPoint_Find = new Point(ExcelFindTextBox.Location.X, ExcelFindTextBox.Location.Y + TempLocationY);
            Point TempPoint_Replace = new Point(ExcelReplaceBox.Location.X, ExcelReplaceBox.Location.Y + TempLocationY);

            TextBox TempFindTextBox = DynamicControlAdd<TextBox>(
                ExcelFindReplace_FindPanel,
                TempControlName_Find,
                TempPoint_Find,
                ExcelFindTextBox.Size);
            TextBox TempReplaceTextBox = DynamicControlAdd<TextBox>(
                ExcelFindReplace_ReplacePanel,
                TempControlName_Replace,
                TempPoint_Replace,
                ExcelReplaceBox.Size);

            DynamicAddControlParams.ExcelFindReplace_FindControl_Lists.Add(TempFindTextBox);
            DynamicAddControlParams.ExcelFindReplace_ReplaceControl_Lists.Add(TempReplaceTextBox);
        }

        private void ExcelReplaceFindRemove_Click(object sender, EventArgs e)
        {
            if(DynamicAddControlParams.ExcelFindReplace_FindControl_Lists.Count > 0)
            {
                Control LastControl = DynamicAddControlParams.ExcelFindReplace_FindControl_Lists.Last();
                ExcelFindReplace_FindPanel.Controls.Remove(LastControl);
                DynamicAddControlParams.ExcelFindReplace_FindControl_Lists.Remove(LastControl);
            }

            if(DynamicAddControlParams.ExcelFindReplace_ReplaceControl_Lists.Count > 0)
            {
                Control LastControl = DynamicAddControlParams.ExcelFindReplace_ReplaceControl_Lists.Last();
                ExcelFindReplace_ReplacePanel.Controls.Remove(LastControl);
                DynamicAddControlParams.ExcelFindReplace_ReplaceControl_Lists.Remove(LastControl);
            }
        }

        private void CompressIamge_NoCompress_CheckedChanged(object sender, EventArgs e)
        {
            this.CompressImage_OutPutImageFormat.Enabled = CompressIamge_NoCompress.Checked ||
                CompressImage_NotUseSelfExt.Checked;
        }

        private void Excel2Pdf_PrintScalePercent_Enable_CheckedChanged(object sender, EventArgs e)
        {
            this.Excel2Pdf_PrintScale_Row.Enabled = !Excel2Pdf_PrintScalePercent_Enable.Checked;
            //this.Excel2Pdf_PrintScale_Row.Text = string.Empty;
            this.Excel2Pdf_PrintScale_Col.Enabled = !Excel2Pdf_PrintScalePercent_Enable.Checked;
            //this.Excel2Pdf_PrintScale_Col.Text = string.Empty;
        }

        private void TargetTextBox_VScroll(object sender, EventArgs e)
        {
            TargetBoxIndex_number.Text = string.Empty;
            UpdateNameIndex();
            TargetBoxIndex_number.Invalidate();
        }

        private void MenuSearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if(GetTabPageIndex(MenuSearchTextBox.Text) >= 0 && (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab))
            {
                this.MainTabPage.SelectedIndex = GetTabPageIndex(MenuSearchTextBox.Text);
            }
        }

        private void 跳转到该功能ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(GetTabPageIndex(MenuSearchTextBox.Text) >= 0)
            {
                this.MainTabPage.SelectedIndex = GetTabPageIndex(MenuSearchTextBox.Text);
            }
        }

        private void AddPdfWaterMark_FullScren_CheckedChanged(object sender, EventArgs e)
        {
            this.AddPdfWaterMark_FullScren_HorDmi.Enabled = AddPdfWaterMark_FullScren.Checked;
            this.AddPdfWaterMark_FullScren_VerDmi.Enabled = AddPdfWaterMark_FullScren.Checked;
            this.WaterMarkImageHeight.Enabled = !AddPdfWaterMark_FullScren.Checked;
            this.WaterMarkImageWidth.Enabled = !AddPdfWaterMark_FullScren.Checked;
        }

        private void RemovePdfAddWaterImage_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(this.AddWaterMarkBackGroundImageFileName) && this.WaterMarkImage.Image != null)
            {
                this.AddWaterMarkBackGroundImageFileName = string.Empty;
                this.WaterMarkImageHeight.Text = string.Empty;
                this.WaterMarkImageWidth.Text = string.Empty;
                this.WaterMarkImage.Image.Dispose();
                this.WaterMarkImage.Image = null;
            }
        }

        private void 返回上一个功能ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(TabMenPagePreIndex != -1)
            {
                this.MainTabPage.SelectedIndex = TabMenPagePreIndex;
            }
        }

        private void MenuSearchTextBox_TextUpdate(object sender, EventArgs e)
        {
            // 使用 BeginInvoke 以确保不在 UI 线程中调用
            this.BeginInvoke(
                new Action(
                    () =>
                    {
                        string currentText = this.MenuSearchTextBox.Text.ToUpper();

                        // 先关闭下拉列表，以防止在更新项目时发生冲突
                        this.MenuSearchTextBox.DroppedDown = false;

                        // 清除现有的项
                        this.MenuSearchTextBox.Items.Clear();
                        MenuStringNew.Clear();

                        foreach(string MenuString in MenuStringOrign)
                        {
                            if(MenuString.Contains(currentText))
                            {
                                MenuStringNew.Add(MenuString);
                            }
                        }

                        this.MenuSearchTextBox.Items.AddRange(MenuStringNew.ToArray());

                        // 重新设置光标位置
                        this.MenuSearchTextBox.SelectionStart = currentText.Length;
                        Cursor = Cursors.Default;

                        // 使用 BeginInvoke 延迟打开下拉列表，避免与更新操作冲突
                        this.BeginInvoke(
                            new Action(
                                        () =>
                                        {
                                            if(this.MenuSearchTextBox.Text != string.Empty &&
                                                this.MenuSearchTextBox.Items.Count > 0)
                                            {
                                                this.MenuSearchTextBox.DroppedDown = true;
                                            }
                                        }));
                    }));
        }

        private void MenuSearchMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(MenuSearchMode.SelectedIndex == 0)
            {
                //切换搜索模式时候，清除原有item，然后将初始化时候的list数据重新添加进来
                this.MenuSearchTextBox.TextUpdate -= new System.EventHandler(this.MenuSearchTextBox_TextUpdate);
                this.MenuSearchTextBox.Items.Clear();
                this.MenuSearchTextBox.Items.AddRange(MenuStringOrign.ToArray());
            } else
            {
                this.MenuSearchTextBox.TextUpdate -= new System.EventHandler(this.MenuSearchTextBox_TextUpdate);
                this.MenuSearchTextBox.TextUpdate += new System.EventHandler(this.MenuSearchTextBox_TextUpdate);
            }
        }

        private void GetChinsesHanPinYin_CheckedChanged(object sender, EventArgs e)
        {
            DoubleTextAlignCheck.Checked = !GetChinsesHanPinYin.Checked;
            DoubleTextAlignCheck.Enabled = !GetChinsesHanPinYin.Checked;
            //if (GetChinsesHanPinYin.Checked==true)
            //{
            //    DoubleTextAlignCheck.Checked = false;
            //    DoubleTextAlignCheck.Enabled = false;
            //}
            //else
            //{
            //    DoubleTextAlignCheck.Checked = true;
            //    DoubleTextAlignCheck.Enabled = true;
            //}
        }

        private void ClickTurnImageConv_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { this.MainTabPage.SelectTab(GetTabPageIndex("压缩图片（格式转换）")); }

        private void SignDocumentUseEmbedImage_CheckedChanged(object sender, EventArgs e)
        { SignDocumentNeedCount.Maximum = 999; }

        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(string.IsNullOrEmpty(ChangeExamComputerNameHelpPdfFile) ||
                !File.Exists(ChangeExamComputerNameHelpPdfFile))
            {
                ChangeExamComputerNameHelpPdfFile = Path.GetTempFileName().Replace(".tmp", ".pdf");
                byte[] resourceBytes = (byte[])Util.GetEmbedResourceObj("ChangeExamComputerName");
                using(MemoryStream TempChangeReportItemHelpStream = new MemoryStream(resourceBytes))
                    using(FileStream TempPdfFileStream = File.Create(ChangeExamComputerNameHelpPdfFile))
                    {
                        TempChangeReportItemHelpStream.CopyTo(TempPdfFileStream);
                        TempPdfFileStream.Flush(); // 确保文件写入完成
                    }
            }
            Process.Start(ChangeExamComputerNameHelpPdfFile);
        }

        private void ExamComputerNames_MouseHover(object sender, EventArgs e)
        { MainToolsTip.SetToolTip(ExamComputerNames, ExamComputerNames.Text); }

        private void ExamComputerChangeName_MouseHover(object sender, EventArgs e)
        {
            //MainToolsTip.SetToolTip(ExamComputerChangeName, ExamComputerChangeName.Text);
        }

        private void ExamPicPathCombox_MouseHover(object sender, EventArgs e)
        { MainToolsTip.SetToolTip(ExamPicPathCombox, ExamPicPathCombox.Text); }

        private void ChangeExamCompNameSelect_CheckedChanged(object sender, EventArgs e)
        {
            ExamComputerNames.Enabled = ChangeExamCompNameSelect.Checked;
            ExamComputerChangeName.Enabled = ChangeExamCompNameSelect.Checked;
            GetRandomComputerNameRandLength.Enabled = ChangeExamCompNameSelect.Checked;
            GetRandomComputerNameButton.Enabled = ChangeExamCompNameSelect.Checked;

            ExamPicPathCombox.Enabled = !ChangeExamCompNameSelect.Checked;
            ExamTargetPicPathTextBox.Enabled = !ChangeExamCompNameSelect.Checked;
        }

        private void ExamPicPathCombox_SelectedIndexChanged(object sender, EventArgs e)
        { ExamTargetPicPathTextBox.Text = ExamPicPathCombox.SelectedItem.ToString(); }

        private void ExamComputerNames_SelectedIndexChanged(object sender, EventArgs e)
        { ExamComputerChangeName.Text = ExamComputerNames.SelectedItem.ToString(); }

        private void GetComputerNameSelf_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(Environment.MachineName))
            {
                Clipboard.SetText(Environment.MachineName);

                MessageBox.Show("已复制计算机名到剪贴板", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void SelectWatcherPath_Click(object sender, EventArgs e)
        {
            string WatcherPathName = Util.ShowSelectDialog(false, true, string.Empty, string.Empty).FirstOrDefault();

            if(!string.IsNullOrEmpty(WatcherPathName))
            {
                WatcherPathTextBox.Text = WatcherPathName;
                WatcherMakeTestFile.Enabled = !string.IsNullOrEmpty(WatcherPathTextBox.Text);
                if(TempSystemFileWatcher != null)
                {
                    TempSystemFileWatcher.Path = WatcherPathTextBox.Text;
                }
            }
        }

        private void StartPathWatcher_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(WatcherPathTextBox.Text))
            {
                this.LatestWatcherFileNameBox.Text = "已启动监控，等待中...";
                TempSystemFileWatcher = new FileSystemWatcher(WatcherPathTextBox.Text);
                TempSystemFileWatcher.NotifyFilter = NotifyFilters.FileName;
                TempSystemFileWatcher.EnableRaisingEvents = true;
                TempSystemFileWatcher.Created += TempSystemFileWatcher_Watch_JPG_TIFF_Created;
                (sender as Button).Enabled = false;
            }
        }

        private async void TempSystemFileWatcher_Watch_JPG_TIFF_Created(object sender, FileSystemEventArgs e)
        {
            string[] FilterExtensions = { ".jpg.err", ".jpg.han", ".tif.err", ".tif.han" };
            string[] TargetExtensions = { ".jpg", ".tif", ".bmp" };
            string TempFileExtension = Path.GetExtension(e.Name).ToLower();

            //使用array.exists判断TempFileExtension的值是否在FilterExtensions中，然后用equals比对返回结果
            bool isFiltered = Array.Exists(
                FilterExtensions,
                x =>
                {
                    return TempFileExtension.Equals(x, StringComparison.OrdinalIgnoreCase);
                });

            bool isTargetExtend = !isFiltered &&
                Array.Exists(
                    TargetExtensions,
                    x =>
                    {
                        return TempFileExtension.Equals(x, StringComparison.OrdinalIgnoreCase);
                    });

            if(isTargetExtend)
            {
                if(double.TryParse(WatcherTimeDelayOut.Value.ToString(), out double WaitTime))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(WaitTime));
                }

                if(File.Exists(e.FullPath))
                {
                    LatestWatcherFileNameBox.Invoke(
                        new Action(
                            () =>
                            {
                                LatestWatcherFileNameBox.Text = e.Name;
                            }));

                    if(WatcherSendNotify.Checked)
                    {
                        string OsNumber = Util.Check_11_Or_10_Version();
                        if(!string.IsNullOrEmpty(OsNumber) && int.Parse(OsNumber) >= 10)
                        {
                            ToastContentBuilder toastContentBuilder = new ToastContentBuilder();

                            toastContentBuilder
                                .AddText("补充功能-图片文件监控", AdaptiveTextStyle.HeaderNumeral)
                                .AddText(e.Name, AdaptiveTextStyle.BodySubtle)
                                .AddInlineImage(new Uri(e.FullPath))
                                .AddAppLogoOverride(
                                    new Uri(e.FullPath, UriKind.Relative),
                                    ToastGenericAppLogoCrop.Circle);

                            toastContentBuilder.Show(
                                x =>
                                {
                                    x.Group = "补充功能-图片文件监控";
                                    x.Tag = "补充功能-图片文件监控";
                                });
                        } else
                        {
                            MainNotify.ShowBalloonTip(0, "最新文件", e.Name, ToolTipIcon.Info);
                        }
                    }
                }
            }
        }

        private void LatestWatcherFileNameBox_MouseMove(object sender, MouseEventArgs e)
        { MainToolsTip.SetToolTip(LatestWatcherFileNameBox, LatestWatcherFileNameBox.Text); }

        private void WatcherPathTextBox_MouseHover(object sender, EventArgs e)
        { MainToolsTip.SetToolTip(WatcherPathTextBox, WatcherPathTextBox.Text); }


        private void WatcherMakeTestFile_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(WatcherPathTextBox.Text))
            {
                WatcherMakeTestFileHelper.RandomTempFileName = Path.Combine(
                    WatcherPathTextBox.Text,
                    Guid.NewGuid().ToString());
            }
        }

        private void MainNotify_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if(this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
            {
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            }
        }

        private void ExitApplication_Click(object sender, EventArgs e)
        {
            if(ChangeReportItemTempHelpPdfFile != string.Empty)
            {
                File.Delete(ChangeReportItemTempHelpPdfFile);
            }

            if(ChangeExamComputerNameHelpPdfFile != string.Empty)
            {
                File.Delete(ChangeExamComputerNameHelpPdfFile);
            }
            if(VoteMssTifHelpPdfFile != string.Empty)
            {
                File.Delete(VoteMssTifHelpPdfFile);
            }
            MainNotify.Visible = false;
            MainNotify.Icon = null;
            MainNotify.Dispose();
            Environment.Exit(0);
        }

        private void StopPathWatcher_Click(object sender, EventArgs e)
        {
            if(TempSystemFileWatcher != null)
            {
                TempSystemFileWatcher.EnableRaisingEvents = false;
                TempSystemFileWatcher.Dispose();
                this.LatestWatcherFileNameBox.Text = "未开启监控，等待中...";
                StartPathWatcher.Enabled = true;
            }
        }

        private void OpenMainForm_Click(object sender, EventArgs e)
        {
            if(this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
            {
                this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            }
        }

        private void WatcherMakeTestFile_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.WatcherMakeTestFileMenu.Show(WatcherMakeTestFile, new Point(0, WatcherMakeTestFile.Height));
            }
        }

        private void MakeJPGTestFile_Click(object sender, EventArgs e) { WatcherMakeTestFileHelper.MakeJpgFile(); }

        private void MakeJPGErrTestFile_Click(object sender, EventArgs e)
        { WatcherMakeTestFileHelper.MakeJpgErrFile(); }

        private void MakeJPGHanTestFile_Click(object sender, EventArgs e)
        { WatcherMakeTestFileHelper.MakeJpgHanFile(); }

        private void MakeTIFTestFile_Click(object sender, EventArgs e) { WatcherMakeTestFileHelper.MakeTifFile(); }

        private void MakeTIFErrTestFile_Click(object sender, EventArgs e)
        { WatcherMakeTestFileHelper.MakeTifErrFile(); }

        private void MakeTIFHanTestFile_Click(object sender, EventArgs e)
        { WatcherMakeTestFileHelper.MakeTifHanFile(); }

        private void MakeALLFiles_Click(object sender, EventArgs e)
        {
            WatcherMakeTestFileHelper.MakeJpgFile();
            WatcherMakeTestFileHelper.MakeJpgErrFile();
            WatcherMakeTestFileHelper.MakeJpgHanFile();
            WatcherMakeTestFileHelper.MakeTifFile();
            WatcherMakeTestFileHelper.MakeTifErrFile();
            WatcherMakeTestFileHelper.MakeTifHanFile();
        }

        private void ChangeMssItemColumn_CheckedChanged(object sender, EventArgs e)
        { ResetMssItemColumn.Enabled = !ChangeMssItemColumn.Checked; }

        private void ResetMssItemColumn_CheckedChanged(object sender, EventArgs e)
        { ChangeMssItemColumn.Enabled = !ResetMssItemColumn.Checked; }

        private void MoveToConvertTif_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { this.MainTabPage.SelectTab(GetTabPageIndex("压缩图片（格式转换）")); }

        private void SetGrayCheckBox_CheckedChanged(object sender, EventArgs e)
        { MoveToConvertTif.Visible = SetGrayCheckBox.Checked; }

        private void CompressImage_OutPutImageFormat_SelectedIndexChanged(object sender, EventArgs e)
        { VoteMssTifHelpFile.Visible = CompressImage_OutPutImageFormat.Text == "TIF"; }

        private void VoteMssTifHelpFile_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(string.IsNullOrEmpty(VoteMssTifHelpPdfFile) || !File.Exists(VoteMssTifHelpPdfFile))
            {
                VoteMssTifHelpPdfFile = Path.GetTempFileName().Replace(".tmp", ".pdf");
                byte[] resourceBytes = (byte[])Util.GetEmbedResourceObj("VoteMssTif");
                using(MemoryStream TempChangeReportItemHelpStream = new MemoryStream(resourceBytes))
                    using(FileStream TempPdfFileStream = File.Create(VoteMssTifHelpPdfFile))
                    {
                        TempChangeReportItemHelpStream.CopyTo(TempPdfFileStream);
                        TempPdfFileStream.Flush(); // 确保文件写入完成
                    }
            }
            Process.Start(VoteMssTifHelpPdfFile);
        }

        private void GetAllExamName_Click(object sender, EventArgs e)
        {
            try
            {
                if(ExamReportRowMode_ExamList.Items.Count > 0)
                {
                    string ItemSplitsString = string.Join(
                        "|",
                        ExamReportRowMode_ExamList.Items.Cast<object>().Select(item => item.ToString()).ToList());
                    if(ItemSplitsString != string.Empty)
                    {
                        Clipboard.SetText(ItemSplitsString);

                        MessageBox.Show("已复制所有项目，请自行粘贴", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GetAllExamSelector_Click(object sender, EventArgs e)
        {
            try
            {
                if(ExamItemsSelectorsCombox.Items.Count > 0)
                {
                    string SelectorSplitsString = string.Join(
                        "|",
                        ExamItemsSelectorsCombox.Items.Cast<object>().Select(item => item.ToString()).ToList());
                    if(SelectorSplitsString != string.Empty)
                    {
                        Clipboard.SetText(SelectorSplitsString);

                        MessageBox.Show("已复制所选项目选项，请自行粘贴", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            } catch(Exception)
            {
                throw;
            }
        }

        private void GetAllItemSplit_Click(object sender, EventArgs e)
        { GetExamNameAndSelectorMenu.Show(GetAllItemSplit, new Point(0, GetAllItemSplit.Height)); }

        private void ExamReportPara_RowMode_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Middle)
            {
                if(ExamReportMiddleClickStatus)
                {
                    this.ExamReportRowMode_Row_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
                    this.ExamReportRowMode_Col_Start_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
                    this.ExamReportRowMode_Col_End_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
                    this.ExamReportRowMode_ExamList.DropDownStyle = ComboBoxStyle.DropDownList;

                    this.ExamReportColMode_Col_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
                    this.ExamReportColMode_Row_Start_Combox.DropDownStyle = ComboBoxStyle.DropDownList;
                    this.ExamReportColMode_Row_End_Combox.DropDownStyle = ComboBoxStyle.DropDownList;

                    ExamReportMiddleClickStatus = false;
                } else
                {
                    this.ExamReportRowMode_Row_Combox.DropDownStyle = ComboBoxStyle.DropDown;
                    this.ExamReportRowMode_Col_Start_Combox.DropDownStyle = ComboBoxStyle.DropDown;
                    this.ExamReportRowMode_Col_End_Combox.DropDownStyle = ComboBoxStyle.DropDown;
                    this.ExamReportRowMode_ExamList.DropDownStyle = ComboBoxStyle.DropDown;

                    this.ExamReportColMode_Col_Combox.DropDownStyle = ComboBoxStyle.DropDown;
                    this.ExamReportColMode_Row_Start_Combox.DropDownStyle = ComboBoxStyle.DropDown;
                    this.ExamReportColMode_Row_End_Combox.DropDownStyle = ComboBoxStyle.DropDown;

                    ExamReportMiddleClickStatus = true;
                }
            }
        }

        private void ExamReportParaDataFormat_Combox_MouseHover(object sender, EventArgs e)
        {
            string SetToolTipText = ExamReportParaDataFormat_Combox.Text;
            if(ExamReportParaDataFormat_Combox.SelectedIndex == ExamReportParaDataFormat_Combox.Items.Count - 1)
            {
                SetToolTipText += "（只对行模式生效，且布局为 票数|比例 的报表才有效，执行完毕后，如果需要修改小数点位数，请在系统中进行批量位数修改）";
            }
            MainToolsTip.SetToolTip(ExamReportParaDataFormat_Combox, SetToolTipText);
        }

        private void AddPdfWater_CustomPostion_CheckedChanged(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Enabled = AddPdfWater_CustomPostion.Checked;
            AddPdfWater_CustomPostion_Y.Enabled = AddPdfWater_CustomPostion.Checked;
        }

        private void AddPdfWater_GetSelectPdfPointInfo_Click(object sender, EventArgs e)
        {
            if(PdfWaterMarkFileList.SelectedItems.Count == 1)
            {
                if(!string.IsNullOrEmpty(PdfWaterMarkFileList.SelectedItems[0].Text))
                {
                    Task.Run(
                        () =>
                        {
                            try
                            {
                                Aspose.Pdf.Facades.PdfFileInfo TempSelectPdfInfo = new Aspose.Pdf.Facades.PdfFileInfo(
                                    PdfWaterMarkFileList.SelectedItems[0].Text);

                                float FirstPageWidth = TempSelectPdfInfo.GetPageWidth(1);
                                float FirstPageHeight = TempSelectPdfInfo.GetPageHeight(1);

                                Aspose.Pdf.Point TopLeftPoint = new Aspose.Pdf.Point(15, FirstPageHeight - 15);
                                Aspose.Pdf.Point TopCenterPoint = new Aspose.Pdf.Point(
                                    (FirstPageWidth / 2) - 35,
                                    FirstPageHeight - 15);
                                Aspose.Pdf.Point TopRightPoint = new Aspose.Pdf.Point(
                                    FirstPageWidth - 100,
                                    FirstPageHeight - 15);

                                Aspose.Pdf.Point CenterLeftPoint = new Aspose.Pdf.Point(15, FirstPageHeight / 2);
                                Aspose.Pdf.Point CenterMiddlePoint = new Aspose.Pdf.Point(
                                    (FirstPageWidth / 2) - 35,
                                    FirstPageHeight / 2);
                                Aspose.Pdf.Point CenterRightPoint = new Aspose.Pdf.Point(
                                    FirstPageWidth - 100,
                                    FirstPageHeight / 2);

                                Aspose.Pdf.Point BottomLeftPoint = new Aspose.Pdf.Point(15, 15);
                                Aspose.Pdf.Point BottomCenterPoint = new Aspose.Pdf.Point((FirstPageWidth / 2) - 35, 15);
                                Aspose.Pdf.Point BottomRightPoint = new Aspose.Pdf.Point(FirstPageWidth - 100, 15);


                                PdfWaterMarkPreCustomPostionValue = (Convert.ToInt32(TopLeftPoint.X), Convert.ToInt32(
                                    TopLeftPoint.Y),
                                                                     Convert.ToInt32(TopCenterPoint.X), Convert.ToInt32(
                                    TopCenterPoint.Y),
                                                                     Convert.ToInt32(TopRightPoint.X), Convert.ToInt32(
                                    TopRightPoint.Y),
                                                                     Convert.ToInt32(CenterLeftPoint.X), Convert.ToInt32(
                                    CenterLeftPoint.Y),
                                                                     Convert.ToInt32(CenterMiddlePoint.X), Convert.ToInt32(
                                    CenterMiddlePoint.Y),
                                                                     Convert.ToInt32(CenterRightPoint.X), Convert.ToInt32(
                                    CenterRightPoint.Y),
                                                                     Convert.ToInt32(BottomLeftPoint.X), Convert.ToInt32(
                                    BottomLeftPoint.Y),
                                                                     Convert.ToInt32(BottomCenterPoint.X), Convert.ToInt32(
                                    BottomCenterPoint.Y),
                                                                     Convert.ToInt32(BottomRightPoint.X), Convert.ToInt32(
                                    BottomRightPoint.Y));

                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            PdfWaterSetPreCustomMenu.Show(
                                                AddPdfWater_GetSelectPdfPointInfo,
                                                new Point(0, AddPdfWater_GetSelectPdfPointInfo.Height));
                                        }));
                            } catch(Exception ex)
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
                                        }));
                            }
                        });
                }
            }
        }

        private void PdfWaterMarkPrePostion_TopLeft_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_TopLeft_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_TopLeft_Y;
        }

        private void PdfWaterMarkPrePostion_TopMiddle_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_TopCenter_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_TopCenter_Y;
        }

        private void PdfWaterMarkPrePostion_TopRight_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_TopRight_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_TopRight_Y;
        }

        private void PdfWaterMarkPrePostion_CenterLeft_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_CenterLeft_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_CenterLeft_Y;
        }

        private void PdfWaterMarkPrePostion_CenterMiddle_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_CenterMiddle_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_CenterMiddle_Y;
        }

        private void PdfWaterMarkPrePostion_CenterRight_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_CenterRight_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_CenterRight_Y;
        }

        private void PdfWaterMarkPrePostion_BottomLeft_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_BottomLeft_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_BottomLeft_Y;
        }

        private void PdfWaterMarkPrePostion_BottomMiddle_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_BottomCenter_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_BottomCenter_Y;
        }

        private void PdfWaterMarkPrePostion_BottomRight_Click(object sender, EventArgs e)
        {
            AddPdfWater_CustomPostion_X.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_BottomRight_X;
            AddPdfWater_CustomPostion_Y.Value = PdfWaterMarkPreCustomPostionValue.PdfWaterCustomPos_BottomRight_Y;
        }

        private void EmojiPicture_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                if(IsClickEmojiClick)
                {
                    EmojiPicture.Image = (Image)Util.GetEmbedResourceObj("大头儿子");

                    IsClickEmojiClick = false;

                    var DefaultAudioStatus = Util.GetSystemDefaultAudioStatus();

                    if(DefaultAudioStatus.AudioMute)
                    {
                        Util.SetSystemAudioVolume(false, float.Parse("0.3"));
                    }

                    Util.PlayMp3WithStream(
                        "dtez",
                        status =>
                        {
                        });
                } else
                {
                    EmojiPicture.Image = (Image)Util.GetEmbedResourceObj("懒得喷");

                    IsClickEmojiClick = true;
                }
                money_small.Enabled = !IsClickEmojiClick;
                money_big.Enabled = !IsClickEmojiClick;
            }
        }


        private async Task SetLimitControl()
        {
            XDocument LimitControlXml = null;


            string LimitXmlStringByRemote = string.Empty;

            string VFSOFTLicenseKey = Util.GetEmbedResourceObj("LocalXMLFileName").ToString();

            string[] EmebedLimitButtonNames =
            {
                "DecodeDataMarix",
                "ConvertTextButton",
                "ConvertWord2ImageButton",
                "SelectTwainDevice",
                "SetTwainDevice",
                "StartScanButton",
                "StartInsert",
                "SignDocumentReplaceStart",
                "ExamReportChange",
                "ChangeExamReportParaButton",
                "VoteTable_Render",
                "gen_card_button",
                "InsertExamItemAndSelctor_StartInsert",
                "ChangeExamTableNo_Start"
            };

            string[] EmebedLimitGroupBoxNames =
            {
                "groupBox15",
                "groupBox46",
                "groupBox60",
                "groupBox28",
                "groupBox67",
                "groupBox69",
                "groupBox41"
            };

            try
            {
                LimitXmlStringByRemote = await Task.Run(() => Util.DownloadString(VersionInfo.LimitControlXmlUrl));
                if(File.Exists(VFSOFTLicenseKey))
                {
                    LimitControlXml = XDocument.Load(VFSOFTLicenseKey);
                } else if(!File.Exists(VFSOFTLicenseKey) && !string.IsNullOrEmpty(LimitXmlStringByRemote))
                {
                    LimitControlXml = XDocument.Parse(LimitXmlStringByRemote);
                }

                if(LimitControlXml != null)
                {
                    IEnumerable<XElement> RemoteLimitButtonNames = LimitControlXml.Descendants("Application")
                        .Elements("HiddenButton")
                        .Elements("Button");
                    IEnumerable<XElement> RemoteLimitGroupNames = LimitControlXml.Descendants("Application")
                        .Elements("HiddenGroupBox")
                        .Elements("GroupBox");

                    foreach(XElement TempElement in RemoteLimitButtonNames)
                    {
                        GetFormMembers(typeof(Button))
                            .ForEach(
                                x =>
                                {
                                    Button TempButton = x as Button;

                                    if(TempButton.Name == TempElement.Value)
                                    {
                                        bool HiddenStatus = bool.Parse(TempElement.Attribute("Hidden").Value);
                                        bool UseStatus = bool.Parse(TempElement.Attribute("CanUse").Value);

                                        TempButton.Visible = !HiddenStatus;

                                        if(!HiddenStatus && !UseStatus)
                                        {
                                            TempButton.Enabled = false;
                                            TempButton.Text = TempElement.Attribute("NotUseMessAge").Value;
                                        }
                                    }
                                });
                    }

                    foreach(XElement TempElement in RemoteLimitGroupNames)
                    {
                        GetFormMembers(typeof(GroupBox))
                            .ForEach(
                                x =>
                                {
                                    GroupBox TempGroupBox = x as GroupBox;

                                    if(TempGroupBox.Name == TempElement.Value)
                                    {
                                        bool HiddenStatus = bool.Parse(TempElement.Attribute("Hidden").Value);
                                        bool UseStatus = bool.Parse(TempElement.Attribute("CanUse").Value);

                                        TempGroupBox.Visible = !HiddenStatus;

                                        if(!HiddenStatus && !UseStatus)
                                        {
                                            TempGroupBox.Enabled = false;
                                            TempGroupBox.Text = TempElement.Attribute("NotUseMessAge").Value;
                                        }
                                    }
                                });
                    }
                } else
                {
                    foreach(string EmebedLimitButtonName in EmebedLimitButtonNames)
                    {
                        GetFormMembers(typeof(Button))
                            .ForEach(
                                x =>
                                {
                                    Button TempButton = x as Button;

                                    if(TempButton.Name == EmebedLimitButtonName)
                                    {
                                        TempButton.Visible = false;
                                    }
                                });
                    }

                    foreach(string EmebedLimitGroupBoxName in EmebedLimitGroupBoxNames)
                    {
                        GetFormMembers(typeof(GroupBox))
                            .ForEach(
                                x =>
                                {
                                    GroupBox TempGroupBox = x as GroupBox;

                                    if(TempGroupBox.Name == EmebedLimitGroupBoxName)
                                    {
                                        TempGroupBox.Visible = false;
                                    }
                                });
                    }
                }
            } catch(XmlException)
            {
                foreach(string EmebedLimitButtonName in EmebedLimitButtonNames)
                {
                    GetFormMembers(typeof(Button))
                        .ForEach(
                            x =>
                            {
                                Button TempButton = x as Button;

                                if(TempButton.Name == EmebedLimitButtonName)
                                {
                                    TempButton.Visible = false;
                                }
                            });
                }

                foreach(string EmebedLimitGroupBoxName in EmebedLimitGroupBoxNames)
                {
                    GetFormMembers(typeof(GroupBox))
                        .ForEach(
                            x =>
                            {
                                GroupBox TempGroupBox = x as GroupBox;

                                if(TempGroupBox.Name == EmebedLimitGroupBoxName)
                                {
                                    TempGroupBox.Visible = false;
                                }
                            });
                }
            } catch(Exception)
            {
                if(File.Exists(VFSOFTLicenseKey))
                {
                    LimitControlXml = XDocument.Load(VFSOFTLicenseKey);
                }
                if(LimitControlXml != null)
                {
                    IEnumerable<XElement> RemoteLimitButtonNames = LimitControlXml.Descendants("Application")
                        .Elements("HiddenButton")
                        .Elements("Button");
                    IEnumerable<XElement> RemoteLimitGroupNames = LimitControlXml.Descendants("Application")
                        .Elements("HiddenGroupBox")
                        .Elements("GroupBox");

                    foreach(XElement TempElement in RemoteLimitButtonNames)
                    {
                        GetFormMembers(typeof(Button))
                            .ForEach(
                                x =>
                                {
                                    Button TempButton = x as Button;

                                    if(TempButton.Name == TempElement.Value)
                                    {
                                        bool HiddenStatus = bool.Parse(TempElement.Attribute("Hidden").Value);
                                        bool UseStatus = bool.Parse(TempElement.Attribute("CanUse").Value);

                                        TempButton.Visible = !HiddenStatus;

                                        if(!HiddenStatus && !UseStatus)
                                        {
                                            TempButton.Enabled = false;
                                            TempButton.Text = TempElement.Attribute("NotUseMessAge").Value;
                                        }
                                    }
                                });
                    }

                    foreach(XElement TempElement in RemoteLimitGroupNames)
                    {
                        GetFormMembers(typeof(GroupBox))
                            .ForEach(
                                x =>
                                {
                                    GroupBox TempGroupBox = x as GroupBox;

                                    if(TempGroupBox.Name == TempElement.Value)
                                    {
                                        bool HiddenStatus = bool.Parse(TempElement.Attribute("Hidden").Value);
                                        bool UseStatus = bool.Parse(TempElement.Attribute("CanUse").Value);

                                        TempGroupBox.Visible = !HiddenStatus;

                                        if(!HiddenStatus && !UseStatus)
                                        {
                                            TempGroupBox.Enabled = false;
                                            TempGroupBox.Text = TempElement.Attribute("NotUseMessAge").Value;
                                        }
                                    }
                                });
                    }
                } else
                {
                    foreach(string EmebedLimitButtonName in EmebedLimitButtonNames)
                    {
                        GetFormMembers(typeof(Button))
                            .ForEach(
                                x =>
                                {
                                    Button TempButton = x as Button;

                                    if(TempButton.Name == EmebedLimitButtonName)
                                    {
                                        TempButton.Visible = false;
                                    }
                                });
                    }

                    foreach(string EmebedLimitGroupBoxName in EmebedLimitGroupBoxNames)
                    {
                        GetFormMembers(typeof(GroupBox))
                            .ForEach(
                                x =>
                                {
                                    GroupBox TempGroupBox = x as GroupBox;

                                    if(TempGroupBox.Name == EmebedLimitGroupBoxName)
                                    {
                                        TempGroupBox.Visible = false;
                                    }
                                });
                    }
                }
            }
        }

        private void TempButton_Click(object sender, EventArgs e) { throw new NotImplementedException(); }

        private async Task GetApplicationStartLimit()
        {
            string LocalXmlFile = Util.GetEmbedResourceObj("LocalXMLFileName").ToString();

            XDocument LimitControlXml = null;

            string LimitXmlStringByRemote = string.Empty;

            LimitXmlStringByRemote = await Task.Run(() => Util.DownloadString(VersionInfo.LimitControlXmlUrl));

            if(File.Exists(LocalXmlFile))
            {
                LimitControlXml = XDocument.Load(LocalXmlFile);
            } else if(!File.Exists(LocalXmlFile) && !string.IsNullOrEmpty(LimitXmlStringByRemote))
            {
                LimitControlXml = XDocument.Parse(LimitXmlStringByRemote);
            }

            if(LimitControlXml != null)
            {
                XElement TempApplicationOtherControl = LimitControlXml.Descendants("Application")
                    .Elements("ApplicationOtherControl")
                    .FirstOrDefault();
                XElement ApplicationLimit_StartControl = TempApplicationOtherControl.Element("StartControl");
                XElement ApplicationLimit_LimitApplicationOpen = ApplicationLimit_StartControl.Element(
                    "LimitApplicationOpen");//限制程序是否可以打开
                XElement ApplicationLimit_LimitApplicationOpen_DirectLimitOpen = ApplicationLimit_LimitApplicationOpen.Element(
                    "DirectLimitOpen");//限制的方式-直接限制
                XElement ApplicationLimit_LimitApplicationOpen_ExpireDateLimit = ApplicationLimit_LimitApplicationOpen.Element(
                    "ExpireDateLimit");//限制的方式-日期限制

                XElement ApplicationLimit_VersionLimit = ApplicationLimit_StartControl.Element("VersionLimit");//程序版本限制
                XElement ApplicationLimit_VersionLimit_WhiteList = ApplicationLimit_VersionLimit.Element("WhiteList");//版本白名单

                //启用了程序打开的限制
                if(bool.Parse(ApplicationLimit_StartControl.Attribute("Enable").Value))
                {
                    //是否限制了程序打开，控制程序打开逻辑，和版本限制区分开
                    if(bool.Parse(ApplicationLimit_LimitApplicationOpen.Attribute("Enable").Value) &&
                        bool.Parse(ApplicationLimit_VersionLimit.Attribute("Enable").Value) == false)
                    {
                        if(bool.Parse(ApplicationLimit_LimitApplicationOpen_DirectLimitOpen.Attribute("Enable").Value))
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        this.Visible = !bool.Parse(
                                            ApplicationLimit_LimitApplicationOpen_DirectLimitOpen.Attribute(
                                                        "HiddenForm")
                                                        .Value);
                                        MessageBox.Show(
                                            ApplicationLimit_LimitApplicationOpen_DirectLimitOpen.Value,
                                            "提示",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Stop);
                                        Environment.Exit(0);
                                    }));
                        } else if(bool.Parse(
                                   ApplicationLimit_LimitApplicationOpen_ExpireDateLimit.Attribute("Enable").Value))
                        {
                            DateTime MachineLocalDate = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd"));
                            DateTime ExpireDateTime = DateTime.Parse(
                                ApplicationLimit_LimitApplicationOpen_ExpireDateLimit.Attribute("ExpireDate").Value);

                            if(MachineLocalDate > ExpireDateTime)
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            this.Visible = !bool.Parse(
                                                ApplicationLimit_LimitApplicationOpen_ExpireDateLimit.Attribute(
                                                            "HiddenForm")
                                                            .Value);
                                            MessageBox.Show(
                                                ApplicationLimit_LimitApplicationOpen_ExpireDateLimit.Value,
                                                "提示",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Stop);
                                            Environment.Exit(0);
                                        }));
                            }
                        }
                    } else if(bool.Parse(ApplicationLimit_LimitApplicationOpen.Attribute("Enable").Value) == false &&
                        bool.Parse(ApplicationLimit_VersionLimit.Attribute("Enable").Value))
                    {
                        if(bool.Parse(ApplicationLimit_VersionLimit_WhiteList.Attribute("Enable").Value))
                        {
                            string[] VersionWhiteList = ApplicationLimit_VersionLimit_WhiteList.Attribute("VersionList")
                                .Value
                                .Split('|');

                            if(!VersionWhiteList.Contains(VersionInfo.VersionNumber))
                            {
                                this.BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            this.Visible = !bool.Parse(
                                                ApplicationLimit_VersionLimit_WhiteList.Attribute("HiddenForm").Value);
                                            MessageBox.Show(
                                                ApplicationLimit_VersionLimit_WhiteList.Value,
                                                "提示",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Stop);
                                            Environment.Exit(0);
                                        }));
                            }
                        }
                    }
                }
            }
        }


        private async Task GetOnlineApplicationCMD()
        {
            string LocalXmlFile = Util.GetEmbedResourceObj("LocalXMLFileName").ToString();

            XDocument RemoteDocumentXml = null;

            string RemoteDocumentString = string.Empty;

            RemoteDocumentString = await Task.Run(() => Util.DownloadString(VersionInfo.LimitControlXmlUrl));

            if(File.Exists(LocalXmlFile))
            {
                RemoteDocumentXml = XDocument.Load(LocalXmlFile);
            } else if(!File.Exists(LocalXmlFile) && !string.IsNullOrEmpty(RemoteDocumentString))
            {
                RemoteDocumentXml = XDocument.Parse(RemoteDocumentString);
            }

            if(RemoteDocumentXml != null)
            {
                XElement TempApplication = RemoteDocumentXml.Descendants("Application")
                    .Elements("ApplicationOtherControl")
                    .FirstOrDefault();

                if(TempApplication != default)
                {
                    XElement TempApplication_RunTimeControl = TempApplication.Element("RuntimeControl");
                    XElement TempApplication_RunTimeControl_ExitApplication = TempApplication_RunTimeControl.Element(
                        "ExitApplication");
                    XElement TempApplication_RunTimeControl_DisableApplication = TempApplication_RunTimeControl.Element(
                        "DisableApplication");

                    if(bool.Parse(TempApplication.Element("StartControl").Attribute("Enable").Value) &&
                        bool.Parse(TempApplication_RunTimeControl.Attribute("Enable").Value))
                    {
                        if(bool.Parse(TempApplication_RunTimeControl_ExitApplication.Attribute("Enable").Value))
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        if(!RuntimeControl_SendMessage_Status)
                                        {
                                            RuntimeControl_SendMessage_Status = true;
                                            this.Visible = !bool.Parse(
                                                TempApplication_RunTimeControl_ExitApplication.Attribute("HiddenForm")
                                                    .Value);
                                            MessageBox.Show(
                                                TempApplication_RunTimeControl_ExitApplication.Value,
                                                "提示",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Stop);
                                            Environment.Exit(0);
                                        }
                                    }));
                        } else
                        {
                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        bool EnableStatus = !bool.Parse(
                                            TempApplication_RunTimeControl_DisableApplication.Attribute("Enable").Value);
                                        MainTabPage.Enabled = EnableStatus;
                                        menuStrip1.Enabled = EnableStatus;
                                        statusStrip1.Enabled = EnableStatus;
                                        string DisableTitle = VersionInfo.VersionTitle;
                                        if(!EnableStatus)
                                        {
                                            DisableTitle += TempApplication_RunTimeControl_DisableApplication.Value;
                                        }
                                        this.Text = DisableTitle;
                                    }));
                        }
                    }
                }
            }
        }

        private void ExamPicture_UseOcr_CheckedChanged(object sender, EventArgs e)
        {
            Exam_Picture_OcrMark.Enabled = ExamPicture_UseOcr.Checked;
            ExamPictureOcrMatchString.Enabled = ExamPicture_UseOcr.Checked;
            ExamPictureDownloadOcrModel.Visible = ExamPicture_UseOcr.Checked;
        }

        private void ExamPictureDownloadOcrModel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_OcrMode").ToString()); }


        private async void RefreshExamBaseInfoTableView()
        {
            try
            {
                if(!string.IsNullOrEmpty(InsertExam_SelectFilePath.Text) && File.Exists(InsertExam_SelectFilePath.Text))
                {
                    string ExamFileExtension = Path.GetExtension(InsertExam_SelectFilePath.Text);
                    DataTable TempExamBaseInfoDataTable = await Task.Run(
                        () =>
                        {
                            DataTable TempDataTable = new DataTable();

                            if(ExamFileExtension==".mdb")
                            {
                                TempDataTable =  Util.GetExamBaseInfo(InsertExam_SelectFilePath.Text,new AccessDatabaseFactory());
                            }
                            else if(ExamFileExtension==".db")
                            {
                                TempDataTable = Util.GetExamBaseInfo(InsertExam_SelectFilePath.Text, new SqliteDatabaseFactory());
                            }

                            return TempDataTable;
                        });
                    InsertExamTableView.DataSource = TempExamBaseInfoDataTable;

                    if(TempExamBaseInfoDataTable != null && TempExamBaseInfoDataTable.Rows.Count > 0)
                    {
                        InsertExamTableView.Sort(InsertExamTableView.Columns[0], ListSortDirection.Ascending);
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void RefreshExamUnitInfoTableView()
        {
            try
            {
                if (!string.IsNullOrEmpty(ExamDataPakOrder_Path.Text) && File.Exists(ExamDataPakOrder_Path.Text))
                {
                    string ExamFileExtension = Path.GetExtension(ExamDataPakOrder_Path.Text);
                    DataTable TempExamBaseInfoDataTable = await Task.Run(
                        () =>
                        {
                            DataTable TempDataTable = new DataTable();

                            if (ExamFileExtension == ".mdb")
                            {
                                TempDataTable = Util.GetExamUnitInfo(ExamDataPakOrder_Path.Text, new AccessDatabaseFactory());
                            }
                            else if (ExamFileExtension == ".db")
                            {
                                TempDataTable = Util.GetExamUnitInfo(ExamDataPakOrder_Path.Text, new SqliteDatabaseFactory());
                            }

                            return TempDataTable;
                        });

                    ExamDataPakOrder_UnitListView.DataSource = TempExamBaseInfoDataTable;

                    if (TempExamBaseInfoDataTable != null && TempExamBaseInfoDataTable.Rows.Count > 0)
                    {
                        // 记录原始顺序，用于高亮“已调整过顺序”的行
                        if (!TempExamBaseInfoDataTable.Columns.Contains("__orig_order"))
                        {
                            TempExamBaseInfoDataTable.Columns.Add("__orig_order", typeof(int));
                        }
                        for (int i = 0; i < TempExamBaseInfoDataTable.Rows.Count; i++)
                        {
                            TempExamBaseInfoDataTable.Rows[i]["__orig_order"] = i + 1;
                        }

                        ExamDataPakOrder_MarkChangedRows();
                        if (ExamDataPakOrder_UnitListView.Columns.Contains("__orig_order"))
                        {
                            ExamDataPakOrder_UnitListView.Columns["__orig_order"].Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //private void InsertExamItemAndSelctor_StartInsert_Click(object sender, EventArgs e)
        //{
        //    if(!string.IsNullOrEmpty(InsertExam_SelectFilePath.Text) &&
        //        !string.IsNullOrEmpty(InsertExamBaseInfo_ExamTableName.Text))
        //    {
        //        string ExamFileExtension = Path.GetExtension(InsertExam_SelectFilePath.Text);
        //        string GetExamTableName = $"SELECT ExamName FROM Exam_EssInfo WHERE ExamName='{InsertExamBaseInfo_ExamTableName.Text}'";

        //        var (TempExamData, TempConnection) = Util.GetAccessDataByOne(
        //            InsertExam_SelectFilePath.Text,
        //            GetExamTableName);

        //        if(TempExamData != null && TempExamData.ToString() == InsertExamBaseInfo_ExamTableName.Text)
        //        {
        //            this.BeginInvoke(
        //                new Action(
        //                    () =>
        //                    {
        //                        MessageBox.Show("该测评表名称已存在，无法添加", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        //                    }));
        //        } else
        //        {
        //            List<TextBox> TempExamNames = InsertExam_ExamSet.Controls
        //                .OfType<TextBox>()
        //                .Where(x => !string.IsNullOrEmpty(x.Text))
        //                .ToList();

        //            List<NumericUpDown> TempExamScores = InsertExam_ExamSet.Controls
        //                .OfType<NumericUpDown>()
        //                .Where(x => x.Value >= -1)
        //                .ToList();

        //            List<TextBox> TempExamSelectorNames = InsertExam_ExamSelectorSet.Controls
        //                .OfType<TextBox>()
        //                .Where(x => !string.IsNullOrEmpty(x.Text))
        //                .ToList();

        //            List<NumericUpDown> TempExamSelectorPerValues = InsertExam_ExamSelectorSet.Controls
        //                .OfType<NumericUpDown>()
        //                .Where(x => x.Value >= -1)
        //                .ToList();

        //            List<CheckBox> TempExamSelectorAvgValues = InsertExam_ExamSelectorSet.Controls
        //                .OfType<CheckBox>()
        //                .ToList();


        //            JObject TempExamInfoJson = new JObject();

        //            for(int TempIndex = 0; TempIndex < TempExamNames.Count; TempIndex++)
        //            {
        //                string TempExamName = TempExamNames[TempIndex].Text;
        //                string TempExamScore = TempExamScores[TempIndex].Value.ToString();

        //                JObject TempExamNameAndScore = new JObject()
        //                {
        //                    ["Type"] = "项目",
        //                    ["Score"] = TempExamScore,
        //                    ["Selectors"] = new JObject()
        //                };

        //                for(int TempIndex_Selector = 0; TempIndex_Selector < TempExamSelectorNames.Count; TempIndex_Selector++)
        //                {
        //                    string TempSelectorName = TempExamSelectorNames[TempIndex_Selector].Text;
        //                    string TempSelectorScorePer = TempExamSelectorPerValues[TempIndex_Selector].Value.ToString();
        //                    bool TempSelectorCalcAvg = TempExamSelectorAvgValues[TempIndex_Selector].Checked;

        //                    JObject TempExamSelectorsInfo = new JObject()
        //                    {
        //                        ["Type"] = "选项",
        //                        ["ScorePer"] = TempSelectorScorePer,
        //                        ["CalcAvg"] = TempSelectorCalcAvg
        //                    };
        //                    (TempExamNameAndScore["Selectors"] as JObject).Add(TempSelectorName, TempExamSelectorsInfo);
        //                }
        //                JObject InvalidObject = new JObject()
        //                {
        //                    ["Type"] = "选项",
        //                    ["ScorePer"] = InsertExam_SelectorInvalid_ScorePer.Value.ToString(),
        //                    ["CalcAvg"] = InsertExam_SelectorInvalid_CacAvg.Checked
        //                };
        //                (TempExamNameAndScore["Selectors"] as JObject).Add("无效", InvalidObject);

        //                JObject AbsTainObject = new JObject()
        //                {
        //                    ["Type"] = "选项",
        //                    ["ScorePer"] = InsertExam_SelectorAbstain_ScorePer.Value.ToString(),
        //                    ["CalcAvg"] = InsertExam_SelectorAbstain_CalcAvg.Checked
        //                };
        //                (TempExamNameAndScore["Selectors"] as JObject).Add("弃权", AbsTainObject);

        //                TempExamInfoJson.Add(TempExamName, TempExamNameAndScore);
        //            }

        //            if(ExamFileExtension==".mdb")
        //            {
        //                Util.InsertExamItemAndSelector(
        //                    InsertExam_SelectFilePath.Text,
        //                    InsertExamBaseInfo_ExamTableName.Text,
        //                    InserExamBaseInfo_FlowClassCombox.Text,
        //                    TempExamInfoJson,
        //                    InsertExam_ProcessTextTip,
        //                    InsertExam_ProcessProgressBarTip,
        //                    new AccessDatabaseFactory());
        //            }
        //            else if(ExamFileExtension==".db")
        //            {
        //                Util.InsertExamItemAndSelector(
        //                    InsertExam_SelectFilePath.Text,
        //                    InsertExamBaseInfo_ExamTableName.Text,
        //                    InserExamBaseInfo_FlowClassCombox.Text,
        //                    TempExamInfoJson,
        //                    InsertExam_ProcessTextTip,
        //                    InsertExam_ProcessProgressBarTip,
        //                    new SqliteDatabaseFactory());
        //            }


        //            RefreshExamBaseInfoTableView();
        //        }
        //    }
        //}

        private void MemroyTimerConrolButton_Click(object sender, EventArgs e)
        {
            if(MemroyTimerConrolButton.Text == "暂停定时器")
            {
                MemroyTimerConrolButton.Text = "开启定时器";
                MemoryTimer.Stop();
            } else if(MemroyTimerConrolButton.Text == "开启定时器")
            {
                MemroyTimerConrolButton.Text = "暂停定时器";
                MemoryTimer.Start();
            }
        }

        private void InsertExam_BatchMenuButton_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.InsetExam_BacthMenu
                    .Show(InsertExam_BatchMenuButton, new Point(0, InsertExam_BatchMenuButton.Height));
            }
        }

        private void InsertExam_BacthAddItem_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(InsertExam_BacthItemName_CustomFormat.Text))
            {
                bool MatchFormat = Regex.Match(InsertExam_BacthItemName_CustomFormat.Text, @"X").Success;

                if(MatchFormat && InserExam_BatchInsertItem.Value > 0)
                {
                    DynamicAddControlParams.InsertExam_ExamNameControl_Lists
                        .ForEach(
                            TempControl =>
                            {
                                InsertExam_ExamSet.Controls.Remove(TempControl);
                            });

                    DynamicAddControlParams.InsertExam_ExamScoreControl_Lists
                        .ForEach(
                            TempControl =>
                            {
                                InsertExam_ExamSet.Controls.Remove(TempControl);
                            });

                    DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Clear();
                    DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Clear();
                    string TempNumber = string.Empty;
                    if(InserExam_BatchCustomFormat_ChinseNum.Checked)
                    {
                        TempNumber = "一";
                    } else
                    {
                        TempNumber = "1";
                    }
                    InsertExam_ExamName.Text = InsertExam_BacthItemName_CustomFormat.Text.Replace("X", TempNumber);
                    InsertExam_ExamScore.Value = InsertExam_BacthScoreSet.Value;
                    for(int TempIndex = 2; TempIndex <= InserExam_BatchInsertItem.Value; TempIndex++)
                    {
                        if(InserExam_BatchCustomFormat_ChinseNum.Checked)
                        {
                            TempNumber = Util.ConvertNumberToChinese(TempIndex);
                        } else
                        {
                            TempNumber = TempIndex.ToString();
                        }

                        string TempItemName = InsertExam_BacthItemName_CustomFormat.Text.Replace("X", TempNumber);

                        DynamicAddControlParams.InsertExam_Control_Index++;

                        int TempBaseControlY = InsertExam_ExamName.Location.Y;

                        int TempLocationY = TempBaseControlY +
                            (DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Count * 25);

                        string InsertExamNameControlName = $"{DynamicAddControlParams.InsertExam_ExamName_Prefix}{DynamicAddControlParams.InsertExam_Control_Index}";
                        string InsertExamScoreControlName = $"{DynamicAddControlParams.InsertExam_ExamScore_Prefix}{DynamicAddControlParams.InsertExam_Control_Index}";

                        Point InsertExamNameControlNewPoint = new Point(
                            InsertExam_ExamName.Location.X,
                            InsertExam_ExamName.Location.Y + TempLocationY);

                        Point InsertExamScoreControlNewPoint = new Point(
                            InsertExam_ExamScore.Location.X,
                            InsertExam_ExamScore.Location.Y + TempLocationY);

                        TextBox TempAddItemNameBox = DynamicControlAdd<TextBox>(
                            InsertExam_ExamSet,
                            InsertExamNameControlName,
                            InsertExamNameControlNewPoint,
                            InsertExam_ExamName.Size);
                        NumericUpDown TempAddItemScoreBox = DynamicControlAdd<NumericUpDown>(
                            InsertExam_ExamSet,
                            InsertExamScoreControlName,
                            InsertExamScoreControlNewPoint,
                            InsertExam_ExamScore.Size);

                        TempAddItemNameBox.Text = TempItemName;
                        TempAddItemScoreBox.DecimalPlaces = 1;
                        TempAddItemScoreBox.Maximum = 10000;
                        TempAddItemScoreBox.Minimum = -1;
                        TempAddItemScoreBox.Increment = 1;
                        TempAddItemScoreBox.Value = InsertExam_BacthScoreSet.Value;

                        DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Add(TempAddItemNameBox);
                        DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Add(TempAddItemScoreBox);
                    }
                }
            }
        }

        private void InsertExam_AddControl_Click_1(object sender, EventArgs e)
        {
            DynamicAddControlParams.InsertExam_Control_Index++;
            //DynamicAddControlParams.InsertExam_Control_LocationY += 25;
            int TempBaseControlY = DynamicAddControlParams.TempBaseInsertItemLocationY;

            int TempLocationY = TempBaseControlY + (DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Count * 25);

            string InsertExamNameControlName = $"{DynamicAddControlParams.InsertExam_ExamName_Prefix}{DynamicAddControlParams.InsertExam_Control_Index}";
            string InsertExamScoreControlName = $"{DynamicAddControlParams.InsertExam_ExamScore_Prefix}{DynamicAddControlParams.InsertExam_Control_Index}";

            Point InsertExamNameControlNewPoint = new Point(
                InsertExam_ExamName.Location.X,
                InsertExam_ExamName.Location.Y + TempLocationY);

            Point InsertExamScoreControlNewPoint = new Point(
                InsertExam_ExamScore.Location.X,
                InsertExam_ExamScore.Location.Y + TempLocationY);

            TextBox TempItemNameBox = DynamicControlAdd<TextBox>(
                InsertExam_ExamSet,
                InsertExamNameControlName,
                InsertExamNameControlNewPoint,
                InsertExam_ExamName.Size);

            NumericUpDown TempNumberUpDown = DynamicControlAdd<NumericUpDown>(
                InsertExam_ExamSet,
                InsertExamScoreControlName,
                InsertExamScoreControlNewPoint,
                InsertExam_ExamScore.Size);

            TempNumberUpDown.DecimalPlaces = 1;
            TempNumberUpDown.Minimum = -1;
            TempNumberUpDown.Maximum = 10000;
            TempNumberUpDown.Increment = 1;

            DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Add(TempItemNameBox);
            DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Add(TempNumberUpDown);
        }

        private void InsertExam_RemoveControl_Click_1(object sender, EventArgs e)
        {
            if(DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Count > 0)
            {
                Control LastControl = DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Last();
                InsertExam_ExamSet.Controls.Remove(LastControl);
                DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Remove(LastControl);
            }

            if(DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Count > 0)
            {
                Control LastControl = DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Last();
                InsertExam_ExamSet.Controls.Remove(LastControl);
                DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Remove(LastControl);
            }
        }

        private void InsertExam_ExamSelector_AddControl_Click_1(object sender, EventArgs e)
        {
            DynamicAddControlParams.InsertExamSelector_Control_Index++;
            //DynamicAddControlParams.InsertExamSelector_Control_LocationY += 25;

            int TempBaseControlY = DynamicAddControlParams.TempBaseInsertItemLocationY;

            int TempLocationY = TempBaseControlY +
                (DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Count * 25);


            string InsertExamSelectorNameControlName = $"{DynamicAddControlParams.InsertExam_ExamSelector_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";
            string InsertExamSelectorPerControlName = $"{DynamicAddControlParams.InsertExam_ExamSelectorPer_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";
            string InsertExamSelectorAvgControlName = $"{DynamicAddControlParams.InsertExam_ExamSelectorAvg_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";

            Point InsertExamSelectorNameControlNewPoint = new Point(
                InsertExam_ExamSelector.Location.X,
                InsertExam_ExamSelector.Location.Y + TempLocationY);

            Point InsertExamSelectorPerControlNewPoint = new Point(
                InsertExam_ExamSelectorPerCent.Location.X,
                InsertExam_ExamSelectorPerCent.Location.Y + TempLocationY);

            Point InsertExamSelectorAvgControlNewPoint = new Point(
                InsertExam_ExamSelector_AVG.Location.X,
                InsertExam_ExamSelector_AVG.Location.Y + TempLocationY);

            Control TempExamSelectorNameControl = DynamicControlAdd<TextBox>(
                InsertExam_ExamSelectorSet,
                InsertExamSelectorNameControlName,
                InsertExamSelectorNameControlNewPoint,
                InsertExam_ExamSelector.Size);

            Control TempExamSelectorPerControl = DynamicControlAdd<NumericUpDown>(
                InsertExam_ExamSelectorSet,
                InsertExamSelectorPerControlName,
                InsertExamSelectorPerControlNewPoint,
                InsertExam_ExamSelectorPerCent.Size);

            Control TempExamSelectorAvgControl = DynamicControlAdd<CheckBox>(
                InsertExam_ExamSelectorSet,
                InsertExamSelectorAvgControlName,
                InsertExamSelectorAvgControlNewPoint,
                InsertExam_ExamSelector_AVG.Size);

            NumericUpDown TempPerNumberic = TempExamSelectorPerControl as NumericUpDown;

            TempPerNumberic.DecimalPlaces = 1;
            TempPerNumberic.Maximum = 10000;
            TempPerNumberic.Minimum = -1;
            TempPerNumberic.Increment = 1;

            (TempExamSelectorAvgControl as CheckBox).Checked = true;

            DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Add(TempExamSelectorNameControl);
            DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Add(TempExamSelectorPerControl);
            DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Add(TempExamSelectorAvgControl);
        }

        private void InsertExam_ExamSelector_RemoveControl_Click_1(object sender, EventArgs e)
        {
            if(DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Count > 0)
            {
                InsertExam_ExamSelectorSet.Controls
                    .Remove(DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Last());
                DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists
                    .Remove(DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Last());
            }

            if(DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Count > 0)
            {
                InsertExam_ExamSelectorSet.Controls
                    .Remove(DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Last());
                DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists
                    .Remove(DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Last());
            }

            if(DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Count > 0)
            {
                InsertExam_ExamSelectorSet.Controls
                    .Remove(DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Last());
                DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists
                    .Remove(DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Last());
            }
        }

        private void MemroyTimerConrolButton_Click_1(object sender, EventArgs e)
        {
            if(MemroyTimerConrolButton.Text == "暂停定时器")
            {
                MemroyTimerConrolButton.Text = "开启定时器";
                MemoryTimer.Stop();
            } else if(MemroyTimerConrolButton.Text == "开启定时器")
            {
                MemroyTimerConrolButton.Text = "暂停定时器";
                MemoryTimer.Start();
            }
        }

        private void InsertExamItemAndSelctor_StartInsert_Click_1(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(InsertExam_SelectFilePath.Text) &&
                !string.IsNullOrEmpty(InsertExamBaseInfo_ExamTableName.Text))
            {
                string ExamFileExtension = Path.GetExtension(InsertExam_SelectFilePath.Text);
                
                DialogResult TempAskResult = MessageBox.Show(
                    "确认要插入考评表数据吗？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Asterisk);

                if(TempAskResult == DialogResult.Yes)
                {
                    if(!Util.CheckInsertExamSpecialChar(InsertExamBaseInfo_ExamTableName.Text))
                    {
                        List<string> TempSpecialExamName = new List<string>();

                        List<string> TempSpecialExamSelectorName = new List<string>();

                        HashSet<string> TempExamFourNameHashSet = new HashSet<string>();

                        List<string> TempDupliFourNameExamName = new List<string>();


                        List<TextBox> TempExamNames = InsertExam_ExamSet.Controls
                            .OfType<TextBox>()
                            .Where(x => !string.IsNullOrEmpty(x.Text))
                            .ToList();

                        List<NumericUpDown> TempExamScores = InsertExam_ExamSet.Controls
                            .OfType<NumericUpDown>()
                            .Where(x => x.Value >= -1)
                            .ToList();

                        List<TextBox> TempExamSelectorNames = InsertExam_ExamSelectorSet.Controls
                            .OfType<TextBox>()
                            .Where(x => !string.IsNullOrEmpty(x.Text))
                            .ToList();

                        List<NumericUpDown> TempExamSelectorPerValues = InsertExam_ExamSelectorSet.Controls
                            .OfType<NumericUpDown>()
                            .Where(x => x.Value >= -1)
                            .ToList();


                        List<CheckBox> TempExamSelectorAvgValues = InsertExam_ExamSelectorSet.Controls
                            .OfType<CheckBox>()
                            .ToList();

                        TempExamNames.ForEach(
                            TempExamNameBox =>
                            {
                                if(TempExamNameBox.Text.Length >= 4)
                                {
                                    //项目名称前四位一样的数据不允许插入
                                    string TempExamName_Front4 = TempExamNameBox.Text.Substring(0, 4);

                                    if(!TempExamFourNameHashSet.Add(TempExamName_Front4))
                                    {
                                        TempDupliFourNameExamName.Add(TempExamNameBox.Text);
                                    }
                                }
                                if(Util.CheckInsertExamSpecialChar(TempExamNameBox.Text))
                                {
                                    TempSpecialExamName.Add(TempExamNameBox.Text);
                                }
                            });

                        TempExamSelectorNames.ForEach(
                            TempExamSelectorNameBox =>
                            {
                                if(Util.CheckInsertExamSpecialChar(TempExamSelectorNameBox.Text))
                                {
                                    TempSpecialExamSelectorName.Add(TempExamSelectorNameBox.Text);
                                }
                            });

                        if(TempSpecialExamName.Count == 0 &&
                            TempSpecialExamSelectorName.Count == 0 &&
                            TempDupliFourNameExamName.Count == 0)
                        {
                            JObject TempExamInfoJson = new JObject();

                            for(int TempIndex = 0; TempIndex < TempExamNames.Count; TempIndex++)
                            {
                                string TempExamName = TempExamNames[TempIndex].Text;
                                string TempExamScore = TempExamScores[TempIndex].Value.ToString();

                                JObject TempExamNameAndScore = new JObject()
                                {
                                    ["Type"] = "项目",
                                    ["Score"] = TempExamScore,
                                    ["Selectors"] = new JObject()
                                };

                                for(int TempIndex_Selector = 0; TempIndex_Selector < TempExamSelectorNames.Count; TempIndex_Selector++)
                                {
                                    string TempSelectorName = TempExamSelectorNames[TempIndex_Selector].Text;
                                    string TempSelectorScorePer = TempExamSelectorPerValues[TempIndex_Selector].Value
                                        .ToString();
                                    bool TempSelectorCalcAvg = TempExamSelectorAvgValues[TempIndex_Selector].Checked;

                                    JObject TempExamSelectorsInfo = new JObject()
                                    {
                                        ["Type"] = "选项",
                                        ["ScorePer"] = TempSelectorScorePer,
                                        ["CalcAvg"] = TempSelectorCalcAvg
                                    };
                                    (TempExamNameAndScore["Selectors"] as JObject).Add(
                                        TempSelectorName,
                                        TempExamSelectorsInfo);
                                }
                                JObject InvalidObject = new JObject()
                                {
                                    ["Type"] = "选项",
                                    ["ScorePer"] = InsertExam_SelectorInvalid_ScorePer.Value.ToString(),
                                    ["CalcAvg"] = InsertExam_SelectorInvalid_CacAvg.Checked
                                };
                                (TempExamNameAndScore["Selectors"] as JObject).Add("无效", InvalidObject);

                                JObject AbsTainObject = new JObject()
                                {
                                    ["Type"] = "选项",
                                    ["ScorePer"] = InsertExam_SelectorAbstain_ScorePer.Value.ToString(),
                                    ["CalcAvg"] = InsertExam_SelectorAbstain_CalcAvg.Checked
                                };
                                (TempExamNameAndScore["Selectors"] as JObject).Add("弃权", AbsTainObject);

                                TempExamInfoJson.Add(TempExamName, TempExamNameAndScore);
                            }
                            Task.Run(
                                () =>
                                {
                                    try
                                    {
                                        (sender as Button).Enabled = false;

                                        ChangeExamTableNo_Start.Enabled = false;

                                        if (ExamFileExtension == ".mdb")
                                        {
                                            Util.InsertExamItemAndSelector(
                                                InsertExam_SelectFilePath.Text,
                                                InsertExamBaseInfo_ExamTableName.Text,
                                                InserExamBaseInfo_FlowClassCombox.Text,
                                                TempExamInfoJson,
                                                InsertExam_ProcessTextTip,
                                                InsertExam_ProcessProgressBarTip,
                                                new AccessDatabaseFactory());
                                        }
                                        else if (ExamFileExtension == ".db")
                                        {
                                            Util.InsertExamItemAndSelector(
                                                InsertExam_SelectFilePath.Text,
                                                InsertExamBaseInfo_ExamTableName.Text,
                                                InserExamBaseInfo_FlowClassCombox.Text,
                                                TempExamInfoJson,
                                                InsertExam_ProcessTextTip,
                                                InsertExam_ProcessProgressBarTip,
                                                new SqliteDatabaseFactory());
                                        }

                                        BeginInvoke(
                                            new Action(
                                                () =>
                                                {
                                                    MessageBox.Show(
                                                        "执行完毕",
                                                        "提示",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Asterisk);

                                                    (sender as Button).Enabled = true;
                                                    RefreshExamBaseInfoTableView();
                                                    ChangeExamTableNo_Start.Enabled = true;
                                                }));
                                    } catch(Exception ex)
                                    {
                                        BeginInvoke(
                                            new Action(
                                                () =>
                                                {
                                                    MessageBox.Show(
                                                        ex.Message,
                                                        "错误",
                                                        MessageBoxButtons.OK,
                                                        MessageBoxIcon.Error);
                                                    (sender as Button).Enabled = true;
                                                    ChangeExamTableNo_Start.Enabled = true;
                                                    ExceptionParamter.ExceptionCount = 0;
                                                }));
                                    }
                                });
                        } else
                        {
                            StringBuilder TempTipMessage = new StringBuilder();

                            if(TempSpecialExamName.Count > 0)
                            {
                                TempTipMessage.AppendLine("====以下项目名称存在特殊字符，请修改====");
                                TempSpecialExamName.ForEach(TempExamName => TempTipMessage.AppendLine(TempExamName));
                            }

                            if(TempSpecialExamSelectorName.Count > 0)
                            {
                                TempTipMessage.AppendLine("====以下选项名称存在特殊字符，请修改====");
                                TempSpecialExamSelectorName.ForEach(
                                    TempExamSelectorName => TempTipMessage.AppendLine(TempExamSelectorName));
                            }

                            if(TempDupliFourNameExamName.Count > 0)
                            {
                                TempTipMessage.AppendLine("====以下项目名称前四位名称相同，请修改====");
                                TempDupliFourNameExamName.ForEach(
                                    TempDupliFourName => TempTipMessage.AppendLine(TempDupliFourName));
                            }

                            TempTipMessage.AppendLine();

                            BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        MessageBox.Show(
                                            TempTipMessage.ToString(),
                                            "提示",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Asterisk);
                                    }));
                        }
                    } else
                    {
                        BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show(
                                        "考评表名称中存在非法字符，无法插入",
                                        "提示",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Asterisk);
                                }));
                    }
                }

                RefreshExamBaseInfoTableView();
            }
        }

        private void InsertExamTable_SelectExamFile_Click_1(object sender, EventArgs e)
        {
            string TempMdbFilePathName = Util.ShowSelectDialog(
                false,
                false,
                "数据库文件",
                "*.mdb;*.db",
                vfsoft_examdata_path_textbox.Text)
                .FirstOrDefault();

            if(!string.IsNullOrEmpty(TempMdbFilePathName) && TempMdbFilePathName != default)
            {
                InsertExam_SelectFilePath.Text = TempMdbFilePathName;
                RefreshExamBaseInfoTableView();
            }
        }

        private void InsertExam_BatchCustomItemWithName(
            List<string> ItemNames,
            bool IsFromCliboard = false,
            string PasteStuffixString = "",
            bool AddonMode=false)
        {
            //不是新增模式
            if(!AddonMode)
            {
                DynamicAddControlParams.InsertExam_ExamNameControl_Lists
                                .ForEach(
                                    TempControl =>
                                    {
                                        InsertExam_ExamSet.Controls.Remove(TempControl);
                                    });

                DynamicAddControlParams.InsertExam_ExamScoreControl_Lists
                    .ForEach(
                        TempControl =>
                        {
                            InsertExam_ExamSet.Controls.Remove(TempControl);
                        });

                DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Clear();
                DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Clear();
            }
            
            if(IsFromCliboard && !string.IsNullOrEmpty(Clipboard.GetText()))
            {
                ItemNames = Clipboard.GetText()
                    .Split(new [] { '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }
            if(ItemNames != null && ItemNames.Count > 0)
            {
                //不是新增模式
                if (!AddonMode)
                {
                    InsertExam_ExamName.Text = string.IsNullOrEmpty(PasteStuffixString)
                    ? ItemNames[0]
                    : $"{ItemNames[0]}{PasteStuffixString}";
                    ItemNames = ItemNames.Skip(1).ToList();
                }
                
                foreach(string ItemName in ItemNames)
                {
                    string TempItemName = string.IsNullOrEmpty(PasteStuffixString)
                        ? ItemName
                        : $"{ItemName}{PasteStuffixString}";

                    DynamicAddControlParams.InsertExam_Control_Index++;

                    int TempBaseControlY = InsertExam_ExamName.Location.Y;

                    int TempLocationY = TempBaseControlY +
                        (DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Count * 25);

                    string InsertExamNameControlName = $"{DynamicAddControlParams.InsertExam_ExamName_Prefix}{DynamicAddControlParams.InsertExam_Control_Index}";
                    string InsertExamScoreControlName = $"{DynamicAddControlParams.InsertExam_ExamScore_Prefix}{DynamicAddControlParams.InsertExam_Control_Index}";

                    Point InsertExamNameControlNewPoint = new Point(
                        InsertExam_ExamName.Location.X,
                        InsertExam_ExamName.Location.Y + TempLocationY);

                    Point InsertExamScoreControlNewPoint = new Point(
                        InsertExam_ExamScore.Location.X,
                        InsertExam_ExamScore.Location.Y + TempLocationY);

                    TextBox TempAddItemNameBox = DynamicControlAdd<TextBox>(
                        InsertExam_ExamSet,
                        InsertExamNameControlName,
                        InsertExamNameControlNewPoint,
                        InsertExam_ExamName.Size);
                    NumericUpDown TempAddItemScoreBox = DynamicControlAdd<NumericUpDown>(
                        InsertExam_ExamSet,
                        InsertExamScoreControlName,
                        InsertExamScoreControlNewPoint,
                        InsertExam_ExamScore.Size);

                    TempAddItemNameBox.Text = TempItemName;
                    TempAddItemScoreBox.DecimalPlaces = 1;
                    TempAddItemScoreBox.Maximum = 10000;
                    TempAddItemScoreBox.Minimum = -1;
                    TempAddItemScoreBox.Increment = 1;
                    TempAddItemScoreBox.Value = InsertExam_BacthScoreSet.Value;

                    DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Add(TempAddItemNameBox);
                    DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Add(TempAddItemScoreBox);
                }
            }
        }

        private void InsertExam_BatchCustomSelectorsWithName(
            List<string> SelectorNames,
            bool IsFromCliboard = false,
            string PasteStuffixString = "")
        {
            DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Clear();


            if(IsFromCliboard && !string.IsNullOrEmpty(Clipboard.GetText()))
            {
                SelectorNames = Clipboard.GetText()
                    .Split(new [] { '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            }
            if(SelectorNames != null && SelectorNames.Count > 0)
            {
                InsertExam_ExamSelector.Text = string.IsNullOrEmpty(PasteStuffixString)
                    ? SelectorNames[0]
                    : $"{SelectorNames[0]}{PasteStuffixString}";

                foreach(string SelectorName in SelectorNames.Skip(1))
                {
                    string TempSelectorName = string.IsNullOrEmpty(PasteStuffixString)
                        ? SelectorName
                        : $"{SelectorName}{PasteStuffixString}";

                    DynamicAddControlParams.InsertExamSelector_Control_Index++;

                    int TempBaseControlY = InsertExam_ExamSelector.Location.Y;

                    int TempLocationY = TempBaseControlY +
                        (DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Count * 25);

                    string InsertExamSelectorNameControlName = $"{DynamicAddControlParams.InsertExam_ExamSelector_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";
                    string InsertExamSelectorPerControlName = $"{DynamicAddControlParams.InsertExam_ExamSelectorPer_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";
                    string InsertExamSelectorAvgControlName = $"{DynamicAddControlParams.InsertExam_ExamSelectorAvg_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";

                    Point InsertExamSelectorNameControlNewPoint = new Point(
                        InsertExam_ExamSelector.Location.X,
                        InsertExam_ExamSelector.Location.Y + TempLocationY);

                    Point InsertExamSelectorPerControlNewPoint = new Point(
                        InsertExam_ExamSelectorPerCent.Location.X,
                        InsertExam_ExamSelectorPerCent.Location.Y + TempLocationY);

                    Point InsertExamSelectorAvgControlNewPoint = new Point(
                        InsertExam_ExamSelector_AVG.Location.X,
                        InsertExam_ExamSelector_AVG.Location.Y + TempLocationY);

                    TextBox TempExamSelectorNameControl = DynamicControlAdd<TextBox>(
                        InsertExam_ExamSelectorSet,
                        InsertExamSelectorNameControlName,
                        InsertExamSelectorNameControlNewPoint,
                        InsertExam_ExamSelector.Size);

                    NumericUpDown TempExamSelectorPerControl = DynamicControlAdd<NumericUpDown>(
                        InsertExam_ExamSelectorSet,
                        InsertExamSelectorPerControlName,
                        InsertExamSelectorPerControlNewPoint,
                        InsertExam_ExamSelectorPerCent.Size);

                    CheckBox TempExamSelectorAvgControl = DynamicControlAdd<CheckBox>(
                        InsertExam_ExamSelectorSet,
                        InsertExamSelectorAvgControlName,
                        InsertExamSelectorAvgControlNewPoint,
                        InsertExam_ExamSelector_AVG.Size);

                    TempExamSelectorNameControl.Text = TempSelectorName;
                    TempExamSelectorPerControl.Value = InsertExam_BacthScoreSet.Value;

                    TempExamSelectorPerControl.DecimalPlaces = 1;
                    TempExamSelectorPerControl.Maximum = 10000;
                    TempExamSelectorPerControl.Minimum = -1;
                    TempExamSelectorPerControl.Increment = 1;
                    TempExamSelectorAvgControl.Checked = true;


                    DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Add(TempExamSelectorNameControl);
                    DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Add(TempExamSelectorPerControl);
                    DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Add(TempExamSelectorAvgControl);
                }
            }
        }

        private void InsertExam_BatchSelector_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(InsertExam_BacthItemName_CustomFormat.Text))
            {
                bool MatchFormat = Regex.Match(InsertExam_BacthItemName_CustomFormat.Text, @"X").Success;

                if(MatchFormat && InserExam_BatchInsertItem.Value > 1)
                {
                    DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists
                        .ForEach(
                            TempControl =>
                            {
                                InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                            });

                    DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists
                        .ForEach(
                            TempControl =>
                            {
                                InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                            });

                    DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists
                        .ForEach(
                            TempControl =>
                            {
                                InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                            });

                    DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Clear();
                    DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Clear();
                    DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Clear();

                    string TempNumber = string.Empty;
                    if(InserExam_BatchCustomFormat_ChinseNum.Checked)
                    {
                        TempNumber = "一";
                    } else
                    {
                        TempNumber = "1";
                    }

                    InsertExam_ExamSelector.Text = InsertExam_BacthItemName_CustomFormat.Text.Replace("X", TempNumber);
                    InsertExam_ExamSelectorPerCent.Value = InsertExam_BacthScoreSet.Value;
                    for(int TempIndex = 2; TempIndex <= InserExam_BatchInsertItem.Value; TempIndex++)
                    {
                        if(InserExam_BatchCustomFormat_ChinseNum.Checked)
                        {
                            TempNumber = Util.ConvertNumberToChinese(TempIndex);
                        } else
                        {
                            TempNumber = TempIndex.ToString();
                        }
                        string TempSelectorName = InsertExam_BacthItemName_CustomFormat.Text.Replace("X", TempNumber);

                        DynamicAddControlParams.InsertExamSelector_Control_Index++;

                        int TempBaseControlY = InsertExam_ExamSelector.Location.Y;

                        int TempLocationY = TempBaseControlY +
                            (DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Count * 25);

                        string InsertExamSelectorNameControlName = $"{DynamicAddControlParams.InsertExam_ExamSelector_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";
                        string InsertExamSelectorPerControlName = $"{DynamicAddControlParams.InsertExam_ExamSelectorPer_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";
                        string InsertExamSelectorAvgControlName = $"{DynamicAddControlParams.InsertExam_ExamSelectorAvg_Prefix}{DynamicAddControlParams.InsertExamSelector_Control_Index}";

                        Point InsertExamSelectorNameControlNewPoint = new Point(
                            InsertExam_ExamSelector.Location.X,
                            InsertExam_ExamSelector.Location.Y + TempLocationY);

                        Point InsertExamSelectorPerControlNewPoint = new Point(
                            InsertExam_ExamSelectorPerCent.Location.X,
                            InsertExam_ExamSelectorPerCent.Location.Y + TempLocationY);

                        Point InsertExamSelectorAvgControlNewPoint = new Point(
                            InsertExam_ExamSelector_AVG.Location.X,
                            InsertExam_ExamSelector_AVG.Location.Y + TempLocationY);

                        TextBox TempExamSelectorNameControl = DynamicControlAdd<TextBox>(
                            InsertExam_ExamSelectorSet,
                            InsertExamSelectorNameControlName,
                            InsertExamSelectorNameControlNewPoint,
                            InsertExam_ExamSelector.Size);

                        NumericUpDown TempExamSelectorPerControl = DynamicControlAdd<NumericUpDown>(
                            InsertExam_ExamSelectorSet,
                            InsertExamSelectorPerControlName,
                            InsertExamSelectorPerControlNewPoint,
                            InsertExam_ExamSelectorPerCent.Size);

                        CheckBox TempExamSelectorAvgControl = DynamicControlAdd<CheckBox>(
                            InsertExam_ExamSelectorSet,
                            InsertExamSelectorAvgControlName,
                            InsertExamSelectorAvgControlNewPoint,
                            InsertExam_ExamSelector_AVG.Size);

                        TempExamSelectorNameControl.Text = TempSelectorName;

                        TempExamSelectorPerControl.Value = InsertExam_BacthScoreSet.Value;
                        TempExamSelectorPerControl.DecimalPlaces = 1;
                        TempExamSelectorPerControl.Maximum = 10000;
                        TempExamSelectorPerControl.Minimum = -1;
                        TempExamSelectorPerControl.Increment = 1;
                        TempExamSelectorAvgControl.Checked = true;


                        DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Add(TempExamSelectorNameControl);
                        DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Add(TempExamSelectorPerControl);
                        DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Add(TempExamSelectorAvgControl);
                    }
                }
            }
        }

        private void InsertExam_BatchRemoveItem_Click(object sender, EventArgs e)
        {
            DynamicAddControlParams.InsertExam_ExamNameControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamScoreControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Clear();

            InsertExam_ExamName.Text = string.Empty;

            InsertExam_ExamScore.Value = 0;
        }

        private void InsertExam_BatchRemoveSelector_Click(object sender, EventArgs e)
        {
            DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Clear();

            InsertExam_ExamSelector.Text = string.Empty;
            InsertExam_ExamSelectorPerCent.Value = 0;
        }

        private void InsertExam_RefreshExamInfo_Click(object sender, EventArgs e) { RefreshExamBaseInfoTableView(); }

        private void InsertExam_BatchSelectorSetCalcAvg_Click(object sender, EventArgs e)
        {
            List<CheckBox> TempExamSelectorAvgValues = InsertExam_ExamSelectorSet.Controls.OfType<CheckBox>().ToList();

            TempExamSelectorAvgValues.ForEach(
                TempControl =>
                {
                    TempControl.Checked = true;
                });
        }

        private void InsertExam_BatchSelectorSetNoCalcAvg_Click(object sender, EventArgs e)
        {
            List<CheckBox> TempExamSelectorAvgValues = InsertExam_ExamSelectorSet.Controls.OfType<CheckBox>().ToList();

            TempExamSelectorAvgValues.ForEach(
                TempControl =>
                {
                    TempControl.Checked = false;
                });
        }

        private void linkLabel11_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        { Process.Start(Util.GetEmbedResourceObj("Link_NetPan_Download_AccessEngine").ToString()); }

        private void InsertExamTableView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                if(InsertExamTableView.SelectedRows.Count > 0)
                {
                    string SelectTableExamId = InsertExamTableView.SelectedRows[0].Cells["考评表ID"]?.Value?.ToString();
                    string SelectTableExamName = InsertExamTableView.SelectedRows[0].Cells["考评表名称"]?.Value?.ToString();

                    if(!string.IsNullOrEmpty(SelectTableExamId) && !string.IsNullOrEmpty(SelectTableExamName))
                    {
                        ChangeExamTableNo_OrginTableName.Text = SelectTableExamName;

                        ChangeExamTableNo_OrginTableIndex.Text = SelectTableExamId;
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ChangeExamTableNo_Start_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(InsertExam_SelectFilePath.Text) &&
                !string.IsNullOrEmpty(ChangeExamTableNo_OrginTableIndex.Text))
            {
                DialogResult TempAskResult = MessageBox.Show(
                    "确认要进行修改吗？",
                    "提示",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Asterisk);


                string ExamFileExtension = Path.GetExtension(InsertExam_SelectFilePath.Text);

                if(TempAskResult == DialogResult.Yes)
                {
                    StringBuilder TempMessAgeBuilder = new StringBuilder();

                    MessageBoxIcon TempTipMessageIcon = MessageBoxIcon.Asterisk;

                    Task.Run(
                        () =>
                        {
                            try
                            {
                                (sender as Button).Enabled = false;
                                InsertExamItemAndSelctor_StartInsert.Enabled = false;
                                (sender as Button).Text = "修改中...";

                                (bool TableChangeStatus, int TableChangeRecommendExamId) TempBaseEnty = (false,0);

                                if(ExamFileExtension==".mdb")
                                {
                                        TempBaseEnty = Util.ChangeTableIndex(
                                        InsertExam_SelectFilePath.Text,
                                        ChangeExamTableNo_OrginTableIndex.Text,
                                        ChangeExamTableNo_OrginTableNewIndex.Value.ToString(),
                                        InsertExam_ProcessTextTip,
                                        new AccessDatabaseFactory());
                                }
                                else if(ExamFileExtension==".db")
                                {
                                    TempBaseEnty = Util.ChangeTableIndex(
                                    InsertExam_SelectFilePath.Text,
                                    ChangeExamTableNo_OrginTableIndex.Text,
                                    ChangeExamTableNo_OrginTableNewIndex.Value.ToString(),
                                    InsertExam_ProcessTextTip,
                                    new SqliteDatabaseFactory());
                                }



                                if(TempBaseEnty.TableChangeStatus)
                                {
                                    TempMessAgeBuilder.Append("修改成功");
                                } else
                                {
                                    TempTipMessageIcon = MessageBoxIcon.Error;

                                    TempMessAgeBuilder.Append($"修改失败，表序号已存在，推荐设置序号为{TempBaseEnty.TableChangeRecommendExamId}");
                                }

                                BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show(
                                                TempMessAgeBuilder.ToString(),
                                                "提示",
                                                MessageBoxButtons.OK,
                                                TempTipMessageIcon);
                                            (sender as Button).Enabled = true;
                                            (sender as Button).Text = "执行修改";
                                            RefreshExamBaseInfoTableView();
                                            InsertExamItemAndSelctor_StartInsert.Enabled = true;
                                        }));
                            } catch(Exception ex)
                            {
                                BeginInvoke(
                                    new Action(
                                        () =>
                                        {
                                            MessageBox.Show(
                                                ex.Message,
                                                "错误",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                            (sender as Button).Enabled = true;
                                            (sender as Button).Text = "执行修改";
                                            InsertExamItemAndSelctor_StartInsert.Enabled = true;
                                        }));
                            }
                        });
                }
            }
        }

        private void 考评表信息插入ToolStripMenuItem_Click(object sender, EventArgs e)
        { MainTabPage.SelectedIndex = GetTabPageIndex("考评表信息插入"); }

        private void GetAllExamName_ExamBox_Click(object sender, EventArgs e)
        {
            try
            {
                if(ExamItemsCombox.Items.Count > 0)
                {
                    string ItemSplitsString = string.Join(
                        "|",
                        ExamItemsCombox.Items.Cast<object>().Select(item => item.ToString()).ToList());
                    if(ItemSplitsString != string.Empty)
                    {
                        Clipboard.SetText(ItemSplitsString);

                        MessageBox.Show("已复制所有项目，请自行粘贴", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExamReportList_Sort_Click(object sender, EventArgs e)
        {
            if(ExamReportRowMode_ExamList.Items.Count > 0 && TempExamReportItemLists.Count > 0)
            {
                TempExamReportItemLists.Sort(new FileNameComparerInstance());
                ExamReportRowMode_ExamList.DataSource = new List<string>(TempExamReportItemLists);
            }
        }

        private void ExamReportList_NoSort_Click(object sender, EventArgs e)
        {
            if(ExamReportRowMode_ExamList.Items.Count > 0 && TempExamReportItemLists.Count > 0)
            {
                ExamReportRowMode_ExamList.DataSource = new List<string>(TempExamReportItemLists_NoSort);
            }
        }

        private void InsertExam_BatchSelectors_C1_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "好", "较好", "一般", "差" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_CustomBatchSelectors_DropDownOpened(object sender, EventArgs e)
        { InsertExam_BatchSelectorCustomLists.Clear(); }

        private void InsertExam_BatchSelectors_C6_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "好", "较好", "一般", "较差" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C2_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "优秀", "良好", "一般", "较差" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C8_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "优秀", "良好", "一般", "差" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C3_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "优秀", "称职", "基本称职", "不称职" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C4_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "优秀", "合格", "基本合格", "不合格" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C5_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "认同", "基本认同", "不认同", "不了解" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C7_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "好", "一般", "不好", "不了解" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C9_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "满意", "较满意", "基本满意", "不满意" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C10_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "A", "B", "C", "D" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_BatchSelectors_C11_Click(object sender, EventArgs e)
        {
            InsertExam_BatchSelectorCustomLists = new List<string> { "a", "b", "c", "d" };

            InsertExam_BatchCustomSelectorsWithName(InsertExam_BatchSelectorCustomLists);
        }

        private void InsertExam_ExamSelector_PasteSelectorName_Click(object sender, EventArgs e)
        {
        }


        private void InsertExam_RemoveAllControl_Click(object sender, EventArgs e)
        {
            DynamicAddControlParams.InsertExam_ExamNameControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamScoreControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamNameControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamScoreControl_Lists.Clear();

            InsertExam_ExamName.Text = string.Empty;

            InsertExam_ExamScore.Value = 0;
        }

        private void InsertExam_ExamSelector_RemoveAllControl_Click(object sender, EventArgs e)
        {
            DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists
                .ForEach(
                    TempControl =>
                    {
                        InsertExam_ExamSelectorSet.Controls.Remove(TempControl);
                    });

            DynamicAddControlParams.InsertExam_ExamSelectorControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamSelectorPerControl_Lists.Clear();
            DynamicAddControlParams.InsertExam_ExamSelectorAvgControl_Lists.Clear();

            InsertExam_ExamSelector.Text = string.Empty;
            InsertExam_ExamSelectorPerCent.Value = 0;
        }

        private void Insert_ExamItem_PasteButton_Click(object sender, EventArgs e)
        { InsertExam_BatchCustomItemWithName(null, true, Insert_ExamItem_PasteOption_StuffixTextBox.Text); }

        private void InsertExam_ExamItemName_PasteItemName_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Insert_ExamItem_PasteStuffix
                    .Show(
                        InsertExam_ExamItemName_PasteItemName,
                        new Point(0, InsertExam_ExamItemName_PasteItemName.Height));
            }
        }

        private void InsertExam_ExamSelector_PasteSelectorName_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                this.Insert_ExamSelector_PasteStuffix
                    .Show(
                        InsertExam_ExamSelector_PasteSelectorName,
                        new Point(0, InsertExam_ExamSelector_PasteSelectorName.Height));
            }
        }

        private void Insert_ExamSelector_PasteButton_Click(object sender, EventArgs e)
        { InsertExam_BatchCustomSelectorsWithName(null, true, Insert_ExamSelector_PasteStuffixTextBox.Text); }

        private void InsertExam_BactchSelectorsButton_Click(object sender, EventArgs e)
        {
            new InsertExamBacthCustomSelectors();
        }

        private void ExamBacthSelectors_Custom1_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_1);
        }

        private void ExamBacthSelectors_Custom2_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_2);

        }

        private void ExamBacthSelectors_Custom3_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_3);

        }

        private void ExamBacthSelectors_Custom4_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_4);

        }

        private void ExamBacthSelectors_Custom5_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_5);

        }

        private void ExamBacthSelectors_Custom6_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_6);

        }

        private void ExamBacthSelectors_Custom7_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_7);

        }

        private void ExamBacthSelectors_Custom8_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_8);

        }

        private void ExamBacthSelectors_Custom9_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_9);

        }

        private void ExamBacthSelectors_Custom10_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_10);

        }

        private void ExamBacthSelectors_Custom11_Click(object sender, EventArgs e)
        {
            InsertExam_BatchCustomSelectorsWithName(InsertExamBacthCustomSelectors.CustomSelectors_11);
        }

        private void InsertExam_BactchSelectorsButton_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.InsertExamCustomSelectorsBacthMenu
                    .Show(
                        InsertExam_BactchSelectorsButton,
                        new Point(0, InsertExam_BactchSelectorsButton.Height));
            }
        }

        private void pictureBox5_MouseHover(object sender, EventArgs e)
        {
        }

        private void pictureBox5_MouseLeave(object sender, EventArgs e)
        {
            pictureBox5.Image = (Image)Util.GetEmbedResourceObj("大头儿子");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            var DefaultAudioStatus = Util.GetSystemDefaultAudioStatus();

            if (DefaultAudioStatus.AudioMute)
            {
                Util.SetSystemAudioVolume(false, float.Parse("0.3"));
            }

            Util.PlayMp3WithStream(
                "dtez",
                status =>
                {
                });
            pictureBox5.Image = (Image)Util.GetEmbedResourceObj("xtbb");
        }

        private void BarCodeLogoImageOpen_Click(object sender, EventArgs e)
        {
            IEnumerable<string> LogoImageFile = Util.ShowSelectDialog(false, false, "图片文件","*.jpg;*.jpeg:*.png;*.bmp");
            
            string LogoImagePath = LogoImageFile.FirstOrDefault();

            if(!string.IsNullOrEmpty(LogoImagePath))
            {
                Bitmap TempLogoImage = new Bitmap(LogoImagePath);

                BarCodLogoImageBox.Image = TempLogoImage;
            }
        }

        private void ClearBarcodeLogoImage_Click(object sender, EventArgs e)
        {
            BarCodLogoImageBox.Image = null;
        }

        private void AddPdfWaterMark_UseFileNameAsMark_CheckedChanged(object sender, EventArgs e)
        {
            PdfWaterMarkText.Enabled = !AddPdfWaterMark_UseFileNameAsMark.Checked;
        }

        private void ExamDataPakOrder_Select_Click(object sender, EventArgs e)
        {
           ExamDataPakOrder_Path.Text  =  Util.ShowSelectDialog(false, false,"数据库文件", "*.mdb;*.db").FirstOrDefault();

            if(!string.IsNullOrEmpty(ExamDataPakOrder_Path.Text) && ExamDataPakOrder_Path.Text != string.Empty )
            {
                RefreshExamUnitInfoTableView();
            }
        }

        private void ExamDataPakOrder_MarkChangedRows()
        {
            try
            {
                var dgv = ExamDataPakOrder_UnitListView;
                if (dgv == null) return;
                if (!(dgv.DataSource is System.Data.DataTable dt)) return;

                foreach (DataGridViewRow r in dgv.Rows)
                {
                    r.DefaultCellStyle.BackColor = System.Drawing.Color.Empty;
                }

                if (!dt.Columns.Contains("__orig_order")) return;
                if (!dt.Columns.Contains("单位编码")) return;

                var origCol = dt.Columns["__orig_order"];

                for (int i = 0; i < dt.Rows.Count && i < dgv.Rows.Count; i++)
                {
                    var unitCode = dt.Rows[i]["单位编码"]?.ToString();
                    var origOrderObj = dt.Rows[i][origCol];
                    int origOrder;
                    if (origOrderObj != null && int.TryParse(origOrderObj.ToString(), out origOrder))
                    {
                        if (origOrder != i + 1)
                        {
                            dgv.Rows[i].DefaultCellStyle.BackColor = System.Drawing.Color.LemonChiffon;
                        }
                    }
                }
            }
            catch { }
        }

        private void ExamDataPackUnitOrder_Up_Click(object sender, EventArgs e)
        {
            try
            {
                var dgv = ExamDataPakOrder_UnitListView;
                if (dgv == null || dgv.DataSource == null) return;
                if (dgv.CurrentRow == null) return;

                int rowIndex = dgv.CurrentRow.Index;
                if (rowIndex <= 0) return;

                if (!(dgv.DataSource is System.Data.DataTable dt)) return;

                // 交换当前行与上一行
                var curArr = (object[])dt.Rows[rowIndex].ItemArray.Clone();
                var prevArr = (object[])dt.Rows[rowIndex - 1].ItemArray.Clone();

                dt.Rows[rowIndex].ItemArray = prevArr;
                dt.Rows[rowIndex - 1].ItemArray = curArr;

                // 重新按当前位置生成序号（1..N），保证序号与当前位置一致
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    dt.Rows[k][0] = k + 1;
                }

                ExamDataPakOrder_MarkChangedRows();

                dgv.ClearSelection();
                dgv.CurrentCell = dgv.Rows[rowIndex - 1].Cells[0];
                dgv.Rows[rowIndex - 1].Selected = true;
            }
            catch { }
        }

        private void ExamDataPackUnitOrder_Down_Click(object sender, EventArgs e)
        {
            try
            {
                var dgv = ExamDataPakOrder_UnitListView;
                if (dgv == null || dgv.DataSource == null) return;
                if (dgv.CurrentRow == null) return;

                int rowIndex = dgv.CurrentRow.Index;
                if (!(dgv.DataSource is System.Data.DataTable dt)) return;
                if (rowIndex < 0 || rowIndex >= dt.Rows.Count - 1) return;

                // 交换当前行与下一行
                var curArr = (object[])dt.Rows[rowIndex].ItemArray.Clone();
                var nextArr = (object[])dt.Rows[rowIndex + 1].ItemArray.Clone();

                dt.Rows[rowIndex].ItemArray = nextArr;
                dt.Rows[rowIndex + 1].ItemArray = curArr;

                // 重新按当前位置生成序号（1..N），保证序号与当前位置一致
                for (int k = 0; k < dt.Rows.Count; k++)
                {
                    dt.Rows[k][0] = k + 1;
                }

                ExamDataPakOrder_MarkChangedRows();

                dgv.ClearSelection();
                dgv.CurrentCell = dgv.Rows[rowIndex + 1].Cells[0];
                dgv.Rows[rowIndex + 1].Selected = true;
            }
            catch { }
        }

        private void ExamDataPakOrder_Modify_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ExamDataPakOrder_Path.Text) || File.Exists(ExamDataPakOrder_Path.Text))
            {
                var confirm = MessageBox.Show(
                 "修改后无法复原，请谨慎操作，确定继续吗？",
                 "提示",
                 MessageBoxButtons.YesNo,
                 MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;

                if (!(ExamDataPakOrder_UnitListView?.DataSource is System.Data.DataTable dt) || dt.Rows.Count <= 0)
                {
                    return;
                }

                string ext = Path.GetExtension(ExamDataPakOrder_Path.Text)?.ToLowerInvariant();

                var callButton = sender as Button;
                if (callButton != null) callButton.Enabled = false;
                try { ExamDataPackUnitOrder_Up.Enabled = false; } catch { }
                try { ExamDataPackUnitOrder_Down.Enabled = false; } catch { }

                Task.Run(
                    () =>
                    {
                        try
                        {
                            // 按当前 DataGridView 顺序写回 Unit_Ess.AtOrder
                            // 约定：dt 第一列为“单位序号”，第三列为“单位编码”
                            var factory = new AccessDatabaseFactory();

                            for (int i = 0; i < dt.Rows.Count; i++)
                            {
                                var unitCodeObj = dt.Rows[i][2];
                                var unitCode = unitCodeObj?.ToString();
                                if (string.IsNullOrEmpty(unitCode)) continue;

                                int atOrder;
                                if (!int.TryParse(dt.Rows[i][0]?.ToString(), out atOrder))
                                {
                                    atOrder = i + 1;
                                }

                                string sql = $"UPDATE Unit_Ess SET AtOrder={atOrder} WHERE UnitCode='{unitCode.Replace("'", "''")}'";
                                var (_, conn) = Util.ExecuteAccessSql(ExamDataPakOrder_Path.Text, sql, factory);
                                conn.Close();
                            }

                            this.BeginInvoke(
                                new Action(
                                    () =>
                                    {
                                        RefreshExamUnitInfoTableView();
                                        MessageBox.Show("修改完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        finally
                        {
                            if (callButton != null)
                            {
                                try
                                {
                                    callButton.BeginInvoke(new Action(() => callButton.Enabled = true));
                                }
                                catch { }
                            }

                            try
                            {
                                ExamDataPackUnitOrder_Up.BeginInvoke(new Action(() => ExamDataPackUnitOrder_Up.Enabled = true));
                            }
                            catch { }

                            try
                            {
                                ExamDataPackUnitOrder_Down.BeginInvoke(new Action(() => ExamDataPackUnitOrder_Down.Enabled = true));
                            }
                            catch { }
                        }
                    });
            }
        }

        private void ExamDataPakOrder_Refresh_Click(object sender, EventArgs e)
        {
            RefreshExamUnitInfoTableView();
        }

        private void ExamMergeDataPak_SourceSelect_Click(object sender, EventArgs e)
        {
            ExamMergeDataPak_SourcePath.Text = Util.ShowSelectDialog(false, true, "数据包目录", string.Empty).FirstOrDefault();
        }

        private void ExamMergeDataPak_TargetSelect_Click(object sender, EventArgs e)
        {
            ExamMergeDataPak_TargetPath.Text = Util.ShowSelectDialog(false, true, "数据包目录", string.Empty).FirstOrDefault();
        }

        private void ExamMergeDataPak_MergeButton_Click(object sender, EventArgs e)
        {
            var sourceRoot = ExamMergeDataPak_SourcePath.Text?.Trim();
            var targetRoot = ExamMergeDataPak_TargetPath.Text?.Trim();

            if (!string.IsNullOrEmpty(sourceRoot) && !string.IsNullOrEmpty(targetRoot))
            {
                var sourceMdb = Path.Combine(sourceRoot, "Exam_Base.mdb");
                var targetMdb = Path.Combine(targetRoot, "Exam_Base.mdb");

                if (!File.Exists(sourceMdb) || !File.Exists(targetMdb))
                {
                    MessageBox.Show("数据包目录下未找到 Exam_Base.mdb。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

            var confirm = MessageBox.Show(
                "即将合并，该操作不可撤销，是否继续？",
                "提示",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            var btn = sender as Button;
            if (btn != null) btn.Enabled = false;

            try
            {
                ExamMergeDataPak_SourceSelect.Enabled = false;
                ExamMergeDataPak_TargetSelect.Enabled = false;
            }
            catch { }

            var copyExam = ExamMergeDataPak_CopyExam.Checked;

            Task.Run(
                () =>
                {
                    try
                    {
                        var factory = new AccessDatabaseFactory();

                        // 1. 合并 Exam_People：源全部插入到目标
                        MergeExamPeople(sourceMdb, targetMdb, factory);

                        // 2. 合并 Unit_Ess：按目标库最大 AtOrder 递增
                        MergeUnitEss(sourceMdb, targetMdb, factory);

                        // 3. 复制 .exam 文件（可选）
                        if (copyExam)
                        {
                            CopyExamFiles(sourceRoot, targetRoot);
                        }

                        this.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    MessageBox.Show("数据包合并完成。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    finally
                    {
                        try
                        {
                            if (btn != null)
                            {
                                btn.BeginInvoke(new Action(() => btn.Enabled = true));
                            }
                        }
                        catch { }

                        try
                        {
                            ExamMergeDataPak_SourceSelect.BeginInvoke(new Action(() => ExamMergeDataPak_SourceSelect.Enabled = true));
                        }
                        catch { }

                        try
                        {
                            ExamMergeDataPak_TargetSelect.BeginInvoke(new Action(() => ExamMergeDataPak_TargetSelect.Enabled = true));
                        }
                        catch { }
                    }
                });
            }

            
        }

        private static void MergeExamPeople(string sourceMdb, string targetMdb, DataBaseFactory.IDatabaseMethodFactory factory)
        {
            var accessFactory = factory as AccessDatabaseFactory ?? new AccessDatabaseFactory();

            using (var sourceConn = accessFactory.CreateConnection(sourceMdb))
            using (var targetConn = accessFactory.CreateConnection(targetMdb))
            {
                sourceConn.Open();
                targetConn.Open();

                using (var sourceCmd = sourceConn.CreateCommand())
                {
                    sourceCmd.CommandText = "SELECT * FROM Exam_People";
                    using (var reader = sourceCmd.ExecuteReader())
                    {
                        var schemaTable = reader.GetSchemaTable();
                        if (schemaTable == null)
                        {
                            return;
                        }

                        // 构造 INSERT 语句（不包含自增列，按现有库结构简单处理）
                        var columnNames = new System.Collections.Generic.List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            if (!string.IsNullOrEmpty(name))
                            {
                                columnNames.Add(name);
                            }
                        }

                        if (columnNames.Count == 0)
                        {
                            return;
                        }

                        var columnList = string.Join(",", columnNames.Select(n => $"[{n}]"));
                        var paramList = string.Join(",", columnNames.Select((n, idx) => $"@p{idx}"));

                        using (var insertCmd = accessFactory.CreateCommand(string.Empty, targetConn))
                        {
                            insertCmd.CommandText = $"INSERT INTO Exam_People ({columnList}) VALUES ({paramList})";

                            for (int i = 0; i < columnNames.Count; i++)
                            {
                                accessFactory.CreateCommandParameter(insertCmd, $"@p{i}", DBNull.Value);
                            }

                            while (reader.Read())
                            {
                                for (int i = 0; i < columnNames.Count; i++)
                                {
                                    var value = reader[columnNames[i]] ?? DBNull.Value;
                                    insertCmd.Parameters[i].Value = value;
                                }

                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        private static void MergeUnitEss(string sourceMdb, string targetMdb, DataBaseFactory.IDatabaseMethodFactory factory)
        {
            var accessFactory = factory as AccessDatabaseFactory ?? new AccessDatabaseFactory();

            using (var sourceConn = accessFactory.CreateConnection(sourceMdb))
            using (var targetConn = accessFactory.CreateConnection(targetMdb))
            {
                sourceConn.Open();
                targetConn.Open();

                int maxAtOrder = 0;
                using (var maxCmd = targetConn.CreateCommand())
                {
                    maxCmd.CommandText = "SELECT MAX(AtOrder) FROM Unit_Ess";
                    var obj = maxCmd.ExecuteScalar();
                    if (obj != DBNull.Value && obj != null)
                    {
                        int.TryParse(obj.ToString(), out maxAtOrder);
                    }
                }

                using (var sourceCmd = sourceConn.CreateCommand())
                {
                    sourceCmd.CommandText = "SELECT * FROM Unit_Ess";
                    using (var reader = sourceCmd.ExecuteReader())
                    {
                        var schemaTable = reader.GetSchemaTable();
                        if (schemaTable == null)
                        {
                            return;
                        }

                        var columnNames = new System.Collections.Generic.List<string>();
                        int atOrderIndex = -1;

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            if (string.Equals(name, "AtOrder", StringComparison.OrdinalIgnoreCase))
                            {
                                atOrderIndex = i;
                            }

                            if (!string.IsNullOrEmpty(name))
                            {
                                columnNames.Add(name);
                            }
                        }

                        if (columnNames.Count == 0)
                        {
                            return;
                        }

                        var columnList = string.Join(",", columnNames.Select(n => $"[{n}]"));
                        var paramList = string.Join(",", columnNames.Select((n, idx) => $"@p{idx}"));

                        using (var insertCmd = accessFactory.CreateCommand(string.Empty, targetConn))
                        {
                            insertCmd.CommandText = $"INSERT INTO Unit_Ess ({columnList}) VALUES ({paramList})";

                            for (int i = 0; i < columnNames.Count; i++)
                            {
                                accessFactory.CreateCommandParameter(insertCmd, $"@p{i}", DBNull.Value);
                            }

                            while (reader.Read())
                            {
                                maxAtOrder++;

                                for (int i = 0; i < columnNames.Count; i++)
                                {
                                    object value;

                                    if (string.Equals(columnNames[i], "AtOrder", StringComparison.OrdinalIgnoreCase))
                                    {
                                        value = maxAtOrder;
                                    }
                                    else
                                    {
                                        value = reader[columnNames[i]] ?? DBNull.Value;
                                    }

                                    insertCmd.Parameters[i].Value = value;
                                }

                                insertCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
        }

        private static void CopyExamFiles(string sourceRoot, string targetRoot)
        {
            if (!Directory.Exists(sourceRoot) || !Directory.Exists(targetRoot))
            {
                return;
            }

            var examFiles = Directory.GetFiles(sourceRoot, "*.exam", SearchOption.TopDirectoryOnly);
            foreach (var src in examFiles)
            {
                try
                {
                    var fileName = Path.GetFileName(src);
                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    var dest = Path.Combine(targetRoot, fileName);
                    File.Copy(src, dest, true);
                }
                catch
                {
                    // 单个文件失败不影响整体
                }
            }
        }
    }
}