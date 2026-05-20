namespace Alphabit.App.Models;

public class PasswordRecoveryResponseViewModel : AuthResponseViewModel
{
    public string EmailMascarado { get; set; } = string.Empty;
}
