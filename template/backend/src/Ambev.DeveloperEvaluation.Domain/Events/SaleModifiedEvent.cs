using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Events;

/// <summary>
/// Domain event raised when an existing sale is modified.
/// </summary>
public class SaleModifiedEvent
{
    public Sale Sale { get; }

    public SaleModifiedEvent(Sale sale)
    {
        Sale = sale;
    }
}
