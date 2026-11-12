## ADDED Requirements

### Requirement: Order screen titles the plate and the routine

The order screen SHALL load `GET /api/orders/{id}`. The title SHALL be the plate plus the routine name. Amounts SHALL be shown in COP with thousands separators. The screen SHALL show the history items returned by the API, using each item's name and detail.

#### Scenario: Quoted amount is formatted

- **WHEN** the order is `Quoted` with quote amount 2000000
- **THEN** the screen shows the amount as COP with thousands separators and the line "Requiere su aprobación porque supera 1.500.000 COP"

#### Scenario: Unknown order

- **WHEN** `GET /api/orders/{id}` returns 404
- **THEN** the screen shows the problem title and a way back to the fleet

### Requirement: Client actions follow the order status

The screen SHALL offer only the client actions allowed for the current status. Approve, reject, and cancel SHALL ask for confirmation before calling the API. The reject and cancel confirmation SHALL state that a cancelled order is not replaced in this version. The app SHALL NOT offer a control to create another order.

#### Scenario: Created order can only be cancelled

- **WHEN** the status is `Created`
- **THEN** the screen says the order is waiting for the workshop quote and shows only Cancel

#### Scenario: Quoted order can be approved, rejected, or cancelled

- **WHEN** the status is `Quoted` and the user confirms Approve
- **THEN** the app calls `POST /api/orders/{id}/approve` and reloads the order

#### Scenario: Reject confirms and cancels

- **WHEN** the status is `Quoted` and the user confirms Reject
- **THEN** the app calls `POST /api/orders/{id}/reject` and does not offer a new order

#### Scenario: Approved order can still be cancelled

- **WHEN** the status is `Approved`
- **THEN** the screen shows the amount and "Aprobada", and the only client action is Cancel

#### Scenario: In progress hides client actions

- **WHEN** the status is `InProgress`
- **THEN** the screen shows "El taller ya inició el servicio" and hides Cancel, Approve, and Reject

#### Scenario: Completed order points back to the vehicle

- **WHEN** the status is `Completed`
- **THEN** the screen shows the reported kilometers and date, the line "La rutina volvió a estar al día", and a link back to the vehicle

#### Scenario: Cancelled order shows the reason

- **WHEN** the status is `Cancelled` and the reason is `QuoteRejected` or `CancelledByClient`
- **THEN** the screen shows "Cotización rechazada" or "Cancelada por el cliente" and no client actions

#### Scenario: Dismissing confirmation does not call the API

- **WHEN** the user opens Cancel, Reject, or Approve and then dismisses the confirmation
- **THEN** the app does not call the corresponding endpoint
