using FootballPredictionAPI.Models;
using FootballPredictionAPI.Enums;

namespace FootballPredictionAPI.Tests.Infrastructure
{
    public static class TestDataFactory
    {
        public static League CreateLeague(
            int externalApiId = -1,
            string name = "Test League")
        {
            return new League
            {
                Name = name,
                ExternalApiId = externalApiId
            };
        }

        public static Season CreateSeason(
            League league,
            int externalApiId = -1)
        {
            return new Season
            {
                Name = "2026/27",
                StartDate = new DateOnly(2026, 8, 1),
                EndDate = new DateOnly(2027, 5, 31),
                ExternalApiId = externalApiId,
                League = league
            };
        }

        public static Team CreateTeam(
            int externalApiId,
            string name)
        {
            return new Team
            {
                Name = name,
                ExternalApiId = externalApiId
            };
        }

        public static User CreateUser(int number)
        {
            return new User
            {
                UserName = $"TestPlayer{number}",
                Email = $"testplayer{number}@example.com",
                Role = UserRole.Player,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Match CreateMatch(
            Season season,
            Team homeTeam,
            Team awayTeam,
            int externalApiId = -1,
            MatchStatus status = MatchStatus.Finished,
            int? homeScore = 3,
            int? awayScore = 0)
        {
            return new Match
            {
                Season = season,
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                ExternalApiId = externalApiId,
                MatchDate = new DateTime(
                    2026, 10, 15, 20, 0, 0, DateTimeKind.Utc),
                Status = status,
                HomeTeamScore = homeScore,
                AwayTeamScore = awayScore
            };
        }

        public static Tournament CreateTournament(
    Season season,
    User creator,
    int number = 1)
        {
            return new Tournament
            {
                Name = $"Test Tournament {number}",
                Season = season,
                CreatedByUser = creator,
                StartDate = season.StartDate,
                EndDate = season.EndDate,
                InvitationCode = $"TEST{number:D4}"
            };
        }

        public static TournamentParticipant CreateParticipant(
            Tournament tournament,
            User user,
            int totalPoints = 0)
        {
            return new TournamentParticipant
            {
                Tournament = tournament,
                User = user,
                TotalPoints = totalPoints,
                JoinedAt = DateTime.UtcNow
            };
        }

        public static Prediction CreatePrediction(
            TournamentParticipant participant,
            Match match,
            int predictedHomeGoals,
            int predictedAwayGoals,
            int? pointsAwarded = null)
        {
            return new Prediction
            {
                TournamentParticipant = participant,
                Match = match,
                PredictedHomeGoals = predictedHomeGoals,
                PredictedAwayGoals = predictedAwayGoals,
                PointsAwarded = pointsAwarded,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}