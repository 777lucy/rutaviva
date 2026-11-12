## Why

The RutaViva API already decides routine status and order transitions, but a reviewer still cannot watch those rules on a screen. The UX mockup in `docs/ux-proposal.md` defines one client session for Transportes Andinos, and this change builds that app so the six demo flows can be shown without a login or a second product.

## What Changes

- Add an Angular 18 app in `rutaviva-webui` that talks to the existing API. Do not change `rutaviva-api`.
- Build the three client views from the mockup: fleet, vehicle, and order. Spanish labels match the mockup.
- Keep a simulation panel on the right of every view for telemetry and workshop actions. Client buttons stay limited to approve, reject, and cancel.
- Show the due-soon banner, status chips, COP amounts, and confirmations described in the mockup.
- Organize the app by feature, with small standalone components and pure view rules. Do not use Doggie UI, Angular Material, NgRx, or single-spa.

## Capabilities

### New Capabilities

- `fleet-view`: Fleet list, routine chips, the due-soon banner, the empty state, and navigation to a vehicle.
- `vehicle-view`: Vehicle detail, interval text, combined status, and the link to an open order.
- `order-view`: Order status copy, the 1,500,000 COP approval line, and approve, reject, and cancel with confirmation.
- `simulation-panel`: Odometer readings, the daily review, quotes, start, complete, ignored readings, and the recent-event list.

### Modified Capabilities

- None. This project has no existing specs.

## Impact

- New Angular project under `rutaviva-webui/`, including its own `openspec/` folder. The API project stays untouched.
- Reads and writes use the endpoints that already exist: fleet, vehicles, orders, telemetry, workshop orders, and system events and daily review.
- The API already allows `http://localhost:4200`. The app uses that origin in development.
- Authentication, vehicle editing, payments, and a separate workshop inbox stay out of scope, as in the mockup.
