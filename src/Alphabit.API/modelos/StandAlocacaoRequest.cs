namespace Alphabit.API.modelos;

public class StandAlocacaoRequest
{
    public bool Reservado { get; set; }
    public string Setor { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string TipoArea { get; set; } = "Stand";
    public double AreaX { get; set; }
    public double AreaY { get; set; }
    public double AreaLargura { get; set; } = 12;
    public double AreaAltura { get; set; } = 8;
    public decimal AreaMetrosQuadrados { get; set; }
    public decimal PrecoPorMetroQuadrado { get; set; }
    public decimal PrecoFixo { get; set; }
    public string NomeOcupante { get; set; } = string.Empty;
    public string TipoOcupante { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
}
