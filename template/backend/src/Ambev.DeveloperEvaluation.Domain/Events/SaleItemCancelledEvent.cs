using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when a single item within a sale is cancelled.
/// </summary>
public class SaleItemCancelledEvent
{
    public Sale Sale { get; }
    public SaleItem Item { get; }

    public SaleItemCancelledEvent(Sale sale, SaleItem item)
    {
        Sale = sale;
        Item = item;
    }
}
