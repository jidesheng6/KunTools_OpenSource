using Aspose.Cells;
using DocumentTools.Core.Services.Code;

namespace DocumentTools.Tests;

public sealed class BarcodeBatchTemplateTests : IDisposable
{
    private readonly BarcodeService _service = new();
    private readonly string _root = Path.Combine(Path.GetTempPath(), "dt-barcode-batch-" + Guid.NewGuid().ToString("N"));

    public BarcodeBatchTemplateTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void ExportAndReadTemplate_RoundTripsAllThreeFields()
    {
        string file = Path.Combine(_root, "template.xlsx");
        BarcodeBatchRow[] expected =
        [
            new("工单-001", "一号标题", "DOC-001"),
            new("工单-002", "", "DOC-002")
        ];

        _service.ExportBatchTemplate(file, expected);
        IReadOnlyList<BarcodeBatchRow> actual = _service.ReadBatchTemplate(file);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ReadTemplate_AcceptsLegacyXlsAndMapsNumberCaptionContent()
    {
        string file = Path.Combine(_root, "legacy.xls");
        using (var workbook = new Workbook())
        {
            Cells cells = workbook.Worksheets[0].Cells;
            cells[0, 0].PutValue("编号");
            cells[0, 1].PutValue("标题");
            cells[0, 2].PutValue("内容");
            cells[1, 0].PutValue("legacy-name");
            cells[1, 1].PutValue("legacy-caption");
            cells[1, 2].PutValue("LEGACY-42");
            workbook.Save(file, Aspose.Cells.SaveFormat.Excel97To2003);
        }

        BarcodeBatchRow row = Assert.Single(_service.ReadBatchTemplate(file));

        Assert.Equal("legacy-name", row.FileName);
        Assert.Equal("legacy-caption", row.Caption);
        Assert.Equal("LEGACY-42", row.Content);
    }

    [Fact]
    public void GenerateBatch_UsesTemplateFileNamesAndSupportsPngAndBmp()
    {
        BarcodeBatchRow[] rows = [new("named-output", "标题", "DOC-2026")];
        var options = new BarcodeRenderOptions
        {
            Symbology = "Code39",
            ModuleSize = 4,
            CaptionPosition = "below"
        };
        string pngDirectory = Path.Combine(_root, "png");
        string bmpDirectory = Path.Combine(_root, "bmp");

        BarcodeBatchResult png = _service.GenerateBatch(rows, pngDirectory, options, BarcodeBatchImageFormat.Png);
        BarcodeBatchResult bmp = _service.GenerateBatch(rows, bmpDirectory, options, BarcodeBatchImageFormat.Bmp);

        string pngFile = Assert.Single(png.OutputFiles);
        string bmpFile = Assert.Single(bmp.OutputFiles);
        Assert.Equal("named-output.png", Path.GetFileName(pngFile));
        Assert.Equal("named-output.bmp", Path.GetFileName(bmpFile));
        Assert.Equal(["DOC-2026"], _service.ReadTexts(pngFile));
        Assert.Equal((byte)'B', File.ReadAllBytes(bmpFile)[0]);
        Assert.Equal((byte)'M', File.ReadAllBytes(bmpFile)[1]);
    }

    [Fact]
    public void GenerateBatch_DuplicateNamesFailBeforeWritingAnything()
    {
        string output = Path.Combine(_root, "duplicates");
        BarcodeBatchRow[] rows =
        [
            new("same", "A", "ONE"),
            new("same.png", "B", "TWO")
        ];

        InvalidDataException error = Assert.Throws<InvalidDataException>(() =>
            _service.GenerateBatch(rows, output, new BarcodeRenderOptions()));

        Assert.Contains("重复编号", error.Message);
        Assert.Empty(Directory.GetFiles(output));
    }

    [Fact]
    public void GenerateBatch_RowCaptionOverridesSharedOptions()
    {
        string output = Path.Combine(_root, "captions");
        BarcodeBatchRow[] rows =
        [
            new("with-caption", "本行标题", "SAME-CONTENT"),
            new("without-caption", "", "SAME-CONTENT")
        ];

        BarcodeBatchResult result = _service.GenerateBatch(
            rows,
            output,
            new BarcodeRenderOptions
            {
                Symbology = "Code39",
                Caption = "不应沿用的全局标题",
                CaptionPosition = "none"
            });

        byte[] withCaption = File.ReadAllBytes(result.OutputFiles[0]);
        byte[] withoutCaption = File.ReadAllBytes(result.OutputFiles[1]);
        Assert.False(withCaption.SequenceEqual(withoutCaption));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
            // A locked temp file should not hide the assertion result.
        }
    }
}
