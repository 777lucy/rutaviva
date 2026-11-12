## Context

The API in `rutaviva-api` already implements fleet, vehicle, order, telemetry, workshop, and system routes. `docs/ux-proposal.md` is the screen contract. Architecture decision A11 fixes the stack: Angular 18, standalone components, signals, and plain CSS. This repo does not use Doggie UI.

`vehicle-schedulestatuschange-webui` is a reference for organization only. It keeps a feature folder per screen, a template, a stylesheet, and a spec beside each component, typed models, an environment API URL, and HTTP inside injectable services. It also uses NgModules, NgRx, single-spa, and Angular Material. Those do not belong in RutaViva. Its confirmation dialog is the pattern to keep: a small shared component that receives title, message, and button labels, and returns whether the user confirmed.

The API listens on `http://localhost:5092` in the `http` launch profile and already allows `http://localhost:4200`.

## Goals / Non-Goals

**Goals:**

- Render the fleet, vehicle, and order screens and the fixed simulation panel from the UX mockup.
- Call only the existing endpoints. Reload from the API after every successful command.
- Put view rules in pure functions so they stay testable and out of templates.
- Keep each TypeScript file at or under 150 lines, excluding specs. A page composes small components. Each API service covers one API area.

**Non-Goals:**

- Changing the API, adding endpoints, or generating a client from OpenAPI.
- Login, Doggie UI, Angular Material, NgRx, single-spa, or a global store library.
- Editing vehicles, routines, or intervals. Creating a replacement order after cancel or reject.
- Docker and the root README. Those stay with the API change's run story until a later change.

## Decisions

### D1. Standalone Angular 18 app next to the API

Create the app inside `rutaviva-webui/` and leave `openspec/` in place. The CLI refuses a non-empty directory, so scaffold into a temporary directory and move the Angular files in, without touching `openspec/`.

Use SCSS, routing, and standalone bootstrap. Do not enable SSR. Development API base URL: `http://localhost:5092`, read from `environment.ts`. Services concatenate paths. They do not hardcode the host.

Alternative considered: an Angular proxy. CORS is already open for port 4200, so a proxy adds a moving part with no gain.

### D2. Group files by who uses them

A file lives in the feature that is its only caller. `shared/` holds only what two or more features import. `core/` holds the API and the reload signal. Features do not import each other. A feature state service loads data and exposes a view model. It does not perform HTTP itself.

```text
src/app/
  app.config.ts                     providers only
  app.routes.ts                     fleet, vehicles/:id, orders/:id
  app.component.*                   shell: header, outlet, simulation panel
  client-session.ts                 seeded name "Transportes Andinos"
  core/
    api/
      fleet-api.service.ts
      vehicle-api.service.ts
      order-api.service.ts
      telemetry-api.service.ts
      workshop-api.service.ts
      system-api.service.ts
      models/                       one file per response group
    problem-title.ts
    refresh.service.ts
  shared/
    status-label.ts                 every screen maps a status
    components/status-chip/
    components/confirm-dialog/
  features/
    fleet/                          page, card, banner, due-soon-banner.ts, state
    vehicle/                        page, routine row, state
    order/                          page, cop-amount.ts, order-actions.ts, cancellation-label.ts, state
    simulation/                     reading, workshop, and event sections, bogota-today.ts, workshop-actions.ts, state
```

One public class per file. The page component binds a view model. It does not format money, map statuses, or decide which buttons exist. Those functions sit in the feature folder next to the page.

The reference app's `features/<name>/*.component.ts|html|scss|spec.ts` split is the file pattern. Its `store/` folder is not copied. State is a signal on a small service in the same feature: load, error title, and the last DTO. `ChangeDetectionStrategy.OnPush` on every component.

Enforce the size limit with `angular-eslint` rule `max-lines` at 150 for `*.ts`, ignoring `*.spec.ts`.

### D3. Shell owns the simulation panel

The panel is a child of the shell, not of each page, so it stays on the right across navigation. Layout is a two-column grid: the routed view, then a 20rem panel. Styles live in `styles.scss` as tokens: Inter, an 8px spacing scale, and three chip colors (neutral, warning, danger). The mockup has no Penpot file in this workspace, so these tokens are the source until a visual file is added.

A `refresh.service.ts` in `core/` exposes a signal that increments after every successful command. Fleet, vehicle, order, and the event list reload when it changes and when the route id changes. Features do not call each other.

### D4. View rules are pure functions

| Function | Home | Responsibility |
| --- | --- | --- |
| `statusLabel` | `shared/status-label.ts` | `UpToDate` → "Al día" / neutral, `DueSoon` → "Por vencer" / warning, `Overdue` → "Vencida" / danger. Order statuses → Creada, Cotizada, Aprobada, En ejecución, Completada, Cancelada. |
| `formatCop` | `features/order/cop-amount.ts` | Thousands separators, suffix `COP`. The approval line is fixed copy, shown only when the status is `Quoted`. |
| `activeDueSoonBanners` | `features/fleet/due-soon-banner.ts` | From events newest-first plus the fleet list. A banner exists when the newest of `RoutineBecameDueSoon` and `ServiceCompleted` for that vehicle and routine is `RoutineBecameDueSoon`. Text is `{plate} · {routineName} por vencer`. |
| `clientActions` | `features/order/order-actions.ts` | `Created` and `Approved`: cancel. `Quoted`: approve, reject, cancel. `InProgress`, `Completed`, `Cancelled`: none. |
| `cancellationLabel` | `features/order/cancellation-label.ts` | `QuoteRejected` → "Cotización rechazada". `CancelledByClient` → "Cancelada por el cliente". |
| `workshopActions` | `features/simulation/workshop-actions.ts` | `Created`: quote. `Approved`: start. `InProgress`: complete. |
| `todayInBogota` | `features/simulation/bogota-today.ts` | Today's calendar date in `America/Bogota`, as `yyyy-MM-dd`. |

The API already applies the 1,500,000 COP rule and the "ignore lower readings" rule. The UI displays the result. It does not reimplement those policies.

### D5. HTTP services match the existing contract

JSON is camelCase. Status fields are the enum names (`DueSoon`, `Quoted`, `InProgress`). One service per API area, under `core/api/`. The simulation state service calls the telemetry, workshop, and system services. It does not own those HTTP calls.

| Service | Call |
| --- | --- |
| `fleet-api` | `GET /api/fleet` |
| `vehicle-api` | `GET /api/vehicles/{id}` |
| `order-api` | `GET /api/orders/{id}`, `POST .../approve`, `/reject`, `/cancel` |
| `telemetry-api` | `POST /api/telemetry/readings` |
| `system-api` | `POST /api/system/daily-review`, `GET /api/system/events` |
| `workshop-api` | `POST /api/workshop/orders/{id}/quote`, `/start`, `/complete` |

There is no "list orders" route. The panel loads the fleet, then each vehicle detail, and keeps routines whose `openOrder` is set. Three seeded vehicles make that acceptable. Do not add an endpoint in this change.

`ReadingOutcome.result` of `Ignored` shows "Lectura ignorada" and does not apply a new odometer. `Accepted` goes through the refresh signal.

Approve, reject, and cancel open `confirm-dialog` first. Dismissing it does not call the API. Reject and cancel copy says the order is not replaced in this version. Use the native `<dialog>` element so Escape and focus work without a component library.

Errors: `problem-title` reads `title` from a problem-details body. The screen shows that title and keeps the last successful DTO. The control that started the request stays disabled until the call finishes.

### D6. Testing tiers

| Tier | What | Tool |
| --- | --- | --- |
| Strict, test first | The pure functions in D4 | Jasmine, no TestBed |
| Contract | Each HTTP service: method, URL, and body | `HttpClientTestingModule` |
| Component | A chip renders the label it is given. The confirm dialog emits confirm or dismiss. A page shows the actions of a given view model. | TestBed |
| Smoke | `ng build` succeeds. The shell route renders the outlet and the panel. | Build, one shell test |

Do not assert a business rule twice, once in the function and again by re-deriving it in the component. The component test feeds the view model the function already produced.

## Risks / Trade-offs

- [Banner depends on the full event log] → The in-memory log returns every event, newest first. If a later API caps that list, the banner rule must move server-side. This change does not cap it.
- [Order selector needs one request per vehicle] → Fine for the seeded fleet. A larger fleet would need a list endpoint, which is out of scope.
- [Client name is a constant] → The API has no display-name field. The constant matches the mockup's single seeded client.
- [Visual tokens are not from a Penpot file] → Severity colors and Inter match the mockup's language. Swap the token values if a visual file arrives. Do not introduce a component library to get them.
- [150-line cap can split a file too early] → Prefer a named function in the same feature over a generic abstraction. Extract only when the file has two responsibilities or crosses the cap.

## Migration Plan

This is a new app. Rollback is removing `rutaviva-webui` except when a later change has committed it. No data migration. The API stays as it is.

## Open Questions

None. The mockup, the API contract, and the decision to skip Doggie UI are settled.
