namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleEventStore
{
    Task StoreEventAsync(string eventType, object domainEvent, Guid saleId, string saleNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<object>> GetEventsAsync(Guid saleId, CancellationToken cancellationToken = default);
}
