using System.ComponentModel.DataAnnotations;

namespace Alphabit.App.Models;

public class LoginInputModel
{
    [Required(ErrorMessage = "Informe o CPF.")]
    public string Cpf { get; set; } = string.Empty;
}
