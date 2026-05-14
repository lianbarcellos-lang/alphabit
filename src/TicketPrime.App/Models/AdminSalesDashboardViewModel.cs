namespace TicketPrime.App.Models;

public class AdminSalesDashboardViewModel
{
    public decimal TotalVendidoHoje { get; set; }
    public int TotalIngressosVendidos { get; set; }
    public int PagamentosPix { get; set; }
    public int PagamentosCartao { get; set; }
    public int PagamentosBoleto { get; set; }
    public List<AdminSaleViewModel> Compras { get; set; } = [];
    public List<AdminSaleViewModel> UltimasCompras { get; set; } = [];
    public List<AdminTopShowViewModel> ShowsMaisVendidos { get; set; } = [];
}

public class AdminSaleViewModel
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

public class AdminTopShowViewModel
{
    public string Show { get; set; } = string.Empty;
    public int IngressosVendidos { get; set; }
    public decimal ValorArrecadado { get; set; }
}
