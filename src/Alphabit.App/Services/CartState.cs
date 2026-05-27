using Alphabit.App.Models;

namespace Alphabit.App.Services;

public class CartState
{
    private readonly List<CartItemViewModel> items = [];

    public IReadOnlyList<CartItemViewModel> Items => items;
    public int TotalItems => items.Sum(item => item.Quantidade);
    public decimal TotalValue => items.Sum(item => item.Subtotal);

    public void AddEvent(EventViewModel eventItem, IReadOnlyCollection<string> assentos, TicketTypeViewModel? ticketType)
    {
        AddEvent(eventItem, assentos.Count, ticketType, assentos);
    }

    public void AddEvent(EventViewModel eventItem, int quantidade, TicketTypeViewModel? ticketType)
    {
        AddEvent(eventItem, quantidade, ticketType, []);
    }

    private void AddEvent(EventViewModel eventItem, int quantidade, TicketTypeViewModel? ticketType, IReadOnlyCollection<string> assentos)
    {
        if (quantidade <= 0)
            return;

        var existing = items.FirstOrDefault(item =>
            item.EventoId == eventItem.Id &&
            item.TipoIngressoId == ticketType?.Id);
        if (existing is not null)
        {
            existing.Assentos = assentos.Distinct().OrderBy(item => item).ToList();
            existing.Quantidade = quantidade;
            return;
        }

        items.Add(new CartItemViewModel
        {
            EventoId = eventItem.Id,
            TipoIngressoId = ticketType?.Id,
            TipoIngressoNome = ticketType?.Nome ?? "Normal",
            TipoIngressoBeneficios = ticketType?.Beneficios ?? string.Empty,
            Nome = eventItem.Nome,
            DataEvento = eventItem.DataEvento,
            PrecoPadrao = eventItem.PrecoPadrao,
            PrecoUnitario = ticketType?.Preco ?? eventItem.PrecoPadrao,
            ImagemUrl = eventItem.ImagemUrl,
            Assentos = assentos.Distinct().OrderBy(item => item).ToList(),
            Quantidade = quantidade
        });
    }

    public void UpdateQuantity(int eventoId, int? tipoIngressoId, int quantidade)
    {
        var item = items.FirstOrDefault(current =>
            current.EventoId == eventoId &&
            current.TipoIngressoId == tipoIngressoId);
        if (item is null)
            return;

        item.Quantidade = Math.Clamp(quantidade, 1, 2);
    }

    public void RemoveItem(int eventoId, int? tipoIngressoId = null)
    {
        var item = items.FirstOrDefault(current =>
            current.EventoId == eventoId &&
            current.TipoIngressoId == tipoIngressoId);
        if (item is not null)
            items.Remove(item);
    }

    public void Clear()
    {
        items.Clear();
    }
}
