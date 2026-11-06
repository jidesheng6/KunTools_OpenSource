using System.Windows;
using System.Windows.Media;

namespace DocumentTools.App.Ui;

/// <summary>
/// 壳层与系统页共用的设计标记：4 基准间距、四级字号、中性色。
/// 页面底色固定为白（见 <c>FeaturePageBase</c>），因此中性色按浅色主题取值；
/// 视图里不要再散落硬编码字号/颜色，统一从这里取。
/// </summary>
public static class UiTokens
{
    // 间距：4 的倍数。分组靠留白与分隔线，不靠堆描边。
    public const double SpaceXs = 4;
    public const double SpaceSm = 8;
    public const double SpaceMd = 12;
    public const double SpaceLg = 16;
    public const double SpaceXl = 20;

    // 字号：每页一个大标题，卡片标题 / 正文 / 辅助文字各一级。
    public const double TitleFontSize = 22;
    public const double SectionTitleFontSize = 14;
    public const double SubtitleFontSize = 13;
    public const double BodyFontSize = 13;
    public const double CaptionFontSize = 12;

    // 常用厚度。
    public static readonly Thickness PagePadding = new(SpaceXs);
    public static readonly Thickness CardPadding = new(14);
    public static readonly Thickness CardMargin = new(0, 0, 0, SpaceMd);
    public static readonly Thickness SectionTitleMargin = new(0, 0, 0, SpaceSm);
    public static readonly Thickness RowGap = new(0, 0, 0, SpaceSm);
    public static readonly Thickness ButtonPadding = new(14, 6, 14, 6);
    public static readonly Thickness ButtonGap = new(0, 0, SpaceSm, 0);

    public static readonly CornerRadius CardCornerRadius = new(8);
    public static readonly CornerRadius ListCornerRadius = new(6);

    // 行高：导航的分组头（父级）与功能项（子级）用同一行高，父级不许比子级矮。
    // 34 = 13 号字行高 + 上下各 7 的内边距，也是触控/鼠标点按的最小舒适高度。
    public const double RowMinHeight = 34;
    public static readonly Thickness NavRowPadding = new(10, 7, 10, 7);
    public static readonly Thickness GroupHeaderPadding = new(10, 0, 8, 0);

    // 输入控件：路径框要装得下一条绝对路径并随窗口拉伸；数字框窄且右对齐；长文本多行。
    public const double PathBoxMinWidth = 320;
    public const double NumberBoxMinWidth = 70;
    public const double NumberBoxMaxWidth = 110;
    public const double FieldLabelWidth = 96;
    public const double MultilineMinHeight = 60;


    /// <summary>表单/内容最大宽度：宽窗口下避免一行被拉得过长。</summary>
    public const double ContentMaxWidth = 980;

    public static readonly Brush BorderBrush = Rgb(0xD9, 0xD9, 0xD9);
    public static readonly Brush DividerBrush = Rgb(0xEC, 0xEC, 0xEC);
    public static readonly Brush MutedBrush = Rgb(0x6E, 0x6E, 0x6E);
    public static readonly Brush SurfaceBrush = Brushes.White;
    public static readonly Brush SubtleBrush = Rgb(0xF7, 0xF7, 0xF7);

    private static SolidColorBrush Rgb(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}
