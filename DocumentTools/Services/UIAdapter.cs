using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools.Services
{
    /// <summary>
    /// 向后兼容的静态适配器，内部委托给 IUIHelperService。
    /// 后续迁移完成后，可逐步移除对 UI.cs 静态方法的依赖。
    /// </summary>
    public static class UIAdapter
    {
        private static IUIHelperService _uiHelperService;

        public static void Initialize(IUIHelperService uiHelperService)
        {
            _uiHelperService = uiHelperService;
        }

        private static IUIHelperService Service =>
            _uiHelperService ?? throw new InvalidOperationException("UIAdapter 未初始化，请在入口处调用 Initialize。");

        public static Task<int> AddFilesToListViewAsync(string displayFilterString, string filterArgs, ListView listViewControl)
            => Service.AddFilesToListViewAsync(displayFilterString, filterArgs, listViewControl);

        public static Task<int> AddWordFilesAsync() => Service.AddWordFilesAsync();

        public static Task AddCompressImageFilesAsync() => Service.AddCompressImageFilesAsync();
        public static Task AddCompressPdfFilesAsync() => Service.AddCompressPdfFilesAsync();
        public static Task AddConvertComplicePdfFilesAsync() => Service.AddConvertComplicePdfFilesAsync();
        public static Task AddConvertImage2PDFFilesAsync() => Service.AddConvertImage2PDFFilesAsync();
        public static Task AddDataMatrixWordFilesAsync() => Service.AddDataMatrixWordFilesAsync();

        public static Task AddHtml2PdfFilesAsync() => Service.AddHtml2PdfFilesAsync();
        public static Task AddIcoConvertFilesAsync() => Service.AddIcoConvertFilesAsync();
        public static Task AddMergePdfFilesAsync() => Service.AddMergePdfFilesAsync();
        public static Task AddMergeWordFilesAsync() => Service.AddMergeWordFilesAsync();
        public static Task AddPdf2ExcelFilesAsync() => Service.AddPdf2ExcelFilesAsync();
        public static Task AddPdf2PPTFilesAsync() => Service.AddPdf2PPTFilesAsync();
        public static Task AddPdfFilesAsync() => Service.AddPdfFilesAsync();
        public static Task AddPPT2PdfFilesAsync() => Service.AddPPT2PdfFilesAsync();
        public static Task AddRemoveWaterFilesAsync() => Service.AddRemoveWaterFilesAsync();
        public static Task AddSplitPdfFilesAsync() => Service.AddSplitPdfFilesAsync();
        public static Task AddSplitWordFilesAsync() => Service.AddSplitWordFilesAsync();
        public static Task AddWaterMarkPdfFilesAsync() => Service.AddWaterMarkPdfFilesAsync();
        public static Task AddWeBpFilesAsync() => Service.AddWeBpFilesAsync();
        public static Task AddWord2EPUBFilesAsync() => Service.AddWord2EPUBFilesAsync();
        public static Task AddWord2ExcelFilesAsync() => Service.AddWord2ExcelFilesAsync();
        public static Task AddWord2ImageFilesAsync() => Service.AddWord2ImageFilesAsync();
        public static Task AddWordReplaceFilesAsync() => Service.AddWordReplaceFilesAsync();
        public static Task AddSplitPPTFilesAsync() => Service.AddSplitPPTFilesAsync();
        public static Task AddEpub2PdfFilesAsync() => Service.AddEpub2PdfFilesAsync();
        public static Task AddEpub2WordFilesAsync() => Service.AddEpub2WordFilesAsync();
        public static Task AddExcel2WordFilesAsync() => Service.AddExcel2WordFilesAsync();
        public static Task AddExcelFilesAsync() => Service.AddExcelFilesAsync();
        public static Task AddExcelReplaceFilesAsync() => Service.AddExcelReplaceFilesAsync();

        public static void AddFilesToListViewWithNotDialog(string[] fileList, ListView listViewControl, string[] fileFilters)
            => Service.AddFilesToListViewWithNotDialog(fileList, listViewControl, fileFilters);

        public static Task ConvertStartAsync(
            ListView listViewControl,
            Label statusLabelControl,
            DocumentTools.ConvertFileType convertType,
            string extensionString,
            bool useSourcePath = false,
            Button callButton = null)
            => Service.ConvertStartAsync(listViewControl, statusLabelControl, convertType, extensionString, useSourcePath, callButton);

        public static Task ConvertWord2PdfAsync() => Service.ConvertWord2PdfAsync();
    }
}
