using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Infrastructure.Licensing;
using DocumentTools.App.Shell;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.Core.Common;
using DocumentTools.Core.Features;
using DocumentTools.Core.Licensing;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui.Controls;

namespace DocumentTools.App.Features.Modules.About;

public sealed class AboutFeatureModule : FeatureModuleBase
{
    public AboutFeatureModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "about"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new AboutView(
            services.GetRequiredService<IDialogService>(),
            services.GetRequiredService<DocumentTools.Core.Services.System.IWindowsSystemToolsService>());
}

/// <summary>
/// 关于页：版本、许可、运行信息各成一组，唯一主操作是打开 Hook 日志。
/// </summary>
public sealed class AboutView : FeaturePageBase
{
    private readonly IDialogService _dialogs;
    private readonly DocumentTools.Core.Services.System.IWindowsSystemToolsService _sys;
    private readonly TextBlock _licenseValue;
    private readonly TextBlock _licensePath;
    private readonly TextBlock _feedback;
    private readonly TextBlock _uacStatus;

    public AboutView(
        IDialogService dialogs,
        DocumentTools.Core.Services.System.IWindowsSystemToolsService sys)
        : base(AppInfo.DisplayName, $"版本 {AppInfo.Version} · {AppInfo.Description}")
    {
        _dialogs = dialogs;
        _sys = sys;

        _licenseValue = new TextBlock { FontSize = UiTokens.BodyFontSize, TextWrapping = TextWrapping.Wrap };
        _licensePath = Caption(string.Empty);
        _feedback = Caption(string.Empty);
        _uacStatus = Caption(string.Empty);

        var content = new StackPanel
        {
            MaxWidth = UiTokens.ContentMaxWidth,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        content.Children.Add(Card(
            "许可",
            InfoRow("状态", _licenseValue),
            _licensePath));
        content.Children.Add(Card(
            "运行信息",
            InfoRow("版本", Value(AppInfo.Version)),
            InfoRow("界面框架", Value($"WPF-UI {UiVersion()} · Fluent")),
            InfoRow("运行环境", Value($"{TargetFramework()} · {Environment.OSVersion.VersionString}")),
            InfoRow("业务核心", Value("DocumentTools.Core · 文档转换与办公辅助")),
            InfoRow("Hook 日志", Value(AsposeBootstrap.LogPath, wrap: true)),
            InfoRow("日志目录", Value(CrashLog.DirectoryPath, wrap: true))));
        // legacy「开发信息」卡里的 DisableAdminBehavior（BodyForm.cs:5657-5675）。
        content.Children.Add(Card(
            "管理员批准模式（UAC）",
            InfoRow("当前状态", _uacStatus),
            MakeButton("切换管理员批准模式", ControlAppearance.Caution, ToggleUac),
            Caption("关闭后安装/改名等操作不再弹 UAC 提示，但系统安全性下降；修改需要管理员身份，且重启电脑后生效。")));
        content.Children.Add(Card(
            "软件说明",
            Bullet("用途：批量把 Word / PDF / Excel / PPT 互相转换，另有水印、合并、拆分、压缩、条码、考评等日常工具，处理全程在本机完成。"),
            Bullet("转换能力：由随程序发布的 Aspose 组件直接转换，本机不需要安装 Office 或 WPS。"),
            Bullet("考评模块：面向试卷整理、成绩上报等教务流程，需要按页面提示配置本机数据路径后才能使用。"),
            Bullet("注意事项：软件免费、禁止售卖盈利；完全离线，不上传任何文件；批量处理前请先备份原文档。")));
        content.Children.Add(Card(
            "引用库",
            LibraryList(),
            Caption("版本取自项目的包引用，实际随程序发布的组件即此表；升级依赖后请同步这里。")));
        content.Children.Add(Card(
            "捐赠",
            Bullet("本软件完全免费，没有任何付费功能；如果它对你有帮助，欢迎自愿赞助作者。"),
            DonateBody(),
            MakeButton("查看捐赠说明", ControlAppearance.Secondary, ShowDonate)));

        var bar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, UiTokens.SpaceSm, 0, 0)
        };
        bar.Children.Add(MakeButton("打开 Hook 日志", ControlAppearance.Primary, OpenHookLog));
        bar.Children.Add(MakeButton("打开日志目录", ControlAppearance.Secondary, OpenLogDirectory));
        bar.Children.Add(MakeButton("刷新许可状态", ControlAppearance.Secondary, () => RefreshLicense()));

        var bottom = new StackPanel { Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0) };
        bottom.Children.Add(_feedback);
        bottom.Children.Add(bar);

        SetBody(BuildBottomActionLayout(
            new ScrollViewer
            {
                Content = content,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            },
            optionsBottom: null,
            actionBar: bottom));

        RefreshLicense(announce: false);
        RefreshUacStatus();
    }

    private void RefreshUacStatus()
    {
        try
        {
            bool enabled = _sys.IsUacEnabled();
            _uacStatus.Text = enabled
                ? "已启用（默认，安装/改动系统设置会弹提示）"
                : "已禁用（不弹 UAC 提示）";
            _uacStatus.Foreground = enabled ? UiTokens.MutedBrush : System.Windows.Media.Brushes.Firebrick;
        }
        catch (Exception ex)
        {
            _uacStatus.Text = "读取失败：" + ex.Message;
        }
    }

    private void ToggleUac()
    {
        bool current;
        try
        {
            current = _sys.IsUacEnabled();
        }
        catch (Exception ex)
        {
            _dialogs.Error("无法读取管理员批准模式：" + ex.Message);
            return;
        }

        bool target = !current;
        string action = target ? "启用" : "禁用";
        if (!_dialogs.Confirm(
                $"确定要{action}管理员批准模式（UAC）吗？\r\n\r\n"
                + "修改后需要重启电脑才生效；禁用后系统安全性会下降。",
                "管理员批准模式"))
        {
            return;
        }

        try
        {
            _sys.SetUacEnabled(target);
            RefreshUacStatus();
            _dialogs.Info($"已{action}管理员批准模式。请重启电脑后设置生效。");
        }
        catch (UnauthorizedAccessException ex)
        {
            _dialogs.Error(ex.Message);
        }
        catch (Exception ex)
        {
            _dialogs.Error("修改管理员批准模式失败：" + ex.Message);
        }
    }

    private void OpenHookLog()
    {
        AsposeBootstrap.DumpHookStatus("about-page");
        if (!File.Exists(AsposeBootstrap.LogPath))
        {
            _feedback.Text = "Hook 日志还没有生成，先运行一次需要转换的功能再回来看。";
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = AsposeBootstrap.LogPath,
                UseShellExecute = true
            });
            _feedback.Text = "已用系统默认程序打开 Hook 日志。";
        }
        catch (Exception ex)
        {
            _dialogs.Error($"打不开日志文件：{ex.Message}");
        }
    }

    /// <summary>打开 logs 目录：崩溃日志与 Hook 日志都在这里，目录不存在就先建出来。</summary>
    private void OpenLogDirectory()
    {
        try
        {
            System.IO.Directory.CreateDirectory(CrashLog.DirectoryPath);
            Process.Start(new ProcessStartInfo
            {
                FileName = CrashLog.DirectoryPath,
                UseShellExecute = true
            });
            _feedback.Text = "已打开日志目录。";
        }
        catch (Exception ex)
        {
            _dialogs.Error($"打不开日志目录：{ex.Message}");
        }
    }

    private void RefreshLicense(bool announce = true)
    {
        AsposeLicenseStatus status;
        try
        {
            status = AsposeLicenseLoader.TryLoad();
        }
        catch (Exception ex)
        {
            _licenseValue.Text = "读取失败";
            _licensePath.Text = ex.Message;
            if (announce)
            {
                _feedback.Text = "许可状态读取失败，详见上方提示。";
            }

            return;
        }

        if (status.IsLicensed)
        {
            _licenseValue.Text = "已加载正式许可";
            _licensePath.Text = string.IsNullOrWhiteSpace(status.LicensePath)
                ? "许可文件路径未知。"
                : $"许可文件：{status.LicensePath}";
        }
        else
        {
            _licenseValue.Text = "评估 / Hook 模式";
            _licensePath.Text = "没有找到 lic 文件（可放在 licenses 目录或用 ASPOSE_LICENSE_PATH 指定），转换结果按评估版规则输出。";
        }

        if (announce)
        {
            _feedback.Text = status.IsLicensed ? "许可状态已刷新：已加载。" : "许可状态已刷新：评估模式。";
        }
    }

    private static string UiVersion()
    {
        Version? version = typeof(Button).Assembly.GetName().Version;
        return version is null ? "4.x" : version.ToString(3);
    }

    /// <summary>运行框架取自运行时描述，不写死版本号。</summary>
    private static string TargetFramework() => RuntimeInformation.FrameworkDescription;

    private static TextBlock Bullet(string text)
        => new()
        {
            Text = $"· {text}",
            FontSize = UiTokens.BodyFontSize,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs + 2)
        };

    /// <summary>
    /// 引用库静态表。唯一数据源：`src\DocumentTools.Core\DocumentTools.Core.csproj` 与
    /// `src\DocumentTools.App\DocumentTools.App.csproj` 的 PackageReference；升包时同步这里。
    /// 以后若想改成自动读取，把这张表换成 deps.json / PackageReference 解析即可，渲染部分不用动。
    /// </summary>
    private static readonly (string Name, string Version)[] Libraries =
    {
        ("Aspose.Words", "26.1.0"),
        ("Aspose.PDF", "25.12.0"),
        ("Aspose.Cells", "25.12.0"),
        ("Aspose.Slides.NET", "25.12.0"),
        ("Aspose.BarCode", "25.3.0"),
        ("Aspose.HTML", "25.12.0"),
        ("SkiaSharp", "3.119.0"),
        ("NTwain", "3.7.5"),
        ("PuppeteerSharp", "20.2.4"),
        ("NPinyin.Core", "3.0.0"),
        ("Microsoft.Extensions.DependencyInjection", "10.0.4")
    };

    private static UIElement LibraryList()
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs) };
        foreach ((string name, string version) in Libraries)
        {
            panel.Children.Add(LibraryRow(name, version));
        }

        panel.Children.Add(LibraryRow("WPF-UI", UiVersion()));
        return panel;
    }

    private static UIElement LibraryRow(string name, string version)
    {
        var row = new DockPanel { LastChildFill = true, Margin = new Thickness(0, 0, 0, UiTokens.SpaceXs) };

        var value = new TextBlock
        {
            Text = version,
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Center
        };
        DockPanel.SetDock(value, Dock.Right);
        row.Children.Add(value);

        row.Children.Add(new TextBlock
        {
            Text = name,
            FontSize = UiTokens.BodyFontSize,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = name
        });

        return row;
    }

    /// <summary>
    /// 捐赠内容：优先用随包发布的赞赏码图片（输出目录 assets\donate\*.png|jpg），
    /// 图片不存在时降级为文字说明，不静默失败。
    /// </summary>
    private static UIElement DonateBody()
    {
        string[] images = FindDonateImages();
        if (images.Length > 0)
        {
            var panel = new WrapPanel { Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0) };
            foreach (string path in images)
            {
                try
                {
                    panel.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri(path)),
                        MaxWidth = 180,
                        Stretch = System.Windows.Media.Stretch.Uniform,
                        Margin = new Thickness(0, 0, UiTokens.SpaceMd, 0)
                    });
                }
                catch (Exception)
                {
                    // 单张图片坏了不影响文字说明入口。
                }
            }

            if (panel.Children.Count > 0)
            {
                return panel;
            }
        }

        return Bullet("本次构建没有附带赞赏码图片，点下面的按钮可以看到文字版说明，入口不会失效。");
    }

    private static string[] FindDonateImages()
    {
        string dir = Path.Combine(AppContext.BaseDirectory, "assets", "donate");
        if (!Directory.Exists(dir))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(dir)
            .Where(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                        || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                        || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void ShowDonate()
    {
        _dialogs.Info(
            "感谢使用 " + AppInfo.DisplayName + "。\n\n"
            + "· 软件免费发布，禁止售卖盈利；赞助完全自愿，不影响任何功能。\n"
            + "· 本次构建没有附带赞赏码图片，因此这里给文字版说明：如需赞助作者，请在原发布帖里通过作者留下的联系方式联系。\n"
            + "· 原发布帖：https://www.52pojie.cn/thread-1909757-1-1.html\n\n"
            + "图片版入口会在赞赏码随包发布后自动替换这里，无需改动代码。",
            "捐赠说明");
    }

    private static TextBlock Value(string text, bool wrap = false)
        => new()
        {
            Text = text,
            FontSize = UiTokens.BodyFontSize,
            TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = wrap ? null : text
        };

    private static TextBlock Caption(string text)
        => new()
        {
            Text = text,
            FontSize = UiTokens.CaptionFontSize,
            Foreground = UiTokens.MutedBrush,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, UiTokens.SpaceXs, 0, 0)
        };

    private static Grid InfoRow(string label, UIElement value)
    {
        var grid = new Grid { Margin = new Thickness(0, 0, 0, UiTokens.SpaceSm) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(92) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var caption = new TextBlock
        {
            Text = label,
            FontSize = UiTokens.BodyFontSize,
            Foreground = UiTokens.MutedBrush,
            VerticalAlignment = VerticalAlignment.Top
        };
        Grid.SetColumn(caption, 0);
        grid.Children.Add(caption);

        Grid.SetColumn(value, 1);
        grid.Children.Add(value);

        return grid;
    }

    private static Border Card(string title, params UIElement[] children)
    {
        var stack = new StackPanel();
        stack.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = UiTokens.SectionTitleFontSize,
            FontWeight = FontWeights.SemiBold,
            Margin = UiTokens.SectionTitleMargin
        });

        foreach (UIElement child in children)
        {
            stack.Children.Add(child);
        }

        return new Border
        {
            BorderBrush = UiTokens.BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = UiTokens.CardCornerRadius,
            Padding = UiTokens.CardPadding,
            Margin = UiTokens.CardMargin,
            Background = UiTokens.SurfaceBrush,
            Child = stack
        };
    }

    private static Button MakeButton(string text, ControlAppearance appearance, Action onClick)
    {
        var button = new Button
        {
            Content = text,
            Appearance = appearance,
            Padding = UiTokens.ButtonPadding,
            Margin = UiTokens.ButtonGap,
            CornerRadius = UiTokens.ListCornerRadius
        };
        button.Click += (_, _) => onClick();
        return button;
    }
}
