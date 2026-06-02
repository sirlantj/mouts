using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>
/// Centralizes the side effects every write-side sale operation must run:
/// persist the event to the event store, refresh the read model and invalidate cache.
/// Pulled out of the individual handlers to remove DRY violations and keep them focused
/// on orchestration of domain logic.
/// </summary>
public interface ISaleSideEffects
{
    Task PublishAsync<TEvent>(
        Sale sale,
        TEvent domainEvent,
        CancellationToken cancellationToken = default)
        where TEvent : class;
}
