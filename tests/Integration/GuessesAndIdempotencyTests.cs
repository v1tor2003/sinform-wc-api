using SinformWcApi.Features.Sweepstakes;
using SinformWcApi.Data;
using SinformWcApi.Features.Guesses;
using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace SinformWcApi.Tests.Integration;

public class GuessesAndIdempotencyTests : IntegrationTestBase
{
    public GuessesAndIdempotencyTests(TestcontainersFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task IT05_SubmitGuessAfterDeadline_ShouldReturn400BadRequest()
    {
        // Arrange
        var apiKey = "user-a-key";
        await CreateUserAsync("User A", "user.a@email.com", apiKey);

        var sweepPayload = new
        {
            Name = "Sweep 1",
            Description = "Description 1",
            Phase = "Group Stage",
            GuessesDeadline = DateTime.UtcNow.AddDays(5),
            QualifiedCount = 2,
            IncludeThird = false
        };

        // Create sweepstakes
        var createResponse = await PostJsonAsync("/sweepstakes", sweepPayload, apiKey);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var sweepResult = await createResponse.Content.ReadFromJsonAsync<CreateSweepstakesEndpoint.Response>();
        Assert.NotNull(sweepResult);

        // Manually update the deadline to the past in the database
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var sweep = await db.Sweepstakes.FindAsync(sweepResult.Id);
            Assert.NotNull(sweep);
            sweep.GuessesDeadline = DateTime.UtcNow.AddMinutes(-10);
            await db.SaveChangesAsync();
        }

        // Try to submit guess
        var guessPayload = new
        {
            SweepstakesId = sweepResult.Id,
            FinalTable = new
            {
                First = "Brazil",
                Second = "Argentina",
                Third = ""
            }
        };

        // Act
        var guessResponse = await PostJsonAsync("/guesses", guessPayload, apiKey, idempotencyKey: Guid.NewGuid().ToString());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, guessResponse.StatusCode);
    }

    [Fact]
    public async Task IT06_IdempotentGuesses_ShouldAvoidDuplicateInsertion()
    {
        // Arrange
        var apiKey = "user-a-key";
        await CreateUserAsync("User A", "user.a@email.com", apiKey);

        var sweepPayload = new
        {
            Name = "Sweep 1",
            Description = "Description 1",
            Phase = "Group Stage",
            GuessesDeadline = DateTime.UtcNow.AddDays(5),
            QualifiedCount = 2,
            IncludeThird = false
        };

        // Create sweepstakes (User A is automatically a participant)
        var createResponse = await PostJsonAsync("/sweepstakes", sweepPayload, apiKey);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var sweepResult = await createResponse.Content.ReadFromJsonAsync<CreateSweepstakesEndpoint.Response>();
        Assert.NotNull(sweepResult);

        var idempotencyKey = "123e4567-e89b-12d3-a456-426614174000";
        var guessPayload = new
        {
            SweepstakesId = sweepResult.Id,
            FinalTable = new
            {
                First = "Brazil",
                Second = "Argentina",
                Third = ""
            }
        };

        // Act
        // Step 1: Submit first guess
        var response1 = await PostJsonAsync("/guesses", guessPayload, apiKey, idempotencyKey);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        var guessResult1 = await response1.Content.ReadFromJsonAsync<CreateOrUpdateGuessEndpoint.Response>();
        Assert.NotNull(guessResult1);

        // Verify it was stored in Db
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var guessesCount = await db.Guesses.CountAsync(g => g.ParticipantId == guessResult1.ParticipantId);
            Assert.Equal(1, guessesCount);
        }

        // Step 2: Submit exactly the same payload and headers
        var response2 = await PostJsonAsync("/guesses", guessPayload, apiKey, idempotencyKey);
        
        // Assert
        Assert.True(response2.StatusCode == HttpStatusCode.OK || response2.StatusCode == HttpStatusCode.Created);

        // Verify count of guesses in database is still exactly 1
        using (var scope = Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var guessesCount = await db.Guesses.CountAsync(g => g.ParticipantId == guessResult1.ParticipantId);
            Assert.Equal(1, guessesCount);
        }
    }
}
