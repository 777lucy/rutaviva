using System.Collections.Concurrent;
using RutaViva.Application.Ports;
using RutaViva.Domain.ValueObjects.Identifiers;

namespace RutaViva.Infrastructure.Concurrency;

public sealed class VehicleLock : IVehicleLock
{
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _semaphores = new();

    public async Task<IAsyncDisposable> AcquireAsync(VehicleId vehicleId, CancellationToken cancellationToken)
    {
        SemaphoreSlim semaphore = _semaphores.GetOrAdd(vehicleId.Value, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new Release(semaphore);
    }

    private sealed class Release : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public Release(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public ValueTask DisposeAsync()
        {
            _semaphore.Release();
            return ValueTask.CompletedTask;
        }
    }
}
