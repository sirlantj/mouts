using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Exceptions;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string CustomerExternalId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string BranchExternalId { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public SaleStatus Status { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    private readonly List<SaleItem> _items = new();
    public IReadOnlyCollection<SaleItem> Items => _items.AsReadOnly();

    public Sale()
    {
        CreatedAt = DateTime.UtcNow;
        Status = SaleStatus.Active;
    }

    public SaleItem AddItem(string productExternalId, string productName, int quantity, decimal unitPrice)
    {
        var item = new SaleItem
        {
            SaleId = Id,
            ProductExternalId = productExternalId,
            ProductName = productName,
            Quantity = quantity,
            UnitPrice = unitPrice
        };

        item.CalculateDiscount();
        _items.Add(item);
        RecalculateTotal();
        return item;
    }

    public void CancelItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Item with id {itemId} not found in this sale.");

        if (item.IsCancelled)
            throw new DomainException("Item is already cancelled.");

        item.Cancel();
        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        EnsureActive("cancel");
        Status = SaleStatus.Cancelled;
        foreach (var item in _items.Where(i => !i.IsCancelled))
            item.Cancel();
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive => Status == SaleStatus.Active;

    /// <summary>
    /// Guards write operations: throws if the sale is not active.
    /// </summary>
    public void EnsureActive(string operation)
    {
        if (!IsActive)
            throw new DomainException($"Cannot {operation} a cancelled sale.");
    }

    public void RecalculateTotal()
    {
        TotalAmount = _items.Where(i => !i.IsCancelled).Sum(i => i.TotalAmount);
    }

    public void SetItems(List<SaleItem> items)
    {
        _items.Clear();
        _items.AddRange(items);
        RecalculateTotal();
    }
}
