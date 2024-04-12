namespace DocumentTools
{
    partial class PublicNoticeDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.NoticeWeb = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.NoticeWeb)).BeginInit();
            this.SuspendLayout();
            // 
            // NoticeWeb
            // 
            this.NoticeWeb.AllowExternalDrop = true;
            this.NoticeWeb.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.NoticeWeb.CreationProperties = null;
            this.NoticeWeb.DefaultBackgroundColor = System.Drawing.Color.White;
            this.NoticeWeb.Location = new System.Drawing.Point(3, 0);
            this.NoticeWeb.Name = "NoticeWeb";
            this.NoticeWeb.Size = new System.Drawing.Size(1248, 825);
            this.NoticeWeb.TabIndex = 0;
            this.NoticeWeb.ZoomFactor = 1D;
            // 
            // PublicNoticeDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1252, 827);
            this.Controls.Add(this.NoticeWeb);
            this.MinimizeBox = false;
            this.Name = "PublicNoticeDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "公告信息";
            this.Load += new System.EventHandler(this.PublicNoticeDialog_Load);
            ((System.ComponentModel.ISupportInitialize)(this.NoticeWeb)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 NoticeWeb;
    }
}