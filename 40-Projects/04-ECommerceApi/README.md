# Project 4 — E-Commerce API (EF Core + JWT Auth)

A buildable, tested implementation of the project described in [`40-Projects/README.md`](../README.md). Applies concepts from modules [21](../../21-Entity-Framework-Core) (EF Core), [22](../../22-Authentication) (JWT, roles), and [23](../../23-Security) (password hashing, anti-IDOR object-level authorization).

## What it does
Products, Customers (as `User`s), and Orders backed by EF Core (SQLite), with:
- **Registration/login** issuing short-lived JWTs, passwords hashed with `PasswordHasher<T>` (PBKDF2 under the hood) — never stored in plaintext.
- **Role-based authorization** — only `Admin` users can create products (`Customer` gets a `403 Forbidden`, anonymous gets `401 Unauthorized`).
- **Object-level authorization (anti-IDOR)** — `GET /api/orders/{id}` checks the caller actually *owns* the order (or is an Admin), not just that they're authenticated. This is the exact vulnerability class described in [23 — Security](../../23-Security#authentication--authorization-flaws) (Authentication & Authorization Flaws section), demonstrated as a passing test, not just prose.
- **Stock enforcement** — placing an order that exceeds available stock returns `409 Conflict` and leaves stock unchanged (no partial application).
- **Price snapshotting** — `OrderLine.UnitPriceAtPurchase` is captured at order time, so a later price change never retroactively alters historical orders.

## Structure
```
04-ECommerceApi/
├── ECommerceApi.sln
├── src/ECommerceApi/
│   ├── Models/ (User, Product, Order, OrderLine)
│   ├── Data/AppDbContext.cs        — including the encapsulated-collection EF Core mapping for Order.Lines
│   ├── Auth/ (TokenService, ClaimsPrincipalExtensions)
│   ├── Dtos/ (Auth, Product, Order)
│   ├── Endpoints/ (Auth, Product, Order)
│   └── Program.cs
└── tests/ECommerceApi.Tests/
    ├── ECommerceApiFactory.cs      — in-memory SQLite, same technique as the WMS capstone
    ├── AuthApiTests.cs
    ├── ProductApiTests.cs          — role-based authorization
    └── OrderApiTests.cs            — stock enforcement + the anti-IDOR test
```

## Run it
```bash
cd src/ECommerceApi
dotnet run
```
Then open `/swagger`. Try: register → login → copy the `accessToken` → `Authorize` in Swagger UI with `Bearer <token>` → attempt `POST /api/products` (fails, 403, you're a Customer) → create an Admin the same way the tests do, or promote a user directly in the SQLite file for manual testing.

## Test it
```bash
dotnet test
```
17 integration tests via `WebApplicationFactory`, including:
- `Create_AsCustomer_ReturnsForbidden` / `Create_AsAdmin_Succeeds` — role-based authorization.
- `GetById_AsDifferentCustomer_ReturnsForbidden` — the anti-IDOR test: customer B cannot read customer A's order just by knowing its id.
- `Create_ExceedingStock_ReturnsConflict_AndDoesNotDecrementStock` — verifies the failed operation didn't partially apply.

## Design notes
- The JWT signing key in `appsettings.json` is a **placeholder for local development only** — see [22 — Authentication](../../22-Authentication) and [39 — Azure](../../39-Azure) for why production secrets belong in a vault (Key Vault, environment variables), never in a committed config file.
- `Order.Lines` is `IReadOnlyList<OrderLine>` backed by a private `List<OrderLine>` field — EF Core is configured to read/write through that field directly (`SetPropertyAccessMode(PropertyAccessMode.Field)`), so the aggregate stays encapsulated even from its own ORM (see [27 — DDD](../../27-DDD)).

## What to try next
- Add refresh token rotation (see [22 — Authentication](../../22-Authentication)) instead of only a 15-minute access token.
- Add rate limiting to `/api/auth/login` to slow down credential-stuffing attempts (see [23 — Security](../../23-Security) and [33 — Resilience](../../33-Resilience)).
- Add pagination to `GET /api/products` per [19 — Web API](../../19-Web-API).
