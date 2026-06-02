using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;

public sealed class DeleteSaleHandler : IRequestHandler<DeleteSaleCommand, DeleteSaleResult>
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleSideEffects _sideEffects;

    public DeleteSaleHandler(ISaleRepository saleRepository, ISaleSideEffects sideEffects)
    {
        _saleRepository = saleRepository;
        _sideEffects = sideEffects;
    }

    public async Task<DeleteSaleResult> Handle(DeleteSaleCommand command, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Sale with id {command.Id} not found.");

        sale.Cancel(); // throws if already cancelled

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _sideEffects.PublishAsync(sale, new SaleCancelledEvent(sale), cancellationToken);

        return new DeleteSaleResult
        {
            Id = sale.Id,
            Status = sale.Status.ToString()
        };
    }
}
