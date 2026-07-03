using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Query for retrieving a paginated, ordered list of sales.
/// </summary>
public class ListSalesQuery : IRequest<ListSalesResult>
{
    /// <summary>1-based page number (default: 1).</summary>
    public int Page { get; set; } = 1;

    /// <summary>Page size (default: 10).</summary>
    public int Size { get; set; } = 10;

    /// <summary>Optional ordering expression (e.g. "saleDate desc, saleNumber asc").</summary>
    public string? Order { get; set; }
}
