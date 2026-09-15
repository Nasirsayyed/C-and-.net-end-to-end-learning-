namespace ECommerceApi.Models;

public class OrderLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPriceAtPurchase { get; init; } // snapshot — price changes later must not retroactively change past orders
}

public class Order
{
    private readonly List<OrderLine> _lines = new();

    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid CustomerId { get; init; }
    public DateTimeOffset PlacedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(l => l.UnitPriceAtPurchase * l.Quantity);

    // Required by EF Core to materialize the entity (including its private _lines backing field) from the database.
    private Order()
    {
    }

    public Order(Guid customerId, IEnumerable<OrderLine> lines)
    {
        CustomerId = customerId;
        var lineList = lines.ToList();
        if (lineList.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one line.", nameof(lines));
        }
        _lines.AddRange(lineList);
    }
}
