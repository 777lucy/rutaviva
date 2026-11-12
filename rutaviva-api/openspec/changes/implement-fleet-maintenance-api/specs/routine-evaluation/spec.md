## ADDED Requirements

### Requirement: A routine interval has at least one side
A maintenance interval MUST have a kilometer length, a calendar-month length, or both. Each present side MUST be greater than zero. A missing side MUST stay `NotApplicable` and MUST be excluded from the combined status.

#### Scenario: Kilometer-only routine
- **WHEN** a routine is every 10,000 km and has no month length
- **THEN** distance status is evaluated
- **THEN** time status stays `NotApplicable`

#### Scenario: Time-only routine
- **WHEN** a routine is every 6 months and has no kilometer length
- **THEN** time status is evaluated
- **THEN** distance status stays `NotApplicable`

### Requirement: Dimension status uses the 10 percent window
For a dimension that exists, the system MUST treat it as `Overdue` when elapsed is greater than or equal to the cycle length. Otherwise it MUST treat it as `DueSoon` when the remaining amount times 10 is less than or equal to the cycle length. Otherwise it MUST treat it as `UpToDate`. The comparison MUST use integers.

#### Scenario: Ten thousand kilometers at the warning boundary
- **WHEN** 1,000 km remain of a 10,000 km interval
- **THEN** the distance dimension is `DueSoon`

#### Scenario: One kilometer outside the warning
- **WHEN** 1,001 km remain of a 10,000 km interval
- **THEN** the distance dimension is `UpToDate`

#### Scenario: Exact kilometer interval is overdue
- **WHEN** elapsed kilometers equal the interval
- **THEN** the distance dimension is `Overdue`

### Requirement: Time uses calendar months
The due date MUST be the baseline date plus the configured number of calendar months. The cycle length MUST be the real number of days between those dates. A month MUST NOT be stored as a fixed 30 days.

#### Scenario: Six months from 1 January 2026
- **WHEN** the baseline date is 1 January 2026 and the interval is 6 months
- **THEN** the due date is 1 July 2026
- **THEN** the cycle length is 181 days
- **THEN** the routine is `DueSoon` on 13 June 2026
- **THEN** the routine is `Overdue` on 1 July 2026

#### Scenario: Month end
- **WHEN** the baseline date is 31 January 2026 and the interval is 1 month
- **THEN** the due date is 28 February 2026
- **THEN** the routine is `DueSoon` on 26 February 2026
- **THEN** the routine is `Overdue` on 28 February 2026

### Requirement: Combined status is the worse applicable dimension
The routine status MUST be the more severe of the applicable dimensions, in the order `UpToDate`, then `DueSoon`, then `Overdue`.

#### Scenario: One side is due soon and the other is up to date
- **WHEN** distance status is `DueSoon` and time status is `UpToDate`
- **THEN** the routine status is `DueSoon`

#### Scenario: Either side overdue wins
- **WHEN** distance status is `DueSoon` and time status is `Overdue`
- **THEN** the routine status is `Overdue`

### Requirement: Each trigger updates only its dimension
A higher odometer reading MUST recompute distance status and MUST NOT recompute time status. The daily review MUST recompute time status and MUST NOT treat the call as a new odometer reading. Combined status MUST be recomputed after the updated dimension.

#### Scenario: Reading does not apply a passed date
- **WHEN** a higher reading arrives on a date that is already past the time due date
- **AND** the daily review has not run for that date
- **THEN** time status stays unchanged

#### Scenario: Daily review uses the stored odometer
- **WHEN** the daily review runs
- **THEN** distance status stays at the value last produced by an accepted reading
- **THEN** time status is recomputed from the baseline date and the review date

### Requirement: The odometer never decreases
The vehicle MUST store a reading only when it is strictly greater than the current odometer. An equal or lower reading MUST leave the vehicle unchanged and MUST NOT be reported as an error.

#### Scenario: Lower reading is ignored
- **WHEN** the current odometer is 59,000 km and a reading of 58,000 km arrives
- **THEN** the odometer stays 59,000 km
- **THEN** no routine status changes

#### Scenario: Equal reading is ignored
- **WHEN** a reading equals the current odometer
- **THEN** the odometer stays unchanged

### Requirement: Notices fire once per cycle on entry
The vehicle MUST raise the due-soon signal only when combined status enters `DueSoon`. It MUST raise the overdue signal only when combined status enters `Overdue`. Later evaluations that stay in the same status MUST NOT raise the signal again. A jump from `UpToDate` to `Overdue` MUST raise only the overdue signal.

#### Scenario: Due soon is notified once
- **WHEN** a routine enters `DueSoon` and a later evaluation is still `DueSoon`
- **THEN** the due-soon signal is raised only on the first entry

#### Scenario: Jump skips the due-soon notice
- **WHEN** one evaluation moves a routine from `UpToDate` to `Overdue`
- **THEN** the overdue signal is raised
- **THEN** the due-soon signal is not raised

### Requirement: Completion restarts the cycle
Restarting a routine MUST set the baseline to the reported kilometers and date, MUST return applicable dimensions to `UpToDate`, and MUST allow a new due-soon signal in the new cycle. Reported kilometers greater than the vehicle odometer MUST advance the odometer. Reported kilometers lower than the odometer MUST NOT decrease it, and the baseline MUST still use the reported kilometers.

#### Scenario: Restart clears the cycle
- **WHEN** a routine is overdue and is restarted from a reported service
- **THEN** its status is `UpToDate`
- **THEN** a later entry into `DueSoon` can raise the due-soon signal again

#### Scenario: Reported mileage above the odometer
- **WHEN** the reported service kilometers are greater than the current odometer
- **THEN** the vehicle odometer becomes those kilometers
