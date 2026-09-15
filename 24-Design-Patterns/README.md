# 24 — Design Patterns

## 🎯 Learning Objectives
- Recognize the problem shape each classic pattern solves, not just its structure.
- Implement the most commonly used Creational, Structural, and Behavioral patterns in idiomatic C#.
- Know when *not* to apply a pattern.

## 🤔 What is it?
Design patterns are named, reusable solutions to recurring design problems — a shared vocabulary ("just use a Strategy here") that lets engineers communicate design intent quickly.

## ❓ Why do we need it?
Without shared vocabulary, every team reinvents (and often gets subtly wrong) the same handful of solutions to problems like "vary behavior at runtime" or "decouple object creation from usage."

## This module is split into three parts

1. **[Creational Patterns](./01-Creational-Patterns.md)** — Singleton, Factory Method/Abstract Factory, Builder.
2. **[Structural Patterns](./02-Structural-Patterns.md)** — Adapter, Decorator, Facade, Proxy.
3. **[Behavioral Patterns](./03-Behavioral-Patterns.md)** — Strategy, Observer, Command, Chain of Responsibility, Template Method.

## ⚠️ Common Mistakes (across all patterns)
- Forcing a pattern where a simpler solution (a plain method, a record, an `if`) would do — pattern overuse is itself an anti-pattern ("pattern-itis").
- Implementing Singleton via a static field instead of DI, hurting testability.
- Confusing Strategy (behavior swapped via composition) with simple method overloading.

## ✅ Best Practices
- Reach for a pattern only when its *problem* genuinely matches your situation — name the problem first, then the pattern.
- Prefer DI-driven Strategy/Factory over hand-rolled static dispatch.
- Recognize that many .NET framework features (middleware, `event`, `DelegatingHandler`) are patterns you're already using.

## 🔄 Related Concepts
- [07 — Interfaces](../07-Interfaces)
- [09 — Delegates & Events](../09-Delegates-Events) (Observer)
- [17 — Middleware](../17-Middleware) (Chain of Responsibility)
- [18 — Dependency Injection](../18-Dependency-Injection) (Strategy, Factory)
- [25 — SOLID](../25-SOLID)

## 🎤 Top Interview Questions

**Junior:** "What problem does the Strategy pattern solve?"
→ Full detail in [03 — Behavioral Patterns](./03-Behavioral-Patterns.md).

**Mid-level:** "How is Decorator different from inheritance-based extension?"
→ Full detail in [02 — Structural Patterns](./02-Structural-Patterns.md).

**Senior:** "Where in ASP.NET Core itself do you see the Chain of Responsibility and Decorator patterns already in use?"
*Expected:* Middleware pipeline = Chain of Responsibility (each middleware decides to handle/pass along); `IHttpClientFactory`'s `DelegatingHandler` pipeline = Decorator (each handler wraps the next, adding behavior like retry/logging around the core HTTP call).

## 🧪 Capstone Exercise
**Real-world scenario:** A codebase has a 200-line `switch` statement selecting shipping cost calculation logic, duplicated in 3 places. Refactor it using an appropriate pattern — see [03 — Behavioral Patterns](./03-Behavioral-Patterns.md) (Strategy).

## 📌 Key Takeaways
- Patterns are named solutions to recurring problems — learn the *problem* each solves, not just the diagram.
- Many are already built into .NET/ASP.NET Core (middleware = Chain of Responsibility, `event` = Observer, `DelegatingHandler` = Decorator).
- Don't force a pattern where simpler code suffices.
