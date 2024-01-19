using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Layout;
using System;
using System.Diagnostics;
using System.Linq;
using Aspose.Slides;
using System.IO;

namespace DocumentTestDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Aspose.Hook.HookManager.StartHook();

            string f1 = @"C:\Users\pwn\Desktop\新建文件夹 (2)\001.pptx";
            string f2 = @"C:\Users\pwn\Desktop\新建文件夹 (2)";

            testdemo0(f1,f2);

        }


        static void testdemo0(string f1,string f2)
        {
            Presentation DestPre = new Presentation();
            
            DestPre.SlideSize.SetSize(SlideSizeType.OnScreen16x9, SlideSizeScaleType.Maximize);

            using (Presentation SourcePre = new Presentation(f1))
            {
                ISlideCollection SourceSildes = SourcePre.Slides;//原稿的slides集合

                for(int SlideIndex = 0;SlideIndex< SourceSildes.Count;SlideIndex++)
                {
                    ISlide TempSourceSlide = SourceSildes[SlideIndex];//原告的单个slide
                    
                    //原稿的母版slide
                    IMasterSlide TempSourceMasterSlide = TempSourceSlide.LayoutSlide.MasterSlide;

                        IMasterSlideCollection DestMasters = DestPre.Masters;

                        IMasterSlide DestMasterSlide = TempSourceSlide.LayoutSlide.MasterSlide;

                        IMasterSlide DestClonePreSlide = DestMasters.AddClone(TempSourceMasterSlide);

                        ISlideCollection DestSlides = DestPre.Slides;


                        DestSlides.RemoveAt(0);
                        
                        DestSlides.AddClone(TempSourceSlide, DestClonePreSlide,false);

                        string SaveDestName = $"{Path.Combine(f2,SlideIndex.ToString())}.pptx";

                        DestPre.Save(SaveDestName,Aspose.Slides.Export.SaveFormat.Pptx);
                }
                DestPre.Dispose();
            }
            

        }
    }
}
