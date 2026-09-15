# 21 — Entity Framework Core

## 🎯 Learning Objectives
- Explain how a LINQ query becomes SQL.
- Use tracking vs no-tracking queries, `Include`/`ThenInclude`, and migrations correctly.
- Handle concurrency and transactions with EF Core.

## 🤔 What is it?
EF Core is Microsoft's Object-Relational Mapper (ORM) for .NET: it maps C# classes (entities) to database tables, translates LINQ queries into SQL, and manages change tracking so you can mutate objects in memory and persist the changes with `SaveChanges()`.

## This module is split into four parts

1. **[DbContext, DbSet & Migrations](./01-DbContext-DbSet-Migrations.md)** — the basic building blocks and schema versioning.
2. **[Tracking & Loading Strategies](./02-Tracking-and-Loading-Strategies.md)** — `AsNoTracking`, eager/lazy/explicit loading, and the N+1 problem.
3. **[LINQ → SQL Translation](./03-LINQ-to-SQL-Translation.md)** — what actually happens between your `Where()` call and rows coming back.
4. **[Concurrency, Transactions & Filters](./04-Concurrency-Transactions-Filters.md)** — optimistic concurrency, transactions, global query filters, value converters.

Working example used throughout: an e-commerce style `Order`/`Customer`/`OrderLine`/`Product` schema.

## 🔄 Related Concepts
- [10 — LINQ](../10-LINQ)
- [20 — SQL Server](../20-SQL-Server)
- [18 — Dependency Injection](../18-Dependency-Injection) (`DbContext` lifetime)

## 🎤 Top Interview Questions

**Junior:** "What is the N+1 problem and how do you fix it?"
→ Full detail in [02 — Tracking & Loading Strategies](./02-Tracking-and-Loading-Strategies.md).

**Mid-level:** "When would you use `AsNoTracking()` and when would you avoid it?"
→ Full detail in [02 — Tracking & Loading Strategies](./02-Tracking-and-Loading-Strategies.md).

**Senior:** "Explain, mechanically, what happens between calling `.Where()` on a `DbSet<T>` and rows arriving from SQL Server."
→ Full detail in [03 — LINQ → SQL Translation](./03-LINQ-to-SQL-Translation.md).

## 🧪 Capstone Exercise
**Real-world scenario:** An API endpoint listing orders with customer names is timing out under load. Logging reveals hundreds of queries per request. Diagnose and fix — see [02 — Tracking & Loading Strategies](./02-Tracking-and-Loading-Strategies.md).

## 📌 Key Takeaways
- LINQ on `DbSet<T>` builds an expression tree, translated to SQL only when materialized.
- Use `AsNoTracking()` for reads; eager-load with `Include`/`ThenInclude` (or project directly with `Select`) to avoid the N+1 problem.
- Migrations version your schema; commit them alongside the code changes that require them.
