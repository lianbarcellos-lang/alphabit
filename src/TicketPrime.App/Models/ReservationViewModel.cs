namespace TicketPrime.App.Models;

public class ReservationViewModel
{
    public int Id { get; set; }
    public string UsuarioCpf { get; set; } = string.Empty;
    public int EventoId { get; set; }
    public string EventoNome { get; set; } = string.Empty;
    public string CupomUtilizado { get; set; } = string.Empty;
    public string Assentos { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorFinalPago { get; set; }
    public DateTime DataReserva { get; set; }
}
