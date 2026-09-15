using System.Net;
using System.Net.Http.Json;
using ECommerceApi.Dtos;
using Xunit;

namespace ECommerceApi.Tests;

public class AuthApiTests : IClassFixture<ECommerceApiFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(ECommerceApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsAccessToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{Guid.NewGuid():N}@example.com", "Password123!"));

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.AccessToken));
        Assert.Equal("Customer", auth.Role);
    }

    [Fact]
    public async Task Register_WithShortPassword_ReturnsValidationProblem()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest($"{Guid.NewGuid():N}@example.com", "short"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password123!"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsAccessToken()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Password123!"));

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid():N}@example.com";
        await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Password123!"));

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPassword!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownEmail_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("nobody@example.com", "Password123!"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
