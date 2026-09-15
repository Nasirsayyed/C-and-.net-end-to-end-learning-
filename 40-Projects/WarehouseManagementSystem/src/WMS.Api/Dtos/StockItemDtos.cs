using WMS.Domain;

namespace WMS.Api.Dtos;

public record CreateStockItemRequestDto(string Sku, string WarehouseId, int InitialQuantity);

public record ReceiveStockRequestDto(int Quantity);

public record ReserveStockRequestDto(int Quantity);

public record StockItemResponseDto(string Sku, string WarehouseId, int QuantityOnHand, int QuantityReserved, int Available)
{
    public static StockItemResponseDto FromDomain(StockItem item) =>
        new(item.Sku, item.WarehouseId, item.QuantityOnHand, item.QuantityReserved, item.Available);
}
