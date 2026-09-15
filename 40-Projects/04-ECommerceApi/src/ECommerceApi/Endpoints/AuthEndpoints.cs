using ECommerceApi.Auth;
using ECommerceApi.Data;
using ECommerceApi.Dtos;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Endpoints;

/// <summary>See module 22-Authentication and module 23-Security: passwords are never
/// stored in plaintext (PasswordHasher uses PBKDF2 under the hood), and login/register
/// return a short-lived JWT rather than establishing a server-side session.</summary>
public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        return group;
    }

    private static async Task<Results<Ok<AuthResponse>, Conflict<string>, ValidationProblem>> Register(
        AppDbContext db, TokenService tokenService, IPasswordHasher<User> hasher, RegisterRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Password"] = ["Password must be at least 8 characters."]
            });
        }

        if (await db.Users.AnyAsync(u => u.Email == request.Email, ct))
        {
            return TypedResults.Conflict("An account with this email already exists.");
        }

        var user = new User { Email = request.Email, PasswordHash = string.Empty };
        user.PasswordHash = hasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = tokenService.CreateAccessToken(user);
        return TypedResults.Ok(new AuthResponse(token, user.Email, user.Role.ToString()));
    }

    private static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> Login(
        AppDbContext db, TokenService tokenService, IPasswordHasher<User> hasher, LoginRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, ct);
        if (user is null)
        {
            return TypedResults.Unauthorized();
        }

        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return TypedResults.Unauthorized();
        }

        var token = tokenService.CreateAccessToken(user);
        return TypedResults.Ok(new AuthResponse(token, user.Email, user.Role.ToString()));
    }
}
