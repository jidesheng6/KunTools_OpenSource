using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace DocumentTools.Services
{
    /// <summary>
    /// 工具服务：封装通用的工具方法
    /// </summary>
    public class UtilityService : IUtilityService
    {
        /// <summary>
        /// 生成指定长度的随机字符串
        /// </summary>
        public string GetRandomString(int stringLength)
        {
            const string fixedString = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();
            return new string(
                Enumerable.Repeat(fixedString, stringLength)
                    .Select(s => s[random.Next(s.Length)])
                    .ToArray());
        }

        /// <summary>
        /// 检查字符串是否包含特殊字符（用于测评表）
        /// </summary>
        public bool CheckInsertExamSpecialChar(string checkString)
        {
            if (string.IsNullOrEmpty(checkString)) return false;
            
            string specialChars = @"[""/&\:\*\?\,\<\>\|\.\+\-\=\^\%\$\#\@\!\~\`\;\'\,]";
            return Regex.IsMatch(checkString, specialChars);
        }

        /// <summary>
        /// 正则匹配结果，如果匹配到了就返回true
        /// </summary>
        public bool MatchRegexStatus(string matchMode, string inputText)
        {
            if (string.IsNullOrEmpty(matchMode) || string.IsNullOrEmpty(inputText))
                return false;
            return Regex.Match(inputText, matchMode).Success;
        }

        /// <summary>
        /// 使用正则表达式获取匹配的文本内容
        /// </summary>
        public string GetReportContentText(Regex regexClass, string contentString)
        {
            if (regexClass == null || string.IsNullOrEmpty(contentString))
                return string.Empty;

            Match match = regexClass.Match(contentString);
            return match.Success ? match.Value : string.Empty;
        }

        /// <summary>
        /// 使用正则表达式获取所有匹配的文本内容列表
        /// </summary>
        public System.Collections.Generic.List<string> GetReportContentTexts(Regex regexClass, string contentString)
        {
            var contentTexts = new System.Collections.Generic.List<string>();
            
            if (regexClass == null || string.IsNullOrEmpty(contentString))
                return contentTexts;

            MatchCollection matches = regexClass.Matches(contentString);
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    contentTexts.Add(match.Value);
                }
            }
            
            return contentTexts;
        }
    }

    public interface IUtilityService
    {
        string GetRandomString(int stringLength);
        bool CheckInsertExamSpecialChar(string checkString);
        bool MatchRegexStatus(string matchMode, string inputText);
        string GetReportContentText(Regex regexClass, string contentString);
        System.Collections.Generic.List<string> GetReportContentTexts(Regex regexClass, string contentString);
    }
}
