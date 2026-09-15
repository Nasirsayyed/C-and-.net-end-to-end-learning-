# Project 3 — Todo Web API (CRUD)

A buildable, tested implementation of the project described in [`40-Projects/README.md`](../README.md). Applies concepts from modules [16](../../16-ASPNet-Core) (Minimal APIs), [18](../../18-Dependency-Injection), [19](../../19-Web-API) (DTOs, status codes, `ProblemDetails`), and [34](../../34-Testing) (`WebApplicationFactory` integration tests).

## What it does
A full Todo CRUD API using ASP.NET Core Minimal APIs, backed by a thread-safe in-memory repository (no database yet — see Project 4 for EF Core). DTOs at the boundary (never exposing `TodoItem` directly), correct HTTP status codes (`201 Created` with `Location` header, `204 No Content`, `404 Not Found`, `400` via `ValidationProblem`), and Swagger enabled in Development.

## Structure
```
03-TodoApi/
├── TodoApi.sln
├── src/TodoApi/
│   ├── Models/TodoItem.cs
│   ├── Dtos/TodoDtos.cs              — CreateTodoRequest, UpdateTodoRequest, TodoResponse
│   ├── Repositories/
│   │   ├── ITodoRepository.cs
│   │   └── InMemoryTodoRepository.cs — ConcurrentDictionary-backed, registered as Singleton
│   ├── Endpoints/TodoEndpoints.cs    — MapGet/Post/Put/Delete using TypedResults
│   └── Program.cs
└── tests/TodoApi.Tests/
    └── TodoApiTests.cs               — WebApplicationFactory-based integration tests
```

## Run it
```bash
cd src/TodoApi
dotnet run
```
Then open `https://localhost:<port>/swagger` to try it interactively, or:
```bash
curl -X POST https://localhost:<port>/api/todos -H "Content-Type: application/json" -d '{"title":"Learn Minimal APIs"}'
curl https://localhost:<port>/api/todos
```

## Test it
```bash
dotnet test
```
9 integration tests exercising the **real** ASP.NET Core pipeline in-memory (routing, DI, JSON serialization) via `WebApplicationFactory<Program>` — covering the full CRUD flow, the `isComplete` filter, validation failures, and 404 cases.

## Design notes
- `ITodoRepository` is registered `Singleton` deliberately — it *is* the storage (an in-memory dictionary), not a wrapper around a per-request resource like `DbContext`. See [18 — Dependency Injection](../../18-Dependency-Injection) for why this would be wrong for an EF Core-backed repository.
- `public partial class Program;` at the bottom of `Program.cs` is required so the test project's `WebApplicationFactory<Program>` can reference the top-level-statement-generated `Program` class.
- Minimal APIs don't run `[Required]`/`[StringLength]` DataAnnotations automatically (that's an MVC/`[ApiController]` feature) — validation here is deliberately manual and explicit in `TodoEndpoints`, returning a proper `ValidationProblem`.

## What to try next
- Swap `InMemoryTodoRepository` for an EF Core-backed one (see Project 4 and [21 — EF Core](../../21-Entity-Framework-Core)) — `ITodoRepository`'s consumers (`TodoEndpoints`) shouldn't need to change at all.
- Add pagination (`?page=&pageSize=`) to `GET /api/todos` per [19 — Web API](../../19-Web-API).
- Add a `[Authorize]`-protected endpoint and JWT auth per [22 — Authentication](../../22-Authentication).
