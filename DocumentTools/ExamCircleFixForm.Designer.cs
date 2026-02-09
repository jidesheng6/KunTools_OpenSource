namespace DocumentTools
{
    partial class ExamCircleFixForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExamCircleFixForm));
            this.pnlMain = new System.Windows.Forms.Panel();
            this.picMain = new System.Windows.Forms.PictureBox();
            this.pnlTools = new System.Windows.Forms.Panel();
            this.btnClearAllLocations = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnManualAdd = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.numRectH = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numRectW = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnClearSingle = new System.Windows.Forms.Button();
            this.btnDoFix = new System.Windows.Forms.Button();
            this.btnStartMatch = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.numSimilarity = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbZoom = new System.Windows.Forms.ComboBox();
            this.btnSelectRange = new System.Windows.Forms.Button();
            this.btnSelectTemplate = new System.Windows.Forms.Button();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.picTemplatePreview = new System.Windows.Forms.PictureBox();
            this.btnSelectReplacementImage = new System.Windows.Forms.Button();
            this.pnlMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picMain)).BeginInit();
            this.pnlTools.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRectH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRectW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSimilarity)).BeginInit();
            this.grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picTemplatePreview)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlMain
            // 
            this.pnlMain.AutoScroll = true;
            this.pnlMain.BackColor = System.Drawing.SystemColors.ControlDark;
            this.pnlMain.Controls.Add(this.picMain);
            this.pnlMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMain.Location = new System.Drawing.Point(0, 0);
            this.pnlMain.Name = "pnlMain";
            this.pnlMain.Size = new System.Drawing.Size(820, 781);
            this.pnlMain.TabIndex = 0;
            // 
            // picMain
            // 
            this.picMain.BackColor = System.Drawing.Color.White;
            this.picMain.Location = new System.Drawing.Point(0, 0);
            this.picMain.Name = "picMain";
            this.picMain.Size = new System.Drawing.Size(100, 100);
            this.picMain.TabIndex = 0;
            this.picMain.TabStop = false;
            // 
            // pnlTools
            // 
            this.pnlTools.Controls.Add(this.btnClearAllLocations);
            this.pnlTools.Controls.Add(this.btnExit);
            this.pnlTools.Controls.Add(this.btnSave);
            this.pnlTools.Controls.Add(this.btnManualAdd);
            this.pnlTools.Controls.Add(this.groupBox1);
            this.pnlTools.Controls.Add(this.btnReset);
            this.pnlTools.Controls.Add(this.btnClearSingle);
            this.pnlTools.Controls.Add(this.btnDoFix);
            this.pnlTools.Controls.Add(this.btnStartMatch);
            this.pnlTools.Controls.Add(this.label2);
            this.pnlTools.Controls.Add(this.numSimilarity);
            this.pnlTools.Controls.Add(this.label1);
            this.pnlTools.Controls.Add(this.cmbZoom);
            this.pnlTools.Controls.Add(this.btnSelectRange);
            this.pnlTools.Controls.Add(this.btnSelectTemplate);
            this.pnlTools.Controls.Add(this.grpPreview);
            this.pnlTools.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlTools.Location = new System.Drawing.Point(820, 0);
            this.pnlTools.Name = "pnlTools";
            this.pnlTools.Size = new System.Drawing.Size(184, 781);
            this.pnlTools.TabIndex = 1;
            // 
            // btnClearAllLocations
            // 
            this.btnClearAllLocations.Location = new System.Drawing.Point(19, 600);
            this.btnClearAllLocations.Name = "btnClearAllLocations";
            this.btnClearAllLocations.Size = new System.Drawing.Size(148, 35);
            this.btnClearAllLocations.TabIndex = 15;
            this.btnClearAllLocations.Text = "清空所有定位";
            this.btnClearAllLocations.UseVisualStyleBackColor = true;
            this.btnClearAllLocations.Click += new System.EventHandler(this.btnClearAllLocations_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(19, 735);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(148, 35);
            this.btnExit.TabIndex = 13;
            this.btnExit.Text = "退出";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.SkyBlue;
            this.btnSave.Location = new System.Drawing.Point(19, 685);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(148, 45);
            this.btnSave.TabIndex = 12;
            this.btnSave.Text = "保存图片";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnManualAdd
            // 
            this.btnManualAdd.Location = new System.Drawing.Point(19, 520);
            this.btnManualAdd.Name = "btnManualAdd";
            this.btnManualAdd.Size = new System.Drawing.Size(148, 35);
            this.btnManualAdd.TabIndex = 11;
            this.btnManualAdd.Text = "手动定位(补漏)";
            this.btnManualAdd.UseVisualStyleBackColor = true;
            this.btnManualAdd.Click += new System.EventHandler(this.btnManualAdd_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.numRectH);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numRectW);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(19, 305);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(148, 80);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "当前选框尺寸";
            // 
            // numRectH
            // 
            this.numRectH.Location = new System.Drawing.Point(40, 48);
            this.numRectH.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numRectH.Name = "numRectH";
            this.numRectH.Size = new System.Drawing.Size(90, 21);
            this.numRectH.TabIndex = 3;
            this.numRectH.ValueChanged += new System.EventHandler(this.numRectDimension_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(23, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "高:";
            // 
            // numRectW
            // 
            this.numRectW.Location = new System.Drawing.Point(40, 20);
            this.numRectW.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numRectW.Name = "numRectW";
            this.numRectW.Size = new System.Drawing.Size(90, 21);
            this.numRectW.TabIndex = 1;
            this.numRectW.ValueChanged += new System.EventHandler(this.numRectDimension_ValueChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 12);
            this.label3.TabIndex = 0;
            this.label3.Text = "宽:";
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(19, 640);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(148, 35);
            this.btnReset.TabIndex = 9;
            this.btnReset.Text = "重置图片";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnClearSingle
            // 
            this.btnClearSingle.Location = new System.Drawing.Point(19, 560);
            this.btnClearSingle.Name = "btnClearSingle";
            this.btnClearSingle.Size = new System.Drawing.Size(148, 35);
            this.btnClearSingle.TabIndex = 8;
            this.btnClearSingle.Text = "清除选中的定位";
            this.btnClearSingle.UseVisualStyleBackColor = true;
            this.btnClearSingle.Click += new System.EventHandler(this.btnClearSingle_Click);
            // 
            // btnDoFix
            // 
            this.btnDoFix.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnDoFix.Location = new System.Drawing.Point(19, 460);
            this.btnDoFix.Name = "btnDoFix";
            this.btnDoFix.Size = new System.Drawing.Size(148, 45);
            this.btnDoFix.TabIndex = 7;
            this.btnDoFix.Text = "开始修复";
            this.btnDoFix.UseVisualStyleBackColor = false;
            this.btnDoFix.Click += new System.EventHandler(this.btnDoFix_Click);
            // 
            // btnStartMatch
            // 
            this.btnStartMatch.Enabled = false;
            this.btnStartMatch.Location = new System.Drawing.Point(19, 410);
            this.btnStartMatch.Name = "btnStartMatch";
            this.btnStartMatch.Size = new System.Drawing.Size(148, 35);
            this.btnStartMatch.TabIndex = 6;
            this.btnStartMatch.Text = "开始查找";
            this.btnStartMatch.UseVisualStyleBackColor = true;
            this.btnStartMatch.Click += new System.EventHandler(this.btnStartMatch_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 255);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "匹配相似度(%)";
            // 
            // numSimilarity
            // 
            this.numSimilarity.Location = new System.Drawing.Point(19, 273);
            this.numSimilarity.Name = "numSimilarity";
            this.numSimilarity.Size = new System.Drawing.Size(148, 21);
            this.numSimilarity.TabIndex = 4;
            this.numSimilarity.Value = new decimal(new int[] {
            80,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 205);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "显示缩放";
            // 
            // cmbZoom
            // 
            this.cmbZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbZoom.FormattingEnabled = true;
            this.cmbZoom.Items.AddRange(new object[] {
            "100%",
            "90%",
            "80%",
            "70%",
            "60%",
            "50%"});
            this.cmbZoom.Location = new System.Drawing.Point(19, 223);
            this.cmbZoom.Name = "cmbZoom";
            this.cmbZoom.Size = new System.Drawing.Size(148, 20);
            this.cmbZoom.TabIndex = 2;
            this.cmbZoom.SelectedIndexChanged += new System.EventHandler(this.cmbZoom_SelectedIndexChanged);
            // 
            // btnSelectRange
            // 
            this.btnSelectRange.Location = new System.Drawing.Point(19, 65);
            this.btnSelectRange.Name = "btnSelectRange";
            this.btnSelectRange.Size = new System.Drawing.Size(148, 35);
            this.btnSelectRange.TabIndex = 1;
            this.btnSelectRange.Text = "2. 自动查找";
            this.btnSelectRange.UseVisualStyleBackColor = true;
            this.btnSelectRange.Click += new System.EventHandler(this.btnSelectRange_Click);
            // 
            // btnSelectTemplate
            // 
            this.btnSelectTemplate.Location = new System.Drawing.Point(19, 15);
            this.btnSelectTemplate.Name = "btnSelectTemplate";
            this.btnSelectTemplate.Size = new System.Drawing.Size(148, 35);
            this.btnSelectTemplate.TabIndex = 0;
            this.btnSelectTemplate.Text = "1. 选择区域";
            this.btnSelectTemplate.UseVisualStyleBackColor = true;
            this.btnSelectTemplate.Click += new System.EventHandler(this.btnSelectTemplate_Click);
            // 
            // grpPreview
            // 
            this.grpPreview.Controls.Add(this.picTemplatePreview);
            this.grpPreview.Controls.Add(this.btnSelectReplacementImage);
            this.grpPreview.Location = new System.Drawing.Point(19, 105);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(148, 100);
            this.grpPreview.TabIndex = 14;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = "模板预览";
            // 
            // picTemplatePreview
            // 
            this.picTemplatePreview.BackColor = System.Drawing.Color.Black;
            this.picTemplatePreview.Location = new System.Drawing.Point(6, 31);
            this.picTemplatePreview.Name = "picTemplatePreview";
            this.picTemplatePreview.Size = new System.Drawing.Size(50, 50);
            this.picTemplatePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picTemplatePreview.TabIndex = 0;
            this.picTemplatePreview.TabStop = false;
            // 
            // btnSelectReplacementImage
            // 
            this.btnSelectReplacementImage.Enabled = false;
            this.btnSelectReplacementImage.Location = new System.Drawing.Point(62, 31);
            this.btnSelectReplacementImage.Name = "btnSelectReplacementImage";
            this.btnSelectReplacementImage.Size = new System.Drawing.Size(80, 50);
            this.btnSelectReplacementImage.TabIndex = 1;
            this.btnSelectReplacementImage.Text = "选择图片...";
            this.btnSelectReplacementImage.UseVisualStyleBackColor = true;
            this.btnSelectReplacementImage.Click += new System.EventHandler(this.btnSelectReplacementImage_Click);
            // 
            // ExamCircleFixForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1004, 781);
            this.Controls.Add(this.pnlMain);
            this.Controls.Add(this.pnlTools);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ExamCircleFixForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "考评圆圈修复工具";
            this.pnlMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picMain)).EndInit();
            this.pnlTools.ResumeLayout(false);
            this.pnlTools.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRectH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRectW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numSimilarity)).EndInit();
            this.grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picTemplatePreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlMain;
        private System.Windows.Forms.PictureBox picMain;
        private System.Windows.Forms.Panel pnlTools;
        private System.Windows.Forms.Button btnSelectTemplate;
        private System.Windows.Forms.Button btnSelectRange;
        private System.Windows.Forms.ComboBox cmbZoom;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numSimilarity;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStartMatch;
        private System.Windows.Forms.Button btnDoFix;
        private System.Windows.Forms.Button btnClearSingle;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown numRectH;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numRectW;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnManualAdd;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.GroupBox grpPreview;
        private System.Windows.Forms.PictureBox picTemplatePreview;
        private System.Windows.Forms.Button btnSelectReplacementImage;
        private System.Windows.Forms.Button btnClearAllLocations;
    }
}
