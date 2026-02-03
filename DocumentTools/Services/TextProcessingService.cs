using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using NPinyin;

namespace DocumentTools.Services
{
    public class TextProcessingService
    {
        private readonly IUtilityService _utilityService;

        public TextProcessingService(IUtilityService utilityService)
        {
            _utilityService = utilityService;
        }

        public string[] ProcessText(string inputText, TextProcessingOptions options)
        {
            if (string.IsNullOrEmpty(inputText)) return new string[0];

            // 优先处理拼音转换
            if (options.GetChinsesHanPinYin)
            {
                var lines = inputText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var pinyinList = new List<string>();

                foreach (var line in lines)
                {
                    switch (options.PinYinTypeIndex)
                    {
                        case 0: // 首字母大写
                            pinyinList.Add(Pinyin.GetInitials(line).ToUpper());
                            break;
                        case 1: // 首字母小写
                            pinyinList.Add(Pinyin.GetInitials(line).ToLower());
                            break;
                        case 2: // 拼音大写（空格）
                            pinyinList.Add(Pinyin.GetPinyin(line).ToUpper());
                            break;
                        case 3: // 拼音大写（无空格）
                            pinyinList.Add(Regex.Replace(Pinyin.GetPinyin(line).ToUpper(), @"\s+", string.Empty));
                            break;
                        case 4: // 拼音小写（空格）
                            pinyinList.Add(Pinyin.GetPinyin(line).ToLower());
                            break;
                        case 5: // 拼音小写（无空格）
                            pinyinList.Add(Regex.Replace(Pinyin.GetPinyin(line).ToLower(), @"\s+", string.Empty));
                            break;
                        default:
                            pinyinList.Add(line);
                            break;
                    }
                }
                return pinyinList.ToArray();
            }

            // 常规文本处理
            List<string> textList = new List<string>();
            string[] returnTextList;

            // 检查是否为表格内容（包含制表符）
            if (_utilityService.MatchRegexStatus("\\t", inputText))
            {
                inputText = Regex.Replace(inputText, "\n", string.Empty, RegexOptions.Multiline);
                inputText = Regex.Replace(inputText, "\t", "\n", RegexOptions.Multiline);

                string[] tempInputTextByTap = inputText.Split(new char[] { '\n' });
                string[] allSpaceChar = new string[] { "\\s", "○", "□", "£" };

                foreach (string tempText in tempInputTextByTap)
                {
                    string tempInputTextNoSpace = RemoveSpeciCharacter(allSpaceChar, tempText);

                    if (options.RemoveNumber)
                    {
                        tempInputTextNoSpace = RemoveSpeciCharacter(new string[] { "[0-9]" }, tempInputTextNoSpace);
                    }

                    if (options.RemoveSquare)
                    {
                        tempInputTextNoSpace = RemoveSpeciCharacter(
                            new string[] { "\\(.+?\\)|\\（.+?\\）|\\(\\)|\\（\\）" },
                            tempInputTextNoSpace);
                    }

                    if (options.NameReplace && !string.IsNullOrEmpty(options.SearchText))
                    {
                        if (options.NameReplaceMode)
                        {
                            try
                            {
                                tempInputTextNoSpace = Regex.Replace(
                                    tempInputTextNoSpace,
                                    options.SearchText,
                                    options.ReplaceText,
                                    RegexOptions.Multiline);
                            }
                            catch
                            {
                                // 忽略正则错误
                            }
                        }
                        else
                        {
                            tempInputTextNoSpace = tempInputTextNoSpace.Replace(options.SearchText, options.ReplaceText);
                        }
                    }

                    if (!string.IsNullOrEmpty(tempInputTextNoSpace))
                    {
                        textList.Add(tempInputTextNoSpace);
                    }
                }
            }
            else
            {
                string[] tempNewLineText = inputText.Split(new char[] { '\n' });
                foreach (string tempText in tempNewLineText)
                {
                    if (!string.IsNullOrEmpty(tempText))
                    {
                        string addListText = RemoveSpeciCharacter(new string[] { "\\s" }, tempText);

                        if (options.RemoveNumber)
                        {
                            addListText = RemoveSpeciCharacter(new string[] { "[0-9]" }, addListText);
                        }

                        if (options.RemoveSquare)
                        {
                            addListText = RemoveSpeciCharacter(
                                new string[] { "\\(.+?\\)|\\（.+?\\）|\\(\\)|\\（\\）" },
                                addListText);
                        }

                        if (options.NameReplace && !string.IsNullOrEmpty(options.SearchText))
                        {
                            if (options.NameReplaceMode)
                            {
                                try
                                {
                                    addListText = Regex.Replace(
                                        addListText,
                                        options.SearchText,
                                        options.ReplaceText,
                                        RegexOptions.Multiline);
                                }
                                catch
                                {
                                    // 忽略正则错误
                                }
                            }
                            else
                            {
                                addListText = addListText.Replace(options.SearchText, options.ReplaceText);
                            }
                        }

                        if (!string.IsNullOrEmpty(addListText))
                        {
                            textList.Add(addListText);
                        }
                    }
                }
            }

            returnTextList = textList.ToArray();

            if (options.NameStroke)
            {
                try
                {
                    CultureInfo cultureInfo = new CultureInfo(133124);
                    Thread.CurrentThread.CurrentCulture = cultureInfo;
                    Array.Sort(returnTextList);
                }
                catch
                {
                    // 忽略排序错误（如不支持的区域性）
                }
            }

            return returnTextList;
        }

        private string RemoveSpeciCharacter(string[] specialCharacters, string inputText)
        {
            foreach (string specialChar in specialCharacters)
            {
                inputText = Regex.Replace(inputText, specialChar, string.Empty);
            }
            return inputText;
        }
    }
}
