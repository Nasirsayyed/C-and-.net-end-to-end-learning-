using System.Net.Http.Json;
using OrdersService.Dtos;

namespace OrdersService.Clients;

public class NotificationClient : INotificationClient
{
    private readonly HttpClient _httpClient;

    public NotificationClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task NotifyOrderPlacedAsync(Guid orderId, string customerEmail, decimal total, CancellationToken ct)
    {
        var payload = new OrderPlacedWebhookPayload(orderId, customerEmail, total);
        var response = await _httpClient.PostAsJsonAsync("/api/notifications/order-placed", payload, ct);
        response.EnsureSuccessStatusCode();
    }
}
