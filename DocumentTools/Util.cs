using Aspose.BarCode.BarCodeRecognition;
using Aspose.Cells;
using Aspose.Cells.Rendering;
using Aspose.Html.Net;
using Aspose.Html.Services;
using Aspose.Imaging;
using Aspose.Pdf;
using Aspose.Pdf.Annotations;
using Aspose.Pdf.Text;
using Aspose.Slides.Export;
using Microsoft.VisualBasic.Devices;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

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
        public static void PlayMp3WithStream()
        {
            //这里必须这样写，不要用using语句来自动执行dispose，否则声音还没播放的时候就已经被dispose了
            MemoryStream EmbedResouceObj = new MemoryStream((byte[])GetEmbedResourceObj("zdsny"));
            StreamMediaFoundationReader TempStream = new StreamMediaFoundationReader(EmbedResouceObj);
            IWavePlayer Player = new WaveOut();
            Player.Init(TempStream);
            Player.Play();

        }

        /// <summary>
        /// 将word文档转换为pdf文件
        /// </summary>
        /// <param name="documentFileName">doc/docx文档路径</param>
        /// <param name="SavePdfName">导出pdf的路径</param>
        /// <param name="tempSaveDocxPath">临时存放转换的docx目录</param>
        public static void ConvertWord2Pdf(string documentFileName, string SavePdfName, string tempSaveDocxPath, Aspose.Words.Saving.PdfCompliance PdfComplanceOption)
        {
            Aspose.Hook.HookManager.StartHook();
            string DocumentName, SaveDocxName;

            //设置加载word文档的时候参数
            Aspose.Words.Loading.LoadOptions TempDocuemntLoadOption = new Aspose.Words.Loading.LoadOptions()
            {
                ConvertMetafilesToPng = false,
                MswVersion = Aspose.Words.Settings.MsWordVersion.Word2010,
            };

            //设置PDF保存参数
            Aspose.Words.Saving.PdfSaveOptions TempPdfaveOption = new Aspose.Words.Saving.PdfSaveOptions()
            {
                FontEmbeddingMode = Aspose.Words.Saving.PdfFontEmbeddingMode.EmbedAll,
                ColorMode = Aspose.Words.Saving.ColorMode.Normal,
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
                //Compliance = Aspose.Words.Saving.OoxmlCompliance.Iso29500_2008_Strict,
                SaveFormat = Aspose.Words.SaveFormat.Docx,

            };



            Aspose.Words.Document TempDocument = new Aspose.Words.Document(documentFileName,
                                                                           TempDocuemntLoadOption)
            {
                RemovePersonalInformation = true
            };

            //TempDocument.JustificationMode = Aspose.Words.Settings.JustificationMode.CompressKana;

            DocumentName = System.IO.Path.GetFileNameWithoutExtension(documentFileName);

            SaveDocxName = System.IO.Path.Combine(tempSaveDocxPath, DocumentName + ".docx");

            TempDocument.UpdateFields();

            TempDocument.Save(SaveDocxName, TempDocxSaveOption);

            Aspose.Words.Document TempDocxDocument = new Aspose.Words.Document(SaveDocxName, TempDocuemntLoadOption);

            //TempDocxDocument.JustificationMode = Aspose.Words.Settings.JustificationMode.CompressKana;

            TempDocxDocument.UpdateFields();


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
        public static void ConvertImage2PDF(string ImageFileName, string SavedDocumentFileName, bool IsNeedMerge, List<string> ImageFilesList, bool LandStyle, bool AutoOptiomizePdf, bool CompressPdfImage, double Margin_TOP = 0, double Margin_LEFT = 0, double Margin_Bottom = 0, double Margin_RIGHT = 0)
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
                    TempPage.PageInfo.IsLandscape = LandStyle;
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
                TempPage.PageInfo.IsLandscape = LandStyle;
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

            Aspose.Slides.Presentation TempPresentationDocument = null;
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
                    TempPresentationDocument = new Aspose.Slides.Presentation(DocumentsFileName);
                    break;
                case ".pptx":
                    TempPresentationDocument = new Aspose.Slides.Presentation(DocumentsFileName);
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
                    //单独处理tif的转换
                    TempPdfTifDevice = new Aspose.Pdf.Devices.TiffDevice(TempPdfImageResolution);
                    TempExcelImageOPtion.ImageType = Aspose.Cells.Drawing.ImageType.Tiff;
                    break;
                default: break;
            }

            //word转图片的设置
            TempWordImageSaveOption.UseHighQualityRendering = true;
            TempWordImageSaveOption.Resolution = DpiValue;
            TempWordImageSaveOption.ColorMode = Aspose.Words.Saving.ColorMode.Normal;
            TempWordImageSaveOption.PixelFormat = Aspose.Words.Saving.ImagePixelFormat.Format24BppRgb;
            TempWordImageSaveOption.UseAntiAliasing = true;

            //excel转图片的设置
            TempExcelImageOPtion.CheckWorkbookDefaultFont = true;
            TempExcelImageOPtion.HorizontalResolution = DpiValue;
            TempExcelImageOPtion.VerticalResolution = DpiValue;
            TempExcelImageOPtion.PixelFormat = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            TempWordImageSaveOption.UseAntiAliasing = true;



            if (SetGrayMode)
            {
                //对图像进行灰度处理（只有word转图片才可以设置）
                TempWordImageSaveOption.ImageColorMode = Aspose.Words.Saving.ImageColorMode.Grayscale;
                TempWordImageSaveOption.ColorMode = Aspose.Words.Saving.ColorMode.Grayscale;
            }

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
                        Debug.WriteLine(TempSavedImageileWithSheetName);
                        SheetRender TempSheetRender = new SheetRender(TempWorkSheet, TempExcelImageOPtion);
                        TempSheetRender.ToImage(0, TempSavedImageileWithSheetName);
                    }
                }
            }
            else//合并一页导出，暂时不做
            {
                TempWordDocument.Save(SavedImageFileName, TempWordImageSaveOption);
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
        public static void RelaceDocumentContent(string DocumentsFileName, string SavedDoucmentFileName, string RelaceDocumentType, string InputRegexRule, string ReplaceContent, bool IgnoreFootText, bool NoIgnoreCase, bool RemoveRowMode = false, int ExcelStartRow = -1, int ExcelEndRow = -1, bool UseRegex = false)
        {
            Aspose.Hook.HookManager.StartHook();

            Regex DocuemntRegex = new Regex(InputRegexRule);
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
                    break;
                case ".jpeg":
                    TempJpgOption.CompressionType = Compress_Jpeg_Mode;
                    TempJpgOption.ColorType = Compress_Jpeg_ColorMode;
                    TempJpgOption.Quality = Compress_Jpeg_Quailty;
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
        public static string[] AddText2List(string InputText, bool RemoveNumber, bool RemoveSquare, bool NameStorke,bool NameReplace,string SearchText,string ReplaceText)
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
                            TempInputTextNoSpace = Regex.Replace(TempInputTextNoSpace,SearchText,ReplaceText,RegexOptions.Multiline);
                        }
                        else
                        {
                            TempInputTextNoSpace = TempInputTextNoSpace.Replace(SearchText,ReplaceText);
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

                        if (NameReplace && SearchText!=string.Empty)
                        {
                            if (BodyForm.MainForm.NameReplaceMode)
                            {
                                AddListText = Regex.Replace(AddListText, SearchText, ReplaceText,RegexOptions.Multiline);
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
        /// HTML转PDF，暂且搁置
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void ConvertHtml2PDF(string DocumentFileName,string SavedFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Html.Configuration TempConfig = new Aspose.Html.Configuration();
            Aspose.Html.Rendering.Pdf.PdfRenderingOptions TempPdfRenderOption = new Aspose.Html.Rendering.Pdf.PdfRenderingOptions();
            TempPdfRenderOption.Css.MediaType = Aspose.Html.Rendering.MediaType.Screen;
            TempPdfRenderOption.PageSetup.AdjustToWidestPage = true;

            using (Aspose.Html.HTMLDocument TempDocument = new Aspose.Html.HTMLDocument(DocumentFileName)) {

                using Aspose.Html.Rendering.Pdf.PdfDevice TempPdfDevice = new Aspose.Html.Rendering.Pdf.PdfDevice(TempPdfRenderOption, SavedFileName);
                TempDocument.RenderTo(TempPdfDevice);
            };

           //INetworkService networkService =  TempConfig.GetService<INetworkService>();

           // networkService.MessageHandlers.Insert(0, new TimeoutMessageHandler());

           //    Aspose.Html.Saving.PdfSaveOptions TempOption = new Aspose.Html.Saving.PdfSaveOptions()
           //         {
           //             JpegQuality = 100,
           //         };

           //    TempOption.PageSetup.AdjustToWidestPage = true;

           //     Aspose.Html.Converters.Converter.ConvertHTML(DocumentFileName, TempConfig,TempOption,SavedFileName);
        }

        /// <summary>
        /// 将word文档转换为EPUB电子书
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void ConvertWord2EPUB(string DocumentFileName,string SavedFileName) {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Words.Document TempLoadDocument =  new Aspose.Words.Document(DocumentFileName);

            Aspose.Words.Saving.HtmlSaveOptions TempHtmlSaveOption = new Aspose.Words.Saving.HtmlSaveOptions
            {
                Encoding = Encoding.UTF8,

                DocumentSplitCriteria = Aspose.Words.Saving.DocumentSplitCriteria.HeadingParagraph,

                ExportDocumentProperties = true,

                SaveFormat = Aspose.Words.SaveFormat.Epub
            };

            TempLoadDocument.Save(SavedFileName,TempHtmlSaveOption);

        }
        /// <summary>
        /// EPUB转WORD
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void ConvertEPUB2Word(string DocumentFileName,string SavedFileName) {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Html.Saving.DocSaveOptions TempSaveOption = new Aspose.Html.Saving.DocSaveOptions();

            TempSaveOption.PageSetup.AdjustToWidestPage = true;

            //using Aspose.Html.Rendering.Doc.DocDevice docDevice = new Aspose.Html.Rendering.Doc.DocDevice(SavedFileName);

            //using Aspose.Html.HTMLDocument TempLoadDocument = new Aspose.Html.HTMLDocument(DocumentFileName);
            //TempLoadDocument.RenderTo(docDevice);

            Aspose.Html.Converters.Converter.ConvertEPUB(DocumentFileName,TempSaveOption,SavedFileName);
        }

        /// <summary>
        /// EPUB转PDF
        /// </summary>
        /// <param name="DocumentFileName"></param>
        /// <param name="SavedFileName"></param>
        public static void ConvertEPUB2Pdf(string DocumentFileName,string SavedFileName)
        {
            Aspose.Hook.HookManager.StartHook();

            Aspose.Html.Saving.PdfSaveOptions TempSaveOption = new Aspose.Html.Saving.PdfSaveOptions
            {
                JpegQuality = 100,
            };
            TempSaveOption.PageSetup.AdjustToWidestPage = true;

            Aspose.Html.Converters.Converter.ConvertEPUB(DocumentFileName,TempSaveOption,SavedFileName);
            
        }

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
            SetRegistryValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons","29", $"{SystemRootDir}\\system32\\imageres.dll,197", RegistryValueKind.String);
            SetIconCacheAttr();

        }
        /// <summary>
        /// 恢复小箭头
        /// </summary>
        public static void RestoreShort()
        {
            DeleteRegistryKey(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons","29");
            SetIconCacheAttr();
        }
        /// <summary>
        /// 获取小箭头去除状态
        /// </summary>
        /// <returns></returns>
        public static string GetShortStatus()
        {
            object TempObj = GetRegistryValue(RegistryHive.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons", "29");

            if(TempObj != null && TempObj.ToString().Contains("imageres.dll"))
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
        private static void SetRegistryValue(RegistryHive OpenRegistryHive,string OpenRegistryPath,string WriteRegistryName, object WriteRegistryValue,RegistryValueKind WriteKind)
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


            RegistryKey TempSubKey =  TempRootRegistryKey.OpenSubKey(OpenRegistryPath,true);

            if (TempSubKey!=null)
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
                
                if(TempValue != null)
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
        private static void DeleteRegistryKey(RegistryHive OpenRegistryHive, string OpenRegistryPath,string DeleteName)
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

            if(TempSubKey!=null && TempSubKey.GetValueNames().Contains(DeleteName))
            {
                TempSubKey.DeleteValue(DeleteName);
                TempSubKey.Close();
            }
            TempRootRegistryKey.Dispose();
        }
    }

    public class TimeoutMessageHandler : MessageHandler
    {
        // Override the Invoke() method
        public override void Invoke(INetworkOperationContext context)
        {
            context.Request.Timeout = TimeSpan.FromSeconds(1);
            Next(context);
        }
    }




}
