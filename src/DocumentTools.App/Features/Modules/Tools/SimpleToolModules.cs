using DocumentTools.App.Features.Abstractions;

namespace DocumentTools.App.Features.Modules.Tools;

/// <summary>
/// Aggregator for remaining tool modules (Factory registration entry).
/// Prefer adding new features as dedicated files under this folder — do not grow this file.
/// </summary>
public static class SimpleToolModules
{
    public static IEnumerable<IFeatureModule> CreateAll()
    {
        yield return new DataMatrixFeatureModule();
        foreach (var replace in ReplaceFeatureModule.CreateAll())
        {
            yield return replace;
        }

        foreach (var misc in MiscToolModules.CreateAll())
        {
            yield return misc;
        }
    }
}
