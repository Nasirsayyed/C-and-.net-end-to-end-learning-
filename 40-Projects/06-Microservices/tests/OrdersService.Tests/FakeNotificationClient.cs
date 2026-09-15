using OrdersService.Clients;

namespace OrdersService.Tests;

/// <summary>A test double (see module 34-Testing) replacing the real HTTP call so
/// OrdersService's own endpoint logic can be tested in isolation from NotificationsService.</summary>
public class FakeNotificationClient : INotificationClient
{
    public List<(Guid OrderId, string CustomerEmail, decimal Total)> Calls { get; } = new();
    public bool ShouldThrow { get; set; }

    public Task NotifyOrderPlacedAsync(Guid orderId, string customerEmail, decimal total, CancellationToken ct)
    {
        if (ShouldThrow)
        {
            throw new HttpRequestException("Simulated NotificationsService outage.");
        }
        Calls.Add((orderId, customerEmail, total));
        return Task.CompletedTask;
    }
}
