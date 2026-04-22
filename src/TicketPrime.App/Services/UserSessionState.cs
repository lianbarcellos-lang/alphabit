using TicketPrime.App.Models;

namespace TicketPrime.App.Services;

public class UserSessionState
{
    public event Action? OnChange;

    public UserViewModel? CurrentUser { get; private set; }
    public bool IsAdmin { get; private set; }
    public bool IsAuthenticated => CurrentUser is not null || IsAdmin;
    public string? AdminToken { get; private set; }
    public string DisplayName => IsAdmin ? "Administrador TicketPrime" : CurrentUser?.Nome ?? string.Empty;

    public void SetUser(UserViewModel user)
    {
        IsAdmin = false;
        AdminToken = null;
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void UpdateUser(UserViewModel user)
    {
        CurrentUser = user;
        NotifyStateChanged();
    }

    public void SetAdmin(string token)
    {
        IsAdmin = true;
        AdminToken = token;
        CurrentUser = null;
        NotifyStateChanged();
    }

    public void Logout()
    {
        IsAdmin = false;
        AdminToken = null;
        CurrentUser = null;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
