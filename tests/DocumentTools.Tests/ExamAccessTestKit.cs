using DocumentTools.Core.Data;
using DocumentTools.ExamFixture;
using SkiaSharp;

namespace DocumentTools.Tests;

/// <summary>
/// 考评 Access 集成测试入口前提：本机必须真的能建/读 .mdb。
/// xunit 2.9 没有 <c>Assert.Skip</c>（那是 v3 的 API），所以跳过信息放在 FactAttribute 的 Skip 上，
/// 该值在发现阶段求值；ACE 缺失的机器上这些用例整体跳过，并在测试报告里显示具体原因，不会报红。
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AceFactAttribute : FactAttribute
{
    public AceFactAttribute()
    {
        if (!AceAvailability.Available)
        {
            Skip = AceAvailability.Reason;
        }
    }
}

/// <summary>考评 Access 集成测试共用的夹具与断言辅助。</summary>
internal static class ExamAccessTestKit
{
    /// <summary>新建一套临时夹具（Exam_Base.mdb），用完请 Dispose。</summary>
    public static ExamFixtureSet NewFixture(ExamFixtureOptions? options = null)
        => ExamFixtureBuilder.Create(ExamFixtureBuilder.CreateRootDirectory(), options);

    /// <summary>造一张可确定性复现的小图（白底黑块），不需要任何外部素材。</summary>
    public static string CreateImage(string filePath, int width, int height)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = false };
            canvas.DrawRect(SKRect.Create(width / 4f, height / 4f, width / 2f, height / 2f), paint);
        }

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(filePath, data.ToArray());
        return filePath;
    }

    /// <summary>把 Query 结果压成便于断言的行字典（列名 → 值），避免测试里到处 IndexOf 列。</summary>
    public static List<Dictionary<string, object?>> Rows(AccessDb session, string sql)
    {
        var list = new List<Dictionary<string, object?>>();
        global::System.Data.DataTable table = session.Query(sql);
        foreach (global::System.Data.DataRow row in table.Rows)
        {
            var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (global::System.Data.DataColumn column in table.Columns)
            {
                object? value = row[column];
                values[column.ColumnName] = value is DBNull ? null : value;
            }

            list.Add(values);
        }

        return list;
    }

    public static int Count(AccessDb session, string sql)
        => Convert.ToInt32(session.Scalar(sql) ?? 0);

    public static string Text(Dictionary<string, object?> row, string column)
        => Convert.ToString(row[column]) ?? string.Empty;
}
