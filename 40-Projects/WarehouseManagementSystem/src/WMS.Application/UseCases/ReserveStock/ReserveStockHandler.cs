using WMS.Application.Interfaces;

namespace WMS.Application.UseCases.ReserveStock;

public record ReserveStockRequest(string Sku, string WarehouseId, int Quantity);

public class ReserveStockHandler
{
    private readonly IStockRepository _repository;

    public ReserveStockHandler(IStockRepository repository) => _repository = repository;

    public async Task HandleAsync(ReserveStockRequest request, CancellationToken ct)
    {
        var item = await _repository.GetAsync(request.Sku, request.WarehouseId, ct)
            ?? throw new KeyNotFoundException($"Stock item '{request.Sku}' not found in warehouse '{request.WarehouseId}'.");

        // InsufficientStockException propagates from here straight out of the use case —
        // the Application layer doesn't catch/reinterpret it; the API's global exception
        // handler (module 11-Exception-Handling / 17-Middleware) maps it to a 409 Conflict.
        item.Reserve(request.Quantity);
        await _repository.SaveChangesAsync(ct);
    }
}
