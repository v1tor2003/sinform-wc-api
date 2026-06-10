using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using SinformWcApi;

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
app.UseOutputCache();

app.MapGet("/health-check", () => Results.Ok("OK"))
   .WithName("HealthCheck");

app.Run();
