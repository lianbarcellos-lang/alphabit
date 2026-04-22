using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class LoginInputModel
{
    [Required(ErrorMessage = "Informe o CPF.")]
    public string Cpf { get; set; } = string.Empty;
}
