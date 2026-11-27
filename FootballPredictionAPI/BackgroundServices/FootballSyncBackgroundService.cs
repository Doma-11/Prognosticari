using FootballPredictionAPI.Services;
using FootballPredictionAPI.Services.Interfaces;
using FootballPredictionAPI.Data;
using FootballPredictionAPI.Enums;
using Microsoft.EntityFrameworkCore;

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
            using var timer = new PeriodicTimer(
                TimeSpan.FromMinutes(15));

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await SynchronizeMatchesAsync();

                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    await ScorePendingMatchesAsync(stoppingToken);

                    await timer.WaitForNextTickAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                // Normal application shutdown.
            }
        }

        private async Task SynchronizeMatchesAsync()
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
        }

        private async Task ScorePendingMatchesAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var dbContext = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var scoringService = scope.ServiceProvider
                    .GetRequiredService<IScoringService>();

                var matchIds = await dbContext.Matches
                    .Where(m =>
                        m.Status == MatchStatus.Finished &&
                        m.HomeTeamScore.HasValue &&
                        m.AwayTeamScore.HasValue &&
                        m.Predictions.Any(p => p.PointsAwarded == null))
                    .Select(m => m.Id)
                    .ToListAsync(stoppingToken);

                _logger.LogInformation(
                    "Found {MatchCount} matches pending scoring.",
                    matchIds.Count);

                foreach (int matchId in matchIds)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    try
                    {
                        await scoringService.ScoreMatchAsync(matchId);

                        _logger.LogInformation(
                            "Scoring operation completed for match {MatchId}.",
                            matchId);
                    }
                    catch (OperationCanceledException)
                        when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Scoring failed for match {MatchId}. " +
                            "Pending predictions will be retried in a future cycle.",
                            matchId);
                    }
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Automatic match scoring failed.");
            }
        }


    }
}
