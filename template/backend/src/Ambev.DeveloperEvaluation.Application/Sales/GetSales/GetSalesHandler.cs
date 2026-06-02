using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSales;

public class GetSalesHandler : IRequestHandler<GetSalesCommand, GetSalesResult>
{
    private readonly ISaleReadRepository _readRepository;
    private readonly ICacheService _cache;

    public GetSalesHandler(ISaleReadRepository readRepository, ICacheService cache)
    {
        _readRepository = readRepository;
        _cache = cache;
    }

    public async Task<GetSalesResult> Handle(GetSalesCommand command, CancellationToken cancellationToken)
    {
        var validator = new GetSalesValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cacheKey = CacheKeys.ForSalesList(
            command.Page, command.Size, command.Order,
            command.CustomerName, command.BranchName, command.Status,
            command.StartDate, command.EndDate);

        var cached = await _cache.GetAsync<GetSalesResult>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var (items, totalCount) = await _readRepository.GetPaginatedAsync(
            command.Page, command.Size, command.Order,
            command.CustomerName, command.BranchName, command.Status,
            command.StartDate, command.EndDate,
            cancellationToken);

        var data = items.Select(sale => new GetSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            SaleDate = sale.SaleDate,
            CustomerExternalId = sale.CustomerExternalId,
            CustomerName = sale.CustomerName,
            BranchExternalId = sale.BranchExternalId,
            BranchName = sale.BranchName,
            TotalAmount = sale.TotalAmount,
            Status = sale.Status.ToString(),
            Items = sale.Items.Select(i => new GetSaleItemResult
            {
                Id = i.Id,
                ProductExternalId = i.ProductExternalId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalAmount = i.TotalAmount,
                IsCancelled = i.IsCancelled
            }).ToList()
        });

        var result = new GetSalesResult
        {
            Data = data,
            TotalCount = totalCount,
            CurrentPage = command.Page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)command.Size)
        };

        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(2), cancellationToken);

        return result;
    }
}
