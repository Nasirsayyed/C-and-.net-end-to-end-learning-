using Microsoft.EntityFrameworkCore;
using WMS.Application.Interfaces;
using WMS.Domain;

namespace WMS.Infrastructure;

public class StockRepository : IStockRepository
{
    private readonly WmsDbContext _context;

    public StockRepository(WmsDbContext context) => _context = context;

    public Task<StockItem?> GetAsync(string sku, string warehouseId, CancellationToken ct) =>
        _context.StockItems.FirstOrDefaultAsync(s => s.Sku == sku && s.WarehouseId == warehouseId, ct);

    public async Task AddAsync(StockItem item, CancellationToken ct) =>
        await _context.StockItems.AddAsync(item, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
