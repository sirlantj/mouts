using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleEventStore _eventStore;
    private readonly ICacheService _cache;
    private readonly ILogger<UpdateSaleHandler> _logger;

    public UpdateSaleHandler(
        ISaleRepository saleRepository,
        ISaleReadRepository readRepository,
        ISaleEventStore eventStore,
        ICacheService cache,
        ILogger<UpdateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _readRepository = readRepository;
        _eventStore = eventStore;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {command.Id} not found.");

        var spec = new ActiveSaleSpecification();
        if (!spec.IsSatisfiedBy(sale))
            throw new InvalidOperationException("Cannot update a cancelled sale.");

        sale.CustomerExternalId = command.CustomerExternalId;
        sale.CustomerName = command.CustomerName;
        sale.BranchExternalId = command.BranchExternalId;
        sale.BranchName = command.BranchName;
        sale.UpdatedAt = DateTime.UtcNow;

        var newItems = command.Items.Select(i =>
        {
            var item = new Domain.Entities.SaleItem
            {
                SaleId = sale.Id,
                ProductExternalId = i.ProductExternalId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            };
            item.CalculateDiscount();
            return item;
        }).ToList();

        sale.SetItems(newItems);

        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var domainEvent = new SaleModifiedEvent(sale);
        _logger.LogInformation("SaleModified: {SaleNumber}", sale.SaleNumber);
        await _eventStore.StoreEventAsync(nameof(SaleModifiedEvent), domainEvent, sale.Id, sale.SaleNumber, cancellationToken);
        await _readRepository.UpsertAsync(sale, cancellationToken);

        await _cache.RemoveAsync(CacheKeys.ForSale(sale.Id), cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.SalesListPrefix, cancellationToken);

        return new UpdateSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount,
            Status = sale.Status.ToString()
        };
    }
}
