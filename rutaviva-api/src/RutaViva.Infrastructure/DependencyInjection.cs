using Microsoft.Extensions.DependencyInjection;
using RutaViva.Application.Ports;
using RutaViva.Infrastructure.Clients;
using RutaViva.Infrastructure.Concurrency;
using RutaViva.Infrastructure.Events;
using RutaViva.Infrastructure.Persistence;
using RutaViva.Infrastructure.Seed;
using RutaViva.Infrastructure.Time;
using RutaViva.Infrastructure.Workshops;

namespace RutaViva.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, BogotaClock>();
        services.AddSingleton<InMemoryVehicleRepository>();
        services.AddSingleton<IVehicleRepository>(provider => provider.GetRequiredService<InMemoryVehicleRepository>());
        services.AddSingleton<InMemoryServiceOrderRepository>();
        services.AddSingleton<IServiceOrderRepository>(provider => provider.GetRequiredService<InMemoryServiceOrderRepository>());
        services.AddSingleton<InMemoryEventLog>();
        services.AddSingleton<IEventPublisher>(provider => provider.GetRequiredService<InMemoryEventLog>());
        services.AddSingleton<IEventLog>(provider => provider.GetRequiredService<InMemoryEventLog>());
        services.AddSingleton<IVehicleLock, VehicleLock>();
        services.AddSingleton<ICurrentClient, CurrentClient>();
        services.AddSingleton<IWorkshopDirectory, WorkshopDirectory>();
        services.AddSingleton<FleetSeeder>();
        return services;
    }
}
