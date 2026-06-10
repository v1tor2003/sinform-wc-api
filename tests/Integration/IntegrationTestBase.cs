using Microsoft.AspNetCore.Hosting;
using SinformWcApi.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SinformWcApi.Entities;
using StackExchange.Redis;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SinformWcApi.Tests.Integration;

[Collection("SharedContainers")]
public class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly TestcontainersFixture Fixture;

    public IntegrationTestBase(TestcontainersFixture fixture)
    {
        Fixture = fixture;

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                // Override connection strings to point to the containers
                builder.UseSetting("ConnectionStrings:DefaultConnection", Fixture.PostgreSqlContainer.GetConnectionString());
                builder.UseSetting("ConnectionStrings:RedisConnection", Fixture.RedisContainer.GetConnectionString());

                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var dbContextDescriptors = services.Where(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList();
                    foreach (var desc in dbContextDescriptors)
                    {
                        services.Remove(desc);
                    }

                    // Add PostgreSQL DbContext pointing to our container
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseNpgsql(Fixture.PostgreSqlContainer.GetConnectionString());
                    });
                });
            });

        // Initialize schema (ensure created, then truncate to clean slate)
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            db.Database.ExecuteSqlRaw("TRUNCATE TABLE \"Guesses\", \"Participants\", \"Sweepstakes\", \"Users\", \"OfficialPhaseResults\" RESTART IDENTITY CASCADE;");
        }

        // Flush Redis to guarantee cache isolation before each test
        FlushRedis();

        Client = Factory.CreateClient();
    }

    private void FlushRedis()
    {
        var options = ConfigurationOptions.Parse(Fixture.RedisContainer.GetConnectionString());
        options.AllowAdmin = true;
        using var redis = ConnectionMultiplexer.Connect(options);
        var endpoints = redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            server.FlushAllDatabases();
        }
    }

    protected async Task<User> CreateUserAsync(string name, string email, string apiKey)
    {
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var user = new User
        {
            Name = name,
            Email = email,
            ApiKey = apiKey,
            PasswordHash = "dummyhash"
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    protected void SetApiKeyHeader(string apiKey)
    {
        Client.DefaultRequestHeaders.Remove("X-API-KEY");
        if (apiKey != null)
        {
            Client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        }
    }

    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T content, string? apiKey = null, string? idempotencyKey = null)
    {
        var targetUrl = url.StartsWith("/api/v1") ? url : $"/api/v1{url}";
        var request = new HttpRequestMessage(HttpMethod.Post, targetUrl)
        {
            Content = JsonContent.Create(content)
        };

        if (apiKey != null)
        {
            request.Headers.Add("X-API-KEY", apiKey);
        }

        if (idempotencyKey != null)
        {
            request.Headers.Add("Idempotency-Key", idempotencyKey);
        }

        return await Client.SendAsync(request);
    }

    protected async Task<HttpResponseMessage> GetAsync(string url, string? apiKey = null)
    {
        var targetUrl = url.StartsWith("/api/v1") ? url : $"/api/v1{url}";
        var request = new HttpRequestMessage(HttpMethod.Get, targetUrl);

        if (apiKey != null)
        {
            request.Headers.Add("X-API-KEY", apiKey);
        }

        return await Client.SendAsync(request);
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
        GC.SuppressFinalize(this);
    }
}
