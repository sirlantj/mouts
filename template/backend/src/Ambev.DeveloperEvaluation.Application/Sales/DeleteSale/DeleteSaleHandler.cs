using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Domain.Specifications;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleEventStore _eventStore;
    private readonly ICacheService _cache;
    private readonly ILogger<DeleteSaleHandler> _logger;

    public DeleteSaleHandler(
        ISaleRepository saleRepository,
        ISaleReadRepository readRepository,
        ISaleEventStore eventStore,
        ICacheService cache,
        ILogger<DeleteSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _readRepository = readRepository;
        _eventStore = eventStore;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DeleteSaleResult> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new DeleteSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {command.Id} not found.");

        var spec = new ActiveSaleSpecification();
        if (!spec.IsSatisfiedBy(sale))
            throw new InvalidOperationException("Sale is already cancelled.");

        sale.Cancel();
        await _saleRepository.UpdateAsync(sale, cancellationToken);

        var domainEvent = new SaleCancelledEvent(sale);
        _logger.LogInformation("SaleCancelled: {SaleNumber}", sale.SaleNumber);
        await _eventStore.StoreEventAsync(nameof(SaleCancelledEvent), domainEvent, sale.Id, sale.SaleNumber, cancellationToken);
        await _readRepository.UpsertAsync(sale, cancellationToken);

        await _cache.RemoveAsync(CacheKeys.ForSale(sale.Id), cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.SalesListPrefix, cancellationToken);

        return new DeleteSaleResult
        {
            Id = sale.Id,
            Status = sale.Status.ToString()
        };
    }
}
