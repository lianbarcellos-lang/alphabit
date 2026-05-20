namespace Alphabit.App.Models;

public class ReservationViewModel
{
    public int Id { get; set; }
    public string UsuarioCpf { get; set; } = string.Empty;
    public int EventoId { get; set; }
    public string EventoNome { get; set; } = string.Empty;
    public int? TipoIngressoId { get; set; }
    public string TipoIngressoNome { get; set; } = string.Empty;
    public string CupomUtilizado { get; set; } = string.Empty;
    public string Assentos { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal ValorFinalPago { get; set; }
    public string FormaPagamento { get; set; } = "Pix";
    public string StatusPagamento { get; set; } = "Pago";
    public string CodigoPedido { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string CheckinStatus { get; set; } = "Pendente";
    public DateTime? DataCheckin { get; set; }
    public DateTime DataReserva { get; set; }
}
