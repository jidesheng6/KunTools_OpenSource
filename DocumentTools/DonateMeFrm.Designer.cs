namespace DocumentTools
{
    partial class DonateMeFrm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.WechatDonatePicture = new System.Windows.Forms.PictureBox();
            this.ShowWechatQR = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WechatDonatePicture)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.groupBox1.Controls.Add(this.WechatDonatePicture);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(246, 273);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "微信";
            // 
            // WechatDonatePicture
            // 
            this.WechatDonatePicture.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.WechatDonatePicture.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.WechatDonatePicture.Image = global::DocumentTools.Properties.Resources._6f258af2c19c9c3cdb97517573506c11ce48ff98;
            this.WechatDonatePicture.InitialImage = null;
            this.WechatDonatePicture.Location = new System.Drawing.Point(6, 27);
            this.WechatDonatePicture.Name = "WechatDonatePicture";
            this.WechatDonatePicture.Size = new System.Drawing.Size(234, 240);
            this.WechatDonatePicture.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.WechatDonatePicture.TabIndex = 0;
            this.WechatDonatePicture.TabStop = false;
            // 
            // ShowWechatQR
            // 
            this.ShowWechatQR.Location = new System.Drawing.Point(47, 291);
            this.ShowWechatQR.Name = "ShowWechatQR";
            this.ShowWechatQR.Size = new System.Drawing.Size(185, 57);
            this.ShowWechatQR.TabIndex = 2;
            this.ShowWechatQR.Text = "加载赞赏码";
            this.ShowWechatQR.UseVisualStyleBackColor = true;
            this.ShowWechatQR.Click += new System.EventHandler(this.ShowWechatQR_Click);
            // 
            // DonateMeFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(290, 360);
            this.Controls.Add(this.ShowWechatQR);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DonateMeFrm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "谢谢老板！";
            this.Load += new System.EventHandler(this.DonateMeFrm_Load);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.WechatDonatePicture)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ShowWechatQR;
        private System.Windows.Forms.PictureBox WechatDonatePicture;
    }
}