using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrdersService.Clients;

namespace OrdersService.Tests;

public class OrdersApiFactory : WebApplicationFactory<Program>
{
    public FakeNotificationClient FakeNotificationClient { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<INotificationClient>();
            services.AddSingleton<INotificationClient>(FakeNotificationClient);
        });
    }
}
