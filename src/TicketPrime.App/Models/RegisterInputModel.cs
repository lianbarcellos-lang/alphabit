using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class RegisterInputModel
{
    [Required(ErrorMessage = "Informe o CPF.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o nome.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o email.")]
    [EmailAddress(ErrorMessage = "Informe um email valido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe uma senha.")]
    [MinLength(4, ErrorMessage = "Use pelo menos 4 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}
