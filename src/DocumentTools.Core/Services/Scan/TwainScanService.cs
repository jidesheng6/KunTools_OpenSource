using System.Reflection;
using NTwain;
using NTwain.Data;
using SkiaSharp;

namespace DocumentTools.Core.Services.Scan;

/// <summary>
/// NTwain-backed scanner service. Falls back to unavailable when DSM/driver missing.
/// </summary>
public sealed class TwainScanService : ITwainScanService, IDisposable
{
    private TwainSession? _session;
    private string _unavailable = string.Empty;
    private bool _initTried;

    public bool IsAvailable
    {
        get
        {
            EnsureSession();
            return _session is not null;
        }
    }

    public string UnavailableReason
    {
        get
        {
            EnsureSession();

            // 会话能建起来就说明 Twain 可用：此时再回「未初始化 Twain。」会误导用户
            // （可用设备列表就在旁边）。只有真正失败时才给原因，调用方用空串自行降级文案。
            if (_session is not null && string.IsNullOrEmpty(_unavailable))
            {
                return string.Empty;
            }

            return string.IsNullOrEmpty(_unavailable)
                ? "未初始化 Twain。"
                : _unavailable;
        }
    }

    public IReadOnlyList<TwainSourceInfo> ListSources()
    {
        EnsureSession();
        if (_session is null)
        {
            return Array.Empty<TwainSourceInfo>();
        }

        try
        {
            return _session.GetSources()
                .Select(s => new TwainSourceInfo(s.Name, s.Name, s.ProductFamily))
                .ToList();
        }
        catch (Exception ex)
        {
            _unavailable = "枚举扫描仪失败：" + ex.Message;
            return Array.Empty<TwainSourceInfo>();
        }
    }

    public IReadOnlyList<string> Scan(string sourceId, TwainScanOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
        {
            throw new ArgumentException("请先选择扫描保存路径。");
        }

        Directory.CreateDirectory(options.OutputDirectory);
        EnsureSession();
        if (_session is null)
        {
            throw new NotSupportedException(UnavailableReason);
        }

        DataSource? source = _session.GetSources()
            .FirstOrDefault(s => string.Equals(s.Name, sourceId, StringComparison.OrdinalIgnoreCase));
        if (source is null)
        {
            throw new InvalidOperationException("未找到扫描仪：" + sourceId);
        }

        var outputs = new List<string>();
        var done = new ManualResetEventSlim(false);
        Exception? transferError = null;

        void OnData(object? sender, DataTransferredEventArgs e)
        {
            try
            {
                string file = Path.Combine(
                    options.OutputDirectory,
                    $"scan_{DateTime.Now:yyyyMMdd_HHmmss_fff}.jpg");

                if (!string.IsNullOrEmpty(e.FileDataPath) && File.Exists(e.FileDataPath))
                {
                    File.Copy(e.FileDataPath, file, overwrite: true);
                    outputs.Add(file);
                    return;
                }

                // Native/Memory transfer: let SkiaSharp decode whatever the DSM hands back.
                using Stream? stream = e.GetNativeImageStream();
                if (stream is null)
                {
                    return;
                }

                using SKBitmap? bitmap = SKBitmap.Decode(stream);
                if (bitmap is null)
                {
                    return;
                }

                using (SKImage image = SKImage.FromBitmap(bitmap))
                using (SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 92))
                using (FileStream fs = File.Create(file))
                {
                    data.SaveTo(fs);
                }

                outputs.Add(file);
            }
            catch (Exception ex)
            {
                transferError = ex;
            }
        }

        void OnDisabled(object? sender, EventArgs e) => done.Set();

        _session.DataTransferred += OnData;
        _session.SourceDisabled += OnDisabled;
        try
        {
            if (source.Open() != ReturnCode.Success)
            {
                throw new InvalidOperationException("无法打开扫描仪数据源。");
            }

            try
            {
                if (source.Capabilities.ICapXResolution.IsSupported)
                {
                    source.Capabilities.ICapXResolution.SetValue(options.Dpi);
                }

                if (source.Capabilities.ICapYResolution.IsSupported)
                {
                    source.Capabilities.ICapYResolution.SetValue(options.Dpi);
                }
            }
            catch
            {
                // Resolution remains best-effort for compatibility with drivers that advertise
                // the capability but reject a particular DPI.
            }

            // These switches are visible in the UI, so they must never be silently ignored.
            // Apply both true and false when supported: a TWAIN source may retain the value
            // selected by its previous session.
            TwainCapabilityApplicator.Apply(new NTwainCapabilityTarget(source), options);

            SourceEnableMode mode = options.ShowUi ? SourceEnableMode.ShowUI : SourceEnableMode.NoUI;
            if (source.Enable(mode, modal: true, windowHandle: IntPtr.Zero) != ReturnCode.Success)
            {
                throw new InvalidOperationException("启动扫描失败（Enable 返回失败）。");
            }

            if (!done.Wait(TimeSpan.FromMinutes(5)))
            {
                throw new TimeoutException("扫描超时。");
            }

            if (transferError is not null)
            {
                throw transferError;
            }

            return outputs;
        }
        finally
        {
            _session.DataTransferred -= OnData;
            _session.SourceDisabled -= OnDisabled;
            try
            {
                source.Close();
            }
            catch
            {
                // ignore
            }
        }
    }

    public void Dispose()
    {
        try
        {
            _session?.Close();
        }
        catch
        {
            // ignore
        }

        _session = null;
    }

    /// <summary>
    /// Builds the TWAIN identity from assembly metadata without touching <see cref="Assembly.Location"/>.
    /// Single-file publish leaves Location empty, and NTwain's CreateFromAssembly would then call
    /// Path.GetFileNameWithoutExtension("") and throw ArgumentException("The path is empty.").
    /// </summary>
    private static TWIdentity CreateIdentity()
    {
        AssemblyName name = Assembly.GetExecutingAssembly().GetName();
        string product = string.IsNullOrWhiteSpace(name.Name) ? "DocumentTools" : name.Name;
        return TWIdentity.Create(
            DataGroups.Image,
            name.Version ?? new Version(1, 0),
            product,
            product,
            product,
            product);
    }

    private void EnsureSession()
    {
        if (_initTried)
        {
            return;
        }

        _initTried = true;
        try
        {
            TWIdentity id = CreateIdentity();
            var session = new TwainSession(id);
            ReturnCode rc = session.Open();
            if (rc != ReturnCode.Success)
            {
                _unavailable = $"打开 Twain Session 失败：{rc}。请确认已安装 TWAINDSM.dll 与扫描仪驱动。";
                session.Close();
                return;
            }

            _session = session;
            _unavailable = string.Empty;
        }
        catch (Exception ex)
        {
            _unavailable =
                "无法加载 Twain（NTwain/DSM）：" + ex.Message +
                "。请安装扫描仪驱动与 TWAIN Data Source Manager。";
            _session = null;
        }
    }
}
