using Markdig;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace DocumentTools
{
    public partial class PublicNoticeDialog : Form
    {
        public PublicNoticeDialog()
        {
            InitializeComponent();
        }

        private async void PublicNoticeDialog_Load(object sender, EventArgs e)
        {
            try
            {
                await NoticeWeb.EnsureCoreWebView2Async(null);

                NoticeWeb.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
                NoticeWeb.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                NoticeWeb.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                NoticeWeb.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                NoticeWeb.CoreWebView2.Settings.IsSwipeNavigationEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            try
            {
                NoticeWeb.CoreWebView2.Navigate(VersionInfo.VersionNoticeMessageUrl);
            }
            catch (Exception ex)
            {
                NoticeWeb.CoreWebView2.NavigateToString(ex.ToString());
            }
        }

        private void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            this.Text = "正在获取公告信息中...";
        }

        private void CoreWebView2_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                this.Text = "公告信息获取完毕";
            }
            else
            {
                this.Text = "公告信息获取失败，请重试";
            }
        }

        private void CoreWebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            Process.Start(e.Uri);

            e.Handled = true;
        }

        private string GetNoticeMeeage(string NoticeMessageUrl)
        {
            string ReturnDownloadString = Util.DownloadString(NoticeMessageUrl);

            return string.IsNullOrEmpty(ReturnDownloadString) ? $"## 暂时没有公告内容或网络异常无法获取公告内容{Environment.NewLine}**{Util.GetNowDateTime()}**" : ReturnDownloadString;
        }

        private string ConvertMarkDown2Html(string MarkdownString)
        {
            return Markdown.ToHtml(MarkdownString);
        }
    }
}
