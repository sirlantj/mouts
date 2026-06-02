using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public sealed class UpdateSaleHandler : IRequestHandler<UpdateSaleCommand, UpdateSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleSideEffects _sideEffects;

    public UpdateSaleHandler(ISaleRepository saleRepository, ISaleSideEffects sideEffects)
    {
        _saleRepository = saleRepository;
        _sideEffects = sideEffects;
    }

    public async Task<UpdateSaleResult> Handle(UpdateSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {command.Id} not found.");

        sale.EnsureActive("update");

        sale.CustomerExternalId = command.CustomerExternalId;
        sale.CustomerName = command.CustomerName;
        sale.BranchExternalId = command.BranchExternalId;
        sale.BranchName = command.BranchName;
        sale.UpdatedAt = DateTime.UtcNow;

        var newItems = command.Items.Select(i =>
        {
            var item = new SaleItem
            {
                SaleId = sale.Id,
                ProductExternalId = i.ProductExternalId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            };
            item.CalculateDiscount();
            return item;
        }).ToList();

        sale.SetItems(newItems);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _sideEffects.PublishAsync(sale, new SaleModifiedEvent(sale), cancellationToken);

        return new UpdateSaleResult
        {
            Id = sale.Id,
            SaleNumber = sale.SaleNumber,
            TotalAmount = sale.TotalAmount,
            Status = sale.Status.ToString()
        };
    }
}
