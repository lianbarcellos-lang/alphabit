namespace Alphabit.App.Models;

public class EventReviewRequest
{
    public int EventoId { get; set; }
    public string UsuarioCpf { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
}
