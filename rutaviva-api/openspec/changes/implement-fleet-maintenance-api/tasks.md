## 1. Value objects (Strict)

- [x] 1.1 RED: Add failing tests for `Kilometers`, `Money`, and `MaintenanceInterval` construction, comparison, and the rule that an interval has at least one positive side.
- [x] 1.2 GREEN: Implement those value objects so the tests pass.

## 2. Routine progress (Strict)

- [x] 2.1 RED: Add failing `RoutineProgressPolicy` tests for the 10,000 km boundaries, exact overdue, 6 months from 1 January 2026, and 31 January plus 1 month.
- [x] 2.2 GREEN: Implement `RoutineProgressPolicy` with calendar months and the integer 10 percent comparison.

## 3. Vehicle and routines (Strict)

- [x] 3.1 RED: Add failing tests for accepting a higher reading, ignoring an equal reading, ignoring a lower reading, and not updating time status from a reading.
- [x] 3.2 GREEN: Implement `Vehicle.RegisterOdometerReading` and distance evaluation.
- [x] 3.3 RED: Add failing tests for one due-soon signal per cycle, one overdue signal per cycle, a jump straight to overdue, and restart clearing the cycle.
- [x] 3.4 GREEN: Implement routine transitions, `RunDailyReview`, and `RestartRoutine`, including a higher reported mileage advancing the odometer.

## 4. Service orders (Strict)

- [x] 4.1 RED: Add failing tests for the legal transitions, the 1,500,000 COP boundary, cancel refused from `InProgress`, and complete refused without a date.
- [x] 4.2 GREEN: Implement `ServiceOrder` and `QuoteApprovalPolicy`.
- [x] 4.3 RED: Add a failing test that `OpenOrderPolicy` refuses a second open order.
- [x] 4.4 GREEN: Implement `OpenOrderPolicy`.

## 5. Persistence ports (support for Contract tests)

- [x] 5.1 Add in-memory vehicle and order repositories, the event log, and the per-vehicle lock. Register them from `AddInfrastructure`.

## 6. Reading and daily review (Contract)

- [x] 6.1 RED: Add failing use-case tests for an accepted reading, an ignored reading, opening one order when a routine becomes overdue, and not opening a second order.
- [x] 6.2 GREEN: Implement `RegisterOdometerReading` and the shared evaluation step under the vehicle lock.
- [x] 6.3 RED: Add failing tests for a chosen review date entering `DueSoon`, an overdue review opening one order, and a reading leaving time status unchanged.
- [x] 6.4 GREEN: Implement `RunDailyMaintenanceReview` using the same evaluation step.

## 7. Order commands (Contract)

- [x] 7.1 RED: Add failing tests for a quote above 1,500,000 COP staying `Quoted`, a quote at the threshold becoming `Approved`, client approve, and client reject leaving the routine overdue with no new order.
- [x] 7.2 GREEN: Implement `SubmitQuote`, `ApproveQuote`, and `RejectQuote`.
- [x] 7.3 RED: Add failing tests for cancel before `InProgress`, cancel refused after start, start, and complete restarting the routine from the reported values.
- [x] 7.4 GREEN: Implement `CancelOrder`, `StartService`, and `CompleteService`.

## 8. Client queries (Contract)

- [x] 8.1 RED: Add failing tests for the fleet summary, vehicle detail, and order detail of the current client, and for a vehicle the client does not own.
- [x] 8.2 GREEN: Implement the three query handlers and `ICurrentClient` for the seeded client.

## 9. HTTP and operations (Smoke)

- [x] 9.1 Replace the 501 handlers with the use cases and queries. Map validation to 400, not found to 404, and an illegal transition to 409, with Spanish problem titles.
- [x] 9.2 Smoke: Cover an accepted reading, an ignored reading, a negative kilometer value, and one 409 through `WebApplicationFactory`.
- [x] 9.3 Seed Transportes Andinos, Taller Central, and the three demo vehicles on startup. Add the hosted daily job and `POST /api/system/daily-review` for a chosen date, plus `GET /api/system/events`.
- [x] 9.4 Smoke: Run the daily review for a seeded routine and assert the event log contains the resulting event.
