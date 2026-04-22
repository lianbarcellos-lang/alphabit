namespace TicketPrime.API.modelos;

public class AuthResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? Nome { get; set; }
    public string? Cpf { get; set; }
    public string? Email { get; set; }
}
