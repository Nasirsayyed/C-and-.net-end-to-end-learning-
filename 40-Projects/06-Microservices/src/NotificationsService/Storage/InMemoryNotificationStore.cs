using System.Collections.Concurrent;
using NotificationsService.Models;

namespace NotificationsService.Storage;

/// <summary>
/// Registered as a Singleton (see module 18-Dependency-Injection) — it IS the storage.
/// Deduplicates by OrderId so a redelivered "order placed" call (network retry, at-least-once
/// delivery — see module 31-Messaging) never produces a duplicate notification.
/// </summary>
public class InMemoryNotificationStore : INotificationStore
{
    private readonly ConcurrentDictionary<Guid, Notification> _byOrderId = new();

    public bool AddIfNotExists(Notification notification) =>
        _byOrderId.TryAdd(notification.OrderId, notification);

    public IReadOnlyList<Notification> GetAll() =>
        _byOrderId.Values.OrderBy(n => n.ReceivedAtUtc).ToList();
}
