using SinformWcApi.Entities;
using SinformWcApi.Services;
using System;
using Xunit;

namespace SinformWcApi.Tests;

public class ScoringStrategyTests
{
    private readonly IScoringStrategy _scoringStrategy = new DefaultScoringStrategy();

    [Fact]
    public void UT01_ExactMatchBothPositions_ShouldReturn20Points()
    {
        // Arrange
        var guess = new Guess
        {
            First = "Brazil",
            Second = "Switzerland"
        };
        var officialResult = new OfficialPhaseResult
        {
            FirstPlace = "Brazil",
            SecondPlace = "Switzerland"
        };

        // Act
        int score = _scoringStrategy.CalculateScore(guess, officialResult, includeThird: false);

        // Assert
        Assert.Equal(20, score);
    }

    [Fact]
    public void UT02_SwappedPositions_ShouldReturn0Points()
    {
        // Arrange
        var guess = new Guess
        {
            First = "Switzerland",
            Second = "Brazil"
        };
        var officialResult = new OfficialPhaseResult
        {
            FirstPlace = "Brazil",
            SecondPlace = "Switzerland"
        };

        // Act
        int score = _scoringStrategy.CalculateScore(guess, officialResult, includeThird: false);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void UT03_ExactFirstIncorrectSecond_ShouldReturn10Points()
    {
        // Arrange
        var guess = new Guess
        {
            First = "Brazil",
            Second = "Cameroon"
        };
        var officialResult = new OfficialPhaseResult
        {
            FirstPlace = "Brazil",
            SecondPlace = "Switzerland"
        };

        // Act
        int score = _scoringStrategy.CalculateScore(guess, officialResult, includeThird: false);

        // Assert
        Assert.Equal(10, score);
    }

    [Fact]
    public void UT04_CompleteMiss_ShouldReturn0Points()
    {
        // Arrange
        var guess = new Guess
        {
            First = "Cameroon",
            Second = "Serbia"
        };
        var officialResult = new OfficialPhaseResult
        {
            FirstPlace = "Brazil",
            SecondPlace = "Switzerland"
        };

        // Act
        int score = _scoringStrategy.CalculateScore(guess, officialResult, includeThird: false);

        // Assert
        Assert.Equal(0, score);
    }

    [Fact]
    public void UT05_IncludeThirdExactFirstOnly_ShouldReturn10Points()
    {
        // Arrange
        var guess = new Guess
        {
            First = "Brazil",
            Second = "Switzerland",
            Third = "Serbia"
        };
        var officialResult = new OfficialPhaseResult
        {
            FirstPlace = "Brazil",
            SecondPlace = "Serbia",
            ThirdPlace = "Switzerland"
        };

        // Act
        int score = _scoringStrategy.CalculateScore(guess, officialResult, includeThird: true);

        // Assert
        Assert.Equal(10, score);
    }
}
