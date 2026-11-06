using NTwain;
using NTwain.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DocumentTools.Tests")]

namespace DocumentTools.Core.Services.Scan;

/// <summary>
/// Applies the user-visible scanner switches to an opened TWAIN data source.
/// The small target abstraction keeps the policy testable without scanner hardware.
/// </summary>
internal static class TwainCapabilityApplicator
{
    public static void Apply(ITwainCapabilityTarget target, TwainScanOptions options)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(options);

        ApplyDuplex(target, options.Duplex);
        ApplyBlankPageDiscard(target, options.SkipBlankPages);
    }

    private static void ApplyDuplex(ITwainCapabilityTarget target, bool requested)
    {
        if (!target.CanSetDuplex)
        {
            if (requested)
            {
                throw new NotSupportedException(
                    "当前扫描仪或驱动不支持双面扫描，无法应用“双面”选项。请取消勾选后重试，或在扫描仪界面中确认设备能力。");
            }

            return;
        }

        target.SetDuplex(requested);
    }

    private static void ApplyBlankPageDiscard(ITwainCapabilityTarget target, bool requested)
    {
        if (!target.CanSetBlankPageDiscard)
        {
            if (requested)
            {
                throw new NotSupportedException(
                    "当前扫描仪或驱动不支持自动丢弃空白页，无法应用“跳过空白页”选项。请取消勾选后重试。");
            }

            return;
        }

        target.SetBlankPageDiscard(requested);
    }
}

internal interface ITwainCapabilityTarget
{
    bool CanSetDuplex { get; }

    bool CanSetBlankPageDiscard { get; }

    void SetDuplex(bool enabled);

    void SetBlankPageDiscard(bool enabled);
}

/// <summary>NTwain adapter for capability policy; the source must already be open.</summary>
internal sealed class NTwainCapabilityTarget(DataSource source) : ITwainCapabilityTarget
{
    private readonly DataSource _source = source ?? throw new ArgumentNullException(nameof(source));

    public bool CanSetDuplex
        => _source.Capabilities.CapDuplex.IsSupported
           && _source.Capabilities.CapDuplexEnabled.IsSupported
           && _source.Capabilities.CapDuplexEnabled.CanSet;

    public bool CanSetBlankPageDiscard
        => _source.Capabilities.ICapAutoDiscardBlankPages.IsSupported
           && _source.Capabilities.ICapAutoDiscardBlankPages.CanSet;

    public void SetDuplex(bool enabled)
    {
        string action = enabled ? "启用双面扫描" : "关闭双面扫描";
        try
        {
            ReturnCode result = _source.Capabilities.CapDuplexEnabled.SetValue(
                enabled ? BoolType.True : BoolType.False);
            EnsureSuccess(result, action);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"扫描仪驱动无法{action}：{ex.Message}。请调整选项或打开扫描仪界面后重试。", ex);
        }
    }

    public void SetBlankPageDiscard(bool enabled)
    {
        string action = enabled ? "启用跳过空白页" : "关闭跳过空白页";
        try
        {
            ReturnCode result = _source.Capabilities.ICapAutoDiscardBlankPages.SetValue(
                enabled ? BlankPage.Auto : BlankPage.Disable);
            EnsureSuccess(result, action);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"扫描仪驱动无法{action}：{ex.Message}。请调整选项或打开扫描仪界面后重试。", ex);
        }
    }

    private static void EnsureSuccess(ReturnCode result, string action)
    {
        if (result != ReturnCode.Success)
        {
            throw new InvalidOperationException($"扫描仪驱动拒绝{action}（TWAIN 返回 {result}）。请调整选项或打开扫描仪界面后重试。");
        }
    }
}
