using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities;

public class SaleItemTests
{
    [Fact(DisplayName = "Item with quantity 1 should have zero discount")]
    public void Given_ItemWithQuantity1_When_DiscountCalculated_Then_DiscountShouldBeZero()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 1, unitPrice: 100m);
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(100m);
    }

    [Fact(DisplayName = "Item with quantity 3 should have zero discount")]
    public void Given_ItemWithQuantity3_When_DiscountCalculated_Then_DiscountShouldBeZero()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 3, unitPrice: 100m);
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(300m);
    }

    [Fact(DisplayName = "Item with quantity 4 should have 10% discount")]
    public void Given_ItemWithQuantity4_When_DiscountCalculated_Then_DiscountShouldBe10Percent()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 4, unitPrice: 100m);
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(360m);
    }

    [Fact(DisplayName = "Item with quantity 9 should have 10% discount")]
    public void Given_ItemWithQuantity9_When_DiscountCalculated_Then_DiscountShouldBe10Percent()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 9, unitPrice: 100m);
        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(810m);
    }

    [Fact(DisplayName = "Item with quantity 10 should have 20% discount")]
    public void Given_ItemWithQuantity10_When_DiscountCalculated_Then_DiscountShouldBe20Percent()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 10, unitPrice: 100m);
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(800m);
    }

    [Fact(DisplayName = "Item with quantity 19 should have 20% discount")]
    public void Given_ItemWithQuantity19_When_DiscountCalculated_Then_DiscountShouldBe20Percent()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 19, unitPrice: 100m);
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(1520m);
    }

    [Fact(DisplayName = "Item with quantity 20 should have 20% discount")]
    public void Given_ItemWithQuantity20_When_DiscountCalculated_Then_DiscountShouldBe20Percent()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 20, unitPrice: 100m);
        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(1600m);
    }

    [Fact(DisplayName = "Item with quantity 21 should throw DomainException")]
    public void Given_ItemWithQuantity21_When_DiscountCalculated_Then_ShouldThrowDomainException()
    {
        var item = new SaleItem
        {
            SaleId = Guid.NewGuid(),
            ProductExternalId = "PROD-1",
            ProductName = "Test Product",
            Quantity = 21,
            UnitPrice = 100m
        };

        var act = () => item.CalculateDiscount();
        act.Should().Throw<DomainException>().WithMessage("*more than 20*");
    }

    [Fact(DisplayName = "Active item when cancelled should set IsCancelled to true")]
    public void Given_ActiveItem_When_Cancelled_Then_IsCancelledShouldBeTrue()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 5);
        item.IsCancelled.Should().BeFalse();

        item.Cancel();

        item.IsCancelled.Should().BeTrue();
        item.UpdatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "Total amount should be correctly calculated with discount")]
    public void Given_Item_When_TotalCalculated_Then_ShouldApplyDiscountCorrectly()
    {
        var item = SaleItemTestData.GenerateValidItem(quantity: 5, unitPrice: 200m);
        // 5 items * 200 * (1 - 0.10) = 900
        item.TotalAmount.Should().Be(900m);
    }
}
