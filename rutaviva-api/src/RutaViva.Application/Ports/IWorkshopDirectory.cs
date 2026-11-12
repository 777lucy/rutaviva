using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Application.Ports;

public interface IWorkshopDirectory
{
    string GetName(WorkshopId id);
}
