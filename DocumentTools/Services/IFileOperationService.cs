using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentTools.Services
{
    public interface IFileOperationService
    {
        Task<IList<string>> SelectFilesAsync(string filterDescription, string filterPattern, bool allowMulti = true);
        Task<string> SelectFolderAsync();
    }
}
