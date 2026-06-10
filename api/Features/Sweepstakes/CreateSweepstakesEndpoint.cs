using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Data;
using SinformWcApi.Entities;
using SinformWcApi.Contexts;
using SinformWcApi.Validation;
using SinformWcApi.Services.Impls;
using SinformWcApi.Services;
using SinformWcApi.Middleware;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;

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
        endpoints.MapPost("/sweepstakes", async (Request request, ISweepstakesService sweepstakesService, IUserContext userContext) =>
        endpoints.MapPost("/sweepstakes", async (Request request, AppDbContext dbContext, IUserContext userContext, IActiveSweepstakesRule activeSweepstakesRule) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var sweepstakes = await sweepstakesService.CreateAsync(
                userContext.UserId.Value,
                request.Name,
                request.Description,
                request.Phase,
                request.GuessesDeadline,
                request.QualifiedCount,
                request.IncludeThird);

            var response = sweepstakes.ToResponse();
            return Results.Created($"/api/v1/sweepstakes/{sweepstakes.Id}", response);
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
                Sweepstakes = sweepstakes,
                UserId = userId,
                TotalScore = 0,
                JoinedAt = DateTime.UtcNow
            };
            dbContext.Sweepstakes.Add(sweepstakes);
            dbContext.Participants.Add(participant);
            await dbContext.SaveChangesAsync();
            return Results.Created($"/sweepstakes/{sweepstakes.Id}", response);

        })
        .WithName("CreateSweepstakes")
        .WithTags("Sweepstakes")
        .RequireApiKey();
    }
}

public static class CreateSweepstakesMapper
{
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

public static class CreateSweepstakesMapper
{
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
