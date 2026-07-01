using Ambev.DeveloperEvaluation.Application.Common.Events;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.Common;
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
/// Unit tests for <see cref="CancelSaleItemHandler"/>.
/// </summary>
public class CancelSaleItemHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    private readonly CancelSaleItemHandler _handler;

    public CancelSaleItemHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _mediator = Substitute.For<IMediator>();
        _handler = new CancelSaleItemHandler(_saleRepository, _mapper, _mediator);
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(ci => new SaleResult { Id = ci.Arg<Sale>().Id, TotalAmount = ci.Arg<Sale>().TotalAmount });
        _saleRepository.UpdateAsync(Arg.Any<Sale>(), Arg.Any<CancellationToken>()).Returns(ci => ci.Arg<Sale>());
    }

    [Fact(DisplayName = "Given existing item When cancelling Then item excluded from total and event published")]
    public async Task Handle_ExistingItem_CancelsAndPublishes()
    {
        // Given: two items of 5 x 100 (10% discount) => 450 each => total 900; cancel one => 450
        var sale = SaleHandlerTestData.GenerateSaleEntity(itemCount: 2);
        var itemToCancel = sale.Items.First();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(new CancelSaleItemCommand(sale.Id, itemToCancel.Id), CancellationToken.None);

        // Then
        itemToCancel.IsCancelled.Should().BeTrue();
        result.TotalAmount.Should().Be(450m);
        await _mediator.Received(1).Publish(Arg.Any<DomainEventNotification<SaleItemCancelledEvent>>(), Arg.Any<CancellationToken>());
    }

    [Fact(DisplayName = "Given missing sale When cancelling item Then throws KeyNotFound")]
    public async Task Handle_MissingSale_Throws()
    {
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);
        var act = () => _handler.Handle(new CancelSaleItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
