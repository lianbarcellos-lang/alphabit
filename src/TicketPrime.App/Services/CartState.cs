using TicketPrime.App.Models;

namespace TicketPrime.App.Services;

public class CartState
{
    private readonly List<CartItemViewModel> items = [];

    public IReadOnlyList<CartItemViewModel> Items => items;
    public int TotalItems => items.Sum(item => item.Quantidade);
    public decimal TotalValue => items.Sum(item => item.Subtotal);

    public void AddEvent(EventViewModel eventItem, IReadOnlyCollection<string> assentos)
    {
        if (assentos.Count == 0)
            return;

        var existing = items.FirstOrDefault(item => item.EventoId == eventItem.Id);
        if (existing is not null)
        {
            existing.Assentos = assentos.Distinct().OrderBy(item => item).ToList();
            existing.Quantidade = existing.Assentos.Count;
            return;
        }

        items.Add(new CartItemViewModel
        {
            EventoId = eventItem.Id,
            Nome = eventItem.Nome,
            DataEvento = eventItem.DataEvento,
            PrecoPadrao = eventItem.PrecoPadrao,
            ImagemUrl = eventItem.ImagemUrl,
            Assentos = assentos.Distinct().OrderBy(item => item).ToList(),
            Quantidade = assentos.Count
        });
    }

    public void UpdateQuantity(int eventoId, int quantidade)
    {
        var item = items.FirstOrDefault(current => current.EventoId == eventoId);
        if (item is null)
            return;

        item.Quantidade = item.Assentos.Count;
    }

    public void RemoveItem(int eventoId)
    {
        var item = items.FirstOrDefault(current => current.EventoId == eventoId);
        if (item is not null)
            items.Remove(item);
    }

    public void Clear()
    {
        items.Clear();
    }
}
