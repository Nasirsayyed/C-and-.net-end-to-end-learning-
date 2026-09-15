# Project 1 — Console Calculator

A buildable, tested implementation of the project described in [`40-Projects/README.md`](../README.md). Applies concepts from modules [01](../../01-CSharp-Fundamentals)–[05](../../05-Methods) and [11](../../11-Exception-Handling) (exception handling for invalid input).

## What it does
A REPL-style calculator supporting `+ - * /`, parentheses, unary minus, and decimal numbers, implemented as a hand-written recursive-descent parser (no external expression-eval library) so the grammar/precedence logic is visible and testable. Includes a `history` command backed by a `List<string>`.

## Structure
```
01-ConsoleCalculator/
├── ConsoleCalculator.sln
├── src/ConsoleCalculator/
│   ├── Calculator.cs   — the expression parser/evaluator
│   └── Program.cs      — the REPL loop
└── tests/ConsoleCalculator.Tests/
    └── CalculatorTests.cs
```

## Run it
```bash
cd src/ConsoleCalculator
dotnet run
```
```
> 2 + 3 * 4
14
> (2 + 3) * 4
20
> history
2 + 3 * 4 = 14
(2 + 3) * 4 = 20
> exit
```

## Test it
```bash
dotnet test
```
14 tests covering operator precedence, parentheses, unary minus, decimals, and error cases (division by zero, malformed expressions, empty input) — see [11 — Exception Handling](../../11-Exception-Handling) for why these are handled with typed exceptions rather than generic `catch (Exception)`.

## What to try next
- Add support for `^` (exponentiation) — requires adding a new precedence level to the grammar.
- Add variables (`x = 5`, then `x + 2`) — requires a symbol table.
- Refactor `Tokenize`/`Parse` using the concepts from [08 — Advanced C#](../../08-Advanced-CSharp) (`ReadOnlySpan<char>` instead of `List<string>` tokens) to avoid intermediate string allocations.
