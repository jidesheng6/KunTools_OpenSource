using System.Text.RegularExpressions;

namespace DocumentTools.Core.Services.Text;

/// <summary>Result of validating a mainland China resident ID number (mirrors legacy Util.ValidateChinaCard).</summary>
public sealed record ChineseIdCardResult(
    string Birthday,
    string Sex,
    string Sequence,
    bool IsChecksumValid,
    string ExpectedChecksum);

/// <summary>China mainland resident ID number checksum helper.</summary>
public static class ChineseIdCard
{
    private static readonly int[] Weights = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2];
    private static readonly char[] CheckChars = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];

    public static bool IsValid(string? id) => Validate(id).IsChecksumValid;

    /// <summary>给前 17 位补上校验位，返回完整 18 位身份证号。前 17 位必须是数字。</summary>
    public static string AppendChecksum(string first17)
    {
        if (first17.Length != 17 || !Regex.IsMatch(first17, @"^\d{17}$"))
        {
            throw new ArgumentException("身份证前 17 位必须是 17 个数字。", nameof(first17));
        }

        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += (first17[i] - '0') * Weights[i];
        }

        return first17 + CheckChars[sum % 11];
    }

    public static ChineseIdCardResult Validate(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return new ChineseIdCardResult(string.Empty, string.Empty, string.Empty, false, string.Empty);
        }

        id = id.Trim().ToUpperInvariant();
        if (id.Length != 18 || !Regex.IsMatch(id, @"^\d{17}[\dX]$"))
        {
            return new ChineseIdCardResult(string.Empty, string.Empty, string.Empty, false, string.Empty);
        }

        string birthday;
        try
        {
            int year = int.Parse(id.Substring(6, 4));
            int month = int.Parse(id.Substring(10, 2));
            int day = int.Parse(id.Substring(12, 2));
            if (year is < 1900 or > 2100 || month is < 1 or > 12 || day is < 1 or > 31)
            {
                birthday = "日期不正确";
            }
            else
            {
                birthday = new DateTime(year, month, day).ToString("yyyy年MM月dd日");
            }
        }
        catch
        {
            birthday = "日期不正确";
        }

        string sequence = id.Substring(14, 3);
        int sexDigit = id[16] - '0';
        string sex = sexDigit % 2 == 0 ? "女" : "男";

        int sum = 0;
        for (int i = 0; i < 17; i++)
        {
            sum += (id[i] - '0') * Weights[i];
        }

        char expected = CheckChars[sum % 11];
        bool ok = expected == id[17];
        return new ChineseIdCardResult(birthday, sex, sequence, ok, expected.ToString());
    }
}
