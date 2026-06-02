using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleCreatedEventHandler : IHandleMessages<SaleCreatedEvent>
{
    private readonly ISaleEventStore _eventStore;
    private readonly ISaleReadRepository _readRepository;
    private readonly ILogger<SaleCreatedEventHandler> _logger;

    public SaleCreatedEventHandler(ISaleEventStore eventStore, ISaleReadRepository readRepository, ILogger<SaleCreatedEventHandler> logger)
    {
        _eventStore = eventStore;
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task Handle(SaleCreatedEvent message)
    {
        _logger.LogInformation("[Rebus] SaleCreated: {SaleNumber} ({SaleId})", message.Sale.SaleNumber, message.Sale.Id);

        await _eventStore.StoreEventAsync(nameof(SaleCreatedEvent), message, message.Sale.Id, message.Sale.SaleNumber);
        await _readRepository.UpsertAsync(message.Sale);
    }
}
