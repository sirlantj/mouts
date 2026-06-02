using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleEventStore _eventStore;
    private readonly ICacheService _cache;
    private readonly IBus _bus;
    private readonly ILogger<CancelSaleItemHandler> _logger;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _readRepository = Substitute.For<ISaleReadRepository>();
        _eventStore = Substitute.For<ISaleEventStore>();
        _cache = Substitute.For<ICacheService>();
        _bus = Substitute.For<IBus>();
        _logger = Substitute.For<ILogger<CancelSaleItemHandler>>();
        _handler = new CancelSaleItemHandler(_saleRepository, _readRepository, _eventStore, _cache, _bus, _logger);
    }

    private static Sale CreateSaleWithItemIds()
    {
        var sale = SaleTestData.GenerateValidSale();
        var item1 = sale.AddItem("PROD-1", "Product 1", 5, 100m);
        item1.Id = Guid.NewGuid();
        var item2 = sale.AddItem("PROD-2", "Product 2", 2, 50m);
        item2.Id = Guid.NewGuid();
        return sale;
    }

    [Fact(DisplayName = "Cancel item should succeed and recalculate total")]
    public async Task Handle_ValidItem_ShouldCancelAndRecalculate()
    {
        var sale = CreateSaleWithItemIds();
        var itemId = sale.Items.First().Id;

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, itemId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.NewTotalAmount.Should().Be(100m); // Only item2 remains: 2 * 50
        await _cache.Received(1).RemoveAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveByPrefixAsync("sales:", Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Cancel item in already cancelled sale should throw")]
    public async Task Handle_CancelledSale_ShouldThrow()
    {
        var sale = CreateSaleWithItemIds();
        sale.Cancel();
        var itemId = sale.Items.First().Id;

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, itemId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact(DisplayName = "Cancel already cancelled item should throw DomainException")]
    public async Task Handle_AlreadyCancelledItem_ShouldThrow()
    {
        var sale = CreateSaleWithItemIds();
        var itemId = sale.Items.First().Id;
        sale.CancelItem(itemId);

        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        var command = new CancelSaleItemCommand(sale.Id, itemId);
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*already cancelled*");
    }
}
