namespace Alphabit.App.Models;

public class ActivitySignupRequest
{
    public string UsuarioCpf { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public string Assentos { get; set; } = string.Empty;
}
