using Ambev.DeveloperEvaluation.Application.Common.Events;
using Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;
using Ambev.DeveloperEvaluation.Domain.Events;
using Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Unit tests for <see cref="SaleEventLoggingHandler"/>.
/// </summary>
public class SaleEventLoggingHandlerTests
{
    private readonly ILogger<SaleEventLoggingHandler> _logger = Substitute.For<ILogger<SaleEventLoggingHandler>>();

    [Fact(DisplayName = "SaleCreated event is logged at Information level")]
    public async Task Handle_SaleCreated_LogsInformation()
    {
        // Given
        var handler = new SaleEventLoggingHandler(_logger);
        var sale = SaleHandlerTestData.GenerateSaleEntity();
        var notification = new DomainEventNotification<SaleCreatedEvent>(new SaleCreatedEvent(sale));

        // When
        var act = () => handler.Handle(notification, CancellationToken.None);

        // Then
        await act.Should().NotThrowAsync();
        _logger.ReceivedWithAnyArgs(1).Log(default, default, default!, default, default!);
    }

    [Fact(DisplayName = "All sale events are handled without throwing")]
    public async Task Handle_AllEvents_DoNotThrow()
    {
        // Given
        var handler = new SaleEventLoggingHandler(_logger);
        var sale = SaleHandlerTestData.GenerateSaleEntity();
        var item = sale.Items.First();

        // When / Then
        await handler.Handle(new DomainEventNotification<SaleCreatedEvent>(new SaleCreatedEvent(sale)), CancellationToken.None);
        await handler.Handle(new DomainEventNotification<SaleModifiedEvent>(new SaleModifiedEvent(sale)), CancellationToken.None);
        await handler.Handle(new DomainEventNotification<SaleCancelledEvent>(new SaleCancelledEvent(sale)), CancellationToken.None);
        await handler.Handle(new DomainEventNotification<SaleItemCancelledEvent>(new SaleItemCancelledEvent(sale, item)), CancellationToken.None);
    }
}
