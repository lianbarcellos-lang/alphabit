using System.ComponentModel.DataAnnotations;

namespace Alphabit.App.Models;

public class AdminEventEditModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome do evento.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o local do evento.")]
    public string LocalEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade do evento.")]
    public string CidadeEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a atração principal do evento.")]
    public string Artista { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a categoria geek.")]
    public string GeneroMusical { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "A capacidade deve ser maior que zero.")]
    public int CapacidadeTotal { get; set; }

    public DateTime DataEvento { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Informe um valor valido.")]
    public decimal PrecoPadrao { get; set; }

    [Required(ErrorMessage = "Informe a URL da imagem.")]
    public string ImagemUrl { get; set; } = string.Empty;

    public bool EhDestaque { get; set; }
}
