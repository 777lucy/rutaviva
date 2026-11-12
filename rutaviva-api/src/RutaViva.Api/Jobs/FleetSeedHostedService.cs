using RutaViva.Infrastructure.Seed;

namespace RutaViva.Api.Jobs;

public sealed class FleetSeedHostedService : IHostedService
{
    private readonly FleetSeeder _seeder;

    public FleetSeedHostedService(FleetSeeder seeder)
    {
        _seeder = seeder;
    }

    public Task StartAsync(CancellationToken cancellationToken) => _seeder.SeedAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
