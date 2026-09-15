using WMS.Application.Interfaces;

namespace WMS.Application.UseCases.ReceiveStock;

public record ReceiveStockRequest(string Sku, string WarehouseId, int Quantity);

public class ReceiveStockHandler
{
    private readonly IStockRepository _repository;

    public ReceiveStockHandler(IStockRepository repository) => _repository = repository;

    public async Task HandleAsync(ReceiveStockRequest request, CancellationToken ct)
    {
        var item = await _repository.GetAsync(request.Sku, request.WarehouseId, ct)
            ?? throw new KeyNotFoundException($"Stock item '{request.Sku}' not found in warehouse '{request.WarehouseId}'.");

        item.Receive(request.Quantity); // invariant enforcement happens INSIDE the aggregate, not here
        await _repository.SaveChangesAsync(ct);
    }
}
