using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Domain.Validation;

public class SaleValidator : AbstractValidator<Sale>
{
    public SaleValidator()
    {
        RuleFor(s => s.SaleNumber).NotEmpty();
        RuleFor(s => s.CustomerExternalId).NotEmpty();
        RuleFor(s => s.CustomerName).NotEmpty();
        RuleFor(s => s.BranchExternalId).NotEmpty();
        RuleFor(s => s.BranchName).NotEmpty();
        RuleFor(s => s.Status).NotEqual(SaleStatus.Unknown);
        RuleFor(s => s.Items).NotEmpty().WithMessage("Sale must have at least one item.");
    }
}
