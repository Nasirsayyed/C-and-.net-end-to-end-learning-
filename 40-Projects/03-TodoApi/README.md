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

## 🚀 Deploying to Render (free tier)

This project ships with a [`Dockerfile`](./Dockerfile), [`.dockerignore`](./.dockerignore), and a [`render.yaml`](../../render.yaml) at the repo root — Render's free web service tier can build and run it directly from GitHub.

**Option A — Blueprint (one click, uses `render.yaml`):**
1. Push this repo to your own GitHub account.
2. In the [Render dashboard](https://dashboard.render.com), click **New → Blueprint**, and select the repo. Render reads `render.yaml` and provisions the service automatically.
3. Wait for the build to finish, then open the assigned `https://<name>.onrender.com/swagger` URL.

**Option B — Manual web service (no render.yaml needed):**
1. **New → Web Service** → connect the repo.
2. **Runtime:** Docker.
3. **Root Directory:** `40-Projects/03-TodoApi`.
4. **Dockerfile Path:** `Dockerfile` (relative to the root directory above).
5. **Plan:** Free.
6. **Environment Variable:** `ASPNETCORE_ENVIRONMENT=Development` (keeps Swagger UI enabled on the public URL — see `Program.cs`).
7. Deploy.

### Why this Dockerfile works with zero code changes
Render assigns a random port at container start via the `PORT` environment variable and expects the app to listen on it. Kestrel doesn't read `PORT` on its own, so rather than hardcoding a deployment concern into the pedagogical `Program.cs`, the Dockerfile's `ENTRYPOINT` translates it at container start:
```dockerfile
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet TodoApi.dll"]
```
This is the same principle as [15 — .NET Fundamentals](../../15-DotNet-Fundamentals) (environment-driven configuration) and [37 — Docker](../../37-Docker) (infrastructure concerns belong in the Dockerfile, not the app) — the app code stays identical to what you'd run locally with `dotnet run`.

### Free-tier caveats (be aware, not surprised)
- **Cold starts:** Render's free web services spin down after ~15 minutes of no traffic. The next request pays a ~30–60 second cold start while it restarts.
- **No persistent disk on the free plan:** irrelevant for this project (in-memory storage is already reset on every restart by design), but matters if you later swap in EF Core/SQLite (Project 4's pattern) — the database file would reset on every redeploy/restart too. Render's free tier doesn't include a persistent disk; that requires a paid instance or an external managed database.
- **No custom domain/TLS on the free plan** beyond the provided `onrender.com` subdomain (which does get HTTPS automatically).

## What to try next
- Swap `InMemoryTodoRepository` for an EF Core-backed one (see Project 4 and [21 — EF Core](../../21-Entity-Framework-Core)) — `ITodoRepository`'s consumers (`TodoEndpoints`) shouldn't need to change at all.
- Add pagination (`?page=&pageSize=`) to `GET /api/todos` per [19 — Web API](../../19-Web-API).
- Add a `[Authorize]`-protected endpoint and JWT auth per [22 — Authentication](../../22-Authentication).
