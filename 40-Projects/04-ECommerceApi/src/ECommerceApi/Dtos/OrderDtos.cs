using ECommerceApi.Models;

namespace ECommerceApi.Dtos;

public record CreateOrderLineRequest(Guid ProductId, int Quantity);

public record CreateOrderRequest(List<CreateOrderLineRequest> Lines);

public record OrderLineResponse(Guid ProductId, int Quantity, decimal UnitPriceAtPurchase);

public record OrderResponse(Guid Id, Guid CustomerId, DateTimeOffset PlacedAtUtc, decimal Total, List<OrderLineResponse> Lines)
{
    public static OrderResponse FromModel(Order order) => new(
        order.Id,
        order.CustomerId,
        order.PlacedAtUtc,
        order.Total,
        order.Lines.Select(l => new OrderLineResponse(l.ProductId, l.Quantity, l.UnitPriceAtPurchase)).ToList());
}
