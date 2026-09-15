using NotificationsService.Models;

namespace NotificationsService.Storage;

public interface INotificationStore
{
    /// <returns>true if a new notification was recorded; false if this OrderId was already processed (idempotent no-op).</returns>
    bool AddIfNotExists(Notification notification);
    IReadOnlyList<Notification> GetAll();
}
