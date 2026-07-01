using Ambev.DeveloperEvaluation.Application.Common.Events;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
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
/// Unit tests for <see cref="UpdateSaleHandler"/>.
/// </summary>
public class UpdateSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly UpdateSaleHandler _handler;

    public UpdateSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _mediator = Substitute.For<IMediator>();
        _handler = new UpdateSaleHandler(_saleRepository, _mapper, _mediator);

        _mapper.Map<SaleItem>(Arg.Any<UpdateSaleItemCommand>()).Returns(ci =>
        {
            var c = ci.Arg<UpdateSaleItemCommand>();
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
            return new SaleResult { Id = s.Id, TotalAmount = s.TotalAmount };
        });
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());
    }

    [Fact(DisplayName = "Given existing sale When updating Then replaces items, recomputes total and publishes event")]
    public async Task Handle_ExistingSale_UpdatesAndPublishes()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateSaleEntity(itemCount: 3);
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(sale.Id); // single item 5 x 100 => 450

        // When
        var result = await _handler.Handle(command, CancellationToken.None);

        // Then
        sale.Items.Should().HaveCount(1);
        result.TotalAmount.Should().Be(450m);
        await _mediator.Received(1).Publish(Arg.Any<DomainEventNotification<SaleModifiedEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When updating Then throws KeyNotFound")]
    public async Task Handle_MissingSale_Throws()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(Guid.NewGuid());
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given cancelled sale When updating Then throws InvalidOperation")]
    public async Task Handle_CancelledSale_Throws()
    {
        var sale = SaleHandlerTestData.GenerateSaleEntity();
        sale.Cancel();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);
        var command = SaleHandlerTestData.GenerateValidUpdateCommand(sale.Id);
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
