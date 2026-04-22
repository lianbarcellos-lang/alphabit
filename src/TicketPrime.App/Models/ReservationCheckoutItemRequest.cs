namespace TicketPrime.App.Models;

public class ReservationCheckoutItemRequest
{
    public int EventoId { get; set; }
    public int Quantidade { get; set; }
    public List<string> Assentos { get; set; } = [];
}
