using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SinformWcApi.Contexts;
using SinformWcApi.Attributes;
using SinformWcApi.Middleware;
using SinformWcApi.Services.Impls;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;
using System.Text.Json;
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
    public record FinalTable(string First, string Second, string Third);
    public record Request(Guid SweepstakesId, FinalTable FinalTable);
        

    public static void MapCreateOrUpdateGuessEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/guesses", async (
            Request request, 
            IGuessService guessService,
            IUserContext userContext) =>
        {
            if (!userContext.IsAuthenticated || !userContext.UserId.HasValue)
            HttpContext httpContext, 
            IDistributedCache cache) =>
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
                var cachedResponse = JsonSerializer.Deserialize<Response>(cachedJson);
                return Results.Ok(cachedResponse);
            // 2. Auth user retrieval
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var userId = userContext.UserId.Value;

            // Find participant record
            // 3. Find participant record
            var participant = await dbContext.Participants
                .Include(p => p.Sweepstakes)
                .FirstOrDefaultAsync(p => p.SweepstakesId == request.SweepstakesId && p.UserId == userId);
            if (participant == null)
            {
                return Results.Json(new { message = "User is not a participant of this sweepstakes." }, statusCode: StatusCodes.Status403Forbidden);
            }
            // Verify guesses deadline
            // 4. Verify guesses deadline
            if (DateTime.UtcNow > participant.Sweepstakes!.GuessesDeadline)
                throw new DomainException("Guesses deadline has passed.");
            // Verify third place requirement
            // 5. Input validation
            if (string.IsNullOrWhiteSpace(request.FinalTable.First) || 
                string.IsNullOrWhiteSpace(request.FinalTable.Second))
                throw new DomainException("First and Second place countries are required.");
            if (participant.Sweepstakes.IncludeThird && string.IsNullOrWhiteSpace(request.FinalTable.Third))
                throw new DomainException("Third place country is required for this sweepstakes.");
            // Verify unique countries in selection
            var countries = new List<string> { request.FinalTable.First.Trim(), request.FinalTable.Second.Trim() };
            if (participant.Sweepstakes.IncludeThird && !string.IsNullOrWhiteSpace(request.FinalTable.Third))
                countries.Add(request.FinalTable.Third.Trim());
            if (countries.Count != countries.Distinct(StringComparer.OrdinalIgnoreCase).Count())
                throw new DomainException("Countries in the final table must be unique.");
            // Upsert Guess
            // 6. Upsert Guess
            var guess = await dbContext.Guesses.FirstOrDefaultAsync(g => g.ParticipantId == participant.Id);
            var isNew = false;
            
            if (guess == null)
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
            else
                guess.First = request.FinalTable.First.Trim();
                guess.Second = request.FinalTable.Second.Trim();
                guess.Third = participant.Sweepstakes.IncludeThird ? request.FinalTable.Third.Trim() : string.Empty;
                guess.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            var (guess, isNew) = await guessService.CreateOrUpdateAsync(
                userContext.UserId.Value,
                request.SweepstakesId,
                request.FinalTable.First,
                request.FinalTable.Second,
                request.FinalTable.Third);

            var response = new Response(guess.Id, guess.ParticipantId, guess.First, guess.Second, guess.Third);
            // 7. Save to Cache
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            };
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(response), cacheOptions);

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

        .RequireApiKey();
