using Aspose.Words;
using Aspose.Words.Drawing;
using Aspose.Words.Layout;
using System;
using System.Diagnostics;
using System.Linq;

namespace DocumentTestDemo
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Aspose.Hook.HookManager.StartHook();

            string filename = @"C:\Users\pwn\Desktop\测试表格\1、政治素质测评（单人单表）.docx";
            string savefilename = @"C:\Users\pwn\Desktop\测试表格\1、政治素质测评（单人单表）_1.docx";

            // testdemo1(filename,savefilename);

            testdemo0(filename,savefilename);


        }


        static void testdemo0(string f1,string f2)
        {
            Document testdoc = new Document(f1);

            DocumentBuilder testbud = new DocumentBuilder(testdoc);

            LayoutCollector TempLayoutCol = new LayoutCollector(testdoc);
            LayoutEnumerator TempLayoutEm = new LayoutEnumerator(testdoc);

            foreach (Shape shape in testdoc.GetChildNodes(NodeType.Shape,true))
            {
                //插入单人表逻辑。
                if(shape.IsImage)
                {

                    if(shape.AlternativeText!=string.Empty)
                    {
                        //Debug.WriteLine(shape.AlternativeText);

                        shape.ImageData.SetImage(@"C:\Users\pwn\Pictures\二维码\测评\右下角\002测评人.bmp");
                    }
                }
            }

            testdoc.Save(f2);
        }
        static void testdemo1(string f1,string f2)
        {
            Document TempDocument = new Document(f1);

            LayoutCollector TempLayoutCol = new LayoutCollector(TempDocument);
            LayoutEnumerator TempLayoutEm = new LayoutEnumerator(TempDocument);

            foreach (Shape TempShape in TempDocument.GetChildNodes(NodeType.Shape, true))
            {
                if (TempShape.IsImage)
                {
                    TempLayoutEm.Current = TempLayoutCol.GetEntity(TempShape);
                    TempShape.RelativeVerticalPosition = RelativeVerticalPosition.Page;
                    TempShape.Top = TempLayoutEm.Rectangle.Top;
                }

            }

            TempDocument.Save(f2);

            Console.ReadLine();
        }
    }
}
