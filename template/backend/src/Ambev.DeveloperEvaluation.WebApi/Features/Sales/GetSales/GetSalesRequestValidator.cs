using FluentValidation;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSales;

public class GetSalesRequestValidator : AbstractValidator<GetSalesRequest>
{
    public GetSalesRequestValidator()
    {
        RuleFor(x => x._page).GreaterThanOrEqualTo(1).When(x => x._page.HasValue);
        RuleFor(x => x._size).InclusiveBetween(1, 100).When(x => x._size.HasValue);
    }
}
