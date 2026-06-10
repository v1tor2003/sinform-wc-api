using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Data;
using SinformWcApi.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Auth;

public static class RegisterEndpoint
{
    public record Request(
        [Required(ErrorMessage = "Name is required.")]
        string Name,
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        string Email,
        [Required(ErrorMessage = "Password is required.")]
        string Password);
    public record Response(string Message);

    public static void MapRegisterEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/register", async (Request request, AppDbContext dbContext) =>
        {
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

