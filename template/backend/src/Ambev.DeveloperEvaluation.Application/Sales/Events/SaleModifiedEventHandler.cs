using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleModifiedEventHandler : IHandleMessages<SaleModifiedEvent>
{
    private readonly ISaleEventStore _eventStore;
    private readonly ISaleReadRepository _readRepository;
    private readonly ILogger<SaleModifiedEventHandler> _logger;

    public SaleModifiedEventHandler(ISaleEventStore eventStore, ISaleReadRepository readRepository, ILogger<SaleModifiedEventHandler> logger)
    {
        _eventStore = eventStore;
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task Handle(SaleModifiedEvent message)
    {
        _logger.LogInformation("[Rebus] SaleModified: {SaleNumber} ({SaleId})", message.Sale.SaleNumber, message.Sale.Id);

        await _eventStore.StoreEventAsync(nameof(SaleModifiedEvent), message, message.Sale.Id, message.Sale.SaleNumber);
        await _readRepository.UpsertAsync(message.Sale);
    }
}
