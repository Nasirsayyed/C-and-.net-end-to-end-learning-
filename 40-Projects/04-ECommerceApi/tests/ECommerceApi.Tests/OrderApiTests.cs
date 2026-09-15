using System.Net;
using System.Net.Http.Json;
using ECommerceApi.Dtos;
using Xunit;

namespace ECommerceApi.Tests;

public class OrderApiTests : IClassFixture<ECommerceApiFactory>
{
    private readonly ECommerceApiFactory _factory;
    private readonly HttpClient _client;

    public OrderApiTests(ECommerceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_AsAnonymous_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest(new List<CreateOrderLineRequest>()));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_WithValidProduct_ComputesTotalAndDecrementsStock()
    {
        var product = await CreateProductAsync("Gadget", price: 25.00m, stock: 10);
        var customerToken = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, customerToken);

        var response = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new List<CreateOrderLineRequest> { new(product.Id, 3) }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(75.00m, order!.Total);

        var updatedProduct = await GetProductAsync(product.Id);
        Assert.Equal(7, updatedProduct.StockQuantity);
    }

    [Fact]
    public async Task Create_ExceedingStock_ReturnsConflict_AndDoesNotDecrementStock()
    {
        var product = await CreateProductAsync("Limited Item", price: 10.00m, stock: 2);
        var customerToken = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, customerToken);

        var response = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new List<CreateOrderLineRequest> { new(product.Id, 5) }));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var updatedProduct = await GetProductAsync(product.Id);
        Assert.Equal(2, updatedProduct.StockQuantity); // unchanged — the failed order must not have partially applied
    }

    [Fact]
    public async Task GetById_AsOwner_Succeeds()
    {
        var product = await CreateProductAsync("Book", price: 15.00m, stock: 10);
        var token = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, token);
        var orderId = await CreateOrderAsync(product.Id, quantity: 1);

        var response = await _client.GetAsync($"/api/orders/{orderId}");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_AsDifferentCustomer_ReturnsForbidden()
    {
        // This is the anti-IDOR test (module 23-Security): customer A's order must NOT
        // be readable by customer B just because B is authenticated and knows the id.
        var product = await CreateProductAsync("Exclusive Item", price: 50.00m, stock: 10);

        var ownerToken = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, ownerToken);
        var orderId = await CreateOrderAsync(product.Id, quantity: 1);

        var otherToken = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, otherToken);

        var response = await _client.GetAsync($"/api/orders/{orderId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_AsAdmin_CanReadAnyOrder()
    {
        var product = await CreateProductAsync("Admin-Visible Item", price: 20.00m, stock: 10);

        var ownerToken = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, ownerToken);
        var orderId = await CreateOrderAsync(product.Id, quantity: 1);

        var adminToken = await TestHelpers.CreateAdminAndLoginAsync(_factory, _client);
        TestHelpers.UseToken(_client, adminToken);

        var response = await _client.GetAsync($"/api/orders/{orderId}");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetMine_OnlyReturnsCallersOwnOrders()
    {
        var product = await CreateProductAsync("Shared Product", price: 5.00m, stock: 100);

        var tokenA = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, tokenA);
        await CreateOrderAsync(product.Id, quantity: 1);

        var tokenB = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, tokenB);
        var orderIdB = await CreateOrderAsync(product.Id, quantity: 1);

        var response = await _client.GetFromJsonAsync<List<OrderResponse>>("/api/orders/mine");

        Assert.NotNull(response);
        Assert.All(response!, o => Assert.Equal(orderIdB, o.Id)); // every order returned belongs to caller B
        Assert.Contains(response!, o => o.Id == orderIdB);
    }

    private async Task<ProductResponse> CreateProductAsync(string name, decimal price, int stock)
    {
        var adminToken = await TestHelpers.CreateAdminAndLoginAsync(_factory, _client);
        TestHelpers.UseToken(_client, adminToken);

        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest(name, price, stock));
        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();

        _client.DefaultRequestHeaders.Authorization = null; // reset so callers set their own token next
        return product!;
    }

    private async Task<ProductResponse> GetProductAsync(Guid id)
    {
        var savedAuth = _client.DefaultRequestHeaders.Authorization;
        _client.DefaultRequestHeaders.Authorization = null;
        var product = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{id}");
        _client.DefaultRequestHeaders.Authorization = savedAuth;
        return product!;
    }

    private async Task<Guid> CreateOrderAsync(Guid productId, int quantity)
    {
        var response = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest(new List<CreateOrderLineRequest> { new(productId, quantity) }));
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        return order!.Id;
    }
}
