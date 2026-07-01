using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Sales;

/// <summary>
/// xUnit fixture that starts a throwaway PostgreSQL container (matching the
/// docker-compose image) and applies the EF Core migrations once per test class.
/// </summary>
public class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("developer_evaluation_test")
        .WithUsername("developer")
        .WithPassword("ev@luAt10n")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>Creates a new <see cref="DefaultContext"/> bound to the container database.</summary>
    public DefaultContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(ConnectionString, b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM"))
            .Options;
        return new DefaultContext(options);
    }
}
