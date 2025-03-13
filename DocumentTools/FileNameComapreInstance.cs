using System;
using System.Collections.Generic;
using System.Linq;

namespace DocumentTools
{
    internal class FileNameComparerInstance : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return ShareImportWindowsApi.StrCmpLogicalW(x, y);
        }
    }
}
