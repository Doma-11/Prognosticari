using FootballPredictionAPI.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Xunit;

namespace FootballPredictionAPI.Tests.Infrastructure
{
    public class PostgresFixture : IAsyncLifetime, IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public PostgresFixture()
        {
            string connectionString =
                Environment.GetEnvironmentVariable(
                    "PROGNOSTICARI_TEST_CONNECTION_STRING")
                ?? throw new InvalidOperationException(
                    "Test database connection string is missing.");

            var connectionStringBuilder =
                new NpgsqlConnectionStringBuilder(connectionString);

            if (connectionStringBuilder.Database != "prognosticari_test")
            {
                throw new InvalidOperationException(
                    "Integration tests must use the prognosticari_test database.");
            }

            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(connectionString)
                .Options;
        }

        public AppDbContext CreateDbContext()
        {
            return new AppDbContext(_options);
        }

        public Task<AppDbContext> CreateDbContextAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(CreateDbContext());
        }

        public async Task InitializeAsync()
        {
            await using var dbContext = CreateDbContext();

            await dbContext.Database.MigrateAsync();
        }

        public async Task ResetDatabaseAsync()
        {
            await using var dbContext = CreateDbContext();

            if (dbContext.Database.GetDbConnection().Database != "prognosticari_test")
            {
                throw new InvalidOperationException(
                    "Database cleanup is only allowed on the test database.");
            }

            await dbContext.Database.ExecuteSqlRawAsync(
                """
                TRUNCATE TABLE
                    public."Predictions",
                    public."TournamentParticipants",
                    public."Tournaments",
                    public."Matches",
                    public."Teams",
                    public."Seasons",
                    public."Leagues",
                    public."Users"
                RESTART IDENTITY;
                """);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}