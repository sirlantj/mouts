using Ambev.DeveloperEvaluation.Application.Sales.DeleteSale;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleEventStore _eventStore;
    private readonly ICacheService _cache;
    private readonly IBus _bus;
    private readonly ILogger<DeleteSaleHandler> _logger;
    private readonly DeleteSaleHandler _handler;

    public CancelSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _readRepository = Substitute.For<ISaleReadRepository>();
        _eventStore = Substitute.For<ISaleEventStore>();
        _cache = Substitute.For<ICacheService>();
        _bus = Substitute.For<IBus>();
        _logger = Substitute.For<ILogger<DeleteSaleHandler>>();
        _handler = new DeleteSaleHandler(_saleRepository, _readRepository, _eventStore, _cache, _bus, _logger);
    }

    [Fact(DisplayName = "Cancel active sale should succeed")]
    public async Task Handle_ActiveSale_ShouldCancelSuccessfully()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new DeleteSaleCommand(sale.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(SaleStatus.Cancelled.ToString());
        await _saleRepository.Received(1).UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Cancel non-existent sale should throw KeyNotFoundException")]
    public async Task Handle_NonExistentSale_ShouldThrowKeyNotFoundException()
    {
        var saleId = Guid.NewGuid();
        _saleRepository.GetByIdAsync(saleId, Arg.Any<CancellationToken>()).Returns((Sale?)null);

        var command = new DeleteSaleCommand(saleId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Cancel already cancelled sale should throw InvalidOperationException")]
    public async Task Handle_AlreadyCancelledSale_ShouldThrowInvalidOperationException()
    {
        var sale = SaleTestData.GenerateValidSaleWithItems();
        sale.Cancel();

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new DeleteSaleCommand(sale.Id);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*already cancelled*");
    }
}
