← Back to [24 — Design Patterns overview](./README.md)

# Behavioral Patterns

## Strategy
**Problem:** Select an algorithm/behavior at runtime, from a family of interchangeable options.
```csharp
public interface IShippingCostStrategy { decimal Calculate(Order order); }
public class StandardShipping : IShippingCostStrategy { public decimal Calculate(Order o) => 5.99m; }
public class ExpressShipping : IShippingCostStrategy { public decimal Calculate(Order o) => 15.99m; }

public class ShippingCalculator
{
    private readonly IShippingCostStrategy _strategy;
    public ShippingCalculator(IShippingCostStrategy strategy) => _strategy = strategy;
    public decimal GetCost(Order order) => _strategy.Calculate(order);
}
```
**When to use:** Multiple interchangeable algorithms selected at runtime — directly enabled by DI (see [18 — Dependency Injection](../18-Dependency-Injection)).

## Observer
**Problem:** Notify multiple dependents automatically when an object's state changes.
**.NET equivalent:** C# `event`s (see [09 — Delegates & Events](../09-Delegates-Events)) natively implement this pattern.

## Command
**Problem:** Encapsulate a request as an object, enabling queuing, logging, undo.
```csharp
public interface ICommand { Task ExecuteAsync(); }
public class ShipOrderCommand : ICommand
{
    private readonly Order _order;
    public ShipOrderCommand(Order order) => _order = order;
    public Task ExecuteAsync() => /* ... */ Task.CompletedTask;
}
```
**.NET equivalent:** MediatR's `IRequest`/`IRequestHandler` is essentially the Command pattern plus a mediator.

## Chain of Responsibility
**Problem:** Pass a request along a chain of handlers until one handles it.
**.NET equivalent:** The entire ASP.NET Core **middleware pipeline** (see [17 — Middleware](../17-Middleware)) is a live example of this pattern.

## Template Method
**Problem:** Define the skeleton of an algorithm in a base class, letting subclasses override specific steps.
```csharp
public abstract class ReportGenerator
{
    public async Task GenerateAsync() // the template — fixed skeleton
    {
        var data = await FetchDataAsync();
        var formatted = FormatData(data);
        await SaveAsync(formatted);
    }
    protected abstract Task<IEnumerable<object>> FetchDataAsync();
    protected virtual string FormatData(IEnumerable<object> data) => string.Join("\n", data);
    protected abstract Task SaveAsync(string content);
}
```

## 🎤 Interview Questions

**Junior:** "What problem does the Strategy pattern solve?"
*Expected:* Lets you select/swap an algorithm or behavior at runtime by depending on an abstraction rather than hardcoding one implementation.

**Senior:** "Where in ASP.NET Core itself do you see the Chain of Responsibility pattern already in use?"
*Expected:* The middleware pipeline — each middleware decides to handle the request, modify it, or pass it along to the next one, exactly matching the Chain of Responsibility structure.

## 🧪 Practice Exercises

**Easy**
1. Implement a Strategy for 3 discount calculation algorithms.

**Medium**
1. Implement a Chain of Responsibility for request validation (multiple validators, first failure short-circuits).
2. Refactor a large `switch` statement selecting behavior into a Strategy + DI-registered implementations.

**Hard**
1. Implement a Command pattern with undo support for a simple text editor's operations.

**Real-world scenario:** A codebase has a 200-line `switch` statement selecting shipping cost calculation logic, duplicated in 3 places. Refactor it using the Strategy pattern.

---
Previous: [← 02 — Structural Patterns](./02-Structural-Patterns.md) · Back to [24 — Design Patterns overview](./README.md)
