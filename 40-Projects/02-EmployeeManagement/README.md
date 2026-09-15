# Project 2 — Employee Management (OOP)

A buildable, tested implementation of the project described in [`40-Projects/README.md`](../README.md). Applies concepts from modules [06](../../06-OOP) (inheritance, runtime polymorphism), [07](../../07-Interfaces), [08](../../08-Advanced-CSharp) (generics), and [10](../../10-LINQ).

## What it does
Models an `Employee → Manager → Executive` hierarchy where `CalculateBonus()` is `virtual`/`override`n at each level — calling it through a base `Employee` reference still dispatches to the correct runtime type's implementation (see the `CalculateBonus_IsPolymorphic_WhenCalledThroughBaseReference` test). Stores employees in a generic `Repository<T>`, then reports on them with LINQ (`GroupBy`, `Average`, `OrderByDescending`).

## Structure
```
02-EmployeeManagement/
├── EmployeeManagement.sln
├── src/EmployeeManagement/
│   ├── Employee.cs      — base class, bonus policy = 5% of salary
│   ├── Manager.cs        — 10% + $100/report
│   ├── Executive.cs      — 20% + stock grant value
│   ├── Repository.cs     — generic in-memory repository, constrained to `Employee`
│   ├── Reports.cs        — LINQ-based reporting
│   └── Program.cs        — demo wiring it all together
└── tests/EmployeeManagement.Tests/
    ├── EmployeeTests.cs
    ├── RepositoryTests.cs
    └── ReportsTests.cs
```

## Run it
```bash
cd src/EmployeeManagement
dotnet run
```

## Test it
```bash
dotnet test
```
17 tests covering bonus calculation per subtype (including the polymorphism case specifically), constructor validation, repository operations, and LINQ report correctness.

## What to try next
- Refactor bonus calculation into a `Strategy` ([24 — Design Patterns](../../24-Design-Patterns)) injected via constructor instead of `virtual`/`override`, and compare the trade-offs.
- Add an `IBonusEligible` interface ([07 — Interfaces](../../07-Interfaces)) if you introduce a non-`Employee` type (e.g. a `Contractor`) that should also participate in bonus reporting without inheriting from `Employee`.
- Swap `Repository<T>`'s in-memory `List<T>` for an EF Core-backed implementation once you've worked through [21 — Entity Framework Core](../../21-Entity-Framework-Core).
