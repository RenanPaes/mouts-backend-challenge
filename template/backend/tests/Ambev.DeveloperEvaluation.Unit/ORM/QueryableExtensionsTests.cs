using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Extensions;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM;

/// <summary>
/// Unit tests for the dynamic ordering helper used by the sales listing.
/// </summary>
public class QueryableExtensionsTests
{
    private static IQueryable<Sale> BuildSales()
    {
        return new List<Sale>
        {
            new() { SaleNumber = "B", SaleDate = new DateTime(2026, 1, 2) },
            new() { SaleNumber = "A", SaleDate = new DateTime(2026, 1, 3) },
            new() { SaleNumber = "C", SaleDate = new DateTime(2026, 1, 1) }
        }.AsQueryable();
    }

    [Fact(DisplayName = "Ordering ascending by a single field")]
    public void ApplyOrdering_SingleAscending()
    {
        var ordered = BuildSales().ApplyOrdering("saleNumber asc", nameof(Sale.CreatedAt)).ToList();
        ordered.Select(s => s.SaleNumber).Should().ContainInOrder("A", "B", "C");
    }

    [Fact(DisplayName = "Ordering descending by a single field")]
    public void ApplyOrdering_SingleDescending()
    {
        var ordered = BuildSales().ApplyOrdering("saleDate desc", nameof(Sale.CreatedAt)).ToList();
        ordered.Select(s => s.SaleNumber).Should().ContainInOrder("A", "B", "C");
    }

    [Fact(DisplayName = "Unknown fields are ignored and do not throw")]
    public void ApplyOrdering_UnknownField_Ignored()
    {
        var act = () => BuildSales().ApplyOrdering("nonexistent desc", nameof(Sale.CreatedAt)).ToList();
        act.Should().NotThrow();
    }
}
