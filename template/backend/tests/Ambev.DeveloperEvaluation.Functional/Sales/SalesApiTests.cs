using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// End-to-end functional tests exercising the Sales API over HTTP against a real database.
/// </summary>
public class SalesApiTests : IClassFixture<SalesApiFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public SalesApiTests(SalesApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object BuildSalePayload(string saleNumber, int quantity, decimal unitPrice) => new
    {
        saleNumber,
        saleDate = DateTime.UtcNow,
        customerId = Guid.NewGuid(),
        customerName = "Customer",
        branchId = Guid.NewGuid(),
        branchName = "Branch",
        items = new[]
        {
            new { productId = Guid.NewGuid(), productTitle = "Product", quantity, unitPrice }
        }
    };

    private static string UniqueNumber() => $"FN-{Guid.NewGuid():N}";

    [Fact(DisplayName = "POST creates a sale and applies the discount")]
    public async Task Post_CreatesSale_WithDiscount()
    {
        // 10 units at 100 => 20% discount => 800
        var response = await _client.PostAsJsonAsync("/api/sales", BuildSalePayload(UniqueNumber(), 10, 100m));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("success").GetBoolean().Should().BeTrue();
        var data = body.GetProperty("data");
        data.GetProperty("totalAmount").GetDecimal().Should().Be(800m);
        data.GetProperty("items")[0].GetProperty("discountPercentage").GetDecimal().Should().Be(0.20m);
    }

    [Fact(DisplayName = "GET returns a previously created sale")]
    public async Task Get_ReturnsSale()
    {
        var create = await _client.PostAsJsonAsync("/api/sales", BuildSalePayload(UniqueNumber(), 5, 50m));
        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("data").GetProperty("id").GetGuid();

        var response = await _client.GetAsync($"/api/sales/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "GET list returns paginated envelope")]
    public async Task List_ReturnsPaginated()
    {
        await _client.PostAsJsonAsync("/api/sales", BuildSalePayload(UniqueNumber(), 4, 10m));

        var response = await _client.GetAsync("/api/sales?_page=1&_size=5&_order=saleDate desc");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("currentPage").GetInt32().Should().Be(1);
        body.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact(DisplayName = "POST with quantity above 20 returns 400 in the standard error shape")]
    public async Task Post_QuantityAboveLimit_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/sales", BuildSalePayload(UniqueNumber(), 21, 10m));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("type").GetString().Should().Be("ValidationError");
        body.GetProperty("detail").GetString().Should().Contain("20 identical items");
    }

    [Fact(DisplayName = "GET missing sale returns 404 ResourceNotFound")]
    public async Task Get_Missing_Returns404()
    {
        var response = await _client.GetAsync($"/api/sales/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("type").GetString().Should().Be("ResourceNotFound");
    }

    [Fact(DisplayName = "POST duplicate sale number returns 409 ConflictError")]
    public async Task Post_Duplicate_Returns409()
    {
        var number = UniqueNumber();
        await _client.PostAsJsonAsync("/api/sales", BuildSalePayload(number, 5, 10m));

        var response = await _client.PostAsJsonAsync("/api/sales", BuildSalePayload(number, 5, 10m));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("type").GetString().Should().Be("ConflictError");
    }

    [Fact(DisplayName = "POST cancel sets the sale as cancelled and zeroes the total")]
    public async Task Cancel_Sale_ZeroesTotal()
    {
        var create = await _client.PostAsJsonAsync("/api/sales", BuildSalePayload(UniqueNumber(), 5, 100m));
        var id = (await create.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data").GetProperty("id").GetGuid();

        var response = await _client.PostAsync($"/api/sales/{id}/cancel", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = (await response.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("data");
        data.GetProperty("isCancelled").GetBoolean().Should().BeTrue();
        data.GetProperty("totalAmount").GetDecimal().Should().Be(0m);
    }
}
