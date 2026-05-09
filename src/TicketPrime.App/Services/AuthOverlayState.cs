namespace TicketPrime.App.Services;

public class AuthOverlayState
{
    public event Action? OnChange;

    public bool IsOpen { get; private set; }
    public string Mode { get; private set; } = "login";

    public void OpenLogin()
    {
        IsOpen = true;
        Mode = "login";
        NotifyStateChanged();
    }

    public void OpenRegister()
    {
        IsOpen = true;
        Mode = "register";
        NotifyStateChanged();
    }

    public void OpenForgotPassword()
    {
        IsOpen = true;
        Mode = "forgot";
        NotifyStateChanged();
    }

    public void OpenResetPassword()
    {
        IsOpen = true;
        Mode = "reset";
        NotifyStateChanged();
    }

    public void Close()
    {
        IsOpen = false;
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
