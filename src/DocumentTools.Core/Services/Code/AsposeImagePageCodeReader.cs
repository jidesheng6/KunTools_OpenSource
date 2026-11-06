using Aspose.BarCode.BarCodeRecognition;

namespace DocumentTools.Core.Services.Code;

/// <summary>
/// 默认页码码读取：用 Aspose.BarCode 解码图片里的全部 DataMatrix，取第一处 <c>T</c> 开头的内容。
/// <para>
/// 评估版（无授权）会截断末字符并追加水印句，例如 <c>T001</c> 读成 <c>T00*Recognized by Aspose...</c>；
/// 这是授权状态造成的产物差异，不是本类的解析规则。授权版返回原文。
/// </para>
/// </summary>
public sealed class AsposeImagePageCodeReader : IImagePageCodeReader
{
    /// <summary>
    /// legacy <c>CodeText.Split('T')[1]</c> 的等价实现：取第一个 T 码的后半段。
    /// legacy 会对同一张图里的每个 T 码各复制一份到不同页码目录；这里取第一处，避免一张图散落多份。
    /// </summary>
    public string? ReadPageCode(string imageFile)
    {
        using var reader = new BarCodeReader(imageFile, DecodeType.DataMatrix);
        foreach (BarCodeResult result in reader.ReadBarCodes())
        {
            string text = result.CodeText ?? string.Empty;
            if (text.Length > 1 && text[0] == 'T')
            {
                return text[1..];
            }
        }

        return null;
    }
}
