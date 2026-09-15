namespace NotificationsService.Models;

public class Notification
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid OrderId { get; init; }
    public required string Message { get; init; }
    public DateTimeOffset ReceivedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
