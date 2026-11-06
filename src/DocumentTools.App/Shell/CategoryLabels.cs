using DocumentTools.Core.Features;

namespace DocumentTools.App.Shell;

/// <summary>
/// 分类 → 中文显示名。侧边栏分组、页头面包屑、首页分区共用一处，避免各视图各自硬编码。
/// </summary>
public static class CategoryLabels
{
    public static string For(FeatureCategory category)
        => category switch
        {
            FeatureCategory.Home => "首页",
            FeatureCategory.Word => "WORD",
            FeatureCategory.Pdf => "PDF",
            FeatureCategory.Excel => "EXCEL",
            FeatureCategory.Ppt => "PPT",
            FeatureCategory.Image => "图片",
            FeatureCategory.Epub => "EPUB",
            FeatureCategory.Tools => "工具",
            FeatureCategory.Exam => "考评",
            FeatureCategory.System => "系统",
            FeatureCategory.About => "关于",
            _ => "其他"
        };
}
