using Ambev.DeveloperEvaluation.Application.Common.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using MediatR;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="CreateSaleHandler"/>.
/// </summary>
public class CreateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly CreateSaleHandler _handler;

    public CreateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _mediator = Substitute.For<IMediator>();
        _handler = new CreateSaleHandler(_saleRepository, _mapper, _mediator);

        // Map header command -> Sale (items handled by the aggregate).
        _mapper.Map<Sale>(Arg.Any<CreateSaleCommand>()).Returns(ci =>
        {
            var c = ci.Arg<CreateSaleCommand>();
            return new Sale
            {
                SaleNumber = c.SaleNumber,
                SaleDate = c.SaleDate,
                CustomerId = c.CustomerId,
                CustomerName = c.CustomerName,
                BranchId = c.BranchId,
                BranchName = c.BranchName
            };
        });

        _mapper.Map<SaleItem>(Arg.Any<CreateSaleItemCommand>()).Returns(ci =>
        {
            var c = ci.Arg<CreateSaleItemCommand>();
            return new SaleItem
            {
                ProductId = c.ProductId,
                ProductTitle = c.ProductTitle,
                Quantity = c.Quantity,
                UnitPrice = c.UnitPrice
            };
        });

        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(ci =>
        {
            var s = ci.Arg<Sale>();
            return new SaleResult { Id = s.Id, SaleNumber = s.SaleNumber, TotalAmount = s.TotalAmount };
        });

        _saleRepository.GetBySaleNumberAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Sale?)null);
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => ci.Arg<Sale>());
    }

    [Fact(DisplayName = "Given valid command When creating sale Then persists and returns result")]
    public async Task Handle_ValidCommand_CreatesSale()
    {
        // Given
        var command = SaleHandlerTestData.GenerateValidCreateCommand();

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        result.Should().NotBeNull();
        await _saleRepository.Received(1).CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given valid command When creating sale Then publishes SaleCreated event")]
    public async Task Handle_ValidCommand_PublishesEvent()
    {
        // Given
        var command = SaleHandlerTestData.GenerateValidCreateCommand();

        // When
        await _handler.Handle(command, CancellationToken.None);

        // Then
        await _mediator.Received(1).Publish(
            Arg.Any<DomainEventNotification<SaleCreatedEvent>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given command with discount tier When creating sale Then total reflects discount")]
    public async Task Handle_AppliesDiscountRules()
    {
        // Given: 10 units at 100 => 20% discount => 800
        var command = SaleHandlerTestData.GenerateCreateCommandWithItem(quantity: 10, unitPrice: 100m);
        Sale? persisted = null;
        _saleRepository.CreateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>())
            .Returns(ci => { persisted = ci.Arg<Sale>(); return persisted; });

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        persisted!.TotalAmount.Should().Be(800m);
        result.TotalAmount.Should().Be(800m);
    }

    [Fact(DisplayName = "Given empty command When creating sale Then throws validation exception")]
    public async Task Handle_InvalidCommand_ThrowsValidationException()
    {
        // Given
        var command = new CreateSaleCommand();

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }

    [Fact(DisplayName = "Given duplicate sale number When creating sale Then throws")]
    public async Task Handle_DuplicateSaleNumber_Throws()
    {
        // Given
        var command = SaleHandlerTestData.GenerateValidCreateCommand();
        _saleRepository.GetBySaleNumberAsync(command.SaleNumber, Arg.Any<CancellationToken>())
            .Returns(SaleHandlerTestData.GenerateSaleEntity());

        // When
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
