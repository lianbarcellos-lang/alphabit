using Alphabit.API;
using Alphabit.API.modelos;

namespace Alphabit.Tests;

public class AlphabitRulesTests
{
    [Fact]
    public void HashPassword_ShouldReturnDeterministicSha256()
    {
        var expected = "8C6976E5B5410415BDE908BD4DEE15DFB167A9C873FC4BB8A81F6F2AB448A918";

        var actual = AlphabitRules.HashPassword("admin");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void IsAdminCredential_ShouldReturnTrue_ForDefaultAdmin()
    {
        var actual = AlphabitRules.IsAdminCredential("admin", "admin", "admin", "admin");

        Assert.True(actual);
    }

    [Fact]
    public void IsAdminCredential_ShouldReturnFalse_ForWrongPassword()
    {
        var actual = AlphabitRules.IsAdminCredential("admin", "1234", "admin", "admin");

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

        var actual = AlphabitRules.IsValidUserRegistration(request);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidEvent_ShouldReturnTrue_ForCompleteEvent()
    {
        var evento = new Evento
        {
            Nome = "Anime Friends Experience",
            LocalEvento = "Distrito Anhembi",
            CidadeEvento = "Sao Paulo",
            Artista = "Cosplayers convidados",
            GeneroMusical = "Anime",
            CapacidadeTotal = 18000,
            DataEvento = new DateTime(2026, 6, 20, 17, 0, 0),
            PrecoPadrao = 249.90m,
            ImagemUrl = "https://images.example.com/anime.jpg"
        };

        var actual = AlphabitRules.IsValidEvent(evento);

        Assert.True(actual);
    }

    [Fact]
    public void IsValidEvent_ShouldReturnFalse_WhenArtistIsMissing()
    {
        var evento = new Evento
        {
            Nome = "Cosplay Summit Brasil",
            LocalEvento = "Espaco Unimed",
            CidadeEvento = "Sao Paulo",
            Artista = "",
            GeneroMusical = "Cosplay",
            CapacidadeTotal = 12000,
            DataEvento = new DateTime(2026, 8, 8, 20, 30, 0),
            PrecoPadrao = 219.90m
        };

        var actual = AlphabitRules.IsValidEvent(evento);

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

        var actual = AlphabitRules.IsValidCoupon(cupom);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidCoupon_ShouldReturnFalse_WhenDiscountIsAboveOneHundred()
    {
        var cupom = new Cupom
        {
            Codigo = "PROMO101",
            PorcentagemDesconto = 101,
            ValorMinimoRegra = 50
        };

        var actual = AlphabitRules.IsValidCoupon(cupom);

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
                    Quantidade = 1,
                    Assentos = ["A1"]
                }
            ]
        };

        var actual = AlphabitRules.IsValidCheckout(request);

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

        var actual = AlphabitRules.IsValidCheckout(request);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidCheckout_ShouldReturnFalse_WhenSeatCountDoesNotMatchQuantity()
    {
        var request = new ReservaCheckoutRequest
        {
            UsuarioCpf = "12345678900",
            Itens =
            [
                new ReservaCheckoutItemRequest
                {
                    EventoId = 1,
                    Quantidade = 2,
                    Assentos = ["A1"]
                }
            ]
        };

        var actual = AlphabitRules.IsValidCheckout(request);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidCheckout_ShouldReturnFalse_WhenSeatsAreDuplicated()
    {
        var request = new ReservaCheckoutRequest
        {
            UsuarioCpf = "12345678900",
            Itens =
            [
                new ReservaCheckoutItemRequest
                {
                    EventoId = 1,
                    Quantidade = 2,
                    Assentos = ["A1", "a1"]
                }
            ]
        };

        var actual = AlphabitRules.IsValidCheckout(request);

        Assert.False(actual);
    }

    [Fact]
    public void IsValidCheckout_ShouldReturnFalse_WhenSameSeatAppearsInDifferentTicketTypes()
    {
        var request = new ReservaCheckoutRequest
        {
            UsuarioCpf = "12345678900",
            Itens =
            [
                new ReservaCheckoutItemRequest
                {
                    EventoId = 1,
                    TipoIngressoId = 10,
                    Quantidade = 1,
                    Assentos = ["A1"]
                },
                new ReservaCheckoutItemRequest
                {
                    EventoId = 1,
                    TipoIngressoId = 11,
                    Quantidade = 1,
                    Assentos = ["a1"]
                }
            ]
        };

        var actual = AlphabitRules.IsValidCheckout(request);

        Assert.False(actual);
    }
}
