using Aspose.Pdf;
using Aspose.Pdf.Text;
using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Office;

/// <summary>PDF 水印：校验入参 → 按页加水印 / 移除 → 保存。</summary>
public sealed class PdfWatermarkService : IPdfWatermarkService
{
    public void Apply(string sourceFile, string targetFile, PdfWatermarkOptions options)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(targetFile, nameof(targetFile));
        ArgumentNullException.ThrowIfNull(options);

        if (!options.HasContent)
        {
            throw new ArgumentException("水印内容为空：文本、图片、文件名三选一。", nameof(options));
        }

        PdfWatermarkOptions normalized = options.Normalize();
        EnsureTargetDirectory(targetFile);

        string text = normalized.UseFileNameAsText
            ? Path.GetFileNameWithoutExtension(sourceFile)
            : normalized.Text;

        using var document = new Document(sourceFile);
        foreach (Page page in document.Pages)
        {
            if (normalized.UsesImage)
            {
                ApplyImage(page, normalized);
            }
            else
            {
                ApplyText(page, normalized, text);
            }
        }

        document.Save(targetFile, SaveFormat.Pdf);
    }

    public void Remove(string sourceFile, string targetFile)
    {
        PathGuard.RequireExistingFile(sourceFile, nameof(sourceFile));
        PathGuard.RequirePath(targetFile, nameof(targetFile));
        EnsureTargetDirectory(targetFile);

        using var document = new Document(sourceFile);
        foreach (Page page in document.Pages)
        {
            var removable = new List<Artifact>();
            foreach (Artifact artifact in page.Artifacts)
            {
                if (artifact.Subtype is Artifact.ArtifactSubtype.Watermark
                    or Artifact.ArtifactSubtype.Background)
                {
                    removable.Add(artifact);
                }
                else if (artifact.Type == Artifact.ArtifactType.Undefined
                    && artifact.Subtype == Artifact.ArtifactSubtype.Undefined)
                {
                    // 平铺水印走 Stamp 路径（见 ApplyText），AddStamp 生成的是没有 Type/Subtype 的匿名 artifact；
                    // 规范里的 artifact 必须带 /Type（Pagination/Layout/Page/Background），匿名的那种只会由
                    // AddStamp 这类调用产生，所以把「无类型 artifact」一并视为水印，去水印才不会漏。
                    removable.Add(artifact);
                }
            }

            foreach (Artifact artifact in removable)
            {
                page.Artifacts.Delete(artifact);
            }
        }

        document.Save(targetFile, SaveFormat.Pdf);
    }

    private static void ApplyImage(Page page, PdfWatermarkOptions options)
    {
        if (!options.Repeat)
        {
            var stamp = new ImageStamp(options.ImageFile)
            {
                Opacity = options.Opacity,
                RotateAngle = (float)options.RotationDegrees,
                Width = options.ImageWidth,
                Height = options.ImageHeight,
                HorizontalAlignment = ToHorizontal(options.Horizontal),
                VerticalAlignment = ToVertical(options.Vertical),
                XIndent = 100,
                YIndent = 100
            };
            page.AddStamp(stamp);
            return;
        }

        int rows = (int)(page.Rect.Height / options.RepeatSpacingY);
        int columns = (int)(page.Rect.Width / options.RepeatSpacingX);

        for (int row = 0; row <= rows; row++)
        {
            for (int column = 0; column <= columns; column++)
            {
                var artifact = new WatermarkArtifact
                {
                    ArtifactHorizontalAlignment = ToHorizontal(options.Horizontal),
                    ArtifactVerticalAlignment = ToVertical(options.Vertical),
                    Rotation = (float)options.RotationDegrees,
                    Opacity = options.Opacity,
                    IsBackground = options.PlaceBehindContent,
                    Position = new Point(column * options.RepeatSpacingX, row * options.RepeatSpacingY)
                };
                artifact.SetImage(options.ImageFile);
                page.Artifacts.Add(artifact);
            }
        }
    }

    private static void ApplyText(Page page, PdfWatermarkOptions options, string text)
    {
        var textState = new TextState
        {
            Font = ResolveFont(options.FontName),
            FontSize = (float)options.FontSize,
            ForegroundColor = Color.FromArgb(options.FontColor.R, options.FontColor.G, options.FontColor.B)
        };

        if (!options.Repeat)
        {
            var artifact = CreateTextArtifact(options, text, textState);
            if (options.CustomPosition)
            {
                artifact.Position = new Point(options.PositionX, options.PositionY);
            }

            page.Artifacts.Add(artifact);
            return;
        }

        int rows = (int)(page.Rect.Height / options.RepeatSpacingY);
        int columns = (int)(page.Rect.Width / options.RepeatSpacingX);

        // 平铺用 TextStamp 逐格落位，而不是每格 new WatermarkArtifact：
        // Artifacts.Add 单次约 9.6ms 且随格数超线性增长（A4/25pt 间距 816 格/页，单页 8s、10 页约 78s），
        // 而复用同一个 TextStamp 只改 XIndent/YIndent 每格约 0.4ms。实测 4 页 3264 个 watermark artifact
        // 的渲染结果与旧实现逐像素一致（PageDiff 0.000%），artifact 计数同为 3264。
        // TextStamp 的旋转支点比 WatermarkArtifact 高 1.1×字号（8/12/24/36/100/200pt 与 SimSun/SimHei/
        // Microsoft YaHei/Arial/Times New Roman 全部实测为 1.1，与字族无关），故 XIndent 需减去
        // 1.1×字号×sin(旋转角) 才能与旧输出重合；忽略它会让整片水印沿 x 平移（25pt 间距下差异 41%）。
        var stamp = new TextStamp(text, textState)
        {
            RotateAngle = (float)options.RotationDegrees,
            Opacity = options.Opacity,
            Background = options.PlaceBehindContent,
            HorizontalAlignment = HorizontalAlignment.None,
            VerticalAlignment = VerticalAlignment.None
        };

        double indentCompensation = 1.1 * options.FontSize * Math.Sin(options.RotationDegrees * Math.PI / 180);

        // AddStamp 生成的 artifact 没有 Watermark/Background 子类型（读完再回写要走 Artifacts.Update，
        // 实测每次约 21ms，比原来的 Artifacts.Add 还慢，会把这个优化的收益全吃掉），
        // 所以这里保持纯 AddStamp，由 Remove 认「匿名 artifact」把它们删掉，闭环照样成立。
        for (int row = 0; row <= rows; row++)
        {
            for (int column = 0; column <= columns; column++)
            {
                stamp.XIndent = (column * options.RepeatSpacingX) - indentCompensation;
                stamp.YIndent = row * options.RepeatSpacingY;
                page.AddStamp(stamp);
            }
        }
    }

    private static WatermarkArtifact CreateTextArtifact(
        PdfWatermarkOptions options,
        string text,
        TextState textState)
    {
        var artifact = new WatermarkArtifact
        {
            ArtifactHorizontalAlignment = ToHorizontal(options.Horizontal),
            ArtifactVerticalAlignment = ToVertical(options.Vertical),
            Rotation = (float)options.RotationDegrees,
            Opacity = options.Opacity,
            IsBackground = options.PlaceBehindContent
        };
        artifact.SetTextAndState(text, textState);
        return artifact;
    }

    /// <summary>
    /// legacy 直接用中文字族名（`Util.cs:598-735` 的 FontName 控件）；
    /// Aspose.Pdf 25.12 的 FontRepository 只认英文族名（实测「宋体」抛 Font 宋体 was not found），
    /// 故先按别名表映射，失败再回退 SimSun，两者都失败才抛并把用户原名带回消息。
    /// </summary>
    private static readonly Dictionary<string, string> FontAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["宋体"] = "SimSun",
        ["新宋体"] = "NSimSun",
        ["黑体"] = "SimHei",
        ["微软雅黑"] = "Microsoft YaHei",
        ["楷体"] = "KaiTi",
        ["仿宋"] = "FangSong"
    };

    private static Font ResolveFont(string name)
    {
        string requested = (name ?? string.Empty).Trim();
        if (requested.Length == 0)
        {
            requested = PdfWatermarkOptions.DefaultFontName;
        }

        string candidate = FontAliases.TryGetValue(requested, out string? alias) ? alias : requested;
        Exception? aliasFailure = null;
        if (!string.Equals(candidate, "SimSun", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                return FontRepository.FindFont(candidate);
            }
            catch (Exception ex)
            {
                aliasFailure = ex;
            }
        }

        try
        {
            return FontRepository.FindFont("SimSun");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"找不到可用字体：{requested}（已依次尝试 {candidate} 与 SimSun）。",
                aliasFailure ?? ex);
        }
    }

    private static HorizontalAlignment ToHorizontal(PdfWatermarkHorizontal value)
        => value switch
        {
            PdfWatermarkHorizontal.Left => HorizontalAlignment.Left,
            PdfWatermarkHorizontal.Center => HorizontalAlignment.Center,
            PdfWatermarkHorizontal.Right => HorizontalAlignment.Right,
            PdfWatermarkHorizontal.Justify => HorizontalAlignment.Justify,
            PdfWatermarkHorizontal.FullJustify => HorizontalAlignment.FullJustify,
            _ => HorizontalAlignment.None
        };

    private static VerticalAlignment ToVertical(PdfWatermarkVertical value)
        => value switch
        {
            PdfWatermarkVertical.Top => VerticalAlignment.Top,
            PdfWatermarkVertical.Center => VerticalAlignment.Center,
            PdfWatermarkVertical.Bottom => VerticalAlignment.Bottom,
            _ => VerticalAlignment.None
        };

    private static void EnsureTargetDirectory(string targetFile)
    {
        string? directory = Path.GetDirectoryName(Path.GetFullPath(targetFile));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
