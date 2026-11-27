using Xunit;

namespace FootballPredictionAPI.Tests.Infrastructure
{
    [CollectionDefinition("PostgreSQL")]
    public class PostgresTestCollection
        : ICollectionFixture<PostgresFixture>
    {
    }
}