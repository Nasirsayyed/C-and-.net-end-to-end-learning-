using System.Net;
using System.Net.Http.Json;
using OrdersService.Dtos;
using Xunit;

namespace OrdersService.Tests;

public class OrdersApiTests : IClassFixture<OrdersApiFactory>
{
    private readonly OrdersApiFactory _factory;
    private readonly HttpClient _client;

    public OrdersApiTests(OrdersApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_ReturnsCreatedOrder_AndNotifiesNotificationsService()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest("buyer@example.com", 99.99m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.Contains(_factory.FakeNotificationClient.Calls, c => c.OrderId == order!.Id && c.Total == 99.99m);
    }

    [Fact]
    public async Task GetById_ForCreatedOrder_ReturnsIt()
    {
        var created = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest("buyer2@example.com", 10.00m));
        var order = await created.Content.ReadFromJsonAsync<OrderResponse>();

        var response = await _client.GetAsync($"/api/orders/{order!.Id}");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_ForUnknownOrder_Returns404()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_WhenNotificationsServiceFails_StillSucceeds()
    {
        // The key architectural assertion of this whole project (module 32/33): a
        // downstream NotificationsService outage must NOT prevent an order from
        // being placed — the two services' availability must not be coupled.
        _factory.FakeNotificationClient.ShouldThrow = true;
        try
        {
            var response = await _client.PostAsJsonAsync("/api/orders", new CreateOrderRequest("resilient@example.com", 5.00m));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        finally
        {
            _factory.FakeNotificationClient.ShouldThrow = false; // reset for other tests sharing this fixture
        }
    }
}
