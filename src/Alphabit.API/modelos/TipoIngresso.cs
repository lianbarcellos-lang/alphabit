namespace Alphabit.API.modelos;

public class TipoIngresso
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Beneficios { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeDisponivel { get; set; }
}
