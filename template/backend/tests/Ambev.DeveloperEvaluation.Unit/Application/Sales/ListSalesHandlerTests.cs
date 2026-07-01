using Ambev.DeveloperEvaluation.Application.Sales.Common;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="ListSalesHandler"/>.
/// </summary>
public class ListSalesHandlerTests
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMapper _mapper;
    private readonly ListSalesHandler _handler;

    public ListSalesHandlerTests()
    {
        _saleRepository = Substitute.For<ISaleRepository>();
        _mapper = Substitute.For<IMapper>();
        _handler = new ListSalesHandler(_saleRepository, _mapper);
        _mapper.Map<List<SaleResult>>(Arg.Any<List<Sale>>())
            .Returns(ci => ci.Arg<List<Sale>>().Select(s => new SaleResult { Id = s.Id }).ToList());
    }

    [Fact(DisplayName = "Given sales When listing Then returns paginated result with metadata")]
    public async Task Handle_ReturnsPaginatedResult()
    {
        // Given
        var sales = new List<Sale>
        {
            SaleHandlerTestData.GenerateSaleEntity(),
            SaleHandlerTestData.GenerateSaleEntity()
        };
        _saleRepository.ListAsync(1, 10, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((sales, 25));

        // When
        var result = await _handler.Handle(new ListSalesCommand { Page = 1, Size = 10 }, CancellationToken.None);

        // Then
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(25);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(3);
    }

    [Fact(DisplayName = "Given invalid page and size When listing Then defaults are applied")]
    public async Task Handle_InvalidPaging_UsesDefaults()
    {
        // Given
        _saleRepository.ListAsync(1, 10, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((new List<Sale>(), 0));

        // When
        var result = await _handler.Handle(new ListSalesCommand { Page = 0, Size = 0 }, CancellationToken.None);

        // Then
        result.CurrentPage.Should().Be(1);
        await _saleRepository.Received(1).ListAsync(1, 10, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }
}
