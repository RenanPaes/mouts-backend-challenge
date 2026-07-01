using Ambev.DeveloperEvaluation.Application.Common.Events;
using Ambev.DeveloperEvaluation.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ambev.DeveloperEvaluation.Application.Sales.EventHandlers;

/// <summary>
/// Subscribes to the sale domain events and logs them to the application log.
/// This stands in for publishing to a real message broker, as allowed by the challenge.
/// </summary>
public class SaleEventLoggingHandler :
    INotificationHandler<DomainEventNotification<SaleCreatedEvent>>,
    INotificationHandler<DomainEventNotification<SaleModifiedEvent>>,
    INotificationHandler<DomainEventNotification<SaleCancelledEvent>>,
    INotificationHandler<DomainEventNotification<SaleItemCancelledEvent>>
{
    private readonly ILogger<SaleEventLoggingHandler> _logger;

    public SaleEventLoggingHandler(ILogger<SaleEventLoggingHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(DomainEventNotification<SaleCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;
        _logger.LogInformation(
            "Event {EventName}: Sale {SaleId} (number {SaleNumber}) created with total {TotalAmount:F2}",
            nameof(SaleCreatedEvent), sale.Id, sale.SaleNumber, sale.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(DomainEventNotification<SaleModifiedEvent> notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;
        _logger.LogInformation(
            "Event {EventName}: Sale {SaleId} (number {SaleNumber}) modified, new total {TotalAmount:F2}",
            nameof(SaleModifiedEvent), sale.Id, sale.SaleNumber, sale.TotalAmount);
        return Task.CompletedTask;
    }

    public Task Handle(DomainEventNotification<SaleCancelledEvent> notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;
        _logger.LogInformation(
            "Event {EventName}: Sale {SaleId} (number {SaleNumber}) cancelled",
            nameof(SaleCancelledEvent), sale.Id, sale.SaleNumber);
        return Task.CompletedTask;
    }

    public Task Handle(DomainEventNotification<SaleItemCancelledEvent> notification, CancellationToken cancellationToken)
    {
        var sale = notification.DomainEvent.Sale;
        var item = notification.DomainEvent.Item;
        _logger.LogInformation(
            "Event {EventName}: Item {ItemId} (product {ProductId}) cancelled on Sale {SaleId}, new total {TotalAmount:F2}",
            nameof(SaleItemCancelledEvent), item.Id, item.ProductId, sale.Id, sale.TotalAmount);
        return Task.CompletedTask;
    }
}
