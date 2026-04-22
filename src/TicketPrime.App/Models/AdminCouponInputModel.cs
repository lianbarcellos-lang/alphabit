using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class AdminCouponInputModel
{
    [Required(ErrorMessage = "Informe o codigo do cupom.")]
    public string Codigo { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Informe um percentual valido.")]
    public decimal PorcentagemDesconto { get; set; } = 10;

    [Range(0, double.MaxValue, ErrorMessage = "Informe um valor minimo valido.")]
    public decimal ValorMinimoRegra { get; set; } = 50;
}
