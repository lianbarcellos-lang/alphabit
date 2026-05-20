namespace Alphabit.API.modelos;

public class AdminSalesDashboardResponse
{
    public int TotalEventos { get; set; }
    public int TotalReservas { get; set; }
    public decimal ReceitaTotal { get; set; }
    public decimal ValorPendente { get; set; }
    public decimal TotalVendidoHoje { get; set; }
    public int TotalIngressosVendidos { get; set; }
    public int ReservasPagas { get; set; }
    public int ReservasPendentesPagamento { get; set; }
    public int ReservasCanceladas { get; set; }
    public int CheckinsRealizados { get; set; }
    public int CapacidadeTotal { get; set; }
    public int CapacidadeRestante { get; set; }
    public int CuponsUtilizados { get; set; }
    public int TotalAvaliacoes { get; set; }
    public decimal MediaAvaliacoes { get; set; }
    public decimal TaxaOcupacaoPercentual { get; set; }
    public decimal TaxaCheckinPercentual { get; set; }
    public int PagamentosPix { get; set; }
    public int PagamentosCartão { get; set; }
    public int PagamentosBoleto { get; set; }
    public List<AdminSaleResponse> Compras { get; set; } = [];
    public List<AdminSaleResponse> UltimasCompras { get; set; } = [];
    public List<AdminReviewResponse> UltimasAvaliacoes { get; set; } = [];
    public List<AdminTopEventResponse> EventosMaisVendidos { get; set; } = [];
}

public class AdminSaleResponse
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public string ClienteCpf { get; set; } = string.Empty;
    public string Evento { get; set; } = string.Empty;
    public DateTime DataCompra { get; set; }
    public string FormaPagamento { get; set; } = "Pix";
    public string StatusPagamento { get; set; } = "Pago";
    public decimal ValorPago { get; set; }
    public int QuantidadeIngressos { get; set; }
    public string SetorCadeiraLote { get; set; } = string.Empty;
    public string CodigoPedido { get; set; } = string.Empty;
}

public class AdminTopEventResponse
{
    public string Evento { get; set; } = string.Empty;
    public int IngressosVendidos { get; set; }
    public decimal ValorArrecadado { get; set; }
}

public class AdminReviewResponse
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    public string Evento { get; set; } = string.Empty;
    public string UsuarioNome { get; set; } = string.Empty;
    public string UsuarioCpf { get; set; } = string.Empty;
    public int Nota { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; }
}
