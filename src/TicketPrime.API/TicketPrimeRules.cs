using System.Security.Cryptography;
using System.Text;
using TicketPrime.API.modelos;

namespace TicketPrime.API;

public static class TicketPrimeRules
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool IsAdminCredential(string login, string senha)
    {
        return login == "admin" && senha == "admin";
    }

    public static bool IsValidUserRegistration(UsuarioCadastroRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.Cpf) &&
               !string.IsNullOrWhiteSpace(request.Nome) &&
               !string.IsNullOrWhiteSpace(request.Email) &&
               !string.IsNullOrWhiteSpace(request.Senha);
    }

    public static bool IsValidEvent(Evento evento)
    {
        return !string.IsNullOrWhiteSpace(evento.Nome) &&
               !string.IsNullOrWhiteSpace(evento.LocalEvento) &&
               !string.IsNullOrWhiteSpace(evento.CidadeEvento) &&
               !string.IsNullOrWhiteSpace(evento.Artista) &&
               !string.IsNullOrWhiteSpace(evento.GeneroMusical) &&
               evento.CapacidadeTotal > 0 &&
               evento.PrecoPadrao >= 0;
    }

    public static bool IsValidCoupon(Cupom cupom)
    {
        return !string.IsNullOrWhiteSpace(cupom.Codigo) &&
               cupom.PorcentagemDesconto > 0 &&
               cupom.ValorMinimoRegra >= 0;
    }

    public static bool IsValidCheckout(ReservaCheckoutRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.UsuarioCpf) &&
               request.Itens.Count > 0 &&
               request.Itens.All(item =>
                   item.EventoId > 0 &&
                   item.Quantidade > 0 &&
                   item.Assentos.Count == item.Quantidade &&
                   item.Assentos.All(seat => !string.IsNullOrWhiteSpace(seat)));
    }
}
