using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <inheritdoc cref="ISaleSideEffects"/>
public sealed class SaleSideEffects : ISaleSideEffects
{
    private readonly ISaleEventStore _eventStore;
    private readonly ISaleReadRepository _readRepository;
    private readonly ICacheService _cache;
    private readonly ILogger<SaleSideEffects> _logger;

    public SaleSideEffects(
        ISaleEventStore eventStore,
        ISaleReadRepository readRepository,
        ICacheService cache,
        ILogger<SaleSideEffects> logger)
    {
        _eventStore = eventStore;
        _readRepository = readRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(
        Sale sale,
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventType = typeof(TEvent).Name;
        _logger.LogInformation("{EventType}: {SaleNumber}", eventType, sale.SaleNumber);

        await _eventStore.StoreEventAsync(eventType, domainEvent, sale.Id, sale.SaleNumber, cancellationToken);
        await _readRepository.UpsertAsync(sale, cancellationToken);

        await _cache.RemoveAsync(CacheKeys.ForSale(sale.Id), cancellationToken);
        await _cache.RemoveByPrefixAsync(CacheKeys.SalesListPrefix, cancellationToken);
    }
}
