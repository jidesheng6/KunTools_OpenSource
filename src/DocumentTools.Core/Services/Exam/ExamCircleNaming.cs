using DocumentTools.Core.Common;

namespace DocumentTools.Core.Services.Exam;

/// <summary>
/// 考评表图片另存时的文件名规则：修复件加 <c>_fixed</c>，旋转件加 <c>_r{角度}</c>，
/// 自动转正加 <c>_auto{角度}</c>；同名时追加 <c>_1</c>/<c>_2</c>。
/// 这是业务命名契约（产出的文件名要能被后续脚本认出），所以放在 Core，
/// 视图只把结果交给保存逻辑，不得自己拼 <c>stem + suffix + ext</c>。
/// </summary>
public static class ExamCircleNaming
{
    /// <summary>修复件的后缀。</summary>
    public const string FixedSuffix = "_fixed";

    /// <summary>按指定角度手动旋转另存的后缀。</summary>
    public static string RotationSuffix(int degrees) => $"_r{degrees}";

    /// <summary>自动转正另存的后缀。</summary>
    public static string AutoRotationSuffix(int degrees) => $"_auto{degrees}";

    /// <summary>修复件的默认文件名（另存对话框初值）：原主名 + <see cref="FixedSuffix"/> + 原扩展名。</summary>
    public static string FixedFileName(string sourcePath)
        => Path.GetFileNameWithoutExtension(sourcePath) + FixedSuffix + Path.GetExtension(sourcePath);

    /// <summary>给另存路径挑一个不冲突的名字（<paramref name="sourcePath"/> 同目录已有同名时递增 _1/_2）。</summary>
    public static string UniquePath(string sourcePath, string suffix)
        => UniqueFileName.For(sourcePath, suffix);
}
