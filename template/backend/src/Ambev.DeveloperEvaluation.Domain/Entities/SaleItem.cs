using Ambev.DeveloperEvaluation.Domain.Common;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleItem : BaseEntity
{
    public Guid SaleId { get; set; }
    public string ProductExternalId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public SaleItem()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void CalculateDiscount()
    {
        if (Quantity > 20)
            throw new DomainException("Cannot sell more than 20 identical items.");

        if (Quantity >= 10)
            Discount = 0.20m;
        else if (Quantity >= 4)
            Discount = 0.10m;
        else
            Discount = 0m;

        CalculateTotalAmount();
    }

    public void CalculateTotalAmount()
    {
        TotalAmount = Quantity * UnitPrice * (1 - Discount);
    }

    public void Cancel()
    {
        IsCancelled = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
