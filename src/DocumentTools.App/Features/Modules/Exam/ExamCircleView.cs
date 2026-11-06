using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Services.Exam;
using DocumentTools.Core.Services.Scan;
using Microsoft.Extensions.DependencyInjection;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace DocumentTools.App.Features.Modules.Exam;

/// <summary>
/// exam.circle — 圆圈修复: frame a template on the ballot sheet, match every occurrence
/// (normalized cross-correlation, port of ExamCircleFixForm.btnStartMatch_Click) and patch them
/// with a replacement image or the template content itself.
/// </summary>
public sealed class ExamCircleView : FeaturePageBase
{
    private const string ImageFilter = "图片|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff;*.webp|所有文件|*.*";

    private readonly IDialogService _dialogs;
    private readonly IExamCircleFixService _fixer;
    private readonly IExamCircleImageService _imageService;
    private readonly IExamCircleOrientationService _orientation;
    private readonly ITwainScanService _scanner;

    private readonly TextBox _path = PathInputs.ForFile("考评表图片路径");
    private readonly TextBox _replacement = PathInputs.ForFile("替换图（可空：用模板内容自身覆盖）");
    private readonly TextBox _threshold = new() { Text = "80" };
    private readonly TextBox _maxMatches = new() { Text = "200" };
    private readonly ComboBox _mode = new();
    private readonly ComboBox _zoom = new();
    private readonly ComboBox _rotate = new();
    private readonly TextBlock _status = ExamViewKit.Status("打开一张考评表图片，然后用鼠标框选模板区域。");
    private readonly Border _legend;
    private readonly Button _runMatch;
    private readonly Button _runFix;

    private readonly Canvas _canvas = new() { Background = Brushes.Transparent };
    // Stretch.Fill + 显式像素尺寸：JPEG 的 DPI 元数据（扫描件常是 300dpi）会让
    // Stretch.None 按“物理尺寸”绘制，导致画面比像素坐标小一圈、框选与匹配对不上。
    private readonly Image _preview = new() { Stretch = Stretch.Fill };
    private readonly ScaleTransform _scale = new(1, 1);
    private readonly TextBlock _emptyHint = new()
    {
        Text = "尚未载入图片。\n点上方「打开…」选一张考评表图片，再在图上框选圆圈模板。",
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        TextAlignment = TextAlignment.Center,
        FontSize = DocumentTools.App.Ui.UiTokens.SubtitleFontSize,
        Foreground = DocumentTools.App.Ui.UiTokens.MutedBrush,
        IsHitTestVisible = false
    };

    private ExamCircleRect? _templateRect;
    private ExamCircleRect? _searchRect;
    private readonly List<ExamCircleMatch> _matches = new();
    private Point? _dragStart;
    private Rectangle? _rubber;
    private int _imageWidth;
    private int _imageHeight;
    private double _zoomValue = 1.0;

    public ExamCircleView(FeatureDescriptor descriptor, IServiceProvider services)
        : base(descriptor.Title, "模板匹配定位所有圆圈后批量修复（对齐原 ExamCircleFixForm 的匹配 / 替换 / 保存）。")
    {
        _dialogs = services.GetRequiredService<IDialogService>();
        _fixer = services.GetRequiredService<IExamCircleFixService>();
        _imageService = services.GetRequiredService<IExamCircleImageService>();
        _orientation = services.GetRequiredService<IExamCircleOrientationService>();
        _scanner = services.GetRequiredService<ITwainScanService>();

        _mode.ItemsSource = new[] { "框选为模板", "框选为搜索范围", "手动添加匹配点" };
        _mode.SelectedIndex = 0;
        _zoom.ItemsSource = new[] { "50%", "75%", "100%", "150%", "200%", "300%" };
        _zoom.SelectedIndex = 2;
        _zoom.SelectionChanged += (_, _) => ApplyZoom();
        _rotate.ItemsSource = new[] { "顺时针 90°", "顺时针 180°", "顺时针 270°（逆时针 90°）" };
        _rotate.SelectedIndex = 0;

        // LayoutTransform (not RenderTransform) so the ScrollViewer extent follows the zoom level.
        _canvas.LayoutTransform = _scale;
        _canvas.MouseLeftButtonDown += OnCanvasMouseDown;
        _canvas.MouseMove += OnCanvasMouseMove;
        _canvas.MouseLeftButtonUp += OnCanvasMouseUp;

        // 空画布在 1200x800 里就是一块灰板，容易被当成「加载失败」，所以叠一层引导文案，载入图片后隐藏。
        // 图例贴在画布左下角：三种线型的含义原来只写在状态行，滚出视口就看不到了。
        _legend = BuildLegend();
        var preview = new Border
        {
            BorderBrush = DocumentTools.App.Ui.UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = DocumentTools.App.Ui.UiTokens.CardCornerRadius,
            Background = new SolidColorBrush(Color.FromRgb(0xF3, 0xF3, 0xF3)),
            MinHeight = 340,
            Child = new Grid
            {
                Children =
                {
                    new ScrollViewer
                    {
                        Content = _canvas,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                    },
                    _emptyHint,
                    _legend
                }
            }
        };

        Border source = ExamViewKit.Card(
            "图片",
            ExamViewKit.Field("原图", _path, ExamViewKit.ActionBar(
                ExamViewKit.Secondary("扫描仪…", ScanImage),
                ExamViewKit.Secondary("打开…", () =>
                {
                    ExamViewKit.PickFile(_dialogs, _path, ImageFilter);
                    LoadImage(ExamViewKit.Value(_path));
                }))),
            ExamViewKit.Field("替换图", _replacement, ExamViewKit.Secondary("选择…", () =>
                ExamViewKit.PickFile(_dialogs, _replacement, ImageFilter))));

        var options = new StackPanel { Margin = new Thickness(0, 0, 0, 6) };
        options.Children.Add(ExamViewKit.Inline(
            Caption("框选目标"),
            Sized(_mode, 170),
            Caption("缩放", 16),
            Sized(_zoom, 100),
            Caption("相似度(%)", 16),
            Sized(_threshold, 70),
            Caption("上限", 16),
            Sized(_maxMatches, 70)));
        options.Children.Add(ExamViewKit.Inline(
            Caption("旋转"),
            Sized(_rotate, 210),
            ExamViewKit.Secondary("向左旋转", () => RotateImageBy(270)),
            ExamViewKit.Secondary("向右旋转", () => RotateImageBy(90)),
            ExamViewKit.Secondary("旋转另存", RotateImage),
            ExamViewKit.Secondary("自动旋转", AutoRotate),
            ExamViewKit.Secondary("清除标记", () =>
            {
                ResetMarks();
                ExamViewKit.ShowStatus(_status, "已清除框选与匹配结果。");
            })));

        // 唯一主操作 = 「开始匹配」（产出匹配结果这一核心动作），保存类动作按工具页惯例走次要按钮。
        _runMatch = ExamViewKit.Primary("开始匹配", RunMatch);
        _runFix = ExamViewKit.Secondary("修复并保存", RunFix);
        var actions = ExamViewKit.ActionBar(_runMatch, _runFix);

        var bottom = new StackPanel();
        bottom.Children.Add(_status);
        bottom.Children.Add(actions);

        SetBody(BuildBottomActionLayout(FillArea(source, preview), options, bottom));
        UpdateActions();
    }

    /// <summary>画布三色线型图例：模板 / 搜索范围 / 匹配，最后一行是「点绿框移除」的可发现入口。</summary>
    private static Border BuildLegend()
    {
        var rows = new StackPanel();
        rows.Children.Add(LegendRow(Brushes.DarkOrange, dashed: false, "模板框（实线）"));
        rows.Children.Add(LegendRow(Brushes.DodgerBlue, dashed: true, "搜索范围（虚线）"));
        rows.Children.Add(LegendRow(Brushes.LimeGreen, dashed: false, "匹配（点框可移除误匹配）"));

        return new Border
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(UiTokens.SpaceSm),
            Padding = new Thickness(UiTokens.SpaceSm),
            CornerRadius = UiTokens.ListCornerRadius,
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromArgb(0xE6, 0xFF, 0xFF, 0xFF)),
            IsHitTestVisible = false,
            Child = rows
        };
    }

    private static FrameworkElement LegendRow(Brush stroke, bool dashed, string text)
    {
        var swatch = new Rectangle
        {
            Width = 18,
            Height = 10,
            Stroke = stroke,
            StrokeThickness = 2,
            VerticalAlignment = VerticalAlignment.Center
        };

        if (dashed)
        {
            swatch.StrokeDashArray = new DoubleCollection { 6, 4 };
        }

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 2)
        };
        row.Children.Add(swatch);
        row.Children.Add(new TextBlock
        {
            Text = text,
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(UiTokens.SpaceSm, 0, 0, 0)
        });
        return row;
    }

    /// <summary>主按钮跟随画布状态：没图/没模板不匹配，没匹配结果不修复。</summary>
    private void UpdateActions()
    {
        if (_runMatch is null)
        {
            return;
        }

        _runMatch.IsEnabled = _preview.Source is not null && _templateRect is not null;
        _runFix.IsEnabled = _matches.Count > 0;
    }

    private static FrameworkElement FillArea(UIElement top, UIElement preview)
    {
        var dock = new DockPanel();
        DockPanel.SetDock(top, Dock.Top);
        dock.Children.Add(top);
        dock.Children.Add(preview);
        return dock;
    }

    private void LoadImage(string path)
    {
        if (path.Length == 0)
        {
            return;
        }

        if (PathGuard.MissingFileReason(path, "考评表图片") is { } problem)
        {
            ExamViewKit.ShowStatus(_status, problem + "：" + path, error: true);
            return;
        }

        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bitmap.UriSource = new Uri(Path.GetFullPath(path));
            bitmap.EndInit();
            bitmap.Freeze();

            _imageWidth = bitmap.PixelWidth;
            _imageHeight = bitmap.PixelHeight;
            _preview.Source = bitmap;
            _preview.Width = _imageWidth;
            _preview.Height = _imageHeight;
            RenderOptions.SetBitmapScalingMode(_preview, BitmapScalingMode.HighQuality);
            _canvas.Width = _imageWidth;
            _canvas.Height = _imageHeight;
            _emptyHint.Visibility = Visibility.Collapsed;
            // 图例只在有图时出现：空画布上三色线型没有对应物，和空态提示叠在一起只会造成噪声。
            _legend.Visibility = Visibility.Visible;

            ResetMarks();
            _path.Text = path;
            (int width, int height) = _fixer.GetImageSize(path);
            ExamViewKit.ShowStatus(_status, $"已载入 {width}×{height}。用鼠标拖拽框选模板区域。");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
        }
    }

    private void ResetMarks()
    {
        _templateRect = null;
        _searchRect = null;
        _matches.Clear();
        _dragStart = null;
        _rubber = null;
        DrawOverlays();
    }

    private void ApplyZoom()
    {
        _zoomValue = _zoom.SelectedIndex switch
        {
            0 => 0.5,
            1 => 0.75,
            3 => 1.5,
            4 => 2.0,
            5 => 3.0,
            _ => 1.0
        };

        _scale.ScaleX = _zoomValue;
        _scale.ScaleY = _zoomValue;
        DrawOverlays();
    }

    private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (_preview.Source is null)
        {
            return;
        }

        _dragStart = Clamp(e.GetPosition(_canvas));
        _rubber = new Rectangle
        {
            Stroke = Brushes.OrangeRed,
            StrokeThickness = 1 / _zoomValue,
            StrokeDashArray = new DoubleCollection { 4, 3 },
            Fill = new SolidColorBrush(Color.FromArgb(32, 255, 80, 0))
        };
        _canvas.Children.Add(_rubber);
        _canvas.CaptureMouse();
        e.Handled = true;
    }

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (_dragStart is null || _rubber is null)
        {
            return;
        }

        Point current = Clamp(e.GetPosition(_canvas));
        double left = Math.Min(_dragStart.Value.X, current.X);
        double top = Math.Min(_dragStart.Value.Y, current.Y);
        Canvas.SetLeft(_rubber, left);
        Canvas.SetTop(_rubber, top);
        _rubber.Width = Math.Abs(current.X - _dragStart.Value.X);
        _rubber.Height = Math.Abs(current.Y - _dragStart.Value.Y);
    }

    private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_dragStart is null || _rubber is null)
        {
            return;
        }

        Point end = Clamp(e.GetPosition(_canvas));
        Point start = _dragStart.Value;
        _dragStart = null;
        _canvas.ReleaseMouseCapture();

        var rect = new ExamCircleRect
        {
            Left = (int)Math.Round(Math.Min(start.X, end.X)),
            Top = (int)Math.Round(Math.Min(start.Y, end.Y)),
            Width = (int)Math.Round(Math.Abs(end.X - start.X)),
            Height = (int)Math.Round(Math.Abs(end.Y - start.Y))
        };

        if (_rubber is not null)
        {
            _canvas.Children.Remove(_rubber);
            _rubber = null;
        }

        if (rect.Width < 4 || rect.Height < 4)
        {
            DrawOverlays();
            return;
        }

        if (_mode.SelectedIndex == 2)
        {
            _matches.Add(new ExamCircleMatch
            {
                Left = rect.Left,
                Top = rect.Top,
                Width = rect.Width,
                Height = rect.Height,
                Similarity = 1.0
            });
            ExamViewKit.ShowStatus(_status, $"已手动添加 1 处匹配，当前共 {_matches.Count} 处。");
        }
        else if (_mode.SelectedIndex == 1)
        {
            _searchRect = rect;
            ExamViewKit.ShowStatus(_status, $"搜索范围：{rect.Width}×{rect.Height} @({rect.Left},{rect.Top})。");
        }
        else
        {
            _templateRect = rect;
            _matches.Clear();
            ExamViewKit.ShowStatus(_status, $"模板：{rect.Width}×{rect.Height} @({rect.Left},{rect.Top})。");
        }

        DrawOverlays();
    }

    private void DrawOverlays()
    {
        _canvas.Children.Clear();
        if (_preview.Source is not null)
        {
            _canvas.Children.Add(_preview);
            Canvas.SetLeft(_preview, 0);
            Canvas.SetTop(_preview, 0);
        }

        if (_searchRect is not null)
        {
            AddRect(_searchRect, Brushes.DodgerBlue, dashed: true);
        }

        foreach (ExamCircleMatch match in _matches.ToArray())
        {
            Rectangle shape = AddRect(
                new ExamCircleRect { Left = match.Left, Top = match.Top, Width = match.Width, Height = match.Height },
                Brushes.LimeGreen,
                dashed: false);
            shape.ToolTip = $"相似度 {match.Similarity:P0}，点击移除该匹配";
            shape.Cursor = Cursors.Hand;
            // 只有匹配框透明填充（可命中测试，才能点掉误匹配）；模板/搜索框保持不可命中，避免挡住拖拽。
            shape.Fill = Brushes.Transparent;
            ExamCircleMatch captured = match;
            shape.MouseLeftButtonDown += (_, args) =>
            {
                _matches.Remove(captured);
                DrawOverlays();
                ExamViewKit.ShowStatus(
                    _status,
                    _matches.Count == 0 ? "已移除全部匹配。" : $"已移除该匹配，剩余 {_matches.Count} 处。");
                args.Handled = true;
            };
        }

        if (_templateRect is not null)
        {
            AddRect(_templateRect, Brushes.DarkOrange, dashed: false);
        }

        UpdateActions();
    }

    private Rectangle AddRect(ExamCircleRect rect, Brush stroke, bool dashed)
    {
        var shape = new Rectangle
        {
            Stroke = stroke,
            StrokeThickness = 2 / _zoomValue,
            Width = rect.Width,
            Height = rect.Height
        };

        if (dashed)
        {
            shape.StrokeDashArray = new DoubleCollection { 6, 4 };
        }

        Canvas.SetLeft(shape, rect.Left);
        Canvas.SetTop(shape, rect.Top);
        _canvas.Children.Add(shape);
        return shape;
    }

    private void RunMatch()
    {
        string path = ExamViewKit.Value(_path);
        if (PathGuard.MissingFileReason(path, "考评表图片") is { } problem)
        {
            _dialogs.Info(problem + "，请先打开考评表图片。");
            return;
        }

        if (_templateRect is null)
        {
            _dialogs.Info("请先框选模板区域（框选目标 = 框选为模板）。");
            return;
        }

        double threshold = ReadThreshold();
        int maxMatches = ReadInt(_maxMatches, 200, 1, 5000);

        try
        {
            _matches.Clear();
            _matches.AddRange(_fixer.Match(path, _templateRect, _searchRect, threshold, maxMatches));
            DrawOverlays();
            ExamViewKit.ShowStatus(
                _status,
                _matches.Count == 0
                    ? $"未找到相似度 ≥ {threshold:P0} 的匹配。可降低相似度或重选框选区域。"
                    : $"匹配到 {_matches.Count} 处（相似度 ≥ {threshold:P0}）。点绿色框可移除误匹配。");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    private void RunFix()
    {
        string path = ExamViewKit.Value(_path);
        if (PathGuard.MissingFileReason(path, "考评表图片") is { } problem)
        {
            _dialogs.Info(problem + "，请先打开考评表图片。");
            return;
        }

        if (_matches.Count == 0)
        {
            _dialogs.Info("请先执行匹配，得到待修复位置。");
            return;
        }

        string? output = _dialogs.SaveFile("图片|*.png;*.jpg;*.jpeg;*.bmp", ExamCircleNaming.FixedFileName(path));
        if (output is null)
        {
            return;
        }

        string replacement = ExamViewKit.Value(_replacement);
        // 替换图是可选的：空 = 用模板内容自身覆盖。只有「填了但文件没了」才算问题。
        if (PathInputs.IsMissingFile(replacement))
        {
            _dialogs.Info(PathInputs.MissingFileNotice(replacement));
            return;
        }

        try
        {
            string result = _fixer.Fix(new ExamCircleFixRequest
            {
                SourceImage = path,
                TemplateRect = _templateRect ?? new ExamCircleRect(),
                SearchRect = _searchRect,
                SimilarityThreshold = ReadThreshold(),
                MaxMatches = ReadInt(_maxMatches, 200, 1, 5000),
                ReplacementImage = replacement.Length == 0 ? null : replacement,
                ManualMatches = _matches,
                OutputFile = output
            });

            ExamViewKit.ShowStatus(_status, $"已修复 {_matches.Count} 处并保存：{result}", StatusTone.Success);
            if (_dialogs.Confirm("是否打开输出目录？"))
            {
                ExamViewKit.RevealFile(result);
            }
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>
    /// 「扫描仪…」：legacy 该页有「扫描仪获取图片」入口（BodyForm.cs:176-179/247-251），
    /// 但取图流程在 dev 自带的 TWAIN 封装里。这里复用 <see cref="ITwainScanService"/>：
    /// 扫到 <c>%LOCALAPPDATA%\DocumentTools\scans\{时间戳}</c>，载入第一张；DPI 交给驱动界面（ShowUi）临时改。
    /// </summary>
    private void ScanImage()
    {
        if (!_scanner.IsAvailable)
        {
            ExamViewKit.ShowStatus(_status, _scanner.UnavailableReason, error: true);
            return;
        }

        IReadOnlyList<TwainSourceInfo> sources = _scanner.ListSources();
        if (sources.Count == 0)
        {
            ExamViewKit.ShowStatus(
                _status,
                "未检测到扫描仪设备。请确认设备已开机并连接，且已安装厂商驱动。",
                error: true);
            return;
        }

        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DocumentTools",
            "scans",
            DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture));
        Directory.CreateDirectory(directory);

        try
        {
            ExamViewKit.ShowStatus(_status, $"正在用「{sources[0].Name}」扫描…", StatusTone.Busy);

            IReadOnlyList<string> files = _scanner.Scan(sources[0].Id, new TwainScanOptions
            {
                OutputDirectory = directory,
                Dpi = 300,
                Duplex = false,
                // 考评表是单张原稿：让驱动界面能临时改分辨率 / 纸张。
                ShowUi = true,
                SkipBlankPages = false
            });

            if (files.Count == 0)
            {
                ExamViewKit.ShowStatus(_status, "扫描结束，但没有扫到图片。", error: true);
                return;
            }

            _path.Text = files[0];
            LoadImage(files[0]);

            string extra = files.Count > 1
                ? $"（共 {files.Count} 张，其余在 {directory}）"
                : string.Empty;
            ExamViewKit.ShowStatus(
                _status,
                $"已从「{sources[0].Name}」扫描并载入 1 张{extra}。",
                StatusTone.Success);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, "扫描失败：" + ex.Message, error: true);
        }
    }

    private void RotateImage()
        => RotateImageBy(_rotate.SelectedIndex switch
        {
            1 => 180,
            2 => 270,
            _ => 90
        });

    /// <summary>
    /// legacy 的「向左 / 向右旋转」（ExamCircleFix_RotateLeft/RotateRight，Designer:16360/16413）：
    /// 单次 90°，方向只决定顺时针还是逆时针，落盘仍走「另存带角度后缀」的同一条路径。
    /// </summary>
    private void RotateImageBy(int degrees)
    {
        string path = ExamViewKit.Value(_path);
        if (PathGuard.MissingFileReason(path, "考评表图片") is { } problem)
        {
            _dialogs.Info(problem + "，请先打开考评表图片。");
            return;
        }

        try
        {
            string output = ExamCircleNaming.UniquePath(path, ExamCircleNaming.RotationSuffix(degrees));
            string rotated = _imageService.Rotate(path, degrees, output);
            LoadImage(rotated);
            ExamViewKit.ShowStatus(_status, $"已旋转 {degrees}° 并另存：{rotated}");
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    /// <summary>legacy「自动旋转」（BodyForm.cs:322-400）：按 T 码所在象限把图片转正并另存。</summary>
    private void AutoRotate()
    {
        string path = ExamViewKit.Value(_path);
        if (PathGuard.MissingFileReason(path, "考评表图片") is { } problem)
        {
            _dialogs.Info(problem + "，请先打开考评表图片。");
            return;
        }

        try
        {
            ExamCircleOrientation orientation = _orientation.Detect(path);
            if (!orientation.FoundTCode)
            {
                ExamViewKit.ShowStatus(_status, orientation.Message, error: true);
                return;
            }

            if (orientation.Rotation == ExamCircleRotation.None)
            {
                ExamViewKit.ShowStatus(_status, orientation.Message, StatusTone.Success);
                return;
            }

            int degrees = (int)orientation.Rotation;
            string output = ExamCircleNaming.UniquePath(path, ExamCircleNaming.AutoRotationSuffix(degrees));
            string rotated = _imageService.Rotate(path, degrees, output);
            LoadImage(rotated);
            ExamViewKit.ShowStatus(_status, orientation.Message + " 已另存：" + rotated, StatusTone.Success);
        }
        catch (Exception ex)
        {
            ExamViewKit.ShowStatus(_status, ex.Message, error: true);
            _dialogs.Error(ex.Message);
        }
    }

    private double ReadThreshold()
    {
        int percent = ReadInt(_threshold, 80, 1, 100);
        return percent / 100.0;
    }

    private int ReadInt(TextBox box, int fallback, int min, int max)
    {
        if (!int.TryParse(ExamViewKit.Value(box), out int value))
        {
            return fallback;
        }

        return Math.Clamp(value, min, max);
    }

    private Point Clamp(Point point)
        => new(
            Math.Clamp(point.X, 0, Math.Max(0, _imageWidth)),
            Math.Clamp(point.Y, 0, Math.Max(0, _imageHeight)));

    private static TextBlock Caption(string text, double leftMargin = 0)
        => new()
        {
            Text = text,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(leftMargin, 0, 8, 0),
            Foreground = DocumentTools.App.Ui.UiTokens.MutedBrush
        };

    private static TextBox Sized(TextBox box, double width)
    {
        box.Width = width;
        return box;
    }

    private static ComboBox Sized(ComboBox box, double width)
    {
        box.Width = width;
        return box;
    }
}
