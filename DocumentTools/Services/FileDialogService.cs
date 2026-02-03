using System.Windows.Forms;

namespace DocumentTools.Services
{
    /// <summary>
    /// 文件对话框服务：封装保存/打开文件对话框操作
    /// </summary>
    public class FileDialogService : IFileDialogService
    {
        private readonly IFormAccessor _formAccessor;

        public FileDialogService(IFormAccessor formAccessor)
        {
            _formAccessor = formAccessor;
        }

        public string ShowSaveFileDialog(string fileFilterString, string displayTitleString = "保存文件")
        {
            var saveFileDialog = _formAccessor.GetSaveFileDialog();
            if (saveFileDialog == null) return string.Empty;

            string saveFileName = string.Empty;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.AddExtension = true;
            saveFileDialog.Title = displayTitleString;
            saveFileDialog.Filter = fileFilterString;
            
            DialogResult saveFileDialogResult = saveFileDialog.ShowDialog();
            if (saveFileDialogResult == DialogResult.OK)
            {
                saveFileName = saveFileDialog.FileName;
            }
            
            return saveFileName;
        }
    }

    public interface IFileDialogService
    {
        string ShowSaveFileDialog(string fileFilterString, string displayTitleString = "保存文件");
    }
}
