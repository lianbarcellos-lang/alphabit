namespace Alphabit.API.modelos;

public class AvaliacaoRequest
{
    public int EventoId { get; set; }
    public string UsuarioCpf { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
}
