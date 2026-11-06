using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.Core.Features;

namespace DocumentTools.UiSnapshot;

/// <summary>
/// 逐页创建真视图、按 1200×800 布局、渲染为 PNG，并顺带跑视觉树检查。
/// 视图来自真实 DI 容器（<see cref="DocumentTools.App.Bootstrap.AppHost"/>），不是设计稿或假数据。
/// </summary>
internal static class SnapshotRunner
{
    internal const int PageWidth = 1200;

    internal const int PageHeight = 800;

    private const long MaxPngBytes = 300 * 1024;

    private const int SettleMilliseconds = 150;

    internal static PageSnapshot Capture(
        FeatureDescriptor feature,
        int index,
        IFeatureViewFactory factory,
        string outputDirectory,
        string? fileName = null,
        int dpi = 96,
        Action<FrameworkElement>? interact = null,
        Action<FrameworkElement>? inspect = null)
    {
        string path = Path.Combine(outputDirectory, fileName ?? feature.Id + ".png");
        var snapshot = new PageSnapshot(index, feature, path) { Dpi = dpi };

        FrameworkElement? view;
        try
        {
            view = factory.Create(feature.Id);
        }
        catch (Exception ex)
        {
            snapshot.FatalError = "创建视图失败：" + ex.GetType().Name + ": " + ex.Message;
            return snapshot;
        }

        if (view is null)
        {
            snapshot.FatalError = "创建视图失败：功能未注册视图模块";
            return snapshot;
        }

        // 页面自身背景是白色，但透明区域在 PNG 里会变成全黑，套一层白底保证截图可读。
        var host = new Border
        {
            Background = Brushes.White,
            Width = PageWidth,
            Height = PageHeight,
            Child = view
        };

        // 必须放进真实 Window 再渲染：WPF-UI 的 ControlAppearance（Primary 等）在离屏
        // Border 里解析不出背景画刷（实测 Background 变成 #00FFFFFF，按钮看起来是"消失"），
        // 一旦挂到 Window 上就恢复正常（#FF1D6978）。窗口移到屏幕外，不抢焦点。
        Window? window = null;
        try
        {
            window = new Window
            {
                Content = host,
                Width = PageWidth,
                Height = PageHeight,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = -4000,
                Top = -4000
            };
            window.Show();
        }
        catch (Exception ex)
        {
            snapshot.FatalError = "创建宿主窗口失败：" + ex.GetType().Name + ": " + ex.Message;
            return snapshot;
        }

        try
        {
            try
            {
                host.Measure(new Size(PageWidth, PageHeight));
                host.Arrange(new Rect(0, 0, PageWidth, PageHeight));
                host.UpdateLayout();
                PumpDispatcher(SettleMilliseconds);

                // 交互态：在布局完成后先触发点击，再重新泵一次消息让状态行/占位区更新，然后才截图。
                if (interact is not null)
                {
                    try
                    {
                        interact(view);
                        PumpDispatcher(SettleMilliseconds * 2);
                    }
                    catch (Exception ex)
                    {
                        snapshot.AddIssue(new AuditIssue(
                            Severity.Medium,
                            "交互脚本异常",
                            ex.GetType().Name,
                            ex.Message));
                    }
                }

                host.UpdateLayout();

                // 诊断钩子：布局与交互都完成后，让调用方按需打印真实矩形（只读，不改变布局）。
                inspect?.Invoke(view);
            }
            catch (Exception ex)
            {
                snapshot.FatalError = "布局失败：" + ex.GetType().Name + ": " + ex.Message;
                return snapshot;
            }

            try
            {
                Render(host, snapshot, dpi);
            }
            catch (Exception ex)
            {
                snapshot.FatalError = "渲染失败：" + ex.GetType().Name + ": " + ex.Message;
            }

            try
            {
                VisualAudit.Run(host, PageWidth, PageHeight, snapshot);
            }
            catch (Exception ex)
            {
                snapshot.AddIssue(new AuditIssue(Severity.Low, "审计异常", "-", ex.GetType().Name + ": " + ex.Message));
            }

            return snapshot;
        }
        finally
        {
            try
            {
                window.Close();
            }
            catch (InvalidOperationException)
            {
                // 窗口已关闭，忽略。
            }
        }
    }

    /// <summary>跑一次消息泵，让绑定、模板应用、Dispatcher 排队任务在截图前落定。</summary>
    internal static void PumpDispatcher(int milliseconds)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(milliseconds)
        };

        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };

        timer.Start();
        Dispatcher.PushFrame(frame);
    }

    private static void Render(FrameworkElement host, PageSnapshot snapshot, int dpi)
    {
        // 布局始终是 1200×800 DIP；DPI 只放大栅格化分辨率（120 → 1500×1000，144 → 1800×1200），
        // 用于观察高 DPI 下的文字栅格化与固定像素资源，不改变 WPF 的 DIP 布局。
        double dpiScale = dpi / 96.0;
        int pixelWidth = (int)Math.Round(PageWidth * dpiScale);
        int pixelHeight = (int)Math.Round(PageHeight * dpiScale);

        var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, dpi, dpi, PixelFormats.Pbgra32);
        bitmap.Render(host);
        bitmap.Freeze();

        // 1200×800 的扁平界面通常远小于 300KB；万一超限就按 0.75 / 0.5 降尺寸，保持 3:2。
        double[] scales = [1.0, 0.75, 0.5];
        foreach (double scale in scales)
        {
            BitmapSource frame = Math.Abs(scale - 1.0) < double.Epsilon
                ? bitmap
                : new TransformedBitmap(bitmap, new ScaleTransform(scale, scale));

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(frame));

            using var buffer = new MemoryStream();
            encoder.Save(buffer);

            if (buffer.Length <= MaxPngBytes || scale <= 0.5)
            {
                File.WriteAllBytes(snapshot.ScreenshotPath, buffer.ToArray());
                snapshot.PngBytes = buffer.Length;
                snapshot.PngScale = scale;
                snapshot.ScreenshotFile = Path.GetFileName(snapshot.ScreenshotPath);
                return;
            }
        }

        throw new InvalidOperationException("PNG 编码未产生输出");
    }
}
