using FootballPredictionAPI.Data;
using FootballPredictionAPI.DTOs.Tournaments;
using FootballPredictionAPI.Models;
using FootballPredictionAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using FootballPredictionAPI.Exceptions;
using Npgsql;
namespace FootballPredictionAPI.Services
{
    public class TournamentService : ITournamentService
    {

        private readonly AppDbContext _context;

        public TournamentService(AppDbContext context)
        {
            _context = context;
        }

        // 1. CREATE TOURNAMENT
        public async Task<CreatedTournamentResponseDto> CreateTournamentAsync(CreateTournamentRequestDto request, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Tournament name is required.");

            if (request.StartDate > request.EndDate)
                throw new ArgumentException("Start date cannot be after end date.");

            var season = await _context.Seasons
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.SeasonId);

            if (season == null)
                throw new KeyNotFoundException("Season not found.");

            if (request.StartDate < season.StartDate ||
                request.EndDate > season.EndDate)
                throw new ArgumentException("Tournament dates must be within the season.");

            string invitationCode;

            do
            {
                invitationCode = Convert.ToHexString(RandomNumberGenerator.GetBytes(6));
            }
            while (await _context.Tournaments
                .AnyAsync(t => t.InvitationCode == invitationCode));

            var tournament = new Tournament
            {
                Name = request.Name.Trim(),
                SeasonId = request.SeasonId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                InvitationCode = invitationCode,
                CreatedByUserId = currentUserId
            };

            _context.Tournaments.Add(tournament);

            // The creator automatically becomes the first participant.
            var participant = new TournamentParticipant
            {
                Tournament = tournament,
                UserId = currentUserId,
                TotalPoints = 0,
                JoinedAt = DateTime.UtcNow
            };

            _context.TournamentParticipants.Add(participant);

            const int maxAttempts = 3;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    break;
                }
                catch (DbUpdateException ex)
                    when (ex.InnerException is PostgresException pgEx
                          && pgEx.SqlState == PostgresErrorCodes.UniqueViolation
                          && pgEx.ConstraintName == "IX_Tournaments_InvitationCode"
                          && attempt < maxAttempts)
                {
                    tournament.InvitationCode =
                        Convert.ToHexString(RandomNumberGenerator.GetBytes(6));
                }
            }

            return new CreatedTournamentResponseDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                InvitationCode = tournament.InvitationCode
            };

        }

        // 2. GET TOURNAMENT BY ID

        public async Task<TournamentResponseDto?> GetTournamentByIdAsync(int tournamentId, int currentUserId)
        {
            return await _context.Tournaments
               .AsNoTracking()
               .Where(t => t.Id == tournamentId)
               .Where(t => t.Participants
               .Any(p => p.UserId == currentUserId))
               .Select(t => new TournamentResponseDto
               {
                   Id = t.Id,
                   Name = t.Name,
                   SeasonId = t.SeasonId,
                   StartDate = t.StartDate,
                   EndDate = t.EndDate,
                   CreatedByUserId = t.CreatedByUserId,
                   ParticipantCount = t.Participants.Count()
               })
               .FirstOrDefaultAsync();
        }

        // 3. GET MY TOURNAMENTS
        public async Task<List<TournamentResponseDto>> GetMyTournamentsAsync(int currentUserId)
        {
            return await _context.Tournaments
                 .AsNoTracking()
                 .Where(t => t.Participants
                 .Any(p => p.UserId == currentUserId))
                 .Select(t => new TournamentResponseDto
                 {
                     Id = t.Id,
                     Name = t.Name,
                     SeasonId = t.SeasonId,
                     StartDate = t.StartDate,
                     EndDate = t.EndDate,
                     CreatedByUserId = t.CreatedByUserId,
                     ParticipantCount = t.Participants.Count()
                 })
                 .ToListAsync();
        }


        public async Task<JoinTournamentResponseDto> JoinTournamentAsync(JoinTournamentRequestDto request, int currentUserId)
        {
            if (string.IsNullOrWhiteSpace(request.InvitationCode))
                throw new ArgumentException("Invitation code is required.");

            string code = request.InvitationCode.Trim().ToUpperInvariant();

            var tournament = await _context.Tournaments
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.InvitationCode == code);

            if (tournament == null)
                throw new KeyNotFoundException("Invalid invitation code.");

            if (DateOnly.FromDateTime(DateTime.UtcNow) > tournament.EndDate)
                throw new InvalidOperationException(
                    "This tournament has already ended.");

            bool alreadyJoined = await _context.TournamentParticipants
                .AnyAsync(p => p.TournamentId == tournament.Id &&
                               p.UserId == currentUserId);

            if (alreadyJoined)
                throw new InvalidOperationException(
                    "You have already joined this tournament.");

            var participant = new TournamentParticipant
            {
                TournamentId = tournament.Id,
                UserId = currentUserId,
                TotalPoints = 0,
                JoinedAt = DateTime.UtcNow
            };

            _context.TournamentParticipants.Add(participant);




            //two layers of protection: 1.The service checks membership before inserting. 2.PostgreSQL guarantees uniqueness even when requests arrive simultaneously.
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
                when (ex.InnerException is PostgresException pgEx
                      && pgEx.SqlState == PostgresErrorCodes.UniqueViolation
                      && pgEx.ConstraintName == "IX_TournamentParticipants_TournamentId_UserId")
            {
                throw new InvalidOperationException(
                    "You are already a participant in this tournament.", ex);
            }

            return new JoinTournamentResponseDto
            {
                TournamentId = tournament.Id,
                TournamentName = tournament.Name,
                JoinedAt = participant.JoinedAt
            };
        }
        public async Task<List<TournamentParticipantResponseDto>> GetParticipantsAsync(int tournamentId, int currentUserId)
        {
            bool isParticipant = await _context.TournamentParticipants
               .AnyAsync(p => p.TournamentId == tournamentId &&
                              p.UserId == currentUserId);

            if (!isParticipant)
                throw new ForbiddenException("You cannot view participants of this tournament.");

            return await _context.TournamentParticipants
                .AsNoTracking()
                .Where(p => p.TournamentId == tournamentId)
                .OrderByDescending(p => p.TotalPoints)
                .ThenBy(p => p.JoinedAt)
                .Select(p => new TournamentParticipantResponseDto
                {
                    UserId = p.UserId,
                    UserName = p.User.UserName,
                    TotalPoints = p.TotalPoints,
                    JoinedAt = p.JoinedAt
                })
                .ToListAsync();
        }
        public async Task<TournamentResponseDto> UpdateTournamentAsync(int tournamentId, UpdateTournamentRequestDto request, int currentUserId)
        {
            var tournament = await _context.Tournaments
                .Include(t => t.Season)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
                throw new KeyNotFoundException("Tournament not found.");

            if (tournament.CreatedByUserId != currentUserId)
                throw new ForbiddenException("Only the tournament creator can update it.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Tournament name is required.");

            if (request.StartDate > request.EndDate)
                throw new ArgumentException(
                    "Start date cannot be after end date.");

            if (request.StartDate < tournament.Season.StartDate ||
                request.EndDate > tournament.Season.EndDate)
                throw new ArgumentException(
                    "Tournament dates must be within the selected season.");

            tournament.Name = request.Name.Trim();
            tournament.StartDate = request.StartDate;
            tournament.EndDate = request.EndDate;

            await _context.SaveChangesAsync();

            int participantCount = await _context.TournamentParticipants
                .CountAsync(p => p.TournamentId == tournamentId);

            return new TournamentResponseDto
            {
                Id = tournament.Id,
                Name = tournament.Name,
                SeasonId = tournament.SeasonId,
                StartDate = tournament.StartDate,
                EndDate = tournament.EndDate,
                CreatedByUserId = tournament.CreatedByUserId,
                ParticipantCount = participantCount
            };
        }

        public async Task LeaveTournamentAsync(int tournamentId, int currentUserId)
        {
            var tournament = await _context.Tournaments
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
                throw new KeyNotFoundException("Tournament not found.");

            if (tournament.CreatedByUserId == currentUserId)
                throw new InvalidOperationException(
                    "The tournament creator cannot leave.");

            var participant = await _context.TournamentParticipants
                .FirstOrDefaultAsync(p =>
                    p.TournamentId == tournamentId &&
                    p.UserId == currentUserId);

            if (participant == null)
                throw new KeyNotFoundException(
                    "You are not a participant of this tournament.");

            bool hasPredictions = await _context.Predictions
                .AnyAsync(p => p.TournamentParticipantId == participant.Id);

            if (hasPredictions)
                throw new InvalidOperationException(
                    "You cannot leave a tournament after submitting predictions.");

            _context.TournamentParticipants.Remove(participant);

            await _context.SaveChangesAsync();
        }

     
    }
}
