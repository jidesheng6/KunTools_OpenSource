using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DocumentTools.Core.Services.System;

/// <summary>一张网卡的静态配置快照（导出 / 加载用）。</summary>
public sealed record NetworkConfig(string Ip, string Mask, string Gateway, string Dns1, string Dns2)
{
    public static readonly NetworkConfig Empty = new("0.0.0.0", "0.0.0.0", "0.0.0.0", "0.0.0.0", "0.0.0.0");
}

/// <summary>
/// 网卡配置文件（<c>.KUNNET</c>）的读写。键名与 legacy <c>set_interface_export/load</c> 一致：
/// <c>IPADDRESS / IPMASK / IPGATEWAY / DNS1 / DNS2</c>。把「文件格式」知识从视图搬到 Core。
/// </summary>
public static class NetworkConfigFile
{
    public const string Extension = ".KUNNET";
    public const string Filter = "坤Tools网卡配置(*.KUNNET)|*.KUNNET|所有文件|*.*";

    private const string KeyIp = "IPADDRESS";
    private const string KeyMask = "IPMASK";
    private const string KeyGateway = "IPGATEWAY";
    private const string KeyDns1 = "DNS1";
    private const string KeyDns2 = "DNS2";

    public static void Save(string path, NetworkConfig config)
    {
        var payload = new Dictionary<string, string>
        {
            [KeyIp] = config.Ip,
            [KeyMask] = config.Mask,
            [KeyGateway] = config.Gateway,
            [KeyDns1] = config.Dns1,
            [KeyDns2] = config.Dns2
        };

        string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static NetworkConfig Load(string path)
    {
        string content = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidDataException("配置文件内无内容，配置未加载。");
        }

        // 容错解析：legacy 早期用单/裸键写盘，新实现写标准 JSON；两种都按「键 → 值」正则抽取。
        return new NetworkConfig(
            Extract(content, KeyIp),
            Extract(content, KeyMask),
            Extract(content, KeyGateway),
            Extract(content, KeyDns1),
            Extract(content, KeyDns2));
    }

    private static string Extract(string content, string key)
    {
        Match match = Regex.Match(
            content,
            $"[\"']?{Regex.Escape(key)}[\"']?\\s*:\\s*[\"']?(?<value>[^\"',\\r\\n}}]+)",
            RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["value"].Value.Trim() : "0.0.0.0";
    }
}
