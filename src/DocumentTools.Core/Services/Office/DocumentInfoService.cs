using System.Text;
using Aspose.Cells;
using Aspose.Pdf;
using Aspose.Slides;
using Aspose.Words;
using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Office;

public sealed class DocumentInfoService : IDocumentInfoService
{
    public string GetInfo(string filePath)
    {
        PathGuard.RequireExistingFile(filePath, nameof(filePath));
        string ext = Path.GetExtension(filePath);

        return ext.ToLowerInvariant() switch
        {
            ".pdf" => GetPdf(filePath),
            ".doc" or ".docx" => GetWord(filePath),
            ".xls" or ".xlsx" => GetExcel(filePath),
            ".ppt" or ".pptx" => GetPpt(filePath),
            _ => "暂不支持获取该文件信息"
        };
    }

    private static string GetPdf(string filePath)
    {
        using var document = new Aspose.Pdf.Document(filePath);
        var sb = new StringBuilder();
        sb.AppendLine($"PDF文档版本：{document.Version}");
        sb.AppendLine();
        sb.AppendLine($"PDF文档作者：{(document.Info.Author ?? "无")}");
        sb.AppendLine();
        sb.AppendLine($"PDF文档创建者：{(document.Info.Creator ?? "无")}");
        sb.AppendLine();
        sb.AppendLine($"PDF文档提供者：{(document.Info.Producer ?? "无")}");
        sb.AppendLine();
        sb.AppendLine($"PDF文档主题：{(document.Info.Subject ?? "无")}");
        sb.AppendLine();
        sb.AppendLine($"页数：{document.Pages.Count}");
        return sb.ToString();
    }

    private static string GetWord(string filePath)
    {
        var document = new Aspose.Words.Document(filePath);
        var props = document.BuiltInDocumentProperties;
        var sb = new StringBuilder();
        sb.AppendLine($"Word文档作者：{(string.IsNullOrEmpty(props.Author) ? "无" : props.Author)}");
        sb.AppendLine();
        sb.AppendLine($"Word文档提供者：{(string.IsNullOrEmpty(props.Company) ? "无" : props.Company)}");
        sb.AppendLine();
        sb.AppendLine($"Word文档最后保存作者：{(string.IsNullOrEmpty(props.LastSavedBy) ? "无" : props.LastSavedBy)}");
        sb.AppendLine();
        sb.AppendLine($"Word文档版本：{props.Version}");
        sb.AppendLine();
        sb.AppendLine($"页数：{document.PageCount}");
        return sb.ToString();
    }

    private static string GetExcel(string filePath)
    {
        using var workbook = new Workbook(filePath);
        var sb = new StringBuilder();
        sb.AppendLine($"工作表数量：{workbook.Worksheets.Count}");
        sb.AppendLine();
        foreach (Worksheet sheet in workbook.Worksheets)
        {
            sb.AppendLine($"- {sheet.Name}（可见：{sheet.IsVisible}）");
        }

        return sb.ToString();
    }

    private static string GetPpt(string filePath)
    {
        using var presentation = new Presentation(filePath);
        var sb = new StringBuilder();
        sb.AppendLine($"幻灯片数量：{presentation.Slides.Count}");
        return sb.ToString();
    }
}
