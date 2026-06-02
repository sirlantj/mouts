using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleEventStore _eventStore;
    private readonly ILogger<CancelSaleItemHandler> _logger;

    public CancelSaleItemHandler(
        ISaleRepository saleRepository,
        ISaleReadRepository readRepository,
        ISaleEventStore eventStore,
        ILogger<CancelSaleItemHandler> logger)
    {
        _saleRepository = saleRepository;
        _readRepository = readRepository;
        _eventStore = eventStore;
        _logger = logger;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var validator = new CancelSaleItemValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {command.SaleId} not found.");

        var spec = new ActiveSaleSpecification();
        if (!spec.IsSatisfiedBy(sale))
            throw new InvalidOperationException("Cannot cancel items from a cancelled sale.");

        sale.CancelItem(command.ItemId);
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var domainEvent = new ItemCancelledEvent(sale, command.ItemId);
        _logger.LogInformation("ItemCancelled: Sale {SaleNumber}, Item {ItemId}", sale.SaleNumber, command.ItemId);
        await _eventStore.StoreEventAsync(nameof(ItemCancelledEvent), domainEvent, sale.Id, sale.SaleNumber, cancellationToken);
        await _readRepository.UpsertAsync(sale, cancellationToken);

        return new CancelSaleItemResult
        {
            SaleId = sale.Id,
            ItemId = command.ItemId,
            NewTotalAmount = sale.TotalAmount
        };
    }
}
