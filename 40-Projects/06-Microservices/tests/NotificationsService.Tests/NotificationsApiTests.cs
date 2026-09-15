using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using NotificationsService.Dtos;
using Xunit;

namespace NotificationsService.Tests;

public class NotificationsApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public NotificationsApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task PostOrderPlaced_RecordsNotification()
    {
        var orderId = Guid.NewGuid();
        var response = await _client.PostAsJsonAsync("/api/notifications/order-placed",
            new OrderPlacedWebhookRequest(orderId, "customer@example.com", 42.50m));

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var all = await _client.GetFromJsonAsync<List<NotificationResponse>>("/api/notifications");
        Assert.Contains(all!, n => n.OrderId == orderId);
    }

    [Fact]
    public async Task PostOrderPlaced_Redelivered_IsIdempotent()
    {
        // Simulates at-least-once delivery (module 31-Messaging): the same webhook call
        // arrives twice (e.g. after a network retry) — it must not create a duplicate.
        var orderId = Guid.NewGuid();
        var payload = new OrderPlacedWebhookRequest(orderId, "customer@example.com", 10.00m);

        await _client.PostAsJsonAsync("/api/notifications/order-placed", payload);
        await _client.PostAsJsonAsync("/api/notifications/order-placed", payload); // redelivered

        var all = await _client.GetFromJsonAsync<List<NotificationResponse>>("/api/notifications");
        Assert.Single(all!, n => n.OrderId == orderId);
    }
}
