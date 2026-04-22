namespace TicketPrime.App.Models;

public class SeatAvailabilityViewModel
{
    public int EventoId { get; set; }
    public List<string> AssentosOcupados { get; set; } = [];
}
