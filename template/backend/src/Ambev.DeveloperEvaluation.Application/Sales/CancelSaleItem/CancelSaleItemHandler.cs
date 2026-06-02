using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;

public sealed class CancelSaleItemHandler : IRequestHandler<CancelSaleItemCommand, CancelSaleItemResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleSideEffects _sideEffects;

    public CancelSaleItemHandler(ISaleRepository saleRepository, ISaleSideEffects sideEffects)
    {
        _saleRepository = saleRepository;
        _sideEffects = sideEffects;
    }

    public async Task<CancelSaleItemResult> Handle(CancelSaleItemCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {command.SaleId} not found.");

        sale.EnsureActive("cancel items from");
        sale.CancelItem(command.ItemId);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _sideEffects.PublishAsync(sale, new ItemCancelledEvent(sale, command.ItemId), cancellationToken);

        return new CancelSaleItemResult
        {
            SaleId = sale.Id,
            ItemId = command.ItemId,
            NewTotalAmount = sale.TotalAmount
        };
    }
}
