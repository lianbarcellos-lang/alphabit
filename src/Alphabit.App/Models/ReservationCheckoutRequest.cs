namespace Alphabit.App.Models;

public class ReservationCheckoutRequest
{
    public string UsuarioCpf { get; set; } = string.Empty;
    public string? CupomCodigo { get; set; }
    public string? FormaPagamento { get; set; }
    public List<ReservationCheckoutItemRequest> Itens { get; set; } = [];
}
