using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Contexts;
using SinformWcApi.Attributes;
using SinformWcApi.Middleware;
using SinformWcApi.Services.Impls;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;

namespace SinformWcApi.Features.Guesses;

public static class CreateOrUpdateGuessEndpoint
{
    public record FinalTable(
        [Required(ErrorMessage = "First place country is required.")]
        string First,
        
        [Required(ErrorMessage = "Second place country is required.")]
        string Second,
        
        string Third);

    public record Request(
        [Required(ErrorMessage = "SweepstakesId is required.")]
        Guid SweepstakesId,
        
        [Required(ErrorMessage = "FinalTable is required.")]
        FinalTable FinalTable);

    public record Response(Guid Id, Guid ParticipantId, string First, string Second, string Third);

    [Idempotent]
    public static void MapCreateOrUpdateGuessEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/guesses", async (
            Request request, 
            IGuessService guessService,
            IUserContext userContext) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var (guess, isNew) = await guessService.CreateOrUpdateAsync(
                userContext.UserId.Value,
                request.SweepstakesId,
                request.FinalTable.First,
                request.FinalTable.Second,
                request.FinalTable.Third);

            var response = new Response(guess.Id, guess.ParticipantId, guess.First, guess.Second, guess.Third);

            return isNew 
                ? Results.Created($"/api/v1/guesses/{guess.Id}", response)
                : Results.Ok(response);
        })
        .WithName("CreateOrUpdateGuess")
        .WithTags("Guesses")
        .RequireApiKey()
        .WithMetadata(new IdempotentAttribute());
    }
}
