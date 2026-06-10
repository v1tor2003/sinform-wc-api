using SinformWcApi.Features.Sweepstakes;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace SinformWcApi.Tests;

public class SweepstakesFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task IT03_PreventSecondActiveSweepstakes_ShouldReturn400BadRequest()
    {
        // Arrange
        var apiKey = "user-a-key";
        await CreateUserAsync("User A", "user.a@email.com", apiKey);

        var firstSweep = new
        {
            Name = "Sweep 1",
            Description = "Description 1",
            Phase = "Group Stage",
            GuessesDeadline = DateTime.UtcNow.AddDays(5),
            QualifiedCount = 2,
            IncludeThird = false
        };

        var secondSweep = new
        {
            Name = "Sweep 2",
            Description = "Description 2",
            Phase = "Group Stage",
            GuessesDeadline = DateTime.UtcNow.AddDays(10),
            QualifiedCount = 2,
            IncludeThird = false
        };

        // Act
        // Step 1: Create first sweepstakes successfully (201 Created)
        var response1 = await PostJsonAsync("/sweepstakes", firstSweep, apiKey);
        Assert.Equal(HttpStatusCode.Created, response1.StatusCode);

        // Step 2: Attempt to create second sweepstakes under same creator (400 Bad Request)
        var response2 = await PostJsonAsync("/sweepstakes", secondSweep, apiKey);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task IT04_JoinSweepstakesWithInviteCode_ShouldSucceedAndAssociateUser()
    {
        // Arrange
        var apiKeyA = "user-a-key";
        var userA = await CreateUserAsync("User A", "user.a@email.com", apiKeyA);

        var apiKeyB = "user-b-key";
        var userB = await CreateUserAsync("User B", "user.b@email.com", apiKeyB);

        var sweepPayload = new
        {
            Name = "Sweep 1",
            Description = "Description 1",
            Phase = "Group Stage",
            GuessesDeadline = DateTime.UtcNow.AddDays(5),
            QualifiedCount = 2,
            IncludeThird = false
        };

        // Act
        // Step 1: Create sweepstakes as User A and get invite code
        var createResponse = await PostJsonAsync("/sweepstakes", sweepPayload, apiKeyA);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var sweepResult = await createResponse.Content.ReadFromJsonAsync<CreateSweepstakesEndpoint.Response>();
        Assert.NotNull(sweepResult);
        var inviteCode = sweepResult.InviteCode;

        // Step 2: User B joins using invite code
        var joinPayload = new { InviteCode = inviteCode };
        var joinResponse = await PostJsonAsync("/sweepstakes/join", joinPayload, apiKeyB);

        // Assert
        Assert.Equal(HttpStatusCode.OK, joinResponse.StatusCode);

        var joinResult = await joinResponse.Content.ReadFromJsonAsync<JoinSweepstakesEndpoint.Response>();
        Assert.NotNull(joinResult);
        Assert.Equal(sweepResult.Id, joinResult.SweepstakesId);

        // Check Db association
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var participant = await db.Participants
            .FirstOrDefaultAsync(p => p.SweepstakesId == sweepResult.Id && p.UserId == userB.Id);
        Assert.NotNull(participant);
    }
}
