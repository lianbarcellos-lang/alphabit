namespace TicketPrime.API.modelos
{
    public class Reserva
    {
        public int Id { get; set; }
        public string UsuarioCpf { get; set; } = string.Empty;
        public int EventoId { get; set; }
        public string? CupomUtilizado { get; set; }
        public string Assentos { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal ValorFinalPago { get; set; }
        public DateTime DataReserva { get; set; }
    }
}
