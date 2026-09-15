← Back to [21 — EF Core overview](./README.md)

# Concurrency, Transactions & Filters

## Optimistic Concurrency

```csharp
public class Order
{
    [Timestamp]
    public byte[] RowVersion { get; set; } = default!; // maps to SQL Server ROWVERSION
}
```
```csharp
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    // another user modified this row since it was loaded — reload and let the user decide
}
```

## Transactions

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    _context.Inventory.Update(item);
    _context.Orders.Add(order);
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```
A single `SaveChangesAsync()` call is already atomic across all its tracked changes — an explicit transaction is needed mainly when you must combine multiple `SaveChangesAsync()` calls, or mix EF Core changes with raw SQL, into one atomic unit.

## Global query filters & value converters

```csharp
modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted); // soft-delete, applied to EVERY query automatically

modelBuilder.Entity<Order>()
    .Property(o => o.Status)
    .HasConversion<string>(); // store enum as string instead of int
```

## ⚠️ Common Mistakes
- Catching `DbUpdateConcurrencyException` and simply retrying `SaveChangesAsync()` without reloading the entity — this silently overwrites the conflicting change instead of surfacing it.
- Wrapping long-running or external-call logic inside an EF Core transaction, holding database locks far longer than necessary (see [20 — SQL Server](../20-SQL-Server)).

## ✅ Best Practices
- Use `[Timestamp]`/`RowVersion` on any entity multiple users might update concurrently.
- Keep explicit transactions as short as possible, and only when genuinely spanning multiple `SaveChangesAsync()` calls.
- Prefer a query filter for cross-cutting concerns like soft-delete over remembering to add `.Where(o => !o.IsDeleted)` to every query by hand.

## 🧪 Practice Exercises

**Easy**
1. Add a `[Timestamp]` `RowVersion` to an entity and generate the migration for it.

**Medium**
1. Implement optimistic concurrency with a `[Timestamp]` `RowVersion` and handle `DbUpdateConcurrencyException` by reloading and prompting for a decision.
2. Add a global query filter for soft-delete and verify deleted rows are excluded automatically from a normal query, then explicitly included with `IgnoreQueryFilters()`.

**Hard**
1. Reproduce a concurrency conflict with two simulated concurrent updates to the same row and handle it correctly (not just catch-and-ignore).

---
Previous: [← 03 — LINQ → SQL Translation](./03-LINQ-to-SQL-Translation.md) · Back to [21 — EF Core overview](./README.md)
