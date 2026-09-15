using WMS.Application.Interfaces;
using WMS.Domain;

namespace WMS.Application.UseCases.CreateStockItem;

public record CreateStockItemRequest(string Sku, string WarehouseId, int InitialQuantity);

/// <summary>
/// Application-layer use case: orchestrates loading/saving via the repository and
/// invoking domain logic. Contains NO business rules itself — those live in
/// StockItem (see module 27-DDD's distinction between Domain Service and
/// Application Service).
/// </summary>
public class CreateStockItemHandler
{
    private readonly IStockRepository _repository;

    public CreateStockItemHandler(IStockRepository repository) => _repository = repository;

    public async Task<StockItem> HandleAsync(CreateStockItemRequest request, CancellationToken ct)
    {
        var existing = await _repository.GetAsync(request.Sku, request.WarehouseId, ct);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Stock item '{request.Sku}' already exists in warehouse '{request.WarehouseId}'.");
        }

        var item = new StockItem(request.Sku, request.WarehouseId, request.InitialQuantity);
        await _repository.AddAsync(item, ct);
        await _repository.SaveChangesAsync(ct);
        return item;
    }
}
