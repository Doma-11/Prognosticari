
using FootballPredictionAPI.Services;

namespace FootballPredictionAPI.BackgroundServices
{
    public class FootballSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FootballSyncBackgroundService> _logger;
        public FootballSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<FootballSyncBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(15));

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();

                        var syncService = scope.ServiceProvider
                            .GetRequiredService<FootballSyncService>();

                        await syncService.SyncAllMatchesAsync();

                        _logger.LogInformation(
                            "Automatic football match synchronization completed.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Automatic football match synchronization failed.");
                    }

                    await timer.WaitForNextTickAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal application shutdown.
            }
        }
    }
}
