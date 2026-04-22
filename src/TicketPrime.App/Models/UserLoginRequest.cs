namespace TicketPrime.App.Models;

public class UserLoginRequest
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
