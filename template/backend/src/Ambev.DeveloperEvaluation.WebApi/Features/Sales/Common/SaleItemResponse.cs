namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.Common;

/// <summary>
/// API response model for a single sale item.
/// </summary>
public class SaleItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public bool IsCancelled { get; set; }
}
