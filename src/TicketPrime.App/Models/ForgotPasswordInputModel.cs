using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class ForgotPasswordInputModel
{
    [Required(ErrorMessage = "Informe seu e-mail ou CPF.")]
    public string Login { get; set; } = string.Empty;
}
