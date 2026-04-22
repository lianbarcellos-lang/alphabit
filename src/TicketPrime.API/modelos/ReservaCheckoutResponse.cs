namespace TicketPrime.API.modelos;

public class ReservaCheckoutResponse
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public string CupomAplicado { get; set; } = string.Empty;
    public decimal TotalOriginal { get; set; }
    public decimal DescontoAplicado { get; set; }
    public decimal TotalFinal { get; set; }
}
