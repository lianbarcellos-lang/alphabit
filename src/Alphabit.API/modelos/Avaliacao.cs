namespace Alphabit.API.modelos;

public class Avaliacao
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public string UsuarioCpf { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
