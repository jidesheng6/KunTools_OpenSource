using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Layout;
using System;
using System.Diagnostics;
using System.Linq;
using Aspose.Slides;
using System.IO;
using Aspose.OCR;

namespace DocumentTestDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Aspose.Hook.HookManager.StartHook();

            string f1 = @"F:\BaiduNetdiskDownload\税务局面试真题258页_第1到5页.pdf";
            string f2 = @"F:\BaiduNetdiskDownload\";

            testdemo0(f1,f2);

        }


        static void testdemo0(string f1,string f2)
        {
            Aspose.OCR.Logging.Console = true;
            Aspose.OCR.Logging.LoggingLevel = LoggingLevel.Debug;
            AsposeOcr ocr = new AsposeOcr();
            RecognitionSettings recognitionSettings = new RecognitionSettings();
            OcrInput input = new OcrInput(InputType.PDF);

            input.Add(f1);

            recognitionSettings.Language = Language.Chi;

            AsposeOcr.SaveMultipageDocument($@"{f2}1.docx",Aspose.OCR.SaveFormat.Docx,ocr.Recognize(input,recognitionSettings));

        }
    }
}
