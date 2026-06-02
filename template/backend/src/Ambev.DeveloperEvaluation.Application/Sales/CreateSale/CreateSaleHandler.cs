using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

public sealed class CreateSaleHandler : IRequestHandler<CreateSaleCommand, CreateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleSideEffects _sideEffects;

    public CreateSaleHandler(ISaleRepository saleRepository, ISaleSideEffects sideEffects)
    {
        _saleRepository = saleRepository;
        _sideEffects = sideEffects;
    }

    public async Task<CreateSaleResult> Handle(CreateSaleCommand command, CancellationToken cancellationToken)
    {
        // ValidationBehavior already ran CreateSaleValidator via the MediatR pipeline.

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

        await _sideEffects.PublishAsync(created, new SaleCreatedEvent(created), cancellationToken);

        return new CreateSaleResult
        {
            Id = created.Id,
            SaleNumber = created.SaleNumber,
            TotalAmount = created.TotalAmount
        };
    }
}
