## 1. App scaffold

- [x] 1.1 Scaffold Angular 18 inside `rutaviva-webui` without deleting `openspec/`. Use standalone components, routing, and SCSS. Do not enable SSR. Set `environment.apiUrl` to `http://localhost:5092`.
- [x] 1.2 Add the shell grid, header "RutaViva" / "Transportes Andinos" from `client-session.ts` beside the shell, empty router outlet, and a panel placeholder labeled "Simulación". Add CSS tokens for Inter, 8px spacing, and chip colors neutral, warning, and danger.
- [x] 1.3 Add routes `''`, `vehicles/:id`, and `orders/:id` with placeholder pages. Add `angular-eslint` `max-lines` at 150 for production `*.ts`.
- [x] 1.4 Smoke: `ng build` succeeds and a shell test shows the header, the outlet, and the panel placeholder.

## 2. View rules (Strict)

- [x] 2.1 RED: failing tests for `statusLabel`, `formatCop`, `cancellationLabel`, and `todayInBogota` covering the Spanish labels, COP grouping, both cancellation reasons, and the `America/Bogota` calendar date.
- [x] 2.2 GREEN: implement those four functions so the tests pass, each in its own file: `shared/status-label.ts`, `features/order/cop-amount.ts`, `features/order/cancellation-label.ts`, and `features/simulation/bogota-today.ts`.
- [x] 2.3 RED: failing tests for `activeDueSoonBanners` — banner text, banner removed by a later `ServiceCompleted`, banner kept after overdue, and no banner when the cycle jumped straight to overdue.
- [x] 2.4 GREEN: implement `activeDueSoonBanners` in `features/fleet/due-soon-banner.ts`.
- [x] 2.5 RED: failing tests for `clientActions` and `workshopActions` for every order status, including hidden actions on `InProgress`, `Completed`, and `Cancelled`.
- [x] 2.6 GREEN: implement `clientActions` in `features/order/order-actions.ts` and `workshopActions` in `features/simulation/workshop-actions.ts`.
- [x] 2.7 REFACTOR: split any of these files that mix two rules or pass 150 lines. Re-run the unit tests.

## 3. HTTP services (Contract)

- [x] 3.1 RED: failing `HttpClient` tests for fleet `GET /api/fleet` and vehicle `GET /api/vehicles/{id}`, asserting the URL and the typed response.
- [x] 3.2 GREEN: add the camelCase models under `core/api/models/` and the two services `fleet-api` and `vehicle-api`.
- [x] 3.3 RED: failing tests for `GET /api/orders/{id}` and `POST` approve, reject, and cancel, including that a body is not sent.
- [x] 3.4 GREEN: add the order models and `order-api` under `core/api/`.
- [x] 3.5 RED: failing tests for `telemetry-api` readings, `system-api` daily review and events, and `workshop-api` quote, start, and complete, asserting method, URL, and body.
- [x] 3.6 GREEN: add those three services under `core/api/`. Readings expose `result` as `Accepted` or `Ignored`. Do not add a simulation HTTP service.
- [x] 3.7 RED: failing tests for `problemTitle` reading `title` from a problem-details body and returning a fallback when it is missing.
- [x] 3.8 GREEN: implement `problemTitle` in `core/problem-title.ts`.

## 4. Shared components

- [x] 4.1 RED: failing tests for the status chip (label plus severity class) and the confirm dialog (confirm emits, dismiss does not).
- [x] 4.2 GREEN: implement `status-chip` and `confirm-dialog` with the native `dialog` element. Copy for reject and cancel states that the order is not replaced in this version.
- [x] 4.3 REFACTOR: keep the dialog data type in its own file if the component approaches 150 lines.

## 5. Fleet view

- [x] 5.1 RED: failing page tests for one card per vehicle (plate, description, grouped kilometers), chips from a view model, the "Cotización por aprobar" line, the empty copy "Esta flota no tiene vehículos.", and card navigation.
- [x] 5.2 GREEN: fleet state service (signals), vehicle card, due-soon banner, and fleet page. The page only binds the view model. Banner input comes from `activeDueSoonBanners`.
- [x] 5.3 RED: failing test that a load error shows the problem title and keeps the previous list.
- [x] 5.4 GREEN: wire that error state in the fleet state service.

## 6. Vehicle view

- [x] 6.1 RED: failing page tests for plate, description, odometer, interval text, status chip, open-order Spanish status and link, and no link when `openOrder` is null.
- [x] 6.2 GREEN: vehicle state service, routine row, and vehicle page.
- [x] 6.3 RED: failing test that a 404 shows the problem title and a link back to the fleet.
- [x] 6.4 GREEN: wire the 404 state.

## 7. Order view

- [x] 7.1 RED: failing page tests for the title, formatted quote, the 1,500,000 approval line only when `Quoted`, history names, and the copy for `Created`, `Approved`, `InProgress`, `Completed`, and `Cancelled`.
- [x] 7.2 GREEN: order state service and order page bound to `clientActions` and `cancellationLabel`.
- [x] 7.3 RED: failing tests that confirming approve, reject, or cancel calls the matching endpoint and reloads, and that dismissing the dialog does not call it.
- [x] 7.4 GREEN: connect the confirm dialog to those three actions.
- [x] 7.5 REFACTOR: if the order page mixes layout and action handling, move the action handling into the state service.

## 8. Simulation panel

- [x] 8.1 RED: failing tests that the panel lists open orders (plate, routine, Spanish status) gathered from fleet plus vehicle details, and that quote, start, and complete follow `workshopActions`.
- [x] 8.2 GREEN: simulation state service and the order selector section of the panel.
- [x] 8.3 RED: failing tests for "Lectura ignorada" on `Ignored`, a refresh on `Accepted`, the daily-review date defaulting to `todayInBogota`, and the event titles newest first.
- [x] 8.4 GREEN: reading form, daily-review form, and event list. Disable the control that started a request until it finishes.
- [x] 8.5 RED: failing test that a 409 shows the problem title and leaves the last loaded order list in place.
- [x] 8.6 GREEN: wire that error state. Mount the real panel in the shell so it stays visible on fleet, vehicle, and order routes.
- [x] 8.7 REFACTOR: split the panel into reading, workshop, and event components if the panel file passes 150 lines.

## 9. Refresh after commands

- [x] 9.1 RED: failing tests that a successful reading, review, workshop call, or client call increments the refresh signal, and that fleet, vehicle, order, and events reload when it changes.
- [x] 9.2 GREEN: add `refresh.service.ts` and subscribe each state service to it together with the route id.
- [x] 9.3 REFACTOR: re-run the unit tests and split any production file over 150 lines.

## 10. Smoke

- [x] 10.1 Smoke: `ng build` and the full unit suite pass. The shell test shows the simulation panel beside the routed outlet. No Doggie UI, Angular Material, NgRx, or single-spa dependency is added.
