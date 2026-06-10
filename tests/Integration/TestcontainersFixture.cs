using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using System.Threading.Tasks;
using Xunit;

namespace SinformWcApi.Tests.Integration;

public class TestcontainersFixture : IAsyncLifetime
{
    public PostgreSqlContainer PostgreSqlContainer { get; }
    public RedisContainer RedisContainer { get; }

    public TestcontainersFixture()
    {
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .Build();

        RedisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            PostgreSqlContainer.StartAsync(),
            RedisContainer.StartAsync()
        );
    }

    public async Task DisposeAsync()
    {
        await Task.WhenAll(
            PostgreSqlContainer.DisposeAsync().AsTask(),
            RedisContainer.DisposeAsync().AsTask()
        );
    }
}

[CollectionDefinition("SharedContainers")]
public class SharedContainersCollection : ICollectionFixture<TestcontainersFixture>
{
}
