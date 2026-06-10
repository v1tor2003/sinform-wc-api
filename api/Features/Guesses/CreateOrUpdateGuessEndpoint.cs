using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using SinformWcApi.Entities;
using SinformWcApi.Exceptions;
using SinformWcApi.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace SinformWcApi.Features.Guesses;

public static class CreateOrUpdateGuessEndpoint
{
    public record FinalTable(string First, string Second, string Third);
    public record Request(Guid SweepstakesId, FinalTable FinalTable);
    public record Response(Guid Id, Guid ParticipantId, string First, string Second, string Third);

    public static void MapCreateOrUpdateGuessEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/guesses", async (
            Request request, 
            HttpContext httpContext, 
            AppDbContext dbContext,
            IDistributedCache cache) =>
        {
            // 1. Idempotency validation
            if (!httpContext.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKeyValues) || 
                string.IsNullOrWhiteSpace(idempotencyKeyValues.ToString()))
            {
                throw new DomainException("Idempotency-Key header is required.");
            }

            var idempotencyKey = idempotencyKeyValues.ToString();
            var cacheKey = $"idemp:{idempotencyKey}";

            // Check if request was already processed
            var cachedJson = await cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                var cachedResponse = JsonSerializer.Deserialize<Response>(cachedJson);
                return Results.Ok(cachedResponse);
            }

            // 2. Auth user retrieval
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            // 3. Find participant record
            var participant = await dbContext.Participants
                .Include(p => p.Sweepstakes)
                .FirstOrDefaultAsync(p => p.SweepstakesId == request.SweepstakesId && p.UserId == userId);

            if (participant == null)
            {
                return Results.Json(new { message = "User is not a participant of this sweepstakes." }, statusCode: StatusCodes.Status403Forbidden);
            }

            // 4. Verify guesses deadline
            if (DateTime.UtcNow > participant.Sweepstakes!.GuessesDeadline)
            {
                throw new DomainException("Guesses deadline has passed.");
            }

            // 5. Input validation
            if (string.IsNullOrWhiteSpace(request.FinalTable.First) || 
                string.IsNullOrWhiteSpace(request.FinalTable.Second))
            {
                throw new DomainException("First and Second place countries are required.");
            }

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

            // 6. Upsert Guess
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

            // 7. Save to Cache
            var response = new Response(guess.Id, guess.ParticipantId, guess.First, guess.Second, guess.Third);
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions);

            return isNew 
                ? Results.Created($"/guesses/{guess.Id}", response)
                : Results.Ok(response);
        })
        .WithName("CreateOrUpdateGuess")
        .WithTags("Guesses")
        .RequireApiKey();
    }
}
