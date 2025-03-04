using Aspose.BarCode.BarCodeRecognition;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //SetProcessDPIAware();
            //string AsposeCrackString = Util.GetEmbedResourceObj("AsposeCrackLicBaseString").ToString();
            //using (MemoryStream TempLoadStream = new MemoryStream(Convert.FromBase64String(AsposeCrackString)))
            //{
            //    new Aspose.Cells.License().SetLicense(TempLoadStream);
            //}
            Aspose.Hook.HookManager.ShowHookDetails(true);
            Aspose.Hook.HookManager.StartHook();
            int workerThreads;
            int completionPortThreads;
            ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
            ThreadPool.SetMaxThreads(Math.Max(Environment.ProcessorCount * 4, workerThreads), completionPortThreads);
            ThreadPool.GetMinThreads(out workerThreads, out completionPortThreads);
            ThreadPool.SetMinThreads(Math.Max(Environment.ProcessorCount * 4, workerThreads), completionPortThreads);

            BarCodeReader.ProcessorSettings.UseAllCores = true;
            BarCodeReader.ProcessorSettings.MaxAdditionalAllowedThreads = Environment.ProcessorCount * 2;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new BodyForm());
        }
    }
}
