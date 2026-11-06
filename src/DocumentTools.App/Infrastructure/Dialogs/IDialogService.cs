using System.Windows;
using Microsoft.Win32;

namespace DocumentTools.App.Infrastructure.Dialogs;

public interface IDialogService
{
    IReadOnlyList<string> OpenFiles(string filter, bool multiSelect = true);

    string? SaveFile(string filter, string? defaultName = null);

    string? PickFolder();

    void Info(string message, string title = "提示");

    void Error(string message, string title = "错误");

    bool Confirm(string message, string title = "确认");
}

public sealed class DialogService : IDialogService
{
    public IReadOnlyList<string> OpenFiles(string filter, bool multiSelect = true)
    {
        var dialog = new OpenFileDialog
        {
            Filter = filter,
            Multiselect = multiSelect,
            CheckFileExists = true
        };

        return dialog.ShowDialog() == true
            ? dialog.FileNames
            : Array.Empty<string>();
    }

    public string? SaveFile(string filter, string? defaultName = null)
    {
        var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultName ?? string.Empty
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? PickFolder()
    {
        var dialog = new OpenFolderDialog();
        return dialog.ShowDialog() == true ? dialog.FolderName : null;
    }

    public void Info(string message, string title = "提示")
        => MessageBox.Show(Owner(), message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void Error(string message, string title = "错误")
        => MessageBox.Show(Owner(), message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirm(string message, string title = "确认")
        => MessageBox.Show(Owner(), message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question)
           == MessageBoxResult.OK;

    private static Window? Owner()
        => Application.Current?.MainWindow;
}
