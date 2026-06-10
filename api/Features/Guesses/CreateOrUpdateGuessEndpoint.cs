using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;
using SinformWcApi.Exceptions;
using SinformWcApi.Contexts;
using SinformWcApi.Attributes;
using SinformWcApi.Middleware;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
            AppDbContext dbContext,
            IUserContext userContext) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
            {
                return Results.Unauthorized();
            }

            var userId = userContext.UserId.Value;

            // Find participant record
            var participant = await dbContext.Participants
                .Include(p => p.Sweepstakes)
                .FirstOrDefaultAsync(p => p.SweepstakesId == request.SweepstakesId && p.UserId == userId);

            if (participant == null)
            {
                return Results.Json(new { message = "User is not a participant of this sweepstakes." }, statusCode: StatusCodes.Status403Forbidden);
            }

            // Verify guesses deadline
            if (DateTime.UtcNow > participant.Sweepstakes!.GuessesDeadline)
            {
                throw new DomainException("Guesses deadline has passed.");
            }

            // Verify third place requirement
            if (participant.Sweepstakes.IncludeThird && string.IsNullOrWhiteSpace(request.FinalTable.Third))
            {
                throw new DomainException("Third place country is required for this sweepstakes.");
            }

            // Verify unique countries in selection
            var countries = new List<string> { request.FinalTable.First.Trim(), request.FinalTable.Second.Trim() };
            if (participant.Sweepstakes.IncludeThird && !string.IsNullOrWhiteSpace(request.FinalTable.Third))
            {
                countries.Add(request.FinalTable.Third.Trim());
            }

            if (countries.Count != countries.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            {
                throw new DomainException("Countries in the final table must be unique.");
            }

            // Upsert Guess
            var guess = await dbContext.Guesses.FirstOrDefaultAsync(g => g.ParticipantId == participant.Id);
            var isNew = false;
            
            if (guess == null)
            {
                isNew = true;
                guess = new Guess
                {
                    ParticipantId = participant.Id,
                    First = request.FinalTable.First.Trim(),
                    Second = request.FinalTable.Second.Trim(),
                    Third = participant.Sweepstakes.IncludeThird ? request.FinalTable.Third.Trim() : string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                dbContext.Guesses.Add(guess);
            }
            else
            {
                guess.First = request.FinalTable.First.Trim();
                guess.Second = request.FinalTable.Second.Trim();
                guess.Third = participant.Sweepstakes.IncludeThird ? request.FinalTable.Third.Trim() : string.Empty;
                guess.UpdatedAt = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync();

            var response = new Response(guess.Id, guess.ParticipantId, guess.First, guess.Second, guess.Third);

            return isNew 
                ? Results.Created($"/guesses/{guess.Id}", response)
                : Results.Ok(response);
        })
        .WithName("CreateOrUpdateGuess")
        .WithTags("Guesses")
        .RequireApiKey()
        .WithMetadata(new IdempotentAttribute());
    }
}

