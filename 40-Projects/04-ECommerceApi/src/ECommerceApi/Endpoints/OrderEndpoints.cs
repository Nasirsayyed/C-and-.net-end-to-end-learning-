using ECommerceApi.Auth;
using ECommerceApi.Data;
using ECommerceApi.Dtos;
using ECommerceApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApi.Endpoints;

public static class OrderEndpoints
{
    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)
    {
        group.RequireAuthorization(); // every endpoint in this group requires SOME authenticated user
        group.MapPost("/", Create);
        group.MapGet("/mine", GetMine);
        group.MapGet("/{id:guid}", GetById);
        return group;
    }

    private static async Task<Results<Created<OrderResponse>, ValidationProblem, Conflict<string>>> Create(
        AppDbContext db, HttpContext httpContext, CreateOrderRequest request, CancellationToken ct)
    {
        if (request.Lines.Count == 0)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Lines"] = ["An order must contain at least one line."]
            });
        }

        var productIds = request.Lines.Select(l => l.ProductId).ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, ct);

        var lines = new List<OrderLine>();
        foreach (var requestedLine in request.Lines)
        {
            if (!products.TryGetValue(requestedLine.ProductId, out var product))
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Lines"] = [$"Product '{requestedLine.ProductId}' does not exist."]
                });
            }
            if (requestedLine.Quantity <= 0)
            {
                return TypedResults.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["Lines"] = ["Quantity must be positive."]
                });
            }
            if (requestedLine.Quantity > product.StockQuantity)
            {
                return TypedResults.Conflict($"Insufficient stock for '{product.Name}': requested {requestedLine.Quantity}, only {product.StockQuantity} available.");
            }

            product.StockQuantity -= requestedLine.Quantity; // decrement — enforced HERE, in one place
            lines.Add(new OrderLine
            {
                ProductId = product.Id,
                Quantity = requestedLine.Quantity,
                UnitPriceAtPurchase = product.Price // snapshot the price at time of purchase
            });
        }

        var order = new Order(httpContext.User.GetUserId(), lines);
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        var response = OrderResponse.FromModel(order);
        return TypedResults.Created($"/api/orders/{order.Id}", response);
    }

    private static async Task<Ok<List<OrderResponse>>> GetMine(AppDbContext db, HttpContext httpContext, CancellationToken ct)
    {
        var userId = httpContext.User.GetUserId();
        var orders = await db.Orders.AsNoTracking()
            .Include(o => o.Lines)
            .Where(o => o.CustomerId == userId)
            .ToListAsync(ct);

        return TypedResults.Ok(orders.Select(OrderResponse.FromModel).ToList());
    }

    // Demonstrates object-level authorization (anti-IDOR — see module 23-Security):
    // being AUTHENTICATED is not enough; we also verify the caller owns this specific
    // resource (or is an Admin) before returning it.
    private static async Task<Results<Ok<OrderResponse>, NotFound, ForbidHttpResult>> GetById(
        AppDbContext db, HttpContext httpContext, Guid id, CancellationToken ct)
    {
        var order = await db.Orders.AsNoTracking().Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id, ct);
        if (order is null)
        {
            return TypedResults.NotFound();
        }

        var callerId = httpContext.User.GetUserId();
        var isOwner = order.CustomerId == callerId;
        var isAdmin = httpContext.User.IsInRole(UserRole.Admin);

        if (!isOwner && !isAdmin)
        {
            return TypedResults.Forbid();
        }

        return TypedResults.Ok(OrderResponse.FromModel(order));
    }
}
