using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="GetSaleHandler"/>.
/// </summary>
public class GetSaleHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly GetSaleHandler _handler;

    public GetSaleHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new GetSaleHandler(_saleRepository, _mapper);
        _mapper.Map<SaleResult>(Arg.Any<Sale>()).Returns(ci => new SaleResult { Id = ci.Arg<Sale>().Id });
    }

    [Fact(DisplayName = "Given existing sale When getting Then returns result")]
    public async Task Handle_ExistingSale_ReturnsResult()
    {
        // Given
        var sale = SaleHandlerTestData.GenerateSaleEntity();
        _saleRepository.GetByIdAsync(sale.Id, Arg.Any<CancellationToken>()).Returns(sale);

        // When
        var result = await _handler.Handle(new GetSaleCommand(sale.Id), CancellationToken.None);

        // Then
        result.Id.Should().Be(sale.Id);
    }

    [Fact(DisplayName = "Given missing sale When getting Then throws KeyNotFound")]
    public async Task Handle_MissingSale_Throws()
    {
        // Given
        _saleRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Sale?)null);

        // When
        var act = () => _handler.Handle(new GetSaleCommand(Guid.NewGuid()), CancellationToken.None);

        // Then
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact(DisplayName = "Given empty id When getting Then throws validation exception")]
    public async Task Handle_EmptyId_ThrowsValidation()
    {
        var act = () => _handler.Handle(new GetSaleCommand(Guid.Empty), CancellationToken.None);
        await act.Should().ThrowAsync<FluentValidation.ValidationException>();
    }
}
