namespace Alphabit.API.modelos;

public class AtividadeInscricaoRequest
{
    public string UsuarioCpf { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public string Assentos { get; set; } = string.Empty;
}
