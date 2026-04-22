namespace TicketPrime.API.modelos;

public class ReservaCheckoutItemRequest
{
    public int EventoId { get; set; }
    public int Quantidade { get; set; }
    public List<string> Assentos { get; set; } = [];
}
