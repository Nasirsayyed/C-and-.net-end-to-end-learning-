using WMS.Api;
using WMS.Api.Endpoints;
using WMS.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("Default") ?? "Data Source=wms.db";
builder.Services.AddWmsInfrastructure(connectionString);

var app = builder.Build();

// Applying migrations / ensuring schema exists automatically in Development only —
// production deployments should apply migrations via an explicit release step (see module 21).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WmsDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();

app.MapGroup("/api/stock-items")
    .WithTags("StockItems")
    .MapStockItemEndpoints();

app.Run();

// Exposed so WebApplicationFactory<Program> in the test project can bootstrap this app in-memory.
public partial class Program;
