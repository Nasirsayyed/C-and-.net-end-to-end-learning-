using Microsoft.AspNetCore.Http.HttpResults;
using NotificationsService.Dtos;
using NotificationsService.Models;
using NotificationsService.Storage;

namespace NotificationsService;

// A CLASSIC Main method (not top-level statements) — deliberately, so this service's
// entry-point class is NotificationsService.Program, not the global ::Program that
// top-level statements would generate. Two independent services in one solution would
// otherwise both produce a global `Program` type, colliding when a cross-service test
// project references both assemblies (see tests/CrossService.Tests).
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSingleton<INotificationStore, InMemoryNotificationStore>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapPost("/api/notifications/order-placed", (INotificationStore store, OrderPlacedWebhookRequest request) =>
        {
            var notification = new Notification
            {
                OrderId = request.OrderId,
                Message = $"Order confirmed for {request.CustomerEmail}: {request.Total:C}"
            };

            store.AddIfNotExists(notification); // idempotent — a redelivered webhook call is a safe no-op
            return TypedResults.Accepted((string?)null);
        });

        app.MapGet("/api/notifications", (INotificationStore store) =>
            TypedResults.Ok(store.GetAll().Select(NotificationResponse.FromModel).ToList()));

        app.Run();
    }
}
