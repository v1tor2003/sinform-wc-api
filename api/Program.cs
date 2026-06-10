using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SinformWcApi.Data;
using SinformWcApi.Features.Auth;
using SinformWcApi.Features.Guesses;
using SinformWcApi.Features.OfficialResults;
using SinformWcApi.Features.Sweepstakes;
using SinformWcApi.Middleware;
using SinformWcApi.Workers;
using SinformWcApi.Services.Interfaces;
using SinformWcApi.Services.Impls;
using SinformWcApi.Contexts;
using SinformWcApi.Repositories.Interfaces;
using SinformWcApi.Repositories.Impls;
using SinformWcApi.Factories;
using SinformWcApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add database context
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Configure caching services
builder.Services.AddOutputCache();
builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// Register scoring strategy and rules
builder.Services.AddSingleton<IScoringStrategy, DefaultScoringStrategy>();
builder.Services.AddSingleton<IActiveSweepstakesRule, ActiveSweepstakesRule>();
// Register user context
builder.Services.AddScoped<UserContext>();
builder.Services.AddScoped<IUserContext>(sp => sp.GetRequiredService<UserContext>());
// Register native validation
builder.Services.AddValidation();
// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISweepstakesRepository, SweepstakesRepository>();
builder.Services.AddScoped<IParticipantRepository, ParticipantRepository>();
builder.Services.AddScoped<IGuessRepository, GuessRepository>();
builder.Services.AddScoped<IOfficialResultRepository, OfficialResultRepository>();
// Register factories
builder.Services.AddSingleton<GuessFactory>();
// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISweepstakesService, SweepstakesService>();
builder.Services.AddScoped<IGuessService, GuessService>();
builder.Services.AddScoped<IOfficialResultService, OfficialResultService>();

// Register background worker
builder.Services.AddHostedService<SweepstakesProcessingWorker>();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Global exception handling middleware (first in pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Custom API Key Authentication middleware
app.UseApiKeyAuthentication();

// Attach User Context and check Idempotency after auth
app.UseMiddleware<AttachUserContextMiddleware>();
app.UseMiddleware<IdempotencyActionMiddleware>();

app.UseOutputCache();

// Register endpoints
app.MapRegisterEndpoint();
app.MapLoginEndpoint();
app.MapCreateSweepstakesEndpoint();
app.MapGetSweepstakesEndpoint();
app.MapGetSweepstakesMetadataEndpoint();
app.MapJoinSweepstakesEndpoint();
app.MapCreateOrUpdateGuessEndpoint();
app.MapGetLeaderboardEndpoint();
app.MapCreateOfficialResultEndpoint();
// Register api/v1 route group and map endpoints under it
var apiV1 = app.MapGroup("api/v1");
apiV1.MapRegisterEndpoint();
apiV1.MapLoginEndpoint();
apiV1.MapCreateSweepstakesEndpoint();
apiV1.MapJoinSweepstakesEndpoint();
apiV1.MapCreateOrUpdateGuessEndpoint();
apiV1.MapGetLeaderboardEndpoint();
apiV1.MapCreateOfficialResultEndpoint();

app.MapGet("/health-check", () => Results.Ok("OK"))
   .WithName("HealthCheck");

app.Run();

public partial class Program { }

