namespace DocumentTools.Core.Services.System;

/// <summary>
/// IPv4 参数校验。legacy 网卡页用的是 Win32 的 SysIPAddress32 公共控件
/// （<c>DocumentTools/IPAddressInputTextBox.cs</c>，四段 0–255 由控件自己拦住），
/// 新版是普通 TextBox，所以把「控件级别」的校验补回到服务层，让 WMI/netsh 拿到的一定是合法值。
/// </summary>
public static class NetworkAddressValidator
{
    /// <summary>校验 IP/掩码/网关/DNS 组合；不合法时抛 <see cref="ArgumentException"/>，消息可直接展示给用户。</summary>
    public static void ValidateStaticArguments(
        string? ip,
        string? mask,
        string? gateway,
        string? dns1,
        string? dns2)
    {
        if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(mask))
        {
            throw new ArgumentException("IP 与子网掩码不能为空。");
        }

        if (!TryParseIPv4(ip.Trim(), out uint ipValue))
        {
            throw new ArgumentException(IpMessage("IP 地址", ip));
        }

        if (!TryParseIPv4(mask.Trim(), out uint maskValue) || !IsContiguousMask(maskValue))
        {
            throw new ArgumentException(
                "子网掩码格式不正确：" + Quote(mask) + "。掩码必须是连续的 1 后接连续的 0，"
                + "例如 255.255.255.0（/24）或 255.255.252.0（/22）。");
        }

        if (!string.IsNullOrWhiteSpace(gateway) && !TryParseIPv4(gateway.Trim(), out _))
        {
            throw new ArgumentException(
                "网关格式不正确：" + Quote(gateway) + "。留空表示不设网关；"
                + "填写时按 4 段 0–255 的点分十进制，例如 192.168.1.1。");
        }

        if (!string.IsNullOrWhiteSpace(dns1) && !TryParseIPv4(dns1.Trim(), out _))
        {
            throw new ArgumentException(DnsMessage(dns1));
        }

        if (!string.IsNullOrWhiteSpace(dns2) && !TryParseIPv4(dns2.Trim(), out _))
        {
            throw new ArgumentException(DnsMessage(dns2));
        }

        // 掩码全 0 时网段无意义，netsh/WMI 也接受不了，提前拦下。
        if (maskValue == 0)
        {
            throw new ArgumentException(
                "子网掩码不能是 0.0.0.0。请至少填 255.0.0.0 这类有效掩码。");
        }

        // 掩码位与 IP 值的组合本身无需额外校验：host 位是否被置 1 由 Windows 自行归一。
        _ = ipValue;
    }

    /// <summary>严格 IPv4：必须 4 段、每段 0–255、不允许前导 0（避免 010 被当成八进制）。</summary>
    public static bool TryParseIPv4(string? text, out uint value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string[] parts = text.Trim().Split('.');
        if (parts.Length != 4)
        {
            return false;
        }

        uint result = 0;
        foreach (string part in parts)
        {
            if (part.Length == 0 || part.Length > 3)
            {
                return false;
            }

            if (part.Length > 1 && part[0] == '0')
            {
                return false;
            }

            foreach (char ch in part)
            {
                if (ch is < '0' or > '9')
                {
                    return false;
                }
            }

            int octet = int.Parse(part);
            if (octet > 255)
            {
                return false;
            }

            result = (result << 8) | (uint)octet;
        }

        value = result;
        return true;
    }

    /// <summary>掩码是否「连续的 1 后接连续的 0」。</summary>
    public static bool IsContiguousMask(uint mask)
    {
        // 取反后加一：合法掩码的补码是「若干 0 + 若干 1」，再减一应为 0。
        uint inverted = ~mask;
        return (inverted & (inverted + 1)) == 0;
    }

    /// <summary>掩码前缀长度（/0 到 /32），非法掩码返回 -1。</summary>
    public static int MaskPrefixLength(uint mask)
    {
        if (!IsContiguousMask(mask))
        {
            return -1;
        }

        int bits = 0;
        for (int i = 31; i >= 0; i--)
        {
            if ((mask & (1u << i)) == 0)
            {
                break;
            }

            bits++;
        }

        return bits;
    }

    private static string IpMessage(string label, string? value)
        => label + "格式不正确：" + Quote(value) + "。请按 4 段 0–255 的点分十进制填写，例如 192.168.1.10。";

    private static string DnsMessage(string? value)
        => "DNS 格式不正确：" + Quote(value) + "。留空表示不改 DNS；填写时按 4 段 0–255 的点分十进制，例如 223.5.5.5。";

    private static string Quote(string? value)
        => value is null ? "空" : "\"" + value + "\"";
}
