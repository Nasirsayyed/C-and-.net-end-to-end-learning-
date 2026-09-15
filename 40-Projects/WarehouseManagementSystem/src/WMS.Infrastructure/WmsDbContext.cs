using Microsoft.EntityFrameworkCore;
using WMS.Domain;

namespace WMS.Infrastructure;

/// <summary>See module 21-Entity-Framework-Core. This is the ONLY place in the whole
/// solution that knows about EF Core's mapping details for StockItem.</summary>
public class WmsDbContext : DbContext
{
    public WmsDbContext(DbContextOptions<WmsDbContext> options) : base(options)
    {
    }

    public DbSet<StockItem> StockItems => Set<StockItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockItem>(entity =>
        {
            entity.HasKey(nameof(StockItem.Sku), nameof(StockItem.WarehouseId)); // composite key
            entity.Property(e => e.Sku).HasMaxLength(64);
            entity.Property(e => e.WarehouseId).HasMaxLength(64);
        });
    }
}
