using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;
using SinformWcApi.Exceptions;
using SinformWcApi.Middleware;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Sweepstakes;

public static class JoinSweepstakesEndpoint
{
    public record Request(string InviteCode);
    public record Response(Guid ParticipantId, Guid SweepstakesId, string SweepstakesName);

    public static void MapJoinSweepstakesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/sweepstakes/join", async (Request request, HttpContext httpContext, AppDbContext dbContext) =>
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.InviteCode))
            {
                throw new DomainException("Invite code is required.");
            }

            var inviteCodeNormalized = request.InviteCode.Trim().ToUpper();

            // Find sweepstakes
            var sweepstakes = await dbContext.Sweepstakes
                .FirstOrDefaultAsync(s => s.InviteCode == inviteCodeNormalized);

            if (sweepstakes == null || !sweepstakes.IsActive)
            {
                return Results.NotFound("Sweepstakes not found or is closed.");
            }

            // Check if already a participant
            var isAlreadyParticipant = await dbContext.Participants
                .AnyAsync(p => p.SweepstakesId == sweepstakes.Id && p.UserId == userId);

            if (isAlreadyParticipant)
            {
                return Results.BadRequest("User is already a participant of this sweepstakes.");
            }

            // Add participant
            var participant = new Participant
            {
                SweepstakesId = sweepstakes.Id,
                UserId = userId,
                TotalScore = 0,
                JoinedAt = DateTime.UtcNow
            };

            dbContext.Participants.Add(participant);
            await dbContext.SaveChangesAsync();

            var response = new Response(participant.Id, sweepstakes.Id, sweepstakes.Name);
            return Results.Ok(response);
        })
        .WithName("JoinSweepstakes")
        .WithTags("Sweepstakes")
        .RequireApiKey();
    }
}
