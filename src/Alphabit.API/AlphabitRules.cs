using System.Security.Cryptography;
using System.Text;
using Alphabit.API.modelos;

namespace Alphabit.API;

public static class AlphabitRules
{
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public static bool IsAdminCredential(string login, string senha, string expectedLogin, string expectedSenha)
    {
        return FixedTimeEquals(login, expectedLogin) && FixedTimeEquals(senha, expectedSenha);
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
               evento.DataEvento > DateTime.MinValue &&
               evento.PrecoPadrao >= 0;
    }

    public static bool IsValidCoupon(Cupom cupom)
    {
        return !string.IsNullOrWhiteSpace(cupom.Codigo) &&
               cupom.PorcentagemDesconto > 0 &&
               cupom.PorcentagemDesconto <= 100 &&
               cupom.ValorMinimoRegra >= 0;
    }

    public static bool IsValidCheckout(ReservaCheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UsuarioCpf) || request.Itens.Count == 0)
            return false;

        if (!request.Itens.All(item =>
                item.EventoId > 0 &&
                item.Quantidade > 0 &&
                (item.Assentos.Count == 0 || item.Assentos.Count == item.Quantidade) &&
                item.Assentos.All(seat => !string.IsNullOrWhiteSpace(seat)) &&
                item.Assentos.Distinct(StringComparer.OrdinalIgnoreCase).Count() == item.Assentos.Count))
            return false;

        return request.Itens
            .GroupBy(item => item.EventoId)
            .All(group =>
            {
                var seats = group
                    .SelectMany(item => item.Assentos)
                    .Select(seat => seat.Trim())
                    .ToList();

                return seats.Distinct(StringComparer.OrdinalIgnoreCase).Count() == seats.Count;
            });
    }

    private static bool FixedTimeEquals(string value, string expectedValue)
    {
        var valueBytes = Encoding.UTF8.GetBytes(value);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedValue);
        return CryptographicOperations.FixedTimeEquals(valueBytes, expectedBytes);
    }
}
