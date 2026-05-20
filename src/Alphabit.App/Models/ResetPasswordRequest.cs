namespace Alphabit.App.Models;

public class ResetPasswordRequest
{
    public string Login { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
