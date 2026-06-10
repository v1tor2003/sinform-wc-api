using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Data;
using SinformWcApi.Entities;
using SinformWcApi.Exceptions;
using SinformWcApi.Middleware;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

using SinformWcApi.Services;

namespace SinformWcApi.Features.Sweepstakes;

public static class CreateSweepstakesEndpoint
{
    public record Request(
        string Name,
        string Description,
        string Phase,
        DateTime GuessesDeadline,
        int QualifiedCount,
        bool IncludeThird);

    public record Response(
        Guid Id,
        string Name,
        string Description,
        string Phase,
        string InviteCode,
        DateTime GuessesDeadline,
        int QualifiedCount,
        bool IncludeThird,
        bool IsActive);

    public static void MapCreateSweepstakesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/sweepstakes", async (Request request, HttpContext httpContext, AppDbContext dbContext, IActiveSweepstakesRule activeSweepstakesRule) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            // Input validations
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new DomainException("Sweepstakes name is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Phase))
            {
                throw new DomainException("Sweepstakes phase is required.");
            }

            if (request.QualifiedCount < 1)
            {
                throw new DomainException("Qualified count must be at least 1.");
            }

            if (request.GuessesDeadline.ToUniversalTime() <= DateTime.UtcNow)
            {
                throw new DomainException("Guesses deadline must be in the future.");
            }

            // Rule: 1 active sweepstakes per creator
            var activeCount = await dbContext.Sweepstakes.CountAsync(s => s.CreatorId == userId && s.IsActive);
            activeSweepstakesRule.Validate(activeCount);

            // Generate invite code
            string inviteCode;
            do
            {
                inviteCode = GenerateInviteCode();
            } while (await dbContext.Sweepstakes.AnyAsync(s => s.InviteCode == inviteCode));

            var sweepstakes = new Entities.Sweepstakes
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                Phase = request.Phase,
                InviteCode = inviteCode,
                CreatorId = userId,
                QualifiedCount = request.QualifiedCount,
                IncludeThird = request.IncludeThird,
                GuessesDeadline = request.GuessesDeadline.ToUniversalTime(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var participant = new Participant
            {
                Sweepstakes = sweepstakes,
                UserId = userId,
                TotalScore = 0,
                JoinedAt = DateTime.UtcNow
            };

            dbContext.Sweepstakes.Add(sweepstakes);
            dbContext.Participants.Add(participant);
            await dbContext.SaveChangesAsync();

            var response = new Response(
                sweepstakes.Id,
                sweepstakes.Name,
                sweepstakes.Description,
                sweepstakes.Phase,
                sweepstakes.InviteCode,
                sweepstakes.GuessesDeadline,
                sweepstakes.QualifiedCount,
                sweepstakes.IncludeThird,
                sweepstakes.IsActive);

            return Results.Created($"/sweepstakes/{sweepstakes.Id}", response);
        })
        .WithName("CreateSweepstakes")
        .WithTags("Sweepstakes")
        .RequireApiKey();
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new char[6];
        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }
        return new string(code);
    }
}
