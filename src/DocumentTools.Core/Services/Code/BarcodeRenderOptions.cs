namespace DocumentTools.Core.Services.Code;

/// <summary>UI-agnostic barcode appearance (legacy groupBox 外观 / 标题 / LOGO).</summary>
public sealed class BarcodeRenderOptions
{
    public string Symbology { get; init; } = "QR";

    public int ModuleSize { get; init; } = 4;

    public int ResolutionDpi { get; init; } = 96;

    public string? Caption { get; init; }

    /// <summary>none / above / below</summary>
    public string CaptionPosition { get; init; } = "none";

    public float CaptionFontSize { get; init; } = 12;

    public string? LogoFile { get; init; }

    /// <summary>
    /// 旋转角度，只接受 0 / 90 / 180 / 270（其它值按 0 处理）。
    /// 90 与 270 会交换输出宽高。
    /// </summary>
    public int Rotate { get; init; }

    /// <summary>auto（跟随条码内容自适应）/ fixed（按 <see cref="BarcodeWidth"/>/<see cref="BarcodeHeight"/> 输出）。</summary>
    public string SizeMode { get; init; } = "auto";

    /// <summary>固定尺寸模式的宽度（像素）；0 表示按高度等比缩放。</summary>
    public int BarcodeWidth { get; init; }

    /// <summary>固定尺寸模式的高度（像素）；0 表示按宽度等比缩放。</summary>
    public int BarcodeHeight { get; init; }
}
