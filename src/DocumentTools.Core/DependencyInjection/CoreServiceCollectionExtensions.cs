using System.Runtime.Versioning;
using DocumentTools.Core.Features;
using DocumentTools.Core.Operations;
using DocumentTools.Core.Services.Assets;
using DocumentTools.Core.Services.Code;
using DocumentTools.Core.Services.Exam;
using DocumentTools.Core.Services.Files;
using DocumentTools.Core.Services.Image;
using DocumentTools.Core.Services.Office;
using DocumentTools.Core.Services.Scan;
using DocumentTools.Core.Services.System;
using DocumentTools.Core.Services.Text;
using DocumentTools.Core.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace DocumentTools.Core.DependencyInjection;

public static class CoreServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentToolsCore(this IServiceCollection services)
    {
        // 显式工厂：AppSettingsService 的第二个参数是「首次运行种子」（可不传），
        // 交给构造函数推断会和无参构造形成歧义，这里直接钉死注册。
        services.AddSingleton<ISettingsStore>(_ => new JsonFileSettingsStore());
        services.AddSingleton<IAppSettings>(sp => new AppSettingsService(sp.GetRequiredService<ISettingsStore>()));
        services.AddSingleton<IFeatureCatalog, FeatureCatalog>();
        services.AddSingleton<IBackgroundOperationRunner, BackgroundOperationRunner>();
        services.AddSingleton<IDocumentOperationRunner, DocumentOperationRunner>();
        services.AddSingleton<IOfficeConvertService, OfficeConvertService>();
        services.AddSingleton<IDocumentMergeSplitService, DocumentMergeSplitService>();
        services.AddSingleton<IDocumentReplaceService, DocumentReplaceService>();
        services.AddSingleton<IPdfSplitService, PdfSplitService>();
        services.AddSingleton<IPdfWatermarkService, PdfWatermarkService>();
        services.AddSingleton<IPdfCompressionService, PdfCompressionService>();
        services.AddSingleton<IHtmlToPdfService, HtmlToPdfService>();
        services.AddSingleton<IImageConversionService, ImageConversionService>();
        services.AddSingleton<INameVerticalService, NameVerticalService>();
        services.AddSingleton<IMoneyChineseService, MoneyChineseService>();
        services.AddSingleton<IRandomIdCardService, RandomIdCardService>();
        services.AddSingleton<IFileCopyService, FileCopyService>();
        services.AddSingleton<IBarcodeService, BarcodeService>();
        services.AddSingleton<IDataMatrixService, DataMatrixService>();
        services.AddSingleton<IDataMatrixImageClassifyService, DataMatrixImageClassifyService>();
        services.AddSingleton<IImagePageCodeReader, AsposeImagePageCodeReader>();
        services.AddSingleton<IDocumentInfoService, DocumentInfoService>();
        services.AddSingleton<IEmbeddedAssetService, EmbeddedAssetService>();
        services.AddSingleton<IVoteReportService, VoteReportService>();
        services.AddSingleton<ISingleDocumentService, SingleDocumentService>();
        services.AddSingleton<IExamInsertService, ExamInsertService>();
        services.AddSingleton<IExamPackageService, ExamPackageService>();
        services.AddSingleton<IUnitExamMaintenanceService, UnitExamMaintenanceService>();
        services.AddSingleton<IMss3VoteService, Mss3VoteService>();
        services.AddSingleton<IExamReportService, ExamReportService>();
        services.AddSingleton<IExamIndexService, ExamIndexService>();
        services.AddSingleton<IExamCircleImageService, ExamCircleImageService>();
        services.AddSingleton<IExamCircleFixService, ExamCircleFixService>();
        services.AddSingleton<IExamCircleOrientationService, ExamCircleOrientationService>();
        services.AddSingleton<ITwainScanService, TwainScanService>();
#pragma warning disable CA1416 // App targets Windows; registry/SPI/WMI helpers are Windows-only by design.
        services.AddSingleton<IWindowsSystemToolsService, WindowsSystemToolsService>();
        services.AddSingleton<INetworkAdapterService, NetworkAdapterService>();
        services.AddSingleton<IKeyboardLockService, KeyboardLockService>();
#pragma warning restore CA1416

        foreach (var feature in BuiltInFeatures.ConvertFeatures)
        {
            string id = feature.Id;
            services.AddSingleton<IDocumentOperation>(sp => CreateOperation(id, sp));
        }

        // Replace is not in ConvertFeatures (different view) but still an IDocumentOperation.
        foreach (string replaceId in new[] { "word.replace", "excel.replace" })
        {
            string id = replaceId;
            services.AddSingleton<IDocumentOperation>(sp =>
                new ReplaceDocumentOperation(id, sp.GetRequiredService<IDocumentReplaceService>()));
        }

        return services;
    }

    private static IDocumentOperation CreateOperation(string featureId, IServiceProvider sp)
        => featureId switch
        {
            "word.pdf" or "pdf.word" or "excel.pdf" or "ppt.pdf" or "image.pdf"
                or "pdf.excel" or "pdf.ppt" or "word.excel" or "excel.word"
                or "word.epub" or "epub.word" or "epub.pdf"
                or "word.image" or "pdf.compliance"
                => new OfficeConvertOperation(featureId, sp.GetRequiredService<IOfficeConvertService>()),

            "pdf.merge" or "word.merge"
                => new MergeDocumentOperation(featureId, sp.GetRequiredService<IDocumentMergeSplitService>()),

            "pdf.split" or "word.split" or "ppt.split"
                => new SplitDocumentOperation(
                    featureId,
                    sp.GetRequiredService<IDocumentMergeSplitService>(),
                    sp.GetRequiredService<IPdfSplitService>()),

            "pdf.watermark.add" or "pdf.watermark.remove"
                => new PdfWatermarkOperation(featureId, sp.GetRequiredService<IPdfWatermarkService>()),

            "pdf.compress"
                => new PdfCompressOperation(sp.GetRequiredService<IPdfCompressionService>()),

            "html.pdf"
                => new HtmlToPdfOperation(sp.GetRequiredService<IHtmlToPdfService>()),

            "image.compress" or "webp.image" or "image.ico"
                => new ImageConvertOperation(featureId, sp.GetRequiredService<IImageConversionService>()),

            _ => throw new NotSupportedException($"未注册操作：{featureId}")
        };
}
