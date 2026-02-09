using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using System.Drawing.Imaging;
using DrawingPoint = System.Drawing.Point;
using DrawingRectangle = System.Drawing.Rectangle;
using DrawingSize = System.Drawing.Size;

namespace DocumentTools
{
    public partial class ExamCircleFixForm : Form
    {
        private enum SelectMode
        {
            None = 0,
            SearchRange = 1,
            Template = 2,
            Manual = 3,
        }

        private string _imagePath;
        private string _suggestedFileName; // 建议保存的文件名
        private Bitmap _originalBmp;
        private Bitmap _resetBackupBmp; // 新增：用于重置图片的初始备份
        private Bitmap _displayBmp;
        private float _zoom = 1.0f;

        private DrawingRectangle _searchRange = DrawingRectangle.Empty;
        private DrawingRectangle _templateRect = DrawingRectangle.Empty;

        private readonly List<DrawingRectangle> _matchRects = new List<DrawingRectangle>();
        private DrawingRectangle _selectedRect = DrawingRectangle.Empty;
        private int _selectedIndex = -1;

        private SelectMode _mode = SelectMode.None;
        private bool _isMouseDown;
        private DrawingPoint _mouseDownPtDisplay;
        private DrawingRectangle _mouseDownRectDisplay;

        private enum DragKind
        {
            None = 0,
            DrawNew = 1,
            MoveRect = 2,
            ResizeTL = 3,
            ResizeTR = 4,
            ResizeBL = 5,
            ResizeBR = 6,
        }

        private DragKind _dragKind = DragKind.None;
        private const int HandleSize = 10;

        public ExamCircleFixForm(string imagePath)
            : this()
        {
            _imagePath = imagePath;
            if (!string.IsNullOrEmpty(imagePath))
            {
                _suggestedFileName = Path.GetFileNameWithoutExtension(imagePath);
            }
        }

        public ExamCircleFixForm(Bitmap bmp, string suggestedName = "")
            : this()
        {
            if (bmp != null)
            {
                _originalBmp = (Bitmap)bmp.Clone();
                _resetBackupBmp = (Bitmap)bmp.Clone(); // 备份初始状态用于重置
                _imagePath = "MemoryStream";
                _suggestedFileName = string.IsNullOrEmpty(suggestedName) ? "Scan_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") : suggestedName;
            }
        }

        public ExamCircleFixForm()
        {
            InitializeComponent();

            this.Load += ExamCircleFixForm_Load;
            this.FormClosing += ExamCircleFixForm_FormClosing;
            this.KeyDown += ExamCircleFixForm_KeyDown;

            picMain.Paint += PicMain_Paint;
            picMain.MouseDown += PicMain_MouseDown;
            picMain.MouseMove += PicMain_MouseMove;
            picMain.MouseUp += PicMain_MouseUp;

            cmbZoom.SelectedIndex = 0; // 100%
        }

        private void ExamCircleFixForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (_originalBmp == null) return;

            // Delete 键删除选中结果框
            if (e.KeyCode == Keys.Delete)
            {
                if (_focusTarget == FocusTarget.MatchRect && _selectedIndex >= 0)
                {
                    _matchRects.RemoveAt(_selectedIndex);
                    _selectedIndex = -1;
                    _selectedRect = DrawingRectangle.Empty;
                    _focusTarget = FocusTarget.None;
                    picMain.Invalidate();
                    SyncDimensionFromFocusedRect();
                }
                return;
            }

            // 方向键微调位置
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right || e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                int step = e.Shift ? 10 : 1;
                int dx = 0, dy = 0;
                if (e.KeyCode == Keys.Left) dx = -step;
                if (e.KeyCode == Keys.Right) dx = step;
                if (e.KeyCode == Keys.Up) dy = -step;
                if (e.KeyCode == Keys.Down) dy = step;

                if (_focusTarget == FocusTarget.Template)
                {
                    _templateRect = ClampToImage(new DrawingRectangle(_templateRect.X + dx, _templateRect.Y + dy, _templateRect.Width, _templateRect.Height));
                    UpdateTemplatePreview();
                }
                else if (_focusTarget == FocusTarget.SearchRange)
                {
                    _searchRange = ClampToImage(new DrawingRectangle(_searchRange.X + dx, _searchRange.Y + dy, _searchRange.Width, _searchRange.Height));
                }
                else if (_focusTarget == FocusTarget.MatchRect && _selectedIndex >= 0)
                {
                    var r = _matchRects[_selectedIndex];
                    _matchRects[_selectedIndex] = ClampToImage(new DrawingRectangle(r.X + dx, r.Y + dy, r.Width, r.Height));
                    _selectedRect = _matchRects[_selectedIndex];
                    SyncDimensionFromFocusedRect();
                }

                picMain.Invalidate();
                e.Handled = true;
            }
        }

        private void EnsureInitTemplateRect()
        {
            if (_templateRect.IsEmpty && _originalBmp != null)
            {
                int w = Math.Min(30, _originalBmp.Width);
                int h = Math.Min(30, _originalBmp.Height);
                int x = (_originalBmp.Width - w) / 2;
                int y = (_originalBmp.Height - h) / 2;
                _templateRect = ClampToImage(new DrawingRectangle(x, y, w, h));
            }
        }

        private void EnsureInitSearchRangeRect()
        {
            if (_searchRange.IsEmpty && _originalBmp != null)
            {
                // 默认给一个与图片尺寸相关的范围（约 80%），并留出边距
                int w = Math.Max(1, (int)Math.Round(_originalBmp.Width * 0.8));
                int h = Math.Max(1, (int)Math.Round(_originalBmp.Height * 0.8));

                w = Math.Min(w, _originalBmp.Width);
                h = Math.Min(h, _originalBmp.Height);

                int x = (_originalBmp.Width - w) / 2;
                int y = (_originalBmp.Height - h) / 2;

                _searchRange = ClampToImage(new DrawingRectangle(x, y, w, h));
            }
        }

        private void SyncDimensionFromFocusedRect()
        {
            _suppressDimensionEvent = true;
            DrawingRectangle r = DrawingRectangle.Empty;
            if (_focusTarget == FocusTarget.Template) r = _templateRect;
            else if (_focusTarget == FocusTarget.SearchRange) r = _searchRange;
            else if (_focusTarget == FocusTarget.MatchRect && _selectedIndex >= 0) r = _matchRects[_selectedIndex];

            if (!r.IsEmpty)
            {
                numRectW.Value = r.Width;
                numRectH.Value = r.Height;
            }
            else
            {
                numRectW.Value = 0;
                numRectH.Value = 0;
            }
            _suppressDimensionEvent = false;
        }

        private void numRectDimension_ValueChanged(object sender, EventArgs e)
        {
            if (_suppressDimensionEvent || _originalBmp == null) return;

            int w = (int)numRectW.Value;
            int h = (int)numRectH.Value;
            if (w <= 0 || h <= 0) return;

            if (_focusTarget == FocusTarget.Template)
            {
                _templateRect = ClampToImage(new DrawingRectangle(_templateRect.X, _templateRect.Y, w, h));
                UpdateTemplatePreview();
            }
            else if (_focusTarget == FocusTarget.SearchRange)
            {
                _searchRange = ClampToImage(new DrawingRectangle(_searchRange.X, _searchRange.Y, w, h));
            }
            else if (_focusTarget == FocusTarget.MatchRect && _selectedIndex >= 0)
            {
                var r = _matchRects[_selectedIndex];
                _matchRects[_selectedIndex] = ClampToImage(new DrawingRectangle(r.X, r.Y, w, h));
                _selectedRect = _matchRects[_selectedIndex];
            }
            picMain.Invalidate();
        }

        private void btnManualAdd_Click(object sender, EventArgs e)
        {
            if (_originalBmp == null) return;

            // 保存当前滚动位置，防止 Focus() 导致跳回顶部
            DrawingPoint oldPos = pnlMain.AutoScrollPosition;

            // 手动补漏：在当前可视区域中心生成
            int w = _templateRect.Width > 0 ? _templateRect.Width : 30;
            int h = _templateRect.Height > 0 ? _templateRect.Height : 30;

            // AutoScrollPosition 是屏幕坐标偏移，读取时通常为负数
            int viewX = -oldPos.X;
            int viewY = -oldPos.Y;

            // 计算可视区域中心点在 PictureBox 上的显示坐标
            int centerXDisp = viewX + pnlMain.ClientSize.Width / 2;
            int centerYDisp = viewY + pnlMain.ClientSize.Height / 2;

            // 换算回原始图片坐标
            int x = (int)(centerXDisp / _zoom - w / 2.0);
            int y = (int)(centerYDisp / _zoom - h / 2.0);

            var newRect = ClampToImage(new DrawingRectangle(x, y, w, h));
            _matchRects.Add(newRect);
            _selectedIndex = _matchRects.Count - 1;
            _selectedRect = newRect;
            _focusTarget = FocusTarget.MatchRect;
            _mode = SelectMode.Manual;

            SyncDimensionFromFocusedRect();
            
            // 聚焦并恢复滚动位置
            picMain.Focus();
            pnlMain.AutoScrollPosition = new DrawingPoint(-oldPos.X, -oldPos.Y);
            
            picMain.Invalidate();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_originalBmp == null) return;
            
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "JPEG图片|*.jpg";
                string baseName = string.IsNullOrEmpty(_suggestedFileName) ? "修复后图片" : _suggestedFileName;
                sfd.FileName = baseName + "_修复后.jpg";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // 1. 创建副本保存，避免 GDI+ 因为图片被 PictureBox 占用或 LockBits 状态未完全清除导致“一般性错误”
                        using (Bitmap saveBmp = new Bitmap(_originalBmp))
                        {
                            // 2. 强制以高质量保存 JPG
                            ImageCodecInfo encoder = ImageCodecInfo.GetImageEncoders().FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);
                            if (encoder == null)
                            {
                                saveBmp.Save(sfd.FileName, ImageFormat.Jpeg);
                            }
                            else
                            {
                                using (EncoderParameters parameters = new EncoderParameters(1))
                                {
                                    parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 95L);
                                    saveBmp.Save(sfd.FileName, encoder, parameters);
                                }
                            }
                        }
                        MessageBox.Show("保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("保存失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private Bitmap _replacementBmp;

        private void ExamCircleFixForm_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized; // 进入窗体最大化

            if(_originalBmp != null)
            {
                // 从内存 Bitmap 进入
                numRectW.Maximum = _originalBmp.Width;
                numRectH.Maximum = _originalBmp.Height;

                ApplyZoom(_zoom);

                _searchRange = DrawingRectangle.Empty;
                _templateRect = DrawingRectangle.Empty;
                _showTemplateRect = false;
                _showSearchRangeRect = false;

                _matchRects.Clear();
                _selectedRect = DrawingRectangle.Empty;
                _selectedIndex = -1;

                picMain.Invalidate();
                return;
            }

            if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
            {
                LoadImage(_imagePath);
            }
        }

        private void UpdateTemplatePreview()
        {
            if (_originalBmp == null) return;

            // 释放旧预览
            if (picTemplatePreview.Image != null)
            {
                var old = picTemplatePreview.Image;
                picTemplatePreview.Image = null;
                // 注意：如果是 _replacementBmp，不在这里 Dispose，因为它是持久引用的
                if (old != _replacementBmp) old.Dispose();
            }

            if (_replacementBmp != null)
            {
                // 方案B：选了替代图后显示替代图
                picTemplatePreview.Image = _replacementBmp;
            }
            else if (!_templateRect.IsEmpty)
            {
                // 显示当前模板框抠出来的图
                try
                {
                    var rect = ClampToImage(_templateRect);
                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        using (var srcMat = BitmapConverter.ToMat(_originalBmp))
                        using (var roi = new Mat(srcMat, new OpenCvSharp.Rect(rect.X, rect.Y, rect.Width, rect.Height)))
                        {
                            // ToBitmap 返回的新 Bitmap 独立于原图，不会触发 Clone 的 GDI+ 异常
                            picTemplatePreview.Image = BitmapConverter.ToBitmap(roi);
                        }
                    }
                }
                catch { }
            }
        }

        private void btnSelectReplacementImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "图片文件|*.jpg;*.png;*.bmp;*.jpeg";
                ofd.Title = "选择用于覆盖的替代图片";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var fs = new FileStream(ofd.FileName, FileMode.Open, FileAccess.Read))
                        {
                            _replacementBmp?.Dispose();
                            _replacementBmp = new Bitmap(fs);
                        }
                        UpdateTemplatePreview();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("加载替代图片失败：" + ex.Message);
                    }
                }
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ExamCircleFixForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (picMain.Image != null)
            {
                var img = picMain.Image;
                picMain.Image = null;
                try { img.Dispose(); } catch { }
            }

            if (picTemplatePreview.Image != null)
            {
                var img = picTemplatePreview.Image;
                picTemplatePreview.Image = null;
                try { img.Dispose(); } catch { }
            }

            _displayBmp?.Dispose();
            _displayBmp = null;

            _originalBmp?.Dispose();
            _originalBmp = null;

            _resetBackupBmp?.Dispose();
            _resetBackupBmp = null;

            _replacementBmp?.Dispose();
            _replacementBmp = null;

            // 强制建议GC回收，确保大图占用的内存及时归还系统
            GC.Collect();
        }

        private void LoadImage(string path)
        {
            _imagePath = path;

            _originalBmp?.Dispose();
            _originalBmp = null;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _originalBmp = new Bitmap(fs);
            }

            // 数值框边界限制（不允许超过图片宽高）
            if (_originalBmp != null)
            {
                numRectW.Maximum = _originalBmp.Width;
                numRectH.Maximum = _originalBmp.Height;
            }

            ApplyZoom(_zoom);

            _searchRange = DrawingRectangle.Empty;
            _templateRect = DrawingRectangle.Empty;
            _showTemplateRect = false;
            _showSearchRangeRect = false;

            _matchRects.Clear();
            _selectedRect = DrawingRectangle.Empty;
            _selectedIndex = -1;

            picMain.Invalidate();
        }

        private void ApplyZoom(float zoom)
        {
            if (_originalBmp == null) return;

            _zoom = zoom;

            _displayBmp?.Dispose();
            _displayBmp = null;

            int w = Math.Max(1, (int)Math.Round(_originalBmp.Width * _zoom));
            int h = Math.Max(1, (int)Math.Round(_originalBmp.Height * _zoom));

            _displayBmp = new Bitmap(_originalBmp, new DrawingSize(w, h));

            if (picMain.Image != null)
            {
                var old = picMain.Image;
                picMain.Image = null;
                old.Dispose();
            }

            picMain.Image = _displayBmp;
            picMain.Size = _displayBmp.Size;

            picMain.Invalidate();
        }

        private static DrawingRectangle NormalizeRect(DrawingPoint p1, DrawingPoint p2)
        {
            int x1 = Math.Min(p1.X, p2.X);
            int y1 = Math.Min(p1.Y, p2.Y);
            int x2 = Math.Max(p1.X, p2.X);
            int y2 = Math.Max(p1.Y, p2.Y);
            return new DrawingRectangle(x1, y1, Math.Max(1, x2 - x1), Math.Max(1, y2 - y1));
        }

        private DrawingPoint DisplayToOriginal(DrawingPoint p)
        {
            return new DrawingPoint(
                (int)Math.Round(p.X / _zoom),
                (int)Math.Round(p.Y / _zoom));
        }

        private DrawingRectangle DisplayToOriginal(DrawingRectangle r)
        {
            return new DrawingRectangle(
                (int)Math.Round(r.X / _zoom),
                (int)Math.Round(r.Y / _zoom),
                (int)Math.Round(r.Width / _zoom),
                (int)Math.Round(r.Height / _zoom));
        }

        private DrawingRectangle OriginalToDisplay(DrawingRectangle r)
        {
            return new DrawingRectangle(
                (int)Math.Round(r.X * _zoom),
                (int)Math.Round(r.Y * _zoom),
                (int)Math.Round(r.Width * _zoom),
                (int)Math.Round(r.Height * _zoom));
        }

        private DrawingRectangle ClampToImage(DrawingRectangle r)
        {
            if (_originalBmp == null) return DrawingRectangle.Empty;

            int imgW = _originalBmp.Width;
            int imgH = _originalBmp.Height;

            int w = Math.Max(1, r.Width);
            int h = Math.Max(1, r.Height);

            if (w > imgW) w = imgW;
            if (h > imgH) h = imgH;

            int x = r.X;
            int y = r.Y;

            if (x < 0) x = 0;
            if (y < 0) y = 0;
            if (x > imgW - w) x = imgW - w;
            if (y > imgH - h) y = imgH - h;

            return new DrawingRectangle(x, y, w, h);
        }

        private enum FocusTarget
        {
            None = 0,
            Template = 1,
            SearchRange = 2,
            MatchRect = 3,
        }

        private FocusTarget _focusTarget = FocusTarget.None;
        private bool _suppressDimensionEvent;
        private bool _fixedDirty;

        private bool _showTemplateRect = false;
        private bool _showSearchRangeRect = false;

        private void btnSelectRange_Click(object sender, EventArgs e)
        {
            if (_originalBmp == null) return;

            // 点击“自动查找”后，才允许启用“开始查找”按钮
            btnStartMatch.Enabled = true;

            // 切换到搜索范围：只隐藏模板显示，不清除数据
            _showTemplateRect = false;
            _showSearchRangeRect = true;
            _selectedIndex = -1;
            _selectedRect = DrawingRectangle.Empty;

            _mode = SelectMode.SearchRange;
            _focusTarget = FocusTarget.SearchRange;

            EnsureInitSearchRangeRect();
            SyncDimensionFromFocusedRect();

            picMain.Focus();
            picMain.Invalidate();
        }

        private void btnSelectTemplate_Click(object sender, EventArgs e)
        {
            if (_originalBmp == null) return;

            // 切换到模板：只隐藏范围显示，不清除数据
            _showSearchRangeRect = false;
            _showTemplateRect = true;
            _selectedIndex = -1;
            _selectedRect = DrawingRectangle.Empty;

            _mode = SelectMode.Template;
            _focusTarget = FocusTarget.Template;

            EnsureInitTemplateRect();
            SyncDimensionFromFocusedRect();
            UpdateTemplatePreview();

            picMain.Focus();
            picMain.Invalidate();
        }

        private void cmbZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            string s = (cmbZoom.SelectedItem ?? "100%").ToString().Trim();
            if (s.EndsWith("%")) s = s.Substring(0, s.Length - 1);
            if (!int.TryParse(s, out int v)) v = 100;
            v = Math.Max(10, Math.Min(400, v));
            ApplyZoom(v / 100f);
        }

        private void btnStartMatch_Click(object sender, EventArgs e)
        {
            if (_originalBmp == null) return;
            if (_templateRect.IsEmpty)
            {
                MessageBox.Show("请先选择圆圈模板区域。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_searchRange.IsEmpty)
            {
                MessageBox.Show("请先选择自动查找范围。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var template = ClampToImage(_templateRect);
            var range = ClampToImage(_searchRange);
            if (template.IsEmpty || range.IsEmpty)
            {
                MessageBox.Show("选框超出图片范围，请重新选择。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (template.Width > range.Width || template.Height > range.Height)
            {
                MessageBox.Show("模板区域不能大于搜索范围。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            float thr = (float)numSimilarity.Value / 100f;
            if (thr < 0.01f) thr = 0.01f;
            if (thr > 1f) thr = 1f;

            _matchRects.Clear();
            _selectedRect = DrawingRectangle.Empty;
            _selectedIndex = -1;

            using (var srcMat = BitmapConverter.ToMat(_originalBmp))
            using (var tmplMat = new Mat(srcMat, new OpenCvSharp.Rect(template.X, template.Y, template.Width, template.Height)))
            using (var roiMat = new Mat(srcMat, new OpenCvSharp.Rect(range.X, range.Y, range.Width, range.Height)))
            using (var result = new Mat())
            {
                Cv2.MatchTemplate(roiMat, tmplMat, result, TemplateMatchModes.CCoeffNormed);

                while (true)
                {
                    Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);
                    if (maxVal < thr) break;

                    var found = new DrawingRectangle(range.X + maxLoc.X, range.Y + maxLoc.Y, template.Width, template.Height);
                    _matchRects.Add(found);

                    int x0 = Math.Max(0, maxLoc.X - template.Width / 2);
                    int y0 = Math.Max(0, maxLoc.Y - template.Height / 2);
                    int x1 = Math.Min(result.Cols, maxLoc.X + template.Width / 2);
                    int y1 = Math.Min(result.Rows, maxLoc.Y + template.Height / 2);
                    if (x1 > x0 && y1 > y0)
                    {
                        result[new OpenCvSharp.Rect(x0, y0, x1 - x0, y1 - y0)].SetTo(0);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            // 模式切换，隐藏模板/范围框
            _showTemplateRect = false;
            _showSearchRangeRect = false;
            _focusTarget = FocusTarget.None;

            // 自动查找结束后启用替代图片选择
            btnSelectReplacementImage.Enabled = true;

            _mode = SelectMode.Manual;
            if (_matchRects.Count > 0)
            {
                _selectedIndex = 0;
                _selectedRect = _matchRects[0];
                _focusTarget = FocusTarget.MatchRect;
                SyncDimensionFromFocusedRect();
            }
            
            picMain.Invalidate();

            MessageBox.Show($"查找完成：{_matchRects.Count} 处。\r\n您可以点击某个框进行单个删除(Del键)，或拖动手柄调节大小后再修复。\r\n现在您也可以选择替代图片进行覆盖。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDoFix_Click(object sender, EventArgs e)
        {
            if (_originalBmp == null) return;
            if (_templateRect.IsEmpty && _replacementBmp == null)
            {
                MessageBox.Show("请先选择圆圈模板区域或替代图片。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (_matchRects.Count == 0)
            {
                MessageBox.Show("当前没有要修复的定位区域。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var template = ClampToImage(_templateRect);
            
            BitmapData bmpData = null;
            try
            {
                bmpData = _originalBmp.LockBits(
                    new DrawingRectangle(0, 0, _originalBmp.Width, _originalBmp.Height),
                    ImageLockMode.ReadWrite,
                    _originalBmp.PixelFormat);

                int bits = Image.GetPixelFormatSize(_originalBmp.PixelFormat);
                int channels = bits / 8;

                MatType matType;
                if (channels == 4) matType = MatType.CV_8UC4;
                else if (channels == 3) matType = MatType.CV_8UC3;
                else if (channels == 1) matType = MatType.CV_8UC1;
                else
                {
                    MessageBox.Show($"不支持的像素格式：{_originalBmp.PixelFormat}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                using (var srcMat = Mat.FromPixelData(bmpData.Height, bmpData.Width, matType, bmpData.Scan0, bmpData.Stride))
                {
                    Mat patchMat = null;
                    bool shouldDisposePatch = false;

                    if (_replacementBmp != null)
                    {
                        // 使用替代图片
                        using (var tempMat = OpenCvSharp.Extensions.BitmapConverter.ToMat(_replacementBmp))
                        {
                            patchMat = new Mat();
                            if (channels == 4)
                            {
                                Cv2.CvtColor(tempMat, patchMat, ColorConversionCodes.BGR2BGRA);
                            }
                            else
                            {
                                tempMat.CopyTo(patchMat); // BGR -> BGR 直接拷贝即可
                            }
                            shouldDisposePatch = true;
                        }
                    }
                    else
                    {
                        // 使用模板区域
                        patchMat = new Mat(srcMat, new OpenCvSharp.Rect(template.X, template.Y, template.Width, template.Height)).Clone();
                        shouldDisposePatch = true;
                    }

                    try
                    {
                        foreach (var r0 in _matchRects.ToList())
                        {
                            var r = ClampToImage(r0);
                            if (r.IsEmpty) continue;

                            using (var dstRoi = new Mat(srcMat, new OpenCvSharp.Rect(r.X, r.Y, r.Width, r.Height)))
                            {
                                if (r.Width == patchMat.Cols && r.Height == patchMat.Rows)
                                {
                                    patchMat.CopyTo(dstRoi);
                                }
                                else
                                {
                                    Cv2.Resize(patchMat, dstRoi, new OpenCvSharp.Size(r.Width, r.Height), 0, 0, InterpolationFlags.Linear);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (shouldDisposePatch) patchMat?.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("修复失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            finally
            {
                if (bmpData != null) _originalBmp.UnlockBits(bmpData);
            }

            ApplyZoom(_zoom);
            picMain.Invalidate();

            MessageBox.Show("修复已完成。", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClearSingle_Click(object sender, EventArgs e)
        {
            if (_selectedIndex >= 0 && _selectedIndex < _matchRects.Count)
            {
                _matchRects.RemoveAt(_selectedIndex);
                _selectedIndex = -1;
                _selectedRect = DrawingRectangle.Empty;
                picMain.Invalidate();
            }
        }

        private void btnClearAllLocations_Click(object sender, EventArgs e)
        {
            try
            {
                // 清空所有匹配结果
                _matchRects.Clear();
                _selectedIndex = -1;
                _selectedRect = DrawingRectangle.Empty;

                // 保留并重新显示模板框
                _showTemplateRect = true;
                _showSearchRangeRect = false;
                _focusTarget = FocusTarget.Template;

                // 禁用查找和替代图按钮，因为范围框现在隐藏了，需要重新点“2. 自动查找”
                btnStartMatch.Enabled = false;
                btnSelectReplacementImage.Enabled = false;

                // 清空替代图
                if (_replacementBmp != null)
                {
                    _replacementBmp.Dispose();
                    _replacementBmp = null;
                }

                // 刷新预览（回到显示模板框内容）
                UpdateTemplatePreview();
                SyncDimensionFromFocusedRect();

                picMain.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("清空定位失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                _matchRects.Clear();
                _selectedIndex = -1;
                _selectedRect = DrawingRectangle.Empty;
                _focusTarget = FocusTarget.None;
                _showTemplateRect = false;
                _showSearchRangeRect = false;

                btnStartMatch.Enabled = false;
                btnSelectReplacementImage.Enabled = false;

                if (_replacementBmp != null)
                {
                    _replacementBmp.Dispose();
                    _replacementBmp = null;
                }

                if (picTemplatePreview.Image != null)
                {
                    var old = picTemplatePreview.Image;
                    picTemplatePreview.Image = null;
                    if (old != _replacementBmp) try { old.Dispose(); } catch { }
                }

                if (_resetBackupBmp != null)
                {
                    _originalBmp?.Dispose();
                    _originalBmp = (Bitmap)_resetBackupBmp.Clone();
                    
                    numRectW.Maximum = _originalBmp.Width;
                    numRectH.Maximum = _originalBmp.Height;
                    ApplyZoom(_zoom);
                }
                else if (!string.IsNullOrWhiteSpace(_imagePath) && File.Exists(_imagePath))
                {
                    ReloadImagePixelsOnly();
                }
                
                if (!_templateRect.IsEmpty) _templateRect = ClampToImage(_templateRect);
                if (!_searchRange.IsEmpty) _searchRange = ClampToImage(_searchRange);

                picMain.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("重置图片失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadImagePixelsOnly()
        {
            _originalBmp?.Dispose();
            _originalBmp = null;

            using (var fs = new FileStream(_imagePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _originalBmp = new Bitmap(fs);
            }

            // 更新数值框最大限制
            if (_originalBmp != null)
            {
                numRectW.Maximum = _originalBmp.Width;
                numRectH.Maximum = _originalBmp.Height;
            }

            ApplyZoom(_zoom);
            
            // 确保现有的矩形不越界（比如图片被外部改小了）
            if (!_templateRect.IsEmpty) _templateRect = ClampToImage(_templateRect);
            if (!_searchRange.IsEmpty) _searchRange = ClampToImage(_searchRange);
            
            UpdateTemplatePreview();
        }

        private int HitTestRect(DrawingRectangle rectOriginal, DrawingPoint ptDisplay, out DragKind kind)
        {
            kind = DragKind.None;
            if (rectOriginal.IsEmpty) return -1;

            var rDisp = OriginalToDisplay(rectOriginal);
            var handles = GetHandleRects(rDisp);
            if (handles[0].Contains(ptDisplay)) { kind = DragKind.ResizeTL; return 0; }
            if (handles[1].Contains(ptDisplay)) { kind = DragKind.ResizeTR; return 0; }
            if (handles[2].Contains(ptDisplay)) { kind = DragKind.ResizeBL; return 0; }
            if (handles[3].Contains(ptDisplay)) { kind = DragKind.ResizeBR; return 0; }
            if (rDisp.Contains(ptDisplay)) { kind = DragKind.MoveRect; return 0; }
            return -1;
        }

        private void PicMain_MouseDown(object sender, MouseEventArgs e)
        {
            if (_originalBmp == null) return;

            _isMouseDown = true;
            _mouseDownPtDisplay = (DrawingPoint)e.Location;

            // 1. 结果框（补漏框或匹配结果）优先
            int idx = HitTestMatchRect((DrawingPoint)e.Location, out DragKind kind);
            if (idx >= 0)
            {
                _selectedIndex = idx;
                _selectedRect = _matchRects[idx];
                _dragKind = kind;
                _mouseDownRectDisplay = OriginalToDisplay(_selectedRect);
                _focusTarget = FocusTarget.MatchRect;
                _mode = SelectMode.Manual;
                SyncDimensionFromFocusedRect();
                picMain.Invalidate();
                return;
            }

            // 2. 模板框：支持手柄缩放 + 移动
            if (_showTemplateRect && !_templateRect.IsEmpty)
            {
                if (HitTestRect(_templateRect, (DrawingPoint)e.Location, out kind) >= 0)
                {
                    _focusTarget = FocusTarget.Template;
                    _dragKind = kind;
                    _mouseDownRectDisplay = OriginalToDisplay(_templateRect);
                    SyncDimensionFromFocusedRect();
                    picMain.Invalidate();
                    return;
                }
            }

            // 3. 范围框：支持手柄缩放 + 移动
            if (_showSearchRangeRect && !_searchRange.IsEmpty)
            {
                if (HitTestRect(_searchRange, (DrawingPoint)e.Location, out kind) >= 0)
                {
                    _focusTarget = FocusTarget.SearchRange;
                    _dragKind = kind;
                    _mouseDownRectDisplay = OriginalToDisplay(_searchRange);
                    SyncDimensionFromFocusedRect();
                    picMain.Invalidate();
                    return;
                }
            }

            // 4. 点击空白处
            _selectedIndex = -1;
            _selectedRect = DrawingRectangle.Empty;
            _dragKind = DragKind.None;
            SyncDimensionFromFocusedRect();
            picMain.Invalidate();
        }

        private void PicMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (_originalBmp == null) return;

            if (!_isMouseDown)
            {
                // 鼠标悬停光标逻辑：优先结果框手柄，其次模板/范围
                _ = HitTestMatchRect((DrawingPoint)e.Location, out DragKind kind);

                if (kind == DragKind.None)
                {
                    // 模板
                    if (_showTemplateRect && !_templateRect.IsEmpty)
                    {
                        _ = HitTestRect(_templateRect, (DrawingPoint)e.Location, out kind);
                    }
                    // 范围
                    if (kind == DragKind.None && _showSearchRangeRect && !_searchRange.IsEmpty)
                    {
                        _ = HitTestRect(_searchRange, (DrawingPoint)e.Location, out kind);
                    }
                }

                if (kind == DragKind.ResizeTL || kind == DragKind.ResizeBR) picMain.Cursor = Cursors.SizeNWSE;
                else if (kind == DragKind.ResizeTR || kind == DragKind.ResizeBL) picMain.Cursor = Cursors.SizeNESW;
                else if (kind == DragKind.MoveRect) picMain.Cursor = Cursors.SizeAll;
                else picMain.Cursor = Cursors.Default;
                return;
            }

            if (_dragKind != DragKind.None)
            {
                var curDisp = (DrawingPoint)e.Location;
                var dx = curDisp.X - _mouseDownPtDisplay.X;
                var dy = curDisp.Y - _mouseDownPtDisplay.Y;
                var rDisp = _mouseDownRectDisplay;

                DrawingRectangle newDisp = rDisp;
                if (_dragKind == DragKind.MoveRect)
                {
                    newDisp = new DrawingRectangle(rDisp.X + dx, rDisp.Y + dy, rDisp.Width, rDisp.Height);
                }
                else
                {
                    if (_dragKind == DragKind.ResizeTL) newDisp = NormalizeRect(new DrawingPoint(rDisp.Right, rDisp.Bottom), new DrawingPoint(rDisp.X + dx, rDisp.Y + dy));
                    else if (_dragKind == DragKind.ResizeTR) newDisp = NormalizeRect(new DrawingPoint(rDisp.X, rDisp.Bottom), new DrawingPoint(rDisp.Right + dx, rDisp.Y + dy));
                    else if (_dragKind == DragKind.ResizeBL) newDisp = NormalizeRect(new DrawingPoint(rDisp.Right, rDisp.Y), new DrawingPoint(rDisp.X + dx, rDisp.Bottom + dy));
                    else if (_dragKind == DragKind.ResizeBR) newDisp = NormalizeRect(new DrawingPoint(rDisp.X, rDisp.Y), new DrawingPoint(rDisp.Right + dx, rDisp.Bottom + dy));
                }

                var newOri = ClampToImage(DisplayToOriginal(newDisp));
                if (!newOri.IsEmpty)
                {
                    if (_focusTarget == FocusTarget.Template)
                    {
                        _templateRect = newOri;
                        if (_replacementBmp == null)
                        {
                            UpdateTemplatePreview();
                        }
                    }
                    else if (_focusTarget == FocusTarget.SearchRange)
                    {
                        _searchRange = newOri;
                    }
                    else if (_focusTarget == FocusTarget.MatchRect && _selectedIndex >= 0)
                    {
                        _matchRects[_selectedIndex] = newOri;
                        _selectedRect = newOri;
                    }

                    SyncDimensionFromFocusedRect();
                    picMain.Invalidate();
                }
            }
        }

        private void PicMain_MouseUp(object sender, MouseEventArgs e)
        {
            _isMouseDown = false;
            _dragKind = DragKind.None;
        }

        private void PicMain_Paint(object sender, PaintEventArgs e)
        {
            if (_originalBmp == null) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // 搜索范围（焦点：红；非焦点：蓝）
            if (_showSearchRangeRect && !_searchRange.IsEmpty)
            {
                var r = OriginalToDisplay(_searchRange);
                bool focused = (_focusTarget == FocusTarget.SearchRange);
                using (var pen = new Pen(focused ? Color.Red : Color.Blue, focused ? 3 : 2))
                {
                    e.Graphics.DrawRectangle(pen, r);
                }
                if (focused)
                {
                    DrawHandles(e.Graphics, r);
                }
            }

            // 模板（焦点：红；非焦点：蓝）
            if (_showTemplateRect && !_templateRect.IsEmpty)
            {
                var r = OriginalToDisplay(_templateRect);
                bool focused = (_focusTarget == FocusTarget.Template);
                using (var pen = new Pen(focused ? Color.Red : Color.Blue, focused ? 3 : 2))
                {
                    e.Graphics.DrawRectangle(pen, r);
                }
                if (focused)
                {
                    DrawHandles(e.Graphics, r);
                }
            }

            // 匹配结果（未选中：金色；选中：红色）
            for (int i = 0; i < _matchRects.Count; i++)
            {
                var rr = OriginalToDisplay(_matchRects[i]);
                bool selected = (i == _selectedIndex);

                using (var pen = new Pen(selected ? Color.Red : Color.Gold, selected ? 3 : 2))
                {
                    e.Graphics.DrawRectangle(pen, rr);
                }

                if (selected)
                {
                    DrawHandles(e.Graphics, rr);
                }
            }
        }

        private void DrawHandles(Graphics g, DrawingRectangle r)
        {
            foreach (var h in GetHandleRects(r))
            {
                using (var b = new SolidBrush(Color.Cyan))
                {
                    g.FillRectangle(b, h);
                }
                using (var p = new Pen(Color.Black, 1))
                {
                    g.DrawRectangle(p, h);
                }
            }
        }

        private DrawingRectangle[] GetHandleRects(DrawingRectangle r)
        {
            int hs = HandleSize;
            return new[]
            {
                new DrawingRectangle(r.Left - hs / 2, r.Top - hs / 2, hs, hs),
                new DrawingRectangle(r.Right - hs / 2, r.Top - hs / 2, hs, hs),
                new DrawingRectangle(r.Left - hs / 2, r.Bottom - hs / 2, hs, hs),
                new DrawingRectangle(r.Right - hs / 2, r.Bottom - hs / 2, hs, hs),
            };
        }

        private byte[] BitmapToBytes(Bitmap bmp)
        {
            using (Bitmap copy = new Bitmap(bmp))
            using (MemoryStream ms = new MemoryStream())
            {
                copy.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        private int HitTestMatchRect(DrawingPoint ptDisplay, out DragKind kind)
        {
            kind = DragKind.None;

            for (int i = _matchRects.Count - 1; i >= 0; i--)
            {
                var rDisp = OriginalToDisplay(_matchRects[i]);

                // handles
                var handles = GetHandleRects(rDisp);
                if (handles[0].Contains(ptDisplay)) { kind = DragKind.ResizeTL; return i; }
                if (handles[1].Contains(ptDisplay)) { kind = DragKind.ResizeTR; return i; }
                if (handles[2].Contains(ptDisplay)) { kind = DragKind.ResizeBL; return i; }
                if (handles[3].Contains(ptDisplay)) { kind = DragKind.ResizeBR; return i; }

                if (rDisp.Contains(ptDisplay))
                {
                    kind = DragKind.MoveRect;
                    return i;
                }
            }

            return -1;
        }
    }
}
