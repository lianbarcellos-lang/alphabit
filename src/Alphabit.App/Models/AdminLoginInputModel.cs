using System.ComponentModel.DataAnnotations;

namespace Alphabit.App.Models;

public class AdminLoginInputModel
{
    [Required(ErrorMessage = "Informe o login do administrador.")]
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha do administrador.")]
    public string Senha { get; set; } = string.Empty;
}
