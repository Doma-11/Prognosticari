using FootballPredictionAPI.Enums;
using FootballPredictionAPI.Services;
using FootballPredictionAPI.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FootballPredictionAPI.Tests.IntegrationTests
{
    [Collection("PostgreSQL")]
    public class ScoringServiceTests : IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;
        private readonly ScoringService _scoringService;

        public ScoringServiceTests(PostgresFixture fixture)
        {
            _fixture = fixture;

            _scoringService = new ScoringService(
                fixture,
                NullLogger<ScoringService>.Instance);
        }

        public async Task InitializeAsync()
        {
            await _fixture.ResetDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            await _fixture.ResetDatabaseAsync();
        }

        private async Task<(int MatchId, int PredictionId)> SeedPredictionAsync(
            int predictedHomeGoals,
            int predictedAwayGoals,
            MatchStatus status = MatchStatus.Finished,
            int? homeScore = 3,
            int? awayScore = 0)
        {
            var league = TestDataFactory.CreateLeague();
            var season = TestDataFactory.CreateSeason(league);

            var homeTeam = TestDataFactory.CreateTeam(-1, "Arsenal");
            var awayTeam = TestDataFactory.CreateTeam(-2, "Chelsea");

            var match = TestDataFactory.CreateMatch(
                season,
                homeTeam,
                awayTeam,
                status: status,
                homeScore: homeScore,
                awayScore: awayScore);

            var user = TestDataFactory.CreateUser(1);

            var tournament = TestDataFactory.CreateTournament(
                season, user);

            var participant = TestDataFactory.CreateParticipant(
                tournament, user);

            var prediction = TestDataFactory.CreatePrediction(
                participant,
                match,
                predictedHomeGoals,
                predictedAwayGoals);

            await using var dbContext = _fixture.CreateDbContext();

            dbContext.Predictions.Add(prediction);

            await dbContext.SaveChangesAsync();

            return (match.Id, prediction.Id);
        }

        private async Task AssertPointsAsync(
            int predictionId,
            int? expectedPredictionPoints,
            int expectedParticipantPoints)
        {
            await using var dbContext = _fixture.CreateDbContext();

            var prediction = await dbContext.Predictions
                .Include(p => p.TournamentParticipant)
                .SingleAsync(p => p.Id == predictionId);

            Assert.Equal(
                expectedPredictionPoints,
                prediction.PointsAwarded);

            Assert.Equal(
                expectedParticipantPoints,
                prediction.TournamentParticipant.TotalPoints);
        }

        [Theory]
        [InlineData(3, 0, 4)]
        [InlineData(2, 0, 1)]
        [InlineData(0, 3, 0)]
        public async Task ScoreMatch_FinishedMatch_AwardsCorrectPoints(
            int predictedHomeGoals,
            int predictedAwayGoals,
            int expectedPoints)
        {
            // Arrange
            var data = await SeedPredictionAsync(
                predictedHomeGoals,
                predictedAwayGoals);

            // Act
            await _scoringService.ScoreMatchAsync(data.MatchId);

            // Assert
            await AssertPointsAsync(
                data.PredictionId,
                expectedPoints,
                expectedPoints);
        }

        [Fact]
        public async Task ScoreMatch_UnfinishedMatch_DoesNotAwardPoints()
        {
            // Arrange
            var data = await SeedPredictionAsync(
                3, 0,
                status: MatchStatus.Live);

            // Act
            await _scoringService.ScoreMatchAsync(data.MatchId);

            // Assert
            await AssertPointsAsync(
                data.PredictionId, null, 0);
        }

        [Fact]
        public async Task ScoreMatch_MissingResult_DoesNotAwardPoints()
        {
            // Arrange
            var data = await SeedPredictionAsync(
                3, 0,
                homeScore: null,
                awayScore: null);

            // Act
            await _scoringService.ScoreMatchAsync(data.MatchId);

            // Assert
            await AssertPointsAsync(
                data.PredictionId, null, 0);
        }

        [Fact]
        public async Task ScoreMatch_CalledMultipleTimes_DoesNotDuplicatePoints()
        {
            // Arrange
            var data = await SeedPredictionAsync(3, 0);

            // Act
            await _scoringService.ScoreMatchAsync(data.MatchId);

            await AssertPointsAsync(
                data.PredictionId, 4, 4);

            await _scoringService.ScoreMatchAsync(data.MatchId);
            await _scoringService.ScoreMatchAsync(data.MatchId);

            // Assert
            await AssertPointsAsync(
                data.PredictionId, 4, 4);
        }

        [Fact]
        public async Task ScoreMatch_MultipleTournaments_CalculatesRarityIndependently()
        {
            // Arrange
            var league = TestDataFactory.CreateLeague();
            var season = TestDataFactory.CreateSeason(league);

            var homeTeam = TestDataFactory.CreateTeam(-1, "Arsenal");
            var awayTeam = TestDataFactory.CreateTeam(-2, "Chelsea");

            var match = TestDataFactory.CreateMatch(
                season, homeTeam, awayTeam);

            var user = TestDataFactory.CreateUser(1);

            var tournamentA = TestDataFactory.CreateTournament(
                season, user, 1);

            var tournamentB = TestDataFactory.CreateTournament(
                season, user, 2);

            var participantA = TestDataFactory.CreateParticipant(
                tournamentA, user);

            var participantB = TestDataFactory.CreateParticipant(
                tournamentB, user);

            var predictionA = TestDataFactory.CreatePrediction(
                participantA, match, 3, 0);

            var predictionB = TestDataFactory.CreatePrediction(
                participantB, match, 3, 0);

            await using (var dbContext = _fixture.CreateDbContext())
            {
                dbContext.Predictions.Add(predictionA);
                dbContext.Predictions.Add(predictionB);

                for (int i = 2; i <= 10; i++)
                {
                    var otherUser = TestDataFactory.CreateUser(i);

                    var otherParticipant =
                        TestDataFactory.CreateParticipant(
                            tournamentB, otherUser);

                    var otherPrediction =
                        TestDataFactory.CreatePrediction(
                            otherParticipant, match, 0, 3);

                    dbContext.Predictions.Add(otherPrediction);
                }

                await dbContext.SaveChangesAsync();
            }

            // Act
            await _scoringService.ScoreMatchAsync(match.Id);

            // Assert
            await AssertPointsAsync(predictionA.Id, 4, 4);
            await AssertPointsAsync(predictionB.Id, 6, 6);

            await using var verificationContext =
                _fixture.CreateDbContext();

            var incorrectPredictions =
                await verificationContext.Predictions
                    .Include(p => p.TournamentParticipant)
                    .Where(p => p.PredictedHomeGoals == 0 &&
                                p.PredictedAwayGoals == 3)
                    .ToListAsync();

            Assert.Equal(9, incorrectPredictions.Count);

            Assert.All(incorrectPredictions, prediction =>
            {
                Assert.Equal(0, prediction.PointsAwarded);

                Assert.Equal(
                    0, prediction.TournamentParticipant.TotalPoints);
            });
        }
    }
}