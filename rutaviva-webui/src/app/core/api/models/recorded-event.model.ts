export interface RecordedEvent {
  name: string;
  title: string;
  vehicleId: string | null;
  routineId: string | null;
  orderId: string | null;
  occurredAt: string;
}
