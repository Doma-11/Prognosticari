using FootballPredictionAPI.Data;
using FootballPredictionAPI.DTOs.Predictions;
using FootballPredictionAPI.Enums;
using FootballPredictionAPI.Exceptions;
using FootballPredictionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using FootballPredictionAPI.Models;
using Npgsql;




namespace FootballPredictionAPI.Services
{
    public class PredictionService : IPredictionService
    {
        private readonly AppDbContext _dbContext;
        public PredictionService(AppDbContext dbContext) {

            _dbContext = dbContext;

        }
        public async Task<PredictionResponseDto> CreatePredictionAsync(int tournamentId, int userId, CreatePredictionDto createPredictionDto)
        {
            var tournament = await _dbContext.Tournaments
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                throw new KeyNotFoundException($"Tournament with ID {tournamentId} does not exist.");
            }
            
            var participant = await _dbContext.TournamentParticipants
                .AsNoTracking()
                .FirstOrDefaultAsync(tp => tp.TournamentId == tournamentId && tp.UserId == userId);

            if (participant == null)
            {
                throw new ForbiddenException("You are not a participant in this tournament.");
            }


            var match= await _dbContext.Matches
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == createPredictionDto.MatchId);

            if (match == null)
            {
                throw new KeyNotFoundException("Match not found.");
            }

            if(match.SeasonId != tournament.SeasonId)
            {
                throw new ArgumentException("Match does not belong to the tournament's season.");
            }

            var matchDate = DateOnly.FromDateTime(match.MatchDate);

            if (matchDate < tournament.StartDate ||
                matchDate > tournament.EndDate)
            {
                throw new ArgumentException(
                    "Match is outside the tournament's date range.");
            }

            if (match.MatchDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Predictions are closed for this match.");
            }


            if (match.Status != MatchStatus.Scheduled)
            {
                throw new InvalidOperationException(
                    "Predictions are only allowed for scheduled matches.");
            }


            // AnyAsync returns bool if prediction exists
            var predictionExists = await _dbContext.Predictions
                .AnyAsync(p => p.TournamentParticipantId == participant.Id && p.MatchId == match.Id);

            if (predictionExists)
            {
                throw new ConflictException(
                    "You have already submitted a prediction for this match.");
            }

            var now = DateTime.UtcNow;

            var prediction = new Prediction
            {
                TournamentParticipantId = participant.Id,
                MatchId = match.Id,
                PredictedHomeGoals = createPredictionDto.PredictedHomeGoals,
                PredictedAwayGoals = createPredictionDto.PredictedAwayGoals,
                PointsAwarded = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            // Add the prediction to the database context, EF Core now tracks it and will insert it into the database when SaveChangesAsync is called
            _dbContext.Predictions.Add(prediction);

            try
            {
                //save changes to the database, this will insert the new prediction into the Predictions table
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pgEx &&
                      pgEx.SqlState == PostgresErrorCodes.UniqueViolation &&
                      pgEx.ConstraintName == "IX_Predictions_TournamentParticipantId_MatchId")
            {
                throw new ConflictException(
                    "You have already submitted a prediction for this match.");
            }

            //return the prediction as a DTO, mapping the properties from the Prediction entity to the PredictionResponseDto
            return new PredictionResponseDto
            {
                Id = prediction.Id,
                MatchId = prediction.MatchId,
                PredictedHomeGoals = prediction.PredictedHomeGoals,
                PredictedAwayGoals = prediction.PredictedAwayGoals,
                PointsAwarded = prediction.PointsAwarded,
                CreatedAt = prediction.CreatedAt,
                UpdatedAt = prediction.UpdatedAt
            };

        }

        public async Task<PredictionResponseDto> UpdatePredictionAsync(int tournamentId,int predictionId,int userId,UpdatePredictionDto dto)
        {
            // 1. Find the prediction and its participant
            var prediction = await _dbContext.Predictions
                .Include(p => p.TournamentParticipant)
                .Include(p => p.Match)
                .FirstOrDefaultAsync(p => p.Id == predictionId);

            if (prediction == null)
            {
                throw new KeyNotFoundException("Prediction not found.");
            }

            // 2. Verify the prediction belongs to this tournament
            if (prediction.TournamentParticipant.TournamentId != tournamentId)
            {
                throw new KeyNotFoundException(
                    "Prediction not found in this tournament.");
            }

            // 3. Verify that the authenticated user owns the prediction
            if (prediction.TournamentParticipant.UserId != userId)
            {
                throw new ForbiddenException(
                    "You cannot edit another player's prediction.");
            }

            // 4. Check that the match has not started
            if (prediction.Match.MatchDate <= DateTime.UtcNow)
            {
                throw new InvalidOperationException(
                    "Predictions cannot be edited after kickoff.");
            }

            // 5. Check that the match is still scheduled
            if (prediction.Match.Status != MatchStatus.Scheduled)
            {
                throw new InvalidOperationException(
                    "Predictions can only be edited for scheduled matches.");
            }

            // 6. Update the predicted score
            prediction.PredictedHomeGoals = dto.PredictedHomeGoals;
            prediction.PredictedAwayGoals = dto.PredictedAwayGoals;
            prediction.UpdatedAt = DateTime.UtcNow;

            // 7. Save changes
            await _dbContext.SaveChangesAsync();

            // 8. Return the updated prediction
            return new PredictionResponseDto
            {
                Id = prediction.Id,
                MatchId = prediction.MatchId,
                PredictedHomeGoals = prediction.PredictedHomeGoals,
                PredictedAwayGoals = prediction.PredictedAwayGoals,
                PointsAwarded = prediction.PointsAwarded,
                CreatedAt = prediction.CreatedAt,
                UpdatedAt = prediction.UpdatedAt
            };
        }


        public async Task<List<PredictionResponseDto>> GetMyPredictionsAsync(int tournamentId,int userId)
        {
            // 1. Verify that the tournament exists
            var tournamentExists = await _dbContext.Tournaments
                .AnyAsync(t => t.Id == tournamentId);

            if (!tournamentExists)
            {
                throw new KeyNotFoundException("Tournament not found.");
            }

            // 2. Find the authenticated user's tournament participation
            var participant = await _dbContext.TournamentParticipants
                .AsNoTracking()
                .FirstOrDefaultAsync(tp =>
                    tp.TournamentId == tournamentId &&
                    tp.UserId == userId);

            if (participant == null)
            {
                throw new ForbiddenException(
                    "You are not a participant in this tournament.");
            }

            // 3. Retrieve the player's predictions
            var predictions = await _dbContext.Predictions
                .AsNoTracking()
                .Where(p => p.TournamentParticipantId == participant.Id)
                .OrderBy(p => p.Match.MatchDate)
                .Select(p => new PredictionResponseDto
                {
                    Id = p.Id,
                    MatchId = p.MatchId,
                    PredictedHomeGoals = p.PredictedHomeGoals,
                    PredictedAwayGoals = p.PredictedAwayGoals,
                    PointsAwarded = p.PointsAwarded,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return predictions;
        }

        public async Task<PredictionResponseDto> GetMyPredictionAsync(int tournamentId, int matchId, int userId)
        {
            //verify the tournament exists
            var tournamentExists = await _dbContext.Tournaments.AnyAsync(t => t.Id == tournamentId);
            if (!tournamentExists)
            {
                throw new KeyNotFoundException("Tournament not found.");
            }
            //verify that user is a participant in the tournament
            var participant = await _dbContext.TournamentParticipants
                .AsNoTracking()
                .FirstOrDefaultAsync(tp => tp.Id == tournamentId && tp.UserId== userId);

            if (participant == null)
            {
                throw new ForbiddenException(
                    "You are not a participant in this tournament.");
            }
            // retrieve the prediction for specific match
            var prediction = await _dbContext.Predictions
                .AsNoTracking()
                .Where(p => p.TournamentParticipantId == participant.Id && p.MatchId == matchId)
                .Select(p => new PredictionResponseDto
                {

                    Id = p.Id,
                    MatchId = p.MatchId,
                    PredictedHomeGoals = p.PredictedHomeGoals,
                    PredictedAwayGoals = p.PredictedAwayGoals,
                    PointsAwarded = p.PointsAwarded,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt


                }).SingleOrDefaultAsync();

            if (prediction == null)
            {
                throw new KeyNotFoundException("Prediction not found.");
            }

            return prediction;

        }

        public async Task<List<TournamentMatchDto>> GetTournamentMatchesAsync(int tournamentId,int userId)
        {
            // 1. Find the tournament
            var tournament = await _dbContext.Tournaments
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                throw new KeyNotFoundException("Tournament not found.");
            }

            // 2. Verify tournament membership
            var isParticipant = await _dbContext.TournamentParticipants
                .AnyAsync(tp =>
                    tp.TournamentId == tournamentId &&
                    tp.UserId == userId);

            if (!isParticipant)
            {
                throw new ForbiddenException(
                    "You are not a participant in this tournament.");
            }

            // 3. Retrieve matches belonging to the tournament
            var matches = await _dbContext.Matches
                .AsNoTracking()
                .Where(m =>
                    m.SeasonId == tournament.SeasonId &&
                    DateOnly.FromDateTime(m.MatchDate) >= tournament.StartDate &&
                    DateOnly.FromDateTime(m.MatchDate) <= tournament.EndDate)
                .OrderBy(m => m.MatchDate)
                .Select(m => new TournamentMatchDto
                {
                    Id = m.Id,
                    HomeTeamId = m.HomeTeamId,
                    AwayTeamId = m.AwayTeamId,
                    MatchDate = m.MatchDate,
                    Status = m.Status.ToString(),
                    HomeTeamScore = m.HomeTeamScore,
                    AwayTeamScore = m.AwayTeamScore
                })
                .ToListAsync();

            return matches;
        }


    }
}
