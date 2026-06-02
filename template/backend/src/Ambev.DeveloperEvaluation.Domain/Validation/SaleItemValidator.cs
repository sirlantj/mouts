using Ambev.DeveloperEvaluation.Domain.Entities;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleItemValidator : AbstractValidator<SaleItem>
{
    public SaleItemValidator()
    {
        RuleFor(i => i.ProductExternalId).NotEmpty();
        RuleFor(i => i.ProductName).NotEmpty();
        RuleFor(i => i.Quantity).GreaterThan(0).LessThanOrEqualTo(20);
        RuleFor(i => i.UnitPrice).GreaterThan(0);
    }
}
