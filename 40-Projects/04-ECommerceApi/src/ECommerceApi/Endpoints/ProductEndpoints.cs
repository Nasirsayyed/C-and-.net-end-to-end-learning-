using ECommerceApi.Data;
using ECommerceApi.Dtos;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Endpoints;

public static class ProductEndpoints
{
    public static RouteGroupBuilder MapProductEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapPost("/", Create).RequireAuthorization(policy => policy.RequireRole(nameof(UserRole.Admin)));
        return group;
    }

    private static async Task<Ok<List<ProductResponse>>> GetAll(AppDbContext db, CancellationToken ct)
    {
        var products = await db.Products.AsNoTracking()
            .Select(p => ProductResponse.FromModel(p))
            .ToListAsync(ct);
        return TypedResults.Ok(products);
    }

    private static async Task<Results<Ok<ProductResponse>, NotFound>> GetById(AppDbContext db, Guid id, CancellationToken ct)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return product is null ? TypedResults.NotFound() : TypedResults.Ok(ProductResponse.FromModel(product));
    }

    // Only Admins can reach this endpoint at all (see the RequireRole policy above) —
    // demonstrates role-based authorization from module 22-Authentication.
    private static async Task<Created<ProductResponse>> Create(AppDbContext db, CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product { Name = request.Name, Price = request.Price, StockQuantity = request.StockQuantity };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        var response = ProductResponse.FromModel(product);
        return TypedResults.Created($"/api/products/{product.Id}", response);
    }
}
