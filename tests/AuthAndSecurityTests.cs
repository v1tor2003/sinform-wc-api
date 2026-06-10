using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace SinformWcApi.Tests;

public class AuthAndSecurityTests : IntegrationTestBase
{
    [Fact]
    public async Task IT01_MissingApiKey_ShouldReturn401Unauthorized()
    {
        // Arrange
        var requestPayload = new
        {
            Name = "World Cup 2026",
            Description = "Friendly Sweeps",
            Phase = "Group Stage",
            GuessesDeadline = DateTime.UtcNow.AddDays(5),
            QualifiedCount = 2,
            IncludeThird = false
        };

        // Act
        var response = await PostJsonAsync("/sweepstakes", requestPayload, apiKey: null);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IT02_InvalidApiKey_ShouldReturn401Unauthorized()
    {
        // Arrange
        var requestPayload = new
        {
            Name = "World Cup 2026",
            Description = "Friendly Sweeps",
            Phase = "Group Stage",
            GuessesDeadline = DateTime.UtcNow.AddDays(5),
            QualifiedCount = 2,
            IncludeThird = false
        };

        // Act
        var response = await PostJsonAsync("/sweepstakes", requestPayload, apiKey: "invalid-key-12345");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
