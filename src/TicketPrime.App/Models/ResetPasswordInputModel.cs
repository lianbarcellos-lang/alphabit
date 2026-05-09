using System.ComponentModel.DataAnnotations;

namespace TicketPrime.App.Models;

public class ResetPasswordInputModel
{
    public string Login { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o codigo enviado por e-mail.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a nova senha.")]
    [MinLength(4, ErrorMessage = "Use pelo menos 4 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme a nova senha.")]
    [MinLength(4, ErrorMessage = "Use pelo menos 4 caracteres.")]
    public string ConfirmarSenha { get; set; } = string.Empty;
}
