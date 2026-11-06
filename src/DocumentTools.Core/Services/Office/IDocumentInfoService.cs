namespace DocumentTools.Core.Services.Office;

/// <summary>Port of legacy Util.GetPdfVersion / GetWordInformation / …</summary>
public interface IDocumentInfoService
{
    string GetInfo(string filePath);
}
