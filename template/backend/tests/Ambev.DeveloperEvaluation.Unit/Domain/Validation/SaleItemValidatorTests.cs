using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Validation;
using FluentValidation.TestHelper;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Validation;

public class SaleItemValidatorTests
{
    private readonly SaleItemValidator _validator = new();

    [Fact(DisplayName = "Valid item should pass validation")]
    public void Given_ValidItem_When_Validated_Then_ShouldPass()
    {
        var item = new SaleItem
        {
            ProductExternalId = "PROD-1",
            ProductName = "Test Product",
            Quantity = 5,
            UnitPrice = 100m
        };
        var result = _validator.TestValidate(item);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = "Empty ProductExternalId should fail")]
    public void Given_EmptyProductExternalId_When_Validated_Then_ShouldFail()
    {
        var item = new SaleItem { ProductExternalId = "", ProductName = "Test", Quantity = 1, UnitPrice = 10m };
        var result = _validator.TestValidate(item);
        result.ShouldHaveValidationErrorFor(i => i.ProductExternalId);
    }

    [Fact(DisplayName = "Empty ProductName should fail")]
    public void Given_EmptyProductName_When_Validated_Then_ShouldFail()
    {
        var item = new SaleItem { ProductExternalId = "PROD-1", ProductName = "", Quantity = 1, UnitPrice = 10m };
        var result = _validator.TestValidate(item);
        result.ShouldHaveValidationErrorFor(i => i.ProductName);
    }

    [Fact(DisplayName = "Zero quantity should fail")]
    public void Given_ZeroQuantity_When_Validated_Then_ShouldFail()
    {
        var item = new SaleItem { ProductExternalId = "PROD-1", ProductName = "Test", Quantity = 0, UnitPrice = 10m };
        var result = _validator.TestValidate(item);
        result.ShouldHaveValidationErrorFor(i => i.Quantity);
    }

    [Fact(DisplayName = "Quantity above 20 should fail")]
    public void Given_Quantity21_When_Validated_Then_ShouldFail()
    {
        var item = new SaleItem { ProductExternalId = "PROD-1", ProductName = "Test", Quantity = 21, UnitPrice = 10m };
        var result = _validator.TestValidate(item);
        result.ShouldHaveValidationErrorFor(i => i.Quantity);
    }

    [Fact(DisplayName = "Zero UnitPrice should fail")]
    public void Given_ZeroUnitPrice_When_Validated_Then_ShouldFail()
    {
        var item = new SaleItem { ProductExternalId = "PROD-1", ProductName = "Test", Quantity = 1, UnitPrice = 0m };
        var result = _validator.TestValidate(item);
        result.ShouldHaveValidationErrorFor(i => i.UnitPrice);
    }
}
