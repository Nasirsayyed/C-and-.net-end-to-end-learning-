← Back to [24 — Design Patterns overview](./README.md)

# Structural Patterns

## Adapter
**Problem:** Make an existing class's interface compatible with what client code expects, without modifying the original class.
```csharp
public interface IModernLogger { void Log(string message); }
public class LegacyLoggerAdapter : IModernLogger
{
    private readonly LegacyLogger _legacy;
    public LegacyLoggerAdapter(LegacyLogger legacy) => _legacy = legacy;
    public void Log(string message) => _legacy.WriteToLogFile(message); // translates the call
}
```
**When to use:** Integrating a third-party/legacy API into a codebase expecting a different interface.

## Decorator
**Problem:** Add behavior to an object dynamically without modifying its class or affecting other instances.
```csharp
public interface INotifier { Task SendAsync(string message); }
public class EmailNotifier : INotifier { public Task SendAsync(string m) => /*...*/ Task.CompletedTask; }
public class LoggingNotifierDecorator : INotifier
{
    private readonly INotifier _inner;
    public LoggingNotifierDecorator(INotifier inner) => _inner = inner;
    public async Task SendAsync(string message)
    {
        Console.WriteLine($"Sending: {message}");
        await _inner.SendAsync(message);
    }
}
```
**.NET equivalent:** `IHttpClientFactory`'s `DelegatingHandler` chain is a real-world decorator chain.
**When to use:** Layering cross-cutting behavior (logging, caching, retry) around a core implementation without subclassing.

## Facade
**Problem:** Provide a simple, unified interface over a complex subsystem.
```csharp
public class CheckoutFacade
{
    public async Task<OrderResult> CheckoutAsync(Cart cart)
    {
        await _inventory.ReserveAsync(cart);
        var payment = await _payments.ChargeAsync(cart.Total);
        await _shipping.ScheduleAsync(cart);
        return new OrderResult(payment.Success);
    }
}
```
**When to use:** Simplifying a complex set of collaborating subsystems for common client use cases.

## Proxy
**Problem:** Control access to an object (lazy loading, caching, access control) without changing its interface.
**.NET equivalent:** EF Core's lazy-loading proxies, `Lazy<T>`.

## 🎤 Interview Questions

**Mid-level:** "How is Decorator different from inheritance-based extension?"
*Expected:* Decorator composes behavior at runtime around an existing instance (wrapping the same interface), letting you mix and match behaviors freely without an explosion of subclasses; inheritance fixes the extension at compile time per subclass.

**Senior:** "Where in ASP.NET Core itself do you see the Decorator pattern already in use?"
*Expected:* `IHttpClientFactory`'s `DelegatingHandler` pipeline — each handler wraps the next, adding behavior like retry/logging around the core HTTP call, exactly matching the Decorator structure.

## 🧪 Practice Exercises

**Easy**
1. Implement a Decorator adding logging around an existing service interface.
2. Implement an Adapter wrapping a third-party library's incompatible interface.

**Medium**
1. Implement a Facade over 3 collaborating subsystems (e.g. Inventory, Payments, Shipping) for a checkout use case.

**Hard**
1. Chain 3 Decorators (logging, caching, retry) around one core service and trace the call order.

---
Previous: [← 01 — Creational Patterns](./01-Creational-Patterns.md) · Next: [03 — Behavioral Patterns →](./03-Behavioral-Patterns.md)
