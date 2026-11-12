## ADDED Requirements

### Requirement: The client API shows only the current client's fleet
Client routes MUST read and change data for the seeded current client only. A vehicle or order that belongs to another client MUST be treated as not found. The fleet summary MUST include each vehicle plate, description, odometer, combined routine status, and whether an order is waiting for a quote. The vehicle detail MUST include each routine name, a plain-language interval, the combined status, and the open order when one exists. The order detail MUST include status, quote amount when present, workshop, and the history needed by the order screen.

#### Scenario: Fleet lists the seeded vehicles
- **WHEN** the client requests the fleet
- **THEN** the response contains the seeded vehicles for Transportes Andinos
- **THEN** each routine includes its combined status

#### Scenario: Foreign vehicle is hidden
- **WHEN** the client requests a vehicle that is not theirs
- **THEN** the response status is 404

### Requirement: Telemetry reports acceptance or a silent ignore
`POST /api/telemetry/readings` MUST accept a vehicle id and a kilometer value. A strictly higher value MUST update the odometer, evaluate distance, and return `result` `Accepted`. An equal or lower value MUST return HTTP 200 with `result` `Ignored` and MUST NOT change state. A negative value or an unknown vehicle MUST NOT update state.

#### Scenario: Higher reading is accepted
- **WHEN** telemetry submits kilometers above the vehicle odometer
- **THEN** the response status is 200
- **THEN** the result is `Accepted`
- **THEN** a later fleet read shows the new odometer

#### Scenario: Lower reading is ignored without an error
- **WHEN** telemetry submits kilometers below the vehicle odometer
- **THEN** the response status is 200
- **THEN** the result is `Ignored`
- **THEN** the stored odometer is unchanged

#### Scenario: Unknown vehicle
- **WHEN** telemetry submits a reading for an unknown vehicle
- **THEN** the response status is 404

### Requirement: Workshop and client commands follow the order state machine
Workshop quote, start, and complete routes, and client approve, reject, and cancel routes, MUST call the matching use case. An illegal transition MUST return HTTP 409 and leave the order unchanged. A missing order MUST return HTTP 404. A quote above 1,500,000 COP MUST leave the order readable as `Quoted`. A quote of 1,500,000 COP or less MUST leave it readable as `Approved`.

#### Scenario: High quote becomes visible to the client
- **WHEN** the workshop submits a quote above 1,500,000 COP for a `Created` order
- **THEN** the response status is 200
- **THEN** the client can read that order as `Quoted`

#### Scenario: Illegal start returns conflict
- **WHEN** the workshop starts an order that is not `Approved`
- **THEN** the response status is 409
- **THEN** the order status is unchanged

#### Scenario: Client cancels before execution
- **WHEN** the client cancels an order in `Created`, `Quoted`, or `Approved`
- **THEN** the response status is 200
- **THEN** a later read shows the order as `Cancelled`

### Requirement: The daily review can be run for a chosen date
`POST /api/system/daily-review` MUST run time evaluation for the current client's vehicles on the supplied date. The hosted daily job MUST call the same use case with today's date in `America/Bogota`. A review that makes a routine overdue MUST open one order when none is open. The response MUST list the routines whose status changed.

#### Scenario: Chosen date marks a routine due soon
- **WHEN** the caller runs the daily review on a date inside the 10 percent window of a time interval
- **THEN** the routine is `DueSoon` on the next vehicle read
- **THEN** the due-soon event is available in the event log

#### Scenario: Overdue review opens an order
- **WHEN** the daily review date reaches the calendar due date and no open order exists
- **THEN** the routine is `Overdue`
- **THEN** one `Created` order exists for that routine

### Requirement: Recent events are readable and failures use problem details
`GET /api/system/events` MUST return the published domain events in newest-first order. Problem responses MUST use problem details. Titles shown to the caller MUST be Spanish. An ignored reading MUST NOT be a problem response.

#### Scenario: Event log after a due-soon transition
- **WHEN** a routine enters `DueSoon` and the caller reads the event log
- **THEN** the log contains the due-soon event for that vehicle and routine

#### Scenario: Invalid kilometers are a bad request
- **WHEN** telemetry submits a negative kilometer value
- **THEN** the response status is 400
- **THEN** the problem title is Spanish
