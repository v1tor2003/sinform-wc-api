using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SinformWcApi.Data;
using SinformWcApi.Features.Auth;
using SinformWcApi.Features.Guesses;
using SinformWcApi.Features.Sweepstakes;
using SinformWcApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.MapGet("/health-check", () => Results.Ok("OK"))
   .WithName("HealthCheck");

app.Run();
