using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DocumentTools
{
    internal class ShareImportWindowsApi
    {
        /// <summary>
        /// 文件名自然序API
        /// </summary>
        /// <param name="psz1"></param>
        /// <param name="psz2"></param>
        /// <returns></returns>
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }
}
