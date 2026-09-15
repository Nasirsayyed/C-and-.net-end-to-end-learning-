namespace OrdersService.Models;

public class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CustomerEmail { get; init; }
    public required decimal Total { get; init; }
    public DateTimeOffset PlacedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
