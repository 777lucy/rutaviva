using MediatR;
using RutaViva.Application.Commands.RunDailyReview;

namespace RutaViva.Api.Jobs;

public sealed class DailyMaintenanceReviewJob : BackgroundService
{
    private static readonly TimeSpan ReviewInterval = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopes;

    public DailyMaintenanceReviewJob(IServiceScopeFactory scopes)
    {
        _scopes = scopes;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // El primer ciclo espera un día. La revisión inmediata entra por POST /api/system/daily-review.
        using PeriodicTimer timer = new PeriodicTimer(ReviewInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            using IServiceScope scope = _scopes.CreateScope();
            ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();
            await sender.Send(new RunDailyReviewCommand(null), stoppingToken);
        }
    }
}
