using RutaViva.Application.Ports;
using RutaViva.Domain.ValueObjects.Identifiers;
using RutaViva.Infrastructure.Seed;

namespace RutaViva.Infrastructure.Workshops;

public sealed class WorkshopDirectory : IWorkshopDirectory
{
    public string GetName(WorkshopId id) =>
        id == DemoFleet.Workshop ? DemoFleet.WorkshopName : "Taller";
}
