using System.Net;
using System.Net.Http.Json;
using ECommerceApi.Dtos;
using Xunit;

namespace ECommerceApi.Tests;

public class ProductApiTests : IClassFixture<ECommerceApiFactory>
{
    private readonly ECommerceApiFactory _factory;
    private readonly HttpClient _client;

    public ProductApiTests(ECommerceApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_WithNoAuth_Succeeds()
    {
        // Product browsing is intentionally public — no [Authorize] on the GET endpoints.
        var response = await _client.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Create_AsAnonymous_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", 9.99m, 100));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsCustomer_ReturnsForbidden()
    {
        var token = await TestHelpers.RegisterAndLoginAsync(_client);
        TestHelpers.UseToken(_client, token);

        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", 9.99m, 100));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_AsAdmin_Succeeds()
    {
        var token = await TestHelpers.CreateAdminAndLoginAsync(_factory, _client);
        TestHelpers.UseToken(_client, token);

        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest("Widget", 9.99m, 100));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        Assert.Equal("Widget", product!.Name);
    }
}
