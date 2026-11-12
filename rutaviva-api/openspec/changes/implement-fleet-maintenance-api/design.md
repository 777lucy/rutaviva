## Context

The RutaViva skeleton is a .NET 8 solution with Domain, Application, Infrastructure, and Api projects. Endpoint routes exist and return 501. Behavior is specified in `docs/domain-model.md`, structure in `docs/architecture.md`, and the screen contract in `docs/ux-proposal.md`.

This change fills that skeleton. It does not introduce a new architecture.

## Goals / Non-Goals

**Goals:**

- Encode the domain rules so a test can fail before the production code exists.
- Expose one HTTP API the future client screen and the simulation panel can call.
- Keep a fresh process demoable through seed data, an on-demand daily review, and an event log.

**Non-Goals:**

- The Angular app, authentication, a real database, a message broker, payments, and sending notifications.
- Replacing a cancelled order, re-quoting, or choosing a workshop.
- Any behavior after cancellation beyond leaving the routine overdue with no open order.

## Decisions

### 1. Keep the four-project dependency rule

Domain references nothing. Application references Domain. Infrastructure implements Application ports. Api sends commands and queries and references Infrastructure only to register it.

Alternative: implement the rules inside the controllers. Rejected because the same evaluation step must serve both a reading and the daily review, and the tests must run without HTTP.

### 2. Test by tier

| Tier | Project | Work |
| --- | --- | --- |
| Strict | `RutaViva.Domain.Tests` | Value objects, `RoutineProgressPolicy`, `Vehicle` readings, routine transitions, `ServiceOrder` transitions. RED before GREEN. |
| Contract | `RutaViva.Application.Tests` | Use cases against the in-memory adapters: one open order, ignored readings, completion restart, reject leaves the routine overdue. RED before GREEN. |
| Smoke | `RutaViva.Api.Tests` | `WebApplicationFactory` for health, one accepted reading, one 409, one ignored reading. Labeled Smoke. Not line-by-line TDD. |

Alternative: only API tests. Rejected because a 200 response would not lock the 10% boundary or the calendar-month examples.

### 3. One evaluation step for both triggers

The `RegisterOdometerReadingCommand` and `RunDailyReviewCommand` handlers call the same `DomainEventDispatcher` after the aggregate raises events. That dispatcher publishes each domain event as a MediatR notification. `OpenOrderWhenRoutineBecameOverdue` opens an order when `OpenOrderPolicy` allows it. `DomainEventFlusher` then writes every event, including `RoutineBecameDueSoon`, to the log.

A kilometer reading calls `Vehicle.RegisterOdometerReading`, which updates distance status only. The daily review calls `Vehicle.RunDailyReview`, which updates time status only. Combined status is the worse applicable dimension.

### 4. Writes for one vehicle share one lock

`IVehicleLock` is acquired for every use case that writes a vehicle or one of its orders. The check-then-create of an open order happens inside that lock.

Alternative: rely on a single-threaded demo. Rejected because two readings can be in flight in ASP.NET Core, and the one-open-order rule would be accidental.

### 5. Events are recorded after save, and state changes are explicit

Aggregates collect events. The use case saves, then appends them to `IEventPublisher`. Opening an order is a direct call in the evaluation step, not a hidden handler.

The log is what `GET /api/system/events` returns. Titles stored for the client are Spanish. Codes and logs stay English.

### 6. Time stays outside the domain

Use cases pass `DateOnly` and `DateTimeOffset` in. "Today" is computed from `TimeProvider` in `America/Bogota`. `POST /api/system/daily-review` accepts a date so a reviewer can run the 6-month example without waiting. The hosted job calls the same use case once a day with that clock.

### 7. HTTP matches the architecture table

| Situation | HTTP |
| --- | --- |
| Invalid body, such as negative kilometers or a completion without a date | 400 |
| Missing vehicle or order, or a vehicle the current client does not own | 404 |
| Illegal order transition | 409 |
| Reading lower than or equal to the odometer | 200 with `result: Ignored` |

`ICurrentClient` returns the seeded client. Client queries filter on it.

### 8. CQRS through MediatR, exposed by controllers

Every write is a command and every read is a query, each with one handler under `Commands/<Name>/` or `Queries/<Name>/`. MediatR 12.4.1 (Apache 2.0) dispatches them. `DomainExceptionBehavior` maps a `DomainException` to `ConflictException` in the pipeline, so handlers call the domain directly and the API never references Domain. Controllers grouped by actor only build a request from the route and body and call `ISender.Send`. Domain events are MediatR notifications. Application `INotificationHandler`s run after save, inside the vehicle lock, through `DomainEventDispatcher`. `DomainEventFlusher` writes the log after those handlers return.

Alternative: use-case classes injected into Minimal API endpoints. Rejected because each class had to wrap domain errors itself and reads were not separated from writes.

### 9. Seed the three demo vehicles on startup

Infrastructure seeds Transportes Andinos, Taller Central, and the three vehicles from `docs/ux-proposal.md`, so every demo flow is reachable after a restart. Restarting the process clears the memory.

## Risks / Trade-offs

- [In-memory state disappears on restart] → Accept it. The challenge allows memory, and seed data restores the starting point.
- [A missed daily run could skip the due-soon window] → The review computes status from the baseline and the given date, so the next run still marks the routine overdue and opens the order. It does not emit a late due-soon notice. That matches the domain decision.
- [Calendar-month length changes by cycle] → `RoutineProgressPolicy` tests cover 1 January plus 6 months (181 days) and 31 January plus 1 month (28 February).
- [Two aggregates change in `CompleteService`] → Both saves run under the vehicle lock. If the second save throws, the in-memory adapters must leave the order uncompleted. The adapter updates each aggregate only after the domain method succeeds, and the use case saves the order only after the vehicle restart succeeds.

## Migration Plan

There is no production data. Deploy is a new process. Rollback is reverting the commit and restarting. Existing 501 routes become real responses, which is the intended break for this greenfield API.

## Open Questions

None. The domain, architecture, and UX documents already settle the rules this change implements.
