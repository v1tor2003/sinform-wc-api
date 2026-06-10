using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Auth;

public static class RegisterEndpoint
{
    public record Request(string Name, string Email, string Password);
    public record Response(string Message);

    public static void MapRegisterEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/register", async (Request request, AppDbContext dbContext) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest("Name, Email, and Password are required.");
            }

            var emailNormalized = request.Email.Trim().ToLower();

            var exists = await dbContext.Users.AnyAsync(u => u.Email == emailNormalized);
            if (exists)
            {
                return Results.BadRequest("Email is already registered.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var apiKey = "usr_live_" + Guid.NewGuid().ToString("N");

            var newUser = new User
            {
                Name = request.Name,
                Email = emailNormalized,
                PasswordHash = passwordHash,
                ApiKey = apiKey,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(newUser);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/users/{newUser.Id}", new Response("User registered successfully."));
        })
        .WithName("RegisterUser")
        .WithTags("Auth");
    }
}
