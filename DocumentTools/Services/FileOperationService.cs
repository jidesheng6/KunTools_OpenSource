using System.Collections.Generic;
using System.Threading.Tasks;

namespace DocumentTools.Services
{
    /// <summary>
    /// 文件操作服务占位实现。后续将迁移 Util 中的具体逻辑。
    /// </summary>
    public class FileOperationService : IFileOperationService
    {
        public Task<IList<string>> SelectFilesAsync(string filterDescription, string filterPattern, bool allowMulti = true)
        {
            // 占位：后续实现调用现有的文件对话框逻辑。
            return Task.FromResult<IList<string>>(new List<string>());
        }

        public Task<string> SelectFolderAsync()
        {
            // 占位：后续实现调用现有的文件夹选择逻辑。
            return Task.FromResult(string.Empty);
        }
    }
}
