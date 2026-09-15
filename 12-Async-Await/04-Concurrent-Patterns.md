← Back to [12 — Async/Await overview](./README.md)

# Concurrent Patterns

## Task.WhenAll / Task.WhenAny

```csharp
var ordersTask = GetOrdersAsync(ct);
var inventoryTask = GetInventoryAsync(ct);
await Task.WhenAll(ordersTask, inventoryTask); // run concurrently, wait for both

var winner = await Task.WhenAny(primaryProviderTask, backupProviderTask); // first to finish wins
```
`Task.WhenAll` runs independent async operations **concurrently** rather than sequentially awaiting each one — a very common, very impactful optimization.

## SemaphoreSlim
Used to limit concurrent access to a resource in async code (a `lock` statement cannot be used across an `await`):
```csharp
private static readonly SemaphoreSlim _semaphore = new(maxConcurrency: 3);

public async Task CallExternalApiAsync()
{
    await _semaphore.WaitAsync();
    try { await _httpClient.GetAsync("..."); }
    finally { _semaphore.Release(); }
}
```

## 💻 Basic Example

```csharp
public async Task<OrderSummary> BuildSummaryAsync(Guid orderId, CancellationToken ct)
{
    var orderTask = _orderRepository.GetByIdAsync(orderId, ct);
    var customerTask = _customerRepository.GetByOrderIdAsync(orderId, ct);

    await Task.WhenAll(orderTask, customerTask); // both run concurrently

    return new OrderSummary(orderTask.Result, customerTask.Result);
}
```

## 🔍 Code Walkthrough
- Starting both `orderTask` and `customerTask` **before** awaiting either kicks off both operations concurrently; awaiting them one at a time (`await orderTask; await customerTask;`) would serialize them unnecessarily, doubling the wall-clock latency for no reason.
- `Task.WhenAll` completes once both underlying tasks complete; accessing `.Result` afterward is safe here specifically because we already know both are complete (accessing `.Result` on an incomplete task would **block** the thread — see [03 — Compiler & State Machine Internals](./03-Compiler-State-Machine-Internals.md) for the deadlock risk this normally carries).

## 🏢 Real-World Example
A Web API endpoint fetching order details, customer details, and inventory status from three separate downstream services should issue all three calls concurrently with `Task.WhenAll`, cutting total latency to roughly the slowest single call instead of the sum of all three.

## 🚀 Production-Ready Example

```csharp
[HttpGet("{id:guid}")]
public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(Guid id, CancellationToken cancellationToken)
{
    var orderTask = _orderService.GetOrderAsync(id, cancellationToken);
    var customerTask = _customerService.GetCustomerForOrderAsync(id, cancellationToken);
    var inventoryTask = _inventoryService.GetStatusAsync(id, cancellationToken);

    try
    {
        await Task.WhenAll(orderTask, customerTask, inventoryTask);
    }
    catch when (cancellationToken.IsCancellationRequested)
    {
        return StatusCode(StatusCodes.Status499ClientClosedRequest);
    }

    return Ok(new OrderDetailsDto(orderTask.Result, customerTask.Result, inventoryTask.Result));
}
```

## ⚠️ Common Mistakes
- Sequentially `await`ing independent operations that could run concurrently via `Task.WhenAll`.

## ✅ Best Practices
- Start independent async operations before awaiting any of them, then `Task.WhenAll`.

## ⚡ Performance Considerations
- `async`/`await` dramatically improves **throughput and scalability** (requests handled per second under load) by freeing threads during I/O waits — it does **not** make a single request individually faster in latency terms, and can add small overhead (state machine allocation) for trivial, already-fast operations.

## 🎤 Interview Questions

**Mid-level:** "Why start multiple tasks before awaiting any of them, instead of awaiting each in turn?"
*Expected:* Awaiting immediately after starting each task serializes them — the total time becomes the sum of all operations; starting all of them first lets them run concurrently, so total time is roughly the slowest single operation.

## 🧪 Practice Exercises

**Easy**
1. Write two independent async calls sequentially, then rewrite using `Task.WhenAll`, and reason about the latency difference.

**Medium**
1. Use `SemaphoreSlim` to limit concurrent calls to an external API to 3 at a time.
2. Use `Task.WhenAny` to implement a simple timeout pattern around a slow operation.

**Hard**
1. Implement a small async producer/consumer pipeline using `Channel<T>` (BCL) and explain how it relates to what you've learned about async I/O.

**Real-world scenario:** A Web API endpoint that calls three independent downstream services sequentially with `await` takes 900ms total (300ms each). Rewrite it to reduce latency and explain the expected new total.

---
Previous: [← 03 — Compiler & State Machine Internals](./03-Compiler-State-Machine-Internals.md) · Back to [12 — Async/Await overview](./README.md)
