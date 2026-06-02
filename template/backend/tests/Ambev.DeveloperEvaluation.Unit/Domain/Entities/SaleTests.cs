using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleTests
{
    [Fact(DisplayName = "New sale should have Active status")]
    public void Given_NewSale_When_Created_Then_StatusShouldBeActive()
    {
        var sale = new Sale();
        sale.Status.Should().Be(SaleStatus.Active);
    }

    [Fact(DisplayName = "Cancelled sale should have Cancelled status")]
    public void Given_ActiveSale_When_Cancelled_Then_StatusShouldBeCancelled()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.Cancel();
        sale.Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact(DisplayName = "All items should be cancelled when sale is cancelled")]
    public void Given_ActiveSale_When_Cancelled_Then_AllItemsShouldBeCancelled()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems(itemCount: 3);
        sale.Cancel();
        sale.Items.Should().AllSatisfy(item => item.IsCancelled.Should().BeTrue());
    }

    [Fact(DisplayName = "Total should be recalculated when item is cancelled")]
    public void Given_SaleWithItems_When_ItemCancelled_Then_TotalShouldBeRecalculated()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item1 = sale.AddItem("PROD-1", "Product 1", 5, 100m); // 450 (10% off)
        item1.Id = Guid.NewGuid();
        var item2 = sale.AddItem("PROD-2", "Product 2", 2, 50m);  // 100 (no discount)
        item2.Id = Guid.NewGuid();
        var totalBefore = sale.TotalAmount;
        totalBefore.Should().Be(550m);

        sale.CancelItem(item1.Id);

        sale.TotalAmount.Should().BeLessThan(totalBefore);
        sale.TotalAmount.Should().Be(100m);
    }

    [Fact(DisplayName = "Adding item should update total")]
    public void Given_Sale_When_AddingItem_Then_TotalShouldBeUpdated()
    {
        var sale = SaleTestData.GenerateValidSale();
        sale.TotalAmount.Should().Be(0m);

        sale.AddItem("PROD-1", "Product 1", 2, 100m);
        sale.TotalAmount.Should().Be(200m);

        sale.AddItem("PROD-2", "Product 2", 4, 50m); // 10% discount → 180
        sale.TotalAmount.Should().Be(380m);
    }

    [Fact(DisplayName = "Cancel item that does not exist should throw")]
    public void Given_Sale_When_CancellingNonExistentItem_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        var act = () => sale.CancelItem(Guid.NewGuid());
        act.Should().Throw<DomainException>().WithMessage("*not found*");
    }

    [Fact(DisplayName = "Cancel already cancelled item should throw")]
    public void Given_Sale_When_CancellingAlreadyCancelledItem_Then_ShouldThrow()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item = sale.AddItem("PROD-1", "Product 1", 2, 50m);
        item.Id = Guid.NewGuid();
        sale.CancelItem(item.Id);

        var act = () => sale.CancelItem(item.Id);
        act.Should().Throw<DomainException>().WithMessage("*already cancelled*");
    }

    [Fact(DisplayName = "SetItems should replace all items and recalculate")]
    public void Given_Sale_When_SetItems_Then_ShouldReplaceAndRecalculate()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems(itemCount: 2);
        var oldTotal = sale.TotalAmount;

        var newItems = new List<SaleItem>
        {
            SaleItemTestData.GenerateValidItem(quantity: 1, unitPrice: 10m)
        };
        sale.SetItems(newItems);

        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().Be(10m);
    }
}
