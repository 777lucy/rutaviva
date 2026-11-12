# RutaViva API

Preventive maintenance for one fleet. The domain decides when a routine is due, opens one service order, and applies the quote rules. The Angular app lives in `rutaviva-webui` and is not part of this project yet.

## Run

```bash
dotnet run --project src/RutaViva.Api
```

The Development profile opens Swagger UI at [http://localhost:5092/swagger](http://localhost:5092/swagger). The OpenAPI document is `GET /swagger/v1/swagger.json`.

`dotnet test RutaViva.sln` runs the domain, application, and API tests.

## Projects

Dependencies point inward.

| Project | References | Owns |
| --- | --- | --- |
| `RutaViva.Domain` | nothing | Aggregates, value objects, policies, domain events |
| `RutaViva.Application` | Domain | Commands, queries, handlers, ports |
| `RutaViva.Infrastructure` | Application and Domain | In-memory adapters, clock, seed |
| `RutaViva.Api` | Application and Infrastructure | Controllers, Swagger, the daily job |

The API does not reference Domain. A broken domain rule reaches HTTP as 409 through `DomainExceptionBehavior`.

## HTTP

| Caller | Routes |
| --- | --- |
| Client | `GET /api/fleet`, `GET /api/vehicles/{id}`, `GET /api/orders/{id}`, `POST /api/orders/{id}/approve`, `/reject`, `/cancel` |
| Telemetry | `POST /api/telemetry/readings` |
| Workshop | `POST /api/workshop/orders/{id}/quote`, `/start`, `/complete` |
| System | `GET /health`, `POST /api/system/daily-review`, `GET /api/system/events` |

A reading equal to or below the odometer returns 200 with `result: Ignored`. Validation is 400, a missing resource is 404, and an illegal transition is 409. Problem titles are in Spanish.

On startup the process seeds Transportes Andinos, Taller Central, and three vehicles. Restarting the process clears the in-memory state and seeds again.

The business rules and the screen contract are in `../docs`.
