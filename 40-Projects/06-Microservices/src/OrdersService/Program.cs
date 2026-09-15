using Microsoft.AspNetCore.Http.HttpResults;
using OrdersService.Clients;
using OrdersService.Dtos;
using OrdersService.Storage;
using Polly;
using Polly.Extensions.Http;

namespace OrdersService;

// Classic Main method for the same reason as NotificationsService — see its Program.cs
// for why top-level statements would collide across the two services' assemblies.
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<IOrderStore, InMemoryOrderStore>();

        var notificationsBaseUrl = builder.Configuration["NotificationsService:BaseUrl"] ?? "http://localhost:5051";

        // See module 33-Resilience: retry with exponential backoff + jitter, so a brief
        // blip in NotificationsService doesn't fail the whole call on the first attempt —
        // but see the endpoint below for why a notification failure still must NOT fail
        // the order itself.
        builder.Services.AddHttpClient<INotificationClient, NotificationClient>(client =>
            {
                client.BaseAddress = new Uri(notificationsBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(5);
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt =>
                    TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 100))));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapPost("/api/orders", async Task<Created<OrderResponse>> (
            IOrderStore store, INotificationClient notificationClient, ILogger<Program> logger,
            CreateOrderRequest request, CancellationToken ct) =>
        {
            var order = new Models.Order { CustomerEmail = request.CustomerEmail, Total = request.Total };
            store.Add(order);

            try
            {
                await notificationClient.NotifyOrderPlacedAsync(order.Id, order.CustomerEmail, order.Total, ct);
            }
            catch (Exception ex)
            {
                // Deliberate design choice (module 32-Microservices / 33-Resilience):
                // the order has ALREADY succeeded — a downstream notification failure
                // must not roll it back or fail this request. In production this gap
                // would be closed with a durable outbox + message queue (module
                // 31-Messaging) rather than a best-effort HTTP call; logging here is the
                // honest, minimal version of that for a synchronous HTTP demo.
                logger.LogWarning(ex, "Failed to notify NotificationsService for order {OrderId}", order.Id);
            }

            var response = OrderResponse.FromModel(order);
            return TypedResults.Created($"/api/orders/{order.Id}", response);
        });

        app.MapGet("/api/orders/{id:guid}", (IOrderStore store, Guid id) =>
        {
            var order = store.GetById(id);
            return order is null ? Results.NotFound() : Results.Ok(OrderResponse.FromModel(order));
        });

        app.Run();
    }
}
