## ADDED Requirements

### Requirement: Simulation panel stays beside every client view

The simulation panel SHALL be visible on the fleet, vehicle, and order screens. It SHALL be labeled "Simulación" and SHALL show the note "Estas acciones no las hace el cliente. Sirven para mostrar el flujo." Client actions SHALL remain outside this panel.

#### Scenario: Panel is present on each view

- **WHEN** the user opens the fleet, a vehicle, or an order
- **THEN** the simulation panel is still visible on the right

### Requirement: Odometer readings update only when accepted

The panel SHALL let the reviewer pick a vehicle from the fleet and send kilometers with `POST /api/telemetry/readings`. When the response result is `Ignored`, the panel SHALL show "Lectura ignorada" and SHALL NOT change the displayed odometer. When the result is `Accepted`, the app SHALL reload the fleet data from the API.

#### Scenario: Lower or duplicate reading is ignored

- **WHEN** the reviewer sends a reading and the API returns result `Ignored`
- **THEN** the panel shows "Lectura ignorada" and the fleet odometer stays as last loaded

#### Scenario: Higher reading is accepted

- **WHEN** the reviewer sends a reading and the API returns result `Accepted`
- **THEN** the app reloads the fleet and the vehicle odometer shows the new value

### Requirement: Daily review uses a reviewer-chosen date

The panel SHALL offer a date and "Correr revisión diaria". The default date SHALL be today in `America/Bogota`. Submitting SHALL call `POST /api/system/daily-review` with that date and then reload the fleet.

#### Scenario: Review runs for the chosen date

- **WHEN** the reviewer sets a date and runs the daily review
- **THEN** the app posts that date to `/api/system/daily-review` and reloads the fleet

### Requirement: Workshop controls follow the selected order status

The panel SHALL list open orders of the current client's vehicles, each with plate, routine name, and Spanish status. "Enviar cotización" SHALL be enabled only for `Created` and SHALL call `POST /api/workshop/orders/{id}/quote` with the entered amount. "Iniciar servicio" SHALL be enabled only for `Approved` and SHALL call `POST /api/workshop/orders/{id}/start`. "Completar servicio" SHALL be enabled only for `InProgress` and SHALL call `POST /api/workshop/orders/{id}/complete` with the entered kilometers and date. After each successful call the app SHALL reload the affected screens.

#### Scenario: Quote is sent only for a created order

- **WHEN** the selected order is `Created` and the reviewer sends an amount
- **THEN** the app posts that amount to the quote endpoint

#### Scenario: Start and complete stay disabled until the status matches

- **WHEN** the selected order is `Created`
- **THEN** "Iniciar servicio" and "Completar servicio" are disabled

#### Scenario: Start is enabled for an approved order

- **WHEN** the selected order is `Approved` and the reviewer starts the service
- **THEN** the app calls the start endpoint and the order screen shows "El taller ya inició el servicio"

#### Scenario: Complete is enabled for an in-progress order

- **WHEN** the selected order is `InProgress` and the reviewer sends kilometers and a date
- **THEN** the app calls the complete endpoint

### Requirement: The panel lists recent domain events

The panel SHALL load `GET /api/system/events` and show each event title in Spanish, newest first. It SHALL refresh this list after a reading, a daily review, a workshop action, or a client action.

#### Scenario: Latest event is shown

- **WHEN** the event log returns "Orden abierta" as the newest title
- **THEN** the panel shows "Orden abierta" at the top of the event list

### Requirement: A failed command keeps the last successful data

While a panel or client request is in flight, the control that started it SHALL be disabled. When the API returns a problem response, the screen SHALL show the problem title and SHALL keep the last successfully loaded data.

#### Scenario: Conflict shows the problem title

- **WHEN** a workshop or client call returns 409 with a problem title
- **THEN** the panel or screen shows that title and the previous order data remains visible
