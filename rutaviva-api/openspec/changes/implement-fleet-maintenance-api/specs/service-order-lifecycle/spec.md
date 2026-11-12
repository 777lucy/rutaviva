## ADDED Requirements

### Requirement: An order covers one routine at the assigned workshop
A service order MUST reference exactly one vehicle routine. Its workshop MUST be the workshop assigned to the vehicle when the order is opened. The order MUST store the routine name as it was at that moment.

#### Scenario: Order is opened for one overdue routine
- **WHEN** a routine becomes overdue and no open order exists for that vehicle and routine
- **THEN** a new order is created in `Created`
- **THEN** the order workshop is the vehicle's assigned workshop

### Requirement: At most one open order per vehicle and routine
An order MUST count as open unless its status is `Completed` or `Cancelled`. The system MUST refuse a second open order for the same vehicle and routine. An order MUST be opened only on the transition into `Overdue`.

#### Scenario: Second open order is refused
- **WHEN** an open order already exists for a vehicle and routine
- **THEN** another order is not created for that pair

#### Scenario: Cancellation does not open a replacement
- **WHEN** an order is cancelled and the routine is still overdue
- **THEN** no new order is created
- **THEN** the routine stays overdue

### Requirement: Quote approval uses 1500000 COP
Submitting a quote MUST be allowed only from `Created`. An amount greater than 1,500,000 COP MUST leave the order in `Quoted`. An amount equal to or less than 1,500,000 COP MUST leave the order in `Approved` in the same operation. The automatic path MUST NOT wait for the client.

#### Scenario: High quote waits for the client
- **WHEN** the workshop submits a quote of 1,800,000 COP
- **THEN** the order status is `Quoted`

#### Scenario: Threshold quote is approved automatically
- **WHEN** the workshop submits a quote of 1,500,000 COP
- **THEN** the order status is `Approved`

#### Scenario: Low quote is approved automatically
- **WHEN** the workshop submits a quote below 1,500,000 COP
- **THEN** the order status is `Approved`

### Requirement: The client decides only while the order is quoted
Approve and reject MUST be allowed only from `Quoted`. Approve MUST move the order to `Approved`. Reject MUST move the order to `Cancelled` with reason `QuoteRejected`.

#### Scenario: Client approves
- **WHEN** the client approves a `Quoted` order
- **THEN** the order status is `Approved`

#### Scenario: Client rejects
- **WHEN** the client rejects a `Quoted` order
- **THEN** the order status is `Cancelled`
- **THEN** the cancellation reason is `QuoteRejected`

### Requirement: The client can cancel before execution
Cancel MUST be allowed from `Created`, `Quoted`, and `Approved`, and MUST set reason `CancelledByClient`. Cancel MUST be refused from `InProgress`, `Completed`, and `Cancelled`.

#### Scenario: Cancel before work starts
- **WHEN** the client cancels an `Approved` order
- **THEN** the order status is `Cancelled`
- **THEN** the cancellation reason is `CancelledByClient`

#### Scenario: Cancel after start is refused
- **WHEN** the client cancels an `InProgress` order
- **THEN** the order status stays `InProgress`

### Requirement: The workshop starts and completes in order
Start MUST be allowed only from `Approved` and MUST move the order to `InProgress`. Complete MUST be allowed only from `InProgress` and MUST require reported kilometers and a service date. Complete MUST move the order to `Completed` and MUST restart that routine from the reported values. Any other transition MUST be refused and MUST leave the order unchanged.

#### Scenario: Start from approved
- **WHEN** the workshop starts an `Approved` order
- **THEN** the order status is `InProgress`

#### Scenario: Start before approval is refused
- **WHEN** the workshop starts a `Quoted` order
- **THEN** the order status stays `Quoted`

#### Scenario: Complete restarts the routine
- **WHEN** the workshop completes an `InProgress` order with kilometers and a date
- **THEN** the order status is `Completed`
- **THEN** the routine baseline is those reported values
- **THEN** the routine status is `UpToDate`

#### Scenario: Complete without a date is refused
- **WHEN** a completion is submitted without a service date
- **THEN** the order status stays `InProgress`
