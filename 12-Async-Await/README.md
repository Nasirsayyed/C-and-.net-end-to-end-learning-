# 12 — Async / Await

## 🎯 Learning Objectives
- Explain what `async`/`await` actually compiles to and why it doesn't mean "runs on another thread."
- Distinguish CPU-bound from I/O-bound work and pick the right tool for each.
- Use `Task`, `Task<T>`, `ValueTask<T>`, `CancellationToken`, `ConfigureAwait`, `Task.WhenAll/WhenAny`, and `SemaphoreSlim` correctly.

## 🤔 What is it?
`async`/`await` is C# syntax for writing asynchronous code that *reads* like sequential, synchronous code, while the compiler transforms it into a state machine that can pause at each `await` and resume later — typically when an I/O operation completes — **without blocking the calling thread**.

## ❓ Why do we need it?
A thread blocked waiting on a database call, HTTP request, or disk read is a thread doing nothing but occupying a valuable, limited OS resource (thread pool threads are expensive to create and limited in number). Async I/O lets a server handle thousands of concurrent in-flight requests with a small thread pool, because threads are only occupied during actual CPU work, not while waiting on external systems.

## 🌍 Real-World Analogy
A synchronous, blocking call is like a chef who starts the oven, then **stands in front of it doing nothing** until the food is done, unable to help anyone else. An `async` call is a chef who starts the oven, walks away to help other customers, and comes back to check the food only when a timer goes off — the same chef (thread) serves many more customers this way. Notice this analogy does **not** involve a second chef — that's the key insight most learners get wrong.

## This module is split into four parts

1. **[Async Fundamentals](./01-Async-Fundamentals.md)** — why `async` doesn't mean multithreading, CPU-bound vs I/O-bound.
2. **[Task, ValueTask & CancellationToken](./02-Task-ValueTask-CancellationToken.md)** — the core vocabulary types.
3. **[Compiler & State Machine Internals](./03-Compiler-State-Machine-Internals.md)** — what `async`/`await` actually compiles to, and the classic deadlock.
4. **[Concurrent Patterns](./04-Concurrent-Patterns.md)** — `WhenAll`/`WhenAny`/`SemaphoreSlim` and a full production example.

## 🔄 Related Concepts
- [11 — Exception Handling](../11-Exception-Handling) (exceptions in `async void`)
- [30 — Background Services](../30-Background-Services)
- [33 — Resilience](../33-Resilience)

## 🎤 Top Interview Questions

**Junior:** "Does `async` mean the code runs on a separate thread?"
*Expected:* No — `await`ing I/O-bound work releases the current thread back to the pool while waiting; no dedicated second thread is spun up for the wait itself.
→ Full detail in [01 — Async Fundamentals](./01-Async-Fundamentals.md).

**Mid-level:** "Why does calling `.Result` on a `Task` sometimes deadlock?"
→ Full detail in [03 — Compiler & State Machine Internals](./03-Compiler-State-Machine-Internals.md).

**Senior:** "Walk me through exactly what the compiler generates for an `async` method."
→ Full detail in [03 — Compiler & State Machine Internals](./03-Compiler-State-Machine-Internals.md).

## 🧪 Capstone Exercise
**Real-world scenario:** A Web API endpoint that calls three independent downstream services sequentially with `await` takes 900ms total (300ms each). Rewrite it to reduce latency and explain the expected new total — see [04 — Concurrent Patterns](./04-Concurrent-Patterns.md).

## 📌 Key Takeaways
- `async`/`await` frees threads during I/O waits; it does not itself create parallelism for CPU-bound work.
- Never block on async code with `.Result`/`.Wait()` — it risks deadlocks and defeats the purpose.
- Start independent async operations concurrently and `Task.WhenAll` them instead of awaiting sequentially.
- Propagate `CancellationToken` through the entire async call chain.
