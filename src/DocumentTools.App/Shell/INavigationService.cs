namespace DocumentTools.App.Shell;

public interface INavigationService
{
    event Action<string>? FeatureRequested;

    void Navigate(string featureId);
}

public sealed class NavigationService : INavigationService
{
    public event Action<string>? FeatureRequested;

    public void Navigate(string featureId)
    {
        if (string.IsNullOrWhiteSpace(featureId))
        {
            return;
        }

        FeatureRequested?.Invoke(featureId.Trim());
    }
}
