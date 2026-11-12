## Why

RutaViva clients still track preventive maintenance in spreadsheets. The fleet, the workshop, and telemetry need one API that decides when a routine is due, opens a single service order, and applies the quote and approval rules. The solution skeleton exists and every business route still returns 501, so the behavior has nowhere to live yet.

## What Changes

- Implement the domain model: vehicles, routines, kilometer and calendar-month progress, and the service-order state machine.
- Implement the use cases that register readings, run the daily review, open at most one order per vehicle and routine, and drive quote, approval, rejection, cancel, start, and completion.
- Replace the 501 stubs with the client, telemetry, workshop, and system endpoints described in the architecture, backed by in-memory adapters and seed data.
- Publish domain events to an in-memory log the simulation panel can read. Do not send real notifications.
- Keep the Angular app out of this change. The API contract is what that app will call later.

## Capabilities

### New Capabilities

- `routine-evaluation`: Combined routine status from kilometers and calendar months, including the 10% due-soon window, one notice per cycle, and restart after completion.
- `service-order-lifecycle`: Order states, the 1,500,000 COP approval rule, cancellation limits, and at most one open order per vehicle and routine.
- `fleet-maintenance-api`: HTTP contract for the client, telemetry, the workshop, and the daily review, including ignored readings and problem responses.

### Modified Capabilities

- None. There are no existing specs.

## Impact

- `src/RutaViva.Domain`, `src/RutaViva.Application`, `src/RutaViva.Infrastructure`, and `src/RutaViva.Api`, plus their test projects.
- New domain types and use cases. Endpoint signatures that already exist change from 501 to real responses.
- In-memory repositories, a per-vehicle lock, `TimeProvider`, and a hosted daily job plus `POST /api/system/daily-review`.
- No database, authentication, message broker, or frontend project.
