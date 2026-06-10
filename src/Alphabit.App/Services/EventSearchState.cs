namespace Alphabit.App.Services;

public sealed class EventSearchState
{
    private string searchText = string.Empty;

    public event Action? OnChange;

    public string SearchText => searchText;

    public void SetSearch(string? value)
    {
        var nextValue = value?.TrimStart() ?? string.Empty;
        if (searchText == nextValue)
            return;

        searchText = nextValue;
        OnChange?.Invoke();
    }

    public void Clear()
    {
        SetSearch(string.Empty);
    }
}
