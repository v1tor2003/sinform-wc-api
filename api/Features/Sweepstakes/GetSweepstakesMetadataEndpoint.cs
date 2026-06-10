using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Data;
using SinformWcApi.Middleware;
using System.Collections.Generic;

namespace SinformWcApi.Features.Sweepstakes;

public static class GetSweepstakesMetadataEndpoint
{
    public record Response(
        string[] Phases,
        Dictionary<string, string[]> Groups,
        string[] AllTeams);

    public static void MapGetSweepstakesMetadataEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/sweepstakes/metadata", () =>
        {
            var response = new Response(
                WorldCupData.Phases,
                WorldCupData.Groups,
                WorldCupData.AllTeams);

            return Results.Ok(response);
        })
        .WithName("GetSweepstakesMetadata")
        .WithTags("Sweepstakes")
        .RequireApiKey();
    }
}
