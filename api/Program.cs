using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SinformWcApi;
using SinformWcApi.Features.Auth;
using SinformWcApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure caching services
builder.Services.AddOutputCache();

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

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

app.MapGet("/health-check", () => Results.Ok("OK"))
   .WithName("HealthCheck");

app.Run();
