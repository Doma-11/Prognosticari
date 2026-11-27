using FootballPredictionAPI.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FootballPredictionAPI.Tests.IntegrationTests
{
    [Collection("PostgreSQL")]
    public class DatabaseInfrastructureTests : IAsyncLifetime
    {
        private readonly PostgresFixture _fixture;

        public DatabaseInfrastructureTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        public async Task InitializeAsync()
        {
            await _fixture.ResetDatabaseAsync();
        }

        public async Task DisposeAsync()
        {
            await _fixture.ResetDatabaseAsync();
        }

        private async Task<int> SeedDatabaseAsync()
        {
            var league = TestDataFactory.CreateLeague();

            var season = TestDataFactory.CreateSeason(league);

            var homeTeam = TestDataFactory.CreateTeam(-1, "Arsenal");
            var awayTeam = TestDataFactory.CreateTeam(-2, "Chelsea");

            var match = TestDataFactory.CreateMatch(
                season, homeTeam, awayTeam);

            var user = TestDataFactory.CreateUser(1);

            var tournament = TestDataFactory.CreateTournament(
                season, user);

            var participant = TestDataFactory.CreateParticipant(
                tournament, user);

            var prediction = TestDataFactory.CreatePrediction(
                participant, match, 3, 0);

            await using var dbContext = _fixture.CreateDbContext();

            dbContext.Predictions.Add(prediction);

            await dbContext.SaveChangesAsync();

            return prediction.Id;
        }

        [Fact]
        public async Task Database_CanSeedRelatedEntities()
        {
            // Arrange
            int predictionId = await SeedDatabaseAsync();

            // Act
            await using var dbContext = _fixture.CreateDbContext();

            var prediction = await dbContext.Predictions
                .Include(p => p.TournamentParticipant)
                    .ThenInclude(tp => tp.User)
                .Include(p => p.TournamentParticipant)
                    .ThenInclude(tp => tp.Tournament)
                .Include(p => p.Match)
                    .ThenInclude(m => m.HomeTeam)
                .Include(p => p.Match)
                    .ThenInclude(m => m.AwayTeam)
                .Include(p => p.Match)
                    .ThenInclude(m => m.Season)
                        .ThenInclude(s => s.League)
                .SingleAsync(p => p.Id == predictionId);

            // Assert
            Assert.Equal(3, prediction.PredictedHomeGoals);
            Assert.Equal(0, prediction.PredictedAwayGoals);
            Assert.Null(prediction.PointsAwarded);

            Assert.Equal(
                "TestPlayer1",
                prediction.TournamentParticipant.User.UserName);

            Assert.Equal(
                "Test Tournament 1",
                prediction.TournamentParticipant.Tournament.Name);

            Assert.Equal("Arsenal", prediction.Match.HomeTeam.Name);
            Assert.Equal("Chelsea", prediction.Match.AwayTeam.Name);

            Assert.Equal(
                "Test League",
                prediction.Match.Season.League.Name);

            Assert.Equal(
                prediction.Match.SeasonId,
                prediction.TournamentParticipant.Tournament.SeasonId);

            Assert.Equal(
                prediction.TournamentParticipant.UserId,
                prediction.TournamentParticipant.Tournament.CreatedByUserId);

            Assert.Equal(
                0, prediction.TournamentParticipant.TotalPoints);
        }

        [Fact]
        public async Task Database_Reset_RemovesAllData()
        {
            // Arrange
            await SeedDatabaseAsync();

            await using (var dbContext = _fixture.CreateDbContext())
            {
                Assert.Equal(1, await dbContext.Predictions.CountAsync());
            }

            // Act
            await _fixture.ResetDatabaseAsync();

            // Assert
            await using var verificationContext =
                _fixture.CreateDbContext();

            Assert.Equal(0, await verificationContext.Predictions.CountAsync());
            Assert.Equal(0, await verificationContext.TournamentParticipants.CountAsync());
            Assert.Equal(0, await verificationContext.Tournaments.CountAsync());
            Assert.Equal(0, await verificationContext.Matches.CountAsync());
            Assert.Equal(0, await verificationContext.Teams.CountAsync());
            Assert.Equal(0, await verificationContext.Seasons.CountAsync());
            Assert.Equal(0, await verificationContext.Leagues.CountAsync());
            Assert.Equal(0, await verificationContext.Users.CountAsync());

            Assert.NotEmpty(
                await verificationContext.Database.GetAppliedMigrationsAsync());

            Assert.Empty(
                await verificationContext.Database.GetPendingMigrationsAsync());
        }
    }
}