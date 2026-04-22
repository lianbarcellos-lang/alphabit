using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class AdminEventInputModel
{
    [Required(ErrorMessage = "Informe o nome do evento.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o local do evento.")]
    public string LocalEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a cidade do evento.")]
    public string CidadeEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o artista do evento.")]
    public string Artista { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o genero musical.")]
    public string GeneroMusical { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "A capacidade deve ser maior que zero.")]
    public int CapacidadeTotal { get; set; } = 1000;

    public DateTime DataEvento { get; set; } = DateTime.Now.AddDays(15);

    [Range(0.01, double.MaxValue, ErrorMessage = "Informe um valor valido.")]
    public decimal PrecoPadrao { get; set; } = 99.90m;

    [Required(ErrorMessage = "Informe a URL da imagem.")]
    public string ImagemUrl { get; set; } = string.Empty;
}
