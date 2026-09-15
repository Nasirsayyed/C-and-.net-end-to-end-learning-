← Back to [12 — Async/Await overview](./README.md)

# Task, ValueTask & CancellationToken

## Task, Task&lt;T&gt;, ValueTask&lt;T&gt;

- **`Task`** — represents an in-progress or completed operation with no return value.
- **`Task<T>`** — same, but with a result of type `T`.
- **`ValueTask<T>`** — a struct-based alternative to `Task<T>` that avoids a heap allocation when the result is **already available synchronously** (e.g. served from a cache) — a targeted optimization for hot paths with frequent synchronous completion; don't reach for it by default, since misuse (awaiting it twice, storing it) has real pitfalls `Task<T>` doesn't.

```csharp
public ValueTask<Product> GetProductAsync(Guid id)
{
    if (_cache.TryGetValue(id, out var cached))
    {
        return new ValueTask<Product>(cached); // no allocation — the common, fast path
    }
    return new ValueTask<Product>(FetchFromDatabaseAsync(id)); // falls back to a real Task
}
```

## CancellationToken

```csharp
public async Task<List<Order>> GetOrdersAsync(CancellationToken cancellationToken)
{
    return await _dbContext.Orders.ToListAsync(cancellationToken);
}
```
Every async method that can take meaningful time should accept and propagate a `CancellationToken` — ASP.NET Core automatically supplies one tied to the client's request lifetime, so a client disconnecting cancels the whole downstream chain of work, freeing server resources immediately instead of finishing pointless work.

## ConfigureAwait(false)

```csharp
var data = await SomeLibraryCallAsync().ConfigureAwait(false);
```
Tells the awaiter not to try to resume on the original synchronization context (relevant for UI apps with a UI thread context, and legacy ASP.NET (Framework) with `HttpContext`-bound context). **In modern ASP.NET Core there is no such synchronization context**, so `ConfigureAwait(false)` is largely unnecessary in application code there — but it remains a best practice in general-purpose **library** code that might be consumed by UI applications, to avoid forcing an unnecessary context-capturing continuation.

## ⚠️ Common Mistakes
- Forgetting to pass/propagate `CancellationToken` through the whole async call chain — a token accepted but never passed downstream does nothing.
- Awaiting a `ValueTask<T>` more than once, or storing it for later use — unlike `Task<T>`, this is explicitly unsupported and can produce undefined behavior.
- Adding `ValueTask<T>` everywhere reflexively "for performance" without measuring whether the method actually completes synchronously often enough to matter.

## ✅ Best Practices
- Propagate `CancellationToken` end to end, from the API entry point down to every database/HTTP call.
- Use `ConfigureAwait(false)` in library code that isn't ASP.NET Core-specific.
- Default to `Task`/`Task<T>`; adopt `ValueTask<T>` only in profiler-justified hot paths.

## 🎤 Interview Questions

**Mid-level:** "When would you use `ValueTask<T>` instead of `Task<T>`?"
*Expected:* In a hot path where the operation frequently completes synchronously (e.g. a cache hit) and the allocation of a `Task<T>` per call is measurably significant — not as a default replacement, since `ValueTask<T>` has stricter usage rules (can't be awaited twice, shouldn't be stored).

## 🧪 Practice Exercises

**Easy**
1. Add a `CancellationToken` parameter to an existing async method and thread it through to an EF Core call.
2. Write a method returning `ValueTask<T>` that short-circuits on a cache hit.

**Medium**
1. Explain tiered/ahead-of-time considerations aside, benchmark `ValueTask<T>` vs `Task<T>` for a method that completes synchronously 90% of the time.

---
Previous: [← 01 — Async Fundamentals](./01-Async-Fundamentals.md) · Next: [03 — Compiler & State Machine Internals →](./03-Compiler-State-Machine-Internals.md)
