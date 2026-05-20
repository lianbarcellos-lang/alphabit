namespace Alphabit.API.modelos;

public class ReservaCheckoutItemRequest
{
    public int EventoId { get; set; }
    public int? TipoIngressoId { get; set; }
    public int Quantidade { get; set; }
    public List<string> Assentos { get; set; } = [];
}
