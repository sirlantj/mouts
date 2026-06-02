using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Common.Caching;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Rebus.Bus;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application;

public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly ISaleReadRepository _readRepository;
    private readonly ISaleEventStore _eventStore;
    private readonly ICacheService _cache;
    private readonly IBus _bus;
    private readonly ILogger<CreateSaleHandler> _logger;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _readRepository = Substitute.For<ISaleReadRepository>();
        _eventStore = Substitute.For<ISaleEventStore>();
        _cache = Substitute.For<ICacheService>();
        _bus = Substitute.For<IBus>();
        _logger = Substitute.For<ILogger<CreateSaleHandler>>();
        _handler = new CreateSaleHandler(_saleRepository, _readRepository, _eventStore, _cache, _bus, _logger);
    }

    [Fact(DisplayName = "Valid create sale command should return success")]
    public async Task Handle_ValidRequest_ReturnsSuccessResponse()
    {
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-001",
            SaleDate = DateTime.UtcNow,
            CustomerExternalId = "CUST-1",
            CustomerName = "John Doe",
            BranchExternalId = "BR-1",
            BranchName = "Main Branch",
            Items = new List<CreateSaleItemDto>
            {
                new() { ProductExternalId = "PROD-1", ProductName = "Product 1", Quantity = 5, UnitPrice = 100m }
            }
        };

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                var sale = callInfo.Arg<Sale>();
                return sale;
            });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.SaleNumber.Should().Be("SALE-001");
        result.TotalAmount.Should().Be(450m); // 5 * 100 * 0.90
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
        await _eventStore.Received(1).StoreEventAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _bus.Received(1).Publish(Arg.Any<object>());
    }

    [Fact(DisplayName = "Invalid create sale command should throw validation exception")]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var command = new CreateSaleCommand();
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Create sale should apply correct discounts")]
    public async Task Handle_ValidRequest_AppliesCorrectDiscounts()
    {
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-002",
            SaleDate = DateTime.UtcNow,
            CustomerExternalId = "CUST-1",
            CustomerName = "Jane Doe",
            BranchExternalId = "BR-1",
            BranchName = "Main Branch",
            Items = new List<CreateSaleItemDto>
            {
                new() { ProductExternalId = "PROD-1", ProductName = "Product A", Quantity = 3, UnitPrice = 100m },  // 300 (no discount)
                new() { ProductExternalId = "PROD-2", ProductName = "Product B", Quantity = 10, UnitPrice = 50m }   // 400 (20% off)
            }
        };

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(700m);
    }

    [Fact(DisplayName = "Create sale should invalidate list cache")]
    public async Task Handle_ValidRequest_InvalidatesListCache()
    {
        var command = new CreateSaleCommand
        {
            SaleNumber = "SALE-003",
            SaleDate = DateTime.UtcNow,
            CustomerExternalId = "CUST-1",
            CustomerName = "Test",
            BranchExternalId = "BR-1",
            BranchName = "Branch",
            Items = new List<CreateSaleItemDto>
            {
                new() { ProductExternalId = "P-1", ProductName = "Prod", Quantity = 1, UnitPrice = 10m }
            }
        };

        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Sale>());

        await _handler.Handle(command, CancellationToken.None);

        await _cache.Received(1).RemoveByPrefixAsync("sales:", Arg.Any<CancellationToken>());
    }
}
