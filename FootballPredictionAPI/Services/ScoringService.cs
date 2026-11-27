using FootballPredictionAPI.Data;
using FootballPredictionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using FootballPredictionAPI.Models;
using FootballPredictionAPI.Enums;
using FootballPredictionAPI.Scoring;
using System.Data;
using Npgsql;

namespace FootballPredictionAPI.Services
{
    public class ScoringService : IScoringService
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        private readonly ILogger<ScoringService> _logger;

        public ScoringService(IDbContextFactory<AppDbContext> dbContextFactory, ILogger<ScoringService> logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task ScoreMatchAsync(int matchId)
        {
            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await ScoreMatchInternalAsync(matchId);

                    return;
                }
                catch (Exception ex) when (
                    attempt < maxAttempts &&
                    IsRetryableException(ex))
                {
                    _logger.LogWarning(
                        ex,
                        "Scoring attempt {Attempt} failed for match {MatchId}. Retrying.",
                        attempt,
                        matchId);

                    await Task.Delay(attempt * 200);
                }
            }
        }

        private async Task ScoreMatchInternalAsync(int matchId)
        {
            await using var dbContext =
                await _dbContextFactory.CreateDbContextAsync();

            await using var transaction =
                await dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

          
                var match = await dbContext.Matches
                    .FirstOrDefaultAsync(m => m.Id == matchId);

                if (match == null)
                {
                    throw new KeyNotFoundException("Match not found.");
                }

                if (match.Status != MatchStatus.Finished)
                {
                    return;
                }

                if (!match.HomeTeamScore.HasValue ||
                    !match.AwayTeamScore.HasValue)
                {
                    return;
                }

                int actualHomeGoals = match.HomeTeamScore.Value;
                int actualAwayGoals = match.AwayTeamScore.Value;

                var predictions = await dbContext.Predictions
                    .Include(p => p.TournamentParticipant)
                    .Where(p => p.MatchId == matchId)
                    .ToListAsync();

                if (predictions.Count == 0)
                {
                    return;
                }

                var tournamentGroups = predictions
                    .GroupBy(p => p.TournamentParticipant.TournamentId);

                foreach (var group in tournamentGroups)
                {
                    var tournamentPredictions = group.ToList();

                    int totalPredictions = tournamentPredictions.Count;

                    int correctOutcomes =
                        ScoringCalculator.CountCorrectOutcomes(
                            tournamentPredictions,
                            actualHomeGoals,
                            actualAwayGoals);

                    int correctExactScores =
                        ScoringCalculator.CountCorrectExactScores(
                            tournamentPredictions,
                            actualHomeGoals,
                            actualAwayGoals);

                    int outcomeRarityBonus =
                        ScoringCalculator.CalculateRarityBonus(
                            correctOutcomes,
                            totalPredictions);

                    int exactScoreRarityBonus =
                        ScoringCalculator.CalculateRarityBonus(
                            correctExactScores,
                            totalPredictions);

                    foreach (var prediction in tournamentPredictions)
                    {
                        if (prediction.PointsAwarded.HasValue)
                        {
                            continue;
                        }

                        int points = ScoringCalculator.CalculateTotalPoints(
                            prediction,
                            actualHomeGoals,
                            actualAwayGoals,
                            outcomeRarityBonus,
                            exactScoreRarityBonus);

                        prediction.PointsAwarded = points;

                        prediction.TournamentParticipant.TotalPoints += points;
                    }
                }

                await dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
        }
            
        

        private static bool IsRetryableException(Exception exception)
        {
            if (exception is PostgresException postgresException)
            {
                return postgresException.SqlState ==
                           PostgresErrorCodes.SerializationFailure
                       ||
                       postgresException.SqlState ==
                           PostgresErrorCodes.DeadlockDetected;
            }

            if (exception.InnerException != null)
            {
                return IsRetryableException(exception.InnerException);
            }

            return false;
        }
    }
}