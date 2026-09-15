namespace OrdersService.Clients;

/// <summary>
/// The Application layer's port to the Notifications service — see module
/// 32-Microservices: OrdersService never talks to NotificationsService's database or
/// internals, only this HTTP contract.
/// </summary>
public interface INotificationClient
{
    Task NotifyOrderPlacedAsync(Guid orderId, string customerEmail, decimal total, CancellationToken ct);
}
