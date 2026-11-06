using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DocumentTools.App.Features.Abstractions;
using DocumentTools.App.Infrastructure.Dialogs;
using DocumentTools.App.Ui;
using DocumentTools.App.Ui.Controls;
using DocumentTools.App.Ui.Options;
using DocumentTools.Core.Features;
using DocumentTools.Core.Features.Options;
using DocumentTools.Core.Services.Code;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.App.Features.Modules.Tools;

public sealed class BarcodeFeatureModule : FeatureModuleBase
{
    public BarcodeFeatureModule()
        : base(BuiltInFeatures.All.First(f => f.Id == "code.barcode"))
    {
    }

    public override FrameworkElement CreateView(IServiceProvider services)
        => new BarcodeView(
            services.GetRequiredService<IBarcodeService>(),
            services.GetRequiredService<IDialogService>());
}

/// <summary>
/// Content + LOGO stay as dedicated UI; appearance/caption come from Core Schema via OptionPanelBuilder.
/// Layout: content on top, preview beside it, status + single primary action docked bottom.
/// </summary>
public sealed class BarcodeView : FeaturePageBase
{
    private readonly IBarcodeService _barcodes;
    private readonly IDialogService _dialogs;
    private readonly TextBox _content = new()
    {
        PlaceholderText = "条码内容（多行时按行批量生成可另存）",
        MinHeight = 80,
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap
    };
    // 路径类：只读 + 可聚焦 + 可拖放，值只由「选择…」/「清除」写入（见 PathInputs 政策）。
    private readonly TextBox _logoPath = ToolViewKit.PathBox("LOGO 图片路径", "LOGO 图片路径（可选）");
    private readonly TextBox _readPath = ToolViewKit.PathBox("要识别的条码图片路径", "要识别的条码图片路径");
    private readonly TextBox _readResult = new()
    {
        IsReadOnly = true,
        AcceptsReturn = true,
        TextWrapping = TextWrapping.Wrap,
        MinHeight = 60,
        PlaceholderText = "识别结果会显示在这里，可复制到别处使用"
    };
    private readonly TextBox _templatePath = ToolViewKit.PathBox("条码 Excel 模板", "条码 Excel 模板路径");
    private readonly DataGrid _templateGrid = new()
    {
        AutoGenerateColumns = false,
        IsReadOnly = true,
        CanUserAddRows = false,
        CanUserDeleteRows = false,
        MinHeight = 140,
        MaxHeight = 220,
        HeadersVisibility = DataGridHeadersVisibility.Column,
        GridLinesVisibility = DataGridGridLinesVisibility.Horizontal
    };
    private readonly ComboBox _templateFormat = new() { MinWidth = 160 };
    private readonly StackPanel _optionsHost = new();
    private readonly OptionPanelBuilder _options;
    private readonly System.Windows.Controls.Image _preview = new() { Stretch = Stretch.Uniform, MaxHeight = 260 };
    private readonly Border _previewFrame;
    private readonly TextBlock _status = ToolViewKit.Status("输入内容后点「生成预览」；多行内容可用「批量生成」。");
    private byte[]? _png;
    private readonly List<(string Line, byte[] Png)> _batch = new();
    private IReadOnlyList<BarcodeBatchRow> _templateRows = Array.Empty<BarcodeBatchRow>();

    public BarcodeView(IBarcodeService barcodes, IDialogService dialogs)
        : base("简易二维码", "生成、预览、保存或批量输出条码，并可从 Excel 模板导入任务。")
    {
        _barcodes = barcodes;
        _dialogs = dialogs;

        // 读屏软件：这几个控件都没有可见标签（占位提示不是标签，输入后即消失），必须补可访问名称。
        AutomationProperties.SetName(_content, "条码内容");
        AutomationProperties.SetName(_readResult, "识别结果（只读）");
        AutomationProperties.SetName(_preview, "条码预览图片");
        AutomationProperties.SetName(_templateGrid, "Excel 模板条码列表");
        AutomationProperties.SetName(_templateFormat, "模板批量输出格式");

        _templateGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "编号 / 文件名",
            Binding = new System.Windows.Data.Binding(nameof(BarcodeBatchRow.FileName))
        });
        _templateGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "标题",
            Binding = new System.Windows.Data.Binding(nameof(BarcodeBatchRow.Caption))
        });
        _templateGrid.Columns.Add(new DataGridTextColumn
        {
            Header = "条码内容",
            Binding = new System.Windows.Data.Binding(nameof(BarcodeBatchRow.Content))
        });
        _templateFormat.Items.Add("PNG（默认）");
        _templateFormat.Items.Add("BMP（兼容旧版）");
        _templateFormat.SelectedIndex = 0;

        _options = new OptionPanelBuilder(dialogs);
        _options.BuildInto(_optionsHost, FeatureOptionSchemas.GetSections("code.barcode"));

        Border contentCard = ToolViewKit.Card("条码内容设置", _content);

        Border logoCard = ToolViewKit.Card(
            "LOGO 设置",
            ToolViewKit.Field("LOGO 图片", _logoPath, ToolViewKit.ActionBar(
                ToolViewKit.Secondary("选择…", () =>
                {
                    var files = dialogs.OpenFiles("图片|*.png;*.jpg;*.jpeg;*.bmp|所有文件|*.*", multiSelect: false);
                    if (files.Count > 0)
                    {
                        _logoPath.Text = files[0];
                    }
                }),
                ToolViewKit.Secondary("清除", () => _logoPath.Text = string.Empty))));

        Border templateCard = ToolViewKit.Card(
            "Excel 模板批量",
            ToolViewKit.Field("模板文件", _templatePath, ToolViewKit.ActionBar(
                ToolViewKit.Secondary("选择并导入模板…", ImportBatchTemplate),
                ToolViewKit.Secondary("导出空模板…", ExportBatchTemplate))),
            ToolViewKit.Field("输出格式", _templateFormat, null, 76, stretchContent: false),
            _templateGrid,
            ToolViewKit.Hint("模板列：编号（输出文件名）/ 标题（显示在条码下方）/ 内容（条码编码内容）。默认输出 PNG；需要沿用旧模板时可选择 BMP。"),
            ToolViewKit.ActionBar(ToolViewKit.Secondary("按模板批量输出…", GenerateTemplateBatch)));

        _previewFrame = ToolViewKit.Placeholder("生成后在此预览条码：单行显示 1 个，批量显示第 1 个。", 240);
        Border previewCard = ToolViewKit.Card("条码预览", _previewFrame);

        Border readCard = ToolViewKit.Card(
            "条码识别",
            ToolViewKit.Field("图片文件", _readPath, ToolViewKit.ActionBar(
                ToolViewKit.Secondary("选择图片…", PickReadImage),
                ToolViewKit.Secondary("识别条码", RecognizeBarcode),
                ToolViewKit.Secondary("复制识别内容", CopyReadResult))),
            _readResult);

        ScrollViewer left = ToolViewKit.FormArea(contentCard, _optionsHost, logoCard, templateCard, readCard);
        Grid.SetColumn(previewCard, 1);
        previewCard.Margin = new Thickness(12, 0, 0, 0);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.Children.Add(left);
        grid.Children.Add(previewCard);

        var generate = ToolViewKit.Primary("生成预览", GeneratePreview);
        var actions = ToolViewKit.ActionBar(
            generate,
            ToolViewKit.Secondary("批量生成", GenerateBatch),
            ToolViewKit.Secondary("保存 PNG", SaveSingle),
            ToolViewKit.Secondary("批量保存", SaveBatch),
            ToolViewKit.Secondary("复制图片", CopyImage));

        SetBody(BuildToolLayout(grid, _status, actions));

        // 键盘：焦点不在输入框里时 Enter 生成预览；Esc 把状态行收回初始提示（不动已输入的内容）。
        ToolViewKit.AttachPageKeyboard(
            this,
            generate,
            () => ToolViewKit.ShowStatus(_status, "输入内容后点「生成预览」；多行内容可用「批量生成」。"));
    }

    private void GeneratePreview()
    {
        string text = FirstNonEmptyLine(_content.Text);
        if (text.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "请先输入条码内容，再点「生成预览」。", StatusTone.Error);
            return;
        }

        try
        {
            _png = _barcodes.CreatePng(text, BuildRenderOptions());
            _batch.Clear();
            ShowPreview(_png);
            ToolViewKit.ShowStatus(_status, $"已生成预览：{text}", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "生成失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void GenerateBatch()
    {
        var lines = SplitLines(_content.Text);
        if (lines.Count == 0)
        {
            ToolViewKit.ShowStatus(_status, "请输入至少一行条码内容，再点「批量生成」。", StatusTone.Error);
            return;
        }

        try
        {
            var options = BuildRenderOptions();
            _batch.Clear();
            foreach (string line in lines)
            {
                _batch.Add((line, _barcodes.CreatePng(line, options)));
            }

            _png = _batch[0].Png;
            ShowPreview(_png);
            ToolViewKit.ShowStatus(
                _status,
                $"已批量生成 {_batch.Count} 个条码，预览显示第 1 个；点「批量保存」按行写 PNG。",
                StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "批量生成失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void SaveSingle()
    {
        if (_png is null || _png.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "还没有可保存的条码：请先「生成预览」。", StatusTone.Error);
            return;
        }

        string? path = _dialogs.SaveFile("PNG 图片|*.png", "barcode.png");
        if (string.IsNullOrWhiteSpace(path))
        {
            ToolViewKit.ShowStatus(_status, "已取消保存。");
            return;
        }

        try
        {
            File.WriteAllBytes(path, _png);
            ToolViewKit.ShowStatus(_status, "已保存：" + path, StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "保存失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void SaveBatch()
    {
        if (_batch.Count == 0)
        {
            ToolViewKit.ShowStatus(_status, "还没有批量结果：请先「批量生成」。", StatusTone.Error);
            return;
        }

        string? dir = _dialogs.PickFolder();
        if (string.IsNullOrWhiteSpace(dir))
        {
            ToolViewKit.ShowStatus(_status, "已取消保存。");
            return;
        }

        try
        {
            int index = 0;
            foreach (var (line, png) in _batch)
            {
                index++;
                string safe = SanitizeFileName(line);
                if (safe.Length == 0)
                {
                    safe = "barcode_" + index;
                }

                File.WriteAllBytes(Path.Combine(dir, safe + ".png"), png);
            }

            ToolViewKit.ShowStatus(_status, $"已保存 {_batch.Count} 个 PNG 到：{dir}", StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "批量保存失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void ExportBatchTemplate()
    {
        string? path = _dialogs.SaveFile("Excel 模板|*.xlsx", "条码批量模板.xlsx");
        if (string.IsNullOrWhiteSpace(path))
        {
            ToolViewKit.ShowStatus(_status, "已取消导出模板。");
            return;
        }

        try
        {
            _barcodes.ExportBatchTemplate(path);
            ToolViewKit.ShowStatus(_status, "已导出空模板：" + path, StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "导出模板失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void ImportBatchTemplate()
    {
        IReadOnlyList<string> files = _dialogs.OpenFiles("Excel 模板|*.xls;*.xlsx", multiSelect: false);
        if (files.Count == 0)
        {
            ToolViewKit.ShowStatus(_status, "已取消导入模板。");
            return;
        }

        try
        {
            // 选择动作先写入路径框；即使模板内容有误，用户也能看见刚才校验的是哪个文件。
            _templatePath.Text = files[0];
            _templateRows = _barcodes.ReadBatchTemplate(files[0]);
            _templateGrid.ItemsSource = _templateRows;
            ToolViewKit.ShowStatus(
                _status,
                _templateRows.Count == 0
                    ? "模板已载入，但还没有数据行。请在 Excel 中填写后重新导入。"
                    : $"已载入 {_templateRows.Count} 行模板数据。",
                _templateRows.Count == 0 ? StatusTone.Neutral : StatusTone.Success);
        }
        catch (Exception ex)
        {
            _templateRows = Array.Empty<BarcodeBatchRow>();
            _templateGrid.ItemsSource = null;
            ToolViewKit.ShowStatus(_status, "导入模板失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void GenerateTemplateBatch()
    {
        if (_templateRows.Count == 0)
        {
            ToolViewKit.ShowStatus(_status, "请先导入含数据行的 Excel 模板。", StatusTone.Error);
            return;
        }

        string? directory = _dialogs.PickFolder();
        if (string.IsNullOrWhiteSpace(directory))
        {
            ToolViewKit.ShowStatus(_status, "已取消模板批量输出。");
            return;
        }

        try
        {
            BarcodeBatchImageFormat format = _templateFormat.SelectedIndex == 1
                ? BarcodeBatchImageFormat.Bmp
                : BarcodeBatchImageFormat.Png;
            BarcodeBatchResult result = _barcodes.GenerateBatch(
                _templateRows,
                directory,
                BuildRenderOptions(),
                format);
            ToolViewKit.ShowStatus(
                _status,
                $"模板批量输出完成：{result.Count} 个 {format.ToString().ToUpperInvariant()} 文件，目录：{directory}",
                StatusTone.Success);
        }
        catch (Exception ex)
        {
            ToolViewKit.ShowStatus(_status, "模板批量输出失败：" + ex.Message, StatusTone.Error);
        }
    }

    private void CopyImage()
    {
        if (_preview.Source is BitmapSource bmp)
        {
            Clipboard.SetImage(bmp);
            ToolViewKit.ShowStatus(_status, "已复制到剪贴板。", StatusTone.Success);
            return;
        }

        ToolViewKit.ShowStatus(_status, "还没有可复制的条码：请先「生成预览」。", StatusTone.Error);
    }

    private void PickReadImage()
    {
        var files = _dialogs.OpenFiles("图片|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|所有文件|*.*", multiSelect: false);
        if (files.Count > 0)
        {
            _readPath.Text = files[0];
            _readResult.Text = string.Empty;
            _readResult.ToolTip = null;
            ToolViewKit.ShowStatus(_status, "已选择识别图片：" + Path.GetFileName(files[0]), StatusTone.Neutral);
        }
    }

    private void RecognizeBarcode()
    {
        string path = _readPath.Text.Trim();
        if (path.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "请先选择要识别的条码图片。", StatusTone.Error);
            return;
        }

        if (!File.Exists(path))
        {
            // 与 PathInputs 同一句式：说清是哪一条、下一步怎么做，不甩底层异常。
            ToolViewKit.ShowStatus(_status, PathInputs.MissingFileNotice(path), StatusTone.Error);
            return;
        }

        try
        {
            IReadOnlyList<string> texts = _barcodes.ReadTexts(path);
            _readResult.ToolTip = null;
            if (texts.Count == 0)
            {
                _readResult.Text = string.Empty;
                ToolViewKit.ShowStatus(
                    _status,
                    "未识别到条码内容：请确认条码在图中清晰、未被裁切，必要时换更大分辨率重试。",
                    StatusTone.Neutral);
                return;
            }

            _readResult.Text = string.Join(Environment.NewLine, texts);
            ToolViewKit.ShowStatus(_status, $"识别成功：{texts.Count} 处条码内容。", StatusTone.Success);
        }
        catch (Exception ex)
        {
            _readResult.Text = string.Empty;

            // 底层（Aspose.BarCode）的异常是英文的 "Image loading failed." 之类，用户既看不懂也拿不到下一步；
            // 主文案只说「做了什么 / 为什么 / 怎么修」，英文原文降级到 ToolTip 与调试输出。
            ToolViewKit.ShowStatus(
                _status,
                "识别条码失败：这张图读不出来（可能不是图片、扩展名与内容不符、或文件正被其他程序占用）。"
                + "请换一张 PNG/JPG 后重试，或先用画图打开确认能正常显示。",
                StatusTone.Error);
            _readResult.ToolTip = "技术详情：" + ex.Message;
            System.Diagnostics.Debug.WriteLine($"[barcode] 识别失败 {path}: {ex}");
        }
    }

    private void CopyReadResult()
    {
        string text = _readResult.Text.Trim();
        if (text.Length == 0)
        {
            ToolViewKit.ShowStatus(_status, "还没有识别结果：请先「识别条码」。", StatusTone.Error);
            return;
        }

        Clipboard.SetText(text);
        ToolViewKit.ShowStatus(_status, "已复制识别内容到剪贴板。", StatusTone.Success);
    }

    private BarcodeRenderOptions BuildRenderOptions()
    {
        var collected = _options.Collect();
        collected.TryGetValue("symbology", out string? symbology);
        collected.TryGetValue("moduleSize", out string? moduleText);
        collected.TryGetValue("dpi", out string? dpiText);
        collected.TryGetValue("caption", out string? caption);
        collected.TryGetValue("captionPosition", out string? captionPos);
        collected.TryGetValue("captionFontSize", out string? fontText);
        collected.TryGetValue("sizeMode", out string? sizeMode);
        collected.TryGetValue("barcodeWidth", out string? widthText);
        collected.TryGetValue("barcodeHeight", out string? heightText);
        collected.TryGetValue("rotate", out string? rotateText);

        // 「固定尺寸」之外一律交回 Core 自适应（auto）；固定模式下某条边为 0 表示按另一条边等比缩放。
        bool fixedSize = string.Equals(sizeMode?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase);

        return new BarcodeRenderOptions
        {
            Symbology = string.IsNullOrWhiteSpace(symbology) ? "QR" : symbology,
            ModuleSize = int.TryParse(moduleText, out int module) ? module : 4,
            ResolutionDpi = int.TryParse(dpiText, out int dpi) ? dpi : 96,
            Caption = caption,
            CaptionPosition = string.IsNullOrWhiteSpace(captionPos) ? "none" : captionPos,
            CaptionFontSize = float.TryParse(fontText, out float font) ? font : 12f,
            LogoFile = string.IsNullOrWhiteSpace(_logoPath.Text) ? null : _logoPath.Text.Trim(),
            SizeMode = fixedSize ? "fixed" : "auto",
            BarcodeWidth = fixedSize && int.TryParse(widthText, out int width) && width > 0 ? width : 0,
            BarcodeHeight = fixedSize && int.TryParse(heightText, out int height) && height > 0 ? height : 0,
            Rotate = int.TryParse(rotateText, out int rotate) ? rotate : 0
        };
    }

    private void ShowPreview(byte[] png)
    {
        using var ms = new MemoryStream(png);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = ms;
        image.EndInit();
        image.Freeze();
        _preview.Source = image;
        ToolViewKit.ShowPlaceholder(_previewFrame, _preview, string.Empty);
    }

    private static string FirstNonEmptyLine(string? text)
        => SplitLines(text).FirstOrDefault() ?? string.Empty;

    private static List<string> SplitLines(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return text
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => l.Length > 0)
            .ToList();
    }

    private static string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }

        return name.Length > 40 ? name[..40] : name;
    }
}
