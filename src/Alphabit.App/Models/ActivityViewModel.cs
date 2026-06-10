namespace Alphabit.App.Models;

public class ActivityViewModel
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime Horario { get; set; }
    public DateTime HorarioFim { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int LimiteParticipantes { get; set; }
    public int Inscritos { get; set; }
    public int VagasRestantes { get; set; }
    public bool UsuarioInscrito { get; set; }
    public int UsuarioQuantidade { get; set; }
    public string UsuarioAssentos { get; set; } = string.Empty;
    public string AssentosOcupados { get; set; } = string.Empty;
}
