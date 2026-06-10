using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Middleware;
using SinformWcApi.Contexts;
using SinformWcApi.Services.Impls;
using System;
using System.Linq;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SinformWcApi.Features.Sweepstakes;

public static class GetLeaderboardEndpoint
{
    public record LeaderboardItem(int Position, string Name, int Score);
    public record Response(Guid SweepstakesId, LeaderboardItem[] Leaderboard);

    public static void MapGetLeaderboardEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sweepstakes/{id:guid}/leaderboard", async (Guid id, ISweepstakesService sweepstakesService, IUserContext userContext) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
        endpoints.MapGet("/sweepstakes/{id:guid}/leaderboard", async (Guid id, HttpContext httpContext, AppDbContext dbContext) =>
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var participants = await sweepstakesService.GetLeaderboardAsync(id, userContext.UserId.Value);

            var items = participants
                .OrderByDescending(p => p.TotalScore)
                .ThenBy(p => p.User!.Name)
            // Verify if sweepstakes exists
            var sweepstakesExists = await dbContext.Sweepstakes.AnyAsync(s => s.Id == id);
            if (!sweepstakesExists)
            {
                return Results.NotFound("Sweepstakes not found.");
            }
            // Verify if user is participant
            var isParticipant = await dbContext.Participants.AnyAsync(p => p.SweepstakesId == id && p.UserId == userId);
            if (!isParticipant)
                return Results.Json(new { message = "User is not a participant of this sweepstakes." }, statusCode: StatusCodes.Status403Forbidden);
            // Get leaderboard items
            var participants = await dbContext.Participants
                .Where(p => p.SweepstakesId == id)
                .Include(p => p.User)
                .ToListAsync();
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
