namespace Alphabit.API.modelos;

public class Atividade
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int LimiteParticipantes { get; set; }
}
