using DocumentTools.Core.Features;
using DocumentTools.Core.Features.Options;

namespace DocumentTools.Tests;

public class FeatureOptionSchemasTests
{
    [Fact]
    public void WordPdf_HasEmbedFontsKey()
    {
        var sections = FeatureOptionSchemas.GetSections("word.pdf");
        Assert.Contains(sections.SelectMany(s => s.Fields), f => f.Key == "embedFonts");
    }

    [Fact]
    public void AllConvertFeatures_HaveSchemaOrEmptyMerge()
    {
        foreach (var feature in BuiltInFeatures.ConvertFeatures)
        {
            var sections = FeatureOptionSchemas.GetSections(feature.Id);
            Assert.NotNull(sections);
            _ = FeatureOptionSchemas.GetFileFilter(feature.Id);
        }
    }

    [Fact]
    public void Choice_DefaultValue_IsStableValueNotOnlyLabel()
    {
        var field = FeatureOptionSchemas.GetSections("word.pdf")
            .SelectMany(s => s.Fields)
            .First(f => f.Key == "compliance");
        Assert.Equal(OptionFieldKind.Choice, field.Kind);
        Assert.Equal("Pdf17", field.DefaultValue);
        Assert.Contains(field.Choices!, c => c.Value == "Pdf17");
    }
}
