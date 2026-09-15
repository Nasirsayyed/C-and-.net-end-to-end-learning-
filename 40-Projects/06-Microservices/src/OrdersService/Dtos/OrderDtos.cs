using OrdersService.Models;

namespace OrdersService.Dtos;

public record CreateOrderRequest(string CustomerEmail, decimal Total);

public record OrderResponse(Guid Id, string CustomerEmail, decimal Total, DateTimeOffset PlacedAtUtc)
{
    public static OrderResponse FromModel(Order order) => new(order.Id, order.CustomerEmail, order.Total, order.PlacedAtUtc);
}

/// <summary>
/// The wire contract this service PUBLISHES to NotificationsService. Deliberately a
/// separate, local type — not a shared library reference to NotificationsService's own
/// DTO — because independently deployable services should never share compiled contracts
/// (that reintroduces coupling); they agree on a wire shape instead (see module
/// 32-Microservices on service boundaries).
/// </summary>
public record OrderPlacedWebhookPayload(Guid OrderId, string CustomerEmail, decimal Total);
