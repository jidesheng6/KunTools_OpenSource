using DocumentTools.Core.Services.Text;

namespace DocumentTools.Tests;

public class RandomIdCardServiceTests
{
    private readonly RandomIdCardService _sut = new();

    [Fact]
    public void Provinces_AreLoadedFromEmbeddedResource()
    {
        var provinces = _sut.Provinces;
        Assert.NotEmpty(provinces);
        Assert.Contains(provinces, p => p.Name.Contains("北京"));
    }

    [Fact]
    public void Cities_And_Districts_Resolve()
    {
        var cities = _sut.Cities("11");
        Assert.NotEmpty(cities);
        var districts = _sut.Districts("11", cities[0].Code);
        Assert.NotEmpty(districts);
        Assert.All(districts, d => Assert.Equal(6, d.Code.Length));
    }

    [Fact]
    public void Generate_ProducesValidIds_WithChosenDistrictPrefix()
    {
        var batch = _sut.Generate("110101", 20, onlyAdult: false, withNames: true);

        Assert.Equal(20, batch.Count);
        Assert.All(batch, item =>
        {
            Assert.StartsWith("110101", item.Id);
            Assert.True(ChineseIdCard.IsValid(item.Id), item.Id);
            Assert.False(string.IsNullOrWhiteSpace(item.Name));
        });
    }

    [Fact]
    public void Generate_OnlyAdult_KeepsPeopleAtLeast18()
    {
        var batch = _sut.Generate("110101", 30, onlyAdult: true, withNames: false);
        int cutoff = DateTime.Now.Year - 18;
        Assert.All(batch, item =>
        {
            int year = int.Parse(item.Id.Substring(6, 4));
            Assert.True(year <= cutoff, item.Id);
            Assert.Null(item.Name);
        });
    }

    [Fact]
    public void Generate_InvalidDistrict_Throws()
    {
        Assert.Throws<ArgumentException>(() => _sut.Generate("abc", 1, false, false));
    }
}
