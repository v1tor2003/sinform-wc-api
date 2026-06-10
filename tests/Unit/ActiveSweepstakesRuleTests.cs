using SinformWcApi.Exceptions;
using SinformWcApi.Services.Interfaces;
using SinformWcApi.Services.Impls;
using Xunit;

namespace SinformWcApi.Tests.Unit;

public class ActiveSweepstakesRuleTests
{
    private readonly IActiveSweepstakesRule _rule = new ActiveSweepstakesRule();

    [Fact]
    public void UT06_NoActiveSweepstakes_ShouldSucceed()
    {
        // Arrange
        int activeCount = 0;

        // Act & Assert (No exception should be thrown)
        _rule.Validate(activeCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void UT07_HasActiveSweepstakes_ShouldThrowDomainException(int activeCount)
    {
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => _rule.Validate(activeCount));
        Assert.Equal("User already has an active sweepstakes.", exception.Message);
    }
}
