using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

public class ItemCancelledEvent
{
    public Sale Sale { get; }
    public Guid ItemId { get; }

    public ItemCancelledEvent(Sale sale, Guid itemId)
    {
        Sale = sale;
        ItemId = itemId;
    }
}
