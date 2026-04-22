using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class CustomerLoginInputModel
{
    [Required(ErrorMessage = "Informe seu email ou CPF.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe sua senha.")]
    public string Senha { get; set; } = string.Empty;
}
