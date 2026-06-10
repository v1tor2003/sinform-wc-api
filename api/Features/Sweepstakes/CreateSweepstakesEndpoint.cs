using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Data;
using SinformWcApi.Entities;
using SinformWcApi.Contexts;
using SinformWcApi.Validation;
using SinformWcApi.Services;
using SinformWcApi.Middleware;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Sweepstakes;

public static class CreateSweepstakesEndpoint
{
    public record Request(
        [Required(ErrorMessage = "Sweepstakes name is required.")]
        [StringLength(150, ErrorMessage = "Sweepstakes name cannot exceed 150 characters.")]
        string Name,

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        string Description,

        [Required(ErrorMessage = "Sweepstakes phase is required.")]
        [StringLength(50, ErrorMessage = "Phase cannot exceed 50 characters.")]
        string Phase,

        [Required(ErrorMessage = "Guesses deadline is required.")]
        [FutureDate(ErrorMessage = "Guesses deadline must be in the future.")]
        DateTime GuessesDeadline,

        [Range(1, int.MaxValue, ErrorMessage = "Qualified count must be at least 1.")]
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
        endpoints.MapPost("/sweepstakes", async (Request request, AppDbContext dbContext, IUserContext userContext, IActiveSweepstakesRule activeSweepstakesRule) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var userId = userContext.UserId.Value;

            // Rule: 1 active sweepstakes per creator
            var activeCount = await dbContext.Sweepstakes.CountAsync(s => s.CreatorId == userId && s.IsActive);
            activeSweepstakesRule.Validate(activeCount);

            // Generate invite code
            string inviteCode;
            do
            {
                inviteCode = GenerateInviteCode();
            } while (await dbContext.Sweepstakes.AnyAsync(s => s.InviteCode == inviteCode));

            var sweepstakes = request.ToEntity(inviteCode, userId);

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

            var response = sweepstakes.ToResponse();

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

public static class CreateSweepstakesMapper
{
    public static Entities.Sweepstakes ToEntity(this CreateSweepstakesEndpoint.Request request, string inviteCode, Guid creatorId)
    {
        return new Entities.Sweepstakes
        {
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            Phase = request.Phase,
            InviteCode = inviteCode,
            CreatorId = creatorId,
            QualifiedCount = request.QualifiedCount,
            IncludeThird = request.IncludeThird,
            GuessesDeadline = request.GuessesDeadline.ToUniversalTime(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateSweepstakesEndpoint.Response ToResponse(this Entities.Sweepstakes sweepstakes)
    {
        return new CreateSweepstakesEndpoint.Response(
            sweepstakes.Id,
            sweepstakes.Name,
            sweepstakes.Description,
            sweepstakes.Phase,
            sweepstakes.InviteCode,
            sweepstakes.GuessesDeadline,
            sweepstakes.QualifiedCount,
            sweepstakes.IncludeThird,
            sweepstakes.IsActive);
    }
}

