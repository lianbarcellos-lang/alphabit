namespace Alphabit.API.modelos;

public class UsuarioRedefinirSenhaRequest
{
    public string Login { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string NovaSenha { get; set; } = string.Empty;
}
