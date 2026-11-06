namespace DocumentTools.Core.Services.Text;

/// <summary>Faithful port of legacy Util.ConvertMoney2ChineseYulan (no UI).</summary>
public sealed class MoneyChineseService : IMoneyChineseService
{
    public string ToChineseUpper(string? amountText)
    {
        if (string.IsNullOrWhiteSpace(amountText)
            || !decimal.TryParse(amountText.Trim(), out decimal amount)
            || amount <= 0)
        {
            return string.Empty;
        }

        return ToChineseUpper(amount);
    }

    public string ToChineseUpper(decimal num)
    {
        const string digits = "零壹贰叁肆伍陆柒捌玖";
        string units = "万仟佰拾亿仟佰拾万仟佰拾元角分";
        string result = string.Empty;
        int zeroRun = 0;

        num = Math.Round(Math.Abs(num), 2);
        string scaled = ((long)(num * 100)).ToString();
        int length = scaled.Length;
        if (length > 15)
        {
            return "溢出";
        }

        units = units[(15 - length)..];

        for (int i = 0; i < length; i++)
        {
            string digitChar = scaled.Substring(i, 1);
            int digit = int.Parse(digitChar);
            string ch1;
            string ch2;

            bool critical = i == length - 3 || i == length - 7 || i == length - 11 || i == length - 15;
            if (!critical)
            {
                if (digitChar == "0")
                {
                    ch1 = string.Empty;
                    ch2 = string.Empty;
                    zeroRun++;
                }
                else if (digitChar != "0" && zeroRun != 0)
                {
                    ch1 = "零" + digits.Substring(digit, 1);
                    ch2 = units.Substring(i, 1);
                    zeroRun = 0;
                }
                else
                {
                    ch1 = digits.Substring(digit, 1);
                    ch2 = units.Substring(i, 1);
                    zeroRun = 0;
                }
            }
            else if (digitChar != "0" && zeroRun != 0)
            {
                ch1 = "零" + digits.Substring(digit, 1);
                ch2 = units.Substring(i, 1);
                zeroRun = 0;
            }
            else if (digitChar != "0" && zeroRun == 0)
            {
                ch1 = digits.Substring(digit, 1);
                ch2 = units.Substring(i, 1);
                zeroRun = 0;
            }
            else if (digitChar == "0" && zeroRun >= 3)
            {
                ch1 = string.Empty;
                ch2 = string.Empty;
                zeroRun++;
            }
            else if (length >= 11)
            {
                ch1 = string.Empty;
                ch2 = string.Empty;
                zeroRun++;
            }
            else
            {
                ch1 = string.Empty;
                ch2 = units.Substring(i, 1);
                zeroRun++;
            }

            if (i == length - 11 || i == length - 3)
            {
                ch2 = units.Substring(i, 1);
            }

            result += ch1 + ch2;

            if (i == length - 1 && digitChar == "0")
            {
                result += "整";
            }
        }

        if (num == 0)
        {
            return "零元整";
        }

        return result;
    }
}
