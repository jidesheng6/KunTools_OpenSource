namespace DocumentTools.Core.Services.System;

public sealed record NetworkAdapterInfo(
    string Name,
    string Description,
    string Status,
    string Type,
    string? Ipv4,
    string? SubnetMask,
    string? Gateway,
    string? Dns1,
    string? Dns2,
    bool DhcpEnabled,
    string? Mac = null);

public interface INetworkAdapterService
{
    IReadOnlyList<NetworkAdapterInfo> ListAdapters();

    void SetEnabled(string descriptionOrName, bool enabled);

    void ApplyStatic(
        string descriptionOrName,
        string ip,
        string mask,
        string gateway,
        string? dns1,
        string? dns2);

    void ApplyDhcp(string descriptionOrName);
}
