namespace DocumentTools.Services
{
    public class TextProcessingOptions
    {
        public bool RemoveNumber { get; set; }
        public bool RemoveSquare { get; set; }
        public bool NameStroke { get; set; }
        public bool NameReplace { get; set; }
        public string SearchText { get; set; }
        public string ReplaceText { get; set; }
        public bool DoubleTextAlign { get; set; }
        public bool GetChinsesHanPinYin { get; set; }
        public int PinYinTypeIndex { get; set; } // 0-5
        public bool NameReplaceMode { get; set; } // True for Regex, False for String
    }
}