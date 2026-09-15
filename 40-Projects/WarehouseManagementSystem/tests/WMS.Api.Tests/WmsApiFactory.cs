using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WMS.Infrastructure;

namespace WMS.Api.Tests;

/// <summary>
/// Replaces the real SQLite file connection with a SQLite IN-MEMORY connection that
/// stays open for the factory's lifetime — real relational engine behavior (unlike
/// EF Core's InMemory provider), but fully isolated and disposable per test class.
/// See module 34-Testing for the reasoning behind WebApplicationFactory-based
/// integration tests.
/// </summary>
public class WmsApiFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open(); // must stay open for the in-memory DB to persist across the test

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<WmsDbContext>>();
            services.AddDbContext<WmsDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public new void Dispose()
    {
        _connection.Dispose();
        base.Dispose();
    }
}
