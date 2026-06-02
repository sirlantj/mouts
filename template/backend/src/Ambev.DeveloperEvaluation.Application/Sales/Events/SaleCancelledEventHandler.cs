using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class SaleCancelledEventHandler : IHandleMessages<SaleCancelledEvent>
{
    private readonly ISaleEventStore _eventStore;
    private readonly ISaleReadRepository _readRepository;
    private readonly ILogger<SaleCancelledEventHandler> _logger;

    public SaleCancelledEventHandler(ISaleEventStore eventStore, ISaleReadRepository readRepository, ILogger<SaleCancelledEventHandler> logger)
    {
        _eventStore = eventStore;
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task Handle(SaleCancelledEvent message)
    {
        _logger.LogInformation("[Rebus] SaleCancelled: {SaleNumber} ({SaleId})", message.Sale.SaleNumber, message.Sale.Id);

        await _eventStore.StoreEventAsync(nameof(SaleCancelledEvent), message, message.Sale.Id, message.Sale.SaleNumber);
        await _readRepository.UpsertAsync(message.Sale);
    }
}
