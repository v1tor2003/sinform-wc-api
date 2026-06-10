using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Data;
using SinformWcApi.Exceptions;
using SinformWcApi.Middleware;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Sweepstakes;

public static class GetSweepstakesEndpoint
{
    public record Response(
        Guid Id,
        string Name,
        string Description,
        string Phase,
        string InviteCode,
        DateTime GuessesDeadline,
        int QualifiedCount,
        bool IncludeThird,
        bool IsActive,
        string[] EligibleTeams);

    public static void MapGetSweepstakesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sweepstakes/{id:guid}", async (Guid id, HttpContext httpContext, AppDbContext dbContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var sweepstakes = await dbContext.Sweepstakes
                .Include(s => s.Participants)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sweepstakes is null)
                throw new NotFoundException($"Sweepstakes {id} not found.");

            var isParticipant = sweepstakes.Participants.Any(p => p.UserId == userId);
            var isCreator = sweepstakes.CreatorId == userId;
            if (!isParticipant && !isCreator)
                throw new ForbiddenException("You are not a participant of this sweepstakes.");

            var eligibleTeams = WorldCupData.GetTeamsForPhase(sweepstakes.Phase);

            var response = new Response(
                sweepstakes.Id,
                sweepstakes.Name,
                sweepstakes.Description,
                sweepstakes.Phase,
                sweepstakes.InviteCode,
                sweepstakes.GuessesDeadline,
                sweepstakes.QualifiedCount,
                sweepstakes.IncludeThird,
                sweepstakes.IsActive,
                eligibleTeams);

            return Results.Ok(response);
        })
        .WithName("GetSweepstakes")
        .WithTags("Sweepstakes")
        .RequireApiKey();
    }
}
