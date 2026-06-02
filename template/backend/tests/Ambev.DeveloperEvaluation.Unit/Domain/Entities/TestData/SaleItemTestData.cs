using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleItemTestData
{
    public static SaleItem GenerateValidItem(int quantity = 5, decimal unitPrice = 100m)
    {
        var faker = new Faker();
        var item = new SaleItem
        {
            Id = Guid.NewGuid(),
            SaleId = Guid.NewGuid(),
            ProductExternalId = Guid.NewGuid().ToString(),
            ProductName = faker.Commerce.ProductName(),
            Quantity = quantity,
            UnitPrice = unitPrice
        };
        item.CalculateDiscount();
        return item;
    }
}
