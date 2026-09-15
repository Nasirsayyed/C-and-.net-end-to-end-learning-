using WMS.Domain.Exceptions;

namespace WMS.Domain;

/// <summary>
/// Aggregate Root (see module 27-DDD). ALL mutation of quantity happens through this
/// class's own methods — there are no public setters — so the "quantity can never go
/// negative" invariant cannot be bypassed by any caller, anywhere in the system.
/// This class has ZERO references to EF Core, ASP.NET Core, or any other framework
/// (see module 26-Clean-Architecture): it is pure C#, testable in complete isolation.
/// </summary>
public class StockItem
{
    public string Sku { get; private set; } = string.Empty;
    public string WarehouseId { get; private set; } = string.Empty;
    public int QuantityOnHand { get; private set; }
    public int QuantityReserved { get; private set; }

    public int Available => QuantityOnHand - QuantityReserved;

    // Required by EF Core to materialize entities from the database via reflection —
    // never used by application code, which must go through the constructor below.
    private StockItem()
    {
    }

    public StockItem(string sku, string warehouseId, int initialQuantity = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU cannot be empty.", nameof(sku));
        }
        if (string.IsNullOrWhiteSpace(warehouseId))
        {
            throw new ArgumentException("Warehouse id cannot be empty.", nameof(warehouseId));
        }
        if (initialQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialQuantity), "Initial quantity cannot be negative.");
        }

        Sku = sku;
        WarehouseId = warehouseId;
        QuantityOnHand = initialQuantity;
    }

    /// <summary>Receiving inbound stock always increases quantity on hand.</summary>
    public void Receive(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Received quantity must be positive.");
        }
        QuantityOnHand += quantity;
    }

    /// <summary>Reserve stock for an order. Cannot exceed what's actually available.</summary>
    public void Reserve(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Reserved quantity must be positive.");
        }
        if (quantity > Available)
        {
            throw new InsufficientStockException(Sku, quantity, Available);
        }
        QuantityReserved += quantity;
    }

    /// <summary>Release a previously made reservation (e.g. order cancelled) without changing quantity on hand.</summary>
    public void Release(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Released quantity must be positive.");
        }
        if (quantity > QuantityReserved)
        {
            throw new InvalidOperationException(
                $"Cannot release {quantity} units for {Sku}; only {QuantityReserved} are currently reserved.");
        }
        QuantityReserved -= quantity;
    }

    /// <summary>Ship previously reserved stock — removes it from both on-hand and reserved quantities.</summary>
    public void ShipReserved(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Shipped quantity must be positive.");
        }
        if (quantity > QuantityReserved)
        {
            throw new InvalidOperationException(
                $"Cannot ship {quantity} units for {Sku}; only {QuantityReserved} are currently reserved.");
        }
        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
    }
}
