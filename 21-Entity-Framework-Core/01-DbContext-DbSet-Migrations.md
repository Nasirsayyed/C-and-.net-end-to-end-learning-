← Back to [21 — EF Core overview](./README.md)

# DbContext, DbSet & Migrations

## DbContext & DbSet

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId);
    }
}
```
`DbContext` represents a unit of work + a session with the database; `DbSet<T>` represents a queryable/updatable table. See [18 — Dependency Injection](../18-Dependency-Injection) for why `DbContext` is registered `Scoped`, never `Singleton`.

## Migrations

```bash
dotnet ef migrations add AddOrderStatus
dotnet ef database update
```
Migrations are auto-generated (and hand-editable) C# classes describing incremental schema changes, letting you version-control your database schema alongside your code and apply it consistently across environments.

## ⚠️ Common Mistakes
- Forgetting migrations in source control, causing schema drift between environments.
- Editing an already-applied migration instead of creating a new one, causing mismatches between what's recorded as "applied" and what the file actually contains.

## ✅ Best Practices
- Keep migrations small, reviewed, and committed alongside the code change that needs them.
- Apply migrations via an explicit, auditable release step in production — not automatically on every app startup (see the caveat in the [WMS capstone project](../40-Projects/WarehouseManagementSystem/README.md), which does auto-apply for local development convenience only).

## 🧪 Practice Exercises

**Easy**
1. Define `Order`/`Customer` entities with a one-to-many relationship and create a migration.
2. Add a new column to an entity, generate a migration, and inspect the generated `Up`/`Down` methods.

**Medium**
1. Roll back a migration (`dotnet ef database update <PreviousMigrationName>`) and verify the schema change is reverted.

---
Next: [02 — Tracking & Loading Strategies →](./02-Tracking-and-Loading-Strategies.md)
