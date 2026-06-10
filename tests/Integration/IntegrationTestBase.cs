using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SinformWcApi.Entities;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SinformWcApi.Tests.Integration;

public class IntegrationTestBase : IDisposable
{
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly HttpClient Client;
    protected readonly Mock<IOutputCacheStore> MockCacheStore;
    private readonly SqliteConnection _connection;

    public IntegrationTestBase()
    {
        // Setup SQLite in-memory database connection
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        MockCacheStore = new Mock<IOutputCacheStore>();
        MockCacheStore
            .Setup(c => c.EvictByTagAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var dbContextDescriptors = services.Where(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList();
                    foreach (var desc in dbContextDescriptors)
                    {
                        services.Remove(desc);
                    }

                    // Add SQLite DbContext
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlite(_connection);
                    });

                    // Remove existing IDistributedCache registration
                    var distributedCacheDescriptors = services.Where(
                        d => d.ServiceType == typeof(IDistributedCache)).ToList();
                    foreach (var desc in distributedCacheDescriptors)
                    {
                        services.Remove(desc);
                    }

                    // Add Memory cache to replace Redis IDistributedCache
                    services.AddDistributedMemoryCache();

                    // Replace IOutputCacheStore with our mock
                    var cacheDescriptors = services.Where(
                        d => d.ServiceType == typeof(IOutputCacheStore)).ToList();
                    foreach (var desc in cacheDescriptors)
                    {
                        services.Remove(desc);
                    }
                    services.AddSingleton<IOutputCacheStore>(MockCacheStore.Object);
                });
            });

        // Initialize schema
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        Client = Factory.CreateClient();
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
        var request = new HttpRequestMessage(HttpMethod.Post, url)
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
        var request = new HttpRequestMessage(HttpMethod.Get, url);

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
        _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
