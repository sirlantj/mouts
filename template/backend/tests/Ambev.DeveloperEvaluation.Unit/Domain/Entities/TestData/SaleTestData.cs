using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

public static class SaleTestData
{
    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.Id, f => Guid.NewGuid())
        .RuleFor(s => s.SaleNumber, f => $"SALE-{f.Random.Number(1000, 9999)}")
        .RuleFor(s => s.SaleDate, f => f.Date.Recent())
        .RuleFor(s => s.CustomerExternalId, f => Guid.NewGuid().ToString())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchExternalId, f => Guid.NewGuid().ToString())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName());

    public static Sale GenerateValidSale()
    {
        return SaleFaker.Generate();
    }

    public static Sale GenerateValidSaleWithItems(int itemCount = 2, int quantityPerItem = 5)
    {
        var sale = SaleFaker.Generate();
        for (int i = 0; i < itemCount; i++)
        {
            sale.AddItem(
                Guid.NewGuid().ToString(),
                new Faker().Commerce.ProductName(),
                quantityPerItem,
                decimal.Parse(new Faker().Commerce.Price(10, 100)));
        }
        return sale;
    }
}
