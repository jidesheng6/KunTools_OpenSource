using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools.Services
{
    /// <summary>
    /// UI 辅助服务：先迁移 UI.cs 中“添加文件到 ListView”的通用能力，作为解耦的第一步。
    /// </summary>
    public class UIHelperService : IUIHelperService
    {
        private readonly IFormAccessor _formAccessor;
        private readonly IDocumentConversionService _conversionService;

        public UIHelperService(IFormAccessor formAccessor, IDocumentConversionService conversionService)
        {
            _formAccessor = formAccessor;
            _conversionService = conversionService;
        }

        public Task<int> AddFilesToListViewAsync(string displayFilterString, string filterArgs, ListView listViewControl)
        {
            if (listViewControl == null) return Task.FromResult(0);

            return Task.Run(
                () =>
                {
                    IEnumerable<string> tempDocumentFileNames = Util.ShowSelectDialog(
                        true,
                        false,
                        displayFilterString,
                        filterArgs);

                    var itemsToAdd = new List<ListViewItem>();

                    try
                    {
                        foreach (string tempDocumentFileName in tempDocumentFileNames)
                        {
                            var item = new ListViewItem
                            {
                                Text = tempDocumentFileName
                            };
                            item.SubItems.Add(Util.GetNowDateTime());
                            itemsToAdd.Add(item);
                        }

                        listViewControl.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    if (itemsToAdd.Count > 0)
                                    {
                                        listViewControl.BeginUpdate();
                                        listViewControl.Items.AddRange(itemsToAdd.ToArray());
                                        listViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                        listViewControl.EndUpdate();
                                    }
                                }));
                    }
                    catch
                    {
                        // 保持与原 UI.cs 一致：吞异常，避免影响主流程（后续可统一错误处理）
                    }

                    return itemsToAdd.Count;
                });
        }

        /// <summary>
        /// 迁移自 UI.AddFilesToListViewWithNotDialog：用于 drag/drop 场景。
        /// </summary>
        public void AddFilesToListViewWithNotDialog(string[] fileList, ListView listViewControl, string[] fileFilters)
        {
            if (fileList == null || fileList.Length == 0) return;
            if (listViewControl == null) return;
            if (fileFilters == null || fileFilters.Length == 0) return;

            Task.Run(
                () =>
                {
                    try
                    {
                        fileList = fileList.OrderBy(x => x, new FileNameComparerInstance()).ToArray();

                        var itemsToAdd = new List<ListViewItem>();

                        foreach (string file in fileList)
                        {
                            FileInfo tempFileInfo = new FileInfo(file);
                            DirectoryInfo tempDirInfo = new DirectoryInfo(file);

                            if (Directory.Exists(file) &&
                                (tempDirInfo.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                            {
                                // 递归：一层目录文件
                                string[] pathFiles = Directory.GetFiles(file);
                                AddFilesToListViewWithNotDialog(pathFiles, listViewControl, fileFilters);

                                // 递归：子目录文件
                                string[] subPathFiles = Directory.GetDirectories(file);
                                AddFilesToListViewWithNotDialog(subPathFiles, listViewControl, fileFilters);
                            }
                            else if (File.Exists(file) &&
                                     (tempFileInfo.Attributes & FileAttributes.ReparsePoint) != FileAttributes.ReparsePoint)
                            {
                                foreach (string fileFilter in fileFilters)
                                {
                                    if (string.Compare(Path.GetExtension(file), fileFilter, StringComparison.Ordinal) == 0)
                                    {
                                        ListViewItem fileListListView = new ListViewItem { Text = file };
                                        fileListListView.SubItems.Add(Util.GetNowDateTime());
                                        itemsToAdd.Add(fileListListView);
                                    }
                                }
                            }
                        }

                        listViewControl.BeginInvoke(
                            new Action(
                                () =>
                                {
                                    if (itemsToAdd.Count > 0)
                                    {
                                        listViewControl.BeginUpdate();
                                        listViewControl.Items.AddRange(itemsToAdd.ToArray());
                                        listViewControl.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                                        listViewControl.EndUpdate();
                                    }
                                }));
                    }
                    catch
                    {
                    }
                });
        }

        public Task<int> AddWordFilesAsync()
        {
            var listView = _formAccessor.GetWordFileListView();
            return AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", listView);
        }

        public Task AddCompressImageFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.jpg;*.jpeg;*.png;*.bmp;*.svg", _formAccessor.GetCompressImageFileList());

        public Task AddCompressPdfFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetCompressPdfFileList());

        public Task AddConvertComplicePdfFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetConvertPdfComplianceFileList());

        public Task AddConvertImage2PDFFilesAsync()
            => AddFilesToListViewAsync("图片文件", "*.jpg;*.jpeg;*.png;*.bmp", _formAccessor.GetConvertImage2PDFFileList());

        public Task AddDataMatrixWordFilesAsync()
            => AddFilesToListViewAsync("WORD文档|PDF文档", "*.doc;*.docx;*.pdf", _formAccessor.GetDecodeMatrixWordListView());

        public Task AddHtml2PdfFilesAsync()
            => AddFilesToListViewAsync("HTML文档", "*.html;*.htm;*.mhtml;*.mht", _formAccessor.GetHtml2PdfFileList());

        public Task AddIcoConvertFilesAsync()
            => AddFilesToListViewAsync("图片文件", "*.jpg;*.png;*.bmp", _formAccessor.GetIcoFileView());

        public Task AddMergePdfFilesAsync()
            => AddFilesToListViewAsync("支持的文件", "*.pdf;*.jpg;*.png;*.bmp;*.tif", _formAccessor.GetMergePdfFileView());

        public Task AddMergeWordFilesAsync()
            => AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", _formAccessor.GetMergeWordFileList());

        public Task AddPdf2ExcelFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetPdf2ExcelFileView());

        public Task AddPdf2PPTFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetPdf2PptFileList());

        public Task AddPdfFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetPdfFileListView());

        public Task AddPPT2PdfFilesAsync()
            => AddFilesToListViewAsync("演示文稿", "*.ppt;*.pptx", _formAccessor.GetPpt2PdfFileList());

        public Task AddRemoveWaterFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetRemovePdfWaterMarkFileList());

        public Task AddSplitPdfFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetSplitPdfFileList());

        public Task AddSplitWordFilesAsync()
            => AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", _formAccessor.GetSplitWordFileList());

        public Task AddWaterMarkPdfFilesAsync()
            => AddFilesToListViewAsync("PDF文档", "*.pdf", _formAccessor.GetPdfWaterMarkFileList());

        public Task AddWeBpFilesAsync()
            => AddFilesToListViewAsync("WEBP图片", "*.webp", _formAccessor.GetWebpFileList());

        public Task AddWord2EPUBFilesAsync()
            => AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", _formAccessor.GetWord2EpubFileList());

        public Task AddWord2ExcelFilesAsync()
            => AddFilesToListViewAsync("WORD文档", "*.doc;*.docx;*.wps", _formAccessor.GetWord2ExcelFileList());

        public Task AddWord2ImageFilesAsync()
            => AddFilesToListViewAsync("支持的文档", "*.docx;*.doc;*.xls;*.xlsx;*.pdf", _formAccessor.GetDocumentsListView());

        public Task AddWordReplaceFilesAsync()
            => AddFilesToListViewAsync("WORD文档", "*.docx;*.doc;*.wps", _formAccessor.GetReplaceWordFileList());

        public Task AddSplitPPTFilesAsync()
            => AddFilesToListViewAsync("演示文稿", "*.ppt;*.pptx", _formAccessor.GetSplitPptFilesList());

        public Task AddEpub2PdfFilesAsync()
            => AddFilesToListViewAsync("EPUB文件", "*.epub", _formAccessor.GetEpub2PdfFileList());

        public Task AddEpub2WordFilesAsync()
            => AddFilesToListViewAsync("EPUB文件", "*.epub", _formAccessor.GetEpub2WordFileList());

        public Task AddExcel2WordFilesAsync()
            => AddFilesToListViewAsync("EXCEL文档", "*.xls;*.xlsx", _formAccessor.GetExcel2WordFileList());

        public Task AddExcelFilesAsync()
            => AddFilesToListViewAsync("EXCEL文档", "*.xls;*.xlsx;*.et", _formAccessor.GetExcelFileListView());

        public Task AddExcelReplaceFilesAsync()
            => AddFilesToListViewAsync("EXCEL文档", "*.xls;*.xlsx", _formAccessor.GetReplaceExcelFileView());

        public Task ConvertStartAsync(
            ListView listViewControl,
            Label statusLabelControl,
            DocumentTools.ConvertFileType convertType,
            string extensionString,
            bool useSourcePath = false,
            Button callButton = null)
        {
            return _conversionService.ConvertStartAsync(listViewControl, statusLabelControl, convertType, extensionString, useSourcePath, callButton);
        }

        public Task ConvertWord2PdfAsync()
        {
            // 占位：后续迁移为专用入口，并通过 options 传参而非读取 UI 控件
            throw new NotImplementedException("ConvertWord2PdfAsync 将在后续阶段实现。");
        }
    }
}
