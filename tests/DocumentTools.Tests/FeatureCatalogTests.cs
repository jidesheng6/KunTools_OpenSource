using DocumentTools.Core.Features;

namespace DocumentTools.Tests;

public class FeatureCatalogTests
{
    [Fact]
    public void Find_Returns_Known_Feature()
    {
        var catalog = new FeatureCatalog();
        var feature = catalog.Find("word.pdf");
        Assert.NotNull(feature);
        Assert.Equal(FeatureCategory.Word, feature!.Category);
    }

    [Fact]
    public void Search_Matches_Keyword()
    {
        var catalog = new FeatureCatalog();
        var hits = catalog.Search("w2p");
        Assert.Contains(hits, f => f.Id == "word.pdf");
    }

    [Fact]
    public void NameVertical_Distinct()
    {
        var service = new DocumentTools.Core.Services.Text.NameVerticalService();
        string result = service.ToVertical("张三 张三 李四", distinct: true);
        Assert.Contains("张", result);
        Assert.Contains("李", result);
    }
}
