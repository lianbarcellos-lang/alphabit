namespace TicketPrime.API.modelos;

public class ReservaCheckoutRequest
{
    public string UsuarioCpf { get; set; } = string.Empty;
    public string? CupomCodigo { get; set; }
    public List<ReservaCheckoutItemRequest> Itens { get; set; } = [];
}
