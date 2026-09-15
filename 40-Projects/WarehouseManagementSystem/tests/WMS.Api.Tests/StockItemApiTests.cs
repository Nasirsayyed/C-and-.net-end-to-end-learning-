using System.Net;
using System.Net.Http.Json;
using WMS.Api.Dtos;
using Xunit;

namespace WMS.Api.Tests;

public class StockItemApiTests : IClassFixture<WmsApiFactory>
{
    private readonly HttpClient _client;

    public StockItemApiTests(WmsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ThenGet_ReturnsCreatedStockItem()
    {
        var sku = UniqueSku();
        await CreateAsync(sku, "WH-1", 50);

        var response = await _client.GetAsync($"/api/stock-items/{sku}/warehouses/WH-1");

        response.EnsureSuccessStatusCode();
        var item = await response.Content.ReadFromJsonAsync<StockItemResponseDto>();
        Assert.Equal(50, item!.QuantityOnHand);
        Assert.Equal(50, item.Available);
    }

    [Fact]
    public async Task Get_ForUnknownStockItem_Returns404()
    {
        var response = await _client.GetAsync($"/api/stock-items/{UniqueSku()}/warehouses/WH-1");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Duplicate_Returns409Conflict()
    {
        var sku = UniqueSku();
        await CreateAsync(sku, "WH-1", 10);

        var response = await _client.PostAsJsonAsync("/api/stock-items",
            new CreateStockItemRequestDto(sku, "WH-1", 5));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Receive_IncreasesQuantityOnHand()
    {
        var sku = UniqueSku();
        await CreateAsync(sku, "WH-1", 10);

        var response = await _client.PostAsJsonAsync(
            $"/api/stock-items/{sku}/warehouses/WH-1/receive", new ReceiveStockRequestDto(5));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var item = await _client.GetFromJsonAsync<StockItemResponseDto>($"/api/stock-items/{sku}/warehouses/WH-1");
        Assert.Equal(15, item!.QuantityOnHand);
    }

    [Fact]
    public async Task Reserve_WithinAvailableStock_Succeeds()
    {
        var sku = UniqueSku();
        await CreateAsync(sku, "WH-1", 10);

        var response = await _client.PostAsJsonAsync(
            $"/api/stock-items/{sku}/warehouses/WH-1/reserve", new ReserveStockRequestDto(4));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var item = await _client.GetFromJsonAsync<StockItemResponseDto>($"/api/stock-items/{sku}/warehouses/WH-1");
        Assert.Equal(4, item!.QuantityReserved);
        Assert.Equal(6, item.Available);
    }

    [Fact]
    public async Task Reserve_ExceedingAvailableStock_Returns409Conflict()
    {
        var sku = UniqueSku();
        await CreateAsync(sku, "WH-1", 5);

        var response = await _client.PostAsJsonAsync(
            $"/api/stock-items/{sku}/warehouses/WH-1/reserve", new ReserveStockRequestDto(10));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        // The failed reservation attempt must not have partially applied — this is the
        // whole point of enforcing the invariant inside the aggregate (module 27-DDD).
        var item = await _client.GetFromJsonAsync<StockItemResponseDto>($"/api/stock-items/{sku}/warehouses/WH-1");
        Assert.Equal(0, item!.QuantityReserved);
    }

    [Fact]
    public async Task Reserve_ForUnknownStockItem_Returns404()
    {
        var response = await _client.PostAsJsonAsync(
            $"/api/stock-items/{UniqueSku()}/warehouses/WH-1/reserve", new ReserveStockRequestDto(1));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task CreateAsync(string sku, string warehouseId, int initialQuantity)
    {
        var response = await _client.PostAsJsonAsync("/api/stock-items",
            new CreateStockItemRequestDto(sku, warehouseId, initialQuantity));
        response.EnsureSuccessStatusCode();
    }

    private static string UniqueSku() => $"SKU-{Guid.NewGuid():N}";
}
