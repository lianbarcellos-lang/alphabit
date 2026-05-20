namespace Alphabit.API.modelos;

public class CheckinResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int ReservaId { get; set; }
    public string EventoNome { get; set; } = string.Empty;
    public string ClienteCpf { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? DataCheckin { get; set; }
}
