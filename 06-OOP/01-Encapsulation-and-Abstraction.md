← Back to [06 — OOP overview](./README.md)

# Encapsulation & Abstraction

## 🤔 What is it?
**Encapsulation** bundles data with the methods that operate on it, and restricts direct access to that data from outside — you interact through a controlled public surface, not raw fields. **Abstraction** is showing *what* something does while hiding *how* it does it.

They're related but distinct: encapsulation is a *mechanism* (private fields, public methods/properties); abstraction is a *design goal* it helps achieve (a simple, stable interface hiding complex, changeable internals).

## ❓ Why do we need it?
Without encapsulation, any code anywhere can mutate an object's fields directly, making it impossible to guarantee the object is ever in a valid state — a `BankAccount.Balance` field with a public setter can be set to a negative number by any careless line of code, anywhere in a large codebase. Without abstraction, callers become coupled to implementation details that should be free to change.

## 🌍 Real-World Analogy
A car's accelerator pedal is an abstraction: pressing it makes the car go faster, and you don't need to know whether it's a combustion engine or an electric motor underneath (abstraction), and you *cannot* reach in and directly rewire the fuel injectors from the driver's seat (encapsulation) — the pedal is the only sanctioned interface.

## 💻 Example

```csharp
public class BankAccount
{
    private decimal _balance; // encapsulated — no external code can touch this directly

    public decimal Balance => _balance; // read-only exposure

    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        _balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount > _balance) throw new InvalidOperationException("Insufficient funds.");
        _balance -= amount;
    }
}
```

## 🔍 Code Walkthrough
- `_balance` is `private` — the *only* way to change it is through `Deposit`/`Withdraw`, which is exactly where the invariant "balance can never go negative" is enforced.
- `Balance` exposes a read-only view (an expression-bodied property, see [05 — Methods](../05-Methods)) — callers can observe state but never bypass the rules to set it directly.
- This is abstraction in action too: a caller calls `account.Withdraw(50)` without knowing (or needing to know) whether balances are stored in a field, a database round-trip, or computed from a ledger of transactions.

## ⚠️ Common Mistakes
- Public mutable fields/auto-properties with public setters on anything that has business rules to protect (`public decimal Balance { get; set; }` defeats the entire purpose).
- "Encapsulation" mistaken for simply making fields `private` while still exposing a public setter that allows any value — that's not protecting an invariant, just moving where the bug can happen.
- Abstraction leaking: a method name/signature that reveals internal implementation details (e.g. `GetBalanceFromSqlDatabase()` instead of `GetBalance()`), coupling callers to something that should be free to change.

## ✅ Best Practices
- Default to `private` fields; expose behavior through methods, and state only through properties that can't put the object in an invalid state.
- Ask "what invariant does this class need to protect?" before writing any setter.
- Name public members by *what* they do, never *how* they do it.

## 🎤 Interview Questions

**Junior:** "What's the difference between encapsulation and abstraction?"
*Expected:* Encapsulation is the mechanism (restricting direct access to internal state); abstraction is the goal (exposing a simple interface that hides complexity) — encapsulation is one of the main tools used to achieve abstraction.

**Mid-level:** "Why is `public int Count { get; set; }` often a design smell?"
*Expected:* If `Count` should always reflect the number of items in an internal collection, an open setter lets any caller desynchronize it from reality — it should usually be a computed, read-only property (`public int Count => _items.Count;`) instead.

## 🧪 Practice Exercises

**Easy**
1. Convert a class with public mutable fields into one with private fields and validated methods.
2. Write a `Temperature` class that only allows values within a physically valid range.

**Medium**
1. Design a `Stack<T>`-like class where `Push`/`Pop` are the only ways to mutate an internal array — no direct external access to the array.
2. Identify an abstraction leak in a class you've written before (a method name/parameter revealing an implementation detail) and fix it.

**Hard**
1. Design a `TemperatureSensor` class that abstracts over three different underlying hardware APIs, exposing one consistent `ReadCelsius()` method regardless of which hardware is in use.

---
Next: [02 — Inheritance →](./02-Inheritance.md)
