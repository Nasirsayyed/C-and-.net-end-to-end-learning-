using System.Net.Http.Headers;
using System.Net.Http.Json;
using ECommerceApi.Data;
using ECommerceApi.Dtos;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceApi.Tests;

public static class TestHelpers
{
    public static async Task<string> RegisterAndLoginAsync(HttpClient client, string? email = null)
    {
        email ??= $"{Guid.NewGuid():N}@example.com";
        var response = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password123!"));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    public static async Task<string> CreateAdminAndLoginAsync(ECommerceApiFactory factory, HttpClient client)
    {
        var email = $"{Guid.NewGuid():N}@admin.example.com";
        const string password = "AdminPass123!";

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            var admin = new User { Email = email, PasswordHash = string.Empty, Role = UserRole.Admin };
            admin.PasswordHash = hasher.HashPassword(admin, password);
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return auth!.AccessToken;
    }

    public static void UseToken(HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
