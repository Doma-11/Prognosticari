using FootballPredictionAPI.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FootballPredictionAPI.Tests.IntegrationTests
{
    [Collection("PostgreSQL")]
    public class DatabaseConnectionTests
    {
        private readonly PostgresFixture _fixture;

        public DatabaseConnectionTests(PostgresFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Database_CanConnect_ReturnsTrue()
        {
            // Arrange
            await using var dbContext = _fixture.CreateDbContext();

            // Act
            bool canConnect = await dbContext.Database.CanConnectAsync();

            // Assert
            Assert.True(canConnect);
        }

        [Fact]
        public async Task Database_HasNoPendingMigrations()
        {
            // Arrange
            await using var dbContext = _fixture.CreateDbContext();

            // Act
            var pendingMigrations =
                await dbContext.Database.GetPendingMigrationsAsync();

            // Assert
            Assert.Empty(pendingMigrations);
        }
    }
}