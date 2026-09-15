using Microsoft.AspNetCore.Http.HttpResults;
using WMS.Api.Dtos;
using WMS.Application.Interfaces;
using WMS.Application.UseCases.CreateStockItem;
using WMS.Application.UseCases.ReceiveStock;
using WMS.Application.UseCases.ReserveStock;

namespace WMS.Api.Endpoints;

public static class StockItemEndpoints
{
    public static RouteGroupBuilder MapStockItemEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/{sku}/warehouses/{warehouseId}", GetBySkuAndWarehouse);
        group.MapPost("/", Create);
        group.MapPost("/{sku}/warehouses/{warehouseId}/receive", Receive);
        group.MapPost("/{sku}/warehouses/{warehouseId}/reserve", Reserve);
        return group;
    }

    private static async Task<Results<Ok<StockItemResponseDto>, NotFound>> GetBySkuAndWarehouse(
        IStockRepository repository, string sku, string warehouseId, CancellationToken ct)
    {
        var item = await repository.GetAsync(sku, warehouseId, ct);
        return item is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(StockItemResponseDto.FromDomain(item));
    }

    private static async Task<Created<StockItemResponseDto>> Create(
        CreateStockItemHandler handler, CreateStockItemRequestDto request, CancellationToken ct)
    {
        var item = await handler.HandleAsync(
            new CreateStockItemRequest(request.Sku, request.WarehouseId, request.InitialQuantity), ct);

        var response = StockItemResponseDto.FromDomain(item);
        return TypedResults.Created($"/api/stock-items/{item.Sku}/warehouses/{item.WarehouseId}", response);
    }

    private static async Task<NoContent> Receive(
        ReceiveStockHandler handler, string sku, string warehouseId, ReceiveStockRequestDto request, CancellationToken ct)
    {
        await handler.HandleAsync(new ReceiveStockRequest(sku, warehouseId, request.Quantity), ct);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Reserve(
        ReserveStockHandler handler, string sku, string warehouseId, ReserveStockRequestDto request, CancellationToken ct)
    {
        await handler.HandleAsync(new ReserveStockRequest(sku, warehouseId, request.Quantity), ct);
        return TypedResults.NoContent();
    }
}
