namespace TicketPrime.API.modelos;

public class AdminSalesDashboardResponse
{
    public decimal TotalVendidoHoje { get; set; }
    public int TotalIngressosVendidos { get; set; }
    public int PagamentosPix { get; set; }
    public int PagamentosCartao { get; set; }
    public int PagamentosBoleto { get; set; }
    public List<AdminSaleResponse> Compras { get; set; } = [];
    public List<AdminSaleResponse> UltimasCompras { get; set; } = [];
    public List<AdminTopShowResponse> ShowsMaisVendidos { get; set; } = [];
}

public class AdminSaleResponse
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteCpf { get; set; } = string.Empty;
    public string Show { get; set; } = string.Empty;
    public DateTime DataCompra { get; set; }
    public string FormaPagamento { get; set; } = "Pix";
    public string StatusPagamento { get; set; } = "Pago";
    public decimal ValorPago { get; set; }
    public int QuantidadeIngressos { get; set; }
    public string SetorCadeiraLote { get; set; } = string.Empty;
    public string CodigoPedido { get; set; } = string.Empty;
}

public class AdminTopShowResponse
{
    public string Show { get; set; } = string.Empty;
    public int IngressosVendidos { get; set; }
    public decimal ValorArrecadado { get; set; }
}
