using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Specifications;

public class ActiveSaleSpecificationTests
{
    [Fact(DisplayName = "Active sale should satisfy specification")]
    public void Given_ActiveSale_When_Checked_Then_ShouldBeTrue()
    {
        var sale = SaleTestData.GenerateValidSale();
        var spec = new ActiveSaleSpecification();
        spec.IsSatisfiedBy(sale).Should().BeTrue();
    }

    [Fact(DisplayName = "Cancelled sale should not satisfy specification")]
    public void Given_CancelledSale_When_Checked_Then_ShouldBeFalse()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.Cancel();
        var spec = new ActiveSaleSpecification();
        spec.IsSatisfiedBy(sale).Should().BeFalse();
    }
}
