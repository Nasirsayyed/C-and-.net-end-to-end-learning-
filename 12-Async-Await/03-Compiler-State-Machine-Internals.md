← Back to [12 — Async/Await overview](./README.md)

# Compiler & State Machine Internals

## ⚙️ How It Works Internally

The compiler transforms an `async` method into a **compiler-generated state machine** (a struct or class implementing `IAsyncStateMachine`):

```
Compiler
   ↓
Generates a state machine class/struct
   ↓
Each `await` becomes a state transition point
   ↓
The state machine registers a continuation with the awaited Task's awaiter
   ↓
Method returns a Task immediately to its caller (does NOT block)
   ↓
When the awaited operation completes, the continuation resumes the state machine
   ↓
Execution continues from right after the `await`, on whatever thread the continuation is scheduled on
```

Critically: **`await` does not create a new thread.** It registers a callback and returns control to the caller. The "magic" is entirely in *who* invokes that callback later — typically the I/O completion mechanism handing it back to a thread pool thread.

## The classic deadlock

```csharp
// DEADLOCK RISK (classic ASP.NET / UI app pattern — avoid entirely)
public void Button_Click(object sender, EventArgs e)
{
    var result = GetDataAsync().Result; // blocks the UI thread waiting for a continuation
                                          // that needs... the UI thread's synchronization context to resume on
}
```
This deadlocks specifically in environments with a capturing `SynchronizationContext` (WinForms/WPF UI thread, classic ASP.NET's request context) because the continuation is scheduled to resume on that same context, but the context's one thread is stuck blocking on `.Result`. **This is why `async` should be "async all the way down"** — never block on async code with `.Result`/`.Wait()`.

## ⚠️ Common Mistakes
- Blocking on async code with `.Result` or `.Wait()` — risks deadlocks and defeats the entire purpose of async.
- `async void` methods (except event handlers) — exceptions thrown inside them **cannot be caught by the caller** and crash the process instead; always use `async Task`.

```csharp
public async void ProcessOrder(Order order) // async void — avoid outside event handlers
{
    await _repository.SaveAsync(order); // if this throws, the exception is unhandleable by the caller
}
```

## ✅ Best Practices
- Return `Task`/`Task<T>` from async methods, never `async void` (except UI event handlers).
- Never call `.Result`/`.Wait()` on a `Task` from synchronous code — go async all the way up the call stack instead.
- Suffix async methods with `Async` by convention.

## 🎤 Interview Questions

**Mid-level:** "Why does calling `.Result` on a `Task` sometimes deadlock?"
*Expected:* In environments with a capturing synchronization context (UI apps, classic ASP.NET), the awaited task's continuation is scheduled to resume on that same context/thread; blocking that thread with `.Result` while it's also needed to run the continuation creates a deadlock. ASP.NET Core has no such context by default, reducing (but not eliminating as a bad practice) this specific risk.

**Senior:** "Walk me through exactly what the compiler generates for an `async` method, and why that design avoids blocking threads."
*Expected:* A state machine implementing `IAsyncStateMachine` with a `MoveNext()` method; each `await` is a state boundary where the method registers a continuation with the awaiter and returns control (and a `Task` handle) to the caller immediately, rather than blocking; when the awaited operation signals completion (often via I/O completion ports), the scheduler invokes the continuation, which resumes `MoveNext()` from the saved state.

## 🧪 Practice Exercises

**Easy**
1. Demonstrate why `async void` swallows exceptions with a small reproducible example.

**Medium**
1. Reproduce the classic UI-thread `.Result` deadlock in a small console/WinForms repro (or explain precisely why it would occur).

**Hard**
1. Use a decompiler (e.g. SharpLab) to view the generated state machine for a simple `async` method and identify the `MoveNext()` method and its state field.

---
Previous: [← 02 — Task, ValueTask & CancellationToken](./02-Task-ValueTask-CancellationToken.md) · Next: [04 — Concurrent Patterns →](./04-Concurrent-Patterns.md)
