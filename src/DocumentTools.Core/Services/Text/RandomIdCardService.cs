using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace DocumentTools.Core.Services.Text;

/// <summary>行政区节点（省 / 市 / 区县的编码与名称）。</summary>
public sealed record DivisionNode(string Code, string Name)
{
    public override string ToString() => Name;
}

/// <summary>生成的一条身份证号（可选附带随机姓名）。</summary>
public sealed record GeneratedIdCard(string Id, string? Name);

public interface IRandomIdCardService
{
    IReadOnlyList<DivisionNode> Provinces { get; }

    IReadOnlyList<DivisionNode> Cities(string provinceCode);

    IReadOnlyList<DivisionNode> Districts(string provinceCode, string cityCode);

    /// <summary>按区县编码批量生成合法身份证号；<paramref name="onlyAdult"/> 限定年满 18 岁，<paramref name="withNames"/> 同时生成姓名。</summary>
    IReadOnlyList<GeneratedIdCard> Generate(string districtCode, int count, bool onlyAdult, bool withNames);
}

/// <summary>
/// 随机身份证号码生成（对齐 legacy <c>Util.GenChinaCard / GenRandomBirthday / GenRandomIndex / GenRandomName</c>）。
/// 行政区数据来自内嵌 UTF-8 资源（构建期由 legacy GBK 资源转码），姓名用姓氏表 + 名字用字表随机拼接。
/// </summary>
public sealed class RandomIdCardService : IRandomIdCardService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true
    };

    private static readonly Lazy<IReadOnlyList<Province>> Divisions = new(LoadDivisions, isThreadSafe: true);
    private static readonly Lazy<string[]> Surnames = new(() => LoadChars("DocumentTools.Core.Resources.chinse_first.txt"), isThreadSafe: true);
    private static readonly Lazy<string[]> GivenChars = new(() => LoadChars("DocumentTools.Core.Resources.chinese_han.txt"), isThreadSafe: true);

    public IReadOnlyList<DivisionNode> Provinces
        => Divisions.Value.Select(p => new DivisionNode(p.Code, p.Name)).ToList();

    public IReadOnlyList<DivisionNode> Cities(string provinceCode)
        => FindProvince(provinceCode)?.Cities.Select(c => new DivisionNode(c.Code, c.Name)).ToList()
           ?? new List<DivisionNode>();

    public IReadOnlyList<DivisionNode> Districts(string provinceCode, string cityCode)
        => FindProvince(provinceCode)?.Cities.FirstOrDefault(c => c.Code == cityCode)?.Districts
           ?? Array.Empty<DivisionNode>();

    public IReadOnlyList<GeneratedIdCard> Generate(string districtCode, int count, bool onlyAdult, bool withNames)
    {
        if (string.IsNullOrWhiteSpace(districtCode) || districtCode.Length != 6 || !IsAllDigits(districtCode))
        {
            throw new ArgumentException("请选择一个有效的区县（编码需为 6 位数字）。", nameof(districtCode));
        }

        if (count < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "生成数量至少为 1。");
        }

        var result = new List<GeneratedIdCard>(count);
        for (int i = 0; i < count; i++)
        {
            string first17 = districtCode
                + RandomBirthday(onlyAdult)
                + RandomIndex();
            string id = ChineseIdCard.AppendChecksum(first17);
            result.Add(new GeneratedIdCard(id, withNames ? RandomName() : null));
        }

        return result;
    }

    private static Province? FindProvince(string code)
        => Divisions.Value.FirstOrDefault(p => p.Code == code);

    private static string RandomBirthday(bool onlyAdult)
    {
        int maxYear = DateTime.Now.Year - (onlyAdult ? 18 : 0);
        int year = Random.Shared.Next(1900, maxYear);
        int month = Random.Shared.Next(1, 13);
        int day = Random.Shared.Next(1, DateTime.DaysInMonth(year, month) + 1);
        return new DateTime(year, month, day).ToString("yyyyMMdd");
    }

    private static string RandomIndex() => Random.Shared.Next(1, 1000).ToString("D3");

    private static string RandomName()
    {
        string[] surnames = Surnames.Value;
        string[] chars = GivenChars.Value;
        if (surnames.Length == 0 || chars.Length == 0)
        {
            return string.Empty;
        }

        string surname = surnames[Random.Shared.Next(surnames.Length)];
        int givenLen = Random.Shared.Next(1, 3); // 1 或 2 个字
        var given = new StringBuilder();
        for (int i = 0; i < givenLen; i++)
        {
            given.Append(chars[Random.Shared.Next(chars.Length)]);
        }

        return surname + given;
    }

    private static bool IsAllDigits(string text)
    {
        foreach (char c in text)
        {
            if (c is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }

    // ---- 内嵌资源与数据模型 ----

    private sealed record City(string Code, string Name, IReadOnlyList<DivisionNode> Districts);
    private sealed record Province(string Code, string Name, IReadOnlyList<City> Cities);

    private sealed class RawDivision
    {
        public string? code { get; set; }
        public string? name { get; set; }
        public List<RawDivision>? children { get; set; }
    }

    private static IReadOnlyList<Province> LoadDivisions()
    {
        string json = ReadResource("DocumentTools.Core.Resources.china_level.json");
        List<RawDivision> raw = JsonSerializer.Deserialize<List<RawDivision>>(json, JsonOptions) ?? [];

        var provinces = new List<Province>(raw.Count);
        foreach (RawDivision p in raw)
        {
            var cities = new List<City>();
            foreach (RawDivision city in p.children ?? [])
            {
                var districts = (city.children ?? [])
                    .Select(d => new DivisionNode(d.code ?? string.Empty, d.name ?? string.Empty))
                    .Where(d => d.Code.Length == 6)
                    .ToList();
                cities.Add(new City(city.code ?? string.Empty, city.name ?? string.Empty, districts));
            }

            provinces.Add(new Province(p.code ?? string.Empty, p.name ?? string.Empty, cities));
        }

        return provinces;
    }

    private static string[] LoadChars(string resourceName)
        => ReadResource(resourceName)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string ReadResource(string logicalName)
    {
        Assembly assembly = typeof(RandomIdCardService).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(logicalName);
        if (stream is null)
        {
            throw new InvalidOperationException($"内嵌资源缺失：{logicalName}");
        }

        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
