namespace DocumentTools.Core.Licensing;

/// <summary>
/// 查询当前进程的 Aspose 授权状态。
/// 本构建不内置授权（2026-11-05 决定：不再随产物分发 .lic，也不再读 <c>ASPOSE_LICENSE_PATH</c>），
/// 因此正常情况下恒为评估模式。只保留「exe 目录下 licenses\*.lic 若存在则尝试加载」这一条被动探测：
/// 调用方（关于页、QA 工具）要继续看得到 <see cref="AsposeLicenseStatus.IsLicensed"/>，
/// 但产品里不再出现任何「去这里放授权文件 / 设环境变量」的引导。
/// </summary>
public static class AsposeLicenseLoader
{
    /// <summary>
    /// 尝试加载 exe 目录 <c>licenses\*.lic</c>；没有可用授权时返回 <see cref="AsposeLicenseStatus.Evaluation"/>。
    /// </summary>
    public static AsposeLicenseStatus TryLoad(string? baseDirectory = null)
    {
        string root = baseDirectory ?? AppContext.BaseDirectory;
        string licensesDirectory = Path.Combine(root, "licenses");
        if (!Directory.Exists(licensesDirectory))
        {
            return AsposeLicenseStatus.Evaluation;
        }

        foreach (string lic in Directory.EnumerateFiles(licensesDirectory, "*.lic"))
        {
            if (Apply(lic))
            {
                return AsposeLicenseStatus.Loaded(lic);
            }
        }

        return AsposeLicenseStatus.Evaluation;
    }

    private static bool Apply(string licensePath)
    {
        try
        {
            new Aspose.Words.License().SetLicense(licensePath);
            new Aspose.Pdf.License().SetLicense(licensePath);
            new Aspose.Cells.License().SetLicense(licensePath);
            new Aspose.Slides.License().SetLicense(licensePath);
            new Aspose.BarCode.License().SetLicense(licensePath);
            TrySetLicense(() => new Aspose.Html.License().SetLicense(licensePath));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void TrySetLicense(Action apply)
    {
        try
        {
            apply();
        }
        catch
        {
            // Optional product license may be absent in a multi-product .lic file.
        }
    }
}

public sealed class AsposeLicenseStatus
{
    private AsposeLicenseStatus(bool isLicensed, string? path)
    {
        IsLicensed = isLicensed;
        LicensePath = path;
    }

    public bool IsLicensed { get; }

    public string? LicensePath { get; }

    public static AsposeLicenseStatus Loaded(string path) => new(true, path);

    public static AsposeLicenseStatus Evaluation { get; } = new(false, null);
}
