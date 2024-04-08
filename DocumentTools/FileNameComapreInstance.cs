using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentTools
{
    internal class FileNameComparerInstance:IComparer<string>
    {
        public int Compare(string x,string y)
        {
            return ShareImportWindowsApi.StrCmpLogicalW(x,y);
        }
    }
}
