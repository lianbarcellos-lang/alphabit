namespace Alphabit.App.Models;

public class ActivityCreateRequest
{
    public int EventoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime Horario { get; set; } = DateTime.Today.AddHours(18);
    public DateTime HorarioFim { get; set; } = DateTime.Today.AddHours(19);
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int LimiteParticipantes { get; set; } = 20;
}
