← Back to [06 — OOP overview](./README.md)

# Polymorphism

## 🤔 What is it?
Polymorphism ("many forms") means the same call can produce different behavior depending on the actual type involved. C# has two distinct kinds:

| | Compile-time (overloading) | Runtime (overriding) |
|---|---|---|
| Resolved by | Static argument types, at compile time | Actual object type, at runtime via vtable |
| Mechanism | Multiple methods, same name, different signatures | `virtual` base method + `override` in derived |
| Example | `Add(int,int)` vs `Add(double,double)` | `vehicle.Drive()` calling `Car.Drive()` or `ElectricCar.Drive()` depending on the real object |

## 💻 Example — runtime polymorphism

```csharp
Vehicle v = new ElectricCar { Make = "Tesla", Model = "3", Doors = 4, BatteryCapacityKwh = 75 };
v.Drive(); // "Tesla 3 silently drives using 75 kWh." — runtime polymorphism
```
(Using the `Vehicle`/`Car`/`ElectricCar` hierarchy from [02 — Inheritance](./02-Inheritance.md).)

## 🔍 Code Walkthrough
- The **compile-time (declared) type** of `v` is `Vehicle`, but its **runtime type** is `ElectricCar`.
- Because `Drive` is `virtual` in `Vehicle` and `override`n down the chain, the CLR looks up the **actual runtime type's** implementation, calling `ElectricCar.Drive()` even though `v` is statically typed as `Vehicle`.

## ⚙️ How It Works Internally
Every type with at least one `virtual`/`override` method gets a **virtual method table (vtable)** — an array of method pointers built once when the type is loaded. A non-virtual call is resolved directly at compile time (a fixed address); a virtual call instead does one indirection: look up the object's actual type, find that type's vtable, jump to the slot for that method. This indirection is the (tiny, usually negligible) performance cost of polymorphism, and it's exactly how `override` differs mechanically from method hiding via `new`, which does **not** use the vtable and instead resolves by the *static* type.

## ❌ The `new` vs `override` trap

```csharp
public class Base { public void Greet() => Console.WriteLine("Base"); }
public class Derived : Base { public new void Greet() => Console.WriteLine("Derived"); }

Base b = new Derived();
b.Greet(); // prints "Base" — NOT "Derived", because `new` hides, it doesn't override
```
Using `new` to "override" a non-virtual method hides the base member for that specific type — calling through a base-typed reference invokes the **base** implementation, not the derived one, which is the opposite of what most developers expect and is one of the most common OOP interview traps.

## 🏢 Real-World Example
A payment processing system defines `abstract class PaymentMethod { public abstract Task<PaymentResult> ChargeAsync(decimal amount); }` with `CreditCardPayment`, `PayPalPayment`, `BankTransferPayment` overriding it — the order service calls `paymentMethod.ChargeAsync(total)` without ever knowing which concrete type it's holding, and new payment providers can be added without touching the order service at all (Open/Closed Principle — see [25 — SOLID](../25-SOLID)).

## 🚀 Production-Ready Example

```csharp
public abstract class PaymentMethod
{
    public abstract Task<PaymentResult> ChargeAsync(decimal amount, CancellationToken ct);
}

public sealed class CreditCardPayment : PaymentMethod
{
    private readonly ICardGateway _gateway;
    public CreditCardPayment(ICardGateway gateway) => _gateway = gateway;

    public override async Task<PaymentResult> ChargeAsync(decimal amount, CancellationToken ct)
        => await _gateway.ChargeAsync(amount, ct);
}
```
`sealed` on `CreditCardPayment` communicates intent — see [04 — Constructors, sealed, static](./04-Constructors-Sealed-Static.md) for why.

## ⚡ Performance Considerations
Virtual calls are a single pointer indirection — negligible for almost all business logic, but `sealed` classes/methods let the JIT devirtualize and even inline calls in hot paths. Inheritance depth doesn't slow down virtual dispatch further (it's always one vtable lookup regardless of how many levels deep the actual type is).

## 🎤 Interview Questions

**Mid-level:** "What's the difference between overriding a method and hiding it with `new`?"
*Expected:* `override` participates in virtual dispatch — the runtime type decides which implementation runs, even through a base-typed reference. `new` hides the base member entirely for that type; which implementation runs depends on the *static* type of the reference used to call it.
*Trap:* Assuming `new` and `override` behave the same when called through a base reference.

**Senior:** "Why does a `sealed` method potentially perform better than a `virtual` one?"
*Expected:* The JIT can prove no further override is possible, so it can devirtualize the call (resolve it directly, like a non-virtual call) or even inline it — an optimization that isn't safe for a type that could still be further overridden.

## 🧪 Practice Exercises

**Easy**
1. Reproduce the `new` vs `override` trap above yourself and explain the output.
2. Model `Shape → Circle/Rectangle` with an abstract `Area()` and compute total area of a mixed list polymorphically.

**Medium**
1. Explain, with a diagram, what happens in memory/vtables when `Drive()` is called on an `ElectricCar` through a `Vehicle` reference.
2. Implement a plugin-style payment system (like the production example) with 3 payment methods and a factory selecting one at runtime.

**Hard**
1. Benchmark (conceptually or with BenchmarkDotNet) calling a `sealed` vs non-`sealed` virtual method in a tight loop and explain any measured difference.

**Real-world scenario:** A junior developer "overrides" a base class method using `new` instead of `virtual`/`override` and can't understand why polymorphic behavior isn't working in a list of mixed subclasses. Diagnose and fix it.

---
Previous: [← 02 — Inheritance](./02-Inheritance.md) · Next: [04 — Constructors, sealed, static →](./04-Constructors-Sealed-Static.md)
