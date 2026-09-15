using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WMS.Application.Interfaces;
using WMS.Application.UseCases.CreateStockItem;
using WMS.Application.UseCases.ReceiveStock;
using WMS.Application.UseCases.ReserveStock;

namespace WMS.Infrastructure;

/// <summary>
/// Composition root helper (see module 18-Dependency-Injection). The API project calls
/// this ONE method instead of knowing about EF Core or repository implementations directly.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddWmsInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<WmsDbContext>(options => options.UseSqlite(connectionString));

        // Scoped: matches DbContext's own lifetime (see module 18) — a fresh repository
        // per request/scope, never shared across concurrent operations.
        services.AddScoped<IStockRepository, StockRepository>();

        services.AddScoped<CreateStockItemHandler>();
        services.AddScoped<ReceiveStockHandler>();
        services.AddScoped<ReserveStockHandler>();

        return services;
    }
}
