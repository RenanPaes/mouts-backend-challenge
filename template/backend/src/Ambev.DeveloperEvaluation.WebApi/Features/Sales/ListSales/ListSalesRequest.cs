using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

/// <summary>
/// Query parameters for listing sales, following the general API pagination/ordering convention.
/// </summary>
public class ListSalesRequest
{
    /// <summary>Page number (default: 1).</summary>
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    /// <summary>Page size (default: 10).</summary>
    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    /// <summary>Ordering expression, e.g. "saleDate desc, saleNumber asc".</summary>
    [FromQuery(Name = "_order")]
    public string? Order { get; set; }
}
