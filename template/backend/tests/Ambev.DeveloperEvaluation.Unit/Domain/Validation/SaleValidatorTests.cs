using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Validation;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class SaleValidatorTests
{
    private readonly SaleValidator _validator = new();

    [Fact(DisplayName = "Valid sale should pass validation")]
    public void Given_ValidSale_When_Validated_Then_ShouldPass()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        var result = _validator.TestValidate(sale);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty SaleNumber should fail")]
    public void Given_EmptySaleNumber_When_Validated_Then_ShouldFail()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.SaleNumber = string.Empty;
        var result = _validator.TestValidate(sale);
        result.ShouldHaveValidationErrorFor(s => s.SaleNumber);
    }

    [Fact(DisplayName = "Empty CustomerName should fail")]
    public void Given_EmptyCustomerName_When_Validated_Then_ShouldFail()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.CustomerName = string.Empty;
        var result = _validator.TestValidate(sale);
        result.ShouldHaveValidationErrorFor(s => s.CustomerName);
    }

    [Fact(DisplayName = "Empty BranchName should fail")]
    public void Given_EmptyBranchName_When_Validated_Then_ShouldFail()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.BranchName = string.Empty;
        var result = _validator.TestValidate(sale);
        result.ShouldHaveValidationErrorFor(s => s.BranchName);
    }

    [Fact(DisplayName = "Sale with no items should fail")]
    public void Given_SaleWithNoItems_When_Validated_Then_ShouldFail()
    {
        var sale = SaleTestData.GenerateValidSale();
        var result = _validator.TestValidate(sale);
        result.ShouldHaveValidationErrorFor(s => s.Items);
    }
}
