## ADDED Requirements

### Requirement: Fleet lists the current client's vehicles

The fleet screen SHALL load `GET /api/fleet` and show one card per vehicle. Each card SHALL show the plate, the description, and the odometer in kilometers with thousands separators. The screen SHALL NOT offer login, and it SHALL NOT let the user pick another client.

#### Scenario: Seeded fleet is visible

- **WHEN** the user opens the app and the API returns ABC-123, XYZ-789, and QWE-456
- **THEN** the fleet screen shows one card for each plate with its description and odometer

#### Scenario: Empty fleet

- **WHEN** `GET /api/fleet` returns an empty list
- **THEN** the screen shows "Esta flota no tiene vehículos."

### Requirement: Header names the seeded client

The header SHALL show "RutaViva" and "Transportes Andinos". The API does not return a client display name, so the app SHALL use that seeded name.

#### Scenario: Header on the fleet screen

- **WHEN** the fleet screen is open
- **THEN** the header shows "RutaViva" and "Transportes Andinos"

### Requirement: Routine chips show the combined status only

Each routine on a fleet card SHALL be one chip. The app SHALL map the API status to the Spanish label and the severity color, and SHALL NOT show distance or time on the fleet list.

#### Scenario: Status labels and colors

- **WHEN** a vehicle's routines include status `UpToDate`, `DueSoon`, and `Overdue`
- **THEN** the chips read "Al día" (neutral), "Por vencer" (warning), and "Vencida" (danger)

#### Scenario: Quote waiting on the card

- **WHEN** a fleet vehicle has `awaitingQuote` true
- **THEN** the card shows a second line "Cotización por aprobar"

### Requirement: Opening a card goes to the vehicle

Selecting a vehicle card SHALL navigate to that vehicle's detail.

#### Scenario: Card opens the vehicle

- **WHEN** the user selects the card for a vehicle
- **THEN** the app opens the vehicle screen for that vehicle id

### Requirement: Due-soon banner stays until the cycle restarts

The fleet screen SHALL show a banner for each routine whose newest relevant event in `GET /api/system/events` is `RoutineBecameDueSoon` for that vehicle and routine, with no later `ServiceCompleted` for the same pair. The banner text SHALL be the plate, the routine name, and "por vencer". A jump straight to overdue SHALL NOT add a banner, because that path does not emit `RoutineBecameDueSoon`. Becoming overdue after a due-soon notice SHALL NOT remove the banner.

#### Scenario: Banner after a due-soon reading

- **WHEN** the event log contains `RoutineBecameDueSoon` for ABC-123 and Aceite, and no later `ServiceCompleted` for that routine
- **THEN** the fleet screen shows a banner "ABC-123 · Aceite por vencer"

#### Scenario: Banner ends when the service is completed

- **WHEN** `ServiceCompleted` for that same vehicle and routine is newer than `RoutineBecameDueSoon`
- **THEN** the fleet screen does not show that banner

#### Scenario: Overdue without a due-soon event has no banner

- **WHEN** the log contains `RoutineBecameOverdue` for a routine and no `RoutineBecameDueSoon` for that cycle
- **THEN** the fleet screen does not show a due-soon banner for that routine
