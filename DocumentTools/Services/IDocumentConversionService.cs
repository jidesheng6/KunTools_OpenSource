using System.Threading.Tasks;
using System.Windows.Forms;

namespace DocumentTools.Services
{
    public interface IDocumentConversionService
    {
        Task ConvertWord2PdfAsync(Word2PdfOptions options);

        Task ConvertStartAsync(
            ListView listViewControl,
            Label statusLabelControl,
            DocumentTools.ConvertFileType convertType,
            string extensionString,
            bool useSourcePath = false,
            Button callButton = null);

        // 其他转换方法将在后续阶段补充
    }
}
