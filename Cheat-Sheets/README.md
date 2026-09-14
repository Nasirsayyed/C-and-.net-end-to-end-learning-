# Cheat Sheets — Quick Comparison Tables

One-page reference tables for concepts commonly confused or asked about in interviews. Each links back to the full module for depth.

## class vs struct
| | class | struct |
|---|---|---|
| Category | Reference type | Value type |
| Storage | Heap (referenced by pointer) | Inline (stack when local, or embedded in containing object) |
| Default equality | Reference | Value (field-by-field) |
| Inheritance | Full support | Interfaces only |
| Copy behavior | Copies the reference | Copies the whole value |
→ [03 — Value vs Reference Types](../03-Value-vs-Reference-Types)

## interface vs abstract class
| | interface | abstract class |
|---|---|---|
| Multiple implementation | Yes | No (single inheritance) |
| State (fields) | No | Yes |
| Constructor | No | Yes |
| Default implementation | Yes (C# 8+, for versioning) | Yes, fully |
→ [07 — Interfaces](../07-Interfaces)

## IEnumerable vs IQueryable
| | IEnumerable\<T\> | IQueryable\<T\> |
|---|---|---|
| Execution | In-process (CLR) | Translated to the data source's query language |
| Filtering location | After loading into memory | At the source (e.g. SQL `WHERE`) |
| Typical source | `List<T>`, arrays | `DbSet<T>` (EF Core) |
→ [10 — LINQ](../10-LINQ)

## Task vs ValueTask
| | Task\<T\> | ValueTask\<T\> |
|---|---|---|
| Type | Reference type (heap allocation) | Struct (no allocation if completed synchronously) |
| Can await twice | Yes | No — undefined behavior |
| Use case | Default choice | Hot paths that frequently complete synchronously (e.g. cache hits) |
→ [12 — Async/Await](../12-Async-Await)

## ref vs out vs in
| | ref | out | in |
|---|---|---|---|
| Direction | In + out | Out only | In only |
| Caller must initialize first | Yes | No | Yes |
| Callee must assign | No (recommended) | Yes, mandatory | Cannot assign |
→ [05 — Methods](../05-Methods)

## const vs readonly
| | const | readonly |
|---|---|---|
| Value known at | Compile time | Can be runtime (constructor) |
| Implicitly static | Yes | No |
| Baked into IL at call sites | Yes | No |
→ [02 — Data Types](../02-Data-Types)

## string vs StringBuilder
| | string | StringBuilder |
|---|---|---|
| Mutability | Immutable — every "modification" creates a new string | Mutable buffer, modified in place |
| Use case | Few concatenations | Many concatenations in a loop (avoids O(n²) allocation churn) |
→ [08 — Advanced C#](../08-Advanced-CSharp)

## Thread vs Task
| | Thread | Task |
|---|---|---|
| Level | Raw OS/CLR thread | Higher-level unit of work, may or may not use a thread |
| Pooling | Manual (expensive to create) | Backed by the thread pool by default |
| Async I/O | No native async model | Designed for `async`/`await`, frees threads during I/O waits |
→ [12 — Async/Await](../12-Async-Await)

## var vs dynamic
| | var | dynamic |
|---|---|---|
| Type resolution | Compile time (inferred, still static) | Runtime |
| IntelliSense/compile errors | Full support | None until runtime |
→ [02 — Data Types](../02-Data-Types)

## record vs class
| | record | class |
|---|---|---|
| Default equality | Value-based | Reference-based |
| Non-destructive copy | `with` expression | Manual |
| Typical use | DTOs, value objects | Entities with identity/mutable lifecycle |
→ [14 — Modern C#](../14-Modern-CSharp)

## AddSingleton vs AddScoped vs AddTransient
| | Singleton | Scoped | Transient |
|---|---|---|---|
| Instances created | One, app lifetime | One per request/scope | New every resolution |
| Typical use | Caches, config | `DbContext`, per-request state | Lightweight stateless services |
→ [18 — Dependency Injection](../18-Dependency-Injection)

## REST vs gRPC
| | REST | gRPC |
|---|---|---|
| Payload | JSON (text, human-readable) | Protobuf (binary, compact) |
| Contract | Loosely typed (OpenAPI optional) | Strongly typed `.proto` contract |
| Browser support | Native | Requires grpc-web proxy |
| Best for | Public APIs, broad compatibility | Internal service-to-service, high throughput/low latency |
→ [19 — Web API](../19-Web-API), [32 — Microservices](../32-Microservices)

## Monolith vs Microservices
| | Monolith | Microservices |
|---|---|---|
| Deployment | One unit | Many independent units |
| Data | Usually one database | Database per service |
| Team scaling | Harder past 1-2 teams | Designed for many independent teams |
| Operational complexity | Low | High (network, distributed consistency) |
→ [28 — Architecture](../28-Architecture), [32 — Microservices](../32-Microservices)
