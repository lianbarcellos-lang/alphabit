using TicketPrime.API;
using TicketPrime.API.modelos;

namespace TicketPrime.Tests;

public class TicketPrimeRulesTests
{
    [Fact]
    public void HashPassword_ShouldReturnDeterministicSha256()
    {
        var expected = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918";

        var actual = TicketPrimeRules.HashPassword("admin");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsAdminCredential_ShouldReturnTrue_ForDefaultAdmin()
    {
        var actual = TicketPrimeRules.IsAdminCredential("admin", "admin");

        Assert.True(actual);
    }

    [Fact]
    public void IsAdminCredential_ShouldReturnFalse_ForWrongPassword()
    {
        var actual = TicketPrimeRules.IsAdminCredential("admin", "1234");

        Assert.False(actual);
    }

    [Fact]
    public void IsValidUserRegistration_ShouldReturnFalse_WhenPasswordIsMissing()
    {
        var request = new UsuarioCadastroRequest
        {
            Cpf = "12345678900",
            Nome = "Raphael",
            Email = "raphael@email.com",
            Senha = ""
        };

        var actual = TicketPrimeRules.IsValidUserRegistration(request);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidEvent_ShouldReturnTrue_ForCompleteEvent()
    {
        var evento = new Evento
        {
            Nome = "Festival Sunset Brasil",
            LocalEvento = "Allianz Parque",
            CidadeEvento = "Sao Paulo",
            Artista = "Line-up Sunset",
            GeneroMusical = "Festival",
            CapacidadeTotal = 18000,
            DataEvento = new DateTime(2026, 6, 20, 17, 0, 0),
            PrecoPadrao = 249.90m,
            ImagemUrl = "https://images.example.com/show.jpg"
        };

        var actual = TicketPrimeRules.IsValidEvent(evento);

        Assert.True(actual);
    }

    [Fact]
    public void IsValidEvent_ShouldReturnFalse_WhenArtistIsMissing()
    {
        var evento = new Evento
        {
            Nome = "Pop Experience Live",
            LocalEvento = "Espaco Unimed",
            CidadeEvento = "Sao Paulo",
            Artista = "",
            GeneroMusical = "Pop",
            CapacidadeTotal = 12000,
            DataEvento = new DateTime(2026, 8, 8, 20, 30, 0),
            PrecoPadrao = 219.90m
        };

        var actual = TicketPrimeRules.IsValidEvent(evento);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidCoupon_ShouldReturnFalse_WhenDiscountIsZero()
    {
        var cupom = new Cupom
        {
            Codigo = "PROMO0",
            PorcentagemDesconto = 0,
            ValorMinimoRegra = 50
        };

        var actual = TicketPrimeRules.IsValidCoupon(cupom);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidCheckout_ShouldReturnTrue_ForValidReservationRequest()
    {
        var request = new ReservaCheckoutRequest
        {
            UsuarioCpf = "12345678900",
            CupomCodigo = "PROMO10",
            Itens =
            [
                new ReservaCheckoutItemRequest
                {
                    EventoId = 1,
                    Quantidade = 1
                }
            ]
        };

        var actual = TicketPrimeRules.IsValidCheckout(request);

        Assert.True(actual);
    }

    [Fact]
    public void IsValidCheckout_ShouldReturnFalse_WhenItemQuantityIsInvalid()
    {
        var request = new ReservaCheckoutRequest
        {
            UsuarioCpf = "12345678900",
            Itens =
            [
                new ReservaCheckoutItemRequest
                {
                    EventoId = 1,
                    Quantidade = 0
                }
            ]
        };

        var actual = TicketPrimeRules.IsValidCheckout(request);

        Assert.False(actual);
    }
}
