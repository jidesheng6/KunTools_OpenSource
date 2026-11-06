using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.Versioning;

namespace DocumentTools.Core.Services.System;

/// <summary>Port of Util.SetInterfaceInformation / SetInterfaceStatus (WMI + netsh fallback).</summary>
[SupportedOSPlatform("windows")]
public sealed class NetworkAdapterService : INetworkAdapterService
{
    public IReadOnlyList<NetworkAdapterInfo> ListAdapters()
    {
        var list = new List<NetworkAdapterInfo>();
        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            var props = nic.GetIPProperties();
            var ipv4 = props.UnicastAddresses
                .FirstOrDefault(a => a.Address.AddressFamily == global::System.Net.Sockets.AddressFamily.InterNetwork);
            var gateway = props.GatewayAddresses
                .FirstOrDefault(g => g.Address.AddressFamily == global::System.Net.Sockets.AddressFamily.InterNetwork)
                ?.Address.ToString();
            var dns = props.DnsAddresses
                .Where(d => d.AddressFamily == global::System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(d => d.ToString())
                .ToList();

            bool dhcp = false;
            try
            {
                dhcp = props.GetIPv4Properties()?.IsDhcpEnabled ?? false;
            }
            catch
            {
                // some adapter types throw
            }

            list.Add(new NetworkAdapterInfo(
                nic.Name,
                nic.Description,
                nic.OperationalStatus.ToString(),
                nic.NetworkInterfaceType.ToString(),
                ipv4?.Address.ToString(),
                ipv4?.IPv4Mask?.ToString(),
                gateway,
                dns.ElementAtOrDefault(0),
                dns.ElementAtOrDefault(1),
                dhcp,
                FormatMac(nic.GetPhysicalAddress().GetAddressBytes())));
        }

        return list;
    }

    /// <summary>MAC 形如 AA-BB-CC-DD-EE-FF；空地址返回 null。</summary>
    private static string? FormatMac(byte[] bytes)
        => bytes.Length == 0 ? null : string.Join("-", bytes.Select(b => b.ToString("X2")));

    public void SetEnabled(string descriptionOrName, bool enabled)
    {
        string method = enabled ? "Enable" : "Disable";
        if (TryWmiAdapterMethod(descriptionOrName, method))
        {
            return;
        }

        // Fallback: netsh by interface name
        string interfaceName = ResolveInterfaceName(descriptionOrName);
        RunNetsh($"interface set interface name=\"{Escape(interfaceName)}\" admin={(enabled ? "enabled" : "disabled")}");
    }

    public void ApplyStatic(
        string descriptionOrName,
        string ip,
        string mask,
        string gateway,
        string? dns1,
        string? dns2)
    {
        // WPF 页是普通 TextBox，没有 legacy SysIPAddress32 的段限字符；
        // 不先校验就会把 "999.1.1.1" 直接送进 netsh，报回来的错误用户看不懂。
        NetworkAddressValidator.ValidateStaticArguments(ip, mask, gateway, dns1, dns2);

        if (TryWmiSetStatic(descriptionOrName, ip.Trim(), mask.Trim(), gateway?.Trim() ?? string.Empty, dns1, dns2))
        {
            return;
        }

        string name = ResolveInterfaceName(descriptionOrName);
        RunNetsh($"interface ip set address name=\"{Escape(name)}\" static {ip.Trim()} {mask.Trim()} {(string.IsNullOrWhiteSpace(gateway) ? "" : gateway.Trim())}".Trim());
        if (!string.IsNullOrWhiteSpace(dns1))
        {
            RunNetsh($"interface ip set dns name=\"{Escape(name)}\" static {dns1.Trim()}");
            if (!string.IsNullOrWhiteSpace(dns2))
            {
                RunNetsh($"interface ip add dns name=\"{Escape(name)}\" {dns2.Trim()} index=2");
            }
        }
    }

    public void ApplyDhcp(string descriptionOrName)
    {
        if (TryWmiSetDhcp(descriptionOrName))
        {
            return;
        }

        string name = ResolveInterfaceName(descriptionOrName);
        RunNetsh($"interface ip set address name=\"{Escape(name)}\" dhcp");
        RunNetsh($"interface ip set dns name=\"{Escape(name)}\" dhcp");
    }

    private static bool TryWmiAdapterMethod(string descriptionOrName, string method)
    {
        try
        {
            string esc = EscapeWmi(descriptionOrName);
            using var searcher = new ManagementObjectSearcher(
                $"SELECT * FROM Win32_NetworkAdapter WHERE Description='{esc}' OR NetConnectionID='{esc}' OR Name='{esc}'");
            using ManagementObjectCollection list = searcher.Get();
            bool any = false;
            foreach (ManagementObject obj in list)
            {
                obj.InvokeMethod(method, null);
                any = true;
            }

            return any;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryWmiSetStatic(
        string descriptionOrName,
        string ip,
        string mask,
        string gateway,
        string? dns1,
        string? dns2)
    {
        try
        {
            using var cls = new ManagementClass("Win32_NetworkAdapterConfiguration");
            using ManagementObjectCollection objects = cls.GetInstances();
            foreach (ManagementObject obj in objects)
            {
                if (obj["IPEnabled"] is not true)
                {
                    continue;
                }

                string desc = obj["Description"]?.ToString() ?? string.Empty;
                if (!Matches(desc, descriptionOrName))
                {
                    continue;
                }

                var staticParams = obj.GetMethodParameters("EnableStatic");
                staticParams["IPAddress"] = new[] { ip };
                staticParams["SubnetMask"] = new[] { mask };
                obj.InvokeMethod("EnableStatic", staticParams, null);

                if (!string.IsNullOrWhiteSpace(gateway))
                {
                    var gw = obj.GetMethodParameters("SetGateways");
                    gw["DefaultIPGateway"] = new[] { gateway };
                    obj.InvokeMethod("SetGateways", gw, null);
                }

                var dns = obj.GetMethodParameters("SetDNSServerSearchOrder");
                var servers = new List<string>();
                if (!string.IsNullOrWhiteSpace(dns1) && dns1 != "0.0.0.0")
                {
                    servers.Add(dns1.Trim());
                }

                if (!string.IsNullOrWhiteSpace(dns2) && dns2 != "0.0.0.0")
                {
                    servers.Add(dns2.Trim());
                }

                dns["DNSServerSearchOrder"] = servers.Count > 0 ? servers.ToArray() : null;
                obj.InvokeMethod("SetDNSServerSearchOrder", dns, null);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool TryWmiSetDhcp(string descriptionOrName)
    {
        try
        {
            using var cls = new ManagementClass("Win32_NetworkAdapterConfiguration");
            using ManagementObjectCollection objects = cls.GetInstances();
            foreach (ManagementObject obj in objects)
            {
                if (obj["IPEnabled"] is not true)
                {
                    continue;
                }

                string desc = obj["Description"]?.ToString() ?? string.Empty;
                if (!Matches(desc, descriptionOrName))
                {
                    continue;
                }

                obj.InvokeMethod("EnableDHCP", obj.GetMethodParameters("EnableDHCP"), null);
                var dns = obj.GetMethodParameters("SetDNSServerSearchOrder");
                dns["DNSServerSearchOrder"] = null;
                obj.InvokeMethod("SetDNSServerSearchOrder", dns, null);
                return true;
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static string ResolveInterfaceName(string descriptionOrName)
    {
        NetworkInterface? match = NetworkInterface.GetAllNetworkInterfaces()
            .FirstOrDefault(n =>
                string.Equals(n.Description, descriptionOrName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(n.Name, descriptionOrName, StringComparison.OrdinalIgnoreCase));
        return match?.Name ?? descriptionOrName;
    }

    private static bool Matches(string description, string key)
        => string.Equals(description, key, StringComparison.OrdinalIgnoreCase) ||
           string.Equals(description, key, StringComparison.Ordinal);

    private static string EscapeWmi(string value) => value.Replace("'", "\\'", StringComparison.Ordinal);

    private static string Escape(string value) => value.Replace("\"", "\\\"", StringComparison.Ordinal);

    private static void RunNetsh(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "netsh",
            Arguments = args,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        using Process? p = Process.Start(psi);
        if (p is null)
        {
            throw new InvalidOperationException("无法启动 netsh。");
        }

        string stdout = p.StandardOutput.ReadToEnd();
        string stderr = p.StandardError.ReadToEnd();
        p.WaitForExit(15000);
        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "网卡操作失败（通常需要管理员权限）：" +
                (string.IsNullOrWhiteSpace(stderr) ? stdout : stderr).Trim());
        }
    }
}
