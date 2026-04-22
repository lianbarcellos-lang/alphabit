namespace TicketPrime.App.Models;

public class AdminEventUpdateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string LocalEvento { get; set; } = string.Empty;
    public string CidadeEvento { get; set; } = string.Empty;
    public string Artista { get; set; } = string.Empty;
    public string GeneroMusical { get; set; } = string.Empty;
    public int CapacidadeTotal { get; set; }
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public string? ImagemUrl { get; set; }
}
