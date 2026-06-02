using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.GetSale;

public class GetSaleHandler : IRequestHandler<GetSaleCommand, GetSaleResult>
{
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleRepository _saleRepository;
    private readonly ICacheService _cache;

    public GetSaleHandler(ISaleReadRepository readRepository, ISaleRepository saleRepository, ICacheService cache)
    {
        _readRepository = readRepository;
        _saleRepository = saleRepository;
        _cache = cache;
    }

    public async Task<GetSaleResult> Handle(GetSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new GetSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var cacheKey = CacheKeys.ForSale(command.Id);
        var cached = await _cache.GetAsync<GetSaleResult>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var sale = await _readRepository.GetByIdAsync(command.Id, cancellationToken);

        // fallback to PostgreSQL
        sale ??= await _saleRepository.GetByIdAsync(command.Id, cancellationToken);

        if (sale == null)
            throw new KeyNotFoundException($"Sale with id {command.Id} not found.");

        var result = new GetSaleResult
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
        };

        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return result;
    }
}
