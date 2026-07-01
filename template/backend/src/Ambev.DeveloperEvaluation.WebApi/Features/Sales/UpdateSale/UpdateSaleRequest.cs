namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Request model for updating an existing sale.
/// </summary>
public class UpdateSaleRequest
{
    public string SaleNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public List<UpdateSaleItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Request model for a single item within an <see cref="UpdateSaleRequest"/>.
/// </summary>
public class UpdateSaleItemRequest
{
    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
