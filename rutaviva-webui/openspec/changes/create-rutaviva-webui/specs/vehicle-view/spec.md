## ADDED Requirements

### Requirement: Vehicle detail shows plate, odometer, and routines

The vehicle screen SHALL load `GET /api/vehicles/{id}` and show the plate, the description, and the odometer in kilometers. It SHALL show one row per routine with the name, the interval text from the API, and the combined status chip. The user SHALL NOT be able to edit the vehicle, the routine, or the interval.

#### Scenario: Routine row uses the API interval

- **WHEN** the vehicle detail returns Aceite with interval "cada 10.000 km o 6 meses" and status `DueSoon`
- **THEN** the row shows that interval text and a "Por vencer" chip

#### Scenario: Unknown vehicle

- **WHEN** `GET /api/vehicles/{id}` returns 404
- **THEN** the screen shows the problem title and a way back to the fleet

### Requirement: An open order links to the order screen

When a routine includes an open order, the row SHALL link to the order screen and SHALL show the order status in Spanish. A routine without an open order SHALL show the routine status only.

#### Scenario: Open order statuses

- **WHEN** the open order status is `Created`, `Quoted`, `Approved`, or `InProgress`
- **THEN** the row shows "Creada", "Cotizada", "Aprobada", or "En ejecución" and links to that order

#### Scenario: No open order

- **WHEN** the routine's open order is null
- **THEN** the row does not link to an order and does not show an order status
