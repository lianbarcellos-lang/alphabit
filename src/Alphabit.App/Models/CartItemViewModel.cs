namespace Alphabit.App.Models;

public class CartItemViewModel
{
    public int EventoId { get; set; }
    public int? TipoIngressoId { get; set; }
    public string TipoIngressoNome { get; set; } = "Normal";
    public string TipoIngressoBeneficios { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public DateTime DataEvento { get; set; }
    public decimal PrecoPadrao { get; set; }
    public decimal PrecoUnitario { get; set; }
    public string? ImagemUrl { get; set; }
    public List<string> Assentos { get; set; } = [];
    public int Quantidade { get; set; }
    public decimal Subtotal => PrecoUnitario * Quantidade;
}
