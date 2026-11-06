namespace DocumentTools.Core.Services.Text;

/// <summary>Port of legacy <c>Util.ConvertMoney2ChineseYulan</c>.</summary>
public interface IMoneyChineseService
{
    /// <summary>Converts a money amount to Chinese uppercase RMB form (e.g. 壹佰元整).</summary>
    string ToChineseUpper(decimal amount);

    /// <summary>Parses text then converts; returns empty string when input invalid.</summary>
    string ToChineseUpper(string? amountText);
}
