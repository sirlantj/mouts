using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleEventStore _eventStore;
    private readonly ICacheService _cache;
    private readonly ILogger<CreateSaleHandler> _logger;

    public CreateSaleHandler(
        ISaleRepository saleRepository,
        ISaleReadRepository readRepository,
        ISaleEventStore eventStore,
        ICacheService cache,
        ILogger<CreateSaleHandler> logger)
    {
        _saleRepository = saleRepository;
        _readRepository = readRepository;
        _eventStore = eventStore;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleValidator();
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var sale = new Sale
        {
            SaleNumber = command.SaleNumber,
            SaleDate = command.SaleDate,
            CustomerExternalId = command.CustomerExternalId,
            CustomerName = command.CustomerName,
            BranchExternalId = command.BranchExternalId,
            BranchName = command.BranchName
        };

        foreach (var item in command.Items)
            sale.AddItem(item.ProductExternalId, item.ProductName, item.Quantity, item.UnitPrice);

        var created = await _saleRepository.CreateAsync(sale, cancellationToken);

        var domainEvent = new SaleCreatedEvent(created);
        _logger.LogInformation("SaleCreated: {SaleNumber}", created.SaleNumber);
        await _eventStore.StoreEventAsync(nameof(SaleCreatedEvent), domainEvent, created.Id, created.SaleNumber, cancellationToken);
        await _readRepository.UpsertAsync(created, cancellationToken);

        await _cache.RemoveByPrefixAsync(CacheKeys.SalesListPrefix, cancellationToken);

        return new CreateSaleResult
        {
            Id = created.Id,
            SaleNumber = created.SaleNumber,
            TotalAmount = created.TotalAmount
        };
    }
}
