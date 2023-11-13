using Aspose.BarCode.BarCodeRecognition;
using Aspose.BarCode.Generation;
using Aspose.Cells;
using Aspose.Cells.Rendering;
using Aspose.Imaging;
using Aspose.Imaging.ImageOptions;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Text;
using Aspose.Slides.Export;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NAudio.Wave;
using PuppeteerSharp.Media;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Color = System.Drawing.Color;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DocumentTools
{
    internal class Util
    {
        /// <summary>
        /// 显示选择文件框，获取选中的文件名称列表
        /// </summary>
        /// <param name="MulitSelect">是否可以多选</param>
        /// <param name="isPickFolder">是否选择目录</param>
        /// <param name="FilterDisplayName">过滤显示的名称</param>
        /// <param name="FilterExtensionsName">实际过滤的文件名称列表</param>
        /// <returns></returns>
        public static IEnumerable<string> ShwoSelectDialog(bool MulitSelect, bool isPickFolder, string FilterDisplayName, string FilterExtensionsName)
        {
            IEnumerable<string> ReturnFileNameString = new List<string>();

            CommonOpenFileDialog FileSelectDialog = new CommonOpenFileDialog
            {
                Multiselect = MulitSelect,
                IsFolderPicker = isPickFolder
            };

            if (!isPickFolder)
            {
                CommonFileDialogFilter FileFilterItems = new CommonFileDialogFilter(FilterDisplayName, FilterExtensionsName);
                FileSelectDialog.Filters.Add(FileFilterItems);
            }

            CommonFileDialogResult FileSelectResult = FileSelectDialog.ShowDialog();

            if (FileSelectResult == CommonFileDialogResult.Ok)
            {
                ReturnFileNameString = FileSelectDialog.FileNames;
            }

            return ReturnFileNameString;
        }

        /// <summary>
        /// 显示保存文件对话框，用于保存单个文件
        /// </summary>
        /// <param name="FileFilterString">过滤保存文件的类型</param>
        /// <param name="DisplayTitleString">设置对话框的标题提示名称</param>
        /// <returns></returns>
        public static string ShowSaveFileDialog(string FileFilterString, string DisplayTitleString = "保存文件")
        {
            string SaveFileName = string.Empty;
            BodyForm.MainForm.SaveFileDialog.CheckPathExists = true;
            BodyForm.MainForm.SaveFileDialog.AddExtension = true;
            BodyForm.MainForm.SaveFileDialog.Title = DisplayTitleString;
            BodyForm.MainForm.SaveFileDialog.Filter = FileFilterString;
            DialogResult SaveFileDialogResult = BodyForm.MainForm.SaveFileDialog.ShowDialog();
            if (SaveFileDialogResult == DialogResult.OK)
            {
                SaveFileName = BodyForm.MainForm.SaveFileDialog.FileName;
            }
            return SaveFileName;
        }


        /// <summary>
        /// 获取当前时间，简单格式化
        /// </summary>
        /// <returns></returns>
        public static string GetNowDateTime()
        {
            DateTime NowDateTime = DateTime.Now;

            return NowDateTime.ToString("yyyy年/MM月/dd日 HH:mm:ss");
        }

        /// <summary>
        /// 获取内嵌的资源对象
        /// </summary>
        /// <param name="ResourceName">资源名称，不需要带后缀</param>
        /// <returns></returns>
        public static object GetEmbedResourceObj(string ResourceName)
        {
            Assembly SelfAssmbly = Assembly.GetExecutingAssembly();
            ResourceManager TempManager = new ResourceManager("DocumentTools.Properties.Resources", SelfAssmbly);
            return TempManager.GetObject(ResourceName);
        }

        /// <summary>
        /// 播放MP3流
        /// </summary>
        public static void PlayMp3WithStream(string MusicName)
        {
            //这里必须这样写，不要用using语句来自动执行dispose，否则声音还没播放的时候就已经被dispose了
            MemoryStream EmbedResouceObj = new MemoryStream((byte[])GetEmbedResourceObj(MusicName));
            StreamMediaFoundationReader TempStream = new StreamMediaFoundationReader(EmbedResouceObj);
            IWavePlayer Player = new WaveOut();
            Player.Init(TempStream);
            Player.Play();

        }
        /// <summary>
        /// 获取文件页数，暂时不用了，影响加载性能
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static string GetDocumentPages(string FileName)
        {
            Aspose.Hook.HookManager.StartHook();
            string Extension = Path.GetExtension(FileName);

            int FilePages = 0;

            switch (Extension)
            {
                case ".doc":
                    FilePages = new Aspose.Words.Document(FileName).PageCount;
                    break;
                case ".docx":
                    FilePages = new Aspose.Words.Document(FileName).PageCount;
                    break;
                case ".wps":
                    FilePages = new Aspose.Words.Document(FileName).PageCount;
                    break;
                case ".xls":
                    FilePages = new Aspose.Cells.Workbook(FileName).Worksheets.Count;
                    break;
                case ".xlsx":
                    FilePages = new Aspose.Cells.Workbook(FileName).Worksheets.Count;
                    break;
                case ".pdf":
                    FilePages = new Aspose.Pdf.Document(FileName).Pages.Count;
                    break;
                case ".ppt":
                    FilePages = new Aspose.Slides.Presentation(FileName).Slides.Count;
                    break;
                default:
                    break;
            }
            return FilePages.ToString();
        }

        /// <summary>
        /// 将word文档转换为pdf文件
        /// </summary>
        /// <param name="documentFileName">doc/docx文档路径</param>
        /// <param name="SavePdfName">导出pdf的路径</param>
        /// <param name="tempSaveDocxPath">临时存放转换的docx目录</param>
        public static void ConvertWord2Pdf(string documentFileName, string SavePdfName, string tempSaveDocxPath, Aspose.Words.Saving.PdfCompliance PdfComplanceOption, Aspose.Words.Settings.MsWordVersion OptiMizMswVersion, bool UpdateField,
            Aspose.Words.Saving.ColorMode SavePdfColorMode)
        {
            Aspose.Hook.HookManager.StartHook();
            string DocumentName, SaveDocxName;

            //设置加载word文档的时候参数
            Aspose.Words.Loading.LoadOptions TempDocuemntLoadOption = new Aspose.Words.Loading.LoadOptions()
            {
                ConvertMetafilesToPng = false,
                MswVersion = OptiMizMswVersion,
            };

            //设置PDF保存参数
            Aspose.Words.Saving.PdfSaveOptions TempPdfaveOption = new Aspose.Words.Saving.PdfSaveOptions()
            {
                FontEmbeddingMode = Aspose.Words.Saving.PdfFontEmbeddingMode.EmbedAll,
                ColorMode = SavePdfColorMode,
                ImageColorSpaceExportMode = Aspose.Words.Saving.PdfImageColorSpaceExportMode.Auto,
                ImageCompression = Aspose.Words.Saving.PdfImageCompression.Auto,
                PageMode = Aspose.Words.Saving.PdfPageMode.UseOutlines,
                ZoomBehavior = Aspose.Words.Saving.PdfZoomBehavior.None,
                SaveFormat = Aspose.Words.SaveFormat.Pdf,
                UseHighQualityRendering = true,
                UseAntiAliasing = true,
                PreblendImages = true,
                JpegQuality = 100,
                Compliance = PdfComplanceOption,
                ExportGeneratorName = false,
                UpdateFields = UpdateField,
            };

            //设置PDF保存带word中的目录
            TempPdfaveOption.OutlineOptions.DefaultBookmarksOutlineLevel = 0;
            TempPdfaveOption.OutlineOptions.CreateMissingOutlineLevels = false;
            TempPdfaveOption.OutlineOptions.ExpandedOutlineLevels = 3;
            TempPdfaveOption.OutlineOptions.HeadingsOutlineLevels = 9;
            TempPdfaveOption.HeaderFooterBookmarksExportMode = Aspose.Words.Saving.HeaderFooterBookmarksExportMode.All;


            //设置Docx保存参数
            Aspose.Words.Saving.OoxmlSaveOptions TempDocxSaveOption = new Aspose.Words.Saving.OoxmlSaveOptions()
            {
                Compliance = Aspose.Words.Saving.OoxmlCompliance.Iso29500_2008_Transitional,
                SaveFormat = Aspose.Words.SaveFormat.Docx,
                KeepLegacyControlChars = true,
                UpdateFields = UpdateField,
            };

            Aspose.Words.Document TempDocument = new Aspose.Words.Document(documentFileName,
                                                                           TempDocuemntLoadOption)
            {
                RemovePersonalInformation = true,
            };

            TempDocument.CompatibilityOptions.OptimizeFor(OptiMizMswVersion);
            TempDocument.CompatibilityOptions.BalanceSingleByteDoubleByteWidth = true;//区分全角和半角字符


            DocumentName = System.IO.Path.GetFileNameWithoutExtension(documentFileName);

            SaveDocxName = System.IO.Path.Combine(tempSaveDocxPath, DocumentName + ".docx");


            TempDocument.Save(SaveDocxName, TempDocxSaveOption);

            Aspose.Words.Document TempDocxDocument = new Aspose.Words.Document(SaveDocxName, TempDocuemntLoadOption);

            TempDocxDocument.Save(SavePdfName, TempPdfaveOption);
        }

        /// <summary>
        /// 将PDF文档转为DOCX文件
        /// </summary>
        /// <param name="pdfFileName">pdf文件名</param>
        /// <param name="SavePdfName">导出docx</param>
        public static void ConvertPdf2Word(string pdfFileName, string SavePdfName, bool UseBullet, bool UseSubOcr, Aspose.Pdf.DocSaveOptions.RecognitionMode UseMode)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Pdf.DocSaveOptions TempDocxSaveOption = new Aspose.Pdf.DocSaveOptions
            {
                ExtractOcrSublayerOnly = UseSubOcr,
                Format = Aspose.Pdf.DocSaveOptions.DocFormat.DocX,
                Mode = UseMode,
                RecognizeBullets = UseBullet,
                CustomProgressHandler = new Aspose.Pdf.UnifiedSaveOptions.ConversionProgressEventHandler(WritePdf2WordDebugInfo)
            };

            using Aspose.Pdf.Document TempPdfFileDocument = new Aspose.Pdf.Document(pdfFileName);

            TempPdfFileDocument.Info.Author = "DocumentTools";
            TempPdfFileDocument.Info.Producer = "DocumentTools";
            TempPdfFileDocument.Info.Creator = "DocumentTools";

            //TempPdfFileDocument.Decrypt();

            TempPdfFileDocument.Save(SavePdfName, TempDocxSaveOption);
        }
        /// <summary>
        /// 调试pdf2word的功能输出信息
        /// </summary>
        /// <param name="info"></param>
        public static void WritePdf2WordDebugInfo(Aspose.Pdf.DocSaveOptions.ProgressEventHandlerInfo info)
        {
            switch (info.EventType)
            {
                case Aspose.Pdf.ProgressEventType.TotalProgress:
                    Debug.WriteLine($"Pdf2Word_Process：{info.Value}");
                    break;
                case Aspose.Pdf.ProgressEventType.SourcePageAnalysed:
                    Debug.WriteLine($"Pdf2Word_AnalsyPage：{info.Value}");
                    break;
                case Aspose.Pdf.ProgressEventType.ResultPageCreated:
                    Debug.WriteLine($"Pdf2Word_PageCreate：{info.Value}");
                    break;
                case Aspose.Pdf.ProgressEventType.ResultPageSaved:
                    BodyForm.MainForm.ProcessPage_P2W.Text = $"已保存{info.Value}页";
                    break;
                default: break;
            }
        }

        /// <summary>
        /// 将多个PDF文件合并为一个PDF文件
        /// </summary>
        /// <param name="pdfFileNames">要合并的PDF文档列表</param>
        /// <param name="mergrePdfFileName">合并后保存的PDF文件名称</param>
        public static void MergrePdfFIle(List<string> pdfFileNames, string mergrePdfFileName, bool IsMergePicture, string PageMode)
        {
            Aspose.Hook.HookManager.StartHook();
            List<string> TempFileList = new List<string>();
            Aspose.Words.Document TempPicturePdf = new Aspose.Words.Document();
            Aspose.Words.DocumentBuilder TempPicturePdfBuilder = new Aspose.Words.DocumentBuilder(TempPicturePdf);
            switch (PageMode)
            {
                case "A4_Hen":
                    TempPicturePdfBuilder.PageSetup.PaperSize = Aspose.Words.PaperSize.A4;
                    TempPicturePdfBuilder.PageSetup.Orientation = Aspose.Words.Orientation.Landscape;
                    break;
                case "A4_Shu":
                    TempPicturePdfBuilder.PageSetup.PaperSize = Aspose.Words.PaperSize.A4;
                    TempPicturePdfBuilder.PageSetup.Orientation = Aspose.Words.Orientation.Portrait;
                    break;
                case "A3_Hen":
                    TempPicturePdfBuilder.PageSetup.PaperSize = Aspose.Words.PaperSize.A3;
                    TempPicturePdfBuilder.PageSetup.Orientation = Aspose.Words.Orientation.Landscape;
                    break;
                case "A3_Shu":
                    TempPicturePdfBuilder.PageSetup.PaperSize = Aspose.Words.PaperSize.A3;
                    TempPicturePdfBuilder.PageSetup.Orientation = Aspose.Words.Orientation.Portrait;
                    break;
                default: break;
            }

            //如果是pdf格式则进行pdf合并，如果不是pdf则进行document插入图片合并转换为pdf
            pdfFileNames.ForEach(FileName =>
            {
                string FileExtension = System.IO.Path.GetExtension(FileName);
                if (FileExtension != ".pdf")
                {
                    TempPicturePdfBuilder.InsertImage(FileName);
                    TempPicturePdfBuilder.Writeln();

                }
                else
                {
                    TempFileList.Add(FileName);
                }
            });
            if (IsMergePicture)
            {
                TempPicturePdf.Save(mergrePdfFileName);
            }
            else
            {
                Aspose.Pdf.Facades.PdfFileEditor TempPdfEditor = new Aspose.Pdf.Facades.PdfFileEditor();
                TempPdfEditor.Concatenate(TempFileList.ToArray(), mergrePdfFileName);
            }
        }

        /// <summary>
        /// 图片转为PDF
        /// </summary>
        /// <param name="ImageFileName"></param>
        /// <param name="SavedDocumentFileName"></param>
        /// <param name="IsNeedMerge"></param>
        /// <param name="ImageFilesList"></param>
        /// <param name="LandStyle"></param>
        /// <param name="AutoOptiomizePdf"></param>
        /// <param name="CompressPdfImage"></param>
        /// <param name="Margin_TOP"></param>
        /// <param name="Margin_LEFT"></param>
        /// <param name="Margin_Bottom"></param>
        /// <param name="Margin_RIGHT"></param>
        public static void ConvertImage2PDF(string ImageFileName, string SavedDocumentFileName, bool IsNeedMerge, List<string> ImageFilesList, int LandStyleIndex, bool AutoOptiomizePdf, bool CompressPdfImage, double Margin_TOP = 0, double Margin_LEFT = 0, double Margin_Bottom = 0, double Margin_RIGHT = 0)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Pdf.Document BuilderDocument = new Aspose.Pdf.Document();
            Aspose.Pdf.Optimization.OptimizationOptions TempOpt = new Aspose.Pdf.Optimization.OptimizationOptions
            {
                RemoveUnusedObjects = true,
                RemoveUnusedStreams = true,
                AllowReusePageContent = true,
                LinkDuplcateStreams = true
            };

            if (CompressPdfImage)
            {
                TempOpt.ImageCompressionOptions.CompressImages = true;
                TempOpt.ImageCompressionOptions.Encoding = Aspose.Pdf.Optimization.ImageEncoding.Flate;
            }

            if (IsNeedMerge)
            {
                ImageFilesList.ForEach(file =>
                {
                    Page TempPage = BuilderDocument.Pages.Add();
                    Aspose.Pdf.Image TempLoadImage = new Aspose.Pdf.Image { File = file };
                    switch (LandStyleIndex)
                    {
                        case 0:
                            TempPage.PageInfo.Height = System.Drawing.Image.FromFile(file).Height;
                            TempPage.PageInfo.Width = System.Drawing.Image.FromFile(file).Height;
                            break;
                        case 1:
                            TempPage.PageInfo.IsLandscape = true;
                            break;
                        case 2:
                            TempPage.PageInfo.IsLandscape = false;
                            break;
                        default:
                            break;
                    }
                    TempPage.PageInfo.Margin.Top = Margin_TOP;
                    TempPage.PageInfo.Margin.Left = Margin_LEFT;
                    TempPage.PageInfo.Margin.Right = Margin_RIGHT;
                    TempPage.PageInfo.Margin.Bottom = Margin_Bottom;
                    TempPage.Paragraphs.Add(TempLoadImage);
                });
                if (AutoOptiomizePdf)
                {
                    BuilderDocument.Optimize();
                    BuilderDocument.OptimizeResources(TempOpt);
                }
                BuilderDocument.Save(SavedDocumentFileName);
            }
            else
            {
                Page TempPage = BuilderDocument.Pages.Add();
                Aspose.Pdf.Image TempLoadImage = new Aspose.Pdf.Image { File = ImageFileName };
                switch (LandStyleIndex)
                {
                    case 0:
                        TempPage.PageInfo.Height = System.Drawing.Image.FromFile(ImageFileName).Height;
                        TempPage.PageInfo.Width = System.Drawing.Image.FromFile(ImageFileName).Height;
                        break;
                    case 1:
                        TempPage.PageInfo.IsLandscape = true;
                        break;
                    case 2:
                        TempPage.PageInfo.IsLandscape = false;
                        break;
                    default:
                        break;
                }
                TempPage.PageInfo.Margin.Top = Margin_TOP;
                TempPage.PageInfo.Margin.Left = Margin_LEFT;
                TempPage.PageInfo.Margin.Right = Margin_RIGHT;
                TempPage.PageInfo.Margin.Bottom = Margin_Bottom;
                TempPage.Paragraphs.Add(TempLoadImage);
                if (AutoOptiomizePdf)
                {
                    BuilderDocument.Optimize();
                    BuilderDocument.OptimizeResources(TempOpt);
                }
                BuilderDocument.Save(SavedDocumentFileName);
            }
        }

        /// <summary>
        /// 将Excel转为PDF文件
        /// </summary>
        /// <param name="ExcelFileName">Excel文件名称</param>
        /// <param name="SavePdfFileName">导出pdf的路径</param>
        public static void ConvertExcel2Pdf(string ExcelFileName, string SavePdfFileName, bool ExportEachSheet)
        {
            Aspose.Hook.HookManager.StartHook();

            using Aspose.Cells.Workbook TempExcelDocument = new Aspose.Cells.Workbook(ExcelFileName);
            if (ExportEachSheet)
            {
                Aspose.Cells.PdfSaveOptions SavePdfOptions = new Aspose.Cells.PdfSaveOptions()
                {
                    OnePagePerSheet = true,
                    CalculateFormula = true,

                };

                //将Excel中的每一个Sheet保存为单个PDF文件
                for (int i = 0; i < TempExcelDocument.Worksheets.Count; i++)
                {
                    TempExcelDocument.Worksheets[i].IsVisible = true;
                    for (int j = 0; j < TempExcelDocument.Worksheets.Count; j++)
                    {
                        if (j != i)
                        {
                            TempExcelDocument.Worksheets[j].IsVisible = false;
                        }
                    }
                    string TempSavedPdfileWithSheetName = SavePdfFileName.Replace(".pdf", $"_{TempExcelDocument.Worksheets[i].Name}.pdf");
                    TempExcelDocument.Save(TempSavedPdfileWithSheetName, SavePdfOptions);
                }
            }
            else
            {
                TempExcelDocument.Save(SavePdfFileName, Aspose.Cells.SaveFormat.Pdf);
            }
        }

        /// <summary>
        /// 将PDF转为Excel文件
        /// </summary>
        /// <param name="PdfFileName">PDF文件名称</param>
        /// <param name="SaveExcelFileName">导出excel的路径</param>
        public static void ConvertPdf2Excel(string PdfFileName, string SaveExcelFileName, bool MergeAllPages)
        {
            Aspose.Hook.HookManager.StartHook();

            using Aspose.Pdf.Document TempPdfDocuemnt = new Aspose.Pdf.Document(PdfFileName);

            Aspose.Pdf.ExcelSaveOptions TempExcelSaveOption = new Aspose.Pdf.ExcelSaveOptions()
            {
                Format = Aspose.Pdf.ExcelSaveOptions.ExcelFormat.XLSX,
                ExtractOcrSublayerOnly = true,
            };
            if (MergeAllPages)
            {
                TempExcelSaveOption.MinimizeTheNumberOfWorksheets = true;
            }
            else
            {
                TempExcelSaveOption.MinimizeTheNumberOfWorksheets = false;
            }

            TempPdfDocuemnt.Save(SaveExcelFileName, TempExcelSaveOption);
        }


        /// <summary>
        /// 将word文档转换为图片文件
        /// </summary>
        /// <param name="DocumentsFileName">文档名称</param>
        /// <param name="SavedImageFileName">保存后图片的名称</param>
        /// <param name="ConvertImageType">转换的目标图片类型</param>
        /// <param name="SetGrayMode">是否需要进行灰度处理</param>
        /// <param name="PageExport">是否需要逐页导出</param>
        /// <param name="DpiValue">导出的图像DPI分辨率</param>
        public static void ConvertWord2Image(string DocumentsFileName, string SavedImageFileName, string ConvertImageType, bool SetGrayMode, bool PageExport, int DpiValue = 150)
        {
            Aspose.Hook.HookManager.StartHook();

            string FileType = System.IO.Path.GetExtension(DocumentsFileName);

            //初始化一个临时参数，到时候重新初始化
            Aspose.Words.Saving.ImageSaveOptions TempWordImageSaveOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Jpeg);

            Aspose.Words.Document TempWordDocument = null;
            Aspose.Cells.Workbook TempExcelDocument = null;
            Aspose.Cells.Rendering.ImageOrPrintOptions TempExcelImageOPtion = new Aspose.Cells.Rendering.ImageOrPrintOptions();
            Aspose.Pdf.Document TempPdfDocument = null;
            Aspose.Pdf.Devices.ImageDevice TempPdfImageDevice = null;
            Aspose.Pdf.Devices.TiffDevice TempPdfTifDevice = null;
            Aspose.Pdf.Devices.Resolution TempPdfImageResolution = new Aspose.Pdf.Devices.Resolution(DpiValue);
            Aspose.Imaging.ImageOptions.JpegOptions TempConvertJpegOption = new JpegOptions();
            Aspose.Imaging.ImageOptions.PngOptions TempConvertPngOption = new PngOptions();
            Aspose.Imaging.ImageOptions.BmpOptions TempConvertBmpOption = new BmpOptions();



            switch (FileType)
            {
                case ".doc":
                    TempWordDocument = new Aspose.Words.Document(DocumentsFileName);
                    break;
                case ".docx":
                    TempWordDocument = new Aspose.Words.Document(DocumentsFileName);
                    break;
                case ".pdf":
                    TempPdfDocument = new Aspose.Pdf.Document(DocumentsFileName);
                    break;
                case ".xls":
                    TempExcelDocument = new Aspose.Cells.Workbook(DocumentsFileName);
                    break;
                case ".xlsx":
                    TempExcelDocument = new Aspose.Cells.Workbook(DocumentsFileName);
                    break;
                case ".ppt":
                    _ = new Aspose.Slides.Presentation(DocumentsFileName);
                    break;
                case ".pptx":
                    _ = new Aspose.Slides.Presentation(DocumentsFileName);
                    break;
                default: break;
            }

            //根据不同导出类型进行预初始化处理
            switch (ConvertImageType)
            {
                case ".jpg":
                    TempWordImageSaveOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Jpeg)
                    {
                        JpegQuality = 100
                    };
                    TempPdfImageDevice = new Aspose.Pdf.Devices.JpegDevice(TempPdfImageResolution, 100);
                    TempExcelImageOPtion.ImageType = Aspose.Cells.Drawing.ImageType.Jpeg;
                    TempExcelImageOPtion.Quality = 100;
                    break;
                case ".png":
                    TempWordImageSaveOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png);
                    TempPdfImageDevice = new Aspose.Pdf.Devices.PngDevice(TempPdfImageResolution);
                    TempExcelImageOPtion.ImageType = Aspose.Cells.Drawing.ImageType.Png;
                    break;
                case ".bmp":
                    TempWordImageSaveOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Bmp);
                    TempPdfImageDevice = new Aspose.Pdf.Devices.BmpDevice(TempPdfImageResolution);
                    TempExcelImageOPtion.ImageType = Aspose.Cells.Drawing.ImageType.Bmp;
                    break;
                case ".tif":
                    TempWordImageSaveOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Tiff);
                    TempPdfTifDevice = new Aspose.Pdf.Devices.TiffDevice(TempPdfImageResolution);
                    TempExcelImageOPtion.ImageType = Aspose.Cells.Drawing.ImageType.Tiff;
                    break;
                default: break;
            }

            //word转图片的设置
            TempWordImageSaveOption.UseHighQualityRendering = true;
            TempWordImageSaveOption.Resolution = DpiValue;
            TempWordImageSaveOption.ColorMode = Aspose.Words.Saving.ColorMode.Normal;
            TempWordImageSaveOption.UseAntiAliasing = true;
            TempWordImageSaveOption.AllowEmbeddingPostScriptFonts = true;

            //pdf转图片设置
            TempPdfImageDevice.RenderingOptions.DefaultFontName = "SimSun";

            //excel转图片的设置
            TempExcelImageOPtion.CheckWorkbookDefaultFont = true;
            TempExcelImageOPtion.HorizontalResolution = DpiValue;
            TempExcelImageOPtion.VerticalResolution = DpiValue;
            TempExcelImageOPtion.PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            TempExcelImageOPtion.CheckWorkbookDefaultFont = true;
            TempExcelImageOPtion.DefaultFont = "SimSun";

            if (PageExport)
            {
                if (FileType == ".doc" || FileType == ".docx")
                {
                    //逐页导出WORD图片
                    for (int pageIndex = 0; pageIndex < TempWordDocument.PageCount; pageIndex++)
                    {
                        string MutilPageFileName = SavedImageFileName.Replace(ConvertImageType, $"_第{pageIndex + 1}页{ConvertImageType}");
                        TempWordImageSaveOption.PageSet = new Aspose.Words.Saving.PageSet(pageIndex);
                        TempWordDocument.Save(MutilPageFileName, TempWordImageSaveOption);
                        if (SetGrayMode)
                        {
                            string SavedPath = Path.GetDirectoryName(SavedImageFileName);
                            string BMP_8BitPath = Path.Combine(SavedPath, "8_bmp_image");
                            Directory.CreateDirectory(BMP_8BitPath);
                            string ConvertedBmpFileName = BMP_8BitPath + "\\" + Path.GetFileName(MutilPageFileName).Replace(ConvertImageType, ".bmp");
                            using Bitmap TempBitMap = new Bitmap(MutilPageFileName);
                            using Bitmap ConvertedBitMap = ConvertTo256Color(TempBitMap);
                            ConvertedBitMap.Save(ConvertedBmpFileName, ImageFormat.Bmp);
                        }
                    }
                }
                else if (FileType == ".pdf")
                {
                    //逐页导出PDF的图片，tif图片需要单独处理，他不是imagedeivice类型
                    for (int PdfPageIndex = 0; PdfPageIndex < TempPdfDocument.Pages.Count; PdfPageIndex++)
                    {
                        string MutilPageFileName = SavedImageFileName.Replace(ConvertImageType, $"_第{PdfPageIndex + 1}页{ConvertImageType}");
                        if (ConvertImageType != ".tif")
                        {
                            TempPdfImageDevice.Process(TempPdfDocument.Pages[PdfPageIndex + 1], MutilPageFileName);
                        }
                        else
                        {
                            TempPdfTifDevice.Process(TempPdfDocument, PdfPageIndex + 1, PdfPageIndex + 1, MutilPageFileName);
                        }
                    }
                }
                else
                {
                    //对excel文件进行sheet表逐个导出
                    for (int ExcelSheetIndex = 0; ExcelSheetIndex < TempExcelDocument.Worksheets.Count; ExcelSheetIndex++)
                    {
                        Worksheet TempWorkSheet = TempExcelDocument.Worksheets[ExcelSheetIndex];
                        string TempSavedImageileWithSheetName = SavedImageFileName.Replace(ConvertImageType, $"_{TempWorkSheet.Name}{ConvertImageType}");
                        SheetRender TempSheetRender = new SheetRender(TempWorkSheet, TempExcelImageOPtion);
                        TempSheetRender.ToImage(0, TempSavedImageileWithSheetName);
                    }
                }
            }
            else//合并一页导出，只支持word文档
            {
                string TempSvgFile = SavedImageFileName.Replace(ConvertImageType, ".svg");
                if (FileType == ".doc" || FileType == ".docx")
                {
                    TempWordImageSaveOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Svg)
                    {
                        HorizontalResolution = DpiValue,
                        VerticalResolution = DpiValue
                    };
                    TempWordDocument.Save(TempSvgFile, TempWordImageSaveOption);
                    using Aspose.Imaging.Image TempLoadImage = Aspose.Imaging.Image.Load(TempSvgFile);
                    using SvgRasterizationOptions TempSvgOption = new SvgRasterizationOptions();
                    TempSvgOption.PageHeight = TempLoadImage.Height;
                    TempSvgOption.PageWidth = TempLoadImage.Width;
                    TempSvgOption.ResolutionSettings = new ResolutionSetting(DpiValue, DpiValue);
                    TempSvgOption.TextRenderingHint = TextRenderingHint.AntiAlias;
                    //using Aspose.Imaging.ImageOptions.VectorRasterizationOptions TempRasterOption = new VectorRasterizationOptions();
                    //TempRasterOption.PageWidth = TempLoadImage.Width;
                    //TempRasterOption.PageHeight = TempLoadImage.Height;
                    switch (ConvertImageType)
                    {
                        case ".jpg":
                            TempConvertJpegOption.Quality = 100;
                            TempConvertJpegOption.VectorRasterizationOptions = TempSvgOption;
                            TempConvertJpegOption.ResolutionSettings = new ResolutionSetting(DpiValue, DpiValue);
                            TempLoadImage.Save(SavedImageFileName, TempConvertJpegOption);
                            break;
                        case ".png":
                            TempConvertPngOption.VectorRasterizationOptions = TempSvgOption;
                            TempConvertPngOption.ResolutionSettings = new ResolutionSetting(DpiValue, DpiValue);
                            TempLoadImage.Save(SavedImageFileName, TempConvertPngOption);
                            break;
                        case ".bmp":
                            TempConvertBmpOption.VectorRasterizationOptions = TempSvgOption;
                            TempConvertBmpOption.ResolutionSettings = new ResolutionSetting(DpiValue, DpiValue);
                            TempLoadImage.Save(SavedImageFileName, TempConvertBmpOption);
                            break;
                        default:
                            break;
                    }
                }

            }



        }

        /// <summary>
        /// 替换文档的内容
        /// </summary>
        /// <param name="DocumentsFileName">文件名称</param>
        /// <param name="SavedDoucmentFileName">保存名称</param>
        /// <param name="RelaceDocumentType">文档类型</param>
        /// <param name="InputRegexRule">正则表达式规则</param>
        /// <param name="ReplaceContent">要替换的内容</param>
        /// <param name="IgnoreFootText">是否忽略脚注内容</param>
        /// <param name="IgnoreCase">是否忽略大小写</param>
        public static void RelaceDocumentContent(string DocumentsFileName, string SavedDoucmentFileName, string RelaceDocumentType, string InputRegexRule, string ReplaceContent, bool IgnoreFootText, bool NoIgnoreCase, bool RemoveRowMode = false, int ExcelStartRow = -1, int ExcelEndRow = -1, bool UseRegex = false, string replace_word_fontName = "SimSun", float replace_word_fontSize = 12, bool UseNewStyle = false)
        {
            Aspose.Hook.HookManager.StartHook();

            Regex DocuemntRegex = new Regex(InputRegexRule, RegexOptions.Multiline);
            if (RelaceDocumentType == "WORD")
            {
                Aspose.Words.Loading.LoadOptions DocuemntLoadOption = new Aspose.Words.Loading.LoadOptions()
                {
                    ConvertMetafilesToPng = true,
                    MswVersion = Aspose.Words.Settings.MsWordVersion.Word2010,
                };
                Aspose.Words.Saving.OoxmlSaveOptions DocumentSaveOption = new Aspose.Words.Saving.OoxmlSaveOptions()
                {
                    SaveFormat = Aspose.Words.SaveFormat.Docx,
                };

                Aspose.Words.Replacing.FindReplaceOptions TempWordFindReplaceOption = new Aspose.Words.Replacing.FindReplaceOptions();

                Aspose.Words.Document TempWordDocument = new Aspose.Words.Document(DocumentsFileName, DocuemntLoadOption);
                TempWordFindReplaceOption.IgnoreFootnotes = IgnoreFootText;
                TempWordFindReplaceOption.MatchCase = NoIgnoreCase;
                TempWordFindReplaceOption.UseSubstitutions = true;
                if (UseNewStyle)
                {
                    TempWordFindReplaceOption.ApplyFont.Name = replace_word_fontName;
                    TempWordFindReplaceOption.ApplyFont.Size = Convert.ToDouble(replace_word_fontSize);
                }
                TempWordDocument.Range.Replace(DocuemntRegex, ReplaceContent, TempWordFindReplaceOption);
                TempWordDocument.Save(SavedDoucmentFileName, DocumentSaveOption);
            }
            else
            {
                Aspose.Cells.Workbook TempExcelDocument = new Aspose.Cells.Workbook(DocumentsFileName);
                Aspose.Cells.ReplaceOptions TempExcelReplaceOptions = new Aspose.Cells.ReplaceOptions();
                Aspose.Cells.XlsSaveOptions TempXlsSaveOption = new XlsSaveOptions();
                TempExcelReplaceOptions.CaseSensitive = NoIgnoreCase;
                TempExcelReplaceOptions.RegexKey = UseRegex;
                if (InputRegexRule != string.Empty)
                {
                    TempExcelDocument.Replace(InputRegexRule, ReplaceContent, TempExcelReplaceOptions);
                }
                //不为默认值才进行行数据操作
                if (ExcelStartRow > -1 && ExcelEndRow > -1)
                {
                    foreach (Worksheet TempSheet in TempExcelDocument.Worksheets)
                    {
                        Aspose.Cells.Cells TempCells = TempSheet.Cells;
                        if (RemoveRowMode)
                        {
                            TempCells.DeleteRows(ExcelStartRow - 1, ExcelEndRow - 1);//删除行，excel行号从1开始，但是代码里面从0，所以输入什么数据都要-1，确保区间正确
                        }
                        else
                        {
                            TempCells.ClearRange(ExcelStartRow - 1, 0, ExcelEndRow - 1, 255);//清空行内数据，但是保留行高
                        }
                    }
                }
                TempExcelDocument.CalculateFormula();
                TempExcelDocument.Save(SavedDoucmentFileName, TempXlsSaveOption);
            }
        }


        /// <summary>
        /// 合并WORD功能
        /// </summary>
        /// <param name="DocumentsFileNames">文件列表</param>
        /// <param name="SavedDocumentFileName">合并后word的名称</param>
        public static void MergeWordFile(List<string> DocumentsFileNames, string SavedDocumentFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Words.Document MergeDocument = new Aspose.Words.Document();

            DocumentsFileNames.ForEach(DocumentName =>
            {
                Aspose.Words.Document LoadTempDocument = new Aspose.Words.Document(DocumentName);
                MergeDocument.AppendDocument(LoadTempDocument, Aspose.Words.ImportFormatMode.KeepSourceFormatting);
            });

            MergeDocument.FirstSection.Remove();
            MergeDocument.Save(SavedDocumentFileName);
        }

        /// <summary>
        /// 将Word转换为Excel，暂时只能转换单页的Word文档
        /// </summary>
        /// <param name="DocumentFileName">文档名称</param>
        /// <param name="SavedDcoumentFileName">保存后的文件名称</param>
        public static void ConvertWord2Excel(string DocumentFileName, string SavedDcoumentFileName)
        {
            Aspose.Hook.HookManager.StartHook();



            Aspose.Words.Document TempLoadDocument = new Aspose.Words.Document(DocumentFileName);

            Aspose.Words.Saving.XlsxSaveOptions TempSaveXlsOption = new Aspose.Words.Saving.XlsxSaveOptions
            {
                SaveFormat = Aspose.Words.SaveFormat.Xlsx,
                CompressionLevel = Aspose.Words.Saving.CompressionLevel.Maximum
            };

            TempLoadDocument.Save(SavedDcoumentFileName, TempSaveXlsOption);
        }
        /// <summary>
        /// 将Excel转换为Word，转换的格式不是太完美，等23.8版本出来，换控件以后会好很多
        /// </summary>
        /// <param name="DocumentFileName">文档名称</param>
        /// <param name="SaveDocumentFileName">转换后的文件名称</param>
        public static void ConvertExcel2Word(string DocumentFileName, string SaveDocumentFileName)
        {
            Aspose.Hook.HookManager.StartHook();
            using Aspose.Cells.Workbook TempLoadWorkBook = new Aspose.Cells.Workbook(DocumentFileName);
            Aspose.Cells.DocxSaveOptions TempDocxSaveOption = new DocxSaveOptions
            {
                AllColumnsInOnePagePerSheet = false,
                CheckWorkbookDefaultFont = true,
                MergeAreas = true,
                ValidateMergedAreas = true,
                GridlineType = GridlineType.Hair,
                TextCrossType = TextCrossType.Default,
                OnePagePerSheet = false,
            };
            TempLoadWorkBook.Save(SaveDocumentFileName, TempDocxSaveOption);
        }

        /// <summary>
        /// 拆分PDF
        /// </summary>
        /// <param name="DocumentFileName">文档名称</param>
        /// <param name="SavedPath">保存目录</param>
        public static void SplitPdf(string DocumentFileName, string SavedPath)
        {
            Aspose.Hook.HookManager.StartHook();

            using Aspose.Pdf.Document TempLoadDocument = new Aspose.Pdf.Document(DocumentFileName);

            int PdfPageNumber = 1;

            foreach (Aspose.Pdf.Page TempPage in TempLoadDocument.Pages)
            {
                string SavedFileName = $"{Path.Combine(SavedPath, Path.GetFileNameWithoutExtension(DocumentFileName) + $"_第{PdfPageNumber}页.pdf")}";
                Aspose.Pdf.Document ExportTempDocument = new Aspose.Pdf.Document();
                ExportTempDocument.Pages.Add(TempPage);
                ExportTempDocument.Save(SavedFileName);
                PdfPageNumber++;
            }
        }

        public static void SplitWord(string DocumentFileName, string SavedPath)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Words.Document TempDocument = new Aspose.Words.Document(DocumentFileName);

            Aspose.Words.Saving.OoxmlSaveOptions TempSaveOption = new Aspose.Words.Saving.OoxmlSaveOptions
            {
                Compliance = Aspose.Words.Saving.OoxmlCompliance.Iso29500_2008_Transitional,
            };
            for (int pageIndex = 0; pageIndex < TempDocument.PageCount; pageIndex++)
            {
                string SavedFileName = $"{Path.Combine(SavedPath, Path.GetFileNameWithoutExtension(DocumentFileName))}_第{pageIndex + 1}页.docx";
                Aspose.Words.Document TempSplitDocument = TempDocument.ExtractPages(pageIndex, 1);
                TempSplitDocument.CompatibilityOptions.OptimizeFor(Aspose.Words.Settings.MsWordVersion.Word2010);
                TempSplitDocument.CompatibilityOptions.BalanceSingleByteDoubleByteWidth = true;
                TempSplitDocument.Save(SavedFileName, TempSaveOption);
            }
        }

        /// <summary>
        /// 为pdf添加水印
        /// </summary>
        /// <param name="DocuementFileName">文档名称</param>
        /// <param name="SaveFileName">保存后的名称</param>
        /// <param name="WaterMarkContent">水印文本</param>
        /// <param name="WaterMarkOpactiyNum">不透明度</param>
        /// <param name="WaterMarkRotateDegNum">旋转角度</param>
        /// <param name="WaterMarkHorAlign">水平对齐方式</param>
        /// <param name="WaterMarkVerAlign">垂直对齐方式</param>
        /// <param name="WaterMarkFontColor">字体颜色</param>
        /// <param name="WaterMarkFontName">字体名称</param>
        /// <param name="WaterMarkFontSize">字体大小</param>
        public static void AddWaterMark2Pdf(string DocuementFileName, string SaveFileName, string WaterMarkContent, float WaterMarkOpactiyNum, string WaterMarkRotateDegNum, int WaterMarkHorAlignIndex, int WaterMarkVerAlignIndex, System.Drawing.Color WaterMarkFontColor, string WaterMarkFontName = "宋体", float WaterMarkFontSize = 12, string AddImageFileName = "", string AddImageHeight = "100", string AddImageWidth = "100")
        {
            Aspose.Hook.HookManager.StartHook();


            using Aspose.Pdf.Document TempLoadDocument = new Aspose.Pdf.Document(DocuementFileName);
            foreach (Aspose.Pdf.Page TempPage in TempLoadDocument.Pages)
            {
                if (AddImageFileName != string.Empty && WaterMarkContent == string.Empty)
                {
                    ImageStamp TempAddArtifact = new ImageStamp(AddImageFileName);

                    //通过获取combox索引来设置对齐方式
                    switch (WaterMarkHorAlignIndex)
                    {
                        case 0:
                            TempAddArtifact.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.None;
                            break;
                        case 1:
                            TempAddArtifact.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Left;
                            break;
                        case 2:
                            TempAddArtifact.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Center;
                            break;
                        case 3:
                            TempAddArtifact.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Right;
                            break;
                        case 4:
                            TempAddArtifact.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Justify;
                            break;
                        case 5:
                            TempAddArtifact.HorizontalAlignment = Aspose.Pdf.HorizontalAlignment.FullJustify;
                            break;
                        default:
                            break;

                    }

                    switch (WaterMarkVerAlignIndex)
                    {
                        case 0:
                            TempAddArtifact.VerticalAlignment = Aspose.Pdf.VerticalAlignment.None;
                            break;
                        case 1:
                            TempAddArtifact.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Top;
                            break;
                        case 2:
                            TempAddArtifact.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Center;
                            break;
                        case 3:
                            TempAddArtifact.VerticalAlignment = Aspose.Pdf.VerticalAlignment.Bottom;
                            break;
                        default:
                            break;
                    }

                    TempAddArtifact.Opacity = WaterMarkOpactiyNum;
                    TempAddArtifact.RotateAngle = Convert.ToInt32(WaterMarkRotateDegNum);
                    TempAddArtifact.XIndent = 100;
                    TempAddArtifact.YIndent = 100;
                    TempAddArtifact.Width = double.Parse(AddImageWidth);
                    TempAddArtifact.Height = double.Parse(AddImageHeight);
                    TempPage.AddStamp(TempAddArtifact);
                }
                //没有图片就用文字水印
                else
                {
                    WatermarkArtifact TempAddArtifact = new WatermarkArtifact();

                    TextState TempTextState = new TextState
                    {
                        Font = FontRepository.FindFont(WaterMarkFontName),
                        FontSize = WaterMarkFontSize,
                        ForegroundColor = Aspose.Pdf.Color.FromArgb(WaterMarkFontColor.R, WaterMarkFontColor.G, WaterMarkFontColor.B)
                    };

                    TempAddArtifact.SetTextAndState(
                        WaterMarkContent, TempTextState
                        );
                    //通过获取combox索引来设置对齐方式
                    switch (WaterMarkHorAlignIndex)
                    {
                        case 0:
                            TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.None;
                            break;
                        case 1:
                            TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Left;
                            break;
                        case 2:
                            TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Center;
                            break;
                        case 3:
                            TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Right;
                            break;
                        case 4:
                            TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Justify;
                            break;
                        case 5:
                            TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.FullJustify;
                            break;
                        default:
                            break;

                    }

                    switch (WaterMarkVerAlignIndex)
                    {
                        case 0:
                            TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.None;
                            break;
                        case 1:
                            TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.Top;
                            break;
                        case 2:
                            TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.Center;
                            break;
                        case 3:
                            TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.Bottom;
                            break;
                        default:
                            break;
                    }

                    TempAddArtifact.Rotation = Convert.ToInt32(WaterMarkRotateDegNum);
                    TempAddArtifact.Opacity = WaterMarkOpactiyNum;
                    TempAddArtifact.IsBackground = false;
                    TempPage.Artifacts.Add(TempAddArtifact);
                }


            }

            TempLoadDocument.Save(SaveFileName, Aspose.Pdf.SaveFormat.Pdf);

        }

        /// <summary>
        /// 为pdf添加水印
        /// </summary>
        /// <param name="DocuementFileName">文档名称</param>
        /// <param name="SaveFileName">保存后的名称</param>
        /// <param name="WaterMarkOpactiyNum">不透明度</param>
        /// <param name="WaterMarkRotateDegNum">旋转角度</param>
        [Obsolete]
        public static void AddWaterMark2Pdf(string DocuementFileName, string SaveFileName, float WaterMarkOpactiyNum, string WaterMarkRotateDegNum, int WaterMarkHorAlignIndex, int WaterMarkVerAlignIndex, string AddBackGroundImageFileName)
        {
            Aspose.Hook.HookManager.StartHook();
            using Document TempLoadDocument = new Aspose.Pdf.Document(DocuementFileName);
            foreach (Aspose.Pdf.Page TempPage in TempLoadDocument.Pages)
            {
                BackgroundArtifact TempAddArtifact = new BackgroundArtifact
                {
                    BackgroundImage = File.OpenRead(AddBackGroundImageFileName)
                };

                //通过获取combox索引来设置对齐方式
                switch (WaterMarkHorAlignIndex)
                {
                    case 0:
                        TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.None;
                        break;
                    case 1:
                        TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Left;
                        break;
                    case 2:
                        TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Center;
                        break;
                    case 3:
                        TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Right;
                        break;
                    case 4:
                        TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.Justify;
                        break;
                    case 5:
                        TempAddArtifact.ArtifactHorizontalAlignment = Aspose.Pdf.HorizontalAlignment.FullJustify;
                        break;
                    default:
                        break;

                }

                switch (WaterMarkVerAlignIndex)
                {
                    case 0:
                        TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.None;
                        break;
                    case 1:
                        TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.Top;
                        break;
                    case 2:
                        TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.Center;
                        break;
                    case 3:
                        TempAddArtifact.ArtifactVerticalAlignment = Aspose.Pdf.VerticalAlignment.Bottom;
                        break;
                    default:
                        break;
                }

                TempAddArtifact.Rotation = Convert.ToInt32(WaterMarkRotateDegNum);
                TempAddArtifact.Opacity = WaterMarkOpactiyNum;
                TempPage.Artifacts.Add(TempAddArtifact);

                TempAddArtifact.Dispose();
            }

            TempLoadDocument.Save(SaveFileName, Aspose.Pdf.SaveFormat.Pdf);
        }


        /// <summary>
        /// 移除pdf水印
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void RemovePdfWaterMark(string DocumentFileName, string SavedFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Pdf.Document TempLoadDocument = new Aspose.Pdf.Document(DocumentFileName);

            foreach (Aspose.Pdf.Page TempPage in TempLoadDocument.Pages)
            {
                foreach (Aspose.Pdf.Artifact TempWaterMark in TempPage.Artifacts)
                {
                    Debug.WriteLine(TempWaterMark.Subtype);
                    if (TempWaterMark.Subtype == Artifact.ArtifactSubtype.Watermark || TempWaterMark.Subtype == Artifact.ArtifactSubtype.Background)
                    {
                        TempPage.Artifacts.Delete(TempWaterMark);
                    }
                }
            }
            TempLoadDocument.Save(SavedFileName);

        }

        /// <summary>
        /// ppt转pdf
        /// </summary>
        /// <param name="DocumentFileName">文档名称</param>
        /// <param name="SavedFileName"></param>
        /// <param name="DrawBlackLine">是否画边框</param>
        /// <param name="ProtectedPassword">文档密码</param>
        /// <param name="ExportHiddenPage">是否导出隐藏幻灯片</param>
        /// <param name="Position">备注页是否导出</param>
        public static void ConvertPPT2PDF(string DocumentFileName, string SavedFileName, bool DrawBlackLine, string ProtectedPassword, bool ExportHiddenPage, Aspose.Slides.Export.NotesPositions Position)
        {
            Aspose.Hook.HookManager.StartHook();

            using Aspose.Slides.Presentation TempDocument = new Aspose.Slides.Presentation(DocumentFileName);
            Aspose.Slides.Export.PdfOptions TempPdfOption = new Aspose.Slides.Export.PdfOptions()
            {
                DrawSlidesFrame = DrawBlackLine,
                JpegQuality = 100,
                ShowHiddenSlides = ExportHiddenPage,
                Password = ProtectedPassword
            };

            INotesCommentsLayoutingOptions layoutingOptions = TempPdfOption.NotesCommentsLayouting;
            layoutingOptions.NotesPosition = Position;

            TempDocument.Save(SavedFileName, Aspose.Slides.Export.SaveFormat.Pdf, TempPdfOption);
        }
        /// <summary>
        /// 将pdf转为ppt
        /// </summary>
        /// <param name="DocumnentFileName">文件名称</param>
        /// <param name="SavedFileName">保存后文件名称</param>
        /// <param name="ExtraOcr">是否开启ocr识别</param>
        /// <param name="SplitImageShape">是否分割图形图像</param>
        /// <param name="RenderAsImage">是否导出为图片</param>
        public static void ConvertPDF2PPT(string DocumnentFileName, string SavedFileName, bool ExtraOcr, bool SplitImageShape, bool RenderAsImage)
        {
            Aspose.Hook.HookManager.StartHook();
            using Aspose.Pdf.Document TempDocument = new Aspose.Pdf.Document(DocumnentFileName);
            Aspose.Pdf.PptxSaveOptions TempPPTOption = new Aspose.Pdf.PptxSaveOptions
            {
                ExtractOcrSublayerOnly = ExtraOcr,
                SeparateImages = SplitImageShape,
                SlidesAsImages = RenderAsImage,
            };
            TempDocument.Save(SavedFileName, TempPPTOption);

        }

        /// <summary>
        /// 转换PDF版本
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ConvertFormat"></param>
        public static void ConvertPdfCompliance(string DocumentFileName, string SavedFileName, Aspose.Pdf.PdfFormat ConvertFormat)
        {
            Aspose.Hook.HookManager.StartHook();

            using Aspose.Pdf.Document TempDocument = new Aspose.Pdf.Document(DocumentFileName);
            using MemoryStream TempMemoryStream = new MemoryStream();
            TempDocument.Convert(TempMemoryStream, ConvertFormat, ConvertErrorAction.Delete);
            TempDocument.Save(SavedFileName);

        }

        /// <summary>
        /// 压缩PDF
        /// </summary>
        /// <param name="DocumentFileName">文档名称</param>
        /// <param name="SavedFileName">保存后的文档名称</param>
        /// <param name="CompressPDF_WEB">是否进行web优化</param>
        /// <param name="CommpressPDF_CompressImage">是否压缩图片</param>
        /// <param name="CompressPDF_CompressImageQuailty">压缩的图片质量</param>
        /// <param name="CompressPDF_ImageMaxDPI">如果数值大于0，则修改图像的最大DPI值</param>
        /// <param name="CompressPDF_CompressImageVersion">压缩的算法</param>
        /// <param name="CompressPDF_UseFlateCode">是否使用flate编码</param>
        /// <param name="CompressPDF_RemoveUnUseStream">是否移除未使用的流</param>
        /// <param name="CompressPDF_LinkeSameStream">是否链接重复流</param>
        /// <param name="CompressPDF_CancelEmbedFont">是否取消字体内嵌</param>
        /// <param name="CompressPDF_RemoveForms">是否移除表单域</param>
        /// <param name="CompressPDF_RemoveUnUseObject">是否移除未使用的对象</param>
        public static void CompressPDF(string DocumentFileName, string SavedFileName,
                                        bool CompressPDF_WEB, bool CommpressPDF_CompressImage,
                                        int CompressPDF_CompressImageQuailty, int CompressPDF_ImageMaxDPI, Aspose.Pdf.Optimization.ImageCompressionVersion CompressPDF_CompressImageVersion,
                                        bool CompressPDF_UseFlateCode, bool CompressPDF_RemoveUnUseStream,
                                        bool CompressPDF_LinkeSameStream, bool CompressPDF_CancelEmbedFont,
                                        bool CompressPDF_RemoveForms, bool CompressPDF_RemoveUnUseObject)
        {

            Aspose.Hook.HookManager.StartHook();
            using Aspose.Pdf.Document TempLoadDocument = new Document(DocumentFileName);
            Aspose.Pdf.Optimization.OptimizationOptions TempComressOption = new Aspose.Pdf.Optimization.OptimizationOptions();

            if (CompressPDF_WEB)//如果勾选了WEB优化，就执行自动优化
            {
                TempLoadDocument.Optimize();
            }

            if (CommpressPDF_CompressImage)//勾选了压缩图片，则图片质量设置就生效
            {
                TempComressOption.ImageCompressionOptions.CompressImages = true;
                TempComressOption.ImageCompressionOptions.ImageQuality = CompressPDF_CompressImageQuailty;
            }

            if (CompressPDF_ImageMaxDPI > 0)//如果设置DPI大于0，则修改DPI生效
            {
                TempComressOption.ImageCompressionOptions.ResizeImages = true;
                TempComressOption.ImageCompressionOptions.MaxResolution = CompressPDF_ImageMaxDPI;
            }

            if (CompressPDF_UseFlateCode)//如果勾选flate编码，则生效
            {
                TempComressOption.ImageCompressionOptions.Encoding = Aspose.Pdf.Optimization.ImageEncoding.Flate;
            }

            if (CompressPDF_RemoveForms)//如果勾选了移除表单域，则进行移除
            {
                foreach (Page TempPage in TempLoadDocument.Pages)
                {
                    foreach (Annotation annotation in TempPage.Annotations)
                    {
                        annotation.Flatten();
                    }
                }
            }

            TempComressOption.ImageCompressionOptions.Version = CompressPDF_CompressImageVersion;
            TempComressOption.RemoveUnusedStreams = CompressPDF_RemoveUnUseStream;
            TempComressOption.LinkDuplcateStreams = CompressPDF_LinkeSameStream;
            TempComressOption.RemoveUnusedObjects = CompressPDF_RemoveUnUseObject;
            TempComressOption.UnembedFonts = CompressPDF_CancelEmbedFont;

            TempLoadDocument.OptimizeResources(TempComressOption);//设置优化参数，此时生效


            TempLoadDocument.Save(SavedFileName);
        }

        /// <summary>
        /// 压缩图片
        /// </summary>
        /// <param name="ImageFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="Compress_Jpeg_Mode"></param>
        /// <param name="Compress_Jpeg_ColorMode"></param>
        /// <param name="Compress_Jpeg_Quailty"></param>
        /// <param name="Compress_Png_Level"></param>
        /// <param name="Compress_Png_ColorType"></param>
        /// <param name="Compress_Bmp_Compression"></param>
        /// <param name="Compress_Bmp_Bits_Pixel"></param>
        public static void CompressImage(string ImageFileName, string SavedFileName,
                                        Aspose.Imaging.FileFormats.Jpeg.JpegCompressionMode Compress_Jpeg_Mode,
                                        Aspose.Imaging.FileFormats.Jpeg.JpegCompressionColorMode Compress_Jpeg_ColorMode,
                                        int Compress_Jpeg_Quailty,
                                        int Compress_Png_Level, Aspose.Imaging.FileFormats.Png.PngColorType Compress_Png_ColorType,
                                        Aspose.Imaging.FileFormats.Bmp.BitmapCompression Compress_Bmp_Compression, int Compress_Bmp_Bits_Pixel
                                        )
        {
            Aspose.Hook.HookManager.StartHook();

            //支持的图像格式：.jpg、.jpeg、.png、.bmp

            using Aspose.Imaging.Image TempLoadImage = Aspose.Imaging.Image.Load(ImageFileName);

            string ImageExtensionString = Path.GetExtension(ImageFileName);//返回格式: .png

            Aspose.Imaging.ImageOptions.JpegOptions TempJpgOption = new Aspose.Imaging.ImageOptions.JpegOptions();

            Aspose.Imaging.ImageOptions.PngOptions TempPngOption = new Aspose.Imaging.ImageOptions.PngOptions();

            Aspose.Imaging.ImageOptions.BmpOptions TempBmpOption = new Aspose.Imaging.ImageOptions.BmpOptions();

            switch (ImageExtensionString)
            {
                case ".jpg":
                    TempJpgOption.CompressionType = Compress_Jpeg_Mode;
                    TempJpgOption.ColorType = Compress_Jpeg_ColorMode;
                    TempJpgOption.Quality = Compress_Jpeg_Quailty;
                    //TempJpgOption.BitsPerChannel = 8;
                    //TempJpgOption.Palette = Aspose.Imaging.ColorPaletteHelper.GetCloseImagePalette((Aspose.Imaging.RasterImage)TempLoadImage, 256, Aspose.Imaging.PaletteMiningMethod.Histogram);
                    break;
                case ".jpeg":
                    TempJpgOption.CompressionType = Compress_Jpeg_Mode;
                    TempJpgOption.ColorType = Compress_Jpeg_ColorMode;
                    TempJpgOption.Quality = Compress_Jpeg_Quailty;
                    //TempJpgOption.BitsPerChannel = 8;
                    break;
                case ".png":
                    TempPngOption.CompressionLevel = Compress_Png_Level;
                    TempPngOption.ColorType = Compress_Png_ColorType;
                    break;
                case ".bmp":
                    TempBmpOption.Compression = Compress_Bmp_Compression;
                    TempBmpOption.BitsPerPixel = Compress_Bmp_Bits_Pixel;
                    if (Compress_Bmp_Compression == Aspose.Imaging.FileFormats.Bmp.BitmapCompression.Rle4 &&
                        Compress_Bmp_Bits_Pixel == 4)
                    {
                        TempBmpOption.Palette = ColorPaletteHelper.Create4Bit();
                    }
                    else if (Compress_Bmp_Compression == Aspose.Imaging.FileFormats.Bmp.BitmapCompression.Rle8 &&
                        Compress_Bmp_Bits_Pixel == 8)
                    {
                        TempBmpOption.Palette = ColorPaletteHelper.Create8Bit();
                    }
                    else
                    {
                        TempBmpOption.Palette = ColorPaletteHelper.CreateMonochrome();
                    }
                    break;
                default:
                    break;
            }

            switch (Path.GetExtension(SavedFileName))
            {
                case ".jpg":
                    TempLoadImage.Save(SavedFileName, TempJpgOption);
                    break;
                case ".jpeg":
                    TempLoadImage.Save(SavedFileName, TempJpgOption);
                    break;
                case ".png":
                    TempLoadImage.Save(SavedFileName, TempPngOption);
                    break;
                case ".bmp":
                    TempLoadImage.Save(SavedFileName, TempBmpOption);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 读取文件中的DataMatrix，交给treeview处理，功能写的差不多了，随便写写了
        /// </summary>
        /// <param name="DocumentFileName">文档名称</param>
        /// <param name="TempTreeView">控件</param>
        public static void ReadDocumentDataMatrix(string DocumentFileName, TreeView TempTreeView)
        {
            Aspose.Hook.HookManager.StartHook();
            TreeNode TempTreeNode = new TreeNode();
            TreeNode TempDocumentNameRootNode = TempTreeNode.Nodes.Add(DocumentFileName);//添加文件节点信息
            Aspose.Words.Document TempDocuemnt = new Aspose.Words.Document(DocumentFileName);
            for (int PageIndex = 0; PageIndex < TempDocuemnt.PageCount; PageIndex++)
            {
                TreeNode PageIndexNode = TempDocumentNameRootNode.Nodes.Add($"第{PageIndex + 1}页");//在文件节点信息基础上添加页面信息
                PageIndexNode.ForeColor = System.Drawing.Color.ForestGreen;
                Aspose.Words.Saving.ImageSaveOptions TempImageSaveOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Png)
                {
                    PageSet = new Aspose.Words.Saving.PageSet(PageIndex),
                    Resolution = 300
                };
                using MemoryStream TempMemoryStream = new MemoryStream();
                TempDocuemnt.Save(TempMemoryStream, TempImageSaveOption);
                TempMemoryStream.Position = 0;

                using BarCodeReader TempCodeReader = new BarCodeReader(TempMemoryStream, DecodeType.DataMatrix);
                foreach (BarCodeResult TempResult in TempCodeReader.ReadBarCodes())
                {
                    string TempText = TempResult.CodeText;
                    string TableType = string.Empty;
                    string CurrentDataMatrixNumber = string.Empty;
                    string SignleNumber = string.Empty;
                    System.Drawing.Color TipsColor = System.Drawing.Color.White;
                    TreeNode TableTypeNode, PageMatrixNumberNode, SignleNumberNode;
                    if (TempText == "A01")
                    {
                        TableType = "测评";
                    }
                    else if (TempText == "B")
                    {
                        TableType = "选举/推荐";
                    }
                    else if (TempText.StartsWith("T"))
                    {
                        CurrentDataMatrixNumber = TempText.Replace("T", "");
                    }
                    else
                    {
                        SignleNumber = TempText;
                    }
                    //页面信息基础上添加更深层信息
                    if (TableType != string.Empty)
                    {
                        TableTypeNode = PageIndexNode.Nodes.Add("表格类型");

                        TableTypeNode.Nodes.Add(TableType).ForeColor = System.Drawing.Color.Red;
                    }
                    if (CurrentDataMatrixNumber != string.Empty)
                    {
                        PageMatrixNumberNode = PageIndexNode.Nodes.Add("页面编码");
                        PageMatrixNumberNode.Nodes.Add(CurrentDataMatrixNumber).ForeColor = System.Drawing.Color.Red;
                    }
                    if (SignleNumber != string.Empty)
                    {
                        SignleNumberNode = PageIndexNode.Nodes.Add("单人码编码");
                        SignleNumberNode.Nodes.Add(SignleNumber).ForeColor = System.Drawing.Color.Red;
                    }


                    /**
                     * 测评单位码 A01 测评页码 T11
                     * 选举单位码 B   选举页码 T11
                     * 推荐单位吗 B   推荐页码 T10
                     * **/
                }
            }
            TempTreeView.Invoke(new Action(() => { TempTreeView.Nodes.Add(TempDocumentNameRootNode); }));

        }


        /// <summary>
        /// 通过正则表达式移除指定的字符
        /// </summary>
        /// <param name="SpecialCharacters"></param>
        /// <param name="InputText"></param>
        /// <returns></returns>
        private static string RemoveSpeciCharacter(string[] SpecialCharacters, string InputText)
        {
            foreach (string SpecialCharacter in SpecialCharacters)
            {
                bool MatchStatus = Regex.Match(InputText, SpecialCharacter, RegexOptions.Multiline).Success;
                if (MatchStatus)
                {
                    InputText = Regex.Replace(InputText, SpecialCharacter, "", RegexOptions.Multiline);
                }
            }
            return InputText;
        }

        /// <summary>
        /// 正则匹配结果，如果匹配到了就返回true
        /// </summary>
        /// <param name="MachMode">表达式</param>
        /// <param name="InputText">输入的文本</param>
        /// <returns></returns>
        private static bool MatchRegexStatus(string MachMode, string InputText)
        {
            return Regex.Match(InputText, MachMode).Success;
        }

        /// <summary>
        /// 添加处理后的姓名到列表中
        /// </summary>
        /// <param name="InputText">源文本</param>
        /// <param name="DoubleTextAligin">是否双字对齐</param>
        /// <param name="RemoveNumber">是否移除数字</param>
        /// <param name="RemoveSquare">是否移除括号</param>
        /// <returns></returns>
        public static string[] AddText2List(string InputText, bool RemoveNumber, bool RemoveSquare, bool NameStorke, bool NameReplace, string SearchText, string ReplaceText)
        {
            List<string> TextList = new List<string>();

            string[] ReturnTextList;

            if (MatchRegexStatus("\\t", InputText))//表示匹配到的是传统word表格中直接复制的内容
            {
                InputText = Regex.Replace(InputText, "\t", "\n", RegexOptions.Multiline);//将\t全部换成\n换行符

                string[] TempInputTextByTap = InputText.Split(new char[] { '\n' });//使用\n来分割数组，确保每个文字都独立存储

                string[] AllSpaceChar = new string[] { "\\s", "○", "□", "£" };//设置基础特殊字符列表

                foreach (string TempText in TempInputTextByTap)//遍历分割数组，得到每个文字
                {
                    string TempInputTextNoSpace = RemoveSpeciCharacter(AllSpaceChar, TempText);//对每个文字进行删除特殊字符处理

                    if (RemoveNumber)
                    {
                        TempInputTextNoSpace = RemoveSpeciCharacter(new string[] { "[0-9]" }, TempInputTextNoSpace);//移除数字
                    }

                    if (RemoveSquare)
                    {
                        //删除括号
                        TempInputTextNoSpace = RemoveSpeciCharacter(new string[] { "\\(.+?\\)|\\（.+?\\）|\\(\\)|\\（\\）" }, TempInputTextNoSpace);
                    }

                    if (NameReplace && SearchText != string.Empty)
                    {
                        if (BodyForm.MainForm.NameReplaceMode)
                        {
                            TempInputTextNoSpace = Regex.Replace(TempInputTextNoSpace, SearchText, ReplaceText, RegexOptions.Multiline);
                        }
                        else
                        {
                            TempInputTextNoSpace = TempInputTextNoSpace.Replace(SearchText, ReplaceText);
                        }
                    }

                    if (TempInputTextNoSpace != string.Empty)
                    {
                        //只有非空白文本才添加到数组中
                        TextList.Add(TempInputTextNoSpace);
                    }
                }
            }
            else
            {
                //以下是对常规列文本进行普通处理
                string[] TempNewLineText = InputText.Split(new char[] { '\n' });
                foreach (string TempText in TempNewLineText)
                {

                    if (TempText != string.Empty)
                    {
                        string AddListText = RemoveSpeciCharacter(new string[] { "\\s" }, TempText);

                        if (RemoveNumber)
                        {
                            AddListText = RemoveSpeciCharacter(new string[] { "[0-9]" }, AddListText);//移除数字
                        }

                        if (RemoveSquare)
                        {
                            //删除括号
                            AddListText = RemoveSpeciCharacter(new string[] { "\\(.+?\\)|\\（.+?\\）|\\(\\)|\\（\\）" }, AddListText);
                        }

                        if (NameReplace && SearchText != string.Empty)
                        {
                            if (BodyForm.MainForm.NameReplaceMode)
                            {
                                AddListText = Regex.Replace(AddListText, SearchText, ReplaceText, RegexOptions.Multiline);
                            }
                            else
                            {
                                AddListText = AddListText.Replace(SearchText, ReplaceText);
                            }
                        }

                        if (AddListText != string.Empty)
                        {
                            //只有非空白文本才添加到数组中
                            TextList.Add(AddListText);
                        }
                    }
                }
            }

            ReturnTextList = TextList.ToArray();

            if (NameStorke)
            {
                //按照姓氏笔画排序
                CultureInfo cultureInfo = new CultureInfo(133124);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Array.Sort(ReturnTextList);
            }


            return ReturnTextList;

        }

        /// <summary>
        /// 获取程序使用掉的内存，单位MB
        /// </summary>
        /// <returns></returns>
        public static string GetProcessUsedMemory()
        {
            ComputerInfo computerInfo = new ComputerInfo();
            double MachineTotalMemory = Byte2GB((long)computerInfo.TotalPhysicalMemory);
            double MachineFreeMemory = Byte2GB((long)computerInfo.AvailablePhysicalMemory);
            double ProcessUsedMemory = Process.GetCurrentProcess().PrivateMemorySize64 / 1024.0 / 1024.0;

            return $"程序占用内存：{ProcessUsedMemory:f2}MB|本机可用内存：{MachineFreeMemory:f2}GB|本机总内存：{MachineTotalMemory:f2}GB";
        }

        /// <summary>
        /// 将字节转为GB
        /// </summary>
        /// <param name="byteNumber"></param>
        /// <returns></returns>
        private static double Byte2GB(long byteNumber)
        {

            return byteNumber / Math.Pow(1024.0, 3);
        }

        /// <summary>
        /// 通过HTTPCLIENT来下载文件，并展示下载进度
        /// </summary>
        /// <param name="FileUrl">下载链接</param>
        /// <param name="SavedFileName">保存的文件名称</param>
        /// <param name="DownloadProgressBar">使用的进度条</param>
        /// <param name="DownloadStatusLabel">使用的提示文本标签</param>
        public static void DwonloadFile(string FileUrl, string SavedFileName, ProgressBar DownloadProgressBar, Label DownloadStatusLabel)
        {
            DownloadProgressBar.Maximum = 100;
            WebClient TempDownloadClient = new WebClient();
            Uri TempDownloadUrl = new Uri(FileUrl);
            TempDownloadClient.DownloadFileAsync(TempDownloadUrl, SavedFileName);
            TempDownloadClient.DownloadProgressChanged += (download_sender, download_event) =>
            {
                int CurrentPercent = download_event.ProgressPercentage;
                DownloadProgressBar.Value = CurrentPercent;
                DownloadStatusLabel.Text = $"下载进度：{CurrentPercent}%";
            };
            TempDownloadClient.DownloadFileCompleted += (download_sender, download_event) =>
            {
                DownloadStatusLabel.Text = $"下载完毕，文件名称为：{SavedFileName}";
                DownloadProgressBar.Value = 0;
                MessageBox.Show($"下载完毕，文件在软件同目录下：{SavedFileName}", "下载文件", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                DownloadStatusLabel.Text = "下载进度：0%";
            };

        }

        /// <summary>
        /// 图片转ICO
        /// </summary>
        /// <param name="ImageFileName"></param>
        /// <param name="SaveFileName"></param>
        /// <param name="icoSize"></param>
        public static void ConvertImage2Ico(string ImageFileName, string SaveFileName, int icoSize)
        {

            Aspose.Hook.HookManager.StartHook();

            using Aspose.Imaging.Image TempLoadImage = Aspose.Imaging.Image.Load(ImageFileName);
            Aspose.Imaging.ImageOptions.IcoOptions TempIcoOption = new Aspose.Imaging.ImageOptions.IcoOptions
            {
                BitsPerPixel = 32,
                VectorRasterizationOptions = new Aspose.Imaging.ImageOptions.VectorRasterizationOptions
                {
                    PageHeight = icoSize,
                    PageWidth = icoSize,
                },

            };

            TempLoadImage.Save(SaveFileName, TempIcoOption);
        }


        /// <summary>
        /// html转PDF，浏览器渲染
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        /// <param name="ChromeDriverPath">chrome驱动路径</param>
        /// <param name="PdfPaperSize">纸张尺寸</param>
        /// <param name="isLand">是否横向</param>
        /// <param name="isUseCss">是否使用css尺寸设置</param>
        /// <param name="TopPx">上边距</param>
        /// <param name="LeftPx">左边距</param>
        /// <param name="BottomPx">下边距</param>
        /// <param name="RightPx">右边距</param>
        public static async void ConvertHtml2PDFAsync(string DocumentFileName, string SavedFileName, string ChromeDriverPath, int PdfPaperSizeIndex, bool isLand, bool isUseCss, string TopPx, string LeftPx, string BottomPx, string RightPx,
            decimal DefaultTimeOut, decimal WaitTimeOut,bool WaitAllResouce)
        {

            if (ChromeDriverPath != string.Empty && ChromeDriverPath != null && File.Exists(ChromeDriverPath))
            {
                PuppeteerSharp.LaunchOptions LanuachOption = new PuppeteerSharp.LaunchOptions
                {
                    Headless = true,
                    IgnoreHTTPSErrors = true,
                    ExecutablePath = ChromeDriverPath,
                };


                PuppeteerSharp.PdfOptions TempPdfOption = new PuppeteerSharp.PdfOptions();

                using PuppeteerSharp.IBrowser TempBroswer = await PuppeteerSharp.Puppeteer.LaunchAsync(LanuachOption);
                

                using PuppeteerSharp.IPage TempPage = await TempBroswer.NewPageAsync();

                if (WaitAllResouce)
                {
                    await TempPage.WaitForFrameAsync(TempPage.Url);
                }
                else
                {
                    TempPage.DefaultTimeout = (int)DefaultTimeOut * 1000;
                    TempPage.DefaultNavigationTimeout = (int)WaitTimeOut * 1000;
                }
                await TempPage.GoToAsync(DocumentFileName);


                //根据索引选择纸张大小
                TempPdfOption.Format = PdfPaperSizeIndex switch
                {
                    0 => PuppeteerSharp.Media.PaperFormat.A0,
                    1 => PuppeteerSharp.Media.PaperFormat.A1,
                    2 => PuppeteerSharp.Media.PaperFormat.A2,
                    3 => PuppeteerSharp.Media.PaperFormat.A3,
                    4 => PuppeteerSharp.Media.PaperFormat.A4,
                    5 => PuppeteerSharp.Media.PaperFormat.A5,
                    6 => PuppeteerSharp.Media.PaperFormat.A6,
                    7 => PuppeteerSharp.Media.PaperFormat.Ledger,
                    8 => PuppeteerSharp.Media.PaperFormat.Legal,
                    9 => PuppeteerSharp.Media.PaperFormat.Letter,
                    10 => PuppeteerSharp.Media.PaperFormat.Tabloid,
                    _ => PuppeteerSharp.Media.PaperFormat.A4,
                };

                TempPdfOption.DisplayHeaderFooter = false;
                TempPdfOption.Landscape = isLand;
                TempPdfOption.PrintBackground = true;

                if (isUseCss)
                {
                    TempPdfOption.PreferCSSPageSize = true;
                }
                else
                {
                    TempPdfOption.PreferCSSPageSize = false;

                    PuppeteerSharp.Media.MarginOptions TempMarginOption = new MarginOptions
                    {
                        Top = TopPx + "px",
                        Left = LeftPx + "px",
                        Bottom = BottomPx + "px",
                        Right = RightPx + "px"
                    };
                }

                await TempPage.PdfAsync(SavedFileName, TempPdfOption);
                await TempPage.CloseAsync();
            }

        }

        public static void ConvertHtml2Pdf(string DocumentFileName,string SavedFileName)
        {
            Aspose.Hook.HookManager.StartHook();
            using Aspose.Html.HTMLDocument TempLoadDocument = new Aspose.Html.HTMLDocument(DocumentFileName);
            Aspose.Html.Saving.PdfSaveOptions TempPdfOption = new Aspose.Html.Saving.PdfSaveOptions();

            Aspose.Html.Converters.Converter.ConvertHTML(TempLoadDocument,TempPdfOption,SavedFileName);

        }
        /// <summary>
        /// 将word文档转换为EPUB电子书
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void ConvertWord2EPUB(string DocumentFileName, string SavedFileName, string AuthorName, string CoverImageFileName, int CoverImageDpi)
        {
            Aspose.Hook.HookManager.StartHook();

            byte[] ConverImageBytes;

            Aspose.Words.Document TempLoadDocument = new Aspose.Words.Document(DocumentFileName);

            Aspose.Words.Saving.ImageSaveOptions TempImageOption = new Aspose.Words.Saving.ImageSaveOptions(Aspose.Words.SaveFormat.Jpeg)
            {
                PageSet = new Aspose.Words.Saving.PageSet(0),
                Resolution = CoverImageDpi
            };
            if (!string.IsNullOrEmpty(CoverImageFileName) && File.Exists(CoverImageFileName))
            {

                ConverImageBytes = File.ReadAllBytes(CoverImageFileName);
            }
            else
            {
                MemoryStream TempMemoryStream = new MemoryStream();

                TempLoadDocument.Save(TempMemoryStream, TempImageOption);

                ConverImageBytes = TempMemoryStream.ToArray();

                TempMemoryStream.Close();
            }

            TempLoadDocument.BuiltInDocumentProperties.Thumbnail = ConverImageBytes;


            TempLoadDocument.BuiltInDocumentProperties.Title = "Title";
            TempLoadDocument.BuiltInDocumentProperties.Author = AuthorName == string.Empty ? "DocumentTools" : AuthorName;
            TempLoadDocument.BuiltInDocumentProperties.Company = "DocumentTools";
            Aspose.Words.Saving.HtmlSaveOptions TempHtmlSaveOption = new Aspose.Words.Saving.HtmlSaveOptions

            {
                Encoding = Encoding.UTF8,

                DocumentSplitCriteria = Aspose.Words.Saving.DocumentSplitCriteria.HeadingParagraph,

                ExportDocumentProperties = true,

                ExportGeneratorName = false,

                ExportPageMargins = true,

                ExportPageSetup = true,

                ExportTocPageNumbers = true,

                SaveFormat = Aspose.Words.SaveFormat.Epub
            };

            TempLoadDocument.Save(SavedFileName, TempHtmlSaveOption);


        }
        /// <summary>
        /// EPUB转WORD
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void ConvertEPUB2Word(string DocumentFileName, string SavedFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Html.Saving.DocSaveOptions TempSaveOption = new Aspose.Html.Saving.DocSaveOptions();

            TempSaveOption.PageSetup.AdjustToWidestPage = true;

            //using Aspose.Html.Rendering.Doc.DocDevice docDevice = new Aspose.Html.Rendering.Doc.DocDevice(SavedFileName);

            //using Aspose.Html.HTMLDocument TempLoadDocument = new Aspose.Html.HTMLDocument(DocumentFileName);
            //TempLoadDocument.RenderTo(docDevice);

            Aspose.Html.Converters.Converter.ConvertEPUB(DocumentFileName, TempSaveOption, SavedFileName);
        }

        /// <summary>
        /// EPUB转PDF
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void ConvertEPUB2Pdf(string DocumentFileName, string SavedFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Html.Saving.PdfSaveOptions TempSaveOption = new Aspose.Html.Saving.PdfSaveOptions
            {
                JpegQuality = 100,
            };
            TempSaveOption.PageSetup.AdjustToWidestPage = true;

            Aspose.Html.Converters.Converter.ConvertEPUB(DocumentFileName, TempSaveOption, SavedFileName);

        }

        public static void ConvertWebp2Image(string ImageFileName, string SavedFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Imaging.Image TempWebpImage = Aspose.Imaging.Image.Load(ImageFileName);

            Aspose.Imaging.ImageOptionsBase TempImageOption = null;

            string SavedFileExt = Path.GetExtension(SavedFileName);


            switch (SavedFileExt)
            {
                case ".jpg":
                    TempImageOption = new Aspose.Imaging.ImageOptions.JpegOptions()
                    {
                        Quality = 100,
                    };
                    break;
                case ".jpeg":
                    TempImageOption = new Aspose.Imaging.ImageOptions.JpegOptions()
                    {
                        Quality = 100,
                    };
                    break;
                case ".png":
                    TempImageOption = new Aspose.Imaging.ImageOptions.PngOptions();
                    break;
                case ".bmp":
                    TempImageOption = new Aspose.Imaging.ImageOptions.BmpOptions();
                    break;
                case ".gif":
                    TempImageOption = new Aspose.Imaging.ImageOptions.GifOptions();
                    break;
                default:
                    break;
            }
            TempWebpImage.Save(SavedFileName, TempImageOption);

        }

        public static string GetPdfVersion(string DocumentFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            using Aspose.Pdf.Document TempDocument = new Aspose.Pdf.Document(DocumentFileName);

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"PDF文档版本：{TempDocument.Version}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF文档作者：{(TempDocument.Info.Author ?? "无")}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF文档创建者：{(TempDocument.Info.Creator ?? "无")}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF文档提供者：{(TempDocument.Info.Producer ?? "无")}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF文档主题：{(TempDocument.Info.Subject ?? "无")}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF文档创建日期：{TempDocument.Info.CreationDate}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF文档修改日期：{TempDocument.Info.ModDate}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF是否符合PDF/A标准：{(TempDocument.IsPdfaCompliant == true ? "符合" : "不符合")}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PDF是否符合PDF/UA标准：{(TempDocument.IsPdfUaCompliant == true ? "符合" : "不符合")}");

            return stringBuilder.ToString();
        }

        public static string GetWordInfomation(string DocumentFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Words.Document TempLoadDocument = new Aspose.Words.Document(DocumentFileName);

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Word文档作者：{(TempLoadDocument.BuiltInDocumentProperties.Author == string.Empty ? "无" : TempLoadDocument.BuiltInDocumentProperties.Author)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Word文档提供者：{(TempLoadDocument.BuiltInDocumentProperties.Company == string.Empty ? "无" : TempLoadDocument.BuiltInDocumentProperties.Company)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Word文档最后保存作者：{(TempLoadDocument.BuiltInDocumentProperties.LastSavedBy == string.Empty ? "无" : TempLoadDocument.BuiltInDocumentProperties.LastSavedBy)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Word文档版本：{TempLoadDocument.BuiltInDocumentProperties.Version}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Word文档创建日期：{TempLoadDocument.BuiltInDocumentProperties.CreatedTime}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Word文档最后一次保存日期：{TempLoadDocument.BuiltInDocumentProperties.LastSavedTime}");

            return stringBuilder.ToString();
        }

        public static string GetExcelInfomation(string DocumentFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            StringBuilder stringBuilder = new StringBuilder();

            using Aspose.Cells.Workbook TempLoadWorkBook = new Aspose.Cells.Workbook(DocumentFileName);

            stringBuilder.AppendLine($"Excel文档作者：{(TempLoadWorkBook.BuiltInDocumentProperties.Author == string.Empty ? "无" : TempLoadWorkBook.BuiltInDocumentProperties.Author)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Excel文档提供者：{(TempLoadWorkBook.BuiltInDocumentProperties.Company == string.Empty ? "无" : TempLoadWorkBook.BuiltInDocumentProperties.Company)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Excel文档最后保存作者：{(TempLoadWorkBook.BuiltInDocumentProperties.LastSavedBy == string.Empty ? "无" : TempLoadWorkBook.BuiltInDocumentProperties.LastSavedBy)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Excel文档版本：{(TempLoadWorkBook.BuiltInDocumentProperties.DocumentVersion)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Excel文档创建日期：{(TempLoadWorkBook.BuiltInDocumentProperties.CreatedTime)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"Excel文档最后保存：{(TempLoadWorkBook.BuiltInDocumentProperties.LastSavedTime)}");

            return stringBuilder.ToString();
        }

        public static string GetPPtInformation(string DocumentFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            StringBuilder stringBuilder = new StringBuilder();

            using Aspose.Slides.Presentation TemoLoadPPT = new Aspose.Slides.Presentation(DocumentFileName);

            stringBuilder.AppendLine($"PPT文档作者：{(TemoLoadPPT.DocumentProperties.Author == string.Empty ? "无" : TemoLoadPPT.DocumentProperties.Author)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PPT文档提供者：{(TemoLoadPPT.DocumentProperties.Company == string.Empty ? "无" : TemoLoadPPT.DocumentProperties.Company)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PPT文档最后保存作者：{(TemoLoadPPT.DocumentProperties.LastSavedBy == string.Empty ? "无" : TemoLoadPPT.DocumentProperties.LastSavedBy)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PPT文档版本：{(TemoLoadPPT.DocumentProperties.AppVersion)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PPT文档创建日期：{(TemoLoadPPT.DocumentProperties.CreatedTime)}");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine($"PPT文档最后保存：{(TemoLoadPPT.DocumentProperties.LastSavedTime)}");

            return stringBuilder.ToString();
        }

        //public static string GetImageInfomation(string ImageFileName)
        //{
        //    Aspose.Hook.HookManager.StartHook();

        //    StringBuilder stringBuilder= new StringBuilder();

        //    using Aspose.Imaging.Image TempImage =  Aspose.Imaging.Image.Load(ImageFileName);

        //    PngImage tempjpeg = TempImage as Aspose.Imaging.FileFormats.Png;

        //    tempjpeg
        //}

        public static string GetFileSizeDisplayString(long fileSizeBytes)
        {
            const double KB_Value = 1024;
            const double MB_Value = KB_Value * 1024;
            const double GB_Value = MB_Value * 1024;

            string ReturnValue;

            if (fileSizeBytes < KB_Value)
            {
                ReturnValue = $"{fileSizeBytes} 字节";
            }
            else if (fileSizeBytes < MB_Value)
            {
                double KB_Size = fileSizeBytes / KB_Value;
                ReturnValue = $"{KB_Size:F2} KB";
            }
            else if (fileSizeBytes < GB_Value)
            {
                double MB_SIZE = fileSizeBytes / MB_Value;
                ReturnValue = $"{MB_SIZE:F2} MB";
            }
            else
            {
                double GB_SIZE = fileSizeBytes / GB_Value;
                ReturnValue = $"{GB_SIZE:F2} GB";
            }

            return ReturnValue;
        }

        /// <summary>
        /// 移除小箭头
        /// </summary>
        public static void RemoveShort()
        {
            string SystemRootDir = Environment.GetEnvironmentVariable("systemroot");
            SetRegistryValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29", $"{SystemRootDir}\\system32\\imageres.dll,197", RegistryValueKind.String);
            SetIconCacheAttr();

        }
        /// <summary>
        /// 恢复小箭头
        /// </summary>
        public static void RestoreShort()
        {
            DeleteRegistryKey(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29");
            SetIconCacheAttr();
        }
        /// <summary>
        /// 获取小箭头去除状态
        /// </summary>
        /// <returns></returns>
        public static string GetShortStatus()
        {
            object TempObj = GetRegistryValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29");

            if (TempObj != null && TempObj.ToString().Contains("imageres.dll"))
            {
                return "已移除";
            }
            else
            {
                return "未移除";
            }

        }


        /// <summary>
        /// 设置ico属性
        /// </summary>
        private static void SetIconCacheAttr()
        {
            string UserProfileDir, IconCacheFile;
            UserProfileDir = Environment.GetEnvironmentVariable("userprofile");
            IconCacheFile = $@"{UserProfileDir}\AppData\Local\iconcache.db";
            if (File.Exists(IconCacheFile))
            {
                File.SetAttributes(IconCacheFile, FileAttributes.Normal);
                File.Delete(IconCacheFile);
            }
        }
        /// <summary>
        /// 设置注册表键值
        /// </summary>
        /// <param name="OpenRegistryHive"></param>
        /// <param name="OpenRegistryPath"></param>
        /// <param name="WriteRegistryName"></param>
        /// <param name="WriteRegistryValue"></param>
        /// <param name="WriteKind"></param>
        private static void SetRegistryValue(RegistryHive OpenRegistryHive, string OpenRegistryPath, string WriteRegistryName, object WriteRegistryValue, RegistryValueKind WriteKind)
        {
            RegistryKey TempRootRegistryKey;
            if (Environment.Is64BitOperatingSystem)
            {
                TempRootRegistryKey = RegistryKey.OpenBaseKey(OpenRegistryHive, RegistryView.Registry64);
            }
            else
            {
                TempRootRegistryKey = RegistryKey.OpenBaseKey(OpenRegistryHive, RegistryView.Registry32);
            }

            TempRootRegistryKey.CreateSubKey(OpenRegistryPath, true);


            RegistryKey TempSubKey = TempRootRegistryKey.OpenSubKey(OpenRegistryPath, true);

            if (TempSubKey != null)
            {
                TempSubKey.SetValue(WriteRegistryName, WriteRegistryValue, WriteKind);
                TempSubKey.Close();
            }
            TempRootRegistryKey.Dispose();
        }
        /// <summary>
        /// 获取注册表键值
        /// </summary>
        /// <param name="OpenRegistryHive"></param>
        /// <param name="OpenRegistryPath"></param>
        /// <param name="RegistryName"></param>
        /// <returns></returns>
        private static object GetRegistryValue(RegistryHive OpenRegistryHive, string OpenRegistryPath, string RegistryName)
        {
            object TempValue = null;
            RegistryKey TempRootRegistryKey;
            if (Environment.Is64BitOperatingSystem)
            {
                TempRootRegistryKey = RegistryKey.OpenBaseKey(OpenRegistryHive, RegistryView.Registry64);
            }
            else
            {
                TempRootRegistryKey = RegistryKey.OpenBaseKey(OpenRegistryHive, RegistryView.Registry32);
            }
            RegistryKey TempSubKey = TempRootRegistryKey.OpenSubKey(OpenRegistryPath);

            if (TempSubKey != null)
            {
                TempValue = TempSubKey.GetValue(RegistryName);

                if (TempValue != null)
                {
                    TempSubKey.Close();
                }
            }
            TempRootRegistryKey.Dispose();
            return TempValue;
        }
        /// <summary>
        /// 删除注册表键值
        /// </summary>
        /// <param name="OpenRegistryHive"></param>
        /// <param name="OpenRegistryPath"></param>
        /// <param name="DeleteName"></param>
        private static void DeleteRegistryKey(RegistryHive OpenRegistryHive, string OpenRegistryPath, string DeleteName)
        {
            RegistryKey TempRootRegistryKey;
            if (Environment.Is64BitOperatingSystem)
            {
                TempRootRegistryKey = RegistryKey.OpenBaseKey(OpenRegistryHive, RegistryView.Registry64);
            }
            else
            {
                TempRootRegistryKey = RegistryKey.OpenBaseKey(OpenRegistryHive, RegistryView.Registry32);
            }
            RegistryKey TempSubKey = TempRootRegistryKey.OpenSubKey(OpenRegistryPath, true);

            if (TempSubKey != null && TempSubKey.GetValueNames().Contains(DeleteName))
            {
                TempSubKey.DeleteValue(DeleteName);
                TempSubKey.Close();
            }
            TempRootRegistryKey.Dispose();
        }


        public static DataTable GetMss3VoteInfo(string DataBaseFile)
        {
            DataTable VoteEssInfoTable = new DataTable();


            if (DataBaseFile != string.Empty && DataBaseFile != null)
            {
                string ConnectString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DataBaseFile};Persist Security Info=False;";
                string QueryVoteInfoCommand = $"SELECT VoteId AS 选票序号,VoteName AS 选票名称,Threshold AS 色差校准值,HxrScore AS 候选人填涂值（pixel）,LxrScore AS 另选人填涂值（pixel）,OutsideThreshold AS 纸张级别,SelName AS 选框类型 FROM Vote_EssInfo";

                using OleDbConnection TempConnection = new OleDbConnection(ConnectString);
                TempConnection.Open();

                using OleDbCommand queryVoteInfoCommand = new OleDbCommand(QueryVoteInfoCommand, TempConnection);

                using OleDbDataAdapter TempDataAdapter = new OleDbDataAdapter(queryVoteInfoCommand);

                TempDataAdapter.Fill(VoteEssInfoTable);//填充到DataTable里面
            }

            return VoteEssInfoTable;
        }

        /// <summary>
        /// 修改MSS3模板图片
        /// </summary>
        /// <param name="DataBaseFile"></param>
        /// <param name="ImageFileName"></param>
        /// <param name="statusLabel"></param>
        /// <param name="VoteId"></param>
        /// <param name="VoteName"></param>
        /// <param name="VoteIndex"></param>
        /// <param name="pageIndex"></param>
        public static void InsertVotePicContent(string DataBaseFile, string ImageFileName, Label statusLabel, string VoteId,
                                                string VoteName, string VoteIndex, string pageIndex, string pageSquareType)
        {
            if (DataBaseFile != string.Empty && DataBaseFile != null && ImageFileName != string.Empty && ImageFileName != null && VoteId != string.Empty && VoteId != null && VoteName != string.Empty && VoteName != null)
            {
                //打开数据库连接
                string ConnectString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={DataBaseFile};Persist Security Info=False;";
                string InsertCommand = $"INSERT INTO Vote_PageInfo (VoteID,PageIndex,MatrixCode,PicContent) VALUES(?,?,?,?)";
                string DeleteCommand = $"DELETE FROM Vote_PageInfo WHERE VoteID={VoteId} AND PageIndex={int.Parse(pageIndex)}";
                string CheckCommand = $"SELECT COUNT(*) FROM Vote_PageInfo WHERE VoteID={VoteId} AND PageIndex={int.Parse(pageIndex)}";
                string CheckEssInfoCommand = $"SELECT COUNT(*) FROM Vote_EssInfo WHERE VoteID={VoteId}";
                string InsertTempCommand = $"INSERT INTO Vote_EssInfo (VoteID,VoteName,Threshold,HxrScore,LxrScore,OutsideThreshold,SelName,HxrWidth,HxrHeight,LxrWidth,LxrHeight) VALUES (?,?,?,?,?,?,?,?,?,?,?)";

                using Bitmap TempBmp = new Bitmap(ImageFileName);
                Bitmap outBmp = ConvertTo256Color(TempBmp);
                outBmp.Save("Vote_Gray.bmp", ImageFormat.Bmp);
                byte[] PictureBytes = File.ReadAllBytes("Vote_Gray.bmp");
                using OleDbConnection TempConnection = new OleDbConnection(ConnectString);
                TempConnection.Open();

                //开始插入图片
                using OleDbCommand checkComand = new OleDbCommand(CheckCommand, TempConnection);
                int row_count = Convert.ToInt32(checkComand.ExecuteScalar());

                //根据输入的内容，对数据库进行查询，如果找到记录，就对记录进行删除
                if (row_count > 0)
                {
                    using OleDbCommand deleteComand = new OleDbCommand(DeleteCommand, TempConnection);
                    deleteComand.ExecuteNonQuery();
                }

                using OleDbCommand checkEssComand = new OleDbCommand(CheckEssInfoCommand, TempConnection);

                int ess_count = Convert.ToInt32(checkEssComand.ExecuteScalar());

                //正常情况是允许有两条记录存在的
                if (ess_count == 0)
                {
                    using OleDbCommand insertEssComand = new OleDbCommand(InsertTempCommand, TempConnection);
                    insertEssComand.Parameters.AddWithValue("@VoteID", VoteId);
                    insertEssComand.Parameters.AddWithValue("@VoteName", VoteName);
                    insertEssComand.Parameters.AddWithValue("@Threshold", 45);
                    insertEssComand.Parameters.AddWithValue("@HxrScore", 15);
                    insertEssComand.Parameters.AddWithValue("@LxrScore", 20);
                    insertEssComand.Parameters.AddWithValue("@OutsideThreshold", 1);
                    insertEssComand.Parameters.AddWithValue("@SelName", pageSquareType);
                    insertEssComand.Parameters.AddWithValue("@HxrWidth", 42);
                    insertEssComand.Parameters.AddWithValue("@HxrHeight", 42);
                    insertEssComand.Parameters.AddWithValue("@LxrWidth", 150);
                    insertEssComand.Parameters.AddWithValue("@LxrHeight", 45);
                    insertEssComand.ExecuteNonQuery();
                }

                using OleDbCommand insertPicComand = new OleDbCommand(InsertCommand, TempConnection);
                insertPicComand.Parameters.AddWithValue("@VoteID", VoteId);
                insertPicComand.Parameters.AddWithValue("@PageIndex", int.Parse(pageIndex));
                insertPicComand.Parameters.AddWithValue("@MatrixCode", VoteIndex);
                insertPicComand.Parameters.AddWithValue("@PicContent", PictureBytes);
                insertPicComand.ExecuteNonQuery();
                File.Delete("Vote_Gray.bmp");
                BodyForm.MainForm.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show("插入完毕", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    BodyForm.MainForm.votedataview.DataSource = GetMss3VoteInfo(DataBaseFile);
                    BodyForm.MainForm.votedataview.FirstDisplayedScrollingRowIndex = BodyForm.MainForm.votedataview.RowCount - 1;
                }));
            }

        }
        /// <summary>
        /// 简易生成二维码
        /// </summary>
        /// <param name="barcodeTypeIndex">条码类型</param>
        /// <param name="barcodeAutoSizeMode">条码尺寸模式</param>
        /// <param name="barcodeDmiValue">条码密度</param>
        /// <param name="barcodeRotateValue">旋转角度</param>
        /// <param name="barcodeImageWidth">图片宽度</param>
        /// <param name="barcodeImageHeight">图片高度</param>
        /// <param name="barcodeDpi">分辨率</param>
        /// <param name="barcodeTextContent">文本内容</param>
        /// <param name="barcodeTextLocation">文本位置</param>
        /// <param name="barcodeTextAlign">对齐方式</param>
        /// <param name="barcodeTextSpace">间距</param>
        /// <param name="barcodePaddingSize">内填充间距</param>
        /// <param name="barcodeTextFontSize">字体大小</param>
        /// <param name="useState">使用模式</param>
        /// <param name="savedFileName">保存的路径文件名称</param>
        /// <param name="barcodeCaptionContent">标题内容</param>
        /// <param name="barcodeCaptionLocationIndex">标题位置</param>
        /// <param name="barcodeCaptionFontSize">标题字体大小</param>
        /// <param name="barcodeGenTypeIndex">条码生成类型</param>
        /// <returns></returns>
        public static MemoryStream GenerateBarCode(int barcodeTypeIndex, Aspose.BarCode.Generation.AutoSizeMode barcodeAutoSizeMode, float barcodeDmiValue, float barcodeRotateValue, float barcodeImageWidth,
            float barcodeImageHeight, float barcodeDpi, string barcodeTextContent, Aspose.BarCode.Generation.CodeLocation barcodeTextLocation, Aspose.BarCode.Generation.TextAlignment barcodeTextAlign, float barcodeTextSpace, int barcodePaddingSize,
            float barcodeTextFontSize, string useState, string savedFileName, string barcodeCaptionContent, int barcodeCaptionLocationIndex, float barcodeCaptionFontSize, int barcodeGenTypeIndex)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.BarCode.Generation.BarcodeGenerator TempGenerate = null;
            MemoryStream TempStream = new MemoryStream();

            switch (barcodeTypeIndex)
            {
                case 0:
                    TempGenerate = new Aspose.BarCode.Generation.BarcodeGenerator(Aspose.BarCode.Generation.EncodeTypes.QR);
                    break;
                case 1:
                    TempGenerate = new Aspose.BarCode.Generation.BarcodeGenerator(Aspose.BarCode.Generation.EncodeTypes.DataMatrix);
                    break;
                case 2:
                    TempGenerate = new Aspose.BarCode.Generation.BarcodeGenerator(Aspose.BarCode.Generation.EncodeTypes.Pdf417);
                    TempGenerate.Parameters.Barcode.Pdf417.Rows = 6;
                    break;
                case 3:
                    TempGenerate = new Aspose.BarCode.Generation.BarcodeGenerator(Aspose.BarCode.Generation.EncodeTypes.Aztec);
                    break;
                case 4:
                    TempGenerate = new Aspose.BarCode.Generation.BarcodeGenerator(Aspose.BarCode.Generation.EncodeTypes.HanXin);
                    break;
                default:
                    break;
            }

            if (barcodeGenTypeIndex == 0)
            {
                TempGenerate.Parameters.Barcode.QR.QrEncodeMode = QREncodeMode.ECIEncoding;
                TempGenerate.Parameters.Barcode.QR.QrECIEncoding = ECIEncodings.UTF8;
            }
            else if (barcodeGenTypeIndex == 1)
            {
                TempGenerate.Parameters.Barcode.QR.QrEncodeMode = QREncodeMode.Bytes;
            }




            TempGenerate.CodeText = barcodeTextContent;
            TempGenerate.Parameters.AutoSizeMode = barcodeAutoSizeMode;

            TempGenerate.Parameters.Barcode.XDimension.Pixels = barcodeDmiValue;
            TempGenerate.Parameters.RotationAngle = barcodeRotateValue;
            TempGenerate.Parameters.Resolution = barcodeDpi;//条码分辨率，但是这个值设置不好容易报错

            TempGenerate.Parameters.Barcode.Padding.Left.Pixels = barcodePaddingSize;
            TempGenerate.Parameters.Barcode.Padding.Top.Pixels = barcodePaddingSize;
            TempGenerate.Parameters.Barcode.Padding.Right.Pixels = barcodePaddingSize;
            TempGenerate.Parameters.Barcode.Padding.Bottom.Pixels = barcodePaddingSize;



            TempGenerate.Parameters.Barcode.CodeTextParameters.Location = barcodeTextLocation;
            TempGenerate.Parameters.Barcode.CodeTextParameters.Alignment = barcodeTextAlign;
            TempGenerate.Parameters.Barcode.CodeTextParameters.Space.Pixels = barcodeTextSpace;
            TempGenerate.Parameters.Barcode.CodeTextParameters.NoWrap = false;
            TempGenerate.Parameters.Barcode.CodeTextParameters.FontMode = Aspose.BarCode.Generation.FontMode.Manual;
            TempGenerate.Parameters.Barcode.CodeTextParameters.Font.Size.Point = barcodeTextFontSize;

            TempGenerate.Parameters.CaptionAbove.Font.Size.Point = barcodeCaptionFontSize;
            TempGenerate.Parameters.CaptionBelow.Font.Size.Point = barcodeCaptionFontSize;
            TempGenerate.Parameters.CaptionAbove.NoWrap = false;
            TempGenerate.Parameters.CaptionBelow.NoWrap = false;
            TempGenerate.Parameters.CaptionAbove.Alignment = Aspose.BarCode.Generation.TextAlignment.Center;
            TempGenerate.Parameters.CaptionBelow.Alignment = Aspose.BarCode.Generation.TextAlignment.Center;

            if (barcodeAutoSizeMode == Aspose.BarCode.Generation.AutoSizeMode.Interpolation)
            {
                //当设置为插值的时候才生效
                TempGenerate.Parameters.ImageWidth.Pixels = barcodeImageWidth;
                TempGenerate.Parameters.ImageHeight.Pixels = barcodeImageHeight;
            }


            if (barcodeCaptionContent != string.Empty)
            {
                switch (barcodeCaptionLocationIndex)
                {
                    case 0:
                        TempGenerate.Parameters.CaptionBelow.Visible = false;
                        TempGenerate.Parameters.CaptionAbove.Visible = false;
                        break;
                    case 1:
                        TempGenerate.Parameters.CaptionAbove.Visible = true;
                        TempGenerate.Parameters.CaptionAbove.Text = barcodeCaptionContent;
                        break;
                    case 2:
                        TempGenerate.Parameters.CaptionBelow.Visible = true;
                        TempGenerate.Parameters.CaptionBelow.Text = barcodeCaptionContent;
                        break;
                    default:
                        break;
                }
            }

            switch (useState)
            {
                case "barcode_preview":
                    TempGenerate.Save(TempStream, Aspose.BarCode.Generation.BarCodeImageFormat.Png);
                    break;
                case "barcode_mutil_save":
                    TempGenerate.Save(savedFileName, Aspose.BarCode.Generation.BarCodeImageFormat.Bmp);
                    break;
                default:
                    break;
            }


            return TempStream;

        }

        /// <summary>
        /// 识别条码图片返回结果
        /// </summary>
        /// <param name="barcodeImage">条码图片</param>
        /// <returns></returns>
        public static string ReadBarCodeContent(System.Drawing.Bitmap barcodeImage)
        {
            Aspose.Hook.HookManager.StartHook();
            string ReaderResult = string.Empty;
            using BarCodeReader TempReader = new BarCodeReader(barcodeImage, DecodeType.AllSupportedTypes.Exclude(DecodeType.MicrE13B));
            foreach (BarCodeResult TempResult in TempReader.ReadBarCodes())
            {
                ReaderResult += TempResult.CodeText;
            }
            if (ReaderResult == string.Empty)
            {
                ReaderResult = "识别失败，可能是以下原因：\r\n1、条码不完整\r\n2、图片内无条码内容\r\n3、不支持的条码类型";
            }

            return ReaderResult;
        }
        /// <summary>
        /// 读取条码模板内容
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static DataTable ReadBarCodeTemplate(string FileName)
        {
            Aspose.Hook.HookManager.StartHook();
            Workbook TempLoadBook = new Workbook(FileName);
            Worksheet TempWorkSheet = TempLoadBook.Worksheets[0];
            DataTable TempBarCodeGenTable = new DataTable();
            TempBarCodeGenTable.Columns.Add("编号");
            TempBarCodeGenTable.Columns.Add("标题");
            TempBarCodeGenTable.Columns.Add("内容");
            for (int row = 1; row <= TempWorkSheet.Cells.MaxDataRow; row++)
            {
                Aspose.Cells.Cell TempCell_GenNumber = TempWorkSheet.Cells.GetCell(row, 0);
                Aspose.Cells.Cell TempCell_GenCaption = TempWorkSheet.Cells.GetCell(row, 1);
                Aspose.Cells.Cell TempCell_GenContent = TempWorkSheet.Cells.GetCell(row, 2);

                if (TempCell_GenNumber != null && TempCell_GenContent != null)
                {
                    if (TempCell_GenNumber.StringValue != string.Empty && TempCell_GenContent.StringValue != string.Empty)
                    {
                        TempBarCodeGenTable.Rows.Add(TempCell_GenNumber.StringValue, TempCell_GenCaption?.StringValue, TempCell_GenContent.StringValue);
                    }
                }
                else
                {
                    TempBarCodeGenTable.Rows.Add("模板错误_编号列无内容", "模板错误_标题列无内容", "模板错误_内容列无内容");

                }

            }
            return TempBarCodeGenTable;
        }

        /// <summary>
        /// 生成身份证信息，带有计算校验位
        /// </summary>
        /// <param name="card_pac_code">地区编码</param>
        /// <param name="card_birthday">出生日期</param>
        /// <param name="card_end_code">出生顺序编码</param>
        public static string GenChinaCard(string card_pac_code, string card_birthday, string card_end_code)
        {
            string result_card_id = card_pac_code + card_birthday + card_end_code;

            int result_card_id_0 = int.Parse(result_card_id[0].ToString());
            int result_card_id_1 = int.Parse(result_card_id[1].ToString());
            int result_card_id_2 = int.Parse(result_card_id[2].ToString());
            int result_card_id_3 = int.Parse(result_card_id[3].ToString());
            int result_card_id_4 = int.Parse(result_card_id[4].ToString());
            int result_card_id_5 = int.Parse(result_card_id[5].ToString());
            int result_card_id_6 = int.Parse(result_card_id[6].ToString());
            int result_card_id_7 = int.Parse(result_card_id[7].ToString());
            int result_card_id_8 = int.Parse(result_card_id[8].ToString());
            int result_card_id_9 = int.Parse(result_card_id[9].ToString());
            int result_card_id_10 = int.Parse(result_card_id[10].ToString());
            int result_card_id_11 = int.Parse(result_card_id[11].ToString());
            int result_card_id_12 = int.Parse(result_card_id[12].ToString());
            int result_card_id_13 = int.Parse(result_card_id[13].ToString());
            int result_card_id_14 = int.Parse(result_card_id[14].ToString());
            int result_card_id_15 = int.Parse(result_card_id[15].ToString());
            int result_card_id_16 = int.Parse(result_card_id[16].ToString());

            //计算校验位算法
            int result_sign_code = ((result_card_id_0 * 7) + (result_card_id_1 * 9) + (result_card_id_2 * 10) + (result_card_id_3 * 5) + (result_card_id_4 * 8) +
                (result_card_id_5 * 4) + (result_card_id_6 * 2) + (result_card_id_7 * 1) + (result_card_id_8 * 6) + (result_card_id_9 * 3) +
                (result_card_id_10 * 7) + (result_card_id_11 * 9) + (result_card_id_12 * 10) + (result_card_id_13 * 5) + (result_card_id_14 * 8) + (result_card_id_15 * 4) + (result_card_id_16 * 2)) % 11;


            switch (result_sign_code)
            {
                case 0:
                    result_card_id += "1";
                    break;
                case 1:
                    result_card_id += "0";
                    break;
                case 2:
                    result_card_id += "X";
                    break;
                case 3:
                    result_card_id += "9";
                    break;
                case 4:
                    result_card_id += "8";
                    break;
                case 5:
                    result_card_id += "7";
                    break;
                case 6:
                    result_card_id += "6";
                    break;
                case 7:
                    result_card_id += "5";
                    break;
                case 8:
                    result_card_id += "4";
                    break;
                case 9:
                    result_card_id += "3";
                    break;
                case 10:
                    result_card_id += "2";
                    break;
                default:
                    break;
            }

            return result_card_id;



        }

        public static Tuple<string, string, string, bool, string> VaildateChinaCard(string card_id)
        {
            Tuple<string, string, string, bool, string> card_id_ver_result = new Tuple<string, string, string, bool, string>("", "", "", false, "");
            if (card_id.Length == 18)
            {
                //提取前6位的信息

                string card_id_01 = card_id[0].ToString();
                string card_id_02 = card_id[1].ToString();
                string card_id_03 = card_id[2].ToString();
                string card_id_04 = card_id[3].ToString();
                string card_id_05 = card_id[4].ToString();
                string card_id_06 = card_id[5].ToString();

                bool card_id_01_ver = int.TryParse(card_id_01, out int card_id_01_int);
                bool card_id_02_ver = int.TryParse(card_id_02, out int card_id_02_int);
                bool card_id_03_ver = int.TryParse(card_id_03, out int card_id_03_int);
                bool card_id_04_ver = int.TryParse(card_id_04, out int card_id_04_int);
                bool card_id_05_ver = int.TryParse(card_id_05, out int card_id_05_int);
                bool card_id_06_ver = int.TryParse(card_id_06, out int card_id_06_int);


                int now_year = DateTime.Now.Year;
                string card_id_year = card_id.Substring(6, 4);
                string card_id_month = card_id.Substring(10, 2);
                string card_id_day = card_id.Substring(12, 2);



                //提取顺序码和性别信息

                string card_id_index_number = card_id.Substring(14, 3);
                char card_id_sex_sign = card_id_index_number[2];



                bool card_id_year_number_ver = int.TryParse(card_id_year, out int card_id_year_int);
                bool card_id_month_number_ver = int.TryParse(card_id_month, out int card_id_month_int);
                bool card_id_day_number_ver = int.TryParse(card_id_day, out int card_id_day_int);

                bool card_id_index_number_ver = int.TryParse(card_id_index_number, out int card_id_index_number_int);
                bool card_id_sex_sign_number_ver = int.TryParse(card_id_sex_sign.ToString(), out int card_id_sex_sign_int);


                string ver_pass_date = "";//出生日期
                string ver_pass_index = "";//出生顺序
                string ver_pass_sex = "";//性别
                bool sign_ver_status = false;//校验状态

                //日期校验
                if (card_id_year_number_ver && card_id_month_number_ver && card_id_day_number_ver)
                {
                    if (card_id_year_int >= 1900 && card_id_year_int <= now_year &&
                        card_id_month_int <= 12 && card_id_day_int <= 31)
                    {
                        ver_pass_date = new DateTime(card_id_year_int, card_id_month_int, card_id_day_int).ToString("yyyy年MM月dd日");
                    }
                    else
                    {
                        ver_pass_date = "日期不正确";
                    }
                }

                //提取顺序
                if (card_id_index_number_ver)
                {
                    ver_pass_index = card_id_index_number_int.ToString();
                }
                //计算性别
                if (card_id_sex_sign_number_ver)
                {
                    if (card_id_sex_sign_int % 2 == 0)
                    {
                        ver_pass_sex = "女";
                    }
                    else
                    {
                        ver_pass_sex = "男";
                    }
                }


                //计算校验码，确保全是数字以后再进行计算
                if (card_id_01_ver && card_id_02_ver && card_id_03_ver && card_id_04_ver
                    && card_id_05_ver && card_id_06_ver && card_id_year_number_ver && card_id_day_number_ver
                    && card_id_day_number_ver && card_id_index_number_ver && card_id_sex_sign_number_ver)
                {
                    int result_card_id_0 = int.Parse(card_id[0].ToString());
                    int result_card_id_1 = int.Parse(card_id[1].ToString());
                    int result_card_id_2 = int.Parse(card_id[2].ToString());
                    int result_card_id_3 = int.Parse(card_id[3].ToString());
                    int result_card_id_4 = int.Parse(card_id[4].ToString());
                    int result_card_id_5 = int.Parse(card_id[5].ToString());
                    int result_card_id_6 = int.Parse(card_id[6].ToString());
                    int result_card_id_7 = int.Parse(card_id[7].ToString());
                    int result_card_id_8 = int.Parse(card_id[8].ToString());
                    int result_card_id_9 = int.Parse(card_id[9].ToString());
                    int result_card_id_10 = int.Parse(card_id[10].ToString());
                    int result_card_id_11 = int.Parse(card_id[11].ToString());
                    int result_card_id_12 = int.Parse(card_id[12].ToString());
                    int result_card_id_13 = int.Parse(card_id[13].ToString());
                    int result_card_id_14 = int.Parse(card_id[14].ToString());
                    int result_card_id_15 = int.Parse(card_id[15].ToString());
                    int result_card_id_16 = int.Parse(card_id[16].ToString());
                    string result_card_id_17 = card_id[17].ToString();

                    //计算校验位算法
                    int result_sign_code = ((result_card_id_0 * 7) + (result_card_id_1 * 9) + (result_card_id_2 * 10) + (result_card_id_3 * 5) + (result_card_id_4 * 8) +
                        (result_card_id_5 * 4) + (result_card_id_6 * 2) + (result_card_id_7 * 1) + (result_card_id_8 * 6) + (result_card_id_9 * 3) +
                        (result_card_id_10 * 7) + (result_card_id_11 * 9) + (result_card_id_12 * 10) + (result_card_id_13 * 5) + (result_card_id_14 * 8) + (result_card_id_15 * 4) + (result_card_id_16 * 2)) % 11;

                    string curren_sign_code = "";

                    switch (result_sign_code)
                    {
                        case 0:
                            curren_sign_code = "1";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 1:
                            curren_sign_code = "0";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 2:
                            curren_sign_code = "X";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 3:
                            curren_sign_code = "9";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 4:
                            curren_sign_code = "8";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 5:
                            curren_sign_code = "7";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 6:
                            curren_sign_code = "6";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 7:
                            curren_sign_code = "5";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 8:
                            curren_sign_code = "4";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 9:
                            curren_sign_code = "3";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        case 10:
                            curren_sign_code = "2";
                            sign_ver_status = result_card_id_17.ToString() == curren_sign_code;
                            break;
                        default:
                            break;
                    }


                    card_id_ver_result = new Tuple<string, string, string, bool, string>(ver_pass_date, ver_pass_sex, ver_pass_index, sign_ver_status, curren_sign_code.ToString());
                }
            }
            return card_id_ver_result;
        }

        /// <summary>
        /// 生成随机日期
        /// </summary>
        /// <param name="random_obj">随机实例</param>
        /// <param name="only_gen_18_year">是否大于18岁</param>
        /// <returns></returns>
        public static string GenRandomBirthday(Random random_obj, bool only_gen_18_year)
        {
            int random_year = random_obj.Next(1900, DateTime.Now.Year);
            if (only_gen_18_year)
            {
                random_year = random_obj.Next(1900, DateTime.Now.Year - 18);
            }
            int random_month = random_obj.Next(1, 13);
            int max_day = DateTime.DaysInMonth(random_year, random_month);
            int random_day = random_obj.Next(1, max_day + 1);

            string result_random_birthday = new DateTime(random_year, random_month, random_day).ToString("yyyyMMdd");

            return result_random_birthday;
        }
        /// <summary>
        /// 生成随机序号，三位数字
        /// </summary>
        /// <param name="random_obj">随机实例</param>
        /// <returns></returns>
        public static string GenRandomIndex(Random random_obj)
        {

            return random_obj.Next(001, 1000).ToString("D3");//格式化字符串，不足三位前导0补足
        }

        /// <summary>
        /// 生成随机姓名
        /// </summary>
        /// <param name="random_obj_first"></param>
        /// <param name="random_obj_name"></param>
        /// <returns></returns>
        public static string GenRandomName(Random random_obj_first, Random random_obj_name)
        {

            string[] Name_First_List = (string[])GetEmbedResourceObj("chinse_first").ToString().Split('\r');
            string[] Name_Text_List = (string[])GetEmbedResourceObj("chinese_han").ToString().Split('\r');
            string gen_random_result = "";

            int random_first_index = random_obj_first.Next(0, Name_First_List.Length);
            int random_name_index = random_obj_name.Next(0, Name_Text_List.Length);

            int random_name_len = random_obj_name.Next(2, 4);


            string random_first = Name_First_List[random_first_index];

            string random_name_1 = Name_Text_List[random_name_index];

            string random_name_2 = Name_Text_List[random_name_index + 1];

            if (random_name_len == 2)
            {
                gen_random_result = random_first + "  " + random_name_1;
            }
            else
            {
                gen_random_result = random_first + random_name_1 + random_name_2;

            }

            return gen_random_result;
        }


        /// <summary>
        /// 批量生成文件副本（GPT写的，我根本看不懂）
        /// </summary>
        /// <param name="SourceFile"></param>
        /// <param name="GenNumber"></param>
        /// <param name="StatusLabel"></param>
        /// <param name="ReNameStr"></param>
        /// <param name="OutPutPath"></param>
        public static void GenCopySameFile(string SourceFile, int GenNumber, Label StatusLabel, string ReNameStr, string PrefixNumberCount, string OutPutPath)
        {
            if (SourceFile != string.Empty && File.Exists(SourceFile))
            {
                string SourceFileName = Path.GetFileNameWithoutExtension(SourceFile);
                string SourceFileNameExt = Path.GetExtension(SourceFile);

                // 获取目标目录中已存在的副本文件
                string[] existingCopies = Directory.GetFiles(OutPutPath)
                    .Where(file => Path.GetFileName(file).StartsWith($"{SourceFileName}- {ReNameStr}"))
                    .ToArray();

                Dictionary<string, int> fileCounts = new Dictionary<string, int>();

                foreach (string existingCopy in existingCopies)
                {
                    string existingFileName = Path.GetFileNameWithoutExtension(existingCopy);
                    if (existingFileName.StartsWith($"{SourceFileName}- {ReNameStr} （"))
                    {
                        int countStartIndex = existingFileName.LastIndexOf('（') + 1;
                        int countEndIndex = existingFileName.LastIndexOf('）');
                        if (countStartIndex >= 0 && countEndIndex >= 0)
                        {
                            string countStr = existingFileName.Substring(countStartIndex, countEndIndex - countStartIndex);
                            if (int.TryParse(countStr, out int count))
                            {
                                fileCounts[existingCopy] = count;
                            }
                        }
                    }
                }

                int maxCount = 0;
                if (fileCounts.Count > 0)
                {
                    maxCount = fileCounts.Values.Max();
                }

                for (int i = 1; i <= GenNumber; i++)
                {
                    string TempCopiedName = $"{SourceFileName}- {ReNameStr} （{(maxCount + i).ToString($"D{PrefixNumberCount}")}）{SourceFileNameExt}";
                    string GenFileName = Path.Combine(OutPutPath, TempCopiedName);
                    File.Copy(SourceFile, GenFileName);
                    StatusLabel.Text = $"目录内共有{maxCount + i}个该文件";
                }
            }
        }


        /// <summary>
        /// 获取网卡名称列表，wmi方式
        /// </summary>
        /// <param name="OnlyShowNormal"></param>
        /// <returns></returns>
        public static List<string> GetNetWorkCardList(bool OnlyShowNormal)
        {
            List<string> NetworkLists = new List<string>();
            string QueryNetwork = "SELECT * FROM Win32_NetworkAdapter";

            if (OnlyShowNormal)
            {
                QueryNetwork += " WHERE NetConnectionStatus = 2";
            }

            using ManagementObjectSearcher TempSearcher = new ManagementObjectSearcher(QueryNetwork);
            using ManagementObjectCollection TempNetworkList = TempSearcher.Get();
            foreach (ManagementObject TempNetwork in TempNetworkList)
            {
                string NetworkCardName = TempNetwork.GetPropertyValue("Description") as string;
                NetworkLists.Add(NetworkCardName);
            }

            return NetworkLists;
        }

        /// <summary>
        /// 根据网卡名称获取基础的网络信息
        /// </summary>
        /// <param name="NetworkCardName"></param>
        /// <returns></returns>
        public static (List<string>, List<string>, List<string>, string, string, string,bool) GetCard_Information(string NetworkCardName)
        {
            List<string> IPV4_ADDRESSES = new List<string>();
            List<string> SUBNET_MASKS = new List<string>();
            List<string> DEFAULT_GATEWAYS = new List<string>();
            List<string> DnsLists = new List<string>();
            string DNS_1 = "";
            string DNS_2 = "";
            string MAC_ADDRESS = "";
            bool IS_ENABLE_DHCP = false;

            NetworkInterface TempInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.Description == NetworkCardName);

            if (TempInterface != null && TempInterface != default)
            {
                foreach (UnicastIPAddressInformation TempIp in TempInterface.GetIPProperties().UnicastAddresses)
                {
                    if (TempIp.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        IPV4_ADDRESSES.Add(TempIp.Address.ToString());
                        SUBNET_MASKS.Add(TempIp.IPv4Mask.ToString());

                        foreach (GatewayIPAddressInformation TempGateway in TempInterface.GetIPProperties().GatewayAddresses)
                        {
                            if (TempGateway.Address.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                DEFAULT_GATEWAYS.Add(TempGateway.Address.ToString());
                            }
                        }
                    }
                }

                IPInterfaceProperties TempInterfacePro = TempInterface.GetIPProperties();
                IPAddressCollection DNSAddresses = TempInterfacePro.DnsAddresses;
                IS_ENABLE_DHCP = TempInterfacePro.GetIPv4Properties().IsDhcpEnabled;
                foreach (IPAddress DnsAddress in DNSAddresses)
                {
                    if(DnsAddress.AddressFamily==System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        DnsLists.Add(DnsAddress.ToString());
                    }
                }
                DNS_1 = DnsLists.Count > 0 ? DnsLists[0] : "未获取到";
                DNS_2 = DnsLists.Count > 1 ? DnsLists[1] : "未获取到";

                MAC_ADDRESS = BitConverter.ToString(TempInterface.GetPhysicalAddress().GetAddressBytes());

                return (IPV4_ADDRESSES, SUBNET_MASKS, DEFAULT_GATEWAYS, DNS_1, DNS_2, MAC_ADDRESS, IS_ENABLE_DHCP);
            }
            return (new List<string>() { "未获取到" }, new List<string>() { "未获取到" }, new List<string>() { "未获取到" }, "未获取到", "未获取到", "未获取到",false);
        }


        /// <summary>
        /// 根据网卡名称设置ip地址等信息
        /// </summary>
        /// <param name="NetworkCardName"></param>
        /// <param name="ipaddress"></param>
        /// <param name="mask"></param>
        /// <param name="gateway"></param>
        /// <param name="dns1"></param>
        /// <param name="dns2"></param>
        /// <param name="UseStatic"></param>
        public static void SetInterfaceInformation(string NetworkCardName,
            string ipaddress, string mask, string gateway, string dns1, string dns2, bool UseStatic)
        {
            ManagementBaseObject InputParama = null;
            ManagementBaseObject OutputResult = null;

            using ManagementClass TempClass = new ManagementClass("Win32_NetworkAdapterConfiguration");

            using ManagementObjectCollection TempObjects = TempClass.GetInstances();

            foreach (ManagementObject TempObject in TempObjects)
            {
                //网卡必须已启用且有效
                if ((bool)TempObject.GetPropertyValue("IPEnabled"))
                {
                    //根据网卡描述匹配进行地址设置
                    if (TempObject.GetPropertyValue("Description").ToString() == NetworkCardName)
                    {

                        if (UseStatic)
                        {
                            //使用静态IP
                            InputParama = TempObject.GetMethodParameters("EnableStatic");
                            InputParama["IPAddress"] = new string[] { ipaddress };
                            InputParama["SubnetMask"] = new string[] { mask };
                            OutputResult = TempObject.InvokeMethod("EnableStatic", InputParama, null);

                            InputParama = TempObject.GetMethodParameters("SetGateways");
                            InputParama["DefaultIPGateway"] = new string[] { gateway };
                            OutputResult = TempObject.InvokeMethod("SetGateways", InputParama, null);

                            InputParama = TempObject.GetMethodParameters("SetDNSServerSearchOrder");
                            if(dns1=="0.0.0.0" || dns1==string.Empty)
                            {
                                InputParama["DNSServerSearchOrder"] = new string[] { dns2 };
                            }
                            else if(dns2=="0.0.0.0" || dns1 == string.Empty)
                            {
                                InputParama["DNSServerSearchOrder"] = new string[] { dns1 };
                            }
                            else
                            {
                                InputParama["DNSServerSearchOrder"] = new string[] { dns1,dns2 };
                            }

                            OutputResult = TempObject.InvokeMethod("SetDNSServerSearchOrder", InputParama, null);

                        }
                        else//设置DHCP
                        {
                            InputParama = TempObject.GetMethodParameters("EnableDHCP");
                            OutputResult = TempObject.InvokeMethod("EnableDHCP", InputParama, null);

                            //设置自动DNS
                            InputParama = TempObject.GetMethodParameters("SetDNSServerSearchOrder");
                            InputParama["DNSServerSearchOrder"] = null;
                            OutputResult = TempObject.InvokeMethod("SetDNSServerSearchOrder", InputParama, null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 根据网卡名称来启用或禁用网卡
        /// </summary>
        /// <param name="NetworkCardName"></param>
        /// <param name="StatusMethodText"></param>
        public static void SetInterfaceStatus(string NetworkCardName, string StatusMethodText)
        {
            using ManagementObjectSearcher TempSearcher = new ManagementObjectSearcher($"SELECT * FROM Win32_NetworkAdapter WHERE Description='" + NetworkCardName + "'");
            using ManagementObjectCollection TempList = TempSearcher.Get();

            foreach (ManagementObject TempObject in TempList)
            {
                TempObject.InvokeMethod(StatusMethodText, null);
            }
        }


        public static DataTable GetSharePaths()
        {
            DataTable TempTable = new DataTable();
            TempTable.Columns.Add("DisplayValue");
            TempTable.Columns.Add("RealValue");
            using ManagementObjectSearcher TempSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Share");
            foreach(ManagementObject TempShare in TempSearcher.Get())
            {
                string ShareName = TempShare.GetPropertyValue("Name").ToString();
                string SharePath = TempShare.GetPropertyValue("Path").ToString();
                if(SharePath!=string.Empty)
                {
                    TempTable.Rows.Add(ShareName, SharePath);
                }
            }
            return TempTable;

        }

        public static string ConvertMoney2ChineseYuan(decimal num)
        {
            string str1 = "零壹贰叁肆伍陆柒捌玖";            //0-9所对应的汉字 
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; //数字位所对应的汉字 
            string str3 = "";    //从原num值中取出的值 
            string str4 = "";    //数字的字符串形式 
            string str5 = "";  //人民币大写金额形式 
            int i;    //循环变量 
            int j;    //num的值乘以100的字符串长度 
            string ch1 = "";    //数字的汉语读法 
            string ch2 = "";    //数字位的汉字读法 
            int nzero = 0;  //用来计算连续的零值是几个 
            int temp;            //从原num值中取出的值 

            num = Math.Round(Math.Abs(num), 2);    //将num取绝对值并四舍五入取2位小数 
            str4 = ((long)(num * 100)).ToString();        //将num乘100并转换成字符串形式 
            j = str4.Length;      //找出最高位 
            if (j > 15) { return "溢出"; }
            str2 = str2.Substring(15 - j);   //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分 

            //循环取出每一位需要转换的值 
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //取出需转换的某一位的值 
                temp = Convert.ToInt32(str3);      //转换为数字 
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时 
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位 
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp * 1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上 
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整” 
                    str5 = str5 + '整';
                }
            }
            if (num == 0)
            {
                str5 = "零元整";
            }
            return str5;
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity WI = WindowsIdentity.GetCurrent();
            WindowsPrincipal WP = new WindowsPrincipal(WI);

            return WP.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static string GetAdministratorBehavior()
        {
            string BehaviorStatus =  GetRegistryValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA").ToString();

            return BehaviorStatus;

        }

        public static void SetAdministratorBehavior(bool Enbale)
        {
            if(Enbale)
            {
                SetRegistryValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 1,RegistryValueKind.DWord);

            }
            else
            {
                SetRegistryValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "EnableLUA", 0,RegistryValueKind.DWord);

            }
        }





        /// <summary>
        /// bmp转为256色
        /// </summary>
        /// <param name="inputBitmap"></param>
        /// <returns></returns>
        static Bitmap ConvertTo256Color(Bitmap inputBitmap)
        {
            Bitmap outputBitmap = new Bitmap(inputBitmap.Width, inputBitmap.Height, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            System.Drawing.Imaging.ColorPalette palette = outputBitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                palette.Entries[i] = Color.FromArgb(i, i, i);
            }
            outputBitmap.Palette = palette;

            BitmapData inputData = inputBitmap.LockBits(new System.Drawing.Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            BitmapData outputData = outputBitmap.LockBits(new System.Drawing.Rectangle(0, 0, outputBitmap.Width, outputBitmap.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

            unsafe
            {
                byte* inputPointer = (byte*)inputData.Scan0;
                byte* outputPointer = (byte*)outputData.Scan0;

                for (int y = 0; y < inputBitmap.Height; y++)
                {
                    for (int x = 0; x < inputBitmap.Width; x++)
                    {
                        byte r = inputPointer[2];
                        byte g = inputPointer[1];
                        byte b = inputPointer[0];
                        byte grayValue = (byte)(0.299 * r + 0.587 * g + 0.114 * b);

                        outputPointer[0] = grayValue;

                        inputPointer += 3;
                        outputPointer++;
                    }

                    inputPointer += inputData.Stride - inputBitmap.Width * 3;
                    outputPointer += outputData.Stride - outputBitmap.Width;
                }
            }

            inputBitmap.UnlockBits(inputData);
            outputBitmap.UnlockBits(outputData);

            return outputBitmap;
        }
    }
}
