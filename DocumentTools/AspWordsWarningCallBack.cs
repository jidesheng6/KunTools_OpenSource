using Aspose.Words;
using System;
using System.Diagnostics;
using System.Linq;

namespace DocumentTools
{
    internal class AspWordsWarningCallBack : Aspose.Words.IWarningCallback
    {
        public void Warning(WarningInfo info)
        {
            Debug.WriteLine(info.Description);
            Warnings.Warning(info);
        }

        public WarningInfoCollection Warnings = new WarningInfoCollection();
    }
}
