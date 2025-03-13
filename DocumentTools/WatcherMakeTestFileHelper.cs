using System;
using System.IO;
using System.Linq;

namespace DocumentTools
{
    internal static class WatcherMakeTestFileHelper
    {
        public static string RandomTempFileName { get; set; }
        private static FileStream TempFileStream;

        public static void MakeJpgFile()
        {
            string Random_JPG_File = string.Concat(RandomTempFileName, ".jpg");
            TempFileStream = File.Create(Random_JPG_File);
            TempFileStream.Dispose();
        }

        public static void MakeJpgErrFile()
        {
            string Random_JPGErr_File = string.Concat(RandomTempFileName, ".jpg.err");
            TempFileStream = File.Create(Random_JPGErr_File);
            TempFileStream.Dispose();
        }

        public static void MakeJpgHanFile()
        {
            string Random_JPGHan_File = string.Concat(RandomTempFileName, ".jpg.han");
            TempFileStream = File.Create(Random_JPGHan_File);
            TempFileStream.Dispose();
        }

        public static void MakeTifFile()
        {
            string Random_TIF_File = string.Concat(RandomTempFileName, ".tif");
            TempFileStream = File.Create(Random_TIF_File);
            TempFileStream.Dispose();
        }

        public static void MakeTifErrFile()
        {
            string Random_TIFErr_File = string.Concat(RandomTempFileName, ".tif.err");
            TempFileStream = File.Create(Random_TIFErr_File);
            TempFileStream.Dispose();
        }

        public static void MakeTifHanFile()
        {
            string Random_TIFHan_File = string.Concat(RandomTempFileName, ".tif.han");
            TempFileStream = File.Create(Random_TIFHan_File);
            TempFileStream.Dispose();
        }
    }
}
