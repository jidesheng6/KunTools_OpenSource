using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools.Services
{
    public interface IUIHelperService
    {
        Task<int> AddFilesToListViewAsync(string displayFilterString, string filterArgs, ListView listViewControl);

        void AddFilesToListViewWithNotDialog(string[] fileList, ListView listViewControl, string[] fileFilters);

        Task<int> AddWordFilesAsync();

        Task AddCompressImageFilesAsync();
        Task AddCompressPdfFilesAsync();
        Task AddConvertComplicePdfFilesAsync();
        Task AddConvertImage2PDFFilesAsync();
        Task AddDataMatrixWordFilesAsync();

        Task AddHtml2PdfFilesAsync();
        Task AddIcoConvertFilesAsync();
        Task AddMergePdfFilesAsync();
        Task AddMergeWordFilesAsync();
        Task AddPdf2ExcelFilesAsync();
        Task AddPdf2PPTFilesAsync();
        Task AddPdfFilesAsync();
        Task AddPPT2PdfFilesAsync();
        Task AddRemoveWaterFilesAsync();
        Task AddSplitPdfFilesAsync();
        Task AddSplitWordFilesAsync();
        Task AddWaterMarkPdfFilesAsync();
        Task AddWeBpFilesAsync();
        Task AddWord2EPUBFilesAsync();
        Task AddWord2ExcelFilesAsync();
        Task AddWord2ImageFilesAsync();
        Task AddWordReplaceFilesAsync();
        Task AddSplitPPTFilesAsync();
        Task AddEpub2PdfFilesAsync();
        Task AddEpub2WordFilesAsync();
        Task AddExcel2WordFilesAsync();
        Task AddExcelFilesAsync();
        Task AddExcelReplaceFilesAsync();

        Task ConvertStartAsync(
            ListView listViewControl,
            Label statusLabelControl,
            DocumentTools.ConvertFileType convertType,
            string extensionString,
            bool useSourcePath = false,
            Button callButton = null);

        // 先保留占位，后续迁移转换逻辑
        Task ConvertWord2PdfAsync();
    }
}
