using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Contexts;
using SinformWcApi.Middleware;
using SinformWcApi.Services.Impls;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;

namespace SinformWcApi.Features.Sweepstakes;

public static class JoinSweepstakesEndpoint
{
    public record Request(
        [Required(ErrorMessage = "Invite code is required.")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Invite code must be exactly 6 characters.")]
        string InviteCode);

    public record Response(Guid ParticipantId, Guid SweepstakesId, string SweepstakesName);

    public static void MapJoinSweepstakesEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/sweepstakes/join", async (Request request, ISweepstakesService sweepstakesService, IUserContext userContext) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var participant = await sweepstakesService.JoinAsync(userContext.UserId.Value, request.InviteCode);
            var response = new Response(participant.Id, participant.SweepstakesId, participant.Sweepstakes!.Name);
            return Results.Ok(response);
        })
        .WithName("JoinSweepstakes")
        .WithTags("Sweepstakes")
        .RequireApiKey();
    }
}
