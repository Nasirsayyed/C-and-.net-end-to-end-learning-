← Back to [21 — EF Core overview](./README.md)

# Tracking & Loading Strategies

## Tracking vs No-Tracking

```csharp
var order = await _context.Orders.FirstAsync(o => o.Id == id);         // TRACKED — EF watches for changes
order.Status = "Shipped";
await _context.SaveChangesAsync();                                      // detects the change, issues UPDATE

var readOnlyOrders = await _context.Orders.AsNoTracking().ToListAsync(); // NOT tracked — faster, less memory, can't be saved back
```
Use `AsNoTracking()` for pure read scenarios (GET endpoints, reports) — it skips the overhead of EF's change-tracking snapshot for every entity returned.

## Loading related data

```csharp
// Eager loading — one query (or a JOIN), related data included upfront
var order = await _context.Orders
    .Include(o => o.Customer)
    .ThenInclude(c => c.Address)
    .FirstAsync(o => o.Id == id);

// Explicit loading — load related data on demand, separately
await _context.Entry(order).Reference(o => o.Customer).LoadAsync();

// Lazy loading — related data fetched automatically the first time a navigation property is accessed
// (requires virtual navigation properties + proxies package; causes the N+1 problem if not careful)
```

## The N+1 problem
```csharp
var orders = await _context.Orders.ToListAsync();     // 1 query
foreach (var order in orders)
{
    Console.WriteLine(order.Customer.Name);            // N additional queries with lazy loading!
}
```
Fixed by eager-loading with `.Include(o => o.Customer)` upfront — turns N+1 queries into 1. This is one of the most common, most severe real-world EF Core performance bugs — always watch for it in code review.

## 🚀 Production-Ready Example — projection instead of full-entity loading

```csharp
public async Task<OrderDetailsDto?> GetOrderDetailsAsync(Guid id, CancellationToken ct)
{
    return await _context.Orders
        .AsNoTracking()
        .Where(o => o.Id == id)
        .Select(o => new OrderDetailsDto(
            o.Id,
            o.Customer.Name,
            o.Lines.Select(l => new OrderLineDto(l.Product.Name, l.Quantity, l.UnitPrice)).ToList(),
            o.Total))
        .FirstOrDefaultAsync(ct);
    // Projecting directly with Select() often outperforms Include() —
    // EF Core generates SQL that fetches ONLY the columns the DTO needs.
}
```

## ⚠️ Common Mistakes
- Leaving lazy loading enabled without discipline, causing silent N+1 query storms.
- Using tracked queries for read-only endpoints, wasting memory/CPU on unnecessary change-tracking.
- Calling `.ToList()` before `.Where()`/`.Select()`, forcing full table loads into memory (see [10 — LINQ](../10-LINQ)).

```csharp
// ❌ What NOT to do
var orders = _context.Orders.ToList(); // loads EVERYTHING
foreach (var o in orders.Where(o => o.Total > 1000)) { } // filters in memory, too late
```

## ✅ Best Practices
- Default to `AsNoTracking()` for reads; only track when you intend to call `SaveChanges()`.
- Prefer `Select()` projections to DTOs over `Include()` when you don't need the full entity graph.
- Avoid lazy loading in server applications; prefer explicit eager loading so query shape is visible in the code.

## ⚡ Performance Considerations
- `AsNoTracking()` measurably reduces memory and CPU for read-heavy endpoints at scale.
- Projection (`Select` to a DTO) can generate SQL that only fetches needed columns, reducing I/O versus loading full entities via `Include`.

## 🎤 Interview Questions

**Junior:** "What is the N+1 problem and how do you fix it?"
*Expected:* Loading a list of entities (1 query) then lazily accessing a navigation property per item (N more queries); fixed by eager-loading the related data upfront with `.Include()`.

**Mid-level:** "When would you use `AsNoTracking()` and when would you avoid it?"
*Expected:* Use it for read-only queries (GETs, reports) to skip change-tracking overhead; avoid it when you intend to modify the entities and call `SaveChanges()`, since untracked entities won't have their changes detected.

## 🧪 Practice Exercises

**Easy**
1. Query with and without `AsNoTracking()` and compare (conceptually) the tracked entries count.
2. Use `.Include()` to eager-load a related entity and inspect the generated SQL (via logging).

**Medium**
1. Reproduce the N+1 problem with lazy loading enabled, then fix it with eager loading.

**Hard**
1. Compare generated SQL and performance (conceptually) between `.Include()` and a `.Select()` projection for the same data shape.
2. Implement a repository pattern over EF Core and discuss whether it adds real value given `DbSet<T>` already implements a similar abstraction (see [24 — Design Patterns](../24-Design-Patterns)).

**Real-world scenario:** An API endpoint listing orders with customer names is timing out under load. Logging reveals hundreds of queries per request. Diagnose and fix.

---
Previous: [← 01 — DbContext, DbSet & Migrations](./01-DbContext-DbSet-Migrations.md) · Next: [03 — LINQ → SQL Translation →](./03-LINQ-to-SQL-Translation.md)
