using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using NotificationsDtos = NotificationsService.Dtos;
using OrdersDtos = OrdersService.Dtos;

namespace CrossService.Tests;

/// <summary>
/// A genuine cross-service integration test: TWO real ASP.NET Core apps (each with its
/// own DI container, routing, and endpoints) running in-memory via their own
/// WebApplicationFactory, wired together over an HTTP message handler instead of real
/// network sockets. This is the technique to reach for when you need to verify that two
/// independently deployable services actually agree on their wire contract — see module
/// 32-Microservices (service boundaries) and module 34-Testing (the testing pyramid: this
/// sits above per-service integration tests, below a full deployed end-to-end test).
/// </summary>
public class OrderPlacedFlowTests : IClassFixture<WebApplicationFactory<NotificationsService.Program>>
{
    private readonly WebApplicationFactory<NotificationsService.Program> _notificationsFactory;

    public OrderPlacedFlowTests(WebApplicationFactory<NotificationsService.Program> notificationsFactory)
    {
        _notificationsFactory = notificationsFactory;
    }

    [Fact]
    public async Task PlacingAnOrder_ActuallyNotifiesTheRealNotificationsService()
    {
        // Force NotificationsService's TestServer to start, then grab a handler that
        // routes HTTP calls directly into its in-memory pipeline — no real port needed.
        _ = _notificationsFactory.Server;
        var notificationsHandler = _notificationsFactory.Server.CreateHandler();

        await using var ordersFactory = new WebApplicationFactory<OrdersService.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<OrdersService.Clients.INotificationClient>();

                    services.AddHttpClient<OrdersService.Clients.INotificationClient, OrdersService.Clients.NotificationClient>(client =>
                        {
                            client.BaseAddress = new Uri("http://notifications-service.local"); // arbitrary — routed via the handler below, never a real DNS lookup
                        })
                        .ConfigurePrimaryHttpMessageHandler(() => notificationsHandler);
                });
            });

        var ordersClient = ordersFactory.CreateClient();
        var notificationsClient = _notificationsFactory.CreateClient();

        var response = await ordersClient.PostAsJsonAsync("/api/orders",
            new OrdersDtos.CreateOrderRequest("cross-service@example.com", 123.45m));
        response.EnsureSuccessStatusCode();
        var order = await response.Content.ReadFromJsonAsync<OrdersDtos.OrderResponse>();

        // Verify the notification actually arrived at the SEPARATE NotificationsService
        // instance — proving the two services' wire contract genuinely matches, not just
        // that OrdersService called *something*.
        var notifications = await notificationsClient.GetFromJsonAsync<List<NotificationsDtos.NotificationResponse>>("/api/notifications");
        Assert.Contains(notifications!, n => n.OrderId == order!.Id);
    }
}
