using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Infrastructure.Seed;

public static class DemoFleet
{
    public static readonly Guid ClientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid WorkshopId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid AbcVehicleId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid XyzVehicleId = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid QweVehicleId = Guid.Parse("55555555-5555-5555-5555-555555555555");
    public static readonly Guid AceiteRoutineId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    public static readonly Guid FrenosRoutineId = Guid.Parse("77777777-7777-7777-7777-777777777777");
    public static readonly Guid LlantasRoutineId = Guid.Parse("88888888-8888-8888-8888-888888888888");
    public static readonly Guid AbcLlantasRoutineId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid XyzAceiteRoutineId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid XyzLlantasRoutineId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    public static readonly Guid QweAceiteRoutineId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");
    public static readonly Guid FrenosOrderId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    public const string WorkshopName = "Taller Central";

    public static FleetClientId Client => new(ClientId);

    public static WorkshopId Workshop => new(WorkshopId);
}
