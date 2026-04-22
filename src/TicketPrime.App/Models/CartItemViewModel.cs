namespace TicketPrime.App.Models;

public class CartItemViewModel
{
    public int EventoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public string? ImagemUrl { get; set; }
    public List<string> Assentos { get; set; } = [];
    public int Quantidade { get; set; }
    public decimal Subtotal => PrecoPadrao * Quantidade;
}
