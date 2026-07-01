using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Domain.Entities.TestData;

/// <summary>
/// Provides methods for generating Sale/SaleItem test data using the Bogus library.
/// Centralizes test data generation to ensure consistency across test cases.
/// </summary>
public static class SaleTestData
{
    private static readonly Faker<SaleItem> SaleItemFaker = new Faker<SaleItem>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid())
        .RuleFor(i => i.ProductTitle, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, SaleItem.MaxQuantity))
        .RuleFor(i => i.UnitPrice, f => decimal.Round(f.Random.Decimal(1, 500), 2));

    private static readonly Faker<Sale> SaleFaker = new Faker<Sale>()
        .RuleFor(s => s.SaleNumber, f => f.Commerce.Ean13())
        .RuleFor(s => s.SaleDate, f => f.Date.Recent())
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName());

    /// <summary>
    /// Generates a valid sale item with a randomized, in-range quantity.
    /// </summary>
    public static SaleItem GenerateValidItem()
    {
        return SaleItemFaker.Generate();
    }

    /// <summary>
    /// Generates a valid sale item with a specific quantity and unit price.
    /// </summary>
    public static SaleItem GenerateItem(int quantity, decimal unitPrice = 100m)
    {
        var item = SaleItemFaker.Generate();
        item.Quantity = quantity;
        item.UnitPrice = unitPrice;
        return item;
    }

    /// <summary>
    /// Generates a valid sale (without items attached).
    /// </summary>
    public static Sale GenerateValidSale()
    {
        return SaleFaker.Generate();
    }

    /// <summary>
    /// Generates a valid sale already populated with the given number of valid items.
    /// </summary>
    public static Sale GenerateSaleWithItems(int itemCount = 3)
    {
        var sale = SaleFaker.Generate();
        for (var i = 0; i < itemCount; i++)
            sale.AddItem(GenerateValidItem());
        return sale;
    }
}
