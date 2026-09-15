using NotificationsService.Models;

namespace NotificationsService.Dtos;

public record OrderPlacedWebhookRequest(Guid OrderId, string CustomerEmail, decimal Total);

public record NotificationResponse(Guid Id, Guid OrderId, string Message, DateTimeOffset ReceivedAtUtc)
{
    public static NotificationResponse FromModel(Notification n) => new(n.Id, n.OrderId, n.Message, n.ReceivedAtUtc);
}
