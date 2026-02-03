using System.Collections.Generic;

namespace DocumentTools.Services
{
    /// <summary>
    /// 通用转换选项基类。
    /// 后续可根据需要扩展具体类型。
    /// </summary>
    public class ConversionOptions
    {
        /// <summary>
        /// 源文件路径列表。
        /// </summary>
        public IList<string> SourceFiles { get; set; } = new List<string>();

        /// <summary>
        /// 目标输出目录或文件名。
        /// </summary>
        public string TargetPath { get; set; }
    }

    /// <summary>
    /// Word 转 PDF 选项。
    /// </summary>
    public class Word2PdfOptions : ConversionOptions
    {
        public string PdfCompliance { get; set; }
        public string WordVersion { get; set; }
        public bool NeedUpdateFields { get; set; }
        public string ColorMode { get; set; }
        public bool EmbedFonts { get; set; }
        public string LoadPassword { get; set; }
        public bool UseLocalOffice { get; set; }
    }
}
