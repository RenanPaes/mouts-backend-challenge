using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales.TestData;

/// <summary>
/// Provides Bogus-generated commands and entities for the sale application handler tests.
/// </summary>
public static class SaleHandlerTestData
{
    private static readonly Faker<CreateSaleItemCommand> CreateItemFaker = new Faker<CreateSaleItemCommand>()
        .RuleFor(i => i.ProductId, f => f.Random.Guid())
        .RuleFor(i => i.ProductTitle, f => f.Commerce.ProductName())
        .RuleFor(i => i.Quantity, f => f.Random.Int(1, SaleItem.MaxQuantity))
        .RuleFor(i => i.UnitPrice, f => decimal.Round(f.Random.Decimal(1, 500), 2));

    private static readonly Faker<CreateSaleCommand> CreateSaleFaker = new Faker<CreateSaleCommand>()
        .RuleFor(s => s.SaleNumber, f => f.Commerce.Ean13())
        .RuleFor(s => s.SaleDate, f => f.Date.Recent())
        .RuleFor(s => s.CustomerId, f => f.Random.Guid())
        .RuleFor(s => s.CustomerName, f => f.Person.FullName)
        .RuleFor(s => s.BranchId, f => f.Random.Guid())
        .RuleFor(s => s.BranchName, f => f.Company.CompanyName())
        .RuleFor(s => s.Items, _ => CreateItemFaker.Generate(2));

    /// <summary>Generates a valid CreateSaleCommand with random items.</summary>
    public static CreateSaleCommand GenerateValidCreateCommand()
    {
        return CreateSaleFaker.Generate();
    }

    /// <summary>Generates a valid CreateSaleCommand with a specific single item.</summary>
    public static CreateSaleCommand GenerateCreateCommandWithItem(int quantity, decimal unitPrice)
    {
        var command = CreateSaleFaker.Generate();
        command.Items = new List<CreateSaleItemCommand>
        {
            new()
            {
                ProductId = Guid.NewGuid(),
                ProductTitle = "Test Product",
                Quantity = quantity,
                UnitPrice = unitPrice
            }
        };
        return command;
    }

    /// <summary>Generates a valid UpdateSaleCommand for the given sale id.</summary>
    public static UpdateSaleCommand GenerateValidUpdateCommand(Guid saleId)
    {
        return new UpdateSaleCommand
        {
            Id = saleId,
            SaleNumber = new Faker().Commerce.Ean13(),
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = new Faker().Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = new Faker().Company.CompanyName(),
            Items = new List<UpdateSaleItemCommand>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductTitle = "Updated Product",
                    Quantity = 5,
                    UnitPrice = 100m
                }
            }
        };
    }

    /// <summary>Builds a persisted-like Sale entity with the given items applied through the aggregate.</summary>
    public static Sale GenerateSaleEntity(int itemCount = 2)
    {
        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = new Faker().Commerce.Ean13(),
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = new Faker().Person.FullName,
            BranchId = Guid.NewGuid(),
            BranchName = new Faker().Company.CompanyName()
        };

        for (var i = 0; i < itemCount; i++)
        {
            sale.AddItem(new SaleItem
            {
                Id = Guid.NewGuid(),
                ProductId = Guid.NewGuid(),
                ProductTitle = "Product",
                Quantity = 5,
                UnitPrice = 100m
            });
        }

        return sale;
    }
}
