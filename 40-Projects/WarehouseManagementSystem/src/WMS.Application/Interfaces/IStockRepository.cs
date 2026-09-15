using WMS.Domain;

namespace WMS.Application.Interfaces;

/// <summary>
/// Defined here in the Application layer, implemented in Infrastructure (see module
/// 26-Clean-Architecture). Application depends only on this abstraction — it never
/// references EF Core or any specific persistence technology directly.
/// Returns and accepts the Aggregate Root only (StockItem), never a bare sub-entity.
/// </summary>
public interface IStockRepository
{
    Task<StockItem?> GetAsync(string sku, string warehouseId, CancellationToken ct);
    Task AddAsync(StockItem item, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
