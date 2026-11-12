# RutaViva — UX proposal

One fleet client opens the app and works only with their own vehicles and orders. Workshop actions and telemetry are simulated inputs on the same screen, so a reviewer can watch the client's view change. There is no login. Authentication is out of scope.

This document follows `domain-model.md`. Screen copy can be Spanish later. Names of statuses and actions below match the domain model.

## Who the screen is for

The person using the app is one seeded fleet client, **Transportes Andinos**. The header shows that name. Every list is already filtered to that client.

The workshop and the telemetry platform are not other sessions. Their actions appear in a panel labeled **Simulación**, visually separate from the client's buttons. 

## What the client can do

| Need | Screen action |
| --- | --- |
| See which routines are up to date, due soon, or overdue | Fleet list and vehicle detail |
| Read the due-soon notice for the current cycle | Banner on the fleet screen. It appears when `RoutineBecameDueSoon` fires and stays until the cycle restarts |
| Approve or reject a quote above 1,500,000 COP | Order screen, only while the order is `Quoted` |
| Cancel an order before work starts | Order screen, while the order is `Created`, `Quoted`, or `Approved` |

Quotes of 1,500,000 COP or less never stop on this screen. The order arrives already `Approved`.

## Screens

Three views. The client moves between Fleet, Vehicle, and Order. The simulation panel stays on the right on every view.

```text
+--------------------------------------------------+------------------+
| RutaViva          Transportes Andinos · Flota   |                  |
+--------------------------------------------------+  Simulación     |
| Banner: ABC-123 · Aceite por vencer             |                  |
|                                                  |  Lectura km     |
|  Mis vehículos                                   |  Revisión diaria|
|  ABC-123   Aceite por vencer · Frenos al día    |  Cotizar        |
|  XYZ-789   Frenos vencida                       |  Iniciar        |
|                                                  |  Completar      |
+--------------------------------------------------+------------------+
```

### 1. Fleet

First screen after open.

- One card per vehicle: plate, short description, current odometer.
- Each routine is a chip: **Al día**, **Por vencer**, **Vencida**.
- The chip color follows severity: neutral, warning, danger. The combined status from the domain model is what the chip shows. Distance and time stay off this list.
- A vehicle with an order waiting for the client shows a second line: "Cotización por aprobar".
- Clicking the card opens Vehicle.

Empty state: "Esta flota no tiene vehículos." Seed data always has vehicles, so this is only a fallback.

### 2. Vehicle

- Plate, description, odometer.
- One row per routine: name, interval in plain language ("cada 10.000 km o 6 meses"), combined status, and the open order if one exists.
- The interval text is a label. The client does not edit intervals in this challenge.
- If the routine has an open order, the row links to Order and shows the order status in Spanish: Creada, Cotizada, Aprobada, En ejecución.
- A completed or cancelled order is not an open order. The row shows the routine status only.

### 3. Order

One order. The title is the plate plus the routine name.

| Order status | What the client sees | Client actions |
| --- | --- | --- |
| `Created` | Waiting for the workshop quote | Cancel |
| `Quoted` | Amount in COP, and the line "Requiere su aprobación porque supera 1.500.000 COP" | Approve, Reject, Cancel |
| `Approved` | Amount and "Aprobada" | Cancel |
| `InProgress` | "El taller ya inició el servicio" | None. Cancel is hidden |
| `Completed` | Reported kilometers and date. Short line: "La rutina volvió a estar al día" | None. Link back to the vehicle |
| `Cancelled` | Reason: quote rejected, or cancelled by the client | None |

Approve asks for a confirmation. Reject and Cancel do too, because both end the order. The copy states that a cancelled order is not replaced in this version.

## Simulation panel

Fixed on the right, with a label **Simulación** and a short note: "Estas acciones no las hace el cliente. Sirven para mostrar el flujo."

| Control | What it calls | What the client then sees |
| --- | --- | --- |
| Vehicle + kilometers, then **Enviar lectura** | `RegisterOdometerReading` | Odometer updates only when the value is higher. A duplicate or lower value shows "Lectura ignorada" inside the panel and does not change the fleet |
| **Correr revisión diaria** and a date field | `RunDailyMaintenanceReview` | Time-based status can move. Default date is today in `America/Bogota` |
| Order selector + amount, then **Enviar cotización** | `SubmitQuote` | Above 1,500,000 COP the order becomes `Quoted` and the client can approve. At or under that amount the order becomes `Approved` with no client step |
| **Iniciar servicio** | `Start` | Status `InProgress`. Client actions disappear |
| Kilometers + date, then **Completar servicio** | `Complete` | Status `Completed`. The routine returns to **Al día** |

The panel lists only orders the workshop can act on. **Enviar cotización** is enabled for `Created`. **Iniciar servicio** is enabled for `Approved`. **Completar servicio** is enabled for `InProgress`.

A small event list under the controls shows the last domain events in plain Spanish ("Orden abierta", "Cotización enviada", "Servicio completado"). This replaces a real notification inbox.

## Status language

| Domain | On screen |
| --- | --- |
| `UpToDate` | Al día |
| `DueSoon` | Por vencer |
| `Overdue` | Vencida |
| `Created` | Creada |
| `Quoted` | Cotizada |
| `Approved` | Aprobada |
| `InProgress` | En ejecución |
| `Completed` | Completada |
| `Cancelled` | Cancelada |

One chip per routine. The worse applicable dimension wins, as in the domain model. A kilometer-only routine does not show a time chip.

## Flows to be able to demo

1. **Due soon, then overdue by kilometers.** Send a reading inside the last 10%. The banner appears once. Send a reading past the interval. The chip becomes **Vencida** and an order in **Creada** appears. The banner does not repeat.
2. **Due soon by date.** Set the review date inside the last 10% of the calendar-month cycle and run the daily review.
3. **High quote.** Workshop quotes above 1,500,000 COP. The client approves. Workshop starts, then completes with kilometers and a date. The routine chip returns to **Al día**.
4. **Automatic approval.** Workshop quotes 1,500,000 COP or less. The order is **Aprobada** with no approve button.
5. **Reject.** Client rejects a high quote. The order is **Cancelada**. The routine stays **Vencida**. No new order appears.
6. **Ignored reading.** Send the same kilometers twice, then a lower value. The panel says the reading was ignored. The odometer stays.

## Decisions

| # | Decision | Why |
| --- | --- | --- |
| U1 | One client session, no login screen. | Authentication is out of scope. The demo still has a clear person. |
| U2 | Workshop and telemetry live in a simulation panel, not a second app. | The reviewer watches one screen. The client's buttons stay limited to approve, reject, and cancel. |
| U3 | The fleet shows the combined status only. | Distance and time are evaluation details. The client acts on the result. |
| U4 | The due-soon banner is once per cycle and is not also shown when a reading jumps straight to overdue. | Matches the domain event `RoutineBecameDueSoon`. |
| U5 | After cancel or reject, the UI does not offer "create another order". | Post-cancellation behavior is out of the domain model. |
| U6 | The daily review has a date the reviewer can set. | Calendar-month rules are hard to demo if the only clock is the real today. |
| U7 | Amounts are shown in COP with thousands separators. The threshold is visible on the order when the client must decide. | The 1,500,000 rule should be obvious without reading the code. |

## Out of these screens

- Choosing or changing the workshop.
- Editing vehicles, routines, or intervals. Seed data owns that setup.
- Payments, invoices, and sending email or push. The event list is the stand-in.
- A workshop inbox as its own product. The simulation panel is enough for the review.
- Anything after an order is cancelled.

## Seed data suggested for the mock

| Vehicle | Odometer | Routine | Interval | Starting point |
| --- | --- | --- | --- | --- |
| ABC-123 | 58,000 km | Aceite | 10,000 km or 6 months | Baseline 50,000 km, 1 Jan 2026. A reading of 59,000 km makes it due soon. 60,000 km makes it overdue |
| XYZ-789 | 102,100 km | Frenos | 6 months | Already overdue by date, order in `Created`, so the quote flow can start immediately |
| QWE-456 | 21,300 km | Llantas | 15,000 km | Up to date, used to show an ignored lower reading |

Workshop for all three: **Taller Central**. Already assigned. The client never picks it.
