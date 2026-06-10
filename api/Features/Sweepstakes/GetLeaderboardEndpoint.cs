using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Middleware;
using SinformWcApi.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Sweepstakes;

public static class GetLeaderboardEndpoint
{
    public record LeaderboardItem(int Position, string Name, int Score);
    public record Response(Guid SweepstakesId, LeaderboardItem[] Leaderboard);

    public static void MapGetLeaderboardEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sweepstakes/{id:guid}/leaderboard", async (Guid id, AppDbContext dbContext, IUserContext userContext) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var userId = userContext.UserId.Value;

            // Verify if sweepstakes exists
            var sweepstakesExists = await dbContext.Sweepstakes.AnyAsync(s => s.Id == id);
            if (!sweepstakesExists)
            {
                return Results.NotFound("Sweepstakes not found.");
            }

            // Verify if user is participant
            var isParticipant = await dbContext.Participants.AnyAsync(p => p.SweepstakesId == id && p.UserId == userId);
            if (!isParticipant)
            {
                return Results.Json(new { message = "User is not a participant of this sweepstakes." }, statusCode: StatusCodes.Status403Forbidden);
            }

            // Get leaderboard items
            var participants = await dbContext.Participants
                .Where(p => p.SweepstakesId == id)
                .Include(p => p.User)
                .OrderByDescending(p => p.TotalScore)
                .ThenBy(p => p.User!.Name)
                .ToListAsync();

            var items = participants
                .Select((p, idx) => new LeaderboardItem(idx + 1, p.User?.Name ?? "Unknown", p.TotalScore))
                .ToArray();

            var response = new Response(id, items);
            return Results.Ok(response);
        })
        .WithName("GetLeaderboard")
        .WithTags("Sweepstakes")
        .RequireApiKey()
        .CacheOutput(policy => policy
            .Expire(TimeSpan.FromMinutes(5))
            .SetVaryByRouteValue("id")
            .Tag("sb-leaderboard"));
    }
}

