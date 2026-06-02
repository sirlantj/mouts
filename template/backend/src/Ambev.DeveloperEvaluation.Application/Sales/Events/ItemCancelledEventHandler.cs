using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Rebus.Handlers;

namespace Ambev.DeveloperEvaluation.Application.Sales.Events;

public class ItemCancelledEventHandler : IHandleMessages<ItemCancelledEvent>
{
    private readonly ISaleEventStore _eventStore;
    private readonly ISaleReadRepository _readRepository;
    private readonly ILogger<ItemCancelledEventHandler> _logger;

    public ItemCancelledEventHandler(ISaleEventStore eventStore, ISaleReadRepository readRepository, ILogger<ItemCancelledEventHandler> logger)
    {
        _eventStore = eventStore;
        _readRepository = readRepository;
        _logger = logger;
    }

    public async Task Handle(ItemCancelledEvent message)
    {
        _logger.LogInformation("[Rebus] ItemCancelled: Sale {SaleNumber}, Item {ItemId}", message.Sale.SaleNumber, message.ItemId);

        await _eventStore.StoreEventAsync(nameof(ItemCancelledEvent), message, message.Sale.Id, message.Sale.SaleNumber);
        await _readRepository.UpsertAsync(message.Sale);
    }
}
