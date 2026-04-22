using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class CustomerProfileViewModel
{
    [Required]
    public string Cpf { get; set; } = string.Empty;

    [Required]
    public string Nome { get; set; } = string.Empty;

    public string Sobrenome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string PaisResidencia { get; set; } = "Brasil";
    public string TipoDocumento { get; set; } = "CPF";
    public string CodigoPais { get; set; } = "+55";
    public string Telefone { get; set; } = string.Empty;
    public DateTime? DataNascimento { get; set; }
    public string Sexo { get; set; } = string.Empty;
}
