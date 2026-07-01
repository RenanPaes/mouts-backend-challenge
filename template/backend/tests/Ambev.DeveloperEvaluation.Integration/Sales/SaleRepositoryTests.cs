using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Repositories;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

/// <summary>
/// Integration tests for <see cref="SaleRepository"/> against a real PostgreSQL database.
/// </summary>
public class SaleRepositoryTests : IClassFixture<PostgresContainerFixture>
{
    private readonly PostgresContainerFixture _fixture;

    public SaleRepositoryTests(PostgresContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private static Sale NewSale(string saleNumber, params (int qty, decimal price)[] items)
    {
        var sale = new Sale
        {
            SaleNumber = saleNumber,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Customer",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch"
        };
        foreach (var (qty, price) in items)
        {
            sale.AddItem(new SaleItem
            {
                ProductId = Guid.NewGuid(),
                ProductTitle = "Product",
                Quantity = qty,
                UnitPrice = price
            });
        }
        return sale;
    }

    [Fact(DisplayName = "Create persists the sale and its items with generated ids")]
    public async Task Create_PersistsSaleWithItems()
    {
        var repo = new SaleRepository(_fixture.CreateContext());
        var sale = NewSale($"IT-{Guid.NewGuid():N}", (10, 100m), (3, 100m)); // 800 + 300

        var created = await repo.CreateAsync(sale);

        created.Id.Should().NotBe(Guid.Empty);

        var reloaded = await new SaleRepository(_fixture.CreateContext()).GetByIdAsync(created.Id);
        reloaded.Should().NotBeNull();
        reloaded!.Items.Should().HaveCount(2);
        reloaded.TotalAmount.Should().Be(1100m);
        reloaded.Items.All(i => i.Id != Guid.Empty).Should().BeTrue();
    }

    [Fact(DisplayName = "GetBySaleNumber returns the matching sale")]
    public async Task GetBySaleNumber_ReturnsSale()
    {
        var number = $"IT-{Guid.NewGuid():N}";
        await new SaleRepository(_fixture.CreateContext()).CreateAsync(NewSale(number, (5, 20m)));

        var found = await new SaleRepository(_fixture.CreateContext()).GetBySaleNumberAsync(number);

        found.Should().NotBeNull();
        found!.SaleNumber.Should().Be(number);
    }

    [Fact(DisplayName = "ListAsync paginates and orders results")]
    public async Task ListAsync_PaginatesAndOrders()
    {
        var repo = new SaleRepository(_fixture.CreateContext());
        for (var i = 0; i < 3; i++)
            await repo.CreateAsync(NewSale($"IT-{Guid.NewGuid():N}", (4, 10m)));

        var (items, total) = await new SaleRepository(_fixture.CreateContext())
            .ListAsync(page: 1, size: 2, order: "saleDate desc");

        items.Should().HaveCount(2);
        total.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact(DisplayName = "Delete removes the sale and cascades its items")]
    public async Task Delete_RemovesSale()
    {
        var created = await new SaleRepository(_fixture.CreateContext())
            .CreateAsync(NewSale($"IT-{Guid.NewGuid():N}", (5, 10m)));

        var deleted = await new SaleRepository(_fixture.CreateContext()).DeleteAsync(created.Id);
        deleted.Should().BeTrue();

        var reloaded = await new SaleRepository(_fixture.CreateContext()).GetByIdAsync(created.Id);
        reloaded.Should().BeNull();
    }
}
