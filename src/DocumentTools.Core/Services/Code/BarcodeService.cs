using Aspose.BarCode.Generation;
using Aspose.BarCode.BarCodeRecognition;
using Aspose.Cells;
using SkiaSharp;

namespace DocumentTools.Core.Services.Code;

public sealed class BarcodeService : IBarcodeService
{
    private const string BatchSheetName = "条码批量模板";
    private static readonly string[] BatchHeaders = ["编号", "标题", "内容"];

    public byte[] CreateQrCodePng(string content, int moduleSize = 4)
        => CreatePng(content, new BarcodeRenderOptions { Symbology = "QR", ModuleSize = moduleSize });

    public byte[] CreatePng(
        string content,
        string symbology,
        int moduleSize = 4,
        string? logoFile = null)
        => CreatePng(content, new BarcodeRenderOptions
        {
            Symbology = symbology,
            ModuleSize = moduleSize,
            LogoFile = logoFile
        });

    public byte[] CreatePng(string content, BarcodeRenderOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("内容不能为空。", nameof(content));
        }

        options ??= new BarcodeRenderOptions();
        BaseEncodeType type = MapSymbology(options.Symbology);
        using var generator = new BarcodeGenerator(type, content.Trim());
        generator.Parameters.Barcode.XDimension.Pixels = Math.Max(1, options.ModuleSize);

        int dpi = Math.Clamp(options.ResolutionDpi, 72, 600);
        generator.Parameters.Resolution = dpi;

        if (type == EncodeTypes.QR)
        {
            generator.Parameters.Barcode.QR.QrEncodeMode = QREncodeMode.ECI;
            generator.Parameters.Barcode.QR.QrECIEncoding = ECIEncodings.UTF8;
        }

        ApplyCaption(generator, options);

        using var ms = new MemoryStream();
        generator.Save(ms, BarCodeImageFormat.Png);
        byte[] png = ms.ToArray();

        if (!string.IsNullOrWhiteSpace(options.LogoFile) && File.Exists(options.LogoFile))
        {
            png = OverlayLogo(png, options.LogoFile);
        }

        return ApplyAppearance(png, options);
    }

    public IReadOnlyList<string> ReadTexts(string imageFile)
    {
        if (string.IsNullOrWhiteSpace(imageFile))
        {
            throw new ArgumentException("图片路径不能为空。", nameof(imageFile));
        }

        if (!File.Exists(imageFile))
        {
            throw new FileNotFoundException("图片文件不存在：" + imageFile, imageFile);
        }

        // MicrE13B is a check-printing font barcode; legacy excluded it because it floods false positives.
        using FileStream stream = File.OpenRead(imageFile);
        using var reader = new BarCodeReader(stream, DecodeType.AllSupportedTypes.Exclude(DecodeType.MicrE13B));

        var texts = new List<string>();
        foreach (BarCodeResult result in reader.ReadBarCodes())
        {
            if (!string.IsNullOrEmpty(result.CodeText))
            {
                texts.Add(result.CodeText);
            }
        }

        return texts;
    }

    public void ExportBatchTemplate(string outputFile, IReadOnlyList<BarcodeBatchRow>? rows = null)
    {
        if (string.IsNullOrWhiteSpace(outputFile))
        {
            throw new ArgumentException("模板保存路径不能为空。", nameof(outputFile));
        }

        if (!string.Equals(Path.GetExtension(outputFile), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("批量模板只能导出为 .xlsx 文件。", nameof(outputFile));
        }

        string? directory = Path.GetDirectoryName(Path.GetFullPath(outputFile));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var workbook = new Workbook();
        Worksheet sheet = workbook.Worksheets[0];
        sheet.Name = BatchSheetName;
        for (int column = 0; column < BatchHeaders.Length; column++)
        {
            sheet.Cells[0, column].PutValue(BatchHeaders[column]);
        }

        int rowIndex = 1;
        foreach (BarcodeBatchRow row in rows ?? Array.Empty<BarcodeBatchRow>())
        {
            sheet.Cells[rowIndex, 0].PutValue(row.FileName);
            sheet.Cells[rowIndex, 1].PutValue(row.Caption);
            sheet.Cells[rowIndex, 2].PutValue(row.Content);
            rowIndex++;
        }

        sheet.AutoFitColumns();
        workbook.Save(outputFile, Aspose.Cells.SaveFormat.Xlsx);
    }

    public IReadOnlyList<BarcodeBatchRow> ReadBatchTemplate(string templateFile)
    {
        if (string.IsNullOrWhiteSpace(templateFile))
        {
            throw new ArgumentException("模板文件路径不能为空。", nameof(templateFile));
        }

        if (!File.Exists(templateFile))
        {
            throw new FileNotFoundException("条码批量模板不存在：" + templateFile, templateFile);
        }

        string extension = Path.GetExtension(templateFile);
        if (!extension.Equals(".xls", StringComparison.OrdinalIgnoreCase)
            && !extension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("条码批量模板必须是 .xls 或 .xlsx 文件。", nameof(templateFile));
        }

        using var workbook = new Workbook(templateFile);
        Worksheet? sheet = workbook.Worksheets.Cast<Worksheet>().FirstOrDefault(HasBatchHeaders);
        if (sheet is null)
        {
            throw new InvalidDataException("模板格式不正确：第一行必须包含“编号、标题、内容”三列。");
        }

        var rows = new List<BarcodeBatchRow>();
        for (int rowIndex = 1; rowIndex <= sheet.Cells.MaxDataRow; rowIndex++)
        {
            string fileName = sheet.Cells.CheckCell(rowIndex, 0)?.StringValue.Trim() ?? string.Empty;
            string caption = sheet.Cells.CheckCell(rowIndex, 1)?.StringValue.Trim() ?? string.Empty;
            string content = sheet.Cells.CheckCell(rowIndex, 2)?.StringValue.Trim() ?? string.Empty;
            if (fileName.Length == 0 && caption.Length == 0 && content.Length == 0)
            {
                continue;
            }

            if (fileName.Length == 0 || content.Length == 0)
            {
                throw new InvalidDataException($"模板第 {rowIndex + 1} 行不完整：“编号”和“内容”不能为空。");
            }

            rows.Add(new BarcodeBatchRow(fileName, caption, content));
        }

        return rows;
    }

    public BarcodeBatchResult GenerateBatch(
        IReadOnlyList<BarcodeBatchRow> rows,
        string outputDirectory,
        BarcodeRenderOptions options,
        BarcodeBatchImageFormat format = BarcodeBatchImageFormat.Png)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(options);
        if (rows.Count == 0)
        {
            throw new ArgumentException("模板中没有可生成的条码数据。", nameof(rows));
        }

        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("输出目录不能为空。", nameof(outputDirectory));
        }

        Directory.CreateDirectory(outputDirectory);
        string extension = format == BarcodeBatchImageFormat.Bmp ? ".bmp" : ".png";
        var outputs = new List<string>(rows.Count);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (BarcodeBatchRow row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.Content))
            {
                throw new InvalidDataException($"模板中编号“{row.FileName}”的“内容”为空，批量生成已取消。");
            }

            string fileName = SanitizeBatchFileName(row.FileName);
            if (!names.Add(fileName))
            {
                throw new InvalidDataException($"模板中存在重复编号“{fileName}”，为避免覆盖文件，批量生成已取消。");
            }

            string outputFile = Path.Combine(outputDirectory, fileName + extension);
            if (File.Exists(outputFile))
            {
                throw new IOException("输出文件已存在，为避免覆盖，批量生成已取消：" + outputFile);
            }

            outputs.Add(outputFile);
        }

        var createdFiles = new List<string>(rows.Count);
        try
        {
            for (int index = 0; index < rows.Count; index++)
            {
                BarcodeBatchRow row = rows[index];
                byte[] png = CreatePng(row.Content, WithCaption(options, row.Caption));
                byte[] image = format == BarcodeBatchImageFormat.Bmp ? ConvertPngToBmp(png) : png;
                createdFiles.Add(outputs[index]);
                File.WriteAllBytes(outputs[index], image);
            }
        }
        catch
        {
            // A batch is one user operation. Do not leave a misleading half-finished directory
            // when a later row cannot render or the disk write fails.
            foreach (string file in createdFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch
                {
                    // Preserve the original rendering/write error.
                }
            }

            throw;
        }

        return new BarcodeBatchResult(outputs);
    }

    private static bool HasBatchHeaders(Worksheet sheet)
        => BatchHeaders.Select((header, column) =>
                string.Equals(sheet.Cells.CheckCell(0, column)?.StringValue.Trim(), header, StringComparison.OrdinalIgnoreCase))
            .All(static matches => matches);

    private static BarcodeRenderOptions WithCaption(BarcodeRenderOptions source, string caption)
        => new()
        {
            Symbology = source.Symbology,
            ModuleSize = source.ModuleSize,
            ResolutionDpi = source.ResolutionDpi,
            Caption = caption,
            CaptionPosition = string.IsNullOrWhiteSpace(caption)
                ? "none"
                : source.CaptionPosition is "none" or "0" or "隐藏" ? "below" : source.CaptionPosition,
            CaptionFontSize = source.CaptionFontSize,
            LogoFile = source.LogoFile,
            Rotate = source.Rotate,
            SizeMode = source.SizeMode,
            BarcodeWidth = source.BarcodeWidth,
            BarcodeHeight = source.BarcodeHeight
        };

    private static string SanitizeBatchFileName(string fileName)
    {
        string value = Path.GetFileNameWithoutExtension(fileName.Trim());
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        value = value.Trim().TrimEnd('.');
        if (value.Length == 0)
        {
            throw new InvalidDataException("模板中的“编号”不能生成有效文件名。");
        }

        return value;
    }

    /// <summary>SkiaSharp has no BMP encoder; emit a standard 32-bit bottom-up BMP.</summary>
    private static byte[] ConvertPngToBmp(byte[] png)
    {
        using SKBitmap bitmap = SKBitmap.Decode(png)
            ?? throw new InvalidOperationException("无法把条码图像转换为 BMP。");
        int stride = checked(bitmap.Width * 4);
        int pixelBytes = checked(stride * bitmap.Height);
        const int headerBytes = 14 + 40;
        using var stream = new MemoryStream(headerBytes + pixelBytes);
        using var writer = new BinaryWriter(stream);
        writer.Write((byte)'B');
        writer.Write((byte)'M');
        writer.Write(headerBytes + pixelBytes);
        writer.Write(0);
        writer.Write(headerBytes);
        writer.Write(40);
        writer.Write(bitmap.Width);
        writer.Write(bitmap.Height);
        writer.Write((short)1);
        writer.Write((short)32);
        writer.Write(0);
        writer.Write(pixelBytes);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);

        for (int y = bitmap.Height - 1; y >= 0; y--)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                SKColor color = bitmap.GetPixel(x, y);
                writer.Write(color.Blue);
                writer.Write(color.Green);
                writer.Write(color.Red);
                writer.Write(color.Alpha);
            }
        }

        return stream.ToArray();
    }

    private static void ApplyCaption(BarcodeGenerator generator, BarcodeRenderOptions options)
    {
        string caption = options.Caption?.Trim() ?? string.Empty;
        string position = (options.CaptionPosition ?? "none").Trim().ToLowerInvariant();
        float fontSize = options.CaptionFontSize <= 0 ? 12f : options.CaptionFontSize;

        generator.Parameters.CaptionAbove.Visible = false;
        generator.Parameters.CaptionBelow.Visible = false;
        generator.Parameters.CaptionAbove.Font.Size.Point = fontSize;
        generator.Parameters.CaptionBelow.Font.Size.Point = fontSize;
        generator.Parameters.CaptionAbove.Alignment = TextAlignment.Center;
        generator.Parameters.CaptionBelow.Alignment = TextAlignment.Center;

        if (caption.Length == 0 || position is "none" or "0" or "隐藏")
        {
            return;
        }

        if (position is "above" or "1" or "上方")
        {
            generator.Parameters.CaptionAbove.Visible = true;
            generator.Parameters.CaptionAbove.Text = caption;
            return;
        }

        // below / 下方 / default when caption present
        generator.Parameters.CaptionBelow.Visible = true;
        generator.Parameters.CaptionBelow.Text = caption;
    }

    /// <summary>
    /// 固定尺寸与旋转的落地：先按 <see cref="BarcodeRenderOptions.SizeMode"/> 缩放到目标尺寸
    /// （只给宽或只给高时等比），再按 <see cref="BarcodeRenderOptions.Rotate"/> 旋转，
    /// 90/270 会交换输出宽高。缩放走 SkiaSharp 重采样，不改条码内容。
    /// </summary>
    private static byte[] ApplyAppearance(byte[] png, BarcodeRenderOptions options)
    {
        int rotate = NormalizeRotate(options.Rotate);
        bool fixedSize = string.Equals(options.SizeMode?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase);
        int width = Math.Max(0, options.BarcodeWidth);
        int height = Math.Max(0, options.BarcodeHeight);
        bool resizing = fixedSize && (width > 0 || height > 0);

        if (!resizing && rotate == 0)
        {
            return png;
        }

        using var source = SKBitmap.Decode(png)
            ?? throw new InvalidOperationException("无法解码条码图像。");

        int scaledWidth = source.Width;
        int scaledHeight = source.Height;
        if (resizing)
        {
            (scaledWidth, scaledHeight) = FitSize(source.Width, source.Height, width, height);
        }

        bool swapped = rotate is 90 or 270;
        int canvasWidth = swapped ? scaledHeight : scaledWidth;
        int canvasHeight = swapped ? scaledWidth : scaledHeight;

        using var surface = SKSurface.Create(new SKImageInfo(canvasWidth, canvasHeight));
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);
        canvas.Translate(canvasWidth / 2f, canvasHeight / 2f);
        canvas.RotateDegrees(rotate);

        using var scaled = source.Resize(new SKImageInfo(scaledWidth, scaledHeight), SKSamplingOptions.Default)
            ?? throw new InvalidOperationException("条码缩放失败。");
        canvas.DrawBitmap(scaled, -scaledWidth / 2f, -scaledHeight / 2f);
        canvas.Flush();

        using SKImage image = surface.Snapshot();
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("条码编码失败。");
        return data.ToArray();
    }

    /// <summary>把任意角度吸附到 0/90/180/270。</summary>
    private static int NormalizeRotate(int rotate)
    {
        int normalized = ((rotate % 360) + 360) % 360;
        return normalized switch
        {
            90 => 90,
            180 => 180,
            270 => 270,
            _ => 0
        };
    }

    /// <summary>固定尺寸：两维都给则精确到该尺寸，只给一维则等比。</summary>
    private static (int Width, int Height) FitSize(int sourceWidth, int sourceHeight, int width, int height)
    {
        if (width > 0 && height > 0)
        {
            return (width, height);
        }

        if (width > 0)
        {
            int scaled = Math.Max(1, (int)Math.Round(sourceHeight * (double)width / sourceWidth));
            return (width, scaled);
        }

        int scaledWidth = Math.Max(1, (int)Math.Round(sourceWidth * (double)height / sourceHeight));
        return (scaledWidth, height);
    }

    private static byte[] OverlayLogo(byte[] barcodePng, string logoFile)
    {
        using var barcode = SKBitmap.Decode(barcodePng)
            ?? throw new InvalidOperationException("无法解码条码图像。");
        using var logo = SKBitmap.Decode(logoFile)
            ?? throw new InvalidOperationException("无法解码 LOGO 图片：" + logoFile);

        int logoWidth = Math.Max(1, barcode.Width / 6);
        int logoHeight = Math.Max(1, logo.Height * logoWidth / Math.Max(1, logo.Width));
        int x = (barcode.Width - logoWidth) / 2;
        int y = (barcode.Height - logoHeight) / 2;

        using var surface = SKSurface.Create(new SKImageInfo(barcode.Width, barcode.Height));
        SKCanvas canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.DrawBitmap(barcode, 0, 0);
        using var resized = logo.Resize(new SKImageInfo(logoWidth, logoHeight), SKSamplingOptions.Default)
            ?? throw new InvalidOperationException("LOGO 缩放失败。");
        canvas.DrawBitmap(resized, x, y);
        canvas.Flush();

        using var image = surface.Snapshot();
        using var data = image.Encode(SKEncodedImageFormat.Png, 100)
            ?? throw new InvalidOperationException("条码编码失败。");
        return data.ToArray();
    }

    private static BaseEncodeType MapSymbology(string? symbology)
    {
        string key = (symbology ?? "QR").Trim();
        if (key.Equals("DataMatrix", StringComparison.OrdinalIgnoreCase)
            || key.Equals("DM", StringComparison.OrdinalIgnoreCase))
        {
            return EncodeTypes.DataMatrix;
        }

        if (key.Equals("Pdf417", StringComparison.OrdinalIgnoreCase)
            || key.Equals("PDF417", StringComparison.OrdinalIgnoreCase))
        {
            return EncodeTypes.Pdf417;
        }

        if (key.Equals("Aztec", StringComparison.OrdinalIgnoreCase))
        {
            return EncodeTypes.Aztec;
        }

        if (key.Equals("Code128", StringComparison.OrdinalIgnoreCase)
            || key.Equals("CODE_128", StringComparison.OrdinalIgnoreCase))
        {
            return EncodeTypes.Code128;
        }

        if (key.Equals("Code39", StringComparison.OrdinalIgnoreCase)
            || key.Equals("CODE_39", StringComparison.OrdinalIgnoreCase))
        {
            return EncodeTypes.Code39;
        }

        return EncodeTypes.QR;
    }
}
