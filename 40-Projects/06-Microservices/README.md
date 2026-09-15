# Project 6 — Microservices Split (Orders + Notifications)

A buildable, tested implementation of the project described in [`40-Projects/README.md`](../README.md). Applies concepts from modules [31](../../31-Messaging) (idempotency), [32](../../32-Microservices) (service boundaries, HTTP communication), [33](../../33-Resilience) (retry policies, fault isolation), and [34](../../34-Testing) (cross-service integration testing).

## What it does
Two **independently runnable** ASP.NET Core services:
- **OrdersService** — accepts `POST /api/orders`, stores the order, and notifies NotificationsService over HTTP.
- **NotificationsService** — accepts a webhook (`POST /api/notifications/order-placed`) and records a notification, idempotently.

Each has its own process, its own storage, and its own solution-relative folder — communicating only through an HTTP contract, never a shared database or shared compiled types (see [32 — Microservices](../../32-Microservices) on why "database per service" and independent deployability matter).

## ⚠️ Honest scope note
A real deployment of this pattern uses a message broker (RabbitMQ, Azure Service Bus — see [31 — Messaging](../../31-Messaging)) so OrdersService doesn't need NotificationsService to be reachable *at that exact moment*. This project uses direct HTTP calls instead, because the sandbox this was built in has no Docker daemon available to run a real broker. The trade-off is documented, not hidden — see **Design notes** below for exactly what changes if you swap in a real broker.

## Structure
```
06-Microservices/
├── Microservices.sln
├── src/
│   ├── OrdersService/
│   │   ├── Clients/ (INotificationClient, NotificationClient — Polly retry policy)
│   │   ├── Storage/, Models/, Dtos/
│   │   └── Program.cs
│   └── NotificationsService/
│       ├── Storage/ (idempotent InMemoryNotificationStore — dedupes by OrderId)
│       ├── Models/, Dtos/
│       └── Program.cs
└── tests/
    ├── OrdersService.Tests/        — uses a FakeNotificationClient test double
    ├── NotificationsService.Tests/ — includes an idempotent-redelivery test
    └── CrossService.Tests/         — THE interesting one, see below
```

## The cross-service test
`CrossService.Tests/OrderPlacedFlowTests.cs` spins up **two real, separate** `WebApplicationFactory` instances (one per service) and wires OrdersService's `HttpClient` to route directly into NotificationsService's in-memory `TestServer` via `ConfigurePrimaryHttpMessageHandler`. No real ports, no Docker — but a genuinely real, separate DI container, routing table, and endpoint pipeline per service. This is the technique to reach for whenever you need to verify two independently deployable services actually agree on their wire contract, sitting between per-service integration tests and a full deployed end-to-end test on the [testing pyramid](../../34-Testing).

## Run it (two terminals)
```bash
# Terminal 1
cd src/NotificationsService && dotnet run --launch-profile http   # listens on :5051

# Terminal 2
cd src/OrdersService && dotnet run --launch-profile http           # listens on :5145, calls :5051
```
```bash
curl -X POST http://localhost:5145/api/orders -H "Content-Type: application/json" \
  -d '{"customerEmail":"you@example.com","total":42.50}'

curl http://localhost:5051/api/notifications   # should show the notification arrived
```

## Test it
```bash
dotnet test    # 7 tests across all three test projects
```

## Design notes — what changes with a real message broker
- **Fault isolation** already works correctly even over HTTP: `OrdersApiTests.Create_WhenNotificationsServiceFails_StillSucceeds` proves an order succeeds even if notifying fails — OrdersService never rolls back or blocks on NotificationsService's availability (see [32 — Microservices](../../32-Microservices) and [33 — Resilience](../../33-Resilience)).
- **What a real broker would add**: guaranteed eventual delivery even if NotificationsService is down for an *extended* period (right now, a sustained outage just means that specific order's notification is silently dropped after 3 retries) — this gap is exactly what the **transactional outbox pattern** (write the event to the same DB transaction as the order, have a separate process publish it) closes, described in [31 — Messaging](../../31-Messaging).
- **Idempotency matters either way**: `InMemoryNotificationStore` dedupes by `OrderId` regardless of transport, because at-least-once delivery (via retries here, or via a broker in production) is the default assumption, not an edge case.
- Swapping HTTP for a real broker changes `NotificationClient` (publish to a topic instead of POST) and `NotificationsService`'s entry point (a `BackgroundService` consumer instead of a webhook endpoint, per [30 — Background Services](../../30-Background-Services)) — the domain logic in both services is untouched.

## What to try next
- Implement the transactional outbox pattern for guaranteed delivery.
- Swap the HTTP call for a real message broker (RabbitMQ via Docker, once available) and compare the `NotificationClient`/`NotificationsService` diff.
- Add a third service (`ShippingService`) and turn the 2-step flow into an actual Saga per [32 — Microservices](../../32-Microservices).
