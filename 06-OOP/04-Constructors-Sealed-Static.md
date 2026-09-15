← Back to [06 — OOP overview](./README.md)

# Constructors, `sealed`, and `static` Classes

## 🤔 What is it?
- A **constructor** initializes a new instance, establishing its invariants before any other code can observe it.
- `sealed` on a class forbids further inheritance from it; on a method, forbids further overriding of it.
- A `static` class can never be instantiated at all — it's purely a container for static members.

## ❓ Why do we need them?
Constructors guarantee an object is never observed in a half-initialized, invalid state. `sealed` communicates and enforces a real design decision ("this type is not meant to be extended") rather than leaving it ambiguous. `static` classes express "this is a pure utility/namespace-like container" with no notion of instance identity at all.

## 💻 Constructors

```csharp
public class Order
{
    public Guid Id { get; }
    public DateTimeOffset PlacedAtUtc { get; }

    public Order(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("Order id cannot be empty.", nameof(id));
        Id = id;
        PlacedAtUtc = DateTimeOffset.UtcNow; // invariant established BEFORE the object is usable
    }
}
```
Validating in the constructor means there is no window in which an `Order` with an empty `Id` exists anywhere in the program — the invalid state is rejected before construction completes.

## 💻 `sealed`

```csharp
public sealed class CreditCardPayment : PaymentMethod
{
    // no further subclassing possible — the compiler enforces this
}
```
Sealing communicates: "this specific payment method implementation is complete and not meant to be specialized further." It also lets the JIT devirtualize calls to it (see [03 — Polymorphism](./03-Polymorphism.md#-performance-considerations)).

## 💻 `static` classes

```csharp
public static class MathUtils
{
    public static double CelsiusToFahrenheit(double celsius) => celsius * 9 / 5 + 32;
}

// var utils = new MathUtils(); // COMPILE ERROR — static classes cannot be instantiated
var f = MathUtils.CelsiusToFahrenheit(100);
```
A `static` class has no instance constructor, no instance members are allowed at all, and the compiler enforces that it can never be `new`'d up — appropriate for pure, stateless helper functions with no identity or lifecycle of their own.

## ⚠️ Common Mistakes
- Forgetting to validate constructor arguments, allowing invalid objects to exist.
- Leaving a class unsealed "just in case," when there's no actual plan or reason to subclass it — an unsealed class is an open-ended promise to every future maintainer that subclassing it is safe and supported.
- Using a `static` class as a dumping ground for unrelated helper methods instead of grouping genuinely related utilities.

## ✅ Best Practices
- Enforce every invariant you can in the constructor, not in a separate `Initialize()` method called after construction.
- Default new classes to `sealed` unless you have a concrete, current reason for them to be a base class — you can always unseal later; you can't easily "reseal" a class already extended by other code.
- Keep `static` classes small and focused on one clear category of stateless operations.

## 🎤 Interview Questions

**Junior:** "Why can't you create an instance of a `static` class?"
*Expected:* Static classes have no instance constructor and cannot hold instance state — the compiler enforces this because the class only exists to group static members, not to represent an "object" with identity.

**Mid-level:** "Why might you default new classes to `sealed`?"
*Expected:* Being unsealed is an open-ended commitment that any future subclass can override any virtual member and rely on the base class's exact current behavior; sealing by default avoids accidentally supporting a fragile inheritance relationship nobody explicitly designed for.

## 🧪 Practice Exercises

**Easy**
1. Write a class whose constructor rejects invalid arguments, and show the exception it throws.
2. Create a `sealed` class and explain, in your own words, why you sealed it.
3. Write a `static` utility class and confirm the compiler rejects `new`-ing it up.

**Medium**
1. Take an existing class in a project you've worked on and decide, with justification, whether it should be `sealed`.
2. Refactor a "junk drawer" static class with 15 unrelated helper methods into smaller, cohesive ones.

---
Previous: [← 03 — Polymorphism](./03-Polymorphism.md) · Back to [06 — OOP overview](./README.md)
